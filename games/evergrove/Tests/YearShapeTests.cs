using System;
using System.Collections.Generic;
using Evergrove.Core;

/// <summary>
/// I312 (idea 38): the year has a shape. Through the public API, a garden walks its first year
/// (spring to winter, by story progress): each season turn opens exactly one new chapter, the
/// chapter is usable that turn, earlier chapters stay, and the chapters survive a save. Also
/// records finding 9 of economy-v1 (seasons turn on first solves, bought plants included).
/// </summary>
internal static class YearShapeTests
{
    private static int assertions;

    private static void Expect(bool ok, string message)
    {
        assertions++;
        if (!ok) throw new Exception("Year shape test failed: " + message);
    }

    // Story progress: each chapter solved to its answer, then chosen (three per season turn).
    private static void Advance(WorldModel w, int chapters)
    {
        for (int chapter = 0; chapter < chapters; chapter++)
        {
            Puzzle board = w.CurrentPuzzle;
            for (int i = 0; i < board.CellCount; i++)
                while (w.Rotations[i] != board.Solution[i]) w.Rotate(i);
            w.Choose((VillageChoice)(w.Choices.Length % 3));
        }
    }

    // Takes the chapter's gift the turn it opens, with the neighbour it needs standing in the garden.
    private static void UseChapter(WorldModel w, SeasonUnlock chapter)
    {
        int standing = 0;
        if (chapter.Needs.HasValue)
        {
            // The first four pairings are open from the start; a deeper one (the orchard) opens once all four are found.
            foreach (Pairing first in new[] { Pairing.Sheltered, Pairing.ByTheWater, Pairing.FlowerBed, Pairing.Grove })
                w.DiscoverPairing(first);
            w.DiscoverPairing(chapter.Needs.Value);
            Expect(w.HasPairing(chapter.Needs.Value), "the neighbour is found: " + chapter.Needs.Value);
            standing = PairingMask.Of(chapter.Needs.Value);
        }
        if (chapter.Gives == SeasonYield.Jam)
        {
            // Winter: neighbours bring jam back for fruit given through the year. Tend the shade trees for fruit.
            int[] choices = w.Choices;
            for (int i = 0; i < choices.Length && w.Fruit == 0; i++)
                if (choices[i] == (int)VillageChoice.ShadeTree) w.SolveTree(i);
            Expect(w.Fruit > 0 && w.GiveFruit(0), "winter: a fruit given to Mira");
        }

        int seeds = w.Seeds, fruit = w.Fruit, jam = w.Jam, gifts = w.Gifts.Length;
        Expect(w.TakeSeasonYield(standing) == chapter.Gives, "usable: " + chapter.Brings);
        switch (chapter.Gives)
        {
            case SeasonYield.Seedling: Expect(w.Gifts.Length == gifts + 1, "a seedling joins the basket"); break;
            case SeasonYield.Seeds: Expect(w.Seeds == seeds + SeasonalYields.SummerSeeds, "the grove's seeds"); break;
            case SeasonYield.Fruit: Expect(w.Fruit == fruit + SeasonalYields.AutumnFruit, "the orchard's fruit"); break;
            default: Expect(w.Jam == jam + 1, "a jar of jam"); break;
        }
        Expect(!SeasonalYields.Waiting(w), "taken: nothing waiting this turn");
        Expect(w.TakeSeasonYield(standing) == SeasonYield.None, "once a season turn");
    }

    private static List<string> Names(SeasonUnlock[] chapters)
    {
        var names = new List<string>(chapters.Length);
        foreach (SeasonUnlock chapter in chapters) names.Add(chapter.Brings);
        return names;
    }

