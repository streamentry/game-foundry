using System.Collections.Generic;

namespace Evergrove.Core
{
    /// <summary>One object on the memory shelf: a first, kept once it truly happened.</summary>
    public sealed class Keepsake
    {
        public string Art;
        public string Label;
        public bool Kept;
    }

    /// <summary>
    /// I314 (idea 34, after unpacking games, where a life is told through objects): a shelf of
    /// firsts in the book. Every keepsake is read from the save as it is, so nothing is invented
    /// and nothing extra is stored: a first letter, a first fruit, a first nursery plant, a
    /// puzzle played together, help from family, a lantern, new river, the story's end.
    /// </summary>
    public static class MemoryShelf
    {
        public static Keepsake[] For(WorldModel world)
        {
            if (world == null) return new Keepsake[0];
            bool treeTended = false;
            int[] made = world.Choices;
            for (int i = 0; i < made.Length; i++)
                if (made[i] == (int)VillageChoice.ShadeTree && world.TreeSolved(i)) treeTended = true;
            int[] bought = world.Bought;
            string firstPlant = bought.Length >= 2 && bought[0] >= 0 && bought[0] < Nursery.Kinds
                ? Nursery.Item(bought[0]).Art : "tree-00";
            int reaches = GardenMap.ReachesFor(made.Length, world.Completed, world.PuzzlesSolved, world.RiverReplays);
            var shelf = new List<Keepsake>
            {
                Make("letter-sealed", "Your first letter", world.Letters.Length > 0),
                Make("food-pear", "Your first fruit, picked from a tree you tended", treeTended),
                Make(firstPlant, "Your first plant from the nursery", world.BoughtCount > 0),
                Make("ui-neighbors", "A puzzle you played together", world.Together.Length > 0),
                Make("food-basket", "Help from family, in your garden", world.Helped.Length > 0),
                Make("build-96", "A lantern left for you", world.Lanterns.Length > 0),
                Make("stone-wet", "New river, past the story", reaches > GardenMap.MaxReaches),
                // I386: where things stand matters; the first good neighbours, then all seven.
                Make("flower-05", "Your first good neighbours", world.PairingsFound != 0),
                Make("food-apple", "The first gift of a season", world.YieldTaken != 0), // I432
                Make("tree-05", "Every good neighbour found, all seven",
                    world.PairingsFound == (1 << System.Enum.GetValues(typeof(Pairing)).Length) - 1),
                Make("vignette-bridge", "The story of your garden, told to its end", world.Completed)
            };
            return shelf.ToArray();
        }

        private static Keepsake Make(string art, string label, bool kept)
        {
            return new Keepsake { Art = art, Label = label, Kept = kept };
        }
    }
}
