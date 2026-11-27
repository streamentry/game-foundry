using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace Evergrove.Core
{
    /// <summary>
    /// Smart companion SC03: the reviewed words for facts, one bank per locale, shipped as data
    /// (Assets/Resources/Evergrove/Lines/&lt;locale&gt;.json). Separate from GameLocalization's
    /// catalogue. Shape: <c>{ "&lt;FactKind&gt;": { "0|1|2|any": ["line with {slots}", ...] } }</c>.
    /// Every line was read by a person before it was committed; nothing is generated at runtime.
    /// Immutable once parsed.
    /// </summary>
    public sealed class LineBank
    {
        public const string AnyVoice = "any";

        private readonly Dictionary<FactKind, Dictionary<string, string[]>> lines;

        public static readonly LineBank Empty = new LineBank(new Dictionary<FactKind, Dictionary<string, string[]>>());

        private LineBank(Dictionary<FactKind, Dictionary<string, string[]>> lines) { this.lines = lines; }

        public bool IsEmpty { get { return lines.Count == 0; } }

        public IEnumerable<FactKind> Kinds { get { return lines.Keys; } }

        /// <summary>The wordings for a kind in a voice ("0", "1", "2" or "any"); empty when none.</summary>
        public IReadOnlyList<string> Lines(FactKind kind, string voice)
        {
            return lines.TryGetValue(kind, out var voices) && voices.TryGetValue(voice, out string[] list)
                ? (IReadOnlyList<string>)Array.AsReadOnly(list) : Array.Empty<string>();
        }

        public IEnumerable<string> Voices(FactKind kind)
        {
            return lines.TryGetValue(kind, out var voices) ? voices.Keys : (IEnumerable<string>)Array.Empty<string>();
        }

        /// <summary>A speaker's wordings for a kind: their own voice first, else anyone's.</summary>
        public IReadOnlyList<string> For(FactKind kind, int speaker)
        {
            IReadOnlyList<string> own = Lines(kind, speaker.ToString(CultureInfo.InvariantCulture));
            return own.Count > 0 ? own : Lines(kind, AnyVoice);
        }

        /// <summary>
        /// The words for a fact, choosing a wording by <see cref="CompanionRanker.Variant"/>. Null when the
        /// bank has none for this kind and voice, or the wording would leave a slot unfilled (the caller
        /// then uses today's sentence). <paramref name="variant"/> is the wording used, or -1.
        /// </summary>
        public string Speak(Fact fact, int speaker, int garden, int day, CompanionMemory memory, out int variant)
        {
            return Speak(fact, speaker, garden, day, memory, -1, out variant);
        }

        /// <summary>
        /// As above, but keeps <paramref name="keep"/> (the wording already said this visit) when it is
        /// still a valid wording, so a reopened card never changes its words.
        /// </summary>
        public string Speak(Fact fact, int speaker, int garden, int day, CompanionMemory memory, int keep, out int variant)
        {
            variant = -1;
            if (fact == null) return null;
            IReadOnlyList<string> list = For(fact.Kind, speaker);
            if (list.Count == 0) return null;
            int chosen = keep >= 0 && keep < list.Count ? keep
                : CompanionRanker.Variant(fact.Kind, speaker, list.Count, garden, day, fact.Key, memory);
            string text = Fill(list[chosen], fact.Slots);
            if (text == null) return null;
            variant = chosen;
            return text;
        }

        /// <summary>Replaces every {slot}; null if any is missing or empty (a line never shows a brace).</summary>
        public static string Fill(string line, IReadOnlyDictionary<string, string> slots)
        {
            if (line == null) return null;
            var text = new StringBuilder(line.Length + 24);
            int i = 0;
            while (i < line.Length)
            {
                char c = line[i];
                if (c == '}') return null;
                if (c != '{') { text.Append(c); i++; continue; }
                int end = line.IndexOf('}', i + 1);
                if (end < 0) return null;
                string name = line.Substring(i + 1, end - i - 1);
                if (slots == null || !slots.TryGetValue(name, out string value) || string.IsNullOrEmpty(value)) return null;
                text.Append(value);
                i = end + 1;
            }
            return text.ToString();
        }

        /// <summary>The {slot} names a line uses, in order.</summary>
        public static List<string> SlotsIn(string line)
        {
            var names = new List<string>();
            int i = 0;
            while (line != null && (i = line.IndexOf('{', i)) >= 0)
            {
                int end = line.IndexOf('}', i + 1);
                if (end < 0) break;
                names.Add(line.Substring(i + 1, end - i - 1));
                i = end + 1;
            }
            return names;
        }

        /// <summary>
        /// Reads a bank. Unknown kinds or voices are an error (a typo would otherwise silently drop lines).
        /// Throws <see cref="FormatException"/> on anything that isn't the expected shape.
        /// </summary>
        public static LineBank Parse(string json)
        {
            var reader = new JsonReader(json ?? "");
            var result = new Dictionary<FactKind, Dictionary<string, string[]>>();
            reader.Expect('{');
            if (!reader.TryConsume('}'))
                do
                {
                    string kindName = reader.String();
                    if (!Enum.TryParse(kindName, out FactKind kind) || !Enum.IsDefined(typeof(FactKind), kind) || kindName != kind.ToString())
                        throw new FormatException("Unknown fact kind in line bank: " + kindName);
                    if (result.ContainsKey(kind)) throw new FormatException("Fact kind twice in line bank: " + kindName);
                    reader.Expect(':');
                    var voices = new Dictionary<string, string[]>();
                    reader.Expect('{');
                    if (!reader.TryConsume('}'))
                        do
                        {
                            string voice = reader.String();
                            if (voice != "0" && voice != "1" && voice != "2" && voice != AnyVoice)
                                throw new FormatException("Unknown voice in line bank: " + voice);
                            if (voices.ContainsKey(voice)) throw new FormatException("Voice twice: " + kindName + "/" + voice);
                            reader.Expect(':');
                            var list = new List<string>();
                            reader.Expect('[');
                            if (!reader.TryConsume(']'))
                                do list.Add(reader.String()); while (reader.TryConsume(','));
                            if (list.Count > 0) reader.Expect(']');
                            voices[voice] = list.ToArray();
                        } while (reader.TryConsume(','));
                    if (voices.Count > 0) reader.Expect('}');
                    result[kind] = voices;
                } while (reader.TryConsume(','));
            if (result.Count > 0) reader.Expect('}');
            reader.End();
            return new LineBank(result);
        }

        /// <summary>A small reader for the bank's one shape: objects, arrays and strings only.</summary>
        private sealed class JsonReader
        {
            private readonly string text;
            private int at;

            public JsonReader(string text) { this.text = text; }

            private void Skip()
            {
                while (at < text.Length && char.IsWhiteSpace(text[at])) at++;
                if (at == 0 && text.Length > 0 && text[0] == '﻿') { at++; Skip(); }
            }

            public void Expect(char c)
            {
                Skip();
                if (at >= text.Length || text[at] != c) throw new FormatException("Line bank: expected '" + c + "' at " + at);
                at++;
            }

            public bool TryConsume(char c)
            {
                Skip();
                if (at < text.Length && text[at] == c) { at++; return true; }
                return false;
            }

            public void End()
            {
                Skip();
                if (at != text.Length) throw new FormatException("Line bank: unexpected text at " + at);
            }

            public string String()
            {
                Expect('"');
                var s = new StringBuilder();
                while (true)
                {
                    if (at >= text.Length) throw new FormatException("Line bank: unterminated string");
                    char c = text[at++];
                    if (c == '"') return s.ToString();
                    if (c != '\\') { s.Append(c); continue; }
                    if (at >= text.Length) throw new FormatException("Line bank: bad escape");
                    char e = text[at++];
                    switch (e)
                    {
                        case '"': s.Append('"'); break;
                        case '\\': s.Append('\\'); break;
                        case '/': s.Append('/'); break;
                        case 'n': s.Append('\n'); break;
                        case 't': s.Append('\t'); break;
                        case 'u':
                            if (at + 4 > text.Length ||
                                !int.TryParse(text.Substring(at, 4), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out int code))
                                throw new FormatException("Line bank: bad \\u escape");
                            s.Append((char)code);
                            at += 4;
                            break;
                        default: throw new FormatException("Line bank: bad escape \\" + e);
                    }
                }
            }
        }
    }
}
