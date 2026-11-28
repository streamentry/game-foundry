using System;
using System.Collections.Generic;
using Evergrove.Core;

/// <summary>
/// Smart companion SC02: the ranker and the device-local memory. Deterministic; never the same key
/// within 3 days (unless waiting) or the same wording within 14; one remark per neighbour per visit;
/// anyone-facts to one neighbour; none when nothing qualifies.
/// </summary>
internal static class CompanionRankerTests
{
    private static int assertions;

    private static void Expect(bool ok, string message)
    {
        assertions++;
        if (!ok) throw new Exception("Companion ranker test failed: " + message);
    }

    private static Fact F(FactKind kind, int keeper, string key, int weight, bool waiting = false, bool fresh = false)
    {
        return new Fact(kind, keeper, key, new Dictionary<string, string>(), weight, waiting, fresh, 0);
    }

    public static int Run()
    {
        assertions = 0;
        Weights();
        Repeats();
        Visits();
        Sharing();
        Variants();
        GoldenHash();
        MemoryText();
        ScriptedWeek();
        return assertions;
    }

    private static void Weights()
    {
        const int day = 9000, garden = 1;
        var facts = new List<Fact> {
            F(FactKind.NewestThing, 1, "newest:1:1", 0),
            F(FactKind.KindSolved, 1, "solved:Mosaic:2", CompanionFacts.OwnDoingWeight),
            F(FactKind.LetterWaiting, 1, "letter:Anh:1", CompanionFacts.WaitingWeight, waiting: true),
        };
        Expect(CompanionRanker.Pick(facts, CompanionMemory.Empty, 1, day, garden).Key == "letter:Anh:1", "a waiting letter outranks the rest");
        Expect(CompanionRanker.Pick(facts, null, 1, day, garden).Key == "letter:Anh:1", "no memory is an empty memory");
        Expect(CompanionRanker.Score(facts[2], 1, CompanionMemory.Empty) == 50 + 30 + 10, "waiting + new + own line");
        Expect(CompanionRanker.Score(facts[2], 0, CompanionMemory.Empty) == 50 + 30, "no own-line bonus for another voice");
        CompanionMemory heard = CompanionMemory.Empty.With(new Heard(day - 5, 1, FactKind.KindSolved, -1, "solved:Mosaic:2"));
        Expect(CompanionRanker.Score(facts[1], 1, heard) == 15 + 10, "heard before: no 'new' weight");
        Expect(CompanionRanker.Pick(new List<Fact>(), CompanionMemory.Empty, 0, day, garden) == null, "no facts: none (the story speaks)");
        Expect(CompanionRanker.Pick(facts, CompanionMemory.Empty, 0, day, garden) == null, "only Orin's facts: Mira has none");
        Expect(CompanionRanker.Pick(facts, CompanionMemory.Empty, 7, day, garden) == null, "no such neighbour");

        // Ties break by a hash of (garden, day, key): stable, and they change with the day.
        var ties = new List<Fact>();
        for (int i = 0; i < 6; i++) ties.Add(F(FactKind.KindSolved, 2, "solved:Pairs:" + i, 15));
        var picks = new HashSet<string>();
        for (int d = 0; d < 30; d++)
        {
            Fact a = CompanionRanker.Pick(ties, CompanionMemory.Empty, 2, day + d, garden);
            Fact b = CompanionRanker.Pick(new List<Fact>(ties), CompanionMemory.Empty, 2, day + d, garden);
            Expect(a.Key == b.Key, "deterministic for the same inputs");
            picks.Add(a.Key);
        }
        Expect(picks.Count >= 3, "equal facts take turns across days: " + picks.Count);
        Expect(CompanionRanker.Hash(1, 2, "x") == CompanionRanker.Hash(1, 2, "x") && CompanionRanker.Hash(1, 2, "x") != CompanionRanker.Hash(2, 2, "x"),
            "the hash is stable and depends on the garden");
    }

