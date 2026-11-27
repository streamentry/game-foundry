using System;
using System.Collections;
using Evergrove.Core;
using UnityEngine;

namespace Evergrove.UnityClient
{
    /// <summary>
    /// Sheets slide in from the edge they belong to and slide back out (checklist 17).
    /// Only the panel's anchoredPosition and the sheet's CanvasGroup.alpha move, so layout is
    /// never touched while a sheet travels: callers fit the sheet before showing it.
    /// Reduced motion keeps the same timing but replaces each slide with a fade in place.
    /// </summary>
    internal static class SheetMotion
    {
        internal const float SlideSeconds = 0.28f;
        internal const float FadeSeconds = 0.18f;

        // The dev QA tour sets this so screenshots capture settled sheets, not mid-flight ones.
        internal static bool Settled;

        /// <summary>Opens a sheet from its edge. An open or opening sheet is left alone, so rebuilds don't re-enter.</summary>
        internal static void Show(GameObject sheet, SheetEdge.Side from, bool reducedMotion,
                                  RectTransform panel = null, bool instant = false)
        {
            if (sheet == null) return;
            SheetMotionPlayer player = PlayerOf(sheet);
            player.Configure(panel != null ? panel : PanelOf(sheet), from, reducedMotion);
            if (sheet.activeSelf && player.Target == 1f) return;
            sheet.SetActive(true);
            player.Play(1f, instant, null);
        }

        /// <summary>Closes a sheet toward its edge, then deactivates it and runs done (e.g. Destroy).</summary>
        internal static void Hide(GameObject sheet, SheetEdge.Side from, bool reducedMotion, Action done = null,
                                  RectTransform panel = null, bool instant = false)
        {
            if (sheet == null) return;
            if (!sheet.activeSelf)
            {
                if (done != null) done();
                return;
            }
            SheetMotionPlayer player = PlayerOf(sheet);
            player.Configure(panel != null ? panel : PanelOf(sheet), from, reducedMotion);
            player.Play(0f, instant, done);
        }

        /// <summary>Shows or hides a persistent sheet; a state that is already reached does nothing.</summary>
        internal static void Set(GameObject sheet, bool open, SheetEdge.Side from, bool reducedMotion)
        {
            if (open) Show(sheet, from, reducedMotion);
            else Hide(sheet, from, reducedMotion);
        }

        private static SheetMotionPlayer PlayerOf(GameObject sheet)
        {
            SheetMotionPlayer player = sheet.GetComponent<SheetMotionPlayer>();
            if (player == null) player = sheet.AddComponent<SheetMotionPlayer>();
            if (player.Fade == null)
            {
                player.Fade = sheet.GetComponent<CanvasGroup>();
                if (player.Fade == null) player.Fade = sheet.AddComponent<CanvasGroup>();
            }
            return player;
        }

        // Sheets built by BuildSheet name their panel "<name> panel".
        private static RectTransform PanelOf(GameObject sheet)
        {
            foreach (Transform child in sheet.transform)
                if (child.name.EndsWith(" panel", StringComparison.Ordinal)) return child as RectTransform;
            return null;
        }
    }

    /// <summary>Runs one sheet's motion. Lives on the sheet root, so it stops with the sheet.</summary>
    internal sealed class SheetMotionPlayer : MonoBehaviour
    {
        internal CanvasGroup Fade;
        internal float Target;          // 0 closed, 1 open: where the sheet is heading
        internal Action Done;           // runs once a closing sheet has been put away

        private float progress;         // 0 closed .. 1 open, eased
        private bool reduced;
        private SheetEdge.Side side;
        private RectTransform panel;
        private Vector2 rest;           // the panel's anchoredPosition when it is not moving
        private Vector2 shift;          // the offset last applied to the panel
        private Coroutine running;

        internal void Configure(RectTransform target, SheetEdge.Side edge, bool reducedMotion)
        {
            if (target != panel)
            {
                panel = target;
                shift = Vector2.zero;
                rest = panel != null ? panel.anchoredPosition : Vector2.zero;
            }
            else if (running == null && panel != null && (panel.anchoredPosition - (rest + shift)).sqrMagnitude > 0.0001f)
            {
                // Something else (a refit) moved the panel since we last did: take its place as rest.
                rest = panel.anchoredPosition;
                shift = Vector2.zero;
            }
            side = edge;
            reduced = reducedMotion;
        }

        internal void Play(float target, bool instant, Action done)
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            if (SheetMotion.Settled) instant = true;
#endif
            if (running != null && target == Target)
            {
                Done += done; // already on its way there: just remember the callback
                return;
            }
            if (running != null) StopCoroutine(running);
            running = null;
            Target = target;
            Done = target == 0f ? done : null;
            // Blocking stops the moment a sheet starts to leave, so taps fall through to what is underneath.
            if (Fade != null)
            {
                Fade.blocksRaycasts = target == 1f;
                Fade.interactable = target == 1f;
            }

            float start = progress;
            if (instant || Mathf.Approximately(start, target))
            {
                Finish(target);
                return;
            }
            float seconds = (reduced ? SheetMotion.FadeSeconds : SheetMotion.SlideSeconds) * Mathf.Abs(target - start);
            Apply(start); // the first frame sits where the sheet already is
            running = StartCoroutine(Run(start, target, seconds));
        }

        private IEnumerator Run(float start, float target, float seconds)
        {
            yield return null; // the caller fits the layout first; motion starts next frame
            float elapsed = 0f;
            while (elapsed < seconds)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / seconds);
                Apply(Mathf.Lerp(start, target, SheetEdge.Ease(t)));
                yield return null;
            }
            running = null;
            Finish(target);
        }

        private void Finish(float target)
        {
            Apply(target);
            if (target != 0f) return;
            Action done = Done;
            Done = null;
            gameObject.SetActive(false);
            if (done != null) done();
        }

        // progress is what this sheet shows: alpha is the progress, and the slide is the rest of the way.
        private void Apply(float p)
        {
            progress = p;
            if (Fade != null) Fade.alpha = p;
            if (panel == null) return;
            shift = reduced ? Vector2.zero : Direction(side) * ((1f - p) * Distance());
            panel.anchoredPosition = rest + shift;
        }

        private float Distance()
        {
            Rect r = panel.rect;
            bool vertical = side == SheetEdge.Side.Bottom || side == SheetEdge.Side.Top;
            return (vertical ? r.height : r.width) + 24f; // a little past its own size: fully off the edge
        }

        private static Vector2 Direction(SheetEdge.Side edge)
        {
            switch (edge)
            {
                case SheetEdge.Side.Top: return Vector2.up;
                case SheetEdge.Side.Left: return Vector2.left;
                case SheetEdge.Side.Right: return Vector2.right;
                default: return Vector2.down;
            }
        }

        private void OnDisable()
        {
            running = null; // a deactivated sheet has no coroutine left to finish it
        }
    }
}
