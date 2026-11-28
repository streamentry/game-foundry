using System;
using System.IO;
using Evergrove.Core;

/// <summary>
/// I338–I342 (run 3): seasons you can feel. How each thing looks through the year, true to
/// the catalogue: trees blossom, leaf, turn and rest; a flower blooms in its own season
/// and rests in winter unless winter is its season.
/// </summary>
internal static class SeasonLookTests
{
    private static int assertions;

    private static void Expect(bool ok, string message)
    {
        assertions++;
        if (!ok) throw new Exception("Season look test failed: " + message);
    }

    public static int Run()
    {
        // Trees through the year.
        Expect(SeasonLook.For(Season.Spring, VillageChoice.ShadeTree, -1) == ThingLook.Blossom, "trees blossom in spring");
        Expect(SeasonLook.For(Season.Summer, VillageChoice.ShadeTree, -1) == ThingLook.Full, "trees in full leaf in summer");
        Expect(SeasonLook.For(Season.Autumn, VillageChoice.ShadeTree, -1) == ThingLook.Turning, "trees turn in autumn");
        Expect(SeasonLook.For(Season.Winter, VillageChoice.ShadeTree, -1) == ThingLook.Resting, "trees rest in winter");
        // Evergreens keep their green; deciduous trees still change.
        foreach (string name in new[] { "holly", "yew", "Scots pine" })
            foreach (Season s in new[] { Season.Spring, Season.Summer, Season.Autumn, Season.Winter })
                Expect(SeasonLook.For(s, VillageChoice.ShadeTree, GardenCatalog.TreeIndexOf(name)) == ThingLook.Full, name + " stays green: " + s);
        int apple = GardenCatalog.TreeIndexOf("apple tree");
        int oak = GardenCatalog.TreeIndexOf("oak");
        Expect(apple >= 0 && oak >= 0, "apple tree and oak are in the catalogue");
        Expect(SeasonLook.For(Season.Spring, VillageChoice.ShadeTree, apple) == ThingLook.Blossom && SeasonLook.For(Season.Autumn, VillageChoice.ShadeTree, apple) == ThingLook.Turning
            && SeasonLook.For(Season.Winter, VillageChoice.ShadeTree, apple) == ThingLook.Resting && SeasonLook.For(Season.Summer, VillageChoice.ShadeTree, apple) == ThingLook.Full, "apple tree changes with the year");
        Expect(SeasonLook.For(Season.Spring, VillageChoice.ShadeTree, oak) == ThingLook.Blossom && SeasonLook.For(Season.Autumn, VillageChoice.ShadeTree, oak) == ThingLook.Turning
            && SeasonLook.For(Season.Winter, VillageChoice.ShadeTree, oak) == ThingLook.Resting && SeasonLook.For(Season.Summer, VillageChoice.ShadeTree, oak) == ThingLook.Full, "oak changes with the year");
        int evergreens = 0;
        for (int i = 0; i < GardenCatalog.TreeCount; i++)
            if (GardenCatalog.IsEvergreenTree(i)) evergreens++;
        Expect(evergreens == 10, "exactly 10 evergreen trees, found " + evergreens);
        Expect(!GardenCatalog.IsEvergreenTree(-1) && !GardenCatalog.IsEvergreenTree(1000), "no evergreen at -1 or 1000");
        Expect(GardenCatalog.TreeIndexOf("no such tree") == -1, "an unknown tree name has no index");
        // Built things never change with the season (they only gather snow).
        foreach (Season s in new[] { Season.Spring, Season.Summer, Season.Autumn, Season.Winter })
            Expect(SeasonLook.For(s, VillageChoice.Bridge, -1) == ThingLook.Full, "built things stay as built: " + s);
        // Flowers: bloom spring to autumn; in winter only winter flowers bloom.
        int snowdrops = Array.IndexOf(GardenCatalog.FlowersOf(Season.Winter), 21) >= 0 ? 21 : GardenCatalog.FlowersOf(Season.Winter)[0];
        int sunflowers = GardenCatalog.FlowersOf(Season.Summer)[0];
        Expect(SeasonLook.For(Season.Winter, VillageChoice.Flowers, snowdrops) == ThingLook.Full, "snowdrops bloom in winter");
        Expect(SeasonLook.For(Season.Winter, VillageChoice.Flowers, sunflowers) == ThingLook.Resting, "summer flowers rest in winter");
        Expect(SeasonLook.For(Season.Summer, VillageChoice.Flowers, sunflowers) == ThingLook.Full, "summer flowers bloom in summer");
        Expect(SeasonLook.For(Season.Spring, VillageChoice.Flowers, -1) == ThingLook.Full, "an unknown flower blooms outside winter");
        Expect(SeasonLook.For(Season.Winter, VillageChoice.Flowers, -1) == ThingLook.Resting, "an unknown flower rests in winter");
        // Snow: only in winter, on everything, and lying on the ground.
        Expect(SeasonLook.SnowCaps(Season.Winter) && !SeasonLook.SnowCaps(Season.Autumn), "snow caps in winter only");
        Expect(SeasonLook.SnowLying(Season.Winter) > 0.15f && SeasonLook.SnowLying(Season.Summer) == 0f, "snow lies in winter only");
        // Drift: enough to feel, never a blizzard.
        Expect(SeasonLook.DriftCount(Drift.Snow) >= 30 && SeasonLook.DriftCount(Drift.Snow) <= 60, "winter snowfall you can see");
        Expect(SeasonLook.DriftCount(Drift.Leaves) >= 16 && SeasonLook.DriftCount(Drift.Petals) >= 16, "autumn and spring drift you can see");
        Expect(SeasonLook.DriftCount(Drift.None) == 0, "summer is still");
        // Falling pieces need a deciduous tree to fall from.
        int holly = GardenCatalog.TreeIndexOf("holly");
        Expect(SeasonLook.Falls(VillageChoice.ShadeTree, apple), "a deciduous tree drops its leaves");
        Expect(SeasonLook.Falls(VillageChoice.ShadeTree, oak), "an oak drops its leaves");
        Expect(!SeasonLook.Falls(VillageChoice.ShadeTree, holly), "an evergreen (holly) does not drop leaves");
        Expect(SeasonLook.Falls(VillageChoice.ShadeTree, -1), "an unknown tree counts as falling");
        Expect(!SeasonLook.Falls(VillageChoice.Flowers, sunflowers), "flowers do not fall");
        Expect(!SeasonLook.Falls(VillageChoice.Bridge, -1), "a bridge does not fall");
        Expect(SeasonLook.DriftCount(Drift.Petals, 0) == 0, "no falling tree, no petals");
        Expect(SeasonLook.DriftCount(Drift.Petals, 1) == 10, "one tree drops 10 petals");
        Expect(SeasonLook.DriftCount(Drift.Leaves, 2) == 14, "two trees drop 14 leaves");
        Expect(SeasonLook.DriftCount(Drift.Petals, 4) == 20, "four trees reach 20");
        Expect(SeasonLook.DriftCount(Drift.Petals, 100) == 20, "many trees stay capped at 20");
        Expect(SeasonLook.DriftCount(Drift.Snow, 0) == 42, "snow does not need a tree");
        Expect(SeasonLook.DriftCount(Drift.None, 5) == 0, "no drift, no pieces");
        string mapView = Read("Assets/Scripts/Unity/GardenMapView.cs");
        string speech = Read("Assets/Scripts/Unity/GardenMapView.Speech.cs");
        Expect(speech.Contains("SeasonLook.Falls") && mapView.Contains("int falling = FallingTrees(items);"), "the map counts the falling trees");
        Expect(mapView.Contains("(DriftNow == Drift.Snow || falling > 0)"), "no petals or leaves without a tree that sheds them; snow still falls");
        Expect(mapView.Contains("DriftCount(Kind, "), "the drift is sized by the falling trees");
        // The river through the year.
        Expect(SeasonLook.River(Season.Winter) == RiverLook.IceEdged && SeasonLook.River(Season.Autumn) == RiverLook.LeavesAfloat &&
               SeasonLook.River(Season.Spring) == RiverLook.Running && SeasonLook.River(Season.Summer) == RiverLook.Sparkling,
            "the river changes with the year");
        return assertions;
    }

    private static string Read(string relative)
    {
        string dir = AppContext.BaseDirectory;
        while (dir != null && !File.Exists(Path.Combine(dir, relative)))
            dir = Path.GetDirectoryName(dir);
        if (dir == null) throw new Exception(relative + " not found");
        return File.ReadAllText(Path.Combine(dir, relative));
    }
}
