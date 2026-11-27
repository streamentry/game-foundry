using System;
using Evergrove.Core;

/// <summary>
/// Neighbours who notice (depth spine, Layer 4): one line, only from true facts
/// in the save, never guilt, never fake intimacy.
/// </summary>
internal static class NoticeTests
{
    private static int assertions;

    private static void Expect(bool ok, string message)
    {
        assertions++;
        if (!ok) throw new Exception("Notice test failed: " + message);
    }

    private static readonly string[] Banned = {
        "miss", "lonely", "come back", "streak", "don't forget", "hurry", "only today", "waiting for you", "love you"
    };

    private static void Kind(string line, string label)
    {
        string lower = line.ToLowerInvariant();
        foreach (string word in Banned) Expect(!lower.Contains(word), label + ": no pressure or fake intimacy (" + word + ")");
    }

    public static int Run()
    {
        WorldModel world = WorldModel.New(PlayMode.Living);
        for (int v = 0; v < 3; v++)
            Expect(NeighbourNotice.Line(v, world) == null, "nothing done yet, nothing to notice");

        Grow(world, 9); // choices cycle tree, build, flowers: three of each
        for (int v = 0; v < 3; v++)
            Expect(NeighbourNotice.Line(v, world) == null, "grown things alone are the old lines' job");

        // Solve the puzzles under two trees (items 0 and 3) and one flower bed (item 2).
        world.SolveTree(0);
        world.SolveTree(3);
        world.SolveTree(2);
        string mira = NeighbourNotice.Line(0, world);
        Expect(mira != null && mira.Contains("2"), "Mira counts the two trees whose puzzles you solved: " + mira);
        string fen = NeighbourNotice.Line(2, world);
        Expect(fen != null && fen.Contains("1") || fen != null && fen.ToLowerInvariant().Contains("one"),
            "Fen notices the one flower bed: " + fen);
        Expect(NeighbourNotice.Line(1, world) == null, "Orin has nothing true to say yet");
        Kind(mira, "Mira");
        Kind(fen, "Fen");

        // A plant from the nursery: its own neighbour notices it by name.
        WorldSnapshot rich = world.Export();
        rich.seeds = 500;
        Expect(WorldModel.TryRestore(rich, out WorldModel buyer), "restore with seeds");
        Expect(buyer.Buy(1, 40), "buy the garden gate");
        string orin = NeighbourNotice.Line(1, buyer);
        Expect(orin != null && orin.Contains(Nursery.Item(1).Name), "Orin names the gate from the nursery: " + orin);
        Kind(orin, "Orin");

        // The same save always says the same thing (no hidden randomness, no clock).
        Expect(NeighbourNotice.Line(1, buyer) == orin, "deterministic");

        // Every possible line over many saves stays kind and true to its counts.
        for (int solved = 0; solved <= 4; solved++)
        {
            WorldModel w = WorldModel.New(PlayMode.Living);
            Grow(w, 12);
            for (int i = 0; i < 12 && solved > 0; i++)
                if (i % 3 == 0 && i / 3 < solved) w.SolveTree(i);
            string line = NeighbourNotice.Line(0, w);
            if (solved == 0) Expect(line == null, "no solves, no line");
            else
            {
                Expect(line != null, "a line for " + solved);
                Kind(line, "count " + solved);
                // I225: three of four first solves after the story: Mira speaks for the river instead.
                if (line.Contains("mist")) { Expect(solved % GardenMap.SolvesPerReach == 3, "river line only at three of four"); continue; }
                if (line.Contains("river")) { Expect(solved % GardenMap.SolvesPerReach == 0, "opened line only on a full row"); continue; }
                Expect(line.Contains(solved.ToString()) || (solved == 1 && line.ToLowerInvariant().Contains("one")),
                    "the count is the true count " + solved + ": " + line);
                Kind(line, "count " + solved);
            }
        }
        // I222: a cutting from family waiting in the basket: its neighbour mentions it, by name.
        WorldSnapshot gifted = world.Export();
        gifted.gifts = new[] { 2 }; // a clover cutting (flowers: Fen)
        Expect(WorldModel.TryRestore(gifted, out WorldModel withGift), "restore with a gift waiting");
        string fenGift = NeighbourNotice.Line(2, withGift);
        Expect(fenGift != null && fenGift.Contains(Nursery.Item(2).Name) && fenGift.ToLowerInvariant().Contains("basket"),
            "Fen mentions the clover waiting in the basket: " + fenGift);
        Kind(fenGift, "gift");
        Expect(NeighbourNotice.Line(0, withGift) == NeighbourNotice.Line(0, world), "Mira isn't told about Fen's flowers");
        Expect(withGift.PlantGift(0, 41) && NeighbourNotice.Line(2, withGift) != fenGift,
            "once planted, the gift line gives way");

        // I225: Mira, who keeps the trees by the water, tells you what the drops on the
        // mist mean, only when it's true: one more first solve opens more river.
        WorldModel done = WorldModel.New(PlayMode.Living);
        Grow(done, 12);
        Expect(done.Completed, "the story is finished");
        done.SolveTree(0);
        done.SolveTree(1);
        string two = NeighbourNotice.Line(0, done);
        Expect(two != null && !two.ToLowerInvariant().Contains("mist"), "two of four: no river line yet: " + two);
        done.SolveTree(2);
        string near = NeighbourNotice.Line(0, done);
        Expect(near != null && near.ToLowerInvariant().Contains("mist") && near.ToLowerInvariant().Contains("one more"),
            "three of four: Mira says one more will lift the mist: " + near);
        Kind(near, "river near");
        Expect(NeighbourNotice.Line(1, done) == null || !NeighbourNotice.Line(1, done).ToLowerInvariant().Contains("mist"),
            "only Mira speaks for the river");
        done.SolveTree(3);
        Expect(GardenMap.ReachesFor(12, true, done.PuzzlesSolved) == GardenMap.MaxReaches + 1, "a stretch opened");
        string opened = NeighbourNotice.Line(0, done);
        Expect(opened != null && opened.ToLowerInvariant().Contains("river") && !opened.ToLowerInvariant().Contains("one more"),
            "after it opens Mira says the river runs further: " + opened);
        Kind(opened, "river opened");
        done.SolveTree(4);
        string after = NeighbourNotice.Line(0, done);
        Expect(after != null && !after.ToLowerInvariant().Contains("mist") && !after.ToLowerInvariant().Contains("further"),
            "then the tended count speaks again: " + after);
        // During the story the drops count streams: no tending line about the mist.
        WorldModel midway = WorldModel.New(PlayMode.Living);
        Grow(midway, 5);
        for (int i = 0; i < 5; i++) midway.SolveTree(i);
        string mid = NeighbourNotice.Line(0, midway);
        Expect(mid == null || !mid.ToLowerInvariant().Contains("mist"), "no river line before the story ends: " + mid);

        Expect(NeighbourNotice.Line(5, world) == null && NeighbourNotice.Line(-1, world) == null, "only our three neighbours");
        Expect(NeighbourNotice.Line(0, WorldModel.New(PlayMode.PuzzleOnly)) == null, "no neighbours in puzzle-only mode");
        return assertions;
    }

    private static void Grow(WorldModel world, int count)
    {
        for (int chapter = 0; chapter < count; chapter++)
        {
            Puzzle board = world.CurrentPuzzle;
            for (int i = 0; i < board.CellCount; i++)
                while (world.Rotations[i] != board.Solution[i]) world.Rotate(i);
            world.Choose((VillageChoice)(chapter % 3));
        }
    }
}
