using System;
using System.Text.Json;
using Evergrove.Core;

/// <summary>
/// The choice preview names the same cause the protected pipe will show after confirmation
/// (vision run 20261128-1801, idea 2). Pins that the preview's next-stream rule uses
/// LandmarkCause.For over the detached WorldAfter, that creating previews never changes the
/// live garden, and that research controls fall back to the generic environmental line.
/// </summary>
internal static class PreviewCauseTests
{
    private static int assertions;
    private static readonly JsonSerializerOptions Options = new JsonSerializerOptions { IncludeFields = true };

    private static void Expect(bool condition, string message)
    {
        assertions++;
        if (!condition) throw new Exception("PreviewCauseTests failed: " + message);
    }

    private static string Snapshot(WorldModel world)
    {
        return JsonSerializer.Serialize(world.Export(), Options);
    }

    private static void Solve(WorldModel world)
    {
        Puzzle board = world.CurrentPuzzle;
        for (int tile = 0; tile < board.CellCount; tile++)
            for (int turn = 0; turn < 4 && world.Rotations[tile] != board.Solution[tile]; turn++)
                Expect(world.Rotate(tile), "solver turns only mutable tiles");
        Expect(world.Solved, "fixture reaches a valid solution");
    }

    public static int Run()
    {
        assertions = 0;
        string previousLocale = GameLocalization.Current;
        try
        {
            Expect(GameLocalization.SetCurrent("en"), "English locale must be selectable for the preview cause check");

            VillageChoice[] choices = { VillageChoice.ShadeTree, VillageChoice.Bridge, VillageChoice.Flowers };
            foreach (VillageChoice choice in choices)
            {
                WorldModel world = WorldModel.New(PlayMode.Living);
                Solve(world);
                string before = Snapshot(world);

                VillageChoicePreview preview;
                Expect(VillageChoicePreview.TryCreate(world, choice, out preview),
                    "living stream 1 previews " + choice);
                Expect(Snapshot(world) == before && world.Choices.Length == 0,
                    "previewing " + choice + " leaves the live garden and its choices unchanged");

                string item = GardenCatalog.For(world.Garden, 0, choice).Name;
                string specific = LandmarkCause.Specific(preview.WorldAfter);
                Expect(specific.Length > 0, "preview of " + choice + " proves a specific cause");
                Expect(specific.Contains(item), "preview of " + choice + " names the catalogue item " + item);
                Expect(LandmarkCause.For(preview.WorldAfter) == specific,
                    "preview rule for " + choice + " equals the specific line the board will show");
            }

            // Research control: no story-choice preview exists, so the generic line is the only cause.
            WorldModel puzzle = WorldModel.New(PlayMode.PuzzleOnly);
            Solve(puzzle);
            string puzzleBefore = Snapshot(puzzle);
            VillageChoicePreview rejected;
            Expect(!VillageChoicePreview.TryCreate(puzzle, VillageChoice.Bridge, out rejected) && rejected == null,
                "puzzle-only world offers no choice preview");
            Expect(Snapshot(puzzle) == puzzleBefore, "rejected puzzle-only preview leaves the world unchanged");
            Expect(LandmarkCause.Specific(puzzle) == "", "puzzle-only world has no specific cause");
            Expect(LandmarkCause.For(puzzle) == LocalizedVillage.EnvironmentalRule(puzzle),
                "puzzle-only world uses the generic environmental line");
        }
        finally
        {
            if (previousLocale != null) GameLocalization.SetCurrent(previousLocale);
        }
        Expect(GameLocalization.Current == previousLocale || previousLocale == null,
            "locale restored after the preview cause check");
        Console.WriteLine("Evergrove preview cause tests: " + assertions + " assertions.");
        return assertions;
    }
}
