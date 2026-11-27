using Evergrove.Core;
using UnityEngine;
using UnityEngine.UI;

namespace Evergrove.UnityClient
{
    /// <summary>
    /// I243 — planting by numbers (nonogram). Numbers beside each row and above each
    /// column say how many plants grow there in unbroken runs. Tap a bed to plant a
    /// sprout, again to mark "nothing grows here", again to clear. A row's numbers fade
    /// once the row is right. When every plant is in, the picture is a garden bed.
    /// </summary>
    internal sealed class NonogramBoard : TreePlay.IPuzzleBoard
    {
        private readonly NonogramGame game;
        private TreePlay play;
        private Image[] cells;
        private Image[] sprouts;
        private Image[] marks;
        private Text[] rowClues, columnClues;
        private Sprite sprout, mark;
        private bool locked;
        private int shown = -1; // the hinted cell, waiting for a second tap (I253)

        public NonogramBoard(NonogramGame game) { this.game = game; }

        public void Build(Transform card, float width, float screenHeight, Font font, TreePlay host)
        {
            play = host;
            int n = game.Size;
            int rowRuns = 1, columnRuns = 1;
            for (int i = 0; i < n; i++)
            {
                rowRuns = Mathf.Max(rowRuns, game.RowClues[i].Length);
                columnRuns = Mathf.Max(columnRuns, game.ColumnClues[i].Length);
            }
            Sprite sproutArt = MosseloraArt.Load(TreePlay.SeedIcon);
            float rule = BoardKit.FirstTimeRule(card, "nonogram", row =>
            {
                BoardKit.RuleNumber(row, "3", font);
                BoardKit.RuleArrow(row);
                for (int k = 0; k < 3; k++) BoardKit.RuleSquare(row, BoardKit.Leafy, sproutArt);
                BoardKit.RuleSquare(row, BoardKit.Soil);
            });
            Vector2 room = BoardKit.Room(width, screenHeight, rule);
            // Clues get about 0.55 of a cell per number; cells take the rest.
            float cell = Mathf.Floor(Mathf.Min(room.x / (n + rowRuns * 0.55f), room.y / (n + columnRuns * 0.6f)));
            cell = Mathf.Min(cell, 56f);
            float gap = cell >= 36f ? 3f : 2f;
            float clueW = Mathf.Ceil(rowRuns * cell * 0.55f), clueH = Mathf.Ceil(columnRuns * cell * 0.6f);
            float side = n * cell;
            RectTransform board = BoardKit.Bed(card, "Planting by numbers", new Vector2(clueW + side, clueH + side));
            int clueSize = Mathf.Clamp(Mathf.RoundToInt(cell * 0.42f), 14, 22);
            sprout = MosseloraArt.Load(TreePlay.SeedIcon);
            mark = MosseloraArt.Load(MosseloraArt.Sprig);
            rowClues = new Text[n];
            columnClues = new Text[n];
            for (int i = 0; i < n; i++)
            {
                rowClues[i] = BoardKit.Label(board, "Row " + (i + 1) + " numbers", Runs(game.RowClues[i], "  "),
                    new Vector2(0f, clueH + i * cell), new Vector2(clueW - 6f, cell), font, clueSize,
                    MosseloraTheme.EarthInk, TextAnchor.MiddleRight);
                columnClues[i] = BoardKit.Label(board, "Column " + (i + 1) + " numbers", Runs(game.ColumnClues[i], "\n"),
                    new Vector2(clueW + i * cell, 0f), new Vector2(cell, clueH - 4f), font, clueSize,
                    MosseloraTheme.EarthInk, TextAnchor.LowerCenter);
                columnClues[i].lineSpacing = 0.9f;
            }
            cells = new Image[n * n];
            sprouts = new Image[n * n];
            marks = new Image[n * n];
            for (int c = 0; c < n * n; c++)
            {
                int index = c;
                Vector2 at = new Vector2(clueW + (c % n) * cell + gap / 2f, clueH + (c / n) * cell + gap / 2f);
                cells[c] = BoardKit.Cell(board, "Bed row " + (c / n + 1) + " column " + (c % n + 1), at,
                    new Vector2(cell - gap, cell - gap), () => Tap(index), 6);
                sprouts[c] = BoardKit.Glyph(cells[c].transform, "Sprout", sprout, cell * 0.14f);
                marks[c] = BoardKit.Glyph(cells[c].transform, "Nothing here", mark, cell * 0.3f);
            }
            Paint();
        }

        private static string Runs(int[] runs, string between)
        {
            if (runs.Length == 0) return "0";
            var parts = new string[runs.Length];
            for (int i = 0; i < runs.Length; i++) parts[i] = runs[i].ToString();
            return string.Join(between, parts);
        }

        private void Tap(int cell)
        {
            if (locked) return;
            if (game.Tap(cell) == NonogramMove.Ignored) return;
            shown = -1;
            play.Moved();
            Paint();
            if (game.Solved) play.Done();
        }

        private void Paint()
        {
            int n = game.Size;
            for (int c = 0; c < n * n; c++)
            {
                CellState state = game.State(c);
                cells[c].color = state == CellState.Filled ? BoardKit.Leafy : BoardKit.Soil;
                sprouts[c].enabled = state == CellState.Filled && sprout != null;
                marks[c].enabled = state == CellState.Marked && mark != null;
                marks[c].color = new Color(0.45f, 0.5f, 0.4f, 0.55f);
            }
            for (int i = 0; i < n; i++)
            {
                // A row or column that is right steps back, so the eye goes where work remains.
                rowClues[i].color = game.RowDone(i) ? new Color(0.36f, 0.33f, 0.26f, 0.35f) : MosseloraTheme.EarthInk;
                columnClues[i].color = game.ColumnDone(i) ? new Color(0.36f, 0.33f, 0.26f, 0.35f) : MosseloraTheme.EarthInk;
            }
        }

        public void Hint()
        {
            if (locked) return;
            int cell = game.HintCell();
            if (cell < 0) return;
            if (cell != shown)
            {
                shown = cell;
                cells[cell].color = BoardKit.Picked;
                BloomPulse.Add(cells[cell].gameObject, 0f, 0.18f);
                return;
            }
            shown = -1;
            game.Set(cell, game.Solution(cell) ? CellState.Filled : CellState.Marked);
            play.Moved();
            Paint();
            if (game.Solved) play.Done();
        }

        public void Lock()
        {
            locked = true;
            Paint();
            if (!play.Gentle) return;
            int n = game.Size;
            for (int c = 0; c < n * n; c++)
                if (game.State(c) == CellState.Filled) BloomPulse.Add(cells[c].gameObject, 0.05f + (c / n + c % n) * 0.04f);
        }

        public void QaSolve()
        {
            int n = game.Size;
            for (int c = 0; c < n * n && !game.Solved; c++)
                while (!game.Solved && (game.State(c) == CellState.Filled) != game.Solution(c)) Tap(c);
        }
    }
}
