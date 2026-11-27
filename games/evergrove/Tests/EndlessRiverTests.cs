using System;
using Evergrove.Core;

internal static class EndlessRiverTests
{
    private static int assertions;

    private static void Expect(bool ok, string message)
    {
        assertions++;
        if (!ok) throw new Exception("EndlessRiverTests failed: " + message);
    }

    public static int Run()
    {
        Names();
        ReachesGrow();
        LayoutScales();
        SaveHasNoStretchCount();
        WellScalesWithReaches();
        return assertions;
    }

    // I363: every stretch has a name, deterministic, adjacent stretches differ.
    private static void Names()
    {
        for (int reach = 0; reach <= 500; reach++)
        {
            string name = RiverNames.For(reach);
            Expect(!string.IsNullOrEmpty(name), "named " + reach);
            Expect(name == RiverNames.For(reach), "deterministic " + reach);
            if (reach > 0) Expect(name != RiverNames.For(reach - 1), "adjacent stretches differ at " + reach);
        }
        Expect(RiverNames.For(0) == "Cottage Reach" && RiverNames.For(19) == "Far Reach", "the authored list still opens the river");
    }

    // Reaches keep opening with solves, past the old twenty, with no maximum.
    private static void ReachesGrow()
    {
        int last = 0;
        for (int solved = 0; solved <= 2000; solved++)
        {
            int reaches = GardenMap.ReachesFor(12, true, solved);
            Expect(reaches >= last, "monotone at " + solved);
            last = reaches;
        }
        Expect(GardenMap.ReachesFor(12, true, 400) == 105, "400 solves open 100 more stretches after the story");
        Expect(GardenMap.ReachesFor(12, true, 400) > 20, "well past the old ceiling of twenty");
        Expect(GardenMap.ReachesFor(12, true, int.MaxValue) == GardenMap.SafetyReachBound, "only the safety bound stops it");
        Expect(!GardenMap.TowardNext(12, true, int.MaxValue, out _, out _), "the mist shows no drops at the bound only");
        Expect(GardenMap.TowardNext(12, true, 400, out _, out _), "the mist shows drops past twenty");
    }

    // 200 reaches build fast, every reach has land, and the water distances match a brute-force scan.
    private static void LayoutScales()
    {
        string mask = Mask();
        var watch = System.Diagnostics.Stopwatch.StartNew();
        GardenMap map = GardenMap.River(10, 15, mask, 200);
        int[] choices = { 0, 1, 2, 0, 1, 2, 0, 1, 2, 0, 1, 2 };
        int[] layout = map.Layout(choices, null);
        watch.Stop();
        Expect(watch.ElapsedMilliseconds < 200, "200 reaches lay out in under 200 ms (" + watch.ElapsedMilliseconds + ")");
        Expect(map.Reaches == 200 && map.CellCount == 200 * 150, "200 reaches stack");
        for (int reach = 0; reach < 200; reach++)
        {
            bool land = false;
            for (int cell = reach * 150; cell < (reach + 1) * 150 && !land; cell++) land = map.IsLand(cell);
            Expect(land, "reach " + reach + " has land");
        }
        for (int i = 0; i < choices.Length; i++) Expect(map.IsLand(layout[i]), "item " + i + " on land");
        for (int cell = 0; cell < map.CellCount; cell += 37)
        {
            int brute = 99;
            int x = cell % map.Columns, y = cell / map.Columns;
            for (int other = 0; other < map.CellCount; other++)
                if (map.At(other) == GardenMap.Water)
                    brute = Math.Min(brute, Math.Abs(other % map.Columns - x) + Math.Abs(other / map.Columns - y));
            Expect(map.DistanceToWater(cell) == brute, "water distance matches the scan at " + cell);
        }
    }

