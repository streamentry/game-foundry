#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System;
using System.Collections;
using System.IO;
using System.Linq;
using Evergrove.Core;
using UnityEngine;
using UnityEngine.UI;

namespace Evergrove.UnityClient
{
    /// <summary>
    /// Development-build-only scripted visual QA tour of the core loop.
    /// Launch with <c>-mosseloraQaTour /absolute/output/dir</c>. The tour uses
    /// an in-memory world (EvergroveApp never loads or saves the player's file)
    /// and drives the real buttons and tiles by invoking their onClick listeners,
    /// so every screenshot is the genuine UI state. Writes numbered PNGs, logs
    /// <c>MOSSELORA_QA_TOUR_DONE &lt;count&gt;</c> and quits.
    /// Inactive in release builds, in the Editor and when the argument is absent.
    /// </summary>
    public sealed partial class DevQaTour : MonoBehaviour
    {
        private const string TourArg = "-mosseloraQaTour";
        private const int JournalAfterChapter = 3; // zero-based: choice made on chapter 4
        private const string SnapshotStamp = "2026-01-01T00:00:00Z";

        // Mixed, non-repeating choice pattern for the main (ending) run.
        private static readonly VillageChoice[] MainPattern =
        {
            VillageChoice.Bridge, VillageChoice.ShadeTree, VillageChoice.Flowers,
            VillageChoice.Bridge, VillageChoice.ShadeTree, VillageChoice.Flowers,
            VillageChoice.Bridge, VillageChoice.Flowers, VillageChoice.ShadeTree,
            VillageChoice.Bridge, VillageChoice.ShadeTree, VillageChoice.Flowers
        };

        private string outputDir;
        private EvergroveApp app;
        private int saved;

        /// <summary>True only for a debug player launched with the tour argument.</summary>
        public static bool IsRequested
        {
            get { return Debug.isDebugBuild && !Application.isEditor && OutputDirectory() != null; }
        }

