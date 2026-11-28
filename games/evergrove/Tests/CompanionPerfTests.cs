using System;
using System.Collections.Generic;
using System.Diagnostics;
using Evergrove.Core;

/// <summary>
/// Smart companion CP20: the app calls CompanionFacts.For and CompanionRanker.Allot for the three
/// neighbours on each rebuild, so the largest realistic living garden must stay cheap on a phone.
/// Median per-iteration time is asserted below 2 ms (p95 is printed only). Set
/// EVERGROVE_SKIP_TIMING=1 on a busy machine to keep the printout and skip the assertion.
/// </summary>
internal static class CompanionPerfTests
{
    private const int Warmup = 20;
    private const int Iterations = 200;
    private const double MedianBudgetMs = 2.0;

    private static int assertions;

    private static void Expect(bool ok, string message)
    {
        assertions++;
        if (!ok) throw new Exception("Companion perf test failed: " + message);
    }

    public static int Run()
    {
        assertions = 0;
        GardenMap map = GardenMap.River(10, 15, CompanionFactsTests.Mask(), 3);
        WorldModel world = LargestGarden();
        DateTime today = new DateTime(2026, 11, 28);
        int day = CompanionContext.DayNumberOf(today);
        var context = new CompanionContext(today, lastSeen: today.AddDays(-2), map: map,
            hourCounts: new[] { 1, 4, 9, 2, 7, 0 });
        Expect(world.KeepNote("Anh", "The plums are early this year."), "a letter waiting in the box");
        CompanionMemory memory = MemoryOf60Days(day);

        IReadOnlyList<Fact> sample = CompanionFacts.For(world, context);
        Expect(sample.Count >= 20, "the largest garden has a full feed: " + sample.Count);

        for (int i = 0; i < Warmup; i++) Once(world, context, memory, day);

        var times = new double[Iterations];
        for (int i = 0; i < Iterations; i++)
        {
            var clock = Stopwatch.StartNew();
            Once(world, context, memory, day);
            clock.Stop();
            times[i] = clock.Elapsed.TotalMilliseconds;
        }
        Array.Sort(times);
        double median = times[Iterations / 2];
        double p95 = times[(int)Math.Ceiling(Iterations * 0.95) - 1];

        Console.WriteLine("Companion perf: median " + Micros(median) + " us, p95 " + Micros(p95) + " us over " +
            Iterations + " iterations (" + sample.Count + " facts, " + memory.Count + " memory entries, Release " +
            "build recommended).");

        bool skip = Environment.GetEnvironmentVariable("EVERGROVE_SKIP_TIMING") == "1";
        if (skip)
        {
            Console.WriteLine("Companion perf: EVERGROVE_SKIP_TIMING=1, budget assertion skipped.");
            return assertions;
        }
        Expect(median < MedianBudgetMs, "median under " + MedianBudgetMs + " ms (" + Micros(median) + " us)");
        return assertions;
    }

    private static void Once(WorldModel world, CompanionContext context, CompanionMemory memory, int day)
    {
        IReadOnlyList<Fact> facts = CompanionFacts.For(world, context);
        CompanionRanker.Allot(facts, memory, day, world.Garden, (f, speaker) => true);
    }

    private static string Micros(double ms)
    {
        return (ms * 1000.0).ToString("F1", System.Globalization.CultureInfo.InvariantCulture);
    }

    // Grown(12) with every tree solved: the most the living garden can show at once.
    private static WorldModel LargestGarden()
    {
        WorldModel world = CompanionFactsTests.Grown(12);
        for (int i = 0; i < world.Choices.Length; i++) world.SolveTree(i);
        world.GiveFruit(0);
        return world;
    }

    // 300 remarks (the memory cap) spread over 60 days, oldest first so none is redated.
    private static CompanionMemory MemoryOf60Days(int today)
    {
        CompanionMemory memory = CompanionMemory.Empty;
        var kinds = (FactKind[])Enum.GetValues(typeof(FactKind));
        for (int i = 0; i < CompanionMemory.MaxEntries; i++)
        {
            int day = today - 59 + i / 5;
            memory = memory.With(new Heard(day, i % 3, kinds[i % kinds.Length], i % 4, "perf:" + i));
        }
        Expect(memory.Count == CompanionMemory.MaxEntries, "the memory holds 300 entries: " + memory.Count);
        return memory;
    }
}
