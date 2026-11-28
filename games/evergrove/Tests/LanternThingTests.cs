using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reflection;
using Evergrove.Core;

/// <summary>
/// The lantern letter names the thing the player chose last, and only when the save proves it.
/// Pins the null and non-Living guards, the latest-choice name, determinism, the three templates,
/// the catalogue length limit, and that the Unity dusk path passes the world in.
/// </summary>
internal static class LanternThingTests
{
    private static int assertions;

    private static void Expect(bool ok, string message)
    {
        assertions++;
        if (!ok) throw new Exception("LanternThingTests failed: " + message);
    }

    public static int Run()
    {
        assertions = 0;
        int[] days = { 20261101, 20261102, 20261103, 20261104, 20261105 };

        // Fresh Living world with no choices: the generic letter.
        WorldModel fresh = WorldModel.New(PlayMode.Living);
        Expect(DayRhythm.LatestThing(fresh) == null, "fresh world has no latest thing");
        foreach (int day in days)
            Expect(DayRhythm.LanternWords(day, fresh) == DayRhythm.LanternWords(day), "fresh world uses the generic letter on " + day);

        // Null world and puzzle-only world: the generic letter.
        Expect(DayRhythm.LatestThing(null) == null, "null world has no latest thing");
        foreach (int day in days)
            Expect(DayRhythm.LanternWords(day, null) == DayRhythm.LanternWords(day), "null world uses the generic letter on " + day);

        WorldModel puzzle = WorldModel.New(PlayMode.PuzzleOnly);
        Solve(puzzle);
        Expect(DayRhythm.LatestThing(puzzle) == null, "puzzle-only world has no latest thing");
        foreach (int day in days)
            Expect(DayRhythm.LanternWords(day, puzzle) == DayRhythm.LanternWords(day), "puzzle-only uses the generic letter on " + day);

        // After one choice, the letter names it.
        WorldModel world = WorldModel.New(PlayMode.Living);
        Solve(world);
        Expect(world.Choose(VillageChoice.Bridge), "first choice is accepted");
        string first = GardenCatalog.For(world.Garden, 0, VillageChoice.Bridge).Name;
        Expect(DayRhythm.LatestThing(world) == first, "latest thing is the first choice's name");
        foreach (int day in days)
        {
            string words = DayRhythm.LanternWords(day, world);
            Expect(words.Contains(first), "letter names the first choice on " + day);
            Expect(words != DayRhythm.LanternWords(day), "named letter differs from the generic letter on " + day);
        }

        // After a second, different choice, the name follows the latest choice.
        Solve(world);
        Expect(world.Choose(VillageChoice.Flowers), "second choice is accepted");
        string second = GardenCatalog.For(world.Garden, 1, VillageChoice.Flowers).Name;
        Expect(DayRhythm.LatestThing(world) == second, "latest thing follows the second choice");
        Expect(DayRhythm.LanternWords(days[0], world).Contains(second), "letter names the second choice");
        Expect(!DayRhythm.LanternWords(days[0], world).Contains(first) || first == second,
            "letter no longer names the first choice");

        // Deterministic: same day and same world give the same string; templates vary by day.
        foreach (int day in days)
            Expect(DayRhythm.LanternWords(day, world) == DayRhythm.LanternWords(day, world), "same day and world repeat on " + day);
        HashSet<string> seen = new HashSet<string>();
        for (int day = 20261101; day <= 20261103; day++)
            seen.Add(DayRhythm.LanternWords(day, world));
        Expect(seen.Count == 3, "all three named templates appear across 20261101..20261103");

        // Every named line stays short and free of absence language, for every catalogue item.
        string[] templates = Templates();
        Expect(templates.Length == 3, "three named templates exist");
        foreach (string template in templates)
        {
            for (int family = 0; family <= 2; family++)
            {
                VillageChoice choice = (VillageChoice)family;
                int count = GardenCatalog.CountOf(choice);
                for (int i = 0; i < count; i++)
                {
                    string name = GardenCatalog.Get(choice, i).Name;
                    string line = string.Format(CultureInfo.InvariantCulture, template, name);
                    Expect(line.Length < 120, "named line under 120 characters for " + name);
                    Expect(!line.Contains("away") && !line.Contains("miss") && !line.Contains("while you"),
                        "named line avoids absence language for " + name);
                }
            }
        }

        // The Unity dusk path passes the world into the letter.
        string app = ReadRepoFile("games/evergrove/Assets/Scripts/Unity/EvergroveApp.DayRhythm.cs");
        Expect(app != null, "EvergroveApp.DayRhythm.cs can be found");
        if (app != null)
            Expect(app.Contains("DayRhythm.LanternWords(day, world)"), "dusk path uses LanternWords(day, world)");

        return assertions;
    }

    private static string[] Templates()
    {
        FieldInfo field = typeof(DayRhythm).GetField("NamedLetters", BindingFlags.NonPublic | BindingFlags.Static);
        Expect(field != null, "named templates are reachable");
        return (string[])field.GetValue(null);
    }

    private static void Solve(WorldModel world)
    {
        Puzzle board = world.CurrentPuzzle;
        for (int tile = 0; tile < board.CellCount; tile++)
            for (int turn = 0; turn < 4 && world.Rotations[tile] != board.Solution[tile]; turn++)
                Expect(world.Rotate(tile), "solver turns only mutable tiles");
        Expect(world.Solved, "fixture reaches a valid solution");
    }

    private static string ReadRepoFile(string relative)
    {
        foreach (string start in new[] { Directory.GetCurrentDirectory(), AppContext.BaseDirectory })
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
