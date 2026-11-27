using System;

namespace Evergrove.Core
{
    /// <summary>
    /// I205: the nursery, a catalogue of plants bought with seeds. Three rows
    /// (50, 100, 200 seeds) of three families each make nine slots. Each kind wears an
    /// item from a later garden's catalogue, so it looks prettier than what the player
    /// grew, with no new painting. A price never changes once seen.
    /// I240: the nursery restocks. Each slot has <see cref="Editions"/> editions, each a
    /// different catalogue item of the same family, and buying a slot's plant brings out
    /// its next edition, so buying feeds the almanac. kind = edition * Slots + slot and
    /// slot = row * Families + family. Edition 0 is exactly the original nine kinds.
    /// Deterministic: no clock, no randomness.
    /// </summary>
    public static class Nursery
    {
        public const int Rows = 3;
        public const int Families = 3;
        /// <summary>Plants on the shelf at once: one per row and family.</summary>
        public const int Slots = Rows * Families;
        /// <summary>How many plants a slot offers in turn before it keeps the last one.</summary>
        public const int Editions = 5;
        public const int Kinds = Editions * Slots;
        private static readonly int[] Prices = { 50, 100, 200 };

        public static int Edition(int kind) { Check(kind); return kind / Slots; }
        public static int Slot(int kind) { Check(kind); return kind % Slots; }
        public static int Row(int kind) { return Slot(kind) / Families; }
        public static VillageChoice Family(int kind) { Check(kind); return (VillageChoice)(kind % Families); }
        public static int Price(int kind) { return Prices[Row(kind)]; }

        /// <summary>
        /// The garden item this kind looks like. Edition 0: garden row + 1, chapter 2, 6 or 10.
        /// Edition e moves e * Rows gardens further along the family's fixed order, which keeps
        /// every one of a family's Editions * Rows items distinct (tested).
        /// </summary>
        public static GardenItem Item(int kind)
        {
            int row = Row(kind);
            int chapter = Math.Min(GardenCatalog.ChaptersPerGarden - 1, 2 + row * 4);
            return GardenCatalog.For(row + 1 + Edition(kind) * Rows, chapter, Family(kind));
        }

        /// <summary>
        /// The kind a slot offers now, given a garden's bought pairs [kind, cell, ...]:
        /// one edition on for each plant of the slot already bought, stopping at the last.
        /// </summary>
        public static int InStock(int slot, int[] bought)
        {
            if (slot < 0 || slot >= Slots) throw new ArgumentOutOfRangeException(nameof(slot));
            int owned = 0;
            if (bought != null)
                for (int i = 0; i + 1 < bought.Length; i += 2)
                    if (bought[i] >= 0 && bought[i] < Kinds && bought[i] % Slots == slot) owned++;
            return Math.Min(owned, Editions - 1) * Slots + slot;
        }

        /// <summary>The nursery kind that sells this catalogue item, or -1 (I241: the almanac's hint).</summary>
        public static int KindOf(GardenItem item)
        {
            if (item == null) return -1;
            for (int kind = 0; kind < Kinds; kind++)
                if (Family(kind) == item.Family && Item(kind).Art == item.Art) return kind;
            return -1;
        }

        private static void Check(int kind)
        {
            if (kind < 0 || kind >= Kinds) throw new ArgumentOutOfRangeException(nameof(kind));
        }
    }
}
