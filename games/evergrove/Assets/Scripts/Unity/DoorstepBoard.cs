using Evergrove.Core;
using UnityEngine;
using UnityEngine.UI;

namespace Evergrove.UnityClient
{
    /// <summary>
    /// I332 (idea 44, after kolam and rangoli, the symmetric patterns drawn at the threshold
    /// in India): a doorstep pattern that mirrors left to right and top to bottom (and, at the
    /// deepest tier, across the diagonals too). A few marks are given; every mark tells you its
    /// mirror images. Tap a square to draw a petal, again to leave it bare. Faint lines show
    /// the mirrors. A mark that disagrees with its given mirror glows rose.
    /// </summary>
    internal sealed class DoorstepBoard : TreePlay.IPuzzleBoard
    {
        private readonly DoorstepGame game;
        private readonly bool diagonals;
        private TreePlay play;
        private Image[] cells, petals;
        private bool locked;
        private int shownCell = -1;
        private static readonly Color Ground = new Color(0.76f, 0.52f, 0.38f, 1f);   // swept earth
        private static readonly Color Chalk = new Color(0.99f, 0.97f, 0.92f, 1f);    // rice-flour white

        public DoorstepBoard(DoorstepGame game, int tier)
        {
            this.game = game;
            diagonals = tier >= 2;
        }

        public void Build(Transform card, float width, float screenHeight, Font font, TreePlay host)
        {
            play = host;
            int n = game.Size;
            float rule = BoardKit.FirstTimeRule(card, "doorstep", row =>
            {
                // A petal on one side means a petal in its mirror.
                BoardKit.RuleSquare(row, Ground, MosseloraArt.Rounded(12), 34f);
                BoardKit.RulePicture(row, MosseloraArt.Load(MosseloraArt.IconChange), 26f, MosseloraTheme.EarthInk);
                BoardKit.RuleSquare(row, Ground, MosseloraArt.Rounded(12), 34f);
                BoardKit.RuleArrow(row);
                BoardKit.RulePicture(row, MosseloraArt.Load(MosseloraArt.IconConfirm), 28f, MosseloraTheme.ForestAction);
            });
            Vector2 room = BoardKit.Room(width, screenHeight, rule);
            float cell = Mathf.Min(Mathf.Floor(Mathf.Min(room.x, room.y) / n), 64f);
            const float gap = 3f;
            RectTransform board = BoardKit.Bed(card, "Doorstep pattern", new Vector2(n * cell, n * cell));
            cells = new Image[n * n];
            petals = new Image[n * n];
            for (int c = 0; c < n * n; c++)
            {
                int index = c;
                cells[c] = BoardKit.Cell(board, "Square row " + (c / n + 1) + " column " + (c % n + 1),
                    new Vector2((c % n) * cell + gap / 2f, (c / n) * cell + gap / 2f), new Vector2(cell - gap, cell - gap),
                    () => Tap(index), 6);
                petals[c] = BoardKit.Glyph(cells[c].transform, "Petal", MosseloraArt.Rounded(16), cell * 0.2f);
                petals[c].color = Chalk;
                if (game.IsGiven(c)) MosseloraArt.Edge(cells[c].transform, new Color(0.98f, 0.86f, 0.5f, 1f), 6);
            }
            // The mirrors, faint across the board: the pattern's own rule, made visible.
            Line(board, n * cell, true, 0f);
            Line(board, n * cell, false, 0f);
            if (diagonals) { Line(board, n * cell, true, 45f); Line(board, n * cell, true, -45f); }
            BoardKit.ClashLens(card.Find("Tree puzzle top"), Paint);
            Paint();
        }

        private static void Line(RectTransform board, float side, bool across, float angle)
        {
            var node = new GameObject("Mirror", typeof(RectTransform));
            node.transform.SetParent(board, false);
            var rect = (RectTransform)node.transform;
            rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
            float length = angle == 0f ? side : side * 1.414f;
            rect.sizeDelta = across ? new Vector2(length, 2f) : new Vector2(2f, length);
            rect.localRotation = Quaternion.Euler(0f, 0f, angle);
            var image = node.AddComponent<Image>();
            image.color = new Color(1f, 1f, 1f, 0.35f);
            image.raycastTarget = false;
        }

        private void Tap(int cell)
        {
            if (locked) return;
            DoorMove move = game.Tap(cell);
            if (move == DoorMove.Given) { BoardKit.Shiver(cells[cell].gameObject); return; }
            if (move != DoorMove.Changed) return;
            shownCell = -1;
            play.Moved();
            Paint();
            if (game.Solved) play.Done();
        }

        private void Paint()
        {
            for (int c = 0; c < cells.Length; c++)
            {
                CellMark mark = game.State(c);
                bool clash = mark != CellMark.Unknown && c != shownCell && BoardKit.ShowClashes && game.Conflicts(c);
                cells[c].color = c == shownCell ? BoardKit.Picked
                    : mark == CellMark.Unknown ? BoardKit.Soil
                    : clash ? BoardKit.Clash : Ground;
                BoardKit.MarkClash(cells[c], clash);
                petals[c].enabled = mark == CellMark.Drawn;
            }
        }

        public void Hint()
        {
            if (locked || !game.TryHint(out int cell, out CellMark mark)) return;
            if (cell != shownCell)
            {
                shownCell = cell;
                Paint();
                BloomPulse.Add(cells[cell].gameObject, 0f, 0.18f);
                return;
            }
            shownCell = -1;
            for (int guard = 0; guard < 3 && game.State(cell) != mark; guard++) game.Tap(cell);
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
            for (int c = 0; c < cells.Length; c++)
                if (game.State(c) == CellMark.Drawn)
                {
                    float x = c % n - (n - 1) / 2f, y = c / n - (n - 1) / 2f;
                    BloomPulse.Add(cells[c].gameObject, 0.04f + Mathf.Sqrt(x * x + y * y) * 0.06f); // blooms out from the centre
                }
        }

        public void QaSolve()
        {
            for (int c = 0; c < cells.Length && !game.Solved; c++)
            {
                if (game.IsGiven(c)) continue;
                CellMark want = game.Solution(c) ? CellMark.Drawn : CellMark.Bare;
                for (int guard = 0; guard < 3 && game.State(c) != want; guard++) Tap(c);
            }
        }
    }
}
