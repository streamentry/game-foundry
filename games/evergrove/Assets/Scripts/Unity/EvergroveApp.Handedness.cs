using Evergrove.Core;
using UnityEngine;
using UnityEngine.UI;

namespace Evergrove.UnityClient
{
    public sealed partial class EvergroveApp
    {
        // ---- Checklist 5, remember the hand (founder decision 2026-11-28). Asked once, on the first long-press
        // while the garden is quiet; the answer lives on the device (PlayerPrefs), never in the save. It mirrors
        // the menu button, its fan, the basket tray and the pinned action row, and the sideways control panel.
        private Hand hand = Hand.Unknown;
        private GameObject handSheet;
        private const string HandSheetName = "Hand";
        private const int HandTokenSize = 64;

        private void LoadHand()
        {
            hand = Handedness.Parse(PlayerPrefs.GetString(Handedness.HandKey, ""));
            TreePlay.HandHolding = hand;
        }

        private bool HandAsked { get { return PlayerPrefs.GetInt(Handedness.AskedKey, 0) == 1; } }

        // The menu's corner follows the hand. Unknown keeps bottom-left, as before the question.
        internal bool MenuOnRight { get { return Handedness.MenuCorner(hand) == HandCorner.BottomRight; } }
        internal Vector2 MenuCornerAnchor { get { return MenuOnRight ? new Vector2(1f, 0f) : new Vector2(0f, 0f); } }
        internal float MenuSideOffset { get { return MenuOnRight ? -MenuInset : MenuInset; } }

        // The bottom controls keep MenuReserve clear on the menu's side (the other side is free).
        internal float MenuReserveOffset { get { return MenuOnRight ? -MenuReserve / 2f : MenuReserve / 2f; } }

        // Long-press hook (LongPressCaption.Shown). The question waits for a quiet hub: not in the first minutes
        // (no living choice made yet), not during a puzzle or any open sheet, card, fan or the quiet garden.
        private void OnLongPressShown()
        {
            if (!Handedness.ShouldAsk(hand, HandAsked) || handSheet != null) return;
            if (titleScreen == null || world == null || world.Mode != PlayMode.Living || world.Choices.Length == 0) return;
            if (AnyLayerOpen()) return;
            OpenHandSheet();
        }

        private bool AnyLayerOpen()
        {
            return menuOpen || showJournal || fan != null || Quiet || openCard != null || choicePreview != null ||
                   letterCard != null || familySheet != null || seedBookSheet != null || marketSheet != null ||
                   nameSheet != null || datesSheet != null || weeklySheet != null || travelSheet != null ||
                   riverMapSheet != null;
        }

        // A small sheet: the question, two big hand tokens (the left one mirrored) and a quiet "Not now".
        private void OpenHandSheet()
        {
            handSheet = BuildSheet(titleScreen.transform, HandSheetName, DeclineHand,
                out RectTransform content, out ScrollRect scroll);
            SheetClose(content, "Close hand", DeclineHand);
            Text question = Label(content, "Hand question", "Which hand do you hold your phone with?", 24, 0, Ink,
                TextAnchor.MiddleCenter);
            Emphasize(question);
            GameObject row = Row(content, "Hand row");
            Button left = IconButton(row.transform, "Left hand", "Left hand", MosseloraArt.IconTap,
                () => ChooseHand(Hand.Left), HandTokenSize, MosseloraTheme.QuietAction, true, null, true);
            Transform glyph = left.transform.Find("Icon");
            if (glyph != null) glyph.localScale = new Vector3(-1f, 1f, 1f); // the tap icon, mirrored for the left hand
            IconButton(row.transform, "Right hand", "Right hand", MosseloraArt.IconTap,
                () => ChooseHand(Hand.Right), HandTokenSize, MosseloraTheme.QuietAction, true, null, true);
            Button(content, "Not now", "Not now", DeclineHand, 52);
            SheetMotion.Show(handSheet, SheetEdge.For(HandSheetName), ReducedMotion);
            handSheet.transform.SetAsLastSibling();
            FitSheet(handSheet, HandSheetName + " panel");
            Canvas.ForceUpdateCanvases();
            LayoutRebuilder.ForceRebuildLayoutImmediate(content);
            if (scroll != null) scroll.verticalNormalizedPosition = 1f;
            semanticsDirty = true;
        }

        // Asked once, whichever way the sheet closes: "Not now", the close token or the dimmed garden.
        private void DeclineHand()
        {
            RecordHandAsked();
            CloseHandSheet();
        }

        private void ChooseHand(Hand choice)
        {
            SetHand(choice);
            RecordHandAsked();
            CloseHandSheet();
            Rebuild();
        }

        // Settings: the hand is changeable at any time, and applies at once.
        private void HandSettingButton(Transform row)
        {
            string caption = hand == Hand.Left ? "Hand: left" : hand == Hand.Right ? "Hand: right" : "Hand: not chosen yet";
            IconButton(row, "Hand setting", caption, MosseloraArt.IconTap, () => ChooseHand(Handedness.Next(hand)),
                SettingToken, hand == Hand.Left ? MosseloraTheme.ForestAction : MosseloraTheme.QuietAction,
                false, null, false, 0, 0, true, stateOn: hand == Hand.Left);
        }

        private void SetHand(Hand value)
        {
            hand = value;
            TreePlay.HandHolding = value;
            PlayerPrefs.SetString(Handedness.HandKey, Handedness.Serialise(value));
        }

        private void RecordHandAsked()
        {
            PlayerPrefs.SetInt(Handedness.AskedKey, 1);
            PlayerPrefs.Save();
        }

        private void CloseHandSheet()
        {
            RetireSheet(handSheet, HandSheetName, false);
            handSheet = null;
            semanticsDirty = true;
        }
    }
}
