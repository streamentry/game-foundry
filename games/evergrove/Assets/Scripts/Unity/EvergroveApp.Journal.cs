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

        // The menu button's own dot (B1b) clears when the menu is opened; the book inside the
        // menu keeps its dot until the book itself is opened (JournalSeenKey).
        private const string MenuSeenKey = "evergrove:menu-seen";

        private void MarkUnreadMemory(Button book, string seenKey = JournalSeenKey)
        {
            int seen = PlayerPrefs.GetInt(seenKey, 0);
            if (seen > world.Choices.Length)
            {
                // Read in another garden (restored or new): this garden's memories so far count as seen.
                PlayerPrefs.SetInt(seenKey, world.Choices.Length);
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
            SheetClose(journalContent, "Close journal", CloseJournal);
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
                    LongPressCaption.Attach(page, "Memory " + (pageIndex + 1) + ". Tap to open it.", font);
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
            // The family page (I269): letters, help and puzzles played together. Checklist 8: offered from day 3.
            if (FamilyShown()) IconButton(tabs, "Family in the garden", "Family in the garden", MosseloraArt.IconNeighbors,
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
            WaysAtAGlance(page.transform); // I423
            PairingRows(page.transform); // I369
            VisitorsRows(page.transform);
            YearRows(page.transform); // I392
        }

        // I423: the garden's ways at a glance: one painting per good neighbour (in colour once
        // found, a pencil shadow before; the deeper three only once open), then a dot per visitor.
        private void WaysAtAGlance(Transform page)
        {
            if (world == null || world.Mode != PlayMode.Living) return;
            Transform row = CenteredRowIn(page, "Good neighbours at a glance");
            row.GetComponent<HorizontalLayoutGroup>().spacing = 6;
            int open = Pairings.Open(world);
            foreach (Pairing pairing in Enum.GetValues(typeof(Pairing)))
            {
                if ((open & PairingMask.Of(pairing)) == 0) continue;
                var mark = Node(pairing.ToString(), row);
                var layout = mark.AddComponent<LayoutElement>();
                layout.preferredWidth = layout.minWidth = layout.preferredHeight = 34f;
                var image = mark.AddComponent<Image>();
                image.sprite = PairingMarks(pairing)[0];
                image.preserveAspect = true;
                image.color = world.HasPairing(pairing) ? Color.white : new Color(0.33f, 0.30f, 0.25f, 0.18f);
                image.raycastTarget = false;
            }
            Transform dots = CenteredRowIn(page, "Visitors at a glance");
            dots.GetComponent<HorizontalLayoutGroup>().spacing = 8;
            foreach (Visitor visitor in Enum.GetValues(typeof(Visitor)))
            {
                var dot = Node(visitor.ToString(), dots);
                var layout = dot.AddComponent<LayoutElement>();
                layout.preferredWidth = layout.minWidth = layout.preferredHeight = 14f;
                var image = dot.AddComponent<Image>();
                image.sprite = MosseloraArt.Rounded(7);
                image.color = Logged(visitor) ? VisitorColour(visitor) : new Color(0.33f, 0.30f, 0.25f, 0.16f); // checklist 49: night creatures once seen
                image.raycastTarget = false;
            }
        }

        private static Color VisitorColour(Visitor visitor)
        {
            switch (visitor)
            {
                case Visitor.Bees: return new Color(0.98f, 0.78f, 0.18f, 1f);
                case Visitor.Birds: return new Color(0.45f, 0.38f, 0.30f, 1f);
                case Visitor.Heron: return new Color(0.62f, 0.68f, 0.74f, 1f);
                case Visitor.Hedgehog: return new Color(0.55f, 0.42f, 0.30f, 1f);
                case Visitor.Owl: return new Color(0.50f, 0.40f, 0.28f, 1f);
                case Visitor.Kingfisher: return new Color(0.16f, 0.55f, 0.78f, 1f);
                case Visitor.Moths: return new Color(0.90f, 0.84f, 0.56f, 1f); // checklist 49
                case Visitor.Frogs: return new Color(0.42f, 0.66f, 0.38f, 1f);
                default: return new Color(0.93f, 0.62f, 0.72f, 1f);
            }
        }

        private void AlmanacPage()
        {
            var page = Card(journalContent, "The almanac", new Color(0f, 0f, 0f, 0f));
            MemoryShelfRow(page.transform);
            TechniqueRow(page.transform);
            WeeklyDiaryRow(page.transform);
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
                // Checklist 91: each almanac cell is a tap target, at least 48 pt. Seven
                // columns on a narrow page would fall below that, so six take over there.
                int columns = (PageWidth() - 40f) / 7f - 4f >= 48f ? 7 : 6;
                float cell = Mathf.Floor(Mathf.Min(52f, (PageWidth() - 40f) / columns - 4f));
                layout.cellSize = new Vector2(cell, cell);
                layout.spacing = new Vector2(4f, 4f);
                layout.childAlignment = TextAnchor.UpperCenter;
                layout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
                layout.constraintCount = columns;
                int rows = (count + columns - 1) / columns;
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
                        LongPressCaption.Attach(slot, where, font);
                        continue;
                    }
                    var name = slot.AddComponent<Button>();
                    name.transition = Selectable.Transition.None;
                    string label = item.Name;
                    // I304: its name, then a line from its keeper (true things only).
                    string said = char.ToUpperInvariant(label[0]) + label.Substring(1) + "\n" + Almanac.KeeperLine(item, world);
                    name.onClick.AddListener(() => RuleToast.Show(slot.GetComponent<RectTransform>(),
                        said, font, Ink, MosseloraTheme.CardFill));
                    LongPressCaption.Attach(slot, said, font);
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
                bool come = Logged(visitor); // checklist 49: moths and frogs are logged once seen
                Transform row = CenteredRowIn(page, "Visitor " + visitor);
                var layout = row.GetComponent<HorizontalLayoutGroup>();
                layout.spacing = 10;
                layout.childAlignment = TextAnchor.MiddleLeft;
                var mark = Node(visitor.ToString(), row);
                var markLayout = mark.AddComponent<LayoutElement>();
                markLayout.preferredWidth = markLayout.minWidth = markLayout.preferredHeight = 36f;
                var markImage = mark.AddComponent<Image>();
                markImage.sprite = MosseloraArt.Rounded(12);
                markImage.color = !come ? new Color(0.33f, 0.30f, 0.25f, 0.16f) : VisitorColour(visitor);
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
            // Checklist 8: a relative's gifts stay off the shelf until family is offered.
            if (!FamilyShown()) shelf = System.Array.FindAll(shelf, k => !FamilyOffer.IsFamilyKeepsake(k.Art));
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
                LongPressCaption.Attach(slot, said, font);
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
    }
}
