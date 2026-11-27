using System;
using System.Collections.Generic;
using Evergrove.Core;

/// <summary>StreamPath, the puzzle water meadows hold (depth spine, Layer 2): one stream through every field.</summary>
internal static class StreamPathTests
{
    private static int assertions;

    private static void Expect(bool ok, string message)
    {
        assertions++;
        if (!ok) throw new Exception("StreamPath test failed: " + message);
    }

    private static int CountOpen(StreamPathGame g)
    {
        int open = 0;
        for (int c = 0; c < g.Size * g.Size; c++) if (!g.IsRock(c)) open++;
        return open;
    }

    private static bool Adjacent(int size, int a, int b)
    {
        return Math.Abs(a / size - b / size) + Math.Abs(a % size - b % size) == 1;
    }

    // Independent exhaustive count of spring-to-pond Hamiltonian paths (plain DFS, capped at 2).
    private static int BruteCount(StreamPathGame g, int head, int depth, bool[] seen)
    {
        if (depth == g.OpenCount) return head == g.Pond ? 1 : 0;
        int total = 0, size = g.Size;
        int r = head / size, c = head % size;
        int[] next = { r > 0 ? head - size : -1, c > 0 ? head - 1 : -1, c < size - 1 ? head + 1 : -1, r < size - 1 ? head + size : -1 };
        foreach (int n in next)
        {
            if (n < 0 || g.IsRock(n) || seen[n]) continue;
            seen[n] = true;
            total += BruteCount(g, n, depth + 1, seen);
            seen[n] = false;
            if (total >= 2) return 2;
        }
        return total;
    }

