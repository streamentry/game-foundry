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
                Expect(line.Contains(solved.ToString()) || (solved == 1 && line.ToLowerInvariant().Contains("one")),
                    "the count is the true count " + solved + ": " + line);
                Kind(line, "count " + solved);
            }
        }
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
