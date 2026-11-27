using System;
using Evergrove.Core;

/// <summary>I419 to I422: the companion notices gently, from device-only facts.</summary>
internal static class CompanionTests
{
    private static int assertions;

    private static void Expect(bool ok, string message)
    {
        assertions++;
        if (!ok) throw new Exception("Companion test failed: " + message);
    }

    public static int Run()
    {
        assertions = 0;
        Expect(Companion.Habit(null) == null && Companion.Habit(new int[3]) == null, "no counts, no habit");
        Expect(Companion.Habit(new[] { 0, 2, 0, 0, 3, 0 }) == null, "too few returns to say");
        Expect(Companion.Habit(new[] { 1, 1, 1, 1, 2, 1 }) == null, "no clear hour");
        Expect(Companion.Habit(new[] { 0, 1, 0, 1, 6, 0 }) == DayPhase.Dusk, "an evening visitor");
        int said = 0;
        int[] evenings = { 0, 1, 0, 1, 6, 0 };
        for (int day = 0; day < 70; day++)
        {
            for (int v = 0; v < 3; v++)
            {
                string line = Companion.HabitLine(evenings, v, day);
                if (line == null) continue;
                Expect(v == 1, "only Orin notices evenings");
                Expect(!line.Contains("!") && !line.ToLowerInvariant().Contains("streak"), "calm, no streaks");
                said++;
            }
        }
        Expect(said == 10, "one day in seven: " + said);
        Expect(Companion.WelcomeBack(3) == null && Companion.WelcomeBack(7) != null && Companion.WelcomeBack(40) != Companion.WelcomeBack(7),
            "a welcome after a week away, a warmer one after a month");
        foreach (int days in new[] { 7, 40 })
            Expect(!Companion.WelcomeBack(days).ToLowerInvariant().Contains("miss") && Companion.WelcomeBack(days).Contains("lost"),
                "reassurance, never guilt");
        var first = new DateTime(2026, 3, 14);
        Expect(Companion.Anniversary(first, new DateTime(2027, 3, 14)) == 1 && Companion.Anniversary(first, new DateTime(2028, 3, 14)) == 2, "a year, two years");
        Expect(Companion.Anniversary(first, new DateTime(2027, 3, 15)) == 0 && Companion.Anniversary(first, first) == 0, "only on the day, never the first one");
        Expect(Companion.Anniversary(new DateTime(2028, 2, 29), new DateTime(2029, 2, 28)) == 1, "a leap-day garden keeps its day");
        Expect(Companion.AnniversaryNote(1).StartsWith("A year") && Companion.AnniversaryNote(3).StartsWith("3 years"), "the note counts years");
        Expect(Companion.QuietHours(23) && Companion.QuietHours(5) && !Companion.QuietHours(7) && !Companion.QuietHours(21), "quiet from ten to seven");
        Expect(Companion.Volume(23) < Companion.Volume(12), "softer at night");
        return assertions;
    }
}
