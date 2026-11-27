using System;
using System.Collections.Generic;

namespace Evergrove.Core
{
    /// <summary>Outcome of placing or clearing a seed count in a Seed Sums cell.</summary>
    public enum SumMove { Ignored, Placed, Cleared, Conflict }

    /// <summary>
    /// Seed Sums: a sum-cross puzzle (the UI paints digit d as d seeds or as a numeral).
    /// The board is a small grid of open cells and clue cells. Every maximal run of two or
    /// more open cells, across or down, has a clue: the sum of its digits. Fill the open
    /// cells with 1..9 so every run adds up to its clue and no digit repeats within a run.
    /// Tier 0 has a 3x3 open area, tier 1 4x4 and tier 2 5x5, each with one clue row on top
    /// and one clue column on the left (so Width = Height = area + 1). Every board has exactly
    /// one solution and is finished by simple deduction alone: digit combinations that fit a
    /// clue, digits a run needs that have only one place, and elimination of a settled digit
    /// from the rest of its run. No guessing, no timer, no penalty. Dealt from a seed, so the
    /// same seed and tier always give the same board. Cell index = row * Width + column.
    /// </summary>
    public sealed class SeedSumsGame
    {
        private const int AllDigits = 0x3FE;

        private sealed class Run
        {
            public int Clue;
            public int[] Cells;
        }

        // Digit sets (bit d = digit d) of a given length and sum, built once.
        private static readonly int[][][] Combos = BuildCombos();

        private readonly int width, height;
        private readonly bool[] open;
        private readonly int[] solution;
        private readonly int[] values;
        private readonly int[] clueAcross, clueDown;
        private readonly Run[] runs;
        private readonly int[] acrossRun, downRun;
        private int[] acrossIndex, downIndex; // generation scratch: run index of each open cell

        public int Width { get { return width; } }
        public int Height { get { return height; } }
        public int Moves { get; private set; }

        /// <summary>True when simple deduction alone reaches the solution (always, for dealt boards).</summary>
        public bool SolvableByDeduction { get; private set; }

        /// <summary>
        /// Raw difficulty: how many rounds of deduction (every clue run processed in turn) change
        /// something before the board is settled. A round settles what the last one exposed, so a
        /// board that needs more rounds needs a longer chain of reasoning from run to run.
        /// </summary>
        public int DeductionPasses { get; private set; }

        /// <summary>Depth leaves 1..3, from DeductionPasses (see LeavesFor).</summary>
        public int Leaves { get; private set; }

        // Cut points chosen from seeds 1..300 over all tiers: rounds up to LeafCut1 are 1 leaf,
        // up to LeafCut2 are 2 leaves, more are 3.
        private const int LeafCut1 = 3, LeafCut2 = 5;

        /// <summary>The leaves a raw pass count earns.</summary>
        public static int LeavesFor(int passes) { return passes <= LeafCut1 ? 1 : passes <= LeafCut2 ? 2 : 3; }

        public SeedSumsGame(int seed, int tier)
        {
            if (tier < 0 || tier >= TreePuzzles.Tiers) throw new ArgumentOutOfRangeException(nameof(tier));
            int area = 3 + tier;
            width = height = area + 1;
            uint state = unchecked((uint)seed * 0x9E3779B1u + (uint)tier * 0x85EBCA6Bu + 0x165667B1u);
            if (state == 0) state = 0x6d2b79f5u;
            var random = new Pick(state);

            int cells = width * height;
            open = new bool[cells];
            solution = new int[cells];
            while (true)
            {
                if (!TryLayout(random, area, tier)) continue;
                var found = BuildRuns();
                if (found == null) continue;
                FillSolution(found, random);
                if (!Polish(found, random)) continue;
                runs = found.ToArray();
                break;
            }
            SolvableByDeduction = true;
            DeductionPasses = CountPasses();
            Leaves = LeavesFor(DeductionPasses);
            values = new int[cells];
            clueAcross = new int[cells];
            clueDown = new int[cells];
            acrossRun = new int[cells];
            downRun = new int[cells];
            for (int i = 0; i < cells; i++) { acrossRun[i] = -1; downRun[i] = -1; }
            for (int r = 0; r < runs.Length; r++)
            {
                var run = runs[r];
                bool across = run.Cells[1] == run.Cells[0] + 1;
                int before = run.Cells[0] - (across ? 1 : width);
                if (across) clueAcross[before] = run.Clue; else clueDown[before] = run.Clue;
                foreach (int c in run.Cells) { if (across) acrossRun[c] = r; else downRun[c] = r; }
            }
        }

