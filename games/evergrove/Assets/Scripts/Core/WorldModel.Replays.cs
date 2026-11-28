using System;

namespace Evergrove.Core
{
    public sealed partial class WorldModel
    {
        // ---- v1 #80: the first solves of a day pay fully; later replays pay less, then nothing.
        // Play stays open and farming stays pointless. First solves never count toward the cap.
        public const int FullReplaysPerDay = 3;
        public const int SmallReplaysPerDay = 5;
        public const int SmallReplayReward = 1;
        /// <summary>Upper bound of the replay counter in a save; past the paid replays it only needs to stay here.</summary>
        public const int MaxReplaysPaid = 1000;
        private const int MinPayDay = 19000101;
        private const int MaxPayDay = 99991231;

        /// <summary>
        /// What the next replay of a tier pays on <paramref name="day"/>, without changing anything. Day 0 (or less)
        /// is the uncapped replay reward of old saves.
        /// </summary>
        public int ReplayRewardOn(int tier, int day)
        {
            if (day <= 0) return TreeReward(tier, false);
            return ReplayPay(tier, day > state.payDay ? 0 : state.replaysPaid);
        }

        /// <summary>The replay pay after <paramref name="paidSoFar"/> paid replays on one day.</summary>
        private static int ReplayPay(int tier, int paidSoFar)
        {
            if (paidSoFar < FullReplaysPerDay) return TreeReward(tier, false);
            return paidSoFar < FullReplaysPerDay + SmallReplaysPerDay ? SmallReplayReward : 0;
        }

        /// <summary>
        /// Pays one replay against the day's cap and counts it. Day 0 pays uncapped, as before.
        /// <paramref name="full"/> is true when this replay is one of the day's full-paid replays (v1 #80: fruit follows them).
        /// </summary>
        private int PayReplay(int tier, int day, out bool full)
        {
            if (day <= 0) { full = true; return TreeReward(tier, false); }
            // Only a later day starts a fresh count: setting the clock back counts against the
            // latest day already paid, so switching between two dates cannot reset the cap.
            if (day > state.payDay)
            {
                state.payDay = day;
                state.replaysPaid = 0;
            }
            full = state.replaysPaid < FullReplaysPerDay;
            int reward = ReplayPay(tier, state.replaysPaid);
            state.replaysPaid = Math.Min(MaxReplaysPaid, state.replaysPaid + 1);
            return reward;
        }

        /// <summary>
        /// Credits a solve: the first solve pays in full, a replay pays under the day's cap. Fruit comes with the
        /// first solve and with each full-paid replay (v1 #80), so a tree cannot be farmed past the daily cap;
        /// <paramref name="shadeTree"/> says whether the solved thing is a ShadeTree.
        /// </summary>
        private int CreditSolve(int tier, bool first, int day, bool shadeTree)
        {
            int reward;
            bool full = true;
            if (first) reward = TreeReward(tier, true);
            else reward = PayReplay(tier, day, out full);
            ChangeSeeds(reward, first ? SeedReason.FirstSolve : SeedReason.Replay);
            if (shadeTree && (first || full)) PickFruit();
            return reward;
        }

        // ---- checklist 41: once nothing new is left to solve, replays slowly open the river.
        // Counted in the save as riverReplays (never a stretch count, so the format has no maximum).
        // Clamped to 8 safety-bound stretches' worth of replays: far past any real garden, never a gameplay cap.
        public const int MaxRiverReplays = GardenMap.ReplaysPerReach * GardenMap.SafetyReachBound;

        /// <summary>Replays made while every owned thing was already solved (checklist 41).</summary>
        public int RiverReplays { get { return state.riverReplays; } }

        /// <summary>
        /// True when every story choice made so far and every bought plant is already solved, so a further
        /// solve can only be a replay. A new story choice or a new plant makes this false again.
        /// </summary>
        public bool NothingNewToSolve
        {
            get { return state.treeSolved.Length == state.choices.Length && state.boughtSolved.Length == state.bought.Length / 2; }
        }

        /// <summary>
        /// How close the next stretch is (I224, checklist 41), for the drops on its mist. Uses replays once nothing
        /// new is left to solve; see <see cref="GardenMap.TowardNext(int, bool, int, int, bool, out int, out int)"/>.
        /// </summary>
        public bool TowardNextStretch(out int have, out int need)
        {
            return GardenMap.TowardNext(state.choices.Length, Completed, PuzzlesSolved, state.riverReplays, NothingNewToSolve,
                out have, out need);
        }

        /// <summary>The replays a save holds (checklist 41): clamped to 0..MaxRiverReplays, and 0 outside a Living garden.</summary>
        public static int RiverReplaysOf(WorldSnapshot source)
        {
            if (source.mode != "living") return 0;
            return Math.Max(0, Math.Min(MaxRiverReplays, source.riverReplays));
        }

        /// <summary>
        /// Credits a solve like CreditSolve, and counts it as a river replay when it is not a first solve and
        /// every owned thing is already solved (checklist 41). A first solve never counts.
        /// </summary>
        private int CreditSolveCounted(int tier, bool first, int day, bool shadeTree)
        {
            int reward = CreditSolve(tier, first, day, shadeTree);
            if (!first && NothingNewToSolve && state.riverReplays < MaxRiverReplays) state.riverReplays++;
            return reward;
        }
    }
}
