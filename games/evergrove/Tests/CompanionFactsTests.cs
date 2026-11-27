using System;
using System.Collections.Generic;
using System.IO;
using Evergrove.Core;

/// <summary>
/// Smart companion SC01: the fact feed reproduces exactly what the neighbours said before it.
/// <see cref="LegacySays"/> is a verbatim port of EvergroveApp.NeighborSays as it stood on main at
/// 0cf0825 (English, living garden; run 3's deeper pairings, visitor remarks, hour habit and welcome-back included), calling the same Core functions. For many constructed saves and
/// contexts, feed → old chain → old sentence must equal it, word for word.
/// </summary>
internal static class CompanionFactsTests
{
    private static int assertions;

    private static void Expect(bool ok, string message)
    {
        assertions++;
        if (!ok) throw new Exception("Companion facts test failed: " + message);
    }

    public static int Run()
    {
        assertions = 0;
        GardenMap map = GardenMap.River(10, 15, Mask(), 2);
        int compared = 0, spoken = 0;
        var kindsSeen = new HashSet<FactKind>();
        foreach (WorldModel world in Saves())
            foreach (CompanionContext context in Contexts(map))
            {
                IReadOnlyList<Fact> facts = CompanionFacts.For(world, context);
                foreach (Fact f in facts) kindsSeen.Add(f.Kind);
                for (int v = 0; v < 3; v++)
                {
                    string before = LegacySays(world, context, v);
                    Fact pick = CompanionSpeech.LegacyPick(facts, v);
                    string after = pick == null ? null : CompanionSpeech.Legacy(pick, v);
                    Expect(before == after, "save " + compared + ", neighbour " + v + ": was \"" + before + "\", feed says \"" + after +
                        "\" (" + pick + ")");
                    compared++;
                    if (before != null) spoken++;
                }
            }
        Console.WriteLine("Companion facts SC01: " + compared + " comparisons with the old NeighborSays, " + spoken + " noticing lines, all identical.");
        Expect(compared > 2000, "a broad set of saves compared: " + compared);
        Expect(spoken > compared / 3 && spoken < compared, "noticing lines and story lines both compared: " + spoken);
        foreach (FactKind kind in Enum.GetValues(typeof(FactKind)))
            Expect(kindsSeen.Contains(kind), "the saves exercise every fact kind: " + kind);

        Truths(map);
        return assertions;
    }

