using System;
using System.Collections.Generic;

namespace Evergrove.Core
{
    /// <summary>
    /// I373 (run 3, P14: early on the green fit glow covered nearly the whole board, so it said
    /// nothing): the snug fits for a piece as it is turned now. A fit is snug when its squares
    /// press against the most edges (the silhouette's border or pieces already down), the way a
    /// careful player fills a board from its corners in. Fits that would leave an empty pocket
    /// smaller than every piece still in the tray are never snug: nothing could fill it.
    /// It points at good places, never at the answer.
    /// </summary>
    public static class GardenPiecesFit
    {
        /// <summary>Squares covered by the chosen piece's snug fits; all false when none.</summary>
        public static bool[] Snug(GardenPiecesGame game, int piece)
        {
            if (game == null) return new bool[0];
            int size = game.Width * game.Height;
            var glow = new bool[size];
            if (piece < 0 || piece >= game.PieceCount || game.Placed(piece)) return glow;
            int[] offsets = game.PieceCells(piece);
            int smallest = SmallestOtherPiece(game, piece);
            int best = -1;
            var bestAnchors = new List<int>();
            for (int anchor = 0; anchor < size; anchor++)
            {
                if (!game.CanPlace(piece, anchor)) continue;
                bool[] covered = Cover(game, offsets, anchor);
                if (LeavesDeadPocket(game, covered, smallest)) continue;
                int score = Pressed(game, covered);
                if (score > best) { best = score; bestAnchors.Clear(); }
                if (score == best) bestAnchors.Add(anchor);
            }
            foreach (int anchor in bestAnchors)
            {
                bool[] covered = Cover(game, offsets, anchor);
                for (int c = 0; c < size; c++) if (covered[c]) glow[c] = true;
            }
            return glow;
        }

        /// <summary>Edges of the covered squares that touch the border or a square already taken.</summary>
        public static int Pressed(GardenPiecesGame game, bool[] covered)
        {
            int score = 0, w = game.Width, h = game.Height;
            for (int c = 0; c < covered.Length; c++)
            {
                if (!covered[c]) continue;
                int x = c % w, y = c / w;
                score += Closed(game, covered, x - 1, y) + Closed(game, covered, x + 1, y)
                       + Closed(game, covered, x, y - 1) + Closed(game, covered, x, y + 1);
            }
            return score;
        }

        /// <summary>Whether laying these squares leaves an empty pocket smaller than <paramref name="smallest"/>.</summary>
        public static bool LeavesDeadPocket(GardenPiecesGame game, bool[] covered, int smallest)
        {
            int w = game.Width, h = game.Height;
            var seen = new bool[w * h];
            var stack = new Stack<int>();
            for (int start = 0; start < seen.Length; start++)
            {
                if (seen[start] || !Empty(game, covered, start)) continue;
                int count = 0;
                seen[start] = true;
                stack.Push(start);
                while (stack.Count > 0)
                {
                    int c = stack.Pop();
                    count++;
                    int x = c % w, y = c / w;
                    int[] next = { x > 0 ? c - 1 : -1, x < w - 1 ? c + 1 : -1, y > 0 ? c - w : -1, y < h - 1 ? c + w : -1 };
                    foreach (int n in next)
                        if (n >= 0 && !seen[n] && Empty(game, covered, n)) { seen[n] = true; stack.Push(n); }
                }
                if (count < smallest) return true;
            }
            return false;
        }

        private static int SmallestOtherPiece(GardenPiecesGame game, int piece)
        {
            int smallest = int.MaxValue;
            for (int p = 0; p < game.PieceCount; p++)
                if (p != piece && !game.Placed(p)) smallest = Math.Min(smallest, game.PieceCells(p).Length / 2);
            return smallest == int.MaxValue ? 1 : smallest; // the last piece: any pocket left is unfillable
        }

        public static bool[] Cover(GardenPiecesGame game, int[] offsets, int anchor)
        {
            var covered = new bool[game.Width * game.Height];
            int ax = anchor % game.Width, ay = anchor / game.Width;
            for (int k = 0; k + 1 < offsets.Length; k += 2)
                covered[(ay + offsets[k + 1]) * game.Width + ax + offsets[k]] = true;
            return covered;
        }

        private static bool Empty(GardenPiecesGame game, bool[] covered, int c)
        {
            return game.InShape(c) && game.Owner(c) < 0 && !covered[c];
        }

        private static int Closed(GardenPiecesGame game, bool[] covered, int x, int y)
        {
            if (x < 0 || y < 0 || x >= game.Width || y >= game.Height) return 1;
            int c = y * game.Width + x;
            if (covered[c]) return 0;
            return !game.InShape(c) || game.Owner(c) >= 0 ? 1 : 0;
        }
    }
}
