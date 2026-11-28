using System;
using Evergrove.Core;
using UnityEngine;
using UnityEngine.UI;

namespace Evergrove.UnityClient
{
    /// <summary>
    /// Shared pieces for the newer puzzle boards (I243 on): the earth-toned bed every
    /// table sits in, paper cells that take a tap, painted glyphs and numbers. One look
    /// for all eight kinds, the same as the Pairs table.
    /// </summary>
    internal static class BoardKit
    {
        public const float CardPadding = 36f;   // the card's side padding, both sides
        public const float BedPadding = 8f;
        public static readonly Color BedFill = new Color(0.55f, 0.5f, 0.36f, 0.55f);
        public static readonly Color Soil = new Color(0.93f, 0.91f, 0.86f, 1f);
        public static readonly Color Leafy = new Color(0.80f, 0.90f, 0.74f, 1f);
        public static readonly Color Water = new Color(0.62f, 0.83f, 0.88f, 1f);
        public static readonly Color Given = new Color(0.90f, 0.86f, 0.74f, 1f);
        public static readonly Color Picked = new Color(0.98f, 0.93f, 0.74f, 1f);
        // A conflict tile: muted sand (the palette's AnchoredTile, #F1DEB6), never red. It is only
        // painted once the player asks (see ShowClashes), and always with the ring below.
        public static readonly Color Clash = new Color(0.945f, 0.871f, 0.714f, 1f);

        // ---- Kind mistakes: conflicts are never shown by themselves. A lens token beside each
        // board's hint shows them, as a sand tile and a small ring; the choice is kept on this device.
        public const string ShowClashesKey = "evergrove:showClashes";
        private const string ClashMarkName = "Clash mark";
        private const string ShowCaption = "Show clashes";
        private const string HideCaption = "Hide clashes";

        /// <summary>Device-only, off by default. Saved at once, so it survives the app closing.</summary>
        public static bool ShowClashes
        {
            get { return PlayerPrefs.GetInt(ShowClashesKey, 0) == 1; }
            set
            {
                PlayerPrefs.SetInt(ShowClashesKey, value ? 1 : 0);
                PlayerPrefs.Save();
            }
        }

        /// <summary>
        /// The lens token, placed just after the hint token in the card's top row. Tapping it
        /// flips <see cref="ShowClashes"/> and calls <paramref name="repaint"/>. Its name is the
        /// spoken caption ("Show clashes" / "Hide clashes").
        /// </summary>
        public static void ClashLens(Transform top, Action repaint)
        {
            if (top == null) return;
            string caption = ShowClashes ? HideCaption : ShowCaption;
            // The lens is held by its own reference: upright, MoveControlsToFoot moves it off the top row,
            // so looking it up from the top row later would miss it.
            Transform lens = null;
            TreePlay.Token(top, caption, MosseloraArt.BoldRing(), 52f, LensTone(ShowClashes),
                () => ToggleClashes(lens, repaint), true);
            lens = top.Find(caption);
            Transform hint = top.Find("Hint: show the next move");
            if (lens != null && hint != null) lens.SetSiblingIndex(hint.GetSiblingIndex() + 1);
            MosseloraArt.StateMark(lens, ShowClashes, 20f);
        }

        private static void ToggleClashes(Transform lens, Action repaint)
        {
            ShowClashes = !ShowClashes;
            if (lens != null)
            {
                lens.name = ShowClashes ? HideCaption : ShowCaption;
                var image = lens.GetComponent<Image>();
                if (image != null) image.color = LensTone(ShowClashes);
                MosseloraArt.StateMark(lens, ShowClashes, 20f);
            }
            repaint();
        }

        private static Color LensTone(bool on)
        {
            return on ? MosseloraTheme.WaterAction : MosseloraTheme.ForestAction;
        }

