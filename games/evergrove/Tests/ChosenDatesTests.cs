using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Evergrove.Core;

/// <summary>
/// Checklist 87: chosen dates the neighbours mark. Round trip and fail-closed decoding; limits; the
/// mark on matching days (Feb 29 included); and the privacy test: no export, parcel or postcard carries
/// the dates, and only ChosenDates.cs and EvergroveApp.Dates.cs touch the key prefix.
/// </summary>
internal static class ChosenDatesTests
{
    private static int assertions;
    private const string INDIC = "\u0915\u093F"; // Devanagari ki: one text element
    private const string Stamp = "2026-11-27T12:00:00Z";

    private static void Expect(bool ok, string message)
    {
        assertions++;
        if (!ok) throw new Exception("ChosenDatesTests failed: " + message);
    }

    private static ChosenDate Make(int month, int day, ChosenDateKind kind, string label)
    {
        ChosenDate date;
        Expect(ChosenDates.TryMake(month, day, kind, label, out date), "valid date accepted: " + month + "/" + day);
        return date;
    }

    private static ChosenDate[] Of(params ChosenDate[] dates) { return dates; }

    private static string Repeat(string unit, int count)
    {
        var sb = new StringBuilder();
        for (int i = 0; i < count; i++) sb.Append(unit);
        return sb.ToString();
    }

    private static int Graphemes(string text)
    {
        return new System.Globalization.StringInfo(text).LengthInTextElements;
    }

    public static int Run()
    {
        RoundTrip();
        ForgedStringsFailClosed();
        Limits();
        LabelSanitizing();
        AddRemove();
        MarksOnMatchingDays();
        FebruaryTwentyNinth();
        MarksAreDeterministic();
        InvalidDaysGiveNoMark();
        PrivacyOutputsNeverCarryDates();
        PrivacySourceScan();
        return assertions;
    }

    private static void RoundTrip()
    {
        Expect(ChosenDates.Encode(Of()) == "", "an empty list encodes to nothing");
        ChosenDate[] none;
        Expect(ChosenDates.TryDecode("", out none) && none.Length == 0, "empty text decodes to no dates");
        ChosenDate[] list = Of(
            Make(5, 3, ChosenDateKind.Birthday, "Nan's"),
            Make(12, 25, ChosenDateKind.Anniversary, ""),
            Make(2, 29, ChosenDateKind.Other, "Leap day \U0001F33C"));
        string text = ChosenDates.Encode(list);
        ChosenDate[] back;
        Expect(ChosenDates.TryDecode(text, out back), "encoded list decodes");
        Expect(back.Length == 3, "three dates come back");
        Expect(back[0].Month == 5 && back[0].Day == 3 && back[0].Kind == ChosenDateKind.Birthday && back[0].Label == "Nan's", "first date round trips");
        Expect(back[1].Label == "" && back[1].Kind == ChosenDateKind.Anniversary, "a date without a label round trips");
        Expect(back[2].Month == 2 && back[2].Day == 29 && back[2].Label == "Leap day \U0001F33C", "Feb 29 and emoji label round trip");
        Expect(ChosenDates.Encode(back) == text, "re-encoding is stable");
        Expect(!text.Contains("2026") && !text.Contains("1990"), "no year is ever stored");
    }

