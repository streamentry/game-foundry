using UnityEngine;
using UnityEngine.UI;

namespace Evergrove.UnityClient
{
    /// <summary>
    /// Lightweight hand-composed vector illustration. What appears on the canvas
    /// is derived from world choices, not a decorative static wallpaper.
    /// No external image licensing, GPU effects or runtime network dependency.
    /// </summary>
    public sealed class VillageIllustration : MaskableGraphic
    {
        private int chapter;
        private int trees;
        private int bridges;
        private int flowers;
        private bool waterRestored;

        public void Show(int chapterIndex, int shade, int crossings, int meadows, bool flowing)
        {
            chapter = chapterIndex;
            trees = Mathf.Max(0, shade);
            bridges = Mathf.Max(0, crossings);
            flowers = Mathf.Max(0, meadows);
            waterRestored = flowing;
            raycastTarget = false;
            SetVerticesDirty();
        }

        private static Color32 C(string hex)
        {
            ColorUtility.TryParseHtmlString(hex, out Color color);
            return color;
        }

        private Vector3 P(float x, float y)
        {
            Rect frame = rectTransform.rect;
            return new Vector3(frame.xMin + frame.width * x, frame.yMin + frame.height * y, 0f);
        }

        private static void Quad(VertexHelper vh, Vector3 a, Vector3 b, Vector3 c, Vector3 d, Color32 color)
        {
            int index = vh.currentVertCount;
            vh.AddVert(a, color, Vector2.zero);
            vh.AddVert(b, color, Vector2.zero);
            vh.AddVert(c, color, Vector2.zero);
            vh.AddVert(d, color, Vector2.zero);
            vh.AddTriangle(index, index + 1, index + 2);
            vh.AddTriangle(index, index + 2, index + 3);
        }

        private void Rect(VertexHelper vh, float x, float y, float w, float h, Color32 color)
        {
            Quad(vh, P(x, y), P(x, y + h), P(x + w, y + h), P(x + w, y), color);
        }

        private void Triangle(VertexHelper vh, float ax, float ay, float bx, float by,
                              float cx, float cy, Color32 color)
        {
            int index = vh.currentVertCount;
            vh.AddVert(P(ax, ay), color, Vector2.zero);
            vh.AddVert(P(bx, by), color, Vector2.zero);
            vh.AddVert(P(cx, cy), color, Vector2.zero);
            vh.AddTriangle(index, index + 1, index + 2);
        }

        private void Circle(VertexHelper vh, float x, float y, float radius, Color32 color, int sections = 16)
        {
            float width = Mathf.Max(1f, rectTransform.rect.width);
            float height = Mathf.Max(1f, rectTransform.rect.height);
            float ry = radius * width / height;
            for (int i = 0; i < sections; i++)
            {
                float a = Mathf.PI * 2f * i / sections;
                float b = Mathf.PI * 2f * (i + 1) / sections;
                Triangle(vh, x, y, x + radius * Mathf.Cos(a), y + ry * Mathf.Sin(a),
                    x + radius * Mathf.Cos(b), y + ry * Mathf.Sin(b), color);
            }
        }

        private void Flower(VertexHelper vh, float x, float y, Color32 petals)
        {
            Rect(vh, x - 0.0025f, y - 0.031f, 0.005f, 0.033f, C("#46795D"));
            float offset = 0.010f;
            Circle(vh, x - offset, y + offset, 0.008f, petals, 8);
            Circle(vh, x + offset, y + offset, 0.008f, petals, 8);
            Circle(vh, x, y + offset * 2f, 0.008f, petals, 8);
            Circle(vh, x, y, 0.008f, petals, 8);
            Circle(vh, x, y + offset, 0.006f, C("#F5B94C"), 8);
        }

        private void Tree(VertexHelper vh, float x, float y, float size)
        {
            Color32 trunk = C("#755441");
            Color32 canopy = chapter >= 9 ? C("#B77842") :
                chapter >= 6 ? C("#5A8A62") : C("#4E8B65");
            Rect(vh, x - 0.010f * size, y, 0.020f * size, 0.16f * size, trunk);
            Circle(vh, x, y + 0.155f * size, 0.058f * size, canopy);
            Circle(vh, x - 0.043f * size, y + 0.130f * size, 0.046f * size, canopy);
            Circle(vh, x + 0.041f * size, y + 0.133f * size, 0.045f * size, canopy);
            Circle(vh, x - 0.021f * size, y + 0.191f * size, 0.043f * size, C("#81AB70"));
        }

