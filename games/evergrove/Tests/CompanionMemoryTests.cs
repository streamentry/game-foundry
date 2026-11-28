using System;
using System.Collections.Generic;
using System.Text;
using Evergrove.Core;

/// <summary>
/// CP01: corrupt device-local companion memory never throws. Parse skips malformed rows and keeps
/// the well-formed ones; a seeded fuzz of 1,000 strings is deterministic and must never throw.
/// </summary>
internal static class CompanionMemoryTests
{
    private static int assertions;

    private static void Expect(bool ok, string message)
    {
        assertions++;
        if (!ok) throw new Exception("Companion memory test failed: " + message);
    }

    public static int Run()
    {
        assertions = 0;
        FuzzNeverThrows();
        KeepsValidRowsAmongGarbage();
        RoundTrip();
        ClockMovedBack();
        CapKeepsEverSaid();
        MemoryHelpers();
        StorageKeys();
        return assertions;
    }

    // CP06: read-only helpers used by CP07-CP09 (who spoke last, how many days a key was heard, a kind on a day).
    private static void MemoryHelpers()
    {
        // Empty memory: nothing has been said.
        CompanionMemory empty = CompanionMemory.Empty;
        Expect(empty.LastSpoke(0) == int.MinValue, "an empty memory has never heard anyone speak");
        Expect(empty.DaysHeardBy("key:x", 0, 10, 3) == 0, "an empty memory has no days heard");
        Expect(!empty.KindSaidOn(FactKind.Hour, 0, 5), "an empty memory has no kind said on any day");

        // Window edges with today = 10 and days = 3: days 10, 9 and 8 count; day 7 does not.
        const string key = "solved:Mosaic:2";
        CompanionMemory memory = CompanionMemory.Empty
            .With(new Heard(7, 0, FactKind.KindSolved, 2, key))
            .With(new Heard(8, 0, FactKind.KindSolved, 2, key))
            .With(new Heard(9, 0, FactKind.KindSolved, 2, key))
            .With(new Heard(10, 0, FactKind.KindSolved, 2, key));
        Expect(memory.DaysHeardBy(key, 0, 10, 3) == 3, "days=3 counts today, today-1 and today-2 only");
        Expect(memory.DaysHeardBy(key, 0, 10, 4) == 4, "days=4 also counts today-3");
        Expect(memory.DaysHeardBy(key, 0, 10, 0) == 0, "days=0 counts nothing");

        // Two remarks of one key on the same day count once; distinct days count separately.
        CompanionMemory twice = CompanionMemory.Empty
            .With(new Heard(9, 1, FactKind.Hour, 0, "hour:dawn"))
            .With(new Heard(9, 1, FactKind.Hour, 0, "hour:dawn"))
            .With(new Heard(10, 1, FactKind.Hour, 0, "hour:dawn"));
        Expect(twice.DaysHeardBy("hour:dawn", 1, 10, 3) == 2, "two remarks on one day count as one day");

        // Another speaker's remarks, or another key, are ignored.
        CompanionMemory mixed = CompanionMemory.Empty
            .With(new Heard(9, 1, FactKind.Hour, 0, "hour:dawn"))
            .With(new Heard(10, 1, FactKind.Hour, 0, "hour:dawn"))
            .With(new Heard(10, 2, FactKind.Hour, 0, "hour:dawn"))
            .With(new Heard(10, 0, FactKind.Hour, 0, "hour:other"));
        Expect(mixed.DaysHeardBy("hour:dawn", 0, 10, 3) == 0, "another speaker's remarks are not counted");
        Expect(mixed.DaysHeardBy("hour:dawn", 2, 10, 3) == 1, "only the named speaker's days are counted");
        Expect(mixed.DaysHeardBy("hour:other", 1, 10, 3) == 0, "a key said by another speaker is not counted");

        // The key is cleaned like the other lookups: a separator in the query matches the stored key.
        CompanionMemory piped = CompanionMemory.Empty.With(new Heard(10, 0, FactKind.Hour, 0, "letter|Orin"));
        Expect(piped.DaysHeardBy("letter|Orin", 0, 10, 3) == 1, "the queried key is cleaned before matching");

        // A future-dated remark is heard today (age 0), so it does not add a second day.
        CompanionMemory future = CompanionMemory.Empty
            .With(new Heard(12, 0, FactKind.Hour, 0, "hour:dawn"))
            .With(new Heard(10, 0, FactKind.Hour, 0, "hour:dawn"));
        Expect(future.DaysHeardBy("hour:dawn", 0, 10, 3) == 1, "a future-dated remark counts as today, once");

        // LastSpoke: the latest day per speaker, or MinValue for a speaker who never spoke.
        CompanionMemory spoke = CompanionMemory.Empty
            .With(new Heard(3, 0, FactKind.Hour, 0, "hour:a"))
            .With(new Heard(5, 0, FactKind.Hour, 0, "hour:c"))
            .With(new Heard(9, 0, FactKind.Hour, 0, "hour:b"))
            .With(new Heard(20, 1, FactKind.Hour, 0, "hour:d"));
        Expect(spoke.LastSpoke(0) == 9, "LastSpoke is the latest day this speaker said anything");
        Expect(spoke.LastSpoke(1) == 20, "LastSpoke is per speaker");
        Expect(spoke.LastSpoke(2) == int.MinValue, "a speaker who never spoke gives int.MinValue");

        // KindSaidOn: the kind, by this speaker, on exactly this day.
        CompanionMemory kinds = CompanionMemory.Empty
            .With(new Heard(4, 0, FactKind.Hour, 0, "hour:dawn"))
            .With(new Heard(5, 0, FactKind.KindSolved, 2, key))
            .With(new Heard(6, 1, FactKind.Hour, 0, "hour:dusk"));
        Expect(kinds.KindSaidOn(FactKind.Hour, 0, 4), "a kind said on that day is found");
        Expect(!kinds.KindSaidOn(FactKind.Hour, 0, 5), "a kind said on another day is not found");
        Expect(kinds.KindSaidOn(FactKind.KindSolved, 0, 5), "a second kind on its day is found");
        Expect(!kinds.KindSaidOn(FactKind.KindSolved, 1, 5), "another speaker's kind on that day is not found");
        Expect(kinds.KindSaidOn(FactKind.Hour, 1, 6) && !kinds.KindSaidOn(FactKind.Hour, 0, 6), "KindSaidOn is per speaker");
    }

