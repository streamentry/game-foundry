using System;
using System.IO;
using System.Text.Json;
using Evergrove.Core;

/// <summary>
/// The protected pipe names the village choice that keeps it in place. Pins the exact sentences,
/// the " · " suffix strip, the truth guard (live, unfinished, matching variant, English only), and
/// that the Unity tap path calls LandmarkCause.For instead of the generic line.
/// </summary>
internal static class LandmarkCauseTests
{
    private static int assertions;

    private static void Expect(bool ok, string message)
    {
        assertions++;
        if (!ok) throw new Exception("LandmarkCauseTests failed: " + message);
    }

    public static int Run()
    {
        assertions = 0;

        // Exact sentences, with and without board names.
        Expect(LandmarkCause.Line(VillageChoice.ShadeTree, "First Bend") ==
            "Your shade tree from First Bend roots this pipe in place.", "tree sentence");
        Expect(LandmarkCause.Line(VillageChoice.Bridge, "First Bend") ==
            "Your footbridge from First Bend holds this pipe in place.", "bridge sentence");
        Expect(LandmarkCause.Line(VillageChoice.Flowers, "First Bend") ==
            "Your wildflowers from First Bend keep this pipe in place.", "flowers sentence");
        Expect(LandmarkCause.Line(VillageChoice.ShadeTree, "Copper Run · Bridge foundation") ==
            "Your shade tree from Copper Run roots this pipe in place.", "suffix stripped after the middle dot");
        Expect(LandmarkCause.Line(VillageChoice.Bridge, null) ==
            "Your footbridge holds this pipe in place.", "null board name omits the board");
        Expect(LandmarkCause.Line(VillageChoice.Flowers, "   ") ==
            "Your wildflowers keep this pipe in place.", "blank board name omits the board");

        // Fresh world: no choices, generic line (empty at variant 0).
        WorldModel fresh = WorldModel.New(PlayMode.Living);
        Expect(LandmarkCause.For(fresh) == LocalizedVillage.EnvironmentalRule(fresh), "fresh world uses the generic line");
        Expect(LandmarkCause.For(null) == "", "null world gives an empty line");

        // Living world after each first choice on board 0.
        VillageChoice[] choices = { VillageChoice.ShadeTree, VillageChoice.Bridge, VillageChoice.Flowers };
        string[] nouns = { "shade tree", "footbridge", "wildflowers" };
        for (int i = 0; i < choices.Length; i++)
        {
            WorldModel world = WorldModel.New(PlayMode.Living);
            Solve(world);
            Expect(world.Choose(choices[i]), "choice " + choices[i] + " is accepted after board 0");
            string line = LandmarkCause.For(world);
            Expect(line.Contains(nouns[i]), "cause names " + nouns[i]);
            Expect(line.Contains("First Bend"), "cause names the board where the choice was made");
            Expect(line != LocalizedVillage.EnvironmentalRule(world), "cause differs from the generic line");
            Expect(line == LandmarkCause.Line(choices[i], PuzzleCatalog.Get(0).Name), "cause equals Line for board 0");
        }

        // Puzzle-only and matched worlds never record choices, so the generic line stays.
        WorldModel puzzle = WorldModel.New(PlayMode.PuzzleOnly);
        Solve(puzzle);
        Expect(!puzzle.Choose(VillageChoice.Bridge), "puzzle-only world rejects a village choice");
        Expect(LandmarkCause.For(puzzle) == LocalizedVillage.EnvironmentalRule(puzzle), "puzzle-only uses the generic line");

        int[] schedule = new int[WorldModel.ScheduleLength];
        Array.Fill(schedule, 1);
        WorldModel matched = WorldModel.NewMatched(schedule);
        Solve(matched);
        Expect(!matched.Choose(VillageChoice.Bridge), "matched world rejects a village choice");
        Expect(LandmarkCause.For(matched) == LocalizedVillage.EnvironmentalRule(matched), "matched uses the generic line");

        // Non-English locale keeps the generic (localized) line.
        WorldModel localeWorld = WorldModel.New(PlayMode.Living);
        Solve(localeWorld);
        Expect(localeWorld.Choose(VillageChoice.Bridge), "locale fixture records a choice");
        string japanese = LocalizedVillage.EnvironmentalRule(localeWorld);
        Expect(japanese.Length > 0, "generic line is non-empty before the locale switch");
        Expect(GameLocalization.Register(LoadBundle("ja.json")) && GameLocalization.SetCurrent("ja"),
            "Japanese catalogue activates");
        try
        {
            Expect(LandmarkCause.For(localeWorld) == LocalizedVillage.EnvironmentalRule(localeWorld),
                "non-English locale uses the generic line");
        }
        finally
        {
            GameLocalization.SetCurrent("en");
        }
        Expect(GameLocalization.Current == GameLocalization.Fallback, "English restored after the locale check");

        // Every sentence fits in 80 characters for every board name.
        for (int i = 0; i < PuzzleCatalog.Count; i++)
        {
            string name = PuzzleCatalog.Get(i).Name;
            foreach (VillageChoice choice in choices)
                Expect(LandmarkCause.Line(choice, name).Length < 80,
                    "line under 80 characters for board " + name);
        }

        // Unity tap path calls the truthful wrapper.
        string board = ReadRepoFile("games/evergrove/Assets/Scripts/Unity/EvergroveApp.Board.cs");
        Expect(board != null, "EvergroveApp.Board.cs can be found");
        if (board != null)
            Expect(board.Contains("LandmarkCause.For(world)"), "tap path uses LandmarkCause.For(world)");

        return assertions;
    }

    private static void Solve(WorldModel world)
    {
        Puzzle board = world.CurrentPuzzle;
        for (int tile = 0; tile < board.CellCount; tile++)
            for (int turn = 0; turn < 4 && world.Rotations[tile] != board.Solution[tile]; turn++)
                Expect(world.Rotate(tile), "solver turns only mutable tiles");
        Expect(world.Solved, "fixture reaches a valid solution");
    }

    private static TranslationBundle LoadBundle(string fileName)
    {
        string file = Path.Combine(FindLocaleDirectory(), fileName);
        return JsonSerializer.Deserialize<TranslationBundle>(
            File.ReadAllText(file), new JsonSerializerOptions { IncludeFields = true });
    }

    private static string FindLocaleDirectory()
    {
        foreach (string start in new[] { Directory.GetCurrentDirectory(), AppContext.BaseDirectory })
        {
            for (DirectoryInfo dir = new DirectoryInfo(start); dir != null; dir = dir.Parent)
            {
                string at = Path.Combine(dir.FullName, "games", "evergrove", "Assets", "Resources", "Evergrove", "Locales");
                if (Directory.Exists(at)) return at;
            }
        }
        throw new Exception("Could not find checked-in locale assets");
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
