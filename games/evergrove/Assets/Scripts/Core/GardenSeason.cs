using System;

namespace Evergrove.Core
{
    public enum Season { Spring, Summer, Autumn, Winter }

    /// <summary>
    /// I248: four seasons, by progress, never by the clock (vision: "seasons and chapters
    /// by progress"; a garden left alone waits unchanged). The story walks spring to
    /// winter, three streams a season. The festival that ends it is summer; after it,
    /// every few first solves (the drops on the mist, I224) turn the year on, and it keeps
    /// turning past the river's ceiling. Only the look and the words change: nothing is
    /// earned, gated or missed by season.
    /// </summary>
    public static class GardenSeason
    {
        public static Season ForStory(int landscapePhase)
        {
            return (Season)Math.Max(0, Math.Min(3, landscapePhase));
        }

        public static Season AfterStory(int puzzlesSolved)
        {
            int turns = Math.Max(0, puzzlesSolved) / GardenMap.SolvesPerReach;
            return (Season)(((int)Season.Summer + turns) % 4);
        }

        public static Season For(WorldModel world)
        {
            if (world == null || world.Mode != PlayMode.Living) return Season.Summer;
            return world.Completed ? AfterStory(world.PuzzlesSolved)
                : ForStory(VillageScenePlan.From(world).LandscapePhase);
        }
    }
}
