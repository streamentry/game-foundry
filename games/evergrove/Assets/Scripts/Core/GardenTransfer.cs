using System;
using System.Collections.Generic;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;

namespace Evergrove.Core
{
    public enum GardenImportError
    {
        None = 0,
        Empty,
        TooLarge,
        Malformed,
        WrongFormat,
        UnsupportedFutureVersion,
        ChecksumMismatch,
        InvalidSnapshot
    }

    /// <summary>
    /// Player-exportable garden file, format v1. Pure C#: the caller supplies the
    /// timestamp and no clock is read. The SHA-256 checksum detects accidental
    /// corruption only. It is NOT tamper-proofing, because anyone can recompute it.
    /// Snapshot text is produced in the same field order as Unity JsonUtility.ToJson
    /// for WorldSnapshot, so the embedded snapshot matches the local save format.
    /// JSON is written and parsed by hand so the same code runs in Unity and .NET 8.
    /// </summary>
    public static class GardenTransfer
    {
        public const string FormatName = "mosselora-garden";
        public const int CurrentFormatVersion = 1;
        public const int MaxImportBytes = 1024 * 1024;
        private const int MaxExportedAtLength = 64;
        private const int MaxNestingDepth = 16;

        private static readonly HashSet<string> SnapshotKeys = new HashSet<string> {
            "version", "mode", "round", "solved", "rotations", "previous", "choices",
            "hintsUsed", "boardVariant", "lastSolvedVariant", "hintStep", "hintTile", "variantSchedule",
            "history", "garden", "placements", "plantings", "seeds", "treeSolved", "bought", "boughtSolved",
            "gifts", "parcelIds", "letters", "letterFrom", "helped", "helpedFrom",
            "fruit", "fruitGiven", "lastFruitTo", "together", "lanterns", "jam"
        };

        public static string Export(WorldModel world, string exportedAtUtcIso)
        {
            if (world == null) throw new ArgumentNullException("world");
            return ExportSnapshot(world.Export(), exportedAtUtcIso);
        }

        internal static string ExportSnapshot(WorldSnapshot snapshot, string exportedAtUtcIso)
        {
            if (snapshot == null) throw new ArgumentNullException("snapshot");
            if (string.IsNullOrWhiteSpace(exportedAtUtcIso) || exportedAtUtcIso.Length > MaxExportedAtLength)
                throw new ArgumentException("A non-empty exportedAtUtc timestamp of at most 64 characters is required.", "exportedAtUtcIso");

            string snapshotJson = SnapshotJson(snapshot);
            return "{\"format\":" + Quote(FormatName) +
                ",\"formatVersion\":" + Int(CurrentFormatVersion) +
                ",\"exportedAtUtc\":" + Quote(exportedAtUtcIso) +
                ",\"checksumSha256\":\"" + Sha256Hex(snapshotJson) + "\"" +
                ",\"snapshot\":" + snapshotJson + "}";
        }

