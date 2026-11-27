using System;
using System.Collections.Generic;

namespace Evergrove.Core
{
    /// <summary>Outcome of placing or clearing a plant in a Seed Sudoku cell.</summary>
    public enum SudokuMove { Ignored, Placed, Cleared }

    /// <summary>
    /// Seed Sudoku: sudoku with plants instead of digits (values 1..Size; the UI
    /// paints value v as the v-th plant). Every row, column and box holds each
    /// plant exactly once. Tier 0 is 4x4 (2x2 boxes), tiers 1 and 2 are 6x6
    /// (2-row x 3-column boxes), tier 2 with fewer givens. Every puzzle has exactly
    /// one solution and can be finished by naked and hidden singles alone: no
    /// guessing, no timer, no penalty. Dealt from a seed, so the same seed always
    /// gives the same garden bed. Cell index = row * Size + column.
    /// </summary>
    public sealed class SeedSudokuGame
    {
        private readonly int size, boxRows, boxColumns;
        private readonly int[] solution;
        private readonly int[] givens;
        private readonly int[] values;

        public int Size { get { return size; } }
        public int BoxRows { get { return boxRows; } }
        public int BoxColumns { get { return boxColumns; } }
        public int Cells { get { return solution.Length; } }
        public int Moves { get; private set; }

        /// <summary>
        /// Raw difficulty: solving rounds a singles solver needs. A round settles every cell that
        /// is a naked single (one plant left) at the start of the round; when there is none, a
        /// round settles one hidden single (the only place for a plant in a row, column or box).
        /// More rounds mean longer chains where one answer opens the next.
        /// </summary>
        public int SolveRounds { get; private set; }

        /// <summary>
        /// How many rounds had to be a hidden single, because no cell had a single candidate.
        /// 0 means naked singles alone finish the bed.
        /// </summary>
        public int HiddenSinglesNeeded { get; private set; }

        /// <summary>True when even hidden singles leave cells open (a harder technique is needed).</summary>
        public bool NeedsMoreThanSingles { get; private set; }

        /// <summary>
        /// Depth leaves 1..3: a bed that naked singles finish in few rounds is 1, a longer
        /// naked-single chain or any hidden single is 2, a long chain with hidden singles, or a
        /// bed beyond singles, is 3 (see LeavesFor).
        /// </summary>
        public int Leaves { get; private set; }

        // Cut points chosen from seeds 1..300 over all tiers, on SolveRounds.
        private const int LeafCut1 = 3, LeafCut2 = 5;

        /// <summary>The leaves a solve needing this many rounds earns (hidden singles lift it one leaf).</summary>
        public static int LeavesFor(int rounds, int hiddenSingles, bool beyondSingles)
        {
            if (beyondSingles) return 3;
            int leaves = rounds <= LeafCut1 ? 1 : rounds <= LeafCut2 ? 2 : 3;
            if (hiddenSingles > 0) leaves = Math.Max(2, leaves);
            return leaves;
        }

        /// <summary>The seed and tier this board was dealt from; undo rebuilds it from them.</summary>
        public int Seed { get; private set; }
        public int Tier { get; private set; }

        public SeedSudokuGame(int seed, int tier)
        {
            Seed = seed;
            Tier = tier;
            if (tier < 0 || tier >= TreePuzzles.Tiers) throw new ArgumentOutOfRangeException(nameof(tier));
            size = tier == 0 ? 4 : 6;
            boxRows = 2;
            boxColumns = tier == 0 ? 2 : 3;
            uint state = unchecked((uint)seed * 0x9E3779B1u + (uint)tier * 0x85EBCA6Bu + 0x27D4EB2Fu);
            if (state == 0) state = 0x6d2b79f5u;
            var random = new Pick(state);
            solution = BuildSolution(random);
            int target = tier == 0 ? 8 : tier == 1 ? 16 : 12;
            // A greedy removal pass can stall above the target, so try a few seed-determined
            // orders and keep the sparsest result.
            int[] best = null;
            int bestCount = int.MaxValue;
            for (int attempt = 0; attempt < 40 && bestCount > target; attempt++)
            {
                var trial = (int[])solution.Clone();
                int count = trial.Length;
                foreach (int cell in Shuffled(trial.Length, random))
                {
                    if (count <= target) break;
                    int keep = trial[cell];
                    trial[cell] = 0;
                    if (CountSolutions(trial, 2) == 1 && SolvableBySingles(trial)) count--;
                    else trial[cell] = keep;
                }
                if (count < bestCount) { best = trial; bestCount = count; }
            }
            givens = best;
            values = (int[])givens.Clone();
            AnalyseSingles(givens);
            Leaves = LeavesFor(SolveRounds, HiddenSinglesNeeded, NeedsMoreThanSingles);
        }

