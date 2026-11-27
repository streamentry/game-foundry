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
        public const int HabitNeeds = 5;           // visits to one hour before a habit can show (idea 82)
        public const int LongAway = 7, VeryLongAway = 30;
        public const int LetterAway = 90;          // days away before the return letter (idea 90)

        /// <summary>
        /// I419 (idea 82): the hour (phase) you usually come, or null while there's no clear habit.
        /// The busiest hour counts alone: visits to other hours neither add to it nor cancel it.
        /// </summary>
        public static DayPhase? Habit(int[] counts)
        {
            if (counts == null || counts.Length != PhaseCount) return null;
            int best = 0;
            for (int i = 1; i < counts.Length; i++)
                if (counts[i] > counts[best]) best = i;
            if (counts[best] < HabitNeeds) return null;
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

        /// <summary>
        /// Idea 90: the letter after a long absence (LetterAway days or more). Three short lines,
        /// each naming something of this garden (its name, else the last thing grown), with no
        /// loss or guilt words. Null for shorter gaps. Pure: the same garden and days always give
        /// the same letter.
        /// </summary>
        public static string[] AbsenceLetter(WorldModel world, int daysAway)
        {
            if (world == null || daysAway < LetterAway) return null;
            string specific;
            if (world.GardenName.Length > 0)
                specific = world.GardenName + " is still here, and so is everything you grew in it.";
            else
            {
                string thing = LastGrown(world);
                specific = thing != null
                    ? "The " + thing + " you grew is still there, right where you left it."
                    : "Your garden is quiet and ready whenever you are.";
            }
            return new[]
            {
                "It's been a while, and it's lovely to have you back.",
                specific,
                "Nothing needs doing. The garden looked after itself, and it's glad you came.",
            };
        }

        /// <summary>The name of the most recent thing grown in the garden, or null if nothing is grown yet.</summary>
        private static string LastGrown(WorldModel world)
        {
            int[] made = world.Choices;
            for (int i = made.Length - 1; i >= 0; i--)
            {
                if (made[i] < 0 || made[i] > 2) continue;
                return GardenCatalog.For(world.Garden, Math.Min(i, GardenCatalog.ChaptersPerGarden - 1), (VillageChoice)made[i]).Name;
            }
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
