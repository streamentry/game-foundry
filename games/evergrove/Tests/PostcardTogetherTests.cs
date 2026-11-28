using System;
using System.Text.Json;
using Evergrove.Core;

/// <summary>
/// Vision run 20261128-2001, idea 3: the postcard names what the family solved together. Pins that a
/// garden without marks keeps its postcard, that names come from the real items, that the list is
/// shortened after three, and that building the postcard never changes the world.
/// </summary>
internal static class PostcardTogetherTests
{
    private static int assertions;
    private static readonly JsonSerializerOptions Options = new JsonSerializerOptions { IncludeFields = true };
    private const string Stream = "Stream 1 / 12";

    private static void Expect(bool condition, string message)
    {
        assertions++;
        if (!condition) throw new Exception("PostcardTogetherTests failed: " + message);
    }

    private static string Snapshot(WorldModel world)
    {
        return JsonSerializer.Serialize(world.Export(), Options);
    }

    /// <summary>Plays <paramref name="count"/> chapters through the real puzzle and choice APIs.</summary>
    private static WorldModel Grown(int count)
    {
        WorldModel world = WorldModel.New(PlayMode.Living);
        for (int chapter = 0; chapter < count; chapter++)
        {
            Puzzle board = world.CurrentPuzzle;
            for (int i = 0; i < board.CellCount; i++)
                while (world.Rotations[i] != board.Solution[i]) world.Rotate(i);
            Expect(world.Choose((VillageChoice)(chapter % 3)), "chapter " + chapter + " accepts its choice");
        }
        return world;
    }

    /// <summary>Grown things are real names only through the catalog, so the test asks the same catalog.</summary>
    private static string GrownName(WorldModel world, int index)
    {
        return GardenCatalog.For(world.Garden, Math.Min(index, GardenCatalog.ChaptersPerGarden - 1),
            (VillageChoice)world.Choices[index]).Name;
    }

    /// <summary>Six chapters with enough seeds to buy plants, restored through the save path.</summary>
    private static WorldModel RichGrown()
    {
        WorldModel world = Grown(6);
        WorldSnapshot snapshot = world.Export();
        snapshot.seeds = 500;
        Expect(WorldModel.TryRestore(snapshot, out world), "rich garden restores");
        return world;
    }

    public static int Run()
    {
        assertions = 0;

        // A fresh garden has no marks, so its postcard has no together line.
        WorldModel fresh = WorldModel.New(PlayMode.Living);
        string freshCard = GardenPostcard.Build(fresh, Stream);
        Expect(!freshCard.Contains("Solved together"), "fresh postcard has no together line");
        Expect(freshCard.EndsWith("  " + Stream + "\n"), "fresh postcard ends at the stream line");

        // One grown thing solved and marked: one name, the real item name.
        WorldModel one = Grown(1);
        one.SolveTree(0);
        Expect(one.MarkTogether(0), "one solved tree is marked");
        string oneCard = GardenPostcard.Build(one, Stream);
        string oneName = GrownName(one, 0);
        Expect(oneCard.EndsWith("\nSolved together: the " + oneName + ".\n"),
            "one mark names the real item after the stream line");
        Expect(oneCard.StartsWith("Mosselora\n"), "postcard still opens with the game name");

        // Two marks: "the A and the B", ascending mark order.
        WorldModel two = RichGrown();
        two.SolveTree(0);
        two.SolveTree(4);
        Expect(two.MarkTogether(0) && two.MarkTogether(4), "two solved trees are marked");
        string twoCard = GardenPostcard.Build(two, Stream);
        Expect(twoCard.EndsWith("\nSolved together: the " + GrownName(two, 0) + " and the " + GrownName(two, 4) + ".\n"),
            "two marks join with and");

        // A bought plant resolves through the nursery, and it is named as the plant.
        WorldModel bought = RichGrown();
        Expect(bought.Buy(2, 60), "a plant can be bought");
        bought.SolveBought(0);
        Expect(bought.BoughtSolved(0) && bought.MarkTogether(WorldModel.BoughtBase + 0),
            "a solved bought plant is marked");
        string boughtName = Nursery.Item(bought.Bought[0]).Name;
        Expect(GardenPostcard.Build(bought, Stream).EndsWith("\nSolved together: the " + boughtName + ".\n"),
            "a bought plant mark names the nursery item");

        // Five resolvable marks: three names, then "and 2 more."
        WorldModel five = RichGrown();
        for (int tree = 0; tree < 5; tree++) five.SolveTree(tree);
        for (int tree = 0; tree < 5; tree++) Expect(five.MarkTogether(tree), "tree " + tree + " is marked");
        string fiveCard = GardenPostcard.Build(five, Stream);
        Expect(fiveCard.EndsWith("\nSolved together: the " + GrownName(five, 0) + ", the " + GrownName(five, 1) +
            ", the " + GrownName(five, 2) + " and 2 more.\n"), "five marks show three names and 2 more");
        Expect(!fiveCard.Contains("3 more") && !fiveCard.Contains("1 more"), "the remaining count is exact");

        // Building the postcard never changes the save.
        string before = Snapshot(five);
        GardenPostcard.Build(five, Stream);
        GardenPostcard.Build(two, Stream);
        Expect(Snapshot(five) == before, "building the postcard leaves the world snapshot byte-identical");

        Console.WriteLine("Evergrove postcard together tests: " + assertions + " assertions.");
        return assertions;
    }
}
