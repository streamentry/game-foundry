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

        public static string For(int reach)
        {
            return Names[Math.Max(0, Math.Min(Names.Length - 1, reach))];
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
