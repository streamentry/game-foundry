using System.Collections.Generic;
using UnityEngine;
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
}
