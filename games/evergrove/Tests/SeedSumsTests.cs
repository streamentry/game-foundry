using System;
using System.Collections.Generic;
using System.Diagnostics;
using Evergrove.Core;

/// <summary>Seed sums: a sum-cross puzzle with painted seed counts instead of numerals.</summary>
internal static class SeedSumsTests
{
    private static int assertions;

    private static void Expect(bool ok, string message)
    {
        assertions++;
        if (!ok) throw new Exception("SeedSums test failed: " + message);
    }

    // A maximal run of open cells and the clue that the board shows for it.
    private sealed class SumRun
    {
        public int Clue;
        public bool Across;
        public List<int> Cells = new List<int>();
    }

    // Independent run extraction straight from IsOpen and the clues.
    private static List<SumRun> Runs(SeedSumsGame g)
    {
        var runs = new List<SumRun>();
        for (int dir = 0; dir < 2; dir++)
        {
            bool across = dir == 0;
            int lines = across ? g.Height : g.Width, length = across ? g.Width : g.Height;
            for (int line = 0; line < lines; line++)
            {
                int i = 0;
                while (i < length)
                {
                    if (!g.IsOpen(At(g, across, line, i))) { i++; continue; }
                    var run = new SumRun { Across = across };
                    int start = i;
                    while (i < length && g.IsOpen(At(g, across, line, i)))
                    {
                        run.Cells.Add(At(g, across, line, i));
                        i++;
                    }
                    if (start > 0)
                    {
                        int clueCell = At(g, across, line, start - 1);
                        run.Clue = across ? g.ClueAcross(clueCell) : g.ClueDown(clueCell);
                    }
                    runs.Add(run);
                }
            }
        }
        return runs;
    }

    private static int At(SeedSumsGame g, bool across, int line, int i)
    {
        return across ? line * g.Width + i : i * g.Width + line;
    }

    // Independent brute force: counts fillings (stopping at limit) where every run sums to its clue with no repeats.
    private static int CountSolutions(SeedSumsGame g, List<SumRun> runs, int limit)
    {
        int cells = g.Width * g.Height;
        var runsOf = new List<SumRun>[cells];
        for (int i = 0; i < cells; i++) runsOf[i] = new List<SumRun>();
        foreach (var r in runs) foreach (int c in r.Cells) runsOf[c].Add(r);
        var order = new List<int>();
        for (int i = 0; i < cells; i++) if (g.IsOpen(i)) order.Add(i);
        return Fill(new int[cells], order, 0, runsOf, limit);
    }

    private static int Fill(int[] grid, List<int> order, int at, List<SumRun>[] runsOf, int limit)
    {
        if (at == order.Count) return 1;
        int cell = order[at], total = 0;
        for (int d = 1; d <= 9 && total < limit; d++)
        {
            bool ok = true;
            foreach (var r in runsOf[cell])
            {
                int sum = d, filled = 1;
                foreach (int c in r.Cells)
                {
                    if (c == cell || grid[c] == 0) continue;
                    if (grid[c] == d) { ok = false; break; }
                    sum += grid[c]; filled++;
                }
                if (!ok) break;
                if (sum > r.Clue || (filled == r.Cells.Count && sum != r.Clue)) { ok = false; break; }
            }
            if (!ok) continue;
            grid[cell] = d;
            total += Fill(grid, order, at + 1, runsOf, limit - total);
            grid[cell] = 0;
        }
        return total;
    }

