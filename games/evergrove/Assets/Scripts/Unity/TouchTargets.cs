using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Evergrove.UnityClient
{
    /// <summary>
    /// Checklist 91: every interactive element must offer a touch area of at least
    /// 48 points in both dimensions. The area is the graphic's rect, shrunk by its
    /// raycastPadding, measured in the root canvas's units (points). The DevQa tour
    /// calls <see cref="Undersized"/> on a screen to list anything below the minimum.
    /// </summary>
    public static class TouchTargets
    {
        public const float MinPt = 48f;

        /// <summary>
        /// Interactive elements under <paramref name="root"/> (active only) whose touch
        /// area is below <paramref name="minPt"/> in either dimension. An element counts
        /// when it is a Selectable (interactable) or has an IPointerClickHandler, and its
        /// hit graphic has raycastTarget on.
        /// </summary>
        public static List<RectTransform> Undersized(RectTransform root, float minPt = MinPt)
        {
            var found = new List<RectTransform>();
            if (root == null) return found;
            var seen = new HashSet<RectTransform>();
            foreach (Selectable selectable in root.GetComponentsInChildren<Selectable>(false))
            {
                if (!selectable.IsInteractable()) continue;
                Consider(selectable.GetComponent<Graphic>(), root, minPt, seen, found);
            }
            foreach (IPointerClickHandler handler in root.GetComponentsInChildren<IPointerClickHandler>(false))
            {
                Component component = (Component)handler;
                if (component.GetComponent<Selectable>() != null) continue; // already measured
                Consider(component.GetComponent<Graphic>(), root, minPt, seen, found);
            }
            return found;
        }

        /// <summary>Touch area of <paramref name="hit"/> in <paramref name="root"/> units (points).</summary>
        public static Vector2 TouchSize(Graphic hit, RectTransform root)
        {
            RectTransform rect = hit.rectTransform;
            Vector3[] corners = new Vector3[4];
            rect.GetWorldCorners(corners);
            float minX = float.MaxValue, minY = float.MaxValue, maxX = float.MinValue, maxY = float.MinValue;
            for (int i = 0; i < 4; i++)
            {
                Vector3 local = root.InverseTransformPoint(corners[i]);
                minX = Mathf.Min(minX, local.x); maxX = Mathf.Max(maxX, local.x);
                minY = Mathf.Min(minY, local.y); maxY = Mathf.Max(maxY, local.y);
            }
            float width = maxX - minX, height = maxY - minY;
            // The padding is in the graphic's own units; scale it into root points.
            float ownWidth = rect.rect.width, ownHeight = rect.rect.height;
            Vector4 pad = hit.raycastPadding;
            float sx = ownWidth > 0f ? width / ownWidth : 1f;
            float sy = ownHeight > 0f ? height / ownHeight : 1f;
            return new Vector2(width - (pad.x + pad.z) * sx, height - (pad.y + pad.w) * sy);
        }

        private static void Consider(Graphic hit, RectTransform root, float minPt,
            HashSet<RectTransform> seen, List<RectTransform> found)
        {
            if (hit == null || !hit.raycastTarget) return;
            if (!seen.Add(hit.rectTransform)) return;
            Vector2 size = TouchSize(hit, root);
            if (size.x < minPt || size.y < minPt) found.Add(hit.rectTransform);
        }
    }
}
