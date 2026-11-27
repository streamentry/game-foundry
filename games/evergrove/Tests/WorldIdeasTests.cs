using System;
using System.Collections.Generic;
using Evergrove.Core;

/// <summary>
/// Ideas from the world's games (docs/specs/proposals/world-games-50-ideas-2026-11-27.md),
/// the rules part of each, test-first.
/// </summary>
internal static class WorldIdeasTests
{
    private static int assertions;

    private static void Expect(bool ok, string message)
    {
        assertions++;
        if (!ok) throw new Exception("World ideas test failed: " + message);
    }

    public static int Run()
    {
        // Idea 3 (a flower card for each month): Pairs deals the flowers of the season.
        var seen = new HashSet<int>();
        foreach (Season season in new[] { Season.Spring, Season.Summer, Season.Autumn, Season.Winter })
        {
            int[] flowers = GardenCatalog.FlowersOf(season);
            Expect(flowers.Length >= 8, season + " has at least eight flowers for the biggest Pairs table");
            foreach (int f in flowers)
            {
                Expect(f >= 0 && f < GardenCatalog.CountOf(VillageChoice.Flowers), "a real flower " + f);
                Expect(seen.Add(f), "each flower belongs to one season (" + f + ")");
            }
        }
        Expect(seen.Count == GardenCatalog.CountOf(VillageChoice.Flowers), "every flower has its season");
        Expect(Array.IndexOf(GardenCatalog.FlowersOf(Season.Winter), Index("snowdrops")) >= 0, "snowdrops are winter");
        Expect(Array.IndexOf(GardenCatalog.FlowersOf(Season.Spring), Index("tulips")) >= 0, "tulips are spring");
        Expect(Array.IndexOf(GardenCatalog.FlowersOf(Season.Summer), Index("sunflowers")) >= 0, "sunflowers are summer");
        Expect(Array.IndexOf(GardenCatalog.FlowersOf(Season.Autumn), Index("asters")) >= 0, "asters are autumn");
        // Idea 2 (mahjong's season tiles): one face is the season set, exactly four tiles
        // drawn as four different seasons, and any two of them match.
        for (int tier = 0; tier < TreePuzzles.Tiers; tier++)
            for (int seed = 1; seed <= 40; seed++)
            {
                var game = new MahjongGame(seed * 7919, tier);
                int seasons = 0;
                for (int t = 0; t < game.TileCount; t++)
                    if (game.Face(t) == MahjongGame.SeasonFace) seasons++;
                Expect(seasons == 4, "four season tiles at tier " + tier + " seed " + seed);
                int[] set = MahjongGame.SeasonTiles(game);
                Expect(set.Length == 4, "the season set lists four tiles");
                for (int k = 1; k < set.Length; k++) Expect(set[k] > set[k - 1], "season tiles in a fixed order");
            }
        // Idea 7 (shogi's promotion): a thing is crowned once a replay was solved at the
        // deepest tier; never by its first solve, however deep.
        int deepest = TreePuzzles.Tiers - 1;
        for (int baseTier = 0; baseTier <= deepest; baseTier++)
        {
            Expect(!ReplayBoards.Crowned(baseTier, 0), "a first solve alone is never crowned (base " + baseTier + ")");
            bool was = false;
            for (int done = 1; done <= 12; done++)
            {
                bool now = ReplayBoards.Crowned(baseTier, done);
                Expect(now == (ReplayBoards.Tier(baseTier, done - 1) == deepest),
                    "crowned exactly when the last replay was at the deepest tier");
                Expect(!was || now, "a crown is never lost");
                was = now;
            }
            Expect(was, "every thing can be crowned by replaying");
        }
        Expect(!ReplayBoards.Crowned(0, ReplayBoards.ReplaysPerTier * 2), "base 0 needs a replay at the deepest tier");
        Expect(ReplayBoards.Crowned(0, ReplayBoards.ReplaysPerTier * 2 + 1), "base 0 is crowned on its seventh replay");
        Expect(ReplayBoards.Crowned(deepest, 1), "a deep thing is crowned on its first replay");
        return assertions;
    }

    private static int Index(string name)
    {
        for (int i = 0; i < GardenCatalog.CountOf(VillageChoice.Flowers); i++)
            if (GardenCatalog.Get(VillageChoice.Flowers, i).Name == name) return i;
        throw new Exception("no flower " + name);
    }
}
