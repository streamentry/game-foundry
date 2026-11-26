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
        return assertions;
    }
}
