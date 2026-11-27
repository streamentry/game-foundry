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

        // I358: [villager][weather - 1]: Mira the trees, Orin what's built, Fen the flowers.
        private static readonly string[][] OwnLines =
        {
            new[] {
                "Rain today. The trees are drinking deep; you can almost hear the roots.",
                "The wind's in the branches today. The old ones creak, but they've seen worse.",
                "A hot one. The shade under the trees is the best seat in the garden today.",
                "Snow on every branch today. The trees are asleep under it, and quite content." },
            new[] {
                "Rain today. The gutters are singing and the bridge stones shine.",
                "A brisk wind today. I've checked the roofs; everything's holding.",
                "Hot today. The stones are warm to the touch, even in the shade.",
                "Snowing properly today. I've cleared the bridge so nobody slips." },
            new[] {
                "Rain today. Every flower's holding a little cup of it.",
                "Windy today. The tall flowers are bowing; they'll stand straight tomorrow.",
                "A hot one today. The flowers are folding up a little until evening.",
                "Snow today. Only the snowdrops are up, and they don't seem to mind." },
        };

        /// <summary>I358: the weather as one neighbour sees it, or null on a fair day.</summary>
        public static string Remark(Weather weather, int villager)
        {
            if (weather == Weather.Fair) return null;
            int v = Math.Max(0, Math.Min(2, villager));
            return OwnLines[v][(int)weather - 1];
        }

        private static readonly string[] Counted = { "no", "one", "two", "three", "four", "five", "six", "seven", "eight", "nine" };

        /// <summary>
        /// I359: since you were last here, told truly and without guilt: the weather of the days
        /// in between, and that the garden looked after itself. Null for today or yesterday, and
        /// for a clock set back. Offline, nothing else changes while you're away, so nothing
        /// else is claimed.
        /// </summary>
        public static string SinceLastVisit(DateTime lastDay, DateTime today, Season season, int garden)
        {
            int days = (today.Date - lastDay.Date).Days;
            if (days < 2) return null;
            if (days > 30) return "A good while has passed, with all sorts of weather. The garden looked after itself.";
            int rainy = 0, windy = 0, hot = 0, snowy = 0;
            for (int d = 1; d < days; d++)
                switch (For(lastDay.Date.AddDays(d), season, garden))
                {
                    case Weather.Rainy: rainy++; break;
                    case Weather.Windy: windy++; break;
                    case Weather.Hot: hot++; break;
                    case Weather.Snowy: snowy++; break;
                }
            var parts = new System.Collections.Generic.List<string>();
            if (rainy > 0) parts.Add(Days(rainy, "rainy"));
            if (snowy > 0) parts.Add(Days(snowy, "snowy"));
            if (windy > 0) parts.Add(Days(windy, "windy"));
            if (hot > 0) parts.Add(Days(hot, "hot"));
            string weather = parts.Count == 0 ? "fine weather all the while"
                : parts.Count == 1 ? parts[0]
                : string.Join(", ", parts.GetRange(0, parts.Count - 1).ToArray()) + " and " + parts[parts.Count - 1];
            return "Since you were last here: " + weather + ". The garden looked after itself.";
        }

        private static string Days(int n, string kind)
        {
            string count = n < Counted.Length ? Counted[n] : n.ToString();
            return n == 1 ? "a " + kind + " day" : count + " " + kind + " days";
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
