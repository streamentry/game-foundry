using System;
using System.Collections.Generic;

namespace Evergrove.Core
{
    /// <summary>
    /// Root stone solver gate (depth proposal C1, automated gate K5).
    /// A 4x4 pipe board where one or two cells are root stones: blank cells with no
    /// ports that never turn and are not part of the stream. Generates such boards from
    /// a seed, proves each has exactly one answer, and reports solver nodes as a crude
    /// difficulty proxy. SOLVER GATE ONLY: NOT wired into the game until a painted
    /// root-stone tile and a readability test exist.
    /// </summary>
    public sealed class RootedBoard
    {
        public const int Edge = 4;

        public readonly int Size;
        public readonly bool[] Root;      // true = root stone
        public readonly PipeKind[] Kinds; // kind for non-root cells (ignored for roots)
        public readonly int[] Solution;   // 1..OrientationCount for pipes, 0 for roots
        public readonly int[] Initial;    // scrambled start, 0 for roots

        public RootedBoard(bool[] root, PipeKind[] kinds, int[] solution, int[] initial)
        {
            Size = Edge;
            Root = (bool[])root.Clone();
            Kinds = (PipeKind[])kinds.Clone();
            Solution = (int[])solution.Clone();
            Initial = (int[])initial.Clone();
            int pipes = 0;
            foreach (bool isRoot in root) if (!isRoot) pipes++;
            PipeCount = pipes;
        }

        /// <summary>Cells that are not root stones.</summary>
        public int PipeCount { get; }
    }

    public static class RootedBoards
    {
        private const int Size = RootedBoard.Edge;
        private const int Cells = Size * Size;
        private const int Attempts = 400;
        private const int NodeBudget = 20000;

        /// <summary>Deterministic per (seed, roots). Null if no unique board was found.</summary>
        public static RootedBoard Generate(int seed, int roots)
        {
            if (roots < 1 || roots > 2) throw new ArgumentOutOfRangeException(nameof(roots));
            for (int attempt = 0; attempt < Attempts; attempt++)
            {
                RootedBoard board = TryBuild(roots, new RootRandom(Mix(seed, roots, attempt)));
                if (board != null && CountSolutions(board, 2) == 1) return board;
            }
            return null;
        }

        /// <summary>Exhaustive count of complete answers, stopping at stopAt.</summary>
        public static int CountSolutions(RootedBoard b, int stopAt = 2)
        {
            if (b == null) throw new ArgumentNullException(nameof(b));
            if (stopAt < 1) throw new ArgumentOutOfRangeException(nameof(stopAt));
            int count = 0, nodes = 0;
            Search(b, new int[Cells], 0, stopAt, ref count, ref nodes);
            return count;
        }

        /// <summary>Partial assignments visited by a full count (stopAt = 2): a crude difficulty proxy.</summary>
        public static int SolverNodes(RootedBoard b)
        {
            if (b == null) throw new ArgumentNullException(nameof(b));
            int count = 0, nodes = 0;
            Search(b, new int[Cells], 0, 2, ref count, ref nodes);
            return nodes;
        }

        public static bool IsComplete(RootedBoard b, int[] rotations)
        {
            if (b == null || rotations == null || rotations.Length != Cells) return false;
            if (!ValidRotations(b, rotations) || EndCount(b) != 2) return false;
            return Leaks(b, rotations) == 0 && Reached(b, rotations) == b.PipeCount;
        }

        /// <summary>Unmatched ports, counting ports that face the edge or a root stone.</summary>
        public static int Leaks(RootedBoard b, int[] rotations)
        {
            int leaks = 0;
            for (int i = 0; i < Cells; i++)
                for (int d = 0; d < 4; d++)
                {
                    if (!PortOf(b, i, rotations[i], d)) continue;
                    int j = PipeRules.Neighbor(i, d, Size);
                    if (j < 0 || !PortOf(b, j, rotations[j], (d + 2) % 4)) leaks++;
                }
            return leaks;
        }

        private static uint Mix(int seed, int roots, int attempt)
        {
            unchecked { return (uint)seed * 0x9E3779B1u + (uint)roots * 0x85EBCA6Bu + (uint)attempt * 0xC2B2AE35u + 0x27d4eb2fu; }
        }

        private static RootedBoard TryBuild(int roots, RootRandom random)
        {
            bool[] root = PickRoots(roots, random);
            int[] path = HamiltonPath(root, random);
            if (path == null) return null;
            var kinds = new PipeKind[Cells];
            var solution = new int[Cells];
            for (int step = 0; step < path.Length; step++) SetFromStep(path, step, kinds, solution);
            return new RootedBoard(root, kinds, solution, Scramble(root, kinds, solution, random));
        }

        private static bool[] PickRoots(int roots, RootRandom random)
        {
            var root = new bool[Cells];
            for (int placed = 0; placed < roots;)
            {
                int cell = random.Next(Cells);
                if (root[cell]) continue;
                root[cell] = true;
                placed++;
            }
            return root;
        }

        // Randomized depth-first Hamiltonian path over the non-root cells, with a node budget.
        private static int[] HamiltonPath(bool[] root, RootRandom random)
        {
            int free = 0;
            foreach (bool isRoot in root) if (!isRoot) free++;
            int start = random.Next(Cells);
            while (root[start]) start = (start + 1) % Cells;
            var path = new int[free];
            var used = new bool[Cells];
            path[0] = start;
            used[start] = true;
            int budget = NodeBudget;
            return Extend(root, path, used, 1, random, ref budget) ? path : null;
        }

