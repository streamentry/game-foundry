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
            Expect(SameBoard(b, ForkBoards.Generate(seed, ForkBoard.Edge)), "the default is the 5x5 board");
            if (b == null) continue;
            boards++;
            CheckBoard(b);
        }
        Expect(boards >= MinBoards, "enough boards generated across seeds");
        CheckHandMade();
        RunSixByEdge();
        return assertions;
    }

    // Measured on 6x6 seeds 0..39 (see the locked constants below): how many boards generated,
    // fork range, forced cells, how many needed a guess, and the worst guess count.
    private const int SixSeeds = 40;
    private const int SixEdge = 6;
    private const int SixExpectedBoards = 40;
    private const int SixExpectedMinForks = 7;
    private const int SixExpectedMaxForks = 12;
    private const int SixExpectedForced = 1394;
    private const int SixExpectedNeedingGuess = 7;
    private const int SixExpectedMaxGuesses = 4;

    private static void RunSixByEdge()
    {
        int generated = 0, minForks = int.MaxValue, maxForks = 0;
        int forced = 0, cells = 0, needingGuess = 0, maxGuesses = 0;
        for (int seed = 0; seed < SixSeeds; seed++)
        {
            ForkBoard b = ForkBoards.Generate(seed, SixEdge);
            Expect(SameBoard(b, ForkBoards.Generate(seed, SixEdge)), "6x6 generation is deterministic");
            if (b == null) continue;
            generated++;
            Expect(b.Size == SixEdge && b.CellCount == 36, "6x6 board has 36 cells");
            CheckBoard(b);
            CheckSpanningTree(b);
            minForks = Math.Min(minForks, b.ForkCount);
            maxForks = Math.Max(maxForks, b.ForkCount);
            forced += ForkBoards.ForcedMoves(b);
            cells += b.CellCount;
            int guesses = ForkBoards.Guesses(b);
            if (guesses > 0) needingGuess++;
            maxGuesses = Math.Max(maxGuesses, guesses);
        }
        Expect(cells == 36 * generated, "6x6 cell total is 36 per board");
        Expect(generated == SixExpectedBoards, "6x6 generated board count is locked");
        Expect(minForks == SixExpectedMinForks && maxForks == SixExpectedMaxForks, "6x6 fork range is locked");
        Expect(forced == SixExpectedForced, "6x6 forced cell count is locked");
        Expect(needingGuess == SixExpectedNeedingGuess, "6x6 boards needing a guess are locked");
        Expect(maxGuesses == SixExpectedMaxGuesses, "6x6 maximum guesses is locked");
        bool rejected = false;
        try { ForkBoards.Generate(0, 3); }
        catch (ArgumentOutOfRangeException) { rejected = true; }
        Expect(rejected, "edges outside 4 to 6 are rejected");
    }

    // Independent check on the solution: every port matched, joins = cells - 1, all cells connected.
    private static void CheckSpanningTree(ForkBoard b)
    {
        int n = b.Size, cells = b.CellCount, joins = 0;
        for (int i = 0; i < cells; i++)
            for (int d = 0; d < 4; d++)
            {
                if (((b.Solution[i] >> d) & 1) == 0) continue;
                int j = Step(n, i, d);
                Expect(j >= 0 && ((b.Solution[j] >> ((d + 2) % 4)) & 1) != 0, "every port is matched on the 6x6 solution");
                if (d == 1 || d == 2) joins++;
            }
        Expect(joins == cells - 1, "6x6 solution has cells minus one joins");

        var seen = new bool[cells];
        var pending = new System.Collections.Generic.Stack<int>();
        seen[0] = true;
        pending.Push(0);
        int reached = 0;
        while (pending.Count > 0)
        {
            int i = pending.Pop();
            reached++;
            for (int d = 0; d < 4; d++)
            {
                if (((b.Solution[i] >> d) & 1) == 0) continue;
                int j = Step(n, i, d);
                if (j < 0 || seen[j]) continue;
                seen[j] = true;
                pending.Push(j);
            }
        }
        Expect(reached == cells, "6x6 solution is connected");
    }

    private static int Step(int n, int cell, int d)
    {
        int row = cell / n, col = cell % n;
        if (d == 0) row--;
        else if (d == 1) col++;
        else if (d == 2) row++;
        else col--;
        return row >= 0 && row < n && col >= 0 && col < n ? row * n + col : -1;
    }

    private static void CheckBoard(ForkBoard b)
    {
        Expect(b.ForkCount >= 3, "boards carry at least three forks");
        Expect(ForkBoards.CountSolutions(b, 2) == 1, "generated boards have exactly one answer");
        Expect(ForkBoards.IsComplete(b, b.Solution), "the solution is complete");
        Expect(!ForkBoards.IsComplete(b, b.Initial), "the scrambled start is not complete");
        CheckInitialIsRotation(b);

        bool[][] candidates = ForkBoards.Candidates(b);
        Expect(candidates.Length == b.CellCount, "one candidate row per cell");
        for (int i = 0; i < b.CellCount; i++)
            Expect(HasSolutionCandidate(b, candidates, i), "candidates keep the solution orientation");

        int forced = ForkBoards.ForcedMoves(b);
        Expect(forced >= 0 && forced <= b.CellCount, "forced moves are bounded by cell count");
        Expect(forced == ForkBoards.ForcedMoves(b), "forced moves are deterministic");
        Expect(ForkBoards.Guesses(b) >= 0, "guess count is non-negative");

        int[] solved = ForkBoards.SolveByGuessing(b);
        Expect(SameMasks(solved, b.Solution), "guessing recovers the unique solution");
    }

    private static void CheckInitialIsRotation(ForkBoard b)
    {
        for (int i = 0; i < b.CellCount; i++)
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
