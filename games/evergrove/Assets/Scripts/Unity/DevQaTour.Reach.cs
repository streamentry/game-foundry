#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System.Collections;
using System.Collections.Generic;
using Evergrove.Core;
using UnityEngine;
using UnityEngine.UI;

namespace Evergrove.UnityClient
{
    /// <summary>
    /// Checklist 94, one-hand tour audit: every active, interactable control on the hub (the same set the
    /// rest count uses: no garden content) and every fan token is placed in canvas points, origin top-left,
    /// and logged as MOSSELORA_REACH reachable or out, by ThumbReach (the heuristic in ThumbReach.cs).
    /// Shots: 29-reach-hub.png and 29-reach-fan.png. Part of the dev QA tour (DevQaTour.cs); the same guard
    /// applies. Run it in portrait and in landscape, and at 150% text, to audit each.
    /// </summary>
    public sealed partial class DevQaTour
    {
        private IEnumerator ReachShots()
        {
            app.QaShowHub(true);
            for (int f = 0; f < 10; f++) yield return null;
            yield return new WaitForSecondsRealtime(0.4f);
            yield return new WaitForEndOfFrame();
            int missed = LogReach("hub", HubReachControls());
            SavePng("29-reach-hub.png");
            Press("Open menu");
            for (int f = 0; f < 12; f++) yield return null;
            yield return new WaitForSecondsRealtime(0.4f);
            yield return new WaitForEndOfFrame();
            missed += LogReach("fan", FanReachControls());
            SavePng("29-reach-fan.png");
            Press("Open menu"); // folds the fan away
            for (int f = 0; f < 4; f++) yield return null;
            Debug.Log("MOSSELORA_REACH summary out " + missed);
        }

        // The hub's controls, leaving out garden content (the same rule as the rest count, DevQaTour.Rest.cs).
        private static List<RectTransform> HubReachControls()
        {
            var controls = new List<RectTransform>();
            foreach (Selectable control in FindObjectsByType<Selectable>(FindObjectsInactive.Exclude, FindObjectsSortMode.None))
                if (control.isActiveAndEnabled && control.interactable && IsRestControl(control.transform))
                    controls.Add((RectTransform)control.transform);
            return controls;
        }

        // The fan's tokens: the active, interactable children of the open fan column.
        private static List<RectTransform> FanReachControls()
        {
            var tokens = new List<RectTransform>();
            foreach (RectTransform column in FindObjectsByType<RectTransform>(FindObjectsInactive.Exclude, FindObjectsSortMode.None))
            {
                if (column.name != "Fan column") continue;
                for (int i = 0; i < column.childCount; i++)
                {
                    var token = column.GetChild(i) as RectTransform;
                    var control = token != null ? token.GetComponent<Selectable>() : null;
                    if (control != null && control.isActiveAndEnabled && control.interactable) tokens.Add(token);
                }
            }
            return tokens;
        }

        // Logs each control; returns how many are out of one-hand reach.
        private static int LogReach(string scope, List<RectTransform> controls)
        {
            int missed = 0;
            foreach (RectTransform control in controls)
            {
                bool reachable = InThumbReach(control);
                if (!reachable) missed++;
                Debug.Log("MOSSELORA_REACH " + scope + " " + control.name + " " + (reachable ? "reachable" : "out"));
            }
            return missed;
        }

        // The control's centre in canvas points (origin top-left, y down) against the root canvas size.
        private static bool InThumbReach(RectTransform control)
        {
            Canvas canvas = control.GetComponentInParent<Canvas>();
            if (canvas == null) return false;
            var root = canvas.rootCanvas.transform as RectTransform;
            if (root == null) return false;
            Vector3 local = root.InverseTransformPoint(Centre(control));
            float x = local.x - root.rect.xMin;
            float y = root.rect.yMax - local.y;
            return ThumbReach.Reachable(x, y, root.rect.width, root.rect.height);
        }
    }
}
#endif