        /// <summary>
        /// Import goes through the same validation as local saves. Errors are checked
        /// in order: size, JSON syntax, envelope identity, future version, snapshot
        /// shape, checksum, then WorldModel.TryRestore rules.
        /// </summary>
        public static bool TryImport(string text, out WorldModel world, out GardenImportError error)
        {
            world = null;
            error = GardenImportError.None;
            if (text == null || text.Trim().Length == 0)
            {
                error = GardenImportError.Empty;
                return false;
            }
            if (Encoding.UTF8.GetByteCount(text) > MaxImportBytes)
            {
                error = GardenImportError.TooLarge;
                return false;
            }

            object root;
            if (!Parser.TryParse(text, out root))
            {
                error = GardenImportError.Malformed;
                return false;
            }
            var envelope = root as Dictionary<string, object>;
            if (envelope == null)
            {
                error = GardenImportError.Malformed;
                return false;
            }

            object formatValue;
            string format = envelope.TryGetValue("format", out formatValue) ? formatValue as string : null;
            if (format != FormatName)
            {
                error = GardenImportError.WrongFormat;
                return false;
            }

            int formatVersion;
            if (!ReadRequiredInt(envelope, "formatVersion", out formatVersion) || formatVersion < 1)
            {
                error = GardenImportError.WrongFormat;
                return false;
            }
            if (formatVersion > CurrentFormatVersion)
            {
                error = GardenImportError.UnsupportedFutureVersion;
                return false;
            }

            string exportedAt;
            string checksum;
            if (!ReadRequiredString(envelope, "exportedAtUtc", out exportedAt) ||
                !ReadRequiredString(envelope, "checksumSha256", out checksum))
            {
                error = GardenImportError.Malformed;
                return false;
            }

            Dictionary<string, object> snapshotFields;
            WorldSnapshot snapshot;
            if (!ReadRequiredObject(envelope, "snapshot", out snapshotFields) ||
                !TryReadSnapshot(snapshotFields, out snapshot))
            {
                error = GardenImportError.InvalidSnapshot;
                return false;
            }

            // Gardens exported before EG-111 have no "history" key in their checksummed text.
            // Accept that exact pre-history canonical form only when the import has no history.
            bool matchesCurrent = string.Equals(Sha256Hex(SnapshotJson(snapshot)), checksum, StringComparison.Ordinal);
            // Gardens exported before E4 (I177) have no "garden"/"placements" keys.
            // Gardens exported before I213 have no "gifts"/"parcelIds"/"letters"/"letterFrom" keys.
            // Gardens exported before I226 have no "helped"/"helpedFrom" keys.
            // Gardens exported before I228 have no "fruit"/"fruitGiven"/"lastFruitTo" keys.
            // Gardens exported before I263 have no "together" key.
            // Gardens exported before lantern parcels have no "lanterns" key.
            bool noLanternsYet = snapshot.lanterns == null || snapshot.lanterns.Length == 0;
            bool noTogetherYet = snapshot.together.Length == 0 && noLanternsYet;
            bool noFruitYet = snapshot.fruit == 0 && snapshot.lastFruitTo == -1 && AllZero(snapshot.fruitGiven) && noTogetherYet;
            bool noHelpYet = snapshot.helped.Length == 0 && snapshot.helpedFrom.Length == 0 && noFruitYet;
            bool noFamilyYet = snapshot.gifts.Length == 0 && snapshot.parcelIds.Length == 0 &&
                snapshot.letters.Length == 0 && snapshot.letterFrom.Length == 0 && noHelpYet;
            bool noNurseryYet = snapshot.bought.Length == 0 && snapshot.boughtSolved.Length == 0 && noFamilyYet;
            bool noTreesYet = snapshot.seeds == 0 && snapshot.treeSolved.Length == 0 && noNurseryYet;
            bool noMapYet = snapshot.garden == 0 && snapshot.placements.Length == 0 && snapshot.plantings.Length == 0 && noTreesYet;
            // Gardens exported before I193 have no "plantings" key.
            bool matchesPrePlantings = snapshot.plantings.Length == 0 && noTreesYet &&
                string.Equals(Sha256Hex(SnapshotJson(snapshot, true, true, false, false, false)), checksum, StringComparison.Ordinal);
            bool matchesPreLanterns = noLanternsYet &&
                string.Equals(Sha256Hex(SnapshotJson(snapshot, true, true, true, true, true, true, true, true, true, false)), checksum, StringComparison.Ordinal);
            bool matchesPreTogether = noTogetherYet &&
                string.Equals(Sha256Hex(SnapshotJson(snapshot, true, true, true, true, true, true, true, true, false)), checksum, StringComparison.Ordinal);
            bool matchesPreFruit = noFruitYet &&
                string.Equals(Sha256Hex(SnapshotJson(snapshot, true, true, true, true, true, true, true, false, false)), checksum, StringComparison.Ordinal);
            bool matchesPreHelp = noHelpYet &&
                string.Equals(Sha256Hex(SnapshotJson(snapshot, true, true, true, true, true, true, false, false, false)), checksum, StringComparison.Ordinal);
            bool matchesPreFamily = noFamilyYet &&
                string.Equals(Sha256Hex(SnapshotJson(snapshot, true, true, true, true, true, false, false, false, false)), checksum, StringComparison.Ordinal);
            // Gardens exported before I205 have no "bought"/"boughtSolved" keys; seeds may be set.
            bool matchesPreNursery = noNurseryYet &&
                string.Equals(Sha256Hex(SnapshotJson(snapshot, true, true, true, true, false)), checksum, StringComparison.Ordinal);
            // Gardens exported before I204 have no "seeds"/"treeSolved" keys.
            bool matchesPreTrees = noTreesYet &&
                string.Equals(Sha256Hex(SnapshotJson(snapshot, true, true, true, false, false)), checksum, StringComparison.Ordinal);
            bool matchesPreMap = noMapYet &&
                string.Equals(Sha256Hex(SnapshotJson(snapshot, true, false)), checksum, StringComparison.Ordinal);
            bool matchesPreHistory = noMapYet && snapshot.history.Length == 0 &&
                string.Equals(Sha256Hex(SnapshotJson(snapshot, false, false)), checksum, StringComparison.Ordinal);
            if (!matchesCurrent && !matchesPreLanterns && !matchesPreTogether && !matchesPreFruit && !matchesPreHelp && !matchesPreFamily && !matchesPreNursery && !matchesPreTrees && !matchesPrePlantings && !matchesPreMap && !matchesPreHistory)
            {
                error = GardenImportError.ChecksumMismatch;
                return false;
            }

            if (!WorldModel.TryRestore(snapshot, out world))
            {
                world = null;
                error = GardenImportError.InvalidSnapshot;
                return false;
            }
            return true;
        }

