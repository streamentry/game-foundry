using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Evergrove.UnityClient
{
    /// <summary>
    /// Painted storybook skin: original watercolor art generated for Mosselora
    /// (masters in games/evergrove/art/generated, runtime copies in
    /// Resources/Evergrove/Art) plus procedural rounded shapes for every
    /// control. Missing art never blocks play: callers get null and keep the
    /// plain, fully readable layout.
    /// </summary>
    internal static class MosseloraArt
    {
        public const string BannerFlowing = "banner-stream-flowing";
        public const string BannerDry = "banner-stream-dry";
        public const string VineCorner = "vine-corner";
        public const string Wordmark = "wordmark";
        public const string HeroVillage = "hero-village";
        public const string ShadeTree = "icon-shade-tree";
        public const string Bridge = "icon-bridge";
        public const string Wildflowers = "icon-wildflowers";
        public const string Foundation = "icon-foundation";
        public const string Sprout = "glyph-sprout";
        public const string Droplet = "glyph-droplet";
        public const string Sprig = "glyph-sprig";
        public const string SpringWaves = "glyph-waves";
        // Tintable white UI glyphs (icons-ui.png, sliced 4x4).
        public const string IconUndo = "ui-undo";
        public const string IconHint = "ui-hint";
        public const string IconRestart = "ui-restart";
        public const string IconLanguage = "ui-language";
        public const string IconTextSize = "ui-text-size";
        public const string IconBoardSize = "ui-zoom-board";
        public const string IconChimeOn = "ui-sound-on";
        public const string IconChimeOff = "ui-sound-off";
        public const string IconJournal = "ui-journal";
        public const string IconContinue = "ui-continue";
        public const string IconChange = "ui-exchange"; // distinct from the circular Restart
        public const string IconCopyFile = "ui-copy-file";
        public const string IconRestoreFile = "ui-restore-file";
        public const string IconNewGarden = "ui-new-garden";
        public const string IconConfirm = "ui-confirm";
        public const string IconCancel = "ui-cancel";
        public const string IconMenu = "ui-menu";
        public const string IconSettings = "ui-settings";
        public const string IconNeighbors = "ui-neighbors";
        public const string IconHome = "ui-home";
        public const string IconMotionOn = "ui-motion-on";
        public const string IconMotionOff = "ui-motion-off";
        public const string IconPath = "ui-path";
        public const string IconChat = "ui-chat";

        public static Sprite Portrait(int villager)
        {
            switch (villager)
            {
                case 0: return Load("portrait-mira");
                case 1: return Load("portrait-orin");
                default: return Load("portrait-fen");
            }
        }

        private static readonly Dictionary<string, Sprite> Loaded = new Dictionary<string, Sprite>();
        private static readonly Dictionary<int, Sprite> RoundedCache = new Dictionary<int, Sprite>();
        private static readonly Dictionary<int, Sprite> RingCache = new Dictionary<int, Sprite>();

        public static Sprite Load(string name)
        {
            Sprite sprite;
            if (Loaded.TryGetValue(name, out sprite)) return sprite;
            sprite = Resources.Load<Sprite>("Evergrove/Art/" + name);
            if (sprite == null)
            {
                // A Default-imported texture still renders; wrap it once.
                var texture = Resources.Load<Texture2D>("Evergrove/Art/" + name);
                if (texture != null)
                    sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height),
                        new Vector2(0.5f, 0.5f), 100f);
            }
            if (sprite == null) Debug.LogWarning("Mosselora art missing: " + name);
            Loaded[name] = sprite;
            return sprite;
        }

        public static float Aspect(Sprite sprite)
        {
            if (sprite == null || sprite.rect.height <= 0f) return 1f;
            return sprite.rect.width / sprite.rect.height;
        }

        /// <summary>White anti-aliased rounded rectangle, 9-sliced; tint with Image.color.</summary>
        public static Sprite Rounded(int radius = 16)
        {
            Sprite sprite;
            if (RoundedCache.TryGetValue(radius, out sprite) && sprite != null) return sprite;
            sprite = BuildRounded(radius, -1f);
            RoundedCache[radius] = sprite;
            return sprite;
        }

        /// <summary>Rounded outline only (2 px at reference scale), for card and tile edges.</summary>
        public static Sprite Ring(int radius = 16)
        {
            Sprite sprite;
            if (RingCache.TryGetValue(radius, out sprite) && sprite != null) return sprite;
            sprite = BuildRounded(radius, 2.2f);
            RingCache[radius] = sprite;
            return sprite;
        }

        private static Sprite boldRing;

        /// <summary>A heavier full-circle ring for small markers on busy tiles.</summary>
        public static Sprite BoldRing()
        {
            if (boldRing == null) boldRing = BuildRounded(8, 3.6f);
            return boldRing;
        }

        // Textures are drawn at 2x so corners stay smooth on retina phones;
        // pixelsPerUnitMultiplier on the Image brings them back to logical size.
        private const int Supersample = 2;

        private static Sprite BuildRounded(int radius, float ringWidth)
        {
            int r = radius * Supersample;
            int size = r * 2 + 4;
            var texture = new Texture2D(size, size, TextureFormat.RGBA32, false)
            {
                name = ringWidth > 0f ? "Mosselora ring" : "Mosselora rounded",
                wrapMode = TextureWrapMode.Clamp,
                filterMode = FilterMode.Bilinear
            };
            var pixels = new Color32[size * size];
            float centre = size / 2f;
            float inner = Mathf.Max(0f, (size / 2f) - r);
            float width = ringWidth * Supersample;
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float px = Mathf.Abs(x + 0.5f - centre) - inner;
                    float py = Mathf.Abs(y + 0.5f - centre) - inner;
                    float outside = new Vector2(Mathf.Max(px, 0f), Mathf.Max(py, 0f)).magnitude +
                        Mathf.Min(Mathf.Max(px, py), 0f) - r;
                    float alpha = Mathf.Clamp01(0.5f - outside);
                    if (width > 0f) alpha *= Mathf.Clamp01(outside + width + 0.5f);
                    pixels[y * size + x] = new Color32(255, 255, 255, (byte)Mathf.RoundToInt(alpha * 255f));
                }
            }
            texture.SetPixels32(pixels);
            texture.Apply(false, true);
            float border = r + 1;
            return Sprite.Create(texture, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f),
                100f, 0, SpriteMeshType.FullRect, new Vector4(border, border, border, border));
        }

        public const string TilePaper = "tile-paper";
        public const string ButterflyOpen = "butterfly-open";
        public const string ButterflyClosed = "butterfly-closed";

        /// <summary>
        /// Rounded tile body on Codex-generated watercolor paper (near-white,
        /// so Image.color still carries the dry/wet/solved/hint state tint).
        /// </summary>
        public static void PaperTile(Image image, int radius = 12)
        {
            Sprite paper = Load(TilePaper);
            if (paper == null) { Round(image, Color.white, radius); return; }
            image.sprite = paper;
            image.type = Image.Type.Sliced;
            image.pixelsPerUnitMultiplier = Supersample; // 24 px corner = 12 pt
            image.color = Color.white;
        }

        private static Sprite fade;

        /// <summary>White, opaque at the top, transparent at the bottom; tint for a paper fade.</summary>
        public static Sprite TopFade()
        {
            if (fade != null) return fade;
            var texture = new Texture2D(4, 64, TextureFormat.RGBA32, false)
            { wrapMode = TextureWrapMode.Clamp, filterMode = FilterMode.Bilinear, name = "Mosselora top fade" };
            var pixels = new Color32[4 * 64];
            for (int y = 0; y < 64; y++)
            {
                float t = y / 63f; // 0 bottom .. 1 top
                byte a = (byte)Mathf.RoundToInt(255f * Mathf.SmoothStep(0f, 1f, Mathf.Clamp01((t - 0.05f) / 0.6f)));
                for (int x = 0; x < 4; x++) pixels[y * 4 + x] = new Color32(255, 255, 255, a);
            }
            texture.SetPixels32(pixels);
            texture.Apply(false, true);
            fade = Sprite.Create(texture, new Rect(0, 0, 4, 64), new Vector2(0.5f, 0.5f), 100f);
            return fade;
        }

        /// <summary>Gives an Image a rounded, sliced, tinted body.</summary>
        public static void Round(Image image, Color fill, int radius = 16)
        {
            image.sprite = Rounded(radius);
            image.type = Image.Type.Sliced;
            image.pixelsPerUnitMultiplier = Supersample;
            image.color = fill;
        }

        /// <summary>Adds a non-interactive rounded outline on top of a rounded body.</summary>
        public static Image Edge(Transform parent, Color stroke, int radius = 16)
        {
            var node = new GameObject("Painted edge", typeof(RectTransform));
            node.transform.SetParent(parent, false);
            var rect = node.GetComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            node.AddComponent<LayoutElement>().ignoreLayout = true;
            var image = node.AddComponent<Image>();
            image.sprite = Ring(radius);
            image.type = Image.Type.Sliced;
            image.pixelsPerUnitMultiplier = Supersample;
            image.color = stroke;
            image.raycastTarget = false;
            return image;
        }

        /// <summary>Soft paper-lift shadow under cards, tiles and buttons.</summary>
        public static void Lift(Graphic graphic, float depth = 2f, float strength = 0.16f)
        {
            var shadow = graphic.gameObject.AddComponent<Shadow>();
            shadow.effectColor = new Color(0.36f, 0.27f, 0.13f, strength);
            shadow.effectDistance = new Vector2(0f, -depth);
            shadow.useGraphicAlpha = true;
        }

        /// <summary>
        /// A decorative, non-interactive picture that ignores layout. Anchors are
        /// normalized within the parent; size is in logical pixels.
        /// </summary>
        public static Image Ornament(Transform parent, string name, Sprite sprite,
                                     Vector2 anchor, Vector2 pivot, Vector2 size, Vector2 offset,
                                     bool mirror = false)
        {
            if (sprite == null) return null;
            var node = new GameObject(name, typeof(RectTransform));
            node.transform.SetParent(parent, false);
            node.AddComponent<LayoutElement>().ignoreLayout = true;
            var rect = node.GetComponent<RectTransform>();
            rect.anchorMin = anchor;
            rect.anchorMax = anchor;
            rect.pivot = pivot;
            rect.sizeDelta = size;
            rect.anchoredPosition = offset;
            if (mirror) rect.localScale = new Vector3(-1f, 1f, 1f);
            var image = node.AddComponent<Image>();
            image.sprite = sprite;
            image.preserveAspect = true;
            image.raycastTarget = false;
            return image;
        }

        /// <summary>The icon that illustrates a village choice on its card and in the journal.</summary>
        public static Sprite ChoiceIcon(Evergrove.Core.VillageChoice choice)
        {
            switch (choice)
            {
                case Evergrove.Core.VillageChoice.ShadeTree: return Load(ShadeTree);
                case Evergrove.Core.VillageChoice.Bridge: return Load(Bridge);
                default: return Load(Wildflowers);
            }
        }
    }

    /// <summary>
    /// Draws the open-end marker as a ring, mirroring its "○" text. The text
    /// stays the semantic source (screen readers, refresh logic), but not every
    /// runtime face has the glyph, so the visible mark is geometry.
    /// </summary>
    internal sealed class OpenEndRing : MonoBehaviour
    {
        private Text source;
        private Image ring;

        public static void Attach(Text marker, Color stroke, float size)
        {
            var node = new GameObject("Open end ring", typeof(RectTransform));
            node.transform.SetParent(marker.transform, false);
            var rect = node.GetComponent<RectTransform>();
            rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = new Vector2(size, size);
            var image = node.AddComponent<Image>();
            image.sprite = MosseloraArt.BoldRing(); // a full circle when drawn unsliced
            image.type = Image.Type.Simple;
            image.color = stroke;
            image.raycastTarget = false;
            var view = node.AddComponent<OpenEndRing>();
            view.source = marker;
            view.ring = image;
            marker.color = new Color(stroke.r, stroke.g, stroke.b, 0f);
            view.LateUpdate();
        }

        private void LateUpdate()
        {
            if (ring != null && source != null) ring.enabled = source.text.Length > 0;
        }
    }

    /// <summary>
    /// Tactile press: the control squashes slightly under the finger and
    /// springs back, so every tap is felt as well as seen. Scale only; the
    /// hit area and layout do not move.
    /// </summary>
    internal sealed class PressSquash : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler
    {
        private const float Pressed = 0.94f;
        private const float Speed = 18f;
        private float target = 1f;
        private Selectable selectable;

        public static void Add(GameObject node)
        {
            if (node.GetComponent<PressSquash>() == null) node.AddComponent<PressSquash>();
        }

        private void Awake() { selectable = GetComponent<Selectable>(); }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (selectable == null || selectable.interactable) target = Pressed;
        }

        public void OnPointerUp(PointerEventData eventData) { target = 1f; }
        public void OnPointerExit(PointerEventData eventData) { target = 1f; }

        private void Update()
        {
            float current = transform.localScale.x;
            if (Mathf.Abs(current - target) < 0.001f) return;
            float next = Mathf.Lerp(current, target, 1f - Mathf.Exp(-Speed * Time.unscaledDeltaTime));
            transform.localScale = new Vector3(next, next, 1f);
        }
    }

    /// <summary>
    /// One gentle swell (about 7%) after a delay, used once when a creek is
    /// completed. Not looped, not decorative idling: it marks the solve.
    /// </summary>
    internal sealed class BloomPulse : MonoBehaviour
    {
        private const float Duration = 0.36f;
        private float delay;
        private float elapsed;

        public static void Add(GameObject node, float startDelay)
        {
            node.AddComponent<BloomPulse>().delay = startDelay;
        }

        private void Update()
        {
            elapsed += Time.unscaledDeltaTime;
            float t = (elapsed - delay) / Duration;
            if (t < 0f) return;
            if (t >= 1f)
            {
                transform.localScale = Vector3.one;
                Destroy(this);
                return;
            }
            float swell = 1f + 0.07f * Mathf.Sin(Mathf.PI * t);
            transform.localScale = new Vector3(swell, swell, 1f);
        }
    }

    /// <summary>
    /// Press-and-hold name tip for icon-only controls: after a short hold the
    /// control's own (localized) caption appears in a bubble above it. A hold
    /// that showed the tip never triggers the action on release, so curiosity
    /// about Restart cannot restart the stream.
    /// </summary>
    internal sealed class IconTip : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler
    {
        private const float HoldSeconds = 0.45f;
        private Text caption;
        private Color ink;
        private Color paper;
        private PointerEventData press;
        private float held = -1f;
        private GameObject bubble;

        public static void Add(GameObject node, Text caption, Color ink, Color paper)
        {
            var tip = node.AddComponent<IconTip>();
            tip.caption = caption;
            tip.ink = ink;
            tip.paper = paper;
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            press = eventData;
            held = 0f;
        }

        public void OnPointerUp(PointerEventData eventData) { Hide(); }
        public void OnPointerExit(PointerEventData eventData) { Hide(); }
        private void OnDisable() { Hide(); }

        private void Update()
        {
            if (held < 0f || bubble != null) return;
            held += Time.unscaledDeltaTime;
            if (held >= HoldSeconds) Show();
        }

        private void Show()
        {
            if (caption == null || string.IsNullOrWhiteSpace(caption.text)) return;
            if (press != null) press.eligibleForClick = false; // the hold was a question, not a tap
            Canvas canvas = GetComponentInParent<Canvas>();
            if (canvas == null) return;
            Transform root = canvas.rootCanvas.transform;
            bubble = new GameObject("Icon name tip", typeof(RectTransform));
            bubble.transform.SetParent(root, false);
            var rect = bubble.GetComponent<RectTransform>();
            rect.pivot = new Vector2(0.5f, 0f);
            var fill = bubble.AddComponent<Image>();
            MosseloraArt.Round(fill, ink, 14);
            fill.raycastTarget = false;
            MosseloraArt.Lift(fill, 3f, 0.25f);
            var fit = bubble.AddComponent<ContentSizeFitter>();
            fit.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
            fit.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            var pad = bubble.AddComponent<HorizontalLayoutGroup>();
            pad.padding = new RectOffset(14, 14, 8, 8);
            pad.childControlWidth = true;
            pad.childControlHeight = true;
            var words = new GameObject("Tip words", typeof(RectTransform));
            words.transform.SetParent(bubble.transform, false);
            var text = words.AddComponent<Text>();
            text.font = caption.font;
            text.fontSize = Mathf.Max(16, caption.fontSize);
            text.fontStyle = FontStyle.Bold;
            text.color = paper;
            text.alignment = TextAnchor.MiddleCenter;
            text.horizontalOverflow = HorizontalWrapMode.Overflow;
            text.raycastTarget = false;
            text.text = caption.text;
            var self = (RectTransform)transform;
            Vector3 top = self.TransformPoint(new Vector3(self.rect.center.x, self.rect.yMax, 0f));
            rect.position = top;
            rect.anchoredPosition += new Vector2(0f, 10f);
            LayoutRebuilder.ForceRebuildLayoutImmediate(rect);
            // Keep the bubble on screen near the edges.
            var rootRect = (RectTransform)root;
            float half = rect.rect.width / 2f;
            float limit = rootRect.rect.width / 2f - 8f;
            Vector2 at = rect.anchoredPosition;
            Vector3 local = rootRect.InverseTransformPoint(rect.position);
            float shift = Mathf.Clamp(local.x, -limit + half, limit - half) - local.x;
            rect.anchoredPosition = at + new Vector2(shift, 0f);
        }

        private void Hide()
        {
            held = -1f;
            if (bubble != null) Destroy(bubble);
            bubble = null;
        }
    }

    /// <summary>
    /// Solve reward: a painted butterfly lifts off the garden tile, flaps
    /// between two watercolor frames and drifts up and away over ~2 s while
    /// the finished creek holds on screen. Plays once; destroys itself.
    /// </summary>
    internal sealed class ButterflyFlight : MonoBehaviour
    {
        private const float Duration = 2.1f;
        private const float FlapSeconds = 0.11f;
        private RectTransform from;
        private Image image;
        private Sprite open;
        private Sprite closed;
        private Vector2 start;
        private float elapsed = -1f;

        public static void Launch(RectTransform gardenTile, Transform overlay)
        {
            Sprite wingsOpen = MosseloraArt.Load(MosseloraArt.ButterflyOpen);
            Sprite wingsClosed = MosseloraArt.Load(MosseloraArt.ButterflyClosed);
            if (gardenTile == null || overlay == null || wingsOpen == null) return;
            var node = new GameObject("Solve butterfly", typeof(RectTransform));
            node.transform.SetParent(overlay, false);
            node.AddComponent<LayoutElement>().ignoreLayout = true;
            var rect = node.GetComponent<RectTransform>();
            rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = new Vector2(46f, 46f);
            var flight = node.AddComponent<ButterflyFlight>();
            flight.from = gardenTile;
            flight.open = wingsOpen;
            flight.closed = wingsClosed != null ? wingsClosed : wingsOpen;
            flight.image = node.AddComponent<Image>();
            flight.image.sprite = wingsOpen;
            flight.image.preserveAspect = true;
            flight.image.raycastTarget = false;
            flight.image.color = new Color(1f, 1f, 1f, 0f); // appears once layout has placed the tile
        }

        private void Update()
        {
            var rect = (RectTransform)transform;
            if (elapsed < 0f)
            {
                if (from == null) { Destroy(gameObject); return; }
                // Start on the garden tile's centre, in the overlay's space.
                Vector3 world = from.TransformPoint(from.rect.center);
                start = ((RectTransform)rect.parent).InverseTransformPoint(world);
                elapsed = 0f;
            }
            elapsed += Time.unscaledDeltaTime;
            float t = elapsed / Duration;
            if (t >= 1f) { Destroy(gameObject); return; }
            // Up and away in a soft S, drifting toward the open sky.
            float rise = Mathf.SmoothStep(0f, 1f, t) * 210f;
            float sway = Mathf.Sin(t * Mathf.PI * 2.2f) * 34f + t * 60f;
            rect.anchoredPosition = start + new Vector2(sway, rise);
            rect.localRotation = Quaternion.Euler(0f, 0f, Mathf.Sin(t * Mathf.PI * 4f) * 10f);
            image.sprite = ((int)(elapsed / FlapSeconds) % 2 == 0) ? open : closed;
            float alpha = t < 0.12f ? t / 0.12f : t > 0.75f ? (1f - t) / 0.25f : 1f;
            image.color = new Color(1f, 1f, 1f, alpha);
        }
    }

    /// <summary>
    /// Paper band behind the floating chrome that appears only once the page
    /// has scrolled, so content melts under the buttons instead of colliding.
    /// </summary>
    internal sealed class ScrollFade : MonoBehaviour
    {
        private RectTransform page;
        private Image band;

        public static void Attach(Image band, RectTransform page)
        {
            var fade = band.gameObject.AddComponent<ScrollFade>();
            fade.band = band;
            fade.page = page;
            fade.LateUpdate();
        }

        private void LateUpdate()
        {
            if (band == null || page == null) return;
            float scrolled = Mathf.Clamp01(page.anchoredPosition.y / 36f);
            Color c = band.color;
            if (Mathf.Abs(c.a - scrolled) < 0.01f) return;
            band.color = new Color(c.r, c.g, c.b, scrolled);
        }
    }

    /// <summary>A slow, gentle alpha breath for a hint glow (only with gentle motion).</summary>
    internal sealed class Breathe : MonoBehaviour
    {
        private Graphic graphic;
        private Color baseColor;

        public static void Add(Graphic target)
        {
            var breathe = target.gameObject.AddComponent<Breathe>();
            breathe.graphic = target;
            breathe.baseColor = target.color;
        }

        private void Update()
        {
            if (graphic == null) return;
            float a = 0.55f + 0.45f * (0.5f + 0.5f * Mathf.Sin(Time.unscaledTime * 2.4f));
            graphic.color = new Color(baseColor.r, baseColor.g, baseColor.b, baseColor.a * a);
        }
    }
}
