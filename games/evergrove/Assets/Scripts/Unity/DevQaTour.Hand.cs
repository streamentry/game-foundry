#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using Evergrove.Core;
using UnityEngine;

namespace Evergrove.UnityClient
{
    /// <summary>
    /// Checklist 5, the hand tour: for each hand the hub and the open fan are shot (31-hand-left-hub.png and so on)
    /// and every active, interactable control is logged as MOSSELORA_HANDMAP &lt;hand&gt; &lt;name&gt; &lt;x&gt; &lt;y&gt; &lt;w&gt; &lt;h&gt;:
    /// the centre and the screen size, in canvas points (origin top-left, y down). Names have spaces replaced
    /// by underscores and fan tokens are prefixed "fan:". tools/evergrove_hand_heatmap.py draws the heatmap.
    /// The hand and its asked flag are restored afterwards, so later tour steps see the same state.
    /// Part of the dev QA tour (DevQaTour.cs); the same guard applies.
    /// </summary>
    public sealed partial class DevQaTour
    {
        private IEnumerator HandShots()
        {
            bool asked = app.QaHandAsked;
            foreach (Hand holding in new[] { Hand.Left, Hand.Right })
            {
                string side = holding == Hand.Left ? "left" : "right";
                app.QaSetHand(holding, true); // asked is set, so the question never opens during the tour
                app.QaShowHub(false);
                app.QaShowHub(true);
                for (int f = 0; f < 10; f++) yield return null;
                yield return new WaitForSecondsRealtime(0.4f);
                yield return new WaitForEndOfFrame();
                LogHand(side, "", HubReachControls());
                SavePng("31-hand-" + side + "-hub.png");
                Press("Open menu");
                for (int f = 0; f < 12; f++) yield return null;
                yield return new WaitForSecondsRealtime(0.4f);
                yield return new WaitForEndOfFrame();
                LogHand(side, "fan:", FanReachControls());
                SavePng("31-hand-" + side + "-fan.png");
                Press("Open menu"); // folds the fan away
                for (int f = 0; f < 4; f++) yield return null;
                app.QaShowHub(false);
            }
            app.QaSetHand(Hand.Unknown, asked);
            app.QaShowHub(false);
            app.QaShowHub(true); // the hub is left shown, as the reach tour leaves it
        }

        // One line per control: MOSSELORA_HANDMAP <hand> <name> <x> <y> <w> <h>, canvas points, origin top-left.
        private static void LogHand(string side, string prefix, List<RectTransform> controls)
        {
            foreach (RectTransform control in controls)
            {
                Canvas canvas = control.GetComponentInParent<Canvas>();
                var root = canvas != null && canvas.rootCanvas != null ? canvas.rootCanvas.transform as RectTransform : null;
                if (root == null) continue;
                Vector3 local = root.InverseTransformPoint(Centre(control));
                float x = local.x - root.rect.xMin;
                float y = root.rect.yMax - local.y;
                string name = (prefix + control.name).Replace(' ', '_');
                Debug.Log("MOSSELORA_HANDMAP " + side + " " + name + " " +
                    x.ToString("0.0", CultureInfo.InvariantCulture) + " " +
                    y.ToString("0.0", CultureInfo.InvariantCulture) + " " +
                    root.rect.width.ToString("0.0", CultureInfo.InvariantCulture) + " " +
                    root.rect.height.ToString("0.0", CultureInfo.InvariantCulture));
            }
        }
    }
}
#endif
