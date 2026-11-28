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

        private void SaveWorld()
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            if (qaInMemoryOnly) return; // QA tour: in-memory only, no persistence
#endif
            saveFailed = !WorldStore.Save(world);
            UpdateSaveWarning(); // toggles immediately even without a full UI rebuild
            if (!saveFailed) NoteSaved(); // EvergroveApp.Saving.cs: the quiet "Saved" mark (item 96)
            if (saveFailed)
                Debug.LogWarning("Evergrove is playable but cannot persist this change.");
        }

#if UNITY_EDITOR || DEVELOPMENT_BUILD
        // Dev-only seam for DevQaTour. Replaces the in-memory world and redraws
        // through the normal Rebuild path; refuses any world that is not in-memory.
        internal WorldModel QaWorld { get { return world; } }

#if UNITY_EDITOR || DEVELOPMENT_BUILD
        private bool qaShowHub;

        // The tour normally skips the hub; this opens it once for a screenshot.
        // The tour opens the basket and plants three of the newest thing near it (I193).
        internal void QaPlantFromBasket()
        {
            GardenMap map = LoadGardenMap();
            if (map == null || world.Choices.Length == 0) return;
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            qaShowHub = true; // the tour's hub rebuilds after each planting
#endif
            basketOpen = true;
            basketKind = world.Choices.Length - 1;
            int[] layout = map.Layout(world.Choices, world.Placements, Occupied());
            int near = layout[layout.Length - 1];
            int planted = 0;
            for (int d = 1; d < 6 && planted < 3; d++)
                foreach (int cell in new[] { near - d, near + d, near - d * map.Columns, near + d * map.Columns })
                    if (planted < 3 && map.CanPlace(layout, cell) && !PlantedAt(cell))
                    {
                        OnMapLand(cell);
                        planted++;
                        layout = map.Layout(world.Choices, world.Placements, Occupied());
                    }
            RefreshMaps(); // shows the open tray with the kind picked
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            qaShowHub = false;
#endif
        }

        // I208 tour: open the nursery with the first plant picked, then buy one beside the newest thing.
        internal void QaOpenNursery()
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            qaShowHub = true;
#endif
            basketOpen = true;
            nurseryOpen = true;
            nurseryKind = world.Seeds >= Nursery.Price(0) ? 0 : -1;
            RefreshMaps();
        }

        internal void QaBuyFromNursery()
        {
            GardenMap map = LoadGardenMap();
            if (map == null || nurseryKind < 0) return;
            int[] layout = map.Layout(world.Choices, world.Placements, Occupied());
            int near = layout.Length > 0 ? layout[layout.Length - 1] : map.CellCount / 2;
            for (int d = 1; d < 8; d++)
                foreach (int cell in new[] { near + d, near - d, near + d * map.Columns, near - d * map.Columns })
                    if (cell >= 0 && map.CanPlace(layout, cell) && !PlantedAt(cell)) { OnMapLand(cell); return; }
        }

        internal void QaTalk(int villager)
        {
            if (villager < 0) { speakingWith = -1; Rebuild(); return; }
            OpenConversation(villager);
        }

        // I215 tour: open Family, send 50 seeds with a letter, then open a parcel from "Nan".
        internal FamilySheet QaFamily()
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            qaShowHub = true;
#endif
            if (titleScreen == null && canvasRoot != null) BuildTitleScreen(canvasRoot);
            OpenFamily();
            return familySheet;
        }

        internal void QaCloseVisit() { CloseVisit(); }

        internal void QaCloseFamily()
        {
            DismissFamily();
            RefreshMaps(); // as a real close does: the hub rebuilds
        }

        internal void QaCloseBasket()
        {
            basketOpen = false; basketKind = -1; heldPlanting = -1;
            nurseryOpen = false; nurseryKind = -1; heldBought = -1;
        }

        // I204 tour: pick up a grown thing, open its puzzle, solve it, see it bloom.
        private TreePlay qaTreePlay;
        internal int QaHoldThing(int family = -1)
        {
            // The classic kinds live in the first four things (I243): pick from those, so the
            // tour keeps playing water, Pairs and Mosaic (the newer kinds have their own shots).
            int[] made = world.Choices;
            int want = family >= 0 ? family : (int)VillageChoice.ShadeTree;
            int item = -1;
            for (int i = 0; i < made.Length && i < ThingPuzzle.ClassicChapters && item < 0; i++)
                if (made[i] == want) item = i;
            if (item < 0) item = family >= 0 ? System.Array.LastIndexOf(made, family) : Mathf.Min(4, made.Length - 1);
            if (item < 0) return -1;
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            qaShowHub = true; // the hub rebuilds when the puzzle closes
#endif
            heldPlanting = -1;
            mapInHand = item;
            RefreshMaps();
            return item;
        }

        // I223 tour: a grown thing whose puzzle is still unsolved (a first solve counts toward the river).
        internal int QaHoldUnsolved()
        {
            int item = -1;
            for (int i = world.Choices.Length - 1; i >= 0 && item < 0; i--)
                if (!world.TreeSolved(i) && world.Choices[i] == (int)VillageChoice.ShadeTree) item = i;
            if (item < 0) return -1;
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            qaShowHub = true;
#endif
            heldPlanting = -1;
            mapInHand = item;
            RefreshMaps();
            return item;
        }

        internal int QaPuzzlesSolved() { return world.PuzzlesSolved; }
        internal int QaFruit() { return world.Fruit; }
        internal void QaGiveFruit(int villager) { GiveFruit(villager); }

        // I227 tour: help with the first unsolved thing of a kind in the garden being visited.
        internal bool QaHelpOnVisit(int family)
        {
            if (visitWorld == null || visitLayer == null) return false;
            int[] made = visitWorld.Choices;
            for (int i = 0; i < made.Length; i++)
                if (made[i] == family && !visitWorld.TreeSolved(i) && !visitWorld.IsHelped(i))
                {
                    HelpWith(i);
                    qaTreePlay = visitLayer.GetComponentInChildren<TreePlay>();
                    return qaTreePlay != null;
                }
            return false;
        }

        // The first grown thing of a kind in your garden still unsolved, or -1.
        internal int QaUnsolved(int family)
        {
            int[] made = world.Choices;
            for (int i = 0; i < made.Length; i++)
                if (made[i] == family && !world.TreeSolved(i)) return i;
            return -1;
        }

        // I224 tour: home scrolled to the river's foot, where the drops sit on the mist.
        internal void QaHubToFoot()
        {
            qaShowHub = true;
            if (titleScreen == null && canvasRoot != null) BuildTitleScreen(canvasRoot);
            if (hubMapView != null) hubMapView.KeepAt(new Vector2(0.5f, 0f));
        }

        // I243 tour: open one of the newer kinds as a grown thing would hold it.
        internal void QaOpenKind(PuzzleKind kind, int tier)
        {
            qaShowHub = true;
            PlayerPrefs.DeleteKey("evergrove:rule-seen-" + kind.ToString().ToLowerInvariant()); // I254: shown each tour
            if (titleScreen == null && canvasRoot != null) BuildTitleScreen(canvasRoot);
            int item = Mathf.Min(5, world.Choices.Length - 1);
            if (titleScreen == null || item < 0) return;
            int[] made = world.Choices;
            PuzzleCard((RectTransform)titleScreen.transform, world.Garden * 1000 + 77 + (int)kind, tier, kind,
                ItemArt(item, made[item]), ItemAt(item, made[item]).Name, 0,
                count => PairsPictures(count, item, (c, f) => ItemArt(c, f)), () => PlayTock(), () => 0,
                () => { qaShowHub = false; });
            qaTreePlay = titleScreen.GetComponentInChildren<TreePlay>();
        }

        internal void QaOpenTreePlay(int item)
        {
            OpenTreePlay(item);
            qaTreePlay = titleScreen != null ? titleScreen.GetComponentInChildren<TreePlay>() : null;
        }

        internal void QaSolveTreePlay() { if (qaTreePlay != null) qaTreePlay.QaSolve(); }
        internal void QaHintTreePlay() { if (qaTreePlay != null) qaTreePlay.QaHint(); }
        internal void QaNotesAsk() { if (titleScreen != null) ShowNotesAsk(); }
        internal void QaSetClock(int hour) { qaClockHour = hour; }
        internal void QaSetDay(int offset) { qaDayOffset = offset; spokenThisVisit.Clear(); spokenVariant.Clear(); } // SC02 tour: a new day, a new visit
        internal string QaNeighborSays(int villager) { return NeighborSays(villager); }
        // CP40: the top three candidates behind a remark, for the tour's QA log (dev-only).
        internal string QaCompanionExplain(int villager)
        {
            return string.Join(" | ", CompanionRanker.Explain(CompanionFactsNow(), CompanionMemoryNow(), villager,
                CompanionNow().DayNumber, world.Garden, CompanionCanSay, 3));
        }
        internal void QaSetWeather(int weather) { qaWeather = weather; } // I353 tour: -1 is the real day's
        internal void QaRainbow(bool on) { qaRainbow = on; } // I364 tour
        internal void QaCloseNotesAsk() { CloseNotesAsk(); }
        internal void QaTogetherTreePlay() { if (qaTreePlay != null) qaTreePlay.QaTogether(); }
        internal void QaFourTurnsTreePlay() { if (qaTreePlay != null) qaTreePlay.QaFourTurns(); }

        internal void QaEndTreePlay()
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            qaShowHub = false;
#endif
            qaTreePlay = null;
        }

        // I292/I294 tour: crown a thing as if replayed to the deepest tier; enter or leave quiet.
        internal void QaCrown(int item)
        {
            if (!qaInMemoryOnly || item < 0) return;
            PlayerPrefs.SetInt(ReplayKey(item), ReplayBoards.ReplaysPerTier * 3);
            justSolved = item; // home frames it
            RefreshMapsFresh();
        }

        // I328/I330 tour: a jar of jam from three fruit, and a bench and stepping stones set down
        // on free land near the newest thing.
        internal void QaDecorAndJam()
        {
            if (!qaInMemoryOnly) return;
            world.MakeJam();
            GardenMap map = LoadGardenMap();
            if (map == null) return;
            int[] cells = map.Layout(world.Choices, world.Placements, world.Occupied);
            int near = cells.Length > 0 ? cells[cells.Length - 1] : 30;
            int placed = 0;
            for (int step = 1; step < map.CellCount && placed < 2; step++)
            {
                int cell = (near + step) % map.CellCount;
                if (world.PlaceDecor(placed == 0 ? Decoration.Bench : Decoration.Stones, cell, map)) placed++;
            }
            RefreshMaps();
        }

        // I333 tour: open the old well's board.
        internal void QaOpenWell()
        {
            if (!qaInMemoryOnly) return;
            OpenWell();
            qaTreePlay = titleScreen != null ? titleScreen.GetComponentInChildren<TreePlay>() : null;
        }

        // I310 tour: a real lantern parcel from a relative, opened here.
        internal void QaLantern()
        {
            if (!qaInMemoryOnly) return;
            if (FamilyParcel.TryCreate("Nan", "", ParcelGift.Lantern, 0, "qa-lantern-" + world.Lanterns.Length,
                    out string text, out ParcelContents _, out ParcelError _))
                world.Open(text);
        }

        // I299 (fit): the cottage shots carry a name, so the tour shows the card's title line too.
        internal void QaGardenName(string name)
        {
            if (!qaInMemoryOnly) return;
            world.SetGardenName(name);
        }

        internal void QaQuiet(bool on, bool wholeRiver = false)
        {
            if (on) EnterQuiet(wholeRiver); else ExitQuiet();
        }

        internal void QaShowHub(bool show)
        {
            if (!show) { CloseTitle(); return; }
            qaShowHub = true;
            if (titleScreen == null && canvasRoot != null) BuildTitleScreen(canvasRoot);
            qaShowHub = false;
        }
