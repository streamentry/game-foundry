using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Evergrove.Core;

/// <summary>
/// CP42: the companion's day turns at 4 a.m. (a 00:30 visit still belongs to the evening before), so
/// the ranker's and memory's day moves at 04:00. Facts keep the real calendar date: weather, season,
/// "rain coming tomorrow" and the last visit are not shifted.
/// </summary>
internal static class CompanionDayTests
{
    private static int assertions;

    private static void Expect(bool ok, string message)
    {
        assertions++;
        if (!ok) throw new Exception("Companion day test failed: " + message);
    }

    public static int Run()
    {
        assertions = 0;
        Rollover();
        MidnightDatesUnchanged();
        FactsKeepTheRealDate();
        SignatureFollowsCompanionDay();
        WholeDayOffsetsStillAdvanceOneDay();
        return assertions;
    }

    private static void Rollover()
    {
        Expect(CompanionContext.DayTurnsAtHour == 4, "the day turns at 4 a.m.");
        int evening = CompanionContext.CompanionDayOf(new DateTime(2026, 11, 27, 23, 50, 0));
        int lateNight = CompanionContext.CompanionDayOf(new DateTime(2026, 11, 28, 3, 59, 0));
        int morning = CompanionContext.CompanionDayOf(new DateTime(2026, 11, 28, 4, 0, 0));
        Expect(evening == lateNight, "23:50 and 03:59 are the same companion day");
        Expect(morning == evening + 1, "04:00 is the next companion day");
        Expect(CompanionContext.CompanionDayOf(new DateTime(2026, 11, 28, 0, 30, 0)) == CompanionContext.DayNumberOf(new DateTime(2026, 11, 27)),
            "00:30 belongs to the evening before");
    }

    private static void MidnightDatesUnchanged()
    {
        var nov27 = new DateTime(2026, 11, 27);
        var nov28 = new DateTime(2026, 11, 28);
        Expect(CompanionContext.DayNumberOf(nov28) - CompanionContext.DayNumberOf(nov27) == 1, "consecutive dates differ by one");
        Expect(new CompanionContext(nov28).DayNumber == CompanionContext.DayNumberOf(nov28),
            "a context with no clock keeps the date's day number");
        Expect(CompanionContext.DateOfDay(CompanionContext.DayNumberOf(nov28)) == nov28, "DateOfDay still inverts DayNumberOf");
    }

    private static void FactsKeepTheRealDate()
    {
        WorldModel world = CompanionFactsTests.Grown(3);
        var nov28 = new DateTime(2026, 11, 28);
        var lateNight = new DateTime(2026, 11, 28, 0, 30, 0);
        var lastSeen = new DateTime(2026, 11, 20);

        var atMidnight = new CompanionContext(nov28, lastSeen: lastSeen, weather: Weather.Rainy);
        var atHalfPastMidnight = new CompanionContext(lateNight.Date, lastSeen: lastSeen, weather: Weather.Rainy, localNow: lateNight);

        Expect(atHalfPastMidnight.Today == nov28, "the context's date is the real calendar date at 00:30");
        Expect(atHalfPastMidnight.DayNumber == CompanionContext.DayNumberOf(new DateTime(2026, 11, 27)),
            "the companion's day at 00:30 is the evening before");

        string Weatherish(CompanionContext context)
        {
            IEnumerable<string> kept = CompanionFacts.For(world, context)
                .Where(f => f.Kind == FactKind.WeatherToday || f.Kind == FactKind.RainComing
                    || f.Kind == FactKind.WeatherWhileAway || f.Kind == FactKind.SeasonTurned || f.Kind == FactKind.Hour)
                .Select(f => f.Kind + "|" + f.Key + "|" + string.Join(",", f.Slots.Select(s => s.Key + "=" + s.Value)));
            return string.Join(";", kept.OrderBy(s => s, StringComparer.Ordinal));
        }

        Expect(Weatherish(atHalfPastMidnight) == Weatherish(atMidnight),
            "weather, season, rain coming and the last visit read the real date, not the companion's day");
        Expect(CompanionFacts.For(world, atHalfPastMidnight).Any(f => f.Kind == FactKind.WeatherToday),
            "today's rain is still a fact at 00:30");
        Expect(Weatherish(atHalfPastMidnight).Contains("away:20261120"),
            "the last visit keeps its real date at 00:30");
    }

    private static void SignatureFollowsCompanionDay()
    {
        WorldModel world = CompanionFactsTests.Grown(3);
        var nov28 = new DateTime(2026, 11, 28);
        var before4 = new CompanionContext(nov28, localNow: new DateTime(2026, 11, 28, 3, 59, 0));
        var after4 = new CompanionContext(nov28, localNow: new DateTime(2026, 11, 28, 4, 0, 0));
        // CP42: the same real date, a new companion day: the facts cache must not serve the old list.
        Expect(CompanionFacts.Signature(world, before4) != CompanionFacts.Signature(world, after4),
            "the companion's day at 04:00 changes the signature");
    }

    private static void WholeDayOffsetsStillAdvanceOneDay()
    {
        // The QA tour adds whole days to the clock; each scripted day must be exactly one companion day.
        foreach (DateTime now in new[] { new DateTime(2026, 11, 28, 0, 30, 0), new DateTime(2026, 11, 28, 13, 0, 0) })
            for (int offset = 0; offset < 8; offset++)
                Expect(CompanionContext.CompanionDayOf(now.AddDays(offset)) == CompanionContext.CompanionDayOf(now) + offset,
                    "a scripted day moves the companion's day by one");
    }
}
