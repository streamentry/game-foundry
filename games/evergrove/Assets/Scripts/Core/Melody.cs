using System;

namespace Evergrove.Core
{
    /// <summary>
    /// I305 (idea 4, after music puzzle games and the tune under falling blocks): every move
    /// plays the next note of a short phrase, a phrase per kind of puzzle. Notes are in the
    /// major pentatonic scale, so any run of moves sounds in tune, and steps stay within a
    /// fifth. A solve comes home to the tonic. Pure: semitones from middle C, and their pitch.
    /// </summary>
    public static class Melody
    {
        /// <summary>The major pentatonic over an octave and a half, in semitones above middle C.</summary>
        public static readonly int[] Scale = { 0, 2, 4, 7, 9, 12, 14, 16, 19 };

        public const int PhraseLength = 8;

        // Each phrase as positions in Scale, in PuzzleKind order (Water, Pairs, Mosaic,
        // Nonogram, Mahjong, Sudoku, Plots, StreamPath, Sowing, Sums). Wrapping stays within a fifth.
        private static readonly int[][] Phrases =
        {
            new[] { 0, 1, 2, 3, 4, 3, 2, 1 },
            new[] { 3, 4, 5, 4, 3, 2, 1, 2 },
            new[] { 2, 3, 2, 1, 2, 3, 4, 3 },
            new[] { 5, 4, 3, 4, 5, 6, 5, 4 },
            new[] { 1, 3, 2, 4, 3, 5, 4, 3 },
            new[] { 4, 5, 6, 5, 4, 3, 4, 5 },
            new[] { 0, 2, 1, 3, 2, 4, 3, 1 },
            new[] { 3, 2, 3, 4, 5, 4, 3, 2 },
            new[] { 0, 1, 2, 1, 0, 1, 2, 3 }, // Sowing (I318)
            new[] { 4, 3, 4, 5, 6, 5, 4, 3 }, // Sums (I324)
            new[] { 2, 4, 3, 5, 4, 3, 2, 3 }, // Pieces (I331)
            new[] { 5, 6, 7, 6, 5, 4, 5, 6 }  // Doorstep (I332)
        };

        /// <summary>The note (semitones above middle C) for a kind's nth move.</summary>
        public static int NoteAt(PuzzleKind kind, int move)
        {
            int[] phrase = Phrases[Math.Max(0, Math.Min(Phrases.Length - 1, (int)kind))];
            int at = ((move % PhraseLength) + PhraseLength) % PhraseLength;
            return Scale[phrase[at]];
        }

        /// <summary>The note a solve ends on: the tonic, an octave up.</summary>
        public static int Home(PuzzleKind kind) { return Scale[5]; }

        /// <summary>Equal-tempered pitch of a note, from middle C (261.63 Hz).</summary>
        public static double Hertz(int semitones) { return 261.6256 * Math.Pow(2.0, semitones / 12.0); }
    }
}