    private static void Repeats()
    {
        const int garden = 2;
        Fact solved = F(FactKind.KindSolved, 0, "solved:Water:3", 15);
        Fact letter = F(FactKind.LetterWaiting, 1, "letter:Ruth:2", 50, waiting: true);
        var facts = new List<Fact> { solved, letter };
        CompanionMemory memory = CompanionMemory.Empty
            .With(new Heard(100, 0, FactKind.KindSolved, -1, solved.Key))
            .With(new Heard(100, 1, FactKind.LetterWaiting, -1, letter.Key));
        for (int day = 100; day < 103; day++)
            Expect(CompanionRanker.Pick(facts, memory, 0, day, garden) == null, "not again within 3 days (day " + day + ")");
        Expect(CompanionRanker.Pick(facts, memory, 0, 103, garden) == solved, "said again on the fourth day, if still true");
        Expect(CompanionRanker.Pick(facts, memory, 1, 101, garden) == letter, "a letter still waiting may be mentioned again");
        Expect(!CompanionRanker.Eligible(solved, memory, 102) && CompanionRanker.Eligible(solved, memory, 103), "the 3-day window");
        // CP02: a clock set back counts the remark as heard today: blocked for the window, not forever.
        Expect(!CompanionRanker.Eligible(solved, memory, 99), "a clock set back: heard today, so not again yet");
        Expect(CompanionRanker.Eligible(solved, memory, 103), "a clock set back: eligible again once the window passes");
        // SC04: once-facts (a season turned, a pairing found) are said once, then never again.
        Fact grove = new Fact(FactKind.PairingFound, 0, "pairing-found:Grove", new Dictionary<string, string>(), 20, false, false, 0, true);
        Expect(CompanionRanker.Pick(new List<Fact> { grove }, CompanionMemory.Empty, 0, 200, garden) == grove, "a once-fact, never heard: said");
        CompanionMemory heardGrove = CompanionMemory.Empty.With(new Heard(200, 0, FactKind.PairingFound, 0, grove.Key));
        for (int day = 200; day < 260; day += 7)
            Expect(CompanionRanker.Pick(new List<Fact> { grove }, heardGrove, 0, day, garden) == null, "heard once, not again (day " + day + ")");
        Expect(CompanionRanker.Pick(new List<Fact> { grove }, heardGrove, 0, 200, garden, grove.Key) == grove,
            "but it stays their remark for the rest of the visit");
    }

    private static void Visits()
    {
        const int day = 500, garden = 3;
        Fact weather = F(FactKind.WeatherToday, -1, "weather:Rainy", 5);
        Fact newest = F(FactKind.NewestThing, 0, "newest:3:2", 0);
        var facts = new List<Fact> { weather, newest };
        Fact first = CompanionRanker.Pick(facts, CompanionMemory.Empty, 0, day, garden);
        Expect(first != null, "Mira has something to say");
        CompanionMemory memory = CompanionMemory.Empty.With(new Heard(day, 0, first.Kind, -1, first.Key));
        Expect(CompanionRanker.Pick(facts, memory, 0, day, garden, first.Key) == first, "the same remark for the rest of the visit");
        var withFruit = new List<Fact>(facts) { F(FactKind.FruitGiven, 0, "fruit:0:1", 65, fresh: true) };
        Expect(CompanionRanker.Pick(withFruit, memory, 0, day, garden, first.Key).Kind == FactKind.FruitGiven,
            "fruit just handed over is thanked straight away, mid-visit");
        var gone = new List<Fact> { weather };
        if (first != weather)
            Expect(CompanionRanker.Pick(gone, memory, 0, day, garden, first.Key) == null,
                "once said, no second remark this visit, even when the first is no longer true");
        Expect(CompanionRanker.Pick(facts, memory, 0, day, garden) != first, "next visit: something else, or nothing");
    }

    private static void Sharing()
    {
        const int day = 777;
        // Weather is anyone's: only one of three mentions it, and only one with nothing better.
        for (int garden = 0; garden < 40; garden++)
        {
            var facts = new List<Fact> { F(FactKind.WeatherToday, -1, "weather:Snowy", 5), F(FactKind.Hour, -1, "hour:Dusk", 5) };
            Fact[] said = CompanionRanker.Allot(facts, CompanionMemory.Empty, day, garden);
            int weather = 0, hour = 0;
            foreach (Fact f in said)
            {
                if (f == null) continue;
                if (f.Kind == FactKind.WeatherToday) weather++;
                if (f.Kind == FactKind.Hour) hour++;
            }
            Expect(weather == 1 && hour == 1, "each anyone-fact goes to exactly one neighbour (garden " + garden + ")");
            for (int k = 0; k < 3; k++)
                Expect(CompanionRanker.Pick(facts, CompanionMemory.Empty, k, day, garden) == said[k], "Pick agrees with Allot");

            var busy = new List<Fact>(facts) {
                F(FactKind.LetterWaiting, 1, "letter:Anh:1", 50, waiting: true),
                F(FactKind.KindSolved, 0, "solved:Water:1", 15),
            };
            Fact[] busySaid = CompanionRanker.Allot(busy, CompanionMemory.Empty, day, garden);
            Expect(busySaid[1].Kind == FactKind.LetterWaiting && busySaid[0].Kind == FactKind.KindSolved,
                "neighbours with their own news keep it");
            Expect(busySaid[2] != null && (busySaid[2].Kind == FactKind.WeatherToday || busySaid[2].Kind == FactKind.Hour),
                "Fen, with nothing of her own, takes an anyone-fact");
        }
        // A voice with no words for a fact never gets it.
        var only = new List<Fact> { F(FactKind.WeatherToday, -1, "weather:Hot", 5) };
        Fact[] picky = CompanionRanker.Allot(only, CompanionMemory.Empty, day, 1, (f, k) => k == 2);
        Expect(picky[0] == null && picky[1] == null && picky[2] != null, "canSay is respected");
    }

