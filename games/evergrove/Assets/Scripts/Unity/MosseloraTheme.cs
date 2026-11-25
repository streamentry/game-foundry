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
        public static readonly Color Paper = Hex("#F9F2E3");
        public static readonly Color Ink = Hex("#183E34");
        public static readonly Color SecondaryInk = Hex("#49645A");
        public static readonly Color LeafAction = Hex("#276D54");
        public static readonly Color LeafSoft = Hex("#DFEBDD");
        public static readonly Color Sand = Hex("#F2E7D4");
        public static readonly Color RiverWash = Hex("#DCEDEF");
        public static readonly Color EarthInk = Hex("#765F45");
        public static readonly Color WaterInk = Hex("#176C83");
        /// <summary>Dry copper channel; #9C5A38 passes 3:1 on every tile state, incl. HintFocusTile 3.39:1 (painted UI audit 2026-11-23).</summary>
        public static readonly Color DryChannel = Hex("#9C5A38");
        /// <summary>Spring-connected teal channel; #18717F passes 3:1 on every tile state.</summary>
        public static readonly Color WaterChannel = Hex("#18717F");
        public static readonly Color DryTile = Hex("#F4ECDA");
        public static readonly Color WetTile = Hex("#E1F0EC");
        public static readonly Color SolvedTile = Hex("#E4F5EF"); // sunlit water, brighter than wet (I132)
        public static readonly Color TileEdge = Hex("#D5C6A4");
        public static readonly Color CardFill = Hex("#FBF7EC");
        public static readonly Color CardEdge = Hex("#D9CCAB");
        public static readonly Color SagePanel = Hex("#E3EBD6");
        /// <summary>Action tones; white captions pass 4.5:1 on all three.</summary>
        public static readonly Color ForestAction = Hex("#1F5A43");
        public static readonly Color WaterAction = Hex("#1C6F7A");
        public static readonly Color EarthAction = Hex("#A4583A");
        public static readonly Color QuietAction = Hex("#EFE6D2");
        public static readonly Color AnchoredTile = Hex("#F1DEB6");
        public static readonly Color HintRowTile = Hex("#F7E8BB");
        public static readonly Color HintFocusTile = Hex("#E8CB8C");
        /// <summary>Protected-landmark frame and badge ink: heavy bark brown, high contrast on AnchoredTile.</summary>
        public static readonly Color LandmarkFrame = Hex("#4A3524");
        public static readonly Color LandmarkPlate = Hex("#FFF9EC");

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
            // Hints draw honey rings over the tiles (EvergroveApp) and never repaint
            // them: a hint must not hide where the water already reaches (I91).
            if (anchored) return AnchoredTile;
            return solved ? SolvedTile : reached ? WetTile : DryTile;
        }
    }
}
