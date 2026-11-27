using System;
using System.Collections.Generic;
using Evergrove.Core;

/// <summary>Nonogram, planting by numbers (depth spine, Layer 2): logic-only picture puzzles.</summary>
internal static class NonogramTests
{
    private static int assertions;

    private static void Expect(bool ok, string message)
    {
        assertions++;
        if (!ok) throw new Exception("Nonogram test failed: " + message);
    }

    private static int[] RunsOf(NonogramGame g, int idx, bool row)
    {
        var runs = new List<int>();
        int run = 0;
        for (int k = 0; k <= g.Size; k++)
        {
            bool on = k < g.Size && g.Solution(row ? idx * g.Size + k : k * g.Size + idx);
            if (on) run++;
            else if (run > 0) { runs.Add(run); run = 0; }
        }
        return runs.ToArray();
    }

    private static bool Same(int[] a, int[] b)
    {
        if (a.Length != b.Length) return false;
        for (int i = 0; i < a.Length; i++) if (a[i] != b[i]) return false;
        return true;
    }

    private static string Key(NonogramGame g)
    {
        var parts = new List<string>();
        foreach (var c in g.RowClues) parts.Add(string.Join(".", c));
        parts.Add("|");
        foreach (var c in g.ColumnClues) parts.Add(string.Join(".", c));
        return string.Join(",", parts);
    }

