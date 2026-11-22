using System;
using Evergrove.Core;

internal static class Program
{
    private static int assertions;

    private static void Expect(bool condition, string description)
    {
        assertions++;
        if (!condition) throw new Exception("FAILED: " + description);
    }

    private static void Solve(WorldModel model)
    {
        Puzzle board = PuzzleCatalog.Get(model.Round);
        for (int index = 0; index < board.CellCount; index++)
        {
            int attempts = 0;
            while (model.Rotations[index] != board.Solution[index])
            {
                Expect(model.Rotate(index), "tile rotates");
                attempts++;
                Expect(attempts <= PipeRules.OrientationCount(board.Kinds[index]),
                    "rotation bounded");
            }
        }
        Expect(model.Solved, "reference solution accepted");
        Expect(model.Insight.LeakCount == 0 &&
               model.Insight.ConnectedCount == board.CellCount, "all water reaches the outlet");
    }

    private static void ContentAndRules()
    {
        Expect(PuzzleCatalog.Count == 12, "12 authored water puzzles");
        for (int index = 0; index < PuzzleCatalog.Count; index++)
        {
            Puzzle board = PuzzleCatalog.Get(index);
            int expectedSize = index < 6 ? 3 : 4;
            Expect(board.Size == expectedSize, "difficulty graduates to 4x4");
            Expect(board.Kinds.Length == board.Size * board.Size, "board has exact size");
            Expect(PipeRules.ValidRotations(board, board.Initial), "board starts valid");
            Expect(PipeRules.ValidRotations(board, board.Solution), "authored answer valid");
            Expect(!PipeRules.IsComplete(board, board.Initial), "board starts unsolved");
            Expect(PipeRules.IsComplete(board, board.Solution), "authored solution wins");
            Expect(PipeRules.CountSolutions(board, 2) == 1, "one mathematical solution");
            Expect(PipeRules.Inspect(board, board.Solution).JoinedEdges == board.CellCount - 1,
                "complete path has N-1 joined edges");
            Expect(PipeRules.Inspect(board, board.Solution).LeakCount == 0,
                "complete path never leaks");
            int[] wrong = (int[])board.Solution.Clone();
            wrong[0] = wrong[0] % PipeRules.OrientationCount(board.Kinds[0]) + 1;
            Expect(!PipeRules.IsComplete(board, wrong), "wrong orientation fails");
            wrong[0] = 100;
            Expect(!PipeRules.ValidRotations(board, wrong), "out of range fails");
        }
    }

    private static void RestoreAndMigration()
    {
        WorldModel initial = WorldModel.New();
        Expect(initial.CanSwitchMode, "mode can be chosen before first action");
        Expect(!initial.Choose(VillageChoice.ShadeTree), "choices gated until water flows");
        Expect(!initial.Rotate(-1) && !initial.Rotate(9), "out of range tile rejected");
        Expect(initial.Rotate(0), "rotation succeeds");
        Expect(!initial.CanSwitchMode, "mode locks during play");
        Expect(initial.Undo(), "last rotation can be undone");
        Expect(initial.CanSwitchMode, "mode unlocks after undo");
        Expect(initial.GuideOneConnection(), "optional hint aligns exactly one tile");
        Expect(initial.HintsUsed == 1, "hint count recorded");
        Expect(initial.Undo(), "guided tile can also be undone");
        Expect(initial.HintsUsed == 1, "undo does not secretly refund a used hint");
        Expect(initial.Restart(), "restart works");

        WorldSnapshot corrupt = initial.Export();
        corrupt.choices = new[] { 0, 1 };
        WorldModel parsed;
        Expect(!WorldModel.TryRestore(corrupt, out parsed), "forged actions fail closed");
        corrupt = initial.Export();
        corrupt.rotations[0] = 999;
        Expect(!WorldModel.TryRestore(corrupt, out parsed), "invalid rotations fail closed");
        corrupt = initial.Export();
        corrupt.solved = true;
        Expect(!WorldModel.TryRestore(corrupt, out parsed), "forged win fails closed");
        corrupt = initial.Export();
        corrupt.version = 999;
        Expect(!WorldModel.TryRestore(corrupt, out parsed), "unknown version fails closed");
        corrupt = initial.Export();
        corrupt.hintsUsed = -2;
        Expect(!WorldModel.TryRestore(corrupt, out parsed), "invalid hint count fails closed");

        // Original Evergrove v1 ran for exactly six chapters.
        var legacyDone = new WorldSnapshot {
            version = 1, mode = "living", round = 6,
            solved = false, rotations = Array.Empty<int>(), previous = Array.Empty<int>(),
            choices = new[] { 0, 1, 2, 0, 1, 2 }
        };
        Expect(WorldModel.TryRestore(legacyDone, out parsed), "legacy completed garden migrates");
        Expect(parsed.Round == 6 && !parsed.Completed, "old ending unlocks chapter seven");
        Expect(parsed.Rotations.Length == 16, "migration creates first 4x4 board");
        Expect(parsed.Choices.Length == 6, "original history retained");
        Expect(parsed.Export().version == WorldModel.Version, "migrated snapshot uses v2");

        var legacyPartial = WorldModel.New().Export();
        legacyPartial.version = 1;
        Expect(WorldModel.TryRestore(legacyPartial, out parsed), "v1 incomplete progress upgrades");
        Expect(parsed.Round == 0 && parsed.Rotations.Length == 9, "incomplete board survives upgrade");
    }