    // ---- Each fact appears only when its condition holds, with slots that match the save.
    private static void Truths(GardenMap map)
    {
        var today = new DateTime(2026, 11, 27);
        WorldModel story = WorldModel.New(PlayMode.PuzzleOnly);
        Expect(CompanionFacts.For(story, new CompanionContext(today, weather: Weather.Rainy, lightOn: true, phase: DayPhase.Night)).Count == 0,
            "a puzzle-only garden has no companion facts");
        WorldModel empty = WorldModel.New(PlayMode.Living);
        foreach (Fact f in CompanionFacts.For(empty, new CompanionContext(today)))
            Expect(f.Kind == FactKind.PairingToFind, "a new garden has only pairing hints: " + f);

        WorldModel world = Grown(6);
        IReadOnlyList<Fact> facts = CompanionFacts.For(world, new CompanionContext(today, map: map));
        Fact newestFen = Find(facts, FactKind.NewestThing, 2);
        Expect(newestFen != null && newestFen.Slot("thing") == GardenCatalog.For(world.Garden, 5, VillageChoice.Flowers).Name,
            "Fen's newest thing is the sixth choice's flower");
        Expect(newestFen.IntSlot("count") == 2, "two flower things grown");
        Expect(Find(facts, FactKind.WeatherToday, -1) == null, "fair weather says nothing");
        Expect(Find(facts, FactKind.Hour, -1) == null, "the light switched off says nothing about the hour");
        Expect(Find(facts, FactKind.LetterWaiting, 1) == null, "no letter, no letter fact");

        Expect(world.KeepNote("Anh", "The plums are early this year."), "a note in the box");
        Fact letter = Find(CompanionFacts.For(world, new CompanionContext(today)), FactKind.LetterWaiting, 1);
        Expect(letter != null && letter.Slot("name") == "Anh" && letter.Waiting && letter.Weight >= CompanionFacts.WaitingWeight,
            "an unread letter is waiting, from Anh");
        Expect(Find(CompanionFacts.For(world, new CompanionContext(today, lettersRead: 1)), FactKind.LetterWaiting, 1) == null,
            "a read letter is not waiting");

        IReadOnlyList<Fact> away = CompanionFacts.For(world, new CompanionContext(today, lastSeen: today.AddDays(-1)));
        Expect(Find(away, FactKind.WeatherWhileAway, 0) == null, "yesterday is not 'a while'");
        Fact gone = Find(CompanionFacts.For(world, new CompanionContext(today, lastSeen: today.AddDays(-5))), FactKind.WeatherWhileAway, 0);
        Expect(gone != null && gone.IntSlot("days") == 5, "five days away, told as five");
        Expect(Find(CompanionFacts.For(world, new CompanionContext(today, lastSeen: today.AddDays(3))), FactKind.WeatherWhileAway, 0) == null,
            "a clock set back claims nothing");

        // SC04: a hope partly come true, with its true count, said once.
        WorldModel tending = Grown(9);
        tending.SolveTree(0);
        tending.SolveTree(3);
        Fact wish = Find(CompanionFacts.For(tending, new CompanionContext(today)), FactKind.WishProgress, 0);
        Expect(wish != null && wish.Slot("have") == "two" && wish.Slot("need") == "five" && wish.Once,
            "Mira's hope: two of five trees tended, said once");
        Expect(Find(CompanionFacts.For(Grown(9), new CompanionContext(today)), FactKind.WishProgress, 0) == null,
            "no trees tended, no progress to tell");
        Expect(Find(CompanionFacts.For(Grown(2), new CompanionContext(today)), FactKind.SeasonTurned, -1) == null,
            "the garden's first season is not a turn");
        WorldModel finished = Solving(GardenCatalog.ChaptersPerGarden, 4);
        Fact turned = Find(CompanionFacts.For(finished, new CompanionContext(today)), FactKind.SeasonTurned, -1);
        Expect(turned != null && turned.Once && turned.Slot("season") == GardenSeason.For(finished).ToString(), "a season turned after the story");
        Expect(Find(CompanionFacts.For(finished, new CompanionContext(today)), FactKind.YieldGiven, 0) == null &&
               Find(CompanionFacts.For(finished, new CompanionContext(today)), FactKind.YieldGiven, -1) == null &&
               Find(CompanionFacts.For(finished, new CompanionContext(today)), FactKind.YieldGiven, 2) == null,
            "no yield taken, none told");
        WorldModel found = Restore(Grown(7), x => x.pairings = PairingMask.Of(Pairing.Grove) | PairingMask.Of(Pairing.Waterside));
        IReadOnlyList<Fact> foundFacts = CompanionFacts.For(found, new CompanionContext(today));
        Expect(Find(foundFacts, FactKind.PairingFound, 0)?.Slot("pairing") == "Grove", "Mira tells of the grove found");
        Expect(Find(foundFacts, FactKind.PairingFound, 2)?.Slot("pairing") == "Waterside", "Fen tells of the waterside flower");
        Expect(Find(foundFacts, FactKind.PairingFound, 1) == null, "Orin's pairings aren't found");
        foreach (Fact f in foundFacts)
            if (f.Kind == FactKind.WishProgress || f.Kind == FactKind.SeasonTurned || f.Kind == FactKind.YieldGiven || f.Kind == FactKind.PairingFound)
                Expect(CompanionSpeech.LegacyPick(new[] { f }, f.Keeper < 0 ? 0 : f.Keeper) == null, "the old chain never says " + f);

        // Keys are stable across calls and unique within a feed.
        var a = CompanionFacts.For(world, new CompanionContext(today, weather: Weather.Windy, lightOn: true, phase: DayPhase.Dusk, map: map));
        var b = CompanionFacts.For(world, new CompanionContext(today, weather: Weather.Windy, lightOn: true, phase: DayPhase.Dusk, map: map));
        Expect(a.Count == b.Count, "the same inputs give the same facts");
        var keys = new HashSet<string>();
        for (int i = 0; i < a.Count; i++)
        {
            Expect(a[i].Key == b[i].Key && a[i].Kind == b[i].Kind, "deterministic: " + a[i]);
            Expect(keys.Add(a[i].Key), "keys are unique in one feed: " + a[i].Key);
        }
        foreach (Fact f in a)
        {
            string line = CompanionSpeech.Legacy(f, f.Keeper < 0 ? 0 : f.Keeper);
            // SC04's new kinds were never said by the old chain: they speak only through the reviewed bank.
            if (line == null && f.Slot(CompanionFacts.LegacyOffSlot) != null && f.LegacyOrder >= 14 * 100000) continue;
            Expect(line != null, "every fact has words in its keeper's voice: " + f);
            Expect(!line.ToLowerInvariant().Contains("missed you") && !line.Contains("streak"), "no guilt: " + line);
        }
    }

