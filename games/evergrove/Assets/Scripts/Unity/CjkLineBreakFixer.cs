using System.Collections.Generic;
using Evergrove.Core;
using UnityEngine;
using UnityEngine.UI;

namespace Evergrove.UnityClient
{
    /// <summary>
    /// Fits a legacy uGUI Text to its width for locales that need it. CJK locales
    /// get kinsoku line-start rules. Right-to-left script locales (ar, fa, ur, he)
    /// get shaping, bidi reordering and per-line wrapping (RtlText), because uGUI
    /// draws text left to right and cannot reorder it. The logical source is kept
    /// and re-fitted whenever the width, font size or source changes.
    /// Only attached in locales returned by <see cref="NeedsFitting"/>; other
    /// locales assign text directly.
    /// </summary>
    [RequireComponent(typeof(Text))]
    public sealed class CjkLineBreakFixer : MonoBehaviour
    {
        private Text target;
        private string source = "";
        private int appliedWidth = -1;
        private int appliedSize = -1;

        /// <summary>True when the locale needs <see cref="Attach"/> rather than plain assignment.</summary>
        public static bool NeedsFitting(string code)
        {
            return KinsokuLineBreaks.IsCjkLocale(code) || RtlText.IsRtlScriptLocale(code);
        }

        /// <summary>Sets the source for a label and fits it to the current width.</summary>
        public static void Attach(Text text, string value)
        {
            if (text == null) return;
            CjkLineBreakFixer fixer = text.GetComponent<CjkLineBreakFixer>();
            if (fixer == null) fixer = text.gameObject.AddComponent<CjkLineBreakFixer>();
            fixer.Bind(text, value);
        }

        /// <summary>Updates a label's source; plain assignment when no fixer is attached.</summary>
        public static void SetText(Text text, string value)
        {
            if (text == null) return;
            CjkLineBreakFixer fixer = text.GetComponent<CjkLineBreakFixer>();
            if (fixer == null) text.text = value ?? "";
            else fixer.Bind(text, value);
        }

        private void Bind(Text text, string value)
        {
            target = text;
            source = value ?? "";
            Refit(true);
        }

        private void OnRectTransformDimensionsChange()
        {
            Refit(false);
        }

        private bool IsRtl()
        {
            return RtlText.IsRtlScriptLocale(GameLocalization.Current);
        }

        private void Refit(bool force)
        {
            if (target == null) return;
            Rect rect = target.rectTransform.rect;
            int width = Mathf.RoundToInt(rect.width);
            if (width <= 0)
            {
                // Not laid out yet: show the unfitted source until a real width arrives.
                string unfitted = IsRtl() ? RtlText.Visual(source) : source;
                if (target.text != unfitted) target.text = unfitted;
                appliedWidth = -1;
                return;
            }
            if (!force && width == appliedWidth && target.fontSize == appliedSize) return;
            appliedWidth = width;
            appliedSize = target.fontSize;

            Vector2 size = rect.size;
            Text measured = target;
            string fitted = IsRtl()
                ? RtlText.Wrap(source, rect.width, visual => MeasureWidth(measured, visual, size))
                : KinsokuLineBreaks.Fit(source, display => MeasureLineStarts(measured, display, size));
            if (target.text != fitted) target.text = fitted;
        }

        // Lays out a display string with the label's own settings and returns each
        // wrapped line's starting character index.
        private static IReadOnlyList<int> MeasureLineStarts(Text text, string display, Vector2 size)
        {
            var generator = new TextGenerator();
            generator.Populate(display, text.GetGenerationSettings(size));
            var starts = new List<int>(generator.lineCount);
            IList<UILineInfo> lines = generator.lines;
            for (int index = 0; index < lines.Count; index++)
                starts.Add(lines[index].startCharIdx);
            return starts;
        }

        // Unwrapped width of one visual line in the label's own font settings.
        private static float MeasureWidth(Text text, string display, Vector2 size)
        {
            var generator = new TextGenerator();
            TextGenerationSettings settings = text.GetGenerationSettings(Vector2.zero);
            settings.horizontalOverflow = HorizontalWrapMode.Overflow;
            return generator.GetPreferredWidth(display, settings) / text.pixelsPerUnit;
        }
    }
}
