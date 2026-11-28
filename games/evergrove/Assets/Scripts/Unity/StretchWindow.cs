using Evergrove.Core;
using UnityEngine;

namespace Evergrove.UnityClient
{
    /// <summary>
    /// Checklist 50: keeps only the river stretches on screen, plus one either side, live
    /// (StretchWindowRule). Everything on the land (each stretch's painting, things, visitors, signs,
    /// neighbours, shadows) further away is set inactive, and comes back as the view nears it. Large
    /// layers that span the whole river (the painting's frame, washes, the misty land below) stay live.
    /// Work is done only when the range on screen or the land's children change, never every frame.
    /// </summary>
    internal sealed class StretchWindow : MonoBehaviour
    {
        /// <summary>On in play. The dev tour turns it off once, to measure the river without it.</summary>
        internal static bool Enabled = true;

        private RectTransform viewport, land, painting;
        private int stretches;
        private int lastFirst = -1, lastLast = -1, lastChildren = -1, lastPaintings = -1;
        private bool lastEnabled = true;
        private readonly Vector3[] corners = new Vector3[4];

        /// <summary>Adds the window to a map view; a river too short to window gets none.</summary>
        public static void Attach(RectTransform viewport, RectTransform land, RectTransform painting, int stretches)
        {
            if (viewport == null || land == null || painting == null || stretches < StretchWindowRule.MinStretches) return;
            var window = viewport.gameObject.AddComponent<StretchWindow>();
            window.viewport = viewport;
            window.land = land;
            window.painting = painting;
            window.stretches = stretches;
        }

        /// <summary>How many things on the land are live now (the tour logs it).</summary>
        internal int LiveCount()
        {
            int live = 0;
            if (land != null) foreach (Transform child in land) if (child.gameObject.activeSelf) live++;
            if (painting != null) foreach (Transform child in painting) if (child.gameObject.activeSelf) live++;
            return live;
        }

        private void LateUpdate()
        {
            if (land == null || painting == null || viewport == null) return;
            float stretchHeight = painting.rect.height / stretches;
            if (!(stretchHeight > 1f)) return;
            viewport.GetWorldCorners(corners);
            float top = Depth(corners[1]), bottom = Depth(corners[0]);
            int first = 0, last = stretches;
            if (Enabled) StretchWindowRule.Range(top, bottom, stretchHeight, stretches, out first, out last);
            if (first == lastFirst && last == lastLast && land.childCount == lastChildren &&
                painting.childCount == lastPaintings && Enabled == lastEnabled) return;
            lastFirst = first; lastLast = last; lastEnabled = Enabled;
            lastChildren = land.childCount; lastPaintings = painting.childCount;
            Apply(land, stretchHeight, first, last);
            Apply(painting, stretchHeight, first, last);
        }

        private void Apply(RectTransform parent, float stretchHeight, int first, int last)
        {
            foreach (Transform child in parent)
            {
                var rect = child as RectTransform;
                if (rect == null || rect == painting) continue;
                // Layers taller than two stretches belong to the whole river: they stay.
                if (rect.rect.height > stretchHeight * 2f) { if (!child.gameObject.activeSelf) child.gameObject.SetActive(true); continue; }
                bool keep = StretchWindowRule.Keep(Depth(rect.TransformPoint(rect.rect.center)), stretchHeight, stretches, first, last);
                if (child.gameObject.activeSelf != keep) child.gameObject.SetActive(keep);
            }
        }

        // Depth below the top of the river, in the land's own points.
        private float Depth(Vector3 world)
        {
            Vector3 local = land.InverseTransformPoint(world);
            return land.rect.yMax - local.y;
        }
    }
}
