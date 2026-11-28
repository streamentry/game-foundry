using System;
using Evergrove.Core;
using UnityEngine;
using UnityEngine.UI;

namespace Evergrove.UnityClient
{
    /// <summary>
    /// Checklist 38, chains between puzzles (visible only). After one of your own stream path or
    /// plots boards is solved, the card's foot names the cause in one short line, with a small
    /// motion icon (a water drop, a sprout). Stream path: the day's watered mark appears under the
    /// things holding the plots puzzle on the hub map. Device-local day records only; no seeds, no
    /// stock, no save change. Reduced motion: the line shows at once and nothing drifts.
    /// </summary>
    internal static class ChainCause
    {
        // The line appears ShowAfter seconds after the solve and fades in over FadeTime,
        // inside TreePlay's settle (1.0 s).
        internal const float ShowAfter = 0.35f;
        internal const float FadeTime = 0.35f;
        internal const float DriftTime = 1.4f;
        internal const float DriftDistance = 14f;
        private const string WateredKey = "evergrove:chain-watered:";
        private const string SeedlingKey = "evergrove:chain-seedling:";

        // Set by the app just before a board card opens (PuzzleCard); read when it is solved.
        private static PuzzleKind opened;
        private static bool openedOwn, openedGardenHasPlots, openedNurseryOpen;
        private static int openedGarden;

        /// <summary>Unscaled time of the last board solve that reached the card; the tour measures from it.</summary>
        internal static float SolvedAt = -1f;
        /// <summary>Unscaled time the last cause line first showed (-1: none). The tour measures to it.</summary>
        internal static float ShownAt = -1f;

        /// <summary>Called by the app when a board card opens: what this board belongs to.</summary>
        internal static void Opening(PuzzleKind kind, bool own, bool gardenHasPlots, bool nurseryOpen, int garden)
        {
            opened = kind;
            openedOwn = own;
            openedGardenHasPlots = gardenHasPlots;
            openedNurseryOpen = nurseryOpen;
            openedGarden = garden;
        }

        /// <summary>Called by TreePlay when a board is solved: records the day, then shows the cause line.</summary>
        internal static void Show(RectTransform card, Font font, bool gentle)
        {
            SolvedAt = Time.unscaledTime;
            ShownAt = -1f;
            if (!openedOwn) return; // a relative's or the well's board is not about this garden
            if (opened == PuzzleKind.StreamPath) PlayerPrefs.SetInt(WateredKey + openedGarden, Today());
            if (opened == PuzzleKind.Plots) PlayerPrefs.SetInt(SeedlingKey + openedGarden, Today());
            if (opened == PuzzleKind.StreamPath || opened == PuzzleKind.Plots) PlayerPrefs.Save();
            string cause = PuzzleChain.CauseFor(opened, openedGardenHasPlots, openedNurseryOpen);
            if (cause == null || card == null) return;
            Sprite icon = MosseloraArt.Load(opened == PuzzleKind.StreamPath ? MosseloraArt.Droplet : TreePlay.SeedIcon);
            BuildLine(card, cause, icon, font, gentle);
        }

        private static void BuildLine(RectTransform card, string cause, Sprite sprite, Font font, bool gentle)
        {
            var node = new GameObject("Chain cause", typeof(RectTransform), typeof(CanvasGroup), typeof(LayoutElement));
            node.transform.SetParent(card, false);
            node.GetComponent<LayoutElement>().ignoreLayout = true;
            var rect = (RectTransform)node.transform;
            rect.anchorMin = new Vector2(0f, 0f);
            rect.anchorMax = new Vector2(1f, 0f);
            rect.pivot = new Vector2(0.5f, 0f);
            rect.offsetMin = new Vector2(26f, 22f);
            rect.offsetMax = new Vector2(-26f, 86f);
            var fill = node.AddComponent<Image>();
            MosseloraArt.Round(fill, MosseloraTheme.LeafSoft, 18);
            fill.raycastTarget = false;

            var icon = new GameObject("Cause icon", typeof(RectTransform));
            icon.transform.SetParent(node.transform, false);
            var iconRect = (RectTransform)icon.transform;
            iconRect.anchorMin = iconRect.anchorMax = new Vector2(0f, 0.5f);
            iconRect.pivot = new Vector2(0.5f, 0.5f);
            iconRect.sizeDelta = new Vector2(28f, 28f);
            iconRect.anchoredPosition = new Vector2(24f, 0f);
            var iconImage = icon.AddComponent<Image>();
            iconImage.sprite = sprite;
            iconImage.preserveAspect = true;
            iconImage.color = MosseloraTheme.WaterAction;
            iconImage.raycastTarget = false;

            var label = new GameObject("Cause line", typeof(RectTransform));
            label.transform.SetParent(node.transform, false);
            var labelRect = (RectTransform)label.transform;
            labelRect.anchorMin = Vector2.zero;
            labelRect.anchorMax = Vector2.one;
            labelRect.offsetMin = new Vector2(48f, 4f);
            labelRect.offsetMax = new Vector2(-12f, -4f);
            var text = label.AddComponent<Text>();
            text.font = font;
            text.fontSize = TextScale.Scale(16);
            text.color = MosseloraTheme.Ink;
            text.alignment = TextAnchor.MiddleLeft;
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Truncate;
            text.raycastTarget = false;
            text.text = cause;

            var line = node.AddComponent<ChainLine>();
            line.Begin(node.GetComponent<CanvasGroup>(), iconRect, gentle);
        }

