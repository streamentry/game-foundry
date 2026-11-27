using System;
using System.IO;
using System.Text.RegularExpressions;
using Evergrove.Core;

// Checklists 49 and 54: night creatures on every open stretch (evening only), and a morning and an
// evening (a dew board in the morning, a lantern letter at dusk, at most one a day). Pure rules are
// tested here; the Unity side (tokens, placement, tour shots) is checked by source audit.
internal static class DayRhythmTests
{
    private static int assertions;

    private static void Expect(bool ok, string message)
    {
        assertions++;
        if (!ok) throw new Exception("DayRhythmTests failed: " + message);
    }

    private static readonly Regex LossWords = new Regex(
        @"\b(died|dead|dying|die|wilt\w*|withered|lost|lose|loss|missed|miss|gone|broke|broken|weeds?|overgrown|decay\w*|too late|sorry|neglect\w*|abandon\w*|ruin\w*|forgot\w*|dry|dried|guilt\w*|waiting for you|wasted?|empty|fail\w*|left behind|away|hurry|hurry up|late)\b",
        RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

    private static readonly DayPhase[] AllPhases = { DayPhase.Dawn, DayPhase.Morning, DayPhase.Day, DayPhase.Golden, DayPhase.Dusk, DayPhase.Night };

    public static int Run()
    {
        assertions = 0;
        EveryStretchHasANightCreature();
        NoNightCreatureOutsideTheEvening();
        NightCreaturesAreTheNewKinds();
        NightSpotsSitMidStretch();
        OfferIsDewInTheMorningOnly();
        OfferIsALanternAtDuskOnly();
        OfferIsDeterministic();
        AtMostOneLanternLetterAday();
        CopyPassesWordingRules();
        TourSavesTheShots();
        KindsAppendedAtTheEnd();
        RegisteredInTheSuite();
        return assertions;
    }

    private const int StretchesChecked = 201; // reaches 0..200

    private static void EveryStretchHasANightCreature()
    {
        for (int reach = 0; reach < StretchesChecked; reach++)
        {
            Expect(NightCreatures.For(reach, DayPhase.Dusk) != null, "dusk has a night creature on stretch " + reach);
            Expect(NightCreatures.For(reach, DayPhase.Night) != null, "night has a night creature on stretch " + reach);
        }
    }

    private static void NoNightCreatureOutsideTheEvening()
    {
        for (int reach = 0; reach < StretchesChecked; reach++)
            foreach (DayPhase phase in AllPhases)
                if (phase != DayPhase.Dusk && phase != DayPhase.Night)
                    Expect(NightCreatures.For(reach, phase) == null, "no night creature in " + phase + " on stretch " + reach);
        Expect(NightCreatures.For(-1, DayPhase.Night) == null, "no stretch below home");
        Expect(!NightCreatures.Dark(DayPhase.Morning) && !NightCreatures.Dark(DayPhase.Day) && !NightCreatures.Dark(DayPhase.Golden),
            "morning, day and golden hour are not the evening");
    }

    private static void NightCreaturesAreTheNewKinds()
    {
        Expect(NightCreatures.For(0, DayPhase.Dusk) == Visitor.Moths, "even stretches have moths");
        Expect(NightCreatures.For(1, DayPhase.Dusk) == Visitor.Frogs, "odd stretches have frogs");
        Expect(NightCreatures.For(2, DayPhase.Night) == Visitor.Moths && NightCreatures.For(3, DayPhase.Night) == Visitor.Frogs,
            "the rule holds at night too");
        Expect(NightCreatures.IsNightKind(Visitor.Moths) && NightCreatures.IsNightKind(Visitor.Frogs), "moths and frogs are night kinds");
        Expect(!NightCreatures.IsNightKind(Visitor.Owl), "the owl keeps its own grove rule");
        Expect(Visitors.Out(Visitor.Moths, Season.Summer, DayPhase.Night) && !Visitors.Out(Visitor.Moths, Season.Summer, DayPhase.Day),
            "moths are out in the evening only");
        Expect(Visitors.Out(Visitor.Frogs, Season.Spring, DayPhase.Dusk) && !Visitors.Out(Visitor.Frogs, Season.Spring, DayPhase.Morning),
            "frogs are out in the evening only");
        Expect(!Visitors.Welcomes(WorldModel.New(PlayMode.Living), Visitor.Moths), "a bare garden has no moths yet");
        Expect(!string.IsNullOrEmpty(Visitors.Why(Visitor.Moths)) && !string.IsNullOrEmpty(Visitors.Why(Visitor.Frogs)),
            "the almanac says why each comes");
    }

    private static void OfferIsDewInTheMorningOnly()
    {
        int day = 20261128;
        foreach (DayPhase phase in AllPhases)
        {
            DayOffer offer = DayRhythm.Offer(phase, day);
            Expect((offer == DayOffer.DewBoard) == (phase == DayPhase.Morning), "dew board only in the morning, not " + phase);
        }
        Expect(DayRhythm.Offer(DayPhase.Morning, 0) == DayOffer.None, "a bad day offers nothing");
    }

    private static void OfferIsALanternAtDuskOnly()
    {
        int day = 20261128;
        foreach (DayPhase phase in AllPhases)
        {
            DayOffer offer = DayRhythm.Offer(phase, day);
            Expect((offer == DayOffer.LanternLetter) == (phase == DayPhase.Dusk), "lantern letter only at dusk, not " + phase);
        }
    }

    private static void OfferIsDeterministic()
    {
        for (int day = 20261101; day < 20261201; day++)
        {
            foreach (DayPhase phase in AllPhases)
                Expect(DayRhythm.Offer(phase, day) == DayRhythm.Offer(phase, day), "same day, same offer");
            Expect(DayRhythm.LanternFrom(day) == DayRhythm.LanternFrom(day) && DayRhythm.LanternWords(day) == DayRhythm.LanternWords(day),
                "the same day's letter is the same letter");
        }
    }

    private static void AtMostOneLanternLetterAday()
    {
        int day = 20261128, readOn = 0, delivered = 0;
        for (int hour = 0; hour < 24; hour++)
        {
            for (int minute = 0; minute < 60; minute += 15)
            {
                DayPhase phase = DayLight.PhaseAt(hour, minute);
                if (DayRhythm.LetterDue(phase, day, readOn))
                {
                    delivered++;
                    readOn = day; // the app remembers the day it was read
                }
            }
        }
        Expect(delivered == 1, "exactly one lantern letter in a day, not " + delivered);
        Expect(!DayRhythm.LetterDue(DayPhase.Dusk, day, day), "a read letter is not due again that day");
        Expect(DayRhythm.LetterDue(DayPhase.Dusk, day + 1, day), "tomorrow's evening brings tomorrow's letter");
    }

    private static void CopyPassesWordingRules()
    {
        var copy = new System.Collections.Generic.List<string> { DayRhythm.DewBoardWords };
        for (int day = 20261101; day < 20261131; day++)
        {
            copy.Add(DayRhythm.LanternWords(day));
            copy.Add(DayRhythm.LanternFrom(day));
        }
        foreach (string line in copy)
        {
            Expect(!LossWords.IsMatch(line), "no loss-like words in: " + line);
            Expect(line.Length > 0 && line.Length <= 140, "short copy: " + line);
            Expect(!line.Contains("!"), "no exclamations: " + line);
        }
        Expect(!DayRhythm.LanternWords(20261128).Contains("write back") || DayRhythm.LanternWords(20261128).Contains("No need"),
            "a lantern letter never asks for a reply");
    }

    // Each stretch's creature sits on free land inside its own stretch, away from the screen's edges
    // (not in the outer columns, where the buttons are), frogs beside the water where the stretch has such a cell.
    private static void NightSpotsSitMidStretch()
    {
        GardenMap map = GardenMap.River(10, 15, CompanionFactsTests.Mask(), 6);
        Expect(NightCreatures.Spots(map, null, DayPhase.Day) == null, "no spots by day");
        Expect(NightCreatures.Spots(null, null, DayPhase.Night) == null, "no map, no spots");
        int[] spots = NightCreatures.Spots(map, null, DayPhase.Night);
        Expect(spots != null && spots.Length == map.Reaches, "one spot per stretch");
        for (int reach = 0; reach < spots.Length; reach++)
        {
            int cell = spots[reach];
            Expect(cell >= 0 && map.IsLand(cell), "stretch " + reach + " has a creature on free land");
            int row = cell / map.Columns, col = cell % map.Columns;
            Expect(row > reach * map.ReachRows && row < (reach + 1) * map.ReachRows - 1, "stretch " + reach + "'s creature is inside its stretch");
            Expect(col >= 2 && col <= map.Columns - 3, "stretch " + reach + "'s creature is away from the screen's edges (column " + col + ")");
            if (reach % 2 == 1) Expect(map.DistanceToWater(cell) == 1, "stretch " + reach + "'s frog is beside the water");
            else Expect(map.DistanceToWater(cell) >= 2, "stretch " + reach + "'s moth is away from the water");
        }
        // A taken cell is never chosen; the creature moves to the next free one.
        var taken = new System.Collections.Generic.HashSet<int> { spots[0] };
        int[] again = NightCreatures.Spots(map, taken, DayPhase.Dusk);
        Expect(again[0] >= 0 && again[0] != spots[0], "a placed thing's cell is left to the thing");
        // A stretch with no free land has no creature (-1), and nothing throws.
        var all = new System.Collections.Generic.HashSet<int>();
        for (int cell = 0; cell < map.CellCount; cell++) all.Add(cell);
        int[] none = NightCreatures.Spots(map, all, DayPhase.Night);
        Expect(Array.TrueForAll(none, c => c == -1), "no free land, no creature");
    }

    private static void TourSavesTheShots()
    {
        string tour = ReadEvergrove("Assets/Scripts/Unity/DevQaTour.DayRhythm.cs");
        Expect(ReadEvergrove("Assets/Scripts/Unity/DevQaTour.cs").Contains("yield return DayRhythmShots();"), "the tour runs the day rhythm shots");
        Expect(tour.Contains("SavePng(\"20-night-reach-\" + reach + \".png\")"), "the tour shoots each open stretch at night");
        Expect(tour.Contains("app.QaSetClock(22);") && tour.Contains("app.QaTravel(reach);"), "the night tour moves to each stretch");
        Expect(tour.IndexOf("app.QaShowHub(true);", StringComparison.Ordinal) < tour.IndexOf("app.QaTravel(reach);", StringComparison.Ordinal),
            "the hub is built before the boat travels (travel moves the hub's view)");
        Expect(tour.Contains("SavePng(\"20-morning-dew.png\")"), "the tour shoots the morning dew board");
        Expect(tour.Contains("SavePng(\"20-evening-lantern.png\")"), "the tour shoots the evening lantern letter");
        Expect(tour.IndexOf("SavePng(\"20-morning-dew.png\")", StringComparison.Ordinal) >
               tour.IndexOf("app.QaSetClock(7);", StringComparison.Ordinal), "the morning shot is taken at seven");
    }

    private static void KindsAppendedAtTheEnd()
    {
        Visitor[] kinds = (Visitor[])Enum.GetValues(typeof(Visitor));
        Expect(kinds.Length == 9, "nine visitors");
        Expect((int)Visitor.Bees == 0 && (int)Visitor.Owl == 5 && (int)Visitor.Kingfisher == 6, "existing visitors keep their order");
        Expect(kinds[7] == Visitor.Moths && kinds[8] == Visitor.Frogs, "moths and frogs are appended at the end");
    }

    private static void RegisteredInTheSuite()
    {
        string program = ReadEvergrove("Tests/Program.cs");
        Expect(program.Contains("assertions += KitchenTests.Run();\n            assertions += DayRhythmTests.Run();\n"),
            "DayRhythmTests runs directly after KitchenTests");
    }

    private static string ReadEvergrove(string relative)
    {
        foreach (string start in new[] { Directory.GetCurrentDirectory(), AppContext.BaseDirectory })
        {
            for (var dir = new DirectoryInfo(start); dir != null; dir = dir.Parent)
            {
                string nested = Path.Combine(dir.FullName, "games", "evergrove", relative);
                if (File.Exists(nested)) return File.ReadAllText(nested);
                string direct = Path.Combine(dir.FullName, relative);
                if (File.Exists(direct)) return File.ReadAllText(direct);
            }
        }
        throw new Exception("Could not find " + relative + " from " + Directory.GetCurrentDirectory());
    }
}
