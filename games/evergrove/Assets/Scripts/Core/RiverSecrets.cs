using System;

namespace Evergrove.Core
{
    /// <summary>
    /// I320 (idea 24, after the secrets under stones in action-adventure classics): every
    /// stretch of river after the first hides one mossy stone on its bank. Tap it, and under
    /// it is a cutting someone lost long ago, once. Deterministic: the same stone, always;
    /// if a thing stands there, the stone sits further along the bank.
    /// </summary>
    public static class RiverSecrets
    {
        /// <summary>The cell of the stretch's mossy stone, or -1 (the first stretch has none).</summary>
        public static int CellFor(GardenMap map, int reach, int[] occupied)
        {
            if (map == null || reach < 1 || reach >= map.Reaches) return -1;
            int rows = map.Rows / Math.Max(1, map.Reaches);
            int start = (reach * 7 + 4) % rows; // a different place on each stretch's bank
            for (int k = 0; k < rows; k++)
            {
                int row = reach * rows + (start + k) % rows;
                for (int col = 0; col < map.Columns; col++)
                {
                    int cell = row * map.Columns + col;
                    if (!map.IsLand(cell) || !OnBank(map, cell)) continue;
                    if (occupied != null && Array.IndexOf(occupied, cell) >= 0) continue;
                    return cell;
                }
            }
            return -1;
        }

        private static bool OnBank(GardenMap map, int cell)
        {
            int col = cell % map.Columns;
            if (col > 0 && map.At(cell - 1) == GardenMap.Water) return true;
            if (col < map.Columns - 1 && map.At(cell + 1) == GardenMap.Water) return true;
            if (cell - map.Columns >= 0 && map.At(cell - map.Columns) == GardenMap.Water) return true;
            return cell + map.Columns < map.CellCount && map.At(cell + map.Columns) == GardenMap.Water;
        }

        /// <summary>What's under the stone: a cutting from a nursery row already open, by stretch.</summary>
        public static int CuttingFor(WorldModel world, int reach)
        {
            if (world == null || reach < 1) return -1;
            int open = 0;
            for (int kind = 0; kind < Nursery.Kinds; kind++) if (world.RowOpen(Nursery.Row(kind))) open++;
            if (open == 0) return -1;
            int pick = (reach * 5) % open;
            for (int kind = 0; kind < Nursery.Kinds; kind++)
                if (world.RowOpen(Nursery.Row(kind)) && pick-- == 0) return kind;
            return -1;
        }
    }
}
