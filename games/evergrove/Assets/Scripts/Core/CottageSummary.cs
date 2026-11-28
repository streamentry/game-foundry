using System;

namespace Evergrove.Core
{
    /// <summary>
    /// I299 (idea 36, after the inn in classic role-playing games, the calm place where you
    /// see where you stand): tap your cottage and see, at a glance, what's truly waiting.
    /// Counts only; no pressure, nothing invented.
    /// </summary>
    public sealed class CottageSummary
    {
        /// <summary>Things (grown and bought) still holding their first puzzle.</summary>
        public int Waiting { get; private set; }
        /// <summary>Unread letters, as the app counts them on this device.</summary>
        public int Letters { get; private set; }
        /// <summary>Cuttings from family waiting in the basket.</summary>
        public int Cuttings { get; private set; }
        /// <summary>Drops on the mist toward the next stretch; DropsNeed is 0 when no more river can open.</summary>
        public int DropsHave { get; private set; }
        public int DropsNeed { get; private set; }
        public int Seeds { get; private set; }
        public int Fruit { get; private set; }
        /// <summary>Days this garden has been played: a total that only grows, never a streak (v1 #52).</summary>
        public int DaysPlayed { get; private set; }
        public Season Season { get; private set; }

        public bool Anything { get { return Waiting > 0 || Letters > 0 || Cuttings > 0; } }

        public static CottageSummary For(WorldModel world, int unreadLetters)
        {
            if (world == null) return null;
            int waiting = 0;
            int[] made = world.Choices;
            for (int i = 0; i < made.Length; i++) if (!world.TreeSolved(i)) waiting++;
            for (int k = 0; k < world.BoughtCount; k++) if (!world.BoughtSolved(k)) waiting++;
            bool growing = world.TowardNextStretch(out int have, out int need);
            return new CottageSummary
            {
                Waiting = waiting,
                Letters = Math.Max(0, unreadLetters),
                Cuttings = world.Gifts.Length,
                DropsHave = growing ? have : 0,
                DropsNeed = growing ? need : 0,
                Seeds = world.Seeds,
                Fruit = world.Fruit,
                DaysPlayed = world.DaysPlayed,
                Season = GardenSeason.For(world)
            };
        }
    }
}
