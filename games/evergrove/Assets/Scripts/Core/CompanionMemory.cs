using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace Evergrove.Core
{
    /// <summary>One remark heard: which fact (key and kind), who said it, which wording, and the day.</summary>
    public readonly struct Heard
    {
        public readonly int Day;
        public readonly int Speaker;
        public readonly FactKind Kind;
        public readonly int Variant;
        public readonly string Key;

        public Heard(int day, int speaker, FactKind kind, int variant, string key)
        {
            Day = day;
            Speaker = speaker;
            Kind = kind;
            Variant = variant;
            Key = CompanionMemory.Clean(key);
        }
    }

    /// <summary>
    /// Smart companion SC02: what the neighbours have said on this device, and when. Conversation
    /// state, not garden state: it lives in PlayerPrefs, never in the save or the garden file, so a
    /// garden restored elsewhere may hear a remark again. Immutable; <see cref="With"/> returns a copy.
    /// Kept for <see cref="KeepDays"/> days and at most <see cref="MaxEntries"/> remarks.
    /// </summary>
    public sealed class CompanionMemory
    {
        public const int KeepDays = 60;
        public const int MaxEntries = 300;

        private readonly Heard[] heard;

        public static readonly CompanionMemory Empty = new CompanionMemory(Array.Empty<Heard>());

        private CompanionMemory(Heard[] entries) { heard = entries; }

        public int Count { get { return heard.Length; } }

        public IReadOnlyList<Heard> Entries { get { return Array.AsReadOnly(heard); } }

        /// <summary>Remember a remark; older than <see cref="KeepDays"/> before it, or beyond the cap, is let go.</summary>
        public CompanionMemory With(Heard said)
        {
            var kept = new List<Heard>(heard.Length + 1);
            foreach (Heard h in heard)
                if (said.Day - h.Day < KeepDays && h.Day <= said.Day) kept.Add(h);
            kept.Add(said);
            int skip = Math.Max(0, kept.Count - MaxEntries);
            return new CompanionMemory(kept.GetRange(skip, kept.Count - skip).ToArray());
        }

        /// <summary>The key was heard within the last <paramref name="days"/> days, today included.</summary>
        public bool SaidWithin(string key, int today, int days)
        {
            string clean = Clean(key);
            foreach (Heard h in heard)
                if (h.Key == clean && today - h.Day >= 0 && today - h.Day < days) return true;
            return false;
        }

        public bool EverSaid(string key)
        {
            string clean = Clean(key);
            foreach (Heard h in heard)
                if (h.Key == clean) return true;
            return false;
        }

        /// <summary>This wording of a kind, in this voice, was heard within the last <paramref name="days"/> days.</summary>
        public bool VariantWithin(FactKind kind, int speaker, int variant, int today, int days)
        {
            foreach (Heard h in heard)
                if (h.Kind == kind && h.Speaker == speaker && h.Variant == variant && today - h.Day >= 0 && today - h.Day < days)
                    return true;
            return false;
        }

        /// <summary>The last day this wording was heard, or int.MinValue.</summary>
        public int LastHeard(FactKind kind, int speaker, int variant)
        {
            int last = int.MinValue;
            foreach (Heard h in heard)
                if (h.Kind == kind && h.Speaker == speaker && h.Variant == variant && h.Day > last) last = h.Day;
            return last;
        }

        // ---- PlayerPrefs text: one remark per line, "day|speaker|kind|variant|key".
        public string Serialize()
        {
            var text = new StringBuilder();
            foreach (Heard h in heard)
            {
                if (text.Length > 0) text.Append('\n');
                text.Append(h.Day.ToString(CultureInfo.InvariantCulture)).Append('|')
                    .Append(h.Speaker.ToString(CultureInfo.InvariantCulture)).Append('|')
                    .Append(h.Kind.ToString()).Append('|')
                    .Append(h.Variant.ToString(CultureInfo.InvariantCulture)).Append('|')
                    .Append(h.Key);
            }
            return text.ToString();
        }

        /// <summary>Reads what <see cref="Serialize"/> wrote. Unreadable lines are skipped, never thrown on.</summary>
        public static CompanionMemory Parse(string text)
        {
            if (string.IsNullOrEmpty(text)) return Empty;
            var list = new List<Heard>();
            foreach (string line in text.Split('\n'))
            {
                string[] parts = line.Split(new[] { '|' }, 5);
                if (parts.Length != 5 || parts[4].Length == 0) continue;
                if (!int.TryParse(parts[0], NumberStyles.Integer, CultureInfo.InvariantCulture, out int day) ||
                    !int.TryParse(parts[1], NumberStyles.Integer, CultureInfo.InvariantCulture, out int speaker) ||
                    speaker < 0 || speaker > 2 ||
                    !Enum.TryParse(parts[2], out FactKind kind) || !Enum.IsDefined(typeof(FactKind), kind) ||
                    !int.TryParse(parts[3], NumberStyles.Integer, CultureInfo.InvariantCulture, out int variant))
                    continue;
                list.Add(new Heard(day, speaker, kind, variant, parts[4]));
            }
            int skip = Math.Max(0, list.Count - MaxEntries);
            return new CompanionMemory(list.GetRange(skip, list.Count - skip).ToArray());
        }

        /// <summary>Keys never carry the separators (a letter-writer's name could).</summary>
        internal static string Clean(string key)
        {
            if (key == null) return "";
            return key.Replace('|', '/').Replace('\n', ' ').Replace('\r', ' ');
        }
    }
}
