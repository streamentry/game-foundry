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

        private void PlaceTip(Transform window, int held)
        {
            GardenItem item = ItemAt(held, world.Choices[held]);
            var tip = Node("How to place", window);
            tip.AddComponent<LayoutElement>().ignoreLayout = true;
            var rect = tip.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0f, 0f);
            rect.anchorMax = new Vector2(1f, 0f);
            rect.pivot = new Vector2(0.5f, 0f);
            rect.offsetMin = new Vector2(12f, 10f);
            rect.offsetMax = new Vector2(-12f, 10f);
            var body = tip.AddComponent<Image>();
            MosseloraArt.Round(body, MosseloraTheme.CardFill, 16);
            body.raycastTarget = false;
            MosseloraArt.Lift(body, 3f, 0.25f);
            var row = tip.AddComponent<HorizontalLayoutGroup>();
            row.padding = new RectOffset(10, 14, 8, 8);
            row.spacing = 8;
            row.childAlignment = TextAnchor.MiddleLeft;
            row.childControlWidth = true;
            row.childControlHeight = true;
            row.childForceExpandWidth = false;
            row.childForceExpandHeight = false;
            tip.AddComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            Sprite art = MosseloraArt.Load(item.Art);
            foreach (Sprite glyph in new[] { art, MosseloraArt.Load(MosseloraArt.IconTap) })
            {
                if (glyph == null) continue;
                var icon = Node("Tip picture", tip.transform);
                var iconLayout = icon.AddComponent<LayoutElement>();
                iconLayout.minWidth = iconLayout.preferredWidth = 38;
                iconLayout.minHeight = iconLayout.preferredHeight = 38;
                var iconImage = icon.AddComponent<Image>();
                iconImage.sprite = glyph;
                iconImage.preserveAspect = true;
                iconImage.raycastTarget = false;
                if (glyph != art) iconImage.color = MosseloraTheme.WaterChannel;
            }
            string verb = item.Family == VillageChoice.ShadeTree ? "plant" : item.Family == VillageChoice.Flowers ? "grow" : "build";
            string words = GameLocalization.Current == "en"
                ? "Tap the grass to " + verb + " your " + item.Name + " there." : "";
            if (words.Length == 0) return; // other languages: the pictures alone
            Text line = Label(tip.transform, "Tip words", words, 16, 0, Ink, TextAnchor.MiddleLeft);
            Emphasize(line);
            line.GetComponent<LayoutElement>().flexibleWidth = 1f;
        }

        private void CloseTitle()
        {
            if (quietLayer != null) ExitQuiet(); // never leave the page beneath hidden (I294)
            if (titleScreen == null) return;
            Destroy(titleScreen);
            titleScreen = null;
            semanticsDirty = true;
        }

        private Transform CenteredRowIn(Transform parent, string name)
        {
            var row = Node(name, parent);
            var group = row.AddComponent<HorizontalLayoutGroup>();
            group.childAlignment = TextAnchor.MiddleCenter;
            group.childControlWidth = true;
            group.childControlHeight = true;
            group.childForceExpandWidth = false;
            group.childForceExpandHeight = false;
            return row.transform;
        }

        private static void ClearChildren(Transform parent)
        {
            if (parent == null) return;
            for (int i = parent.childCount - 1; i >= 0; i--)
            {
                GameObject old = parent.GetChild(i).gameObject;
                old.SetActive(false);
                Destroy(old);
            }
        }

        // Reduced motion is the Gentle motion setting turned off: each slide becomes a short fade.
        private bool ReducedMotion => !gentleMotion;

        // Puts a built sheet away from its edge, then destroys it. A sheet replaced by a rebuild
        // goes at once, so the new one is the only motion the player sees.
        private void RetireSheet(GameObject sheet, string name, bool replacing)
        {
            if (sheet == null) return;
            SheetMotion.Hide(sheet, SheetEdge.For(name), ReducedMotion, () => { if (sheet != null) Destroy(sheet); }, null, replacing);
        }

        // A parchment sheet that rises over the game; tapping the dimmed garden
        // behind it closes it. Built once; its contents are rebuilt with the page.
        private void BuildMenuSheet(Transform host)
        {
            menuSheet = BuildSheet(host, "Menu", () => SetMenu(false), out menuContent, out menuScroll);
            journalSheet = BuildSheet(host, "Journal", CloseJournal, out journalContent, out journalScroll);
        }

        // A parchment sheet that rises over the dimmed garden; tapping the
        // garden behind it closes it. Its contents are rebuilt with the page.
        private GameObject BuildSheet(Transform host, string name, Action close,
                                      out RectTransform sheetContent, out ScrollRect sheetScroll)
        {
            var sheet = Node(name + " sheet", host);
            Fill(sheet.GetComponent<RectTransform>());
            var dim = sheet.AddComponent<Image>();
            // A warm umber veil, like an old photograph, rather than a cool grey
            // web-modal scrim; the parchment sheet still stands clear of it (I88).
            dim.color = new Color(0.30f, 0.22f, 0.13f, 0.40f);
            var dimButton = sheet.AddComponent<Button>();
            dimButton.transition = Selectable.Transition.None;
            dimButton.onClick.AddListener(() => close());
            var panel = Node(name + " panel", sheet.transform);
            var panelRect = panel.GetComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(0f, 0f);
            panelRect.anchorMax = new Vector2(1f, 0.86f);
            panelRect.offsetMin = new Vector2(8f, 0f);
            panelRect.offsetMax = new Vector2(-8f, 0f);
            var panelImage = panel.AddComponent<Image>();
            MosseloraArt.Round(panelImage, MosseloraTheme.Paper, 26);
            MosseloraArt.Lift(panelImage, 4f, 0.25f);
            // The sheet is the same painted paper as the page, clipped to its
            // rounded corners by the panel's own shape (I95).
            var grain = Node(name + " paper", panel.transform);
            grain.AddComponent<LayoutElement>().ignoreLayout = true;
            Fill(grain.GetComponent<RectTransform>());
            var grainImage = grain.AddComponent<Image>();
            MosseloraArt.PagePaper(grainImage, MosseloraTheme.Paper);
            grainImage.raycastTarget = false;
            grain.transform.SetAsFirstSibling();
            panel.AddComponent<Mask>().showMaskGraphic = true;
            MosseloraArt.Edge(panel.transform, MosseloraTheme.CardEdge, 26);
            sheetScroll = panel.AddComponent<ScrollRect>();
            sheetScroll.horizontal = false;
            sheetScroll.movementType = ScrollRect.MovementType.Clamped;
            sheetScroll.scrollSensitivity = 28f;
            var view = Node(name + " viewport", panel.transform);
            var viewRect = view.GetComponent<RectTransform>();
            Fill(viewRect);
            viewRect.offsetMin = new Vector2(0f, 4f);
            viewRect.offsetMax = new Vector2(0f, -SheetHeader); // a fixed band holds the close button (I101)
            view.AddComponent<RectMask2D>();
            sheetScroll.viewport = viewRect;
            var body = Node(name + " content", view.transform);
            sheetContent = body.GetComponent<RectTransform>();
            sheetContent.anchorMin = new Vector2(0f, 1f);
            sheetContent.anchorMax = new Vector2(1f, 1f);
            sheetContent.pivot = new Vector2(0.5f, 1f);
            sheetContent.sizeDelta = Vector2.zero;
            var column = body.AddComponent<VerticalLayoutGroup>();
            column.childAlignment = TextAnchor.UpperCenter;
            column.padding = new RectOffset(16, 16, 14, 28);
            column.spacing = 12;
            column.childControlWidth = true;
            column.childControlHeight = true;
            column.childForceExpandWidth = true;
            column.childForceExpandHeight = false;
            body.AddComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            sheetScroll.content = sheetContent;
            sheet.SetActive(false);
            return sheet;
        }

        // A sheet is as tall as what it holds, up to 86% of the screen, so two
        // memories don't sit in a tall empty page like a web modal (I128).
        private static void FitSheetToPages(GameObject sheet, RectTransform pages)
        {
            if (sheet == null || pages == null) return;
            var panel = pages.parent != null ? pages.parent.parent as RectTransform : null;
            var sheetRect = sheet.GetComponent<RectTransform>();
            float height = sheetRect.rect.height;
            if (panel == null || height <= 1f) return;
            float wanted = pages.rect.height + SheetHeader + 4f + 10f;
            // Height only: keep the x anchor FitSheet chose (centred when sideways).
            // I128 wrote x = 1 here, which pushed the landscape sheet off screen (I150).
            float cap = panel.anchorMin.x > 0.25f ? 0.92f : 0.86f;
            panel.anchorMax = new Vector2(panel.anchorMax.x, Mathf.Clamp(wanted / height, 0.34f, cap));
        }

        private const float SheetHeader = 78f; // the plaque art stands a little taller than its 52 pt box

        private void CreekBehindBoard(Transform board)
        {
            string prefix = PaintedVillage.BannerFor(PaintedVillage.ArtSeason(world));
            Sprite creek = MosseloraArt.Load(prefix + (world.Solved ? "flowing" : "dry"));
            if (creek == null) return;
            var window = Node("Creek behind board", board);
            window.AddComponent<LayoutElement>().ignoreLayout = true;
            var windowRect = window.GetComponent<RectTransform>();
            Fill(windowRect);
            // Screen edge to screen edge (past the tablet column, I111). It stops at
            // the bed's top: the creek is drawn after the text above the board, so
            // reaching upward would cover it. Below, a band melts into the paper.
            windowRect.offsetMin = new Vector2(-17f - columnLeft, -46f);
            windowRect.offsetMax = new Vector2(17f + columnRight, 0f);
            window.AddComponent<RectMask2D>();
            var art = Node("Creek painting", window.transform);
            Fill(art.GetComponent<RectTransform>());
            var image = art.AddComponent<Image>();
            image.sprite = creek;
            image.raycastTarget = false;
            image.color = new Color(1f, 1f, 1f, 0.55f);
            var fit = art.AddComponent<AspectRatioFitter>();
            fit.aspectMode = AspectRatioFitter.AspectMode.EnvelopeParent;
            fit.aspectRatio = MosseloraArt.Aspect(creek);
            // The creek fills as the board does (I167): the flowing painting lies
            // over the dry one, as opaque as the share of tiles the water reaches.
            creekWater = null;
            Sprite flowing = world.Solved ? null : MosseloraArt.Load(prefix + "flowing");
            if (flowing != null)
            {
                var water = Node("Creek water", window.transform);
                Fill(water.GetComponent<RectTransform>());
                creekWater = water.AddComponent<Image>();
                creekWater.sprite = flowing;
                creekWater.raycastTarget = false;
                var waterFit = water.AddComponent<AspectRatioFitter>();
                waterFit.aspectMode = AspectRatioFitter.AspectMode.EnvelopeParent;
                waterFit.aspectRatio = MosseloraArt.Aspect(flowing);
                creekShown = CreekShare();
                creekWater.color = new Color(1f, 1f, 1f, 0.55f * creekShown);
            }
            // Soft top and bottom: the painting melts into the paper instead of
            // ending in a ruled line (I111).
            Sprite fade = MosseloraArt.TopFade();
            for (int edge = 1; edge < 2; edge++) // bottom only; the bed's stones edge the top
            {
                var melt = Node(edge == 0 ? "Creek melt top" : "Creek melt bottom", window.transform);
                var meltRect = melt.GetComponent<RectTransform>();
                meltRect.anchorMin = new Vector2(0f, edge == 0 ? 1f : 0f);
                meltRect.anchorMax = new Vector2(1f, edge == 0 ? 1f : 0f);
                meltRect.pivot = new Vector2(0.5f, 0.5f); // centre pivot, so the flip stays inside the window
                meltRect.sizeDelta = new Vector2(0f, 44f);
                meltRect.anchoredPosition = new Vector2(0f, edge == 0 ? -22f : 22f);
                if (edge == 1) meltRect.localScale = new Vector3(1f, -1f, 1f);
                var meltImage = melt.AddComponent<Image>();
                meltImage.sprite = fade;
                meltImage.color = Paper;
                meltImage.raycastTarget = false;
            }
            window.transform.SetAsFirstSibling(); // under the stone bed and the tiles
        }

        private float CreekShare()
        {
            Puzzle puzzle = world.CurrentPuzzle;
            return puzzle.CellCount > 0 ? (float)world.Insight.ConnectedCount / puzzle.CellCount : 0f;
        }

        // Eases the creek's water toward the board's share (gentle motion), or sets it.
        private IEnumerator FillCreek(Image water, float target)
        {
            float from = creekShown;
            creekShown = target;
            bool animate = gentleMotion;
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            if (qaInMemoryOnly) animate = false;
#endif
            for (float t = 0f; animate && t < 1f; t += Time.unscaledDeltaTime / 0.8f)
            {
                if (water == null || water != creekWater) yield break;
                water.color = new Color(1f, 1f, 1f, 0.55f * Mathf.Lerp(from, target, Mathf.SmoothStep(0f, 1f, t)));
                yield return null;
            }
            if (water != null) water.color = new Color(1f, 1f, 1f, 0.55f * target);
        }

        // A sheet's close button lives in a fixed band above its scrolling
        // pages, so it stays in reach at the end of a long journal or menu (I101).
        // Rebuilt with the page (fresh language), replacing the previous one.
        private void SheetClose(RectTransform sheetContent, string name, Action close)
        {
            Transform panel = sheetContent != null && sheetContent.parent != null ? sheetContent.parent.parent : null;
            if (panel == null) return;
            Transform old = panel.Find(name);
            if (old != null) { old.gameObject.SetActive(false); Destroy(old.gameObject); }
            Button button = IconButton(panel, name, GameLocalization.Text("close"), MosseloraArt.IconCancel,
                () => close(), 56, MosseloraTheme.QuietAction); // 56: BackPlacement.Size, enforced by PlaceBack
            PlaceBack(button.GetComponent<RectTransform>());
            // After the paper grain, before the pages: read first, drawn clear of the pages.
            button.transform.SetSiblingIndex(Mathf.Min(1, panel.childCount - 1));
        }
    }
}
