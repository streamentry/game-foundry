using System;

namespace Evergrove.Core
{
    /// <summary>How a thing looks in a season (I338–I342).</summary>
    public enum ThingLook { Full, Blossom, Turning, Resting }

    /// <summary>How the river looks in a season (I342).</summary>
    public enum RiverLook { Running, Sparkling, LeavesAfloat, IceEdged }

    /// <summary>
    /// Run 3, I338–I342: seasons you can feel (founder: "should there be snow? what about
    /// autumn, spring, summer?"). The season is the garden's progress season (I248), never the
    /// clock. Trees blossom, leaf, turn and rest. Flowers bloom from spring to autumn; in winter
    /// only a winter flower (I290: snowdrops, crocuses...) blooms and the rest rest. Built
    /// things stay as built and only gather snow. Pure rules; Unity draws them.
    /// </summary>
    public static class SeasonLook
    {
        /// <param name="catalogueIndex">The thing's catalogue index within its family (a tree or a flower); -1 if unknown.</param>
        public static ThingLook For(Season season, VillageChoice family, int catalogueIndex)
        {
            if (family == VillageChoice.ShadeTree)
            {
                if (GardenCatalog.IsEvergreenTree(catalogueIndex)) return ThingLook.Full; // evergreens keep their green
                return season == Season.Spring ? ThingLook.Blossom : season == Season.Autumn ? ThingLook.Turning
                    : season == Season.Winter ? ThingLook.Resting : ThingLook.Full;
            }
            if (family == VillageChoice.Flowers)
            {
                if (season != Season.Winter) return ThingLook.Full;
                bool winterFlower = catalogueIndex >= 0 && Array.IndexOf(GardenCatalog.FlowersOf(Season.Winter), catalogueIndex) >= 0;
                return winterFlower ? ThingLook.Full : ThingLook.Resting;
            }
            return ThingLook.Full;
        }

        public static bool SnowCaps(Season season) { return season == Season.Winter; }

        /// <summary>How much snow lies on the ground, as a white veil's strength (0 to 1).</summary>
        public static float SnowLying(Season season) { return season == Season.Winter ? 0.26f : 0f; }

        /// <summary>How many pieces drift over home: enough to feel, never a blizzard.</summary>
        public static int DriftCount(Drift drift)
        {
            switch (drift)
            {
                case Drift.Snow: return 42;
                case Drift.Leaves: return 20;
                case Drift.Petals: return 20;
                default: return 0;
            }
        }

        public static RiverLook River(Season season)
        {
            return season == Season.Winter ? RiverLook.IceEdged : season == Season.Autumn ? RiverLook.LeavesAfloat
                : season == Season.Summer ? RiverLook.Sparkling : RiverLook.Running;
        }
    }
}
