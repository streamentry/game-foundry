using System;
using System.Collections.Generic;
using Evergrove.Core;

/// <summary>
/// Checklist 33: "How my garden grew". The almanac page lists the pairings a garden has found,
/// in definition order, and must read back byte-identical after a save or garden-file round trip.
/// </summary>
internal static class GardenGrewTests
{
    private const string Title = "How my garden grew";
    private static int assertions;

    private static void Expect(bool ok, string message)
    {
        assertions++;
        if (!ok) throw new Exception("GardenGrewTests failed: " + message);
    }

    // Found by discovering every pairing the layout makes, as PairingTests does.
    private static int DiscoverAll(WorldModel world, int found)
    {
        int paid = 0;
        foreach (Pairing pairing in Enum.GetValues(typeof(Pairing)))
            if ((found & PairingMask.Of(pairing)) != 0) paid += world.DiscoverPairing(pairing) > 0 ? 1 : 0;
        return paid;
    }

    private static string Lines(params string[] lines) { return string.Join("\n", lines); }

    private static string Closing(int found)
    {
        return found + " of " + Enum.GetValues(typeof(Pairing)).Length +
            (found == 1 ? " good-neighbour pairing found." : " good-neighbour pairings found.");
    }

    public static int Run()
    {
        var map = new GardenMap(5, 3, "..~..\n.....\n.....");
        // Same cells as PairingTests: row 0: 0 1 [2 water] 3 4 ; row 1: 5..9 ; row 2: 10..14
        var tree = new PlacedThing(6, VillageChoice.ShadeTree, "tree-01");
        var flower = new PlacedThing(7, VillageChoice.Flowers, "flower-03");
        var bridge = new PlacedThing(1, VillageChoice.Bridge, "build-00");
        var flower2 = new PlacedThing(8, VillageChoice.Flowers, "flower-09");
        var tree2 = new PlacedThing(11, VillageChoice.ShadeTree, "tree-05");

        // Empty garden: the title and the empty-state line, and no count.
        WorldModel empty = WorldModel.New(PlayMode.Living);
        Expect(GardenGrew.Page(empty) == Lines(Title, GardenGrew.EmptyLine), "empty garden gives the empty-state line");
        Expect(GardenGrew.Page(WorldModel.New(PlayMode.PuzzleOnly)) == Lines(Title, GardenGrew.EmptyLine),
            "a puzzle-only garden has no pairings to show");

        // A garden that has found pairings from its layout: the first four and two deeper ones.
        WorldModel world = WorldModel.New(PlayMode.Living);
        int found = Pairings.Between(map, new[] { tree, flower, bridge, flower2, tree2 });
        Expect(DiscoverAll(world, found) == 6, "the layout makes six pairings, each paid once");
        Expect(world.HasPairing(Pairing.Sheltered) && world.HasPairing(Pairing.ByTheWater) && world.HasPairing(Pairing.FlowerBed)
            && world.HasPairing(Pairing.Grove) && world.HasPairing(Pairing.Waterside) && world.HasPairing(Pairing.ShadySeat)
            && !world.HasPairing(Pairing.Orchard), "the found set is the one the layout makes");
        string expected = Lines(Title,
            Pairings.Hint(Pairing.Sheltered), Pairings.Hint(Pairing.ByTheWater), Pairings.Hint(Pairing.FlowerBed),
            Pairings.Hint(Pairing.Grove), Pairings.Hint(Pairing.Waterside), Pairings.Hint(Pairing.ShadySeat), Closing(6));
        string page = GardenGrew.Page(world);
        Expect(page == expected, "the page lists exactly the found pairings, in definition order, then the count");
        Expect(GardenGrew.Page(world) == page, "the page is deterministic");

        // The garden's name goes in the title.
        Expect(world.SetGardenName("Mill Field"), "a garden name is accepted");
        Expect(GardenGrew.Page(world).StartsWith("How Mill Field grew\n", StringComparison.Ordinal), "a named garden uses its name");
        string named = GardenGrew.Page(world);

        // Byte-identical after a save round trip (Export / TryRestore) and a garden file (GardenTransfer).
        Expect(WorldModel.TryRestore(world.Export(), out WorldModel restored), "the save restores");
        Expect(GardenGrew.Page(restored) == named, "the page is byte-identical after Export / TryRestore");
        string file = GardenTransfer.Export(world, "2026-11-27T00:00:00Z");
        Expect(GardenTransfer.TryImport(file, out WorldModel imported, out GardenImportError error) && error == GardenImportError.None,
            "the garden file imports");
        Expect(GardenGrew.Page(imported) == named, "the page is byte-identical after a garden-file round trip");

        // Closing count: one pairing reads in the singular; seven fills the page exactly.
        WorldModel one = WorldModel.New(PlayMode.Living);
        one.DiscoverPairing(Pairing.Grove);
        Expect(GardenGrew.Page(one) == Lines(Title, Pairings.Hint(Pairing.Grove), Closing(1)), "one pairing: singular count");
        WorldModel all = WorldModel.New(PlayMode.Living);
        foreach (Pairing pairing in Enum.GetValues(typeof(Pairing))) all.DiscoverPairing(pairing);
        string allPage = GardenGrew.Page(all);
        string[] allLines = allPage.Split('\n');
        Expect(allLines.Length == GardenGrew.MaxLines, "all seven found fills the page, and no more");
        Expect(allLines[allLines.Length - 1] == Closing(7), "all seven: the closing count");

        // Wording and length rules hold for every line of every page built here.
        foreach (string text in new[] { page, named, allPage, GardenGrew.Page(empty) })
            foreach (string line in text.Split('\n'))
            {
                Expect(line.Length <= GardenGrew.MaxLineLength, "lines fit in 80 characters: " + line);
                Expect(!line.Contains("!"), "no exclamation marks: " + line);
                Expect(!System.Text.RegularExpressions.Regex.IsMatch(line,
                    "lost|dead|died|wither|fail|sick|health|memory|brain|heal", System.Text.RegularExpressions.RegexOptions.IgnoreCase),
                    "no loss or health words: " + line);
            }
        Expect(allLines.Length <= GardenGrew.MaxLines && page.Split('\n').Length <= GardenGrew.MaxLines, "at most nine lines");
        return assertions;
    }
}
