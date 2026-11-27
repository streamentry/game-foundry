using System;
using Evergrove.Core;
using UnityEngine;
using UnityEngine.UI;

namespace Evergrove.UnityClient
{
    public sealed partial class EvergroveApp
    {
        /// <summary>
        /// Today's local calendar day as yyyymmdd, the day the replay cap (v1 #80) counts against.
        /// Uses DateTime.Now like the other day-based views (TodayWeather, the returning diary), so
        /// the QA tour's fixed hour (Now) never changes which day pays.
        /// </summary>
        private static int LocalDayStamp()
        {
            DateTime today = DateTime.Now;
            return today.Year * 10000 + today.Month * 100 + today.Day;
        }

        // ---- Item 78: the seed book. Tapping the seed purse opens a sheet that lists the
        // last 20 seed movements (world.Ledger, newest first), so nothing in the purse is a mystery.
        private GameObject seedBookSheet;

        // The purse becomes a button: tap opens the seed book; the spoken name gives the count.
        // Called from SeedPurse in place of the old non-raycast body.
        private void SeedPurseTap(GameObject purse, Image body)
        {
            body.raycastTarget = true;
            var tap = purse.AddComponent<Button>();
            tap.targetGraphic = body;
            tap.colors = PressTint();
            tap.onClick.AddListener(OpenSeedBook);
            // A hidden "Caption" child is what screen readers speak (ScreenReaderBridge.CaptionOf).
            var spoken = new GameObject("Caption", typeof(RectTransform));
            spoken.transform.SetParent(purse.transform, false);
            spoken.AddComponent<LayoutElement>().ignoreLayout = true;
            Text text = spoken.AddComponent<Text>();
            text.font = font;
            text.fontSize = 14;
            text.text = "Seeds you have, " + world.Seeds + ". Opens the seed book.";
            text.color = new Color(0f, 0f, 0f, 0f);
            text.raycastTarget = false;
        }

        // Same sheet pattern as the menu and journal: a dimmed veil, a parchment panel with a
        // close button in its fixed header, and tapping the veil closes it. Built fresh on each
        // open, so it shows the seed state at opening (no live updates needed).
        internal void OpenSeedBook()
        {
            if (titleScreen == null || world == null) return;
            CloseSeedBook();
            seedBookSheet = BuildSheet(titleScreen.transform, "Seed book", CloseSeedBook,
                out RectTransform content, out ScrollRect scroll);
            SheetClose(content, "Close seed book", CloseSeedBook);
            Text title = Label(content, "Seed book title", "Seed book", 26, 0, Ink, TextAnchor.MiddleCenter);
            Emphasize(title);
            LedgerLine[] lines = world.Ledger; // newest first, at most 20
            if (lines.Length == 0)
            {
                Text empty = Label(content, "Seed book empty", "Nothing yet. Solve a puzzle to earn seeds.",
                    18, 0, Ink, TextAnchor.MiddleCenter);
                empty.GetComponent<LayoutElement>().flexibleWidth = 1f;
            }
            foreach (LedgerLine line in lines) SeedBookLine(content, line);
            seedBookSheet.SetActive(true);
            seedBookSheet.transform.SetAsLastSibling(); // over the purse and the garden
            FitSheet(seedBookSheet, "Seed book panel");
            Canvas.ForceUpdateCanvases();
            LayoutRebuilder.ForceRebuildLayoutImmediate(content);
            FitSheetToPages(seedBookSheet, content);
            if (scroll != null) scroll.verticalNormalizedPosition = 1f;
            semanticsDirty = true;
        }

        internal void CloseSeedBook()
        {
            if (seedBookSheet != null) Destroy(seedBookSheet);
            seedBookSheet = null;
            semanticsDirty = true;
        }

        // One ledger line: a small picture, a plain label, and the signed amount.
        // Gains use the calm forest accent; spending stays ink. No red anywhere.
        private void SeedBookLine(Transform parent, LedgerLine line)
        {
            Transform row = CenteredRowIn(parent, "Seed line");
            var layout = row.GetComponent<HorizontalLayoutGroup>();
            layout.spacing = 12;
            layout.childAlignment = TextAnchor.MiddleLeft;
            MinHeight(row.gameObject, TextScale.Scale(48)); // a 48 pt row, grows with text size
            Sprite icon = MosseloraArt.Load(SeedReasonIcon(line.Reason));
            var mark = Node("Picture", row);
            var markLayout = mark.AddComponent<LayoutElement>();
            markLayout.preferredWidth = markLayout.preferredHeight = markLayout.minWidth = 36f;
            var markImage = mark.AddComponent<Image>();
            markImage.sprite = icon;
            markImage.preserveAspect = true;
            markImage.raycastTarget = false;
            if (icon != null && icon.name.StartsWith("ui-")) markImage.color = MosseloraTheme.ForestAction;
            Text words = Label(row, "Reason", SeedReasonWords(line.Reason), 18, 0, Ink, TextAnchor.MiddleLeft);
            words.GetComponent<LayoutElement>().flexibleWidth = 1f;
            int delta = line.Delta;
            // A real minus sign (U+2212) for spending, a plus for gains.
            string signed = (delta > 0 ? "+" : delta < 0 ? "−" : "") + Mathf.Abs(delta);
            Text amount = Label(row, "Amount", signed, 20, 0,
                delta > 0 ? MosseloraTheme.ForestAction : Ink, TextAnchor.MiddleRight);
            Emphasize(amount);
            amount.GetComponent<LayoutElement>().minWidth = TextScale.Percent > 100 ? 96f : 72f;
        }

        // Icons reuse the existing painted set. Unknown or future reasons fall back to the seed.
        private static string SeedReasonIcon(SeedReason reason)
        {
            switch (reason)
            {
                case SeedReason.FirstSolve: return MosseloraArt.IconConfirm;
                case SeedReason.Replay: return MosseloraArt.IconContinue;
                case SeedReason.Nursery: return MosseloraArt.IconRooted;
                case SeedReason.Keepsake: return MosseloraArt.Sprig;
                case SeedReason.ParcelSent: return "letter-sealed";
                case SeedReason.ParcelOpened: return "letter-open";
                case SeedReason.Pairing: return MosseloraArt.IconNeighbors;
                case SeedReason.Season: return TreePlay.SeedIcon;
                default: return TreePlay.SeedIcon;
            }
        }

        // Plain English for each reason. Unknown or future reasons get the generic line.
        private static string SeedReasonWords(SeedReason reason)
        {
            switch (reason)
            {
                case SeedReason.FirstSolve: return "A puzzle, first time";
                case SeedReason.Replay: return "A puzzle, again";
                case SeedReason.Nursery: return "From the nursery";
                case SeedReason.Keepsake: return "A keepsake cutting";
                case SeedReason.ParcelSent: return "Sent to family";
                case SeedReason.ParcelOpened: return "From family";
                case SeedReason.Pairing: return "Good neighbours";
                case SeedReason.Season: return "The season's harvest";
                default: return "In the garden";
            }
        }

        // ---- Item 72: the nursery price says why. Row 0 is the quick, small plants; row 2 the slow, large ones.
        // Shown as the spoken caption of each nursery token, since the shelf has no room for a second line at 150% text.
        private static string PriceReason(int row)
        {
            switch (row)
            {
                case 0: return "Small and quick to grow.";
                case 1: return "Takes a season to grow.";
                case 2: return "Large and slow to grow.";
                default: return "";
            }
        }
    }
}
