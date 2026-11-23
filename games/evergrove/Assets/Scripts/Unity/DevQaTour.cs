#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System;
using System.Collections;
using System.IO;
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
            for (int f = 0; f < 10; f++) yield return null;
            ScrollTo("Choose title");
            for (int f = 0; f < 4; f++) yield return null;
            yield return new WaitForEndOfFrame();
            SavePng("03-chapter1-solved-choice.png");

            for (int chapter = 0; chapter < PuzzleCatalog.Count; chapter++)
            {
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
                    Press("Memories"); // close it before the rest of the run
                    for (int f = 0; f < 4; f++) yield return null;
                }
            }
            if (!app.QaWorld.Completed) throw new InvalidOperationException("Main run did not reach the ending");
            ScrollTo("The ending");
            for (int f = 0; f < 4; f++) yield return null;
            yield return new WaitForEndOfFrame();
            SavePng("11-ending.png");

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
            throw new InvalidOperationException("No active button: " + description);
        }

        private static void Press(string name)
        {
            FindButton(n => n == name, name).onClick.Invoke();
        }

        private static void Tap(int index, int size)
        {
            string prefix = "Row " + (index / size + 1) + " column " + (index % size + 1) + " ";
            FindButton(n => n.StartsWith(prefix, StringComparison.Ordinal), prefix).onClick.Invoke();
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
            var scroll = FindFirstObjectByType<ScrollRect>();
            if (scroll == null || scroll.content == null || scroll.viewport == null)
                throw new InvalidOperationException("Scroll view missing");
            GameObject target = null;
            foreach (Transform t in FindObjectsByType<Transform>(FindObjectsInactive.Exclude, FindObjectsSortMode.None))
                if (t.name == targetName) { target = t.gameObject; break; }
            if (target == null) throw new InvalidOperationException("No visible element: " + targetName);

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

        private static string ReadArg(string name)
        {
            string[] args = Environment.GetCommandLineArgs();
            for (int i = 0; i < args.Length - 1; i++)
                if (string.Equals(args[i], name, StringComparison.Ordinal)) return args[i + 1];
            return null;
        }
    }
}
#endif
