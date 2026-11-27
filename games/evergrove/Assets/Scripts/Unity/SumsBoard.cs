using Evergrove.Core;
using UnityEngine;
using UnityEngine.UI;

namespace Evergrove.UnityClient
{
    /// <summary>
    /// I324 (idea 10, after the sum-cross puzzles of Japan, and the studio's own first-game
    /// candidate): seed sums. Each row or column of beds adds up to the number on its marker,
    /// with no number twice in a row. Tap a bed, then a number in the tray; the same number
    /// again lifts it. A number that can't fit glows rose: a hint, never a penalty.
    /// </summary>
    internal sealed class SumsBoard : TreePlay.IPuzzleBoard
    {
        private readonly BoardHistory<SeedSumsGame> history; // step 39: undo and redo
        private SeedSumsGame game { get { return history.Current; } }
        private BoardKit.UndoRow undo;
        private TreePlay play;
        private Font font;
        private Image[] cells;
        private Text[] values;
        private int selected = -1;
        private bool locked;
        private int shownCell = -1, shownDigit;

        public SumsBoard(SeedSumsGame game)
        {
            history = new BoardHistory<SeedSumsGame>(game, () => new SeedSumsGame(game.Seed, game.Tier));
        }

        public void Build(Transform card, float width, float screenHeight, Font uiFont, TreePlay host)
        {
            play = host;
            font = uiFont;
            int w = game.Width, h = game.Height;
            const float trayHeight = 128f; // two rows of large numbers
            float rule = BoardKit.FirstTimeRule(card, "sums", row =>
            {
                // "3 across means two beds that add to 3": 1 and 2.
                BoardKit.RuleSquare(row, BoardKit.Given);
                BoardKit.RuleNumber(row, "3", font);
                BoardKit.RuleArrow(row);
                BoardKit.RuleNumber(row, "1", font);
                BoardKit.RuleNumber(row, "+", font);
                BoardKit.RuleNumber(row, "2", font);
                BoardKit.RuleArrow(row);
                BoardKit.RulePicture(row, MosseloraArt.Load(MosseloraArt.IconConfirm), 28f, MosseloraTheme.ForestAction);
            });
            Vector2 room = BoardKit.Room(width, screenHeight, trayHeight + 12f + rule + BoardKit.UndoRowSpace);
            float cell = Mathf.Min(Mathf.Floor(Mathf.Min(room.x / w, room.y / h)), 70f);
            const float gap = 3f;
            RectTransform board = BoardKit.Bed(card, "Seed sums", new Vector2(w * cell, h * cell));
            cells = new Image[w * h];
            values = new Text[w * h];
            for (int c = 0; c < w * h; c++)
            {
                int index = c;
                Vector2 at = new Vector2((c % w) * cell + gap / 2f, (c / w) * cell + gap / 2f);
                if (!game.IsOpen(c))
                {
                    cells[c] = BoardKit.Cell(board, "Marker", at, new Vector2(cell - gap, cell - gap), null, 6);
                    cells[c].color = new Color(0.55f, 0.5f, 0.36f, 0.35f);
                    int across = game.ClueAcross(c), down = game.ClueDown(c);
                    if (across > 0) Clue(cells[c].transform, across, true, cell);
                    if (down > 0) Clue(cells[c].transform, down, false, cell);
                    continue;
                }
                cells[c] = BoardKit.Cell(board, "Bed row " + (c / w + 1) + " column " + (c % w + 1), at,
                    new Vector2(cell - gap, cell - gap), () => Select(index), 8);
                values[c] = BoardKit.Number(cells[c].transform, "Number", "", font, Mathf.RoundToInt(cell * 0.5f),
                    MosseloraTheme.EarthInk);
            }
            // The tray: 1 to 5, then 6 to 9, big enough for any hand.
            float token = Mathf.Floor(Mathf.Min(58f, (width - BoardKit.CardPadding - 8f * 4f) / 5f));
            for (int r = 0; r < 2; r++)
            {
                Transform tray = TreePlay.Row(card, "Number tray " + (r + 1), token + 4f);
                tray.GetComponent<HorizontalLayoutGroup>().spacing = 8f;
                for (int v = r * 5 + 1; v <= Mathf.Min(9, r * 5 + 5); v++)
                {
                    int value = v;
                    TreePlay.Token(tray, "Number " + v, null, token, MosseloraTheme.QuietAction, () => Put(value), true);
                    BoardKit.Number(tray.GetChild(tray.childCount - 1), "Digit", v.ToString(), font,
                        Mathf.RoundToInt(token * 0.48f), MosseloraTheme.EarthInk);
                }
            }
            for (int c = 0; c < w * h && selected < 0; c++) if (game.IsOpen(c)) selected = c;
            BoardKit.ClashLens(card.Find("Tree puzzle top"), Paint);
            undo = BoardKit.UndoRow.Build(card, uiFont, history, () => { shownCell = -1; Paint(); });
            Paint();
        }

