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
    /// <summary>
    /// One coherent first-screen experience: an illustrated evolving village,
    /// an approachable water puzzle, and consequential choices. Offline-first.
    /// UI is generated from code so no proprietary assets or prefab machinery
    /// are required for the playable research slice.
    /// </summary>
    public sealed class EvergroveApp : MonoBehaviour
    {
        private static readonly Color Paper = MosseloraTheme.Paper;
        private static readonly Color Ink = MosseloraTheme.Ink;
        private static readonly Color Moss = MosseloraTheme.SecondaryInk;
        private static readonly Color Leaf = MosseloraTheme.LeafAction;
        private static readonly Color Pale = MosseloraTheme.LeafSoft;
        private static readonly Color Sand = MosseloraTheme.Sand;
        private static readonly Color White = Color.white;
        private static readonly Color River = MosseloraTheme.RiverWash;

        private WorldModel world;
        private Font font;
        private Font baseFont;
        private bool languagePicker;
        private RectTransform content;
        private RectTransform scrollArea;
        private RectTransform warningViewport;
        private GameObject saveWarningBanner;
        private Text saveWarningCaption;
        private ScrollRect scroll;
        private CanvasScaler viewportScaler;
        private Rect lastSafeArea;
        private Vector2Int lastScreen;
        private bool showingMemory;
        private bool showJournal;
        private int speakingWith = -1;
        private bool confirmReset;
        private bool soundOn;
        private bool showMiniature;
        private bool animateMiniature;
        private VillageDiorama diorama;
        private VillageIllustration activeIllustration;
        private AudioSource sound;
        private AudioClip chime;
        private LocalPlaytest localPlaytest; // opt-in, in-memory only
        private bool playtestCopied;
        private bool saveFailed; // shown visibly; do not silently pretend an unsaved session is safe
        // Garden file (clipboard): a parsed garden waits for an explicit second tap.
        private bool gardenRestoreConfirm;
        private WorldModel pendingGardenRestore;
        private string gardenNoticeKey = "";
        private bool gardenNoticeIsError;
        // Keep live puzzle controls mounted during every tap. Recreating the
        // entire uGUI hierarchy on each rotation would cause GC, jank and
        // scroll-position jumps on older phones.
        private Button[] visibleTiles;
        private Image[] tileBackgrounds;
        private PipeGraphic[] tileArtwork;
        private Text[] visibleLeaks; // punctuation, not hue, denotes unmatched water ports
        private Text feedbackText;
        private Button undoButton;
        private RectTransform decisionHeading; // anchor for intentional post-solve navigation
        // Set by a rebuilt screen (ending card, journal) so a player who opens it
        // lands on its heading instead of wherever the previous screen was scrolled.
        private RectTransform screenHeading;
        private bool revealNewScreen; // one-shot: the current rebuild opened a new screen
        private const float RevealMarginPx = 16f;
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        private bool qaInMemoryOnly; // dev QA tour: never reads or writes the player's save
        // Matched M4 facilitator state; exists only in research-capable builds.
        private bool matchedEntryOpen;
        private bool confirmMatchedReplace;     // two-step replace, like confirmReset
        private int[] pendingMatchedSchedule;
        private string matchedDraft = "";
        private string matchedError = "";
        private bool matchedScheduleCopied;
#endif

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Bootstrap()
        {
            if (FindFirstObjectByType<EvergroveApp>() != null) return;
            var host = new GameObject("Evergrove");
            DontDestroyOnLoad(host);
            host.AddComponent<EvergroveApp>();
        }

        private static Color Hex(string value)
        {
            ColorUtility.TryParseHtmlString(value, out Color color);
            return color;
        }

        private void Awake()
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            qaInMemoryOnly = DevQaTour.IsRequested;
            world = qaInMemoryOnly ? WorldModel.New() : WorldStore.Load();
#else
            world = WorldStore.Load();
#endif
            // An unreadable save cannot silently be overwritten by fresh progress.
            saveFailed = WorldStore.RecoveryBlocked;
            soundOn = PlayerPrefs.GetInt("evergrove:gentle-audio", 0) == 1;
            // Default to the proven-in-code 2D art; keep the experimental
            // orthographic 3D visual opt-in until real player/device evidence.
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            // Research-only rendering options must never leak into a store
            // candidate, even when an old development preference is present.
            showMiniature = PlayerPrefs.GetInt("evergrove:visual-miniature", 0) == 1;
            animateMiniature = PlayerPrefs.GetInt("evergrove:miniature-motion", 0) == 1;
#else
            showMiniature = false;
            animateMiniature = false;
#endif
            baseFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (baseFont == null) baseFont = Font.CreateDynamicFontFromOSFont("Arial", 16);
            LocaleRuntime.Initialize();
            TextScale.Initialize();
            font = LocaleFonts.Choose(LocaleRuntime.Current, baseFont);
            sound = gameObject.AddComponent<AudioSource>();
            sound.playOnAwake = false;
            sound.volume = 0.13f;
            CreateRoot();
            Rebuild(true);
        }

        private void Update()
        {
            if (scrollArea == null) return;
            Rect safe = Screen.safeArea;
            var size = new Vector2Int(Screen.width, Screen.height);
            if (lastScreen != size || safe != lastSafeArea)
            {
                lastScreen = size;
                lastSafeArea = safe;
                FitSafeArea();
            }
        }

        private static GameObject Node(string name, Transform parent)
        {
            var obj = new GameObject(name, typeof(RectTransform));
            obj.transform.SetParent(parent, false);
            return obj;
        }

        // Non-linguistic puzzle landmarks: three blue waves = spring;
        // green stem + leaves = garden. These use only UI geometry, so
        // no language-specific glyph or font is needed on the board.
        // Screen-reader semantics still require a dedicated device-tested
        // accessibility layer (see EG-113), not just GameObject names.
        private static void IconPart(Transform parent, string name, Color color,
                                     Vector2 size, Vector2 offset, float angle = 0f)
        {
            var part = Node(name, parent);
            RectTransform rect = part.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = rect.anchorMin;
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = size;
            rect.anchoredPosition = offset;
            rect.localRotation = Quaternion.Euler(0f, 0f, angle);
            var image = part.AddComponent<Image>();
            image.color = color;
            image.raycastTarget = false;
        }

        private static void DrawEndpointIcon(Transform parent, bool spring)
        {
            if (spring)
            {
                // Sinusoidal mesh waves, not straight bars: straight stacked
                // bars read as a menu button (☰) to first-time adult players.
                var waves = parent.gameObject.AddComponent<WaterWaveIcon>();
                waves.Configure(Hex("#216A96"));
            }
            else
            {
                Color green = Hex("#2B7150");
                IconPart(parent, "Sprout stem", green, new Vector2(3f, 13f), new Vector2(0f, -3f));
                IconPart(parent, "Left leaf", green, new Vector2(9f, 6f), new Vector2(-4f, 4f), 35f);
                IconPart(parent, "Right leaf", green, new Vector2(9f, 6f), new Vector2(4f, 6f), -35f);
            }
        }

        private static void Fill(RectTransform rect)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }

        private static void Height(GameObject obj, float amount)
        {
            var element = obj.AddComponent<LayoutElement>();
            element.minHeight = amount;
            element.preferredHeight = amount;
        }

        // Floor only: the element grows with its text when text scale is raised.
        private static void MinHeight(GameObject obj, float amount)
        {
            var element = obj.AddComponent<LayoutElement>();
            element.minHeight = amount;
        }

        // Applies the single text-size scale to a generated Text.
        private void StyleText(Text text, int basePoints)
        {
            text.font = font;
            text.fontSize = TextScale.Scale(basePoints);
        }

        private static int SaveBannerHeight()
        {
            return TextScale.Scale(72);
        }

        private void FitSafeArea()
        {
            if (Screen.width <= 0 || Screen.height <= 0 || scrollArea == null) return;
            Rect safe = Screen.safeArea;
            scrollArea.anchorMin = new Vector2(safe.xMin / Screen.width, safe.yMin / Screen.height);
            scrollArea.anchorMax = new Vector2(safe.xMax / Screen.width, safe.yMax / Screen.height);
            scrollArea.offsetMin = Vector2.zero;
            scrollArea.offsetMax = Vector2.zero;
        }

        private void CreateRoot()
        {
            var canvasNode = Node("Evergrove Canvas", transform);
            var canvas = canvasNode.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = canvasNode.AddComponent<CanvasScaler>();
            viewportScaler = scaler;
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(414, 896);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;
            canvasNode.AddComponent<GraphicRaycaster>();

            var backdrop = Node("Warm paper", canvasNode.transform);
            Fill(backdrop.GetComponent<RectTransform>());
            backdrop.AddComponent<Image>().color = Paper;
            backdrop.GetComponent<Image>().raycastTarget = false;

            var scrollNode = Node("Safe scroll area", canvasNode.transform);
            scrollArea = scrollNode.GetComponent<RectTransform>();
            FitSafeArea();
            scroll = scrollNode.AddComponent<ScrollRect>();
            scroll.horizontal = false;
            scroll.vertical = true;
            scroll.movementType = ScrollRect.MovementType.Clamped;
            scroll.scrollSensitivity = 28f;
            scroll.inertia = true;

            var viewport = Node("Viewport", scrollNode.transform);
            warningViewport = viewport.GetComponent<RectTransform>();
            Fill(warningViewport);
            var viewportImage = viewport.AddComponent<Image>();
            viewportImage.color = new Color(1f, 1f, 1f, 0.004f);
            viewport.AddComponent<RectMask2D>();
            scroll.viewport = viewport.GetComponent<RectTransform>();

            var body = Node("Content", viewport.transform);
            content = body.GetComponent<RectTransform>();
            content.anchorMin = new Vector2(0f, 1f);
            content.anchorMax = new Vector2(1f, 1f);
            content.pivot = new Vector2(0.5f, 1f);
            content.anchoredPosition = Vector2.zero;
            content.sizeDelta = Vector2.zero;
            var column = body.AddComponent<VerticalLayoutGroup>();
            column.childAlignment = TextAnchor.UpperCenter;
            column.padding = new RectOffset(17, 17, 15, 36);
            column.spacing = 9;
            column.childControlWidth = true;
            column.childControlHeight = true;
            column.childForceExpandWidth = true;
            column.childForceExpandHeight = false;
            body.AddComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            scroll.content = content;

            // Save failures are high-severity, not normal scroll content.
            // The old message could be offscreen when a player rotated a tile,
            // and could stay visible after a later successful save. Keep this
            // noninteractive banner pinned inside the actual device safe area,
            // reserving space in the scrolling viewport only while it is shown.
            saveWarningBanner = Node("Persistent save warning", scrollNode.transform);
            RectTransform warningRect = saveWarningBanner.GetComponent<RectTransform>();
            warningRect.anchorMin = new Vector2(0f, 1f);
            warningRect.anchorMax = new Vector2(1f, 1f);
            warningRect.pivot = new Vector2(0.5f, 1f);
            warningRect.sizeDelta = new Vector2(0f, SaveBannerHeight());
            warningRect.anchoredPosition = Vector2.zero;
            var warningBackground = saveWarningBanner.AddComponent<Image>();
            warningBackground.color = Hex("#F9E5E0");
            warningBackground.raycastTarget = false;
            var warningTextNode = Node("Save status message", saveWarningBanner.transform);
            RectTransform warningTextRect = warningTextNode.GetComponent<RectTransform>();
            Fill(warningTextRect);
            warningTextRect.offsetMin = new Vector2(12f, 0f);
            warningTextRect.offsetMax = new Vector2(-12f, 0f);
            saveWarningCaption = warningTextNode.AddComponent<Text>();
            StyleText(saveWarningCaption, 14);
            saveWarningCaption.resizeTextForBestFit = true;
            saveWarningCaption.resizeTextMinSize = TextScale.Scale(12);
            saveWarningCaption.resizeTextMaxSize = TextScale.Scale(14);
            saveWarningCaption.color = Hex("#983E37");
            saveWarningCaption.alignment = TextAnchor.MiddleCenter;
            saveWarningCaption.raycastTarget = false;
            UpdateSaveWarning();

            if (FindFirstObjectByType<EventSystem>() == null)
            {
                var eventNode = new GameObject("Evergrove UI Event System");
                DontDestroyOnLoad(eventNode);
                eventNode.AddComponent<EventSystem>();
#if ENABLE_INPUT_SYSTEM
                // Unity 6.x defaults to the modern Input System. The legacy
                // StandaloneInputModule fails to deliver mobile UI taps when
                // Active Input Handling = Input System Package (New).
                eventNode.AddComponent<InputSystemUIInputModule>();
#elif ENABLE_LEGACY_INPUT_MANAGER
                eventNode.AddComponent<StandaloneInputModule>();
#else
                Debug.LogError("Evergrove requires Unity Input System or legacy Input Manager.");
#endif
            }
        }

        private void UpdateSaveWarning()
        {
            if (saveWarningBanner == null || saveWarningCaption == null || warningViewport == null)
                return;
            saveWarningCaption.font = font;
            saveWarningCaption.text = GameLocalization.Text("save_warning");
            saveWarningBanner.SetActive(saveFailed);
            warningViewport.offsetMax = new Vector2(0f, saveFailed ? -SaveBannerHeight() : 0f);
        }

        private Text Label(Transform parent, string name, string value, int size, int minimum,
                           Color color, TextAnchor alignment = TextAnchor.MiddleLeft)
        {
            var node = Node(name, parent);
            // Minimum is a floor for short touch/visual rhythm only. Body copy
            // passes 0: a large floor centers multi-line text inside empty space
            // (the "gap" bug). Preferred height from the Text grows with locale/font.
            var layout = node.AddComponent<LayoutElement>();
            layout.minHeight = minimum;
            var label = node.AddComponent<Text>();
            StyleText(label, size);
            label.color = color;
            label.alignment = GameLocalization.IsRtl(GameLocalization.Current) &&
                alignment == TextAnchor.MiddleLeft ? TextAnchor.MiddleRight : alignment;
            label.horizontalOverflow = HorizontalWrapMode.Wrap;
            label.verticalOverflow = VerticalWrapMode.Overflow;
            label.supportRichText = false;
            label.raycastTarget = false;
            label.text = GameLocalization.Display(value);
            return label;
        }

        private GameObject Card(Transform parent, string name, Color fill)
        {
            var node = Node(name, parent);
            var image = node.AddComponent<Image>();
            image.color = fill;
            var stack = node.AddComponent<VerticalLayoutGroup>();
            stack.childAlignment = TextAnchor.UpperCenter;
            stack.padding = new RectOffset(14, 14, 14, 14);
            stack.spacing = 8;
            stack.childControlWidth = true;
            stack.childControlHeight = true;
            stack.childForceExpandWidth = true;
            stack.childForceExpandHeight = false;
            node.AddComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            return node;
        }

        private Button Button(Transform parent, string name, string caption, Action action,
                              int height = 52, bool primary = false)
        {
            var node = Node(name, parent);
            // Minimum target height scales with text; the caption layout grows it further.
            MinHeight(node, TextScale.Scale(height));
            var stack = node.AddComponent<VerticalLayoutGroup>();
            stack.padding = new RectOffset(10, 10, 6, 6);
            stack.childAlignment = TextAnchor.MiddleCenter;
            stack.childControlWidth = true;
            stack.childControlHeight = true;
            stack.childForceExpandWidth = true;
            stack.childForceExpandHeight = true;
            var image = node.AddComponent<Image>();
            image.color = primary ? Leaf : Pale; // Primary actions are rare; choices are equal.
            var button = node.AddComponent<Button>();
            button.targetGraphic = image;
            var palette = button.colors;
            palette.highlightedColor = primary ? Hex("#337C63") : Hex("#C6DFC9");
            palette.pressedColor = primary ? Hex("#1D4C3A") : Hex("#B2CCBB");
            palette.disabledColor = Hex("#DADDD5");
            button.colors = palette;
            var textNode = Node("Caption", node.transform);
            Fill(textNode.GetComponent<RectTransform>());
            var text = textNode.AddComponent<Text>();
            StyleText(text, 15);
            text.color = primary ? White : Ink;
            text.alignment = TextAnchor.MiddleCenter;
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Overflow;
            text.raycastTarget = false;
            text.text = GameLocalization.Display(caption);
            button.onClick.AddListener(() => action());
            return button;
        }

        private GameObject Row(Transform parent, string name)
        {
            var row = Node(name, parent);
            var group = row.AddComponent<HorizontalLayoutGroup>();
            group.reverseArrangement = GameLocalization.IsRtl(GameLocalization.Current);
            group.spacing = 8;
            group.childAlignment = TextAnchor.MiddleCenter;
            group.childControlWidth = true;
            group.childControlHeight = true;
            group.childForceExpandWidth = true;
            group.childForceExpandHeight = false;
            MinHeight(row, TextScale.Scale(54));
            return row;
        }

        private void Rebuild(bool top = false)
        {
            if (content == null) return;
            UpdateSaveWarning();
            float previousPosition = scroll.verticalNormalizedPosition;
            visibleTiles = null;
            activeIllustration = null;
            tileBackgrounds = null;
            tileArtwork = null;
            visibleLeaks = null;
            feedbackText = null;
            undoButton = null;
            decisionHeading = null;
            screenHeading = null;
            for (int i = content.childCount - 1; i >= 0; i--)
            {
                GameObject old = content.GetChild(i).gameObject;
                old.SetActive(false); // Destroy() is deferred until the frame ends.
                Destroy(old);
            }

            bool living = world.Mode == PlayMode.Living;
            // Keep the first playable board in the first visible viewport,
            // rather than putting language/settings and a giant hero above it.
            bool firstPuzzle = world.Round == 0 && !world.Solved && !showingMemory;
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
            Text wordmark = Label(content, "Mosselora wordmark", GameLocalization.Text("title"),
                15, 26, Leaf);
            wordmark.fontStyle = FontStyle.Bold;
            Label(content, "Title", GameLocalization.Text("subtitle"),
                puzzleScreen ? 16 : 24, puzzleScreen ? 28 : 44, Ink);
            if (multiLanguage && settingsAtTop)
                Button(content, "Language settings",
                    GameLocalization.Text("language") + ": " +
                    GameLocalization.NativeName(GameLocalization.Current),
                    () => { languagePicker = !languagePicker; Rebuild(true); }, 48);
            if (settingsAtTop)
                TextSizeButton(content);
            if (languagePicker)
            {
                BuildLanguagePicker();
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
                        () => { confirmReset = false; Rebuild(); }, 44);
            }

            // A player must see a tappable channel before any decorative
            // illustration. The source-connected water remains legible on each
            // tile even when the optional world is below.
            if (puzzleScreen)
                BuildPuzzle();

            if (living)
            {
                VillageScenePlan scenePlan = VillageScenePlan.From(world);
                var illustration = Node(showMiniature ? "Miniature diorama" : "Living storybook art", content);
                Height(illustration, firstPuzzle ? 96 : 193); // Art never takes the first playable fold.
                bool renderedMiniature = false;
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
                if (!renderedMiniature)
                {
                    if (diorama != null) diorama.Hide();
                    var art = illustration.AddComponent<VillageIllustration>();
                    activeIllustration = art;
                    art.Show(scenePlan.Chapter, scenePlan.TreeCount,
                        scenePlan.CrossingCount, scenePlan.FlowerCount,
                        scenePlan.WaterRestored, scenePlan.WaterReachPercent);
                }
            }
            else if (diorama != null)
                diorama.Hide();

            if (living)
            {
                if (!firstPuzzle)
                    Label(content, "World consequence", LocalizedVillage.Consequence(world), 16, 52, Ink,
                        TextAnchor.MiddleCenter);
                if (world.Choices.Length > 0)
                    Label(content, "Remembered choice", LocalizedVillage.Memory(world), 13, 38, Moss,
                        TextAnchor.MiddleCenter);
            }
            else
                Label(content, "Puzzle condition", GameLocalization.Text("puzzle_desc"),
                    14, 44, Moss, TextAnchor.MiddleCenter);

            if (memoryScreen)
            {
                var moment = Card(content, "A remembered moment", Sand);
                Label(moment.transform, "Moment title", GameLocalization.Text("memory_title"), 15, 31, Leaf);
                Label(moment.transform, "Story", LocalizedVillage.MemoryEvent(world), 17, 0, Ink);
                Button(content, "Continue", GameLocalization.Text("continue"), () => {
                    showingMemory = false;
                    revealNewScreen = true; // the ending, if reached, opens at its heading
                    Rebuild(true);
                }, 62, true);
                Button(content, "Change your choice", GameLocalization.Text("change"), () => {
                    if (!world.ReconsiderLatestChoice()) return;
                    if (localPlaytest != null) localPlaytest.Reconsider(Time.unscaledTime);
                    SaveWorld();
                    showingMemory = false;
                    Rebuild(false);
                }, 47);
                SettingsRow(multiLanguage);
            }
            else if (world.Completed)
            {
                var conclusion = Card(content, "The ending", Sand);
                screenHeading = conclusion.GetComponent<RectTransform>();
                Label(conclusion.transform, "Ending title", GameLocalization.Text("ending_title"),
                    15, 31, Leaf);
                Label(conclusion.transform, "Final words", LocalizedVillage.Ending(world),
                    17, 0, Ink);
                if (living) MemoryJournal();
                Button(content, "Reset title", confirmReset ?
                    GameLocalization.Text("erase") : GameLocalization.Text("explore"),
                    () => {
                        if (!confirmReset) { confirmReset = true; Rebuild(); return; }
                        ConfirmedReset();
                    }, 62, confirmReset);
                if (confirmReset)
                    Button(content, "Cancel reset", GameLocalization.Text("keep"), () => {
                        confirmReset = false;
                        Rebuild();
                    });
            }
            if (puzzleScreen && !languagePicker)
                SettingsRow(multiLanguage);

            if (living && !world.Completed && !showingMemory && world.Choices.Length > 0)
                MemoryJournal();

            // Optional face-to-face NPC interaction. Character dialogue is
            // genuinely driven by previous world decisions, not generated text.
            // It stays below the puzzle so onboarding reaches gameplay first.
            if (living && !showingMemory)
            {
                Label(content, "Neighbors", GameLocalization.Text("neighbors"),
                    12, 27, Leaf);
                var neighbors = Row(content, "Speak with a neighbor");
                Button(neighbors.transform, "Mira", "Mira", () => OpenConversation(0), 52);
                Button(neighbors.transform, "Orin", "Orin", () => OpenConversation(1), 52);
                Button(neighbors.transform, "Fen", "Fen", () => OpenConversation(2), 52);
                if (speakingWith >= 0)
                {
                    var conversation = Card(content, "A neighbor's memory", Sand);
                    Label(conversation.transform, "Neighbor name",
                        speakingWith == 0 ? "Mira" : speakingWith == 1 ? "Orin" : "Fen", 16, 31, Leaf);
                    Label(conversation.transform, "What they say",
                        LocalizedVillage.Speak(world, speakingWith), 16, 0, Ink);
                    Button(conversation.transform, "Thanks", GameLocalization.Text("thanks"),
                        () => { speakingWith = -1; Rebuild(); }, 50, true);
                }
            }

