using System;
using System.Collections.Generic;
using System.Linq;
using Evergrove.Core;

/// <summary>
/// Items 74 and 79: the market stall and the neighbours' trades. Fruit and jam are sold for seeds at the
/// garden's season price, or traded at fixed rates. Nothing here creates goods: the no-profit proof and a
/// 365-day greedy bot check that the stall and the trades convert what the garden has and never create value.
/// </summary>
internal static class MarketTests
{
    private static int assertions;
    private const int Day1 = 20270101;
    private const int Day2 = 20270102;
    private const int Day3 = 20270103;
    private const int Days = 365;
    private const int MaxCycle = 6;
    private const string Stamp = "2027-01-01T12:00:00Z";
    private const int MiraTrade = 0;
    private const int OrinTrade = 1;
    private const int FenTrade = 2;
    private static readonly DateTime FirstDay = new DateTime(2027, 1, 1);
    private static readonly Season[] Seasons = { Season.Spring, Season.Summer, Season.Autumn, Season.Winter };

    private static void Expect(bool ok, string message)
    {
        assertions++;
        if (!ok) throw new Exception("Market test failed: " + message);
    }

    private static string State(WorldModel w) { return GardenTransfer.SnapshotJson(w.Export()); }
    private static WorldSnapshot Tweak(WorldSnapshot s, Action<WorldSnapshot> change) { change(s); return s; }

    private static bool Throws(Action act)
    {
        try { act(); return false; }
        catch (ArgumentOutOfRangeException) { return true; }
    }

    // The starting garden of the economy tests: twelve grown things, none solved, in summer.
    private static WorldModel Grown()
    {
        WorldModel world = WorldModel.New(PlayMode.Living);
        for (int chapter = 0; chapter < PuzzleCatalog.Count; chapter++)
        {
            Puzzle board = world.CurrentPuzzle;
            for (int i = 0; i < board.CellCount; i++)
                while (world.Rotations[i] != board.Solution[i]) world.Rotate(i);
            Expect(world.Choose((VillageChoice)(chapter % 3)), "chapter " + chapter + " is chosen");
        }
        return world;
    }

    // A living garden with goods, in the given season. Each season turn is SolvesPerReach first solves; summer is turn 0.
    private static WorldModel Garden(Season season, int fruit = 0, int jam = 0, int seeds = 0)
    {
        int turns = ((int)season - (int)Season.Summer + 4) % 4;
        WorldSnapshot s = Grown().Export();
        s.treeSolved = Enumerable.Range(0, turns * GardenMap.SolvesPerReach).ToArray();
        s.fruit = fruit;
        s.jam = jam;
        s.seeds = seeds;
        Expect(WorldModel.TryRestore(s, out WorldModel w), season + " garden with " + fruit + " fruit, " + jam + " jam restores");
        Expect(GardenSeason.For(w) == season, "the garden is in " + season);
        return w;
    }

    public static int Run()
    {
        Tables();
        Pricing();
        Selling();
        Refusals();
        DailyCaps();
        Trading();
        Saving();
        Forgeries();
        NoProfitLoop();
        ArbitrageBot();
        return assertions;
    }

    private static void Tables()
    {
        Expect(Market.MaxFruitSoldPerDay == 10 && Market.MaxJamSoldPerDay == 3 && Market.MaxTradesPerDay == 3,
            "the daily caps: 10 fruit, 3 jam, 3 trades");
        Expect(Market.TradeCount == 3, "three neighbours trade");

        NeighbourTrade mira = Market.TradeAt(MiraTrade), orin = Market.TradeAt(OrinTrade), fen = Market.TradeAt(FenTrade);
        Expect(mira.Villager == 0 && mira.Give == Good.Fruit && mira.GiveAmount == 4 && mira.Get == Good.Jam && mira.GetAmount == 1,
            "Mira: four fruit for a jar of jam");
        Expect(orin.Villager == 1 && orin.Give == Good.Fruit && orin.GiveAmount == 2 && orin.Get == Good.Seeds && orin.GetAmount == 4,
            "Orin: two fruit for four seeds");
        Expect(fen.Villager == 2 && fen.Give == Good.Jam && fen.GiveAmount == 1 && fen.Get == Good.Seeds && fen.GetAmount == 5,
            "Fen: a jar of jam for five seeds");
        foreach (NeighbourTrade t in Market.Trades)
            Expect(t.GiveAmount > 0 && t.GetAmount > 0 && t.Give != Good.Seeds && t.Get != Good.Fruit,
                "trade for villager " + t.Villager + " only takes fruit or jam and never pays fruit");

        NeighbourTrade[] copy = Market.Trades;
        copy[0].GiveAmount = 99;
        Expect(Market.TradeAt(MiraTrade).GiveAmount == 4, "the trade table cannot be changed through its copy");
        Expect(Throws(() => Market.TradeAt(3)) && Throws(() => Market.TradeAt(-1)), "no trade past the table");
        Expect(Throws(() => Market.FruitPrice((Season)4)) && Throws(() => Market.JamPrice((Season)(-1))), "no price for a made-up season");
    }