    // The save holds no stretch count: stretches come from the solves it records, and it
    // round-trips through the save snapshot and the garden file with every solve kept.
    private static void SaveHasNoStretchCount()
    {
        WorldModel world = WorldModel.New(PlayMode.Living);
        for (int chapter = 0; chapter < PuzzleCatalog.Count; chapter++)
        {
            Puzzle board = world.CurrentPuzzle;
            for (int i = 0; i < board.CellCount; i++)
                while (world.Rotations[i] != board.Solution[i]) world.Rotate(i);
            world.Choose((VillageChoice)(chapter % 3));
        }
        Expect(world.Completed, "the story is finished");

        // The most solves the format can hold: every story item solved, and 200 bought plants solved.
        WorldSnapshot s = world.Export();
        s.treeSolved = new int[world.Choices.Length];
        for (int i = 0; i < s.treeSolved.Length; i++) s.treeSolved[i] = i;
        s.bought = new int[WorldModel.MaxBought * 2];
        s.boughtSolved = new int[WorldModel.MaxBought];
        for (int i = 0; i < WorldModel.MaxBought; i++)
        {
            s.bought[i * 2] = i % Nursery.Kinds;
            s.bought[i * 2 + 1] = 9000 + i * 40; // past the old 10,000-cell range
            s.boughtSolved[i] = i;
        }
        int solvedMax = s.treeSolved.Length + s.boughtSolved.Length;
        int expected = GardenMap.ReachesFor(12, true, solvedMax);
        Expect(expected == 5 + solvedMax / GardenMap.SolvesPerReach, "stretches follow solves: " + expected);

        Expect(WorldModel.TryRestore(s, out WorldModel restored), "the largest save restores");
        Expect(restored.PuzzlesSolved == solvedMax, "every solve is kept");
        WorldSnapshot again = restored.Export();
        Expect(again.boughtSolved.Length == WorldModel.MaxBought && again.bought.Length == s.bought.Length, "solves round-trip");
        Expect(WorldModel.TryRestore(again, out WorldModel second) && second.PuzzlesSolved == solvedMax, "a second restore agrees");

        string text = GardenTransfer.Export(restored, "2026-11-27T00:00:00Z");
        Expect(GardenTransfer.TryImport(text, out WorldModel imported, out GardenImportError error),
            "the garden file restores: " + error);
        Expect(imported.PuzzlesSolved == solvedMax && imported.Completed, "the garden file keeps every solve");

        // Nothing opens by payment: spending seeds changes no solve and no stretch.
        WorldSnapshot rich = world.Export();
        rich.seeds = WorldModel.MaxSeeds;
        Expect(WorldModel.TryRestore(rich, out WorldModel wealthy), "a rich save restores");
        Expect(GardenMap.ReachesFor(12, true, wealthy.PuzzlesSolved) == GardenMap.ReachesFor(12, true, 0),
            "seeds open no stretch");
    }

    // The well holds up to as many boards as stretches are open, and refuses more.
    private static void WellScalesWithReaches()
    {
        WorldModel world = WorldModel.New(PlayMode.Living);
        for (int chapter = 0; chapter < PuzzleCatalog.Count; chapter++)
        {
            Puzzle board = world.CurrentPuzzle;
            for (int i = 0; i < board.CellCount; i++)
                while (world.Rotations[i] != board.Solution[i]) world.Rotate(i);
            world.Choose((VillageChoice)(chapter % 3));
        }
        WorldSnapshot s = world.Export();
        s.treeSolved = new int[world.Choices.Length];
        for (int i = 0; i < s.treeSolved.Length; i++) s.treeSolved[i] = i;
        s.bought = new int[WorldModel.MaxBought * 2];
        s.boughtSolved = new int[WorldModel.MaxBought];
        for (int i = 0; i < WorldModel.MaxBought; i++)
        {
            s.bought[i * 2] = i % Nursery.Kinds;
            s.bought[i * 2 + 1] = 9000 + i * 40;
            s.boughtSolved[i] = i;
        }
        int open = GardenMap.ReachesFor(12, true, s.treeSolved.Length + s.boughtSolved.Length);

        s.well = open;
        Expect(WorldModel.TryRestore(s, out WorldModel all), "a well with every open stretch's board restores");
        Expect(all.WellSolved == open && OldWell.Waiting(all) == 0, "and no board waits past that");
        s.well = 40;
        Expect(WorldModel.TryRestore(s, out WorldModel fortyWell) && fortyWell.WellSolved == 40, "counts above twenty restore");
        s.well = open + 1;
        Expect(!WorldModel.TryRestore(s, out _), "one board more than the open stretches is refused");
        s.well = -1;
        Expect(!WorldModel.TryRestore(s, out _), "a negative count is refused");

        // Counts survive the garden file too.
        string text = GardenTransfer.Export(fortyWell, "2026-11-27T00:00:00Z");
        Expect(GardenTransfer.TryImport(text, out WorldModel imported, out GardenImportError error) && imported.WellSolved == 40,
            "a well above twenty survives the garden file: " + error);
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
