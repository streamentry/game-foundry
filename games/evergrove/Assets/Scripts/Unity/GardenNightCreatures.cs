using System.Collections.Generic;
using Evergrove.Core;
using UnityEngine;
using UnityEngine.UI;

namespace Evergrove.UnityClient
{
    /// <summary>
    /// Checklist 49: the night creatures, one per open stretch, placed on the map in the evening only.
    /// The app sets <see cref="Cells"/> (one free cell per stretch, -1 none) from the evening's phase;
    /// the map view places them when it is built and the cells are used once.
    /// Even stretches get a moth (kind 4), odd stretches a frog (kind 5): see NightCreatures.For.
    /// </summary>
    internal static class GardenNightCreatures
    {
        public static int[] Cells;

        public static void Place(RectTransform land, List<RectTransform> nodes, GardenMap map, float cellW, float cellH)
        {
            int[] cells = Cells;
            Cells = null;
            for (int reach = 0; cells != null && reach < cells.Length; reach++)
            {
                int cell = cells[reach];
                if (cell < 0 || cell >= map.CellCount) continue;
                int kind = reach % 2 == 0 ? 4 : 5;
                var night = new GameObject(kind == 4 ? "A moth, by the lamp" : "A frog, at the water's edge", typeof(RectTransform));
                night.transform.SetParent(land, false);
                var nightRect = (RectTransform)night.transform;
                nightRect.anchorMin = nightRect.anchorMax = new Vector2(0f, 1f);
                nightRect.sizeDelta = new Vector2(40f, 40f);
                nightRect.anchoredPosition = new Vector2((cell % map.Columns + 0.5f) * cellW, -(cell / map.Columns + 0.6f) * cellH);
                night.AddComponent<GardenCreature>().Kind = kind;
                LongPressCaption.Attach(night, night.name, LongPressCaption.DefaultFont);
                nightRect.localScale = Vector3.one * Mathf.Clamp(cellW / 30f, 1f, 2.4f);
                nodes.Add(nightRect);
            }
        }
    }
}
