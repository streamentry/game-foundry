using System;
using Evergrove.Core;

/// <summary>Ideas 82, 84 and 90: the habit line, the wish sweep, and the long-absence letter.</summary>
internal static class CompanionRulesTests
{
    private static int assertions;

    private static void Expect(bool ok, string message)
    {
        assertions++;
        if (!ok) throw new Exception("CompanionRulesTests failed: " + message);
    }

    private static readonly string[] Forbidden = { "died", "lost", "missed", "gone", "forgot", "sorry", "finally", "where were you" };

    public static int Run()
    {
        assertions = 0;
        HabitNeedsFiveMornings();
        WishSweep();
        LongAwayLetters();
        return assertions;
    }

    // ---- Idea 82: the morning line appears only after five morning visits, never before.

    private static void HabitNeedsFiveMornings()
    {
        int morning = (int)DayPhase.Morning;
        int[] four = new int[Companion.PhaseCount];
        four[morning] = 4;
        Expect(Companion.Habit(four) == null, "four morning visits: no habit yet");
        for (int day = 0; day < 70; day++)
            Expect(Companion.HabitLine(four, 0, day) == null, "four mornings: no line on day " + day);

        int[] five = new int[Companion.PhaseCount];
        five[morning] = 5;
        Expect(Companion.Habit(five) == DayPhase.Morning, "five morning visits: the morning habit");
        int said = 0;
        for (int day = 0; day < 70; day++)
        {
            string line = Companion.HabitLine(five, 0, day);
            if (line == null) continue;
            Expect(!line.Contains("!") && !line.ToLowerInvariant().Contains("you never") && !line.ToLowerInvariant().Contains("you always"),
                "the morning line stays gentle: " + line);
            said++;
        }
        Expect(said == 10, "five mornings: the line shows one day in seven (" + said + ")");

        // Non-morning visits do not count towards mornings, and do not hide them.
        int[] dayVisits = new int[Companion.PhaseCount];
        dayVisits[(int)DayPhase.Morning] = 4;
        dayVisits[(int)DayPhase.Day] = 9;
        Expect(Companion.Habit(dayVisits) == DayPhase.Day, "four mornings and nine afternoons: the afternoon habit, not mornings");
        for (int day = 0; day < 70; day++)
            Expect(Companion.HabitLine(dayVisits, 0, day) == null, "four mornings never show the morning line on day " + day);

        int[] mixed = new int[Companion.PhaseCount];
        mixed[(int)DayPhase.Morning] = 5;
        mixed[(int)DayPhase.Dusk] = 4;
        Expect(Companion.Habit(mixed) == DayPhase.Morning, "five mornings with four evenings: still mornings");

        // Every hour uses the same five-visit threshold.
        foreach (DayPhase phase in Enum.GetValues(typeof(DayPhase)))
        {
            int[] counts = new int[Companion.PhaseCount];
            counts[(int)phase] = 4;
            Expect(Companion.Habit(counts) == null, phase + ": four visits, no habit");
            counts[(int)phase] = 5;
            Expect(Companion.Habit(counts) == phase, phase + ": five visits, the habit");
        }
    }

    // ---- Idea 84: every wish a neighbour can offer, at every state of a grown garden, is satisfiable.

    private static void WishSweep()
    {
        int[] gardens = { 0, 7, 999 };
        int[][] policies = { new[] { 0, 1, 2 }, new[] { 1 }, new[] { 2 } };
        foreach (int garden in gardens)
        {
            foreach (int[] policy in policies)
            {
                WorldModel world = WorldModel.New(PlayMode.Living, garden);
                for (int chapter = 0; ; chapter++)
                {
                    for (int villager = 0; villager < 3; villager++)
                    {
                        NeighbourWish wish = NeighbourWish.For(world, villager);
                        Expect(wish != null, "a wish is offered for villager " + villager);
                        Expect(CanSatisfy(world, villager),
                            "garden " + garden + " at chapter " + chapter + ": '" + wish.Hope + "' can be satisfied from here");
                    }
                    if (world.Completed) break;
                    if (!Play(world, (VillageChoice)policy[chapter % policy.Length])) break;
                }
                Expect(world.Completed, "garden " + garden + " grows through all chapters");
            }
        }
    }

    // Proof by simulation: from this state, the public actions alone reach the wish.
    private static bool CanSatisfy(WorldModel start, int villager)
    {
        WorldModel w = WorldModel.Restore(start.Export());
        VillageChoice pick = (VillageChoice)villager;
        int day = 0;
        for (int guard = 0; guard < 600 && !NeighbourWish.For(w, villager).Granted; guard++)
        {
            if (!w.Completed)
            {
                if (!Play(w, pick)) return false;
                continue;
            }
            if (villager == (int)VillageChoice.Bridge)
            {
                int unsolved = FirstUnsolved(w);
                if (unsolved < 0) return false;
                w.SolveTree(unsolved);
            }
            else if (!BuyOne(w, pick, ref day)) return false;
        }
        return NeighbourWish.For(w, villager).Granted;
    }