        /// <summary>Field order mirrors Unity JsonUtility.ToJson(WorldSnapshot).</summary>
        internal static string SnapshotJson(WorldSnapshot s)
        {
            return SnapshotJson(s, true, true);
        }

        /// <summary>
        /// Canonical text. With includeHistory false it reproduces the pre-EG-111
        /// layout, used only to verify checksums of older garden files.
        /// </summary>
        private static string SnapshotJson(WorldSnapshot s, bool includeHistory, bool includeMap, bool includePlantings = true,
                                           bool includeTrees = true, bool includeNursery = true, bool includeFamily = true,
                                           bool includeHelp = true, bool includeFruit = true, bool includeTogether = true,
                                           bool includeLanterns = true)
        {
            var sb = new StringBuilder(256);
            sb.Append("{\"version\":").Append(Int(s.version))
              .Append(",\"mode\":").Append(Quote(s.mode))
              .Append(",\"round\":").Append(Int(s.round))
              .Append(",\"solved\":").Append(s.solved ? "true" : "false")
              .Append(",\"rotations\":").Append(IntArray(s.rotations))
              .Append(",\"previous\":").Append(IntArray(s.previous))
              .Append(",\"choices\":").Append(IntArray(s.choices))
              .Append(",\"hintsUsed\":").Append(Int(s.hintsUsed))
              .Append(",\"boardVariant\":").Append(Int(s.boardVariant))
              .Append(",\"lastSolvedVariant\":").Append(Int(s.lastSolvedVariant))
              .Append(",\"hintStep\":").Append(Int(s.hintStep))
              .Append(",\"hintTile\":").Append(Int(s.hintTile))
              .Append(",\"variantSchedule\":").Append(IntArray(s.variantSchedule));
            if (includeHistory) sb.Append(",\"history\":").Append(IntArray(s.history));
            if (includeMap)
                sb.Append(",\"garden\":").Append(Int(s.garden))
                  .Append(",\"placements\":").Append(IntArray(s.placements));
            if (includeMap && includePlantings)
                sb.Append(",\"plantings\":").Append(IntArray(s.plantings));
            if (includeMap && includePlantings && includeTrees)
                sb.Append(",\"seeds\":").Append(Int(s.seeds))
                  .Append(",\"treeSolved\":").Append(IntArray(s.treeSolved));
            if (includeMap && includePlantings && includeTrees && includeNursery)
                sb.Append(",\"bought\":").Append(IntArray(s.bought))
                  .Append(",\"boughtSolved\":").Append(IntArray(s.boughtSolved));
            if (includeMap && includePlantings && includeTrees && includeNursery && includeFamily)
                sb.Append(",\"gifts\":").Append(IntArray(s.gifts))
                  .Append(",\"parcelIds\":").Append(StringArray(s.parcelIds))
                  .Append(",\"letters\":").Append(StringArray(s.letters))
                  .Append(",\"letterFrom\":").Append(StringArray(s.letterFrom));
            if (includeMap && includePlantings && includeTrees && includeNursery && includeFamily && includeHelp)
                sb.Append(",\"helped\":").Append(IntArray(s.helped))
                  .Append(",\"helpedFrom\":").Append(StringArray(s.helpedFrom));
            if (includeMap && includePlantings && includeTrees && includeNursery && includeFamily && includeHelp && includeFruit)
                sb.Append(",\"fruit\":").Append(Int(s.fruit))
                  .Append(",\"fruitGiven\":").Append(IntArray(s.fruitGiven))
                  .Append(",\"lastFruitTo\":").Append(Int(s.lastFruitTo));
            if (includeMap && includePlantings && includeTrees && includeNursery && includeFamily && includeHelp && includeFruit && includeTogether)
                sb.Append(",\"together\":").Append(IntArray(s.together));
            if (includeMap && includePlantings && includeTrees && includeNursery && includeFamily && includeHelp && includeFruit && includeTogether && includeLanterns)
                sb.Append(",\"lanterns\":").Append(StringArray(s.lanterns));
            // Newer keys are written only when they hold something, so every older garden's text and checksum
            // stay byte-identical and no explicit "pre" step is needed for them.
            if (includeMap && includePlantings && includeTrees && includeNursery && includeFamily && includeHelp && includeFruit && includeTogether && includeLanterns)
            {
                if (s.jam != 0) sb.Append(",\"jam\":").Append(Int(s.jam));
            }
            return sb.Append('}').ToString();
        }