#if UNITY_EDITOR || DEVELOPMENT_BUILD
            // Research-only controls compile only in the Editor or explicitly
            // opted-in Development Builds, never in ordinary player builds.
            // A mode comparison requires matched puzzle fixtures before any
            // research result can be used as evidence of narrative value.
            if (world.CanSwitchMode)
            {
                Label(content, "Comparison hint",
                    GameLocalization.Text("playtest"), 11, 21, Moss);
                var modes = Row(content, "Comparison modes");
                Button(modes.transform, "Living mode", GameLocalization.Text("living"), () => SwitchMode(PlayMode.Living),
                    52, living);
                Button(modes.transform, "Puzzle control", GameLocalization.Text("puzzle"), () => SwitchMode(PlayMode.PuzzleOnly),
                    52, !living);
            }

            // Matched M4 control: facilitator-only. Its strings are English
            // literals by design (the 81-key player catalog would need 30
            // unreviewed translations for dev-only UI). Nothing here compiles
            // into a player build, and the entry is hidden while an unreadable
            // save blocks writes so an unreadable save cannot be silently replaced.
            if (living && world.Choices.Length >= WorldModel.ScheduleLength)
            {
                Button(content, "Copy matched schedule", "Copy this Living run's schedule", CopyLivingSchedule, 46);
                if (matchedScheduleCopied)
                    Label(content, "Schedule copied", "Copied privately", 12, 43, Moss, TextAnchor.MiddleCenter);
            }
            if (!WorldStore.RecoveryBlocked)
            {
                Button(content, "Matched control entry",
                    matchedEntryOpen ? "Close matched control" : "Start matched control (dev)",
                    () => {
                        matchedEntryOpen = !matchedEntryOpen;
                        confirmMatchedReplace = false;
                        pendingMatchedSchedule = null;
                        matchedError = "";
                        Rebuild();
                    }, 46);
                if (matchedEntryOpen) BuildMatchedEntry();
            }

            // Puzzle-only condition must not display or switch into the
            // changing village art: doing so would confound the experiment.
            if (living)
            {
                // Two comparable art renderers using the same saved decisions.
                // Keep this manual A/B surface below the gameplay.
                Button(content, "Diorama visual comparison",
                    GameLocalization.Text(showMiniature ? "art_3d" : "art_2d"), () => {
                        showMiniature = !showMiniature;
                        if (localPlaytest != null) localPlaytest.ChangeVisual(showMiniature, Time.unscaledTime);
                        PlayerPrefs.SetInt("evergrove:visual-miniature", showMiniature ? 1 : 0);
                        PlayerPrefs.Save();
                        Rebuild();
                    }, 49);
                if (showMiniature)
                    Button(content, "Optional miniature motion",
                        GameLocalization.Text(animateMiniature ? "motion_on" : "motion_off"), () => {
                            animateMiniature = !animateMiniature;
                            PlayerPrefs.SetInt("evergrove:miniature-motion", animateMiniature ? 1 : 0);
                            PlayerPrefs.Save();
                            Rebuild();
                        }, 45);
            }

