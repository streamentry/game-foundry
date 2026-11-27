using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Evergrove.Core;

/// <summary>
/// Checklist 46 (run 3): things grow outside the garden. Flowers the player planted spread
/// along the river, one cell every few days played, deterministic from (garden, planted kinds,
/// days played), never on anything standing, never shrinking.
/// </summary>
internal static class WildGrowthTests
{
    private static int assertions;

    private static void Expect(bool ok, string message)
    {
        assertions++;
        if (!ok) throw new Exception("WildGrowthTests failed: " + message);
    }

    public static int Run()
    {
        var clock = Stopwatch.StartNew();
        GardenMap map = GardenMap.River(10, 15, Mask(), 3);

        NothingBeforeAFlower(map);
        OnlyPlantedFlowerKindsSpread(map);
        GrowthIsGradual(map);
        DeterministicOverDaysAndGardens(map);
        MonotonicOverDays(map);
        NeverOnForbiddenCells(map);
        RespectsTheCap(map);

        Expect(clock.ElapsedMilliseconds < 1000, "runs in under a second (" + clock.ElapsedMilliseconds + " ms)");
        return assertions;
    }

    private static void NothingBeforeAFlower(GardenMap map)
    {
        WorldModel empty = Garden(0, 0, 1);
        for (int days = 0; days <= 1000; days += 7)
            Expect(WildGrowth.Spread(map, empty, days).Length == 0, "an empty garden has no wild flowers");

        WorldModel trees = Garden(2, 0, 1);
        Expect(trees.Plant(0, FreeCell(trees, map)), "a shade tree planted by hand");
        for (int days = 0; days <= 1000; days += 7)
            Expect(WildGrowth.Spread(map, trees, days).Length == 0, "nothing spreads before a flower is planted");
    }

    private static void OnlyPlantedFlowerKindsSpread(GardenMap map)
    {
        WorldModel w = Garden(3, 200, 1);
        Expect(w.Plant(0, FreeCell(w, map)) && w.Buy(0, FreeCell(w, map)), "a tree planted and a tree bought");
        Expect(WildGrowth.Spread(map, w, 1000).Length == 0, "non-flower plants never spread");

        Expect(w.Buy(2, FreeCell(w, map)), "a flower bought from the nursery");
        Expect(WildGrowth.Spread(map, w, 1000).Length > 0, "a bought flower spreads");

        WorldModel basket = Garden(3, 0, 1);
        Expect(basket.Plant(2, FreeCell(basket, map)), "a flower planted from the basket");
        Expect(WildGrowth.Spread(map, basket, 1000).Length > 0, "a planted flower spreads");
    }

    private static void GrowthIsGradual(GardenMap map)
    {
        WorldModel w = Garden(3, 0, 1);
        Expect(w.Plant(2, FreeCell(w, map)), "a flower to grow from");
        Expect(WildGrowth.Spread(map, w, 0).Length == 0 && WildGrowth.Spread(map, w, 1).Length == 0, "none on the first day");
        Expect(WildGrowth.Spread(map, w, 2).Length == 1, "one cell after two days");
        Expect(WildGrowth.Spread(map, w, 4).Length == 2, "one more every two days");
        // One flower seeds at most PerFlower cells, however many days have passed: no burst for a late first flower.
        Expect(WildGrowth.Spread(map, w, 10).Length == WildGrowth.PerFlower &&
               WildGrowth.Spread(map, w, 100).Length == WildGrowth.PerFlower, "one flower seeds at most three wild cells");
        Expect(w.Plant(2, FreeCell(w, map)) && WildGrowth.Spread(map, w, 100).Length == 2 * WildGrowth.PerFlower,
            "a second flower lets it spread a little further");
    }

    private static void DeterministicOverDaysAndGardens(GardenMap map)
    {
        var firsts = new List<string>();
        for (int garden = 1; garden <= 8; garden++)
        {
            WorldModel a = Garden(3, 200, garden);
            Expect(a.Plant(2, FreeCell(a, map)) && a.Buy(2, FreeCell(a, map)), "a flower and a bought flower");
            WorldSnapshot snap = a.Export();
            WorldModel b;
            Expect(WorldModel.TryRestore(snap, out b), "the same garden restores");
            for (int days = 0; days <= 300; days += 6)
            {
                int[] first = WildGrowth.Spread(map, a, days);
                int[] second = WildGrowth.Spread(map, b, days);
                Expect(first.SequenceEqual(second), "same inputs, same cells (garden " + garden + ", day " + days + ")");
            }
            firsts.Add(string.Join(",", WildGrowth.Spread(map, a, 300)));
        }
        Expect(firsts.Distinct().Count() > 1, "different gardens grow differently");
    }

