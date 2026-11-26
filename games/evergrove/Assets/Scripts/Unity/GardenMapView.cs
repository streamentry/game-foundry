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
            // Large enough to read at arm's length (I179: was 1.7 / 1.35 / 1.05).
            return family == VillageChoice.ShadeTree ? 2.3f : family == VillageChoice.Flowers ? 1.45f : 1.85f;
        }

        public static GardenMapView Build(RectTransform window, Sprite plot, GardenMap map,
                                          IList<Placed> items, float widthScale = 1.5f,
                                          Action<int> onLand = null, Action<int> onItem = null,
                                          int inHand = -1, bool growNewest = false,
                                          float greened = 1f, float greenedBefore = -1f,
                                          Func<RectTransform, Func<int, Vector2>, List<RectTransform>> decorate = null,
                                          float apron = 0f)
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
            // The painting itself (I190: a child, so the land can run on past it).
            var plotNode = new GameObject("Garden painting", typeof(RectTransform));
            plotNode.transform.SetParent(land.transform, false);
            var plotRect = (RectTransform)plotNode.transform;
            plotRect.anchorMin = new Vector2(0f, 1f);
            plotRect.anchorMax = new Vector2(1f, 1f);
            plotRect.pivot = new Vector2(0.5f, 1f);
            var ground = plotNode.AddComponent<Image>();
            view.ground = ground;
            view.plotRect = plotRect;
            ground.sprite = plot;
            ground.raycastTarget = onLand != null;
            if (onLand != null) plotNode.AddComponent<MapTap>().Tapped = local => view.TapLand(local);
            // Below the painting the meadow runs on, mirrored so it joins without a
            // seam (I190): the view can then frame the lowest things above the
            // play token instead of stopping at the painting's edge.
            view.apron = Mathf.Clamp(apron, 0f, 0.4f);
            if (view.apron > 0f && plot != null)
            {
                var apronNode = new GameObject("Meadow beyond", typeof(RectTransform));
                apronNode.transform.SetParent(land.transform, false);
                var apronRect = (RectTransform)apronNode.transform;
                apronRect.anchorMin = new Vector2(0f, 0f);
                apronRect.anchorMax = new Vector2(1f, 0f);
                apronRect.pivot = new Vector2(0.5f, 0f);
                view.apronRect = apronRect;
                var apronImage = apronNode.AddComponent<RawImage>();
                apronImage.texture = plot.texture;
                Rect r = plot.textureRect;
                float tw = plot.texture.width, th = plot.texture.height;
                float u = r.x / tw, v = r.y / th, uw = r.width / tw, vh = r.height / th * view.apron;
                apronImage.uvRect = new Rect(u, v + vh, uw, -vh);
                apronImage.raycastTarget = false;
                if (greened < 0.999f)
                {
                    // Still dry down here, like the painting's lower valley.
                    var wash = new GameObject("Dry beyond", typeof(RectTransform));
                    wash.transform.SetParent(apronNode.transform, false);
                    var washRect = (RectTransform)wash.transform;
                    washRect.anchorMin = Vector2.zero;
                    washRect.anchorMax = Vector2.one;
                    washRect.offsetMin = washRect.offsetMax = Vector2.zero;
                    var washImage = wash.AddComponent<Image>();
                    washImage.color = new Color(0.84f, 0.74f, 0.50f, 0.58f);
                    washImage.raycastTarget = false;
                }
            }

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
            view.greened = Mathf.Clamp01(greened);
            view.greenedFrom = greenedBefore < 0f ? view.greened : Mathf.Clamp01(greenedBefore);
            view.decorate = decorate;
            view.BuildDryLand();
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
        private float greened = 1f, greenedFrom = 1f;
        // Where in the window (0 top … 1 bottom) the focused item sits (I185: lower
        // when a title floats over the map's top).
        public float FocusFromTop = 0.5f;
        private Func<RectTransform, Func<int, Vector2>, List<RectTransform>> decorate;
        private RectTransform dry;
        private float dryClock;
        private readonly List<RectTransform> nodes = new List<RectTransform>();

        // Water shapes the land (E5, I180): the valley below the restored water is
        // dry straw. Each stream restored greens the next stretch down the map;
        // after a choice the dry edge draws back while you watch.
        private void BuildDryLand()
        {
            if (greened >= 0.999f && greenedFrom >= 0.999f) return;
            var node = new GameObject("Dry land", typeof(RectTransform));
            node.transform.SetParent(plotRect, false); // over the painting only (I190)
            dry = (RectTransform)node.transform;
            dry.anchorMin = new Vector2(0f, 0f);
            dry.anchorMax = new Vector2(1f, 1f - greenedFrom);
            dry.offsetMin = dry.offsetMax = Vector2.zero;
            var wash = node.AddComponent<Image>();
            wash.color = new Color(0.84f, 0.74f, 0.50f, 0.58f);
            wash.raycastTarget = false;
            // A soft edge where the green meets the dry ground.
            var edge = new GameObject("Dry edge", typeof(RectTransform));
            edge.transform.SetParent(node.transform, false);
            var edgeRect = (RectTransform)edge.transform;
            edgeRect.anchorMin = new Vector2(0f, 1f);
            edgeRect.anchorMax = new Vector2(1f, 1f);
            edgeRect.pivot = new Vector2(0.5f, 0f);
            edgeRect.sizeDelta = new Vector2(0f, 60f);
            edgeRect.localScale = new Vector3(1f, -1f, 1f);
            var edgeImage = edge.AddComponent<Image>();
            edgeImage.sprite = MosseloraArt.TopFade();
            edgeImage.color = wash.color;
            edgeImage.raycastTarget = false;
            edgeRect.anchoredPosition = new Vector2(0f, 60f);
            node.transform.SetSiblingIndex(0); // under every item on the map
        }

        private Image ground;
        private RectTransform plotRect, apronRect;
        private float apron;

        private void Update()
        {
            if (dry == null || Mathf.Approximately(greenedFrom, greened)) return;
            dryClock += Time.unscaledDeltaTime;
            float t = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01((dryClock - 0.4f) / 1.4f));
            float at = Mathf.Lerp(greenedFrom, greened, t);
            dry.anchorMax = new Vector2(1f, 1f - at);
            if (t >= 1f) greenedFrom = greened;
        }

        private void LateUpdate()
        {
            if (window != null && window.rect.size != lastWindow) Layout();
        }

        // A tap on bare land: the cell under the finger.
        private void TapLand(Vector2 local)
        {
            if (onLand == null || map == null) return;
            float width = land.rect.width, height = land.rect.height;
            width = plotRect.rect.width; height = plotRect.rect.height;
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
            plotRect.sizeDelta = new Vector2(0f, height);
            if (apronRect != null) apronRect.sizeDelta = new Vector2(0f, height * apron);
            float cellW = width / map.Columns, cellH = height / map.Rows;
            height *= 1f + apron; // the land runs on below the painting (I190)
            land.sizeDelta = new Vector2(width, height);
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
                if (held)
                {
                    node.AddComponent<Bob>();
                    // A small pressing finger beside it: "tap the land to put it down" (I179).
                    Sprite tap = MosseloraArt.Load(MosseloraArt.IconTap);
                    if (tap != null)
                    {
                        var hint = new GameObject("Tap land to place", typeof(RectTransform));
                        hint.transform.SetParent(land, false);
                        var hintRect = (RectTransform)hint.transform;
                        hintRect.anchorMin = hintRect.anchorMax = new Vector2(0f, 1f);
                        float size = Mathf.Max(30f, cellW * 0.7f);
                        hintRect.sizeDelta = new Vector2(size, size);
                        hintRect.anchoredPosition = at + new Vector2(cellW * 0.9f, cellH * 0.9f);
                        var hintImage = hint.AddComponent<Image>();
                        hintImage.sprite = tap;
                        hintImage.preserveAspect = true;
                        hintImage.color = new Color(0.12f, 0.27f, 0.22f, 0.9f);
                        hintImage.raycastTarget = false;
                        hint.AddComponent<GhostPress>();
                        nodes.Add(hintRect);
                    }
                }
                else if (growNewest && item.Newest) node.AddComponent<GrowUp>();
                nodes.Add(rect);
                if (held || (inHand < 0 && item.Newest)) { focus = at; focused = true; }
            }
            // Others standing on the map (neighbours, I181), laid out with it.
            if (decorate != null)
            {
                List<RectTransform> extra = decorate(land, cell =>
                    new Vector2((cell % map.Columns + 0.5f) * cellW, -(cell / map.Columns + 0.78f) * cellH));
                if (extra != null) nodes.AddRange(extra);
            }
            Vector2 view = window.rect.size;
            float spanX = Mathf.Max(1f, width - view.x), spanY = Mathf.Max(1f, height - view.y);
            float nx = focused ? Mathf.Clamp01((focus.x - view.x / 2f) / spanX) : 0.5f;
            float ny = focused ? 1f - Mathf.Clamp01((-focus.y - view.y * FocusFromTop) / spanY) : 1f;
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
