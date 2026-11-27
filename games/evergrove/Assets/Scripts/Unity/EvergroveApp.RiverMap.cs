using Evergrove.Core;
using UnityEngine;
using UnityEngine.UI;

namespace Evergrove.UnityClient
{
    public sealed partial class EvergroveApp
    {
        // ---- Checklist 45: the river map. From the menu, "River map" opens a painted scroll of the river:
        // a thin river line down the sheet, each open stretch as a line with its name (the one the view is on
        // says "you are here" in words and wears a check), the mist below the last open stretch, and family
        // names at the river's mouth, only once family is shown. Tapping a stretch travels there by boat.
        private GameObject riverMapSheet;

        private const string RiverMapHeader = "Your river runs down the valley. Tap a stretch to travel there by boat.";
        private const string RiverMapMist = "The mist hides the stretches not yet opened. Keep tending the garden and the river runs on.";
        private const string RiverMapFamilyHeader = "Family along the river";
        private const string RiverMapFamilyNote = "Where they are is not known, so their names wait at the river's mouth.";

        // The menu's entry (tap one). Living gardens only; one open stretch is already a river worth a map.
        private void BuildRiverMapEntry()
        {
            if (world == null || world.Mode != PlayMode.Living) return;
            Button(content, "River map", RiverMap.MenuLabel, () => { SetMenu(false); OpenRiverMap(); });
        }

        internal void OpenRiverMap()
        {
            if (titleScreen == null || world == null || world.Mode != PlayMode.Living) return;
            bool replacing = riverMapSheet != null;
            RetireRiverMapSheet(replacing);
            int open = OpenReaches();
            int here = CurrentStretch();
            // Family pins only once family is shown (checklist 8); nothing is listed for a solo garden.
            string[] family = FamilyShown() ? RiverMap.FamilyNamesFrom(world.LetterFrom, world.HelpedFrom, world.Lanterns) : null;
            var rows = RiverMap.Rows(open, here, family);

            riverMapSheet = BuildSheet(titleScreen.transform, "River map", CloseRiverMap,
                out RectTransform content, out ScrollRect scroll);
            SheetClose(content, "Close river map", CloseRiverMap);
            Text title = Label(content, "River map title", RiverMap.MenuLabel, 26, 0, Ink, TextAnchor.MiddleCenter);
            Emphasize(title);
            Text header = Label(content, "River map header", RiverMapHeader, 18, 0, Ink, TextAnchor.MiddleLeft);
            header.GetComponent<LayoutElement>().flexibleWidth = 1f;

            // The scroll's heading rides on the painted stream-sign plank (no new art); text-only if the art is missing.
            Transform plank = PaintedSign(content);
            if (plank != null) OneLine(Label(plank, "River name", "The river", 20, 0, Ink, TextAnchor.MiddleCenter));

            // Source first: the stretches from home's stretch down the river, the one in view marked.
            foreach (RiverMapRow row in rows.FindAll(r => r.Kind == RiverMapKind.Stretch))
                RiverStretchLine(content, row);

            // The mist: the river goes on past the last open stretch, which is not yet shown.
            Text mist = Label(content, "River mist", RiverMapMist, 16, 0, MosseloraTheme.SecondaryInk, TextAnchor.MiddleLeft);
            mist.GetComponent<LayoutElement>().flexibleWidth = 1f;

            // The river's mouth: relatives known from this device, with no claim about where they are.
            var relatives = rows.FindAll(r => r.Kind == RiverMapKind.Family);
            if (relatives.Count > 0)
            {
                Emphasize(Label(content, "River family header", RiverMapFamilyHeader, 20, 0, Ink, TextAnchor.MiddleLeft));
                Text note = Label(content, "River family note", RiverMapFamilyNote, 16, 0, MosseloraTheme.SecondaryInk, TextAnchor.MiddleLeft);
                note.GetComponent<LayoutElement>().flexibleWidth = 1f;
                for (int i = 0; i < relatives.Count; i++)
                    Label(content, "River family " + (i + 1), relatives[i].Name, 20, 0, Ink, TextAnchor.MiddleLeft);
            }

            SheetMotion.Show(riverMapSheet, SheetEdge.For("River map"), ReducedMotion);
            riverMapSheet.transform.SetAsLastSibling(); // over the garden
            FitSheet(riverMapSheet, "River map panel");
            Canvas.ForceUpdateCanvases();
            LayoutRebuilder.ForceRebuildLayoutImmediate(content);
            FitSheetToPages(riverMapSheet, content);
            if (scroll != null) scroll.verticalNormalizedPosition = 1f;
            semanticsDirty = true;
        }

        // One open stretch: a river segment, then the stretch as a button. Tapping it travels there (the boat's own TravelTo).
        private void RiverStretchLine(RectTransform parent, RiverMapRow row)
        {
            int reach = row.Reach;
            GameObject line = Node("River reach " + reach, parent);
            var layout = line.AddComponent<HorizontalLayoutGroup>();
            layout.spacing = 10f;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = false;
            layout.childForceExpandHeight = false;

            GameObject channel = Node("River channel", line.transform);
            var water = channel.AddComponent<Image>();
            water.color = MosseloraTheme.WaterChannel;
            water.raycastTarget = false;
            var channelSize = channel.AddComponent<LayoutElement>();
            channelSize.minWidth = channelSize.preferredWidth = 10f;
            channelSize.flexibleHeight = 1f;

            string caption = row.Current ? row.Name + ", you are here" : row.Name;
            Button stretch = Button(line.transform, "Stretch " + reach, caption, () =>
            {
                RetireRiverMapSheet(false);
                TravelTo(reach);
            }, 52, false);
            LayoutElement stretchSize = stretch.GetComponent<LayoutElement>();
            if (stretchSize == null) stretchSize = stretch.gameObject.AddComponent<LayoutElement>();
            stretchSize.flexibleWidth = 1f;
            MosseloraArt.StateMark(stretch.transform, row.Current, 18f);
        }

        internal void CloseRiverMap() { RetireRiverMapSheet(false); }

#if UNITY_EDITOR || DEVELOPMENT_BUILD
        // ---- Dev QA tour hook (checklist 45 screenshot): opens the map over the hub, as the menu entry does.
        internal void QaOpenRiverMap()
        {
            qaShowHub = true;
            if (titleScreen == null && canvasRoot != null) BuildTitleScreen(canvasRoot);
            OpenRiverMap();
        }
#endif

        private void RetireRiverMapSheet(bool replacing)
        {
            RetireSheet(riverMapSheet, "River map", replacing);
            riverMapSheet = null;
            semanticsDirty = true;
        }
    }
}
