using System;

namespace Evergrove.Core
{
    /// <summary>
    /// Checklist 48: weather moves down the river. Rain upstream today reaches the garden
    /// tomorrow, and a neighbour says so. The rule holds both ways: upstream is rainy today
    /// exactly when home is rainy tomorrow. It is built on <see cref="DayWeather"/> without
    /// changing it, so every existing day keeps its weather. The forecast is true by
    /// construction: upstream today is read off tomorrow's own weather. Cosmetic, like all
    /// weather (I353): it never changes a puzzle, a reward or progress.
    /// </summary>
    public static class RiverWeather
    {
        // A separate stream of sky for the valley above, so dry upstream days still vary.
        private const int UpstreamSalt = 7919;

        /// <summary>
        /// What Orin says on the day before the rain reaches home. Orin notices what is built,
        /// and the bridge and gutters are what the river's rain touches first.
        /// </summary>
        public const string ComingLine = "Rain on the hills upstream today. It'll reach the garden by tomorrow.";

        /// <summary>Tomorrow's weather at home: the day the river's weather arrives.</summary>
        public static Weather Tomorrow(DateTime day, int garden, Season season)
        {
            return DayWeather.For(day.Date.AddDays(1), season, garden);
        }

        /// <summary>
        /// Today's weather upstream. Rain upstream today is rain at home tomorrow. Otherwise the
        /// valley has its own sky, which is never rain, so the rule holds in both directions.
        /// </summary>
        public static Weather Upstream(DateTime day, int garden, Season season)
        {
            if (Tomorrow(day, garden, season) == Weather.Rainy) return Weather.Rainy;
            Weather above = DayWeather.For(day.Date, season, garden + UpstreamSalt);
            return above == Weather.Rainy ? Weather.Fair : above;
        }

        /// <summary>True on the day before rain reaches home: the neighbour says it is coming.</summary>
        public static bool RainComing(DateTime day, int garden, Season season)
        {
            return Tomorrow(day, garden, season) == Weather.Rainy;
        }
    }
}
