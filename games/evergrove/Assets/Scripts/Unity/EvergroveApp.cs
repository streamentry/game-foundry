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
        private Font displayFont; // optional rounded heading face (iOS)
        private bool languagePicker;
        private RectTransform content;
        private RectTransform scrollArea;
        // Smart navigation: play stays on the page; everything else lives in a
        // menu sheet opened from one leaf button. The chrome (menu + journal
        // buttons) floats over the page and never scrolls away.
        private RectTransform canvasRoot;
        private RectTransform chrome;
        private GameObject menuSheet;
        private RectTransform menuContent;
        private ScrollRect menuScroll;
        private bool menuOpen;
        private GameObject titleScreen; // the painted welcome on every launch
        private GameObject coachBubble;  // stream-1 teaching bubble, gone after the first move
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
        // WCAG 2.3.3: motion triggered by play (pipe swing, bloom, butterfly,
        // glide) can be switched off. Off means snap, still and instant.
        private const string GentleMotionKey = "evergrove:gentle-motion:v1";
        private bool gentleMotion = true;
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        private bool showMiniature;
        // Decorative motion allowed (true) or reduced (false, the default).
        // Persisted in PlayerPrefs, separate from the save.
        private const string ReducedMotionKey = "evergrove:miniature-motion";
        private bool animateMiniature;
        private VillageDiorama diorama;
#endif
        private VillageIllustration activeIllustration;
        private PaintedVillage activePainting; // watercolor skin; same causal inputs
        private AudioSource sound;
        private AudioClip chime;
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        private LocalPlaytest localPlaytest; // opt-in, in-memory only
        private bool playtestCopied;
#endif
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
        private int rebuildGeneration; // lets a pending glide notice any later rebuild
        private bool celebrateSolve;
        private bool celebrateGrowth; // one-shot: the memory screen shows the new landmark growing
        private string lastScreenKey = "";
        private CanvasGroup pageFade; // one-shot: the rebuild after the solving tap blooms
        private Image[] waterDrops; // one painted droplet per tile the water can reach
        private Button undoButton;
        private RectTransform decisionHeading; // anchor for intentional post-solve navigation
        // Set by a rebuilt screen (ending card, journal) so a player who opens it
        // lands on its heading instead of wherever the previous screen was scrolled.
        private RectTransform screenHeading;
        private bool revealNewScreen; // one-shot: the current rebuild opened a new screen
        private const float RevealMarginPx = 16f;
        // EG-113: the screen-reader hierarchy is republished once per frame in
        // which the visible screen changed (see ScreenReaderBridge).
        private bool semanticsDirty;
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        private bool semanticsLogged; // dev QA: log the first puzzle's node list once
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
            gentleMotion = PlayerPrefs.GetInt(GentleMotionKey, 1) == 1;
            PipeGraphic.Animate = gentleMotion;
            // Default to the proven-in-code 2D art; keep the experimental
            // orthographic 3D visual opt-in until real player/device evidence.
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            // Research-only rendering options must never leak into a store
            // candidate, even when an old development preference is present.
            showMiniature = PlayerPrefs.GetInt("evergrove:visual-miniature", 0) == 1;
            // The one reduced-motion preference (EG-114). Unity 6000.3 exposes no
            // OS reduce-motion API, so the default is 0: no decorative motion.
            // Every consumer reads this flag; when it is 0 the miniature is a
            // still frame that shows the same causal state as the animated one.
            animateMiniature = PlayerPrefs.GetInt(ReducedMotionKey, 0) == 1;
#endif
            baseFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (baseFont == null) baseFont = Font.CreateDynamicFontFromOSFont("Arial", 16);
            displayFont = RoundedDisplayFont();
            LocaleRuntime.Initialize();
            TextScale.Initialize();
            BoardSize.Initialize();
            font =LocaleFonts.Choose(LocaleRuntime.Current, baseFont);
            sound = gameObject.AddComponent<AudioSource>();
            sound.playOnAwake = false;
            sound.volume = 0.13f;
            CreateRoot();
            Rebuild(true);
#if UNITY_IOS || UNITY_ANDROID
            UnityEngine.Accessibility.AssistiveSupport.screenReaderStatusChanged += OnScreenReaderStatusChanged;
#endif
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            ScreenReaderBridge.LogStatus("awake");
#endif
        }

        // Rounded bold display face for English headings and actions, as in the
        // painted references. iOS ships Arial Rounded MT Bold (a single upright
        // face); elsewhere headings fall back to the runtime face in bold. Never bundled.
        private static Font RoundedDisplayFont()
        {
            try
            {
                foreach (string name in Font.GetOSInstalledFontNames())
                    if (name.StartsWith("Arial Rounded MT", StringComparison.OrdinalIgnoreCase))
                        return Font.CreateDynamicFontFromOSFont(name, 17);
            }
            catch (Exception exception)
            {
                Debug.LogWarning("Mosselora OS font lookup unavailable: " + exception.GetType().Name);
            }
            return null;
        }

        // Headings and action captions: rounded display face in English, bold elsewhere.
        private void Emphasize(Text text)
        {
            if (displayFont != null && font == baseFont && GameLocalization.Current == "en")
            {
                text.font = displayFont;
                text.fontStyle = FontStyle.Normal;
            }
            else
                text.fontStyle = FontStyle.Bold;
        }

        private void OnDestroy()
        {
#if UNITY_IOS || UNITY_ANDROID
            UnityEngine.Accessibility.AssistiveSupport.screenReaderStatusChanged -= OnScreenReaderStatusChanged;
#endif
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

        // Runs after this frame's layout, so every node frame is final.
        // Does nothing on desktop unless a dev QA tour asks for the log.
        private void LateUpdate()
        {
            if (!semanticsDirty || content == null) return;
            semanticsDirty = false;
            bool logThisScreen = false;
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            logThisScreen = DevQaTour.IsRequested && !semanticsLogged && visibleTiles != null;
#endif
            if (!logThisScreen && !ScreenReaderBridge.Active) return;
            var nodes = ScreenReaderBridge.Collect(canvasRoot != null ? canvasRoot : content, DescribeButton);
            ScreenReaderBridge.Publish(nodes);
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            if (logThisScreen)
            {
                semanticsLogged = true;
                ScreenReaderBridge.LogForQa(nodes);
            }
#endif
        }

#if UNITY_IOS || UNITY_ANDROID
        private void OnScreenReaderStatusChanged(bool enabled)
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            ScreenReaderBridge.LogStatus("changed-to-" + (enabled ? "on" : "off"));
#endif
            semanticsDirty = true;
        }