    private static void Pricing()
    {
        int[] fruit = { 3, 2, 1, 3 };
        int[] jam = { 8, 6, 3, 9 };
        for (int i = 0; i < Seasons.Length; i++)
        {
            Expect(Market.FruitPrice(Seasons[i]) == fruit[i] && Market.JamPrice(Seasons[i]) == jam[i],
                "stall prices in " + Seasons[i] + " are " + fruit[i] + " fruit and " + jam[i] + " jam");
            Expect(Market.JamPrice(Seasons[i]) <= WorldModel.JamPerJar * Market.FruitPrice(Seasons[i]),
                "in " + Seasons[i] + ", a jar sells for no more than the three fruit it took");
        }
        Expect(Market.FruitPrice(Season.Autumn) < Market.FruitPrice(Season.Spring) &&
               Market.FruitPrice(Season.Autumn) < Market.FruitPrice(Season.Winter), "fruit is cheapest at the autumn harvest");
        Expect(Market.FruitPrice(Season.Winter) > Market.FruitPrice(Season.Summer), "fruit is dearer in winter than in summer");

        // The valuation: the best seeds the garden gets for a good that season (the stall or a neighbour).
        Expect(Market.ValueOf(Good.Fruit, Season.Spring) == 3 && Market.ValueOf(Good.Fruit, Season.Autumn) == 2 &&
               Market.ValueOf(Good.Jam, Season.Autumn) == 5 && Market.ValueOf(Good.Jam, Season.Winter) == 9 &&
               Market.ValueOf(Good.Seeds, Season.Summer) == 1, "valuation of fruit, jam and seeds by season");
    }

    private static void Selling()
    {
        WorldModel w = Garden(Season.Summer, fruit: 5, seeds: 10);
        Expect(w.SellFruit(3, Season.Summer, Day1) == 6 && w.Fruit == 2 && w.Seeds == 16,
            "three fruit at summer's two each earn six seeds");
        Expect(w.Ledger.Length == 1 && w.Ledger[0].Reason == SeedReason.MarketFruit && w.Ledger[0].Delta == 6,
            "one MarketFruit ledger line of +6");
        Expect(w.MarketDay == Day1 && w.FruitSoldOn(Day1) == 3 && w.JamSoldOn(Day1) == 0, "the day's count is kept");

        WorldModel j = Garden(Season.Summer, jam: 2);
        Expect(j.SellJam(2, Season.Summer, Day1) == 12 && j.Jam == 0 && j.Seeds == 12, "two jars at six each earn twelve");
        Expect(j.Ledger.Length == 1 && j.Ledger[0].Reason == SeedReason.MarketJam && j.Ledger[0].Delta == 12,
            "one MarketJam ledger line of +12");

        foreach (Season s in Seasons)
        {
            WorldModel f = Garden(s, fruit: 1);
            Expect(f.SellFruit(1, s, Day1) == Market.FruitPrice(s), "one fruit in " + s + " earns the stall price");
            WorldModel k = Garden(s, jam: 1);
            Expect(k.SellJam(1, s, Day1) == Market.JamPrice(s), "one jar in " + s + " earns the stall price");
        }

        // Prices follow the season, never the date: the same season sells for the same on any day.
        WorldModel day1 = Garden(Season.Autumn, fruit: 1);
        WorldModel day3 = Garden(Season.Autumn, fruit: 1);
        Expect(day1.SellFruit(1, Season.Autumn, Day1) == day3.SellFruit(1, Season.Autumn, Day3) && Market.FruitPrice(Season.Autumn) == 1,
            "the same season has the same price on any day");
    }

