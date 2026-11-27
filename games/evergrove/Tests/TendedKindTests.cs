using System;
using Evergrove.Core;

/// <summary>
/// I257: neighbours speak truly of the puzzles you solved, by kind (since I243 a tree may
/// hold a nonogram, so "freed the water under your trees" could be false).
/// </summary>
internal static class TendedKindTests
{
    private static int assertions;

    private static void Expect(bool ok, string message)
    {
        assertions++;
        if (!ok) throw new Exception("Tended-by-kind test failed: " + message);
    }

    public static int Run()
    {
        // Owners: Mira water/stream/numbers, Orin mosaic/plots/tiles, Fen pairs/sudoku.
        Expect(NeighbourNotice.OwnerOf(PuzzleKind.Water) == 0 && NeighbourNotice.OwnerOf(PuzzleKind.StreamPath) == 0 &&
               NeighbourNotice.OwnerOf(PuzzleKind.Nonogram) == 0, "Mira keeps the water kinds");
        Expect(NeighbourNotice.OwnerOf(PuzzleKind.Mosaic) == 1 && NeighbourNotice.OwnerOf(PuzzleKind.Plots) == 1 &&
               NeighbourNotice.OwnerOf(PuzzleKind.Mahjong) == 1, "Orin keeps the building kinds");
        Expect(NeighbourNotice.OwnerOf(PuzzleKind.Pairs) == 2 && NeighbourNotice.OwnerOf(PuzzleKind.Sudoku) == 2,
            "Fen keeps the flower kinds");

        WorldModel world = WorldModel.New(PlayMode.Living);
        Grow(world, 12);
        int[] made = world.Choices;
        // Solve a tree from the later chapters: whatever kind it holds, the right neighbour says so.
        for (int i = ThingPuzzle.ClassicChapters; i < made.Length; i++)
        {
            WorldModel w = Clone(world);
            w.SolveTree(i);
            PuzzleKind kind = ThingPuzzle.ForStory(i, (VillageChoice)made[i]);
            int owner = NeighbourNotice.OwnerOf(kind);
            Expect(NeighbourNotice.SolvedOf(w, kind) == 1, "one solve of " + kind);
            string line = NeighbourNotice.Line(owner, w);
            Expect(line != null && line.Contains(NeighbourNotice.KindWord(kind)),
                kind + " is named by its keeper: " + line);
            for (int v = 0; v < 3; v++)
                if (v != owner)
                {
                    string other = NeighbourNotice.Line(v, w);
                    Expect(other == null || !other.Contains(NeighbourNotice.KindWord(kind)), "only the keeper speaks of " + kind);
                }
            // Never the old family wording for a kind it isn't.
            if (kind != PuzzleKind.Water)
                Expect(!line.ToLowerInvariant().Contains("freed the water"), "no false water line for " + kind);
        }
        // Counts add up per kind across story things.
        WorldModel all = Clone(world);
        for (int i = 0; i < made.Length; i++) all.SolveTree(i);
        int total = 0;
        foreach (PuzzleKind k in Enum.GetValues(typeof(PuzzleKind))) total += NeighbourNotice.SolvedOf(all, k);
        Expect(total == made.Length, "every solved thing counted once, by its kind");
        // I266: after a puzzle solved taking turns, its keeper says so, kindly and truly.
        string[] banned = { "miss", "lonely", "come back", "streak", "hurry", "waiting for you", "love you", "grandma", "child" };
        foreach (PuzzleKind kind in Enum.GetValues(typeof(PuzzleKind)))
        {
            string line = NeighbourNotice.TogetherLine(kind);
            Expect(!string.IsNullOrEmpty(line) && line.Contains(NeighbourNotice.KindWord(kind)),
                "the together line names the kind: " + line);
            Expect(line.ToLowerInvariant().Contains("two of you") || line.ToLowerInvariant().Contains("together"),
                "it says you played together: " + line);
            foreach (string word in banned)
                Expect(!line.ToLowerInvariant().Contains(word), "no pressure, no guessing who played (" + word + ")");
        }
        return assertions;
    }

    private static WorldModel Clone(WorldModel w)
    {
        Expect(WorldModel.TryRestore(w.Export(), out WorldModel copy), "clone");
        return copy;
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
