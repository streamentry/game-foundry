using System;
using System.IO;
using Evergrove.Core;

/// <summary>
/// Restore card comparison: the plain summary lines (GardenCompare) shown before a garden file is
/// restored. Pins the wording, singular and plural, the optional name, the "smaller" warning, that
/// nothing is mutated, and that the Unity card calls the compare lines.
/// </summary>
internal static class GardenCompareTests
{
    private static int assertions;

    private static void Expect(bool ok, string message)
    {
        assertions++;
        if (!ok) throw new Exception("GardenCompareTests failed: " + message);
    }

    private static void Solve(WorldModel world)
    {
        Puzzle board = world.CurrentPuzzle;
        for (int tile = 0; tile < board.CellCount; tile++)
            for (int turn = 0; turn < 4 && world.Rotations[tile] != board.Solution[tile]; turn++)
                Expect(world.Rotate(tile), "solver turns only mutable tiles");
        Expect(world.Solved, "fixture reaches a valid solution");
    }

    private static GardenCompare.Summary Make(string name, int streams, int things, int days)
    {
        return new GardenCompare.Summary { Name = name, Streams = streams, Things = things, Days = days };
    }

    public static int Run()
    {
        assertions = 0;

        // Line formatting: named, plural everywhere.
        Expect(GardenCompare.Line("On this phone", Make("Willowbank", 7, 23, 12)) ==
               "On this phone: Willowbank · 7 streams · 23 things · 12 days",
            "named plural line is exact");

        // Unnamed (blank and null), singular everywhere: the name and its separator are omitted.
        Expect(GardenCompare.Line("In the file", Make("", 1, 1, 1)) ==
               "In the file: 1 stream · 1 thing · 1 day",
            "unnamed singular line is exact");
        Expect(GardenCompare.Line("In the file", Make("   ", 1, 1, 1)) ==
               "In the file: 1 stream · 1 thing · 1 day",
            "a whitespace-only name is omitted");
        Expect(GardenCompare.Line("In the file", Make(null, 0, 0, 0)) ==
               "In the file: 0 streams · 0 things · 0 days",
            "a null name with zero counts is exact");

        // New() vs New(): two lines, no warning.
        WorldModel fresh = WorldModel.New(PlayMode.Living);
        WorldModel freshFile = WorldModel.New(PlayMode.Living);
        string[] same = GardenCompare.Lines(fresh, freshFile);
        Expect(same.Length == 2, "two fresh gardens give two lines");
        Expect(same[0].StartsWith("On this phone: ", StringComparison.Ordinal), "first line is this phone");
        Expect(same[1].StartsWith("In the file: ", StringComparison.Ordinal), "second line is the file");
        Expect(Array.IndexOf(same, GardenCompare.SmallerLine) < 0, "equal gardens show no smaller warning");

        // A named world line: the name appears when set.
        WorldModel named = WorldModel.New(PlayMode.Living);
        Expect(named.SetGardenName("Quiet Corner"), "a garden name for the compare test");
        Expect(GardenCompare.Lines(named, freshFile)[0] ==
               "On this phone: Quiet Corner · 0 streams · 0 things · 0 days",
            "a named garden shows its name first");

        // One solved board and one committed choice (current) vs a fresh garden (incoming): warning.
        WorldModel played = WorldModel.New(PlayMode.Living);
        Solve(played);
        Expect(played.Choose(VillageChoice.Flowers), "the solved board records a choice");
        string[] smaller = GardenCompare.Lines(played, WorldModel.New(PlayMode.Living));
        Expect(smaller.Length == 3, "a smaller file gives a third line");
        Expect(smaller[2] == "The file's garden is smaller than this phone's.", "third line is the smaller warning");
        Expect(smaller[1] == "In the file: 0 streams · 0 things · 0 days", "fresh file line is zero counts");
        Expect(smaller[0] == "On this phone: 1 stream · 0 things · 0 days", "played phone line is singular");

        // Swapped: the file is the bigger garden, so no warning.
        string[] bigger = GardenCompare.Lines(WorldModel.New(PlayMode.Living), played);
        Expect(bigger.Length == 2, "a bigger file gives no third line");
        Expect(Array.IndexOf(bigger, GardenCompare.SmallerLine) < 0, "bigger file has no smaller warning");

        // Lines does not mutate either world.
        int choicesBefore = played.Choices.Length;
        int daysBefore = played.DaysPlayed;
        string nameBefore = named.GardenName;
        GardenCompare.Lines(played, named);
        GardenCompare.Lines(named, played);
        Expect(played.Choices.Length == choicesBefore && played.DaysPlayed == daysBefore,
            "Lines leaves the played world unchanged");
        Expect(named.GardenName == nameBefore && named.Choices.Length == 0,
            "Lines leaves the named world unchanged");

        // Of(null) is safe and empty.
        GardenCompare.Summary none = GardenCompare.Of(null);
        Expect(none.Name == "" && none.Streams == 0 && none.Things == 0 && none.Days == 0, "Of(null) is empty");
        Expect(GardenCompare.Line("In the file", none) == "In the file: 0 streams · 0 things · 0 days",
            "an empty summary still reads as a line");
        string[] nullLines = GardenCompare.Lines(null, null);
        Expect(nullLines.Length == 2, "null worlds still give the two base lines");

        // Unity card calls the compare lines.
        string source = ReadRepoFile("games/evergrove/Assets/Scripts/Unity/EvergroveApp.GardenFile.cs");
        Expect(source != null, "EvergroveApp.GardenFile.cs can be found");
        if (source != null)
            Expect(source.Contains("GardenCompare.Lines(world, pendingGardenRestore)"),
                "the restore card calls GardenCompare.Lines(world, pendingGardenRestore)");

        return assertions;
    }

    private static string ReadRepoFile(string relative)
    {
        string[] starts = { Directory.GetCurrentDirectory(), AppContext.BaseDirectory };
        foreach (string start in starts)
        {
            string dir = Path.GetFullPath(start);
            for (int depth = 0; depth < 12 && dir != null; depth++)
            {
                string candidate = Path.Combine(dir, relative);
                if (File.Exists(candidate)) return File.ReadAllText(candidate);
                dir = Path.GetDirectoryName(dir);
            }
        }
        return null;
    }
}