        /// <summary>
        /// The shape cue for a conflict: a small earth-ink ring in the cell's top-right corner,
        /// created on first use and shown or hidden after that. Colour is never the only cue.
        /// </summary>
        public static void MarkClash(Image cell, bool on)
        {
            Transform mark = cell.transform.Find(ClashMarkName);
            if (mark == null)
            {
                if (!on) return;
                var node = new GameObject(ClashMarkName, typeof(RectTransform));
                node.transform.SetParent(cell.transform, false);
                var rect = (RectTransform)node.transform;
                rect.anchorMin = new Vector2(0.62f, 0.62f);
                rect.anchorMax = new Vector2(0.9f, 0.9f);
                rect.offsetMin = rect.offsetMax = Vector2.zero;
                var image = node.AddComponent<Image>();
                image.sprite = MosseloraArt.BoldRing();
                image.type = Image.Type.Simple;
                image.preserveAspect = true;
                image.color = MosseloraTheme.EarthInk;
                image.raycastTarget = false;
                mark = node.transform;
            }
            mark.gameObject.SetActive(on);
        }

        // ---- Colour-blind pass (checklist 92): a selected, hinted or snug-fit cell also wears a ring.
        public const string FocusMarkName = "Focus mark";
        public const string FitMarkName = "Fit mark";

        /// <summary>
        /// The shape cue for a cell that is selected, hinted or a snug fit: a full-cell ring, shown
        /// only while <paramref name="on"/>. Created on first use, then shown or hidden.
        /// </summary>
        public static void MarkRing(Image cell, bool on, string name = FocusMarkName)
        {
            if (cell == null) return;
            Transform mark = cell.transform.Find(name);
            if (mark == null)
            {
                if (!on) return;
                var node = new GameObject(name, typeof(RectTransform));
                node.transform.SetParent(cell.transform, false);
                node.AddComponent<LayoutElement>().ignoreLayout = true;
                var rect = (RectTransform)node.transform;
                rect.anchorMin = new Vector2(0.06f, 0.06f);
                rect.anchorMax = new Vector2(0.94f, 0.94f);
                rect.offsetMin = rect.offsetMax = Vector2.zero;
                var image = node.AddComponent<Image>();
                image.sprite = MosseloraArt.BoldRing();
                image.type = Image.Type.Simple;
                image.preserveAspect = true;
                image.color = MosseloraTheme.EarthInk;
                image.raycastTarget = false;
                mark = node.transform;
            }
            mark.gameObject.SetActive(on);
        }

        // ---- Step 39: undo and redo, in a row under the board (the top row is full on a narrow
        // phone). Between the two tokens, the number of undo steps left. Session only, never saved.
        public const float UndoRowHeight = 52f;
        public const float UndoRowSpace = UndoRowHeight + 12f; // the row and the card's spacing

        /// <summary>The two tokens and their count, kept up to date by <see cref="UndoRow.Refresh"/>.</summary>
        internal sealed class UndoRow
        {
            private readonly IBoardHistory history;
            private readonly Action changed;
            private readonly Color tone = MosseloraTheme.QuietAction;
            private Transform undoToken, redoToken;
            private Text count;

            internal UndoRow(IBoardHistory history, Action changed)
            {
                this.history = history;
                this.changed = changed;
            }

