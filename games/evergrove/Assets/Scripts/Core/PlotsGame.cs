using System;
using System.Collections.Generic;

namespace Evergrove.Core
{
    /// <summary>What a drag or tap on the plots field did.</summary>
    public enum PlotMove { Invalid, Placed, Removed }

    /// <summary>
    /// Garden plots (depth spine, Layer 2): divide the field into rectangular beds.
    /// Each bed holds exactly one marker, and the marker's number is the bed's area
    /// in cells. Every cell ends up in exactly one bed. Each field has exactly one
    /// answer, dealt from a seed. No timer, no penalty for a bed that does not fit.
    /// </summary>
    public sealed class PlotsGame
    {
        private struct Rect
        {
            public readonly int Left, Top, W, H;
            public Rect(int left, int top, int w, int h) { Left = left; Top = top; W = w; H = h; }
            public int Right { get { return Left + W - 1; } }
            public int Bottom { get { return Top + H - 1; } }
            public int Area { get { return W * H; } }
            public bool Contains(int x, int y) { return x >= Left && x <= Right && y >= Top && y <= Bottom; }
            public bool Overlaps(Rect o)
            {
                return Left <= o.Right && o.Left <= Right && Top <= o.Bottom && o.Top <= Bottom;
            }
        }

        private const int MaxAttempts = 100000;
        private const int MinArea = 2, MaxArea = 8;

        /// <summary>Field size per tier: 5, 6, 7.</summary>
        public static int SizeFor(int tier)
        {
            if (tier < 0 || tier >= TreePuzzles.Tiers) throw new ArgumentOutOfRangeException(nameof(tier));
            return 5 + tier;
        }

        private readonly int size;
        private readonly int[] clues;       // area per cell, 0 = no marker
        private readonly int[] markers;     // marker cells, ascending
        private readonly Rect[] solution;   // the answer's bed per marker (parallel to markers)
        private readonly List<Rect> beds = new List<Rect>();
        private readonly List<int> bedMarkers = new List<int>();
        private readonly int[] bedOf;       // placed bed index per cell, -1 = open

        public int Size { get { return size; } }
        public int[] Markers { get { return (int[])markers.Clone(); } }
        public int BedCount { get { return beds.Count; } }
        public int Moves { get; private set; }
        /// <summary>How many placed beds the last Draw replaced.</summary>
        public int LastReplaced { get; private set; }
        public bool Solved { get; private set; }
        /// <summary>True when plain deduction alone solves the field, with no guessing.</summary>
        public bool SolvableByDeduction { get; private set; }

        /// <summary>The seed and tier this board was dealt from; undo rebuilds it from them.</summary>
        public int Seed { get; private set; }
        public int Tier { get; private set; }

        public PlotsGame(int seed, int tier)
        {
            Seed = seed;
            Tier = tier;
            size = SizeFor(tier);
            uint state = unchecked((uint)seed * 0x9E3779B1u + (uint)tier * 0x85EBCA6Bu + 0x27D4EB2Fu);
            if (state == 0) state = 0x6d2b79f5u;
            clues = new int[size * size];
            bedOf = new int[size * size];
            for (int i = 0; i < bedOf.Length; i++) bedOf[i] = -1;

            int[] cells = null;
            Rect[] parts = null;
            for (int attempt = 0; attempt < MaxAttempts && parts == null; attempt++)
            {
                List<Rect> partition = Partition(ref state, size);
                if (partition == null) continue;
                int[] marks = PickMarkers(ref state, partition, size);
                List<Rect>[] cands = Candidates(marks, partition, size);
                if (CountSolutions(cands, size, 2) != 1) continue;
                // Tier 0 must fall to plain deduction; the others only need to be unique.
                if (tier == 0 && attempt < MaxAttempts / 2 &&
                    Deduce(cands, new bool[size * size], new bool[marks.Length], size, int.MaxValue).Count != marks.Length)
                    continue;
                cells = marks;
                parts = partition.ToArray();
                SolvableByDeduction = Deduce(cands, new bool[size * size], new bool[marks.Length], size, int.MaxValue).Count == marks.Length;
            }
            if (parts == null) throw new InvalidOperationException("no unique plots field found");

            var order = new int[cells.Length];
            for (int i = 0; i < order.Length; i++) order[i] = i;
            Array.Sort(order, (a, b) => cells[a].CompareTo(cells[b]));
            markers = new int[cells.Length];
            solution = new Rect[cells.Length];
            for (int i = 0; i < order.Length; i++)
            {
                markers[i] = cells[order[i]];
                solution[i] = parts[order[i]];
                clues[markers[i]] = solution[i].Area;
            }
        }

