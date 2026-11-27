using System;
using System.Collections.Generic;

namespace Evergrove.Core
{
    /// <summary>I368: the good-neighbour pairings a garden can find.</summary>
    public enum Pairing { Sheltered, ByTheWater, FlowerBed, Grove, Waterside, ShadySeat, Orchard }

    public static class PairingMask
    {
        public static int Of(Pairing pairing) { return 1 << (int)pairing; }
    }

    /// <summary>A thing standing on the map: its cell, family and painting (to tell two flowers apart).</summary>
    public readonly struct PlacedThing
    {
        public readonly int Cell;
        public readonly VillageChoice Family;
        public readonly string Art;

        public PlacedThing(int cell, VillageChoice family, string art)
        {
            Cell = cell;
            Family = family;
            Art = art ?? "";
        }
    }

    /// <summary>
    /// I368 (run 3: "very logical complexity behind simple game plays"): good neighbours, after
    /// companion planting. Where you put things matters: a flower beside a tree is sheltered,
    /// a built thing beside the water stands by the water, two different flowers make a bed,
    /// two trees side by side a grove. Beside means touching, diagonals included. The first
    /// time a garden finds each, it pays a little (once, so the economy stays bounded).
    /// </summary>
    public static class Pairings
    {
        public const int Reward = 15;
        /// <summary>I380: the deeper three pay a little more.</summary>
        public const int DeeperReward = 25;
        /// <summary>I380: the first four, open from the start.</summary>
        public const int FirstFour = 0b1111;

        /// <summary>
        /// I380: the pairings a garden can find now: the first four, and once all four are
        /// found, three deeper ones (flowers by the water, a shady seat, an orchard). The more
        /// you play, the more the garden has to show you.
        /// </summary>
        public static int Open(WorldModel world)
        {
            int all = (1 << Enum.GetValues(typeof(Pairing)).Length) - 1;
            return world != null && (world.PairingsFound & FirstFour) == FirstFour ? all : FirstFour;
        }

        public static bool Deeper(Pairing pairing) { return (PairingMask.Of(pairing) & FirstFour) == 0; }

        public static int RewardFor(Pairing pairing) { return Deeper(pairing) ? DeeperReward : Reward; }

        public static int Between(GardenMap map, IList<PlacedThing> things)
        {
            int found = 0;
            for (int i = 0; i < things.Count; i++)
            {
                PlacedThing a = things[i];
                if (a.Cell < 0 || a.Cell >= map.CellCount) continue;
                if (a.Family == VillageChoice.Bridge && BesideWater(map, a.Cell)) found |= PairingMask.Of(Pairing.ByTheWater);
                if (a.Family == VillageChoice.Flowers && BesideWater(map, a.Cell)) found |= PairingMask.Of(Pairing.Waterside); // I380
                int treesBeside = 0; // I380: a tree touching two others makes an orchard
                if (a.Family == VillageChoice.ShadeTree)
                    for (int k = 0; k < things.Count; k++)
                        if (k != i && things[k].Family == VillageChoice.ShadeTree && things[k].Cell >= 0 && things[k].Cell < map.CellCount
                            && Beside(map, a.Cell, things[k].Cell)) treesBeside++;
                if (treesBeside >= 2) found |= PairingMask.Of(Pairing.Orchard);
                for (int j = i + 1; j < things.Count; j++)
                {
                    PlacedThing b = things[j];
                    if (b.Cell < 0 || b.Cell >= map.CellCount || !Beside(map, a.Cell, b.Cell)) continue;
                    bool flowerTree = (a.Family == VillageChoice.Flowers && b.Family == VillageChoice.ShadeTree) ||
                                      (b.Family == VillageChoice.Flowers && a.Family == VillageChoice.ShadeTree);
                    if (flowerTree) found |= PairingMask.Of(Pairing.Sheltered);
                    if (a.Family == VillageChoice.Flowers && b.Family == VillageChoice.Flowers && a.Art != b.Art)
                        found |= PairingMask.Of(Pairing.FlowerBed);
                    if (a.Family == VillageChoice.ShadeTree && b.Family == VillageChoice.ShadeTree)
                        found |= PairingMask.Of(Pairing.Grove);
                    if ((a.Family == VillageChoice.Bridge && b.Family == VillageChoice.ShadeTree) ||
                        (b.Family == VillageChoice.Bridge && a.Family == VillageChoice.ShadeTree))
                        found |= PairingMask.Of(Pairing.ShadySeat); // I380
                }
            }
            return found;
        }

