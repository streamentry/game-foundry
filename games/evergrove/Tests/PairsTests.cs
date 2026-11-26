using System;
using System.Collections.Generic;
using Evergrove.Core;

/// <summary>Pairs, the puzzle flowers hold (depth spine, Layer 2).</summary>
internal static class PairsTests
{
    private static int assertions;

    private static void Expect(bool ok, string message)
    {
        assertions++;
        if (!ok) throw new Exception("Pairs test failed: " + message);
    }

    public static int Run()
    {
        Expect(PairsGame.PairsFor(0) == 4 && PairsGame.PairsFor(1) == 6 && PairsGame.PairsFor(2) == 8,
            "4, 6 and 8 pairs by tier");
        bool threw = false;
        try { PairsGame.PairsFor(3); } catch (ArgumentOutOfRangeException) { threw = true; }
        Expect(threw, "unknown tier refused");

        var tables = new HashSet<string>();
        for (int tier = 0; tier < TreePuzzles.Tiers; tier++)
            for (int seed = 0; seed < 60; seed++)
            {
                var game = new PairsGame(seed, tier);
                var again = new PairsGame(seed, tier);
                var seen = new int[game.Pairs];
                var faces = new List<int>();
                for (int c = 0; c < game.Count; c++)
                {
                    seen[game.Face(c)]++;
                    faces.Add(game.Face(c));
                    Expect(game.Face(c) == again.Face(c), "same flower, same table");
                    Expect(!game.Showing(c), "every card starts face down");
                }
                foreach (int n in seen) Expect(n == 2, "every picture appears exactly twice");
                tables.Add(tier + ":" + string.Join(",", faces));
            }
        Expect(tables.Count > 150, "tables differ from flower to flower");

        // Play one table by memory: a miss shows both cards until the next tap.
        var play = new PairsGame(7, 0);
        int a = 0, b = -1, other = -1;
        for (int c = 1; c < play.Count; c++)
        {
            if (play.Face(c) == play.Face(a)) b = c;
            else if (other < 0) other = c;
        }
        Expect(play.Flip(a) == PairsGame.Turn.First && play.Showing(a), "first card turns up");
        Expect(play.Flip(a) == PairsGame.Turn.Ignored, "tapping it again does nothing");
        Expect(play.Flip(other) == PairsGame.Turn.Miss, "a different picture is a miss");
        Expect(play.Showing(a) && play.Showing(other), "a miss stays up so you can see it");
        Expect(play.Turns == 1, "one turn taken");
        Expect(play.Flip(a) == PairsGame.Turn.First && !play.Showing(other), "the miss turns back on the next tap");
        Expect(play.Flip(b) == PairsGame.Turn.Match && play.Kept(a) && play.Kept(b), "a match is kept");
        Expect(play.Flip(a) == PairsGame.Turn.Ignored, "a kept card can't be turned");
        Expect(!play.Solved, "not solved with pairs left");

        // Finish it perfectly and it is solved in exactly Pairs turns.
        var perfect = new PairsGame(11, 2);
        var done = new bool[perfect.Count];
        for (int c = 0; c < perfect.Count; c++)
        {
            if (done[c]) continue;
            for (int d = c + 1; d < perfect.Count; d++)
                if (!done[d] && perfect.Face(d) == perfect.Face(c))
                {
                    Expect(perfect.Flip(c) == PairsGame.Turn.First, "turn up a card");
                    Expect(perfect.Flip(d) == PairsGame.Turn.Match, "its pair matches");
                    done[c] = done[d] = true;
                    break;
                }
        }
        Expect(perfect.Solved && perfect.Turns == perfect.Pairs, "solved in one turn per pair");
        Expect(perfect.Flip(0) == PairsGame.Turn.Ignored, "a solved table takes no more turns");
        return assertions;
    }
}