    // CP03: over MaxEntries, older repeats of a key go first, so a once-fact's only record survives the cap.
    private static void CapKeepsEverSaid()
    {
        // (a) One once-fact, then 400 repeats of another key: the once-fact is still remembered.
        CompanionMemory repeats = CompanionMemory.Empty.With(new Heard(1, 0, FactKind.Hour, 0, "once:A"));
        for (int i = 0; i < 400; i++)
            repeats = repeats.With(new Heard(10, 0, FactKind.Hour, 0, "repeat"));
        Expect(repeats.Count <= CompanionMemory.MaxEntries, "the cap holds after 400 repeats");
        Expect(repeats.Count == CompanionMemory.MaxEntries, "the cap is filled, not overshot");
        Expect(repeats.EverSaid("once:A"), "a once-fact survives the cap when only repeats are dropped");
        Expect(repeats.EverSaid("repeat"), "the newest repeat is kept");

        // (b) More than MaxEntries distinct keys: the oldest go, the newest are kept.
        CompanionMemory distinct = CompanionMemory.Empty;
        for (int i = 0; i < 350; i++)
            distinct = distinct.With(new Heard(10, 0, FactKind.Hour, 0, "distinct:" + i));
        Expect(distinct.Count == CompanionMemory.MaxEntries, "distinct keys are capped at MaxEntries");
        Expect(distinct.EverSaid("distinct:349"), "the newest distinct key is kept");
        Expect(distinct.EverSaid("distinct:50"), "the 300th-newest distinct key is kept");
        Expect(!distinct.EverSaid("distinct:0"), "the oldest distinct key is dropped when all keys are distinct");

        // (c) A capped memory still round-trips through Serialize and Parse, in the same order.
        CompanionMemory back = CompanionMemory.Parse(repeats.Serialize());
        Expect(back.Count == repeats.Count, "a capped memory round-trips to the same count");
        List<Heard> before = new List<Heard>(repeats.Entries);
        List<Heard> after = new List<Heard>(back.Entries);
        for (int i = 0; i < before.Count; i++)
            Expect(after[i].Key == before[i].Key && after[i].Day == before[i].Day, "a capped memory round-trips in order");
    }

