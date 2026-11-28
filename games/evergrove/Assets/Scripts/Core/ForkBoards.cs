using System;
using System.Collections.Generic;

namespace Evergrove.Core
{
    /// <summary>
    /// A 5x5 fork-tile board (solver gate only, not wired into the game). Each cell holds a
    /// 4-bit port mask: bit d is set when the tile has a port toward direction d
    /// (0 north, 1 east, 2 south, 3 west). A solved board is a spanning tree of the grid.
    /// </summary>
    public sealed class ForkBoard
    {
        public const int Edge = 5;
        public const int Cells = Edge * Edge;

        public readonly int Size;
        public readonly int[] Solution; // port mask per cell
        public readonly int[] Initial;  // scrambled masks, each a rotation of the solution mask

        public ForkBoard(int[] solution, int[] initial)
        {
            if (solution == null || initial == null || solution.Length != Cells || initial.Length != Cells)
                throw new ArgumentException("A fork board needs one mask per cell.");
            Size = Edge;
            Solution = (int[])solution.Clone();
            Initial = (int[])initial.Clone();
            int forks = 0;
            foreach (int mask in Solution) if (ForkBoards.PortCount(mask) == 3) forks++;
            ForkCount = forks;
        }

        /// <summary>Solution tiles with three ports (T-shaped forks).</summary>
        public int ForkCount { get; }
    }

    /// <summary>
    /// Generation, uniqueness counting and solver depth for fork boards. Orientation r of a base
    /// mask is the base rotated clockwise r times. Candidate arrays hold four flags per cell,
    /// indexed by r relative to the cell's canonical base mask; repeated rotations are cleared
    /// so each distinct orientation appears once. Pure: seeded xorshift only, no clock.
    /// </summary>
    public static class ForkBoards
    {
        private const int Cells = ForkBoard.Cells;
        private const int Edge = ForkBoard.Edge;
        private const int Attempts = 300;
        private const int NodeBudget = 2000000;

        /// <summary>Deterministic per seed. Null if no board with a unique answer was found.</summary>
        public static ForkBoard Generate(int seed)
        {
            for (int attempt = 0; attempt < Attempts; attempt++)
            {
                var random = new ForkRandom(Mix(seed, attempt));
                int[] tree = RandomTree(random);
                if (tree == null || ForkCountOf(tree) < 3) continue;
                var board = new ForkBoard(tree, Scramble(tree, random));
                if (CountWithinBudget(board, 2) == 1) return board;
            }
            return null;
        }

        /// <summary>Exhaustive count of complete answers, stopping at stopAt.</summary>
        public static int CountSolutions(ForkBoard b, int stopAt = 2)
        {
            RequireBoard(b);
            if (stopAt < 1) throw new ArgumentOutOfRangeException(nameof(stopAt));
            int count = CountWithinBudget(b, stopAt);
            if (count < 0) throw new InvalidOperationException("Solution count exceeded the node budget.");
            return count;
        }

        /// <summary>True when every cell has a rotation of its tile, no port leaks, and the ports form one tree.</summary>
        public static bool IsComplete(ForkBoard b, int[] masks)
        {
            if (b == null || masks == null || masks.Length != Cells) return false;
            for (int i = 0; i < Cells; i++)
                if (masks[i] < 0 || masks[i] > 15 || Canonical(masks[i]) != Canonical(b.Solution[i])) return false;
            return CountLeaks(masks) == 0 && IsTree(masks);
        }

        /// <summary>Arc consistency over orientations. Throws if the true orientation is ever removed.</summary>
        public static bool[][] Candidates(ForkBoard b)
        {
            RequireBoard(b);
            bool[][] candidates = InitialCandidates(b);
            Settle(b, candidates, true);
            return candidates;
        }

        /// <summary>Cells with more than one orientation that propagation reduces to exactly one.</summary>
        public static int ForcedMoves(ForkBoard b)
        {
            bool[][] settled = Candidates(b);
            bool[][] initial = InitialCandidates(b);
            int forced = 0;
            for (int i = 0; i < Cells; i++)
                if (CountTrue(initial[i]) > 1 && CountTrue(settled[i]) == 1) forced++;
            return forced;
        }

        /// <summary>Trial orientations tried by deduce-then-guess search before its first complete answer.</summary>
        public static int Guesses(ForkBoard b)
        {
            Solve(b, out int guesses);
            return guesses;
        }

        /// <summary>The first complete assignment found by deduce-then-guess search.</summary>
        public static int[] SolveByGuessing(ForkBoard b)
        {
            int[] masks = Solve(b, out _);
            if (masks == null) throw new InvalidOperationException("Guessing found no complete answer.");
            return masks;
        }

        /// <summary>Number of ports on a mask (0..4).</summary>
        public static int PortCount(int mask)
        {
            int count = 0;
            for (int d = 0; d < 4; d++) if (HasPort(mask, d)) count++;
            return count;
        }

