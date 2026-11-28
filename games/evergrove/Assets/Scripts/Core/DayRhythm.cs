using System;
using System.Globalization;

namespace Evergrove.Core
{
    /// <summary>Checklist 54: what the morning and the evening bring, by local time of day.</summary>
    public enum DayOffer { None, DewBoard, LanternLetter }

    /// <summary>
    /// Checklist 54: a morning and an evening. In the morning the dew is on the grass, so a
    /// small board waits by the garden (dew board). At dusk a neighbour's lantern letter waits
    /// in the letterbox, once a day at most. Pure and deterministic: the day stamp is yyyymmdd.
    /// Nothing here is saved to the garden; the app keeps the day a letter was read, on the device.
    /// </summary>
    public static class DayRhythm
    {
        /// <summary>What the day offers at this phase. Dew in the morning, a lantern letter at dusk, nothing else.</summary>
        public static DayOffer Offer(DayPhase phase, int dayStamp)
        {
            if (!ValidDay(dayStamp)) return DayOffer.None;
            if (phase == DayPhase.Morning) return DayOffer.DewBoard;
            if (phase == DayPhase.Dusk) return DayOffer.LanternLetter;
            return DayOffer.None;
        }

        /// <summary>A lantern letter is due at dusk until one has been read on this day (at most one a day).</summary>
        public static bool LetterDue(DayPhase phase, int dayStamp, int readOn)
        {
            return Offer(phase, dayStamp) == DayOffer.LanternLetter && readOn != dayStamp;
        }

        /// <summary>The neighbour who writes, by the day, so the letters vary without any randomness.</summary>
        public static string LanternFrom(int dayStamp)
        {
            return Speakers[Math.Abs(dayStamp % Speakers.Length)];
        }

        /// <summary>The lantern letter's words, by the day. Short, gentle, no reply needed.</summary>
        public static string LanternWords(int dayStamp)
        {
            return Letters[Math.Abs(dayStamp % Letters.Length)];
        }

        /// <summary>
        /// The lantern letter's words, naming the thing the player chose last when the save proves it.
        /// Falls back to the generic letter when there is no such choice.
        /// </summary>
        public static string LanternWords(int dayStamp, WorldModel world)
        {
            string thing = LatestThing(world);
            if (thing == null) return LanternWords(dayStamp);
            string template = NamedLetters[Math.Abs(dayStamp % NamedLetters.Length)];
            return string.Format(CultureInfo.InvariantCulture, template, thing);
        }

        /// <summary>The name of the thing from the most recent saved village choice, or null when the save shows none.</summary>
        public static string LatestThing(WorldModel world)
        {
            if (world == null || world.Mode != PlayMode.Living) return null;
            int[] choices = world.Choices;
            if (choices.Length == 0) return null;
            int i = choices.Length - 1;
            if (choices[i] < 0 || choices[i] > 2) return null;
            return GardenCatalog.For(world.Garden, Math.Min(i, GardenCatalog.ChaptersPerGarden - 1), (VillageChoice)choices[i]).Name;
        }

        /// <summary>The dew board's framing line, shown on the token.</summary>
        public const string DewBoardWords = "Dew on the grass this morning. A small board, if you fancy it.";

        private static readonly string[] Speakers = { "Mira", "Orin", "Fen" };

        private static readonly string[] Letters =
        {
            "I've lit the lantern by the bridge tonight. The river is easy to hear from here. No need to write back.",
            "The lamp is lit on my step and the path is clear. Sit with it a while if you like. Nothing to answer.",
            "Dusk came down soft over the water. I thought of your garden and left you a light. Goodnight, neighbour.",
        };

        private static readonly string[] NamedLetters =
        {
            "I've hung the lantern by your {0} tonight. The river is easy to hear from there. No need to write back.",
            "The lamp is lit beside your {0}, and the path is clear. Sit with it a while if you like.",
            "Dusk came down soft over your {0}. I left you a light there. Goodnight, neighbour.",
        };

        private static bool ValidDay(int dayStamp)
        {
            return dayStamp >= 20000101 && dayStamp <= 99991231;
        }
    }
}
