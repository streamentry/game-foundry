using System;

namespace Evergrove.Core
{
    /// <summary>The goods a garden holds or earns. Seeds are the only money; fruit and jam are goods.</summary>
    public enum Good { Fruit, Jam, Seeds }

    /// <summary>
    /// A neighbour's fixed-rate trade: the garden gives <see cref="GiveAmount"/> of <see cref="Give"/> and
    /// receives <see cref="GetAmount"/> of <see cref="Get"/>. Villager is 0 Mira, 1 Orin, 2 Fen (as in GiveFruit).
    /// </summary>
    public struct NeighbourTrade
    {
        public int Villager;
        public Good Give;
        public int GiveAmount;
        public Good Get;
        public int GetAmount;
    }

    /// <summary>
    /// Item 74 (market stall) and item 79 (neighbours trade). Pure tables and prices: no clock, no state.
    /// <para>
    /// DEVIATION (decided, not reopened): Mosselora has no wood or stone and adds no currency. Item 79 is built
    /// over the existing goods only: Orin gives seeds for fruit, Fen gives seeds for jam, and Mira swaps fruit
    /// for a jar of jam. Seeds stay the only money.
    /// </para>
    /// <para>
    /// "By date": the game has no date to season mapping (GardenSeason follows progress, never the clock), so a
    /// price is a function of the garden's season alone. The same season always has the same price, on any day.
    /// </para>
    /// <para>
    /// The no-profit rule. <see cref="ValueOf"/> values a good in the season at the best seeds the garden can
    /// get for it that season (the stall or a neighbour). No sale or trade leaves the value of a basket higher
    /// than before, and cooking never raises it either (see the price table comments). Holding goods across a
    /// season change can still gain up to the spread between seasons; that spread is small and bounded by the
    /// fruit the garden is given through play.
    /// </para>
    /// </summary>
    public static class Market
    {
        /// <summary>Most fruit sold to the stall on one local day.</summary>
        public const int MaxFruitSoldPerDay = 10;
        /// <summary>Most jars of jam sold to the stall on one local day.</summary>
        public const int MaxJamSoldPerDay = 3;
        /// <summary>Most neighbour trades made on one local day, across all neighbours.</summary>
        public const int MaxTradesPerDay = 3;

        /// <summary>Stall price of one fruit, seeds, by season (Spring, Summer, Autumn, Winter).</summary>
        private static readonly int[] FruitPrices = { 3, 2, 1, 3 };

        /// <summary>
        /// Stall price of one jar of jam, seeds, by season. Each is at most three fruit at that season's
        /// stall price, so cooking three fruit never returns more seeds than selling them raw (the spring
        /// and autumn jars are a little less). The stall in autumn is cheapest for fruit; the fair trade
        /// below is what beats it.
        /// </summary>
        private static readonly int[] JamPrices = { 8, 6, 3, 9 };

        /// <summary>
        /// The neighbours' fixed rates, stable through the year. Orin's rate (2 per fruit) beats the stall in
        /// autumn; Fen's (5 per jar) beats it in autumn and summer; the stall wins in spring and winter. Mira asks
        /// 4 fruit for a jar, one more than the kitchen's 3, so cooking your own jam stays the better way.
        /// Every rate is at most the best seeds the garden gets for the good that season (see ValueOf).
        /// </summary>
        private static readonly NeighbourTrade[] TradeTable =
        {
            new NeighbourTrade { Villager = 0, Give = Good.Fruit, GiveAmount = 4, Get = Good.Jam, GetAmount = 1 },
            new NeighbourTrade { Villager = 1, Give = Good.Fruit, GiveAmount = 2, Get = Good.Seeds, GetAmount = 4 },
            new NeighbourTrade { Villager = 2, Give = Good.Jam, GiveAmount = 1, Get = Good.Seeds, GetAmount = 5 }
        };

        /// <summary>The trade table, a copy: changing it changes nothing.</summary>
        public static NeighbourTrade[] Trades { get { return (NeighbourTrade[])TradeTable.Clone(); } }
        public static int TradeCount { get { return TradeTable.Length; } }

        /// <summary>Trade <paramref name="index"/> of the table.</summary>
        public static NeighbourTrade TradeAt(int index)
        {
            if (index < 0 || index >= TradeTable.Length) throw new ArgumentOutOfRangeException(nameof(index));
            return TradeTable[index];
        }

        public static bool IsSeason(Season season) { return (int)season >= 0 && (int)season <= (int)Season.Winter; }

        public static int FruitPrice(Season season) { Check(season); return FruitPrices[(int)season]; }
        public static int JamPrice(Season season) { Check(season); return JamPrices[(int)season]; }

        /// <summary>
        /// The value of one unit of a good in a season, in seeds: the best the garden can get for it that season,
        /// from the stall or from a neighbour that takes it for seeds. Seeds are worth one each. This is the
        /// potential the no-profit proof uses, not a price.
        /// </summary>
        public static int ValueOf(Good good, Season season)
        {
            Check(season);
            switch (good)
            {
                case Good.Fruit: return Math.Max(FruitPrice(season), BestSeedsPerUnit(Good.Fruit));
                case Good.Jam: return Math.Max(JamPrice(season), BestSeedsPerUnit(Good.Jam));
                default: return 1;
            }
        }

        /// <summary>The best whole number of seeds a neighbour pays for one unit of a good (0 when none does).</summary>
        private static int BestSeedsPerUnit(Good good)
        {
            int best = 0;
            foreach (NeighbourTrade trade in TradeTable)
                if (trade.Give == good && trade.Get == Good.Seeds)
                    best = Math.Max(best, trade.GetAmount / trade.GiveAmount);
            return best;
        }

        private static void Check(Season season)
        {
            if (!IsSeason(season)) throw new ArgumentOutOfRangeException(nameof(season));
        }
    }
}
