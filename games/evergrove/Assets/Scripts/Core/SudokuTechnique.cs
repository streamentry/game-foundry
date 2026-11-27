using System;

namespace Evergrove.Core
{
    /// <summary>
    /// Names the Seed Sudoku technique behind a placement (v1 #36), so the almanac can
    /// describe a move the first time a player makes it. Only the two singles are named:
    /// a naked single (the cell has one plant left) and a hidden single (the plant has
    /// only one place left in some row, column or box). Both use the same rules as
    /// SeedSudokuGame: every row, column and box holds each plant once. A placement that
    /// is neither is a guess or needs a harder technique, and names nothing.
    /// </summary>
    public static class SudokuTechnique
    {
        /// <summary>The technique a placement uses. Simpler techniques are reported first.</summary>
        public enum Technique { None = 0, NakedSingle = 1, HiddenSingle = 2 }

        // Units a hidden single is checked against: row, column, box.
        private const int UnitRow = 0, UnitColumn = 1, UnitBox = 2, UnitCount = 3;

        /// <summary>Classify a placement on the game's current board (see Classify with explicit board shape).</summary>
        public static Technique Classify(SeedSudokuGame game, int cell, int value)
        {
            if (game == null) return Technique.None;
            var values = new int[game.Cells];
            for (int i = 0; i < values.Length; i++) values[i] = game.Value(i);
            return Classify(game.Size, game.BoxRows, game.BoxColumns, values, cell, value);
        }

        /// <summary>
        /// Classify placing <paramref name="value"/> in the empty <paramref name="cell"/> of a board
        /// given as the player sees it before the placement (0 = empty, cell index = row * size + column).
        /// NakedSingle when the value is the only plant the cell can still take; otherwise HiddenSingle
        /// when the value has no other place in at least one of the cell's row, column or box; otherwise
        /// None (a guess, a plant that already repeats in a unit, or a harder technique).
        /// </summary>
        public static Technique Classify(int size, int boxRows, int boxColumns, int[] values, int cell, int value)
        {
            if (values == null || size <= 0 || values.Length != size * size) return Technique.None;
            if (cell < 0 || cell >= values.Length || values[cell] != 0) return Technique.None;
            if (value < 1 || value > size) return Technique.None;
            if (!CanHold(size, boxRows, boxColumns, values, cell, value)) return Technique.None;
            if (OnlyCandidate(size, boxRows, boxColumns, values, cell, value)) return Technique.NakedSingle;
            if (OnlyPlaceInAUnit(size, boxRows, boxColumns, values, cell, value)) return Technique.HiddenSingle;
            return Technique.None;
        }

        /// <summary>Plain garden words for the almanac. Empty for None.</summary>
        public static string Name(Technique technique)
        {
            switch (technique)
            {
                case Technique.NakedSingle: return "One plant left";
                case Technique.HiddenSingle: return "Only place for it";
                default: return "";
            }
        }

        // True when no filled cell sharing a unit with this empty cell already holds the value.
        private static bool CanHold(int size, int boxRows, int boxColumns, int[] values, int cell, int value)
        {
            for (int other = 0; other < values.Length; other++)
                if (values[other] == value && SharesUnit(size, boxRows, boxColumns, cell, other)) return false;
            return true;
        }

        // The value is the only plant this cell can still take.
        private static bool OnlyCandidate(int size, int boxRows, int boxColumns, int[] values, int cell, int value)
        {
            for (int v = 1; v <= size; v++)
                if (v != value && CanHold(size, boxRows, boxColumns, values, cell, v)) return false;
            return true;
        }

        // The value has no other possible place in at least one unit the cell belongs to.
        private static bool OnlyPlaceInAUnit(int size, int boxRows, int boxColumns, int[] values, int cell, int value)
        {
            for (int unit = 0; unit < UnitCount; unit++)
            {
                bool onlyHere = true;
                for (int other = 0; other < values.Length && onlyHere; other++)
                {
                    if (other == cell || values[other] != 0) continue;
                    if (!InUnit(size, boxRows, boxColumns, unit, cell, other)) continue;
                    if (CanHold(size, boxRows, boxColumns, values, other, value)) onlyHere = false;
                }
                if (onlyHere) return true;
            }
            return false;
        }

        private static bool SharesUnit(int size, int boxRows, int boxColumns, int a, int b)
        {
            return InUnit(size, boxRows, boxColumns, UnitRow, a, b)
                || InUnit(size, boxRows, boxColumns, UnitColumn, a, b)
                || InUnit(size, boxRows, boxColumns, UnitBox, a, b);
        }

        private static bool InUnit(int size, int boxRows, int boxColumns, int unit, int a, int b)
        {
            int ar = a / size, ac = a % size, br = b / size, bc = b % size;
            switch (unit)
            {
                case UnitRow: return ar == br;
                case UnitColumn: return ac == bc;
                default: return ar / boxRows == br / boxRows && ac / boxColumns == bc / boxColumns;
            }
        }
    }
}
