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
        // Water's Undo | step count | Redo row (BoardKit.UndoRow's look): the row, its redo token and count.
        private GameObject undoRowNode;
        private Button redoButton;
        private Text undoStepCount;
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
            SheetMotion.Settled = qaInMemoryOnly; // QA tour screenshots capture settled sheets
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
            LoadHand(); // checklist 5: the hand, kept on the device
            LongPressCaption.Shown += OnLongPressShown; // checklist 5: the question follows the first long-press
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
            LongPressCaption.DefaultFont = baseFont; // static helpers (TreePlay.Token, BoardKit.Cell) explain with it
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
            LongPressCaption.Shown -= OnLongPressShown;
#if UNITY_IOS || UNITY_ANDROID
            UnityEngine.Accessibility.AssistiveSupport.screenReaderStatusChanged -= OnScreenReaderStatusChanged;
#endif
        }

        private void Update()
        {
            HandleBackGesture();
            TickSoundscape();
            TickIdleFade(); // EvergroveApp.Idle.cs: the corner chrome fades after 6 s with no touch
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
    }
}
