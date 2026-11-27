namespace Evergrove.Core
{
    /// <summary>
    /// I413 to I417 (run 3, "the river alive"): when the river shows its small moments. All
    /// cosmetic, like the weather (I353): rain rings when it rains; fish rising in the warm
    /// months, best at the edges of the day (which is why the heron and kingfisher come, I374,
    /// I382); glitter on the water in the golden hour of a clear day; autumn leaves carried
    /// downstream. Nothing on a frozen river (I342).
    /// </summary>
    public static class RiverMoments
    {
        public static bool Frozen(Season season) { return SeasonLook.River(season) == RiverLook.IceEdged; } // winter: ice at the edges

        public static bool RainRings(Season season, Weather weather)
        {
            return weather == Weather.Rainy && !Frozen(season);
        }

        /// <summary>Seconds, on average, between two fish rising; 0 when none rise.</summary>
        public static float FishEvery(Season season, DayPhase phase)
        {
            if (Frozen(season) || season == Season.Winter || phase == DayPhase.Night) return 0f;
            bool edge = phase == DayPhase.Dawn || phase == DayPhase.Morning || phase == DayPhase.Golden || phase == DayPhase.Dusk;
            return edge ? 5f : 11f;
        }

        public static bool Glitter(Season season, DayPhase phase, Weather weather)
        {
            return phase == DayPhase.Golden && !Frozen(season) && (weather == Weather.Fair || weather == Weather.Hot || weather == Weather.Windy);
        }

        public static bool LeavesAfloat(Season season) { return season == Season.Autumn; }

        /// <summary>I425: moonlight glints on the water on a clear night (not under rain or snow).</summary>
        public static bool Moonlight(DayPhase phase, Weather weather)
        {
            return phase == DayPhase.Night && weather != Weather.Rainy && weather != Weather.Snowy;
        }
    }
}
