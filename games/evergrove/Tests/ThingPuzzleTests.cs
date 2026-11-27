using System;
using System.Collections.Generic;
using Evergrove.Core;

/// <summary>
/// I243: which puzzle a thing holds. The story's first four things teach the classic three;
/// later things and the nursery's restocked editions bring the five new kinds.
/// </summary>
internal static class ThingPuzzleTests
{
    private static int assertions;

    private static void Expect(bool ok, string message)
    {
        assertions++;
        if (!ok) throw new Exception("Thing puzzle test failed: " + message);
    }

    private static readonly PuzzleKind[] NewKinds = {
        PuzzleKind.Nonogram, PuzzleKind.Mahjong, PuzzleKind.Sudoku, PuzzleKind.Plots, PuzzleKind.StreamPath
    };

    public static int Run()
    {
        // The classic three by family, for the first four story things and edition 0.
        for (int chapter = 0; chapter < ThingPuzzle.ClassicChapters; chapter++)
        {
            Expect(ThingPuzzle.ForStory(chapter, VillageChoice.ShadeTree) == PuzzleKind.Water, "trees hold water");
            Expect(ThingPuzzle.ForStory(chapter, VillageChoice.Flowers) == PuzzleKind.Pairs, "flowers hold Pairs");
            Expect(ThingPuzzle.ForStory(chapter, VillageChoice.Bridge) == PuzzleKind.Mosaic, "buildings hold a Mosaic");
        }
        for (int slot = 0; slot < Nursery.Slots; slot++)
        {
            int kind = slot; // edition 0
            Expect(ThingPuzzle.ForNursery(kind) == ThingPuzzle.Classic(Nursery.Family(kind)),
                "edition 0 keeps its family's classic puzzle (" + kind + ")");
        }
        // Later story things: every new kind appears in one garden's twelve, for any order of choices.
        foreach (int start in new[] { 0, 1, 2 })
        {
            var seen = new HashSet<PuzzleKind>();
            for (int chapter = ThingPuzzle.ClassicChapters; chapter < GardenCatalog.ChaptersPerGarden; chapter++)
            {
                PuzzleKind k = ThingPuzzle.ForStory(chapter, (VillageChoice)((chapter + start) % 3));
                Expect(Array.IndexOf(NewKinds, k) >= 0, "a later story thing holds a new kind: " + k);
                seen.Add(k);
            }
            Expect(seen.Count == NewKinds.Length, "a garden's later things show all five new kinds (start " + start + ")");
        }
        // The nursery's restocked editions: new kinds only, and each new kind in at least two families.
        var families = new Dictionary<PuzzleKind, HashSet<VillageChoice>>();
        for (int kind = Nursery.Slots; kind < Nursery.Kinds; kind++)
        {
            PuzzleKind k = ThingPuzzle.ForNursery(kind);
            Expect(Array.IndexOf(NewKinds, k) >= 0, "a restocked plant holds a new kind (" + kind + ")");
            if (!families.ContainsKey(k)) families[k] = new HashSet<VillageChoice>();
            families[k].Add(Nursery.Family(kind));
        }
        foreach (PuzzleKind k in NewKinds)
            Expect(families.ContainsKey(k) && families[k].Count >= 2, k + " grows in at least two families");
        // Deterministic and in range.
        Expect(ThingPuzzle.ForStory(7, VillageChoice.Flowers) == ThingPuzzle.ForStory(7, VillageChoice.Flowers), "same answer");
        Expect(ThingPuzzle.ForStory(-1, VillageChoice.Flowers) == PuzzleKind.Pairs, "a bad chapter falls back to classic");
        Expect(ThingPuzzle.ForNursery(-1) == PuzzleKind.Water && ThingPuzzle.ForNursery(Nursery.Kinds) == PuzzleKind.Water,
            "a bad kind falls back to water");
        return assertions;
    }
}
