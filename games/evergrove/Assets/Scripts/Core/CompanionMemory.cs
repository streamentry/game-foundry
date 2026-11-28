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
            {
                if (said.Day - h.Day >= KeepDays) continue;
                // CP02: a remark dated after this one means the clock moved back; keep it, heard today.
                kept.Add(h.Day > said.Day ? Redate(h, said.Day) : h);
            }
            kept.Add(said);
            return new CompanionMemory(Capped(kept));
        }

        /// <summary>
        /// CP03: over <see cref="MaxEntries"/>, first drop the oldest older repeats (a key with a newer
        /// entry still kept), so a key's only record survives; only if still over, drop the oldest remaining.
        /// Order is kept, oldest first. O(n).
        /// </summary>
        private static Heard[] Capped(List<Heard> list)
        {
            int excess = list.Count - MaxEntries;
            if (excess <= 0) return list.ToArray();

            var newest = new Dictionary<string, int>(StringComparer.Ordinal);
            for (int i = 0; i < list.Count; i++) newest[list[i].Key] = i;

            var drop = new bool[list.Count];
            for (int i = 0; i < list.Count && excess > 0; i++)
            {
                if (newest[list[i].Key] == i) continue;
                drop[i] = true;
                excess--;
            }
            for (int i = 0; i < list.Count && excess > 0; i++)
            {
                if (drop[i]) continue;
                drop[i] = true;
                excess--;
            }

            var kept = new List<Heard>(MaxEntries);
            for (int i = 0; i < list.Count; i++)
                if (!drop[i]) kept.Add(list[i]);
            return kept.ToArray();
        }

        /// <summary>The key was heard within the last <paramref name="days"/> days, today included.</summary>
        public bool SaidWithin(string key, int today, int days)
        {
            string clean = Clean(key);
            foreach (Heard h in heard)
                if (h.Key == clean && Age(h.Day, today) < days) return true;
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
                if (h.Kind == kind && h.Speaker == speaker && h.Variant == variant && Age(h.Day, today) < days)
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

        // CP06: read-only helpers for the companion's turn-taking and cooldown rules.

        /// <summary>The last day this speaker said anything, or int.MinValue if they never have.</summary>
        public int LastSpoke(int speaker)
        {
            int last = int.MinValue;
            foreach (Heard h in heard)
                if (h.Speaker == speaker && h.Day > last) last = h.Day;
            return last;
        }

        /// <summary>
        /// How many distinct days within the last <paramref name="days"/> days (today included) this speaker
        /// said the key. Two remarks on one day count once; a future-dated remark counts as today.
        /// </summary>
        public int DaysHeardBy(string key, int speaker, int today, int days)
        {
            string clean = Clean(key);
            var seen = new HashSet<int>();
            foreach (Heard h in heard)
                if (h.Key == clean && h.Speaker == speaker && Age(h.Day, today) < days)
                    seen.Add(Math.Min(h.Day, today));
            return seen.Count;
        }

        /// <summary>This speaker said a remark of this kind on exactly this day.</summary>
        public bool KindSaidOn(FactKind kind, int speaker, int day)
        {
            foreach (Heard h in heard)
                if (h.Kind == kind && h.Speaker == speaker && h.Day == day) return true;
            return false;
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
            return new CompanionMemory(Capped(list));
        }

        /// <summary>
        /// Days since heard. CP02: a remark dated after today (the clock moved back) is age 0, heard today,
        /// never a negative age that falls outside a window.
        /// </summary>
        private static int Age(int heardDay, int today)
        {
            return Math.Max(0, today - heardDay);
        }

        private static Heard Redate(Heard h, int day)
        {
            return new Heard(day, h.Speaker, h.Kind, h.Variant, h.Key);
        }

        /// <summary>Keys never carry the separators (a letter-writer's name could).</summary>
        internal static string Clean(string key)
        {
            if (key == null) return "";
            return key.Replace('|', '/').Replace('\n', ' ').Replace('\r', ' ');
        }
    }
}
