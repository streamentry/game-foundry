using System.Collections.Generic;
using Evergrove.Core;
using UnityEngine;
using UnityEngine.UI;

namespace Evergrove.UnityClient
{
    /// <summary>
    /// Vision run 20261128-1601, ideas 4 and 5. Kept out of GardenMapView.cs, which holds a fixed line count.
    /// Idea 4: petals and leaves fall only from the deciduous trees on the map (SeasonLook.Falls).
    /// Idea 5: each thing on the map carries its own words for a screen reader, in a hidden child named
    /// "Caption" (ScreenReaderBridge.CaptionOf speaks it instead of the node's name, "On the map").
    /// </summary>
    internal sealed partial class GardenMapView
    {
        /// <summary>How many things on the map shed petals in spring and leaves in autumn.</summary>
        private static int FallingTrees(IList<Placed> placed)
        {
            int falling = 0;
            if (placed == null) return 0;
            foreach (Placed thing in placed)
                if (SeasonLook.Falls(thing.Family, thing.FlowerIndex - 1)) falling++;
            return falling;
        }

        /// <summary>A hidden, transparent "Caption" child that carries a thing's spoken words (not drawn, not tapped).</summary>
        private static void SpeakMapThing(Transform node, string words)
        {
            var spoken = new GameObject("Caption", typeof(RectTransform));
            spoken.transform.SetParent(node, false);
            var rect = (RectTransform)spoken.transform;
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = rect.offsetMax = Vector2.zero;
            var text = spoken.AddComponent<Text>();
            text.font = LongPressCaption.DefaultFont;
            text.text = words;
            text.color = new Color(0f, 0f, 0f, 0f);
            text.raycastTarget = false;
        }
    }
}
