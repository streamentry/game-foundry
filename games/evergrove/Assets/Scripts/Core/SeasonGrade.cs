using System;

namespace Evergrove.Core
{
    /// <summary>A colour as 0 to 1 channels, for Core (no UnityEngine). Values are sRGB, as Unity stores them (Gamma project).</summary>
    public readonly struct GradeRgb
    {
        public readonly float R;
        public readonly float G;
        public readonly float B;

        public GradeRgb(float r, float g, float b) { R = r; G = g; B = b; }
    }

    /// <summary>
    /// Run 3, I338–I341: one whole-scene colour grade per season. The painting of the garden is
    /// laid over with a single tint at a single strength, the same alpha blend Unity draws for an
    /// overlay image, so <see cref="Apply"/> is exactly what the screen shows. Things, signs and
    /// text are drawn above the grade and are not graded; that keeps built things as built.
    /// The table is keyed by season only, never by object.
    /// </summary>
    public static class SeasonGrade
    {
        /// <summary>The season's overlay: its tint and how much of it lies over the scene (0 to 1).</summary>
        public readonly struct Grade
        {
            public readonly GradeRgb Tint;
            public readonly float Strength;

            public Grade(GradeRgb tint, float strength) { Tint = tint; Strength = strength; }
        }

        /// <summary>The one grade table: spring fresh, summer sunlit, autumn russet, winter cool.</summary>
        public static Grade For(Season season)
        {
            switch (season)
            {
                case Season.Spring: return new Grade(new GradeRgb(0.97f, 1.00f, 0.86f), 0.06f);
                case Season.Summer: return new Grade(new GradeRgb(1.00f, 0.90f, 0.66f), 0.08f);
                case Season.Autumn: return new Grade(new GradeRgb(0.80f, 0.50f, 0.22f), 0.10f);
                default: return new Grade(new GradeRgb(0.72f, 0.82f, 0.96f), 0.10f); // Winter
            }
        }

        /// <summary>A scene colour as it looks in a season: the overlay blended over it.</summary>
        public static GradeRgb Apply(GradeRgb scene, Season season)
        {
            Grade grade = For(season);
            float k = grade.Strength;
            return new GradeRgb(Mix(scene.R, grade.Tint.R, k), Mix(scene.G, grade.Tint.G, k), Mix(scene.B, grade.Tint.B, k));
        }

        /// <summary>A text colour drawn at an alpha over a background: the colour the eye reads.</summary>
        public static GradeRgb Over(GradeRgb text, float alpha, GradeRgb background)
        {
            return new GradeRgb(Mix(background.R, text.R, alpha), Mix(background.G, text.G, alpha), Mix(background.B, text.B, alpha));
        }

        /// <summary>WCAG relative luminance (the grey level the colour reads as).</summary>
        public static double Luminance(GradeRgb c)
        {
            return 0.2126 * Linear(c.R) + 0.7152 * Linear(c.G) + 0.0722 * Linear(c.B);
        }

        /// <summary>WCAG contrast ratio, 1 to 21, between two colours.</summary>
        public static double Contrast(GradeRgb a, GradeRgb b)
        {
            double la = Luminance(a), lb = Luminance(b);
            return (Math.Max(la, lb) + 0.05) / (Math.Min(la, lb) + 0.05);
        }

        private static float Mix(float from, float to, float amount)
        {
            return from + (to - from) * amount;
        }

        private static double Linear(float v)
        {
            return v <= 0.03928 ? v / 12.92 : Math.Pow((v + 0.055) / 1.055, 2.4);
        }
    }
}