#endif

        // Button label/hint override. Tiles speak the same state RefreshBoard draws.
        private ScreenReaderBridge.Speech? DescribeButton(Button button)
        {
            if (visibleTiles == null || world == null) return null;
            int index = Array.IndexOf(visibleTiles, button);
            return index < 0 ? (ScreenReaderBridge.Speech?)null : TileSpeech(index);
        }

        private ScreenReaderBridge.Speech TileSpeech(int index)
        {
            Puzzle puzzle = world.CurrentPuzzle;
            PipeInsight insight = world.Insight;
            int firstEnd = Array.IndexOf(puzzle.Kinds, PipeKind.End);
            string kind;
            if (puzzle.Kinds[index] == PipeKind.End)
                kind = GameLocalization.Text(index == firstEnd ? "sr_spring" : "sr_outlet");
            else
                kind = GameLocalization.Text(puzzle.Kinds[index] == PipeKind.Corner ? "sr_corner" : "sr_straight");
            int row = index / puzzle.Size + 1;
            int column = index % puzzle.Size + 1;
            string label = GameLocalization.Text(insight.Flowing[index] ? "sr_tile_wet" : "sr_tile_dry",
                row, column, kind);
            bool protectedTile = puzzle.IsAnchored(index);
            // The landmark sentence already says the pipe is protected and cannot rotate.
            if (protectedTile) label += " " + LocalizedVillage.EnvironmentalRule(world);
            string hint = world.Solved || protectedTile ? "" : GameLocalization.Text("how");
            return new ScreenReaderBridge.Speech(label, hint);
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

        private static void DrawEndpointIcon(Transform parent, bool spring, float scale)
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
                IconPart(parent, "Sprout stem", green, new Vector2(3f, 13f) * scale, new Vector2(0f, -3f) * scale);
                IconPart(parent, "Left leaf", green, new Vector2(9f, 6f) * scale, new Vector2(-4f, 4f) * scale, 35f);
                IconPart(parent, "Right leaf", green, new Vector2(9f, 6f) * scale, new Vector2(4f, 6f) * scale, -35f);
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
            canvasRoot = canvasNode.GetComponent<RectTransform>();
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
            BuildMenuSheet(scrollNode.transform);
            // Header band: a solid parchment strip as tall as the chrome, then a
            // soft fade. It appears once the page scrolls, so scrolled content
            // never sits under (or beside) the menu and memory buttons.
            var bandNode = Node("Chrome paper band", scrollNode.transform);
            var bandRect = bandNode.GetComponent<RectTransform>();
            bandRect.anchorMin = new Vector2(0f, 1f);
            bandRect.anchorMax = new Vector2(1f, 1f);
            bandRect.pivot = new Vector2(0.5f, 1f);
            bandRect.sizeDelta = new Vector2(0f, 104f);
            var bandGroup = bandNode.AddComponent<CanvasGroup>();
            bandGroup.blocksRaycasts = false;
            bandGroup.alpha = 0f;
            var solidNode = Node("Band solid", bandNode.transform);
            var solidRect = solidNode.GetComponent<RectTransform>();
            solidRect.anchorMin = new Vector2(0f, 1f);
            solidRect.anchorMax = new Vector2(1f, 1f);
            solidRect.pivot = new Vector2(0.5f, 1f);
            solidRect.sizeDelta = new Vector2(0f, 74f);
            var solidImage = solidNode.AddComponent<Image>();
            solidImage.color = Paper;
            solidImage.raycastTarget = false;
            var fadeNode = Node("Band fade", bandNode.transform);
            var fadeRect = fadeNode.GetComponent<RectTransform>();
            fadeRect.anchorMin = new Vector2(0f, 1f);
            fadeRect.anchorMax = new Vector2(1f, 1f);
            fadeRect.pivot = new Vector2(0.5f, 1f);
            fadeRect.sizeDelta = new Vector2(0f, 30f);
            fadeRect.anchoredPosition = new Vector2(0f, -74f);
            var fadeImage = fadeNode.AddComponent<Image>();
            fadeImage.sprite = MosseloraArt.TopFade();
            fadeImage.color = Paper;
            fadeImage.raycastTarget = false;
            ScrollFade.Attach(bandGroup, content);
            var chromeNode = Node("Game chrome", scrollNode.transform);
            chrome = chromeNode.GetComponent<RectTransform>();
            chrome.anchorMin = new Vector2(0f, 1f);
            chrome.anchorMax = new Vector2(1f, 1f);
            chrome.pivot = new Vector2(0.5f, 1f);
            chrome.sizeDelta = new Vector2(0f, 70f);
            chrome.anchoredPosition = Vector2.zero;
            // The sheet must cover the chrome when open.
            menuSheet.transform.SetAsLastSibling();
            BuildTitleScreen(canvasNode.transform);

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
            // An unreadable saved garden is not a write failure: say so, so players
            // do not hunt for a device-storage problem that does not exist.
            saveWarningCaption.text = GameLocalization.Text(
                WorldStore.RecoveryBlocked ? "save_unreadable" : "save_warning");
            if (CjkLineBreakFixer.NeedsFitting(GameLocalization.Current))
                CjkLineBreakFixer.Attach(saveWarningCaption, saveWarningCaption.text);
            saveWarningBanner.SetActive(saveFailed);
            warningViewport.offsetMax = new Vector2(0f, saveFailed ? -SaveBannerHeight() : 0f);
        }

        // Right-to-left locales mirror left-aligned text to the right edge. Centered text stays centered.
        private static TextAnchor RightAligned(TextAnchor alignment)
        {
            switch (alignment)
            {
                case TextAnchor.UpperLeft: return TextAnchor.UpperRight;
                case TextAnchor.MiddleLeft: return TextAnchor.MiddleRight;
                case TextAnchor.LowerLeft: return TextAnchor.LowerRight;
                default: return alignment;
            }
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
            label.alignment = GameLocalization.IsRtl(GameLocalization.Current)
                ? RightAligned(alignment) : alignment;
            label.horizontalOverflow = HorizontalWrapMode.Wrap;
            label.verticalOverflow = VerticalWrapMode.Overflow;
            label.supportRichText = false;
            label.raycastTarget = false;
            string shown = GameLocalization.Display(value);
            if (CjkLineBreakFixer.NeedsFitting(GameLocalization.Current))
                CjkLineBreakFixer.Attach(label, shown);
            else
                label.text = shown;
            return label;
        }

        private GameObject Card(Transform parent, string name, Color fill)
        {
            var node = Node(name, parent);
            var image = node.AddComponent<Image>();
            MosseloraArt.Round(image, fill, 18);
            image.raycastTarget = false;
            if (fill.a > 0.01f)
            {
                MosseloraArt.Edge(node.transform, MosseloraTheme.CardEdge, 18);
                MosseloraArt.Lift(image, 2f, 0.10f);
            }
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
            return Button(parent, name, caption, action, height,
                primary ? MosseloraTheme.ForestAction : MosseloraTheme.QuietAction);
        }

        // Toned buttons carry white captions (all tones pass 4.5:1); the quiet
        // parchment tone carries ink and a drawn edge, as in the painted references.
        private Button Button(Transform parent, string name, string caption, Action action,
                              int height, Color tone)
        {
            bool quiet = tone == MosseloraTheme.QuietAction;
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
            MosseloraArt.Round(image, tone, 16); // Primary actions are rare; choices are equal.
            MosseloraArt.Lift(image, 2f, quiet ? 0.10f : 0.22f);
            if (quiet) MosseloraArt.Edge(node.transform, MosseloraTheme.CardEdge, 16);
            var button = node.AddComponent<Button>();
            button.targetGraphic = image;
            button.colors = PressTint();
            PressSquash.Add(node);
            var textNode = Node("Caption", node.transform);
            Fill(textNode.GetComponent<RectTransform>());
            var text = textNode.AddComponent<Text>();
            StyleText(text, 16);
            text.color = quiet ? Ink : White;
            Emphasize(text);
            text.alignment = TextAnchor.MiddleCenter;
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Overflow;
            text.raycastTarget = false;
            string shownCaption = GameLocalization.Display(caption);
            if (CjkLineBreakFixer.NeedsFitting(GameLocalization.Current))
                CjkLineBreakFixer.Attach(text, shownCaption);
            else
                text.text = shownCaption;
            button.onClick.AddListener(() => action());
            return button;
        }

        // Multipliers over the tone, so pressed and disabled states stay in palette.
        private static ColorBlock PressTint()
        {
            ColorBlock palette = ColorBlock.defaultColorBlock;
            palette.normalColor = Color.white;
            palette.highlightedColor = Color.white; // touch screens: no hover state
            palette.selectedColor = Color.white;
            // Warm press (sunlit, not grey) so parchment never looks dirty.
            palette.pressedColor = new Color(0.94f, 0.88f, 0.76f);
            palette.disabledColor = new Color(0.86f, 0.86f, 0.84f, 0.55f);
            palette.fadeDuration = 0.08f;
            return palette;
        }

        // Icon-first control. The caption stays as a hidden "Caption" Text so
        // screen readers (ScreenReaderBridge.CaptionOf) and QA keep the same
        // label; showCaption puts it beside the icon for rare or destructive
        // actions where a picture alone is not enough. badge shows live state.
        private Button IconButton(Transform parent, string name, string caption, string icon,
                                  Action action, int height, Color tone,
                                  bool showCaption = false, string badge = null,
                                  bool captionBelow = false, int pips = 0, int pipsOn = 0)
        {
            Sprite glyph = MosseloraArt.Load(icon);
            if (glyph == null) return Button(parent, name, caption, action, height, tone);
            bool quiet = tone == MosseloraTheme.QuietAction;
            Color ink = quiet ? Ink : White;
            var node = Node(name, parent);
            if (captionBelow) showCaption = true; // before sizing, so the caption's text scale counts
            MinHeight(node, showCaption ? TextScale.Scale(height) : height);
            var image = node.AddComponent<Image>();
            MosseloraArt.Round(image, tone, 18);
            MosseloraArt.Lift(image, 2f, quiet ? 0.10f : 0.24f);
            if (quiet) MosseloraArt.Edge(node.transform, MosseloraTheme.CardEdge, 18);
            var button = node.AddComponent<Button>();
            button.targetGraphic = image;
            button.colors = PressTint();
            button.onClick.AddListener(() => action());
            PressSquash.Add(node);
            HorizontalOrVerticalLayoutGroup group = captionBelow
                ? (HorizontalOrVerticalLayoutGroup)node.AddComponent<VerticalLayoutGroup>()
                : node.AddComponent<HorizontalLayoutGroup>();
            if (!captionBelow)
                ((HorizontalLayoutGroup)group).reverseArrangement = GameLocalization.IsRtl(GameLocalization.Current);
            group.padding = captionBelow ? new RectOffset(6, 6, 8, 6) : new RectOffset(12, 12, 6, 6);
            group.spacing = captionBelow ? 2 : 8;
            group.childAlignment = TextAnchor.MiddleCenter;
            group.childControlWidth = true;
            group.childControlHeight = true;
            group.childForceExpandWidth = false;
            group.childForceExpandHeight = false;
            float iconSize = Mathf.Round(height * (captionBelow ? 0.40f : showCaption ? 0.42f : 0.52f));
            var iconNode = Node("Icon", node.transform);
            var iconLayout = iconNode.AddComponent<LayoutElement>();
            iconLayout.minWidth = iconLayout.preferredWidth = iconSize;
            iconLayout.minHeight = iconLayout.preferredHeight = iconSize;
            var iconImage = iconNode.AddComponent<Image>();
            iconImage.sprite = glyph;
            iconImage.preserveAspect = true;
            iconImage.color = ink;
            iconImage.raycastTarget = false;
            if (pips > 0)
            {
                // Level shown as filled dots, not words or numbers.
                var dots = Node("Level dots", node.transform);
                var dotRow = dots.AddComponent<HorizontalLayoutGroup>();
                dotRow.spacing = 4;
                dotRow.childAlignment = TextAnchor.MiddleCenter;
                dotRow.childControlWidth = true;
                dotRow.childControlHeight = true;
                dotRow.childForceExpandWidth = false;
                dotRow.childForceExpandHeight = false;
                for (int i = 0; i < pips; i++)
                {
                    var dot = Node(i < pipsOn ? "Level on" : "Level off", dots.transform);
                    var dotLayout = dot.AddComponent<LayoutElement>();
                    dotLayout.minWidth = dotLayout.preferredWidth = 8;
                    dotLayout.minHeight = dotLayout.preferredHeight = 8;
                    var dotImage = dot.AddComponent<Image>();
                    dotImage.sprite = MosseloraArt.Rounded(4);
                    dotImage.type = Image.Type.Simple;
                    dotImage.color = i < pipsOn ? ink : new Color(ink.r, ink.g, ink.b, 0.32f);
                    dotImage.raycastTarget = false;
                }
            }
            if (badge != null)
            {
                Text value = Label(node.transform, "Value", badge, 15, 0, ink, TextAnchor.MiddleCenter);
                Emphasize(value);
                // State badges are short numbers; never wrap them ("150%" not "150 / %").
                value.horizontalOverflow = HorizontalWrapMode.Overflow;
                value.fontSize = Mathf.Min(value.fontSize, 17);
                var valueLayout = value.GetComponent<LayoutElement>();
                valueLayout.minWidth = value.preferredWidth;
            }
            var textNode = Node("Caption", node.transform);
            var text = textNode.AddComponent<Text>();
            StyleText(text, captionBelow ? 13 : 16);
            text.alignment = TextAnchor.MiddleCenter;
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Overflow;
            text.raycastTarget = false;
            text.text = GameLocalization.Display(caption);
            if (showCaption)
            {
                text.color = ink;
                Emphasize(text);
                textNode.AddComponent<LayoutElement>().flexibleWidth = 0;
            }
            else
            {
                // Spoken, not shown.
                text.color = new Color(ink.r, ink.g, ink.b, 0f);
                textNode.AddComponent<LayoutElement>().ignoreLayout = true;
                Fill(textNode.GetComponent<RectTransform>());
                // Shown on press-and-hold, never on a plain tap.
                IconTip.Add(node, text, Ink, MosseloraTheme.CardFill);
            }
            return button;
        }

        // The village's story at a glance: every choice, in order, as its
        // painted icon (no text, so screen readers hear only the journal lines).
        private void ChoiceTimeline(Transform parent)
        {
            int[] chosen = world.Choices;
            if (chosen.Length == 0) return;
            var strip = Node("Choice timeline", parent);
            var grid = strip.AddComponent<GridLayoutGroup>();
            grid.cellSize = new Vector2(50f, 46f);
            grid.spacing = new Vector2(4f, 6f);
            grid.childAlignment = TextAnchor.MiddleCenter;
            grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            grid.constraintCount = Mathf.Min(6, chosen.Length);
            int rows = (chosen.Length + grid.constraintCount - 1) / grid.constraintCount;
            Height(strip, rows * 46f + (rows - 1) * 6f);
            for (int i = 0; i < chosen.Length; i++)
            {
                Sprite icon = MosseloraArt.ChoiceIcon((VillageChoice)chosen[i]);
                var cell = Node("Chapter choice", strip.transform);
                if (icon != null)
                {
                    var picture = Node("Picture", cell.transform);
                    var pictureRect = picture.GetComponent<RectTransform>();
                    pictureRect.anchorMin = Vector2.zero;
                    pictureRect.anchorMax = Vector2.one;
                    pictureRect.offsetMin = pictureRect.offsetMax = Vector2.zero;
                    var pictureImage = picture.AddComponent<Image>();
                    pictureImage.sprite = icon;
                    pictureImage.preserveAspect = true;
                    pictureImage.raycastTarget = false;
                }
            }
        }

        // Same rule as PipeRules.Inspect leaks: a port with no partner
        // (board edge or a neighbor without the facing port) is open.
        private static int OpenPorts(Puzzle puzzle, int[] rotations, int tile)
        {
            int mask = 0;
            for (int d = 0; d < 4; d++)
            {
                if (!PipeRules.HasPort(puzzle.Kinds[tile], rotations[tile], d)) continue;
                int other = PipeRules.Neighbor(tile, d, puzzle.Size);
                if (other < 0 || !PipeRules.HasPort(puzzle.Kinds[other], rotations[other], (d + 2) % 4))
                    mask |= 1 << d;
            }
            return mask;
        }

        // Progress at a glance: a droplet per tile, filled as real water reaches it.
        // The sentence below stays as the exact, spoken statement.
        private void WaterMeter(int tiles, int reached)
        {
            Sprite drop = MosseloraArt.Load(MosseloraArt.Droplet);
            if (drop == null) return;
            Transform row = CenteredRow("Water meter", tiles > 9 ? 3f : 6f);
            MinHeight(row.gameObject, 26);
            waterDrops = new Image[tiles];
            float height = tiles > 9 ? 19f : 23f;
            for (int i = 0; i < tiles; i++)
            {
                var node = Node("Water drop", row);
                var layout = node.AddComponent<LayoutElement>();
                layout.minWidth = layout.preferredWidth = height * MosseloraArt.Aspect(drop);
                layout.minHeight = layout.preferredHeight = height;
                var image = node.AddComponent<Image>();
                image.sprite = drop;
                image.preserveAspect = true;
                image.raycastTarget = false;
                waterDrops[i] = image;
            }
            PaintDrops(reached);
        }

        private void PaintDrops(int reached)
        {
            if (waterDrops == null) return;
            for (int i = 0; i < waterDrops.Length; i++)
                waterDrops[i].color = i < reached
                    ? Color.white
                    : new Color(0.55f, 0.50f, 0.42f, 0.28f); // dry: faded earth silhouette
        }

        // A calm daily ritual entry: the village painting full-bleed, the
        // wordmark, the stones walked so far and one big play button. No
        // streaks, no "you were away" - the garden simply waited.
        private void BuildTitleScreen(Transform host)
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            if (qaInMemoryOnly) return; // the scripted QA tour starts on the board
#endif
            Sprite hero = MosseloraArt.Load(MosseloraArt.HeroVillage);
            if (hero == null) return;
            titleScreen = Node("Title screen", host);
            Fill(titleScreen.GetComponent<RectTransform>());
            titleScreen.AddComponent<Image>().color = Paper;

            // The village at the top, clipped to the upper half.
            var window = Node("Village window", titleScreen.transform);
            var windowRect = window.GetComponent<RectTransform>();
            windowRect.anchorMin = new Vector2(0f, 0.46f);
            windowRect.anchorMax = Vector2.one;
            windowRect.offsetMin = windowRect.offsetMax = Vector2.zero;
            window.AddComponent<RectMask2D>();
            var art = Node("Village at the start", window.transform);
            var artRect = art.GetComponent<RectTransform>();
            artRect.anchorMin = new Vector2(0f, 1f);
            artRect.anchorMax = new Vector2(1f, 1f);
            artRect.pivot = new Vector2(0.5f, 1f);
            artRect.offsetMin = artRect.offsetMax = Vector2.zero;
            var artImage = art.AddComponent<Image>();
            artImage.sprite = hero;
            artImage.raycastTarget = false;
            var fitter = art.AddComponent<AspectRatioFitter>();
            fitter.aspectMode = AspectRatioFitter.AspectMode.WidthControlsHeight;
            fitter.aspectRatio = MosseloraArt.Aspect(hero);
            var melt = Node("Village melt", window.transform);
            var meltRect = melt.GetComponent<RectTransform>();
            meltRect.anchorMin = Vector2.zero;
            meltRect.anchorMax = new Vector2(1f, 0.30f);
            meltRect.offsetMin = meltRect.offsetMax = Vector2.zero;
            var meltImage = melt.AddComponent<Image>();
            meltImage.sprite = MosseloraArt.TopFade();
            meltImage.color = Paper;
            meltImage.rectTransform.localScale = new Vector3(1f, -1f, 1f);
            meltImage.raycastTarget = false;

            // Your creek, as you left it: real water, your landmarks, your neighbors.
            var strip = Node("Your village", titleScreen.transform);
            var stripRect = strip.GetComponent<RectTransform>();
            stripRect.anchorMin = new Vector2(0f, 0.33f);
            stripRect.anchorMax = new Vector2(1f, 0.52f);
            stripRect.offsetMin = stripRect.offsetMax = Vector2.zero;
            var painting = strip.AddComponent<PaintedVillage>();
            painting.Build(0f);
            if (painting.Ready)
            {
                VillageScenePlan plan = VillageScenePlan.From(world);
                painting.Show(plan.Chapter, plan.TreeCount, plan.CrossingCount, plan.FlowerCount,
                    plan.WaterRestored, plan.WaterReachPercent);
            }
            if (world.Mode == PlayMode.Living) NeighborsOnPainting(strip.transform, true);

            var stack = Node("Title stack", titleScreen.transform);
            var stackRect = stack.GetComponent<RectTransform>();
            stackRect.anchorMin = new Vector2(0f, 0.03f);
            stackRect.anchorMax = new Vector2(1f, 0.33f);
            stackRect.offsetMin = new Vector2(24f, 0f);
            stackRect.offsetMax = new Vector2(-24f, 0f);
            var column = stack.AddComponent<VerticalLayoutGroup>();
            column.childAlignment = TextAnchor.MiddleCenter;
            column.spacing = 12;
            column.childControlWidth = true;
            column.childControlHeight = true;
            column.childForceExpandWidth = true;
            column.childForceExpandHeight = false;
            Sprite mark = MosseloraArt.Load(MosseloraArt.Wordmark);
            if (mark != null)
            {
                var markNode = Node("Mosselora wordmark", stack.transform);
                Height(markNode, 56f);
                var markImage = markNode.AddComponent<Image>();
                markImage.sprite = mark;
                markImage.preserveAspect = true;
                markImage.raycastTarget = false;
            }
            Text tagline = Label(stack.transform, "Title", GameLocalization.Text("subtitle"), 17, 0, Ink,
                TextAnchor.MiddleCenter);
            Emphasize(tagline);
            // How far along the creek this garden has walked: resumes, never resets.
            if (!world.Completed) StreamStones(world.CurrentPuzzle, stack.transform, false);
            Transform play = CenteredRowIn(stack.transform, "Play row");
            Button start = IconButton(play, "Play", GameLocalization.Text("continue"),
                MosseloraArt.IconContinue, CloseTitle, 80, MosseloraTheme.WaterAction);
            Width(start, 168);
            titleScreen.transform.SetAsLastSibling();
        }

        private void CloseTitle()
        {
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

        // A parchment sheet that rises over the game; tapping the dimmed garden
        // behind it closes it. Built once; its contents are rebuilt with the page.
        private void BuildMenuSheet(Transform host)
        {
            menuSheet = Node("Menu sheet", host);
            Fill(menuSheet.GetComponent<RectTransform>());
            var dim = menuSheet.AddComponent<Image>();
            dim.color = new Color(0.08f, 0.14f, 0.11f, 0.42f);
            var dimButton = menuSheet.AddComponent<Button>();
            dimButton.transition = Selectable.Transition.None;
            dimButton.onClick.AddListener(() => SetMenu(false));
            var panel = Node("Menu panel", menuSheet.transform);
            var panelRect = panel.GetComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(0f, 0f);
            panelRect.anchorMax = new Vector2(1f, 0.86f);
            panelRect.offsetMin = new Vector2(8f, 0f);
            panelRect.offsetMax = new Vector2(-8f, 0f);
            var panelImage = panel.AddComponent<Image>();
            MosseloraArt.Round(panelImage, MosseloraTheme.Paper, 26);
            MosseloraArt.Lift(panelImage, 4f, 0.25f);
            MosseloraArt.Edge(panel.transform, MosseloraTheme.CardEdge, 26);
            menuScroll = panel.AddComponent<ScrollRect>();
            menuScroll.horizontal = false;
            menuScroll.movementType = ScrollRect.MovementType.Clamped;
            menuScroll.scrollSensitivity = 28f;
            var view = Node("Menu viewport", panel.transform);
            var viewRect = view.GetComponent<RectTransform>();
            Fill(viewRect);
            viewRect.offsetMin = new Vector2(0f, 4f);
            viewRect.offsetMax = new Vector2(0f, -4f);
            view.AddComponent<RectMask2D>();
            menuScroll.viewport = viewRect;
            var body = Node("Menu content", view.transform);
            menuContent = body.GetComponent<RectTransform>();
            menuContent.anchorMin = new Vector2(0f, 1f);
            menuContent.anchorMax = new Vector2(1f, 1f);
            menuContent.pivot = new Vector2(0.5f, 1f);
            menuContent.sizeDelta = Vector2.zero;
            var column = body.AddComponent<VerticalLayoutGroup>();
            column.childAlignment = TextAnchor.UpperCenter;
            column.padding = new RectOffset(16, 16, 14, 28);
            column.spacing = 12;
            column.childControlWidth = true;
            column.childControlHeight = true;
            column.childForceExpandWidth = true;
            column.childForceExpandHeight = false;
            body.AddComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            menuScroll.content = menuContent;
            menuSheet.SetActive(false);
        }

        private void SetMenu(bool open)
        {
            menuOpen = open;
            if (open) speakingWith = -1;
            Rebuild();
            if (open && menuScroll != null) menuScroll.verticalNormalizedPosition = 1f;
        }

        // Top of the sheet: a close button, then every setting as one icon row.
        private void MenuHeader(bool offerLanguage)
        {
            Transform top = CenteredRow("Menu top", 8f);
            var spacer = Node("Menu spacer", top);
            spacer.AddComponent<LayoutElement>().flexibleWidth = 1f;
            Width(IconButton(top, "Close menu", GameLocalization.Text("close"),
                MosseloraArt.IconCancel, () => SetMenu(false), 52, MosseloraTheme.QuietAction), 60);
            SettingsRow(offerLanguage);
        }

        // Floating game chrome: a leaf menu button (left) and the memory book
        // with its count (right). Rebuilt with the page so state stays honest.
        private void BuildChrome(bool living)
        {
            if (chrome == null) return;
            Button menu = IconButton(chrome, "Open menu", GameLocalization.Text("menu"),
                MosseloraArt.IconMenu, () => SetMenu(!menuOpen), 54, MosseloraTheme.QuietAction);
            PlaceChrome(menu, new Vector2(0f, 1f), new Vector2(10f, -6f));
            if (living && world.Choices.Length > 0)
            {
                Button book = IconButton(chrome, "Memories",
                    GameLocalization.Text(showJournal ? "journal_hide" : "journal_show"),
                    MosseloraArt.IconJournal, () => {
                        showJournal = !showJournal;
                        revealNewScreen = showJournal; // opening the journal lands on its title
                        Rebuild();
                    }, 54, showJournal ? MosseloraTheme.ForestAction : MosseloraTheme.QuietAction,
                    // The memory count makes the book read as "your collection", not "rules".
                    false, world.Choices.Length.ToString(CultureInfo.InvariantCulture));
                PlaceChrome(book, new Vector2(1f, 1f), new Vector2(-10f, -6f));
            }
        }

        private static void PlaceChrome(Button control, Vector2 corner, Vector2 offset)
        {
            var rect = control.GetComponent<RectTransform>();
            rect.anchorMin = rect.anchorMax = corner;
            rect.pivot = corner;
            rect.anchoredPosition = offset;
            var fit = control.gameObject.AddComponent<ContentSizeFitter>();
            fit.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
            fit.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            var layout = control.GetComponent<LayoutElement>();
            if (layout != null) layout.minWidth = Mathf.Max(layout.minWidth, 54f);
        }

        // Start a new garden: a seedling icon first; only the confirm step
        // spells out the erase, beside an always-visible cancel.
        private IEnumerator FadeInPage()
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            if (qaInMemoryOnly) yield break; // tour screenshots capture settled screens
#endif
            if (pageFade == null) pageFade = content.gameObject.AddComponent<CanvasGroup>();
            pageFade.alpha = 0f;
            for (float t = 0f; t < 1f; t += Time.unscaledDeltaTime / 0.26f)
            {
                if (pageFade == null) yield break;
                pageFade.alpha = Mathf.SmoothStep(0f, 1f, t);
                yield return null;
            }
            if (pageFade != null) pageFade.alpha = 1f;
        }

        private void ResetControls(string resetName, string cancelName, Color tone)
        {
            Transform row = CenteredRow(resetName + " row");
            if (!confirmReset)
            {
                Width(IconButton(row, resetName, GameLocalization.Text("explore"),
                    MosseloraArt.IconNewGarden, () => { confirmReset = true; Rebuild(); },
                    58, tone), 96);
                return;
            }
            Width(IconButton(row, cancelName, GameLocalization.Text("keep"), MosseloraArt.IconCancel,
                () => { confirmReset = false; Rebuild(); }, 58, MosseloraTheme.QuietAction), 76);
            Width(IconButton(row, resetName, GameLocalization.Text("erase"), MosseloraArt.IconConfirm,
                ConfirmedReset, 58, MosseloraTheme.EarthAction, true), 200, 1f);
        }

        // A row that centres compact controls instead of stretching them edge to edge.
        private Transform CenteredRow(string name, float spacing = 12f)
        {
            var row = Node(name, content);
            var group = row.AddComponent<HorizontalLayoutGroup>();
            group.reverseArrangement = GameLocalization.IsRtl(GameLocalization.Current);
            group.childAlignment = TextAnchor.MiddleCenter;
            group.spacing = spacing;
            group.childControlWidth = true;
            group.childControlHeight = true;
            group.childForceExpandWidth = false;
            group.childForceExpandHeight = false;
            return row.transform;
        }

        private static void Width(Component control, float width, float flexible = 0f)
        {
            var layout = control.gameObject.GetComponent<LayoutElement>();
            if (layout == null) layout = control.gameObject.AddComponent<LayoutElement>();
            layout.minWidth = width;
            layout.preferredWidth = width;
            layout.flexibleWidth = flexible;
        }

        // The villagers stand in their village: portrait medallions along the
        // bottom of the painted strip, names in white over the art.
        private void NeighborsOnPainting(Transform painting, bool fromTitle = false)
        {
            var row = Node("Speak with a neighbor", painting);
            row.AddComponent<LayoutElement>().ignoreLayout = true;
            var rect = row.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0f, 0f);
            rect.anchorMax = new Vector2(1f, 0f);
            rect.pivot = new Vector2(0.5f, 0f);
            rect.sizeDelta = new Vector2(0f, 86f);
            rect.anchoredPosition = new Vector2(0f, 4f);
            var group = row.AddComponent<HorizontalLayoutGroup>();
            group.reverseArrangement = GameLocalization.IsRtl(GameLocalization.Current);
            group.childAlignment = TextAnchor.LowerCenter;
            group.spacing = 30f;
            group.childControlWidth = true;
            group.childControlHeight = true;
            group.childForceExpandWidth = false;
            group.childForceExpandHeight = false;
            for (int villager = 0; villager < 3; villager++)
                PortraitButton(row.transform, villager, true, fromTitle);
            row.transform.SetAsLastSibling();
        }

        private void NeighborConversation()
        {
            var conversation = Card(content, "A neighbor's memory", MosseloraTheme.CardFill);
            screenHeading = conversation.GetComponent<RectTransform>();
            Text speaker = Label(conversation.transform, "Neighbor name",
                LocalizedVillage.Name(speakingWith), 16, 31, Leaf, TextAnchor.MiddleCenter);
            Emphasize(speaker);
            Label(conversation.transform, "What they say",
                LocalizedVillage.Speak(world, speakingWith), 17, 0, Ink, TextAnchor.MiddleCenter);
            IconButton(conversation.transform, "Thanks", GameLocalization.Text("thanks"),
                MosseloraArt.IconConfirm, () => { speakingWith = -1; Rebuild(); }, 54,
                MosseloraTheme.ForestAction);
        }

        // Neighbor as a painted portrait medallion with their name beneath.
        private void PortraitButton(Transform parent, int villager, bool onPainting = false,
                                    bool fromTitle = false)
        {
            string label = LocalizedVillage.Name(villager);
            Sprite face = MosseloraArt.Portrait(villager);
            string name = villager == 0 ? "Mira" : villager == 1 ? "Orin" : "Fen";
            if (face == null)
            {
                Button(parent, name, label, () => OpenConversation(villager), 52);
                return;
            }
            var node = Node(name, parent);
            var stack = node.AddComponent<VerticalLayoutGroup>();
            stack.spacing = 4;
            stack.childAlignment = TextAnchor.UpperCenter;
            stack.childControlWidth = true;
            stack.childControlHeight = true;
            stack.childForceExpandWidth = false;
            stack.childForceExpandHeight = false;
            var hit = node.AddComponent<Image>();
            hit.color = new Color(1f, 1f, 1f, 0f); // whole medallion and name are the target
            var button = node.AddComponent<Button>();
            button.targetGraphic = hit;
            button.transition = Selectable.Transition.None;
            button.onClick.AddListener(() => {
                if (fromTitle) CloseTitle(); // step from the village straight into their memory
                OpenConversation(villager);
            });
            PressSquash.Add(node);
            var picture = Node("Portrait", node.transform);
            var pictureLayout = picture.AddComponent<LayoutElement>();
            float medallion = onPainting ? 58f : 76f;
            pictureLayout.minWidth = pictureLayout.preferredWidth = medallion;
            pictureLayout.minHeight = pictureLayout.preferredHeight = medallion;
            var pictureImage = picture.AddComponent<Image>();
            pictureImage.sprite = face;
            pictureImage.preserveAspect = true;
            pictureImage.raycastTarget = false;
            MosseloraArt.Lift(pictureImage, 2f, 0.18f);
            if (speakingWith == villager)
            {
                var ring = MosseloraArt.Ornament(picture.transform, "Speaking ring", MosseloraArt.Ring(16),
                    new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(medallion + 8f, medallion + 8f), Vector2.zero);
                if (ring != null) ring.color = Leaf;
            }
            var caption = Label(node.transform, "Caption", label, onPainting ? 13 : 15, 0,
                onPainting ? White : Ink, TextAnchor.MiddleCenter);
            Emphasize(caption);
            if (onPainting)
            {
                // Legible white name over any part of the painting.
                var shadow = caption.gameObject.AddComponent<Shadow>();
                shadow.effectColor = new Color(0.05f, 0.12f, 0.08f, 0.85f);
                shadow.effectDistance = new Vector2(1f, -1.5f);
                var outline = caption.gameObject.AddComponent<Outline>();
                outline.effectColor = new Color(0.05f, 0.12f, 0.08f, 0.55f);
            }
        }

        // Painted page header: generated wordmark with vine corners. The
        // localized title stays as transparent text for screen readers.
        private void GardenHeader(bool compact)
        {
            Sprite mark = MosseloraArt.Load(MosseloraArt.Wordmark);
            if (mark == null)
            {
                Text plain = Label(content, "Mosselora wordmark", GameLocalization.Text("title"),
                    15, 26, Leaf, TextAnchor.MiddleCenter);
                Emphasize(plain);
                return;
            }
            // Compact enough to sit between the floating menu and book buttons.
            float markHeight = 46f; // always clears the floating menu and book buttons
            var header = Node("Mosselora wordmark", content);
            Height(header, markHeight + 8f);
            var markImage = header.AddComponent<Image>();
            markImage.sprite = mark;
            markImage.preserveAspect = true;
            markImage.raycastTarget = false;
            Text spoken = Label(header.transform, "Title for screen readers",
                GameLocalization.Text("title"), 15, 0, new Color(0f, 0f, 0f, 0f));
            spoken.gameObject.GetComponent<LayoutElement>().ignoreLayout = true;
            Sprite vine = MosseloraArt.Load(MosseloraArt.VineCorner);
            float vineSize = compact ? 100f : 124f;
            MosseloraArt.Ornament(header.transform, "Vine left", vine, new Vector2(0f, 1f),
                new Vector2(0f, 1f), new Vector2(vineSize, vineSize), new Vector2(-17f, 15f));
            MosseloraArt.Ornament(header.transform, "Vine right", vine, new Vector2(1f, 1f),
                new Vector2(0f, 1f), new Vector2(vineSize, vineSize), new Vector2(17f, 15f), true);
        }

        // Centered sage pill, e.g. "STREAM 1 / 12 · First Bend".
        private Text Pill(string name, string value, Color fill)
        {
            var row = Node(name + " row", content);
            var rowGroup = row.AddComponent<HorizontalLayoutGroup>();
            rowGroup.childAlignment = TextAnchor.MiddleCenter;
            rowGroup.childControlWidth = true;
            rowGroup.childControlHeight = true;
            rowGroup.childForceExpandWidth = false;
            rowGroup.childForceExpandHeight = false;
            var pill = Node(name, row.transform);
            var pillImage = pill.AddComponent<Image>();
            MosseloraArt.Round(pillImage, fill, 15);
            pillImage.raycastTarget = false;
            var pillGroup = pill.AddComponent<HorizontalLayoutGroup>();
            pillGroup.padding = new RectOffset(18, 18, 5, 5);
            pillGroup.childAlignment = TextAnchor.MiddleCenter;
            pillGroup.childControlWidth = true;
            pillGroup.childControlHeight = true;
            pillGroup.childForceExpandWidth = false;
            Text label = Label(pill.transform, name + " text", value, 14, 0, Ink, TextAnchor.MiddleCenter);
            Emphasize(label);
            return label;
        }

        // Where you are along the creek: twelve stepping stones (passed, here,
        // ahead) with the stream's name. "Stream n / 12" stays as spoken text.
        private void StreamStones(Puzzle puzzle)
        {
            StreamStones(puzzle, content, true);
        }

        private void StreamStones(Puzzle puzzle, Transform parent, bool named)
        {
            var row = Node(named ? "Chapter" : "Walk so far", parent);
            var group = row.AddComponent<VerticalLayoutGroup>();
            group.childAlignment = TextAnchor.MiddleCenter;
            group.spacing = 4;
            group.childControlWidth = true;
            group.childControlHeight = true;
            group.childForceExpandWidth = true;
            group.childForceExpandHeight = false;
            var stones = Node("Stepping stones", row.transform);
            var stoneRow = stones.AddComponent<HorizontalLayoutGroup>();
            stoneRow.reverseArrangement = GameLocalization.IsRtl(GameLocalization.Current);
            stoneRow.childAlignment = TextAnchor.MiddleCenter;
            stoneRow.spacing = 6;
            stoneRow.childControlWidth = true;
            stoneRow.childControlHeight = true;
            stoneRow.childForceExpandWidth = false;
            stoneRow.childForceExpandHeight = false;
            for (int i = 0; i < PuzzleCatalog.Count; i++)
            {
                bool here = i == world.Round;
                bool passed = i < world.Round;
                var stone = Node(passed ? "Stone passed" : here ? "Stone here" : "Stone ahead", stones.transform);
                var layout = stone.AddComponent<LayoutElement>();
                float size = here ? 18f : 11f;
                layout.minWidth = layout.preferredWidth = size;
                layout.minHeight = layout.preferredHeight = size;
                var image = stone.AddComponent<Image>();
                image.sprite = here ? MosseloraArt.BoldRing() : MosseloraArt.Rounded(4);
                image.type = Image.Type.Simple;
                image.color = here ? Leaf : passed ? MosseloraTheme.WaterChannel
                    : new Color(0.55f, 0.50f, 0.42f, 0.35f);
                image.raycastTarget = false;
            }
            if (!named) return;
            string name = GameLocalization.Current == "en" ? puzzle.Name : "";
            Text title = Label(row.transform, "Stream name", name.Length > 0 ? name : " ", 14, 0, Moss,
                TextAnchor.MiddleCenter);
            Emphasize(title);
            // Spoken position along the creek, not shown: the stones show it.
            Text spoken = Label(row.transform, "Stream position",
                GameLocalization.Text("stream", world.Round + 1, PuzzleCatalog.Count),
                12, 0, new Color(0f, 0f, 0f, 0f), TextAnchor.MiddleCenter);
            spoken.GetComponent<LayoutElement>().ignoreLayout = true;
        }

        // Thin leaf sprig between sections; purely decorative.
        private void Sprig(float height = 22f)
        {
            Sprite sprig = MosseloraArt.Load(MosseloraArt.Sprig);
            if (sprig == null) return;
            var node = Node("Leaf divider", content);
            Height(node, height);
            var image = node.AddComponent<Image>();
            image.sprite = sprig;
            image.preserveAspect = true;
            image.raycastTarget = false;
            image.color = new Color(1f, 1f, 1f, 0.9f);
        }

        // Full-bleed painting with its own aspect; crops nothing.
        private void Painting(string name, string art, float maxHeight)
        {
            Sprite sprite = MosseloraArt.Load(art);
            if (sprite == null) return;
            float width = PageWidth() + 34f;
            float height = Mathf.Min(maxHeight, width / MosseloraArt.Aspect(sprite));
            var node = Node(name, content);
            Height(node, height);
            var frame = Node(name + " frame", node.transform);
            var rect = frame.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(1f, 1f);
            rect.pivot = new Vector2(0.5f, 1f);
            rect.offsetMin = new Vector2(-17f, 0f);
            rect.offsetMax = new Vector2(17f, 0f);
            rect.sizeDelta = new Vector2(34f, width / MosseloraArt.Aspect(sprite));
            node.AddComponent<RectMask2D>();
            var image = frame.AddComponent<Image>();
            image.sprite = sprite;
            image.raycastTarget = false;
        }

        // Logical width inside the page margins, from the real safe area.
        private float PageWidth()
        {
            // CanvasScaler.scaleFactor is only the constant-pixel setting; the live
            // ScaleWithScreenSize factor is on the Canvas. Prefer the laid-out page.
            // On the very first build after launch neither the layout nor
            // Canvas.scaleFactor is settled yet (the painted strip came out ~3x
            // too tall on a cold start), so derive the scale from the scaler's
            // own ScaleWithScreenSize formula, which is valid immediately.
            if (content != null && content.rect.width > 1f && Time.frameCount > 2)
                return Mathf.Max(225f, content.rect.width - 34f);
            return Mathf.Max(225f, Screen.safeArea.width / LiveCanvasScale() - 34f);
        }

        private float LiveCanvasScale()
        {
            if (viewportScaler == null || Screen.width <= 0 || Screen.height <= 0) return 1f;
            Vector2 reference = viewportScaler.referenceResolution;
            float logWidth = Mathf.Log(Screen.width / reference.x, 2f);
            float logHeight = Mathf.Log(Screen.height / reference.y, 2f);
            return Mathf.Max(0.1f, Mathf.Pow(2f, Mathf.Lerp(logWidth, logHeight,
                viewportScaler.matchWidthOrHeight)));
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
            rebuildGeneration++;
            if (content == null) { celebrateSolve = false; return; }
            semanticsDirty = true;
            UpdateSaveWarning();
            float previousPosition = scroll.verticalNormalizedPosition;
            visibleTiles = null;
            coachBubble = null;
            activeIllustration = null;
            activePainting = null;
            tileBackgrounds = null;
            tileArtwork = null;
            visibleLeaks = null;
            feedbackText = null;
            waterDrops = null;
            undoButton = null;
            decisionHeading = null;
            screenHeading = null;
            for (int i = content.childCount - 1; i >= 0; i--)
            {
                GameObject old = content.GetChild(i).gameObject;
                old.SetActive(false); // Destroy() is deferred until the frame ends.
                Destroy(old);
            }
            ClearChildren(menuContent);
            ClearChildren(chrome);

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
            GardenHeader(puzzleScreen);
            // After the first stream the board, tools and painting share one
            // screen; the tagline returns on story screens (memory, ending).
            if (!puzzleScreen || world.Round == 0)
            {
                Text subtitle = Label(content, "Title", GameLocalization.Text("subtitle"),
                    puzzleScreen ? 17 : 24, puzzleScreen ? 28 : 44, Ink, TextAnchor.MiddleCenter);
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
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                var illustration = Node(showMiniature ? "Miniature diorama" : "Living storybook art", content);
#else
                var illustration = Node("Living storybook art", content);
#endif
                // Art never takes the first playable fold: it follows the board and controls.
                Sprite bannerArt = MosseloraArt.Load(MosseloraArt.BannerFlowing);
                float bannerWidth = PageWidth() + 34f;
                Height(illustration, bannerArt != null
                    ? Mathf.Round(bannerWidth / MosseloraArt.Aspect(bannerArt) * (memoryScreen ? 1.15f : 1f))
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
                if (!renderedMiniature)
                {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                    if (diorama != null) diorama.Hide();
#endif
                    var painting = illustration.AddComponent<PaintedVillage>();
                    painting.Build(17f);
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
                if (!showingMemory) NeighborsOnPainting(illustration.transform);
            }
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            else if (diorama != null)
                diorama.Hide();
#endif

            if (living && !showingMemory && speakingWith >= 0) NeighborConversation();
            if (living)
            {
                // Said once: the solved board already says the water flows, and
                // the memory card already tells who remembers what.
                bool solvedBoard = puzzleScreen && world.Solved;
                if (!firstPuzzle && !memoryScreen && !solvedBoard)
                    Label(content, "World consequence", LocalizedVillage.Consequence(world), 16, 52, Ink,
                        TextAnchor.MiddleCenter);
                if (world.Choices.Length > 0 && !memoryScreen)
                    Label(content, "Remembered choice", LocalizedVillage.Memory(world), 13, 38, Moss,
                        TextAnchor.MiddleCenter);
            }
            else
                Label(content, "Puzzle condition", GameLocalization.Text("puzzle_desc"),
                    14, 44, Moss, TextAnchor.MiddleCenter);

            if (memoryScreen)
            {
                var moment = Card(content, "A remembered moment", MosseloraTheme.CardFill);
                // Room above the title for the leaf sprigs at any text size.
                moment.GetComponent<VerticalLayoutGroup>().padding = new RectOffset(14, 14, 34, 16);
                // The neighbor who will remember this choice, painted, at the top of the card.
                int[] madeChoices = world.Choices;
                Sprite rememberer = MosseloraArt.Portrait(madeChoices[madeChoices.Length - 1]);
                if (rememberer != null)
                {
                    var face = Node("Neighbor who remembers", moment.transform);
                    Height(face, 72f);
                    var faceImage = face.AddComponent<Image>();
                    faceImage.sprite = rememberer;
                    faceImage.preserveAspect = true;
                    faceImage.raycastTarget = false;
                }
                Text momentTitle = Label(moment.transform, "Moment title",
                    GameLocalization.Text("memory_title"), 15, 31, Leaf, TextAnchor.MiddleCenter);
                Emphasize(momentTitle);
                Label(moment.transform, "Story", LocalizedVillage.MemoryEvent(world), 17, 0, Ink,
                    TextAnchor.MiddleCenter);
                Sprite memorySprig = MosseloraArt.Load(MosseloraArt.Sprig);
                MosseloraArt.Ornament(moment.transform, "Sprig left", memorySprig, new Vector2(0f, 1f),
                    new Vector2(0f, 1f), new Vector2(64f, 22f), new Vector2(10f, -8f));
                MosseloraArt.Ornament(moment.transform, "Sprig right", memorySprig, new Vector2(1f, 1f),
                    new Vector2(0f, 1f), new Vector2(64f, 22f), new Vector2(-10f, -8f), true);
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
                }, 64, MosseloraTheme.QuietAction);
                Width(changeButton, 76);
                Button continueButton = IconButton(next, "Continue", GameLocalization.Text("continue"),
                    MosseloraArt.IconContinue, () => {
                        showingMemory = false;
                        revealNewScreen = true; // the ending, if reached, opens at its heading
                        Rebuild(true);
                    }, 64, MosseloraTheme.WaterAction);
                Width(continueButton, 160, 1f);
            }
            else if (world.Completed)
            {
                // The whole village gathers by the restored creek (falls back to the
                // everyday village painting if the festival art is missing).
                Painting("Village memory painting",
                    MosseloraArt.Load(MosseloraArt.EndingFestival) != null
                        ? MosseloraArt.EndingFestival : MosseloraArt.HeroVillage, 460f);
                var conclusion = Card(content, "The ending", MosseloraTheme.CardFill);
                screenHeading = conclusion.GetComponent<RectTransform>();
                Text endingTitle = Label(conclusion.transform, "Ending title",
                    GameLocalization.Text("ending_title"), 15, 31, Leaf, TextAnchor.MiddleCenter);
                Emphasize(endingTitle);
                if (living) ChoiceTimeline(conclusion.transform);
                Text finalWords = Label(conclusion.transform, "Final words", LocalizedVillage.Ending(world),
                    19, 0, Ink, TextAnchor.MiddleCenter);
                Emphasize(finalWords);
                if (living) MemoryJournal();
                ResetControls("Reset title", "Cancel reset", MosseloraTheme.WaterAction);
            }
            if (living && !world.Completed && !showingMemory && world.Choices.Length > 0)
                MemoryJournal();

            // ---- Menu sheet: settings, files, new garden, research tools, promise.
            RectTransform page = content;
            content = menuContent;
            MenuHeader(multiLanguage);
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
                            PlayerPrefs.SetInt(ReducedMotionKey, animateMiniature ? 1 : 0);
                            PlayerPrefs.Save();
                            Rebuild();
                        }, 45);
            }

