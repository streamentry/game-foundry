using System.Collections.Generic;
using Evergrove.Core;
using UnityEngine;
using UnityEngine.UI;

namespace Evergrove.UnityClient
{
    /// <summary>
    /// Keeps CJK closing punctuation off the start of wrapped lines in a legacy
    /// uGUI Text. The original source is kept and re-fitted whenever the width,
    /// font size or source changes, so manual breaks never go stale on resize.
    /// Only attached in CJK locales; other locales assign text directly.
    /// </summary>
    [RequireComponent(typeof(Text))]
    public sealed class CjkLineBreakFixer : MonoBehaviour
    {
        private Text target;
        private string source = "";
        private int appliedWidth = -1;
        private int appliedSize = -1;

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

        private void Refit(bool force)
        {
            if (target == null) return;
            Rect rect = target.rectTransform.rect;
            int width = Mathf.RoundToInt(rect.width);
            if (width <= 0)
            {
                // Not laid out yet: show the unbroken source until a real width arrives.
                if (target.text != source) target.text = source;
                appliedWidth = -1;
                return;
            }
            if (!force && width == appliedWidth && target.fontSize == appliedSize) return;
            appliedWidth = width;
            appliedSize = target.fontSize;

            Vector2 size = rect.size;
            Text measured = target;
            string fitted = KinsokuLineBreaks.Fit(source,
                display => MeasureLineStarts(measured, display, size));
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
    }
}
