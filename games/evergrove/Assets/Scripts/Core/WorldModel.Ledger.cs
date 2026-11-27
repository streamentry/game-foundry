using System;

namespace Evergrove.Core
{
    /// <summary>Why seeds moved. Stored as its number in the save, so values never change once shipped.</summary>
    public enum SeedReason
    {
        FirstSolve = 1,
        Replay = 2,
        Nursery = 3,
        Keepsake = 4,
        ParcelSent = 5,
        ParcelOpened = 6,
        /// <summary>A good-neighbour pairing discovered for the first time (Pairings.cs: DiscoverPairing).</summary>
        Pairing = 7,
        /// <summary>A seasonal yield (summer seeds); reserved for the run-3 seasons branch, which routes through ChangeSeeds.</summary>
        Season = 8
    }

    /// <summary>One line of the seed ledger: a change to the seed count and why it happened.</summary>
    public struct LedgerLine
    {
        public SeedReason Reason;
        public int Delta;
    }

    /// <summary>
    /// v1 #78: the seed ledger. Every change to the seed count is recorded with its reason and the
    /// amount actually applied (after the MaxSeeds cap and the floor at zero), newest kept, so a player
    /// can see where their seeds came from and went. The ledger only describes seeds; it never moves any.
    /// </summary>
    public sealed partial class WorldModel
    {
        /// <summary>Most ledger lines kept; appending past this drops the oldest.</summary>
        public const int MaxLedger = 20;

        /// <summary>The ledger, newest first. A copy: changing it changes nothing.</summary>
        public LedgerLine[] Ledger
        {
            get
            {
                int count = state.ledger.Length / 2;
                var lines = new LedgerLine[count];
                for (int i = 0; i < count; i++)
                {
                    int at = state.ledger.Length - 2 - i * 2;
                    lines[i] = new LedgerLine { Reason = (SeedReason)state.ledger[at], Delta = state.ledger[at + 1] };
                }
                return lines;
            }
        }

        /// <summary>
        /// The only place seeds change. Applies <paramref name="delta"/> clamped to 0..MaxSeeds and records
        /// the amount actually applied; a change that applies nothing writes no line.
        /// </summary>
        private void ChangeSeeds(int delta, SeedReason reason)
        {
            int before = state.seeds;
            state.seeds = Math.Max(0, Math.Min(MaxSeeds, before + delta));
            int applied = state.seeds - before;
            if (applied != 0) state.ledger = AppendLedger(state.ledger, (int)reason, applied);
        }

        /// <summary>Appends one line and drops the oldest past MaxLedger. Always builds a new array.</summary>
        private static int[] AppendLedger(int[] flat, int reason, int delta)
        {
            int keep = Math.Min(flat.Length / 2, MaxLedger - 1);
            var next = new int[keep * 2 + 2];
            Array.Copy(flat, flat.Length - keep * 2, next, 0, keep * 2);
            next[keep * 2] = reason;
            next[keep * 2 + 1] = delta;
            return next;
        }

        /// <summary>
        /// Fail-closed validation of the ledger: pairs of a defined reason and a non-zero delta within
        /// MaxSeeds. Only a Living garden may hold any. A missing array means "absent in an older save".
        /// </summary>
        private static bool ReadLedger(WorldSnapshot source, out int[] ledger)
        {
            ledger = null;
            int[] flat = source.ledger ?? Array.Empty<int>();
            if (flat.Length % 2 != 0 || flat.Length > 2 * MaxLedger || (source.mode != "living" && flat.Length != 0))
                return false;
            for (int i = 0; i < flat.Length; i += 2)
                if (!Enum.IsDefined(typeof(SeedReason), flat[i]) || flat[i + 1] == 0 ||
                    flat[i + 1] < -MaxSeeds || flat[i + 1] > MaxSeeds)
                    return false;
            ledger = (int[])flat.Clone();
            return true;
        }
    }
}
