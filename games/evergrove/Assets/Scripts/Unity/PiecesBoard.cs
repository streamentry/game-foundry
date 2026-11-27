using Evergrove.Core;
using UnityEngine;
using UnityEngine.UI;

namespace Evergrove.UnityClient
{
    /// <summary>
    /// I331 (idea 9, after the seven-piece tangram of China, made square for big fingers):
    /// garden pieces. Fill the silhouette exactly with the pieces in the tray. Tap a piece,
    /// turn it (and, deeper, mirror it), then tap where it should lie: it settles over that
    /// square if it fits. Tap a laid piece to take it back. No timer, no wrong answers kept.
    /// </summary>
    internal sealed class PiecesBoard : TreePlay.IPuzzleBoard
    {
        private readonly GardenPiecesGame game;
        private readonly bool canMirror;
        private TreePlay play;
        private Image[] cells;
        private RectTransform[] slots;
        private int selected;
        private bool locked;
        private int shownPiece = -1;
        private float thumb;
        private static readonly Color[] Colours = {
            new Color(0.62f, 0.78f, 0.55f, 1f), new Color(0.93f, 0.76f, 0.42f, 1f), new Color(0.56f, 0.72f, 0.86f, 1f),
            new Color(0.86f, 0.60f, 0.64f, 1f), new Color(0.74f, 0.64f, 0.86f, 1f), new Color(0.88f, 0.66f, 0.44f, 1f),
            new Color(0.58f, 0.80f, 0.76f, 1f)
        };

        public PiecesBoard(GardenPiecesGame game, int tier)
        {
            this.game = game;
            canMirror = tier >= 1;
        }

        public void Build(Transform card, float width, float screenHeight, Font font, TreePlay host)
        {
            play = host;
            float rule = BoardKit.FirstTimeRule(card, "pieces", row =>
            {
                BoardKit.RuleSquare(row, Colours[0]);
                BoardKit.RuleSquare(row, Colours[1]);
                BoardKit.RuleArrow(row);
                BoardKit.RuleSquare(row, BoardKit.Soil);
                BoardKit.RuleSquare(row, BoardKit.Soil);
                BoardKit.RuleArrow(row);
                BoardKit.RulePicture(row, MosseloraArt.Load(MosseloraArt.IconConfirm), 28f, MosseloraTheme.ForestAction);
            });
            const float trayHeight = 150f;
            Vector2 room = BoardKit.Room(width, screenHeight, trayHeight + rule);
            float cell = Mathf.Min(Mathf.Floor(Mathf.Min(room.x / game.Width, room.y / game.Height)), 58f);
            const float gap = 2f;
            RectTransform board = BoardKit.Bed(card, "Garden pieces", new Vector2(game.Width * cell, game.Height * cell));
            cells = new Image[game.Width * game.Height];
            for (int c = 0; c < cells.Length; c++)
            {
                if (!game.InShape(c)) continue;
                int index = c;
                cells[c] = BoardKit.Cell(board, "Square row " + (c / game.Width + 1) + " column " + (c % game.Width + 1),
                    new Vector2((c % game.Width) * cell + gap / 2f, (c / game.Width) * cell + gap / 2f),
                    new Vector2(cell - gap, cell - gap), () => TapCell(index), 6);
            }
            // The tray: each piece drawn small; then turn and mirror.
            Transform tray = TreePlay.Row(card, "Piece tray", 84f);
            tray.GetComponent<HorizontalLayoutGroup>().spacing = 6f;
            thumb = Mathf.Floor(Mathf.Min(14f, (width - BoardKit.CardPadding) / (game.PieceCount * 4.2f)));
            slots = new RectTransform[game.PieceCount];
            for (int p = 0; p < game.PieceCount; p++)
            {
                int piece = p;
                var slot = new GameObject("Piece " + (p + 1), typeof(RectTransform), typeof(LayoutElement));
                slot.transform.SetParent(tray, false);
                var layout = slot.GetComponent<LayoutElement>();
                layout.preferredWidth = layout.minWidth = thumb * 4f;
                layout.preferredHeight = layout.minHeight = thumb * 4f + 8f;
                var hit = slot.AddComponent<Image>();
                MosseloraArt.Round(hit, new Color(1f, 1f, 1f, 0f), 10);
                var button = slot.AddComponent<Button>();
                button.transition = Selectable.Transition.None;
                button.onClick.AddListener(() => { if (!locked) { selected = piece; Paint(); } });
                slots[p] = (RectTransform)slot.transform;
            }
            Transform tools = TreePlay.Row(card, "Turn and mirror", 60f);
            tools.GetComponent<HorizontalLayoutGroup>().spacing = 12f;
            TreePlay.Token(tools, "Turn the piece", MosseloraArt.Load(MosseloraArt.IconTurn), 56f, MosseloraTheme.QuietAction,
                () => { if (!locked && game.Rotate(selected) == PieceMove.Done) { play.Moved(); Paint(); } }, true);
            if (canMirror)
                TreePlay.Token(tools, "Mirror the piece", MosseloraArt.Load(MosseloraArt.IconChange), 56f, MosseloraTheme.QuietAction,
                    () => { if (!locked && game.Mirror(selected) == PieceMove.Done) { play.Moved(); Paint(); } }, true);
            Paint();
        }

