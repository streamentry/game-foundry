using System;
using Evergrove.Core;

/// <summary>
/// Pins the linked-fork solver gate over generated boards: same seed gives the same board, every
/// board has one linked answer, the truth survives deduction, and the link-needed and guess counts
/// over the measured seeds are locked. Solver gate only; nothing in play uses these boards.
/// </summary>
public static class LinkedForksTests
{
    private const int Seeds = 60;
    private static int assertions;

    // Measured on seeds 0..59 after the review fix: the generator keeps only boards with two or more
    // answers without the link, so the link is needed on every board. Link-aware deduction alone
    // settles 47 of 60; 13 still need a guess (at most 4).
    private const int ExpectedLinkNeeded = 60;
    private const int ExpectedNeedingGuess = 13;
    private const int ExpectedMaxGuesses = 4;

    private static void Expect(bool ok, string message)
    {
        assertions++;
        if (!ok) throw new Exception("LinkedForksTests failed: " + message);
    }

    public static int Run()
    {
        assertions = 0;
        int boards = 0, linkNeeded = 0, needingGuess = 0, maxGuesses = 0;
        for (int seed = 0; seed < Seeds; seed++)
        {
            LinkedForkBoard b = LinkedForks.Generate(seed);
            LinkedForkBoard again = LinkedForks.Generate(seed);
            Expect(SameBoard(b, again), "generation is deterministic");
            Expect(b != null, "a linked board is generated for seed " + seed);
            if (b == null) continue;
            boards++;

            Expect(b.A != b.B && !LinkedForks.Adjacent(b.Board.Size, b.A, b.B), "the linked pair is distinct and non-adjacent");
            Expect(LinkedForks.CountSolutions(b, 2) == 1, "a linked board has exactly one linked answer");
            Expect(SolutionSatisfiesLink(b), "the true answer satisfies the link with one shared turn count");
            Expect(LinkedForks.TruthSurvivesDeduction(b), "deduction keeps every true mask");

            int guesses = LinkedForks.Guesses(b);
            Expect(guesses >= 0, "guess count is non-negative");
            if (LinkedForks.LinkNeeded(b)) linkNeeded++;
            if (guesses > 0) needingGuess++;
            maxGuesses = Math.Max(maxGuesses, guesses);
        }

        Console.WriteLine("LinkedForksTests: seeds " + Seeds + ", boards " + boards +
            ", link needed " + linkNeeded + ", needing guess " + needingGuess + ", max guesses " + maxGuesses);
        Expect(boards == Seeds, "every seed generates a board");
        Expect(linkNeeded == ExpectedLinkNeeded, "link-needed count is locked");
        Expect(needingGuess == ExpectedNeedingGuess, "boards needing a guess are locked");
        Expect(maxGuesses == ExpectedMaxGuesses, "maximum guesses is locked");
        return assertions;
    }

    // Some single turn count k brings both linked cells to their solution masks.
    private static bool SolutionSatisfiesLink(LinkedForkBoard b)
    {
        for (int k = 0; k < 4; k++)
        {
            if (ForkBoards.Rotate(b.Board.Initial[b.A], k) == b.Board.Solution[b.A] &&
                ForkBoards.Rotate(b.Board.Initial[b.B], k) == b.Board.Solution[b.B]) return true;
        }
        return false;
    }

    private static bool SameBoard(LinkedForkBoard a, LinkedForkBoard b)
    {
        if (a == null || b == null) return a == null && b == null;
        return a.A == b.A && a.B == b.B && SameMasks(a.Board.Solution, b.Board.Solution) && SameMasks(a.Board.Initial, b.Board.Initial);
    }

    private static bool SameMasks(int[] a, int[] b)
    {
        if (a == null || b == null || a.Length != b.Length) return false;
        for (int i = 0; i < a.Length; i++) if (a[i] != b[i]) return false;
        return true;
    }
}
