using System;

namespace Evergrove.Core
{
    /// <summary>
    /// The open garden map (E3, I175): a top-down painted plot divided into a
    /// soft grid. Each cell is free land, water or built (the cottage). Every
    /// choice's item stands on one free cell. Pure and deterministic: the mask
    /// comes from the painting (tools/evergrove_art_pipeline.py), placements
    /// from the choices (and, from E4, from where the player put things).
    /// </summary>
    public sealed class GardenMap
    {
        public const char Free = '.';
        public const char Water = '~';
        public const char Built = '#';

        public readonly int Columns;
        public readonly int Rows;
        /// <summary>Rows in one reach of the endless river (I192); 0 for a single painting.</summary>
        public readonly int ReachRows;
        private readonly char[] cells;

        /// <summary>Streams restored per new reach of river land.</summary>
        public const int StreamsPerReach = 3;
        public const int MaxReaches = 5;

        /// <summary>
        /// Reaches of river open for a garden (I192): the first from the start,
        /// one more for every three streams restored, all five once it's finished.
        /// </summary>
        public static int ReachesFor(int restored, bool completed)
        {
            if (completed) return MaxReaches;
            int open = 1 + Math.Max(0, restored) / StreamsPerReach;
            return open < 1 ? 1 : open > MaxReaches ? MaxReaches : open;
        }

        // ---- I220: after the story, the river keeps opening downstream as you tend
        // what you've grown. Derived from the save (puzzles solved), never stored, so
        // the ceiling can be raised later without touching any garden.
        public const int SolvesPerReach = 4;
        public const int ReachesCeiling = 20;

        public static int ReachesFor(int restored, bool completed, int puzzlesSolved)
        {
            if (!completed) return ReachesFor(restored, false);
            int open = MaxReaches + Math.Max(0, puzzlesSolved) / SolvesPerReach;
            return open > ReachesCeiling ? ReachesCeiling : open;
        }

        /// <summary>
        /// The endless river (I192): one reach's mask repeated down the map, every
        /// other reach mirrored top to bottom, exactly as the painting is drawn, so
        /// the creek and the banks join without a seam.
        /// </summary>
        public static GardenMap River(int columns, int reachRows, string reachMask, int reaches)
        {
            if (reaches < 1) throw new ArgumentOutOfRangeException(nameof(reaches));
            if (reachMask == null) throw new ArgumentNullException(nameof(reachMask));
            string flat = reachMask.Replace("\n", "").Replace("\r", "");
            if (flat.Length != columns * reachRows)
                throw new ArgumentException("reach mask size " + flat.Length + " != " + columns * reachRows);
            var all = new System.Text.StringBuilder(flat.Length * reaches);
            for (int reach = 0; reach < reaches; reach++)
                for (int row = 0; row < reachRows; row++)
                {
                    int source = reach % 2 == 0 ? row : reachRows - 1 - row;
                    all.Append(flat, source * columns, columns);
                }
            return new GardenMap(columns, reachRows * reaches, all.ToString(), reachRows);
        }

        /// <summary>How many reaches the map holds (1 for a single painting).</summary>
        public int Reaches { get { return ReachRows > 0 ? Rows / ReachRows : 1; } }

        private GardenMap(int columns, int rows, string mask, int reachRows) : this(columns, rows, mask)
        {
            ReachRows = reachRows;
        }

        public GardenMap(int columns, int rows, string mask)
        {
            if (columns <= 0 || rows <= 0) throw new ArgumentOutOfRangeException(nameof(columns));
            if (mask == null) throw new ArgumentNullException(nameof(mask));
            string flat = mask.Replace("\n", "").Replace("\r", "");
            if (flat.Length != columns * rows) throw new ArgumentException("mask size " + flat.Length + " != " + columns * rows);
            Columns = columns;
            Rows = rows;
            cells = new char[flat.Length];
            for (int i = 0; i < flat.Length; i++)
            {
                char c = flat[i];
                cells[i] = c == Water || c == Built ? c : Free;
            }
        }

        public int CellCount { get { return cells.Length; } }
        public char At(int cell) { return cells[cell]; }
        public bool IsLand(int cell) { return cell >= 0 && cell < cells.Length && cells[cell] == Free; }

        /// <summary>True when an item may stand here: land, inside, and not under another item.</summary>
        public bool CanPlace(int[] placements, int cell, int movingIndex = -1)
        {
            if (!IsLand(cell)) return false;
            if (placements == null) return true;
            for (int i = 0; i < placements.Length; i++)
                if (i != movingIndex && placements[i] == cell) return false;
            return true;
        }

        /// <summary>Steps (4-way) from this cell to the nearest water cell; large when there is none.</summary>
        public int DistanceToWater(int cell)
        {
            int best = int.MaxValue;
            int x = cell % Columns, y = cell / Columns;
            for (int i = 0; i < cells.Length; i++)
            {
                if (cells[i] != Water) continue;
                int d = Math.Abs(i % Columns - x) + Math.Abs(i / Columns - y);
                if (d < best) best = d;
            }
            return best == int.MaxValue ? 99 : best;
        }

