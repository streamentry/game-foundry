using System;
using System.Collections.Generic;

namespace Evergrove.Core
{
    public enum PipeKind { End, Straight, Corner }

    /// <summary>Immutable puzzle definition. A board is either 3x3 or 4x4.</summary>
    public sealed class Puzzle
    {
        public readonly string Name;
        public readonly int Size;
        public readonly PipeKind[] Kinds;
        public readonly int[] Initial;
        public readonly int[] Solution;
        public int CellCount { get { return Size * Size; } }

        public Puzzle(string name, string layout, int[] initial, int[] solution)
        {
            if (layout == null || (layout.Length != 9 && layout.Length != 16) ||
                initial == null || solution == null || initial.Length != layout.Length ||
                solution.Length != layout.Length)
                throw new ArgumentException("Puzzle must contain a complete 3x3 or 4x4 fixture.");
            Name = name;
            Size = layout.Length == 9 ? 3 : 4;
            Kinds = new PipeKind[layout.Length];
            for (int i = 0; i < layout.Length; i++)
            {
                switch (layout[i])
                {
                    case 'E': Kinds[i] = PipeKind.End; break;
                    case 'I': Kinds[i] = PipeKind.Straight; break;
                    case 'L': Kinds[i] = PipeKind.Corner; break;
                    default: throw new ArgumentException("Unknown water channel.");
                }
            }
            Initial = (int[])initial.Clone();
            Solution = (int[])solution.Clone();
        }
    }

    /// <summary>
    /// Twelve authored fixtures: six 3x3 from the studio's own Pipe Garden,
    /// plus six new original 4x4 Hamiltonian water channels. The independently
    /// exhaustive solver in tests establishes one valid orientation per fixture.
    /// </summary>
    public static class PuzzleCatalog
    {
        private static readonly Puzzle[] Puzzles = {
            new Puzzle("First Bend", "LIEIELLIL",
                new[] {2,2,4,1,1,2,3,2,2}, new[] {2,2,4,1,2,3,1,2,4}),
            new Puzzle("Rainwater", "EILLEILIL",
                new[] {1,2,2,4,4,1,4,2,3}, new[] {2,2,3,2,4,1,1,2,4}),
            new Puzzle("The Long Way", "ELLLLIEIL",
                new[] {2,1,2,1,3,1,4,2,4}, new[] {3,2,3,1,4,1,2,2,4}),
            new Puzzle("Flowing Home", "LLEIIIELL",
                new[] {4,3,2,1,1,1,1,3,3}, new[] {2,3,3,1,1,1,1,1,4}),
            new Puzzle("Copper Run", "LLEILLLIE",
                new[] {1,3,1,1,3,3,1,2,2}, new[] {2,3,3,1,1,4,1,2,4}),
            new Puzzle("Final Garden", "LILLEIEIL",
                new[] {1,2,1,4,2,1,4,2,3}, new[] {2,2,3,1,4,1,2,2,4}),

            new Puzzle("Sunlit Bend", "LIILLLEELLLLLIIL",
                new[] {1,1,1,2,4,4,4,4,4,1,2,1,3,1,1,3},
                new[] {2,2,2,3,1,3,3,1,2,4,1,3,1,2,2,4}),
            new Puzzle("Stone Crossing", "LIILEELLLLLLLIIL",
                new[] {4,1,1,2,3,2,1,3,4,2,3,4,3,1,1,3},
                new[] {2,2,2,3,1,3,2,4,2,4,1,3,1,2,2,4}),
            new Puzzle("Moonwater", "LIEEILLIIIIILLLL",
                new[] {3,1,3,4,2,3,2,2,2,2,2,2,3,2,2,1},
                new[] {2,2,4,3,1,2,3,1,1,1,1,1,1,4,1,4}),
            new Puzzle("Winding Meadow", "LIILILILIIELELIL",
                new[] {1,1,1,1,2,1,1,3,2,2,4,4,4,3,1,2},
                new[] {2,2,2,3,1,2,2,4,1,1,2,3,1,1,2,4}),
            new Puzzle("Old Orchard", "EIILLIEIILLILLLL",
                new[] {3,1,1,2,4,1,1,2,2,4,2,2,2,2,3,1},
                new[] {2,2,2,3,2,2,4,1,1,2,3,1,1,4,1,4}),
            new Puzzle("Homeward Creek", "LELLLLIIELLILIIL",
                new[] {4,3,4,1,4,2,2,2,2,3,3,2,2,1,1,1},
                new[] {2,4,2,3,1,3,1,1,3,1,4,1,1,2,2,4})
        };

        public static int Count { get { return Puzzles.Length; } }
        public static Puzzle Get(int index)
        {
            if (index < 0 || index >= Count) throw new ArgumentOutOfRangeException(nameof(index));
            return Puzzles[index];
        }
    }

    public sealed class PipeInsight
    {
        public readonly int ConnectedCount;
        public readonly int LeakCount;
        public readonly int JoinedEdges;
        public readonly bool[] Flowing;

        public PipeInsight(int connected, int leaks, int edges, bool[] flowing)
        {
            ConnectedCount = connected;
            LeakCount = leaks;
            JoinedEdges = edges;
            Flowing = flowing;
        }
    }

