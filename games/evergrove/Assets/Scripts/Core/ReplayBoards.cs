using System;

namespace Evergrove.Core
{
    /// <summary>
    /// I259/I260: replays that stay fresh and deepen. The first play of a thing is always
    /// its own board (seeded by garden and place, so a relative helping plays the same one).
    /// Each replay deals a different board; every few replays the thing's puzzle grows one
    /// tier harder, up to the hardest. Seeds are unchanged: depth is the reward, the economy
    /// stays as it is (first solve always pays more than any replay).
    /// </summary>
    public static class ReplayBoards
    {
        public const int ReplaysPerTier = 3;

        public static int Seed(int baseSeed, int replays)
        {
            if (replays <= 0) return baseSeed;
            unchecked { return baseSeed + replays * 7919 + 13; }
        }

        public static int Tier(int baseTier, int replays)
        {
            int tier = baseTier + Math.Max(0, replays) / ReplaysPerTier;
            return Math.Max(0, Math.Min(TreePuzzles.Tiers - 1, tier));
        }
    }
}
