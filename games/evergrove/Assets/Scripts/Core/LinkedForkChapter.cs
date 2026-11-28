using System;
using System.Collections.Generic;

namespace Evergrove.Core
{
    /// <summary>
    /// Solver gate only, not wired into play (vision run 20261128-1502 idea 1). Picks linked-fork seeds
    /// whose axle moment is an "aha": neighbour-only deduction stalls, one axle prune restarts it, and the
    /// joint deduction solves the board with no guess and keeps the true orientation. Qualifying seeds are
    /// ranked by work, the number of cells the axle still has to settle after neighbour deduction stalls,
    /// so a chapter reads from gentle to firm. A first draw that took the twelve lowest-work seeds was flat (all
    /// work 4); spacing the draw across the ranking is what gives the chapter a curve. A wide draw over 240 seeds (vision
    /// run 20261128-1601) finds the same work curve and the same 9 to 17 cliff: 197 boards qualify and 15 sit at work 10 to 15, but most of the ranking is work 4, so spacing by rank still steps from the 9s straight to the lone 17.
    /// Drawing by work level (<see cref="DrawByLevel"/>) fills that cliff: DrawByLevel(12, 240) gives works
    /// 4,4,6,8,9,10,11,12,13,14,15,17 with a largest step of 2 (vision run 20261128-1701). Nothing in the game reads these seeds.
    /// Reach does not set difficulty (vision run 20261128-1801): <see cref="WorkByReach"/> over 240 seeds gives mean work
    /// 6.29, 5.73, 6.63, 6.52, 6.44 at reach 2 to 6 (flat, mixed), and only single boards at reach 7 and 8 (both work 8).
    /// No board in the drawn chapter is another turned or mirrored (vision run 20261128-1901): <see cref="DistinctShapes"/>
    /// over DrawByLevel(12, 240) gives 12 distinct shapes among 12 boards.
    /// </summary>
    public static class LinkedForkChapter
    {
        /// <summary>
        /// Returns up to <paramref name="length"/> qualifying seeds from 0 to <paramref name="seedLimit"/> - 1,
        /// evenly spaced through the qualifying seeds ranked by work ascending (then seed ascending), so the first is
        /// the gentlest and the last the firmest. All qualifying seeds are returned when fewer than length qualify.
        /// Empty when length or seedLimit is not positive. Deterministic; the inputs are not changed.
        /// </summary>
        public static int[] Draw(int length, int seedLimit)
        {
            if (length <= 0 || seedLimit <= 0) return Array.Empty<int>();

            List<KeyValuePair<int, int>> kept = Qualifying(seedLimit);

            kept.Sort((x, y) => x.Value != y.Value
                ? x.Value.CompareTo(y.Value)
                : x.Key.CompareTo(y.Key));

            int n = kept.Count;
            int take = Math.Min(length, n);
            var result = new int[take];
            // Evenly spaced through the ranked list, gentlest first and firmest last, so the chapter spans the
            // whole range of work. When length <= n the spacing is at least one, so no seed repeats.
            for (int i = 0; i < take; i++)
            {
                int at = take == 1 ? 0 : (int)Math.Round(i * (n - 1) / (double)(take - 1), MidpointRounding.AwayFromZero);
                result[i] = kept[at].Key;
            }
            return result;
        }

