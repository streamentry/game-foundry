using System;
using System.Collections.Generic;

namespace Evergrove.Core
{
    /// <summary>
    /// A fork board with one linked pair (solver gate only, not wired into play; vision run
    /// 20261128-1301 idea 1). Cells A and B are non-adjacent and share an axle: one tap turns both
    /// a quarter turn clockwise. The shared turn count k must make both masks match the answer.
    /// </summary>
    public sealed class LinkedForkBoard
    {
        public readonly ForkBoard Board;
        public readonly int A;
        public readonly int B;

        public LinkedForkBoard(ForkBoard board, int a, int b)
        {
            if (board == null) throw new ArgumentNullException(nameof(board));
            if (a < 0 || b < 0 || a >= board.CellCount || b >= board.CellCount || a == b)
                throw new ArgumentException("A linked pair needs two distinct cells on the board.");
            if (LinkedForks.Adjacent(board.Size, a, b))
                throw new ArgumentException("A linked pair cannot share an edge.");
            Board = board;
            A = a;
            B = b;
        }
    }

    /// <summary>
    /// Measurement of when the axle matters: how far neighbour-only deduction gets before it stalls, whether
    /// one axle prune restarts it, and whether the joint deduction solves the board with no guess.
    /// </summary>
    public sealed class AxleMomentResult
    {
        public readonly int FixedBeforeStall;
        public readonly int Cells;
        public readonly bool AxleRestarts;
        public readonly bool SolvedWithoutGuess;
        public readonly bool TruthKept;

        public AxleMomentResult(int fixedBeforeStall, int cells, bool axleRestarts, bool solvedWithoutGuess, bool truthKept)
        {
            FixedBeforeStall = fixedBeforeStall;
            Cells = cells;
            AxleRestarts = axleRestarts;
            SolvedWithoutGuess = solvedWithoutGuess;
            TruthKept = truthKept;
        }
    }

    /// <summary>
    /// Generation, uniqueness counting and deduce-then-guess search for linked fork boards. Solver
    /// gate only: nothing in play calls this. Candidate arrays follow ForkBoards: four flags per
    /// cell, indexed by rotation relative to the cell's canonical tile. Link propagation keeps only
    /// turn counts k where both linked masks still have a candidate. Pure: seeded xorshift only.
    /// </summary>
    public static class LinkedForks
    {
        private const int Attempts = 2000; // only ~2% of random trees are ambiguous without a link
        private const int NodeBudget = 2000000;

        /// <summary>True when two cells share an edge on a square board of the given edge length.</summary>
        public static bool Adjacent(int edge, int a, int b)
        {
            int rows = Math.Abs(a / edge - b / edge);
            int cols = Math.Abs(a % edge - b % edge);
            return rows + cols == 1;
        }

        private const int PairTries = 60;

        /// <summary>
        /// Deterministic per seed on the 5x5 board: a board with two or more answers without the link and
        /// exactly one with it. Null if none was found.
        /// </summary>
        public static LinkedForkBoard Generate(int seed)
        {
            int edge = ForkBoard.Edge;
            for (int attempt = 0; attempt < Attempts; attempt++)
            {
                var random = new LinkedRandom(Mix(seed, attempt));
                int[] tree = RandomTree(random, edge);
                if (tree == null || ForkCountOf(tree) < 3) continue;
                // Review fix: only a board that is ambiguous without the link can show what the link adds.
                int[] scrambled = Scramble(tree, random);
                if (ForkBoards.CountSolutions(new ForkBoard(tree, scrambled), 2) < 2) continue;
                List<int[]> pairs = EligiblePairs(tree, edge);
                for (int tries = 0; tries < PairTries && pairs.Count > 0; tries++)
                {
                    int pick = random.Next(pairs.Count);
                    int[] pair = pairs[pick];
                    pairs.RemoveAt(pick);
                    int turns = 1 + random.Next(3);
                    int[] initial = (int[])scrambled.Clone();
                    initial[pair[0]] = ForkBoards.Rotate(tree[pair[0]], 4 - turns);
                    initial[pair[1]] = ForkBoards.Rotate(tree[pair[1]], 4 - turns);
                    var board = new LinkedForkBoard(new ForkBoard(tree, initial), pair[0], pair[1]);
                    if (CountWithinBudget(board, 2) == 1) return board;
                }
            }
            return null;
        }

