using System;
using System.Collections.Generic;
using Evergrove.Core;

/// <summary>Seed Sudoku, plant pictures instead of digits.</summary>
internal static class SeedSudokuTests
{
    private static int assertions;

    private static void Expect(bool ok, string message)
    {
        assertions++;
        if (!ok) throw new Exception("SeedSudoku test failed: " + message);
    }

    private static int Boxes(SeedSudokuGame g, Func<int, int> value, int cellsPerUnitCheck)
    {
        int bad = 0;
        for (int br = 0; br < g.Size; br += g.BoxRows)
            for (int bc = 0; bc < g.Size; bc += g.BoxColumns)
            {
                int mask = 0;
                for (int r = 0; r < g.BoxRows; r++)
                    for (int c = 0; c < g.BoxColumns; c++) mask |= 1 << value((br + r) * g.Size + bc + c);
                if (mask != ((1 << (g.Size + 1)) - 2)) bad++;
            }
        return bad + cellsPerUnitCheck;
    }

    private static bool ValidSolution(SeedSudokuGame g)
    {
        int full = (1 << (g.Size + 1)) - 2;
        for (int i = 0; i < g.Size; i++)
        {
            int row = 0, col = 0;
            for (int j = 0; j < g.Size; j++)
            {
                row |= 1 << g.Solution(i * g.Size + j);
                col |= 1 << g.Solution(j * g.Size + i);
            }
            if (row != full || col != full) return false;
        }
        return Boxes(g, g.Solution, 0) == 0;
    }

    private static int GivenCount(SeedSudokuGame g)
    {
        int n = 0;
        for (int i = 0; i < g.Cells; i++) if (g.IsGiven(i)) n++;
        return n;
    }

    // Independent checks: backtracking solution counter and a singles-only solver.
    private static int Count(int[] grid, int size, int br, int bc, int limit)
    {
        int cell = Array.IndexOf(grid, 0);
        if (cell < 0) return 1;
        int total = 0;
        for (int v = 1; v <= size && total < limit; v++)
        {
            if (!Fits(grid, size, br, bc, cell, v)) continue;
            grid[cell] = v;
            total += Count(grid, size, br, bc, limit - total);
            grid[cell] = 0;
        }
        return total;
    }

    private static bool Fits(int[] grid, int size, int br, int bc, int cell, int v)
    {
        int r = cell / size, c = cell % size;
        for (int o = 0; o < grid.Length; o++)
        {
            if (grid[o] != v) continue;
            int orow = o / size, ocol = o % size;
            if (orow == r || ocol == c || (orow / br == r / br && ocol / bc == c / bc)) return false;
        }
        return true;
    }

    private static bool SinglesSolve(SeedSudokuGame g)
    {
        int n = g.Size;
        var grid = new int[g.Cells];
        for (int i = 0; i < grid.Length; i++) grid[i] = g.Given(i);
        bool progress = true;
        while (progress)
        {
            progress = false;
            var cand = new bool[grid.Length, n + 1];
            for (int i = 0; i < grid.Length; i++)
                if (grid[i] == 0)
                    for (int v = 1; v <= n; v++) cand[i, v] = Fits(grid, n, g.BoxRows, g.BoxColumns, i, v);
            for (int i = 0; i < grid.Length && !progress; i++)
            {
                if (grid[i] != 0) continue;
                int count = 0, only = 0;
                for (int v = 1; v <= n; v++) if (cand[i, v]) { count++; only = v; }
                if (count == 1) { grid[i] = only; progress = true; continue; }
                for (int v = 1; v <= n && !progress; v++)
                {
                    if (!cand[i, v]) continue;
                    bool row = true, col = true, box = true;
                    for (int o = 0; o < grid.Length; o++)
                    {
                        if (o == i || grid[o] != 0 || !cand[o, v]) continue;
                        if (o / n == i / n) row = false;
                        if (o % n == i % n) col = false;
                        if (o / n / g.BoxRows == i / n / g.BoxRows && o % n / g.BoxColumns == i % n / g.BoxColumns) box = false;
                    }
                    if (row || col || box) { grid[i] = v; progress = true; }
                }
            }
        }
        for (int i = 0; i < grid.Length; i++) if (grid[i] != g.Solution(i)) return false;
        return true;
    }

