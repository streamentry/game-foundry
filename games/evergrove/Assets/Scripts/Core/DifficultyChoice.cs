using System;

namespace Evergrove.Core
{
    /// <summary>
    /// Item 34: the player chooses a board's leaves (1 gentlest to 3 deepest) for each puzzle kind,
    /// and the choice is remembered per kind on this device. Level n is generator tier n-1 for every
    /// kind, so level 1 is the gentlest board each kind has and level 3 the deepest.
    /// Kinds that measure their own logic (I336: nonogram, seed sudoku, seed sums, doorstep) must show
    /// a board whose measured leaves equal the chosen level, so DealAt searches seeds from the given
    /// one for the first board that measures that level. The other kinds have no logic measure, so
    /// they only offer size (their labels say size, not logic).
    /// </summary>
    public static class DifficultyChoice
    {
        /// <summary>Levels a player can choose: one per generator tier.</summary>
        public const int Levels = TreePuzzles.Tiers;

        /// <summary>The gentlest level, which every kind has.</summary>
        public const int Gentlest = 1;

        // Seeds tried after the given one when a logic kind must measure the chosen leaves. Every
        // (kind, level) pair is reached within a few seeds in the tests (50 seeds, all kinds).
        private const int SearchLimit = 200;

        /// <summary>True when the kind measures its own logic (leaves), not only its size.</summary>
        public static bool MeasuresLogic(PuzzleKind kind)
        {
            return kind == PuzzleKind.Nonogram || kind == PuzzleKind.Sudoku
                || kind == PuzzleKind.Sums || kind == PuzzleKind.Doorstep;
        }

        /// <summary>A level clamped to 1..Levels.</summary>
        public static int Clamp(int level)
        {
            return Math.Max(Gentlest, Math.Min(Levels, level));
        }

        /// <summary>The generator tier a chosen level deals: level n is tier n-1, for every kind.</summary>
        public static int TierFor(PuzzleKind kind, int level)
        {
            return Clamp(level) - 1;
        }

        /// <summary>The level a tier is shown as (the inverse of TierFor).</summary>
        public static int LevelOfTier(int tier)
        {
            return Math.Max(0, Math.Min(TreePuzzles.Tiers - 1, tier)) + 1;
        }

        /// <summary>The gentle tier every kind has (tier 0, which TierFor(kind, Gentlest) returns).</summary>
        public static int GentleTier(PuzzleKind kind)
        {
            return TierFor(kind, Gentlest);
        }

        /// <summary>Measured leaves of a logic kind's board, or 0 for a kind that measures only size.</summary>
        public static int LeavesOf(PuzzleKind kind, int seed, int tier)
        {
            switch (kind)
            {
                case PuzzleKind.Nonogram: return new NonogramGame(seed, tier).Leaves;
                case PuzzleKind.Sudoku: return new SeedSudokuGame(seed, tier).Leaves;
                case PuzzleKind.Sums: return new SeedSumsGame(seed, tier).Leaves;
                case PuzzleKind.Doorstep: return new DoorstepGame(seed, tier).Leaves;
                default: return 0;
            }
        }

        /// <summary>The raw logic measure behind the leaves (rounds, sweeps, passes, decoy %), or 0.</summary>
        public static int RawOf(PuzzleKind kind, int seed, int tier)
        {
            switch (kind)
            {
                case PuzzleKind.Nonogram: return new NonogramGame(seed, tier).LineSweeps;
                case PuzzleKind.Sudoku: return new SeedSudokuGame(seed, tier).SolveRounds;
                case PuzzleKind.Sums: return new SeedSumsGame(seed, tier).DeductionPasses;
                case PuzzleKind.Doorstep: return new DoorstepGame(seed, tier).DecoyPercent;
                default: return 0;
            }
        }

        /// <summary>
        /// The board's size measure, for every kind: cells, tiles, pieces, cards or seeds, whichever
        /// the kind counts. Larger means more to do, so a gentler board is never larger.
        /// </summary>
        public static int SizeOf(PuzzleKind kind, int seed, int tier)
        {
            switch (kind)
            {
                case PuzzleKind.Water: return TreePuzzles.For(seed, tier).CellCount;
                case PuzzleKind.Pairs: return new PairsGame(seed, tier).Count;
                case PuzzleKind.Mosaic: return new MosaicGame(seed, tier).Count;
                case PuzzleKind.Nonogram: { int size = new NonogramGame(seed, tier).Size; return size * size; }
                case PuzzleKind.Mahjong: return new MahjongGame(seed, tier).TileCount;
                case PuzzleKind.Sudoku: return new SeedSudokuGame(seed, tier).Cells;
                case PuzzleKind.Plots: { int size = new PlotsGame(seed, tier).Size; return size * size; }
                case PuzzleKind.StreamPath: { int size = new StreamPathGame(seed, tier).Size; return size * size; }
                case PuzzleKind.Sowing: return new SowingGame(seed, tier).Total;
                case PuzzleKind.Sums: { var sums = new SeedSumsGame(seed, tier); return sums.Width * sums.Height; }
                case PuzzleKind.Pieces: return new GardenPiecesGame(seed, tier).CellCount;
                case PuzzleKind.Doorstep: { int size = new DoorstepGame(seed, tier).Size; return size * size; }
                default: throw new ArgumentOutOfRangeException(nameof(kind));
            }
        }

        /// <summary>A board dealt for a chosen level: its seed, tier, and the level it was dealt at.</summary>
        public readonly struct Deal
        {
            public readonly int Seed;
            public readonly int Tier;
            public readonly int Level;

            public Deal(int seed, int tier, int level)
            {
                Seed = seed;
                Tier = tier;
                Level = level;
            }
        }

        /// <summary>
        /// The board for a chosen level, from the given seed. Size-only kinds keep the seed and take
        /// the level's tier. Logic kinds take the first seed (from the given one) whose measured leaves
        /// equal the level at that tier, so the board's own leaves show the choice. If none is found
        /// within the search, the board keeps its own leaves, which the UI shows honestly.
        /// </summary>
        public static Deal DealAt(PuzzleKind kind, int seed, int level)
        {
            int chosen = Clamp(level);
            int tier = TierFor(kind, chosen);
            if (!MeasuresLogic(kind)) return new Deal(seed, tier, chosen);
            for (int offset = 0; offset < SearchLimit; offset++)
            {
                int candidate = unchecked(seed + offset);
                if (LeavesOf(kind, candidate, tier) == chosen) return new Deal(candidate, tier, chosen);
            }
            return new Deal(seed, tier, chosen);
        }
    }
}
