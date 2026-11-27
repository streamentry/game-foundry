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
    public sealed partial class EvergroveApp : MonoBehaviour
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
        private Font bookFont;    // optional book serif for the story's own voice (iOS, I117)
        private bool languagePicker;
        private RectTransform content;
        private RectTransform scrollArea;
        // Smart navigation: play stays on the page; everything else lives in a
        // menu sheet opened from one leaf button. The chrome (menu + journal
        // buttons) floats over the page and never scrolls away.
        private RectTransform canvasRoot;
        private RectTransform chrome;
        private RectTransform worldStage; // I189: the garden behind every board
        private bool worldShownNow;
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
        private float spentTipHeight;    // its space, kept for the rest of the first board (I141)
        private RectTransform warningViewport;
        private GameObject saveWarningBanner;
        private Text saveWarningCaption;
        private ScrollRect scroll;
        private CanvasScaler viewportScaler;
        private Rect lastSafeArea;
        private Vector2Int lastScreen;
        private bool showingMemory;
        private bool showJournal;
        private int journalOpenPage = -1; // a page opened full size in the book (I166)
        private int speakingWith = -1;
        // The neighbor's speech floats over the page (I127), so talking never
        // scrolls the board away or pushes the header off screen.
        private GameObject neighborLayer;
        private GameObject pinnedActions;
        private Image creekWater; // the flowing creek over the dry one, as full as the board (I167)
        private float creekShown; // the memory card's actions, held at the screen's foot (I164)
        private int neighborShown = -1;
        private bool confirmReset;
        private bool soundOn;
        // Which garden this is (0, 1, 2…): picks the catalogue's items (I172).
        private const string GardenNumberKey = "evergrove:garden-number";
        private int gardenNumber;
        // E2 (I174): every tree, flower and build ever made, across gardens.
        private const string AlmanacKey = "evergrove:almanac";
        private Almanac almanac = new Almanac();
        private bool almanacTab;
        private bool waysTab; // I403: the garden's ways (visitors, good neighbours, the year)
        private bool familyTab; // I269: the book's family page
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
        private RectTransform decisionCards;
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
            gardenNumber = Mathf.Max(0, PlayerPrefs.GetInt(GardenNumberKey, 0));
            almanac = Almanac.Parse(PlayerPrefs.GetString(AlmanacKey, ""));
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            // The QA tour plays garden 0 in memory: it never reads the player's almanac.
            if (qaInMemoryOnly) { gardenNumber = 0; almanac = new Almanac(); }
#endif
            RecordInAlmanac();
            NotesLifecycle(false); // a cold start is an opening too (I275)
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            if (!qaInMemoryOnly) GardenNotes.DevTest(world); // I284: see a real note delivered
#endif
            PaintedVillage.ItemSprite = (family, nth) =>
            {
                // The nth tree (or build, or flowers) in the village is the one
                // chosen at that family's nth choice.
                if (world == null) return null;
                int[] made = world.Choices;
                for (int i = 0, seen = 0; i < made.Length; i++)
                    if (made[i] == family && seen++ == nth) return ItemArt(i, family, false);
                return null;
            };
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
            bookFont = BookFont();
            LocaleRuntime.Initialize();
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            // Dev-only: hold the app sideways so landscape can be toured on any
            // simulator from the shell (SIMCTL_CHILD_mosseloraOrientation=landscape).
            if (DevArgs.Read("-mosseloraOrientation") == "landscape")
                Screen.orientation = ScreenOrientation.LandscapeLeft;
