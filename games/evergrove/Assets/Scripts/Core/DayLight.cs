using System;

namespace Evergrove.Core
{
    public enum DayPhase { Dawn, Morning, Day, Golden, Dusk, Night }

    /// <summary>
    /// The light laid over the garden painting: a colour wash (RGB, strength A) and, under it,
    /// a darkening (Dark, I282 tune: a wash alone barely read as night).
    /// </summary>
    public readonly struct DayTint
    {
        public readonly float R, G, B, A, Dark;
        public DayTint(float r, float g, float b, float a, float dark = 0f) { R = r; G = g; B = b; A = a; Dark = dark; }
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
        public const float MaxDark = 0.48f;   // the deepest night still shows the garden plainly

        // Minute of the day → wash. Midnight wraps to the first key.
        private static readonly int[] Minutes = { 0, 270, 360, 450, 660, 960, 1080, 1200, 1290, 1440 };
        private static readonly DayTint[] Keys = {
            new DayTint(0.10f, 0.14f, 0.36f, 0.30f, 0.45f), // 00:00 night
            new DayTint(0.10f, 0.14f, 0.36f, 0.30f, 0.45f), // 04:30 night
            new DayTint(1.00f, 0.62f, 0.74f, 0.24f, 0.06f), // 06:00 dawn, rosy pink (b296: read olive)
            new DayTint(1.00f, 0.95f, 0.80f, 0.06f, 0.00f), // 07:30 morning, faint warmth
            new DayTint(1.00f, 1.00f, 1.00f, 0.00f, 0.00f), // 11:00 day, clear
            new DayTint(1.00f, 1.00f, 1.00f, 0.00f, 0.00f), // 16:00 day
            new DayTint(1.00f, 0.58f, 0.18f, 0.28f, 0.05f), // 18:00 golden hour, deep amber
            new DayTint(0.55f, 0.35f, 0.55f, 0.28f, 0.25f), // 20:00 dusk, violet
            new DayTint(0.10f, 0.14f, 0.36f, 0.30f, 0.45f), // 21:30 night, blue
            new DayTint(0.10f, 0.14f, 0.36f, 0.30f, 0.45f), // 24:00 night
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
                    Math.Min(MaxTint, a.A + (b.A - a.A) * t), Math.Min(MaxDark, a.Dark + (b.Dark - a.Dark) * t));
            }
            return Keys[0];
        }

        /// <summary>
        /// I281: a neighbour notices the hour, kindly, only when the light is worth a word (dawn,
        /// golden hour, dusk, night). Never "you're up late"; never a reason to stay or go.
        /// </summary>
        public static string Remark(int villager, DayPhase phase, Season season)
        {
            if (villager < 0 || villager > 2) return null;
            bool winter = season == Season.Winter;
            switch (phase)
            {
                case DayPhase.Dawn:
                    return villager == 0 ? "First light on the water. It's the quietest hour on the river."
                        : villager == 1 ? "The bridge looks new every morning in this light."
                        : "The dew is still on the petals. They'll open as the sun finds them.";
                case DayPhase.Golden:
                    return villager == 0 ? "The trees are throwing long shadows. My favourite light."
                        : villager == 1 ? "This light makes the old stones look like honey."
                        : "The bees are making their last rounds of the day.";
                case DayPhase.Dusk:
                    return villager == 0 ? "The river goes silver at dusk."
                        : villager == 1 ? "I've lit the lamp by the bridge."
                        : winter ? "The flowers have folded up against the cold. They'll keep."
                                 : "The first fireflies are out over the meadow.";
                case DayPhase.Night:
                    return villager == 0 ? (winter ? "Frost on the water tonight. The stars are very clear." : "You can hear the river better at night.")
                        : villager == 1 ? "The cottage windows look warm from out here."
                        : winter ? "Not a firefly in sight in this cold. The stars make up for it."
                                 : "The fireflies are out. Mind the steps by the creek.";
                default:
                    return null;
            }
        }

        /// <summary>Fireflies drift at dusk and night, but not in winter.</summary>
        public static bool Fireflies(DayPhase phase, Season season = Season.Summer)
        {
            return season != Season.Winter && (phase == DayPhase.Dusk || phase == DayPhase.Night);
        }
    }
}
