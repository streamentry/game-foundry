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

        /// <summary>
        /// I292 (idea 7, after shogi's promotion): crowned once a replay has been solved at
        /// the deepest tier. Replays done counts solved replays; the last was dealt at
        /// Tier(base, done - 1). A first solve alone is never crowned, however deep.
        /// </summary>
        /// <summary>
        /// I300 (idea 21, after bonsai, where patience shows in the tree): the size a thing
        /// is drawn at, growing 7% for each tier its replays have deepened it. A thing that
        /// began at the deepest tier has no further to deepen (the crown marks it, I292).
        /// </summary>
        public static float Maturity(int baseTier, int replaysDone)
        {
            int deepened = Tier(baseTier, replaysDone) - Math.Max(0, Math.Min(TreePuzzles.Tiers - 1, baseTier));
            return 1f + 0.07f * Math.Max(0, deepened);
        }

        public static bool Crowned(int baseTier, int replaysDone)
        {
            return replaysDone > 0 && Tier(baseTier, replaysDone - 1) == TreePuzzles.Tiers - 1;
        }
    }
}
