using System;
using Evergrove.Core;

// I373: snug fits. They sit inside the plain fits, say less than them, and the answer's own
// placements never leave a dead pocket.
internal static class GardenPiecesFitTests
{
    private static int assertions;

    private static void Expect(bool condition, string message)
    {
        assertions++;
        if (!condition) throw new Exception("GardenPiecesFit: " + message);
    }

    public static int Run()
    {
        assertions = 0;
        Expect(GardenPiecesFit.Snug(null, 0).Length == 0, "no game, no glow");
        int plainTotal = 0, snugTotal = 0;
        for (int tier = 0; tier < TreePuzzles.Tiers; tier++)
        {
            for (int seed = 1; seed <= 60; seed++)
            {
                var g = new GardenPiecesGame(seed, tier);
                int size = g.Width * g.Height;
                Expect(!GardenPiecesFit.LeavesDeadPocket(g, new bool[size], 2), "a fresh board has no dead pocket");
                for (int p = 0; p < g.PieceCount; p++)
                {
                    bool[] snug = GardenPiecesFit.Snug(g, p);
                    bool[] plain = Plain(g, p);
                    for (int c = 0; c < size; c++)
                    {
                        if (snug[c]) { Expect(plain[c] && g.InShape(c), "snug squares are fit squares"); snugTotal++; }
                        if (plain[c]) plainTotal++;
                    }
                }
                FollowAnswer(g);
            }
        }
        Console.WriteLine("Garden pieces snug glow: " + snugTotal + " of " + plainTotal + " plain fit squares");
        Expect(snugTotal * 2 < plainTotal, "snug glow says far less than the plain glow (" + snugTotal + " of " + plainTotal + ")");
        return assertions;
    }

    // Lay the answer piece by piece: no step leaves a pocket the rest could not fill.
    private static void FollowAnswer(GardenPiecesGame g)
    {
        for (int guard = 0; guard < 40 && !g.Solved; guard++)
        {
            GardenHint hint = g.Hint();
            if (!hint.Has || hint.Misplaced) break;
            while (g.Turn(hint.Piece) != hint.Turn) g.Rotate(hint.Piece);
            if (g.Mirrored(hint.Piece) != hint.Mirrored) g.Mirror(hint.Piece);
            int smallest = int.MaxValue;
            for (int p = 0; p < g.PieceCount; p++)
                if (p != hint.Piece && !g.Placed(p)) smallest = Math.Min(smallest, g.PieceCells(p).Length / 2);
            if (smallest == int.MaxValue) smallest = 1;
            bool[] covered = GardenPiecesFit.Cover(g, g.PieceCells(hint.Piece), hint.Cell);
            Expect(!GardenPiecesFit.LeavesDeadPocket(g, covered, smallest), "the answer never leaves a dead pocket");
            bool[] snug = GardenPiecesFit.Snug(g, hint.Piece);
            bool any = false;
            foreach (bool s in snug) any |= s;
            Expect(any, "a piece turned as the answer has it always has a snug fit");
            Expect(g.Place(hint.Piece, hint.Cell) == PieceMove.Done, "answer placement is accepted");
        }
        Expect(g.Solved, "the answer solves the board");
    }

    private static bool[] Plain(GardenPiecesGame g, int piece)
    {
        var fits = new bool[g.Width * g.Height];
        if (g.Placed(piece)) return fits;
        int[] offsets = g.PieceCells(piece);
        for (int anchor = 0; anchor < fits.Length; anchor++)
        {
            if (!g.CanPlace(piece, anchor)) continue;
            int ax = anchor % g.Width, ay = anchor / g.Width;
            for (int k = 0; k + 1 < offsets.Length; k += 2) fits[(ay + offsets[k + 1]) * g.Width + ax + offsets[k]] = true;
        }
        return fits;
    }
}
