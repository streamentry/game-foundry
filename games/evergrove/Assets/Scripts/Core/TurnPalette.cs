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

        private static readonly float[][] Colours =
        {
            new float[] { 0.42f, 0.57f, 0.43f }, // sage
            new float[] { 0.97f, 0.87f, 0.60f }, // honey
            new float[] { 0.64f, 0.77f, 0.91f }, // sky
            new float[] { 0.84f, 0.56f, 0.60f }, // rose
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
