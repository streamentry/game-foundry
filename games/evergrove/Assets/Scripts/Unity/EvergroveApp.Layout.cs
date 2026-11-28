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
            // TileOpenings is English copy, so other locales keep the localized label unchanged.
            if (GameLocalization.Current == "en")
            {
                string opens = TileOpenings.Words(puzzle.Kinds[index], world.Rotations[index]);
                if (opens.Length > 0) label += " " + opens;
            }
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
    }
}
