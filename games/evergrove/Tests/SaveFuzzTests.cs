using System;
using System.Collections.Generic;
using System.IO;
using Evergrove.Core;

/// <summary>
/// Deterministic "app killed during save" simulation for SaveJournal.Commit.
/// Crash states are reconstructed from PlanCommit, which mirrors the file-system
/// steps of SaveJournal.Commit (SaveJournal.cs:65-133). Each round checks that the
/// mirror predicts the real Commit's final disk state, so drift fails the test.
/// Payloads are complete garden files (GardenTransfer checksum import) or synthetic
/// length-prefixed envelopes that give random payload sizes. Pure C#; no Unity types.
/// </summary>
internal static class SaveFuzzTests
{
    private const int Seed = 96;
    private const int Rounds = 150; // every step boundary and torn variant per round; 150 keeps the suite fast in CI
    private const string Stamp = "2026-11-27T10:00:00Z";
    private const string Alphabet = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
    private const string GarbageChars = "{}[]\":,|env 0123456789abcxyz\n";

    private static int assertions;
    private static int crashStates;
    private static int readerCases;

    private enum Slot { Primary = 0, Backup = 1, Staged = 2 }
    private enum Op { Write, Copy, Move, Delete }

    private static readonly string[] SlotNames = { "primary", "backup", "temporary" };

    private sealed class Slots
    {
        private readonly string[] files = new string[3];

        public string this[Slot slot]
        {
            get { return files[(int)slot]; }
            set { files[(int)slot] = value; }
        }

        public Slots Clone()
        {
            Slots copy = new Slots();
            Array.Copy(files, copy.files, files.Length);
            return copy;
        }

        public bool AnyPresent()
        {
            return files[0] != null || files[1] != null || files[2] != null;
        }
    }

    private sealed class Step
    {
        public string Label;
        public Op Op;
        public Slot Target;
        public string Content;   // full text written by this step; null for Delete
        public Slot? Clears;     // slot emptied by a rename (Move)
    }

    private static void Expect(bool ok, string message)
    {
        assertions++;
        if (!ok) throw new Exception("SaveFuzzTests failed: " + message);
    }

    // ---- Validators --------------------------------------------------------

    /// <summary>Complete garden files only: GardenTransfer checks format, length and checksum.</summary>
    private static bool ValidGarden(string text)
    {
        if (text == null) return false;
        bool cached;
        if (gardenVerdicts.TryGetValue(text, out cached)) return cached;
        WorldModel world;
        GardenImportError error;
        bool verdict = GardenTransfer.TryImport(text, out world, out error);
        // Pure function of the text; bounded so truncated prefixes cannot grow it without limit.
        if (gardenVerdicts.Count >= 256) gardenVerdicts.Clear();
        gardenVerdicts[text] = verdict;
        return verdict;
    }

    private static readonly Dictionary<string, bool> gardenVerdicts = new Dictionary<string, bool>(StringComparer.Ordinal);

    /// <summary>Synthetic "env|LEN|BODY|end" envelope. A truncated prefix never validates.</summary>
    private static bool ValidEnvelope(string text)
    {
        if (text == null || !text.StartsWith("env|", StringComparison.Ordinal)) return false;
        int headerEnd = text.IndexOf('|', 4);
        int length;
        if (headerEnd < 5 || !int.TryParse(text.Substring(4, headerEnd - 4), out length) || length < 0)
            return false;
        int bodyStart = headerEnd + 1;
        if (text.Length != bodyStart + length + 4) return false;
        return string.CompareOrdinal(text, bodyStart + length, "|end", 0, 4) == 0;
    }

    private static string Envelope(string body)
    {
        return "env|" + body.Length + "|" + body + "|end";
    }

    private static string RandomBody(Random rng)
    {
        int length = rng.Next(0, 2500);
        char[] chars = new char[length];
        for (int i = 0; i < length; i++) chars[i] = Alphabet[rng.Next(Alphabet.Length)];
        return new string(chars);
    }

    private static string GardenText(int garden)
    {
        return GardenTransfer.Export(WorldModel.New(PlayMode.Living, garden), Stamp);
    }

