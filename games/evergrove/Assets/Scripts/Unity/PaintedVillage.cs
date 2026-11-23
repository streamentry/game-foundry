using UnityEngine;
using UnityEngine.UI;

namespace Evergrove.UnityClient
{
    /// <summary>
    /// Watercolor village strip. Same causal contract as VillageIllustration:
    /// the dry creek painting gives way to the flowing one only as far as real
    /// spring-connected water reaches (waterReachPercent), the full creek only
    /// once restored, and every saved choice adds its own painted landmark.
    /// Nothing here is a decorative fake-progress animation.
    /// </summary>
    public sealed class PaintedVillage : MonoBehaviour
    {
        // Normalized (x, y-from-bottom, height-as-fraction-of-strip) slots,
        // composed against banner-stream-*.png. Pivot is bottom-centre.
        private static readonly Vector3[] TreeSlots =
        {
            new Vector3(0.61f, 0.39f, 0.40f), new Vector3(0.80f, 0.42f, 0.34f),
            new Vector3(0.35f, 0.39f, 0.33f), new Vector3(0.50f, 0.42f, 0.27f)
        };
        private static readonly Vector3[] BridgeSlots =
        {
            new Vector3(0.46f, 0.10f, 0.34f), new Vector3(0.71f, 0.05f, 0.30f)
        };
        private static readonly Vector3[] FlowerSlots =
        {
            new Vector3(0.28f, 0.30f, 0.20f), new Vector3(0.69f, 0.30f, 0.20f),
            new Vector3(0.56f, 0.35f, 0.16f), new Vector3(0.42f, 0.36f, 0.15f)
        };

        private Image dry;
        private Image flowing;
        private RectTransform landmarks;
        private int shownTrees = -1;
        private int shownBridges = -1;
        private int shownFlowers = -1;

        /// <summary>False when the painted masters are not in the build; caller falls back.</summary>
        public bool Ready { get; private set; }

        public void Build(float bleed)
        {
            Sprite drySprite = MosseloraArt.Load(MosseloraArt.BannerDry);
            Sprite wetSprite = MosseloraArt.Load(MosseloraArt.BannerFlowing);
            Ready = drySprite != null && wetSprite != null;
            if (!Ready) return;
            // The strip runs edge to edge, past the page margins.
            var frame = Layer("Painted creek frame", transform);
            frame.offsetMin = new Vector2(-bleed, 0f);
            frame.offsetMax = new Vector2(bleed, 0f);
            dry = Picture(frame, "Dry creek painting", drySprite);
            flowing = Picture(frame, "Flowing creek painting", wetSprite);
            landmarks = Layer("Village choices", frame);
        }

        private static RectTransform Layer(string name, Transform parent)
        {
            var node = new GameObject(name, typeof(RectTransform));
            node.transform.SetParent(parent, false);
            var rect = node.GetComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            return rect;
        }

        private static Image Picture(Transform parent, string name, Sprite sprite)
        {
            var image = Layer(name, parent).gameObject.AddComponent<Image>();
            image.sprite = sprite;
            image.raycastTarget = false;
            return image;
        }

        public void Show(int chapterIndex, int shade, int crossings, int meadows,
                         bool restored, int waterReachPercent)
        {
            if (!Ready) return;
            // Honest water: alpha follows the measured spring reach, and
            // only a solved, leak-free route shows the whole flowing creek.
            float reach = restored ? 1f : Mathf.Clamp01(waterReachPercent / 100f) * 0.85f;
            flowing.color = new Color(1f, 1f, 1f, reach);
            // Later chapters are a touch warmer and greener: the village has lived longer.
            float season = Mathf.Clamp01(chapterIndex / 11f);
            dry.color = Color.Lerp(new Color(0.97f, 0.95f, 0.90f), Color.white, season);
            if (shade == shownTrees && crossings == shownBridges && meadows == shownFlowers) return;
            shownTrees = shade;
            shownBridges = crossings;
            shownFlowers = meadows;
            for (int i = landmarks.childCount - 1; i >= 0; i--)
                Destroy(landmarks.GetChild(i).gameObject);
            Place(MosseloraArt.Load(MosseloraArt.ShadeTree), TreeSlots, shade, "Planted shade tree");
            Place(MosseloraArt.Load(MosseloraArt.Wildflowers), FlowerSlots, meadows, "Grown wildflowers");
            Place(MosseloraArt.Load(MosseloraArt.Bridge), BridgeSlots, crossings, "Restored crossing");
        }

        private void Place(Sprite sprite, Vector3[] slots, int count, string label)
        {
            if (sprite == null) return;
            int shown = Mathf.Min(Mathf.Max(0, count), slots.Length);
            for (int i = 0; i < shown; i++)
            {
                var node = new GameObject(label, typeof(RectTransform));
                node.transform.SetParent(landmarks, false);
                var rect = node.GetComponent<RectTransform>();
                Vector2 at = new Vector2(slots[i].x, slots[i].y);
                rect.anchorMin = at;
                rect.anchorMax = at;
                rect.pivot = new Vector2(0.5f, 0f);
                // Height is a share of the strip; width follows the sprite.
                rect.sizeDelta = Vector2.zero;
                var fitter = node.AddComponent<SlotSize>();
                fitter.Configure(slots[i].z, MosseloraArt.Aspect(sprite));
                var image = node.AddComponent<Image>();
                image.sprite = sprite;
                image.preserveAspect = true;
                image.raycastTarget = false;
            }
        }

        /// <summary>Keeps a landmark proportional to the strip at any phone width.</summary>
        private sealed class SlotSize : MonoBehaviour
        {
            private float share;
            private float aspect = 1f;
            private Vector2 lastParent;

            public void Configure(float heightShare, float spriteAspect)
            {
                share = heightShare;
                aspect = spriteAspect;
                Apply();
            }

            private void LateUpdate()
            {
                var parent = transform.parent as RectTransform;
                if (parent != null && parent.rect.size != lastParent) Apply();
            }

            private void Apply()
            {
                var parent = transform.parent as RectTransform;
                if (parent == null) return;
                lastParent = parent.rect.size;
                float height = parent.rect.height * share;
                ((RectTransform)transform).sizeDelta = new Vector2(height * aspect, height);
            }
        }
    }
}
