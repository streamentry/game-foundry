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

        private int UnreadLetterCount()
        {
            return Mathf.Max(0, world.Letters.Length - PlayerPrefs.GetInt(LettersReadKey, 0));
        }

        internal void OpenCottage()
        {
            CottageSummary summary = CottageSummary.For(world, UnreadLetterCount());
            if (summary == null || titleScreen == null) return;
            CloseCottage();
            cottageCard = Node("Your cottage. Tap to close.", titleScreen.transform);
            Fill(cottageCard.GetComponent<RectTransform>());
            cottageCard.AddComponent<Image>().color = new Color(0.08f, 0.14f, 0.11f, 0.32f);
            var backdrop = cottageCard.AddComponent<Button>();
            backdrop.transition = Selectable.Transition.None;
            backdrop.onClick.AddListener(CloseCottage);
            // I299 (fit, 2026-11-28): the paper never reaches past the safe area. Its lines sit in a
            // scroll view inside it and scroll only when there are more than fit (landscape,
            // 150% text); sideways it is wider and a little tighter so fewer lines wrap.
            var card = Node("Cottage paper", cottageCard.transform);
            var cardRect = card.GetComponent<RectTransform>();
            cardRect.anchorMin = cardRect.anchorMax = new Vector2(0.5f, 0.5f);
            cardRect.sizeDelta = new Vector2(CottageCardWidth(), 0f);
            var paperImage = card.AddComponent<Image>();
            MosseloraArt.PaperTile(paperImage, 22);
            paperImage.color = new Color(0.99f, 0.97f, 0.9f, 1f);
            MosseloraArt.Lift(paperImage, 4f, 0.22f);
            float cardInset = landscape ? 12f : 18f;
            var cardScroll = card.AddComponent<ScrollRect>();
            cardScroll.horizontal = false;
            cardScroll.movementType = ScrollRect.MovementType.Clamped;
            cardScroll.scrollSensitivity = 28f;
            var view = Node("Cottage view", card.transform);
            var viewRect = view.GetComponent<RectTransform>();
            Fill(viewRect);
            viewRect.offsetMin = new Vector2(0f, cardInset);
            viewRect.offsetMax = new Vector2(0f, -cardInset);
            view.AddComponent<RectMask2D>();
            var paper = Node("Cottage lines", view.transform);
            var linesRect = paper.GetComponent<RectTransform>();
            linesRect.anchorMin = new Vector2(0f, 1f);
            linesRect.anchorMax = new Vector2(1f, 1f);
            linesRect.pivot = new Vector2(0.5f, 1f);
            linesRect.sizeDelta = Vector2.zero;
            var column = paper.AddComponent<VerticalLayoutGroup>();
            column.padding = new RectOffset(22, 22, 0, 0);
            column.spacing = landscape ? 6 : 10;
            column.childControlWidth = column.childControlHeight = true;
            column.childForceExpandWidth = true;
            column.childForceExpandHeight = false;
            paper.AddComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            cardScroll.viewport = viewRect;
            cardScroll.content = linesRect;
            GardenNameLine(paper.transform); // EvergroveApp.Name.cs: the garden's name, as the sign's title (checklist 3)
            // The season, as its stone: the year at a glance.
            string[] stones = { "stone-spring", "stone-summer", "stone-autumn", "stone-winter" };
            CottageLine(paper.transform, MosseloraArt.Load(stones[(int)summary.Season]), summary.Season.ToString());
            // I365: today's weather, in a word and a mark (I353).
            Weather today = TodayWeather(world);
            string weatherWords = today == Weather.Rainy ? "A rainy day" : today == Weather.Windy ? "A windy day"
                : today == Weather.Hot ? "A hot day" : today == Weather.Snowy ? "A snowy day" : "A fair day";
            Sprite weatherMark = today == Weather.Rainy ? MosseloraArt.Load(MosseloraArt.Droplet)
                : today == Weather.Snowy ? MosseloraArt.Load("stone-winter")
                : today == Weather.Windy ? MosseloraArt.Load("stone-autumn") : MosseloraArt.Load(MosseloraArt.IconSun);
            if (DayLightOn) CottageLine(paper.transform, weatherMark, weatherWords);
            // SC05: the waiting lines (letters, cuttings, puzzles, the season's yield, the nearest hope)
            // come in the companion ranker's order: what waits for you first. Never a nag: each says what is.
            var cottageLines = new System.Collections.Generic.List<CottageItem>();
            if (world.Mode == PlayMode.Living && SeasonalYields.Waiting(world)) cottageLines.Add(CottageItem.Yield);
            if (summary.Letters > 0) cottageLines.Add(CottageItem.Letters);
            if (summary.Cuttings > 0) cottageLines.Add(CottageItem.Cuttings);
            if (summary.Waiting > 0) cottageLines.Add(CottageItem.Waiting);
            // I313: the open hope nearest to coming true, with its true count (one line, not three).
            NeighbourWish nearest = null;
            int nearestWho = -1;
            for (int villager = 0; villager < 3; villager++)
            {
                NeighbourWish wish = NeighbourWish.For(world, villager);
                if (wish == null || wish.Granted) continue;
                if (nearest == null || wish.Have * nearest.Need > nearest.Have * wish.Need) { nearest = wish; nearestWho = villager; }
            }
            if (nearest != null) cottageLines.Add(CottageItem.Wish);
            foreach (CottageItem item in CompanionAttention.CottageOrder(cottageLines, CompanionFacts.For(world, CompanionNow()),
                         CompanionContext.DayNumberOf(CompanionToday()), world.Garden))
                switch (item)
                {
                    case CottageItem.Yield:
                        Season season = GardenSeason.For(world); // I400: what this season can still give
                        CottageLine(paper.transform, MosseloraArt.Load("stone-" + season.ToString().ToLowerInvariant()), SeasonalYields.Why(season));
                        break;
                    case CottageItem.Letters:
                        CottageLine(paper.transform, MosseloraArt.Load("letter-sealed"),
                            summary.Letters == 1 ? "A letter in your box" : summary.Letters + " letters in your box");
                        break;
                    case CottageItem.Cuttings:
                        CottageLine(paper.transform, MosseloraArt.Load(MosseloraArt.Sprig),
                            summary.Cuttings == 1 ? "A cutting in your basket" : summary.Cuttings + " cuttings in your basket");
                        break;
                    case CottageItem.Waiting:
                        CottageLine(paper.transform, MosseloraArt.Load(MosseloraArt.IconContinue),
                            summary.Waiting == 1 ? "One thing holds a new puzzle" : summary.Waiting + " things hold a new puzzle");
                        break;
                    default:
                        CottageLine(paper.transform, MosseloraArt.Portrait(nearestWho), nearest.Hope + ": " + nearest.Have + " of " + nearest.Need);
                        break;
                }
            if (summary.DropsNeed > 0) CottageDrops(paper.transform, summary.DropsHave, summary.DropsNeed);
            CottageDaysLine(paper.transform, summary.DaysPlayed);
            // I310: who left a light here, newest first.
            string[] lit = world.Lanterns;
            if (lit.Length > 0)
            {
                var names = new System.Collections.Generic.List<string>();
                for (int l = lit.Length - 1; l >= 0 && names.Count < 3; l--) if (!names.Contains(lit[l])) names.Add(lit[l]);
                CottageLine(paper.transform, MosseloraArt.Load(FamilySheet.LanternArt), "Lanterns from " +
                    (names.Count == 1 ? names[0] : string.Join(", ", names.GetRange(0, names.Count - 1).ToArray()) + " and " + names[names.Count - 1]));
            }
            if (!summary.Anything)
                CottageLine(paper.transform, MosseloraArt.Load(MosseloraArt.IconConfirm), "All tended. Replays wait whenever you like.");
            // I328 (idea 45, after cooking games): three fruit make a jar of jam, a richer gift.
            if (world.Jam > 0)
                CottageLine(paper.transform, MosseloraArt.Load("food-jam"),
                    world.Jam == 1 ? "A jar of jam in the pantry" : world.Jam + " jars of jam in the pantry");
            CottageTableRow(paper.transform); // I328/I228: the fruit and jars, as objects on the table
            // I330 (ideas 14 and 16, after block-building sandboxes): decorate anywhere, for free.
            Transform decorRow = CenteredRowIn(paper.transform, "Decorate");
            decorRow.gameObject.SetActive(world.DecorCount < WorldModel.MaxDecor || world.CanUndoDecor);
            decorRow.GetComponent<HorizontalLayoutGroup>().spacing = 8;
            foreach (Decoration piece in new[] { Decoration.Stones, Decoration.Bench, Decoration.Fence, Decoration.LanternPost })
            {
                Decoration chosen = piece;
                TreePlay.Token(decorRow, "Place " + DecorName(piece), MosseloraArt.Load(DecorArt(piece)), 56f,
                    MosseloraTheme.QuietAction, () => {
                        CloseCottage();
                        decorInHand = (int)chosen;
                        heldDecor = -1;
                        RefreshMaps();
                        if (titleScreen != null)
                            RuleToast.Show((RectTransform)titleScreen.transform, "Tap free land to set down " + DecorName(chosen) + ".",
                                font, Ink, MosseloraTheme.CardFill);
                    }, true);
                Transform glyph = decorRow.GetChild(decorRow.childCount - 1).Find("Icon");
                if (glyph != null) { ((RectTransform)glyph).sizeDelta = new Vector2(46f, 46f); glyph.GetComponent<Image>().color = Color.white; }
            }
            // Step 47: take back the last placement, move or removal (session only; a refused undo changes nothing).
            if (world.CanUndoDecor)
                TreePlay.Token(decorRow, "Undo the last decoration", MosseloraArt.Load(MosseloraArt.IconUndo), 56f,
                    MosseloraTheme.QuietAction, () => {
                        CloseCottage();
                        decorInHand = -1;
                        heldDecor = -1;
                        justDecorated = -1;
                        GardenMap undoMap = LoadGardenMap();
                        if (undoMap != null && world.UndoDecor(undoMap)) SaveWorld();
                        else if (titleScreen != null)
                            RuleToast.Show((RectTransform)titleScreen.transform, "That can't be taken back here.", font, Ink, MosseloraTheme.CardFill);
                        RefreshMaps();
                    }, true);
            // I319: from the cottage, look out over the whole river.
            Transform look = CenteredRowIn(paper.transform, "Look out");
            TreePlay.Token(look, "Look out over the whole river", MosseloraArt.Load(MosseloraArt.IconPath), 56f,
                MosseloraTheme.WaterAction, () => { CloseCottage(); EnterQuiet(true); }, true);
            if (world.Fruit >= WorldModel.JamPerJar && world.Jam < world.JamCapacity)
            {
                TreePlay.Token(look, "Make a jar of jam from three fruit", MosseloraArt.Load("food-jam"), 56f,
                    MosseloraTheme.QuietAction, () => {
                        if (!world.MakeJam()) return;
                        SaveWorld();
                        PlayChime();
                        if (purseFruit != null && gentleMotion) BloomPulse.Add(purseFruit.gameObject, 0f, 0.2f);
                        OpenCottage(); // the card again, with the jar counted
                    }, true);
                Transform jamGlyph = look.GetChild(look.childCount - 1).Find("Icon");
                if (jamGlyph != null) { ((RectTransform)jamGlyph).sizeDelta = new Vector2(44f, 44f); jamGlyph.GetComponent<Image>().color = Color.white; }
            }
            FitCottageCard(cardRect, linesRect, cardScroll, cardInset);
            if (gentleMotion) BloomPulse.Add(card, 0.05f, 0.04f);
            semanticsDirty = true;
        }

        private const float CottageMargin = 12f; // paper to the safe area's edge, top and bottom

        // Upright: the page width, as before. Sideways: wider (the screen has the room), so
        // long lines wrap less and the decorate row's five tokens fit inside the paper.
        private float CottageCardWidth()
        {
            if (!landscape) return Mathf.Min(PageWidth() - 20f, 360f);
            float safeWidth = Screen.safeArea.width / CottageCanvasScale();
            return Mathf.Clamp(safeWidth - 40f, 300f, 460f);
        }

        // Canvas units per screen pixel, from the full-screen card itself (it fills the
        // title screen, which fills the overlay canvas); the scaler formula is the fallback.
        private float CottageCanvasScale()
        {
            var host = cottageCard != null ? cottageCard.GetComponent<RectTransform>() : null;
            if (host != null && host.rect.height > 1f && Screen.height > 0) return Screen.height / host.rect.height;
            return LiveCanvasScale();
        }

        // As tall as its lines, but never past the safe area; centred in the safe area (not
        // the whole screen) so the notch and home bar never cover the name or the last row.
        private void FitCottageCard(RectTransform card, RectTransform lines, ScrollRect scroll, float inset)
        {
            Canvas.ForceUpdateCanvases();
            LayoutRebuilder.ForceRebuildLayoutImmediate(lines);
            float scale = CottageCanvasScale();
            Rect safe = Screen.safeArea;
            float room = Mathf.Max(160f, safe.height / scale - 2f * CottageMargin);
            float wanted = lines.rect.height + 2f * inset;
            float height = Mathf.Min(wanted, room);
            card.sizeDelta = new Vector2(card.sizeDelta.x, height);
            card.anchoredPosition = new Vector2((safe.center.x - Screen.width * 0.5f) / scale,
                (safe.center.y - Screen.height * 0.5f) / scale);
            scroll.vertical = wanted > height + 0.5f;
            scroll.StopMovement();
            scroll.verticalNormalizedPosition = 1f; // the garden's name first
        }

        private void CottageLine(Transform parent, Sprite icon, string words)
        {
            Transform row = CenteredRowIn(parent, "Cottage line");
            var layout = row.GetComponent<HorizontalLayoutGroup>();
            layout.spacing = 12;
            layout.childAlignment = TextAnchor.MiddleLeft;
            var mark = Node("Picture", row);
            var markLayout = mark.AddComponent<LayoutElement>();
            markLayout.preferredWidth = markLayout.preferredHeight = markLayout.minWidth = landscape ? 36f : 44f;
            var markImage = mark.AddComponent<Image>();
            markImage.sprite = icon;
            markImage.preserveAspect = true;
            markImage.raycastTarget = false;
            if (icon != null && icon.name.StartsWith("ui-")) markImage.color = MosseloraTheme.ForestAction;
            Text text = Label(row, "Line", words, 19, 0, Ink, TextAnchor.MiddleLeft);
            text.GetComponent<LayoutElement>().flexibleWidth = 1f;
            Narrate(text, false);
        }

        // The drops toward the next stretch of river, as on the mist (I224).
        private void CottageDrops(Transform parent, int have, int need)
        {
            Transform row = CenteredRowIn(parent, "Toward the next stretch, " + have + " of " + need);
            var layout = row.GetComponent<HorizontalLayoutGroup>();
            layout.spacing = 8;
            layout.childAlignment = TextAnchor.MiddleLeft;
            for (int d = 0; d < need; d++)
            {
                var drop = Node(d < have ? "Drop" : "Drop to come", row);
                var dropLayout = drop.AddComponent<LayoutElement>();
                dropLayout.preferredWidth = dropLayout.preferredHeight = dropLayout.minWidth = 24f;
                var dropImage = drop.AddComponent<Image>();
                dropImage.sprite = d < have ? MosseloraArt.Rounded(12) : MosseloraArt.Ring(12);
                dropImage.color = MosseloraTheme.WaterAction;
                dropImage.raycastTarget = false;
            }
            Text text = Label(row, "Line", "toward new river", 17, 0, Ink, TextAnchor.MiddleLeft);
            text.GetComponent<LayoutElement>().flexibleWidth = 1f;
        }

        internal void CloseCottage()
        {
            if (cottageCard != null) Destroy(cottageCard);
            cottageCard = null;
            semanticsDirty = true;
        }

        private void CloseNotesAsk()
        {
            if (notesCard != null) Destroy(notesCard);
            notesCard = null;
        }
    }
}
