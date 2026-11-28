using Evergrove.Core;
using UnityEngine;

namespace Evergrove.UnityClient
{
    /// <summary>
    /// Checklist 18: with no touch for 6 s on the home garden, the corner chrome (menu, seed purse,
    /// open basket tray) fades to 20% so the garden fills the screen. Any touch brings it back at
    /// once. Alpha only: the faded controls keep their raycasts, so the first tap on one both wakes
    /// the chrome and performs its action. Sheets, boards, letters and visits count as in use.
    /// </summary>
    public sealed partial class EvergroveApp
    {
        // Seconds the QA tour holds the chrome idle (any value past IdleAfterSeconds works).
        private const float QaIdleSeconds = 30f;

        // Chrome that fades, besides the menu bar, all children of the home screen.
        private static readonly string[] IdleHudNames = { "Hub chrome", "Seeds you have", "Basket corner" }; // Hub chrome: the home menu button (tour found it unfaded)

        private float idleTouchAt = -1f; // Time.unscaledTime of the last touch
        private bool? idleQaOverride;    // QaIdle(): forces idle on or off for the tour; a real touch clears it

        // Runs every frame from Update(). Sets the chrome alpha; never changes active state.
        private void TickIdleFade()
        {
            float now = Time.unscaledTime;
            if (idleTouchAt < 0f) idleTouchAt = now;
            if (IdleTouching())
            {
                idleTouchAt = now;
                idleQaOverride = null;
            }
            float seconds = idleQaOverride.HasValue
                ? (idleQaOverride.Value ? QaIdleSeconds : 0f)
                : now - idleTouchAt;
            float alpha = IdleInUse() ? 1f : IdleFade.AlphaAt(seconds, !gentleMotion);
            ApplyIdleAlpha(alpha);
        }

        // QA tour hook: forces the chrome idle (true) or awake (false) without waiting six seconds.
        internal void QaIdle(bool idle)
        {
            idleQaOverride = idle;
            idleTouchAt = Time.unscaledTime;
            TickIdleFade();
        }

        private static bool IdleTouching()
        {
            return Input.touchCount > 0 || Input.GetMouseButton(0);
        }

        // A sheet, board, letter, visit or basket tray is open: the chrome stays at full strength.
        private bool IdleInUse()
        {
            if (titleScreen == null || canvasRoot == null) return true;
            if (menuOpen || showJournal || fan != null || letterCard != null || basketOpen) return true;
            if (quietLayer != null || neighborLayer != null || confirmReset || gardenRestoreConfirm) return true;
            return IdleSheetOpen(titleScreen.transform) || IdleSheetOpen(canvasRoot);
        }

        // Open sheets, letters and visits are named "... sheet", "Seed book", "Reading a letter",
        // "Visiting a garden". Only active direct children count.
        private static bool IdleSheetOpen(Transform parent)
        {
            for (int i = 0; i < parent.childCount; i++)
            {
                Transform child = parent.GetChild(i);
                if (!child.gameObject.activeSelf) continue;
                string name = child.name.ToLowerInvariant();
                if (name.Contains("sheet") || name.Contains("book") || name.Contains("reading") || name.Contains("visit"))
                    return true;
            }
            return false;
        }

        private void ApplyIdleAlpha(float alpha)
        {
            if (chrome != null) SetIdleAlpha(chrome, alpha);
            if (titleScreen == null) return;
            foreach (string name in IdleHudNames)
            {
                Transform hud = titleScreen.transform.Find(name);
                if (hud != null) SetIdleAlpha(hud, alpha);
            }
        }

        // Adds a CanvasGroup only when the chrome must dim; full alpha needs none.
        private static void SetIdleAlpha(Transform hud, float alpha)
        {
            var group = hud.GetComponent<CanvasGroup>();
            if (group == null)
            {
                if (alpha >= 1f) return;
                group = hud.gameObject.AddComponent<CanvasGroup>();
            }
            if (group.alpha != alpha) group.alpha = alpha;
        }
    }
}
