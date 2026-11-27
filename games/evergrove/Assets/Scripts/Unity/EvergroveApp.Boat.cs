using System;
using System.Collections;
using System.Collections.Generic;
using Evergrove.Core;
using UnityEngine;
using UnityEngine.UI;

namespace Evergrove.UnityClient
{
    public sealed partial class EvergroveApp
    {
        // ---- Checklist 43: travel by boat. From the menu, "Travel by boat" lists every open stretch of
        // the river; choosing one closes the sheet and moves the garden's view there. With motion on, the
        // rowing boat glides down the river while the view eases to the stretch; reduced motion jumps.
        private GameObject travelSheet;
        private GameObject travelBoat;
        private Coroutine travelGlide;

        // The chosen stretch settles this far down the window, clear of the title above the map.
        private const float TravelViewShare = 0.4f;

        // The menu's entry (tap one). Living gardens with two or more open stretches only.
        private void BuildTravelEntry()
        {
            if (world == null || world.Mode != PlayMode.Living || !RiverTravel.CanTravel(OpenReaches())) return;
            Button(content, "Travel by boat", RiverTravel.MenuLabel, () => { SetMenu(false); OpenTravelSheet(); });
        }

        internal void OpenTravelSheet()
        {
            if (titleScreen == null || world == null || world.Mode != PlayMode.Living) return;
            int open = OpenReaches();
            if (!RiverTravel.CanTravel(open)) return;
            bool replacing = travelSheet != null;
            RetireTravelSheet(replacing);
            travelSheet = BuildSheet(titleScreen.transform, "Travel by boat", CloseTravelSheet,
                out RectTransform content, out ScrollRect scroll);
            SheetClose(content, "Close travel", CloseTravelSheet);
            Text title = Label(content, "Travel title", RiverTravel.MenuLabel, 26, 0, Ink, TextAnchor.MiddleCenter);
            Emphasize(title);
            Text header = Label(content, "Travel header", "Choose a stretch of river. Your boat rows you there.",
                18, 0, Ink, TextAnchor.MiddleLeft);
            header.GetComponent<LayoutElement>().flexibleWidth = 1f;

            // Each stretch is one button (tap two). The stretch the view is on wears a check and says so in words.
            int here = CurrentStretch();
            foreach (RiverStop stop in RiverTravel.Stops(open))
            {
                int reach = stop.Reach;
                bool current = reach == here;
                string caption = current ? stop.Name + ", you are here" : stop.Name;
                Button row = Button(content, "Stretch " + reach, caption, () => TravelTo(reach), 52, false);
                MosseloraArt.StateMark(row.transform, current, 18f);
            }

            SheetMotion.Show(travelSheet, SheetEdge.For("Travel by boat"), ReducedMotion);
            travelSheet.transform.SetAsLastSibling(); // over the garden
            FitSheet(travelSheet, "Travel by boat panel");
            Canvas.ForceUpdateCanvases();
            LayoutRebuilder.ForceRebuildLayoutImmediate(content);
            FitSheetToPages(travelSheet, content);
            if (scroll != null) scroll.verticalNormalizedPosition = 1f;
            semanticsDirty = true;
        }

        internal void CloseTravelSheet() { RetireTravelSheet(false); }

        private void RetireTravelSheet(bool replacing)
        {
            RetireSheet(travelSheet, "Travel by boat", replacing);
            travelSheet = null;
            semanticsDirty = true;
        }

        // Tap two: the sheet goes away, then the view travels to the chosen stretch (checklist 43).
        private void TravelTo(int reach)
        {
            RetireTravelSheet(false);
            ScrollRect scroll = hubMapView != null ? hubMapView.GetComponent<ScrollRect>() : null;
            RectTransform[] stretches = ReachRects(scroll);
            if (scroll == null || reach < 0 || reach >= stretches.Length) return;
            float targetY = NormalizedYFor(scroll, stretches[reach]);
            if (travelGlide != null) StopCoroutine(travelGlide);
            travelGlide = null;
            DestroyTravelBoat();
            if (!gentleMotion)
            {
                // Reduced motion: no glide, the view jumps straight there.
                hubMapView.KeepAt(new Vector2(scroll.normalizedPosition.x, targetY));
                return;
            }
            travelGlide = StartCoroutine(GlideTo(scroll, reach, targetY));
        }