        public int Given(int cell) { return InRange(cell) ? givens[cell] : 0; }
        public int Value(int cell) { return InRange(cell) ? values[cell] : 0; }
        public bool IsGiven(int cell) { return InRange(cell) && givens[cell] != 0; }
        public int Solution(int cell) { return InRange(cell) ? solution[cell] : 0; }

        public bool Solved
        {
            get
            {
                for (int i = 0; i < values.Length; i++) if (values[i] != solution[i]) return false;
                return true;
            }
        }

        /// <summary>Plant a value (1..Size) or clear the cell (0). Givens, out-of-range cells and a solved bed are ignored.</summary>
        public SudokuMove Place(int cell, int value)
        {
            if (Solved || !InRange(cell) || givens[cell] != 0 || value < 0 || value > size) return SudokuMove.Ignored;
            values[cell] = value;
            Moves++;
            return value == 0 ? SudokuMove.Cleared : SudokuMove.Placed;
        }

        /// <summary>True when the cell's plant repeats in its row, column or box. A gentle hint, never a penalty.</summary>
        public bool Conflicts(int cell)
        {
            if (!InRange(cell) || values[cell] == 0) return false;
            int r = cell / size, c = cell % size;
            for (int other = 0; other < values.Length; other++)
            {
                if (other == cell || values[other] != values[cell]) continue;
                if (SharesUnit(r, c, other / size, other % size)) return true;
            }
            return false;
        }

        /// <summary>Plants not yet in the cell's row, column or box (empty for out-of-range cells).</summary>
        public int[] Candidates(int cell)
        {
            if (!InRange(cell)) return new int[0];
            var list = new List<int>();
            int mask = CandidateMask(values, cell);
            for (int v = 1; v <= size; v++) if ((mask & (1 << v)) != 0) list.Add(v);
            return list.ToArray();
        }

        /// <summary>
        /// A cell the player can settle next by a single: first any wrong plant, then an empty
        /// cell with one candidate or the only place for a plant in its row, column or box.
        /// -1 when solved.
        /// </summary>
        public int HintCell()
        {
            for (int i = 0; i < values.Length; i++)
                if (values[i] != 0 && values[i] != solution[i]) return i;
            for (int i = 0; i < values.Length; i++)
                if (values[i] == 0 && SingleValue(values, i) != 0) return i;
            for (int i = 0; i < values.Length; i++) if (values[i] == 0) return i;
            return -1;
        }

        private bool InRange(int cell) { return cell >= 0 && cell < solution.Length; }

        private bool SharesUnit(int r1, int c1, int r2, int c2)
        {
            return r1 == r2 || c1 == c2 || (r1 / boxRows == r2 / boxRows && c1 / boxColumns == c2 / boxColumns);
        }

        private int CandidateMask(int[] grid, int cell)
        {
            int r = cell / size, c = cell % size;
            int used = 0;
            for (int other = 0; other < grid.Length; other++)
                if (grid[other] != 0 && other != cell && SharesUnit(r, c, other / size, other % size))
                    used |= 1 << grid[other];
            int all = 0;
            for (int v = 1; v <= size; v++) all |= 1 << v;
            return all & ~used;
        }

        // The value a naked or hidden single fixes in this empty cell, or 0.
        private int SingleValue(int[] grid, int cell)
        {
            int mask = CandidateMask(grid, cell);
            if (mask == 0) return 0;
            if ((mask & (mask - 1)) == 0)
            {
                // Bit v stands for value v, so shifting it down to 1 takes exactly v shifts
                // (b309: this returned v + 1, letting boards that need guessing through).
                int n = 0;
                while ((mask >>= 1) != 0) n++;
                return n;
            }
            int r = cell / size, c = cell % size;
            for (int v = 1; v <= size; v++)
            {
                if ((mask & (1 << v)) == 0) continue;
                bool row = true, col = true, box = true;
                for (int other = 0; other < grid.Length; other++)
                {
                    if (other == cell || grid[other] != 0) continue;
                    if ((CandidateMask(grid, other) & (1 << v)) == 0) continue;
                    int orow = other / size, ocol = other % size;
                    if (orow == r) row = false;
                    if (ocol == c) col = false;
                    if (orow / boxRows == r / boxRows && ocol / boxColumns == c / boxColumns) box = false;
                }
                if (row || col || box) return v;
            }
            return 0;
        }

        private bool SolvableBySingles(int[] puzzle)
        {
            var grid = (int[])puzzle.Clone();
            bool progress = true;
            while (progress)
            {
                progress = false;
                for (int i = 0; i < grid.Length; i++)
                {
                    if (grid[i] != 0) continue;
                    int v = SingleValue(grid, i);
                    if (v != 0) { grid[i] = v; progress = true; }
                }
            }
            for (int i = 0; i < grid.Length; i++) if (grid[i] == 0) return false;
            return true;
        }

