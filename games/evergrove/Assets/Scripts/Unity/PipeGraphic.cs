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
            // The channel geometry remains readable without any color.
            Color32 outer = completed ? new Color32(28, 100, 84, 255) :
                hasWater ? new Color32(40, 117, 139, 255) :
                new Color32(137, 128, 110, 255);
            Color32 inner = hasWater ?
                new Color32(191, 229, 226, 255) :
                new Color32(225, 215, 190, 255);
            Rect area = rectTransform.rect;
            float half = Mathf.Min(area.width, area.height) * 0.44f;
            float stroke = Mathf.Max(12f, half * 0.32f);
            for (int direction = 0; direction < 4; direction++)
            {
                if (!PipeRules.HasPort(kind, orientation, direction)) continue;
                switch (direction)
                {
                    case 0:
                        AddRect(vh, new Rect(-stroke / 2f, 0, stroke, half), outer);
                        AddRect(vh, new Rect(-2f, 0, 4f, half * 0.90f), inner);
                        break;
                    case 1:
                        AddRect(vh, new Rect(0, -stroke / 2f, half, stroke), outer);
                        AddRect(vh, new Rect(0, -2f, half * 0.90f, 4f), inner);
                        break;
                    case 2:
                        AddRect(vh, new Rect(-stroke / 2f, -half, stroke, half), outer);
                        AddRect(vh, new Rect(-2f, -half * 0.90f, 4f, half * 0.90f), inner);
                        break;
                    case 3:
                        AddRect(vh, new Rect(-half, -stroke / 2f, half, stroke), outer);
                        AddRect(vh, new Rect(-half * 0.90f, -2f, half * 0.90f, 4f), inner);
                        break;
                }
            }
            AddRect(vh, new Rect(-stroke / 2f, -stroke / 2f, stroke, stroke), outer);
            if (kind == PipeKind.End)
                AddRect(vh, new Rect(-4f, -4f, 8f, 8f), new Color32(255, 245, 215, 255));
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
