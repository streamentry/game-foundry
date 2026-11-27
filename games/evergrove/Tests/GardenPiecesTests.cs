using System;
using System.Collections.Generic;
using Evergrove.Core;

/// <summary>Garden pieces, the grid tangram gardens hold: fill the silhouette exactly with the pieces.</summary>
internal static class GardenPiecesTests
{
    private static int assertions;

    private static void Expect(bool ok, string message)
    {
        assertions++;
        if (!ok) throw new Exception("GardenPieces test failed: " + message);
    }

    private static readonly int[] Pieces = { 3, 5, 7 };
    private static readonly int[] MinCells = { 9, 16, 24 };
    private static readonly int[] MaxCells = { 11, 20, 30 };

    private static string Key(GardenPiecesGame g)
    {
        var parts = new List<string> { g.Width + "x" + g.Height };
        for (int c = 0; c < g.Width * g.Height; c++) parts.Add(g.InShape(c) ? "#" : ".");
        for (int p = 0; p < g.PieceCount; p++) parts.Add(string.Join(",", g.PieceCells(p)) + ";");
        GardenHint h = g.Hint();
        parts.Add(h.Piece + ":" + h.Turn + ":" + h.Mirrored + ":" + h.Cell);
        return string.Join("", parts);
    }

    private static string State(GardenPiecesGame g)
    {
        var parts = new List<string> { g.Moves.ToString(), g.Solved.ToString() };
        for (int c = 0; c < g.Width * g.Height; c++) parts.Add(g.Owner(c).ToString());
        for (int p = 0; p < g.PieceCount; p++)
            parts.Add(p + ":" + g.Turn(p) + g.Mirrored(p) + g.Placed(p) + g.PlacedAt(p));
        return string.Join("|", parts);
    }

    private static int Cells(GardenPiecesGame g)
    {
        int n = 0;
        for (int c = 0; c < g.Width * g.Height; c++) if (g.InShape(c)) n++;
        return n;
    }

    private static bool Connected(GardenPiecesGame g)
    {
        int w = g.Width, h = g.Height, start = -1, total = 0;
        for (int c = 0; c < w * h; c++) if (g.InShape(c)) { total++; if (start < 0) start = c; }
        var seen = new bool[w * h];
        var stack = new Stack<int>();
        stack.Push(start);
        seen[start] = true;
        int reached = 0;
        while (stack.Count > 0)
        {
            int c = stack.Pop();
            reached++;
            int x = c % w, y = c / w;
            int[] nb = { x > 0 ? c - 1 : -1, x < w - 1 ? c + 1 : -1, y > 0 ? c - w : -1, y < h - 1 ? c + w : -1 };
            foreach (int n in nb)
                if (n >= 0 && g.InShape(n) && !seen[n]) { seen[n] = true; stack.Push(n); }
        }
        return reached == total;
    }

    private static int PerimeterOf(GardenPiecesGame g)
    {
        int w = g.Width, h = g.Height, edges = 0;
        for (int y = 0; y < h; y++)
            for (int x = 0; x < w; x++)
            {
                if (!g.InShape(y * w + x)) continue;
                if (x == 0 || !g.InShape(y * w + x - 1)) edges++;
                if (x == w - 1 || !g.InShape(y * w + x + 1)) edges++;
                if (y == 0 || !g.InShape(y * w + x - w)) edges++;
                if (y == h - 1 || !g.InShape(y * w + x + w)) edges++;
            }
        return edges;
    }

    /// <summary>Every piece's cells and the owner grid must agree, and nothing overlaps.</summary>
    private static void CheckConsistent(GardenPiecesGame g)
    {
        int w = g.Width, covered = 0;
        var count = new int[g.PieceCount];
        for (int c = 0; c < w * g.Height; c++)
        {
            int o = g.Owner(c);
            if (o < 0) continue;
            Expect(g.InShape(c), "only silhouette cells are covered");
            Expect(o < g.PieceCount && g.Placed(o), "owner is a placed piece");
            count[o]++;
            covered++;
        }
        int expected = 0;
        for (int p = 0; p < g.PieceCount; p++)
        {
            int[] cells = g.PieceCells(p);
            if (!g.Placed(p)) { Expect(count[p] == 0, "unplaced piece owns nothing"); continue; }
            expected += cells.Length / 2;
            Expect(count[p] == cells.Length / 2, "placed piece owns all its cells");
            int ax = g.PlacedAt(p) % w, ay = g.PlacedAt(p) / w;
            for (int i = 0; i < cells.Length; i += 2)
                Expect(g.Owner((ay + cells[i + 1]) * w + ax + cells[i]) == p, "cell offset from the anchor is owned");
        }
        Expect(covered == expected, "no overlap, no stray cover");
        Expect(g.Solved == (covered == Cells(g)), "Solved means every silhouette cell is covered");
    }