        /// <summary>
        /// Returns up to <paramref name="length"/> qualifying seeds from 0 to <paramref name="seedLimit"/> - 1,
        /// drawn by work level rather than by rank. When there are at least length distinct work levels, one seed
        /// (the lowest) is taken from each of <paramref name="length"/> levels evenly spaced through the ascending
        /// levels. When there are fewer levels, the lowest seed of every level is taken and extra boards are added
        /// to the gentlest levels first, one per level per pass, until length seeds or no seeds remain.
        /// The result is ordered by work ascending, then seed ascending. Empty when length or seedLimit is not
        /// positive. Deterministic; the inputs are not changed.
        /// </summary>
        public static int[] DrawByLevel(int length, int seedLimit)
        {
            if (length <= 0 || seedLimit <= 0) return Array.Empty<int>();

            // The qualifying scan runs in seed order, so each level's seeds are already ascending.
            var levels = new SortedDictionary<int, List<int>>();
            foreach (KeyValuePair<int, int> entry in Qualifying(seedLimit))
            {
                if (!levels.TryGetValue(entry.Value, out List<int> seeds))
                {
                    seeds = new List<int>();
                    levels[entry.Value] = seeds;
                }
                seeds.Add(entry.Key);
            }

            var works = new List<int>(levels.Keys);
            var groups = new List<List<int>>(levels.Values);
            int levelCount = groups.Count;
            var picked = new List<KeyValuePair<int, int>>();

            if (levelCount >= length)
            {
                for (int i = 0; i < length; i++)
                {
                    int at = length == 1 ? 0 : (int)Math.Round(i * (levelCount - 1) / (double)(length - 1), MidpointRounding.AwayFromZero);
                    picked.Add(new KeyValuePair<int, int>(groups[at][0], works[at]));
                }
            }
            else
            {
                var next = new int[levelCount];
                for (int j = 0; j < levelCount; j++)
                {
                    picked.Add(new KeyValuePair<int, int>(groups[j][0], works[j]));
                    next[j] = 1;
                }

                bool added = true;
                while (picked.Count < length && added)
                {
                    added = false;
                    for (int j = 0; j < levelCount && picked.Count < length; j++)
                    {
                        if (next[j] >= groups[j].Count) continue;
                        picked.Add(new KeyValuePair<int, int>(groups[j][next[j]], works[j]));
                        next[j]++;
                        added = true;
                    }
                }
            }

            picked.Sort((x, y) => x.Value != y.Value
                ? x.Value.CompareTo(y.Value)
                : x.Key.CompareTo(y.Key));

            var result = new int[picked.Count];
            for (int i = 0; i < picked.Count; i++) result[i] = picked[i].Key;
            return result;
        }

        /// <summary>
        /// Reach of the axle against work, over the same qualifying seeds as <see cref="Draw"/> and
        /// <see cref="DrawByLevel"/> (seeds 0 to <paramref name="seedLimit"/> - 1). Reach is the Manhattan distance
        /// between cells A and B on their board. Returns one row <c>{distance, boards, totalWork}</c> per distance
        /// present, ascending by distance; the mean work of a row is totalWork / boards. Empty when seedLimit is not
        /// positive. Deterministic; the inputs are not changed.
        /// </summary>
        public static int[][] WorkByReach(int seedLimit)
        {
            if (seedLimit <= 0) return Array.Empty<int[]>();

            var reach = new SortedDictionary<int, (int Boards, int TotalWork)>();
            foreach (KeyValuePair<int, int> entry in Qualifying(seedLimit))
            {
                LinkedForkBoard board = LinkedForks.Generate(entry.Key);
                int edge = board.Board.Size;
                int distance = Math.Abs(board.A / edge - board.B / edge) + Math.Abs(board.A % edge - board.B % edge);
                reach.TryGetValue(distance, out (int Boards, int TotalWork) tally);
                reach[distance] = (tally.Boards + 1, tally.TotalWork + entry.Value);
            }

            var result = new int[reach.Count][];
            int row = 0;
            foreach (KeyValuePair<int, (int Boards, int TotalWork)> kv in reach)
            {
                result[row++] = new[] { kv.Key, kv.Value.Boards, kv.Value.TotalWork };
            }
            return result;
        }

        /// <summary>
        /// Work for one seed: cells minus cells fixed before neighbour deduction stalls. Throws when the seed
        /// has no linked board.
        /// </summary>
        public static int Work(int seed)
        {
            LinkedForkBoard board = LinkedForks.Generate(seed);
            if (board == null) throw new InvalidOperationException("No linked fork board for seed " + seed + ".");
            AxleMomentResult moment = LinkedForks.AxleMoment(board);
            return moment.Cells - moment.FixedBeforeStall;
        }

        /// <summary>
        /// Largest increase in <see cref="Work"/> between consecutive seeds in the given order. A decrease counts
        /// as 0. Returns 0 for null or fewer than two seeds. Throws when a seed has no linked board. The input is not changed.
        /// </summary>
        public static int LargestStep(int[] seeds)
        {
            if (seeds == null || seeds.Length < 2) return 0;

            int largest = 0;
            int previous = Work(seeds[0]);
            for (int i = 1; i < seeds.Length; i++)
            {
                int current = Work(seeds[i]);
                int step = current - previous;
                if (step > largest) largest = step;
                previous = current;
            }
            return largest;
        }