#endif
            BuildGardenFileSection();
            Button(content, "Quiet audio setting",
                GameLocalization.Text(soundOn ? "sound_on" : "sound_off"),
                () => {
                    soundOn = !soundOn;
                    PlayerPrefs.SetInt("evergrove:gentle-audio", soundOn ? 1 : 0);
                    PlayerPrefs.Save();
                    Rebuild();
                }, 44);
            if (!world.Completed && world.Round > 0 && !WorldStore.RecoveryBlocked)
            {
                Button(content, "Restart garden",
                    GameLocalization.Text(confirmReset ? "erase" : "explore"),
                    () => {
                        if (!confirmReset) { confirmReset = true; Rebuild(); return; }
                        ConfirmedReset();
                    }, 45, confirmReset);
                if (confirmReset)
                    Button(content, "Cancel", GameLocalization.Text("keep"), () => {
                        confirmReset = false;
                        Rebuild();
                    }, 44);
            }
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            if (localPlaytest == null)
            {
                Button(content, "Playtest consent", GameLocalization.Text("record"),
                    () => {
                        localPlaytest = new LocalPlaytest(world.Mode, showMiniature,
                            world.Round, Time.unscaledTime);
                        playtestCopied = false;
                        Rebuild();
                    }, 44);
                Label(content, "No tracking by default",
                    GameLocalization.Text("privacy"),
                    12, 43, Moss, TextAnchor.MiddleCenter);
            }
            else
            {
                Button(content, "Copy playtest", GameLocalization.Text("copy"),
                    () => {
                        GUIUtility.systemCopyBuffer = localPlaytest.Report(Time.unscaledTime);
                        playtestCopied = true;
                        Rebuild();
                    }, 48);
                if (playtestCopied)
                    Label(content, "Copied privately",
                        GameLocalization.Text("copied"),
                        12, 43, Moss, TextAnchor.MiddleCenter);
                Button(content, "Stop playtest", GameLocalization.Text("clear"),
                    () => { localPlaytest = null; playtestCopied = false; Rebuild(); }, 43);
            }
