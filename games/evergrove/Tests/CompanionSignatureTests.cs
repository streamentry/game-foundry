using System;
using Evergrove.Core;

/// <summary>
/// CP37: the fact-list signature. Equal inputs give equal signatures; any input <see cref="CompanionFacts.For"/>
/// reads changes it. The Unity cache trusts this to never serve a stale list.
/// </summary>
internal static class CompanionSignatureTests
{
    private static int assertions;

    private static void Expect(bool ok, string message)
    {
        assertions++;
        if (!ok) throw new Exception("Companion signature test failed: " + message);
    }

    public static int Run()
    {
        assertions = 0;
        var today = new DateTime(2026, 11, 27);
        WorldModel grown = CompanionFactsTests.Grown(3);

        Expect(CompanionFacts.Signature(grown, new CompanionContext(today)) ==
            CompanionFacts.Signature(CompanionFactsTests.Grown(3), new CompanionContext(today)),
            "equal saves and contexts give equal signatures");
        string baseline = CompanionFacts.Signature(grown, new CompanionContext(today));
        Expect(baseline != CompanionFacts.Signature(grown, new CompanionContext(today.AddDays(1))), "a new day changes it");
        Expect(baseline != CompanionFacts.Signature(grown, new CompanionContext(today, weather: Weather.Rainy)), "weather changes it");
        Expect(baseline != CompanionFacts.Signature(grown, new CompanionContext(today, phase: DayPhase.Night)), "the phase changes it");
        Expect(baseline != CompanionFacts.Signature(grown, new CompanionContext(today, lightOn: true)), "the light changes it");
        Expect(baseline != CompanionFacts.Signature(grown, new CompanionContext(today, lastSeen: today.AddDays(-4))), "the last visit changes it");
        Expect(baseline != CompanionFacts.Signature(grown, new CompanionContext(today, lettersRead: 2)), "letters read change it");
        Expect(baseline != CompanionFacts.Signature(grown, new CompanionContext(today, fruitJustGivenTo: 1)), "fruit handed over this visit changes it");
        Expect(baseline != CompanionFacts.Signature(grown, new CompanionContext(today, togetherKeeper: 2, togetherKind: PuzzleKind.Sums)),
            "a puzzle solved together changes it");
        Expect(baseline != CompanionFacts.Signature(grown, new CompanionContext(today, hourCounts: new[] { 0, 0, 1, 0, 0, 0 })),
            "an hour count changes it");
        Expect(CompanionFacts.Signature(grown, new CompanionContext(today, hourCounts: new[] { 0, 1, 0, 0, 0, 0 })) !=
            CompanionFacts.Signature(grown, new CompanionContext(today, hourCounts: new[] { 1, 0, 0, 0, 0, 0 })),
            "the same total in another hour changes it");

        Expect(baseline != CompanionFacts.Signature(CompanionFactsTests.Grown(4), new CompanionContext(today)), "a thing grown changes it");
        Expect(CompanionFacts.Signature(WorldModel.New(PlayMode.Living, 3), new CompanionContext(today)) !=
            CompanionFacts.Signature(WorldModel.New(PlayMode.Living, 4), new CompanionContext(today)), "the garden index changes it");
        Expect(CompanionFacts.Signature(WorldModel.New(PlayMode.Living, 3), new CompanionContext(today)) !=
            CompanionFacts.Signature(WorldModel.New(PlayMode.PuzzleOnly, 3), new CompanionContext(today)), "the mode changes it");

        WorldModel solved = CompanionFactsTests.Grown(3);
        solved.SolveTree(0, 0);
        Expect(baseline != CompanionFacts.Signature(solved, new CompanionContext(today)), "a tree solved changes it");

        WorldModel fruit = Restored(CompanionFactsTests.Grown(3), snap => snap.fruit = 3);
        string withFruit = CompanionFacts.Signature(fruit, new CompanionContext(today));
        fruit.GiveFruit(1);
        Expect(withFruit != CompanionFacts.Signature(fruit, new CompanionContext(today)), "fruit given changes it");

        Expect(CompanionFacts.Signature(null, null).Length > 0, "missing inputs still give a signature");
        Expect(CompanionFacts.Signature(null, new CompanionContext(today)) != CompanionFacts.Signature(grown, new CompanionContext(today)),
            "no save differs from a save");
        return assertions;
    }

    private static WorldModel Restored(WorldModel world, Action<WorldSnapshot> change)
    {
        WorldSnapshot snap = world.Export();
        change(snap);
        return WorldModel.Restore(snap);
    }
}
