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

        private void GiftTokens(Transform row)
        {
            int[] gifts = world.Gifts;
            for (int i = 0; i < gifts.Length; i++)
            {
                int index = i;
                GardenItem plant = Nursery.Item(gifts[i]);
                Button token = IconButton(row, "Gift " + i, "A gift: " + plant.Name, MosseloraArt.IconRooted, () => {
                        giftPick = giftPick == index ? -1 : index; basketKind = -1; heldPlanting = -1; RefreshMaps();
                    }, 74, index == giftPick ? MosseloraTheme.ForestAction : new Color(0.98f, 0.9f, 0.86f, 1f),
                    false, null, false, 0, 0, true);
                Transform glyph = token.transform.Find("Icon");
                Image glyphImage = glyph != null ? glyph.GetComponent<Image>() : null;
                Sprite art = MosseloraArt.Load(plant.Art);
                if (glyphImage != null && art != null)
                {
                    glyphImage.sprite = art;
                    glyphImage.color = Color.white;
                    var size = glyph.GetComponent<LayoutElement>();
                    size.minWidth = size.preferredWidth = 50f;
                    size.minHeight = size.preferredHeight = 50f;
                }
                Image ribbon = MosseloraArt.Ornament(token.transform, "Ribbon", MosseloraArt.Rounded(8),
                    new Vector2(1f, 1f), new Vector2(0.5f, 0.5f), new Vector2(18f, 18f), new Vector2(-8f, -8f));
                // B5: honey, not red (badge rule: no red on a badge).
                if (ribbon != null) { ribbon.preserveAspect = false; ribbon.color = new Color(0.93f, 0.72f, 0.36f, 1f); }
                if (index == giftPick && gentleMotion) BloomPulse.Add(token.gameObject, 0.9f, 0.06f);
            }
        }

        // The nursery's nine plants in the tray: row by row, each with its price. A
        // closed row is faded with a small lock; a plant you can't afford yet shows
        // its price dimmed. Pick one, then tap the grass to buy and plant it.
        private void NurseryTokens(Transform row)
        {
            // One token per slot, showing what it has in stock now (I240: it restocks as you buy).
            for (int slot = 0; slot < Nursery.Slots; slot++)
            {
                int kind = world.NurseryStock(slot);
                int pick = kind;
                GardenItem plant = Nursery.Item(kind);
                int price = Nursery.Price(kind);
                bool open = world.RowOpen(Nursery.Row(kind));
                bool affordable = open && world.Seeds >= price;
                Button token = IconButton(row, "Nursery " + kind, plant.Name + ", " + price + " seeds. " + PriceReason(Nursery.Row(kind)),
                    MosseloraArt.IconRooted, () => {
                        if (!affordable) return; // gently refused: the faded look already says why
                        nurseryKind = nurseryKind == pick ? -1 : pick;
                        RefreshMaps();
                    }, 74, kind == nurseryKind ? MosseloraTheme.ForestAction : MosseloraTheme.QuietAction,
                    false, null, false, 0, 0, true);
                Transform glyph = token.transform.Find("Icon");
                Image glyphImage = glyph != null ? glyph.GetComponent<Image>() : null;
                Sprite art = MosseloraArt.Load(plant.Art);
                if (glyphImage != null && art != null)
                {
                    glyphImage.sprite = art;
                    glyphImage.color = open ? Color.white : new Color(1f, 1f, 1f, 0.35f);
                    var size = glyph.GetComponent<LayoutElement>();
                    size.minWidth = size.preferredWidth = 50f;
                    size.minHeight = size.preferredHeight = 50f;
                }
                // A closed row still shows its price, greyed: you can see what you save for (I213).
                PriceChip(token.transform, price.ToString(), affordable, open);
                // Not yet in your almanac (I239): a small book on the token's shoulder.
                if (!almanac.Has(plant)) NewToAlmanac(token.transform);
                if (kind == nurseryKind && gentleMotion) BloomPulse.Add(token.gameObject, 0.9f, 0.06f);
            }
        }

        private void NewToAlmanac(Transform token)
        {
            Image disc = MosseloraArt.Ornament(token, "New to your almanac", MosseloraArt.Rounded(13), new Vector2(1f, 1f),
                new Vector2(0.5f, 0.5f), new Vector2(26f, 26f), new Vector2(-8f, -8f));
            if (disc == null) return;
            disc.preserveAspect = false;
            disc.color = new Color(0.98f, 0.95f, 0.86f, 1f);
            Image book = MosseloraArt.Ornament(disc.transform, "Almanac mark", MosseloraArt.Load(MosseloraArt.IconJournal),
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(16f, 16f), Vector2.zero);
            if (book != null) book.color = Leaf;
        }

        // A small chip on a nursery token's lower edge: the price, or a lock when the row is closed.
        private void PriceChip(Transform token, string price, bool affordable, bool open)
        {
            Image chip = MosseloraArt.Ornament(token, "Price", MosseloraArt.Rounded(10), new Vector2(0.5f, 0f),
                new Vector2(0.5f, 0.5f), new Vector2(46f, 22f), new Vector2(0f, 2f));
            if (chip == null) return;
            chip.preserveAspect = false;
            chip.color = affordable ? new Color(0.98f, 0.93f, 0.74f, 1f)
                : open ? new Color(0.9f, 0.88f, 0.82f, 0.95f) : new Color(0.82f, 0.82f, 0.8f, 0.9f);
            Text value = Label(chip.transform, "Price value", price, 14, 0,
                affordable ? Ink : new Color(Ink.r, Ink.g, Ink.b, open ? 0.45f : 0.35f), TextAnchor.MiddleCenter);
            value.horizontalOverflow = HorizontalWrapMode.Overflow;
            value.GetComponent<LayoutElement>().ignoreLayout = true;
            Fill(value.rectTransform);
            Emphasize(value);
        }

        // Redraw home framed on the newest/just-solved thing, not where the view was.
        private void RefreshMapsFresh()
        {
            keepHubAt = new Vector2(-1f, -1f);
            if (titleScreen != null && canvasRoot != null)
            {
                Transform host = titleScreen.transform.parent;
                CloseTitle();
                BuildTitleScreen(host);
            }
            else Rebuild();
        }

        // Redraw whichever map is showing, keeping the page where it is.
        private void RefreshMaps()
        {
            if (titleScreen != null && canvasRoot != null)
            {
                if (hubMapView != null) keepHubAt = hubMapView.Position;
                Transform host = titleScreen.transform.parent;
                CloseTitle();
                BuildTitleScreen(host);
            }
            else Rebuild();
        }

        // The page's garden window (memory card, boards): the same map as the hub.
        private bool heroGarden;

        private bool PageGardenMap(GameObject window, bool memoryScreen)
        {
            ApplyDayLight(world);
            Sprite plot = MapPainting();
            GardenMap map = LoadGardenMap();
            if (plot == null || map == null) return false;
            var size = window.GetComponent<LayoutElement>();
            if (size == null) size = window.AddComponent<LayoutElement>();
            bool hero = heroGarden && memoryScreen;
            size.minHeight = size.preferredHeight = hero ? (TextScale.Percent > 100 ? 360f : 420f)
                : TextScale.Percent > 100 ? 230f : 260f;
            // Every page's garden runs edge to edge (I186); the memory card's also
            // reaches up under the status bar (I185).
            {
                if (hero) window.transform.SetAsFirstSibling();
                var bleed = Node(hero ? "Garden hero" : "Garden band", window.transform);
                var bleedRect = bleed.GetComponent<RectTransform>();
                Fill(bleedRect);
                bleedRect.offsetMin = new Vector2(-17f - columnLeft, 0f);
                bleedRect.offsetMax = new Vector2(17f + columnRight, hero ? 15f + topInset : 0f);
                window = bleed;
                if (!hero)
                {
                    // Below a board the garden melts into the paper at its top too.
                    var top = Node("Garden melt top", window.transform);
                    var topRect = top.GetComponent<RectTransform>();
                    topRect.anchorMin = new Vector2(0f, 1f);
                    topRect.anchorMax = new Vector2(1f, 1f);
                    topRect.pivot = new Vector2(0.5f, 1f);
                    topRect.sizeDelta = new Vector2(0f, 40f);
                    var topImage = top.AddComponent<Image>();
                    topImage.sprite = MosseloraArt.TopFade();
                    topImage.color = Paper;
                    topImage.raycastTarget = false;
                }
                // Melt into the paper at the bottom, as the home screen does.
                var melt = Node("Hero melt", window.transform);
                var meltRect = melt.GetComponent<RectTransform>();
                meltRect.anchorMin = Vector2.zero;
                meltRect.anchorMax = new Vector2(1f, 0f);
                meltRect.pivot = new Vector2(0.5f, 0f);
                meltRect.sizeDelta = new Vector2(0f, 48f);
                meltRect.localScale = new Vector3(1f, -1f, 1f);
                meltRect.anchoredPosition = new Vector2(0f, 48f);
                var meltImage = melt.AddComponent<Image>();
                meltImage.sprite = MosseloraArt.TopFade();
                meltImage.color = Paper;
                meltImage.raycastTarget = false;
            }
            window.AddComponent<RectMask2D>();
            bool grow = celebrateGrowth && memoryScreen;
            celebrateGrowth = false;
            bool animate = grow && gentleMotion;
            GardenMapView view = GardenMapView.Build(window.GetComponent<RectTransform>(), plot, map, MapItems(map), 1.25f,
                OnMapLand, OnMapItem, ItemInHand(), animate, GreenedShare(0),
                animate ? GreenedShare(1) : -1f,
                memoryScreen ? null : (land, point) => NeighborsOnMap(land, point, map, false), 0f, RiverGoesOn(map));
            // This stream opened a new reach of river: watch its mist lift (I195).
            if (animate && !world.Completed && map.Reaches > 1 &&
                world.Choices.Length % GardenMap.StreamsPerReach == 0)
            {
                view.LiftMist(map.Reaches - 1);
                PlayChime();
            }
            if (!memoryScreen) RiverDrops(view, false);
            // Under a floating title the new thing sits lower in view (I185).
            if (hero) view.FocusFromTop = 0.55f;
            int held = ItemInHand();
            if (held >= 0 && PlayerPrefs.GetInt(PlacedOnceKey, 0) == 0) PlaceTip(window.transform, held);
            // No wordmark or tagline over your garden (I194): you know where you are.
            return true;
        }

        // The wordmark and tagline float on the garden, under the menu and book.
        private void HeroTitle(Transform hero)
        {
            Sprite mark = MosseloraArt.Load(MosseloraArt.Wordmark);
            var title = Node("Floating title", hero);
            title.AddComponent<LayoutElement>().ignoreLayout = true;
            var rect = title.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(1f, 1f);
            rect.pivot = new Vector2(0.5f, 1f);
            rect.sizeDelta = new Vector2(0f, 118f);
            rect.anchoredPosition = new Vector2(0f, -topInset - 6f);
            if (mark != null)
            {
                var markNode = Node("Mosselora wordmark", title.transform);
                var markRect = markNode.GetComponent<RectTransform>();
                markRect.anchorMin = new Vector2(0f, 1f);
                markRect.anchorMax = new Vector2(1f, 1f);
                markRect.pivot = new Vector2(0.5f, 1f);
                markRect.sizeDelta = new Vector2(-2f * ChromeInset, 72f);
                markRect.anchoredPosition = Vector2.zero;
                var markImage = markNode.AddComponent<Image>();
                markImage.sprite = mark;
                markImage.preserveAspect = true;
                markImage.raycastTarget = false;
            }
            // The tagline on a small cream plate, readable over any grass.
            var plate = Node("Tagline plate", title.transform);
            var plateRect = plate.GetComponent<RectTransform>();
            plateRect.anchorMin = plateRect.anchorMax = new Vector2(0.5f, 1f);
            plateRect.pivot = new Vector2(0.5f, 1f);
            plateRect.anchoredPosition = new Vector2(0f, -76f);
            var plateImage = plate.AddComponent<Image>();
            MosseloraArt.Round(plateImage, new Color(1f, 0.973f, 0.918f, 0.92f), 14);
            plateImage.raycastTarget = false;
            var plateRow = plate.AddComponent<HorizontalLayoutGroup>();
            plateRow.padding = new RectOffset(14, 14, 4, 5);
            plateRow.childControlWidth = true;
            plateRow.childControlHeight = true;
            var fit = plate.AddComponent<ContentSizeFitter>();
            fit.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
            fit.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            Text line = Label(plate.transform, "Title", GameLocalization.Text("subtitle"), 17, 0, Ink,
                TextAnchor.MiddleCenter);
            Emphasize(line);
            line.horizontalOverflow = HorizontalWrapMode.Overflow;
        }

        // Until a player has put something down once, the map says how (I182): a
        // cold read pressed the arrow and never found that the birch could move.
        private const string PlacedOnceKey = "evergrove:placed-once";
    }
}
