using System;
using System.Collections.Generic;

namespace Evergrove.Core
{
    /// <summary>What a cell shows: not yet decided, a drawn petal, or marked bare.</summary>
    public enum CellMark { Unknown, Drawn, Bare }

    /// <summary>What a tap on the doorstep did.</summary>
    public enum DoorMove { Changed, Given, Ignored }

    /// <summary>
    /// Doorstep pattern (idea 44): the design drawn at the threshold each morning is mirror
    /// symmetric, so a cell and its mirror images (its orbit) always agree. A few cells are
    /// shown as given; because every orbit holds exactly one given, the whole pattern follows
    /// by reflection and has exactly one answer. A tap cycles a cell: unknown, drawn, bare.
    /// Solved when every cell is marked and the grid equals the hidden pattern. No timer,
    /// no penalty. Tier 0: 5x5 and tier 1: 7x7 with both mirrors; tier 2: 8x8 with square
    /// symmetry (both mirrors and both diagonals). Deterministic from (seed, tier).
    /// </summary>
    public sealed class DoorstepGame
    {
        private const int MaxQuarterTries = 5000;
        private const double MinDensity = 0.35, MaxDensity = 0.65;

        /// <summary>Grid size per tier: 5, 7, 8.</summary>
        public static int SizeFor(int tier)
        {
            if (tier < 0 || tier >= TreePuzzles.Tiers) throw new ArgumentOutOfRangeException(nameof(tier));
            return tier == 0 ? 5 : tier == 1 ? 7 : 8;
        }

        private readonly int size;
        private readonly int tier;
        private readonly int[][] orbits;     // sorted cells per orbit
        private readonly int[] orbitOf;      // orbit index per cell
        private readonly bool[] drawn;       // the hidden pattern per cell
        private readonly bool[] given;
        private readonly CellMark[] marks;
        private uint rng;

        public int Size { get { return size; } }
        public int Moves { get; private set; }

        /// <summary>
        /// Raw difficulty: the percentage of cells to decide that a nearer given of the opposite
        /// kind would mislead. Reading the reflection is the whole puzzle, but every orbit sits
        /// the same distance from its own given whatever the seed (a given is as far from the
        /// other cells of its orbit as any member is), so that reach cannot tell boards apart.
        /// What does differ is the decoys: a cell whose closest given, strictly nearer than its
        /// own orbit's given, shows the opposite kind (drawn or bare) tempts a "copy the
        /// neighbour" guess, so the player must trust the mirror and not the eye.
        /// </summary>
        public int DecoyPercent { get; private set; }

        /// <summary>Depth leaves 1..3, from DecoyPercent (see LeavesFor).</summary>
        public int Leaves { get; private set; }

        // Cut points chosen from seeds 1..300 over all tiers.
        private const int LeafCut1 = 27, LeafCut2 = 39;

        /// <summary>The leaves a decoy percentage earns.</summary>
        public static int LeavesFor(int decoyPercent) { return decoyPercent <= LeafCut1 ? 1 : decoyPercent <= LeafCut2 ? 2 : 3; }

        public DoorstepGame(int seed, int tier)
        {
            size = SizeFor(tier);
            this.tier = tier;
            rng = unchecked((uint)seed * 0x9E3779B1u + (uint)tier * 0x85EBCA6Bu + 0x165667B1u);
            if (rng == 0) rng = 0x6d2b79f5u;
            int n = size * size;
            orbitOf = new int[n];
            for (int i = 0; i < n; i++) orbitOf[i] = -1;
            var list = new List<int[]>();
            for (int c = 0; c < n; c++)
            {
                if (orbitOf[c] >= 0) continue;
                int[] members = ComputeOrbit(c);
                foreach (int m in members) orbitOf[m] = list.Count;
                list.Add(members);
            }
            orbits = list.ToArray();

            drawn = new bool[n];
            given = new bool[n];
            marks = new CellMark[n];
            DrawPattern();
            PickGivens();
            DecoyPercent = MeasureDecoys();
            Leaves = LeavesFor(DecoyPercent);
            for (int c = 0; c < n; c++)
                if (given[c]) marks[c] = drawn[c] ? CellMark.Drawn : CellMark.Bare;
        }