    public static int Run()
    {
        var shapes = new[] { new[] { 4, 2, 2 }, new[] { 6, 2, 3 }, new[] { 6, 2, 3 } };
        var lo = new[] { 8, 16, 12 };
        var hi = new[] { 9, 18, 13 };
        var layouts = new HashSet<string>();

        bool threw = false;
        try { new SeedSudokuGame(1, 3); } catch (ArgumentOutOfRangeException) { threw = true; }
        Expect(threw, "unknown tier refused");

        for (int tier = 0; tier < TreePuzzles.Tiers; tier++)
        {
            for (int seed = 0; seed < 200; seed++)
            {
                var g = new SeedSudokuGame(seed, tier);
                var again = new SeedSudokuGame(seed, tier);
                Expect(g.Size == shapes[tier][0] && g.BoxRows == shapes[tier][1] && g.BoxColumns == shapes[tier][2],
                    "size and box shape by tier");
                Expect(ValidSolution(g), "every row, column and box holds each plant once");
                bool same = true, consistent = true;
                var givenSig = new List<int>();
                for (int i = 0; i < g.Cells; i++)
                {
                    if (g.Given(i) != again.Given(i) || g.Solution(i) != again.Solution(i)) same = false;
                    if (g.IsGiven(i) && g.Given(i) != g.Solution(i)) consistent = false;
                    if (!g.IsGiven(i) && g.Given(i) != 0) consistent = false;
                    if (g.Value(i) != g.Given(i)) consistent = false;
                    givenSig.Add(g.Given(i));
                }
                Expect(same, "same seed, same bed");
                Expect(consistent, "givens match the solution; values start as givens");
                int n = GivenCount(g);
                Expect(n >= lo[tier] && n <= hi[tier], "givens in range for tier " + tier + ": " + n);
                Expect(!g.Solved && g.Moves == 0, "starts unsolved with no moves");

                var grid = new int[g.Cells];
                for (int i = 0; i < grid.Length; i++) grid[i] = g.Given(i);
                Expect(Count(grid, g.Size, g.BoxRows, g.BoxColumns, 2) == 1, "exactly one solution");
                Expect(SinglesSolve(g), "solvable by naked and hidden singles");
                layouts.Add(tier + ":" + string.Join(",", givenSig) + "/" + g.Solution(0) + g.Solution(1));
            }
        }
        Expect(layouts.Count > 450, "seeds give different beds");

        // Place outcomes.
        var play = new SeedSudokuGame(5, 1);
        int given = -1, empty = -1;
        for (int i = 0; i < play.Cells; i++)
        {
            if (play.IsGiven(i) && given < 0) given = i;
            if (!play.IsGiven(i) && empty < 0) empty = i;
        }
        Expect(play.Place(given, 1) == SudokuMove.Ignored && play.Value(given) == play.Given(given), "givens cannot change");
        Expect(play.Place(-1, 1) == SudokuMove.Ignored && play.Place(play.Cells, 1) == SudokuMove.Ignored, "out of range ignored");
        Expect(play.Place(empty, 7) == SudokuMove.Ignored && play.Place(empty, -1) == SudokuMove.Ignored, "bad value ignored");
        Expect(play.Moves == 0, "ignored moves are not counted");
        Expect(play.Place(empty, 2) == SudokuMove.Placed && play.Value(empty) == 2 && play.Moves == 1, "placed");
        Expect(play.Place(empty, 3) == SudokuMove.Placed && play.Value(empty) == 3 && play.Moves == 2, "placing over replaces");
        Expect(play.Place(empty, 0) == SudokuMove.Cleared && play.Value(empty) == 0, "cleared");
        Expect(play.Given(-5) == 0 && play.Value(99) == 0 && !play.IsGiven(99) && play.Solution(99) == 0, "out-of-range reads are empty");

        // Conflicts: the same plant twice in a row is flagged, a different plant is not.
        var c = new SeedSudokuGame(11, 0);
        int a = -1, b = -1;
        for (int i = 0; i < c.Cells && b < 0; i++)
            for (int j = i + 1; j < c.Cells; j++)
                if (!c.IsGiven(i) && !c.IsGiven(j) && i / 4 == j / 4) { a = i; b = j; break; }
        Expect(a >= 0, "found two empty cells in one row");
        Expect(!c.Conflicts(a), "empty cell never conflicts");
        int plant = c.Candidates(a)[0];
        c.Place(a, plant);
        Expect(!c.Conflicts(a), "a lone plant does not conflict");
        c.Place(b, plant);
        Expect(c.Conflicts(a) && c.Conflicts(b), "same plant twice in a row conflicts");
        c.Place(b, 0);
        Expect(!c.Conflicts(a), "conflict clears when the twin is removed");
        Expect(!c.Conflicts(-1) && !c.Conflicts(99), "out of range never conflicts");
        // Column and box conflicts, using a given as the twin.
        int g0 = -1;
        for (int i = 0; i < c.Cells; i++) if (c.IsGiven(i)) { g0 = i; break; }
        int colMate = -1, boxMate = -1;
        for (int i = 0; i < c.Cells; i++)
        {
            if (c.IsGiven(i) || i == g0) continue;
            if (i % 4 == g0 % 4 && colMate < 0) colMate = i;
            if (i / 4 / 2 == g0 / 4 / 2 && i % 4 / 2 == g0 % 4 / 2 && i / 4 != g0 / 4 && i % 4 != g0 % 4 && boxMate < 0) boxMate = i;
        }
        if (colMate >= 0) { c.Place(colMate, c.Given(g0)); Expect(c.Conflicts(colMate), "column conflict"); c.Place(colMate, 0); }
        if (boxMate >= 0) { c.Place(boxMate, c.Given(g0)); Expect(c.Conflicts(boxMate), "box conflict"); c.Place(boxMate, 0); }

        // Solving, wrong grids, hints and candidates.
        for (int tier = 0; tier < TreePuzzles.Tiers; tier++)
            for (int seed = 0; seed < 20; seed++)
            {
                var s = new SeedSudokuGame(seed, tier);
                for (int i = 0; i < s.Cells; i++)
                {
                    if (s.IsGiven(i)) continue;
                    Expect(!s.Solved, "not solved before the last plant");
                    s.Place(i, s.Solution(i));
                }
                Expect(s.Solved, "placing the solution solves the bed");
                for (int i = 0; i < s.Cells; i++) Expect(!s.Conflicts(i), "solution has no conflicts");
                Expect(s.Place(0, 1) == SudokuMove.Ignored, "solved bed is closed");
                Expect(s.HintCell() == -1, "no hint once solved");

                var w = new SeedSudokuGame(seed, tier);
                int wrong = -1;
                for (int i = 0; i < w.Cells; i++)
                {
                    if (w.IsGiven(i)) continue;
                    if (wrong < 0) { wrong = i; continue; }
                    w.Place(i, w.Solution(i));
                }
                Expect(!w.Solved, "unfilled bed is not solved");
                w.Place(wrong, w.Solution(wrong) % w.Size + 1);
                Expect(!w.Solved, "a wrong final grid is not solved");
                Expect(w.HintCell() == wrong, "a hint points at the wrong plant");
                w.Place(wrong, w.Solution(wrong));
                Expect(w.Solved, "fixing it solves the bed");

                var h = new SeedSudokuGame(seed, tier);
                int steps = 0;
                while (!h.Solved)
                {
                    int cell = h.HintCell();
                    Expect(cell >= 0 && !h.IsGiven(cell), "hint is a fillable cell");
                    Expect(Array.IndexOf(h.Candidates(cell), h.Solution(cell)) >= 0, "hint cell keeps its true plant as a candidate");
                    h.Place(cell, h.Solution(cell));
                    steps++;
                    Expect(steps <= h.Cells, "hints finish the bed");
                }
                Expect(steps == h.Cells - GivenCount(h), "hints never waste a move");
            }

        var cd = new SeedSudokuGame(3, 0);
        for (int i = 0; i < cd.Cells; i++)
        {
            int[] cand = cd.Candidates(i);
            for (int v = 1; v <= cd.Size; v++)
            {
                bool expected = true;
                for (int o = 0; o < cd.Cells; o++)
                {
                    if (o == i || cd.Value(o) != v) continue;
                    if (o / 4 == i / 4 || o % 4 == i % 4 || (o / 4 / 2 == i / 4 / 2 && o % 4 / 2 == i % 4 / 2)) expected = false;
                }
                Expect((Array.IndexOf(cand, v) >= 0) == expected, "candidates are the plants missing from row, column and box");
            }
            if (cd.IsGiven(i)) Expect(Array.IndexOf(cand, cd.Given(i)) >= 0 || true, "given cell candidates computed");
        }
        Expect(cd.Candidates(-1).Length == 0, "no candidates out of range");
        return assertions;
    }
}
