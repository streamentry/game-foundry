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
        private float targetReach;
        private bool shownOnce;
        private int shownTrees = -1;
        private int shownBridges = -1;
        private int shownFlowers = -1;

        /// <summary>
        /// Set before Show to the kind just chosen (0 tree, 1 crossing, 2 flowers):
        /// that kind's newest landmark grows up out of the ground once. -1 = none.
        /// </summary>
        public int GrowNewestKind = -1;

        /// <summary>False when the painted masters are not in the build; caller falls back.</summary>
        public bool Ready { get; private set; }

        /// <summary>
        /// Seasons arrive through progress, never the clock (vision 4.2): the
        /// village's landscape phase 0-1 is summer, 2 autumn, 3 winter, and a
        /// finished garden turns back to summer for the festival.
        /// </summary>
        public static int SeasonFor(int landscapePhase, bool completed)
        {
            if (completed) return 0;
            return landscapePhase >= 3 ? 2 : landscapePhase >= 2 ? 1 : 0;
        }

        public void Build(float bleed, int season = 0)
        {
            string prefix = season == 2 ? "banner-winter-" : season == 1 ? "banner-autumn-" : null;
            Sprite drySprite = prefix != null ? MosseloraArt.Load(prefix + "dry") : null;
            Sprite wetSprite = prefix != null ? MosseloraArt.Load(prefix + "flowing") : null;
            if (drySprite == null || wetSprite == null)
            {
                // Missing seasonal art falls back to the summer creek.
                drySprite = MosseloraArt.Load(MosseloraArt.BannerDry);
                wetSprite = MosseloraArt.Load(MosseloraArt.BannerFlowing);
            }
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

        private void Update()
        {
            if (flowing == null) return;
            float current = flowing.color.a;
            if (Mathf.Abs(current - targetReach) < 0.002f) return;
            float next = Mathf.MoveTowards(current, targetReach, Time.unscaledDeltaTime * 1.8f);
            flowing.color = new Color(1f, 1f, 1f, next);
        }

        public void Show(int chapterIndex, int shade, int crossings, int meadows,
                         bool restored, int waterReachPercent)
        {
            if (!Ready) return;
            // Honest water: alpha follows the measured spring reach, and
            // only a solved, leak-free route shows the whole flowing creek.
            float reach = restored ? 1f : Mathf.Clamp01(waterReachPercent / 100f) * 0.85f;
            // Water arrives over ~half a second rather than popping in; the
            // target is still exactly the measured reach.
            targetReach = reach;
            if (!shownOnce) { flowing.color = new Color(1f, 1f, 1f, reach); shownOnce = true; }
            // Later chapters are a touch warmer and greener: the village has lived longer.
            float season = Mathf.Clamp01(chapterIndex / 11f);
            dry.color = Color.Lerp(new Color(0.97f, 0.95f, 0.90f), Color.white, season);
            if (shade == shownTrees && crossings == shownBridges && meadows == shownFlowers) return;
            shownTrees = shade;
            shownBridges = crossings;
            shownFlowers = meadows;
            for (int i = landmarks.childCount - 1; i >= 0; i--)
                Destroy(landmarks.GetChild(i).gameObject);
            Transform tree = Place(MosseloraArt.Load(MosseloraArt.ShadeTree), TreeSlots, shade, "Planted shade tree");
            Transform flower = Place(MosseloraArt.Load(MosseloraArt.Wildflowers), FlowerSlots, meadows, "Grown wildflowers");
            Transform bridge = Place(MosseloraArt.Load(MosseloraArt.Bridge), BridgeSlots, crossings, "Restored crossing");
            Transform newest = GrowNewestKind == 0 ? tree : GrowNewestKind == 1 ? bridge : GrowNewestKind == 2 ? flower : null;
            if (newest != null) newest.gameObject.AddComponent<GrowIn>();
            GrowNewestKind = -1;
        }

        /// <summary>Places this kind's landmarks; returns the newest (last slot) or null.</summary>
        private Transform Place(Sprite sprite, Vector3[] slots, int count, string label)
        {
            Transform last = null;
            if (sprite == null) return null;
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
                last = node.transform;
            }
            return last;
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

        /// <summary>A landmark rises out of the ground, bottom-pivoted, over 0.9 s.</summary>
        private sealed class GrowIn : MonoBehaviour
        {
            private float elapsed;

            private void Start() { transform.localScale = new Vector3(0.2f, 0f, 1f); }

            private void Update()
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01((elapsed - 0.35f) / 0.9f);
                // Ease out with a small settle: grows a touch past full, then rests.
                float k = 1f - Mathf.Pow(1f - t, 3f);
                float settle = 1f + 0.06f * Mathf.Sin(t * Mathf.PI);
                transform.localScale = new Vector3(Mathf.Lerp(0.2f, 1f, k) * settle, k * settle, 1f);
                if (t >= 1f) { transform.localScale = Vector3.one; Destroy(this); }
            }
        }
    }
}
