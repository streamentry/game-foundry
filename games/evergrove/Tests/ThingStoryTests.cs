using System;
using System.IO;
using Evergrove.Core;

/// <summary>
/// The long-press caption for a chosen thing. Pins the exact sentences and the stream counts
/// (never days or clock words), the living-only truth guard, determinism against a real saved
/// world, and that the Unity map keeps the generic family line as its fallback.
/// </summary>
internal static class ThingStoryTests
{
    private static int assertions;

    private static void Expect(bool ok, string message)
    {
        assertions++;
        if (!ok) throw new Exception("ThingStoryTests failed: " + message);
    }

    public static int Run()
    {
        assertions = 0;

        // Exact sentences for the stream counts that matter.
        Expect(ThingStory.Line("quince tree", 2, 3) ==
            "Quince tree, chosen after stream 3. No streams have run past it yet.", "zero since");
        Expect(ThingStory.Line("quince tree", 1, 3) ==
            "Quince tree, chosen after stream 2. One stream has run past it since.", "one since");
        Expect(ThingStory.Line("quince tree", 0, 3) ==
            "Quince tree, chosen after stream 1. Two streams have run past it since.", "two since");
        Expect(ThingStory.Line("quince tree", 0, 5) ==
            "Quince tree, chosen after stream 1. Four streams have run past it since.", "four since");
        Expect(ThingStory.Line("quince tree", 0, 13) ==
            "Quince tree, chosen after stream 1. Twelve streams have run past it since.", "twelve since");
        Expect(ThingStory.Line("quince tree", 0, 14) ==
            "Quince tree, chosen after stream 1. 13 streams have run past it since.", "thirteen since uses digits");

        // Capitalisation: only the first letter changes.
        Expect(ThingStory.Line("scots pine", 0, 1).StartsWith("Scots pine, chosen", StringComparison.Ordinal),
            "first letter capitalised");
        Expect(ThingStory.Line("Sweet chestnut", 0, 1).StartsWith("Sweet chestnut,", StringComparison.Ordinal),
            "already capitalised name unchanged");

        // Invalid inputs give an empty line.
        Expect(ThingStory.Line(null, 0, 1) == "", "null name");
        Expect(ThingStory.Line("", 0, 1) == "", "empty name");
        Expect(ThingStory.Line("   ", 0, 1) == "", "blank name");
        Expect(ThingStory.Line("oak", -1, 1) == "", "negative index");
        Expect(ThingStory.Line("oak", 1, 1) == "", "index equal to choices made");
        Expect(ThingStory.Line("oak", 3, 1) == "", "index beyond choices made");

        // Deterministic and free of clock words.
        string first = ThingStory.Line("oak", 0, 4);
        Expect(first == ThingStory.Line("oak", 0, 4), "same inputs, same output");
        string[] banned = { "day", "days", "ago", "yesterday", "week", "weeks", "miss", "waiting" };
        for (int count = 1; count <= 20; count++)
            for (int at = 0; at < count; at++)
                foreach (string word in Words(ThingStory.Line("oak", at, count)))
                    foreach (string bad in banned)
                        Expect(word != bad, "no clock word '" + bad + "' in caption");

        // Puzzle-only and matched worlds never describe a choice.
        WorldModel puzzle = WorldModel.New(PlayMode.PuzzleOnly);
        Expect(ThingStory.For(puzzle, 0, "oak") == "", "puzzle-only world gives no caption");
        int[] schedule = new int[WorldModel.ScheduleLength];
        Array.Fill(schedule, 1);
        WorldModel matched = WorldModel.NewMatched(schedule);
        Expect(ThingStory.For(matched, 0, "oak") == "", "matched world gives no caption");
        Expect(ThingStory.For(null, 0, "oak") == "", "null world gives no caption");

        // A real living world: choose three things, each on its own board.
        WorldModel world = WorldModel.New(PlayMode.Living);
        VillageChoice[] picks = { VillageChoice.ShadeTree, VillageChoice.Bridge, VillageChoice.Flowers };
        for (int i = 0; i < picks.Length; i++)
        {
            Solve(world);
            Expect(world.Choose(picks[i]), "choice " + i + " accepted");
        }
        Expect(world.Choices.Length == 3, "three streams recorded");

        Expect(ThingStory.For(world, 3, "oak") == "", "index past the recorded choices gives no caption");
        Expect(ThingStory.For(world, -1, "oak") == "", "negative index gives no caption");

        for (int i = 0; i < picks.Length; i++)
        {
            string name = GardenCatalog.For(world.Garden, i, picks[i]).Name;
            string line = ThingStory.For(world, i, name);
            int since = picks.Length - 1 - i;
            string expectStream = "chosen after stream " + (i + 1) + ". ";
            Expect(line.Contains(expectStream), "stream number for index " + i);
            Expect(line.StartsWith(char.ToUpperInvariant(name[0]) + name.Substring(1) + ", ", StringComparison.Ordinal),
                "caption names the actual catalogue item for index " + i);
            Expect(line == ThingStory.Line(name, i, 3), "For equals Line for index " + i);
            if (since == 0) Expect(line.EndsWith("No streams have run past it yet.", StringComparison.Ordinal),
                "newest choice has no streams since");
            if (since == 2) Expect(line.EndsWith("Two streams have run past it since.", StringComparison.Ordinal),
                "oldest of three has two streams since");
        }

        // Determinism against the saved world: the same save gives the same captions.
        WorldModel reloaded = WorldModel.Restore(world.Export());
        for (int i = 0; i < picks.Length; i++)
        {
            string name = GardenCatalog.For(world.Garden, i, picks[i]).Name;
            Expect(ThingStory.For(reloaded, i, name) == ThingStory.For(world, i, name),
                "restored save gives the same caption for index " + i);
        }

        // Solved together: the sentence is appended only to the marked thing, and only in a living world.
        string plainOak = ThingStory.Line("oak", 0, 2);
        Expect(ThingStory.Line("oak", 0, 2, true) == plainOak + " You solved it together.", "together sentence appended exactly");
        Expect(ThingStory.Line("oak", 0, 2, false) == plainOak, "together false leaves the sentence unchanged");
        Expect(ThingStory.Line("oak", 5, 2, true) == "" && ThingStory.Line(null, 0, 2, true) == "",
            "together does not invent a line for invalid input");

        world.SolveTree(0);
        Expect(world.MarkTogether(0), "a solved grown thing can be marked together");
        Expect(ThingStory.For(world, 0, "oak") ==
            ThingStory.Line("oak", 0, 3) + " You solved it together.", "marked thing gets the together sentence");
        Expect(ThingStory.For(world, 1, "oak") == ThingStory.Line("oak", 1, 3), "unmarked thing keeps its sentence");
        Expect(!ThingStory.For(world, 1, "oak").Contains("together"), "no together sentence on unmarked thing");

        // Puzzle-only and matched worlds stay empty even with a together mark attempt (refused there).
        Expect(!puzzle.MarkTogether(0) && ThingStory.For(puzzle, 0, "oak") == "", "puzzle-only: no together caption");

        // Reconsidering the latest choice removes the mark and the sentence with it.
        WorldModel undo = WorldModel.New(PlayMode.Living);
        Solve(undo);
        Expect(undo.Choose(VillageChoice.Bridge), "undo: first choice accepted");
        undo.SolveTree(0);
        Expect(undo.MarkTogether(0), "undo: thing marked together");
        Expect(ThingStory.For(undo, 0, "bridge").EndsWith(" You solved it together.", StringComparison.Ordinal),
            "undo: sentence present before reconsider");
        Expect(undo.ReconsiderLatestChoice(), "undo: latest choice reconsidered");
        Expect(undo.TogetherCount == 0 && ThingStory.For(undo, 0, "bridge") == "",
            "undo: mark and sentence gone after reconsider");

        // A together mark survives a save and reload.
        WorldModel togetherSaved = WorldModel.Restore(world.Export());
        Expect(ThingStory.For(togetherSaved, 0, "oak") == ThingStory.For(world, 0, "oak"),
            "together sentence survives a save/load round trip");

        // Nursery plants: the pure sentence, with each clause only where its fact holds.
        Expect(ThingStory.NurseryLine("quince tree", false, false) == "Quince tree, from the nursery.", "nursery, not grown");
        Expect(ThingStory.NurseryLine("quince tree", true, false) ==
            "Quince tree, from the nursery. You grew it from its puzzle.", "nursery, grown");
        Expect(ThingStory.NurseryLine("quince tree", true, true) ==
            "Quince tree, from the nursery. You grew it from its puzzle. You solved it together.", "nursery, grown and together");
        Expect(ThingStory.NurseryLine("quince tree", false, true) == "Quince tree, from the nursery.",
            "together without grown adds no clause");
        Expect(ThingStory.NurseryLine(null, true, true) == "" && ThingStory.NurseryLine("  ", false, false) == "",
            "blank nursery name gives no line");

        // Basket plantings: exact sentence, blank and out-of-range give "".
        Expect(ThingStory.PlantingLine("quince tree", 2, 3) ==
            "Planted from your basket. It came from the quince tree you chose after stream 3.", "planting line");
        Expect(ThingStory.PlantingLine(" oak ", 0, 1) ==
            "Planted from your basket. It came from the oak you chose after stream 1.", "planting name trimmed");
        Expect(ThingStory.PlantingLine("", 0, 1) == "" && ThingStory.PlantingLine(null, 0, 1) == "",
            "blank planting name gives no line");
        Expect(ThingStory.PlantingLine("oak", -1, 1) == "" && ThingStory.PlantingLine("oak", 1, 1) == "",
            "planting index out of range gives no line");

        // ForNursery / ForPlanting on a real living save with bought and planted things.
        Expect(ThingStory.ForNursery(null, 0, "oak") == "" && ThingStory.ForPlanting(null, 0, "oak") == "",
            "null world gives no nursery or planting caption");
        WorldSnapshot shelf = world.Export();
        shelf.bought = new[] { 0, 40, 1, 41 };
        shelf.boughtSolved = new[] { 0 };
        shelf.together = new[] { WorldModel.BoughtBase + 0 };
        shelf.plantings = new[] { 0, 42, 2, 43 };
        Expect(WorldModel.TryRestore(shelf, out WorldModel stocked), "snapshot with bought and planted things restores");
        string nurseryName = Nursery.Item(0).Name;
        Expect(ThingStory.ForNursery(stocked, 0, nurseryName) ==
            ThingStory.NurseryLine(nurseryName, true, true), "solved together bought plant gets the full line");
        Expect(ThingStory.ForNursery(stocked, 1, Nursery.Item(1).Name) ==
            ThingStory.NurseryLine(Nursery.Item(1).Name, false, false), "unsolved bought plant gets the plain line");
        Expect(ThingStory.ForNursery(stocked, 2, nurseryName) == "" && ThingStory.ForNursery(stocked, -1, nurseryName) == "",
            "bought index out of range gives no caption");
        Expect(ThingStory.ForPlanting(stocked, 0, "quince tree") ==
            ThingStory.PlantingLine("quince tree", 0, 3), "planting 0 sourced from choice 0");
        Expect(ThingStory.ForPlanting(stocked, 1, "scots pine") ==
            "Planted from your basket. It came from the scots pine you chose after stream 3.",
            "planting 1 sourced from choice 2");
        Expect(ThingStory.ForPlanting(stocked, 2, "oak") == "" && ThingStory.ForPlanting(stocked, -1, "oak") == "",
            "planting index out of range gives no caption");

        // Unity map: the caption is used, and the family line stays as the fallback.
        string map = ReadRepoFile("games/evergrove/Assets/Scripts/Unity/GardenMapView.cs");
        Expect(map != null, "GardenMapView.cs can be found");
        if (map != null)
        {
            Expect(map.Contains("string.IsNullOrEmpty(item.Caption) ? LocalizedVillage.Choice(item.Family) : item.Caption"),
                "long press falls back to LocalizedVillage.Choice when the caption is empty");
        }
        string appMap = ReadRepoFile("games/evergrove/Assets/Scripts/Unity/EvergroveApp.Map.cs");
        Expect(appMap != null, "EvergroveApp.Map.cs can be found");
        if (appMap != null)
            Expect(appMap.Contains("ThingStory.For(world, i,"), "map items use ThingStory.For for chosen things");

        return assertions;
    }

    private static string[] Words(string text)
    {
        return text.ToLowerInvariant().Split(new[] { ' ', ',', '.' }, StringSplitOptions.RemoveEmptyEntries);
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
