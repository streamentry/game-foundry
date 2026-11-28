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
        NoRepeatedWording();
        OnceIsOnce();
        NoStarvation();
        FairSharing();
        return assertions;
    }

    // CP13: the app gives the context the garden's painted map, so {where} fills and the bank speaks.
    private static GardenMap MapFor(WorldModel world)
    {
        return GardenMap.River(10, 15, CompanionFactsTests.Mask(),
            GardenMap.ReachesFor(world.Choices.Length, world.Completed, world.PuzzlesSolved));
    }

    private static CompanionContext ContextFor(int day, GardenMap map)
    {
        return new CompanionContext(Start.AddDays(day), weather: Cycle[day % Cycle.Length], lightOn: true, phase: DayPhase.Day, map: map);
    }

    private static LineBank LoadBank()
    {
        return LineBank.Parse(File.ReadAllText(Path.Combine(Root(), "games/evergrove/Assets/Resources/Evergrove/Lines/en.json")));
    }

    private static IReadOnlyList<SimLine> Simulate(WorldModel world, LineBank bank, CompanionMemory memory = null,
        Func<int, GardenMap, CompanionContext> contextFor = null, int days = 7)
    {
        GardenMap map = MapFor(world);
        return CompanionSimulator.Run(world, day => (contextFor ?? ContextFor)(day, map), days, bank, CompanionLocale.English, memory);
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
        Expect(CompanionSimulator.Run(world, day => ContextFor(day, MapFor(world)), 0, bank).Count == 0, "zero days is an empty transcript");

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
        string factsBefore = KeysOf(CompanionFacts.For(world, ContextFor(0, MapFor(world))));
        Simulate(world, bank);
        Expect(KeysOf(CompanionFacts.For(world, ContextFor(0, MapFor(world)))) == factsBefore, "the world is not mutated");
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

    // CP12: every weather and every phase in turn, so the qualifier groups (WeatherToday.*, Hour.*) all come up.
    private static CompanionContext VariedContextFor(int day, GardenMap map)
    {
        var weathers = (Weather[])Enum.GetValues(typeof(Weather));
        var phases = (DayPhase[])Enum.GetValues(typeof(DayPhase));
        return new CompanionContext(Start.AddDays(day), weather: weathers[day % weathers.Length], lightOn: true,
            phase: phases[day % phases.Length], map: map);
    }

    private static void NoRepeatedWording()
    {
        const int Days = 30;
        LineBank bank = LoadBank();
        int checkedLines = 0;
        var repeats = new List<string>();
        foreach (int grown in new[] { 3, 6, 9, 12 })
        {
            WorldModel world = CompanionFactsTests.Grown(grown);
            world.SolveTree(0);
            world.SolveTree(3);
            IReadOnlyList<SimLine> transcript = Simulate(world, bank, null, VariedContextFor, Days);
            checkedLines += transcript.Count;
            for (int speaker = 0; speaker < 3; speaker++)
            {
                var seen = new List<SimLine>();
                foreach (SimLine line in transcript)
                {
                    if (line.Speaker != speaker) continue;
                    foreach (SimLine earlier in seen)
                    {
                        if (earlier.Words != line.Words) continue;
                        int gap = line.Day - earlier.Day;
                        if (gap < CompanionRanker.VariantDays)
                            repeats.Add("garden " + grown + ", speaker " + speaker + ": \"" + line.Words + "\" repeated after " + gap +
                                " day(s) (" + earlier.Key + " on day " + earlier.Day + ", " + line.Key + " on day " + line.Day +
                                "; " + CompanionRanker.VariantDays + " days required)");
                    }
                    seen.Add(line);
                }
            }
        }
        Expect(checkedLines > 0, "the varied simulation says something to check");
        Expect(repeats.Count == 0, "no-repeat check over " + checkedLines + " lines: " + string.Join(" | ", repeats));
        Console.WriteLine("Companion simulator CP12: " + checkedLines + " transcript lines over " + Days + " days, none repeated within " +
            CompanionRanker.VariantDays + " days per speaker.");
    }

    // CP13: news said once (SC04 Once facts) is said once across 120 simulated days, one date per day.
    // A season turn comes from the save (the story's landscape, or the turns after it), not the date, so
    // each garden's once-facts are fixed; the memory must still keep each to a single telling.
    private static void OnceIsOnce()
    {
        const int Days = 120;
        LineBank bank = LoadBank();
        int onceSaid = 0, checkedLines = 0;
        var repeats = new List<string>();
        foreach (int grown in new[] { 3, 9, 12 })
        {
            WorldModel world = CompanionFactsTests.Grown(grown);
            world.SolveTree(0);
            world.SolveTree(3);
            // Good-neighbour pairings found (the first four are open from the start) and the season's yield taken.
            world.DiscoverPairing(Pairing.Grove);
            world.DiscoverPairing(Pairing.Waterside);
            world.TakeSeasonYield(world.PairingsFound);
            GardenMap map = MapFor(world);

            var onceKeys = new HashSet<string>();
            for (int day = 0; day < Days; day++)
                foreach (Fact f in CompanionFacts.For(world, ContextFor(day, map)))
                    if (f.Once) onceKeys.Add(f.Key);

            IReadOnlyList<SimLine> transcript = Simulate(world, bank, null, ContextFor, Days);
            checkedLines += transcript.Count;
            var sayings = new Dictionary<string, int>();
            foreach (SimLine line in transcript)
            {
                if (!onceKeys.Contains(line.Key)) continue;
                onceSaid++;
                sayings.TryGetValue(line.Key, out int before);
                sayings[line.Key] = before + 1;
            }
            foreach (KeyValuePair<string, int> said in sayings)
                if (said.Value > 1)
                    repeats.Add("garden " + grown + ": once-fact \"" + said.Key + "\" said " + said.Value + " times");
        }
        Expect(onceSaid > 0, "a once-fact was said, so the check is not vacuous");
        Expect(repeats.Count == 0, "no once-fact said twice: " + string.Join(" | ", repeats));
        Console.WriteLine("Companion simulator CP13: " + onceSaid + " once-fact lines in " + checkedLines + " lines over " + Days +
            " days; none said twice.");
    }

    // CP14: a neighbour who has something true and sayable on a day gets a line that day on at least 80% of
    // those days, per garden and per neighbour. "Sayable" is independent of memory: a fact meant for the
    // neighbour (or shared) that the bank or the legacy text can voice.
    private static void NoStarvation()
    {
        const int Days = 30;
        const double Threshold = 0.8;
        LineBank bank = LoadBank();
        var summary = new List<string>();
        var failures = new List<string>();
        foreach (int grown in new[] { 6, 9, 12 })
        {
            WorldModel world = CompanionFactsTests.Grown(grown);
            world.SolveTree(0);
            world.SolveTree(3);
            GardenMap map = MapFor(world);
            IReadOnlyList<SimLine> transcript = Simulate(world, bank, null, VariedContextFor, Days);
            var spokeOn = new HashSet<string>();
            foreach (SimLine line in transcript) spokeOn.Add(line.Day + "/" + line.Speaker);

            for (int speaker = 0; speaker < 3; speaker++)
            {
                int sayableDays = 0, spokeDays = 0;
                for (int d = 0; d < Days; d++)
                {
                    CompanionContext context = VariedContextFor(d, map);
                    if (!HadSomethingSayable(CompanionFacts.For(world, context), bank, speaker)) continue;
                    sayableDays++;
                    if (spokeOn.Contains(context.DayNumber + "/" + speaker)) spokeDays++;
                }
                double percent = sayableDays == 0 ? 0 : 100.0 * spokeDays / sayableDays;
                string cell = "garden " + grown + ", neighbour " + speaker + ": " + spokeDays + "/" + sayableDays + " = " +
                    percent.ToString("F0", System.Globalization.CultureInfo.InvariantCulture) + "%";
                summary.Add(cell);
                Expect(sayableDays > 0, "CP14: the check is not vacuous (" + cell + ")");
                if (sayableDays > 0 && spokeDays < Threshold * sayableDays) failures.Add(cell);
            }
        }
        Expect(failures.Count == 0, "CP14: a neighbour with something to say speaks on at least 80% of those days; below: " +
            string.Join(" | ", failures) + ". All: " + string.Join(" | ", summary));
        Console.WriteLine("Companion simulator CP14: " + string.Join("; ", summary) + ".");
    }

    // CP15: over 30 days, anyone-facts (Keeper == Fact.AnyKeeper: weather, hour, river, visitors, rain coming)
    // are spread across the three neighbours. Pooled over three gardens, each neighbour gets at least 20%.
    // CP15: who gets the stage. By the spec an anyone-fact goes to a neighbour with nothing better of their own,
    // so a neighbour with plenty of their own news (often Mira) rightly gets few shared remarks. Fairness is
    // therefore measured on all remarks: each neighbour has at least 20% of what is said, and shared remarks
    // don't all go to one neighbour.
    private static void FairSharing()
    {
        const int Days = 30;
        const double MinShare = 0.2;
        LineBank bank = LoadBank();
        var all = new int[3];
        var shared = new int[3];
        int total = 0, sharedTotal = 0;
        foreach (int grown in new[] { 6, 9, 12 })
        {
            WorldModel world = CompanionFactsTests.Grown(grown);
            world.SolveTree(0);
            world.SolveTree(3);
            GardenMap map = MapFor(world);

            // The anyone keys of each simulated day, looked up from the same facts the simulator ranks.
            var anyoneByDay = new Dictionary<int, HashSet<string>>();
            for (int d = 0; d < Days; d++)
            {
                CompanionContext context = VariedContextFor(d, map);
                var keys = new HashSet<string>();
                foreach (Fact f in CompanionFacts.For(world, context))
                    if (f.Keeper == Fact.AnyKeeper) keys.Add(f.Key);
                anyoneByDay[context.DayNumber] = keys;
            }

            foreach (SimLine line in Simulate(world, bank, null, VariedContextFor, Days))
            {
                all[line.Speaker]++;
                total++;
                if (!anyoneByDay.TryGetValue(line.Day, out HashSet<string> keys) || !keys.Contains(line.Key)) continue;
                shared[line.Speaker]++;
                sharedTotal++;
            }
        }

        string counts = "all lines (Mira, Orin, Fen) = " + all[0] + ", " + all[1] + ", " + all[2] + " of " + total +
            "; shared lines = " + shared[0] + ", " + shared[1] + ", " + shared[2] + " of " + sharedTotal;
        Expect(total > 0 && sharedTotal > 0, "CP15: the check is not vacuous; " + counts);
        for (int speaker = 0; speaker < 3; speaker++)
            Expect((double)all[speaker] / total >= MinShare, "CP15: neighbour " + speaker + " has at least 20% of all remarks; " + counts);
        int receivers = 0;
        for (int speaker = 0; speaker < 3; speaker++) if (shared[speaker] > 0) receivers++;
        Expect(receivers >= 2, "CP15: shared remarks go to more than one neighbour; " + counts);
        Console.WriteLine("Companion simulator CP15: " + counts + ".");
    }

    private static bool HadSomethingSayable(IReadOnlyList<Fact> facts, LineBank bank, int speaker)
    {
        foreach (Fact f in facts)
            if (f.For(speaker) && CompanionLocale.CanSay(CompanionLocale.English, bank, f, speaker)) return true;
        return false;
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