        /// <summary>
        /// I372: the pairings not yet found (outside <paramref name="foundMask"/>) that moving
        /// thing <paramref name="moving"/> to <paramref name="cell"/> would make.
        /// </summary>
        public static int NewAt(GardenMap map, IList<PlacedThing> things, int moving, int cell, int foundMask)
        {
            if (moving < 0 || moving >= things.Count || cell < 0 || cell >= map.CellCount) return 0;
            var moved = new List<PlacedThing>(things);
            moved[moving] = new PlacedThing(cell, things[moving].Family, things[moving].Art);
            return Between(map, moved) & ~Between(map, things) & ~foundMask;
        }

        /// <summary>I371: touching pairs that make a pairing, as cell pairs (a, b, a, b...), for the map.</summary>
        public static int[] Links(GardenMap map, IList<PlacedThing> things)
        {
            var links = new List<int>();
            for (int i = 0; i < things.Count; i++)
                for (int j = i + 1; j < things.Count; j++)
                {
                    PlacedThing a = things[i], b = things[j];
                    if (a.Cell < 0 || b.Cell < 0 || a.Cell >= map.CellCount || b.Cell >= map.CellCount || !Beside(map, a.Cell, b.Cell)) continue;
                    int pair = Between(map, new[] { a, b }) & ~PairingMask.Of(Pairing.ByTheWater) & ~PairingMask.Of(Pairing.Waterside);
                    if (pair == 0) continue;
                    links.Add(a.Cell);
                    links.Add(b.Cell);
                }
            return links.ToArray();
        }

        /// <summary>What a garden's own things show, read from the save and its map.</summary>
        public static int Found(WorldModel world, GardenMap map)
        {
            return world == null || map == null || world.Mode != PlayMode.Living ? 0 : Between(map, Things(world, map)) & Open(world);
        }

        /// <summary>The garden's own things as placed: story things, plantings and nursery plants.</summary>
        public static List<PlacedThing> Things(WorldModel world, GardenMap map)
        {
            var things = new List<PlacedThing>();
            if (world == null || map == null) return things;
            int[] made = world.Choices;
            int[] cells = map.Layout(made, world.Placements, world.Occupied);
            for (int i = 0; i < made.Length; i++)
            {
                var family = (VillageChoice)Math.Max(0, Math.Min(2, made[i]));
                things.Add(new PlacedThing(cells[i], family,
                    GardenCatalog.For(world.Garden, Math.Min(i, GardenCatalog.ChaptersPerGarden - 1), family).Art));
            }
            int[] planted = world.Plantings;
            for (int k = 0; k * 2 + 1 < planted.Length; k++)
            {
                int source = planted[k * 2];
                if (source < 0 || source >= made.Length) continue;
                things.Add(new PlacedThing(planted[k * 2 + 1], things[source].Family, things[source].Art));
            }
            int[] bought = world.Bought;
            for (int k = 0; k * 2 + 1 < bought.Length; k++)
            {
                int kind = bought[k * 2];
                if (kind < 0 || kind >= Nursery.Kinds) continue;
                things.Add(new PlacedThing(bought[k * 2 + 1], Nursery.Family(kind), Nursery.Item(kind).Art));
            }
            return things;
        }

        public static string Hint(Pairing pairing)
        {
            switch (pairing)
            {
                case Pairing.Sheltered: return "A flower beside a tree: sheltered.";
                case Pairing.ByTheWater: return "Something built beside the water.";
                case Pairing.FlowerBed: return "Two different flowers side by side: a bed.";
                case Pairing.Grove: return "Two trees side by side: a grove.";
                case Pairing.Waterside: return "A flower beside the water.";
                case Pairing.ShadySeat: return "Something built beside a tree: a shady seat.";
                default: return "A tree touching two others: an orchard.";
            }
        }

