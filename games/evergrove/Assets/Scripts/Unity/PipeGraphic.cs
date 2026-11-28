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

        /// <summary>Set from the gentle-motion setting; false snaps every turn.</summary>
        public static bool Animate = true;
        private bool configured;
        private float spin;            // remaining visual offset in degrees, eases to 0
        private const float SpinSpeed = 22f;

        private int openPorts; // bit d set: the port facing direction d has no partner

        public void Configure(PipeKind pipeKind, int rotation, bool sourceConnected, bool solved,
                              int unmatchedPorts = 0)
        {
            openPorts = unmatchedPorts;
            // A single quarter turn (tap or undo) swings into place instead of
            // snapping, so the player sees which way the channel moved. The mesh
            // is already in its final orientation; only the transform eases.
            if (configured && Animate && pipeKind == kind && rotation != orientation)
            {
                int step = ((rotation - orientation) % 4 + 4) % 4;
                if (step == 1) spin = 90f;        // clockwise turn: start a quarter back
                else if (step == 3) spin = -90f;  // undo: counter-clockwise
                else
                {
                    // Restart or larger jumps snap, including mid-swing.
                    spin = 0f;
                    rectTransform.localRotation = Quaternion.identity;
                }
            }
            configured = true;
            kind = pipeKind;
            orientation = rotation;
            completed = solved;
            hasWater = sourceConnected || solved;
            raycastTarget = false;
            SetVerticesDirty();
        }

        private void Update()
        {
            if (spin == 0f) return;
            spin = Mathf.Abs(spin) < 0.5f ? 0f
                : Mathf.Lerp(spin, 0f, 1f - Mathf.Exp(-SpinSpeed * Time.unscaledDeltaTime));
            rectTransform.localRotation = Quaternion.Euler(0f, 0f, spin);
        }

        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();
            // Dry pipes are warm/earthen. Only genuinely spring-connected
            // pipes receive teal water. A finished stream is the brightest state,
            // never a duller one: clearer teal and a sunlit stripe (I132).
            // Non-color cue (WCAG 1.4.1): connected channels carry a solid,
            // wider water stripe along each arm; dry channels carry a
            // segmented, broken stripe. Same arm geometry and hit area.
            Color32 outer = completed ? new Color32(22, 132, 146, 255) :
                hasWater ? (Color32)MosseloraTheme.WetPipe :
                (Color32)MosseloraTheme.DryChannel;
            Color32 collar = completed ? new Color32(16, 96, 108, 255) :
                hasWater ? new Color32(11, 56, 64, 255) : // a shade below WetPipe, as the old collar sat below WaterChannel
                new Color32(122, 67, 40, 255);
            Color32 inner = hasWater ?
                new Color32(205, 239, 240, 255) :
                new Color32(236, 197, 162, 255);
            if (completed) inner = new Color32(232, 251, 250, 255); // sunlit stripe (I132)
            Rect area = rectTransform.rect;
            float half = Mathf.Min(area.width, area.height) * 0.47f;
            float stroke = Mathf.Max(14f, half * 0.40f);
            float flowWidth = hasWater ? Mathf.Max(5f, stroke * 0.36f) : Mathf.Max(3f, stroke * 0.22f);
            float collarLength = stroke * 0.32f;
            float collarWidth = stroke * 1.32f;
            for (int direction = 0; direction < 4; direction++)
            {
                if (!PipeRules.HasPort(kind, orientation, direction)) continue;
                // Rotate one canonical arm (pointing up) into place: copper
                // tube, flowing/dry stripe, then a coupling collar at the rim.
                Vector2 along = Dir(direction);
                Vector2 across = new Vector2(along.y, -along.x);
                AddBar(vh, along, across, 0f, half, stroke, outer);
                // Painted tube shading: a soft light edge on the upper/left side
                // of each arm, so pipes read as rounded copper or glazed clay.
                Vector2 lit = (across.y > 0f || across.x < 0f) ? across : -across;
                Color32 shine = new Color32(255, 255, 255, 70);
                AddBar(vh, along, across, 0f, half * 0.92f, stroke * 0.16f, shine, lit * stroke * 0.30f);
                if (hasWater)
                    AddBar(vh, along, across, 0f, half * 0.90f, flowWidth, inner);
                else
                    AddDashes(vh, along, across, half * 0.86f, flowWidth, inner);
                AddBar(vh, along, across, half - collarLength, half, collarWidth, collar);
                AddBar(vh, along, across, half - collarLength * 0.62f, half - collarLength * 0.38f,
                    collarWidth, inner);
                if ((openPorts & (1 << direction)) != 0)
                {
                    // An open mouth where water would spill: a cream disc in a
                    // dark collar, drawn on the pipe end itself ("circles mark
                    // open ends"), not in a tile corner.
                    Vector2 mouth = along * (half - collarLength * 0.5f);
                    AddDisc(vh, stroke * 0.66f, collar, mouth);
                    AddDisc(vh, stroke * 0.42f, new Color32(255, 246, 226, 255), mouth);
                }
            }
            // Rounded heart where channels meet: a water-worn stone, not a
            // square plumbing junction. Keep true connectivity as the only
            // source of wet styling; this is visual geometry only.
            AddDisc(vh, stroke * 0.60f, outer);
            AddDisc(vh, kind == PipeKind.End ? stroke * 0.30f : stroke * 0.18f,
                kind == PipeKind.End ? new Color32(255, 245, 215, 255) : inner);
        }

        private static Vector2 Dir(int direction)
        {
            switch (direction)
            {
                case 0: return Vector2.up;
                case 1: return Vector2.right;
                case 2: return Vector2.down;
                default: return Vector2.left;
            }
        }

        /// <summary>A bar from <paramref name="from"/> to <paramref name="to"/> along an arm.</summary>
        private static void AddBar(VertexHelper vh, Vector2 along, Vector2 across,
                                   float from, float to, float width, Color32 shade)
        {
            AddBar(vh, along, across, from, to, width, shade, Vector2.zero);
        }

        private static void AddBar(VertexHelper vh, Vector2 along, Vector2 across,
                                   float from, float to, float width, Color32 shade, Vector2 shift)
        {
            Vector2 side = across * (width / 2f);
            Vector2 a = along * from + shift;
            Vector2 b = along * to + shift;
            int offset = vh.currentVertCount;
            vh.AddVert(a - side, shade, Vector2.zero);
            vh.AddVert(b - side, shade, Vector2.up);
            vh.AddVert(b + side, shade, Vector2.one);
            vh.AddVert(a + side, shade, Vector2.right);
            vh.AddTriangle(offset, offset + 1, offset + 2);
            vh.AddTriangle(offset, offset + 2, offset + 3);
        }

        /// <summary>
        /// Dry: three separated segments with gaps, so the state reads as
        /// broken or dry in grayscale (WCAG 1.4.1 non-color cue).
        /// </summary>
        private static void AddDashes(VertexHelper vh, Vector2 along, Vector2 across,
                                      float length, float width, Color32 shade)
        {
            const int segments = 3;
            const float gapRatio = 0.18f;
            float step = length / segments;
            float inset = step * gapRatio;
            for (int i = 0; i < segments; i++)
                AddBar(vh, along, across, step * i + inset, step * (i + 1) - inset, width, shade);
        }

        private static void AddDisc(VertexHelper vh, float radius, Color32 shade)
        {
            AddDisc(vh, radius, shade, Vector2.zero);
        }

        private static void AddDisc(VertexHelper vh, float radius, Color32 shade, Vector2 centre)
        {
            const int segments = 20;
            int start = vh.currentVertCount;
            vh.AddVert(centre, shade, Vector2.zero);
            for (int i = 0; i < segments; i++)
            {
                float angle = 2f * Mathf.PI * i / segments;
                vh.AddVert(new Vector3(centre.x + radius * Mathf.Cos(angle),
                    centre.y + radius * Mathf.Sin(angle), 0f), shade, Vector2.zero);
            }
            for (int i = 0; i < segments; i++)
                vh.AddTriangle(start, start + 1 + i, start + 1 + (i + 1) % segments);
        }
    }
}
