using System;

namespace Evergrove.Core
{
    public enum Visitor { Bees, Birds, Butterflies }

    /// <summary>
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
                case Visitor.Bees: return flowers >= BeesNeedFlowers;
                case Visitor.Birds: return trees >= BirdsNeedTrees;
                default: return flowers >= 1;
            }
        }

        /// <summary>Whether a welcome visitor is out now: by season and hour, never in the dark.</summary>
        public static bool Out(Visitor visitor, Season season, DayPhase phase)
        {
            if (phase == DayPhase.Dusk || phase == DayPhase.Night) return false;
            switch (visitor)
            {
                case Visitor.Bees: return season == Season.Spring || season == Season.Summer;
                case Visitor.Birds: return true;
                default: return season == Season.Spring || season == Season.Summer;
            }
        }

        /// <summary>Why it comes, as the almanac's visitors page says it.</summary>
        public static string Why(Visitor visitor)
        {
            switch (visitor)
            {
                case Visitor.Bees: return "Bees come once three flowers are growing, on warm days.";
                case Visitor.Birds: return "Birds come once two trees are tended, for the fruit, even in winter.";
                default: return "Butterflies come to any flower in spring and summer.";
            }
        }
    }
}
