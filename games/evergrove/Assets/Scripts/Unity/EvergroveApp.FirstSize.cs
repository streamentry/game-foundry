using System;
using Evergrove.Core;
using UnityEngine;
using UnityEngine.UI;

namespace Evergrove.UnityClient
{
    /// <summary>
    /// Checklist 6: text size is offered on first launch, over the garden, before the first
    /// puzzle. Choosing a size applies it at once: the garden behind re-renders through Rebuild
    /// and the chooser re-renders at the same size. Done (or any later screen) closes it, and
    /// nothing waits for it. Settings keeps its own text-size control.
    /// </summary>
    public sealed partial class EvergroveApp
    {
        // Device once-flag: written when the offer first appears, so it never returns.
        private const string FirstSizeOfferedKey = "evergrove:first-size-offered:v1";
        // Tap height of each size choice; the checklist asks for at least 56 pt.
        private const int ChoiceHeight = 64;
        // Glyph base sizes grow with each step: the first A is the smallest.
        private const int GlyphBaseSize = 16;
        private const int GlyphStepSize = 6;

        private RectTransform firstSizePanel;
        private bool firstSizeOffered; // shown at most once per launch

        // Hook from Rebuild, on the first-run path before the first puzzle is built.
        private void OfferFirstSize(bool firstPuzzle)
        {
            if (!firstPuzzle)
            {
                HideFirstSize(); // the player has moved past the first board
                return;
            }
            if (firstSizeOffered || !FirstSizeDue()) return;
            firstSizeOffered = true;
            try
            {
                PlayerPrefs.SetInt(FirstSizeOfferedKey, 1);
                PlayerPrefs.Save();
            }
            catch (Exception exception)
            {
                Debug.LogWarning("Evergrove text size offer not recorded: " + exception.GetType().Name);
            }
            ShowFirstSize();
        }

        // Offered only to a device that has never chosen a size and has never been offered one.
        private bool FirstSizeDue()
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            if (qaInMemoryOnly) return false; // the QA tour opens the chooser itself (QaFirstSize)
            if (DevArgs.Read("-mosseloraTextScale") != null) return false; // a launch override picked the size
#endif
            try
            {
                return !PlayerPrefs.HasKey(TextScale.PreferenceKey)
                    && PlayerPrefs.GetInt(FirstSizeOfferedKey, 0) == 0;
            }
            catch (Exception)
            {
                return false;
            }
        }

        // Seam for the scripted tour shots: shows or hides the chooser over the first board.
        internal void QaFirstSize(bool show)
        {
            if (show) ShowFirstSize();
            else HideFirstSize();
        }

        private void ChooseFirstSize(int percent)
        {
            if (!TextScale.Choose(percent)) return;
            Rebuild(); // the garden and hub behind re-render at the new size (OfferFirstSize stays quiet)
            ShowFirstSize(); // the chooser re-renders at the same size
        }

        private void ShowFirstSize()
        {
            if (canvasRoot == null) return;
            HideFirstSize();
            var node = Node("First text size", canvasRoot);
            firstSizePanel = node.GetComponent<RectTransform>();
            firstSizePanel.anchorMin = new Vector2(0f, 0f);
            firstSizePanel.anchorMax = new Vector2(1f, 0f);
            firstSizePanel.pivot = new Vector2(0.5f, 0f);
            firstSizePanel.sizeDelta = new Vector2(-32f, 0f);
            firstSizePanel.anchoredPosition = new Vector2(0f, 16f);
            var image = node.AddComponent<Image>();
            MosseloraArt.Round(image, MosseloraTheme.CardFill, 18);
            MosseloraArt.Edge(node.transform, MosseloraTheme.CardEdge, 18);
            MosseloraArt.Lift(image, 2f, 0.10f);
            var stack = node.AddComponent<VerticalLayoutGroup>();
            stack.padding = new RectOffset(16, 16, 14, 14);
            stack.spacing = 10;
            stack.childAlignment = TextAnchor.UpperCenter;
            stack.childControlWidth = true;
            stack.childControlHeight = true;
            stack.childForceExpandWidth = true;
            stack.childForceExpandHeight = false;
            node.AddComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            Label(node.transform, "First text size heading", GameLocalization.Text("text_size"),
                19, 0, Ink, TextAnchor.MiddleCenter);

            Transform row = CenteredRowIn(node.transform, "First text size choices");
            row.GetComponent<HorizontalLayoutGroup>().spacing = 14;
            int[] steps = TextScale.Steps;
            for (int i = 0; i < steps.Length; i++)
            {
                int percent = steps[i];
                bool chosen = TextScale.Percent == percent;
                Button choice = Button(row, "Text size " + percent + " percent", "A",
                    () => ChooseFirstSize(percent), ChoiceHeight,
                    chosen ? MosseloraTheme.ForestAction : MosseloraTheme.QuietAction);
                // Each A is drawn at the size it stands for, so the choice shows what it does.
                Text glyph = choice.GetComponentInChildren<Text>();
                if (glyph != null) glyph.fontSize = TextScale.Scale(GlyphBaseSize + GlyphStepSize * i);
            }

            IconButton(node.transform, "Done", GameLocalization.Text("close"), MosseloraArt.IconConfirm,
                HideFirstSize, ChoiceHeight, MosseloraTheme.QuietAction, false, null, false, 0, 0, true);

            firstSizePanel.SetAsLastSibling(); // above the garden layers built after it
        }

        private void HideFirstSize()
        {
            if (firstSizePanel == null) return;
            firstSizePanel.gameObject.SetActive(false); // Destroy() is deferred until the frame ends.
            Destroy(firstSizePanel.gameObject);
            firstSizePanel = null;
        }
    }
}
