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
        for (int index = 0; index < 9; index++)
        {
            int attempts = 0;
            while (model.Rotations[index] != board.Solution[index])
            {
                Expect(model.Rotate(index), "tile rotates");
                attempts++;
                Expect(attempts <= PipeRules.OrientationCount(board.Kinds[index]), "rotation bounded");
            }
        }
        Expect(model.Solved, "reference solution accepted");
    }

    public static int Main()
    {
        try
        {
            Expect(PuzzleCatalog.Count == 6, "exactly six original boards");
            for (int index = 0; index < PuzzleCatalog.Count; index++)
            {
                Puzzle board = PuzzleCatalog.Get(index);
                Expect(PipeRules.ValidRotations(board, board.Initial), "board has valid starts");
                Expect(PipeRules.ValidRotations(board, board.Solution), "board has valid reference solution");
                Expect(!PipeRules.IsComplete(board, board.Initial), "starts unsolved");
                Expect(PipeRules.IsComplete(board, board.Solution), "reference solution is valid");
                Expect(PipeRules.CountSolutions(board) == 1, "one exact mathematical solution");
                int[] invalid = (int[])board.Solution.Clone();
                invalid[0] = 100;
                Expect(!PipeRules.ValidRotations(board, invalid), "corrupt rotation rejected");
                invalid = (int[])board.Solution.Clone();
                invalid[0] = invalid[0] % PipeRules.OrientationCount(board.Kinds[0]) + 1;
                Expect(!PipeRules.IsComplete(board, invalid), "wrong rotation does not win");
            }

            WorldModel living = WorldModel.New();
            Expect(living.Mode == PlayMode.Living && living.Round == 0, "default mode");
            Expect(living.CanSwitchMode, "can pick comparison mode before playing");
            Expect(!living.Choose(VillageChoice.ShadeTree), "choice gated until puzzle solved");
            Expect(!living.Rotate(-1) && !living.Rotate(9), "index bounds checked");
            Expect(living.Rotate(0), "first tile rotates");
            Expect(!living.CanSwitchMode, "cannot change mode mid-puzzle");
            Expect(living.Undo(), "undo works");
            Expect(living.CanSwitchMode, "undo restores switch option");

            Solve(living);
            Expect(living.CanUndo && !living.Rotate(0), "no extra rotations after solution");
            Expect(living.Undo() && !living.Solved, "undo a solved state");
            Solve(living);
            Expect(living.Choose(VillageChoice.ShadeTree), "first event");
            Expect(living.Round == 1 && living.Count(VillageChoice.ShadeTree) == 1, "tree consequence recorded");
            Expect(living.Memory.Contains("Mira"), "Mira remembers tree choice");
            Expect(!living.Choose(VillageChoice.Bridge), "no unsolved choice duplication");

            Solve(living);
            Expect(living.Choose(VillageChoice.Bridge), "bridge event");
            Expect(living.Consequence.Contains("crossing"), "combined consequence visible");
            Solve(living);
            Expect(living.Choose(VillageChoice.Flowers), "flower event");
            Expect(living.Count(VillageChoice.Flowers) == 1, "flower event persisted");
            Expect(living.Characters.Length == 3, "three distinct NPC strings");

            WorldSnapshot snapshot = living.Export();
            WorldModel restored = WorldModel.Restore(snapshot);
            Expect(restored.Round == 3 && restored.Choices.Length == 3, "round-trip save reconstructs history");
            Expect(restored.Consequence == living.Consequence, "derived state stable after reload");
            snapshot.choices = new[] { 0, 999, 2 };
            Expect(WorldModel.Restore(snapshot).Round == 0, "bad action fails closed");
            snapshot = living.Export();
            snapshot.rotations[0] = 999;
            Expect(WorldModel.Restore(snapshot).Round == 0, "bad pipe rotation fails closed");
            snapshot = living.Export();
            snapshot.solved = true;
            Expect(WorldModel.Restore(snapshot).Round == 0, "forged completed puzzle fails closed");

            for (int index = 3; index < PuzzleCatalog.Count; index++)
            {
                Solve(restored);
                Expect(restored.Choose((VillageChoice)(index % 3)), "later choice accepted");
            }
            Expect(restored.Completed && !restored.Choose(VillageChoice.Bridge), "world stops after six puzzles");
            Expect(restored.Export().rotations.Length == 0, "finished world has no dangling board");
            Expect(WorldModel.Restore(restored.Export()).Completed, "finished state persists");

            WorldModel control = WorldModel.New(PlayMode.PuzzleOnly);
            Solve(control);
            Expect(!control.Choose(VillageChoice.Flowers), "puzzle-only cannot change the world");
            Expect(control.NextPuzzle(), "puzzle-only advances");
            Expect(control.Choices.Length == 0 && control.Round == 1, "no simulated memories in control");

            Console.WriteLine("Evergrove C# domain tests passed: " + assertions + " assertions");
            return 0;
        }
        catch (Exception error)
        {
            Console.Error.WriteLine(error);
            return 1;
        }
    }
}
