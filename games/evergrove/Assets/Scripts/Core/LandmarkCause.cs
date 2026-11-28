using System;

namespace Evergrove.Core
{
    /// <summary>
    /// Names the village choice that keeps the fixed (anchored) pipe in place.
    /// The sentence is shown only when the saved state proves the link: a live,
    /// unfinished garden whose current board variant matches the most recent
    /// choice. Other states, and every non-English locale, keep the generic line.
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
        /// Truthful wrapper for the app. Returns the specific cause only when the saved
        /// board variant proves it; otherwise the generic environmental line.
        /// </summary>
        public static string For(WorldModel world)
        {
            if (world == null) return "";
            string generic = LocalizedVillage.EnvironmentalRule(world);
            if (world.Completed || world.Mode != PlayMode.Living) return generic;

            int[] choices = world.Choices;
            if (choices.Length == 0) return generic;

            int last = choices.Length - 1;
            int chosen = choices[last];
            if (chosen < 0 || chosen > 2) return generic;
            if (world.BoardVariant != chosen + 1) return generic;
            if (GameLocalization.Current != GameLocalization.Fallback) return generic;

            // Choose() records choice i while Round == i (board i just solved); Advance() then
            // applies that choice's variant to board i + 1. So the cause lives on board i.
            return Line((VillageChoice)chosen, PuzzleCatalog.Get(last).Name);
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