        /// <summary>
        /// True when one of the eight symmetries of the square (four quarter turns, each optionally mirrored) maps
        /// board a onto board b: every cell moves to its transformed position, its port mask is transformed the same
        /// way, and the linked pair {A, B} maps onto the linked pair of b as an unordered pair. Boards of different
        /// size, and null, are never the same shape. Deterministic; the inputs are not changed.
        /// </summary>
        public static bool SameShape(LinkedForkBoard a, LinkedForkBoard b)
        {
            if (a == null || b == null) return false;
            if (a.Board.Size != b.Board.Size) return false;
            for (int mirror = 0; mirror < 2; mirror++)
            {
                for (int turns = 0; turns < 4; turns++)
                {
                    if (MapsOnto(a, b, mirror == 1, turns)) return true;
                }
            }
            return false;
        }

        /// <summary>
        /// True when the boards generated from the two seeds have the same shape (see the board overload).
        /// </summary>
        public static bool SameShape(int seedA, int seedB)
        {
            return SameShape(LinkedForks.Generate(seedA), LinkedForks.Generate(seedB));
        }

        /// <summary>
        /// Number of shape equivalence classes among the boards generated from the seeds. Seeds with no linked
        /// board are skipped. Null or empty gives 0. Deterministic; the input is not changed.
        /// </summary>
        public static int DistinctShapes(int[] seeds)
        {
            if (seeds == null || seeds.Length == 0) return 0;
            var representatives = new List<LinkedForkBoard>();
            foreach (int seed in seeds)
            {
                LinkedForkBoard board = LinkedForks.Generate(seed);
                if (board == null) continue;
                bool known = false;
                foreach (LinkedForkBoard representative in representatives)
                {
                    if (SameShape(representative, board))
                    {
                        known = true;
                        break;
                    }
                }
                if (!known) representatives.Add(board);
            }
            return representatives.Count;
        }

        // Mirror (left to right) first when set, then quarter turns clockwise. Maps every cell and mask of a onto b.
        private static bool MapsOnto(LinkedForkBoard a, LinkedForkBoard b, bool mirror, int turns)
        {
            int edge = a.Board.Size;
            int cells = a.Board.CellCount;
            int[] solutionA = a.Board.Solution;
            int[] solutionB = b.Board.Solution;
            for (int i = 0; i < cells; i++)
            {
                int target = Place(i, edge, mirror, turns);
                if (solutionB[target] != Turn(solutionA[i], mirror, turns)) return false;
            }
            int moved1 = Place(a.A, edge, mirror, turns);
            int moved2 = Place(a.B, edge, mirror, turns);
            return (moved1 == b.A && moved2 == b.B) || (moved1 == b.B && moved2 == b.A);
        }

        private static int Place(int cell, int edge, bool mirror, int turns)
        {
            int row = cell / edge;
            int col = cell % edge;
            if (mirror) col = edge - 1 - col;
            for (int t = 0; t < turns; t++)
            {
                int oldRow = row;
                row = col;
                col = edge - 1 - oldRow;
            }
            return row * edge + col;
        }

        // Ports follow the board: a mirror swaps east and west, and a clockwise quarter turn moves each port one direction on.
        private static int Turn(int mask, bool mirror, int turns)
        {
            if (mirror) mask = (mask & 5) | ((mask & 2) << 2) | ((mask & 8) >> 2);
            return ForkBoards.Rotate(mask, turns);
        }

        /// <summary>
        /// Qualifying seeds from 0 to <paramref name="seedLimit"/> - 1 in seed order, as (seed, work) pairs.
        /// </summary>
        private static List<KeyValuePair<int, int>> Qualifying(int seedLimit)
        {
            var kept = new List<KeyValuePair<int, int>>();
            for (int seed = 0; seed < seedLimit; seed++)
            {
                LinkedForkBoard board = LinkedForks.Generate(seed);
                if (board == null) continue;
                AxleMomentResult moment = LinkedForks.AxleMoment(board);
                if (!moment.SolvedWithoutGuess || !moment.AxleRestarts || !moment.TruthKept) continue;
                kept.Add(new KeyValuePair<int, int>(seed, moment.Cells - moment.FixedBeforeStall));
            }
            return kept;
        }
    }
}
