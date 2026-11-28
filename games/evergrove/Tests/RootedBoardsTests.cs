using System;
using System.Collections.Generic;
using Evergrove.Core;

/// <summary>
/// Root stone solver gate (K5). Pins that generated 4x4 root boards have exactly one
/// answer, that ports facing a root or the edge count as leaks, and reports the yield
/// and a solver-node difficulty proxy.
/// </summary>
internal static class RootedBoardsTests
{
    private const int Cells = 16;
    private static int assertions;

    private static void Expect(bool ok, string message)
    {
        assertions++;
        if (!ok) throw new Exception("RootedBoardsTests failed: " + message);
    }

    public static int Run()
    {
        assertions = 0;
        var yields = new int[3];
        var nodes = new List<int>();
        RootedBoard sample = null;

        for (int roots = 1; roots <= 2; roots++)
        {
            for (int seed = 0; seed < 200; seed++)
            {
                RootedBoard board = RootedBoards.Generate(seed, roots);
                if (board == null) continue;
                yields[roots]++;
                string tag = "seed " + seed + " roots " + roots;
                CheckBoard(board, roots, tag);
                nodes.Add(RootedBoards.SolverNodes(board));
                RootedBoard again = RootedBoards.Generate(seed, roots);
                Expect(again != null && SameArray(board.Root, again.Root) && SameArray(board.Kinds, again.Kinds) &&
                       SameArray(board.Solution, again.Solution) && SameArray(board.Initial, again.Initial),
                       tag + " is deterministic");
                if (sample == null) sample = board;
            }
        }

        nodes.Sort();
        int median = nodes.Count == 0 ? 0 : nodes[nodes.Count / 2];
        Console.WriteLine("RootedBoards yield: " + yields[1] + "/200 (1 root), " + yields[2] +
                          "/200 (2 roots), median solver nodes " + median);
        Expect(yields[1] + yields[2] >= 380, "at least 95% of (seed, roots) pairs generate");
        Expect(sample != null, "a sample board exists");
        if (sample != null) CheckRootFacingLeak(sample);

        ExpectOutOfRange(() => RootedBoards.Generate(0, 0), "Generate(0, 0) throws");
        ExpectOutOfRange(() => RootedBoards.Generate(0, 3), "Generate(0, 3) throws");

        return assertions;
    }

    private static void CheckBoard(RootedBoard b, int roots, string tag)
    {
        int rootCount = 0, ends = 0, differs = 0;
        for (int i = 0; i < Cells; i++)
        {
            if (b.Root[i])
            {
                rootCount++;
                Expect(b.Solution[i] == 0 && b.Initial[i] == 0, tag + " root cell carries no turn");
                continue;
            }
            int turns = Evergrove.Core.PipeRules.OrientationCount(b.Kinds[i]);
            Expect(b.Solution[i] >= 1 && b.Solution[i] <= turns, tag + " solution in range");
            Expect(b.Initial[i] >= 1 && b.Initial[i] <= turns, tag + " initial in range");
            if (b.Initial[i] != b.Solution[i]) differs++;
            if (b.Kinds[i] == PipeKind.End) ends++;
        }
        Expect(rootCount == roots, tag + " root count");
        Expect(ends == 2, tag + " exactly two End tiles");
        Expect(differs >= 3, tag + " initial differs on at least 3 pipes");
        Expect(RootedBoards.CountSolutions(b, 2) == 1, tag + " exactly one solution");
        Expect(RootedBoards.IsComplete(b, b.Solution), tag + " solution is complete");
        Expect(!RootedBoards.IsComplete(b, b.Initial), tag + " scrambled start is not complete");
        Expect(RootedBoards.Leaks(b, b.Solution) == 0, tag + " solution has no leaks");
    }

    // A pipe turned to face a neighbouring root stone must leak.
    private static void CheckRootFacingLeak(RootedBoard b)
    {
        for (int r = 0; r < Cells; r++)
        {
            if (!b.Root[r]) continue;
            for (int d = 0; d < 4; d++)
            {
                int j = Evergrove.Core.PipeRules.Neighbor(r, d, 4);
                if (j < 0 || b.Root[j]) continue;
                int back = (d + 2) % 4;
                int[] rotations = (int[])b.Solution.Clone();
                int turns = Evergrove.Core.PipeRules.OrientationCount(b.Kinds[j]);
                for (int rotation = 1; rotation <= turns; rotation++)
                {
                    if (!Evergrove.Core.PipeRules.HasPort(b.Kinds[j], rotation, back)) continue;
                    rotations[j] = rotation;
                    break;
                }
                Expect(RootedBoards.Leaks(b, rotations) > 0, "a port facing a root is a leak");
                Expect(!RootedBoards.IsComplete(b, rotations), "a root-facing port is not complete");
                return;
            }
        }
        Expect(false, "sample board has a root beside a pipe");
    }

    private static void ExpectOutOfRange(Action action, string message)
    {
        try
        {
            action();
        }
        catch (ArgumentOutOfRangeException)
        {
            Expect(true, message);
            return;
        }
        Expect(false, message + " should throw");
    }

    private static bool SameArray<T>(T[] a, T[] b)
    {
        if (a.Length != b.Length) return false;
        for (int i = 0; i < a.Length; i++)
            if (!EqualityComparer<T>.Default.Equals(a[i], b[i])) return false;
        return true;
    }
}
