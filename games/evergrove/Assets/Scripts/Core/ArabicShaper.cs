using System;
using System.Collections.Generic;
using System.Text;

namespace Evergrove.Core
{
    /// <summary>
    /// Contextual shaping for Arabic-script letters (Arabic, Persian, Urdu).
    /// Replaces each joining letter with its Arabic Presentation Forms-B glyph
    /// (isolated, initial, medial or final) and merges lam-alef pairs into the
    /// ligature glyphs. Output stays in logical order; bidi reordering is a
    /// separate step (see RtlText).
    ///
    /// Joining types follow Unicode ArabicShaping.txt: D (dual joining), R (right
    /// joining) and U (non-joining). Combining marks are transparent. The table
    /// is limited to the letters used by the Arabic, Persian and Urdu catalogs;
    /// any other character is passed through unchanged.
    /// </summary>
    public static class ArabicShaper
    {
        private enum Joining : byte { NonJoining, Right, Dual }

        // Presentation forms for one base letter: isolated, final, initial, medial.
        // Right-joining letters have no initial or medial form; they use the
        // isolated and final entries only.
        private struct Letter
        {
            public readonly char Base;
            public readonly Joining Type;
            public readonly char Isolated;
            public readonly char Final;
            public readonly char Initial;
            public readonly char Medial;

            public Letter(char baseChar, Joining type, int isolated, int final, int initial, int medial)
            {
                Base = baseChar;
                Type = type;
                Isolated = (char)isolated;
                Final = (char)final;
                Initial = (char)initial;
                Medial = (char)medial;
            }
        }

        private static readonly Dictionary<char, Letter> Letters = BuildTable();

        // Lam followed by an alef variant forms one ligature (isolated or final).
        private static readonly Dictionary<char, KeyValuePair<char, char>> LamAlef = new Dictionary<char, KeyValuePair<char, char>>
        {
            { 'آ', new KeyValuePair<char, char>('ﻵ', 'ﻶ') }, // alef with madda above
            { 'أ', new KeyValuePair<char, char>('ﻷ', 'ﻸ') }, // alef with hamza above
            { 'إ', new KeyValuePair<char, char>('ﻹ', 'ﻺ') }, // alef with hamza below
            { 'ا', new KeyValuePair<char, char>('ﻻ', 'ﻼ') }, // alef
        };

        public static bool IsArabicLetter(char c)
        {
            return Letters.ContainsKey(c);
        }

        /// <summary>True for combining marks that do not break joining (tashkeel and Quranic marks).</summary>
        public static bool IsTransparent(char c)
        {
            return (c >= 'ؐ' && c <= 'ؚ') ||
                   (c >= 'ً' && c <= 'ٟ') ||
                   c == 'ٰ' ||
                   (c >= 'ۖ' && c <= 'ۜ') ||
                   (c >= '۟' && c <= 'ۤ') ||
                   c == 'ۧ' || c == 'ۨ' ||
                   (c >= '۪' && c <= 'ۭ');
        }

        /// <summary>Returns the logical-order text with joining letters replaced by presentation forms.</summary>
        public static string Shape(string text)
        {
            if (string.IsNullOrEmpty(text)) return text ?? "";
            var output = new StringBuilder(text.Length);
            var consumed = new bool[text.Length];

            for (int i = 0; i < text.Length; i++)
            {
                if (consumed[i]) continue;
                char c = text[i];
                Letter letter;
                if (!Letters.TryGetValue(c, out letter))
                {
                    output.Append(c);
                    continue;
                }

                int previous = PreviousLetter(text, i);
                int next = NextLetter(text, i);
                bool joinsBack = previous >= 0 && Letters[text[previous]].Type == Joining.Dual &&
                                 letter.Type != Joining.NonJoining;
                bool joinsForward = letter.Type == Joining.Dual && next >= 0 &&
                                    Letters[text[next]].Type != Joining.NonJoining;

                if (c == 'ل' && next >= 0 && LamAlef.ContainsKey(text[next]))
                {
                    // Lam-alef ligature: the ligature itself never joins forward.
                    KeyValuePair<char, char> pair = LamAlef[text[next]];
                    output.Append(joinsBack ? pair.Value : pair.Key);
                    consumed[next] = true;
                    continue;
                }

                output.Append(FormFor(letter, joinsBack, joinsForward));
            }
            return output.ToString();
        }

        private static char FormFor(Letter letter, bool joinsBack, bool joinsForward)
        {
            if (joinsBack && joinsForward) return letter.Medial;
            if (joinsBack) return letter.Final;
            if (joinsForward) return letter.Initial;
            return letter.Isolated;
        }

        // Index of the nearest earlier character that is not a transparent mark, or -1.
        private static int PreviousLetter(string text, int index)
        {
            for (int j = index - 1; j >= 0; j--)
            {
                if (IsTransparent(text[j])) continue;
                return Letters.ContainsKey(text[j]) ? j : -1;
            }
            return -1;
        }

        // Index of the nearest later character that is not a transparent mark, or -1.
        private static int NextLetter(string text, int index)
        {
            for (int j = index + 1; j < text.Length; j++)
            {
                if (IsTransparent(text[j])) continue;
                return Letters.ContainsKey(text[j]) ? j : -1;
            }
            return -1;
        }

