using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
        FeedIsPure();
        LetterNamesStayClean();
        return assertions;
    }

    // CP17: the feed, the ranker and the simulator only read the save. Same input gives the same feed,
    // and the save text (GardenTransfer.Export) and the memory text are unchanged afterwards.
    private const string Stamp = "2026-11-28T00:00:00Z";

    private static void FeedIsPure()
    {
        var rng = new Random(Seed + 17);
        GardenMap map = GardenMap.River(10, 15, CompanionFactsTests.Mask(), 2);
        LineBank bank = LoadBank();
        for (int i = 0; i < 100; i++)
        {
            WorldModel world = RandomWorld(rng, i);
            CompanionContext context = RandomContext(rng, map);
            var days = new CompanionContext[3];
            for (int d = 0; d < days.Length; d++) days[d] = RandomContext(rng, map);

            string saveBefore = GardenTransfer.Export(world, Stamp);
            IReadOnlyList<Fact> first = CompanionFacts.For(world, context);
            IReadOnlyList<Fact> second = CompanionFacts.For(world, context);
            SameFeed(first, second, "case " + i + ": For twice");

            // Memory that already heard some of the feed, so the ranker and simulator read real history.
            CompanionMemory memory = CompanionMemory.Empty;
            for (int f = 0; f < first.Count && f < 3; f++)
                memory = memory.With(new Heard(context.DayNumber - 1, f % 3, first[f].Kind, 0, first[f].Key));
            string memoryBefore = memory.Serialize();

            CompanionRanker.Allot(first, memory, context.DayNumber, world.Garden);
            CompanionSimulator.Run(world, d => days[d], 3, bank, CompanionLocale.English, memory);

            Expect(GardenTransfer.Export(world, Stamp) == saveBefore, "case " + i + ": the save is unchanged after the feed, ranker and simulator");
            Expect(memory.Serialize() == memoryBefore, "case " + i + ": the memory is unchanged after the ranker and simulator");
            SameFeed(first, CompanionFacts.For(world, context), "case " + i + ": the feed after the ranker and simulator");
        }
    }

    private static void SameFeed(IReadOnlyList<Fact> a, IReadOnlyList<Fact> b, string where)
    {
        Expect(a.Count == b.Count, where + ": same number of facts");
        for (int i = 0; i < a.Count; i++)
        {
            Fact x = a[i], y = b[i];
            Expect(x.Kind == y.Kind && x.Keeper == y.Keeper && x.Key == y.Key && x.Weight == y.Weight &&
                x.Waiting == y.Waiting && x.Fresh == y.Fresh && x.Once == y.Once && x.LegacyOrder == y.LegacyOrder,
                where + ": fact " + i + " matches: " + x);
            Expect(x.Slots.Count == y.Slots.Count, where + ": fact " + i + " has the same slots: " + x);
            foreach (KeyValuePair<string, string> slot in x.Slots)
                Expect(y.Slots.TryGetValue(slot.Key, out string value) && value == slot.Value,
                    where + ": fact " + i + " slot " + slot.Key + " matches: " + x);
        }
    }

    // CP17 gap: a letter sender with '|' or a newline. Names come from the save (letterFrom) and from
    // KeepNote/SendParcel, both through FamilyParcel.ValidName, which allows no '|' and no newline.
    // A forged save must be refused, so no key can carry a separator.
    private static void LetterNamesStayClean()
    {
        var world = WorldModel.New(PlayMode.Living);
        Expect(!world.KeepNote("Anh|Ruth", "A note."), "CP17: a sender name with '|' is refused");
        Expect(!world.KeepNote("Anh\nRuth", "A note."), "CP17: a sender name with a newline is refused");
        Expect(world.Letters.Length == 0, "CP17: refused names leave the letterbox empty");

        Expect(world.KeepNote("Anh", "A note."), "CP17: a plain name is kept");
        WorldSnapshot snapshot = world.Export();
        snapshot.letterFrom[0] = "Anh|Ruth";
        Expect(!WorldModel.TryRestore(snapshot, out WorldModel _), "CP17: a save with '|' in a sender name is refused");
        snapshot.letterFrom[0] = "Anh\nRuth";
        Expect(!WorldModel.TryRestore(snapshot, out WorldModel _), "CP17: a save with a newline in a sender name is refused");

        CompanionContext today = new CompanionContext(new DateTime(2026, 11, 28));
        IReadOnlyList<Fact> feed = CompanionFacts.For(world, today);
        foreach (Fact f in feed)
            Expect(f.Key.IndexOf('|') < 0 && f.Key.IndexOf('\n') < 0, "CP17: key has no separator: " + f.Key);
        Expect(feed.Any(f => f.Kind == FactKind.LetterWaiting && f.Key == "letter:Anh:1"),
            "CP17: a plain sender's letter gives the key letter:Anh:1");
    }

    private static LineBank LoadBank()
    {
        string dir = AppContext.BaseDirectory;
        while (dir != null && !File.Exists(Path.Combine(dir, "Assets/Resources/Evergrove/Lines/en.json")))
            dir = Path.GetDirectoryName(dir);
        if (dir == null) throw new Exception("Companion fuzz test failed: en.json not found");
        return LineBank.Parse(File.ReadAllText(Path.Combine(dir, "Assets/Resources/Evergrove/Lines/en.json")));
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
