using System;
using System.Linq;
using Evergrove.Core;

/// <summary>
/// Items 75 and 76: recipes found by cooking (the only recipe is the jam kitchen's, with no new stored food),
/// and the larder's shelves, bought with seeds, that grow the fruit and jam caps. Refusals change nothing,
/// a full larder still refuses parcels, and forged saves are refused.
/// </summary>
internal static class RecipeTests
{
    private static int assertions;
    private const string Stamp = "2026-11-27T12:00:00Z";
    private const int Price1 = 150;
    private const int Price2 = 300;
    private const int Price3 = 600;

    private static void Expect(bool ok, string message)
    {
        assertions++;
        if (!ok) throw new Exception("Recipe test failed: " + message);
    }

    private static WorldModel Garden(int fruit = 0, int jam = 0, int seeds = 0, int shelves = 0)
    {
        WorldSnapshot s = WorldModel.New(PlayMode.Living).Export();
        s.fruit = fruit;
        s.jam = jam;
        s.seeds = seeds;
        s.larderShelves = shelves;
        Expect(WorldModel.TryRestore(s, out WorldModel w), "garden restores (fruit " + fruit + ", jam " + jam + ", shelves " + shelves + ")");
        return w;
    }

    private static string State(WorldModel w) { return GardenTransfer.SnapshotJson(w.Export()); }
    private static WorldSnapshot Tweak(WorldSnapshot s, Action<WorldSnapshot> change) { change(s); return s; }
    private static bool Restores(WorldSnapshot s) { return WorldModel.TryRestore(s, out _); }

    private static string Parcel(ParcelGift gift, int amount, string nonce)
    {
        Expect(FamilyParcel.TryCreate("Nan", "", gift, amount, nonce, out string text, out _, out _), "parcel " + nonce + " is made");
        return text;
    }

    public static int Run()
    {
        CookingSpendsAndMakes();
        RefusalsChangeNothing();
        FindingAndSaving();
        AlmanacListsFound();
        RecipesStayAboveNoProfit();
        LarderShelvesCostSeeds();
        LarderCapsAndRefusals();
        LarderFullUsesNewCapacity();
        ForgedSavesRefused();
        return assertions;
    }

    private static void CookingSpendsAndMakes()
    {
        WorldModel w = Garden(fruit: 5);
        Expect(w.CanCookRecipe(Recipes.Jam) && w.CookRecipe(Recipes.Jam), "three fruit cook a jar");
        Expect(w.Fruit == 2 && w.Jam == 1, "the recipe spends exactly three fruit and makes one jar");
        Expect(w.Ledger.Length == 0, "cooking moves no seeds, so the ledger is untouched");
        Expect(!w.CanCookRecipe(Recipes.Jam) && !w.CookRecipe(Recipes.Jam), "two fruit are not enough again");

        WorldModel full = Garden(fruit: 3, jam: WorldModel.MaxJam);
        Expect(!full.CanCookRecipe(Recipes.Jam), "a full pantry cannot cook");
        Expect(Recipes.Name(Recipes.Jam) == "Jam" && Recipes.IsRecipe(Recipes.Jam) && !Recipes.IsRecipe(Recipes.Count),
            "the recipe table names its one recipe and rejects unknown ids");
    }

    private static void RefusalsChangeNothing()
    {
        WorldModel few = Garden(fruit: 2);
        string before = State(few);
        Expect(!few.CookRecipe(Recipes.Jam) && State(few) == before, "too little fruit: refused, nothing changes");

        WorldModel full = Garden(fruit: 9, jam: WorldModel.MaxJam);
        before = State(full);
        Expect(!full.CookRecipe(Recipes.Jam) && State(full) == before, "full pantry: refused, no fruit lost");

        WorldModel puzzle = WorldModel.New(PlayMode.PuzzleOnly);
        before = State(puzzle);
        Expect(!puzzle.CanCookRecipe(Recipes.Jam) && !puzzle.CookRecipe(Recipes.Jam) && State(puzzle) == before,
            "a puzzle-only garden cannot cook");

        WorldModel unknown = Garden(fruit: 9);
        before = State(unknown);
        Expect(!unknown.CookRecipe(Recipes.Count) && !unknown.CookRecipe(-1) && !unknown.CanCookRecipe(99) && State(unknown) == before,
            "an unknown recipe id is refused and changes nothing");
    }

