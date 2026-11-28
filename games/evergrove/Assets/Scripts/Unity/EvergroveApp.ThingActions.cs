using System;
using System.Collections.Generic;
using Evergrove.Core;
using UnityEngine;
using UnityEngine.UI;

namespace Evergrove.UnityClient
{
    /// <summary>
    /// Checklist 13: a tapped thing shows its two actions right beside it, where the thumb is: round 52 pt
    /// tokens, Play its puzzle and Move it. Only the hub map asks for them (Map.cs), and only while a story
    /// thing or a bought plant is held in a Living garden. Move keeps the thing in hand; the next tap on free
    /// land puts it down (OnMapLand). The bottom play token is gone; see ThingTaps for the tap counts.
    /// </summary>
    public sealed partial class EvergroveApp
    {
        // Thing actions sit to the side of the held thing; the thing's middle is estimated at this many rows above its cell point.
        private const float ThingMiddleRows = 0.5f;
        private const float ThingSideGap = 8f;

        /// <summary>The held thing's Play and Move tokens, placed on the land beside it (none while nothing is held).</summary>
        private const string ThingActionsName = "Thing actions";

        private List<RectTransform> ThingActionsOnMap(RectTransform land, Func<int, Vector2> point)
        {
            var placed = new List<RectTransform>();
            // Each layout places the row afresh: the last one goes first (renamed and hidden at once, so a
            // second layout in the same frame never finds it again before Destroy runs).
            Transform old = land != null ? land.Find(ThingActionsName) : null;
            if (old != null) { old.name = ThingActionsName + " (gone)"; old.gameObject.SetActive(false); Destroy(old.gameObject); }
            if (land == null || world.Mode != PlayMode.Living) return placed;
            int held = mapInHand >= 0 && mapInHand < world.Choices.Length ? mapInHand : -1;
            int heldPlant = held < 0 && heldBought >= 0 && heldBought < world.BoughtCount ? heldBought : -1;
            if (held < 0 && heldPlant < 0) return placed;
            GardenMap map = LoadGardenMap();
            if (map == null) return placed;
            int cell = -1;
            if (held >= 0)
            {
                int[] cells = map.Layout(world.Choices, world.Placements, Occupied());
                if (held < cells.Length) cell = cells[held];
            }
            else cell = world.Bought[heldPlant * 2 + 1];
            if (cell < 0 || cell >= map.CellCount) return placed;

            // Cell sizes come from the same anchored grid the map lays out (GardenMapView.Layout).
            Vector2 at = point(cell);
            float cellW = Mathf.Abs((point(1) - point(0)).x);
            float rowH = Mathf.Abs((point(map.Columns) - point(0)).y);
            Vector2 middle = at + new Vector2(0f, rowH * ThingMiddleRows);
            float rowWidth = 52f * 2f + ThingSideGap;
            // Beside the thing; in the two rightmost columns, on its left so the row stays on the land.
            bool onLeft = cell % map.Columns >= map.Columns - 2;
            float beside = cellW * 0.6f + ThingSideGap;
            float x = onLeft ? middle.x - beside - rowWidth : middle.x + beside;
            x = Mathf.Clamp(x, 4f, Mathf.Max(4f, land.sizeDelta.x - rowWidth - 4f));

            GameObject box = Node(ThingActionsName, land);
            var boxRect = (RectTransform)box.transform;
            boxRect.anchorMin = boxRect.anchorMax = new Vector2(0f, 1f);
            boxRect.pivot = new Vector2(0.5f, 0.5f);
            boxRect.sizeDelta = new Vector2(rowWidth, 52f);
            boxRect.anchoredPosition = new Vector2(x + rowWidth / 2f, middle.y);
            var row = box.AddComponent<HorizontalLayoutGroup>();
            row.spacing = ThingSideGap;
            row.childAlignment = TextAnchor.MiddleCenter;
            row.childControlWidth = true;
            row.childControlHeight = true;
            row.childForceExpandWidth = false;
            row.childForceExpandHeight = false;

            string heldName = held >= 0 ? ItemAt(held, world.Choices[held]).Name : Nursery.Item(world.Bought[heldPlant * 2]).Name;
            Button play = IconButton(box.transform, "Play its puzzle", heldName, MosseloraArt.IconContinue,
                () => { if (heldPlant >= 0) OpenBoughtPlay(heldPlant); else OpenTreePlay(held); },
                52, MosseloraTheme.WaterAction, false, null, false, 0, 0, true);
            Button move = IconButton(box.transform, "Move it", "Move " + heldName, MosseloraArt.IconChange,
                ShowMoveHint, 52, MosseloraTheme.QuietAction, false, null, false, 0, 0, true);
            if (gentleMotion) BloomPulse.Add(play.gameObject, 0.2f, 0.06f);
            placed.Add(boxRect);
            return placed;
        }

        // Move it: the thing stays in hand; the next tap on free land puts it down.
        private void ShowMoveHint()
        {
            if (titleScreen != null)
                RuleToast.Show((RectTransform)titleScreen.transform, "Tap where it should go.", font, Ink, MosseloraTheme.CardFill);
        }

#if UNITY_EDITOR || DEVELOPMENT_BUILD
        // Checklist 13 tour: put down whatever is held, where it stands (the tour's Move check leaves it in place).
        internal void QaPutDownThing()
        {
            mapInHand = -1;
            heldBought = -1;
            RefreshMaps();
        }
#endif
    }
}
