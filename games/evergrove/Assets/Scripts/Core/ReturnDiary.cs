#if UNITY_EDITOR || DEVELOPMENT_BUILD || EVERGROVE_RESEARCH_TOOLS
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace Evergrove.Core
{
    public enum ReturnStatus
    {
        NotConsented,
        NotYetDue,
        Returned,
        NotReturned
    }

    /// <summary>
    /// Opt-in, on-device record of the local calendar dates on which a consenting
    /// research participant opened the app, for the facilitator's D1/D7 follow-up.
    ///
    /// Stores dates only: no times, identifiers, game content, network or device data.
    /// Callers pass the local calendar date; this type never reads a clock.
    /// Nothing is recorded until GrantConsent. Persistence belongs to the caller and
    /// must stay separate from the garden save. Research/development builds only.
    /// </summary>
    public sealed class ReturnDiary
    {
        public const int MaxEntries = 60;

        private const string DateFormat = "yyyy-MM-dd";
        private const string Header = "EVERGROVE RETURN DIARY v1";
        private const string ConsentLine =
            "CONSENT: opted in on this device; local calendar dates only; no times, identifiers or game content; exported only by the participant's copy action";
        private const string LimitLine =
            "LIMIT: local dates only; does not measure identity, cohorts, install source, satisfaction or causality";

        // Sorted ascending. Index 0 is always the first consented day when consented.
        private readonly List<DateTime> days = new List<DateTime>();
        private DateTime? firstDay;
        private bool truncated;

        public bool Consented { get { return firstDay.HasValue; } }
        public DateTime? FirstDay { get { return firstDay; } }
        public bool Truncated { get { return truncated; } }
        public IReadOnlyList<DateTime> Days { get { return days.AsReadOnly(); } }

        /// <summary>Starts the diary. Returns false if already consented.</summary>
        public bool GrantConsent(DateTime localToday)
        {
            if (Consented) return false;
            DateTime today = LocalDay(localToday);
            firstDay = today;
            days.Add(today);
            return true;
        }

        /// <summary>
        /// Records one open on a local calendar date. Returns true only when a new
        /// date was kept. Nothing is recorded before consent, for repeated dates, or
        /// for dates earlier than the first consented day (for example after a clock change).
        /// </summary>
        public bool RecordOpen(DateTime localToday)
        {
            if (!Consented) return false;
            DateTime today = LocalDay(localToday);
            if (today < firstDay.Value) return false;
            int index = days.BinarySearch(today);
            if (index >= 0) return false;
            int position = ~index;
            days.Insert(position, today);
            if (days.Count <= MaxEntries) return true;

            // Over the cap: drop the oldest entry after the first consented day,
            // so the first day (the D1/D7 anchor) is never lost.
            bool droppedNew = position == 1;
            days.RemoveAt(1);
            truncated = true;
            return !droppedNew;
        }

        /// <summary>One-action withdrawal: clears consent and every recorded date.</summary>
        public void Delete()
        {
            firstDay = null;
            days.Clear();
            truncated = false;
        }

        /// <summary>Exact-day return: day N after the first consented day.</summary>
        public ReturnStatus Status(int offsetDays, DateTime localToday)
        {
            if (!Consented) return ReturnStatus.NotConsented;
            if (offsetDays < 1) throw new ArgumentOutOfRangeException(nameof(offsetDays));
            DateTime due = firstDay.Value.AddDays(offsetDays);
            if (days.BinarySearch(due) >= 0) return ReturnStatus.Returned;
            return LocalDay(localToday) < due ? ReturnStatus.NotYetDue : ReturnStatus.NotReturned;
        }

        /// <summary>True if any recorded date falls on days 1..maxOffsetDays after the first day.</summary>
        public bool ReturnedWithin(int maxOffsetDays)
        {
            if (!Consented) return false;
            DateTime last = firstDay.Value.AddDays(maxOffsetDays);
            foreach (DateTime day in days)
            {
                if (day > firstDay.Value && day <= last) return true;
            }
            return false;
        }

        /// <summary>Participant-copyable text. Field order and wording are a stable format (v1).</summary>
        public string Export(DateTime localToday)
        {
            var text = new StringBuilder();
            text.Append(Header).Append('\n');
            if (!Consented)
            {
                text.Append("CONSENT: none; nothing recorded\n");
                text.Append(LimitLine).Append('\n');
                return text.ToString();
            }

            DateTime today = LocalDay(localToday);
            DateTime first = firstDay.Value;
            text.Append(ConsentLine).Append('\n');
            text.Append("first_day=").Append(Format(first)).Append('\n');
            text.Append("opened_days=").Append(days.Count).Append('\n');
            text.Append("days=").Append(JoinDays()).Append('\n');
            text.Append("day1_return=").Append(StatusName(Status(1, today))).Append('\n');
            text.Append("day7_return=").Append(StatusName(Status(7, today))).Append('\n');
            string within = ReturnedWithin(7) ? "yes"
                : today < first.AddDays(7) ? "not_yet_due" : "no";
            text.Append("returned_within_7_days=").Append(within).Append('\n');
            text.Append("truncated=").Append(truncated ? "yes" : "no").Append('\n');
            text.Append(LimitLine).Append('\n');
            return text.ToString();
        }

        /// <summary>Compact storage form. Empty string when not consented.</summary>
        public string Serialize()
        {
            if (!Consented) return "";
            return "v1;first=" + Format(firstDay.Value) + ";days=" + JoinDays()
                + ";truncated=" + (truncated ? "1" : "0");
        }

        /// <summary>Strict parser. Anything malformed yields an unconsented, empty diary.</summary>
        public static ReturnDiary Parse(string text)
        {
            if (string.IsNullOrEmpty(text)) return new ReturnDiary();
            string[] parts = text.Split(';');
            if (parts.Length != 4 || parts[0] != "v1") return new ReturnDiary();
            if (!parts[1].StartsWith("first=", StringComparison.Ordinal)) return new ReturnDiary();
            if (!parts[2].StartsWith("days=", StringComparison.Ordinal)) return new ReturnDiary();
            bool wasTruncated;
            if (parts[3] == "truncated=1") wasTruncated = true;
            else if (parts[3] == "truncated=0") wasTruncated = false;
            else return new ReturnDiary();

            DateTime first;
            if (!TryDate(parts[1].Substring(6), out first)) return new ReturnDiary();
            string[] items = parts[2].Substring(5).Split(',');
            if (items.Length < 1 || items.Length > MaxEntries) return new ReturnDiary();

            var parsed = new ReturnDiary();
            DateTime previous = DateTime.MinValue;
            foreach (string item in items)
            {
                DateTime day;
                if (!TryDate(item, out day)) return new ReturnDiary();
                if (day <= previous) return new ReturnDiary(); // strictly ascending, no duplicates
                parsed.days.Add(day);
                previous = day;
            }
            if (parsed.days[0] != first) return new ReturnDiary();
            parsed.firstDay = first;
            parsed.truncated = wasTruncated;
            return parsed;
        }

        private string JoinDays()
        {
            var parts = new List<string>(days.Count);
            foreach (DateTime day in days) parts.Add(Format(day));
            return string.Join(",", parts);
        }

        private static string StatusName(ReturnStatus status)
        {
            switch (status)
            {
                case ReturnStatus.Returned: return "returned";
                case ReturnStatus.NotReturned: return "not_returned";
                case ReturnStatus.NotYetDue: return "not_yet_due";
                default: return "not_consented";
            }
        }

        private static DateTime LocalDay(DateTime value)
        {
            return new DateTime(value.Year, value.Month, value.Day);
        }

        private static string Format(DateTime day)
        {
            return day.ToString(DateFormat, CultureInfo.InvariantCulture);
        }

        private static bool TryDate(string text, out DateTime day)
        {
            return DateTime.TryParseExact(text, DateFormat, CultureInfo.InvariantCulture,
                DateTimeStyles.None, out day);
        }
    }
}
#endif
