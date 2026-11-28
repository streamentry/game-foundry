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
        return assertions;
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
}
