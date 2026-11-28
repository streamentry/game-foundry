using System;
using System.Collections.Generic;
using Evergrove.Core;

/// <summary>
/// Smart companion CP16: the fact feed never throws and keeps its invariants on 500 seeded random
/// worlds and contexts (grown gardens with random choices, solves, fruit, letters, dates across
/// 2000-2100, clock set back, odd hour counts and out-of-range keepers).
/// </summary>
internal static class CompanionFuzzTests
{
    private const int Seed = 1609;
    private const int Cases = 500;

    private static int assertions;

    private static void Expect(bool ok, string message)
    {
        assertions++;
        if (!ok) throw new Exception("Companion fuzz test failed: " + message);
    }

    public static int Run()
    {
        assertions = 0;
        var rng = new Random(Seed);
        GardenMap map = GardenMap.River(10, 15, CompanionFactsTests.Mask(), 2);
        var kindsSeen = new HashSet<FactKind>();
        int facts = 0;

        for (int i = 0; i < Cases; i++)
        {
            WorldModel world = RandomWorld(rng, i);
            CompanionContext context = RandomContext(rng, map);
            IReadOnlyList<Fact> feed = CompanionFactsFor(world, context, i);
            Expect(feed != null, "case " + i + ": the feed is a list, never null");
            foreach (Fact f in feed)
            {
                facts++;
                kindsSeen.Add(f.Kind);
                Expect(f.Keeper >= -1 && f.Keeper <= 2, "case " + i + ": keeper in -1..2: " + f);
                Expect(!string.IsNullOrEmpty(f.Key) && f.Key.IndexOf('|') < 0 && f.Key.IndexOf('\n') < 0 && f.Key.IndexOf('\r') < 0,
                    "case " + i + ": key is non-empty with no '|' or newline: " + f.Key);
                Expect(f.Weight >= 0, "case " + i + ": weight is not negative: " + f);
                Expect(f.Once == CompanionFacts.IsOnceKind(f.Kind), "case " + i + ": Once matches IsOnceKind: " + f);
            }
        }
        Console.WriteLine("Companion fuzz CP16: " + Cases + " random worlds and contexts, " + facts + " facts, " +
            kindsSeen.Count + " kinds, no throws.");
        Expect(facts > 0, "the fuzz produced facts to check");

        FreshAndPuzzleOnlyFeeds(map);
        return assertions;
    }

    // Named regression cases, one per Core fault found by the fuzz (none so far).
    private static void FreshAndPuzzleOnlyFeeds(GardenMap map)
    {
        var today = new DateTime(2026, 11, 27);
        Expect(CompanionFacts.For(WorldModel.New(PlayMode.PuzzleOnly), new CompanionContext(today)).Count == 0,
            "CP16: a puzzle-only garden gives an empty feed, not an error");
        Expect(CompanionFacts.For(WorldModel.New(PlayMode.Living), new CompanionContext(today, hourCounts: new[] { -5, -1, 9, 0, 0, 0 }, map: map)) != null,
            "CP16: a fresh living garden with negative hour counts does not throw");
    }

    private static IReadOnlyList<Fact> CompanionFactsFor(WorldModel world, CompanionContext context, int i)
    {
        try
        {
            return CompanionFacts.For(world, context);
        }
        catch (Exception e)
        {
            throw new Exception("Companion fuzz test failed: case " + i + " threw " + e.GetType().Name + ": " + e.Message, e);
        }
    }

    private static WorldModel RandomWorld(Random rng, int i)
    {
        if (i % 10 == 0) return WorldModel.New(PlayMode.Living);
        if (i % 10 == 1) return WorldModel.New(PlayMode.PuzzleOnly);
        WorldModel world = WorldModel.New(PlayMode.Living, rng.Next(0, 10));
        int grown = rng.Next(0, 13);
        for (int chapter = 0; chapter < grown && !world.Completed; chapter++)
        {
            Puzzle board = world.CurrentPuzzle;
            for (int t = 0; t < board.CellCount; t++)
                while (world.Rotations[t] != board.Solution[t]) world.Rotate(t);
            world.Choose((VillageChoice)rng.Next(0, 3));
        }
        int solves = world.Choices.Length == 0 ? 0 : rng.Next(0, world.Choices.Length + 1);
        for (int s = 0; s < solves; s++) world.SolveTree(rng.Next(0, world.Choices.Length));
        if (rng.Next(0, 2) == 0) world.GiveFruit(rng.Next(0, 3));
        if (rng.Next(0, 3) == 0) world.KeepNote(rng.Next(0, 2) == 0 ? "Anh" : "Ruth", "A note, " + rng.Next(0, 100) + ".");
        if (rng.Next(0, 4) == 0) world.ReceiveNeighbourCutting(rng.Next(0, Nursery.Kinds));
        return world;
    }

    private static CompanionContext RandomContext(Random rng, GardenMap map)
    {
        DateTime[] fixedDays = {
            new DateTime(2000, 1, 1), new DateTime(2000, 2, 29), new DateTime(2024, 2, 29),
            new DateTime(2099, 12, 31), new DateTime(2100, 12, 31), new DateTime(2026, 11, 27),
        };
        DateTime today = rng.Next(0, 4) == 0
            ? fixedDays[rng.Next(0, fixedDays.Length)]
            : new DateTime(2000, 1, 1).AddDays(rng.Next(0, 36525));

        DateTime? lastSeen = null;
        switch (rng.Next(0, 4))
        {
            case 1: lastSeen = today.AddDays(-rng.Next(0, 400)); break;  // before today
            case 2: lastSeen = today.AddDays(rng.Next(1, 30)); break;    // clock set back: after today
            case 3: lastSeen = today; break;
        }

        Weather[] weathers = (Weather[])Enum.GetValues(typeof(Weather));
        DayPhase[] phases = (DayPhase[])Enum.GetValues(typeof(DayPhase));
        PuzzleKind[] kinds = (PuzzleKind[])Enum.GetValues(typeof(PuzzleKind));

        return new CompanionContext(
            today,
            lastSeen: lastSeen,
            lettersRead: rng.Next(-5, 201),
            weather: weathers[rng.Next(0, weathers.Length)],
            lightOn: rng.Next(0, 2) == 0,
            phase: phases[rng.Next(0, phases.Length)],
            fruitJustGivenTo: rng.Next(-1, 4),
            togetherKeeper: rng.Next(-1, 4),
            togetherKind: kinds[rng.Next(0, kinds.Length)],
            map: rng.Next(0, 2) == 0 ? null : map,
            hourCounts: RandomHours(rng));
    }

    private static int[] RandomHours(Random rng)
    {
        switch (rng.Next(0, 4))
        {
            case 0: return null;
            case 1: return new int[rng.Next(0, 12) == 6 ? 6 : rng.Next(0, 6)]; // wrong length, or empty
            case 2:
                int[] counts = new int[Companion.PhaseCount];
                for (int i = 0; i < counts.Length; i++) counts[i] = rng.Next(-20, 40);
                return counts;
            default:
                int[] habit = new int[Companion.PhaseCount];
                habit[rng.Next(0, habit.Length)] = rng.Next(0, 20);
                return habit;
        }
    }
}
