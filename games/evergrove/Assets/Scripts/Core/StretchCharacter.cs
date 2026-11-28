using System;

namespace Evergrove.Core
{
    /// <summary>The three characters a stretch of river can have (checklist 42).</summary>
    public enum StretchKind { WaterMeadow, Mill, HeronPool }

    /// <summary>A colour overlay as 0 to 1 channels (no UnityEngine), laid softly over a stretch's painting.</summary>
    public readonly struct StretchTint
    {
        public readonly float R;
        public readonly float G;
        public readonly float B;
        public readonly float A;

        public StretchTint(float r, float g, float b, float a) { R = r; G = g; B = b; A = a; }
    }

    /// <summary>
    /// Checklist 42: each stretch has a character, its own palette, its own neighbour and its own puzzle
    /// flavour. Founder decision (2026-11-28): no new characters, so each kind is one of the three existing
    /// neighbours (Mira 0, Orin 1, Fen 2). Every rule here is pure and deterministic.
    /// </summary>
    public static class StretchCharacter
    {
        /// <summary>
        /// The kind of a stretch by its reach index: the kinds cycle in order, reach 0 water meadow, 1 mill,
        /// 2 heron pool, 3 water meadow again, and so on. Negative reaches keep to the first (water meadow).
        /// </summary>
        public static StretchKind KindOf(int reach)
        {
            int index = Math.Max(0, reach) % 3;
            return (StretchKind)index;
        }

        /// <summary>The neighbour who stands in a stretch of this kind: Mira (0), Orin (1) or Fen (2).</summary>
        public static int Neighbour(StretchKind kind)
        {
            switch (kind)
            {
                case StretchKind.Mill: return 2;       // Fen mends the mill
                case StretchKind.HeronPool: return 1;  // Orin builds the boat, and keeps the heron pool
                default: return 0;                     // Mira, by the water meadow
            }
        }

        /// <summary>The puzzle kinds the stretch's neighbour keeps (NeighbourNotice.KeptBy), in their order.</summary>
        public static PuzzleKind[] Flavour(StretchKind kind)
        {
            return NeighbourNotice.KeptBy(Neighbour(kind));
        }

        /// <summary>
        /// The gentle wash over a stretch's painting. Alpha stays low (0.12 to 0.18) so the painting still
        /// reads: a fresh spring green-gold for the meadow, warm wheat for the mill, cool blue-grey for the pool.
        /// </summary>
        public static StretchTint Tint(StretchKind kind)
        {
            switch (kind)
            {
                case StretchKind.Mill: return new StretchTint(0.90f, 0.66f, 0.22f, 0.16f);
                case StretchKind.HeronPool: return new StretchTint(0.42f, 0.56f, 0.70f, 0.14f);
                default: return new StretchTint(0.62f, 0.86f, 0.30f, 0.14f);
            }
        }

        /// <summary>
        /// The cell where a stretch's neighbour stands: land in the stretch's own rows, nearest the middle of
        /// the stretch (its middle row first), and never a cell <paramref name="taken"/> reports. Returns -1
        /// when the stretch has no free land.
        /// </summary>
        public static int StandCell(GardenMap map, int reach, Func<int, bool> taken)
        {
            if (map == null || map.ReachRows <= 0 || reach < 0 || reach >= map.Reaches) return -1;
            int top = reach * map.ReachRows;
            int bottom = Math.Min(map.Rows, top + map.ReachRows);
            int middleRow = top + map.ReachRows / 2;
            int middleColumn = map.Columns / 2;
            int best = -1;
            int bestScore = int.MaxValue;
            for (int row = top; row < bottom; row++)
                for (int column = 0; column < map.Columns; column++)
                {
                    int cell = row * map.Columns + column;
                    if (!map.IsLand(cell) || (taken != null && taken(cell))) continue;
                    int down = row - middleRow, across = column - middleColumn;
                    int score = down * down * 4 + across * across;
                    if (score < bestScore) { bestScore = score; best = cell; }
                }
            return best;
        }
    }
}
