using System;
using System.Collections.Generic;

namespace Evergrove.Core
{
    /// <summary>What a turn, mirror, place or lift did. Refused moves change nothing.</summary>
    public enum PieceMove { Done, Outside, Overlaps, Ignored }

    /// <summary>
    /// The next step towards the generator's own answer. When <see cref="Misplaced"/> the named
    /// piece is placed somewhere the answer does not have it: lift it. Otherwise the piece is in
    /// the tray: bring it to <see cref="Turn"/> and <see cref="Mirrored"/>, then place it on
    /// <see cref="Cell"/>. <see cref="Has"/> is false once the board is solved.
    /// </summary>
    public struct GardenHint
    {
        public bool Has;
        public bool Misplaced;
        public int Piece;
        public int Turn;
        public bool Mirrored;
        public int Cell;
    }

    /// <summary>
    /// Garden pieces: fill a silhouette exactly with a handful of polyomino pieces (2 to 5 cells).
    /// Pick a piece from the tray, turn it a quarter at a time (mirror it from tier 1), then tap a
    /// silhouette cell to put the piece's anchor cell there. A silhouette is grown by tiling it with
    /// the pieces, so every board can be solved; any exact fill counts. No timer, no penalty.
    /// Cells are indexed y * Width + x within the silhouette's tight bounding box.
    /// </summary>
    public sealed class GardenPiecesGame
    {
        /// <summary>Pieces per tier: 3, 5, 7.</summary>
        private static readonly int[] PieceCounts = { 3, 5, 7 };
        private static readonly int[] MinCells = { 9, 16, 24 };
        private static readonly int[] MaxCells = { 11, 20, 30 };
        private static readonly int[] GridSize = { 5, 6, 8 };
        private const int MinPiece = 2, MaxPiece = 5, MaxAttempts = 20000;

        /// <summary>How many edges beyond the tidiest possible outline a silhouette may have.</summary>
        public const int MaxPerimeterExcess = 4;
        private const int MinFillPercent = 55;

        private readonly int tier;
        private readonly int width, height;
        private readonly bool[] shape;
        private readonly int cellCount;
        private readonly int[][] bases;       // each piece's cells as (x, y) pairs, as it lies in the tray
        private readonly int[] solTurn;
        private readonly bool[] solMirror;
        private readonly int[] solCell;       // anchor cell of each piece in the answer
        private readonly int[][] solCells;    // each piece's covered cells in the answer, ascending

        private readonly int[] turn;
        private readonly bool[] mirrored;
        private readonly int[] placedAt;
        private readonly int[] owner;

        public int Width { get { return width; } }
        public int Height { get { return height; } }
        public int PieceCount { get { return bases.Length; } }
        public int CellCount { get { return cellCount; } }
        public int Moves { get; private set; }
        public bool Solved { get; private set; }
        /// <summary>Unit edges around the silhouette (holes included).</summary>
        public int Perimeter { get; private set; }
        public bool Compact { get { return IsCompact(shape, width, height, cellCount); } }

        /// <summary>The smallest perimeter any shape of this many cells can have.</summary>
        public static int MinPerimeter(int cells)
        {
            int best = int.MaxValue;
            for (int w = 1; w <= cells; w++)
            {
                int h = (cells + w - 1) / w;
                best = Math.Min(best, 2 * (w + h));
            }
            return best;
        }

