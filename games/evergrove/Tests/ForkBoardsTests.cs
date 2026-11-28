using System;
using Evergrove.Core;

/// <summary>
/// Pins the fork-tile solver gate over generated boards: every generated board has one answer,
/// candidates never drop the truth, and deduce-then-guess search recovers that answer. Also checks
/// the completeness rules on hand-built masks.
/// </summary>
internal static class ForkBoardsTests
{
    private const int Seeds = 100;
    private const int MinBoards = 90;
    private static int assertions;

    private static void Expect(bool ok, string message)
    {
        assertions++;
        if (!ok) throw new Exception("ForkBoardsTests failed: " + message);
    }

    public static int Run()
    {
        assertions = 0;
        int boards = 0;
        for (int seed = 0; seed < Seeds; seed++)
        {
            ForkBoard b = ForkBoards.Generate(seed);
            Expect(SameBoard(b, ForkBoards.Generate(seed)), "generation is deterministic");
            if (b == null) continue;
            boards++;
            CheckBoard(b);
        }
        Expect(boards >= MinBoards, "enough boards generated across seeds");
        CheckHandMade();
        return assertions;
    }

    private static void CheckBoard(ForkBoard b)
    {
        Expect(b.ForkCount >= 3, "boards carry at least three forks");
        Expect(ForkBoards.CountSolutions(b, 2) == 1, "generated boards have exactly one answer");
        Expect(ForkBoards.IsComplete(b, b.Solution), "the solution is complete");
        Expect(!ForkBoards.IsComplete(b, b.Initial), "the scrambled start is not complete");
        CheckInitialIsRotation(b);

        bool[][] candidates = ForkBoards.Candidates(b);
        Expect(candidates.Length == ForkBoard.Cells, "one candidate row per cell");
        for (int i = 0; i < ForkBoard.Cells; i++)
            Expect(HasSolutionCandidate(b, candidates, i), "candidates keep the solution orientation");

        int forced = ForkBoards.ForcedMoves(b);
        Expect(forced >= 0 && forced <= ForkBoard.Cells, "forced moves are bounded by cell count");
        Expect(forced == ForkBoards.ForcedMoves(b), "forced moves are deterministic");
        Expect(ForkBoards.Guesses(b) >= 0, "guess count is non-negative");

        int[] solved = ForkBoards.SolveByGuessing(b);
        Expect(SameMasks(solved, b.Solution), "guessing recovers the unique solution");
    }

    private static void CheckInitialIsRotation(ForkBoard b)
    {
        for (int i = 0; i < ForkBoard.Cells; i++)
            Expect(ForkBoards.Canonical(b.Initial[i]) == ForkBoards.Canonical(b.Solution[i]),
                "each scrambled tile is a rotation of its solution tile");
    }

    private static bool HasSolutionCandidate(ForkBoard b, bool[][] candidates, int cell)
    {
        int tileBase = ForkBoards.Canonical(b.Solution[cell]);
        for (int r = 0; r < 4; r++)
            if (candidates[cell][r] && ForkBoards.Rotate(tileBase, r) == b.Solution[cell]) return true;
        return false;
    }

    private static void CheckHandMade()
    {
        ForkBoard seedBoard = FirstBoard();
        Expect(seedBoard != null, "a generated board exists for hand-made checks");

        // A north port on the top-left corner faces the board edge: never complete.
        int[] edgeLeak = (int[])seedBoard.Solution.Clone();
        edgeLeak[0] = RotationWithPort(seedBoard.Solution[0], 0);
        Expect(!ForkBoards.IsComplete(seedBoard, edgeLeak), "a port facing the edge is not complete");

        // A 2x2 loop on the top-left block with everything else empty: the ports cannot form one tree.
        var loopSolution = new int[ForkBoard.Cells];
        loopSolution[0] = 6;  // east and south
        loopSolution[1] = 12; // south and west
        loopSolution[5] = 3;  // north and east
        loopSolution[6] = 9;  // north and west
        var loopBoard = new ForkBoard(loopSolution, loopSolution);
        Expect(!ForkBoards.IsComplete(loopBoard, loopBoard.Solution), "a 2x2 loop is rejected");

        // A mask that is not a rotation of the tile is never complete.
        int[] wrongTile = (int[])seedBoard.Solution.Clone();
        wrongTile[7] = 15;
        Expect(!ForkBoards.IsComplete(seedBoard, wrongTile), "a mask outside the tile's rotations is rejected");
    }

    // A rotation of the cell's tile with its north port set, so it faces the top edge.
    private static int RotationWithPort(int tileMask, int direction)
    {
        for (int r = 0; r < 4; r++)
        {
            int rotated = ForkBoards.Rotate(tileMask, r);
            if (((rotated >> direction) & 1) != 0) return rotated;
        }
        throw new InvalidOperationException("Nonzero tile masks have a rotation with every port.");
    }

    private static ForkBoard FirstBoard()
    {
        for (int seed = 0; seed < Seeds; seed++)
        {
            ForkBoard b = ForkBoards.Generate(seed);
            if (b != null) return b;
        }
        return null;
    }

    private static bool SameBoard(ForkBoard a, ForkBoard b)
    {
        if (a == null || b == null) return a == null && b == null;
        return SameMasks(a.Solution, b.Solution) && SameMasks(a.Initial, b.Initial);
    }

    private static bool SameMasks(int[] a, int[] b)
    {
        if (a == null || b == null || a.Length != b.Length) return false;
        for (int i = 0; i < a.Length; i++) if (a[i] != b[i]) return false;
        return true;
    }
}
