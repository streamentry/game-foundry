using System;
using System.Collections.Generic;
using System.Text;
using BidiSharp;

namespace Evergrove.Core
{
    /// <summary>
    /// Converts logical-order right-to-left text into the visual order a legacy
    /// uGUI Text draws left to right. Steps per line: Arabic contextual shaping
    /// (ArabicShaper), then Unicode bidi reordering (UAX #9 via the vendored
    /// BidiSharp) with the paragraph forced to RTL. Latin and digit runs keep
    /// their order. Wrapping happens here, before reordering, so each visual
    /// line is ordered on its own (UAX #9 rule L1/L2 apply per line).
    /// Applied only to ar, fa, ur and he; other locales never call this.
    /// </summary>
    public static class RtlText
    {
        private const int RtlParagraphLevel = 1;

        public static bool IsRtlScriptLocale(string code)
        {
            return code == "ar" || code == "fa" || code == "ur" || code == "he";
        }

        /// <summary>
        /// Shapes and reorders one line of logical text for display. The paragraph is
        /// forced to RTL only when the line contains Arabic or Hebrew letters, so
        /// Latin-only captions (for example a development label) keep their order and
        /// brackets.
        /// </summary>
        public static string Visual(string logicalLine)
        {
            if (string.IsNullOrEmpty(logicalLine)) return logicalLine ?? "";
            string shaped = ArabicShaper.Shape(logicalLine);
            int level = ContainsRtlLetter(logicalLine) ? RtlParagraphLevel : 0;
            return Bidi.LogicalToVisual(shaped, null, level);
        }

        // Hebrew, Arabic, Arabic Supplement, Arabic Presentation Forms-A and -B.
        private static bool ContainsRtlLetter(string text)
        {
            foreach (char c in text)
            {
                if ((c >= '֐' && c <= 'ࣿ') || (c >= 'יִ' && c <= '﷿') ||
                    (c >= 'ﹰ' && c <= '﻿'))
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Wraps logical text to <paramref name="maxWidth"/> and returns display text
        /// with explicit newlines, each line already in visual order. Source newlines
        /// start new paragraphs. <paramref name="measureWidth"/> receives a visual line
        /// and returns the width the engine would draw it at. A non-positive width
        /// disables wrapping.
        /// </summary>
        public static string Wrap(string logical, float maxWidth, Func<string, float> measureWidth)
        {
            if (string.IsNullOrEmpty(logical)) return logical ?? "";
            var lines = new List<string>();
            foreach (string paragraph in logical.Split('\n'))
                WrapParagraph(paragraph, maxWidth, measureWidth, lines);
            return string.Join("\n", lines);
        }

        private static void WrapParagraph(string paragraph, float maxWidth, Func<string, float> measureWidth,
                                          List<string> output)
        {
            var words = new List<string>();
            foreach (string word in paragraph.Split(' '))
                if (word.Length > 0) words.Add(word);
            if (words.Count == 0)
            {
                output.Add("");
                return;
            }

            bool wrap = maxWidth > 0f && measureWidth != null;
            var current = new StringBuilder(words[0]);
            for (int i = 1; i < words.Count; i++)
            {
                if (wrap)
                {
                    string candidate = current + " " + words[i];
                    if (measureWidth(Visual(candidate)) > maxWidth)
                    {
                        output.Add(Visual(current.ToString()));
                        current.Clear();
                        current.Append(words[i]);
                        continue;
                    }
                }
                current.Append(' ').Append(words[i]);
            }
            output.Add(Visual(current.ToString()));
        }
    }
}
