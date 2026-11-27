using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using Evergrove.Core;

/// <summary>
/// v1 #35: weekly deeper boards. Every kind with HasWeekly proves its top-tier board has exactly
/// one solution, and that the board is solvable, for 52 weeks in each of 3 gardens. Where the
/// generator exposes its own count (Water) that count is used; elsewhere an independent count
/// here, capped at 2 solutions, does the proof.
/// </summary>
internal static class WeeklyBoardTests
{
    private static int assertions;
    private static int boards;
    private static int kinds;

    private static readonly int[] Gardens = { 1, 2, 3 };

    private static void Expect(bool ok, string message)
    {
        assertions++;
        if (!ok) throw new Exception("Weekly board test failed: " + message);
    }

    public static int Run()
    {
        IsoWeekKeys();
        Expect(WeeklyBoard.Tier == TreePuzzles.Tiers - 1, "weekly boards are dealt at the deepest tier");
        Expect(WeeklyBoard.SeedFor(1, PuzzleKind.Sudoku, 202601) == WeeklyBoard.SeedFor(1, PuzzleKind.Sudoku, 202601),
            "seed is deterministic");
        SeedsAreApart();

        foreach (PuzzleKind kind in Enum.GetValues(typeof(PuzzleKind)))
        {
            if (!WeeklyBoard.HasWeekly(kind)) continue;
            kinds++;
            ProveKind(kind);
        }
        Expect(kinds == 6, "six kinds offer weekly boards");

        Console.WriteLine("Weekly boards: kinds " + kinds + ", boards " + boards + ", all unique");
        return assertions;
    }

    // ---- week keys and seeds -------------------------------------------------------------

    private static void IsoWeekKeys()
    {
        Expect(WeeklyBoard.IsoWeekOf(20261231) == 202653, "2026-12-31 is 2026 week 53");
        Expect(WeeklyBoard.IsoWeekOf(20270104) == 202701, "2027-01-04 is 2027 week 1");
        Expect(WeeklyBoard.IsoWeekOf(20251229) == 202601, "2025-12-29 already belongs to 2026 week 1");
        Expect(WeeklyBoard.IsoWeekOf(20260101) == 202601, "2026-01-01 is 2026 week 1");
        Expect(WeeklyBoard.Week(2026, 7) == 202607, "week key is year then week");
        foreach (int yyyymmdd in new[] { 20260105, 20260615, 20261231, 20270104, 20280229 })
        {
            var date = new DateTime(yyyymmdd / 10000, yyyymmdd / 100 % 100, yyyymmdd % 100);
            int expected = WeeklyBoard.Week(ISOWeek.GetYear(date), ISOWeek.GetWeekOfYear(date));
            Expect(WeeklyBoard.IsoWeekOf(yyyymmdd) == expected, "matches ISOWeek for " + yyyymmdd);
        }
    }

    private static void SeedsAreApart()
    {
        // ReplayBoards seeds for a sample of things (garden * 1000 + item) and replay counts.
        var replay = new HashSet<int>();
        foreach (int garden in Gardens)
            for (int item = 0; item < 20; item++)
                for (int r = 0; r <= 40; r++)
                    replay.Add(ReplayBoards.Seed(garden * 1000 + item, r));
        var seen = new HashSet<int>();
        foreach (int garden in Gardens)
            for (int week = 1; week <= 52; week++)
                foreach (PuzzleKind kind in Enum.GetValues(typeof(PuzzleKind)))
                {
                    int seed = WeeklyBoard.SeedFor(garden, kind, WeeklyBoard.Week(2026, week));
                    Expect(!replay.Contains(seed), "weekly seed never equals a replay seed");
                    seen.Add(seed);
                }
        Expect(seen.Count == Gardens.Length * 52 * Enum.GetValues(typeof(PuzzleKind)).Length,
            "weekly seeds are distinct across gardens, weeks and kinds");
    }

    // ---- per kind ------------------------------------------------------------------------

    private static void ProveKind(PuzzleKind kind)
    {
        foreach (int garden in Gardens)
        {
            string previous = null;
            for (int week = 1; week <= 52; week++)
            {
                int seed = WeeklyBoard.SeedFor(garden, kind, WeeklyBoard.Week(2026, week));
                string signature = Prove(kind, seed);
                boards++;
                if (previous != null)
                    Expect(signature != previous, kind + " adjacent weeks differ (garden " + garden + ", week " + week + ")");
                if (garden == Gardens[0] && week % 4 == 1)
                    Expect(Prove(kind, seed) == signature, kind + " same inputs give the same board (week " + week + ")");
                previous = signature;
            }
        }
    }

