using System.Collections.Generic;
using System.Text;

namespace Evergrove.Core
{
    /// <summary>
    /// Spoken description of which way a pipe tile opens, so a screen-reader player can
    /// tell an orientation apart without seeing the board. Directions are read clockwise
    /// from the top (up, right, down, left) and match PipeRules.HasPort.
    /// </summary>
    public static class TileOpenings
    {
        private static readonly string[] Names = { "up", "right", "down", "left" };

        /// <summary>
        /// "Opens up." / "Opens up and right." for a valid rotation, or "" when the rotation
        /// is outside 1..OrientationCount or the tile has no port in that orientation.
        /// </summary>
        public static string Words(PipeKind kind, int rotation)
        {
            if (rotation < 1 || rotation > PipeRules.OrientationCount(kind)) return "";
            var open = new List<string>();
            for (int direction = 0; direction < 4; direction++)
                if (PipeRules.HasPort(kind, rotation, direction)) open.Add(Names[direction]);
            if (open.Count == 0) return "";
            var text = new StringBuilder("Opens ");
            for (int i = 0; i < open.Count; i++)
            {
                if (i > 0) text.Append(i == open.Count - 1 ? " and " : ", ");
                text.Append(open[i]);
            }
            return text.Append('.').ToString();
        }

        /// <summary>
        /// Long-press caption for a water tile in English: the action, then the openings.
        /// "Turn this tile. Opens up and right." / "This tile stays where it is. Opens left and down."
        /// When Words is "" (invalid rotation or no port) the bare caption is returned unchanged.
        /// </summary>
        public static string Caption(bool anchored, PipeKind kind, int rotation)
        {
            string words = Words(kind, rotation);
            if (anchored)
                return words.Length == 0
                    ? "This tile stays where it is."
                    : "This tile stays where it is. " + words;
            return words.Length == 0
                ? "Turn this tile"
                : "Turn this tile. " + words;
        }
    }
}
