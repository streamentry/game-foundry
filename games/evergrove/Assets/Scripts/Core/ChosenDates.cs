using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace Evergrove.Core
{
    /// <summary>What a chosen date is for. The neighbours mark it in a matching way.</summary>
    public enum ChosenDateKind
    {
        Birthday = 0,
        Anniversary = 1,
        Other = 2
    }

    /// <summary>
    /// One real date the player chose to remember: a month and day, a kind, and an optional short label.
    /// Immutable; build it with <see cref="ChosenDates.TryMake"/>.
    /// </summary>
    public sealed class ChosenDate
    {
        public int Month { get; }
        public int Day { get; }
        public ChosenDateKind Kind { get; }
        /// <summary>The optional label, already cleaned; "" when none.</summary>
        public string Label { get; }

        internal ChosenDate(int month, int day, ChosenDateKind kind, string label)
        {
            Month = month;
            Day = day;
            Kind = kind;
            Label = label;
        }
    }

    /// <summary>
    /// Checklist 87: "They mark real dates you choose." The player may add a birthday, an anniversary or
    /// another day; the neighbours mark it on that day. Pure rules, no Unity.
    ///
    /// PRIVACY. A date is only month, day, kind and an optional short label. No year is stored, so no
    /// age or birth year can be computed: a day and a month are enough to mark a day, and a record without
    /// a year is less sensitive data. The list lives in device-local PlayerPrefs under <see cref="Prefix"/>.
    /// It is never written into the garden save, the garden file, family parcels, postcards, notifications
    /// or any share text. Only Core/ChosenDates.cs and EvergroveApp.Dates.cs may use that prefix.
    /// </summary>
    public static class ChosenDates
    {
        /// <summary>The most dates one device keeps.</summary>
        public const int MaxDates = 6;

        /// <summary>The most text elements (graphemes, so an emoji counts as one) in a label.</summary>
        public const int MaxLabelLength = 24;

        /// <summary>The PlayerPrefs key prefix. Device-local only; see the class comment.</summary>
        public const string Prefix = "evergrove:dates:";

        /// <summary>The PlayerPrefs key that holds the encoded list.</summary>
        public const string ListKey = Prefix + "list";

        /// <summary>The PlayerPrefs key that holds the yyyymmdd the mark was last shown (once per day).</summary>
        public const string ShownKey = Prefix + "shown";

        private const string Version = "v1";
        // Upper bound on the stored string before any parsing; a full list is far below this.
        private const int MaxEncodedLength = 4096;
        // Upper bound on a label's UTF-16 length before it is cleaned (24 text elements never need more).
        private const int MaxRawLabelChars = 96;

        private static readonly int[] DaysInMonth = { 31, 29, 31, 30, 31, 30, 31, 31, 30, 31, 30, 31 };

        // The neighbours who leave the marks. Same three names as the rest of the village.
        private static readonly string[] Neighbours = { "Mira", "Orin", "Fen" };

        /// <summary>
        /// Builds a date, or returns false when the month, day, kind or label is not allowed.
        /// Feb 29 is a real date: it is stored without a year, so it can be marked every year.
        /// </summary>
        public static bool TryMake(int month, int day, ChosenDateKind kind, string label, out ChosenDate date)
        {
            date = null;
            if (month < 1 || month > 12) return false;
            if (day < 1 || day > DaysInMonth[month - 1]) return false;
            if (!Enum.IsDefined(typeof(ChosenDateKind), kind)) return false;
            string clean;
            if (!CleanLabel(label, out clean)) return false;
            date = new ChosenDate(month, day, kind, clean);
            return true;
        }

        /// <summary>
        /// Normalises a label the same way garden names are: trimmed, internal whitespace collapsed to one
        /// space, at most <see cref="MaxLabelLength"/> text elements. Control characters (so no line breaks),
        /// '&lt;' and '&gt;' are refused. Empty or spaces-only means no label and is allowed.
        /// </summary>
        public static bool CleanLabel(string raw, out string clean)
        {
            clean = null;
            if (raw == null) { clean = ""; return true; }
            if (raw.Length > MaxRawLabelChars) return false;
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
            if (new StringInfo(result).LengthInTextElements > MaxLabelLength) return false;
            clean = result;
            return true;
        }

        /// <summary>Adds a date. Refuses a full list (6) and a second date on the same month and day.</summary>
        public static bool TryAdd(IReadOnlyList<ChosenDate> current, ChosenDate date, out ChosenDate[] next)
        {
            next = null;
            if (current == null || date == null || current.Count >= MaxDates) return false;
            for (int i = 0; i < current.Count; i++)
                if (current[i].Month == date.Month && current[i].Day == date.Day) return false;
            var list = new ChosenDate[current.Count + 1];
            for (int i = 0; i < current.Count; i++) list[i] = current[i];
            list[current.Count] = date;
            next = list;
            return true;
        }

        /// <summary>Removes the date at <paramref name="index"/>; returns a new list.</summary>
        public static bool TryRemoveAt(IReadOnlyList<ChosenDate> current, int index, out ChosenDate[] next)
        {
            next = null;
            if (current == null || index < 0 || index >= current.Count) return false;
            var list = new ChosenDate[current.Count - 1];
            for (int i = 0, j = 0; i < current.Count; i++)
                if (i != index) list[j++] = current[i];
            next = list;
            return true;
        }

        /// <summary>
        /// The text for the PlayerPrefs string. Empty list is "". Otherwise "v1" then ";M.D.K.HEX" per date,
        /// where K is the kind number and HEX is the label's UTF-8 bytes in lowercase hex (empty for no label).
        /// Hex keeps the label free of separators, so the format needs no escaping.
        /// </summary>
        public static string Encode(IReadOnlyList<ChosenDate> dates)
        {
            if (dates == null || dates.Count == 0) return "";
            var sb = new StringBuilder(Version);
            for (int i = 0; i < dates.Count; i++)
            {
                ChosenDate d = dates[i];
                sb.Append(';')
                  .Append(d.Month.ToString(CultureInfo.InvariantCulture)).Append('.')
                  .Append(d.Day.ToString(CultureInfo.InvariantCulture)).Append('.')
                  .Append(((int)d.Kind).ToString(CultureInfo.InvariantCulture)).Append('.')
                  .Append(ToHex(Encoding.UTF8.GetBytes(d.Label)));
            }
            return sb.ToString();
        }

        /// <summary>
        /// Strict, fail-closed read of a stored string. Anything that is not exactly what <see cref="Encode"/>
        /// writes (wrong version, extra or missing fields, signs or spaces in numbers, bad hex, invalid UTF-8,
        /// a label that would not be cleaned, a day that does not exist, a duplicate day, more than six dates,
        /// or a string that does not re-encode to itself) returns false and an empty list.
        /// </summary>
        public static bool TryDecode(string text, out ChosenDate[] dates)
        {
            dates = new ChosenDate[0];
            if (text == null) return false;
            if (text.Length == 0) return true;
            if (text.Length > MaxEncodedLength) return false;
            if (!text.StartsWith(Version + ";", StringComparison.Ordinal)) return false;
            string[] parts = text.Substring(Version.Length + 1).Split(';');
            if (parts.Length > MaxDates) return false;
            var list = new ChosenDate[parts.Length];
            try
            {
                for (int i = 0; i < parts.Length; i++)
                {
                    if (!TryDecodeOne(parts[i], out list[i])) return false;
                    for (int j = 0; j < i; j++)
                        if (list[j].Month == list[i].Month && list[j].Day == list[i].Day) return false;
                }
            }
            catch (Exception)
            {
                // Bad input must never escape as an exception: fail closed.
                return false;
            }
            // Only the exact canonical text is accepted, so a forged or padded string cannot slip through.
            if (Encode(list) != text) return false;
            dates = list;
            return true;
        }

        /// <summary>The neighbour's gentle line for a day, or null when no chosen date falls on it.</summary>
        /// <param name="yyyymmdd">The local day, as yyyymmdd. Anything that is not a real day gives null.</param>
        /// <param name="dates">The chosen dates; the first match in list order is used.</param>
        /// <remarks>
        /// A Feb 29 date is marked on Feb 28 in a year with no Feb 29, so it is never missed.
        /// The neighbour is picked from the day itself, so the same day always gets the same line.
        /// </remarks>
        public static string MarkFor(int yyyymmdd, IReadOnlyList<ChosenDate> dates)
        {
            if (dates == null) return null;
            int year = yyyymmdd / 10000;
            int month = yyyymmdd / 100 % 100;
            int day = yyyymmdd % 100;
            if (year < 1 || year > 9999 || month < 1 || month > 12) return null;
            bool leap = DateTime.IsLeapYear(year);
            int daysThisMonth = DaysInMonth[month - 1] - (month == 2 && !leap ? 1 : 0);
            if (day < 1 || day > daysThisMonth) return null;

            bool feb28Fallback = month == 2 && day == 28 && !leap;
            for (int i = 0; i < dates.Count; i++)
            {
                ChosenDate d = dates[i];
                if (d == null || d.Month != month) continue;
                bool onDay = d.Day == day || (feb28Fallback && d.Day == 29);
                if (onDay) return Line(d, Neighbours[(month * 31 + day) % Neighbours.Length]);
            }
            return null;
        }

        private static string Line(ChosenDate d, string neighbour)
        {
            string label = d.Label.TrimEnd('.');
            bool named = label.Length > 0;
            switch (d.Kind)
            {
                case ChosenDateKind.Birthday:
                    return neighbour + " tied a ribbon on the gate for " + (named ? label : "a birthday") + ".";
                case ChosenDateKind.Anniversary:
                    return neighbour + " set a candle in the window for " + (named ? label : "an anniversary") + ".";
                default:
                    return neighbour + " left a flower by the gate for " + (named ? label : "a day you chose") + ".";
            }
        }

        private static bool TryDecodeOne(string part, out ChosenDate date)
        {
            date = null;
            string[] fields = part.Split('.');
            if (fields.Length != 4) return false;
            int month, day, kind;
            if (!TryDigits(fields[0], out month) || !TryDigits(fields[1], out day) || !TryDigits(fields[2], out kind))
                return false;
            if (kind < 0 || kind > (int)ChosenDateKind.Other) return false;
            byte[] bytes;
            if (!TryFromHex(fields[3], out bytes)) return false;
            string label = new UTF8Encoding(false, true).GetString(bytes);
            string clean;
            if (!CleanLabel(label, out clean) || clean != label) return false;
            return TryMake(month, day, (ChosenDateKind)kind, clean, out date);
        }

        // Digits only: no sign, no spaces, one or two characters.
        private static bool TryDigits(string text, out int value)
        {
            value = 0;
            if (text.Length < 1 || text.Length > 2) return false;
            for (int i = 0; i < text.Length; i++)
                if (text[i] < '0' || text[i] > '9') return false;
            value = int.Parse(text, NumberStyles.None, CultureInfo.InvariantCulture);
            return true;
        }

        private static string ToHex(byte[] bytes)
        {
            var sb = new StringBuilder(bytes.Length * 2);
            for (int i = 0; i < bytes.Length; i++) sb.Append(bytes[i].ToString("x2", CultureInfo.InvariantCulture));
            return sb.ToString();
        }

        // Lowercase hex only, an even number of characters. Empty is an empty label.
        private static bool TryFromHex(string text, out byte[] bytes)
        {
            bytes = null;
            if (text.Length % 2 != 0) return false;
            var result = new byte[text.Length / 2];
            for (int i = 0; i < result.Length; i++)
            {
                int high = HexValue(text[i * 2]), low = HexValue(text[i * 2 + 1]);
                if (high < 0 || low < 0) return false;
                result[i] = (byte)(high * 16 + low);
            }
            bytes = result;
            return true;
        }

        private static int HexValue(char ch)
        {
            if (ch >= '0' && ch <= '9') return ch - '0';
            if (ch >= 'a' && ch <= 'f') return ch - 'a' + 10;
            return -1;
        }
    }
}