    /// <summary>Three distinct committed states: old (previous save), new (payload), third (recovery).</summary>
    private static void MakeStates(Random rng, bool envelope, out string oldText, out string newText, out string thirdText)
    {
        if (envelope)
        {
            oldText = Envelope("old" + RandomBody(rng));
            newText = Envelope("new" + RandomBody(rng));
            thirdText = Envelope("third" + RandomBody(rng));
        }
        else
        {
            int a = rng.Next(4);
            int b;
            do { b = rng.Next(4); } while (b == a);
            int c;
            do { c = rng.Next(4); } while (c == a || c == b);
            oldText = GardenText(a);
            newText = GardenText(b);
            thirdText = GardenText(c);
        }
        Expect(oldText != newText && newText != thirdText && oldText != thirdText,
               "old, new and third states must be distinct");
    }

    private static string Garbage(Random rng)
    {
        int length = rng.Next(0, 80);
        char[] chars = new char[length];
        for (int i = 0; i < length; i++) chars[i] = GarbageChars[rng.Next(GarbageChars.Length)];
        return new string(chars);
    }

    /// <summary>One on-disk slot: absent, empty, garbage, a complete save, or a truncated save.</summary>
    private static string RandomSlot(Random rng, string[] fulls, Func<string, bool> accepts)
    {
        int roll = rng.Next(100);
        if (roll < 20) return null;
        if (roll < 30) return "";
        if (roll < 45) return Garbage(rng);
        string full = fulls[rng.Next(fulls.Length)];
        if (roll < 80) return full;
        string cut = full.Substring(0, rng.Next(0, full.Length));
        Expect(!accepts(cut), "a truncated save must never validate");
        return cut;
    }

    // ---- Mirror of SaveJournal.Commit ---------------------------------------

    private static bool Valid(string text, Func<string, bool> accepts)
    {
        return text != null && accepts(text);
    }

    private static void Apply(Step step, Slots state)
    {
        state[step.Target] = step.Content;
        if (step.Clears.HasValue) state[step.Clears.Value] = null;
    }

    private static void Add(List<Step> steps, Slots cur, string label, Op op, Slot target, string content, Slot? clears)
    {
        Step step = new Step { Label = label, Op = op, Target = target, Content = content, Clears = clears };
        Apply(step, cur);
        steps.Add(step);
    }

    /// <summary>
    /// File-system steps SaveJournal.Commit performs for a payload that passes accepts.
    /// Returns null when Commit returns false before writing anything.
    /// </summary>
    private static List<Step> PlanCommit(Slots start, string payload, Func<string, bool> accepts)
    {
        bool primaryValid = Valid(start[Slot.Primary], accepts);
        bool backupValid = Valid(start[Slot.Backup], accepts);
        bool stagedValid = Valid(start[Slot.Staged], accepts);
        List<Step> steps = new List<Step>();
        Slots cur = start.Clone();

        if (!primaryValid && !backupValid)
        {
            // SaveJournal.cs:89 fails closed when files exist but none is valid.
            if (!stagedValid && start.AnyPresent()) return null;
            // SaveJournal.cs:92 keeps the sole valid staged snapshot in backup first.
            if (stagedValid)
                Add(steps, cur, "SaveJournal.cs:92 copy .tmp -> .bak", Op.Copy, Slot.Backup, cur[Slot.Staged], null);
        }
        // SaveJournal.cs:100 write the staging file.
        Add(steps, cur, "SaveJournal.cs:100 write .tmp", Op.Write, Slot.Staged, payload, null);
        // SaveJournal.cs:109-111 rotate a validated primary into backup.
        if (primaryValid)
            Add(steps, cur, "SaveJournal.cs:111 copy primary -> .bak", Op.Copy, Slot.Backup, cur[Slot.Primary], null);
        // SaveJournal.cs:118-121 publish: copy over an existing primary, or rename when absent.
        if (start[Slot.Primary] != null)
            Add(steps, cur, "SaveJournal.cs:119 copy .tmp -> primary", Op.Copy, Slot.Primary, payload, null);
        else
            Add(steps, cur, "SaveJournal.cs:121 move .tmp -> primary", Op.Move, Slot.Primary, payload, Slot.Staged);
        // SaveJournal.cs:129 optional stale-stage cleanup after success.
        Add(steps, cur, "SaveJournal.cs:129 delete .tmp", Op.Delete, Slot.Staged, null, null);
        return steps;
    }