    private static void StoryAndCompleteRun()
    {
        WorldModel world = WorldModel.New();
        Solve(world);
        Expect(!world.Rotate(0), "no extra turns after win");
        Expect(world.CanUndo && world.Undo() && !world.Solved,
            "solved board can be undone without losing progress");
        Solve(world);
        Expect(world.Choose(VillageChoice.ShadeTree), "tree choice commits");
        Expect(world.Round == 1 && world.Count(VillageChoice.ShadeTree) == 1, "one tree remembered");
        Expect(world.Memory.Contains("Mira"), "Mira recalls tree");
        Expect(!world.Choose(VillageChoice.Bridge), "cannot choose twice without solving");

        Expect(world.ReconsiderLatestChoice(), "last garden choice can be reconsidered");
        Expect(world.Round == 0 && world.Solved &&
            world.Choices.Length == 0, "undo choice reopens solved board without history");
        Expect(world.Choose(VillageChoice.ShadeTree), "corrected choice can recommit");
        Expect(world.Round == 1 && world.Choices.Length == 1,
            "corrected history contains only one action");

        Solve(world);
        Expect(world.Choose(VillageChoice.Bridge), "bridge choice commits");
        Expect(VillageStory.HasShadedCrossing(world), "two decisions create a new landmark");
        Expect(world.Consequence.Contains("crossing"), "player sees combined effect");
        Solve(world);
        Expect(world.Choose(VillageChoice.Flowers), "wildflower choice commits");
        Expect(VillageStory.HasButterflyGrove(world) &&
               VillageStory.HasCommunityWalk(world) &&
               VillageStory.HasLivingHeart(world), "three decisions create interlocking systems");
        Expect(world.Characters.Length == 3, "NPC trio available");
        Expect(world.Journal.Length == 3, "ordered history visible");
        Expect(VillageStory.MemoryEvent(world).Contains("CHAPTER 3"),
            "memory interlude references exact chapter");

        WorldSnapshot saved = world.Export();
        WorldModel loaded;
        Expect(WorldModel.TryRestore(saved, out loaded) && loaded.Round == 3,
            "saved progress round trips");
        Expect(loaded.Consequence == world.Consequence, "persistent consequence deterministic");
        for (int chapter = 3; chapter < PuzzleCatalog.Count; chapter++)
        {
            Solve(loaded);
            Expect(loaded.Choose((VillageChoice)(chapter % 3)),
                "chapter progression and consequence");
        }
        Expect(loaded.Completed, "12 streams completed");
        Expect(loaded.Choices.Length == 12 && loaded.Journal.Length == 12,
            "all memories recorded");
        Expect(loaded.Ending.Contains("VILLAGE WE GREW"),
            "balanced choices create the shared-world ending");
        Expect(!loaded.Rotate(0) && !loaded.Choose(VillageChoice.ShadeTree),
            "completed world cannot double collect choices");
        Expect(loaded.Export().rotations.Length == 0, "completed snapshot has no board");
        Expect(WorldModel.TryRestore(loaded.Export(), out world) && world.Completed,
            "end-game round trip consistent");

        var solo = WorldModel.New(PlayMode.PuzzleOnly);
        Solve(solo);
        Expect(!solo.Choose(VillageChoice.Flowers), "control cannot alter world");
        Expect(solo.NextPuzzle(), "control advances without a story action");
        Expect(solo.Round == 1 && solo.Choices.Length == 0,
            "control keeps identical puzzles and no memories");
    }

    public static int Main()
    {
        try
        {
            ContentAndRules();
            RestoreAndMigration();
            StoryAndCompleteRun();
            Console.WriteLine("Evergrove C# tests passed: " + assertions + " assertions");
            return 0;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine(ex);
            return 1;
        }
    }
}
