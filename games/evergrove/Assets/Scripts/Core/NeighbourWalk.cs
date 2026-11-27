using System;

namespace Evergrove.Core
{
    /// <summary>
    /// Checklist 24: a neighbour who moves to a new spot on the hub walks there in a short, gentle
    /// walk instead of appearing. Pure and deterministic: no randomness, no clock. Time is in seconds
    /// from the start of the walk, distances are in points. The Unity side (NeighbourWalker) only
    /// samples these values each frame.
    /// </summary>
    public static class NeighbourWalk
    {
        /// <summary>A move shorter than this (points) is not a walk: the neighbour stays where they are.</summary>
        public const float MinDistance = 6f;

        /// <summary>The walking pace, in points per second.</summary>
        public const float Speed = 140f;

        /// <summary>The shortest walk (seconds), so even a near move reads as a walk.</summary>
        public const float MinSeconds = 0.6f;

        /// <summary>The longest walk (seconds), so a long move across the valley stays gentle.</summary>
        public const float MaxSeconds = 1.6f;

        /// <summary>The length of one step, in points. A walk has at least two steps.</summary>
        public const float StepLength = 34f;

        /// <summary>The most steps a walk takes, however far it goes.</summary>
        public const int MaxSteps = 8;

        /// <summary>The height of one step's lift, in points (peak of a step, not the whole walk).</summary>
        public const float BobPoints = 5f;

        /// <summary>The rock of one step, in degrees (alternating left and right).</summary>
        public const float TiltDegrees = 3f;

        /// <summary>True when a move of this many points is long enough to walk.</summary>
        public static bool Moves(float distance)
        {
            return distance >= MinDistance;
        }

        /// <summary>Walk time for a move of this distance: distance at the gentle pace, clamped to 0.6 to 1.6 s.</summary>
        public static float Duration(float distance)
        {
            if (!(distance > 0f)) return MinSeconds; // also catches NaN
            return Clamp(distance / Speed, MinSeconds, MaxSeconds);
        }

        /// <summary>The number of steps for a move of this distance: at least two, at most MaxSteps.</summary>
        public static int Steps(float distance)
        {
            if (!(distance > 0f)) return 2;
            int steps = (int)Math.Ceiling(distance / StepLength);
            if (steps < 2) return 2;
            return steps > MaxSteps ? MaxSteps : steps;
        }

        /// <summary>
        /// How far along the route the neighbour is, 0 to 1, eased (smoothstep) so the walk starts and
        /// ends softly. 0 at the start, exactly 1 from the duration on, and never falls back.
        /// </summary>
        public static float Progress(float t, float duration)
        {
            if (!(duration > 0f)) return 1f;
            float x = Clamp(t / duration, 0f, 1f);
            return x * x * (3f - 2f * x);
        }

        /// <summary>
        /// The small lift of the current step, in points: 0 at the start and the end, and at most
        /// BobPoints (5) at the middle of each step.
        /// </summary>
        public static float Bob(float t, float duration, int steps)
        {
            return BobPoints * StepSine(t, duration, steps);
        }

        /// <summary>
        /// The rock of the current step, in degrees: alternates +3 and -3 from one step to the next,
        /// and is 0 at the start and the end.
        /// </summary>
        public static float Tilt(float t, float duration, int steps)
        {
            if (steps < 1) return 0f;
            int index = StepIndex(t, duration, steps);
            float side = index % 2 == 0 ? 1f : -1f;
            return side * TiltDegrees * StepSine(t, duration, steps);
        }

        // 0 at each step's start and end, 1 at its middle; the walk's steps share its duration evenly.
        private static float StepSine(float t, float duration, int steps)
        {
            if (steps < 1 || !(duration > 0f) || t <= 0f || t >= duration) return 0f; // exactly 0 at both ends
            float phase = Clamp(t / duration, 0f, 1f) * steps;
            float within = phase - StepIndex(t, duration, steps);
            return (float)Math.Sin(within * Math.PI);
        }

        private static int StepIndex(float t, float duration, int steps)
        {
            if (steps < 1 || !(duration > 0f)) return 0;
            float phase = Clamp(t / duration, 0f, 1f) * steps;
            int index = (int)Math.Floor(phase);
            return index > steps - 1 ? steps - 1 : index;
        }

        private static float Clamp(float value, float low, float high)
        {
            if (value < low) return low;
            if (value > high) return high;
            return value;
        }
    }
}
