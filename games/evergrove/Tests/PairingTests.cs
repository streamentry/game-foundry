using System;
using Evergrove.Core;

/// <summary>
/// I368 (run 3): good neighbours. Where you put things matters, logically: a flower beside a
/// tree is sheltered, a built thing beside the water stands by the water, two different
/// flowers make a bed, two trees a grove. Each pays a little, the first time only.
/// </summary>
internal static class PairingTests
{
    private static int assertions;

    private static void Expect(bool ok, string message)
    {
        assertions++;
        if (!ok) throw new Exception("Pairing test failed: " + message);
    }

    public static int Run()
    {
        var map = new GardenMap(5, 3, "..~..\n.....\n.....");
        // cells: row 0: 0 1 [2 water] 3 4 ; row 1: 5..9 ; row 2: 10..14
        var tree = new PlacedThing(6, VillageChoice.ShadeTree, "tree-01");
        var flower = new PlacedThing(7, VillageChoice.Flowers, "flower-03");
        Expect((Pairings.Between(map, new[] { tree, flower }) & Pairings.FirstFour) == PairingMask.Of(Pairing.Sheltered), "a flower beside a tree is sheltered");
        var far = new PlacedThing(14, VillageChoice.Flowers, "flower-03");
        Expect(Pairings.Between(map, new[] { tree, far }) == 0, "not beside: nothing");
        var bridge = new PlacedThing(1, VillageChoice.Bridge, "build-00");
        Expect(Pairings.Between(map, new[] { bridge }) == PairingMask.Of(Pairing.ByTheWater), "built beside the water");
        var flower2 = new PlacedThing(8, VillageChoice.Flowers, "flower-09");
        int bed = Pairings.Between(map, new[] { flower, flower2 });
        Expect((bed & PairingMask.Of(Pairing.FlowerBed)) != 0, "two different flowers make a bed");
        var same = new PlacedThing(8, VillageChoice.Flowers, "flower-03");
        Expect((Pairings.Between(map, new[] { flower, same }) & PairingMask.Of(Pairing.FlowerBed)) == 0, "the same flower twice isn't a bed");
        var tree2 = new PlacedThing(11, VillageChoice.ShadeTree, "tree-05");
        Expect((Pairings.Between(map, new[] { tree, tree2 }) & PairingMask.Of(Pairing.Grove)) != 0, "two trees side by side make a grove");
        Expect(Pairings.Between(map, new PlacedThing[0]) == 0, "nothing placed, nothing found");
        // I372: holding a thing, which free spots would make a pairing not yet found.
        var holding = new System.Collections.Generic.List<PlacedThing> { tree, new PlacedThing(14, VillageChoice.Flowers, "flower-03") };
        int fresh = Pairings.NewAt(map, holding, 1, 7, 0);
        Expect((fresh & PairingMask.Of(Pairing.Sheltered)) != 0, "moving the flower beside the tree would shelter it");
        Expect(Pairings.NewAt(map, holding, 1, 13, 0) == 0, "far from the tree, nothing new");
        Expect(Pairings.NewAt(map, holding, 1, 7, PairingMask.Of(Pairing.Sheltered) | ~Pairings.FirstFour) == 0, "already found (deeper ones still closed): not new");
        // I371: the links drawn on the map: one per touching pair that makes a pairing.
        int[] links = Pairings.Links(map, new[] { tree, flower, far });
        Expect(links.Length == 2 && ((links[0] == 6 && links[1] == 7) || (links[0] == 7 && links[1] == 6)),
            "one link, between the tree and the flower beside it");
        Expect(Pairings.Links(map, new[] { tree, far }).Length == 0, "no link between things apart");

        // Discovered once, paid once, kept in the save.
        WorldModel world = WorldModel.New(PlayMode.Living);
        int seeds = world.Seeds;
        Expect(world.DiscoverPairing(Pairing.Sheltered) == Pairings.Reward && world.Seeds == seeds + Pairings.Reward, "first time pays");
        Expect(world.DiscoverPairing(Pairing.Sheltered) == 0 && world.Seeds == seeds + Pairings.Reward, "never twice");
        Expect(world.HasPairing(Pairing.Sheltered) && !world.HasPairing(Pairing.Grove), "remembered");
        Expect(WorldModel.New(PlayMode.PuzzleOnly).DiscoverPairing(Pairing.Grove) == 0, "only in a living garden");
        Expect(!GardenTransfer.Export(WorldModel.New(PlayMode.Living), "2026-11-27T00:00:00Z").Contains("\"pairings\""),
            "older gardens' files are unchanged");
        Expect(GardenTransfer.TryImport(GardenTransfer.Export(world, "2026-11-27T00:00:00Z"), out WorldModel back, out _) &&
               back.HasPairing(Pairing.Sheltered), "found pairings travel in the garden file");
        foreach (Pairing p in Enum.GetValues(typeof(Pairing)))
            Expect(Pairings.Hint(p).Length > 0 && !Pairings.Hint(p).Contains("!"), "each has a calm hint");
        // I374: where each pairing's visitor stands, and when it is out.
        WorldModel owlGarden = WorldModel.New(PlayMode.Living);
        Expect(!Visitors.Welcomes(owlGarden, Visitor.Owl), "no grove yet, no owl");
        owlGarden.DiscoverPairing(Pairing.Grove);
        Expect(Visitors.Welcomes(owlGarden, Visitor.Owl) && !Visitors.Welcomes(owlGarden, Visitor.Heron), "a grove found welcomes the owl, only");
        Expect(Pairings.SpotFor(map, new[] { bridge }, Pairing.ByTheWater) == 2, "the heron stands in the water beside the built thing");
        Expect(Pairings.SpotFor(map, new[] { tree, flower }, Pairing.Sheltered) == 7, "the hedgehog comes to the sheltered flower");
        Expect(Pairings.SpotFor(map, new[] { tree, tree2 }, Pairing.Grove) >= 0, "the owl comes to the grove");
        Expect(Pairings.SpotFor(map, new[] { tree, far }, Pairing.Sheltered) == -1, "no pairing, no spot");
        Expect(Visitors.Out(Visitor.Hedgehog, Season.Summer, DayPhase.Dusk) && !Visitors.Out(Visitor.Hedgehog, Season.Summer, DayPhase.Day),
            "the hedgehog comes at dusk, not by day");
        Expect(!Visitors.Out(Visitor.Hedgehog, Season.Winter, DayPhase.Night), "the hedgehog sleeps the winter through");
        Expect(Visitors.Out(Visitor.Owl, Season.Winter, DayPhase.Night) && !Visitors.Out(Visitor.Owl, Season.Summer, DayPhase.Dusk),
            "the owl only at night, any season");
        Expect(Visitors.Out(Visitor.Heron, Season.Autumn, DayPhase.Day) && !Visitors.Out(Visitor.Heron, Season.Winter, DayPhase.Day)
            && !Visitors.Out(Visitor.Heron, Season.Summer, DayPhase.Night), "the heron by day, not on a frozen river");
        // I377: a keeper's remark about their visitor, once it has come, one day in three.
        Expect(Visitors.KeeperRemark(WorldModel.New(PlayMode.Living), 0, 0) == null, "no owl yet, nothing to say");
        int said = 0;
        for (int day = 0; day < 30; day++) if (Visitors.KeeperRemark(owlGarden, 0, day) != null) said++;
        Expect(said == 10, "Mira mentions the owl one day in three");
        Expect(Visitors.KeeperRemark(owlGarden, 1, 2) == null, "Orin has no heron to speak of");
        Expect(Visitors.KeeperVisitor(2) == Visitor.Hedgehog, "Fen's flowers bring the hedgehog");
        // I378: a tree with a flower beside it, bees about, gives one more fruit when tended.
        Expect(Pairings.Pollinated(map, new[] { tree, flower }, 0), "the tree beside a flower is pollinated");
        Expect(!Pairings.Pollinated(map, new[] { tree, flower }, 1), "the flower itself isn't");
        Expect(!Pairings.Pollinated(map, new[] { tree, far }, 0), "a flower far off: no");
        WorldModel orchard = WorldModel.New(PlayMode.Living);
        Expect(!orchard.PickPollinatedFruit(true, Season.Summer), "no bees yet: no extra fruit");
        Expect(orchard.Fruit == 0, "nothing picked");
        // I380: three deeper pairings, open once the first four are found.
        Expect((Pairings.Between(map, new[] { flower }) & PairingMask.Of(Pairing.Waterside)) != 0, "a flower beside the water");
        Expect((Pairings.Between(map, new[] { far }) & PairingMask.Of(Pairing.Waterside)) == 0, "a flower far from the water: not waterside");
        var seat = new PlacedThing(12, VillageChoice.Bridge, "build-03");
        Expect((Pairings.Between(map, new[] { tree2, seat }) & PairingMask.Of(Pairing.ShadySeat)) != 0, "built beside a tree: a shady seat");
        var tree3 = new PlacedThing(10, VillageChoice.ShadeTree, "tree-02");
        Expect((Pairings.Between(map, new[] { tree, tree2 }) & PairingMask.Of(Pairing.Orchard)) == 0, "two trees: a grove, not yet an orchard");
        Expect((Pairings.Between(map, new[] { tree, tree2, tree3 }) & PairingMask.Of(Pairing.Orchard)) != 0, "a tree touching two others: an orchard");
        WorldModel deep = WorldModel.New(PlayMode.Living);
        Expect(Pairings.Open(deep) == Pairings.FirstFour, "at first only the four are open");
        Expect(deep.DiscoverPairing(Pairing.Orchard) == 0 && !deep.HasPairing(Pairing.Orchard), "a deeper one can't be found early");
        foreach (Pairing p in new[] { Pairing.Sheltered, Pairing.ByTheWater, Pairing.FlowerBed, Pairing.Grove }) deep.DiscoverPairing(p);
        Expect((Pairings.Open(deep) & PairingMask.Of(Pairing.Orchard)) != 0, "all four found: the deeper three open");
        int seedsBeforeOrchard = deep.Seeds;
        Expect(deep.DiscoverPairing(Pairing.Orchard) == Pairings.DeeperReward && deep.Seeds == seedsBeforeOrchard + Pairings.DeeperReward,
            "a deeper pairing pays a little more");
        foreach (Pairing p in Enum.GetValues(typeof(Pairing))) Expect(Pairings.Hint(p).Length > 0, "each has its rule written");
        // v1 #32: each pairing is a rule you can say out loud: one short line, no line breaks.
        foreach (Pairing p in Enum.GetValues(typeof(Pairing)))
            Expect(Pairings.Hint(p).IndexOf('\n') < 0 && Pairings.Hint(p).Length <= 100,
                "each rule is one short line: " + Pairings.Hint(p));
        // I382: the kingfisher at a waterside flower.
        Expect(Pairings.SpotFor(map, new[] { flower }, Pairing.Waterside) == 2, "the kingfisher over the water beside the flower");
        Expect(Visitors.Out(Visitor.Kingfisher, Season.Spring, DayPhase.Morning) && !Visitors.Out(Visitor.Kingfisher, Season.Winter, DayPhase.Day)
            && !Visitors.Out(Visitor.Kingfisher, Season.Summer, DayPhase.Night), "the kingfisher by day, on open water");
        // I398: a gift idea for a family garden: what one thing could do there.
        Expect(Pairings.GiftIdea(map, new[] { tree }, 0, Pairings.FirstFour, out VillageChoice giftKind, out Pairing giftMakes)
               && giftKind == VillageChoice.Flowers && giftMakes == Pairing.Sheltered, "a lone tree: a flower would be sheltered");
        Expect(Pairings.GiftIdea(map, new[] { tree }, PairingMask.Of(Pairing.Sheltered), Pairings.FirstFour, out giftKind, out giftMakes)
               && giftMakes != Pairing.Sheltered, "already sheltered: another idea");
        Expect(!Pairings.GiftIdea(map, new PlacedThing[0], Pairings.FirstFour, Pairings.FirstFour, out _, out _), "all found: no idea needed");
        // I399: what two gardens share.
        WorldModel mine = WorldModel.New(PlayMode.Living), theirs = WorldModel.New(PlayMode.Living);
        mine.DiscoverPairing(Pairing.Grove); mine.DiscoverPairing(Pairing.FlowerBed); theirs.DiscoverPairing(Pairing.Grove);
        Expect(Pairings.Shared(mine, theirs) == PairingMask.Of(Pairing.Grove) && Pairings.Shared(null, theirs) == 0, "both have a grove");
        // I401/I402: each says what it brings; the shady seat is where neighbours sit.
        foreach (Pairing p in Enum.GetValues(typeof(Pairing))) Expect(Pairings.Brings(p).Length > 0 && !Pairings.Brings(p).Contains("!"), "each brings something");
        Expect(Pairings.SpotFor(map, new[] { tree2, seat }, Pairing.ShadySeat) == seat.Cell, "the seat itself");
        // I405: visitors keep the weather (what is seen only).
        Expect(Visitors.OutIn(Visitor.Bees, Season.Summer, DayPhase.Day, Weather.Fair) && !Visitors.OutIn(Visitor.Bees, Season.Summer, DayPhase.Day, Weather.Rainy),
            "bees stay in when it rains");
        Expect(!Visitors.OutIn(Visitor.Butterflies, Season.Summer, DayPhase.Day, Weather.Windy) && Visitors.OutIn(Visitor.Heron, Season.Summer, DayPhase.Day, Weather.Rainy),
            "butterflies keep out of the wind; a heron doesn't mind rain");
        Expect(!Visitors.OutIn(Visitor.Owl, Season.Autumn, DayPhase.Night, Weather.Rainy), "the owl keeps to its tree on a rainy night");
        // I409-I411: mist at dawn and morning but not on hot or windy days; smoke on cold
        // evenings; frogs on summer dusks.
        Expect(DayLight.Mist(DayPhase.Dawn, Weather.Fair) && !DayLight.Mist(DayPhase.Day, Weather.Fair) && !DayLight.Mist(DayPhase.Morning, Weather.Windy),
            "mist lies at dawn, burns off by day, blows away in the wind");
        Expect(DayLight.ChimneySmoke(DayPhase.Dusk, Season.Autumn, Weather.Fair) && !DayLight.ChimneySmoke(DayPhase.Dusk, Season.Summer, Weather.Fair)
            && DayLight.ChimneySmoke(DayPhase.Night, Season.Spring, Weather.Snowy) && !DayLight.ChimneySmoke(DayPhase.Day, Season.Winter, Weather.Fair),
            "the fire is lit on cold evenings");
        Expect(Array.IndexOf(Soundscape.For(Season.Summer, DayPhase.Dusk), AmbientVoice.Frogs) >= 0 &&
               Array.IndexOf(Soundscape.For(Season.Autumn, DayPhase.Dusk), AmbientVoice.Frogs) < 0, "frogs on summer dusks only");
        // I413-I417: the river's moments.
        Expect(RiverMoments.RainRings(Season.Summer, Weather.Rainy) && !RiverMoments.RainRings(Season.Winter, Weather.Rainy)
            && !RiverMoments.RainRings(Season.Summer, Weather.Fair), "rain rings on open water when it rains");
        Expect(RiverMoments.FishEvery(Season.Summer, DayPhase.Dawn) > 0f && RiverMoments.FishEvery(Season.Summer, DayPhase.Dawn) < RiverMoments.FishEvery(Season.Summer, DayPhase.Day)
            && RiverMoments.FishEvery(Season.Winter, DayPhase.Day) == 0f && RiverMoments.FishEvery(Season.Summer, DayPhase.Night) == 0f,
            "fish rise most at the edges of the day, never in winter or at night");
        Expect(RiverMoments.Glitter(Season.Summer, DayPhase.Golden, Weather.Fair) && !RiverMoments.Glitter(Season.Summer, DayPhase.Golden, Weather.Rainy)
            && !RiverMoments.Glitter(Season.Summer, DayPhase.Day, Weather.Fair), "glitter in a clear golden hour");
        Expect(RiverMoments.LeavesAfloat(Season.Autumn) && !RiverMoments.LeavesAfloat(Season.Spring), "leaves float in autumn");
        // I425/I426: moonlight on clear nights; bees a voice of their own, rarer than birds.
        Expect(RiverMoments.Moonlight(DayPhase.Night, Weather.Fair) && !RiverMoments.Moonlight(DayPhase.Night, Weather.Rainy)
            && !RiverMoments.Moonlight(DayPhase.Dusk, Weather.Fair), "moonlight on the water on a clear night");
        Expect(Soundscape.SecondsBetween(AmbientVoice.Bees) > Soundscape.SecondsBetween(AmbientVoice.Birds), "bees heard less often than birds");
        // I376: the owl heard at night, only.
        Expect(Soundscape.VisitorVoice(Visitor.Owl, Season.Autumn, DayPhase.Night) == AmbientVoice.Owl, "the owl calls at night");
        Expect(Soundscape.VisitorVoice(Visitor.Owl, Season.Autumn, DayPhase.Day) == null, "never by day");
        Expect(Soundscape.VisitorVoice(Visitor.Heron, Season.Summer, DayPhase.Day) == null, "the heron fishes quietly");
        Expect(Soundscape.SecondsBetween(AmbientVoice.Owl) > Soundscape.SecondsBetween(AmbientVoice.Birds), "the owl is rarer than birds");
        return assertions;
    }
}
