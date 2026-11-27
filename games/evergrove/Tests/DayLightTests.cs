using System;
using Evergrove.Core;

/// <summary>
/// I272: light that follows the player's own time of day. Cosmetic only: it gates nothing
/// and changes nothing in the save. Smooth, never too dark to read the garden.
/// </summary>
internal static class DayLightTests
{
    private static int assertions;

    private static void Expect(bool ok, string message)
    {
        assertions++;
        if (!ok) throw new Exception("Day light test failed: " + message);
    }

    public static int Run()
    {
        // Phases by local time.
        Expect(DayLight.PhaseAt(6, 0) == DayPhase.Dawn, "6:00 is dawn");
        Expect(DayLight.PhaseAt(9, 30) == DayPhase.Morning, "9:30 is morning");
        Expect(DayLight.PhaseAt(13, 0) == DayPhase.Day, "13:00 is day");
        Expect(DayLight.PhaseAt(18, 0) == DayPhase.Golden, "18:00 is golden hour");
        Expect(DayLight.PhaseAt(20, 0) == DayPhase.Dusk, "20:00 is dusk");
        Expect(DayLight.PhaseAt(23, 0) == DayPhase.Night && DayLight.PhaseAt(3, 0) == DayPhase.Night, "night wraps midnight");
        // Bad input reads as midday.
        Expect(DayLight.PhaseAt(-1, 0) == DayPhase.Day && DayLight.PhaseAt(25, 70) == DayPhase.Day, "bad time is day");

        // The tint: midday is clear; night is the strongest but never past the floor.
        DayTint noon = DayLight.TintAt(13, 0);
        Expect(noon.A < 0.01f, "midday adds no tint");
        float most = 0f;
        DayTint last = DayLight.TintAt(0, 0);
        for (int minute = 1; minute <= 24 * 60; minute++)
        {
            DayTint t = DayLight.TintAt((minute / 60) % 24, minute % 60);
            Expect(t.A >= 0f && t.A <= DayLight.MaxTint + 1e-4f, "within the readability floor at " + minute);
            Expect(Math.Abs(t.A - last.A) < 0.02f && Math.Abs(t.R - last.R) < 0.03f && Math.Abs(t.G - last.G) < 0.03f &&
                   Math.Abs(t.B - last.B) < 0.03f, "no jump at minute " + minute);
            if (t.A > most) most = t.A;
            last = t;
        }
        Expect(DayLight.TintAt(23, 0).A >= most - 0.01f, "night carries the deepest tint");
        DayTint night = DayLight.TintAt(23, 0), golden = DayLight.TintAt(18, 0), dawn = DayLight.TintAt(6, 0);
        Expect(night.B > night.R && night.B > night.G, "night is blue");
        Expect(golden.R > golden.B && golden.G > golden.B, "golden hour is amber");
        Expect(dawn.R > dawn.G && dawn.R > dawn.B, "dawn is rosy");

        // Fireflies at dusk and night only.
        Expect(DayLight.Fireflies(DayPhase.Night) && DayLight.Fireflies(DayPhase.Dusk), "fireflies at dusk and night");
        Expect(!DayLight.Fireflies(DayPhase.Day) && !DayLight.Fireflies(DayPhase.Morning) && !DayLight.Fireflies(DayPhase.Dawn),
            "none by day");
        // Winter nights have no fireflies (they'd be wrong); the season decides.
        Expect(!DayLight.Fireflies(DayPhase.Night, Season.Winter) && DayLight.Fireflies(DayPhase.Night, Season.Summer),
            "no fireflies in winter");
        return assertions;
    }
}