        // Tap a laid piece to take it back; tap an empty square to lay the chosen piece over it.
        private void TapCell(int cell)
        {
            if (locked) return;
            int owner = game.Owner(cell);
            if (owner >= 0)
            {
                if (game.Lift(owner) == PieceMove.Done) { selected = owner; play.Moved(); Paint(); }
                return;
            }
            if (selected < 0 || game.Placed(selected)) return;
            if (!TryLay(selected, cell)) { BoardKit.Shiver(cells[cell].gameObject); return; }
            play.Moved();
            NextInTray();
            Paint();
            if (game.Solved) play.Done();
        }

        // The tapped square may be any of the piece's squares: try each as the one tapped.
        private bool TryLay(int piece, int cell)
        {
            int[] offsets = game.PieceCells(piece);
            int x = cell % game.Width, y = cell / game.Width;
            for (int k = 0; k + 1 < offsets.Length; k += 2)
            {
                int ax = x - offsets[k], ay = y - offsets[k + 1];
                if (ax < 0 || ay < 0 || ax >= game.Width || ay >= game.Height) continue;
                if (game.Place(piece, ay * game.Width + ax) == PieceMove.Done) return true;
            }
            return false;
        }

        private void NextInTray()
        {
            for (int p = 0; p < game.PieceCount; p++) if (!game.Placed(p)) { selected = p; return; }
        }

        private void Paint()
        {
            for (int c = 0; c < cells.Length; c++)
            {
                if (cells[c] == null) continue;
                int owner = game.Owner(c);
                cells[c].color = owner >= 0 ? Colours[owner % Colours.Length] : BoardKit.Soil;
            }
            for (int p = 0; p < slots.Length; p++)
            {
                for (int i = slots[p].childCount - 1; i >= 0; i--) Object.Destroy(slots[p].GetChild(i).gameObject);
                bool inTray = !game.Placed(p);
                slots[p].GetComponent<Image>().color = inTray && p == selected && !locked
                    ? new Color(0.98f, 0.93f, 0.74f, 1f) : p == shownPiece ? BoardKit.Picked : new Color(1f, 1f, 1f, 0f);
                if (!inTray) continue;
                int[] offsets = game.PieceCells(p);
                int minX = 0;
                for (int k = 0; k + 1 < offsets.Length; k += 2) minX = Mathf.Min(minX, offsets[k]);
                for (int k = 0; k + 1 < offsets.Length; k += 2)
                {
                    var square = new GameObject("Square", typeof(RectTransform));
                    square.transform.SetParent(slots[p], false);
                    var rect = (RectTransform)square.transform;
                    rect.anchorMin = rect.anchorMax = new Vector2(0f, 1f);
                    rect.sizeDelta = new Vector2(thumb - 1f, thumb - 1f);
                    rect.anchoredPosition = new Vector2(4f + (offsets[k] - minX + 0.5f) * thumb, -4f - (offsets[k + 1] + 0.5f) * thumb);
                    var image = square.AddComponent<Image>();
                    image.sprite = MosseloraArt.Rounded(3);
                    image.color = Colours[p % Colours.Length];
                    image.raycastTarget = false;
                }
            }
        }

        public void Hint()
        {
            if (locked) return;
            GardenHint hint = game.Hint();
            if (!hint.Has) return;
            if (hint.Piece != shownPiece)
            {
                shownPiece = hint.Piece;
                selected = hint.Piece;
                Paint();
                if (cells[hint.Cell] != null) BloomPulse.Add(cells[hint.Cell].gameObject, 0f, 0.18f);
                return;
            }
            shownPiece = -1;
            if (hint.Misplaced) { game.Lift(hint.Piece); play.Moved(); Paint(); return; }
            for (int guard = 0; guard < 2 && game.Mirrored(hint.Piece) != hint.Mirrored; guard++) game.Mirror(hint.Piece);
            for (int guard = 0; guard < 4 && game.Turn(hint.Piece) != hint.Turn; guard++) game.Rotate(hint.Piece);
            game.Place(hint.Piece, hint.Cell);
            play.Moved();
            NextInTray();
            Paint();
            if (game.Solved) play.Done();
        }

        public void Lock()
        {
            locked = true;
            Paint();
            if (!play.Gentle) return;
            for (int c = 0; c < cells.Length; c++)
                if (cells[c] != null) BloomPulse.Add(cells[c].gameObject, 0.04f + (game.Owner(c) % 7) * 0.05f);
        }

        public void QaSolve()
        {
            for (int guard = 0; guard < 40 && !game.Solved; guard++)
            {
                shownPiece = -1;
                GardenHint hint = game.Hint();
                if (!hint.Has) break;
                shownPiece = hint.Piece;
                Hint();
            }
        }
    }
}
