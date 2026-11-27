using System;
using System.Collections.Generic;

namespace Evergrove.Core
{
    /// <summary>I368: the good-neighbour pairings a garden can find.</summary>
    public enum Pairing { Sheltered, ByTheWater, FlowerBed, Grove }

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

        public static int Between(GardenMap map, IList<PlacedThing> things)
        {
            int found = 0;
            for (int i = 0; i < things.Count; i++)
            {
                PlacedThing a = things[i];
                if (a.Cell < 0 || a.Cell >= map.CellCount) continue;
                if (a.Family == VillageChoice.Bridge && BesideWater(map, a.Cell)) found |= PairingMask.Of(Pairing.ByTheWater);
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
                    int pair = Between(map, new[] { a, b }) & ~PairingMask.Of(Pairing.ByTheWater);
                    if (pair == 0) continue;
                    links.Add(a.Cell);
                    links.Add(b.Cell);
                }
            return links.ToArray();
        }

        /// <summary>What a garden's own things show, read from the save and its map.</summary>
        public static int Found(WorldModel world, GardenMap map)
        {
            return world == null || map == null || world.Mode != PlayMode.Living ? 0 : Between(map, Things(world, map));
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
                default: return "Two trees side by side: a grove.";
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
                if (pairing == Pairing.ByTheWater)
                {
                    if (a.Family != VillageChoice.Bridge) continue;
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
                }
            }
            return -1;
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
        public int DiscoverPairing(Pairing pairing)
        {
            if (Mode != PlayMode.Living || HasPairing(pairing)) return 0;
            state.pairings |= PairingMask.Of(pairing);
            int before = state.seeds;
            ChangeSeeds(Pairings.Reward, SeedReason.Pairing); // v1 #78: every seed change is in the ledger
            return state.seeds - before;
        }

        private static bool ReadPairings(WorldSnapshot source)
        {
            int all = (1 << Enum.GetValues(typeof(Pairing)).Length) - 1;
            return source.pairings >= 0 && (source.pairings & ~all) == 0 && (source.mode == "living" || source.pairings == 0);
        }
    }
}