        // Solve by singles, naked ones first, and record the rounds, the hidden singles used and
        // whether singles run out before the bed is full.
        private void AnalyseSingles(int[] puzzle)
        {
            var grid = (int[])puzzle.Clone();
            int rounds = 0, hidden = 0;
            while (true)
            {
                int empty = 0;
                var masks = new int[grid.Length];
                var settle = new List<int>();
                for (int i = 0; i < grid.Length; i++)
                {
                    if (grid[i] != 0) continue;
                    empty++;
                    masks[i] = CandidateMask(grid, i);
                    if (masks[i] != 0 && (masks[i] & (masks[i] - 1)) == 0) settle.Add(i);
                }
                if (empty == 0) break;
                if (settle.Count > 0)
                {
                    foreach (int cell in settle)
                    {
                        int value = 1;
                        while ((masks[cell] >> value) != 1) value++;
                        grid[cell] = value;
                    }
                }
                else
                {
                    int cell, value;
                    FindHidden(grid, masks, out cell, out value);
                    if (cell < 0) { NeedsMoreThanSingles = true; break; }
                    grid[cell] = value;
                    hidden++;
                }
                rounds++;
            }
            SolveRounds = rounds;
            HiddenSinglesNeeded = hidden;
        }

        // The first empty cell that is the only place for some value in its row, column or box.
        private void FindHidden(int[] grid, int[] masks, out int cell, out int value)
        {
            cell = -1; value = 0;
            for (int i = 0; i < grid.Length && cell < 0; i++)
            {
                if (grid[i] != 0) continue;
                int r = i / size, c = i % size;
                for (int v = 1; v <= size && cell < 0; v++)
                {
                    if ((masks[i] & (1 << v)) == 0) continue;
                    bool row = true, col = true, box = true;
                    for (int o = 0; o < grid.Length; o++)
                    {
                        if (o == i || grid[o] != 0 || (masks[o] & (1 << v)) == 0) continue;
                        int orow = o / size, ocol = o % size;
                        if (orow == r) row = false;
                        if (ocol == c) col = false;
                        if (orow / boxRows == r / boxRows && ocol / boxColumns == c / boxColumns) box = false;
                    }
                    if (row || col || box) { cell = i; value = v; }
                }
            }
        }

        private int CountSolutions(int[] puzzle, int limit)
        {
            var grid = (int[])puzzle.Clone();
            int found = 0;
            Count(grid, limit, ref found);
            return found;
        }

        private void Count(int[] grid, int limit, ref int found)
        {
            int best = -1, bestMask = 0, bestCount = int.MaxValue;
            for (int i = 0; i < grid.Length; i++)
            {
                if (grid[i] != 0) continue;
                int mask = CandidateMask(grid, i);
                int n = 0;
                for (int m = mask; m != 0; m &= m - 1) n++;
                if (n < bestCount)
                {
                    best = i; bestMask = mask; bestCount = n;
                    if (n == 0) return;
                }
            }
            if (best < 0) { found++; return; }
            for (int v = 1; v <= size && found < limit; v++)
            {
                if ((bestMask & (1 << v)) == 0) continue;
                grid[best] = v;
                Count(grid, limit, ref found);
                grid[best] = 0;
            }
        }

        // A base pattern, then symbol permutation, row swaps within bands, column swaps
        // within stacks, and band/stack swaps: all from the seed's generator.
        private int[] BuildSolution(Pick random)
        {
            int bands = size / boxRows, stacks = size / boxColumns;
            int[] symbol = Shuffled(size, random);
            int[] rowOrder = GroupOrder(bands, boxRows, random);
            int[] colOrder = GroupOrder(stacks, boxColumns, random);
            var grid = new int[size * size];
            for (int r = 0; r < size; r++)
                for (int c = 0; c < size; c++)
                {
                    int rr = rowOrder[r], cc = colOrder[c];
                    int pattern = (boxColumns * (rr % boxRows) + rr / boxRows + cc) % size;
                    grid[r * size + c] = symbol[pattern] + 1;
                }
            return grid;
        }

        private static int[] GroupOrder(int groups, int per, Pick random)
        {
            var order = new List<int>();
            foreach (int g in Shuffled(groups, random))
                foreach (int i in Shuffled(per, random)) order.Add(g * per + i);
            return order.ToArray();
        }

        private static int[] Shuffled(int n, Pick random)
        {
            var a = new int[n];
            for (int i = 0; i < n; i++) a[i] = i;
            for (int i = n - 1; i > 0; i--)
            {
                int j = random.Next(i + 1);
                int swap = a[i]; a[i] = a[j]; a[j] = swap;
            }
            return a;
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