    private static void ForgedStringsFailClosed()
    {
        string[] forged =
        {
            "v2;5.3.0.",                 // unknown version
            "5.3.0.",                    // no version marker
            "v1;13.1.0.",                // month out of range
            "v1;2.30.0.",                // no such day
            "v1;5.3.3.",                 // unknown kind
            "v1;5.3.0.zz",               // bad hex
            "v1;5.3.0.abc",              // odd hex length
            "v1;+5.3.0.",                // sign in number
            "v1;5. 3.0.",                // space in number
            "v1;5.3.0.3c3e",             // label that is markup
            "v1;5.3.0.0a",               // label with a newline
            "v1;5.3.0.ff",               // invalid UTF-8
            "v1;5.3.0.;5.3.1.",          // one day is fine, but the 5.3 pair ...
            "v1;5.3.0.;5.3.0.",          // ... a duplicate month/day is refused
            "v1;5.3.0",                  // missing field
            "v1;5.3.0..",                // extra field
            "v1;5.3.0.4d69726120",       // label "Mira " with a trailing space: not canonical
            " v1;5.3.0.",                // leading space
        };
        int accepted = 0;
        foreach (string text in forged)
        {
            ChosenDate[] result;
            bool ok = ChosenDates.TryDecode(text, out result);
            if (ok) accepted++;
            Expect(!ok || (result != null && ChosenDates.Encode(result) == text), "accepted only canonical text: " + text);
        }
        Expect(ChosenDates.TryDecode("v1;2.29.0.", out _), "a canonical Feb 29 entry is accepted");
        Expect(!ChosenDates.TryDecode("v1;5.3.0.;5.3.0.", out _), "a duplicate day is refused");
        Expect(accepted <= 3, "only canonical forms pass");
        Expect(!ChosenDates.TryDecode("v1;5.3.0.3c3e", out _), "markup label refused");
        Expect(!ChosenDates.TryDecode("v1;5.3.0.ff", out _), "invalid UTF-8 refused");
        ChosenDate[] outOfFourK;
        Expect(!ChosenDates.TryDecode(Repeat("x", 5000), out outOfFourK), "oversized text refused before parsing");
        Expect(!ChosenDates.TryDecode(null, out outOfFourK), "null refused");
        var seven = new StringBuilder("v1");
        for (int i = 1; i <= 7; i++) seven.Append(';').Append(i).Append(".1.0.");
        Expect(!ChosenDates.TryDecode(seven.ToString(), out outOfFourK), "seven stored dates refused");
    }

    private static void Limits()
    {
        ChosenDate[] list = new ChosenDate[0];
        for (int i = 1; i <= ChosenDates.MaxDates; i++)
        {
            ChosenDate next;
            Expect(ChosenDates.TryMake(i, 1, ChosenDateKind.Other, "", out next), "date " + i + " made");
            ChosenDate[] grown;
            Expect(ChosenDates.TryAdd(list, next, out grown), "date " + i + " added");
            list = grown;
        }
        Expect(list.Length == 6, "six dates are allowed");
        ChosenDate seventh;
        ChosenDates.TryMake(7, 1, ChosenDateKind.Other, "", out seventh);
        ChosenDate[] refused;
        Expect(!ChosenDates.TryAdd(list, seventh, out refused), "a seventh date is refused");

        string twentyFour = Repeat("a", 24);
        string twentyFive = Repeat("a", 25);
        string emoji24 = Repeat("\U0001F33C", 24);
        string emoji25 = Repeat("\U0001F33C", 25);
        string indic24 = Repeat(INDIC, 24);
        string indic25 = Repeat(INDIC, 25);
        Expect(Graphemes(emoji24) == 24 && Graphemes(indic24) == 24, "label lengths count text elements");
        ChosenDate ok;
        Expect(ChosenDates.TryMake(1, 1, ChosenDateKind.Birthday, twentyFour, out ok), "24 letters allowed");
        Expect(ChosenDates.TryMake(1, 1, ChosenDateKind.Birthday, emoji24, out ok), "24 emoji allowed");
        Expect(ChosenDates.TryMake(1, 1, ChosenDateKind.Birthday, indic24, out ok), "24 text elements of Indic allowed");
        Expect(!ChosenDates.TryMake(1, 1, ChosenDateKind.Birthday, twentyFive, out ok), "25 letters refused");
        Expect(!ChosenDates.TryMake(1, 1, ChosenDateKind.Birthday, emoji25, out ok), "25 emoji refused");
        Expect(!ChosenDates.TryMake(1, 1, ChosenDateKind.Birthday, indic25, out ok), "25 Indic text elements refused");
        Expect(!ChosenDates.TryMake(1, 1, ChosenDateKind.Birthday, Repeat("a", 200), out ok), "very long raw label refused");
        Expect(!ChosenDates.TryMake(0, 1, ChosenDateKind.Birthday, "", out ok), "month 0 refused");
        Expect(!ChosenDates.TryMake(4, 31, ChosenDateKind.Birthday, "", out ok), "31 April refused");
        Expect(!ChosenDates.TryMake(1, 1, (ChosenDateKind)9, "", out ok), "unknown kind refused");
    }

