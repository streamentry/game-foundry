using System;

namespace Evergrove.Core
{
    /// <summary>
    /// Item 76: the larder grows. Shelves are bought at the market stall with seeds, and each shelf adds room for
    /// fruit and jam on top of the base caps (MaxFruit, MaxJam). The capacity getters are the caps every store
    /// checks. Nothing here moves goods: a full larder still refuses (LarderFull) and never drops anything.
    /// </summary>
    public sealed partial class WorldModel
    {
        /// <summary>Most shelves a larder can have.</summary>
        public const int MaxShelves = 3;
        /// <summary>Fruit added by each shelf.</summary>
        public const int FruitPerShelf = 20;
        /// <summary>Jars of jam added by each shelf.</summary>
        public const int JamPerShelf = 5;

        /// <summary>Seed price of the first, second and third shelf.</summary>
        private static readonly int[] ShelfPrices = { 150, 300, 600 };

        /// <summary>Shelves bought, 0..<see cref="MaxShelves"/>.</summary>
        public int LarderShelves { get { return state.larderShelves; } }

        /// <summary>Most fruit the larder holds: the base cap plus a shelf's room for each shelf bought.</summary>
        public int FruitCapacity { get { return FruitCapacityFor(state.larderShelves); } }

        /// <summary>Most jars of jam the larder holds: the base cap plus a shelf's room for each shelf bought.</summary>
        public int JamCapacity { get { return JamCapacityFor(state.larderShelves); } }

        /// <summary>Seeds the next shelf costs, or -1 when the larder is full or the garden is not Living.</summary>
        public int NextShelfPrice
        {
            get
            {
                if (Mode != PlayMode.Living || state.larderShelves >= MaxShelves) return -1;
                return ShelfPrices[state.larderShelves];
            }
        }

        public static int FruitCapacityFor(int shelves) { return MaxFruit + FruitPerShelf * Clamp(shelves); }
        public static int JamCapacityFor(int shelves) { return MaxJam + JamPerShelf * Clamp(shelves); }

        private static int Clamp(int shelves) { return Math.Max(0, Math.Min(MaxShelves, shelves)); }

        /// <summary>
        /// Buys the next shelf for its seed price, which is written to the ledger as <see cref="SeedReason.Larder"/>.
        /// Refused, changing nothing, when the larder is full, the garden is not Living, or the purse is short.
        /// </summary>
        public bool BuyShelf()
        {
            int price = NextShelfPrice;
            if (price < 0 || state.seeds < price) return false;
            state.larderShelves++;
            ChangeSeeds(-price, SeedReason.Larder);
            return true;
        }

        /// <summary>Fail-closed validation of the shelf count: 0..MaxShelves, and 0 outside a Living garden.</summary>
        private static bool ReadLarder(WorldSnapshot source)
        {
            return source.larderShelves >= 0 && source.larderShelves <= MaxShelves &&
                (source.mode == "living" || source.larderShelves == 0);
        }
    }
}
