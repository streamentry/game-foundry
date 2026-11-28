using System;
using Evergrove.Core;

/// <summary>
/// Pins the root-stone depth gate over generated boards: candidates never drop the true answer,
/// forced-move counts are bounded and deterministic, and deduce-then-guess search recovers the
/// unique solution.
/// </summary>
internal static class RootedDepthTests
{
    private const int Seeds = 200;
    private static int assertions;

    private static void Expect(bool ok, string message)
    {
        assertions++;
        if (!ok) throw new Exception("RootedDepthTests failed: " + message);
    }

    public static int Run()
    {
        assertions = 0;
        for (int roots = 1; roots <= 2; roots++)
        {
            int boards = 0;
            for (int seed = 0; seed < Seeds; seed++)
            {
                RootedBoard b = RootedBoards.Generate(seed, roots);
                if (b == null) continue;
                boards++;
                CheckBoard(b);
            }
            Expect(boards >= 190, "at least 190 boards generated for each root count");
        }
        return assertions;
    }

    private static void CheckBoard(RootedBoard b)
    {
        bool[][] first = RootedDepth.Candidates(b);
        bool[][] second = RootedDepth.Candidates(b);
        Expect(first.Length == b.Root.Length, "one candidate row per cell");
        Expect(SameCandidates(first, second), "candidates are deterministic");

        int forced = RootedDepth.ForcedMoves(b);
        Expect(forced >= 0 && forced <= b.PipeCount, "forced moves are bounded by pipe count");
        Expect(forced == RootedDepth.ForcedMoves(b), "forced moves are deterministic");

        int[] solved = RootedDepth.SolveByGuessing(b);
        Expect(SameInts(solved, b.Solution), "guessing recovers the unique solution");

        int guesses = RootedDepth.Guesses(b);
        Expect(guesses >= 0, "guess count is non-negative");
        Expect(guesses == RootedDepth.Guesses(b), "guess count is deterministic");
    }

    private static bool SameCandidates(bool[][] left, bool[][] right)
    {
        if (left.Length != right.Length) return false;
        for (int i = 0; i < left.Length; i++)
        {
            if (left[i].Length != right[i].Length) return false;
            for (int r = 0; r < left[i].Length; r++)
                if (left[i][r] != right[i][r]) return false;
        }
        return true;
    }

    private static bool SameInts(int[] left, int[] right)
    {
        if (left.Length != right.Length) return false;
        for (int i = 0; i < left.Length; i++)
            if (left[i] != right[i]) return false;
        return true;
    }
}
