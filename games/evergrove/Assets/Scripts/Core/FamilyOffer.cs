using System;

namespace Evergrove.Core
{
    /// <summary>
    /// Checklist 8, no sign-up wall: family features are an offer, never a gate. They appear from the
    /// third day played, or at once when the garden already has family activity (someone arrived through a
    /// relative's parcel). Pure rules; the Unity layer decides where each entry point shows.
    /// Solo play never needs any of this, and nothing here touches a network or an account.
    /// </summary>
    public static class FamilyOffer
    {
        /// <summary>The first day played on which family features are offered to a garden with no family activity.</summary>
        public const int FirstDay = 3;

        /// <summary>The one-time hub offer. Gentle, optional, and never needed to play.</summary>
        public const string Line = "If you'd like, you can send a letter or a gift to family. It's never needed to play.";

        /// <summary>PlayerPrefs key for the once-only flag. Stays on this device; nothing is sent.</summary>
        public const string OfferedKey = "evergrove:family-offer-shown";

        // Neighbours write notes, leave lanterns and give cuttings. They are not family, and their notes
        // share the letterbox with family letters, so they must not open the family features early.
        private static readonly string[] Neighbours = { "Mira", "Orin", "Fen" };

        /// <summary>True when family features may be shown: from day 3 on, or at once with family activity.</summary>
        public static bool Visible(int daysPlayed, bool hasFamilyActivity)
        {
            return hasFamilyActivity || daysPlayed >= FirstDay;
        }

        /// <summary>True for a village neighbour's name (not family).</summary>
        public static bool IsNeighbour(string name)
        {
            return name != null && Array.IndexOf(Neighbours, name) >= 0;
        }

        /// <summary>True for a memory-shelf keepsake that only family can give (help from family, a lantern left).</summary>
        public static bool IsFamilyKeepsake(string art)
        {
            return art == "food-basket" || art == "build-96";
        }

        /// <summary>
        /// True when the garden already has family activity: a letter from family, a parcel sent or opened,
        /// help from family, or a lantern. Neighbours' notes do not count. A puzzle-only garden has none.
        /// </summary>
        public static bool HasActivity(WorldModel world)
        {
            if (world == null || world.Mode != PlayMode.Living) return false;
            string[] letters = world.Letters;
            string[] from = world.LetterFrom;
            for (int i = 0; i < letters.Length; i++)
            {
                string sender = i < from.Length ? from[i] : null;
                if (!IsNeighbour(sender)) return true;
            }
            return world.Helped.Length > 0 || world.Lanterns.Length > 0 || world.Export().parcelIds.Length > 0;
        }
    }
}
