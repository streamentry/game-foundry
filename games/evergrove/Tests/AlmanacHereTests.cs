using System;
using System.Text.Json;
using Evergrove.Core;

/// <summary>
/// The almanac's keeper line says "You chose it for this garden." only for items this garden's
/// own choices made (vision run 20261128-1901, idea 5). Pins that the overload equals the old
/// line everywhere else, that it never adds a negative sentence, and that it leaves the world
/// unchanged.
/// </summary>
internal static class AlmanacHereTests
{
    private static int assertions;
    private static readonly JsonSerializerOptions Options = new JsonSerializerOptions { IncludeFields = true };
    private const string Chosen = " You chose it for this garden.";

    private static void Expect(bool condition, string message)
    {
        assertions++;
        if (!condition) throw new Exception("AlmanacHereTests failed: " + message);
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

        // Fresh world: no choices, so the new overload equals the old one for every family.
        WorldModel fresh = WorldModel.New(PlayMode.Living);
        VillageChoice[] families = { VillageChoice.ShadeTree, VillageChoice.Bridge, VillageChoice.Flowers };
        foreach (VillageChoice family in families)
            for (int index = 0; index < 3; index++)
            {
                GardenItem item = GardenCatalog.For(fresh.Garden, index, family);
                Expect(Almanac.KeeperLine(item, fresh) == Almanac.KeeperLine(item),
                    "fresh world leaves the line of " + item.Art + " unchanged");
            }

        // Stream 1 solved and a real choice made: only the chosen family gains the sentence.
        WorldModel world = WorldModel.New(PlayMode.Living);
        Solve(world);
        Expect(world.Choose(VillageChoice.ShadeTree), "the real ShadeTree choice is accepted");
        Expect(world.Choices.Length == 1 && world.Choices[0] == (int)VillageChoice.ShadeTree,
            "one choice is recorded");

        GardenItem chosenItem = GardenCatalog.For(world.Garden, 0, VillageChoice.ShadeTree);
        string chosenLine = Almanac.KeeperLine(chosenItem, world);
        Expect(chosenLine.EndsWith(Chosen), "chosen item line ends with the sentence");
        Expect(chosenLine == Almanac.KeeperLine(chosenItem) + Chosen,
            "chosen item line is the old line plus exactly the sentence");

        foreach (VillageChoice other in new[] { VillageChoice.Bridge, VillageChoice.Flowers })
        {
            GardenItem notChosen = GardenCatalog.For(world.Garden, 0, other);
            Expect(notChosen.Art != chosenItem.Art, "stream 1 " + other + " item differs from the chosen one");
            string line = Almanac.KeeperLine(notChosen, world);
            Expect(!line.Contains("You chose it"), "unchosen " + other + " item gains no sentence");
            Expect(line == Almanac.KeeperLine(notChosen), "unchosen " + other + " item equals the old line");
        }

        // Null world and null item keep the old behaviour.
        GardenItem anyItem = GardenCatalog.For(world.Garden, 0, VillageChoice.Bridge);
        Expect(Almanac.KeeperLine(chosenItem, null) == Almanac.KeeperLine(chosenItem),
            "null world gives the old line");
        Expect(Almanac.KeeperLine(null, world) == "", "null item gives an empty line");
        Expect(Almanac.KeeperLine(null, null) == "", "null item and null world give an empty line");
        Expect(anyItem.Art.Length > 0, "fixture item has an art key");

        // Calling the overload never changes the world.
        string before = Snapshot(world);
        Almanac.KeeperLine(chosenItem, world);
        Almanac.KeeperLine(anyItem, world);
        Expect(Snapshot(world) == before, "the overload leaves the world snapshot byte-identical");

        Console.WriteLine("Evergrove almanac here tests: " + assertions + " assertions.");
        return assertions;
    }
}