    private static void Refusals()
    {
        WorldModel few = Garden(Season.Summer, fruit: 2, seeds: 5);
        string before = State(few);
        Expect(few.SellFruit(3, Season.Summer, Day1) == 0 && State(few) == before, "not enough fruit: refused, nothing changes");
        Expect(few.SellFruit(0, Season.Summer, Day1) == 0 && State(few) == before, "no fruit sold: refused, nothing changes");
        Expect(few.SellFruit(-1, Season.Summer, Day1) == 0 && State(few) == before, "negative count: refused, nothing changes");
        Expect(few.SellJam(1, Season.Summer, Day1) == 0 && State(few) == before, "no jam to sell: refused, nothing changes");
        Expect(few.SellFruit(1, Season.Summer, 0) == 0 && State(few) == before, "day 0: refused");
        Expect(few.SellFruit(1, Season.Summer, -1) == 0 && State(few) == before, "negative day: refused");
        Expect(few.SellFruit(1, Season.Summer, 12345) == 0 && State(few) == before, "a day before 1900 is refused");
        Expect(few.SellFruit(1, Season.Summer, 99999999) == 0 && State(few) == before, "a day past the range is refused");
        Expect(few.SellFruit(1, Season.Winter, Day1) == 0 && State(few) == before, "a season the garden is not in: refused");
        Expect(few.SellFruit(1, (Season)9, Day1) == 0 && State(few) == before, "a made-up season: refused");
        Expect(few.Trade(OrinTrade, Day1) && few.Fruit == 0 && few.Seeds == 9, "a trade with exactly enough fruit still works");
        string afterTrade = State(few);
        Expect(!few.Trade(OrinTrade, Day1) && State(few) == afterTrade, "Orin with one fruit left: refused, nothing changes");
        Expect(!few.Trade(3, Day1) && !few.Trade(-1, Day1) && !few.Trade(OrinTrade, 0) && State(few) == afterTrade,
            "a trade past the table or on day 0: refused, nothing changes");

        WorldModel puzzle = WorldModel.New(PlayMode.PuzzleOnly);
        WorldModel matched = WorldModel.NewMatched(Enumerable.Repeat(1, WorldModel.ScheduleLength).ToArray());
        foreach (WorldModel p in new[] { puzzle, matched })
        {
            string pBefore = State(p);
            Expect(p.SellFruit(1, Season.Summer, Day1) == 0 && p.SellJam(1, Season.Summer, Day1) == 0 &&
                   !p.Trade(OrinTrade, Day1) && State(p) == pBefore, p.Mode + ": no market at all, nothing changes");
        }

        WorldModel room = Garden(Season.Summer, fruit: 1, seeds: WorldModel.MaxSeeds);
        string full = State(room);
        Expect(room.SellFruit(1, Season.Summer, Day1) == 0 && State(room) == full, "seeds full: a sale is refused, the fruit is kept");
        WorldModel nearly = Garden(Season.Summer, fruit: 2, seeds: WorldModel.MaxSeeds - 3);
        string nearlyBefore = State(nearly);
        Expect(nearly.SellFruit(2, Season.Summer, Day1) == 0 && State(nearly) == nearlyBefore,
            "a sale that would overflow the seeds is refused whole, no silent loss");
        Expect(nearly.SellFruit(1, Season.Summer, Day1) == 2 && nearly.Seeds == WorldModel.MaxSeeds - 1,
            "a smaller sale that fits still works");
        Expect(!nearly.Trade(OrinTrade, Day1) && nearly.Fruit == 1, "an overflowing trade is refused whole");
    }