    // CP02: a device clock moved back must not forget what was heard "in the future", nor let a
    // once-fact be said again. Future-dated remarks count as heard today (age 0).
    private static void ClockMovedBack()
    {
        const string key = "solved:Mosaic:2";
        CompanionMemory future = CompanionMemory.Empty.With(new Heard(100, 0, FactKind.KindSolved, 2, key));

        // (a) An entry dated after the new remark is kept, and re-dated to the remark's day.
        CompanionMemory both = future.With(new Heard(90, 1, FactKind.Hour, 0, "hour:dawn"));
        Expect(both.Count == 2, "a future-dated entry is kept when the clock moved back");
        Expect(both.EverSaid(key), "the future-dated key is still remembered");
        foreach (Heard h in both.Entries)
            if (h.Key == key) Expect(h.Day == 90, "the future-dated entry is re-dated to the day it is heard now");

        // (b) Within a window, a remark dated in the future counts as heard today.
        Expect(future.SaidWithin(key, 90, 3), "a remark dated 100 is heard today when today is 90");
        Expect(future.VariantWithin(FactKind.KindSolved, 0, 2, 90, 14), "a future-dated wording counts as heard today");
        Expect(!future.SaidWithin("other:key", 90, 3), "an unrelated key is still not heard");

        // (c) A once-fact heard on day 100 is not eligible again on day 90.
        var once = new Fact(FactKind.PairingFound, 0, "pairing-found:Grove", new Dictionary<string, string>(), 20, false, false, 0, true);
        CompanionMemory heardOnce = CompanionMemory.Empty.With(new Heard(100, 0, FactKind.PairingFound, 0, once.Key));
        Expect(!CompanionRanker.Eligible(once, heardOnce, 90), "a once-fact heard on day 100 is not said again on day 90");

        // KeepDays pruning still drops genuinely old entries.
        CompanionMemory old = CompanionMemory.Empty
            .With(new Heard(0, 0, FactKind.Hour, 0, "hour:old"))
            .With(new Heard(CompanionMemory.KeepDays, 0, FactKind.Hour, 0, "hour:new"));
        Expect(!old.EverSaid("hour:old") && old.EverSaid("hour:new"), "entries older than KeepDays are still pruned");
    }

    private static readonly string[] Pieces = {
        "0", "1", "2", "3", "-1", "-7", "9000", "2147483647", "2147483648", "-2147483649",
        "99999999999999999999", "Hour", "KindSolved", "NoSuchKind", "hour", "5", "-0", " 1 ",
        "Speaker", "", "|", "||", "\n", "\r", "\r\n", "key:Mira", "solved:Mosaic:2", "é中", "\0",
    };

    private static string RandomRow(Random rng)
    {
        var row = new StringBuilder();
        int fields = rng.Next(0, 8);
        for (int f = 0; f < fields; f++)
        {
            if (f > 0) row.Append('|');
            row.Append(Pieces[rng.Next(Pieces.Length)]);
        }
        return row.ToString();
    }

    private static string RandomChars(Random rng)
    {
        const string pool = "0123456789-|| \n\r:abcXYZé";
        var text = new StringBuilder();
        int length = rng.Next(0, 120);
        for (int i = 0; i < length; i++) text.Append(pool[rng.Next(pool.Length)]);
        return text.ToString();
    }

    private static void FuzzNeverThrows()
    {
        var rng = new Random(20261128);
        for (int i = 0; i < 1000; i++)
        {
            string text;
            switch (i % 3)
            {
                case 0: text = RandomChars(rng); break;
                case 1:
                    // Partial valid rows: real day|speaker|kind|variant|key shapes with random pieces.
                    var rows = new StringBuilder();
                    int lines = rng.Next(1, 6);
                    for (int l = 0; l < lines; l++) rows.Append(RandomRow(rng)).Append('\n');
                    text = rows.ToString();
                    break;
                default:
                    text = RandomRow(rng) + "\n" + RandomChars(rng);
                    break;
            }

            CompanionMemory parsed = CompanionMemory.Parse(text);
            Expect(parsed != null, "Parse returns a memory, never null");
            Expect(parsed.Count <= CompanionMemory.MaxEntries, "Parse respects the cap");
            foreach (Heard h in parsed.Entries)
            {
                Expect(h.Speaker >= 0 && h.Speaker <= 2, "only known voices survive");
                Expect(Enum.IsDefined(typeof(FactKind), h.Kind), "only known kinds survive");
                Expect(h.Key.Length > 0, "an empty key never survives");
            }
        }
    }

