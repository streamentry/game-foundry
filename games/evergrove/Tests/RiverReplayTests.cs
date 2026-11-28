using System;
using System.Reflection;
using Evergrove.Core;

// Checklist 41: once nothing new is left to solve, replays slowly open the river, one stretch per eight.
// Pure rules and saves only; the mist's drops and the map wiring are checked by the Unity compile and the tour.
internal static class RiverReplayTests
{
    private static int assertions;

    private static void Expect(bool ok, string message)
    {
        assertions++;
        if (!ok) throw new Exception("RiverReplayTests failed: " + message);
    }

    public static int Run()
    {
        assertions = 0;
        ThreeArgOverloadUnchanged();
        CapIsNeverExceeded();
        FirstSolvesNeverCount();
        ReplaysWhileSomethingIsLeftDoNotCount();
        ReplaysOpenOneStretchPerEight();
        CountSurvivesSaveAndClamps();
        OldSaveLoadsWithZero();
        NoStretchCountIsStored();
        DropsCountReplaysOnceNothingIsNew();
        SeedsIgnoreTheRiverRule();
        RiverGrowsBeyondFiftyEight();
        return assertions;
    }

    private static int Reaches(WorldModel world)
    {
        return GardenMap.ReachesFor(world.Choices.Length, world.Completed, world.PuzzlesSolved, world.RiverReplays);
    }

    // A living garden whose story is finished: twelve choices made, no puzzle solved yet.
    private static WorldModel Finished()
    {
        WorldModel world = WorldModel.New(PlayMode.Living);
        for (int chapter = 0; chapter < PuzzleCatalog.Count; chapter++)
        {
            Puzzle board = world.CurrentPuzzle;
            for (int i = 0; i < board.CellCount; i++)
                while (world.Rotations[i] != board.Solution[i]) world.Rotate(i);
            world.Choose((VillageChoice)(chapter % 3));
        }
        Expect(world.Completed && world.Choices.Length == PuzzleCatalog.Count, "the story finishes with every choice made");
        return world;
    }

    // The finished garden with every story thing solved, plus `bought` plants of which `boughtSolved` are solved.
    private static WorldModel Garden(int bought, int boughtSolved)
    {
        WorldModel world = Finished();
        for (int i = 0; i < world.Choices.Length; i++) world.SolveTree(i);
        if (bought == 0) return world;
        WorldSnapshot s = world.Export();
        s.bought = new int[bought * 2];
        s.boughtSolved = new int[boughtSolved];
        for (int k = 0; k < bought; k++)
        {
            s.bought[k * 2] = k % Nursery.Kinds;
            s.bought[k * 2 + 1] = 9000 + k * 40;
        }
        for (int k = 0; k < boughtSolved; k++) s.boughtSolved[k] = k;
        Expect(WorldModel.TryRestore(s, out WorldModel restored), "a garden with " + bought + " plants restores");
        return restored;
    }

    // Existing rules are unchanged: the three-argument forms equal the four-argument ones with no replays.
    private static void ThreeArgOverloadUnchanged()
    {
        for (int restored = 0; restored <= 14; restored++)
            for (int solved = 0; solved <= 300; solved++)
            {
                Expect(GardenMap.ReachesFor(restored, true, solved) == GardenMap.ReachesFor(restored, true, solved, 0),
                    "three-argument reach equals zero replays at " + solved);
                Expect(GardenMap.ReachesFor(restored, false, solved) == GardenMap.ReachesFor(restored, false, solved, 9),
                    "replays never open stretches before the story ends");
            }
        for (int solved = 0; solved <= 40; solved++)
        {
            Expect(GardenMap.TowardNext(12, true, solved, out int h3, out int n3) &&
                   GardenMap.TowardNext(12, true, solved, 0, false, out int h4, out int n4) && h3 == h4 && n3 == n4,
                "three-argument drops equal the no-replay drops at " + solved);
        }
        Expect(GardenMap.ReachesFor(12, true, 0, 0) == GardenMap.MaxReaches, "the finished story alone opens five");
    }

