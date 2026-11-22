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
        // How much of the *current spring-connected route* reaches the
        // village before chapter one is completed. Full restoration stays
        // a separate, solved-only event. Integer for deterministic tests.
        public readonly int WaterReachPercent;

        private VillageScenePlan(int chapter, bool restoredWater, int reachPercent, int trees, int crossings, int flowers)
        {
            Chapter = chapter;
            WaterRestored = restoredWater;
            WaterReachPercent = restoredWater ? 100 : reachPercent;
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
            // An earlier completed stream keeps the village creek flowing.
            // On the first puzzle, show truthful incremental reach instead
            // of waiting for the final turn to paint every channel blue.
            bool restored = world.Round > 0 || world.Solved;
            int percent = 100;
            if (!restored)
            {
                int total = world.CurrentPuzzle.CellCount;
                int connectedBeyondSpring = world.Insight.ConnectedCount - 1;
                percent = total < 2 ? 0 :
                    System.Math.Min(99, System.Math.Max(0,
                        connectedBeyondSpring * 100 / (total - 1)));
            }
            return new VillageScenePlan(
                world.Round, restored, percent,
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
