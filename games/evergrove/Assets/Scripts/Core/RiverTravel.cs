using System;
using System.Collections.Generic;

namespace Evergrove.Core
{
    /// <summary>One stop the boat can take: a stretch of river that is open now, with its name.</summary>
    public sealed class RiverStop
    {
        public readonly int Reach;   // 0 is home's stretch
        public readonly string Name; // RiverNames.For(Reach)

        public RiverStop(int reach, string name)
        {
            Reach = reach;
            Name = name;
        }
    }

    /// <summary>
    /// Checklist 43, travel by boat: any open stretch of the endless river is two taps away.
    /// Tap one opens "Travel by boat" from the menu; tap two chooses the stretch. Only open
    /// stretches are stops, so the boat never goes into the mist. Pure: no UI types.
    /// </summary>
    public static class RiverTravel
    {
        /// <summary>The boat is offered only once there is more than one stretch to choose.</summary>
        public const int MinimumOpenReaches = 2;

        /// <summary>The menu entry's words (tap one).</summary>
        public const string MenuLabel = "Travel by boat";

        /// <summary>The catalogue thing the boat is drawn with (Mend the rowing boat).</summary>
        public const string BoatName = "rowing boat";

        /// <summary>How long the boat's glide takes, in unscaled seconds (never more than 1.2).</summary>
        public const float GlideSeconds = 1.1f;

        public static bool CanTravel(int openReaches)
        {
            return openReaches >= MinimumOpenReaches;
        }

        /// <summary>Every open stretch, in order from home's stretch down the river. Never a misted one.</summary>
        public static List<RiverStop> Stops(int openReaches)
        {
            int count = Math.Min(Math.Max(0, openReaches), GardenMap.SafetyReachBound);
            var stops = new List<RiverStop>(count);
            for (int reach = 0; reach < count; reach++)
                stops.Add(new RiverStop(reach, RiverNames.For(reach)));
            return stops;
        }

        /// <summary>
        /// The taps that reach a stretch, as data: the menu entry, then the stretch's own button.
        /// </summary>
        public static string[] Steps(int reach)
        {
            if (reach < 0) throw new ArgumentOutOfRangeException(nameof(reach));
            return new[] { MenuLabel, RiverNames.For(reach) };
        }

        /// <summary>How many taps a stretch takes. Always two (Steps lists exactly two).</summary>
        public static int TapsTo(int reach)
        {
            return Steps(reach).Length;
        }

        /// <summary>The catalogue index of the rowing boat, or -1 if the catalogue ever loses it.</summary>
        public static int BoatItemIndex()
        {
            int count = GardenCatalog.CountOf(VillageChoice.Bridge);
            for (int index = 0; index < count; index++)
                if (GardenCatalog.Get(VillageChoice.Bridge, index).Name == BoatName) return index;
            return -1;
        }
    }
}