        private static string OutputDirectory()
        {
            string path = ReadArg(TourArg);
            return string.IsNullOrWhiteSpace(path) || !Path.IsPathRooted(path) ? null : path;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Install()
        {
            if (!IsRequested) return;
            // Without this, an unfocused QA window stops advancing frames and the
            // tour stalls silently. Dev-only; the player's own runs are untouched.
            Application.runInBackground = true;
            var host = new GameObject("Mosselora QA Tour");
            DontDestroyOnLoad(host);
            var tour = host.AddComponent<DevQaTour>();
            tour.outputDir = OutputDirectory();
        }

        private IEnumerator Start()
        {
            // Single-level pump so an exception in any step is caught here and
            // the process always exits instead of hanging on a stalled tour.
            IEnumerator run = RunTour();
            while (true)
            {
                object current;
                try
                {
                    if (!run.MoveNext()) break;
                    current = run.Current;
                }
                catch (Exception error)
                {
                    Debug.LogError("MOSSELORA_QA_TOUR_FAILED " + error.Message);
                    Application.Quit();
                    yield break;
                }
                yield return current;
            }
        }

        private IEnumerator RunTour()
        {
            Directory.CreateDirectory(outputDir);
            for (int i = 0; i < 600 && app == null; i++)
            {
                app = FindFirstObjectByType<EvergroveApp>();
                if (app == null) yield return null;
            }
            if (app == null) throw new InvalidOperationException("EvergroveApp was not created");
            // A fixed clock (I282): shots compare build to build whatever time the tour runs.
            app.QaSetClock(13);
            app.QaSetWeather((int)Weather.Fair); // I353: tours never depend on today's real weather
            // Picture rules show the first few times per device (I254/I262): every tour sees them.
            foreach (string kind in new[] { "water", "pairs", "mosaic", "nonogram", "sudoku", "plots", "streampath", "mahjong" })
                PlayerPrefs.DeleteKey("evergrove:rule-seen-" + kind);
            for (int f = 0; f < 8; f++) yield return null;

            // Run A: the first chapter, then the mixed choice path to the ending.
            // The app starts with a fresh in-memory world (no save is read).
            Puzzle first = app.QaWorld.CurrentPuzzle;
            yield return new WaitForEndOfFrame();
            SavePng("01-chapter1-start.png");
            // v1 #6: the first-launch text size chooser, shown over the first board.
            app.QaFirstSize(true);
            for (int f = 0; f < 6; f++) yield return null;
            yield return new WaitForEndOfFrame();
            SavePng("01a-first-size.png");
            app.QaFirstSize(false);
            for (int f = 0; f < 4; f++) yield return null;

            SolvePartially(first, 3);
            for (int f = 0; f < 6; f++) yield return null;
            yield return new WaitForEndOfFrame();
            SavePng("02-chapter1-partially-solved.png");

            SolveBoard();
            // Let the game's own payoff hold and glide place the screen, exactly
            // as a player sees it; a forced jump here hid I74's whole-board stop (I96).
            yield return new WaitForSecondsRealtime(2.4f);
            yield return new WaitForEndOfFrame();
            SavePng("03-chapter1-solved-choice.png");

            for (int chapter = 0; chapter < PuzzleCatalog.Count; chapter++)
            {
                // Seasons arrive through progress: capture the village in autumn and winter.
                if (chapter == 7 || chapter == 10)
                {
                    // Sideways the village lives on the left of the screen, not in the
                    // page: capture the screen as it is, so the landscape run completes (I170).
                    if (Screen.width <= Screen.height && GameObject.Find("Living storybook art") != null) ScrollTo("Living storybook art");
                    for (int f = 0; f < 4; f++) yield return null;
                    yield return new WaitForEndOfFrame();
                    SavePng(chapter == 7 ? "15-autumn-village.png" : "16-winter-village.png");
                }
                SolveBoard();
                for (int f = 0; f < 6; f++) yield return null;
                string beforePreview = GardenTransfer.Export(app.QaWorld, SnapshotStamp);
                string afterChoice = ExpectedChoice(MainPattern[chapter]);
                Press("Choice " + MainPattern[chapter]);
                for (int f = 0; f < 6; f++) yield return null;
                RequirePreview(beforePreview, chapter);
                if (chapter == 0)
                {
                    // Inspect another card, then close it: neither action may
                    // change any saved board, history, hint or garden field.
                    Button oldApply = FindButton(n => n == "Apply garden choice", "Apply garden choice");
                    Button oldClose = FindButton(n => n == "Close choice preview", "Close choice preview");
                    Press("Choice " + VillageChoice.Flowers);
                    // Rebuild destroys old controls at frame end. A stale
                    // activation must not apply or cancel the new selection.
                    oldApply.onClick.Invoke();
                    oldClose.onClick.Invoke();
                    RequirePreview(beforePreview, chapter);
                    for (int f = 0; f < 6; f++) yield return null;
                    RequirePreview(beforePreview, chapter);
                    Press("Close choice preview");
                    for (int f = 0; f < 6; f++) yield return null;
                    RequireWorld(beforePreview, "Closing a preview changed the garden");
                    if (HasActiveElement("Choice preview") || HasActiveElement("Apply garden choice"))
                        throw new InvalidOperationException("Closing a preview left its controls active");
                    Press("Choice " + MainPattern[chapter]);
                    for (int f = 0; f < 6; f++) yield return null;
                    RequirePreview(beforePreview, chapter);
                    ScrollTo("Choice preview");
                    for (int f = 0; f < 4; f++) yield return null;
                    yield return new WaitForEndOfFrame();
                    SavePng("03b-chapter1-choice-preview.png");
                }
                if (chapter == PuzzleCatalog.Count - 1)
                {
                    ScrollTo("Choice preview");
                    for (int f = 0; f < 4; f++) yield return null;
                    yield return new WaitForEndOfFrame();
                    SavePng("17-final-choice-preview.png");
                }
                Press("Apply garden choice");
                for (int f = 0; f < 6; f++) yield return null;
                RequireWorld(afterChoice, "Applying a preview differed from the chosen outcome");
                if (HasActiveElement("Choice preview"))
                    throw new InvalidOperationException("Committed choice left a preview active");
                if (chapter == 2)
                {
                    // The third stream opens the second reach: its mist lifting (I195).
                    yield return new WaitForSecondsRealtime(1.4f);
                    yield return new WaitForEndOfFrame();
                    SavePng("05b-new-reach.png");
                }
                Press("Continue");
                for (int f = 0; f < 6; f++) yield return null;

                if (chapter == JournalAfterChapter)
                {
                    Press("Memories");
                    for (int f = 0; f < 4; f++) yield return null;
                    ScrollTo("A living journal");
                    for (int f = 0; f < 4; f++) yield return null;
                    yield return new WaitForEndOfFrame();
                    SavePng("10-journal.png");
                    ScrollJournalToBottom();
                    for (int f = 0; f < 4; f++) yield return null;
                    yield return new WaitForEndOfFrame();
                    SavePng("10b-journal-bottom.png");
                    // A page opens to its memory (I166), then back to the book.
                    Press("Journal chapter");
                    for (int f = 0; f < 4; f++) yield return null;
                    yield return new WaitForEndOfFrame();
                    SavePng("10c-journal-page.png");
                    Press("Back to the book");
                    for (int f = 0; f < 4; f++) yield return null;
                    // The almanac tab (I174), then back to the pages.
                    Press("Almanac");
                    for (int f = 0; f < 4; f++) yield return null;
                    yield return new WaitForEndOfFrame();
                    SavePng("10d-almanac.png");
                    Press("The garden's ways"); // I403
                    for (int f = 0; f < 4; f++) yield return null;
                    yield return new WaitForEndOfFrame();
                    SavePng("10e-garden-ways.png");
                    Press("Our pages");
                    for (int f = 0; f < 4; f++) yield return null;
                    Press("Close journal"); // close it before the rest of the run
                    for (int f = 0; f < 4; f++) yield return null;
                }
            }
            if (!app.QaWorld.Completed) throw new InvalidOperationException("Main run did not reach the ending");
            ScrollTo("The ending");
            for (int f = 0; f < 4; f++) yield return null;
            yield return new WaitForEndOfFrame();
            SavePng("11-ending.png");
            ScrollToBottom();
            for (int f = 0; f < 4; f++) yield return null;
            yield return new WaitForEndOfFrame();
            SavePng("11b-ending-bottom.png");

            // The finished season's journal: all twelve memories, scrolled to the end (I99).
            Press("Memories");
            for (int f = 0; f < 4; f++) yield return null;
            ScrollJournalToBottom();
            for (int f = 0; f < 4; f++) yield return null;
            yield return new WaitForEndOfFrame();
            SavePng("11c-ending-journal-bottom.png");
            Press("Close journal");
            for (int f = 0; f < 4; f++) yield return null;
            // The home screen's garden map with the whole year's twelve things (I176).
            app.QaShowHub(true);
            for (int f = 0; f < 8; f++) yield return null;
            yield return new WaitForEndOfFrame();
            SavePng("17-hub-map.png");
            // v1 #18: the same hub at rest (corner chrome faded) and back in use.
            app.QaIdle(true);
            for (int f = 0; f < 4; f++) yield return null;
            yield return new WaitForEndOfFrame();
            SavePng("17-hub-idle.png");
            app.QaIdle(false);
            for (int f = 0; f < 4; f++) yield return null;
            yield return new WaitForEndOfFrame();
            SavePng("17-hub-awake.png");
            // The one corner button's fan (I191), with a whole year of memories.
            Press("Open menu");
            for (int f = 0; f < 12; f++) yield return null;
            yield return new WaitForSecondsRealtime(0.4f);
            yield return new WaitForEndOfFrame();
            SavePng("17b-menu-fan.png");
            Press("Open menu");
            for (int f = 0; f < 4; f++) yield return null;
            // The basket (I193): three more of the newest thing, planted beside it.
            app.QaPlantFromBasket();
            for (int f = 0; f < 8; f++) yield return null;
            yield return new WaitForSecondsRealtime(1.2f);
            yield return new WaitForEndOfFrame();
            SavePng("17c-basket.png");
            app.QaCloseBasket();
            // Every grown thing holds a puzzle (I203/I204): pick one up, play it, see it bloom.
            int thing = app.QaHoldThing();
            if (thing >= 0)
            {
                for (int f = 0; f < 6; f++) yield return null;
                yield return new WaitForEndOfFrame();
                SavePng("17e-thing-picked-up.png");
                app.QaOpenTreePlay(thing);
                for (int f = 0; f < 8; f++) yield return null;
                yield return new WaitForSecondsRealtime(0.4f);
                yield return new WaitForEndOfFrame();
                SavePng("17f-tree-puzzle.png");
                // I256: the classic kinds hint too: a wrong tile glows.
                app.QaHintTreePlay();
                for (int f = 0; f < 4; f++) yield return null;
                yield return new WaitForEndOfFrame();
                SavePng("17f2-water-hint.png");
                app.QaSolveTreePlay();
                yield return new WaitForSecondsRealtime(0.9f);
                yield return new WaitForEndOfFrame();
                SavePng("17g-tree-solved.png");
                // I293: the solved board settles into the thing it belongs to.
                yield return new WaitForSecondsRealtime(1.0f);
                yield return new WaitForEndOfFrame();
                SavePng("17g2-becomes-the-thing.png");
                yield return new WaitForSecondsRealtime(1.4f);
                for (int f = 0; f < 6; f++) yield return null;
                yield return new WaitForEndOfFrame();
                SavePng("17h-tree-bloomed.png");
                app.QaEndTreePlay();
                // I259/I261: play it again: a fresh board, depth shown in leaves.
                thing = app.QaHoldThing();
                app.QaOpenTreePlay(thing);
                for (int f = 0; f < 8; f++) yield return null;
                yield return new WaitForSecondsRealtime(0.4f);
                yield return new WaitForEndOfFrame();
                SavePng("17h2-replay-fresh-board.png");
                app.QaSolveTreePlay();
                yield return new WaitForSecondsRealtime(3.2f);
                for (int f = 0; f < 6; f++) yield return null;
                app.QaEndTreePlay();
            }
            // Flowers hold Pairs (depth spine): turn two cards, keep a match.
            int flower = app.QaHoldThing((int)VillageChoice.Flowers);
            if (flower >= 0)
            {
                for (int f = 0; f < 6; f++) yield return null;
                app.QaOpenTreePlay(flower);
                for (int f = 0; f < 8; f++) yield return null;
                yield return new WaitForSecondsRealtime(0.4f);
                yield return new WaitForEndOfFrame();
                SavePng("17i-pairs.png");
                // I264: take turns: the token wears whose turn it is.
                app.QaTogetherTreePlay();
                for (int f = 0; f < 4; f++) yield return null;
                yield return new WaitForEndOfFrame();
                SavePng("17i2-taking-turns.png");
                // I303/I302: four round one board, the second player's turn: the hint glows as an offer.
                app.QaFourTurnsTreePlay();
                for (int f = 0; f < 4; f++) yield return null;
                yield return new WaitForEndOfFrame();
                SavePng("17i3-four-take-turns.png");
                app.QaSolveTreePlay();
                yield return new WaitForSecondsRealtime(0.9f);
                yield return new WaitForEndOfFrame();
                SavePng("17j-pairs-solved.png");
                yield return new WaitForSecondsRealtime(2.4f);
                app.QaEndTreePlay();
                // I265/I266: the flowers wear two players' colours; Fen says you played it together.
                for (int f = 0; f < 6; f++) yield return null;
                yield return new WaitForEndOfFrame();
                SavePng("17j2-solved-together-mark.png");
                // I292: a thing replayed to its deepest tier wears a crown ring.
                app.QaCrown(flower);
                app.QaShowHub(true); // the refresh closes home in the tour; open it again
                for (int f = 0; f < 6; f++) yield return null;
                yield return new WaitForEndOfFrame();
                SavePng("17j3-crowned.png");
                // I294: quiet mode, just the garden; a tap brings everything back.
                app.QaQuiet(true);
                for (int f = 0; f < 4; f++) yield return null;
                yield return new WaitForEndOfFrame();
                SavePng("17j4-quiet-garden.png");
                app.QaQuiet(false);
                for (int f = 0; f < 4; f++) yield return null;
                // I328/I330: a jar of jam made, a bench and stepping stones set down.
                app.QaDecorAndJam();
                app.QaShowHub(true);
                for (int f = 0; f < 6; f++) yield return null;
                yield return new WaitForEndOfFrame();
                SavePng("17j7-decorations.png");
                // I333: the old well holds a deepest board; solving pays two fruit.
                app.QaOpenWell();
                for (int f = 0; f < 8; f++) yield return null;
                yield return new WaitForSecondsRealtime(0.4f);
                yield return new WaitForEndOfFrame();
                SavePng("17j8-old-well.png");
                app.QaSolveTreePlay();
                yield return new WaitForSecondsRealtime(3.2f);
                app.QaEndTreePlay();
                app.QaShowHub(true);
                for (int f = 0; f < 4; f++) yield return null;
                // I310: Nan left a lantern by the cottage (it glows at night below).
                app.QaLantern();
                // I299: the cottage's summary, what's waiting at a glance (and who left lanterns).
                // DevQaTour.Cottage.cs: 17j5-cottage*.png, every line. Stepped inline so the pump in Start still catches its errors.
                for (IEnumerator shots = CottageShots(); shots.MoveNext();) yield return shots.Current;
                for (int f = 0; f < 4; f++) yield return null;
                // I319: the lookout, the whole river at once.
                app.QaQuiet(true, true);
                for (int f = 0; f < 6; f++) yield return null;
                yield return new WaitForEndOfFrame();
                SavePng("17j6-lookout.png");
                app.QaQuiet(false);
                for (int f = 0; f < 4; f++) yield return null;
                app.QaShowHub(false);
                app.QaTalk(2);
                for (int f = 0; f < 8; f++) yield return null;
                yield return new WaitForSecondsRealtime(0.5f);
                yield return new WaitForEndOfFrame();
                SavePng("17j3-fen-together.png");
                app.QaTalk(-1);
            }
            // Buildings hold a Mosaic (I209): swap tiles to mend a memory painting.
            int building = app.QaHoldThing((int)VillageChoice.Bridge);
            if (building >= 0)
            {
                for (int f = 0; f < 6; f++) yield return null;
                app.QaOpenTreePlay(building);
                for (int f = 0; f < 8; f++) yield return null;
                yield return new WaitForSecondsRealtime(0.4f);
                yield return new WaitForEndOfFrame();
                SavePng("17m-mosaic.png");
                app.QaSolveTreePlay();
                yield return new WaitForSecondsRealtime(0.9f);
                yield return new WaitForEndOfFrame();
                SavePng("17n-mosaic-mended.png");
                yield return new WaitForSecondsRealtime(2.4f);
                app.QaEndTreePlay();
            }
            // I223: one more first solve opens a new stretch of river; home lifts its mist.
            // Make the river step independent of what came before: first solves until exactly one
            // drop is left (the b290 tour reached it one short after the I256 tour change).
            for (int guard = 0; guard < 8 && app.QaPuzzlesSolved() % GardenMap.SolvesPerReach != GardenMap.SolvesPerReach - 1; guard++)
            {
                app.QaShowHub(true); // a thing's puzzle opens over home (b291: the loop ran with home closed)
                for (int f = 0; f < 4; f++) yield return null;
                int extra = app.QaHoldUnsolved();
                if (extra < 0) break;
                for (int f = 0; f < 4; f++) yield return null;
                app.QaOpenTreePlay(extra);
                for (int f = 0; f < 6; f++) yield return null;
                app.QaSolveTreePlay();
                yield return new WaitForSecondsRealtime(3.2f);
                for (int f = 0; f < 4; f++) yield return null;
                app.QaEndTreePlay();
            }
            // I224: at the river's foot, three of four drops filled on the next stretch's mist.
            app.QaHubToFoot();
            for (int f = 0; f < 8; f++) yield return null;
            yield return new WaitForEndOfFrame();
            SavePng("17y-drops-on-the-mist.png");
            int tree = app.QaHoldUnsolved();
            if (tree >= 0)
            {
                Debug.Log("MOSSELORA_QA_RIVER solves before " + app.QaPuzzlesSolved() + " (wants " +
                    (GardenMap.SolvesPerReach - 1) + ")");
                for (int f = 0; f < 6; f++) yield return null;
                app.QaOpenTreePlay(tree);
                for (int f = 0; f < 8; f++) yield return null;
                app.QaSolveTreePlay();
                yield return new WaitForSecondsRealtime(2.4f);
                for (int f = 0; f < 4; f++) yield return null;
                yield return new WaitForSecondsRealtime(0.9f);
                yield return new WaitForEndOfFrame();
                SavePng("17w-river-opens.png");
                yield return new WaitForSecondsRealtime(3.2f);
                yield return new WaitForEndOfFrame();
                SavePng("17x-river-opened.png");
                app.QaEndTreePlay();
                // Checklist 43: travel by boat, from the hub, to the furthest open stretch.
                for (int f = 0; f < 8; f++) yield return null;
                app.QaOpenTravel();
                yield return new WaitForSecondsRealtime(0.5f);
                yield return new WaitForEndOfFrame();
                SavePng("17zc-boat-travel.png");
                app.QaTravel(app.QaOpenReaches() - 1);
                yield return new WaitForSecondsRealtime(1.5f);
                yield return new WaitForEndOfFrame();
                SavePng("17zd-boat-arrived.png");
            }
            // I243-I247: the five newer kinds, each unsolved and then solved.
            foreach (PuzzleKind kind in new[] { PuzzleKind.Nonogram, PuzzleKind.Sudoku, PuzzleKind.Plots,
                         PuzzleKind.StreamPath, PuzzleKind.Mahjong, PuzzleKind.Sowing, PuzzleKind.Sums,
                         PuzzleKind.Pieces, PuzzleKind.Doorstep })
            {
                app.QaOpenKind(kind, 1);
                for (int f = 0; f < 8; f++) yield return null;
                yield return new WaitForSecondsRealtime(0.4f);
                yield return new WaitForEndOfFrame();
                SavePng("18-" + kind.ToString().ToLowerInvariant() + ".png");
                // I253: one hint, shown (not yet made).
                app.QaHintTreePlay();
                for (int f = 0; f < 4; f++) yield return null;
                yield return new WaitForSecondsRealtime(0.3f);
                yield return new WaitForEndOfFrame();
                SavePng("18-" + kind.ToString().ToLowerInvariant() + "-hint.png");
                app.QaSolveTreePlay();
                yield return new WaitForSecondsRealtime(0.9f);
                yield return new WaitForEndOfFrame();
                SavePng("18-" + kind.ToString().ToLowerInvariant() + "-solved.png");
                yield return new WaitForSecondsRealtime(2.4f);
                for (int f = 0; f < 4; f++) yield return null;
                app.QaEndTreePlay();
            }
            // The nursery (I208): seeds buy a new plant that holds its own puzzle.
            app.QaOpenNursery();
            for (int f = 0; f < 6; f++) yield return null;
            yield return new WaitForEndOfFrame();
            SavePng("17k-nursery.png");
            app.QaBuyFromNursery();
            for (int f = 0; f < 6; f++) yield return null;
            yield return new WaitForSecondsRealtime(1.2f);
            yield return new WaitForEndOfFrame();
            SavePng("17l-nursery-bought.png");
            app.QaCloseBasket();
            // I238/I242: the poplar you bought is in your almanac now.
            app.QaShowHub(false);
            Press("Open menu");
            for (int f = 0; f < 8; f++) yield return null;
            Press("Almanac");
            for (int f = 0; f < 8; f++) yield return null;
            yield return new WaitForEndOfFrame();
            SavePng("17l2-almanac-after-buying.png");
            Press("Close journal");
            for (int f = 0; f < 4; f++) yield return null;
            app.QaEndTreePlay();
            // Family parcels (I215): send 50 seeds with a letter; then open one from Nan.
            FamilySheet family = app.QaFamily();
            if (family != null)
            {
                for (int f = 0; f < 6; f++) yield return null;
                yield return new WaitForEndOfFrame();
                SavePng("17p-family-send.png");
                // I343/I344: the wrapping step (a wish and a picture stamp), then the words step.
                family.QaStep(1, 50, 2, 3);
                for (int f = 0; f < 6; f++) yield return null;
                yield return new WaitForEndOfFrame();
                SavePng("17p1-family-wrap.png");
                family.QaStep(2, 50, 2, 3);
                for (int f = 0; f < 6; f++) yield return null;
                yield return new WaitForEndOfFrame();
                SavePng("17p2-family-words.png");
                family.QaSend("Grandma Lan", 50, "The pear tree is in flower. Come and see it soon.");
                for (int f = 0; f < 6; f++) yield return null;
                yield return new WaitForEndOfFrame();
                SavePng("17q-family-sent.png");
                FamilyParcel.TryCreate("Nan", "Planted one of these for you. Love, Nan", ParcelGift.Cutting, 2,
                    "qatour1", out string parcel, out _, out _);
                GUIUtility.systemCopyBuffer = "Look what I sent!\n" + FamilyParcel.Wrap(parcel, "Nan sent you a gift.");
                family.QaOpen();
                for (int f = 0; f < 6; f++) yield return null;
                yield return new WaitForEndOfFrame();
                SavePng("17r-family-opened.png");
                // Visit (I216): a relative's garden file, twelve streams and a basket of plantings.
                WorldModel theirs = WorldModel.New(PlayMode.Living);
                for (int chapter = 0; chapter < 12; chapter++)
                {
                    Puzzle theirBoard = theirs.CurrentPuzzle;
                    for (int i = 0; i < theirBoard.CellCount; i++)
                        while (theirs.Rotations[i] != theirBoard.Solution[i]) theirs.Rotate(i);
                    theirs.Choose((VillageChoice)((chapter * 2) % 3));
                }
                for (int cell = 31; cell < 140; cell += 13) theirs.Plant(11, cell);
                GUIUtility.systemCopyBuffer = GardenTransfer.Export(theirs, "2026-11-27T10:00:00Z");
                family.QaVisit();
                for (int f = 0; f < 8; f++) yield return null;
                yield return new WaitForSecondsRealtime(0.4f);
                yield return new WaitForEndOfFrame();
                SavePng("17s-visiting.png");
                // I227: help with one of their flower beds; it blooms in their garden.
                if (app.QaHelpOnVisit((int)VillageChoice.Flowers))
                {
                    for (int f = 0; f < 8; f++) yield return null;
                    yield return new WaitForSecondsRealtime(0.4f);
                    yield return new WaitForEndOfFrame();
                    SavePng("17s2-helping.png");
                    app.QaSolveTreePlay();
                    yield return new WaitForSecondsRealtime(3.2f);
                    for (int f = 0; f < 8; f++) yield return null;
                    yield return new WaitForSecondsRealtime(0.8f);
                    yield return new WaitForEndOfFrame();
                    SavePng("17s3-helped-their-garden.png");
                    app.QaEndTreePlay();
                }
                app.QaCloseVisit();
                // And the other way: Nan helped with one of your buildings.
                int helpedThing = app.QaUnsolved((int)VillageChoice.Bridge);
                if (helpedThing >= 0 && FamilyParcel.TryCreate("Nan", "", ParcelGift.Help, helpedThing,
                        "qatour2", out string help, out _, out _))
                {
                    GUIUtility.systemCopyBuffer = FamilyParcel.Wrap(help, "Nan helped in your garden.");
                    family = app.QaFamily(); // a visit closes the family card
                    for (int f = 0; f < 4; f++) yield return null;
                    family.QaOpen();
                    for (int f = 0; f < 6; f++) yield return null;
                    yield return new WaitForEndOfFrame();
                    SavePng("17s4-help-opened.png");
                }
                app.QaCloseFamily();
                // Letters stay in the garden (I217): the letterbox by the cottage, then Nan's letter.
                for (int f = 0; f < 6; f++) yield return null;
                yield return new WaitForEndOfFrame();
                SavePng("17t-letterbox.png");
                // I236: Orin points to the letter waiting unread.
                app.QaShowHub(false);
                app.QaTalk(1);
                for (int f = 0; f < 8; f++) yield return null;
                yield return new WaitForEndOfFrame();
                SavePng("17t2-orin-letter.png");
                app.QaTalk(-1);
                app.QaShowHub(true);
                for (int f = 0; f < 6; f++) yield return null;
                app.QaReadLetter();
                // I234: the seal lifts, then the paper rises.
                for (int f = 0; f < 6; f++) yield return null;
                yield return new WaitForSecondsRealtime(0.3f);
                yield return new WaitForEndOfFrame();
                SavePng("17u0-envelope.png");
                yield return new WaitForSecondsRealtime(1.2f);
                yield return new WaitForEndOfFrame();
                SavePng("17u-letter.png");
                // I235: write back, "Dear Nan," begun.
                app.QaWriteBack();
                for (int f = 0; f < 8; f++) yield return null;
                yield return new WaitForEndOfFrame();
                SavePng("17u2-write-back.png");
                app.QaCloseFamily();
                app.QaCloseLetter();
                // I269: the book's family page: letters (Nan's, and Mira's river note), help, together.
                app.QaShowHub(false);
                Press("Open menu");
                for (int f = 0; f < 8; f++) yield return null;
                Press("Memories");
                for (int f = 0; f < 6; f++) yield return null;
                Press("Family in the garden");
                for (int f = 0; f < 8; f++) yield return null;
                yield return new WaitForEndOfFrame();
                SavePng("17u3-family-page.png");
                Press("Close journal");
                for (int f = 0; f < 4; f++) yield return null;
            }
            // Neighbours notice (I210): Mira names the poplar you just bought.
            app.QaShowHub(false);
            app.QaTalk(0);
            for (int f = 0; f < 8; f++) yield return null;
            yield return new WaitForSecondsRealtime(0.6f);
            yield return new WaitForEndOfFrame();
            SavePng("17o-mira-notices.png");
            app.QaTalk(-1);
            // I222: Fen points to the clover cutting Nan sent, still in the basket.
            app.QaTalk(2);
            for (int f = 0; f < 8; f++) yield return null;
            yield return new WaitForSecondsRealtime(0.6f);
            yield return new WaitForEndOfFrame();
            SavePng("17v-fen-gift.png");
            app.QaTalk(-1);
            // I227: Orin names Nan, who helped with one of your buildings.
            app.QaTalk(1);
            for (int f = 0; f < 8; f++) yield return null;
            yield return new WaitForSecondsRealtime(0.6f);
            yield return new WaitForEndOfFrame();
            SavePng("17z-orin-help.png");
            // I231: hand Fen a fruit from your trees; she thanks you at once.
            Debug.Log("MOSSELORA_QA_FRUIT " + app.QaFruit());
            app.QaTalk(2);
            for (int f = 0; f < 6; f++) yield return null;
            yield return new WaitForEndOfFrame();
            SavePng("17za-give-fruit.png");
            app.QaGiveFruit(2);
            for (int f = 0; f < 8; f++) yield return null;
            yield return new WaitForSecondsRealtime(0.6f);
            yield return new WaitForEndOfFrame();
            SavePng("17zb-fen-thanks.png");
            app.QaTalk(-1);
            // Settings as round tokens (I194).
            Press("Open menu");
            for (int f = 0; f < 4; f++) yield return null;
            Press("Settings");
            for (int f = 0; f < 8; f++) yield return null;
            yield return new WaitForEndOfFrame();
            SavePng("17d-settings.png");
            // I276: the soft ask for garden notes, in the garden's own words.
            Press("Close menu");
            for (int f = 0; f < 4; f++) yield return null;
            app.QaShowHub(true);
            for (int f = 0; f < 4; f++) yield return null;
            app.QaNotesAsk();
            for (int f = 0; f < 6; f++) yield return null;
            yield return new WaitForEndOfFrame();
            SavePng("17d2-garden-notes-ask.png");
            app.QaCloseNotesAsk();
            app.QaShowHub(false);
            Press("Open menu");
            for (int f = 0; f < 4; f++) yield return null;
            Press("Settings");
            for (int f = 0; f < 6; f++) yield return null;
            Press("Close menu");
            for (int f = 0; f < 4; f++) yield return null;
            app.QaShowHub(true);
            for (int f = 0; f < 4; f++) yield return null;
            app.QaShowHub(false);
            for (int f = 0; f < 4; f++) yield return null;

            // Runs B-D: a fresh world per village choice on chapter 1. Each shows
            // the consequence (04-06) and the chapter-2 board it produced (07-09).
            VillageChoice[] branches = { VillageChoice.ShadeTree, VillageChoice.Bridge, VillageChoice.Flowers };
            string[] consequenceShots = { "04-consequence-shade-tree.png", "05-consequence-bridge.png", "06-consequence-flowers.png" };
            string[] boardShots = { "07-chapter2-after-shade-tree.png", "08-chapter2-after-bridge.png", "09-chapter2-after-flowers.png" };
            for (int b = 0; b < branches.Length; b++)
            {
                app.QaReplaceWorld(WorldModel.New());
                for (int f = 0; f < 6; f++) yield return null;
                SolveBoard();
                for (int f = 0; f < 6; f++) yield return null;
                string beforePreview = GardenTransfer.Export(app.QaWorld, SnapshotStamp);
                string afterChoice = ExpectedChoice(branches[b]);
                Press("Choice " + branches[b]);
                for (int f = 0; f < 6; f++) yield return null;
                RequirePreview(beforePreview, 0);
                Press("Apply garden choice");
                for (int f = 0; f < 6; f++) yield return null;
                RequireWorld(afterChoice, "Branch preview committed a different outcome");
                yield return new WaitForEndOfFrame();
                SavePng(consequenceShots[b]);
                if (b == 0)
                {
                    ScrollToBottom();
                    for (int f = 0; f < 4; f++) yield return null;
                    yield return new WaitForEndOfFrame();
                    SavePng("04b-consequence-shade-tree-bottom.png");
                }
                Press("Continue");
                for (int f = 0; f < 6; f++) yield return null;
                ScrollTo("Chapter");
                for (int f = 0; f < 4; f++) yield return null;
                yield return new WaitForEndOfFrame();
                SavePng(boardShots[b]);
            }

            // Run E: PuzzleMatched control. Chapter-2 board uses the same variant
            // a Living Bridge choice produces, so shot 12 compares against 08.
            app.QaReplaceWorld(WorldModel.NewMatched(
                WorldModel.ScheduleFromChoices(new[] { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 })));
            for (int f = 0; f < 6; f++) yield return null;
            SolveBoard();
            for (int f = 0; f < 6; f++) yield return null;
            if (HasActiveElement("Choice preview") || HasActiveElement("Apply garden choice") ||
                HasActiveElement("Choice " + VillageChoice.ShadeTree))
                throw new InvalidOperationException("PuzzleMatched exposed a village choice");
            Press("Next stream");
            for (int f = 0; f < 6; f++) yield return null;
            ScrollTo("Chapter");
            for (int f = 0; f < 4; f++) yield return null;
            yield return new WaitForEndOfFrame();
            SavePng("12-matched-chapter2.png");

            // Step 13: tap a protected landmark on that board. It must explain
            // itself in the status line without spending a move or undo entry.
            Puzzle board = app.QaWorld.CurrentPuzzle;
            if (board.AnchoredTile >= 0)
            {
                int[] rotationsBefore = app.QaWorld.Rotations;
                bool undoBefore = app.QaWorld.CanUndo;
                Tap(board.AnchoredTile, board.Size);
                for (int f = 0; f < 4; f++) yield return null;
                if (!Enumerable.SequenceEqual(rotationsBefore, app.QaWorld.Rotations) ||
                    undoBefore != app.QaWorld.CanUndo)
                    throw new InvalidOperationException("Protected tap changed the board or undo state");
                ScrollTo("Water feedback");
                for (int f = 0; f < 4; f++) yield return null;
                yield return new WaitForEndOfFrame();
                SavePng("13-anchored-tap.png");
            }

            // Step 14: garden file round trip through the real settings buttons and
            // the system clipboard. Copy, change the garden, restore, and require
            // the restored garden to match the copied one exactly.
            string beforeCopy = GardenTransfer.Export(app.QaWorld, SnapshotStamp);
            Press("Copy garden file");
            for (int f = 0; f < 4; f++) yield return null;
            WorldModel copiedWorld;
            GardenImportError copyError;
            if (!GardenTransfer.TryImport(GUIUtility.systemCopyBuffer, out copiedWorld, out copyError))
                throw new InvalidOperationException("Copied garden file did not import: " + copyError);
            if (GardenTransfer.Export(copiedWorld, SnapshotStamp) != beforeCopy)
                throw new InvalidOperationException("Clipboard garden file differs from the live garden");
            app.QaReplaceWorld(WorldModel.New());
            for (int f = 0; f < 4; f++) yield return null;
            if (GardenTransfer.Export(app.QaWorld, SnapshotStamp) == beforeCopy)
                throw new InvalidOperationException("Garden change did not take effect before restore");
            Press("Restore garden file");
            for (int f = 0; f < 4; f++) yield return null;
            Press("Confirm garden restore");
            for (int f = 0; f < 4; f++) yield return null;
            if (GardenTransfer.Export(app.QaWorld, SnapshotStamp) != beforeCopy)
                throw new InvalidOperationException("Restored garden does not match the copied garden");
            ScrollTo("Garden file");
            for (int f = 0; f < 4; f++) yield return null;
            yield return new WaitForEndOfFrame();
            SavePng("14-garden-file.png");

            // I251: home through the year. A finished garden, then 4, 8 and 12 first
            // solves: autumn, winter, spring (the festival itself is summer).
            Press("Close menu");
            for (int f = 0; f < 4; f++) yield return null;
            string[] seasons = { "19-summer-home.png", "19-autumn-home.png", "19-winter-home.png", "19-spring-home.png" };
            for (int turn = 0; turn < seasons.Length; turn++)
            {
                WorldModel year = WorldModel.New(PlayMode.Living);
                for (int chapter = 0; chapter < PuzzleCatalog.Count; chapter++)
                {
                    Puzzle stream = year.CurrentPuzzle;
                    for (int i = 0; i < stream.CellCount; i++)
                        while (year.Rotations[i] != stream.Solution[i]) year.Rotate(i);
                    year.Choose((VillageChoice)(chapter % 3));
                }
                for (int i = 0; i < turn * GardenMap.SolvesPerReach; i++) year.SolveTree(i);
                app.QaReplaceWorld(year);
                app.QaShowHub(true);
                for (int f = 0; f < 8; f++) yield return null;
                yield return new WaitForEndOfFrame();
                SavePng(seasons[turn]);
                LogAtmosphere(seasons[turn]); // I433
                app.QaShowHub(false);
                // I354–I357: a day of each season's own weather: hot summer, windy autumn,
                // snowy winter, rainy spring.
                Weather[] dayOf = { Weather.Hot, Weather.Windy, Weather.Snowy, Weather.Rainy };
                app.QaSetWeather((int)dayOf[turn]);
                app.QaShowHub(true);
                for (int f = 0; f < 8; f++) yield return null;
                yield return new WaitForSecondsRealtime(0.6f);
                yield return new WaitForEndOfFrame();
                SavePng("19w-" + dayOf[turn].ToString().ToLowerInvariant() + "-day.png");
                app.QaShowHub(false);
                app.QaSetWeather((int)Weather.Fair);
            }
            // I364: the first fair day after rain brings a rainbow.
            app.QaRainbow(true);
            app.QaShowHub(true);
            for (int f = 0; f < 8; f++) yield return null;
            yield return new WaitForEndOfFrame();
            SavePng("19r-rainbow.png");
            app.QaShowHub(false);
            app.QaRainbow(false);

            // I278-I282: the same garden at dawn, golden hour, dusk and night; Fen at night.
            foreach (int hour in new[] { 6, 18, 20, 23 })
            {
                app.QaSetClock(hour);
                app.QaShowHub(true);
                for (int f = 0; f < 10; f++) yield return null;
                yield return new WaitForSecondsRealtime(0.4f);
                yield return new WaitForEndOfFrame();
                SavePng("20-light-" + hour.ToString("00") + ".png");
                LogAtmosphere("20-light-" + hour.ToString("00")); // I433
                app.QaShowHub(false);
            }
            app.QaTalk(2);
            for (int f = 0; f < 8; f++) yield return null;
            yield return new WaitForEndOfFrame();
            SavePng("20-fen-at-night.png");
            app.QaTalk(-1);
            app.QaSetClock(13);

            // I383: good neighbours and the visitors they bring, by day and by night, on a
            // finished summer garden arranged so every pairing stands.
            WorldModel neighbourly = WorldModel.New(PlayMode.Living);
            for (int chapter = 0; chapter < PuzzleCatalog.Count; chapter++)
            {
                Puzzle stream = neighbourly.CurrentPuzzle;
                for (int i = 0; i < stream.CellCount; i++)
                    while (neighbourly.Rotations[i] != stream.Solution[i]) neighbourly.Rotate(i);
                neighbourly.Choose((VillageChoice)(chapter % 3));
            }
            app.QaReplaceWorld(neighbourly);
            if (!app.QaArrangeGoodNeighbours()) throw new InvalidOperationException("No bank holds every good neighbour");
            // I429: the moments themselves: the first four found (toast and a keeper's hop), the
            // deeper three, then the season's gift on the next return.
            string[] moments = { "21a-good-neighbours-found.png", "21b-deeper-neighbours-found.png", "21c-season-gift.png" };
            for (int pass = 0; pass < moments.Length; pass++)
            {
                app.QaShowHub(true);
                for (int f = 0; f < 8; f++) yield return null;
                yield return new WaitForSecondsRealtime(0.9f);
                yield return new WaitForEndOfFrame();
                SavePng(moments[pass]);
                LogAtmosphere(moments[pass]); // I436: is the news toast there?
                app.QaShowHub(false);
            }
            foreach (int hour in new[] { 11, 18, 23 })
            {
                app.QaSetClock(hour);
                app.QaShowHub(true);
                for (int f = 0; f < 10; f++) yield return null;
                yield return new WaitForSecondsRealtime(0.8f);
                yield return new WaitForEndOfFrame();
                SavePng(hour == 11 ? "21-good-neighbours-day.png" : hour == 18 ? "21-good-neighbours-golden.png" : "21-good-neighbours-night.png");
                app.QaShowHub(false);
            }

            // Smart companion (SC02): a scripted week. Nine things grown, two tended, a letter from
            // Anh; one visit a day, each neighbour spoken to once; then each neighbour on day eight.
            WorldModel week = WorldModel.New(PlayMode.Living);
            for (int chapter = 0; chapter < 9; chapter++)
            {
                Puzzle stream = week.CurrentPuzzle;
                for (int i = 0; i < stream.CellCount; i++)
                    while (week.Rotations[i] != stream.Solution[i]) week.Rotate(i);
                week.Choose((VillageChoice)(chapter % 3));
            }
            week.SolveTree(0);
            week.SolveTree(1);
            week.KeepNote("Anh", "The plums are early this year.");
            // SC03: a cutting of Mira's kind waits in the basket all week, so her wordings rotate.
            for (int kind = 0; kind < Nursery.Kinds; kind++)
                if (Nursery.Family(kind) == VillageChoice.ShadeTree) { week.ReceiveNeighbourCutting(kind); break; }
            app.QaReplaceWorld(week);
            Weather[] weekWeather = { Weather.Rainy, Weather.Fair, Weather.Windy, Weather.Fair, Weather.Snowy, Weather.Fair, Weather.Hot, Weather.Fair };
            string[] who = { "mira", "orin", "fen" };
            for (int day = 0; day < 8; day++)
            {
                app.QaSetDay(day);
                app.QaSetWeather((int)weekWeather[day]);
                app.QaSetClock(day % 2 == 0 ? 18 : 13);
                if (day == 2) week.SolveTree(2); // something new mid-week
                for (int v = 0; v < 3; v++)
                {
                    Debug.Log("MOSSELORA_COMPANION day " + (day + 1) + " " + who[v] + ": " + app.QaNeighborSays(v));
                    app.QaTalk(v);
                    for (int f = 0; f < 4; f++) yield return null;
                    if (day == 7)
                    {
                        yield return new WaitForEndOfFrame();
                        SavePng("21-week-" + who[v] + ".png");
                    }
                    app.QaTalk(-1);
                }
            }
            app.QaSetDay(0);
            app.QaSetWeather((int)Weather.Fair);

            // Smart companion (SC05): one ranker, many uses. A finished garden with a cutting in the
            // basket, a letter, and family help on thing 2: the arrow plays the helped thing, one soft
            // mark shows where the top waiting thing lives, the cottage card lists what waits first.
            WorldModel waiting = WorldModel.New(PlayMode.Living);
            for (int chapter = 0; chapter < PuzzleCatalog.Count; chapter++)
            {
                Puzzle stream = waiting.CurrentPuzzle;
                for (int i = 0; i < stream.CellCount; i++)
                    while (waiting.Rotations[i] != stream.Solution[i]) waiting.Rotate(i);
                waiting.Choose((VillageChoice)(chapter % 3));
            }
            WorldSnapshot helpedSave = waiting.Export();
            helpedSave.helped = new[] { 2 };
            helpedSave.helpedFrom = new[] { "Anh" };
            if (WorldModel.TryRestore(helpedSave, out WorldModel helped)) waiting = helped;
            for (int kind = 0; kind < Nursery.Kinds; kind++)
                if (Nursery.Family(kind) == VillageChoice.Flowers) { waiting.ReceiveNeighbourCutting(kind); break; }
            app.QaReplaceWorld(waiting);
            app.QaShowHub(true);
            for (int f = 0; f < 8; f++) yield return null;
            yield return new WaitForEndOfFrame();
            SavePng("22-attention-hub.png");
            app.OpenCottage();
            for (int f = 0; f < 6; f++) yield return null;
            yield return new WaitForEndOfFrame();
            SavePng("22-attention-cottage.png");
            app.CloseCottage();
            app.QaShowHub(false);
            app.QaSetClock(13);

            Debug.Log("MOSSELORA_QA_TOUR_DONE " + saved);
            Application.Quit();
        }

        private string ExpectedChoice(VillageChoice choice)
        {
            WorldModel expected;
            if (!WorldModel.TryRestore(app.QaWorld.Export(), out expected) || !expected.Choose(choice))
                throw new InvalidOperationException("Cannot prepare the expected choice outcome");
            return GardenTransfer.Export(expected, SnapshotStamp);
        }

        private void RequireWorld(string expected, string failure)
        {
            if (GardenTransfer.Export(app.QaWorld, SnapshotStamp) != expected)
                throw new InvalidOperationException(failure);
        }

        private void RequirePreview(string beforePreview, int chapter)
        {
            RequireWorld(beforePreview, "Inspecting a preview changed the garden");
            if (!HasActiveElement("Choice preview") || !HasActiveElement("Apply garden choice"))
                throw new InvalidOperationException("Choice preview or its confirmation is missing");
            bool hasNextStream = HasActiveElement("Next stream preview");
            if (hasNextStream != (chapter < PuzzleCatalog.Count - 1))
                throw new InvalidOperationException("Preview promised the wrong next-stream state");
        }

        private static bool HasActiveElement(string name)
        {
            return FindObjectsByType<Transform>(FindObjectsInactive.Exclude, FindObjectsSortMode.None)
                .Any(t => t.name == name);
        }

        // I433: ground truth for the atmosphere, not just a picture: how many of each named
        // effect object stand in the scene at this shot (faint ones can hide in a screenshot).
        private void LogAtmosphere(string shot)
        {
            string[] names = { "Morning mist", "Dew", "Frost", "Chimney smoke", "Footprint in the snow", "The river's life",
                "Ring on the water", "Glitter on the water", "Moonlight on the water", "A leaf on the water", "A lantern by the seat", "Rule toast" };
            var counts = new System.Text.StringBuilder("MOSSELORA_ATMOSPHERE " + shot);
            var all = UnityEngine.Object.FindObjectsByType<RectTransform>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
            foreach (string name in names)
            {
                int n = 0;
                RectTransform first = null;
                foreach (RectTransform t in all) if (t.name == name) { n++; if (first == null) first = t; }
                counts.Append(" | ").Append(name).Append('=').Append(n);
                if (first != null)
                {
                    // Where it is on screen, how visible its parents leave it, and who holds it.
                    Vector2 screen = RectTransformUtility.WorldToScreenPoint(null, first.position);
                    float alpha = 1f;
                    foreach (CanvasGroup g in first.GetComponentsInParent<CanvasGroup>()) alpha *= g.alpha;
                    var graphic = first.GetComponent<UnityEngine.UI.Graphic>();
                    counts.Append(" @").Append(Mathf.RoundToInt(screen.x)).Append(',').Append(Mathf.RoundToInt(screen.y))
                        .Append(" of ").Append(Screen.width).Append('x').Append(Screen.height)
                        .Append(" a=").Append((alpha * (graphic != null ? graphic.color.a : 1f)).ToString("0.00"))
                        .Append(" scale=").Append(first.lossyScale.x.ToString("0.00"))
                        .Append(" in ").Append(first.parent != null ? first.parent.name : "-")
                        .Append(" #").Append(first.GetSiblingIndex()).Append('/').Append(first.parent != null ? first.parent.childCount : 0);
                }
            }
            Debug.Log(counts.ToString());
        }

        private void SavePng(string fileName)
        {
            var frame = new Texture2D(Screen.width, Screen.height, TextureFormat.RGB24, false);
            frame.ReadPixels(new Rect(0, 0, Screen.width, Screen.height), 0, 0);
            frame.Apply();
            File.WriteAllBytes(Path.Combine(outputDir, fileName), frame.EncodeToPNG());
            Destroy(frame);
            saved++;
            Debug.Log("MOSSELORA_QA_TOUR_SHOT " + fileName);
        }

        // Only active buttons: Rebuild deactivates the old hierarchy before Destroy.
        private static Button FindButton(Func<string, bool> nameMatches, string description)
        {
            foreach (Button button in FindObjectsByType<Button>(FindObjectsInactive.Exclude, FindObjectsSortMode.None))
                if (nameMatches(button.name)) return button;
            // Settings, files and research tools live in the menu sheet: open it
            // with the real menu button, then look again.
            foreach (Button button in FindObjectsByType<Button>(FindObjectsInactive.Exclude, FindObjectsSortMode.None))
                if (button.name == "Open menu")
                {
                    button.onClick.Invoke();
                    foreach (Button again in FindObjectsByType<Button>(FindObjectsInactive.Exclude, FindObjectsSortMode.None))
                        if (nameMatches(again.name)) return again;
                    // The corner button opens a fan (I191); settings is one of its tokens.
                    foreach (Button token in FindObjectsByType<Button>(FindObjectsInactive.Exclude, FindObjectsSortMode.None))
                        if (token.name == "Settings")
                        {
                            token.onClick.Invoke();
                            foreach (Button again in FindObjectsByType<Button>(FindObjectsInactive.Exclude, FindObjectsSortMode.None))
                                if (nameMatches(again.name)) return again;
                            break;
                        }
                    break;
                }
            throw new InvalidOperationException("No active button: " + description);
        }

        private static void Press(string name)
        {
            FindButton(n => n == name, name).onClick.Invoke();
        }

        private static void Tap(int index, int size)
        {
            string prefix = "Row " + (index / size + 1) + " column " + (index % size + 1) + " ";
            Button tile = FindButton(n => n.StartsWith(prefix, StringComparison.Ordinal), prefix);
            // Same gate a real pointer press uses: disabled tiles never fire.
            if (!tile.interactable) throw new InvalidOperationException("Tile not tappable: " + prefix);
            tile.onClick.Invoke();
        }

        private void SolveTile(Puzzle puzzle, int index)
        {
            if (puzzle.IsAnchored(index)) return;
            int limit = PipeRules.OrientationCount(puzzle.Kinds[index]);
            for (int attempt = 0; attempt <= limit; attempt++)
            {
                if (app.QaWorld.Rotations[index] == puzzle.Solution[index]) return;
                Tap(index, puzzle.Size); // the same tile button a player taps
            }
            throw new InvalidOperationException("Tile " + index + " did not reach its solution");
        }

        private void SolveBoard()
        {
            Puzzle puzzle = app.QaWorld.CurrentPuzzle;
            for (int index = 0; index < puzzle.CellCount; index++) SolveTile(puzzle, index);
            if (!app.QaWorld.Solved) throw new InvalidOperationException("Board unsolved after taps");
        }

        // Fixes the first `count` tiles that are actually misaligned. The authored
        // start already aligns some tiles, so a fixed index range would be a no-op.
        private void SolvePartially(Puzzle puzzle, int count)
        {
            int fixedTiles = 0;
            for (int index = 0; index < puzzle.CellCount && fixedTiles < count; index++)
            {
                if (puzzle.IsAnchored(index) || app.QaWorld.Rotations[index] == puzzle.Solution[index]) continue;
                SolveTile(puzzle, index);
                fixedTiles++;
            }
            if (fixedTiles < count) throw new InvalidOperationException("Too few misaligned tiles for partial state");
        }

        // Mirrors EvergroveApp.ScrollToDecision so the target sits near the top.
        private void ScrollTo(string targetName)
        {
            GameObject target = null;
            foreach (Transform t in FindObjectsByType<Transform>(FindObjectsInactive.Exclude, FindObjectsSortMode.None))
                if (t.name == targetName) { target = t.gameObject; break; }
            if (target == null) throw new InvalidOperationException("No visible element: " + targetName);
            // The page and the menu sheet each scroll; use the one holding the target.
            var scroll = target.GetComponentInParent<ScrollRect>();
            if (scroll == null || scroll.content == null || scroll.viewport == null)
                throw new InvalidOperationException("Scroll view missing");

            RectTransform content = scroll.content;
            var targetRect = target.GetComponent<RectTransform>();
            Canvas.ForceUpdateCanvases();
            LayoutRebuilder.ForceRebuildLayoutImmediate(content);
            float distance = content.rect.height - scroll.viewport.rect.height;
            if (distance <= 0f)
            {
                scroll.verticalNormalizedPosition = 1f;
                return;
            }
            Vector3 worldTop = targetRect.TransformPoint(
                new Vector3(targetRect.rect.center.x, targetRect.rect.yMax, 0f));
            float targetTop = content.rect.yMax - content.InverseTransformPoint(worldTop).y;
            scroll.StopMovement();
            scroll.verticalNormalizedPosition = 1f - Mathf.Clamp01((targetTop - 16f) / distance);
        }

        // Scrolls the main ScrollRect to its lowest position so the capture shows
        // whatever sits below the fold on long screens.
        private void ScrollToBottom()
        {
            var pageNode = GameObject.Find("Safe scroll area");
            var scroll = pageNode != null ? pageNode.GetComponent<ScrollRect>() : null;
            if (scroll == null) throw new InvalidOperationException("Scroll view missing");
            Canvas.ForceUpdateCanvases();
            LayoutRebuilder.ForceRebuildLayoutImmediate(scroll.content);
            scroll.StopMovement();
            scroll.verticalNormalizedPosition = 0f;
        }

        // The journal sheet scrolls on its own; the page behind it does not move (I99).
        private void ScrollJournalToBottom()
        {
            var panel = GameObject.Find("Journal panel");
            var scroll = panel != null ? panel.GetComponent<ScrollRect>() : null;
            if (scroll == null || scroll.content == null) throw new InvalidOperationException("Journal scroll missing");
            Canvas.ForceUpdateCanvases();
            LayoutRebuilder.ForceRebuildLayoutImmediate(scroll.content);
            scroll.StopMovement();
            scroll.verticalNormalizedPosition = 0f;
        }

        private static string ReadArg(string name)
        {
            return DevArgs.Read(name);
        }
    }
}
#endif