    public static int Run()
    {
        Expect(StreamPathGame.SizeFor(0) == 4 && StreamPathGame.SizeFor(1) == 5 && StreamPathGame.SizeFor(2) == 6,
            "4, 5, 6 by tier");
        bool threw = false;
        try { StreamPathGame.SizeFor(3); } catch (ArgumentOutOfRangeException) { threw = true; }
        Expect(threw, "unknown tier refused");

        int[] minRocks = { 0, 1, 2 }, maxRocks = { 1, 3, 4 };
        var meadows = new HashSet<string>();
        var sw = System.Diagnostics.Stopwatch.StartNew();
        long genMs = 0, bruteMs = 0;
        for (int tier = 0; tier < TreePuzzles.Tiers; tier++)
            for (int seed = 0; seed < 150; seed++)
            {
                long t0 = sw.ElapsedMilliseconds;
                var g = new StreamPathGame(seed, tier);
                genMs += sw.ElapsedMilliseconds - t0;
                var again = seed % 5 == 0 ? new StreamPathGame(seed, tier) : g;
                int cells = g.Size * g.Size;
                int rocks = cells - g.OpenCount;
                Expect(g.Size == StreamPathGame.SizeFor(tier), "size per tier");
                Expect(rocks >= minRocks[tier] && rocks <= maxRocks[tier], "rock count per tier");
                Expect(CountOpen(g) == g.OpenCount, "OpenCount matches the rocks");
                Expect(!g.IsRock(g.Spring) && !g.IsRock(g.Pond) && g.Spring != g.Pond, "spring and pond open and apart");
                Expect(!g.IsRock(-1) && !g.IsRock(cells), "out of range is not a rock");

                int[] sol = g.SolutionPath();
                Expect(sol.Length == g.OpenCount && sol[0] == g.Spring && sol[sol.Length - 1] == g.Pond,
                    "solution runs spring to pond over every open field");
                var seen = new bool[cells];
                for (int i = 0; i < sol.Length; i++)
                {
                    Expect(!g.IsRock(sol[i]) && !seen[sol[i]], "solution visits each open field once");
                    seen[sol[i]] = true;
                    Expect(i == 0 || Adjacent(g.Size, sol[i - 1], sol[i]), "solution steps are orthogonal");
                    Expect(g.Solution(i) == sol[i], "Solution(index) agrees");
                }
                Expect(!ReferenceEquals(g.SolutionPath(), sol), "SolutionPath is a copy");

                var rockMap = new char[cells];
                for (int c = 0; c < cells; c++) rockMap[c] = g.IsRock(c) ? '#' : '.';
                Expect(again.Spring == g.Spring && again.Pond == g.Pond && again.OpenCount == g.OpenCount,
                    "same seed deals the same meadow");
                for (int c = 0; c < cells; c++) Expect(again.IsRock(c) == g.IsRock(c), "same rocks");
                for (int i = 0; i < sol.Length; i++) Expect(again.Solution(i) == sol[i], "same solution");
                meadows.Add(tier + ":" + new string(rockMap) + ":" + g.Spring + ":" + g.Pond);

                // Independent uniqueness check (plain DFS, no pruning) on the smaller meadows.
                if (tier < 2 || seed < 15)
                {
                    long t1 = sw.ElapsedMilliseconds;
                    var mark = new bool[cells];
                    mark[g.Spring] = true;
                    Expect(BruteCount(g, g.Spring, 1, mark) == 1, "exactly one way through (brute force)");
                    bruteMs += sw.ElapsedMilliseconds - t1;
                }
            }
        Expect(meadows.Count > 300, "meadows differ from seed to seed");
        Expect(genMs < 4000, "generation is fast");

        // Starting state.
        var play = new StreamPathGame(7, 1);
        Expect(play.Path.Length == 1 && play.Path[0] == play.Spring && play.Moves == 0 && !play.Solved,
            "stream starts at the spring");
        var copy = play.Path; copy[0] = -5;
        Expect(play.Path[0] == play.Spring, "Path is a copy");

        // Step outcomes.
        int size = play.Size;
        int rockCell = -1;
        for (int c = 0; c < size * size; c++) if (play.IsRock(c)) rockCell = c;
        Expect(play.Step(-1) == StreamPathGame.PathMove.Ignored, "negative cell ignored");
        Expect(play.Step(size * size) == StreamPathGame.PathMove.Ignored, "past the end ignored");
        Expect(play.Step(play.Spring) == StreamPathGame.PathMove.Ignored, "tapping the head ignored");
        int far = -1;
        for (int c = 0; c < size * size; c++)
            if (!play.IsRock(c) && !Adjacent(size, c, play.Spring) && c != play.Spring) { far = c; break; }
        Expect(play.Step(far) == StreamPathGame.PathMove.Ignored, "non-adjacent ignored");
        Expect(rockCell >= 0 && play.Step(rockCell) == StreamPathGame.PathMove.Ignored, "rock never takes the stream");
        bool rockBesideSpring = false;
        for (int seed = 0; !rockBesideSpring && seed < 200; seed++)
        {
            var g = new StreamPathGame(seed, 2);
            for (int c = 0; c < g.Size * g.Size; c++)
                if (g.IsRock(c) && Adjacent(g.Size, c, g.Spring))
                {
                    Expect(g.Step(c) == StreamPathGame.PathMove.Ignored && g.Moves == 0, "adjacent rock ignored");
                    rockBesideSpring = true;
                    break;
                }
        }
        Expect(rockBesideSpring, "found a rock beside a spring to test");
        Expect(play.Moves == 0, "ignored taps are not moves");

        int[] route = play.SolutionPath();
        Expect(play.Step(route[1]) == StreamPathGame.PathMove.Extended && play.Moves == 1, "extends one field");
        Expect(play.Step(route[1]) == StreamPathGame.PathMove.Ignored, "head again ignored");
        Expect(play.Step(route[2]) == StreamPathGame.PathMove.Extended, "extends again");
        Expect(play.Step(route[1]) == StreamPathGame.PathMove.Trimmed && play.Path.Length == 2 && play.Moves == 3,
            "tapping a field on the stream pulls it back");
        Expect(!play.OnPath(route[2]) && play.OnPath(route[1]), "trimmed fields are free again");
        Expect(play.Step(route[2]) == StreamPathGame.PathMove.Extended, "free field can be used again");
        Expect(play.Step(play.Spring) == StreamPathGame.PathMove.Trimmed && play.Path.Length == 1,
            "tapping the spring trims to just the spring");

        // TooSoon: the pond touches the stream before every field is watered.
        bool sawTooSoon = false;
        for (int tier = 0; tier < TreePuzzles.Tiers && !sawTooSoon; tier++)
            for (int seed = 0; seed < 200 && !sawTooSoon; seed++)
            {
                var g = new StreamPathGame(seed, tier);
                int[] s = g.SolutionPath();
                // Find a solution prefix whose head touches the pond before the last field.
                for (int i = 1; i < s.Length - 2; i++)
                {
                    g.Step(s[i]);
                    if (Adjacent(g.Size, s[i], g.Pond))
                    {
                        int before = g.Path.Length;
                        Expect(g.Step(g.Pond) == StreamPathGame.PathMove.TooSoon, "pond refuses an early stream");
                        Expect(g.Path.Length == before && !g.Solved && !g.OnPath(g.Pond), "TooSoon changes nothing");
                        sawTooSoon = true;
                        break;
                    }
                }
            }
        Expect(sawTooSoon, "exercised TooSoon");

        // Following the solution solves, for every tier.
        for (int tier = 0; tier < TreePuzzles.Tiers; tier++)
            for (int seed = 0; seed < 40; seed++)
            {
                var g = new StreamPathGame(seed, tier);
                int[] s = g.SolutionPath();
                for (int i = 1; i < s.Length; i++)
                {
                    Expect(!g.Solved, "not solved before the last field");
                    Expect(g.Step(s[i]) == StreamPathGame.PathMove.Extended, "solution steps extend");
                }
                Expect(g.Solved && g.Moves == s.Length - 1, "solution solves in OpenCount-1 moves");
                Expect(g.NextHint() == -1, "no hint once solved");
                Expect(g.Step(s[1]) == StreamPathGame.PathMove.Ignored, "solved stream takes no more taps");
            }

        // A wrong stream never reports Solved: random tapping may only solve with the unique stream.
        uint rng = 12345;
        int wrongTried = 0;
        for (int seed = 0; seed < 60; seed++)
        {
            var g = new StreamPathGame(seed, 2);
            for (int step = 0; step < 400 && !g.Solved; step++)
            {
                rng ^= rng << 13; rng ^= rng >> 17; rng ^= rng << 5;
                g.Step((int)(rng % (uint)(g.Size * g.Size)));
                int[] p = g.Path;
                bool correct = true;
                for (int i = 0; i < p.Length; i++) if (p[i] != g.Solution(i)) correct = false;
                if (!correct) wrongTried++;
                if (g.Solved)
                {
                    Expect(p.Length == g.OpenCount && p[p.Length - 1] == g.Pond && correct,
                        "the only solved stream is the unique one");
                }
                else if (!correct)
                {
                    Expect(!g.Solved, "a wrong stream is never solved");
                }
            }
        }
        Expect(wrongTried > 0, "wrong streams were tried");
        // One short of the pond is not solved.
        var dead = new StreamPathGame(3, 0);
        int[] ds = dead.SolutionPath();
        for (int i = 1; i < ds.Length - 1; i++) dead.Step(ds[i]);
        Expect(!dead.Solved && dead.Path.Length == ds.Length - 1, "one short of the pond is not solved");

        // NextHint: from empty, then from a wrong prefix, following it solves.
        for (int tier = 0; tier < TreePuzzles.Tiers; tier++)
            for (int seed = 0; seed < 25; seed++)
            {
                var g = new StreamPathGame(seed, tier);
                Expect(g.NextHint() == g.Solution(1), "hint from the bare spring is the first solution step");
                uint r2 = unchecked((uint)seed * 2654435761u + 77u);
                for (int wander = 0; wander < 12; wander++)
                {
                    int head = g.Path[g.Path.Length - 1];
                    for (int tries = 0; tries < 20; tries++)
                    {
                        r2 = r2 * 1664525u + 1013904223u;
                        int cell = (int)((r2 >> 8) % (uint)(g.Size * g.Size));
                        if (g.IsRock(cell) || g.OnPath(cell) || !Adjacent(g.Size, head, cell)) continue;
                        g.Step(cell);
                        break;
                    }
                }
                int guard = 0;
                while (!g.Solved && guard++ < 500)
                {
                    int hint = g.NextHint();
                    Expect(hint >= 0 && hint < g.Size * g.Size && !g.IsRock(hint), "hint is an open field");
                    var result = g.Step(hint);
                    Expect(result == StreamPathGame.PathMove.Extended || result == StreamPathGame.PathMove.Trimmed,
                        "hint is always an acceptable move");
                }
                Expect(g.Solved, "following hints solves from a wrong prefix");
            }

        // Documented stray hint: the last correct field.
        bool strayChecked = false;
        for (int seed = 0; seed < 100 && !strayChecked; seed++)
        {
            var g = new StreamPathGame(seed, 1);
            int[] s = g.SolutionPath();
            g.Step(s[1]);
            for (int c = 0; c < g.Size * g.Size; c++)
                if (!g.IsRock(c) && Adjacent(g.Size, s[1], c) && c != s[2] && c != s[0])
                {
                    Expect(g.Step(c) == StreamPathGame.PathMove.Extended, "stray step taken");
                    Expect(g.NextHint() == s[1], "stray stream hints the last correct field to trim back to");
                    strayChecked = true;
                    break;
                }
        }
        Expect(strayChecked, "exercised the stray hint");

        // Moves counts extends and trims, not ignored taps.
        var counted = new StreamPathGame(2, 0);
        int[] cs = counted.SolutionPath();
        counted.Step(cs[1]); counted.Step(cs[2]); counted.Step(cs[1]); counted.Step(-3);
        Expect(counted.Moves == 3, "extend, extend, trim = 3 moves; ignored tap uncounted");
        return assertions;
    }
}
