namespace Evergrove.Core
{
    /// <summary>
    /// I303 (idea 39, after the four sides of a mahjong table and a ludo board): take turns
    /// on one device with up to four people. One token cycles alone, two, three, four, then
    /// alone again. Nothing about who plays is stored; only that it was played together (I263).
    /// </summary>
    public static class TakeTurns
    {
        public const int MaxPlayers = 4;

        /// <summary>The player count after a tap on the take-turns token.</summary>
        public static int NextPlayers(int players)
        {
            if (players < 1) return 2;
            if (players >= MaxPlayers) return 1;
            return players + 1;
        }

        /// <summary>Whose turn follows, round the table in order.</summary>
        public static int Next(int turn, int players)
        {
            if (players <= 1) return 0;
            return (turn + 1) % players;
        }

        /// <summary>I302: the hint glows as an offer on every turn but the first player's.</summary>
        public static bool OfferHint(int turn, int players)
        {
            return players > 1 && turn > 0 && turn < players;
        }
    }
}
