using System;
using System.Collections.Generic;

namespace Evergrove.Core
{
    /// <summary>The kinds of food that stand on the cottage table.</summary>
    public enum FamilyTableKind { Fruit = 0, Jam = 1 }

    /// <summary>
    /// One group of food on the cottage table. <see cref="Total"/> is the true count in the garden;
    /// <see cref="Shown"/> is how many objects are drawn (capped for display). <see cref="Words"/> always
    /// states the true count, so the caption never undercounts.
    /// </summary>
    public sealed class FamilyTableItem
    {
        public FamilyTableKind Kind;
        public int Shown;
        public int Total;
        public string Words;
    }

    /// <summary>
    /// What the cottage table shows: the fruit in the basket and the jars in the pantry, as objects (I228, I328).
    /// The save does not record which relative sent each item, so the table shows counts only and never a sender.
    /// It shows the garden's stock, not only what arrived by parcel: received food cannot be told apart from
    /// food the garden picked or cooked. Pure: no clock, no randomness, same order every time.
    /// </summary>
    public static class FamilyTable
    {
        public const int MaxFruitShown = 9;
        public const int MaxJarsShown = 6;

        /// <summary>Fruit first, then jam; a group with nothing is left out. Empty for a null world.</summary>
        public static IReadOnlyList<FamilyTableItem> Items(WorldModel world)
        {
            var items = new List<FamilyTableItem>(2);
            if (world == null) return items;
            if (world.Fruit > 0)
                items.Add(Item(FamilyTableKind.Fruit, world.Fruit, MaxFruitShown));
            if (world.Jam > 0)
                items.Add(Item(FamilyTableKind.Jam, world.Jam, MaxJarsShown));
            return items;
        }

        private static FamilyTableItem Item(FamilyTableKind kind, int total, int cap)
        {
            string words = kind == FamilyTableKind.Fruit
                ? total + " fruit in your basket"
                : total + (total == 1 ? " jar of jam in your pantry" : " jars of jam in your pantry");
            return new FamilyTableItem { Kind = kind, Shown = Math.Min(total, cap), Total = total, Words = words };
        }
    }
}
