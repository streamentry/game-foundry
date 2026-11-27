using System;
using System.Linq;
using Evergrove.Core;

/// <summary>
/// Free decorations: stepping stones, a bench, a fence, a lantern post. They cost and create
/// nothing, stand only on free land, travel in the garden file, and keep later plants off their cells.
/// </summary>
internal static class DecorTests
{
    private static int assertions;
    private const string Stamp = "2026-11-27T12:00:00Z";

    private static void Expect(bool ok, string message)
    {
        assertions++;
        if (!ok) throw new Exception("Decor test failed: " + message);
    }

    /// <summary>12 columns by 10 rows: a stream at row 1 (cells 13-15), a cottage at cell 51, the rest land.</summary>
    private static GardenMap Map()
    {
        string[] rows = new string[10];
        for (int r = 0; r < 10; r++) rows[r] = "............";
        rows[1] = ".~~~........";
        rows[4] = "...#........";
        return new GardenMap(12, 10, string.Concat(rows));
    }

    private static WorldModel Grown(int count, int seeds = 0)
    {
        WorldModel world = WorldModel.New(PlayMode.Living);
        for (int chapter = 0; chapter < count; chapter++)
        {
            Puzzle board = world.CurrentPuzzle;
            for (int i = 0; i < board.CellCount; i++)
                while (world.Rotations[i] != board.Solution[i]) world.Rotate(i);
            world.Choose((VillageChoice)(chapter % 3));
        }
        if (seeds == 0) return world;
        WorldSnapshot s = world.Export();
        s.seeds = seeds;
        Expect(WorldModel.TryRestore(s, out world), "rich garden restores");
        return world;
    }

    private static string State(WorldModel w) { return GardenTransfer.SnapshotJson(w.Export()); }
    private static WorldSnapshot Tweak(WorldSnapshot s, Action<WorldSnapshot> change) { change(s); return s; }

    /// <summary>A land cell that nothing stands on in this garden.</summary>
    private static int FreeCell(WorldModel w, GardenMap map, int skip = 0)
    {
        int[] occupied = w.Occupied;
        int[] layout = map.Layout(w.Choices, w.Placements, occupied);
        for (int cell = 0; cell < map.CellCount; cell++)
            if (map.IsFree(layout, occupied, cell) && skip-- <= 0) return cell;
        throw new Exception("no free cell");
    }

    public static int Run()
    {
        Constants();
        Placing();
        Refusals();
        Removing();
        Moving();
        PlantsAvoidDecor();
        StoryThingsAvoidDecor();
        Cap();
        Saving();
        FileTravel();
        return assertions;
    }

    private static void Constants()
    {
        Expect((int)Decoration.Stones == 0 && (int)Decoration.Bench == 1 && (int)Decoration.Fence == 2 && (int)Decoration.LanternPost == 3, "four decorations, in order");
        Expect(WorldModel.MaxDecor == 60, "at most sixty pieces");
        WorldModel w = WorldModel.New(PlayMode.Living);
        Expect(w.Decor.Length == 0 && w.DecorCount == 0, "a new garden has none");
    }

    private static void Placing()
    {
        GardenMap map = Map();
        WorldModel w = Grown(3, 50);
        w.Buy(0, 100);
        int seeds = w.Seeds, fruit = w.Fruit, jam = w.Jam, bought = w.BoughtCount;
        string[] letters = w.Letters;
        int a = FreeCell(w, map), b = FreeCell(w, map, 5);
        Expect(a != b, "two different free cells");
        Expect(w.PlaceDecor(Decoration.Bench, a, map), "a bench goes on a free cell");
        Expect(w.DecorCount == 1 && w.Decor.SequenceEqual(new[] { 1, a }), "stored as (kind, cell)");
        Expect(w.PlaceDecor(Decoration.LanternPost, b, map) && w.Decor.SequenceEqual(new[] { 1, a, 3, b }), "a second piece is appended");
        Expect(w.Seeds == seeds && w.Fruit == fruit && w.Jam == jam && w.BoughtCount == bought && w.Letters.SequenceEqual(letters),
            "it costs and creates nothing");
        foreach (Decoration kind in new[] { Decoration.Stones, Decoration.Fence })
            Expect(w.PlaceDecor(kind, FreeCell(w, map), map), kind + " goes down");
        Expect(w.Decor.Length == 8 && w.Seeds == seeds, "four pieces, still free");
        int[] copy = w.Decor;
        copy[0] = 9;
        Expect(w.Decor[0] == 1, "Decor is a copy");
    }