        // A marker's sum: across in its upper right, down in its lower left.
        private void Clue(Transform marker, int sum, bool across, float cell)
        {
            var node = new GameObject(across ? "Across sum" : "Down sum", typeof(RectTransform));
            node.transform.SetParent(marker, false);
            var rect = (RectTransform)node.transform;
            rect.anchorMin = rect.anchorMax = across ? new Vector2(0.72f, 0.72f) : new Vector2(0.28f, 0.28f);
            rect.sizeDelta = new Vector2(cell * 0.5f, cell * 0.45f);
            BoardKit.Number(node.transform, "Sum", sum.ToString(), font, Mathf.RoundToInt(cell * 0.36f),
                across ? MosseloraTheme.ForestAction : MosseloraTheme.EarthInk);
        }

        private void Select(int cell)
        {
            if (locked || !game.IsOpen(cell)) return;
            selected = cell;
            Paint();
        }

        private void Put(int digit)
        {
            if (locked || selected < 0) return;
            if (game.Value(selected) == digit) digit = 0; // the same number again lifts it
            int at = selected; // copied now: a replay runs the same move later, after the selection may have moved
            SumMove move = history.Play(g => g.Place(at, digit), m => m != SumMove.Ignored);
            if (move == SumMove.Ignored) return;
            shownCell = -1;
            play.Moved();
            if (move == SumMove.Conflict) BoardKit.Shiver(cells[selected].gameObject);
            if (digit > 0 && move != SumMove.Conflict)
                for (int step = 1; step < cells.Length; step++)
                {
                    int next = (selected + step) % cells.Length;
                    if (game.IsOpen(next) && game.Value(next) == 0) { selected = next; break; }
                }
            Paint();
            if (game.Solved) play.Done();
        }

        private void Paint()
        {
            for (int c = 0; c < cells.Length; c++)
            {
                if (!game.IsOpen(c)) continue;
                int v = game.Value(c);
                bool clash = v > 0 && c != shownCell && !(c == selected && !locked) && BoardKit.ShowClashes && game.Conflicts(c);
                cells[c].color = c == shownCell ? BoardKit.Picked
                    : c == selected && !locked ? BoardKit.Picked
                    : clash ? BoardKit.Clash : BoardKit.Soil;
                BoardKit.MarkClash(cells[c], clash);
                values[c].text = v > 0 ? v.ToString() : c == shownCell ? shownDigit.ToString() : "";
                values[c].color = c == shownCell && v == 0 ? new Color(0.46f, 0.37f, 0.27f, 0.45f) : MosseloraTheme.EarthInk;
            }
            if (undo != null) undo.Refresh(locked);
        }

        public void Hint()
        {
            if (locked || !game.Hint(out int cell, out int digit)) return;
            if (cell != shownCell)
            {
                shownCell = cell;
                shownDigit = digit;
                selected = cell;
                Paint();
                BloomPulse.Add(cells[cell].gameObject, 0f, 0.18f);
                return;
            }
            shownCell = -1;
            selected = cell;
            if (game.Value(cell) == digit) return;
            history.Play(g => g.Place(cell, digit), m => m != SumMove.Ignored);
            play.Moved();
            Paint();
            if (game.Solved) play.Done();
        }

        public void Lock()
        {
            locked = true;
            Paint();
            if (!play.Gentle) return;
            for (int c = 0; c < cells.Length; c++)
                if (game.IsOpen(c)) BloomPulse.Add(cells[c].gameObject, 0.04f + (c % game.Width + c / game.Width) * 0.04f);
        }

        public void QaSolve()
        {
            for (int c = 0; c < cells.Length && !game.Solved; c++)
                if (game.IsOpen(c) && game.Value(c) != game.Solution(c))
                {
                    selected = c;
                    Put(game.Solution(c));
                }
        }
    }
}
