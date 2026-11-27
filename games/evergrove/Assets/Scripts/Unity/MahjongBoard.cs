using Evergrove.Core;
using UnityEngine;
using UnityEngine.UI;

namespace Evergrove.UnityClient
{
    /// <summary>
    /// I247 — garden tiles (mahjong solitaire). Painted tiles lie in a small terrace.
    /// A tile is free when nothing rests on it and one side is open; blocked tiles sit
    /// a little shaded so you can see which are free. Tap two free tiles with the same
    /// picture to lift them. Tapping a different picture simply moves your choice: no
    /// "wrong", no penalty. If no pair is free, the tiles quietly reshuffle.
    /// </summary>
    internal sealed class MahjongBoard : TreePlay.IPuzzleBoard
    {
        private readonly MahjongGame game;
        private readonly Sprite[] pictures;
        private TreePlay play;
        private Image[] tiles;
        private RectTransform board;
        private int tier;
        private bool locked;
        private int[] shown; // I253

        public MahjongBoard(MahjongGame game, int tier, Sprite[] pictures)
        {
            this.game = game;
            this.tier = tier;
            this.pictures = pictures;
        }

        public void Build(Transform card, float width, float screenHeight, Font font, TreePlay host)
        {
            play = host;
            int count = game.TileCount, maxX = 0, maxY = 0, maxZ = 0;
            for (int t = 0; t < count; t++)
            {
                maxX = Mathf.Max(maxX, game.X(t));
                maxY = Mathf.Max(maxY, game.Y(t));
                maxZ = Mathf.Max(maxZ, game.Z(t));
            }
            // Positions are in half tiles; a tile is 2 x 2 halves, drawn a little taller than wide.
            float rule = BoardKit.FirstTimeRule(card, "mahjong", row =>
            {
                Sprite face = pictures != null && pictures.Length > 0 ? pictures[0] : null;
                BoardKit.RuleSquare(row, BoardKit.Soil, face, 34f);
                BoardKit.RuleSquare(row, BoardKit.Soil, face, 34f);
                BoardKit.RuleArrow(row);
                BoardKit.RulePicture(row, MosseloraArt.Load(MosseloraArt.IconConfirm), 28f, MosseloraTheme.ForestAction);
            });
            Vector2 room = BoardKit.Room(width, screenHeight, rule);
            float lift = 6f; // each layer sits up and left, so the terrace reads in depth
            float half = Mathf.Floor(Mathf.Min((room.x - maxZ * lift) / (maxX + 2), (room.y - maxZ * lift) / ((maxY + 2) * 1.2f)));
            half = Mathf.Min(half, 34f);
            float tileW = half * 2f, tileH = half * 2.4f;
            var inner = new Vector2((maxX + 2) * half + maxZ * lift, (maxY + 2) * half * 1.2f + maxZ * lift);
            board = BoardKit.Bed(card, "Garden tiles", inner);
            tiles = new Image[count];
            // Lower layers first, then top to bottom, left to right, so upper tiles draw over lower.
            var order = new System.Collections.Generic.List<int>();
            for (int t = 0; t < count; t++) order.Add(t);
            order.Sort((a, b) => game.Z(a) != game.Z(b) ? game.Z(a).CompareTo(game.Z(b))
                : game.Y(a) != game.Y(b) ? game.Y(a).CompareTo(game.Y(b)) : game.X(a).CompareTo(game.X(b)));
            foreach (int t in order)
            {
                int tile = t;
                Vector2 at = new Vector2(game.X(t) * half + (maxZ - game.Z(t)) * lift,
                    game.Y(t) * half * 1.2f + (maxZ - game.Z(t)) * lift);
                tiles[t] = BoardKit.Cell(board, "Tile " + (t + 1), at, new Vector2(tileW - 3f, tileH - 3f), () => Tap(tile), 8);
                MosseloraArt.Lift(tiles[t], 2f, 0.22f);
                MosseloraArt.Edge(tiles[t].transform, MosseloraTheme.TileEdge, 8);
                int face = game.Face(t);
                BoardKit.Glyph(tiles[t].transform, "Picture",
                    pictures != null && face < pictures.Length ? pictures[face] : null, half * 0.22f);
            }
            Paint();
        }