#endif
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
        // A book serif already on the device: iOS ships Charter and Georgia, Android
        // ships Noto Serif (I125). Never bundled; without one the story keeps the runtime face.
        private static readonly string[] BookFaces = { "Charter", "Georgia", "Noto Serif", "NotoSerif" };

        private static Font BookFont()
        {
            try
            {
                string[] installed = Font.GetOSInstalledFontNames();
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                Debug.Log("Mosselora OS serif candidates: " + string.Join(", ",
                    Array.FindAll(installed, n => n.IndexOf("serif", StringComparison.OrdinalIgnoreCase) >= 0
                        || n.StartsWith("Charter", StringComparison.OrdinalIgnoreCase)
                        || n.StartsWith("Georgia", StringComparison.OrdinalIgnoreCase))));
#endif
                foreach (string wanted in BookFaces)
                {
                    // The plain family first ("Charter"), so a heavier cut is never picked.
                    foreach (string name in installed)
                        if (string.Equals(name, wanted, StringComparison.OrdinalIgnoreCase))
                        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                            Debug.Log("Mosselora book face: " + name);
#endif
                            return Font.CreateDynamicFontFromOSFont(name, 18);
                        }
                    foreach (string name in installed)
                        if (name.StartsWith(wanted, StringComparison.OrdinalIgnoreCase)
                            && name.IndexOf("Sans", StringComparison.OrdinalIgnoreCase) < 0
                            && name.IndexOf("Italic", StringComparison.OrdinalIgnoreCase) < 0
                            && name.IndexOf("Bold", StringComparison.OrdinalIgnoreCase) < 0)
                        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                            Debug.Log("Mosselora book face: " + name);
#endif
                            return Font.CreateDynamicFontFromOSFont(name, 18);
                        }
                }
            }
            catch (Exception exception)
            {
                Debug.LogWarning("Mosselora book font lookup unavailable: " + exception.GetType().Name);
            }
            return null;
        }

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
        // The story's own voice (memories, what neighbours say, the ending) is set
        // like a book; labels and instructions keep the rounded sans (I117).
        // Upright roman throughout: bold serif ran wide and orphaned a word, and a
        // thin italic aside lost contrast on the paper for older eyes (I117).
        private void Narrate(Text text, bool lead = false)
        {
            if (bookFont != null && font == baseFont && GameLocalization.Current == "en")
            {
                text.font = bookFont;
                text.fontStyle = FontStyle.Normal;
            }
            else if (lead)
                Emphasize(text);
        }

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
            TickSoundscape();
            if (scrollArea == null) return;
            Rect safe = Screen.safeArea;
            var size = new Vector2Int(Screen.width, Screen.height);
            if (lastScreen != size || safe != lastSafeArea)
            {
                lastScreen = size;
                lastSafeArea = safe;
                FitSafeArea();
                if (landscape != lastLandscape)
                {
                    lastLandscape = landscape;
                    OnOrientationChanged();
                }
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
            // Upright, the page reaches the very top of the screen so the garden can
            // paint under the status bar; the inset becomes padding instead (I185).
            bool upright = safe.width <= safe.height;
            scrollArea.anchorMin = new Vector2(safe.xMin / Screen.width, safe.yMin / Screen.height);
            scrollArea.anchorMax = new Vector2(safe.xMax / Screen.width, upright ? 1f : safe.yMax / Screen.height);
            scrollArea.offsetMin = Vector2.zero;
            scrollArea.offsetMax = Vector2.zero;
            FitColumn();
        }

        // Tablets (I106): scale by height so the page keeps a phone's vertical
        // room, and keep the page a book-width column in the middle instead of
        // stretching every painting across a wide screen. Phones are unchanged.
        private const float TabletAspect = 0.62f;
        private const float MaxColumn = 480f;
        private float columnLeft, columnRight;
        // Landscape (I114): turning the phone opens the garden full height on the
        // left and keeps play in a book-width column on the right.
        private bool landscape, lastLandscape;
        // A sideways tablet: a wider play column and bigger tiles than a phone (I122).
        private bool landscapeTablet;
        private const float TabletLandscapeColumn = 560f;
        private RectTransform chromeBand;
        private float topInset; // status-bar height in canvas units, upright only (I185)
        private GameObject landscapeGarden;

        private void FitColumn()
        {
            if (viewportScaler == null || Screen.height <= 0) return;
            Rect safe = Screen.safeArea;
            if (safe.width <= 0f || safe.height <= 0f) return;
            float aspect = safe.width / safe.height;
            landscape = aspect > 1f;
            bool tablet = !landscape && aspect > TabletAspect;
            // A sideways phone keeps a phone's sizes: it scales to a 414-point-tall
            // reference instead of shrinking the portrait page to fit its height.
            bool landscapePhone = landscape && aspect >= 1.6f;
            landscapeTablet = landscape && !landscapePhone;
            viewportScaler.referenceResolution = landscapePhone ? new Vector2(896, 414) : new Vector2(414, 896);
            viewportScaler.matchWidthOrHeight = tablet || landscape ? 1f : 0.5f;
            float logicalWidth = safe.width / LiveCanvasScale();
            if (landscape)
            {
                float width = Mathf.Clamp(Mathf.Floor(logicalWidth * 0.5f), 380f,
                    landscapeTablet ? TabletLandscapeColumn : MaxColumn);
                columnLeft = Mathf.Max(0f, Mathf.Floor(logicalWidth - width));
                columnRight = 0f;
            }
            else
                columnLeft = columnRight = tablet ? Mathf.Max(0f, Mathf.Floor((logicalWidth - MaxColumn) / 2f)) : 0f;
            topInset = landscape ? 0f : Mathf.Max(0f, (Screen.height - safe.yMax) / LiveCanvasScale());
            var column = content != null ? content.GetComponent<VerticalLayoutGroup>() : null;
            // Sideways the menu and book sit on the painting (I116), so the play
            // column starts at the top: no inset spent on chrome it doesn't carry.
            if (column != null)
                column.padding = new RectOffset(17 + (int)columnLeft, 17 + (int)columnRight,
                    15 + Mathf.RoundToInt(topInset), 36);
            if (chrome != null) chrome.anchoredPosition = new Vector2(0f, -topInset);
            if (chromeBand != null)
            {
                chromeBand.offsetMin = new Vector2(columnLeft, -104f - topInset);
                // The band's paper covers the status bar too, then fades (I185).
                var solid = chromeBand.Find("Band solid") as RectTransform;
                if (solid != null) solid.sizeDelta = new Vector2(0f, 74f + topInset);
                var fade = chromeBand.Find("Band fade") as RectTransform;
                if (fade != null) fade.anchoredPosition = new Vector2(0f, -74f - topInset);
                chromeBand.offsetMax = Vector2.zero;
                chromeBand.gameObject.SetActive(!landscape && !worldShownNow);
            }
            FitSheet(menuSheet, "Menu panel");
            FitSheet(journalSheet, "Journal panel");
        }

        // Sheets stay a readable width when the phone is sideways (I114).
        private void FitSheet(GameObject sheet, string panelName)
        {
            Transform panel = sheet != null ? sheet.transform.Find(panelName) : null;
            if (panel == null) return;
            var rect = (RectTransform)panel;
            if (landscape)
            {
                rect.anchorMin = new Vector2(0.5f, 0f);
                rect.anchorMax = new Vector2(0.5f, 0.92f);
                rect.offsetMin = new Vector2(-260f, 0f);
                rect.offsetMax = new Vector2(260f, 0f);
            }
            else
            {
                rect.anchorMin = Vector2.zero;
                rect.anchorMax = new Vector2(1f, 0.86f);
                rect.offsetMin = new Vector2(8f, 0f);
                rect.offsetMax = new Vector2(-8f, 0f);
            }
        }

        // The phone turned: rebuild the hub (if open) and the page for the new shape.
        private void OnOrientationChanged()
        {
            if (titleScreen != null)
            {
                Transform host = titleScreen.transform.parent;
                titleScreen.SetActive(false);
                Destroy(titleScreen);
                titleScreen = null;
                BuildTitleScreen(host);
            }
            Rebuild();
        }

        // Landscape only: the season's garden, full height, left of the play column.
        private void LandscapeGarden(bool living)
        {
            if (landscapeGarden != null)
            {
                landscapeGarden.SetActive(false);
                Destroy(landscapeGarden);
                landscapeGarden = null;
            }
            if (!landscape || canvasRoot == null) return;
            Rect safe = Screen.safeArea;
            float split = (safe.xMin + columnLeft * LiveCanvasScale()) / Mathf.Max(1, Screen.width);
            landscapeGarden = Node("Landscape garden", canvasRoot);
            landscapeGarden.transform.SetSiblingIndex(1); // above the paper, under the page
            var rect = landscapeGarden.GetComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = new Vector2(split, 1f);
            rect.offsetMin = rect.offsetMax = Vector2.zero;
            int[] story = world.Choices;
            Sprite hero = world.Completed ? MosseloraArt.Load(MosseloraArt.EndingFestival) : null;
            if (hero == null && living && story.Length > 0)
            {
                int last = story[story.Length - 1];
                hero = MosseloraArt.Load(last == 0 ? MosseloraArt.VignetteTree
                    : last == 1 ? MosseloraArt.VignetteBridge : MosseloraArt.VignetteFlowers);
            }
            if (hero == null) hero = MosseloraArt.Load(MosseloraArt.HeroVillage);
            // Sideways the whole left side is your garden map, editable like upright (I183).
            if (living && LandscapeGardenMap(landscapeGarden.transform)) return;
            GardenScene(landscapeGarden.transform, hero, living, false, Vector2.one, new Vector2(1f, 0.36f));
        }

        private bool LandscapeGardenMap(Transform pane)
        {
            ApplyDayLight(world);
            Sprite plot = MapPainting();
            GardenMap map = LoadGardenMap();
            if (plot == null || map == null) return false;
            var window = Node("Garden window", pane);
            var windowRect = window.GetComponent<RectTransform>();
            Fill(windowRect);
            window.AddComponent<RectMask2D>();
            bool memoryScreen = showingMemory && world.Choices.Length > 0;
            bool animate = celebrateGrowth && memoryScreen && gentleMotion;
            GardenMapView.Build(windowRect, plot, map, MapItems(map), 1.15f,
                OnMapLand, OnMapItem, ItemInHand(), animate, GreenedShare(0),
                animate ? GreenedShare(1) : -1f,
                (land, point) => NeighborsOnMap(land, point, map, false), 0f, RiverGoesOn(map));
            int held = ItemInHand();
            if (held >= 0 && PlayerPrefs.GetInt(PlacedOnceKey, 0) == 0) PlaceTip(window.transform, held);
            var side = Node("Garden melt right", window.transform);
            var sideRect = side.GetComponent<RectTransform>();
            sideRect.anchorMin = new Vector2(1f, 0f);
            sideRect.anchorMax = new Vector2(1f, 1f);
            sideRect.pivot = new Vector2(1f, 0.5f);
            sideRect.sizeDelta = new Vector2(40f, 0f);
            var sideImage = side.AddComponent<Image>();
            sideImage.sprite = MosseloraArt.SideFade();
            sideImage.color = Paper;
            sideImage.raycastTarget = false;
            return true;
        }

        // Painting above, your village strip (real water, landmarks, neighbours) below,
        // melting into the paper at the bottom and on the right edge.
        private void GardenScene(Transform parent, Sprite hero, bool living, bool fromTitle,
                                 Vector2 windowMax, Vector2 stripMax)
        {
            var window = Node("Village window", parent);
            var windowRect = window.GetComponent<RectTransform>();
            windowRect.anchorMin = Vector2.zero;
            windowRect.anchorMax = windowMax;
            windowRect.offsetMin = windowRect.offsetMax = Vector2.zero;
            window.AddComponent<RectMask2D>();
            if (hero != null)
            {
                var art = Node("Garden painting", window.transform);
                Fill(art.GetComponent<RectTransform>());
                var image = art.AddComponent<Image>();
                image.sprite = hero;
                image.raycastTarget = false;
                var fit = art.AddComponent<AspectRatioFitter>();
                fit.aspectMode = AspectRatioFitter.AspectMode.EnvelopeParent;
                fit.aspectRatio = MosseloraArt.Aspect(hero);
            }
            var side = Node("Garden melt right", window.transform);
            var sideRect = side.GetComponent<RectTransform>();
            sideRect.anchorMin = new Vector2(1f, 0f);
            sideRect.anchorMax = new Vector2(1f, 1f);
            sideRect.pivot = new Vector2(1f, 0.5f);
            sideRect.sizeDelta = new Vector2(56f, 0f);
            var sideImage = side.AddComponent<Image>();
            sideImage.sprite = MosseloraArt.SideFade();
            sideImage.color = Paper;
            sideImage.raycastTarget = false;
            if (!living) return;
            var strip = Node("Your village", parent);
            var stripRect = strip.GetComponent<RectTransform>();
            stripRect.anchorMin = Vector2.zero;
            stripRect.anchorMax = stripMax;
            stripRect.offsetMin = stripRect.offsetMax = Vector2.zero;
            var painting = strip.AddComponent<PaintedVillage>();
            VillageScenePlan plan = VillageScenePlan.From(world);
            painting.Build(0f, PaintedVillage.ArtSeason(world));
            if (painting.Ready)
                painting.Show(plan.Chapter, plan.TreeCount, plan.CrossingCount, plan.FlowerCount,
                    plan.WaterRestored, plan.WaterReachPercent);
            NeighborsOnPainting(strip.transform, fromTitle);
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

            // The garden is the stage (I189): behind the page, full screen, your own map.
            var stageNode = Node("Living world", canvasNode.transform);
            worldStage = stageNode.GetComponent<RectTransform>();
            Fill(worldStage);
            stageNode.AddComponent<RectMask2D>();
            stageNode.SetActive(false);

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
            chromeBand = bandRect;
            var chromeNode = Node("Game chrome", scrollNode.transform);
            chrome = chromeNode.GetComponent<RectTransform>();
            chrome.anchorMin = new Vector2(0f, 1f);
            chrome.anchorMax = new Vector2(1f, 1f);
            chrome.pivot = new Vector2(0.5f, 1f);
            chrome.sizeDelta = new Vector2(0f, 70f);
            chrome.anchoredPosition = new Vector2(0f, -topInset);
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
                                  bool captionBelow = false, int pips = 0, int pipsOn = 0,
                                  bool round = false)
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
            Image plaque = round ? MosseloraArt.Medallion(image, tone) : MosseloraArt.Plaque(image, tone);
            if (plaque == null) MosseloraArt.Round(image, tone, round ? height / 2 : 18);
            if (round)
            {
                // A round token as wide as it is tall (I129).
                var roundLayout = node.GetComponent<LayoutElement>();
                if (roundLayout == null) roundLayout = node.AddComponent<LayoutElement>();
                roundLayout.minWidth = roundLayout.preferredWidth = height;
                roundLayout.flexibleWidth = 0f;
            }
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
            float iconSize = Mathf.Round(height * (captionBelow ? 0.40f : showCaption ? 0.42f : round ? 0.44f : 0.52f));
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
                if (round)
                {
                    // On a token the level sits under the glyph, inside the disc.
                    dots.AddComponent<LayoutElement>().ignoreLayout = true;
                    var dotsRect = dots.GetComponent<RectTransform>();
                    dotsRect.anchorMin = dotsRect.anchorMax = new Vector2(0.5f, 0f);
                    dotsRect.pivot = new Vector2(0.5f, 0f);
                    dotsRect.sizeDelta = new Vector2(height * 0.5f, 10f);
                    dotsRect.anchoredPosition = new Vector2(0f, height * 0.13f);
                }
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
            // Each choice stands on the painted stone of the stream it was made in,
            // in its season, so the summary reads as the path walked through the
            // year, not an inventory tally (I161).
            const float cellHeight = 64f;
            grid.cellSize = new Vector2(50f, cellHeight);
            grid.spacing = new Vector2(4f, 6f);
            grid.childAlignment = TextAnchor.MiddleCenter;
            grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            grid.constraintCount = Mathf.Min(6, chosen.Length);
            int rows = (chosen.Length + grid.constraintCount - 1) / grid.constraintCount;
            Height(strip, rows * cellHeight + (rows - 1) * 6f);
            for (int i = 0; i < chosen.Length; i++)
            {
                Sprite icon = ItemArt(i, chosen[i]);
                var cell = Node("Chapter choice", strip.transform);
                Sprite stone = PathStone(i, false, false);
                if (stone != null)
                {
                    var step = Node("Stone", cell.transform);
                    var stepRect = step.GetComponent<RectTransform>();
                    stepRect.anchorMin = new Vector2(0.08f, 0f);
                    stepRect.anchorMax = new Vector2(0.92f, 0.36f);
                    stepRect.offsetMin = stepRect.offsetMax = Vector2.zero;
                    var stepImage = step.AddComponent<Image>();
                    stepImage.sprite = stone;
                    stepImage.preserveAspect = true;
                    stepImage.raycastTarget = false;
                }
                if (icon != null)
                {
                    var picture = Node("Picture", cell.transform);
                    var pictureRect = picture.GetComponent<RectTransform>();
                    pictureRect.anchorMin = new Vector2(0f, stone != null ? 0.2f : 0f);
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
            Text line = Label(memory, name, Remembered(past, index), 14, 0, Ink, TextAnchor.MiddleLeft);
            Narrate(line);
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

        private void MeterEnd(Transform row, string art, float size, Color tint)
        {
            Sprite sprite = MosseloraArt.Load(art);
            if (sprite == null) return;
            var node = Node("Water meter " + (art == TreePlay.SeedIcon ? "garden" : "spring"), row);
            var layout = node.AddComponent<LayoutElement>();
            layout.minWidth = layout.preferredWidth = size;
            layout.minHeight = layout.preferredHeight = size;
            var image = node.AddComponent<Image>();
            image.sprite = sprite;
            image.preserveAspect = true;
            image.color = tint;
            image.raycastTarget = false;
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
            // I350 (run 3, P7): the row starts at the spring and ends at the sprout, the same
            // marks as the board, so the drops read as water reaching the garden.
            MeterEnd(row, "glyph-waves", height + 4f, MosseloraTheme.WaterAction);
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
            MeterEnd(row, TreePlay.SeedIcon, height + 4f, Color.white);
            PaintDrops(reached);
        }

        // Water that newly reaches a tile flies down into its drop, so the meter
        // explains itself without a word: each drop is a tile with water (I130).
        private void PaintDropsWithFlights(Puzzle puzzle, PipeInsight insight, float[] arrive = null)
        {
            int reached = world.Solved ? puzzle.CellCount : insight.ConnectedCount;
            int before = 0;
            if (lastFlowing != null) foreach (bool wet in lastFlowing) if (wet) before++;
            bool fly = gentleMotion && waterDrops != null && canvasRoot != null && tileArtwork != null &&
                lastFlowing != null && lastFlowing.Length == puzzle.CellCount && reached > before;
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            if (qaInMemoryOnly) fly = false; // tour screenshots capture settled screens
#endif
            if (!fly) { PaintDrops(reached); return; }
            PaintDrops(before);
            int slot = before, order = 0;
            var fresh = new System.Collections.Generic.List<int>();
            for (int i = 0; i < puzzle.CellCount; i++)
                if (insight.Flowing[i] && !lastFlowing[i]) fresh.Add(i);
            // Nearest the spring first, leaving as each tile fills (I144).
            if (arrive != null) fresh.Sort((a, b) => arrive[a].CompareTo(arrive[b]));
            foreach (int i in fresh)
            {
                if (slot >= reached || slot >= waterDrops.Length) break;
                float wait = arrive != null ? arrive[i] : order * 0.07f;
                StartCoroutine(FlyDrop(tileArtwork[i].rectTransform, slot++, wait));
                order++;
            }
            // Drops not tied to a newly wet tile (a solve fills the rest) fill at once.
            for (; slot < reached && slot < waterDrops.Length; slot++) SetDrop(waterDrops[slot], true);
        }

        // Something arcs from mid-screen into where it's kept (I229 a pear into the
        // purse, I237 a letter into the letterbox); its home swells once.
        private IEnumerator FlyInto(RectTransform target, string art, Vector2 size)
        {
            Sprite sprite = MosseloraArt.Load(art);
            if (sprite == null || target == null || canvasRoot == null) yield break;
            for (float wait = 0.35f; wait > 0f; wait -= Time.unscaledDeltaTime) yield return null;
            if (target == null) yield break;
            var node = Node("Arriving " + art, canvasRoot);
            var rect = node.GetComponent<RectTransform>();
            rect.sizeDelta = size;
            var image = node.AddComponent<Image>();
            image.sprite = sprite;
            image.preserveAspect = true;
            image.raycastTarget = false;
            Vector3 start = canvasRoot.TransformPoint(canvasRoot.rect.center);
            float lift = 90f * canvasRoot.lossyScale.y;
            for (float t = 0f; t < 1f; t += Time.unscaledDeltaTime / 0.8f)
            {
                if (target == null) { Destroy(node); yield break; }
                Vector3 end = target.TransformPoint(target.rect.center);
                float k = Mathf.SmoothStep(0f, 1f, t);
                Vector3 at = Vector3.Lerp(start, end, k);
                at.y += Mathf.Sin(k * Mathf.PI) * lift; // tossed up, then down into the purse
                rect.position = at;
                rect.localScale = Vector3.one * Mathf.Lerp(1.2f, 0.6f, k);
                rect.localRotation = Quaternion.Euler(0f, 0f, Mathf.Sin(k * Mathf.PI * 2f) * 14f);
                yield return null;
            }
            Destroy(node);
            if (target != null) BloomPulse.Add(target.gameObject, 0f, 0.25f);
        }

        private IEnumerator FlyDrop(RectTransform from, int slot, float startDelay)
        {
            Image[] drops = waterDrops;
            Sprite sprite = MosseloraArt.Load(MosseloraArt.Droplet);
            if (sprite == null || from == null || drops == null || slot >= drops.Length) yield break;
            for (float wait = startDelay; wait > 0f; wait -= Time.unscaledDeltaTime) yield return null;
            if (drops != waterDrops || from == null || drops[slot] == null) yield break;
            var node = Node("Flying drop", canvasRoot);
            var rect = node.GetComponent<RectTransform>();
            rect.sizeDelta = drops[slot].rectTransform.rect.size * 1.3f;
            var image = node.AddComponent<Image>();
            image.sprite = sprite;
            image.preserveAspect = true;
            image.raycastTarget = false;
            Vector3 start = from.TransformPoint(from.rect.center);
            float lift = 36f * canvasRoot.lossyScale.y;
            for (float t = 0f; t < 1f; t += Time.unscaledDeltaTime / 0.5f)
            {
                if (drops != waterDrops || drops[slot] == null) { Destroy(node); yield break; }
                RectTransform target = drops[slot].rectTransform;
                Vector3 end = target.TransformPoint(target.rect.center);
                float k = Mathf.SmoothStep(0f, 1f, t);
                Vector3 at = Vector3.Lerp(start, end, k);
                at.y += Mathf.Sin(k * Mathf.PI) * lift; // a small arc, like a drop flicked off a leaf
                rect.position = at;
                rect.localScale = Vector3.one * Mathf.Lerp(1.15f, 0.8f, k);
                yield return null;
            }
            Destroy(node);
            if (drops != waterDrops || drops[slot] == null) yield break;
            Puzzle puzzle = world.CurrentPuzzle;
            int now = world.Solved ? puzzle.CellCount : world.Insight.ConnectedCount;
            if (slot < now)
            {
                SetDrop(drops[slot], true);
                BloomPulse.Add(drops[slot].gameObject, 0f, 0.22f);
            }
        }

        // Seconds after the move at which each newly wet tile fills: its step
        // count along connected pipes from the spring, relative to the nearest
        // newly wet tile, 0.07s a step. Null when there is nothing to stagger.
        private float[] WaterArrival(Puzzle puzzle, int[] angles, PipeInsight insight)
        {
            bool animate = gentleMotion && lastFlowing != null && lastFlowing.Length == puzzle.CellCount;
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            if (qaInMemoryOnly) animate = false; // tour screenshots capture settled screens
#endif
            if (!animate) return null;
            int spring = Array.IndexOf(puzzle.Kinds, PipeKind.End);
            if (spring < 0 || !insight.Flowing[spring]) return null;
            int[] steps = new int[puzzle.CellCount];
            for (int i = 0; i < steps.Length; i++) steps[i] = -1;
            var queue = new System.Collections.Generic.Queue<int>();
            steps[spring] = 0;
            queue.Enqueue(spring);
            while (queue.Count > 0)
            {
                int tile = queue.Dequeue();
                for (int d = 0; d < 4; d++)
                {
                    if (!PipeRules.HasPort(puzzle.Kinds[tile], angles[tile], d)) continue;
                    int next = PipeRules.Neighbor(tile, d, puzzle.Size);
                    if (next < 0 || steps[next] >= 0 || !insight.Flowing[next]) continue;
                    if (!PipeRules.HasPort(puzzle.Kinds[next], angles[next], (d + 2) % 4)) continue;
                    steps[next] = steps[tile] + 1;
                    queue.Enqueue(next);
                }
            }
            int nearest = int.MaxValue;
            for (int i = 0; i < steps.Length; i++)
                if (steps[i] >= 0 && !lastFlowing[i]) nearest = Mathf.Min(nearest, steps[i]);
            if (nearest == int.MaxValue) return null;
            var arrive = new float[puzzle.CellCount];
            for (int i = 0; i < steps.Length; i++)
                arrive[i] = steps[i] >= 0 && !lastFlowing[i] ? (steps[i] - nearest) * 0.07f : 0f;
            return arrive;
        }

        // Fills one tile once the running water reaches it, from the board's
        // state at that moment (another move may have come in between).
        private IEnumerator WetLater(int tile, float delay)
        {
            int generation = rebuildGeneration;
            PipeGraphic[] artwork = tileArtwork;
            for (float wait = delay; wait > 0f; wait -= Time.unscaledDeltaTime) yield return null;
            if (generation != rebuildGeneration || artwork != tileArtwork || tileArtwork == null ||
                tile >= tileArtwork.Length || world.Completed) yield break;
            Puzzle puzzle = world.CurrentPuzzle;
            if (tile >= puzzle.CellCount) yield break;
            PaintRefreshedTile(puzzle, world.Insight, world.Rotations, tile, false);
        }

        // The one in-place visual rule for a tile (refresh and the running water).
        private void PaintRefreshedTile(Puzzle puzzle, PipeInsight insight, int[] angles, int i, bool held)
        {
            tileBackgrounds[i].color = MosseloraTheme.TileFill(world.Solved,
                !held && insight.Flowing[i], puzzle.IsAnchored(i), world.HintStep,
                world.HintTile, i, puzzle.Size);
            tileArtwork[i].Configure(puzzle.Kinds[i], angles[i],
                held ? false : insight.Flowing[i], world.Solved, OpenPorts(puzzle, angles, i));
        }

        private void PaintDrops(int reached)
        {
            if (waterDrops == null) return;
            for (int i = 0; i < waterDrops.Length; i++) SetDrop(waterDrops[i], i < reached);
        }

        // A filled drop is the painted droplet; an empty one is its slate outline
        // with a faint fill, so the row reads "still to fill", never "lives
        // left" (I146). The outline keeps I105's >=3:1 contrast on paper.
        private static Sprite dropFull, dropEmpty;

        private static void SetDrop(Image drop, bool wet)
        {
            if (dropFull == null) dropFull = MosseloraArt.Load(MosseloraArt.Droplet);
            if (dropEmpty == null) dropEmpty = MosseloraArt.Load(MosseloraArt.DropletEmpty);
            if (wet || dropEmpty == null)
            {
                if (dropFull != null) drop.sprite = dropFull;
                drop.color = wet ? Color.white : new Color(0.36f, 0.40f, 0.37f, 0.85f);
                return;
            }
            drop.sprite = dropEmpty;
            drop.color = new Color(0.30f, 0.35f, 0.33f, 1f);
        }

        // A calm daily ritual entry: the village painting full-bleed, the
        // wordmark, the stones walked so far and one big play button. No
        // streaks, no "you were away" - the garden simply waited.
        private void BuildTitleScreen(Transform host)
        {
            if (quietLayer != null) ExitQuiet(); // a new home never starts hidden (I294)
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            if (qaInMemoryOnly && !qaShowHub) return; // the scripted QA tour starts on the board
#endif
            // A finished garden opens on its festival; otherwise the everyday village.
            // The hub opens on *your* story: the finished garden's festival, else
            // the painting of your most recent memory, else the everyday village.
            int[] story = world.Choices;
            // Newest memory first; each later launch recalls an earlier one (I107).
            if (!hubRecallPicked)
            {
                hubRecall = world.Mode == PlayMode.Living ? PickHubRecall(story.Length) : -1;
                hubRecallPicked = true;
            }
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
            // Now and then a butterfly drifts across the village: the garden is
            // alive when you come back to it (I157). Decoration only, gentle motion only.
            bool lively = gentleMotion;
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            if (qaInMemoryOnly) lively = false;
#endif
            // I325: butterflies come to flowers in spring and summer (before I325 they always flew).
            bool livingGarden = world != null && world.Mode == PlayMode.Living;
            if (lively && (!livingGarden || (Visitors.Welcomes(world, Visitor.Butterflies) &&
                Visitors.OutIn(Visitor.Butterflies, GardenSeason.For(world), DayLightOn ? CurrentPhase() : DayPhase.Day, TodayWeather(world)))))
                window.AddComponent<HubButterflies>();
            // The living garden opens on its own map (E3, I176): everything you made
            // standing where it grew. Otherwise, the painting as before.
            bool mapShown = world.Mode == PlayMode.Living && GardenMapOnHub(windowRect);
            var art = Node("Village at the start", window.transform);
            art.SetActive(!mapShown);
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
            meltRect.anchorMax = new Vector2(1f, mapShown ? 0.08f : 0.24f); // the map runs nearly to the wordmark (I181)
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
            // With the map, the neighbours stand in the garden itself and the map
            // takes the strip's place (I181).
            strip.SetActive(!mapShown);
            if (mapShown) windowRect.anchorMin = new Vector2(0f, 0.33f);
            var painting = strip.AddComponent<PaintedVillage>();
            painting.Build(0f, PaintedVillage.ArtSeason(world));
            if (painting.Ready)
            {
                VillageScenePlan plan = VillageScenePlan.From(world);
                painting.Show(plan.Chapter, plan.TreeCount, plan.CrossingCount, plan.FlowerCount,
                    plan.WaterRestored, plan.WaterReachPercent);
            }
            if (world.Mode == PlayMode.Living) NeighborsOnPainting(strip.transform, true);

            // Home is your garden, full screen (I190): no paper, no wordmark, no
            // tagline; the walk, the next stream's sign and one round play button
            // stand on the grass at the bottom.
            bool worldHome = mapShown;
            if (worldHome)
            {
                windowRect.anchorMin = Vector2.zero;
                melt.SetActive(false);
            }
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
            if (worldHome) column.childAlignment = TextAnchor.LowerCenter;
            if (worldHome && landscape)
            {
                // Sideways the screen is short and the neighbours stand mid-garden, so
                // the sign and play token keep to a right-hand column (I197).
                stackRect.anchorMin = new Vector2(0.7f, 0.04f);
                stackRect.anchorMax = new Vector2(1f, 0.8f);
            }
            Sprite mark = worldHome ? null : MosseloraArt.Load(MosseloraArt.Wordmark);
            if (mark != null)
            {
                var markNode = Node("Mosselora wordmark", stack.transform);
                Height(markNode, 96f); // the repo's painted brand logo (about 2.6:1)
                var markImage = markNode.AddComponent<Image>();
                markImage.sprite = mark;
                markImage.preserveAspect = true;
                markImage.raycastTarget = false;
            }
            if (!worldHome)
            {
                Text tagline = Label(stack.transform, "Title", GameLocalization.Text("subtitle"), 17, 0, Ink,
                    TextAnchor.MiddleCenter);
                Emphasize(tagline);
            }
            // How far along the creek this garden has walked: resumes, never resets.
            if (!world.Completed && !(worldHome && landscape))
                StreamStones(world.CurrentPuzzle, stack.transform, false);
            // What changed last time, told by the neighbor who remembers it.
            if (world.Mode == PlayMode.Living && world.Choices.Length > 0 && !worldHome)
                KeeperLine(stack.transform, "Last memory", hubRecall);
            if (worldHome)
            {
                // I204: a thing you've grown, picked up, holds its own puzzle: the big
                // token becomes that thing, and plays its puzzle.
                int held = world.Mode == PlayMode.Living && mapInHand >= 0 && mapInHand < world.Choices.Length
                    ? mapInHand : -1;
                int heldPlant = world.Mode == PlayMode.Living && heldBought >= 0 && heldBought < world.BoughtCount
                    ? heldBought : -1;
                if (held >= 0 || heldPlant >= 0)
                {
                    Transform heldRow = CenteredRowIn(stack.transform, "Play row");
                    int heldKind = heldPlant >= 0 ? world.Bought[heldPlant * 2] : -1;
                    string heldName = heldPlant >= 0 ? Nursery.Item(heldKind).Name : ItemAt(held, world.Choices[held]).Name;
                    Button thing = IconButton(heldRow, "Play its puzzle", heldName,
                        MosseloraArt.IconContinue, () => { if (heldPlant >= 0) OpenBoughtPlay(heldPlant); else OpenTreePlay(held); },
                        96, MosseloraTheme.WaterAction, false, null, false, 0, 0, true);
                    Transform glyph = thing.transform.Find("Icon");
                    Image glyphImage = glyph != null ? glyph.GetComponent<Image>() : null;
                    Sprite heldArt = heldPlant >= 0 ? MosseloraArt.Load(Nursery.Item(heldKind).Art)
                        : ItemArt(held, world.Choices[held]);
                    if (glyphImage != null && heldArt != null)
                    {
                        glyphImage.sprite = heldArt;
                        glyphImage.color = Color.white;
                        var glyphSize = glyph.GetComponent<LayoutElement>();
                        glyphSize.minWidth = glyphSize.preferredWidth = 64f;
                        glyphSize.minHeight = glyphSize.preferredHeight = 64f;
                        // A small round "play" mark on the token's lower right.
                        Image disc = MosseloraArt.Ornament(thing.transform, "Plays", MosseloraArt.Rounded(16),
                            new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(34f, 34f), new Vector2(-4f, 4f));
                        if (disc != null)
                        {
                            disc.preserveAspect = false;
                            disc.color = MosseloraTheme.WaterAction;
                            Image playMark = MosseloraArt.Ornament(disc.transform, "Play mark", MosseloraArt.Load(MosseloraArt.IconContinue),
                                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(20f, 20f), Vector2.zero);
                            if (playMark != null) playMark.color = Color.white;
                        }
                    }
                    if (gentleMotion) BloomPulse.Add(thing.gameObject, 0.2f, 0.06f);
                    SeedPurse();
                    LetterBox();
                    if (world.Mode == PlayMode.Living) BasketControls(titleScreen.transform, stack.transform);
                    HubChrome();
                    MaybeAskNotes();
                    FollowNote();
                    titleScreen.transform.SetAsLastSibling();
                    return;
                }
                // The same plank as over the board, then a round play token (I190).
                if (!world.Completed && GameLocalization.Current == "en")
                    NextStreamLine(stack.transform, world.CurrentPuzzle.Name, false);
                Transform token = CenteredRowIn(stack.transform, "Play row");
                // I351 (run 3, P9): after the story, the big arrow plays what's waiting: the
                // newest thing still holding its first puzzle, whose picture peeks from the token.
                int waitingBought = -1, waitingThing = -1;
                if (world.Completed && world.Mode == PlayMode.Living)
                {
                    for (int k = world.BoughtCount - 1; k >= 0 && waitingBought < 0; k--) if (!world.BoughtSolved(k)) waitingBought = k;
                    if (waitingBought < 0)
                        for (int i = world.Choices.Length - 1; i >= 0 && waitingThing < 0; i--) if (!world.TreeSolved(i)) waitingThing = i;
                }
                Button playToken = IconButton(token, "Play", world.Completed ? GameLocalization.Text("continue")
                        : world.CurrentPuzzle.Name, MosseloraArt.IconContinue, () => {
                            basketOpen = false; basketKind = -1; heldPlanting = -1;
                            nurseryOpen = false; nurseryKind = -1;
                            if (waitingBought >= 0) { heldBought = waitingBought; mapInHand = -1; OpenBoughtPlay(waitingBought); return; }
                            heldBought = -1;
                            if (waitingThing >= 0) { mapInHand = waitingThing; OpenTreePlay(waitingThing); return; }
                            CloseTitle();
                        }, 96, MosseloraTheme.WaterAction, false, null, false, 0, 0, true);
                Sprite waiting = waitingBought >= 0 ? MosseloraArt.Load(Nursery.Item(world.Bought[waitingBought * 2]).Art)
                    : waitingThing >= 0 ? ItemArt(waitingThing, world.Choices[waitingThing]) : null;
                if (waiting != null)
                    MosseloraArt.Ornament(playToken.transform, "What it plays", waiting, new Vector2(1f, 1f),
                        new Vector2(0.5f, 0.5f), new Vector2(44f, 44f), new Vector2(-4f, -4f));
                if (world.Mode == PlayMode.Living) BasketControls(titleScreen.transform, stack.transform);
                SeedPurse();
                LetterBox();
                HubChrome();
                MaybeAskNotes();
                FollowNote();
                titleScreen.transform.SetAsLastSibling();
                return;
            }
            Transform play = CenteredRowIn(stack.transform, "Play row");
            // The big button names where the walk continues ("First Bend").
            string nextStop = !world.Completed && GameLocalization.Current == "en"
                ? world.CurrentPuzzle.Name : GameLocalization.Text("continue");
            // The play button carries the next stream's name beside its arrow, as
            // casual games show "Level 23" on Play: a returning player could not
            // tell what the bare arrow would start (I156). Words only where a
            // name is known; otherwise the arrow alone.
            bool named = !world.Completed && GameLocalization.Current == "en";
            int cut = nextStop.IndexOf(" · ", System.StringComparison.Ordinal);
            string place = cut > 0 ? nextStop.Substring(0, cut) : nextStop;
            Button start = IconButton(play, "Play", named ? place : nextStop,
                MosseloraArt.IconContinue, CloseTitle, 72, MosseloraTheme.WaterAction, named);
            if (named)
            {
                Transform caption = start.transform.Find("Caption");
                Text placeText = caption != null ? caption.GetComponent<Text>() : null;
                if (placeText != null) { StyleText(placeText, 20); Emphasize(placeText); }
                Width(start, 250);
            }
            else Width(start, 150);
            HubChrome();
            titleScreen.transform.SetAsLastSibling();
            if (landscape)
            {
                // Sideways (I114): the painting and village fill the left, the
                // wordmark and Play stand in the right-hand column.
                const float split = 0.56f;
                windowRect.anchorMin = Vector2.zero;
                windowRect.anchorMax = new Vector2(split, 1f);
                stripRect.anchorMin = Vector2.zero;
                stripRect.anchorMax = new Vector2(split, 0.36f);
                stackRect.anchorMin = new Vector2(split, 0.04f);
                stackRect.anchorMax = new Vector2(1f, 0.96f);
                var side = Node("Village melt right", window.transform);
                var sideRect = side.GetComponent<RectTransform>();
                sideRect.anchorMin = new Vector2(1f, 0f);
                sideRect.anchorMax = new Vector2(1f, 1f);
                sideRect.pivot = new Vector2(1f, 0.5f);
                sideRect.sizeDelta = new Vector2(56f, 0f);
                var sideImage = side.AddComponent<Image>();
                sideImage.sprite = MosseloraArt.SideFade();
                sideImage.color = Paper;
                sideImage.raycastTarget = false;
            }
            else
                StartCoroutine(FitHubStack(windowRect, stripRect, stackRect, mapShown ? 0.33f : 0.41f));
        }

        // I201: the memory screen's map grows into the blank paper between the
        // memory card and the pinned buttons, measured on screen after layout, so
        // it fits any phone and text size.
        private GameObject fillTarget;
        private IEnumerator GrowToFill(GameObject element, RectTransform above, RectTransform buttons)
        {
            yield return null; // after the first layout pass, at the real canvas size
            if (element == null || above == null || buttons == null) yield break;
            var layout = element.GetComponent<LayoutElement>();
            if (layout == null) yield break;
            var corners = new Vector3[4];
            above.GetWorldCorners(corners);
            float cardBottom = element.transform.parent.InverseTransformPoint(corners[0]).y;
            float buttonTop = float.NegativeInfinity;
            foreach (Transform child in buttons)
            {
                if (!child.gameObject.activeSelf) continue;
                ((RectTransform)child).GetWorldCorners(corners);
                buttonTop = Mathf.Max(buttonTop, element.transform.parent.InverseTransformPoint(corners[1]).y);
            }
            float gap = cardBottom - buttonTop - 22f; // keep a little air above the buttons
            if (float.IsInfinity(buttonTop) || gap < 12f) yield break;
            layout.minHeight = layout.preferredHeight = Mathf.Round(layout.preferredHeight + Mathf.Min(gap, 320f));
        }

        // The words and Play under the painting take the room they need: at
        // larger text, or with a memory line and the next stream, the painting
        // and village strip move up instead of the words overflowing (I85).
        private IEnumerator FitHubStack(RectTransform window, RectTransform strip, RectTransform stack,
                                        float windowBase)
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
            // The map reaches down to the words (I181); the painting kept its old base.
            window.anchorMin = new Vector2(0f, windowBase + shift);
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
            // One button (I191): it opens the fan of everything else.
            Button menu = IconButton(bar.transform, "Open menu", GameLocalization.Text("menu"),
                MosseloraArt.IconMenu, () => ToggleFan(true), 54, MosseloraTheme.QuietAction);
            PlaceChrome(menu, new Vector2(0f, 1f), new Vector2(10f, ChromeTop));
            if (world.Mode == PlayMode.Living && world.Choices.Length > 0) MarkUnreadMemory(menu);
        }

        // "Rainwater" with the painted landmark your last choice planted in it:
        // the next stream visibly carries what you grew (I73). Placed above Play.
        private void NextStreamLine(Transform stack, string nextStop, bool abovePlay)
        {
            int split = nextStop.IndexOf(" · ", System.StringComparison.Ordinal);
            int[] past = world.Choices;
            Sprite mark = split > 0 && past.Length > 0
                ? ItemArt(past.Length - 1, past[past.Length - 1]) : null;
            Transform row = CenteredRowIn(stack, "Next stream row");
            row.GetComponent<HorizontalLayoutGroup>().spacing = 6;
            Transform outer = row;
            row = PaintedSign(row) ?? row; // the same plank as on the board (I165)
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
            bool onSign = row != outer;
            Text where = Label(row, "Next stream", mark != null ? nextStop.Substring(0, split) : nextStop,
                16, 0, onSign ? Ink : Moss, TextAnchor.MiddleCenter);
            Emphasize(where);
            if (onSign) OneLine(where);
            if (abovePlay) outer.SetSiblingIndex(stack.childCount - 2);
        }

        private int hubRecall = -1;
        private bool hubRecallPicked; // a rotation rebuilds the hub; it must not advance the recall
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

        // ---- E3 (I176): the open garden map.
        private GardenMap gardenMap;

        // The endless river (I192): one reach of the painting per three streams
        // restored, all of them once the garden is finished.
        private GardenMap LoadGardenMap()
        {
            // After the story the river keeps opening as you tend your things (I220).
            int reaches = GardenMap.ReachesFor(world.Choices.Length, world.Completed, world.PuzzlesSolved);
            if (gardenMap != null && gardenMap.Reaches == reaches) return gardenMap;
            var mask = Resources.Load<TextAsset>("Evergrove/garden-map-mask");
            if (mask == null) return null;
            try { gardenMap = GardenMap.River(10, 15, mask.text.Trim(), reaches); }
            catch (ArgumentException error) { Debug.LogWarning("Garden map mask unreadable: " + error.Message); }
            return gardenMap;
        }

        // After the story, the stretch in the mist wears the season it will bring (I252).
        private Sprite NextSeasonPlot()
        {
            if (world.Mode != PlayMode.Living || !world.Completed) return null;
            if (!GardenMap.TowardNext(world.Choices.Length, true, world.PuzzlesSolved, out int have, out int need)) return null;
            Season next = GardenSeason.AfterStory(world.PuzzlesSolved + (need - have));
            int art = next == Season.Spring ? 3 : next == Season.Autumn ? 1 : next == Season.Winter ? 2 : 0;
            return MosseloraArt.Load(PaintedVillage.MapFor(art));
        }

        // The next reach shows in mist below the open river until it opens.
        private static bool RiverGoesOn(GardenMap map) { return map != null && map.Reaches < GardenMap.ReachesCeiling; }

        private readonly float[] hubZoom = { 0f, 0f };
        private bool markPuzzles;

        // ---- I204: the puzzle a grown thing holds, and the seeds it pays.
        private void OpenTreePlay(int item)
        {
            int[] made = world.Choices;
            if (titleScreen == null || item < 0 || item >= made.Length) return;
            GardenItem thing = ItemAt(item, made[item]);
            // A thing's first board is its own (seeded by garden and place); replays deal fresh
            // boards that deepen as you come back to it (I259/I260).
            int replays = world.TreeSolved(item) ? Replays(item) : 0;
            int tier = ReplayBoards.Tier(TreePuzzles.TierFor(item), replays);
            int seed = ReplayBoards.Seed(world.Garden * 1000 + item * 7 + 1, world.TreeSolved(item) ? replays + 1 : 0);
            // v1 #80: one day decides both the reward shown here and the seeds paid on solve.
            int day = LocalDayStamp();
            int reward = world.TreeSolved(item)
                ? world.ReplayRewardOn(TreePuzzles.TierFor(item), day)
                : WorldModel.TreeReward(TreePuzzles.TierFor(item), true);
            bool replay = world.TreeSolved(item);
            Func<int> solved = () => { int fruitBeforeSolve = world.Fruit; int earned = world.SolveTree(item, day); if (replay) CountReplay(item);
                NotePollinated(item, world.Fruit > fruitBeforeSolve); // I378, gated by the fruit this solve picked
                SaveWorld(); PlayChime(); return earned; };
            OpenThingPuzzle(seed, tier, ThingPuzzle.ForStory(item, (VillageChoice)made[item]), ItemArt(item, made[item]),
                thing.Name, reward, item, solved, ownBoard: true);
        }

        // Replays per thing, on this device (I259): only what boards to deal next, never progress.
        private string ReplayKey(int index) { return "evergrove:replays:" + world.Garden + ":" + index; }
        private int Replays(int index) { return Mathf.Max(0, PlayerPrefs.GetInt(ReplayKey(index), 0)); }
        private void CountReplay(int index)
        {
            PlayerPrefs.SetInt(ReplayKey(index), Replays(index) + 1);
            PlayerPrefs.Save();
        }

        // A nursery plant's puzzle (I208): its row is its tier; its own seed.
        private void OpenBoughtPlay(int k)
        {
            if (titleScreen == null || k < 0 || k >= world.BoughtCount) return;
            int kind = world.Bought[k * 2];
            GardenItem plant = Nursery.Item(kind);
            bool replay = world.BoughtSolved(k);
            int replays = replay ? Replays(BoughtIndex + k) : 0;
            int tier = ReplayBoards.Tier(Nursery.Row(kind), replays);
            int day = LocalDayStamp(); // v1 #80: the same day for the reward shown and the seeds paid
            int reward = replay ? world.ReplayRewardOn(Nursery.Row(kind), day) : WorldModel.TreeReward(Nursery.Row(kind), true);
            OpenThingPuzzle(ReplayBoards.Seed(world.Garden * 1000 + 500 + k * 7, replay ? replays + 1 : 0), tier,
                ThingPuzzle.ForNursery(kind),
                MosseloraArt.Load(plant.Art), plant.Name, reward, k * 5 + kind,
                () => { int fruitBeforeSolve = world.Fruit; int earned = world.SolveBought(k, day); if (replay) CountReplay(BoughtIndex + k);
                    NotePollinated(world.Choices.Length + world.PlantingCount + k, world.Fruit > fruitBeforeSolve); // I378
                    SaveWorld(); PlayChime(); return earned; }, ownBoard: true);
        }

        // Each thing holds one kind of puzzle (I243: ThingPuzzle).
        private void OpenThingPuzzle(int seed, int tier, PuzzleKind kind, Sprite art, string name, int reward,
                                     int pictureStart, Func<int> solved, bool ownBoard = false)
        {
            moveNote = 0; // I305: each board starts its phrase from the top
            Action turned = () => { PlayMove(kind); if (gentleMotion) Haptics.Tick(); };
            int shown = mapInHand >= 0 ? mapInHand : heldBought >= 0 ? BoughtIndex + heldBought : -1;
            // Back home, the view returns to the thing and its ring grows in (I212).
            string hintsKey = "evergrove:hints:" + (int)kind; // I322: per kind, on this device
            Action closed = () => { if (openCard != null) { PlayerPrefs.SetInt(hintsKey, openCard.HintsUsed); PlayerPrefs.Save(); }
                justSolved = solvedHere ? shown : -1; solvedHere = false;
                mapInHand = -1; heldBought = -1; keepHubAt = new Vector2(-1f, -1f); RefreshMapsFresh(); };
            Func<int> solvedOnce = solved;
            solved = () =>
            {
                solvedHere = true;
                int before = OpenReaches(), solvedBefore = world.PuzzlesSolved, fruitBefore = world.Fruit;
                int earned = solvedOnce();
                // Played taking turns (I263/I264): the garden remembers it was solved together.
                if (openCard != null && openCard.PlayedTogether && shown >= 0 && world.MarkTogether(shown))
                {
                    SaveWorld();
                    togetherKind = kind; // I266: its keeper has news
                    togetherKeeper = NeighbourNotice.OwnerOf(kind);
                }
                fruitPicked = world.Fruit > fruitBefore; // trees bear fruit when tended (I229)
                // After the story, every fourth solve opens more river (I220): home shows it (I223).
                riverOpened = OpenReaches() > before;
                // Mira leaves a note in the letterbox when the river opens (I271).
                if (riverOpened && world.Completed &&
                    world.KeepNote("Mira", NeighbourNotice.RiverNote(GardenSeason.AfterStory(world.PuzzlesSolved),
                        RiverNames.For(OpenReaches() - 1)))) // I362: by name
                {
                    letterArrived = true;
                    SaveWorld();
                }
                dropEarned = world.PuzzlesSolved > solvedBefore;
                return earned;
            };
            PuzzleCard((RectTransform)titleScreen.transform, seed, tier, kind, art, name, reward,
                count => PairsPictures(count, pictureStart, (c, f) => ItemArt(c, f)), turned, solved, closed);
            TreePlay card = titleScreen.GetComponentInChildren<TreePlay>();
            openCard = card;
            // I261: how deep this board is, in leaves. I336 (idea 13, after Nikoli's honest
            // difficulty): kinds that can measure their own logic show that, not their size.
            if (card != null) card.ShowTier(boardLeaves >= 1 ? boardLeaves - 1 : tier);
            // I323: each kind's flourish when solved.
            if (card != null) { card.Flourish = FlourishArt(kind, out Color tint); card.FlourishTint = tint; }
            // I322: a kind that needed hints last time starts with a move or two made.
            if (card != null) card.GiveHeadStart(HeadStart.Moves(PlayerPrefs.GetInt(hintsKey, 0), tier));
            // I309: a board of your own can be sent to family once solved. v1 #80: ownership is
            // explicit, because a capped replay of your own thing now pays 0 seeds.
            if (card != null && ownBoard && world.Mode == PlayMode.Living)
                card.SendBoard = () => SendBoardToFamily(kind, tier, seed, name);
        }

        // The card itself, for your things and, visiting, for theirs (I227); eight kinds (I243).
        private int boardLeaves; // I336: 1..3 from the board's logic, or 0 when the kind can't measure it

        private void PuzzleCard(RectTransform host, int seed, int tier, PuzzleKind kind, Sprite art, string name,
                                int reward, Func<int, Sprite[]> pictures, Action turned, Func<int> solved, Action closed)
        {
            TreePlay.IPuzzleBoard board = null;
            boardLeaves = 0; // I336: set by kinds that measure their own logic
            switch (kind)
            {
                case PuzzleKind.Nonogram:
                {
                    var nonogram = new NonogramGame(seed, tier);
                    boardLeaves = nonogram.Leaves;
                    board = new NonogramBoard(nonogram);
                    break;
                }
                case PuzzleKind.Sudoku:
                {
                    var sudoku = new SeedSudokuGame(seed, tier);
                    boardLeaves = sudoku.Leaves;
                    board = new SudokuBoard(sudoku, pictures(sudoku.Size));
                    break;
                }
                case PuzzleKind.Plots: board = new PlotsBoard(new PlotsGame(seed, tier)); break;
                case PuzzleKind.StreamPath: board = new StreamBoard(new StreamPathGame(seed, tier)); break;
                case PuzzleKind.Sowing: board = new SowBoard(seed, tier); break;            // I318
                case PuzzleKind.Sums: // I324
                {
                    var sums = new SeedSumsGame(seed, tier);
                    boardLeaves = sums.Leaves;
                    board = new SumsBoard(sums);
                    break;
                }
                case PuzzleKind.Pieces: board = new PiecesBoard(new GardenPiecesGame(seed, tier), tier); break; // I331
                case PuzzleKind.Doorstep: // I332
                {
                    var doorstep = new DoorstepGame(seed, tier);
                    boardLeaves = doorstep.Leaves;
                    board = new DoorstepBoard(doorstep, tier);
                    break;
                }
                case PuzzleKind.Mahjong: board = new MahjongBoard(new MahjongGame(seed, tier), tier, pictures(MahjongGame.FacesFor(tier))); break;
            }
            if (board != null)
            {
                TreePlay.OpenBoard(host, board, art, name, reward, font, gentleMotion, turned, solved, closed);
                return;
            }
            if (kind == PuzzleKind.Pairs)
            {
                var game = new PairsGame(seed, tier);
                TreePlay.OpenPairs(host, game, pictures(game.Pairs), art, name, reward, font,
                    gentleMotion, turned, solved, closed);
                return;
            }
            if (kind == PuzzleKind.Mosaic)
            {
                // Buildings hold a Mosaic of one of the village's memory paintings.
                string[] paintings = { MosseloraArt.VignetteBridge, MosseloraArt.VignetteTree,
                    MosseloraArt.VignetteFlowers, MosseloraArt.HeroVillage, MosseloraArt.EndingFestival };
                Sprite picture = MosseloraArt.Load(paintings[Mathf.Abs(seed) % paintings.Length]);
                if (picture != null)
                {
                    TreePlay.OpenMosaic(host, new MosaicGame(seed, tier), picture, art, name, reward, font,
                        gentleMotion, turned, solved, closed);
                    return;
                }
            }
                        TreePlay.Open(host, TreePuzzles.For(seed, tier, name), art, name, reward, font, gentleMotion,
                turned, solved, closed);
        }

        // Distinct paintings from this garden's catalogue for a Pairs table, starting
        // at a place set by the flower, so each flower deals different pictures.
        private Sprite[] PairsPictures(int count, int item, Func<int, int, Sprite> artOf)
        {
            var found = new System.Collections.Generic.List<Sprite>();
            // I290: the table is dealt from the flowers of the garden's season first, a
            // card for each flower in bloom, starting where this thing points.
            int[] inBloom = GardenCatalog.FlowersOf(world != null ? GardenSeason.For(world) : Season.Spring);
            for (int step = 0; step < inBloom.Length && found.Count < count; step++)
            {
                int flower = inBloom[(Mathf.Abs(item) * 3 + step) % inBloom.Length];
                Sprite art = MosseloraArt.Load(GardenCatalog.Get(VillageChoice.Flowers, flower).Art);
                if (art != null && !found.Contains(art)) found.Add(art);
            }
            int total = GardenCatalog.ChaptersPerGarden * 3;
            for (int step = 0; step < total && found.Count < count; step++)
            {
                int slot = (item * 5 + step) % total;
                Sprite art = artOf(slot / 3, slot % 3);
                if (art != null && !found.Contains(art)) found.Add(art);
            }
            return found.ToArray();
        }

        // ---- I215: family parcels, over home (or the page when on a board).
        private FamilySheet familySheet;
        private const string FamilyNameKey = "evergrove:family-name";

        private void OpenFamily()
        {
            if (familySheet != null) return;
            RectTransform host = titleScreen != null ? (RectTransform)titleScreen.transform : canvasRoot;
            familySheet = FamilySheet.Open(host, font, new FamilySheet.Hooks
            {
                Seeds = () => world.Seeds,
                Fruit = () => world.Fruit,
                Jam = () => world.Jam,
                RowOpen = row => world.RowOpen(row),
                OwnedKinds = () => {
                    int[] bought = world.Bought;
                    var kinds = new System.Collections.Generic.List<int>();
                    for (int k = bought.Length / 2 - 1; k >= 0; k--)
                        if (!kinds.Contains(bought[k * 2])) kinds.Add(bought[k * 2]);
                    return kinds.ToArray();
                },
                Send = (from, words, gift, value, nonce, wish, picture) => {
                    ParcelSendResult sent = world.SendParcel(from, words, gift, value, nonce, wish, 0, picture);
                    if (sent.Ok) { SaveWorld(); PlayChime(); }
                    return sent;
                },
                Open = text => {
                    ParcelOpenResult opened = world.Open(text);
                    if (opened.Ok && opened.Contents != null && opened.Contents.Letter.Length > 0) letterArrived = true;
                    if (opened.Ok) { SaveWorld(); PlayChime(); }
                    return opened;
                },
                Share = text => { if (!NativeShare.Text(text, "Mosselora")) GUIUtility.systemCopyBuffer = text; },
                Paste = () => GUIUtility.systemCopyBuffer,
                Postcard = () => {
                    string text = GardenPostcard.Build(world,
                        world.Completed ? "" : GameLocalization.Text("stream", world.Round + 1, PuzzleCatalog.Count));
                    if (!NativeShare.Text(text, GameLocalization.Text("postcard"))) GUIUtility.systemCopyBuffer = text;
                },
                Visit = text => {
                    if (!OpenVisit(text)) return false;
                    if (familySheet != null) Destroy(familySheet.gameObject);
                    familySheet = null;
                    return true;
                },
                Closed = () => { familySheet = null; RefreshMaps(); },
                Name = () => PlayerPrefs.GetString(FamilyNameKey, ""),
                SaveName = name => { PlayerPrefs.SetString(FamilyNameKey, name); PlayerPrefs.Save(); },
                PlayBoard = (kind, tier, seed, from) => {
                    if (familySheet != null) Destroy(familySheet.gameObject);
                    familySheet = null;
                    PlayTheirBoard(kind, tier, seed, from);
                },
            });
        }

        // ---- I309: the very board a relative solved, played here. It pays nothing and
        // changes nothing in your garden; it's simply the same puzzle, shared.
        private void PlayTheirBoard(int kind, int tier, int seed, string from)
        {
            if (titleScreen == null || kind < 0 || kind > (int)PuzzleKind.Doorstep) return;
            OpenThingPuzzle(seed, Mathf.Clamp(tier, 0, TreePuzzles.Tiers - 1), (PuzzleKind)kind,
                MosseloraArt.Load("letter-sealed"), from + "'s board", 0, Mathf.Abs(seed % 97), () => 0);
        }

        private Sprite FlourishArt(PuzzleKind kind, out Color tint)
        {
            tint = Color.white;
            switch (kind)
            {
                case PuzzleKind.Water: case PuzzleKind.StreamPath:
                    return MosseloraArt.Load(MosseloraArt.Droplet);
                case PuzzleKind.Pairs:
                    Sprite[] bloom = PairsPictures(1, 0, (c, f) => ItemArt(c, f));
                    return bloom.Length > 0 ? bloom[0] : MosseloraArt.Load(MosseloraArt.Sprig);
                case PuzzleKind.Mosaic:
                    tint = new Color(1f, 0.86f, 0.45f, 1f);
                    return MosseloraArt.Rounded(6);
                case PuzzleKind.Mahjong:
                    return MosseloraArt.Load("stone-spring");
                case PuzzleKind.Plots:
                    return MosseloraArt.Load(MosseloraArt.Sprig);
                default:
                    return MosseloraArt.Load(TreePlay.SeedIcon);
            }
        }

        private void SendBoardToFamily(PuzzleKind kind, int tier, int seed, string thing)
        {
            string from = PlayerPrefs.GetString(FamilyNameKey, "");
            if (!FamilyParcel.ValidName(from)) { OpenFamily(); return; } // your name first, on the family page
            string nonce = Guid.NewGuid().ToString("N").Substring(0, 16);
            ParcelSendResult sent = world.SendBoard(from, "", (int)kind, tier, seed, nonce);
            if (!sent.Ok) return;
            SaveWorld();
            string text = FamilyParcel.Wrap(sent.Text, from + " sent you the " + thing +
                " puzzle they solved in Mosselora. Open it in the game: Family, then Open.");
            if (!NativeShare.Text(text, "Mosselora")) GUIUtility.systemCopyBuffer = text;
        }

        // ---- I216: visit a relative's garden, read-only. Their garden file (the one
        // Settings copies) is read without touching yours; their river, things,
        // plantings and nursery plants stand on their own painted map.
        private GameObject visitLayer;
        private string visitText;
        private WorldModel visitWorld;
        private readonly System.Collections.Generic.List<int> helpedOnVisit = new System.Collections.Generic.List<int>();

        private bool OpenVisit(string text)
        {
            if (!GardenTransfer.TryImport(text, out WorldModel visited, out GardenImportError _) ||
                visited.Mode != PlayMode.Living) return false;
            var mask = Resources.Load<TextAsset>("Evergrove/garden-map-mask");
            if (mask == null) return false;
            GardenMap map;
            try { map = GardenMap.River(10, 15, mask.text.Trim(), GardenMap.ReachesFor(visited.Choices.Length, visited.Completed, visited.PuzzlesSolved)); }
            catch (ArgumentException) { return false; }
            ApplyDayLight(visited); // your hour, their season
            // Their garden in its own season (I249): families see each other's year.
            int season = PaintedVillage.ArtSeason(visited);
            Sprite plot = MosseloraArt.Load(PaintedVillage.MapFor(season)) ?? MosseloraArt.Load("garden-map");
            if (plot == null) return false;

            if (text != visitText) helpedOnVisit.Clear();
            visitText = text;
            visitWorld = visited;
            visitLayer = Node("Visiting a garden", canvasRoot);
            Fill(visitLayer.GetComponent<RectTransform>());
            visitLayer.AddComponent<Image>().color = Paper;
            visitLayer.transform.SetAsLastSibling();
            var window = Node("Their garden", visitLayer.transform);
            Fill(window.GetComponent<RectTransform>());
            window.AddComponent<RectMask2D>();
            SetVisitorsFor(visited, map, Pairings.Things(visited, map)); // I384: their visitors too
            GardenMapView.PairLinksNext = Pairings.Links(map, Pairings.Things(visited, map)); // and their sprigs
            // Help (I227): tap one of their things that still holds a puzzle, solve it for them.
            GardenMapView view = GardenMapView.Build((RectTransform)window.transform, plot, map, VisitItems(visited, map),
                landscape ? 1f : 1.3f, null, HelpWith, -1, false, 1f, -1f, null, 0.22f, RiverGoesOn(map));
            view.FocusFromTop = 0.4f;

            // A wooden "back home" token where the menu button lives, and a sign.
            var bar = Node("Visit chrome", visitLayer.transform);
            var barRect = bar.GetComponent<RectTransform>();
            Rect safe = Screen.safeArea;
            float w = Mathf.Max(1, Screen.width), h = Mathf.Max(1, Screen.height);
            barRect.anchorMin = new Vector2(safe.xMin / w, safe.yMax / h);
            barRect.anchorMax = new Vector2(safe.xMax / w, safe.yMax / h);
            barRect.pivot = new Vector2(0.5f, 1f);
            barRect.sizeDelta = new Vector2(0f, 70f);
            Button home = IconButton(bar.transform, "Back to your garden", "Back to your garden", MosseloraArt.IconHome,
                CloseVisit, 54, MosseloraTheme.QuietAction);
            PlaceChrome(home, new Vector2(0f, 1f), new Vector2(10f, ChromeTop));
            var sign = Node("Visiting sign", bar.transform);
            var signRect = sign.GetComponent<RectTransform>();
            signRect.anchorMin = signRect.anchorMax = new Vector2(0f, 1f);
            signRect.pivot = new Vector2(0f, 1f);
            signRect.sizeDelta = new Vector2(Mathf.Round(220f * Mathf.Max(1f, TextScale.Percent / 100f)), 44f);
            signRect.anchoredPosition = new Vector2(78f, -ChromeTop - 4f); // right of the home token
            var signImage = sign.AddComponent<Image>();
            MosseloraArt.Round(signImage, new Color(0.98f, 0.95f, 0.86f, 0.96f), 22);
            MosseloraArt.Lift(signImage, 2f, 0.2f);
            Text words = Label(sign.transform, "Visiting words", "Visiting a garden", 18, 0, Ink, TextAnchor.MiddleCenter);
            words.GetComponent<LayoutElement>().ignoreLayout = true;
            Fill(words.rectTransform);
            words.rectTransform.offsetMin = new Vector2(12f, 4f);
            words.rectTransform.offsetMax = new Vector2(-12f, -4f);
            Emphasize(words);
            words.horizontalOverflow = HorizontalWrapMode.Overflow; // one line; the sign grows with text size
            // I334 (idea 40, omiyage): take a keepsake home: a cutting of their newest nursery plant,
            // copied, never taken from them, at its nursery price. Once a visit.
            int[] theirs = visited.Bought;
            if (theirs.Length >= 2 && keepsakeTakenFrom != text && world.Mode == PlayMode.Living)
            {
                int keepKind = theirs[theirs.Length - 2];
                if (keepKind >= 0 && keepKind < Nursery.Kinds)
                {
                    bool afford = world.Seeds >= Nursery.Price(keepKind);
                    Button keep = IconButton(bar.transform, "Take a keepsake cutting home, " + Nursery.Price(keepKind) + " seeds",
                        "Keepsake", Nursery.Item(keepKind).Art, () => TakeKeepsake(keepKind, text), 54, MosseloraTheme.QuietAction);
                    PlaceChrome(keep, new Vector2(1f, 1f), new Vector2(-10f, ChromeTop));
                    Transform glyph = keep.transform.Find("Icon");
                    if (glyph != null) glyph.GetComponent<Image>().color = afford ? Color.white : new Color(1f, 1f, 1f, 0.4f);
                }
            }
            VisitGiftIdea(visited, map); // I398, I399
            semanticsDirty = true;
            return true;
        }

        // I398: on arrival, a gift idea: the one thing that would give their garden a good
        // neighbour it hasn't found. I399: and what your two gardens both have.
        private void VisitGiftIdea(WorldModel visited, GardenMap map)
        {
            if (visitLayer == null || world == null || world.Mode != PlayMode.Living) return;
            var things = Pairings.Things(visited, map);
            string shared = SharedWords(Pairings.Shared(world, visited));
            FamilySheet.IdeaFamily = -1;
            if (Pairings.GiftIdea(map, things, visited.PairingsFound, Pairings.Open(visited), out VillageChoice kind, out Pairing makes))
            {
                FamilySheet.IdeaFamily = (int)kind; // I408: the send sheet remembers it
                // I418 (P25): the rule itself, and its two paintings, said once.
                RuleToast.Show((RectTransform)visitLayer.transform,
                    "A gift idea for their garden: " + Pairings.Hint(makes).TrimEnd('.').ToLowerInvariant() + "." + shared,
                    font, Ink, MosseloraTheme.CardFill, PairingMarks(makes));
            }
            else if (shared.Length > 0)
                RuleToast.Show((RectTransform)visitLayer.transform, shared.Trim(), font, Ink, MosseloraTheme.CardFill);
        }

        private static string PairingName(Pairing pairing)
        {
            switch (pairing)
            {
                case Pairing.Sheltered: return "a sheltered flower";
                case Pairing.ByTheWater: return "something by the water";
                case Pairing.FlowerBed: return "a flower bed";
                case Pairing.Grove: return "a grove";
                case Pairing.Waterside: return "flowers by the water";
                case Pairing.ShadySeat: return "a shady seat";
                default: return "an orchard";
            }
        }

        private static string SharedWords(int shared)
        {
            var names = new System.Collections.Generic.List<string>();
            foreach (Pairing p in Enum.GetValues(typeof(Pairing)))
                if ((shared & PairingMask.Of(p)) != 0) names.Add(PairingName(p));
            if (names.Count == 0) return "";
            return " You both have " + (names.Count == 1 ? names[0]
                : string.Join(", ", names.GetRange(0, names.Count - 1).ToArray()) + " and " + names[names.Count - 1]) + ".";
        }

        private string keepsakeTakenFrom;

        private void TakeKeepsake(int kind, string visit)
        {
            if (!world.TakeKeepsake(kind))
            {
                if (visitLayer != null)
                    RuleToast.Show((RectTransform)visitLayer.transform, world.Seeds < Nursery.Price(kind)
                        ? "A keepsake cutting costs " + Nursery.Price(kind) + " seeds, like the nursery."
                        : "Your basket is full. Plant something first.", font, Ink, MosseloraTheme.CardFill);
                return;
            }
            keepsakeTakenFrom = visit;
            world.KeepNote("Fen", "You brought home a cutting of " + Nursery.Item(kind).Name +
                " from your family's garden. It's in your basket, a keepsake of the visit.");
            letterArrived = true;
            SaveWorld();
            PlayChime();
            if (visitLayer != null) { Destroy(visitLayer); visitLayer = null; OpenVisit(visit); }
        }

        private System.Collections.Generic.List<GardenMapView.Placed> VisitItems(WorldModel visited, GardenMap map)
        {
            int[] made = visited.Choices, planted = visited.Plantings, bought = visited.Bought;
            int[] cells = map.Layout(made, visited.Placements, visited.Occupied); // decorations too (I330)
            var items = new System.Collections.Generic.List<GardenMapView.Placed>();
            Func<int, int, Sprite> art = (chapter, family) => MosseloraArt.Load(GardenCatalog.For(visited.Garden,
                Mathf.Clamp(chapter, 0, GardenCatalog.ChaptersPerGarden - 1), (VillageChoice)Mathf.Clamp(family, 0, 2)).Art);
            for (int i = 0; i < made.Length; i++)
            {
                bool done = visited.TreeSolved(i) || visited.IsHelped(i) || helpedOnVisit.Contains(i);
                items.Add(new GardenMapView.Placed { Art = art(i, made[i]), Cell = cells[i], Index = i,
                    Family = (VillageChoice)Mathf.Clamp(made[i], 0, 2), Bloomed = done,
                    HoldsPuzzle = !done && world.Mode == PlayMode.Living,
                    Kind = (int)ThingPuzzle.ForStory(i, (VillageChoice)Mathf.Clamp(made[i], 0, 2)) + 1,
                    Together = visited.SolvedTogether(i), // I270: their family moments show when you visit
                    BloomNow = gentleMotion && helpedOnVisit.Count > 0 && helpedOnVisit[helpedOnVisit.Count - 1] == i });
            }
            for (int k = 0; k * 2 + 1 < planted.Length; k++)
                if (planted[k * 2] < made.Length && map.IsLand(planted[k * 2 + 1]))
                    items.Add(new GardenMapView.Placed { Art = art(planted[k * 2], made[planted[k * 2]]),
                        Cell = planted[k * 2 + 1], Index = PlantingIndex + k,
                        Family = (VillageChoice)Mathf.Clamp(made[planted[k * 2]], 0, 2) });
            for (int k = 0; k * 2 + 1 < bought.Length; k++)
                if (map.IsLand(bought[k * 2 + 1]))
                    items.Add(new GardenMapView.Placed { Art = MosseloraArt.Load(Nursery.Item(bought[k * 2]).Art),
                        Cell = bought[k * 2 + 1], Index = BoughtIndex + k, Family = Nursery.Family(bought[k * 2]),
                        Bloomed = visited.BoughtSolved(k), Together = visited.SolvedTogether(BoughtIndex + k) });
            if (items.Count > 0)
            {
                // Frame their newest thing, or the one you just helped with, blooming (I227).
                int frame = helpedOnVisit.Count > 0 ? helpedOnVisit[helpedOnVisit.Count - 1] : made.Length > 0 ? made.Length - 1 : 0;
                var newest = items[frame];
                newest.Newest = true;
                items[frame] = newest;
            }
            // I330 (idea 16, after visiting player-made worlds): their decorations stand where they set them.
            int[] theirDecor = visited.Decor;
            for (int d = 0; d * 2 + 1 < theirDecor.Length; d++)
                if (map.IsLand(theirDecor[d * 2 + 1]))
                    items.Add(new GardenMapView.Placed { Art = MosseloraArt.Load(DecorArt((Decoration)theirDecor[d * 2])),
                        Cell = theirDecor[d * 2 + 1], Index = DecorIndex + d, Family = VillageChoice.Bridge });
            return items;
        }

        private void CloseVisit()
        {
            if (visitLayer != null) Destroy(visitLayer);
            visitLayer = null;
            semanticsDirty = true;
        }

        // I227: help with one of their things. You play the very board they would
        // (same seed), earn nothing, and a Help parcel goes to them by any message
        // app; it blooms in their garden when they open it.
        private void HelpWith(int item)
        {
            WorldModel theirs = visitWorld;
            if (theirs == null || visitLayer == null || world.Mode != PlayMode.Living) return;
            int[] made = theirs.Choices;
            if (item < 0 || item >= made.Length || theirs.TreeSolved(item) || theirs.IsHelped(item) ||
                helpedOnVisit.Contains(item)) return;
            int tier = TreePuzzles.TierFor(item);
            GardenItem thing = GardenCatalog.For(theirs.Garden, Mathf.Clamp(item, 0, GardenCatalog.ChaptersPerGarden - 1),
                (VillageChoice)Mathf.Clamp(made[item], 0, 2));
            Func<int, int, Sprite> artOf = (chapter, family) => MosseloraArt.Load(GardenCatalog.For(theirs.Garden,
                Mathf.Clamp(chapter, 0, GardenCatalog.ChaptersPerGarden - 1), (VillageChoice)Mathf.Clamp(family, 0, 2)).Art);
            moveNote = 0;
            PuzzleKind helpKind = ThingPuzzle.ForStory(item, (VillageChoice)made[item]);
            Action turned = () => { PlayMove(helpKind); if (gentleMotion) Haptics.Tick(); };
            Func<int> solved = () =>
            {
                string from = PlayerPrefs.GetString(FamilyNameKey, "");
                if (!FamilyParcel.ValidName(from)) from = "Family";
                string nonce = Guid.NewGuid().ToString("N").Substring(0, 16);
                ParcelSendResult sent = world.SendParcel(from, "", ParcelGift.Help, item, nonce);
                if (!sent.Ok) return 0;
                SaveWorld();
                PlayChime();
                helpedOnVisit.Add(item);
                helpShare = FamilyParcel.Wrap(sent.Text, from + " helped in your Mosselora garden.");
                return 0;
            };
            Action closed = () =>
            {
                string share = helpShare;
                helpShare = null;
                if (visitLayer != null) { Destroy(visitLayer); visitLayer = null; OpenVisit(visitText); }
                if (share != null && !NativeShare.Text(share, "Mosselora")) GUIUtility.systemCopyBuffer = share;
            };
            PuzzleCard((RectTransform)visitLayer.transform, theirs.Garden * 1000 + item * 7 + 1, tier,
                ThingPuzzle.ForStory(item, (VillageChoice)made[item]), artOf(item, made[item]), thing.Name, -1,
                count => PairsPictures(count, item, artOf), turned, solved, closed);
        }
        private string helpShare;

        // The seeds you have, top right of home: a sprout and a number.
        private void SeedPurse()
        {
            if (titleScreen == null || world.Mode != PlayMode.Living || world.Choices.Length == 0) return;
            var purse = Node("Seeds you have", titleScreen.transform);
            var rect = purse.GetComponent<RectTransform>();
            Rect safe = Screen.safeArea;
            float w = Mathf.Max(1, Screen.width), h = Mathf.Max(1, Screen.height);
            rect.anchorMin = rect.anchorMax = new Vector2(safe.xMax / w, safe.yMax / h);
            rect.pivot = new Vector2(1f, 1f);
            rect.sizeDelta = new Vector2(104f, 48f);
            rect.anchoredPosition = new Vector2(-12f, -ChromeTop - 4f);
            var body = purse.AddComponent<Image>();
            MosseloraArt.Round(body, new Color(0.98f, 0.95f, 0.86f, 0.96f), 24);
            MosseloraArt.Lift(body, 2f, 0.2f);
            SeedPurseTap(purse, body); // tap opens the seed book (EvergroveApp.Economy.cs)
            var row = purse.AddComponent<HorizontalLayoutGroup>();
            row.padding = new RectOffset(8, 12, 4, 4);
            row.spacing = 4;
            row.childAlignment = TextAnchor.MiddleCenter;
            row.childControlWidth = true;
            row.childControlHeight = true;
            row.childForceExpandWidth = false;
            row.childForceExpandHeight = false;
            var icon = Node("Seed", purse.transform);
            var iconLayout = icon.AddComponent<LayoutElement>();
            iconLayout.preferredWidth = iconLayout.preferredHeight = 30f;
            var iconImage = icon.AddComponent<Image>();
            iconImage.sprite = MosseloraArt.Load(TreePlay.SeedIcon);
            iconImage.preserveAspect = true;
            iconImage.raycastTarget = false;
            Text count = Label(purse.transform, "Seed count", world.Seeds.ToString(), 20, 0, Ink, TextAnchor.MiddleLeft);
            Emphasize(count);
            // One line at any text size; the purse grows leftward to fit "100" at 150% (I227).
            count.horizontalOverflow = HorizontalWrapMode.Overflow;
            // Fruit from your trees sits beside the seeds once you have picked any (I229).
            purseFruit = null;
            if (world.Fruit > 0 || fruitPicked)
            {
                var pear = Node("Fruit", purse.transform);
                var pearLayout = pear.AddComponent<LayoutElement>();
                pearLayout.preferredWidth = pearLayout.preferredHeight = 30f;
                purseFruit = pear.AddComponent<Image>();
                purseFruit.sprite = MosseloraArt.Load("food-pear");
                purseFruit.preserveAspect = true;
                purseFruit.raycastTarget = false;
                Text fruit = Label(purse.transform, "Fruit count", world.Fruit.ToString(), 20, 0, Ink, TextAnchor.MiddleLeft);
                Emphasize(fruit);
                fruit.horizontalOverflow = HorizontalWrapMode.Overflow;
            }
            var fit = purse.AddComponent<ContentSizeFitter>();
            fit.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
        }
        // ---- I311/I312: when the garden's season turns, the next neighbour leaves a note with
        // a tale. The first time home is seen only records the season, so nothing is sudden.
        private void NoteSeasonTurn()
        {
            if (world == null || world.Mode != PlayMode.Living || world.Choices.Length == 0) return;
            string seenKey = "evergrove:season-seen:" + world.Garden, turnsKey = "evergrove:season-turns:" + world.Garden;
            int now = (int)GardenSeason.For(world) + 1, seen = PlayerPrefs.GetInt(seenKey, 0);
            if (seen == now) return;
            PlayerPrefs.SetInt(seenKey, now);
            if (seen != 0)
            {
                int turn = PlayerPrefs.GetInt(turnsKey, 0);
                SeasonNote note = SeasonTales.For(GardenSeason.For(world), turn);
                if (world.KeepNote(note.From, note.Body)) { letterArrived = true; SaveWorld(); }
                if (GardenSeason.Festival(world) &&
                    world.KeepNote("Orin", GardenSeason.FestivalNote(GardenSeason.YearAfterStory(world.PuzzlesSolved))))
                { letterArrived = true; SaveWorld(); }
                PlayerPrefs.SetInt(turnsKey, turn + 1);
            }
            PlayerPrefs.Save();
        }

        // I313: a wish that has come true is thanked for once, by letter.
        private void NoteGrantedWishes()
        {
            if (world == null || world.Mode != PlayMode.Living) return;
            for (int villager = 0; villager < 3; villager++)
            {
                NeighbourWish wish = NeighbourWish.For(world, villager);
                string key = "evergrove:wish-thanked:" + world.Garden + ":" + villager;
                if (wish == null || !wish.Granted || PlayerPrefs.GetInt(key, 0) == 1) continue;
                if (!world.KeepNote(wish.From, wish.Thanks)) continue; // review fix: try again next time
                PlayerPrefs.SetInt(key, 1);
                letterArrived = true;
                SaveWorld();
            }
            PlayerPrefs.Save();
        }

        // I315: at a few milestones of puzzles solved, a neighbour leaves a cutting and a note.
        private void NoteNeighbourGifts()
        {
            if (world == null || world.Mode != PlayMode.Living) return;
            string key = "evergrove:neighbour-gifts:" + world.Garden;
            int given = PlayerPrefs.GetInt(key, 0);
            for (int due = NeighbourGifts.Due(world.PuzzlesSolved, given); due > 0; due--, given++)
            {
                int kind = NeighbourGifts.CuttingFor(world, given);
                if (kind < 0 || !world.ReceiveNeighbourCutting(kind)) break; // basket full: it waits for room
                string what = Nursery.Family(kind) == VillageChoice.Bridge
                    ? "I've made you a " + Nursery.Item(kind).Name + ". It's in your basket; set it wherever you like."
                    : "I've left a cutting of " + Nursery.Item(kind).Name + " in your basket, from my own. Plant it wherever you like.";
                world.KeepNote(NeighbourGifts.From(given), what);
                letterArrived = true;
                PlayerPrefs.SetInt(key, given + 1);
                SaveWorld();
            }
            PlayerPrefs.Save();
        }

        // I359: when you come back after two days or more, Mira's next words say, truly, what
        // the weather did meanwhile, and that the garden looked after itself. Said once.
        private DateTime? sinceSeen; // the last day seen before this return, until Mira has said it (SC01)

        private void NoteReturn()
        {
            if (world == null || world.Mode != PlayMode.Living || world.Choices.Length == 0) return;
            string key = "evergrove:last-seen:" + world.Garden;
            string today = DateTime.Now.ToString("yyyyMMdd", CultureInfo.InvariantCulture);
            string seen = PlayerPrefs.GetString(key, "");
            if (seen != today)
            {
                if (seen.Length == 8 && DateTime.TryParseExact(seen, "yyyyMMdd", CultureInfo.InvariantCulture,
                        DateTimeStyles.None, out DateTime last))
                    sinceSeen = last;
                spokenThisVisit.Clear(); // SC02: a new day is a new visit
                PlayerPrefs.SetString(key, today);
                // I419: the hour of each day's first return, counted on this device only.
                int[] hours = HourCounts();
                Now(out int hour, out int minute);
                hours[(int)DayLight.PhaseAt(hour, minute)]++;
                PlayerPrefs.SetString(HoursKey, string.Join(",", Array.ConvertAll(hours, h => Mathf.Min(h, 999).ToString(CultureInfo.InvariantCulture))));
                NoteAnniversary();
                PlayerPrefs.Save();
            }
        }

        // I419: device-only counts of the hour you come (first return of each day), never in the
        // garden file, which travels to family.
        private string HoursKey { get { return "evergrove:hours:" + world.Garden; } }

        private int[] HourCounts()
        {
            var counts = new int[Companion.PhaseCount];
            string[] parts = PlayerPrefs.GetString(HoursKey, "").Split(',');
            for (int i = 0; i < counts.Length && i < parts.Length; i++)
                int.TryParse(parts[i], NumberStyles.Integer, CultureInfo.InvariantCulture, out counts[i]);
            return counts;
        }

        // I421: the garden's anniversary (from the first day it was opened on this device):
        // Orin keeps a note, once that day.
        private void NoteAnniversary()
        {
            string firstKey = "evergrove:first-seen:" + world.Garden;
            string today = DateTime.Now.ToString("yyyyMMdd", CultureInfo.InvariantCulture);
            string first = PlayerPrefs.GetString(firstKey, "");
            if (first.Length != 8) { PlayerPrefs.SetString(firstKey, today); return; }
            if (!DateTime.TryParseExact(first, "yyyyMMdd", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime began)) return;
            int years = Companion.Anniversary(began, DateTime.Now.Date);
            string doneKey = "evergrove:anniversary:" + world.Garden;
            if (years <= 0 || PlayerPrefs.GetInt(doneKey, 0) >= years) return;
            if (world.KeepNote("Orin", Companion.AnniversaryNote(years))) { letterArrived = true; SaveWorld(); }
            PlayerPrefs.SetInt(doneKey, years);
        }

        // I374/I382: the visitors a garden's good neighbours bring, where they'd stand, when
        // they're out. I384: the same for a family garden you visit (your hour, their season).
        private void SetVisitorsFor(WorldModel garden, GardenMap map, System.Collections.Generic.List<PlacedThing> things)
        {
            DayPhase phase = DayLightOn ? CurrentPhase() : DayPhase.Day;
            Season season = GardenSeason.For(garden);
            GardenMapView.HeronAtNext = VisitorSpot(garden, map, things, Visitor.Heron, Pairing.ByTheWater, season, phase);
            GardenMapView.HedgehogAtNext = VisitorSpot(garden, map, things, Visitor.Hedgehog, Pairing.Sheltered, season, phase);
            GardenMapView.OwlAtNext = VisitorSpot(garden, map, things, Visitor.Owl, Pairing.Grove, season, phase);
            GardenMapView.KingfisherAtNext = VisitorSpot(garden, map, things, Visitor.Kingfisher, Pairing.Waterside, season, phase);
        }

        private int VisitorSpot(WorldModel garden, GardenMap map, System.Collections.Generic.List<PlacedThing> things,
            Visitor visitor, Pairing pairing, Season season, DayPhase phase)
        {
            if (!Visitors.Welcomes(garden, visitor) || !Visitors.OutIn(visitor, season, phase, TodayWeather(garden))) return -1;
            return Pairings.SpotFor(map, things, pairing);
        }

        // I372: holding something to place, the free spots where it would make a good-neighbour
        // pairing not yet found. Nothing is held: no spots.
        private int[] PairingSpots(GardenMap map, System.Collections.Generic.List<PlacedThing> things)
        {
            int[] made = world.Choices;
            int moving = -1;
            if (mapInHand >= 0 && mapInHand < made.Length) moving = mapInHand;
            else if (heldPlanting >= 0) moving = made.Length + heldPlanting;
            else if (heldBought >= 0) moving = made.Length + world.PlantingCount + heldBought;
            else if (basketOpen && basketKind >= 0 && basketKind < made.Length)
            {
                things.Add(new PlacedThing(-1, (VillageChoice)Mathf.Clamp(made[basketKind], 0, 2), ItemAt(basketKind, made[basketKind]).Art));
                moving = things.Count - 1;
            }
            else if (nurseryOpen && nurseryKind >= 0 && nurseryKind < Nursery.Kinds)
            {
                things.Add(new PlacedThing(-1, Nursery.Family(nurseryKind), Nursery.Item(nurseryKind).Art));
                moving = things.Count - 1;
            }
            int open = Pairings.Open(world); // I380: deeper pairings glow only once open
            if (moving < 0 || moving >= things.Count || (open & ~world.PairingsFound) == 0) return null;
            var taken = new System.Collections.Generic.HashSet<int>();
            foreach (PlacedThing t in things) taken.Add(t.Cell);
            var spots = new System.Collections.Generic.List<int>();
            for (int c = 0; c < map.CellCount && spots.Count < 40; c++)
                if (map.IsLand(c) && !taken.Contains(c) && Pairings.NewAt(map, things, moving, c, world.PairingsFound | ~open) != 0)
                    spots.Add(c);
            return spots.ToArray();
        }

        // I368: good neighbours found in the garden as it stands: each pays once, with a word.
        // (Also carries I378's word about the bees: whatever the hub should say once, on return.)
        private string pairingFound;
        private Sprite[] toastMarks; // I387: the paintings the toast shows above its words

        // I388-I391: once a season, what the season gives: spring seedlings from a flower bed,
        // summer seeds from a grove, an autumn orchard's fruit, winter jam from neighbours fed.
        private void NoteSeasonYield(GardenMap map)
        {
            if (world == null || world.Mode != PlayMode.Living || map == null) return;
            if (pairingFound != null) return; // one word per return: the season's gift waits for the next
            int last = world.LastFruitTo;
            SeasonYield got = world.TakeSeasonYield(Pairings.Found(world, map));
            if (got == SeasonYield.None) return;
            string giver = last == 0 ? "Mira" : last == 1 ? "Orin" : "Fen";
            switch (got)
            {
                case SeasonYield.Seedling:
                    pairingFound = "Spring: your flower bed has sown itself. A seedling is waiting in your basket.";
                    toastMarks = new[] { MosseloraArt.Load("flower-12"), MosseloraArt.Load("glyph-sprout"), MosseloraArt.Load("food-basket") };
                    break;
                case SeasonYield.Seeds:
                    pairingFound = "Summer: the grove let fall its seeds. +" + SeasonalYields.SummerSeeds + " seeds.";
                    toastMarks = new[] { MosseloraArt.Load("tree-00"), MosseloraArt.Load("tree-05"), MosseloraArt.Load("glyph-sprout") };
                    break;
                case SeasonYield.Fruit:
                    pairingFound = "Autumn: the orchard is ready. " + SeasonalYields.AutumnFruit + " fruit picked.";
                    toastMarks = new[] { MosseloraArt.Load("tree-05"), MosseloraArt.Load("food-apple"), MosseloraArt.Load("food-pear") };
                    break;
                default:
                    pairingFound = "Winter: " + giver + " brought a jar of jam, for the fruit you gave this year.";
                    toastMarks = new[] { MosseloraArt.Load("portrait-" + giver.ToLowerInvariant()), MosseloraArt.Load("food-jam") };
                    break;
            }
            SaveWorld();
            PlayChime();
        }

        // I387: each pairing as two paintings side by side (water as a wet stone).
        private static Sprite[] PairingMarks(Pairing pairing)
        {
            string[] arts;
            switch (pairing)
            {
                case Pairing.Sheltered: arts = new[] { "flower-05", "tree-00" }; break;
                case Pairing.ByTheWater: arts = new[] { "build-00", "stone-wet" }; break;
                case Pairing.FlowerBed: arts = new[] { "flower-05", "flower-12" }; break;
                case Pairing.Grove: arts = new[] { "tree-00", "tree-05" }; break;
                case Pairing.Waterside: arts = new[] { "flower-08", "stone-wet" }; break;
                case Pairing.ShadySeat: arts = new[] { "build-03", "tree-00" }; break;
                default: arts = new[] { "tree-00", "tree-05", "tree-02" }; break;
            }
            var marks = new Sprite[arts.Length];
            for (int i = 0; i < arts.Length; i++) marks[i] = MosseloraArt.Load(arts[i]);
            return marks;
        }

        // I378: a tended tree with a flower beside it, while the bees are about, gives one more fruit.
        // I404 follow-up: only when this solve itself picked fruit, so the bees' extra fruit
        // follows whatever limit governs a tree's own fruit (a replay cap included).
        private void NotePollinated(int thingIndex, bool pickedFruit)
        {
            if (!pickedFruit) return;
            GardenMap map = LoadGardenMap();
            if (map == null) return;
            var things = Pairings.Things(world, map);
            if (world.PickPollinatedFruit(Pairings.Pollinated(map, things, thingIndex), GardenSeason.For(world)))
            {
                pairingFound = "The bees carried pollen from the flower beside it: one more fruit.";
                toastMarks = new[] { MosseloraArt.Load("flower-05"), MosseloraArt.Load("food-pear") };
            }
        }

        private void NotePairings(GardenMap map)
        {
            if (world == null || world.Mode != PlayMode.Living || map == null) return;
            int found = Pairings.Found(world, map);
            foreach (Pairing pairing in Enum.GetValues(typeof(Pairing)))
                if ((found & PairingMask.Of(pairing)) != 0 && world.DiscoverPairing(pairing) > 0)
                {
                    pairingFound = "Good neighbours: " + Pairings.Hint(pairing).ToLowerInvariant().TrimEnd('.') +
                        ". +" + Pairings.RewardFor(pairing) + " seeds.";
                    toastMarks = PairingMarks(pairing);
                    SaveWorld();
                    if (soundOn && sound != null) sound.PlayOneShot(GardenSounds.Neighbours(), 0.9f); // I406
                    else PlayChime();
                }
        }

        private bool GardenMapOnHub(RectTransform window)
        {
            NotePlayedToday();
            NoteReturn();
            NoteSeasonTurn();
            NoteGrantedWishes();
            NoteNeighbourGifts();
            ApplyDayLight(world);
            Sprite plot = MapPainting();
            GardenMap map = LoadGardenMap();
            NotePairings(map); // I368
            NoteSeasonYield(map); // I388-I391
            if (map != null && world.Mode == PlayMode.Living)
            {
                var things = Pairings.Things(world, map);
                GardenMapView.PairLinksNext = Pairings.Links(map, things); // I371
                GardenMapView.PairSpotsNext = PairingSpots(map, things);    // I372
                SetVisitorsFor(world, map, things); // I374, I382
                owlInGrove = Pairings.SpotFor(map, things, Pairing.Grove) >= 0; // I376: heard only while a grove stands
            }
            if (plot == null || map == null) return false;
            float zoom = hubZoom[landscape ? 1 : 0];
            // Only home marks things that hold a puzzle: only home plays them (I221).
            markPuzzles = world.Mode == PlayMode.Living;
            var hubItems = MapItems(map);
            markPuzzles = false;
            GardenMapView.LanternsNext = world.Lanterns.Length;
            // I363: wild flowers on the banks, two for each stream restored, never on a thing.
            int restored = world.Completed ? PuzzleCatalog.Count : world.Round;
            var taken = new System.Collections.Generic.List<int>(map.Layout(world.Choices, world.Placements, world.Occupied));
            taken.AddRange(world.Occupied);
            GardenMapView.MeadowTufts = MeadowLife.Cells(map, MeadowLife.Count(restored), taken.ToArray());
            GardenMapView view = GardenMapView.Build(window, plot, map, hubItems, zoom > 0f ? zoom : landscape ? 1f : 1.3f, OnMapLand, OnMapItem,
                heldDecor >= 0 ? DecorIndex + heldDecor : heldBought >= 0 ? BoughtIndex + heldBought : heldPlanting >= 0 ? PlantingIndex + heldPlanting : mapInHand,
                (justPlanted >= 0 || justBought >= 0) && gentleMotion,
                GreenedShare(0), -1f, (land, point) => NeighborsOnMap(land, point, map, true),
                0.22f, RiverGoesOn(map), NextSeasonPlot());
            hubMapView = view;
            if (pairingFound != null && titleScreen != null)
            {
                RuleToast.Show((RectTransform)titleScreen.transform, pairingFound, font, Ink, MosseloraTheme.CardFill, toastMarks);
                pairingFound = null;
                toastMarks = null;
            }
            // I310: lanterns are passed in before the build (LanternsNext), so nothing lays out twice.
            waveHello = false; // they waved (or would have) on this return; not on every rebuild
            // A solve opened more river (I223): its mist lifts, framed, with the chime.
            if (riverOpened && map.Reaches > 1)
            {
                if (gentleMotion) view.LiftMist(map.Reaches - 1);
                PlayChime();
            }
            riverOpened = false;
            if (fruitPicked && purseFruit != null)
            {
                if (gentleMotion) StartCoroutine(FlyInto(purseFruit.rectTransform, "food-pear", new Vector2(54f, 54f)));
                else BloomPulse.Add(purseFruit.gameObject, 0.1f, 0.22f);
            }
            fruitPicked = false;
            RiverDrops(view, dropEarned && gentleMotion);
            dropEarned = false;
            justBought = -1;
            justSolved = -1;
            // Pinch to zoom (I202), kept while the app runs, per orientation.
            bool sideways = landscape;
            view.Zoomed = z => hubZoom[sideways ? 1 : 0] = z;
            justPlanted = -1;
            if (keepHubAt.x >= 0f) view.KeepAt(keepHubAt);
            keepHubAt = new Vector2(-1f, -1f);
            // Full-screen home (I190): what you made sits in the upper part of the
            // screen, clear of the play token at the bottom.
            // Sideways too (I197): home is the whole garden whichever way you hold it.
            // Sideways the window is short, so what you made sits higher still (I197).
            view.FocusFromTop = landscape ? 0.24f : 0.36f;
            // The play row and the basket live in the bottom of home: neighbours keep out of it (I218).
            view.ClearBottom = landscape ? 0f : 190f;
            // The letterbox stands under the purse (I233): neighbours keep below it too.
            view.ClearTop = landscape ? 0f : world.Letters.Length > 0 ? 220f : 140f; // the box's foot is ~195 pt down
            return true;
        }

        // I189: the garden is the stage. Boards float over your own garden map,
        // full screen and gently veiled so the board stays the brightest thing,
        // instead of a paper page with a strip of garden under the board.
        // The map wears the season the walk has reached (I198): summer, then autumn
        // amber, then winter frost; a finished garden is summer again.
        private Sprite MapPainting()
        {
            // Four seasons by progress (I248/I249): the story walks spring to winter; after it,
            // tending turns the year on.
            Sprite graded = MosseloraArt.Load(PaintedVillage.MapFor(PaintedVillage.ArtSeason(world)));
            return graded != null ? graded : MosseloraArt.Load("garden-map");
        }

        private bool WorldStage(bool show)
        {
            if (worldStage == null) return false;
            ClearChildren(worldStage);
            worldStage.gameObject.SetActive(false);
            if (!show) return false;
            Sprite plot = MapPainting();
            GardenMap map = LoadGardenMap();
            if (plot == null || map == null) return false;
            worldStage.gameObject.SetActive(true);
            GardenMapView view = GardenMapView.Build(worldStage, plot, map, MapItems(map), 1.1f,
                null, null, -1, false, GreenedShare(0), -1f, null, 0f, RiverGoesOn(map));
            view.FocusFromTop = 0.82f; // your newest thing peeks out under the board
            var veil = Node("Morning veil", worldStage);
            Fill(veil.GetComponent<RectTransform>());
            var veilImage = veil.AddComponent<Image>();
            veilImage.color = new Color(Paper.r, Paper.g, Paper.b, 0.2f);
            veilImage.raycastTarget = false;
            return true;
        }

        // ---- E4 (I178): place what you made. Tap a thing to pick it up, tap land to put it down.
        private int mapInHand = -1;
        // ---- I193: the basket. Plant more of anything you've grown, as often as you like.
        private const int PlantingIndex = 1000; // map indices at or above this are plantings
        private bool basketOpen;
        private int basketKind = -1;   // the choice whose item is planted on a tap, -1 none
        private int heldPlanting = -1; // a planting picked up to move or put back
        // ---- I208: the nursery. Seeds buy new plants; each holds its own puzzle.
        private const int BoughtIndex = 2000; // map indices at or above this are bought plants
        private const int SecretIndex = 3000; // I320: map indices at or above this are mossy stones, by stretch
        private const int DecorIndex = 4000;  // I330: map indices at or above this are decorations
        private const int WellIndex = 5000;   // I333: the old well at the river's head
        private int decorInHand = -1;          // a Decoration chosen at the cottage, to place
        private int heldDecor = -1;            // a placed decoration picked up, to move or put away

        private static string DecorArt(Decoration piece)
        {
            return piece == Decoration.Stones ? "build-02" : piece == Decoration.Bench ? "build-17"
                : piece == Decoration.Fence ? "build-19" : "build-22";
        }

        private static string DecorName(Decoration piece)
        {
            return piece == Decoration.Stones ? "stepping stones" : piece == Decoration.Bench ? "a bench"
                : piece == Decoration.Fence ? "a fence" : "a lantern post";
        }
        private int heldBought = -1;   // a bought plant picked up (to move, or to play its puzzle)
        private bool nurseryOpen;      // the basket tray shows the nursery instead of your own kinds
        private int nurseryKind = -1;  // the nursery kind planted (bought) on a tap, -1 none
        private int justBought = -1;
        private int justSolved = -1;   // map index of the thing whose puzzle was just solved (I212)
        private bool solvedHere;
        private bool riverOpened;   // the last solve opened a new stretch of river (I223)
        private TreePlay openCard;  // the puzzle card now open (I264: was it played together?)
        private PuzzleKind togetherKind;
        private int togetherKeeper = -1; // I266: the neighbour who'll say you played it together
        private bool dropEarned;    // the last solve was a first solve: a drop on the mist fills (I224)
        private bool fruitPicked;   // the last solve picked fruit from a tree (I229)
        private Image purseFruit;   // the pear in the purse, where picked fruit flies (I229)

        // Drops on the next stretch's mist (I224): how close more river is.
        private void RiverDrops(GardenMapView view, bool swell)
        {
            if (view == null || world.Mode != PlayMode.Living) return;
            if (GardenMap.TowardNext(world.Choices.Length, world.Completed, world.PuzzlesSolved, out int have, out int need))
                view.ShowDrops(have, need, swell);
        }
        private int OpenReaches() { return GardenMap.ReachesFor(world.Choices.Length, world.Completed, world.PuzzlesSolved); }

        // Everything standing on the map besides your story things: plantings, then bought plants.
        private int[] Occupied()
        {
            return world.Occupied; // plantings, bought plants and, since I330, decorations
        }
        private int justPlanted = -1;  // rises from the ground once
        private int justDecorated = -1; // I330
        private GardenMapView hubMapView;
        private Vector2 keepHubAt = new Vector2(-1f, -1f);

        private bool PlantedAt(int cell, int except = -1, int exceptBought = -1)
        {
            int[] planted = world.Plantings;
            for (int k = 0; k * 2 + 1 < planted.Length; k++)
                if (k != except && planted[k * 2 + 1] == cell) return true;
            int[] bought = world.Bought;
            for (int k = 0; k * 2 + 1 < bought.Length; k++)
                if (k != exceptBought && bought[k * 2 + 1] == cell) return true;
            return false;
        }

        private System.Collections.Generic.List<GardenMapView.Placed> MapItems(GardenMap map)
        {
            int[] made = world.Choices;
            int[] planted = world.Plantings;
            int[] cells = map.Layout(made, world.Placements, Occupied());
            var items = new System.Collections.Generic.List<GardenMapView.Placed>();
            for (int i = 0; i < made.Length; i++)
                items.Add(new GardenMapView.Placed
                {
                    Art = ItemArt(i, made[i]),
                    Cell = cells[i],
                    Index = i,
                    Family = (VillageChoice)Mathf.Clamp(made[i], 0, 2),
                    Newest = justSolved >= 0 ? i == justSolved : i == made.Length - 1 && justPlanted < 0,
                    Bloomed = world.TreeSolved(i) || world.IsHelped(i), // solved (I204), or by family (I227)
                    HoldsPuzzle = markPuzzles && !world.TreeSolved(i), // helped still holds your own first solve
                    Kind = (int)ThingPuzzle.ForStory(i, (VillageChoice)Mathf.Clamp(made[i], 0, 2)) + 1, // I255
                    Together = world.SolvedTogether(i), // I265
                    FlowerIndex = made[i] == (int)VillageChoice.Flowers ? ItemAt(i, made[i]).Index + 1 : 0, // I338
                    Crowned = world.TreeSolved(i) && ReplayBoards.Crowned(TreePuzzles.TierFor(i), Replays(i)), // I292
                    Grown = world.TreeSolved(i) ? ReplayBoards.Maturity(TreePuzzles.TierFor(i), Replays(i)) : 1f, // I300
                    BloomNow = i == justSolved && gentleMotion
                });
            // More of what you grew, planted from the basket (I193).
            for (int k = 0; k * 2 + 1 < planted.Length; k++)
            {
                int source = planted[k * 2];
                if (source < 0 || source >= made.Length || !map.IsLand(planted[k * 2 + 1])) continue;
                items.Add(new GardenMapView.Placed
                {
                    Art = ItemArt(source, made[source]),
                    Cell = planted[k * 2 + 1],
                    Index = PlantingIndex + k,
                    Family = (VillageChoice)Mathf.Clamp(made[source], 0, 2),
                    Newest = k == justPlanted
                });
            }
            // Plants from the nursery (I208): each holds its own puzzle.
            int[] bought = world.Bought;
            for (int k = 0; k * 2 + 1 < bought.Length; k++)
            {
                int kind = bought[k * 2];
                if (kind < 0 || kind >= Nursery.Kinds || !map.IsLand(bought[k * 2 + 1])) continue;
                items.Add(new GardenMapView.Placed
                {
                    Art = MosseloraArt.Load(Nursery.Item(kind).Art),
                    Cell = bought[k * 2 + 1],
                    Index = BoughtIndex + k,
                    Family = Nursery.Family(kind),
                    Newest = justSolved >= 0 ? BoughtIndex + k == justSolved : k == justBought,
                    Bloomed = world.BoughtSolved(k),
                    HoldsPuzzle = markPuzzles && !world.BoughtSolved(k),
                    Kind = (int)ThingPuzzle.ForNursery(kind) + 1, // I255
                    Together = world.SolvedTogether(BoughtIndex + k), // I265
                    FlowerIndex = Nursery.Family(kind) == VillageChoice.Flowers ? Nursery.Item(kind).Index + 1 : 0, // I338
                    Crowned = world.BoughtSolved(k) && ReplayBoards.Crowned(Nursery.Row(kind), Replays(BoughtIndex + k)), // I292
                    Grown = world.BoughtSolved(k) ? ReplayBoards.Maturity(Nursery.Row(kind), Replays(BoughtIndex + k)) : 1f, // I300
                    BloomNow = BoughtIndex + k == justSolved && gentleMotion
                });
            }
            // I330: decorations, free to place and move; drawn as the catalogue's own paintings.
            int[] decor = world.Decor;
            for (int d = 0; d * 2 + 1 < decor.Length; d++)
            {
                if (!map.IsLand(decor[d * 2 + 1])) continue;
                items.Add(new GardenMapView.Placed { Art = MosseloraArt.Load(DecorArt((Decoration)decor[d * 2])),
                    Cell = decor[d * 2 + 1], Index = DecorIndex + d, Family = VillageChoice.Bridge,
                    Newest = d == justDecorated });
            }
            // I333: the old well at the head of the river, first free land at the top left.
            if (markPuzzles)
            {
                var wellTaken = new System.Collections.Generic.List<int>(cells);
                for (int k = 1; k < planted.Length; k += 2) wellTaken.Add(planted[k]);
                for (int k = 1; k < bought.Length; k += 2) wellTaken.Add(bought[k]);
                for (int k = 1; k < decor.Length; k += 2) wellTaken.Add(decor[k]);
                int wellCell = -1;
                for (int row = 1; row < 4 && wellCell < 0; row++)
                    for (int col = 0; col < 3 && wellCell < 0; col++)
                    {
                        int c = row * map.Columns + col;
                        if (map.IsLand(c) && !wellTaken.Contains(c)) wellCell = c;
                    }
                if (wellCell >= 0)
                    items.Add(new GardenMapView.Placed { Art = MosseloraArt.Load("build-05"), Cell = wellCell, Index = WellIndex,
                        Family = VillageChoice.Bridge, HoldsPuzzle = OldWell.Waiting(world) > 0,
                        Kind = (int)OldWell.KindFor(world.WellSolved) + 1 });
            }
            // I320: one mossy stone on the bank of each stretch after the first, until found.
            if (markPuzzles)
            {
                var taken = new System.Collections.Generic.List<int>(cells);
                for (int k = 1; k < planted.Length; k += 2) taken.Add(planted[k]);
                for (int k = 1; k < bought.Length; k += 2) taken.Add(bought[k]);
                Sprite moss = MosseloraArt.Load("stone-spring");
                for (int reach = 1; reach < map.Reaches && moss != null; reach++)
                {
                    int cell = RiverSecrets.CellFor(map, reach, taken.ToArray());
                    if (cell < 0 || SecretFound(reach)) continue;
                    items.Add(new GardenMapView.Placed { Art = moss, Cell = cell, Index = SecretIndex + reach,
                        Family = VillageChoice.Bridge });
                }
            }
            return items;
        }

        // I333: the well's next deepest board; two fruit when solved, once.
        private void OpenWell()
        {
            if (titleScreen == null) return;
            if (OldWell.Waiting(world) <= 0)
            {
                Transform hub = titleScreen.transform;
                RuleToast.Show((RectTransform)hub, "The well is still. New river will fill it again.", font, Ink, MosseloraTheme.CardFill);
                return;
            }
            int index = world.WellSolved;
            OpenThingPuzzle(OldWell.SeedFor(world.Garden, index), OldWell.Tier, OldWell.KindFor(index),
                MosseloraArt.Load("build-05"), "the old well", 0, index, () =>
                {
                    int fruitBefore = world.Fruit;
                    if (world.TendWell() > 0) { SaveWorld(); PlayChime(); fruitPicked = world.Fruit > fruitBefore; }
                    return 0;
                });
            if (openCard != null) openCard.PaysFruit(OldWell.FruitPerBoard);
        }

        private string StoneKey(int reach) { return "evergrove:mossy-stone:" + world.Garden + ":" + reach; }
        private bool SecretFound(int reach) { return PlayerPrefs.GetInt(StoneKey(reach), 0) == 1; }

        // I320: lift the mossy stone: a lost cutting goes to the basket, with a note from Fen.
        private void FindSecret(int reach)
        {
            if (SecretFound(reach) || world.Mode != PlayMode.Living) return;
            int kind = RiverSecrets.CuttingFor(world, reach);
            if (kind < 0 || !world.ReceiveNeighbourCutting(kind))
            {
                if (titleScreen != null)
                    RuleToast.Show((RectTransform)titleScreen.transform, "Your basket is full. Plant something first; the stone will wait.",
                        font, Ink, MosseloraTheme.CardFill);
                return;
            }
            PlayerPrefs.SetInt(StoneKey(reach), 1);
            PlayerPrefs.Save();
            string what = Nursery.Family(kind) == VillageChoice.Bridge ? "a little " + Nursery.Item(kind).Name
                : "a cutting of " + Nursery.Item(kind).Name;
            world.KeepNote("Fen", "You found what was under the mossy stone: " + what +
                ", lost there long ago. It's in your basket now.");
            letterArrived = true;
            SaveWorld();
            PlayChime();
            RefreshMaps();
        }

        // Mira, Orin and Fen stand in the garden (I181): each beside the newest thing
        // of their family (trees, builds, flowers), or by the cottage before that.
        // ---- I217: letters from family stay in your garden: a small letterbox on the
        // lane below the cottage. A tap reads them, newest first, one at a time.
        private const string LettersReadKey = "evergrove:letters-read";
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
            button.onClick.AddListener(() => ShowLetter(0));
            PressSquash.Add(node);
            letterboxRect = rect;
            if (PlayerPrefs.GetInt(LettersReadKey, 0) < letters.Length)
            {
                Image sealedLetter = MosseloraArt.Ornament(node.transform, "Unread letter", MosseloraArt.Load("letter-sealed"),
                    new Vector2(0.5f, 1f), new Vector2(0.5f, 0.5f), new Vector2(44f, 34f), new Vector2(-4f, -8f));
                if (sealedLetter != null && gentleMotion) BloomPulse.Add(sealedLetter.gameObject, 0.6f, 0.08f);
            }
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
            if (letterCard != null) Destroy(letterCard);
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
                if (card == null || sealedLetter == null) yield break;
                float k = Mathf.SmoothStep(0f, 1f, t);
                sealedLetter.transform.localScale = Vector3.one * Mathf.Lerp(0.82f, 1.06f, k);
                sealedLetter.transform.localRotation = Quaternion.Euler(0f, 0f, Mathf.Sin(t * Mathf.PI * 3f) * 4f * (1f - k));
                yield return null;
            }
            if (card == null) yield break;
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
            column.padding = new RectOffset(22, 22, 20, 18);
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
            Transform row = CenteredRowIn(paper.transform, "Letter actions");
            if (fromNewest < letters.Length - 1)
                IconButton(row, "Older letter", "Older letter", MosseloraArt.IconUndo, () => ShowLetter(letterShown + 1),
                    56, MosseloraTheme.QuietAction, false, null, false, 0, 0, true);
            IconButton(row, "Close letter", GameLocalization.Text("close"), MosseloraArt.IconConfirm, CloseLetter,
                56, MosseloraTheme.ForestAction, false, null, false, 0, 0, true);
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
            if (rise) StartCoroutine(RiseIn(paperRect));
            semanticsDirty = true;
        }

        private void WriteBack(string to)
        {
            CloseLetter();
            OpenFamily();
            if (familySheet != null) familySheet.Prefill("Dear " + to + ",\n");
        }

        private void CloseLetter()
        {
            if (letterCard != null) Destroy(letterCard);
            letterCard = null;
            letterShown = -1;
            RefreshMaps(); // the unread dot goes
        }

        internal void QaReadLetter() { ShowLetter(0); }
        internal void QaCloseLetter() { CloseLetter(); }
        internal void QaWriteBack() { string[] from = world.LetterFrom; if (from.Length > 0) WriteBack(from[from.Length - 1]); }

        private System.Collections.Generic.List<RectTransform> NeighborsOnMap(RectTransform land,
            Func<int, Vector2> point, GardenMap map, bool fromTitle)
        {
            var placed = new System.Collections.Generic.List<RectTransform>();
            if (world.Mode != PlayMode.Living) return placed;
            int[] made = world.Choices;
            int[] cells = map.Layout(made, world.Placements);
            // Before they have a thing of their own, each waits in a different
            // part of the valley (land cells in the mask), never in a huddle.
            // Row 2 col 5 (Mira), row 3 col 6 (Orin), row 6 col 7 (Fen): clear of the
            // corner buttons and of each other, on land.
            int[] byCottage = { 25, 36, 67 };
            Vector2 step = point(1) - point(0);
            // Those with a thing of their own take their place first; newcomers fit around them.
            var order = new System.Collections.Generic.List<int> { 0, 1, 2 };
            order.Sort((a, b) => (Array.IndexOf(made, b) >= 0).CompareTo(Array.IndexOf(made, a) >= 0) != 0
                ? (Array.IndexOf(made, b) >= 0).CompareTo(Array.IndexOf(made, a) >= 0) : a.CompareTo(b));
            foreach (int villager in order)
            {
                // I301: beside the newest thing of their kind, a nursery plant of theirs first.
                int cell = -1;
                int beside = NeighbourNotice.StandBy(world, villager, out bool besideBought);
                int[] boughtNow = world.Bought;
                if (beside >= 0 && besideBought && beside * 2 + 1 < boughtNow.Length && map.IsLand(boughtNow[beside * 2 + 1]))
                    cell = boughtNow[beside * 2 + 1];
                else
                    for (int i = made.Length - 1; i >= 0 && cell < 0; i--)
                        if (made[i] == villager) cell = cells[i];
                Vector2 at = cell >= 0
                    // I397 (b320 tour: Fen stood half on her birch): a painting is wider than its
                    // cell, so they stand a step and a quarter beside it, toward the middle.
                    ? point(cell) + new Vector2(step.x * (cell % map.Columns < map.Columns / 2 ? 1.25f : -1.25f), 0f)
                    : point(Mathf.Min(byCottage[villager], map.CellCount - 1));
                // Nothing of their own yet: they come to see the newest thing you made,
                // which the map always opens on, so they are in view (I187).
                float rowStep = Mathf.Abs((point(map.Columns) - point(0)).y);
                if (cell < 0 && made.Length > 0 && cells[made.Length - 1] >= 0)
                {
                    // Try the spots around it in turn: either side, then a step up.
                    Vector2 newestAt = point(cells[made.Length - 1]);
                    // Toward the middle of the valley first, so nobody stands off the edge (I188).
                    float inward = cells[made.Length - 1] % map.Columns < map.Columns / 2 ? 1f : -1f;
                    Vector2[] around = {
                        new Vector2(inward * 1.15f * step.x, 0f), new Vector2(inward * 2.5f * step.x, 0f),
                        new Vector2(inward * 1.2f * step.x, -1.6f * rowStep), new Vector2(inward * 1.2f * step.x, 1.6f * rowStep),
                        new Vector2(inward * 3.8f * step.x, 0f), new Vector2(-inward * 1.15f * step.x, 0f) };
                    at = newestAt + around[0];
                    foreach (Vector2 offset in around)
                    {
                        Vector2 spot = newestAt + offset;
                        bool taken = false;
                        foreach (RectTransform other in placed)
                            if (Mathf.Abs(other.anchoredPosition.x - spot.x) < Mathf.Abs(step.x) * 1.3f &&
                                Mathf.Abs(other.anchoredPosition.y - spot.y) < rowStep * 1.3f) taken = true;
                        if (!taken) { at = spot; break; }
                    }
                }
                // I402: in the golden hour they sit by the shady seat, if the garden has one.
                if (fromTitle && DayLightOn && CurrentPhase() == DayPhase.Golden && world.HasPairing(Pairing.ShadySeat))
                {
                    int seatCell = Pairings.SpotFor(map, Pairings.Things(world, map), Pairing.ShadySeat);
                    if (seatCell >= 0)
                    {
                        float side = villager == 0 ? -1.3f : villager == 1 ? 1.3f : 0f;
                        at = point(seatCell) + new Vector2(side * step.x, villager == 2 ? -1.4f * rowStep : 0f);
                    }
                }
                // I367: in the evening they gather on the path below the lamplit cottage door.
                if (fromTitle && DayLightOn && DayLight.NeighboursGather(CurrentPhase()))
                {
                    int[] byDoor = { 2 * map.Columns + 7, 3 * map.Columns + 8, 2 * map.Columns + 5 };
                    int spot = byDoor[villager % byDoor.Length];
                    if (spot < map.CellCount) at = point(spot);
                }
                // Never stand on another neighbour (I184): step down a row and a half
                // until clear of everyone already placed.
                for (int tries = 0; tries < 4; tries++)
                {
                    bool clash = false;
                    foreach (RectTransform other in placed)
                        if (Mathf.Abs(other.anchoredPosition.x - at.x) < Mathf.Abs(step.x) * 1.3f &&
                            Mathf.Abs(other.anchoredPosition.y - at.y) < rowStep * 1.3f) clash = true;
                    if (!clash) break;
                    at.y -= rowStep * 1.5f;
                }
                var holder = Node("Neighbour on the map", land);
                var rect = holder.GetComponent<RectTransform>();
                rect.anchorMin = rect.anchorMax = new Vector2(0f, 1f);
                rect.pivot = new Vector2(0.5f, 0f);
                rect.anchoredPosition = at;
                // A fixed footprint (no nested size fitters) keeps face and name centred.
                rect.sizeDelta = new Vector2(TextScale.Scale(96), 62f + TextScale.Scale(30));
                var stack = holder.AddComponent<VerticalLayoutGroup>();
                stack.childAlignment = TextAnchor.LowerCenter;
                stack.childControlWidth = true;
                stack.childControlHeight = true;
                stack.childForceExpandWidth = true;
                stack.childForceExpandHeight = true;
                PortraitButton(holder.transform, villager, true, fromTitle);
                // I298 (idea 35, presence without words): when you come back, they look up and
                // give a little hop, one after another. Once per return; never with reduced motion.
                if (fromTitle && waveHello && gentleMotion)
                    holder.AddComponent<HelloHop>().Delay = 0.35f + placed.Count * 0.28f;
                placed.Add(rect);
            }
            return placed;
        }

        // What a neighbour says. Smart companion: the true facts of the save and the day
        // (CompanionFacts, SC01), the one worth saying now (CompanionRanker, SC02), in today's
        // words (I184, I210, I236, I266, I281, I357–I359, I370). With nothing worth saying, or
        // outside English, their authored lines (LocalizedVillage).
        private string NeighborSays(int villager)
        {
            Fact pick = CompanionPick(villager);
            return pick != null ? CompanionSpeech.Legacy(pick, villager) : LocalizedVillage.Speak(world, villager);
        }

        // ---- SC02: device-local memory of what was said (PlayerPrefs; never the save or the garden file).
        private const string CompanionMemoryKey = "evergrove:companion-heard:";
        private readonly System.Collections.Generic.Dictionary<int, string> spokenThisVisit =
            new System.Collections.Generic.Dictionary<int, string>();
        private int visitGarden = int.MinValue;

        private Fact CompanionPick(int villager)
        {
            if (world == null || world.Mode != PlayMode.Living || GameLocalization.Current != "en") return null;
            if (visitGarden != world.Garden) { spokenThisVisit.Clear(); visitGarden = world.Garden; }
            CompanionContext context = CompanionNow();
            spokenThisVisit.TryGetValue(villager, out string spoken);
            return CompanionRanker.Pick(CompanionFacts.For(world, context), CompanionMemoryNow(), villager,
                context.DayNumber, world.Garden, spoken, (fact, who) => CompanionSpeech.Legacy(fact, who) != null);
        }

        private CompanionMemory CompanionMemoryNow()
        {
            return CompanionMemory.Parse(PlayerPrefs.GetString(CompanionMemoryKey + world.Garden, ""));
        }

        // Called when a conversation opens: the remark is now heard, and is theirs for this visit.
        private void CompanionHeard(int villager, Fact said)
        {
            if (said == null || world == null) return;
            spokenThisVisit[villager] = said.Key;
            CompanionMemory memory = CompanionMemoryNow().With(
                new Heard(CompanionContext.DayNumberOf(CompanionToday()), villager, said.Kind, -1, said.Key));
            PlayerPrefs.SetString(CompanionMemoryKey + world.Garden, memory.Serialize());
        }

        // What the companion needs that the save doesn't hold (SC01).
        private CompanionContext CompanionNow()
        {
            bool living = world != null && world.Mode == PlayMode.Living;
            return new CompanionContext(CompanionToday(),
                lastSeen: sinceSeen,
                lettersRead: PlayerPrefs.GetInt(LettersReadKey, 0),
                weather: TodayWeather(world),
                lightOn: DayLightOn && living,
                phase: CurrentPhase(),
                fruitJustGivenTo: fruitThanks,
                hourCounts: living ? HourCounts() : null,
                togetherKeeper: togetherKeeper,
                togetherKind: togetherKind,
                map: living ? LoadGardenMap() : null);
        }

        // How much of the valley the restored water has greened (I180): the
        // cottage's top rows from the start, then a twelfth per stream restored.
        // `back` counts that many streams fewer (the edge before a choice).
        private float GreenedShare(int back)
        {
            // The river's open reaches are all green; new land arrives as a new
            // reach (I192) rather than as the dry edge drawing back.
            if (world.Completed || world.Mode == PlayMode.Living) return 1f;
            int restored = Mathf.Max(0, world.Choices.Length - back);
            return Mathf.Clamp01(0.14f + 0.86f * restored / (float)GardenCatalog.ChaptersPerGarden);
        }

        // Right after a choice the new thing is in your hand until you put it down.
        private int ItemInHand()
        {
            if (mapInHand >= 0) return mapInHand;
            int[] placed = world.Placements;
            int newest = world.Choices.Length - 1;
            bool justMade = showingMemory && newest >= 0 && (newest >= placed.Length || placed[newest] < 0);
            return justMade ? newest : -1;
        }

        private void OnMapItem(int index)
        {
            if (index == WellIndex) { decorInHand = -1; heldDecor = -1; OpenWell(); return; }
            if (index >= DecorIndex)
            {
                // I330: tap a decoration to pick it up; tap it again to put it away.
                int d = index - DecorIndex;
                if (heldDecor == d) { if (world.RemoveDecor(d)) SaveWorld(); heldDecor = -1; }
                else { heldDecor = d; decorInHand = -1; }
                RefreshMaps();
                return;
            }
            decorInHand = -1; // review fix: picking up anything else lets go of a decoration
            heldDecor = -1;
            if (index >= SecretIndex) { FindSecret(index - SecretIndex); return; }
            if (index >= BoughtIndex)
            {
                // A bought plant: pick it up (to move it, or play its puzzle), or put it down.
                int k = index - BoughtIndex;
                heldBought = heldBought == k ? -1 : k;
                heldPlanting = -1;
                mapInHand = -1;
                RefreshMaps();
                return;
            }
            heldBought = -1;
            if (index >= PlantingIndex)
            {
                // A planting: pick it up (or put it down again where it is).
                int k = index - PlantingIndex;
                heldPlanting = heldPlanting == k ? -1 : k;
                mapInHand = -1;
                RefreshMaps();
                return;
            }
            heldPlanting = -1;
            mapInHand = ItemInHand() == index ? -2 : index; // -2: put nothing in hand
            RefreshMaps();
            if (mapInHand == -2) mapInHand = -1;
        }

        private void OnMapLand(int cell)
        {
            GardenMap map = LoadGardenMap();
            if (map == null) return;
            int[] layout = map.Layout(world.Choices, world.Placements, Occupied());
            if (decorInHand >= 0 || heldDecor >= 0)
            {
                // I330: set a decoration down (or move one): free land only, never on a thing.
                bool done = heldDecor >= 0 ? world.MoveDecor(heldDecor, cell, map)
                    : world.PlaceDecor((Decoration)decorInHand, cell, map);
                if (!done)
                {
                    // Review fix: never a silent lock. A tap where it can't go puts it back.
                    decorInHand = -1;
                    heldDecor = -1;
                    if (titleScreen != null)
                        RuleToast.Show((RectTransform)titleScreen.transform, "It can't stand there, so it went back to the cottage.",
                            font, Ink, MosseloraTheme.CardFill);
                    RefreshMaps();
                    return;
                }
                justDecorated = heldDecor >= 0 ? heldDecor : world.DecorCount - 1;
                decorInHand = -1;
                heldDecor = -1;
                SaveWorld();
                PlayTock();
                RefreshMaps();
                return;
            }
            if (heldBought >= 0)
            {
                // Move a bought plant: only to free land.
                if (!map.CanPlace(layout, cell) || PlantedAt(cell, -1, heldBought)) return;
                if (!world.MoveBought(heldBought, cell)) return;
                heldBought = -1;
                PlantDone();
                return;
            }
            if (giftPick >= 0 && giftPick < world.Gifts.Length && ItemInHand() < 0)
            {
                // Plant a cutting a relative sent: it becomes a nursery plant of yours.
                if (!map.CanPlace(layout, cell) || PlantedAt(cell)) return;
                if (!world.PlantGift(giftPick, cell)) return;
                RecordInAlmanac();
                justBought = world.BoughtCount - 1;
                giftPick = -1;
                PlayChime();
                PlantDone();
                return;
            }
            if (nurseryOpen && nurseryKind >= 0 && ItemInHand() < 0)
            {
                // Buy and plant from the nursery; it stays picked while you can afford another.
                if (!map.CanPlace(layout, cell) || PlantedAt(cell)) return;
                if (!world.Buy(nurseryKind, cell)) return;
                RecordInAlmanac();
                justBought = world.BoughtCount - 1;
                // The slot restocks with its next plant (I240); keep it picked while affordable.
                nurseryKind = world.NurseryStock(Nursery.Slot(nurseryKind));
                if (world.Seeds < Nursery.Price(nurseryKind)) nurseryKind = -1;
                PlayChime();
                PlantDone();
                return;
            }
            if (heldPlanting >= 0)
            {
                // Move a planting: only to free land.
                if (!map.CanPlace(layout, cell) || PlantedAt(cell, heldPlanting)) return;
                if (!world.MovePlanting(heldPlanting, cell)) return;
                heldPlanting = -1;
                PlantDone();
                return;
            }
            // I299: your cottage, tapped with nothing in hand: what's waiting, at a glance.
            if (ItemInHand() < 0 && !basketOpen && titleScreen != null && map.IsCottage(cell))
            {
                OpenCottage();
                return;
            }
            int target = ItemInHand();
            if (target < 0 && basketOpen && basketKind >= 0)
            {
                // Plant one more from the basket; the basket stays ready for the next tap.
                if (!map.CanPlace(layout, cell) || PlantedAt(cell)) return;
                if (!world.Plant(basketKind, cell)) return;
                justPlanted = world.PlantingCount - 1;
                PlantDone();
                return;
            }
            if (target < 0) return;
            if (!map.CanPlace(layout, cell, target) || PlantedAt(cell)) return; // water, the cottage, or taken: it stays in hand
            if (!world.PlaceAt(target, cell)) return;
            PlayerPrefs.SetInt(PlacedOnceKey, 1);
            SaveWorld();
            mapInHand = -1;
            PlayTock();
            if (gentleMotion) Haptics.Tick();
            RefreshMaps();
        }

        private void PlantDone()
        {
            SaveWorld();
            PlayTock();
            if (gentleMotion) Haptics.Tick();
            RefreshMaps();
        }

        // Put a picked-up planting back in the basket.
        private void PutBack()
        {
            if (heldPlanting < 0 || !world.RemovePlanting(heldPlanting)) return;
            heldPlanting = -1;
            PlantDone();
        }

        // The basket on home (I193): a round token bottom left. Open, it shows a tray
        // of everything you've grown; pick one and tap the grass to plant more of it.
        private void BasketControls(Transform screen, Transform stack)
        {
            int[] made = world.Choices;
            if (world.Mode != PlayMode.Living || made.Length == 0) return;
            var corner = Node("Basket corner", screen);
            var cornerRect = corner.GetComponent<RectTransform>();
            Rect safe = Screen.safeArea;
            float w = Mathf.Max(1, Screen.width), h = Mathf.Max(1, Screen.height);
            cornerRect.anchorMin = new Vector2(safe.xMin / w, safe.yMin / h);
            cornerRect.anchorMax = new Vector2(safe.xMax / w, safe.yMin / h);
            cornerRect.pivot = new Vector2(0.5f, 0f);
            cornerRect.sizeDelta = new Vector2(0f, basketOpen ? 104f : 76f);
            cornerRect.anchoredPosition = new Vector2(0f, 10f);
            if (basketOpen)
            {
                stack.gameObject.SetActive(false);
                var tray = Node("Seed tray", corner.transform);
                var trayRect = tray.GetComponent<RectTransform>();
                Fill(trayRect);
                trayRect.offsetMin = new Vector2(86f, 0f);
                trayRect.offsetMax = new Vector2(-10f, 0f);
                var trayImage = tray.AddComponent<Image>();
                MosseloraArt.Round(trayImage, MosseloraTheme.CardFill, 26);
                MosseloraArt.Lift(trayImage, 2f, 0.18f);
                tray.AddComponent<RectMask2D>();
                var trayScroll = tray.AddComponent<ScrollRect>();
                trayScroll.horizontal = true;
                trayScroll.vertical = false;
                trayScroll.movementType = ScrollRect.MovementType.Clamped;
                var row = Node("Tray row", tray.transform);
                var rowRect = row.GetComponent<RectTransform>();
                rowRect.anchorMin = new Vector2(0f, 0f);
                rowRect.anchorMax = new Vector2(0f, 1f);
                rowRect.pivot = new Vector2(0f, 0.5f);
                rowRect.sizeDelta = Vector2.zero;
                var rowGroup = row.AddComponent<HorizontalLayoutGroup>();
                rowGroup.padding = new RectOffset(12, 12, 10, 10);
                rowGroup.spacing = 10;
                rowGroup.childAlignment = TextAnchor.MiddleLeft;
                rowGroup.childControlWidth = true;
                rowGroup.childControlHeight = true;
                rowGroup.childForceExpandWidth = false;
                rowGroup.childForceExpandHeight = false;
                row.AddComponent<ContentSizeFitter>().horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
                trayScroll.content = rowRect;
                trayScroll.viewport = trayRect;
                // The nursery (I208): a seed-bag token first in the tray switches it
                // between your own kinds and the plants seeds can buy.
                Button bag = IconButton(row.transform, nurseryOpen ? "Your own" : "Nursery",
                    nurseryOpen ? "Your own" : "Nursery", TreePlay.SeedIcon, () => {
                        nurseryOpen = !nurseryOpen; nurseryKind = -1; basketKind = -1; heldPlanting = -1; RefreshMaps();
                    }, 74, nurseryOpen ? MosseloraTheme.ForestAction : new Color(0.93f, 0.88f, 0.72f, 1f),
                    false, null, false, 0, 0, true);
                Transform bagGlyph = bag.transform.Find("Icon");
                if (bagGlyph != null) bagGlyph.GetComponent<Image>().color = Color.white;
                // Shop, not plant (I213): the sprout wears the same seed chip as the
                // purse and the prices, so the token reads "spend seeds here".
                if (!nurseryOpen) PriceChip(bag.transform, world.Seeds.ToString(), true, true);
                if (nurseryOpen) NurseryTokens(row.transform);
                else GiftTokens(row.transform);
                for (int i = nurseryOpen ? -1 : made.Length - 1; i >= 0; i--) // your own kinds, newest first
                {
                    int kind = i;
                    Sprite art = ItemArt(i, made[i]);
                    Button seed = IconButton(row.transform, "Basket " + i, ItemAt(i, made[i]).Name, MosseloraArt.IconRooted,
                        () => { basketKind = basketKind == kind ? -1 : kind; heldPlanting = -1; giftPick = -1; RefreshMaps(); },
                        74, kind == basketKind ? MosseloraTheme.ForestAction : MosseloraTheme.QuietAction,
                        false, null, false, 0, 0, true);
                    Transform glyph = seed.transform.Find("Icon");
                    Image glyphImage = glyph != null ? glyph.GetComponent<Image>() : null;
                    if (glyphImage != null && art != null)
                    {
                        glyphImage.sprite = art;
                        glyphImage.color = Color.white;
                        var glyphSize = glyph.GetComponent<LayoutElement>();
                        glyphSize.minWidth = glyphSize.preferredWidth = 50f;
                        glyphSize.minHeight = glyphSize.preferredHeight = 50f;
                    }
                    if (kind == basketKind && gentleMotion) BloomPulse.Add(seed.gameObject, 0.9f, 0.06f);
                }
            }
            // The basket itself: open, close, or (holding a planting) put it back.
            bool holding = heldPlanting >= 0;
            string glyphName = holding ? MosseloraArt.IconUndo : basketOpen ? MosseloraArt.IconCancel : MosseloraArt.IconRooted;
            Button basket = IconButton(corner.transform, holding ? "Put back" : basketOpen ? "Close basket" : "Plant more",
                holding ? "Put back" : basketOpen ? GameLocalization.Text("close") : "Plant more", glyphName, () => {
                    if (heldPlanting >= 0) { PutBack(); return; }
                    basketOpen = !basketOpen;
                    basketKind = basketOpen ? world.Choices.Length - 1 : -1;
                    if (!basketOpen) { nurseryOpen = false; nurseryKind = -1; }
                    RefreshMaps();
                }, 72, holding || basketOpen ? MosseloraTheme.ForestAction : MosseloraTheme.QuietAction,
                false, null, false, 0, 0, true);
            var basketRect = basket.GetComponent<RectTransform>();
            basketRect.anchorMin = basketRect.anchorMax = new Vector2(0f, 0f);
            basketRect.pivot = new Vector2(0f, 0f);
            basketRect.anchoredPosition = new Vector2(10f, basketOpen ? 16f : 2f);
            basketRect.sizeDelta = new Vector2(72f, 72f);
            if (!basketOpen && !holding)
            {
                // I345 (run 3, P3): closed, it reads as a basket (the painted one) with the
                // newest thing you grew peeking out of it, so "things to plant" says itself.
                Transform glyph = basket.transform.Find("Icon");
                Image glyphImage = glyph != null ? glyph.GetComponent<Image>() : null;
                Sprite newest = ItemArt(made.Length - 1, made[made.Length - 1]);
                Sprite basketArt = MosseloraArt.Load("food-basket");
                if (glyphImage != null && basketArt != null)
                {
                    glyphImage.sprite = basketArt;
                    glyphImage.color = Color.white;
                    ((RectTransform)glyph).anchoredPosition = new Vector2(0f, -6f);
                    Image peek = newest == null ? null : MosseloraArt.Ornament(basket.transform, "Newest thing, peeking out",
                        newest, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0f), new Vector2(34f, 34f), new Vector2(4f, 2f));
                    if (peek != null) peek.transform.SetSiblingIndex(glyph.GetSiblingIndex()); // behind the basket's rim
                }
                else if (glyphImage != null && newest != null) { glyphImage.sprite = newest; glyphImage.color = Color.white; }
                if (world.PlantingCount == 0 && gentleMotion) BloomPulse.Add(basket.gameObject, 0.8f, 0.08f);
            }
        }

        // Cuttings relatives sent (I215), waiting in the basket: each wears a small
        // ribbon. Pick one, tap the grass: it's planted and becomes yours.
        private int giftPick = -1;
        private void GiftTokens(Transform row)
        {
            int[] gifts = world.Gifts;
            for (int i = 0; i < gifts.Length; i++)
            {
                int index = i;
                GardenItem plant = Nursery.Item(gifts[i]);
                Button token = IconButton(row, "Gift " + i, "A gift: " + plant.Name, MosseloraArt.IconRooted, () => {
                        giftPick = giftPick == index ? -1 : index; basketKind = -1; heldPlanting = -1; RefreshMaps();
                    }, 74, index == giftPick ? MosseloraTheme.ForestAction : new Color(0.98f, 0.9f, 0.86f, 1f),
                    false, null, false, 0, 0, true);
                Transform glyph = token.transform.Find("Icon");
                Image glyphImage = glyph != null ? glyph.GetComponent<Image>() : null;
                Sprite art = MosseloraArt.Load(plant.Art);
                if (glyphImage != null && art != null)
                {
                    glyphImage.sprite = art;
                    glyphImage.color = Color.white;
                    var size = glyph.GetComponent<LayoutElement>();
                    size.minWidth = size.preferredWidth = 50f;
                    size.minHeight = size.preferredHeight = 50f;
                }
                Image ribbon = MosseloraArt.Ornament(token.transform, "Ribbon", MosseloraArt.Rounded(8),
                    new Vector2(1f, 1f), new Vector2(0.5f, 0.5f), new Vector2(18f, 18f), new Vector2(-8f, -8f));
                if (ribbon != null) { ribbon.preserveAspect = false; ribbon.color = new Color(0.85f, 0.32f, 0.35f, 1f); }
                if (index == giftPick && gentleMotion) BloomPulse.Add(token.gameObject, 0.9f, 0.06f);
            }
        }

        // The nursery's nine plants in the tray: row by row, each with its price. A
        // closed row is faded with a small lock; a plant you can't afford yet shows
        // its price dimmed. Pick one, then tap the grass to buy and plant it.
        private void NurseryTokens(Transform row)
        {
            // One token per slot, showing what it has in stock now (I240: it restocks as you buy).
            for (int slot = 0; slot < Nursery.Slots; slot++)
            {
                int kind = world.NurseryStock(slot);
                int pick = kind;
                GardenItem plant = Nursery.Item(kind);
                int price = Nursery.Price(kind);
                bool open = world.RowOpen(Nursery.Row(kind));
                bool affordable = open && world.Seeds >= price;
                Button token = IconButton(row, "Nursery " + kind, plant.Name + ", " + price + " seeds. " + PriceReason(Nursery.Row(kind)),
                    MosseloraArt.IconRooted, () => {
                        if (!affordable) return; // gently refused: the faded look already says why
                        nurseryKind = nurseryKind == pick ? -1 : pick;
                        RefreshMaps();
                    }, 74, kind == nurseryKind ? MosseloraTheme.ForestAction : MosseloraTheme.QuietAction,
                    false, null, false, 0, 0, true);
                Transform glyph = token.transform.Find("Icon");
                Image glyphImage = glyph != null ? glyph.GetComponent<Image>() : null;
                Sprite art = MosseloraArt.Load(plant.Art);
                if (glyphImage != null && art != null)
                {
                    glyphImage.sprite = art;
                    glyphImage.color = open ? Color.white : new Color(1f, 1f, 1f, 0.35f);
                    var size = glyph.GetComponent<LayoutElement>();
                    size.minWidth = size.preferredWidth = 50f;
                    size.minHeight = size.preferredHeight = 50f;
                }
                // A closed row still shows its price, greyed: you can see what you save for (I213).
                PriceChip(token.transform, price.ToString(), affordable, open);
                // Not yet in your almanac (I239): a small book on the token's shoulder.
                if (!almanac.Has(plant)) NewToAlmanac(token.transform);
                if (kind == nurseryKind && gentleMotion) BloomPulse.Add(token.gameObject, 0.9f, 0.06f);
            }
        }

        private void NewToAlmanac(Transform token)
        {
            Image disc = MosseloraArt.Ornament(token, "New to your almanac", MosseloraArt.Rounded(13), new Vector2(1f, 1f),
                new Vector2(0.5f, 0.5f), new Vector2(26f, 26f), new Vector2(-8f, -8f));
            if (disc == null) return;
            disc.preserveAspect = false;
            disc.color = new Color(0.98f, 0.95f, 0.86f, 1f);
            Image book = MosseloraArt.Ornament(disc.transform, "Almanac mark", MosseloraArt.Load(MosseloraArt.IconJournal),
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(16f, 16f), Vector2.zero);
            if (book != null) book.color = Leaf;
        }

        // A small chip on a nursery token's lower edge: the price, or a lock when the row is closed.
        private void PriceChip(Transform token, string price, bool affordable, bool open)
        {
            Image chip = MosseloraArt.Ornament(token, "Price", MosseloraArt.Rounded(10), new Vector2(0.5f, 0f),
                new Vector2(0.5f, 0.5f), new Vector2(46f, 22f), new Vector2(0f, 2f));
            if (chip == null) return;
            chip.preserveAspect = false;
            chip.color = affordable ? new Color(0.98f, 0.93f, 0.74f, 1f)
                : open ? new Color(0.9f, 0.88f, 0.82f, 0.95f) : new Color(0.82f, 0.82f, 0.8f, 0.9f);
            Text value = Label(chip.transform, "Price value", price, 14, 0,
                affordable ? Ink : new Color(Ink.r, Ink.g, Ink.b, open ? 0.45f : 0.35f), TextAnchor.MiddleCenter);
            value.horizontalOverflow = HorizontalWrapMode.Overflow;
            value.GetComponent<LayoutElement>().ignoreLayout = true;
            Fill(value.rectTransform);
            Emphasize(value);
        }

        // Redraw home framed on the newest/just-solved thing, not where the view was.
        private void RefreshMapsFresh()
        {
            keepHubAt = new Vector2(-1f, -1f);
            if (titleScreen != null && canvasRoot != null)
            {
                Transform host = titleScreen.transform.parent;
                CloseTitle();
                BuildTitleScreen(host);
            }
            else Rebuild();
        }

        // Redraw whichever map is showing, keeping the page where it is.
        private void RefreshMaps()
        {
            if (titleScreen != null && canvasRoot != null)
            {
                if (hubMapView != null) keepHubAt = hubMapView.Position;
                Transform host = titleScreen.transform.parent;
                CloseTitle();
                BuildTitleScreen(host);
            }
            else Rebuild();
        }

        // The page's garden window (memory card, boards): the same map as the hub.
        private bool heroGarden;

        private bool PageGardenMap(GameObject window, bool memoryScreen)
        {
            ApplyDayLight(world);
            Sprite plot = MapPainting();
            GardenMap map = LoadGardenMap();
            if (plot == null || map == null) return false;
            var size = window.GetComponent<LayoutElement>();
            if (size == null) size = window.AddComponent<LayoutElement>();
            bool hero = heroGarden && memoryScreen;
            size.minHeight = size.preferredHeight = hero ? (TextScale.Percent > 100 ? 360f : 420f)
                : TextScale.Percent > 100 ? 230f : 260f;
            // Every page's garden runs edge to edge (I186); the memory card's also
            // reaches up under the status bar (I185).
            {
                if (hero) window.transform.SetAsFirstSibling();
                var bleed = Node(hero ? "Garden hero" : "Garden band", window.transform);
                var bleedRect = bleed.GetComponent<RectTransform>();
                Fill(bleedRect);
                bleedRect.offsetMin = new Vector2(-17f - columnLeft, 0f);
                bleedRect.offsetMax = new Vector2(17f + columnRight, hero ? 15f + topInset : 0f);
                window = bleed;
                if (!hero)
                {
                    // Below a board the garden melts into the paper at its top too.
                    var top = Node("Garden melt top", window.transform);
                    var topRect = top.GetComponent<RectTransform>();
                    topRect.anchorMin = new Vector2(0f, 1f);
                    topRect.anchorMax = new Vector2(1f, 1f);
                    topRect.pivot = new Vector2(0.5f, 1f);
                    topRect.sizeDelta = new Vector2(0f, 40f);
                    var topImage = top.AddComponent<Image>();
                    topImage.sprite = MosseloraArt.TopFade();
                    topImage.color = Paper;
                    topImage.raycastTarget = false;
                }
                // Melt into the paper at the bottom, as the home screen does.
                var melt = Node("Hero melt", window.transform);
                var meltRect = melt.GetComponent<RectTransform>();
                meltRect.anchorMin = Vector2.zero;
                meltRect.anchorMax = new Vector2(1f, 0f);
                meltRect.pivot = new Vector2(0.5f, 0f);
                meltRect.sizeDelta = new Vector2(0f, 48f);
                meltRect.localScale = new Vector3(1f, -1f, 1f);
                meltRect.anchoredPosition = new Vector2(0f, 48f);
                var meltImage = melt.AddComponent<Image>();
                meltImage.sprite = MosseloraArt.TopFade();
                meltImage.color = Paper;
                meltImage.raycastTarget = false;
            }
            window.AddComponent<RectMask2D>();
            bool grow = celebrateGrowth && memoryScreen;
            celebrateGrowth = false;
            bool animate = grow && gentleMotion;
            GardenMapView view = GardenMapView.Build(window.GetComponent<RectTransform>(), plot, map, MapItems(map), 1.25f,
                OnMapLand, OnMapItem, ItemInHand(), animate, GreenedShare(0),
                animate ? GreenedShare(1) : -1f,
                memoryScreen ? null : (land, point) => NeighborsOnMap(land, point, map, false), 0f, RiverGoesOn(map));
            // This stream opened a new reach of river: watch its mist lift (I195).
            if (animate && !world.Completed && map.Reaches > 1 &&
                world.Choices.Length % GardenMap.StreamsPerReach == 0)
            {
                view.LiftMist(map.Reaches - 1);
                PlayChime();
            }
            if (!memoryScreen) RiverDrops(view, false);
            // Under a floating title the new thing sits lower in view (I185).
            if (hero) view.FocusFromTop = 0.55f;
            int held = ItemInHand();
            if (held >= 0 && PlayerPrefs.GetInt(PlacedOnceKey, 0) == 0) PlaceTip(window.transform, held);
            // No wordmark or tagline over your garden (I194): you know where you are.
            return true;
        }

        // The wordmark and tagline float on the garden, under the menu and book.
        private void HeroTitle(Transform hero)
        {
            Sprite mark = MosseloraArt.Load(MosseloraArt.Wordmark);
            var title = Node("Floating title", hero);
            title.AddComponent<LayoutElement>().ignoreLayout = true;
            var rect = title.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(1f, 1f);
            rect.pivot = new Vector2(0.5f, 1f);
            rect.sizeDelta = new Vector2(0f, 118f);
            rect.anchoredPosition = new Vector2(0f, -topInset - 6f);
            if (mark != null)
            {
                var markNode = Node("Mosselora wordmark", title.transform);
                var markRect = markNode.GetComponent<RectTransform>();
                markRect.anchorMin = new Vector2(0f, 1f);
                markRect.anchorMax = new Vector2(1f, 1f);
                markRect.pivot = new Vector2(0.5f, 1f);
                markRect.sizeDelta = new Vector2(-2f * ChromeInset, 72f);
                markRect.anchoredPosition = Vector2.zero;
                var markImage = markNode.AddComponent<Image>();
                markImage.sprite = mark;
                markImage.preserveAspect = true;
                markImage.raycastTarget = false;
            }
            // The tagline on a small cream plate, readable over any grass.
            var plate = Node("Tagline plate", title.transform);
            var plateRect = plate.GetComponent<RectTransform>();
            plateRect.anchorMin = plateRect.anchorMax = new Vector2(0.5f, 1f);
            plateRect.pivot = new Vector2(0.5f, 1f);
            plateRect.anchoredPosition = new Vector2(0f, -76f);
            var plateImage = plate.AddComponent<Image>();
            MosseloraArt.Round(plateImage, new Color(1f, 0.973f, 0.918f, 0.92f), 14);
            plateImage.raycastTarget = false;
            var plateRow = plate.AddComponent<HorizontalLayoutGroup>();
            plateRow.padding = new RectOffset(14, 14, 4, 5);
            plateRow.childControlWidth = true;
            plateRow.childControlHeight = true;
            var fit = plate.AddComponent<ContentSizeFitter>();
            fit.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
            fit.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            Text line = Label(plate.transform, "Title", GameLocalization.Text("subtitle"), 17, 0, Ink,
                TextAnchor.MiddleCenter);
            Emphasize(line);
            line.horizontalOverflow = HorizontalWrapMode.Overflow;
        }

        // Until a player has put something down once, the map says how (I182): a
        // cold read pressed the arrow and never found that the birch could move.
        private const string PlacedOnceKey = "evergrove:placed-once";

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

        // Floating game chrome: a leaf menu button (left) and the memory book
        // with its count (right). Rebuilt with the page so state stays honest.
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
            if (living && world.Choices.Length > 0 && !showJournal) MarkUnreadMemory(menu);
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

        private void ToggleFan(bool fromHub)
        {
            if (fan != null) { CloseFan(); return; }
            if (canvasRoot == null) return;
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
            if (living)
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

        private sealed class CueFade : MonoBehaviour
        {
            public float After = 2.5f;
            private float start;
            private Image image;
            private void Start() { start = Time.unscaledTime; image = GetComponent<Image>(); }
            private void Update()
            {
                if (image == null) return;
                float t = Time.unscaledTime - start - After;
                if (t <= 0f) return;
                Color c = image.color;
                c.a = Mathf.Clamp01(1f - t / 1.2f) * 0.9f;
                image.color = c;
                if (c.a <= 0f) enabled = false;
            }
        }

        private GameObject FanToken(Transform parent, string name, string caption, string icon, Action action)
        {
            Button token = IconButton(parent, name, caption, icon, action, 64, MosseloraTheme.QuietAction,
                false, null, false, 0, 0, true);
            var size = token.GetComponent<LayoutElement>();
            if (size == null) size = token.gameObject.AddComponent<LayoutElement>();
            size.minHeight = size.preferredHeight = 64f;
            size.flexibleHeight = 0f;
            return token.gameObject;
        }

        private void OpenBook(bool almanac)
        {
            CloseFan();
            CloseTitle();
            menuOpen = false;
            showJournal = true;
            almanacTab = almanac;
            familyTab = false;
            journalOpenPage = -1;
            Rebuild();
            if (journalScroll != null) journalScroll.verticalNormalizedPosition = 1f;
        }

        private static IEnumerator FanIn(GameObject token, float delay)
        {
            if (token == null) yield break;
            var group = token.AddComponent<CanvasGroup>();
            group.alpha = 0f;
            token.transform.localScale = new Vector3(0.7f, 0.7f, 1f);
            float clock = -delay;
            while (clock < 0.22f && token != null)
            {
                clock += Time.unscaledDeltaTime;
                float t = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(clock / 0.22f));
                group.alpha = t;
                token.transform.localScale = Vector3.one * Mathf.Lerp(0.7f, 1f, t);
                yield return null;
            }
        }

        // Sideways there is no top safe inset, so the chrome sits a little lower
        // and the book's new-memory mark is not clipped by the screen edge (I116).
        private float ChromeTop { get { return landscape ? -12f : -6f; } }

        // Sideways, Undo and Hint stand in a rail right of the board, so the
        // board gets the height a tool row would take below it (I116).
        private const float ToolRailWidth = BoardFit.RailWidth;
        private const float ToolRailGap = BoardFit.RailGap;

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

        private IEnumerator FadeAway(GameObject node)
        {
            var fade = node.GetComponent<CanvasGroup>();
            if (fade == null) fade = node.AddComponent<CanvasGroup>();
            fade.blocksRaycasts = false;
            fade.interactable = false;
            float from = fade.alpha;
            bool animate = gentleMotion;
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            if (qaInMemoryOnly) animate = false; // tour screenshots capture settled screens
#endif
            for (float t = 0f; animate && t < 1f; t += Time.unscaledDeltaTime / 0.35f)
            {
                if (fade == null) yield break;
                fade.alpha = Mathf.Lerp(from, 0f, Mathf.SmoothStep(0f, 1f, t));
                yield return null;
            }
            if (fade != null) fade.alpha = 0f;
        }

        // Undo appears only once there is something to undo: before the first
        // move its place stays empty (no layout shift), then it fades in (I155).
        private void ShowUndo(bool available, bool animate)
        {
            if (undoButton == null) return;
            var group = undoButton.GetComponent<CanvasGroup>();
            if (group == null) group = undoButton.gameObject.AddComponent<CanvasGroup>();
            group.blocksRaycasts = available;
            float target = available ? 1f : 0f;
            if (Mathf.Approximately(group.alpha, target)) return;
            bool motion = animate && gentleMotion;
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            if (qaInMemoryOnly) motion = false;
#endif
            if (!motion) { group.alpha = target; return; }
            StartCoroutine(FadeGroup(group, target, 0.25f));
        }

        private static IEnumerator FadeGroup(CanvasGroup group, float target, float seconds)
        {
            float from = group.alpha;
            for (float t = 0f; t < 1f; t += Time.unscaledDeltaTime / seconds)
            {
                if (group == null) yield break;
                group.alpha = Mathf.Lerp(from, target, Mathf.SmoothStep(0f, 1f, t));
                yield return null;
            }
            if (group != null) group.alpha = target;
        }

        // A cream disc with the turning arrow at a tile's top-right corner. It
        // turns a quarter at a time, as the tile will, and leaves with the goal
        // glows on the first move.
        private void TurnMark(Transform tile, float tileSize)
        {
            float size = Mathf.Round(Mathf.Clamp(tileSize * 0.42f, 30f, 44f));
            var mark = Node("Tap turns this tile", tile);
            goalGlows.Add(mark);
            mark.AddComponent<LayoutElement>().ignoreLayout = true;
            var rect = mark.GetComponent<RectTransform>();
            rect.anchorMin = rect.anchorMax = new Vector2(1f, 1f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = new Vector2(size, size);
            rect.anchoredPosition = new Vector2(-size * 0.3f, -size * 0.3f);
            var disc = mark.AddComponent<Image>();
            MosseloraArt.Round(disc, MosseloraTheme.CardFill, Mathf.RoundToInt(size / 2f));
            disc.raycastTarget = false;
            MosseloraArt.Lift(disc, 1.5f, 0.25f);
            var glyph = MosseloraArt.Ornament(mark.transform, "Turning arrow", MosseloraArt.Load(MosseloraArt.IconTurn),
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(size * 0.66f, size * 0.66f), Vector2.zero);
            if (glyph == null) return;
            glyph.color = MosseloraTheme.WaterChannel;
            if (gentleMotion) glyph.gameObject.AddComponent<QuarterTurns>();
            // A painted finger rests on the tile and presses it in time with the
            // arrow's turn: the board's own "how to play", without words (I147).
            Sprite tap = MosseloraArt.Load(MosseloraArt.IconTap);
            if (tap == null) return;
            var hand = Node("Shows a tap here", tile);
            goalGlows.Add(hand);
            hand.AddComponent<LayoutElement>().ignoreLayout = true;
            var handRect = hand.GetComponent<RectTransform>();
            // The whole hand sits in the tile's lower-right quarter, which a channel
            // never crosses (arms run along the centre lines): it presses the tile
            // without hiding the pipe (I159), and stays inside the tile, under no
            // neighbour.
            handRect.anchorMin = handRect.anchorMax = new Vector2(0.74f, 0.36f);
            handRect.pivot = new Vector2(0.35f, 0.9f); // the fingertip
            float handSize = Mathf.Round(tileSize * 0.36f);
            handRect.sizeDelta = new Vector2(handSize, handSize);
            handRect.anchoredPosition = Vector2.zero;
            var handImage = hand.AddComponent<Image>();
            handImage.sprite = tap;
            handImage.preserveAspect = true;
            handImage.raycastTarget = false;
            handImage.color = new Color(Ink.r, Ink.g, Ink.b, 0.82f);
            if (gentleMotion) hand.AddComponent<GhostPress>();
        }

        // Holds a row of primary actions at the foot of the screen over a soft paper
        // fade, so "continue" is always in reach at any text size; the page keeps
        // an empty space of the same height so its end can scroll clear (I164).
        private void PinToFoot(Transform row)
        {
            if (row == null || scrollArea == null) return;
            LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)row);
            float rowHeight = Mathf.Max(64f, LayoutUtility.GetPreferredHeight((RectTransform)row));
            float barHeight = rowHeight + 30f;
            pinnedActions = Node("Pinned actions", scrollArea);
            pinnedActions.transform.SetSiblingIndex(1); // above the page, under menus and chrome
            var bar = pinnedActions.GetComponent<RectTransform>();
            bar.anchorMin = new Vector2(0f, 0f);
            bar.anchorMax = new Vector2(1f, 0f);
            bar.pivot = new Vector2(0.5f, 0f);
            bar.sizeDelta = new Vector2(0f, barHeight + 26f);
            bar.anchoredPosition = Vector2.zero;
            // Only over the page's column: sideways, the garden painting on the left
            // stays uncovered (I170: the fade had blanked its lower half).
            bar.offsetMin = new Vector2(columnLeft, bar.offsetMin.y);
            bar.offsetMax = new Vector2(-columnRight, bar.offsetMax.y);
            var fade = Node("Paper fade", pinnedActions.transform);
            var fadeRect = fade.GetComponent<RectTransform>();
            Fill(fadeRect);
            fadeRect.localScale = new Vector3(1f, -1f, 1f); // TopFade, flipped: solid at the foot
            var fadeImage = fade.AddComponent<Image>();
            fadeImage.sprite = MosseloraArt.TopFade();
            fadeImage.color = Paper;
            fadeImage.raycastTarget = false;
            var solid = Node("Paper", pinnedActions.transform);
            var solidRect = solid.GetComponent<RectTransform>();
            solidRect.anchorMin = Vector2.zero;
            solidRect.anchorMax = new Vector2(1f, 0f);
            solidRect.pivot = new Vector2(0.5f, 0f);
            solidRect.sizeDelta = new Vector2(0f, barHeight * 0.62f);
            var solidImage = solid.AddComponent<Image>();
            solidImage.color = Paper;
            solidImage.raycastTarget = false;
            var keep = Node("Space for the pinned actions", content);
            MinHeight(keep, barHeight);
            int index = row.GetSiblingIndex();
            row.SetParent(pinnedActions.transform, false);
            var layout = row.GetComponent<LayoutElement>();
            if (layout == null) layout = row.gameObject.AddComponent<LayoutElement>();
            layout.ignoreLayout = true;
            var rect = (RectTransform)row;
            rect.anchorMin = new Vector2(0f, 0f);
            rect.anchorMax = new Vector2(1f, 0f);
            rect.pivot = new Vector2(0.5f, 0f);
            rect.sizeDelta = new Vector2(-34f, rowHeight);
            rect.anchoredPosition = new Vector2(0f, 14f);
            keep.transform.SetSiblingIndex(Mathf.Min(index, content.childCount - 1));
        }

        // ---- I172: the garden catalogue (Core/GardenCatalog) at every choice.
        private GardenItem ItemAt(int chapter, int family)
        {
            int safe = Mathf.Clamp(chapter, 0, GardenCatalog.ChaptersPerGarden - 1);
            // The save knows its garden (I177/I178); a restored file keeps its items.
            int garden = world != null ? world.Garden : gardenNumber;
            return GardenCatalog.For(garden, safe, (VillageChoice)Mathf.Clamp(family, 0, 2));
        }

        // The painted item, or the family's own art while an item's art is missing.
        private Sprite ItemArt(int chapter, int family, bool fallback = true)
        {
            Sprite art = MosseloraArt.Load(ItemAt(chapter, family).Art);
            return art != null || !fallback ? art : MosseloraArt.ChoiceIcon((VillageChoice)Mathf.Clamp(family, 0, 2));
        }

        // "Mira remembers the quince tree you planted." English names the item; other
        // languages keep their reviewed family lines until the catalogue is translated.
        private string Remembered(int[] choices, int index)
        {
            if (GameLocalization.Current != "en" || choices == null || index < 0 || index >= choices.Length)
                return LocalizedVillage.MemoryAt(choices, index);
            return LocalizedVillage.Name(choices[index]) + " remembers " + ItemAt(index, choices[index]).Memory + ".";
        }

        // The chosen item's painted icon on a cream disc, at a painting's lower-left.
        private void ItemBadge(Transform painting, int chapter, int family)
        {
            Sprite art = ItemArt(chapter, family, false);
            if (art == null) return;
            var disc = MosseloraArt.Ornament(painting, "What was made", MosseloraArt.Rounded(30),
                new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(70f, 70f), new Vector2(8f, 6f));
            if (disc == null) return;
            disc.preserveAspect = false;
            disc.color = MosseloraTheme.CardFill;
            MosseloraArt.Lift(disc, 2f, 0.25f);
            MosseloraArt.Ornament(disc.transform, "Item", art, new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0.5f), new Vector2(58f, 58f), Vector2.zero);
        }

        // I393: text a screen reader still speaks but the screen no longer shows (the picture
        // or an earlier lesson already says it).
        private static void SpokenOnly(Text text)
        {
            if (text == null) return;
            text.color = new Color(0f, 0f, 0f, 0f);
            LayoutElement layout = text.GetComponent<LayoutElement>() ?? text.gameObject.AddComponent<LayoutElement>();
            layout.ignoreLayout = true;
        }

        // The card's visible label (I196): the thing itself, "Silver birch", not the
        // sentence "Plant a silver birch". The full sentence is still spoken.
        private string ChoiceName(VillageChoice choice)
        {
            if (GameLocalization.Current != "en") return LocalizedVillage.Choice(choice);
            string name = ItemAt(world.Round, (int)choice).Name;
            return string.IsNullOrEmpty(name) ? ChoiceTitle(choice)
                : char.ToUpperInvariant(name[0]) + name.Substring(1);
        }

        private string ChoiceTitle(VillageChoice choice)
        {
            return GameLocalization.Current == "en"
                ? ItemAt(world.Round, (int)choice).Card : LocalizedVillage.Choice(choice);
        }

        // A slim painted wooden plank (I162) holding a row of icon + name, or null
        // when the art is missing. Its height is set, not taken from the sprite.
        private Transform PaintedSign(Transform parent)
        {
            Sprite plank = MosseloraArt.Load(MosseloraArt.StreamSign);
            if (plank == null) return null;
            var sign = Node("Stream sign", parent);
            var signImage = sign.AddComponent<Image>();
            signImage.sprite = plank;
            signImage.type = Image.Type.Sliced;
            signImage.pixelsPerUnitMultiplier = 2.6f;
            signImage.raycastTarget = false;
            var signRow = sign.AddComponent<HorizontalLayoutGroup>();
            signRow.padding = new RectOffset(24, 24, 7, 8);
            signRow.spacing = 6;
            signRow.childAlignment = TextAnchor.MiddleCenter;
            signRow.childControlWidth = true;
            signRow.childControlHeight = true;
            signRow.childForceExpandWidth = false;
            signRow.childForceExpandHeight = false;
            var signSize = sign.AddComponent<LayoutElement>();
            signSize.layoutPriority = 2;
            signSize.minHeight = signSize.preferredHeight = TextScale.Scale(36);
            signSize.flexibleHeight = 0f;
            return sign.transform;
        }

        // One line on a sign; the sign grows to fit the name.
        private static void OneLine(Text text)
        {
            text.horizontalOverflow = HorizontalWrapMode.Overflow;
            var size = text.GetComponent<LayoutElement>();
            size.minWidth = size.preferredWidth = Mathf.Ceil(text.preferredWidth) + 2f;
        }

        // The neighbor's card rises a few points and fades in, like a word said.
        private IEnumerator RiseIn(RectTransform card)
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            if (qaInMemoryOnly) yield break;
#endif
            var fade = card.gameObject.AddComponent<CanvasGroup>();
            Vector2 rest = card.anchoredPosition;
            for (float t = 0f; t < 1f; t += Time.unscaledDeltaTime / 0.22f)
            {
                if (card == null) yield break;
                float k = Mathf.SmoothStep(0f, 1f, t);
                fade.alpha = k;
                card.anchoredPosition = rest + new Vector2(0f, -12f * (1f - k));
                yield return null;
            }
            if (card == null) yield break;
            fade.alpha = 1f;
            card.anchoredPosition = rest;
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
                            // The phone's own share sheet first (I149); the clipboard if none.
                            string text = GardenPostcard.Build(world, "");
                            if (NativeShare.Text(text, GameLocalization.Text("postcard"))) return;
                            GUIUtility.systemCopyBuffer = text;
                            RuleToast.Show(postcard.GetComponent<RectTransform>(),
                                GameLocalization.Text("postcard_copied"), font, Ink, MosseloraTheme.CardFill);
                        }, 96, MosseloraTheme.WaterAction, false, null, true);
                    Width(postcard, 150);
                    ShowPostcard(postcard);
                    EndingCaption(postcard);
                }
                // The ending's two offers are rare and lasting (share the season,
                // or start another garden), so here they say what they do under
                // the picture; a cold read could not tell them apart (I136).
                Button explore = IconButton(row, resetName, GameLocalization.Text("explore"),
                    MosseloraArt.IconNewGarden, () => { confirmReset = true; Rebuild(); },
                    inMenu ? 78 : sharePostcard ? 96 : 58, sharePostcard ? MosseloraTheme.QuietAction : tone,
                    false, null, inMenu || sharePostcard);
                Width(explore, inMenu ? 200 : sharePostcard ? 150 : 96);
                if (sharePostcard) EndingCaption(explore);
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
            // A layer over the page, under the menu and the chrome buttons.
            neighborLayer = Node("Neighbor speaking", scrollArea);
            neighborLayer.transform.SetSiblingIndex(1);
            Fill(neighborLayer.GetComponent<RectTransform>());
            var veil = neighborLayer.AddComponent<Image>();
            // I347 (run 3, P6): the neighbour comes forward; the page behind steps back.
            veil.color = new Color(Ink.r, Ink.g, Ink.b, 0.36f);
            // A tap anywhere else ends the conversation, as the check does.
            neighborLayer.AddComponent<TapCatcher>().Tapped = () => {
                // I359 "said once" is now the companion memory's 3-day rule (SC02).
                speakingWith = -1;
                Rebuild();
            };
            var row = Node("A neighbor's memory", neighborLayer.transform);
            var rowRect = row.GetComponent<RectTransform>();
            rowRect.anchorMin = new Vector2(0f, 0f);
            rowRect.anchorMax = new Vector2(1f, 0f);
            rowRect.pivot = new Vector2(0.5f, 0f);
            rowRect.sizeDelta = new Vector2(-34f - columnLeft - columnRight, 0f);
            rowRect.anchoredPosition = new Vector2((columnLeft - columnRight) / 2f, 14f);
            row.AddComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            if (gentleMotion && neighborShown != speakingWith) StartCoroutine(RiseIn(rowRect));
            neighborShown = speakingWith;
            // One paper card holds the portrait and the words, so nothing on the
            // page shows through beside the face (I127).
            var rowImage = row.AddComponent<Image>();
            MosseloraArt.Round(rowImage, MosseloraTheme.CardFill, 22);
            MosseloraArt.Edge(row.transform, MosseloraTheme.CardEdge, 22);
            MosseloraArt.Lift(rowImage, 4f, 0.22f);
            var rowGroup = row.AddComponent<HorizontalLayoutGroup>();
            rowGroup.padding = new RectOffset(12, 6, 12, 6);
            rowGroup.reverseArrangement = GameLocalization.IsRtl(GameLocalization.Current);
            rowGroup.spacing = 6;
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
                portraitLayout.minWidth = portraitLayout.preferredWidth = 92; // I347: a face you can see
                portraitLayout.minHeight = portraitLayout.preferredHeight = 92;
                var portraitImage = portrait.AddComponent<Image>();
                portraitImage.sprite = face;
                portraitImage.preserveAspect = true;
                portraitImage.raycastTarget = false;
                MosseloraArt.Lift(portraitImage, 2f, 0.2f);
            }
            var bubble = Card(row.transform, "Speech bubble", new Color(0f, 0f, 0f, 0f));
            bubble.AddComponent<LayoutElement>().flexibleWidth = 1f;
            Text speaker = Label(bubble.transform, "Neighbor name",
                LocalizedVillage.Name(speakingWith), 17, 0, Leaf, TextAnchor.MiddleLeft);
            Emphasize(speaker);
            Narrate(Label(bubble.transform, "What they say",
                NeighborSays(speakingWith), 20, 0, Ink, TextAnchor.MiddleLeft));
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
            // Bring them some fruit from your trees (I231): one pear, a kind word back.
            if (world.Mode == PlayMode.Living && world.Fruit > 0)
            {
                int villager = speakingWith;
                var give = IconButton(bubble.transform, "Give fruit", "Give " + LocalizedVillage.Name(villager) + " some fruit",
                    "food-basket", () => GiveFruit(villager), 44, MosseloraTheme.QuietAction);
                give.gameObject.GetComponent<LayoutElement>().ignoreLayout = true;
                var giveRect = give.GetComponent<RectTransform>();
                giveRect.anchorMin = giveRect.anchorMax = new Vector2(1f, 0f);
                giveRect.pivot = new Vector2(1f, 0f);
                giveRect.sizeDelta = new Vector2(64f, 48f);
                giveRect.anchoredPosition = new Vector2(-74f, 8f);
                Transform giveIcon = give.transform.Find("Icon");
                if (giveIcon != null)
                {
                    giveIcon.GetComponent<Image>().color = Color.white; // painted, not a glyph
                    // A painted basket needs room to read as fruit (I232): about twice a glyph.
                    var iconSize = giveIcon.GetComponent<LayoutElement>();
                    iconSize.minWidth = iconSize.preferredWidth = 38f;
                    iconSize.minHeight = iconSize.preferredHeight = 38f;
                }
            }
            // Room under the words for the tokens (I232): the basket stands a little taller.
            bool basket = world.Mode == PlayMode.Living && world.Fruit > 0;
            bubble.GetComponent<VerticalLayoutGroup>().padding = new RectOffset(6, 10, 2, basket ? 64 : 56);
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
                plateGroup.padding = new RectOffset(10, 10, 2, 3);
                plateGroup.childControlWidth = true;
                plateGroup.childControlHeight = true;
                plateGroup.childForceExpandWidth = false;
                // 16pt: a cold read found the names too small to read at arm's length (I137).
                caption = Label(plate.transform, "Caption text", label, 16, 0, Ink, TextAnchor.MiddleCenter);
            }
            else
                caption = Label(node.transform, "Caption", label, 15, 0, Ink, TextAnchor.MiddleCenter);
            Emphasize(caption);
            // The speech mark is a picture; screen readers hear it with the name (I81).
            if (news) SpeakInstead(node.transform, label + ". " + GameLocalization.Text("has_news"));
        }

        // The share button shows what it shares: a tiny postcard of the season's
        // festival painting, with the chat mark tucked in its corner (I97).
        // Readable captions under the ending's two pictures: 15pt, clear of the
        // postcard's corner badge (I136).
        private void EndingCaption(Button button)
        {
            var group = button.GetComponent<VerticalLayoutGroup>();
            if (group != null) { group.spacing = 9; group.padding = new RectOffset(8, 8, 9, 9); }
            Transform caption = button.transform.Find("Caption");
            Text text = caption != null ? caption.GetComponent<Text>() : null;
            if (text != null) StyleText(text, 15);
            MinHeight(button.gameObject, TextScale.Scale(104));
        }

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
        private static void ShowChoiceToChange(Button button, Sprite painted)
        {
            Transform iconNode = button.transform.Find("Icon");
            Image icon = iconNode != null ? iconNode.GetComponent<Image>() : null;
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
            // Painted stones sit on one waterline, like a path across the creek (I120).
            bool paintedPath = MosseloraArt.Load(MosseloraArt.StoneHere) != null;
            stoneRow.childAlignment = paintedPath ? TextAnchor.LowerCenter : TextAnchor.MiddleCenter;
            stoneRow.spacing = paintedPath ? 4 : 7;
            stoneRow.childControlWidth = true;
            stoneRow.childControlHeight = true;
            stoneRow.childForceExpandWidth = false;
            stoneRow.childForceExpandHeight = false;
            // The path starts at the spring (I123): water already runs from it to your
            // stone, so even on the first stream "the water reaches here, not beyond"
            // shows before any stone has been walked.
            Sprite spring = paintedPath ? MosseloraArt.Load(MosseloraArt.SpringWaves) : null;
            if (spring != null)
            {
                var source = Node("Spring", stones.transform);
                var sourceLayout = source.AddComponent<LayoutElement>();
                sourceLayout.minWidth = sourceLayout.preferredWidth = 20f;
                sourceLayout.minHeight = sourceLayout.preferredHeight = 20f;
                PathWater(source.transform, stoneRow, true, false);
                var sourceArt = Node("Spring art", source.transform);
                Fill(sourceArt.GetComponent<RectTransform>());
                var sourceImage = sourceArt.AddComponent<Image>();
                sourceImage.sprite = spring;
                sourceImage.preserveAspect = true;
                sourceImage.raycastTarget = false;
            }
            for (int i = 0; i < PuzzleCatalog.Count; i++)
            {
                bool here = i == world.Round;
                bool passed = i < world.Round;
                var stone = Node(passed ? "Stone passed" : here ? "Stone here" : "Stone ahead", stones.transform);
                var layout = stone.AddComponent<LayoutElement>();
                // The stone just passed lights up once on arrival at the next stream.
                if (named && gentleMotion && settledRound != world.Round && i == world.Round - 1)
                    BloomPulse.Add(stone, 0.25f, 0.45f);
                // Painted river stones across the creek (Codex, I120): the ones ahead wear
                // their season, the one you stand on grows a sprout, and the water you
                // have already brought runs under every walked stone up to yours.
                Sprite painted = PathStone(i, here, passed);
                if (painted != null)
                {
                    float width = here ? 34f : 22f;
                    layout.minWidth = layout.preferredWidth = width;
                    layout.minHeight = layout.preferredHeight = Mathf.Round(width / MosseloraArt.Aspect(painted));
                    // A cold-read could not tell walked stones from ahead by the stones
                    // alone; a thin stream under every walked stone, reaching half way
                    // under yours, says "the water got this far" in the game's language.
                    if (passed || (here && (i > 0 || spring != null)))
                        PathWater(stone.transform, stoneRow, !here, here);
                    if (here)
                    {
                        // The old leaf-green ring, now around your stone: one high-contrast
                        // mark the eye finds at once, which the sprout alone did not match.
                        var ring = Node("You are here", stone.transform);
                        var ringRect = ring.GetComponent<RectTransform>();
                        ringRect.anchorMin = ringRect.anchorMax = new Vector2(0.5f, 0.5f);
                        ringRect.sizeDelta = new Vector2(width + 10f, layout.preferredHeight + 10f);
                        ringRect.anchoredPosition = Vector2.zero;
                        var ringImage = ring.AddComponent<Image>();
                        ringImage.sprite = MosseloraArt.MarkerRing();
                        ringImage.type = Image.Type.Sliced;
                        ringImage.pixelsPerUnitMultiplier = MosseloraArt.MarkerRingScale;
                        ringImage.color = Leaf;
                        ringImage.raycastTarget = false;
                    }
                    var art = Node("Stone art", stone.transform);
                    Fill(art.GetComponent<RectTransform>());
                    var artImage = art.AddComponent<Image>();
                    artImage.sprite = painted;
                    artImage.preserveAspect = true;
                    artImage.raycastTarget = false;
                    // Pale frost, moss and the sunlit "here" stone are deepened a little so
                    // every stone keeps at least 3:1 against the paper, as the old dots did
                    // (WCAG 1.4.11, I84): summer 3.9, autumn 4.1, winter 3.4, here 3.2.
                    artImage.color = passed ? Color.white : new Color(0.84f, 0.84f, 0.84f, 1f);
                    continue;
                }
                var image = stone.AddComponent<Image>();
                image.raycastTarget = false;
                float size = here ? 22f : 14f; // was 18 / 11: legible at arm's length (I84)
                layout.minWidth = layout.preferredWidth = size;
                layout.minHeight = layout.preferredHeight = size;
                image.sprite = here ? MosseloraArt.BoldRing() : MosseloraArt.Rounded(4);
                image.type = Image.Type.Simple;
                image.color = here ? Leaf : passed ? MosseloraTheme.WaterChannel : SeasonStone(i);
            }
            if (!named) return;
            string name = GameLocalization.Current == "en" ? puzzle.Name : "";
            // "Rainwater · Wildflower sanctuary" reads as the stream's name beside
            // the painted landmark that roots it, as on the hub (I86); the rooted
            // tile's spoken rule still names the sanctuary.
            int split = name.IndexOf(" · ", StringComparison.Ordinal);
            int[] past = world.Choices;
            Sprite mark = split > 0 && past.Length > 0
                ? ItemArt(past.Length - 1, past[past.Length - 1]) : null;
            // The stream's name is painted on a small wooden sign by the path,
            // part of the scenery rather than a caption (I162).
            Transform nameRow = CenteredRowIn(row.transform, "Stream name row");
            nameRow.GetComponent<HorizontalLayoutGroup>().spacing = 6;
            Transform sign = PaintedSign(nameRow);
            bool plank = sign != null;
            if (plank) nameRow = sign;
            if (mark != null)
            {
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
                name.Length == 0 ? " " : mark != null ? name.Substring(0, split) : name, 16, 0,
                plank ? Ink : Moss, // was 14 (I137); ink on the painted wood (I162)
                TextAnchor.MiddleCenter);
            Emphasize(title);
            if (plank) OneLine(title);
            // Spoken position along the creek, not shown: the stones show it.
            Text spoken = Label(row.transform, "Stream position",
                GameLocalization.Text("stream", world.Round + 1, PuzzleCatalog.Count),
                12, 0, new Color(0f, 0f, 0f, 0f), TextAnchor.MiddleCenter);
            spoken.GetComponent<LayoutElement>().ignoreLayout = true;
        }

        // The thin stream under the path: full width under walked stones and the
        // spring, half way under your stone, overlapping neighbours so it has no seams.
        private void PathWater(Transform holder, HorizontalLayoutGroup row, bool flowsOn, bool endsHere)
        {
            var water = Node("Water so far", holder);
            water.transform.SetAsFirstSibling(); // under the stone
            var waterRect = water.GetComponent<RectTransform>();
            bool rtl = row.reverseArrangement;
            float reachStart = rtl && endsHere ? 0.5f : 0f;
            float reachEnd = !rtl && endsHere ? 0.5f : 1f;
            waterRect.anchorMin = new Vector2(reachStart, 0f);
            waterRect.anchorMax = new Vector2(reachEnd, 0f);
            waterRect.pivot = new Vector2(0.5f, 0f);
            float before = rtl && flowsOn ? row.spacing + 3f : 3f;
            float after = !rtl && flowsOn ? row.spacing + 3f : 3f;
            // 9 pt: a 6 pt stroke was "easy to miss at phone size" in a cold-read (I124).
            waterRect.offsetMin = new Vector2(-before, -4f);
            waterRect.offsetMax = new Vector2(after, 5f);
            var waterImage = water.AddComponent<Image>();
            waterImage.sprite = MosseloraArt.Rounded(3);
            // Opaque, so the overlaps between stones leave no darker seams.
            waterImage.color = Color.Lerp(Paper, MosseloraTheme.WaterChannel, 0.8f);
            waterImage.raycastTarget = false;
        }

        private static Sprite PathStone(int stream, bool here, bool passed)
        {
            if (here) return MosseloraArt.Load(MosseloraArt.StoneHere);
            if (passed) return MosseloraArt.Load(MosseloraArt.StoneWalked);
            int phase = stream >= 9 ? 3 : stream >= 6 ? 2 : stream >= 3 ? 1 : 0;
            switch (GardenSeason.ForStory(phase))
            {
                case Season.Spring: return MosseloraArt.Load("stone-spring") ?? MosseloraArt.Load(MosseloraArt.StoneSummer);
                case Season.Autumn: return MosseloraArt.Load(MosseloraArt.StoneAutumn);
                case Season.Winter: return MosseloraArt.Load(MosseloraArt.StoneWinter);
                default: return MosseloraArt.Load(MosseloraArt.StoneSummer);
            }
        }

        // Stones ahead wear the season they will be walked in (spring mint, summer
        // sage, autumn amber, winter frost): the year ahead is visible, never timed.
        private static Color SeasonStone(int stream)
        {
            int phase = stream >= 9 ? 3 : stream >= 6 ? 2 : stream >= 3 ? 1 : 0;
            switch (GardenSeason.ForStory(phase))
            {
                // Opaque season tones, each at least 3:1 against the paper
                // (WCAG 1.4.11); the old translucent stones measured about 1.6:1 (I84).
                case Season.Spring: return new Color(0.27f, 0.53f, 0.43f, 1f); // spring mint, darker than summer for 3:1
                case Season.Autumn: return new Color(0.72f, 0.47f, 0.20f, 1f);   // autumn 3.3:1
                case Season.Winter: return new Color(0.36f, 0.52f, 0.66f, 1f);   // winter 3.5:1
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
        private GameObject Painting(string name, string art, float maxHeight)
        {
            Sprite sprite = MosseloraArt.Load(art);
            if (sprite == null) return null;
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
            return node;
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
                return Mathf.Max(225f, content.rect.width - 34f - columnLeft - columnRight);
            return Mathf.Max(225f, Screen.safeArea.width / LiveCanvasScale() - 34f - columnLeft - columnRight);
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
                        () => { confirmReset = false; Rebuild(); }, 44);
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
            menuSheet.SetActive(menuOpen);
            journalSheet.SetActive(showJournal && !menuOpen);
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

        private void MarkUnreadMemory(Button book)
        {
            int seen = PlayerPrefs.GetInt(JournalSeenKey, 0);
            if (seen > world.Choices.Length)
            {
                // Read in another garden (restored or new): this garden's memories so far count as seen.
                PlayerPrefs.SetInt(JournalSeenKey, world.Choices.Length);
                PlayerPrefs.Save();
                return;
            }
            if (world.Choices.Length == seen) return;
            // On the book's inner (top-left) corner: at the outer corner it sat half
            // off the screen's edge and a cold read took it for a glitch (I145).
            var note = MosseloraArt.Ornament(book.transform, "Has a new memory", MosseloraArt.Rounded(12),
                new Vector2(0f, 1f), new Vector2(0.5f, 0.5f), new Vector2(26f, 26f), new Vector2(4f, -4f));
            if (note == null) return;
            note.preserveAspect = false;
            note.color = MosseloraTheme.CardFill;
            MosseloraArt.Lift(note, 1.5f, 0.22f);
            MosseloraArt.Ornament(note.transform, "Sprig mark", MosseloraArt.Load(MosseloraArt.Sprout),
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(17f, 17f), Vector2.zero);
            if (gentleMotion) BloomPulse.Add(note.gameObject, 0.7f, 0.18f);
        }

        private void MemoryJournal()
        {
            // The toggle is the book in the floating chrome (BuildChrome); the
            // scrapbook rises as its own sheet instead of lengthening the page.
            if (!showJournal) return;
            // Opening the book reads every memory so far: its new-memory mark clears (I112).
            PlayerPrefs.SetInt(JournalSeenKey, world.Choices.Length);
            PlayerPrefs.Save();
            RectTransform pageRoot = content;
            content = journalContent;
            SheetClose(journalContent, "Close journal", () => { showJournal = false; journalOpenPage = -1; almanacTab = false; familyTab = false; waysTab = false; Rebuild(); });
            content = pageRoot;
            JournalTabs();
            if (familyTab)
            {
                FamilyPage();
                return;
            }
            if (almanacTab)
            {
                AlmanacPage();
                return;
            }
            if (waysTab)
            {
                WaysPage();
                return;
            }
            string[] allEntries = LocalizedVillage.Journal(world);
            if (journalOpenPage >= 0 && journalOpenPage < allEntries.Length && journalOpenPage < world.Choices.Length)
            {
                JournalPageOpen(journalOpenPage, allEntries[journalOpenPage]);
                return;
            }
            journalOpenPage = -1;
            // The pages lie on the sheet's own paper: no card inside the sheet (I128).
            var journal = Card(journalContent, "A living journal", new Color(0f, 0f, 0f, 0f));
            Text journalTitle = Label(journal.transform, "Journal title",
                GameLocalization.Text("journal_title"), 18, 30, Leaf, TextAnchor.MiddleCenter);
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
                if (i < chosen.Length)
                {
                    // A page opens to its memory, full size (I166).
                    int pageIndex = i;
                    pageImage.raycastTarget = true;
                    var pageButton = page.AddComponent<Button>();
                    pageButton.targetGraphic = pageImage;
                    pageButton.colors = PressTint();
                    pageButton.onClick.AddListener(() => { journalOpenPage = pageIndex; Rebuild(); });
                    PressSquash.Add(page);
                    DogEar(page.transform);
                }
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
                // The painted stepping stone of that stream and its number, not a
                // small "STREAM 3 / 12" caption that read like a record (I135).
                var stone = Node("Season stone", when);
                var stoneLayout = stone.AddComponent<LayoutElement>();
                var stoneImage = stone.AddComponent<Image>();
                Sprite pathStone = PathStone(i, false, false);
                float stoneHeight = pathStone != null ? 22f : 12f;
                stoneLayout.minHeight = stoneLayout.preferredHeight = stoneHeight;
                stoneLayout.minWidth = stoneLayout.preferredWidth =
                    pathStone != null ? Mathf.Round(stoneHeight * MosseloraArt.Aspect(pathStone)) : 12f;
                if (pathStone != null) { stoneImage.sprite = pathStone; stoneImage.preserveAspect = true; }
                else { stoneImage.sprite = MosseloraArt.Rounded(4); stoneImage.color = SeasonStone(i); }
                stoneImage.raycastTarget = false;
                Text number = Label(when, "Journal stream number",
                    (i + 1).ToString(CultureInfo.InvariantCulture), 17, 0, Moss, TextAnchor.MiddleCenter);
                Emphasize(number);
                // The full "Stream 3 of 12" is spoken, not shown.
                Text streamName = Label(when, "Journal stream name",
                    GameLocalization.Text("stream", i + 1, PuzzleCatalog.Count), 13, 0,
                    new Color(0f, 0f, 0f, 0f), TextAnchor.MiddleCenter);
                streamName.GetComponent<LayoutElement>().ignoreLayout = true;
                Sprite icon = i < chosen.Length ? ItemArt(i, chosen[i]) : null;
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
                string remembered = i < chosen.Length ? Remembered(chosen, i) : entries[i];
                // The journal is read, not glanced at: body size, as on the memory card (I98).
                Narrate(Label(page.transform, "Remembered line", remembered, 17, 0, Ink, TextAnchor.UpperCenter));
                Text record = Label(page.transform, "Recorded cause and effect", entries[i], 14, 0,
                    new Color(0f, 0f, 0f, 0f), TextAnchor.UpperCenter);
                record.GetComponent<LayoutElement>().ignoreLayout = true;
            }
            if (!onePerRow && entries.Length % 2 == 1 && spread != null)
                Node("Scrapbook blank", spread.transform); // keeps the last page half width
            if (world.Journal.Length == 0)
                Label(journal.transform, "Blank page", GameLocalization.Text("journal_empty"), 14, 0, Moss);
        }

        // Every living-garden choice so far, saved to the device-wide almanac.
        private void RecordInAlmanac()
        {
            if (world == null || world.Mode != PlayMode.Living) return;
            int[] made = world.Choices;
            bool changed = false;
            for (int i = 0; i < made.Length && i < GardenCatalog.ChaptersPerGarden; i++)
                changed |= almanac.Add(ItemAt(i, made[i]));
            // Nursery plants and planted gifts from family are catalogue things too (I238).
            int[] bought = world.Bought;
            for (int k = 0; k * 2 + 1 < bought.Length; k++)
                changed |= almanac.Add(Nursery.Item(bought[k * 2]));
            if (!changed) return;
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            if (qaInMemoryOnly) return; // the tour never writes the player's almanac
#endif
            PlayerPrefs.SetString(AlmanacKey, almanac.Serialize());
            PlayerPrefs.Save();
        }

        // Two round tabs at the top of the book: your pages, and the almanac (I174).
        private void JournalTabs()
        {
            if (world.Mode != PlayMode.Living) { almanacTab = false; return; }
            Transform tabs = CenteredRowIn(journalContent, "Book tabs");
            tabs.GetComponent<HorizontalLayoutGroup>().spacing = 18;
            bool pagesTab = !almanacTab && !familyTab && !waysTab;
            IconButton(tabs, "Our pages", GameLocalization.Text("journal_title"), MosseloraArt.IconJournal,
                () => { almanacTab = false; familyTab = false; waysTab = false; journalOpenPage = -1; Rebuild(); }, 56,
                pagesTab ? MosseloraTheme.ForestAction : MosseloraTheme.QuietAction, false, null, false, 0, 0, true);
            Button almanacButton = IconButton(tabs, "Almanac", "Almanac", MosseloraArt.IconJournal,
                () => { almanacTab = true; familyTab = false; waysTab = false; journalOpenPage = -1; Rebuild(); }, 56,
                almanacTab ? MosseloraTheme.ForestAction : MosseloraTheme.QuietAction, false, null, false, 0, 0, true);
            // I403: the garden's ways, under a sprig: who visits, good neighbours, the year.
            IconButton(tabs, "The garden's ways", "The garden's ways", "glyph-sprig",
                () => { waysTab = true; almanacTab = false; familyTab = false; journalOpenPage = -1; Rebuild(); }, 56,
                waysTab ? MosseloraTheme.ForestAction : MosseloraTheme.QuietAction, false, null, false, 0, 0, true);
            // The family page (I269): letters, help and puzzles played together.
            IconButton(tabs, "Family in the garden", "Family in the garden", MosseloraArt.IconNeighbors,
                () => { familyTab = true; almanacTab = false; waysTab = false; journalOpenPage = -1; Rebuild(); }, 56,
                familyTab ? MosseloraTheme.ForestAction : MosseloraTheme.QuietAction, false, null, false, 0, 0, true);
            // The almanac's mark is a painted sapling, not a second book.
            Transform glyph = almanacButton.transform.Find("Icon");
            Image glyphImage = glyph != null ? glyph.GetComponent<Image>() : null;
            Sprite sapling = MosseloraArt.Load(GardenCatalog.Get(VillageChoice.ShadeTree, 0).Art);
            if (glyphImage != null && sapling != null)
            {
                glyphImage.sprite = sapling;
                glyphImage.color = Color.white;
                var glyphSize = glyph.GetComponent<LayoutElement>();
                glyphSize.minWidth = glyphSize.preferredWidth = 38f;
                glyphSize.minHeight = glyphSize.preferredHeight = 38f;
            }
        }

        // All 200 garden items: made ones painted, the rest as faint silhouettes.
        // Counts per neighbour, no rewards, no pressure (E2, I174).
        // I269: the family layer, remembered truly from the save. No dates, no counts to beat.
        private void FamilyPage()
        {
            var page = Card(journalContent, "Family in the garden", new Color(0f, 0f, 0f, 0f));
            string[] letters = world.Letters, from = world.LetterFrom;
            int[] helped = world.Helped, together = world.Together;
            if (letters.Length == 0 && helped.Length == 0 && together.Length == 0)
            {
                Narrate(Label(page.transform, "Nothing yet",
                    "When family write, help, or play a puzzle with you, it is kept here.", 18, 0, Moss,
                    TextAnchor.MiddleCenter), true);
                return;
            }
            if (letters.Length > 0)
            {
                FamilyHeading(page.transform, "letter-sealed", "Letters");
                for (int i = letters.Length - 1; i >= 0 && i >= letters.Length - 6; i--)
                {
                    string first = letters[i].Split('\n')[0];
                    if (first.Length > 48) first = first.Substring(0, 46).TrimEnd() + "\u2026";
                    Narrate(Label(page.transform, "Letter from " + from[i], "\u2014 " + from[i] + ": \u201c" + first + "\u201d",
                        17, 0, Ink, TextAnchor.MiddleLeft), true);
                }
            }
            if (helped.Length > 0)
            {
                FamilyHeading(page.transform, MosseloraArt.IconNeighbors, "Helped");
                int[] made = world.Choices;
                foreach (int item in helped)
                {
                    if (item < 0 || item >= made.Length) continue;
                    Narrate(Label(page.transform, "Helped with " + item,
                        world.HelpedBy(item) + " helped with your " + ItemAt(item, made[item]).Name + ".",
                        17, 0, Ink, TextAnchor.MiddleLeft), true);
                }
            }
            if (together.Length > 0)
            {
                FamilyHeading(page.transform, MosseloraArt.IconNeighbors, "Played together");
                int[] made = world.Choices, bought = world.Bought;
                foreach (int thing in together)
                {
                    string name = thing < made.Length ? ItemAt(thing, made[thing]).Name
                        : thing >= BoughtIndex && (thing - BoughtIndex) * 2 < bought.Length
                            ? Nursery.Item(bought[(thing - BoughtIndex) * 2]).Name : null;
                    if (name == null) continue;
                    Narrate(Label(page.transform, "Together " + thing, "The " + name + ", taking turns.",
                        17, 0, Ink, TextAnchor.MiddleLeft), true);
                }
            }
        }

        private void FamilyHeading(Transform page, string art, string words)
        {
            Transform head = CenteredRowIn(page, "Family heading");
            head.GetComponent<HorizontalLayoutGroup>().spacing = 8;
            var icon = Node("Mark", head);
            var iconLayout = icon.AddComponent<LayoutElement>();
            iconLayout.minWidth = iconLayout.preferredWidth = 34;
            iconLayout.minHeight = iconLayout.preferredHeight = 34;
            var iconImage = icon.AddComponent<Image>();
            iconImage.sprite = MosseloraArt.Load(art);
            iconImage.preserveAspect = true;
            iconImage.raycastTarget = false;
            if (art == MosseloraArt.IconNeighbors) iconImage.color = Leaf;
            Emphasize(Label(head, words, words, 19, 0, Moss, TextAnchor.MiddleLeft));
        }

        // I403: the garden's ways: what lives here and why, which good neighbours there are, and
        // what each season gives. The almanac keeps the collections; this page keeps the logic.
        private void WaysPage()
        {
            var page = Card(journalContent, "The garden's ways", new Color(0f, 0f, 0f, 0f));
            PairingRows(page.transform); // I369
            VisitorsRows(page.transform);
            YearRows(page.transform); // I392
        }

        private void AlmanacPage()
        {
            var page = Card(journalContent, "The almanac", new Color(0f, 0f, 0f, 0f));
            MemoryShelfRow(page.transform);
            foreach (VillageChoice family in new[] { VillageChoice.ShadeTree, VillageChoice.Flowers, VillageChoice.Bridge })
            {
                Transform head = CenteredRowIn(page.transform, "Almanac family");
                head.GetComponent<HorizontalLayoutGroup>().spacing = 8;
                Sprite face = MosseloraArt.Portrait((int)family);
                if (face != null)
                {
                    var faceNode = Node("Keeper", head);
                    var faceLayout = faceNode.AddComponent<LayoutElement>();
                    faceLayout.minWidth = faceLayout.preferredWidth = 40;
                    faceLayout.minHeight = faceLayout.preferredHeight = 40;
                    var faceImage = faceNode.AddComponent<Image>();
                    faceImage.sprite = face;
                    faceImage.preserveAspect = true;
                    faceImage.raycastTarget = false;
                }
                int count = GardenCatalog.CountOf(family);
                Text tally = Label(head, "Found", almanac.CountOf(family) + " / " + count, 18, 0, Moss,
                    TextAnchor.MiddleLeft);
                Emphasize(tally);
                var grid = Node("Almanac grid", page.transform);
                var layout = grid.AddComponent<GridLayoutGroup>();
                float cell = Mathf.Floor(Mathf.Min(52f, (PageWidth() - 40f) / 7f - 4f));
                layout.cellSize = new Vector2(cell, cell);
                layout.spacing = new Vector2(4f, 4f);
                layout.childAlignment = TextAnchor.UpperCenter;
                layout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
                layout.constraintCount = 7;
                int rows = (count + 6) / 7;
                Height(grid, rows * cell + (rows - 1) * 4f);
                for (int i = 0; i < count; i++)
                {
                    GardenItem item = GardenCatalog.Get(family, i);
                    Sprite art = MosseloraArt.Load(item.Art);
                    var slot = Node(item.Name, grid.transform);
                    var image = slot.AddComponent<Image>();
                    image.sprite = art;
                    image.preserveAspect = true;
                    bool found = almanac.Has(item);
                    // Not yet made: a soft pencil silhouette of the same painting.
                    image.color = found ? Color.white : new Color(0.33f, 0.30f, 0.25f, 0.16f);
                    image.raycastTarget = true;
                    if (!found)
                    {
                        // Where an unfound thing grows (I241); its name stays a surprise.
                        int sold = Nursery.KindOf(item);
                        bool now = sold >= 0 && world.Mode == PlayMode.Living &&
                            world.NurseryStock(Nursery.Slot(sold)) == sold;
                        string where = now ? "In the nursery now" : sold >= 0 ? "Comes to the nursery in time"
                            : "In a garden yet to come";
                        var hint = slot.AddComponent<Button>();
                        hint.transition = Selectable.Transition.None;
                        hint.onClick.AddListener(() => RuleToast.Show(slot.GetComponent<RectTransform>(),
                            where, font, Ink, MosseloraTheme.CardFill));
                        continue;
                    }
                    var name = slot.AddComponent<Button>();
                    name.transition = Selectable.Transition.None;
                    string label = item.Name;
                    // I304: its name, then a line from its keeper (true things only).
                    string said = char.ToUpperInvariant(label[0]) + label.Substring(1) + "\n" + Almanac.KeeperLine(item);
                    name.onClick.AddListener(() => RuleToast.Show(slot.GetComponent<RectTransform>(),
                        said, font, Ink, MosseloraTheme.CardFill));
                }
            }
        }


        // I392: the garden's year: what each season gives and why, the season now in ink.
        private void YearRows(Transform page)
        {
            if (world == null || world.Mode != PlayMode.Living) return;
            Season now = GardenSeason.For(world);
            foreach (Season season in new[] { Season.Spring, Season.Summer, Season.Autumn, Season.Winter })
            {
                bool current = season == now;
                Transform row = CenteredRowIn(page, "The year " + season);
                var layout = row.GetComponent<HorizontalLayoutGroup>();
                layout.spacing = 10;
                layout.childAlignment = TextAnchor.MiddleLeft;
                var mark = Node(season.ToString(), row);
                var markLayout = mark.AddComponent<LayoutElement>();
                markLayout.preferredWidth = markLayout.minWidth = markLayout.preferredHeight = 36f;
                var markImage = mark.AddComponent<Image>();
                markImage.sprite = MosseloraArt.Load("stone-" + season.ToString().ToLowerInvariant());
                markImage.preserveAspect = true;
                markImage.color = current ? Color.white : new Color(1f, 1f, 1f, 0.45f);
                markImage.raycastTarget = false;
                Text words = Label(row, "Why", SeasonalYields.Why(season), 16, 0, current ? Ink : new Color(0.4f, 0.36f, 0.3f, 0.7f),
                    TextAnchor.MiddleLeft);
                words.GetComponent<LayoutElement>().flexibleWidth = 1f;
            }
        }

        // I369: the good neighbours a garden has found, and the ones still to find, as hints.
        private void PairingRows(Transform page)
        {
            if (world == null || world.Mode != PlayMode.Living) return;
            bool deeperOpen = (Pairings.Open(world) & ~Pairings.FirstFour) != 0;
            foreach (Pairing pairing in Enum.GetValues(typeof(Pairing)))
            {
                if (Pairings.Deeper(pairing) && !deeperOpen) continue; // I381: revealed once the four are found
                bool found = world.HasPairing(pairing);
                Transform row = CenteredRowIn(page, "Good neighbours " + pairing);
                var layout = row.GetComponent<HorizontalLayoutGroup>();
                layout.spacing = 10;
                layout.childAlignment = TextAnchor.MiddleLeft;
                var mark = Node(pairing.ToString(), row);
                var markLayout = mark.AddComponent<LayoutElement>();
                markLayout.preferredWidth = markLayout.minWidth = markLayout.preferredHeight = 36f;
                var markImage = mark.AddComponent<Image>();
                markImage.sprite = MosseloraArt.Load(pairing == Pairing.Sheltered ? "flower-05" : pairing == Pairing.ByTheWater
                    ? "build-00" : pairing == Pairing.FlowerBed ? "flower-12" : pairing == Pairing.Waterside ? "flower-08"
                    : pairing == Pairing.ShadySeat ? "build-03" : pairing == Pairing.Orchard ? "tree-05" : "tree-00");
                markImage.preserveAspect = true;
                markImage.color = found ? Color.white : new Color(0.33f, 0.30f, 0.25f, 0.18f);
                markImage.raycastTarget = false;
                Text words = Label(row, "Hint", Pairings.Hint(pairing) + (found ? " " + Pairings.Brings(pairing) : ""), 16, 0,
                    found ? Ink : new Color(0.4f, 0.36f, 0.3f, 0.7f), TextAnchor.MiddleLeft); // I401: once found, what it brings
                words.GetComponent<LayoutElement>().flexibleWidth = 1f;
            }
            // I381: a promise, not a list: the deeper three show themselves once the four are found.
            if (!deeperOpen)
            {
                Transform more = CenteredRowIn(page, "Good neighbours still to come");
                Label(more, "More", "Three more, once these four are found.", 15, 0, new Color(0.4f, 0.36f, 0.3f, 0.6f), TextAnchor.MiddleCenter);
            }
        }

        // I327 (idea 19, after the joy of a full page in creature-collecting games): who visits
        // the garden and why. Each says, truly, what brings it; those who've come are in ink.
        private void VisitorsRows(Transform page)
        {
            if (world == null || world.Mode != PlayMode.Living) return;
            foreach (Visitor visitor in Enum.GetValues(typeof(Visitor)))
            {
                bool come = Visitors.Welcomes(world, visitor);
                Transform row = CenteredRowIn(page, "Visitor " + visitor);
                var layout = row.GetComponent<HorizontalLayoutGroup>();
                layout.spacing = 10;
                layout.childAlignment = TextAnchor.MiddleLeft;
                var mark = Node(visitor.ToString(), row);
                var markLayout = mark.AddComponent<LayoutElement>();
                markLayout.preferredWidth = markLayout.minWidth = markLayout.preferredHeight = 36f;
                var markImage = mark.AddComponent<Image>();
                markImage.sprite = MosseloraArt.Rounded(12);
                markImage.color = !come ? new Color(0.33f, 0.30f, 0.25f, 0.16f)
                    : visitor == Visitor.Bees ? new Color(0.98f, 0.78f, 0.18f, 1f)
                    : visitor == Visitor.Birds ? new Color(0.45f, 0.38f, 0.30f, 1f)
                    : visitor == Visitor.Heron ? new Color(0.62f, 0.68f, 0.74f, 1f)
                    : visitor == Visitor.Hedgehog ? new Color(0.55f, 0.42f, 0.30f, 1f)
                    : visitor == Visitor.Owl ? new Color(0.50f, 0.40f, 0.28f, 1f)
                    : visitor == Visitor.Kingfisher ? new Color(0.16f, 0.55f, 0.78f, 1f) : new Color(0.93f, 0.62f, 0.72f, 1f);
                markImage.raycastTarget = false;
                Text why = Label(row, "Why", Visitors.Why(visitor), 16, 0, come ? Ink : new Color(0.4f, 0.36f, 0.3f, 0.7f),
                    TextAnchor.MiddleLeft);
                why.GetComponent<LayoutElement>().flexibleWidth = 1f;
            }
        }

        // I314: the memory shelf, your firsts, above the almanac's collections. Kept ones in
        // colour; the rest are pencil silhouettes, like unfound almanac items.
        private void MemoryShelfRow(Transform page)
        {
            if (world == null || world.Mode != PlayMode.Living) return;
            Keepsake[] shelf = MemoryShelf.For(world);
            var grid = Node("Memory shelf", page);
            var layout = grid.AddComponent<GridLayoutGroup>();
            float cell = Mathf.Floor(Mathf.Min(64f, (PageWidth() - 40f) / 4f - 8f));
            layout.cellSize = new Vector2(cell, cell);
            layout.spacing = new Vector2(8f, 8f);
            layout.childAlignment = TextAnchor.UpperCenter;
            layout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            layout.constraintCount = 4;
            int rows = (shelf.Length + 3) / 4;
            Height(grid, rows * cell + (rows - 1) * 8f + 12f);
            foreach (Keepsake keepsake in shelf)
            {
                var slot = Node(keepsake.Kept ? keepsake.Label : "Not yet", grid.transform);
                var image = slot.AddComponent<Image>();
                image.sprite = MosseloraArt.Load(keepsake.Art);
                image.preserveAspect = true;
                bool icon = keepsake.Art.StartsWith("ui-");
                image.color = keepsake.Kept ? (icon ? MosseloraTheme.ForestAction : Color.white)
                    : new Color(0.33f, 0.30f, 0.25f, 0.16f);
                var tap = slot.AddComponent<Button>();
                tap.transition = Selectable.Transition.None;
                string said = keepsake.Kept ? keepsake.Label : "Not yet";
                tap.onClick.AddListener(() => RuleToast.Show(slot.GetComponent<RectTransform>(), said, font, Ink, MosseloraTheme.CardFill));
            }
        }

        // A folded top-right corner: the page can be turned (I168). A square turned
        // 45 degrees on the corner, clipped by the page, reads as a dog-ear.
        private static void DogEar(Transform page)
        {
            if (page.GetComponent<RectMask2D>() == null) page.gameObject.AddComponent<RectMask2D>();
            var fold = new GameObject("Page corner", typeof(RectTransform));
            fold.transform.SetParent(page, false);
            fold.AddComponent<LayoutElement>().ignoreLayout = true;
            var rect = fold.GetComponent<RectTransform>();
            rect.anchorMin = rect.anchorMax = new Vector2(1f, 1f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = new Vector2(26f, 26f);
            rect.anchoredPosition = Vector2.zero;
            rect.localRotation = Quaternion.Euler(0f, 0f, 45f);
            var image = fold.AddComponent<Image>();
            image.sprite = MosseloraArt.Rounded(3);
            image.type = Image.Type.Sliced;
            image.color = MosseloraTheme.CardEdge;
            image.raycastTarget = false;
            MosseloraArt.Lift(image, 1.5f, 0.18f);
        }

        // One memory, opened from the book: its painting, who remembers it, the
        // remembered line and the full cause-and-effect record. A tap anywhere on
        // the page (or the back token) returns to the book (I166).
        private void JournalPageOpen(int index, string record)
        {
            int[] chosen = world.Choices;
            var choice = (VillageChoice)chosen[index];
            var open = Card(journalContent, "Open memory", new Color(0f, 0f, 0f, 0f));
            var back = open.AddComponent<Button>();
            var hit = open.GetComponent<Image>();
            hit.raycastTarget = true;
            back.targetGraphic = hit;
            back.transition = Selectable.Transition.None;
            back.onClick.AddListener(() => { journalOpenPage = -1; Rebuild(); });
            Transform when = CenteredRowIn(open.transform, "Open memory stream");
            when.GetComponent<HorizontalLayoutGroup>().spacing = 6;
            Sprite stone = PathStone(index, false, false);
            if (stone != null)
            {
                var stoneNode = Node("Season stone", when);
                var stoneLayout = stoneNode.AddComponent<LayoutElement>();
                stoneLayout.minHeight = stoneLayout.preferredHeight = 26f;
                stoneLayout.minWidth = stoneLayout.preferredWidth = Mathf.Round(26f * MosseloraArt.Aspect(stone));
                var stoneImage = stoneNode.AddComponent<Image>();
                stoneImage.sprite = stone;
                stoneImage.preserveAspect = true;
                stoneImage.raycastTarget = false;
            }
            Text number = Label(when, "Open memory number", (index + 1).ToString(CultureInfo.InvariantCulture),
                20, 0, Moss, TextAnchor.MiddleCenter);
            Emphasize(number);
            Text spokenStream = Label(when, "Open memory stream name",
                GameLocalization.Text("stream", index + 1, PuzzleCatalog.Count), 13, 0,
                new Color(0f, 0f, 0f, 0f), TextAnchor.MiddleCenter);
            spokenStream.GetComponent<LayoutElement>().ignoreLayout = true;
            Sprite vignette = MosseloraArt.Load(choice == VillageChoice.ShadeTree ? MosseloraArt.VignetteTree
                : choice == VillageChoice.Bridge ? MosseloraArt.VignetteBridge : MosseloraArt.VignetteFlowers);
            if (vignette == null) vignette = MosseloraArt.ChoiceIcon(choice);
            if (vignette != null)
            {
                var picture = Node("Open memory painting", open.transform);
                float width = Mathf.Min(PageWidth() - 24f, 420f);
                Height(picture, Mathf.Round(width / MosseloraArt.Aspect(vignette)));
                var pictureImage = picture.AddComponent<Image>();
                pictureImage.sprite = vignette;
                pictureImage.preserveAspect = true;
                pictureImage.raycastTarget = false;
                Sprite face = MosseloraArt.Portrait(chosen[index]);
                if (face != null)
                    MosseloraArt.Ornament(picture.transform, "Who remembers", face, new Vector2(1f, 0f),
                        new Vector2(1f, 0f), new Vector2(54f, 54f), new Vector2(-8f, 6f));
                ItemBadge(picture.transform, index, chosen[index]);
            }
            Narrate(Label(open.transform, "Remembered line", Remembered(chosen, index), 20, 0,
                Ink, TextAnchor.MiddleCenter), true);
            // Shown: only what changed ("The crossing connects both banks."). The
            // full record, with its "Chapter 1 · Orin · …" heading, stays spoken.
            int lineBreak = record.LastIndexOf('\n');
            string consequence = lineBreak >= 0 ? record.Substring(lineBreak + 1) : record;
            Narrate(Label(open.transform, "What changed", consequence, 17, 0, Ink, TextAnchor.MiddleCenter));
            Text fullRecord = Label(open.transform, "Recorded cause and effect", record, 14, 0,
                new Color(0f, 0f, 0f, 0f), TextAnchor.MiddleCenter);
            fullRecord.GetComponent<LayoutElement>().ignoreLayout = true;
            Transform backRow = CenteredRowIn(open.transform, "Back to the book row");
            IconButton(backRow, "Back to the book", GameLocalization.Text("journal_title"),
                MosseloraArt.IconUndo, () => { journalOpenPage = -1; Rebuild(); }, 64,
                MosseloraTheme.QuietAction, false, null, false, 0, 0, true);
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
                else if (spentTipHeight > 0f && !landscape)
                {
                    // Any later rebuild of the first board (the solve, the menu, text
                    // size) keeps the spent tip's space, so the board never jumps; the
                    // solve's own glide is then the only movement (I141).
                    var space = Node("Spent tip space", content);
                    MinHeight(space, spentTipHeight);
                }
            }
            else spentTipHeight = 0f;
            // Later boards open on play and the place alone: the generic line that
            // stood over every board ("A little water can change a place.") said
            // nothing about this board, so it is gone (I133).
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
            RectTransform bedFrame = null;
            if (bed != null)
            {
                var bedNode = Node("Garden bed", board.transform);
                bedNode.AddComponent<LayoutElement>().ignoreLayout = true;
                var bedRect = bedNode.GetComponent<RectTransform>();
                Fill(bedRect);
                bedRect.offsetMin = new Vector2(-10f, -10f);
                bedRect.offsetMax = new Vector2(10f, 10f);
                bedFrame = bedRect;
                var bedImage = bedNode.AddComponent<Image>();
                bedImage.sprite = bed;
                bedImage.raycastTarget = false;
                bedNode.transform.SetAsFirstSibling();
                // The board sits in the landscape (I110): this season's painted creek,
                // dry until the stream is solved and flowing after, softly behind the
                // stone bed. Tiles stay opaque parchment, so the puzzle reads the same.
                // Living world only; the Puzzle-only control keeps a plain page.
                if (world.Mode == PlayMode.Living && !landscape && !worldShownNow) CreekBehindBoard(board.transform);
            }
            var gridNode = Node("Water connections", board.transform);
            // Keep the entire puzzle inside a compact phone, including the
            // story card's horizontal padding and safe-area insets.
            int gap = puzzle.Size == 3 ? 7 : 5;
            // CanvasScaler.scaleFactor is only the constant-pixel field (always 1
            // here); the live ScaleWithScreenSize factor gives true logical width.
            float layoutWidth = Screen.safeArea.width / LiveCanvasScale() - columnLeft - columnRight;
            // Standard keeps the EG-007 sizing exactly. Large (EG-112) trims the
            // card's side padding and lifts the tile cap so the board uses the width.
            float available = Mathf.Max(225f, layoutWidth - 34f - 20f - 5f);
            // Pure and tested (BoardFit, I118): portrait by width; sideways by the
            // height left over, with the tool rail beside an unsolved board (I114-I116).
            int standardTile = landscape
                ? BoardFit.Landscape(Screen.safeArea.height / LiveCanvasScale(), available,
                    puzzle.Size, gap, world.Solved, landscapeTablet ? BoardFit.TabletCap : 0)
                : BoardFit.Portrait(available, puzzle.Size, gap);
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
            // Sideways the screen height is the limit for every board size (I116).
            if (landscape) tileSize = Mathf.Min(tileSize, standardTile);
            // Markers, badges and endpoint icons scale with the tile; Standard is 1.
            float markerScale = tileSize / (float)standardTile;
            int span = (tileSize + gap) * puzzle.Size - gap;
            Height(gridNode, span + 1);
            // Sideways the stone bed hugs the tiles, not the whole column (I116);
            // while unsolved the board steps left to make room for the tool rail.
            float railShift = landscape && !world.Solved ? ToolRailWidth + ToolRailGap : 0f;
            if (railShift > 0f)
            {
                var boardLayout = board.GetComponent<VerticalLayoutGroup>();
                boardLayout.padding = new RectOffset(boardLayout.padding.left,
                    boardLayout.padding.right + (int)railShift, boardLayout.padding.top, boardLayout.padding.bottom);
            }
            if (landscape && bedFrame != null)
            {
                bedFrame.anchorMin = new Vector2(0.5f, 0f);
                bedFrame.anchorMax = new Vector2(0.5f, 1f);
                bedFrame.sizeDelta = new Vector2(span + 48f, 20f);
                bedFrame.anchoredPosition = new Vector2(-railShift / 2f, 0f);
            }
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
            // Before the very first move, one tile that needs turning wears a small
            // turning-arrow mark: "tap turns a tile", shown rather than told (I138).
            int firstTurn = world.Round == 0 && !world.CanUndo && !world.Solved
                ? HintPlanner.SelectTarget(puzzle, rotations) : -1;
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
                    int rootedFamily = Mathf.Clamp(world.BoardVariant, 1, 3) - 1;
                    Sprite holder = world.Round > 0 ? ItemArt(world.Round - 1, rootedFamily)
                        : MosseloraArt.ChoiceIcon((VillageChoice)rootedFamily);
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
                // Every board, not only the first: a cold read of a later board could
                // not tell what finishing it meant (I148). Gone after the first move.
                if (!hintedHere && puzzle.Kinds[index] == PipeKind.End && !world.CanUndo && !world.Solved)
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
                if (index == firstTurn) TurnMark(tileNode.transform, tileSize);
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
                    // I349 (run 3, P12): the heading keeps clear of the floating menu button on
                    // both sides, so no scroll position can hide its first letters under it.
                    var titleRow = Node("Choose title row", content);
                    var titleGroup = titleRow.AddComponent<HorizontalLayoutGroup>();
                    titleGroup.padding = new RectOffset(72, 72, 0, 0);
                    titleGroup.childControlWidth = titleGroup.childControlHeight = true;
                    titleGroup.childForceExpandWidth = true;
                    titleGroup.childForceExpandHeight = false;
                    Text chooseTitle = Label(titleRow.transform, "Choose title", GameLocalization.Text("choose"),
                        24, 56, Ink, TextAnchor.MiddleCenter);
                    Emphasize(chooseTitle);
                    decisionHeading = chooseTitle.rectTransform;
                    Text chooseHelp = Label(content, "Choice preview help", GameLocalization.Text("preview_help"),
                        16, 0, Ink, TextAnchor.MiddleCenter);
                    // I393 (run 3, P18): said once, at the first choice; after that it is known,
                    // and still spoken to a screen reader.
                    if (world.Round > 0) SpokenOnly(chooseHelp);
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
                    decisionCards = choiceRow.GetComponent<RectTransform>();
                    for (int choiceIndex = 0; choiceIndex < 3; choiceIndex++)
                    {
                        VillageChoice choice = (VillageChoice)choiceIndex;
                        // None is visually preselected. A golden default would
                        // bias research toward the first habitat, not player care.
                        Transform cards = choiceRow.transform;
                        ChoiceCard(cards, choice, () => PreviewChoice(choice), stacked);
                    }
                    BuildChoicePreview(choiceRow.transform);
                }
                // No "revisit the water" token here any more (I169): once the stream
                // runs, the only decision is what to grow. A cold read took the round
                // undo arrow for "cancel the choice", and second thoughts about the
                // choice itself already live on the memory card ("change my choice").
            }
            else
            {
                var tools = Row(content, "Gentle controls");
                // Progressive disclosure: the first stream teaches the three tools
                // with small captions; afterwards they are pictures only.
                // Icon-only in every stream: Mira's coach bubble teaches the board,
                // and every tool names itself on press-and-hold (and is spoken).
                bool teach = false;
                // Upright, the tools are two round tokens flanking the water drops,
                // not two wide slabs under them (I129). Sideways they stand in a rail.
                // A cold read preferred the tokens but asked for them bigger: 76pt upright.
                bool roundTools = true;
                int toolHeight = landscape ? 72 : 76;
                undoButton = IconButton(tools.transform, "Undo", GameLocalization.Text("undo"),
                    MosseloraArt.IconUndo, () => Act(() => world.Undo(), "undo"),
                    toolHeight, MosseloraTheme.ForestAction, false, null, teach, 0, 0, roundTools);
                undoButton.interactable = world.CanUndo;
                ShowUndo(world.CanUndo, false);
                // Hint depth shows as a small 1/2 or 2/2 badge, not words.
                Button hintButton = IconButton(tools.transform, "Hint",
                    GameLocalization.Text(world.HintStep == 2 ? "hint_turn" :
                        world.HintStep == 1 ? "hint_more" : "hint"),
                    MosseloraArt.IconHint, UseGentleHint, toolHeight, MosseloraTheme.WaterAction, false,
                    null, teach, world.HintStep > 0 ? 2 : 0, world.HintStep, roundTools);
                if (!landscape) FlankDrops(tools, toolHeight);
                Transform hintDots = hintButton.transform.Find("Level dots");
                if (hintDots != null) hintMarkers.Add(hintDots.gameObject); // a move ends the hint: dots go too
                if (landscape && solvedBoard != null) DockToolRail(tools, toolHeight);
                // Restart stream lives in the menu (I109): it clears the undo history,
                // is rarely needed, and its arrow sat beside Undo's. The board keeps
                // only the two tools a player reaches for: Undo and Guide me.
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

        // Undo | water drops | Hint on one line under the board (I129). When a big
        // board's drops are too wide to sit between the tokens, they keep their own
        // line above, and the tokens stand at either end.
        private void FlankDrops(GameObject tools, int toolHeight)
        {
            var across = tools.GetComponent<HorizontalLayoutGroup>();
            across.childForceExpandWidth = false;
            across.childAlignment = TextAnchor.MiddleCenter;
            across.spacing = 10;
            MinHeight(tools, toolHeight);
            Transform meter = waterDrops != null && waterDrops.Length > 0 ? waterDrops[0].transform.parent : null;
            float room = PageWidth() - 2f * toolHeight - 2f * across.spacing - 8f;
            if (meter != null)
            {
                LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)meter);
                float wanted = LayoutUtility.GetPreferredWidth((RectTransform)meter);
                // A big board's drops stack in two rows at full size between the
                // tokens, instead of one shrunken row (I139: 16 drops were ~11pt).
                if (wanted > room && waterDrops.Length > 9 && waterDrops[0].sprite != null)
                {
                    int columns = Mathf.CeilToInt(waterDrops.Length / 2f);
                    float dropHeight = 22f;
                    float dropWidth = Mathf.Round(dropHeight * MosseloraArt.Aspect(waterDrops[0].sprite));
                    float gapX = 4f, gapY = 4f;
                    float gridWidth = columns * dropWidth + (columns - 1) * gapX;
                    if (gridWidth <= room)
                    {
                        DestroyImmediate(meter.GetComponent<HorizontalLayoutGroup>());
                        var stack = meter.gameObject.AddComponent<GridLayoutGroup>();
                        stack.cellSize = new Vector2(dropWidth, dropHeight);
                        stack.spacing = new Vector2(gapX, gapY);
                        stack.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
                        stack.constraintCount = columns;
                        stack.childAlignment = TextAnchor.MiddleCenter;
                        var stackLayout = meter.GetComponent<LayoutElement>();
                        if (stackLayout == null) stackLayout = meter.gameObject.AddComponent<LayoutElement>();
                        stackLayout.minWidth = stackLayout.preferredWidth = gridWidth;
                        stackLayout.minHeight = stackLayout.preferredHeight = 2f * dropHeight + gapY;
                        wanted = gridWidth;
                    }
                }
                // Otherwise a 4x4 board's sixteen drops shrink a little (to 70% at most) to fit.
                float shrink = wanted > room ? room / wanted : 1f;
                if (shrink >= 0.7f && shrink < 1f && meter.GetComponent<HorizontalLayoutGroup>() != null)
                {
                    foreach (Image drop in waterDrops)
                    {
                        var dropLayout = drop.GetComponent<LayoutElement>();
                        dropLayout.minWidth = dropLayout.preferredWidth = Mathf.Floor(dropLayout.preferredWidth * shrink);
                        dropLayout.minHeight = dropLayout.preferredHeight = Mathf.Floor(dropLayout.preferredHeight * shrink);
                    }
                    var meterGroup = meter.GetComponent<HorizontalLayoutGroup>();
                    meterGroup.spacing = Mathf.Floor(meterGroup.spacing * shrink);
                    wanted *= shrink;
                }
                if (wanted <= room)
                {
                    meter.SetParent(tools.transform, false);
                    meter.SetSiblingIndex(1);
                    var meterLayout = meter.GetComponent<LayoutElement>();
                    if (meterLayout == null) meterLayout = meter.gameObject.AddComponent<LayoutElement>();
                    meterLayout.flexibleWidth = 1f;
                    return;
                }
            }
            var gap = Node("Between the tools", tools.transform);
            gap.AddComponent<LayoutElement>().flexibleWidth = 1f;
            gap.transform.SetSiblingIndex(1);
        }

        // Undo above Hint, standing right of the board (landscape, unsolved).
        private void DockToolRail(GameObject tools, int toolHeight)
        {
            var across = tools.GetComponent<HorizontalLayoutGroup>();
            if (across != null) DestroyImmediate(across);
            var down = tools.AddComponent<VerticalLayoutGroup>();
            down.spacing = ToolRailGap;
            down.childAlignment = TextAnchor.MiddleCenter;
            down.childControlWidth = true;
            down.childControlHeight = true;
            down.childForceExpandWidth = true;
            down.childForceExpandHeight = false;
            tools.transform.SetParent(solvedBoard, false);
            var place = tools.GetComponent<LayoutElement>();
            if (place == null) place = tools.AddComponent<LayoutElement>();
            place.ignoreLayout = true;
            var rect = tools.GetComponent<RectTransform>();
            rect.anchorMin = rect.anchorMax = new Vector2(1f, 0.5f);
            rect.pivot = new Vector2(1f, 0.5f);
            rect.sizeDelta = new Vector2(ToolRailWidth, toolHeight * 2f + ToolRailGap);
            rect.anchoredPosition = Vector2.zero;
        }

        private void CoachBubble(string message)
        {
            // Sideways Mira speaks from the painting, pointing right at the board, so
            // the whole board and its tools fit the short screen on first play (I116).
            bool beside = landscape && landscapeGarden != null;
            var row = Node("Prompt", beside ? landscapeGarden.transform : content);
            if (beside)
            {
                var rowRect = row.GetComponent<RectTransform>();
                rowRect.anchorMin = new Vector2(0f, 0.36f);
                rowRect.anchorMax = new Vector2(1f, 0.36f);
                rowRect.pivot = new Vector2(0.5f, 0f);
                // Clear of the camera cutout when it sits on the painting's side.
                rowRect.offsetMin = new Vector2(16f + Screen.safeArea.xMin / LiveCanvasScale(), 8f);
                rowRect.offsetMax = new Vector2(-30f, 8f);
                row.AddComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            }
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
            // Since I151 the pictures are the tip at every size; at larger text the
            // sentence returns under them (it was hidden everywhere, leaving an
            // empty bubble at 125-150%; I163).
            bool showPicture = true;
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
            // Pictures only at standard text (I151): the board now demonstrates the
            // turn (I138 arrow, I147 finger), and a blind A/B read understood the
            // goal and the turn as well without the sentence. A player who has
            // asked for larger text also gets the sentence (I163). Always spoken.
            if (TextScale.Percent <= 100)
            {
                words.color = new Color(0f, 0f, 0f, 0f);
                words.GetComponent<LayoutElement>().ignoreLayout = true;
            }
            var tail = Node("Coach tail", bubble.transform);
            tail.AddComponent<LayoutElement>().ignoreLayout = true;
            var tailRect = tail.GetComponent<RectTransform>();
            tailRect.anchorMin = tailRect.anchorMax = beside ? new Vector2(1f, 0.5f) : new Vector2(0.5f, 0f);
            tailRect.sizeDelta = new Vector2(16f, 16f);
            tailRect.anchoredPosition = beside ? new Vector2(2f, 0f) : new Vector2(0f, -2f);
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
            bool selected = choicePreview != null && choicePreview.Choice == choice;
            image.color = selected ? MosseloraTheme.LeafSoft : MosseloraTheme.CardFill;
            MosseloraArt.Lift(image, 3f, 0.16f);
            MosseloraArt.Edge(node.transform, selected ? MosseloraTheme.LeafAction : MosseloraTheme.CardEdge, 18);
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
            Sprite icon = ItemArt(world.Round, (int)choice);
            if (icon != null)
            {
                var picture = Node("Choice picture", node.transform);
                var pictureLayout = picture.AddComponent<LayoutElement>();
                pictureLayout.minHeight = pictureLayout.preferredHeight = wide ? 84 : 104;
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
                // I196: the miniature next-stream tile (I134) read as a mystery box in
                // play; the next board shows that landmark itself, so the card is
                // just the thing and who will remember it.
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
            Text title = Label(words, "Choice name", ChoiceName(choice), 16, 0, Ink, align);
            Emphasize(title);
            if (!wide && !CjkLineBreakFixer.NeedsFitting(GameLocalization.Current))
                WholeWordFit.Attach(title, 12);
            // PR #82: the selected card has a shape cue as well as a tint; the full,
            // truthful consequence (from the real model) is in the preview before any commit.
            if (selected)
                Label(words, "Selected choice marker", "\u2713", 18, 22, Ink, align);
            VillageChoicePreview candidate;
            string outcome = VillageChoicePreview.TryCreate(world, choice, out candidate)
                ? LocalizedVillage.Consequence(candidate.WorldAfter)
                : LocalizedVillage.ChoiceOutcome(choice, world);
            // A hidden "Caption" gives screen readers the full choice.
            Text spoken = Label(node.transform, "Caption",
                ChoiceTitle(choice) + "\n" + outcome,
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
                    MosseloraArt.IconLanguage, () => { languagePicker = true; Rebuild(true); }, SettingToken,
                    MosseloraTheme.QuietAction, false, null, false, 0, 0, true);
            TextSizeButton(row.transform);
            BoardSizeButton(row.transform);
            row = Row(panel.transform, "Settings row 2");
            IconButton(row.transform, "Gentle motion setting",
                GameLocalization.Text("gentle_motion"),
                gentleMotion ? MosseloraArt.IconMotionOn : MosseloraArt.IconMotionOff,
                () => {
                    gentleMotion = !gentleMotion;
                    PipeGraphic.Animate = gentleMotion;
                    PlayerPrefs.SetInt(GentleMotionKey, gentleMotion ? 1 : 0);
                    PlayerPrefs.Save();
                    Rebuild();
                }, SettingToken, gentleMotion ? MosseloraTheme.ForestAction : MosseloraTheme.QuietAction,
                false, null, false, 0, 0, true);
            IconButton(row.transform, "Quiet audio setting",
                GameLocalization.Text(soundOn ? "sound_on" : "sound_off"),
                soundOn ? MosseloraArt.IconChimeOn : MosseloraArt.IconChimeOff,
                () => {
                    soundOn = !soundOn;
                    PlayerPrefs.SetInt("evergrove:gentle-audio", soundOn ? 1 : 0);
                    PlayerPrefs.Save();
                    Rebuild();
                }, SettingToken, soundOn ? MosseloraTheme.ForestAction : MosseloraTheme.QuietAction,
                false, null, false, 0, 0, true);
            // Light that follows the day (I280): on by default; off keeps the garden in clear daylight.
            IconButton(row.transform, "Day light setting",
                DayLightOn ? "The garden's light follows your day: on" : "The garden's light follows your day: off",
                MosseloraArt.IconSun, () => {
                    PlayerPrefs.SetInt(DayLightKey, DayLightOn ? 0 : 1);
                    PlayerPrefs.Save();
                    Rebuild();
                }, SettingToken, DayLightOn ? MosseloraTheme.ForestAction : MosseloraTheme.QuietAction,
                false, null, false, 0, 0, true);
            // Garden notes (I277): one switch in plain sight, and the hour when it's on.
            if (GardenNotes.Supported && world.Mode == PlayMode.Living)
            {
                row = Row(panel.transform, "Settings row 3");
                Button notes = IconButton(row.transform, "Garden notes setting",
                    GardenNotes.On ? "A note from the garden once a day: on" : "A note from the garden once a day: off",
                    "letter-sealed", () => {
                        if (GardenNotes.On) GardenNotes.TurnOff(); else GardenNotes.TurnOn(GardenNotes.Hour);
                        Rebuild();
                    }, SettingToken, GardenNotes.On ? MosseloraTheme.ForestAction : MosseloraTheme.QuietAction,
                    false, null, false, 0, 0, true);
                PaintedIcon(notes, GardenNotes.On ? 1f : 0.55f);
                if (GardenNotes.On)
                {
                    Button hour = IconButton(row.transform, "Garden note hour",
                        "Notes at " + GardenNotes.Hour + ":00", MosseloraArt.IconSun, () => {
                            int[] hours = { 9, 15, 19 };
                            int at = System.Array.IndexOf(hours, GardenNotes.Hour);
                            GardenNotes.TurnOn(hours[(at + 1) % hours.Length]);
                            Rebuild();
                        }, SettingToken, MosseloraTheme.QuietAction, false, null, false, 0, 0, true);
                    Text when = Label(hour.transform, "Note hour", GardenNotes.Hour + ":00", 15, 0, Ink, TextAnchor.LowerCenter);
                    when.GetComponent<LayoutElement>().ignoreLayout = true;
                    Fill(when.rectTransform);
                    when.rectTransform.offsetMin = new Vector2(0f, 4f);
                    when.horizontalOverflow = HorizontalWrapMode.Overflow;
                }
            }
            // An even grid: every tile half the panel wide, the odd one out centred
            // at the same width, instead of widths set by each caption (I143).
            // Round tokens (I194): the picture and its on/off tone say it; the name is
            // spoken and shown on press-and-hold. Rows centre their tokens, never stretch them.
            foreach (Transform settingsRow in panel.transform)
            {
                var group = settingsRow.GetComponent<HorizontalLayoutGroup>();
                if (group == null) continue;
                group.childForceExpandWidth = false;
                group.childAlignment = TextAnchor.MiddleCenter;
                group.spacing = 22;
            }
        }

        private const int SettingToken = 76;

        private static void EvenTiles(Transform row)
        {
            var group = row.GetComponent<HorizontalLayoutGroup>();
            if (group == null) return;
            group.childForceExpandWidth = false; // force-expand would give each spacer a full share
            int tiles = row.childCount;
            for (int i = 0; i < tiles; i++)
            {
                var layout = row.GetChild(i).GetComponent<LayoutElement>();
                if (layout == null) layout = row.GetChild(i).gameObject.AddComponent<LayoutElement>();
                if (layout.ignoreLayout) continue;
                layout.minWidth = 0f;
                layout.preferredWidth = 0f;
                layout.flexibleWidth = 1f;
            }
            if (tiles != 1) return;
            // A lone tile: quarter-width spacers either side keep it half wide.
            for (int side = 0; side < 2; side++)
            {
                var spacer = new GameObject("Even grid spacer", typeof(RectTransform));
                spacer.transform.SetParent(row, false);
                spacer.AddComponent<LayoutElement>().flexibleWidth = 0.5f;
                if (side == 0) spacer.transform.SetAsFirstSibling();
            }
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
                }, SettingToken, BoardSize.IsLarge ? MosseloraTheme.ForestAction : MosseloraTheme.QuietAction,
                false, null, false, 2, BoardSize.IsLarge ? 2 : 1, true);
        }

        // Language-neutral label: "Aa" plus the current size. Cycles 100/125/150%.
        private void TextSizeButton(Transform parent)
        {
            IconButton(parent, "Text size",
                GameLocalization.Text("text_size") + " " + TextScale.Percent + "%",
                MosseloraArt.IconTextSize, () => {
                    TextScale.Cycle();
                    Rebuild();
                }, SettingToken, TextScale.Percent > 100 ? MosseloraTheme.ForestAction : MosseloraTheme.QuietAction,
                false, null, false, 3, TextScale.Percent >= 150 ? 3 : TextScale.Percent >= 125 ? 2 : 1, true);
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
            return NeighborSays(villager) != PlayerPrefs.GetString(HeardKey + villager, "");
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

        // One fruit to a neighbour (I231): they remember it truthfully (no meter).
        private void GiveFruit(int villager)
        {
            if (!world.GiveFruit(villager)) return;
            SaveWorld();
            PlayChime();
            if (gentleMotion) Haptics.Tick();
            OpenConversation(villager, true); // their next line is the thank-you
        }
        private int fruitThanks = -1; // the neighbour just given fruit: thanks come first (I231)

        private void OpenConversation(int villagerIndex, bool afterFruit = false)
        {
            fruitThanks = afterFruit ? villagerIndex : -1;
            // The "together" line is said once: talking to anyone else lets it pass (I266).
            if (togetherKeeper >= 0 && villagerIndex != togetherKeeper) togetherKeeper = -1;
            speakingWith = villagerIndex;
            if (world.Mode == PlayMode.Living)
            {
                Fact said = CompanionPick(villagerIndex);
                PlayerPrefs.SetString(HeardKey + villagerIndex, NeighborSays(villagerIndex));
                CompanionHeard(villagerIndex, said); // SC02: after the line is taken, so rebuilds keep it
                PlayerPrefs.Save();
            }
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            if (localPlaytest != null) localPlaytest.Talk(Time.unscaledTime);
#endif
            Rebuild(); // the card floats over the page; the scroll stays put (I127)
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
            // It fades where it stands and keeps its space, so the board never jumps
            // under the player's finger on their very first move (I131).
            if (coachBubble != null && world.CanUndo)
            {
                if (coachBubble.transform.parent == content)
                    spentTipHeight = ((RectTransform)coachBubble.transform).rect.height;
                StartCoroutine(FadeAway(coachBubble));
                coachBubble = null;
            }
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
            // Newly wet tiles fill in order along the water's path from the spring,
            // one short beat apart, so the player watches the water run (I144).
            float[] arrive = WaterArrival(puzzle, angles, insight);
            for (int i = 0; i < puzzle.CellCount; i++)
            {
                bool newlyWet = lastFlowing != null && i < lastFlowing.Length &&
                    insight.Flowing[i] && !lastFlowing[i];
                float delay = arrive != null && newlyWet ? arrive[i] : 0f;
                // Water just arrived here: the tile gives one small, soft swell.
                if (gentleMotion && newlyWet)
                    BloomPulse.Add(tileArtwork[i].gameObject, 0.04f + delay); // the pipe, not the pressed tile
                visibleTiles[i].interactable = !world.Solved;
                // Open ports are geometry, not water: always current.
                if (visibleLeaks != null && visibleLeaks[i] != null)
                    visibleLeaks[i].text = insight.Leaking[i] ? "○" : "";
                // A tile the running water hasn't reached yet stays dry for now;
                // WetLater repaints it from the live insight when it arrives (I144).
                PaintRefreshedTile(puzzle, insight, angles, i, delay > 0f);
                if (delay > 0f) StartCoroutine(WetLater(i, delay));
            }
            PaintDropsWithFlights(puzzle, insight, arrive);
            if (creekWater != null && !world.Solved)
            {
                float share = CreekShare();
                if (!Mathf.Approximately(share, creekShown)) StartCoroutine(FillCreek(creekWater, share));
            }
            lastFlowing = (bool[])insight.Flowing.Clone();
            if (feedbackText != null)
                CjkLineBreakFixer.SetText(feedbackText, GameLocalization.Text("progress",
                    insight.ConnectedCount, puzzle.CellCount, insight.LeakCount));
            if (undoButton != null)
            {
                undoButton.interactable = world.CanUndo;
                ShowUndo(world.CanUndo, true);
            }
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
                    if (gentleMotion) Haptics.Success();
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
            if (gentleMotion) Haptics.Tick(); // felt with the turn (I140)
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
            if (solvedBoard != null && decisionCards != null)
            {
                float boardTop = DepthInContent(solvedBoard, true) - 10f; // painted bed overhang
                float cardsBottom = DepthInContent(decisionCards, false);
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

        // The chosen card lifts and the others step back for a quarter second,
        // so a choice is seen to be taken before the memory page arrives (I142).
        private bool settlingChoice;

        private void SettleChoice(Transform cards, VillageChoice choice)
        {
            if (settlingChoice) return;
            bool animate = gentleMotion && cards != null;
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            if (qaInMemoryOnly) animate = false; // the tour chooses instantly
#endif
            if (!animate) { MakeChoice(choice); return; }
            StartCoroutine(LiftChosen(cards, choice));
        }

        private IEnumerator LiftChosen(Transform cards, VillageChoice choice)
        {
            settlingChoice = true;
            int generation = rebuildGeneration;
            Transform chosen = cards.Find("Choice " + choice);
            var groups = new System.Collections.Generic.List<CanvasGroup>();
            for (int i = 0; i < cards.childCount; i++)
            {
                Transform card = cards.GetChild(i);
                var group = card.GetComponent<CanvasGroup>();
                if (group == null) group = card.gameObject.AddComponent<CanvasGroup>();
                group.interactable = false;
                if (card != chosen) groups.Add(group);
            }
            var squash = chosen != null ? chosen.GetComponent<PressSquash>() : null;
            if (squash != null) Destroy(squash); // the lift owns the scale now
            for (float t = 0f; t < 1f; t += Time.unscaledDeltaTime / 0.24f)
            {
                if (generation != rebuildGeneration) { settlingChoice = false; yield break; }
                float k = Mathf.SmoothStep(0f, 1f, t);
                if (chosen != null) chosen.localScale = Vector3.one * Mathf.Lerp(0.94f, 1.07f, k);
                foreach (CanvasGroup group in groups) if (group != null) group.alpha = Mathf.Lerp(1f, 0.35f, k);
                yield return null;
            }
            settlingChoice = false;
            if (generation == rebuildGeneration) MakeChoice(choice);
        }

        private void MakeChoice(VillageChoice choice)
        {
            if (!world.Choose(choice)) return;
            ClearChoicePreview();
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            if (localPlaytest != null) localPlaytest.Choose(Time.unscaledTime);
#endif
            SaveWorld();
            RecordInAlmanac();
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
                    string text = GardenPostcard.Build(world,
                        world.Completed ? "" : GameLocalization.Text("stream", world.Round + 1, PuzzleCatalog.Count));
                    if (NativeShare.Text(text, GameLocalization.Text("postcard"))) return; // share sheet (I149)
                    GUIUtility.systemCopyBuffer = text;
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
            BuildSaveGardenFileButton(files.transform); // EvergroveApp.Saving.cs (item 96)
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
                    // I219: restoring is what the player came to do, not an erasure: the button
                    // names the restore, in the restore's own colour; the warning above stays.
                    IconButton(card.transform, "Confirm garden restore", GameLocalization.Text("garden_restore"),
                        MosseloraArt.IconRestoreFile, ConfirmGardenRestore, 54, MosseloraTheme.WaterAction, true);
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
            // A new garden meets the next twelve trees, flowers and builds (I172),
            // and remembers which garden it is in its own save (I178).
            gardenNumber = Mathf.Max(gardenNumber, world != null ? world.Garden : 0) + 1;
            world = WorldModel.New(PlayMode.Living, gardenNumber);
            mapInHand = -1;
            bool persist = true;
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            persist = !qaInMemoryOnly;
#endif
            if (persist)
            {
                PlayerPrefs.SetInt(GardenNumberKey, gardenNumber);
                PlayerPrefs.Save();
            }
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

        private void ResearchPause(bool paused)
        {
            if (!paused) RecordReturnDiaryOpen();
        }
#endif

        // Leaving or returning (I275): the one garden note is queued on the way out and
        // cleared on the way back in.
        private bool waveHello = true; // I298: set on launch and on every return

        private sealed class HelloHop : MonoBehaviour
        {
            public float Delay;
            private float clock;
            private Vector2 home;
            private bool started;

            private void Update()
            {
                var rect = (RectTransform)transform;
                if (!started) { home = rect.anchoredPosition; started = true; }
                clock += Time.unscaledDeltaTime;
                float t = clock - Delay;
                if (t < 0f) return;
                const float hop = 0.42f;
                if (t >= hop * 2f) { rect.anchoredPosition = home; rect.localRotation = Quaternion.identity; enabled = false; return; }
                float k = (t % hop) / hop;
                rect.anchoredPosition = home + new Vector2(0f, Mathf.Sin(k * Mathf.PI) * 9f);
                rect.localRotation = Quaternion.Euler(0f, 0f, (t < hop ? 1f : -1f) * Mathf.Sin(k * Mathf.PI) * 4f);
            }
        }

        private void OnApplicationPause(bool paused)
        {
            if (!paused) { waveHello = true; spokenThisVisit.Clear(); } // SC02: a new visit
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            ResearchPause(paused);
#endif
            NotesLifecycle(paused);
        }

        private void OnApplicationQuit() { NotesLifecycle(true); }

        private void NotesLifecycle(bool leaving)
        {
            if (world == null) return;
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            if (qaInMemoryOnly)
            {
                // The tour never schedules a real note; it logs what would be said.
                GardenNote qa = GardenNote.For(world, UnreadLetters());
                Debug.Log("MOSSELORA_GARDEN_NOTE_QA " + (qa == null ? "(nothing waiting)" : qa.From + ": " + qa.Body));
                return;
            }
#endif
            if (leaving) { GardenNotes.Schedule(world, UnreadLetters()); return; }
            noteCame = GardenNotes.Opened(); // shown once home is built (I283)
            if (noteCame != null && titleScreen != null) FollowNote(); // resumed onto home already built
        }

        private bool UnreadLetters() { return PlayerPrefs.GetInt(LettersReadKey, 0) < world.Letters.Length; }

        // I283: the note came while you were away; home shows what it pointed at, once.
        private GardenNote noteCame;

        private void FollowNote()
        {
            GardenNote note = noteCame;
            noteCame = null;
            if (note == null || titleScreen == null || world.Mode != PlayMode.Living) return;
            switch (note.Target)
            {
                case NoteTarget.Letterbox:
                    if (world.Letters.Length > 0) ShowLetter(0);
                    break;
                case NoteTarget.Basket:
                    if (world.Gifts.Length > 0) { basketOpen = true; giftPick = world.Gifts.Length - 1; RefreshMaps(); }
                    break;
                case NoteTarget.Thing:
                    // The thing it named is picked up: its puzzle is one tap away.
                    if (note.Thing >= BoughtIndex && note.Thing - BoughtIndex < world.BoughtCount) { heldBought = note.Thing - BoughtIndex; RefreshMaps(); }
                    else if (note.Thing >= 0 && note.Thing < world.Choices.Length) { mapInHand = note.Thing; RefreshMaps(); }
                    break;
                case NoteTarget.River:
                    if (hubMapView != null) hubMapView.KeepAt(new Vector2(0.5f, 0f)); // the mist and its drops
                    break;
            }
        }

        // ---- I278-I281: light that follows the player's day. Cosmetic only, never saved.
        private const string DayLightKey = "evergrove:daylight";
        private static bool DayLightOn { get { return PlayerPrefs.GetInt(DayLightKey, 1) == 1; } }
        private int qaClockHour = -1; // the tour's fixed clock (I282)
        private int qaDayOffset; // SC02: the tour's scripted week moves the companion's day; 0 outside the tour

        // The companion's local day (SC02): today, or the tour's scripted day.
        private DateTime CompanionToday() { return DateTime.Now.Date.AddDays(qaDayOffset); }

        private void Now(out int hour, out int minute)
        {
            DateTime now = DateTime.Now;
            hour = qaClockHour >= 0 ? qaClockHour : now.Hour;
            minute = qaClockHour >= 0 ? 0 : now.Minute;
        }

        private DayPhase CurrentPhase() { Now(out int hour, out int minute); return DayLight.PhaseAt(hour, minute); }

        // I353: the day's weather for a living garden, with the light on; the tour may set it.
        private int qaWeather = -1;
        private bool qaRainbow; // I364 tour; read by shipping code, so declared outside the dev-only block
        private Weather TodayWeather(WorldModel garden)
        {
            if (!DayLightOn || garden == null || garden.Mode != PlayMode.Living) return Weather.Fair;
            if (qaWeather >= 0) return (Weather)qaWeather;
            return DayWeather.For(DateTime.Now.Date, GardenSeason.For(garden), garden.Garden);
        }

        // I404: a tapped visitor answers in its own voice (the sound switch still rules).
        private void OnVisitorTapped(int kind)
        {
            if (!soundOn || sound == null) return;
            if (kind == 2) sound.PlayOneShot(GardenSounds.Ambient(AmbientVoice.Owl, ambientDice.Next(3)), 0.7f);
            else if (kind == 0 || kind == 3) sound.PlayOneShot(GardenSounds.Ambient(AmbientVoice.Water, ambientDice.Next(3)), 0.8f);
            if (gentleMotion) Haptics.Tick();
        }

        private void ApplyDayLight(WorldModel garden)
        {
            GardenCreature.Tapped = OnVisitorTapped; // I404
            Now(out int quietHour, out int _);
            AudioListener.volume = Companion.Volume(quietHour); // I422: softer late at night
            Now(out int hour, out int minute);
            DayTint tint = DayLight.TintAt(hour, minute);
            bool on = DayLightOn && garden != null && garden.Mode == PlayMode.Living;
            GardenMapView.Tint = on ? new Color(tint.R, tint.G, tint.B, tint.A) : new Color(0f, 0f, 0f, 0f);
            GardenMapView.Dark = on ? tint.Dark : 0f;
            GardenMapView.Fireflies = on && DayLight.Fireflies(DayLight.PhaseAt(hour, minute), GardenSeason.For(garden));
            GardenMapView.FirefliesMove = gentleMotion;
            GardenMapView.LampLit = on && DayLight.LampLit(DayLight.PhaseAt(hour, minute)); // I296
            GardenMapView.FestivalNow = GardenSeason.Festival(garden); // I317
            // I325/I326: visitors come for logical reasons, and are out by season and hour.
            DayPhase visitPhase = DayLightOn ? DayLight.PhaseAt(hour, minute) : DayPhase.Day;
            Season visitSeason = GardenSeason.For(garden);
            Weather visitWeather = TodayWeather(garden); // I405: they keep the weather
            GardenMapView.BeesOut = Visitors.Welcomes(garden, Visitor.Bees) && Visitors.OutIn(Visitor.Bees, visitSeason, visitPhase, visitWeather);
            GardenMapView.BirdsOut = Visitors.Welcomes(garden, Visitor.Birds) && Visitors.Out(Visitor.Birds, visitSeason, visitPhase);
            GardenMapView.CatBy = GardenCat.NapBy(garden, visitPhase); // I335
            GardenMapView.SeasonNow = garden != null && garden.Mode == PlayMode.Living ? visitSeason : Season.Summer; // I338
            GardenMapView.WeatherNow = TodayWeather(garden); // I353
            GardenMapView.SignFont = font; // I360
            GardenMapView.RainbowNow = DayLightOn && garden != null && garden.Mode == PlayMode.Living && qaWeather < 0 &&
                DayWeather.Rainbow(DateTime.Now.Date, GardenSeason.For(garden), garden.Garden) &&
                (visitPhase == DayPhase.Morning || visitPhase == DayPhase.Day || visitPhase == DayPhase.Golden) || qaRainbow; // I364
            bool livingHere = garden != null && garden.Mode == PlayMode.Living;
            Season riverSeason = GardenSeason.For(garden);
            GardenMapView.RiverRainNow = livingHere && RiverMoments.RainRings(riverSeason, visitWeather); // I413
            GardenMapView.RiverGlitterNow = livingHere && DayLightOn && RiverMoments.Glitter(riverSeason, visitPhase, visitWeather); // I415
            GardenMapView.RiverLeavesNow = livingHere && RiverMoments.LeavesAfloat(riverSeason); // I417
            GardenMapView.FishEveryNow = livingHere ? RiverMoments.FishEvery(riverSeason, DayLightOn ? visitPhase : DayPhase.Day) : 0f; // I416
            GardenMapView.WaterTapped = () => { if (soundOn && sound != null) sound.PlayOneShot(GardenSounds.Ambient(AmbientVoice.Water, ambientDice.Next(3)), 0.9f); };
            GardenMapView.MistNow = DayLightOn && garden != null && garden.Mode == PlayMode.Living && DayLight.Mist(visitPhase, visitWeather); // I409
            GardenMapView.SmokeNow = DayLightOn && garden != null && garden.Mode == PlayMode.Living &&
                DayLight.ChimneySmoke(visitPhase, GardenSeason.For(garden), visitWeather); // I410
            GardenMapView.FootprintsNow = garden != null && garden.Mode == PlayMode.Living && SeasonLook.SnowLying(GardenSeason.For(garden)) > 0f; // I412
            GardenMapView.DewNow = DayLightOn && garden != null && garden.Mode == PlayMode.Living
                ? DayLight.Dew(visitPhase, GardenSeason.For(garden)) : Dew.None; // I366
            // I295: the season's drift by day; it follows the season even with the light switch off.
            bool living = garden != null && garden.Mode == PlayMode.Living;
            GardenMapView.DriftNow = living ? GardenSeason.DriftOf(GardenSeason.For(garden),
                DayLightOn ? DayLight.PhaseAt(hour, minute) : DayPhase.Day) : Drift.None;
        }

        // A painted picture in a token keeps its colours (not tinted like a glyph).
        private static void PaintedIcon(Button token, float alpha)
        {
            Transform icon = token != null ? token.transform.Find("Icon") : null;
            if (icon != null) icon.GetComponent<Image>().color = new Color(1f, 1f, 1f, alpha);
        }

        // ---- I276: the soft ask, in the garden's own words, once there's a reason to want it.
        private bool notesAskedThisSession;
        private GameObject notesCard;

        private void MaybeAskNotes()
        {
            if (!GardenNotes.Supported || GardenNotes.On || notesAskedThisSession || titleScreen == null) return;
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            if (qaInMemoryOnly) return; // the tour shoots the ask on purpose (QaNotesAsk)
#endif
            if (world.Mode != PlayMode.Living || letterCard != null || familySheet != null || openCard != null) return;
            int asked = GardenNotes.Asked;
            bool reason = asked == 0 ? (world.Choices.Length >= 3 || PlayerPrefs.GetInt(LettersReadKey, 0) > 0)
                : asked == 1 && world.PuzzlesSolved >= 10;
            if (!reason) return;
            notesAskedThisSession = true;
            ShowNotesAsk();
        }

        private void ShowNotesAsk()
        {
            if (notesCard != null) Destroy(notesCard);
            notesCard = Node("A note from the garden?", titleScreen.transform);
            Fill(notesCard.GetComponent<RectTransform>());
            notesCard.AddComponent<Image>().color = new Color(0.08f, 0.14f, 0.11f, 0.38f);
            var paper = Node("Ask paper", notesCard.transform);
            var paperRect = paper.GetComponent<RectTransform>();
            paperRect.anchorMin = paperRect.anchorMax = new Vector2(0.5f, 0.5f);
            paperRect.sizeDelta = new Vector2(Mathf.Min(PageWidth() + 6f, 420f), 0f);
            var paperImage = paper.AddComponent<Image>();
            MosseloraArt.PaperTile(paperImage, 22);
            paperImage.color = new Color(0.99f, 0.97f, 0.9f, 1f);
            MosseloraArt.Lift(paperImage, 4f, 0.22f);
            var column = paper.AddComponent<VerticalLayoutGroup>();
            column.padding = new RectOffset(22, 22, 18, 18);
            column.spacing = 12;
            column.childControlWidth = column.childControlHeight = true;
            column.childForceExpandWidth = true;
            column.childForceExpandHeight = false;
            paper.AddComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            var envelope = Node("Envelope", paper.transform);
            var envelopeLayout = envelope.AddComponent<LayoutElement>();
            envelopeLayout.preferredHeight = envelopeLayout.minHeight = 64f;
            var envelopeImage = envelope.AddComponent<Image>();
            envelopeImage.sprite = MosseloraArt.Load("letter-sealed");
            envelopeImage.preserveAspect = true;
            envelopeImage.raycastTarget = false;
            Narrate(Label(paper.transform, "The ask",
                "Would you like a short note from the garden once a day, at a time you choose? Only when something's waiting.",
                19, 0, Ink, TextAnchor.UpperCenter), true);
            Transform times = CenteredRowIn(paper.transform, "Note times");
            times.GetComponent<HorizontalLayoutGroup>().spacing = 12;
            foreach (int hour in new[] { 9, 15, 19 })
            {
                int pick = hour;
                // Each hour stands under its sun in ink (b293: white-on-paper hours were unreadable).
                var hourColumn = Node("Hour " + hour, times);
                // Wide enough for "15:00" at 150% text, so each hour stays under its own sun (I287).
                var columnWidth = hourColumn.AddComponent<LayoutElement>();
                columnWidth.minWidth = columnWidth.preferredWidth = Mathf.Max(72f, 58f * TextScale.Percent / 100f);
                var stack = hourColumn.AddComponent<VerticalLayoutGroup>();
                stack.spacing = 4;
                stack.childAlignment = TextAnchor.UpperCenter;
                stack.childControlWidth = stack.childControlHeight = true;
                stack.childForceExpandWidth = stack.childForceExpandHeight = false;
                IconButton(hourColumn.transform, "Notes at " + hour + ":00", "Notes at " + hour + ":00", MosseloraArt.IconSun,
                    () => { GardenNotes.TurnOn(pick); CloseNotesAsk(); PlayChime(); }, 64, MosseloraTheme.ForestAction,
                    false, null, false, 0, 0, true);
                Text when = Label(hourColumn.transform, "Hour", hour + ":00", 17, 0, Ink, TextAnchor.MiddleCenter);
                when.horizontalOverflow = HorizontalWrapMode.Overflow;
                Emphasize(when);
            }
            IconButton(times, "Not now", "Not now", MosseloraArt.IconCancel,
                () => { GardenNotes.NotNow(); CloseNotesAsk(); }, 64, MosseloraTheme.QuietAction,
                false, null, false, 0, 0, true);
            semanticsDirty = true;
        }

        // ---- I299 (idea 36): the cottage summary. Pictures and counts, true ones only.
        private GameObject cottageCard;

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
            var paper = Node("Cottage paper", cottageCard.transform);
            var paperRect = paper.GetComponent<RectTransform>();
            paperRect.anchorMin = paperRect.anchorMax = new Vector2(0.5f, 0.5f);
            paperRect.sizeDelta = new Vector2(Mathf.Min(PageWidth() - 20f, 360f), 0f);
            var paperImage = paper.AddComponent<Image>();
            MosseloraArt.PaperTile(paperImage, 22);
            paperImage.color = new Color(0.99f, 0.97f, 0.9f, 1f);
            MosseloraArt.Lift(paperImage, 4f, 0.22f);
            var column = paper.AddComponent<VerticalLayoutGroup>();
            column.padding = new RectOffset(22, 22, 18, 18);
            column.spacing = 10;
            column.childControlWidth = column.childControlHeight = true;
            column.childForceExpandWidth = true;
            column.childForceExpandHeight = false;
            paper.AddComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;
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
            // I400: what this season can still give, so there is something to aim for.
            if (world.Mode == PlayMode.Living && SeasonalYields.Waiting(world))
            {
                Season season = GardenSeason.For(world);
                CottageLine(paper.transform, MosseloraArt.Load("stone-" + season.ToString().ToLowerInvariant()), SeasonalYields.Why(season));
            }
            if (summary.Letters > 0)
                CottageLine(paper.transform, MosseloraArt.Load("letter-sealed"),
                    summary.Letters == 1 ? "A letter in your box" : summary.Letters + " letters in your box");
            if (summary.Cuttings > 0)
                CottageLine(paper.transform, MosseloraArt.Load(MosseloraArt.Sprig),
                    summary.Cuttings == 1 ? "A cutting in your basket" : summary.Cuttings + " cuttings in your basket");
            if (summary.Waiting > 0)
                CottageLine(paper.transform, MosseloraArt.Load(MosseloraArt.IconContinue),
                    summary.Waiting == 1 ? "One thing holds a new puzzle" : summary.Waiting + " things hold a new puzzle");
            if (summary.DropsNeed > 0) CottageDrops(paper.transform, summary.DropsHave, summary.DropsNeed);
            // I313: the open hope nearest to coming true, with its true count (one line, not three).
            NeighbourWish nearest = null;
            int nearestWho = -1;
            for (int villager = 0; villager < 3; villager++)
            {
                NeighbourWish wish = NeighbourWish.For(world, villager);
                if (wish == null || wish.Granted) continue;
                if (nearest == null || wish.Have * nearest.Need > nearest.Have * wish.Need) { nearest = wish; nearestWho = villager; }
            }
            if (nearest != null)
                CottageLine(paper.transform, MosseloraArt.Portrait(nearestWho), nearest.Hope + ": " + nearest.Have + " of " + nearest.Need);
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
            // I330 (ideas 14 and 16, after block-building sandboxes): decorate anywhere, for free.
            Transform decorRow = CenteredRowIn(paper.transform, "Decorate");
            decorRow.gameObject.SetActive(world.DecorCount < WorldModel.MaxDecor);
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
            // I319: from the cottage, look out over the whole river.
            Transform look = CenteredRowIn(paper.transform, "Look out");
            TreePlay.Token(look, "Look out over the whole river", MosseloraArt.Load(MosseloraArt.IconPath), 56f,
                MosseloraTheme.WaterAction, () => { CloseCottage(); EnterQuiet(true); }, true);
            if (world.Fruit >= WorldModel.JamPerJar && world.Jam < WorldModel.MaxJam)
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
            if (gentleMotion) BloomPulse.Add(paper, 0.05f, 0.04f);
            semanticsDirty = true;
        }

        private void CottageLine(Transform parent, Sprite icon, string words)
        {
            Transform row = CenteredRowIn(parent, "Cottage line");
            var layout = row.GetComponent<HorizontalLayoutGroup>();
            layout.spacing = 12;
            layout.childAlignment = TextAnchor.MiddleLeft;
            var mark = Node("Picture", row);
            var markLayout = mark.AddComponent<LayoutElement>();
            markLayout.preferredWidth = markLayout.preferredHeight = markLayout.minWidth = 44f;
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

        private void SaveWorld()
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            if (qaInMemoryOnly) return; // QA tour: in-memory only, no persistence
#endif
            saveFailed = !WorldStore.Save(world);
            UpdateSaveWarning(); // toggles immediately even without a full UI rebuild
            if (!saveFailed) NoteSaved(); // EvergroveApp.Saving.cs: the quiet "Saved" mark (item 96)
            if (saveFailed)
                Debug.LogWarning("Evergrove is playable but cannot persist this change.");
        }

#if UNITY_EDITOR || DEVELOPMENT_BUILD
        // Dev-only seam for DevQaTour. Replaces the in-memory world and redraws
        // through the normal Rebuild path; refuses any world that is not in-memory.
        internal WorldModel QaWorld { get { return world; } }

#if UNITY_EDITOR || DEVELOPMENT_BUILD
        private bool qaShowHub;

        // The tour normally skips the hub; this opens it once for a screenshot.
        // The tour opens the basket and plants three of the newest thing near it (I193).
        internal void QaPlantFromBasket()
        {
            GardenMap map = LoadGardenMap();
            if (map == null || world.Choices.Length == 0) return;
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            qaShowHub = true; // the tour's hub rebuilds after each planting
#endif
            basketOpen = true;
            basketKind = world.Choices.Length - 1;
            int[] layout = map.Layout(world.Choices, world.Placements, Occupied());
            int near = layout[layout.Length - 1];
            int planted = 0;
            for (int d = 1; d < 6 && planted < 3; d++)
                foreach (int cell in new[] { near - d, near + d, near - d * map.Columns, near + d * map.Columns })
                    if (planted < 3 && map.CanPlace(layout, cell) && !PlantedAt(cell))
                    {
                        OnMapLand(cell);
                        planted++;
                        layout = map.Layout(world.Choices, world.Placements, Occupied());
                    }
            RefreshMaps(); // shows the open tray with the kind picked
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            qaShowHub = false;
#endif
        }

        // I208 tour: open the nursery with the first plant picked, then buy one beside the newest thing.
        internal void QaOpenNursery()
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            qaShowHub = true;
#endif
            basketOpen = true;
            nurseryOpen = true;
            nurseryKind = world.Seeds >= Nursery.Price(0) ? 0 : -1;
            RefreshMaps();
        }

        internal void QaBuyFromNursery()
        {
            GardenMap map = LoadGardenMap();
            if (map == null || nurseryKind < 0) return;
            int[] layout = map.Layout(world.Choices, world.Placements, Occupied());
            int near = layout.Length > 0 ? layout[layout.Length - 1] : map.CellCount / 2;
            for (int d = 1; d < 8; d++)
                foreach (int cell in new[] { near + d, near - d, near + d * map.Columns, near - d * map.Columns })
                    if (cell >= 0 && map.CanPlace(layout, cell) && !PlantedAt(cell)) { OnMapLand(cell); return; }
        }

        internal void QaTalk(int villager)
        {
            if (villager < 0) { speakingWith = -1; Rebuild(); return; }
            OpenConversation(villager);
        }

        // I215 tour: open Family, send 50 seeds with a letter, then open a parcel from "Nan".
        internal FamilySheet QaFamily()
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            qaShowHub = true;
#endif
            if (titleScreen == null && canvasRoot != null) BuildTitleScreen(canvasRoot);
            OpenFamily();
            return familySheet;
        }

        internal void QaCloseVisit() { CloseVisit(); }

        internal void QaCloseFamily()
        {
            if (familySheet != null) Destroy(familySheet.gameObject);
            familySheet = null;
            RefreshMaps(); // as a real close does: the letterbox appears
        }

        internal void QaCloseBasket()
        {
            basketOpen = false; basketKind = -1; heldPlanting = -1;
            nurseryOpen = false; nurseryKind = -1; heldBought = -1;
        }

        // I204 tour: pick up a grown thing, open its puzzle, solve it, see it bloom.
        private TreePlay qaTreePlay;
        internal int QaHoldThing(int family = -1)
        {
            // The classic kinds live in the first four things (I243): pick from those, so the
            // tour keeps playing water, Pairs and Mosaic (the newer kinds have their own shots).
            int[] made = world.Choices;
            int want = family >= 0 ? family : (int)VillageChoice.ShadeTree;
            int item = -1;
            for (int i = 0; i < made.Length && i < ThingPuzzle.ClassicChapters && item < 0; i++)
                if (made[i] == want) item = i;
            if (item < 0) item = family >= 0 ? System.Array.LastIndexOf(made, family) : Mathf.Min(4, made.Length - 1);
            if (item < 0) return -1;
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            qaShowHub = true; // the hub rebuilds when the puzzle closes
#endif
            heldPlanting = -1;
            mapInHand = item;
            RefreshMaps();
            return item;
        }

        // I223 tour: a grown thing whose puzzle is still unsolved (a first solve counts toward the river).
        internal int QaHoldUnsolved()
        {
            int item = -1;
            for (int i = world.Choices.Length - 1; i >= 0 && item < 0; i--)
                if (!world.TreeSolved(i) && world.Choices[i] == (int)VillageChoice.ShadeTree) item = i;
            if (item < 0) return -1;
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            qaShowHub = true;
#endif
            heldPlanting = -1;
            mapInHand = item;
            RefreshMaps();
            return item;
        }

        internal int QaPuzzlesSolved() { return world.PuzzlesSolved; }
        internal int QaFruit() { return world.Fruit; }
        internal void QaGiveFruit(int villager) { GiveFruit(villager); }

        // I227 tour: help with the first unsolved thing of a kind in the garden being visited.
        internal bool QaHelpOnVisit(int family)
        {
            if (visitWorld == null || visitLayer == null) return false;
            int[] made = visitWorld.Choices;
            for (int i = 0; i < made.Length; i++)
                if (made[i] == family && !visitWorld.TreeSolved(i) && !visitWorld.IsHelped(i))
                {
                    HelpWith(i);
                    qaTreePlay = visitLayer.GetComponentInChildren<TreePlay>();
                    return qaTreePlay != null;
                }
            return false;
        }

        // The first grown thing of a kind in your garden still unsolved, or -1.
        internal int QaUnsolved(int family)
        {
            int[] made = world.Choices;
            for (int i = 0; i < made.Length; i++)
                if (made[i] == family && !world.TreeSolved(i)) return i;
            return -1;
        }

        // I224 tour: home scrolled to the river's foot, where the drops sit on the mist.
        internal void QaHubToFoot()
        {
            qaShowHub = true;
            if (titleScreen == null && canvasRoot != null) BuildTitleScreen(canvasRoot);
            if (hubMapView != null) hubMapView.KeepAt(new Vector2(0.5f, 0f));
        }

        // I243 tour: open one of the newer kinds as a grown thing would hold it.
        internal void QaOpenKind(PuzzleKind kind, int tier)
        {
            qaShowHub = true;
            PlayerPrefs.DeleteKey("evergrove:rule-seen-" + kind.ToString().ToLowerInvariant()); // I254: shown each tour
            if (titleScreen == null && canvasRoot != null) BuildTitleScreen(canvasRoot);
            int item = Mathf.Min(5, world.Choices.Length - 1);
            if (titleScreen == null || item < 0) return;
            int[] made = world.Choices;
            PuzzleCard((RectTransform)titleScreen.transform, world.Garden * 1000 + 77 + (int)kind, tier, kind,
                ItemArt(item, made[item]), ItemAt(item, made[item]).Name, 0,
                count => PairsPictures(count, item, (c, f) => ItemArt(c, f)), () => PlayTock(), () => 0,
                () => { qaShowHub = false; });
            qaTreePlay = titleScreen.GetComponentInChildren<TreePlay>();
        }

        internal void QaOpenTreePlay(int item)
        {
            OpenTreePlay(item);
            qaTreePlay = titleScreen != null ? titleScreen.GetComponentInChildren<TreePlay>() : null;
        }

        internal void QaSolveTreePlay() { if (qaTreePlay != null) qaTreePlay.QaSolve(); }
        internal void QaHintTreePlay() { if (qaTreePlay != null) qaTreePlay.QaHint(); }
        internal void QaNotesAsk() { if (titleScreen != null) ShowNotesAsk(); }
        internal void QaSetClock(int hour) { qaClockHour = hour; }
        internal void QaSetDay(int offset) { qaDayOffset = offset; spokenThisVisit.Clear(); } // SC02 tour: a new day, a new visit
        internal string QaNeighborSays(int villager) { return NeighborSays(villager); }
        internal void QaSetWeather(int weather) { qaWeather = weather; } // I353 tour: -1 is the real day's
        internal void QaRainbow(bool on) { qaRainbow = on; } // I364 tour
        internal void QaCloseNotesAsk() { CloseNotesAsk(); }
        internal void QaTogetherTreePlay() { if (qaTreePlay != null) qaTreePlay.QaTogether(); }
        internal void QaFourTurnsTreePlay() { if (qaTreePlay != null) qaTreePlay.QaFourTurns(); }

        internal void QaEndTreePlay()
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            qaShowHub = false;
#endif
            qaTreePlay = null;
        }

        // I292/I294 tour: crown a thing as if replayed to the deepest tier; enter or leave quiet.
        internal void QaCrown(int item)
        {
            if (!qaInMemoryOnly || item < 0) return;
            PlayerPrefs.SetInt(ReplayKey(item), ReplayBoards.ReplaysPerTier * 3);
            justSolved = item; // home frames it
            RefreshMapsFresh();
        }

        // I328/I330 tour: a jar of jam from three fruit, and a bench and stepping stones set down
        // on free land near the newest thing.
        internal void QaDecorAndJam()
        {
            if (!qaInMemoryOnly) return;
            world.MakeJam();
            GardenMap map = LoadGardenMap();
            if (map == null) return;
            int[] cells = map.Layout(world.Choices, world.Placements, world.Occupied);
            int near = cells.Length > 0 ? cells[cells.Length - 1] : 30;
            int placed = 0;
            for (int step = 1; step < map.CellCount && placed < 2; step++)
            {
                int cell = (near + step) % map.CellCount;
                if (world.PlaceDecor(placed == 0 ? Decoration.Bench : Decoration.Stones, cell, map)) placed++;
            }
            RefreshMaps();
        }

        // I333 tour: open the old well's board.
        internal void QaOpenWell()
        {
            if (!qaInMemoryOnly) return;
            OpenWell();
            qaTreePlay = titleScreen != null ? titleScreen.GetComponentInChildren<TreePlay>() : null;
        }

        // I310 tour: a real lantern parcel from a relative, opened here.
        internal void QaLantern()
        {
            if (!qaInMemoryOnly) return;
            if (FamilyParcel.TryCreate("Nan", "", ParcelGift.Lantern, 0, "qa-lantern-" + world.Lanterns.Length,
                    out string text, out ParcelContents _, out ParcelError _))
                world.Open(text);
        }

        internal void QaQuiet(bool on, bool wholeRiver = false)
        {
            if (on) EnterQuiet(wholeRiver); else ExitQuiet();
        }

        internal void QaShowHub(bool show)
        {
            if (!show) { CloseTitle(); return; }
            qaShowHub = true;
            if (titleScreen == null && canvasRoot != null) BuildTitleScreen(canvasRoot);
            qaShowHub = false;
        }
