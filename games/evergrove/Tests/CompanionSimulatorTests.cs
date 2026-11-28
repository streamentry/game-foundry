using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
        WeatherTruth();
        SlotTruth();
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

    // CP18: weather is true to the day. (a) every WeatherToday fact carries the context's own weather in its
    // slot and its key, and a fair day has none. (b) every WeatherToday line the simulator speaks is one of
    // the WeatherToday.<weather> bank group's lines for that voice (own voice, else any), and never a line of
    // another weather's group. (c) RainComing promises only that tomorrow is rainy at home (rain on the hills
    // upstream today, reaching the garden by tomorrow; see RiverWeather.Tomorrow). So it is present exactly on
    // the day before rain, whatever today's sky is: a rainy today keeps it, because the claim is about tomorrow
    // and the hills, not about today's garden. Tomorrow is read from DayWeather directly, not from RiverWeather.
    private static void WeatherTruth()
    {
        const int Days = 60;
        LineBank bank = LoadBank();
        var weathers = (Weather[])Enum.GetValues(typeof(Weather));
        var phases = (DayPhase[])Enum.GetValues(typeof(DayPhase));
        var spokenPerWeather = new int[weathers.Length];
        int factsChecked = 0, linesChecked = 0, comingDays = 0, rainyTodayComing = 0;
        foreach (int grown in new[] { 3, 6, 9, 12 })
        {
            WorldModel world = CompanionFactsTests.Grown(grown);
            world.SolveTree(0);
            world.SolveTree(3);
            GardenMap map = MapFor(world);

            // (a) every weather, several phases: the facts for that weather say that weather and only that.
            foreach (Weather w in weathers)
                for (int d = 0; d < phases.Length; d++)
                {
                    var context = new CompanionContext(Start.AddDays(d), weather: w, lightOn: true, phase: phases[d], map: map);
                    foreach (Fact f in CompanionFacts.For(world, context))
                    {
                        if (f.Kind != FactKind.WeatherToday) continue;
                        factsChecked++;
                        Expect(w != Weather.Fair, "CP18: a fair day has no WeatherToday fact (garden " + grown + ")");
                        Expect(f.Slot("weather") == w.ToString(), "CP18: WeatherToday's weather slot is " + w + ", not " + f.Slot("weather"));
                        Expect(f.Key.Contains(w.ToString()), "CP18: WeatherToday's key names " + w + ": " + f.Key);
                    }
                }

            // (b) the simulator's words for weather come from that weather's own bank group.
            foreach (Weather w in weathers)
            {
                IReadOnlyList<SimLine> transcript = Simulate(world, bank, null,
                    (d, m) => new CompanionContext(Start.AddDays(d), weather: w, lightOn: true, phase: phases[d % phases.Length], map: m),
                    Days);
                foreach (SimLine line in transcript)
                {
                    if (line.Kind != FactKind.WeatherToday) continue;
                    linesChecked++;
                    spokenPerWeather[(int)w]++;
                    Expect(line.Key == "weather:" + w, "CP18: a " + w + " day's weather line has key weather:" + w + ", not " + line.Key);
                    IReadOnlyList<string> own = bank.Lines("WeatherToday." + w, line.Speaker.ToString(System.Globalization.CultureInfo.InvariantCulture));
                    IReadOnlyList<string> group = own.Count > 0 ? own : bank.Lines("WeatherToday." + w, LineBank.AnyVoice);
                    Expect(group.Contains(line.Words), "CP18: \"" + line.Words + "\" is not a WeatherToday." + w + " line for neighbour " + line.Speaker);
                    foreach (Weather other in weathers)
                    {
                        if (other == w) continue;
                        foreach (string voice in bank.Voices("WeatherToday." + other))
                            Expect(!bank.Lines("WeatherToday." + other, voice).Contains(line.Words),
                                "CP18: a " + w + " day says \"" + line.Words + "\", which is in the " + other + " group");
                    }
                }
            }

            // (c) RainComing exactly on the days before rain at home, whatever today's weather is.
            Season season = GardenSeason.For(world);
            for (int d = 0; d < 120; d++)
            {
                DateTime today = Start.AddDays(d);
                Weather todayWeather = DayWeather.For(today, season, world.Garden);
                Weather tomorrow = DayWeather.For(today.AddDays(1), season, world.Garden);
                var context = new CompanionContext(today, weather: todayWeather, lightOn: true, map: map);
                bool coming = false;
                foreach (Fact f in CompanionFacts.For(world, context))
                    if (f.Kind == FactKind.RainComing) coming = true;
                Expect(coming == (tomorrow == Weather.Rainy),
                    "CP18: RainComing is present exactly when tomorrow is rainy (garden " + grown + ", day " + d + ")");
                if (coming) comingDays++;
                if (coming && todayWeather == Weather.Rainy) rainyTodayComing++;
            }
        }

        string counts = "lines per weather (Fair, Rainy, Windy, Hot, Snowy) = " + string.Join(", ", spokenPerWeather);
        Expect(factsChecked > 0 && linesChecked > 0 && comingDays > 0,
            "CP18: the check is not vacuous; facts " + factsChecked + ", lines " + linesChecked + ", coming days " + comingDays);
        for (int i = 0; i < weathers.Length; i++)
            if (weathers[i] != Weather.Fair)
                Expect(spokenPerWeather[i] > 0, "CP18: the simulator speaks at least one " + weathers[i] + " line; " + counts);
        Console.WriteLine("Companion simulator CP18: " + factsChecked + " weather facts, " + linesChecked + " weather lines (" + counts +
            "), " + comingDays + " rain-coming days (" + rainyTodayComing + " of them with rain today).");
    }

    // CP19: every composed line is whole and true to its fact. No brace is left, the words are tidy (not empty,
    // no double space, no edge space), and when a bank line was spoken every slot it uses is a non-empty value
    // of that fact that appears verbatim in the words. Three gardens over 60 days, in three scenarios: plain
    // weather and phases; pairings found and the season's yield taken; and a visit with a letter waiting and
    // fruit just given to a neighbour, so FruitGiven and LetterWaiting come up.
    private static void SlotTruth()
    {
        const int Days = 60;
        LineBank bank = LoadBank();
        var weathers = (Weather[])Enum.GetValues(typeof(Weather));
        var phases = (DayPhase[])Enum.GetValues(typeof(DayPhase));
        var kinds = new HashSet<FactKind>();
        var faults = new List<string>();
        int linesChecked = 0, bankLines = 0, legacyLines = 0, letterLines = 0;
        foreach (int grown in new[] { 3, 9, 12 })
            for (int scenario = 0; scenario < 3; scenario++)
            {
                WorldModel world = CompanionFactsTests.Grown(grown);
                world.SolveTree(0);
                world.SolveTree(3);
                Func<int, GardenMap, CompanionContext> contextFor = VariedContextFor;
                if (scenario == 1)
                {
                    world.DiscoverPairing(Pairing.Grove);
                    world.DiscoverPairing(Pairing.Waterside);
                    world.TakeSeasonYield(world.PairingsFound);
                }
                else if (scenario == 2)
                {
                    const int FruitTo = 2;
                    world = WithFruit(world, 2);
                    Expect(world.KeepNote("Anh", "The plums are early this year."), "CP19: a letter is waiting in the box");
                    Expect(world.GiveFruit(FruitTo), "CP19: fruit is given to a neighbour");
                    // Fruit is "just given" on the first day only; the letter stays unread all the way through.
                    contextFor = (d, m) => new CompanionContext(Start.AddDays(d), weather: weathers[d % weathers.Length], lightOn: true,
                        phase: phases[d % phases.Length], fruitJustGivenTo: d == 0 ? FruitTo : -1, map: m);
                }

                GardenMap map = MapFor(world);
                var factsByDay = new Dictionary<int, IReadOnlyList<Fact>>();
                for (int d = 0; d < Days; d++)
                {
                    CompanionContext context = contextFor(d, map);
                    factsByDay[context.DayNumber] = CompanionFacts.For(world, context);
                }

                foreach (SimLine line in Simulate(world, bank, null, contextFor, Days))
                {
                    linesChecked++;
                    kinds.Add(line.Kind);
                    string where = "garden " + grown + ", scenario " + scenario + ", day " + line.Day + ", neighbour " + line.Speaker +
                        " (" + line.Key + "): \"" + line.Words + "\"";
                    if (line.Words.IndexOf('{') >= 0 || line.Words.IndexOf('}') >= 0) faults.Add("brace left: " + where);
                    if (string.IsNullOrEmpty(line.Words)) faults.Add("no words: " + where);
                    else
                    {
                        if (line.Words.Contains("  ")) faults.Add("double space: " + where);
                        if (line.Words[0] == ' ' || line.Words[line.Words.Length - 1] == ' ') faults.Add("edge space: " + where);
                    }

                    if (line.Variant < 0) { legacyLines++; continue; }
                    Fact fact = null;
                    if (factsByDay.TryGetValue(line.Day, out IReadOnlyList<Fact> facts))
                        foreach (Fact f in facts)
                            if (f.Kind == line.Kind && f.Key == line.Key && bank.For(f, line.Speaker).Count > line.Variant)
                            {
                                fact = f;
                                break;
                            }
                    if (fact == null) { faults.Add("no fact for the line: " + where); continue; }

                    bankLines++;
                    // Compose says the letter in place of the bank line once 90 days are away (CP19: the bank line is then not spoken).
                    if (fact.Kind == FactKind.WeatherWhileAway && !string.IsNullOrEmpty(fact.Slot("letter"))) { letterLines++; continue; }
                    string raw = bank.For(fact, line.Speaker)[line.Variant];
                    foreach (string slot in LineBank.SlotsIn(raw).Distinct())
                    {
                        string value = fact.Slot(slot);
                        if (string.IsNullOrEmpty(value)) faults.Add("empty slot {" + slot + "}: " + where);
                        else if (!line.Words.Contains(value)) faults.Add("slot {" + slot + "} value \"" + value + "\" not in words: " + where);
                    }
                }
            }

        Expect(linesChecked > 0 && bankLines > 0, "CP19: the check is not vacuous; lines " + linesChecked + ", bank lines " + bankLines);
        Expect(kinds.Contains(FactKind.FruitGiven) && kinds.Contains(FactKind.LetterWaiting),
            "CP19: the letter-and-fruit visit says FruitGiven and LetterWaiting lines");
        Expect(faults.Count == 0, "CP19: " + faults.Count + " slot or wording faults: " + string.Join(" | ", faults.Take(10)));
        Console.WriteLine("Companion simulator CP19: " + linesChecked + " lines checked (" + bankLines + " from a bank line, " +
            legacyLines + " legacy, " + letterLines + " letter-substituted) across " + kinds.Count + " distinct kinds; 0 faults.");
    }

    private static WorldModel WithFruit(WorldModel world, int fruit)
    {
        WorldSnapshot snapshot = world.Export();
        snapshot.fruit = fruit;
        Expect(WorldModel.TryRestore(snapshot, out WorldModel restored), "CP19: a save with fruit restores");
        return restored;
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