    // ---- Disk helpers ---------------------------------------------------------

    private static string PathOf(string primary, Slot slot)
    {
        switch (slot)
        {
            case Slot.Primary: return primary;
            case Slot.Backup: return primary + ".bak";
            default: return primary + ".tmp";
        }
    }

    private static void WriteSlots(string primary, Slots state)
    {
        foreach (Slot slot in new[] { Slot.Primary, Slot.Backup, Slot.Staged })
        {
            string path = PathOf(primary, slot);
            string text = state[slot];
            if (text == null)
            {
                if (File.Exists(path)) File.Delete(path);
            }
            else
            {
                File.WriteAllText(path, text);
            }
        }
    }

    private static Slots ReadSlots(string primary)
    {
        Slots state = new Slots();
        foreach (Slot slot in new[] { Slot.Primary, Slot.Backup, Slot.Staged })
        {
            string path = PathOf(primary, slot);
            state[slot] = File.Exists(path) ? File.ReadAllText(path) : null;
        }
        return state;
    }

    private static bool SameSlots(Slots a, Slots b)
    {
        foreach (Slot slot in new[] { Slot.Primary, Slot.Backup, Slot.Staged })
        {
            if (!string.Equals(a[slot], b[slot], StringComparison.Ordinal)) return false;
        }
        return true;
    }

    // ---- Crash checks -----------------------------------------------------------

    /// <summary>
    /// Simulates a process that died with `crashed` on disk. Checks the load invariant,
    /// then checks that recovery works (or fails closed when only corrupt evidence is left).
    /// </summary>
    private static void CheckCrash(string primary, Slots crashed, Func<string, bool> accepts,
                                   bool hadOld, string old, string payload, string third, string where)
    {
        crashStates++;
        WriteSlots(primary, crashed);
        string got, origin;
        bool loaded = SaveJournal.TryLoad(primary, accepts, out got, out origin);
        if (hadOld)
            Expect(loaded && (got == old || got == payload),
                   where + ": crash lost the complete old save or returned a partial one");
        else if (loaded)
            Expect(got == payload, where + ": crash returned a state that was never a complete save");
        if (loaded)
        {
            int index = Array.IndexOf(SlotNames, origin);
            Expect(index >= 0 && crashed[(Slot)index] == got, where + ": origin does not match the served slot");
        }

        // Recovery: after the crash, a successful commit of a third state must load it back.
        WriteSlots(primary, crashed);
        bool recovered = SaveJournal.Commit(primary, third, accepts);
        if (loaded || !crashed.AnyPresent())
        {
            Expect(recovered, where + ": recovery commit after crash must succeed");
            string again, againOrigin;
            Expect(SaveJournal.TryLoad(primary, accepts, out again, out againOrigin) &&
                   again == third && againOrigin == "primary",
                   where + ": recovery did not load the third state from primary");
        }
        else
        {
            // Only invalid evidence remains: fail closed and leave every file untouched.
            Expect(!recovered && SameSlots(ReadSlots(primary), crashed),
                   where + ": unreadable slots must fail closed with files untouched");
        }
    }