        public GardenPiecesGame(int seed, int tier)
        {
            if (tier < 0 || tier >= TreePuzzles.Tiers) throw new ArgumentOutOfRangeException(nameof(tier));
            this.tier = tier;
            uint state = unchecked((uint)seed * 0x9E3779B1u + (uint)tier * 0x85EBCA6Bu + 0x165667B1u);
            if (state == 0) state = 0x6d2b79f5u;

            int count = PieceCounts[tier], grid = GridSize[tier];
            bool built = false;
            int[] owners = null;
            for (int attempt = 0; attempt < MaxAttempts && !built; attempt++)
            {
                int[] sizes = DealSizes(ref state, count, MinCells[tier], MaxCells[tier]);
                owners = Grow(ref state, sizes, grid);
                if (owners != null && TightAndNice(owners, grid, out width, out height, out shape, out cellCount))
                    built = true;
            }
            if (!built) throw new InvalidOperationException("no garden pieces board found");

            Perimeter = PerimeterOf(shape, width, height);
            int minX = grid, minY = grid;
            for (int c = 0; c < owners.Length; c++)
                if (owners[c] >= 0) { minX = Math.Min(minX, c % grid); minY = Math.Min(minY, c / grid); }

            bases = new int[count][];
            solTurn = new int[count];
            solMirror = new bool[count];
            solCell = new int[count];
            solCells = new int[count][];
            for (int p = 0; p < count; p++)
            {
                var placed = new List<int>();        // (x, y) pairs in the silhouette box
                for (int c = 0; c < owners.Length; c++)
                    if (owners[c] == p) { placed.Add(c % grid - minX); placed.Add(c / grid - minY); }
                int[] answer = Orient(placed.ToArray(), 0, false);
                // Hand the piece to the tray in a random pose, then find the pose that undoes it.
                int t0 = Next(ref state, 4);
                bool m0 = tier > 0 && Next(ref state, 2) == 1;
                bases[p] = Normalize(Orient(placed.ToArray(), t0, m0), false);
                bool found = false;
                for (int m = 0; m < (tier > 0 ? 2 : 1) && !found; m++)
                    for (int t = 0; t < 4 && !found; t++)
                        if (Same(Orient(bases[p], t, m == 1), answer)) { solTurn[p] = t; solMirror[p] = m == 1; found = true; }
                if (!found) throw new InvalidOperationException("piece pose not recoverable");

                int ax = 0, ay = 0, best = int.MaxValue;
                for (int i = 0; i < placed.Count; i += 2)
                {
                    int key = placed[i + 1] * 1000 + placed[i];
                    if (key < best) { best = key; ax = placed[i]; ay = placed[i + 1]; }
                }
                solCell[p] = ay * width + ax;
                var covered = new int[placed.Count / 2];
                for (int i = 0; i < covered.Length; i++) covered[i] = placed[2 * i + 1] * width + placed[2 * i];
                Array.Sort(covered);
                solCells[p] = covered;
            }

            turn = new int[count];
            mirrored = new bool[count];
            placedAt = new int[count];
            for (int p = 0; p < count; p++) placedAt[p] = -1;
            owner = new int[width * height];
            for (int c = 0; c < owner.Length; c++) owner[c] = -1;
        }

        // ---- queries -------------------------------------------------------------------

        public bool InShape(int cell) { return cell >= 0 && cell < shape.Length && shape[cell]; }

        /// <summary>The piece covering the cell, or -1.</summary>
        public int Owner(int cell) { return cell < 0 || cell >= owner.Length ? -1 : owner[cell]; }

        /// <summary>
        /// The piece's cells as (dx, dy) pairs from its anchor in its current turn: [dx0, dy0, dx1, dy1, ...].
        /// The anchor is the first pair, (0, 0), the piece's topmost then leftmost cell.
        /// </summary>
        public int[] PieceCells(int piece)
        {
            return Orient(bases[piece], turn[piece], mirrored[piece]);
        }

        /// <summary>Quarter turns clockwise, 0 to 3.</summary>
        public int Turn(int piece) { return turn[piece]; }
        public bool Mirrored(int piece) { return mirrored[piece]; }
        public bool Placed(int piece) { return placedAt[piece] >= 0; }
        /// <summary>The cell under the anchor, or -1 while in the tray.</summary>
        public int PlacedAt(int piece) { return placedAt[piece]; }

        // ---- moves ---------------------------------------------------------------------

        private bool Valid(int piece) { return piece >= 0 && piece < bases.Length; }

        public PieceMove Rotate(int piece)
        {
            if (!Valid(piece) || Placed(piece)) return PieceMove.Ignored;
            turn[piece] = (turn[piece] + 1) % 4;
            Moves++;
            return PieceMove.Done;
        }

