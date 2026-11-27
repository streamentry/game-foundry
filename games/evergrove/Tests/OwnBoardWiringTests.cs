using System;
using System.Collections.Generic;
using System.IO;
using Evergrove.Core;

/// <summary>
/// Own-garden board wiring (checklist item 37): the own nonogram is the garden's bed, a relative
/// plays the same clues through the standard generator, the leaves chooser measures the own bed,
/// the standard generator is unchanged, and the mosaic of your own thing uses its own picture.
/// </summary>
internal static class OwnBoardWiringTests
{
    private static int assertions;

    // FNV-1a over the standard generator's clues for seeds 0..119 in every tier. Recorded from the
    // standard generator before the item 37 change; NonogramGame.cs is not edited by this change.
    private const ulong StandardFingerprint = 15945868890821070808UL;

    private static void Expect(bool ok, string message)
    {
        assertions++;
        if (!ok) throw new Exception("OwnBoardWiringTests failed: " + message);
    }

    private static string Key(NonogramGame g)
    {
        var parts = new List<string>();
        foreach (var c in g.RowClues) parts.Add(string.Join(".", c));
        parts.Add("|");
        foreach (var c in g.ColumnClues) parts.Add(string.Join(".", c));
        return string.Join(",", parts);
    }

    private static string SolutionKey(NonogramGame g)
    {
        var chars = new char[g.Size * g.Size];
        for (int i = 0; i < chars.Length; i++) chars[i] = g.Solution(i) ? '#' : '.';
        return new string(chars);
    }

