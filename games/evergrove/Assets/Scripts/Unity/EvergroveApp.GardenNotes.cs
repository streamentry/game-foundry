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

        // The seeds you have, top right of home: a sprout and a number.
        private void SeedPurse()
        {
            if (titleScreen == null || world.Mode != PlayMode.Living || world.Choices.Length == 0) return;
            var purse = Node("Seeds you have", titleScreen.transform);
            var rect = purse.GetComponent<RectTransform>();
            Rect safe = Screen.safeArea;
            float w = Mathf.Max(1, Screen.width), h = Mathf.Max(1, Screen.height);
            rect.anchorMin = rect.anchorMax = new Vector2(safe.xMax / w, safe.yMax / h);
            rect.pivot = new Vector2(1f, 1f);
            rect.sizeDelta = new Vector2(104f, 48f);
            rect.anchoredPosition = new Vector2(-12f, -ChromeTop - 4f);
            var body = purse.AddComponent<Image>();
            MosseloraArt.Round(body, new Color(0.98f, 0.95f, 0.86f, 0.96f), 24);
            MosseloraArt.Lift(body, 2f, 0.2f);
            SeedPurseTap(purse, body); // tap opens the seed book (EvergroveApp.Economy.cs)
            var row = purse.AddComponent<HorizontalLayoutGroup>();
            row.padding = new RectOffset(8, 12, 4, 4);
            row.spacing = 4;
            row.childAlignment = TextAnchor.MiddleCenter;
            row.childControlWidth = true;
            row.childControlHeight = true;
            row.childForceExpandWidth = false;
            row.childForceExpandHeight = false;
            var icon = Node("Seed", purse.transform);
            var iconLayout = icon.AddComponent<LayoutElement>();
            iconLayout.preferredWidth = iconLayout.preferredHeight = 30f;
            var iconImage = icon.AddComponent<Image>();
            iconImage.sprite = MosseloraArt.Load(TreePlay.SeedIcon);
            iconImage.preserveAspect = true;
            iconImage.raycastTarget = false;
            Text count = Label(purse.transform, "Seed count", world.Seeds.ToString(), 20, 0, Ink, TextAnchor.MiddleLeft);
            Emphasize(count);
            // One line at any text size; the purse grows leftward to fit "100" at 150% (I227).
            count.horizontalOverflow = HorizontalWrapMode.Overflow;
            // Fruit from your trees sits beside the seeds once you have picked any (I229).
            purseFruit = null;
            if (world.Fruit > 0 || fruitPicked)
            {
                var pear = Node("Fruit", purse.transform);
                var pearLayout = pear.AddComponent<LayoutElement>();
                pearLayout.preferredWidth = pearLayout.preferredHeight = 30f;
                purseFruit = pear.AddComponent<Image>();
                purseFruit.sprite = MosseloraArt.Load("food-pear");
                purseFruit.preserveAspect = true;
                purseFruit.raycastTarget = false;
                Text fruit = Label(purse.transform, "Fruit count", world.Fruit.ToString(), 20, 0, Ink, TextAnchor.MiddleLeft);
                Emphasize(fruit);
                fruit.horizontalOverflow = HorizontalWrapMode.Overflow;
            }
            var fit = purse.AddComponent<ContentSizeFitter>();
            fit.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
        }
        // ---- I311/I312: when the garden's season turns, the next neighbour leaves a note with
        // a tale. The first time home is seen only records the season, so nothing is sudden.
        private void NoteSeasonTurn()
        {
            if (world == null || world.Mode != PlayMode.Living || world.Choices.Length == 0) return;
            string seenKey = "evergrove:season-seen:" + world.Garden, turnsKey = "evergrove:season-turns:" + world.Garden;
            int now = (int)GardenSeason.For(world) + 1, seen = PlayerPrefs.GetInt(seenKey, 0);
            if (seen == now) return;
            PlayerPrefs.SetInt(seenKey, now);
            if (seen != 0)
            {
                int turn = PlayerPrefs.GetInt(turnsKey, 0);
                SeasonNote note = SeasonTales.For(GardenSeason.For(world), turn);
                if (world.KeepNote(note.From, note.Body)) { letterArrived = true; SaveWorld(); }
                if (GardenSeason.Festival(world) &&
                    world.KeepNote("Orin", GardenSeason.FestivalNote(GardenSeason.YearAfterStory(world.PuzzlesSolved))))
                { letterArrived = true; SaveWorld(); }
                PlayerPrefs.SetInt(turnsKey, turn + 1);
            }
            PlayerPrefs.Save();
        }

        // I313: a wish that has come true is thanked for once, by letter.
        private void NoteGrantedWishes()
        {
            if (world == null || world.Mode != PlayMode.Living) return;
            for (int villager = 0; villager < 3; villager++)
            {
                NeighbourWish wish = NeighbourWish.For(world, villager);
                string key = "evergrove:wish-thanked:" + world.Garden + ":" + villager;
                if (wish == null || !wish.Granted || PlayerPrefs.GetInt(key, 0) == 1) continue;
                if (!world.KeepNote(wish.From, wish.Thanks)) continue; // review fix: try again next time
                PlayerPrefs.SetInt(key, 1);
                letterArrived = true;
                SaveWorld();
            }
            PlayerPrefs.Save();
        }

        // I315: at a few milestones of puzzles solved, a neighbour leaves a cutting and a note.
        private void NoteNeighbourGifts()
        {
            if (world == null || world.Mode != PlayMode.Living) return;
            string key = "evergrove:neighbour-gifts:" + world.Garden;
            int given = PlayerPrefs.GetInt(key, 0);
            for (int due = NeighbourGifts.Due(world.PuzzlesSolved, given); due > 0; due--, given++)
            {
                int kind = NeighbourGifts.CuttingFor(world, given);
                if (kind < 0 || !world.ReceiveNeighbourCutting(kind)) break; // basket full: it waits for room
                string what = Nursery.Family(kind) == VillageChoice.Bridge
                    ? "I've made you a " + Nursery.Item(kind).Name + ". It's in your basket; set it wherever you like."
                    : "I've left a cutting of " + Nursery.Item(kind).Name + " in your basket, from my own. Plant it wherever you like.";
                world.KeepNote(NeighbourGifts.From(given), what);
                letterArrived = true;
                PlayerPrefs.SetInt(key, given + 1);
                SaveWorld();
            }
            PlayerPrefs.Save();
        }

        // I359: when you come back after two days or more, Mira's next words say, truly, what
        // the weather did meanwhile, and that the garden looked after itself. Said once.
        private DateTime? sinceSeen; // the last day seen before this return, until Mira has said it (SC01)

        private void NoteReturn()
        {
            if (world == null || world.Mode != PlayMode.Living || world.Choices.Length == 0) return;
            string key = "evergrove:last-seen:" + world.Garden;
            string today = DateTime.Now.ToString("yyyyMMdd", CultureInfo.InvariantCulture);
            string seen = PlayerPrefs.GetString(key, "");
            if (seen != today)
            {
                if (seen.Length == 8 && DateTime.TryParseExact(seen, "yyyyMMdd", CultureInfo.InvariantCulture,
                        DateTimeStyles.None, out DateTime last))
                    sinceSeen = last;
                spokenThisVisit.Clear(); spokenVariant.Clear(); // SC02: a new day is a new visit
                PlayerPrefs.SetString(key, today);
                // I419: the hour of each day's first return, counted on this device only.
                int[] hours = HourCounts();
                Now(out int hour, out int minute);
                hours[(int)DayLight.PhaseAt(hour, minute)]++;
                PlayerPrefs.SetString(HoursKey, string.Join(",", Array.ConvertAll(hours, h => Mathf.Min(h, 999).ToString(CultureInfo.InvariantCulture))));
                NoteAnniversary();
                PlayerPrefs.Save();
            }
        }

        // I419: device-only counts of the hour you come (first return of each day), never in the
        // garden file, which travels to family.
        private string HoursKey { get { return "evergrove:hours:" + world.Garden; } }

        private int[] HourCounts()
        {
            var counts = new int[Companion.PhaseCount];
            string[] parts = PlayerPrefs.GetString(HoursKey, "").Split(',');
            for (int i = 0; i < counts.Length && i < parts.Length; i++)
                int.TryParse(parts[i], NumberStyles.Integer, CultureInfo.InvariantCulture, out counts[i]);
            return counts;
        }

        // I421: the garden's anniversary (from the first day it was opened on this device):
        // Orin keeps a note, once that day.
        private void NoteAnniversary()
        {
            string firstKey = "evergrove:first-seen:" + world.Garden;
            string today = DateTime.Now.ToString("yyyyMMdd", CultureInfo.InvariantCulture);
            string first = PlayerPrefs.GetString(firstKey, "");
            if (first.Length != 8) { PlayerPrefs.SetString(firstKey, today); return; }
            if (!DateTime.TryParseExact(first, "yyyyMMdd", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime began)) return;
            int years = Companion.Anniversary(began, DateTime.Now.Date);
            string doneKey = "evergrove:anniversary:" + world.Garden;
            if (years <= 0 || PlayerPrefs.GetInt(doneKey, 0) >= years) return;
            if (world.KeepNote("Orin", Companion.AnniversaryNote(years))) { letterArrived = true; SaveWorld(); }
            PlayerPrefs.SetInt(doneKey, years);
        }

        // I374/I382: the visitors a garden's good neighbours bring, where they'd stand, when
        // they're out. I384: the same for a family garden you visit (your hour, their season).
        private void SetVisitorsFor(WorldModel garden, GardenMap map, System.Collections.Generic.List<PlacedThing> things)
        {
            DayPhase phase = DayLightOn ? CurrentPhase() : DayPhase.Day;
            Season season = GardenSeason.For(garden);
            GardenMapView.HeronAtNext = VisitorSpot(garden, map, things, Visitor.Heron, Pairing.ByTheWater, season, phase);
            GardenMapView.HedgehogAtNext = VisitorSpot(garden, map, things, Visitor.Hedgehog, Pairing.Sheltered, season, phase);
            GardenMapView.OwlAtNext = VisitorSpot(garden, map, things, Visitor.Owl, Pairing.Grove, season, phase);
            GardenMapView.KingfisherAtNext = VisitorSpot(garden, map, things, Visitor.Kingfisher, Pairing.Waterside, season, phase);
            GardenNightCreatures.Cells = NightSpots(map, things, phase); // checklist 49: moths and frogs, evening only
            NoteNightsSeen(garden, GardenNightCreatures.Cells); // who visits (I327): logged the first time they are out
        }

        private int VisitorSpot(WorldModel garden, GardenMap map, System.Collections.Generic.List<PlacedThing> things,
            Visitor visitor, Pairing pairing, Season season, DayPhase phase)
        {
            if (!Visitors.Welcomes(garden, visitor) || !Visitors.OutIn(visitor, season, phase, TodayWeather(garden))) return -1;
            return Pairings.SpotFor(map, things, pairing);
        }

        // I372: holding something to place, the free spots where it would make a good-neighbour
        // pairing not yet found. Nothing is held: no spots.
        private int[] PairingSpots(GardenMap map, System.Collections.Generic.List<PlacedThing> things)
        {
            int[] made = world.Choices;
            int moving = -1;
            if (mapInHand >= 0 && mapInHand < made.Length) moving = mapInHand;
            else if (heldPlanting >= 0) moving = made.Length + heldPlanting;
            else if (heldBought >= 0) moving = made.Length + world.PlantingCount + heldBought;
            else if (basketOpen && basketKind >= 0 && basketKind < made.Length)
            {
                things.Add(new PlacedThing(-1, (VillageChoice)Mathf.Clamp(made[basketKind], 0, 2), ItemAt(basketKind, made[basketKind]).Art));
                moving = things.Count - 1;
            }
            else if (nurseryOpen && nurseryKind >= 0 && nurseryKind < Nursery.Kinds)
            {
                things.Add(new PlacedThing(-1, Nursery.Family(nurseryKind), Nursery.Item(nurseryKind).Art));
                moving = things.Count - 1;
            }
            int open = Pairings.Open(world); // I380: deeper pairings glow only once open
            if (moving < 0 || moving >= things.Count || (open & ~world.PairingsFound) == 0) return null;
            var taken = new System.Collections.Generic.HashSet<int>();
            foreach (PlacedThing t in things) taken.Add(t.Cell);
            var spots = new System.Collections.Generic.List<int>();
            for (int c = 0; c < map.CellCount && spots.Count < 40; c++)
                if (map.IsLand(c) && !taken.Contains(c) && Pairings.NewAt(map, things, moving, c, world.PairingsFound | ~open) != 0)
                    spots.Add(c);
            return spots.ToArray();
        }

        // I368: good neighbours found in the garden as it stands: each pays once, with a word.
        // (Also carries I378's word about the bees: whatever the hub should say once, on return.)
        private string pairingFound;
        private Sprite[] toastMarks; // I387: the paintings the toast shows above its words

        // I388-I391: once a season, what the season gives: spring seedlings from a flower bed,
        // summer seeds from a grove, an autumn orchard's fruit, winter jam from neighbours fed.
        private void NoteSeasonYield(GardenMap map)
        {
            if (world == null || world.Mode != PlayMode.Living || map == null) return;
            if (pairingFound != null) return; // one word per return: the season's gift waits for the next
            int last = world.LastFruitTo;
            SeasonYield got = world.TakeSeasonYield(Pairings.Found(world, map));
            if (got == SeasonYield.None) return;
            string giver = last == 0 ? "Mira" : last == 1 ? "Orin" : "Fen";
            switch (got)
            {
                case SeasonYield.Seedling:
                    celebrateKeeper = 2; // I431: Fen, for the flowers
                    pairingFound = "Spring: your flower bed has sown itself. A seedling is waiting in your basket.";
                    toastMarks = new[] { MosseloraArt.Load("flower-12"), MosseloraArt.Load("glyph-sprout"), MosseloraArt.Load("food-basket") };
                    break;
                case SeasonYield.Seeds:
                    celebrateKeeper = 0; // I431: Mira, for the grove
                    pairingFound = "Summer: the grove let fall its seeds. +" + SeasonalYields.SummerSeeds + " seeds.";
                    toastMarks = new[] { MosseloraArt.Load("tree-00"), MosseloraArt.Load("tree-05"), MosseloraArt.Load("glyph-sprout") };
                    break;
                case SeasonYield.Fruit:
                    celebrateKeeper = 0; // I431: Mira, for the orchard
                    pairingFound = "Autumn: the orchard is ready. " + SeasonalYields.AutumnFruit + " fruit picked.";
                    toastMarks = new[] { MosseloraArt.Load("tree-05"), MosseloraArt.Load("food-apple"), MosseloraArt.Load("food-pear") };
                    break;
                default:
                    celebrateKeeper = last; // I431: whoever brought the jam
                    pairingFound = "Winter: " + giver + " brought a jar of jam, for the fruit you gave this year.";
                    toastMarks = new[] { MosseloraArt.Load("portrait-" + giver.ToLowerInvariant()), MosseloraArt.Load("food-jam") };
                    break;
            }
            SaveWorld();
            PlayChime();
        }

        // I424: whose line of work a good neighbour is: Mira trees, Orin builds, Fen flowers.
        private int celebrateKeeper = -1;

        private static int KeeperOf(Pairing pairing)
        {
            switch (pairing)
            {
                case Pairing.Grove: case Pairing.Orchard: return 0;
                case Pairing.ByTheWater: case Pairing.ShadySeat: return 1;
                default: return 2;
            }
        }

        // I387: each pairing as two paintings side by side (water as a wet stone).
        private static Sprite[] PairingMarks(Pairing pairing)
        {
            string[] arts;
            switch (pairing)
            {
                case Pairing.Sheltered: arts = new[] { "flower-05", "tree-00" }; break;
                case Pairing.ByTheWater: arts = new[] { "build-00", "stone-wet" }; break;
                case Pairing.FlowerBed: arts = new[] { "flower-05", "flower-12" }; break;
                case Pairing.Grove: arts = new[] { "tree-00", "tree-05" }; break;
                case Pairing.Waterside: arts = new[] { "flower-08", "stone-wet" }; break;
                case Pairing.ShadySeat: arts = new[] { "build-03", "tree-00" }; break;
                default: arts = new[] { "tree-00", "tree-05", "tree-02" }; break;
            }
            var marks = new Sprite[arts.Length];
            for (int i = 0; i < arts.Length; i++) marks[i] = MosseloraArt.Load(arts[i]);
            return marks;
        }

        // I378: a tended tree with a flower beside it, while the bees are about, gives one more fruit.
        // I404 follow-up: only when this solve itself picked fruit, so the bees' extra fruit
        // follows whatever limit governs a tree's own fruit (a replay cap included).
        private void NotePollinated(int thingIndex, bool pickedFruit)
        {
            if (!pickedFruit) return;
            GardenMap map = LoadGardenMap();
            if (map == null) return;
            var things = Pairings.Things(world, map);
            if (world.PickPollinatedFruit(Pairings.Pollinated(map, things, thingIndex), GardenSeason.For(world)))
            {
                pairingFound = "The bees carried pollen from the flower beside it: one more fruit.";
                toastMarks = new[] { MosseloraArt.Load("flower-05"), MosseloraArt.Load("food-pear") };
            }
        }

        private void NotePairings(GardenMap map)
        {
            if (world == null || world.Mode != PlayMode.Living || map == null) return;
            int found = Pairings.Found(world, map);
            foreach (Pairing pairing in Enum.GetValues(typeof(Pairing)))
                if ((found & PairingMask.Of(pairing)) != 0 && world.DiscoverPairing(pairing) > 0)
                {
                    pairingFound = "Good neighbours: " + Pairings.Hint(pairing).ToLowerInvariant().TrimEnd('.') +
                        ". +" + Pairings.RewardFor(pairing) + " seeds.";
                    toastMarks = PairingMarks(pairing);
                    celebrateKeeper = KeeperOf(pairing); // I424
                    SaveWorld();
                    if (soundOn && sound != null) sound.PlayOneShot(GardenSounds.Neighbours(), 0.9f); // I406
                    else PlayChime();
                }
        }
    }
}
