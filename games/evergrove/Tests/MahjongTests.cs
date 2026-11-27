using System;
using System.Collections.Generic;
using Evergrove.Core;

/// <summary>Mahjong, garden tiles: a gentle mahjong-solitaire.</summary>
internal static class MahjongTests
{
    private static int assertions;

    private static void Expect(bool ok, string message)
    {
        assertions++;
        if (!ok) throw new Exception("Mahjong test failed: " + message);
    }

    // Independent statement of the free rule, from positions alone.
    private static bool FreeByRule(MahjongGame g, int t)
    {
        if (g.Removed(t)) return false;
        bool left = false, right = false;
        for (int j = 0; j < g.TileCount; j++)
        {
            if (j == t || g.Removed(j)) continue;
            int dx = g.X(j) - g.X(t), dy = g.Y(j) - g.Y(t);
            if (Math.Abs(dy) >= 2) continue;
            if (g.Z(j) > g.Z(t) && Math.Abs(dx) < 2) return false;
            if (g.Z(j) == g.Z(t) && dx == -2) left = true;
            if (g.Z(j) == g.Z(t) && dx == 2) right = true;
        }
        return !(left && right);
    }

    private static ulong RemovedMask(MahjongGame g)
    {
        ulong mask = 0;
        for (int i = 0; i < g.TileCount; i++)
            if (g.Removed(i)) mask |= 1UL << i;
        return mask;
    }

    // Depth-first solver over removed-sets, with memo of dead ends.
    private static bool Solvable(MahjongGame g)
    {
        int n = g.TileCount;
        var above = new ulong[n]; var left = new ulong[n]; var right = new ulong[n];
        for (int i = 0; i < n; i++)
            for (int j = 0; j < n; j++)
            {
                if (i == j) continue;
                int dx = g.X(j) - g.X(i), dy = g.Y(j) - g.Y(i);
                if (Math.Abs(dy) >= 2) continue;
                if (g.Z(j) > g.Z(i) && Math.Abs(dx) < 2) above[i] |= 1UL << j;
                if (g.Z(j) == g.Z(i) && dx == -2) left[i] |= 1UL << j;
                if (g.Z(j) == g.Z(i) && dx == 2) right[i] |= 1UL << j;
            }
        ulong full = n == 64 ? ulong.MaxValue : (1UL << n) - 1;
        var dead = new HashSet<ulong>();
        return Dfs(g, RemovedMask(g), full, above, left, right, dead);
    }

    private static bool Dfs(MahjongGame g, ulong gone, ulong full, ulong[] above, ulong[] left, ulong[] right, HashSet<ulong> dead)
    {
        if (gone == full) return true;
        if (dead.Contains(gone)) return false;
        int n = above.Length;
        var free = new List<int>();
        for (int i = 0; i < n; i++)
        {
            if ((gone >> i & 1) != 0) continue;
            if ((above[i] & ~gone) != 0) continue;
            if ((left[i] & ~gone) != 0 && (right[i] & ~gone) != 0) continue;
            free.Add(i);
        }
        for (int a = 0; a < free.Count; a++)
            for (int b = a + 1; b < free.Count; b++)
                if (g.Face(free[a]) == g.Face(free[b]) &&
                    Dfs(g, gone | 1UL << free[a] | 1UL << free[b], full, above, left, right, dead))
                    return true;
        dead.Add(gone);
        return false;
    }

    private static string Signature(MahjongGame g)
    {
        var parts = new List<string>();
        for (int i = 0; i < g.TileCount; i++) parts.Add(g.Face(i).ToString());
        return string.Join(",", parts);
    }

    private static uint rng = 12345;
    private static int Roll(int bound)
    {
        rng ^= rng << 13; rng ^= rng >> 17; rng ^= rng << 5;
        return (int)(rng % (uint)bound);
    }

