using System;
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
        // The river through the year.
        Expect(SeasonLook.River(Season.Winter) == RiverLook.IceEdged && SeasonLook.River(Season.Autumn) == RiverLook.LeavesAfloat &&
               SeasonLook.River(Season.Spring) == RiverLook.Running && SeasonLook.River(Season.Summer) == RiverLook.Sparkling,
            "the river changes with the year");
        return assertions;
    }
}