        public bool IsOpen(int cell) { return InRange(cell) && open[cell]; }
        public int Value(int cell) { return InRange(cell) ? values[cell] : 0; }
        public int Solution(int cell) { return InRange(cell) ? solution[cell] : 0; }

        /// <summary>The across clue a clue cell shows (the run to its right), or 0 when it has none.</summary>
        public int ClueAcross(int cell) { return InRange(cell) ? clueAcross[cell] : 0; }

        /// <summary>The down clue a clue cell shows (the run below it), or 0 when it has none.</summary>
        public int ClueDown(int cell) { return InRange(cell) ? clueDown[cell] : 0; }

        public bool Solved
        {
            get
            {
                for (int i = 0; i < values.Length; i++) if (open[i] && values[i] != solution[i]) return false;
                return true;
            }
        }

        /// <summary>
        /// Put a digit (1..9) in an open cell, or clear it (0). Wrong digits are allowed: a digit
        /// that repeats in its run, overshoots a clue, or completes a run with the wrong total is
        /// still placed but reported as Conflict. Clue cells, out-of-range cells, bad digits,
        /// clearing an empty cell and a solved board are ignored.
        /// </summary>
        public SumMove Place(int cell, int digit)
        {
            if (Solved || !IsOpen(cell) || digit < 0 || digit > 9) return SumMove.Ignored;
            if (digit == 0 && values[cell] == 0) return SumMove.Ignored;
            values[cell] = digit;
            Moves++;
            if (digit == 0) return SumMove.Cleared;
            return Conflicts(cell) ? SumMove.Conflict : SumMove.Placed;
        }

        /// <summary>
        /// True when the cell's digit repeats in a run, a run's digits already pass its clue, or a
        /// full run misses its clue. A gentle flag, never a penalty.
        /// </summary>
        public bool Conflicts(int cell)
        {
            if (!IsOpen(cell) || values[cell] == 0) return false;
            return RunConflicts(runs[acrossRun[cell]]) || RunConflicts(runs[downRun[cell]]);
        }

        /// <summary>
        /// A cell and digit the player can settle next: first any wrong digit (with its right
        /// answer), then an empty cell that deduction from the clues and the digits already placed
        /// settles. False (cell -1, digit 0) when solved.
        /// </summary>
        public bool Hint(out int cell, out int digit)
        {
            cell = -1; digit = 0;
            if (Solved) return false;
            for (int i = 0; i < values.Length; i++)
                if (open[i] && values[i] != 0 && values[i] != solution[i]) { cell = i; digit = solution[i]; return true; }
            var cand = StartCandidates();
            for (int i = 0; i < values.Length; i++) if (open[i] && values[i] != 0) cand[i] = 1 << values[i];
            var list = new List<Run>(runs);
            int found = -1;
            Deduce(list, cand, values, f => found = f);
            if (found < 0)
                for (int i = 0; i < values.Length && found < 0; i++) if (open[i] && values[i] == 0) found = i;
            cell = found; digit = solution[found];
            return true;
        }

        private bool InRange(int cell) { return cell >= 0 && cell < solution.Length; }

        private bool RunConflicts(Run run)
        {
            int sum = 0, seen = 0, filled = 0;
            foreach (int c in run.Cells)
            {
                int v = values[c];
                if (v == 0) continue;
                if ((seen & (1 << v)) != 0) return true;
                seen |= 1 << v; sum += v; filled++;
            }
            return sum > run.Clue || (filled == run.Cells.Length && sum != run.Clue);
        }

        // ---- generation ----

        // Random black cells inside the open area; every open cell must sit in a run of two or
        // more both across and down (checked by BuildRuns).
        private bool TryLayout(Pick random, int area, int tier)
        {
            int blackChance = tier == 0 ? 5 : 4; // one in blackChance interior cells
            int count = 0;
            for (int r = 0; r < height; r++)
                for (int c = 0; c < width; c++)
                {
                    bool isOpen = r > 0 && c > 0 && random.Next(blackChance) != 0;
                    open[r * width + c] = isOpen;
                    if (isOpen) count++;
                }
            // Keep most of the area open, so the tier reads at its size.
            return count >= (area * area * 2 + 2) / 3;
        }

