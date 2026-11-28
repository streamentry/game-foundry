using System;

namespace Evergrove.Core
{
    /// <summary>The two actions a tapped thing offers on the hub map.</summary>
    public enum ThingAction { Play, Move }

    /// <summary>
    /// Checklist 13: the taps a player makes to reach a thing's actions, as data. The change is about reach,
    /// not tap count: Play used to sit in a bottom token (far from a thumb resting on the garden) and now sits
    /// beside the tapped thing. Each count starts from the hub at rest and includes the tap on the thing.
    /// Before: Play is the thing, then the bottom token (2); Move is the thing, then free land (2).
    /// After: Play is the thing, then the token beside it (2); Move is the thing, then free land (2).
    /// The Move token is optional guidance; using it adds one tap (3).
    /// </summary>
    public static class ThingTaps
    {
        public static int Before(ThingAction action)
        {
            switch (action)
            {
                case ThingAction.Play: return 2; // thing, then the bottom "Play its puzzle" token
                case ThingAction.Move: return 2; // thing, then free land
                default: throw new ArgumentOutOfRangeException("action");
            }
        }

        public static int After(ThingAction action)
        {
            switch (action)
            {
                case ThingAction.Play: return 2; // thing, then the "Play its puzzle" token beside it
                case ThingAction.Move: return 2; // thing, then free land
                default: throw new ArgumentOutOfRangeException("action");
            }
        }

        /// <summary>After the change, when the player uses the Move token for its hint (thing, token, free land).</summary>
        public static int AfterWithMoveToken(ThingAction action)
        {
            if (action == ThingAction.Move) return 3;
            return After(action);
        }
    }
}
