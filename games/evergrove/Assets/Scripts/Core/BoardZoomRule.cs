using System;

namespace Evergrove.Core
{
    /// <summary>
    /// Checklist 91 (founder, 2026-11-28): a board whose cells are smaller than the 48 pt touch minimum
    /// opens whole, as a picture; one tap zooms in on that part so every cell is at least 48 pt, and the
    /// zoomed board pans by dragging. Pure numbers: the scale, and where the zoomed board sits.
    /// </summary>
    public static class BoardZoomRule
    {
        /// <summary>The touch minimum, in points.</summary>
        public const float MinTouch = 48f;
        /// <summary>A little over the minimum, so rounding never lands a cell just under it.</summary>
        public const float Headroom = 1.08f;
        /// <summary>The most a board is ever magnified.</summary>
        public const float MaxScale = 4f;

        /// <summary>True when the board's smallest cell is under the touch minimum (and is a real size).</summary>
        public static bool Needed(float smallestCell)
        {
            return smallestCell > 0f && !float.IsNaN(smallestCell) && smallestCell < MinTouch;
        }

        /// <summary>The magnification that brings the smallest cell to at least the minimum (1 when none is needed).</summary>
        public static float Scale(float smallestCell)
        {
            if (!Needed(smallestCell)) return 1f;
            return Math.Min(MaxScale, Math.Max(1f, MinTouch * Headroom / smallestCell));
        }

        /// <summary>
        /// Where the zoomed board's centre sits, along one axis, so the tapped point (<paramref name="tapped"/>,
        /// measured from the board's centre in unscaled points) comes to the middle of the view, without ever
        /// showing past the board's edge. <paramref name="size"/> is the view (and unscaled board) length.
        /// </summary>
        public static float Offset(float tapped, float size, float scale)
        {
            if (!(size > 0f) || !(scale >= 1f) || float.IsNaN(tapped)) return 0f;
            float limit = size * (scale - 1f) / 2f;
            float wanted = -tapped * scale;
            return wanted < -limit ? -limit : wanted > limit ? limit : wanted;
        }
    }
}
