using UnityEngine;
using UnityEngine.UI;

namespace Evergrove.UnityClient
{
    /// <summary>
    /// Procedural spring marker: three sinusoidal water strokes. Drawn as mesh
    /// so no font glyph is needed (a "≋" glyph fell back to a menu-like
    /// stack of bars in some builds). Shape, not color alone, carries meaning.
    /// </summary>
    public sealed class WaterWaveIcon : MaskableGraphic
    {
        private const int SamplesPerStroke = 24;
        private const int StrokeCount = 3;
        private const float CyclesAcrossWidth = 1.5f;

        public void Configure(Color waterColor)
        {
            color = waterColor;
            raycastTarget = false;
            SetVerticesDirty();
        }

        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();
            Rect area = rectTransform.rect;
            float width = area.width;
            float height = area.height;
            if (width <= 0f || height <= 0f) return;

            float thickness = Mathf.Max(2.5f, height * 0.11f);
            float amplitude = height * 0.07f;
            float rowSpacing = height / (StrokeCount + 1);
            for (int stroke = 0; stroke < StrokeCount; stroke++)
            {
                float baseY = area.yMin + rowSpacing * (stroke + 1);
                // Same phase keeps the strokes parallel (like ≋); staggered
                // phases made them cross and read as a braid in QA screenshots.
                const float phase = 0f;
                AddWaveStroke(vh, area.xMin, area.xMax, baseY, amplitude, thickness, phase);
            }
        }

        private void AddWaveStroke(VertexHelper vh, float xMin, float xMax, float baseY,
                                   float amplitude, float thickness, float phase)
        {
            float halfThickness = thickness * 0.5f;
            Vector3 previous = Point(xMin, xMax, baseY, amplitude, 0, phase);
            Vector3 previousNormal = Vector3.up;
            for (int i = 1; i <= SamplesPerStroke; i++)
            {
                Vector3 current = Point(xMin, xMax, baseY, amplitude, i, phase);
                Vector3 direction = (current - previous).normalized;
                Vector3 normal = new Vector3(-direction.y, direction.x, 0f);
                if (i == 1) previousNormal = normal;
                AddQuad(vh, previous, current, previousNormal, normal, halfThickness);
                previous = current;
                previousNormal = normal;
            }
        }

        private Vector3 Point(float xMin, float xMax, float baseY, float amplitude,
                              int sample, float phase)
        {
            float t = (float)sample / SamplesPerStroke;
            float x = Mathf.Lerp(xMin, xMax, t);
            float y = baseY + amplitude * Mathf.Sin(2f * Mathf.PI * CyclesAcrossWidth * t + phase);
            return new Vector3(x, y, 0f);
        }

        private void AddQuad(VertexHelper vh, Vector3 a, Vector3 b,
                             Vector3 normalA, Vector3 normalB, float halfThickness)
        {
            int offset = vh.currentVertCount;
            Color32 shade = color;
            vh.AddVert(a - normalA * halfThickness, shade, Vector2.zero);
            vh.AddVert(a + normalA * halfThickness, shade, Vector2.up);
            vh.AddVert(b + normalB * halfThickness, shade, Vector2.one);
            vh.AddVert(b - normalB * halfThickness, shade, Vector2.right);
            vh.AddTriangle(offset, offset + 1, offset + 2);
            vh.AddTriangle(offset, offset + 2, offset + 3);
        }
    }
}
