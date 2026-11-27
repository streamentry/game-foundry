using System;
using System.Collections.Generic;

namespace Evergrove.Core
{
    public enum CellState { Empty, Filled, Marked }

    public enum NonogramMove { Ignored, Filled, Marked, Cleared }

    /// <summary>
    /// Planting by numbers (depth spine, Layer 2): a square bed with a count for
    /// every row and column, the lengths of the runs of plants in order. Plant
    /// the cells the numbers call for to reveal a small picture; mark a cell
    /// "no plant here" to keep track of what you have ruled out. Every bed is
    /// dealt from a seed and kept only if plain line-by-line reasoning solves it,
    /// so no bed ever needs a guess. No timer, no move limit.
    /// </summary>
    public sealed class NonogramGame
    {
        // Line-solver cell knowledge.
        private const int Unknown = 0, Plant = 1, Bare = 2;

        private readonly bool[] solution;
        private readonly CellState[] cells;

        public int Size { get; private set; }
        public int[][] RowClues { get; private set; }
        public int[][] ColumnClues { get; private set; }
        public int Moves { get; private set; }
        public bool Solved { get; private set; }
        /// <summary>Candidate patterns tried before one solved by logic alone (for tests).</summary>
        public int Attempts { get; private set; }

        /// <summary>Cells per side by tier: 5, 6, 8.</summary>
        public static int SizeFor(int tier)
        {
            if (tier < 0 || tier >= TreePuzzles.Tiers) throw new ArgumentOutOfRangeException(nameof(tier));
            return tier == 0 ? 5 : tier == 1 ? 6 : 8;
        }

        public NonogramGame(int seed, int tier)
        {
            Size = SizeFor(tier);
            int n = Size;
            solution = new bool[n * n];
            cells = new CellState[n * n];
            uint state = unchecked((uint)seed * 0x9E3779B1u + (uint)tier * 0x85EBCA6Bu + 0x7F4A7C15u);
            if (state == 0) state = 0x6d2b79f5u;
            for (int attempt = 1; ; attempt++)
            {
                if (attempt > 200000) throw new InvalidOperationException("no logic-solvable nonogram found");
                for (int i = 0; i < solution.Length; i++)
                {
                    state ^= state << 13;
                    state ^= state >> 17;
                    state ^= state << 5;
                    solution[i] = state % 100u < 56u;
                }
                if (!NoEmptyLine()) continue;
                BuildClues();
                var known = new int[n * n];
                if (!Propagate(known, RowClues, ColumnClues, n, null)) continue;
                bool all = true;
                for (int i = 0; i < known.Length; i++) if (known[i] == Unknown) { all = false; break; }
                if (!all) continue;
                Attempts = attempt;
                break;
            }
        }

        private bool NoEmptyLine()
        {
            int n = Size;
            for (int r = 0; r < n; r++)
            {
                bool row = false, col = false;
                for (int k = 0; k < n; k++) { row |= solution[r * n + k]; col |= solution[k * n + r]; }
                if (!row || !col) return false;
            }
            return true;
        }

        private void BuildClues()
        {
            int n = Size;
            RowClues = new int[n][];
            ColumnClues = new int[n][];
            for (int i = 0; i < n; i++)
            {
                var row = new List<int>();
                var col = new List<int>();
                int rr = 0, cr = 0;
                for (int k = 0; k < n; k++)
                {
                    if (solution[i * n + k]) rr++; else if (rr > 0) { row.Add(rr); rr = 0; }
                    if (solution[k * n + i]) cr++; else if (cr > 0) { col.Add(cr); cr = 0; }
                }
                if (rr > 0) row.Add(rr);
                if (cr > 0) col.Add(cr);
                RowClues[i] = row.ToArray();
                ColumnClues[i] = col.ToArray();
            }
        }

        // ---- line solver ----

        // Tighten one line given what is known; false if no placement fits.
        private static bool SolveLine(int[] clue, int[] line)
        {
            int n = line.Length;
            var canPlant = new bool[n];
            var canBare = new bool[n];
            var placement = new int[n];
            bool any = false;
            Place(clue, 0, 0, line, placement, canPlant, canBare, ref any);
            if (!any) return false;
            for (int i = 0; i < n; i++)
            {
                if (canPlant[i] && !canBare[i]) line[i] = Plant;
                else if (!canPlant[i] && canBare[i]) line[i] = Bare;
            }
            return true;
        }