    private static string Prove(PuzzleKind kind, int seed)
    {
        switch (kind)
        {
            case PuzzleKind.Water: return ProveWater(seed);
            case PuzzleKind.Sudoku: return ProveSudoku(seed);
            case PuzzleKind.Sums: return ProveSums(seed);
            case PuzzleKind.Plots: return ProvePlots(seed);
            case PuzzleKind.StreamPath: return ProveStreamPath(seed);
            case PuzzleKind.Doorstep: return ProveDoorstep(seed);
            default: throw new Exception("no uniqueness proof for " + kind);
        }
    }

    // Water: the generator's own public count (PipeRules.CountSolutions) at the top tier.
    private static string ProveWater(int seed)
    {
        Puzzle board = TreePuzzles.For(seed, WeeklyBoard.Tier, "weekly");
        Expect(board.Size == 4, "top water tier is 4x4");
        Expect(PipeRules.ValidRotations(board, board.Initial), "water starts valid");
        Expect(PipeRules.IsComplete(board, board.Solution), "water answer completes the pipes");
        Expect(PipeRules.CountSolutions(board, 2) == 1, "water has exactly one solution");
        return Sig(board.Initial, board.Solution);
    }

    // Sudoku: an independent count of full grids that match the givens (capped at 2).
    private static string ProveSudoku(int seed)
    {
        var game = new SeedSudokuGame(seed, WeeklyBoard.Tier);
        int n = game.Size, cells = game.Cells;
        Expect(n == 6 && game.BoxRows == 2 && game.BoxColumns == 3, "top sudoku is 6x6 in 2x3 boxes");
        var givens = new int[cells];
        var solution = new int[cells];
        for (int c = 0; c < cells; c++)
        {
            givens[c] = game.Given(c);
            solution[c] = game.Solution(c);
        }
        Expect(new SudokuCount(n, game.BoxRows, game.BoxColumns, givens).Run(2) == 1, "sudoku has exactly one solution");
        Expect(SudokuFull(n, game.BoxRows, game.BoxColumns, solution), "sudoku answer is a full valid grid");
        for (int c = 0; c < cells; c++)
            Expect(givens[c] == 0 || givens[c] == solution[c], "sudoku answer keeps the givens");
        return Sig(givens, solution);
    }

    // Sums: the clue runs are read back from the public clues; a fill counts when every run sums
    // to its clue with distinct digits 1 to 9 (capped at 2 fills).
    private static string ProveSums(int seed)
    {
        var game = new SeedSumsGame(seed, WeeklyBoard.Tier);
        int w = game.Width;
        Expect(w == game.Height && w == 6, "top sums field is 6x6");
        int cells = w * w;
        var runCells = new List<int[]>();
        var runClue = new List<int>();
        var across = new int[cells];
        var down = new int[cells];
        for (int i = 0; i < cells; i++) { across[i] = -1; down[i] = -1; }
        for (int c = 0; c < cells; c++)
        {
            int r = c / w, col = c % w;
            if (game.ClueAcross(c) > 0)
            {
                var run = new List<int>();
                for (int x = col + 1; x < w && game.IsOpen(r * w + x); x++) run.Add(r * w + x);
                AddRun(runCells, runClue, run, game.ClueAcross(c), across);
            }
            if (game.ClueDown(c) > 0)
            {
                var run = new List<int>();
                for (int y = r + 1; y < w && game.IsOpen(y * w + col); y++) run.Add(y * w + col);
                AddRun(runCells, runClue, run, game.ClueDown(c), down);
            }
        }
        var solution = new int[cells];
        var open = new bool[cells];
        for (int i = 0; i < cells; i++)
        {
            open[i] = game.IsOpen(i);
            solution[i] = open[i] ? game.Solution(i) : 0;
            if (open[i])
            {
                Expect(across[i] >= 0 && down[i] >= 0, "every open sums cell lies in an across and a down run");
            }
        }
        // Each run's answer: distinct digits 1..9 summing to its clue.
        for (int k = 0; k < runCells.Count; k++)
        {
            int sum = 0, mask = 0;
            foreach (int c in runCells[k])
            {
                int d = solution[c];
                Expect(d >= 1 && d <= 9 && (mask & (1 << d)) == 0, "sums answer digits are distinct 1..9");
                mask |= 1 << d;
                sum += d;
            }
            Expect(sum == runClue[k], "sums answer meets its clue");
        }
        int fills = new SumsCount(cells, open, runCells, runClue).Run(2);
        Expect(fills == 1, "sums has exactly one fill");
        return Sig(ToInts(open), runClue.ToArray(), solution);
    }

