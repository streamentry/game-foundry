using System;

namespace Evergrove.Core
{
    /// <summary>
    /// I360 (run 3, the open world): every stretch of the river has its own name, as real
    /// rivers' reaches do, so the way down the valley becomes a journey of places, not
    /// numbers. The first is home's. Names are plain English countryside words, short enough
    /// for a wooden sign at 150% text.
    /// </summary>
    public static class RiverNames
    {
        private static readonly string[] Names =
        {
            "Cottage Reach", "Willow Bend", "Heron Pool", "Mill Stream", "Kingfisher Run",
            "Alder Reach", "Otter Bend", "Old Ford", "Cress Beds", "Moorhen Pool",
            "Lily Reach", "Bramble Bend", "Swan Water", "Larch Run", "Wren Hollow",
            "Bluebell Bank", "Dipper Falls", "Long Meadow", "Lantern Pool", "Far Reach"
        };

        /// <summary>
        /// The name of any stretch (I363). The authored list is cycled, and each lap adds a
        /// Roman numeral: reach 0 is "Cottage Reach", reach 20 is "Cottage Reach II", reach 40
        /// is "Cottage Reach III". Deterministic, and adjacent stretches always differ.
        /// Negative indices keep to the first name.
        /// </summary>
        public static string For(int reach)
        {
            int index = Math.Max(0, reach);
            string name = Names[index % Names.Length];
            int lap = index / Names.Length;
            return lap == 0 ? name : name + " " + Roman(lap + 1);
        }

        private static string Roman(int number)
        {
            int[] values = { 1000, 900, 500, 400, 100, 90, 50, 40, 10, 9, 5, 4, 1 };
            string[] symbols = { "M", "CM", "D", "CD", "C", "XC", "L", "XL", "X", "IX", "V", "IV", "I" };
            var text = new System.Text.StringBuilder();
            int rest = number;
            for (int i = 0; i < values.Length; i++)
                while (rest >= values[i])
                {
                    text.Append(symbols[i]);
                    rest -= values[i];
                }
            return text.ToString();
        }

        /// <summary>
        /// A deterministic suggestion for naming a garden (checklist 3): the same seed always gives the
        /// same reach name, and every result is one of the river names above.
        /// </summary>
        public static string Suggest(int seed)
        {
            uint mixed = unchecked((uint)seed * 2654435761u);
            return Names[mixed % (uint)Names.Length];
        }
    }
}
