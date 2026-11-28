using System;
using System.IO;

/// <summary>
/// The finishing bloom follows the water. Pins FlowOrder.StepsFromSpring on every authored board and
/// every reactive variant, checks invalid input and non-mutation, and checks that the Unity puzzle
/// build calls it instead of the old Manhattan distance from the spring.
/// </summary>
internal static class FlowOrderTests
{
    private static int assertions;

    private static void Expect(bool ok, string message)
    {
        assertions++;
        if (!ok) throw new Exception("FlowOrderTests failed: " + message);
    }

    public static int Run()
    {
        assertions = 0;

        for (int board = 0; board < Evergrove.Core.PuzzleCatalog.Count; board++)
        {
            Evergrove.Core.Puzzle puzzle = Evergrove.Core.PuzzleCatalog.Get(board);
            CheckSolved(puzzle, "catalog " + board);
        }
        for (int chapter = 1; chapter <= 11; chapter++)
            for (int variant = 1; variant <= 3; variant++)
                CheckSolved(Evergrove.Core.ReactiveWatercourses.ForChapter(chapter, variant),
                    "chapter " + chapter + " variant " + variant);

        CheckInitial(Evergrove.Core.PuzzleCatalog.Get(0));
        CheckInvalid(Evergrove.Core.PuzzleCatalog.Get(0));
        CheckNoMutation(Evergrove.Core.PuzzleCatalog.Get(0));
        CheckUnityWiring();

        return assertions;
    }

    private static void CheckSolved(Evergrove.Core.Puzzle puzzle, string label)
    {
        int count = puzzle.CellCount;
        int[] steps = Evergrove.Core.FlowOrder.StepsFromSpring(puzzle, puzzle.Solution);
        Expect(steps.Length == count, label + " length is CellCount");

        bool[] seen = new bool[count];
        foreach (int value in steps)
        {
            Expect(value >= 0 && value < count, label + " every solved tile is reached in range");
            Expect(!seen[value], label + " steps are unique");
            seen[value] = true;
        }

        int spring = -1, outlet = -1;
        for (int i = 0; i < count; i++)
            if (puzzle.Kinds[i] == Evergrove.Core.PipeKind.End)
            {
                if (spring < 0) spring = i; else outlet = i;
            }
        Expect(spring >= 0 && outlet >= 0, label + " has two endpoints");
        Expect(steps[spring] == 0, label + " spring is step 0");
        Expect(steps[outlet] == count - 1, label + " outlet is the last step");

        for (int tile = 0; tile < count; tile++)
        {
            int k = steps[tile];
            if (k <= 0) continue;
            Expect(HasJoinedPredecessor(puzzle, puzzle.Solution, steps, tile, k), label + " tile " + tile + " joins step " + (k - 1));
        }
    }

    private static bool HasJoinedPredecessor(Evergrove.Core.Puzzle puzzle, int[] rotations, int[] steps, int tile, int k)
    {
        for (int d = 0; d < 4; d++)
        {
            int next = Evergrove.Core.PipeRules.Neighbor(tile, d, puzzle.Size);
            if (next < 0 || steps[next] != k - 1) continue;
            if (Evergrove.Core.PipeRules.HasPort(puzzle.Kinds[tile], rotations[tile], d) &&
                Evergrove.Core.PipeRules.HasPort(puzzle.Kinds[next], rotations[next], (d + 2) % 4))
                return true;
        }
        return false;
    }

    private static void CheckInitial(Evergrove.Core.Puzzle puzzle)
    {
        int[] steps = Evergrove.Core.FlowOrder.StepsFromSpring(puzzle, puzzle.Initial);
        Expect(steps.Length == puzzle.CellCount, "initial length is CellCount");
        int spring = Array.IndexOf(puzzle.Kinds, Evergrove.Core.PipeKind.End);
        Expect(steps[spring] == 0, "initial spring is step 0");
        bool anyUnreached = false;
        foreach (int value in steps)
        {
            Expect(value == -1 || (value >= 0 && value < puzzle.CellCount), "initial values are -1 or in range");
            if (value == -1) anyUnreached = true;
        }
        Expect(anyUnreached, "initial board 0 has at least one unreached tile");
    }

    private static void CheckInvalid(Evergrove.Core.Puzzle puzzle)
    {
        AssertAllUnreached(puzzle, null, "null rotations");
        AssertAllUnreached(puzzle, new int[puzzle.CellCount - 1], "short rotations");
        AssertAllUnreached(puzzle, new int[puzzle.CellCount + 1], "long rotations");
        int[] zeros = new int[puzzle.CellCount];
        AssertAllUnreached(puzzle, zeros, "zero rotations");
    }

    private static void AssertAllUnreached(Evergrove.Core.Puzzle puzzle, int[] rotations, string label)
    {
        int[] steps = Evergrove.Core.FlowOrder.StepsFromSpring(puzzle, rotations);
        Expect(steps.Length == puzzle.CellCount, label + " length is CellCount");
        foreach (int value in steps)
            Expect(value == -1, label + " is all -1");
    }

    private static void CheckNoMutation(Evergrove.Core.Puzzle puzzle)
    {
        int[] rotations = (int[])puzzle.Solution.Clone();
        int[] before = (int[])rotations.Clone();
        Evergrove.Core.FlowOrder.StepsFromSpring(puzzle, rotations);
        for (int i = 0; i < rotations.Length; i++)
            Expect(rotations[i] == before[i], "rotations are not mutated at " + i);

        int[] initial = (int[])puzzle.Initial.Clone();
        int[] initialBefore = (int[])initial.Clone();
        Evergrove.Core.FlowOrder.StepsFromSpring(puzzle, initial);
        for (int i = 0; i < initial.Length; i++)
            Expect(initial[i] == initialBefore[i], "initial rotations are not mutated at " + i);
    }

    private static void CheckUnityWiring()
    {
        string source = ReadRepoFile("games/evergrove/Assets/Scripts/Unity/EvergroveApp.Puzzle.cs");
        Expect(source != null, "EvergroveApp.Puzzle.cs can be found");
        if (source == null) return;
        Expect(source.Contains("FlowOrder.StepsFromSpring("), "bloom uses FlowOrder.StepsFromSpring");
        Expect(!source.Contains("Mathf.Abs(index / puzzle.Size - spring / puzzle.Size)"), "old Manhattan distance is gone");
    }

    private static string ReadRepoFile(string relative)
    {
        string[] starts = { Directory.GetCurrentDirectory(), AppContext.BaseDirectory };
        foreach (string start in starts)
        {
            string dir = Path.GetFullPath(start);
            for (int depth = 0; depth < 12 && dir != null; depth++)
            {
                string candidate = Path.Combine(dir, relative);
                if (File.Exists(candidate)) return File.ReadAllText(candidate);
                dir = Path.GetDirectoryName(dir);
            }
        }
        return null;
    }
}
