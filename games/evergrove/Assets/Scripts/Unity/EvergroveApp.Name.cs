using Evergrove.Core;
using UnityEngine;
using UnityEngine.UI;

namespace Evergrove.UnityClient
{
    public sealed partial class EvergroveApp
    {
        // ---- Checklist 3: name your garden. Optional; shown on the cottage sign and on postcards.
        private GameObject nameSheet;
        private string nameNote = ""; // why the last name was refused, shown until the sheet closes

        // The menu's entry for naming the garden. Called once from the menu build; Living gardens only.
        private void BuildNameEntry()
        {
            if (world == null || world.Mode != PlayMode.Living) return;
            Button(content, "Name your garden", "Name your garden", () => { SetMenu(false); OpenNameSheet(); });
        }

        internal void OpenNameSheet()
        {
            if (titleScreen == null || world == null || world.Mode != PlayMode.Living) return;
            nameNote = "";
            BuildNameSheet(world.GardenName);
        }

        internal void CloseNameSheet() { RetireName(false); }

        private void RetireName(bool replacing)
        {
            RetireSheet(nameSheet, "Name sheet", replacing);
            nameSheet = null;
            semanticsDirty = true;
        }

        // Rebuilt after a refused name, keeping what was typed so nothing is lost.
        private void BuildNameSheet(string typed)
        {
            if (titleScreen == null || world == null) return;
            bool replacing = nameSheet != null;
            RetireName(replacing);
            nameSheet = BuildSheet(titleScreen.transform, "Name sheet", CloseNameSheet,
                out RectTransform sheetContent, out ScrollRect sheetScroll);
            SheetClose(sheetContent, "Close naming", CloseNameSheet);
            Text title = Label(sheetContent, "Name title", "Name your garden", 26, 0, Ink, TextAnchor.MiddleCenter);
            Emphasize(title);
            Label(sheetContent, "Name hint", "It shows on your cottage sign and on postcards. Leave it empty to use the suggested name.",
                18, 0, Ink, TextAnchor.MiddleCenter);
            InputField field = NameField(sheetContent, typed ?? "", WorldModel.SuggestedName(world.Garden));
            if (nameNote.Length > 0) Label(sheetContent, "Name note", nameNote, 18, 0, Ink, TextAnchor.MiddleCenter);
            Button(sheetContent, "Keep this name", "Keep this name", () => KeepGardenName(field.text), 52, true);
            Button(sheetContent, "Use no name", "Use no name", UseNoGardenName, 52, false);

            SheetMotion.Show(nameSheet, SheetEdge.For("Name sheet"), ReducedMotion, null, replacing);
            nameSheet.transform.SetAsLastSibling();
            FitSheet(nameSheet, "Name sheet panel");
            Canvas.ForceUpdateCanvases();
            LayoutRebuilder.ForceRebuildLayoutImmediate(sheetContent);
            FitSheetToPages(nameSheet, sheetContent);
            if (sheetScroll != null) sheetScroll.verticalNormalizedPosition = 1f;
            semanticsDirty = true;
        }

        // An empty field keeps the suggestion shown as the placeholder. A refused name changes nothing.
        private void KeepGardenName(string typed)
        {
            string proposed = string.IsNullOrWhiteSpace(typed) ? WorldModel.SuggestedName(world.Garden) : typed;
            if (!world.SetGardenName(proposed))
            {
                nameNote = "Keep it to 24 letters or fewer, with no < or > and no line breaks.";
                BuildNameSheet(typed);
                return;
            }
            SaveWorld();
            CloseNameSheet();
            Rebuild();
        }

        private void UseNoGardenName()
        {
            world.SetGardenName("");
            SaveWorld();
            CloseNameSheet();
            Rebuild();
        }

        // The cottage sign's title line, when the garden has a name (called from the cottage card).
        private void GardenNameLine(Transform parent)
        {
            if (world == null || world.GardenName.Length == 0) return;
            Emphasize(Label(parent, "Garden name", world.GardenName, 24, 0, Ink, TextAnchor.MiddleCenter));
        }

        private InputField NameField(Transform parent, string value, string suggestion)
        {
            var node = Node("Garden name field", parent);
            node.AddComponent<LayoutElement>().preferredHeight = 56f;
            var back = node.AddComponent<Image>();
            MosseloraArt.Round(back, Color.white, 14);
            MosseloraArt.Edge(node.transform, MosseloraTheme.CardEdge, 14);
            var field = node.AddComponent<InputField>();
            Text shown = NameFieldText(node.transform, "Text", value, MosseloraTheme.EarthInk, false);
            Text placeholder = NameFieldText(node.transform, "Placeholder", suggestion, new Color(0.45f, 0.5f, 0.45f, 0.9f), true);
            field.targetGraphic = back;
            field.textComponent = shown;
            field.placeholder = placeholder;
            field.lineType = InputField.LineType.SingleLine;
            field.characterLimit = 60; // a little over the 24 text elements the save allows, so the check gives the note
            field.text = value;
            return field;
        }

        private Text NameFieldText(Transform parent, string name, string words, Color color, bool italic)
        {
            var node = Node(name, parent);
            var rect = node.GetComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = new Vector2(12f, 8f);
            rect.offsetMax = new Vector2(-12f, -8f);
            var text = node.AddComponent<Text>();
            text.font = font;
            text.fontSize = Mathf.RoundToInt(18 * TextScale.Percent / 100f);
            text.color = color;
            text.supportRichText = false;
            text.alignment = TextAnchor.MiddleLeft;
            text.raycastTarget = false;
            if (italic) text.fontStyle = FontStyle.Italic;
            text.text = words;
            return text;
        }
    }
}
