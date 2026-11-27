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

        private int letterShown = -1;
        private GameObject letterCard;
        private RectTransform letterboxRect;
        private bool letterArrived; // a parcel with a letter was just opened: it flies into the box (I237)

        // Under the seed purse, top right of home: always in view, wherever the
        // river has taken you (I217: on the map by the cottage it was off-screen for
        // anyone whose newest things are downstream).
        private void LetterBox()
        {
            string[] letters = world.Letters;
            if (titleScreen == null || letters.Length == 0) return;
            var node = Node("Letterbox", titleScreen.transform);
            var rect = node.GetComponent<RectTransform>();
            Rect safe = Screen.safeArea;
            float w = Mathf.Max(1, Screen.width), h = Mathf.Max(1, Screen.height);
            rect.anchorMin = rect.anchorMax = new Vector2(safe.xMax / w, safe.yMax / h);
            rect.pivot = new Vector2(1f, 1f);
            // A painted letterbox on its post (I233), not an app badge: a sealed
            // envelope sits on top while a letter waits unread.
            rect.sizeDelta = new Vector2(60f, 80f);
            rect.anchoredPosition = new Vector2(-14f, -ChromeTop - 58f);
            var box = node.AddComponent<Image>();
            box.sprite = MosseloraArt.Load("letterbox");
            box.preserveAspect = true;
            MosseloraArt.Lift(box, 2f, 0.22f);
            var button = node.AddComponent<Button>();
            button.targetGraphic = box;
            LongPressCaption.Attach(node, "Your letterbox. Tap to read the letters waiting for you.", font);
            button.onClick.AddListener(() => ShowLetter(0));
            PressSquash.Add(node);
            letterboxRect = rect;
            if (PlayerPrefs.GetInt(LettersReadKey, 0) < letters.Length)
            {
                Image sealedLetter = MosseloraArt.Ornament(node.transform, "Unread letter", MosseloraArt.Load("letter-sealed"),
                    new Vector2(0.5f, 1f), new Vector2(0.5f, 0.5f), new Vector2(44f, 34f), new Vector2(-4f, -8f));
                if (sealedLetter != null && gentleMotion) BloomPulse.Add(sealedLetter.gameObject, 0.6f, 0.08f);
            }
            if (letterArrived) Haptics.Arrived();
            if (letterArrived && gentleMotion) StartCoroutine(FlyInto(rect, "letter-sealed", new Vector2(64f, 50f)));
            letterArrived = false;
            var caption = Node("Caption", node.transform);
            caption.AddComponent<LayoutElement>().ignoreLayout = true;
            var spoken = caption.AddComponent<Text>();
            spoken.font = font;
            spoken.color = new Color(0f, 0f, 0f, 0f);
            spoken.text = letters.Length == 1 ? "A letter from " + world.LetterFrom[0] : letters.Length + " letters from family";
        }

        // Newest first: index 0 is the last letter received.
        private void ShowLetter(int fromNewest)
        {
            string[] letters = world.Letters, from = world.LetterFrom;
            if (letters.Length == 0 || titleScreen == null) return;
            fromNewest = Mathf.Clamp(fromNewest, 0, letters.Length - 1);
            int i = letters.Length - 1 - fromNewest;
            letterShown = fromNewest;
            bool unread = PlayerPrefs.GetInt(LettersReadKey, 0) < letters.Length && fromNewest == 0;
            PlayerPrefs.SetInt(LettersReadKey, letters.Length);
            PlayerPrefs.Save();
            RetireSheet(letterCard, "Letter", true);
            letterCard = Node("Reading a letter", titleScreen.transform);
            Fill(letterCard.GetComponent<RectTransform>());
            letterCard.AddComponent<Image>().color = new Color(0.08f, 0.14f, 0.11f, 0.38f);
            letterCard.AddComponent<TapCatcher>().Tapped = CloseLetter;
            // An unread letter arrives sealed: the seal lifts, then the paper (I234).
            if (unread && gentleMotion) { StartCoroutine(OpenEnvelope(letterCard, i, from[i], letters[i])); return; }
            LetterPaper(letterCard, i, from[i], letters[i], false);
        }

        private IEnumerator OpenEnvelope(GameObject card, int i, string from, string words)
        {
            Image sealedLetter = MosseloraArt.Ornament(card.transform, "Sealed letter", MosseloraArt.Load("letter-sealed"),
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(200f, 154f), Vector2.zero);
            if (sealedLetter == null) { LetterPaper(card, i, from, words, true); yield break; }
            PlayChime();
            for (float t = 0f; t < 1f; t += Time.unscaledDeltaTime / 0.7f)
            {
                if (card == null || sealedLetter == null || letterCard != card) yield break;
                float k = Mathf.SmoothStep(0f, 1f, t);
                sealedLetter.transform.localScale = Vector3.one * Mathf.Lerp(0.82f, 1.06f, k);
                sealedLetter.transform.localRotation = Quaternion.Euler(0f, 0f, Mathf.Sin(t * Mathf.PI * 3f) * 4f * (1f - k));
                yield return null;
            }
            if (card == null || letterCard != card) yield break; // closed while the seal lifted
            Destroy(sealedLetter.gameObject);
            LetterPaper(card, i, from, words, true);
        }

        private void LetterPaper(GameObject card, int i, string sender, string words, bool rise)
        {
            string[] letters = world.Letters;
            int fromNewest = letters.Length - 1 - i;
            letterCard = card;
            var paper = Node("Letter paper", card.transform);
            var paperRect = paper.GetComponent<RectTransform>();
            paperRect.anchorMin = paperRect.anchorMax = new Vector2(0.5f, 0.5f);
            paperRect.sizeDelta = new Vector2(Mathf.Min(PageWidth() + 6f, 420f), 0f);
            var paperImage = paper.AddComponent<Image>();
            MosseloraArt.PaperTile(paperImage, 22);
            paperImage.color = new Color(0.99f, 0.97f, 0.9f, 1f);
            MosseloraArt.Lift(paperImage, 4f, 0.22f);
            // Taps on the letter itself don't reach the backdrop (which closes it).
            paper.AddComponent<Button>().transition = Selectable.Transition.None;
            var column = paper.AddComponent<VerticalLayoutGroup>();
            column.padding = new RectOffset(22, 22, (int)BackPlacement.Reserve, 18); // top clear of the corner close
            column.spacing = 12;
            column.childControlWidth = column.childControlHeight = true;
            column.childForceExpandWidth = true;
            column.childForceExpandHeight = false;
            paper.AddComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            // The opened envelope crowns the page; the words are big; it's signed at the foot.
            var envelope = Node("Opened envelope", paper.transform);
            var envelopeLayout = envelope.AddComponent<LayoutElement>();
            envelopeLayout.preferredHeight = envelopeLayout.minHeight = 64f;
            var envelopeImage = envelope.AddComponent<Image>();
            envelopeImage.sprite = MosseloraArt.Load("letter-open");
            envelopeImage.preserveAspect = true;
            envelopeImage.raycastTarget = false;
            // I329: the picture stamp they chose, kept with the letter, shown each time it's read.
            int[] stamps = world.LetterStamps;
            int stampOn = i >= 0 && i < stamps.Length ? stamps[i] : 0;
            if (stampOn > 0 && stampOn <= FamilyParcel.StampCount)
            {
                var stampNode = Node("Picture stamp", paper.transform);
                var stampLayout = stampNode.AddComponent<LayoutElement>();
                stampLayout.preferredHeight = stampLayout.minHeight = 96f;
                var stampImage = stampNode.AddComponent<Image>();
                stampImage.sprite = MosseloraArt.Load(FamilyParcel.Stamps[stampOn - 1]);
                stampImage.preserveAspect = true;
                stampImage.raycastTarget = false;
            }
            Narrate(Label(paper.transform, "Letter words", words, 21, 0, Ink, TextAnchor.UpperLeft), true);
            // Signed at the foot, unless they already signed it ("Love, Nan").
            string tail = words.TrimEnd(' ', '.', '!', '\n');
            if (!tail.EndsWith(sender, StringComparison.OrdinalIgnoreCase))
                Emphasize(Label(paper.transform, "Letter from", "\u2014 " + sender, 19, 0, Leaf, TextAnchor.MiddleRight));
            // Checklist 81: a neighbour's letter takes three reply chips, just above its actions.
            int replyTo = Array.IndexOf(VillagerNames, sender);
            if (replyTo >= 0 && world.Mode == PlayMode.Living) ReplyChipRow(paper.transform, replyTo);
            Transform row = CenteredRowIn(paper.transform, "Letter actions");
            if (fromNewest < letters.Length - 1)
                IconButton(row, "Older letter", "Older letter", MosseloraArt.IconUndo, () => ShowLetter(letterShown + 1),
                    56, MosseloraTheme.QuietAction, false, null, false, 0, 0, true);
            // The one back control: top-right of the paper, as on every sheet and card (BackPlacement).
            Button closeLetter = IconButton(paper.transform, "Close letter", GameLocalization.Text("close"),
                MosseloraArt.IconCancel, CloseLetter, 56, // 56: BackPlacement.Size, enforced by PlaceBack
                MosseloraTheme.QuietAction, false, null, false, 0, 0, true);
            PlaceBack(closeLetter.GetComponent<RectTransform>());
            // Write back (I235): the family card opens with "Dear <name>," begun.
            if (world.Mode == PlayMode.Living)
            {
                string to = sender;
                Button reply = IconButton(row, "Write back", "Write back to " + to, "letter-pen", () => WriteBack(to),
                    56, MosseloraTheme.QuietAction, false, null, false, 0, 0, true);
                Transform pen = reply.transform.Find("Icon");
                if (pen != null)
                {
                    pen.GetComponent<Image>().color = Color.white; // painted, not a glyph
                    var penSize = pen.GetComponent<LayoutElement>();
                    penSize.minWidth = penSize.preferredWidth = penSize.minHeight = penSize.preferredHeight = 36f;
                }
            }
            if (fromNewest > 0)
                IconButton(row, "Newer letter", "Newer letter", MosseloraArt.IconContinue, () => ShowLetter(letterShown - 1),
                    56, MosseloraTheme.QuietAction, false, null, false, 0, 0, true);
            SheetMotion.Show(card, SheetEdge.For("Letter"), ReducedMotion, paperRect);
            semanticsDirty = true;
        }

        private void WriteBack(string to)
        {
            CloseLetter();
            OpenFamily();
            if (familySheet != null) familySheet.Prefill("Dear " + to + ",\n");
        }

        // ---- Checklist 81: reply chips. Three icons, never free text; the reply is kept on this device only.
        private const string ReplyKeyPrefix = "evergrove:reply:";
        private Transform replyChipRow; // the open letter's chips; null when no neighbour letter is open

        private void ReplyChipRow(Transform paper, int villager)
        {
            replyChipRow = CenteredRowIn(paper, "Reply chips");
            foreach (ReplyChip chip in ReplyChips.Chips)
            {
                ReplyChip pick = chip;
                IconButton(replyChipRow, "Reply chip " + chip, ReplyChips.Label(chip), ReplyChips.Icon(chip),
                    () => ChooseReplyChip(villager, pick), 56, MosseloraTheme.QuietAction, false, null, false, 0, 0, true);
            }
            RefreshReplyChips(villager);
        }

        // Tapping a chip saves the reply and shows it as chosen (a check mark, not colour alone). Nothing else.
        private void ChooseReplyChip(int villager, ReplyChip chip)
        {
            if (world == null || world.Mode != PlayMode.Living) return;
            SaveReplies(ReplyChips.Choose(ReplyMemoryNow(), villager, chip, ReplyDayNow()));
            RefreshReplyChips(villager);
        }

        private void RefreshReplyChips(int villager)
        {
            if (replyChipRow == null) return;
            ReplyMemory memory = ReplyMemoryNow();
            bool hasReply = memory.TryGet(villager, out ReplyNote note);
            foreach (ReplyChip chip in ReplyChips.Chips)
            {
                Transform token = replyChipRow.Find("Reply chip " + chip);
                if (token != null) MosseloraArt.StateMark(token, hasReply && note.Chip == chip, Mathf.Round(56f * 0.3f));
            }
        }

        // A neighbour answers a reply once, within three days (ReplyChips.EchoDue); then it is used up.
        // Once said, the answer stays that neighbour's words for the rest of the day in this garden:
        // speaking to them asks NeighborSays more than once (the heard-line note, then the bubble), and
        // the hub rebuilds often, so the answer must not be used up by the first ask and vanish.
        private readonly System.Collections.Generic.Dictionary<int, string> replyEchoToday =
            new System.Collections.Generic.Dictionary<int, string>();
        private int replyEchoDay = int.MinValue, replyEchoGarden = int.MinValue;

        private string ReplyEchoSaid(int villager)
        {
            if (world == null || world.Mode != PlayMode.Living) return null;
            int today = ReplyDayNow();
            if (replyEchoDay != today || replyEchoGarden != world.Garden)
            {
                replyEchoToday.Clear();
                replyEchoDay = today;
                replyEchoGarden = world.Garden;
            }
            if (replyEchoToday.TryGetValue(villager, out string said)) return said;
            ReplyMemory memory = ReplyMemoryNow();
            if (!ReplyChips.EchoDue(memory, villager, today)) return null;
            memory.TryGet(villager, out ReplyNote note);
            string line = ReplyChips.Echo(villager, note.Chip);
            if (line.Length == 0) return null;
            SaveReplies(ReplyChips.MarkEchoed(memory, villager));
            replyEchoToday[villager] = line;
            return line;
        }

        private ReplyMemory ReplyMemoryNow() { return ReplyMemory.Parse(PlayerPrefs.GetString(ReplyKeyPrefix + world.Garden, "")); }

        private void SaveReplies(ReplyMemory memory)
        {
            PlayerPrefs.SetString(ReplyKeyPrefix + world.Garden, memory.Serialize());
            PlayerPrefs.Save();
        }

        private int ReplyDayNow() { return CompanionContext.DayNumberOf(CompanionToday()); }

        private void CloseLetter()
        {
            RetireSheet(letterCard, "Letter", false);
            letterCard = null;
            replyChipRow = null;
            letterShown = -1;
            RefreshMaps(); // the unread dot goes
        }

        internal void QaReadLetter() { ShowLetter(0); }
        internal void QaCloseLetter() { CloseLetter(); }
        internal void QaWriteBack() { string[] from = world.LetterFrom; if (from.Length > 0) WriteBack(from[from.Length - 1]); }
    }
}
