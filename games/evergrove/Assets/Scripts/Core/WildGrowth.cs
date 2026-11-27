using System;
using System.Collections.Generic;

namespace Evergrove.Core
{
    /// <summary>
    /// Checklist 46 (things grow outside the garden): flowers the player planted seed themselves
    /// along the river and out over the free land. The wild set is a fixed order of cells, grown
    /// one more cell every <see cref="DaysPerCell"/> days played, up to <see cref="Most"/>. Growth
    /// is paced by days played, not calendar days: the save keeps no planted-at date, and days
    /// played only grows, so the valley never shrinks, and a time away costs nothing.
    /// Deterministic from (garden, the planted flower kinds, days played); cosmetic only.
    /// </summary>
    public static class WildGrowth
    {
        public const int DaysPerCell = 2;
        public const int Most = 24;
        /// <summary>Cells within this many steps of the water are preferred over free land further off.</summary>
        public const int RiverStrip = 3;

        /// <summary>How many wild cells the valley has grown to after <paramref name="daysPlayed"/> days.</summary>
        /// <summary>Wild cells each hand-planted flower can seed, at most.</summary>
        public const int PerFlower = 3;

        /// <summary>Flowers planted by hand: basket plantings of the Flowers family, and nursery flowers.</summary>
        public static int FlowersPlanted(WorldModel world)
        {
            if (world == null) return 0;
            int count = 0;
            int[] choices = world.Choices;
            int[] planted = world.Plantings;
            for (int i = 0; i + 1 < planted.Length; i += 2)
                if (planted[i] >= 0 && planted[i] < choices.Length && choices[planted[i]] == (int)VillageChoice.Flowers) count++;
            int[] bought = world.Bought;
            for (int i = 0; i + 1 < bought.Length; i += 2)
                if (bought[i] >= 0 && bought[i] < Nursery.Kinds && Nursery.Family(bought[i]) == VillageChoice.Flowers) count++;
            return count;
        }

        public static int Grown(int daysPlayed)
        {
            if (daysPlayed <= 0) return 0;
            return Math.Min(Most, daysPlayed / DaysPerCell);
        }

        /// <summary>
        /// The wild cells, in the order they grew. Empty until the player has planted a flower by hand
        /// (from the basket, or a flower bought from the nursery). The result is always a prefix of the
        /// same fixed order, so it only ever grows as days advance.
        /// </summary>
        public static int[] Spread(GardenMap map, WorldModel world, int daysPlayed)
        {
            if (map == null || world == null) return Array.Empty<int>();
            SortedSet<int> kinds = PlantedFlowerKinds(world);
            // The save keeps no planted-at day, so days alone would let a first flower planted late arrive
            // with a burst of spread. It also grows with what you plant: at most PerFlower cells per flower.
            int steps = Math.Min(Grown(daysPlayed), PerFlower * FlowersPlanted(world));
            if (kinds.Count == 0 || steps == 0) return Array.Empty<int>();
            int[] order = Order(map, world, kinds);
            int count = Math.Min(steps, order.Length);
            var grown = new int[count];
            Array.Copy(order, grown, count);
            return grown;
        }

        /// <summary>
        /// The distinct flower kinds the player planted by hand: a basket planting whose item is a flower
        /// (keyed by its choice index), and a nursery plant from the flowers family (keyed past the choices).
        /// </summary>
        public static SortedSet<int> PlantedFlowerKinds(WorldModel world)
        {
            var kinds = new SortedSet<int>();
            int[] choices = world.Choices;
            int[] planted = world.Plantings;
            for (int k = 0; k * 2 + 1 < planted.Length; k++)
            {
                int source = planted[k * 2];
                if (source >= 0 && source < choices.Length && choices[source] == (int)VillageChoice.Flowers) kinds.Add(source);
            }
            int[] bought = world.Bought;
            for (int k = 0; k * 2 + 1 < bought.Length; k++)
            {
                int kind = bought[k * 2];
                if (kind >= 0 && kind < Nursery.Kinds && Nursery.Family(kind) == VillageChoice.Flowers) kinds.Add(BoughtKey + kind);
            }
            return kinds;
        }

        private const int BoughtKey = 1000;

