using System;
using System.Collections.Generic;
using System.Globalization;
using Evergrove.Core;
using UnityEngine;
using UnityEngine.UI;

namespace Evergrove.UnityClient
{
    // Checklists 49 and 54: the night creatures of each stretch, the "who visits" log of them, and
    // the morning's dew board and the evening's lantern letter. Device-local state only (PlayerPrefs),
    // never the garden file, which travels to family.
    public sealed partial class EvergroveApp
    {
        // ---- Checklist 49: one free cell per open stretch, in the evening only (-1 where none is free).
        // The choice of cell is a pure rule (NightCreatures.Spots): mid-stretch, frogs by the water.
        private static int[] NightSpots(GardenMap map, List<PlacedThing> things, DayPhase phase)
        {
            var taken = new HashSet<int>();
            if (things != null) foreach (PlacedThing thing in things) taken.Add(thing.Cell);
            return NightCreatures.Spots(map, taken, phase);
        }

        // The "who visits" log: a night creature is logged the first evening it is out on this device.
        private static string NightSeenKey(WorldModel garden, Visitor visitor)
        {
            return "evergrove:seen:" + garden.Garden + ":" + visitor;
        }

        private void NoteNightsSeen(WorldModel garden, int[] spots)
        {
            if (garden == null || spots == null) return;
            bool moths = false, frogs = false;
            for (int reach = 0; reach < spots.Length; reach++)
                if (spots[reach] >= 0) { if (reach % 2 == 0) moths = true; else frogs = true; }
            if (moths) PlayerPrefs.SetInt(NightSeenKey(garden, Visitor.Moths), 1);
            if (frogs) PlayerPrefs.SetInt(NightSeenKey(garden, Visitor.Frogs), 1);
            if (moths || frogs) PlayerPrefs.Save();
        }

        // Who visits (I327): a visitor is in ink once it has come. Night creatures come once seen.
        private bool Logged(Visitor visitor)
        {
            if (!Visitors.Welcomes(world, visitor)) return false;
            return !NightCreatures.IsNightKind(visitor) || PlayerPrefs.GetInt(NightSeenKey(world, visitor), 0) == 1;
        }

        // ---- Checklist 54: a dew board in the morning, a lantern letter at dusk.
        private string LanternReadKey { get { return "evergrove:lantern-read:" + world.Garden; } }

        private int LanternReadOn()
        {
            int day;
            return int.TryParse(PlayerPrefs.GetString(LanternReadKey, ""), NumberStyles.Integer, CultureInfo.InvariantCulture, out day) ? day : 0;
        }

        // Called from the hub, beside the letterbox: a token for what the day offers now, if anything.
        private void DayRhythmTokens(Transform parent)
        {
            if (world == null || world.Mode != PlayMode.Living || !DayLightOn || titleScreen == null) return;
            Now(out int hour, out int minute);
            DayPhase phase = DayLight.PhaseAt(hour, minute);
            int day = LocalDayStamp();
            if (DayRhythm.Offer(phase, day) == DayOffer.DewBoard)
                DayToken(parent, 1, "glyph-droplet", DayRhythm.DewBoardWords, rect => OpenDewBoard());
            else if (DayRhythm.LetterDue(phase, day, LanternReadOn()))
                DayToken(parent, 1, "build-96", "A lantern letter from " + DayRhythm.LanternFrom(day) + ", waiting by the letterbox.",
                    ShowLanternLetter);
        }

        private void DayToken(Transform parent, int slot, string art, string caption, Action<RectTransform> onTap)
        {
            var node = Node("Day token " + art, parent);
            var rect = node.GetComponent<RectTransform>();
            Rect safe = Screen.safeArea;
            float w = Mathf.Max(1, Screen.width), h = Mathf.Max(1, Screen.height);
            rect.anchorMin = rect.anchorMax = new Vector2(safe.xMax / w, safe.yMax / h);
            rect.pivot = new Vector2(1f, 1f);
            rect.sizeDelta = new Vector2(52f, 52f);
            rect.anchoredPosition = new Vector2(-84f - slot * 60f, -ChromeTop - 58f);
            var image = node.AddComponent<Image>();
            image.sprite = MosseloraArt.Load(art);
            image.preserveAspect = true;
            MosseloraArt.Lift(image, 2f, 0.18f);
            var button = node.AddComponent<Button>();
            button.targetGraphic = image;
            LongPressCaption.Attach(node, caption, font);
            PressSquash.Add(node);
            button.onClick.AddListener(() => onTap(rect));
        }

        // The dew board: one small board, dealt by the day, that never costs or pays anything.
        private void OpenDewBoard()
        {
            if (titleScreen == null || world == null) return;
            int seed = ReplayBoards.Seed(world.Garden * 1000 + 900 + LocalDayStamp() % 1000, 0);
            OpenThingPuzzle(seed, 0, ThingPuzzle.ForStory(0, VillageChoice.Flowers), MosseloraArt.Load("glyph-droplet"),
                "Dew board", 0, 0, () => { PlayChime(); return 0; });
        }

        // The lantern letter: read once a day (the day is remembered on this device), then the token goes.
        private void ShowLanternLetter(RectTransform token)
        {
            if (world == null || titleScreen == null) return;
            int day = LocalDayStamp();
            PlayerPrefs.SetString(LanternReadKey, day.ToString(CultureInfo.InvariantCulture));
            PlayerPrefs.Save();
            RuleToast.Show(token, DayRhythm.LanternWords(day), font, Ink, Paper); // a short toast beside the letterbox
            if (token != null) token.gameObject.SetActive(false);
        }
    }
}
