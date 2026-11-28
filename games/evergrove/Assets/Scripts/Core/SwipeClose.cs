using System;

namespace Evergrove.Core
{
    /// <summary>
    /// Checklist 94 (founder decision 2026-11-28): any sheet or card also closes with a downward swipe
    /// anywhere on it, as iOS sheets do. The one-thumb route to close, since iOS has no back gesture.
    /// Pure rule; the Unity client's SwipeToClose feeds it drags measured in canvas points.
    /// Convention: dx is positive to the right; dy is positive DOWNWARD on screen (the finger moved down
    /// the screen). Screen y grows upward, so the Unity side passes dy = pressY - releaseY.
    /// </summary>
    public static class SwipeClose
    {
        /// <summary>How far the finger must travel downward to close, in canvas points.</summary>
        public const float Threshold = 90f;

        /// <summary>The swipe must be mostly downward: |dy| must exceed this many times |dx|.</summary>
        public const float DominanceRatio = 1.5f;

        /// <summary>The most the panel may follow the finger down while it is held, in canvas points.</summary>
        public const float MaxFollow = 120f;

        /// <summary>True when a drag closes: downward by at least the threshold and mostly vertical.</summary>
        public static bool Closes(float dx, float dy, float threshold = Threshold)
        {
            if (float.IsNaN(dx) || float.IsNaN(dy) || float.IsInfinity(dx) || float.IsInfinity(dy)) return false;
            if (!(dy > 0f) || dy < threshold) return false; // upward, or too short
            return Math.Abs(dy) > DominanceRatio * Math.Abs(dx);
        }

        /// <summary>
        /// How far the panel follows a downward drag while held: half the finger's travel, never more
        /// than <see cref="MaxFollow"/>. Upward or still drags follow nothing.
        /// </summary>
        public static float FollowOffset(float dy)
        {
            if (float.IsNaN(dy) || !(dy > 0f)) return 0f;
            return Math.Min(dy * 0.5f, MaxFollow);
        }
    }
}
