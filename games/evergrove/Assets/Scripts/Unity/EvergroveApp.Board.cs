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

        private void RefreshBoard()
        {
            semanticsDirty = true;
            UpdateCreekSound();
            // The first move is the lesson learned: the bubble steps aside.
            // It fades where it stands and keeps its space, so the board never jumps
            // under the player's finger on their very first move (I131).
            if (coachBubble != null && world.CanUndo)
            {
                if (coachBubble.transform.parent == content)
                    spentTipHeight = ((RectTransform)coachBubble.transform).rect.height;
                StartCoroutine(FadeAway(coachBubble));
                coachBubble = null;
            }
            if (goalGlows.Count > 0 && world.CanUndo)
            {
                foreach (GameObject glow in goalGlows) if (glow != null) glow.SetActive(false);
                goalGlows.Clear();
            }
            if (world.Completed || visibleTiles == null) return;
            // A move ends the hint; its rings go with it instead of lingering (I91).
            if (hintMarkers.Count > 0 && world.HintStep == 0)
            {
                foreach (GameObject marker in hintMarkers) if (marker != null) marker.SetActive(false);
                hintMarkers.Clear();
            }
            Puzzle puzzle = world.CurrentPuzzle;
            if (visibleTiles.Length != puzzle.CellCount) { Rebuild(); return; }
            PipeInsight insight = world.Insight;
            int[] angles = world.Rotations;
            // Newly wet tiles fill in order along the water's path from the spring,
            // one short beat apart, so the player watches the water run (I144).
            float[] arrive = WaterArrival(puzzle, angles, insight);
            for (int i = 0; i < puzzle.CellCount; i++)
            {
                bool newlyWet = lastFlowing != null && i < lastFlowing.Length &&
                    insight.Flowing[i] && !lastFlowing[i];
                float delay = arrive != null && newlyWet ? arrive[i] : 0f;
                // Water just arrived here: the tile gives one small, soft swell.
                if (gentleMotion && newlyWet)
                    BloomPulse.Add(tileArtwork[i].gameObject, 0.04f + delay); // the pipe, not the pressed tile
                visibleTiles[i].interactable = !world.Solved;
                // Open ports are geometry, not water: always current.
                if (visibleLeaks != null && visibleLeaks[i] != null)
                    visibleLeaks[i].text = insight.Leaking[i] ? "○" : "";
                // A tile the running water hasn't reached yet stays dry for now;
                // WetLater repaints it from the live insight when it arrives (I144).
                PaintRefreshedTile(puzzle, insight, angles, i, delay > 0f);
                if (delay > 0f) StartCoroutine(WetLater(i, delay));
            }
            PaintDropsWithFlights(puzzle, insight, arrive);
            if (creekWater != null && !world.Solved)
            {
                float share = CreekShare();
                if (!Mathf.Approximately(share, creekShown)) StartCoroutine(FillCreek(creekWater, share));
            }
            lastFlowing = (bool[])insight.Flowing.Clone();
            if (feedbackText != null)
                CjkLineBreakFixer.SetText(feedbackText, GameLocalization.Text("progress",
                    insight.ConnectedCount, puzzle.CellCount, insight.LeakCount));
            if (undoButton != null)
            {
                RefreshUndoRow();
                ShowUndo(world.CanUndo || world.CanRedo, true);
            }
            if (activeIllustration != null || activePainting != null)
            {
                // The world responds to meaningful changes in reachable water
                // without rebuilding the puzzle or moving the scroll position.
                VillageScenePlan plan = VillageScenePlan.From(world);
                if (activeIllustration != null)
                    activeIllustration.Show(plan.Chapter, plan.TreeCount,
                        plan.CrossingCount, plan.FlowerCount,
                        plan.WaterRestored, plan.WaterReachPercent);
                if (activePainting != null)
                    activePainting.Show(plan.Chapter, plan.TreeCount,
                        plan.CrossingCount, plan.FlowerCount,
                        plan.WaterRestored, plan.WaterReachPercent);
            }
        }

        private void AfterPuzzleAction(bool wasSolved)
        {
            SaveWorld();
            if (wasSolved != world.Solved)
            {
                // Completion changes the available choices and navigation,
                // so this is the one moment that merits rebuilding the view.
                celebrateSolve = !wasSolved && world.Solved;
                Rebuild();
                if (!wasSolved && world.Solved)
                {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                    if (localPlaytest != null) localPlaytest.Solve(Time.unscaledTime);
#endif
                    PlayChime();
                    Haptics.Finished();
                    StartCoroutine(ScrollToDecision());
                }
            }
            else if (saveFailed) Rebuild();
            else RefreshBoard();
        }

        private void UseGentleHint()
        {
            bool wasSolved = world.Solved;
            int target = world.HintTile;
            int stage = world.AdvanceGentleHint();
            if (stage == 0) return;
            // Step 3 turns the tile for you; it swells once so the change is seen (I92).
            hintTurnedTile = stage == 3 ? target : -1;
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            if (localPlaytest != null) localPlaytest.Hint(Time.unscaledTime);
#endif
            SaveWorld();
            celebrateSolve = !wasSolved && world.Solved; // a hint that finishes the creek blooms too
            Rebuild();
            if (!wasSolved && world.Solved)
            {
                PlayChime();
                StartCoroutine(ScrollToDecision());
            }
        }

        private void TapTile(int index)
        {
            if (!world.Solved && world.CurrentPuzzle.IsAnchored(index))
            {
                // Explains the landmark in the status line only: no move, no undo
                // entry, no save. The next real action redraws the progress text.
                string landmark = LandmarkCause.For(world);
                if (feedbackText != null && landmark.Length > 0) CjkLineBreakFixer.SetText(feedbackText, landmark);
                // A gentle "this one stays" shake of the rooted pipe: felt, not scolded.
                if (gentleMotion && tileArtwork != null && index < tileArtwork.Length)
                    tileArtwork[index].gameObject.AddComponent<Nudge>();
                if (visibleTiles != null && index < visibleTiles.Length && landmark.Length > 0)
                    RuleToast.Show(visibleTiles[index].GetComponent<RectTransform>(), landmark, font, Ink,
                        MosseloraTheme.CardFill);
                semanticsDirty = true;
                return;
            }
            bool before = world.Solved;
            if (!world.Rotate(index)) return;
            PlayTock();
            Haptics.Placed(); // felt with the turn (I140)
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            if (localPlaytest != null) localPlaytest.Rotate(Time.unscaledTime);
#endif
            AfterPuzzleAction(before);
        }

        private void Act(Func<bool> action, string actionKind)
        {
            bool before = world.Solved;
            if (!action()) return;
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            if (localPlaytest != null)
            {
                switch (actionKind)
                {
                    case "undo": localPlaytest.Undo(Time.unscaledTime); break;
                    case "hint": localPlaytest.Hint(Time.unscaledTime); break;
                    case "restart": localPlaytest.Restart(Time.unscaledTime); break;
                }
            }
#endif
            AfterPuzzleAction(before);
        }

        private IEnumerator ScrollToDecision()
        {
            // Bottom-of-page scrolling skips the actual decision and lands
            // on secondary settings/footer on compact screens. Instead show
            // the first choice header with some context above it.
            yield return null; // allow one layout frame after full completion
            if (scroll == null || decisionHeading == null || content == null ||
                scroll.viewport == null) yield break;
            Canvas.ForceUpdateCanvases();
            LayoutRebuilder.ForceRebuildLayoutImmediate(content);
            float distance = content.rect.height - scroll.viewport.rect.height;
            if (distance <= 0f) yield break;
            Vector3 worldTop = decisionHeading.TransformPoint(
                new Vector3(decisionHeading.rect.center.x, decisionHeading.rect.yMax, 0f));
            float targetTop = content.rect.yMax -
                content.InverseTransformPoint(worldTop).y;
            // Keep some of the preceding restored-world context visible.
            float context = Mathf.Min(110f, scroll.viewport.rect.height * 0.23f);
            float stopAt = targetTop - context;
            // Better: the whole finished creek above all three choices, so the
            // board is never cut in half under the menu button (I74). Only when
            // both fit on this screen; larger text keeps the heading anchor.
            if (solvedBoard != null && decisionCards != null)
            {
                float boardTop = DepthInContent(solvedBoard, true) - 10f; // painted bed overhang
                float cardsBottom = DepthInContent(decisionCards, false);
                float boardFirst = boardTop - ChromeClearance() - 6f;
                if (cardsBottom - boardFirst <= scroll.viewport.rect.height - 8f)
                    stopAt = boardFirst;
            }
            float target = 1f - Mathf.Clamp01(stopAt / distance);
            int generation = rebuildGeneration;
            // The payoff beat: hold on the finished creek while it blooms and
            // every droplet fills, then glide (not jump) to the decision. A
            // player who scrolls during the hold keeps control.
            float held = scroll.verticalNormalizedPosition;
            float wait = 0f;
            while (wait < SolveHoldSeconds)
            {
                wait += Time.unscaledDeltaTime;
                yield return null;
                // Any rebuild (choice, sound, text size) or a player scroll cancels the glide.
                if (scroll == null || decisionHeading == null || generation != rebuildGeneration) yield break;
                if (Mathf.Abs(scroll.verticalNormalizedPosition - held) > 0.01f) yield break;
            }
            scroll.StopMovement();
            float from = scroll.verticalNormalizedPosition;
            for (float t = gentleMotion ? 0f : 1f; t < 1f; t += Time.unscaledDeltaTime / SolveGlideSeconds)
            {
                if (scroll == null || decisionHeading == null || generation != rebuildGeneration) yield break;
                scroll.verticalNormalizedPosition = Mathf.Lerp(from, target, Mathf.SmoothStep(0f, 1f, t));
                yield return null;
            }
            if (scroll != null) scroll.verticalNormalizedPosition = target;
        }

        // Distance from the top of the page to a node's top (or bottom) edge.
        private float DepthInContent(RectTransform node, bool top)
        {
            Vector3 edge = node.TransformPoint(new Vector3(node.rect.center.x,
                top ? node.rect.yMax : node.rect.yMin, 0f));
            return content.rect.yMax - content.InverseTransformPoint(edge).y;
        }

        // How far the floating menu button reaches into the scrolling viewport.
        private float ChromeClearance()
        {
            if (chrome == null || chrome.childCount == 0 || scroll.viewport == null) return 0f;
            var menu = (chrome.Find("Open menu") ?? chrome.GetChild(0)) as RectTransform;
            if (menu == null) return 0f;
            Vector3 bottom = menu.TransformPoint(new Vector3(menu.rect.center.x, menu.rect.yMin, 0f));
            float reach = scroll.viewport.rect.yMax - scroll.viewport.InverseTransformPoint(bottom).y;
            return Mathf.Clamp(reach, 0f, 160f);
        }

        private const float SolveHoldSeconds = 1.25f;
        private const float SolveGlideSeconds = 0.6f;

        // The chosen card lifts and the others step back for a quarter second,
        // so a choice is seen to be taken before the memory page arrives (I142).
        private bool settlingChoice;

        private void SettleChoice(Transform cards, VillageChoice choice)
        {
            if (settlingChoice) return;
            bool animate = gentleMotion && cards != null;
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            if (qaInMemoryOnly) animate = false; // the tour chooses instantly
#endif
            if (!animate) { MakeChoice(choice); return; }
            StartCoroutine(LiftChosen(cards, choice));
        }

        private IEnumerator LiftChosen(Transform cards, VillageChoice choice)
        {
            settlingChoice = true;
            int generation = rebuildGeneration;
            Transform chosen = cards.Find("Choice " + choice);
            var groups = new System.Collections.Generic.List<CanvasGroup>();
            for (int i = 0; i < cards.childCount; i++)
            {
                Transform card = cards.GetChild(i);
                var group = card.GetComponent<CanvasGroup>();
                if (group == null) group = card.gameObject.AddComponent<CanvasGroup>();
                group.interactable = false;
                if (card != chosen) groups.Add(group);
            }
            var squash = chosen != null ? chosen.GetComponent<PressSquash>() : null;
            if (squash != null) Destroy(squash); // the lift owns the scale now
            for (float t = 0f; t < 1f; t += Time.unscaledDeltaTime / 0.24f)
            {
                if (generation != rebuildGeneration) { settlingChoice = false; yield break; }
                float k = Mathf.SmoothStep(0f, 1f, t);
                if (chosen != null) chosen.localScale = Vector3.one * Mathf.Lerp(0.94f, 1.07f, k);
                foreach (CanvasGroup group in groups) if (group != null) group.alpha = Mathf.Lerp(1f, 0.35f, k);
                yield return null;
            }
            settlingChoice = false;
            if (generation == rebuildGeneration) MakeChoice(choice);
        }

        private void MakeChoice(VillageChoice choice)
        {
            if (!world.Choose(choice)) return;
            ClearChoicePreview();
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            if (localPlaytest != null) localPlaytest.Choose(Time.unscaledTime);
#endif
            SaveWorld();
            RecordInAlmanac();
            showingMemory = true;
            showJournal = false;
            speakingWith = -1;
            PlayChime();
            celebrateGrowth = gentleMotion;
            Rebuild(true);
        }

        // Player-owned copy of the garden. Clipboard only; nothing leaves the device.
        // Lives in settings, below the puzzle, and never on the primary path.
        // Spoiler-free text postcard for a friend (vision 4.4 rung 1). Clipboard only.
        private void BuildPostcard()
        {
            if (world.Mode != PlayMode.Living) return;
            var card = Card(content, "Garden postcard", MosseloraTheme.CardFill);
            Transform row = CenteredRowIn(card.transform, "Postcard row");
            Width(IconButton(row, "Copy garden postcard", GameLocalization.Text("postcard"),
                MosseloraArt.IconChat, () => {
                    string text = GardenPostcard.Build(world,
                        world.Completed ? "" : GameLocalization.Text("stream", world.Round + 1, PuzzleCatalog.Count));
                    if (NativeShare.Text(text, GameLocalization.Text("postcard"))) return; // share sheet (I149)
                    GUIUtility.systemCopyBuffer = text;
                    postcardCopied = true;
                    Rebuild();
                }, 78, MosseloraTheme.WaterAction, false, null, true), 240);
            if (postcardCopied)
                Label(card.transform, "Postcard notice", GameLocalization.Text("postcard_copied"), 15, 0, Ink,
                    TextAnchor.MiddleCenter);
        }
    }
}
