using System;
using System.Collections;
using Evergrove.Core;
using UnityEngine;
using UnityEngine.UI;

namespace Evergrove.UnityClient
{
    /// <summary>
    /// Item 34: the player chooses a board's leaves (1 gentlest to 3 deepest) for each puzzle kind,
    /// remembered per kind on this device. Nothing changes until a player chooses: with no choice the
    /// story's own board is dealt, exactly as before. Logic kinds show their measured leaves; the rest
    /// show size, and say so.
    /// </summary>
    public sealed partial class EvergroveApp
    {
        private const string ChooserName = "Leaves choice";
        private PuzzleKind chooserKind;
        private int chooserTier;
        private Action chooserReopen;

        // The level remembered for a kind, or 0 when the player has not chosen one.
        private static string LeavesKey(PuzzleKind kind) { return "evergrove:leaves:" + (int)kind; }

        private static int ChosenLeaves(PuzzleKind kind)
        {
            int level = PlayerPrefs.GetInt(LeavesKey(kind), 0);
            return level >= DifficultyChoice.Gentlest && level <= DifficultyChoice.Levels ? level : 0;
        }

        // Called first in OpenThingPuzzle. Deals the remembered level's board when there is one, and
        // keeps how to deal it again (from the story's own seed and tier) for the chooser.
        private (int, int) ChosenDeal(PuzzleKind kind, int seed, int tier, Func<int> solved,
                                      Func<int, int, Func<int>, Action> reopenAt)
        {
            chooserKind = kind;
            chooserReopen = reopenAt(seed, tier, solved);
            int level = ChosenLeaves(kind);
            DifficultyChoice.Deal deal = level == 0 ? new DifficultyChoice.Deal(seed, tier, 0)
                : DifficultyChoice.DealAt(kind, seed, level);
            chooserTier = deal.Tier;
            StartCoroutine(AttachLeavesChooser());
            return (deal.Seed, deal.Tier);
        }

        // The chooser sits under the card's top row, once the card has been built.
        private IEnumerator AttachLeavesChooser()
        {
            yield return null;
            TreePlay card = openCard;
            if (card == null) yield break;
            Transform cardRoot = card.transform.Find("Tree puzzle card");
            if (cardRoot == null || cardRoot.Find(ChooserName) != null) yield break;

            PuzzleKind kind = chooserKind;
            bool logic = DifficultyChoice.MeasuresLogic(kind);
            int shown = logic ? boardLeaves : DifficultyChoice.LevelOfTier(chooserTier);
            Transform row = TreePlay.Row(cardRoot, ChooserName, 60f);
            row.SetSiblingIndex(1);

            var caption = new GameObject("Leaves caption", typeof(RectTransform));
            caption.transform.SetParent(row, false);
            caption.AddComponent<LayoutElement>().minWidth = 96f;
            var text = caption.AddComponent<Text>();
            text.font = font;
            text.fontSize = 18;
            text.color = MosseloraTheme.EarthInk;
            text.alignment = TextAnchor.MiddleRight;
            text.raycastTarget = false;
            // Honest labels: logic kinds measure leaves; the rest only have size.
            text.text = logic ? "Leaves" : "Size";

            for (int level = DifficultyChoice.Gentlest; level <= DifficultyChoice.Levels; level++)
                AddLevelButton(row, kind, level, level == shown);
        }

        // One level button: at least 48 pt on each side, the chosen one filled.
        private void AddLevelButton(Transform row, PuzzleKind kind, int level, bool chosen)
        {
            var node = new GameObject("Leaves " + level, typeof(RectTransform));
            node.transform.SetParent(row, false);
            var layout = node.AddComponent<LayoutElement>();
            layout.minWidth = layout.preferredWidth = 56f;
            layout.minHeight = layout.preferredHeight = 56f;
            var image = node.AddComponent<Image>();
            MosseloraArt.Round(image, chosen ? MosseloraTheme.ForestAction : MosseloraTheme.QuietAction, 28);
            var button = node.AddComponent<Button>();
            button.targetGraphic = image;
            int picked = level;
            button.onClick.AddListener(() => ChooseLeaves(kind, picked));
            PressSquash.Add(node);

            var label = new GameObject("Number", typeof(RectTransform));
            label.transform.SetParent(node.transform, false);
            var labelRect = (RectTransform)label.transform;
            labelRect.anchorMin = Vector2.zero;
            labelRect.anchorMax = Vector2.one;
            labelRect.offsetMin = labelRect.offsetMax = Vector2.zero;
            var numeral = label.AddComponent<Text>();
            numeral.font = font;
            numeral.fontSize = 24;
            numeral.fontStyle = FontStyle.Bold;
            numeral.color = chosen ? Color.white : MosseloraTheme.EarthInk;
            numeral.alignment = TextAnchor.MiddleCenter;
            numeral.raycastTarget = false;
            numeral.text = level.ToString();
        }

        // Remembers the choice for this kind, then re-deals the board at that level. The old card is
        // removed without its close handler, so nothing is counted or paid by the change.
        private void ChooseLeaves(PuzzleKind kind, int level)
        {
            PlayerPrefs.SetInt(LeavesKey(kind), DifficultyChoice.Clamp(level));
            PlayerPrefs.Save();
            StartCoroutine(RedealChosen());
        }

        private IEnumerator RedealChosen()
        {
            yield return null;
            Action reopen = chooserReopen;
            if (openCard != null) DestroyImmediate(openCard.gameObject);
            openCard = null;
            reopen?.Invoke();
        }
    }
}
