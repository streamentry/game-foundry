using System;
using System.Collections.Generic;
using System.Globalization;

namespace Evergrove.Core
{
    public enum NoteTarget { Letterbox, Basket, River, Thing }

    /// <summary>One day's note in the plan: the yyyymmdd it is due, and what it says.</summary>
    public sealed class PlannedNote
    {
        public int Day;
        public GardenNote Note;
    }

    /// <summary>
    /// I274, narrowed by checklist 57: the one daily note from the garden
    /// (docs/specs/proposals/notifications-2026-11-27.md). A note is only for an ARRIVAL that is
    /// waiting unopened: a family or neighbour letter in the box, or a cutting in the basket.
    /// Prizes and reminders (drops, replays, things still holding a puzzle) never qualify.
    /// Chosen from the save alone, in a neighbour's voice; never pressure. The garden doesn't
    /// change while you're away, so what's true at close is true when read.
    /// </summary>
    public sealed class GardenNote
    {
        public string From;
        public string Body;
        public NoteTarget Target;
        public int Thing = -1;   // story index, or BoughtBase + k (kept for saved notes; no longer made)

        public const int EarliestHour = 8;   // quiet hours: never before eight...
        public const int LatestHour = 20;    // ...or after eight in the evening
        public const int MaxUnopened = 3;    // three unopened in a row: stop until the game is opened
        public const int PlanDays = 7;       // the plan covers one week

        private static readonly string[] Names = { "Mira", "Orin", "Fen" };

        /// <summary>
        /// The note for what has arrived and is waiting, or null. Only two kinds exist: an unread
        /// letter in the box (Orin, the newest), then a cutting in the basket (its family's keeper).
        /// </summary>
        public static GardenNote For(WorldModel world, bool unreadLetter)
        {
            if (world == null || world.Mode != PlayMode.Living) return null;
            string[] letters = world.Letters;
            if (unreadLetter && letters.Length > 0)
                return new GardenNote { From = "Orin", Target = NoteTarget.Letterbox,
                    Body = "There's a letter in your box from " + world.LetterFrom[letters.Length - 1] +
                           ". It'll keep until you're ready." };
            int[] gifts = world.Gifts;
            if (gifts.Length > 0)
            {
                int kind = gifts[gifts.Length - 1];
                return new GardenNote { From = Names[(int)Nursery.Family(kind)], Target = NoteTarget.Basket,
                    Body = "A " + Nursery.Item(kind).Name + " cutting is in your basket, ready for a good spot." };
            }
            return null;
        }

        /// <summary>
        /// Checklist 57: the notes the scheduler would queue over the next seven days, one per day at most,
        /// from <paramref name="startDay"/> (yyyymmdd). It assumes the player does not open the game
        /// in between, so the same arrival stays waiting: a day is planned while the back-off
        /// allows, and the plan stops after three unopened in a row. Empty when opted out, when
        /// nothing has arrived, or when the back-off already stopped it. Pure: no clock, no device.
        /// </summary>
        public static List<PlannedNote> ForWeek(WorldModel world, int startDay, bool unreadLetter, bool optedIn, int unopenedInARow)
        {
            var plan = new List<PlannedNote>();
            if (!optedIn) return plan;
            GardenNote note = For(world, unreadLetter);
            DateTime first;
            if (note == null || !TryDay(startDay, out first)) return plan;
            int unopened = unopenedInARow;
            for (int day = 0; day < PlanDays && MaySchedule(unopened); day++, unopened++)
                plan.Add(new PlannedNote { Day = DayNumber(first.AddDays(day)), Note = note });
            return plan;
        }

        /// <summary>A calendar day as yyyymmdd.</summary>
        public static int DayNumber(DateTime day) { return day.Year * 10000 + day.Month * 100 + day.Day; }

        private static bool TryDay(int yyyymmdd, out DateTime day)
        {
            return DateTime.TryParseExact(yyyymmdd.ToString(CultureInfo.InvariantCulture), "yyyyMMdd",
                CultureInfo.InvariantCulture, DateTimeStyles.None, out day);
        }

        /// <summary>The next moment at the player's hour (clamped to daytime), at least half an hour away.</summary>
        public static DateTime NextTime(DateTime now, int hour)
        {
            int h = Math.Max(EarliestHour, Math.Min(LatestHour, hour));
            var at = new DateTime(now.Year, now.Month, now.Day, h, 0, 0, now.Kind);
            if (at <= now.AddMinutes(30)) at = at.AddDays(1);
            return at;
        }

        public static bool MaySchedule(int unopenedInARow) { return unopenedInARow < MaxUnopened; }
    }
}
