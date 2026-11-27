using System;
using System.Collections.Generic;
using Evergrove.Core;

/// <summary>Doorstep pattern (idea 44): rebuild a mirror-symmetric threshold design from a few given cells.</summary>
internal static class DoorstepTests
{
    private static int assertions;

    private static void Expect(bool ok, string message)
    {
        assertions++;
        if (!ok) throw new Exception("Doorstep test failed: " + message);
    }

    private static string Key(DoorstepGame g)
    {
        var chars = new char[g.Size * g.Size];
        for (int c = 0; c < chars.Length; c++)
        {
            if (g.Solution(c)) chars[c] = g.IsGiven(c) ? 'O' : 'o';
            else chars[c] = g.IsGiven(c) ? ',' : '.';
        }
        return new string(chars);
    }

    private static int Quarter(int size, int cell)
    {
        int x = cell % size, y = cell / size;
        return (x * 2 >= size ? 1 : 0) + (y * 2 >= size ? 2 : 0);
    }

    private static void SetMark(DoorstepGame g, int cell, CellMark mark)
    {
        for (int i = 0; i < 3 && g.State(cell) != mark; i++) g.Tap(cell);
        Expect(g.State(cell) == mark, "tap cycle reaches every mark");
    }

    public static int Run()
    {
        assertions = 0;
        Expect(DoorstepGame.SizeFor(0) == 5 && DoorstepGame.SizeFor(1) == 7 && DoorstepGame.SizeFor(2) == 8, "5, 7, 8 by tier");
        bool threw = false;
        try { DoorstepGame.SizeFor(3); } catch (ArgumentOutOfRangeException) { threw = true; }
        Expect(threw, "unknown tier refused");
        Expect(new DoorstepGame(1, 0).Size == 5 && new DoorstepGame(1, 1).Size == 7 && new DoorstepGame(1, 2).Size == 8, "instance sizes");

        for (int tier = 0; tier < TreePuzzles.Tiers; tier++)
        {
            var keys = new HashSet<string>();
            for (int seed = 0; seed < 150; seed++)
            {
                var g = new DoorstepGame(seed, tier);
                int s = g.Size, n = s * s;
                Expect(Key(g) == Key(new DoorstepGame(seed, tier)), "same seed deals the same doorstep");
                keys.Add(Key(g));

                int drawn = 0;
                var quarters = new bool[4];
                for (int c = 0; c < n; c++)
                {
                    int x = c % s, y = c / s;
                    bool v = g.Solution(c);
                    if (v) drawn++;
                    Expect(g.Solution(y * s + (s - 1 - x)) == v, "left-right mirror");
                    Expect(g.Solution((s - 1 - y) * s + x) == v, "top-bottom mirror");
                    if (tier == 2)
                    {
                        Expect(g.Solution(x * s + y) == v, "main diagonal");
                        Expect(g.Solution((s - 1 - x) * s + (s - 1 - y)) == v, "anti diagonal");
                    }
                    if (g.IsGiven(c)) quarters[Quarter(s, c)] = true;
                    int[] orbit = g.Orbit(c);
                    Expect(Array.IndexOf(orbit, c) >= 0, "cell is in its own orbit");
                    int givens = 0;
                    foreach (int o in orbit)
                    {
                        Expect(g.Solution(o) == v, "orbit shares one value");
                        Expect(Array.IndexOf(g.Orbit(o), c) >= 0, "orbit is mutual");
                        if (g.IsGiven(o)) givens++;
                    }
                    Expect(givens >= 1, "every orbit has a given");
                    Expect(givens == 1, "exactly one given per orbit");
                    CellMark expected = !g.IsGiven(c) ? CellMark.Unknown : (v ? CellMark.Drawn : CellMark.Bare);
                    Expect(g.State(c) == expected, "start state shows only givens");
                }
                Expect(quarters[0] && quarters[1] && quarters[2] && quarters[3], "givens in all four quarters");
                double density = drawn / (double)n;
                Expect(density >= 0.35 && density <= 0.65, "density 35-65%: " + density);
                Expect(!g.Solved && g.Moves == 0, "starts unsolved, no moves");
            }
            Expect(keys.Count > 100, "seeds give varied doorsteps");
        }

        // Uniqueness: brute force every symmetric pattern (one bit per orbit).
        for (int tier = 0; tier < TreePuzzles.Tiers; tier++)
        {
            for (int seed = 0; seed < 25; seed++)
            {
                var g = new DoorstepGame(seed, tier);
                int n = g.Size * g.Size;
                var orbitIndex = new int[n];
                int orbitCount = 0;
                for (int c = 0; c < n; c++) orbitIndex[c] = -1;
                for (int c = 0; c < n; c++)
                {
                    if (orbitIndex[c] >= 0) continue;
                    foreach (int o in g.Orbit(c)) orbitIndex[o] = orbitCount;
                    orbitCount++;
                }
                Expect(orbitCount <= 20, "few orbits to enumerate");
                int matches = 0;
                for (int mask = 0; mask < (1 << orbitCount); mask++)
                {
                    bool ok = true, same = true;
                    for (int c = 0; c < n && ok; c++)
                    {
                        bool v = ((mask >> orbitIndex[c]) & 1) == 1;
                        if (g.IsGiven(c) && v != g.Solution(c)) ok = false;
                        if (v != g.Solution(c)) same = false;
                    }
                    if (!ok) continue;
                    matches++;
                    Expect(same, "the only pattern fitting the givens is the solution");
                }
                Expect(matches == 1, "exactly one symmetric pattern fits the givens");
            }
        }

        // Taps, givens, conflicts.
        {
            var g = new DoorstepGame(7, 0);
            int n = g.Size * g.Size;
            int given = -1, free = -1;
            for (int c = 0; c < n; c++)
            {
                if (g.IsGiven(c) && given < 0) given = c;
                if (!g.IsGiven(c) && free < 0) free = c;
            }
            CellMark before = g.State(given);
            Expect(g.Tap(given) == DoorMove.Given, "given cell reports Given");
            Expect(g.State(given) == before && g.Moves == 0, "given cells cannot be changed or counted");
            Expect(g.Tap(-1) == DoorMove.Ignored && g.Tap(n) == DoorMove.Ignored, "outside cells ignored");
            Expect(g.Moves == 0, "ignored taps do not count");
            Expect(g.State(free) == CellMark.Unknown, "free cell starts unknown");
            Expect(g.Tap(free) == DoorMove.Changed && g.State(free) == CellMark.Drawn, "unknown to drawn");
            Expect(g.Tap(free) == DoorMove.Changed && g.State(free) == CellMark.Bare, "drawn to bare");
            Expect(g.Tap(free) == DoorMove.Changed && g.State(free) == CellMark.Unknown, "bare to unknown");
            Expect(g.Moves == 3, "three changes counted");

            int partner = -1;
            foreach (int o in g.Orbit(given)) if (o != given) partner = o;
            Expect(partner >= 0, "the first given has orbit mates");
            Expect(!g.Conflicts(partner), "unknown never conflicts");
            SetMark(g, partner, g.Solution(given) ? CellMark.Bare : CellMark.Drawn);
            Expect(g.Conflicts(partner), "mark against a given in the orbit conflicts");
            Expect(!g.Conflicts(given), "a given never conflicts");
            SetMark(g, partner, g.Solution(given) ? CellMark.Drawn : CellMark.Bare);
            Expect(!g.Conflicts(partner), "matching mark does not conflict");
            Expect(!g.Conflicts(-1) && !g.Conflicts(n), "outside cell never conflicts");
        }

        // Hint-driven play solves from any state.
        for (int tier = 0; tier < TreePuzzles.Tiers; tier++)
        {
            for (int seed = 0; seed < 40; seed++)
            {
                var g = new DoorstepGame(seed, tier);
                int n = g.Size * g.Size;
                var rng = new Random(seed * 31 + tier);
                int scramble = rng.Next(0, 3 * n);
                for (int i = 0; i < scramble; i++) g.Tap(rng.Next(n));

                int guard = 0;
                while (!g.Solved)
                {
                    int cell; CellMark mark;
                    Expect(g.TryHint(out cell, out mark), "unsolved board always has a hint");
                    Expect(!g.IsGiven(cell), "hint never points at a given");
                    Expect(mark == (g.Solution(cell) ? CellMark.Drawn : CellMark.Bare), "hint names the true mark");
                    Expect(g.State(cell) != mark, "hint changes something");
                    SetMark(g, cell, mark);
                    Expect(++guard <= n * 2, "hints converge");
                }
                int c0; CellMark m0;
                Expect(!g.TryHint(out c0, out m0), "no hint once solved");
                for (int c = 0; c < n; c++)
                    Expect(g.State(c) != CellMark.Unknown && (g.State(c) == CellMark.Drawn) == g.Solution(c), "solved grid equals the pattern");
            }
        }

        // Hint prefers fixing a conflict over filling a blank.
        {
            var g = new DoorstepGame(3, 1);
            int n = g.Size * g.Size, given = -1;
            for (int c = 0; c < n && given < 0; c++) if (g.IsGiven(c) && g.Orbit(c).Length > 1) given = c;
            int victim = -1;
            foreach (int o in g.Orbit(given)) if (o != given) victim = o;
            SetMark(g, victim, g.Solution(given) ? CellMark.Bare : CellMark.Drawn);
            int cell; CellMark mark;
            Expect(g.TryHint(out cell, out mark) && cell == victim, "conflict hinted first");
            Expect(mark == (g.Solution(victim) ? CellMark.Drawn : CellMark.Bare), "conflict hint gives the true mark");
        }

        // Orbit sizes: centre is alone on odd boards; square symmetry has up to eight.
        {
            var g = new DoorstepGame(1, 0);
            Expect(g.Orbit(12).Length == 1, "centre cell is its own orbit");
            Expect(g.Orbit(0).Length == 4, "corner orbit has four");
            var h = new DoorstepGame(1, 2);
            Expect(h.Orbit(1).Length == 8, "square symmetry orbit of eight");
            Expect(h.Orbit(0).Length == 4, "square symmetry corner orbit of four");
            int[] copy = g.Orbit(0);
            copy[0] = 99;
            Expect(g.Orbit(0)[0] != 99, "Orbit returns a copy");
        }
        return assertions;
    }
}