        /// <summary>Mirror left to right; from tier 1 only.</summary>
        public PieceMove Mirror(int piece)
        {
            if (!Valid(piece) || Placed(piece) || tier < 1) return PieceMove.Ignored;
            mirrored[piece] = !mirrored[piece];
            Moves++;
            return PieceMove.Done;
        }

        public PieceMove Place(int piece, int cell)
        {
            if (!Valid(piece) || Placed(piece)) return PieceMove.Ignored;
            if (cell < 0 || cell >= owner.Length) return PieceMove.Outside;
            int[] cells = PieceCells(piece);
            int ax = cell % width, ay = cell / width;
            for (int i = 0; i < cells.Length; i += 2)
            {
                int x = ax + cells[i], y = ay + cells[i + 1];
                if (x < 0 || x >= width || y < 0 || y >= height || !shape[y * width + x]) return PieceMove.Outside;
            }
            for (int i = 0; i < cells.Length; i += 2)
                if (owner[(ay + cells[i + 1]) * width + ax + cells[i]] >= 0) return PieceMove.Overlaps;
            for (int i = 0; i < cells.Length; i += 2)
                owner[(ay + cells[i + 1]) * width + ax + cells[i]] = piece;
            placedAt[piece] = cell;
            Moves++;
            Refresh();
            return PieceMove.Done;
        }

        /// <summary>Tap a placed piece to take it back to the tray.</summary>
        public PieceMove Lift(int piece)
        {
            if (!Valid(piece) || !Placed(piece)) return PieceMove.Ignored;
            for (int c = 0; c < owner.Length; c++) if (owner[c] == piece) owner[c] = -1;
            placedAt[piece] = -1;
            Moves++;
            Refresh();
            return PieceMove.Done;
        }

        private void Refresh()
        {
            int covered = 0;
            for (int c = 0; c < owner.Length; c++) if (owner[c] >= 0) covered++;
            Solved = covered == cellCount;
        }

        // ---- hint ----------------------------------------------------------------------

        /// <summary>
        /// Prefers correcting a placed piece that the answer does not have there; otherwise the
        /// first tray piece, with the cheapest pose (fewest mirrors and turns from now) that fits.
        /// </summary>
        public GardenHint Hint()
        {
            var hint = new GardenHint { Piece = -1, Cell = -1 };
            if (Solved) return hint;
            for (int p = 0; p < bases.Length; p++)
            {
                if (!Placed(p)) continue;
                var mine = new List<int>();
                for (int c = 0; c < owner.Length; c++) if (owner[c] == p) mine.Add(c);
                if (!SameCells(mine, solCells[p]))
                {
                    hint.Has = true; hint.Misplaced = true; hint.Piece = p;
                    hint.Turn = turn[p]; hint.Mirrored = mirrored[p]; hint.Cell = placedAt[p];
                    return hint;
                }
            }
            for (int p = 0; p < bases.Length; p++)
            {
                if (Placed(p)) continue;
                int[] answer = Orient(bases[p], solTurn[p], solMirror[p]);
                int bestCost = int.MaxValue;
                for (int m = 0; m < (tier > 0 ? 2 : 1); m++)
                    for (int t = 0; t < 4; t++)
                    {
                        if (!Same(Orient(bases[p], t, m == 1), answer)) continue;
                        int cost = ((m == 1) != mirrored[p] ? 1 : 0) + (t - turn[p] + 4) % 4;
                        if (cost >= bestCost) continue;
                        bestCost = cost;
                        hint.Turn = t; hint.Mirrored = m == 1;
                    }
                hint.Has = true; hint.Piece = p; hint.Cell = solCell[p];
                return hint;
            }
            return hint;
        }

        private static bool SameCells(List<int> mine, int[] sorted)
        {
            if (mine.Count != sorted.Length) return false;
            for (int i = 0; i < sorted.Length; i++) if (mine[i] != sorted[i]) return false;
            return true;
        }

        // ---- orientation ---------------------------------------------------------------

