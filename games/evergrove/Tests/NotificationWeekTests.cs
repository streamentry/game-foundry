using System;
using System.Collections.Generic;
using System.IO;
using Evergrove.Core;

/// <summary>
/// Checklist 57: notifications only for real arrivals, at most one a day, opt-in, and never more
/// than seven in a week. Gardens are grown through the public API; the plan is GardenNote.ForWeek.
/// </summary>
internal static class NotificationWeekTests
{
    private static int assertions;
    private const int Monday = 20261130; // a fixed week, starting on a Monday

    private static void Expect(bool ok, string message)
    {
        assertions++;
        if (!ok) throw new Exception("NotificationWeekTests failed: " + message);
    }

    public static int Run()
    {
        int[] growths = { 0, 1, 3, 5, 12 };
        foreach (int grown in growths)
        {
            for (int letters = 0; letters <= 3; letters++)
            {
                for (int gifts = 0; gifts <= 2; gifts++)
                {
                    WorldModel world = Garden(grown, letters, gifts);
                    foreach (bool unread in new[] { false, true })
                    {
                        foreach (bool optedIn in new[] { false, true })
                        {
                            foreach (int unopened in new[] { 0, 1, 2, 3, 5 })
                            {
                                string label = "grown " + grown + ", letters " + letters + ", gifts " + gifts +
                                    ", unread " + unread + ", on " + optedIn + ", unopened " + unopened;
                                CheckWeek(world, unread, optedIn, unopened, label);
                            }
                        }
                    }
                }
            }
        }

        // Nothing has arrived, however much is tended or still holds a puzzle: no note at all.
        Expect(GardenNote.ForWeek(Garden(5, 0, 0), Monday, true, true, 0).Count == 0, "a puzzle waiting is not an arrival");
        Expect(GardenNote.ForWeek(Garden(12, 0, 0), Monday, true, true, 0).Count == 0, "a replay is not an arrival");
        Expect(GardenNote.ForWeek(Garden(12, 0, 0), Monday, false, true, 0).Count == 0, "the river is not an arrival");

        // The week's dates run on across the month and year end, one per day.
        List<PlannedNote> newYear = GardenNote.ForWeek(Garden(0, 1, 0), 20261229, true, true, 0);
        Expect(newYear.Count == 3, "three days of a waiting letter");
        Expect(newYear[0].Day == 20261229 && newYear[1].Day == 20261230 && newYear[2].Day == 20261231,
            "dates run on over the year end");
        Expect(GardenNote.ForWeek(Garden(0, 1, 0), 20261231, true, true, 0)[1].Day == 20270101, "January after New Year's Eve");

        // A bad start day plans nothing rather than guessing.
        Expect(GardenNote.ForWeek(Garden(0, 1, 0), 20261345, true, true, 0).Count == 0, "an impossible date plans nothing");

        // Deterministic.
        WorldModel same = Garden(5, 2, 1);
        Expect(Describe(GardenNote.ForWeek(same, Monday, true, true, 0)) ==
               Describe(GardenNote.ForWeek(same, Monday, true, true, 0)), "same garden, same week");

        SourceCheck();
        return assertions;
    }