    private static void DailyCaps()
    {
        WorldModel w = Garden(Season.Summer, fruit: 30, jam: 4);
        Expect(w.SellFruit(10, Season.Summer, Day1) == 20, "ten fruit sell on day one");
        string before = State(w);
        Expect(w.SellFruit(1, Season.Summer, Day1) == 0 && State(w) == before, "the eleventh fruit on day one is refused");
        Expect(w.SellFruit(1, Season.Summer, Day2) == 2 && w.FruitSoldOn(Day2) == 1, "a later day starts a fresh count");
        Expect(w.SellFruit(9, Season.Summer, Day1) == 18 && w.FruitSoldOn(Day2) == 10,
            "setting the clock back counts against the latest day, so it cannot reset the cap");
        Expect(w.SellFruit(1, Season.Summer, Day1) == 0 && w.FruitSoldOn(Day2) == 10, "still refused after the clock went back");

        Expect(w.SellJam(3, Season.Summer, Day1) == 18 && w.JamSoldOn(Day2) == 3, "three jars sell on the same day");
        Expect(w.SellJam(1, Season.Summer, Day2) == 0 && w.Jam == 1, "a fourth jar is refused");

        Expect(w.Trade(OrinTrade, Day2) && w.Trade(OrinTrade, Day2) && w.Trade(FenTrade, Day2) && w.TradesOn(Day2) == 3,
            "three trades on one day");
        string full = State(w);
        Expect(!w.Trade(OrinTrade, Day2) && !w.Trade(MiraTrade, Day2) && State(w) == full, "a fourth trade on the day is refused");
        Expect(w.Trade(FenTrade, Day3) == false && w.Jam == 0, "the jar is gone; Fen waits for a jar");

        WorldModel batch = Garden(Season.Summer, fruit: 12);
        Expect(batch.SellFruit(8, Season.Summer, Day1) == 16, "eight fruit");
        string partial = State(batch);
        Expect(batch.SellFruit(3, Season.Summer, Day1) == 0 && State(batch) == partial,
            "three more would pass the cap: the whole sale is refused");
        Expect(batch.SellFruit(2, Season.Summer, Day1) == 4 && batch.FruitSoldOn(Day1) == 10, "the last two still fit");
    }

    private static void Trading()
    {
        WorldModel mira = Garden(Season.Summer, fruit: 4);
        Expect(mira.Trade(MiraTrade, Day1) && mira.Fruit == 0 && mira.Jam == 1 && mira.Seeds == 0 && mira.Ledger.Length == 0,
            "Mira: four fruit become a jar, no seeds move, no ledger line");
        Expect(mira.TradesOn(Day1) == 1, "the trade counts for the day");

        WorldModel orin = Garden(Season.Autumn, fruit: 3);
        Expect(orin.Trade(OrinTrade, Day1) && orin.Fruit == 1 && orin.Seeds == 4 &&
               orin.Ledger.Length == 1 && orin.Ledger[0].Reason == SeedReason.Trade && orin.Ledger[0].Delta == 4,
            "Orin: two fruit for four seeds, one Trade ledger line");

        WorldModel fen = Garden(Season.Summer, jam: 2);
        Expect(fen.Trade(FenTrade, Day1) && fen.Jam == 1 && fen.Seeds == 5 &&
               fen.Ledger.Length == 1 && fen.Ledger[0].Reason == SeedReason.Trade && fen.Ledger[0].Delta == 5,
            "Fen: a jar for five seeds, one Trade ledger line");

        WorldModel jammed = Garden(Season.Summer, fruit: 4, jam: WorldModel.MaxJam);
        string before = State(jammed);
        Expect(!jammed.Trade(MiraTrade, Day1) && State(jammed) == before, "a full pantry refuses Mira, nothing is lost");

        // Orin beats the stall in autumn, Fen beats it in autumn and summer, and the stall wins in spring and winter.
        Expect(Market.TradeAt(OrinTrade).GetAmount / Market.TradeAt(OrinTrade).GiveAmount > Market.FruitPrice(Season.Autumn) &&
               Market.TradeAt(OrinTrade).GetAmount / Market.TradeAt(OrinTrade).GiveAmount < Market.FruitPrice(Season.Spring),
            "Orin is the better buyer in autumn, the stall in spring");
        Expect(Market.TradeAt(FenTrade).GetAmount > Market.JamPrice(Season.Autumn) &&
               Market.TradeAt(FenTrade).GetAmount < Market.JamPrice(Season.Spring),
            "Fen is the better buyer in autumn, the stall in spring");
    }