        /// <summary>Mirror left to right, turn clockwise, then express relative to the anchor (topmost, then leftmost).</summary>
        private static int[] Orient(int[] cells, int turns, bool mirror)
        {
            var pts = new int[cells.Length];
            for (int i = 0; i < cells.Length; i += 2)
            {
                int x = mirror ? -cells[i] : cells[i], y = cells[i + 1];
                for (int t = 0; t < turns; t++) { int nx = -y, ny = x; x = nx; y = ny; }
                pts[i] = x; pts[i + 1] = y;
            }
            return Normalize(pts, true);
        }

        /// <summary>Sort reading order; relative to the first cell (anchor) or, if not anchored, to the box corner.</summary>
        private static int[] Normalize(int[] pts, bool anchored)
        {
            int n = pts.Length / 2;
            var idx = new int[n];
            for (int i = 0; i < n; i++) idx[i] = i;
            Array.Sort(idx, (a, b) => pts[2 * a + 1] != pts[2 * b + 1]
                ? pts[2 * a + 1].CompareTo(pts[2 * b + 1]) : pts[2 * a].CompareTo(pts[2 * b]));
            int ox, oy;
            if (anchored) { ox = pts[2 * idx[0]]; oy = pts[2 * idx[0] + 1]; }
            else
            {
                ox = int.MaxValue; oy = int.MaxValue;
                for (int i = 0; i < n; i++) { ox = Math.Min(ox, pts[2 * i]); oy = Math.Min(oy, pts[2 * i + 1]); }
            }
            var result = new int[pts.Length];
            for (int i = 0; i < n; i++)
            {
                result[2 * i] = pts[2 * idx[i]] - ox;
                result[2 * i + 1] = pts[2 * idx[i] + 1] - oy;
            }
            return result;
        }

        private static bool Same(int[] a, int[] b)
        {
            if (a.Length != b.Length) return false;
            for (int i = 0; i < a.Length; i++) if (a[i] != b[i]) return false;
            return true;
        }

        // ---- generation ----------------------------------------------------------------

        private static int Next(ref uint state, int bound)
        {
            state ^= state << 13;
            state ^= state >> 17;
            state ^= state << 5;
            return (int)(state % (uint)bound);
        }

        private static int[] DealSizes(ref uint state, int count, int min, int max)
        {
            var sizes = new int[count];
            while (true)
            {
                int sum = 0;
                for (int i = 0; i < count; i++)
                {
                    sizes[i] = MinPiece + Next(ref state, MaxPiece - MinPiece + 1);
                    sum += sizes[i];
                }
                if (sum >= min && sum <= max) return sizes;
            }
        }

        private static int Neighbours(int[] owners, int grid, int cell, Func<int, bool> counts)
        {
            int x = cell % grid, y = cell / grid, n = 0;
            if (x > 0 && counts(owners[cell - 1])) n++;
            if (x < grid - 1 && counts(owners[cell + 1])) n++;
            if (y > 0 && counts(owners[cell - grid])) n++;
            if (y < grid - 1 && counts(owners[cell + grid])) n++;
            return n;
        }

        /// <summary>
        /// Grow a shape piece by piece: each new piece starts next to what is there and grows by
        /// adjacent cells, leaning towards snug cells so the outline stays round.
        /// </summary>
        private static int[] Grow(ref uint state, int[] sizes, int grid)
        {
            var owners = new int[grid * grid];
            for (int i = 0; i < owners.Length; i++) owners[i] = -1;
            for (int p = 0; p < sizes.Length; p++)
            {
                int start;
                if (p == 0)
                {
                    int lo = grid / 4, span = grid - 2 * lo;
                    start = (lo + Next(ref state, span)) * grid + lo + Next(ref state, span);
                }
                else
                {
                    var open = new List<int>();
                    int most = 0;
                    for (int c = 0; c < owners.Length; c++)
                    {
                        if (owners[c] >= 0) continue;
                        int k = Neighbours(owners, grid, c, o => o >= 0);
                        if (k == 0) continue;
                        if (k > most) { most = k; }
                        open.Add(c);
                    }
                    if (open.Count == 0) return null;
                    if (Next(ref state, 2) == 0 && most > 1)
                        open.RemoveAll(c => Neighbours(owners, grid, c, o => o >= 0) < most);
                    start = open[Next(ref state, open.Count)];
                }
                owners[start] = p;
                for (int grown = 1; grown < sizes[p]; grown++)
                {
                    var open = new List<int>();
                    int most = 0;
                    for (int c = 0; c < owners.Length; c++)
                    {
                        if (owners[c] >= 0) continue;
                        int k = Neighbours(owners, grid, c, o => o == p);
                        if (k == 0) continue;
                        int snug = k + Neighbours(owners, grid, c, o => o >= 0 && o != p);
                        if (snug > most) most = snug;
                        open.Add(c);
                    }
                    if (open.Count == 0) return null;
                    if (Next(ref state, 3) != 0)
                        open.RemoveAll(c => Neighbours(owners, grid, c, o => o == p)
                            + Neighbours(owners, grid, c, o => o >= 0 && o != p) < most);
                    owners[open[Next(ref state, open.Count)]] = p;
                }
            }
            return owners;
        }