        private static Dictionary<char, Letter> BuildTable()
        {
            const Joining D = Joining.Dual;
            const Joining R = Joining.Right;
            var list = new List<Letter>
            {
                new Letter('آ', R, 0xFE81, 0xFE82, 0xFE81, 0xFE82), // alef with madda
                new Letter('أ', R, 0xFE83, 0xFE84, 0xFE83, 0xFE84), // alef with hamza above
                new Letter('ؤ', R, 0xFE85, 0xFE86, 0xFE85, 0xFE86), // waw with hamza
                new Letter('إ', R, 0xFE87, 0xFE88, 0xFE87, 0xFE88), // alef with hamza below
                new Letter('ئ', D, 0xFE89, 0xFE8A, 0xFE8B, 0xFE8C), // yeh with hamza
                new Letter('ا', R, 0xFE8D, 0xFE8E, 0xFE8D, 0xFE8E), // alef
                new Letter('ب', D, 0xFE8F, 0xFE90, 0xFE91, 0xFE92), // beh
                new Letter('ة', R, 0xFE93, 0xFE94, 0xFE93, 0xFE94), // teh marbuta
                new Letter('ت', D, 0xFE95, 0xFE96, 0xFE97, 0xFE98), // teh
                new Letter('ث', D, 0xFE99, 0xFE9A, 0xFE9B, 0xFE9C), // theh
                new Letter('ج', D, 0xFE9D, 0xFE9E, 0xFE9F, 0xFEA0), // jeem
                new Letter('ح', D, 0xFEA1, 0xFEA2, 0xFEA3, 0xFEA4), // hah
                new Letter('خ', D, 0xFEA5, 0xFEA6, 0xFEA7, 0xFEA8), // khah
                new Letter('د', R, 0xFEA9, 0xFEAA, 0xFEA9, 0xFEAA), // dal
                new Letter('ذ', R, 0xFEAB, 0xFEAC, 0xFEAB, 0xFEAC), // thal
                new Letter('ر', R, 0xFEAD, 0xFEAE, 0xFEAD, 0xFEAE), // reh
                new Letter('ز', R, 0xFEAF, 0xFEB0, 0xFEAF, 0xFEB0), // zain
                new Letter('س', D, 0xFEB1, 0xFEB2, 0xFEB3, 0xFEB4), // seen
                new Letter('ش', D, 0xFEB5, 0xFEB6, 0xFEB7, 0xFEB8), // sheen
                new Letter('ص', D, 0xFEB9, 0xFEBA, 0xFEBB, 0xFEBC), // sad
                new Letter('ض', D, 0xFEBD, 0xFEBE, 0xFEBF, 0xFEC0), // dad
                new Letter('ط', D, 0xFEC1, 0xFEC2, 0xFEC3, 0xFEC4), // tah
                new Letter('ظ', D, 0xFEC5, 0xFEC6, 0xFEC7, 0xFEC8), // zah
                new Letter('ع', D, 0xFEC9, 0xFECA, 0xFECB, 0xFECC), // ain
                new Letter('غ', D, 0xFECD, 0xFECE, 0xFECF, 0xFED0), // ghain
                new Letter('ف', D, 0xFED1, 0xFED2, 0xFED3, 0xFED4), // feh
                new Letter('ق', D, 0xFED5, 0xFED6, 0xFED7, 0xFED8), // qaf
                new Letter('ك', D, 0xFED9, 0xFEDA, 0xFEDB, 0xFEDC), // kaf
                new Letter('ل', D, 0xFEDD, 0xFEDE, 0xFEDF, 0xFEE0), // lam
                new Letter('م', D, 0xFEE1, 0xFEE2, 0xFEE3, 0xFEE4), // meem
                new Letter('ن', D, 0xFEE5, 0xFEE6, 0xFEE7, 0xFEE8), // noon
                new Letter('ه', D, 0xFEE9, 0xFEEA, 0xFEEB, 0xFEEC), // heh
                new Letter('و', R, 0xFEED, 0xFEEE, 0xFEED, 0xFEEE), // waw
                new Letter('ى', R, 0xFEEF, 0xFEF0, 0xFEEF, 0xFEF0), // alef maksura
                new Letter('ي', D, 0xFEF1, 0xFEF2, 0xFEF3, 0xFEF4), // yeh
                // Persian and Urdu letters
                new Letter('پ', D, 0xFB56, 0xFB57, 0xFB58, 0xFB59), // peh
                new Letter('ٹ', D, 0xFB66, 0xFB67, 0xFB68, 0xFB69), // tteh
                new Letter('چ', D, 0xFB7A, 0xFB7B, 0xFB7C, 0xFB7D), // tcheh
                new Letter('ڈ', R, 0xFB88, 0xFB89, 0xFB88, 0xFB89), // ddal
                new Letter('ڑ', R, 0xFB8C, 0xFB8D, 0xFB8C, 0xFB8D), // rreh
                new Letter('ژ', R, 0xFB8A, 0xFB8B, 0xFB8A, 0xFB8B), // jeh
                new Letter('ک', D, 0xFB8E, 0xFB8F, 0xFB90, 0xFB91), // keheh (Persian kaf)
                new Letter('گ', D, 0xFB92, 0xFB93, 0xFB94, 0xFB95), // gaf
                new Letter('ں', R, 0xFB9E, 0xFB9F, 0xFB9E, 0xFB9F), // noon ghunna
                new Letter('ھ', D, 0xFBAA, 0xFBAB, 0xFBAC, 0xFBAD), // heh doachashmee
                new Letter('ۀ', R, 0xFBA4, 0xFBA5, 0xFBA4, 0xFBA5), // heh with yeh above
                new Letter('ہ', D, 0xFBA6, 0xFBA7, 0xFBA8, 0xFBA9), // heh goal
                new Letter('ی', D, 0xFBFC, 0xFBFD, 0xFBFE, 0xFBFF), // farsi yeh
                new Letter('ے', R, 0xFBAE, 0xFBAF, 0xFBAE, 0xFBAF), // yeh barree
            };
            var table = new Dictionary<char, Letter>();
            foreach (Letter letter in list) table.Add(letter.Base, letter);
            return table;
        }
    }
}
