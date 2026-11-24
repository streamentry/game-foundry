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
        private GameObject journalSheet; // the memory scrapbook rises as its own sheet
        private RectTransform journalContent;
        private ScrollRect journalScroll;
        private GameObject titleScreen; // the painted welcome on every launch
        private readonly System.Collections.Generic.List<GameObject> goalGlows =
            new System.Collections.Generic.List<GameObject>();
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
        // M3 participant view: research and developer controls hidden while a
        // local report runs. Holding the wordmark brings them back.
        private bool participantView;
        // Opt-in return diary (local dates only). PlayerPrefs, separate from the garden save.
        private const string ReturnDiaryKey = "evergrove:return-diary";
        private ReturnDiary returnDiary = new ReturnDiary();
        private bool returnDiaryAsked;
        private bool returnDiaryCopied;
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
        private bool postcardCopied;
        private int settledRound = -1; // the round whose board has already settled in
        private bool[] lastFlowing; // which tiles held water before this move
        private bool celebrateGrowth; // one-shot: the memory screen shows the new landmark growing
        private string lastScreenKey = "";
        private CanvasGroup pageFade; // one-shot: the rebuild after the solving tap blooms
        private Image[] waterDrops; // one painted droplet per tile the water can reach
        private Button undoButton;
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        private static readonly bool srLogEveryScreen = DevArgs.Read("-mosseloraSrLog") == "1";
#endif
        private RectTransform decisionHeading; // anchor for intentional post-solve navigation
        private RectTransform solvedBoard;     // the board whose payoff the decision glide keeps whole
        private int hintTurnedTile = -1;       // the tile the third hint just turned
        private readonly System.Collections.Generic.List<GameObject> hintMarkers =
            new System.Collections.Generic.List<GameObject>(); // cleared when a move ends the hint
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

        // The scene has no camera, so the first frames cleared to black between
        // the parchment launch screen and the UI. A paper-clearing camera at
        // the earliest hook keeps every frame the same warm paper.
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSplashScreen)]
        private static void PaperCamera()
        {
            if (Camera.main != null) return;
            var host = new GameObject("Paper camera");
            DontDestroyOnLoad(host);
            var camera = host.AddComponent<Camera>();
            camera.tag = "MainCamera";
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = MosseloraTheme.Paper;
            camera.cullingMask = 0; // draws nothing but the paper
            camera.orthographic = true;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Bootstrap()
        {
            if (FindFirstObjectByType<EvergroveApp>() != null) return;
            var host = new GameObject("Evergrove");
            DontDestroyOnLoad(host);
            host.AddComponent<EvergroveApp>();
        }

#if UNITY_IOS && !UNITY_EDITOR
        [System.Runtime.InteropServices.DllImport("__Internal")]
        private static extern void _MosseloraLaunchCoverDone();
#endif

        // The native parchment launch cover (Plugins/iOS) stays until the first
        // screen has been drawn, then fades.
        private IEnumerator ReleaseLaunchCover()
        {
            yield return null;
            yield return new WaitForEndOfFrame();
            yield return null;
#if UNITY_IOS && !UNITY_EDITOR
            _MosseloraLaunchCoverDone();
#endif
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
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            returnDiary = ReturnDiary.Parse(PlayerPrefs.GetString(ReturnDiaryKey, ""));
            RecordReturnDiaryOpen();
#endif
            CreateRoot();
            Rebuild(true);
            StartCoroutine(ReleaseLaunchCover());
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
            // -mosseloraSrLog 1: log the screen-reader tree on every refresh, on any
            // screen (the hub included), to check labels without VoiceOver (I82).
            if (srLogEveryScreen) { logThisScreen = true; semanticsLogged = false; }
#endif
            if (!logThisScreen && !ScreenReaderBridge.Active) return;
            // While the hub covers the page, only the hub is readable: the board
            // and its tools beneath it are not reachable by swiping (I82).
            RectTransform readable = titleScreen != null
                ? titleScreen.GetComponent<RectTransform>()
                : canvasRoot != null ? canvasRoot : content;
            System.Collections.Generic.List<ScreenReaderBridge.SemanticNode> nodes;
            Transform hubChrome = titleScreen != null ? titleScreen.transform.Find("Hub chrome") : null;
            // The hub's menu and book are read first, as they appear top-left/right (I93).
            if (hubChrome != null)
            {
                nodes = ScreenReaderBridge.Collect((RectTransform)hubChrome, DescribeButton);
                nodes.AddRange(ScreenReaderBridge.Collect(readable, DescribeButton, hubChrome));
            }
            else
                nodes = ScreenReaderBridge.Collect(readable, DescribeButton);
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
            FitColumn();
        }

        // Tablets (I106): scale by height so the page keeps a phone's vertical
        // room, and keep the page a book-width column in the middle instead of
        // stretching every painting across a wide screen. Phones are unchanged.
        private const float TabletAspect = 0.62f;
        private const float MaxColumn = 480f;
        private float columnInset;

        private void FitColumn()
        {
            if (viewportScaler == null || Screen.height <= 0) return;
            Rect safe = Screen.safeArea;
            bool tablet = safe.height > 0f && safe.width / safe.height > TabletAspect;
            viewportScaler.matchWidthOrHeight = tablet ? 1f : 0.5f;
            float logicalWidth = safe.width / LiveCanvasScale();
            columnInset = tablet ? Mathf.Max(0f, Mathf.Floor((logicalWidth - MaxColumn) / 2f)) : 0f;
            var column = content != null ? content.GetComponent<VerticalLayoutGroup>() : null;
            if (column != null)
                column.padding = new RectOffset(17 + (int)columnInset, 17 + (int)columnInset, 15, 36);
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
            var backdropImage = backdrop.AddComponent<Image>();
            MosseloraArt.PagePaper(backdropImage, Paper); // painted paper, not a flat fill (I94)
            backdropImage.raycastTarget = false;

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
            FitColumn();
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
            if (NativeTextLabel.Applies)
            {
                NativeTextLabel.Attach(saveWarningCaption);
            }
            else
            {
                saveWarningCaption.text = IndicVisualOrder.Visual(GameLocalization.Current,
                    saveWarningCaption.text);
                if (CjkLineBreakFixer.NeedsFitting(GameLocalization.Current))
                    CjkLineBreakFixer.Attach(saveWarningCaption, saveWarningCaption.text);
            }
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
            if (NativeTextLabel.Applies)
            {
                // EG-006 iteration 7: the OS shaper draws bn; keep the logical string.
                label.text = GameLocalization.Display(value);
                NativeTextLabel.Attach(label);
                return label;
            }
            // uGUI has no Indic shaping: Bengali pre-base vowel signs are
            // pre-ordered into visual order here (IndicVisualOrder, bn only).
            string shown = IndicVisualOrder.Visual(GameLocalization.Current,
                GameLocalization.Display(value));
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
            Image plaque = MosseloraArt.Plaque(image, tone); // Primary actions are rare; choices are equal.
            if (plaque == null) MosseloraArt.Round(image, tone, 16);
            MosseloraArt.Lift(plaque != null ? plaque : image, 2f, quiet ? 0.10f : 0.22f);
            if (quiet && plaque == null) MosseloraArt.Edge(node.transform, MosseloraTheme.CardEdge, 16);
            var button = node.AddComponent<Button>();
            button.targetGraphic = plaque != null ? plaque : image;
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
            if (NativeTextLabel.Applies)
            {
                text.text = GameLocalization.Display(caption);
                NativeTextLabel.Attach(text);
            }
            else
            {
                string shownCaption = IndicVisualOrder.Visual(GameLocalization.Current,
                    GameLocalization.Display(caption));
                if (CjkLineBreakFixer.NeedsFitting(GameLocalization.Current))
                    CjkLineBreakFixer.Attach(text, shownCaption);
                else
                    text.text = shownCaption;
            }
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
            // Painted enamel-and-wood plaque (Codex) for the main tones; the
            // rounded fill remains the fallback and for other tones.
            Image plaque = MosseloraArt.Plaque(image, tone);
            if (plaque == null) MosseloraArt.Round(image, tone, 18);
            MosseloraArt.Lift(plaque != null ? plaque : image, 2f, quiet ? 0.10f : 0.24f);
            if (quiet && plaque == null) MosseloraArt.Edge(node.transform, MosseloraTheme.CardEdge, 18);
            var button = node.AddComponent<Button>();
            button.targetGraphic = plaque != null ? plaque : image;
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
                    dotLayout.minWidth = dotLayout.preferredWidth = 10;
                    dotLayout.minHeight = dotLayout.preferredHeight = 10;
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

        // "Orin remembers the bridge you restored." told beside Orin's portrait.
        private void KeeperLine(Transform parent, string name, int recall = -1)
        {
            int[] past = world.Choices;
            int index = recall >= 0 && recall < past.Length ? recall : past.Length - 1;
            Transform memory = CenteredRowIn(parent, name + " row");
            memory.GetComponent<HorizontalLayoutGroup>().spacing = 8;
            Sprite who = MosseloraArt.Portrait(past[index]);
            if (who != null)
            {
                var face = Node("Who remembers", memory);
                var faceLayout = face.AddComponent<LayoutElement>();
                faceLayout.minWidth = faceLayout.preferredWidth = 34;
                faceLayout.minHeight = faceLayout.preferredHeight = 34;
                var faceImage = face.AddComponent<Image>();
                faceImage.sprite = who;
                faceImage.preserveAspect = true;
                faceImage.raycastTarget = false;
            }
            Text line = Label(memory, name, LocalizedVillage.MemoryAt(past, index), 14, 0, Ink, TextAnchor.MiddleLeft);
            line.GetComponent<LayoutElement>().flexibleWidth = 1f;
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
                    : new Color(0.36f, 0.40f, 0.37f, 0.85f); // dry: a solid slate silhouette, >=3:1 on paper (I105)
        }

        // A calm daily ritual entry: the village painting full-bleed, the
        // wordmark, the stones walked so far and one big play button. No
        // streaks, no "you were away" - the garden simply waited.
        private void BuildTitleScreen(Transform host)
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            if (qaInMemoryOnly) return; // the scripted QA tour starts on the board
#endif
            // A finished garden opens on its festival; otherwise the everyday village.
            // The hub opens on *your* story: the finished garden's festival, else
            // the painting of your most recent memory, else the everyday village.
            int[] story = world.Choices;
            // Newest memory first; each later launch recalls an earlier one (I107).
            hubRecall = world.Mode == PlayMode.Living ? PickHubRecall(story.Length) : -1;
            Sprite hero = null;
            if (world.Completed) hero = MosseloraArt.Load(MosseloraArt.EndingFestival);
            else if (hubRecall >= 0)
                hero = MosseloraArt.Load(story[hubRecall] == 0 ? MosseloraArt.VignetteTree
                    : story[hubRecall] == 1 ? MosseloraArt.VignetteBridge : MosseloraArt.VignetteFlowers);
            if (hero == null) hero = MosseloraArt.Load(MosseloraArt.HeroVillage);
            if (hero == null) return;
            titleScreen = Node("Title screen", host);
            Fill(titleScreen.GetComponent<RectTransform>());
            MosseloraArt.PagePaper(titleScreen.AddComponent<Image>(), Paper);

            // The village at the top, clipped to the upper half.
            var window = Node("Village window", titleScreen.transform);
            var windowRect = window.GetComponent<RectTransform>();
            // Reaches down behind the strip's sky so no blank band opens between them (I73).
            windowRect.anchorMin = new Vector2(0f, 0.41f);
            windowRect.anchorMax = Vector2.one;
            windowRect.offsetMin = windowRect.offsetMax = Vector2.zero;
            window.AddComponent<RectMask2D>();
            var art = Node("Village at the start", window.transform);
            var artRect = art.GetComponent<RectTransform>();
            artRect.anchorMin = Vector2.zero;
            artRect.anchorMax = Vector2.one;
            artRect.pivot = new Vector2(0.5f, 0.6f);
            artRect.offsetMin = artRect.offsetMax = Vector2.zero;
            var artImage = art.AddComponent<Image>();
            artImage.sprite = hero;
            artImage.raycastTarget = false;
            // Cover the window whatever the painting's shape (portrait village,
            // landscape memory vignette), so no blank band appears.
            var fitter = art.AddComponent<AspectRatioFitter>();
            fitter.aspectMode = AspectRatioFitter.AspectMode.EnvelopeParent;
            fitter.aspectRatio = MosseloraArt.Aspect(hero);
            var melt = Node("Village melt", window.transform);
            var meltRect = melt.GetComponent<RectTransform>();
            meltRect.anchorMin = Vector2.zero;
            meltRect.anchorMax = new Vector2(1f, 0.24f);
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
            painting.Build(0f, PaintedVillage.SeasonFor(VillageScenePlan.From(world).LandscapePhase, world.Completed));
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
                Height(markNode, 96f); // the repo's painted brand logo (about 2.6:1)
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
            // What changed last time, told by the neighbor who remembers it.
            if (world.Mode == PlayMode.Living && world.Choices.Length > 0)
                KeeperLine(stack.transform, "Last memory", hubRecall);
            Transform play = CenteredRowIn(stack.transform, "Play row");
            // The big button names where the walk continues ("First Bend").
            string nextStop = !world.Completed && GameLocalization.Current == "en"
                ? world.CurrentPuzzle.Name : GameLocalization.Text("continue");
            // An icon-only play button; the next stream's name sits above it as
            // a plain label (and is the button's spoken name).
            if (!world.Completed && GameLocalization.Current == "en")
                NextStreamLine(stack.transform, nextStop, true);
            Button start = IconButton(play, "Play", nextStop,
                MosseloraArt.IconContinue, CloseTitle, 72, MosseloraTheme.WaterAction);
            Width(start, 150);
            HubChrome();
            titleScreen.transform.SetAsLastSibling();
            StartCoroutine(FitHubStack(windowRect, stripRect, stackRect));
        }

        // The words and Play under the painting take the room they need: at
        // larger text, or with a memory line and the next stream, the painting
        // and village strip move up instead of the words overflowing (I85).
        private IEnumerator FitHubStack(RectTransform window, RectTransform strip, RectTransform stack)
        {
            yield return null; // real canvas size, after the first layout pass
            if (stack == null || canvasRoot == null) yield break;
            LayoutRebuilder.ForceRebuildLayoutImmediate(stack);
            float canvasHeight = canvasRoot.rect.height;
            if (canvasHeight <= 1f) yield break;
            float needed = LayoutUtility.GetPreferredHeight(stack) + 28f; // air above the wordmark
            float top = Mathf.Clamp(0.03f + needed / canvasHeight, 0.33f, 0.52f);
            float shift = top - 0.33f;
            if (shift <= 0.001f) yield break;
            stack.anchorMax = new Vector2(1f, top);
            strip.anchorMin = new Vector2(0f, 0.33f + shift);
            strip.anchorMax = new Vector2(1f, 0.52f + shift);
            window.anchorMin = new Vector2(0f, 0.41f + shift);
        }

        // The hub wears the same two wooden buttons as the game, in the same
        // corners: the menu (left) and, once there are memories, the book (right).
        // Each steps off the hub straight into what it opens (I83).
        private void HubChrome()
        {
            var bar = Node("Hub chrome", titleScreen.transform);
            var rect = bar.GetComponent<RectTransform>();
            Rect safe = Screen.safeArea;
            float w = Mathf.Max(1, Screen.width), h = Mathf.Max(1, Screen.height);
            rect.anchorMin = new Vector2(safe.xMin / w, safe.yMax / h);
            rect.anchorMax = new Vector2(safe.xMax / w, safe.yMax / h);
            rect.pivot = new Vector2(0.5f, 1f);
            rect.sizeDelta = new Vector2(0f, 70f);
            rect.anchoredPosition = Vector2.zero;
            Button menu = IconButton(bar.transform, "Open menu", GameLocalization.Text("menu"),
                MosseloraArt.IconMenu, () => { CloseTitle(); SetMenu(true); }, 54, MosseloraTheme.QuietAction);
            PlaceChrome(menu, new Vector2(0f, 1f), new Vector2(10f, -6f));
            if (world.Mode == PlayMode.Living && world.Choices.Length > 0)
            {
                Button book = IconButton(bar.transform, "Memories", GameLocalization.Text("journal_show"),
                    MosseloraArt.IconJournal, () => {
                        CloseTitle();
                        menuOpen = false;
                        showJournal = true;
                        Rebuild();
                        if (journalScroll != null) journalScroll.verticalNormalizedPosition = 1f;
                    }, 54, MosseloraTheme.QuietAction, false,
                    world.Choices.Length.ToString(CultureInfo.InvariantCulture));
                PlaceChrome(book, new Vector2(1f, 1f), new Vector2(-10f, -6f));
            }
        }

        // "Rainwater" with the painted landmark your last choice planted in it:
        // the next stream visibly carries what you grew (I73). Placed above Play.
        private void NextStreamLine(Transform stack, string nextStop, bool abovePlay)
        {
            int split = nextStop.IndexOf(" · ", System.StringComparison.Ordinal);
            int[] past = world.Choices;
            Sprite mark = split > 0 && past.Length > 0
                ? MosseloraArt.ChoiceIcon((VillageChoice)past[past.Length - 1]) : null;
            Transform row = CenteredRowIn(stack, "Next stream row");
            row.GetComponent<HorizontalLayoutGroup>().spacing = 6;
            if (mark != null)
            {
                var icon = Node("Shaped by your choice", row);
                var iconLayout = icon.AddComponent<LayoutElement>();
                iconLayout.minWidth = iconLayout.preferredWidth = 34;
                iconLayout.minHeight = iconLayout.preferredHeight = 34;
                var iconImage = icon.AddComponent<Image>();
                iconImage.sprite = mark;
                iconImage.preserveAspect = true;
                iconImage.raycastTarget = false;
                BloomPulse.Add(icon, 0.6f, 0.05f);
            }
            Text where = Label(row, "Next stream", mark != null ? nextStop.Substring(0, split) : nextStop,
                16, 0, Moss, TextAnchor.MiddleCenter);
            Emphasize(where);
            if (abovePlay) row.SetSiblingIndex(stack.childCount - 2);
        }

        private int hubRecall = -1;
        private const string HubRecallKey = "evergrove:hub-recall";

        // Reads "count:index" from the last launch, picks this launch's memory
        // with Core.HubRecall, and stores it. Local only; no clock involved.
        private static int PickHubRecall(int memoryCount)
        {
            int lastCount = 0, lastIndex = -1;
            string[] saved = PlayerPrefs.GetString(HubRecallKey, "").Split(':');
            if (saved.Length == 2)
            {
                int.TryParse(saved[0], out lastCount);
                if (!int.TryParse(saved[1], out lastIndex)) lastIndex = -1;
            }
            int index = HubRecall.Next(memoryCount, lastCount, lastIndex);
            if (index >= 0)
            {
                PlayerPrefs.SetString(HubRecallKey, memoryCount + ":" + index);
                PlayerPrefs.Save();
            }
            return index;
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
            menuSheet = BuildSheet(host, "Menu", () => SetMenu(false), out menuContent, out menuScroll);
            journalSheet = BuildSheet(host, "Journal", () => {
                showJournal = false;
                Rebuild();
            }, out journalContent, out journalScroll);
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

        private const float SheetHeader = 78f; // the plaque art stands a little taller than its 52 pt box

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
                () => close(), 52, MosseloraTheme.QuietAction);
            Width(button, 60);
            button.GetComponent<LayoutElement>().ignoreLayout = true;
            PlaceChrome(button, new Vector2(1f, 1f), new Vector2(-12f, -9f));
            // After the paper grain, before the pages: read first, drawn clear of the pages.
            button.transform.SetSiblingIndex(Mathf.Min(1, panel.childCount - 1));
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
                        if (showJournal) menuOpen = false;
                        Rebuild();
                        if (showJournal && journalScroll != null) journalScroll.verticalNormalizedPosition = 1f;
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
                // On the play page the seedling stays a bare icon (ending); in the
                // menu it says what it does.
                bool inMenu = content == menuContent;
                // The finished season's first offer is to share it: a postcard
                // plaque (teal) beside a quieter "new garden" seedling (I89).
                bool sharePostcard = !inMenu && world.Completed && world.Mode == PlayMode.Living;
                if (sharePostcard)
                {
                    Button postcard = null;
                    postcard = IconButton(row, "Copy garden postcard", GameLocalization.Text("postcard"),
                        MosseloraArt.IconChat, () => {
                            GUIUtility.systemCopyBuffer = GardenPostcard.Build(world, "");
                            RuleToast.Show(postcard.GetComponent<RectTransform>(),
                                GameLocalization.Text("postcard_copied"), font, Ink, MosseloraTheme.CardFill);
                        }, 58, MosseloraTheme.WaterAction);
                    Width(postcard, 96);
                    ShowPostcard(postcard);
                }
                Width(IconButton(row, resetName, GameLocalization.Text("explore"),
                    MosseloraArt.IconNewGarden, () => { confirmReset = true; Rebuild(); },
                    inMenu ? 78 : 58, sharePostcard ? MosseloraTheme.QuietAction : tone, false, null, inMenu),
                    inMenu ? 200 : 96);
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

        // The neighbor speaks from their portrait in a speech bubble; tapping
        // the check (or the portrait again) ends the conversation.
        private void NeighborConversation()
        {
            var row = Node("A neighbor's memory", content);
            screenHeading = row.GetComponent<RectTransform>();
            var rowGroup = row.AddComponent<HorizontalLayoutGroup>();
            rowGroup.reverseArrangement = GameLocalization.IsRtl(GameLocalization.Current);
            rowGroup.spacing = 10;
            rowGroup.childAlignment = TextAnchor.UpperLeft;
            rowGroup.childControlWidth = true;
            rowGroup.childControlHeight = true;
            rowGroup.childForceExpandWidth = false;
            rowGroup.childForceExpandHeight = false;
            Sprite face = MosseloraArt.Portrait(speakingWith);
            if (face != null)
            {
                var portrait = Node("Speaker portrait", row.transform);
                var portraitLayout = portrait.AddComponent<LayoutElement>();
                portraitLayout.minWidth = portraitLayout.preferredWidth = 64;
                portraitLayout.minHeight = portraitLayout.preferredHeight = 64;
                var portraitImage = portrait.AddComponent<Image>();
                portraitImage.sprite = face;
                portraitImage.preserveAspect = true;
                portraitImage.raycastTarget = false;
                MosseloraArt.Lift(portraitImage, 2f, 0.2f);
            }
            var bubble = Card(row.transform, "Speech bubble", MosseloraTheme.CardFill);
            bubble.AddComponent<LayoutElement>().flexibleWidth = 1f;
            Text speaker = Label(bubble.transform, "Neighbor name",
                LocalizedVillage.Name(speakingWith), 15, 0, Leaf, TextAnchor.MiddleLeft);
            Emphasize(speaker);
            Label(bubble.transform, "What they say",
                LocalizedVillage.Speak(world, speakingWith), 17, 0, Ink, TextAnchor.MiddleLeft);
            var thanks = IconButton(bubble.transform, "Thanks", GameLocalization.Text("thanks"),
                MosseloraArt.IconConfirm, () => { speakingWith = -1; Rebuild(); }, 44,
                MosseloraTheme.ForestAction);
            // Tucked into the bubble's lower-right corner, outside the text flow.
            thanks.gameObject.GetComponent<LayoutElement>().ignoreLayout = true;
            var thanksRect = thanks.GetComponent<RectTransform>();
            thanksRect.anchorMin = thanksRect.anchorMax = new Vector2(1f, 0f);
            thanksRect.pivot = new Vector2(1f, 0f);
            thanksRect.sizeDelta = new Vector2(56f, 44f);
            thanksRect.anchoredPosition = new Vector2(-10f, 10f);
            bubble.GetComponent<VerticalLayoutGroup>().padding = new RectOffset(16, 16, 14, 58);
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
            // On the hub, a small parchment speech mark on a neighbor with
            // something new to say: the day's reason to stop by (I80).
            bool news = fromTitle && HasNews(villager);
            if (news)
            {
                var note = MosseloraArt.Ornament(picture.transform, "Has something new to say",
                    MosseloraArt.Rounded(14), new Vector2(1f, 1f), new Vector2(0.5f, 0.5f),
                    new Vector2(32f, 32f), new Vector2(-5f, -5f));
                if (note != null)
                {
                    note.preserveAspect = false;
                    note.color = MosseloraTheme.CardFill;
                    MosseloraArt.Lift(note, 1.5f, 0.22f);
                    var mark = MosseloraArt.Ornament(note.transform, "Speech mark",
                        MosseloraArt.Load(MosseloraArt.IconChat), new Vector2(0.5f, 0.5f),
                        new Vector2(0.5f, 0.5f), new Vector2(21f, 21f), Vector2.zero);
                    if (mark != null) mark.color = MosseloraTheme.WaterChannel;
                    if (gentleMotion) BloomPulse.Add(note.gameObject, 0.9f + 0.25f * villager, 0.18f);
                }
            }
            if (speakingWith == villager)
            {
                var ring = MosseloraArt.Ornament(picture.transform, "Speaking ring", MosseloraArt.Ring(16),
                    new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(medallion + 8f, medallion + 8f), Vector2.zero);
                if (ring != null) ring.color = Leaf;
            }
            Text caption;
            if (onPainting)
            {
                // A small cream name plate, dark ink: legible over any part of the
                // painting (white-on-painting measured about 2:1).
                var plate = Node("Caption", node.transform); // ScreenReaderBridge reads its text
                var plateImage = plate.AddComponent<Image>();
                MosseloraArt.Round(plateImage, new Color(1f, 0.973f, 0.918f, 0.94f), 10);
                plateImage.raycastTarget = false;
                var plateGroup = plate.AddComponent<HorizontalLayoutGroup>();
                plateGroup.padding = new RectOffset(8, 8, 1, 2);
                plateGroup.childControlWidth = true;
                plateGroup.childControlHeight = true;
                plateGroup.childForceExpandWidth = false;
                caption = Label(plate.transform, "Caption text", label, 14, 0, Ink, TextAnchor.MiddleCenter);
            }
            else
                caption = Label(node.transform, "Caption", label, 15, 0, Ink, TextAnchor.MiddleCenter);
            Emphasize(caption);
            // The speech mark is a picture; screen readers hear it with the name (I81).
            if (news) SpeakInstead(node.transform, label + ". " + GameLocalization.Text("has_news"));
        }

        // The share button shows what it shares: a tiny postcard of the season's
        // festival painting, with the chat mark tucked in its corner (I97).
        private static void ShowPostcard(Button button)
        {
            Transform iconNode = button.transform.Find("Icon");
            Image icon = iconNode != null ? iconNode.GetComponent<Image>() : null;
            Sprite festival = MosseloraArt.Load(MosseloraArt.EndingFestival);
            if (icon == null || festival == null) return;
            Sprite chat = icon.sprite;
            var layout = iconNode.GetComponent<LayoutElement>();
            layout.minWidth = layout.preferredWidth = 50f;
            layout.minHeight = layout.preferredHeight = 38f;
            // Parchment card with a small border, the painting inset in it.
            icon.sprite = MosseloraArt.Rounded(6);
            icon.type = Image.Type.Sliced;
            icon.preserveAspect = false;
            icon.color = MosseloraTheme.CardFill;
            MosseloraArt.Lift(icon, 1.5f, 0.3f);
            var picture = new GameObject("Postcard painting", typeof(RectTransform));
            picture.transform.SetParent(iconNode, false);
            var pictureRect = picture.GetComponent<RectTransform>();
            pictureRect.anchorMin = Vector2.zero;
            pictureRect.anchorMax = Vector2.one;
            pictureRect.offsetMin = new Vector2(3f, 3f);
            pictureRect.offsetMax = new Vector2(-3f, -3f);
            picture.AddComponent<RectMask2D>();
            var art = new GameObject("Festival", typeof(RectTransform));
            art.transform.SetParent(picture.transform, false);
            var artRect = art.GetComponent<RectTransform>();
            artRect.anchorMin = Vector2.zero;
            artRect.anchorMax = Vector2.one;
            artRect.offsetMin = artRect.offsetMax = Vector2.zero;
            var artImage = art.AddComponent<Image>();
            artImage.sprite = festival;
            artImage.raycastTarget = false;
            var fit = art.AddComponent<AspectRatioFitter>();
            fit.aspectMode = AspectRatioFitter.AspectMode.EnvelopeParent;
            fit.aspectRatio = MosseloraArt.Aspect(festival);
            var badge = MosseloraArt.Ornament(iconNode, "Share mark", MosseloraArt.Rounded(12),
                new Vector2(1f, 0f), new Vector2(0.5f, 0.5f), new Vector2(24f, 24f), new Vector2(-1f, 2f));
            if (badge == null) return;
            badge.preserveAspect = false;
            badge.color = MosseloraTheme.CardFill;
            MosseloraArt.Lift(badge, 1.5f, 0.25f);
            var mark = MosseloraArt.Ornament(badge.transform, "Chat mark", chat,
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(15f, 15f), Vector2.zero);
            if (mark != null) mark.color = MosseloraTheme.WaterChannel;
        }

        // "Change your choice" shows what would change: the painted landmark you
        // just chose, with a small turn-back mark at its corner (I87). Two opposed
        // arrows read as "shuffle" in three AI reviews.
        private static void ShowChoiceToChange(Button button, VillageChoice chosen)
        {
            Transform iconNode = button.transform.Find("Icon");
            Image icon = iconNode != null ? iconNode.GetComponent<Image>() : null;
            Sprite painted = MosseloraArt.ChoiceIcon(chosen);
            if (icon == null || painted == null) return;
            icon.sprite = painted;
            icon.color = Color.white; // painted art, not a tinted glyph
            var layout = iconNode.GetComponent<LayoutElement>();
            layout.minWidth = layout.preferredWidth = 46f;
            layout.minHeight = layout.preferredHeight = 46f;
            var badge = MosseloraArt.Ornament(iconNode, "Turn back", MosseloraArt.Rounded(12),
                new Vector2(1f, 0f), new Vector2(0.5f, 0.5f), new Vector2(24f, 24f), new Vector2(-2f, 4f));
            if (badge == null) return;
            badge.preserveAspect = false;
            badge.color = MosseloraTheme.CardFill;
            MosseloraArt.Lift(badge, 1.5f, 0.25f);
            var mark = MosseloraArt.Ornament(badge.transform, "Undo mark", MosseloraArt.Load(MosseloraArt.IconUndo),
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(15f, 15f), Vector2.zero);
            if (mark != null) mark.color = MosseloraTheme.WaterChannel;
        }

        // Keeps what is shown, and gives screen readers a fuller hidden "Caption"
        // (ScreenReaderBridge reads the child named "Caption").
        private static void SpeakInstead(Transform control, string spoken)
        {
            Transform shown = control.Find("Caption");
            Text shownText = shown != null ? shown.GetComponentInChildren<Text>() : null;
            if (shown == null || shownText == null) return;
            shown.name = "Shown name";
            var spokenNode = new GameObject("Caption", typeof(RectTransform));
            spokenNode.transform.SetParent(control, false);
            spokenNode.AddComponent<LayoutElement>().ignoreLayout = true;
            var text = spokenNode.AddComponent<Text>();
            text.font = shownText.font;
            text.fontSize = shownText.fontSize;
            text.text = spoken;
            text.color = new Color(0f, 0f, 0f, 0f);
            text.raycastTarget = false;
        }

        // Painted page header: generated wordmark with vine corners. The
        // localized title stays as transparent text for screen readers.
        private const float ChromeInset = 84f; // book button with its count badge + 10 pt margin + air

        private void GardenHeader(bool compact)
        {
            Sprite mark = MosseloraArt.Load(MosseloraArt.Wordmark);
            if (mark == null)
            {
                Text plain = Label(content, "Mosselora wordmark", GameLocalization.Text("title"),
                    15, 26, Leaf, TextAnchor.MiddleCenter);
                Emphasize(plain);
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                if (participantView)
                {
                    plain.raycastTarget = true;
                    plain.gameObject.AddComponent<FacilitatorHold>().Held = EndParticipantView;
                }
#endif
                return;
            }
            // Compact enough to sit between the floating menu and book buttons.
            // The repo's brand logo (about 2.6:1) needs more height than the old
            // flat wordmark for its letters to read; still inset past the chrome.
            float markHeight = compact ? 64f : 72f;
            var header = Node("Mosselora wordmark", content);
            Height(header, markHeight + 8f);
            // Inset past the floating menu (left) and book (right) buttons, so the
            // mark scales down instead of touching them (I79).
            var markNode = Node("Mark", header.transform);
            var markRect = markNode.GetComponent<RectTransform>();
            Fill(markRect);
            markRect.offsetMin = new Vector2(ChromeInset, 0f);
            markRect.offsetMax = new Vector2(-ChromeInset, 0f);
            var markImage = markNode.AddComponent<Image>();
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
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            if (participantView)
            {
                markImage.raycastTarget = true;
                header.AddComponent<FacilitatorHold>().Held = EndParticipantView;
            }
#endif
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
            stoneRow.spacing = 7;
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
                float size = here ? 22f : 14f; // was 18 / 11: legible at arm's length (I84)
                layout.minWidth = layout.preferredWidth = size;
                layout.minHeight = layout.preferredHeight = size;
                var image = stone.AddComponent<Image>();
                // The stone just passed lights up once on arrival at the next stream.
                if (named && gentleMotion && settledRound != world.Round && i == world.Round - 1)
                    BloomPulse.Add(stone, 0.25f, 0.45f);
                image.sprite = here ? MosseloraArt.BoldRing() : MosseloraArt.Rounded(4);
                image.type = Image.Type.Simple;
                image.color = here ? Leaf : passed ? MosseloraTheme.WaterChannel : SeasonStone(i);
                image.raycastTarget = false;
            }
            if (!named) return;
            string name = GameLocalization.Current == "en" ? puzzle.Name : "";
            // "Rainwater · Wildflower sanctuary" reads as the stream's name beside
            // the painted landmark that roots it, as on the hub (I86); the rooted
            // tile's spoken rule still names the sanctuary.
            int split = name.IndexOf(" · ", StringComparison.Ordinal);
            int[] past = world.Choices;
            Sprite mark = split > 0 && past.Length > 0
                ? MosseloraArt.ChoiceIcon((VillageChoice)past[past.Length - 1]) : null;
            Transform nameRow = row.transform;
            if (mark != null)
            {
                nameRow = CenteredRowIn(row.transform, "Stream name row");
                nameRow.GetComponent<HorizontalLayoutGroup>().spacing = 6;
                var icon = Node("Rooted by your choice", nameRow);
                var iconLayout = icon.AddComponent<LayoutElement>();
                iconLayout.minWidth = iconLayout.preferredWidth = 26;
                iconLayout.minHeight = iconLayout.preferredHeight = 26;
                var iconImage = icon.AddComponent<Image>();
                iconImage.sprite = mark;
                iconImage.preserveAspect = true;
                iconImage.raycastTarget = false;
            }
            Text title = Label(nameRow, "Stream name",
                name.Length == 0 ? " " : mark != null ? name.Substring(0, split) : name, 14, 0, Moss,
                TextAnchor.MiddleCenter);
            Emphasize(title);
            // Spoken position along the creek, not shown: the stones show it.
            Text spoken = Label(row.transform, "Stream position",
                GameLocalization.Text("stream", world.Round + 1, PuzzleCatalog.Count),
                12, 0, new Color(0f, 0f, 0f, 0f), TextAnchor.MiddleCenter);
            spoken.GetComponent<LayoutElement>().ignoreLayout = true;
        }

        // Stones ahead wear the season they will be walked in (summer sage,
        // autumn amber, winter frost): the year ahead is visible, never timed.
        private static Color SeasonStone(int stream)
        {
            int phase = stream >= 9 ? 3 : stream >= 6 ? 2 : stream >= 3 ? 1 : 0;
            switch (PaintedVillage.SeasonFor(phase, false))
            {
                // Opaque season tones, each at least 3:1 against the paper
                // (WCAG 1.4.11); the old translucent stones measured about 1.6:1 (I84).
                case 1: return new Color(0.72f, 0.47f, 0.20f, 1f);   // autumn 3.3:1
                case 2: return new Color(0.36f, 0.52f, 0.66f, 1f);   // winter 3.5:1
                default: return new Color(0.40f, 0.55f, 0.35f, 1f);  // summer 3.4:1
            }
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
                return Mathf.Max(225f, content.rect.width - 34f - 2f * columnInset);
            return Mathf.Max(225f, Screen.safeArea.width / LiveCanvasScale() - 34f - 2f * columnInset);
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
            decisionHeading = null;
            solvedBoard = null;
            screenHeading = null;
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
                if (!renderedMiniature)
                {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                    if (diorama != null) diorama.Hide();
#endif
                    var painting = illustration.AddComponent<PaintedVillage>();
                    painting.Build(17f, PaintedVillage.SeasonFor(scenePlan.LandscapePhase, world.Completed));
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
                // Puzzle pages show only play and the place; the story lines live
                // on the hub, the memory screen and the ending.
                if (!firstPuzzle && !memoryScreen && !solvedBoard && !puzzleScreen)
                    Label(content, "World consequence", LocalizedVillage.Consequence(world), 16, 52, Ink,
                        TextAnchor.MiddleCenter);
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
                moment.GetComponent<VerticalLayoutGroup>().padding = new RectOffset(14, 14, 34, 16);
                // The moment itself, painted: the neighbor doing what you chose
                // (Codex vignette), then one remembered sentence and what changed.
                int[] madeChoices = world.Choices;
                int keeper = madeChoices[madeChoices.Length - 1];
                Sprite vignette = MosseloraArt.Load(keeper == 0 ? MosseloraArt.VignetteTree
                    : keeper == 1 ? MosseloraArt.VignetteBridge : MosseloraArt.VignetteFlowers);
                Sprite rememberer = MosseloraArt.Portrait(keeper);
                if (vignette != null)
                {
                    var scene = Node("Memory vignette", moment.transform);
                    // Larger text keeps its size; the painting yields height so the
                    // continue arrow stays on the first screen (I85).
                    Height(scene, Mathf.Round(Mathf.Min(PageWidth() - 28f, 360f) / MosseloraArt.Aspect(vignette)
                        * (TextScale.Percent > 100 ? 0.78f : 1f)));
                    var sceneImage = scene.AddComponent<Image>();
                    sceneImage.sprite = vignette;
                    sceneImage.preserveAspect = true;
                    sceneImage.raycastTarget = false;
                }
                else if (rememberer != null)
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
                // The chapter header lives in the scrapbook; here, the memory and its effect.
                Text story = Label(moment.transform, "Story", LocalizedVillage.Memory(world), 18, 0, Ink,
                    TextAnchor.MiddleCenter);
                Emphasize(story);
                Label(moment.transform, "Story consequence", LocalizedVillage.Consequence(world), 16, 0, Ink,
                    TextAnchor.MiddleCenter);
                Sprite memorySprig = MosseloraArt.Load(MosseloraArt.Sprig);
                MosseloraArt.Ornament(moment.transform, "Sprig left", memorySprig, new Vector2(0f, 1f),
                    new Vector2(0f, 1f), new Vector2(64f, 22f), new Vector2(10f, -8f));
                MosseloraArt.Ornament(moment.transform, "Sprig right", memorySprig, new Vector2(1f, 1f),
                    new Vector2(0f, 1f), new Vector2(64f, 22f), new Vector2(-10f, -8f), true);
                // Where the arrow leads: the stones walked so far and the next
                // stream, wearing the landmark just planted (as on the hub, I78).
                if (!world.Completed)
                {
                    StreamStones(world.CurrentPuzzle, content, false);
                    if (GameLocalization.Current == "en")
                        NextStreamLine(content, world.CurrentPuzzle.Name, false);
                }
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
                ShowChoiceToChange(changeButton, (VillageChoice)world.Choices[world.Choices.Length - 1]);
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
            content = page;
            BuildChrome(living);
            UpdateCreekSound();
            // A different screen (not a tile turn) crossfades in like a turned page.
            string screenKey = (memoryScreen ? "memory" : world.Completed ? "ending" : "puzzle") + ":" +
                world.Round + ":" + world.Solved + ":" + showJournal + ":" + speakingWith + ":" + languagePicker;
            if (screenKey != lastScreenKey && lastScreenKey.Length > 0 && gentleMotion) StartCoroutine(FadeInPage());
            lastScreenKey = screenKey;
            menuSheet.SetActive(menuOpen);
            journalSheet.SetActive(showJournal && !menuOpen);
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
            // The toggle is the book in the floating chrome (BuildChrome); the
            // scrapbook rises as its own sheet instead of lengthening the page.
            if (!showJournal) return;
            RectTransform pageRoot = content;
            content = journalContent;
            SheetClose(journalContent, "Close journal", () => { showJournal = false; Rebuild(); });
            content = pageRoot;
            var journal = Card(journalContent, "A living journal", MosseloraTheme.CardFill);
            Text journalTitle = Label(journal.transform, "Journal title",
                GameLocalization.Text("journal_title"), 15, 30, Leaf, TextAnchor.MiddleCenter);
            Emphasize(journalTitle);
            // A painted scrapbook: two pages to a row, each with the painting of
            // the choice, the neighbor who remembers it, and their written line.
            string[] entries = LocalizedVillage.Journal(world);
            int[] chosen = world.Choices;
            GameObject spread = null;
            // At larger text the pages stack one per row, as the choice cards do,
            // so a memory reads in two lines instead of five (I105).
            bool onePerRow = TextScale.Percent > 100;
            for (int i = 0; i < entries.Length; i++)
            {
                if (onePerRow || i % 2 == 0)
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
                // I102 tilted these pages like a scrapbook; a blind comparison found
                // no gain (slight lean to the flat grid), so I103 removed it.
                MosseloraArt.Lift(pageImage, 2f, 0.12f);
                var pageStack = page.AddComponent<VerticalLayoutGroup>();
                pageStack.padding = new RectOffset(8, 8, 10, 10);
                pageStack.spacing = 4;
                pageStack.childAlignment = TextAnchor.UpperCenter;
                pageStack.childControlWidth = true;
                pageStack.childControlHeight = true;
                pageStack.childForceExpandWidth = true;
                pageStack.childForceExpandHeight = false;
                // Which stream this memory came from, with its season stone: two
                // bridges on two streams read as two memories, not a repeat (I88).
                Transform when = CenteredRowIn(page.transform, "Journal stream");
                when.GetComponent<HorizontalLayoutGroup>().spacing = 5;
                var stone = Node("Season stone", when);
                var stoneLayout = stone.AddComponent<LayoutElement>();
                stoneLayout.minWidth = stoneLayout.preferredWidth = 12;
                stoneLayout.minHeight = stoneLayout.preferredHeight = 12;
                var stoneImage = stone.AddComponent<Image>();
                stoneImage.sprite = MosseloraArt.Rounded(4);
                stoneImage.color = SeasonStone(i);
                stoneImage.raycastTarget = false;
                Label(when, "Journal stream name", GameLocalization.Text("stream", i + 1, PuzzleCatalog.Count),
                    13, 0, Moss, TextAnchor.MiddleCenter);
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
                // One remembered sentence on the page; the full chapter record is spoken.
                string remembered = i < chosen.Length ? LocalizedVillage.MemoryAt(chosen, i) : entries[i];
                // The journal is read, not glanced at: body size, as on the memory card (I98).
                Label(page.transform, "Remembered line", remembered, 17, 0, Ink, TextAnchor.UpperCenter);
                Text record = Label(page.transform, "Recorded cause and effect", entries[i], 14, 0,
                    new Color(0f, 0f, 0f, 0f), TextAnchor.UpperCenter);
                record.GetComponent<LayoutElement>().ignoreLayout = true;
            }
            if (!onePerRow && entries.Length % 2 == 1 && spread != null)
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
            // Spoken, not shown: the framed tile carries the painted landmark that
            // roots it, and tapping it shows the rule in a toast (TapTile).
            if (puzzle.AnchoredTile >= 0)
            {
                Text rule = Label(content, "Protected habitat", LocalizedVillage.EnvironmentalRule(world),
                    14, 0, new Color(0f, 0f, 0f, 0f), TextAnchor.MiddleCenter);
                rule.GetComponent<LayoutElement>().ignoreLayout = true;
            }
            // The board sits on the page itself; each tile is its own parchment square.
            var board = Card(content, "The water puzzle", new Color(1f, 1f, 1f, 0f));
            solvedBoard = board.GetComponent<RectTransform>();
            // The tiles sit in a painted garden bed of mossy stones (Codex art).
            Sprite bed = MosseloraArt.Load(MosseloraArt.BoardBed);
            if (bed != null)
            {
                var bedNode = Node("Garden bed", board.transform);
                bedNode.AddComponent<LayoutElement>().ignoreLayout = true;
                var bedRect = bedNode.GetComponent<RectTransform>();
                Fill(bedRect);
                bedRect.offsetMin = new Vector2(-10f, -10f);
                bedRect.offsetMax = new Vector2(10f, 10f);
                var bedImage = bedNode.AddComponent<Image>();
                bedImage.sprite = bed;
                bedImage.raycastTarget = false;
                bedNode.transform.SetAsFirstSibling();
            }
            var gridNode = Node("Water connections", board.transform);
            // Keep the entire puzzle inside a compact phone, including the
            // story card's horizontal padding and safe-area insets.
            int gap = puzzle.Size == 3 ? 7 : 5;
            // CanvasScaler.scaleFactor is only the constant-pixel field (always 1
            // here); the live ScaleWithScreenSize factor gives true logical width.
            float layoutWidth = Screen.safeArea.width / LiveCanvasScale() - 2f * columnInset;
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
                // A new stream's tiles settle onto the bed one by one (first view only).
                bool settle = gentleMotion && !world.Solved && settledRound != world.Round;
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                if (qaInMemoryOnly) settle = false; // tour screenshots need settled boards
#endif
                if (settle) SettleIn.Add(tileNode, 0.03f + index * 0.035f);
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
                    // The thing that roots it, painted, over the badge: tree, bridge or flowers.
                    Sprite holder = MosseloraArt.ChoiceIcon((VillageChoice)(Mathf.Clamp(world.BoardVariant, 1, 3) - 1));
                    if (holder != null)
                        MosseloraArt.Ornament(landmarkNode.transform, "Rooted by", holder, new Vector2(0f, 1f),
                            new Vector2(0f, 1f), new Vector2(0.46f, 0.46f) * tileSize, new Vector2(2f, -2f));
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
                // Hint step 1: soft honey rings along the row to look at; the
                // tiles keep their water colours (I91).
                if (world.HintStep == 1 && world.HintTile >= 0 && index / puzzle.Size == world.HintTile / puzzle.Size)
                {
                    var rowNode = Node("Hint row marker", tileNode.transform);
                    rowNode.AddComponent<LayoutElement>().ignoreLayout = true;
                    RectTransform rowRect = rowNode.GetComponent<RectTransform>();
                    Fill(rowRect);
                    rowRect.offsetMin = new Vector2(-3f, -3f);
                    rowRect.offsetMax = new Vector2(3f, 3f);
                    var rowRing = rowNode.AddComponent<Image>();
                    rowRing.sprite = MosseloraArt.Ring(15);
                    rowRing.type = Image.Type.Sliced;
                    rowRing.pixelsPerUnitMultiplier = 1.25f; // still thinner than the step-2 ring
                    rowRing.color = new Color(0.89f, 0.63f, 0.18f, 1f); // a cold-read missed the 75% rings (I92)
                    rowRing.raycastTarget = false;
                    hintMarkers.Add(rowNode);
                }
                if (index == hintTurnedTile && tileArtwork != null && index < tileArtwork.Length && tileArtwork[index] != null)
                {
                    if (gentleMotion) BloomPulse.Add(tileArtwork[index].gameObject, 0.05f, 0.22f);
                    hintTurnedTile = -1;
                }
                if (world.HintStep == 2 && index == world.HintTile)
                {
                    // A soft honey glow around the tile to turn: it breathes when
                    // gentle motion is on, and holds still when it is off.
                    var clueNode = Node("Hint focus marker", tileNode.transform);
                    hintMarkers.Add(clueNode);
                    clueNode.AddComponent<LayoutElement>().ignoreLayout = true;
                    RectTransform clueRect = clueNode.GetComponent<RectTransform>();
                    Fill(clueRect);
                    clueRect.offsetMin = new Vector2(-5f, -5f);
                    clueRect.offsetMax = new Vector2(5f, 5f);
                    var glow = clueNode.AddComponent<Image>();
                    glow.sprite = MosseloraArt.Ring(15);
                    glow.type = Image.Type.Sliced;
                    glow.pixelsPerUnitMultiplier = 0.9f; // bolder than the step-1 row rings
                    glow.color = new Color(0.89f, 0.63f, 0.18f, 1f);
                    glow.raycastTarget = false;
                    if (gentleMotion) Breathe.Add(glow);
                }
                bool hintedHere = world.HintStep > 0 && world.HintTile >= 0
                    && index / puzzle.Size == world.HintTile / puzzle.Size
                    && (world.HintStep == 1 || index == world.HintTile);
                // A hint ring and the teal goal glow never share a tile: the hint wins (I91).
                if (!hintedHere && puzzle.Kinds[index] == PipeKind.End && world.Round == 0 && !world.CanUndo && !world.Solved)
                {
                    // Before the first move, the start and the goal glow softly.
                    var goal = Node("Start or goal glow", tileNode.transform);
                    goalGlows.Add(goal);
                    goal.AddComponent<LayoutElement>().ignoreLayout = true;
                    var goalRect = goal.GetComponent<RectTransform>();
                    Fill(goalRect);
                    goalRect.offsetMin = new Vector2(-4f, -4f);
                    goalRect.offsetMax = new Vector2(4f, 4f);
                    var ring = goal.AddComponent<Image>();
                    ring.sprite = MosseloraArt.Ring(15);
                    ring.type = Image.Type.Sliced;
                    ring.pixelsPerUnitMultiplier = 1.2f;
                    ring.color = MosseloraTheme.WaterChannel;
                    ring.raycastTarget = false;
                    if (gentleMotion) Breathe.Add(ring);
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
            settledRound = world.Round;
            celebrateSolve = false;
            string feedback = world.Solved
                ? GameLocalization.Text("solved")
                : GameLocalization.Text("progress", insight.ConnectedCount, puzzle.CellCount, insight.LeakCount);
            WaterMeter(puzzle.CellCount, world.Solved ? puzzle.CellCount : insight.ConnectedCount);
            lastFlowing = (bool[])insight.Flowing.Clone();
            // Spoken, not shown: the droplets and the open mouths already show
            // reach and leaks, and the protected-habitat line explains landmarks.
            Transform meterRow = waterDrops != null ? content.GetChild(content.childCount - 1) : content;
            feedbackText = Label(meterRow, "Water feedback", feedback, 17, 0, new Color(0f, 0f, 0f, 0f),
                TextAnchor.MiddleCenter);
            feedbackText.GetComponent<LayoutElement>().ignoreLayout = true;
            Fill(feedbackText.rectTransform);

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
                    // With larger text they stack as wide cards so no word breaks.
                    bool stacked = TextScale.Percent > 100;
                    GameObject choiceRow;
                    if (stacked)
                    {
                        choiceRow = Node("Village choices", content);
                        var column = choiceRow.AddComponent<VerticalLayoutGroup>();
                        column.spacing = 10;
                        column.childControlWidth = true;
                        column.childControlHeight = true;
                        column.childForceExpandWidth = true;
                        column.childForceExpandHeight = false;
                    }
                    else
                    {
                        choiceRow = Row(content, "Village choices");
                        var choiceGroup = choiceRow.GetComponent<HorizontalLayoutGroup>();
                        choiceGroup.childForceExpandHeight = true;
                        choiceGroup.childAlignment = TextAnchor.UpperCenter;
                    }
                    for (int choiceIndex = 0; choiceIndex < 3; choiceIndex++)
                    {
                        VillageChoice choice = (VillageChoice)choiceIndex;
                        // None is visually preselected. A golden default would
                        // bias research toward the first habitat, not player care.
                        ChoiceCard(choiceRow.transform, choice, () => MakeChoice(choice), stacked);
                    }
                }
                // Second thoughts live after the decision, not between the payoff
                // line and the choices, where a lone back-arrow read as "go back".
                // Same bottom-left slot as the board's undo, so it never moves (I79).
                Transform revisitRow = CenteredRow("Revisit row");
                revisitRow.GetComponent<HorizontalLayoutGroup>().childAlignment = TextAnchor.MiddleLeft;
                Button revisit = IconButton(revisitRow, "Revisit water",
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
                // Icon-only in every stream: Mira's coach bubble teaches the board,
                // and every tool names itself on press-and-hold (and is spoken).
                bool teach = false;
                int toolHeight = 64;
                undoButton = IconButton(tools.transform, "Undo", GameLocalization.Text("undo"),
                    MosseloraArt.IconUndo, () => Act(() => world.Undo(), "undo"),
                    toolHeight, MosseloraTheme.ForestAction, false, null, teach);
                undoButton.interactable = world.CanUndo;
                // Hint depth shows as a small 1/2 or 2/2 badge, not words.
                Button hintButton = IconButton(tools.transform, "Hint",
                    GameLocalization.Text(world.HintStep == 2 ? "hint_turn" :
                        world.HintStep == 1 ? "hint_more" : "hint"),
                    MosseloraArt.IconHint, UseGentleHint, toolHeight, MosseloraTheme.WaterAction, false,
                    null, teach, world.HintStep > 0 ? 2 : 0, world.HintStep);
                Transform hintDots = hintButton.transform.Find("Level dots");
                if (hintDots != null) hintMarkers.Add(hintDots.gameObject); // a move ends the hint: dots go too
                IconButton(tools.transform, "Restart", GameLocalization.Text("restart"),
                    MosseloraArt.IconRestart, () => Act(() => world.Restart(), "restart"),
                    toolHeight, MosseloraTheme.QuietAction, false, null, teach);
                if (world.HintStep > 0 && world.HintTile >= 0)
                {
                    int hintRow = world.HintTile / puzzle.Size + 1;
                    int hintColumn = world.HintTile % puzzle.Size + 1;
                    // Spoken, not shown: the honey row tint and the breathing tile
                    // glow already point at the place to look.
                    Text clue = Label(content, "Optional progressive clue",
                        world.HintStep == 1 ?
                            GameLocalization.Text("hint_notice", hintRow) :
                            GameLocalization.Text("hint_focus", hintRow, hintColumn),
                        14, 0, new Color(0f, 0f, 0f, 0f), TextAnchor.MiddleCenter);
                    clue.GetComponent<LayoutElement>().ignoreLayout = true;
                }
            }
            // The coach bubble teaches; every tile's spoken hint still carries "how".
        }

        // Painted glyphs sit in cream discs; UI glyphs are drawn white on the bubble.
        private static void CoachGlyph(Transform parent, Sprite sprite, bool painted)
        {
            if (sprite == null) return;
            var holder = new GameObject(painted ? "Coach disc" : "Coach mark", typeof(RectTransform));
            holder.transform.SetParent(parent, false);
            var layout = holder.AddComponent<LayoutElement>();
            layout.minWidth = layout.preferredWidth = painted ? 46 : 30;
            layout.minHeight = layout.preferredHeight = painted ? 46 : 30;
            if (painted)
            {
                var disc = holder.AddComponent<Image>();
                MosseloraArt.Round(disc, MosseloraTheme.SagePanel, 23);
                disc.raycastTarget = false;
            }
            var mark = new GameObject("Glyph", typeof(RectTransform));
            mark.transform.SetParent(holder.transform, false);
            var rect = mark.GetComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = new Vector2(7f, 7f) * (painted ? 1f : 0f);
            rect.offsetMax = new Vector2(-7f, -7f) * (painted ? 1f : 0f);
            var image = mark.AddComponent<Image>();
            image.sprite = sprite;
            image.preserveAspect = true;
            image.raycastTarget = false;
            if (!painted) image.color = MosseloraTheme.WaterChannel; // ink-blue marks on parchment
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
            rowGroup.spacing = 8;
            rowGroup.childForceExpandWidth = false;
            rowGroup.childAlignment = TextAnchor.LowerCenter;
            Sprite guide = MosseloraArt.Portrait(0); // Mira welcomes newcomers to the creek
            if (guide != null)
            {
                var face = Node("Coach portrait", row.transform);
                var faceLayout = face.AddComponent<LayoutElement>();
                faceLayout.minWidth = faceLayout.preferredWidth = 54;
                faceLayout.minHeight = faceLayout.preferredHeight = 54;
                var faceImage = face.AddComponent<Image>();
                faceImage.sprite = guide;
                faceImage.preserveAspect = true;
                faceImage.raycastTarget = false;
                MosseloraArt.Lift(faceImage, 2f, 0.2f);
            }
            var bubble = Node("Coach bubble", row.transform);
            bubble.AddComponent<LayoutElement>().flexibleWidth = 1f;
            // Mira's note on painted parchment, like the choice cards, rather
            // than a dark UI panel (I77): part of the garden, not a tooltip.
            var fill = bubble.AddComponent<Image>();
            MosseloraArt.PaperTile(fill, 18);
            fill.color = MosseloraTheme.CardFill;
            fill.raycastTarget = false;
            MosseloraArt.Lift(fill, 3f, 0.18f);
            MosseloraArt.Edge(bubble.transform, MosseloraTheme.CardEdge, 18);
            var pad = bubble.AddComponent<VerticalLayoutGroup>();
            pad.padding = new RectOffset(16, 16, 12, 18); // last line clears the rim (I79)
            pad.childControlWidth = true;
            pad.childControlHeight = true;
            pad.childForceExpandWidth = true;
            // A picture, not a sentence: spring waves → garden sprout, and a
            // tapping hand. The sentence stays as spoken text for screen readers.
            // At larger text the sentence alone carries the rule, so the
            // picture row steps aside and the board's tools stay on screen (I85).
            bool showPicture = TextScale.Percent <= 100;
            var picture = Node("Coach picture", bubble.transform);
            picture.SetActive(showPicture);
            var pictureRow = picture.AddComponent<HorizontalLayoutGroup>();
            pictureRow.spacing = 10;
            pictureRow.childAlignment = TextAnchor.MiddleCenter;
            pictureRow.childControlWidth = true;
            pictureRow.childControlHeight = true;
            pictureRow.childForceExpandWidth = false;
            pictureRow.childForceExpandHeight = false;
            CoachGlyph(picture.transform, MosseloraArt.Load(MosseloraArt.SpringWaves), true);
            CoachGlyph(picture.transform, MosseloraArt.Load(MosseloraArt.IconContinue), false);
            CoachGlyph(picture.transform, MosseloraArt.Load(MosseloraArt.Sprout), true);
            var gap = Node("Coach gap", picture.transform);
            gap.AddComponent<LayoutElement>().minWidth = 6;
            CoachGlyph(picture.transform, MosseloraArt.Load(MosseloraArt.IconTap), false);
            // One plain sentence under the pictures: a first-time proxy walkthrough
            // could not tell from pictures alone that a tap turns a pipe (I64).
            Text words = Label(bubble.transform, "Coach words", message, 16, 0, Ink, TextAnchor.MiddleCenter);
            Emphasize(words);
            var tail = Node("Coach tail", bubble.transform);
            tail.AddComponent<LayoutElement>().ignoreLayout = true;
            var tailRect = tail.GetComponent<RectTransform>();
            tailRect.anchorMin = tailRect.anchorMax = new Vector2(0.5f, 0f);
            tailRect.sizeDelta = new Vector2(16f, 16f);
            tailRect.anchoredPosition = new Vector2(0f, -2f);
            tailRect.localRotation = Quaternion.Euler(0f, 0f, 45f);
            var tailImage = tail.AddComponent<Image>();
            tailImage.color = MosseloraTheme.CardFill;
            tailImage.raycastTarget = false;
            tail.transform.SetAsFirstSibling(); // under the bubble body
            coachBubble = row;
        }

        // Illustrated choice card (painted icon + title + written consequence).
        // One Button per card, so taps, focus and screen-reader text are unchanged.
        private void ChoiceCard(Transform parent, VillageChoice choice, Action action, bool wide = false)
        {
            var node = Node("Choice " + choice, parent);
            MinHeight(node, wide ? 96 : TextScale.Scale(150));
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
            HorizontalOrVerticalLayoutGroup stack = wide
                ? (HorizontalOrVerticalLayoutGroup)node.AddComponent<HorizontalLayoutGroup>()
                : node.AddComponent<VerticalLayoutGroup>();
            stack.padding = wide ? new RectOffset(10, 14, 10, 10) : new RectOffset(8, 8, 12, 12);
            stack.spacing = wide ? 12 : 6;
            stack.childAlignment = wide ? TextAnchor.MiddleLeft : TextAnchor.UpperCenter;
            stack.childControlWidth = true;
            stack.childControlHeight = true;
            stack.childForceExpandWidth = !wide;
            stack.childForceExpandHeight = false;
            Transform words = node.transform;
            Sprite icon = MosseloraArt.ChoiceIcon(choice);
            if (icon != null)
            {
                var picture = Node("Choice picture", node.transform);
                var pictureLayout = picture.AddComponent<LayoutElement>();
                pictureLayout.minHeight = pictureLayout.preferredHeight = 84;
                if (wide) pictureLayout.minWidth = pictureLayout.preferredWidth = 92;
                var pictureImage = picture.AddComponent<Image>();
                pictureImage.sprite = icon;
                pictureImage.preserveAspect = true;
                pictureImage.raycastTarget = false;
                // Who will remember this: the neighbor who cares for it.
                Sprite keeper = MosseloraArt.Portrait((int)choice);
                if (keeper != null)
                    MosseloraArt.Ornament(picture.transform, "Who will remember", keeper, new Vector2(1f, 0f),
                        new Vector2(1f, 0f), new Vector2(34f, 34f), new Vector2(4f, -6f));
            }
            if (wide)
            {
                var column = Node("Choice words", node.transform);
                column.AddComponent<LayoutElement>().flexibleWidth = 1;
                var columnGroup = column.AddComponent<VerticalLayoutGroup>();
                columnGroup.spacing = 4;
                columnGroup.childControlWidth = true;
                columnGroup.childControlHeight = true;
                columnGroup.childForceExpandWidth = true;
                columnGroup.childForceExpandHeight = false;
                words = column.transform;
            }
            TextAnchor align = wide ? TextAnchor.MiddleLeft : TextAnchor.UpperCenter;
            Text title = Label(words, "Choice name", LocalizedVillage.Choice(choice), 16, 0, Ink, align);
            Emphasize(title);
            // Picture first: the painting, the keeper and the name say it; the
            // consequence is spoken (and shown on the memory screen that follows).
            // A hidden "Caption" gives screen readers the full choice.
            Text spoken = Label(node.transform, "Caption",
                LocalizedVillage.Choice(choice) + "\n" + LocalizedVillage.ChoiceOutcome(choice, world),
                14, 0, new Color(0f, 0f, 0f, 0f), align);
            spoken.GetComponent<LayoutElement>().ignoreLayout = true;
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
            Button chimeTile = IconButton(row.transform, "Quiet audio setting",
                GameLocalization.Text(soundOn ? "sound_on" : "sound_off"),
                soundOn ? MosseloraArt.IconChimeOn : MosseloraArt.IconChimeOff,
                () => {
                    soundOn = !soundOn;
                    PlayerPrefs.SetInt("evergrove:gentle-audio", soundOn ? 1 : 0);
                    PlayerPrefs.Save();
                    Rebuild();
                }, 78, soundOn ? MosseloraTheme.ForestAction : MosseloraTheme.QuietAction,
                false, null, true);
            // The tile shows its name like its neighbors ("Gentle chime"); the icon
            // and tone show on/off. The full sentence stays the spoken name (I76).
            ShowShortName(chimeTile);
        }

        // Shows the part of a "Name: state (hint)" caption before the colon, and
        // keeps the whole sentence as the hidden spoken "Caption".
        private static void ShowShortName(Button tile)
        {
            Transform shown = tile.transform.Find("Caption");
            Text text = shown != null ? shown.GetComponent<Text>() : null;
            if (text == null) return;
            string full = text.text;
            int colon = full.IndexOfAny(new[] { ':', '\uFF1A' });
            if (colon <= 0) return;
            shown.name = "Shown name";
            text.text = full.Substring(0, colon).Trim();
            var spokenNode = new GameObject("Caption", typeof(RectTransform));
            spokenNode.transform.SetParent(tile.transform, false);
            spokenNode.AddComponent<LayoutElement>().ignoreLayout = true;
            var spoken = spokenNode.AddComponent<Text>();
            spoken.font = text.font;
            spoken.fontSize = text.fontSize;
            spoken.text = full;
            spoken.color = new Color(0f, 0f, 0f, 0f);
            spoken.raycastTarget = false;
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

        // A neighbor "has news" when what they would say now differs from the
        // last line the player heard from them (I80). Lines change only when the
        // village does (a landmark, a season), never with the clock.
        private const string HeardKey = "evergrove:heard-";

        private bool HasNews(int villager)
        {
            if (world.Mode != PlayMode.Living) return false;
            return LocalizedVillage.Speak(world, villager) != PlayerPrefs.GetString(HeardKey + villager, "");
        }

#if UNITY_EDITOR || DEVELOPMENT_BUILD
        // Research-only controls compile only in the Editor or explicitly
        // opted-in Development Builds, never in ordinary player builds.
        // Hidden in the M3 participant view so the study does not reveal its hypothesis.
        private void BuildResearchControls(bool living)
        {
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
        }

        // Local playtest report and return diary (facilitator-only, opt-in).
        private void BuildLocalStudyControls()
        {
            if (localPlaytest == null)
            {
                Button(content, "Playtest consent", GameLocalization.Text("record"),
                    StartParticipantView, 44);
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
            // Return diary: explicit two-step consent; dates only; copy or delete in one action.
            Label(content, "Return diary", returnDiary.Consented
                ? returnDiary.Days.Count + " local dates recorded on this device"
                : "not recording", 12, 43, Moss, TextAnchor.MiddleCenter);
            if (!returnDiary.Consented && !returnDiaryAsked)
            {
                Button(content, "Return diary consent", "Start return diary",
                    () => { returnDiaryAsked = true; Rebuild(); }, 44);
            }
            else if (!returnDiary.Consented)
            {
                Label(content, "Consent",
                    "Records only the local dates this app is opened on this device. Nothing is uploaded. Delete any time.",
                    12, 43, Moss, TextAnchor.MiddleCenter);
                Button(content, "Agree and record dates", "Agree and record dates",
                    () => {
                        returnDiary.GrantConsent(DateTime.Now.Date);
                        SaveReturnDiary();
                        returnDiaryAsked = false;
                        Rebuild();
                    }, 44);
                Button(content, "Not now", "Not now",
                    () => { returnDiaryAsked = false; Rebuild(); }, 43);
            }
            else
            {
                Button(content, "Copy return diary", "Copy return diary",
                    () => {
                        GUIUtility.systemCopyBuffer = returnDiary.Export(DateTime.Now.Date);
                        returnDiaryCopied = true;
                        Rebuild();
                    }, 48);
                if (returnDiaryCopied)
                    Label(content, "Copied privately", "Copied privately",
                        12, 43, Moss, TextAnchor.MiddleCenter);
                Button(content, "Delete return diary", "Delete return diary",
                    () => { ClearReturnDiary(); Rebuild(); }, 43);
            }
        }

        // M3 participant view: starting the local report hides every research
        // and developer control and returns to the top of the page. Holding the
        // wordmark for FacilitatorHold.HoldSeconds brings the controls back.
        private void StartParticipantView()
        {
            localPlaytest = new LocalPlaytest(world.Mode, showMiniature, world.Round, Time.unscaledTime);
            playtestCopied = false;
            participantView = true;
            Rebuild(true);
        }

        private void EndParticipantView()
        {
            participantView = false;
            Rebuild();
        }
#endif

        private void OpenConversation(int villagerIndex)
        {
            speakingWith = villagerIndex;
            if (world.Mode == PlayMode.Living)
            {
                PlayerPrefs.SetString(HeardKey + villagerIndex, LocalizedVillage.Speak(world, villagerIndex));
                PlayerPrefs.Save();
            }
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
            if (goalGlows.Count > 0 && world.CanUndo)
            {
                foreach (GameObject glow in goalGlows) if (glow != null) glow.SetActive(false);
                goalGlows.Clear();
            }
            if (world.Completed || visibleTiles == null) return;
            // A move ends the hint; its rings go with it instead of lingering (I91).
            if (hintMarkers.Count > 0 && world.HintStep == 0)
            {
                foreach (GameObject marker in hintMarkers) if (marker != null) marker.SetActive(false);
                hintMarkers.Clear();
            }
            Puzzle puzzle = world.CurrentPuzzle;
            if (visibleTiles.Length != puzzle.CellCount) { Rebuild(); return; }
            PipeInsight insight = world.Insight;
            int[] angles = world.Rotations;
            for (int i = 0; i < puzzle.CellCount; i++)
            {
                // Water just arrived here: the tile gives one small, soft swell.
                if (gentleMotion && lastFlowing != null && i < lastFlowing.Length &&
                    insight.Flowing[i] && !lastFlowing[i])
                    BloomPulse.Add(tileArtwork[i].gameObject, 0.04f); // the pipe, not the pressed tile
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
            lastFlowing = (bool[])insight.Flowing.Clone();
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
            int target = world.HintTile;
            int stage = world.AdvanceGentleHint();
            if (stage == 0) return;
            // Step 3 turns the tile for you; it swells once so the change is seen (I92).
            hintTurnedTile = stage == 3 ? target : -1;
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
                // A gentle "this one stays" shake of the rooted pipe: felt, not scolded.
                if (gentleMotion && tileArtwork != null && index < tileArtwork.Length)
                    tileArtwork[index].gameObject.AddComponent<Nudge>();
                if (visibleTiles != null && index < visibleTiles.Length && landmark.Length > 0)
                    RuleToast.Show(visibleTiles[index].GetComponent<RectTransform>(), landmark, font, Ink,
                        MosseloraTheme.CardFill);
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
            float stopAt = targetTop - context;
            // Better: the whole finished creek above all three choices, so the
            // board is never cut in half under the menu button (I74). Only when
            // both fit on this screen; larger text keeps the heading anchor.
            int cardsIndex = decisionHeading.GetSiblingIndex() + 1;
            if (solvedBoard != null && cardsIndex < content.childCount)
            {
                float boardTop = DepthInContent(solvedBoard, true) - 10f; // painted bed overhang
                float cardsBottom = DepthInContent((RectTransform)content.GetChild(cardsIndex), false);
                float boardFirst = boardTop - ChromeClearance() - 6f;
                if (cardsBottom - boardFirst <= scroll.viewport.rect.height - 8f)
                    stopAt = boardFirst;
            }
            float target = 1f - Mathf.Clamp01(stopAt / distance);
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

        // Distance from the top of the page to a node's top (or bottom) edge.
        private float DepthInContent(RectTransform node, bool top)
        {
            Vector3 edge = node.TransformPoint(new Vector3(node.rect.center.x,
                top ? node.rect.yMax : node.rect.yMin, 0f));
            return content.rect.yMax - content.InverseTransformPoint(edge).y;
        }

        // How far the floating menu button reaches into the scrolling viewport.
        private float ChromeClearance()
        {
            if (chrome == null || chrome.childCount == 0 || scroll.viewport == null) return 0f;
            var menu = (chrome.Find("Open menu") ?? chrome.GetChild(0)) as RectTransform;
            if (menu == null) return 0f;
            Vector3 bottom = menu.TransformPoint(new Vector3(menu.rect.center.x, menu.rect.yMin, 0f));
            float reach = scroll.viewport.rect.yMax - scroll.viewport.InverseTransformPoint(bottom).y;
            return Mathf.Clamp(reach, 0f, 160f);
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
        // Spoiler-free text postcard for a friend (vision 4.4 rung 1). Clipboard only.
        private void BuildPostcard()
        {
            if (world.Mode != PlayMode.Living) return;
            var card = Card(content, "Garden postcard", MosseloraTheme.CardFill);
            Transform row = CenteredRowIn(card.transform, "Postcard row");
            Width(IconButton(row, "Copy garden postcard", GameLocalization.Text("postcard"),
                MosseloraArt.IconChat, () => {
                    GUIUtility.systemCopyBuffer = GardenPostcard.Build(world,
                        world.Completed ? "" : GameLocalization.Text("stream", world.Round + 1, PuzzleCatalog.Count));
                    postcardCopied = true;
                    Rebuild();
                }, 78, MosseloraTheme.WaterAction, false, null, true), 240);
            if (postcardCopied)
                Label(card.transform, "Postcard notice", GameLocalization.Text("postcard_copied"), 15, 0, Ink,
                    TextAnchor.MiddleCenter);
        }

        private void BuildGardenFileSection()
        {
            var card = Card(content, "Garden file", MosseloraTheme.CardFill);
            GameObject files = Row(card.transform, "Garden file actions");
            IconButton(files.transform, "Copy garden file", GameLocalization.Text("garden_copy"),
                MosseloraArt.IconCopyFile, CopyGardenFile, 78, MosseloraTheme.ForestAction,
                false, null, true);
            if (!WorldStore.RecoveryBlocked)
            {
                if (!gardenRestoreConfirm)
                    IconButton(files.transform, "Restore garden file", GameLocalization.Text("garden_restore"),
                        MosseloraArt.IconRestoreFile, ReadGardenFile, 78, MosseloraTheme.WaterAction,
                        false, null, true);
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

#if UNITY_EDITOR || DEVELOPMENT_BUILD
        private void RecordReturnDiaryOpen()
        {
            if (returnDiary.RecordOpen(DateTime.Now.Date)) SaveReturnDiary();
        }

        private void SaveReturnDiary()
        {
            PlayerPrefs.SetString(ReturnDiaryKey, returnDiary.Serialize());
            PlayerPrefs.Save();
        }

        private void ClearReturnDiary()
        {
            returnDiary.Delete();
            returnDiaryCopied = false;
            PlayerPrefs.DeleteKey(ReturnDiaryKey);
            PlayerPrefs.Save();
        }

        private void OnApplicationPause(bool paused)
        {
            if (!paused) RecordReturnDiaryOpen();
        }
#endif

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