#endif
            Label(content, "Respectful play", GameLocalization.Text("offline"),
                12, 36, Moss, TextAnchor.MiddleCenter);
            Canvas.ForceUpdateCanvases();
            LayoutRebuilder.ForceRebuildLayoutImmediate(content);
            if (revealNewScreen && screenHeading != null) RevealAtTop(screenHeading);
            else scroll.verticalNormalizedPosition = top ? 1f : previousPosition;
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

        private void MemoryJournal()
        {
            Button(content, "Memories",
                GameLocalization.Text(showJournal ? "journal_hide" : "journal_show"), () => {
                    showJournal = !showJournal;
                    revealNewScreen = showJournal; // opening the journal lands on its title
                    Rebuild();
                });
            if (!showJournal) return;
            var journal = Card(content, "A living journal", River);
            screenHeading = journal.GetComponent<RectTransform>();
            Label(journal.transform, "Journal title", GameLocalization.Text("journal_title"), 12, 26, Leaf);
            foreach (string entry in LocalizedVillage.Journal(world))
                Label(journal.transform, "Recorded cause and effect", entry, 14, 0, Ink);
            if (world.Journal.Length == 0)
                Label(journal.transform, "Blank page", GameLocalization.Text("journal_empty"), 14, 0, Moss);
        }

        private void BuildPuzzle()
        {
            Puzzle puzzle = world.CurrentPuzzle;
            PipeInsight insight = world.Insight;
            Label(content, "Chapter",
                GameLocalization.Text("stream", world.Round + 1, PuzzleCatalog.Count) +
                (GameLocalization.Current == "en" ? "  ·  " + puzzle.Name : ""),
                13, 30, Leaf, TextAnchor.MiddleCenter);
            Label(content, "Prompt", world.Round == 0 ?
                GameLocalization.Text("first") : LocalizedVillage.Prompt(world), 14, 34, Moss,
                TextAnchor.MiddleCenter);
            // The anchor line sits directly above the board: it explains the framed tile.
            if (puzzle.AnchoredTile >= 0)
                Label(content, "Protected habitat", LocalizedVillage.EnvironmentalRule(world),
                    14, 48, Leaf, TextAnchor.MiddleCenter);
            var board = Card(content, "The water puzzle", White);
            var gridNode = Node("Water connections", board.transform);
            // Keep the entire puzzle inside a compact phone, including the
            // story card's horizontal padding and safe-area insets.
            int gap = puzzle.Size == 3 ? 7 : 5;
            float scale = viewportScaler == null ? 1f :
                Mathf.Max(0.1f, viewportScaler.scaleFactor);
            float layoutWidth = Screen.safeArea.width / scale;
            float available = Mathf.Max(225f, layoutWidth - 34f - 20f - 5f);
            int ideal = Mathf.FloorToInt((available - (puzzle.Size - 1) * gap) / puzzle.Size);
            int tileSize = Mathf.Clamp(ideal, 48, puzzle.Size == 3 ? 90 : 67);
            int span = (tileSize + gap) * puzzle.Size - gap;
            Height(gridNode, span + 1);
            var grid = gridNode.AddComponent<GridLayoutGroup>();
            grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            grid.constraintCount = puzzle.Size;
            grid.childAlignment = TextAnchor.MiddleCenter;
            grid.cellSize = new Vector2(tileSize, tileSize);
            grid.spacing = new Vector2(gap, gap);

            int[] rotations = world.Rotations;
            visibleTiles = new Button[puzzle.CellCount];
            tileBackgrounds = new Image[puzzle.CellCount];
            tileArtwork = new PipeGraphic[puzzle.CellCount];
            visibleLeaks = new Text[puzzle.CellCount];
            int seenEnds = 0;
            for (int index = 0; index < puzzle.CellCount; index++)
            {
                int tappedIndex = index;
                var tileNode = Node("Water tile", gridNode.transform);
                var image = tileNode.AddComponent<Image>();
                bool reached = insight.Flowing[index];
                image.color = MosseloraTheme.TileFill(world.Solved, reached,
                    puzzle.IsAnchored(index), world.HintStep, world.HintTile,
                    index, puzzle.Size);
                var tile = tileNode.AddComponent<Button>();
                tile.targetGraphic = image;
                // Protected tiles stay tappable: a tap explains them without a move.
                tile.interactable = !world.Solved;
                var colors = tile.colors;
                colors.highlightedColor = Hex("#BFE0DC");
                colors.pressedColor = Hex("#A3D2CC");
                tile.colors = colors;
                tile.onClick.AddListener(() => TapTile(tappedIndex));
                visibleTiles[index] = tile;
                tileBackgrounds[index] = image;

                string kind = puzzle.Kinds[index] == PipeKind.End ?
                    (++seenEnds == 1 ? "spring" : "outlet") : "pipe";
                tileNode.name = "Row " + (index / puzzle.Size + 1) + " column " +
                    (index % puzzle.Size + 1) + " " + kind +
                    (puzzle.IsAnchored(index) ? ", protected landmark" : ", tap to turn");
                // An anchored landmark is always visually distinct, even under hints.
                if (puzzle.IsAnchored(index))
                {
                    // Created before the channel so arms and endpoints draw on top.
                    var landmarkNode = Node("Protected landmark marker", tileNode.transform);
                    Fill(landmarkNode.GetComponent<RectTransform>());
                    landmarkNode.AddComponent<LandmarkGraphic>()
                        .Configure(Mathf.Clamp(world.BoardVariant, 1, 3));
                }
                var pipeNode = Node("Visible channel", tileNode.transform);
                Fill(pipeNode.GetComponent<RectTransform>());
                var pipe = pipeNode.AddComponent<PipeGraphic>();
                pipe.Configure(puzzle.Kinds[index], rotations[index],
                    insight.Flowing[index], world.Solved);
                pipe.raycastTarget = false;
                tileArtwork[index] = pipe;
                // A hollow circle denotes an open water join. Keep the
                // non-color cue; avoid alarming red error punctuation.
                var leakNode = Node("Unmatched water port marker", tileNode.transform);
                RectTransform leakRect = leakNode.GetComponent<RectTransform>();
                leakRect.anchorMin = Vector2.zero;
                leakRect.anchorMax = Vector2.zero;
                leakRect.pivot = Vector2.zero;
                leakRect.sizeDelta = new Vector2(23f, 23f);
                leakRect.anchoredPosition = new Vector2(3f, 3f);
                var leakText = leakNode.AddComponent<Text>();
                leakText.font = font;
                leakText.fontSize = 23;
                leakText.fontStyle = FontStyle.Bold;
                leakText.alignment = TextAnchor.MiddleCenter;
                leakText.color = MosseloraTheme.EarthInk;
                leakText.raycastTarget = false;
                leakText.text = insight.Leaking[index] ? "○" : "";
                visibleLeaks[index] = leakText;
                if (world.HintStep == 2 && index == world.HintTile)
                {
                    var clueNode = Node("Hint focus marker", tileNode.transform);
                    RectTransform clueRect = clueNode.GetComponent<RectTransform>();
                    clueRect.anchorMin = new Vector2(1f, 0f);
                    clueRect.anchorMax = new Vector2(1f, 0f);
                    clueRect.pivot = new Vector2(1f, 0f);
                    clueRect.sizeDelta = new Vector2(23f, 23f);
                    clueRect.anchoredPosition = new Vector2(-3f, 3f);
                    var clueLabel = clueNode.AddComponent<Text>();
                    clueLabel.font = font;
                    clueLabel.fontSize = 17;
                    clueLabel.alignment = TextAnchor.MiddleCenter;
                    clueLabel.color = Ink;
                    clueLabel.raycastTarget = false;
                    clueLabel.text = "?";
                }
                if (puzzle.Kinds[index] == PipeKind.End)
                {
                    // Visible endpoint semantics instead of relying only on
                    // screen-reader names or a color difference.
                    bool spring = seenEnds == 1;
                    var markerNode = Node(spring ? "Water spring waves" : "Garden sprout",
                        tileNode.transform);
                    RectTransform markerRect = markerNode.GetComponent<RectTransform>();
                    markerRect.anchorMin = new Vector2(0f, 1f);
                    markerRect.anchorMax = new Vector2(0f, 1f);
                    markerRect.pivot = new Vector2(0f, 1f);
                    markerRect.sizeDelta = new Vector2(25f, 23f);
                    markerRect.anchoredPosition = new Vector2(4f, -4f);
                    DrawEndpointIcon(markerNode.transform, spring);
                }
            }

            string feedback = world.Solved
                ? GameLocalization.Text("solved")
                : GameLocalization.Text("progress", insight.ConnectedCount, puzzle.CellCount, insight.LeakCount);
            feedbackText = Label(content, "Water feedback", feedback, 15, 42, Ink, TextAnchor.MiddleCenter);

            if (world.Solved)
            {
                Button(content, "Revisit water", GameLocalization.Text("undo_last"),
                    () => Act(() => world.Undo(), "undo"), 44).interactable = world.CanUndo;
                // Both puzzle-only and matched runs have no village decisions;
                // only the Living world may show choices.
                if (world.Mode != PlayMode.Living)
                    Button(content, "Next stream", GameLocalization.Text("next"), () => {
                        if (!world.NextPuzzle()) return;
                        SaveWorld();
                        Rebuild(true);
                    }, 59, true);
                else
                {
                    decisionHeading = Label(content, "Choose title", GameLocalization.Text("choose"),
                        22, 56, Leaf, TextAnchor.MiddleCenter).rectTransform;
                    for (int choiceIndex = 0; choiceIndex < 3; choiceIndex++)
                    {
                        VillageChoice choice = (VillageChoice)choiceIndex;
                        string caption = LocalizedVillage.Choice(choice) +
                            "\n" + LocalizedVillage.ChoiceOutcome(choice, world);
                        // None is visually preselected. A golden default would
                        // bias research toward the first habitat, not player care.
                        Button(content, "Choice " + choice, caption, () => MakeChoice(choice), 84,
                            false);
                    }
                }
            }
            else
            {
                var tools = Row(content, "Gentle controls");
                undoButton = Button(tools.transform, "Undo", GameLocalization.Text("undo"), () => Act(() => world.Undo(), "undo"),
                    52);
                undoButton.interactable = world.CanUndo;
                Button(tools.transform, "Hint",
                    GameLocalization.Text(world.HintStep == 2 ? "hint_turn" :
                        world.HintStep == 1 ? "hint_more" : "hint"),
                    UseGentleHint, 52);
                Button(content, "Restart", GameLocalization.Text("restart"), () => Act(() => world.Restart(), "restart"), 46);
                if (world.HintStep > 0 && world.HintTile >= 0)
                {
                    int hintRow = world.HintTile / puzzle.Size + 1;
                    int hintColumn = world.HintTile % puzzle.Size + 1;
                    Label(content, "Optional progressive clue",
                        world.HintStep == 1 ?
                            GameLocalization.Text("hint_notice", hintRow) :
                            GameLocalization.Text("hint_focus", hintRow, hintColumn),
                        14, 65, Moss, TextAnchor.MiddleCenter);
                }
            }
            Label(content, "How to play", GameLocalization.Text("how"),
                13, 56, Moss, TextAnchor.MiddleCenter);
        }

        // Compact settings row below the primary content of puzzle and post-choice
        // screens. Same controls and behavior as the top-of-screen settings.
        private void SettingsRow(bool offerLanguage)
        {
            GameObject row = Row(content, "Settings row");
            if (offerLanguage)
                Button(row.transform, "Language settings",
                    GameLocalization.Text("language") + ": " +
                    GameLocalization.NativeName(GameLocalization.Current),
                    () => { languagePicker = true; Rebuild(true); }, 48);
            TextSizeButton(row.transform);
        }

        // Language-neutral label: "Aa" plus the current size. Cycles 100/125/150%.
        private void TextSizeButton(Transform parent)
        {
            Button(parent, "Text size", "Aa " + TextScale.Percent + "%", () => {
                TextScale.Cycle();
                Rebuild();
            }, 52);
        }

        private void BuildLanguagePicker()
        {
            Label(content, "Language information", GameLocalization.Text("language_help"),
                14, 49, Moss);
            Button(content, "Auto language", GameLocalization.Text("auto"), () => {
                SelectLanguage(GameLocalization.Auto);
            }, 56, LocaleRuntime.Preference == GameLocalization.Auto);
            foreach (GameLanguage choice in LocaleRuntime.SelectableLanguages)
            {
                string captured = choice.Code;
                Button item = Button(content, "Locale " + captured, choice.NativeName, () => {
                    SelectLanguage(captured);
                }, 54, LocaleRuntime.Preference == captured);
                // The current language's font cannot be assumed to display
                // every *other* native name in this 30-script picker.
                Text caption = item.GetComponentInChildren<Text>();
                if (caption != null) caption.font = LocaleFonts.Choose(captured, baseFont);
            }
            Button(content, "Close language picker", GameLocalization.Text("continue"), () => {
                languagePicker = false;
                Rebuild(true);
            }, 58);
        }

        private void SelectLanguage(string preference)
        {
            if (!LocaleRuntime.Choose(preference))
                Debug.LogWarning("Evergrove cannot load language " + preference);
            font = LocaleFonts.Choose(GameLocalization.Current, baseFont);
            languagePicker = false;
            Rebuild(true);
        }

        private void OpenConversation(int villagerIndex)
        {
            speakingWith = villagerIndex;
            if (localPlaytest != null) localPlaytest.Talk(Time.unscaledTime);
            Rebuild();
        }