        internal static string Sha256Hex(string text)
        {
            using (SHA256 sha = SHA256.Create())
            {
                byte[] hash = sha.ComputeHash(Encoding.UTF8.GetBytes(text));
                var sb = new StringBuilder(hash.Length * 2);
                foreach (byte b in hash) sb.Append(b.ToString("x2", CultureInfo.InvariantCulture));
                return sb.ToString();
            }
        }

        private static string StringArray(string[] values)
        {
            if (values == null) return "null";
            var sb = new StringBuilder();
            sb.Append('[');
            for (int i = 0; i < values.Length; i++)
            {
                if (i > 0) sb.Append(',');
                sb.Append(Quote(values[i]));
            }
            return sb.Append(']').ToString();
        }

        private static bool AllZero(int[] values)
        {
            if (values == null) return true;
            foreach (int v in values) if (v != 0) return false;
            return true;
        }

        internal static string Int(int value)
        {
            return value.ToString(CultureInfo.InvariantCulture);
        }

        private static string IntArray(int[] values)
        {
            if (values == null) return "null";
            var sb = new StringBuilder();
            sb.Append('[');
            for (int i = 0; i < values.Length; i++)
            {
                if (i > 0) sb.Append(',');
                sb.Append(Int(values[i]));
            }
            return sb.Append(']').ToString();
        }