        public int Clue(int cell)
        {
            return cell < 0 || cell >= clues.Length ? 0 : clues[cell];
        }

        public int BedAt(int cell)
        {
            return cell < 0 || cell >= bedOf.Length ? -1 : bedOf[cell];
        }

        public int BedLeft(int i) { return beds[i].Left; }
        public int BedTop(int i) { return beds[i].Top; }
        public int BedWidth(int i) { return beds[i].W; }
        public int BedHeight(int i) { return beds[i].H; }
        /// <summary>The marker cell inside placed bed i.</summary>
        public int BedMarker(int i) { return bedMarkers[i]; }

        private bool TryRect(int fromCell, int toCell, out Rect rect, out int marker)
        {
            rect = new Rect(0, 0, 0, 0);
            marker = -1;
            int n = size * size;
            if (fromCell < 0 || toCell < 0 || fromCell >= n || toCell >= n) return false;
            int x0 = Math.Min(fromCell % size, toCell % size), x1 = Math.Max(fromCell % size, toCell % size);
            int y0 = Math.Min(fromCell / size, toCell / size), y1 = Math.Max(fromCell / size, toCell / size);
            rect = new Rect(x0, y0, x1 - x0 + 1, y1 - y0 + 1);
            int count = 0;
            for (int y = y0; y <= y1; y++)
                for (int x = x0; x <= x1; x++)
                    if (clues[y * size + x] > 0) { count++; marker = y * size + x; }
            return count == 1 && clues[marker] == rect.Area;
        }

        /// <summary>True when dragging between these corners would make a valid bed.</summary>
        public bool Fits(int fromCell, int toCell)
        {
            Rect r; int m;
            return TryRect(fromCell, toCell, out r, out m);
        }

        /// <summary>Drag a rectangle between two corner cells; a tap on a placed bed lifts it.</summary>
        public PlotMove Draw(int fromCell, int toCell)
        {
            // A refused draw changes nothing, LastReplaced included (undo replays only changing moves).
            if (fromCell == toCell && BedAt(fromCell) >= 0)
            {
                LastReplaced = 0;
                beds.RemoveAt(BedAt(fromCell));
                bedMarkers.RemoveAt(BedAt(fromCell));
                Reindex();
                Moves++;
                return PlotMove.Removed;
            }
            Rect rect; int marker;
            if (!TryRect(fromCell, toCell, out rect, out marker)) return PlotMove.Invalid;
            LastReplaced = 0;
            for (int i = beds.Count - 1; i >= 0; i--)
            {
                if (!beds[i].Overlaps(rect)) continue;
                beds.RemoveAt(i);
                bedMarkers.RemoveAt(i);
                LastReplaced++;
            }
            beds.Add(rect);
            bedMarkers.Add(marker);
            Reindex();
            Moves++;
            return PlotMove.Placed;
        }

        private void Reindex()
        {
            for (int i = 0; i < bedOf.Length; i++) bedOf[i] = -1;
            for (int b = 0; b < beds.Count; b++)
                for (int y = beds[b].Top; y <= beds[b].Bottom; y++)
                    for (int x = beds[b].Left; x <= beds[b].Right; x++)
                        bedOf[y * size + x] = b;
            Solved = Array.TrueForAll(bedOf, c => c >= 0);
        }

