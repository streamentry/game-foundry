namespace Evergrove.Core
{
    /// <summary>The top corners a close / back control may sit in.</summary>
    public enum BackCorner
    {
        TopLeft,
        TopRight,
    }

    /// <summary>
    /// The one rule for every close / back control (sheet, card or panel): the same top-right
    /// corner, the same inset, the same token size, and the same cancel icon. Every site reads
    /// these numbers; the Unity client places the control with them (EvergroveApp.PlaceBack).
    /// The tests audit each site against this rule.
    /// </summary>
    public static class BackPlacement
    {
        /// <summary>The corner the control sits in. Most sheets already keep it top-right.</summary>
        public const BackCorner Corner = BackCorner.TopRight;

        /// <summary>Gap from the panel's top edge and its side edge, in points.</summary>
        public const float Inset = 12f;

        /// <summary>Width and height of the token, in points. Never below a 56 pt target.</summary>
        public const float Size = 56f;

        /// <summary>Width kept clear of the content beside the control, so nothing sits under it.</summary>
        public const float Reserve = Size + Inset;
    }
}