#if UNITY_EDITOR || DEVELOPMENT_BUILD
        private void SwitchMode(PlayMode mode)
        {
            if (!world.CanSwitchMode) return;
            speakingWith = -1;
            world = WorldModel.New(mode);
            if (localPlaytest != null) localPlaytest.ChangeMode(mode, Time.unscaledTime);
            SaveWorld();
            Rebuild();
        }

        private void CopyLivingSchedule()
        {
            if (world.Mode != PlayMode.Living || world.Choices.Length < WorldModel.ScheduleLength) return;
            GUIUtility.systemCopyBuffer = WorldModel.FormatSchedule(
                WorldModel.ScheduleFromChoices(world.Choices));
            matchedScheduleCopied = true;
            Rebuild();
        }

        private void BuildMatchedEntry()
        {
            var card = Card(content, "Matched control card", Sand);
            Label(card.transform, "Matched explanation",
                "Enter 11 digits, each 1, 2 or 3: the variant of boards 2 to 12. " +
                "This run shows no village choices or story.", 12, 52, Ink);
            CreateMatchedInput(card.transform);
            if (matchedError.Length > 0)
                Label(card.transform, "Schedule error", matchedError, 13, 36, Hex("#983E37"));
            if (!confirmMatchedReplace)
                Button(card.transform, "Check matched schedule", "Check schedule", RequestMatchedStart, 50, true);
            else
            {
                Label(card.transform, "Replace warning",
                    "This replaces the saved garden. Tap again to confirm.", 13, 40, Ink);
                Button(card.transform, "Confirm matched replace", GameLocalization.Text("erase"),
                    ConfirmMatchedStart, 52, true);
                Button(card.transform, "Cancel matched replace", GameLocalization.Text("keep"), () => {
                    confirmMatchedReplace = false;
                    pendingMatchedSchedule = null;
                    Rebuild();
                }, 44);
            }
        }

        private void CreateMatchedInput(Transform parent)
        {
            var node = Node("Matched schedule input", parent);
            MinHeight(node, TextScale.Scale(48));
            var background = node.AddComponent<Image>();
            background.color = White;
            var field = node.AddComponent<InputField>();

            var textNode = Node("Schedule text", node.transform);
            Fill(textNode.GetComponent<RectTransform>());
            textNode.GetComponent<RectTransform>().offsetMin = new Vector2(10f, 0f);
            textNode.GetComponent<RectTransform>().offsetMax = new Vector2(-10f, 0f);
            var text = textNode.AddComponent<Text>();
            StyleText(text, 16);
            text.color = Ink;
            text.supportRichText = false;
            text.raycastTarget = false;

            var placeholderNode = Node("Schedule placeholder", node.transform);
            Fill(placeholderNode.GetComponent<RectTransform>());
            placeholderNode.GetComponent<RectTransform>().offsetMin = new Vector2(10f, 0f);
            placeholderNode.GetComponent<RectTransform>().offsetMax = new Vector2(-10f, 0f);
            var placeholder = placeholderNode.AddComponent<Text>();
            StyleText(placeholder, 16);
            placeholder.fontStyle = FontStyle.Italic;
            placeholder.color = Moss;
            placeholder.supportRichText = false;
            placeholder.raycastTarget = false;
            placeholder.text = "e.g. 12332112312";

            field.targetGraphic = background;
            field.textComponent = text;
            field.placeholder = placeholder;
            field.contentType = InputField.ContentType.Standard;
            field.lineType = InputField.LineType.SingleLine;
            field.characterLimit = 0;
            field.text = matchedDraft;
            field.onValueChanged.AddListener(value => matchedDraft = value ?? "");
        }

        private void RequestMatchedStart()
        {
            int[] schedule;
            if (!WorldModel.TryParseSchedule((matchedDraft ?? "").Trim(), out schedule))
            {
                matchedError = "Enter exactly 11 digits, each 1, 2 or 3.";
                Rebuild();
                return;
            }
            matchedError = "";
            pendingMatchedSchedule = schedule;
            confirmMatchedReplace = true;
            Rebuild();
        }

        private void ConfirmMatchedStart()
        {
            if (pendingMatchedSchedule == null || WorldStore.RecoveryBlocked)
            {
                confirmMatchedReplace = false;
                pendingMatchedSchedule = null;
                Rebuild();
                return;
            }
            world = WorldModel.NewMatched(pendingMatchedSchedule);
            if (localPlaytest != null) localPlaytest.ChangeMode(PlayMode.PuzzleMatched, Time.unscaledTime);
            pendingMatchedSchedule = null;
            confirmMatchedReplace = false;
            matchedEntryOpen = false;
            matchedDraft = "";
            matchedError = "";
            matchedScheduleCopied = false;
            speakingWith = -1;
            showingMemory = false;
            showJournal = false;
            SaveWorld();
            Rebuild(true);
        }