    private static void AddRun(List<int[]> cells, List<int> clue, List<int> run, int value, int[] index)
    {
        Expect(run.Count >= 2, "a sums clue has at least two cells");
        int id = cells.Count;
        cells.Add(run.ToArray());
        clue.Add(value);
        foreach (int c in run) index[c] = id;
    }

    // Plots: a tiling of the field by rectangles, each holding exactly one marker and of area equal
    // to that marker's clue (capped at 2 tilings).
    private static string ProvePlots(int seed)
    {
        var game = new PlotsGame(seed, WeeklyBoard.Tier);
        int size = game.Size;
        Expect(size == 7, "top plots field is 7x7");
        int[] markers = game.Markers;
        var clue = new int[size * size];
        foreach (int m in markers) clue[m] = game.Clue(m);
        foreach (int m in markers) Expect(clue[m] >= 1, "a plot clue is a bed area");
        Expect(new PlotTiling(size, clue).Run(2) == 1, "plots has exactly one tiling");
        return Sig(markers, clue);
    }

    // StreamPath: an independent count of Hamiltonian paths from the spring to the pond over the
    // open meadow, with reachability pruning, capped at 2 and a fixed node budget.
    private static string ProveStreamPath(int seed)
    {
        var game = new StreamPathGame(seed, WeeklyBoard.Tier);
        int n = game.Size, cells = n * n;
        Expect(n == 6, "top meadow is 6x6");
        var rock = new bool[cells];
        int open = 0;
        for (int c = 0; c < cells; c++) { rock[c] = game.IsRock(c); if (!rock[c]) open++; }
        Expect(open == game.OpenCount, "meadow open count agrees");
        int[] path = game.SolutionPath();
        Expect(path.Length == open && path[0] == game.Spring && path[path.Length - 1] == game.Pond,
            "meadow answer runs spring to pond over every open field");
        var seen = new bool[cells];
        for (int i = 0; i < path.Length; i++)
        {
            Expect(!rock[path[i]] && !seen[path[i]], "meadow answer visits each open field once");
            seen[path[i]] = true;
            if (i > 0) Expect(Adjacent(n, path[i - 1], path[i]), "meadow answer steps touch");
        }
        int paths = new PathCount(n, rock, game.Spring, game.Pond).Run(2);
        Expect(paths == 1, "meadow has exactly one path");
        return Sig(ToInts(rock), new[] { game.Spring, game.Pond }, path);
    }

    // Doorstep: the answer is constant on each symmetry class, and each class holds exactly one
    // given. A class's answer is then forced by its given, so the solution is unique.
    private static string ProveDoorstep(int seed)
    {
        var game = new DoorstepGame(seed, WeeklyBoard.Tier);
        int n = game.Size, cells = n * n;
        Expect(n == 8, "top doorstep grid is 8x8");
        var givens = new int[cells];
        var solution = new int[cells];
        for (int c = 0; c < cells; c++)
        {
            solution[c] = game.Solution(c) ? 1 : 0;
            givens[c] = game.IsGiven(c) ? 1 : 0;
        }
        for (int c = 0; c < cells; c++)
        {
            int[] orbit = game.Orbit(c);
            int found = 0, given = -1;
            foreach (int m in orbit)
                if (game.IsGiven(m)) { found++; given = m; }
            Expect(found == 1, "each symmetry class holds exactly one given");
            foreach (int m in orbit)
                Expect(solution[m] == solution[given], "the answer is constant on a symmetry class");
        }
        return Sig(givens, solution);
    }

    // ---- independent counters ------------------------------------------------------------

    private sealed class SudokuCount
    {
        private readonly int n, boxRows, boxColumns;
        private readonly int[] grid;
        private int count, limit;

        public SudokuCount(int n, int boxRows, int boxColumns, int[] givens)
        {
            this.n = n; this.boxRows = boxRows; this.boxColumns = boxColumns;
            grid = (int[])givens.Clone();
        }

        public int Run(int cap)
        {
            limit = cap;
            Fill(0);
            return count;
        }

