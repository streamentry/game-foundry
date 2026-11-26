using System;

namespace Evergrove.Core
{
    /// <summary>
    /// I205: the nursery, a fixed catalogue of plants bought with seeds. Three rows
    /// (50, 100, 200 seeds) of three families each, kind = row * 3 + family. Each
    /// kind wears an item from a later garden's catalogue, so it looks prettier than
    /// what the player grew, with no new painting. A price never changes once seen.
    /// Deterministic: no clock, no randomness.
    /// </summary>
    public static class Nursery
    {
        public const int Rows = 3;
        public const int Families = 3;
        public const int Kinds = Rows * Families;
        private static readonly int[] Prices = { 50, 100, 200 };

        public static int Row(int kind) { Check(kind); return kind / Families; }
        public static VillageChoice Family(int kind) { Check(kind); return (VillageChoice)(kind % Families); }
        public static int Price(int kind) { return Prices[Row(kind)]; }

        /// <summary>The garden item this kind looks like: garden row + 1, chapter 2, 6 or 10.</summary>
        public static GardenItem Item(int kind)
        {
            int row = Row(kind);
            int chapter = Math.Min(GardenCatalog.ChaptersPerGarden - 1, 2 + row * 4);
            return GardenCatalog.For(row + 1, chapter, Family(kind));
        }

        private static void Check(int kind)
        {
            if (kind < 0 || kind >= Kinds) throw new ArgumentOutOfRangeException(nameof(kind));
        }
    }
}
