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
        // I358: each neighbour notices the weather in their own way: Mira through the trees,
        // Orin by what's built, Fen by the flowers. Calm, true, and different from each other.
        foreach (Weather w in new[] { Weather.Rainy, Weather.Windy, Weather.Hot, Weather.Snowy })
        {
            var lines = new HashSet<string>();
            for (int villager = 0; villager < 3; villager++)
            {
                string line = DayWeather.Remark(w, villager);
                Expect(line != null && line.Length < 140 && !line.Contains("!"), "a calm line from neighbour " + villager + " on " + w);
                lines.Add(line);
            }
            Expect(lines.Count == 3, "three neighbours, three ways of seeing " + w);
        }
        Expect(DayWeather.Remark(Weather.Fair, 0) == null, "nothing to say on a fair day");
        // I359: since you were last here, told truly: the days' weather you missed, no guilt.
        var last = new DateTime(2026, 3, 1);
        Expect(DayWeather.SinceLastVisit(last, last, Season.Spring, 3) == null, "the same day: nothing to tell");
        Expect(DayWeather.SinceLastVisit(last, last.AddDays(1), Season.Spring, 3) == null, "yesterday: nothing to tell");
        string away = DayWeather.SinceLastVisit(last, last.AddDays(6), Season.Autumn, 3);
        Expect(away != null && away.Contains("looked after itself") && !away.Contains("miss") && !away.Contains("!"),
            "a week away: a calm, true summary: " + away);
        int rainy = 0;
        for (int d = 1; d < 6; d++) if (DayWeather.For(last.AddDays(d), Season.Autumn, 3) == Weather.Rainy) rainy++;
        Expect(rainy == 0 || away.Contains("rain"), "it names rain if it rained: " + away);
        string long_ = DayWeather.SinceLastVisit(last, last.AddDays(400), Season.Winter, 3);
        Expect(long_ != null && long_.Length < 160, "a long time away still reads calmly: " + long_);
        Expect(DayWeather.SinceLastVisit(last.AddDays(3), last, Season.Spring, 3) == null, "a clock set back: nothing");
        // I360: every stretch of river has its own name, the first being home's.
        var names = new HashSet<string>();
        for (int reach = 0; reach < GardenMap.ReachesCeiling; reach++)
        {
            string name = RiverNames.For(reach);
            Expect(!string.IsNullOrEmpty(name) && name.Length <= 18 && !name.Contains("!"), "a short name: " + name);
            Expect(names.Add(name), "each stretch its own name: " + name);
            Expect(RiverNames.For(reach) == name, "always the same name");
        }
        Expect(RiverNames.For(GardenMap.ReachesCeiling + 5) == RiverNames.For(GardenMap.ReachesCeiling - 1) &&
               RiverNames.For(-3) == RiverNames.For(0), "out of range keeps to the ends");
        // I362: Mira's note when the river opens names the new stretch, and stays a letter.
        string note = NeighbourNotice.RiverNote(Season.Spring, RiverNames.For(5));
        Expect(note.Contains(RiverNames.For(5)) && FamilyParcel.ValidLetter(note), "the new stretch by name: " + note);
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