        internal static string Quote(string value)
        {
            if (value == null) return "null";
            var sb = new StringBuilder(value.Length + 2);
            sb.Append('"');
            foreach (char c in value)
            {
                switch (c)
                {
                    case '"': sb.Append("\\\""); break;
                    case '\\': sb.Append("\\\\"); break;
                    case '\n': sb.Append("\\n"); break;
                    case '\r': sb.Append("\\r"); break;
                    case '\t': sb.Append("\\t"); break;
                    default:
                        if (c < 0x20) sb.Append("\\u").Append(((int)c).ToString("x4", CultureInfo.InvariantCulture));
                        else sb.Append(c);
                        break;
                }
            }
            return sb.Append('"').ToString();
        }

        private static bool TryReadSnapshot(Dictionary<string, object> fields, out WorldSnapshot snapshot)
        {
            // Absent fields keep WorldSnapshot defaults, as JsonUtility does.
            snapshot = new WorldSnapshot();
            foreach (string key in fields.Keys)
                if (!SnapshotKeys.Contains(key)) return false;
            return ReadInt(fields, "version", ref snapshot.version) &&
                   ReadString(fields, "mode", ref snapshot.mode) &&
                   ReadInt(fields, "round", ref snapshot.round) &&
                   ReadBool(fields, "solved", ref snapshot.solved) &&
                   ReadInts(fields, "rotations", ref snapshot.rotations) &&
                   ReadInts(fields, "previous", ref snapshot.previous) &&
                   ReadInts(fields, "choices", ref snapshot.choices) &&
                   ReadInt(fields, "hintsUsed", ref snapshot.hintsUsed) &&
                   ReadInt(fields, "boardVariant", ref snapshot.boardVariant) &&
                   ReadInt(fields, "lastSolvedVariant", ref snapshot.lastSolvedVariant) &&
                   ReadInt(fields, "hintStep", ref snapshot.hintStep) &&
                   ReadInt(fields, "hintTile", ref snapshot.hintTile) &&
                   ReadInts(fields, "variantSchedule", ref snapshot.variantSchedule) &&
                   ReadInts(fields, "history", ref snapshot.history) &&
                   ReadInt(fields, "garden", ref snapshot.garden) &&
                   ReadInts(fields, "placements", ref snapshot.placements) &&
                   ReadInts(fields, "plantings", ref snapshot.plantings) &&
                   ReadInt(fields, "seeds", ref snapshot.seeds) &&
                   ReadInts(fields, "treeSolved", ref snapshot.treeSolved) &&
                   ReadInts(fields, "bought", ref snapshot.bought) &&
                   ReadInts(fields, "boughtSolved", ref snapshot.boughtSolved) &&
                   ReadInts(fields, "gifts", ref snapshot.gifts) &&
                   ReadStrings(fields, "parcelIds", ref snapshot.parcelIds) &&
                   ReadStrings(fields, "letters", ref snapshot.letters) &&
                   ReadStrings(fields, "letterFrom", ref snapshot.letterFrom) &&
                   ReadInts(fields, "helped", ref snapshot.helped) &&
                   ReadStrings(fields, "helpedFrom", ref snapshot.helpedFrom) &&
                   ReadInt(fields, "fruit", ref snapshot.fruit) &&
                   ReadInts(fields, "fruitGiven", ref snapshot.fruitGiven) &&
                   ReadInt(fields, "lastFruitTo", ref snapshot.lastFruitTo) &&
                   ReadInts(fields, "together", ref snapshot.together) &&
                   ReadStrings(fields, "lanterns", ref snapshot.lanterns) &&
                   ReadInt(fields, "jam", ref snapshot.jam);
        }

        private static bool ReadInt(Dictionary<string, object> fields, string key, ref int target)
        {
            object raw;
            if (!fields.TryGetValue(key, out raw)) return true;
            return AsInt(raw, out target);
        }

        private static bool ReadBool(Dictionary<string, object> fields, string key, ref bool target)
        {
            object raw;
            if (!fields.TryGetValue(key, out raw)) return true;
            if (!(raw is bool)) return false;
            target = (bool)raw;
            return true;
        }

        private static bool ReadString(Dictionary<string, object> fields, string key, ref string target)
        {
            object raw;
            if (!fields.TryGetValue(key, out raw)) return true;
            if (raw == null) { target = null; return true; }
            var text = raw as string;
            if (text == null) return false;
            target = text;
            return true;
        }

