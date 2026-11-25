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
        private readonly char[] cells;

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
            int band = Rows <= 1 ? 0 : (int)Math.Round(chapter * (Rows - 1) / 11.0);
            int fromBand = Math.Abs(y - band);
            switch (family)
            {
                case VillageChoice.ShadeTree:
                    return -edge * 6 + Math.Min(water, 4) * 3 - fromBand * 4;
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

        private static int Clamp(int family) { return family < 0 ? 0 : family > 2 ? 2 : family; }
    }
}
