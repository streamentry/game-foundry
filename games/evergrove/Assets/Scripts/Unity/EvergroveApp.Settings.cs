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
            LongPressCaption.Attach(node, LocalizedVillage.Choice(choice), font);
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
            SpeakLessButton(row.transform);
            IconButton(row.transform, "Haptics setting",
                Haptics.Enabled ? "Light touch feedback: on" : "Light touch feedback: off",
                MosseloraArt.IconTap, () => {
                    Haptics.Enabled = !Haptics.Enabled;
                    Rebuild();
                }, SettingToken, Haptics.Enabled ? MosseloraTheme.ForestAction : MosseloraTheme.QuietAction,
                false, null, false, 0, 0, true, stateOn: Haptics.Enabled);
            // At most three tiles to a row: five in one row ran off a 375 pt phone (v1 tour, 2026-11-28).
            row = Row(panel.transform, "Settings row 2b");
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
                false, null, false, 0, 0, true, stateOn: DayLightOn);
            // Checklist 5: the hand, changeable any time; it mirrors the menu, the trays and the sideways panel.
            HandSettingButton(row.transform);
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
                    false, null, false, 0, 0, true, stateOn: GardenNotes.On);
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
            string says = NeighborSays(villager);
            // v1 #85: a remark quietened by "speak less" is not news; no badge for silence.
            return says.Length > 0 && says != PlayerPrefs.GetString(HeardKey + villager, "");
        }
    }
}
