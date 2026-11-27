using System;

namespace Evergrove.Core
{
    public enum Season { Spring, Summer, Autumn, Winter }

    /// <summary>I295: what drifts over home in a season.</summary>
    public enum Drift { None, Petals, Leaves, Snow }

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

        /// <summary>
        /// I295 (idea 27, after hanami and momijigari): spring petals, autumn leaves and
        /// winter snow drift over home by day; summer is still. In the dark nothing drifts:
        /// that is the fireflies' time (I279).
        /// </summary>
        public static Drift DriftOf(Season season, DayPhase phase)
        {
            if (phase == DayPhase.Dusk || phase == DayPhase.Night) return Drift.None;
            return season == Season.Spring ? Drift.Petals : season == Season.Autumn ? Drift.Leaves
                : season == Season.Winter ? Drift.Snow : Drift.None;
        }
    }
}