        /// <summary>Crop to the tight box; accept only hole-free, compact silhouettes.</summary>
        private static bool TightAndNice(int[] owners, int grid, out int w, out int h, out bool[] cells, out int count)
        {
            int minX = grid, minY = grid, maxX = -1, maxY = -1;
            count = 0;
            for (int c = 0; c < owners.Length; c++)
            {
                if (owners[c] < 0) continue;
                count++;
                minX = Math.Min(minX, c % grid); maxX = Math.Max(maxX, c % grid);
                minY = Math.Min(minY, c / grid); maxY = Math.Max(maxY, c / grid);
            }
            w = maxX - minX + 1; h = maxY - minY + 1;
            cells = new bool[w * h];
            for (int y = 0; y < h; y++)
                for (int x = 0; x < w; x++)
                    cells[y * w + x] = owners[(y + minY) * grid + x + minX] >= 0;
            return w <= 8 && h <= 8 && IsCompact(cells, w, h, count) && !HasHole(cells, w, h);
        }

        private static bool IsCompact(bool[] cells, int w, int h, int count)
        {
            return count * 100 >= w * h * MinFillPercent &&
                   PerimeterOf(cells, w, h) <= MinPerimeter(count) + MaxPerimeterExcess;
        }

        private static int PerimeterOf(bool[] cells, int w, int h)
        {
            int edges = 0;
            for (int y = 0; y < h; y++)
                for (int x = 0; x < w; x++)
                {
                    if (!cells[y * w + x]) continue;
                    if (x == 0 || !cells[y * w + x - 1]) edges++;
                    if (x == w - 1 || !cells[y * w + x + 1]) edges++;
                    if (y == 0 || !cells[y * w + x - w]) edges++;
                    if (y == h - 1 || !cells[y * w + x + w]) edges++;
                }
            return edges;
        }

        /// <summary>True when an empty cell is walled in by the silhouette.</summary>
        private static bool HasHole(bool[] cells, int w, int h)
        {
            int pw = w + 2, ph = h + 2;
            var seen = new bool[pw * ph];
            var stack = new Stack<int>();
            stack.Push(0);
            seen[0] = true;
            while (stack.Count > 0)
            {
                int c = stack.Pop(), x = c % pw, y = c / pw;
                int[] nb = { x > 0 ? c - 1 : -1, x < pw - 1 ? c + 1 : -1, y > 0 ? c - pw : -1, y < ph - 1 ? c + pw : -1 };
                foreach (int n in nb)
                {
                    if (n < 0 || seen[n]) continue;
                    int nx = n % pw - 1, ny = n / pw - 1;
                    if (nx >= 0 && nx < w && ny >= 0 && ny < h && cells[ny * w + nx]) continue;
                    seen[n] = true;
                    stack.Push(n);
                }
            }
            for (int y = 0; y < h; y++)
                for (int x = 0; x < w; x++)
                    if (!cells[y * w + x] && !seen[(y + 1) * pw + x + 1]) return true;
            return false;
        }
    }
}