    public static int Run()
    {
        assertions = 0;
        Expect(SeasonUnlocks.Unlocked(null).Length == 0, "no garden, no chapters");
        Expect(SeasonUnlocks.Unlocked(WorldModel.New(PlayMode.PuzzleOnly)).Length == 0, "puzzle-only mode has no year");
        Expect(SeasonUnlocks.YearLine(null) == null, "no almanac line without a garden");
        Expect(SeasonUnlocks.For(-1) == null && SeasonUnlocks.For(SeasonUnlocks.FirstYearTurns) == null, "no chapter outside the first year");

        // The first year: one new chapter at every season turn, usable at once, earlier ones kept.
        WorldModel w = WorldModel.New(PlayMode.Living);
        var seen = new List<string>();
        for (int turn = 0; turn < SeasonUnlocks.FirstYearTurns; turn++)
        {
            Expect(SeasonalYields.Key(w) == turn, "season turn " + turn + " reached");
            SeasonUnlock[] open = SeasonUnlocks.Unlocked(w);
            Expect(open.Length == turn + 1, "exactly one new chapter at turn " + turn);
            SeasonUnlock fresh = open[open.Length - 1];
            Expect(fresh == SeasonUnlocks.For(turn) && fresh.Turn == turn, "the new chapter is turn " + turn + "'s");
            Expect(!seen.Contains(fresh.Brings), "a new kind of thing, not a repeat: " + fresh.Brings);
            for (int i = 0; i < seen.Count; i++) Expect(open[i].Brings == seen[i], "earlier chapters stay open");
            seen.Add(fresh.Brings);
            string line = SeasonUnlocks.YearLine(w);
            Expect(line != null && line.Contains(fresh.Season.ToString().ToLowerInvariant() + " brought " + fresh.Brings),
                "the almanac names it: " + line);
            UseChapter(w, fresh);
            if (turn < SeasonUnlocks.FirstYearTurns - 1) Advance(w, 3);
        }

        // Finishing the story keeps all four open; nothing closes.
        Advance(w, 3);
        Expect(w.Completed && SeasonalYields.Key(w) == 4, "the story is finished; the year goes on");
        SeasonUnlock[] all = SeasonUnlocks.Unlocked(w);
        Expect(all.Length == SeasonUnlocks.FirstYearTurns && Names(all).TrueForAll(seen.Contains), "all four still open after the story");
        Expect(SeasonUnlocks.For(4) == null, "after the first year no chapter is named (see report)");
        Expect(SeasonUnlocks.YearLine(w).Contains("winter brought " + seen[3]), "the almanac line lists the year");

        // Deterministic and kept across a save.
        Expect(Names(SeasonUnlocks.Unlocked(w)).TrueForAll(Names(all).Contains), "the same chapters on a second read");
        Expect(WorldModel.TryRestore(w.Export(), out WorldModel back) && Names(SeasonUnlocks.Unlocked(back)).Count == seen.Count &&
               Names(SeasonUnlocks.Unlocked(back)).TrueForAll(seen.Contains), "the chapters survive a save");

        // Finding 9 (economy-v1): after the story the year turns on first solves, bought plants included.
        // Replays never turn it. Twelve grown things solved reach spring, and it waits there.
        Expect(GardenSeason.For(w) == Season.Summer, "after the story: the festival summer");
        for (int i = 0; i < GardenMap.SolvesPerReach; i++) w.SolveTree(i);
        Expect(GardenSeason.For(w) == Season.Autumn, "four first solves turn the year to autumn");
        w.SolveTree(0);
        Expect(GardenSeason.For(w) == Season.Autumn && w.PuzzlesSolved == GardenMap.SolvesPerReach, "a replay does not turn it");
        for (int i = GardenMap.SolvesPerReach; i < w.Choices.Length; i++) w.SolveTree(i);
        Expect(GardenSeason.For(w) == Season.Spring, "all grown things solved: spring");
        for (int i = 0; i < 10; i++) w.SolveTree(i % w.Choices.Length);
        Expect(GardenSeason.For(w) == Season.Spring && w.PuzzlesSolved == w.Choices.Length, "replays keep it at spring");

        // A bought plant's first solve counts too; it takes four solves to turn the year.
        int kind = w.NurseryStock(0);
        Expect(w.Seeds >= Nursery.Price(kind) && w.Buy(kind, 0), "a bought plant (needs seeds)");
        int before = w.PuzzlesSolved;
        w.SolveBought(w.BoughtCount - 1);
        Expect(w.PuzzlesSolved == before + 1 && GardenSeason.For(w) == Season.Spring, "a bought solve counts, one at a time");
        return assertions;
    }
}