        /// <summary>Exhaustive count of complete answers that also satisfy the link, stopping at stopAt.</summary>
        public static int CountSolutions(LinkedForkBoard b, int stopAt = 2)
        {
            RequireBoard(b);
            if (stopAt < 1) throw new ArgumentOutOfRangeException(nameof(stopAt));
            int count = CountWithinBudget(b, stopAt);
            if (count < 0) throw new InvalidOperationException("Linked solution count exceeded the node budget.");
            return count;
        }

        /// <summary>Complete answer count ignoring the link, stopping at stopAt.</summary>
        public static int CountWithoutLink(LinkedForkBoard b, int stopAt = 2)
        {
            RequireBoard(b);
            return ForkBoards.CountSolutions(b.Board, stopAt);
        }

        /// <summary>True when the link removes ambiguity: more than one answer without it.</summary>
        public static bool LinkNeeded(LinkedForkBoard b)
        {
            return CountWithoutLink(b, 2) > 1;
        }

        /// <summary>Debug guard: deduction alone (arc consistency plus link pruning) keeps every true mask.</summary>
        public static bool TruthSurvivesDeduction(LinkedForkBoard b)
        {
            RequireBoard(b);
            bool[][] candidates = InitialCandidates(b);
            return Deduce(b, candidates) && TruthHolds(b, candidates);
        }

        /// <summary>
        /// Solver-gate question: does the axle matter mid-solve (aha) or only at the start (bookkeeping)?
        /// Runs neighbour-only deduction to a fixpoint, then one axle prune, then neighbour-only deduction
        /// again, then joint deduction. Candidate removal is monotone, so the final state is the joint
        /// fixpoint regardless of order. TruthKept is checked on the final state: a removed truth never returns.
        /// </summary>
        public static AxleMomentResult AxleMoment(LinkedForkBoard b)
        {
            RequireBoard(b);
            bool[][] candidates = InitialCandidates(b);
            bool stalled = ArcFixpoint(b.Board, candidates);
            int fixedBefore = CountFixed(candidates);
            bool restarts = false;
            if (stalled)
            {
                bool linkOk = PruneLink(b, candidates, out bool linkChanged);
                bool arcOk = linkOk && ArcFixpoint(b.Board, candidates);
                restarts = arcOk && linkChanged && CountFixed(candidates) > fixedBefore;
            }
            bool solved = Deduce(b, candidates) && CountFixed(candidates) == b.Board.CellCount;
            return new AxleMomentResult(fixedBefore, b.Board.CellCount, restarts, solved, TruthHolds(b, candidates));
        }

        /// <summary>Trial orientations tried by deduce-then-guess search before its first linked answer.</summary>
        public static int Guesses(LinkedForkBoard b)
        {
            RequireBoard(b);
            int guesses = 0;
            if (GuessSearch(b, InitialCandidates(b), ref guesses) == null)
                throw new InvalidOperationException("Guessing found no linked answer.");
            return guesses;
        }

        private static int CountWithinBudget(LinkedForkBoard b, int stopAt)
        {
            var tally = new Tally();
            CountSearch(b, new int[b.Board.CellCount], 0, stopAt, tally);
            return tally.OverBudget ? -1 : tally.Count;
        }

        private static void CountSearch(LinkedForkBoard b, int[] masks, int index, int stopAt, Tally tally)
        {
            if (tally.OverBudget || tally.Count >= stopAt) return;
            if (++tally.Nodes > NodeBudget)
            {
                tally.OverBudget = true;
                return;
            }
            if (index == masks.Length)
            {
                if (ForkBoards.IsComplete(b.Board, masks) && LinkHolds(b, masks)) tally.Count++;
                return;
            }
            int edge = b.Board.Size;
            int tileBase = ForkBoards.Canonical(b.Board.Solution[index]);
            for (int r = 0; r < 4; r++)
            {
                if (!IsFirstRotation(tileBase, r)) continue;
                int mask = ForkBoards.Rotate(tileBase, r);
                if (!FitsSoFar(masks, edge, index, mask)) continue;
                masks[index] = mask;
                if (index == Math.Max(b.A, b.B) && !LinkHolds(b, masks)) continue;
                CountSearch(b, masks, index + 1, stopAt, tally);
            }
        }

