using System;

namespace Evergrove.Core
{
    /// <summary>
    /// v1 #52: days played is a total that only grows. It is never a streak: a day you skip costs nothing,
    /// nothing resets, and coming back later simply adds one more day to the total.
    /// </summary>
    public sealed partial class WorldModel
    {
        /// <summary>Most days a garden counts. Far beyond real play; it only bounds a forged save.</summary>
        public const int MaxDaysPlayed = 100000;

        /// <summary>Days this garden has been played on (a total, never a streak). Living gardens only.</summary>
        public int DaysPlayed { get { return state.daysPlayed; } }

        /// <summary>The last local day (yyyymmdd) counted toward <see cref="DaysPlayed"/>, 0 when none.</summary>
        public int LastPlayedDay { get { return state.lastPlayedDay; } }

        /// <summary>
        /// Counts <paramref name="day"/> (local yyyymmdd) as a day played. Only a later day than the last one
        /// counted adds to the total: the same day again, or the clock set back, changes nothing and returns false.
        /// Day 0, an impossible date, or a Puzzle mode garden also change nothing. The total never decreases.
        /// </summary>
        public bool MarkPlayed(int day)
        {
            if (Mode != PlayMode.Living || !IsCalendarDay(day) || day <= state.lastPlayedDay) return false;
            state.lastPlayedDay = day;
            state.daysPlayed = Math.Min(MaxDaysPlayed, state.daysPlayed + 1);
            return true;
        }

        /// <summary>
        /// Fail-closed validation of the days played (v1 #52): the count is 0..MaxDaysPlayed, the last day is 0 or a
        /// real calendar day, the two are zero together, and both are zero outside a Living garden.
        /// </summary>
        private static bool ReadDays(WorldSnapshot source)
        {
            bool countOk = source.daysPlayed >= 0 && source.daysPlayed <= MaxDaysPlayed;
            bool dayOk = source.lastPlayedDay == 0 || IsCalendarDay(source.lastPlayedDay);
            bool together = (source.daysPlayed == 0) == (source.lastPlayedDay == 0);
            bool living = source.mode == "living" || (source.daysPlayed == 0 && source.lastPlayedDay == 0);
            return countOk && dayOk && together && living;
        }

        /// <summary>True for a real local calendar day written as yyyymmdd, from 1900 to 9999.</summary>
        private static bool IsCalendarDay(int day)
        {
            int year = day / 10000;
            int month = day / 100 % 100;
            int dayOfMonth = day % 100;
            return year >= 1900 && year <= 9999 && month >= 1 && month <= 12 &&
                   dayOfMonth >= 1 && dayOfMonth <= DateTime.DaysInMonth(year, month);
        }
    }
}
