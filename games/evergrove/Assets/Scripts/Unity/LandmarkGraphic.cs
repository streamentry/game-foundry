using UnityEngine;
using UnityEngine.UI;

namespace Evergrove.UnityClient
{
    /// <summary>
    /// Procedural protected-landmark marker: a heavy bark frame around the
    /// anchored tile plus a variant-specific badge in the top-left corner.
    /// The badge sits outside every channel arm (arms leave the tile centre
    /// cardinally), so pipes and endpoint markers keep their geometry. Shape,
    /// not colour, identifies the choice: roots = tendrils, bridge = deck on
    /// stone piers, meadow = flower.
    /// </summary>
    public sealed class LandmarkGraphic : MaskableGraphic
    {
        private const float FrameFraction = 0.055f;
        private const float BadgeRadiusFraction = 0.165f;
        private const float BadgeInsetFraction = 0.08f;
        private const float FrameInset = 1.5f;
        private const int DiscSegments = 20;

        private int variant = 1;

        /// <summary>1 tree roots, 2 bridge foundation, 3 wildflower sanctuary.</summary>
        public void Configure(int landmarkVariant)
        {
            variant = landmarkVariant;
            raycastTarget = false;
            SetVerticesDirty();
        }

        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();
            Rect area = rectTransform.rect;
            float m = Mathf.Min(area.width, area.height);
            if (m <= 0f) return;

            Color32 frame = (Color32)MosseloraTheme.LandmarkFrame;
            Color32 plate = (Color32)MosseloraTheme.LandmarkPlate;

            float t = Mathf.Max(4f, m * FrameFraction);
            float l = area.xMin + FrameInset, r = area.xMax - FrameInset;
            float b = area.yMin + FrameInset, top = area.yMax - FrameInset;
            AddRect(vh, new Rect(l, top - t, r - l, t), frame);
            AddRect(vh, new Rect(l, b, r - l, t), frame);
            AddRect(vh, new Rect(l, b, t, top - b), frame);
            AddRect(vh, new Rect(r - t, b, t, top - b), frame);

            float radius = m * BadgeRadiusFraction;
            Vector2 centre = new Vector2(
                area.xMin + m * BadgeInsetFraction + radius,
                area.yMax - m * BadgeInsetFraction - radius);
            AddDisc(vh, centre, radius, frame);
            AddDisc(vh, centre, radius * 0.84f, plate);

            switch (variant)
            {
                case 1: DrawRoots(vh, centre, radius, frame); break;
                case 2: DrawFoundation(vh, centre, radius, frame); break;
                default: DrawFlowers(vh, centre, radius, frame, plate); break;
            }
        }

        // Tendrils curl outward from a root crown, tapering toward their tips.
        private static void DrawRoots(VertexHelper vh, Vector2 c, float s, Color32 ink)
        {
            AddStroke(vh, c, s, ink,
                new[] { new Vector2(0f, 0.5f), new Vector2(-0.2f, 0.0f), new Vector2(-0.5f, -0.2f), new Vector2(-0.6f, -0.55f) },
                new[] { 0.15f, 0.12f, 0.09f, 0.06f });
            AddStroke(vh, c, s, ink,
                new[] { new Vector2(0f, 0.5f), new Vector2(0.05f, 0.0f), new Vector2(0.0f, -0.6f) },
                new[] { 0.15f, 0.12f, 0.06f });
            AddStroke(vh, c, s, ink,
                new[] { new Vector2(0f, 0.5f), new Vector2(0.3f, 0.0f), new Vector2(0.5f, -0.2f), new Vector2(0.62f, -0.5f) },
                new[] { 0.15f, 0.12f, 0.09f, 0.06f });
        }

        // Stone footing carrying a plank deck: the bridge's protected foundation.
        private static void DrawFoundation(VertexHelper vh, Vector2 c, float s, Color32 ink)
        {
            AddRect(vh, Scaled(c, s, -0.52f, -0.66f, 0.52f, -0.5f), ink);
            AddRect(vh, Scaled(c, s, -0.46f, -0.5f, -0.18f, 0.08f), ink);
            AddRect(vh, Scaled(c, s, 0.18f, -0.5f, 0.46f, 0.08f), ink);
            AddRect(vh, Scaled(c, s, -0.6f, 0.08f, 0.6f, 0.3f), ink);
        }

        // Five petals around a plate-coloured heart: the wildflower sanctuary.
        private static void DrawFlowers(VertexHelper vh, Vector2 c, float s, Color32 ink, Color32 plate)
        {
            for (int petal = 0; petal < 5; petal++)
            {
                float angle = Mathf.PI / 2f + petal * 2f * Mathf.PI / 5f;
                Vector2 centre = c + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * (s * 0.36f);
                AddDisc(vh, centre, s * 0.2f, ink);
            }
            AddDisc(vh, c, s * 0.16f, plate);
        }

        private static Rect Scaled(Vector2 c, float s, float x0, float y0, float x1, float y1)
        {
            return new Rect(c.x + x0 * s, c.y + y0 * s, (x1 - x0) * s, (y1 - y0) * s);
        }

        private static void AddStroke(VertexHelper vh, Vector2 c, float s, Color32 shade,
                                      Vector2[] points, float[] widths)
        {
            for (int i = 0; i < points.Length; i++)
            {
                Vector2 p = c + points[i] * s;
                AddDisc(vh, p, widths[i] * s, shade);
                if (i == points.Length - 1) break;
                Vector2 q = c + points[i + 1] * s;
                Vector2 dir = (q - p).normalized;
                Vector2 normal = new Vector2(-dir.y, dir.x);
                float wp = widths[i] * s, wq = widths[i + 1] * s;
                int offset = vh.currentVertCount;
                vh.AddVert(p - normal * wp, shade, Vector2.zero);
                vh.AddVert(p + normal * wp, shade, Vector2.up);
                vh.AddVert(q + normal * wq, shade, Vector2.one);
                vh.AddVert(q - normal * wq, shade, Vector2.right);
                vh.AddTriangle(offset, offset + 1, offset + 2);
                vh.AddTriangle(offset, offset + 2, offset + 3);
            }
        }

        private static void AddDisc(VertexHelper vh, Vector2 centre, float radius, Color32 shade)
        {
            int start = vh.currentVertCount;
            vh.AddVert(centre, shade, Vector2.zero);
            for (int i = 0; i < DiscSegments; i++)
            {
                float angle = 2f * Mathf.PI * i / DiscSegments;
                vh.AddVert(centre + new Vector2(radius * Mathf.Cos(angle), radius * Mathf.Sin(angle)),
                    shade, Vector2.zero);
            }
            for (int i = 0; i < DiscSegments; i++)
                vh.AddTriangle(start, start + 1 + i, start + 1 + (i + 1) % DiscSegments);
        }

        private static void AddRect(VertexHelper vh, Rect box, Color32 shade)
        {
            int offset = vh.currentVertCount;
            vh.AddVert(new Vector3(box.xMin, box.yMin), shade, Vector2.zero);
            vh.AddVert(new Vector3(box.xMin, box.yMax), shade, Vector2.up);
            vh.AddVert(new Vector3(box.xMax, box.yMax), shade, Vector2.one);
            vh.AddVert(new Vector3(box.xMax, box.yMin), shade, Vector2.right);
            vh.AddTriangle(offset, offset + 1, offset + 2);
            vh.AddTriangle(offset, offset + 2, offset + 3);
        }
    }
}
