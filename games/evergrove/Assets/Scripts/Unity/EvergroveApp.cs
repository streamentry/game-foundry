using System;
using Evergrove.Core;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Evergrove.UnityClient
{
    /// <summary>
    /// Fully code-generated uGUI vertical slice. No purchased assets, server, prefabs,
    /// manually placed scene objects, or analytics are needed to play.
    /// </summary>
    public sealed class EvergroveApp : MonoBehaviour
    {
        private static readonly Color Paper = Hex("#F7F4E9");
        private static readonly Color Ink = Hex("#23443C");
        private static readonly Color Moss = Hex("#516B60");
        private static readonly Color Green = Hex("#276D54");
        private static readonly Color Pale = Hex("#DFEBDD");
        private static readonly Color Sand = Hex("#F3E5C9");
        private static readonly Color White = Color.white;

        private WorldModel world;
        private Font font;
        private RectTransform content;
        private ScrollRect scroll;
        private int? lastScreenWidth;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Bootstrap()
        {
            if (FindFirstObjectByType<EvergroveApp>() != null) return;
            var host = new GameObject("Evergrove Runtime");
            DontDestroyOnLoad(host);
            host.AddComponent<EvergroveApp>();
        }

        private static Color Hex(string value)
        {
            ColorUtility.TryParseHtmlString(value, out Color result);
            return result;
        }

        private void Awake()
        {
            world = WorldStore.Load();
            font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (font == null) font = Font.CreateDynamicFontFromOSFont("Arial", 16);
            CreateRoot();
            Rebuild();
        }

        private void Update()
        {
            // The uGUI canvas scales by screen size; safe-area changes are handled
            // without modifying the deterministic gameplay model.
            if (lastScreenWidth != Screen.width)
            {
                lastScreenWidth = Screen.width;
                if (content != null) LayoutRebuilder.MarkLayoutForRebuild(content);
            }
        }

        private static GameObject RectNode(string name, Transform parent)
        {
            var node = new GameObject(name, typeof(RectTransform));
            node.transform.SetParent(parent, false);
            return node;
        }

        private static void Stretch(RectTransform rt)
        {
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
        }

        private void CreateRoot()
        {
            var canvasNode = RectNode("Canvas", transform);
            var canvas = canvasNode.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = canvasNode.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(414, 896);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;
            canvasNode.AddComponent<GraphicRaycaster>();

            var backdrop = RectNode("Warm paper background", canvasNode.transform);
            Stretch(backdrop.GetComponent<RectTransform>());
            backdrop.AddComponent<Image>().color = Paper;

            var scrollNode = RectNode("Scroll", canvasNode.transform);
            var scrollRt = scrollNode.GetComponent<RectTransform>();
            Stretch(scrollRt);
            scroll = scrollNode.AddComponent<ScrollRect>();
            scroll.horizontal = false;
            scroll.vertical = true;
            scroll.movementType = ScrollRect.MovementType.Clamped;

            var viewport = RectNode("Viewport", scrollNode.transform);
            Stretch(viewport.GetComponent<RectTransform>());
            var viewportImage = viewport.AddComponent<Image>();
            viewportImage.color = new Color(1, 1, 1, 0.005f);
            viewport.AddComponent<RectMask2D>();
            scroll.viewport = viewport.GetComponent<RectTransform>();

            var body = RectNode("Content", viewport.transform);
            content = body.GetComponent<RectTransform>();
            content.anchorMin = new Vector2(0, 1);
            content.anchorMax = new Vector2(1, 1);
            content.pivot = new Vector2(0.5f, 1f);
            content.anchoredPosition = Vector2.zero;
            content.sizeDelta = Vector2.zero;
            var column = body.AddComponent<VerticalLayoutGroup>();
            column.childAlignment = TextAnchor.UpperCenter;
            column.padding = new RectOffset(18, 18, 24, 36);
            column.spacing = 12;
            column.childControlWidth = true;
            column.childControlHeight = true;
            column.childForceExpandWidth = true;
            column.childForceExpandHeight = false;
            var fitter = body.AddComponent<ContentSizeFitter>();
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            scroll.content = content;

            if (FindFirstObjectByType<EventSystem>() == null)
            {
                var events = new GameObject("Evergrove Event System");
                DontDestroyOnLoad(events);
                events.AddComponent<EventSystem>();
                // Old input backend remains available with Active Input Handling = Both.
                events.AddComponent<StandaloneInputModule>();
            }
        }

        private Text Label(Transform parent, string name, string text, int fontSize, int minHeight,
                           Color color, TextAnchor alignment = TextAnchor.MiddleLeft)
        {
            var node = RectNode(name, parent);
            var element = node.AddComponent<LayoutElement>();
            element.minHeight = minHeight;
            element.preferredHeight = minHeight;
            var label = node.AddComponent<Text>();
            label.font = font;
            label.fontSize = fontSize;
            label.color = color;
            label.alignment = alignment;
            label.horizontalOverflow = HorizontalWrapMode.Wrap;
            label.verticalOverflow = VerticalWrapMode.Overflow;
            label.text = text;
            label.supportRichText = false;
            return label;
        }

        private static VerticalLayoutGroup Vertical(Transform parent, string name, int spacing = 7)
        {
            var node = RectNode(name, parent);
            var stack = node.AddComponent<VerticalLayoutGroup>();
            stack.childControlWidth = true;
            stack.childControlHeight = true;
            stack.childForceExpandWidth = true;
            stack.childForceExpandHeight = false;
            stack.spacing = spacing;
            stack.padding = new RectOffset(12, 12, 12, 12);
            node.AddComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            return stack;
        }

        private Button MakeButton(Transform parent, string name, string text, Action click,
                                  int height = 56, bool primary = false)
        {
            var node = RectNode(name, parent);
            var image = node.AddComponent<Image>();
            image.color = primary ? Green : Pale;
            var button = node.AddComponent<Button>();
            button.targetGraphic = image;
            button.transition = Selectable.Transition.ColorTint;
            var palette = button.colors;
            palette.highlightedColor = primary ? new Color(0.78f, 0.92f, 0.82f) : new Color(0.9f, 1f, 0.90f);
            palette.pressedColor = new Color(0.8f, 0.82f, 0.76f);
            button.colors = palette;
            node.AddComponent<LayoutElement>().preferredHeight = height;

            var textNode = RectNode("Text", node.transform);
            Stretch(textNode.GetComponent<RectTransform>());
            var label = textNode.AddComponent<Text>();
            label.font = font;
            label.fontSize = 16;
            label.color = primary ? White : Ink;
            label.alignment = TextAnchor.MiddleCenter;
            label.text = text;
            label.horizontalOverflow = HorizontalWrapMode.Wrap;
            label.verticalOverflow = VerticalWrapMode.Overflow;
            label.raycastTarget = false;
            button.onClick.AddListener(() => click());
            return button;
        }

        private void Action(Action action)
        {
            action();
            WorldStore.Save(world);
            Rebuild();
        }

        private void Rebuild()
        {
            foreach (Transform child in content) Destroy(child.gameObject);
            bool living = world.Mode == PlayMode.Living;
            Label(content, "Eyebrow", "EVERGROVE  /  A WORLD THAT REMEMBERS", 12, 24, Green);
            Label(content, "Title", "Let the little things change everything.", 24, 64, Ink);
            Label(content, "Instruction",
                "Tap to turn each stream tile. Make one continuous, leak-free path through all nine tiles.",
                15, 62, Moss);

            if (world.CanSwitchMode)
            {
                MakeButton(content, "Choose living", "Living world (puzzle + consequences)", () => {
                    world = WorldModel.New(PlayMode.Living); WorldStore.Save(world); Rebuild();
                }, 56, living);
                MakeButton(content, "Choose control", "Puzzle only (comparison)", () => {
                    world = WorldModel.New(PlayMode.PuzzleOnly); WorldStore.Save(world); Rebuild();
                }, 56, !living);
            }

            if (living)
            {
                var panel = Vertical(content, "Living village");
                var image = panel.gameObject.AddComponent<Image>();
                image.color = Sand;
                int trees = world.Count(VillageChoice.ShadeTree);
                int bridges = world.Count(VillageChoice.Bridge);
                int flowers = world.Count(VillageChoice.Flowers);
                Label(panel.transform, "Village", "THE VILLAGE", 13, 24, Green);
                Label(panel.transform, "Counts", "Trees " + trees + "  ·  Bridges " + bridges + "  ·  Meadows " + flowers, 17, 42, Ink);
                Label(panel.transform, "Consequence", world.Consequence, 15, 58, Ink);
                Label(panel.transform, "Memory", world.Memory, 14, 48, Moss);
                foreach (string voice in world.Characters)
                    Label(panel.transform, "Villager", voice, 14, 45, Ink);
            }

            if (world.Completed)
            {
                Label(content, "End", "SIX STREAMS COMPLETED", 21, 62, Green, TextAnchor.MiddleCenter);
                Label(content, "End hint", "The little world remembers your choices. What would bring you back?",
                    17, 80, Ink, TextAnchor.MiddleCenter);
                MakeButton(content, "Erase", "Reset this local playtest", () => {
                    WorldStore.Clear(); world = WorldModel.New(); Rebuild();
                });
                LayoutRebuilder.ForceRebuildLayoutImmediate(content);
                return;
            }

            Puzzle level = PuzzleCatalog.Get(world.Round);
            Label(content, "Chapter", "STREAM " + (world.Round + 1) + " OF " + PuzzleCatalog.Count +
                "  ·  " + level.Name, 17, 42, Green, TextAnchor.MiddleCenter);

            var gridNode = RectNode("Stream", content);
            gridNode.AddComponent<LayoutElement>().preferredHeight = 292;
            var grid = gridNode.AddComponent<GridLayoutGroup>();
            grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            grid.constraintCount = 3;
            grid.cellSize = new Vector2(88, 88);
            grid.spacing = new Vector2(8, 8);
            grid.childAlignment = TextAnchor.MiddleCenter;
            int[] angles = world.Rotations;
            int seenEnds = 0;
            for (int index = 0; index < 9; index++)
            {
                int captured = index;
                string kind = level.Kinds[index] == PipeKind.End
                    ? (++seenEnds == 1 ? "spring" : "outlet") : "stream tile";
                var tile = MakeButton(gridNode.transform, "Tile " + index, "", () => Action(() => world.Rotate(captured)), 88);
                tile.interactable = !world.Solved;
                var graphicNode = RectNode("Stream shape", tile.transform);
                Stretch(graphicNode.GetComponent<RectTransform>());
                var graphic = graphicNode.AddComponent<PipeGraphic>();
                graphic.Configure(level.Kinds[index], angles[index], world.Solved);
                graphic.raycastTarget = false;
                tile.gameObject.name = "Row " + (index / 3 + 1) + " Column " + (index % 3 + 1) + " " + kind;
            }

            Label(content, "Status", world.Solved
                ? (living ? "The water flows. Choose how your village will change." : "Complete. Continue to the next stream.")
                : "Connect each tile. Both ends must be sealed into one route. Tap to rotate.",
                16, 70, Green, TextAnchor.MiddleCenter);
            MakeButton(content, "Undo", "Undo last turn", () => Action(() => world.Undo()), 54).interactable = world.CanUndo;
            MakeButton(content, "Restart", "Restart this stream", () => Action(() => world.Restart()), 54);

            if (world.Solved && living)
            {
                Label(content, "Choice heading", "THE WATER REACHES THE GROVE — WHAT CHANGES?", 15, 58,
                    Green, TextAnchor.MiddleCenter);
                MakeButton(content, "Tree", "Plant a shade tree", () => Action(() => world.Choose(VillageChoice.ShadeTree)), 62, true);
                MakeButton(content, "Bridge", "Restore the footbridge", () => Action(() => world.Choose(VillageChoice.Bridge)), 62);
                MakeButton(content, "Flowers", "Grow wildflowers", () => Action(() => world.Choose(VillageChoice.Flowers)), 62);
            }
            if (world.Solved && !living)
                MakeButton(content, "Next", "Next stream", () => Action(() => world.NextPuzzle()), 62, true);
            Label(content, "Footnote", "Offline • no timer • no account • no AI inference • no streaks",
                12, 44, Moss, TextAnchor.MiddleCenter);
            LayoutRebuilder.ForceRebuildLayoutImmediate(content);
        }
    }
}
