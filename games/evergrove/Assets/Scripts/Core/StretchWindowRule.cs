using System;

namespace Evergrove.Core
{
    /// <summary>
    /// Checklist 50, the performance budget for an endless river: only the stretches on screen, plus one
    /// on either side, are kept live; everything further away rests until the view comes near. Depths are
    /// measured down from the top of the river in the map's own points. Pure: numbers in, numbers out.
    /// </summary>
    public static class StretchWindowRule
    {
        /// <summary>Stretches kept live on either side of those on screen.</summary>
        public const int Margin = 1;

        /// <summary>Below this many stretches every one stays live: there is nothing worth resting.</summary>
        public const int MinStretches = 4;

        /// <summary>
        /// The first and last stretch to keep live for a view showing depths <paramref name="top"/> to
        /// <paramref name="bottom"/> (either order) on a river of <paramref name="stretches"/>, each
        /// <paramref name="stretchHeight"/> tall. Index <paramref name="stretches"/> is the misty land below
        /// the last open stretch. A river too short to window, or a bad height, keeps everything.
        /// </summary>
        public static void Range(float top, float bottom, float stretchHeight, int stretches, out int first, out int last)
        {
            first = 0;
            last = Math.Max(0, stretches);
            if (stretches < MinStretches || !(stretchHeight > 0f) || float.IsNaN(top) || float.IsNaN(bottom)) return;
            float high = Math.Min(top, bottom), low = Math.Max(top, bottom);
            first = Clamp(Index(high, stretchHeight) - Margin, stretches);
            last = Clamp(Index(low, stretchHeight) + Margin, stretches);
        }

        /// <summary>Which stretch a depth lies in: 0 at the top, <paramref name="stretches"/> past the last.</summary>
        public static int StretchAt(float depth, float stretchHeight, int stretches)
        {
            if (!(stretchHeight > 0f) || float.IsNaN(depth)) return 0;
            return Clamp(Index(depth, stretchHeight), stretches);
        }

        /// <summary>True when something at <paramref name="depth"/> should be live for this range.</summary>
        public static bool Keep(float depth, float stretchHeight, int stretches, int first, int last)
        {
            int at = StretchAt(depth, stretchHeight, stretches);
            return at >= first && at <= last;
        }

        private static int Index(float depth, float stretchHeight)
        {
            double at = Math.Floor(depth / stretchHeight);
            return at < int.MinValue ? int.MinValue : at > int.MaxValue ? int.MaxValue : (int)at;
        }

        private static int Clamp(int index, int stretches)
        {
            return index < 0 ? 0 : index > stretches ? stretches : index;
        }
    }
}
