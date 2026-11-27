using System;

namespace Evergrove.Core
{
    public enum NoteTarget { Letterbox, Basket, River, Thing }

    /// <summary>
    /// I274: the one daily note from the garden (docs/specs/proposals/notifications-2026-11-27.md).
    /// Chosen when the game closes, from the save alone: only what is truly waiting, first true
    /// one wins (unread letter, cutting in the basket, one drop from more river, a thing still
    /// holding its first puzzle, a favourite to replay). In a neighbour's voice; never pressure.
    /// The garden doesn't change while you're away, so what's true at close is true when read.
    /// </summary>
    public sealed class GardenNote
    {
        public string From;
        public string Body;
        public NoteTarget Target;
        public int Thing = -1;   // story index, or BoughtBase + k

        public const int EarliestHour = 8;   // quiet hours: never before eight...
        public const int LatestHour = 20;    // ...or after eight in the evening
        public const int MaxUnopened = 3;    // three unopened in a row: stop until the game is opened

        private static readonly string[] Names = { "Mira", "Orin", "Fen" };

        public static GardenNote For(WorldModel world, bool unreadLetter)
        {
            if (world == null || world.Mode != PlayMode.Living) return null;
            string[] letters = world.Letters;
            if (unreadLetter && letters.Length > 0)
                return new GardenNote { From = "Orin", Target = NoteTarget.Letterbox,
                    Body = "There's a letter in your box from " + world.LetterFrom[letters.Length - 1] +
                           ". It'll keep until you're ready." };
            int[] gifts = world.Gifts;
            if (gifts.Length > 0)
            {
                int kind = gifts[gifts.Length - 1];
                return new GardenNote { From = Names[(int)Nursery.Family(kind)], Target = NoteTarget.Basket,
                    Body = "A " + Nursery.Item(kind).Name + " cutting is in your basket, ready for a good spot." };
            }
            if (world.Completed && GardenMap.TowardNext(world.Choices.Length, true, world.PuzzlesSolved, out int have, out int need) &&
                have == need - 1)
                return new GardenNote { From = "Mira", Target = NoteTarget.River,
                    Body = "One more thing tended and the mist downstream will lift." };
            // The newest thing still holding its first puzzle: a bought plant, then a grown one.
            int[] bought = world.Bought;
            for (int k = bought.Length / 2 - 1; k >= 0; k--)
                if (!world.BoughtSolved(k))
                    return Holds(Nursery.Item(bought[k * 2]).Name, ThingPuzzle.ForNursery(bought[k * 2]),
                        WorldModel.BoughtBase + k, false);
            int[] made = world.Choices;
            for (int i = made.Length - 1; i >= 0; i--)
                if (!world.TreeSolved(i)) return Holds(Story(world, i), StoryKind(world, i), i, false);
            // Everything tended: a favourite would deal a fresh board (I259).
            for (int i = made.Length - 1; i >= 0; i--)
                if (world.TreeSolved(i)) return Holds(Story(world, i), StoryKind(world, i), i, true);
            return null;
        }

        private static string Story(WorldModel world, int i)
        {
            int[] made = world.Choices;
            return GardenCatalog.For(world.Garden, Math.Min(i, GardenCatalog.ChaptersPerGarden - 1),
                (VillageChoice)Math.Max(0, Math.Min(2, made[i]))).Name;
        }

        private static PuzzleKind StoryKind(WorldModel world, int i)
        {
            return ThingPuzzle.ForStory(i, (VillageChoice)Math.Max(0, Math.Min(2, world.Choices[i])));
        }

        private static GardenNote Holds(string name, PuzzleKind kind, int thing, bool replay)
        {
            string what = KindPhrase(kind);
            return new GardenNote {
                From = Names[NeighbourNotice.OwnerOf(kind)], Target = NoteTarget.Thing, Thing = thing,
                Body = replay ? "Your " + name + " would deal you a fresh board of " + what + ", if you fancy one."
                              : "Your " + name + " still holds a puzzle of " + what + ", whenever you fancy it." };
        }

        private static string KindPhrase(PuzzleKind kind)
        {
            switch (kind)
            {
                case PuzzleKind.Water: return "water channels";
                case PuzzleKind.StreamPath: return "the stream to lead";
                case PuzzleKind.Nonogram: return "planting by numbers";
                case PuzzleKind.Mosaic: return "an old painting to mend";
                case PuzzleKind.Plots: return "garden plots";
                case PuzzleKind.Mahjong: return "garden tiles";
                case PuzzleKind.Pairs: return "flower pairs";
                case PuzzleKind.Sowing: return "beds to sow";
                case PuzzleKind.Sums: return "seed sums";
                default: return "seed squares";
            }
        }

        /// <summary>The next moment at the player's hour (clamped to daytime), at least half an hour away.</summary>
        public static DateTime NextTime(DateTime now, int hour)
        {
            int h = Math.Max(EarliestHour, Math.Min(LatestHour, hour));
            var at = new DateTime(now.Year, now.Month, now.Day, h, 0, 0, now.Kind);
            if (at <= now.AddMinutes(30)) at = at.AddDays(1);
            return at;
        }

        public static bool MaySchedule(int unopenedInARow) { return unopenedInARow < MaxUnopened; }
    }
}
