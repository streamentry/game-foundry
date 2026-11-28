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

        private bool GardenMapOnHub(RectTransform window)
        {
            NotePlayedToday();
            NoteReturn();
            NoteSeasonTurn();
            NoteGrantedWishes();
            NoteNeighbourGifts();
            ApplyDayLight(world);
            Sprite plot = MapPainting();
            GardenMap map = LoadGardenMap();
            NotePairings(map); // I368
            NoteSeasonYield(map); // I388-I391
            if (map != null && world.Mode == PlayMode.Living)
            {
                var things = Pairings.Things(world, map);
                GardenMapView.PairLinksNext = Pairings.Links(map, things); // I371
                GardenMapView.PairSpotsNext = PairingSpots(map, things);    // I372
                SetVisitorsFor(world, map, things); // I374, I382
                // I430: a lantern by the shady seat, lit from dusk.
                GardenMapView.SeatLanternNext = world.HasPairing(Pairing.ShadySeat) ? Pairings.SpotFor(map, things, Pairing.ShadySeat) : -1;
                owlInGrove = Pairings.SpotFor(map, things, Pairing.Grove) >= 0; // I376: heard only while a grove stands
            }
            if (plot == null || map == null) return false;
            float zoom = hubZoom[landscape ? 1 : 0];
            // Only home marks things that hold a puzzle: only home plays them (I221).
            markPuzzles = world.Mode == PlayMode.Living;
            var hubItems = MapItems(map);
            markPuzzles = false;
            GardenMapView.LanternsNext = world.Lanterns.Length;
            // I363: wild flowers on the banks, two for each stream restored, never on a thing.
            int restored = world.Completed ? PuzzleCatalog.Count : world.Round;
            var taken = new System.Collections.Generic.List<int>(map.Layout(world.Choices, world.Placements, world.Occupied));
            taken.AddRange(world.Occupied);
            // Checklist 46: flowers you planted spread along the river by days played (WildGrowth), drawn as the same tufts.
            var tufts = new System.Collections.Generic.List<int>(MeadowLife.Cells(map, MeadowLife.Count(restored), taken.ToArray()));
            foreach (int wild in WildGrowth.Spread(map, world, world.DaysPlayed))
                if (!tufts.Contains(wild)) tufts.Add(wild);
            GardenMapView.MeadowTufts = tufts.ToArray();
            GardenMapView view = GardenMapView.Build(window, plot, map, hubItems, zoom > 0f ? zoom : landscape ? 1f : 1.3f, OnMapLand, OnMapItem,
                heldDecor >= 0 ? DecorIndex + heldDecor : heldBought >= 0 ? BoughtIndex + heldBought : heldPlanting >= 0 ? PlantingIndex + heldPlanting : mapInHand,
                (justPlanted >= 0 || justBought >= 0) && gentleMotion,
                GreenedShare(0), -1f, (land, point) =>
                {
                    // Neighbours stand on the map; the held thing's Play and Move tokens sit beside it
                    // (checklist 13). The tokens are not returned with the neighbours: the map treats what
                    // it is given as people (footprints in snow, stepping off signs), so they manage themselves.
                    var standing = NeighborsOnMap(land, point, map, true);
                    ThingActionsOnMap(land, point); // after the neighbours, so the tokens draw over (and take taps before) them
                    return standing;
                },
                0.22f, RiverGoesOn(map), NextSeasonPlot());
            hubMapView = view;
            celebrateKeeper = -1; // I424: once
            if (pairingFound != null && titleScreen != null)
            {
                RuleToast.Show((RectTransform)titleScreen.transform, pairingFound, font, Ink, MosseloraTheme.CardFill, toastMarks);
                pairingFound = null;
                toastMarks = null;
            }
            // I310: lanterns are passed in before the build (LanternsNext), so nothing lays out twice.
            waveHello = false; // they waved (or would have) on this return; not on every rebuild
            // A solve opened more river (I223): its mist lifts, framed, with the chime.
            if (riverOpened && map.Reaches > 1)
            {
                if (gentleMotion) view.LiftMist(map.Reaches - 1);
                PlayChime();
            }
            riverOpened = false;
            if (fruitPicked && purseFruit != null)
            {
                if (gentleMotion) StartCoroutine(FlyInto(purseFruit.rectTransform, "food-pear", new Vector2(54f, 54f)));
                else BloomPulse.Add(purseFruit.gameObject, 0.1f, 0.22f);
            }
            fruitPicked = false;
            RiverDrops(view, dropEarned && gentleMotion);
            dropEarned = false;
            justBought = -1;
            justSolved = -1;
            // Pinch to zoom (I202), kept while the app runs, per orientation.
            bool sideways = landscape;
            view.Zoomed = z => hubZoom[sideways ? 1 : 0] = z;
            justPlanted = -1;
            if (keepHubAt.x >= 0f) view.KeepAt(keepHubAt);
            keepHubAt = new Vector2(-1f, -1f);
            // Full-screen home (I190): what you made sits in the upper part of the
            // screen, clear of the play token at the bottom.
            // Sideways too (I197): home is the whole garden whichever way you hold it.
            // Sideways the window is short, so what you made sits higher still (I197).
            view.FocusFromTop = landscape ? 0.24f : 0.36f;
            // The play row and the basket live in the bottom of home: neighbours keep out of it (I218).
            // Sideways the menu button sits at the bottom-left over the garden, so the map keeps clear of it (checklist 94).
            view.ClearBottom = landscape ? MenuInset + MenuSize + 12f : 190f;
            // Neighbours keep below the corner (the letterbox stood under the purse until checklist 11; the clearance stays).
            view.ClearTop = landscape ? 0f : world.Letters.Length > 0 ? 220f : 140f; // the box's foot is ~195 pt down
            return true;
        }

        // I189: the garden is the stage. Boards float over your own garden map,
        // full screen and gently veiled so the board stays the brightest thing,
        // instead of a paper page with a strip of garden under the board.
        // The map wears the season the walk has reached (I198): summer, then autumn
        // amber, then winter frost; a finished garden is summer again.
        private Sprite MapPainting()
        {
            // Four seasons by progress (I248/I249): the story walks spring to winter; after it,
            // tending turns the year on.
            Sprite graded = MosseloraArt.Load(PaintedVillage.MapFor(PaintedVillage.ArtSeason(world)));
            return graded != null ? graded : MosseloraArt.Load("garden-map");
        }

        private bool WorldStage(bool show)
        {
            if (worldStage == null) return false;
            ClearChildren(worldStage);
            worldStage.gameObject.SetActive(false);
            if (!show) return false;
            Sprite plot = MapPainting();
            GardenMap map = LoadGardenMap();
            if (plot == null || map == null) return false;
            worldStage.gameObject.SetActive(true);
            GardenMapView view = GardenMapView.Build(worldStage, plot, map, MapItems(map), 1.1f,
                null, null, -1, false, GreenedShare(0), -1f, null, 0f, RiverGoesOn(map));
            view.FocusFromTop = 0.82f; // your newest thing peeks out under the board
            var veil = Node("Morning veil", worldStage);
            Fill(veil.GetComponent<RectTransform>());
            var veilImage = veil.AddComponent<Image>();
            veilImage.color = new Color(Paper.r, Paper.g, Paper.b, 0.2f);
            veilImage.raycastTarget = false;
            return true;
        }

        // ---- E4 (I178): place what you made. Tap a thing to pick it up, tap land to put it down.
        private int mapInHand = -1;
        // ---- I193: the basket. Plant more of anything you've grown, as often as you like.
        private const int PlantingIndex = 1000; // map indices at or above this are plantings
        private bool basketOpen;
        private int basketKind = -1;   // the choice whose item is planted on a tap, -1 none
        private int heldPlanting = -1; // a planting picked up to move or put back
        // ---- I208: the nursery. Seeds buy new plants; each holds its own puzzle.
        private const int BoughtIndex = 2000; // map indices at or above this are bought plants
        private const int SecretIndex = 3000; // I320: map indices at or above this are mossy stones, by stretch
        private const int DecorIndex = 4000;  // I330: map indices at or above this are decorations
        private const int WellIndex = 5000;   // I333: the old well at the river's head
        private int decorInHand = -1;          // a Decoration chosen at the cottage, to place
        private int heldDecor = -1;            // a placed decoration picked up, to move or put away

        private static string DecorArt(Decoration piece)
        {
            return piece == Decoration.Stones ? "build-02" : piece == Decoration.Bench ? "build-17"
                : piece == Decoration.Fence ? "build-19" : "build-22";
        }

        private static string DecorName(Decoration piece)
        {
            return piece == Decoration.Stones ? "stepping stones" : piece == Decoration.Bench ? "a bench"
                : piece == Decoration.Fence ? "a fence" : "a lantern post";
        }
        private int heldBought = -1;   // a bought plant picked up (to move, or to play its puzzle)
        private bool nurseryOpen;      // the basket tray shows the nursery instead of your own kinds
        private int nurseryKind = -1;  // the nursery kind planted (bought) on a tap, -1 none
        private int justBought = -1;
        private int justSolved = -1;   // map index of the thing whose puzzle was just solved (I212)
        private bool solvedHere;
        private bool riverOpened;   // the last solve opened a new stretch of river (I223)
        private TreePlay openCard;  // the puzzle card now open (I264: was it played together?)
        private PuzzleKind togetherKind;
        private int togetherPlayers = 2; // how many took turns on it (2 until a group says otherwise)
        private int togetherKeeper = -1; // I266: the neighbour who'll say you played it together
        private bool dropEarned;    // the last solve was a first solve: a drop on the mist fills (I224)
        private bool fruitPicked;   // the last solve picked fruit from a tree (I229)
        private Image purseFruit;   // the pear in the purse, where picked fruit flies (I229)

        // Drops on the next stretch's mist (I224): how close more river is.
        private void RiverDrops(GardenMapView view, bool swell)
        {
            if (view == null || world.Mode != PlayMode.Living) return;
            if (world.TowardNextStretch(out int have, out int need))
                view.ShowDrops(have, need, swell);
        }
        private int OpenReaches() { return GardenMap.ReachesFor(world.Choices.Length, world.Completed, world.PuzzlesSolved, world.RiverReplays); }

        // Everything standing on the map besides your story things: plantings, then bought plants.
        private int[] Occupied()
        {
            return world.Occupied; // plantings, bought plants and, since I330, decorations
        }
        private int justPlanted = -1;  // rises from the ground once
        private int justDecorated = -1; // I330
        private GardenMapView hubMapView;
        private Vector2 keepHubAt = new Vector2(-1f, -1f);

        private bool PlantedAt(int cell, int except = -1, int exceptBought = -1)
        {
            int[] planted = world.Plantings;
            for (int k = 0; k * 2 + 1 < planted.Length; k++)
                if (k != except && planted[k * 2 + 1] == cell) return true;
            int[] bought = world.Bought;
            for (int k = 0; k * 2 + 1 < bought.Length; k++)
                if (k != exceptBought && bought[k * 2 + 1] == cell) return true;
            return false;
        }

        private System.Collections.Generic.List<GardenMapView.Placed> MapItems(GardenMap map)
        {
            int[] made = world.Choices;
            int[] planted = world.Plantings;
            int[] cells = map.Layout(made, world.Placements, Occupied());
            var items = new System.Collections.Generic.List<GardenMapView.Placed>();
            for (int i = 0; i < made.Length; i++)
                items.Add(new GardenMapView.Placed
                {
                    Art = ItemArt(i, made[i]),
                    Cell = cells[i],
                    Index = i,
                    Family = (VillageChoice)Mathf.Clamp(made[i], 0, 2),
                    Newest = justSolved >= 0 ? i == justSolved : i == made.Length - 1 && justPlanted < 0,
                    Bloomed = world.TreeSolved(i) || world.IsHelped(i), // solved (I204), or by family (I227)
                    HoldsPuzzle = markPuzzles && !world.TreeSolved(i), // helped still holds your own first solve
                    Kind = (int)ThingPuzzle.ForStory(i, (VillageChoice)Mathf.Clamp(made[i], 0, 2)) + 1, Watered = ChainCause.WateredNow(world.Garden), // I255
                    Together = world.SolvedTogether(i), // I265
                    FlowerIndex = made[i] == (int)VillageChoice.Flowers ? ItemAt(i, made[i]).Index + 1 : 0, // I338
                    Crowned = world.TreeSolved(i) && ReplayBoards.Crowned(TreePuzzles.TierFor(i), Replays(i)), // I292
                    Grown = world.TreeSolved(i) ? ReplayBoards.Maturity(TreePuzzles.TierFor(i), Replays(i)) : 1f, // I300
                    BloomNow = i == justSolved && gentleMotion
                });
            // More of what you grew, planted from the basket (I193).
            for (int k = 0; k * 2 + 1 < planted.Length; k++)
            {
                int source = planted[k * 2];
                if (source < 0 || source >= made.Length || !map.IsLand(planted[k * 2 + 1])) continue;
                items.Add(new GardenMapView.Placed
                {
                    Art = ItemArt(source, made[source]),
                    Cell = planted[k * 2 + 1],
                    Index = PlantingIndex + k,
                    Family = (VillageChoice)Mathf.Clamp(made[source], 0, 2),
                    Newest = k == justPlanted
                });
            }
            // Plants from the nursery (I208): each holds its own puzzle.
            int[] bought = world.Bought;
            for (int k = 0; k * 2 + 1 < bought.Length; k++)
            {
                int kind = bought[k * 2];
                if (kind < 0 || kind >= Nursery.Kinds || !map.IsLand(bought[k * 2 + 1])) continue;
                items.Add(new GardenMapView.Placed
                {
                    Art = MosseloraArt.Load(Nursery.Item(kind).Art),
                    Cell = bought[k * 2 + 1],
                    Index = BoughtIndex + k,
                    Family = Nursery.Family(kind),
                    Newest = justSolved >= 0 ? BoughtIndex + k == justSolved : k == justBought,
                    Bloomed = world.BoughtSolved(k),
                    HoldsPuzzle = markPuzzles && !world.BoughtSolved(k),
                    Kind = (int)ThingPuzzle.ForNursery(kind) + 1, Watered = ChainCause.WateredNow(world.Garden), // I255
                    Together = world.SolvedTogether(BoughtIndex + k), // I265
                    FlowerIndex = Nursery.Family(kind) == VillageChoice.Flowers ? Nursery.Item(kind).Index + 1 : 0, // I338
                    Crowned = world.BoughtSolved(k) && ReplayBoards.Crowned(Nursery.Row(kind), Replays(BoughtIndex + k)), // I292
                    Grown = world.BoughtSolved(k) ? ReplayBoards.Maturity(Nursery.Row(kind), Replays(BoughtIndex + k)) : 1f, // I300
                    BloomNow = BoughtIndex + k == justSolved && gentleMotion
                });
            }
            // I330: decorations, free to place and move; drawn as the catalogue's own paintings.
            int[] decor = world.Decor;
            for (int d = 0; d * 2 + 1 < decor.Length; d++)
            {
                if (!map.IsLand(decor[d * 2 + 1])) continue;
                items.Add(new GardenMapView.Placed { Art = MosseloraArt.Load(DecorArt((Decoration)decor[d * 2])),
                    Cell = decor[d * 2 + 1], Index = DecorIndex + d, Family = VillageChoice.Bridge,
                    Newest = d == justDecorated });
            }
            // I333: the old well at the head of the river, first free land at the top left.
            if (markPuzzles)
            {
                var wellTaken = new System.Collections.Generic.List<int>(cells);
                for (int k = 1; k < planted.Length; k += 2) wellTaken.Add(planted[k]);
                for (int k = 1; k < bought.Length; k += 2) wellTaken.Add(bought[k]);
                for (int k = 1; k < decor.Length; k += 2) wellTaken.Add(decor[k]);
                int wellCell = -1;
                for (int row = 1; row < 4 && wellCell < 0; row++)
                    for (int col = 0; col < 3 && wellCell < 0; col++)
                    {
                        int c = row * map.Columns + col;
                        if (map.IsLand(c) && !wellTaken.Contains(c)) wellCell = c;
                    }
                if (wellCell >= 0)
                    items.Add(new GardenMapView.Placed { Art = MosseloraArt.Load("build-05"), Cell = wellCell, Index = WellIndex,
                        Family = VillageChoice.Bridge, HoldsPuzzle = OldWell.Waiting(world) > 0,
                        Kind = (int)OldWell.KindFor(world.WellSolved) + 1 });
            }
            // I320: one mossy stone on the bank of each stretch after the first, until found.
            if (markPuzzles)
            {
                var taken = new System.Collections.Generic.List<int>(cells);
                for (int k = 1; k < planted.Length; k += 2) taken.Add(planted[k]);
                for (int k = 1; k < bought.Length; k += 2) taken.Add(bought[k]);
                Sprite moss = MosseloraArt.Load("stone-spring");
                for (int reach = 1; reach < map.Reaches && moss != null; reach++)
                {
                    int cell = RiverSecrets.CellFor(map, reach, taken.ToArray());
                    if (cell < 0 || SecretFound(reach)) continue;
                    items.Add(new GardenMapView.Placed { Art = moss, Cell = cell, Index = SecretIndex + reach,
                        Family = VillageChoice.Bridge });
                }
            }
            return items;
        }

        // I333: the well's next deepest board; two fruit when solved, once.
        private void OpenWell()
        {
            if (titleScreen == null) return;
            if (OldWell.Waiting(world) <= 0)
            {
                Transform hub = titleScreen.transform;
                RuleToast.Show((RectTransform)hub, "The well is still. New river will fill it again.", font, Ink, MosseloraTheme.CardFill);
                return;
            }
            int index = world.WellSolved;
            OpenThingPuzzle(OldWell.SeedFor(world.Garden, index), OldWell.Tier, OldWell.KindFor(index),
                MosseloraArt.Load("build-05"), "the old well", 0, index, () =>
                {
                    int fruitBefore = world.Fruit;
                    if (world.TendWell() > 0) { SaveWorld(); PlayChime(); fruitPicked = world.Fruit > fruitBefore; }
                    return 0;
                });
            if (openCard != null) openCard.PaysFruit(OldWell.FruitPerBoard);
        }

        private string StoneKey(int reach) { return "evergrove:mossy-stone:" + world.Garden + ":" + reach; }
        private bool SecretFound(int reach) { return PlayerPrefs.GetInt(StoneKey(reach), 0) == 1; }

        // I320: lift the mossy stone: a lost cutting goes to the basket, with a note from Fen.
        private void FindSecret(int reach)
        {
            if (SecretFound(reach) || world.Mode != PlayMode.Living) return;
            int kind = RiverSecrets.CuttingFor(world, reach);
            if (kind < 0 || !world.ReceiveNeighbourCutting(kind))
            {
                if (titleScreen != null)
                    RuleToast.Show((RectTransform)titleScreen.transform, "Your basket is full. Plant something first; the stone will wait.",
                        font, Ink, MosseloraTheme.CardFill);
                return;
            }
            PlayerPrefs.SetInt(StoneKey(reach), 1);
            PlayerPrefs.Save();
            string what = Nursery.Family(kind) == VillageChoice.Bridge ? "a little " + Nursery.Item(kind).Name
                : "a cutting of " + Nursery.Item(kind).Name;
            world.KeepNote("Fen", "You found what was under the mossy stone: " + what +
                ", lost there long ago. It's in your basket now.");
            letterArrived = true;
            SaveWorld();
            PlayChime();
            RefreshMaps();
        }

        // Mira, Orin and Fen stand in the garden (I181): each beside the newest thing
        // of their family (trees, builds, flowers), or by the cottage before that.
        // ---- I217: letters from family stay in your garden: a small letterbox on the
        // lane below the cottage. A tap reads them, newest first, one at a time.
        private const string LettersReadKey = "evergrove:letters-read";
    }
}