    private static void Variants()
    {
        const int garden = 4;
        CompanionMemory memory = CompanionMemory.Empty;
        var seen = new HashSet<int>();
        // Over 14 days, 20 wordings: never the same twice for the same kind and voice.
        for (int day = 1000; day < 1014; day++)
        {
            int v = CompanionRanker.Variant(FactKind.NewestThing, 0, 20, garden, day, "newest:" + day, memory);
            Expect(v >= 0 && v < 20, "a wording in range");
            Expect(seen.Add(v), "no wording repeated within 14 days: " + v + " on day " + day);
            memory = memory.With(new Heard(day, 0, FactKind.NewestThing, v, "newest:" + day));
        }
        // Another voice keeps its own memory of wordings.
        int fen = CompanionRanker.Variant(FactKind.NewestThing, 2, 20, garden, 1000, "newest:1000", memory);
        Expect(fen == CompanionRanker.Variant(FactKind.NewestThing, 2, 20, garden, 1000, "newest:1000", CompanionMemory.Empty),
            "Mira's wordings don't constrain Fen's");
        // With fewer wordings than days, the least recently heard comes back first.
        CompanionMemory three = CompanionMemory.Empty
            .With(new Heard(10, 1, FactKind.FruitGiven, 0, "a"))
            .With(new Heard(11, 1, FactKind.FruitGiven, 1, "b"))
            .With(new Heard(12, 1, FactKind.FruitGiven, 2, "c"));
        Expect(CompanionRanker.Variant(FactKind.FruitGiven, 1, 3, garden, 13, "d", three) == 0, "all heard: the longest ago");
        Expect(CompanionRanker.Variant(FactKind.FruitGiven, 1, 3, garden, 30, "d", three) >= 0, "after 14 days, any");
        Expect(CompanionRanker.Variant(FactKind.FruitGiven, 1, 0, garden, 13, "d", three) == -1, "no wordings: -1");
    }

    // CP04: golden values for Hash (FNV-1a over UTF-16 code units of "garden|day|key"). Expected values
    // were computed independently in Python from the same algorithm, so a refactor that changes the
    // hash (and so reshuffles every tour) fails here. Null key hashes as "".
    private static void GoldenHash()
    {
        Expect(CompanionRanker.Hash(0, 0, "") == 0x982B0665u, "golden: (0,0,\"\")");
        Expect(CompanionRanker.Hash(1, 9800, "fruit:0:1") == 0x6DB2F08Cu, "golden: (1,9800,fruit:0:1)");
        Expect(CompanionRanker.Hash(-3, 12345, "letter:Anh:1") == 0x11806A24u, "golden: (-3,12345,letter:Anh:1)");
        Expect(CompanionRanker.Hash(7, 1, null) == 0x23955933u, "golden: (7,1,null)");
        // "Gärten" written with an escape so the source encoding can't change the input.
        Expect(CompanionRanker.Hash(2, 500, "pairing-found:Gärten") == 0x95D39275u, "golden: non-ASCII key");
        Expect(CompanionRanker.Hash(4, 77, new string('q', 200)) == 0x0DC9388Du, "golden: 200-char key");
        // Variant with an empty memory is Hash % count: 7 wordings, start at 4.
        Expect(CompanionRanker.Variant(FactKind.FruitGiven, 0, 7, 2, 9800, "fruit:0:1", CompanionMemory.Empty) == 4,
            "golden: Variant with empty memory starts at hash % count");
    }

