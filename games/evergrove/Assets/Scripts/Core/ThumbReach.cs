using System;

namespace Evergrove.Core
{
    /// <summary>
    /// Checklist 94, one-hand reach: can a thumb reach a target's centre without shifting the grip?
    /// A heuristic, not a measurement. It simplifies the thumb-zone studies by Steven Hoober (from
    /// 2013 on: most phone use is one-handed, and the easy zone sits low on the screen, nearer the
    /// grip). The bands below are rounded for this app and are checked by the dev reach tour
    /// in the Unity client, not by a user study.
    /// Portrait: the lower two thirds of the screen (the centre is at or below one third of the height).
    /// Landscape: the lower three quarters, and within 45% of the width from either side (a thumb at
    /// each lower corner). The model is symmetric, so <paramref name="rightHanded"/> does not change the
    /// answer; it records which hand holds the phone for the tour's notes.
    /// Coordinates are points, origin top-left, y down. Pure: no engine types.
    /// </summary>
    public static class ThumbReach
    {
        /// <summary>Share of the height that is out of easy reach in portrait (the top third).</summary>
        public const float PortraitTopShare = 1f / 3f;

        /// <summary>Share of the height that is out of easy reach in landscape (the top quarter).</summary>
        public const float LandscapeTopShare = 1f / 4f;

        /// <summary>Share of the width reached from each side in landscape.</summary>
        public const float LandscapeSideShare = 0.45f;

        /// <summary>
        /// True when a target centred at (x, y) on a width x height screen is reachable one-handed.
        /// A centre off the screen, or a screen with no usable size, is never reachable.
        /// </summary>
        public static bool Reachable(float x, float y, float width, float height, bool rightHanded = true)
        {
            if (!(width > 0f) || !(height > 0f) || float.IsInfinity(width) || float.IsInfinity(height)) return false;
            if (!(x >= 0f && x <= width) || !(y >= 0f && y <= height)) return false;
            if (height > width) return y >= height * PortraitTopShare;
            if (y < height * LandscapeTopShare) return false;
            return x <= width * LandscapeSideShare || x >= width * (1f - LandscapeSideShare);
        }
    }
}
