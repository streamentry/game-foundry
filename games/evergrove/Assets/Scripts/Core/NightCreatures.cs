using System;

namespace Evergrove.Core
{
    /// <summary>
    /// Checklist 49: night creatures, one per open stretch of river. Moths and frogs come out
    /// in the evening only (dusk and night), and every stretch has one: even stretches have
    /// moths, odd stretches frogs. Pure: no UI types, nothing saved. Who has been seen is kept
    /// on the device (see the app's "who visits" log), never in the garden file.
    /// </summary>
    public static class NightCreatures
    {
        /// <summary>The evening: dusk and night. Never morning, day or golden hour.</summary>
        public static bool Dark(DayPhase phase)
        {
            return phase == DayPhase.Dusk || phase == DayPhase.Night;
        }

        /// <summary>True for the creatures this rule owns (the owl keeps its grove, I374).</summary>
        public static bool IsNightKind(Visitor visitor)
        {
            return visitor == Visitor.Moths || visitor == Visitor.Frogs;
        }

        /// <summary>The night creature of a stretch, or null when it is not evening or the stretch is not real.</summary>
        public static Visitor? For(int reach, DayPhase phase)
        {
            if (reach < 0 || !Dark(phase)) return null;
            return reach % 2 == 0 ? Visitor.Moths : Visitor.Frogs;
        }

        /// <summary>
        /// Where each stretch's night creature sits: one free land cell per stretch (-1 where none is
        /// free), or null outside the evening. The search starts at the stretch's middle row and centre
        /// column and works outward, so a creature sits in plain view, never at a screen edge under the
        /// buttons. A frog wants a cell beside the water; a moth a cell away from it. Either falls back
        /// to any free land cell of its stretch. Taken cells (things placed in the garden) are skipped.
        /// </summary>
        public static int[] Spots(GardenMap map, System.Collections.Generic.ICollection<int> taken, DayPhase phase)
        {
            if (map == null || map.ReachRows <= 0 || !Dark(phase)) return null;
            int reaches = map.Reaches;
            var spots = new int[reaches];
            for (int reach = 0; reach < reaches; reach++)
            {
                bool frog = reach % 2 == 1;
                int best = Search(map, taken, reach, cell => frog ? map.DistanceToWater(cell) == 1 : map.DistanceToWater(cell) >= 2);
                spots[reach] = best >= 0 ? best : Search(map, taken, reach, cell => true);
            }
            return spots;
        }

        // Rows from the stretch's middle outward, then columns from the centre outward; the stretch's
        // first and last rows (where the river joins the next stretch) are left alone.
        private static int Search(GardenMap map, System.Collections.Generic.ICollection<int> taken, int reach, Func<int, bool> wants)
        {
            int top = reach * map.ReachRows + 1, bottom = Math.Min(map.Rows, (reach + 1) * map.ReachRows - 1) - 1;
            if (bottom < top) return -1;
            int middleRow = (top + bottom) / 2, middleCol = (map.Columns - 1) / 2;
            for (int r = 0; r <= bottom - top; r++)
            {
                int row = middleRow + (r % 2 == 0 ? r / 2 : -(r + 1) / 2);
                if (row < top || row > bottom) continue;
                for (int c = 0; c < map.Columns; c++)
                {
                    int col = middleCol + (c % 2 == 0 ? c / 2 : -(c + 1) / 2);
                    if (col < 0 || col >= map.Columns) continue;
                    int cell = row * map.Columns + col;
                    if (map.IsLand(cell) && (taken == null || !taken.Contains(cell)) && wants(cell)) return cell;
                }
            }
            return -1;
        }
    }
}
