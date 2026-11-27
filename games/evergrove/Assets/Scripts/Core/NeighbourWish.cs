using System;
using System.Collections.Generic;

namespace Evergrove.Core
{
    /// <summary>
    /// I313 (idea 31, after caring games where each companion has a wish you can grant):
    /// Mira, Orin and Fen each hope for something the garden can give, counted only from the
    /// save. Mira hopes for trees tended, Orin for the river to run on past the story, Fen for
    /// many kinds of flower. When it comes true, they write to say thank you, once.
    /// Never a quest with a timer: a hope, shown with its true count.
    /// </summary>
    public sealed class NeighbourWish
    {
        public string From { get; private set; }
        public string Hope { get; private set; }
        public string Thanks { get; private set; }
        public int Have { get; private set; }
        public int Need { get; private set; }
        public bool Granted { get { return Have >= Need; } }

        private static readonly string[] Names = { "Mira", "Orin", "Fen" };

        public static NeighbourWish For(WorldModel world, int villager)
        {
            if (world == null || villager < 0 || villager > 2) return null;
            int have, need;
            string hope, thanks;
            if (villager == (int)VillageChoice.ShadeTree)
            {
                need = 5;
                have = TreesTended(world);
                hope = "Mira hopes to see five trees tended";
                thanks = "Five trees tended, and every one of them yours. I walked among them this morning and thought of my grandmother's orchard. Thank you.";
            }
            else if (villager == (int)VillageChoice.Bridge)
            {
                need = 2;
                have = Math.Max(0, GardenMap.ReachesFor(world.Choices.Length, world.Completed, world.PuzzlesSolved) - GardenMap.MaxReaches);
                hope = "Orin hopes to see the river run two stretches further";
                thanks = "Two new stretches of river. I walked the bank all the way down today, further than I've walked in years. Thank you for that.";
            }
            else
            {
                need = 6;
                have = FlowerKinds(world);
                hope = "Fen hopes to see six kinds of flower growing";
                thanks = "Six kinds of flower in one garden. I counted them twice, just for the pleasure of it. Thank you.";
            }
            return new NeighbourWish { From = Names[villager], Hope = hope, Thanks = thanks, Have = Math.Min(have, need), Need = need };
        }

        private static int TreesTended(WorldModel world)
        {
            int count = 0;
            int[] made = world.Choices;
            for (int i = 0; i < made.Length; i++)
                if (made[i] == (int)VillageChoice.ShadeTree && world.TreeSolved(i)) count++;
            int[] bought = world.Bought;
            for (int k = 0; k * 2 < bought.Length; k++)
                if (bought[k * 2] >= 0 && bought[k * 2] < Nursery.Kinds &&
                    Nursery.Family(bought[k * 2]) == VillageChoice.ShadeTree && world.BoughtSolved(k)) count++;
            return count;
        }

        private static int FlowerKinds(WorldModel world)
        {
            var kinds = new HashSet<string>(StringComparer.Ordinal);
            int[] made = world.Choices;
            for (int i = 0; i < made.Length; i++)
                if (made[i] == (int)VillageChoice.Flowers)
                    kinds.Add(GardenCatalog.For(world.Garden, Math.Min(i, GardenCatalog.ChaptersPerGarden - 1), VillageChoice.Flowers).Art);
            int[] bought = world.Bought;
            for (int k = 0; k * 2 < bought.Length; k++)
                if (bought[k * 2] >= 0 && bought[k * 2] < Nursery.Kinds && Nursery.Family(bought[k * 2]) == VillageChoice.Flowers)
                    kinds.Add(Nursery.Item(bought[k * 2]).Art);
            return kinds.Count;
        }
    }
}
