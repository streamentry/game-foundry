using System;
using System.Collections.Generic;

namespace Evergrove.Core
{
    /// <summary>
    /// I363 (run 3, P8: "early on the garden is mostly empty grass"): wild flowers come back to
    /// the banks as the streams are restored, two tufts for each, up to a few dozen: the valley
    /// coming alive is something you can see from the first stream. Cosmetic, read from the
    /// save; tufts sit only on free bank land, never where a thing stands.
    /// </summary>
    public static class MeadowLife
    {
        public const int Most = 24;

        public static int Count(int streamsRestored) { return Math.Max(0, Math.Min(Most, streamsRestored * 2)); }

        /// <summary>The bank cells for <paramref name="count"/> tufts, in a fixed order, avoiding occupied cells.</summary>
        public static int[] Cells(GardenMap map, int count, int[] occupied)
        {
            var banks = new List<int>();
            for (int cell = 0; cell < map.CellCount; cell++)
                if (map.IsLand(cell) && NearWater(map, cell) && (occupied == null || Array.IndexOf(occupied, cell) < 0))
                    banks.Add(cell);
            // A fixed scatter: sort by a hash of the cell, so tufts spread along the whole bank.
            banks.Sort((a, b) => Scatter(a).CompareTo(Scatter(b)));
            int n = Math.Min(count, banks.Count);
            return banks.GetRange(0, n).ToArray();
        }

        private static uint Scatter(int cell)
        {
            uint x = (uint)(cell + 1) * 2654435761u;
            x ^= x >> 15; x *= 2246822519u; x ^= x >> 13;
            return x;
        }

        private static bool NearWater(GardenMap map, int cell)
        {
            int col = cell % map.Columns;
            for (int dy = -1; dy <= 1; dy++)
                for (int dx = -1; dx <= 1; dx++)
                {
                    if (dx == 0 && dy == 0) continue;
                    int c = col + dx, other = cell + dy * map.Columns + dx;
                    if (c < 0 || c >= map.Columns || other < 0 || other >= map.CellCount) continue;
                    if (map.At(other) == GardenMap.Water) return true;
                }
            return false;
        }
    }
}
