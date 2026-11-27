using System;
using System.Collections.Generic;

namespace Evergrove.Core
{
    /// <summary>
    /// Smart companion SC02: which true fact each neighbour mentions now. Pure and deterministic:
    /// weights from the spec, then a hash of (garden, day, key) for ties. No random numbers, so
    /// tours and tests repeat exactly.
    /// <list type="bullet">
    /// <item>Never the same key within <see cref="KeyDays"/> days, unless it is still waiting for you.</item>
    /// <item>One remark per neighbour per visit: once said, it stays their remark until the visit ends,
    /// unless you just did something for them (a fresh fact, such as fruit handed over).</item>
    /// <item>A fact goes to its keeper; an anyone-fact goes to one neighbour who has nothing better.</item>
    /// <item>Nothing worth saying: none, and the story's own line speaks.</item>
    /// </list>
    /// </summary>
    public static class CompanionRanker
    {
        public const int KeyDays = 3;
        public const int VariantDays = 14;
        public const int NewWeight = 30;
        public const int OwnLineWeight = 10;

        /// <summary>
        /// The fact <paramref name="speaker"/> mentions, or null. <paramref name="canSay"/> says whether
        /// a voice has words for a fact (null: every voice can say every fact meant for it).
        /// <paramref name="spokenThisVisit"/> is the key this speaker already said this visit, if any.
        /// </summary>
        public static Fact Pick(IReadOnlyList<Fact> facts, CompanionMemory memory, int speaker, int day, int garden,
            string spokenThisVisit = null, Func<Fact, int, bool> canSay = null)
        {
            if (facts == null || speaker < 0 || speaker > 2) return null;
            memory = memory ?? CompanionMemory.Empty;
            if (spokenThisVisit != null)
            {
                // Something you just did for them comes first, even mid-visit.
                Fact fresh = Best(facts, memory, speaker, day, garden, canSay, f => f.Fresh && f.Keeper == speaker && f.Key != spokenThisVisit);
                if (fresh != null) return fresh;
                foreach (Fact f in facts)
                    if (f.Key == spokenThisVisit && f.For(speaker) && Sayable(f, speaker, canSay)) return f;
                return null; // what they said is no longer true (a letter now read): no second remark this visit
            }
            return Allot(facts, memory, day, garden, canSay)[speaker];
        }

        /// <summary>What each of the three would mention now, with anyone-facts shared out (index = neighbour).</summary>
        public static Fact[] Allot(IReadOnlyList<Fact> facts, CompanionMemory memory, int day, int garden,
            Func<Fact, int, bool> canSay = null)
        {
            memory = memory ?? CompanionMemory.Empty;
            var chosen = new Fact[3];
            var best = new int[3];
            for (int k = 0; k < 3; k++)
            {
                int who = k;
                chosen[k] = Best(facts, memory, k, day, garden, canSay, f => f.Keeper == who);
                best[k] = chosen[k] != null ? Score(chosen[k], k, memory) : int.MinValue;
            }
            // Anyone-facts, strongest first, each to one neighbour with nothing better.
            var shared = new List<Fact>();
            foreach (Fact f in facts)
                if (f.Keeper == Fact.AnyKeeper && Eligible(f, memory, day)) shared.Add(f);
            shared.Sort((a, b) => Order(a, b, -1, memory, day, garden));
            var given = new bool[3];
            foreach (Fact f in shared)
            {
                int to = -1;
                uint toHash = 0;
                for (int k = 0; k < 3; k++)
                {
                    if (given[k] || !Sayable(f, k, canSay) || Score(f, k, memory) <= best[k]) continue;
                    uint h = Hash(garden, day, f.Key + "#" + k);
                    if (to < 0 || h < toHash) { to = k; toHash = h; }
                }
                if (to < 0) continue;
                chosen[to] = f;
                best[to] = Score(f, to, memory);
                given[to] = true;
            }
            return chosen;
        }

        /// <summary>The weight a fact has for a speaker: its own, +30 never heard, +10 their own line of work.</summary>
        public static int Score(Fact fact, int speaker, CompanionMemory memory)
        {
            int score = fact.Weight;
            if (memory == null || !memory.EverSaid(fact.Key)) score += NewWeight;
            if (fact.Keeper == speaker) score += OwnLineWeight;
            return score;
        }

        /// <summary>Not heard in the last <see cref="KeyDays"/> days, or still waiting, or caused just now; a once-fact, never heard.</summary>
        public static bool Eligible(Fact fact, CompanionMemory memory, int day)
        {
            if (memory == null) return true;
            if (fact.Once && memory.EverSaid(fact.Key)) return false; // SC04: said once is said
            return fact.Waiting || fact.Fresh || !memory.SaidWithin(fact.Key, day, KeyDays);
        }

        /// <summary>
        /// Which of <paramref name="count"/> wordings to use: a hash of (garden, day, key), stepping past
        /// any heard in this voice within <see cref="VariantDays"/> days; if all were, the longest ago.
        /// </summary>
        public static int Variant(FactKind kind, int speaker, int count, int garden, int day, string key, CompanionMemory memory)
        {
            if (count <= 0) return -1;
            int start = (int)(Hash(garden, day, key) % (uint)count);
            int oldest = start, oldestDay = int.MaxValue;
            for (int i = 0; i < count; i++)
            {
                int v = (start + i) % count;
                if (memory == null || !memory.VariantWithin(kind, speaker, v, day, VariantDays)) return v;
                int last = memory.LastHeard(kind, speaker, v);
                if (last < oldestDay) { oldestDay = last; oldest = v; }
            }
            return oldest;
        }

        /// <summary>FNV-1a over "garden|day|key": stable across runs and platforms (string.GetHashCode is not).</summary>
        public static uint Hash(int garden, int day, string key)
        {
            uint h = 2166136261;
            string text = garden + "|" + day + "|" + (key ?? "");
            foreach (char c in text)
            {
                h ^= c;
                h *= 16777619;
            }
            return h;
        }

        private static Fact Best(IReadOnlyList<Fact> facts, CompanionMemory memory, int speaker, int day, int garden,
            Func<Fact, int, bool> canSay, Predicate<Fact> which)
        {
            Fact best = null;
            foreach (Fact f in facts)
            {
                if (!which(f) || !f.For(speaker) || !Eligible(f, memory, day) || !Sayable(f, speaker, canSay)) continue;
                if (best == null || Order(f, best, speaker, memory, day, garden) < 0) best = f;
            }
            return best;
        }

        // Higher score first; then the lower hash of (garden, day, key).
        private static int Order(Fact a, Fact b, int speaker, CompanionMemory memory, int day, int garden)
        {
            int sa = Score(a, speaker, memory), sb = Score(b, speaker, memory);
            if (sa != sb) return sb.CompareTo(sa);
            return Hash(garden, day, a.Key).CompareTo(Hash(garden, day, b.Key));
        }

        private static bool Sayable(Fact f, int speaker, Func<Fact, int, bool> canSay)
        {
            return canSay == null || canSay(f, speaker);
        }
    }
}
