using System;
using System.Collections.Generic;
using Evergrove.Core;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Evergrove.UnityClient
{
    /// <summary>
    /// The open garden map (E3 I176, E4 I178): the top-down painted plot, pannable
    /// in every direction, with every tree, flower and building the player has made
    /// standing on its cell. Tap a thing to pick it up, tap free land to put it
    /// there. The item "in hand" wears a soft ring and bobs. Placement rules and
    /// saving live outside (GardenMap, WorldModel); this only shows and reports taps.
    /// </summary>
    internal sealed class GardenMapView : MonoBehaviour
    {
        public struct Placed
        {
            public Sprite Art;
            public int Cell;
            public int Index;            // the choice this item came from
            public VillageChoice Family;
            public bool Newest;
        }

        private static float HeightInCells(VillageChoice family)
        {
            return family == VillageChoice.ShadeTree ? 1.7f : family == VillageChoice.Flowers ? 1.05f : 1.35f;
        }

        public static GardenMapView Build(RectTransform window, Sprite plot, GardenMap map,
                                          IList<Placed> items, float widthScale = 1.5f,
                                          Action<int> onLand = null, Action<int> onItem = null,
                                          int inHand = -1, bool growNewest = false)
        {
            var host = new GameObject("Garden map", typeof(RectTransform));
            host.transform.SetParent(window, false);
            var hostRect = (RectTransform)host.transform;
            hostRect.anchorMin = Vector2.zero;
            hostRect.anchorMax = Vector2.one;
            hostRect.offsetMin = hostRect.offsetMax = Vector2.zero;
            host.transform.SetAsFirstSibling();
            var view = host.AddComponent<GardenMapView>();
            var hit = host.AddComponent<Image>();
            hit.color = new Color(1f, 1f, 1f, 0f);
            // Pans both ways; a vertical swipe at the map's top or bottom edge
            // scrolls the page instead, so the two never fight (I178).
            var scroll = host.AddComponent<NestedScrollRect>();
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
            ground.raycastTarget = onLand != null;
            if (onLand != null) land.AddComponent<MapTap>().Tapped = local => view.TapLand(local);

            view.scroll = scroll;
            view.land = landRect;
            view.window = window;
            view.map = map;
            view.widthScale = widthScale;
            view.aspect = plot != null ? plot.rect.height / Mathf.Max(1f, plot.rect.width) : 1.5f;
            view.items = new List<Placed>(items);
            view.onLand = onLand;
            view.onItem = onItem;
            view.inHand = inHand;
            view.growNewest = growNewest;
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
        private Action<int> onLand;
        private Action<int> onItem;
        private int inHand = -1;
        private bool growNewest;
        private Vector2 lastWindow;
        private readonly List<RectTransform> nodes = new List<RectTransform>();

        private void LateUpdate()
        {
            if (window != null && window.rect.size != lastWindow) Layout();
        }

        // A tap on bare land: the cell under the finger.
        private void TapLand(Vector2 local)
        {
            if (onLand == null || map == null) return;
            float width = land.rect.width, height = land.rect.height;
            float x = local.x + width / 2f, y = height / 2f - local.y;
            int column = Mathf.FloorToInt(x / (width / map.Columns));
            int row = Mathf.FloorToInt(y / (height / map.Rows));
            if (column < 0 || column >= map.Columns || row < 0 || row >= map.Rows) return;
            onLand(row * map.Columns + column);
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
                Vector2 at = new Vector2((column + 0.5f) * cellW, -(row + 0.78f) * cellH);
                bool held = item.Index == inHand;
                if (held)
                {
                    // A soft ring on the ground: "this one is in your hand".
                    var ring = new GameObject("In your hand", typeof(RectTransform));
                    ring.transform.SetParent(land, false);
                    var ringRect = (RectTransform)ring.transform;
                    ringRect.anchorMin = ringRect.anchorMax = new Vector2(0f, 1f);
                    ringRect.sizeDelta = new Vector2(cellW * 1.25f, cellH * 0.62f);
                    ringRect.anchoredPosition = at + new Vector2(0f, cellH * 0.08f);
                    var ringImage = ring.AddComponent<Image>();
                    ringImage.sprite = MosseloraArt.Ring(16);
                    ringImage.type = Image.Type.Sliced;
                    ringImage.color = new Color(1f, 0.96f, 0.78f, 0.95f);
                    ringImage.raycastTarget = false;
                    Breathe.Add(ringImage);
                    nodes.Add(ringRect);
                }
                var node = new GameObject("On the map", typeof(RectTransform));
                node.transform.SetParent(land, false);
                var rect = (RectTransform)node.transform;
                rect.anchorMin = rect.anchorMax = new Vector2(0f, 1f);
                rect.pivot = new Vector2(0.5f, 0.08f);
                float tall = cellH * HeightInCells(item.Family) * (held ? 1.12f : 1f);
                float aspectArt = item.Art.rect.width / Mathf.Max(1f, item.Art.rect.height);
                rect.sizeDelta = new Vector2(tall * aspectArt, tall);
                rect.anchoredPosition = at;
                var image = node.AddComponent<Image>();
                image.sprite = item.Art;
                image.preserveAspect = true;
                image.raycastTarget = onItem != null;
                if (onItem != null)
                {
                    int index = item.Index;
                    var button = node.AddComponent<Button>();
                    button.transition = Selectable.Transition.None;
                    button.onClick.AddListener(() => onItem(index));
                }
                if (held) node.AddComponent<Bob>();
                else if (growNewest && item.Newest) node.AddComponent<GrowUp>();
                nodes.Add(rect);
                if (held || (inHand < 0 && item.Newest)) { focus = at; focused = true; }
            }
            Vector2 view = window.rect.size;
            float spanX = Mathf.Max(1f, width - view.x), spanY = Mathf.Max(1f, height - view.y);
            float nx = focused ? Mathf.Clamp01((focus.x - view.x / 2f) / spanX) : 0.5f;
            float ny = focused ? 1f - Mathf.Clamp01((-focus.y - view.y / 2f) / spanY) : 1f;
            scroll.StopMovement();
            scroll.normalizedPosition = new Vector2(nx, ny);
        }

        /// <summary>Reports taps on the plot (no tap when the finger dragged).</summary>
        private sealed class MapTap : MonoBehaviour, IPointerClickHandler
        {
            public Action<Vector2> Tapped;

            public void OnPointerClick(PointerEventData eventData)
            {
                if (eventData.dragging) return;
                var rect = (RectTransform)transform;
                if (RectTransformUtility.ScreenPointToLocalPointInRectangle(rect, eventData.position,
                        eventData.pressEventCamera, out Vector2 local))
                    Tapped?.Invoke(local);
            }
        }

        /// <summary>The item in hand bobs gently.</summary>
        private sealed class Bob : MonoBehaviour
        {
            private float clock;
            private void Update()
            {
                clock += Time.unscaledDeltaTime;
                float lift = (Mathf.Sin(clock * 3.2f) + 1f) * 0.5f;
                transform.localScale = new Vector3(1f, 1f + 0.04f * lift, 1f);
            }
        }

        /// <summary>The newest thing rises from the ground once.</summary>
        private sealed class GrowUp : MonoBehaviour
        {
            private float clock;
            private void Start() { transform.localScale = new Vector3(1f, 0f, 1f); }
            private void Update()
            {
                clock += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01((clock - 0.25f) / 0.8f);
                float s = Mathf.SmoothStep(0f, 1f, t);
                transform.localScale = new Vector3(0.6f + 0.4f * s, s, 1f);
                if (t >= 1f) Destroy(this);
            }
        }
    }

    /// <summary>
    /// A ScrollRect inside the page's ScrollRect. Vertical swipes it can't use
    /// (map shorter than its window, or already at that edge) go to the page.
    /// </summary>
    internal sealed class NestedScrollRect : ScrollRect
    {
        private ScrollRect page;
        private bool toPage;

        protected override void Start()
        {
            base.Start();
            page = transform.parent != null ? transform.parent.GetComponentInParent<ScrollRect>() : null;
        }

        public override void OnInitializePotentialDrag(PointerEventData eventData)
        {
            if (page != null) page.OnInitializePotentialDrag(eventData);
            base.OnInitializePotentialDrag(eventData);
        }

        public override void OnBeginDrag(PointerEventData eventData)
        {
            toPage = false;
            Vector2 d = eventData.delta;
            if (page != null && Mathf.Abs(d.y) > Mathf.Abs(d.x) && content != null && viewport != null)
            {
                bool taller = content.rect.height > viewport.rect.height + 1f;
                float at = verticalNormalizedPosition;
                // Finger moving down shows what is above: past the top, it's the page's turn.
                if (!taller || (d.y < 0f && at >= 0.995f) || (d.y > 0f && at <= 0.005f)) toPage = true;
            }
            if (toPage) page.OnBeginDrag(eventData);
            else base.OnBeginDrag(eventData);
        }

        public override void OnDrag(PointerEventData eventData)
        {
            if (toPage) page.OnDrag(eventData);
            else base.OnDrag(eventData);
        }

        public override void OnEndDrag(PointerEventData eventData)
        {
            if (toPage) page.OnEndDrag(eventData);
            else base.OnEndDrag(eventData);
            toPage = false;
        }

        public override void OnScroll(PointerEventData data)
        {
            if (page != null && !(content != null && viewport != null && content.rect.height > viewport.rect.height + 1f))
                page.OnScroll(data);
            else base.OnScroll(data);
        }
    }
}
