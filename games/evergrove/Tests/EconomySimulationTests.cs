using System;
using System.Linq;
using Evergrove.Core;

/// <summary>
/// v1 #71, #73 and #80 as a simulation: 365 days of one living garden, played by three profiles through the
/// public WorldModel API only. Seeds stay in range, spending is a real sink, grinding earns little more than a
/// normal day, and the balance grows by no more than the daily cap. Deterministic: the days walk forward from
/// 1 January 2027 with DateTime.AddDays, and nothing reads the clock or a random source.
/// </summary>
internal static class EconomySimulationTests
{
    private static int assertions;
    private const int Days = 365;
    private const int GrindPerDay = 200;
    private static readonly DateTime FirstDay = new DateTime(2027, 1, 1);

    private static void Expect(bool ok, string message)
    {
        assertions++;
        if (!ok) throw new Exception("Economy simulation failed: " + message);
    }

    /// <summary>The yyyymmdd number of the Nth simulated day (index 0 is 20270101).</summary>
    private static int DayKey(int index)
    {
        DateTime date = FirstDay.AddDays(index);
        return date.Year * 10000 + date.Month * 100 + date.Day;
    }

    /// <summary>One profile's year: its garden, end-of-day balances and what its replays paid.</summary>
    private sealed class Profile
    {
        public readonly string Name;
        public readonly WorldModel World;
        public readonly int[] EndBalance = new int[Days];
        public readonly int[] ReplayIncome = new int[Days];
        /// <summary>Days elapsed up to and including the day of the last first solve.</summary>
        public int FirstSolveDays;
        public long ReplayTotal;
        public int Purchases;
        /// <summary>Grinder only: the most seeds any day earned beyond its first FullReplaysPerDay replays.</summary>
        public int MaxExtraPerDay;

        public Profile(string name, WorldModel world)
        {
            Name = name;
            World = world;
        }
    }

    // Twelve grown things, none solved, no seeds, no purchases: the same starting garden for every profile.
    private static WorldModel NewGarden()
    {
        WorldModel world = WorldModel.New(PlayMode.Living);
        for (int chapter = 0; chapter < PuzzleCatalog.Count; chapter++)
        {
            Puzzle board = world.CurrentPuzzle;
            for (int i = 0; i < board.CellCount; i++)
                while (world.Rotations[i] != board.Solution[i]) world.Rotate(i);
            Expect(world.Choose((VillageChoice)(chapter % 3)), "chapter " + chapter + " is chosen");
        }
        Expect(world.Choices.Length == PuzzleCatalog.Count && world.Seeds == 0 && world.Ledger.Length == 0,
            "the starting garden has twelve grown things and no seeds");
        return world;
    }

    // Solves one grown thing on a day. A first solve pays in full and sits outside the cap; a replay must pay
    // exactly what ReplayRewardOn promised before the call. Returns the seeds the solve paid.
    private static int Solve(Profile p, int item, int day, int today)
    {
        WorldModel world = p.World;
        int tier = TreePuzzles.TierFor(item);
        bool first = !world.TreeSolved(item);
        int predicted = world.ReplayRewardOn(tier, day);
        int paid = world.SolveTree(item, day);
        if (first)
        {
            Expect(paid == WorldModel.TreeReward(tier, true), p.Name + ": first solve of thing " + item + " pays in full");
            p.FirstSolveDays = today + 1;
        }
        else
        {
            Expect(paid == predicted, p.Name + ": replay of thing " + item + " on day " + today + " pays what ReplayRewardOn said");
            p.ReplayIncome[today] += paid;
            p.ReplayTotal += paid;
        }
        return paid;
    }

    // The lowest-numbered grown thing that is not yet solved, or -1.
    private static int FirstUnsolved(WorldModel world)
    {
        for (int item = 0; item < world.Choices.Length; item++)
            if (!world.TreeSolved(item)) return item;
        return -1;
    }

    // Every grown thing of the highest tier the garden has (the last chapters are the hardest).
    private static int[] TopTier(WorldModel world)
    {
        int top = TreePuzzles.TierFor(world.Choices.Length - 1);
        return Enumerable.Range(0, world.Choices.Length).Where(i => TreePuzzles.TierFor(i) == top).ToArray();
    }

    // Buys the cheapest plant on offer that the open rows and the purse allow. Returns false when none is affordable.
    private static bool BuyCheapest(Profile p, int day)
    {
        WorldModel world = p.World;
        int best = -1;
        for (int slot = 0; slot < Nursery.Slots; slot++)
        {
            int kind = world.NurseryStock(slot);
            if (!world.RowOpen(Nursery.Row(kind)) || world.Seeds < Nursery.Price(kind)) continue;
            if (best < 0 || Nursery.Price(kind) < Nursery.Price(best)) best = kind;
        }
        if (best < 0) return false;

        int before = world.Seeds;
        Expect(world.Buy(best, world.BoughtCount + 1), p.Name + ": buys kind " + best + " on day " + day);
        Expect(world.Seeds == before - Nursery.Price(best), p.Name + ": a plant costs its price");
        Expect(world.Ledger.Length > 0 && world.Ledger[0].Reason == SeedReason.Nursery &&
               world.Ledger[0].Delta == -Nursery.Price(best), p.Name + ": a purchase writes one Nursery ledger line");
        p.Purchases++;
        return true;
    }