        private List<Run> BuildRuns()
        {
            var list = new List<Run>();
            int[] across = new int[width * height], down = new int[width * height];
            for (int dir = 0; dir < 2; dir++)
            {
                bool isAcross = dir == 0;
                int lines = isAcross ? height : width, length = isAcross ? width : height;
                for (int line = 0; line < lines; line++)
                {
                    int i = 0;
                    while (i < length)
                    {
                        if (!open[CellAt(isAcross, line, i)]) { i++; continue; }
                        var cells = new List<int>();
                        while (i < length && open[CellAt(isAcross, line, i)]) { cells.Add(CellAt(isAcross, line, i)); i++; }
                        if (cells.Count < 2) return null;
                        foreach (int c in cells) { if (isAcross) across[c]++; else down[c]++; }
                        list.Add(new Run { Cells = cells.ToArray() });
                    }
                }
            }
            return list.Count == 0 ? null : list;
        }

        private int CellAt(bool across, int line, int i) { return across ? line * width + i : i * width + line; }

        // Random digits, none repeated within a run so far. A cell sees at most 8 used digits
        // (4 across, 4 down), so a free digit always exists. Extremes are favoured: their runs
        // have few digit combinations, which keeps boards deducible.
        private void FillSolution(List<Run> list, Pick random)
        {
            acrossIndex = new int[width * height];
            downIndex = new int[width * height];
            for (int r = 0; r < list.Count; r++)
            {
                bool across = list[r].Cells[1] == list[r].Cells[0] + 1;
                foreach (int c in list[r].Cells) { if (across) acrossIndex[c] = r; else downIndex[c] = r; }
            }
            var used = new int[list.Count];
            for (int i = 0; i < solution.Length; i++)
            {
                solution[i] = 0;
                if (!open[i]) continue;
                int taken = used[acrossIndex[i]] | used[downIndex[i]];
                int d;
                do { d = Weighted(random); } while ((taken & (1 << d)) != 0);
                solution[i] = d;
                used[acrossIndex[i]] |= 1 << d;
                used[downIndex[i]] |= 1 << d;
            }
        }

        private static readonly int[] Pool = { 1, 1, 2, 2, 3, 4, 5, 6, 7, 8, 8, 9, 9 };

        private static int Weighted(Pick random) { return Pool[random.Next(Pool.Length)]; }

        private void SetClues(List<Run> list)
        {
            foreach (var run in list)
            {
                int sum = 0;
                foreach (int c in run.Cells) sum += solution[c];
                run.Clue = sum;
            }
        }

        // ---- deduction ----

        private int[] StartCandidates()
        {
            var cand = new int[width * height];
            for (int i = 0; i < cand.Length; i++) if (open[i]) cand[i] = AllDigits;
            return cand;
        }

        // Simple deduction to a fixpoint; returns how many surplus candidate digits remain
        // across the open cells (0 = solved). A contradiction cannot happen while the clues come from a real
        // solution. When values and onSingle are given, stops as soon as an empty cell
        // (value 0) is down to one digit and reports it.
        private int Deduce(List<Run> list, int[] cand, int[] values, Action<int> onSingle)
        {
            bool progress = true;
            while (progress)
            {
                progress = false;
                foreach (var run in list)
                {
                    int change = Process(run, cand);
                    if (change > 0) progress = true;
                    if (onSingle != null)
                        for (int i = 0; i < cand.Length; i++)
                            if (open[i] && values[i] == 0 && IsSingle(cand[i])) { onSingle(i); return 1; }
                }
            }
            int undecided = 0;
            for (int i = 0; i < cand.Length; i++)
                if (open[i]) for (int m = cand[i] & (cand[i] - 1); m != 0; m &= m - 1) undecided++;
            return undecided;
        }

