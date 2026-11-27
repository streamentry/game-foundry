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
            float seconds = kind == PuzzleKind.Mahjong || kind == PuzzleKind.Pairs || kind == PuzzleKind.Sums ? 0.16f : 0.26f;
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

        // ---- I316: the soundscape's voices, a few variants each, quiet and short.
        private static readonly Dictionary<int, AudioClip> AmbientCache = new Dictionary<int, AudioClip>();

        public static AudioClip Ambient(AmbientVoice voice, int variant)
        {
            int key = (int)voice * 10 + ((variant % 3) + 3) % 3;
            if (AmbientCache.TryGetValue(key, out AudioClip clip) && clip != null) return clip;
            var rng = new System.Random(101 + key);
            float seconds = voice == AmbientVoice.Rain ? 2.4f : voice == AmbientVoice.Wind ? 2.6f : voice == AmbientVoice.Crickets ? 0.5f
                : voice == AmbientVoice.Birds ? 0.6f : 0.18f;
            int length = Mathf.RoundToInt(SampleRate * seconds);
            var samples = new float[length];
            float brown = 0f, smooth = 0f, patter = 0f;
            float pitch = 2800f + (float)rng.NextDouble() * 1400f;
            int chirps = 2 + rng.Next(3);
            for (int i = 0; i < length; i++)
            {
                float t = (float)i / SampleRate;
                float white = (float)(rng.NextDouble() * 2.0 - 1.0);
                switch (voice)
                {
                    case AmbientVoice.Birds:
                    {
                        // Two to four quick rising chirps.
                        float slot = 0.14f, local = t % slot;
                        int n = (int)(t / slot);
                        float on = n < chirps && local < 0.07f ? Mathf.Sin(local / 0.07f * Mathf.PI) : 0f;
                        float hz = pitch * (1f + 0.35f * local / 0.07f) * (1f + 0.04f * n);
                        samples[i] = on * 0.05f * Sine(hz, t);
                        break;
                    }
                    case AmbientVoice.Crickets:
                    {
                        float pulse = Mathf.Max(0f, Mathf.Sin(t * 2f * Mathf.PI * 28f));
                        float swell = Mathf.Sin(t / seconds * Mathf.PI);
                        samples[i] = 0.03f * pulse * pulse * swell * Sine(4600f, t);
                        break;
                    }
                    case AmbientVoice.Rain:
                    {
                        // I354: soft patter, many tiny drops under a hush.
                        brown = Mathf.Clamp(brown + white * 0.04f, -1f, 1f);
                        smooth += (brown - smooth) * 0.2f;
                        patter = rng.NextDouble() < 0.0035 ? 1f : patter * 0.9965f; // each drop, then its decay
                        float swell = Mathf.Sin(t / seconds * Mathf.PI);
                        samples[i] = swell * (0.05f * smooth + 0.05f * white * patter);
                        break;
                    }
                    case AmbientVoice.Wind:
                    {
                        brown = Mathf.Clamp(brown + white * 0.05f, -1f, 1f);
                        smooth += (brown - smooth) * 0.05f;
                        float swell = Mathf.Sin(t / seconds * Mathf.PI);
                        samples[i] = 0.22f * smooth * swell * swell;
                        break;
                    }
                    default:
                    {
                        // A single soft drop into the stream.
                        float hz = 620f + 420f * Mathf.Min(1f, t * 9f);
                        samples[i] = Mathf.Exp(-t * 28f) * 0.06f * Sine(hz, t);
                        break;
                    }
                }
            }
            for (int i = 0; i < 128 && i < length; i++)
            {
                samples[i] *= i / 128f;
                samples[length - 1 - i] *= i / 128f;
            }
            clip = AudioClip.Create("Mosselora " + voice + " " + key, length, 1, SampleRate, false);
            clip.SetData(samples, 0);
            AmbientCache[key] = clip;
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
                case PuzzleKind.Sowing: // a seed dropped into soft earth: a short, round pluck
                    return Mathf.Exp(-t * 30f) * 0.13f * (Sine(hz, t) + 0.15f * Sine(hz * 3f, t)) + 0.03f * white * Mathf.Exp(-t * 300f);
                case PuzzleKind.Sums: // a bead slid on a counting frame
                    return Mathf.Exp(-t * 34f) * 0.11f * (Sine(hz, t) + 0.35f * Sine(hz * 2.01f, t)) + 0.05f * white * Mathf.Exp(-t * 500f);
                case PuzzleKind.Pieces: // a wooden piece set into place
                    return Mathf.Exp(-t * 38f) * 0.12f * (Sine(hz, t) + 0.3f * Sine(hz * 2.5f, t)) + 0.06f * white * Mathf.Exp(-t * 350f);
                case PuzzleKind.Doorstep: // a pinch of rice flour, soft and bright
                    return Mathf.Exp(-t * 20f) * 0.09f * (Sine(hz, t) + 0.4f * Sine(hz * 2f, t)) + 0.02f * white * Mathf.Exp(-t * 80f);
                case PuzzleKind.Plots: // a bed raked: a low, warm note
                    return Mathf.Exp(-t * 16f) * 0.14f * Sine(hz * 0.5f, t) + 0.02f * white * Mathf.Exp(-t * 60f);
                default: // StreamPath: a drop of water, its pitch rising as it falls in
                    return Mathf.Exp(-t * 20f) * 0.12f * Sine(hz * (1f + 0.3f * Mathf.Min(1f, t * 12f)), t);
            }
        }
    }
}
