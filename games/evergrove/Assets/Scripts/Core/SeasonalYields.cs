using System;

namespace Evergrove.Core
{
    /// <summary>What the season gave, if anything.</summary>
    public enum SeasonYield { None, Seedling, Seeds, Fruit, Jam }

    /// <summary>
    /// I388 to I391 (run 3, "the garden's year"): once each season, a good neighbour standing in
    /// the garden gives what that season gives, the way a real garden does. Spring: a flower bed
    /// self-seeds (a flower cutting in the basket). Summer: a grove lets fall its seeds. Autumn:
    /// an orchard is harvested. Winter: neighbours you gave fruit to through the year bring you a
    /// jar of their jam. Once per season turn, so it brings you back through the year, bounded.
    /// </summary>
    public static class SeasonalYields
    {
        public const int SummerSeeds = 20;
        public const int AutumnFruit = 5;
        /// <summary>The self-sown cutting: the first nursery flower (kind % families = flowers).</summary>
        public const int SpringKind = (int)VillageChoice.Flowers;

        /// <summary>
        /// Which season this is, as a number that never repeats: during the story the story's own
        /// season (0..3); after it, 4 + the season turns since (I251).
        /// </summary>
        public static int Key(WorldModel world)
        {
            if (world == null || world.Mode != PlayMode.Living) return -1;
            return world.Completed ? 4 + Math.Max(0, world.PuzzlesSolved) / GardenMap.SolvesPerReach : (int)GardenSeason.For(world);
        }

        /// <summary>The good neighbour a season's yield needs; winter needs none (it is the neighbours' turn).</summary>
        public static Pairing? Needs(Season season)
        {
            switch (season)
            {
                case Season.Spring: return Pairing.FlowerBed;
                case Season.Summer: return Pairing.Grove;
                case Season.Autumn: return Pairing.Orchard;
                default: return null;
            }
        }

        public static SeasonYield Gives(Season season)
        {
            switch (season)
            {
                case Season.Spring: return SeasonYield.Seedling;
                case Season.Summer: return SeasonYield.Seeds;
                case Season.Autumn: return SeasonYield.Fruit;
                default: return SeasonYield.Jam;
            }
        }

        /// <summary>I400: whether this season's yield is still to come (not yet taken this turn).</summary>
        public static bool Waiting(WorldModel world)
        {
            int key = Key(world);
            return key >= 0 && world.YieldTaken != key + 1;
        }

        /// <summary>What the almanac says each season brings, and why.</summary>
        public static string Why(Season season)
        {
            switch (season)
            {
                case Season.Spring: return "Spring: a flower bed sows itself. A seedling for your basket.";
                case Season.Summer: return "Summer: a grove lets fall its seeds. " + SummerSeeds + " seeds.";
                case Season.Autumn: return "Autumn: an orchard is ready to pick. " + AutumnFruit + " fruit.";
                default: return "Winter: neighbours you gave fruit to bring a jar of their jam.";
            }
        }
    }

    public sealed partial class WorldModel
    {
        /// <summary>I388: the last season turn whose yield was taken, plus one (0: none yet).</summary>
        public int YieldTaken { get { return state.yields; } }

        /// <summary>
        /// I388: takes this season's yield, once per season turn. <paramref name="standing"/> is the
        /// pairings mask as the garden stands now; the needed one must also have been found. Gives
        /// nothing (and keeps the yield waiting) when a cap or the basket is full.
        /// </summary>
        public SeasonYield TakeSeasonYield(int standing)
        {
            int key = SeasonalYields.Key(this);
            if (Mode != PlayMode.Living || key < 0 || state.yields == key + 1) return SeasonYield.None;
            Season season = GardenSeason.For(this);
            Pairing? needs = SeasonalYields.Needs(season);
            if (needs.HasValue && (!HasPairing(needs.Value) || (standing & PairingMask.Of(needs.Value)) == 0)) return SeasonYield.None;
            switch (SeasonalYields.Gives(season))
            {
                case SeasonYield.Seedling:
                    if (!ReceiveNeighbourCutting(SeasonalYields.SpringKind)) return SeasonYield.None;
                    break;
                case SeasonYield.Seeds:
                    if (state.seeds >= MaxSeeds) return SeasonYield.None;
                    ChangeSeeds(SeasonalYields.SummerSeeds, SeedReason.Season); // v1 #78: every seed change is in the ledger
                    break;
                case SeasonYield.Fruit:
                    if (state.fruit >= MaxFruit) return SeasonYield.None;
                    state.fruit = Math.Min(MaxFruit, state.fruit + SeasonalYields.AutumnFruit);
                    break;
                default:
                    int given = 0;
                    foreach (int g in state.fruitGiven) given += g;
                    if (given == 0 || state.jam >= MaxJam) return SeasonYield.None;
                    state.jam++;
                    break;
            }
            state.yields = key + 1;
            return SeasonalYields.Gives(season);
        }

        private static bool ReadYields(WorldSnapshot source)
        {
            return source.yields >= 0 && source.yields <= 1000000 && (source.mode == "living" || source.yields == 0);
        }
    }
}
