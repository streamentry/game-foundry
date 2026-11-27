using System;
using System.Linq;
using Evergrove.Core;

/// <summary>
/// v1 #52: days played is a total that only grows, never a streak. A skipped day costs nothing,
/// and nothing a player owns ever moves because of the count.
/// </summary>
internal static class DaysPlayedTests
{
    private static int assertions;
    private const string Stamp = "2026-11-27T12:00:00Z";
    private const int D = 20261127;
    private const int D1 = 20261128;
    private const int D2 = 20261129;
    private const int D5 = 20261202;

    private static void Expect(bool ok, string message)
    {
        assertions++;
        if (!ok) throw new Exception("DaysPlayedTests failed: " + message);
    }

    private static WorldModel Restored(WorldSnapshot s)
    {
        Expect(WorldModel.TryRestore(s, out WorldModel w), "snapshot restores");
        return w;
    }

    private static WorldSnapshot Forge(WorldSnapshot s, Action<WorldSnapshot> change)
    {
        change(s);
        return s;
    }

    // A living garden with `count` grown things (solved and chosen), no days played yet.
    private static WorldModel Grown(int count)
    {
        WorldModel world = WorldModel.New(PlayMode.Living);
        for (int chapter = 0; chapter < count; chapter++)
        {
            Puzzle board = world.CurrentPuzzle;
            for (int i = 0; i < board.CellCount; i++)
                while (world.Rotations[i] != board.Solution[i]) world.Rotate(i);
            world.Choose((VillageChoice)(chapter % 3));
        }
        return world;
    }

    // A living garden holding seeds, fruit, jam, a grown solve, a planting, a bought plant and a ledger line.
    private static WorldModel Rich()
    {
        WorldSnapshot s = Grown(3).Export();
        s.seeds = 40;
        s.fruit = 7;
        s.jam = 1;
        s.treeSolved = new[] { 0 };
        s.plantings = new[] { 0, 5 };
        s.bought = new[] { 0, 9 };
        s.ledger = new[] { (int)SeedReason.FirstSolve, 10 };
        return Restored(s);
    }

    // Everything a played day must never touch, as one comparable line.
    private static string Owned(WorldModel w)
    {
        return string.Join("|", new[] {
            w.Seeds.ToString(), w.Fruit.ToString(), w.Jam.ToString(), w.PuzzlesSolved.ToString(),
            string.Join(",", w.Choices), string.Join(",", w.Plantings), string.Join(",", w.Bought),
            string.Join(";", w.Ledger.Select(l => ((int)l.Reason) + ":" + l.Delta)) });
    }

    // A rich living save that has been played on D and D1, with both fields set.
    private static WorldSnapshot Played()
    {
        WorldModel w = Rich();
        w.MarkPlayed(D);
        w.MarkPlayed(D1);
        return w.Export();
    }

    public static int Run()
    {
        Starts();
        FirstAndRepeatDays();
        SkippingCostsNothing();
        ClockSetBack();
        DayZeroChangesNothing();
        OtherModesNeverCount();
        ImpossibleDatesNeverCount();
        SaveRoundTrip();
        OlderSavesRestoreToZero();
        ForgedSavesRefused();
        CottageReportsTheTotal();
        return assertions;
    }

    private static void Starts()
    {
        WorldModel w = WorldModel.New(PlayMode.Living);
        Expect(w.DaysPlayed == 0 && w.LastPlayedDay == 0, "a new garden has no days played");
    }

    private static void FirstAndRepeatDays()
    {
        WorldModel w = WorldModel.New(PlayMode.Living);
        Expect(w.MarkPlayed(D) && w.DaysPlayed == 1 && w.LastPlayedDay == D, "the first day counts one");
        Expect(!w.MarkPlayed(D) && w.DaysPlayed == 1, "the same day again does not count");
        Expect(w.MarkPlayed(D1) && w.DaysPlayed == 2 && w.LastPlayedDay == D1, "the next day counts two");
    }

    private static void SkippingCostsNothing()
    {
        WorldModel w = Rich();
        w.MarkPlayed(D);
        w.MarkPlayed(D1);
        string before = Owned(w);
        Expect(w.MarkPlayed(D5) && w.DaysPlayed == 3 && w.LastPlayedDay == D5,
            "skipping three days still counts the day played: three");
        Expect(Owned(w) == before, "the three skipped days change nothing owned");
    }

    private static void ClockSetBack()
    {
        WorldModel w = Rich();
        w.MarkPlayed(D);
        w.MarkPlayed(D1);
        w.MarkPlayed(D5);
        string before = Owned(w);
        Expect(!w.MarkPlayed(D2) && w.DaysPlayed == 3 && w.LastPlayedDay == D5,
            "setting the clock back changes nothing");
        Expect(Owned(w) == before, "the clock set back changes nothing owned");
    }

