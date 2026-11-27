using System;
using System.Collections.Generic;
using Evergrove.Core;

/// <summary>
/// I203: every tree in the garden holds its own puzzle (an idea from the
/// founder's daughter). Made from a seed, always the same for that tree, always
/// one answer, and a little harder at each tier.
/// </summary>
internal static class TreePuzzleTests
{
    private static int assertions;

    private static void Expect(bool ok, string message)
    {
        assertions++;
        if (!ok) throw new Exception("Tree puzzle test failed: " + message);
    }

    private static string Key(Puzzle p)
    {
        var parts = new List<string>();
        for (int i = 0; i < p.CellCount; i++) parts.Add(p.Kinds[i] + ":" + p.Solution[i]);
        return string.Join(",", parts);
    }

    private static int Scrambled(Puzzle p)
    {
        int count = 0;
        for (int i = 0; i < p.CellCount; i++) if (p.Initial[i] != p.Solution[i]) count++;
        return count;
    }

    public static int Run()
    {
        Expect(TreePuzzles.Tiers == 3, "three tiers: everyday, a little harder, harder");
        var scrambledByTier = new int[TreePuzzles.Tiers];
        for (int tier = 0; tier < TreePuzzles.Tiers; tier++)
        {
            var distinct = new HashSet<string>();
            for (int seed = 0; seed < 120; seed++)
            {
                Puzzle p = TreePuzzles.For(seed, tier, "Tree " + seed);
                string label = "seed " + seed + " tier " + tier;
                Expect(p.Name == "Tree " + seed, label + ": keeps its name");
                Expect(p.Size == (tier == 0 ? 3 : 4), label + ": 3x3 everyday, 4x4 above");
                int ends = 0;
                foreach (PipeKind kind in p.Kinds) if (kind == PipeKind.End) ends++;
                Expect(ends == 2, label + ": one source, one outlet");
                Expect(PipeRules.IsComplete(p, p.Solution), label + ": its answer works");
                Expect(PipeRules.CountSolutions(p, 2) == 1, label + ": exactly one answer");
                Expect(PipeRules.ValidRotations(p, p.Initial), label + ": starts in valid turns");
                Expect(!PipeRules.IsComplete(p, p.Initial), label + ": never starts solved");
                Puzzle again = TreePuzzles.For(seed, tier, "Tree " + seed);
                Expect(Key(again) == Key(p) && string.Join(",", again.Initial) == string.Join(",", p.Initial),
                    label + ": the same tree always has the same puzzle");
                distinct.Add(Key(p));
                scrambledByTier[tier] += Scrambled(p) * 16 / p.CellCount;
            }
            Expect(distinct.Count >= (tier == 0 ? 6 : 40), "tier " + tier + ": many different boards, got " + distinct.Count);
        }
        Expect(scrambledByTier[2] > scrambledByTier[1], "the top tier starts more turned than the middle one");
        bool threw = false;
        try { TreePuzzles.For(1, TreePuzzles.Tiers); } catch (ArgumentOutOfRangeException) { threw = true; }
        Expect(threw, "an unknown tier is refused");
        Expect(TreePuzzles.TierFor(0) == 0 && TreePuzzles.TierFor(5) == 1 && TreePuzzles.TierFor(11) == 2,
            "tier follows the stream the thing was grown in: early, middle, late");
        SeedTests();
        return assertions;
    }

    // I204: solving a thing's puzzle earns seeds (founder decision 2026-11-26):
    // the first solve pays most, every replay still pays a little; no clock.
    private static void SeedTests()
    {
        WorldModel world = WorldModel.New(PlayMode.Living);
        Expect(world.Seeds == 0, "a new garden has no seeds");
        Expect(world.SolveTree(0) == 0, "nothing grown yet, nothing to solve");
        GrowThings(world, 9);
        Expect(!world.TreeSolved(0), "not solved before playing");
        Expect(world.SolveTree(0) == 10, "first solve of an everyday thing: 10 seeds");
        Expect(world.TreeSolved(0) && world.Seeds == 10, "remembered, and banked");
        Expect(world.SolveTree(0) == 2, "a replay still pays a little");
        Expect(world.SolveTree(5) == 20, "a little harder thing pays 20 the first time");
        Expect(world.SolveTree(8) == 40, "a harder thing pays 40 the first time");
        Expect(world.SolveTree(8) == 8, "and 8 on a replay");
        Expect(world.Seeds == 80, "seeds add up: 10+2+20+40+8");
        Expect(world.SolveTree(9) == 0 && world.SolveTree(-1) == 0, "only things you have grown");

        WorldSnapshot saved = world.Export();
        Expect(WorldModel.TryRestore(saved, out WorldModel back), "a save with seeds restores");
        Expect(back.Seeds == 80 && back.TreeSolved(5) && !back.TreeSolved(1), "seeds and solved things survive a save");
        string file = GardenTransfer.Export(world, "2026-11-26T12:00:00Z");
        Expect(GardenTransfer.TryImport(file, out WorldModel imported, out GardenImportError error) &&
               imported.Seeds == 80 && imported.TreeSolved(8), "seeds travel in the garden file (" + error + ")");

        WorldSnapshot bad = world.Export();
        bad.seeds = -1;
        Expect(!WorldModel.TryRestore(bad, out _), "negative seeds refused");
        bad = world.Export();
        bad.treeSolved = new[] { 0, 0 };
        Expect(!WorldModel.TryRestore(bad, out _), "a thing solved twice in the list is refused");
        bad = world.Export();
        bad.treeSolved = new[] { 42 };
        Expect(!WorldModel.TryRestore(bad, out _), "a solved thing that was never grown is refused");
        WorldSnapshot puzzleOnly = WorldModel.New(PlayMode.PuzzleOnly).Export();
        puzzleOnly.seeds = 5;
        Expect(!WorldModel.TryRestore(puzzleOnly, out _), "seeds belong to the living garden only");
    }

    private static void GrowThings(WorldModel world, int count)
    {
        for (int chapter = 0; chapter < count; chapter++)
        {
            Puzzle board = world.CurrentPuzzle;
            for (int i = 0; i < board.CellCount; i++)
                while (world.Rotations[i] != board.Solution[i]) world.Rotate(i);
            Expect(world.Solved, "stream " + chapter + " solved");
            Expect(world.Choose((VillageChoice)(chapter % 3)), "choice " + chapter + " made");
        }
    }
}
