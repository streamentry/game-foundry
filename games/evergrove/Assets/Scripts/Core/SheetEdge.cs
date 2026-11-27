using System;

namespace Evergrove.Core
{
    /// <summary>
    /// Which edge a sheet slides in from, and the easing it uses (checklist 17).
    /// One table decides every sheet's edge, so a new sheet needs a deliberate entry here.
    /// Pure: no UI types, so the rule is testable headless.
    /// </summary>
    public static class SheetEdge
    {
        public enum Side { Bottom, Top, Left, Right }

        /// <summary>The edge a named sheet belongs to. Unknown names throw, so none slips in unplaced.</summary>
        public static Side For(string sheetName)
        {
            switch (sheetName)
            {
                case "Menu": return Side.Bottom;          // rises from the bottom, as it always has
                case "Journal": return Side.Right;        // the book button sits in the right corner
                case "Seed book": return Side.Bottom;
                case "The stall": return Side.Bottom;
                case "Deeper boards": return Side.Bottom;
                case "Name sheet": return Side.Bottom;
                case "Dates sheet": return Side.Bottom;
                case "Travel by boat": return Side.Bottom; // checklist 43: the river stretches rise from the foot
                case "River map": return Side.Bottom;     // checklist 45: the painted scroll of the river rises from the foot
                case "Family": return Side.Bottom;
                case "Letter": return Side.Top;           // the letterbox stands at the top right, under the purse
                default:
                    throw new ArgumentException("No sheet edge for '" + sheetName + "'", nameof(sheetName));
            }
        }

        /// <summary>Ease-out cubic on [0, 1]: 0 at 0, 1 at 1, never decreasing; inputs are clamped.</summary>
        public static float Ease(float t)
        {
            float x = t <= 0f ? 0f : t >= 1f ? 1f : t;
            float inverse = 1f - x;
            return 1f - inverse * inverse * inverse;
        }
    }
}
