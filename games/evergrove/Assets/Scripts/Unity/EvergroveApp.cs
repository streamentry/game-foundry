using System;
using System.Collections;
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
        private static readonly Color Paper = Hex("#F7F4E9");
        private static readonly Color Ink = Hex("#23443C");
        private static readonly Color Moss = Hex("#52685F");
        private static readonly Color Leaf = Hex("#276D54");
        private static readonly Color Pale = Hex("#DFEBDD");
        private static readonly Color Sand = Hex("#F2E7D4");
        private static readonly Color White = Color.white;
        private static readonly Color River = Hex("#DCEDEF");

        private WorldModel world;
        private Font font;
        private RectTransform content;
        private RectTransform scrollArea;
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
        private AudioSource sound;
        private AudioClip chime;
        private LocalPlaytest localPlaytest; // opt-in, in-memory only
        private bool playtestCopied;
        // Keep live puzzle controls mounted during every tap. Recreating the
        // entire uGUI hierarchy on each rotation would cause GC, jank and
        // scroll-position jumps on older phones.
        private Button[] visibleTiles;
        private Image[] tileBackgrounds;
        private PipeGraphic[] tileArtwork;
        private Text feedbackText;
        private Button undoButton;

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
            world = WorldStore.Load();
            soundOn = PlayerPrefs.GetInt("evergrove:gentle-audio", 0) == 1;
            // Default to the proven-in-code 2D art; keep the experimental
            // orthographic 3D visual opt-in until real player/device evidence.
            showMiniature = PlayerPrefs.GetInt("evergrove:visual-miniature", 0) == 1;
            animateMiniature = PlayerPrefs.GetInt("evergrove:miniature-motion", 0) == 1;
            font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (font == null) font = Font.CreateDynamicFontFromOSFont("Arial", 16);
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
            Fill(viewport.GetComponent<RectTransform>());
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
            column.padding = new RectOffset(17, 17, 15, 30);
            column.spacing = 9;
            column.childControlWidth = true;
            column.childControlHeight = true;
            column.childForceExpandWidth = true;
            column.childForceExpandHeight = false;
            body.AddComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            scroll.content = content;

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

        private Text Label(Transform parent, string name, string value, int size, int minimum,
                           Color color, TextAnchor alignment = TextAnchor.MiddleLeft)
        {
            var node = Node(name, parent);
            // Minimum space for touch/visual rhythm; dynamic text's own
            // preferred height may grow for larger font scales or locales.
            var layout = node.AddComponent<LayoutElement>();
            layout.minHeight = minimum;
            var label = node.AddComponent<Text>();
            label.font = font;
            label.fontSize = size;
            label.color = color;
            label.alignment = alignment;
            label.horizontalOverflow = HorizontalWrapMode.Wrap;
            label.verticalOverflow = VerticalWrapMode.Overflow;
            label.supportRichText = false;
            label.raycastTarget = false;
            label.text = value;
            return label;
        }

        private GameObject Card(Transform parent, string name, Color fill)
        {
            var node = Node(name, parent);
            var image = node.AddComponent<Image>();
            image.color = fill;
            var stack = node.AddComponent<VerticalLayoutGroup>();
            stack.childAlignment = TextAnchor.UpperCenter;
            stack.padding = new RectOffset(10, 10, 10, 10);
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
            Height(node, height);
            var image = node.AddComponent<Image>();
            image.color = primary ? Leaf : Pale;
            var button = node.AddComponent<Button>();
            button.targetGraphic = image;
            var palette = button.colors;
            palette.highlightedColor = primary ? Hex("#337C63") : Hex("#C6DFC9");
            palette.pressedColor = primary ? Hex("#1D4C3A") : Hex("#B2CCBB");
            palette.disabledColor = Hex("#CFD2CB");
            button.colors = palette;
            var textNode = Node("Caption", node.transform);
            Fill(textNode.GetComponent<RectTransform>());
            var text = textNode.AddComponent<Text>();
            text.font = font;
            text.fontSize = 15;
            text.color = primary ? White : Ink;
            text.alignment = TextAnchor.MiddleCenter;
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Overflow;
            text.raycastTarget = false;
            text.text = caption;
            button.onClick.AddListener(() => action());
            return button;
        }

        private GameObject Row(Transform parent, string name)
        {
            var row = Node(name, parent);
            var group = row.AddComponent<HorizontalLayoutGroup>();
            group.spacing = 8;
            group.childAlignment = TextAnchor.MiddleCenter;
            group.childControlWidth = true;
            group.childControlHeight = true;
            group.childForceExpandWidth = true;
            group.childForceExpandHeight = false;
            Height(row, 54);
            return row;
        }

        private void Rebuild(bool top = false)
        {
            if (content == null) return;
            float previousPosition = scroll.verticalNormalizedPosition;
            visibleTiles = null;
            tileBackgrounds = null;
            tileArtwork = null;
            feedbackText = null;
            undoButton = null;
            for (int i = content.childCount - 1; i >= 0; i--)
            {
                GameObject old = content.GetChild(i).gameObject;
                old.SetActive(false); // Destroy() is deferred until the frame ends.
                Destroy(old);
            }

            bool living = world.Mode == PlayMode.Living;
            Label(content, "Brand", "EVERGROVE       /       A WORLD THAT REMEMBERS", 12, 24, Leaf);
            Label(content, "Title", "Little choices. A living place.", 25, 46, Ink);

            if (living)
            {
                var illustration = Node(showMiniature ? "Miniature diorama" : "Living storybook art", content);
                Height(illustration, 193);
                bool renderedMiniature = false;
                if (showMiniature)
                {
                    if (diorama == null)
                    {
                        var stage = new GameObject("Optional orthographic world");
                        stage.transform.SetParent(transform, false);
                        diorama = stage.AddComponent<VillageDiorama>();
                    }
                    renderedMiniature = diorama.Show(VillageScenePlan.From(world), animateMiniature);
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
                    art.Show(world.Round, world.Count(VillageChoice.ShadeTree),
                        world.Count(VillageChoice.Bridge), world.Count(VillageChoice.Flowers),
                        world.Round > 0 || world.Solved);
                }
            }
            else if (diorama != null)
                diorama.Hide();

            if (living)
            {
                Label(content, "World consequence", world.Consequence, 16, 52, Ink,
                    TextAnchor.MiddleCenter);
                if (world.Choices.Length > 0)
                    Label(content, "Remembered choice", world.Memory, 13, 38, Moss,
                        TextAnchor.MiddleCenter);
            }
            else
                Label(content, "Puzzle condition", "Same water puzzles, without the living-world layer.",
                    14, 44, Moss, TextAnchor.MiddleCenter);

            if (showingMemory && world.Choices.Length > 0 && living)
            {
                var moment = Card(content, "A remembered moment", Sand);
                Label(moment.transform, "Moment title", "A NEW MEMORY IN THE GROVE", 12, 26, Leaf);
                Label(moment.transform, "Story", VillageStory.MemoryEvent(world), 17, 170, Ink);
                Button(content, "Continue", "Continue along the stream", () => {
                    showingMemory = false;
                    Rebuild(true);
                }, 62, true);
                Button(content, "Change your choice", "Change this choice", () => {
                    if (!world.ReconsiderLatestChoice()) return;
                    if (localPlaytest != null) localPlaytest.Reconsider(Time.unscaledTime);
                    SaveWorld();
                    showingMemory = false;
                    Rebuild(false);
                }, 47);
            }
            else if (world.Completed)
            {
                var conclusion = Card(content, "The ending", Sand);
                Label(conclusion.transform, "Final words", world.Mode == PlayMode.Living ?
                    world.Ending : "TWELVE STREAMS COMPLETED\n\nEvery stream is flowing. Which puzzle did you enjoy most?",
                    17, 220, Ink);
                if (living) MemoryJournal();
                Button(content, "Reset title", confirmReset ?
                    "Confirm: begin a new garden" : "Explore another garden",
                    () => {
                        if (!confirmReset) { confirmReset = true; Rebuild(); return; }
                        WorldStore.Clear();
                        world = WorldModel.New();
                        confirmReset = false;
                        showJournal = false;
                        showingMemory = false;
                        speakingWith = -1;
                        Rebuild(true);
                    }, 62, confirmReset);
                if (confirmReset)
                    Button(content, "Cancel reset", "Keep this garden", () => {
                        confirmReset = false;
                        Rebuild();
                    });
            }
            else
                BuildPuzzle();

            if (living && !world.Completed && !showingMemory && world.Choices.Length > 0)
                MemoryJournal();

            // Optional face-to-face NPC interaction. Character dialogue is
            // genuinely driven by previous world decisions, not generated text.
            // It stays below the puzzle so onboarding reaches gameplay first.
            if (living && !showingMemory)
            {
                Label(content, "Neighbors", "THREE NEIGHBORS, THREE WAYS OF SEEING YOUR GARDEN",
                    12, 27, Leaf);
                var neighbors = Row(content, "Speak with a neighbor");
                Button(neighbors.transform, "Mira", "Mira", () => OpenConversation(0), 52);
                Button(neighbors.transform, "Orin", "Orin", () => OpenConversation(1), 52);
                Button(neighbors.transform, "Fen", "Fen", () => OpenConversation(2), 52);
                if (speakingWith >= 0)
                {
                    var conversation = Card(content, "A neighbor's memory", Sand);
                    Label(conversation.transform, "Neighbor name",
                        VillageStory.VillagerName(speakingWith), 16, 31, Leaf);
                    Label(conversation.transform, "What they say",
                        VillageStory.Speak(world, speakingWith), 16, 132, Ink);
                    Button(conversation.transform, "Thanks", "Thanks for telling me",
                        () => { speakingWith = -1; Rebuild(); }, 50, true);
                }
            }

            // Research-only comparison controls are intentionally below the
            // puzzle. They must not push the first playable move off-screen.
            if (world.CanSwitchMode)
            {
                Label(content, "Comparison hint",
                    "PLAYTEST MODE  ·  Choose before your first move", 11, 21, Moss);
                var modes = Row(content, "Comparison modes");
                Button(modes.transform, "Living mode", "Living world", () => SwitchMode(PlayMode.Living),
                    52, living);
                Button(modes.transform, "Puzzle control", "Puzzle only", () => SwitchMode(PlayMode.PuzzleOnly),
                    52, !living);
            }

            // Puzzle-only condition must not display or switch into the
            // changing village art: doing so would confound the experiment.
            if (living)
            {
                // Two comparable art renderers using the same saved decisions.
                // Keep this manual A/B surface below the gameplay.
                Button(content, "Diorama visual comparison",
                    showMiniature ? "Visual: 2.5D miniature (tap for 2D)" :
                        "Visual: 2D storybook (tap for 2.5D)", () => {
                        showMiniature = !showMiniature;
                        if (localPlaytest != null) localPlaytest.ChangeVisual(showMiniature, Time.unscaledTime);
                        PlayerPrefs.SetInt("evergrove:visual-miniature", showMiniature ? 1 : 0);
                        PlayerPrefs.Save();
                        Rebuild();
                    }, 49);
                if (showMiniature)
                    Button(content, "Optional miniature motion",
                        animateMiniature ? "Miniature motion: ON (tap to stop)" :
                            "Miniature motion: OFF (tap to animate)", () => {
                            animateMiniature = !animateMiniature;
                            PlayerPrefs.SetInt("evergrove:miniature-motion", animateMiniature ? 1 : 0);
                            PlayerPrefs.Save();
                            Rebuild();
                        }, 45);
            }

            Button(content, "Quiet audio setting",
                soundOn ? "Gentle chime: ON (tap to mute)" : "Gentle chime: OFF (tap to enable)",
                () => {
                    soundOn = !soundOn;
                    PlayerPrefs.SetInt("evergrove:gentle-audio", soundOn ? 1 : 0);
                    PlayerPrefs.Save();
                    Rebuild();
                }, 44);
            if (!world.Completed && world.Round > 0)
            {
                Button(content, "Restart garden",
                    confirmReset ? "Confirm: erase this garden" : "Start a different garden",
                    () => {
                        if (!confirmReset) { confirmReset = true; Rebuild(); return; }
                        WorldStore.Clear();
                        world = WorldModel.New();
                        confirmReset = false;
                        showJournal = false;
                        showingMemory = false;
                        Rebuild(true);
                    }, 45, confirmReset);
                if (confirmReset)
                    Button(content, "Cancel", "Keep my garden", () => {
                        confirmReset = false;
                        Rebuild();
                    }, 44);
            }
            if (localPlaytest == null)
            {
                Button(content, "Playtest consent", "Optional: record a private playtest session",
                    () => {
                        localPlaytest = new LocalPlaytest(world.Mode, showMiniature,
                            world.Round, Time.unscaledTime);
                        playtestCopied = false;
                        Rebuild();
                    }, 44);
                Label(content, "No tracking by default",
                    "Only starts when you tap. Nothing is uploaded, saved or tied to an identity.",
                    12, 43, Moss, TextAnchor.MiddleCenter);
            }
            else
            {
                Button(content, "Copy playtest", "Copy my local playtest notes",
                    () => {
                        GUIUtility.systemCopyBuffer = localPlaytest.Report(Time.unscaledTime);
                        playtestCopied = true;
                        Rebuild();
                    }, 48);
                if (playtestCopied)
                    Label(content, "Copied privately",
                        "Copied to clipboard. Paste into your own research notes when ready.",
                        12, 43, Moss, TextAnchor.MiddleCenter);
                Button(content, "Stop playtest", "Stop and clear local playtest notes",
                    () => { localPlaytest = null; playtestCopied = false; Rebuild(); }, 43);
            }
            Label(content, "Respectful play", "Offline by default  ·  No pressure  ·  No ads  ·  No account",
                12, 36, Moss, TextAnchor.MiddleCenter);
            Canvas.ForceUpdateCanvases();
            LayoutRebuilder.ForceRebuildLayoutImmediate(content);
            scroll.verticalNormalizedPosition = top ? 1f : previousPosition;
        }

        private void MemoryJournal()
        {
            var button = Button(content, "Memories", showJournal ?
                "Hide village memories" : "Read the village's memories", () => {
                    showJournal = !showJournal;
                    Rebuild();
                });
            if (!showJournal) return;
            var journal = Card(content, "A living journal", River);
            Label(journal.transform, "Journal title", "EVERGROVE  /  WHAT WE CHOSE", 12, 26, Leaf);
            foreach (string entry in world.Journal)
                Label(journal.transform, "Recorded choice", entry, 14, 39, Ink);
            if (world.Journal.Length == 0)
                Label(journal.transform, "Blank page", "Your first story has not been written yet.", 14, 48, Moss);
        }

        private void BuildPuzzle()
        {
            Puzzle puzzle = world.CurrentPuzzle;
            PipeInsight insight = world.Insight;
            Label(content, "Chapter",
                "STREAM " + (world.Round + 1) + " / " + PuzzleCatalog.Count + "  ·  " + puzzle.Name,
                13, 30, Leaf, TextAnchor.MiddleCenter);
            if (puzzle.AnchoredTile >= 0)
                Label(content, "Changed landscape rule", puzzle.EnvironmentalRule,
                    14, 48, Leaf, TextAnchor.MiddleCenter);
            Label(content, "Prompt", world.Round == 0 ?
                "S = Spring   G = Grove   ·   Tap tiles to connect the water" :
                VillageStory.ChapterPrompt(world), 14, 34, Moss,
                TextAnchor.MiddleCenter);
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
            int seenEnds = 0;
            for (int index = 0; index < puzzle.CellCount; index++)
            {
                int tappedIndex = index;
                var tileNode = Node("Water tile", gridNode.transform);
                var image = tileNode.AddComponent<Image>();
                bool reached = insight.Flowing[index];
                image.color = world.Solved ? Hex("#D1E6C8") :
                    reached ? Hex("#D8EFED") : Hex("#F0F1E8");
                var tile = tileNode.AddComponent<Button>();
                tile.targetGraphic = image;
                tile.interactable = !world.Solved && !puzzle.IsAnchored(index);
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
                    (puzzle.IsAnchored(index) ? ", protected landmark, cannot rotate" : ", tap to turn");
                if (puzzle.IsAnchored(index)) image.color = Hex("#F1DDB3");
                var pipeNode = Node("Visible channel", tileNode.transform);
                Fill(pipeNode.GetComponent<RectTransform>());
                var pipe = pipeNode.AddComponent<PipeGraphic>();
                pipe.Configure(puzzle.Kinds[index], rotations[index], world.Solved);
                pipe.raycastTarget = false;
                tileArtwork[index] = pipe;
                if (puzzle.IsAnchored(index))
                {
                    var badge = Node("Protected landmark badge", tileNode.transform);
                    RectTransform badgeRect = badge.GetComponent<RectTransform>();
                    badgeRect.anchorMin = new Vector2(1f, 1f);
                    badgeRect.anchorMax = new Vector2(1f, 1f);
                    badgeRect.pivot = new Vector2(1f, 1f);
                    badgeRect.sizeDelta = new Vector2(22f, 22f);
                    badgeRect.anchoredPosition = new Vector2(-3f, -3f);
                    var badgeLabel = badge.AddComponent<Text>();
                    badgeLabel.font = font;
                    badgeLabel.fontSize = 14;
                    badgeLabel.alignment = TextAnchor.MiddleCenter;
                    badgeLabel.color = Ink;
                    badgeLabel.raycastTarget = false;
                    badgeLabel.text = "●";
                }
                if (puzzle.Kinds[index] == PipeKind.End)
                {
                    // Visible endpoint semantics instead of relying only on
                    // screen-reader names or a color difference.
                    var markerNode = Node("Spring or Grove label", tileNode.transform);
                    RectTransform markerRect = markerNode.GetComponent<RectTransform>();
                    markerRect.anchorMin = new Vector2(0f, 1f);
                    markerRect.anchorMax = new Vector2(0f, 1f);
                    markerRect.pivot = new Vector2(0f, 1f);
                    markerRect.sizeDelta = new Vector2(23f, 20f);
                    markerRect.anchoredPosition = new Vector2(4f, -4f);
                    var marker = markerNode.AddComponent<Text>();
                    marker.font = font;
                    marker.fontSize = 13;
                    marker.alignment = TextAnchor.MiddleCenter;
                    marker.color = Ink;
                    marker.raycastTarget = false;
                    marker.text = seenEnds == 1 ? "S" : "G";
                }
            }

            string feedback = world.Solved
                ? "The stream flows through every tile."
                : "Water reaches " + insight.ConnectedCount + " / " + puzzle.CellCount +
                    " tiles   ·   " + insight.LeakCount + " unmatched joins";
            feedbackText = Label(content, "Water feedback", feedback, 15, 42, Ink, TextAnchor.MiddleCenter);

            if (world.Solved)
            {
                Button(content, "Revisit water", "Undo the last water turn",
                    () => Act(() => world.Undo(), "undo"), 44).interactable = world.CanUndo;
                if (world.Mode == PlayMode.PuzzleOnly)
                    Button(content, "Next stream", "Next stream", () => {
                        if (!world.NextPuzzle()) return;
                        SaveWorld();
                        Rebuild(true);
                    }, 59, true);
                else
                {
                    Label(content, "Choose title", "THE WATER HAS ARRIVED. WHAT WILL YOU HELP GROW?",
                        14, 52, Leaf, TextAnchor.MiddleCenter);
                    for (int choiceIndex = 0; choiceIndex < 3; choiceIndex++)
                    {
                        VillageChoice choice = (VillageChoice)choiceIndex;
                        string caption = VillageStory.ChoiceLabel(choice) +
                            "\n" + VillageStory.ChoicePromise(choice, world);
                        Button(content, "Choice " + choice, caption, () => MakeChoice(choice), 77,
                            choiceIndex == 0);
                    }
                }
            }
            else
            {
                var tools = Row(content, "Gentle controls");
                undoButton = Button(tools.transform, "Undo", "Undo", () => Act(() => world.Undo(), "undo"),
                    52);
                undoButton.interactable = world.CanUndo;
                Button(tools.transform, "Hint", "Guide me", () => Act(() => world.GuideOneConnection(), "hint"), 52);
                Button(content, "Restart", "Start this stream again", () => Act(() => world.Restart(), "restart"), 46);
            }
            Label(content, "How to play", "Tap to rotate. S marks the spring; G marks the garden outlet. " +
                "Every pipe must join without open ends. Take as long as you like.",
                13, 56, Moss, TextAnchor.MiddleCenter);
        }

        private void OpenConversation(int villagerIndex)
        {
            speakingWith = villagerIndex;
            if (localPlaytest != null) localPlaytest.Talk(Time.unscaledTime);
            Rebuild();
        }

        private void SwitchMode(PlayMode mode)
        {
            if (!world.CanSwitchMode) return;
            speakingWith = -1;
            world = WorldModel.New(mode);
            if (localPlaytest != null) localPlaytest.ChangeMode(mode, Time.unscaledTime);
            SaveWorld();
            Rebuild();
        }

        private void RefreshBoard()
        {
            if (world.Completed || visibleTiles == null) return;
            Puzzle puzzle = world.CurrentPuzzle;
            if (visibleTiles.Length != puzzle.CellCount) { Rebuild(); return; }
            PipeInsight insight = world.Insight;
            int[] angles = world.Rotations;
            for (int i = 0; i < puzzle.CellCount; i++)
            {
                tileBackgrounds[i].color = puzzle.IsAnchored(i) ? Hex("#F1DDB3") :
                    world.Solved ? Hex("#D1E6C8") :
                    insight.Flowing[i] ? Hex("#D8EFED") : Hex("#F0F1E8");
                visibleTiles[i].interactable = !world.Solved && !puzzle.IsAnchored(i);
                tileArtwork[i].Configure(puzzle.Kinds[i], angles[i], world.Solved);
            }
            if (feedbackText != null)
                feedbackText.text = "Water reaches " + insight.ConnectedCount + " / " +
                    puzzle.CellCount + " tiles   ·   " + insight.LeakCount + " unmatched joins";
            if (undoButton != null) undoButton.interactable = world.CanUndo;
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
            else RefreshBoard();
        }

        private void TapTile(int index)
        {
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
            yield return null;
            if (scroll != null) scroll.verticalNormalizedPosition = 0f;
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

        private void SaveWorld()
        {
            if (!WorldStore.Save(world))
                Debug.LogWarning("Evergrove is playable but cannot persist this change.");
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