    private static void CheckWeek(WorldModel world, bool unread, bool optedIn, int unopened, string label)
    {
        List<PlannedNote> plan = GardenNote.ForWeek(world, Monday, unread, optedIn, unopened);
        bool letterWaiting = unread && world.Letters.Length > 0;
        bool basketWaiting = world.Gifts.Length > 0;
        bool waiting = letterWaiting || basketWaiting;

        Expect(plan.Count <= 7, label + ": at most seven in a week (" + plan.Count + ")");
        var days = new HashSet<int>();
        foreach (PlannedNote item in plan)
        {
            Expect(days.Add(item.Day), label + ": at most one a day (" + item.Day + ")");
            Expect(item.Note != null, label + ": every planned day says something");
            Expect(item.Note.Target == NoteTarget.Letterbox || item.Note.Target == NoteTarget.Basket,
                label + ": only arrival kinds, found " + item.Note.Target);
            Expect(item.Note.Target != NoteTarget.Letterbox || letterWaiting, label + ": a letter note needs an unread letter");
            Expect(item.Note.Target != NoteTarget.Basket || basketWaiting, label + ": a basket note needs a cutting");
            Expect(item.Note.Body.Length <= 140 && !item.Note.Body.Contains("!"), label + ": kind and short");
        }

        if (!optedIn || !waiting || unopened >= GardenNote.MaxUnopened)
            Expect(plan.Count == 0, label + ": nothing is planned (opted out, nothing waiting, or backed off)");
        else
        {
            // Stops after three unopened: the plan holds exactly the days the back-off still allows.
            Expect(plan.Count == GardenNote.MaxUnopened - unopened, label + ": stops after three unopened (" + plan.Count + ")");
            for (int i = 1; i < plan.Count; i++)
                Expect(plan[i].Day == plan[i - 1].Day + 1 || plan[i].Day == NextDay(plan[i - 1].Day),
                    label + ": consecutive days");
        }
    }

    private static int NextDay(int yyyymmdd)
    {
        var day = new DateTime(yyyymmdd / 10000, yyyymmdd / 100 % 100, yyyymmdd % 100).AddDays(1);
        return GardenNote.DayNumber(day);
    }

    private static string Describe(List<PlannedNote> plan)
    {
        var text = new System.Text.StringBuilder();
        foreach (PlannedNote item in plan) text.Append(item.Day).Append(' ').Append(item.Note.Body).Append('|');
        return text.ToString();
    }

    // A garden grown through the public API, then given letters (oldest first) and cuttings in the basket.
    private static WorldModel Garden(int grown, int letters, int gifts)
    {
        WorldModel world = WorldModel.New(PlayMode.Living);
        Grow(world, grown);
        WorldSnapshot s = world.Export();
        s.letters = new string[letters];
        s.letterFrom = new string[letters];
        for (int i = 0; i < letters; i++)
        {
            s.letters[i] = "The pears are in, number " + i + ".";
            s.letterFrom[i] = i % 2 == 0 ? "Nan" : "Amara";
        }
        s.gifts = new int[gifts];
        for (int i = 0; i < gifts; i++) s.gifts[i] = i % Nursery.Kinds;
        Expect(WorldModel.TryRestore(s, out WorldModel restored), "restore a garden with " + letters + " letters and " + gifts + " cuttings");
        return restored;
    }

    private static void Grow(WorldModel world, int count)
    {
        for (int chapter = 0; chapter < count; chapter++)
        {
            Puzzle board = world.CurrentPuzzle;
            for (int i = 0; i < board.CellCount; i++)
                while (world.Rotations[i] != board.Solution[i]) world.Rotate(i);
            world.Choose((VillageChoice)(chapter % 3));
        }
    }

    // Checklist 57: the scheduler in Unity queues only the Core plan, and no other note source.
    private static void SourceCheck()
    {
        string path = FindSource("Assets/Scripts/Unity/GardenNotes.cs");
        string text = File.ReadAllText(path);
        Expect(text.Contains("GardenNote.ForWeek("), "GardenNotes.cs schedules through GardenNote.ForWeek");
        Expect(!text.Contains("GardenNote.For("), "GardenNotes.cs does not pick notes on its own");
        Expect(!text.Contains("From = \""), "GardenNotes.cs authors no sender text");
        Expect(!text.Contains("Body = \""), "GardenNotes.cs assigns no body text");
    }

    private static string FindSource(string relative)
    {
        foreach (string start in new[] { Directory.GetCurrentDirectory(), AppContext.BaseDirectory })
        {
            string dir = Path.GetFullPath(start);
            for (int up = 0; up < 12 && dir != null; up++, dir = Path.GetDirectoryName(dir))
            {
                foreach (string candidate in new[] { Path.Combine(dir, "games", "evergrove", relative), Path.Combine(dir, relative) })
                    if (File.Exists(candidate)) return candidate;
            }
        }
        throw new Exception("NotificationWeekTests failed: cannot find " + relative + " from the working directory");
    }
}
