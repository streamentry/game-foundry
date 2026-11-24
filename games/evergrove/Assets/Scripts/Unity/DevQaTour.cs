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
    public sealed class DevQaTour : MonoBehaviour
    {
        private const string TourArg = "-mosseloraQaTour";
        private const int JournalAfterChapter = 3; // zero-based: choice made on chapter 4

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
            for (int f = 0; f < 8; f++) yield return null;

            // Run A: the first chapter, then the mixed choice path to the ending.
            // The app starts with a fresh in-memory world (no save is read).
            Puzzle first = app.QaWorld.CurrentPuzzle;
            yield return new WaitForEndOfFrame();
            SavePng("01-chapter1-start.png");

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
                    ScrollTo("Living storybook art");
                    for (int f = 0; f < 4; f++) yield return null;
                    yield return new WaitForEndOfFrame();
                    SavePng(chapter == 7 ? "15-autumn-village.png" : "16-winter-village.png");
                }
                SolveBoard();
                for (int f = 0; f < 6; f++) yield return null;
                Press("Choice " + MainPattern[chapter]);
                for (int f = 0; f < 6; f++) yield return null;
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
                    ScrollToBottom();
                    for (int f = 0; f < 4; f++) yield return null;
                    yield return new WaitForEndOfFrame();
                    SavePng("10b-journal-bottom.png");
                    Press("Memories"); // close it before the rest of the run
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
                Press("Choice " + branches[b]);
                for (int f = 0; f < 6; f++) yield return null;
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
            const string probeStamp = "2026-01-01T00:00:00Z";
            string beforeCopy = GardenTransfer.Export(app.QaWorld, probeStamp);
            Press("Copy garden file");
            for (int f = 0; f < 4; f++) yield return null;
            WorldModel copiedWorld;
            GardenImportError copyError;
            if (!GardenTransfer.TryImport(GUIUtility.systemCopyBuffer, out copiedWorld, out copyError))
                throw new InvalidOperationException("Copied garden file did not import: " + copyError);
            if (GardenTransfer.Export(copiedWorld, probeStamp) != beforeCopy)
                throw new InvalidOperationException("Clipboard garden file differs from the live garden");
            app.QaReplaceWorld(WorldModel.New());
            for (int f = 0; f < 4; f++) yield return null;
            if (GardenTransfer.Export(app.QaWorld, probeStamp) == beforeCopy)
                throw new InvalidOperationException("Garden change did not take effect before restore");
            Press("Restore garden file");
            for (int f = 0; f < 4; f++) yield return null;
            Press("Confirm garden restore");
            for (int f = 0; f < 4; f++) yield return null;
            if (GardenTransfer.Export(app.QaWorld, probeStamp) != beforeCopy)
                throw new InvalidOperationException("Restored garden does not match the copied garden");
            ScrollTo("Garden file");
            for (int f = 0; f < 4; f++) yield return null;
            yield return new WaitForEndOfFrame();
            SavePng("14-garden-file.png");

            Debug.Log("MOSSELORA_QA_TOUR_DONE " + saved);
            Application.Quit();
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

        private static string ReadArg(string name)
        {
            return DevArgs.Read(name);
        }
    }
}
#endif
