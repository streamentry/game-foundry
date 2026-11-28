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

        private void OnMapItem(int index)
        {
            if (index == WellIndex) { decorInHand = -1; heldDecor = -1; OpenWell(); return; }
            if (index >= DecorIndex)
            {
                // I330: tap a decoration to pick it up; tap it again to put it away.
                int d = index - DecorIndex;
                if (heldDecor == d) { if (world.RemoveDecor(d)) SaveWorld(); heldDecor = -1; }
                else { heldDecor = d; decorInHand = -1; }
                RefreshMaps();
                return;
            }
            decorInHand = -1; // review fix: picking up anything else lets go of a decoration
            heldDecor = -1;
            if (index >= SecretIndex) { FindSecret(index - SecretIndex); return; }
            if (index >= BoughtIndex)
            {
                // A bought plant: pick it up (to move it, or play its puzzle), or put it down.
                int k = index - BoughtIndex;
                heldBought = heldBought == k ? -1 : k;
                heldPlanting = -1;
                mapInHand = -1;
                RefreshMaps();
                return;
            }
            heldBought = -1;
            if (index >= PlantingIndex)
            {
                // A planting: pick it up (or put it down again where it is).
                int k = index - PlantingIndex;
                heldPlanting = heldPlanting == k ? -1 : k;
                mapInHand = -1;
                RefreshMaps();
                return;
            }
            heldPlanting = -1;
            mapInHand = ItemInHand() == index ? -2 : index; // -2: put nothing in hand
            RefreshMaps();
            if (mapInHand == -2) mapInHand = -1;
        }

        private void OnMapLand(int cell)
        {
            GardenMap map = LoadGardenMap();
            if (map == null) return;
            int[] layout = map.Layout(world.Choices, world.Placements, Occupied());
            if (decorInHand >= 0 || heldDecor >= 0)
            {
                // I330: set a decoration down (or move one): free land only, never on a thing.
                bool done = heldDecor >= 0 ? world.MoveDecor(heldDecor, cell, map)
                    : world.PlaceDecor((Decoration)decorInHand, cell, map);
                if (!done)
                {
                    // Review fix: never a silent lock. A tap where it can't go puts it back.
                    decorInHand = -1;
                    heldDecor = -1;
                    if (titleScreen != null)
                        RuleToast.Show((RectTransform)titleScreen.transform, "It can't stand there, so it went back to the cottage.",
                            font, Ink, MosseloraTheme.CardFill);
                    RefreshMaps();
                    return;
                }
                justDecorated = heldDecor >= 0 ? heldDecor : world.DecorCount - 1;
                decorInHand = -1;
                heldDecor = -1;
                SaveWorld();
                PlayTock();
                RefreshMaps();
                return;
            }
            if (heldBought >= 0)
            {
                // Move a bought plant: only to free land.
                if (!map.CanPlace(layout, cell) || PlantedAt(cell, -1, heldBought)) return;
                if (!world.MoveBought(heldBought, cell)) return;
                heldBought = -1;
                PlantDone();
                return;
            }
            if (giftPick >= 0 && giftPick < world.Gifts.Length && ItemInHand() < 0)
            {
                // Plant a cutting a relative sent: it becomes a nursery plant of yours.
                if (!map.CanPlace(layout, cell) || PlantedAt(cell)) return;
                if (!world.PlantGift(giftPick, cell)) return;
                RecordInAlmanac();
                justBought = world.BoughtCount - 1;
                giftPick = -1;
                PlayChime();
                PlantDone();
                return;
            }
            if (nurseryOpen && nurseryKind >= 0 && ItemInHand() < 0)
            {
                // Buy and plant from the nursery; it stays picked while you can afford another.
                if (!map.CanPlace(layout, cell) || PlantedAt(cell)) return;
                if (!world.Buy(nurseryKind, cell)) return;
                RecordInAlmanac();
                justBought = world.BoughtCount - 1;
                // The slot restocks with its next plant (I240); keep it picked while affordable.
                nurseryKind = world.NurseryStock(Nursery.Slot(nurseryKind));
                if (world.Seeds < Nursery.Price(nurseryKind)) nurseryKind = -1;
                PlayChime();
                PlantDone();
                return;
            }
            if (heldPlanting >= 0)
            {
                // Move a planting: only to free land.
                if (!map.CanPlace(layout, cell) || PlantedAt(cell, heldPlanting)) return;
                if (!world.MovePlanting(heldPlanting, cell)) return;
                heldPlanting = -1;
                PlantDone();
                return;
            }
            // I299: your cottage, tapped with nothing in hand: what's waiting, at a glance.
            if (ItemInHand() < 0 && !basketOpen && titleScreen != null && map.IsCottage(cell))
            {
                OpenCottage();
                return;
            }
            int target = ItemInHand();
            if (target < 0 && basketOpen && basketKind >= 0)
            {
                // Plant one more from the basket; the basket stays ready for the next tap.
                if (!map.CanPlace(layout, cell) || PlantedAt(cell)) return;
                if (!world.Plant(basketKind, cell)) return;
                justPlanted = world.PlantingCount - 1;
                PlantDone();
                return;
            }
            if (target < 0) return;
            if (!map.CanPlace(layout, cell, target) || PlantedAt(cell)) return; // water, the cottage, or taken: it stays in hand
            if (!world.PlaceAt(target, cell)) return;
            PlayerPrefs.SetInt(PlacedOnceKey, 1);
            SaveWorld();
            mapInHand = -1;
            PlayTock();
            Haptics.Placed();
            RefreshMaps();
        }

        private void PlantDone()
        {
            SaveWorld();
            PlayTock();
            Haptics.Placed();
            RefreshMaps();
        }

        // Put a picked-up planting back in the basket.
        private void PutBack()
        {
            if (heldPlanting < 0 || !world.RemovePlanting(heldPlanting)) return;
            heldPlanting = -1;
            PlantDone();
        }

        // The basket on home (I193): a round token bottom left. Open, it shows a tray
        // of everything you've grown; pick one and tap the grass to plant more of it.
        private void BasketControls(Transform screen, Transform stack)
        {
            int[] made = world.Choices;
            if (world.Mode != PlayMode.Living || made.Length == 0) return;
            // Checklist 11: no resting basket control. The corner exists only while the tray is open
            // (or a planting is held, so it can be put back); closed, the fan's Basket token opens it.
            if (!basketOpen && heldPlanting < 0) return;
            var corner = Node("Basket corner", screen);
            var cornerRect = corner.GetComponent<RectTransform>();
            Rect safe = Screen.safeArea;
            float w = Mathf.Max(1, Screen.width), h = Mathf.Max(1, Screen.height);
            cornerRect.anchorMin = new Vector2(safe.xMin / w, safe.yMin / h);
            cornerRect.anchorMax = new Vector2(safe.xMax / w, safe.yMin / h);
            cornerRect.pivot = new Vector2(0.5f, 0f);
            // Checklist 94: the corner starts right of the bottom-left menu button, so the two never overlap.
            cornerRect.sizeDelta = new Vector2(-MenuReserve, basketOpen ? 104f : 76f);
            cornerRect.anchoredPosition = new Vector2(MenuReserveOffset, 10f); // checklist 5: mirrors with the menu
            if (basketOpen)
            {
                stack.gameObject.SetActive(false);
                var tray = Node("Seed tray", corner.transform);
                var trayRect = tray.GetComponent<RectTransform>();
                Fill(trayRect);
                trayRect.offsetMin = new Vector2(86f, 0f);
                trayRect.offsetMax = new Vector2(-10f, 0f);
                var trayImage = tray.AddComponent<Image>();
                MosseloraArt.Round(trayImage, MosseloraTheme.CardFill, 26);
                MosseloraArt.Lift(trayImage, 2f, 0.18f);
                tray.AddComponent<RectMask2D>();
                var trayScroll = tray.AddComponent<ScrollRect>();
                trayScroll.horizontal = true;
                trayScroll.vertical = false;
                trayScroll.movementType = ScrollRect.MovementType.Clamped;
                var row = Node("Tray row", tray.transform);
                var rowRect = row.GetComponent<RectTransform>();
                rowRect.anchorMin = new Vector2(0f, 0f);
                rowRect.anchorMax = new Vector2(0f, 1f);
                rowRect.pivot = new Vector2(0f, 0.5f);
                rowRect.sizeDelta = Vector2.zero;
                var rowGroup = row.AddComponent<HorizontalLayoutGroup>();
                rowGroup.padding = new RectOffset(12, 12, 10, 10);
                rowGroup.spacing = 10;
                rowGroup.childAlignment = TextAnchor.MiddleLeft;
                rowGroup.childControlWidth = true;
                rowGroup.childControlHeight = true;
                rowGroup.childForceExpandWidth = false;
                rowGroup.childForceExpandHeight = false;
                row.AddComponent<ContentSizeFitter>().horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
                trayScroll.content = rowRect;
                trayScroll.viewport = trayRect;
                // The nursery (I208): a seed-bag token first in the tray switches it
                // between your own kinds and the plants seeds can buy.
                Button bag = IconButton(row.transform, nurseryOpen ? "Your own" : "Nursery",
                    nurseryOpen ? "Your own" : "Nursery", TreePlay.SeedIcon, () => {
                        nurseryOpen = !nurseryOpen; nurseryKind = -1; basketKind = -1; heldPlanting = -1; RefreshMaps();
                    }, 74, nurseryOpen ? MosseloraTheme.ForestAction : new Color(0.93f, 0.88f, 0.72f, 1f),
                    false, null, false, 0, 0, true);
                Transform bagGlyph = bag.transform.Find("Icon");
                if (bagGlyph != null) bagGlyph.GetComponent<Image>().color = Color.white;
                // Shop, not plant (I213): the sprout wears the same seed chip as the
                // purse and the prices, so the token reads "spend seeds here".
                if (!nurseryOpen) PriceChip(bag.transform, world.Seeds.ToString(), true, true);
                // Checklist 38: a plots puzzle solved today sent a seedling here; a small sprout says so (cosmetic).
                if (ChainCause.SeedlingNow(world.Garden))
                {
                    Image sprout = MosseloraArt.Ornament(bag.transform, "A seedling from the plots", MosseloraArt.Load(TreePlay.SeedIcon),
                        new Vector2(0f, 1f), new Vector2(0.5f, 0.5f), new Vector2(26f, 26f), new Vector2(8f, -8f));
                    if (sprout != null) sprout.raycastTarget = false;
                }
                if (nurseryOpen) NurseryTokens(row.transform);
                else GiftTokens(row.transform);
                for (int i = nurseryOpen ? -1 : made.Length - 1; i >= 0; i--) // your own kinds, newest first
                {
                    int kind = i;
                    Sprite art = ItemArt(i, made[i]);
                    Button seed = IconButton(row.transform, "Basket " + i, ItemAt(i, made[i]).Name, MosseloraArt.IconRooted,
                        () => { basketKind = basketKind == kind ? -1 : kind; heldPlanting = -1; giftPick = -1; RefreshMaps(); },
                        74, kind == basketKind ? MosseloraTheme.ForestAction : MosseloraTheme.QuietAction,
                        false, null, false, 0, 0, true);
                    Transform glyph = seed.transform.Find("Icon");
                    Image glyphImage = glyph != null ? glyph.GetComponent<Image>() : null;
                    if (glyphImage != null && art != null)
                    {
                        glyphImage.sprite = art;
                        glyphImage.color = Color.white;
                        var glyphSize = glyph.GetComponent<LayoutElement>();
                        glyphSize.minWidth = glyphSize.preferredWidth = 50f;
                        glyphSize.minHeight = glyphSize.preferredHeight = 50f;
                    }
                    if (kind == basketKind && gentleMotion) BloomPulse.Add(seed.gameObject, 0.9f, 0.06f);
                }
            }
            // The basket itself: open, close, or (holding a planting) put it back.
            // Open, the tray's close control (or Put back while a planting is held). At least 48 pt.
            bool holding = heldPlanting >= 0;
            string glyphName = holding ? MosseloraArt.IconUndo : MosseloraArt.IconCancel;
            Button basket = IconButton(corner.transform, holding ? "Put back" : "Close basket",
                holding ? "Put back" : GameLocalization.Text("close"), glyphName, () => {
                    if (heldPlanting >= 0) { PutBack(); return; }
                    basketOpen = false;
                    basketKind = -1;
                    nurseryOpen = false; nurseryKind = -1;
                    RefreshMaps();
                }, 72, MosseloraTheme.ForestAction,
                false, null, false, 0, 0, true);
            var basketRect = basket.GetComponent<RectTransform>();
            basketRect.anchorMin = basketRect.anchorMax = new Vector2(0f, 0f);
            basketRect.pivot = new Vector2(0f, 0f);
            basketRect.anchoredPosition = new Vector2(10f, basketOpen ? 16f : 2f);
            basketRect.sizeDelta = new Vector2(72f, 72f);
        }

        // Checklist 11: the basket lives in the corner fan now. Its token wears the painted basket with
        // the newest thing peeking out, and the gift ribbon when a cutting waits (both as the closed toggle did).
        private void BasketFanCue(Button basket)
        {
            int[] made = world.Choices;
            if (basket == null || made.Length == 0) return;
            // I345 (run 3, P3): closed, it reads as a basket (the painted one) with the
            // newest thing you grew peeking out of it, so "things to plant" says itself.
            Transform glyph = basket.transform.Find("Icon");
            Image glyphImage = glyph != null ? glyph.GetComponent<Image>() : null;
            Sprite newest = ItemArt(made.Length - 1, made[made.Length - 1]);
            Sprite basketArt = MosseloraArt.Load("food-basket");
            if (glyphImage != null && basketArt != null)
            {
                glyphImage.sprite = basketArt;
                glyphImage.color = Color.white;
                ((RectTransform)glyph).anchoredPosition = new Vector2(0f, -6f);
                Image peek = newest == null ? null : MosseloraArt.Ornament(basket.transform, "Newest thing, peeking out",
                    newest, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0f), new Vector2(34f, 34f), new Vector2(4f, 2f));
                if (peek != null) peek.transform.SetSiblingIndex(glyph.GetSiblingIndex()); // behind the basket's rim
            }
            else if (glyphImage != null && newest != null) { glyphImage.sprite = newest; glyphImage.color = Color.white; }
            if (world.PlantingCount == 0 && gentleMotion) BloomPulse.Add(basket.gameObject, 0.8f, 0.08f);
            // SC05: one soft mark where the top waiting thing lives. When that's a cutting, the
            // basket wears the gift ribbon (B5's honey, never red): content waiting, never a count.
            if (world.Gifts.Length > 0 && CompanionAttention.Mark(CompanionFactsNow(),
                    CompanionDayNow(), world.Garden) == Attention.Basket)
            {
                Image ribbon = MosseloraArt.Ornament(basket.transform, "A cutting is waiting", MosseloraArt.Rounded(8),
                    new Vector2(1f, 1f), new Vector2(0.5f, 0.5f), new Vector2(20f, 20f), new Vector2(-8f, -8f));
                if (ribbon != null) { ribbon.preserveAspect = false; ribbon.color = new Color(0.93f, 0.72f, 0.36f, 1f); }
            }
        }

        // Cuttings relatives sent (I215), waiting in the basket: each wears a small
        // ribbon. Pick one, tap the grass: it's planted and becomes yours.
        private int giftPick = -1;
    }
}