        private static bool Beside(GardenMap map, int a, int b)
        {
            int ax = a % map.Columns, ay = a / map.Columns, bx = b % map.Columns, by = b / map.Columns;
            return a != b && Math.Abs(ax - bx) <= 1 && Math.Abs(ay - by) <= 1;
        }

        /// <summary>
        /// I374: where a pairing's visitor stands: for by-the-water, a water square beside the
        /// built thing (the heron's); for sheltered, the flower (the hedgehog's); for a grove, a
        /// tree (the owl's). -1 when the garden doesn't have the pairing as it stands.
        /// </summary>
        public static int SpotFor(GardenMap map, IList<PlacedThing> things, Pairing pairing)
        {
            for (int i = 0; i < things.Count; i++)
            {
                PlacedThing a = things[i];
                if (a.Cell < 0 || a.Cell >= map.CellCount) continue;
                if (pairing == Pairing.ByTheWater || pairing == Pairing.Waterside)
                {
                    if (a.Family != (pairing == Pairing.ByTheWater ? VillageChoice.Bridge : VillageChoice.Flowers)) continue;
                    int water = WaterBeside(map, a.Cell);
                    if (water >= 0) return water;
                    continue;
                }
                for (int j = 0; j < things.Count; j++)
                {
                    PlacedThing b = things[j];
                    if (j == i || b.Cell < 0 || b.Cell >= map.CellCount || !Beside(map, a.Cell, b.Cell)) continue;
                    if (pairing == Pairing.Sheltered && a.Family == VillageChoice.Flowers && b.Family == VillageChoice.ShadeTree) return a.Cell;
                    if (pairing == Pairing.Grove && a.Family == VillageChoice.ShadeTree && b.Family == VillageChoice.ShadeTree) return a.Cell;
                    if (pairing == Pairing.ShadySeat && a.Family == VillageChoice.Bridge && b.Family == VillageChoice.ShadeTree) return a.Cell; // I402
                }
            }
            return -1;
        }

        /// <summary>
        /// I398: a gift idea for a family garden: the first kind of thing (flower, tree, something
        /// built) that, set in some free land spot, would give that garden a good neighbour it
        /// hasn't found (within <paramref name="openMask"/>, outside <paramref name="foundMask"/>).
        /// Returns false when nothing one thing could do. Pure; the map and things are theirs.
        /// </summary>
        public static bool GiftIdea(GardenMap map, IList<PlacedThing> things, int foundMask, int openMask,
            out VillageChoice family, out Pairing pairing)
        {
            family = VillageChoice.Flowers;
            pairing = Pairing.Sheltered;
            if (map == null || things == null) return false;
            var taken = new HashSet<int>();
            foreach (PlacedThing t in things) taken.Add(t.Cell);
            int closed = ~openMask | foundMask;
            foreach (VillageChoice kind in new[] { VillageChoice.Flowers, VillageChoice.ShadeTree, VillageChoice.Bridge })
            {
                var trial = new List<PlacedThing>(things) { new PlacedThing(-1, kind, "gift-" + kind) };
                int best = 0;
                for (int c = 0; c < map.CellCount && best == 0; c++)
                    if (map.IsLand(c) && !taken.Contains(c)) best = NewAt(map, trial, trial.Count - 1, c, closed);
                if (best == 0) continue;
                family = kind;
                for (int p = 0; p < 32; p++)
                    if ((best & (1 << p)) != 0) { pairing = (Pairing)p; break; }
                return true;
            }
            return false;
        }

        /// <summary>I401: what a good neighbour brings, once found (the visitor, the season's gift).</summary>
        public static string Brings(Pairing pairing)
        {
            switch (pairing)
            {
                case Pairing.Sheltered: return "A hedgehog comes at dusk, and the bees carry pollen to the tree.";
                case Pairing.ByTheWater: return "A heron comes to fish.";
                case Pairing.FlowerBed: return "In spring it sows itself.";
                case Pairing.Grove: return "An owl at night, and seeds in summer.";
                case Pairing.Waterside: return "A kingfisher comes.";
                case Pairing.ShadySeat: return "In the golden hour, the neighbours sit there.";
                default: return "In autumn, fruit to pick.";
            }
        }