    private static void Saving()
    {
        WorldModel plain = Garden(Season.Summer, fruit: 4);
        string text = GardenTransfer.SnapshotJson(plain.Export());
        Expect(!text.Contains("marketStamp") && !text.Contains("fruitSold") && !text.Contains("jamSold") && !text.Contains("tradesDone"),
            "a garden that never traded writes none of the new keys");

        WorldModel w = Garden(Season.Summer, fruit: 20, jam: 0);
        Expect(w.SellFruit(4, Season.Summer, Day1) == 8 && w.Trade(MiraTrade, Day1) && w.Jam == 1 && w.SellJam(1, Season.Summer, Day1) == 6,
            "a day of sales and a trade");
        string saved = State(w);
        string exported = GardenTransfer.Export(w, Stamp);
        Expect(GardenTransfer.TryImport(exported, out WorldModel back, out GardenImportError error) && error == GardenImportError.None,
            "the export imports");
        Expect(State(back) == saved, "the same garden comes back, market day and all");
        Expect(back.MarketDay == Day1 && back.FruitSoldOn(Day1) == 4 && back.JamSoldOn(Day1) == 1 && back.TradesOn(Day1) == 1,
            "the day's counts survive a save");
        Expect(back.SellFruit(7, Season.Summer, Day1) == 0 && back.SellFruit(6, Season.Summer, Day1) == 12,
            "the restored garden keeps the cap: four sold, so six more fit and seven do not");
    }

    private static WorldSnapshot Sold()
    {
        WorldModel w = Garden(Season.Summer, fruit: 20, jam: 1);
        Expect(w.SellFruit(4, Season.Summer, Day1) == 8 && w.Trade(MiraTrade, Day1) && w.SellJam(1, Season.Summer, Day1) == 6,
            "the honest save is made");
        WorldSnapshot s = w.Export();
        Expect(WorldModel.TryRestore(s, out _), "the honest save restores");
        return s;
    }

    private static void Forgeries()
    {
        void Refused(string what, Action<WorldSnapshot> change)
        {
            WorldSnapshot s = Tweak(Sold(), change);
            Expect(!WorldModel.TryRestore(s, out _), what + " is refused");
            string text = GardenTransfer.ExportSnapshot(s, Stamp);
            Expect(!GardenTransfer.TryImport(text, out _, out GardenImportError e) && e == GardenImportError.InvalidSnapshot,
                what + " is refused on import, even with a matching checksum");
        }

        Refused("a day before the calendar range", s => s.marketStamp = 12345);
        Refused("a counted day of 0", s => s.marketStamp = 0);
        Refused("more fruit than the day allows", s => s.fruitSold = Market.MaxFruitSoldPerDay + 1);
        Refused("negative fruit sold", s => s.fruitSold = -1);
        Refused("more jam than the day allows", s => s.jamSold = Market.MaxJamSoldPerDay + 1);
        Refused("more trades than the day allows", s => s.tradesDone = Market.MaxTradesPerDay + 1);
        Refused("market counts in a puzzle-only garden", s => { s.mode = "puzzle"; s.jamSold = 0; s.fruitSold = 0; });
        Refused("market counts with the mode changed", s => s.mode = "matched");
    }

