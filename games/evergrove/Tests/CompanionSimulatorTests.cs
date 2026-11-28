using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Evergrove.Core;

/// <summary>
/// CP11: the simulator mirrors the app's visits. Deterministic for the same input, complete (every
/// line has words), and read-only on the world and on the memory it was given.
/// </summary>
internal static class CompanionSimulatorTests
{
    private static int assertions;

    private static void Expect(bool ok, string message)
    {
        assertions++;
        if (!ok) throw new Exception("Companion simulator test failed: " + message);
    }

    private static readonly DateTime Start = new DateTime(2026, 11, 27);
    private static readonly Weather[] Cycle = { Weather.Fair, Weather.Rainy, Weather.Windy, Weather.Hot, Weather.Snowy };

    public static int Run()
    {
        assertions = 0;
        TranscriptIsCompleteAndDeterministic();
        SameMemoryGivesSameTranscript();
        return assertions;
    }

    private static CompanionContext ContextFor(int day)
    {
        return new CompanionContext(Start.AddDays(day), weather: Cycle[day % Cycle.Length], lightOn: true, phase: DayPhase.Day);
    }

    private static LineBank LoadBank()
    {
        return LineBank.Parse(File.ReadAllText(Path.Combine(Root(), "games/evergrove/Assets/Resources/Evergrove/Lines/en.json")));
    }

    private static IReadOnlyList<SimLine> Simulate(WorldModel world, LineBank bank, CompanionMemory memory = null)
    {
        return CompanionSimulator.Run(world, ContextFor, 7, bank, CompanionLocale.English, memory);
    }

    private static string Render(IReadOnlyList<SimLine> transcript)
    {
        var text = new StringBuilder();
        foreach (SimLine line in transcript)
            text.Append(line.Day).Append('|').Append(line.Speaker).Append('|').Append(line.Kind).Append('|')
                .Append(line.Key).Append('|').Append(line.Variant).Append('|').Append(line.Words).Append('\n');
        return text.ToString();
    }

    private static void TranscriptIsCompleteAndDeterministic()
    {
        LineBank bank = LoadBank();
        WorldModel world = CompanionFactsTests.Grown(9);
        world.SolveTree(0);
        Expect(CompanionSimulator.Run(world, ContextFor, 0, bank).Count == 0, "zero days is an empty transcript");

        IReadOnlyList<SimLine> first = Simulate(world, bank);
        IReadOnlyList<SimLine> second = Simulate(world, bank);
        Expect(first.Count > 0, "seven days in a tended garden say something");
        Expect(Render(first) == Render(second), "two runs over the same world are equal");
        foreach (SimLine line in first)
        {
            Expect(!string.IsNullOrWhiteSpace(line.Words), "every simulated line has words: " + line.Key);
            int firstDay = CompanionContext.DayNumberOf(Start);
            Expect(line.Day >= firstDay && line.Day < firstDay + 7, "lines fall on the simulated days");
            Expect(line.Speaker >= 0 && line.Speaker <= 2, "speakers are the three neighbours");
        }

        // The simulator only reads: the world's own facts are the same after a run.
        string factsBefore = KeysOf(CompanionFacts.For(world, ContextFor(0)));
        Simulate(world, bank);
        Expect(KeysOf(CompanionFacts.For(world, ContextFor(0))) == factsBefore, "the world is not mutated");
    }

    private static void SameMemoryGivesSameTranscript()
    {
        LineBank bank = LoadBank();
        WorldModel world = CompanionFactsTests.Grown(9);
        world.SolveTree(0);
        CompanionMemory seed = CompanionMemory.Empty.With(new Heard(CompanionContext.DayNumberOf(Start) - 1, 0, FactKind.FruitGiven, 0, "seed"));

        IReadOnlyList<SimLine> a = Simulate(world, bank, seed);
        IReadOnlyList<SimLine> b = Simulate(world, bank, seed);
        Expect(Render(a) == Render(b), "the same memory gives the same transcript");
        Expect(seed.Count == 1, "the caller's memory is not changed by a run");
    }

    private static string KeysOf(IReadOnlyList<Fact> facts)
    {
        var text = new StringBuilder();
        foreach (Fact f in facts) text.Append(f.Kind).Append(':').Append(f.Key).Append(';');
        return text.ToString();
    }

    private static string Root()
    {
        string dir = AppContext.BaseDirectory;
        while (dir != null && !File.Exists(Path.Combine(dir, "games/evergrove/docs/specs/companion-line-rules.json")))
            dir = Path.GetDirectoryName(dir);
        if (dir == null) throw new Exception("repository root not found");
        return dir;
    }
}
