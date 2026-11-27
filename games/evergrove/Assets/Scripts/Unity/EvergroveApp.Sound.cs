using System;
using System.Collections;
using System.Globalization;
using Evergrove.Core;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem.UI;
#endif
namespace Evergrove.UnityClient
{
    public sealed partial class EvergroveApp
    {

        // ---- I21: procedural creek ambience and a wooden pipe "tock" (sound on only).
        private AudioSource creek;
        private AudioClip creekLoop;
        private AudioClip tock;

        private void PlayTock()
        {
            if (!soundOn) return;
            if (tock == null)
            {
                const int sampleRate = 22050;
                const int length = 2400;
                var samples = new float[length];
                var noise = new System.Random(7);
                for (int i = 0; i < length; i++)
                {
                    float t = (float)i / sampleRate;
                    float envelope = Mathf.Exp(-t * 55f);
                    float body = Mathf.Sin(t * 2f * Mathf.PI * 330f) + 0.4f * Mathf.Sin(t * 2f * Mathf.PI * 495f);
                    float click = (float)(noise.NextDouble() * 2.0 - 1.0) * Mathf.Exp(-t * 400f);
                    samples[i] = envelope * 0.16f * body + 0.05f * click;
                }
                tock = AudioClip.Create("Mosselora wooden tock", length, 1, sampleRate, false);
                tock.SetData(samples, 0);
            }
            sound.PlayOneShot(tock, 0.8f);
        }

        // A soft, seamless creek murmur; loudness follows the real water reach.
        private void UpdateCreekSound()
        {
            if (!soundOn || world == null || world.Completed)
            {
                if (creek != null) creek.Stop();
                return;
            }
            if (creek == null)
            {
                creek = gameObject.AddComponent<AudioSource>();
                creek.loop = true;
                creek.playOnAwake = false;
                creek.volume = 0f;
            }
            if (creekLoop == null)
            {
                const int sampleRate = 22050;
                const int length = sampleRate * 4;
                var samples = new float[length];
                var noise = new System.Random(11);
                float brown = 0f, smooth = 0f;
                for (int i = 0; i < length; i++)
                {
                    brown = Mathf.Clamp(brown + (float)(noise.NextDouble() * 2.0 - 1.0) * 0.06f, -1f, 1f);
                    smooth += (brown - smooth) * 0.08f; // low-pass: water, not hiss
                    float t = (float)i / length;
                    float burble = 0.75f + 0.25f * Mathf.Sin(t * 2f * Mathf.PI * 3f) * Mathf.Sin(t * 2f * Mathf.PI * 7f);
                    samples[i] = smooth * burble;
                }
                // Crossfade the tail into the head so the loop has no seam.
                int fade = sampleRate / 4;
                for (int i = 0; i < fade; i++)
                {
                    float w = (float)i / fade;
                    samples[i] = samples[i] * w + samples[length - fade + i] * (1f - w);
                }
                var looped = new float[length - fade];
                System.Array.Copy(samples, looped, looped.Length);
                creekLoop = AudioClip.Create("Mosselora creek", looped.Length, 1, sampleRate, false);
                creekLoop.SetData(looped, 0);
            }
            if (creek.clip != creekLoop) creek.clip = creekLoop;
            float reach = world.Solved ? 1f : VillageScenePlan.From(world).WaterReachPercent / 100f;
            creek.volume = reach <= 0f ? 0f : 0.05f + 0.20f * reach;
            if (!creek.isPlaying) creek.Play();
        }

        // I316: now and then, over home, a voice of the season and the hour.
        private float nextAmbient;
        private bool owlInGrove; // I376
        private readonly System.Random ambientDice = new System.Random();

        private void TickSoundscape()
        {
            if (!soundOn || sound == null || titleScreen == null || world == null || world.Mode != PlayMode.Living) return;
            if (familySheet != null || openCard != null) return; // quiet while a card is open
            float now = Time.unscaledTime;
            if (now < nextAmbient) return;
            Now(out int hour, out int minute);
            DayPhase phase = DayLightOn ? DayLight.PhaseAt(hour, minute) : DayPhase.Day;
            AmbientVoice[] voices = Soundscape.For(GardenSeason.For(world), phase);
            AmbientVoice voice = voices[ambientDice.Next(voices.Length)];
            AmbientVoice? weatherVoice = Soundscape.WeatherVoice(TodayWeather(world), phase); // I354–I356
            if (weatherVoice.HasValue && ambientDice.NextDouble() < 0.6) voice = weatherVoice.Value;
            // I376: an owl a grove has brought calls now and then in the night.
            AmbientVoice? owl = owlInGrove && Visitors.Welcomes(world, Visitor.Owl)
                ? Soundscape.VisitorVoice(Visitor.Owl, GardenSeason.For(world), phase) : null;
            if (owl.HasValue && ambientDice.NextDouble() < 0.3) voice = owl.Value;
            // I426: bees heard by day when they've come and are out in this weather.
            if (Visitors.Welcomes(world, Visitor.Bees) && Visitors.OutIn(Visitor.Bees, GardenSeason.For(world), phase, TodayWeather(world))
                && ambientDice.NextDouble() < 0.25) voice = AmbientVoice.Bees;
            if (nextAmbient > 0f) sound.PlayOneShot(GardenSounds.Ambient(voice, ambientDice.Next(3)), 0.55f);
            nextAmbient = now + Soundscape.SecondsBetween(voice) * (0.6f + 0.8f * (float)ambientDice.NextDouble());
        }

        // I305/I306: a move on a thing's puzzle sounds in its kind's own voice, at the next
        // note of the kind's phrase.
        private int moveNote;

        private void PlayMove(PuzzleKind kind)
        {
            if (!soundOn) return;
            sound.PlayOneShot(GardenSounds.Move(kind, moveNote++), 0.9f);
        }

        private void PlayChime()
        {
            if (!soundOn) return;
            if (chime == null)
            {
                const int sampleRate = 22050;
                const int length = 6200;
                var samples = new float[length];
                for (int i = 0; i < length; i++)
                {
                    float t = (float)i / sampleRate;
                    float envelope = Mathf.Pow(1f - (float)i / length, 2f);
                    samples[i] = envelope * 0.15f *
                        (Mathf.Sin(t * 2f * Mathf.PI * 523.25f) +
                         0.55f * Mathf.Sin(t * 2f * Mathf.PI * 659.25f));
                }
                chime = AudioClip.Create("Evergrove gentle chime", length, 1, sampleRate, false);
                chime.SetData(samples, 0);
            }
            sound.PlayOneShot(chime);
        }
    }
}
