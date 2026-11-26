namespace Evergrove.Core
{
    /// <summary>
    /// Neighbours who notice (depth spine, Layer 4; founder decision 2026-11-26:
    /// the companion is offline first). Each neighbour keeps to their own kind,
    /// Mira trees, Orin buildings, Fen flowers, and says one line built only from
    /// true facts in the save: a plant you bought of their kind, and how many of
    /// their kind's puzzles you have solved. Never guilt, never "I missed you",
    /// no clock, no randomness. Returns null when there is nothing new and true
    /// to say, so the older story lines speak instead. English only for now.
    /// </summary>
    public static class NeighbourNotice
    {
        public static string Line(int villager, WorldModel world)
        {
            if (world == null || world.Mode != PlayMode.Living || villager < 0 || villager > 2) return null;
            // A new arrival from the nursery is the freshest news: the newest one of
            // their kind, while its puzzle is still unsolved.
            int[] bought = world.Bought;
            for (int k = bought.Length / 2 - 1; k >= 0; k--)
            {
                int kind = bought[k * 2];
                if ((int)Nursery.Family(kind) != villager) continue;
                if (!world.BoughtSolved(k)) return Arrival(villager, Nursery.Item(kind).Name);
                break;
            }
            int solved = SolvedOfKind(villager, world);
            return solved > 0 ? Tended(villager, solved) : null;
        }

        /// <summary>Puzzles solved under things of this neighbour's kind: grown and bought.</summary>
        public static int SolvedOfKind(int villager, WorldModel world)
        {
            int count = 0;
            int[] made = world.Choices;
            for (int i = 0; i < made.Length; i++)
                if (made[i] == villager && world.TreeSolved(i)) count++;
            int[] bought = world.Bought;
            for (int k = 0; k * 2 < bought.Length; k++)
                if ((int)Nursery.Family(bought[k * 2]) == villager && world.BoughtSolved(k)) count++;
            return count;
        }

        private static string Arrival(int villager, string name)
        {
            switch (villager)
            {
                case 0: return "A " + name + " from the nursery. Give it a season and it will shade the whole bank.";
                case 1: return "That " + name + " from the nursery: I would like to see how it is joined.";
                default: return "The " + name + " from the nursery! The bees will find it by noon.";
            }
        }

        private static string Tended(int villager, int n)
        {
            bool one = n == 1;
            switch (villager)
            {
                case 0:
                    return one ? "You freed the water under one of your trees. It is drinking deeply."
                        : "You have freed the water under " + n + " of your trees. They are drinking deeply.";
                case 1:
                    return one ? "One of the old paintings is whole again. I had forgotten that day."
                        : n + " of the old paintings are whole again. I had forgotten some of those days.";
                default:
                    return one ? "You matched one flower bed. I am saving its seeds for you."
                        : "You have matched " + n + " flower beds. I am saving their seeds for you.";
            }
        }
    }
}
