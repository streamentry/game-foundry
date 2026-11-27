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
            public int Kind;             // I255: PuzzleKind + 1 drawn in the mark (0: a plain play arrow)
            public bool Together;        // I265: solved taking turns: two small discs, sage and honey
            public bool Crowned;         // I292: a replay solved at the deepest tier: a second, outer ring
            public float Grown;          // I300: drawn this much larger as replays deepen it (0 or 1: its own size)
            public int FlowerIndex;      // I338: a flower's catalogue index + 1 (0: not a flower or unknown)
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
                                          float apron = 0f, bool teaser = false, Sprite teaserPlot = null)
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
                // The next stretch wears the season it will bring (I252), as the stones ahead do.
                Sprite nextPlot = teaserPlot != null ? teaserPlot : plot;
                RawImage next = ReachImage(landRect, nextPlot, reaches % 2 == 1, "Next reach in mist");
                var nextRect = next.rectTransform;
                nextRect.anchorMin = new Vector2(0f, 0f);
                nextRect.anchorMax = new Vector2(1f, 0f);
                nextRect.pivot = new Vector2(0.5f, 0f);
                view.apronRect = nextRect;
                // Only the top of that reach shows, so trim the painting to match.
                Rect r = nextPlot.textureRect;
                float tu = r.x / nextPlot.texture.width, tv = r.y / nextPlot.texture.height;
                float tw = r.width / nextPlot.texture.width, th = r.height / nextPlot.texture.height, t = view.teaser;
                next.uvRect = reaches % 2 == 1
                    ? new Rect(tu, tv + th * t, tw, -th * t)          // mirrored: the painting's foot, upside down
                    : new Rect(tu, tv + th * (1f - t), tw, th * t);   // as painted: the painting's head
                // Both washes fade in from clear (I199): a solid straw wash drew a hard
                // line across the garden where the open river ends.
                // This season melts into the next over the same ground (I252): no hard line.
                if (nextPlot != plot)
                {
                    var melt = new GameObject("Season melting", typeof(RectTransform));
                    melt.transform.SetParent(nextRect, false);
                    var meltRect = (RectTransform)melt.transform;
                    meltRect.anchorMin = new Vector2(0f, 0.55f);
                    meltRect.anchorMax = Vector2.one;
                    meltRect.offsetMin = meltRect.offsetMax = Vector2.zero;
                    var fade = melt.AddComponent<FadeDownRawImage>();
                    fade.texture = plot.texture;
                    Rect uv = next.uvRect;
                    fade.uvRect = new Rect(uv.x, uv.y + uv.height * 0.55f, uv.width, uv.height * 0.45f);
                    fade.raycastTarget = false;
                }
                Wash(nextRect, new Color(0.84f, 0.74f, 0.50f, 0.5f), true);
                Wash(nextRect, new Color(0.97f, 0.95f, 0.89f, 0.82f), true);
            }
            // The day's light on the ground (I278): over the painting, under every thing, neighbour
            // and mark, so they keep their full contrast at night.
            if (Dark > 0.005f)
            {
                // Evening first darkens the ground (b295: a colour wash alone read as a muted day).
                var shade = new GameObject("Evening shade", typeof(RectTransform));
                shade.transform.SetParent(land.transform, false);
                var shadeRect = (RectTransform)shade.transform;
                shadeRect.anchorMin = Vector2.zero;
                shadeRect.anchorMax = Vector2.one;
                shadeRect.offsetMin = shadeRect.offsetMax = Vector2.zero;
                var dim = shade.AddComponent<Image>();
                dim.color = new Color(0.02f, 0.03f, 0.08f, Dark);
                dim.raycastTarget = false;
            }
            // I338: in winter, snow lies on the ground: a soft white veil over the painting,
            // under every thing, so the things stand out against it.
            // I354/I356: a rainy day greys and cools the light; a hot day warms it.
            if (WeatherNow == Weather.Rainy || WeatherNow == Weather.Hot)
            {
                var sky = new GameObject(WeatherNow == Weather.Rainy ? "Rainy light" : "Hot light", typeof(RectTransform));
                sky.transform.SetParent(land.transform, false);
                var skyRect = (RectTransform)sky.transform;
                skyRect.anchorMin = Vector2.zero;
                skyRect.anchorMax = Vector2.one;
                skyRect.offsetMin = skyRect.offsetMax = Vector2.zero;
                var wash = sky.AddComponent<Image>();
                // b314 tour: the hot day read as an ordinary summer one; warmer, and rain a touch greyer.
                wash.color = WeatherNow == Weather.Rainy ? new Color(0.45f, 0.52f, 0.62f, 0.22f) : new Color(1f, 0.78f, 0.42f, 0.20f);
                wash.raycastTarget = false;
            }
            if (SeasonLook.SnowLying(SeasonNow) > 0f)
            {
                var snow = new GameObject("Snow lying", typeof(RectTransform));
                snow.transform.SetParent(land.transform, false);
                var snowRect = (RectTransform)snow.transform;
                snowRect.anchorMin = Vector2.zero;
                snowRect.anchorMax = Vector2.one;
                snowRect.offsetMin = snowRect.offsetMax = Vector2.zero;
                var veil = snow.AddComponent<Image>();
                veil.color = new Color(0.97f, 0.98f, 1f, SeasonLook.SnowLying(SeasonNow) + (WeatherNow == Weather.Snowy ? 0.12f : 0f));
                veil.raycastTarget = false;
            }
            if (Tint.a > 0.005f)
            {
                var light = new GameObject("Day light", typeof(RectTransform));
                light.transform.SetParent(land.transform, false);
                var lightRect = (RectTransform)light.transform;
                lightRect.anchorMin = Vector2.zero;
                lightRect.anchorMax = Vector2.one;
                lightRect.offsetMin = lightRect.offsetMax = Vector2.zero;
                var wash = light.AddComponent<Image>();
                wash.color = Tint;
                wash.raycastTarget = false;
            }
            // Fireflies at dusk and night (I279), above everything, drifting in the view.
            if (Fireflies)
            {
                var flies = new GameObject("Fireflies", typeof(RectTransform));
                flies.transform.SetParent(host.transform, false);
                var fliesRect = (RectTransform)flies.transform;
                fliesRect.anchorMin = Vector2.zero;
                fliesRect.anchorMax = Vector2.one;
                fliesRect.offsetMin = fliesRect.offsetMax = Vector2.zero;
                flies.AddComponent<FireflyDrift>().Moving = FirefliesMove;
            }
            // I354–I356: the day's weather over the garden (motion only): rain, gusts or heat.
            if (FirefliesMove && (WeatherNow == Weather.Rainy || WeatherNow == Weather.Windy || WeatherNow == Weather.Hot))
            {
                var weather = new GameObject("Weather " + WeatherNow, typeof(RectTransform));
                weather.transform.SetParent(host.transform, false);
                var weatherRect = (RectTransform)weather.transform;
                weatherRect.anchorMin = Vector2.zero;
                weatherRect.anchorMax = Vector2.one;
                weatherRect.offsetMin = weatherRect.offsetMax = Vector2.zero;
                weather.AddComponent<WeatherLayer>().Kind = WeatherNow;
            }
            // I364: a rainbow on the first fair day after rain, soft, over the top of the view.
            if (RainbowNow)
            {
                var bow = new GameObject("Rainbow", typeof(RectTransform), typeof(RectMask2D));
                bow.transform.SetParent(host.transform, false);
                var bowRect = (RectTransform)bow.transform;
                bowRect.anchorMin = new Vector2(0f, 1f);
                bowRect.anchorMax = Vector2.one;
                bowRect.pivot = new Vector2(0.5f, 1f);
                bowRect.sizeDelta = new Vector2(0f, 260f);
                bowRect.anchoredPosition = new Vector2(0f, -40f);
                Color[] bands = {
                    new Color(0.95f, 0.45f, 0.42f), new Color(0.98f, 0.70f, 0.38f), new Color(0.98f, 0.90f, 0.45f),
                    new Color(0.55f, 0.82f, 0.55f), new Color(0.48f, 0.68f, 0.92f), new Color(0.68f, 0.55f, 0.88f) };
                for (int b = 0; b < bands.Length; b++)
                {
                    var arc = new GameObject("Band", typeof(RectTransform));
                    arc.transform.SetParent(bow.transform, false);
                    var arcRect = (RectTransform)arc.transform;
                    arcRect.anchorMin = arcRect.anchorMax = new Vector2(0.5f, 0f);
                    float size = 620f - b * 18f;
                    arcRect.sizeDelta = new Vector2(size, size);
                    arcRect.anchoredPosition = new Vector2(0f, -size * 0.18f);
                    var arcImage = arc.AddComponent<Image>();
                    arcImage.sprite = MosseloraArt.Halo();
                    arcImage.color = new Color(bands[b].r, bands[b].g, bands[b].b, 0.30f);
                    arcImage.raycastTarget = false;
                }
            }
            // I341: summer by day: dappled sunlight drifts slowly over the garden.
            if (SeasonNow == Season.Summer && Dark < 0.005f && FirefliesMove)
            {
                var dapple = new GameObject("Summer sun", typeof(RectTransform));
                dapple.transform.SetParent(host.transform, false);
                var dappleRect = (RectTransform)dapple.transform;
                dappleRect.anchorMin = Vector2.zero;
                dappleRect.anchorMax = Vector2.one;
                dappleRect.offsetMin = dappleRect.offsetMax = Vector2.zero;
                dapple.AddComponent<SunDapple>();
            }
            // I317: through the festival spring, a garland of paper lanterns hangs across the view.
            if (FestivalNow)
            {
                var garland = new GameObject("Festival lanterns", typeof(RectTransform));
                garland.transform.SetParent(host.transform, false);
                var garlandRect = (RectTransform)garland.transform;
                garlandRect.anchorMin = new Vector2(0f, 1f);
                garlandRect.anchorMax = Vector2.one;
                garlandRect.pivot = new Vector2(0.5f, 1f);
                garlandRect.sizeDelta = new Vector2(0f, 110f);
                garlandRect.anchoredPosition = new Vector2(0f, -86f);
                garland.AddComponent<FestivalGarland>().Lit = LampLit;
            }
            // I326: a pair of birds glides over now and then, once trees are tended.
            if (BirdsOut && FirefliesMove)
            {
                var sky = new GameObject("Birds passing", typeof(RectTransform));
                sky.transform.SetParent(host.transform, false);
                var skyRect = (RectTransform)sky.transform;
                skyRect.anchorMin = Vector2.zero;
                skyRect.anchorMax = Vector2.one;
                skyRect.offsetMin = skyRect.offsetMax = Vector2.zero;
                sky.AddComponent<BirdsPass>();
            }
            // I295: the season's own drift over home by day (petals, leaves, snow); motion only.
            if (DriftNow != Drift.None && FirefliesMove)
            {
                var drift = new GameObject("Season drift", typeof(RectTransform));
                drift.transform.SetParent(host.transform, false);
                var driftRect = (RectTransform)drift.transform;
                driftRect.anchorMin = Vector2.zero;
                driftRect.anchorMax = Vector2.one;
                driftRect.offsetMin = driftRect.offsetMax = Vector2.zero;
                drift.AddComponent<SeasonDrift>().Kind = DriftNow;
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
            view.lanterns = Mathf.Max(0, LanternsNext); // review fix: known before the first layout
            view.meadow = MeadowTufts; // I363: home's alone; kept for every re-layout (zoom, lookout)
            MeadowTufts = null;
            view.pairLinks = PairLinksNext; // I371
            PairLinksNext = null;
            view.pairSpots = PairSpotsNext; // I372
            PairSpotsNext = null;
            view.creatureCells = new[] { HeronAtNext, HedgehogAtNext, OwlAtNext, KingfisherAtNext }; // I374, I382
            HeronAtNext = HedgehogAtNext = OwlAtNext = KingfisherAtNext = -1;
            LanternsNext = 0;
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
        private int lanterns; // I310
        private int[] meadow;  // I363
        private int[] pairLinks; // I371
        private int[] pairSpots; // I372
        private int[] creatureCells; // I374: heron, hedgehog, owl
        public static int LanternsNext; // set by the app just before building home's map

        /// <summary>I310: how many lanterns family have left; drawn by the cottage.</summary>
        public void SetLanterns(int count)
        {
            if (count == lanterns) return;
            lanterns = Mathf.Max(0, count);
            Layout();
        }

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
        private float beforeLookout = -1f;

        /// <summary>
        /// I319 (ideas 23 and 15, after the lookouts of hiking games and always knowing your way
        /// home): the whole river at once, cottage at the top, the newest stretch at the foot.
        /// The map's own width rule fits the river's height when asked for the least width.
        /// </summary>
        public void LookOut(bool on)
        {
            if (on == (beforeLookout > 0f)) return;
            if (on) { beforeLookout = widthScale; widthScale = 0.01f; }
            else { widthScale = beforeLookout; beforeLookout = -1f; }
            keep = new Vector2(-1f, -1f);
            growNewest = false;
            Layout();
            if (on && land != null)
            {
                // The whole river, centred in the view (b307: it sat to one side).
                if (scroll != null) scroll.StopMovement();
                land.anchoredPosition = Vector2.zero;
            }
        }
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
            if (beforeLookout > 0f) return; // review fix: no pinching while looking out over the whole river
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
            // I296: from dusk to dawn, lamplight spills from each cottage door (painted top right
            // of every reach): two soft warm discs, over the evening shade.
            for (int reach = 0; LampLit && reach < reaches; reach++)
            {
                // Every other reach is the painting mirrored top to bottom (I192), door and all.
                float down = reach % 2 == 0 ? reach * reachHeight + reachHeight * 0.148f : (reach + 1) * reachHeight - reachHeight * 0.148f;
                Vector2 door = new Vector2(width * 0.858f, -down);
                float[] sizes = { 3.2f, 1.6f };
                float[] alphas = { 0.55f, 0.85f };
                for (int g = 0; g < sizes.Length; g++)
                {
                    var lamp = new GameObject(g == 0 ? "Lamplight" : "Lamp", typeof(RectTransform));
                    lamp.transform.SetParent(land, false);
                    var lampRect = (RectTransform)lamp.transform;
                    lampRect.anchorMin = lampRect.anchorMax = new Vector2(0f, 1f);
                    lampRect.sizeDelta = new Vector2(cellW * sizes[g], cellH * sizes[g] * 0.7f);
                    lampRect.anchoredPosition = door;
                    var lampImage = lamp.AddComponent<Image>();
                    lampImage.sprite = MosseloraArt.SoftGlow();
                    lampImage.color = new Color(1f, 0.80f, 0.42f, alphas[g]);
                    lampImage.raycastTarget = false;
                    nodes.Add(lampRect);
                }
            }
            // I335: the garden cat, asleep beside the newest tended thing, or by the door after dusk.
            if (CatBy != -1)
            {
                Vector2 catAt = new Vector2(-1f, 1f);
                if (CatBy == -2) catAt = new Vector2(width * 0.80f, -reachHeight * 0.175f);
                else foreach (Placed item in items)
                    if (item.Index == CatBy && item.Cell >= 0 && item.Cell < map.CellCount)
                    {
                        int column = item.Cell % map.Columns, row = item.Cell / map.Columns;
                        catAt = new Vector2((column + 0.5f) * cellW + cellW * (column < map.Columns / 2 ? 0.75f : -0.75f),
                            -(row + 0.85f) * cellH);
                    }
                if (catAt.x >= 0f)
                {
                    var cat = new GameObject("The garden cat, asleep", typeof(RectTransform));
                    cat.transform.SetParent(land, false);
                    var catRect = (RectTransform)cat.transform;
                    catRect.anchorMin = catRect.anchorMax = new Vector2(0f, 1f);
                    catRect.sizeDelta = new Vector2(40f, 24f);
                    catRect.anchoredPosition = catAt;
                    cat.AddComponent<SleepingCat>();
                    nodes.Add(catRect);
                }
            }
            // I338: snow on every cottage roof in winter (top right of each reach; mirrored ones too).
            for (int reach = 0; SeasonLook.SnowCaps(SeasonNow) && reach < reaches; reach++)
            {
                float roofDown = reach % 2 == 0 ? reach * reachHeight + reachHeight * 0.075f : (reach + 1) * reachHeight - reachHeight * 0.075f;
                var roof = new GameObject("Snow on the roof", typeof(RectTransform));
                roof.transform.SetParent(land, false);
                var roofRect = (RectTransform)roof.transform;
                roofRect.anchorMin = roofRect.anchorMax = new Vector2(0f, 1f);
                roofRect.sizeDelta = new Vector2(width * 0.2f, reachHeight * 0.085f);
                roofRect.anchoredPosition = new Vector2(width * 0.858f, -roofDown);
                var roofImage = roof.AddComponent<Image>();
                roofImage.sprite = MosseloraArt.SoftGlow();
                roofImage.color = new Color(1f, 1f, 1f, 0.85f);
                roofImage.raycastTarget = false;
                nodes.Add(roofRect);
            }
            // I342: the river through the year, drawn on the real water cells (a few, never all).
            RiverLook riverLook = SeasonLook.River(SeasonNow);
            if (riverLook != RiverLook.Running)
            {
                int drawn = 0;
                for (int c = 0; c < map.CellCount && drawn < 70; c++)
                {
                    if (map.At(c) != GardenMap.Water || (c * 7) % (riverLook == RiverLook.IceEdged ? 2 : 3) != 0) continue;
                    int col = c % map.Columns, row = c / map.Columns;
                    var mark = new GameObject(riverLook.ToString(), typeof(RectTransform));
                    mark.transform.SetParent(land, false);
                    var markRect = (RectTransform)mark.transform;
                    markRect.anchorMin = markRect.anchorMax = new Vector2(0f, 1f);
                    markRect.anchoredPosition = new Vector2((col + 0.5f) * cellW, -(row + 0.5f) * cellH);
                    var markImage = mark.AddComponent<Image>();
                    markImage.raycastTarget = false;
                    if (riverLook == RiverLook.IceEdged)
                    {
                        markRect.sizeDelta = new Vector2(cellW * 1.3f, cellH * 1.1f);
                        markImage.sprite = MosseloraArt.SoftGlow();
                        markImage.color = new Color(0.92f, 0.96f, 1f, 0.55f);
                    }
                    else if (riverLook == RiverLook.LeavesAfloat)
                    {
                        markRect.sizeDelta = new Vector2(cellW * 0.22f, cellW * 0.13f);
                        markRect.localRotation = Quaternion.Euler(0f, 0f, (c * 37) % 180);
                        markImage.sprite = MosseloraArt.Rounded(6);
                        markImage.color = (c % 2 == 0) ? new Color(0.88f, 0.50f, 0.18f, 0.95f) : new Color(0.80f, 0.36f, 0.14f, 0.95f);
                        if (FirefliesMove) mark.AddComponent<Afloat>().Phase = c;
                    }
                    else
                    {
                        markRect.sizeDelta = new Vector2(cellW * 0.16f, cellW * 0.16f);
                        markImage.sprite = MosseloraArt.SoftGlow();
                        markImage.color = new Color(1f, 1f, 0.95f, 0.9f);
                        if (FirefliesMove) mark.AddComponent<Twinkle>().Phase = c;
                    }
                    nodes.Add(markRect);
                    drawn++;
                }
            }
            // I360: a painted plank at the head of each stretch, with its name.
            Sprite plank = SignFont != null ? MosseloraArt.Load(MosseloraArt.StreamSign) : null;
            for (int reach = 0; plank != null && reach < reaches; reach++)
            {
                var sign = new GameObject("River name " + RiverNames.For(reach), typeof(RectTransform));
                sign.transform.SetParent(land, false);
                var signRect = (RectTransform)sign.transform;
                signRect.anchorMin = signRect.anchorMax = new Vector2(0f, 1f);
                signRect.pivot = new Vector2(0.5f, 1f);
                float signW = Mathf.Clamp(width * 0.34f, 120f, 170f);
                signRect.sizeDelta = new Vector2(signW, signW * 0.28f);
                signRect.anchoredPosition = new Vector2(width * 0.58f, -(reach * reachHeight + reachHeight * 0.012f));
                var plankImage = sign.AddComponent<Image>();
                plankImage.sprite = plank;
                plankImage.raycastTarget = false;
                var label = new GameObject("Name", typeof(RectTransform)).AddComponent<Text>();
                label.transform.SetParent(sign.transform, false);
                label.rectTransform.anchorMin = Vector2.zero;
                label.rectTransform.anchorMax = Vector2.one;
                label.rectTransform.offsetMin = new Vector2(8f, 2f);
                label.rectTransform.offsetMax = new Vector2(-8f, -2f);
                label.font = SignFont;
                label.fontSize = 15;
                label.fontStyle = FontStyle.Bold;
                label.alignment = TextAnchor.MiddleCenter;
                label.color = MosseloraTheme.EarthInk;
                label.resizeTextForBestFit = true;
                label.resizeTextMinSize = 10;
                label.resizeTextMaxSize = 15;
                label.raycastTarget = false;
                label.text = RiverNames.For(reach);
                nodes.Add(signRect);
            }
            // I310 (idea 43, leaving a light for another): lanterns family left stand along the
            // path below your cottage door, the newest nearest; lit after dusk.
            for (int l = 0; l < Mathf.Min(lanterns, 3); l++)
            {
                var lantern = new GameObject("A lantern left by family", typeof(RectTransform));
                lantern.transform.SetParent(land, false);
                var lanternRect = (RectTransform)lantern.transform;
                lanternRect.anchorMin = lanternRect.anchorMax = new Vector2(0f, 1f);
                lanternRect.pivot = new Vector2(0.5f, 0f);
                lanternRect.sizeDelta = new Vector2(cellW * 0.8f, cellH * 0.8f);
                lanternRect.anchoredPosition = new Vector2(width * (0.80f - l * 0.07f), -reachHeight * (0.20f + l * 0.035f));
                if (LampLit)
                {
                    var halo = new GameObject("Lantern light", typeof(RectTransform));
                    halo.transform.SetParent(lantern.transform, false);
                    var haloRect = (RectTransform)halo.transform;
                    haloRect.anchorMin = haloRect.anchorMax = new Vector2(0.5f, 0.6f);
                    haloRect.sizeDelta = new Vector2(cellW * 1.4f, cellW * 1.4f);
                    var haloImage = halo.AddComponent<Image>();
                    haloImage.sprite = MosseloraArt.SoftGlow();
                    haloImage.color = new Color(1f, 0.82f, 0.45f, 0.6f);
                    haloImage.raycastTarget = false;
                }
                var art = MosseloraArt.Load(FamilySheet.LanternArt);
                var lanternImage = new GameObject("Lantern", typeof(RectTransform)).AddComponent<Image>();
                lanternImage.transform.SetParent(lantern.transform, false);
                var artRect = lanternImage.rectTransform;
                artRect.anchorMin = Vector2.zero;
                artRect.anchorMax = Vector2.one;
                artRect.offsetMin = artRect.offsetMax = Vector2.zero;
                lanternImage.sprite = art;
                lanternImage.preserveAspect = true;
                lanternImage.raycastTarget = false;
                nodes.Add(lanternRect);
            }
            // I363: wild flowers along the banks, small and behind the things (home's map only).
            int[] tuftCells = meadow;
            for (int t = 0; tuftCells != null && t < tuftCells.Length; t++)
            {
                int cell = tuftCells[t];
                if (cell < 0 || cell >= map.CellCount) continue;
                Sprite wild = MosseloraArt.Load("flower-" + ((cell * 7 + 3) % 50).ToString("00"));
                if (wild == null) continue;
                var tuft = new GameObject("Wild flowers", typeof(RectTransform));
                tuft.transform.SetParent(land, false);
                var tuftRect = (RectTransform)tuft.transform;
                tuftRect.anchorMin = tuftRect.anchorMax = new Vector2(0f, 1f);
                tuftRect.pivot = new Vector2(0.5f, 0.1f);
                float th = cellH * 0.55f;
                tuftRect.sizeDelta = new Vector2(th * wild.rect.width / Mathf.Max(1f, wild.rect.height), th);
                int tc = cell % map.Columns, tr = cell / map.Columns;
                tuftRect.anchoredPosition = new Vector2((tc + 0.5f + ((cell * 13) % 5 - 2) * 0.08f) * cellW, -(tr + 0.8f) * cellH);
                var tuftImage = tuft.AddComponent<Image>();
                tuftImage.sprite = wild;
                tuftImage.preserveAspect = true;
                tuftImage.raycastTarget = false;
                ThingLook tuftLook = SeasonLook.For(SeasonNow, VillageChoice.Flowers, (cell * 7 + 3) % 50);
                tuftImage.color = tuftLook == ThingLook.Resting ? new Color(0.72f, 0.78f, 0.70f, 0.8f) : Color.white;
                nodes.Add(tuftRect);
            }
            // I371: a small sprig between good neighbours (I368), so the pairing shows where it is.
            Sprite sprig = pairLinks != null ? MosseloraArt.Load(MosseloraArt.Sprig) : null;
            for (int l = 0; sprig != null && l + 1 < pairLinks.Length; l += 2)
            {
                int a = pairLinks[l], b = pairLinks[l + 1];
                if (a < 0 || b < 0 || a >= map.CellCount || b >= map.CellCount) continue;
                Vector2 pa = new Vector2((a % map.Columns + 0.5f) * cellW, -(a / map.Columns + 0.85f) * cellH);
                Vector2 pb = new Vector2((b % map.Columns + 0.5f) * cellW, -(b / map.Columns + 0.85f) * cellH);
                var link = new GameObject("Good neighbours", typeof(RectTransform));
                link.transform.SetParent(land, false);
                var linkRect = (RectTransform)link.transform;
                linkRect.anchorMin = linkRect.anchorMax = new Vector2(0f, 1f);
                float ls = Mathf.Max(16f, cellW * 0.32f);
                linkRect.sizeDelta = new Vector2(ls, ls);
                linkRect.anchoredPosition = (pa + pb) * 0.5f;
                var linkImage = link.AddComponent<Image>();
                linkImage.sprite = sprig;
                linkImage.preserveAspect = true;
                linkImage.color = new Color(0.36f, 0.62f, 0.38f, 0.9f);
                linkImage.raycastTarget = false;
                nodes.Add(linkRect);
            }
            // I374: the visitors good neighbours bring.
            for (int k = 0; creatureCells != null && k < creatureCells.Length; k++)
            {
                int cell = creatureCells[k];
                if (cell < 0 || cell >= map.CellCount) continue;
                var creature = new GameObject(k == 0 ? "A heron, fishing" : k == 1 ? "A hedgehog, snuffling"
                    : k == 2 ? "An owl, watching" : "A kingfisher, perched",
                    typeof(RectTransform));
                creature.transform.SetParent(land, false);
                var creatureRect = (RectTransform)creature.transform;
                creatureRect.anchorMin = creatureRect.anchorMax = new Vector2(0f, 1f);
                creatureRect.sizeDelta = new Vector2(40f, 40f);
                float lift = k == 2 ? 0.25f : k == 1 ? 0.95f : 0.6f; // the owl up in the tree, the hedgehog at its foot
                float side = k == 1 ? 0.35f : k == 3 ? -0.2f : 0f;
                creatureRect.anchoredPosition = new Vector2((cell % map.Columns + 0.5f + side) * cellW, -(cell / map.Columns + lift) * cellH);
                creature.AddComponent<GardenCreature>().Kind = k;
                nodes.Add(creatureRect);
            }
            // I372: where the thing in hand would make a new good neighbour, a soft green glow.
            for (int sp = 0; pairSpots != null && sp < pairSpots.Length; sp++)
            {
                int cell = pairSpots[sp];
                if (cell < 0 || cell >= map.CellCount) continue;
                var spot = new GameObject("Would make good neighbours", typeof(RectTransform));
                spot.transform.SetParent(land, false);
                var spotRect = (RectTransform)spot.transform;
                spotRect.anchorMin = spotRect.anchorMax = new Vector2(0f, 1f);
                spotRect.sizeDelta = new Vector2(cellW * 0.9f, cellH * 0.6f);
                spotRect.anchoredPosition = new Vector2((cell % map.Columns + 0.5f) * cellW, -(cell / map.Columns + 0.7f) * cellH);
                var spotImage = spot.AddComponent<Image>();
                spotImage.sprite = MosseloraArt.SoftGlow();
                spotImage.color = new Color(0.55f, 0.85f, 0.50f, 0.55f);
                spotImage.raycastTarget = false;
                if (FirefliesMove) Breathe.Add(spotImage);
                nodes.Add(spotRect);
            }
            // I366: dew glints on the grass in the morning; frost in winter.
            if (DewNow != Dew.None)
            {
                var rng = new System.Random(366);
                for (int g = 0; g < 36; g++)
                {
                    int cell = rng.Next(map.CellCount);
                    if (!map.IsLand(cell)) continue;
                    var glint = new GameObject(DewNow == Dew.Frost ? "Frost" : "Dew", typeof(RectTransform));
                    glint.transform.SetParent(land, false);
                    var glintRect = (RectTransform)glint.transform;
                    glintRect.anchorMin = glintRect.anchorMax = new Vector2(0f, 1f);
                    float gd = DewNow == Dew.Frost ? 14f : 9f;
                    glintRect.sizeDelta = new Vector2(gd, gd);
                    glintRect.anchoredPosition = new Vector2(((cell % map.Columns) + (float)rng.NextDouble()) * cellW,
                        -((cell / map.Columns) + (float)rng.NextDouble()) * cellH);
                    var glintImage = glint.AddComponent<Image>();
                    glintImage.sprite = MosseloraArt.SoftGlow();
                    glintImage.color = DewNow == Dew.Frost ? new Color(1f, 1f, 1f, 0.8f) : new Color(0.85f, 0.95f, 1f, 0.9f);
                    glintImage.raycastTarget = false;
                    if (FirefliesMove && DewNow == Dew.Dew) glint.AddComponent<Twinkle>().Phase = g;
                    nodes.Add(glintRect);
                }
            }
            Vector2 focus = Vector2.zero;
            bool focused = false;
            int beesPlaced = 0;
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
                    if (item.Crowned)
                    {
                        // Promoted, as a far-travelled piece is: a wider, deeper gold ring around the first.
                        var crown = new GameObject("Crowned", typeof(RectTransform));
                        crown.transform.SetParent(land, false);
                        var crownRect = (RectTransform)crown.transform;
                        crownRect.anchorMin = crownRect.anchorMax = new Vector2(0f, 1f);
                        crownRect.sizeDelta = new Vector2(cellW * 1.85f, cellH * 0.9f);
                        crownRect.anchoredPosition = glowRect.anchoredPosition;
                        var crownImage = crown.AddComponent<Image>();
                        crownImage.sprite = MosseloraArt.Ring(16);
                        crownImage.type = Image.Type.Sliced;
                        crownImage.color = new Color(0.86f, 0.62f, 0.16f, 0.9f);
                        crownImage.raycastTarget = false;
                        nodes.Add(crownRect);
                    }
                }
                var node = new GameObject("On the map", typeof(RectTransform));
                node.transform.SetParent(land, false);
                var rect = (RectTransform)node.transform;
                rect.anchorMin = rect.anchorMax = new Vector2(0f, 1f);
                rect.pivot = new Vector2(0.5f, 0.08f);
                // I338–I340: the thing as the season has it: blossom, full, turning or resting.
                ThingLook look = SeasonLook.For(SeasonNow, item.Family, item.FlowerIndex - 1);
                bool restingFlower = look == ThingLook.Resting && item.Family == VillageChoice.Flowers;
                float tall = cellH * HeightInCells(item.Family) * (held ? 1.12f : 1f) * Mathf.Max(1f, item.Grown)
                    * (restingFlower ? 0.84f : 1f);
                float aspectArt = item.Art.rect.width / Mathf.Max(1f, item.Art.rect.height);
                rect.sizeDelta = new Vector2(tall * aspectArt, tall);
                rect.anchoredPosition = at;
                var image = node.AddComponent<Image>();
                image.sprite = item.Art;
                image.preserveAspect = true;
                image.raycastTarget = onItem != null;
                if (!held) DressForSeason(node.transform, image, look, item.Family, rect.sizeDelta);
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
                // I297 (idea 30, the wind of hand-drawn film): living things lean in the breeze,
                // each a little out of step with its neighbours. Built things stay put; still
                // with reduced motion.
                // I307 (idea 29, weather you notice): water just freed for a thing falls back on
                // it as a short, light shower. Only as the ring grows in, so only with motion.
                if (item.BloomNow && item.Kind == (int)PuzzleKind.Water + 1)
                {
                    var shower = new GameObject("A short shower", typeof(RectTransform));
                    shower.transform.SetParent(land, false);
                    var showerRect = (RectTransform)shower.transform;
                    showerRect.anchorMin = showerRect.anchorMax = new Vector2(0f, 1f);
                    showerRect.pivot = new Vector2(0.5f, 0f);
                    showerRect.sizeDelta = new Vector2(Mathf.Max(rect.sizeDelta.x * 1.3f, cellW * 1.6f), rect.sizeDelta.y * 1.4f);
                    showerRect.anchoredPosition = at;
                    shower.AddComponent<Shower>();
                    nodes.Add(showerRect);
                }
                if (!held && FirefliesMove && item.Family != VillageChoice.Bridge)
                    node.AddComponent<Sway>().Phase = item.Cell * 0.73f;
                // I325: bees at the flowers, when they come (three flowers grown) and are out.
                if (BeesOut && FirefliesMove && !held && item.Family == VillageChoice.Flowers && beesPlaced < 3)
                {
                    beesPlaced++;
                    var bee = new GameObject("A bee", typeof(RectTransform));
                    bee.transform.SetParent(land, false);
                    var beeRect = (RectTransform)bee.transform;
                    beeRect.anchorMin = beeRect.anchorMax = new Vector2(0f, 1f);
                    beeRect.sizeDelta = new Vector2(12f, 9f);
                    beeRect.anchoredPosition = at + new Vector2(0f, rect.sizeDelta.y * 0.6f);
                    var buzz = bee.AddComponent<BeeBuzz>();
                    buzz.Phase = item.Cell;
                    // I379: a flower with a tree beside it: its bee carries pollen over to the tree.
                    foreach (Placed other in items)
                    {
                        if (other.Family != VillageChoice.ShadeTree || other.Cell < 0 || other.Cell >= map.CellCount) continue;
                        int dx = other.Cell % map.Columns - item.Cell % map.Columns, dy = other.Cell / map.Columns - item.Cell / map.Columns;
                        if (Mathf.Abs(dx) > 1 || Mathf.Abs(dy) > 1 || (dx == 0 && dy == 0)) continue;
                        buzz.Toward = new Vector2(dx * cellW, -dy * cellH + cellH * 0.25f);
                        break;
                    }
                    nodes.Add(beeRect);
                }
                if (item.Together && !held)
                {
                    // Two players' colours, overlapping, at the other shoulder (I265).
                    var pair = new GameObject("Solved together", typeof(RectTransform));
                    pair.transform.SetParent(land, false);
                    var pairRect = (RectTransform)pair.transform;
                    pairRect.anchorMin = pairRect.anchorMax = new Vector2(0f, 1f);
                    float pd = Mathf.Clamp(cellW * 0.3f, 16f, 22f);
                    pairRect.sizeDelta = new Vector2(pd * 1.6f, pd);
                    pairRect.anchoredPosition = at + new Vector2(-rect.sizeDelta.x * 0.36f, rect.sizeDelta.y * 0.78f);
                    for (int p = 0; p < 2; p++)
                    {
                        var dot = new GameObject(p == 0 ? "Sage" : "Honey", typeof(RectTransform));
                        dot.transform.SetParent(pair.transform, false);
                        var dotRect = (RectTransform)dot.transform;
                        dotRect.sizeDelta = new Vector2(pd, pd);
                        dotRect.anchoredPosition = new Vector2((p - 0.5f) * pd * 0.6f, 0f);
                        var dotImage = dot.AddComponent<Image>();
                        dotImage.sprite = MosseloraArt.Rounded(12);
                        dotImage.color = p == 0 ? new Color(0.55f, 0.72f, 0.55f, 1f) : new Color(0.93f, 0.76f, 0.40f, 1f);
                        dotImage.raycastTarget = false;
                        MosseloraArt.Lift(dotImage, 1f, 0.2f);
                    }
                    nodes.Add(pairRect);
                }
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
                    if (item.Kind > 0) KindBadge(mark.transform, (PuzzleKind)(item.Kind - 1), d * 0.62f);
                    else if (arrow != null)
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
                if (SignFont != null) KeepOffSigns(standing, width, reachHeight, reaches); // I375
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
            // I361: the name of the stretch waiting beyond the mist, faint, above the drops.
            Transform oldName = apronRect.Find("Beyond the mist");
            if (oldName != null) Destroy(oldName.gameObject);
            if (SignFont != null)
            {
                var beyond = new GameObject("Beyond the mist", typeof(RectTransform)).AddComponent<Text>();
                beyond.transform.SetParent(apronRect, false);
                beyond.rectTransform.anchorMin = beyond.rectTransform.anchorMax = new Vector2(0.5f, 0.42f);
                beyond.rectTransform.sizeDelta = new Vector2(260f, 30f);
                beyond.rectTransform.anchoredPosition = new Vector2(0f, 38f);
                beyond.font = SignFont;
                beyond.fontSize = 17;
                beyond.fontStyle = FontStyle.Italic;
                beyond.alignment = TextAnchor.MiddleCenter;
                beyond.color = new Color(0.30f, 0.34f, 0.32f, 0.75f);
                beyond.raycastTarget = false;
                beyond.text = RiverNames.For(reaches);
            }
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

        // I255: which puzzle a thing holds, drawn small in white on the play mark, from
        // shapes the game already uses: choose today's puzzle at a glance.
        private static void KindBadge(Transform mark, PuzzleKind kind, float box)
        {
            var root = new GameObject("Puzzle kind " + kind, typeof(RectTransform));
            root.transform.SetParent(mark, false);
            var rect = (RectTransform)root.transform;
            rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = new Vector2(box, box);
            float u = box / 6f; // a sixth of the badge
            switch (kind)
            {
                case PuzzleKind.Water: Pic(root.transform, "glyph-waves", box); break;
                case PuzzleKind.StreamPath: Pic(root.transform, MosseloraArt.Droplet, box); break;
                case PuzzleKind.Sudoku: Pic(root.transform, "glyph-sprout", box); break;
                case PuzzleKind.Pairs: // two cards
                    Block(root.transform, -1.4f * u, 0f, 2.2f * u, 3.4f * u, 1f);
                    Block(root.transform, 1.4f * u, 0f, 2.2f * u, 3.4f * u, 0.6f);
                    break;
                case PuzzleKind.Mosaic: // four tiles, one lifted
                    Block(root.transform, -1.2f * u, 1.2f * u, 2f * u, 2f * u, 1f);
                    Block(root.transform, 1.2f * u, 1.2f * u, 2f * u, 2f * u, 1f);
                    Block(root.transform, -1.2f * u, -1.2f * u, 2f * u, 2f * u, 1f);
                    Block(root.transform, 1.6f * u, -1.6f * u, 2f * u, 2f * u, 0.6f);
                    break;
                case PuzzleKind.Nonogram: // a row planted, the rest empty
                    for (int r = -1; r <= 1; r++)
                        for (int c = -1; c <= 1; c++)
                            Block(root.transform, c * 1.9f * u, r * 1.9f * u, 1.5f * u, 1.5f * u, r == 0 ? 1f : 0.4f);
                    break;
                case PuzzleKind.Plots: // a field in three beds
                    Block(root.transform, -1.1f * u, 0f, 2.6f * u, 5f * u, 1f);
                    Block(root.transform, 1.6f * u, 1.3f * u, 2.2f * u, 2.4f * u, 0.75f);
                    Block(root.transform, 1.6f * u, -1.3f * u, 2.2f * u, 2.4f * u, 0.5f);
                    break;
                case PuzzleKind.Sowing: // beds of one, two and three seeds, filling toward the basket
                    for (int b = 0; b < 3; b++)
                        Block(root.transform, (b - 1) * 1.9f * u, (b - 1) * 0.9f * u - 0.4f * u, 1.5f * u, (1.4f + b * 0.9f) * u, 0.55f + b * 0.22f);
                    break;
                case PuzzleKind.Sums: // a plus: rows and columns that add up
                    Block(root.transform, 0f, 0f, 4.6f * u, 1.4f * u, 1f);
                    Block(root.transform, 0f, 0f, 1.4f * u, 4.6f * u, 1f);
                    break;
                case PuzzleKind.Pieces: // two pieces fitting together
                    Block(root.transform, -1f * u, 0.9f * u, 3.6f * u, 1.6f * u, 1f);
                    Block(root.transform, 0.9f * u, -0.9f * u, 3.6f * u, 1.6f * u, 0.6f);
                    break;
                case PuzzleKind.Doorstep: // four petals, mirrored about the centre
                    foreach (Vector2 d in new[] { new Vector2(-1f, 1f), new Vector2(1f, 1f), new Vector2(-1f, -1f), new Vector2(1f, -1f) })
                        Block(root.transform, d.x * 1.3f * u, d.y * 1.3f * u, 1.6f * u, 1.6f * u, 1f);
                    Block(root.transform, 0f, 0f, 1.2f * u, 1.2f * u, 0.6f);
                    break;
                case PuzzleKind.Mahjong: // a small terrace of tiles
                    Block(root.transform, -1.3f * u, -0.8f * u, 2.2f * u, 3f * u, 0.75f);
                    Block(root.transform, 1.3f * u, -0.8f * u, 2.2f * u, 3f * u, 0.75f);
                    Block(root.transform, 0f, 0.9f * u, 2.2f * u, 3f * u, 1f);
                    break;
            }
        }

        private static void Pic(Transform parent, string art, float size)
        {
            var node = new GameObject("Glyph", typeof(RectTransform));
            node.transform.SetParent(parent, false);
            ((RectTransform)node.transform).sizeDelta = new Vector2(size, size);
            var image = node.AddComponent<Image>();
            image.sprite = MosseloraArt.Load(art);
            image.preserveAspect = true;
            image.color = Color.white;
            image.raycastTarget = false;
        }

        private static void Block(Transform parent, float x, float y, float w, float h, float alpha)
        {
            var node = new GameObject("Shape", typeof(RectTransform));
            node.transform.SetParent(parent, false);
            var rect = (RectTransform)node.transform;
            rect.sizeDelta = new Vector2(w, h);
            rect.anchoredPosition = new Vector2(x, y);
            var image = node.AddComponent<Image>();
            image.sprite = MosseloraArt.Rounded(3);
            image.color = new Color(1f, 1f, 1f, alpha);
            image.raycastTarget = false;
        }

        // ---- I278/I279: light that follows the day (set by the app before maps are built).
        public static Color Tint = new Color(0f, 0f, 0f, 0f);
        public static float Dark;
        public static bool Fireflies;
        public static bool FirefliesMove = true;

        /// <summary>A dozen soft points of light, wandering slowly; still (but glowing) with reduced motion.</summary>
        private sealed class FireflyDrift : MonoBehaviour
        {
            public bool Moving = true;
            private RectTransform[] dots;
            private Vector2[] home;
            private float[] phase;
            private float clock;

            private void Start()
            {
                var rect = (RectTransform)transform;
                Vector2 size = rect.rect.size;
                var rng = new System.Random(1225); // the same evening every time, not random noise
                dots = new RectTransform[12];
                home = new Vector2[dots.Length];
                phase = new float[dots.Length];
                for (int i = 0; i < dots.Length; i++)
                {
                    var dot = new GameObject("Firefly", typeof(RectTransform));
                    dot.transform.SetParent(transform, false);
                    var dotRect = (RectTransform)dot.transform;
                    dotRect.anchorMin = dotRect.anchorMax = new Vector2(0f, 0f);
                    float d = 6f + (float)rng.NextDouble() * 4f;
                    dotRect.sizeDelta = new Vector2(d, d);
                    home[i] = new Vector2((float)rng.NextDouble() * size.x, (float)rng.NextDouble() * size.y);
                    dotRect.anchoredPosition = home[i];
                    phase[i] = (float)rng.NextDouble() * 6.28f;
                    var image = dot.AddComponent<Image>();
                    image.sprite = MosseloraArt.Rounded(8);
                    image.color = new Color(1f, 0.93f, 0.55f, 0.85f);
                    image.raycastTarget = false;
                    dots[i] = dotRect;
                }
            }

            private void Update()
            {
                if (dots == null || !Moving) return;
                clock += Time.unscaledDeltaTime;
                for (int i = 0; i < dots.Length; i++)
                {
                    float t = clock * 0.35f + phase[i];
                    dots[i].anchoredPosition = home[i] + new Vector2(Mathf.Sin(t) * 18f, Mathf.Sin(t * 1.3f + 1f) * 12f);
                    var image = dots[i].GetComponent<Image>();
                    image.color = new Color(1f, 0.93f, 0.55f, 0.35f + 0.5f * (0.5f + 0.5f * Mathf.Sin(t * 2.1f)));
                }
            }
        }

        public static Drift DriftNow = Drift.None;
        public static bool LampLit; // I296
        public static bool FestivalNow; // I317
        public static bool BeesOut, BirdsOut; // I325, I326
        public static int CatBy = -1;          // I335: a thing's index, -2 by the door, -1 none
        public static Season SeasonNow = Season.Summer; // I338: the garden's season, set before a build
        public static Weather WeatherNow = Weather.Fair; // I353: the day's weather, set before a build
        public static Font SignFont; // I360: the river-name signs' lettering

        // I375 (P15: Orin stood on "Kingfisher Run" and hid half of it): a neighbour standing on
        // a river's name plank steps aside, toward the roomier side, so the name always reads.
        private static void KeepOffSigns(List<RectTransform> standing, float width, float reachHeight, int reaches)
        {
            if (standing == null) return;
            float signW = Mathf.Clamp(width * 0.34f, 120f, 170f), signH = signW * 0.28f, signX = width * 0.58f;
            foreach (RectTransform person in standing)
            {
                if (person == null) continue;
                Vector2 at = person.anchoredPosition, size = person.sizeDelta;
                for (int reach = 0; reach < reaches; reach++)
                {
                    float signTop = -(reach * reachHeight + reachHeight * 0.012f), signBottom = signTop - signH;
                    bool overlapsY = at.y < signTop && at.y + size.y * 0.8f > signBottom;
                    bool overlapsX = Mathf.Abs(at.x - signX) < (signW + size.x * 0.6f) / 2f;
                    if (!overlapsY || !overlapsX) continue;
                    float left = signX - signW / 2f - size.x * 0.32f, right = signX + signW / 2f + size.x * 0.32f;
                    at.x = left >= size.x * 0.5f && (at.x < signX || right > width - size.x * 0.5f) ? left : right;
                    person.anchoredPosition = at;
                }
            }
        }
        public static int[] MeadowTufts; // I363: bank cells for wild flowers, set for home's map
        public static int[] PairLinksNext; // I371: good-neighbour cell pairs, set for home's map
        public static int[] PairSpotsNext; // I372: free cells that would make a new pairing
        public static int HeronAtNext = -1, HedgehogAtNext = -1, OwlAtNext = -1; // I374: cells, -1 none
        public static int KingfisherAtNext = -1; // I382
        public static bool RainbowNow;   // I364
        public static Dew DewNow;        // I366

        /// <summary>
        /// I354–I356: rain falling on a slant; faint gusts of wind sweeping across; or heat
        /// shimmering up in slow, clear bands. Calm in every case: weather, never a storm.
        /// </summary>
        private sealed class WeatherLayer : MonoBehaviour
        {
            public Weather Kind;
            private RectTransform[] bits;
            private Image[] images;
            private float[] x0, speed, phase;
            private float clock, wide, high;

            private void Start()
            {
                var rect = (RectTransform)transform;
                wide = Mathf.Max(1f, rect.rect.width);
                high = Mathf.Max(1f, rect.rect.height);
                var rng = new System.Random(353 + (int)Kind);
                int count = Kind == Weather.Rainy ? 70 : Kind == Weather.Windy ? 9 : 6;
                bits = new RectTransform[count];
                images = new Image[count];
                x0 = new float[count]; speed = new float[count]; phase = new float[count];
                for (int i = 0; i < count; i++)
                {
                    var node = new GameObject(Kind == Weather.Rainy ? "Rain" : Kind == Weather.Windy ? "Gust" : "Heat", typeof(RectTransform));
                    node.transform.SetParent(transform, false);
                    bits[i] = (RectTransform)node.transform;
                    bits[i].anchorMin = bits[i].anchorMax = new Vector2(0f, 1f);
                    images[i] = node.AddComponent<Image>();
                    images[i].raycastTarget = false;
                    if (Kind == Weather.Rainy)
                    {
                        bits[i].sizeDelta = new Vector2(2.6f, 18f + (float)rng.NextDouble() * 12f);
                        bits[i].localRotation = Quaternion.Euler(0f, 0f, -12f);
                        images[i].sprite = MosseloraArt.Rounded(2);
                        images[i].color = new Color(0.84f, 0.90f, 0.98f, 0.72f);
                        speed[i] = 520f + (float)rng.NextDouble() * 220f;
                    }
                    else if (Kind == Weather.Windy)
                    {
                        bits[i].sizeDelta = new Vector2(70f + (float)rng.NextDouble() * 90f, 2.5f);
                        images[i].sprite = MosseloraArt.Rounded(2);
                        speed[i] = 260f + (float)rng.NextDouble() * 160f;
                    }
                    else
                    {
                        bits[i].sizeDelta = new Vector2(wide * 0.9f, 26f);
                        images[i].sprite = MosseloraArt.SoftGlow();
                        speed[i] = 14f + (float)rng.NextDouble() * 10f;
                    }
                    x0[i] = (float)rng.NextDouble() * wide;
                    phase[i] = (float)rng.NextDouble() * high;
                }
            }

            private void Update()
            {
                if (bits == null) return;
                clock += Time.unscaledDeltaTime;
                for (int i = 0; i < bits.Length; i++)
                {
                    if (Kind == Weather.Rainy)
                    {
                        float y = (phase[i] + clock * speed[i]) % (high + 40f);
                        bits[i].anchoredPosition = new Vector2(x0[i] + y * 0.21f % wide, -y);
                    }
                    else if (Kind == Weather.Windy)
                    {
                        float x = (x0[i] + clock * speed[i]) % (wide + 200f) - 100f;
                        float life = Mathf.Repeat(clock * 0.4f + i * 0.37f, 1f);
                        bits[i].anchoredPosition = new Vector2(x, -phase[i]);
                        images[i].color = new Color(1f, 1f, 1f, Mathf.Sin(life * Mathf.PI) * 0.35f);
                    }
                    else
                    {
                        float y = (phase[i] + clock * speed[i]) % (high + 30f);
                        bits[i].anchoredPosition = new Vector2(wide * 0.5f + Mathf.Sin(clock * 0.8f + i) * 12f, -(high - y));
                        images[i].color = new Color(1f, 0.96f, 0.86f, 0.20f * Mathf.Sin(Mathf.Clamp01(y / high) * Mathf.PI));
                    }
                }
            }
        }

        /// <summary>A leaf bobbing on the stream (I342).</summary>
        private sealed class Afloat : MonoBehaviour
        {
            public float Phase;
            private Vector2 home;
            private float clock;
            private void Start() { home = ((RectTransform)transform).anchoredPosition; }
            private void Update()
            {
                clock += Time.unscaledDeltaTime;
                ((RectTransform)transform).anchoredPosition = home + new Vector2(Mathf.Sin(clock * 0.7f + Phase) * 4f, Mathf.Sin(clock * 1.1f + Phase) * 2f);
            }
        }

        /// <summary>Sun glinting on the water (I342).</summary>
        private sealed class Twinkle : MonoBehaviour
        {
            public float Phase;
            private Image image;
            private float clock;
            private void Start() { image = GetComponent<Image>(); }
            private void Update()
            {
                clock += Time.unscaledDeltaTime;
                if (image != null) image.color = new Color(1f, 1f, 0.95f, Mathf.Max(0f, Mathf.Sin(clock * 1.7f + Phase * 1.3f)) * 0.9f);
            }
        }

        /// <summary>Soft warm patches of sunlight wandering slowly, as through leaves (I341).</summary>
        private sealed class SunDapple : MonoBehaviour
        {
            private RectTransform[] spots;
            private Vector2[] home;
            private float clock;
            private void Start()
            {
                var rect = (RectTransform)transform;
                Vector2 size = rect.rect.size;
                var rng = new System.Random(41);
                spots = new RectTransform[7];
                home = new Vector2[spots.Length];
                for (int i = 0; i < spots.Length; i++)
                {
                    var spot = new GameObject("Sunlight", typeof(RectTransform));
                    spot.transform.SetParent(transform, false);
                    spots[i] = (RectTransform)spot.transform;
                    spots[i].anchorMin = spots[i].anchorMax = Vector2.zero;
                    float d = 90f + (float)rng.NextDouble() * 80f;
                    spots[i].sizeDelta = new Vector2(d, d * 0.7f);
                    home[i] = new Vector2((float)rng.NextDouble() * size.x, (float)rng.NextDouble() * size.y);
                    var image = spot.AddComponent<Image>();
                    image.sprite = MosseloraArt.SoftGlow();
                    image.color = new Color(1f, 0.95f, 0.70f, 0.16f);
                    image.raycastTarget = false;
                }
            }
            private void Update()
            {
                if (spots == null) return;
                clock += Time.unscaledDeltaTime;
                for (int i = 0; i < spots.Length; i++)
                    spots[i].anchoredPosition = home[i] + new Vector2(Mathf.Sin(clock * 0.12f + i) * 30f, Mathf.Sin(clock * 0.09f + i * 2f) * 20f);
            }
        }

        // I338–I340: snow caps in winter; warm turned leaves in autumn; a cool, quiet rest in
        // winter; blossom on spring trees. Drawn over the painting, never replacing it.
        private static void DressForSeason(Transform node, Image image, ThingLook look, VillageChoice family, Vector2 size)
        {
            if (look == ThingLook.Turning)
            {
                image.color = new Color(1f, 0.78f, 0.52f, 1f);
                // I339: fallen leaves gather at its foot.
                var rng = new System.Random(11);
                for (int l = 0; l < 6; l++)
                {
                    var leaf = new GameObject("Fallen leaf", typeof(RectTransform));
                    leaf.transform.SetParent(node, false);
                    var r = (RectTransform)leaf.transform;
                    r.anchorMin = r.anchorMax = new Vector2(-0.05f + (float)rng.NextDouble() * 1.1f, 0.02f + (float)rng.NextDouble() * 0.08f);
                    float d = Mathf.Max(5f, size.x * 0.09f);
                    r.sizeDelta = new Vector2(d, d * 0.6f);
                    r.localRotation = Quaternion.Euler(0f, 0f, (float)rng.NextDouble() * 180f);
                    var img = leaf.AddComponent<Image>();
                    img.sprite = MosseloraArt.Rounded(6);
                    img.color = l % 2 == 0 ? new Color(0.86f, 0.46f, 0.16f, 0.95f) : new Color(0.74f, 0.32f, 0.14f, 0.95f);
                    img.raycastTarget = false;
                }
            }
            else if (look == ThingLook.Resting)
                image.color = family == VillageChoice.Flowers ? new Color(0.70f, 0.76f, 0.68f, 1f) : new Color(0.80f, 0.84f, 0.90f, 1f);
            if (look == ThingLook.Blossom)
            {
                var rng = new System.Random(family == VillageChoice.ShadeTree ? 7 : 3);
                for (int b = 0; b < 9; b++)
                {
                    var dot = new GameObject("Blossom", typeof(RectTransform));
                    dot.transform.SetParent(node, false);
                    var r = (RectTransform)dot.transform;
                    r.anchorMin = r.anchorMax = new Vector2(0.2f + (float)rng.NextDouble() * 0.6f, 0.5f + (float)rng.NextDouble() * 0.42f);
                    float d = Mathf.Max(5f, size.x * 0.08f);
                    r.sizeDelta = new Vector2(d, d);
                    var img = dot.AddComponent<Image>();
                    img.sprite = MosseloraArt.Rounded(8);
                    img.color = b % 3 == 0 ? new Color(1f, 0.96f, 0.97f, 0.95f) : new Color(0.98f, 0.74f, 0.82f, 0.95f);
                    img.raycastTarget = false;
                }
            }
            if (SeasonLook.SnowCaps(SeasonNow))
            {
                var cap = new GameObject("Snow cap", typeof(RectTransform));
                cap.transform.SetParent(node, false);
                var r = (RectTransform)cap.transform;
                // b311: a hard pill floated over wide things; a soft mound hugs the top instead.
                r.anchorMin = new Vector2(0.16f, 0.70f);
                r.anchorMax = new Vector2(0.84f, 1.0f);
                r.offsetMin = r.offsetMax = Vector2.zero;
                var img = cap.AddComponent<Image>();
                img.sprite = MosseloraArt.SoftGlow();
                img.color = new Color(1f, 1f, 1f, 0.95f);
                img.raycastTarget = false;
            }
        }

        /// <summary>A small cat asleep, curled: body, head, ears and tail, breathing slowly (I335).</summary>
        private sealed class SleepingCat : MonoBehaviour
        {
            private RectTransform body;
            private float clock;

            private void Start()
            {
                Color fur = new Color(0.86f, 0.60f, 0.36f, 1f), dark = new Color(0.62f, 0.40f, 0.22f, 1f);
                Part("Tail", new Vector2(18f, 5f), new Vector2(10f, -4f), 20f, dark);
                body = Part("Body", new Vector2(28f, 15f), Vector2.zero, 0f, fur);
                Part("Head", new Vector2(13f, 12f), new Vector2(-12f, 3f), 0f, fur);
                Part("Ear", new Vector2(5f, 5f), new Vector2(-16f, 9f), 45f, dark);
                Part("Ear", new Vector2(5f, 5f), new Vector2(-9f, 9f), 45f, dark);
                Part("Stripe", new Vector2(3f, 11f), new Vector2(2f, 1f), 0f, dark);
            }

            private RectTransform Part(string name, Vector2 size, Vector2 at, float angle, Color colour)
            {
                var node = new GameObject(name, typeof(RectTransform));
                node.transform.SetParent(transform, false);
                var rect = (RectTransform)node.transform;
                rect.sizeDelta = size;
                rect.anchoredPosition = at;
                rect.localRotation = Quaternion.Euler(0f, 0f, angle);
                var image = node.AddComponent<Image>();
                image.sprite = MosseloraArt.Rounded(8);
                image.color = colour;
                image.raycastTarget = false;
                return rect;
            }

            private void Update()
            {
                if (body == null || !FirefliesMove) return;
                clock += Time.unscaledDeltaTime;
                body.localScale = new Vector3(1f, 1f + 0.05f * Mathf.Sin(clock * 1.6f), 1f); // asleep, breathing
            }
        }

        /// <summary>A small bee: a striped body and two pale wings, buzzing in loops (I325).</summary>
        private sealed class BeeBuzz : MonoBehaviour
        {
            public float Phase;
            public Vector2 Toward; // I379: offset to a tree beside its flower; zero for none
            private Vector2 home;
            private float clock;
            private RectTransform wings;

            private void Start()
            {
                var rect = (RectTransform)transform;
                home = rect.anchoredPosition;
                Part("Wings", new Vector2(10f, 6f), new Vector2(0f, 4f), new Color(1f, 1f, 1f, 0.75f), out wings);
                Part("Body", new Vector2(11f, 8f), Vector2.zero, new Color(0.98f, 0.78f, 0.18f, 1f), out _);
                Part("Stripe", new Vector2(2.5f, 8f), Vector2.zero, new Color(0.22f, 0.17f, 0.1f, 1f), out _);
            }

            private void Part(string name, Vector2 size, Vector2 at, Color colour, out RectTransform made)
            {
                var node = new GameObject(name, typeof(RectTransform));
                node.transform.SetParent(transform, false);
                made = (RectTransform)node.transform;
                made.sizeDelta = size;
                made.anchoredPosition = at;
                var image = node.AddComponent<Image>();
                image.sprite = MosseloraArt.Rounded(4);
                image.color = colour;
                image.raycastTarget = false;
            }

            private void Update()
            {
                clock += Time.unscaledDeltaTime;
                float t = clock * 1.6f + Phase;
                // I379: to and fro between the flower and the tree beside it, lingering at each.
                float trip = Toward == Vector2.zero ? 0f : Mathf.SmoothStep(0f, 1f, Mathf.PingPong(clock * 0.18f + Phase * 0.1f, 1.4f) - 0.2f);
                Vector2 loop = new Vector2(Mathf.Sin(t) * 22f, Mathf.Sin(t * 2.3f) * 9f) * (Toward == Vector2.zero ? 1f : 0.6f);
                ((RectTransform)transform).anchoredPosition = home + Toward * trip + loop;
                if (wings != null) wings.localScale = new Vector3(1f, 0.5f + 0.5f * Mathf.Abs(Mathf.Sin(clock * 40f)), 1f);
            }
        }

        /// <summary>Two small birds crossing high over the garden, wings beating then gliding (I326).</summary>
        private sealed class BirdsPass : MonoBehaviour
        {
            private RectTransform[] birds;
            private RectTransform[][] wings;
            private float clock, wait = 3f;

            private void Start()
            {
                birds = new RectTransform[2];
                wings = new RectTransform[2][];
                for (int b = 0; b < 2; b++)
                {
                    var bird = new GameObject("A bird", typeof(RectTransform));
                    bird.transform.SetParent(transform, false);
                    birds[b] = (RectTransform)bird.transform;
                    birds[b].anchorMin = birds[b].anchorMax = new Vector2(0f, 1f);
                    birds[b].sizeDelta = new Vector2(18f, 8f);
                    wings[b] = new RectTransform[2];
                    for (int side = 0; side < 2; side++)
                    {
                        var wing = new GameObject(side == 0 ? "Left wing" : "Right wing", typeof(RectTransform));
                        wing.transform.SetParent(bird.transform, false);
                        wings[b][side] = (RectTransform)wing.transform;
                        wings[b][side].sizeDelta = new Vector2(10f, 2.6f);
                        wings[b][side].pivot = new Vector2(side == 0 ? 1f : 0f, 0.5f);
                        var image = wing.AddComponent<Image>();
                        image.sprite = MosseloraArt.Rounded(2);
                        image.color = new Color(0.2f, 0.18f, 0.16f, 0.85f);
                        image.raycastTarget = false;
                    }
                    bird.SetActive(false);
                }
            }

            private void Update()
            {
                if (birds == null) return;
                clock += Time.unscaledDeltaTime;
                var rect = (RectTransform)transform;
                float wide = Mathf.Max(1f, rect.rect.width), across = 6f;
                float t = clock - wait;
                bool flying = t >= 0f && t < across;
                for (int b = 0; b < 2; b++)
                {
                    birds[b].gameObject.SetActive(flying);
                    if (!flying) continue;
                    float k = t / across;
                    birds[b].anchoredPosition = new Vector2(-30f + (wide + 60f) * k - b * 26f, -90f - b * 14f - Mathf.Sin(k * 3f) * 18f);
                    float beat = (k * 8f) % 2f < 1.2f ? Mathf.Sin(clock * 14f + b) * 28f : 8f; // beat, then glide
                    wings[b][0].localRotation = Quaternion.Euler(0f, 0f, -20f - beat);
                    wings[b][1].localRotation = Quaternion.Euler(0f, 0f, 20f + beat);
                }
                if (t >= across) { clock = 0f; wait = 9f + Mathf.Repeat(Time.unscaledTime * 7.3f, 8f); }
            }
        }

        /// <summary>Paper lanterns on a sagging string across the top of the view, swaying a little.</summary>
        private sealed class FestivalGarland : MonoBehaviour
        {
            public bool Lit;
            private RectTransform[] lanterns;
            private float clock;

            private void Start()
            {
                var rect = (RectTransform)transform;
                float wide = Mathf.Max(1f, rect.rect.width);
                int count = Mathf.Clamp(Mathf.RoundToInt(wide / 70f), 4, 9);
                lanterns = new RectTransform[count];
                Sprite art = MosseloraArt.Load(FamilySheet.LanternArt);
                for (int i = 0; i < count; i++)
                {
                    float x = (i + 0.5f) / count;
                    float sag = 4f * x * (1f - x) * 26f; // the string dips toward the middle
                    var node = new GameObject("Paper lantern", typeof(RectTransform));
                    node.transform.SetParent(transform, false);
                    var nodeRect = (RectTransform)node.transform;
                    nodeRect.anchorMin = nodeRect.anchorMax = new Vector2(x, 1f);
                    nodeRect.pivot = new Vector2(0.5f, 1f);
                    nodeRect.sizeDelta = new Vector2(40f, 40f);
                    nodeRect.anchoredPosition = new Vector2(0f, -sag);
                    if (Lit)
                    {
                        var glow = new GameObject("Glow", typeof(RectTransform)).AddComponent<Image>();
                        glow.transform.SetParent(node.transform, false);
                        glow.rectTransform.sizeDelta = new Vector2(70f, 70f);
                        glow.rectTransform.anchoredPosition = new Vector2(0f, -20f);
                        glow.sprite = MosseloraArt.SoftGlow();
                        glow.color = new Color(1f, 0.8f, 0.42f, 0.55f);
                        glow.raycastTarget = false;
                    }
                    var image = new GameObject("Lantern", typeof(RectTransform)).AddComponent<Image>();
                    image.transform.SetParent(node.transform, false);
                    image.rectTransform.anchorMin = Vector2.zero;
                    image.rectTransform.anchorMax = Vector2.one;
                    image.rectTransform.offsetMin = image.rectTransform.offsetMax = Vector2.zero;
                    image.sprite = art;
                    image.preserveAspect = true;
                    image.raycastTarget = false;
                    lanterns[i] = nodeRect;
                }
            }

            private void Update()
            {
                if (lanterns == null || !FirefliesMove) return;
                clock += Time.unscaledDeltaTime;
                for (int i = 0; i < lanterns.Length; i++)
                    lanterns[i].localRotation = Quaternion.Euler(0f, 0f, Mathf.Sin(clock * 0.9f + i * 1.3f) * 4f);
            }
        }

        /// <summary>
        /// A few petals, leaves or flakes falling slowly across the view, swaying as they go,
        /// and coming round again at the top. Few and slow on purpose: weather, not confetti.
        /// </summary>
        private sealed class SeasonDrift : MonoBehaviour
        {
            public Drift Kind;
            private RectTransform[] bits;
            private float[] x0, speed, sway, phase;
            private float clock, high, wide;

            private void Start()
            {
                var rect = (RectTransform)transform;
                wide = Mathf.Max(1f, rect.rect.width);
                high = Mathf.Max(1f, rect.rect.height);
                var rng = new System.Random(527);
                int count = Mathf.Max(SeasonLook.DriftCount(Kind), Kind == Drift.Snow ? DayWeather.SnowfallCount(WeatherNow) : 0); // I357: snowy days snow harder
                bits = new RectTransform[count];
                x0 = new float[count]; speed = new float[count]; sway = new float[count]; phase = new float[count];
                for (int i = 0; i < count; i++)
                {
                    var bit = new GameObject(Kind.ToString(), typeof(RectTransform));
                    bit.transform.SetParent(transform, false);
                    var bitRect = (RectTransform)bit.transform;
                    bitRect.anchorMin = bitRect.anchorMax = new Vector2(0f, 1f);
                    float size = Kind == Drift.Snow ? 5f + (float)rng.NextDouble() * 4f : 9f + (float)rng.NextDouble() * 6f;
                    bitRect.sizeDelta = Kind == Drift.Snow ? new Vector2(size, size) : new Vector2(size, size * 0.62f);
                    var image = bit.AddComponent<Image>();
                    image.sprite = MosseloraArt.Rounded(8);
                    image.raycastTarget = false;
                    image.color = Kind == Drift.Petals ? new Color(0.98f, 0.80f, 0.84f, 0.9f)
                        : Kind == Drift.Leaves ? (i % 2 == 0 ? new Color(0.85f, 0.48f, 0.18f, 0.9f) : new Color(0.72f, 0.36f, 0.16f, 0.9f))
                        : new Color(1f, 1f, 1f, 0.85f);
                    x0[i] = (float)rng.NextDouble() * wide;
                    speed[i] = (Kind == Drift.Snow ? 22f : 30f) + (float)rng.NextDouble() * 18f;
                    sway[i] = 14f + (float)rng.NextDouble() * 16f;
                    phase[i] = (float)rng.NextDouble() * high;
                    bits[i] = bitRect;
                }
                Place();
            }

            private void Update()
            {
                clock += Time.unscaledDeltaTime;
                Place();
            }

            private void Place()
            {
                if (bits == null) return;
                for (int i = 0; i < bits.Length; i++)
                {
                    float fall = (phase[i] + clock * speed[i]) % (high + 40f) - 20f;
                    float drift = clock * 8f * DayWeather.WindStrength(WeatherNow) * DayWeather.WindStrength(WeatherNow); // I355: blown on windy days
                    float x = Mathf.Repeat(x0[i] + drift + Mathf.Sin(clock * 0.9f + i) * sway[i], wide);
                    bits[i].anchoredPosition = new Vector2(x, -fall);
                    if (Kind != Drift.Snow) bits[i].localRotation = Quaternion.Euler(0f, 0f, Mathf.Sin(clock * 1.3f + i * 2f) * 50f);
                }
            }
        }

        /// <summary>A painting that is opaque at its top edge and clear at its foot (I252).</summary>
        private sealed class FadeDownRawImage : RawImage
        {
            protected override void OnPopulateMesh(VertexHelper vh)
            {
                vh.Clear();
                Rect r = GetPixelAdjustedRect();
                Rect uv = uvRect;
                Color top = color, foot = new Color(color.r, color.g, color.b, 0f);
                vh.AddVert(new Vector3(r.xMin, r.yMin), foot, new Vector2(uv.xMin, uv.yMin));
                vh.AddVert(new Vector3(r.xMin, r.yMax), top, new Vector2(uv.xMin, uv.yMax));
                vh.AddVert(new Vector3(r.xMax, r.yMax), top, new Vector2(uv.xMax, uv.yMax));
                vh.AddVert(new Vector3(r.xMax, r.yMin), foot, new Vector2(uv.xMax, uv.yMin));
                vh.AddTriangle(0, 1, 2);
                vh.AddTriangle(2, 3, 0);
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

        /// <summary>Thin streaks of rain falling for under two seconds, then fading (I307).</summary>
        private sealed class Shower : MonoBehaviour
        {
            private RectTransform[] drops;
            private float[] x, offset;
            private float clock, high;
            private const float Lasts = 1.8f;

            private void Start()
            {
                var rect = (RectTransform)transform;
                high = Mathf.Max(1f, rect.rect.height);
                float wide = Mathf.Max(1f, rect.rect.width);
                var rng = new System.Random(29);
                drops = new RectTransform[16];
                x = new float[drops.Length];
                offset = new float[drops.Length];
                for (int i = 0; i < drops.Length; i++)
                {
                    var drop = new GameObject("Rain", typeof(RectTransform));
                    drop.transform.SetParent(transform, false);
                    var dropRect = (RectTransform)drop.transform;
                    dropRect.anchorMin = dropRect.anchorMax = new Vector2(0f, 0f);
                    dropRect.sizeDelta = new Vector2(2.5f, 11f + (float)rng.NextDouble() * 6f);
                    dropRect.localRotation = Quaternion.Euler(0f, 0f, -8f);
                    var image = drop.AddComponent<Image>();
                    image.sprite = MosseloraArt.Rounded(2);
                    image.color = new Color(0.78f, 0.88f, 0.98f, 0.8f);
                    image.raycastTarget = false;
                    x[i] = (float)rng.NextDouble() * wide;
                    offset[i] = (float)rng.NextDouble() * high;
                    drops[i] = dropRect;
                }
            }

            private void Update()
            {
                clock += Time.unscaledDeltaTime;
                float fade = Mathf.Clamp01((Lasts - clock) / 0.5f);
                for (int i = 0; i < drops.Length; i++)
                {
                    float y = high - Mathf.Repeat(offset[i] + clock * high * 1.6f, high);
                    drops[i].anchoredPosition = new Vector2(x[i] + (high - y) * 0.14f, y);
                    drops[i].GetComponent<Image>().color = new Color(0.78f, 0.88f, 0.98f, 0.8f * fade);
                }
                if (clock >= Lasts) Destroy(gameObject);
            }
        }

        /// <summary>A slow lean back and forth about the foot, a degree or two (I297).</summary>
        private sealed class Sway : MonoBehaviour
        {
            public float Phase;
            private float clock;

            private void Update()
            {
                clock += Time.unscaledDeltaTime;
                float gust = 0.6f + 0.4f * Mathf.Sin(clock * 0.21f + Phase * 0.3f); // the breeze rises and falls
                float wind = DayWeather.WindStrength(WeatherNow); // I355: windy days lean harder
                transform.localRotation = Quaternion.Euler(0f, 0f, Mathf.Sin(clock * 1.1f * Mathf.Sqrt(wind) + Phase) * 1.8f * gust * wind);
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
