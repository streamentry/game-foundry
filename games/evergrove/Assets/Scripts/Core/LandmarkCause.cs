using System;

namespace Evergrove.Core
{
    /// <summary>
    /// Names the village choice that keeps the fixed (anchored) pipe in place.
    /// The sentence is shown only when the saved state proves the link: a live,
    /// unfinished garden whose current board variant matches the most recent
    /// choice. Other states, and every non-English locale, keep the generic line.
    /// When proven, the sentence names the specific tree, flower or building the
    /// choice grew in this garden (GardenCatalog); otherwise it names the family.
    /// </summary>
    public static class LandmarkCause
    {
        private const string Separator = " · ";

        /// <summary>
        /// Pure sentence. <paramref name="boardName"/> is the board where the choice was made;
        /// any " · " suffix is dropped, and a blank name leaves out the board reference.
        /// </summary>
        public static string Line(VillageChoice choice, string boardName)
        {
            string board = BoardOnly(boardName);
            string from = board.Length == 0 ? "" : " from " + board;
            switch (choice)
            {
                case VillageChoice.ShadeTree:
                    return "Your shade tree" + from + " roots this pipe in place.";
                case VillageChoice.Bridge:
                    return "Your footbridge" + from + " holds this pipe in place.";
                case VillageChoice.Flowers:
                    return "Your wildflowers" + from + " keep this pipe in place.";
                default:
                    throw new ArgumentOutOfRangeException(nameof(choice));
            }
        }

        /// <summary>
        /// Pure sentence naming the specific item (for example "quince tree" or "lavender").
        /// A blank <paramref name="itemName"/> falls back to <see cref="Line(VillageChoice, string)"/>.
        /// Passive voice, so plural and mass names read correctly.
        /// </summary>
        public static string Line(VillageChoice choice, string boardName, string itemName)
        {
            if (string.IsNullOrWhiteSpace(itemName)) return Line(choice, boardName);
            string board = BoardOnly(boardName);
            string from = board.Length == 0 ? "" : " from " + board;
            string item = itemName.Trim();
            switch (choice)
            {
                case VillageChoice.ShadeTree:
                    return "This pipe is rooted in place by your " + item + from + ".";
                case VillageChoice.Bridge:
                    return "This pipe is held in place by your " + item + from + ".";
                case VillageChoice.Flowers:
                    return "This pipe is kept in place by your " + item + from + ".";
                default:
                    throw new ArgumentOutOfRangeException(nameof(choice));
            }
        }

        /// <summary>
        /// The proven, item-named line, or "" when the saved state does not prove the cause
        /// (null world, fresh or puzzle-only or matched worlds, completed gardens, a board
        /// variant that does not match the last choice, or a non-English locale).
        /// </summary>
        public static string Specific(WorldModel world)
        {
            if (world == null) return "";
            if (world.Completed || world.Mode != PlayMode.Living) return "";

            int[] choices = world.Choices;
            if (choices.Length == 0) return "";

            int last = choices.Length - 1;
            int chosen = choices[last];
            if (chosen < 0 || chosen > 2) return "";
            if (world.BoardVariant != chosen + 1) return "";
            if (GameLocalization.Current != GameLocalization.Fallback) return "";

            // Choose() records choice i while Round == i (board i just solved); Advance() then
            // applies that choice's variant to board i + 1. So the cause lives on board i.
            var choice = (VillageChoice)chosen;
            string board = PuzzleCatalog.Get(last).Name;
            if (world.Garden < 0 || last >= GardenCatalog.ChaptersPerGarden)
                return Line(choice, board);
            return Line(choice, board, GardenCatalog.For(world.Garden, last, choice).Name);
        }

        /// <summary>
        /// Truthful wrapper for the app. Returns the specific, item-named cause when the saved
        /// board variant proves it; otherwise the generic environmental line.
        /// </summary>
        public static string For(WorldModel world)
        {
            if (world == null) return "";
            string specific = Specific(world);
            return specific.Length > 0 ? specific : LocalizedVillage.EnvironmentalRule(world);
        }

        private static string BoardOnly(string boardName)
        {
            if (string.IsNullOrWhiteSpace(boardName)) return "";
            int cut = boardName.IndexOf(Separator, StringComparison.Ordinal);
            string name = cut >= 0 ? boardName.Substring(0, cut) : boardName;
            return name.Trim();
        }
    }
}