        /// <summary>
        /// A bed of the answer not yet placed, as [left, top, width, height]; empty when solved.
        /// Prefers a bed that plain deduction forces from the correct beds already placed.
        /// </summary>
        public int[] HintBed()
        {
            if (Solved) return new int[0];
            var occupied = new bool[size * size];
            var done = new bool[markers.Length];
            for (int m = 0; m < markers.Length; m++)
                for (int b = 0; b < beds.Count; b++)
                    if (bedMarkers[b] == markers[m] && Same(beds[b], solution[m]))
                    {
                        done[m] = true;
                        Mark(occupied, solution[m], true, size);
                    }
            int pick = -1;
            List<Rect>[] cands = Candidates(markers, solution, size);
            List<int> forced = Deduce(cands, occupied, done, size, 1);
            if (forced.Count > 0) pick = forced[0];
            for (int m = 0; m < markers.Length && pick < 0; m++) if (!done[m]) pick = m;
            if (pick < 0) return new int[0];
            Rect r = solution[pick];
            return new[] { r.Left, r.Top, r.W, r.H };
        }

        private static bool Same(Rect a, Rect b)
        {
            return a.Left == b.Left && a.Top == b.Top && a.W == b.W && a.H == b.H;
        }

        private static void Mark(bool[] grid, Rect r, bool value, int size)
        {
            for (int y = r.Top; y <= r.Bottom; y++)
                for (int x = r.Left; x <= r.Right; x++) grid[y * size + x] = value;
        }

        private static bool Free(bool[] taken, Rect r, int size)
        {
            for (int y = r.Top; y <= r.Bottom; y++)
                for (int x = r.Left; x <= r.Right; x++)
                    if (taken[y * size + x]) return false;
            return true;
        }

        // ---- generation ----------------------------------------------------------

        private static int Next(ref uint state, int bound)
        {
            state ^= state << 13;
            state ^= state >> 17;
            state ^= state << 5;
            return (int)(state % (uint)bound);
        }

        /// <summary>Greedy random tiling: each open cell, in reading order, starts a bed of 2..8 cells.</summary>
        private static List<Rect> Partition(ref uint state, int size)
        {
            var taken = new bool[size * size];
            var result = new List<Rect>();
            int singles = 0;
            for (int cell = 0; cell < taken.Length; cell++)
            {
                if (taken[cell]) continue;
                int x = cell % size, y = cell / size;
                var options = new List<Rect>();
                for (int w = 1; w <= MaxArea && x + w <= size; w++)
                    for (int h = 1; h <= MaxArea && y + h <= size; h++)
                    {
                        int area = w * h;
                        if (area < MinArea || area > MaxArea) continue;
                        var r = new Rect(x, y, w, h);
                        if (Free(taken, r, size)) options.Add(r);
                    }
                Rect pick;
                if (options.Count == 0)
                {
                    if (++singles > 1) return null; // area-1 beds only rarely
                    pick = new Rect(x, y, 1, 1);
                }
                else pick = options[Next(ref state, options.Count)];
                Mark(taken, pick, true, size);
                result.Add(pick);
            }
            return result;
        }

        private static int[] PickMarkers(ref uint state, List<Rect> parts, int size)
        {
            var marks = new int[parts.Count];
            for (int i = 0; i < marks.Length; i++)
            {
                Rect r = parts[i];
                int pick = Next(ref state, r.Area);
                marks[i] = (r.Top + pick / r.W) * size + r.Left + pick % r.W;
            }
            return marks;
        }