    private static void LabelSanitizing()
    {
        string clean;
        Expect(ChosenDates.CleanLabel("  Nan   and  Pop ", out clean) && clean == "Nan and Pop", "label trimmed and collapsed");
        Expect(ChosenDates.CleanLabel("   ", out clean) && clean == "", "spaces-only means no label");
        Expect(ChosenDates.CleanLabel(null, out clean) && clean == "", "null means no label");
        Expect(!ChosenDates.CleanLabel("a\nb", out clean), "line break refused");
        Expect(!ChosenDates.CleanLabel("a\tb", out clean), "tab refused");
        Expect(!ChosenDates.CleanLabel("<b>Nan</b>", out clean), "markup refused");
        Expect(!ChosenDates.CleanLabel("a\u2028b", out clean), "line separator refused");
        Expect(!ChosenDates.CleanLabel("a\uD800b", out clean), "lone surrogate refused");
    }

    private static void AddRemove()
    {
        ChosenDate a = Make(4, 1, ChosenDateKind.Birthday, "A");
        ChosenDate dup = Make(4, 1, ChosenDateKind.Anniversary, "Dup");
        ChosenDate[] one;
        Expect(ChosenDates.TryAdd(new ChosenDate[0], a, out one) && one.Length == 1, "a date is added");
        ChosenDate[] unused;
        Expect(!ChosenDates.TryAdd(one, dup, out unused), "a second date on the same day is refused");
        ChosenDate[] none;
        Expect(ChosenDates.TryRemoveAt(one, 0, out none) && none.Length == 0, "a date is removed");
        Expect(!ChosenDates.TryRemoveAt(one, 3, out unused), "removing a missing index is refused");
        Expect(one.Length == 1, "the original list is not changed");
    }

    private static void MarksOnMatchingDays()
    {
        ChosenDate[] dates = Of(Make(5, 3, ChosenDateKind.Birthday, "Nan"));
        string line = ChosenDates.MarkFor(20260503, dates);
        Expect(line != null && line.Contains("tied a ribbon on the gate for Nan."), "birthday mark names the label");
        Expect(line.StartsWith("Mira") || line.StartsWith("Orin") || line.StartsWith("Fen"), "a village neighbour says it");
        Expect(ChosenDates.MarkFor(20260504, dates) == null, "the day after is not marked");
        Expect(ChosenDates.MarkFor(20260403, dates) == null, "the same day in another month is not marked");
        Expect(ChosenDates.MarkFor(20270503, dates) != null, "the mark repeats every year");
        ChosenDate[] unlabelled = Of(Make(9, 9, ChosenDateKind.Anniversary, ""));
        Expect(ChosenDates.MarkFor(20260909, unlabelled).EndsWith("for an anniversary."), "unlabelled anniversary reads plainly");
        ChosenDate[] other = Of(Make(6, 1, ChosenDateKind.Other, "Gran's day."));
        Expect(ChosenDates.MarkFor(20260601, other).EndsWith("for Gran's day."), "a trailing full stop in a label is not doubled");
        Expect(ChosenDates.MarkFor(20260601, new ChosenDate[0]) == null, "no dates, no mark");
    }

    private static void FebruaryTwentyNinth()
    {
        ChosenDate[] leapDay = Of(Make(2, 29, ChosenDateKind.Birthday, "Leap"));
        Expect(ChosenDates.MarkFor(20280229, leapDay) != null, "Feb 29 is marked on Feb 29 in a leap year");
        Expect(ChosenDates.MarkFor(20270228, leapDay) != null, "Feb 29 is marked on Feb 28 in a non-leap year");
        Expect(ChosenDates.MarkFor(20270301, leapDay) == null, "Feb 29 is not marked on Mar 1 in a non-leap year");
        Expect(ChosenDates.MarkFor(20280228, leapDay) == null, "Feb 29 is not marked on Feb 28 in a leap year");
        Expect(ChosenDates.MarkFor(20270229, leapDay) == null, "Feb 29 does not exist in 2027, so no mark");
    }

    private static void MarksAreDeterministic()
    {
        ChosenDate[] dates = Of(Make(7, 14, ChosenDateKind.Other, "Picnic"));
        string first = ChosenDates.MarkFor(20260714, dates);
        for (int i = 0; i < 5; i++)
            Expect(ChosenDates.MarkFor(20260714, dates) == first, "the same day gives the same line");
    }