        private static bool Extend(bool[] root, int[] path, bool[] used, int length,
                                   RootRandom random, ref int budget)
        {
            if (length == path.Length) return true;
            if (--budget < 0) return false;
            int[] order = { 0, 1, 2, 3 };
            random.Shuffle(order);
            foreach (int direction in order)
            {
                int next = PipeRules.Neighbor(path[length - 1], direction, Size);
                if (next < 0 || used[next] || root[next]) continue;
                used[next] = true;
                path[length] = next;
                if (Extend(root, path, used, length + 1, random, ref budget)) return true;
                used[next] = false;
            }
            return false;
        }

        // Mirrors TreePuzzles.FromPath: ends face their one neighbour; straights run
        // north-south or east-west; corners hold two adjacent ports.
        private static void SetFromStep(int[] path, int step, PipeKind[] kinds, int[] solution)
        {
            int cell = path[step];
            int a = step > 0 ? DirectionBetween(cell, path[step - 1]) : -1;
            int b = step < path.Length - 1 ? DirectionBetween(cell, path[step + 1]) : -1;
            if (a < 0 || b < 0)
            {
                kinds[cell] = PipeKind.End;
                solution[cell] = (a < 0 ? b : a) + 1;
            }
            else if ((a + 2) % 4 == b)
            {
                kinds[cell] = PipeKind.Straight;
                solution[cell] = a % 2 == 0 ? 1 : 2;
            }
            else
            {
                kinds[cell] = PipeKind.Corner;
                solution[cell] = ((a + 1) % 4 == b ? a : b) + 1;
            }
        }

        private static int DirectionBetween(int from, int to)
        {
            for (int d = 0; d < 4; d++)
                if (PipeRules.Neighbor(from, d, Size) == to) return d;
            throw new InvalidOperationException("Path cells must touch.");
        }

        // Every pipe is turned to a wrong orientation, so the start is never complete.
        private static int[] Scramble(bool[] root, PipeKind[] kinds, int[] solution, RootRandom random)
        {
            var initial = new int[Cells];
            for (int i = 0; i < Cells; i++)
            {
                if (root[i]) continue;
                int turns = PipeRules.OrientationCount(kinds[i]);
                initial[i] = (solution[i] + random.Next(turns - 1)) % turns + 1;
            }
            return initial;
        }

        private static void Search(RootedBoard b, int[] rotations, int index, int stopAt,
                                   ref int count, ref int nodes)
        {
            nodes++;
            if (count >= stopAt) return;
            if (index == Cells)
            {
                if (IsComplete(b, rotations)) count++;
                return;
            }
            int first = b.Root[index] ? 0 : 1;
            int last = b.Root[index] ? 0 : PipeRules.OrientationCount(b.Kinds[index]);
            for (int rotation = first; rotation <= last; rotation++)
            {
                if (!FitsSoFar(b, rotations, index, rotation)) continue;
                rotations[index] = rotation;
                Search(b, rotations, index + 1, stopAt, ref count, ref nodes);
                rotations[index] = 0;
                if (count >= stopAt) return;
            }
        }

        // Checks every edge to an earlier cell; a root is checked the same way at rotation 0.
        private static bool FitsSoFar(RootedBoard b, int[] rotations, int index, int rotation)
        {
            for (int d = 0; d < 4; d++)
            {
                int j = PipeRules.Neighbor(index, d, Size);
                bool here = PortOf(b, index, rotation, d);
                if (j < 0 && here) return false;
                if (j >= 0 && j < index && here != PortOf(b, j, rotations[j], (d + 2) % 4)) return false;
            }
            return true;
        }

        private static bool PortOf(RootedBoard b, int cell, int rotation, int direction) { return !b.Root[cell] && PipeRules.HasPort(b.Kinds[cell], rotation, direction); }

        private static bool ValidRotations(RootedBoard b, int[] rotations)
        {
            for (int i = 0; i < Cells; i++)
            {
                int low = b.Root[i] ? 0 : 1;
                int high = b.Root[i] ? 0 : PipeRules.OrientationCount(b.Kinds[i]);
                if (rotations[i] < low || rotations[i] > high) return false;
            }
            return true;
        }

        private static int EndCount(RootedBoard b)
        {
            int ends = 0;
            for (int i = 0; i < Cells; i++)
                if (!b.Root[i] && b.Kinds[i] == PipeKind.End) ends++;
            return ends;
        }

        // Pipe cells reachable from the first End through matched ports.
        private static int Reached(RootedBoard b, int[] rotations)
        {
            int start = -1;
            for (int i = 0; i < Cells && start < 0; i++)
                if (!b.Root[i] && b.Kinds[i] == PipeKind.End) start = i;
            if (start < 0) return 0;
            var seen = new bool[Cells];
            var pending = new Stack<int>();
            pending.Push(start);
            int reached = 0;
            while (pending.Count > 0)
            {
                int i = pending.Pop();
                if (seen[i]) continue;
                seen[i] = true;
                reached++;
                for (int d = 0; d < 4; d++)
                {
                    int j = PipeRules.Neighbor(i, d, Size);
                    if (j >= 0 && !seen[j] && PortOf(b, i, rotations[i], d) &&
                        PortOf(b, j, rotations[j], (d + 2) % 4))
                        pending.Push(j);
                }
            }
            return reached;
        }

        // Tiny deterministic xorshift32, the same on every platform.
        private sealed class RootRandom
        {
            private uint state;

            public RootRandom(uint seed) { state = seed == 0 ? 0x6d2b79f5u : seed; }

            public int Next(int below)
            {
                state ^= state << 13;
                state ^= state >> 17;
                state ^= state << 5;
                return (int)(state % (uint)below);
            }

            public void Shuffle(int[] items)
            {
                for (int i = items.Length - 1; i > 0; i--)
                {
                    int j = Next(i + 1);
                    (items[i], items[j]) = (items[j], items[i]);
                }
            }
        }
    }
}
