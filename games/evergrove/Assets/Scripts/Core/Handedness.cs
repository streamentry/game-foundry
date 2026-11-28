using System;

namespace Evergrove.Core
{
    /// <summary>Which hand holds the phone. Unknown until the player answers (checklist 5).</summary>
    public enum Hand { Unknown, Left, Right }

    /// <summary>The bottom corner the menu button sits in.</summary>
    public enum HandCorner { BottomLeft, BottomRight }

    /// <summary>
    /// Checklist 5, remember the hand: the one answer to "which hand do you hold your phone with?", kept on the
    /// device (never in the save), and the layout it decides. Unknown keeps the layout from before the question
    /// (menu bottom-left, the sideways control panel on the right). Pure: no engine types.
    /// </summary>
    public static class Handedness
    {
        /// <summary>PlayerPrefs key holding the hand: "left", "right" or "unknown".</summary>
        public const string HandKey = "evergrove:hand";

        /// <summary>PlayerPrefs key: 1 once the question was asked (answered or declined).</summary>
        public const string AskedKey = "evergrove:hand-asked";

        /// <summary>The text stored under <see cref="HandKey"/> for a hand.</summary>
        public static string Serialise(Hand hand)
        {
            switch (hand)
            {
                case Hand.Left: return "left";
                case Hand.Right: return "right";
                default: return "unknown";
            }
        }

        /// <summary>The hand stored as text. Anything missing or unrecognised is Unknown.</summary>
        public static Hand Parse(string text)
        {
            if (text == null) return Hand.Unknown;
            switch (text.Trim().ToLowerInvariant())
            {
                case "left": return Hand.Left;
                case "right": return Hand.Right;
                default: return Hand.Unknown;
            }
        }

        /// <summary>The bottom corner for the menu button: right for a right hand, otherwise bottom-left.</summary>
        public static HandCorner MenuCorner(Hand hand)
        {
            return hand == Hand.Right ? HandCorner.BottomRight : HandCorner.BottomLeft;
        }

        /// <summary>
        /// Sideways, the puzzle card's control panel sits on the thumb's side: right for a right hand, left for a
        /// left hand, right while unknown. Upright there is no side panel, so the answer is always true there.
        /// </summary>
        public static bool PanelOnRight(Hand hand, bool sideways)
        {
            if (!sideways) return true;
            return hand != Hand.Left;
        }

        /// <summary>The question is asked only once: while the hand is unknown and it was never asked before.</summary>
        public static bool ShouldAsk(Hand stored, bool askedBefore)
        {
            return stored == Hand.Unknown && !askedBefore;
        }

        /// <summary>The Settings token cycles Right, then Left, then Right again. Unknown moves to Right.</summary>
        public static Hand Next(Hand hand)
        {
            return hand == Hand.Right ? Hand.Left : Hand.Right;
        }
    }
}
