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
    }
}