        private void Fill(int cell)
        {
            if (count >= limit) return;
            if (cell == grid.Length) { count++; return; }
            if (grid[cell] != 0)
            {
                if (Legal(cell, grid[cell])) Fill(cell + 1);
                return;
            }
            for (int d = 1; d <= n && count < limit; d++)
            {
                if (!Legal(cell, d)) continue;
                grid[cell] = d;
                Fill(cell + 1);
                grid[cell] = 0;
            }
        }

        private bool Legal(int cell, int d)
        {
            int row = cell / n, col = cell % n;
            int box = (row / boxRows) * (n / boxColumns) + col / boxColumns;
            for (int j = 0; j < grid.Length; j++)
            {
                if (j == cell || grid[j] != d) continue;
                int r = j / n, c = j % n;
                if (r == row || c == col || (r / boxRows) * (n / boxColumns) + c / boxColumns == box) return false;
            }
            return true;
        }
    }

    private static bool SudokuFull(int n, int boxRows, int boxColumns, int[] grid)
    {
        for (int cell = 0; cell < grid.Length; cell++)
        {
            int d = grid[cell];
            if (d < 1 || d > n) return false;
            int row = cell / n, col = cell % n;
            for (int j = 0; j < grid.Length; j++)
            {
                if (j == cell || grid[j] != d) continue;
                int r = j / n, c = j % n;
                bool box = r / boxRows == row / boxRows && c / boxColumns == col / boxColumns;
                if (r == row || c == col || box) return false;
            }
        }
        return true;
    }

    private sealed class SumsCount
    {
        private readonly int[] order;
        private readonly List<int[]> runCells;
        private readonly int[] clue, runSum, runMask, lastCell, filled;
        private readonly List<int>[] runsOf;
        private int count, limit;

        public SumsCount(int cells, bool[] open, List<int[]> runCells, List<int> runClue)
        {
            this.runCells = runCells;
            var list = new List<int>();
            for (int i = 0; i < cells; i++) if (open[i]) list.Add(i);
            order = list.ToArray();
            clue = runClue.ToArray();
            runSum = new int[clue.Length];
            runMask = new int[clue.Length];
            lastCell = new int[clue.Length];
            filled = new int[clue.Length];
            runsOf = new List<int>[cells];
            for (int i = 0; i < cells; i++) runsOf[i] = new List<int>();
            for (int k = 0; k < clue.Length; k++)
            {
                lastCell[k] = -1;
                foreach (int c in runCells[k])
                {
                    runsOf[c].Add(k);
                    if (c > lastCell[k]) lastCell[k] = c;
                }
            }
        }

        public int Run(int cap)
        {
            limit = cap;
            Fill(0);
            return count;
        }

        private void Fill(int index)
        {
            if (count >= limit) return;
            if (index == order.Length) { count++; return; }
            int cell = order[index];
            for (int d = 1; d <= 9 && count < limit; d++)
            {
                if (!Fits(cell, d)) continue;
                Apply(cell, d, 1);
                if (Completes(cell) && Reachable(cell)) Fill(index + 1);
                Apply(cell, d, -1);
            }
        }

        private bool Fits(int cell, int d)
        {
            foreach (int k in runsOf[cell])
                if ((runMask[k] & (1 << d)) != 0 || runSum[k] + d > clue[k]) return false;
            return true;
        }

        private void Apply(int cell, int d, int sign)
        {
            foreach (int k in runsOf[cell])
            {
                runSum[k] += sign * d;
                runMask[k] ^= 1 << d;
                filled[k] += sign;
            }
        }

        // The run's open cells left can still reach its clue with distinct free digits.
        private bool Reachable(int cell)
        {
            foreach (int k in runsOf[cell])
            {
                int left = runCells[k].Length - filled[k];
                int need = clue[k] - runSum[k];
                if (left == 0) { if (need != 0) return false; continue; }
                int low = 0, high = 0, taken = 0;
                for (int d = 1; d <= 9 && taken < left; d++)
                    if ((runMask[k] & (1 << d)) == 0) { low += d; taken++; }
                if (taken < left || need < low) return false;
                taken = 0;
                for (int d = 9; d >= 1 && taken < left; d--)
                    if ((runMask[k] & (1 << d)) == 0) { high += d; taken++; }
                if (need > high) return false;
            }
            return true;
        }

        private bool Completes(int cell)
        {
            foreach (int k in runsOf[cell])
                if (lastCell[k] == cell && runSum[k] != clue[k]) return false;
            return true;
        }
    }

    private sealed class PlotTiling
    {
        private readonly int size;
        private readonly int[] clue;
        private readonly bool[] covered;
        private int count, limit;