        /// <summary>The mask rotated clockwise by turns quarter turns.</summary>
        public static int Rotate(int mask, int turns)
        {
            for (int t = 0; t < turns % 4; t++) mask = ((mask << 1) | (mask >> 3)) & 15;
            return mask;
        }

        /// <summary>The smallest rotation of a mask; two masks are rotations of one tile when these match.</summary>
        public static int Canonical(int mask)
        {
            int best = Rotate(mask, 0);
            for (int r = 1; r < 4; r++) best = Math.Min(best, Rotate(mask, r));
            return best;
        }

        private static int[] Solve(ForkBoard b, out int guesses)
        {
            RequireBoard(b);
            guesses = 0;
            return GuessSearch(b, InitialCandidates(b), ref guesses);
        }

        // Deduce, then branch on the most constrained undecided cell, trying orientations in order.
        private static int[] GuessSearch(ForkBoard b, bool[][] candidates, ref int guesses)
        {
            if (!Settle(b, candidates, false)) return null;
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

        private static int[] CompleteAssignment(ForkBoard b, bool[][] candidates)
        {
            var masks = new int[Cells];
            for (int i = 0; i < Cells; i++)
                masks[i] = Rotate(Canonical(b.Solution[i]), OnlyTrue(candidates[i]));
            return IsComplete(b, masks) ? masks : null;
        }

        private static bool[][] InitialCandidates(ForkBoard b)
        {
            var candidates = new bool[Cells][];
            for (int i = 0; i < Cells; i++)
            {
                int tileBase = Canonical(b.Solution[i]);
                candidates[i] = new bool[4];
                for (int r = 0; r < 4; r++) candidates[i][r] = IsFirstRotation(tileBase, r);
            }
            return candidates;
        }

        // Clears orientations with a port that has no matching partner in some neighbour's candidates.
        // With guardTruth, removing the true orientation throws instead of being silent.
        private static bool Settle(ForkBoard b, bool[][] candidates, bool guardTruth)
        {
            bool changed = true;
            while (changed)
            {
                changed = false;
                for (int i = 0; i < Cells; i++)
                {
                    int tileBase = Canonical(b.Solution[i]);
                    for (int r = 0; r < 4; r++)
                    {
                        if (!candidates[i][r]) continue;
                        int mask = Rotate(tileBase, r);
                        if (Supported(b, candidates, i, mask)) continue;
                        if (guardTruth && mask == b.Solution[i])
                            throw new InvalidOperationException("Arc consistency removed the true fork orientation.");
                        candidates[i][r] = false;
                        changed = true;
                    }
                    if (!AnyTrue(candidates[i])) return false;
                }
            }
            return true;
        }

        private static bool Supported(ForkBoard b, bool[][] candidates, int cell, int mask)
        {
            for (int d = 0; d < 4; d++)
            {
                bool port = HasPort(mask, d);
                int j = Neighbor(cell, d);
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
            int tileBase = Canonical(b.Solution[cell]);
            for (int r = 0; r < 4; r++)
                if (candidates[cell][r] && HasPort(Rotate(tileBase, r), direction) == port) return true;
            return false;
        }

        // Exhaustive search in row-major order. Returns -1 when the node budget runs out.
        private static int CountWithinBudget(ForkBoard b, int stopAt)
        {
            var tally = new Tally();
            CountSearch(b, new int[Cells], 0, stopAt, tally);
            return tally.OverBudget ? -1 : tally.Count;
        }

        private static void CountSearch(ForkBoard b, int[] masks, int index, int stopAt, Tally tally)
        {
            if (tally.OverBudget || tally.Count >= stopAt) return;
            if (++tally.Nodes > NodeBudget)
            {
                tally.OverBudget = true;
                return;
            }
            if (index == Cells)
            {
                if (IsTree(masks)) tally.Count++;
                return;
            }
            int tileBase = Canonical(b.Solution[index]);
            for (int r = 0; r < 4; r++)
            {
                if (!IsFirstRotation(tileBase, r)) continue;
                int mask = Rotate(tileBase, r);
                if (!FitsSoFar(masks, index, mask)) continue;
                masks[index] = mask;
                CountSearch(b, masks, index + 1, stopAt, tally);
            }
        }

        // Checks each port of the new cell against the edge or an already placed north/west neighbour.
        private static bool FitsSoFar(int[] masks, int index, int mask)
        {
            for (int d = 0; d < 4; d++)
            {
                bool port = HasPort(mask, d);
                int j = Neighbor(index, d);
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

        private static bool IsTree(int[] masks)
        {
            int ports = 0;
            foreach (int mask in masks) ports += PortCount(mask);
            if (ports % 2 != 0 || ports / 2 != Cells - 1) return false;
            return Reached(masks) == Cells;
        }

        // Cells reachable from cell 0 through ports that meet a matching port.
        private static int Reached(int[] masks)
        {
            var seen = new bool[Cells];
            var pending = new Stack<int>();
            seen[0] = true;
            pending.Push(0);
            int reached = 0;
            while (pending.Count > 0)
            {
                int i = pending.Pop();
                reached++;
                for (int d = 0; d < 4; d++)
                {
                    int j = Neighbor(i, d);
                    if (j < 0 || seen[j] || !HasPort(masks[i], d) || !HasPort(masks[j], (d + 2) % 4)) continue;
                    seen[j] = true;
                    pending.Push(j);
                }
            }
            return reached;
        }

        private static int CountLeaks(int[] masks)
        {
            int leaks = 0;
            for (int i = 0; i < Cells; i++)
                for (int d = 0; d < 4; d++)
                {
                    if (!HasPort(masks[i], d)) continue;
                    int j = Neighbor(i, d);
                    if (j < 0 || !HasPort(masks[j], (d + 2) % 4)) leaks++;
                }
            return leaks;
        }

        // Randomized Prim growth from a random cell. Returns null if any tile gets four ports.
        private static int[] RandomTree(ForkRandom random)
        {
            var masks = new int[Cells];
            var inTree = new bool[Cells];
            var frontier = new int[Cells * 4];
            inTree[random.Next(Cells)] = true;
            for (int added = 1; added < Cells; added++)
            {
                int count = Frontier(inTree, frontier);
                int pick = frontier[random.Next(count)];
                int from = pick / 4, d = pick % 4;
                int to = Neighbor(from, d);
                masks[from] |= 1 << d;
                masks[to] |= 1 << ((d + 2) % 4);
                inTree[to] = true;
            }
            foreach (int mask in masks) if (PortCount(mask) == 4) return null;
            return masks;
        }

        // Edges (from * 4 + direction) leaving the tree into cells not yet in it.
        private static int Frontier(bool[] inTree, int[] frontier)
        {
            int count = 0;
            for (int i = 0; i < Cells; i++)
            {
                if (!inTree[i]) continue;
                for (int d = 0; d < 4; d++)
                {
                    int j = Neighbor(i, d);
                    if (j >= 0 && !inTree[j]) frontier[count++] = i * 4 + d;
                }
            }
            return count;
        }

        // Each tile is a random rotation of its solution mask; at least one differs from the solution.
        private static int[] Scramble(int[] solution, ForkRandom random)
        {
            var initial = new int[Cells];
            bool differs = false;
            for (int i = 0; i < Cells; i++)
            {
                initial[i] = Rotate(solution[i], random.Next(4));
                if (initial[i] != solution[i]) differs = true;
            }
            if (!differs) initial[0] = Rotate(solution[0], 1);
            return initial;
        }

        private static int ForkCountOf(int[] masks)
        {
            int forks = 0;
            foreach (int mask in masks) if (PortCount(mask) == 3) forks++;
            return forks;
        }

        private static int FirstUndecided(bool[][] candidates)
        {
            int best = -1, bestCount = int.MaxValue;
            for (int i = 0; i < Cells; i++)
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
            var copy = new bool[Cells][];
            for (int i = 0; i < Cells; i++) copy[i] = (bool[])candidates[i].Clone();
            return copy;
        }

        private static void Narrow(bool[] flags, int keep)
        {
            for (int r = 0; r < flags.Length; r++) flags[r] = r == keep;
        }

        private static bool IsFirstRotation(int tileBase, int r)
        {
            for (int q = 0; q < r; q++)
                if (Rotate(tileBase, q) == Rotate(tileBase, r)) return false;
            return true;
        }

        private static bool HasPort(int mask, int direction) { return ((mask >> direction) & 1) != 0; }

        private static int CountTrue(bool[] flags)
        {
            int count = 0;
            foreach (bool flag in flags) if (flag) count++;
            return count;
        }

        private static bool AnyTrue(bool[] flags) { return CountTrue(flags) > 0; }

        private static int OnlyTrue(bool[] flags)
        {
            for (int r = 0; r < flags.Length; r++) if (flags[r]) return r;
            throw new InvalidOperationException("Cell has no orientation left.");
        }

        private static int Neighbor(int cell, int direction)
        {
            int row = cell / Edge, col = cell % Edge;
            switch (direction)
            {
                case 0: row--; break;
                case 1: col++; break;
                case 2: row++; break;
                default: col--; break;
            }
            return row >= 0 && row < Edge && col >= 0 && col < Edge ? row * Edge + col : -1;
        }

        private static void RequireBoard(ForkBoard b)
        {
            if (b == null) throw new ArgumentNullException(nameof(b));
        }

        private static uint Mix(int seed, int attempt)
        {
            unchecked { return (uint)seed * 0x9E3779B1u + (uint)attempt * 0x85EBCA6Bu + 0x27d4eb2fu; }
        }

        private sealed class Tally
        {
            public int Count;
            public int Nodes;
            public bool OverBudget;
        }

        // Tiny deterministic xorshift32, the same on every platform.
        private sealed class ForkRandom
        {
            private uint state;

            public ForkRandom(uint seed) { state = seed == 0 ? 0x6d2b79f5u : seed; }

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
