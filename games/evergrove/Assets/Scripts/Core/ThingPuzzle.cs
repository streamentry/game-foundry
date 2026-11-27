using System;

namespace Evergrove.Core
{
    /// <summary>The kinds of puzzle a garden thing can hold.</summary>
    public enum PuzzleKind { Water, Pairs, Mosaic, Nonogram, Mahjong, Sudoku, Plots, StreamPath, Sowing, Sums, Pieces, Doorstep }

    /// <summary>
    /// I243: which puzzle a thing holds. The story's first four things teach the classic
    /// three by family (trees water, flowers Pairs, buildings a Mosaic). Later story things
    /// and the nursery's restocked editions (I240) bring the five new kinds, so growing and
    /// buying both open new play. Deterministic: a thing always holds the same kind.
    /// </summary>
    public static class ThingPuzzle
    {
        public const int ClassicChapters = 4;

        // Later story chapters (5 to 12) cycle through the new kinds by chapter, so whatever
        // you choose, one garden's eight later things show all five.
        // I318/I324: sowing and seed sums (ideas 8 and 10) join the cycle, so chapters 5 to 12
        // (eight things) meet all seven newer kinds in every garden.
        private static readonly PuzzleKind[] Cycle = {
            PuzzleKind.StreamPath, PuzzleKind.Nonogram, PuzzleKind.Plots, PuzzleKind.Sudoku, PuzzleKind.Mahjong,
            PuzzleKind.Sowing, PuzzleKind.Sums
        };

        // Nursery editions 1..4 per family (each new kind grows in at least two families).
        private static readonly PuzzleKind[][] ByEdition = {
            new[] { PuzzleKind.StreamPath, PuzzleKind.Nonogram, PuzzleKind.Sudoku, PuzzleKind.Plots },   // trees
            new[] { PuzzleKind.Plots, PuzzleKind.Mahjong, PuzzleKind.StreamPath, PuzzleKind.Pieces },    // buildings (I331)
            new[] { PuzzleKind.Nonogram, PuzzleKind.Sudoku, PuzzleKind.Mahjong, PuzzleKind.Doorstep },  // flowers (I332)
        };

        public static PuzzleKind Classic(VillageChoice family)
        {
            return family == VillageChoice.Flowers ? PuzzleKind.Pairs
                : family == VillageChoice.Bridge ? PuzzleKind.Mosaic : PuzzleKind.Water;
        }

        public static PuzzleKind ForStory(int chapter, VillageChoice family)
        {
            if (chapter < ClassicChapters || chapter >= GardenCatalog.ChaptersPerGarden) return Classic(family);
            return Cycle[(chapter - ClassicChapters) % Cycle.Length];
        }

        public static PuzzleKind ForNursery(int kind)
        {
            if (kind < 0 || kind >= Nursery.Kinds) return PuzzleKind.Water;
            int edition = Nursery.Edition(kind);
            VillageChoice family = Nursery.Family(kind);
            return edition == 0 ? Classic(family) : ByEdition[(int)family][edition - 1];
        }
    }
}