#endif
        // I383 tour: arrange a finished garden so every good neighbour stands somewhere: a
        // cluster on a bank (flower by the water, a tree beside it, two more trees, a second
        // flower, something built), found by trying each land cell on the real map.
        internal bool QaArrangeGoodNeighbours()
        {
            GardenMap map = LoadGardenMap();
            int[] made = world.Choices;
            if (map == null || made.Length < 12) return false;
            int f1 = 11, f2 = 8, t1 = 9, t2 = 6, t3 = 3, b1 = 10; // chapter % 3: tree, built, flower
            int[] arranged = { f1, t1, t2, t3, f2, b1 };
            int all = (1 << Enum.GetValues(typeof(Pairing)).Length) - 1;
            int w = map.Columns;
            int[][] shapes =
            {
                // Upward first: home opens with the newest thing (f1) low in view, so the rest shows above it.
                new[] { 0, 1, 1 - w, -w, -1, 2 }, new[] { 0, -1, -1 - w, -w, 1, -2 },
                new[] { 0, 1, 1 - w, -w, -1 - w, 2 - w }, new[] { 0, -1, -1 - w, -w, 1 - w, -2 - w },
                new[] { 0, 1, 1 + w, w, -1, 2 }, new[] { 0, -1, -1 + w, w, 1, -2 },
            };
            for (int c0 = 0; c0 < map.CellCount; c0++)
                foreach (int[] shape in shapes)
                {
                    var cells = new int[shape.Length];
                    bool ok = true;
                    for (int k = 0; k < shape.Length && ok; k++)
                    {
                        cells[k] = c0 + shape[k];
                        int dx = Mathf.Abs(cells[k] % w - c0 % w);
                        ok = cells[k] >= 0 && cells[k] < map.CellCount && dx <= 2 && map.IsLand(cells[k]);
                    }
                    if (!ok) continue;
                    var trial = WorldModel.TryRestore(world.Export(), out WorldModel copy) ? copy : null;
                    if (trial == null) return false;
                    for (int k = 0; k < arranged.Length; k++) trial.PlaceAt(arranged[k], cells[k]);
                    var things = Pairings.Things(trial, map);
                    var seen = new System.Collections.Generic.HashSet<int>();
                    foreach (PlacedThing t in things) if (t.Cell >= 0 && !seen.Add(t.Cell)) ok = false;
                    if (!ok || Pairings.Between(map, things) != all) continue;
                    for (int k = 0; k < arranged.Length; k++) world.PlaceAt(arranged[k], cells[k]);
                    Rebuild(true);
                    return true;
                }
            return false;
        }

        internal void QaReplaceWorld(WorldModel next)
        {
            if (!qaInMemoryOnly || next == null) return;
            world = next;
            speakingWith = -1;
            showingMemory = false;
            showJournal = false;
            confirmReset = false;
            Rebuild(true);
        }
#endif
    }
}