        private void Tap(int tile)
        {
            if (locked) return;
            MahjongMove move = game.Select(tile);
            if (move == MahjongMove.Ignored) return;
            if (move == MahjongMove.NotFree) { BoardKit.Shiver(tiles[tile].gameObject); return; }
            play.Moved();
            if (move == MahjongMove.Matched && game.Solved) { Paint(); play.Done(); return; }
            // Stuck with no free pair: reshuffle what's left. If even that can't help (a rare
            // tier-3 table), the bed counts as tended rather than leaving the player stranded.
            if (move == MahjongMove.Matched && !game.HasMove && !game.Reshuffle()) { Paint(); play.Done(); return; }
            Paint();
        }

        private void Paint()
        {
            for (int t = 0; t < tiles.Length; t++)
            {
                bool gone = game.Removed(t);
                tiles[t].gameObject.SetActive(!gone);
                if (gone) continue;
                bool free = game.IsFree(t);
                tiles[t].color = t == game.Selected ? BoardKit.Picked : free ? BoardKit.Soil : new Color(0.80f, 0.78f, 0.72f, 1f);
                tiles[t].transform.localScale = t == game.Selected ? new Vector3(1.06f, 1.06f, 1f) : Vector3.one;
                Image picture = tiles[t].transform.Find("Picture").GetComponent<Image>();
                picture.color = free ? Color.white : new Color(1f, 1f, 1f, 0.75f);
                int face = game.Face(t);
                picture.sprite = pictures != null && face < pictures.Length ? pictures[face] : null;
            }
        }

        public void Hint()
        {
            if (locked) return;
            int[] pair = game.HintPair();
            if (pair.Length < 2) return;
            if (shown == null || shown[0] != pair[0] || shown[1] != pair[1])
            {
                shown = pair;
                Paint();
                foreach (int t in pair)
                {
                    tiles[t].color = BoardKit.Picked;
                    BloomPulse.Add(tiles[t].gameObject, 0f, 0.18f);
                }
                return;
            }
            shown = null;
            if (game.Selected >= 0 && game.Selected != pair[0]) Tap(game.Selected);
            if (game.Selected != pair[0]) Tap(pair[0]);
            Tap(pair[1]);
        }

        public void Lock()
        {
            locked = true;
            Paint();
            // A cleared table isn't an empty one (I247 tune): the pictures you matched bloom
            // in the bed, one of each, like a sampler of the garden.
            if (board == null || pictures == null) return;
            int faces = Mathf.Min(pictures.Length, MahjongGame.FacesFor(tier));
            int across = Mathf.Min(faces, 6), down = (faces + across - 1) / across;
            float size = Mathf.Min(board.rect.width / across, board.rect.height / down) * 0.86f;
            for (int f = 0; f < faces; f++)
            {
                var node = new GameObject("Bloom " + (f + 1), typeof(RectTransform));
                node.transform.SetParent(board, false);
                var rect = (RectTransform)node.transform;
                rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
                rect.sizeDelta = new Vector2(size, size);
                int row = f / across, column = f % across, inRow = Mathf.Min(across, faces - row * across);
                rect.anchoredPosition = new Vector2((column - (inRow - 1) / 2f) * size / 0.86f,
                    ((down - 1) / 2f - row) * size / 0.86f);
                var image = node.AddComponent<Image>();
                image.sprite = pictures[f];
                image.preserveAspect = true;
                image.raycastTarget = false;
                if (play.Gentle) BloomPulse.Add(node, 0.1f + f * 0.08f);
            }
        }

        public void QaSolve()
        {
            for (int guard = 0; guard < tiles.Length && !game.Solved && !locked; guard++)
            {
                int[] pair = game.HintPair();
                if (pair.Length < 2) { if (!game.Reshuffle()) break; Paint(); continue; }
                if (game.Selected >= 0 && game.Selected != pair[0]) Tap(game.Selected); // let go of a stray pick
                Tap(pair[0]);
                Tap(pair[1]);
            }
        }
    }
}
