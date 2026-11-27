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
        Expect(keys.Count > 100, "beds differ from seed to seed");

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
        return assertions;
    }
}