    private static ulong Fingerprint()
    {
        ulong hash = 14695981039346656037UL;
        for (int tier = 0; tier < TreePuzzles.Tiers; tier++)
            for (int seed = 0; seed < 120; seed++)
            {
                var g = new NonogramGame(seed, tier);
                foreach (char ch in Key(g) + "#" + SolutionKey(g) + ";")
                {
                    hash ^= ch;
                    hash *= 1099511628211UL;
                }
            }
        return hash;
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

    // Same garden, plus a bought plant.
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

    // Reads a source file by walking up from the working directory and the test binary.
    private static string ReadSource(string relative)
    {
        foreach (string start in new[] { Directory.GetCurrentDirectory(), AppContext.BaseDirectory })
        {
            DirectoryInfo dir = new DirectoryInfo(start);
            for (int up = 0; dir != null && up < 12; up++, dir = dir.Parent)
            {
                foreach (string candidate in new[] { Path.Combine(dir.FullName, "games", "evergrove", relative), Path.Combine(dir.FullName, relative) })
                    if (File.Exists(candidate)) return File.ReadAllText(candidate);
            }
        }
        throw new Exception("OwnBoardWiringTests: cannot find " + relative);
    }

    public static int Run()
    {
        assertions = 0;

        var worlds = new List<WorldModel>();
        foreach (int garden in new[] { 1, 2, 3, 5, 8, 13, 21 })
            foreach (int chapters in new[] { 4, 12 })
                worlds.Add(Grown(garden, chapters));
        worlds.Add(WithBought(Grown(4, 6)));
        worlds.Add(WithBought(Grown(9, 12)));
        worlds.Add(WorldModel.New(PlayMode.Living, 0));

        // 1. Own nonograms: unique, line-solvable, and drawn from the garden's pictures when it has one.
        int owns = 0, twins = 0, undoOk = 0, dealt = 0, dealtAtLevel = 0;
        for (int w = 0; w < worlds.Count; w++)
        {
            WorldModel world = worlds[w];
            string[] mapped = OwnGardenBoards.MappedPictures(world);
            for (int tier = 0; tier < TreePuzzles.Tiers; tier++)
            {
                bool garden = OwnGardenBoards.HasGardenPicture(world, tier);
                for (int seed = -3; seed < 12; seed++)
                {
                    NonogramGame own = OwnGardenBoards.NonogramFor(world, seed, tier);
                    owns++;
                    bool[] first;
                    Expect(NonogramUniqueness.Count(own.RowClues, own.ColumnClues, 2, out first) == 1,
                        "an own nonogram has exactly one solution");
                    if (garden) Expect(Array.IndexOf(mapped, own.PictureName) >= 0,
                        "an own nonogram draws one of the garden's pictures");

                    // Solvable by line logic alone from a blank bed.
                    NonogramGame play = OwnGardenBoards.NonogramFor(world, seed, tier);
                    for (int step = 0; step < play.Size * play.Size * 2 && !play.Solved; step++)
                    {
                        int h = play.HintCell();
                        Expect(h >= 0, "a deducible cell while an own nonogram is unsolved");
                        play.Set(h, play.Solution(h) ? CellState.Filled : CellState.Marked);
                    }
                    Expect(play.Solved, "an own nonogram is solvable by line logic");

                    // Undo rebuilds the same bed: NonogramBoard's history factory uses new NonogramGame(Seed, Tier).
                    NonogramGame rebuilt = new NonogramGame(own.Seed, own.Tier);
                    if (Key(rebuilt) == Key(own) && SolutionKey(rebuilt) == SolutionKey(own)) undoOk++;
                    Expect(Key(rebuilt) == Key(own) && SolutionKey(rebuilt) == SolutionKey(own),
                        "undo rebuilds the same own nonogram from its seed");

                    // 2. Sharing: a standard seed with identical clues and solution, or none (no send offered).
                    int shared = OwnGardenBoards.SharedSeedFor(own, tier);
                    if (shared >= 0)
                    {
                        twins++;
                        NonogramGame standard = new NonogramGame(shared, tier);
                        Expect(Key(standard) == Key(own) && SolutionKey(standard) == SolutionKey(own),
                            "the shared seed plays identical clues and solution");
                        Expect(standard.PictureName == own.PictureName, "the shared seed names the same picture");
                    }
                    else
                    {
                        for (int s = 0; s < 2 * NonogramPictures.Count(tier); s++)
                            Expect(Key(new NonogramGame(s, tier)) != Key(own), "no standard seed matches a reported-none own bed");
                    }
                }
            }
        }
        Expect(twins == owns, "every own nonogram has a standard twin (so every own nonogram can be sent)");

        // 3. Leaves chooser: the own bed at the chosen level, or the seed's own bed kept honestly.
        for (int w = 0; w < worlds.Count; w++)
            for (int level = 1; level <= DifficultyChoice.Levels; level++)
                for (int seed = 0; seed < 6; seed++)
                {
                    var deal = OwnGardenBoards.DealAt(worlds[w], seed, level);
                    dealt++;
                    Expect(deal.tier == DifficultyChoice.TierFor(PuzzleKind.Nonogram, level), "the chosen level deals its tier");
                    NonogramGame board = OwnGardenBoards.NonogramFor(worlds[w], deal.seed, deal.tier);
                    if (board.Leaves == level) dealtAtLevel++;
                    else Expect(deal.seed == seed, "a level no own bed reaches keeps the given seed");
                }

        // 4. The standard generator is unchanged for non-own boards.
        Expect(Fingerprint() == StandardFingerprint, "the standard generator's boards are unchanged");

        // 5. Mosaic of your own thing: a mosaic is a shuffle toward one fixed arrangement.
        //    Solving it swaps pieces home; Solved is reported only when every piece is in its own slot.
        int mosaics = 0;
        for (int tier = 0; tier < TreePuzzles.Tiers; tier++)
            for (int seed = -3; seed < 30; seed++)
            {
                var mosaic = new MosaicGame(seed, tier);
                mosaics++;
                var seen = new HashSet<int>();
                for (int i = 0; i < mosaic.Count; i++) seen.Add(mosaic.At(i));
                Expect(seen.Count == mosaic.Count, "a mosaic's pieces are a permutation");
                Expect(!mosaic.Solved, "a dealt mosaic is not already solved");

                int guard = 0;
                while (!mosaic.Solved)
                {
                    Expect(++guard <= mosaic.Count * 2, "a mosaic solves within its piece count");
                    int misplaced = -1;
                    for (int i = 0; i < mosaic.Count; i++) if (!mosaic.InPlace(i)) { misplaced = i; break; }
                    Expect(misplaced >= 0, "an unsolved mosaic has a misplaced piece");
                    int piece = mosaic.At(misplaced);
                    Expect(mosaic.Select(misplaced) == MosaicGame.Step.Picked, "a misplaced piece can be picked");
                    MosaicGame.Step swap = mosaic.Select(piece);
                    Expect(swap == MosaicGame.Step.Swapped, "a piece swaps into its own slot");
                    bool allHome = true;
                    for (int i = 0; i < mosaic.Count; i++) if (mosaic.At(i) != i) { allHome = false; break; }
                    Expect(mosaic.Solved == allHome, "a mosaic is solved exactly when every piece is home");
                }
                for (int i = 0; i < mosaic.Count; i++) Expect(mosaic.At(i) == i, "the solved mosaic is the one arrangement");
            }

        // 6. Source wiring in the Unity app (the app itself does not compile in the core tests).
        string app = ReadSource("Assets/Scripts/Unity/EvergroveApp.cs");
        Expect(app.Contains("ownDealing && world != null ? OwnGardenBoards.NonogramFor(world, seed, tier)"),
            "PuzzleCard draws the own nonogram only when the own flag is set");
        Expect(app.Contains(": new NonogramGame(seed, tier);"), "PuzzleCard keeps the standard nonogram for other boards");
        Expect(app.Contains("ownDealing && art != null ? art"), "an own mosaic uses the thing's own picture");
        Expect(app.Contains("ownDealing = ownBoard;") && app.Contains("ownDealing = false;"),
            "the own flag is set around PuzzleCard and cleared after");
        Expect(app.Contains("OwnNonogramDeal(seed, tier, solved, reopenAt)"), "own nonograms take the own leaves deal");
        Expect(app.Contains("int shared = ShareSeedFor(kind, seed, tier);"), "SendBoardToFamily sends the shared seed");
        Expect(app.Contains("world.SendBoard(from, \"\", (int)kind, tier, shared, nonce)"), "the parcel carries the shared seed");
        Expect(app.Contains("OwnGardenBoards.SharedSeedFor(OwnGardenBoards.NonogramFor(world, seed, tier), tier)"),
            "the send option is offered only when a shared seed exists");

        Console.WriteLine("Own-board wiring: own nonograms " + owns + ", with a standard twin " + twins +
                          ", undo rebuilds " + undoOk + "; chosen levels at their own leaves " + dealtAtLevel + " of " + dealt +
                          "; mosaics solved " + mosaics + "; standard fingerprint " + Fingerprint());
        return assertions;
    }
}
