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
                MosseloraArt.IconConfirm, () => { speakingWith = -1; Rebuild(); }, 48,
                MosseloraTheme.ForestAction);
            // Tucked into the bubble's lower-right corner, outside the text flow.
            thanks.gameObject.GetComponent<LayoutElement>().ignoreLayout = true;
            var thanksRect = thanks.GetComponent<RectTransform>();
            thanksRect.anchorMin = thanksRect.anchorMax = new Vector2(1f, 0f);
            thanksRect.pivot = new Vector2(1f, 0f);
            thanksRect.sizeDelta = new Vector2(56f, 48f); // 48 pt touch target (checklist 91)
            thanksRect.anchoredPosition = new Vector2(-10f, 10f);
            // Bring them some fruit from your trees (I231): one pear, a kind word back.
            if (world.Mode == PlayMode.Living && world.Fruit > 0)
            {
                int villager = speakingWith;
                var give = IconButton(bubble.transform, "Give fruit", "Give " + LocalizedVillage.Name(villager) + " some fruit",
                    "food-basket", () => GiveFruit(villager), 48, MosseloraTheme.QuietAction);
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
            LongPressCaption.Attach(node, label, font);
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
    }
}