    private static void Orient(GardenPiecesGame g, int piece, int turn, bool mirrored)
    {
        int guard = 0;
        while (g.Mirrored(piece) != mirrored && guard++ < 2) Expect(g.Mirror(piece) == PieceMove.Done, "mirror to hint");
        guard = 0;
        while (g.Turn(piece) != turn && guard++ < 4) Expect(g.Rotate(piece) == PieceMove.Done, "turn to hint");
        Expect(g.Turn(piece) == turn && g.Mirrored(piece) == mirrored, "hint orientation reached");
    }

    /// <summary>Follow the hints until solved; returns the number of hint steps.</summary>
    private static int PlayHints(GardenPiecesGame g)
    {
        int steps = 0, limit = g.PieceCount * 7 + 2;
        while (!g.Solved)
        {
            Expect(steps++ < limit, "hints solve within a small bound");
            GardenHint h = g.Hint();
            Expect(h.Has, "an unsolved board has a hint");
            if (h.Misplaced)
            {
                Expect(g.Placed(h.Piece), "a misplaced hint names a placed piece");
                Expect(g.Lift(h.Piece) == PieceMove.Done, "lift the misplaced piece");
            }
            else
            {
                Expect(!g.Placed(h.Piece), "a placing hint names a tray piece");
                Orient(g, h.Piece, h.Turn, h.Mirrored);
                Expect(g.Place(h.Piece, h.Cell) == PieceMove.Done, "hint placement is legal");
            }
            CheckConsistent(g);
        }
        Expect(!g.Hint().Has, "no hint once solved");
        return steps;
    }

    private static uint Rng(ref uint s)
    {
        s ^= s << 13; s ^= s >> 17; s ^= s << 5;
        return s;
    }

    /// <summary>Make a few random legal moves (turns, mirrors, placements) to reach a partial state.</summary>
    private static void Scramble(GardenPiecesGame g, ref uint s, int tries)
    {
        for (int i = 0; i < tries; i++)
        {
            int p = (int)(Rng(ref s) % (uint)g.PieceCount);
            switch (Rng(ref s) % 4)
            {
                case 0: g.Rotate(p); break;
                case 1: g.Mirror(p); break;
                case 2: g.Place(p, (int)(Rng(ref s) % (uint)(g.Width * g.Height))); break;
                default: g.Lift(p); break;
            }
            if (g.Solved) return;
        }
    }

