using System;
using System.Text;

namespace Evergrove.Core
{
    /// <summary>
    /// Every thing grown in the garden holds its own water puzzle (I203, from an
    /// idea by the founder's daughter: "tap a tree to play its puzzle"). A puzzle
    /// is made from a seed, so the same tree always has the same board on every
    /// device; it always has exactly one answer; and it is a little harder at each
    /// tier. No clock, no currency, no limit on replays.
    /// </summary>
    public static class TreePuzzles
    {
        /// <summary>Everyday (3x3), a little harder (4x4, some tiles already right), harder (4x4, all turned).</summary>
        public const int Tiers = 3;
        private const int Attempts = 400;

        /// <summary>Tier for a thing grown in a stream: early streams everyday, late streams harder.</summary>
        public static int TierFor(int chapter)
        {
            return Math.Max(0, Math.Min(Tiers - 1, chapter / 4));
        }

        public static Puzzle For(int seed, int tier, string name = "")
        {
            if (tier < 0 || tier >= Tiers) throw new ArgumentOutOfRangeException(nameof(tier));
            int size = tier == 0 ? 3 : 4;
            var random = new Pick(unchecked((uint)seed * 0x9E3779B1u + (uint)tier * 0x85EBCA6Bu + 0x27d4eb2fu));
            for (int attempt = 0; attempt < Attempts; attempt++)
            {
                int[] path = Path(size, random);
                if (path == null) continue;
                Puzzle solved = FromPath(name, size, path, null);
                if (PipeRules.CountSolutions(solved, 2) != 1) continue;
                int[] initial = Scramble(solved, tier, random);
                return new Puzzle(name, Layout(solved), initial, solved.Solution);
            }
            // Unreachable in practice (tests cover thousands of seeds); fall back to
            // an authored board of the same size rather than fail a player.
            return PuzzleCatalog.Get(size == 3 ? 0 : 6);
        }

        // A random path through every cell (a Hamiltonian path), by randomized
        // depth-first search with backtracking. Small boards keep this instant.
        private static int[] Path(int size, Pick random)
        {
            int cells = size * size;
            var path = new int[cells];
            var used = new bool[cells];
            path[0] = random.Next(cells);
            used[path[0]] = true;
            return Extend(path, used, 1, size, random) ? path : null;
        }

        private static bool Extend(int[] path, bool[] used, int length, int size, Pick random)
        {
            if (length == path.Length) return true;
            int[] order = { 0, 1, 2, 3 };
            for (int i = 3; i > 0; i--)
            {
                int j = random.Next(i + 1);
                int swap = order[i]; order[i] = order[j]; order[j] = swap;
            }
            foreach (int direction in order)
            {
                int next = PipeRules.Neighbor(path[length - 1], direction, size);
                if (next < 0 || used[next]) continue;
                used[next] = true;
                path[length] = next;
                if (Extend(path, used, length + 1, size, random)) return true;
                used[next] = false;
            }
            return false;
        }

        private static int DirectionBetween(int from, int to, int size)
        {
            for (int d = 0; d < 4; d++) if (PipeRules.Neighbor(from, d, size) == to) return d;
            throw new InvalidOperationException("Path cells must touch.");
        }

        private static Puzzle FromPath(string name, int size, int[] path, int[] initial)
        {
            int cells = size * size;
            var layout = new StringBuilder(new string('?', cells));
            var solution = new int[cells];
            for (int step = 0; step < cells; step++)
            {
                int cell = path[step];
                int a = step > 0 ? DirectionBetween(cell, path[step - 1], size) : -1;
                int b = step < cells - 1 ? DirectionBetween(cell, path[step + 1], size) : -1;
                if (a < 0 || b < 0)
                {
                    layout[cell] = 'E';
                    solution[cell] = (a < 0 ? b : a) + 1; // an end's port faces rotation-1
                }
                else if ((a + 2) % 4 == b)
                {
                    layout[cell] = 'I';
                    solution[cell] = a % 2 == 0 ? 1 : 2; // north-south, or east-west
                }
                else
                {
                    layout[cell] = 'L';
                    solution[cell] = ((a + 1) % 4 == b ? a : b) + 1; // ports at index and index+1
                }
            }
            return new Puzzle(name, layout.ToString(), initial ?? solution, solution);
        }

        private static string Layout(Puzzle puzzle)
        {
            var layout = new StringBuilder(puzzle.CellCount);
            foreach (PipeKind kind in puzzle.Kinds)
                layout.Append(kind == PipeKind.End ? 'E' : kind == PipeKind.Straight ? 'I' : 'L');
            return layout.ToString();
        }

        // Tier 0: every tile at a random turn. Tier 1: five tiles already right,
        // the rest turned away. Tier 2: every tile turned away from its answer.
        private static int[] Scramble(Puzzle puzzle, int tier, Pick random)
        {
            int cells = puzzle.CellCount;
            var keep = new bool[cells];
            if (tier == 1)
                for (int kept = 0; kept < 5;)
                {
                    int cell = random.Next(cells);
                    if (!keep[cell]) { keep[cell] = true; kept++; }
                }
            for (int tries = 0; ; tries++)
            {
                var initial = new int[cells];
                for (int i = 0; i < cells; i++)
                {
                    int turns = PipeRules.OrientationCount(puzzle.Kinds[i]);
                    int answer = puzzle.Solution[i];
                    if (keep[i]) initial[i] = answer;
                    else if (tier == 0) initial[i] = random.Next(turns) + 1;
                    else initial[i] = (answer - 1 + 1 + random.Next(turns - 1)) % turns + 1;
                }
                if (!PipeRules.IsComplete(puzzle, initial) || tries > 50) return initial;
            }
        }

        // A tiny deterministic generator (xorshift32): the same on every platform,
        // unlike System.Random, so a tree's puzzle never changes between devices.
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