#endif
        private void RefreshBoard()
        {
            if (world.Completed || visibleTiles == null) return;
            Puzzle puzzle = world.CurrentPuzzle;
            if (visibleTiles.Length != puzzle.CellCount) { Rebuild(); return; }
            PipeInsight insight = world.Insight;
            int[] angles = world.Rotations;
            for (int i = 0; i < puzzle.CellCount; i++)
            {
                tileBackgrounds[i].color = MosseloraTheme.TileFill(world.Solved,
                    insight.Flowing[i], puzzle.IsAnchored(i), world.HintStep,
                    world.HintTile, i, puzzle.Size);
                visibleTiles[i].interactable = !world.Solved;
                tileArtwork[i].Configure(puzzle.Kinds[i], angles[i],
                    insight.Flowing[i], world.Solved);
                if (visibleLeaks != null && visibleLeaks[i] != null)
                    visibleLeaks[i].text = insight.Leaking[i] ? "○" : "";
            }
            if (feedbackText != null)
                feedbackText.text = GameLocalization.Text("progress",
                    insight.ConnectedCount, puzzle.CellCount, insight.LeakCount);
            if (undoButton != null) undoButton.interactable = world.CanUndo;
            if (activeIllustration != null)
            {
                // The world responds to meaningful changes in reachable water
                // without rebuilding the puzzle or moving the scroll position.
                VillageScenePlan plan = VillageScenePlan.From(world);
                activeIllustration.Show(plan.Chapter, plan.TreeCount,
                    plan.CrossingCount, plan.FlowerCount,
                    plan.WaterRestored, plan.WaterReachPercent);
            }
        }

        private void AfterPuzzleAction(bool wasSolved)
        {
            SaveWorld();
            if (wasSolved != world.Solved)
            {
                // Completion changes the available choices and navigation,
                // so this is the one moment that merits rebuilding the view.
                Rebuild();
                if (!wasSolved && world.Solved)
                {
                    if (localPlaytest != null) localPlaytest.Solve(Time.unscaledTime);
                    PlayChime();
                    StartCoroutine(ScrollToDecision());
                }
            }
            else if (saveFailed) Rebuild();
            else RefreshBoard();
        }

        private void UseGentleHint()
        {
            bool wasSolved = world.Solved;
            int stage = world.AdvanceGentleHint();
            if (stage == 0) return;
            if (localPlaytest != null) localPlaytest.Hint(Time.unscaledTime);
            SaveWorld();
            Rebuild();
            if (!wasSolved && world.Solved)
            {
                PlayChime();
                StartCoroutine(ScrollToDecision());
            }
        }

        private void TapTile(int index)
        {
            if (!world.Solved && world.CurrentPuzzle.IsAnchored(index))
            {
                // Explains the landmark in the status line only: no move, no undo
                // entry, no save. The next real action redraws the progress text.
                string landmark = LocalizedVillage.EnvironmentalRule(world);
                if (feedbackText != null && landmark.Length > 0) feedbackText.text = landmark;
                return;
            }
            bool before = world.Solved;
            if (!world.Rotate(index)) return;
            if (localPlaytest != null) localPlaytest.Rotate(Time.unscaledTime);
            AfterPuzzleAction(before);
        }

        private void Act(Func<bool> action, string actionKind)
        {
            bool before = world.Solved;
            if (!action()) return;
            if (localPlaytest != null)
            {
                switch (actionKind)
                {
                    case "undo": localPlaytest.Undo(Time.unscaledTime); break;
                    case "hint": localPlaytest.Hint(Time.unscaledTime); break;
                    case "restart": localPlaytest.Restart(Time.unscaledTime); break;
                }
            }
            AfterPuzzleAction(before);
        }

        private IEnumerator ScrollToDecision()
        {
            // Bottom-of-page scrolling skips the actual decision and lands
            // on secondary settings/footer on compact screens. Instead show
            // the first choice header with some context above it.
            yield return null; // allow one layout frame after full completion
            if (scroll == null || decisionHeading == null || content == null ||
                scroll.viewport == null) yield break;
            Canvas.ForceUpdateCanvases();
            LayoutRebuilder.ForceRebuildLayoutImmediate(content);
            float distance = content.rect.height - scroll.viewport.rect.height;
            if (distance <= 0f) yield break;
            Vector3 worldTop = decisionHeading.TransformPoint(
                new Vector3(decisionHeading.rect.center.x, decisionHeading.rect.yMax, 0f));
            float targetTop = content.rect.yMax -
                content.InverseTransformPoint(worldTop).y;
            // Keep some of the preceding restored-world context visible.
            float context = Mathf.Min(110f, scroll.viewport.rect.height * 0.23f);
            scroll.StopMovement();
            scroll.verticalNormalizedPosition = 1f -
                Mathf.Clamp01((targetTop - context) / distance);
        }

        private void MakeChoice(VillageChoice choice)
        {
            if (!world.Choose(choice)) return;
            if (localPlaytest != null) localPlaytest.Choose(Time.unscaledTime);
            SaveWorld();
            showingMemory = true;
            showJournal = false;
            speakingWith = -1;
            PlayChime();
            Rebuild(true);
        }

        // Player-owned copy of the garden. Clipboard only; nothing leaves the device.
        // Lives in settings, below the puzzle, and never on the primary path.
        private void BuildGardenFileSection()
        {
            var card = Card(content, "Garden file", Sand);
            Button(card.transform, "Copy garden file", GameLocalization.Text("garden_copy"),
                CopyGardenFile, 52);
            if (!WorldStore.RecoveryBlocked)
            {
                if (!gardenRestoreConfirm)
                    Button(card.transform, "Restore garden file", GameLocalization.Text("garden_restore"),
                        ReadGardenFile, 52);
                else
                {
                    Label(card.transform, "Garden replace warning", GameLocalization.Text("garden_replace"),
                        13, 40, Ink);
                    Button(card.transform, "Confirm garden restore", GameLocalization.Text("erase"),
                        ConfirmGardenRestore, 52, true);
                    Button(card.transform, "Cancel garden restore", GameLocalization.Text("keep"),
                        CancelGardenRestore, 44);
                }
            }
            if (gardenNoticeKey.Length > 0)
                Label(card.transform, "Garden file notice", GameLocalization.Text(gardenNoticeKey),
                    13, 40, gardenNoticeIsError ? Hex("#983E37") : Moss);
        }

        private void SetGardenNotice(string key, bool isError)
        {
            gardenNoticeKey = key ?? "";
            gardenNoticeIsError = isError;
        }

        private void CopyGardenFile()
        {
            string stamp = DateTime.UtcNow.ToString("yyyy-MM-dd'T'HH:mm:ss'Z'", CultureInfo.InvariantCulture);
            GUIUtility.systemCopyBuffer = GardenTransfer.Export(world, stamp);
            gardenRestoreConfirm = false;
            pendingGardenRestore = null;
            SetGardenNotice("garden_copied", false);
            Rebuild();
        }

        private void ReadGardenFile()
        {
            WorldModel parsed;
            GardenImportError error;
            if (!GardenTransfer.TryImport(GUIUtility.systemCopyBuffer ?? "", out parsed, out error))
            {
                gardenRestoreConfirm = false;
                pendingGardenRestore = null;
                SetGardenNotice(GardenErrorKey(error), true);
                Rebuild();
                return;
            }
            // Nothing is replaced until the second, explicit confirmation.
            pendingGardenRestore = parsed;
            gardenRestoreConfirm = true;
            SetGardenNotice("", false);
            Rebuild();
        }

        private void ConfirmGardenRestore()
        {
            if (pendingGardenRestore == null || WorldStore.RecoveryBlocked)
            {
                CancelGardenRestore();
                return;
            }
            world = pendingGardenRestore;
            pendingGardenRestore = null;
            gardenRestoreConfirm = false;
            SetGardenNotice("", false);
            speakingWith = -1;
            showingMemory = false;
            showJournal = false;
            confirmReset = false;
            SaveWorld(); // same persistence path as every other progress change
            Rebuild(true);
        }

        private void CancelGardenRestore()
        {
            gardenRestoreConfirm = false;
            pendingGardenRestore = null;
            Rebuild();
        }

        private static string GardenErrorKey(GardenImportError error)
        {
            switch (error)
            {
                case GardenImportError.Empty: return "garden_err_empty";
                case GardenImportError.WrongFormat: return "garden_err_not_garden";
                case GardenImportError.UnsupportedFutureVersion: return "garden_err_newer";
                case GardenImportError.TooLarge: return "garden_err_too_large";
                default: return "garden_err_damaged"; // Malformed, ChecksumMismatch, InvalidSnapshot
            }
        }

        private void ConfirmedReset()
        {
            if (!WorldStore.Clear())
            {
                // Never announce a new saved garden when any old slot could
                // not be cleared; keep the in-memory world and warn instead.
                saveFailed = true;
                Rebuild();
                return;
            }
            world = WorldModel.New();
            saveFailed = false;
            confirmReset = false;
            showJournal = false;
            showingMemory = false;
            speakingWith = -1;
            Rebuild(true);
        }

        private void SaveWorld()
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            if (qaInMemoryOnly) return; // QA tour: in-memory only, no persistence
#endif
            saveFailed = !WorldStore.Save(world);
            UpdateSaveWarning(); // toggles immediately even without a full UI rebuild
            if (saveFailed)
                Debug.LogWarning("Evergrove is playable but cannot persist this change.");
        }