        private int[] ComputeOrbit(int cell)
        {
            int x = cell % size, y = cell / size, m = size - 1;
            var set = new SortedSet<int>();
            set.Add(y * size + x);
            set.Add(y * size + (m - x));
            set.Add((m - y) * size + x);
            set.Add((m - y) * size + (m - x));
            if (tier == 2)
            {
                set.Add(x * size + y);
                set.Add(x * size + (m - y));
                set.Add((m - x) * size + y);
                set.Add((m - x) * size + (m - y));
            }
            var arr = new int[set.Count];
            set.CopyTo(arr);
            return arr;
        }

        private int Next(int bound)
        {
            rng ^= rng << 13; rng ^= rng >> 17; rng ^= rng << 5;
            return (int)(rng % (uint)bound);
        }

        // ---- the hidden pattern: shapes drawn in doubled centre-offsets, mirrored by orbit ----

        private void DrawPattern()
        {
            int orbitCount = orbits.Length;
            var onOrbit = new bool[orbitCount];
            for (int attempt = 0; attempt < 40; attempt++)
            {
                Array.Clear(onOrbit, 0, orbitCount);
                int shapes = 2 + Next(3);
                for (int s = 0; s < shapes; s++) AddShape(onOrbit);
                if (InRange(Density(onOrbit))) break;
            }
            // Gentle repair so the density bound always holds: flip an orbit towards the range.
            int guard = 0;
            while (!InRange(Density(onOrbit)) && guard++ < 10000)
            {
                bool tooFull = Density(onOrbit) > MaxDensity;
                int start = Next(orbitCount);
                for (int k = 0; k < orbitCount; k++)
                {
                    int o = (start + k) % orbitCount;
                    if (onOrbit[o] == tooFull) { onOrbit[o] = !tooFull; break; }
                }
            }
            for (int c = 0; c < drawn.Length; c++) drawn[c] = onOrbit[orbitOf[c]];
        }

        private static bool InRange(double d) { return d >= MinDensity && d <= MaxDensity; }

        private double Density(bool[] onOrbit)
        {
            int count = 0;
            for (int o = 0; o < orbits.Length; o++) if (onOrbit[o]) count += orbits[o].Length;
            return count / (double)(size * size);
        }

        private void AddShape(bool[] onOrbit)
        {
            int kind = Next(6);
            // A random anchor cell, in doubled offsets from the centre (a, b >= 0).
            int ax = Next(size), ay = Next(size);
            int a0 = Math.Abs(2 * ax - (size - 1)), b0 = Math.Abs(2 * ay - (size - 1));
            bool xor = Next(4) == 0;
            for (int o = 0; o < orbits.Length; o++)
            {
                int cell = orbits[o][0];
                int a = Math.Abs(2 * (cell % size) - (size - 1)), b = Math.Abs(2 * (cell / size) - (size - 1));
                bool hit = Shape(kind, a, b, a0, b0);
                if (!hit && tier == 2) hit = Shape(kind, b, a, a0, b0);
                if (!hit) continue;
                onOrbit[o] = xor ? !onOrbit[o] : true;
            }
        }

        private bool Shape(int kind, int a, int b, int a0, int b0)
        {
            switch (kind)
            {
                case 0: return Math.Max(a, b) == Math.Max(a0, b0);                  // square ring
                case 1: return a + b == a0 + b0;                                    // diamond outline
                case 2: return a * a + b * b <= a0 * a0 + b0 * b0 && Math.Max(a0, b0) <= size / 2 + 1; // filled disc
                case 3: return a == b;                                              // diagonal line
                case 4: return Math.Abs(a - a0) + Math.Abs(b - b0) <= 2;            // petal cluster
                default: return tier < 2 ? a == a0 : a == a0 && b == b0;            // cross arm, or a dot
            }
        }

        // ---- givens: one per orbit, at a random member, spread over all four quarters ----

        private int Quarter(int cell)
        {
            int x = cell % size, y = cell / size;
            return (x * 2 >= size ? 1 : 0) + (y * 2 >= size ? 2 : 0);
        }