        /// <summary>
        /// Where an item goes when the player has not placed it: the best free
        /// cell for its family. Trees like the edges and some distance from the
        /// creek; flowers like open meadow a little way from the water; things to
        /// build like the creek bank and the lower valley. Ties go to the lowest
        /// cell index, so the same garden always lays out the same way.
        /// </summary>
        public int DefaultCell(int[] placements, VillageChoice family, int chapter)
        {
            int best = -1;
            int bestScore = int.MinValue;
            for (int cell = 0; cell < cells.Length; cell++)
            {
                if (!CanPlace(placements, cell)) continue;
                int score = Score(cell, family, chapter);
                if (score > bestScore) { bestScore = score; best = cell; }
            }
            return best;
        }

        private int Score(int cell, VillageChoice family, int chapter)
        {
            int x = cell % Columns, y = cell / Columns;
            int water = DistanceToWater(cell);
            int edge = Math.Min(Math.Min(x, Columns - 1 - x), Math.Min(y, Rows - 1 - y));
            // A gentle spread down the valley as the year goes on.
            // Starts five rows down, below the floating title on the screens that
            // show one (I185), and reaches the valley's foot by the twelfth stream.
            int top = Math.Min(5, Rows - 1);
            int band = Rows <= 1 ? 0 : top + (int)Math.Round(chapter * (Rows - 1 - top) / 11.0);
            if (ReachRows > 0)
            {
                // Down the river (I192): each stream's thing grows in the reach that
                // stream opened, spreading down that reach, three streams to a reach.
                int reach = Math.Min(chapter / StreamsPerReach, Reaches - 1);
                int within = chapter - reach * StreamsPerReach;
                int start = reach == 0 ? Math.Min(5, ReachRows - 1) : 2;
                band = reach * ReachRows + start
                    + (int)Math.Round(Math.Min(within, StreamsPerReach - 1) * (ReachRows - 3 - start) / (double)(StreamsPerReach - 1));
            }
            int fromBand = Math.Abs(y - band);
            switch (family)
            {
                case VillageChoice.ShadeTree:
                    // One step in from the edge, never on it: a tree in the very
                    // corner is half off the view (I179).
                    return -Math.Abs(edge - 1) * 6 + Math.Min(water, 4) * 3 - fromBand * 4;
                case VillageChoice.Flowers:
                    return -Math.Abs(water - 2) * 6 + Math.Min(edge, 3) * 2 - fromBand * 4;
                default:
                    return -water * 8 - fromBand * 3;
            }
        }

        /// <summary>
        /// Placements for a garden's choices: kept where given (valid and not
        /// colliding), otherwise the family default. Index i is choice i; -1
        /// means no free cell remains.
        /// </summary>
        public int[] Layout(int[] choices, int[] placed)
        {
            int count = choices == null ? 0 : choices.Length;
            var result = new int[count];
            for (int i = 0; i < count; i++) result[i] = -1;
            for (int i = 0; i < count; i++)
            {
                int wanted = placed != null && i < placed.Length ? placed[i] : -1;
                if (wanted >= 0 && CanPlace(result, wanted, i)) result[i] = wanted;
            }
            for (int i = 0; i < count; i++)
                if (result[i] < 0) result[i] = DefaultCell(result, (VillageChoice)Clamp(choices[i]), i);
            return result;
        }

        /// <summary>
        /// The layout with free plantings (I193): plantings stand where they were
        /// put; each choice's default avoids their cells. Index i is choice i.
        /// </summary>
        public int[] Layout(int[] choices, int[] placed, int[] plantings)
        {
            if (plantings == null || plantings.Length == 0) return Layout(choices, placed);
            int count = choices == null ? 0 : choices.Length;
            // Reserve the planted cells as if they were taken by extra items.
            int extra = plantings.Length / 2;
            var reserved = new int[count + extra];
            for (int i = 0; i < reserved.Length; i++) reserved[i] = -1;
            for (int k = 0; k < extra; k++) reserved[count + k] = IsLand(plantings[k * 2 + 1]) ? plantings[k * 2 + 1] : -1;
            for (int i = 0; i < count; i++)
            {
                int wanted = placed != null && i < placed.Length ? placed[i] : -1;
                if (wanted >= 0 && CanPlace(reserved, wanted, i)) reserved[i] = wanted;
            }
            for (int i = 0; i < count; i++)
                if (reserved[i] < 0) reserved[i] = DefaultCell(reserved, (VillageChoice)Clamp(choices[i]), i);
            var result = new int[count];
            Array.Copy(reserved, result, count);
            return result;
        }

        /// <summary>True when nothing (item or planting) stands on this land cell.</summary>
        public bool IsFree(int[] layout, int[] plantings, int cell)
        {
            if (!CanPlace(layout, cell)) return false;
            if (plantings != null)
                for (int k = 1; k < plantings.Length; k += 2)
                    if (plantings[k] == cell) return false;
            return true;
        }

        private static int Clamp(int family) { return family < 0 ? 0 : family > 2 ? 2 : family; }
    }
}
