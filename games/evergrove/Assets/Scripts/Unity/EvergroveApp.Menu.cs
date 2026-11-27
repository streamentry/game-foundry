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

        // The one back gesture: Android back or the keyboard's Escape closes the topmost open
        // layer, and only that one. Each layer keeps its own close method; this picks which.
        // Top to bottom: the letter, the family sheet, the sheets (seed book, stall, name,
        // dates, deeper boards, travel, river map), the menu, the journal, the choice preview, the puzzle card.
        private void HandleBackGesture()
        {
            if (!BackKeyPressed()) return;
            if (letterCard != null) CloseLetter();
            else if (familySheet != null) familySheet.Close();
            else if (seedBookSheet != null) CloseSeedBook();
            else if (marketSheet != null) CloseMarketSheet();
            else if (nameSheet != null) CloseNameSheet();
            else if (datesSheet != null) CloseDatesSheet();
            else if (weeklySheet != null) CloseWeeklyBoardSheet();
            else if (travelSheet != null) CloseTravelSheet(); // v1 #43
            else if (riverMapSheet != null) CloseRiverMap(); // checklist 45
            else if (menuOpen) SetMenu(false);
            else if (showJournal) CloseJournal();
            else if (choicePreview != null) CloseChoicePreview();
            else if (openCard != null) openCard.Close();
        }

        // Android back and a keyboard's Escape arrive as the same key press.
        private static bool BackKeyPressed()
        {
#if !ENABLE_LEGACY_INPUT_MANAGER && ENABLE_INPUT_SYSTEM
            var keyboard = UnityEngine.InputSystem.Keyboard.current;
            return keyboard != null && keyboard.escapeKey.wasPressedThisFrame;
#elif ENABLE_LEGACY_INPUT_MANAGER
            return Input.GetKeyDown(KeyCode.Escape);
#else
            return false;
#endif
        }

        // Shuts the journal and forgets its page and tab. The close button and the veil both use it.
        internal void CloseJournal()
        {
            showJournal = false;
            journalOpenPage = -1;
            almanacTab = false;
            familyTab = false;
            waysTab = false;
            Rebuild();
        }

        // The one back control's placement (BackPlacement): its top corner, inset and size.
        // Shared by every sheet, card and panel that has a close or back control.
        internal static void PlaceBack(RectTransform control)
        {
            bool right = BackPlacement.Corner == BackCorner.TopRight;
            control.anchorMin = control.anchorMax = control.pivot = new Vector2(right ? 1f : 0f, 1f);
            control.anchoredPosition = new Vector2(right ? -BackPlacement.Inset : BackPlacement.Inset, -BackPlacement.Inset);
            control.sizeDelta = new Vector2(BackPlacement.Size, BackPlacement.Size);
            var layout = control.GetComponent<LayoutElement>();
            if (layout == null) layout = control.gameObject.AddComponent<LayoutElement>();
            layout.ignoreLayout = true;
            layout.minWidth = layout.preferredWidth = layout.minHeight = layout.preferredHeight = BackPlacement.Size;
        }

        private void SetMenu(bool open)
        {
            menuOpen = open;
            if (!open) postcardCopied = false;
            if (open) speakingWith = -1;
            Rebuild();
            if (open && menuScroll != null) menuScroll.verticalNormalizedPosition = 1f;
        }

        // Top of the sheet: a close button, then every setting as one icon row.
        private void MenuHeader(bool offerLanguage)
        {
            SheetClose(menuContent, "Close menu", () => SetMenu(false));
            // Shown only when there is a stream in progress to reset (I109).
            if (!world.Completed && !world.Solved && !showingMemory && world.CanUndo)
            {
                Transform row = CenteredRow("Restart stream row");
                Button restart = IconButton(row, "Restart", GameLocalization.Text("restart"),
                    MosseloraArt.IconRestart, () => { Act(() => world.Restart(), "restart"); SetMenu(false); },
                    78, MosseloraTheme.QuietAction, false, null, true);
                Width(restart, 200);
            }
            SettingsRow(offerLanguage);
        }

        // Floating game chrome: a leaf menu button (left, with a new-memory dot) and the
        // memory book (right). Rebuilt with the page so state stays honest.
        private void BuildChrome(bool living)
        {
            if (chrome == null) return;
            CloseFan();
            // One corner button (I191): book, almanac, postcard, home and settings
            // open from it as a fan of round tokens, instead of a button per corner.
            Button menu = IconButton(chrome, "Open menu", GameLocalization.Text("menu"),
                MosseloraArt.IconMenu, () => {
                    if (menuOpen) SetMenu(false);
                    else if (showJournal) { showJournal = false; journalOpenPage = -1; almanacTab = false; familyTab = false; waysTab = false; Rebuild(); }
                    else ToggleFan(false);
                }, 54, menuOpen || showJournal ? MosseloraTheme.ForestAction : MosseloraTheme.QuietAction);
            PlaceChrome(menu, new Vector2(0f, 1f), new Vector2(10f, ChromeTop));
            if (living && world.Choices.Length > 0 && !showJournal) MarkUnreadMemory(menu, MenuSeenKey);
        }

        // ---- I191: the fan menu. One corner button opens a column of round
        // tokens under it; a tap anywhere else folds it away again.
        private GameObject fan;

        private void CloseFan()
        {
            if (fan == null) return;
            fan.SetActive(false);
            Destroy(fan);
            fan = null;
            semanticsDirty = true;
        }

        // Removes the dot from the menu button itself (its child "Has a new memory"); the
        // book token inside the fan has its own dot and is not touched.
        private void ClearMenuDot()
        {
            if (canvasRoot == null) return;
            foreach (Transform t in canvasRoot.GetComponentsInChildren<Transform>(true))
                if (t.name == "Has a new memory" && t.parent != null && t.parent.name == "Open menu")
                    Destroy(t.gameObject);
        }

        private void ToggleFan(bool fromHub)
        {
            if (fan != null) { CloseFan(); return; }
            if (canvasRoot == null) return;
            // Opening the menu reads its dot (B1b): the dot on the menu button goes now and stays
            // gone. The book inside the fan keeps its own dot until the book is opened.
            if (world.Mode == PlayMode.Living && world.Choices.Length > 0)
            {
                PlayerPrefs.SetInt(MenuSeenKey, world.Choices.Length);
                PlayerPrefs.Save();
                ClearMenuDot();
            }
            speakingWith = -1;
            fan = Node("Menu fan", canvasRoot);
            Fill(fan.GetComponent<RectTransform>());
            var scrim = fan.AddComponent<Image>();
            scrim.color = new Color(0.08f, 0.14f, 0.11f, 0.30f);
            var away = fan.AddComponent<Button>();
            away.transition = Selectable.Transition.None;
            away.onClick.AddListener(CloseFan);
            var column = Node("Fan column", fan.transform);
            var columnRect = column.GetComponent<RectTransform>();
            Rect safe = Screen.safeArea;
            float w = Mathf.Max(1, Screen.width), h = Mathf.Max(1, Screen.height);
            columnRect.anchorMin = columnRect.anchorMax = new Vector2(safe.xMin / w, safe.yMax / h);
            columnRect.pivot = new Vector2(0f, 1f);
            columnRect.anchoredPosition = new Vector2(10f, ChromeTop - 66f);
            var stack = column.AddComponent<VerticalLayoutGroup>();
            stack.spacing = 12;
            stack.childAlignment = TextAnchor.UpperLeft;
            stack.childControlWidth = true;
            stack.childControlHeight = true;
            stack.childForceExpandWidth = false;
            stack.childForceExpandHeight = false;
            var fit = column.AddComponent<ContentSizeFitter>();
            fit.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
            fit.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            bool living = world.Mode == PlayMode.Living;
            bool memories = living && world.Choices.Length > 0;
            var tokens = new System.Collections.Generic.List<GameObject>();
            if (!fromHub && titleScreen == null)
                tokens.Add(FanToken(column.transform, "Home", "Home", MosseloraArt.IconHome, () => {
                    CloseFan();
                    menuOpen = false;
                    showJournal = false;
                    BuildTitleScreen(canvasRoot);
                }));
            if (memories)
            {
                GameObject book = FanToken(column.transform, "Memories", GameLocalization.Text("journal_show"),
                    MosseloraArt.IconJournal, () => OpenBook(false));
                MarkUnreadMemory(book.GetComponent<Button>());
                tokens.Add(book);
                GameObject almanac = FanToken(column.transform, "Almanac", "Almanac", MosseloraArt.IconJournal,
                    () => OpenBook(true));
                // A painted sapling, not a second book (as on the book's tab).
                Transform glyph = almanac.transform.Find("Icon");
                Sprite sapling = MosseloraArt.Load(GardenCatalog.Get(VillageChoice.ShadeTree, 0).Art);
                Image glyphImage = glyph != null ? glyph.GetComponent<Image>() : null;
                if (glyphImage != null && sapling != null) { glyphImage.sprite = sapling; glyphImage.color = Color.white; }
                tokens.Add(almanac);
            }
            if (living && FamilyShown())
                tokens.Add(FanToken(column.transform, "Family", "Family", MosseloraArt.IconNeighbors, () => {
                    CloseFan();
                    OpenFamily();
                }));
            if (living && titleScreen != null)
            {
                // I294 (idea 50, ma): just look. Everything but the garden steps aside.
                GameObject look = FanToken(column.transform, "Just look at the garden", "Look", MosseloraArt.IconSun,
                    () => { CloseFan(); EnterQuiet(); });
                Transform lookGlyph = look.transform.Find("Icon");
                Image lookImage = lookGlyph != null ? lookGlyph.GetComponent<Image>() : null;
                // I346 (run 3, P4): a tiny painting of the garden says "look", where a sprig didn't.
                Sprite view = MosseloraArt.Load(MosseloraArt.VignetteTree) ?? MosseloraArt.Load(MosseloraArt.Sprig);
                if (lookImage != null && view != null)
                {
                    lookImage.sprite = view;
                    lookImage.color = Color.white;
                    lookImage.preserveAspect = true;
                    ((RectTransform)lookGlyph).sizeDelta = new Vector2(48f, 40f);
                }
                tokens.Add(look);
            }
            tokens.Add(FanToken(column.transform, "Settings", GameLocalization.Text("menu"), MosseloraArt.IconSettings,
                () => { CloseFan(); CloseTitle(); SetMenu(true); }));
            tokens = ArrangeFan(tokens); // checklist 15: the token used last opens first, nearest the thumb
            // Hang the fan just under the corner button it came from.
            foreach (Button corner in canvasRoot.GetComponentsInChildren<Button>())
                if (corner.name == "Open menu" && corner.gameObject.activeInHierarchy)
                {
                    var corners = new Vector3[4];
                    ((RectTransform)corner.transform).GetWorldCorners(corners);
                    columnRect.position = corners[0] + new Vector3(0f, -12f * canvasRoot.lossyScale.y, 0f);
                    break;
                }
            if (gentleMotion)
                for (int i = 0; i < tokens.Count; i++) StartCoroutine(FanIn(tokens[i], i * 0.045f));
            semanticsDirty = true;
        }

        // ---- I294: quiet mode. Home's controls, words and purse step aside; only the garden
        // stays, with its light, neighbours and seasons. A tap anywhere brings it all back.
        private readonly System.Collections.Generic.List<GameObject> quieted = new System.Collections.Generic.List<GameObject>();
        private GameObject quietLayer;

        internal bool Quiet { get { return quietLayer != null; } }

        internal void EnterQuiet() { EnterQuiet(false); }

        internal void EnterQuiet(bool wholeRiver)
        {
            if (titleScreen == null || quietLayer != null) return;
            if (wholeRiver && hubMapView != null) hubMapView.LookOut(true); // I319
            quieted.Clear();
            // Keep only the line of parents that holds home's live map; every sibling along
            // that line steps aside (b305: in portrait, controls also lived outside the title).
            Transform keep = hubMapView != null ? hubMapView.transform : titleScreen.transform.Find("Village window");
            Transform stop = canvasRoot != null ? canvasRoot.parent : null;
            for (Transform at = keep; at != null && at.parent != null && at != stop; at = at.parent)
            {
                foreach (Transform sibling in at.parent)
                    if (sibling != at && sibling.gameObject.activeSelf && sibling.GetComponent<Canvas>() == null &&
                        sibling.GetComponent<UnityEngine.EventSystems.EventSystem>() == null)
                    {
                        quieted.Add(sibling.gameObject);
                        sibling.gameObject.SetActive(false);
                    }
                if (at == (Transform)canvasRoot) break;
            }
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            var hidNames = new System.Collections.Generic.List<string>();
            foreach (GameObject hid in quieted) hidNames.Add(hid.name);
            Debug.Log("MOSSELORA_QUIET hid " + quieted.Count + " under " + titleScreen.name + " (" +
                titleScreen.transform.childCount + " children): " + string.Join(", ", hidNames));
#endif
            quietLayer = Node("Quiet garden. Tap anywhere to come back.", canvasRoot != null ? (Transform)canvasRoot : titleScreen.transform);
            quietLayer.transform.SetAsLastSibling();
            Fill(quietLayer.GetComponent<RectTransform>());
            var catcher = quietLayer.AddComponent<Image>();
            catcher.color = new Color(0f, 0f, 0f, 0f); // invisible, still takes the tap
            var back = quietLayer.AddComponent<Button>();
            back.transition = Selectable.Transition.None;
            back.onClick.AddListener(ExitQuiet);
            // A small sprig in the corner says how to come back, then fades away.
            Sprite sprig = MosseloraArt.Load(MosseloraArt.Sprig);
            if (sprig != null)
            {
                var cue = MosseloraArt.Ornament(quietLayer.transform, "Tap to come back", sprig, new Vector2(1f, 0f),
                    new Vector2(1f, 0f), new Vector2(44f, 44f), new Vector2(-28f, 40f));
                if (cue != null) cue.gameObject.AddComponent<CueFade>().After = gentleMotion ? 2.5f : 4f;
            }
            semanticsDirty = true;
        }

        internal void ExitQuiet()
        {
            if (hubMapView != null) hubMapView.LookOut(false);
            if (quietLayer != null) Destroy(quietLayer);
            quietLayer = null;
            foreach (GameObject hidden in quieted)
                if (hidden != null) hidden.SetActive(true);
            quieted.Clear();
            semanticsDirty = true;
        }
    }
}
