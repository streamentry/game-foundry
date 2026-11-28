#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Evergrove.UnityClient
{
    /// <summary>
    /// Checklist 11, tour shots: the hub at rest counts its interactive controls, leaving out garden
    /// content (the map and neighbours in the village window and strip, the day's dew and lantern
    /// tokens). The target is at most two (the menu button and the play token). Then the corner fan
    /// opens and is shot. Part of the dev QA tour (DevQaTour.cs); the same guard applies.
    /// </summary>
    public sealed partial class DevQaTour
    {
        private const string HubRootName = "Title screen";

        // Garden content on the hub, by the names the hub gives it: not "controls" for the rest count.
        private static readonly string[] GardenContentNames = { "Village window", "Your village", "Day token", "Dew", "Lantern" };

        private IEnumerator RestShots()
        {
            app.QaShowHub(true);
            for (int f = 0; f < 10; f++) yield return null;
            yield return new WaitForSecondsRealtime(0.5f);
            yield return new WaitForEndOfFrame();
            LogRestControls();
            SavePng("26-hub-at-rest.png");
            Press("Open menu");
            for (int f = 0; f < 12; f++) yield return null;
            yield return new WaitForSecondsRealtime(0.4f);
            yield return new WaitForEndOfFrame();
            SavePng("26-fan-folded.png");
            LogFanFit(); // every fan token on screen (landscape wraps into columns)
            Press("Open menu"); // folds the fan away
            for (int f = 0; f < 4; f++) yield return null;
        }

        // Logs every active, interactable Selectable under the hub that is not garden content.
        private static void LogRestControls()
        {
            var names = new List<string>();
            foreach (Selectable control in UnityEngine.Object.FindObjectsByType<Selectable>(FindObjectsInactive.Exclude, FindObjectsSortMode.None))
                if (control.isActiveAndEnabled && control.interactable && IsRestControl(control.transform))
                    names.Add(control.name);
            Debug.Log("MOSSELORA_REST controls " + names.Count + " : " + string.Join(", ", names));
        }

        // True when the control sits under the hub root and no garden-content ancestor below that root.
        private static bool IsRestControl(Transform control)
        {
            bool underHub = false;
            for (Transform t = control; t != null; t = t.parent)
            {
                if (t.name == HubRootName) { underHub = true; break; }
                foreach (string garden in GardenContentNames)
                    if (t.name.StartsWith(garden, System.StringComparison.Ordinal)) return false;
            }
            return underHub;
        }

        // Counts fan tokens whose rect leaves the screen: 0 means every one can be reached.
        private static void LogFanFit()
        {
            int tokens = 0, off = 0;
            var corners = new Vector3[4];
            foreach (RectTransform rect in FindObjectsByType<RectTransform>(FindObjectsInactive.Exclude, FindObjectsSortMode.None))
            {
                if (rect.parent == null || rect.parent.name != "Fan column") continue;
                tokens++;
                rect.GetWorldCorners(corners);
                if (corners[0].y < 0f || corners[1].y > Screen.height || corners[0].x < 0f || corners[2].x > Screen.width) off++;
            }
            Debug.Log("MOSSELORA_FAN_FIT tokens " + tokens + " offscreen " + off);
        }
    }
}
#endif