    private static Fact Find(IReadOnlyList<Fact> facts, FactKind kind, int keeper)
    {
        foreach (Fact f in facts)
            if (f.Kind == kind && f.Keeper == keeper) return f;
        return null;
    }

    // ---- The saves: empty, growing, solved, completed, fruit, gifts, help, nursery, letters, pairings.
    internal static IEnumerable<WorldModel> Saves()
    {
        yield return WorldModel.New(PlayMode.Living);
        foreach (int grown in new[] { 1, 2, 3, 4, 7, 9, 11 })
        {
            WorldModel w = Grown(grown);
            yield return w;
            yield return Solving(grown, 1);
            yield return Solving(grown, 4);
        }
        // Finished gardens: the river facts depend on solves after the story.
        for (int solves = 0; solves <= 12; solves++) yield return Solving(GardenCatalog.ChaptersPerGarden, solves);
        foreach (int fruitTo in new[] { 0, 1, 2 })
        {
            WorldModel w = Restore(Grown(6), s => s.fruit = 3);
            Expect(w.GiveFruit(fruitTo), "fruit given to " + fruitTo);
            yield return w;
            WorldModel solved = Restore(Solving(9, 3), s => s.fruit = 2);
            Expect(solved.GiveFruit(fruitTo), "fruit given after solves");
            yield return solved;
        }
        foreach (int kind in new[] { 0, 1, 2, 5, 8 })
        {
            if (kind >= Nursery.Kinds) continue;
            WorldModel w = Grown(5);
            Expect(w.ReceiveNeighbourCutting(kind), "a cutting in the basket");
            yield return w;
            WorldModel two = Grown(8);
            Expect(two.ReceiveNeighbourCutting(kind) && two.ReceiveNeighbourCutting((kind + 1) % Nursery.Kinds), "two cuttings");
            yield return two;
        }
        foreach (int item in new[] { 0, 1, 2, 4, 5 })
        {
            yield return Restore(Grown(6), s => { s.helped = new[] { item }; s.helpedFrom = new[] { "Grandpa Tom" }; });
            WorldModel solvedToo = Restore(Grown(6), s => { s.helped = new[] { item }; s.helpedFrom = new[] { "Mai" }; });
            solvedToo.SolveTree(item);
            yield return solvedToo;
        }
        foreach (int kind in new[] { 0, 1, 2, 3, 4 })
        {
            if (kind >= Nursery.Kinds) continue;
            WorldModel buyer = Restore(Solving(6, 2), s => s.seeds = 500);
            if (buyer.Buy(kind, 40)) yield return buyer;
            WorldModel solvedBuy = Restore(Grown(6), s => s.seeds = 500);
            if (solvedBuy.Buy(kind, 41)) { solvedBuy.SolveBought(0); yield return solvedBuy; }
        }
        foreach (int letters in new[] { 1, 2 })
        {
            WorldModel w = Grown(4);
            for (int i = 0; i < letters; i++) Expect(w.KeepNote(i == 0 ? "Anh" : "Ruth", "A note, number " + (i + 1) + "."), "note kept");
            yield return w;
        }
        foreach (int mask in new[] { 1, 2, 4, 8, 5, 15, 31, 63, 127 })
            yield return Restore(Grown(7), s => s.pairings = mask);
        // Every good neighbour found and little grown: some neighbours have nothing true to say.
        yield return Restore(WorldModel.New(PlayMode.Living), s => s.pairings = 127);
        yield return Restore(Grown(1), s => s.pairings = 127);
        yield return Restore(Grown(2), s => s.pairings = 127);
        WorldModel together = Grown(6);
        together.MarkTogether(0);
        yield return together;
        yield return Restore(Solving(12, 5), s => { s.garden = 3; });
        // SC04: a season's yield taken (I388), in each season after the story.
        for (int turn = 0; turn < 4; turn++)
        {
            WorldModel season = Restore(Solving(GardenCatalog.ChaptersPerGarden, turn * GardenMap.SolvesPerReach), s => { });
            int key = SeasonalYields.Key(season);
            yield return Restore(season, s => s.yields = key + 1);
        }
    }

