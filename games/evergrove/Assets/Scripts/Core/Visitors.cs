using System;

namespace Evergrove.Core
{
    public enum Visitor { Bees, Birds, Butterflies, Heron, Hedgehog, Owl, Kingfisher }

    /// <summary>
    /// I374 adds three drawn by good neighbours (I368): a heron to something built by the water,
    /// a hedgehog to a sheltered flower, an owl to a grove. Nature answers where you put things.
    /// I325 to I327 (ideas 17, 18 and 19, after Japanese cat-collecting games where visitors come
    /// to what you put out, and British creature-garden games where each has a logical reason):
    /// bees come once three flowers are grown, birds once two trees are tended (they bear
    /// fruit), butterflies to any flower in spring and summer. Read from the save only; when
    /// and whether they're out follows the season and the hour.
    /// </summary>
    public static class Visitors
    {
        public const int BeesNeedFlowers = 3, BirdsNeedTrees = 2;

        public static bool Welcomes(WorldModel world, Visitor visitor)
        {
            if (world == null || world.Mode != PlayMode.Living) return false;
            int flowers = 0, trees = 0;
            int[] made = world.Choices;
            for (int i = 0; i < made.Length; i++)
            {
                if (made[i] == (int)VillageChoice.Flowers) flowers++;
                if (made[i] == (int)VillageChoice.ShadeTree && world.TreeSolved(i)) trees++;
            }
            int[] bought = world.Bought;
            for (int k = 0; k * 2 < bought.Length; k++)
            {
                if (bought[k * 2] < 0 || bought[k * 2] >= Nursery.Kinds) continue;
                VillageChoice family = Nursery.Family(bought[k * 2]);
                if (family == VillageChoice.Flowers) flowers++;
                if (family == VillageChoice.ShadeTree && world.BoughtSolved(k)) trees++;
            }
            switch (visitor)
            {
                case Visitor.Heron: return world.HasPairing(Pairing.ByTheWater);   // I374
                case Visitor.Hedgehog: return world.HasPairing(Pairing.Sheltered); // I374
                case Visitor.Owl: return world.HasPairing(Pairing.Grove);          // I374
                case Visitor.Kingfisher: return world.HasPairing(Pairing.Waterside); // I382
                case Visitor.Bees: return flowers >= BeesNeedFlowers;
                case Visitor.Birds: return trees >= BirdsNeedTrees;
                default: return flowers >= 1;
            }
        }

        /// <summary>Whether a welcome visitor is out now: by season and hour, never in the dark.</summary>
        public static bool Out(Visitor visitor, Season season, DayPhase phase)
        {
            bool dark = phase == DayPhase.Dusk || phase == DayPhase.Night;
            // I374: the evening's own visitors. The hedgehog sleeps the winter through; the owl
            // hunts only at night; the heron fishes by day, but not on a frozen river.
            if (visitor == Visitor.Hedgehog) return dark && season != Season.Winter;
            if (visitor == Visitor.Owl) return phase == DayPhase.Night;
            if (dark) return false;
            if (visitor == Visitor.Heron) return season != Season.Winter && phase != DayPhase.Dawn;
            if (visitor == Visitor.Kingfisher) return season != Season.Winter; // I382: by day, while the water is open
            switch (visitor)
            {
                case Visitor.Bees: return season == Season.Spring || season == Season.Summer;
                case Visitor.Birds: return true;
                default: return season == Season.Spring || season == Season.Summer;
            }
        }

        /// <summary>I377: the visitor each keeper's own work brings: Mira's grove the owl, Orin's builds the heron, Fen's flowers the hedgehog.</summary>
        public static Visitor KeeperVisitor(int villager)
        {
            return villager == 0 ? Visitor.Owl : villager == 1 ? Visitor.Heron : Visitor.Hedgehog;
        }

        /// <summary>
        /// I377: what a keeper says about their visitor once it has come, on one day in three
        /// (by the day and the keeper), so it is a remark, not a refrain. Null otherwise.
        /// </summary>
        public static string KeeperRemark(WorldModel world, int villager, int dayNumber)
        {
            Visitor visitor = KeeperVisitor(villager);
            if (!Welcomes(world, visitor) || ((dayNumber + villager) % 3 + 3) % 3 != 0) return null;
            switch (visitor)
            {
                case Visitor.Owl: return "There's an owl in your grove. I heard it last night, calling to the far trees.";
                case Visitor.Heron: return "A heron's taken to fishing by what we built at the water. Patient bird. Good sign.";
                default: return "Something snuffles round your sheltered flowers at dusk. A hedgehog, I'd say. They know a safe spot.";
            }
        }

        /// <summary>
        /// I405: out, and the day's weather suits it: bees and butterflies stay in on rainy and
        /// snowy days (butterflies on windy ones too); the owl keeps to its tree on a rainy night.
        /// Only what is seen changes: the garden's economy never depends on the weather (I353).
        /// </summary>
        public static bool OutIn(Visitor visitor, Season season, DayPhase phase, Weather weather)
        {
            if (!Out(visitor, season, phase)) return false;
            bool wet = weather == Weather.Rainy || weather == Weather.Snowy;
            switch (visitor)
            {
                case Visitor.Bees: return !wet;
                case Visitor.Butterflies: return !wet && weather != Weather.Windy;
                case Visitor.Owl: return weather != Weather.Rainy;
                default: return true;
            }
        }

        /// <summary>Why it comes, as the almanac's visitors page says it.</summary>
        public static string Why(Visitor visitor)
        {
            switch (visitor)
            {
                case Visitor.Heron: return "A heron fishes by something built beside the water, by day, unless the river is frozen.";
                case Visitor.Hedgehog: return "A hedgehog comes at dusk to a flower sheltered by a tree. It sleeps the winter through.";
                case Visitor.Owl: return "An owl comes at night to a grove of two trees.";
                case Visitor.Kingfisher: return "A kingfisher perches over the water by a waterside flower, by day, unless the river is frozen.";
                case Visitor.Bees: return "Bees come once three flowers are growing, on warm days.";
                case Visitor.Birds: return "Birds come once two trees are tended, for the fruit, even in winter.";
                default: return "Butterflies come to any flower in spring and summer.";
            }
        }
    }
}
