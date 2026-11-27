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

        private float clock;
        private float duration;
        private int steps;
        private bool started;

        private void Update()
        {
            var rect = (RectTransform)transform;
            if (!started)
            {
                float distance = Vector2.Distance(From, To);
                duration = NeighbourWalk.Duration(distance);
                steps = NeighbourWalk.Steps(distance);
                started = true;
            }
            clock += Time.unscaledDeltaTime;
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
