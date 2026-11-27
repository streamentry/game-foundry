using System;

namespace Evergrove.Core
{
    /// <summary>
    /// The puzzle buildings hold (depth spine, Layer 2): a neighbour's memory
    /// painting cut into a square of tiles and scattered. Tap one tile, then
    /// another, and they swap; a tile in its place stays put. Mended when every
    /// tile is home. No timer, no move limit. Dealt from a seed, so the same
    /// building always scatters its painting the same way.
    /// </summary>
    public sealed class MosaicGame
    {
        public enum Step { Ignored, Picked, PutDown, Swapped }

        /// <summary>Tiles per side by tier: 3, 4, 5.</summary>
        public static int SizeFor(int tier)
        {
            if (tier < 0 || tier >= TreePuzzles.Tiers) throw new ArgumentOutOfRangeException(nameof(tier));
            return 3 + tier;
        }

        private readonly int[] slots; // slots[s] = the piece lying in slot s; piece p belongs in slot p

        public int Size { get; private set; }
        public int Count { get { return slots.Length; } }
        public int Picked { get; private set; }
        public int Moves { get; private set; }
        public bool Solved { get; private set; }

        /// <summary>The seed and tier this board was dealt from; undo rebuilds it from them.</summary>
        public int Seed { get; private set; }
        public int Tier { get; private set; }

        public MosaicGame(int seed, int tier)
        {
            Seed = seed;
            Tier = tier;
            Size = SizeFor(tier);
            slots = new int[Size * Size];
            Picked = -1;
            uint state = unchecked((uint)seed * 0x9E3779B1u + (uint)tier * 0x85EBCA6Bu + 0x3C6EF372u);
            if (state == 0) state = 0x6d2b79f5u;
            // Shuffle until at most a third of the tiles happen to land home.
            for (int attempt = 0; ; attempt++)
            {
                for (int i = 0; i < slots.Length; i++) slots[i] = i;
                for (int i = slots.Length - 1; i > 0; i--)
                {
                    state ^= state << 13;
                    state ^= state >> 17;
                    state ^= state << 5;
                    int j = (int)(state % (uint)(i + 1));
                    int swap = slots[i]; slots[i] = slots[j]; slots[j] = swap;
                }
                int home = 0;
                for (int i = 0; i < slots.Length; i++) if (slots[i] == i) home++;
                if (home <= slots.Length / 3) break;
                if (attempt > 64) { Rotate(); break; } // unreachable in practice; a rotation is never home
            }
        }

        // Every piece one slot along: nothing starts in place.
        private void Rotate()
        {
            for (int i = 0; i < slots.Length; i++) slots[i] = (i + 1) % slots.Length;
        }

        public int At(int slot) { return slots[slot]; }
        public bool InPlace(int slot) { return slots[slot] == slot; }

        public Step Select(int slot)
        {
            if (Solved || slot < 0 || slot >= slots.Length) return Step.Ignored;
            if (Picked == slot) { Picked = -1; return Step.PutDown; }
            if (InPlace(slot)) return Step.Ignored;
            if (Picked < 0) { Picked = slot; return Step.Picked; }
            int other = Picked;
            Picked = -1;
            int swap = slots[slot]; slots[slot] = slots[other]; slots[other] = swap;
            Moves++;
            Solved = true;
            for (int i = 0; i < slots.Length; i++) if (slots[i] != i) { Solved = false; break; }
            return Step.Swapped;
        }
    }
}
