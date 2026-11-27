using System;
using System.Collections.Generic;
using Evergrove.Core;

/// <summary>
/// Checklist 48: "Weather moves down the river. Rain upstream today reaches you tomorrow, said
/// by a neighbour. Test: same seed, same forecast." Pure, deterministic, no clock.
/// </summary>
internal static class RiverWeatherTests
{
    private static int assertions;

    private static void Expect(bool ok, string message)
    {
        assertions++;
        if (!ok) throw new Exception("RiverWeather test failed: " + message);
    }

    private static readonly int[] Gardens = { 0, 1, 2, 7, 42, 1000000 };
    private static readonly Season[] Seasons = { Season.Spring, Season.Summer, Season.Autumn, Season.Winter };

    // Frozen from DayWeather.For before this change: year, month, day, season, garden, weather (enum value).
    // DayWeather is untouched, so every existing day keeps its weather.
    private static readonly int[][] FrozenDays =
    {
        new[] { 2026, 3, 14, 0, 0, 2 }, new[] { 2026, 6, 1, 1, 1, 1 }, new[] { 2026, 11, 27, 2, 2, 1 },
        new[] { 2027, 1, 9, 3, 7, 0 }, new[] { 2026, 7, 4, 1, 42, 3 }, new[] { 2028, 2, 29, 0, 1000000, 0 },
        new[] { 2026, 11, 28, 2, 0, 0 }, new[] { 2026, 12, 25, 3, 3, 0 }, new[] { 2027, 4, 1, 0, 5, 0 },
        new[] { 2027, 8, 8, 1, 99, 3 }, new[] { 2026, 10, 31, 2, 1, 2 }, new[] { 2027, 6, 21, 1, 1000, 0 },
        new[] { 2026, 1, 1, 3, 7, 0 }, new[] { 2026, 2, 14, 0, 7, 1 }, new[] { 2026, 5, 30, 1, 8, 0 },
        new[] { 2026, 9, 23, 2, 8, 0 }, new[] { 2026, 12, 31, 3, 8, 0 }, new[] { 2027, 3, 20, 0, 12, 0 },
        new[] { 2027, 7, 15, 1, 12, 3 }, new[] { 2027, 10, 4, 2, 12, 1 }, new[] { 2028, 1, 1, 3, 0, 0 },
        new[] { 2028, 5, 5, 0, 3, 0 }, new[] { 2028, 9, 9, 2, 3, 2 }, new[] { 2029, 6, 6, 1, 4, 3 },
        new[] { 2029, 11, 11, 3, 4, 4 }, new[] { 2026, 4, 12, 0, 250, 0 }, new[] { 2026, 8, 20, 1, 250, 3 },
        new[] { 2026, 11, 3, 2, 250, 0 }, new[] { 2026, 12, 1, 3, 250, 0 }, new[] { 2027, 2, 2, 0, 77, 0 },
        new[] { 2027, 5, 17, 1, 77, 0 }, new[] { 2027, 9, 30, 2, 77, 2 }, new[] { 2028, 12, 12, 3, 77, 0 },
        new[] { 2026, 3, 3, 0, 6, 2 }, new[] { 2026, 6, 30, 1, 6, 1 },
    };