        // Hill-climb from a random solution towards a board that deduction finishes: change a
        // digit (preferring undecided cells) and keep the change when no more cells are left
        // undecided. Clues always follow the current digits, so every step is a valid board.
        private bool Polish(List<Run> list, Pick random)
        {
            SetClues(list);
            int score = Deduce(list, StartCandidates(), null, null);
            int[] cand = null;
            for (int step = 0; step < 250 && score > 0; step++)
            {
                if (cand == null || random.Next(3) == 0)
                {
                    cand = StartCandidates();
                    Deduce(list, cand, null, null);
                }
                int cell = -1;
                for (int tries = 0; tries < 12 && cell < 0; tries++)
                {
                    int pick = random.Next(solution.Length);
                    if (open[pick] && (random.Next(4) != 0 ? !IsSingle(cand[pick]) : true)) cell = pick;
                }
                if (cell < 0) continue;
                int old = solution[cell];
                int taken = 0;
                foreach (int c in list[acrossIndex[cell]].Cells) if (c != cell) taken |= 1 << solution[c];
                foreach (int c in list[downIndex[cell]].Cells) if (c != cell) taken |= 1 << solution[c];
                int free = AllDigits & ~taken & ~(1 << old);
                if (free == 0) continue;
                int d;
                do { d = Weighted(random); } while ((free & (1 << d)) == 0);
                solution[cell] = d;
                SetClues(list);
                int next = Deduce(list, StartCandidates(), null, null);
                if (next <= score) { score = next; cand = null; }
                else { solution[cell] = old; SetClues(list); }
            }
            return score == 0;
        }

        // One run: keep only digits of combinations that still fit, remove settled digits from
        // the run's other cells, and settle a digit every combination needs that has one place.
        // Returns -1 on contradiction, 1 if anything changed, else 0.
        private static int Process(Run run, int[] cand)
        {
            int[] cells = run.Cells;
            int union = 0;
            foreach (int c in cells) union |= cand[c];
            int allowed = 0, required = AllDigits;
            bool any = false;
            int[] masks = run.Clue < Combos[cells.Length].Length ? Combos[cells.Length][run.Clue] : null;
            if (masks != null)
                foreach (int mask in masks)
                {
                    if ((mask & ~union) != 0) continue;
                    allowed |= mask; required &= mask; any = true;
                }
            if (!any) return -1;
            bool changed = false;
            foreach (int c in cells)
            {
                int next = cand[c] & allowed;
                if (next == 0) return -1;
                if (next != cand[c]) { cand[c] = next; changed = true; }
            }
            foreach (int c in cells)
            {
                if (!IsSingle(cand[c])) continue;
                foreach (int o in cells)
                {
                    if (o == c || (cand[o] & cand[c]) == 0) continue;
                    cand[o] &= ~cand[c];
                    changed = true;
                    if (cand[o] == 0) return -1;
                }
            }
            for (int d = 1; d <= 9; d++)
            {
                if ((required & (1 << d)) == 0) continue;
                int holder = -1, count = 0;
                foreach (int c in cells) if ((cand[c] & (1 << d)) != 0) { holder = c; count++; }
                if (count == 0) return -1;
                if (count == 1 && cand[holder] != (1 << d)) { cand[holder] = 1 << d; changed = true; }
            }
            return changed ? 1 : 0;
        }

        // Rounds of Process over every run, from no knowledge, that still changed something.
        private int CountPasses()
        {
            var cand = StartCandidates();
            int passes = 0;
            bool progress = true;
            while (progress)
            {
                progress = false;
                foreach (var run in runs) if (Process(run, cand) > 0) progress = true;
                if (progress) passes++;
            }
            return passes;
        }

        private static bool IsSingle(int mask) { return mask != 0 && (mask & (mask - 1)) == 0; }

        // Combos[length][sum] = digit sets with that many distinct digits adding to the sum.
        private static int[][][] BuildCombos()
        {
            var table = new List<int>[10][];
            for (int len = 0; len < 10; len++)
            {
                table[len] = new List<int>[46];
                for (int s = 0; s < 46; s++) table[len][s] = new List<int>();
            }
            for (int mask = 2; mask < 1024; mask += 2)
            {
                int len = 0, sum = 0;
                for (int d = 1; d <= 9; d++) if ((mask & (1 << d)) != 0) { len++; sum += d; }
                table[len][sum].Add(mask);
            }
            var result = new int[10][][];
            for (int len = 0; len < 10; len++)
            {
                result[len] = new int[46][];
                for (int s = 0; s < 46; s++) result[len][s] = table[len][s].ToArray();
            }
            return result;
        }

        // A tiny deterministic generator (xorshift32): the same on every platform.
        private sealed class Pick
        {
            private uint state;
            public Pick(uint seed) { state = seed == 0 ? 0x6d2b79f5u : seed; }
            public int Next(int below)
            {
                state ^= state << 13;
                state ^= state >> 17;
                state ^= state << 5;
                return (int)(state % (uint)below);
            }
        }
    }
}
