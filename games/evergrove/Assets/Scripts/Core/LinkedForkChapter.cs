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
    /// work 4); spacing the draw across the ranking is what gives the chapter a curve. Nothing in the game reads these seeds.
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

            var kept = new List<KeyValuePair<int, int>>();
            for (int seed = 0; seed < seedLimit; seed++)
            {
                LinkedForkBoard board = LinkedForks.Generate(seed);
                if (board == null) continue;
                AxleMomentResult moment = LinkedForks.AxleMoment(board);
                if (!moment.SolvedWithoutGuess || !moment.AxleRestarts || !moment.TruthKept) continue;
                kept.Add(new KeyValuePair<int, int>(seed, moment.Cells - moment.FixedBeforeStall));
            }

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
    }
}
