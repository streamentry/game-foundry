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
            var nodes = ScreenReaderBridge.Collect(content, DescribeButton);
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
            // An unreadable saved garden is not a write failure: say so, so players
            // do not hunt for a device-storage problem that does not exist.
            saveWarningCaption.text = GameLocalization.Text(
                WorldStore.RecoveryBlocked ? "save_unreadable" : "save_warning");
            saveWarningCaption.text = IndicVisualOrder.Visual(GameLocalization.Current,
                saveWarningCaption.text);
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
            MosseloraArt.Round(image, tone, 16); // Primary actions are rare; choices are equal.
            MosseloraArt.Lift(image, 2f, quiet ? 0.10f : 0.22f);
            if (quiet) MosseloraArt.Edge(node.transform, MosseloraTheme.CardEdge, 16);
            var button = node.AddComponent<Button>();
            button.targetGraphic = image;
            button.colors = PressTint();
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
            string shownCaption = IndicVisualOrder.Visual(GameLocalization.Current,
                GameLocalization.Display(caption));
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
            palette.highlightedColor = new Color(0.95f, 0.95f, 0.93f);
            palette.selectedColor = Color.white;
            palette.pressedColor = new Color(0.84f, 0.84f, 0.80f);
            palette.disabledColor = new Color(0.86f, 0.86f, 0.84f, 0.55f);
            palette.fadeDuration = 0.08f;
            return palette;
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
            float markHeight = compact ? 52f : 64f;
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
            if (content != null && content.rect.width > 1f)
                return Mathf.Max(225f, content.rect.width - 34f);
            Canvas canvas = viewportScaler == null ? null : viewportScaler.GetComponent<Canvas>();
            float scale = canvas == null ? 1f : Mathf.Max(0.1f, canvas.scaleFactor);
            return Mathf.Max(225f, Screen.safeArea.width / scale - 34f);
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
            semanticsDirty = true;
            UpdateSaveWarning();
            float previousPosition = scroll.verticalNormalizedPosition;
            visibleTiles = null;
            activeIllustration = null;
            activePainting = null;
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
            GardenHeader(puzzleScreen);
            Text subtitle = Label(content, "Title", GameLocalization.Text("subtitle"),
                puzzleScreen ? 17 : 24, puzzleScreen ? 28 : 44, Ink, TextAnchor.MiddleCenter);
            if (!puzzleScreen) Emphasize(subtitle);
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
            }
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            else if (diorama != null)
                diorama.Hide();
