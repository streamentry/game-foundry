using System;
using System.Collections.Generic;
using Evergrove.Core;

/// <summary>
/// I259/I260: a replay deals a fresh board, and a thing you keep coming back to deepens.
/// The first play is always the thing's own board (family help plays the same one).
/// </summary>
internal static class ReplayBoardTests
{
    private static int assertions;

    private static void Expect(bool ok, string message)
    {
        assertions++;
        if (!ok) throw new Exception("Replay board test failed: " + message);
    }

    public static int Run()
    {
        const int baseSeed = 2001;
        Expect(ReplayBoards.Seed(baseSeed, 0) == baseSeed, "the first play is the thing's own board");
        var seen = new HashSet<int>();
        for (int r = 0; r < 200; r++)
            Expect(seen.Add(ReplayBoards.Seed(baseSeed, r)), "every replay deals a different board (" + r + ")");
        Expect(ReplayBoards.Seed(baseSeed, 5) == ReplayBoards.Seed(baseSeed, 5), "deterministic");
        Expect(ReplayBoards.Seed(baseSeed, -3) == baseSeed, "a bad count reads as the first play");
        // Deepening: one tier per three replays, never past the hardest, never below the thing's own.
        for (int baseTier = 0; baseTier < TreePuzzles.Tiers; baseTier++)
            for (int r = 0; r < 40; r++)
            {
                int t = ReplayBoards.Tier(baseTier, r);
                Expect(t >= baseTier && t <= TreePuzzles.Tiers - 1, "in range " + baseTier + "/" + r);
                Expect(t == Math.Min(TreePuzzles.Tiers - 1, baseTier + r / ReplayBoards.ReplaysPerTier), "steady deepening");
            }
        Expect(ReplayBoards.Tier(0, 0) == 0 && ReplayBoards.Tier(0, 2) == 0 && ReplayBoards.Tier(0, 3) == 1,
            "a gentle start: three replays before it deepens");
        // Every new-kind engine accepts the varied seeds and tiers (no throw, a real board).
        for (int r = 0; r < 12; r++)
        {
            int seed = ReplayBoards.Seed(baseSeed, r), tier = ReplayBoards.Tier(0, r);
            Expect(new NonogramGame(seed, tier).Size > 0 && new SeedSudokuGame(seed, tier).Size > 0 &&
                   new PlotsGame(seed, tier).Size > 0 && new StreamPathGame(seed, tier).Size > 0 &&
                   new MahjongGame(seed, tier).TileCount > 0 && new PairsGame(seed, tier).Count > 0 &&
                   new MosaicGame(seed, tier).Count > 0 && TreePuzzles.For(seed, tier, "x") != null,
                "every kind deals a board for replay " + r);
        }
        return assertions;
    }
}