    private static void FindingAndSaving()
    {
        WorldModel w = Garden(fruit: 3);
        Expect(!w.HasFound(Recipes.Jam) && Recipes.Found(w).Length == 0 && w.RecipesFound == 0, "nothing is found before cooking");
        string fresh = GardenTransfer.Export(Garden(), Stamp);
        Expect(!fresh.Contains("\"recipes\""), "an unfound recipe mask is not written");

        WorldModel refused = Garden(fruit: 2);
        refused.CookRecipe(Recipes.Jam);
        Expect(!refused.HasFound(Recipes.Jam), "a refused cook finds nothing");

        Expect(w.CookRecipe(Recipes.Jam), "the first cook succeeds");
        Expect(w.HasFound(Recipes.Jam) && w.RecipesFound == Recipes.Mask(Recipes.Jam), "the first cook finds the recipe");
        Expect(Recipes.Found(w).SequenceEqual(new[] { "Jam" }), "the found list names the jam recipe");

        WorldSnapshot snap = w.Export();
        Expect(WorldModel.TryRestore(snap, out WorldModel back) && back.HasFound(Recipes.Jam), "the find survives Export and TryRestore");
        string file = GardenTransfer.Export(w, Stamp);
        Expect(file.Contains("\"recipes\":1"), "the garden file carries the found mask");
        Expect(GardenTransfer.TryImport(file, out WorldModel imported, out GardenImportError error) && error == GardenImportError.None &&
            imported.HasFound(Recipes.Jam) && Recipes.Found(imported).SequenceEqual(new[] { "Jam" }),
            "the find survives the garden file");

        WorldModel second = Garden(fruit: 6);
        second.CookRecipe(Recipes.Jam);
        Expect(second.CookRecipe(Recipes.Jam) && second.HasFound(Recipes.Jam), "a second cook keeps the find");
    }

    private static void AlmanacListsFound()
    {
        // The almanac lists every found recipe by name: this is the list it reads.
        for (int id = 0; id < Recipes.Count; id++)
        {
            WorldModel w = Garden(fruit: 3);
            Expect(Recipes.Found(w).Contains(Recipes.Name(id)) == w.HasFound(id), "not yet cooked: the list agrees with HasFound");
            w.CookRecipe(id);
            Expect(Recipes.Found(w).Contains(Recipes.Name(id)), "a recipe once found is in the list");
        }
        Expect(!string.IsNullOrWhiteSpace(Recipes.Method(Recipes.Jam)) && !Recipes.Method(Recipes.Jam).Contains("health"),
            "the method reads plainly, with no health claim");
    }

    // Recipes use only fruit, jam and seeds, so the proof is that the one recipe is no better than its inputs,
    // in every season: three fruit are worth at least one jar.
    private static void RecipesStayAboveNoProfit()
    {
        foreach (Season season in new[] { Season.Spring, Season.Summer, Season.Autumn, Season.Winter })
            Expect(WorldModel.JamPerJar * Market.ValueOf(Good.Fruit, season) >= Market.ValueOf(Good.Jam, season),
                "three fruit are worth at least one jar in " + season);
    }

    private static void LarderShelvesCostSeeds()
    {
        WorldModel w = Garden(seeds: Price1 + Price2 + Price3);
        Expect(w.LarderShelves == 0 && w.FruitCapacity == WorldModel.MaxFruit && w.JamCapacity == WorldModel.MaxJam,
            "a new larder has the base caps");
        Expect(w.NextShelfPrice == Price1, "the first shelf costs 150");
        Expect(w.BuyShelf(), "a shelf is bought with enough seeds");
        Expect(w.LarderShelves == 1 && w.Seeds == Price2 + Price3, "the first shelf takes its price in seeds");
        Expect(w.Ledger.Length > 0 && w.Ledger[0].Reason == SeedReason.Larder && w.Ledger[0].Delta == -Price1,
            "the shelf is one Larder ledger line of minus its price");
        Expect(w.FruitCapacity == WorldModel.MaxFruit + 20 && w.JamCapacity == WorldModel.MaxJam + 5,
            "one shelf adds 20 fruit and 5 jars");
        Expect(w.BuyShelf() && w.Seeds == Price3, "the second shelf costs 300");
        Expect(w.BuyShelf() && w.Seeds == 0, "the third shelf costs 600");
        Expect((int)SeedReason.Larder == 12, "the Larder reason is appended as 12");
    }

    private static void LarderCapsAndRefusals()
    {
        WorldModel w = Garden(seeds: Price1 + Price2 + Price3 + 50, shelves: WorldModel.MaxShelves);
        string before = State(w);
        Expect(w.NextShelfPrice == -1 && !w.BuyShelf() && State(w) == before, "a full larder buys nothing more");
        Expect(w.FruitCapacity == WorldModel.MaxFruit + 60 && w.JamCapacity == WorldModel.MaxJam + 15,
            "three shelves give 159 fruit and 25 jars");

        WorldModel poor = Garden(seeds: Price1 - 1);
        before = State(poor);
        Expect(!poor.BuyShelf() && State(poor) == before, "a shelf is refused when the purse is short, nothing changes");

        WorldModel puzzle = WorldModel.New(PlayMode.PuzzleOnly);
        before = State(puzzle);
        Expect(puzzle.NextShelfPrice == -1 && !puzzle.BuyShelf() && State(puzzle) == before, "a puzzle-only garden buys no shelf");
    }