#if UNITY_EDITOR || DEVELOPMENT_BUILD
        // Dev-only seam for DevQaTour. Replaces the in-memory world and redraws
        // through the normal Rebuild path; refuses any world that is not in-memory.
        internal WorldModel QaWorld { get { return world; } }
        internal void QaReplaceWorld(WorldModel next)
        {
            if (!qaInMemoryOnly || next == null) return;
            world = next;
            speakingWith = -1;
            showingMemory = false;
            showJournal = false;
            confirmReset = false;
            Rebuild(true);
        }
#endif

        private void PlayChime()
        {
            if (!soundOn) return;
            if (chime == null)
            {
                const int sampleRate = 22050;
                const int length = 6200;
                var samples = new float[length];
                for (int i = 0; i < length; i++)
                {
                    float t = (float)i / sampleRate;
                    float envelope = Mathf.Pow(1f - (float)i / length, 2f);
                    samples[i] = envelope * 0.15f *
                        (Mathf.Sin(t * 2f * Mathf.PI * 523.25f) +
                         0.55f * Mathf.Sin(t * 2f * Mathf.PI * 659.25f));
                }
                chime = AudioClip.Create("Evergrove gentle chime", length, 1, sampleRate, false);
                chime.SetData(samples, 0);
            }
            sound.PlayOneShot(chime);
        }
    }
}
