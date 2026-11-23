using Evergrove.Core;
using UnityEngine;
using UnityEngine.UI;

namespace Evergrove.UnityClient
{
    /// <summary>Procedurally drawn, readable pipe tile without fonts or external art.</summary>
    public sealed class PipeGraphic : MaskableGraphic
    {
        private PipeKind kind;
        private int orientation;
        private bool completed;
        private bool hasWater;

        public void Configure(PipeKind pipeKind, int rotation, bool sourceConnected, bool solved)
        {
            kind = pipeKind;
            orientation = rotation;
            completed = solved;
            hasWater = sourceConnected || solved;
            raycastTarget = false;
            SetVerticesDirty();
        }

        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();
            // Dry pipes are warm/earthen. Only genuinely spring-connected
            // pipes receive teal water, and completion uses deeper green.
            // Non-color cue (WCAG 1.4.1): connected channels carry a solid,
            // wider water stripe along each arm; dry channels carry a
            // segmented, broken stripe. Same arm geometry and hit area.
            Color32 outer = completed ? new Color32(39, 109, 84, 255) :
                hasWater ? new Color32(23, 108, 131, 255) :
                (Color32)MosseloraTheme.DryChannel;
            Color32 inner = hasWater ?
                new Color32(211, 239, 231, 255) :
                new Color32(239, 224, 194, 255);
            Rect area = rectTransform.rect;
            float half = Mathf.Min(area.width, area.height) * 0.44f;
            float stroke = Mathf.Max(12f, half * 0.32f);
            float flowWidth = hasWater ? Mathf.Max(5f, stroke * 0.42f) : Mathf.Max(3f, stroke * 0.22f);
            for (int direction = 0; direction < 4; direction++)
            {
                if (!PipeRules.HasPort(kind, orientation, direction)) continue;
                switch (direction)
                {
                    case 0:
                        AddRect(vh, new Rect(-stroke / 2f, 0, stroke, half), outer);
                        AddStripe(vh, new Rect(-flowWidth / 2f, 0, flowWidth, half * 0.90f), inner, hasWater, true);
                        break;
                    case 1:
                        AddRect(vh, new Rect(0, -stroke / 2f, half, stroke), outer);
                        AddStripe(vh, new Rect(0, -flowWidth / 2f, half * 0.90f, flowWidth), inner, hasWater, false);
                        break;
                    case 2:
                        AddRect(vh, new Rect(-stroke / 2f, -half, stroke, half), outer);
                        AddStripe(vh, new Rect(-flowWidth / 2f, -half * 0.90f, flowWidth, half * 0.90f), inner, hasWater, true);
                        break;
                    case 3:
                        AddRect(vh, new Rect(-half, -stroke / 2f, half, stroke), outer);
                        AddStripe(vh, new Rect(-half * 0.90f, -flowWidth / 2f, half * 0.90f, flowWidth), inner, hasWater, false);
                        break;
                }
            }
            // Rounded heart where channels meet: a water-worn stone, not a
            // square plumbing junction. Keep true connectivity as the only
            // source of wet styling; this is visual geometry only.
            AddDisc(vh, stroke * 0.60f, outer);
            AddDisc(vh, kind == PipeKind.End ? 4.5f : 3.0f,
                kind == PipeKind.End ? new Color32(255, 245, 215, 255) : inner);
        }

        private static void AddDisc(VertexHelper vh, float radius, Color32 shade)
        {
            const int segments = 16;
            int start = vh.currentVertCount;
            vh.AddVert(Vector3.zero, shade, Vector2.zero);
            for (int i = 0; i < segments; i++)
            {
                float angle = 2f * Mathf.PI * i / segments;
                vh.AddVert(new Vector3(radius * Mathf.Cos(angle),
                    radius * Mathf.Sin(angle), 0f), shade, Vector2.zero);
            }
            for (int i = 0; i < segments; i++)
                vh.AddTriangle(start, start + 1 + i, start + 1 + (i + 1) % segments);
        }

        /// <summary>
        /// Connected: one continuous water stripe. Dry: three separated
        /// segments with gaps, so the state reads as broken or dry in grayscale.
        /// </summary>
        private static void AddStripe(VertexHelper vh, Rect box, Color32 shade, bool flowing, bool vertical)
        {
            if (flowing)
            {
                AddRect(vh, box, shade);
                return;
            }
            const int segments = 3;
            const float gapRatio = 0.18f;
            float length = vertical ? box.height : box.width;
            float step = length / segments;
            float inset = step * gapRatio;
            for (int i = 0; i < segments; i++)
            {
                float from = step * i + inset;
                float to = step * (i + 1) - inset;
                Rect dash = vertical
                    ? new Rect(box.xMin, box.yMin + from, box.width, to - from)
                    : new Rect(box.xMin + from, box.yMin, to - from, box.height);
                AddRect(vh, dash, shade);
            }
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
