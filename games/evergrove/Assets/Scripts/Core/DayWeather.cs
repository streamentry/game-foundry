using System;

namespace Evergrove.Core
{
    public enum Weather { Fair, Rainy, Windy, Hot, Snowy }

    /// <summary>
    /// I353 (run 3; founder: "should be raining days, snowing days, windy days, normal day, hot
    /// days"): the day's weather. One per local date, the same all day, drawn to fit the
    /// garden's season (snow only in winter, heat only in summer) and a little different in
    /// each garden. Cosmetic, like the light (I272): it never changes a puzzle, a reward or
    /// progress, so a rainy week costs nobody anything. Most days are fair.
    /// </summary>
    public static class DayWeather
    {
        // Chances in percent, by season: fair, rainy, windy, hot, snowy.
        private static readonly int[][] Chances =
        {
            new[] { 55, 25, 20, 0, 0 },   // spring
            new[] { 50, 12, 13, 25, 0 },  // summer
            new[] { 45, 30, 25, 0, 0 },   // autumn
            new[] { 40, 13, 12, 0, 35 },  // winter
        };

        public static Weather For(DateTime localDay, Season season, int garden)
        {
            uint x = unchecked((uint)(localDay.Year * 10000 + localDay.Month * 100 + localDay.Day) * 2654435761u
                                 ^ (uint)(garden + 1) * 2246822519u);
            x ^= x >> 15; x *= 2246822519u; x ^= x >> 13; x *= 3266489917u; x ^= x >> 16;
            int roll = (int)(x % 100u);
            int[] chances = Chances[Math.Max(0, Math.Min(3, (int)season))];
            for (int w = 0; w < chances.Length; w++)
            {
                if (roll < chances[w]) return (Weather)w;
                roll -= chances[w];
            }
            return Weather.Fair;
        }

        /// <summary>A neighbour's true word about the day, or null on a fair day (nothing to say).</summary>
        public static string Remark(Weather weather)
        {
            switch (weather)
            {
                case Weather.Rainy: return "Rain today. The garden's drinking it in, and the river's talking louder.";
                case Weather.Windy: return "A brisk wind today. Everything's leaning and whispering.";
                case Weather.Hot: return "A hot one today. The bees are slow and the shade is busy.";
                case Weather.Snowy: return "Snowing properly today. Everything's gone quiet under it.";
                default: return null;
            }
        }

        public static bool Rain(Weather weather) { return weather == Weather.Rainy; }

        /// <summary>How hard things lean and drift is blown: 1 is the usual breeze.</summary>
        public static float WindStrength(Weather weather)
        {
            switch (weather)
            {
                case Weather.Windy: return 2.6f;
                case Weather.Rainy: return 1.3f;
                case Weather.Snowy: return 1.2f;
                case Weather.Hot: return 0.6f;
                default: return 1f;
            }
        }

        /// <summary>Flakes over home on a snowy day: more than winter's usual drift (I338).</summary>
        public static int SnowfallCount(Weather weather) { return weather == Weather.Snowy ? 90 : 0; }
    }
}
