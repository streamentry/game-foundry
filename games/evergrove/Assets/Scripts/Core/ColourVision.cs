using System;

namespace Evergrove.Core
{
    /// <summary>
    /// Colour-vision simulation and WCAG contrast for the water and dry pipe channels.
    /// Machado, Oliveira and Fernandes (2009), severity 1.0, applied in linear RGB.
    /// Pure: no Unity. Used by tests to prove the two channels stay distinct for players
    /// with dichromacy, not only that each channel has 3:1 against its tile.
    /// </summary>
    public static class ColourVision
    {
        public enum Vision { Normal, Protanopia, Deuteranopia, Tritanopia }

        private static readonly double[][] Protanopia =
        {
            new double[] { 0.152286, 1.052583, -0.204868 },
            new double[] { 0.114503, 0.786281, 0.099216 },
            new double[] { -0.003882, -0.048116, 1.051998 },
        };

        private static readonly double[][] Deuteranopia =
        {
            new double[] { 0.367322, 0.860646, -0.227968 },
            new double[] { 0.280085, 0.672501, 0.047413 },
            new double[] { -0.011820, 0.042940, 0.968881 },
        };

        private static readonly double[][] Tritanopia =
        {
            new double[] { 1.255528, -0.076749, -0.178779 },
            new double[] { -0.078411, 0.930809, 0.147602 },
            new double[] { 0.004733, 0.691367, 0.303900 },
        };

        /// <summary>
        /// Simulates how an sRGB colour (0..1 per channel) appears under the given vision type.
        /// Returns a fresh 0..1 sRGB array. Normal returns the input unchanged.
        /// </summary>
        public static double[] Simulate(double r, double g, double b, Vision v)
        {
            if (v == Vision.Normal) return new double[] { r, g, b };

            double[][] m = MatrixFor(v);
            double lr = SrgbToLinear(r);
            double lg = SrgbToLinear(g);
            double lb = SrgbToLinear(b);

            double[] linear =
            {
                Clamp01(m[0][0] * lr + m[0][1] * lg + m[0][2] * lb),
                Clamp01(m[1][0] * lr + m[1][1] * lg + m[1][2] * lb),
                Clamp01(m[2][0] * lr + m[2][1] * lg + m[2][2] * lb),
            };

            return new double[]
            {
                LinearToSrgb(linear[0]),
                LinearToSrgb(linear[1]),
                LinearToSrgb(linear[2]),
            };
        }

        /// <summary>WCAG 2.x relative luminance of an sRGB colour (0..1 per channel).</summary>
        public static double RelativeLuminance(double r, double g, double b)
        {
            return 0.2126 * SrgbToLinear(r) + 0.7152 * SrgbToLinear(g) + 0.0722 * SrgbToLinear(b);
        }

        /// <summary>WCAG contrast ratio between two relative luminances: (max+0.05)/(min+0.05).</summary>
        public static double Contrast(double l1, double l2)
        {
            double max = Math.Max(l1, l2);
            double min = Math.Min(l1, l2);
            return (max + 0.05) / (min + 0.05);
        }

        /// <summary>
        /// Contrast between two sRGB colours (each a 3-element 0..1 array) as seen under vision type v.
        /// </summary>
        public static double ContrastUnder(Vision v, double[] a, double[] b)
        {
            double[] sa = Simulate(a[0], a[1], a[2], v);
            double[] sb = Simulate(b[0], b[1], b[2], v);
            return Contrast(
                RelativeLuminance(sa[0], sa[1], sa[2]),
                RelativeLuminance(sb[0], sb[1], sb[2]));
        }

        private static double[][] MatrixFor(Vision v)
        {
            switch (v)
            {
                case Vision.Protanopia: return Protanopia;
                case Vision.Deuteranopia: return Deuteranopia;
                case Vision.Tritanopia: return Tritanopia;
                default: throw new ArgumentOutOfRangeException(nameof(v));
            }
        }

        private static double SrgbToLinear(double c)
        {
            return c <= 0.04045 ? c / 12.92 : Math.Pow((c + 0.055) / 1.055, 2.4);
        }

        private static double LinearToSrgb(double c)
        {
            return c <= 0.0031308 ? 12.92 * c : 1.055 * Math.Pow(c, 1.0 / 2.4) - 0.055;
        }

        private static double Clamp01(double x)
        {
            return x < 0 ? 0 : x > 1 ? 1 : x;
        }
    }
}
