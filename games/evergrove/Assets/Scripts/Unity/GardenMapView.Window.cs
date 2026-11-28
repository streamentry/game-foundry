using System;
using UnityEngine;

namespace Evergrove.UnityClient
{
    /// <summary>
    /// Checklist 50: the window of river stretches a map view builds. Only the stretches on screen, plus
    /// one either side (StretchWindowRule), have their things, visitors and signs built; the rest are not
    /// built until the view comes near. StretchWindow sets the window as the view scrolls. Index
    /// <c>reaches</c> is the misty land below the last stretch (it has nothing built in it).
    /// </summary>
    internal sealed partial class GardenMapView
    {
        // All stretches until a window is set (short rivers, or the window switched off).
        private int windowFirst, windowLast = int.MaxValue;
        private bool windowing; // a window change is relaying out: the view keeps its place and any fling

        /// <summary>
        /// Builds only the stretches <paramref name="first"/> to <paramref name="last"/> (inclusive). Relays
        /// out only when the range changes, and holds the view where it is: a relayout must not jump to
        /// the newest thing or to the top of the river.
        /// </summary>
        internal void SetWindow(int first, int last)
        {
            if (first == windowFirst && last == windowLast) return;
            windowFirst = first; windowLast = last;
            Vector2 at = keep;
            keep = new Vector2(Math.Max(0f, Position.x), Position.y);
            windowing = true;
            try { Layout(); }
            finally { keep = at; windowing = false; }
        }

        /// <summary>
        /// Sets the window before the view's first layout, without laying out: the first build then makes
        /// only the top of the river, never every stretch. The window corrects itself in the same frame
        /// (StretchWindow runs in LateUpdate, before anything is drawn) if the view opens further down.
        /// </summary>
        internal void PresetWindow(int first, int last)
        {
            windowFirst = first;
            windowLast = last;
        }

        private bool NearReach(int reach) { return reach >= windowFirst && reach <= windowLast; }

        // The stretch a cell lies in (cells run row by row; a stretch is ReachRows rows).
        private bool NearCell(int cell)
        {
            int reach = map.ReachRows > 0 ? cell / map.Columns / map.ReachRows : 0;
            return NearReach(reach);
        }

        private int FirstReach() { return Math.Max(0, windowFirst); }

        private int EndReach(int reaches) { return windowLast >= reaches ? reaches : windowLast + 1; }
    }
}
