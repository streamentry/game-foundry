using System;
using System.Collections.Generic;
using Evergrove.Core;

/// <summary>Sowing, the puzzle the seed beds hold: sow ripe beds toward the basket until every seed is home.</summary>
internal static class SowingTests
{
    private static int assertions;

    private static void Expect(bool ok, string message)
    {
        assertions++;
        if (!ok) throw new Exception("Sowing test failed: " + message);
    }

    private static string Key(SowingGame g)
    {
        var parts = new List<string>();
        for (int b = 1; b <= g.Beds; b++) parts.Add(g.Seeds(b).ToString());
        return g.Total + ":" + string.Join(",", parts);
    }

    private static int Count(SowingGame g)
    {
        int sum = g.InBasket;
        for (int b = 1; b <= g.Beds; b++) sum += g.Seeds(b);
        return sum;
    }

    private static int[] Snapshot(SowingGame g)
    {
        var s = new int[g.Beds + 2];
        for (int b = 1; b <= g.Beds; b++) s[b] = g.Seeds(b);
        s[g.Beds + 1] = g.InBasket;
        return s;
    }

    // Exhaustive search over every ripe move, deliberately not assuming the nearest-first rule.
    // pits[0] is unused; pits[i] is the seed count of bed i; left counts seeds not yet home.
    private static bool Clearable(int[] pits, int left)
    {
        if (left == 0) return true;
        for (int i = 1; i < pits.Length; i++)
        {
            if (pits[i] != i) continue;
            var next = (int[])pits.Clone();
            next[i] = 0;
            for (int j = 1; j < i; j++) next[j]++;
            if (Clearable(next, left - 1)) return true;
        }
        return false;
    }

    private static void Enumerate(int[] pits, int index, int remaining, List<int[]> all)
    {
        if (index == pits.Length)
        {
            if (remaining == 0) all.Add((int[])pits.Clone());
            return;
        }
        for (int k = 0; k <= remaining; k++)
        {
            pits[index] = k;
            Enumerate(pits, index + 1, remaining - k, all);
        }
        pits[index] = 0;
    }

    private static void UniquenessTests()
    {
        for (int n = 1; n <= 10; n++)
        {
            // A seed in bed k needs k seeds in that bed, so ten beds cover every position of up to ten seeds.
            var all = new List<int[]>();
            Enumerate(new int[11], 1, n, all);
            int clearable = 0;
            int[] found = null;
            foreach (int[] p in all)
            {
                if (!Clearable(p, n)) continue;
                clearable++;
                found = p;
            }
            Expect(clearable == 1, "exactly one clearable start for n=" + n + " (found " + clearable + ")");
            SowingGame g = SowingGame.ForTotal(n);
            Expect(g.Total == n, "ForTotal total n=" + n);
            for (int b = 1; b <= 10; b++)
                Expect(g.Seeds(b) == found[b], "generated start matches the unique one, n=" + n + " bed " + b);
        }
    }

    private static void PlayTests(int tier)
    {
        for (int seed = 1; seed <= 200; seed++)
        {
            var g = new SowingGame(seed, tier);
            int n = g.Total;
            Expect(n >= SowingGame.MinSeeds(tier) && n <= SowingGame.MaxSeeds(tier), "tier range respected t" + tier + " s" + seed);
            Expect(g.Beds >= 1 && g.Beds <= 10, "at most ten beds t" + tier + " s" + seed);
            Expect(!g.Solved && g.Moves == 0 && g.InBasket == 0, "starts unsolved t" + tier + " s" + seed);
            Expect(Count(g) == n, "seeds add up at start");
            int expected = SowingGame.MovesToClear(n);
            int played = 0;
            while (!g.Solved)
            {
                int bed = g.Hint();
                Expect(bed >= 1 && bed <= g.Beds, "hint names a bed while unsolved");
                Expect(g.IsRipe(bed), "hint bed is ripe");
                for (int b = 1; b < bed; b++) Expect(!g.IsRipe(b), "hint is the ripe bed nearest the basket");
                int before = g.InBasket;
                Expect(g.Sow(bed) == SowMove.Sowed, "hint sows");
                played++;
                Expect(g.InBasket == before + 1, "one seed reaches the basket per sow");
                Expect(Count(g) == n, "seeds conserved after every move");
                Expect(g.Moves == played, "moves counted");
                Expect(played <= n, "never loops");
            }
            Expect(played == expected, "solved in exactly the right number of moves t" + tier + " s" + seed);
            Expect(g.InBasket == n, "all seeds in the basket");
            Expect(g.Hint() == -1, "no hint once solved");
            Expect(g.Sow(1) == SowMove.Ignored, "sowing after solved is ignored");
        }
    }