    public static class PipeRules
    {
        // Clockwise directions: north=0, east=1, south=2, west=3.
        public static int OrientationCount(PipeKind kind) { return kind == PipeKind.Straight ? 2 : 4; }

        public static bool HasPort(PipeKind kind, int rotation, int direction)
        {
            if (rotation < 1 || rotation > OrientationCount(kind) || direction < 0 || direction > 3)
                return false;
            int index = rotation - 1;
            if (kind == PipeKind.End) return direction == index;
            if (kind == PipeKind.Straight)
                return index == 0 ? direction == 0 || direction == 2 : direction == 1 || direction == 3;
            return direction == index || direction == (index + 1) % 4;
        }

        public static int Neighbor(int tile, int direction, int size)
        {
            int row = tile / size, col = tile % size;
            switch (direction)
            {
                case 0: row--; break;
                case 1: col++; break;
                case 2: row++; break;
                case 3: col--; break;
                default: return -1;
            }
            return row >= 0 && row < size && col >= 0 && col < size ? row * size + col : -1;
        }

        public static bool ValidRotations(Puzzle puzzle, int[] rotations)
        {
            if (rotations == null || rotations.Length != puzzle.CellCount) return false;
            for (int i = 0; i < rotations.Length; i++)
                if (rotations[i] < 1 || rotations[i] > OrientationCount(puzzle.Kinds[i])) return false;
            return true;
        }

        /// <summary>Live feedback never reveals the reference solution.</summary>
        public static PipeInsight Inspect(Puzzle puzzle, int[] rotations)
        {
            if (!ValidRotations(puzzle, rotations))
                return new PipeInsight(0, 0, 0, new bool[puzzle.CellCount]);
            int leaks = 0, joined = 0;
            for (int i = 0; i < puzzle.CellCount; i++)
                for (int d = 0; d < 4; d++)
                {
                    int j = Neighbor(i, d, puzzle.Size);
                    bool port = HasPort(puzzle.Kinds[i], rotations[i], d);
                    if (j < 0)
                    {
                        if (port) leaks++;
                    }
                    else if (i < j)
                    {
                        bool other = HasPort(puzzle.Kinds[j], rotations[j], (d + 2) % 4);
                        if (port && other) joined++;
                        if (port != other) leaks++;
                    }
                }
            int source = -1;
            for (int i = 0; i < puzzle.CellCount; i++)
                if (puzzle.Kinds[i] == PipeKind.End) { source = i; break; }
            bool[] flowing = new bool[puzzle.CellCount];
            if (source < 0) return new PipeInsight(0, leaks, joined, flowing);
            var pending = new Stack<int>();
            pending.Push(source);
            int connected = 0;
            while (pending.Count > 0)
            {
                int i = pending.Pop();
                if (flowing[i]) continue;
                flowing[i] = true;
                connected++;
                for (int d = 0; d < 4; d++)
                {
                    int j = Neighbor(i, d, puzzle.Size);
                    if (j >= 0 &&
                        HasPort(puzzle.Kinds[i], rotations[i], d) &&
                        HasPort(puzzle.Kinds[j], rotations[j], (d + 2) % 4) &&
                        !flowing[j])
                        pending.Push(j);
                }
            }
            return new PipeInsight(connected, leaks, joined, flowing);
        }

        public static bool IsComplete(Puzzle puzzle, int[] rotations)
        {
            if (!ValidRotations(puzzle, rotations)) return false;
            int endpoints = 0;
            foreach (PipeKind kind in puzzle.Kinds)
                if (kind == PipeKind.End) endpoints++;
            if (endpoints != 2) return false;
            PipeInsight insight = Inspect(puzzle, rotations);
            return insight.ConnectedCount == puzzle.CellCount && insight.LeakCount == 0;
        }

        public static int CountSolutions(Puzzle puzzle, int stopAt = 2)
        {
            if (stopAt < 1) throw new ArgumentOutOfRangeException(nameof(stopAt));
            int[] rotations = new int[puzzle.CellCount];
            int count = 0;
            Search(puzzle, rotations, 0, stopAt, ref count);
            return count;
        }

        private static void Search(Puzzle puzzle, int[] rotations, int index, int stopAt, ref int count)
        {
            if (count >= stopAt) return;
            if (index == puzzle.CellCount)
            {
                if (IsComplete(puzzle, rotations)) count++;
                return;
            }
            for (int rotation = 1; rotation <= OrientationCount(puzzle.Kinds[index]); rotation++)
            {
                bool valid = true;
                for (int d = 0; d < 4; d++)
                {
                    int j = Neighbor(index, d, puzzle.Size);
                    bool here = HasPort(puzzle.Kinds[index], rotation, d);
                    if (j < 0 && here) { valid = false; break; }
                    if (j >= 0 && j < index &&
                        here != HasPort(puzzle.Kinds[j], rotations[j], (d + 2) % 4))
                    { valid = false; break; }
                }
                if (!valid) continue;
                rotations[index] = rotation;
                Search(puzzle, rotations, index + 1, stopAt, ref count);
                rotations[index] = 0;
                if (count >= stopAt) return;
            }
        }
    }
}
