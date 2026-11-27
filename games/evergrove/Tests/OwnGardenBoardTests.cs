using System;
using System.Collections.Generic;
using Evergrove.Core;

/// <summary>Own-garden boards (checklist item 37): nonogram beds drawn from what the garden grew, unique and solvable.</summary>
internal static class OwnGardenBoardTests
{
    private static int assertions;

    private static void Expect(bool ok, string message)
    {
        assertions++;
        if (!ok) throw new Exception("OwnGardenBoardTests failed: " + message);
    }

    private static bool Same(int[] a, int[] b)
    {
        if (a.Length != b.Length) return false;
        for (int i = 0; i < a.Length; i++) if (a[i] != b[i]) return false;
        return true;
    }

    // Runs of row or column idx of a 4x4 grid given as a bit mask (bit r*4+c).
    private static int[] Runs(int idx, bool row, int mask)
    {
        var list = new List<int>();
        int run = 0;
        for (int k = 0; k <= 4; k++)
        {
            bool on = k < 4 && ((mask >> (row ? idx * 4 + k : k * 4 + idx)) & 1) != 0;
            if (on) run++;
            else if (run > 0) { list.Add(run); run = 0; }
        }
        return list.ToArray();
    }

    // A garden grown through the public API: each chapter's pipes solved by rotation, then a choice.
    private static WorldModel Grown(int garden, int chapters)
    {
        WorldModel world = WorldModel.New(PlayMode.Living, garden);
        for (int chapter = 0; chapter < chapters; chapter++)
        {
            Puzzle board = world.CurrentPuzzle;
            for (int i = 0; i < board.CellCount; i++)
                while (world.Rotations[i] != board.Solution[i]) world.Rotate(i);
            Expect(world.Choose((VillageChoice)((chapter * 7 + garden) % 3)), "a solved chapter takes a choice");
        }
        return world;
    }

    // Same garden, plus a bought plant (needs seeds and a free cell on an open nursery row).
    private static WorldModel WithBought(WorldModel world)
    {
        WorldSnapshot snapshot = world.Export();
        snapshot.seeds = 1000;
        WorldModel rich;
        Expect(WorldModel.TryRestore(snapshot, out rich), "a rich garden restores");
        for (int slot = 0; slot < Nursery.Slots; slot++)
            for (int cell = 0; cell < 40; cell++)
                if (rich.BoughtCount < 2 && rich.Buy(rich.NurseryStock(slot), cell)) break;
        return rich;
    }

    private static string Key(NonogramGame g)
    {
        var parts = new List<string>();
        foreach (var c in g.RowClues) parts.Add(string.Join(".", c));
        parts.Add("|");
        foreach (var c in g.ColumnClues) parts.Add(string.Join(".", c));
        return string.Join(",", parts);
    }