    private static void NoProfitLoop()
    {
        // Every edge of the goods graph, per season, valued with Market.ValueOf (seeds are worth 1).
        // A sale: fruit or jam for seeds. A trade: its give for its get. Cooking: three fruit for a jar.
        int closedTotal = 0;
        long walksTotal = 0;
        foreach (Season s in Seasons)
        {
            var edges = new List<Edge>();
            edges.Add(new Edge(Good.Fruit, Good.Seeds, 1, Market.FruitPrice(s)));
            edges.Add(new Edge(Good.Jam, Good.Seeds, 1, Market.JamPrice(s)));
            foreach (NeighbourTrade t in Market.Trades)
                edges.Add(new Edge(t.Give, t.Get, t.GiveAmount, t.GetAmount));
            edges.Add(new Edge(Good.Fruit, Good.Jam, WorldModel.JamPerJar, 1));

            foreach (Edge e in edges)
                Expect(e.Gain(s) <= 0, "in " + s + ", " + e.From + " to " + e.To + " never gains value (gain " + e.Gain(s) + ")");
            Expect(edges.All(e => e.To != Good.Fruit), "in " + s + ", no conversion turns anything back into fruit");

            foreach (Good start in new[] { Good.Fruit, Good.Jam, Good.Seeds })
            {
                int closed = 0;
                int worst = int.MinValue;
                long walks = 0;
                Walk(edges, s, start, start, 0, 0, ref closed, ref worst, ref walks);
                closedTotal += closed;
                walksTotal += walks;
                Expect(closed == 0 || worst <= 0, "in " + s + ", every closed walk from " + start + " loses value (worst " + worst + ")");
            }
        }
        Expect(closedTotal == 0, "no sequence of sales, trades and cooking up to " + MaxCycle + " steps returns to where it started");
        Expect(walksTotal > 0, "the walks were explored (" + walksTotal + ")");

        int lo = Seasons.Min(s => Market.ValueOf(Good.Fruit, s));
        int hi = Seasons.Max(s => Market.ValueOf(Good.Fruit, s));
        Expect(hi - lo <= 1, "fruit is worth at most one seed more in one season than another (" + lo + " to " + hi + ")");
        Console.WriteLine("Market no-profit: " + walksTotal + " walks up to " + MaxCycle + " steps over four seasons, no closed walk.");
    }

    // A conversion: gives `from` x `inAmount` and receives `to` x `outAmount`.
    private readonly struct Edge
    {
        public readonly Good From;
        public readonly Good To;
        public readonly int InAmount;
        public readonly int OutAmount;

        public Edge(Good from, Good to, int inAmount, int outAmount)
        {
            From = from;
            To = to;
            InAmount = inAmount;
            OutAmount = outAmount;
        }

        public int Gain(Season s) { return OutAmount * Market.ValueOf(To, s) - InAmount * Market.ValueOf(From, s); }
    }

    // Brute force: every walk of at most MaxCycle edges from `at`. Counts walks that return to `start`.
    private static void Walk(List<Edge> edges, Season s, Good start, Good at, int length, int gain,
                             ref int closed, ref int worst, ref long walks)
    {
        if (length > 0)
        {
            walks++;
            if (at == start)
            {
                closed++;
                worst = Math.Max(worst, gain);
            }
        }
        if (length == MaxCycle) return;
        foreach (Edge e in edges)
            if (e.From == at)
                Walk(edges, s, start, e.To, length + 1, gain + e.Gain(s), ref closed, ref worst, ref walks);
    }

    // The 365-day greedy bot. It plays one first solve a day while grown things remain, then replays the
    // ShadeTree things (fruit follows the first three full-paid replays of a day), then sells as well as it can.
    private sealed class BotRun
    {
        public long PlayFruit;
        public long MarketSeeds;
        public int EndSeeds;
    }

    private static void ArbitrageBot()
    {
        BotRun first = Bot();
        BotRun second = Bot();
        Expect(first.MarketSeeds == second.MarketSeeds && first.EndSeeds == second.EndSeeds && first.PlayFruit == second.PlayFruit,
            "the bot's year is deterministic");

        int maxFruitValue = Seasons.Max(s => Market.ValueOf(Good.Fruit, s));
        // Fruit comes only from play: at most the three full-paid replays a day, plus one for each ShadeTree first solve.
        int shadeThings = Enumerable.Range(0, PuzzleCatalog.Count).Count(i => i % 3 == (int)VillageChoice.ShadeTree);
        Expect(first.PlayFruit <= (long)WorldModel.FullReplaysPerDay * Days + shadeThings,
            "fruit from play is capped by the daily rule (" + first.PlayFruit + " over a year)");
        Expect(first.MarketSeeds > 0, "the bot did sell something");
        Expect(first.MarketSeeds <= first.PlayFruit * maxFruitValue,
            "the stall and trades convert, never create: " + first.MarketSeeds + " seeds from " + first.PlayFruit +
            " fruit, at most " + maxFruitValue + " each");
        Console.WriteLine("Market bot 365 days: " + first.PlayFruit + " fruit from play, " + first.MarketSeeds +
                          " seeds from the stall and trades, end balance " + first.EndSeeds + ".");
    }

