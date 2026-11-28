using System;
using System.Collections.Generic;
using System.Globalization;

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
        // CP07: a waiting fact said on NagTimes of the last NagDays days loses NagDamp for that neighbour.
        public const int NagDays = 3;
        public const int NagTimes = 2;
        public const int NagDamp = 40;
        // CP09: a neighbour who used the same fact kind yesterday loses KindCooldown (a penalty, not a ban).
        public const int KindCooldown = 15;

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
                best[k] = chosen[k] != null ? Score(chosen[k], k, memory, day) : int.MinValue;
            }
            // Anyone-facts, strongest first, each to one neighbour with nothing better (CP08: the one silent longest).
            var shared = new List<Fact>();
            foreach (Fact f in facts)
                if (f.Keeper == Fact.AnyKeeper && Eligible(f, memory, day)) shared.Add(f);
            shared.Sort((a, b) => Order(a, b, -1, memory, day, garden));
            var given = new bool[3];
            foreach (Fact f in shared)
            {
                int to = -1;
                int toSpoke = 0;
                uint toHash = 0;
                for (int k = 0; k < 3; k++)
                {
                    if (given[k] || !Sayable(f, k, canSay) || Score(f, k, memory, day) <= best[k]) continue;
                    // CP08: whoever spoke longest ago (never: int.MinValue) first; the hash only breaks a tie.
                    int spoke = memory.LastSpoke(k);
                    uint h = Hash(garden, day, f.Key + "#" + k);
                    if (to < 0 || spoke < toSpoke || (spoke == toSpoke && h < toHash)) { to = k; toSpoke = spoke; toHash = h; }
                }
                if (to < 0) continue;
                chosen[to] = f;
                best[to] = Score(f, to, memory, day);
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

        /// <summary>
        /// CP07: the weight as <see cref="Score(Fact, int, CompanionMemory)"/>, less <see cref="NagDamp"/> when a
        /// waiting fact (not fresh) was said on <see cref="NagTimes"/> of the <see cref="NagDays"/> days before
        /// today. Yesterday is the window's end: today's repeats are the visit lock's job.
        /// CP09: less <see cref="KindCooldown"/> when this neighbour used the same kind yesterday (waiting and
        /// fresh facts exempt).
        /// </summary>
        public static int Score(Fact fact, int speaker, CompanionMemory memory, int day)
        {
            return Score(fact, speaker, memory) - NagDampFor(fact, speaker, memory, day) - CooldownFor(fact, speaker, memory, day);
        }

        // CP10: the nag penalty Score takes (0 or NagDamp), shared with Explain.
        private static int NagDampFor(Fact fact, int speaker, CompanionMemory memory, int day)
        {
            return fact.Waiting && !fact.Fresh && memory != null &&
                memory.DaysHeardBy(fact.Key, speaker, day - 1, NagDays) >= NagTimes ? NagDamp : 0;
        }

        // CP10: the kind-cooldown penalty Score takes (0 or KindCooldown), shared with Explain.
        private static int CooldownFor(Fact fact, int speaker, CompanionMemory memory, int day)
        {
            return !fact.Waiting && !fact.Fresh && memory != null && speaker >= 0 && speaker <= 2 &&
                memory.KindSaidOn(fact.Kind, speaker, day - 1) ? KindCooldown : 0;
        }

        /// <summary>
        /// CP10: a plain breakdown for QA logs and tests, best first, one line per fact meant for
        /// <paramref name="speaker"/>; ineligible facts come after the eligible ones. Each line reads
        /// "Kind Key score= base= new= own= nag= cooldown= eligible|not-eligible:reason", with the same
        /// score and eligibility the ranker uses. Pure: no side effects.
        /// </summary>
        public static IReadOnlyList<string> Explain(IReadOnlyList<Fact> facts, CompanionMemory memory, int speaker, int day, int garden,
            Func<Fact, int, bool> canSay = null, int top = 3)
        {
            var lines = new List<string>();
            if (facts == null || speaker < 0 || speaker > 2 || top <= 0) return lines;
            memory = memory ?? CompanionMemory.Empty;
            var eligible = new List<Fact>();
            var ineligible = new List<Fact>();
            foreach (Fact f in facts)
            {
                if (f == null || !f.For(speaker)) continue;
                if (Eligible(f, memory, day) && Sayable(f, speaker, canSay)) eligible.Add(f);
                else ineligible.Add(f);
            }
            Comparison<Fact> best = (a, b) => Order(a, b, speaker, memory, day, garden);
            eligible.Sort(best);
            ineligible.Sort(best);
            var ordered = new List<Fact>(eligible);
            ordered.AddRange(ineligible);
            for (int i = 0; i < ordered.Count && lines.Count < top; i++)
            {
                Fact f = ordered[i];
                int nag = -NagDampFor(f, speaker, memory, day);
                int cooldown = -CooldownFor(f, speaker, memory, day);
                int added = memory.EverSaid(f.Key) ? 0 : NewWeight;
                int own = f.Keeper == speaker ? OwnLineWeight : 0;
                string status = i < eligible.Count ? "eligible" : "not-eligible:" + Reason(f, memory, day);
                lines.Add(string.Format(CultureInfo.InvariantCulture,
                    "{0} {1} score={2} base={3} new={4} own={5} nag={6} cooldown={7} {8}",
                    f.Kind, f.Key, Score(f, speaker, memory, day), f.Weight,
                    Signed(added), Signed(own), Signed(nag), Signed(cooldown), status));
            }
            return lines;
        }

        // CP10: why a fact is not eligible: said once, said within KeyDays, or no words for this voice.
        private static string Reason(Fact fact, CompanionMemory memory, int day)
        {
            if (fact.Once && memory.EverSaid(fact.Key)) return "once-said";
            if (!fact.Waiting && !fact.Fresh && memory.UnvariedWithin(fact.Key, day, VariantDays)) return "same-words-within-14-days";
            if (!Eligible(fact, memory, day)) return "said-within-3-days";
            return "cannot-say";
        }

        // CP10: "+30", "-40" or "0", as the Explain line shows them.
        private static string Signed(int value)
        {
            if (value == 0) return "0";
            return (value > 0 ? "+" : "-") + Math.Abs(value).ToString(CultureInfo.InvariantCulture);
        }

        /// <summary>Not heard in the last <see cref="KeyDays"/> days, or still waiting, or caused just now; a once-fact, never heard.</summary>
        public static bool Eligible(Fact fact, CompanionMemory memory, int day)
        {
            if (memory == null) return true;
            if (fact.Once && memory.EverSaid(fact.Key)) return false; // SC04: said once is said
            if (fact.Waiting || fact.Fresh) return true; // still waiting, or caused just now: always worth saying
            // CP12: a fixed sentence has no other wording to step to, so it waits VariantDays, not KeyDays.
            if (memory.UnvariedWithin(fact.Key, day, VariantDays)) return false;
            return !memory.SaidWithin(fact.Key, day, KeyDays);
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
            int sa = Score(a, speaker, memory, day), sb = Score(b, speaker, memory, day);
            if (sa != sb) return sb.CompareTo(sa);
            int byHash = Hash(garden, day, a.Key).CompareTo(Hash(garden, day, b.Key));
            if (byHash != 0) return byHash;
            // CP05: two keys with the same hash still order the same way, whatever the list order.
            int byKey = string.CompareOrdinal(a.Key, b.Key);
            return byKey != 0 ? byKey : a.Kind.CompareTo(b.Kind);
        }

        private static bool Sayable(Fact f, int speaker, Func<Fact, int, bool> canSay)
        {
            return canSay == null || canSay(f, speaker);
        }
    }
}
