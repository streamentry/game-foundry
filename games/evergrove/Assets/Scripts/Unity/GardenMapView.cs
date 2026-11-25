using System.Collections.Generic;
using Evergrove.Core;
using UnityEngine;
using UnityEngine.UI;

namespace Evergrove.UnityClient
{
    /// <summary>
    /// The open garden map on the home screen (E3, I176): the top-down painted
    /// plot, pannable with one finger, with every tree, flower and building the
    /// player has made standing where GardenMap puts it. Back-to-front by row,
    /// so nearer things overlap further ones. No game state lives here.
    /// </summary>
    internal sealed class GardenMapView : MonoBehaviour
    {
        public struct Placed
        {
            public Sprite Art;
            public int Cell;
            public VillageChoice Family;
            public bool Newest;
        }

        // How tall each family stands, in cells: trees tower, flowers stay low.
        private static float HeightInCells(VillageChoice family)
        {
            return family == VillageChoice.ShadeTree ? 1.7f : family == VillageChoice.Flowers ? 1.05f : 1.35f;
        }

        public static GardenMapView Build(RectTransform window, Sprite plot, GardenMap map,
                                          IList<Placed> items, float widthScale = 1.5f)
        {
            var host = new GameObject("Garden map", typeof(RectTransform));
            host.transform.SetParent(window, false);
            var hostRect = (RectTransform)host.transform;
            hostRect.anchorMin = Vector2.zero;
            hostRect.anchorMax = Vector2.one;
            hostRect.offsetMin = hostRect.offsetMax = Vector2.zero;
            host.transform.SetAsFirstSibling();
            var view = host.AddComponent<GardenMapView>();
            // A transparent hit area so the whole window pans.
            var hit = host.AddComponent<Image>();
            hit.color = new Color(1f, 1f, 1f, 0f);
            var scroll = host.AddComponent<ScrollRect>();
            scroll.horizontal = true;
            scroll.vertical = true;
            scroll.movementType = ScrollRect.MovementType.Elastic;
            scroll.elasticity = 0.12f;
            scroll.inertia = true;
            scroll.decelerationRate = 0.12f;
            scroll.scrollSensitivity = 18f;
            scroll.viewport = hostRect;

            var land = new GameObject("Garden plot", typeof(RectTransform));
            land.transform.SetParent(host.transform, false);
            var landRect = (RectTransform)land.transform;
            landRect.anchorMin = landRect.anchorMax = new Vector2(0.5f, 0.5f);
            landRect.pivot = new Vector2(0.5f, 0.5f);
            scroll.content = landRect;
            var ground = land.AddComponent<Image>();
            ground.sprite = plot;
            ground.raycastTarget = false;

            view.scroll = scroll;
            view.land = landRect;
            view.window = window;
            view.map = map;
            view.widthScale = widthScale;
            view.aspect = plot != null ? plot.rect.height / Mathf.Max(1f, plot.rect.width) : 1.5f;
            view.items = new List<Placed>(items);
            view.Layout();
            return view;
        }

        private ScrollRect scroll;
        private RectTransform land;
        private RectTransform window;
        private GardenMap map;
        private float widthScale;
        private float aspect;
        private List<Placed> items;
        private Vector2 lastWindow;
        private readonly List<RectTransform> nodes = new List<RectTransform>();

        private void LateUpdate()
        {
            // The hub's window is sized by anchors; lay out again once it settles.
            if (window != null && window.rect.size != lastWindow) Layout();
        }

        private void Layout()
        {
            if (window == null || map == null) return;
            lastWindow = window.rect.size;
            float width = Mathf.Max(window.rect.width * widthScale, window.rect.height / aspect);
            float height = width * aspect;
            land.sizeDelta = new Vector2(width, height);
            float cellW = width / map.Columns, cellH = height / map.Rows;
            foreach (RectTransform old in nodes) if (old != null) Destroy(old.gameObject);
            nodes.Clear();
            // Back to front: rows further up the map are drawn first.
            var order = new List<Placed>(items);
            order.Sort((a, b) => a.Cell / map.Columns != b.Cell / map.Columns
                ? (a.Cell / map.Columns).CompareTo(b.Cell / map.Columns)
                : a.Cell.CompareTo(b.Cell));
            Vector2 focus = Vector2.zero;
            bool focused = false;
            foreach (Placed item in order)
            {
                if (item.Art == null || item.Cell < 0 || item.Cell >= map.CellCount) continue;
                int column = item.Cell % map.Columns, row = item.Cell / map.Columns;
                var node = new GameObject("On the map", typeof(RectTransform));
                node.transform.SetParent(land, false);
                var rect = (RectTransform)node.transform;
                rect.anchorMin = rect.anchorMax = new Vector2(0f, 1f);
                rect.pivot = new Vector2(0.5f, 0.08f); // stands on its patch of grass
                float tall = cellH * HeightInCells(item.Family);
                float aspectArt = item.Art.rect.width / Mathf.Max(1f, item.Art.rect.height);
                rect.sizeDelta = new Vector2(tall * aspectArt, tall);
                // Feet near the lower part of the cell.
                Vector2 at = new Vector2((column + 0.5f) * cellW, -(row + 0.78f) * cellH);
                rect.anchoredPosition = at;
                var image = node.AddComponent<Image>();
                image.sprite = item.Art;
                image.preserveAspect = true;
                image.raycastTarget = false;
                nodes.Add(rect);
                if (item.Newest) { focus = at; focused = true; }
            }
            // Open on the newest thing you made, else the top of the valley.
            Vector2 view = window.rect.size;
            float spanX = Mathf.Max(1f, width - view.x), spanY = Mathf.Max(1f, height - view.y);
            float nx = focused ? Mathf.Clamp01((focus.x - view.x / 2f) / spanX) : 0.5f;
            float ny = focused ? 1f - Mathf.Clamp01((-focus.y - view.y / 2f) / spanY) : 1f;
            scroll.StopMovement();
            scroll.normalizedPosition = new Vector2(nx, ny);
        }
    }
}
