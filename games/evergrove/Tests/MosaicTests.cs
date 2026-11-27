using System;
using System.Collections.Generic;
using Evergrove.Core;

/// <summary>Mosaic, the puzzle buildings hold (depth spine, Layer 2): swap tiles to mend a painting.</summary>
internal static class MosaicTests
{
    private static int assertions;

    private static void Expect(bool ok, string message)
    {
        assertions++;
        if (!ok) throw new Exception("Mosaic test failed: " + message);
    }

    public static int Run()
    {
        Expect(MosaicGame.SizeFor(0) == 3 && MosaicGame.SizeFor(1) == 4 && MosaicGame.SizeFor(2) == 5,
            "3×3, 4×4, 5×5 by tier");
        bool threw = false;
        try { MosaicGame.SizeFor(-1); } catch (ArgumentOutOfRangeException) { threw = true; }
        Expect(threw, "unknown tier refused");

        var deals = new HashSet<string>();
        for (int tier = 0; tier < TreePuzzles.Tiers; tier++)
            for (int seed = 0; seed < 80; seed++)
            {
                var game = new MosaicGame(seed, tier);
                var again = new MosaicGame(seed, tier);
                Expect(game.Count == game.Size * game.Size, "a square of tiles");
                var seen = new bool[game.Count];
                var order = new List<int>();
                int inPlace = 0;
                for (int slot = 0; slot < game.Count; slot++)
                {
                    int piece = game.At(slot);
                    Expect(piece >= 0 && piece < game.Count && !seen[piece], "every piece exactly once");
                    seen[piece] = true;
                    Expect(again.At(slot) == piece, "the same building always deals the same mosaic");
                    if (game.InPlace(slot)) inPlace++;
                    order.Add(piece);
                }
                Expect(!game.Solved && inPlace < game.Count, "never starts mended");
                Expect(inPlace <= game.Count / 3, "most tiles start out of place");
                deals.Add(tier + ":" + string.Join(",", order));
            }
        Expect(deals.Count > 200, "deals differ from building to building");

        // Tap a tile, tap another: they swap. Tapping the picked tile again puts it down.
        var play = new MosaicGame(5, 0);
        int a = 0;
        while (play.InPlace(a)) a++;
        Expect(play.Select(a) == MosaicGame.Step.Picked && play.Picked == a, "first tap picks a tile");
        Expect(play.Select(a) == MosaicGame.Step.PutDown && play.Picked < 0, "tapping it again puts it down");
        int home = -1;
        for (int s = 0; s < play.Count; s++) if (play.At(s) == a) home = s; // where piece a sits now
        play.Select(a);
        Expect(play.Select(home) == MosaicGame.Step.Swapped, "second tap swaps the two");
        Expect(play.InPlace(a) && play.Moves == 1, "piece a is home after one swap");
        Expect(play.Select(a) == MosaicGame.Step.Ignored, "a tile in its place stays put");

        // Mend it greedily: each swap brings one piece home, so it ends within Count-1 swaps.
        var mend = new MosaicGame(23, 2);
        for (int slot = 0; slot < mend.Count; slot++)
        {
            if (mend.InPlace(slot)) continue;
            int from = -1;
            for (int s = 0; s < mend.Count; s++) if (mend.At(s) == slot) from = s;
            Expect(mend.Select(slot) == MosaicGame.Step.Picked, "pick the wrong tile");
            Expect(mend.Select(from) == MosaicGame.Step.Swapped, "swap its piece in");
        }
        Expect(mend.Solved && mend.Moves <= mend.Count - 1, "mended within Count-1 swaps");
        Expect(mend.Select(0) == MosaicGame.Step.Ignored, "a mended painting takes no more taps");
        return assertions;
    }
}