        /// <summary>
        /// The full growth order: a seeded frontier that starts at free land beside the garden (anything
        /// standing) and spreads through free land only. Free river-side cells come first, then the rest;
        /// ties broken by a hash of the seed and the cell.
        /// </summary>
        private static int[] Order(GardenMap map, WorldModel world, SortedSet<int> kinds)
        {
            int cells = map.CellCount;
            bool[] taken = Taken(map, world);
            bool[] river = RiverStripCells(map);
            uint seed = Seed(world.Garden, kinds);

            var queued = new bool[cells];
            var frontier = new List<int>();
            for (int cell = 0; cell < cells; cell++)
            {
                if (!Eligible(map, taken, cell) || !NextToGarden(map, taken, cell)) continue;
                queued[cell] = true;
                frontier.Add(cell);
            }

            var order = new List<int>();
            var around = new List<int>(4);
            while (order.Count < Most && frontier.Count > 0)
            {
                int best = 0;
                ulong bestKey = Priority(seed, frontier[0], river);
                for (int i = 1; i < frontier.Count; i++)
                {
                    ulong key = Priority(seed, frontier[i], river);
                    if (key < bestKey) { best = i; bestKey = key; }
                }
                int cell = frontier[best];
                frontier[best] = frontier[frontier.Count - 1];
                frontier.RemoveAt(frontier.Count - 1);
                order.Add(cell);

                around.Clear();
                Neighbours(map, cell, around);
                foreach (int next in around)
                {
                    if (queued[next] || !Eligible(map, taken, next)) continue;
                    queued[next] = true;
                    frontier.Add(next);
                }
            }
            return order.ToArray();
        }

        /// <summary>Free land that nothing stands on, the cottage and the river excluded (IsLand).</summary>
        private static bool Eligible(GardenMap map, bool[] taken, int cell)
        {
            return map.IsLand(cell) && !taken[cell];
        }

        /// <summary>The garden edge: land beside something standing, or beside the cottage.</summary>
        private static bool NextToGarden(GardenMap map, bool[] taken, int cell)
        {
            var around = new List<int>(4);
            Neighbours(map, cell, around);
            foreach (int n in around)
                if (taken[n] || map.At(n) == GardenMap.Built) return true;
            return false;
        }

        /// <summary>Every cell with something standing on it: plantings, bought plants, decorations, and the layout of the items.</summary>
        private static bool[] Taken(GardenMap map, WorldModel world)
        {
            var taken = new bool[map.CellCount];
            int[] occupied = world.Occupied;
            for (int k = 0; k * 2 + 1 < occupied.Length; k++) Mark(taken, occupied[k * 2 + 1]);
            foreach (int cell in map.Layout(world.Choices, world.Placements, world.Plantings)) Mark(taken, cell);
            return taken;
        }

        private static void Mark(bool[] taken, int cell)
        {
            if (cell >= 0 && cell < taken.Length) taken[cell] = true;
        }

        private static bool[] RiverStripCells(GardenMap map)
        {
            var strip = new bool[map.CellCount];
            for (int cell = 0; cell < map.CellCount; cell++)
            {
                if (map.At(cell) != GardenMap.Water) continue;
                int col = cell % map.Columns, row = cell / map.Columns;
                for (int dy = -RiverStrip; dy <= RiverStrip; dy++)
                    for (int dx = -RiverStrip; dx <= RiverStrip; dx++)
                    {
                        int c = col + dx, r = row + dy;
                        if (c >= 0 && c < map.Columns && r >= 0 && r < map.Rows) strip[r * map.Columns + c] = true;
                    }
            }
            return strip;
        }

        private static void Neighbours(GardenMap map, int cell, List<int> into)
        {
            int col = cell % map.Columns, row = cell / map.Columns;
            if (col > 0) into.Add(cell - 1);
            if (col < map.Columns - 1) into.Add(cell + 1);
            if (row > 0) into.Add(cell - map.Columns);
            if (row < map.Rows - 1) into.Add(cell + map.Columns);
        }

        private static ulong Priority(uint seed, int cell, bool[] river)
        {
            ulong band = river[cell] ? 0UL : 1UL << 40;
            return band | Mix(seed ^ unchecked((uint)(cell + 1) * 2654435761u));
        }

        private static uint Seed(int garden, SortedSet<int> kinds)
        {
            uint seed = unchecked((uint)(garden + 1) * 2654435761u);
            foreach (int key in kinds) seed = Mix(seed ^ unchecked((uint)(key + 1) * 2246822519u));
            return seed;
        }

        private static uint Mix(uint x)
        {
            unchecked
            {
                x ^= x >> 15; x *= 2246822519u; x ^= x >> 13; x *= 3266489917u; x ^= x >> 16;
            }
            return x;
        }
    }
}
