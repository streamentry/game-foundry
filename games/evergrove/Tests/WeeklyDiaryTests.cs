using System;
using System.Collections.Generic;
using Evergrove.Core;

/// <summary>
/// Checklist item 58: the almanac's weekly diary page is written only from what the save records, is
/// deterministic, lists gains only, and is short. Scenarios are real play through the public API; only seed
/// and fruit starting points are set on a snapshot, as the economy tests do.
/// </summary>
internal static class WeeklyDiaryTests
{
    private static int assertions;
    private const string Stamp = "2026-11-27T12:00:00Z";
    private const int Week = 48;
    private const int Year = 2026;
    private static readonly string[] LossWords = { "died", "lost", "missed", "wilted", "gone", "forgot", "sorry" };

    private static void Expect(bool ok, string message)
    {
        assertions++;
        if (!ok) throw new Exception("WeeklyDiaryTests failed: " + message);
    }

    // A living garden with `count` chapters played (every board solved, one village choice each) and a starting
    // seed and fruit count. The ledger starts empty.
    private static WorldModel Grown(int count, int seeds, int fruit = 0)
    {
        WorldModel world = WorldModel.New(PlayMode.Living);
        for (int chapter = 0; chapter < count; chapter++)
        {
            SolveBoard(world);
            world.Choose((VillageChoice)(chapter % 3));
        }
        WorldSnapshot s = world.Export();
        s.seeds = seeds;
        s.fruit = fruit;
        Expect(WorldModel.TryRestore(s, out WorldModel restored), "starting garden restores");
        return restored;
    }

    private static void SolveBoard(WorldModel world)
    {
        Puzzle board = world.CurrentPuzzle;
        for (int i = 0; i < board.CellCount; i++)
            while (world.Rotations[i] != board.Solution[i]) world.Rotate(i);
    }

    // A letter from a sender in another garden, as a family parcel text.
    private static string LetterText(string nonce)
    {
        ParcelSendResult sent = WorldModel.New(PlayMode.Living).SendParcel("Nan", "Thinking of you.", ParcelGift.None, 0, nonce);
        Expect(sent.Ok, "a letter-only parcel is sent");
        return sent.Text;
    }

    // Plays a busy week on `world`: two first solves, one more chapter, a bought plant, a discovered pairing,
    // one letter, and a jar of jam made from the fruit the week's first solve picked.
    private static void PlayBusyWeek(WorldModel world)
    {
        world.SolveTree(0);
        world.SolveTree(1);
        SolveBoard(world);
        Expect(world.Choose(VillageChoice.Flowers), "the next chapter's choice is made");
        Expect(world.Buy(0, 9), "a plant is bought from the nursery");
        Expect(world.DiscoverPairing(Pairing.Sheltered) == Pairings.Reward, "a pairing is found");
        Expect(world.Open(LetterText("week-letter")).Ok, "a letter arrives");
        Expect(world.MakeJam(), "a jar of jam is made");
    }

    // The page as one comparable string, so byte-identity is checked exactly.
    private static string Text(DiaryPage page)
    {
        return page.Year + "|" + page.Week + "|" + page.Title + "|" + string.Join("\n", page.Lines);
    }

    private static bool HasLossWord(string line)
    {
        string lower = line.ToLowerInvariant();
        foreach (string word in LossWords)
            if (lower.Contains(word)) return true;
        return false;
    }

    private static void CheckShape(DiaryPage page, string what)
    {
        Expect(page.Lines.Length >= 1 && page.Lines.Length <= WeeklyDiary.MaxLines, what + ": at most 8 lines");
        foreach (string line in page.Lines)
        {
            Expect(line.Length > 0 && line.Length < 80, what + ": line under 80 characters: " + line);
            Expect(!HasLossWord(line), what + ": no loss words: " + line);
            Expect(line.IndexOf('!') < 0, what + ": no exclamation marks: " + line);
        }
    }

    private static void Throws<T>(Action action, string message) where T : Exception
    {
        try
        {
            action();
        }
        catch (T)
        {
            assertions++;
            return;
        }
        Expect(false, message);
    }

    public static int Run()
    {
        Deterministic();
        BusyWeekLists();
        QuietWeek();
        SeedsFromLedger();
        SeedsNetWhenLedgerFull();
        SaveRoundTrip();
        ClosingVaries();
        Refusals();
        return assertions;
    }

