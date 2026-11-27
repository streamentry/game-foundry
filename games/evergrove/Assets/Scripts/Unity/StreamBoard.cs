using Evergrove.Core;
using UnityEngine;
using UnityEngine.UI;

namespace Evergrove.UnityClient
{
    /// <summary>
    /// I246 — lead the stream (one path). From the spring, tap the next field to lead
    /// the water there, one step at a time, around the stones, until every field is
    /// watered and the stream reaches the pond. Tap any watered field to draw the
    /// stream back to it. The pond only takes the water once every field has some.
    /// </summary>
    internal sealed class StreamBoard : TreePlay.IPuzzleBoard
    {
        private readonly StreamPathGame game;
        private TreePlay play;
        private Image[] cells;
        private Image[][] channels; // per cell: up, right, down, left water bars
        private Image head;
        private bool locked;
        private int shown = -1; // I253

        public StreamBoard(StreamPathGame game) { this.game = game; }

        public void Build(Transform card, float width, float screenHeight, Font font, TreePlay host)
        {
            play = host;
            int n = game.Size;
            float rule = BoardKit.FirstTimeRule(card, "streampath", row =>
            {
                BoardKit.RulePicture(row, MosseloraArt.Load("glyph-waves"), 30f, MosseloraTheme.WaterAction);
                BoardKit.RuleArrow(row);
                for (int k = 0; k < 3; k++) BoardKit.RuleSquare(row, BoardKit.Water);
                BoardKit.RuleArrow(row);
                BoardKit.RulePicture(row, MosseloraArt.Load(MosseloraArt.Droplet), 30f);
            });
            Vector2 room = BoardKit.Room(width, screenHeight, rule);
            float cell = Mathf.Min(Mathf.Floor(Mathf.Min(room.x, room.y) / n), 76f);
            const float gap = 4f;
            RectTransform board = BoardKit.Bed(card, "Lead the stream", new Vector2(n * cell, n * cell));
            Sprite stone = MosseloraArt.Load("stone-summer");
            Sprite spring = MosseloraArt.Load("glyph-waves");
            Sprite pond = MosseloraArt.Load(MosseloraArt.Droplet);
            cells = new Image[n * n];
            channels = new Image[n * n][];
            for (int c = 0; c < n * n; c++)
            {
                int index = c;
                bool rock = game.IsRock(c);
                cells[c] = BoardKit.Cell(board, rock ? "Stone" : "Field row " + (c / n + 1) + " column " + (c % n + 1),
                    new Vector2((c % n) * cell + gap / 2f, (c / n) * cell + gap / 2f), new Vector2(cell - gap, cell - gap),
                    rock ? null : (System.Action)(() => Tap(index)), 10);
                if (rock)
                {
                    cells[c].color = new Color(0.55f, 0.5f, 0.36f, 0.25f);
                    BoardKit.Glyph(cells[c].transform, "Stone", stone, cell * 0.08f);
                    continue;
                }
                channels[c] = new Image[4];
                for (int d = 0; d < 4; d++) channels[c][d] = Bar(cells[c].transform, d, cell - gap);
                if (c == game.Spring) BoardKit.Glyph(cells[c].transform, "Spring", spring, cell * 0.2f).color =
                    MosseloraTheme.WaterAction;
                if (c == game.Pond) BoardKit.Glyph(cells[c].transform, "Pond", pond, cell * 0.18f);
            }
            head = BoardKit.Glyph(cells[game.Spring].transform, "Stream head", pond, cell * 0.32f);
            Paint();
        }

        // A water bar from the cell's centre to one edge (0 up, 1 right, 2 down, 3 left).
        private static Image Bar(Transform cell, int direction, float size)
        {
            var node = new GameObject("Water " + direction, typeof(RectTransform));
            node.transform.SetParent(cell, false);
            var rect = (RectTransform)node.transform;
            rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
            float thick = Mathf.Round(size * 0.26f), half = size / 2f + 2f;
            bool vertical = direction % 2 == 0;
            rect.sizeDelta = vertical ? new Vector2(thick, half) : new Vector2(half, thick);
            rect.pivot = direction == 0 ? new Vector2(0.5f, 0f) : direction == 1 ? new Vector2(0f, 0.5f)
                : direction == 2 ? new Vector2(0.5f, 1f) : new Vector2(1f, 0.5f);
            rect.anchoredPosition = Vector2.zero;
            var image = node.AddComponent<Image>();
            MosseloraArt.Round(image, MosseloraTheme.WaterAction, Mathf.RoundToInt(thick / 2f));
            image.raycastTarget = false;
            image.enabled = false;
            return image;
        }

        private void Tap(int cell)
        {
            if (locked) return;
            StreamPathGame.PathMove move = game.Step(cell);
            if (move == StreamPathGame.PathMove.TooSoon) { BoardKit.Shiver(cells[cell].gameObject); return; }
            if (move == StreamPathGame.PathMove.Ignored) return;
            play.Moved();
            Paint();
            if (game.Solved) play.Done();
        }

        private void Paint()
        {
            int n = game.Size;
            int[] path = game.Path;
            for (int c = 0; c < n * n; c++)
            {
                if (game.IsRock(c)) continue;
                cells[c].color = game.OnPath(c) ? BoardKit.Water : BoardKit.Soil;
                for (int d = 0; d < 4; d++) channels[c][d].enabled = false;
            }
            for (int i = 0; i + 1 < path.Length; i++)
            {
                int a = path[i], b = path[i + 1];
                int d = b == a - n ? 0 : b == a + 1 ? 1 : b == a + n ? 2 : 3;
                channels[a][d].enabled = true;
                channels[b][(d + 2) % 4].enabled = true;
            }
            int tip = path[path.Length - 1];
            head.transform.SetParent(cells[tip].transform, false);
            head.enabled = !locked && tip != game.Pond;
            head.color = new Color(1f, 1f, 1f, 0.9f);
        }

        public void Hint()
        {
            if (locked) return;
            int next = game.NextHint();
            if (next < 0) return;
            if (next != shown)
            {
                shown = next;
                cells[next].color = BoardKit.Picked;
                BloomPulse.Add(cells[next].gameObject, 0f, 0.18f);
                return;
            }
            shown = -1;
            Tap(next);
        }

        public void Lock()
        {
            locked = true;
            Paint();
            if (!play.Gentle) return;
            int[] path = game.Path;
            for (int i = 0; i < path.Length; i++) BloomPulse.Add(cells[path[i]].gameObject, 0.04f + i * 0.04f);
        }

        public void QaSolve()
        {
            int[] solution = game.SolutionPath();
            for (int i = 1; i < solution.Length && !game.Solved; i++) Tap(solution[i]);
        }
    }
}
