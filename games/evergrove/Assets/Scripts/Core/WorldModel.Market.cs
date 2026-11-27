using System;

namespace Evergrove.Core
{
    /// <summary>
    /// Items 74 and 79: the market stall and the neighbours' trades. Fruit and jam are sold for seeds, or
    /// traded, at prices fixed by the garden's season. Nothing here creates fruit, jam or seeds from nothing:
    /// every sale and trade removes what it pays out, and every seed change goes through ChangeSeeds with a
    /// SeedReason. Living gardens only. Every call is all-or-nothing: a refused call changes nothing.
    /// <para>
    /// The daily caps (Market.MaxFruitSoldPerDay, MaxJamSoldPerDay, MaxTradesPerDay) follow the replay cap's
    /// rule: a later day starts a fresh count, and an earlier day counts against the latest day already used, so
    /// setting the clock back cannot reset them. A day of zero or less, or a day outside a yyyymmdd range, is refused.
    /// </para>
    /// </summary>
    public sealed partial class WorldModel
    {
        /// <summary>The local day (yyyymmdd) the market's daily caps count for; 0 none yet.</summary>
        public int MarketDay { get { return state.marketStamp; } }

        /// <summary>Fruit sold to the stall on <paramref name="day"/> so far (0 on any later day).</summary>
        public int FruitSoldOn(int day) { return SoldOn(day, state.fruitSold); }
        /// <summary>Jars of jam sold to the stall on <paramref name="day"/> so far.</summary>
        public int JamSoldOn(int day) { return SoldOn(day, state.jamSold); }
        /// <summary>Neighbour trades made on <paramref name="day"/> so far.</summary>
        public int TradesOn(int day) { return SoldOn(day, state.tradesDone); }

        /// <summary>
        /// Sells <paramref name="count"/> fruit to the stall at the season's price, all for seeds. <paramref name="season"/>
        /// must be the garden's own season (GardenSeason.For), so the caller cannot pick a dearer one: otherwise
        /// refused. Returns the seeds earned, or 0 with nothing changed when refused.
        /// </summary>
        public int SellFruit(int count, Season season, int day)
        {
            if (!CanSell(Good.Fruit, count, season, day)) return 0;
            int price = Market.FruitPrice(season);
            int earned = count * price;
            StartMarketDay(day);
            state.fruit -= count;
            state.fruitSold += count;
            ChangeSeeds(earned, SeedReason.MarketFruit);
            return earned;
        }

        /// <summary>As <see cref="SellFruit"/>, for jars of jam.</summary>
        public int SellJam(int count, Season season, int day)
        {
            if (!CanSell(Good.Jam, count, season, day)) return 0;
            int price = Market.JamPrice(season);
            int earned = count * price;
            StartMarketDay(day);
            state.jam -= count;
            state.jamSold += count;
            ChangeSeeds(earned, SeedReason.MarketJam);
            return earned;
        }

        /// <summary>
        /// Makes neighbour trade <paramref name="tradeIndex"/> (see Market.Trades) on <paramref name="day"/>.
        /// Gives the goods the trade asks for and receives the goods it pays. Refused, changing nothing, when the
        /// garden lacks the goods, a receiving store would overflow (seeds, fruit or jam), or the day's trades are used up.
        /// </summary>
        public bool Trade(int tradeIndex, int day)
        {
            if (Mode != PlayMode.Living || tradeIndex < 0 || tradeIndex >= Market.TradeCount || !MarketDayOk(day)) return false;
            NeighbourTrade trade = Market.TradeAt(tradeIndex);
            if (TradesOn(day) >= Market.MaxTradesPerDay || Held(trade.Give) < trade.GiveAmount || !HasRoom(trade.Get, trade.GetAmount))
                return false;
            StartMarketDay(day);
            state.tradesDone++;
            Remove(trade.Give, trade.GiveAmount);
            Receive(trade.Get, trade.GetAmount);
            return true;
        }

        /// <summary>Validates a sale, without changing anything: living, a real day, a whole count within the day's cap, the garden's own season, and goods to sell that fit the seeds.</summary>
        private bool CanSell(Good good, int count, Season season, int day)
        {
            if (Mode != PlayMode.Living || count < 1 || !MarketDayOk(day) || !Market.IsSeason(season)) return false;
            if (season != GardenSeason.For(this)) return false;
            bool fruit = good == Good.Fruit;
            int cap = fruit ? Market.MaxFruitSoldPerDay : Market.MaxJamSoldPerDay;
            int soldSoFar = fruit ? FruitSoldOn(day) : JamSoldOn(day);
            if (count > cap - soldSoFar || Held(good) < count) return false;
            long earned = (long)count * (fruit ? Market.FruitPrice(season) : Market.JamPrice(season));
            return state.seeds + earned <= MaxSeeds;
        }

        private static bool MarketDayOk(int day) { return day >= MinPayDay && day <= MaxPayDay; }

        /// <summary>Only a later day starts a fresh count (see the class note).</summary>
        private void StartMarketDay(int day)
        {
            if (day <= state.marketStamp) return;
            state.marketStamp = day;
            state.fruitSold = 0;
            state.jamSold = 0;
            state.tradesDone = 0;
        }

        private int SoldOn(int day, int stored)
        {
            if (!MarketDayOk(day)) return 0;
            return day > state.marketStamp ? 0 : stored;
        }

        private int Held(Good good)
        {
            switch (good)
            {
                case Good.Fruit: return state.fruit;
                case Good.Jam: return state.jam;
                default: return state.seeds;
            }
        }

        private bool HasRoom(Good good, int amount)
        {
            switch (good)
            {
                case Good.Fruit: return state.fruit + amount <= MaxFruit;
                case Good.Jam: return state.jam + amount <= MaxJam;
                default: return (long)state.seeds + amount <= MaxSeeds;
            }
        }

        private void Remove(Good good, int amount)
        {
            if (good == Good.Fruit) state.fruit -= amount;
            else if (good == Good.Jam) state.jam -= amount;
            else ChangeSeeds(-amount, SeedReason.Trade);
        }

        private void Receive(Good good, int amount)
        {
            if (good == Good.Fruit) state.fruit += amount;
            else if (good == Good.Jam) state.jam += amount;
            else ChangeSeeds(amount, SeedReason.Trade);
        }

        /// <summary>
        /// Fail-closed validation of the market's day stamp and counts (items 74 and 79). Each count is within its
        /// cap, a stamp of 0 holds no counts, and every count is zero outside a Living garden.
        /// </summary>
        private static bool ReadMarket(WorldSnapshot source)
        {
            bool dayOk = source.marketStamp == 0 || (source.marketStamp >= MinPayDay && source.marketStamp <= MaxPayDay);
            bool countsOk = source.fruitSold >= 0 && source.fruitSold <= Market.MaxFruitSoldPerDay &&
                source.jamSold >= 0 && source.jamSold <= Market.MaxJamSoldPerDay &&
                source.tradesDone >= 0 && source.tradesDone <= Market.MaxTradesPerDay;
            bool emptyWhenNoDay = source.marketStamp != 0 || (source.fruitSold == 0 && source.jamSold == 0 && source.tradesDone == 0);
            bool livingOnly = source.mode == "living" ||
                (source.marketStamp == 0 && source.fruitSold == 0 && source.jamSold == 0 && source.tradesDone == 0);
            return dayOk && countsOk && emptyWhenNoDay && livingOnly;
        }
    }
}
