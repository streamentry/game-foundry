using System;
using System.Collections.Generic;

namespace Evergrove.Core
{
    public enum PipeKind { End, Straight, Corner }

    public sealed class Puzzle
    {
        public readonly string Name;
        public readonly PipeKind[] Kinds;
        public readonly int[] Initial;
        public readonly int[] Solution;

        public Puzzle(string name, string layout, int[] initial, int[] solution)
        {
            if (layout == null || layout.Length != 9 || initial.Length != 9 || solution.Length != 9)
                throw new ArgumentException("A puzzle requires nine pipe tiles.");
            Name = name;
            Kinds = new PipeKind[9];
            for (int i = 0; i < 9; i++)
            {
                switch (layout[i])
                {
                    case 'E': Kinds[i] = PipeKind.End; break;
                    case 'I': Kinds[i] = PipeKind.Straight; break;
                    case 'L': Kinds[i] = PipeKind.Corner; break;
                    default: throw new ArgumentException("Unknown tile type.");
                }
            }
            Initial = (int[])initial.Clone();
            Solution = (int[])solution.Clone();
        }
    }

    /// <summary>
    /// Six original 3x3 fixtures from Game Foundry's own Pipe Garden research prototype.
    /// Each is verified by the deterministic pure C# solver, not by any AI model.
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
                new[] {1,2,1,4,2,1,4,2,3}, new[] {2,2,3,1,4,1,2,2,4})
        };

        public static int Count { get { return Puzzles.Length; } }

        public static Puzzle Get(int index)
        {
            if (index < 0 || index >= Count) throw new ArgumentOutOfRangeException(nameof(index));
            return Puzzles[index];
        }
    }

    public static class PipeRules
    {
        // Directions: clockwise, up=0, right=1, down=2, left=3.
        public static int OrientationCount(PipeKind kind) { return kind == PipeKind.Straight ? 2 : 4; }

        public static bool HasPort(PipeKind kind, int rotation, int direction)
        {
            if (rotation < 1 || rotation > OrientationCount(kind) || direction < 0 || direction > 3)
                return false;
            int index = rotation - 1;
            if (kind == PipeKind.End) return direction == index;
            if (kind == PipeKind.Straight) return index == 0 ? direction == 0 || direction == 2 : direction == 1 || direction == 3;
            return direction == index || direction == (index + 1) % 4;
        }

        public static int Neighbor(int tile, int direction)
        {
            int row = tile / 3, column = tile % 3;
            switch (direction)
            {
                case 0: row--; break;
                case 1: column++; break;
                case 2: row++; break;
                case 3: column--; break;
                default: return -1;
            }
            return row >= 0 && row < 3 && column >= 0 && column < 3 ? row * 3 + column : -1;
        }

        public static bool ValidRotations(Puzzle puzzle, int[] rotations)
        {
            if (rotations == null || rotations.Length != 9) return false;
            for (int i = 0; i < 9; i++)
                if (rotations[i] < 1 || rotations[i] > OrientationCount(puzzle.Kinds[i])) return false;
            return true;
        }

        public static bool IsComplete(Puzzle puzzle, int[] rotations)
        {
            if (!ValidRotations(puzzle, rotations)) return false;
            int source = -1, endpoints = 0;
            for (int i = 0; i < 9; i++)
            {
                if (puzzle.Kinds[i] == PipeKind.End) { endpoints++; source = i; }
                for (int direction = 0; direction < 4; direction++)
                {
                    int neighbor = Neighbor(i, direction);
                    bool outgoing = HasPort(puzzle.Kinds[i], rotations[i], direction);
                    if (neighbor < 0)
                    {
                        if (outgoing) return false;
                    }
                    else if (outgoing != HasPort(puzzle.Kinds[neighbor], rotations[neighbor], (direction + 2) % 4))
                        return false;
                }
            }
            if (endpoints != 2) return false;
            var seen = new HashSet<int>();
            var pending = new Stack<int>();
            pending.Push(source);
            while (pending.Count > 0)
            {
                int tile = pending.Pop();
                if (!seen.Add(tile)) continue;
                for (int direction = 0; direction < 4; direction++)
                    if (HasPort(puzzle.Kinds[tile], rotations[tile], direction))
                    {
                        int neighbor = Neighbor(tile, direction);
                        if (neighbor >= 0) pending.Push(neighbor);
                    }
            }
            return seen.Count == 9;
        }

        public static int CountSolutions(Puzzle puzzle, int stopAt = 2)
        {
            if (stopAt < 1) throw new ArgumentOutOfRangeException(nameof(stopAt));
            int[] rotations = new int[9];
            int count = 0;
            Search(puzzle, rotations, 0, stopAt, ref count);
            return count;
        }

        private static void Search(Puzzle puzzle, int[] rotations, int index, int stopAt, ref int count)
        {
            if (count >= stopAt) return;
            if (index == 9)
            {
                if (IsComplete(puzzle, rotations)) count++;
                return;
            }
            for (int rotation = 1; rotation <= OrientationCount(puzzle.Kinds[index]); rotation++)
            {
                bool valid = true;
                for (int direction = 0; direction < 4; direction++)
                {
                    int neighbor = Neighbor(index, direction);
                    bool outgoing = HasPort(puzzle.Kinds[index], rotation, direction);
                    if (neighbor < 0 && outgoing) { valid = false; break; }
                    if (neighbor >= 0 && neighbor < index &&
                        outgoing != HasPort(puzzle.Kinds[neighbor], rotations[neighbor], (direction + 2) % 4))
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
