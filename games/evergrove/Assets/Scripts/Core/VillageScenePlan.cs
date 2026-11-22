namespace Evergrove.Core
{
    /// <summary>
    /// Deterministic world-to-visual contract. A renderer never decides what
    /// happened in the village; it illustrates this inspectable plan.
    /// Value type means no platform dependency or mutable shared references.
    /// </summary>
    public readonly struct VillageScenePlan
    {
        public readonly int Chapter;
        public readonly int TreeCount;
        public readonly int CrossingCount;
        public readonly int FlowerCount;
        public readonly bool HasBridge;
        public readonly bool HasVisitors;
        public readonly bool HasButterflies;
        public readonly bool NeighborsGather;
        public readonly int LandscapePhase;
        public readonly bool WaterRestored;

        private VillageScenePlan(int chapter, bool restoredWater, int trees, int crossings, int flowers)
        {
            Chapter = chapter;
            WaterRestored = restoredWater;
            TreeCount = trees;
            CrossingCount = crossings;
            FlowerCount = flowers;
            HasBridge = crossings > 0;
            HasVisitors = crossings > 0 && flowers > 0;
            HasButterflies = trees > 0 && flowers > 0;
            NeighborsGather = trees > 0 && crossings > 0 && flowers > 0;
            LandscapePhase = chapter >= 9 ? 3 : chapter >= 6 ? 2 : chapter >= 3 ? 1 : 0;
        }

        public static VillageScenePlan From(WorldModel world)
        {
            return new VillageScenePlan(
                world.Round,
                world.Round > 0 || world.Solved,
                world.Count(VillageChoice.ShadeTree),
                world.Count(VillageChoice.Bridge),
                world.Count(VillageChoice.Flowers));
        }

        public string Fingerprint
        {
            get { return Chapter + ":" + (WaterRestored ? "flowing" : "dry") + ":" + TreeCount + ":" + CrossingCount + ":" + FlowerCount; }
        }
    }
}