    public static int Run()
    {
        assertions = 0;
        Expect(NonogramGame.SizeFor(0) == 5 && NonogramGame.SizeFor(1) == 6 && NonogramGame.SizeFor(2) == 8,
            "5, 6, 8 by tier");
        bool threw = false;
        try { NonogramGame.SizeFor(3); } catch (ArgumentOutOfRangeException) { threw = true; }
        Expect(threw, "unknown tier refused");

        var keys = new HashSet<string>();
        for (int tier = 0; tier < TreePuzzles.Tiers; tier++)
            for (int seed = 0; seed < 300; seed++)
            {
                var g = new NonogramGame(seed, tier);
                var again = new NonogramGame(seed, tier);
                int n = g.Size;
                Expect(n == NonogramGame.SizeFor(tier), "size per tier");
                Expect(g.RowClues.Length == n && g.ColumnClues.Length == n, "a clue per line");
                Expect(Key(g) == Key(again), "same seed and tier give the same clues");
                int filled = 0;
                for (int i = 0; i < n * n; i++)
                {
                    Expect(g.Solution(i) == again.Solution(i), "same picture");
                    if (g.Solution(i)) filled++;
                }
                Expect(filled > 0 && filled < n * n, "a picture, not blank or full");
                for (int i = 0; i < n; i++)
                {
                    Expect(Same(g.RowClues[i], RunsOf(g, i, true)), "row clue matches the picture");
                    Expect(Same(g.ColumnClues[i], RunsOf(g, i, false)), "column clue matches the picture");
                    Expect(g.RowClues[i].Length > 0 && g.ColumnClues[i].Length > 0, "no empty line");
                }
                Expect(g.Attempts >= 1 && g.Attempts < 5000, "generation terminates quickly");
                // Logic-solvable: following hints from a blank bed finishes it with no guessing.
                for (int step = 0; step < n * n * 2 && !g.Solved; step++)
                {
                    int h = g.HintCell();
                    Expect(h >= 0, "a deducible cell while unsolved");
                    g.Set(h, g.Solution(h) ? CellState.Filled : CellState.Marked);
                }
                Expect(g.Solved, "line-solving alone completes the bed");
                if (seed < 40) keys.Add(tier + ":" + Key(again));
            }
        // Beds now come from a hand-drawn picture library (each picture plain or mirrored),
        // so variety is bounded by the library, not by the seed space: the first 2 * count
        // seeds of a tier must show at least one distinct bed per picture.
        for (int tier = 0; tier < TreePuzzles.Tiers; tier++)
        {
            var seen = new HashSet<string>();
            for (int seed = 0; seed < 2 * NonogramPictures.Count(tier); seed++)
                seen.Add(Key(new NonogramGame(seed, tier)));
            Expect(seen.Count >= NonogramPictures.Count(tier), "beds differ from seed to seed");
        }
        Expect(keys.Count >= 3 * 8, "every tier deals several different beds");

        // Tapping cycles Empty -> Filled -> Marked -> Empty.
        var play = new NonogramGame(3, 0);
        Expect(play.State(0) == CellState.Empty && !play.Solved && play.Moves == 0, "starts bare");
        Expect(play.Tap(0) == NonogramMove.Filled && play.State(0) == CellState.Filled, "first tap plants");
        Expect(play.Tap(0) == NonogramMove.Marked && play.State(0) == CellState.Marked, "second tap marks");
        Expect(play.Tap(0) == NonogramMove.Cleared && play.State(0) == CellState.Empty, "third tap clears");
        Expect(play.Moves == 3, "each change counts a move");
        Expect(play.Tap(-1) == NonogramMove.Ignored && play.Tap(25) == NonogramMove.Ignored, "out of range ignored");
        Expect(play.Set(0, CellState.Empty) == NonogramMove.Ignored && play.Moves == 3, "no-op set ignored");
        Expect(play.State(-4) == CellState.Empty && play.State(99) == CellState.Empty, "state out of range is empty");
        Expect(!play.RowDone(-1) && !play.ColumnDone(5), "done out of range is false");

        for (int tier = 0; tier < TreePuzzles.Tiers; tier++)
        {
            var g = new NonogramGame(11, tier);
            int n = g.Size;
            // Marks alone never solve.
            for (int i = 0; i < n * n; i++) if (!g.Solution(i)) g.Set(i, CellState.Marked);
            Expect(!g.Solved, "marks alone do not solve");
            int last = -1;
            for (int i = 0; i < n * n; i++) if (g.Solution(i)) last = i;
            for (int i = 0; i < n * n; i++)
            {
                if (!g.Solution(i) || i == last) continue;
                Expect(!g.Solved, "unsolved until the last plant");
                g.Set(i, CellState.Filled);
            }
            Expect(!g.Solved, "one plant short is unsolved");
            g.Set(last, CellState.Filled);
            Expect(g.Solved, "exactly the solution filled solves it");
            int moves = g.Moves;
            Expect(g.Tap(last) == NonogramMove.Ignored && g.State(last) == CellState.Filled, "taps ignored after solved");
            Expect(g.Set(0, CellState.Marked) == NonogramMove.Ignored && g.Moves == moves, "set ignored after solved");
            for (int i = 0; i < n; i++) Expect(g.RowDone(i) && g.ColumnDone(i), "all lines done when solved");

            // Marks do not matter: solve with no marks at all.
            var bare = new NonogramGame(11, tier);
            for (int i = 0; i < n * n; i++) if (bare.Solution(i)) bare.Set(i, CellState.Filled);
            Expect(bare.Solved, "no marks needed");

            // A wrong extra fill keeps it unsolved until cleared.
            int wrong = -1;
            for (int i = 0; i < n * n; i++) if (!bare.Solution(i)) { wrong = i; break; }
            var extra = new NonogramGame(11, tier);
            extra.Set(wrong, CellState.Filled);
            for (int i = 0; i < n * n; i++) if (extra.Solution(i)) extra.Set(i, CellState.Filled);
            Expect(!extra.Solved, "an extra fill keeps it unsolved");
            Expect(extra.HintCell() == wrong, "the hint finds the stray plant");
            extra.Set(wrong, CellState.Marked);
            Expect(extra.Solved, "marking the stray cell mends it");
        }

        // RowDone / ColumnDone follow the clue.
        var rd = new NonogramGame(7, 0);
        Expect(!rd.RowDone(0), "row not done when bare");
        for (int k = 0; k < rd.Size; k++) if (rd.Solution(k)) rd.Set(k, CellState.Filled);
        Expect(rd.RowDone(0), "row done when its plants match the clue");
        int gap = -1;
        for (int k = 0; k < rd.Size; k++) if (!rd.Solution(k)) { gap = k; break; }
        if (gap >= 0)
        {
            rd.Set(gap, CellState.Marked);
            Expect(rd.RowDone(0), "marks do not spoil a done row");
            rd.Set(gap, CellState.Filled);
            Expect(!rd.RowDone(0), "an extra plant undoes the row");
        }
        var cd = new NonogramGame(7, 0);
        for (int r = 0; r < cd.Size; r++) if (cd.Solution(r * cd.Size + 2)) cd.Set(r * cd.Size + 2, CellState.Filled);
        Expect(cd.ColumnDone(2), "column done when its plants match the clue");

        var hint = new NonogramGame(21, 2);
        int hc = hint.HintCell();
        Expect(hc >= 0 && hc < hint.Size * hint.Size, "a hint on a blank bed");
        PictureTests();
        return assertions;
    }

