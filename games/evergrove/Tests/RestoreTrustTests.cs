using System;
using System.Globalization;
using System.IO;
using Evergrove.Core;

/// <summary>
/// Checklist 9, "Restore that the player can trust". SaveStatus.Line formats the line that says
/// the garden is kept on this phone and when it was last saved. The source checks confirm the
/// line is the first thing in the garden-file section, above every copy, save, restore and
/// replace control. Deterministic: fixed clock values, no device time.
/// </summary>
internal static class RestoreTrustTests
{
    private static int assertions;

    private static void Expect(bool ok, string message)
    {
        assertions++;
        if (!ok) throw new Exception("RestoreTrustTests failed: " + message);
    }

    private static readonly DateTime Now = new DateTime(2026, 11, 27, 16, 30, 0);
    private static readonly string[] AlarmWords = { "lost", "corrupt", "failed", "error", "warning" };

    public static int Run()
    {
        FormatsRelativeDays();
        FormatsOlderDaysAndUnknown();
        FormatsNonEnglishCulture();
        NormalLinesHaveNoAlarmWords();
        TrustLineIsFirstInGardenFileSection();
        return assertions;
    }

    private static void FormatsRelativeDays()
    {
        string today = SaveStatus.Line(new DateTime(2026, 11, 27, 14, 5, 0), Now, CultureInfo.InvariantCulture);
        Expect(today == "Your garden is saved on this phone. Last saved today at 14:05.",
            "same-day save reads today: " + today);

        string yesterday = SaveStatus.Line(new DateTime(2026, 11, 26, 9, 10, 0), Now, CultureInfo.InvariantCulture);
        Expect(yesterday == "Your garden is saved on this phone. Last saved yesterday at 09:10.",
            "previous-day save reads yesterday: " + yesterday);

        // Just before and just after midnight: the calendar day decides, not 24 hours.
        string beforeMidnight = SaveStatus.Line(new DateTime(2026, 11, 26, 23, 59, 0),
            new DateTime(2026, 11, 27, 0, 1, 0), CultureInfo.InvariantCulture);
        Expect(beforeMidnight.EndsWith("yesterday at 23:59."), "23:59 yesterday across midnight: " + beforeMidnight);
    }

    private static void FormatsOlderDaysAndUnknown()
    {
        string older = SaveStatus.Line(new DateTime(2026, 12, 3, 14, 5, 0),
            new DateTime(2026, 12, 10, 8, 0, 0), CultureInfo.InvariantCulture);
        Expect(older == "Your garden is saved on this phone. Last saved on 3 Dec at 14:05.",
            "older save names the day: " + older);

        string unknown = SaveStatus.Line(null, Now, CultureInfo.InvariantCulture);
        Expect(unknown == "Your garden is saved on this phone.", "unknown time gives the plain line: " + unknown);
        Expect(unknown == SaveStatus.Kept, "unknown time equals the Kept constant");

        string nullCulture = SaveStatus.Line(new DateTime(2026, 11, 27, 14, 5, 0), Now, null);
        Expect(nullCulture == SaveStatus.Line(new DateTime(2026, 11, 27, 14, 5, 0), Now, CultureInfo.InvariantCulture),
            "null culture uses invariant formatting");

        string again = SaveStatus.Line(new DateTime(2026, 11, 27, 14, 5, 0), Now, CultureInfo.InvariantCulture);
        Expect(again == "Your garden is saved on this phone. Last saved today at 14:05.",
            "same inputs give the same line");
    }

    private static void FormatsNonEnglishCulture()
    {
        // French uses a 24-hour clock and a day-then-month date. Only the time and the leading
        // day are asserted, so the test does not depend on the month abbreviation in ICU data.
        CultureInfo french = new CultureInfo("fr-FR");
        string today = SaveStatus.Line(new DateTime(2026, 11, 27, 14, 5, 0), Now, french);
        Expect(today == "Your garden is saved on this phone. Last saved today at 14:05.",
            "fr-FR same-day line keeps the 24-hour time: " + today);

        string older = SaveStatus.Line(new DateTime(2026, 12, 3, 14, 5, 0),
            new DateTime(2026, 12, 10, 8, 0, 0), french);
        Expect(older.StartsWith("Your garden is saved on this phone. Last saved on 3 ", StringComparison.Ordinal),
            "fr-FR older line names the day first: " + older);
        Expect(older.EndsWith(" at 14:05.", StringComparison.Ordinal), "fr-FR older line ends with the time: " + older);
    }