    private static void Refusals()
    {
        GardenMap map = Map();
        WorldModel w = Grown(3, 50);
        Expect(w.Buy(0, FreeCell(w, map)), "buy a plant");
        Expect(w.Plant(0, FreeCell(w, map)), "plant a seedling");
        string before = State(w);
        int water = 13;
        Expect(map.At(water) == GardenMap.Water, "map setup");
        int built = Enumerable.Range(0, map.CellCount).First(c => map.At(c) == GardenMap.Built);
        Expect(!w.PlaceDecor(Decoration.Bench, water, map), "not on water");
        Expect(!w.PlaceDecor(Decoration.Bench, built, map), "not on a building");
        Expect(!w.PlaceDecor(Decoration.Bench, -1, map) && !w.PlaceDecor(Decoration.Bench, map.CellCount, map) &&
               !w.PlaceDecor(Decoration.Bench, WorldModel.MaxMapCell, map), "not outside the map");
        int[] layout = map.Layout(w.Choices, w.Placements, w.Occupied);
        foreach (int storyCell in layout)
            Expect(!w.PlaceDecor(Decoration.Bench, storyCell, map), "not on a story thing at " + storyCell);
        int planting = w.Plantings[1], boughtCell = w.Bought[1];
        Expect(!w.PlaceDecor(Decoration.Fence, planting, map), "not on a planting");
        Expect(!w.PlaceDecor(Decoration.Fence, boughtCell, map), "not on a bought plant");
        Expect(!w.PlaceDecor(Decoration.Stones, 0, null), "a map is required");
        Expect(!w.PlaceDecor((Decoration)4, FreeCell(w, map), map) && !w.PlaceDecor((Decoration)(-1), FreeCell(w, map), map), "only the four kinds");
        Expect(State(w) == before, "refusals change nothing");

        int free = FreeCell(w, map);
        Expect(w.PlaceDecor(Decoration.Stones, free, map), "a free cell works");
        string withOne = State(w);
        Expect(!w.PlaceDecor(Decoration.Bench, free, map) && !w.PlaceDecor(Decoration.Stones, free, map), "not on another decoration");
        Expect(State(w) == withOne, "still unchanged");

        // A story thing the player put by hand blocks too.
        WorldModel placed = Grown(3);
        int spot = FreeCell(placed, map, 7);
        Expect(placed.PlaceAt(0, spot), "a thing is put by hand");
        Expect(!placed.PlaceDecor(Decoration.Bench, spot, map), "not on a thing placed by hand");

        Expect(!WorldModel.New(PlayMode.PuzzleOnly).PlaceDecor(Decoration.Bench, 0, map), "only a living garden decorates");
    }

    private static void Removing()
    {
        GardenMap map = Map();
        WorldModel w = Grown(2);
        int a = FreeCell(w, map), b = FreeCell(w, map, 3), c = FreeCell(w, map, 6);
        w.PlaceDecor(Decoration.Stones, a, map);
        w.PlaceDecor(Decoration.Bench, b, map);
        w.PlaceDecor(Decoration.Fence, c, map);
        string before = State(w);
        Expect(!w.RemoveDecor(-1) && !w.RemoveDecor(3) && !w.RemoveDecor(100) && State(w) == before, "a bad index removes nothing");
        Expect(w.RemoveDecor(1) && w.Decor.SequenceEqual(new[] { 0, a, 2, c }), "the middle piece goes, the others keep order");
        Expect(w.PlaceDecor(Decoration.LanternPost, b, map), "its cell is free again");
        Expect(w.RemoveDecor(0) && w.RemoveDecor(0) && w.RemoveDecor(0) && w.DecorCount == 0, "all removed");
        Expect(!w.RemoveDecor(0), "nothing left to remove");
    }