    private static long Potential(WorldModel w, Season s)
    {
        return w.Seeds + (long)w.Fruit * Market.ValueOf(Good.Fruit, s) + (long)w.Jam * Market.ValueOf(Good.Jam, s);
    }

    private static BotRun Bot()
    {
        WorldModel world = Grown();
        var run = new BotRun();
        int[] shade = Enumerable.Range(0, world.Choices.Length).Where(i => world.Choices[i] == (int)VillageChoice.ShadeTree).ToArray();
        for (int day = 0; day < Days; day++)
        {
            DateTime date = FirstDay.AddDays(day);
            int key = date.Year * 10000 + date.Month * 100 + date.Day;

            int unsolved = Enumerable.Range(0, world.Choices.Length).FirstOrDefault(i => !world.TreeSolved(i));
            if (!world.TreeSolved(unsolved)) Play(world, unsolved, key, run);
            for (int k = 0; k < WorldModel.FullReplaysPerDay + 1; k++)
                Play(world, shade[(day + k) % shade.Length], key, run);

            Sell(world, GardenSeason.For(world), key, run);
            Expect(world.FruitSoldOn(key) <= Market.MaxFruitSoldPerDay && world.JamSoldOn(key) <= Market.MaxJamSoldPerDay &&
                   world.TradesOn(key) <= Market.MaxTradesPerDay, "day " + day + ": the caps hold");
        }
        run.EndSeeds = world.Seeds;
        return run;
    }

    // Fruit comes from play only: whatever a tree solve adds to the basket is counted as play.
    private static void Play(WorldModel world, int item, int key, BotRun run)
    {
        int before = world.Fruit;
        world.SolveTree(item, key);
        run.PlayFruit += world.Fruit - before;
    }

    // The greedy sales: the better buyer first (Orin or Fen when they beat the stall), then the stall up to
    // its cap, then whatever trades remain. Every step must not lower the garden's value (the potential).
    private static void Sell(WorldModel world, Season s, int key, BotRun run)
    {
        NeighbourTrade orin = Market.TradeAt(OrinTrade), fen = Market.TradeAt(FenTrade);
        if (orin.GetAmount / orin.GiveAmount > Market.FruitPrice(s))
            while (Step(world, s, run, () => world.Trade(OrinTrade, key))) { }

        int fruit = Math.Min(world.Fruit, Market.MaxFruitSoldPerDay - world.FruitSoldOn(key));
        if (fruit > 0)
            Step(world, s, run, () =>
            {
                int earned = world.SellFruit(fruit, s, key);
                Expect(earned == fruit * Market.FruitPrice(s), "fruit sells at the season's price in " + s);
                return earned > 0;
            });
        while (Step(world, s, run, () => world.Trade(OrinTrade, key))) { }

        if (fen.GetAmount / fen.GiveAmount > Market.JamPrice(s))
            while (Step(world, s, run, () => world.Trade(FenTrade, key))) { }
        int jam = Math.Min(world.Jam, Market.MaxJamSoldPerDay - world.JamSoldOn(key));
        if (jam > 0)
            Step(world, s, run, () =>
            {
                int earned = world.SellJam(jam, s, key);
                Expect(earned == jam * Market.JamPrice(s), "jam sells at the season's price in " + s);
                return earned > 0;
            });
        while (Step(world, s, run, () => world.Trade(FenTrade, key))) { }
    }

    // Runs one market step. It must not lower the potential, must not create fruit or jam, and its seeds are counted.
    private static bool Step(WorldModel world, Season s, BotRun run, Func<bool> act)
    {
        int seeds0 = world.Seeds, fruit0 = world.Fruit, jam0 = world.Jam;
        long before = Potential(world, s);
        if (!act()) return false;
        Expect(Potential(world, s) <= before, "a market step in " + s + " never raises the garden's value");
        Expect(world.Fruit <= fruit0 && world.Jam <= jam0, "a market step never creates fruit or jam");
        run.MarketSeeds += world.Seeds - seeds0;
        return true;
    }
}
