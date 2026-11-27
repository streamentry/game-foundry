namespace Evergrove.Core
{
    /// <summary>
    /// "Speak less" (idea 85): neighbours can be quieter. With the setting on, a remark
    /// opportunity is kept only when Keep returns true, so about half of them are spoken and
    /// the rest stay silent. Keep is a pure function of (day, villager, slot): the same
    /// opportunity always gives the same answer, and no clock or randomness is read here.
    /// Letters, gifts and essential notices are not remarks and never pass through this gate.
    /// </summary>
    public static class QuietRemarks
    {
        /// <summary>True when this remark opportunity is spoken under "speak less".</summary>
        public static bool Keep(int day, int villager, int slot)
        {
            unchecked
            {
                uint h = 2166136261u;
                h = Mix(h, (uint)day);
                h = Mix(h, (uint)villager);
                h = Mix(h, (uint)slot);
                // MurmurHash3 finaliser: spreads the low bits so that parity is unbiased.
                h ^= h >> 16;
                h *= 0x85ebca6bu;
                h ^= h >> 13;
                h *= 0xc2b2ae35u;
                h ^= h >> 16;
                return (h & 1u) == 0;
            }
        }

        private static uint Mix(uint hash, uint value)
        {
            unchecked
            {
                for (int shift = 0; shift < 32; shift += 8)
                {
                    hash ^= (value >> shift) & 0xffu;
                    hash *= 16777619u;
                }
                return hash;
            }
        }
    }
}