        /// <summary>I399: the good neighbours two gardens have both found, as a mask.</summary>
        public static int Shared(WorldModel a, WorldModel b)
        {
            return a == null || b == null ? 0 : a.PairingsFound & b.PairingsFound;
        }

        /// <summary>
        /// I378: whether thing <paramref name="index"/> is a tree with a flower beside it, so bees
        /// working the flower carry pollen to it.
        /// </summary>
        public static bool Pollinated(GardenMap map, IList<PlacedThing> things, int index)
        {
            if (map == null || index < 0 || index >= things.Count) return false;
            PlacedThing tree = things[index];
            if (tree.Family != VillageChoice.ShadeTree || tree.Cell < 0 || tree.Cell >= map.CellCount) return false;
            foreach (PlacedThing other in things)
                if (other.Family == VillageChoice.Flowers && other.Cell >= 0 && other.Cell < map.CellCount && Beside(map, tree.Cell, other.Cell))
                    return true;
            return false;
        }

        private static int WaterBeside(GardenMap map, int cell)
        {
            int x = cell % map.Columns, y = cell / map.Columns;
            for (int dy = -1; dy <= 1; dy++)
                for (int dx = -1; dx <= 1; dx++)
                {
                    int nx = x + dx, ny = y + dy;
                    if ((dx == 0 && dy == 0) || nx < 0 || nx >= map.Columns || ny < 0 || ny >= map.Rows) continue;
                    if (map.At(ny * map.Columns + nx) == GardenMap.Water) return ny * map.Columns + nx;
                }
            return -1;
        }

        private static bool BesideWater(GardenMap map, int cell)
        {
            int x = cell % map.Columns, y = cell / map.Columns;
            for (int dy = -1; dy <= 1; dy++)
                for (int dx = -1; dx <= 1; dx++)
                {
                    int nx = x + dx, ny = y + dy;
                    if ((dx == 0 && dy == 0) || nx < 0 || nx >= map.Columns || ny < 0 || ny >= map.Rows) continue;
                    if (map.At(ny * map.Columns + nx) == GardenMap.Water) return true;
                }
            return false;
        }
    }

    public sealed partial class WorldModel
    {
        /// <summary>I368: the pairings this garden has found, as a mask.</summary>
        public int PairingsFound { get { return state.pairings; } }

        public bool HasPairing(Pairing pairing) { return (state.pairings & PairingMask.Of(pairing)) != 0; }

        /// <summary>The first time a pairing is found, it pays Pairings.Reward seeds; never again.</summary>
        /// <summary>
        /// I378: one more fruit when a tended tree has a flower beside it and the bees are about
        /// (three flowers growing, I325; not in winter). Living gardens only; up to the cap.
        /// Returns whether a fruit was picked.
        /// </summary>
        public bool PickPollinatedFruit(bool flowerBeside, Season season)
        {
            if (Mode != PlayMode.Living || !flowerBeside || season == Season.Winter || !Visitors.Welcomes(this, Visitor.Bees)
                || state.fruit >= MaxFruit) return false;
            state.fruit++;
            return true;
        }

        public int DiscoverPairing(Pairing pairing)
        {
            if (Mode != PlayMode.Living || HasPairing(pairing) || (Pairings.Open(this) & PairingMask.Of(pairing)) == 0) return 0;
            state.pairings |= PairingMask.Of(pairing);
            int before = state.seeds;
            ChangeSeeds(Pairings.RewardFor(pairing), SeedReason.Pairing); // v1 #78: every seed change is in the ledger
            return state.seeds - before;
        }

        private static bool ReadPairings(WorldSnapshot source)
        {
            int all = (1 << Enum.GetValues(typeof(Pairing)).Length) - 1;
            return source.pairings >= 0 && (source.pairings & ~all) == 0 && (source.mode == "living" || source.pairings == 0);
        }
    }
}