        // True when one turn count k brings both linked cells to the masks in this assignment.
        private static bool LinkHolds(LinkedForkBoard b, int[] masks)
        {
            for (int k = 0; k < 4; k++)
            {
                if (ForkBoards.Rotate(b.Board.Initial[b.A], k) == masks[b.A] &&
                    ForkBoards.Rotate(b.Board.Initial[b.B], k) == masks[b.B]) return true;
            }
            return false;
        }

        private static int[] GuessSearch(LinkedForkBoard b, bool[][] candidates, ref int guesses)
        {
            if (!Deduce(b, candidates)) return null;
            int cell = FirstUndecided(candidates);
            if (cell < 0) return CompleteAssignment(b, candidates);
            for (int r = 0; r < 4; r++)
            {
                if (!candidates[cell][r]) continue;
                guesses++;
                bool[][] trial = Copy(candidates);
                Narrow(trial[cell], r);
                int[] found = GuessSearch(b, trial, ref guesses);
                if (found != null) return found;
            }
            return null;
        }

        private static int[] CompleteAssignment(LinkedForkBoard b, bool[][] candidates)
        {
            var masks = new int[b.Board.CellCount];
            for (int i = 0; i < masks.Length; i++)
                masks[i] = ForkBoards.Rotate(ForkBoards.Canonical(b.Board.Solution[i]), OnlyTrue(candidates[i]));
            return ForkBoards.IsComplete(b.Board, masks) && LinkHolds(b, masks) ? masks : null;
        }

        // Arc consistency and link pruning to a fixpoint. False if some cell or the link has no option left.
        private static bool Deduce(LinkedForkBoard b, bool[][] candidates)
        {
            bool linkChanged = true;
            while (linkChanged)
            {
                if (!ArcFixpoint(b.Board, candidates)) return false;
                if (!PruneLink(b, candidates, out linkChanged)) return false;
            }
            return true;
        }

        // Neighbour-only arc consistency to a fixpoint, with no link. False if some cell has no orientation left.
        private static bool ArcFixpoint(ForkBoard board, bool[][] candidates)
        {
            bool changed = true;
            while (changed)
            {
                changed = false;
                for (int i = 0; i < candidates.Length; i++)
                {
                    int tileBase = ForkBoards.Canonical(board.Solution[i]);
                    for (int r = 0; r < 4; r++)
                    {
                        if (!candidates[i][r]) continue;
                        if (Supported(board, candidates, i, ForkBoards.Rotate(tileBase, r))) continue;
                        candidates[i][r] = false;
                        changed = true;
                    }
                    if (CountTrue(candidates[i]) == 0) return false;
                }
            }
            return true;
        }

        private static bool TruthHolds(LinkedForkBoard b, bool[][] candidates)
        {
            for (int i = 0; i < candidates.Length; i++)
                if (!Holds(b.Board, candidates, i, b.Board.Solution[i])) return false;
            return true;
        }

        // Cells with exactly one orientation left.
        private static int CountFixed(bool[][] candidates)
        {
            int fixedCells = 0;
            foreach (bool[] flags in candidates) if (CountTrue(flags) == 1) fixedCells++;
            return fixedCells;
        }

        // Keeps only masks for A and B that some turn count k allows on both cells at once.
        private static bool PruneLink(LinkedForkBoard b, bool[][] candidates, out bool changed)
        {
            var keepA = new List<int>();
            var keepB = new List<int>();
            for (int k = 0; k < 4; k++)
            {
                int masA = ForkBoards.Rotate(b.Board.Initial[b.A], k);
                int masB = ForkBoards.Rotate(b.Board.Initial[b.B], k);
                if (Holds(b.Board, candidates, b.A, masA) && Holds(b.Board, candidates, b.B, masB))
                {
                    keepA.Add(masA);
                    keepB.Add(masB);
                }
            }
            changed = Restrict(b.Board, candidates, b.A, keepA);
            changed |= Restrict(b.Board, candidates, b.B, keepB);
            return CountTrue(candidates[b.A]) > 0 && CountTrue(candidates[b.B]) > 0;
        }