    public static int Run()
    {
        assertions = 0;

        // The mapping: a grown tree, flower and building each draw their named picture.
        Expect(OwnGardenBoards.PictureFor(new GardenItem(VillageChoice.ShadeTree, 0, "x", "x", "x", "apple tree")) == "an apple",
            "an apple tree draws the apple picture");
        Expect(OwnGardenBoards.PictureFor(new GardenItem(VillageChoice.Flowers, 0, "x", "x", "x", "tulips")) == "a tulip",
            "tulips map to the tulip picture");
        Expect(OwnGardenBoards.PictureFor(new GardenItem(VillageChoice.Bridge, 0, "x", "x", "x", "stone bridge")) == null,
            "an unmapped thing contributes nothing");

        // Every library name that the mapping can produce exists in some tier.
        var general = new HashSet<string>(OwnGardenBoards.GeneralPictures());
        foreach (string name in OwnGardenBoards.PicturesFor(WorldModel.New(PlayMode.Living, 0)))
            Expect(general.Contains(name), "the empty garden falls back to the general library");

        // The counter itself: known non-unique clues, and a brute-force cross-check on 4x4 grids.
        bool[] found;
        Expect(NonogramUniqueness.Count(new[] { new[] { 1 }, new[] { 1 } }, new[] { new[] { 1 }, new[] { 1 } }, 2, out found) == 2,
            "a 2x2 with two diagonals is not unique");
        Expect(NonogramUniqueness.Count(new[] { new int[0], new int[0] }, new[] { new int[0], new int[0] }, 2, out found) == 1,
            "an empty 2x2 is unique");
        int disagreements = 0, nonUnique = 0;
        uint lcg = 12345;
        for (int sample = 0; sample < 200; sample++)
        {
            lcg = lcg * 1664525u + 1013904223u;
            int mask = (int)((lcg >> 8) & 0xFFFF);
            int[][] rc = new int[4][], cc = new int[4][];
            for (int i = 0; i < 4; i++) { rc[i] = Runs(i, true, mask); cc[i] = Runs(i, false, mask); }
            int brute = 0;
            for (int g = 0; g < 65536 && brute < 3; g++)
            {
                bool ok = true;
                for (int i = 0; i < 4 && ok; i++)
                    ok = Same(Runs(i, true, g), rc[i]) && Same(Runs(i, false, g), cc[i]);
                if (ok) brute++;
            }
            int counted = NonogramUniqueness.Count(rc, cc, 2, out found);
            if (counted != Math.Min(2, brute)) disagreements++;
            if (brute > 1) nonUnique++;
        }
        Expect(disagreements == 0, "the counter agrees with brute force on 4x4 clues");
        Expect(nonUnique > 0, "the cross-check meets non-unique clues");

        // Library uniqueness, reported (real finding about the existing picture library).
        int total;
        int unique = OwnGardenBoards.CountUniqueLibrary(out total);

        var worlds = new List<WorldModel>();
        foreach (int garden in new[] { 1, 2, 3, 5, 8, 13, 21 })
            foreach (int chapters in new[] { 4, 12 })
                worlds.Add(Grown(garden, chapters));
        worlds.Add(WithBought(Grown(4, 6)));
        worlds.Add(WithBought(Grown(9, 12)));
        worlds.Add(WorldModel.New(PlayMode.Living, 0));

        int boards = 0;
        var memo = new Dictionary<string, bool>();
        foreach (WorldModel world in worlds)
        {
            string[] mapped = OwnGardenBoards.MappedPictures(world);
            string[] names = OwnGardenBoards.PicturesFor(world);
            Expect(names.Length > 0, "every garden has pictures to draw");
            for (int tier = 0; tier < TreePuzzles.Tiers; tier++)
            {
                bool own = OwnGardenBoards.HasGardenPicture(world, tier);
                for (int seed = -3; seed < 30; seed++)
                {
                    NonogramGame g = OwnGardenBoards.NonogramFor(world, seed, tier);
                    NonogramGame again = OwnGardenBoards.NonogramFor(world, seed, tier);
                    boards++;

                    Expect(g.Size == NonogramGame.SizeFor(tier), "the bed has its tier's size");
                    Expect(g.PictureName == again.PictureName && g.Mirrored == again.Mirrored && Key(g) == Key(again),
                        "same garden and seed give the same bed");
                    if (own) Expect(Array.IndexOf(mapped, g.PictureName) >= 0,
                        "a garden bed draws one of the garden's own pictures");

                    // Unique by the counter (memoised by clues), and the one solution is the picture.
                    string clueKey = tier + ":" + Key(g);
                    bool isUnique;
                    if (!memo.TryGetValue(clueKey, out isUnique))
                    {
                        bool[] first;
                        isUnique = NonogramUniqueness.Count(g.RowClues, g.ColumnClues, 2, out first) == 1;
                        memo[clueKey] = isUnique;
                    }
                    Expect(isUnique, "every garden bed has exactly one solution");
                    string[] rows = NonogramPictures.Rows(tier, g.PictureIndex);
                    for (int r = 0; r < g.Size; r++)
                        for (int c = 0; c < g.Size; c++)
                        {
                            char want = rows[r][g.Mirrored ? g.Size - 1 - c : c];
                            Expect(g.Solution(r * g.Size + c) == (want == '#'), "the solution is the picture");
                        }

                    // Solvable by line logic alone: following hints from a blank bed completes it.
                    for (int step = 0; step < g.Size * g.Size * 2 && !g.Solved; step++)
                    {
                        int h = g.HintCell();
                        Expect(h >= 0, "a deducible cell while unsolved");
                        g.Set(h, g.Solution(h) ? CellState.Filled : CellState.Marked);
                    }
                    Expect(g.Solved, "the garden bed is solvable by line logic");
                }
            }
        }

        Console.WriteLine("Own-garden boards: library pictures unique " + unique + " of " + total +
                          "; boards checked " + boards);
        return assertions;
    }
}