        private void Butterfly(VertexHelper vh, float x, float y)
        {
            Circle(vh, x - 0.013f, y, 0.013f, C("#F5CC78"), 10);
            Circle(vh, x + 0.013f, y, 0.013f, C("#DCAEC5"), 10);
            Rect(vh, x - 0.0025f, y - 0.020f, 0.005f, 0.039f, C("#4B614E"));
        }

        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();
            if (rectTransform.rect.width <= 0 || rectTransform.rect.height <= 0) return;
            // All layers are clipped by the ScrollRect viewport. Composition is
            // intentionally restrained: a small world, not a procedural art feed.
            Color32 sky = chapter >= 9 ? C("#E3CEBD") : chapter >= 6 ? C("#DCE8D8") : C("#E9EAD9");
            Color32 skyTop = chapter >= 9 ? C("#B7CCD0") : C("#C9E5E3");
            Rect(vh, 0f, 0f, 1f, 1f, sky);
            Rect(vh, 0f, 0.62f, 1f, 0.38f, skyTop);
            Circle(vh, 0.78f, 0.77f, 0.060f, chapter >= 9 ? C("#F7D39A") : C("#FCE9BB"));
            Circle(vh, 0.78f, 0.77f, 0.043f, C("#FFE8B3"));
            Triangle(vh, 0f, 0.44f, 0.24f, 0.70f, 0.50f, 0.43f, C("#B6CCB1"));
            Triangle(vh, 0.27f, 0.46f, 0.60f, 0.68f, 1f, 0.43f, C("#AEC6B0"));
            Rect(vh, 0f, 0f, 1f, 0.42f, C("#B2CEAA"));
            Triangle(vh, 0f, 0.42f, 0.42f, 0.53f, 0.62f, 0.40f, C("#A4C49B"));
            Triangle(vh, 0.45f, 0.40f, 0.78f, 0.54f, 1f, 0.39f, C("#A9C8A1"));

            // Creek meanders through the lower third.
            Triangle(vh, 0.44f, 0.42f, 0.64f, 0.42f, 0.35f, 0f,
                waterRestored ? C("#6FB4BA") : C("#C4B69C"));
            Triangle(vh, 0.64f, 0.42f, 0.70f, 0f, 0.35f, 0f,
                waterRestored ? C("#6FB4BA") : C("#C4B69C"));
            if (waterRestored)
                Triangle(vh, 0.51f, 0.40f, 0.56f, 0.40f, 0.46f, 0.08f, C("#A5D9CE"));
            Rect(vh, 0f, 0f, 1f, 0.025f, C("#8CAD8E"));

            // Tiny home on the left, emerging as a landmark.
            Rect(vh, 0.075f, 0.23f, 0.15f, 0.18f, C("#F5E2BB"));
            Triangle(vh, 0.055f, 0.41f, 0.15f, 0.56f, 0.249f, 0.41f, C("#956955"));
            Rect(vh, 0.137f, 0.23f, 0.040f, 0.10f, C("#8E715D"));
            Rect(vh, 0.09f, 0.345f, 0.032f, 0.035f, C("#95CAC6"));

            if (bridges > 0)
            {
                Rect(vh, 0.34f, 0.27f, 0.36f, 0.055f, C("#8C6754"));
                Rect(vh, 0.34f, 0.325f, 0.36f, 0.018f, C("#BB9270"));
                for (int p = 0; p < 4; p++)
                    Rect(vh, 0.36f + 0.10f * p, 0.24f, 0.014f, 0.10f, C("#735744"));
            }
            if (bridges >= 3)
            {
                Rect(vh, 0.43f, 0.10f, 0.19f, 0.024f, C("#9F7760"));
                Rect(vh, 0.45f, 0.071f, 0.015f, 0.055f, C("#735744"));
                Rect(vh, 0.60f, 0.071f, 0.015f, 0.055f, C("#735744"));
            }

            for (int i = 0; i < Mathf.Min(6, trees); i++)
            {
                float x = i % 2 == 0 ? 0.81f + 0.055f * ((i / 2) % 2) : 0.25f - 0.055f * ((i / 2) % 2);
                float y = 0.18f + 0.028f * (i / 2);
                Tree(vh, x, y, i == 0 ? 1.04f : 0.76f);
            }

            Color32[] palette = { C("#F0C9D8"), C("#F5DE96"), C("#D5C7E8") };
            for (int i = 0; i < Mathf.Min(7, flowers * 3); i++)
            {
                float x = 0.71f + 0.071f * (i % 4);
                float y = 0.09f + 0.060f * (i / 4);
                Flower(vh, x, y, palette[i % palette.Length]);
            }
            if (trees > 0 && flowers > 0)
            {
                Butterfly(vh, 0.68f, 0.49f);
                if (flowers > 2) Butterfly(vh, 0.89f, 0.58f);
            }
            if (bridges > 0 && flowers > 0)
            {
                // A quiet figure visiting the meadow.
                Circle(vh, 0.75f, 0.32f, 0.018f, C("#C18F74"));
                Triangle(vh, 0.73f, 0.20f, 0.77f, 0.20f, 0.75f, 0.30f, C("#795B83"));
            }
            // Three quiet neighbors always exist; their place, not their number,
            // reflects the choices that shaped the world.
            Circle(vh, 0.17f, 0.18f, 0.018f, C("#BB886F"));
            Triangle(vh, 0.15f, 0.08f, 0.19f, 0.08f, 0.17f, 0.16f, C("#547E69"));
            Circle(vh, 0.27f, 0.18f, 0.018f, C("#A87761"));
            Triangle(vh, 0.25f, 0.08f, 0.29f, 0.08f, 0.27f, 0.16f, C("#A56C60"));
            Circle(vh, 0.37f, 0.18f, 0.018f, C("#D4AD88"));
            Triangle(vh, 0.35f, 0.08f, 0.39f, 0.08f, 0.37f, 0.16f, C("#6F84A1"));
        }
    }
}
