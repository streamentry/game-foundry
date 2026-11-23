using System;
using System.Collections.Generic;
using System.Text;

namespace Evergrove.Core
{
    /// <summary>
    /// CJK line-start rules (kinsoku). Legacy uGUI Text wraps Japanese and Chinese
    /// at any character, so closing punctuation can begin a line. uGUI ignores
    /// zero-width joiners, so the fix is an explicit line break placed one
    /// character earlier, which carries the preceding character down with the
    /// punctuation. Pure logic: the engine supplies measured line starts through
    /// a delegate, and the source string is never changed in place.
    /// </summary>
    public static class KinsokuLineBreaks
    {
        // Upper bound on measure/insert passes; each pass only adds breaks.
        public const int MaxPasses = 4;

        // Closing punctuation and small kana that must not begin a line.
        private const string NoLineStart = "。、，．！？：；」』）】〕》〉｝］”’ー・…‥ぁぃぅぇぉっゃゅょゎァィゥェォッャュョヮヵヶゝゞヽヾ々";

        // Opening brackets and quotes that must not end a line.
        private const string NoLineEnd = "「『（【〔《〈｛［“‘";

        public static bool IsCjkLocale(string code)
        {
            return code == "ja" || code == "zh-Hans" || code == "zh-Hant";
        }

        public static bool IsNoLineStart(char character)
        {
            return NoLineStart.IndexOf(character) >= 0;
        }

        public static bool IsNoLineEnd(char character)
        {
            return NoLineEnd.IndexOf(character) >= 0;
        }

        /// <summary>
        /// Given the source-index starts of the wrapped lines, returns the source
        /// indices before which a line break must be inserted (ascending, unique).
        /// The first line is never moved, and an existing explicit newline is
        /// never doubled.
        /// </summary>
        public static List<int> FindBreaksBefore(string source, IReadOnlyList<int> lineStarts)
        {
            var result = new List<int>();
            if (string.IsNullOrEmpty(source) || lineStarts == null) return result;
            for (int line = 1; line < lineStarts.Count; line++)
            {
                int start = lineStarts[line];
                int previousStart = lineStarts[line - 1];
                if (start <= 0 || start >= source.Length || previousStart < 0) continue;
                if (source[start - 1] == '\n') continue;

                bool closingAtStart = IsNoLineStart(source[start]);
                bool openingAtEnd = IsNoLineEnd(source[start - 1]);
                if (!closingAtStart && !openingAtEnd) continue;

                // Breaking before the preceding character must leave it on its own
                // line rather than create an empty line.
                int position = start - 1;
                if (position <= previousStart) continue;
                if (!result.Contains(position)) result.Add(position);
            }
            result.Sort();
            return result;
        }

        /// <summary>Returns the source with a line break inserted before each position.</summary>
        public static string Insert(string source, IEnumerable<int> breakBefore)
        {
            if (string.IsNullOrEmpty(source)) return source ?? "";
            var positions = new HashSet<int>();
            if (breakBefore != null)
                foreach (int position in breakBefore)
                    if (position > 0 && position < source.Length) positions.Add(position);
            if (positions.Count == 0) return source;

            var builder = new StringBuilder(source.Length + positions.Count);
            for (int index = 0; index < source.Length; index++)
            {
                if (positions.Contains(index)) builder.Append('\n');
                builder.Append(source[index]);
            }
            return builder.ToString();
        }

        /// <summary>
        /// Re-wraps the source until no line starts with forbidden punctuation.
        /// <paramref name="measureLineStarts"/> receives a display string (source
        /// plus inserted breaks) and returns the display index at which each
        /// wrapped line starts, as the engine lays it out.
        /// </summary>
        public static string Fit(string source, Func<string, IReadOnlyList<int>> measureLineStarts)
        {
            if (string.IsNullOrEmpty(source) || measureLineStarts == null) return source ?? "";
            var breaks = new SortedSet<int>();
            for (int pass = 0; pass < MaxPasses; pass++)
            {
                string display = Insert(source, breaks);
                IReadOnlyList<int> displayStarts = measureLineStarts(display);
                if (displayStarts == null || displayStarts.Count < 2) break;

                int[] sourceOf = SourceIndexMap(source.Length, breaks);
                var sourceStarts = new List<int>(displayStarts.Count);
                foreach (int displayIndex in displayStarts)
                {
                    bool valid = displayIndex >= 0 && displayIndex < sourceOf.Length;
                    sourceStarts.Add(valid ? sourceOf[displayIndex] : -1);
                }

                int added = 0;
                foreach (int position in FindBreaksBefore(source, sourceStarts))
                    if (breaks.Add(position)) added++;
                if (added == 0) break;
            }
            return Insert(source, breaks);
        }

        // Maps each display index to its source index, or -1 for an inserted break.
        private static int[] SourceIndexMap(int sourceLength, ICollection<int> breaks)
        {
            var map = new int[sourceLength + breaks.Count];
            var inserted = new HashSet<int>(breaks);
            int display = 0;
            for (int index = 0; index < sourceLength; index++)
            {
                if (inserted.Contains(index)) map[display++] = -1;
                map[display++] = index;
            }
            return map;
        }
    }
}