        private static bool ReadInts(Dictionary<string, object> fields, string key, ref int[] target)
        {
            object raw;
            if (!fields.TryGetValue(key, out raw)) return true;
            if (raw == null) { target = null; return true; }
            var items = raw as List<object>;
            if (items == null) return false;
            var values = new int[items.Count];
            for (int i = 0; i < items.Count; i++)
                if (!AsInt(items[i], out values[i])) return false;
            target = values;
            return true;
        }

        private static bool ReadStrings(Dictionary<string, object> fields, string key, ref string[] target)
        {
            object raw;
            if (!fields.TryGetValue(key, out raw)) return true;
            if (raw == null) { target = null; return true; }
            var items = raw as List<object>;
            if (items == null) return false;
            var values = new string[items.Count];
            for (int i = 0; i < items.Count; i++)
                if ((values[i] = items[i] as string) == null) return false;
            target = values;
            return true;
        }

        private static bool ReadRequiredInt(Dictionary<string, object> fields, string key, out int value)
        {
            object raw;
            value = 0;
            return fields.TryGetValue(key, out raw) && AsInt(raw, out value);
        }

        private static bool ReadRequiredString(Dictionary<string, object> fields, string key, out string value)
        {
            object raw;
            value = null;
            return fields.TryGetValue(key, out raw) && (value = raw as string) != null;
        }

        private static bool ReadRequiredObject(Dictionary<string, object> fields, string key, out Dictionary<string, object> value)
        {
            object raw;
            value = null;
            return fields.TryGetValue(key, out raw) && (value = raw as Dictionary<string, object>) != null;
        }

        /// <summary>Accepts only integer JSON tokens (no fraction or exponent) within int range.</summary>
        internal static bool AsInt(object raw, out int value)
        {
            value = 0;
            var number = raw as JsonNumber;
            if (number == null) return false;
            if (number.Raw.IndexOf('.') >= 0 || number.Raw.IndexOf('e') >= 0 || number.Raw.IndexOf('E') >= 0)
                return false;
            return int.TryParse(number.Raw, NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out value);
        }

        internal sealed class JsonNumber
        {
            public readonly string Raw;
            public JsonNumber(string raw) { Raw = raw; }
        }

        /// <summary>Strict, minimal RFC 8259 reader: no duplicate keys, no trailing content.</summary>
        internal sealed class Parser
        {
            private readonly string text;
            private int position;

            private Parser(string text) { this.text = text; }

            public static bool TryParse(string text, out object root)
            {
                root = null;
                var parser = new Parser(text);
                object value;
                if (!parser.ParseValue(0, out value)) return false;
                parser.SkipWhitespace();
                if (parser.position != text.Length) return false;
                root = value;
                return true;
            }

            private bool ParseValue(int depth, out object value)
            {
                value = null;
                if (depth > MaxNestingDepth || position >= text.Length) return false;
                char c = text[position];
                if (c == '{') return ParseObject(depth, out value);
                if (c == '[') return ParseArray(depth, out value);
                if (c == '"')
                {
                    string s;
                    if (!ParseString(out s)) return false;
                    value = s;
                    return true;
                }
                if (c == '-' || IsDigit(c)) return ParseNumber(out value);
                if (Literal("true")) { value = true; return true; }
                if (Literal("false")) { value = false; return true; }
                if (Literal("null")) return true;
                return false;
            }

            private bool ParseObject(int depth, out object value)
            {
                value = null;
                var result = new Dictionary<string, object>(StringComparer.Ordinal);
                position++;
                SkipWhitespace();
                if (Peek() == '}') { position++; value = result; return true; }
                while (true)
                {
                    string key;
                    if (Peek() != '"' || !ParseString(out key)) return false;
                    if (result.ContainsKey(key)) return false;
                    SkipWhitespace();
                    if (Peek() != ':') return false;
                    position++;
                    SkipWhitespace();
                    object item;
                    if (!ParseValue(depth + 1, out item)) return false;
                    result.Add(key, item);
                    SkipWhitespace();
                    char next = Peek();
                    if (next == ',') { position++; SkipWhitespace(); continue; }
                    if (next == '}') { position++; value = result; return true; }
                    return false;
                }
            }

