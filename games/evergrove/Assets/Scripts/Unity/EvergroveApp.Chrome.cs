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

        private sealed class CueFade : MonoBehaviour
        {
            public float After = 2.5f;
            private float start;
            private Image image;
            private void Start() { start = Time.unscaledTime; image = GetComponent<Image>(); }
            private void Update()
            {
                if (image == null) return;
                float t = Time.unscaledTime - start - After;
                if (t <= 0f) return;
                Color c = image.color;
                c.a = Mathf.Clamp01(1f - t / 1.2f) * 0.9f;
                image.color = c;
                if (c.a <= 0f) enabled = false;
            }
        }

        private GameObject FanToken(Transform parent, string name, string caption, string icon, Action action)
        {
            Button token = IconButton(parent, name, caption, icon, action, 64, MosseloraTheme.QuietAction,
                false, null, false, 0, 0, true);
            var size = token.GetComponent<LayoutElement>();
            if (size == null) size = token.gameObject.AddComponent<LayoutElement>();
            size.minHeight = size.preferredHeight = 64f;
            size.flexibleHeight = 0f;
            return token.gameObject;
        }

        private void OpenBook(bool almanac)
        {
            CloseFan();
            CloseTitle();
            menuOpen = false;
            showJournal = true;
            almanacTab = almanac;
            familyTab = false;
            journalOpenPage = -1;
            Rebuild();
            if (journalScroll != null) journalScroll.verticalNormalizedPosition = 1f;
        }

        private static IEnumerator FanIn(GameObject token, float delay)
        {
            if (token == null) yield break;
            var group = token.AddComponent<CanvasGroup>();
            group.alpha = 0f;
            token.transform.localScale = new Vector3(0.7f, 0.7f, 1f);
            float clock = -delay;
            while (clock < 0.22f && token != null)
            {
                clock += Time.unscaledDeltaTime;
                float t = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(clock / 0.22f));
                group.alpha = t;
                token.transform.localScale = Vector3.one * Mathf.Lerp(0.7f, 1f, t);
                yield return null;
            }
        }

        // Sideways there is no top safe inset, so the chrome sits a little lower
        // and the book's new-memory mark is not clipped by the screen edge (I116).
        private float ChromeTop { get { return landscape ? -12f : -6f; } }

        // Sideways, Undo and Hint stand in a rail right of the board, so the
        // board gets the height a tool row would take below it (I116).
        private const float ToolRailWidth = BoardFit.RailWidth;
        private const float ToolRailGap = BoardFit.RailGap;

        private static void PlaceChrome(Button control, Vector2 corner, Vector2 offset)
        {
            var rect = control.GetComponent<RectTransform>();
            rect.anchorMin = rect.anchorMax = corner;
            rect.pivot = corner;
            rect.anchoredPosition = offset;
            var fit = control.gameObject.AddComponent<ContentSizeFitter>();
            fit.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
            fit.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            var layout = control.GetComponent<LayoutElement>();
            if (layout != null) layout.minWidth = Mathf.Max(layout.minWidth, 54f);
        }

        // Start a new garden: a seedling icon first; only the confirm step
        // spells out the erase, beside an always-visible cancel.
        private IEnumerator FadeInPage()
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            if (qaInMemoryOnly) yield break; // tour screenshots capture settled screens
#endif
            if (pageFade == null) pageFade = content.gameObject.AddComponent<CanvasGroup>();
            pageFade.alpha = 0f;
            for (float t = 0f; t < 1f; t += Time.unscaledDeltaTime / 0.26f)
            {
                if (pageFade == null) yield break;
                pageFade.alpha = Mathf.SmoothStep(0f, 1f, t);
                yield return null;
            }
            if (pageFade != null) pageFade.alpha = 1f;
        }

        private IEnumerator FadeAway(GameObject node)
        {
            var fade = node.GetComponent<CanvasGroup>();
            if (fade == null) fade = node.AddComponent<CanvasGroup>();
            fade.blocksRaycasts = false;
            fade.interactable = false;
            float from = fade.alpha;
            bool animate = gentleMotion;
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            if (qaInMemoryOnly) animate = false; // tour screenshots capture settled screens
#endif
            for (float t = 0f; animate && t < 1f; t += Time.unscaledDeltaTime / 0.35f)
            {
                if (fade == null) yield break;
                fade.alpha = Mathf.Lerp(from, 0f, Mathf.SmoothStep(0f, 1f, t));
                yield return null;
            }
            if (fade != null) fade.alpha = 0f;
        }

        private const float WaterUndoGap = 6f;
        private const float WaterUndoCountWidth = 36f;
        private const float WaterUndoCountHeight = 24f; // the count, stacked in the rail

        // Undo | step count | Redo: counts and token names say how far each way goes, as
        // BoardKit's undo row does ("Undo, 3 steps" / "Redo, 1 step"). A token that cannot
        // act is faded and refuses taps; it stays in place so the row keeps its shape.
        private void RefreshUndoRow()
        {
            if (undoButton == null || redoButton == null || undoStepCount == null) return;
            int undoSteps = world.UndoDepth, redoSteps = world.RedoDepth;
            undoButton.name = "Undo, " + StepWords(undoSteps, "nothing to undo");
            redoButton.name = "Redo, " + StepWords(redoSteps, "nothing to redo");
            undoStepCount.text = undoSteps.ToString();
            FadeToken(undoButton, undoSteps > 0);
            FadeToken(redoButton, redoSteps > 0);
        }

        private static string StepWords(int n, string none)
        {
            return n == 0 ? none : n == 1 ? "1 step" : n + " steps";
        }

        private static void FadeToken(Button token, bool on)
        {
            token.interactable = on;
            var group = token.GetComponent<CanvasGroup>();
            if (group == null) group = token.gameObject.AddComponent<CanvasGroup>();
            group.alpha = on ? 1f : 0.35f;
        }

        // Undo appears only once there is something to undo: before the first
        // move its place stays empty (no layout shift), then it fades in (I155).
        private void ShowUndo(bool available, bool animate)
        {
            if (undoRowNode == null) return;
            var group = undoRowNode.GetComponent<CanvasGroup>();
            if (group == null) group = undoRowNode.AddComponent<CanvasGroup>();
            group.blocksRaycasts = available;
            float target = available ? 1f : 0f;
            if (Mathf.Approximately(group.alpha, target)) return;
            bool motion = animate && gentleMotion;
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            if (qaInMemoryOnly) motion = false;
#endif
            if (!motion) { group.alpha = target; return; }
            StartCoroutine(FadeGroup(group, target, 0.25f));
        }

        private static IEnumerator FadeGroup(CanvasGroup group, float target, float seconds)
        {
            float from = group.alpha;
            for (float t = 0f; t < 1f; t += Time.unscaledDeltaTime / seconds)
            {
                if (group == null) yield break;
                group.alpha = Mathf.Lerp(from, target, Mathf.SmoothStep(0f, 1f, t));
                yield return null;
            }
            if (group != null) group.alpha = target;
        }

        // A cream disc with the turning arrow at a tile's top-right corner. It
        // turns a quarter at a time, as the tile will, and leaves with the goal
        // glows on the first move.
        private void TurnMark(Transform tile, float tileSize)
        {
            float size = Mathf.Round(Mathf.Clamp(tileSize * 0.42f, 30f, 44f));
            var mark = Node("Tap turns this tile", tile);
            goalGlows.Add(mark);
            mark.AddComponent<LayoutElement>().ignoreLayout = true;
            var rect = mark.GetComponent<RectTransform>();
            rect.anchorMin = rect.anchorMax = new Vector2(1f, 1f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = new Vector2(size, size);
            rect.anchoredPosition = new Vector2(-size * 0.3f, -size * 0.3f);
            var disc = mark.AddComponent<Image>();
            MosseloraArt.Round(disc, MosseloraTheme.CardFill, Mathf.RoundToInt(size / 2f));
            disc.raycastTarget = false;
            MosseloraArt.Lift(disc, 1.5f, 0.25f);
            var glyph = MosseloraArt.Ornament(mark.transform, "Turning arrow", MosseloraArt.Load(MosseloraArt.IconTurn),
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(size * 0.66f, size * 0.66f), Vector2.zero);
            if (glyph == null) return;
            glyph.color = MosseloraTheme.WaterChannel;
            if (gentleMotion) glyph.gameObject.AddComponent<QuarterTurns>();
            // A painted finger rests on the tile and presses it in time with the
            // arrow's turn: the board's own "how to play", without words (I147).
            Sprite tap = MosseloraArt.Load(MosseloraArt.IconTap);
            if (tap == null) return;
            var hand = Node("Shows a tap here", tile);
            goalGlows.Add(hand);
            hand.AddComponent<LayoutElement>().ignoreLayout = true;
            var handRect = hand.GetComponent<RectTransform>();
            // The whole hand sits in the tile's lower-right quarter, which a channel
            // never crosses (arms run along the centre lines): it presses the tile
            // without hiding the pipe (I159), and stays inside the tile, under no
            // neighbour.
            handRect.anchorMin = handRect.anchorMax = new Vector2(0.74f, 0.36f);
            handRect.pivot = new Vector2(0.35f, 0.9f); // the fingertip
            float handSize = Mathf.Round(tileSize * 0.36f);
            handRect.sizeDelta = new Vector2(handSize, handSize);
            handRect.anchoredPosition = Vector2.zero;
            var handImage = hand.AddComponent<Image>();
            handImage.sprite = tap;
            handImage.preserveAspect = true;
            handImage.raycastTarget = false;
            handImage.color = new Color(Ink.r, Ink.g, Ink.b, 0.82f);
            if (gentleMotion) hand.AddComponent<GhostPress>();
        }

        // Holds a row of primary actions at the foot of the screen over a soft paper
        // fade, so "continue" is always in reach at any text size; the page keeps
        // an empty space of the same height so its end can scroll clear (I164).
        private void PinToFoot(Transform row)
        {
            if (row == null || scrollArea == null) return;
            LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)row);
            float rowHeight = Mathf.Max(64f, LayoutUtility.GetPreferredHeight((RectTransform)row));
            float barHeight = rowHeight + 30f;
            pinnedActions = Node("Pinned actions", scrollArea);
            pinnedActions.transform.SetSiblingIndex(1); // above the page, under menus and chrome
            var bar = pinnedActions.GetComponent<RectTransform>();
            bar.anchorMin = new Vector2(0f, 0f);
            bar.anchorMax = new Vector2(1f, 0f);
            bar.pivot = new Vector2(0.5f, 0f);
            bar.sizeDelta = new Vector2(0f, barHeight + 26f);
            bar.anchoredPosition = Vector2.zero;
            // Only over the page's column: sideways, the garden painting on the left
            // stays uncovered (I170: the fade had blanked its lower half).
            bar.offsetMin = new Vector2(columnLeft, bar.offsetMin.y);
            bar.offsetMax = new Vector2(-columnRight, bar.offsetMax.y);
            var fade = Node("Paper fade", pinnedActions.transform);
            var fadeRect = fade.GetComponent<RectTransform>();
            Fill(fadeRect);
            fadeRect.localScale = new Vector3(1f, -1f, 1f); // TopFade, flipped: solid at the foot
            var fadeImage = fade.AddComponent<Image>();
            fadeImage.sprite = MosseloraArt.TopFade();
            fadeImage.color = Paper;
            fadeImage.raycastTarget = false;
            var solid = Node("Paper", pinnedActions.transform);
            var solidRect = solid.GetComponent<RectTransform>();
            solidRect.anchorMin = Vector2.zero;
            solidRect.anchorMax = new Vector2(1f, 0f);
            solidRect.pivot = new Vector2(0.5f, 0f);
            solidRect.sizeDelta = new Vector2(0f, barHeight * 0.62f);
            var solidImage = solid.AddComponent<Image>();
            solidImage.color = Paper;
            solidImage.raycastTarget = false;
            var keep = Node("Space for the pinned actions", content);
            MinHeight(keep, barHeight);
            int index = row.GetSiblingIndex();
            row.SetParent(pinnedActions.transform, false);
            var layout = row.GetComponent<LayoutElement>();
            if (layout == null) layout = row.gameObject.AddComponent<LayoutElement>();
            layout.ignoreLayout = true;
            var rect = (RectTransform)row;
            rect.anchorMin = new Vector2(0f, 0f);
            rect.anchorMax = new Vector2(1f, 0f);
            rect.pivot = new Vector2(0.5f, 0f);
            rect.sizeDelta = new Vector2(-34f, rowHeight);
            rect.anchoredPosition = new Vector2(0f, 14f);
            keep.transform.SetSiblingIndex(Mathf.Min(index, content.childCount - 1));
        }
    }
}