    internal static IEnumerable<CompanionContext> Contexts(GardenMap map)
    {
        var today = new DateTime(2026, 11, 27);
        yield return new CompanionContext(today);
        yield return new CompanionContext(today, map: map);
        foreach (Weather weather in new[] { Weather.Rainy, Weather.Windy, Weather.Hot, Weather.Snowy })
            yield return new CompanionContext(today, weather: weather, map: map);
        foreach (DayPhase phase in Enum.GetValues(typeof(DayPhase)))
            yield return new CompanionContext(today, lightOn: true, phase: phase, map: map);
        yield return new CompanionContext(today, lastSeen: today.AddDays(-4), map: map);
        yield return new CompanionContext(today, lastSeen: today.AddDays(-40), weather: Weather.Rainy);
        yield return new CompanionContext(today, lettersRead: 1, map: map);
        yield return new CompanionContext(today, fruitJustGivenTo: 2, weather: Weather.Hot, map: map);
        yield return new CompanionContext(today, fruitJustGivenTo: 0, lightOn: true, phase: DayPhase.Golden);
        yield return new CompanionContext(today, togetherKeeper: 1, togetherKind: PuzzleKind.Mosaic, lastSeen: today.AddDays(-3));
        yield return new CompanionContext(today, togetherKeeper: 0, togetherKind: PuzzleKind.StreamPath, weather: Weather.Snowy,
            lightOn: true, phase: DayPhase.Night, map: map);
        yield return new CompanionContext(today, togetherKeeper: 2, togetherKind: PuzzleKind.Doorstep, lettersRead: 5);
        // I377's "one day in three" falls on different days for each keeper.
        yield return new CompanionContext(today.AddDays(1), map: map);
        // I419: a clear habit at each hour, over a week of days so each noticer's day comes round;
        // I420: back after a week and after a month.
        foreach (DayPhase phase in Enum.GetValues(typeof(DayPhase)))
        {
            int[] counts = new int[Companion.PhaseCount];
            counts[(int)phase] = 9;
            counts[((int)phase + 1) % Companion.PhaseCount] = 1;
            for (int d = 0; d < 7; d++) yield return new CompanionContext(today.AddDays(d), hourCounts: counts, map: map);
        }
        yield return new CompanionContext(today, hourCounts: new[] { 3, 3, 3, 0, 0, 0 }, map: map);
        yield return new CompanionContext(today, lastSeen: today.AddDays(-8), map: map);
        yield return new CompanionContext(today, lastSeen: today.AddDays(-31), weather: Weather.Windy);
        yield return new CompanionContext(today.AddDays(2), lightOn: true, phase: DayPhase.Dusk, map: map);
    }