#endif
            BuildGardenFileSection();
            if (!world.Completed && world.Round > 0 && !WorldStore.RecoveryBlocked)
            {
                ResetControls("Restart garden", "Cancel", MosseloraTheme.QuietAction);
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
            Sprig();
            Text promise = Label(content, "Respectful play", GameLocalization.Text("offline"),
                13, 36, Ink, TextAnchor.MiddleCenter);
            Emphasize(promise);
            content = page;
            BuildChrome(living);
            UpdateCreekSound();
            // A different screen (not a tile turn) crossfades in like a turned page.
            string screenKey = (memoryScreen ? "memory" : world.Completed ? "ending" : "puzzle") + ":" +
                world.Round + ":" + world.Solved + ":" + showJournal + ":" + speakingWith + ":" + languagePicker;
            if (screenKey != lastScreenKey && lastScreenKey.Length > 0 && gentleMotion) StartCoroutine(FadeInPage());
            lastScreenKey = screenKey;
            menuSheet.SetActive(menuOpen);
            Canvas.ForceUpdateCanvases();
            LayoutRebuilder.ForceRebuildLayoutImmediate(menuContent);
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
            // The toggle is the book in the floating chrome (BuildChrome).
            if (!showJournal) return;
            var journal = Card(content, "A living journal", MosseloraTheme.CardFill);
            screenHeading = journal.GetComponent<RectTransform>();
            Text journalTitle = Label(journal.transform, "Journal title",
                GameLocalization.Text("journal_title"), 15, 30, Leaf, TextAnchor.MiddleCenter);
            Emphasize(journalTitle);
            // A painted scrapbook: two pages to a row, each with the painting of
            // the choice, the neighbor who remembers it, and their written line.
            string[] entries = LocalizedVillage.Journal(world);
            int[] chosen = world.Choices;
            GameObject spread = null;
            for (int i = 0; i < entries.Length; i++)
            {
                if (i % 2 == 0)
                {
                    spread = Row(journal.transform, "Scrapbook spread");
                    var spreadGroup = spread.GetComponent<HorizontalLayoutGroup>();
                    spreadGroup.childForceExpandHeight = true;
                    spreadGroup.childAlignment = TextAnchor.UpperCenter;
                }
                var page = Node("Journal chapter", spread.transform);
                var pageImage = page.AddComponent<Image>();
                MosseloraArt.PaperTile(pageImage, 14);
                pageImage.color = MosseloraTheme.DryTile;
                pageImage.raycastTarget = false;
                MosseloraArt.Lift(pageImage, 2f, 0.12f);
                var pageStack = page.AddComponent<VerticalLayoutGroup>();
                pageStack.padding = new RectOffset(8, 8, 10, 10);
                pageStack.spacing = 4;
                pageStack.childAlignment = TextAnchor.UpperCenter;
                pageStack.childControlWidth = true;
                pageStack.childControlHeight = true;
                pageStack.childForceExpandWidth = true;
                pageStack.childForceExpandHeight = false;
                Sprite icon = i < chosen.Length ? MosseloraArt.ChoiceIcon((VillageChoice)chosen[i]) : null;
                if (icon != null)
                {
                    var picture = Node("Journal picture", page.transform);
                    Height(picture, 64f);
                    var pictureImage = picture.AddComponent<Image>();
                    pictureImage.sprite = icon;
                    pictureImage.preserveAspect = true;
                    pictureImage.raycastTarget = false;
                    Sprite face = MosseloraArt.Portrait(chosen[i]);
                    if (face != null)
                        MosseloraArt.Ornament(picture.transform, "Who remembers", face, new Vector2(1f, 0f),
                            new Vector2(1f, 0f), new Vector2(30f, 30f), new Vector2(0f, -4f));
                }
                Label(page.transform, "Recorded cause and effect", entries[i], 13, 0, Ink, TextAnchor.UpperCenter);
            }
            if (entries.Length % 2 == 1 && spread != null)
                Node("Scrapbook blank", spread.transform); // keeps the last page half width
            if (world.Journal.Length == 0)
                Label(journal.transform, "Blank page", GameLocalization.Text("journal_empty"), 14, 0, Moss);
        }

        // Upper bound for Large tiles so a tablet-width board stays reachable.
        private const int LargeTileCap = 120;

        private void BuildPuzzle()
        {
            Puzzle puzzle = world.CurrentPuzzle;
            PipeInsight insight = world.Insight;
            StreamStones(puzzle);
            if (world.Round == 0)
            {
                // A speech bubble pointing at the board, like a game's first-run
                // tip; it leaves after the first move (RefreshBoard).
                if (!world.Solved && !world.CanUndo) CoachBubble(GameLocalization.Text("first"));
            }
            else
                Label(content, "Prompt", LocalizedVillage.Prompt(world), 16, 0, Ink,
                    TextAnchor.MiddleCenter);
            // The anchor line sits directly above the board: it explains the framed tile.
            if (puzzle.AnchoredTile >= 0)
                Label(content, "Protected habitat", LocalizedVillage.EnvironmentalRule(world),
                    14, 48, Leaf, TextAnchor.MiddleCenter);
            // The board sits on the page itself; each tile is its own parchment square.
            var board = Card(content, "The water puzzle", new Color(1f, 1f, 1f, 0f));
            var gridNode = Node("Water connections", board.transform);
            // Keep the entire puzzle inside a compact phone, including the
            // story card's horizontal padding and safe-area insets.
            int gap = puzzle.Size == 3 ? 7 : 5;
            // CanvasScaler.scaleFactor is only the constant-pixel field (always 1
            // here); the live ScaleWithScreenSize factor gives true logical width.
            float layoutWidth = Screen.safeArea.width / LiveCanvasScale();
            // Standard keeps the EG-007 sizing exactly. Large (EG-112) trims the
            // card's side padding and lifts the tile cap so the board uses the width.
            float available = Mathf.Max(225f, layoutWidth - 34f - 20f - 5f);
            int standardTile = Mathf.Clamp(
                Mathf.FloorToInt((available - (puzzle.Size - 1) * gap) / puzzle.Size),
                48, puzzle.Size == 3 ? 90 : 67);
            int tileSize = standardTile;
            if (BoardSize.IsLarge)
            {
                var boardPadding = board.GetComponent<VerticalLayoutGroup>();
                boardPadding.padding = new RectOffset(6, 6, 14, 14);
                float largeAvailable = Mathf.Max(225f, layoutWidth - 34f - 12f - 2f);
                tileSize = Mathf.Clamp(
                    Mathf.FloorToInt((largeAvailable - (puzzle.Size - 1) * gap) / puzzle.Size),
                    48, LargeTileCap);
            }
            // Markers, badges and endpoint icons scale with the tile; Standard is 1.
            float markerScale = tileSize / (float)standardTile;
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
            RectTransform gardenTile = null;
            for (int index = 0; index < puzzle.CellCount; index++)
            {
                int tappedIndex = index;
                var tileNode = Node("Water tile", gridNode.transform);
                var image = tileNode.AddComponent<Image>();
                bool reached = insight.Flowing[index];
                MosseloraArt.PaperTile(image, 12);
                image.color = MosseloraTheme.TileFill(world.Solved, reached,
                    puzzle.IsAnchored(index), world.HintStep, world.HintTile,
                    index, puzzle.Size);
                MosseloraArt.Lift(image, 2f, 0.14f);
                MosseloraArt.Edge(tileNode.transform, MosseloraTheme.TileEdge, 12);
                var tile = tileNode.AddComponent<Button>();
                tile.targetGraphic = image;
                // Protected tiles stay tappable: a tap explains them without a move.
                tile.interactable = !world.Solved;
                ColorBlock colors = PressTint();
                // A solved board is a finished picture, not a greyed-out control.
                colors.disabledColor = Color.white;
                tile.colors = colors;
                tile.onClick.AddListener(() => TapTile(tappedIndex));
                if (!world.Solved) PressSquash.Add(tileNode);
                else if (celebrateSolve)
                {
                    // The finished creek blooms outward from the spring, tile by tile.
                    int spring = Array.IndexOf(puzzle.Kinds, PipeKind.End);
                    int steps = spring < 0 ? 0 :
                        Mathf.Abs(index / puzzle.Size - spring / puzzle.Size) +
                        Mathf.Abs(index % puzzle.Size - spring % puzzle.Size);
                    if (gentleMotion) BloomPulse.Add(tileNode, 0.08f + steps * 0.07f);
                }
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
                    insight.Flowing[index], world.Solved, OpenPorts(puzzle, rotations, index));
                pipe.raycastTarget = false;
                tileArtwork[index] = pipe;
                // A hollow circle denotes an open water join. Keep the
                // non-color cue; avoid alarming red error punctuation.
                var leakNode = Node("Unmatched water port marker", tileNode.transform);
                RectTransform leakRect = leakNode.GetComponent<RectTransform>();
                leakRect.anchorMin = Vector2.zero;
                leakRect.anchorMax = Vector2.zero;
                leakRect.pivot = Vector2.zero;
                leakRect.sizeDelta = new Vector2(23f, 23f) * markerScale;
                leakRect.anchoredPosition = new Vector2(3f, 3f) * markerScale;
                var leakText = leakNode.AddComponent<Text>();
                leakText.font = font;
                leakText.fontSize = Mathf.RoundToInt(23 * markerScale);
                leakText.fontStyle = FontStyle.Bold;
                leakText.alignment = TextAnchor.MiddleCenter;
                leakText.color = MosseloraTheme.EarthInk;
                leakText.raycastTarget = false;
                leakText.text = insight.Leaking[index] ? "○" : "";
                // The visible mark is now the open mouth drawn on the pipe end;
                // this "○" stays as the semantic source, invisible.
                leakText.color = new Color(0f, 0f, 0f, 0f);
                visibleLeaks[index] = leakText;
                if (world.HintStep == 2 && index == world.HintTile)
                {
                    // A soft honey glow around the tile to turn: it breathes when
                    // gentle motion is on, and holds still when it is off.
                    var clueNode = Node("Hint focus marker", tileNode.transform);
                    clueNode.AddComponent<LayoutElement>().ignoreLayout = true;
                    RectTransform clueRect = clueNode.GetComponent<RectTransform>();
                    Fill(clueRect);
                    clueRect.offsetMin = new Vector2(-5f, -5f);
                    clueRect.offsetMax = new Vector2(5f, 5f);
                    var glow = clueNode.AddComponent<Image>();
                    glow.sprite = MosseloraArt.Ring(15);
                    glow.type = Image.Type.Sliced;
                    glow.pixelsPerUnitMultiplier = 1.2f;
                    glow.color = new Color(0.89f, 0.63f, 0.18f, 1f);
                    glow.raycastTarget = false;
                    if (gentleMotion) Breathe.Add(glow);
                }
                if (puzzle.Kinds[index] == PipeKind.End)
                {
                    // Visible endpoint semantics instead of relying only on
                    // screen-reader names or a color difference.
                    bool spring = seenEnds == 1;
                    if (!spring) gardenTile = tileNode.GetComponent<RectTransform>();
                    var markerNode = Node(spring ? "Water spring waves" : "Garden sprout",
                        tileNode.transform);
                    RectTransform markerRect = markerNode.GetComponent<RectTransform>();
                    markerRect.anchorMin = new Vector2(0f, 1f);
                    markerRect.anchorMax = new Vector2(0f, 1f);
                    markerRect.pivot = new Vector2(0f, 1f);
                    markerRect.sizeDelta = new Vector2(30f, 30f) * markerScale;
                    markerRect.anchoredPosition = new Vector2(4f, -4f) * markerScale;
                    Sprite glyph = MosseloraArt.Load(spring ? MosseloraArt.SpringWaves : MosseloraArt.Sprout);
                    if (glyph != null)
                    {
                        var glyphImage = markerNode.AddComponent<Image>();
                        glyphImage.sprite = glyph;
                        glyphImage.preserveAspect = true;
                        glyphImage.raycastTarget = false;
                    }
                    else
                        DrawEndpointIcon(markerNode.transform, spring, markerScale);
                }
            }

            if (celebrateSolve && gentleMotion) ButterflyFlight.Launch(gardenTile, board.transform);
            celebrateSolve = false;
            string feedback = world.Solved
                ? GameLocalization.Text("solved")
                : GameLocalization.Text("progress", insight.ConnectedCount, puzzle.CellCount, insight.LeakCount);
            WaterMeter(puzzle.CellCount, world.Solved ? puzzle.CellCount : insight.ConnectedCount);
            feedbackText = Label(content, "Water feedback", feedback, 17, 34, Ink, TextAnchor.MiddleCenter);
            Emphasize(feedbackText);

            if (world.Solved)
            {
                // Both puzzle-only and matched runs have no village decisions;
                // only the Living world may show choices.
                if (world.Mode != PlayMode.Living)
                    IconButton(content, "Next stream", GameLocalization.Text("next"),
                        MosseloraArt.IconContinue, () => {
                            if (!world.NextPuzzle()) return;
                            SaveWorld();
                            Rebuild(true);
                        }, 64, MosseloraTheme.ForestAction);
                else
                {
                    Text chooseTitle = Label(content, "Choose title", GameLocalization.Text("choose"),
                        24, 56, Ink, TextAnchor.MiddleCenter);
                    Emphasize(chooseTitle);
                    decisionHeading = chooseTitle.rectTransform;
                    // Three tall painted cards side by side, like cards on a table.
                    GameObject choiceRow = Row(content, "Village choices");
                    var choiceGroup = choiceRow.GetComponent<HorizontalLayoutGroup>();
                    choiceGroup.childForceExpandHeight = true;
                    choiceGroup.childAlignment = TextAnchor.UpperCenter;
                    for (int choiceIndex = 0; choiceIndex < 3; choiceIndex++)
                    {
                        VillageChoice choice = (VillageChoice)choiceIndex;
                        // None is visually preselected. A golden default would
                        // bias research toward the first habitat, not player care.
                        ChoiceCard(choiceRow.transform, choice, () => MakeChoice(choice));
                    }
                }
                // Second thoughts live after the decision, not between the payoff
                // line and the choices, where a lone back-arrow read as "go back".
                Button revisit = IconButton(CenteredRow("Revisit row"), "Revisit water",
                    GameLocalization.Text("undo_last"), MosseloraArt.IconUndo,
                    () => Act(() => world.Undo(), "undo"), 48, MosseloraTheme.QuietAction);
                Width(revisit, 84);
                revisit.interactable = world.CanUndo;
            }
            else
            {
                var tools = Row(content, "Gentle controls");
                // Progressive disclosure: the first stream teaches the three tools
                // with small captions; afterwards they are pictures only.
                bool teach = world.Round == 0;
                int toolHeight = teach ? 72 : 64;
                undoButton = IconButton(tools.transform, "Undo", GameLocalization.Text("undo"),
                    MosseloraArt.IconUndo, () => Act(() => world.Undo(), "undo"),
                    toolHeight, MosseloraTheme.ForestAction, false, null, teach);
                undoButton.interactable = world.CanUndo;
                // Hint depth shows as a small 1/2 or 2/2 badge, not words.
                IconButton(tools.transform, "Hint",
                    GameLocalization.Text(world.HintStep == 2 ? "hint_turn" :
                        world.HintStep == 1 ? "hint_more" : "hint"),
                    MosseloraArt.IconHint, UseGentleHint, toolHeight, MosseloraTheme.WaterAction, false,
                    null, teach, world.HintStep > 0 ? 2 : 0, world.HintStep);
                IconButton(tools.transform, "Restart", GameLocalization.Text("restart"),
                    MosseloraArt.IconRestart, () => Act(() => world.Restart(), "restart"),
                    toolHeight, MosseloraTheme.EarthAction, false, null, teach);
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
            // The coach bubble teaches; every tile's spoken hint still carries "how".
        }

        private void CoachBubble(string message)
        {
            var row = Node("Prompt", content);
            var rowGroup = row.AddComponent<HorizontalLayoutGroup>();
            rowGroup.childAlignment = TextAnchor.MiddleCenter;
            rowGroup.padding = new RectOffset(10, 10, 0, 12); // room for the tail
            rowGroup.childControlWidth = true;
            rowGroup.childControlHeight = true;
            rowGroup.childForceExpandWidth = true;
            rowGroup.childForceExpandHeight = false;
            var bubble = Node("Coach bubble", row.transform);
            var fill = bubble.AddComponent<Image>();
            MosseloraArt.Round(fill, MosseloraTheme.ForestAction, 18);
            fill.raycastTarget = false;
            MosseloraArt.Lift(fill, 3f, 0.22f);
            var pad = bubble.AddComponent<VerticalLayoutGroup>();
            pad.padding = new RectOffset(16, 16, 10, 10);
            pad.childControlWidth = true;
            pad.childControlHeight = true;
            pad.childForceExpandWidth = true;
            Text words = Label(bubble.transform, "Coach words", message, 16, 0, White, TextAnchor.MiddleCenter);
            Emphasize(words);
            var tail = Node("Coach tail", bubble.transform);
            tail.AddComponent<LayoutElement>().ignoreLayout = true;
            var tailRect = tail.GetComponent<RectTransform>();
            tailRect.anchorMin = tailRect.anchorMax = new Vector2(0.5f, 0f);
            tailRect.sizeDelta = new Vector2(16f, 16f);
            tailRect.anchoredPosition = new Vector2(0f, -2f);
            tailRect.localRotation = Quaternion.Euler(0f, 0f, 45f);
            var tailImage = tail.AddComponent<Image>();
            tailImage.color = MosseloraTheme.ForestAction;
            tailImage.raycastTarget = false;
            tail.transform.SetAsFirstSibling(); // under the bubble body
            coachBubble = row;
        }

        // Illustrated choice card (painted icon + title + written consequence).
        // One Button per card, so taps, focus and screen-reader text are unchanged.
        private void ChoiceCard(Transform parent, VillageChoice choice, Action action)
        {
            var node = Node("Choice " + choice, parent);
            MinHeight(node, TextScale.Scale(190));
            var image = node.AddComponent<Image>();
            MosseloraArt.PaperTile(image, 18);
            image.color = MosseloraTheme.CardFill;
            MosseloraArt.Lift(image, 3f, 0.16f);
            MosseloraArt.Edge(node.transform, MosseloraTheme.CardEdge, 18);
            var button = node.AddComponent<Button>();
            button.targetGraphic = image;
            button.colors = PressTint();
            button.onClick.AddListener(() => action());
            PressSquash.Add(node);
            var stack = node.AddComponent<VerticalLayoutGroup>();
            stack.padding = new RectOffset(8, 8, 12, 12);
            stack.spacing = 6;
            stack.childAlignment = TextAnchor.UpperCenter;
            stack.childControlWidth = true;
            stack.childControlHeight = true;
            stack.childForceExpandWidth = true;
            stack.childForceExpandHeight = false;
            Sprite icon = MosseloraArt.ChoiceIcon(choice);
            if (icon != null)
            {
                var picture = Node("Choice picture", node.transform);
                var pictureLayout = picture.AddComponent<LayoutElement>();
                pictureLayout.minHeight = pictureLayout.preferredHeight = 84;
                var pictureImage = picture.AddComponent<Image>();
                pictureImage.sprite = icon;
                pictureImage.preserveAspect = true;
                pictureImage.raycastTarget = false;
            }
            Text title = Label(node.transform, "Choice name", LocalizedVillage.Choice(choice), 16, 0, Ink,
                TextAnchor.UpperCenter);
            Emphasize(title);
            Label(node.transform, "Choice consequence", LocalizedVillage.ChoiceOutcome(choice, world), 13, 0, Moss,
                TextAnchor.UpperCenter);
        }

        // Compact settings row below the primary content of puzzle and post-choice
        // screens. Same controls and behavior as the top-of-screen settings.
        private void SettingsRow(bool offerLanguage)
        {
            // The settings page is where words belong: each tile is its icon,
            // its live state and its name, two to a row.
            var panel = Card(content, "Settings panel", MosseloraTheme.SagePanel);
            panel.GetComponent<VerticalLayoutGroup>().padding = new RectOffset(10, 10, 10, 10);
            GameObject row = Row(panel.transform, "Settings row");
            if (offerLanguage)
                IconButton(row.transform, "Language settings",
                    GameLocalization.Text("language") + ": " +
                    GameLocalization.NativeName(GameLocalization.Current),
                    MosseloraArt.IconLanguage, () => { languagePicker = true; Rebuild(true); }, 78,
                    MosseloraTheme.QuietAction, false, null, true);
            TextSizeButton(row.transform);
            row = Row(panel.transform, "Settings row 2");
            BoardSizeButton(row.transform);
            IconButton(row.transform, "Gentle motion setting",
                GameLocalization.Text("gentle_motion"),
                gentleMotion ? MosseloraArt.IconMotionOn : MosseloraArt.IconMotionOff,
                () => {
                    gentleMotion = !gentleMotion;
                    PipeGraphic.Animate = gentleMotion;
                    PlayerPrefs.SetInt(GentleMotionKey, gentleMotion ? 1 : 0);
                    PlayerPrefs.Save();
                    Rebuild();
                }, 78, gentleMotion ? MosseloraTheme.ForestAction : MosseloraTheme.QuietAction,
                false, null, true);
            row = Row(panel.transform, "Settings row 3");
            IconButton(row.transform, "Quiet audio setting",
                GameLocalization.Text(soundOn ? "sound_on" : "sound_off"),
                soundOn ? MosseloraArt.IconChimeOn : MosseloraArt.IconChimeOff,
                () => {
                    soundOn = !soundOn;
                    PlayerPrefs.SetInt("evergrove:gentle-audio", soundOn ? 1 : 0);
                    PlayerPrefs.Save();
                    Rebuild();
                }, 78, soundOn ? MosseloraTheme.ForestAction : MosseloraTheme.QuietAction,
                false, null, true);
        }

        // Language-neutral label: the tile scale of the current board (EG-112).
        // Standard is 1x by definition. Large measures about 1.2x for a 3x3
        // board on a 390-point phone (109 px vs 90 px tile cap); 4x4 is closer to 1.03x.
        private void BoardSizeButton(Transform parent)
        {
            IconButton(parent, "Board size",
                GameLocalization.Text("board_size") + (BoardSize.IsLarge ? " 1.2×" : " 1×"),
                MosseloraArt.IconBoardSize, () => {
                    BoardSize.Toggle();
                    Rebuild();
                }, 78, BoardSize.IsLarge ? MosseloraTheme.ForestAction : MosseloraTheme.QuietAction,
                false, null, true, 2, BoardSize.IsLarge ? 2 : 1);
        }

        // Language-neutral label: "Aa" plus the current size. Cycles 100/125/150%.
        private void TextSizeButton(Transform parent)
        {
            IconButton(parent, "Text size",
                GameLocalization.Text("text_size") + " " + TextScale.Percent + "%",
                MosseloraArt.IconTextSize, () => {
                    TextScale.Cycle();
                    Rebuild();
                }, 78, TextScale.Percent > 100 ? MosseloraTheme.ForestAction : MosseloraTheme.QuietAction,
                false, null, true, 3, TextScale.Percent >= 150 ? 3 : TextScale.Percent >= 125 ? 2 : 1);
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
            IconButton(content, "Close language picker", GameLocalization.Text("continue"),
                MosseloraArt.IconConfirm, () => {
                    languagePicker = false;
                    Rebuild(true);
                }, 60, MosseloraTheme.ForestAction);
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
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            if (localPlaytest != null) localPlaytest.Talk(Time.unscaledTime);
#endif
            revealNewScreen = true; // land on the neighbor's card, wherever it sits
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
            semanticsDirty = true;
            UpdateCreekSound();
            // The first move is the lesson learned: the bubble steps aside.
            if (coachBubble != null && world.CanUndo) { coachBubble.SetActive(false); coachBubble = null; }
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
                    insight.Flowing[i], world.Solved, OpenPorts(puzzle, angles, i));
                if (visibleLeaks != null && visibleLeaks[i] != null)
                    visibleLeaks[i].text = insight.Leaking[i] ? "○" : "";
            }
            PaintDrops(world.Solved ? puzzle.CellCount : insight.ConnectedCount);
            if (feedbackText != null)
                CjkLineBreakFixer.SetText(feedbackText, GameLocalization.Text("progress",
                    insight.ConnectedCount, puzzle.CellCount, insight.LeakCount));
            if (undoButton != null) undoButton.interactable = world.CanUndo;
            if (activeIllustration != null || activePainting != null)
            {
                // The world responds to meaningful changes in reachable water
                // without rebuilding the puzzle or moving the scroll position.
                VillageScenePlan plan = VillageScenePlan.From(world);
                if (activeIllustration != null)
                    activeIllustration.Show(plan.Chapter, plan.TreeCount,
                        plan.CrossingCount, plan.FlowerCount,
                        plan.WaterRestored, plan.WaterReachPercent);
                if (activePainting != null)
                    activePainting.Show(plan.Chapter, plan.TreeCount,
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
                celebrateSolve = !wasSolved && world.Solved;
                Rebuild();
                if (!wasSolved && world.Solved)
                {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                    if (localPlaytest != null) localPlaytest.Solve(Time.unscaledTime);
#endif
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
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            if (localPlaytest != null) localPlaytest.Hint(Time.unscaledTime);
#endif
            SaveWorld();
            celebrateSolve = !wasSolved && world.Solved; // a hint that finishes the creek blooms too
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
                if (feedbackText != null && landmark.Length > 0) CjkLineBreakFixer.SetText(feedbackText, landmark);
                semanticsDirty = true;
                return;
            }
            bool before = world.Solved;
            if (!world.Rotate(index)) return;
            PlayTock();
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            if (localPlaytest != null) localPlaytest.Rotate(Time.unscaledTime);
#endif
            AfterPuzzleAction(before);
        }

        private void Act(Func<bool> action, string actionKind)
        {
            bool before = world.Solved;
            if (!action()) return;
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            if (localPlaytest != null)
            {
                switch (actionKind)
                {
                    case "undo": localPlaytest.Undo(Time.unscaledTime); break;
                    case "hint": localPlaytest.Hint(Time.unscaledTime); break;
                    case "restart": localPlaytest.Restart(Time.unscaledTime); break;
                }
            }
#endif
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
            float target = 1f - Mathf.Clamp01((targetTop - context) / distance);
            int generation = rebuildGeneration;
            // The payoff beat: hold on the finished creek while it blooms and
            // every droplet fills, then glide (not jump) to the decision. A
            // player who scrolls during the hold keeps control.
            float held = scroll.verticalNormalizedPosition;
            float wait = 0f;
            while (wait < SolveHoldSeconds)
            {
                wait += Time.unscaledDeltaTime;
                yield return null;
                // Any rebuild (choice, sound, text size) or a player scroll cancels the glide.
                if (scroll == null || decisionHeading == null || generation != rebuildGeneration) yield break;
                if (Mathf.Abs(scroll.verticalNormalizedPosition - held) > 0.01f) yield break;
            }
            scroll.StopMovement();
            float from = scroll.verticalNormalizedPosition;
            for (float t = gentleMotion ? 0f : 1f; t < 1f; t += Time.unscaledDeltaTime / SolveGlideSeconds)
            {
                if (scroll == null || decisionHeading == null || generation != rebuildGeneration) yield break;
                scroll.verticalNormalizedPosition = Mathf.Lerp(from, target, Mathf.SmoothStep(0f, 1f, t));
                yield return null;
            }
            if (scroll != null) scroll.verticalNormalizedPosition = target;
        }

        private const float SolveHoldSeconds = 1.25f;
        private const float SolveGlideSeconds = 0.6f;

        private void MakeChoice(VillageChoice choice)
        {
            if (!world.Choose(choice)) return;
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            if (localPlaytest != null) localPlaytest.Choose(Time.unscaledTime);
#endif
            SaveWorld();
            showingMemory = true;
            showJournal = false;
            speakingWith = -1;
            PlayChime();
            celebrateGrowth = gentleMotion;
            Rebuild(true);
        }

        // Player-owned copy of the garden. Clipboard only; nothing leaves the device.
        // Lives in settings, below the puzzle, and never on the primary path.
        private void BuildGardenFileSection()
        {
            var card = Card(content, "Garden file", MosseloraTheme.CardFill);
            GameObject files = Row(card.transform, "Garden file actions");
            IconButton(files.transform, "Copy garden file", GameLocalization.Text("garden_copy"),
                MosseloraArt.IconCopyFile, CopyGardenFile, 56, MosseloraTheme.ForestAction);
            if (!WorldStore.RecoveryBlocked)
            {
                if (!gardenRestoreConfirm)
                    IconButton(files.transform, "Restore garden file", GameLocalization.Text("garden_restore"),
                        MosseloraArt.IconRestoreFile, ReadGardenFile, 56, MosseloraTheme.WaterAction);
                else
                {
                    Label(card.transform, "Garden replace warning", GameLocalization.Text("garden_replace"),
                        13, 40, Ink);
                    IconButton(card.transform, "Confirm garden restore", GameLocalization.Text("erase"),
                        MosseloraArt.IconConfirm, ConfirmGardenRestore, 54, MosseloraTheme.EarthAction, true);
                    IconButton(card.transform, "Cancel garden restore", GameLocalization.Text("keep"),
                        MosseloraArt.IconCancel, CancelGardenRestore, 52, MosseloraTheme.QuietAction);
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

        // ---- I21: procedural creek ambience and a wooden pipe "tock" (sound on only).
        private AudioSource creek;
        private AudioClip creekLoop;
        private AudioClip tock;

        private void PlayTock()
        {
            if (!soundOn) return;
            if (tock == null)
            {
                const int sampleRate = 22050;
                const int length = 2400;
                var samples = new float[length];
                var noise = new System.Random(7);
                for (int i = 0; i < length; i++)
                {
                    float t = (float)i / sampleRate;
                    float envelope = Mathf.Exp(-t * 55f);
                    float body = Mathf.Sin(t * 2f * Mathf.PI * 330f) + 0.4f * Mathf.Sin(t * 2f * Mathf.PI * 495f);
                    float click = (float)(noise.NextDouble() * 2.0 - 1.0) * Mathf.Exp(-t * 400f);
                    samples[i] = envelope * 0.16f * body + 0.05f * click;
                }
                tock = AudioClip.Create("Mosselora wooden tock", length, 1, sampleRate, false);
                tock.SetData(samples, 0);
            }
            sound.PlayOneShot(tock, 0.8f);
        }

        // A soft, seamless creek murmur; loudness follows the real water reach.
        private void UpdateCreekSound()
        {
            if (!soundOn || world == null || world.Completed)
            {
                if (creek != null) creek.Stop();
                return;
            }
            if (creek == null)
            {
                creek = gameObject.AddComponent<AudioSource>();
                creek.loop = true;
                creek.playOnAwake = false;
                creek.volume = 0f;
            }
            if (creekLoop == null)
            {
                const int sampleRate = 22050;
                const int length = sampleRate * 4;
                var samples = new float[length];
                var noise = new System.Random(11);
                float brown = 0f, smooth = 0f;
                for (int i = 0; i < length; i++)
                {
                    brown = Mathf.Clamp(brown + (float)(noise.NextDouble() * 2.0 - 1.0) * 0.06f, -1f, 1f);
                    smooth += (brown - smooth) * 0.08f; // low-pass: water, not hiss
                    float t = (float)i / length;
                    float burble = 0.75f + 0.25f * Mathf.Sin(t * 2f * Mathf.PI * 3f) * Mathf.Sin(t * 2f * Mathf.PI * 7f);
                    samples[i] = smooth * burble;
                }
                // Crossfade the tail into the head so the loop has no seam.
                int fade = sampleRate / 4;
                for (int i = 0; i < fade; i++)
                {
                    float w = (float)i / fade;
                    samples[i] = samples[i] * w + samples[length - fade + i] * (1f - w);
                }
                var looped = new float[length - fade];
                System.Array.Copy(samples, looped, looped.Length);
                creekLoop = AudioClip.Create("Mosselora creek", looped.Length, 1, sampleRate, false);
                creekLoop.SetData(looped, 0);
            }
            if (creek.clip != creekLoop) creek.clip = creekLoop;
            float reach = world.Solved ? 1f : VillageScenePlan.From(world).WaterReachPercent / 100f;
            creek.volume = reach <= 0f ? 0f : 0.05f + 0.20f * reach;
            if (!creek.isPlaying) creek.Play();
        }

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
