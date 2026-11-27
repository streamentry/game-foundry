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
                PuzzleKind kind = item < made.Length
                    ? ThingPuzzle.ForStory(item, (VillageChoice)System.Math.Max(0, System.Math.Min(2, made[item])))
                    : PuzzleKind.Water;
                // The keeper of the puzzle's kind tells you, in words true to that kind (I257).
                if (item < made.Length && OwnerOf(kind) == villager && !world.TreeSolved(item))
                    return world.HelpedBy(item) + " came by and " + HelpedWith(kind) + " Look how it blooms.";
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
                    return "The river runs further than it ever did. " + SeasonCame(GardenSeason.AfterStory(all));
            }
            // Fruit you gave (I228): one true thank-you, only from the neighbour who got the
            // latest, so the tended count is never hidden for good. The count itself is never said.
            if (world.LastFruitTo == villager && world.FruitGivenTo(villager) > 0) return Thanked(villager);
            // I257: by puzzle kind, not by family: since I243 a tree may hold a nonogram.
            PuzzleKind best = PuzzleKind.Water;
            int most = 0;
            foreach (PuzzleKind kind in Kept[villager])
            {
                int n = SolvedOf(world, kind);
                if (n > most) { most = n; best = kind; }
            }
            return most > 0 ? Tended(best, most) : null;
        }

        // Who speaks for which kind of puzzle: Mira the water, Orin the made things, Fen the flowers.
        private static readonly PuzzleKind[][] Kept = {
            new[] { PuzzleKind.Water, PuzzleKind.StreamPath, PuzzleKind.Nonogram },
            new[] { PuzzleKind.Mosaic, PuzzleKind.Plots, PuzzleKind.Mahjong },
            new[] { PuzzleKind.Pairs, PuzzleKind.Sudoku },
        };

        public static int OwnerOf(PuzzleKind kind)
        {
            for (int v = 0; v < Kept.Length; v++)
                if (System.Array.IndexOf(Kept[v], kind) >= 0) return v;
            return 0;
        }

        /// <summary>First solves of things holding this kind of puzzle: grown and bought.</summary>
        public static int SolvedOf(WorldModel world, PuzzleKind kind)
        {
            int count = 0;
            int[] made = world.Choices;
            for (int i = 0; i < made.Length; i++)
                if (world.TreeSolved(i) &&
                    ThingPuzzle.ForStory(i, (VillageChoice)System.Math.Max(0, System.Math.Min(2, made[i]))) == kind) count++;
            int[] bought = world.Bought;
            for (int k = 0; k * 2 < bought.Length; k++)
                if (world.BoughtSolved(k) && ThingPuzzle.ForNursery(bought[k * 2]) == kind) count++;
            return count;
        }

        /// <summary>The words a neighbour uses for a kind (tests check only its keeper says them).</summary>
        public static string KindWord(PuzzleKind kind)
        {
            switch (kind)
            {
                case PuzzleKind.Water: return "freed the water";
                case PuzzleKind.StreamPath: return "led the stream";
                case PuzzleKind.Nonogram: return "by the numbers";
                case PuzzleKind.Mosaic: return "old paintings";
                case PuzzleKind.Plots: return "plots";
                case PuzzleKind.Mahjong: return "garden tiles";
                case PuzzleKind.Pairs: return "flower beds";
                default: return "seed squares";
            }
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

        // I250: the season the new stretch brought, said once, plainly (seasons turn by tending).
        private static string SeasonCame(Season season)
        {
            switch (season)
            {
                case Season.Spring: return "Spring came down with it. The new bank is all blossom.";
                case Season.Autumn: return "Autumn came down with it. The new bank is gold already.";
                case Season.Winter: return "Winter came down with it. Even the new bank is frosted.";
                default: return "Summer came down with it. There's new bank down there, waiting for roots.";
            }
        }

        private static string HelpedWith(PuzzleKind kind)
        {
            switch (kind)
            {
                case PuzzleKind.Water: return "freed the water under one of your things.";
                case PuzzleKind.StreamPath: return "led the stream through one of your meadows.";
                case PuzzleKind.Nonogram: return "planted one of your beds by the numbers.";
                case PuzzleKind.Mosaic: return "mended one of the old paintings with you in mind.";
                case PuzzleKind.Plots: return "staked out a field of plots for you.";
                case PuzzleKind.Mahjong: return "cleared a table of garden tiles for you.";
                case PuzzleKind.Pairs: return "matched one of your flower beds.";
                default: return "sowed a bed of seed squares for you.";
            }
        }

        /// <summary>
        /// The thank-you said right after you hand over fruit (I231), whatever else is
        /// news. Null unless this neighbour really was the last one given fruit.
        /// </summary>
        public static string ThanksFor(int villager, WorldModel world)
        {
            if (world == null || world.Mode != PlayMode.Living || villager < 0 || villager > 2) return null;
            return world.LastFruitTo == villager && world.FruitGivenTo(villager) > 0 ? Thanked(villager) : null;
        }

        private static string Thanked(int villager)
        {
            switch (villager)
            {
                case 0: return "Thank you for the fruit. I'll dry some for the winter.";
                case 1: return "The fruit you left on my bench was the best part of the day.";
                default: return "I made jam with your fruit. There's a jar with your name on it.";
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

        private static string Tended(PuzzleKind kind, int n)
        {
            bool one = n == 1;
            switch (kind)
            {
                case PuzzleKind.Water:
                    return one ? "You freed the water under one of your things. It is drinking deeply."
                        : "You have freed the water under " + n + " of your things. They are drinking deeply.";
                case PuzzleKind.StreamPath:
                    return one ? "You led the stream through a whole meadow. I could hear it from my door."
                        : "You have led the stream through " + n + " meadows. I can hear them from my door.";
                case PuzzleKind.Nonogram:
                    return one ? "You planted a bed by the numbers. Not one sprout out of place."
                        : "You have planted " + n + " beds by the numbers. Not one sprout out of place.";
                case PuzzleKind.Mosaic:
                    return one ? "One of the old paintings is whole again. I had forgotten that day."
                        : n + " of the old paintings are whole again. I had forgotten some of those days.";
                case PuzzleKind.Plots:
                    return one ? "You staked out a field of plots. Tidy work, every bed its own size."
                        : "You have staked out " + n + " fields of plots. Tidy work, every bed its own size.";
                case PuzzleKind.Mahjong:
                    return one ? "You cleared a table of garden tiles. My mother played those on winter evenings."
                        : "You have cleared " + n + " tables of garden tiles. My mother played those on winter evenings.";
                case PuzzleKind.Pairs:
                    return one ? "You matched one flower bed. I am saving its seeds for you."
                        : "You have matched " + n + " flower beds. I am saving their seeds for you.";
                default:
                    return one ? "You sowed a bed of seed squares, not one plant twice in a row."
                        : "You have sown " + n + " beds of seed squares, not one plant twice in a row.";
            }
        }
    }
}