    private static bool Play(WorldModel w, VillageChoice pick)
    {
        Puzzle board = w.CurrentPuzzle;
        if (board == null) return false;
        for (int i = 0; i < board.CellCount; i++)
            for (int r = 0; r < 4 && w.Rotations[i] != board.Solution[i]; r++) w.Rotate(i);
        int before = w.Choices.Length;
        if (!w.Choose(pick)) return false;
        if (pick == VillageChoice.ShadeTree) w.SolveTree(before);
        return true;
    }

    private static int FirstUnsolved(WorldModel w)
    {
        for (int i = 0; i < w.Choices.Length; i++)
            if (!w.TreeSolved(i)) return i;
        return -1;
    }

    // After the story: earn seeds by solving, then buy a plant of the wanted family from the nursery.
    private static bool BuyOne(WorldModel w, VillageChoice family, ref int day)
    {
        for (int slot = 0; slot < Nursery.Slots; slot++)
        {
            int kind = w.NurseryStock(slot);
            if (kind < 0 || kind >= Nursery.Kinds || Nursery.Family(kind) != family || !w.RowOpen(Nursery.Row(kind))) continue;
            if (Owns(w, kind)) continue;
            if (w.Seeds < Nursery.Price(kind))
            {
                int unsolved = FirstUnsolved(w);
                if (unsolved >= 0) w.SolveTree(unsolved);
                else w.SolveTree(day % w.Choices.Length, 20270101 + day++); // a replay of a solved puzzle, on a new day
                return true;
            }
            for (int cell = 0; cell < 400; cell++)
            {
                if (!w.Buy(kind, cell)) continue;
                w.SolveBought(w.BoughtCount - 1);
                return true;
            }
        }
        return false;
    }

    private static bool Owns(WorldModel w, int kind)
    {
        int[] bought = w.Bought;
        for (int k = 0; k * 2 < bought.Length; k++)
            if (bought[k * 2] == kind) return true;
        return false;
    }

    // ---- Idea 90: a 90, 180 or 365 day absence gets a short, specific, loss-free letter.

    private static void LongAwayLetters()
    {
        int[] gardens = { 0, 7, 999 };
        int[] days = { 90, 180, 365 };
        foreach (int garden in gardens)
        {
            WorldModel named = WorldModel.New(PlayMode.Living, garden);
            WorldModel unnamed = WorldModel.New(PlayMode.Living, garden);
            Expect(named.SetGardenName("Quiet Corner"), "a garden name for the letter test");
            foreach (WorldModel world in new[] { named, unnamed })
            {
                for (int chapter = 0; chapter < 4 && !world.Completed; chapter++)
                    Play(world, chapter % 2 == 0 ? VillageChoice.ShadeTree : VillageChoice.Flowers);
                string thing = LastThing(world);
                foreach (int away in days)
                {
                    string[] letter = Companion.AbsenceLetter(world, away);
                    Expect(letter != null && letter.Length >= 1 && letter.Length <= 3, "garden " + garden + ": up to three lines after " + away);
                    string joined = string.Join("\n", letter);
                    foreach (string line in letter)
                    {
                        Expect(line.Length <= 80, "a line of at most 80 characters: " + line);
                        Expect(!line.Contains("!"), "no exclamation: " + line);
                    }
                    string lower = joined.ToLowerInvariant();
                    foreach (string word in Forbidden)
                        Expect(!lower.Contains(word), "no loss or guilt words, found '" + word + "' in: " + joined);
                    string specific = world.GardenName.Length > 0 ? world.GardenName : thing;
                    Expect(specific != null && joined.Contains(specific), "names this garden after " + away + " days: " + joined);
                    string[] again = Companion.AbsenceLetter(world, away);
                    Expect(joined == string.Join("\n", again), "the same letter every time after " + away + " days");
                    // The return path itself, simulated for the same absence.
                    DateTime last = new DateTime(2026, 9, 1);
                    DayWeather.SinceLastVisit(last, last.AddDays(away), Season.Autumn, garden);
                }
                Expect(Companion.AbsenceLetter(world, 89) == null, "under ninety days, no letter");
            }
        }
        Expect(Companion.AbsenceLetter(null, 90) == null, "no garden, no letter");
    }

    private static string LastThing(WorldModel world)
    {
        int[] made = world.Choices;
        for (int i = made.Length - 1; i >= 0; i--)
            if (made[i] >= 0 && made[i] <= 2)
                return GardenCatalog.For(world.Garden, Math.Min(i, GardenCatalog.ChaptersPerGarden - 1), (VillageChoice)made[i]).Name;
        return null;
    }
}
