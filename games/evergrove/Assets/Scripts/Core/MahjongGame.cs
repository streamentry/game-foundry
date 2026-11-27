using System;
using System.Collections.Generic;

namespace Evergrove.Core
{
    public enum MahjongMove { Ignored, Picked, Unpicked, Matched, Swapped, NotFree }

    /// <summary>
    /// Garden tiles, a gentle mahjong-solitaire: lift matching pairs of free
    /// tiles until the table is clear. A tile is free when nothing rests on it
    /// and its left or right side is open. Positions are in half-tile units
    /// (a tile is 2 wide and 2 tall, so tiles can sit offset by half) with a
    /// layer z. Every deal is built by removing pairs from the full table in a
    /// valid order, so it is always solvable. No timer, no penalty: a tile of
    /// another face simply moves your choice, and a stuck table can be
    /// reshuffled, which re-deals the remaining faces so it is solvable again.
    /// </summary>
    public sealed class MahjongGame
    {
        private static readonly int[][][] Layouts = BuildLayouts();

        /// <summary>Tiles per tier: 16, 24, 36.</summary>
        public static int TilesFor(int tier)
        {
            return LayoutFor(tier).Length;
        }

        /// <summary>Distinct faces per tier: 6, 9, 12.</summary>
        public static int FacesFor(int tier)
        {
            if (tier < 0 || tier >= TreePuzzles.Tiers) throw new ArgumentOutOfRangeException(nameof(tier));
            return 6 + tier * 3;
        }

        private static int[][] LayoutFor(int tier)
        {
            if (tier < 0 || tier >= TreePuzzles.Tiers) throw new ArgumentOutOfRangeException(nameof(tier));
            return Layouts[tier];
        }

        private static int[][] Grid(int[] xs, int[] ys, int z)
        {
            var list = new List<int[]>();
            foreach (int y in ys)
                foreach (int x in xs) list.Add(new[] { x, y, z });
            return list.ToArray();
        }

        private static int[][] Pts(int z, params int[] xy)
        {
            var list = new List<int[]>();
            for (int i = 0; i < xy.Length; i += 2) list.Add(new[] { xy[i], xy[i + 1], z });
            return list.ToArray();
        }

        private static int[][] Join(params int[][][] parts)
        {
            var list = new List<int[]>();
            foreach (var part in parts) list.AddRange(part);
            return list.ToArray();
        }

        private static int[][][] BuildLayouts()
        {
            // Tier 0: a low terrace, 12 tiles with 4 tiles resting on their seams.
            int[][] t0 = Join(
                Grid(new[] { 0, 2, 4, 6 }, new[] { 0, 2, 4 }, 0),
                Pts(1, 1, 1, 5, 1, 1, 3, 5, 3));
            // Tier 1: a 4x4 bed, a broken ring of seven on top, one capstone.
            int[][] t1 = Join(
                Grid(new[] { 0, 2, 4, 6 }, new[] { 0, 2, 4, 6 }, 0),
                Pts(1, 1, 1, 1, 3, 3, 1, 3, 5, 5, 1, 5, 3, 5, 5),
                Pts(2, 4, 5));
            // Tier 2: a 6x4 bed, eight scattered tiles above, four more on top.
            int[][] t2 = Join(
                Grid(new[] { 0, 2, 4, 6, 8, 10 }, new[] { 0, 2, 4, 6 }, 0),
                Pts(1, 1, 1, 1, 3, 3, 3, 3, 5, 5, 1, 5, 5, 7, 3, 9, 3),
                Pts(2, 2, 3, 4, 5, 7, 1, 8, 3));
            return new[] { t0, t1, t2 };
        }

        private readonly int[] xs, ys, zs, faces;
        private readonly bool[] removed;
        private readonly int tier;
        private uint state;

        public int TileCount { get { return xs.Length; } }
        public int Selected { get; private set; }
        public int Remaining { get; private set; }
        public int Moves { get; private set; }
        public int Reshuffles { get; private set; }
        public bool Solved { get { return Remaining == 0; } }

        public MahjongGame(int seed, int tier)
        {
            int[][] layout = LayoutFor(tier);
            FacesFor(tier);
            this.tier = tier;
            int n = layout.Length;
            xs = new int[n]; ys = new int[n]; zs = new int[n];
            faces = new int[n];
            removed = new bool[n];
            for (int i = 0; i < n; i++)
            {
                xs[i] = layout[i][0]; ys[i] = layout[i][1]; zs[i] = layout[i][2];
            }
            state = unchecked((uint)seed * 0x9E3779B1u + (uint)tier * 0x85EBCA6Bu + 0x27D4EB2Fu);
            if (state == 0) state = 0x6d2b79f5u;
            Selected = -1;
            Remaining = n;

            int pairCount = n / 2, f = FacesFor(tier);
            var pairFaces = new List<int>();
            for (int i = 0; i < pairCount; i++) pairFaces.Add(i % f);
            Deal(pairFaces, new bool[n]);
        }

        public int Face(int tile) { return faces[tile]; }

