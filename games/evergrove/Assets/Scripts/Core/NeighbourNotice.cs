namespace Evergrove.Core
{
    /// <summary>
    /// Neighbours who notice (depth spine, Layer 4; founder decision 2026-11-26:
    /// the companion is offline first). Each neighbour keeps to their own kind,
    /// Mira trees, Orin buildings, Fen flowers, and says one line built only from
    /// true facts in the save: a plant you bought of their kind, how many of
    /// their kind's puzzles you have solved, and (Mira) how close more river is.
    /// Never guilt, never "I missed you",
    /// no clock, no randomness. Returns null when there is nothing new and true
    /// to say, so the older story lines speak instead. English only for now.
    /// </summary>
    public static class NeighbourNotice
    {
        public static string Line(int villager, WorldModel world)
        {
            if (world == null || world.Mode != PlayMode.Living || villager < 0 || villager > 2) return null;
            // A cutting from family waiting in the basket comes first (I222): it's
            // easy to miss, and it was sent with care.
            foreach (int kind in world.Gifts)
                if ((int)Nursery.Family(kind) == villager) return Waiting(villager, Nursery.Item(kind).Name);
            // A relative who helped with one of their kind's puzzles (I226): said only while
            // the owner has not solved that thing themselves, naming the helper.
            int[] helped = world.Helped;
            if (helped.Length > 0)
            {
                int item = helped[helped.Length - 1];
                int[] made = world.Choices;
                if (item < made.Length && made[item] == villager && !world.TreeSolved(item))
                    return Helped(villager, world.HelpedBy(item));
            }
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
            // Mira keeps the trees by the water, so she reads the river (I225): what
            // the drops on the mist mean, only when it's true right now.
            if (villager == 0 && world.Completed)
            {
                int all = world.PuzzlesSolved;
                if (GardenMap.TowardNext(world.Choices.Length, true, all, out int have, out int need) && have == need - 1)
                    return "The mist downstream is thinning. One more thing tended and it will lift.";
                if (have == 0 && GardenMap.ReachesFor(world.Choices.Length, true, all) > GardenMap.MaxReaches)
                    return "The river runs further than it ever did. There's new bank down there, waiting for roots.";
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

        // A nursery plant or a planted gift: both are simply new in the garden (the
        // save doesn't record which, so the line claims neither).
        private static string Arrival(int villager, string name)
        {
            switch (villager)
            {
                case 0: return "A " + name + ", new in the garden. Give it a season and it will shade the whole bank.";
                case 1: return "That new " + name + ": I would like to see how it is joined.";
                default: return "A new " + name + "! The bees will find it by noon.";
            }
        }

        private static string Helped(int villager, string helper)
        {
            switch (villager)
            {
                case 0: return helper + " came by and freed the water under one of your trees. Look how it blooms.";
                case 1: return helper + " was here and mended one of the old paintings with you in mind.";
                default: return helper + " matched a flower bed of yours. The bees approve.";
            }
        }

        private static string Waiting(int villager, string name)
        {
            switch (villager)
            {
                case 0: return "Someone sent you a " + name + " cutting. It's waiting in your basket for a good spot.";
                case 1: return "There's a " + name + " in your basket, sent with care. Shall we find it a place?";
                default: return "A " + name + " cutting came for you! It's waiting in your basket.";
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