        public PlotTiling(int size, int[] clue)
        {
            this.size = size;
            this.clue = clue;
            covered = new bool[size * size];
        }

        public int Run(int cap)
        {
            limit = cap;
            Tile(0);
            return count;
        }

        private void Tile(int from)
        {
            if (count >= limit) return;
            int first = from;
            while (first < covered.Length && covered[first]) first++;
            if (first == covered.Length) { count++; return; }
            int top = first / size, left = first % size;
            for (int h = 1; top + h <= size; h++)
                for (int w = 1; left + w <= size; w++)
                {
                    if (!Free(top, left, w, h)) break;
                    int markers = 0, markerClue = 0;
                    for (int r = top; r < top + h; r++)
                        for (int c = left; c < left + w; c++)
                            if (clue[r * size + c] != 0) { markers++; markerClue = clue[r * size + c]; }
                    if (markers != 1 || markerClue != w * h) continue;
                    Mark(top, left, w, h, true);
                    Tile(first + 1);
                    Mark(top, left, w, h, false);
                    if (count >= limit) return;
                }
        }

        private bool Free(int top, int left, int w, int h)
        {
            for (int r = top; r < top + h; r++)
                for (int c = left; c < left + w; c++)
                    if (covered[r * size + c]) return false;
            return true;
        }

        private void Mark(int top, int left, int w, int h, bool value)
        {
            for (int r = top; r < top + h; r++)
                for (int c = left; c < left + w; c++)
                    covered[r * size + c] = value;
        }
    }

    private sealed class PathCount
    {
        private readonly int n;
        private readonly bool[] rock, visited;
        private readonly int spring, pond, open;
        private int count;
        private long budget = 2000000;
        private bool overrun;

        public PathCount(int n, bool[] rock, int spring, int pond)
        {
            this.n = n; this.rock = rock; this.spring = spring; this.pond = pond;
            visited = new bool[rock.Length];
            int o = 0;
            foreach (bool r in rock) if (!r) o++;
            open = o;
        }

        public int Run(int cap)
        {
            visited[spring] = true;
            Walk(spring, 1, cap);
            return overrun ? 0 : count;
        }

        private void Walk(int head, int depth, int cap)
        {
            if (overrun || count >= cap) return;
            if (--budget < 0) { overrun = true; return; }
            if (depth == open)
            {
                if (head == pond) count++;
                return;
            }
            if (head == pond) return;
            if (!Reachable(head)) return;
            foreach (int next in Neighbours(head))
            {
                if (rock[next] || visited[next]) continue;
                if (next == pond && depth + 1 != open) continue;
                visited[next] = true;
                Walk(next, depth + 1, cap);
                visited[next] = false;
                if (overrun || count >= cap) return;
            }
        }

        // Every unvisited open field, and the pond, must be reachable from the head.
        private bool Reachable(int head)
        {
            var seen = new bool[rock.Length];
            var stack = new Stack<int>();
            stack.Push(head);
            seen[head] = true;
            int reached = 0, needed = 0;
            for (int c = 0; c < rock.Length; c++) if (!rock[c] && !visited[c]) needed++;
            while (stack.Count > 0)
            {
                int cell = stack.Pop();
                foreach (int next in Neighbours(cell))
                {
                    if (rock[next] || seen[next]) continue;
                    if (visited[next] && next != pond) continue;
                    seen[next] = true;
                    if (!visited[next]) reached++;
                    stack.Push(next);
                }
            }
            return reached >= needed;
        }

        private IEnumerable<int> Neighbours(int cell)
        {
            int r = cell / n, c = cell % n;
            if (r > 0) yield return cell - n;
            if (r < n - 1) yield return cell + n;
            if (c > 0) yield return cell - 1;
            if (c < n - 1) yield return cell + 1;
        }
    }

    private static bool Adjacent(int n, int a, int b)
    {
        int d = Math.Abs(a - b);
        return (d == 1 && a / n == b / n) || d == n;
    }

    // ---- fingerprints ----------------------------------------------------------------------

    private static int[] ToInts(bool[] values)
    {
        var result = new int[values.Length];
        for (int i = 0; i < values.Length; i++) result[i] = values[i] ? 1 : 0;
        return result;
    }

    private static string Sig(params int[][] parts)
    {
        var sb = new StringBuilder();
        foreach (int[] part in parts)
        {
            foreach (int v in part) sb.Append(v).Append(',');
            sb.Append('|');
        }
        return sb.ToString();
    }
}
