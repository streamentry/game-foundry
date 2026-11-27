using System;
using System.Collections.Generic;
using Evergrove.Core;

/// <summary>
/// I353 (run 3, founder: "should be raining days, snowing days, windy days, normal day,
/// hot days"): the day's weather. One per local date, the same all day, fitting the garden's
/// season. Cosmetic only: it never changes a puzzle, a reward or progress.
/// </summary>
internal static class WeatherTests
{
    private static int assertions;

    private static void Expect(bool ok, string message)
    {
        assertions++;
        if (!ok) throw new Exception("Weather test failed: " + message);
    }

    public static int Run()
    {
        var start = new DateTime(2026, 1, 1);
        foreach (Season season in new[] { Season.Spring, Season.Summer, Season.Autumn, Season.Winter })
        {
            var counts = new Dictionary<Weather, int>();
            for (int d = 0; d < 365; d++)
            {
                DateTime day = start.AddDays(d);
                Weather w = DayWeather.For(day, season, 7);
                counts[w] = counts.TryGetValue(w, out int c) ? c + 1 : 1;
                Expect(w == DayWeather.For(day.AddHours(13), season, 7), "the same weather all day");
                if (w == Weather.Snowy) Expect(season == Season.Winter, "snow only in winter");
                if (w == Weather.Hot) Expect(season == Season.Summer, "hot days only in summer");
            }
            Expect(counts.TryGetValue(Weather.Fair, out int fair) && fair >= 100, season + ": most days are fair (" + fair + ")");
            Expect(counts.TryGetValue(Weather.Rainy, out int rain) && rain >= 20, season + ": some rainy days (" + rain + ")");
            Expect(counts.TryGetValue(Weather.Windy, out int wind) && wind >= 15, season + ": some windy days (" + wind + ")");
            if (season == Season.Winter) Expect(counts.TryGetValue(Weather.Snowy, out int snow) && snow >= 40, "winter snows (" + snow + ")");
            if (season == Season.Summer) Expect(counts.TryGetValue(Weather.Hot, out int hot) && hot >= 30, "summer has hot days (" + hot + ")");
        }
        // Days vary: a week is rarely all one weather.
        var week = new HashSet<Weather>();
        for (int d = 0; d < 14; d++) week.Add(DayWeather.For(start.AddDays(d), Season.Spring, 7));
        Expect(week.Count >= 2, "two weeks bring more than one kind of day");
        // A family sees the same sky on the same day only if they share a garden number: each garden its own.
        int differ = 0;
        for (int d = 0; d < 60; d++)
            if (DayWeather.For(start.AddDays(d), Season.Autumn, 1) != DayWeather.For(start.AddDays(d), Season.Autumn, 2)) differ++;
        Expect(differ > 10, "different gardens have their own weather");
        // Neighbours say true things about it, calmly.
        foreach (Weather w in Enum.GetValues(typeof(Weather)))
        {
            string line = DayWeather.Remark(w);
            Expect(w == Weather.Fair ? line == null : line != null && line.Length < 140 && !line.Contains("!"), "a calm remark for " + w);
        }
        // What it looks like, for the map.
        Expect(DayWeather.Rain(Weather.Rainy) && !DayWeather.Rain(Weather.Fair), "rain falls on rainy days");
        Expect(DayWeather.WindStrength(Weather.Windy) > DayWeather.WindStrength(Weather.Fair), "wind blows harder on windy days");
        Expect(DayWeather.SnowfallCount(Weather.Snowy) > SeasonLook.DriftCount(Drift.Snow), "snowy days snow harder than winter's usual");
        // Its sound (I354–I356): rain patters, wind blows, heat hums by day only.
        Expect(Soundscape.WeatherVoice(Weather.Rainy, DayPhase.Day) == AmbientVoice.Rain, "rain patters");
        Expect(Soundscape.WeatherVoice(Weather.Windy, DayPhase.Night) == AmbientVoice.Wind, "wind day and night");
        Expect(Soundscape.WeatherVoice(Weather.Hot, DayPhase.Day) == AmbientVoice.Crickets &&
               Soundscape.WeatherVoice(Weather.Hot, DayPhase.Night) == null, "heat hums by day only");
        Expect(Soundscape.WeatherVoice(Weather.Fair, DayPhase.Day) == null, "a fair day leaves the season's own voices");
        Expect(Soundscape.SecondsBetween(AmbientVoice.Rain) >= 2f, "patter, but never a din");
        return assertions;
    }
}