    private static void RunRound(string root, Random rng, int round)
    {
        string primary = Path.Combine(root, "save.json");
        bool envelope = rng.Next(2) == 0;
        Func<string, bool> accepts = envelope
            ? new Func<string, bool>(ValidEnvelope)
            : new Func<string, bool>(ValidGarden);
        string oldText, newText, thirdText;
        MakeStates(rng, envelope, out oldText, out newText, out thirdText);
        string[] fulls = { oldText, newText, thirdText };

        Slots start = new Slots();
        start[Slot.Primary] = RandomSlot(rng, fulls, accepts);
        start[Slot.Backup] = RandomSlot(rng, fulls, accepts);
        start[Slot.Staged] = RandomSlot(rng, fulls, accepts);

        // What a load sees before the commit starts is the "old" state.
        WriteSlots(primary, start);
        string old, oldOrigin;
        bool hadOld = SaveJournal.TryLoad(primary, accepts, out old, out oldOrigin);

        // Model check: the mirror must predict the real Commit on the same disk state.
        List<Step> steps = PlanCommit(start, newText, accepts);
        WriteSlots(primary, start);
        bool committed = SaveJournal.Commit(primary, newText, accepts);
        Slots after = ReadSlots(primary);
        string where0 = "round " + round;
        if (steps == null)
        {
            Expect(!committed && SameSlots(after, start), where0 + ": rejected commit must not touch disk");
        }
        else
        {
            Slots final = start.Clone();
            foreach (Step step in steps) Apply(step, final);
            Expect(committed && SameSlots(after, final), where0 + ": step mirror must predict the real final disk state");
        }

        // Crash after every step boundary, plus torn variants of each writing step.
        if (steps == null)
        {
            CheckCrash(primary, start.Clone(), accepts, hadOld, old, newText, thirdText, where0 + " no writes");
            return;
        }
        Slots prefix = start.Clone();
        for (int k = 0; k <= steps.Count; k++)
        {
            string at = where0 + " boundary " + k;
            CheckCrash(primary, prefix.Clone(), accepts, hadOld, old, newText, thirdText, at);
            if (k == steps.Count) break;

            Step step = steps[k];
            if (step.Op == Op.Write || step.Op == Op.Copy)
            {
                int length = step.Content.Length;
                int[] cuts = { 0, Math.Max(0, length - 1), rng.Next(length) };
                foreach (int cut in cuts)
                {
                    Slots torn = prefix.Clone();
                    torn[step.Target] = step.Content.Substring(0, cut);
                    CheckCrash(primary, torn, accepts, hadOld, old, newText, thirdText,
                               at + " torn at " + cut + "/" + length + " in " + step.Label);
                }
            }
            Apply(step, prefix);
        }
    }

    /// <summary>Reader fuzz: random garbage, truncation and corruption in each slot independently.</summary>
    private static void ReaderFuzz(string root, Random rng)
    {
        string primary = Path.Combine(root, "reader.json");
        Slot[] order = { Slot.Primary, Slot.Backup, Slot.Staged };
        for (int i = 0; i < Rounds; i++)
        {
            bool envelope = rng.Next(2) == 0;
            Func<string, bool> accepts = envelope
                ? new Func<string, bool>(ValidEnvelope)
                : new Func<string, bool>(ValidGarden);
            string oldText, newText, thirdText;
            MakeStates(rng, envelope, out oldText, out newText, out thirdText);
            string[] fulls = { oldText, newText, thirdText };

            Slots slots = new Slots();
            foreach (Slot slot in order) slots[slot] = RandomSlot(rng, fulls, accepts);
            WriteSlots(primary, slots);

            string got, origin;
            bool loaded = SaveJournal.TryLoad(primary, accepts, out got, out origin);
            string expectedOrigin = null;
            string expected = null;
            for (int s = 0; s < order.Length; s++)
            {
                string text = slots[order[s]];
                if (text != null && accepts(text))
                {
                    expectedOrigin = SlotNames[s];
                    expected = text;
                    break;
                }
            }
            string where = "reader round " + i;
            Expect(loaded == (expectedOrigin != null), where + ": load must succeed exactly when a slot is complete");
            if (loaded)
            {
                Expect(origin == expectedOrigin && got == expected, where + ": load must serve the first complete slot");
                Expect(got == oldText || got == newText || got == thirdText,
                       where + ": load served content that was never a complete save");
            }
            readerCases++;
        }
    }

    public static int Run()
    {
        var clock = System.Diagnostics.Stopwatch.StartNew();
        assertions = 0;
        crashStates = 0;
        readerCases = 0;
        string root = Path.Combine(Path.GetTempPath(), "evergrove-savefuzz-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);
        try
        {
            Random rng = new Random(Seed);
            for (int round = 0; round < Rounds; round++) RunRound(root, rng, round);
            ReaderFuzz(root, rng);
        }
        finally
        {
            Directory.Delete(root, true);
        }
        Console.WriteLine("Evergrove save fuzz tests: " + assertions + " assertions, " +
                          crashStates + " crash states, " + readerCases + " reader cases in " + clock.ElapsedMilliseconds + " ms.");
        return assertions;
    }
}