    private static void DayZeroChangesNothing()
    {
        WorldModel w = Rich();
        string before = Owned(w);
        Expect(!w.MarkPlayed(0) && !w.MarkPlayed(-D) && w.DaysPlayed == 0 && w.LastPlayedDay == 0,
            "day 0 and negative days change nothing");
        Expect(Owned(w) == before, "day 0 changes nothing owned");
    }

    private static void OtherModesNeverCount()
    {
        WorldModel puzzle = WorldModel.New(PlayMode.PuzzleOnly);
        Expect(!puzzle.MarkPlayed(D) && puzzle.DaysPlayed == 0, "puzzle-only never counts a day");
        int[] schedule = new int[WorldModel.ScheduleLength];
        for (int i = 0; i < schedule.Length; i++) schedule[i] = 1;
        WorldModel matched = WorldModel.NewMatched(schedule);
        Expect(!matched.MarkPlayed(D) && matched.DaysPlayed == 0, "matched never counts a day");
    }

    private static void ImpossibleDatesNeverCount()
    {
        WorldModel w = WorldModel.New(PlayMode.Living);
        Expect(!w.MarkPlayed(20261345) && !w.MarkPlayed(20260230) && !w.MarkPlayed(18991231) &&
               !w.MarkPlayed(10000101) && w.DaysPlayed == 0,
            "impossible dates are never counted");
        Expect(w.MarkPlayed(20240229) && w.DaysPlayed == 1, "a real leap day counts");
    }

    private static void SaveRoundTrip()
    {
        WorldModel w = Rich();
        w.MarkPlayed(D);
        w.MarkPlayed(D5);
        WorldSnapshot snap = w.Export();
        Expect(snap.daysPlayed == 2 && snap.lastPlayedDay == D5, "the snapshot carries both fields");
        Expect(WorldModel.TryRestore(snap, out WorldModel back) && back.DaysPlayed == 2 && back.LastPlayedDay == D5,
            "the snapshot restores both fields");

        string text = GardenTransfer.Export(w, Stamp);
        Expect(text.Contains("\"daysPlayed\":2") && text.Contains("\"lastPlayedDay\":" + D5),
            "the garden file writes both fields");
        Expect(GardenTransfer.TryImport(text, out WorldModel imported, out _) &&
               imported.DaysPlayed == 2 && imported.LastPlayedDay == D5,
            "the garden file restores both fields");
        Expect(GardenTransfer.Export(imported, Stamp) == text, "and re-exports byte-identically");
        Expect(!GardenTransfer.Export(Grown(3), Stamp).Contains("daysPlayed"),
            "a garden never played writes no days field");
    }

    private static void OlderSavesRestoreToZero()
    {
        WorldSnapshot older = Grown(3).Export();
        Expect(older.daysPlayed == 0 && older.lastPlayedDay == 0, "an older save has no days field");
        WorldModel restored = Restored(older);
        Expect(restored.DaysPlayed == 0 && restored.LastPlayedDay == 0, "an older save restores to zero");
    }

    private static void ForgedSavesRefused()
    {
        Expect(WorldModel.TryRestore(Played(), out _), "a valid played save restores");
        Expect(!WorldModel.TryRestore(Forge(Played(), s => s.daysPlayed = -1), out _), "a negative count is refused");
        Expect(!WorldModel.TryRestore(Forge(Played(), s => s.daysPlayed = WorldModel.MaxDaysPlayed + 1), out _),
            "a count past the limit is refused");
        Expect(!WorldModel.TryRestore(Forge(Played(), s => s.lastPlayedDay = 0), out _),
            "days played with no last day is refused");
        Expect(!WorldModel.TryRestore(Forge(Played(), s => { s.daysPlayed = 0; }), out _),
            "a last day with no days played is refused");
        Expect(!WorldModel.TryRestore(Forge(Played(), s => s.lastPlayedDay = 20261345), out _),
            "a month 13 date is refused");
        Expect(!WorldModel.TryRestore(Forge(Played(), s => s.lastPlayedDay = 20260230), out _),
            "a 30 February date is refused");
        Expect(!WorldModel.TryRestore(Forge(Played(), s => s.lastPlayedDay = 18991231), out _),
            "a date before 1900 is refused");
        Expect(!WorldModel.TryRestore(Forge(Played(), s => s.lastPlayedDay = -D), out _),
            "a negative date is refused");

        WorldSnapshot puzzle = WorldModel.New(PlayMode.PuzzleOnly).Export();
        Expect(WorldModel.TryRestore(puzzle, out _), "a puzzle-only save restores");
        Expect(!WorldModel.TryRestore(Forge(puzzle, s => { s.daysPlayed = 1; s.lastPlayedDay = D; }), out _),
            "days played in a puzzle-only garden are refused");
    }

    private static void CottageReportsTheTotal()
    {
        WorldModel w = Grown(3);
        Expect(CottageSummary.For(w, 0).DaysPlayed == 0, "a new garden's cottage reports no days");
        w.MarkPlayed(D);
        w.MarkPlayed(D1);
        w.MarkPlayed(D5);
        Expect(CottageSummary.For(w, 0).DaysPlayed == 3, "the cottage reports three days played");
    }
}
