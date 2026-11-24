using Evergrove.Core;
using UnityEngine;
using UnityEngine.UI;

namespace Evergrove.UnityClient
{
    /// <summary>
    /// EG-006 iteration 7 prototype. For the bn locale only, a visible uGUI Text is
    /// replaced by a RawImage drawn by <see cref="NativeTextBridge"/>. The Text stays
    /// present but disabled and keeps the logical string, so layout code,
    /// ScreenReaderBridge and later direct text assignments still see the real
    /// text. Any change to text, width, size, colour, weight or alignment re-renders.
    /// On any native failure the Text is re-enabled and uGUI shows the label.
    /// </summary>
    [RequireComponent(typeof(Text))]
    public sealed class NativeTextLabel : MonoBehaviour
    {
        public const string Locale = "bn";

        private Text source;
        private RawImage image;
        private RectTransform imageRect;
        private LayoutElement layout;
        private Texture2D texture;
        private bool failed;
        private bool dirty = true;
        // True when the parent layout sizes this label to its content (e.g. the
        // chapter pill). A disabled Text reports no preferred width, which would
        // collapse such a parent to zero, so the width is supplied here instead.
        private bool sizesToContent;
        private string widthFor;

        // Last rendered state; a render happens only when one of these changes.
        private string lastText;
        private int lastWidth = -1;
        private float lastSize = -1f;
        private Color lastInk;
        private TextAnchor lastAlign;
        private bool lastBold;

        /// <summary>True when labels should use the native path right now.</summary>
        public static bool Applies
        {
            get { return NativeTextBridge.Supported && GameLocalization.Current == Locale; }
        }

        /// <summary>
        /// Makes this Text draw natively. The caller must already have set
        /// text.text to the logical (unreordered) string.
        /// </summary>
        public static void Attach(Text text)
        {
            if (text == null) return;
            NativeTextLabel label = text.GetComponent<NativeTextLabel>();
            if (label == null) label = text.gameObject.AddComponent<NativeTextLabel>();
            label.Bind(text);
        }

        private void Bind(Text text)
        {
            source = text;
            if (image != null) return;
            var node = new GameObject("Native text image", typeof(RectTransform));
            node.transform.SetParent(source.transform, false);
            imageRect = node.GetComponent<RectTransform>();
            // Full width, vertically centred, height set from the measured text.
            imageRect.anchorMin = new Vector2(0f, 0.5f);
            imageRect.anchorMax = new Vector2(1f, 0.5f);
            imageRect.pivot = new Vector2(0.5f, 0.5f);
            imageRect.anchoredPosition = Vector2.zero;
            imageRect.sizeDelta = Vector2.zero;
            image = node.AddComponent<RawImage>();
            image.raycastTarget = false;
            image.enabled = false;
            layout = source.GetComponent<LayoutElement>();
            if (layout == null) layout = source.gameObject.AddComponent<LayoutElement>();
            var row = source.transform.parent != null
                ? source.transform.parent.GetComponent<HorizontalLayoutGroup>() : null;
            sizesToContent = row != null && row.childControlWidth && !row.childForceExpandWidth;
            source.enabled = false;
            dirty = true;
        }

        // Margin over the uGUI estimate: native Bengali clusters can be a little
        // wider than uGUI's unshaped glyph run.
        private const float ContentWidthMargin = 1.12f;

        private void UpdateContentWidth()
        {
            string text = source.text ?? "";
            if (text == widthFor) return;
            widthFor = text;
            // Text.preferredWidth uses the text generator and works while disabled.
            layout.preferredWidth = text.Length == 0 ? -1f
                : Mathf.Ceil(source.preferredWidth * ContentWidthMargin);
            LayoutRebuilder.MarkLayoutForRebuild((RectTransform)source.transform);
        }

        private void OnRectTransformDimensionsChange()
        {
            dirty = true;
        }

        private void LateUpdate()
        {
            if (failed || source == null || image == null) return;
            if (sizesToContent) UpdateContentWidth();
            float scale = ScaleFactor();
            Rect rect = ((RectTransform)source.transform).rect;
            int widthPx = Mathf.RoundToInt(rect.width * scale);
            if (widthPx <= 0) return; // not laid out yet
            string text = source.text ?? "";
            float sizePx = source.fontSize * scale;
            bool bold = source.fontStyle == FontStyle.Bold || source.fontStyle == FontStyle.BoldAndItalic;
            if (!dirty && widthPx == lastWidth && Mathf.Approximately(sizePx, lastSize) &&
                text == lastText && source.color == lastInk &&
                source.alignment == lastAlign && bold == lastBold)
                return;
            dirty = false;
            lastText = text;
            lastWidth = widthPx;
            lastSize = sizePx;
            lastInk = source.color;
            lastAlign = source.alignment;
            lastBold = bold;
            Render(text, sizePx, widthPx, bold, scale);
        }

        private void Render(string text, float sizePx, int widthPx, bool bold, float scale)
        {
            if (string.IsNullOrEmpty(text))
            {
                ReleaseTexture();
                image.enabled = false;
                layout.preferredHeight = -1f;
                return;
            }
            Texture2D next = NativeTextBridge.Render(text, sizePx, widthPx, bold, lastInk, lastAlign);
            if (next == null)
            {
                Fail();
                return;
            }
            ReleaseTexture();
            texture = next;
            image.texture = texture;
            image.enabled = true;
            float heightUnits = texture.height / scale;
            imageRect.sizeDelta = new Vector2(0f, heightUnits);
            layout.preferredHeight = heightUnits;
        }

        private float ScaleFactor()
        {
            Canvas canvas = source.canvas != null ? source.canvas.rootCanvas : null;
            float scale = canvas != null ? canvas.scaleFactor : 1f;
            return scale > 0f ? scale : 1f;
        }

        // Falls back to uGUI Text for this label only, with the pre-shaping visual
        // order that the uGUI path has always used for bn.
        private void Fail()
        {
            failed = true;
            ReleaseTexture();
            image.enabled = false;
            layout.preferredHeight = -1f;
            if (sizesToContent) layout.preferredWidth = -1f;
            source.text = IndicVisualOrder.Visual(Locale, source.text);
            source.enabled = true;
            Debug.LogWarning("Mosselora native text unavailable; using uGUI Text for this label.");
        }

        private void ReleaseTexture()
        {
            if (texture == null) return;
            if (image != null) image.texture = null;
            Destroy(texture);
            texture = null;
        }

        private void OnDestroy()
        {
            ReleaseTexture();
        }
    }
}