    // The total never passes the safety bound, whatever the inputs (long math, no overflow).
    private static void CapIsNeverExceeded()
    {
        Expect(GardenMap.ReachesFor(12, true, int.MaxValue, int.MaxValue) == GardenMap.SafetyReachBound,
            "huge solves and replays stop at the safety bound");
        Expect(GardenMap.ReachesFor(12, true, 0, int.MaxValue) == GardenMap.SafetyReachBound,
            "huge replays alone stop at the safety bound");
        Expect(GardenMap.ReachesFor(12, true, 0, int.MaxValue) <= GardenMap.SafetyReachBound, "never above the bound");
        Expect(GardenMap.ReachesFor(12, true, 0, WorldModel.MaxRiverReplays) == GardenMap.SafetyReachBound,
            "the largest replay count reaches the bound and no further");
        Expect(GardenMap.ReachesFor(0, true, int.MaxValue, 0) == GardenMap.SafetyReachBound, "the old bound still holds");
        Expect(!GardenMap.TowardNext(12, true, 0, int.MaxValue, true, out _, out _),
            "no drops once the bound is reached by replays");
    }

    // A first solve is never a river replay, even the last one that finishes the list.
    private static void FirstSolvesNeverCount()
    {
        WorldModel world = Finished();
        for (int i = 0; i < world.Choices.Length; i++)
        {
            world.SolveTree(i);
            Expect(world.RiverReplays == 0, "first solve " + i + " is not a replay");
        }
        Expect(world.NothingNewToSolve, "every thing is solved after the first solves");
        world.SolveTree(0);
        Expect(world.RiverReplays == 1, "the first replay after that counts");
    }

    // While anything is new to solve, replays are only replays: they open nothing by this rule.
    private static void ReplaysWhileSomethingIsLeftDoNotCount()
    {
        WorldModel world = Finished();
        for (int i = 0; i < 5; i++) world.SolveTree(i);
        for (int n = 0; n < 20; n++) world.SolveTree(0);
        Expect(world.RiverReplays == 0, "replays while choices are unsolved do not count");
        Expect(!world.NothingNewToSolve, "something is still new to solve");
        for (int i = 5; i < world.Choices.Length; i++) world.SolveTree(i);
        Expect(world.RiverReplays == 0, "the rest are first solves, not replays");
        world.SolveTree(3);
        Expect(world.RiverReplays == 1, "after everything is solved, a replay counts");

        WorldModel plants = Garden(2, 1);
        plants.SolveTree(0);
        Expect(!plants.NothingNewToSolve && plants.RiverReplays == 0, "a tree replay with a plant unsolved does not count");
        int before = plants.RiverReplays;
        plants.SolveBought(0);
        Expect(plants.RiverReplays == before, "a plant replay while another plant is unsolved does not count");
        plants.SolveBought(1);
        Expect(plants.RiverReplays == before, "a first plant solve does not count");
        plants.SolveBought(0);
        Expect(plants.RiverReplays == before + 1, "with everything solved, a plant replay counts");
        Expect(WorldModel.New(PlayMode.PuzzleOnly).SolveTree(0) == 0, "puzzle-only gardens never count");
    }

    // Every eight replays open one more stretch, with the story's solves unchanged.
    private static void ReplaysOpenOneStretchPerEight()
    {
        WorldModel world = Garden(0, 0);
        int baseline = Reaches(world);
        Expect(baseline == GardenMap.MaxReaches + Math.Max(0, world.PuzzlesSolved) / GardenMap.SolvesPerReach,
            "the baseline counts solves only: " + baseline);
        for (int n = 1; n <= 24; n++)
        {
            world.SolveTree(0);
            Expect(world.RiverReplays == n, "replay " + n + " is counted");
            Expect(Reaches(world) == baseline + n / GardenMap.ReplaysPerReach,
                "after " + n + " replays the river has " + (n / GardenMap.ReplaysPerReach) + " more stretches");
        }
        Expect(GardenMap.ReplaysPerReach == 8, "one stretch per eight replays");
    }

    // The counter round-trips through a save, and negatives or huge values clamp instead of failing.
    private static void CountSurvivesSaveAndClamps()
    {
        WorldModel world = Garden(0, 0);
        for (int n = 0; n < 5; n++) world.SolveTree(0);
        WorldSnapshot s = world.Export();
        Expect(s.riverReplays == 5, "the save holds the five replays");
        Expect(WorldModel.TryRestore(s, out WorldModel again) && again.RiverReplays == 5, "five replays restore");
        Expect(Reaches(again) == Reaches(world), "the same stretches after a restore");

        s.riverReplays = -5;
        Expect(WorldModel.TryRestore(s, out WorldModel low) && low.RiverReplays == 0, "a negative count clamps to zero");
        s.riverReplays = int.MaxValue;
        Expect(WorldModel.TryRestore(s, out WorldModel high) && high.RiverReplays == WorldModel.MaxRiverReplays,
            "a huge count clamps to the maximum");
        Expect(high.Export().riverReplays == WorldModel.MaxRiverReplays, "the clamped maximum is what the save keeps");

        WorldModel puzzle = WorldModel.New(PlayMode.PuzzleOnly);
        WorldSnapshot ps = puzzle.Export();
        ps.riverReplays = 3;
        Expect(WorldModel.TryRestore(ps, out WorldModel pr) && pr.RiverReplays == 0, "outside a living garden the count is zero");
    }

