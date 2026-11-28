#if UNITY_EDITOR || DEVELOPMENT_BUILD
using UnityEngine;

namespace Evergrove.UnityClient
{
    /// <summary>Checklist 50 tour hooks: a long river, the hub's scroll, and how much of it is live.</summary>
    public sealed partial class EvergroveApp
    {
        // A river of this many stretches for the hub (0 puts the garden's own back). Rebuild the hub after.
        internal void QaLongRiver(int stretches) { qaLongRiver = stretches > 0 ? stretches : 0; gardenMap = null; }

        // Scrolls the hub's map as a swipe does (the scroll position only; KeepAt would lay the whole map
        // out again): 1 the top of the river, 0 the bottom.
        internal void QaScrollHub(float y)
        {
            var scroll = hubMapView != null ? hubMapView.GetComponent<UnityEngine.UI.ScrollRect>() : null;
            if (scroll == null) return;
            scroll.StopMovement();
            scroll.normalizedPosition = new Vector2(scroll.normalizedPosition.x, Mathf.Clamp01(y));
        }

        // How many things on the hub's land are live (-1 without a window: a short river).
        internal int QaHubLive()
        {
            if (hubMapView == null) return -1;
            var window = hubMapView.GetComponent<StretchWindow>();
            return window != null ? window.LiveCount() : -1;
        }

        internal int QaHubStretches() { return gardenMap != null ? gardenMap.Reaches : 0; }
    }
}
#endif
