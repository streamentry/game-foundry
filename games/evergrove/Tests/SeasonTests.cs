using System;
using Evergrove.Core;

/// <summary>
/// I248: four seasons, by progress, never the clock. The story walks spring to winter;
/// after it, every few puzzles tended turns the year on, forever.
/// </summary>
internal static class SeasonTests
{
    private static int assertions;

    private static void Expect(bool ok, string message)
    {
        assertions++;
        if (!ok) throw new Exception("Season test failed: " + message);
    }

    public static int Run()
    {
        // The story: three streams per season.
        Expect(GardenSeason.ForStory(0) == Season.Spring, "the first streams are spring");
        Expect(GardenSeason.ForStory(1) == Season.Summer, "then summer");
        Expect(GardenSeason.ForStory(2) == Season.Autumn, "then autumn");
        Expect(GardenSeason.ForStory(3) == Season.Winter, "then winter");
        Expect(GardenSeason.ForStory(9) == Season.Winter && GardenSeason.ForStory(-1) == Season.Spring, "clamped");

        // After the story: the festival is summer; each SolvesPerReach first solves turns the year.
        int step = GardenMap.SolvesPerReach;
        Expect(GardenSeason.AfterStory(0) == Season.Summer, "the festival summer");
        Expect(GardenSeason.AfterStory(step - 1) == Season.Summer, "not yet");
        Expect(GardenSeason.AfterStory(step) == Season.Autumn, "the first new stretch brings autumn");
        Expect(GardenSeason.AfterStory(step * 2) == Season.Winter, "then winter");
        Expect(GardenSeason.AfterStory(step * 3) == Season.Spring, "then spring");
        Expect(GardenSeason.AfterStory(step * 4) == Season.Summer, "and round again");
        Expect(GardenSeason.AfterStory(-5) == Season.Summer, "a bad count reads as none");
        // It never stops turning, even past the river's ceiling (the year isn't capped).
        int past = GardenMap.ReachesCeiling * step * 3;
        Expect(GardenSeason.AfterStory(past + step) != GardenSeason.AfterStory(past), "still turning past the ceiling");
        for (int s = 0; s < 400; s++)
            Expect(GardenSeason.AfterStory(s) == (Season)(((int)Season.Summer + s / step) % 4), "steady cycle " + s);

        // From a world: story progress, then tending.
        WorldModel world = WorldModel.New(PlayMode.Living);
        Expect(GardenSeason.For(world) == Season.Spring, "a new garden begins in spring");
        Grow(world, 4);
        Expect(GardenSeason.For(world) == Season.Summer, "four streams in, summer");
        Grow(world, 8);
        Expect(world.Completed && GardenSeason.For(world) == Season.Summer, "finished: the festival summer");
        for (int i = 0; i < step; i++) world.SolveTree(i);
        Expect(GardenSeason.For(world) == Season.Autumn, "tending turns the year");
        world.SolveTree(0); // a replay
        Expect(GardenSeason.For(world) == Season.Autumn, "replays don't turn it");
        // I250: when tending opens new bank, Mira names the season it brought, truly.
        string opened = NeighbourNotice.Line(0, world);
        Expect(opened != null && opened.ToLowerInvariant().Contains("autumn"),
            "Mira names the autumn the new stretch brought: " + opened);
        for (int i = step; i < step * 3; i++) world.SolveTree(i);
        string spring = NeighbourNotice.Line(0, world);
        Expect(GardenSeason.For(world) == Season.Spring && spring != null && spring.ToLowerInvariant().Contains("spring"),
            "and later the spring: " + spring);
        Expect(GardenSeason.For(null) == Season.Summer, "no garden, summer");
        Expect(GardenSeason.For(WorldModel.New(PlayMode.PuzzleOnly)) == Season.Summer, "puzzle-only mode stays summer");
        return assertions;
    }

    private static void Grow(WorldModel world, int count)
    {
        for (int chapter = 0; chapter < count; chapter++)
        {
            Puzzle board = world.CurrentPuzzle;
            for (int i = 0; i < board.CellCount; i++)
                while (world.Rotations[i] != board.Solution[i]) world.Rotate(i);
            world.Choose((VillageChoice)(world.Choices.Length % 3));
        }
    }
}
