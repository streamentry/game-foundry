using System;
using Evergrove.Core;

/// <summary>I388 to I391: the garden's year. Each season's yield, once per season turn.</summary>
internal static class SeasonalYieldsTests
{
    private static int assertions;

    private static void Expect(bool ok, string message)
    {
        assertions++;
        if (!ok) throw new Exception("Seasonal yields test failed: " + message);
    }

    private static WorldModel Finished()
    {
        WorldModel w = WorldModel.New(PlayMode.Living);
        for (int chapter = 0; chapter < PuzzleCatalog.Count; chapter++)
        {
            Puzzle board = w.CurrentPuzzle;
            for (int i = 0; i < board.CellCount; i++)
                while (w.Rotations[i] != board.Solution[i]) w.Rotate(i);
            w.Choose((VillageChoice)(chapter % 3));
        }
        return w;
    }

    // Tend things until the garden's season is the one wanted.
    private static void TurnTo(WorldModel w, Season season)
    {
        for (int guard = 0; guard < 400 && GardenSeason.For(w) != season; guard++) w.SolveTree(guard % w.Choices.Length);
        Expect(GardenSeason.For(w) == season, "reached " + season);
    }

    public static int Run()
    {
        assertions = 0;
        WorldModel early = WorldModel.New(PlayMode.Living);
        Expect(SeasonalYields.Key(early) == (int)GardenSeason.For(early), "in the story, the story's season");
        Expect(SeasonalYields.Key(null) == -1 && SeasonalYields.Key(WorldModel.New(PlayMode.PuzzleOnly)) == -1, "no living garden, no season");
        Expect(early.TakeSeasonYield(-1) == SeasonYield.None || early.YieldTaken == SeasonalYields.Key(early) + 1, "taken at most once");

        // Summer: a grove found and standing lets fall its seeds, once.
        WorldModel w = Finished();
        TurnTo(w, Season.Summer);
        int grove = PairingMask.Of(Pairing.Grove);
        Expect(w.TakeSeasonYield(grove) == SeasonYield.None, "a grove standing but never found: nothing yet");
        w.DiscoverPairing(Pairing.Grove);
        Expect(w.TakeSeasonYield(0) == SeasonYield.None, "found once but not standing now: nothing");
        int seeds = w.Seeds;
        Expect(w.TakeSeasonYield(grove) == SeasonYield.Seeds && w.Seeds == seeds + SeasonalYields.SummerSeeds, "summer seeds");
        Expect(w.TakeSeasonYield(grove) == SeasonYield.None, "once a season");
        Expect(GardenTransfer.TryImport(GardenTransfer.Export(w, "2026-11-27T00:00:00Z"), out WorldModel back, out _) &&
               back.YieldTaken == w.YieldTaken && back.TakeSeasonYield(grove) == SeasonYield.None, "taken travels in the garden file");
        Expect(!GardenTransfer.Export(WorldModel.New(PlayMode.Living), "2026-11-27T00:00:00Z").Contains("\"yields\""),
            "not written while nothing is taken");

        // Autumn: an orchard (a deeper pairing) is harvested.
        TurnTo(w, Season.Autumn);
        foreach (Pairing p in Enum.GetValues(typeof(Pairing))) w.DiscoverPairing(p);
        foreach (Pairing p in Enum.GetValues(typeof(Pairing))) w.DiscoverPairing(p);
        int fruit = w.Fruit;
        Expect(w.TakeSeasonYield(PairingMask.Of(Pairing.Grove)) == SeasonYield.None, "a grove isn't an orchard");
        Expect(w.TakeSeasonYield(PairingMask.Of(Pairing.Orchard)) == SeasonYield.Fruit &&
               w.Fruit == Math.Min(WorldModel.MaxFruit, fruit + SeasonalYields.AutumnFruit), "the orchard's harvest");

        // Winter: the neighbours' turn. Jam, if you gave them fruit through the year.
        TurnTo(w, Season.Winter);
        WorldModel stingy = WorldModel.TryRestore(w.Export(), out WorldModel copy) ? copy : null;
        while (w.Fruit == 0) w.SolveTree(0);
        Expect(stingy != null, "a copy");
        int jam = w.Jam;
        if (w.Fruit > 0 && w.FruitGivenTo(0) == 0 && w.FruitGivenTo(1) == 0 && w.FruitGivenTo(2) == 0)
            Expect(w.TakeSeasonYield(0) == SeasonYield.None, "no fruit given: no jam back");
        Expect(w.GiveFruit(1), "a fruit for Orin");
        Expect(w.TakeSeasonYield(0) == SeasonYield.Jam && w.Jam == jam + 1, "a jar of their jam in winter");

        // Spring: a flower bed sows itself into the basket.
        TurnTo(w, Season.Spring);
        int basket = w.Gifts.Length;
        Expect(w.TakeSeasonYield(PairingMask.Of(Pairing.FlowerBed)) == SeasonYield.Seedling && w.Gifts.Length == basket + 1 &&
               Nursery.Family(w.Gifts[w.Gifts.Length - 1]) == VillageChoice.Flowers, "a self-sown flower in the basket");
        foreach (Season s in Enum.GetValues(typeof(Season))) Expect(SeasonalYields.Why(s).Length > 0, "the almanac says why");
        return assertions;
    }
}
