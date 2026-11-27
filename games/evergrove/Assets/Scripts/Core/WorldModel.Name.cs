using System;
using System.Globalization;
using System.Text;

namespace Evergrove.Core
{
    /// <summary>
    /// Checklist 3: the player may name the garden. The name is optional, shown on the cottage sign
    /// and on postcards, and travels in the garden file. A suggestion comes from RiverNames, the same
    /// for the same garden. Only a Living garden holds a name; "" means none.
    /// </summary>
    public sealed partial class WorldModel
    {
        /// <summary>The most text elements (graphemes, so an emoji counts as one) in a garden name.</summary>
        public const int MaxGardenNameLength = 24;

        /// <summary>The garden's name, or "" when none is set (a copy of the saved text).</summary>
        public string GardenName { get { return state.gardenName ?? ""; } }

        /// <summary>
        /// Sets the garden's name; "" (or only spaces) clears it. Living gardens only. The name is trimmed,
        /// internal whitespace collapses to one space, and it may be at most
        /// <see cref="MaxGardenNameLength"/> text elements. Control characters, line breaks, '&lt;' and '&gt;'
        /// are refused. A refused name changes nothing and returns false.
        /// </summary>
        public bool SetGardenName(string name)
        {
            if (Mode != PlayMode.Living) return false;
            string clean;
            if (!CleanGardenName(name, out clean)) return false;
            state.gardenName = clean;
            return true;
        }

        /// <summary>The name offered to a garden that has none yet: deterministic, and never empty.</summary>
        public static string SuggestedName(int garden)
        {
            return RiverNames.Suggest(garden);
        }

        /// <summary>Normalises a proposed name, or returns false when it cannot be a name.</summary>
        internal static bool CleanGardenName(string raw, out string clean)
        {
            clean = null;
            if (raw == null) return false;
            var sb = new StringBuilder(raw.Length);
            bool pendingSpace = false;
            for (int i = 0; i < raw.Length; i++)
            {
                char ch = raw[i];
                if (ch == '<' || ch == '>' || ch == '\u2028' || ch == '\u2029') return false;
                if (char.IsControl(ch)) return false;
                if (char.IsHighSurrogate(ch))
                {
                    if (i + 1 >= raw.Length || !char.IsLowSurrogate(raw[i + 1])) return false;
                    if (pendingSpace && sb.Length > 0) sb.Append(' ');
                    pendingSpace = false;
                    sb.Append(ch).Append(raw[++i]);
                    continue;
                }
                if (char.IsLowSurrogate(ch)) return false;
                if (char.IsWhiteSpace(ch)) { pendingSpace = true; continue; }
                if (pendingSpace && sb.Length > 0) sb.Append(' ');
                pendingSpace = false;
                sb.Append(ch);
            }
            string result = sb.ToString();
            if (new StringInfo(result).LengthInTextElements > MaxGardenNameLength) return false;
            clean = result;
            return true;
        }

        /// <summary>Fail-closed validation of a saved name: "" or already exactly as <see cref="CleanGardenName"/> writes it.</summary>
        internal static bool ReadGardenName(WorldSnapshot source, out string name)
        {
            name = null;
            string text = source.gardenName ?? "";
            if (text.Length == 0) { name = ""; return true; }
            string clean;
            if (!CleanGardenName(text, out clean) || clean != text) return false;
            if (source.mode != "living") return false;
            name = text;
            return true;
        }
    }
}