    private static void KeepsValidRowsAmongGarbage()
    {
        CompanionMemory good = CompanionMemory.Empty
            .With(new Heard(120, 1, FactKind.KindSolved, 2, "solved:Mosaic:2"));
        string valid = good.Serialize();
        Expect(valid == "120|1|KindSolved|2|solved:Mosaic:2", "one row in the documented format");

        string garbage = string.Join("\n", new[] {
            "not a row",
            "1|2|3|4",                  // too few fields
            "x|1|Hour|0|key",           // day not a number
            "1|3|Hour|0|key",           // speaker out of range
            "1|-1|Hour|0|key",          // speaker negative
            "1|1|NoSuchKind|0|key",     // unknown kind
            "1|1|99|0|key",             // numeric kind that is not defined
            "1|1|Hour|x|key",           // variant not a number
            "1|1|Hour|0|",              // empty key
            "99999999999999999999|1|Hour|0|key",
            "|||||",
            "",
            "\r",
        });
        CompanionMemory mixed = CompanionMemory.Parse(garbage + "\n" + valid + "\n" + garbage);
        Expect(mixed.Count == 1, "exactly the one valid row survives");
        Heard kept = new List<Heard>(mixed.Entries)[0];
        Expect(kept.Day == 120 && kept.Speaker == 1 && kept.Kind == FactKind.KindSolved
            && kept.Variant == 2 && kept.Key == "solved:Mosaic:2", "the surviving row is the valid one, unchanged");
    }

    private static void RoundTrip()
    {
        CompanionMemory memory = CompanionMemory.Empty
            .With(new Heard(10, 0, FactKind.Hour, 0, "hour:dawn"))
            .With(new Heard(11, 2, FactKind.LetterWaiting, -1, "letter:Orin:3"))
            .With(new Heard(12, 1, FactKind.WeatherToday, 4, "weather:rain"));
        CompanionMemory back = CompanionMemory.Parse(memory.Serialize());
        Expect(back.Count == memory.Count, "round trip keeps every row");
        List<Heard> before = new List<Heard>(memory.Entries);
        List<Heard> after = new List<Heard>(back.Entries);
        for (int i = 0; i < before.Count; i++)
        {
            Expect(after[i].Day == before[i].Day && after[i].Speaker == before[i].Speaker
                && after[i].Kind == before[i].Kind && after[i].Variant == before[i].Variant
                && after[i].Key == before[i].Key, "round trip keeps each field");
        }
        Expect(back.Serialize() == memory.Serialize(), "serialize is stable across a round trip");
        Expect(CompanionMemory.Parse(null).Count == 0 && CompanionMemory.Parse("").Count == 0, "empty text is an empty memory");
    }

    // CP36: device memory is per language, since a wording number belongs to one language's bank.
    // English (and no locale) keeps the pre-CP36 key so existing players keep what they heard.
    private static void StorageKeys()
    {
        Expect(CompanionMemory.StorageKey(3, "en") == "evergrove:companion-heard:3", "English keeps the old key exactly");
        Expect(CompanionMemory.StorageKey(3, null) == "evergrove:companion-heard:3", "no locale keeps the old key");
        Expect(CompanionMemory.StorageKey(3, "") == "evergrove:companion-heard:3", "an empty locale keeps the old key");
        Expect(CompanionMemory.StorageKey(3, "EN") == "evergrove:companion-heard:3", "English is matched without case");
        Expect(CompanionMemory.StorageKey(3, "fr") == "evergrove:companion-heard:fr:3", "French has its own key");
        Expect(CompanionMemory.StorageKey(3, "pt-BR") == "evergrove:companion-heard:pt-br:3", "a regional locale is lowercased");
        Expect(CompanionMemory.StorageKey(3, "Fr_BE") == "evergrove:companion-heard:fr-be:3", "an underscore becomes a dash");
        Expect(CompanionMemory.StorageKey(3, "fr:x/../y z") == "evergrove:companion-heard:fr-x----y-z:3", "separators and spaces are replaced");
        Expect(CompanionMemory.StorageKey(3, "fr") != CompanionMemory.StorageKey(4, "fr"), "a garden keeps its own memory per language");
        Expect(CompanionMemory.StorageKey(3, "fr") != CompanionMemory.StorageKey(3, "en"), "French and English memories are distinct");
        Expect(CompanionMemory.StorageKey(3, "fr") != CompanionMemory.StorageKey(3, "fr-ca"), "distinct locales give distinct keys");
        Expect(CompanionMemory.StorageKey(3, "de") != CompanionMemory.StorageKey(3, "ja"), "two other languages do not share a key");
    }
}
