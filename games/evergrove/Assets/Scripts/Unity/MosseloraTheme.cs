using UnityEngine;

namespace Evergrove.UnityClient
{
    /// <summary>
    /// Mosselora's restrained playable palette. The ornate botanical identity
    /// belongs in the brand and world, never behind water-pipe instructions.
    /// Unity scene, puzzle rules and versioned save IDs remain compatible.
    /// See games/evergrove/design.md for contrast and device QA obligations.
    /// </summary>
    internal static class MosseloraTheme
    {
        public static readonly Color Paper = Hex("#F7F4E9");
        public static readonly Color Ink = Hex("#183E34");
        public static readonly Color SecondaryInk = Hex("#49645A");
        public static readonly Color LeafAction = Hex("#276D54");
        public static readonly Color LeafSoft = Hex("#DFEBDD");
        public static readonly Color Sand = Hex("#F2E7D4");
        public static readonly Color RiverWash = Hex("#DCEDEF");
        public static readonly Color EarthInk = Hex("#765F45");
        public static readonly Color WaterInk = Hex("#176C83");
        public static readonly Color DryTile = Hex("#EAE8DC");
        public static readonly Color WetTile = Hex("#D9EEEB");
        public static readonly Color SolvedTile = Hex("#DDECDD");
        public static readonly Color AnchoredTile = Hex("#F1DEB6");
        public static readonly Color HintRowTile = Hex("#F7E8BB");
        public static readonly Color HintFocusTile = Hex("#E8CB8C");

        private static Color Hex(string html)
        {
            ColorUtility.TryParseHtmlString(html, out Color color);
            return color;
        }

        /// <summary>
        /// One rule for new and refreshed tiles, so undo and hints do not
        /// accidentally repaint a protected landmark or false water state.
        /// </summary>
        public static Color TileFill(bool solved, bool reached, bool anchored,
                                     int hintStep, int hintTile, int index, int size)
        {
            if (anchored) return AnchoredTile;
            if (!solved && hintStep == 2 && index == hintTile) return HintFocusTile;
            if (!solved && hintStep == 1 && hintTile >= 0 &&
                index / size == hintTile / size) return HintRowTile;
            return solved ? SolvedTile : reached ? WetTile : DryTile;
        }
    }
}