    private static void LarderFullUsesNewCapacity()
    {
        // Base fruit cap: 95 + 10 does not fit; with one shelf (119) it does, and 119 is the limit.
        string tenFruit = Parcel(ParcelGift.Fruit, 10, "rf-ten");
        WorldModel base0 = Garden(fruit: 95);
        string before = State(base0);
        ParcelOpenResult refused = base0.Open(tenFruit);
        Expect(!refused.Ok && refused.Error == ParcelError.LarderFull && State(base0) == before,
            "no shelf: ten more fruit are refused with LarderFull, nothing changes");

        WorldModel shelf = Garden(fruit: 95, shelves: 1);
        ParcelOpenResult opened = shelf.Open(tenFruit);
        Expect(opened.Ok && opened.FruitGained == 10 && shelf.Fruit == 105, "one shelf holds the parcel");

        WorldModel full = Garden(fruit: 109, shelves: 1);
        Expect(full.Open(Parcel(ParcelGift.Fruit, 10, "rf-fill")).Ok && full.Fruit == 119, "fruit fills to 119");
        ParcelOpenResult none = full.Open(Parcel(ParcelGift.Fruit, 1, "rf-over"));
        Expect(!none.Ok && none.Error == ParcelError.LarderFull && full.Fruit == 119, "a full larder refuses one more fruit");

        WorldModel jam = Garden(jam: 14, shelves: 1);
        Expect(jam.Open(Parcel(ParcelGift.Jam, 1, "rj-fits")).Ok && jam.Jam == 15, "a shelf holds the fifteenth jar");
        ParcelOpenResult noJar = jam.Open(Parcel(ParcelGift.Jam, 1, "rj-over"));
        Expect(!noJar.Ok && noJar.Error == ParcelError.LarderFull && jam.Jam == 15, "a full shelf of jam refuses another jar");
        Expect(!Garden(jam: 10).Open(Parcel(ParcelGift.Jam, 1, "rj-base")).Ok, "ten jars with no shelf refuse one more");

        // The kitchen uses the new jam cap too.
        WorldModel kitchen = Garden(fruit: 30, jam: 10, shelves: 1);
        Expect(kitchen.MakeJam() && kitchen.Jam == 11, "the kitchen cooks past ten jars on a shelf");
    }

    private static WorldSnapshot Base() { return Garden(fruit: 5).Export(); }

    private static void ForgedSavesRefused()
    {
        Expect(Restores(Tweak(Base(), s => s.fruit = WorldModel.MaxFruit)), "exactly the base fruit cap is accepted");
        Expect(!Restores(Tweak(Base(), s => s.fruit = WorldModel.MaxFruit + 1)), "fruit above the base cap, with no shelf, is refused");
        Expect(Restores(Tweak(Base(), s => { s.fruit = 119; s.larderShelves = 1; })), "fruit up to the shelf's cap is accepted");
        Expect(!Restores(Tweak(Base(), s => { s.fruit = 120; s.larderShelves = 1; })), "fruit above the shelf's cap is refused");

        Expect(Restores(Tweak(Base(), s => { s.jam = 25; s.larderShelves = 3; })), "jam up to three shelves is accepted");
        Expect(!Restores(Tweak(Base(), s => { s.jam = 26; s.larderShelves = 3; })), "jam above three shelves is refused");
        Expect(!Restores(Tweak(Base(), s => { s.jam = 11; s.larderShelves = 0; })), "jam above the base cap, with no shelf, is refused");

        Expect(!Restores(Tweak(Base(), s => s.larderShelves = WorldModel.MaxShelves + 1)), "more than three shelves is refused");
        Expect(!Restores(Tweak(Base(), s => s.larderShelves = -1)), "a negative shelf count is refused");

        WorldSnapshot puzzle = WorldModel.New(PlayMode.PuzzleOnly).Export();
        Expect(!Restores(Tweak(puzzle, s => s.larderShelves = 1)), "shelves in a puzzle-only garden are refused");
        Expect(!Restores(Tweak(puzzle, s => s.recipes = 1)), "a found recipe in a puzzle-only garden is refused");

        Expect(Restores(Tweak(Base(), s => s.recipes = Recipes.AllMask)), "the defined recipe bits are accepted");
        Expect(!Restores(Tweak(Base(), s => s.recipes = Recipes.AllMask + 1)), "an unknown recipe bit is refused");
        Expect(!Restores(Tweak(Base(), s => s.recipes = -1)), "a negative recipe mask is refused");
    }
}
