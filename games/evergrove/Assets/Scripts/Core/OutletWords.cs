using System.Text;

namespace Evergrove.Core
{
    /// <summary>
    /// Spoken sentence naming where the water must reach, so a screen-reader player on the
    /// spring knows the outlets without scanning the board. Outlets are every end tile after
    /// the first (the spring), listed in reading order with 1-based row and column.
    /// </summary>
    public static class OutletWords
    {
        /// <summary>
        /// "The water must reach row 3, column 3." for one outlet, two outlets joined by "and",
        /// and three or more as "row a, column b, row c, column d and row e, column f.".
        /// Returns "" for a null puzzle or a puzzle with no outlets.
        /// </summary>
        public static string For(Puzzle puzzle)
        {
            if (puzzle == null || puzzle.Kinds == null || puzzle.Size <= 0) return "";
            int spring = -1;
            var cells = new System.Collections.Generic.List<string>();
            for (int index = 0; index < puzzle.Kinds.Length; index++)
            {
                if (puzzle.Kinds[index] != PipeKind.End) continue;
                if (spring < 0) { spring = index; continue; }
                int row = index / puzzle.Size + 1;
                int column = index % puzzle.Size + 1;
                cells.Add("row " + row + ", column " + column);
            }
            if (cells.Count == 0) return "";
            var text = new StringBuilder("The water must reach ");
            for (int i = 0; i < cells.Count; i++)
            {
                if (i > 0) text.Append(i == cells.Count - 1 ? " and " : ", ");
                text.Append(cells[i]);
            }
            return text.Append('.').ToString();
        }
    }
}