    // ---- the picture library ----

    private static bool[,] Grid(string[] rows)
    {
        var g = new bool[rows.Length, rows[0].Length];
        for (int r = 0; r < rows.Length; r++)
            for (int c = 0; c < rows[r].Length; c++) g[r, c] = rows[r][c] == '#';
        return g;
    }

    private static int[] ClueOf(bool[,] g, int idx, bool row)
    {
        var runs = new List<int>();
        int n = g.GetLength(0), run = 0;
        for (int k = 0; k <= n; k++)
        {
            bool on = k < n && (row ? g[idx, k] : g[k, idx]);
            if (on) run++;
            else if (run > 0) { runs.Add(run); run = 0; }
        }
        return runs.ToArray();
    }

    // Every line of length n (true = plant) whose runs equal the clue.
    private static List<bool[]> AllLines(int[] clue, int n)
    {
        var result = new List<bool[]>();
        for (int mask = 0; mask < (1 << n); mask++)
        {
            var line = new bool[n];
            var runs = new List<int>();
            int run = 0;
            for (int k = 0; k < n; k++) line[k] = (mask >> k & 1) == 1;
            for (int k = 0; k <= n; k++)
            {
                if (k < n && line[k]) run++;
                else if (run > 0) { runs.Add(run); run = 0; }
            }
            if (Same(runs.ToArray(), clue)) result.Add(line);
        }
        return result;
    }

    // Independent line solver for the tests: row and column deductions only, no guessing.
    // Returns how many cells are still undecided when no line can say more (0 = solved by
    // logic); matches reports that every decided cell agrees with the picture.
    private static int LineSolveUndecided(bool[,] pic, out bool matches)
    {
        int n = pic.GetLength(0);
        var known = new int[n, n]; // 0 unknown, 1 plant, 2 bare
        var opts = new List<bool[]>[2 * n];
        for (int i = 0; i < n; i++)
        {
            opts[i] = AllLines(ClueOf(pic, i, true), n);
            opts[n + i] = AllLines(ClueOf(pic, i, false), n);
        }
        bool changed = true;
        while (changed)
        {
            changed = false;
            for (int pass = 0; pass < 2 * n; pass++)
            {
                bool isRow = pass < n;
                int idx = isRow ? pass : pass - n;
                var yes = new bool[n];
                var no = new bool[n];
                foreach (var o in opts[pass])
                {
                    bool fits = true;
                    for (int k = 0; k < n && fits; k++)
                    {
                        int v = isRow ? known[idx, k] : known[k, idx];
                        if ((v == 1 && !o[k]) || (v == 2 && o[k])) fits = false;
                    }
                    if (!fits) continue;
                    for (int k = 0; k < n; k++) { if (o[k]) yes[k] = true; else no[k] = true; }
                }
                for (int k = 0; k < n; k++)
                {
                    int r = isRow ? idx : k, c = isRow ? k : idx;
                    if (known[r, c] != 0 || (yes[k] && no[k])) continue;
                    known[r, c] = yes[k] ? 1 : 2;
                    changed = true;
                }
            }
        }
        int undecided = 0;
        matches = true;
        for (int r = 0; r < n; r++)
            for (int c = 0; c < n; c++)
            {
                if (known[r, c] == 0) undecided++;
                else if ((known[r, c] == 1) != pic[r, c]) matches = false;
            }
        return undecided;
    }

    private static string[] MirroredRows(string[] rows, bool mirror)
    {
        if (!mirror) return rows;
        var result = new string[rows.Length];
        for (int i = 0; i < rows.Length; i++)
        {
            var chars = rows[i].ToCharArray();
            Array.Reverse(chars);
            result[i] = new string(chars);
        }
        return result;
    }