    private static void Moving()
    {
        GardenMap map = Map();
        WorldModel w = Grown(2, 50);
        w.Buy(0, FreeCell(w, map));
        int a = FreeCell(w, map), b = FreeCell(w, map, 4), c = FreeCell(w, map, 8);
        w.PlaceDecor(Decoration.Stones, a, map);
        w.PlaceDecor(Decoration.Bench, b, map);
        Expect(w.MoveDecor(0, c, map) && w.Decor.SequenceEqual(new[] { 0, c, 1, b }), "a piece moves to a free cell, keeping its kind");
        string before = State(w);
        Expect(!w.MoveDecor(0, b, map), "not onto another piece");
        Expect(!w.MoveDecor(0, 13, map), "not onto water");
        Expect(!w.MoveDecor(0, -1, map) && !w.MoveDecor(0, map.CellCount, map), "not off the map");
        Expect(!w.MoveDecor(0, w.Bought[1], map), "not onto a bought plant");
        Expect(!w.MoveDecor(0, map.Layout(w.Choices, w.Placements, w.Occupied)[0], map), "not onto a story thing");
        Expect(!w.MoveDecor(5, a, map) && !w.MoveDecor(-1, a, map), "bad index");
        Expect(!w.MoveDecor(0, a, null), "a map is required");
        Expect(State(w) == before, "refusals change nothing");
        Expect(w.MoveDecor(0, c, map) && State(w) == before, "moving onto its own cell is a harmless no-op");
        Expect(w.MoveDecor(1, a, map) && w.Decor[3] == a, "the vacated cell can be taken");
    }

    private static void PlantsAvoidDecor()
    {
        GardenMap map = Map();
        WorldModel w = Grown(3, 500);
        int cell = FreeCell(w, map);
        Expect(w.PlaceDecor(Decoration.Bench, cell, map), "a bench");
        int other = FreeCell(w, map);
        Expect(other != cell, "another free cell");

        string before = State(w);
        Expect(!w.Plant(0, cell), "Plant refuses a decorated cell");
        Expect(!w.Buy(0, cell), "Buy refuses a decorated cell");
        Expect(State(w) == before && w.Seeds == 500, "and costs nothing");
        Expect(w.Plant(0, other), "Plant still works elsewhere");
        Expect(w.Buy(0, FreeCell(w, map)), "Buy still works elsewhere");

        Expect(!w.MovePlanting(0, cell), "MovePlanting refuses a decorated cell");
        Expect(!w.MoveBought(0, cell), "MoveBought refuses a decorated cell");
        Expect(w.Plantings[1] == other && w.Bought[1] != cell, "the plants stayed put");
        Expect(w.MovePlanting(0, FreeCell(w, map)) && w.MoveBought(0, FreeCell(w, map)), "moves elsewhere still work");

        Expect(w.ReceiveNeighbourCutting(0), "a cutting arrives");
        int gifts = w.Gifts.Length, boughtCount = w.BoughtCount;
        Expect(!w.PlantGift(0, cell), "PlantGift refuses a decorated cell");
        Expect(w.Gifts.Length == gifts && w.BoughtCount == boughtCount, "the cutting stays in the basket");
        Expect(w.PlantGift(0, FreeCell(w, map)) && w.Gifts.Length == gifts - 1, "and plants fine elsewhere");

        Expect(!w.PlaceAt(0, cell), "a story thing is not set down on a decorated cell either");
        Expect(w.PlaceAt(0, FreeCell(w, map)), "but anywhere else");

        Expect(w.RemoveDecor(0) && w.Plant(0, cell), "once the bench is gone the cell is free for a plant");
        int[] occupied = w.Occupied;
        Expect(occupied.Length == (w.PlantingCount + w.BoughtCount) * 2 && w.Decor.Length == 0, "Occupied lists plantings and bought plants");
        w.PlaceDecor(Decoration.Fence, FreeCell(w, map), map);
        Expect(w.Occupied.Length == occupied.Length + 2, "and decorations");
    }

    private static void StoryThingsAvoidDecor()
    {
        GardenMap map = Map();
        WorldModel w = WorldModel.New(PlayMode.Living);
        int wanted = map.DefaultCell(new int[0], VillageChoice.ShadeTree, 0);
        Expect(wanted >= 0 && w.PlaceDecor(Decoration.Stones, wanted, map), "stones go where the first tree would stand");
        Puzzle board = w.CurrentPuzzle;
        for (int i = 0; i < board.CellCount; i++)
            while (w.Rotations[i] != board.Solution[i]) w.Rotate(i);
        Expect(w.Choose(VillageChoice.ShadeTree), "the tree is chosen");
        int[] layout = map.Layout(w.Choices, w.Placements, w.Occupied);
        Expect(layout[0] >= 0 && layout[0] != wanted, "fed Occupied, the layout moves the tree off the stones");
    }

