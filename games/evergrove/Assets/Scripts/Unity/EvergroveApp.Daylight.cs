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

        // Leaving or returning (I275): the one garden note is queued on the way out and
        // cleared on the way back in.
        private bool waveHello = true; // I298: set on launch and on every return

        private sealed class HelloHop : MonoBehaviour
        {
            public float Delay;
            private float clock;
            private Vector2 home;
            private bool started;

            private void Update()
            {
                var rect = (RectTransform)transform;
                if (!started) { home = rect.anchoredPosition; started = true; }
                clock += Time.unscaledDeltaTime;
                float t = clock - Delay;
                if (t < 0f) return;
                const float hop = 0.42f;
                if (t >= hop * 2f) { rect.anchoredPosition = home; rect.localRotation = Quaternion.identity; enabled = false; return; }
                float k = (t % hop) / hop;
                rect.anchoredPosition = home + new Vector2(0f, Mathf.Sin(k * Mathf.PI) * 9f);
                rect.localRotation = Quaternion.Euler(0f, 0f, (t < hop ? 1f : -1f) * Mathf.Sin(k * Mathf.PI) * 4f);
            }
        }

        private void OnApplicationPause(bool paused)
        {
            if (!paused) { waveHello = true; spokenThisVisit.Clear(); spokenVariant.Clear(); } // SC02: a new visit
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            ResearchPause(paused);
#endif
            NotesLifecycle(paused);
        }

        private void OnApplicationQuit() { NotesLifecycle(true); }

        private void NotesLifecycle(bool leaving)
        {
            if (world == null) return;
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            if (qaInMemoryOnly)
            {
                // The tour never schedules a real note; it logs what would be said.
                GardenNote qa = GardenNote.For(world, UnreadLetters());
                Debug.Log("MOSSELORA_GARDEN_NOTE_QA " + (qa == null ? "(nothing waiting)" : qa.From + ": " + qa.Body));
                return;
            }
#endif
            if (leaving) { GardenNotes.Schedule(world, UnreadLetters()); return; }
            noteCame = GardenNotes.Opened(); // shown once home is built (I283)
            if (noteCame != null && titleScreen != null) FollowNote(); // resumed onto home already built
        }

        private bool UnreadLetters() { return PlayerPrefs.GetInt(LettersReadKey, 0) < world.Letters.Length; }

        // I283: the note came while you were away; home shows what it pointed at, once.
        private GardenNote noteCame;

        private void FollowNote()
        {
            GardenNote note = noteCame;
            noteCame = null;
            if (note == null || titleScreen == null || world.Mode != PlayMode.Living) return;
            switch (note.Target)
            {
                case NoteTarget.Letterbox:
                    if (world.Letters.Length > 0) ShowLetter(0);
                    break;
                case NoteTarget.Basket:
                    if (world.Gifts.Length > 0) { basketOpen = true; giftPick = world.Gifts.Length - 1; RefreshMaps(); }
                    break;
                case NoteTarget.Thing:
                    // The thing it named is picked up: its puzzle is one tap away.
                    if (note.Thing >= BoughtIndex && note.Thing - BoughtIndex < world.BoughtCount) { heldBought = note.Thing - BoughtIndex; RefreshMaps(); }
                    else if (note.Thing >= 0 && note.Thing < world.Choices.Length) { mapInHand = note.Thing; RefreshMaps(); }
                    break;
                case NoteTarget.River:
                    if (hubMapView != null) hubMapView.KeepAt(new Vector2(0.5f, 0f)); // the mist and its drops
                    break;
            }
        }

        // ---- I278-I281: light that follows the player's day. Cosmetic only, never saved.
        private const string DayLightKey = "evergrove:daylight";
        private static bool DayLightOn { get { return PlayerPrefs.GetInt(DayLightKey, 1) == 1; } }
        private int qaClockHour = -1; // the tour's fixed clock (I282)
        private int qaDayOffset; // SC02: the tour's scripted week moves the companion's day; 0 outside the tour

        // The companion's local day (SC02): today, or the tour's scripted day.
        private DateTime CompanionToday() { return DateTime.Now.Date.AddDays(qaDayOffset); }

        // CP42: the companion's day turns at 4 a.m. The clock keeps the time; the tour's whole-day offset still moves it one day per scripted day.
        private DateTime CompanionClock() { return DateTime.Now.AddDays(qaDayOffset); }
        private int CompanionDayNow() { return CompanionContext.CompanionDayOf(CompanionClock()); }

        private void Now(out int hour, out int minute)
        {
            DateTime now = DateTime.Now;
            hour = qaClockHour >= 0 ? qaClockHour : now.Hour;
            minute = qaClockHour >= 0 ? 0 : now.Minute;
        }

        private DayPhase CurrentPhase() { Now(out int hour, out int minute); return DayLight.PhaseAt(hour, minute); }

        // I353: the day's weather for a living garden, with the light on; the tour may set it.
        private int qaWeather = -1;
        private bool qaRainbow; // I364 tour; read by shipping code, so declared outside the dev-only block
        private Weather TodayWeather(WorldModel garden)
        {
            if (!DayLightOn || garden == null || garden.Mode != PlayMode.Living) return Weather.Fair;
            if (qaWeather >= 0) return (Weather)qaWeather;
            return DayWeather.For(DateTime.Now.Date, GardenSeason.For(garden), garden.Garden);
        }

        // I404: a tapped visitor answers in its own voice (the sound switch still rules).
        private void OnVisitorTapped(int kind)
        {
            if (!soundOn || sound == null) return;
            if (kind == 2) sound.PlayOneShot(GardenSounds.Ambient(AmbientVoice.Owl, ambientDice.Next(3)), 0.7f);
            else if (kind == 0 || kind == 3) sound.PlayOneShot(GardenSounds.Ambient(AmbientVoice.Water, ambientDice.Next(3)), 0.8f);
        }

        private void ApplyDayLight(WorldModel garden)
        {
            GardenCreature.Tapped = OnVisitorTapped; // I404
            Now(out int quietHour, out int _);
            AudioListener.volume = Companion.Volume(quietHour); // I422: softer late at night
            Now(out int hour, out int minute);
            DayTint tint = DayLight.TintAt(hour, minute);
            bool on = DayLightOn && garden != null && garden.Mode == PlayMode.Living;
            GardenMapView.Tint = on ? new Color(tint.R, tint.G, tint.B, tint.A) : new Color(0f, 0f, 0f, 0f);
            GardenMapView.Dark = on ? tint.Dark : 0f;
            GardenMapView.Fireflies = on && DayLight.Fireflies(DayLight.PhaseAt(hour, minute), GardenSeason.For(garden));
            GardenMapView.FirefliesMove = gentleMotion;
            GardenMapView.LampLit = on && DayLight.LampLit(DayLight.PhaseAt(hour, minute)); // I296
            SunShadow.Phase = on ? DayLight.PhaseAt(hour, minute) : DayPhase.Night; // checklist 22: no sun shadows with the light off
            GardenMapView.FestivalNow = GardenSeason.Festival(garden); // I317
            // I325/I326: visitors come for logical reasons, and are out by season and hour.
            DayPhase visitPhase = DayLightOn ? DayLight.PhaseAt(hour, minute) : DayPhase.Day;
            Season visitSeason = GardenSeason.For(garden);
            Weather visitWeather = TodayWeather(garden); // I405: they keep the weather
            GardenMapView.BeesOut = Visitors.Welcomes(garden, Visitor.Bees) && Visitors.OutIn(Visitor.Bees, visitSeason, visitPhase, visitWeather);
            GardenMapView.BirdsOut = Visitors.Welcomes(garden, Visitor.Birds) && Visitors.Out(Visitor.Birds, visitSeason, visitPhase);
            GardenMapView.CatBy = GardenCat.NapBy(garden, visitPhase); // I335
            GardenMapView.SeasonNow = garden != null && garden.Mode == PlayMode.Living ? visitSeason : Season.Summer; // I338
            GardenMapView.WeatherNow = TodayWeather(garden); // I353
            GardenMapView.SignFont = font; // I360
            GardenMapView.RainbowNow = DayLightOn && garden != null && garden.Mode == PlayMode.Living && qaWeather < 0 &&
                DayWeather.Rainbow(DateTime.Now.Date, GardenSeason.For(garden), garden.Garden) &&
                (visitPhase == DayPhase.Morning || visitPhase == DayPhase.Day || visitPhase == DayPhase.Golden) || qaRainbow; // I364
            bool livingHere = garden != null && garden.Mode == PlayMode.Living;
            Season riverSeason = GardenSeason.For(garden);
            GardenMapView.RiverRainNow = livingHere && RiverMoments.RainRings(riverSeason, visitWeather); // I413
            GardenMapView.RiverGlitterNow = livingHere && DayLightOn && RiverMoments.Glitter(riverSeason, visitPhase, visitWeather); // I415
            GardenMapView.MoonlightNow = livingHere && DayLightOn && RiverMoments.Moonlight(visitPhase, visitWeather); // I425
            GardenMapView.RiverLeavesNow = livingHere && RiverMoments.LeavesAfloat(riverSeason); // I417
            GardenMapView.FishEveryNow = livingHere ? RiverMoments.FishEvery(riverSeason, DayLightOn ? visitPhase : DayPhase.Day) : 0f; // I416
            GardenMapView.WaterTapped = () => { if (soundOn && sound != null) sound.PlayOneShot(GardenSounds.Ambient(AmbientVoice.Water, ambientDice.Next(3)), 0.9f); };
            GardenMapView.MistNow = DayLightOn && garden != null && garden.Mode == PlayMode.Living && DayLight.Mist(visitPhase, visitWeather); // I409
            GardenMapView.SmokeNow = DayLightOn && garden != null && garden.Mode == PlayMode.Living &&
                DayLight.ChimneySmoke(visitPhase, GardenSeason.For(garden), visitWeather); // I410
            GardenMapView.FootprintsNow = garden != null && garden.Mode == PlayMode.Living && SeasonLook.SnowLying(GardenSeason.For(garden)) > 0f; // I412
            GardenMapView.DewNow = DayLightOn && garden != null && garden.Mode == PlayMode.Living
                ? DayLight.Dew(visitPhase, GardenSeason.For(garden)) : Dew.None; // I366
            // I295: the season's drift by day; it follows the season even with the light switch off.
            bool living = garden != null && garden.Mode == PlayMode.Living;
            // Founder 2026-11-28: with the weather shown, winter snow falls only on a snowy day.
            GardenMapView.DriftNow = !living ? Drift.None : DayLightOn
                ? GardenSeason.DriftOf(GardenSeason.For(garden), DayLight.PhaseAt(hour, minute), TodayWeather(garden))
                : GardenSeason.DriftOf(GardenSeason.For(garden), DayPhase.Day);
        }

        // A painted picture in a token keeps its colours (not tinted like a glyph).
        private static void PaintedIcon(Button token, float alpha)
        {
            Transform icon = token != null ? token.transform.Find("Icon") : null;
            if (icon != null) icon.GetComponent<Image>().color = new Color(1f, 1f, 1f, alpha);
        }

        // ---- I276: the soft ask, in the garden's own words, once there's a reason to want it.
        private bool notesAskedThisSession;
        private GameObject notesCard;

        private void MaybeAskNotes()
        {
            if (!GardenNotes.Supported || GardenNotes.On || notesAskedThisSession || titleScreen == null) return;
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            if (qaInMemoryOnly) return; // the tour shoots the ask on purpose (QaNotesAsk)
#endif
            if (world.Mode != PlayMode.Living || letterCard != null || familySheet != null || openCard != null) return;
            int asked = GardenNotes.Asked;
            bool reason = asked == 0 ? (world.Choices.Length >= 3 || PlayerPrefs.GetInt(LettersReadKey, 0) > 0)
                : asked == 1 && world.PuzzlesSolved >= 10;
            if (!reason) return;
            notesAskedThisSession = true;
            ShowNotesAsk();
        }

        private void ShowNotesAsk()
        {
            if (notesCard != null) Destroy(notesCard);
            notesCard = Node("A note from the garden?", titleScreen.transform);
            Fill(notesCard.GetComponent<RectTransform>());
            notesCard.AddComponent<Image>().color = new Color(0.08f, 0.14f, 0.11f, 0.38f);
            var paper = Node("Ask paper", notesCard.transform);
            var paperRect = paper.GetComponent<RectTransform>();
            paperRect.anchorMin = paperRect.anchorMax = new Vector2(0.5f, 0.5f);
            paperRect.sizeDelta = new Vector2(Mathf.Min(PageWidth() + 6f, 420f), 0f);
            var paperImage = paper.AddComponent<Image>();
            MosseloraArt.PaperTile(paperImage, 22);
            paperImage.color = new Color(0.99f, 0.97f, 0.9f, 1f);
            MosseloraArt.Lift(paperImage, 4f, 0.22f);
            var column = paper.AddComponent<VerticalLayoutGroup>();
            column.padding = new RectOffset(22, 22, 18, 18);
            column.spacing = 12;
            column.childControlWidth = column.childControlHeight = true;
            column.childForceExpandWidth = true;
            column.childForceExpandHeight = false;
            paper.AddComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            var envelope = Node("Envelope", paper.transform);
            var envelopeLayout = envelope.AddComponent<LayoutElement>();
            envelopeLayout.preferredHeight = envelopeLayout.minHeight = 64f;
            var envelopeImage = envelope.AddComponent<Image>();
            envelopeImage.sprite = MosseloraArt.Load("letter-sealed");
            envelopeImage.preserveAspect = true;
            envelopeImage.raycastTarget = false;
            Narrate(Label(paper.transform, "The ask",
                "Would you like a short note from the garden once a day, at a time you choose? Only when something's waiting.",
                19, 0, Ink, TextAnchor.UpperCenter), true);
            Transform times = CenteredRowIn(paper.transform, "Note times");
            times.GetComponent<HorizontalLayoutGroup>().spacing = 12;
            foreach (int hour in new[] { 9, 15, 19 })
            {
                int pick = hour;
                // Each hour stands under its sun in ink (b293: white-on-paper hours were unreadable).
                var hourColumn = Node("Hour " + hour, times);
                // Wide enough for "15:00" at 150% text, so each hour stays under its own sun (I287).
                var columnWidth = hourColumn.AddComponent<LayoutElement>();
                columnWidth.minWidth = columnWidth.preferredWidth = Mathf.Max(72f, 58f * TextScale.Percent / 100f);
                var stack = hourColumn.AddComponent<VerticalLayoutGroup>();
                stack.spacing = 4;
                stack.childAlignment = TextAnchor.UpperCenter;
                stack.childControlWidth = stack.childControlHeight = true;
                stack.childForceExpandWidth = stack.childForceExpandHeight = false;
                IconButton(hourColumn.transform, "Notes at " + hour + ":00", "Notes at " + hour + ":00", MosseloraArt.IconSun,
                    () => { GardenNotes.TurnOn(pick); CloseNotesAsk(); PlayChime(); }, 64, MosseloraTheme.ForestAction,
                    false, null, false, 0, 0, true);
                Text when = Label(hourColumn.transform, "Hour", hour + ":00", 17, 0, Ink, TextAnchor.MiddleCenter);
                when.horizontalOverflow = HorizontalWrapMode.Overflow;
                Emphasize(when);
            }
            IconButton(times, "Not now", "Not now", MosseloraArt.IconCancel,
                () => { GardenNotes.NotNow(); CloseNotesAsk(); }, 64, MosseloraTheme.QuietAction,
                false, null, false, 0, 0, true);
            semanticsDirty = true;
        }

        // ---- I299 (idea 36): the cottage summary. Pictures and counts, true ones only.
        private GameObject cottageCard;
    }
}