    private static void PictureTests()
    {
        var names = new HashSet<string>();
        for (int tier = 0; tier < TreePuzzles.Tiers; tier++)
        {
            int n = NonogramGame.SizeFor(tier);
            int count = NonogramPictures.Count(tier);
            Expect(count >= 8, "at least eight pictures in tier " + tier);
            var shapes = new HashSet<string>();
            for (int i = 0; i < count; i++)
            {
                string name = NonogramPictures.Name(tier, i);
                string[] rows = NonogramPictures.Rows(tier, i);
                Expect(!string.IsNullOrEmpty(name) && name.StartsWith("a"), "short English name: " + name);
                names.Add(name);
                Expect(rows.Length == n, name + " has " + n + " rows");
                var grid = Grid(rows);
                int filled = 0;
                for (int r = 0; r < n; r++)
                {
                    Expect(rows[r].Length == n, name + " row width");
                    for (int c = 0; c < n; c++)
                    {
                        Expect(rows[r][c] == '#' || rows[r][c] == '.', name + " uses only # and .");
                        if (grid[r, c]) filled++;
                    }
                    Expect(ClueOf(grid, r, true).Length > 0 && ClueOf(grid, r, false).Length > 0,
                        name + " has no empty row or column");
                }
                Expect(filled >= n * 2 && filled <= n * n * 17 / 20, name + " is neither sparse nor a block");
                Expect(shapes.Add(string.Join("/", rows)), name + " is not a duplicate picture");
                bool matches;
                Expect(LineSolveUndecided(grid, out matches) == 0 && matches,
                    name + " (tier " + tier + ") is solved by row and column logic alone");
            }
            bool threw = false;
            try { NonogramPictures.Rows(tier, count); } catch (ArgumentOutOfRangeException) { threw = true; }
            Expect(threw, "picture index out of range refused");
        }
        Expect(names.Count >= 12, "a variety of garden subjects across tiers");
        bool badTier = false;
        try { NonogramPictures.Count(3); } catch (ArgumentOutOfRangeException) { badTier = true; }
        Expect(badTier, "unknown tier refused by the library");

        // The constructor picks a picture from the tier's library from the seed, evenly,
        // plain or mirrored; every picture and both orientations come up.
        for (int tier = 0; tier < TreePuzzles.Tiers; tier++)
        {
            int count = NonogramPictures.Count(tier), n = NonogramGame.SizeFor(tier);
            var perPicture = new int[count];
            var orientations = new HashSet<string>();
            for (int seed = -count * 4; seed < count * 8; seed++)
            {
                var g = new NonogramGame(seed, tier);
                var again = new NonogramGame(seed, tier);
                Expect(g.PictureName == again.PictureName && g.PictureIndex == again.PictureIndex
                    && g.Mirrored == again.Mirrored, "the seed picks deterministically");
                Expect(g.PictureIndex >= 0 && g.PictureIndex < count, "picture index in range");
                Expect(g.PictureName == NonogramPictures.Name(tier, g.PictureIndex), "PictureName is the library name");
                if (seed >= 0) perPicture[g.PictureIndex]++;
                orientations.Add(g.PictureIndex + (g.Mirrored ? "m" : "p"));
                string[] rows = NonogramPictures.Rows(tier, g.PictureIndex);
                for (int r = 0; r < n; r++)
                    for (int c = 0; c < n; c++)
                    {
                        char want = rows[r][g.Mirrored ? n - 1 - c : c];
                        Expect(g.Solution(r * n + c) == (want == '#'), "the bed draws the picked picture");
                    }
                bool matches;
                Expect(LineSolveUndecided(Grid(MirroredRows(rows, g.Mirrored)), out matches) == 0 && matches,
                    "each dealt bed is line-solvable");
            }
            int min = int.MaxValue, max = 0;
            foreach (int v in perPicture) { min = Math.Min(min, v); max = Math.Max(max, v); }
            Expect(min > 0 && max - min <= 1, "pictures are spread evenly across seeds in tier " + tier);
            Expect(orientations.Count > count, "some pictures also come mirrored in tier " + tier);
        }
        Expect(new NonogramGame(5, 0).PictureName != new NonogramGame(6, 0).PictureName,
            "neighbouring seeds draw different pictures");
        var extreme = new NonogramGame(int.MinValue, 2);
        Expect(extreme.PictureIndex >= 0 && extreme.PictureName.Length > 0, "extreme seeds still pick a picture");
    }
}