        /// <summary>
        /// I291 (idea 2, after mahjong's season tiles): face 0 is dealt twice at every tier,
        /// so it has exactly four tiles. They are drawn as the four seasons, all different,
        /// and any two still match, because the rule is unchanged: same face, same set.
        /// </summary>
        public const int SeasonFace = 0;

        /// <summary>The season tiles in tile order: the first is spring, the last winter.</summary>
        public static int[] SeasonTiles(MahjongGame game)
        {
            var found = new List<int>();
            for (int t = 0; t < game.TileCount; t++)
                if (game.Face(t) == SeasonFace) found.Add(t);
            return found.ToArray();
        }
        public int X(int tile) { return xs[tile]; }
        public int Y(int tile) { return ys[tile]; }
        public int Z(int tile) { return zs[tile]; }
        public bool Removed(int tile) { return removed[tile]; }

        public bool IsFree(int tile)
        {
            if (tile < 0 || tile >= xs.Length || removed[tile]) return false;
            return FreeIn(tile, removed);
        }

        private bool FreeIn(int tile, bool[] gone)
        {
            bool left = false, right = false;
            for (int j = 0; j < xs.Length; j++)
            {
                if (j == tile || gone[j]) continue;
                int dx = xs[j] - xs[tile], dy = ys[j] - ys[tile];
                if (dy <= -2 || dy >= 2) continue;
                if (zs[j] > zs[tile] && dx > -2 && dx < 2) return false;
                if (zs[j] == zs[tile])
                {
                    if (dx == -2) left = true;
                    else if (dx == 2) right = true;
                }
            }
            return !(left && right);
        }

        public MahjongMove Select(int tile)
        {
            if (Solved || tile < 0 || tile >= xs.Length || removed[tile]) return MahjongMove.Ignored;
            if (!IsFree(tile)) return MahjongMove.NotFree;
            if (Selected < 0)
            {
                Selected = tile;
                return MahjongMove.Picked;
            }
            if (Selected == tile)
            {
                Selected = -1;
                return MahjongMove.Unpicked;
            }
            if (faces[Selected] != faces[tile])
            {
                Selected = tile;
                return MahjongMove.Swapped;
            }
            removed[Selected] = removed[tile] = true;
            Selected = -1;
            Remaining -= 2;
            Moves++;
            return MahjongMove.Matched;
        }

        /// <summary>True while some free matching pair remains.</summary>
        public bool HasMove { get { return HintPair().Length == 2; } }

        /// <summary>The first free matching pair (lower index first), or an empty array.</summary>
        public int[] HintPair()
        {
            int n = xs.Length;
            var free = new List<int>();
            for (int i = 0; i < n; i++)
                if (IsFree(i)) free.Add(i);
            for (int a = 0; a < free.Count; a++)
                for (int b = a + 1; b < free.Count; b++)
                    if (faces[free[a]] == faces[free[b]]) return new[] { free[a], free[b] };
            return new int[0];
        }

        /// <summary>
        /// When no free pair matches and tiles remain, re-deal the remaining faces
        /// among the remaining tiles so the table can be cleared again.
        /// </summary>
        public bool Reshuffle()
        {
            if (Solved || HasMove) return false;
            var counts = new int[FacesFor(tier)];
            for (int i = 0; i < xs.Length; i++)
                if (!removed[i]) counts[faces[i]]++;
            var pairFaces = new List<int>();
            for (int f = 0; f < counts.Length; f++)
                for (int k = 0; k < counts[f] / 2; k++) pairFaces.Add(f);
            if (!Deal(pairFaces, (bool[])removed.Clone())) return false;
            Selected = -1;
            Reshuffles++;
            return true;
        }

        private int Next(int bound)
        {
            state ^= state << 13;
            state ^= state >> 17;
            state ^= state << 5;
            return (int)(state % (uint)bound);
        }

        // Fills `faces` for every tile not in `gone` by lifting random free pairs
        // from the table in turn, giving each pair the next face of a shuffled
        // list. The lifting order is a valid way to clear the table.
        private bool Deal(List<int> pairFaces, bool[] gone)
        {
            for (int attempt = 0; attempt < 400; attempt++)
            {
                for (int i = pairFaces.Count - 1; i > 0; i--)
                {
                    int j = Next(i + 1);
                    int swap = pairFaces[i]; pairFaces[i] = pairFaces[j]; pairFaces[j] = swap;
                }
                bool[] table = (bool[])gone.Clone();
                var assigned = new int[xs.Length];
                bool ok = true;
                foreach (int face in pairFaces)
                {
                    var free = new List<int>();
                    for (int i = 0; i < xs.Length; i++)
                        if (!table[i] && FreeIn(i, table)) free.Add(i);
                    if (free.Count < 2) { ok = false; break; }
                    int a = free[Next(free.Count)];
                    free.Remove(a);
                    int b = free[Next(free.Count)];
                    assigned[a] = assigned[b] = face;
                    table[a] = table[b] = true;
                }
                if (!ok) continue;
                for (int i = 0; i < xs.Length; i++)
                    if (!gone[i]) faces[i] = assigned[i];
                return true;
            }
            return false;
        }
    }
}