    // Gentle: one first solve a day while unsolved things remain, one replay of the first thing, and
    // whatever plants the purse can buy.
    private static Profile Gentle()
    {
        var p = new Profile("gentle", NewGarden());
        for (int day = 0; day < Days; day++)
        {
            int key = DayKey(day);
            int next = FirstUnsolved(p.World);
            if (next >= 0) Solve(p, next, key, day);
            Solve(p, 0, key, day);
            while (BuyCheapest(p, key)) { }
            p.EndBalance[day] = p.World.Seeds;
        }
        return p;
    }

    // Daily: the first day clears the highest-tier things once, then three replays a day spread across them.
    private static Profile Daily()
    {
        var p = new Profile("daily", NewGarden());
        int[] top = TopTier(p.World);
        int spread = 0;
        for (int day = 0; day < Days; day++)
        {
            int key = DayKey(day);
            foreach (int item in top)
                if (!p.World.TreeSolved(item)) Solve(p, item, key, day);
            for (int k = 0; k < WorldModel.FullReplaysPerDay; k++)
                Solve(p, top[spread++ % top.Length], key, day);
            p.EndBalance[day] = p.World.Seeds;
        }
        return p;
    }

    // Grinder: the highest-tier thing, replayed 200 times a day, every day.
    private static Profile Grinder()
    {
        var p = new Profile("grinder", NewGarden());
        int[] top = TopTier(p.World);
        int target = top[top.Length - 1];
        int normalDay = WorldModel.FullReplaysPerDay * WorldModel.TreeReward(TreePuzzles.TierFor(target), false);
        for (int day = 0; day < Days; day++)
        {
            int key = DayKey(day);
            if (!p.World.TreeSolved(target)) Solve(p, target, key, day);
            int extra = 0;
            for (int k = 0; k < GrindPerDay; k++)
            {
                int paid = Solve(p, target, key, day);
                if (k >= WorldModel.FullReplaysPerDay) extra += paid;
            }
            Expect(extra < normalDay, "grinder day " + day + ": replays past the first " + WorldModel.FullReplaysPerDay +
                   " earn " + extra + ", less than a normal day's " + normalDay);
            p.MaxExtraPerDay = Math.Max(p.MaxExtraPerDay, extra);
            p.EndBalance[day] = p.World.Seeds;
        }
        return p;
    }

    private static void GiftsAreCheaperThanBuying()
    {
        for (int kind = 0; kind < Nursery.Kinds; kind++)
        {
            int price = Nursery.Price(kind);
            int gift = Nursery.GiftPrice(kind);
            Expect(gift > 0 && gift < price && gift == price / 2,
                "kind " + kind + ": a cutting sent to family (" + gift + ") costs less than buying it (" + price + ")");
        }
    }

    private static void SeedsStayInRange(Profile p)
    {
        for (int day = 0; day < Days; day++)
            Expect(p.EndBalance[day] >= 0 && p.EndBalance[day] <= WorldModel.MaxSeeds,
                p.Name + " day " + day + ": seeds stay between 0 and MaxSeeds (" + p.EndBalance[day] + ")");
    }

    // After the last first solve, the balance can rise by no more than the daily cap a day, and over the
    // rest of the year by no more than (days left) * cap. Purchases only lower it.
    private static void BalanceIsBoundedByTheCap(Profile p, int dailyCapMax)
    {
        int n = p.FirstSolveDays;
        Expect(n >= 1 && n < Days, p.Name + ": the first solves finish inside the year (" + n + " days)");
        long growth = (long)p.EndBalance[Days - 1] - p.EndBalance[n - 1];
        Expect(growth <= (long)(Days - n) * dailyCapMax,
            p.Name + ": balance grew " + growth + " after day " + n + ", more than " + (Days - n) + " x " + dailyCapMax);
        for (int day = n; day < Days; day++)
            Expect(p.EndBalance[day] - p.EndBalance[day - 1] <= dailyCapMax,
                p.Name + " day " + day + ": no day after the first solves grows past the cap of " + dailyCapMax);
    }

    public static int Run()
    {
        GiftsAreCheaperThanBuying();

        int maxReplay = 0;
        for (int tier = 0; tier < TreePuzzles.Tiers; tier++)
            maxReplay = Math.Max(maxReplay, WorldModel.TreeReward(tier, false));
        int dailyCapMax = WorldModel.FullReplaysPerDay * maxReplay + WorldModel.SmallReplaysPerDay * WorldModel.SmallReplayReward;

        Profile gentle = Gentle();
        Profile daily = Daily();
        Profile grinder = Grinder();

        foreach (Profile p in new[] { gentle, daily, grinder })
        {
            SeedsStayInRange(p);
            BalanceIsBoundedByTheCap(p, dailyCapMax);
        }

        Expect(gentle.Purchases > 0 && gentle.World.BoughtCount == gentle.Purchases,
            "the gentle player spends seeds on plants (" + gentle.Purchases + " bought)");

        long allowance = (long)Days * WorldModel.SmallReplaysPerDay * WorldModel.SmallReplayReward;
        Expect(grinder.ReplayTotal - daily.ReplayTotal <= allowance,
            "grinding over a year earns at most " + allowance + " more than a daily player (got " +
            (grinder.ReplayTotal - daily.ReplayTotal) + ")");

        Console.WriteLine("Economy 365 days: gentle/daily/grinder end balances = " + gentle.EndBalance[Days - 1] + "/" +
                          daily.EndBalance[Days - 1] + "/" + grinder.EndBalance[Days - 1] +
                          "; grinder extra per day <= " + grinder.MaxExtraPerDay);
        return assertions;
    }
}