    private static void Cap()
    {
        GardenMap map = Map();
        WorldModel w = Grown(2);
        for (int i = 0; i < WorldModel.MaxDecor; i++)
            Expect(w.PlaceDecor((Decoration)(i % 4), FreeCell(w, map), map), "piece " + i + " goes down");
        Expect(w.DecorCount == 60 && w.Decor.Length == 120, "sixty pieces");
        string before = State(w);
        Expect(!w.PlaceDecor(Decoration.Bench, FreeCell(w, map), map) && State(w) == before, "the sixty-first is refused");
        Expect(w.RemoveDecor(0) && w.PlaceDecor(Decoration.Bench, FreeCell(w, map), map), "room again after removing one");
        Expect(Restored(w).DecorCount == 60, "a full set saves");
    }

    private static WorldModel Restored(WorldModel w)
    {
        Expect(WorldModel.TryRestore(w.Export(), out WorldModel back), "restores");
        return back;
    }

    private static void Saving()
    {
        GardenMap map = Map();
        WorldModel w = Grown(3, 80);
        w.Buy(0, FreeCell(w, map));
        w.Plant(1, FreeCell(w, map));
        w.PlaceDecor(Decoration.Bench, FreeCell(w, map), map);
        w.PlaceDecor(Decoration.LanternPost, FreeCell(w, map), map);
        WorldModel back = Restored(w);
        Expect(back.Decor.SequenceEqual(w.Decor) && State(back) == State(w), "decorations survive a save");
        Expect(w.Export().decor.SequenceEqual(w.Decor), "the snapshot carries them");
        int plant = w.Plantings[1], bought = w.Bought[1];
        Expect(!WorldModel.TryRestore(Tweak(w.Export(), s => s.decor = new[] { 0, 5, 1 }), out _), "an odd length refused");
        Expect(!WorldModel.TryRestore(Tweak(w.Export(), s => s.decor = new[] { 4, 5 }), out _), "kind 4 refused");
        Expect(!WorldModel.TryRestore(Tweak(w.Export(), s => s.decor = new[] { -1, 5 }), out _), "negative kind refused");
        Expect(!WorldModel.TryRestore(Tweak(w.Export(), s => s.decor = new[] { 0, -1 }), out _), "negative cell refused");
        Expect(!WorldModel.TryRestore(Tweak(w.Export(), s => s.decor = new[] { 0, WorldModel.MaxMapCell }), out _), "a cell past the map limit refused");
        Expect(WorldModel.TryRestore(Tweak(w.Export(), s => s.decor = new[] { 0, WorldModel.MaxMapCell - 1 }), out _), "the last cell is fine");
        Expect(!WorldModel.TryRestore(Tweak(w.Export(), s => s.decor = new[] { 0, 5, 1, 5 }), out _), "two pieces on one cell refused");
        Expect(!WorldModel.TryRestore(Tweak(w.Export(), s => s.decor = new[] { 0, plant }), out _), "a piece on a planting refused");
        Expect(!WorldModel.TryRestore(Tweak(w.Export(), s => s.decor = new[] { 0, bought }), out _), "a piece on a bought plant refused");
        Expect(!WorldModel.TryRestore(Tweak(w.Export(), s => s.decor = Enumerable.Range(0, 61).SelectMany(i => new[] { 0, 1000 + i }).ToArray()), out _), "sixty-one refused");
        Expect(WorldModel.TryRestore(Tweak(w.Export(), s => s.decor = Enumerable.Range(0, 60).SelectMany(i => new[] { i % 4, 1000 + i }).ToArray()), out WorldModel sixty) && sixty.DecorCount == 60, "sixty accepted");
        Expect(WorldModel.TryRestore(Tweak(w.Export(), s => s.decor = null), out WorldModel bare) && bare.Decor.Length == 0, "null restores as none");
        Expect(!WorldModel.TryRestore(Tweak(WorldModel.New(PlayMode.PuzzleOnly).Export(), s => s.decor = new[] { 0, 5 }), out _), "puzzle-only holding any refused");
    }

    private static string Envelope(string snapshot)
    {
        return "{\"format\":\"mosselora-garden\",\"formatVersion\":1,\"exportedAtUtc\":\"" + Stamp + "\",\"checksumSha256\":\"" +
            GardenTransfer.Sha256Hex(snapshot) + "\",\"snapshot\":" + snapshot + "}";
    }

