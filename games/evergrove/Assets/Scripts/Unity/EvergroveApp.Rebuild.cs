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

        private void Rebuild(bool top = false)
        {
            rebuildGeneration++;
            RefreshChoicePreview();
            choicePreviewPanel = null;
            RuleToast.Dismiss();
            if (content == null) { celebrateSolve = false; return; }
            semanticsDirty = true;
            UpdateSaveWarning();
            float previousPosition = scroll.verticalNormalizedPosition;
            visibleTiles = null;
            coachBubble = null;
            goalGlows.Clear();
            hintMarkers.Clear();
            activeIllustration = null;
            activePainting = null;
            tileBackgrounds = null;
            tileArtwork = null;
            visibleLeaks = null;
            feedbackText = null;
            waterDrops = null;
            undoButton = null;
            undoRowNode = null;
            redoButton = null;
            undoStepCount = null;
            decisionHeading = null;
            decisionCards = null;
            solvedBoard = null;
            screenHeading = null;
            if (neighborLayer != null)
            {
                neighborLayer.SetActive(false);
                Destroy(neighborLayer);
                neighborLayer = null;
            }
            if (pinnedActions != null)
            {
                pinnedActions.SetActive(false);
                Destroy(pinnedActions);
                pinnedActions = null;
            }
            for (int i = content.childCount - 1; i >= 0; i--)
            {
                GameObject old = content.GetChild(i).gameObject;
                old.SetActive(false); // Destroy() is deferred until the frame ends.
                Destroy(old);
            }
            ClearChildren(menuContent);
            ClearChildren(journalContent);
            ClearChildren(chrome);

            bool living = world.Mode == PlayMode.Living;
            LandscapeGarden(living);
            // Keep the first playable board in the first visible viewport,
            // rather than putting language/settings and a giant hero above it.
            bool firstPuzzle = world.Round == 0 && !world.Solved && !showingMemory;
            OfferFirstSize(firstPuzzle); // checklist 6: text size first, before the first puzzle (EvergroveApp.FirstSize.cs)
            // The post-choice screen leads with the changed world and the new
            // memory; its settings move below the primary actions.
            bool memoryScreen = living && showingMemory && world.Choices.Length > 0;
            // Every puzzle screen is board-first: stream, anchor, board and its
            // controls come before the village art, consequence, memory and settings.
            bool puzzleScreen = !memoryScreen && !world.Completed;
            // An open language picker keeps its own toggle at the top so it can close.
            // Only the ending keeps its settings at the top.
            bool settingsAtTop = languagePicker || (!puzzleScreen && !memoryScreen);
            bool multiLanguage = LocaleRuntime.SelectableLanguages.Count > 1;
            // Sideways play keeps the height for the board; the wordmark is on the hub.
            // The memory card opens on your garden, full width and up under the
            // status bar, with the wordmark and tagline floating on it (I185).
            heroGarden = living && memoryScreen && !landscape &&
                MosseloraArt.Load("garden-map") != null && LoadGardenMap() != null;
            // Boards float over your own garden, full screen (I189): no paper page, no
            // wordmark header, no strip of garden under the board.
            bool worldShown = WorldStage(living && puzzleScreen && !landscape && !languagePicker);
            worldShownNow = worldShown;
            if (chromeBand != null) chromeBand.gameObject.SetActive(!landscape && !worldShown);
            if (worldShown) Height(Node("Room under the corner buttons", content), 50f);
            else if (!heroGarden && !(landscape && (puzzleScreen || memoryScreen))) GardenHeader(puzzleScreen);
            // Boards (the first one too, I160) open on the path, the stream name and
            // the board; the tagline lives on the hub and the story screens
            // (memory, ending), where it is part of the story, not a header.
            if (!puzzleScreen && !(landscape && memoryScreen) && !heroGarden)
            {
                // Story screens use the hub's size, so the line sits inside the
                // vine corners instead of running into them (I73).
                Text subtitle = Label(content, "Title", GameLocalization.Text("subtitle"),
                    puzzleScreen ? 17 : 19, puzzleScreen ? 28 : 34, Ink, TextAnchor.MiddleCenter);
                if (!puzzleScreen) Emphasize(subtitle);
            }
            if (languagePicker)
            {
                menuOpen = false;
                BuildLanguagePicker();
                celebrateSolve = false; // never carry a bloom into a later board
                Canvas.ForceUpdateCanvases();
                LayoutRebuilder.ForceRebuildLayoutImmediate(content);
                scroll.verticalNormalizedPosition = 1f;
                return;
            }

            // Only an explicit two-step player decision may erase save slots
            // that failed validation on launch. The game remains playable in
            // memory, but every SaveWorld call is blocked until reset.
            if (WorldStore.RecoveryBlocked)
            {
                Button(content, "Recover or erase unreadable save",
                    GameLocalization.Text(confirmReset ? "erase" : "explore"), () => {
                        if (!confirmReset) { confirmReset = true; Rebuild(); return; }
                        ConfirmedReset();
                    }, 52, confirmReset);
                if (confirmReset)
                    Button(content, "Keep unreadable files", GameLocalization.Text("keep"),
                        () => { confirmReset = false; Rebuild(); }, 48);
            }

            // A player must see a tappable channel before any decorative
            // illustration. The source-connected water remains legible on each
            // tile even when the optional world is below.
            if (puzzleScreen)
                BuildPuzzle();

            // Sideways, the village lives full height on the left (LandscapeGarden).
            if (living && !landscape && !worldShown)
            {
                VillageScenePlan scenePlan = VillageScenePlan.From(world);
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                var illustration = Node(showMiniature ? "Miniature diorama" : "Living storybook art", content);
#else
                var illustration = Node("Living storybook art", content);
#endif
                // Art never takes the first playable fold: it follows the board and controls.
                Sprite bannerArt = MosseloraArt.Load(MosseloraArt.BannerFlowing);
                float bannerWidth = PageWidth() + 34f;
                Height(illustration, bannerArt != null
                    // The memory screen's village yields a little height to the path
                    // marker, so the arrow and the next stream share one screen (I78).
                    ? Mathf.Round(bannerWidth / MosseloraArt.Aspect(bannerArt)
                        * (memoryScreen ? (TextScale.Percent > 100 ? 0.5f : 0.68f) : 1f)) // gives the taller brand logo its room (I102)
                    : (firstPuzzle ? 96 : 193));
                bool renderedMiniature = false;
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                if (showMiniature)
                {
                    if (diorama == null)
                    {
                        var stage = new GameObject("Optional orthographic world");
                        stage.transform.SetParent(transform, false);
                        diorama = stage.AddComponent<VillageDiorama>();
                    }
                    renderedMiniature = diorama.Show(scenePlan, animateMiniature);
                    if (!renderedMiniature)
                    {
                        // No missing graphics feature should block the offline game.
                        // Fall back honestly, including the visible preference label.
                        showMiniature = false;
                        PlayerPrefs.SetInt("evergrove:visual-miniature", 0);
                        PlayerPrefs.Save();
                        Debug.LogWarning("Miniature renderer unavailable; keeping the 2D garden.");
                    }
                    if (renderedMiniature)
                    {
                        var raw = illustration.AddComponent<RawImage>();
                        raw.texture = diorama.Preview;
                        raw.raycastTarget = false;
                        raw.color = Color.white;
                    }
                }
#endif
                // The garden map replaces the fixed village strip (I178): swipe it, and
                // tap to put what you made where you want it.
                bool mapped = !renderedMiniature && PageGardenMap(illustration, memoryScreen);
                // I201: on the memory screen the map takes whatever room is left above
                // the pinned buttons, on any phone and text size.
                fillTarget = mapped && memoryScreen ? illustration : null;
                if (!renderedMiniature && !mapped)
                {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                    if (diorama != null) diorama.Hide();
#endif
                    var painting = illustration.AddComponent<PaintedVillage>();
                    painting.Build(17f, PaintedVillage.ArtSeason(world));
                    if (painting.Ready)
                    {
                        int[] grown = world.Choices;
                        if (celebrateGrowth && memoryScreen && grown.Length > 0)
                            painting.GrowNewestKind = grown[grown.Length - 1];
                        celebrateGrowth = false;
                        activePainting = painting;
                        painting.Show(scenePlan.Chapter, scenePlan.TreeCount,
                            scenePlan.CrossingCount, scenePlan.FlowerCount,
                            scenePlan.WaterRestored, scenePlan.WaterReachPercent);
                    }
                    else
                    {
                        // Vector fallback when painted masters are not in the build.
                        var art = illustration.AddComponent<VillageIllustration>();
                        activeIllustration = art;
                        art.Show(scenePlan.Chapter, scenePlan.TreeCount,
                            scenePlan.CrossingCount, scenePlan.FlowerCount,
                            scenePlan.WaterRestored, scenePlan.WaterReachPercent);
                    }
                }
                // After the painting's own layers, so the villagers stand on top of it.
                if (!showingMemory && !mapped) NeighborsOnPainting(illustration.transform);
            }
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            else if (diorama != null)
                diorama.Hide();
#endif

            if (living && !showingMemory && speakingWith >= 0) NeighborConversation();
            else neighborShown = -1;
            if (living)
            {
                // Said once: the solved board already says the water flows, and
                // the memory card already tells who remembers what.
                bool solvedBoard = puzzleScreen && world.Solved;
                // Puzzle pages show only play and the place; the story lines live
                // on the hub, the memory screen and the ending.
                if (!firstPuzzle && !memoryScreen && !solvedBoard && !puzzleScreen)
                    Narrate(Label(content, "World consequence", LocalizedVillage.Consequence(world), 16, 52, Ink,
                        TextAnchor.MiddleCenter));
                if (world.Choices.Length > 0 && !memoryScreen && !puzzleScreen)
                    KeeperLine(content, "Remembered choice");
            }
            else
                Label(content, "Puzzle condition", GameLocalization.Text("puzzle_desc"),
                    14, 44, Moss, TextAnchor.MiddleCenter);

            if (memoryScreen)
            {
                var moment = Card(content, "A remembered moment", MosseloraTheme.CardFill);
                // Room above the title for the leaf sprigs at any text size.
                moment.GetComponent<VerticalLayoutGroup>().padding = new RectOffset(14, 14, landscape ? 26 : 34, 16);
                // The moment itself, painted: the neighbor doing what you chose
                // (Codex vignette), then one remembered sentence and what changed.
                int[] madeChoices = world.Choices;
                int keeper = madeChoices[madeChoices.Length - 1];
                Sprite vignette = MosseloraArt.Load(keeper == 0 ? MosseloraArt.VignetteTree
                    : keeper == 1 ? MosseloraArt.VignetteBridge : MosseloraArt.VignetteFlowers);
                Sprite rememberer = MosseloraArt.Portrait(keeper);
                // I200: one screen, no scrolling. The painting stands beside the
                // neighbour's sentence (narrower at large text, so the words keep the
                // room); no generic title; what changed is shown by the map above
                // and only spoken here.
                bool roomy = false; // stacking at 150% pushed the sentence under the buttons
                float sceneShare = TextScale.Percent > 125 ? 0.38f : 0.48f;
                Transform words = moment.transform;
                // Sideways, the same painting already fills the left side (I114).
                if (vignette != null && !landscape)
                {
                    Transform holder = moment.transform;
                    float sceneWidth = roomy ? Mathf.Min(PageWidth() - 28f, 300f) : Mathf.Round((PageWidth() - 28f) * sceneShare);
                    if (!roomy)
                    {
                        var pair = Node("Memory row", moment.transform);
                        var pairGroup = pair.AddComponent<HorizontalLayoutGroup>();
                        pairGroup.spacing = 12;
                        pairGroup.childAlignment = TextAnchor.MiddleLeft;
                        pairGroup.childControlWidth = true;
                        pairGroup.childControlHeight = true;
                        pairGroup.childForceExpandWidth = false;
                        pairGroup.childForceExpandHeight = false;
                        holder = pair.transform;
                    }
                    var scene = Node("Memory vignette", holder);
                    var sceneLayout = scene.AddComponent<LayoutElement>();
                    sceneLayout.minWidth = sceneLayout.preferredWidth = sceneWidth;
                    sceneLayout.minHeight = sceneLayout.preferredHeight =
                        Mathf.Round(sceneWidth / MosseloraArt.Aspect(vignette));
                    var sceneImage = scene.AddComponent<Image>();
                    sceneImage.sprite = vignette;
                    sceneImage.preserveAspect = true;
                    sceneImage.raycastTarget = false;
                    if (!roomy)
                    {
                        var column = Node("Memory words", holder);
                        column.AddComponent<LayoutElement>().flexibleWidth = 1f;
                        var columnGroup = column.AddComponent<VerticalLayoutGroup>();
                        columnGroup.childAlignment = TextAnchor.MiddleLeft;
                        columnGroup.childControlWidth = true;
                        columnGroup.childControlHeight = true;
                        columnGroup.childForceExpandWidth = true;
                        columnGroup.childForceExpandHeight = false;
                        words = column.transform;
                    }
                }
                // Sideways the left painting already shows who remembers, and the
                // face would push the continue arrow off a 402 pt screen (I115).
                else if (rememberer != null && !landscape)
                {
                    var face = Node("Neighbor who remembers", moment.transform);
                    Height(face, 72f);
                    var faceImage = face.AddComponent<Image>();
                    faceImage.sprite = rememberer;
                    faceImage.preserveAspect = true;
                    faceImage.raycastTarget = false;
                }
                bool beside = words != moment.transform;
                Text story = Label(words, "Story", Remembered(world.Choices, world.Choices.Length - 1), 18, 0, Ink,
                    beside ? TextAnchor.MiddleLeft : TextAnchor.MiddleCenter);
                Narrate(story, true);
                // Still read aloud: what changed in the garden (hidden, takes no room).
                Text changed = Label(words, "Story consequence", LocalizedVillage.Consequence(world), 17, 0,
                    new Color(0f, 0f, 0f, 0f), TextAnchor.MiddleCenter);
                changed.GetComponent<LayoutElement>().ignoreLayout = true;
                Sprite memorySprig = MosseloraArt.Load(MosseloraArt.Sprig);
                MosseloraArt.Ornament(moment.transform, "Sprig left", memorySprig, new Vector2(0f, 1f),
                    new Vector2(0f, 1f), new Vector2(64f, 22f), new Vector2(10f, -8f));
                MosseloraArt.Ornament(moment.transform, "Sprig right", memorySprig, new Vector2(1f, 1f),
                    new Vector2(0f, 1f), new Vector2(64f, 22f), new Vector2(-10f, -8f), true);
                // (I200: the walk stones and the next stream's sign wait on the next
                // board, where they matter; here they pushed the memory off screen.)
                Transform next = CenteredRow("Memory actions");
                Button changeButton = IconButton(next, "Change your choice", GameLocalization.Text("change"),
                    MosseloraArt.IconChange, () => {
                    if (!world.ReconsiderLatestChoice()) return;
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                    if (localPlaytest != null) localPlaytest.Reconsider(Time.unscaledTime);
#endif
                    SaveWorld();
                    showingMemory = false;
                    Rebuild(false);
                }, 76, MosseloraTheme.QuietAction, false, null, false, 0, 0, true); // a round token (I133)
                ShowChoiceToChange(changeButton, ItemArt(world.Choices.Length - 1, world.Choices[world.Choices.Length - 1]));
                Button continueButton = IconButton(next, "Continue", GameLocalization.Text("continue"),
                    MosseloraArt.IconContinue, () => {
                        showingMemory = false;
                        revealNewScreen = true; // the ending, if reached, opens at its heading
                        Rebuild(true);
                    }, 64, MosseloraTheme.WaterAction);
                Width(continueButton, 160, 1f);
                PinToFoot(next);
                if (fillTarget != null)
                    StartCoroutine(GrowToFill(fillTarget, (RectTransform)moment.transform, (RectTransform)next));
                fillTarget = null;
            }
            else if (world.Completed)
            {
                // The whole village gathers by the restored creek (falls back to the
                // everyday village painting if the festival art is missing).
                GameObject festival = Painting("Village memory painting",
                    MosseloraArt.Load(MosseloraArt.EndingFestival) != null
                        ? MosseloraArt.EndingFestival : MosseloraArt.HeroVillage, 460f);
                // The festival evening has its visitors too (I158), as the hub does.
                bool lively = gentleMotion && festival != null;
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                if (qaInMemoryOnly) lively = false;
#endif
                if (lively) festival.AddComponent<HubButterflies>();
                var conclusion = Card(content, "The ending", MosseloraTheme.CardFill);
                screenHeading = conclusion.GetComponent<RectTransform>();
                Text endingTitle = Label(conclusion.transform, "Ending title",
                    GameLocalization.Text("ending_title"), 18, 31, Leaf, TextAnchor.MiddleCenter);
                Emphasize(endingTitle);
                if (living) ChoiceTimeline(conclusion.transform);
                Text finalWords = Label(conclusion.transform, "Final words", LocalizedVillage.Ending(world),
                    19, 0, Ink, TextAnchor.MiddleCenter);
                Narrate(finalWords, true);
                if (living) MemoryJournal();
                ResetControls("Reset title", "Cancel reset", MosseloraTheme.WaterAction);
            }
            // Wherever the book shows (the memory screen included), its pages are
            // built: the book on the memory screen used to open blank (I98).
            if (living && !world.Completed && world.Choices.Length > 0)
                MemoryJournal();

            // ---- Menu sheet: settings, files, new garden, research tools, promise.
            RectTransform page = content;
            content = menuContent;
            MenuHeader(multiLanguage);
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            if (!participantView) BuildResearchControls(living);
#endif
            BuildPostcard();
            BuildStallEntries(); // EvergroveApp.Market.cs: the stall and deeper boards (v1 #74 #79 #35)
            BuildTravelEntry(); // EvergroveApp.Boat.cs: travel by boat to any open stretch (checklist 43)
            BuildRiverMapEntry(); // EvergroveApp.RiverMap.cs: "River map", one tap from the menu (checklist 45)
            BuildNameEntry(); // EvergroveApp.Name.cs: "Name your garden" (checklist 3)
            BuildDatesEntry(); // EvergroveApp.Dates.cs: "Dates to remember" (checklist 87)
            BuildGardenFileSection();
            if (!world.Completed && world.Round > 0 && !WorldStore.RecoveryBlocked)
            {
                ResetControls("Restart garden", "Cancel", MosseloraTheme.QuietAction);
            }
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            if (!participantView) BuildLocalStudyControls();
#endif
            Sprig();
            Text promise = Label(content, "Respectful play", GameLocalization.Text("offline"),
                15, 36, Ink, TextAnchor.MiddleCenter);
            Emphasize(promise);
            Button(content, "Privacy policy", GameLocalization.Text("privacy_policy"),
                () => Application.OpenURL("https://mosselora.com/privacy/"));
            content = page;
            BuildChrome(living);
            UpdateCreekSound();
            // A different screen (not a tile turn) crossfades in like a turned page.
            string screenKey = (memoryScreen ? "memory" : world.Completed ? "ending" : "puzzle") + ":" +
                world.Round + ":" + world.Solved + ":" + showJournal + ":" + languagePicker;
            if (screenKey != lastScreenKey && lastScreenKey.Length > 0 && gentleMotion) StartCoroutine(FadeInPage());
            lastScreenKey = screenKey;
            SheetMotion.Set(menuSheet, menuOpen, SheetEdge.For("Menu"), ReducedMotion);
            SheetMotion.Set(journalSheet, showJournal && !menuOpen, SheetEdge.For("Journal"), ReducedMotion);
            Canvas.ForceUpdateCanvases();
            LayoutRebuilder.ForceRebuildLayoutImmediate(menuContent);
            LayoutRebuilder.ForceRebuildLayoutImmediate(journalContent);
            LayoutRebuilder.ForceRebuildLayoutImmediate(content);
            if (menuOpen) FitSheetToPages(menuSheet, menuContent);
            else if (showJournal) FitSheetToPages(journalSheet, journalContent);
            if (revealChoicePreview && choicePreviewPanel != null) RevealChoicePreview();
            else if (revealChoices && decisionHeading != null) RevealChoices();
            else if (revealNewScreen && screenHeading != null) RevealAtTop(screenHeading);
            else scroll.verticalNormalizedPosition = top ? 1f : previousPosition;
            revealChoicePreview = false;
            revealChoices = false;
            revealNewScreen = false;
        }

        // Puts a newly opened screen's heading near the top of the viewport,
        // using the post-layout geometry, so its title is visible at once.
        private void RevealAtTop(RectTransform target)
        {
            float distance = content.rect.height - scroll.viewport.rect.height;
            if (distance <= 0f)
            {
                scroll.verticalNormalizedPosition = 1f;
                return;
            }
            scroll.StopMovement();
            Vector3 worldTop = target.TransformPoint(
                new Vector3(target.rect.center.x, target.rect.yMax, 0f));
            float targetTop = content.rect.yMax - content.InverseTransformPoint(worldTop).y;
            scroll.verticalNormalizedPosition = 1f - Mathf.Clamp01((targetTop - RevealMarginPx) / distance);
        }

        // The book wears the same parchment mark as a neighbour with news (I80)
        // while it holds a memory the player has not opened yet. It replaces the
        // bare count, which a review found meaningless on the button (I112).
        private const string JournalSeenKey = "evergrove:journal-seen";
    }
}