    // A save written before checklist 41 has no field: an absent int is zero, and the river is what it was.
    private static void OldSaveLoadsWithZero()
    {
        WorldModel world = Garden(0, 0);
        WorldSnapshot old = world.Export();
        old.riverReplays = 0;
        Expect(WorldModel.TryRestore(old, out WorldModel loaded) && loaded.RiverReplays == 0, "an old save loads with zero replays");
        Expect(Reaches(loaded) == GardenMap.ReachesFor(12, true, loaded.PuzzlesSolved), "and opens the stretches it always did");
    }

    // The format stores solves and replays only, never a stretch count, so it has no gameplay maximum.
    private static void NoStretchCountIsStored()
    {
        foreach (FieldInfo field in typeof(WorldSnapshot).GetFields(BindingFlags.Public | BindingFlags.Instance))
        {
            string name = field.Name.ToLowerInvariant();
            Expect(!name.Contains("reach") && !name.Contains("stretch"), "no stretch count is saved: " + field.Name);
        }
    }

    // Once nothing new is left, the mist's drops count replays toward the next stretch, eight to a stretch.
    private static void DropsCountReplaysOnceNothingIsNew()
    {
        WorldModel world = Garden(0, 0);
        Expect(world.TowardNextStretch(out int have, out int need) && have == 0 && need == GardenMap.ReplaysPerReach,
            "no replays yet: no drops");
        for (int n = 1; n <= 3; n++) world.SolveTree(0);
        Expect(world.TowardNextStretch(out have, out need) && have == 3 && need == GardenMap.ReplaysPerReach,
            "three replays fill three drops of eight");
        for (int n = 4; n <= 8; n++) world.SolveTree(0);
        Expect(world.TowardNextStretch(out have, out need) && have == 0 && need == GardenMap.ReplaysPerReach,
            "the eighth replay opens a stretch and the drops start again");

        WorldModel unsolved = Finished();
        for (int i = 0; i < 5; i++) unsolved.SolveTree(i);
        Expect(unsolved.TowardNextStretch(out have, out need) && have == 1 && need == GardenMap.SolvesPerReach,
            "while things are new, the drops count solves as before");
    }

    // Replays pay what they always paid, with or without the river rule: a garden with a plant still unsolved
    // (no replays count) and one with everything solved (replays count) earn the same seeds for the same sequence.
    private static void SeedsIgnoreTheRiverRule()
    {
        WorldModel counting = Garden(0, 0);
        WorldModel notCounting = Garden(1, 0);
        Expect(counting.Seeds == notCounting.Seeds, "both gardens start with the same seeds");
        const int day = 20250101;
        for (int n = 0; n < 30; n++)
        {
            int item = n % 12;
            int a = counting.SolveTree(item, day);
            int b = notCounting.SolveTree(item, day);
            Expect(a == b, "replay " + n + " pays the same");
            Expect(counting.Seeds == notCounting.Seeds, "the seed balance is identical after replay " + n);
        }
        Expect(counting.RiverReplays == 30 && notCounting.RiverReplays == 0, "only the everything-solved garden counts");
    }

    // Once everything is solved, replays keep opening the river past the old fifty-eight stretches.
    private static void RiverGrowsBeyondFiftyEight()
    {
        WorldModel world = Garden(WorldModel.MaxBought, WorldModel.MaxBought);
        Expect(world.PuzzlesSolved == 12 + WorldModel.MaxBought, "every thing is solved");
        int before = Reaches(world);
        Expect(before == 58, "the solves alone stop at fifty-eight stretches, got " + before);
        for (int n = 0; n < 40; n++) world.SolveTree(n % 12);
        int after = Reaches(world);
        Expect(after == 63 && after > 58, "forty replays open five more stretches, got " + after);
    }
}
