using System;
using System.Collections.Generic;
using Evergrove.Core;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
#if !ENABLE_LEGACY_INPUT_MANAGER && ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

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
            public bool Bloomed;         // its puzzle has been solved (I204): a warm ring of light
            public bool BloomNow;        // just solved (I212): the ring grows in while you watch
            public bool HoldsPuzzle;     // has a puzzle not yet solved (I221): a small play mark
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
                                          float apron = 0f, bool teaser = false)
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
            // The endless river (I192): one copy of the painting per open reach,
            // every other one mirrored so the creek runs on without a seam.
            int reaches = map != null ? Mathf.Max(1, map.Reaches) : 1;
            view.reaches = reaches;
            if (reaches == 1) ground.sprite = plot;
            else
            {
                ground.color = new Color(1f, 1f, 1f, 0f); // still catches taps
                for (int reach = 0; reach < reaches; reach++)
                {
                    RawImage copy = ReachImage(plotRect, plot, reach % 2 == 1, "Reach " + (reach + 1));
                    var copyRect = copy.rectTransform;
                    copyRect.anchorMin = new Vector2(0f, 1f - (reach + 1f) / reaches);
                    copyRect.anchorMax = new Vector2(1f, 1f - reach / (float)reaches);
                    copyRect.offsetMin = copyRect.offsetMax = Vector2.zero;
                }
            }
            ground.raycastTarget = onLand != null;
            if (onLand != null) plotNode.AddComponent<MapTap>().Tapped = local => view.TapLand(local);
            // Below the painting the meadow runs on, mirrored so it joins without a
            // seam (I190): the view can then frame the lowest things above the
            // play token instead of stopping at the painting's edge.
            // Below the open river, the next reach waits: dry, in a morning mist,
            // not yet yours to plant (I192). It opens with the next streams.
            if (teaser && plot != null)
            {
                view.teaser = 0.55f;
                RawImage next = ReachImage(landRect, plot, reaches % 2 == 1, "Next reach in mist");
                var nextRect = next.rectTransform;
                nextRect.anchorMin = new Vector2(0f, 0f);
                nextRect.anchorMax = new Vector2(1f, 0f);
                nextRect.pivot = new Vector2(0.5f, 0f);
                view.apronRect = nextRect;
                // Only the top of that reach shows, so trim the painting to match.
                Rect r = plot.textureRect;
                float tu = r.x / plot.texture.width, tv = r.y / plot.texture.height;
                float tw = r.width / plot.texture.width, th = r.height / plot.texture.height, t = view.teaser;
                next.uvRect = reaches % 2 == 1
                    ? new Rect(tu, tv + th * t, tw, -th * t)          // mirrored: the painting's foot, upside down
                    : new Rect(tu, tv + th * (1f - t), tw, th * t);   // as painted: the painting's head
                // Both washes fade in from clear (I199): a solid straw wash drew a hard
                // line across the garden where the open river ends.
                Wash(nextRect, new Color(0.84f, 0.74f, 0.50f, 0.5f), true);
                Wash(nextRect, new Color(0.97f, 0.95f, 0.89f, 0.82f), true);
            }
            view.apron = teaser ? 0f : Mathf.Clamp(apron, 0f, 0.4f);
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
        // Where the view is (normalized), and a place to hold it after a rebuild,
        // so planting one thing after another never jumps the view away (I193).
        public Vector2 Position { get { return scroll != null ? scroll.normalizedPosition : new Vector2(-1f, -1f); } }
        private Vector2 keep = new Vector2(-1f, -1f);
        public void KeepAt(Vector2 position)
        {
            keep = position;
            if (keep.x >= 0f) Layout();
        }
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
        private float apron, teaser;
        private int reaches = 1;

        // One copy of the painting as a RawImage, mirrored top to bottom if asked.
        private static RawImage ReachImage(RectTransform parent, Sprite plot, bool mirrored, string name)
        {
            var node = new GameObject(name, typeof(RectTransform));
            node.transform.SetParent(parent, false);
            var image = node.AddComponent<RawImage>();
            image.texture = plot.texture;
            Rect r = plot.textureRect;
            float tw = plot.texture.width, th = plot.texture.height;
            float u = r.x / tw, v = r.y / th, uw = r.width / tw, vh = r.height / th;
            image.uvRect = mirrored ? new Rect(u, v + vh, uw, -vh) : new Rect(u, v, uw, vh);
            image.raycastTarget = false;
            return image;
        }

        // A full wash over a rect, or (fog) a fade from clear at the top to solid at the bottom.
        private static void Wash(RectTransform parent, Color color, bool fog)
        {
            var node = new GameObject(fog ? "Mist" : "Dry", typeof(RectTransform));
            node.transform.SetParent(parent, false);
            var rect = (RectTransform)node.transform;
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = rect.offsetMax = Vector2.zero;
            var image = node.AddComponent<Image>();
            image.color = color;
            image.raycastTarget = false;
            if (fog)
            {
                image.sprite = MosseloraArt.TopFade();
                rect.localScale = new Vector3(1f, -1f, 1f); // solid at the bottom
            }
        }

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
            Pinch();
            if (!pinching && window != null && window.rect.size != lastWindow) Layout();
        }

        // ---- I202: pinch to zoom. Two fingers scale the land around the point
        // between them; one relayout when they lift. The zoom is reported so the
        // caller can keep it across rebuilds.
        public float MinZoom = 1f, MaxZoom = 3f;
        public float Zoom { get { return widthScale; } }
        public Action<float> Zoomed;
        private bool pinching;
        private float pinchFrom;
        private Vector2 pinchPoint; // the land point under the fingers, unscaled
        private float pinchScale = 1f;
        private float pinchEnded = -10f;
        // Lifting two fingers is not a tap on whatever was under them.
        private bool PinchBusy { get { return pinching || Time.unscaledTime - pinchEnded < 0.35f; } }

        private static bool TwoFingers(out Vector2 first, out Vector2 second)
        {
            first = second = Vector2.zero;
#if ENABLE_LEGACY_INPUT_MANAGER
            if (Input.touchCount != 2) return false;
            first = Input.GetTouch(0).position;
            second = Input.GetTouch(1).position;
            return true;
#elif ENABLE_INPUT_SYSTEM
            Touchscreen screen = Touchscreen.current;
            if (screen == null) return false;
            int found = 0;
            foreach (var touch in screen.touches)
            {
                if (!touch.isInProgress) continue;
                if (found == 0) first = touch.position.ReadValue();
                else if (found == 1) second = touch.position.ReadValue();
                found++;
            }
            return found == 2;
#else
            return false;
#endif
        }

        private void Pinch()
        {
            if (land == null || window == null || map == null) return;
            bool two = TwoFingers(out Vector2 a, out Vector2 b);
            var host = (RectTransform)land.parent;
            Camera eye = null;
            Canvas canvas = host.GetComponentInParent<Canvas>();
            if (canvas != null && canvas.renderMode != RenderMode.ScreenSpaceOverlay) eye = canvas.worldCamera;
            Vector2 middle = (a + b) * 0.5f;
            if (two && !pinching)
            {
                if (!RectTransformUtility.RectangleContainsScreenPoint(host, middle, eye)) return;
                pinching = true;
                pinchFrom = Mathf.Max(1f, Vector2.Distance(a, b));
                pinchScale = 1f;
                scroll.StopMovement();
                scroll.enabled = false; // the fingers own the land while they pinch
                RectTransformUtility.ScreenPointToLocalPointInRectangle(land, middle, eye, out pinchPoint);
                return;
            }
            if (!pinching) return;
            if (two)
            {
                float wanted = widthScale * Vector2.Distance(a, b) / pinchFrom;
                pinchScale = Mathf.Clamp(wanted, MinZoom, MaxZoom) / widthScale;
                land.localScale = new Vector3(pinchScale, pinchScale, 1f);
                RectTransformUtility.ScreenPointToLocalPointInRectangle(host, middle, eye, out Vector2 at);
                land.anchoredPosition = at - pinchPoint * pinchScale;
                return;
            }
            // Fingers lifted: settle at the new size, the same spot under them.
            pinching = false;
            // The land is now as wide as it looked, so the same anchored position
            // keeps the same spot under the fingers.
            Vector2 held = land.anchoredPosition;
            float scale = pinchScale;
            land.localScale = Vector3.one;
            widthScale = Mathf.Clamp(widthScale * scale, MinZoom, MaxZoom);
            growNewest = false; // no replay of the newest thing rising
            keep = new Vector2(-1f, -1f);
            Layout();
            land.anchoredPosition = held;
            pinchEnded = Time.unscaledTime;
            scroll.enabled = true;
            scroll.StopMovement();
            Zoomed?.Invoke(widthScale);
        }

        // A tap on bare land: the cell under the finger.
        private void TapLand(Vector2 local)
        {
            if (onLand == null || map == null || PinchBusy) return;
            // Measured from the painting's own corner, whatever its pivot (the I190
            // move to a top-pivoted child put taps half a map too low; fixed in I193).
            Rect area = plotRect.rect;
            float width = area.width, height = area.height;
            float x = local.x - area.xMin, y = area.yMax - local.y;
            int column = Mathf.FloorToInt(x / (width / map.Columns));
            int row = Mathf.FloorToInt(y / (height / map.Rows));
            if (column < 0 || column >= map.Columns || row < 0 || row >= map.Rows) return;
            onLand(row * map.Columns + column);
        }

        private void Layout()
        {
            if (window == null || map == null) return;
            lastWindow = window.rect.size;
            float riverTall = aspect * (reaches + teaser);
            float width = Mathf.Max(window.rect.width * widthScale, window.rect.height / riverTall);
            float reachHeight = width * aspect;
            float height = reachHeight * reaches;
            plotRect.sizeDelta = new Vector2(0f, height);
            if (apronRect != null) apronRect.sizeDelta = new Vector2(0f, teaser > 0f ? reachHeight * teaser : height * apron);
            float cellW = width / map.Columns, cellH = height / map.Rows;
            height += teaser > 0f ? reachHeight * teaser : height * apron; // the land runs on below (I190, I192)
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
                if (item.Bloomed && !held)
                {
                    // Solved: a soft golden glow on the ground beneath it.
                    var glow = new GameObject("Bloomed", typeof(RectTransform));
                    glow.transform.SetParent(land, false);
                    var glowRect = (RectTransform)glow.transform;
                    glowRect.anchorMin = glowRect.anchorMax = new Vector2(0f, 1f);
                    glowRect.sizeDelta = new Vector2(cellW * 1.4f, cellH * 0.66f);
                    glowRect.anchoredPosition = at + new Vector2(0f, cellH * 0.08f);
                    var glowImage = glow.AddComponent<Image>();
                    glowImage.sprite = MosseloraArt.Ring(16);
                    glowImage.type = Image.Type.Sliced;
                    glowImage.color = new Color(1f, 0.82f, 0.36f, 0.85f);
                    glowImage.raycastTarget = false;
                    if (item.BloomNow) glow.AddComponent<RingGrow>();
                    nodes.Add(glowRect);
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
                    button.onClick.AddListener(() => { if (!PinchBusy) onItem(index); });
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
                if (item.HoldsPuzzle && !held && onItem != null)
                {
                    // A small round play mark at the thing's shoulder: "there's a puzzle here".
                    var mark = new GameObject("Holds a puzzle", typeof(RectTransform));
                    mark.transform.SetParent(land, false);
                    var markRect = (RectTransform)mark.transform;
                    markRect.anchorMin = markRect.anchorMax = new Vector2(0f, 1f);
                    float d = Mathf.Clamp(cellW * 0.42f, 22f, 30f);
                    markRect.sizeDelta = new Vector2(d, d);
                    markRect.anchoredPosition = at + new Vector2(rect.sizeDelta.x * 0.36f, rect.sizeDelta.y * 0.78f);
                    var disc = mark.AddComponent<Image>();
                    disc.sprite = MosseloraArt.Rounded(16);
                    disc.color = MosseloraTheme.WaterAction;
                    disc.raycastTarget = false;
                    MosseloraArt.Lift(disc, 1.5f, 0.25f);
                    Sprite arrow = MosseloraArt.Load(MosseloraArt.IconContinue);
                    if (arrow != null)
                    {
                        var glyph = new GameObject("Play", typeof(RectTransform));
                        glyph.transform.SetParent(mark.transform, false);
                        var glyphRect = (RectTransform)glyph.transform;
                        glyphRect.anchorMin = glyphRect.anchorMax = new Vector2(0.5f, 0.5f);
                        glyphRect.sizeDelta = new Vector2(d * 0.58f, d * 0.58f);
                        var glyphImage = glyph.AddComponent<Image>();
                        glyphImage.sprite = arrow;
                        glyphImage.preserveAspect = true;
                        glyphImage.color = Color.white;
                        glyphImage.raycastTarget = false;
                    }
                    nodes.Add(markRect);
                }
                nodes.Add(rect);
                if (held || (inHand < 0 && item.Newest)) { focus = at; focused = true; }
            }
            // Others standing on the map (neighbours, I181), laid out with it.
            List<RectTransform> standing = null;
            if (decorate != null)
            {
                standing = decorate(land, cell =>
                    new Vector2((cell % map.Columns + 0.5f) * cellW, -(cell / map.Columns + 0.78f) * cellH));
                if (standing != null) nodes.AddRange(standing);
            }
            if (keep.x >= 0f)
            {
                scroll.StopMovement();
                scroll.normalizedPosition = keep;
                KeepClear(standing);
                return;
            }
            if (liftFocus > 0)
            {
                focus = new Vector2(width / 2f, -reachHeight * liftFocus);
                focused = true;
            }
            Vector2 view = window.rect.size;
            float spanX = Mathf.Max(1f, width - view.x), spanY = Mathf.Max(1f, height - view.y);
            float nx = focused ? Mathf.Clamp01((focus.x - view.x / 2f) / spanX) : 0.5f;
            float ny = focused ? 1f - Mathf.Clamp01((-focus.y - view.y * FocusFromTop) / spanY) : 1f;
            scroll.StopMovement();
            scroll.normalizedPosition = new Vector2(nx, ny);
            KeepClear(standing);
        }

        /// <summary>Height at the window's foot kept clear of neighbours (the play row).</summary>
        public float ClearBottom
        {
            get { return clearBottom; }
            set { if (Mathf.Approximately(clearBottom, value)) return; clearBottom = value; Layout(); }
        }
        private float clearBottom;

        /// <summary>Height at the window's top kept clear (the menu button, the purse, the letterbox).</summary>
        public float ClearTop
        {
            get { return clearTop; }
            set { if (Mathf.Approximately(clearTop, value)) return; clearTop = value; Layout(); }
        }
        private float clearTop;

        // Neighbours standing in the bottom band, where the round play token and the
        // basket sit, step up out of it (I218: Fen stood behind the play token).
        private void KeepClear(List<RectTransform> standing)
        {
            if (standing == null || (clearBottom <= 0f && clearTop <= 0f) || window == null) return;
            Canvas.ForceUpdateCanvases();
            Rect frame = window.rect;
            var corners = new Vector3[4];
            foreach (RectTransform person in standing)
            {
                if (person == null) continue;
                person.GetWorldCorners(corners);
                float scale = Mathf.Max(0.001f, land.lossyScale.y / window.lossyScale.y);
                float bottom = window.InverseTransformPoint(corners[0]).y;
                float top = window.InverseTransformPoint(corners[1]).y;
                float low = frame.yMin + clearBottom, high = frame.yMax - clearTop;
                float direction = 0f;
                if (clearBottom > 0f && bottom < low)
                {
                    person.anchoredPosition += new Vector2(0f, (low - bottom + 8f) / scale);
                    direction = 1f;
                }
                else if (clearTop > 0f && top > high && bottom > low)
                {
                    person.anchoredPosition -= new Vector2(0f, (top - high + 8f) / scale);
                    direction = -1f;
                }
                if (direction == 0f) continue;
                // Moved: keep clear of everyone else too, stepping on the same way.
                Vector2 size = person.rect.size;
                for (int tries = 0; tries < 4; tries++)
                {
                    bool clash = false;
                    foreach (RectTransform other in standing)
                        if (other != null && other != person &&
                            Mathf.Abs(other.anchoredPosition.x - person.anchoredPosition.x) < size.x * 0.9f &&
                            Mathf.Abs(other.anchoredPosition.y - person.anchoredPosition.y) < size.y * 1.7f) clash = true;
                    if (!clash) break;
                    // A figure is its portrait and its name below it: step a whole figure.
                    person.anchoredPosition += new Vector2(0f, direction * size.y * 1.7f);
                }
            }
        }

        /// <summary>
        /// A new reach of river has just opened (I195): it starts under the same
        /// dry mist as the reach that waited below, which lifts over two seconds.
        /// </summary>
        public void LiftMist(int reach)
        {
            if (plotRect == null || reach < 1 || reach >= reaches) return;
            var node = new GameObject("Mist lifting", typeof(RectTransform));
            node.transform.SetParent(plotRect, false);
            var rect = (RectTransform)node.transform;
            rect.anchorMin = new Vector2(0f, 1f - (reach + 1f) / reaches);
            rect.anchorMax = new Vector2(1f, 1f - reach / (float)reaches);
            rect.offsetMin = rect.offsetMax = Vector2.zero;
            Wash(rect, new Color(0.84f, 0.74f, 0.50f, 0.5f), true); // soft top edge, as the teaser (I199)
            Wash(rect, new Color(0.97f, 0.95f, 0.89f, 0.82f), true);
            node.AddComponent<CanvasGroup>().blocksRaycasts = false;
            node.AddComponent<Lift>();
            liftFocus = reach; // frame where the old river meets the new reach
            Layout();
        }

        private int liftFocus = -1;

        /// <summary>
        /// Drops on the next stretch's mist (I224): one per stream (story) or first
        /// solve (after), filled as they come; a full row opens the stretch. The
        /// drop just earned swells once. No words: the drops are the progress.
        /// </summary>
        public void ShowDrops(int have, int need, bool swellNewest)
        {
            if (apronRect == null || teaser <= 0f || need < 1) return;
            Transform old = apronRect.Find("Drops toward the next stretch");
            if (old != null) Destroy(old.gameObject);
            var row = new GameObject("Drops toward the next stretch", typeof(RectTransform));
            row.transform.SetParent(apronRect, false);
            var rect = (RectTransform)row.transform;
            rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.42f);
            const float size = 30f, gap = 12f;
            rect.sizeDelta = new Vector2(need * size + (need - 1) * gap, size);
            for (int i = 0; i < need; i++)
            {
                bool full = i < have;
                var drop = new GameObject(full ? "Drop filled" : "Drop to come", typeof(RectTransform));
                drop.transform.SetParent(row.transform, false);
                var dropRect = (RectTransform)drop.transform;
                dropRect.anchorMin = dropRect.anchorMax = new Vector2(0f, 0.5f);
                dropRect.pivot = new Vector2(0.5f, 0.5f);
                dropRect.sizeDelta = new Vector2(size, size);
                dropRect.anchoredPosition = new Vector2(size / 2f + i * (size + gap), 0f);
                var image = drop.AddComponent<Image>();
                image.sprite = MosseloraArt.Load(full ? MosseloraArt.Droplet : MosseloraArt.DropletEmpty);
                image.preserveAspect = true;
                image.raycastTarget = false;
                image.color = full ? Color.white : new Color(0.30f, 0.35f, 0.33f, 1f); // as the board's drops (I146)
                if (full && swellNewest && i == have - 1) drop.AddComponent<RingGrow>();
            }
        }

        private sealed class Lift : MonoBehaviour
        {
            private float clock;
            private CanvasGroup group;
            private void Start() { group = GetComponent<CanvasGroup>(); }
            private void Update()
            {
                clock += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01((clock - 0.6f) / 2.2f);
                if (group != null) group.alpha = 1f - Mathf.SmoothStep(0f, 1f, t);
                transform.localScale = new Vector3(1f, 1f + 0.06f * t, 1f);
                if (t >= 1f) Destroy(gameObject);
            }
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
        /// <summary>A just-solved thing's ring widens from nothing, then settles (I212).</summary>
        private sealed class RingGrow : MonoBehaviour
        {
            private float clock;
            private void Start() { transform.localScale = Vector3.zero; }
            private void Update()
            {
                clock += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01((clock - 0.35f) / 0.9f);
                float s = t < 1f ? Mathf.SmoothStep(0f, 1.12f, t) : 1f;
                if (t >= 1f) { transform.localScale = Vector3.one; Destroy(this); return; }
                transform.localScale = new Vector3(s, s, 1f);
            }
        }

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