        private void PickGivens()
        {
            var picks = new int[orbits.Length];
            for (int attempt = 0; attempt < MaxQuarterTries; attempt++)
            {
                int seen = 0;
                for (int o = 0; o < orbits.Length; o++)
                {
                    picks[o] = orbits[o][Next(orbits[o].Length)];
                    seen |= 1 << Quarter(picks[o]);
                }
                if (seen != 15) continue;
                foreach (int p in picks) given[p] = true;
                return;
            }
            throw new InvalidOperationException("no spread of givens found");
        }

        // Percent (rounded) of the non-given cells whose strictly nearer foreign givens, by grid
        // distance (columns plus rows), all show the opposite kind to the cell's own answer.
        private int MeasureDecoys()
        {
            int decoys = 0, unknown = 0;
            for (int c = 0; c < drawn.Length; c++)
            {
                if (given[c]) continue;
                unknown++;
                int own = int.MaxValue, near = int.MaxValue;
                bool allOpposite = true;
                for (int g = 0; g < drawn.Length; g++)
                {
                    if (!given[g]) continue;
                    int d = Math.Abs(c % size - g % size) + Math.Abs(c / size - g / size);
                    if (orbitOf[g] == orbitOf[c]) { own = d; continue; }
                    if (d < near) { near = d; allOpposite = true; }
                    if (d == near && drawn[g] == drawn[c]) allOpposite = false;
                }
                if (near < own && allOpposite) decoys++;
            }
            return unknown == 0 ? 0 : (decoys * 100 + unknown / 2) / unknown;
        }

        // ---- play ----

        private bool InRangeCell(int cell) { return cell >= 0 && cell < size * size; }

        public bool IsGiven(int cell) { return InRangeCell(cell) && given[cell]; }

        public CellMark State(int cell) { return InRangeCell(cell) ? marks[cell] : CellMark.Unknown; }

        /// <summary>The hidden pattern: true when the cell is drawn.</summary>
        public bool Solution(int cell) { return InRangeCell(cell) && drawn[cell]; }

        /// <summary>The cell and all its mirror images, ascending.</summary>
        public int[] Orbit(int cell)
        {
            if (!InRangeCell(cell)) return new int[0];
            return (int[])orbits[orbitOf[cell]].Clone();
        }

        public DoorMove Tap(int cell)
        {
            if (!InRangeCell(cell)) return DoorMove.Ignored;
            if (given[cell]) return DoorMove.Given;
            marks[cell] = marks[cell] == CellMark.Unknown ? CellMark.Drawn
                : marks[cell] == CellMark.Drawn ? CellMark.Bare : CellMark.Unknown;
            Moves++;
            return DoorMove.Changed;
        }

        /// <summary>True when this cell's mark disagrees with the given shown in its orbit.</summary>
        public bool Conflicts(int cell)
        {
            if (!InRangeCell(cell) || given[cell] || marks[cell] == CellMark.Unknown) return false;
            foreach (int o in orbits[orbitOf[cell]])
                if (given[o]) return (marks[cell] == CellMark.Drawn) != drawn[o];
            return false;
        }

        public bool Solved
        {
            get
            {
                for (int c = 0; c < marks.Length; c++)
                    if (marks[c] == CellMark.Unknown || (marks[c] == CellMark.Drawn) != drawn[c]) return false;
                return true;
            }
        }

        /// <summary>A cell to fix and the mark it should have: a conflict first, else an undecided cell.</summary>
        public bool TryHint(out int cell, out CellMark mark)
        {
            for (int c = 0; c < marks.Length; c++)
                if (Conflicts(c)) return Hint(c, out cell, out mark);
            for (int c = 0; c < marks.Length; c++)
                if (marks[c] == CellMark.Unknown) return Hint(c, out cell, out mark);
            cell = -1;
            mark = CellMark.Unknown;
            return false;
        }

        private bool Hint(int c, out int cell, out CellMark mark)
        {
            cell = c;
            mark = drawn[c] ? CellMark.Drawn : CellMark.Bare;
            return true;
        }
    }
}