    private static void InvalidDaysGiveNoMark()
    {
        ChosenDate[] dates = Of(Make(2, 28, ChosenDateKind.Birthday, ""));
        Expect(ChosenDates.MarkFor(20261332, dates) == null, "month 13 gives no mark");
        Expect(ChosenDates.MarkFor(20260231, dates) == null, "Feb 31 gives no mark");
        Expect(ChosenDates.MarkFor(0, dates) == null, "zero gives no mark");
        Expect(ChosenDates.MarkFor(20260228, null) == null, "null list gives no mark");
    }

    private static WorldModel Garden()
    {
        WorldModel world = WorldModel.New(PlayMode.Living);
        world.SetGardenName("Quiet Corner");
        return world;
    }

    private static void PrivacyOutputsNeverCarryDates()
    {
        ChosenDate[] dates = Of(
            Make(5, 3, ChosenDateKind.Birthday, "Nan"),
            Make(12, 25, ChosenDateKind.Anniversary, "Wedding"));
        string encoded = ChosenDates.Encode(dates);
        string[] needles = { ChosenDates.Prefix, encoded, "Wedding", "Nan's" };

        WorldModel world = Garden();
        string export = GardenTransfer.Export(world, Stamp);

        string parcelText;
        ParcelContents contents;
        ParcelError error;
        Expect(FamilyParcel.TryCreate("Nan", "Thinking of you", ParcelGift.Seeds, 2, "abcdefgh12345678",
                                      out parcelText, out contents, out error),
               "a parcel can be built for the test");
        string wrapped = FamilyParcel.Wrap(parcelText, "Nan sent you seeds.");

        string postcard = GardenPostcard.Build(world, "Stream 1 / 12");

        string[] outputs = { export, wrapped, postcard };
        foreach (string output in outputs)
            foreach (string needle in needles)
                Expect(!output.Contains(needle), "no output carries the chosen dates: " + needle);

        // The dates travel only in their own PlayerPrefs key, so the world save is unchanged by them.
        Expect(GardenTransfer.Export(Garden(), Stamp) == export, "the garden file is the same with or without dates");
    }

    private static void PrivacySourceScan()
    {
        string root = FindRepoFile(Path.Combine("games", "evergrove", "Assets", "Scripts", "Core", "ChosenDates.cs"));
        Expect(root != null, "the evergrove source tree can be located");
        string scriptsRoot = Path.GetFullPath(Path.Combine(Path.GetDirectoryName(root), ".."));
        Expect(Directory.Exists(scriptsRoot), "scripts folder exists");

        var allowed = new HashSet<string> { "ChosenDates.cs", "EvergroveApp.Dates.cs" };
        string[] files = Directory.GetFiles(scriptsRoot, "*.cs", SearchOption.AllDirectories);
        int checkedFiles = 0;
        foreach (string file in files)
        {
            string text = File.ReadAllText(file);
            checkedFiles++;
            if (!allowed.Contains(Path.GetFileName(file)))
                Expect(!text.Contains(ChosenDates.Prefix), "only the dates files use the key prefix: " + Path.GetFileName(file));
        }
        Expect(checkedFiles > 50, "the scan read the source tree");

        string dates = File.ReadAllText(Path.Combine(scriptsRoot, "Unity", "EvergroveApp.Dates.cs"));
        Expect(dates.Contains("ChosenDates.ListKey"), "the Unity dates file keeps its list in the device-local key");
        string[] forbidden =
        {
            "NativeShare", "systemCopyBuffer", "GardenTransfer", "FamilyParcel", "WorldClient",
            "UnityWebRequest", "Notification", "GardenPostcard", "Share", "Send"
        };
        foreach (string word in forbidden)
            Expect(!dates.Contains(word), "EvergroveApp.Dates.cs does not reference " + word);
    }

    // Walks up from the working directory and the test binary's folder until the file is found.
    private static string FindRepoFile(string relative)
    {
        string[] starts = { Directory.GetCurrentDirectory(), AppContext.BaseDirectory };
        foreach (string start in starts)
        {
            string dir = Path.GetFullPath(start);
            for (int depth = 0; depth < 12 && dir != null; depth++)
            {
                string candidate = Path.Combine(dir, relative);
                if (File.Exists(candidate)) return candidate;
                dir = Path.GetDirectoryName(dir);
            }
        }
        return null;
    }
}