    private static void MonotonicOverDays(GardenMap map)
    {
        for (int garden = 1; garden <= 3; garden++)
        {
            WorldModel w = Garden(3, 0, garden);
            Expect(w.Plant(2, FreeCell(w, map)), "a flower to grow from");
            int[] prev = WildGrowth.Spread(map, w, 0);
            for (int days = 1; days <= 300; days++)
            {
                int[] cur = WildGrowth.Spread(map, w, days);
                Expect(cur.Length >= prev.Length, "never shrinks (day " + days + ")");
                Expect(cur.Take(prev.Length).SequenceEqual(prev), "the earlier wild cells stay (day " + days + ")");
                prev = cur;
            }
        }
    }

    private static void NeverOnForbiddenCells(GardenMap map)
    {
        for (int garden = 1; garden <= 4; garden++)
        {
            WorldModel w = Garden(3, 200, garden);
            Expect(w.Plant(2, FreeCell(w, map)) && w.Buy(2, FreeCell(w, map)), "flowers planted");
            Expect(w.PlaceDecor(Decoration.Bench, FreeCell(w, map), map), "a bench to stand in the way");
            int[] layout = map.Layout(w.Choices, w.Placements, w.Plantings);
            int[] standing = w.Occupied;
            for (int days = 0; days <= 600; days += 50)
            {
                int[] wild = WildGrowth.Spread(map, w, days);
                Expect(wild.Distinct().Count() == wild.Length, "one flower per cell");
                foreach (int cell in wild)
                {
                    Expect(map.At(cell) == GardenMap.Free, "never on the river, the cottage or a path");
                    Expect(Array.IndexOf(standing, cell) < 0, "never on a planting, a bought plant or decoration");
                    Expect(Array.IndexOf(layout, cell) < 0, "never on a garden item");
                }
            }
        }
    }

    private static void RespectsTheCap(GardenMap map)
    {
        WorldModel w = Garden(3, 200, 1);
        // Enough flowers that the per-flower limit isn't what stops it: the overall cap is.
        for (int i = 0; i < WildGrowth.Most / WildGrowth.PerFlower; i++)
            Expect(w.Plant(2, FreeCell(w, map)), "flower " + i + " to grow from");
        Expect(WildGrowth.Grown(1000000) == WildGrowth.Most, "grown stops at the cap");
        int count = WildGrowth.Spread(map, w, 1000000).Length;
        Expect(count <= WildGrowth.Most, "never more than the cap");
        Expect(count == WildGrowth.Most, "the cap is reached on a wide river (" + count + ")");
    }

    /// <summary>A living garden with chapters solved and the given seeds, in the given garden.</summary>
    private static WorldModel Garden(int chapters, int seeds, int garden)
    {
        WorldModel world = WorldModel.New(PlayMode.Living);
        for (int chapter = 0; chapter < chapters; chapter++)
        {
            Puzzle board = world.CurrentPuzzle;
            for (int i = 0; i < board.CellCount; i++)
                while (world.Rotations[i] != board.Solution[i]) world.Rotate(i);
            world.Choose((VillageChoice)(chapter % 3));
        }
        WorldSnapshot s = world.Export();
        s.seeds = seeds;
        s.garden = garden;
        Expect(WorldModel.TryRestore(s, out world), "a garden restores");
        return world;
    }

    private static int FreeCell(WorldModel w, GardenMap map)
    {
        int[] occupied = w.Occupied;
        int[] layout = map.Layout(w.Choices, w.Placements, occupied);
        for (int cell = 0; cell < map.CellCount; cell++)
            if (map.IsFree(layout, occupied, cell)) return cell;
        throw new Exception("no free cell");
    }

    private static string Mask()
    {
        string dir = AppContext.BaseDirectory;
        while (dir != null && !System.IO.File.Exists(System.IO.Path.Combine(dir, "Assets/Resources/Evergrove/garden-map-mask.txt")))
            dir = System.IO.Path.GetDirectoryName(dir.TrimEnd(System.IO.Path.DirectorySeparatorChar));
        if (dir == null) throw new Exception("garden-map-mask.txt not found");
        return System.IO.File.ReadAllText(System.IO.Path.Combine(dir, "Assets/Resources/Evergrove/garden-map-mask.txt")).Trim();
    }
}