    public static int Run()
    {
        assertions = 0;
        bool threw = false;
        try { new GardenPiecesGame(1, 3); } catch (ArgumentOutOfRangeException) { threw = true; }
        Expect(threw, "unknown tier refused");
        threw = false;
        try { new GardenPiecesGame(1, -1); } catch (ArgumentOutOfRangeException) { threw = true; }
        Expect(threw, "negative tier refused");

        for (int tier = 0; tier < TreePuzzles.Tiers; tier++)
        {
            var keys = new HashSet<string>();
            for (int seed = 1; seed <= 200; seed++)
            {
                var g = new GardenPiecesGame(seed, tier);
                Expect(Key(g) == Key(new GardenPiecesGame(seed, tier)), "same seed and tier deal the same board");
                keys.Add(Key(g));

                Expect(g.PieceCount == Pieces[tier], "piece count per tier");
                int cells = Cells(g);
                Expect(cells >= MinCells[tier] && cells <= MaxCells[tier], "silhouette size per tier");
                Expect(g.CellCount == cells, "CellCount matches the silhouette");
                Expect(g.Width >= 1 && g.Width <= 8 && g.Height >= 1 && g.Height <= 8, "bounding box within 8x8");
                Expect(!g.InShape(-1) && !g.InShape(g.Width * g.Height), "out of range is not in the shape");
                Expect(Connected(g), "silhouette is connected");
                Expect(g.Perimeter == PerimeterOf(g), "perimeter matches an independent count");
                Expect(g.Perimeter <= GardenPiecesGame.MinPerimeter(cells) + GardenPiecesGame.MaxPerimeterExcess,
                    "silhouette is compact by perimeter");
                Expect(cells * 100 >= g.Width * g.Height * 55, "silhouette fills its box");
                Expect(g.Compact, "Compact agrees");
                // Tight box: every edge of the box touches the silhouette.
                bool left = false, right = false, top = false, bottom = false;
                for (int y = 0; y < g.Height; y++)
                    for (int x = 0; x < g.Width; x++)
                    {
                        if (!g.InShape(y * g.Width + x)) continue;
                        left |= x == 0; right |= x == g.Width - 1; top |= y == 0; bottom |= y == g.Height - 1;
                    }
                Expect(left && right && top && bottom, "bounding box is tight");

                int sum = 0;
                for (int p = 0; p < g.PieceCount; p++)
                {
                    int n = g.PieceCells(p).Length / 2;
                    Expect(n >= 2 && n <= 5, "pieces are 2 to 5 cells");
                    Expect(g.PieceCells(p).Length % 2 == 0, "cells come as pairs");
                    Expect(!g.Placed(p) && g.PlacedAt(p) == -1 && g.Turn(p) == 0 && !g.Mirrored(p), "pieces start in the tray, unturned");
                    sum += n;
                }
                Expect(sum == cells, "pieces add up to the silhouette");
                Expect(g.Moves == 0 && !g.Solved, "fresh board");
                for (int c = 0; c < g.Width * g.Height; c++) Expect(g.Owner(c) == -1, "fresh board is empty");
                Expect(g.Owner(-1) == -1 && g.Owner(g.Width * g.Height) == -1, "owner out of range is -1");

                // The generator's own solution, applied through the public API, solves it.
                var solve = new GardenPiecesGame(seed, tier);
                int steps = PlayHints(solve);
                Expect(steps <= g.PieceCount * 5, "an empty board takes at most five steps per piece");
                Expect(solve.Solved && solve.Moves > 0, "solved by the generator's solution");
                for (int c = 0; c < solve.Width * solve.Height; c++)
                    Expect(solve.InShape(c) == (solve.Owner(c) >= 0), "exact fill: covered iff in the shape");
                if (tier == 0)
                    for (int p = 0; p < solve.PieceCount; p++) Expect(!solve.Mirrored(p), "tier 0 solutions never mirror");
            }
            Expect(keys.Count > 150, "boards vary by seed");

            // Hint-driven play from arbitrary partial states.
            for (int seed = 1; seed <= 60; seed++)
            {
                var g = new GardenPiecesGame(seed, tier);
                uint s = unchecked((uint)(seed * 7919 + tier * 104729 + 17));
                Scramble(g, ref s, 3 + seed % 40);
                CheckConsistent(g);
                PlayHints(g);
            }
        }

        RulesTests();
        PrefersCorrectionTests();
        return assertions;
    }

