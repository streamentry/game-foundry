using System;
using System.Text;

namespace Evergrove.Core
{
    /// <summary>
    /// Pre-orders Bengali text into the visual order a legacy uGUI Text draws left
    /// to right. uGUI performs no Indic shaping, so the pre-base vowel signs must be
    /// moved by hand: ি (U+09BF), ে (U+09C7) and ৈ (U+09C8) are written after the
    /// consonant they belong to but are drawn before it. The two-part vowels ো
    /// (U+09CB = U+09C7 + U+09BE) and ৌ (U+09CC = U+09C7 + U+09D7) are split first,
    /// so only the pre-base part moves and the post-base part stays after the
    /// consonant. A pre-base sign goes before the whole cluster (consonant, then
    /// any virama-joined consonants, then the nukta if present).
    /// Everything else keeps its logical order, including Latin, digits, line
    /// breaks and any sign without a consonant to attach to.
    /// Limits: this does not form conjunct ligatures (for example ক্ষ), reph or
    /// below-base forms. Those need a shaping engine that uGUI does not have.
    /// Applied only to bn; other locales never call this.
    /// References: Unicode Bengali block U+0980-U+09FF
    /// (https://www.unicode.org/charts/PDF/U0980.pdf); OpenType Indic shaping
    /// model, which defines the pre-base and two-part reordering this follows
    /// (https://learn.microsoft.com/typography/script-development/bengali).
    /// </summary>
    public static class IndicVisualOrder
    {
        private const char Nukta = '়';
        private const char VowelSignAa = 'া';
        private const char VowelSignI = 'ি';
        private const char VowelSignE = 'ে';
        private const char VowelSignAi = 'ৈ';
        private const char VowelSignO = 'ো';
        private const char VowelSignAu = 'ৌ';
        private const char AuLengthMark = 'ৗ';
        private const char Virama = '্';

        public static bool IsBengaliLocale(string code)
        {
            return code == "bn";
        }

        /// <summary>
        /// Returns the visual-order text for <paramref name="code"/>. Non-Bengali
        /// locales get the input back unchanged. Never throws: on any failure the
        /// input is returned.
        /// </summary>
        public static string Visual(string code, string logical)
        {
            if (!IsBengaliLocale(code)) return logical;
            return Visual(logical);
        }

        /// <summary>
        /// Reorders each line of Bengali text. Line breaks are kept exactly as given.
        /// </summary>
        public static string Visual(string logical)
        {
            if (string.IsNullOrEmpty(logical)) return logical ?? "";
            try
            {
                string[] lines = logical.Split('\n');
                for (int i = 0; i < lines.Length; i++) lines[i] = VisualLine(lines[i]);
                return string.Join("\n", lines);
            }
            catch (Exception)
            {
                // Reordering is cosmetic. Show the logical text rather than blank a label.
                return logical;
            }
        }

        private static string VisualLine(string line)
        {
            if (line.Length == 0) return line;
            var output = new StringBuilder(line.Length + 4);
            foreach (char c in line)
            {
                switch (c)
                {
                    case VowelSignI:
                    case VowelSignE:
                    case VowelSignAi:
                        AppendPreBase(output, c);
                        break;
                    case VowelSignO:
                        AppendTwoPart(output, c, VowelSignAa);
                        break;
                    case VowelSignAu:
                        AppendTwoPart(output, c, AuLengthMark);
                        break;
                    default:
                        output.Append(c);
                        break;
                }
            }
            return output.ToString();
        }

        // Places a pre-base sign before the cluster it attaches to. With no
        // consonant before it, the sign keeps its logical position.
        private static void AppendPreBase(StringBuilder output, char sign)
        {
            int start = ClusterStart(output);
            if (start < 0) output.Append(sign);
            else output.Insert(start, sign);
        }

        // ো and ৌ: the pre-base part (U+09C7) moves before the cluster, the
        // post-base part follows the consonant. With no consonant, the original
        // character is kept whole.
        private static void AppendTwoPart(StringBuilder output, char whole, char postBase)
        {
            int start = ClusterStart(output);
            if (start < 0)
            {
                output.Append(whole);
                return;
            }
            output.Insert(start, VowelSignE);
            output.Append(postBase);
        }

        // Index of the first code unit of the syllable cluster that ends the buffer,
        // or -1 when the buffer does not end in a consonant (optionally followed by
        // a nukta). A cluster is consonant, then virama-joined consonants
        // (C ্ C ্ C ...), so the start walks back across each virama.
        private static int ClusterStart(StringBuilder buffer)
        {
            int index = buffer.Length - 1;
            if (index < 0) return -1;
            if (buffer[index] == Nukta) index--;
            if (index < 0 || !IsConsonant(buffer[index])) return -1;

            int start = index;
            while (start >= 2 && buffer[start - 1] == Virama)
            {
                int previous = start - 2;
                if (buffer[previous] == Nukta) previous--;
                if (previous < 0 || !IsConsonant(buffer[previous])) break;
                start = previous;
            }
            return start;
        }

        // Bengali consonants: U+0995-U+09B9, plus the letters khanda-ta-free
        // extensions RRA, RHA, YYA, RA with diagonals (U+09DC, U+09DD, U+09DF,
        // U+09F0, U+09F1). Independent vowels (U+0985-U+0994) never take matras.
        private static bool IsConsonant(char c)
        {
            return (c >= 'ক' && c <= 'হ') ||
                   c == 'ড়' || c == 'ঢ়' || c == 'য়' ||
                   c == 'ৰ' || c == 'ৱ';
        }
    }
}