    // ---- Today's NeighborSays (EvergroveApp at 0dcabc9), English and living; null = the story line.
    private static string LegacySays(WorldModel world, CompanionContext c, int villager)
    {
        if (world.Mode != PlayMode.Living) return null;
        string thanks = c.FruitJustGivenTo == villager ? NeighbourNotice.ThanksFor(villager, world) : null;
        if (thanks != null) return thanks;
        if (villager == c.TogetherKeeper) return NeighbourNotice.TogetherLine(c.TogetherKind);
        string sinceLine = null;
        if (c.LastSeen != null && world.Choices.Length > 0)
        {
            sinceLine = DayWeather.SinceLastVisit(c.LastSeen.Value, c.Today, GardenSeason.For(world), world.Garden);
            string welcome = Companion.WelcomeBack((int)(c.Today - c.LastSeen.Value.Date).TotalDays);
            if (welcome != null) sinceLine = welcome + (string.IsNullOrEmpty(sinceLine) ? "" : " " + sinceLine);
        }
        if (villager == 0 && sinceLine != null) return sinceLine;
        string[] mail = world.Letters;
        if (villager == 1 && mail.Length > 0 && c.LettersRead < mail.Length)
            return "There's a letter in your box from " + world.LetterFrom[mail.Length - 1] + ". I saw the flag go up this morning.";
        string noticed = NeighbourNotice.Line(villager, world);
        if (noticed != null) return noticed;
        string weatherLine = DayWeather.Remark(c.Weather, villager);
        if (weatherLine != null) return weatherLine;
        Pairing[] theirs = villager == 0 ? new[] { Pairing.Grove, Pairing.Orchard }
            : villager == 1 ? new[] { Pairing.ByTheWater, Pairing.ShadySeat }
            : new[] { Pairing.Sheltered, Pairing.FlowerBed, Pairing.Waterside };
        foreach (Pairing p in theirs)
            if (!world.HasPairing(p) && (Pairings.Open(world) & PairingMask.Of(p)) != 0) return OldPairingHint(p);
        string visitorLine = Visitors.KeeperRemark(world, villager, (int)(c.Today - new DateTime(2000, 1, 1)).TotalDays);
        if (visitorLine != null) return visitorLine;
        int[] hours = c.HourCounts == null ? new int[Companion.PhaseCount] : new List<int>(c.HourCounts).ToArray();
        string habitLine = Companion.HabitLine(hours, villager, (int)(c.Today - new DateTime(2000, 1, 1)).TotalDays);
        if (habitLine != null) return habitLine;
        string hourLine = c.LightOn ? DayLight.Remark(villager, c.Phase, GardenSeason.For(world)) : null;
        if (hourLine != null) return hourLine;
        int[] made = world.Choices;
        int newest = -1, count = 0;
        for (int i = 0; i < made.Length; i++)
            if (made[i] == villager) { newest = i; count++; }
        if (newest < 0) return null;
        string thing = GardenCatalog.For(world.Garden, Math.Max(0, Math.Min(GardenCatalog.ChaptersPerGarden - 1, newest)),
            (VillageChoice)villager).Name;
        string where = "";
        GardenMap map = c.Map;
        if (map != null)
        {
            int cell = map.Layout(made, world.Placements)[newest];
            if (cell >= 0)
            {
                int row = cell / map.Columns, column = cell % map.Columns;
                where = map.DistanceToWater(cell) <= 1 ? " by the creek"
                    : row <= 3 && column >= 5 ? " by the cottage"
                    : row >= map.Rows - 4 ? " down the valley" : " in the meadow";
            }
        }
        string more = count >= 3 ? " That makes " + count + " now." : "";
        switch (villager)
        {
            case 0: return "Your " + thing + where + " is taking root nicely." + more;
            case 1: return "I walked past your " + thing + where + " this morning. Good, sturdy work." + more;
            default: return "The bees found your " + thing + where + " before I did." + more;
        }
    }

    private static string OldPairingHint(Pairing pairing)
    {
        switch (pairing)
        {
            case Pairing.Grove: return "Two trees side by side keep each other company. A grove, my grandmother called it.";
            case Pairing.ByTheWater: return "Things built beside the water last. The river keeps an eye on them.";
            case Pairing.Sheltered: return "Flowers love a little shade. Try one beside a tree.";
            case Pairing.Orchard: return "Three trees together and it's an orchard. They'd crop better for the company.";
            case Pairing.ShadySeat: return "A seat's no good in full sun. Build something beside a tree and you'll see.";
            case Pairing.Waterside: return "Some flowers like their feet wet. Try one right by the water.";
            default: return "Two different flowers side by side make a bed. They show each other off.";
        }
    }

    // ---- Helpers.
    internal static WorldModel Grown(int count)
    {
        WorldModel world = WorldModel.New(PlayMode.Living);
        for (int chapter = 0; chapter < count; chapter++)
        {
            Puzzle board = world.CurrentPuzzle;
            for (int i = 0; i < board.CellCount; i++)
                while (world.Rotations[i] != board.Solution[i]) world.Rotate(i);
            world.Choose((VillageChoice)(chapter % 3));
        }
        return world;
    }

    private static WorldModel Solving(int grown, int solves)
    {
        WorldModel world = Grown(grown);
        for (int i = 0; i < Math.Min(solves, grown); i++) world.SolveTree(i);
        return world;
    }

    private static WorldModel Restore(WorldModel world, Action<WorldSnapshot> change)
    {
        WorldSnapshot snapshot = world.Export();
        change(snapshot);
        Expect(WorldModel.TryRestore(snapshot, out WorldModel restored), "constructed save restores");
        return restored;
    }

    internal static string Mask()
    {
        string dir = AppContext.BaseDirectory;
        while (dir != null && !File.Exists(Path.Combine(dir, "Assets/Resources/Evergrove/garden-map-mask.txt")))
            dir = Path.GetDirectoryName(dir);
        if (dir == null) throw new Exception("garden-map-mask.txt not found");
        return File.ReadAllText(Path.Combine(dir, "Assets/Resources/Evergrove/garden-map-mask.txt")).Trim();
    }
}