#endif
        // I383 tour: arrange a finished garden so every good neighbour stands somewhere: a
        // cluster on a bank (flower by the water, a tree beside it, two more trees, a second
        // flower, something built), found by trying each land cell on the real map.
        internal bool QaArrangeGoodNeighbours()
        {
            GardenMap map = LoadGardenMap();
            int[] made = world.Choices;
            if (map == null || made.Length < 12) return false;
            int f1 = 11, f2 = 8, t1 = 9, t2 = 6, t3 = 3, b1 = 10; // chapter % 3: tree, built, flower
            int[] arranged = { f1, t1, t2, t3, f2, b1 };
            int all = (1 << Enum.GetValues(typeof(Pairing)).Length) - 1;
            int w = map.Columns;
            int[][] shapes =
            {
                // Upward first: home opens with the newest thing (f1) low in view, so the rest shows above it.
                new[] { 0, 1, 1 - w, -w, -1, 2 }, new[] { 0, -1, -1 - w, -w, 1, -2 },
                new[] { 0, 1, 1 - w, -w, -1 - w, 2 - w }, new[] { 0, -1, -1 - w, -w, 1 - w, -2 - w },
                new[] { 0, 1, 1 + w, w, -1, 2 }, new[] { 0, -1, -1 + w, w, 1, -2 },
            };
            for (int c0 = 0; c0 < map.CellCount; c0++)
                foreach (int[] shape in shapes)
                {
                    var cells = new int[shape.Length];
                    bool ok = true;
                    for (int k = 0; k < shape.Length && ok; k++)
                    {
                        cells[k] = c0 + shape[k];
                        int dx = Mathf.Abs(cells[k] % w - c0 % w);
                        ok = cells[k] >= 0 && cells[k] < map.CellCount && dx <= 2 && map.IsLand(cells[k]);
                    }
                    if (!ok) continue;
                    var trial = WorldModel.TryRestore(world.Export(), out WorldModel copy) ? copy : null;
                    if (trial == null) return false;
                    for (int k = 0; k < arranged.Length; k++) trial.PlaceAt(arranged[k], cells[k]);
                    var things = Pairings.Things(trial, map);
                    var seen = new System.Collections.Generic.HashSet<int>();
                    foreach (PlacedThing t in things) if (t.Cell >= 0 && !seen.Add(t.Cell)) ok = false;
                    if (!ok || Pairings.Between(map, things) != all) continue;
                    for (int k = 0; k < arranged.Length; k++) world.PlaceAt(arranged[k], cells[k]);
                    Rebuild(true);
                    return true;
                }
            return false;
        }

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

        // I316: now and then, over home, a voice of the season and the hour.
        private float nextAmbient;
        private bool owlInGrove; // I376
        private readonly System.Random ambientDice = new System.Random();

        private void TickSoundscape()
        {
            if (!soundOn || sound == null || titleScreen == null || world == null || world.Mode != PlayMode.Living) return;
            if (familySheet != null || openCard != null) return; // quiet while a card is open
            float now = Time.unscaledTime;
            if (now < nextAmbient) return;
            Now(out int hour, out int minute);
            DayPhase phase = DayLightOn ? DayLight.PhaseAt(hour, minute) : DayPhase.Day;
            AmbientVoice[] voices = Soundscape.For(GardenSeason.For(world), phase);
            AmbientVoice voice = voices[ambientDice.Next(voices.Length)];
            AmbientVoice? weatherVoice = Soundscape.WeatherVoice(TodayWeather(world), phase); // I354–I356
            if (weatherVoice.HasValue && ambientDice.NextDouble() < 0.6) voice = weatherVoice.Value;
            // I376: an owl a grove has brought calls now and then in the night.
            AmbientVoice? owl = owlInGrove && Visitors.Welcomes(world, Visitor.Owl)
                ? Soundscape.VisitorVoice(Visitor.Owl, GardenSeason.For(world), phase) : null;
            if (owl.HasValue && ambientDice.NextDouble() < 0.3) voice = owl.Value;
            if (nextAmbient > 0f) sound.PlayOneShot(GardenSounds.Ambient(voice, ambientDice.Next(3)), 0.55f);
            nextAmbient = now + Soundscape.SecondsBetween(voice) * (0.6f + 0.8f * (float)ambientDice.NextDouble());
        }

        // I305/I306: a move on a thing's puzzle sounds in its kind's own voice, at the next
        // note of the kind's phrase.
        private int moveNote;

        private void PlayMove(PuzzleKind kind)
        {
            if (!soundOn) return;
            sound.PlayOneShot(GardenSounds.Move(kind, moveNote++), 0.9f);
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

namespace Evergrove.UnityClient
{
    // A plain tap target that is not a Button, so screen readers don't list it (I127).
    internal sealed class TapCatcher : UnityEngine.MonoBehaviour, UnityEngine.EventSystems.IPointerClickHandler
    {
        public System.Action Tapped;
        public void OnPointerClick(UnityEngine.EventSystems.PointerEventData eventData) => Tapped?.Invoke();
    }
}

namespace Evergrove.UnityClient
{
    // Turns a mark a quarter at a time, with a pause between: the board's own verb (I138).
    internal sealed class QuarterTurns : UnityEngine.MonoBehaviour
    {
        private float clock;
        private float from;

        private void Update()
        {
            clock += UnityEngine.Time.unscaledDeltaTime;
            const float pause = 1.1f, swing = 0.35f;
            if (clock < pause) return;
            float t = UnityEngine.Mathf.Clamp01((clock - pause) / swing);
            float angle = from - 90f * UnityEngine.Mathf.SmoothStep(0f, 1f, t);
            transform.localRotation = UnityEngine.Quaternion.Euler(0f, 0f, angle);
            if (t >= 1f) { clock = 0f; from = angle % 360f; }
        }
    }
}

namespace Evergrove.UnityClient
{
    // Presses a little (down and smaller) just before each quarter turn of the
    // arrow beside it, on the same 1.45s beat as QuarterTurns (I147).
    internal sealed class GhostPress : UnityEngine.MonoBehaviour
    {
        private float clock;
        private UnityEngine.Vector2 rest;
        private UnityEngine.RectTransform rect;

        private void Start()
        {
            rect = (UnityEngine.RectTransform)transform;
            rest = rect.anchoredPosition;
        }

        private void Update()
        {
            clock = (clock + UnityEngine.Time.unscaledDeltaTime) % 1.45f;
            // 0.85–1.25s: down, then up as the arrow swings.
            float press = clock < 0.85f || clock > 1.25f ? 0f
                : UnityEngine.Mathf.Sin((clock - 0.85f) / 0.4f * UnityEngine.Mathf.PI);
            rect.anchoredPosition = rest + new UnityEngine.Vector2(0f, -5f * press);
            transform.localScale = UnityEngine.Vector3.one * (1f - 0.12f * press);
        }
    }
}
