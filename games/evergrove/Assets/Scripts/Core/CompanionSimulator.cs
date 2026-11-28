using System;
using System.Collections.Generic;

namespace Evergrove.Core
{
    /// <summary>CP11: one remark a neighbour makes in a simulated visit.</summary>
    public readonly struct SimLine
    {
        public readonly int Day;
        public readonly int Speaker;
        public readonly FactKind Kind;
        public readonly string Key;
        public readonly int Variant;
        public readonly string Words;

        public SimLine(int day, int speaker, FactKind kind, string key, int variant, string words)
        {
            Day = day;
            Speaker = speaker;
            Kind = kind;
            Key = key;
            Variant = variant;
            Words = words;
        }
    }

    /// <summary>
    /// CP11: runs the companion's visits without the app. Each day, each neighbour 0..2 is tapped once,
    /// in order, using the same ranker and wording as the app (CompanionRanker.Pick, CompanionLocale.Words).
    /// Read-only: the world is only read, and memory is copied forward, never changed in place.
    /// </summary>
    public static class CompanionSimulator
    {
        public static IReadOnlyList<SimLine> Run(WorldModel world, Func<int, CompanionContext> contextFor, int days,
            LineBank bank, string locale = CompanionLocale.English, CompanionMemory memory = null)
        {
            var transcript = new List<SimLine>();
            if (world == null || contextFor == null || days <= 0) return transcript;
            bank = bank ?? LineBank.Empty;
            memory = memory ?? CompanionMemory.Empty;
            for (int d = 0; d < days; d++)
            {
                CompanionContext context = contextFor(d);
                int day = context.DayNumber;
                IReadOnlyList<Fact> facts = CompanionFacts.For(world, context);
                for (int speaker = 0; speaker < 3; speaker++)
                {
                    // CP11: a visit is one tap per neighbour, so no remark is carried over within the visit.
                    Fact fact = CompanionRanker.Pick(facts, memory, speaker, day, world.Garden, null,
                        (f, who) => CompanionLocale.CanSay(locale, bank, f, who));
                    if (fact == null) continue;
                    string words = CompanionLocale.Words(locale, bank, fact, speaker, world.Garden, day, memory, -1, out int variant);
                    if (words == null) continue;
                    transcript.Add(new SimLine(day, speaker, fact.Kind, fact.Key, variant, words));
                    memory = memory.With(new Heard(day, speaker, fact.Kind, variant, fact.Key));
                }
            }
            return transcript;
        }
    }
}
