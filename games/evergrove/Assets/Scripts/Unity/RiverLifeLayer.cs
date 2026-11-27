using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Evergrove.UnityClient
{
    /// <summary>
    /// I413 to I417: the river's small moments on the real water cells of the map. Rings when
    /// it rains, a fish rising now and then, glitter in the golden hour, autumn leaves carried
    /// downstream, and a ripple where you tap the water. When (and whether) each happens is the
    /// Core's (RiverMoments); this only draws. With reduced motion it keeps still glitter only.
    /// </summary>
    internal sealed class RiverLifeLayer : MonoBehaviour
    {
        public List<Vector2> Water = new List<Vector2>();  // water cell centres, land coordinates
        public List<Vector2> Course = new List<Vector2>(); // one point per row, top to bottom: the river's line
        public float Cell = 40f;
        public bool Rain, Glitter, Leaves, Moving = true;
        public bool Moon; // I425: the glitter is moonlight: fewer, paler, slower
        public float FishEvery;

        private readonly List<Ring> rings = new List<Ring>();
        private readonly List<RectTransform> leaves = new List<RectTransform>();
        private readonly List<float> leafAt = new List<float>();
        private readonly List<Image> sparkles = new List<Image>();
        private float clock, nextRain, nextFish = 2f;
        private System.Random dice;

        private sealed class Ring
        {
            public Image Image;
            public float Born, Life, Size;
            public bool Fish;
        }

        private void Start()
        {
            dice = new System.Random(Water.Count * 31 + Course.Count);
            if (Glitter)
                for (int i = 0; i < Mathf.Min(Moon ? 9 : 14, Water.Count); i++)
                {
                    Image spark = Dot(Moon ? "Moonlight on the water" : "Glitter on the water", Water[dice.Next(Water.Count)] + Jitter(0.3f),
                        new Vector2(5f, 5f), Moon ? new Color(0.85f, 0.92f, 1f, 0.8f) : new Color(1f, 0.96f, 0.80f, 0.9f), MosseloraArt.SoftGlow());
                    sparkles.Add(spark);
                }
            if (Leaves && Moving && Course.Count > 2)
                for (int i = 0; i < 5; i++)
                {
                    Image leaf = Dot("A leaf on the water", Course[0], new Vector2(9f, 6f),
                        i % 2 == 0 ? new Color(0.86f, 0.50f, 0.20f, 0.95f) : new Color(0.80f, 0.62f, 0.22f, 0.95f), MosseloraArt.Rounded(3));
                    leaves.Add((RectTransform)leaf.transform);
                    leafAt.Add(i / 5f * (Course.Count - 1));
                }
        }

        /// <summary>I414: a ripple where the water was tapped.</summary>
        public void Ripple(Vector2 at)
        {
            Spawn(at, 0.9f, Cell * 1.1f, false);
        }

        private void Update()
        {
            float dt = Time.unscaledDeltaTime;
            clock += dt;
            for (int i = 0; i < sparkles.Count; i++)
            {
                float twinkle = Moving ? 0.35f + 0.65f * Mathf.Abs(Mathf.Sin(clock * (Moon ? 0.8f : 1.7f) + i * 1.3f)) : 0.7f;
                Color c = sparkles[i].color;
                sparkles[i].color = new Color(c.r, c.g, c.b, twinkle);
            }
            if (!Moving || Water.Count == 0) { Age(); return; }
            if (Rain && clock >= nextRain)
            {
                Spawn(Water[dice.Next(Water.Count)] + Jitter(0.4f), 1.1f, Cell * 0.5f, false);
                nextRain = clock + 0.18f + (float)dice.NextDouble() * 0.2f;
            }
            if (FishEvery > 0f && clock >= nextFish)
            {
                Spawn(Water[dice.Next(Water.Count)] + Jitter(0.25f), 1.6f, Cell * 0.8f, true);
                nextFish = clock + FishEvery * (0.6f + 0.8f * (float)dice.NextDouble());
            }
            for (int i = 0; i < leaves.Count; i++)
            {
                leafAt[i] = (leafAt[i] + dt * 0.12f) % (Course.Count - 1);
                int k = Mathf.FloorToInt(leafAt[i]);
                float f = leafAt[i] - k;
                leaves[i].anchoredPosition = Vector2.Lerp(Course[k], Course[k + 1], f) + new Vector2(Mathf.Sin(clock * 0.8f + i) * Cell * 0.12f, 0f);
                leaves[i].localRotation = Quaternion.Euler(0f, 0f, clock * 25f + i * 70f);
            }
            Age();
        }

        private void Spawn(Vector2 at, float life, float size, bool fish)
        {
            Ring ring = null;
            foreach (Ring r in rings) if (clock - r.Born > r.Life) { ring = r; break; }
            if (ring == null)
            {
                if (rings.Count >= 24) return;
                ring = new Ring { Image = Dot("Ring on the water", at, Vector2.one, Color.clear, MosseloraArt.Halo()) };
                rings.Add(ring);
            }
            ring.Born = clock;
            ring.Life = life;
            ring.Size = size;
            ring.Fish = fish;
            ((RectTransform)ring.Image.transform).anchoredPosition = at;
        }

        private void Age()
        {
            foreach (Ring r in rings)
            {
                float age = (clock - r.Born) / r.Life;
                if (age < 0f || age > 1f) { r.Image.color = Color.clear; continue; }
                float grow = 0.25f + 0.75f * age;
                ((RectTransform)r.Image.transform).sizeDelta = new Vector2(r.Size * grow, r.Size * grow * 0.55f);
                // A fish rising leaves a brighter first ring, the rain a faint one.
                r.Image.color = new Color(1f, 1f, 1f, (r.Fish ? 0.75f : 0.45f) * (1f - age));
            }
        }

        private Vector2 Jitter(float cells)
        {
            return new Vector2(((float)dice.NextDouble() - 0.5f) * Cell * cells * 2f, ((float)dice.NextDouble() - 0.5f) * Cell * cells * 2f);
        }

        private Image Dot(string name, Vector2 at, Vector2 size, Color colour, Sprite sprite)
        {
            var node = new GameObject(name, typeof(RectTransform));
            node.transform.SetParent(transform, false);
            var rect = (RectTransform)node.transform;
            rect.anchorMin = rect.anchorMax = new Vector2(0f, 1f);
            rect.sizeDelta = size;
            rect.anchoredPosition = at;
            var image = node.AddComponent<Image>();
            image.sprite = sprite;
            image.color = colour;
            image.raycastTarget = false;
            return image;
        }
    }
}
