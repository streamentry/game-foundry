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
            saveWarningCaption.color = MosseloraTheme.EarthInk;
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
            LongPressCaption.Attach(node, text.text, text.font); // Checklist 19: hold to hear what it is
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
                                  bool round = false, bool? stateOn = null)
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
            // Colour-blind pass (checklist 92): an on/off token with one icon for both states wears a check when on.
            if (stateOn.HasValue) MosseloraArt.StateMark(node.transform, stateOn.Value, Mathf.Round(height * 0.3f));
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
            }
            LongPressCaption.Attach(node, text.text, text.font); // Checklist 19: replaces IconTip for every icon
            return button;
        }
    }
}