    private static void RefusalTests()
    {
        for (int tier = 0; tier < TreePuzzles.Tiers; tier++)
            for (int seed = 1; seed <= 30; seed++)
            {
                var g = new SowingGame(seed, tier);
                for (int step = 0; step <= 3 && !g.Solved; step++)
                {
                    for (int bed = -2; bed <= g.Beds + 3; bed++)
                    {
                        if (bed >= 1 && bed <= g.Beds && g.IsRipe(bed)) continue;
                        int[] snap = Snapshot(g);
                        int moves = g.Moves;
                        bool inRange = bed >= 1 && bed <= g.Beds;
                        SowMove expected = !inRange ? SowMove.Ignored : g.Seeds(bed) == 0 ? SowMove.Empty : SowMove.NotRipe;
                        Expect(g.Sow(bed) == expected, "refusal reason t" + tier + " s" + seed + " bed " + bed);
                        Expect(g.Moves == moves, "refused move is not counted");
                        int[] after = Snapshot(g);
                        for (int i = 0; i < snap.Length; i++) Expect(snap[i] == after[i], "refused move changes nothing");
                    }
                    Expect(g.Sow(g.Hint()) == SowMove.Sowed, "hint move");
                }
            }
        var one = new SowingGame(7, 0);
        Expect(!one.IsRipe(0) && !one.IsRipe(-1) && !one.IsRipe(one.Beds + 1), "out-of-range beds are never ripe");
        Expect(one.Seeds(0) == 0 && one.Seeds(one.Beds + 1) == 0, "out-of-range beds hold nothing");
    }

    public static int Run()
    {
        assertions = 0;
        Expect(SowingGame.MinSeeds(0) == 6 && SowingGame.MaxSeeds(0) == 9, "tier 0 holds 6 to 9 seeds");
        Expect(SowingGame.MinSeeds(1) == 12 && SowingGame.MaxSeeds(1) == 16, "tier 1 holds 12 to 16 seeds");
        Expect(SowingGame.MinSeeds(2) == 20 && SowingGame.MaxSeeds(2) == 26, "tier 2 holds 20 to 26 seeds");
        bool threw = false;
        try { SowingGame.MinSeeds(3); } catch (ArgumentOutOfRangeException) { threw = true; }
        Expect(threw, "unknown tier refused");
        threw = false;
        try { new SowingGame(1, -1); } catch (ArgumentOutOfRangeException) { threw = true; }
        Expect(threw, "negative tier refused");

        SowingGame two = SowingGame.ForTotal(2);
        Expect(two.Beds == 2 && two.Seeds(1) == 0 && two.Seeds(2) == 2, "n=2 is bed 2 holding two");
        SowingGame three = SowingGame.ForTotal(3);
        Expect(three.Seeds(1) == 1 && three.Seeds(2) == 2, "n=3 is one and two");
        Expect(SowingGame.ForTotal(1).Beds == 1 && SowingGame.ForTotal(1).Seeds(1) == 1, "n=1 is bed 1 holding one");

        for (int tier = 0; tier < TreePuzzles.Tiers; tier++)
        {
            var seen = new HashSet<int>();
            for (int seed = 1; seed <= 200; seed++)
            {
                Expect(Key(new SowingGame(seed, tier)) == Key(new SowingGame(seed, tier)), "deterministic t" + tier + " s" + seed);
                seen.Add(new SowingGame(seed, tier).Total);
            }
            Expect(seen.Count == SowingGame.MaxSeeds(tier) - SowingGame.MinSeeds(tier) + 1, "every total in the tier turns up t" + tier);
            PlayTests(tier);
        }
        Expect(Key(new SowingGame(1, 0)) != Key(new SowingGame(1, 1)), "tiers differ");
        RefusalTests();
        UniquenessTests();
        return assertions;
    }
}