        /// <summary>The garden's watered mark: a soft wet ring and a small drop under a plots thing, today only.</summary>
        internal static void Glisten(Transform land, Vector2 at, float cellW, float cellH, int kind, bool watered)
        {
            if (!watered || kind != (int)PuzzleKind.Plots + 1 || land == null) return;
            var wet = new GameObject("Watered", typeof(RectTransform));
            wet.transform.SetParent(land, false);
            var rect = (RectTransform)wet.transform;
            rect.anchorMin = rect.anchorMax = new Vector2(0f, 1f);
            rect.sizeDelta = new Vector2(cellW * 1.2f, cellH * 0.5f);
            rect.anchoredPosition = at + new Vector2(0f, cellH * 0.06f);
            var ring = wet.AddComponent<Image>();
            ring.sprite = MosseloraArt.Ring(16);
            ring.type = Image.Type.Sliced;
            ring.color = new Color(0.36f, 0.56f, 0.62f, 0.45f);
            ring.raycastTarget = false;

            var drop = new GameObject("Watered drop", typeof(RectTransform));
            drop.transform.SetParent(wet.transform, false);
            var dropRect = (RectTransform)drop.transform;
            dropRect.anchorMin = dropRect.anchorMax = new Vector2(0.85f, 0.5f);
            dropRect.sizeDelta = new Vector2(cellW * 0.22f, cellW * 0.22f);
            var dropImage = drop.AddComponent<Image>();
            dropImage.sprite = MosseloraArt.Load(MosseloraArt.Droplet);
            dropImage.preserveAspect = true;
            dropImage.color = new Color(0.36f, 0.66f, 0.74f, 0.8f);
            dropImage.raycastTarget = false;
        }

        /// <summary>Whether a garden was watered today (stream path solved today), read on this device.</summary>
        internal static bool WateredNow(int garden)
        {
            return PuzzleChain.WateredToday(PlayerPrefs.GetInt(WateredKey + garden, 0), Today());
        }

        /// <summary>Whether a plots puzzle was solved today in this garden (the nursery's fresh-sprout mark).</summary>
        internal static bool SeedlingNow(int garden)
        {
            return PuzzleChain.SeedlingToday(PlayerPrefs.GetInt(SeedlingKey + garden, 0), Today());
        }

        private static int Today() { return PuzzleChain.DayStamp(DateTime.Now); }
    }

    /// <summary>The cause line: fades in after ChainCause.ShowAfter; with motion, its icon drifts up a little.</summary>
    internal sealed class ChainLine : MonoBehaviour
    {
        private CanvasGroup group;
        private RectTransform drift;
        private float elapsed;

        internal void Begin(CanvasGroup fade, RectTransform icon, bool gentle)
        {
            group = fade;
            drift = gentle ? icon : null;
            group.alpha = gentle ? 0f : 1f;
            if (!gentle) ChainCause.ShownAt = Time.unscaledTime;
        }

        private void Update()
        {
            elapsed += Time.unscaledDeltaTime;
            if (group == null) { Destroy(this); return; }
            if (drift != null)
            {
                float fade = Mathf.Clamp01((elapsed - ChainCause.ShowAfter) / ChainCause.FadeTime);
                group.alpha = 1f - Mathf.Pow(1f - fade, 2f);
                if (fade > 0f && ChainCause.ShownAt < 0f) ChainCause.ShownAt = Time.unscaledTime;
                float t = Mathf.Clamp01(elapsed / ChainCause.DriftTime);
                drift.anchoredPosition = new Vector2(24f, Mathf.Lerp(0f, ChainCause.DriftDistance, 1f - Mathf.Pow(1f - t, 3f)));
                if (t >= 1f) drift = null;
            }
            else if (group.alpha >= 1f) Destroy(this);
        }
    }
}