            /// <summary>Builds the row at the foot of the card. <paramref name="changed"/> repaints the board.</summary>
            internal static UndoRow Build(Transform card, Font font, IBoardHistory history, Action changed)
            {
                var row = new GameObject("Undo and redo", typeof(RectTransform));
                row.transform.SetParent(card, false);
                var layout = row.AddComponent<LayoutElement>();
                layout.preferredHeight = layout.minHeight = UndoRowHeight;
                var group = row.AddComponent<HorizontalLayoutGroup>();
                group.spacing = 10f;
                group.childAlignment = TextAnchor.MiddleCenter;
                group.childControlWidth = group.childControlHeight = true;
                group.childForceExpandWidth = group.childForceExpandHeight = false;

                var ui = new UndoRow(history, changed);
                card.GetComponentInParent<TreePlay>()?.Track(history);
                Sprite glyph = MosseloraArt.Load(MosseloraArt.IconUndo);
                TreePlay.Token(row.transform, "Undo", glyph, UndoRowHeight, ui.tone, () => ui.DoUndo(), true);

                var steps = new GameObject("Undo steps", typeof(RectTransform));
                steps.transform.SetParent(row.transform, false);
                var stepsLayout = steps.AddComponent<LayoutElement>();
                stepsLayout.minWidth = stepsLayout.preferredWidth = 36f;
                stepsLayout.minHeight = stepsLayout.preferredHeight = UndoRowHeight;
                ui.count = Number(steps.transform, "Step count", "0", font, 24, MosseloraTheme.EarthInk);

                TreePlay.Token(row.transform, "Redo", glyph, UndoRowHeight, ui.tone, () => ui.DoRedo(), true);
                // The redo arrow is the undo arrow turned round: one glyph, two jobs.
                Transform redoIcon = row.transform.GetChild(2).Find("Icon");
                if (redoIcon != null) redoIcon.localScale = new Vector3(-1f, 1f, 1f);

                ui.undoToken = row.transform.GetChild(0);
                ui.redoToken = row.transform.GetChild(2);
                return ui;
            }

            /// <summary>Spoken captions are the token names: "Undo, 3 steps", "Redo, 1 step".</summary>
            internal void Refresh(bool locked)
            {
                if (undoToken == null || count == null) return;
                int undoSteps = history.UndoSteps, redoSteps = history.RedoSteps;
                undoToken.name = "Undo, " + Steps(undoSteps, "nothing to undo");
                redoToken.name = "Redo, " + Steps(redoSteps, "nothing to redo");
                count.text = undoSteps.ToString();
                Style(undoToken, !locked && undoSteps > 0);
                Style(redoToken, !locked && redoSteps > 0);
            }

            private static string Steps(int n, string none)
            {
                return n == 0 ? none : n == 1 ? "1 step" : n + " steps";
            }

            // A token that cannot act looks faded and refuses taps; it is never hidden, so the row keeps its place.
            private void Style(Transform token, bool on)
            {
                var button = token.GetComponent<Button>();
                if (button != null) button.interactable = on;
                var image = token.GetComponent<Image>();
                if (image != null) image.color = on ? tone : new Color(tone.r, tone.g, tone.b, tone.a * 0.35f);
                Transform icon = token.Find("Icon");
                if (icon != null) icon.GetComponent<Image>().color = on ? Color.white : new Color(1f, 1f, 1f, 0.4f);
            }

            // Undo and redo hand the turn back with the history (taking turns). A board not on a TreePlay has no owner.
            private void DoUndo()
            {
                if (!history.Undo()) return;
                changed();
                Owner()?.TurnToStep();
            }

            private void DoRedo()
            {
                if (!history.Redo()) return;
                changed();
                Owner()?.TurnToStep();
            }

            private TreePlay Owner()
            {
                return undoToken == null ? null : undoToken.GetComponentInParent<TreePlay>();
            }
        }

        /// <summary>Room for a board under the card's top row: width and height in points.</summary>
        public static Vector2 Room(float width, float screenHeight, float below = 0f)
        {
            return new Vector2(width - CardPadding - BedPadding * 2f,
                Mathf.Max(160f, screenHeight - 176f - below - BedPadding * 2f));
        }

        /// <summary>The earth-toned bed, sized exactly; children are placed by hand (top-left origin).</summary>
        public static RectTransform Bed(Transform card, string name, Vector2 inner)
        {
            var bed = new GameObject(name, typeof(RectTransform));
            bed.transform.SetParent(card, false);
            var layout = bed.AddComponent<LayoutElement>();
            layout.preferredHeight = layout.minHeight = inner.y + BedPadding * 2f;
            var image = bed.AddComponent<Image>();
            MosseloraArt.Round(image, BedFill, 18);
            image.raycastTarget = false;
            var holder = new GameObject("Board", typeof(RectTransform));
            holder.transform.SetParent(bed.transform, false);
            var rect = (RectTransform)holder.transform;
            rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = inner;
            return rect;
        }