    // Independent simple deduction: combinations that fit, hidden singles, naked-single elimination.
    private static bool LogicSolves(SeedSumsGame g, List<SumRun> runs)
    {
        int cells = g.Width * g.Height;
        var cand = new int[cells];
        for (int i = 0; i < cells; i++) if (g.IsOpen(i)) cand[i] = 0x3FE;
        bool progress = true;
        while (progress)
        {
            progress = false;
            foreach (var r in runs)
            {
                int union = 0;
                foreach (int c in r.Cells) union |= cand[c];
                int allowed = 0, required = 0x3FE;
                bool any = false;
                for (int mask = 2; mask < 1024; mask += 2)
                {
                    if (BitCount(mask) != r.Cells.Count || SumOf(mask) != r.Clue || (mask & ~union) != 0) continue;
                    allowed |= mask; required &= mask; any = true;
                }
                if (!any) return false;
                foreach (int c in r.Cells)
                {
                    int next = cand[c] & allowed;
                    if (next != cand[c]) { cand[c] = next; progress = true; }
                    if (cand[c] == 0) return false;
                }
                foreach (int c in r.Cells)
                {
                    if (!IsSingle(cand[c])) continue;
                    foreach (int o in r.Cells)
                    {
                        if (o == c || (cand[o] & cand[c]) == 0) continue;
                        cand[o] &= ~cand[c]; progress = true;
                        if (cand[o] == 0) return false;
                    }
                }
                for (int d = 1; d <= 9; d++)
                {
                    if ((required & (1 << d)) == 0) continue;
                    int holder = -1, count = 0;
                    foreach (int c in r.Cells) if ((cand[c] & (1 << d)) != 0) { holder = c; count++; }
                    if (count == 1 && cand[holder] != (1 << d)) { cand[holder] = 1 << d; progress = true; }
                }
            }
        }
        for (int i = 0; i < cells; i++)
            if (g.IsOpen(i) && cand[i] != (1 << g.Solution(i))) return false;
        return true;
    }

    private static bool IsSingle(int mask) { return mask != 0 && (mask & (mask - 1)) == 0; }

    private static int BitCount(int mask)
    {
        int n = 0;
        for (; mask != 0; mask &= mask - 1) n++;
        return n;
    }

    private static int SumOf(int mask)
    {
        int s = 0;
        for (int d = 1; d <= 9; d++) if ((mask & (1 << d)) != 0) s += d;
        return s;
    }

    private static int OpenCount(SeedSumsGame g)
    {
        int n = 0;
        for (int i = 0; i < g.Width * g.Height; i++) if (g.IsOpen(i)) n++;
        return n;
    }