    private static void Deterministic()
    {
        WorldModel start = Grown(3, 100, 3);
        WorldSnapshot weekStart = start.Export();
        PlayBusyWeek(start);
        WorldSnapshot now = start.Export();
        string first = Text(WeeklyDiary.Write(weekStart, now, Week, Year));
        for (int i = 0; i < 2; i++)
            Expect(Text(WeeklyDiary.Write(weekStart, now, Week, Year)) == first, "the same inputs give the same page (call " + (i + 2) + ")");
    }

    // Exact lines of the busy week. Fruit: the first solve picks one, but jam cooks three, so the net is down
    // and nothing about fruit is written; only the jar of jam is a gain.
    private static void BusyWeekLists()
    {
        WorldModel start = Grown(3, 100, 3);
        WorldSnapshot weekStart = start.Export();
        PlayBusyWeek(start);
        WorldSnapshot now = start.Export();
        int change = start.Seeds - 100;
        DiaryPage page = WeeklyDiary.Write(weekStart, now, Week, Year);

        Expect(page.Title == "Week 48, 2026" && page.Week == Week && page.Year == Year, "title and week are set");
        CheckShape(page, "busy week");
        Expect(page.Lines.Length == 8, "a busy week fills eight lines including the closing one");
        Expect(page.Lines[0] == "1 new thing took root in the village.", "one village choice grew: " + page.Lines[0]);
        Expect(page.Lines[1] == "1 plant went into the garden.", "one plant went in: " + page.Lines[1]);
        Expect(page.Lines[2] == "2 puzzles solved for the first time.", "two puzzles solved: " + page.Lines[2]);
        Expect(page.Lines[3] == "1 good-neighbour pairing found.", "one pairing found: " + page.Lines[3]);
        Expect(page.Lines[4] == "1 letter arrived.", "one letter arrived: " + page.Lines[4]);
        Expect(page.Lines[5] == "Jam: 1 jar more in the pantry.", "one jar of jam, no fruit line: " + page.Lines[5]);
        // Earned is the seed change plus the 50 spent on the plant (the ledger is empty at the start).
        Expect(page.Lines[6] == "Seeds: earned " + (change + 50) + ", spent 50.", "seeds earned and spent: " + page.Lines[6]);
        Expect(Array.IndexOf(WeeklyDiary.Closings(), page.Lines[7]) >= 0, "the last line is a closing line");
    }

    private static void QuietWeek()
    {
        WorldModel world = Grown(3, 100, 3);
        WorldSnapshot a = world.Export();
        DiaryPage page = WeeklyDiary.Write(a, world.Export(), Week, Year);
        Expect(page.Lines.Length == 1 && page.Lines[0] == WeeklyDiary.QuietLine, "an empty week is the quiet page");
        Expect(page.Lines[0] == "A quiet week in the garden.", "the quiet line reads as written");
        CheckShape(page, "quiet week");

        // Spending seeds that were only spent (no gains) still reports, neutrally, and does not read as a loss.
        WorldModel buyer = Grown(3, 100);
        WorldSnapshot before = buyer.Export();
        Expect(buyer.Buy(0, 9), "a plant bought for the spend-only week");
        DiaryPage spent = WeeklyDiary.Write(before, buyer.Export(), Week, Year);
        Expect(spent.Lines[0] == "1 plant went into the garden.", "the plant is reported as planted");
        Expect(spent.Lines[1] == "Seeds: spent 50.", "the spend is reported plainly: " + spent.Lines[1]);
        CheckShape(spent, "spend-only week");
    }

    // The seed ledger records every change; with a short start ledger the page splits earned from spent exactly.
    private static void SeedsFromLedger()
    {
        WorldModel world = Grown(3, 100);
        WorldSnapshot before = world.Export();
        Expect(world.Buy(0, 9), "a plant is bought");
        world.SolveTree(0);
        WorldSnapshot now = world.Export();
        // Ledger is newest first: the solve is the newest line, the purchase the one before it.
        int earned = world.Ledger[0].Delta;
        Expect(world.Ledger[0].Reason == SeedReason.FirstSolve && world.Ledger[1].Reason == SeedReason.Nursery,
            "the ledger has the solve and the purchase");
        string seeds = null;
        foreach (string line in WeeklyDiary.Write(before, now, Week, Year).Lines)
            if (line.StartsWith("Seeds:", StringComparison.Ordinal)) seeds = line;
        Expect(seeds == "Seeds: earned " + earned + ", spent 50.", "earned and spent come from the ledger: " + seeds);
    }

