using System;

namespace Evergrove.Core
{
    /// <summary>
    /// I419 to I422 (run 3, "a companion in subtle ways"): what the keepers notice about how you
    /// come to the garden, from facts kept on this device only (never in the garden file, which
    /// travels to family): the hour you usually come, a long time away, the garden's own
    /// anniversary. Gentle and rare by design: no streaks, no guilt, no claims about wellbeing.
    /// </summary>
    public static class Companion
    {
        public const int PhaseCount = 6;
        public const int HabitNeeds = 6;           // returns counted before a habit can show
        public const float HabitShare = 0.6f;      // and the one hour must be this share of them
        public const int LongAway = 7, VeryLongAway = 30;

        /// <summary>I419: the hour (phase) you usually come, or null while there's no clear habit.</summary>
        public static DayPhase? Habit(int[] counts)
        {
            if (counts == null || counts.Length != PhaseCount) return null;
            int total = 0, best = 0;
            for (int i = 0; i < counts.Length; i++)
            {
                total += Math.Max(0, counts[i]);
                if (counts[i] > counts[best]) best = i;
            }
            if (total < HabitNeeds || counts[best] < total * HabitShare) return null;
            return (DayPhase)best;
        }

        /// <summary>Who would notice an hour: Mira mornings, Fen days and nights, Orin evenings.</summary>
        public static int Noticer(DayPhase habit)
        {
            switch (habit)
            {
                case DayPhase.Dawn: case DayPhase.Morning: return 0;
                case DayPhase.Golden: case DayPhase.Dusk: return 1;
                default: return 2;
            }
        }

        /// <summary>I419: the remark, by its noticer only, one day in seven. Null otherwise.</summary>
        public static string HabitLine(int[] counts, int villager, int dayNumber)
        {
            DayPhase? habit = Habit(counts);
            if (!habit.HasValue || Noticer(habit.Value) != villager || ((dayNumber % 7) + 7) % 7 != villager * 2) return null;
            switch (habit.Value)
            {
                case DayPhase.Dawn: return "You're up with the light again. The garden's at its quietest now, before anyone's about.";
                case DayPhase.Morning: return "Mornings suit you. You catch the dew before it goes.";
                case DayPhase.Day: return "You like the garden in the full of the day. So do the bees.";
                case DayPhase.Golden: return "You often come as the light goes gold. I keep an eye out for you about now.";
                case DayPhase.Dusk: return "Evenings suit you, I think. I've left the lamp lit.";
                default: return "A night visitor, like our owl. It's peaceful, isn't it, with everyone asleep.";
            }
        }

        /// <summary>I420: a welcome after a long time away: reassurance, never guilt. Null for short gaps.</summary>
        public static string WelcomeBack(int daysAway)
        {
            if (daysAway >= VeryLongAway) return "There you are. It's been a while, and that's all right: nothing's lost, the garden kept itself.";
            if (daysAway >= LongAway) return "It's good to see you. Nothing's lost while you were away.";
            return null;
        }

        /// <summary>I421: whole years since the garden began, on its anniversary day; 0 on any other day.</summary>
        public static int Anniversary(DateTime first, DateTime today)
        {
            if (today.Date <= first.Date || today.Month != first.Month) return 0;
            bool sameDay = today.Day == first.Day || (first.Month == 2 && first.Day == 29 && today.Day == 28 && !DateTime.IsLeapYear(today.Year));
            return sameDay ? today.Year - first.Year : 0;
        }

        public static string AnniversaryNote(int years)
        {
            string span = years == 1 ? "A year" : years + " years";
            return span + " today since this garden began. We were saying, the three of us, how different the river looks now. Thank you for coming back to it.";
        }

        /// <summary>I422: late at night the garden's sounds are softer (22:00 to 06:59).</summary>
        public static bool QuietHours(int hour) { return hour >= 22 || hour < 7; }

        public static float Volume(int hour) { return QuietHours(hour) ? 0.6f : 1f; }
    }
}
