using System;
using System.Globalization;

namespace Evergrove.Core
{
    /// <summary>
    /// Checklist 9, "Restore that the player can trust": the calm line that says the garden is
    /// kept on this phone and when it was last saved. It is shown before any garden-file restore
    /// or replace prompt. Pure: the caller passes the local times, so there is no clock here.
    /// </summary>
    public static class SaveStatus
    {
        /// <summary>Shown when the last save time is not known.</summary>
        public const string Kept = "Your garden is saved on this phone.";

        /// <summary>
        /// The line for a save time, or <see cref="Kept"/> alone when it is unknown. The time uses the
        /// culture's short time pattern; a day older than yesterday uses the culture's day and month.
        /// </summary>
        public static string Line(DateTime? lastSavedLocal, DateTime nowLocal, CultureInfo culture)
        {
            if (!lastSavedLocal.HasValue) return Kept;
            CultureInfo format = culture ?? CultureInfo.InvariantCulture;
            DateTime saved = lastSavedLocal.Value;
            string time = saved.ToString(format.DateTimeFormat.ShortTimePattern, format);
            DateTime today = nowLocal.Date;
            DateTime day = saved.Date;

            string when;
            if (day == today) when = "today at " + time;
            else if (day == today.AddDays(-1)) when = "yesterday at " + time;
            else when = "on " + day.ToString("d MMM", format) + " at " + time;
            return Kept + " Last saved " + when + ".";
        }
    }
}
