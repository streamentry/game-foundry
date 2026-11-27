using Evergrove.Core;
using UnityEngine;

namespace Evergrove.UnityClient
{
    /// <summary>
    /// Checklist 24: walks a neighbour's map holder from one spot to another with a short step
    /// animation (NeighbourWalk). It moves only anchoredPosition and localRotation, on unscaled
    /// time, ends exactly at To with no tilt, then disables itself. Added only with reduced motion off.
    /// </summary>
    internal sealed class NeighbourWalker : MonoBehaviour
    {
        public Vector2 From;
        public Vector2 To;
        /// <summary>When the walk began (unscaled time). A holder re-placed mid-walk carries on from here.</summary>
        public float StartedAt;

        /// <summary>Where a walk from <paramref name="from"/> to <paramref name="to"/> begun at
        /// <paramref name="startedAt"/> has reached now: the pose a re-placed holder starts in, so a
        /// relayout mid-walk never jumps the neighbour back to the start.</summary>
        public static Vector2 PoseAt(Vector2 from, Vector2 to, float startedAt)
        {
            float distance = Vector2.Distance(from, to);
            float duration = NeighbourWalk.Duration(distance);
            float clock = Time.unscaledTime - startedAt;
            if (clock >= duration) return to;
            return Vector2.Lerp(from, to, NeighbourWalk.Progress(clock, duration))
                + new Vector2(0f, NeighbourWalk.Bob(clock, duration, NeighbourWalk.Steps(distance)));
        }

        private void Update()
        {
            var rect = (RectTransform)transform;
            float distance = Vector2.Distance(From, To);
            float duration = NeighbourWalk.Duration(distance);
            int steps = NeighbourWalk.Steps(distance);
            float clock = Time.unscaledTime - StartedAt;
            if (clock >= duration)
            {
                rect.anchoredPosition = To;
                rect.localRotation = Quaternion.identity;
                enabled = false;
                return;
            }
            float eased = NeighbourWalk.Progress(clock, duration);
            Vector2 along = Vector2.Lerp(From, To, eased);
            rect.anchoredPosition = along + new Vector2(0f, NeighbourWalk.Bob(clock, duration, steps));
            rect.localRotation = Quaternion.Euler(0f, 0f, NeighbourWalk.Tilt(clock, duration, steps));
        }
    }
}
