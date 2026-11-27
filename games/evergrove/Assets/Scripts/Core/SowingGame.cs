using System;

namespace Evergrove.Core
{
    /// <summary>What a tap on a seed bed did. Sowed: the bed was sown. NotRipe: the bed holds seeds, but not as many as its distance to the basket. Empty: nothing to sow. Ignored: no such bed, or the garden is already done.</summary>
    public enum SowMove { Sowed, NotRipe, Empty, Ignored }

    /// <summary>
    /// Sowing (depth spine, Layer 2): a row of seed beds runs out from a basket, bed 1 nearest.
    /// A bed is ripe when it holds exactly as many seeds as its distance to the basket. Sow a
    /// ripe bed: its seeds go one per bed toward the basket, the last into the basket. Clear
    /// every seed into the basket. For any number of seeds exactly one start can be cleared,
    /// so it is a puzzle of reasoning, never of luck. No timer; a bed that is not ripe is
    /// simply refused and nothing changes. Dealt from a seed.
    /// </summary>
    public sealed class SowingGame
    {
        /// <summary>Fewest seeds per tier: 6, 12, 20.</summary>
        public static int MinSeeds(int tier)
        {
            CheckTier(tier);
            return tier == 0 ? 6 : tier == 1 ? 12 : 20;
        }

        /// <summary>Most seeds per tier: 9, 16, 26.</summary>
        public static int MaxSeeds(int tier)
        {
            CheckTier(tier);
            return tier == 0 ? 9 : tier == 1 ? 16 : 26;
        }

        /// <summary>Every sow lands exactly one seed in the basket, so clearing n seeds takes n sows.</summary>
        public static int MovesToClear(int total)
        {
            return total;
        }

        /// <summary>The one clearable start for a given number of seeds.</summary>
        public static SowingGame ForTotal(int total)
        {
            if (total < 1) throw new ArgumentOutOfRangeException(nameof(total));
            return new SowingGame(total);
        }

        private static void CheckTier(int tier)
        {
            if (tier < 0 || tier >= TreePuzzles.Tiers) throw new ArgumentOutOfRangeException(nameof(tier));
        }

        private readonly int[] beds; // index 0 unused; index i is bed i

        public int Beds { get; private set; }
        public int Total { get; private set; }
        public int InBasket { get; private set; }
        public int Moves { get; private set; }
        public bool Solved { get { return InBasket == Total; } }

        public SowingGame(int seed, int tier)
        {
            CheckTier(tier);
            uint state = unchecked((uint)seed * 0x9E3779B1u + (uint)tier * 0x85EBCA6Bu + 0x27D4EB2Fu);
            if (state == 0) state = 0x6d2b79f5u;
            int span = MaxSeeds(tier) - MinSeeds(tier) + 1;
            Total = MinSeeds(tier) + (int)(Next(ref state) % (uint)span);
            beds = Deal(Total);
            Beds = beds.Length - 1;
        }

        private SowingGame(int total)
        {
            Total = total;
            beds = Deal(total);
            Beds = beds.Length - 1;
        }

        private static uint Next(ref uint state)
        {
            state ^= state << 13;
            state ^= state >> 17;
            state ^= state << 5;
            return state;
        }

        /// <summary>
        /// Run the game backwards from an empty board: take a seed from the basket, put it in the
        /// nearest empty bed, and take one from each bed between. Repeat once per seed.
        /// </summary>
        private static int[] Deal(int total)
        {
            var pits = new int[total + 2];
            for (int s = 0; s < total; s++)
            {
                int j = 1;
                while (pits[j] != 0) j++;
                for (int k = 1; k < j; k++) pits[k]--;
                pits[j] = j;
            }
            int top = total;
            while (top > 1 && pits[top] == 0) top--;
            var result = new int[top + 1];
            Array.Copy(pits, result, top + 1);
            return result;
        }

        private bool InRange(int bed) { return bed >= 1 && bed <= Beds; }

        public int Seeds(int bed)
        {
            return InRange(bed) ? beds[bed] : 0;
        }

        public bool IsRipe(int bed)
        {
            return InRange(bed) && beds[bed] == bed;
        }

        public SowMove Sow(int bed)
        {
            if (Solved || !InRange(bed)) return SowMove.Ignored;
            if (beds[bed] == 0) return SowMove.Empty;
            if (beds[bed] != bed) return SowMove.NotRipe;
            beds[bed] = 0;
            for (int k = 1; k < bed; k++) beds[k]++;
            InBasket++;
            Moves++;
            return SowMove.Sowed;
        }

        /// <summary>The bed to sow next: the ripe bed nearest the basket, which always wins; -1 once solved.</summary>
        public int Hint()
        {
            if (Solved) return -1;
            for (int bed = 1; bed <= Beds; bed++)
                if (beds[bed] == bed) return bed;
            return -1;
        }
    }
}