        // Clears orientations of a cell whose mask is not in keep. Returns true if anything was cleared.
        private static bool Restrict(ForkBoard board, bool[][] candidates, int cell, List<int> keep)
        {
            bool changed = false;
            int tileBase = ForkBoards.Canonical(board.Solution[cell]);
            for (int r = 0; r < 4; r++)
            {
                if (!candidates[cell][r] || keep.Contains(ForkBoards.Rotate(tileBase, r))) continue;
                candidates[cell][r] = false;
                changed = true;
            }
            return changed;
        }

        private static bool Holds(ForkBoard board, bool[][] candidates, int cell, int mask)
        {
            int tileBase = ForkBoards.Canonical(board.Solution[cell]);
            for (int r = 0; r < 4; r++)
                if (candidates[cell][r] && ForkBoards.Rotate(tileBase, r) == mask) return true;
            return false;
        }

        private static bool Supported(ForkBoard b, bool[][] candidates, int cell, int mask)
        {
            for (int d = 0; d < 4; d++)
            {
                bool port = HasPort(mask, d);
                int j = Neighbor(b.Size, cell, d);
                if (j < 0)
                {
                    if (port) return false;
                    continue;
                }
                if (!NeighbourAgrees(b, candidates, j, (d + 2) % 4, port)) return false;
            }
            return true;
        }

        private static bool NeighbourAgrees(ForkBoard b, bool[][] candidates, int cell, int direction, bool port)
        {
            int tileBase = ForkBoards.Canonical(b.Solution[cell]);
            for (int r = 0; r < 4; r++)
                if (candidates[cell][r] && HasPort(ForkBoards.Rotate(tileBase, r), direction) == port) return true;
            return false;
        }

        private static bool[][] InitialCandidates(LinkedForkBoard b)
        {
            int cells = b.Board.CellCount;
            var candidates = new bool[cells][];
            for (int i = 0; i < cells; i++)
            {
                int tileBase = ForkBoards.Canonical(b.Board.Solution[i]);
                candidates[i] = new bool[4];
                for (int r = 0; r < 4; r++) candidates[i][r] = IsFirstRotation(tileBase, r);
            }
            return candidates;
        }

        // Checks each port of the new cell against the edge or an already placed north/west neighbour.
        private static bool FitsSoFar(int[] masks, int edge, int index, int mask)
        {
            for (int d = 0; d < 4; d++)
            {
                bool port = HasPort(mask, d);
                int j = Neighbor(edge, index, d);
                if (j < 0)
                {
                    if (port) return false;
                }
                else if (j < index && port != HasPort(masks[j], (d + 2) % 4))
                {
                    return false;
                }
            }
            return true;
        }

        // Randomized Prim growth from a random cell. Returns null if any tile gets four ports.
        private static int[] RandomTree(LinkedRandom random, int edge)
        {
            int cells = edge * edge;
            var masks = new int[cells];
            var inTree = new bool[cells];
            var frontier = new int[cells * 4];
            inTree[random.Next(cells)] = true;
            for (int added = 1; added < cells; added++)
            {
                int count = Frontier(inTree, frontier, edge);
                int pick = frontier[random.Next(count)];
                int from = pick / 4, d = pick % 4;
                int to = Neighbor(edge, from, d);
                masks[from] |= 1 << d;
                masks[to] |= 1 << ((d + 2) % 4);
                inTree[to] = true;
            }
            foreach (int mask in masks) if (ForkBoards.PortCount(mask) == 4) return null;
            return masks;
        }

        // Edges (from * 4 + direction) leaving the tree into cells not yet in it.
        private static int Frontier(bool[] inTree, int[] frontier, int edge)
        {
            int count = 0;
            for (int i = 0; i < inTree.Length; i++)
            {
                if (!inTree[i]) continue;
                for (int d = 0; d < 4; d++)
                {
                    int j = Neighbor(edge, i, d);
                    if (j >= 0 && !inTree[j]) frontier[count++] = i * 4 + d;
                }
            }
            return count;
        }