            private bool ParseArray(int depth, out object value)
            {
                value = null;
                var items = new List<object>();
                position++;
                SkipWhitespace();
                if (Peek() == ']') { position++; value = items; return true; }
                while (true)
                {
                    object item;
                    if (!ParseValue(depth + 1, out item)) return false;
                    items.Add(item);
                    SkipWhitespace();
                    char next = Peek();
                    if (next == ',') { position++; SkipWhitespace(); continue; }
                    if (next == ']') { position++; value = items; return true; }
                    return false;
                }
            }

            private bool ParseString(out string result)
            {
                result = null;
                if (Peek() != '"') return false;
                position++;
                var sb = new StringBuilder();
                while (position < text.Length)
                {
                    char c = text[position++];
                    if (c == '"') { result = sb.ToString(); return true; }
                    if (c < 0x20) return false;
                    if (c != '\\') { sb.Append(c); continue; }
                    if (position >= text.Length) return false;
                    char escape = text[position++];
                    switch (escape)
                    {
                        case '"': sb.Append('"'); break;
                        case '\\': sb.Append('\\'); break;
                        case '/': sb.Append('/'); break;
                        case 'b': sb.Append('\b'); break;
                        case 'f': sb.Append('\f'); break;
                        case 'n': sb.Append('\n'); break;
                        case 'r': sb.Append('\r'); break;
                        case 't': sb.Append('\t'); break;
                        case 'u':
                            int code;
                            if (!ParseHex4(out code)) return false;
                            sb.Append((char)code);
                            break;
                        default:
                            return false;
                    }
                }
                return false;
            }

            private bool ParseHex4(out int code)
            {
                code = 0;
                if (position + 4 > text.Length) return false;
                for (int i = 0; i < 4; i++)
                {
                    int digit = HexValue(text[position + i]);
                    if (digit < 0) return false;
                    code = code * 16 + digit;
                }
                position += 4;
                return true;
            }

            private bool ParseNumber(out object value)
            {
                value = null;
                int start = position;
                if (Peek() == '-') position++;
                if (Peek() == '0') position++;
                else if (IsDigit(Peek())) { while (IsDigit(Peek())) position++; }
                else return false;
                if (Peek() == '.')
                {
                    position++;
                    if (!IsDigit(Peek())) return false;
                    while (IsDigit(Peek())) position++;
                }
                if (Peek() == 'e' || Peek() == 'E')
                {
                    position++;
                    if (Peek() == '+' || Peek() == '-') position++;
                    if (!IsDigit(Peek())) return false;
                    while (IsDigit(Peek())) position++;
                }
                value = new JsonNumber(text.Substring(start, position - start));
                return true;
            }

            private bool Literal(string word)
            {
                if (position + word.Length > text.Length) return false;
                if (string.CompareOrdinal(text, position, word, 0, word.Length) != 0) return false;
                position += word.Length;
                return true;
            }

            private void SkipWhitespace()
            {
                while (position < text.Length &&
                       (text[position] == ' ' || text[position] == '\t' ||
                        text[position] == '\n' || text[position] == '\r'))
                    position++;
            }

            private char Peek()
            {
                return position < text.Length ? text[position] : '\0';
            }

            private static bool IsDigit(char c)
            {
                return c >= '0' && c <= '9';
            }

            private static int HexValue(char c)
            {
                if (c >= '0' && c <= '9') return c - '0';
                if (c >= 'a' && c <= 'f') return c - 'a' + 10;
                if (c >= 'A' && c <= 'F') return c - 'A' + 10;
                return -1;
            }
        }
    }
}
