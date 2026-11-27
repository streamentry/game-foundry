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
        private const float ChromeInset = 84f; // book button with its new-memory dot + 10 pt margin + air

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
    }
}
