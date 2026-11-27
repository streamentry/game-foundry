using System.Collections.Generic;
using Evergrove.Core;
using UnityEngine;

namespace Evergrove.UnityClient
{
    /// <summary>
    /// I305/I306: the sound of a move. Each kind of puzzle has its own touch (idea 47, after
    /// the "every touch answers" craft of Japanese studios): a tile's clack, a card's flip, a
    /// drop of water. Its pitch is the next note of the kind's phrase (idea 4, Melody), so a
    /// solve is also a small tune. Synthesised once per note and kept; quiet by design.
    /// </summary>
    internal static class GardenSounds
    {
        private const int SampleRate = 22050;
        private static readonly Dictionary<int, AudioClip> Cache = new Dictionary<int, AudioClip>();

        public static AudioClip Move(PuzzleKind kind, int move)
        {
            return Note(kind, Melody.NoteAt(kind, move));
        }

        public static AudioClip Note(PuzzleKind kind, int note)
        {
            int key = (int)kind * 100 + note;
            if (Cache.TryGetValue(key, out AudioClip clip) && clip != null) return clip;
            float hz = (float)Melody.Hertz(note);
            float seconds = kind == PuzzleKind.Mahjong || kind == PuzzleKind.Pairs ? 0.16f : 0.26f;
            int length = Mathf.RoundToInt(SampleRate * seconds);
            var samples = new float[length];
            var noise = new System.Random(17 + key);
            for (int i = 0; i < length; i++)
            {
                float t = (float)i / SampleRate;
                samples[i] = Sample(kind, hz, t, (float)(noise.NextDouble() * 2.0 - 1.0));
            }
            // A few samples of fade at each end: no clicks.
            for (int i = 0; i < 64 && i < length; i++)
            {
                samples[i] *= i / 64f;
                samples[length - 1 - i] *= i / 64f;
            }
            clip = AudioClip.Create("Mosselora " + kind + " " + note, length, 1, SampleRate, false);
            clip.SetData(samples, 0);
            Cache[key] = clip;
            return clip;
        }

        private static float Sine(float hz, float t) { return Mathf.Sin(t * 2f * Mathf.PI * hz); }

        private static float Sample(PuzzleKind kind, float hz, float t, float white)
        {
            switch (kind)
            {
                case PuzzleKind.Water: // the wooden pipe tock, now in tune
                    return Mathf.Exp(-t * 40f) * 0.15f * (Sine(hz, t) + 0.4f * Sine(hz * 1.5f, t)) + 0.04f * white * Mathf.Exp(-t * 400f);
                case PuzzleKind.Pairs: // a card turned: a soft brush, then the note
                    return 0.06f * white * Mathf.Exp(-t * 90f) + Mathf.Exp(-t * 30f) * 0.11f * Sine(hz, t);
                case PuzzleKind.Mosaic: // a glass tile set down
                    return Mathf.Exp(-t * 18f) * 0.10f * (Sine(hz, t) + 0.3f * Sine(hz * 3f, t));
                case PuzzleKind.Nonogram: // a seed pressed into soil: a soft pluck
                    return Mathf.Exp(-t * 22f) * 0.13f * (Sine(hz, t) + 0.25f * Sine(hz * 2f, t));
                case PuzzleKind.Mahjong: // a tile's clack, then its ring
                    return 0.09f * white * Mathf.Exp(-t * 260f) + Mathf.Exp(-t * 45f) * 0.10f * Sine(hz, t);
                case PuzzleKind.Sudoku: // a little wooden marimba
                    return Mathf.Exp(-t * 26f) * 0.12f * (Sine(hz, t) + 0.2f * Sine(hz * 4f, t));
                case PuzzleKind.Plots: // a bed raked: a low, warm note
                    return Mathf.Exp(-t * 16f) * 0.14f * Sine(hz * 0.5f, t) + 0.02f * white * Mathf.Exp(-t * 60f);
                default: // StreamPath: a drop of water, its pitch rising as it falls in
                    return Mathf.Exp(-t * 20f) * 0.12f * Sine(hz * (1f + 0.3f * Mathf.Min(1f, t * 12f)), t);
            }
        }
    }
}