    private static void RulesTests()
    {
        for (int tier = 0; tier < TreePuzzles.Tiers; tier++)
            for (int seed = 1; seed <= 30; seed++)
            {
                var g = new GardenPiecesGame(seed, tier);
                int n = g.Width * g.Height;

                // Four turns return the same cells; two mirrors do too.
                for (int p = 0; p < g.PieceCount; p++)
                {
                    string before = string.Join(",", g.PieceCells(p));
                    for (int i = 0; i < 4; i++)
                    {
                        Expect(g.Rotate(p) == PieceMove.Done, "a free piece turns");
                        Expect(g.Turn(p) == (i + 1) % 4, "turn counts a quarter at a time");
                    }
                    Expect(string.Join(",", g.PieceCells(p)) == before, "four turns return the same cells");
                    if (tier == 0)
                    {
                        Expect(g.Mirror(p) == PieceMove.Ignored && !g.Mirrored(p), "tier 0 cannot mirror");
                    }
                    else
                    {
                        Expect(g.Mirror(p) == PieceMove.Done && g.Mirrored(p), "mirror from tier 1");
                        Expect(g.Mirror(p) == PieceMove.Done && !g.Mirrored(p), "mirror twice is the same");
                        Expect(string.Join(",", g.PieceCells(p)) == before, "two mirrors return the same cells");
                    }
                    int[] c = g.PieceCells(p);
                    Expect(c[0] == 0 && c[1] == 0, "the anchor is the first cell, at the origin");
                    for (int i = 0; i < c.Length; i += 2) Expect(c[i + 1] >= 0, "no cell above the anchor");
                }
                Expect(g.Moves == g.PieceCount * (tier == 0 ? 4 : 6), "turns and mirrors that happened count as moves");

                // Refused moves change nothing.
                string snap = State(g);
                Expect(g.Place(0, -1) == PieceMove.Outside, "negative cell is outside");
                Expect(g.Place(0, n) == PieceMove.Outside, "past the box is outside");
                Expect(g.Lift(0) == PieceMove.Ignored, "lifting a tray piece is ignored");
                Expect(g.Rotate(-1) == PieceMove.Ignored && g.Rotate(g.PieceCount) == PieceMove.Ignored, "bad piece index ignored");
                Expect(g.Mirror(99) == PieceMove.Ignored && g.Lift(-3) == PieceMove.Ignored && g.Place(99, 0) == PieceMove.Ignored,
                    "bad piece index ignored (mirror, lift, place)");
                int hole = -1;
                for (int c = 0; c < n; c++) if (!g.InShape(c)) { hole = c; break; }
                if (hole >= 0) Expect(g.Place(0, hole) == PieceMove.Outside, "a cell outside the silhouette is refused");
                Expect(State(g) == snap, "refusals change nothing");

                // Place the first piece anywhere legal, then test overlap and re-place refusals.
                int at = -1;
                for (int c = 0; c < n && at < 0; c++) if (g.InShape(c) && g.Place(0, c) == PieceMove.Done) at = c;
                if (at >= 0)
                {
                    CheckConsistent(g);
                    Expect(g.Placed(0) && g.PlacedAt(0) == at && g.Owner(at) == 0, "anchor cell is covered by the anchor");
                    snap = State(g);
                    Expect(g.Place(0, at) == PieceMove.Ignored, "placing a placed piece is ignored");
                    Expect(g.Rotate(0) == PieceMove.Ignored && g.Mirror(0) == PieceMove.Ignored, "a placed piece cannot be turned");
                    Expect(g.Place(1, at) == PieceMove.Overlaps || g.Place(1, at) == PieceMove.Outside, "cannot cover a covered cell");
                    Expect(State(g) == snap, "a refused move leaves the board unchanged");
                    int movesBefore = g.Moves;
                    Expect(g.Lift(0) == PieceMove.Done && !g.Placed(0) && g.PlacedAt(0) == -1 && g.Owner(at) == -1, "lifting frees the cells");
                    Expect(g.Moves == movesBefore + 1, "lift counts");
                    CheckConsistent(g);
                }
            }

        // Overlap versus outside is told apart: find a board where a second piece overlaps the first.
        bool sawOverlap = false;
        for (int seed = 1; seed <= 40 && !sawOverlap; seed++)
        {
            var g = new GardenPiecesGame(seed, 0);
            int n = g.Width * g.Height, first = -1;
            for (int c = 0; c < n && first < 0; c++) if (g.InShape(c) && g.Place(0, c) == PieceMove.Done) first = c;
            if (first < 0) continue;
            for (int c = 0; c < n; c++)
            {
                string snap = State(g);
                PieceMove m = g.Place(1, c);
                if (m == PieceMove.Overlaps) { sawOverlap = true; Expect(State(g) == snap, "overlap refused unchanged"); break; }
                if (m == PieceMove.Done) g.Lift(1);
            }
        }
        Expect(sawOverlap, "an overlap reason is reachable");
    }

    private static void PrefersCorrectionTests()
    {
        int checkedBoards = 0;
        for (int tier = 0; tier < TreePuzzles.Tiers; tier++)
            for (int seed = 1; seed <= 40; seed++)
            {
                var g = new GardenPiecesGame(seed, tier);
                GardenHint first = g.Hint();
                Expect(first.Has && !first.Misplaced, "an empty board's hint places a piece");
                // Put a piece somewhere that is not part of the answer.
                int n = g.Width * g.Height, wrong = -1, piece = -1;
                for (int p = 0; p < g.PieceCount && wrong < 0; p++)
                    for (int c = 0; c < n && wrong < 0; c++)
                    {
                        if (!g.InShape(c) || g.Place(p, c) != PieceMove.Done) continue;
                        if (g.Hint().Misplaced) { wrong = c; piece = p; }
                        else g.Lift(p);
                    }
                if (wrong < 0) continue;
                checkedBoards++;
                GardenHint fix = g.Hint();
                Expect(fix.Misplaced && fix.Piece == piece, "the hint corrects the misplaced piece first");
                Expect(g.Placed(fix.Piece), "the misplaced piece is the placed one");
                Expect(g.Lift(fix.Piece) == PieceMove.Done, "lifting corrects it");
                Expect(!g.Hint().Misplaced, "nothing is misplaced after the lift");
            }
        Expect(checkedBoards > 20, "misplaced pieces were exercised");
    }
}
