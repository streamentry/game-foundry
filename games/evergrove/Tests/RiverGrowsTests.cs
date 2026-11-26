using System;
using Evergrove.Core;

/// <summary>
/// I220: after the story, the river keeps opening downstream as you tend the
/// things you've grown (their puzzles). Derived from the save, so the cap can be
/// raised later without touching any old garden.
/// </summary>
internal static class RiverGrowsTests
{
    private static int assertions;

    private static void Expect(bool ok, string message)
    {
        assertions++;
        if (!ok) throw new Exception("River grows test failed: " + message);
    }

    public static int Run()
    {
        // The story's own rule is unchanged.
        for (int restored = 0; restored <= 12; restored++)
            Expect(GardenMap.ReachesFor(restored, false, 99) == GardenMap.ReachesFor(restored, false),
                "before the story ends, tending doesn't open the river (" + restored + ")");
        Expect(GardenMap.ReachesFor(12, true) == GardenMap.MaxReaches, "the finished story opens five stretches");
        Expect(GardenMap.ReachesFor(12, true, 0) == GardenMap.MaxReaches, "and nothing more without tending");
        Expect(GardenMap.ReachesFor(12, true, GardenMap.SolvesPerReach - 1) == GardenMap.MaxReaches, "not yet");
        Expect(GardenMap.ReachesFor(12, true, GardenMap.SolvesPerReach) == GardenMap.MaxReaches + 1,
            "every few puzzles solved opens one more stretch");
        Expect(GardenMap.ReachesFor(12, true, 1000) == GardenMap.ReachesCeiling, "up to today's ceiling");
        Expect(GardenMap.ReachesCeiling * 10 * 15 <= WorldModel.MaxMapCell,
            "every stretch up to the ceiling fits the cell range saves already accept");

        // A grown river keeps every default place of the story where it was.
        string mask = Mask();
        GardenMap five = GardenMap.River(10, 15, mask, GardenMap.MaxReaches);
        GardenMap eight = GardenMap.River(10, 15, mask, 8);
        int[] choices = { 0, 1, 2, 0, 1, 2, 0, 1, 2, 0, 1, 2 };
        int[] a = five.Layout(choices, new int[0]), b = eight.Layout(choices, new int[0]);
        for (int i = 0; i < choices.Length; i++)
            Expect(a[i] == b[i], "story thing " + i + " stays put as the river grows");
        Expect(eight.CellCount == 8 * 150 && eight.IsLand(7 * 150 + 10), "the new stretch has land to plant");

        // The world counts what it has tended.
        WorldModel world = WorldModel.New(PlayMode.Living);
        Expect(world.PuzzlesSolved == 0, "nothing tended yet");
        for (int chapter = 0; chapter < 3; chapter++)
        {
            Puzzle board = world.CurrentPuzzle;
            for (int i = 0; i < board.CellCount; i++)
                while (world.Rotations[i] != board.Solution[i]) world.Rotate(i);
            world.Choose((VillageChoice)(chapter % 3));
        }
        world.SolveTree(0);
        world.SolveTree(0);
        world.SolveTree(2);
        Expect(world.PuzzlesSolved == 2, "first solves count once each, replays don't");
        return assertions;
    }

    private static string Mask()
    {
        // The shipped mask, found by walking up from the test binary to the game folder.
        string dir = AppContext.BaseDirectory;
        while (dir != null && !System.IO.File.Exists(System.IO.Path.Combine(dir, "Assets/Resources/Evergrove/garden-map-mask.txt")))
            dir = System.IO.Path.GetDirectoryName(dir.TrimEnd(System.IO.Path.DirectorySeparatorChar));
        if (dir == null) throw new Exception("garden-map-mask.txt not found");
        return System.IO.File.ReadAllText(System.IO.Path.Combine(dir, "Assets/Resources/Evergrove/garden-map-mask.txt")).Trim();
    }
}