    public static int Run()
    {
        assertions = 0;
        var start = new DateTime(2026, 1, 1);
        const int dayCount = 1096; // 2026 to 2028, leap year included

        // Same inputs, same forecast; upstream rain today is rain at home tomorrow, both ways.
        int rainyUpstream = 0, dryUpstream = 0, valleyDiffers = 0, comingDays = 0;
        foreach (Season season in Seasons)
            foreach (int garden in Gardens)
                for (int d = 0; d < dayCount; d++)
                {
                    DateTime day = start.AddDays(d);
                    Weather up = RiverWeather.Upstream(day, garden, season);
                    Weather tomorrow = RiverWeather.Tomorrow(day, garden, season);
                    Expect(up == RiverWeather.Upstream(day.AddHours(13), garden, season), "the same upstream weather all day: " + day);
                    Expect(tomorrow == RiverWeather.Tomorrow(day.AddHours(13), garden, season), "the same forecast all day: " + day);
                    Expect(tomorrow == DayWeather.For(day.AddDays(1), season, garden), "the forecast is home's own weather tomorrow: " + day);
                    Expect((up == Weather.Rainy) == (tomorrow == Weather.Rainy),
                        "rain upstream today is rain at home tomorrow, both ways: " + day + " " + season + " garden " + garden);
                    Expect(RiverWeather.RainComing(day, garden, season) == (tomorrow == Weather.Rainy), "the coming rain matches the forecast: " + day);
                    Expect(up != Weather.Snowy || season == Season.Winter, "snow upstream only in winter: " + day);
                    Expect(up != Weather.Hot || season == Season.Summer, "heat upstream only in summer: " + day);
                    if (up == Weather.Rainy) rainyUpstream++;
                    else dryUpstream++;
                    if (up != Weather.Rainy && up != tomorrow) valleyDiffers++;
                    if (RiverWeather.RainComing(day, garden, season)) comingDays++;
                }
        Expect(rainyUpstream > 100 && rainyUpstream < 4 * Gardens.Length * dayCount, "rain comes down the river some days: " + rainyUpstream);
        Expect(dryUpstream > rainyUpstream, "most days the river brings no rain");
        Expect(valleyDiffers > 100, "the valley's dry days have a sky of their own: " + valleyDiffers);
        Expect(comingDays == rainyUpstream, "every rainy upstream day is a day rain is coming");

        // Existing weather is unchanged: frozen values from the current DayWeather.
        foreach (int[] row in FrozenDays)
        {
            var day = new DateTime(row[0], row[1], row[2]);
            Weather got = DayWeather.For(day, (Season)row[3], row[4]);
            Expect((int)got == row[5], "day weather unchanged on " + day.ToString("yyyy-MM-dd") + " garden " + row[4] + ": " + got);
        }

        // The fact: said by Orin only, on the day before rain reaches home, in the words of the line.
        Expect(RiverWeather.ComingLine == "Rain on the hills upstream today. It'll reach the garden by tomorrow.", "the line's words");
        Expect(RiverWeather.ComingLine.Length <= 90 && !RiverWeather.ComingLine.Contains("!"), "a calm line, under 90 characters");
        int facts = 0;
        foreach (int garden in Gardens)
        {
            WorldModel world = LivingGarden(garden);
            Season season = GardenSeason.For(world);
            for (int d = 0; d < dayCount; d++)
            {
                DateTime today = start.AddDays(d);
                IReadOnlyList<Fact> feed = CompanionFacts.For(world, new CompanionContext(today));
                Fact coming = Find(feed, FactKind.RainComing, 1);
                bool rainTomorrow = RiverWeather.Tomorrow(today, garden, season) == Weather.Rainy;
                Expect((coming != null) == rainTomorrow, "the fact appears only when rain is coming: " + today + " garden " + garden);
                if (coming == null) continue;
                facts++;
                Expect(CompanionSpeech.Legacy(coming, 1) == RiverWeather.ComingLine, "Orin says the line: " + coming);
                Expect(CompanionSpeech.Legacy(coming, 0) == null && CompanionSpeech.Legacy(coming, 2) == null,
                    "only Orin says it");
                Expect(coming.Slot(CompanionFacts.LegacyOffSlot) != null, "the old chain never picks it, so its comparison holds");
                Expect(CompanionSpeech.LegacyPick(feed, 1) != coming, "the old chain does not say it");
            }
        }
        Expect(facts > 50, "rain-coming facts exercised across gardens: " + facts);

        // Nothing is said on a dry day, and no existing fact changes its kind or keeper.
        Expect(Find(CompanionFacts.For(LivingGarden(7), new CompanionContext(start)), FactKind.RainComing, 0) == null,
            "never said by Mira");
        return assertions;
    }

    private static WorldModel LivingGarden(int garden)
    {
        WorldSnapshot snapshot = WorldModel.New(PlayMode.Living).Export();
        snapshot.garden = garden;
        Expect(WorldModel.TryRestore(snapshot, out WorldModel world), "a living garden restores");
        return world;
    }

    private static Fact Find(IReadOnlyList<Fact> feed, FactKind kind, int keeper)
    {
        foreach (Fact f in feed)
            if (f.Kind == kind && f.Keeper == keeper) return f;
        return null;
    }
}