#endif

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
                var moment = Card(content, "A remembered moment", MosseloraTheme.CardFill);
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
                Button(content, "Continue", GameLocalization.Text("continue"), () => {
                    showingMemory = false;
                    revealNewScreen = true; // the ending, if reached, opens at its heading
                    Rebuild(true);
                }, 60, MosseloraTheme.WaterAction);
                Button(content, "Change your choice", GameLocalization.Text("change"), () => {
                    if (!world.ReconsiderLatestChoice()) return;
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                    if (localPlaytest != null) localPlaytest.Reconsider(Time.unscaledTime);
#endif
                    SaveWorld();
                    showingMemory = false;
                    Rebuild(false);
                }, 47);
                SettingsRow(multiLanguage);
            }
            else if (world.Completed)
            {
                Painting("Village memory painting", MosseloraArt.HeroVillage, 460f);
                var conclusion = Card(content, "The ending", MosseloraTheme.CardFill);
                screenHeading = conclusion.GetComponent<RectTransform>();
                Text endingTitle = Label(conclusion.transform, "Ending title",
                    GameLocalization.Text("ending_title"), 15, 31, Leaf, TextAnchor.MiddleCenter);
                Emphasize(endingTitle);
                Text finalWords = Label(conclusion.transform, "Final words", LocalizedVillage.Ending(world),
                    19, 0, Ink, TextAnchor.MiddleCenter);
                Emphasize(finalWords);
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
                Text neighborsTitle = Label(content, "Neighbors", GameLocalization.Text("neighbors"),
                    14, 30, Leaf, TextAnchor.MiddleCenter);
                Emphasize(neighborsTitle);
                var neighbors = Row(content, "Speak with a neighbor");
                Button(neighbors.transform, "Mira", LocalizedVillage.Name(0), () => OpenConversation(0), 52);
                Button(neighbors.transform, "Orin", LocalizedVillage.Name(1), () => OpenConversation(1), 52);
                Button(neighbors.transform, "Fen", LocalizedVillage.Name(2), () => OpenConversation(2), 52);
                if (speakingWith >= 0)
                {
                    var conversation = Card(content, "A neighbor's memory", Sand);
                    Label(conversation.transform, "Neighbor name",
                        LocalizedVillage.Name(speakingWith), 16, 31, Leaf);
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
                            PlayerPrefs.SetInt(ReducedMotionKey, animateMiniature ? 1 : 0);
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
#endif
            Sprig();
            Text promise = Label(content, "Respectful play", GameLocalization.Text("offline"),
                13, 36, Ink, TextAnchor.MiddleCenter);
            Emphasize(promise);
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
            var journal = Card(content, "A living journal", MosseloraTheme.CardFill);
            screenHeading = journal.GetComponent<RectTransform>();
            Text journalTitle = Label(journal.transform, "Journal title",
                GameLocalization.Text("journal_title"), 15, 30, Leaf, TextAnchor.MiddleCenter);
            Emphasize(journalTitle);
            string[] entries = LocalizedVillage.Journal(world);
            int[] chosen = world.Choices;
            for (int i = 0; i < entries.Length; i++)
            {
                // Each remembered chapter carries the painted icon of its choice.
                var line = Node("Journal chapter", journal.transform);
                var lineGroup = line.AddComponent<HorizontalLayoutGroup>();
                lineGroup.reverseArrangement = GameLocalization.IsRtl(GameLocalization.Current);
                lineGroup.spacing = 10;
                lineGroup.childAlignment = TextAnchor.MiddleLeft;
                lineGroup.childControlWidth = true;
                lineGroup.childControlHeight = true;
                lineGroup.childForceExpandWidth = false;
                lineGroup.childForceExpandHeight = false;
                Sprite icon = i < chosen.Length ? MosseloraArt.ChoiceIcon((VillageChoice)chosen[i]) : null;
                if (icon != null)
                {
                    var picture = Node("Journal picture", line.transform);
                    var pictureLayout = picture.AddComponent<LayoutElement>();
                    pictureLayout.preferredWidth = 58;
                    pictureLayout.minWidth = 58;
                    pictureLayout.preferredHeight = 52;
                    var pictureImage = picture.AddComponent<Image>();
                    pictureImage.sprite = icon;
                    pictureImage.preserveAspect = true;
                    pictureImage.raycastTarget = false;
                }
                Text words = Label(line.transform, "Recorded cause and effect", entries[i], 14, 0, Ink);
                words.gameObject.GetComponent<LayoutElement>().flexibleWidth = 1;
            }
            if (world.Journal.Length == 0)
                Label(journal.transform, "Blank page", GameLocalization.Text("journal_empty"), 14, 0, Moss);
        }

        // Upper bound for Large tiles so a tablet-width board stays reachable.
        private const int LargeTileCap = 120;

        private void BuildPuzzle()
        {
            Puzzle puzzle = world.CurrentPuzzle;
            PipeInsight insight = world.Insight;
            Pill("Chapter",
                GameLocalization.Text("stream", world.Round + 1, PuzzleCatalog.Count) +
                (GameLocalization.Current == "en" ? "  ·  " + puzzle.Name : ""),
                MosseloraTheme.SagePanel);
            Label(content, "Prompt", world.Round == 0 ?
                GameLocalization.Text("first") : LocalizedVillage.Prompt(world), 16, 0, Ink,
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
            float scale = viewportScaler == null ? 1f :
                Mathf.Max(0.1f, viewportScaler.scaleFactor);
            float layoutWidth = Screen.safeArea.width / scale;
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
            for (int index = 0; index < puzzle.CellCount; index++)
            {
                int tappedIndex = index;
                var tileNode = Node("Water tile", gridNode.transform);
                var image = tileNode.AddComponent<Image>();
                bool reached = insight.Flowing[index];
                MosseloraArt.Round(image, Color.white, 12);
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
                OpenEndRing.Attach(leakText, MosseloraTheme.EarthInk, 13f * markerScale);
                visibleLeaks[index] = leakText;
                if (world.HintStep == 2 && index == world.HintTile)
                {
                    var clueNode = Node("Hint focus marker", tileNode.transform);
                    RectTransform clueRect = clueNode.GetComponent<RectTransform>();
                    clueRect.anchorMin = new Vector2(1f, 0f);
                    clueRect.anchorMax = new Vector2(1f, 0f);
                    clueRect.pivot = new Vector2(1f, 0f);
                    clueRect.sizeDelta = new Vector2(23f, 23f) * markerScale;
                    clueRect.anchoredPosition = new Vector2(-3f, 3f) * markerScale;
                    var clueLabel = clueNode.AddComponent<Text>();
                    clueLabel.font = font;
                    clueLabel.fontSize = Mathf.RoundToInt(17 * markerScale);
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

            string feedback = world.Solved
                ? GameLocalization.Text("solved")
                : GameLocalization.Text("progress", insight.ConnectedCount, puzzle.CellCount, insight.LeakCount);
            feedbackText = Label(content, "Water feedback", feedback, 17, 34, Ink, TextAnchor.MiddleCenter);
            Emphasize(feedbackText);

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
                    Text chooseTitle = Label(content, "Choose title", GameLocalization.Text("choose"),
                        24, 56, Ink, TextAnchor.MiddleCenter);
                    Emphasize(chooseTitle);
                    decisionHeading = chooseTitle.rectTransform;
                    for (int choiceIndex = 0; choiceIndex < 3; choiceIndex++)
                    {
                        VillageChoice choice = (VillageChoice)choiceIndex;
                        // None is visually preselected. A golden default would
                        // bias research toward the first habitat, not player care.
                        ChoiceCard(choice, () => MakeChoice(choice));
                    }
                }
            }
            else
            {
                var tools = Row(content, "Gentle controls");
                undoButton = Button(tools.transform, "Undo", GameLocalization.Text("undo"), () => Act(() => world.Undo(), "undo"),
                    56, MosseloraTheme.ForestAction);
                undoButton.interactable = world.CanUndo;
                Button(tools.transform, "Hint",
                    GameLocalization.Text(world.HintStep == 2 ? "hint_turn" :
                        world.HintStep == 1 ? "hint_more" : "hint"),
                    UseGentleHint, 56, MosseloraTheme.WaterAction);
                Button(tools.transform, "Restart", GameLocalization.Text("restart"),
                    () => Act(() => world.Restart(), "restart"), 56, MosseloraTheme.EarthAction);
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
                14, 0, Moss, TextAnchor.MiddleCenter);
        }

        // Illustrated choice card (painted icon + title + written consequence).
        // One Button per card, so taps, focus and screen-reader text are unchanged.
        private void ChoiceCard(VillageChoice choice, Action action)
        {
            var node = Node("Choice " + choice, content);
            MinHeight(node, TextScale.Scale(92));
            var image = node.AddComponent<Image>();
            MosseloraArt.Round(image, MosseloraTheme.CardFill, 18);
            MosseloraArt.Lift(image, 2f, 0.12f);
            MosseloraArt.Edge(node.transform, MosseloraTheme.CardEdge, 18);
            var button = node.AddComponent<Button>();
            button.targetGraphic = image;
            button.colors = PressTint();
            button.onClick.AddListener(() => action());
            var group = node.AddComponent<HorizontalLayoutGroup>();
            group.reverseArrangement = GameLocalization.IsRtl(GameLocalization.Current);
            group.padding = new RectOffset(12, 14, 10, 10);
            group.spacing = 12;
            group.childAlignment = TextAnchor.MiddleLeft;
            group.childControlWidth = true;
            group.childControlHeight = true;
            group.childForceExpandWidth = false;
            group.childForceExpandHeight = false;
            Sprite icon = MosseloraArt.ChoiceIcon(choice);
            if (icon != null)
            {
                var picture = Node("Choice picture", node.transform);
                var pictureLayout = picture.AddComponent<LayoutElement>();
                pictureLayout.preferredWidth = 84;
                pictureLayout.minWidth = 84;
                pictureLayout.preferredHeight = 76;
                pictureLayout.flexibleWidth = 0;
                var pictureImage = picture.AddComponent<Image>();
                pictureImage.sprite = icon;
                pictureImage.preserveAspect = true;
                pictureImage.raycastTarget = false;
            }
            var words = Node("Choice words", node.transform);
            words.AddComponent<LayoutElement>().flexibleWidth = 1;
            var stack = words.AddComponent<VerticalLayoutGroup>();
            stack.spacing = 3;
            stack.childAlignment = TextAnchor.MiddleLeft;
            stack.childControlWidth = true;
            stack.childControlHeight = true;
            stack.childForceExpandWidth = true;
            stack.childForceExpandHeight = false;
            Text title = Label(words.transform, "Choice name", LocalizedVillage.Choice(choice), 18, 0, Ink);
            Emphasize(title);
            Label(words.transform, "Choice consequence", LocalizedVillage.ChoiceOutcome(choice, world), 15, 0, Moss);
        }

        // Compact settings row below the primary content of puzzle and post-choice
        // screens. Same controls and behavior as the top-of-screen settings.
        private void SettingsRow(bool offerLanguage)
        {
            var panel = Card(content, "Settings panel", MosseloraTheme.SagePanel);
            panel.GetComponent<VerticalLayoutGroup>().padding = new RectOffset(10, 10, 10, 10);
            GameObject row = Row(panel.transform, "Settings row");
            if (offerLanguage)
                Button(row.transform, "Language settings",
                    GameLocalization.Text("language") + ": " +
                    GameLocalization.NativeName(GameLocalization.Current),
                    () => { languagePicker = true; Rebuild(true); }, 48);
            TextSizeButton(row.transform);
            BoardSizeButton(row.transform);
        }

        // Language-neutral label: the tile scale of the current board (EG-112).
        // Standard is 1x by definition. Large measures about 1.2x for a 3x3
        // board on a 390-point phone (109 px vs 90 px tile cap); 4x4 is closer to 1.03x.
        private void BoardSizeButton(Transform parent)
        {
            Button(parent, "Board size", BoardSize.IsLarge ? "▦ 1.2×" : "▦ 1×", () => {
                BoardSize.Toggle();
                Rebuild();
            }, 52);
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
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            if (localPlaytest != null) localPlaytest.Talk(Time.unscaledTime);
#endif
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
            scroll.StopMovement();
            scroll.verticalNormalizedPosition = 1f -
                Mathf.Clamp01((targetTop - context) / distance);
        }

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
            Rebuild(true);
        }

        // Player-owned copy of the garden. Clipboard only; nothing leaves the device.
        // Lives in settings, below the puzzle, and never on the primary path.
        private void BuildGardenFileSection()
        {
            var card = Card(content, "Garden file", MosseloraTheme.CardFill);
            Button(card.transform, "Copy garden file", GameLocalization.Text("garden_copy"),
                CopyGardenFile, 54, MosseloraTheme.ForestAction);
            if (!WorldStore.RecoveryBlocked)
            {
                if (!gardenRestoreConfirm)
                    Button(card.transform, "Restore garden file", GameLocalization.Text("garden_restore"),
                        ReadGardenFile, 54, MosseloraTheme.WaterAction);
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
