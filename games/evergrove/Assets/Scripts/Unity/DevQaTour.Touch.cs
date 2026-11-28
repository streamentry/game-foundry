#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Evergrove.UnityClient
{
    /// <summary>
    /// Checklist 91, the automated scan: at every tour shot, each root canvas is measured with
    /// <see cref="TouchTargets.Undersized"/> and anything below 48 pt (canvas units, the project's
    /// design points) is written to <c>touch-scan.tsv</c> beside the shots, one line per element per
    /// shot. The tour's last line reports the total (<c>MOSSELORA_TOUCH_SCAN</c>). Part of the dev QA
    /// tour (DevQaTour.cs); the same guard applies.
    /// </summary>
    public sealed partial class DevQaTour
    {
        private const float ScanTolerance = 0.05f;
        private int touchSmall, touchCovered;
        private readonly HashSet<string> touchSmallKinds = new HashSet<string>();

        private void ScanTouchTargets(string shot)
        {
            var lines = new StringBuilder();
            foreach (Canvas canvas in FindObjectsByType<Canvas>(FindObjectsInactive.Exclude, FindObjectsSortMode.None))
            {
                if (!canvas.isRootCanvas || !canvas.enabled) continue;
                var root = (RectTransform)canvas.transform;
                // A hair of tolerance: layout floats put an exact 48 at 47.99, which is not a finding.
                foreach (RectTransform small in TouchTargets.Undersized(root, TouchTargets.MinPt - ScanTolerance))
                {
                    // Only what a finger can reach counts: a target under something else (a full-screen
                    // "tap to come back" layer, a zoomable board's picture) takes no tap of its own.
                    if (!Reachable(small)) { touchCovered++; continue; }
                    var hit = small.GetComponent<UnityEngine.UI.Graphic>();
                    Vector2 size = TouchTargets.TouchSize(hit, root);
                    string name = TouchName(small);
                    lines.Append(shot).Append('\t').Append(name).Append('\t')
                        .Append(size.x.ToString("0", CultureInfo.InvariantCulture)).Append('x')
                        .Append(size.y.ToString("0", CultureInfo.InvariantCulture)).Append('\n');
                    touchSmall++;
                    touchSmallKinds.Add(name);
                }
            }
            if (lines.Length > 0) File.AppendAllText(Path.Combine(outputDir, "touch-scan.tsv"), lines.ToString());
        }

        // The element and its parent, so a row reads "Plot 12 < Garden land" rather than a bare "Image".
        private static string TouchName(RectTransform rect)
        {
            string own = rect.name;
            Transform parent = rect.parent;
            return parent != null ? own + " < " + parent.name : own;
        }

        // A raycast at the target's centre lands on the target itself (or something inside it).
        private static bool Reachable(RectTransform target)
        {
            EventSystem events = EventSystem.current;
            if (events == null) return true;
            var corners = new Vector3[4];
            target.GetWorldCorners(corners);
            Vector3 centre = (corners[0] + corners[2]) * 0.5f;
            Canvas canvas = target.GetComponentInParent<Canvas>();
            Camera eye = canvas != null && canvas.renderMode != RenderMode.ScreenSpaceOverlay ? canvas.worldCamera : null;
            var probe = new PointerEventData(events) { position = RectTransformUtility.WorldToScreenPoint(eye, centre) };
            var hits = new List<RaycastResult>();
            events.RaycastAll(probe, hits);
            if (hits.Count == 0) return false;
            Transform first = hits[0].gameObject.transform;
            return first == target || first.IsChildOf(target);
        }

        private void ReportTouchScan()
        {
            Debug.Log("MOSSELORA_TOUCH_SCAN small=" + touchSmall + " kinds=" + touchSmallKinds.Count + " covered=" + touchCovered);
        }
    }
}
#endif