    private static void FileTravel()
    {
        GardenMap map = Map();
        WorldModel w = Grown(3, 80);
        w.Buy(0, FreeCell(w, map));
        w.PlaceDecor(Decoration.Bench, FreeCell(w, map), map);
        w.PlaceDecor(Decoration.Fence, FreeCell(w, map), map);
        string file = GardenTransfer.Export(w, Stamp);
        string expected = "\"decor\":[" + string.Join(",", w.Decor) + "]";
        Expect(file.Contains(expected), "the garden file carries the decorations");
        Expect(GardenTransfer.TryImport(file, out WorldModel copy, out GardenImportError error) && copy.Decor.SequenceEqual(w.Decor), "a copied garden shows them (" + error + ")");
        Expect(copy.Plantings.SequenceEqual(w.Plantings) && copy.Bought.SequenceEqual(w.Bought) && copy.Placements.SequenceEqual(w.Placements), "with everything else as it was");
        Expect(GardenTransfer.Export(copy, Stamp) == file, "byte-identical export");
        int[] seen = map.Layout(copy.Choices, copy.Placements, copy.Occupied);
        Expect(seen.SequenceEqual(map.Layout(w.Choices, w.Placements, w.Occupied)), "the relative's map lays out the same way");
        Expect(!copy.PlaceDecor(Decoration.Bench, copy.Decor[1], map), "and the visitor's copy refuses those cells too");
        Expect(!GardenTransfer.TryImport(file.Replace(expected, "\"decor\":[0,77]"), out _, out error) && error == GardenImportError.ChecksumMismatch, "edited decorations fail the checksum");
        Expect(!GardenTransfer.TryImport(file.Replace(expected, "\"decor\":3"), out _, out _), "wrong type refused");
        Expect(!GardenTransfer.TryImport(file.Replace("}}", ",\"decorz\":[]}}"), out _, out _), "an unknown key is refused");

        // Order: jam, then letter stamps, then decorations.
        WorldSnapshot s = w.Export();
        s.jam = 2;
        s.letters = new[] { "Hello" };
        s.letterFrom = new[] { "Nan" };
        s.letterStamps = new[] { 5 };
        Expect(WorldModel.TryRestore(s, out WorldModel all), "a garden with every newer key restores");
        string full = GardenTransfer.Export(all, Stamp);
        Expect(full.Contains("\"jam\":2,\"letterStamps\":[5]," + expected), "keys in order");
        Expect(GardenTransfer.TryImport(full, out WorldModel round, out error) && round.Jam == 2 && round.LetterStamps[0] == 5 && round.Decor.SequenceEqual(w.Decor), "all three round trip (" + error + ")");

        // No decorations, no key: older files are byte-identical and still import.
        WorldModel plainGarden = Grown(3, 80);
        string plain = GardenTransfer.Export(plainGarden, Stamp);
        Expect(!plain.Contains("decor"), "no decorations, no key");
        Expect(GardenTransfer.TryImport(plain, out WorldModel older, out error) && older.Decor.Length == 0, "an older file imports (" + error + ")");
        string snapshot = plain.Substring(plain.IndexOf("\"snapshot\":", StringComparison.Ordinal) + 11);
        snapshot = snapshot.Substring(0, snapshot.Length - 1);
        string body = snapshot.Substring(0, snapshot.Length - 1);
        int free = FreeCell(plainGarden, map);
        string smuggled = body + ",\"decor\":[0," + free + "]}";
        Expect(!GardenTransfer.TryImport("{\"format\":\"mosselora-garden\",\"formatVersion\":1,\"exportedAtUtc\":\"" + Stamp + "\",\"checksumSha256\":\"" +
            GardenTransfer.Sha256Hex(snapshot) + "\",\"snapshot\":" + smuggled + "}", out _, out _), "decorations cannot ride under the older checksum");
        Expect(GardenTransfer.TryImport(Envelope(smuggled), out WorldModel honest, out _) && honest.Decor.SequenceEqual(new[] { 0, free }), "a file that checksums its decorations is fine");
        Expect(!GardenTransfer.TryImport(Envelope(body + ",\"decor\":[]}"), out _, out _), "an empty decor key is not canonical");
    }
}
