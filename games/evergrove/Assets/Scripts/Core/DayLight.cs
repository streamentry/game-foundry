using System;

namespace Evergrove.Core
{
    public enum DayPhase { Dawn, Morning, Day, Golden, Dusk, Night }

    /// <summary>A colour wash laid over the garden painting: RGB and how strongly (A).</summary>
    public readonly struct DayTint
    {
        public readonly float R, G, B, A;
        public DayTint(float r, float g, float b, float a) { R = r; G = g; B = b; A = a; }
    }

    /// <summary>
    /// I272: light that follows the player's own local time: rosy dawn, clear day, amber
    /// golden hour, violet dusk, blue night. Cosmetic only. It gates nothing, earns nothing
    /// and is never saved (the vision's "no clock" rule is about progress, and a garden left
    /// alone still waits unchanged). Blends minute by minute with no jumps; the deepest
    /// night wash stays under <see cref="MaxTint"/> so the garden always reads.
    /// </summary>
    public static class DayLight
    {
        public const float MaxTint = 0.36f;

        // Minute of the day → wash. Midnight wraps to the first key.
        private static readonly int[] Minutes = { 0, 270, 360, 450, 660, 960, 1080, 1200, 1290, 1440 };
        private static readonly DayTint[] Keys = {
            new DayTint(0.10f, 0.14f, 0.36f, 0.36f), // 00:00 night
            new DayTint(0.10f, 0.14f, 0.36f, 0.36f), // 04:30 night
            new DayTint(0.95f, 0.55f, 0.55f, 0.16f), // 06:00 dawn, rosy
            new DayTint(1.00f, 0.95f, 0.80f, 0.05f), // 07:30 morning, faint warmth
            new DayTint(1.00f, 1.00f, 1.00f, 0.00f), // 11:00 day, clear
            new DayTint(1.00f, 1.00f, 1.00f, 0.00f), // 16:00 day
            new DayTint(1.00f, 0.70f, 0.30f, 0.18f), // 18:00 golden hour, amber
            new DayTint(0.55f, 0.35f, 0.55f, 0.26f), // 20:00 dusk, violet
            new DayTint(0.10f, 0.14f, 0.36f, 0.36f), // 21:30 night, blue
            new DayTint(0.10f, 0.14f, 0.36f, 0.36f), // 24:00 night
        };

        private static bool Valid(int hour, int minute) { return hour >= 0 && hour < 24 && minute >= 0 && minute < 60; }

        public static DayPhase PhaseAt(int hour, int minute)
        {
            if (!Valid(hour, minute)) return DayPhase.Day;
            if (hour >= 5 && hour < 7) return DayPhase.Dawn;
            if (hour >= 7 && hour < 11) return DayPhase.Morning;
            if (hour >= 11 && hour < 16) return DayPhase.Day;
            if (hour >= 16 && hour < 19) return DayPhase.Golden;
            if (hour >= 19 && hour < 21) return DayPhase.Dusk;
            return DayPhase.Night;
        }

        public static DayTint TintAt(int hour, int minute)
        {
            if (!Valid(hour, minute)) { hour = 13; minute = 0; }
            int at = hour * 60 + minute;
            for (int k = 0; k + 1 < Minutes.Length; k++)
            {
                if (at < Minutes[k] || at > Minutes[k + 1]) continue;
                float t = (at - Minutes[k]) / (float)(Minutes[k + 1] - Minutes[k]);
                DayTint a = Keys[k], b = Keys[k + 1];
                return new DayTint(a.R + (b.R - a.R) * t, a.G + (b.G - a.G) * t, a.B + (b.B - a.B) * t,
                    Math.Min(MaxTint, a.A + (b.A - a.A) * t));
            }
            return Keys[0];
        }

        /// <summary>Fireflies drift at dusk and night, but not in winter.</summary>
        public static bool Fireflies(DayPhase phase, Season season = Season.Summer)
        {
            return season != Season.Winter && (phase == DayPhase.Dusk || phase == DayPhase.Night);
        }
    }
}