        // Cell pairs (a < b) that are non-adjacent and whose tiles each have four distinct rotations.
        private static List<int[]> EligiblePairs(int[] solution, int edge)
        {
            var eligible = new bool[solution.Length];
            for (int i = 0; i < solution.Length; i++) eligible[i] = HasFourRotations(solution[i]);
            var pairs = new List<int[]>();
            for (int a = 0; a < solution.Length; a++)
                for (int b = a + 1; b < solution.Length; b++)
                    if (eligible[a] && eligible[b] && !Adjacent(edge, a, b)) pairs.Add(new[] { a, b });
            return pairs;
        }

        private static bool HasFourRotations(int mask)
        {
            if (ForkBoards.PortCount(mask) == 0) return false;
            var seen = new HashSet<int>();
            for (int r = 0; r < 4; r++) seen.Add(ForkBoards.Rotate(mask, r));
            return seen.Count == 4;
        }

        // Each tile is a random rotation of its solution mask.
        private static int[] Scramble(int[] solution, LinkedRandom random)
        {
            var initial = new int[solution.Length];
            for (int i = 0; i < solution.Length; i++) initial[i] = ForkBoards.Rotate(solution[i], random.Next(4));
            return initial;
        }

        private static int ForkCountOf(int[] masks)
        {
            int forks = 0;
            foreach (int mask in masks) if (ForkBoards.PortCount(mask) == 3) forks++;
            return forks;
        }

        private static int FirstUndecided(bool[][] candidates)
        {
            int best = -1, bestCount = int.MaxValue;
            for (int i = 0; i < candidates.Length; i++)
            {
                int count = CountTrue(candidates[i]);
                if (count > 1 && count < bestCount)
                {
                    best = i;
                    bestCount = count;
                }
            }
            return best;
        }

        private static bool[][] Copy(bool[][] candidates)
        {
            var copy = new bool[candidates.Length][];
            for (int i = 0; i < candidates.Length; i++) copy[i] = (bool[])candidates[i].Clone();
            return copy;
        }

        private static void Narrow(bool[] flags, int keep)
        {
            for (int r = 0; r < flags.Length; r++) flags[r] = r == keep;
        }

        private static bool IsFirstRotation(int tileBase, int r)
        {
            for (int q = 0; q < r; q++)
                if (ForkBoards.Rotate(tileBase, q) == ForkBoards.Rotate(tileBase, r)) return false;
            return true;
        }

        private static bool HasPort(int mask, int direction) { return ((mask >> direction) & 1) != 0; }

        private static int CountTrue(bool[] flags)
        {
            int count = 0;
            foreach (bool flag in flags) if (flag) count++;
            return count;
        }

        private static int OnlyTrue(bool[] flags)
        {
            for (int r = 0; r < flags.Length; r++) if (flags[r]) return r;
            throw new InvalidOperationException("Cell has no orientation left.");
        }

        private static int Neighbor(int edge, int cell, int direction)
        {
            int row = cell / edge, col = cell % edge;
            switch (direction)
            {
                case 0: row--; break;
                case 1: col++; break;
                case 2: row++; break;
                default: col--; break;
            }
            return row >= 0 && row < edge && col >= 0 && col < edge ? row * edge + col : -1;
        }

        private static void RequireBoard(LinkedForkBoard b)
        {
            if (b == null) throw new ArgumentNullException(nameof(b));
        }

        private static uint Mix(int seed, int attempt)
        {
            unchecked { return (uint)seed * 0x7F4A7C15u + (uint)attempt * 0x94D049BBu + 0x2545F491u; }
        }

        private sealed class Tally
        {
            public int Count;
            public int Nodes;
            public bool OverBudget;
        }

        // Tiny deterministic xorshift32, the same on every platform.
        private sealed class LinkedRandom
        {
            private uint state;

            public LinkedRandom(uint seed) { state = seed == 0 ? 0x6d2b79f5u : seed; }

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