    private static void MemoryText()
    {
        CompanionMemory m = CompanionMemory.Empty
            .With(new Heard(200, 0, FactKind.Hour, 3, "hour:Dusk"))
            .With(new Heard(201, 1, FactKind.LetterWaiting, -1, "letter:A|B\nC:1"));
        CompanionMemory back = CompanionMemory.Parse(m.Serialize());
        Expect(back.Count == 2 && back.Serialize() == m.Serialize(), "memory round-trips through PlayerPrefs text");
        Expect(back.EverSaid("letter:A|B\nC:1") && back.SaidWithin("letter:A|B\nC:1", 202, 3), "separators in a key are made safe");
        Expect(CompanionMemory.Parse(null).Count == 0 && CompanionMemory.Parse("").Count == 0, "nothing stored: empty");
        CompanionMemory junk = CompanionMemory.Parse("garbage\n1|9|Hour|0|k\n1|0|NotAKind|0|k\n1|0|Hour|x|k\n1|0|Hour|0|\n5|2|Hour|1|ok");
        Expect(junk.Count == 1 && junk.EverSaid("ok"), "unreadable lines are skipped, not thrown on");
        CompanionMemory old = CompanionMemory.Empty.With(new Heard(0, 0, FactKind.Hour, 0, "old"));
        old = old.With(new Heard(CompanionMemory.KeepDays, 0, FactKind.Hour, 0, "new"));
        Expect(!old.EverSaid("old") && old.EverSaid("new"), "remarks older than 60 days are let go");
        CompanionMemory many = CompanionMemory.Empty;
        for (int i = 0; i < CompanionMemory.MaxEntries + 25; i++) many = many.With(new Heard(300, i % 3, FactKind.Hour, 0, "k" + i));
        Expect(many.Count == CompanionMemory.MaxEntries && !many.EverSaid("k0") && many.EverSaid("k" + (CompanionMemory.MaxEntries + 24)),
            "capped, oldest let go first");
        CompanionMemory before = CompanionMemory.Empty;
        CompanionMemory after = before.With(new Heard(1, 0, FactKind.Hour, 0, "x"));
        Expect(before.Count == 0 && after.Count == 1, "With returns a copy; the original is unchanged");
    }

    // A scripted week with a real garden: one visit a day, each neighbour spoken to once.
    private static void ScriptedWeek()
    {
        GardenMap map = GardenMap.River(10, 15, CompanionFactsTests.Mask(), 2);
        WorldModel world = CompanionFactsTests.Grown(9);
        world.SolveTree(0);
        world.SolveTree(1);
        world.KeepNote("Anh", "The plums are early this year.");
        CompanionMemory memory = CompanionMemory.Empty;
        var start = new DateTime(2026, 12, 1);
        var keysByDay = new List<string>[7];
        Weather[] weathers = { Weather.Rainy, Weather.Fair, Weather.Windy, Weather.Fair, Weather.Snowy, Weather.Fair, Weather.Hot };
        for (int d = 0; d < 7; d++)
        {
            keysByDay[d] = new List<string>();
            var context = new CompanionContext(start.AddDays(d), lettersRead: d >= 3 ? 1 : 0, weather: weathers[d],
                lightOn: true, phase: d % 2 == 0 ? DayPhase.Golden : DayPhase.Day, map: map);
            int day = context.DayNumber;
            for (int k = 0; k < 3; k++)
            {
                IReadOnlyList<Fact> facts = CompanionFacts.For(world, context);
                Fact said = CompanionRanker.Pick(facts, memory, k, day, world.Garden,
                    canSay: (f, who) => CompanionSpeech.Legacy(f, who) != null);
                if (said == null) continue;
                Expect(CompanionSpeech.Legacy(said, k) != null, "what's picked has words");
                Expect(!keysByDay[d].Contains(said.Key), "two neighbours never say the same fact on one visit: " + said.Key);
                for (int back = 1; back <= 2 && d - back >= 0; back++)
                    if (!said.Waiting) Expect(!keysByDay[d - back].Contains(said.Key), "no repeat within 3 days: " + said.Key);
                keysByDay[d].Add(said.Key);
                memory = memory.With(new Heard(day, k, said.Kind, -1, said.Key));
            }
            if (d == 0) Expect(keysByDay[0].Contains("letter:Anh:1"), "the waiting letter is mentioned on the first day");
            if (d == 3) Expect(!keysByDay[3].Contains("letter:Anh:1"), "a read letter is not");
            if (d == 1) world.SolveTree(2); // something new happens mid-week
        }
        int said7 = 0;
        foreach (var day in keysByDay) said7 += day.Count;
        Expect(said7 >= 10, "a week of visits has plenty to say: " + said7);
        Console.WriteLine("Companion ranker SC02: a scripted week said " + said7 + " remarks, none repeated within 3 days.");
    }
}
