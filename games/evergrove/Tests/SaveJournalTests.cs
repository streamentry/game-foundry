using System;
using System.IO;
using Evergrove.Core;

/// <summary>
/// Pure C# storage fault simulations. These do not certify Unity JsonUtility,
/// flash durability, platform file locks, OS force-close or physical devices.
/// </summary>
internal static class SaveJournalTests
{
    private static int assertions;

    private static void Expect(bool ok, string message)
    {
        assertions++;
        if (!ok) throw new Exception("SaveJournal test failed: " + message);
    }

    private static bool Valid(string text)
    {
        return text != null && text.StartsWith("valid:", StringComparison.Ordinal) &&
               text.Length >= 7 && text.Length <= 128;
    }

    private static void InTemp(Action<string> action)
    {
        string root = Path.Combine(Path.GetTempPath(), "evergrove-journal-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);
        try { action(Path.Combine(root, "save.json")); }
        finally { Directory.Delete(root, true); }
    }

    private static void ExpectLoad(string path, string value, string expectedOrigin)
    {
        string actual, origin;
        Expect(SaveJournal.TryLoad(path, Valid, out actual, out origin),
               "at least one valid slot should load");
        Expect(actual == value && origin == expectedOrigin,
               "correct value and recovery origin: " + expectedOrigin);
    }

    public static int Run()
    {
        assertions = 0;
        InTemp(path =>
        {
            string read, origin;
            Expect(!SaveJournal.TryLoad(path, Valid, out read, out origin),
                   "fresh install has no valid saves");
            Expect(!SaveJournal.HasAnySlot(path), "fresh install has no files");
            Expect(SaveJournal.Commit(path, "valid:first", Valid), "first commit succeeds");
            ExpectLoad(path, "valid:first", "primary");
            Expect(!SaveJournal.Commit(path, "not a valid state", Valid),
                   "invalid state is rejected before writing");
            ExpectLoad(path, "valid:first", "primary");

            Expect(SaveJournal.Commit(path, "valid:second", Valid), "second commit succeeds");
            ExpectLoad(path, "valid:second", "primary");
            Expect(File.ReadAllText(path + ".bak") == "valid:first",
                   "the previous good primary is the backup");

            File.WriteAllText(path, "{broken json");
            ExpectLoad(path, "valid:first", "backup");
            Expect(SaveJournal.Commit(path, "valid:recovered", Valid),
                   "commit can repair corrupt primary while keeping old backup");
            ExpectLoad(path, "valid:recovered", "primary");
            Expect(File.ReadAllText(path + ".bak") == "valid:first",
                   "corrupted primary never rotates over a valid backup");

            File.WriteAllText(path, "broken");
            File.WriteAllText(path + ".bak", "broken too");
            File.WriteAllText(path + ".tmp", "valid:staged");
            ExpectLoad(path, "valid:staged", "temporary");
            Expect(SaveJournal.Commit(path, "valid:restored", Valid),
                   "a staged recovery can be re-committed");
            ExpectLoad(path, "valid:restored", "primary");
        });

        // 100 deterministic interruptions across two write boundaries, *not*
        // 100 physical device force-closes. Vary truncation length and save
        // position; check that a valid previous slot always survives.
        for (int i = 0; i < 100; i++)
        {
            int caseNumber = i;
            InTemp(path =>
            {
                string previous = "valid:before-" + caseNumber;
                string next = "valid:after-" + caseNumber;
                Expect(SaveJournal.Commit(path, previous, Valid), "baseline save");

                // Interrupted staging write: old primary stays valid.
                File.WriteAllText(path + ".tmp", next.Substring(0, caseNumber % 7));
                ExpectLoad(path, previous, "primary");

                Expect(SaveJournal.Commit(path, next, Valid), "new staged save");
                // Interrupted overwrite of primary after backup is copied:
                // backup preserves the prior valid state.
                File.WriteAllText(path, next.Substring(0, caseNumber % 7));
                ExpectLoad(path, previous, "backup");

                // The next save must not accidentally rotate the damaged
                // primary over the only good backup.
                string repaired = "valid:repair-" + caseNumber;
                Expect(SaveJournal.Commit(path, repaired, Valid), "re-save after corruption");
                ExpectLoad(path, repaired, "primary");
                Expect(File.ReadAllText(path + ".bak") == previous,
                       "last known good backup survived recovery");
            });
        }

        Console.WriteLine("Evergrove save journal tests: " + assertions +
                          " deterministic assertions; not physical-device evidence.");
        return assertions;
    }
}