        private static void Place(int[] clue, int k, int from, int[] line, int[] cur,
            bool[] canPlant, bool[] canBare, ref bool any)
        {
            int n = line.Length;
            if (k == clue.Length)
            {
                for (int i = from; i < n; i++) if (line[i] == Plant) return;
                for (int i = from; i < n; i++) cur[i] = Bare;
                any = true;
                for (int i = 0; i < n; i++) { if (cur[i] == Plant) canPlant[i] = true; else canBare[i] = true; }
                return;
            }
            int rest = 0;
            for (int j = k + 1; j < clue.Length; j++) rest += clue[j] + 1;
            for (int start = from; start + clue[k] + rest <= n; start++)
            {
                // A known plant before this run's start would be skipped over: no later start can fix that.
                bool skipsPlant = false;
                for (int i = from; i < start; i++) if (line[i] == Plant) { skipsPlant = true; break; }
                if (skipsPlant) break;
                for (int i = from; i < start; i++) cur[i] = Bare;
                bool ok = true;
                for (int i = start; i < start + clue[k] && ok; i++) if (line[i] == Bare) ok = false;
                if (!ok) continue;
                int end = start + clue[k];
                if (end < n && line[end] == Plant) continue;
                for (int i = start; i < end; i++) cur[i] = Plant;
                int next = end;
                if (end < n) { cur[end] = Bare; next = end + 1; }
                Place(clue, k + 1, next, line, cur, canPlant, canBare, ref any);
            }
        }

        // Row/column passes to a fixpoint. If firstFound is non-null, stops at the
        // first line that newly fixes a cell and reports the cell there.
        private static bool Propagate(int[] known, int[][] rows, int[][] cols, int n, int[] firstFound)
        {
            var line = new int[n];
            bool changed = true;
            while (changed)
            {
                changed = false;
                for (int pass = 0; pass < 2 * n; pass++)
                {
                    bool isRow = pass < n;
                    int idx = isRow ? pass : pass - n;
                    for (int k = 0; k < n; k++) line[k] = known[isRow ? idx * n + k : k * n + idx];
                    var before = (int[])line.Clone();
                    if (!SolveLine(isRow ? rows[idx] : cols[idx], line)) return false;
                    for (int k = 0; k < n; k++)
                    {
                        if (line[k] == before[k]) continue;
                        int cell = isRow ? idx * n + k : k * n + idx;
                        known[cell] = line[k];
                        changed = true;
                        if (firstFound != null && firstFound[0] < 0) firstFound[0] = cell;
                    }
                    if (firstFound != null && firstFound[0] >= 0) return true;
                }
            }
            return true;
        }

        // ---- play ----

        public bool Solution(int cell)
        {
            return cell >= 0 && cell < solution.Length && solution[cell];
        }

        public CellState State(int cell)
        {
            return cell >= 0 && cell < cells.Length ? cells[cell] : CellState.Empty;
        }

        public NonogramMove Tap(int cell)
        {
            if (cell < 0 || cell >= cells.Length) return NonogramMove.Ignored;
            CellState next = cells[cell] == CellState.Empty ? CellState.Filled
                : cells[cell] == CellState.Filled ? CellState.Marked : CellState.Empty;
            return Set(cell, next);
        }

        public NonogramMove Set(int cell, CellState s)
        {
            if (Solved || cell < 0 || cell >= cells.Length || cells[cell] == s) return NonogramMove.Ignored;
            cells[cell] = s;
            Moves++;
            Solved = true;
            for (int i = 0; i < cells.Length; i++)
                if ((cells[i] == CellState.Filled) != solution[i]) { Solved = false; break; }
            return s == CellState.Filled ? NonogramMove.Filled
                : s == CellState.Marked ? NonogramMove.Marked : NonogramMove.Cleared;
        }

        public bool RowDone(int r)
        {
            return r >= 0 && r < Size && LineMatches(r, true);
        }

        public bool ColumnDone(int c)
        {
            return c >= 0 && c < Size && LineMatches(c, false);
        }

        private bool LineMatches(int idx, bool isRow)
        {
            int[] clue = isRow ? RowClues[idx] : ColumnClues[idx];
            int ci = 0, run = 0;
            for (int k = 0; k <= Size; k++)
            {
                bool filled = k < Size && cells[isRow ? idx * Size + k : k * Size + idx] == CellState.Filled;
                if (filled) { run++; continue; }
                if (run == 0) continue;
                if (ci >= clue.Length || clue[ci] != run) return false;
                ci++;
                run = 0;
            }
            return ci == clue.Length;
        }

        /// <summary>
        /// A cell the line-solver can deduce from the player's correct cells, which
        /// the player has wrong or has not set yet; -1 when every cell is right.
        /// </summary>
        public int HintCell()
        {
            int n = Size;
            var known = new int[n * n];
            bool wrongOrUnset = false;
            for (int i = 0; i < known.Length; i++)
            {
                bool right = cells[i] == (solution[i] ? CellState.Filled : CellState.Marked);
                if (right) known[i] = solution[i] ? Plant : Bare;
                else wrongOrUnset = true;
            }
            if (!wrongOrUnset) return -1;
            var found = new[] { -1 };
            Propagate(known, RowClues, ColumnClues, n, found);
            return found[0];
        }
    }
}
