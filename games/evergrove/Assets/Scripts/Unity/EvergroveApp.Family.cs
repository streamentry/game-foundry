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

        private const string FamilyNameKey = "evergrove:family-name";

        // Checklist 8: family features are an offer from day 3 (or at once with family activity), never a gate.
        private bool FamilyShown()
        {
            return world != null && FamilyOffer.Visible(world.DaysPlayed, FamilyOffer.HasActivity(world));
        }

        // The one-time hub offer: a gentle line, shown once on this device when family first becomes visible.
        private void OfferFamilyOnce()
        {
            if (titleScreen == null || !FamilyShown()) return;
            if (PlayerPrefs.GetInt(FamilyOffer.OfferedKey, 0) == 1) return;
            PlayerPrefs.SetInt(FamilyOffer.OfferedKey, 1);
            PlayerPrefs.Save();
            RuleToast.Show((RectTransform)titleScreen.transform, FamilyOffer.Line, font, Ink, MosseloraTheme.CardFill);
        }

        private void OpenFamily()
        {
            if (familySheet != null) return;
            if (!FamilyShown()) return; // checklist 8: no family sheet for a fresh solo garden
            RectTransform host = titleScreen != null ? (RectTransform)titleScreen.transform : canvasRoot;
            familySheet = FamilySheet.Open(host, font, new FamilySheet.Hooks
            {
                Seeds = () => world.Seeds,
                Fruit = () => world.Fruit,
                Jam = () => world.Jam,
                RowOpen = row => world.RowOpen(row),
                OwnedKinds = () => {
                    int[] bought = world.Bought;
                    var kinds = new System.Collections.Generic.List<int>();
                    for (int k = bought.Length / 2 - 1; k >= 0; k--)
                        if (!kinds.Contains(bought[k * 2])) kinds.Add(bought[k * 2]);
                    return kinds.ToArray();
                },
                Send = (from, words, gift, value, nonce, wish, picture) => {
                    ParcelSendResult sent = world.SendParcel(from, words, gift, value, nonce, wish, 0, picture);
                    if (sent.Ok) { SaveWorld(); PlayChime(); }
                    return sent;
                },
                Open = text => {
                    ParcelOpenResult opened = world.Open(text);
                    if (opened.Ok && opened.Contents != null && opened.Contents.Letter.Length > 0) letterArrived = true;
                    if (opened.Ok) { SaveWorld(); PlayChime(); }
                    return opened;
                },
                Share = text => { if (!NativeShare.Text(text, "Mosselora")) GUIUtility.systemCopyBuffer = text; },
                Paste = () => GUIUtility.systemCopyBuffer,
                Postcard = () => {
                    string text = GardenPostcard.Build(world,
                        world.Completed ? "" : GameLocalization.Text("stream", world.Round + 1, PuzzleCatalog.Count));
                    if (!NativeShare.Text(text, GameLocalization.Text("postcard"))) GUIUtility.systemCopyBuffer = text;
                },
                Visit = text => {
                    if (!OpenVisit(text)) return false;
                    DismissFamily();
                    return true;
                },
                Closed = () => { familySheet = null; RefreshMaps(); },
                ReducedMotion = () => !gentleMotion,
                Name = () => PlayerPrefs.GetString(FamilyNameKey, ""),
                SaveName = name => { PlayerPrefs.SetString(FamilyNameKey, name); PlayerPrefs.Save(); },
                PlayBoard = (kind, tier, seed, from) => {
                    DismissFamily();
                    PlayTheirBoard(kind, tier, seed, from);
                },
            });
        }

        // Puts the family sheet away from its edge (at once when a board or a visit takes its place).
        private void DismissFamily()
        {
            if (familySheet == null) return;
            familySheet.Dismiss();
            familySheet = null;
        }

        // ---- I309: the very board a relative solved, played here. It pays nothing and
        // changes nothing in your garden; it's simply the same puzzle, shared.
        private void PlayTheirBoard(int kind, int tier, int seed, string from)
        {
            if (titleScreen == null || kind < 0 || kind > (int)PuzzleKind.Doorstep) return;
            OpenThingPuzzle(seed, Mathf.Clamp(tier, 0, TreePuzzles.Tiers - 1), (PuzzleKind)kind,
                MosseloraArt.Load("letter-sealed"), from + "'s board", 0, Mathf.Abs(seed % 97), () => 0);
        }

        private Sprite FlourishArt(PuzzleKind kind, out Color tint)
        {
            tint = Color.white;
            switch (kind)
            {
                case PuzzleKind.Water: case PuzzleKind.StreamPath:
                    return MosseloraArt.Load(MosseloraArt.Droplet);
                case PuzzleKind.Pairs:
                    Sprite[] bloom = PairsPictures(1, 0, (c, f) => ItemArt(c, f));
                    return bloom.Length > 0 ? bloom[0] : MosseloraArt.Load(MosseloraArt.Sprig);
                case PuzzleKind.Mosaic:
                    tint = new Color(1f, 0.86f, 0.45f, 1f);
                    return MosseloraArt.Rounded(6);
                case PuzzleKind.Mahjong:
                    return MosseloraArt.Load("stone-spring");
                case PuzzleKind.Plots:
                    return MosseloraArt.Load(MosseloraArt.Sprig);
                default:
                    return MosseloraArt.Load(TreePlay.SeedIcon);
            }
        }

        private void SendBoardToFamily(PuzzleKind kind, int tier, int seed, string thing)
        {
            string from = PlayerPrefs.GetString(FamilyNameKey, "");
            if (!FamilyParcel.ValidName(from)) { OpenFamily(); return; } // your name first, on the family page
            int shared = ShareSeedFor(kind, seed, tier); // an own nonogram sends the standard seed with the same clues
            if (shared < 0) return;
            string nonce = Guid.NewGuid().ToString("N").Substring(0, 16);
            ParcelSendResult sent = world.SendBoard(from, "", (int)kind, tier, shared, nonce);
            if (!sent.Ok) return;
            SaveWorld();
            string text = FamilyParcel.Wrap(sent.Text, from + " sent you the " + thing +
                " puzzle they solved in Mosselora. Open it in the game: Family, then Open.");
            if (!NativeShare.Text(text, "Mosselora")) GUIUtility.systemCopyBuffer = text;
        }

        // ---- I216: visit a relative's garden, read-only. Their garden file (the one
        // Settings copies) is read without touching yours; their river, things,
        // plantings and nursery plants stand on their own painted map.
        private GameObject visitLayer;
        private string visitText;
        private WorldModel visitWorld;
        private readonly System.Collections.Generic.List<int> helpedOnVisit = new System.Collections.Generic.List<int>();

        private bool OpenVisit(string text)
        {
            if (!GardenTransfer.TryImport(text, out WorldModel visited, out GardenImportError _) ||
                visited.Mode != PlayMode.Living) return false;
            var mask = Resources.Load<TextAsset>("Evergrove/garden-map-mask");
            if (mask == null) return false;
            GardenMap map;
            try { map = GardenMap.River(10, 15, mask.text.Trim(), GardenMap.ReachesFor(visited.Choices.Length, visited.Completed, visited.PuzzlesSolved, visited.RiverReplays)); }
            catch (ArgumentException) { return false; }
            ApplyDayLight(visited); // your hour, their season
            // Their garden in its own season (I249): families see each other's year.
            int season = PaintedVillage.ArtSeason(visited);
            Sprite plot = MosseloraArt.Load(PaintedVillage.MapFor(season)) ?? MosseloraArt.Load("garden-map");
            if (plot == null) return false;

            if (text != visitText) helpedOnVisit.Clear();
            visitText = text;
            visitWorld = visited;
            visitLayer = Node("Visiting a garden", canvasRoot);
            Fill(visitLayer.GetComponent<RectTransform>());
            visitLayer.AddComponent<Image>().color = Paper;
            visitLayer.transform.SetAsLastSibling();
            var window = Node("Their garden", visitLayer.transform);
            Fill(window.GetComponent<RectTransform>());
            window.AddComponent<RectMask2D>();
            SetVisitorsFor(visited, map, Pairings.Things(visited, map)); // I384: their visitors too
            GardenMapView.PairLinksNext = Pairings.Links(map, Pairings.Things(visited, map)); // and their sprigs
            // Help (I227): tap one of their things that still holds a puzzle, solve it for them.
            GardenMapView view = GardenMapView.Build((RectTransform)window.transform, plot, map, VisitItems(visited, map),
                landscape ? 1f : 1.3f, null, HelpWith, -1, false, 1f, -1f, null, 0.22f, RiverGoesOn(map));
            view.FocusFromTop = 0.4f;

            // A wooden "back home" token where the menu button lives, and a sign.
            var bar = Node("Visit chrome", visitLayer.transform);
            var barRect = bar.GetComponent<RectTransform>();
            Rect safe = Screen.safeArea;
            float w = Mathf.Max(1, Screen.width), h = Mathf.Max(1, Screen.height);
            barRect.anchorMin = new Vector2(safe.xMin / w, safe.yMax / h);
            barRect.anchorMax = new Vector2(safe.xMax / w, safe.yMax / h);
            barRect.pivot = new Vector2(0.5f, 1f);
            barRect.sizeDelta = new Vector2(0f, 70f);
            Button home = IconButton(bar.transform, "Back to your garden", "Back to your garden", MosseloraArt.IconHome,
                CloseVisit, 54, MosseloraTheme.QuietAction);
            PlaceChrome(home, new Vector2(0f, 1f), new Vector2(10f, ChromeTop));
            var sign = Node("Visiting sign", bar.transform);
            var signRect = sign.GetComponent<RectTransform>();
            signRect.anchorMin = signRect.anchorMax = new Vector2(0f, 1f);
            signRect.pivot = new Vector2(0f, 1f);
            signRect.sizeDelta = new Vector2(Mathf.Round(220f * Mathf.Max(1f, TextScale.Percent / 100f)), 44f);
            signRect.anchoredPosition = new Vector2(78f, -ChromeTop - 4f); // right of the home token
            var signImage = sign.AddComponent<Image>();
            MosseloraArt.Round(signImage, new Color(0.98f, 0.95f, 0.86f, 0.96f), 22);
            MosseloraArt.Lift(signImage, 2f, 0.2f);
            Text words = Label(sign.transform, "Visiting words", "Visiting a garden", 18, 0, Ink, TextAnchor.MiddleCenter);
            words.GetComponent<LayoutElement>().ignoreLayout = true;
            Fill(words.rectTransform);
            words.rectTransform.offsetMin = new Vector2(12f, 4f);
            words.rectTransform.offsetMax = new Vector2(-12f, -4f);
            Emphasize(words);
            words.horizontalOverflow = HorizontalWrapMode.Overflow; // one line; the sign grows with text size
            // I334 (idea 40, omiyage): take a keepsake home: a cutting of their newest nursery plant,
            // copied, never taken from them, at its nursery price. Once a visit.
            int[] theirs = visited.Bought;
            if (theirs.Length >= 2 && keepsakeTakenFrom != text && world.Mode == PlayMode.Living)
            {
                int keepKind = theirs[theirs.Length - 2];
                if (keepKind >= 0 && keepKind < Nursery.Kinds)
                {
                    bool afford = world.Seeds >= Nursery.Price(keepKind);
                    Button keep = IconButton(bar.transform, "Take a keepsake cutting home, " + Nursery.Price(keepKind) + " seeds",
                        "Keepsake", Nursery.Item(keepKind).Art, () => TakeKeepsake(keepKind, text), 54, MosseloraTheme.QuietAction);
                    PlaceChrome(keep, new Vector2(1f, 1f), new Vector2(-10f, ChromeTop));
                    Transform glyph = keep.transform.Find("Icon");
                    if (glyph != null) glyph.GetComponent<Image>().color = afford ? Color.white : new Color(1f, 1f, 1f, 0.4f);
                }
            }
            VisitGiftIdea(visited, map); // I398, I399
            semanticsDirty = true;
            return true;
        }

        // I398: on arrival, a gift idea: the one thing that would give their garden a good
        // neighbour it hasn't found. I399: and what your two gardens both have.
        private void VisitGiftIdea(WorldModel visited, GardenMap map)
        {
            if (visitLayer == null || world == null || world.Mode != PlayMode.Living) return;
            var things = Pairings.Things(visited, map);
            string shared = SharedWords(Pairings.Shared(world, visited));
            FamilySheet.IdeaFamily = -1;
            if (Pairings.GiftIdea(map, things, visited.PairingsFound, Pairings.Open(visited), out VillageChoice kind, out Pairing makes))
            {
                FamilySheet.IdeaFamily = (int)kind; // I408: the send sheet remembers it
                // I418 (P25): the rule itself, and its two paintings, said once.
                RuleToast.Show((RectTransform)visitLayer.transform,
                    "A gift idea for their garden: " + Pairings.Hint(makes).TrimEnd('.').ToLowerInvariant() + "." + shared,
                    font, Ink, MosseloraTheme.CardFill, PairingMarks(makes));
            }
            else if (shared.Length > 0)
                RuleToast.Show((RectTransform)visitLayer.transform, shared.Trim(), font, Ink, MosseloraTheme.CardFill);
        }

        private static string PairingName(Pairing pairing)
        {
            switch (pairing)
            {
                case Pairing.Sheltered: return "a sheltered flower";
                case Pairing.ByTheWater: return "something by the water";
                case Pairing.FlowerBed: return "a flower bed";
                case Pairing.Grove: return "a grove";
                case Pairing.Waterside: return "flowers by the water";
                case Pairing.ShadySeat: return "a shady seat";
                default: return "an orchard";
            }
        }

        private static string SharedWords(int shared)
        {
            var names = new System.Collections.Generic.List<string>();
            foreach (Pairing p in Enum.GetValues(typeof(Pairing)))
                if ((shared & PairingMask.Of(p)) != 0) names.Add(PairingName(p));
            if (names.Count == 0) return "";
            return " You both have " + (names.Count == 1 ? names[0]
                : string.Join(", ", names.GetRange(0, names.Count - 1).ToArray()) + " and " + names[names.Count - 1]) + ".";
        }

        private string keepsakeTakenFrom;

        private void TakeKeepsake(int kind, string visit)
        {
            if (!world.TakeKeepsake(kind))
            {
                if (visitLayer != null)
                    RuleToast.Show((RectTransform)visitLayer.transform, world.Seeds < Nursery.Price(kind)
                        ? "A keepsake cutting costs " + Nursery.Price(kind) + " seeds, like the nursery."
                        : "Your basket is full. Plant something first.", font, Ink, MosseloraTheme.CardFill);
                return;
            }
            keepsakeTakenFrom = visit;
            world.KeepNote("Fen", "You brought home a cutting of " + Nursery.Item(kind).Name +
                " from your family's garden. It's in your basket, a keepsake of the visit.");
            letterArrived = true;
            SaveWorld();
            PlayChime();
            if (visitLayer != null) { Destroy(visitLayer); visitLayer = null; OpenVisit(visit); }
        }

        private System.Collections.Generic.List<GardenMapView.Placed> VisitItems(WorldModel visited, GardenMap map)
        {
            int[] made = visited.Choices, planted = visited.Plantings, bought = visited.Bought;
            int[] cells = map.Layout(made, visited.Placements, visited.Occupied); // decorations too (I330)
            var items = new System.Collections.Generic.List<GardenMapView.Placed>();
            Func<int, int, Sprite> art = (chapter, family) => MosseloraArt.Load(GardenCatalog.For(visited.Garden,
                Mathf.Clamp(chapter, 0, GardenCatalog.ChaptersPerGarden - 1), (VillageChoice)Mathf.Clamp(family, 0, 2)).Art);
            for (int i = 0; i < made.Length; i++)
            {
                bool done = visited.TreeSolved(i) || visited.IsHelped(i) || helpedOnVisit.Contains(i);
                items.Add(new GardenMapView.Placed { Art = art(i, made[i]), Cell = cells[i], Index = i,
                    Family = (VillageChoice)Mathf.Clamp(made[i], 0, 2), Bloomed = done,
                    HoldsPuzzle = !done && world.Mode == PlayMode.Living,
                    Kind = (int)ThingPuzzle.ForStory(i, (VillageChoice)Mathf.Clamp(made[i], 0, 2)) + 1,
                    Together = visited.SolvedTogether(i), // I270: their family moments show when you visit
                    BloomNow = gentleMotion && helpedOnVisit.Count > 0 && helpedOnVisit[helpedOnVisit.Count - 1] == i });
            }
            for (int k = 0; k * 2 + 1 < planted.Length; k++)
                if (planted[k * 2] < made.Length && map.IsLand(planted[k * 2 + 1]))
                    items.Add(new GardenMapView.Placed { Art = art(planted[k * 2], made[planted[k * 2]]),
                        Cell = planted[k * 2 + 1], Index = PlantingIndex + k,
                        Family = (VillageChoice)Mathf.Clamp(made[planted[k * 2]], 0, 2) });
            for (int k = 0; k * 2 + 1 < bought.Length; k++)
                if (map.IsLand(bought[k * 2 + 1]))
                    items.Add(new GardenMapView.Placed { Art = MosseloraArt.Load(Nursery.Item(bought[k * 2]).Art),
                        Cell = bought[k * 2 + 1], Index = BoughtIndex + k, Family = Nursery.Family(bought[k * 2]),
                        Bloomed = visited.BoughtSolved(k), Together = visited.SolvedTogether(BoughtIndex + k) });
            if (items.Count > 0)
            {
                // Frame their newest thing, or the one you just helped with, blooming (I227).
                int frame = helpedOnVisit.Count > 0 ? helpedOnVisit[helpedOnVisit.Count - 1] : made.Length > 0 ? made.Length - 1 : 0;
                var newest = items[frame];
                newest.Newest = true;
                items[frame] = newest;
            }
            // I330 (idea 16, after visiting player-made worlds): their decorations stand where they set them.
            int[] theirDecor = visited.Decor;
            for (int d = 0; d * 2 + 1 < theirDecor.Length; d++)
                if (map.IsLand(theirDecor[d * 2 + 1]))
                    items.Add(new GardenMapView.Placed { Art = MosseloraArt.Load(DecorArt((Decoration)theirDecor[d * 2])),
                        Cell = theirDecor[d * 2 + 1], Index = DecorIndex + d, Family = VillageChoice.Bridge });
            return items;
        }

        private void CloseVisit()
        {
            if (visitLayer != null) Destroy(visitLayer);
            visitLayer = null;
            semanticsDirty = true;
        }

        // I227: help with one of their things. You play the very board they would
        // (same seed), earn nothing, and a Help parcel goes to them by any message
        // app; it blooms in their garden when they open it.
        private void HelpWith(int item)
        {
            WorldModel theirs = visitWorld;
            if (theirs == null || visitLayer == null || world.Mode != PlayMode.Living) return;
            int[] made = theirs.Choices;
            if (item < 0 || item >= made.Length || theirs.TreeSolved(item) || theirs.IsHelped(item) ||
                helpedOnVisit.Contains(item)) return;
            int tier = TreePuzzles.TierFor(item);
            GardenItem thing = GardenCatalog.For(theirs.Garden, Mathf.Clamp(item, 0, GardenCatalog.ChaptersPerGarden - 1),
                (VillageChoice)Mathf.Clamp(made[item], 0, 2));
            Func<int, int, Sprite> artOf = (chapter, family) => MosseloraArt.Load(GardenCatalog.For(theirs.Garden,
                Mathf.Clamp(chapter, 0, GardenCatalog.ChaptersPerGarden - 1), (VillageChoice)Mathf.Clamp(family, 0, 2)).Art);
            moveNote = 0;
            PuzzleKind helpKind = ThingPuzzle.ForStory(item, (VillageChoice)made[item]);
            Action turned = () => { PlayMove(helpKind); Haptics.Placed(); };
            Func<int> solved = () =>
            {
                string from = PlayerPrefs.GetString(FamilyNameKey, "");
                if (!FamilyParcel.ValidName(from)) from = "Family";
                string nonce = Guid.NewGuid().ToString("N").Substring(0, 16);
                ParcelSendResult sent = world.SendParcel(from, "", ParcelGift.Help, item, nonce);
                if (!sent.Ok) return 0;
                SaveWorld();
                PlayChime();
                helpedOnVisit.Add(item);
                helpShare = FamilyParcel.Wrap(sent.Text, from + " helped in your Mosselora garden.");
                return 0;
            };
            Action closed = () =>
            {
                string share = helpShare;
                helpShare = null;
                if (visitLayer != null) { Destroy(visitLayer); visitLayer = null; OpenVisit(visitText); }
                if (share != null && !NativeShare.Text(share, "Mosselora")) GUIUtility.systemCopyBuffer = share;
            };
            PuzzleCard((RectTransform)visitLayer.transform, theirs.Garden * 1000 + item * 7 + 1, tier,
                ThingPuzzle.ForStory(item, (VillageChoice)made[item]), artOf(item, made[item]), thing.Name, -1,
                count => PairsPictures(count, item, artOf), turned, solved, closed);
        }
        private string helpShare;
    }
}
