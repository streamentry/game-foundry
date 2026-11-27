using System;

namespace Evergrove.Core
{
    /// <summary>
    /// The puzzle flowers hold (depth spine, Layer 2): painted cards face down;
    /// turn two, keep them if they match, otherwise they turn back on your next
    /// turn. Memory, never speed: no timer, no limit on turns, no penalty.
    /// Dealt from a seed, so the same flower always deals the same table.
    /// </summary>
    public sealed class PairsGame
    {
        /// <summary>Pairs per tier: 4 (2×4), 6 (3×4), 8 (4×4).</summary>
        public static int PairsFor(int tier)
        {
            if (tier < 0 || tier >= TreePuzzles.Tiers) throw new ArgumentOutOfRangeException(nameof(tier));
            return 4 + tier * 2;
        }

        public enum Turn { Ignored, First, Match, Miss }

        private readonly int[] faces;    // picture index per card, each used exactly twice
        private readonly bool[] kept;    // matched and staying face up
        private int open = -1;           // the first card of the current turn
        private int missA = -1, missB = -1; // a missed pair, still face up until the next tap

        public int Count { get { return faces.Length; } }
        public int Pairs { get { return faces.Length / 2; } }
        public int Turns { get; private set; }
        public bool Solved { get; private set; }

        /// <summary>The seed and tier this board was dealt from; undo rebuilds it from them.</summary>
        public int Seed { get; private set; }
        public int Tier { get; private set; }

        public PairsGame(int seed, int tier)
        {
            Seed = seed;
            Tier = tier;
            int pairs = PairsFor(tier);
            faces = new int[pairs * 2];
            for (int i = 0; i < faces.Length; i++) faces[i] = i / 2;
            uint state = unchecked((uint)seed * 0x9E3779B1u + (uint)tier * 0x85EBCA6Bu + 0x165667B1u);
            if (state == 0) state = 0x6d2b79f5u;
            for (int i = faces.Length - 1; i > 0; i--)
            {
                state ^= state << 13;
                state ^= state >> 17;
                state ^= state << 5;
                int j = (int)(state % (uint)(i + 1));
                int swap = faces[i]; faces[i] = faces[j]; faces[j] = swap;
            }
            kept = new bool[faces.Length];
        }

        public int Face(int card) { return faces[card]; }
        public bool Kept(int card) { return kept[card]; }

        /// <summary>Face up now: kept cards, the open card, and a missed pair until the next tap.</summary>
        public bool Showing(int card)
        {
            return kept[card] || card == open || card == missA || card == missB;
        }

        public Turn Flip(int card)
        {
            if (Solved || card < 0 || card >= faces.Length || kept[card] || card == open) return Turn.Ignored;
            // A missed pair turns back as you reach for the next card.
            missA = missB = -1;
            if (open < 0)
            {
                open = card;
                return Turn.First;
            }
            int first = open;
            open = -1;
            Turns++;
            if (faces[first] == faces[card])
            {
                kept[first] = kept[card] = true;
                Solved = Array.TrueForAll(kept, k => k);
                return Turn.Match;
            }
            missA = first;
            missB = card;
            return Turn.Miss;
        }
    }
}