    private static List<int[]> FreePairs(MahjongGame g)
    {
        var pairs = new List<int[]>();
        for (int a = 0; a < g.TileCount; a++)
            for (int b = a + 1; b < g.TileCount; b++)
                if (g.IsFree(a) && g.IsFree(b) && g.Face(a) == g.Face(b)) pairs.Add(new[] { a, b });
        return pairs;
    }

    private static void Take(MahjongGame g, int[] pair)
    {
        g.Select(pair[0]);
        g.Select(pair[1]);
    }

    public static int Run()
    {
        assertions = 0;
        int[] tiles = { 16, 24, 36 }, faces = { 6, 9, 12 };

        bool threw = false;
        try { new MahjongGame(1, 3); } catch (ArgumentOutOfRangeException) { threw = true; }
        Expect(threw, "unknown tier refused");

        // Counts, faces, layout sanity.
        for (int tier = 0; tier < TreePuzzles.Tiers; tier++)
        {
            Expect(MahjongGame.TilesFor(tier) == tiles[tier], "tile count tier " + tier);
            Expect(MahjongGame.FacesFor(tier) == faces[tier], "face count tier " + tier);
            for (int seed = 0; seed < 40; seed++)
            {
                var g = new MahjongGame(seed, tier);
                Expect(g.TileCount == tiles[tier] && g.Remaining == tiles[tier], "fresh table is full");
                Expect(!g.Solved && g.Moves == 0 && g.Reshuffles == 0 && g.Selected == -1, "fresh counters");
                var count = new int[faces[tier]];
                for (int i = 0; i < g.TileCount; i++)
                {
                    Expect(g.Face(i) >= 0 && g.Face(i) < faces[tier], "face in range");
                    count[g.Face(i)]++;
                    Expect(g.Z(i) >= 0 && g.Z(i) <= 2 && !g.Removed(i), "layer 0..2");
                    if (tier < 2) Expect(g.Z(i) <= 1 || tier == 1, "tier 0 has two layers");
                }
                foreach (int c in count) Expect(c > 0 && c % 2 == 0, "every face used an even number of times");
            }
            var layout = new MahjongGame(0, tier);
            int maxZ = 0;
            for (int i = 0; i < layout.TileCount; i++)
            {
                maxZ = Math.Max(maxZ, layout.Z(i));
                for (int j = i + 1; j < layout.TileCount; j++)
                    Expect(layout.Z(i) != layout.Z(j) || Math.Abs(layout.X(i) - layout.X(j)) >= 2 ||
                           Math.Abs(layout.Y(i) - layout.Y(j)) >= 2, "no two tiles overlap on a layer");
                // never fully hidden in the 2D projection
                var covered = new HashSet<int>();
                for (int j = 0; j < layout.TileCount; j++)
                    if (layout.Z(j) > layout.Z(i))
                        for (int a = 0; a < 2; a++)
                            for (int b = 0; b < 2; b++) covered.Add((layout.X(j) + a) * 1000 + layout.Y(j) + b);
                bool visible = false;
                for (int a = 0; a < 2; a++)
                    for (int b = 0; b < 2; b++)
                        if (!covered.Contains((layout.X(i) + a) * 1000 + layout.Y(i) + b)) visible = true;
                Expect(visible, "tile " + i + " of tier " + tier + " stays partly visible");
            }
            Expect(maxZ == (tier == 0 ? 1 : tier == 1 ? 2 : 2), "layer count tier " + tier);
        }

        // Determinism and variety.
        for (int tier = 0; tier < TreePuzzles.Tiers; tier++)
        {
            var seen = new HashSet<string>();
            for (int seed = 0; seed < 50; seed++)
            {
                var a = new MahjongGame(seed, tier);
                var b = new MahjongGame(seed, tier);
                Expect(Signature(a) == Signature(b), "same seed deals the same table");
                seen.Add(Signature(a));
            }
            Expect(seen.Count >= 45, "different seeds deal different tables, tier " + tier);
        }

        // Every deal is solvable.
        for (int tier = 0; tier < TreePuzzles.Tiers; tier++)
            for (int seed = 0; seed < 300; seed++)
                Expect(Solvable(new MahjongGame(seed, tier)), "deal solvable: seed " + seed + " tier " + tier);

        // The free rule, against an independent statement of it, through whole games.
        int byAbove = 0, byBothSides = 0, oneSide = 0;
        for (int tier = 0; tier < TreePuzzles.Tiers; tier++)
            for (int seed = 0; seed < 20; seed++)
            {
                var g = new MahjongGame(seed, tier);
                while (!g.Solved)
                {
                    for (int t = 0; t < g.TileCount; t++)
                    {
                        Expect(g.IsFree(t) == FreeByRule(g, t), "IsFree matches the rule");
                        if (g.Removed(t) || g.IsFree(t)) continue;
                        bool covered = false, l = false, r = false;
                        for (int j = 0; j < g.TileCount; j++)
                        {
                            if (j == t || g.Removed(j) || Math.Abs(g.Y(j) - g.Y(t)) >= 2) continue;
                            int dx = g.X(j) - g.X(t);
                            if (g.Z(j) > g.Z(t) && Math.Abs(dx) < 2) covered = true;
                            if (g.Z(j) == g.Z(t) && dx == -2) l = true;
                            if (g.Z(j) == g.Z(t) && dx == 2) r = true;
                        }
                        if (covered) byAbove++;
                        else if (l && r) byBothSides++;
                    }
                    for (int t = 0; t < g.TileCount; t++)
                        if (g.IsFree(t))
                        {
                            bool l = false, r = false;
                            for (int j = 0; j < g.TileCount; j++)
                            {
                                if (j == t || g.Removed(j) || g.Z(j) != g.Z(t) || Math.Abs(g.Y(j) - g.Y(t)) >= 2) continue;
                                if (g.X(j) - g.X(t) == -2) l = true;
                                if (g.X(j) - g.X(t) == 2) r = true;
                            }
                            if (l != r) oneSide++;
                        }
                    int[] hint = g.HintPair();
                    if (hint.Length == 0) { Expect(g.Reshuffle(), "stuck table reshuffles"); continue; }
                    Take(g, hint);
                }
            }
        Expect(byAbove > 0, "tiles blocked from above occur");
        Expect(byBothSides > 0, "tiles blocked on both sides occur");
        Expect(oneSide > 0, "tiles open on exactly one side occur");
        Expect(!new MahjongGame(0, 0).IsFree(-1) && !new MahjongGame(0, 0).IsFree(99), "out of range is not free");

        // Select outcomes.
        {
            var g = new MahjongGame(0, 0);
            Expect(g.Select(-1) == MahjongMove.Ignored && g.Select(99) == MahjongMove.Ignored, "out of range ignored");
            int blocked = -1, a = -1, b = -1;
            for (int t = 0; t < g.TileCount; t++)
                if (!g.IsFree(t)) { blocked = t; break; }
            Expect(blocked >= 0, "a fresh table has blocked tiles");
            Expect(g.Select(blocked) == MahjongMove.NotFree && g.Selected == -1, "blocked tile: NotFree, nothing changes");
            for (int t = 0; t < g.TileCount; t++)
                if (g.IsFree(t))
                {
                    if (a < 0) a = t;
                    else if (g.Face(t) != g.Face(a)) { b = t; break; }
                }
            Expect(a >= 0 && b >= 0, "two free tiles of different faces exist");
            Expect(g.Select(a) == MahjongMove.Picked && g.Selected == a, "first tile picked");
            Expect(g.Select(blocked) == MahjongMove.NotFree && g.Selected == a, "NotFree keeps the pick");
            Expect(g.Select(a) == MahjongMove.Unpicked && g.Selected == -1, "tap again to unpick");
            g.Select(a);
            Expect(g.Select(b) == MahjongMove.Swapped && g.Selected == b, "another face moves the pick");
            Expect(g.Moves == 0 && g.Remaining == 16, "swap costs nothing");
            Expect(g.Select(b) == MahjongMove.Unpicked, "unpick after swap");
            int[] pair = g.HintPair();
            Expect(pair.Length == 2 && g.Face(pair[0]) == g.Face(pair[1]) && g.IsFree(pair[0]) && g.IsFree(pair[1]),
                "hint is a free matching pair");
            Expect(g.Select(pair[0]) == MahjongMove.Picked, "pair first picked");
            Expect(g.Select(pair[1]) == MahjongMove.Matched, "pair matched");
            Expect(g.Removed(pair[0]) && g.Removed(pair[1]) && g.Remaining == 14 && g.Moves == 1 && g.Selected == -1,
                "matched pair removed, move counted");
            Expect(g.Select(pair[0]) == MahjongMove.Ignored, "removed tile ignored");
        }

        // Hints alone clear the table (reshuffling if ever stuck); Moves and Remaining agree.
        for (int tier = 0; tier < TreePuzzles.Tiers; tier++)
            for (int seed = 0; seed < 60; seed++)
            {
                var g = new MahjongGame(seed, tier);
                int guard = 0;
                while (!g.Solved && guard++ < 200)
                {
                    int[] hint = g.HintPair();
                    Expect(g.HasMove == (hint.Length == 2), "HasMove agrees with HintPair");
                    if (hint.Length == 0)
                    {
                        Expect(g.Reshuffle() && g.HasMove, "reshuffle revives a stuck table");
                        continue;
                    }
                    Expect(g.Select(hint[0]) == MahjongMove.Picked && g.Select(hint[1]) == MahjongMove.Matched, "hint plays");
                    Expect(g.Remaining + g.Moves * 2 == g.TileCount, "remaining and moves agree");
                }
                Expect(g.Solved && g.Remaining == 0 && g.Moves == g.TileCount / 2, "table cleared by hints");
                Expect(g.HintPair().Length == 0 && !g.HasMove && !g.Reshuffle(), "solved: no hint, no reshuffle");
                Expect(g.Select(0) == MahjongMove.Ignored, "solved table ignores taps");
            }

        // Random play never reaches a table that no re-deal can clear; stuck tables reshuffle.
        int stuckSeen = 0;
        for (int tier = 0; tier < TreePuzzles.Tiers; tier++)
            for (int seed = 0; seed < 400; seed++)
            {
                var g = new MahjongGame(seed, tier);
                while (!g.Solved)
                {
                    var pairs = FreePairs(g);
                    if (pairs.Count == 0)
                    {
                        stuckSeen++;
                        var before = new List<int>();
                        for (int i = 0; i < g.TileCount; i++) if (!g.Removed(i)) before.Add(g.Face(i));
                        before.Sort();
                        int remaining = g.Remaining, moves = g.Moves;
                        g.Select(Roll(g.TileCount));
                        Expect(!g.HasMove, "stuck means no free matching pair");
                        Expect(g.Reshuffle(), "stuck table reshuffles");
                        Expect(g.Selected == -1 && g.Reshuffles >= 1, "reshuffle clears the pick and counts");
                        Expect(g.Remaining == remaining && g.Moves == moves, "reshuffle keeps progress");
                        var after = new List<int>();
                        for (int i = 0; i < g.TileCount; i++) if (!g.Removed(i)) after.Add(g.Face(i));
                        after.Sort();
                        Expect(before.Count == after.Count && string.Join(",", before) == string.Join(",", after),
                            "reshuffle keeps the same faces");
                        Expect(g.HasMove && Solvable(g), "reshuffled table is solvable");
                        continue;
                    }
                    var pick = pairs[Roll(pairs.Count)];
                    Expect(!g.Reshuffle() || false, "no reshuffle while a pair is free");
                    Take(g, pick);
                }
                Expect(g.Solved, "random play finishes");
            }
        Expect(stuckSeen > 0, "random play does get stuck sometimes (the reshuffle path ran)");

        return assertions;
    }
}
