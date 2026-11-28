using System;

namespace Evergrove.Core
{
    /// <summary>
    /// Four take-turns colours (sage, honey, sky, rose) with distinct lightness steps, so they
    /// still differ in greyscale. v1 #92 / vision run 20261128-0927 idea 4. Pure: no Unity.
    /// </summary>
    public static class TurnPalette
    {
        public const int Count = 4;

        // Honey and rose were darkened or lightened from the first proposal (honey 0.97,0.87,0.60
        // -> 1.00,0.91,0.62; rose 0.84,0.56,0.60 -> 0.56,0.37,0.40) because under protanopia rose
        // sat at 1.22:1 against sage and honey at 1.25:1 against sky. These two colours are the
        // fewest changes that lift every pair under Normal, protanopia, deuteranopia and tritanopia
        // to at least 1.36:1 (smallest pair now 1.365:1, honey-sky under protanopia).
        private static readonly float[][] Colours =
        {
            new float[] { 0.42f, 0.57f, 0.43f }, // sage
            new float[] { 1.00f, 0.91f, 0.62f }, // honey
            new float[] { 0.64f, 0.77f, 0.91f }, // sky
            new float[] { 0.56f, 0.37f, 0.40f }, // rose
        };

        /// <summary>RGB in 0..1 for a turn index; wraps around. Returns a fresh copy.</summary>
        public static float[] Rgb(int turn)
        {
            int index = ((turn % Count) + Count) % Count;
            float[] source = Colours[index];
            return new float[] { source[0], source[1], source[2] };
        }

        /// <summary>WCAG 2.x relative luminance of an sRGB colour.</summary>
        public static double Luminance(float r, float g, float b)
        {
            return 0.2126 * Linear(r) + 0.7152 * Linear(g) + 0.0722 * Linear(b);
        }

        /// <summary>WCAG contrast ratio between two relative luminances.</summary>
        public static double Contrast(double l1, double l2)
        {
            double max = Math.Max(l1, l2);
            double min = Math.Min(l1, l2);
            return (max + 0.05) / (min + 0.05);
        }

        private static double Linear(float c)
        {
            return c <= 0.04045 ? c / 12.92 : Math.Pow((c + 0.055) / 1.055, 2.4);
        }
    }
}
