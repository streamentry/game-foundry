using System;
using System.Globalization;

namespace Evergrove.Core
{
    /// <summary>
    /// v1 #35: a weekly deeper board. Each ISO week a garden offers one harder board per
    /// favourite kind, dealt from the garden, the kind and the week. There is no timer and no
    /// penalty: skipping costs nothing and nothing expires. The week's board is simply replaced
    /// by next week's. Pure rules; the UI hook comes later.
    /// </summary>
    public static class WeeklyBoard
    {
        /// <summary>Every weekly board is dealt at the deepest tier.</summary>
        public const int Tier = TreePuzzles.Tiers - 1;

        // A salt that keeps weekly seeds apart from ReplayBoards' linear replay seeds.
        private const uint Salt = 0x7F4A7C15u;

        /// <summary>A stable week key for an ISO year and ISO week: year * 100 + week.</summary>
        public static int Week(int year, int isoWeek)
        {
            if (isoWeek < 1 || isoWeek > 53) throw new ArgumentOutOfRangeException(nameof(isoWeek));
            return year * 100 + isoWeek;
        }

        /// <summary>
        /// The week key of a date given as yyyymmdd, using the ISO 8601 week-numbering year
        /// (so 2026-12-31 is 2026 week 53 and 2027-01-04 is 2027 week 1).
        /// </summary>
        public static int IsoWeekOf(int yyyymmdd)
        {
            var date = new DateTime(yyyymmdd / 10000, yyyymmdd / 100 % 100, yyyymmdd % 100);
            return Week(ISOWeek.GetYear(date), ISOWeek.GetWeekOfYear(date));
        }

        /// <summary>
        /// The seed for a garden's weekly board of a kind in a week. Well mixed, so adjacent weeks
        /// and neighbouring gardens deal unrelated boards, and salted so it never repeats a
        /// ReplayBoards seed for the same garden.
        /// </summary>
        public static int SeedFor(int garden, PuzzleKind kind, int weekKey)
        {
            unchecked
            {
                uint h = Mix(Salt ^ (uint)garden);
                h = Mix(h ^ ((uint)kind + 1u) * 0x9E3779B1u);
                h = Mix(h ^ (uint)weekKey * 0x85EBCA6Bu);
                return (int)h;
            }
        }

        /// <summary>
        /// Whether a kind offers a weekly board. True only where the generator proves the
        /// top-tier board has exactly one solution (test: WeeklyBoardTests).
        /// Included: Water (TreePuzzles checks PipeRules.CountSolutions), Sudoku (givens counted
        /// unique), Plots (field counted unique), StreamPath (meadow counted unique, within budget),
        /// Sums (the generator's deduction settles every cell, which implies one solution),
        /// Doorstep (one given per symmetry class fixes every answer).
        /// Excluded, and why: Pairs (a shuffled memory match, nothing to deduce); Mosaic (a shuffle
        /// with a fixed target, no uniqueness check); Mahjong (many valid removal orders, no unique
        /// solution); Nonogram (picture library, clues not checked for uniqueness); Sowing (a deal
        /// fixed by the seed count alone, no uniqueness check, only seven boards per tier);
        /// Pieces (a tiling grown from pieces, tilings never counted).
        /// </summary>
        public static bool HasWeekly(PuzzleKind kind)
        {
            switch (kind)
            {
                case PuzzleKind.Water:
                case PuzzleKind.Sudoku:
                case PuzzleKind.Sums:
                case PuzzleKind.Plots:
                case PuzzleKind.StreamPath:
                case PuzzleKind.Doorstep:
                    return true;
                default:
                    return false;
            }
        }

        private static uint Mix(uint x)
        {
            unchecked
            {
                x ^= x >> 16;
                x *= 0x7FEB352Du;
                x ^= x >> 15;
                x *= 0x846CA68Bu;
                x ^= x >> 16;
                return x;
            }
        }
    }
}