    // A start ledger already full at the cap can lose lines, so the split is not provable and only the net
    // seed change is written.
    private static void SeedsNetWhenLedgerFull()
    {
        WorldSnapshot full = Grown(3, 100).Export();
        full.ledger = new int[WorldModel.MaxLedger * 2];
        for (int i = 0; i < full.ledger.Length; i += 2)
        {
            full.ledger[i] = (int)SeedReason.Keepsake;
            full.ledger[i + 1] = 1;
        }
        Expect(WorldModel.TryRestore(full, out WorldModel startWorld), "a full-ledger start restores");
        WorldSnapshot now = startWorld.Export();
        now.seeds += 5;
        DiaryPage page = WeeklyDiary.Write(full, now, Week, Year);
        Expect(page.Lines[0] == "Seeds: 5 gained, net.", "a full ledger gives the net change: " + page.Lines[0]);
    }

    // The page is a function of the save: export, restore, round-trip through the transfer file, and the same
    // page comes out byte for byte.
    private static void SaveRoundTrip()
    {
        WorldModel start = Grown(3, 100, 3);
        string startJson = GardenTransfer.Export(start, Stamp);
        WorldSnapshot weekStart = start.Export();
        PlayBusyWeek(start);
        string original = Text(WeeklyDiary.Write(weekStart, start.Export(), Week, Year));

        Expect(WorldModel.TryRestore(start.Export(), out WorldModel restored), "the week's end restores from its snapshot");
        string restoredText = Text(WeeklyDiary.Write(weekStart, restored.Export(), Week, Year));
        Expect(restoredText == original, "restoring the end gives the same page");

        string endJson = GardenTransfer.Export(restored, Stamp);
        Expect(GardenTransfer.TryImport(endJson, out WorldModel imported, out GardenImportError error), "the end is imported: " + error);
        Expect(GardenTransfer.TryImport(startJson, out WorldModel startImported, out error), "the start is imported: " + error);
        string transferred = Text(WeeklyDiary.Write(startImported.Export(), imported.Export(), Week, Year));
        Expect(transferred == original, "a JSON round trip of both snapshots gives a byte-identical page");
    }

    // The closing line depends on the week, so a run of weeks shows more than one closing line.
    private static void ClosingVaries()
    {
        WorldModel start = Grown(3, 100, 3);
        WorldSnapshot weekStart = start.Export();
        PlayBusyWeek(start);
        WorldSnapshot now = start.Export();
        var seen = new HashSet<string>();
        for (int week = 1; week <= 20; week++)
        {
            DiaryPage page = WeeklyDiary.Write(weekStart, now, week, Year);
            string closing = page.Lines[page.Lines.Length - 1];
            Expect(Array.IndexOf(WeeklyDiary.Closings(), closing) >= 0, "week " + week + " ends with a closing line");
            seen.Add(closing);
        }
        Expect(seen.Count > 1, "closing lines vary across weeks");
        foreach (string closing in WeeklyDiary.Closings())
            Expect(!HasLossWord(closing) && closing.Length < 80, "closing line is calm: " + closing);
    }

    private static void Refusals()
    {
        WorldSnapshot a = Grown(3, 100).Export();
        Throws<ArgumentOutOfRangeException>(() => WeeklyDiary.Write(a, a, 0, Year), "week 0 is refused");
        Throws<ArgumentOutOfRangeException>(() => WeeklyDiary.Write(a, a, 54, Year), "week 54 is refused");
        Throws<ArgumentNullException>(() => WeeklyDiary.Write(null, a, Week, Year), "a missing week start is refused");
        WorldSnapshot other = WorldModel.New(PlayMode.Living, 3).Export();
        Throws<ArgumentException>(() => WeeklyDiary.Write(a, other, Week, Year), "a different garden is refused");
    }
}