    private static void NormalLinesHaveNoAlarmWords()
    {
        string[] lines =
        {
            SaveStatus.Line(new DateTime(2026, 11, 27, 14, 5, 0), Now, CultureInfo.InvariantCulture),
            SaveStatus.Line(new DateTime(2026, 11, 26, 9, 10, 0), Now, CultureInfo.InvariantCulture),
            SaveStatus.Line(new DateTime(2026, 12, 3, 14, 5, 0), new DateTime(2026, 12, 10, 8, 0, 0), CultureInfo.InvariantCulture),
            SaveStatus.Line(null, Now, CultureInfo.InvariantCulture),
        };
        foreach (string line in lines)
        {
            string lower = line.ToLowerInvariant();
            foreach (string word in AlarmWords)
                Expect(!lower.Contains(word), "normal save line avoids the word '" + word + "': " + line);
        }
    }

    // The hook must come before every copy, save, restore and replace control, so the saved
    // status is read first and stays visible while a replace warning is open.
    private static void TrustLineIsFirstInGardenFileSection()
    {
        string app = ReadRepoFile(Path.Combine("games", "evergrove", "Assets", "Scripts", "Unity", "EvergroveApp.cs"));
        int start = app.IndexOf("private void BuildGardenFileSection()", StringComparison.Ordinal);
        Expect(start >= 0, "BuildGardenFileSection exists");
        int end = app.IndexOf("private void SetGardenNotice", start, StringComparison.Ordinal);
        Expect(end > start, "BuildGardenFileSection is followed by SetGardenNotice");
        string section = app.Substring(start, end - start);

        int hook = section.IndexOf("BuildSaveTrustLine(card.transform)", StringComparison.Ordinal);
        Expect(hook >= 0, "section calls BuildSaveTrustLine on the garden-file card");
        Expect(section.IndexOf("BuildSaveTrustLine(", hook + 1, StringComparison.Ordinal) < 0,
            "the trust line is added exactly once");

        string[] controls =
        {
            "Row(card.transform, \"Garden file actions\")",
            "\"Copy garden file\"",
            "BuildSaveGardenFileButton(files.transform)",
            "\"Restore garden file\"",
            "\"Garden replace warning\"",
            "\"Confirm garden restore\"",
        };
        foreach (string control in controls)
        {
            int at = section.IndexOf(control, StringComparison.Ordinal);
            Expect(at > hook, "the trust line comes before " + control);
        }

        string saving = ReadRepoFile(Path.Combine("games", "evergrove", "Assets", "Scripts", "Unity", "EvergroveApp.Saving.cs"));
        Expect(saving.Contains("private void BuildSaveTrustLine(Transform card)"), "Saving partial defines the trust line");
        Expect(saving.Contains("SaveStatus.Line(WorldStore.LastSavedLocal, DateTime.Now, CultureInfo.InvariantCulture)"),
            "the trust line uses the pure formatter with the stored save time");
        Expect(saving.Contains("Label(card, \"Save trust line\", text, 15, 0, Ink)"),
            "the trust line uses the calm Ink colour");

        string store = ReadRepoFile(Path.Combine("games", "evergrove", "Assets", "Scripts", "Unity", "WorldStore.cs"));
        Expect(store.Contains("public static DateTime? LastSavedLocal"), "WorldStore exposes the last save time");
    }

    private static string ReadRepoFile(string relative)
    {
        string[] starts = { Directory.GetCurrentDirectory(), AppContext.BaseDirectory };
        foreach (string start in starts)
        {
            string dir = Path.GetFullPath(start);
            for (int depth = 0; depth < 12 && dir != null; depth++)
            {
                string candidate = Path.Combine(dir, relative);
                if (File.Exists(candidate)) return File.ReadAllText(candidate);
                dir = Path.GetDirectoryName(dir);
            }
        }
        throw new Exception("RestoreTrustTests could not find " + relative);
    }
}
