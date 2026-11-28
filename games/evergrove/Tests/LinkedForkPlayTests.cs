using System;
using System.Collections.Generic;
using Evergrove.Core;

/// <summary>
/// Vision run 20261128-2001, idea 1: pins the axle play model on the drawn chapter. An axle tap turns both ends,
/// undo restores exact masks, and every drawn board solves by its fewest taps. Not wired into play.
/// </summary>
internal static class LinkedForkPlayTests
{
    private const int Length = 12;
    private const int SeedLimit = 240;
    private static readonly int[] ExpectedFewestTaps = { 34, 30, 38, 35, 41, 23, 31, 33, 32, 36, 39, 31 };
    private static int assertions;

    private static void Expect(bool ok, string message)
    {
        assertions++;
        if (!ok) throw new Exception("LinkedForkPlayTests failed: " + message);
    }

    private static bool SameMasks(int[] x, int[] y)
    {
        if (x.Length != y.Length) return false;
        for (int i = 0; i < x.Length; i++)
            if (x[i] != y[i]) return false;
        return true;
    }

    private static bool Throws<TException>(Action action) where TException : Exception
    {
        try
        {
            action();
        }
        catch (TException)
        {
            return true;
        }
        return false;
    }

    public static int Run()
    {
        assertions = 0;

        int[] seeds = LinkedForkChapter.DrawByLevel(Length, SeedLimit);
        Expect(seeds.Length == Length, "chapter draws twelve seeds");

        LinkedForkBoard first = LinkedForks.Generate(seeds[0]);
        Expect(first != null, "first chapter seed has a linked board");
        AxleTapTests(first);
        PlainTapTests(first);
        UndoTests(first);
        Expect(Throws<ArgumentOutOfRangeException>(() => new LinkedForkPlay(first).Tap(-1)), "tap below range throws");
        Expect(Throws<ArgumentOutOfRangeException>(() => new LinkedForkPlay(first).Tap(first.Board.CellCount)),
            "tap above range throws");

        var fewest = new int[seeds.Length];
        for (int i = 0; i < seeds.Length; i++)
        {
            LinkedForkBoard board = LinkedForks.Generate(seeds[i]);
            Expect(board != null, "chapter seed has a linked board");
            Expect(!new LinkedForkPlay(board).Solved, "a fresh play is not solved");

            fewest[i] = LinkedForkPlay.FewestTaps(board);
            SolvesByFewestTaps(board, fewest[i]);
        }
        Expect(SameMasks(fewest, ExpectedFewestTaps), "locked fewest taps for the chapter");

        Console.WriteLine("LinkedForkPlayTests: " + assertions + " assertions");
        return assertions;
    }

    private static void AxleTapTests(LinkedForkBoard board)
    {
        var play = new LinkedForkPlay(board);
        int[] before = play.Masks;
        Expect(play.Partner(board.A) == board.B && play.Partner(board.B) == board.A, "axle ends partner each other");
        Expect(play.IsLinked(board.A) && play.IsLinked(board.B), "axle ends are linked");
        int other = 0;
        while (other == board.A || other == board.B) other++;
        Expect(play.Partner(other) == -1, "partner is -1 for an unlinked cell");

        Expect(play.Tap(board.A), "axle tap returns true");
        int[] after = play.Masks;
        for (int c = 0; c < after.Length; c++)
        {
            int expected = c == board.A || c == board.B
                ? ForkBoards.Rotate(before[c], 1)
                : before[c];
            Expect(after[c] == expected, "axle tap turns both ends and no other cell");
        }
        Expect(play.Taps == 1, "axle tap counts once");

        var fromB = new LinkedForkPlay(board);
        fromB.Tap(board.B);
        Expect(fromB.Masks[board.A] == ForkBoards.Rotate(before[board.A], 1), "tapping B turns A too");
    }

    private static void PlainTapTests(LinkedForkBoard board)
    {
        var play = new LinkedForkPlay(board);
        int[] before = play.Masks;
        int plain = 0;
        while (plain == board.A || plain == board.B) plain++;

        Expect(!play.IsLinked(plain) && play.Partner(plain) == -1, "plain cell is not linked");
        play.Tap(plain);
        int[] after = play.Masks;
        for (int c = 0; c < after.Length; c++)
        {
            int expected = c == plain ? ForkBoards.Rotate(before[c], 1) : before[c];
            Expect(after[c] == expected, "plain tap turns only that cell");
        }
    }

    private static void UndoTests(LinkedForkBoard board)
    {
        var play = new LinkedForkPlay(board);
        int[] start = play.Masks;
        Expect(!play.Undo(), "undo with nothing to undo returns false");

        play.Tap(board.A);
        play.Tap(0);
        play.Tap(board.B);
        Expect(play.Taps == 3, "three taps are counted");
        Expect(play.Undo() && play.Undo() && play.Undo(), "three undos succeed");
        Expect(SameMasks(play.Masks, start), "undo restores exact masks");
        Expect(play.Taps == 0, "no taps left after undoing all");
        Expect(!play.Undo(), "undo after emptying returns false");
    }

    // Tap each unlinked cell its fewest turns, the axle k times, and check the board is solved with Taps matching.
    private static void SolvesByFewestTaps(LinkedForkBoard board, int expectedTaps)
    {
        var play = new LinkedForkPlay(board);
        int[] initial = board.Board.Initial;
        int[] solution = board.Board.Solution;
        int axle = 0;
        while (ForkBoards.Rotate(initial[board.A], axle) != solution[board.A] ||
               ForkBoards.Rotate(initial[board.B], axle) != solution[board.B]) axle++;

        for (int c = 0; c < initial.Length; c++)
        {
            if (c == board.A || c == board.B) continue;
            int turns = 0;
            while (ForkBoards.Rotate(initial[c], turns) != solution[c]) turns++;
            for (int t = 0; t < turns; t++) play.Tap(c);
        }
        for (int t = 0; t < axle; t++) play.Tap(board.A);

        Expect(play.Solved, "fewest-tap play solves the board");
        Expect(play.Taps == expectedTaps, "taps equal FewestTaps");
    }
}