        // The boat rows from the current stretch to the chosen one while the view eases after it.
        // Only transforms move (the boat's position, the scroll position); unscaled, at most 1.2 s.
        private IEnumerator GlideTo(ScrollRect scroll, int reach, float targetY)
        {
            RectTransform content = scroll.content;
            RectTransform[] stretches = ReachRects(scroll);
            int from = CurrentStretch();
            if (from >= stretches.Length || reach >= stretches.Length) { travelGlide = null; yield break; }

            int boatIndex = RiverTravel.BoatItemIndex();
            Sprite art = boatIndex >= 0 ? MosseloraArt.Load(GardenCatalog.Get(VillageChoice.Bridge, boatIndex).Art) : null;
            var node = new GameObject("Rowing boat", typeof(RectTransform));
            node.transform.SetParent(content, false);
            var boat = (RectTransform)node.transform;
            float size = scroll.viewport.rect.width * 0.2f;
            boat.sizeDelta = new Vector2(size, size / Mathf.Max(0.01f, MosseloraArt.Aspect(art)));
            var image = node.AddComponent<Image>();
            image.sprite = art;
            image.preserveAspect = true;
            image.raycastTarget = false;
            travelBoat = node;

            Vector3 start = content.InverseTransformPoint(Centre(stretches[from]));
            Vector3 end = content.InverseTransformPoint(Centre(stretches[reach]));
            float startY = scroll.normalizedPosition.y;
            boat.localPosition = start;
            boat.SetAsLastSibling();
            scroll.StopMovement();

            float elapsed = 0f;
            while (elapsed < RiverTravel.GlideSeconds)
            {
                if (scroll == null || boat == null) { travelGlide = null; yield break; } // the map was rebuilt under us
                elapsed += Time.unscaledDeltaTime;
                float t = SheetEdge.Ease(Mathf.Clamp01(elapsed / RiverTravel.GlideSeconds));
                boat.localPosition = Vector3.Lerp(start, end, t);
                scroll.normalizedPosition = new Vector2(scroll.normalizedPosition.x, Mathf.Lerp(startY, targetY, t));
                yield return null;
            }

            DestroyTravelBoat();
            travelGlide = null;
            // Ends exactly on the chosen stretch, and remembers it for any later rebuild of the map.
            if (hubMapView != null) hubMapView.KeepAt(new Vector2(scroll.normalizedPosition.x, targetY));
        }

        private void DestroyTravelBoat()
        {
            if (travelBoat != null) Destroy(travelBoat);
            travelBoat = null;
        }

        // The painted "Reach N" images of the hub map, in order (none for a single painting).
        private static RectTransform[] ReachRects(ScrollRect scroll)
        {
            if (scroll == null || scroll.content == null) return new RectTransform[0];
            var byNumber = new Dictionary<int, RectTransform>();
            foreach (RectTransform rect in scroll.content.GetComponentsInChildren<RectTransform>(true))
            {
                if (rect == null || !rect.name.StartsWith("Reach ", StringComparison.Ordinal)) continue;
                if (!int.TryParse(rect.name.Substring(6), out int number) || number < 1) continue;
                if (!byNumber.ContainsKey(number)) byNumber.Add(number, rect);
            }
            var stretches = new RectTransform[byNumber.Count];
            for (int i = 0; i < stretches.Length; i++)
                if (!byNumber.TryGetValue(i + 1, out stretches[i])) return new RectTransform[0];
            return stretches;
        }

        private static Vector3 Centre(RectTransform stretch)
        {
            return stretch.TransformPoint(stretch.rect.center);
        }

        // The stretch the view is on: the painted reach nearest the middle of the window.
        private int CurrentStretch()
        {
            ScrollRect scroll = hubMapView != null ? hubMapView.GetComponent<ScrollRect>() : null;
            RectTransform[] stretches = ReachRects(scroll);
            if (stretches.Length == 0) return 0;
            int best = 0;
            float bestGap = float.MaxValue;
            for (int i = 0; i < stretches.Length; i++)
            {
                float gap = Mathf.Abs(scroll.viewport.InverseTransformPoint(Centre(stretches[i])).y);
                if (gap < bestGap) { bestGap = gap; best = i; }
            }
            return best;
        }

        // The normalized scroll position that puts a stretch TravelViewShare of the window down from its top.
        // Clamped to the map, so the first and last stretches sit as near as the map allows.
        private static float NormalizedYFor(ScrollRect scroll, RectTransform stretch)
        {
            RectTransform content = scroll.content;
            float distance = content.rect.height - scroll.viewport.rect.height;
            if (distance <= 0f) return 1f;
            float fromTop = content.rect.yMax - content.InverseTransformPoint(Centre(stretch)).y;
            float offset = fromTop - scroll.viewport.rect.height * TravelViewShare;
            return 1f - Mathf.Clamp01(offset / distance);
        }

#if UNITY_EDITOR || DEVELOPMENT_BUILD
        // ---- Dev QA tour hooks (checklist 43 screenshots)
        internal void QaOpenTravel()
        {
            qaShowHub = true;
            if (titleScreen == null && canvasRoot != null) BuildTitleScreen(canvasRoot);
            OpenTravelSheet();
        }

        internal int QaOpenReaches() { return OpenReaches(); }

        internal void QaTravel(int reach) { TravelTo(reach); }
#endif
    }
}