    public static int Run()
    {
        var area = new[] { 3, 4, 5 };
        var layouts = new HashSet<string>();
        var millis = new double[TreePuzzles.Tiers];
        var worst = new double[TreePuzzles.Tiers];
        const int seeds = 100;

        bool threw = false;
        try { new SeedSumsGame(1, 3); } catch (ArgumentOutOfRangeException) { threw = true; }
        Expect(threw, "unknown tier refused");
        threw = false;
        try { new SeedSumsGame(1, -1); } catch (ArgumentOutOfRangeException) { threw = true; }
        Expect(threw, "negative tier refused");

        for (int tier = 0; tier < TreePuzzles.Tiers; tier++)
        {
            for (int seed = 1; seed <= seeds; seed++)
            {
                var clock = Stopwatch.StartNew();
                var g = new SeedSumsGame(seed, tier);
                clock.Stop();
                millis[tier] += clock.Elapsed.TotalMilliseconds;
                worst[tier] = Math.Max(worst[tier], clock.Elapsed.TotalMilliseconds);
                var again = new SeedSumsGame(seed, tier);
                int total = g.Width * g.Height;

                Expect(g.Width == area[tier] + 1 && g.Height == area[tier] + 1, "grid is the open area plus a clue row and column");
                bool same = g.Width == again.Width && g.Height == again.Height;
                for (int i = 0; i < total; i++)
                    if (g.IsOpen(i) != again.IsOpen(i) || g.Solution(i) != again.Solution(i) ||
                        g.ClueAcross(i) != again.ClueAcross(i) || g.ClueDown(i) != again.ClueDown(i)) same = false;
                Expect(same, "same seed and tier, same board");

                int open = OpenCount(g);
                Expect(open >= 5 && open <= area[tier] * area[tier], "open cell count sane: " + open);
                for (int c = 0; c < g.Width; c++) Expect(!g.IsOpen(c), "top row is clues");
                for (int r = 0; r < g.Height; r++) Expect(!g.IsOpen(r * g.Width), "left column is clues");

                var runs = Runs(g);
                var seen = new int[total];
                int acrossRuns = 0, downRuns = 0;
                foreach (var run in runs)
                {
                    Expect(run.Cells.Count >= 2 && run.Cells.Count <= 9, "runs hold 2..9 cells");
                    Expect(run.Clue > 0, "every run has a clue");
                    int sum = 0, mask = 0;
                    foreach (int c in run.Cells)
                    {
                        int v = g.Solution(c);
                        Expect(v >= 1 && v <= 9, "solution digit in 1..9");
                        Expect((mask & (1 << v)) == 0, "no digit repeats within a run");
                        mask |= 1 << v; sum += v; seen[c]++;
                    }
                    Expect(sum == run.Clue, "run solution sums to its clue");
                    if (run.Across) acrossRuns++; else downRuns++;
                }
                for (int i = 0; i < total; i++)
                {
                    Expect(seen[i] == (g.IsOpen(i) ? 2 : 0), "every open cell sits in exactly one run each way");
                    if (g.IsOpen(i)) Expect(g.ClueAcross(i) == 0 && g.ClueDown(i) == 0, "open cells carry no clue");
                    Expect(g.Value(i) == 0, "starts empty");
                }
                if (tier == 0)
                    Expect(acrossRuns >= 2 && acrossRuns <= 3 && downRuns >= 2 && downRuns <= 3, "tier 0 has 2 to 3 runs each way");

                Expect(CountSolutions(g, runs, 2) == 1, "exactly one solution");
                Expect(LogicSolves(g, runs), "solvable by combinations, hidden singles and elimination");
                Expect(g.SolvableByDeduction, "engine reports the board as deduction-solvable");
                Expect(!g.Solved && g.Moves == 0, "starts unsolved with no moves");

                var sig = new List<int>();
                for (int i = 0; i < total; i++)
                    sig.Add(g.IsOpen(i) ? g.Solution(i) : -g.ClueAcross(i) * 100 - g.ClueDown(i) - 1);
                layouts.Add(tier + ":" + string.Join(",", sig));
            }
        }
        Expect(layouts.Count >= 280, "seeds give different boards: " + layouts.Count);
        for (int tier = 0; tier < TreePuzzles.Tiers; tier++)
        {
            Console.WriteLine("Seed sums tier " + tier + " generation: avg " + (millis[tier] / seeds).ToString("F2") +
                " ms, worst " + worst[tier].ToString("F2") + " ms over " + seeds + " boards");
            Expect(millis[tier] / seeds < 100, "average generation stays inside a phone's frame budget (tier " + tier + ")");
        }

        // Out-of-range reads.
        var play = new SeedSumsGame(5, 1);
        Expect(!play.IsOpen(-1) && !play.IsOpen(99) && play.ClueAcross(-1) == 0 && play.ClueDown(99) == 0 &&
               play.Value(-1) == 0 && play.Solution(99) == 0, "out-of-range reads are empty");

        // Moves.
        int empty = -1;
        for (int i = 0; i < play.Width * play.Height && empty < 0; i++) if (play.IsOpen(i)) empty = i;
        Expect(play.Place(0, 3) == SumMove.Ignored && play.Value(0) == 0, "clue cells take no digit");
        Expect(play.Place(-1, 3) == SumMove.Ignored && play.Place(play.Width * play.Height, 3) == SumMove.Ignored, "out of range ignored");
        Expect(play.Place(empty, 10) == SumMove.Ignored && play.Place(empty, -1) == SumMove.Ignored, "bad digit ignored");
        Expect(play.Moves == 0, "ignored moves are not counted");
        Expect(play.Place(empty, 0) == SumMove.Ignored, "clearing an empty cell is ignored");
        int right = play.Solution(empty);
        Expect(play.Place(empty, right) == SumMove.Placed && play.Value(empty) == right && play.Moves == 1, "a correct lone digit is placed");
        Expect(!play.Conflicts(empty), "a lone correct digit is not flagged");
        Expect(play.Place(empty, 0) == SumMove.Cleared && play.Value(empty) == 0 && play.Moves == 2, "cleared");

        // A repeated digit in a run is allowed but flagged on both cells, and clears with the twin.
        var rep = new SeedSumsGame(11, 1);
        var rruns = Runs(rep);
        int a = rruns[0].Cells[0], b = rruns[0].Cells[1];
        Expect(rep.Place(a, 1) != SumMove.Ignored, "a digit can be placed");
        Expect(!rep.Conflicts(a), "a lone small digit within the clue is fine");
        Expect(rep.Place(b, 1) == SumMove.Conflict && rep.Value(b) == 1, "a repeat is placed and flagged");
        Expect(rep.Conflicts(a) && rep.Conflicts(b), "both repeated cells are flagged");
        rep.Place(b, 0);
        Expect(!rep.Conflicts(a), "the flag clears when the twin goes");

        var down = rruns.Find(r => !r.Across);
        var col = new SeedSumsGame(11, 1);
        col.Place(down.Cells[0], 1);
        Expect(col.Place(down.Cells[1], 1) == SumMove.Conflict && col.Conflicts(down.Cells[0]), "down runs flag repeats");

        // A run that overshoots its clue, or is full with the wrong total, is flagged without a repeat.
        var sum9 = new SeedSumsGame(7, 0);
        var target = Runs(sum9)[0];
        Expect(target.Clue < 9 * target.Cells.Count, "test run can overshoot");
        int placed = 0;
        bool sawOvershoot = false;
        for (int k = 0; k < target.Cells.Count; k++)
        {
            int d = 9 - k;
            placed += d;
            SumMove m = sum9.Place(target.Cells[k], d);
            if (placed > target.Clue) { sawOvershoot = true; Expect(m == SumMove.Conflict, "overshooting the clue is flagged"); }
        }
        Expect(sawOvershoot, "overshoot case exercised");

        // Wrong digits are allowed; solved only when every digit is right.
        for (int tier = 0; tier < TreePuzzles.Tiers; tier++)
            for (int seed = 1; seed <= 20; seed++)
            {
                var s = new SeedSumsGame(seed, tier);
                int total = s.Width * s.Height;
                int last = -1;
                for (int i = 0; i < total; i++) if (s.IsOpen(i)) last = i;
                for (int i = 0; i < total; i++)
                {
                    if (!s.IsOpen(i) || i == last) continue;
                    Expect(!s.Solved, "not solved before the last digit");
                    s.Place(i, s.Solution(i));
                }
                Expect(!s.Solved, "one empty cell left is not solved");
                s.Place(last, s.Solution(last) % 9 + 1);
                Expect(!s.Solved, "a wrong final digit is not solved");
                Expect(s.Conflicts(last), "a wrong digit completing its runs is flagged");
                Expect(s.Place(last, s.Solution(last)) != SumMove.Ignored, "a wrong digit can be replaced");
                Expect(s.Solved, "the right digits solve the board");
                for (int i = 0; i < total; i++) Expect(!s.Conflicts(i), "the solution has no flags");
                Expect(s.Place(last, 0) == SumMove.Ignored && s.Solved, "a solved board is closed");

                // Hints: always a correct placement, never wasted, and they finish the board.
                var h = new SeedSumsGame(seed, tier);
                int cell, digit, steps = 0;
                while (!h.Solved)
                {
                    Expect(h.Hint(out cell, out digit), "unsolved board has a hint");
                    Expect(h.IsOpen(cell) && digit == h.Solution(cell) && h.Value(cell) != digit, "hint is a correct, new placement");
                    h.Place(cell, digit);
                    steps++;
                    Expect(steps <= OpenCount(h), "hints finish the board");
                }
                Expect(steps == OpenCount(h), "hints never waste a move");
                Expect(!h.Hint(out cell, out digit) && cell == -1 && digit == 0, "no hint once solved");

                // With a wrong digit on the board the hint corrects it first.
                var w = new SeedSumsGame(seed, tier);
                int first = -1;
                for (int i = 0; i < total && first < 0; i++) if (w.IsOpen(i)) first = i;
                w.Place(first, w.Solution(first) % 9 + 1);
                Expect(w.Hint(out cell, out digit) && cell == first && digit == w.Solution(first), "a hint corrects a wrong digit first");
            }
        return assertions;
    }
}