        /// <summary>Per marker, every rectangle of its area that holds it and no other marker.</summary>
        private static List<Rect>[] Candidates(int[] marks, IList<Rect> areaSource, int size)
        {
            var isMarker = new bool[size * size];
            foreach (int m in marks) isMarker[m] = true;
            var result = new List<Rect>[marks.Length];
            for (int k = 0; k < marks.Length; k++)
            {
                result[k] = new List<Rect>();
                int area = areaSource[k].Area;
                int mx = marks[k] % size, my = marks[k] / size;
                for (int w = 1; w <= area; w++)
                {
                    if (area % w != 0) continue;
                    int h = area / w;
                    for (int left = mx - w + 1; left <= mx; left++)
                        for (int top = my - h + 1; top <= my; top++)
                        {
                            if (left < 0 || top < 0 || left + w > size || top + h > size) continue;
                            bool clean = true;
                            for (int y = top; y < top + h && clean; y++)
                                for (int x = left; x < left + w; x++)
                                    if (isMarker[y * size + x] && y * size + x != marks[k]) { clean = false; break; }
                            if (clean) result[k].Add(new Rect(left, top, w, h));
                        }
                }
            }
            return result;
        }

        /// <summary>Exact cover by backtracking on the first open cell; stops at <paramref name="limit"/>.</summary>
        private static int CountSolutions(List<Rect>[] cands, int size, int limit)
        {
            return Count(cands, new bool[size * size], new bool[cands.Length], size, limit);
        }

        private static int Count(List<Rect>[] cands, bool[] occupied, bool[] used, int size, int limit)
        {
            int cell = Array.IndexOf(occupied, false);
            if (cell < 0) return 1;
            int cx = cell % size, cy = cell / size;
            int total = 0;
            for (int k = 0; k < cands.Length && total < limit; k++)
            {
                if (used[k]) continue;
                foreach (Rect r in cands[k])
                {
                    if (!r.Contains(cx, cy) || !Free(occupied, r, size)) continue;
                    Mark(occupied, r, true, size);
                    used[k] = true;
                    total += Count(cands, occupied, used, size, limit - total);
                    used[k] = false;
                    Mark(occupied, r, false, size);
                    if (total >= limit) break;
                }
            }
            return total;
        }

        /// <summary>
        /// Plain deduction: a marker with one bed left takes it; a cell only one marker can still
        /// reach pins that marker to beds covering the cell. Returns markers in the order they
        /// became certain (at most <paramref name="max"/>); a full-length list means no guessing.
        /// </summary>
        private static List<int> Deduce(List<Rect>[] cands, bool[] occupied0, bool[] done0, int size, int max)
        {
            var occupied = (bool[])occupied0.Clone();
            var done = (bool[])done0.Clone();
            var live = new List<Rect>[cands.Length];
            for (int k = 0; k < live.Length; k++) live[k] = new List<Rect>(cands[k]);
            var order = new List<int>();
            int remaining = 0;
            foreach (bool d in done) if (!d) remaining++;
            bool progress = true;
            while (progress && order.Count < max && remaining > 0)
            {
                progress = false;
                for (int k = 0; k < live.Length; k++)
                {
                    if (done[k]) continue;
                    live[k].RemoveAll(r => !Free(occupied, r, size));
                    if (live[k].Count != 1) continue;
                    Mark(occupied, live[k][0], true, size);
                    done[k] = true;
                    remaining--;
                    order.Add(k);
                    progress = true;
                    if (order.Count >= max) return order;
                }
                for (int cell = 0; cell < occupied.Length; cell++)
                {
                    if (occupied[cell]) continue;
                    int cx = cell % size, cy = cell / size, owner = -1, reach = 0;
                    for (int k = 0; k < live.Length; k++)
                    {
                        if (done[k]) continue;
                        if (live[k].Exists(r => r.Contains(cx, cy) && Free(occupied, r, size))) { reach++; owner = k; }
                    }
                    if (reach != 1) continue;
                    int before = live[owner].Count;
                    live[owner].RemoveAll(r => !r.Contains(cx, cy));
                    if (live[owner].Count < before) progress = true;
                }
            }
            return order;
        }
    }
}