        /// <summary>A paper cell at (x, y) from the board's top-left, that calls <paramref name="tap"/>.</summary>
        public static Image Cell(RectTransform board, string name, Vector2 at, Vector2 size, Action tap, int radius = 8)
        {
            var node = new GameObject(name, typeof(RectTransform));
            node.transform.SetParent(board, false);
            var rect = (RectTransform)node.transform;
            rect.anchorMin = rect.anchorMax = new Vector2(0f, 1f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = size;
            rect.anchoredPosition = new Vector2(at.x + size.x / 2f, -at.y - size.y / 2f);
            var image = node.AddComponent<Image>();
            MosseloraArt.PaperTile(image, radius);
            image.color = Soil;
            if (tap != null)
            {
                var button = node.AddComponent<Button>();
                button.targetGraphic = image;
                button.transition = Selectable.Transition.None;
                button.onClick.AddListener(() => tap());
                PressSquash.Add(node);
                LongPressCaption.Attach(node, name, LongPressCaption.DefaultFont);
            }
            else image.raycastTarget = false;
            return image;
        }

        /// <summary>A picture inside a cell, inset; null sprite hides it.</summary>
        public static Image Glyph(Transform cell, string name, Sprite sprite, float inset)
        {
            var node = new GameObject(name, typeof(RectTransform));
            node.transform.SetParent(cell, false);
            var rect = (RectTransform)node.transform;
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = new Vector2(inset, inset);
            rect.offsetMax = new Vector2(-inset, -inset);
            var image = node.AddComponent<Image>();
            image.sprite = sprite;
            image.preserveAspect = true;
            image.raycastTarget = false;
            image.enabled = sprite != null;
            return image;
        }

        /// <summary>Bold numbers for clues: digits read in every language.</summary>
        public static Text Number(Transform parent, string name, string words, Font font, int size, Color color,
                                  TextAnchor anchor = TextAnchor.MiddleCenter)
        {
            var node = new GameObject(name, typeof(RectTransform));
            node.transform.SetParent(parent, false);
            var rect = (RectTransform)node.transform;
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = rect.offsetMax = Vector2.zero;
            var text = node.AddComponent<Text>();
            text.font = font;
            text.fontSize = size;
            text.fontStyle = FontStyle.Bold;
            text.color = color;
            text.alignment = anchor;
            text.horizontalOverflow = HorizontalWrapMode.Overflow;
            text.verticalOverflow = VerticalWrapMode.Overflow;
            text.raycastTarget = false;
            text.text = words;
            return text;
        }

        /// <summary>A free-standing label at a spot on the board (clues sit outside cells).</summary>
        public static Text Label(RectTransform board, string name, string words, Vector2 at, Vector2 size, Font font,
                                 int fontSize, Color color, TextAnchor anchor)
        {
            var node = new GameObject(name, typeof(RectTransform));
            node.transform.SetParent(board, false);
            var rect = (RectTransform)node.transform;
            rect.anchorMin = rect.anchorMax = new Vector2(0f, 1f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = size;
            rect.anchoredPosition = new Vector2(at.x + size.x / 2f, -at.y - size.y / 2f);
            Text text = Number(node.transform, "Words", words, font, fontSize, color, anchor);
            return text;
        }

        // ---- I254: the first-time picture rule, in icons, above a board the first few times.
        public const float RuleHeight = 52f;
        private const int RuleShows = 3;

        /// <summary>
        /// Draws the kind's picture rule the first <c>RuleShows</c> times it's met; returns the
        /// height it took (0 once it's familiar). Counted per kind on the device, not in the save.
        /// </summary>
        public static float FirstTimeRule(Transform card, string kind, Action<Transform> parts)
        {
            string key = "evergrove:rule-seen-" + kind;
            int seen = PlayerPrefs.GetInt(key, 0);
            if (seen >= RuleShows || parts == null) return 0f;
            PlayerPrefs.SetInt(key, seen + 1);
            PlayerPrefs.Save();
            var strip = new GameObject("How it works", typeof(RectTransform));
            strip.transform.SetParent(card, false);
            var layout = strip.AddComponent<LayoutElement>();
            layout.preferredHeight = layout.minHeight = RuleHeight;
            var image = strip.AddComponent<Image>();
            MosseloraArt.Round(image, new Color(0.98f, 0.95f, 0.86f, 1f), 18);
            image.raycastTarget = false;
            var row = strip.AddComponent<HorizontalLayoutGroup>();
            row.padding = new RectOffset(12, 12, 6, 6);
            row.spacing = 6f;
            row.childAlignment = TextAnchor.MiddleCenter;
            row.childControlWidth = row.childControlHeight = true;
            row.childForceExpandWidth = row.childForceExpandHeight = false;
            parts(strip.transform);
            return RuleHeight + 12f; // the card's spacing too
        }

        public static void RulePicture(Transform row, Sprite sprite, float size = 34f, Color? tint = null)
        {
            var node = new GameObject("Picture", typeof(RectTransform));
            node.transform.SetParent(row, false);
            var layout = node.AddComponent<LayoutElement>();
            layout.preferredWidth = layout.preferredHeight = size;
            var image = node.AddComponent<Image>();
            image.sprite = sprite;
            image.preserveAspect = true;
            image.raycastTarget = false;
            image.color = tint ?? Color.white;
        }

        public static void RuleSquare(Transform row, Color color, Sprite inside = null, float size = 30f)
        {
            var node = new GameObject("Square", typeof(RectTransform));
            node.transform.SetParent(row, false);
            var layout = node.AddComponent<LayoutElement>();
            layout.preferredWidth = layout.preferredHeight = size;
            var image = node.AddComponent<Image>();
            MosseloraArt.PaperTile(image, 6);
            image.color = color;
            image.raycastTarget = false;
            if (inside != null) Glyph(node.transform, "Inside", inside, size * 0.12f);
        }

        public static void RuleNumber(Transform row, string words, Font font)
        {
            var node = new GameObject("Number", typeof(RectTransform));
            node.transform.SetParent(row, false);
            var layout = node.AddComponent<LayoutElement>();
            layout.preferredWidth = 30f;
            layout.preferredHeight = 34f;
            Number(node.transform, "Words", words, font, 22, MosseloraTheme.EarthInk);
        }

        /// <summary>A wider pause between two parts of one picture rule.</summary>
        public static void RuleGap(Transform row, float width = 22f)
        {
            var gap = new GameObject("Gap", typeof(RectTransform), typeof(LayoutElement));
            gap.transform.SetParent(row, false);
            var layout = gap.GetComponent<LayoutElement>();
            layout.minWidth = width;
            layout.preferredWidth = width;
        }

        public static void RuleArrow(Transform row)
        {
            RulePicture(row, MosseloraArt.Load(MosseloraArt.IconContinue), 24f, MosseloraTheme.EarthInk);
        }

        /// <summary>A gentle "not like that" nudge: a quick sideways shiver, no sound, no penalty.</summary>
        public static void Shiver(GameObject node)
        {
            if (node != null && node.GetComponent<ShiverOnce>() == null) node.AddComponent<ShiverOnce>(); // never stacks
        }

        private sealed class ShiverOnce : MonoBehaviour
        {
            private float clock;
            private Vector3 home;
            private void Start() { home = transform.localPosition; }
            private void Update()
            {
                clock += Time.unscaledDeltaTime;
                float t = clock / 0.35f;
                if (t >= 1f) { transform.localPosition = home; Destroy(this); return; }
                transform.localPosition = home + new Vector3(Mathf.Sin(t * Mathf.PI * 4f) * 5f * (1f - t), 0f, 0f);
            }
        }
    }
}
