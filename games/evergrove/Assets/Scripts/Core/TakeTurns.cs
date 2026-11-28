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

        /// <summary>
        /// Whose turn a new board starts on: after each board finished together, the next person
        /// round the table starts, so everyone gets a first move. Alone, or out of range, the first player.
        /// </summary>
        public static int StartingTurn(int boardsTogether, int players)
        {
            if (players <= 1 || players > MaxPlayers || boardsTogether < 0) return 0;
            return boardsTogether % players;
        }

        /// <summary>
        /// Whose turn it is after the table changes size mid-board (someone joins or leaves). The turn
        /// stays with the same seat when it still exists; otherwise it goes back to the first player.
        /// Alone, or out of range, the first player. Never throws.
        /// </summary>
        public static int TurnAfterResize(int turn, int players)
        {
            if (players <= 1 || players > MaxPlayers) return 0;
            if (turn >= 0 && turn < players) return turn;
            return 0;
        }

        /// <summary>I302: the hint glows as an offer on every turn but the first player's.</summary>
        public static bool OfferHint(int turn, int players)
        {
            return players > 1 && turn > 0 && turn < players;
        }

        /// <summary>
        /// The line a screen reader says about whose turn it is, named by the seat's pip colour, e.g.
        /// "Honey's turn, second of three." Alone, "Playing alone."; never throws.
        /// Vision run 20261128-1701 idea 3: the seat is named by its pip colour.
        /// </summary>
        public static string Spoken(int turn, int players)
        {
            if (players <= 1) return "Playing alone.";
            int count = players > MaxPlayers ? MaxPlayers : players;
            int who = ((turn % count) + count) % count;
            return Capitalised(TurnPalette.Name(who)) + "'s turn, " + Ordinal(who) + " of " + CountWord(count) + ".";
        }

        /// <summary>
        /// The line a screen reader says when a board opens with someone other than the first player
        /// starting it, before any move: "Honey starts, second of three." Alone, the plain line; never throws.
        /// Vision run 20261128-1701 idea 3: the seat is named by its pip colour.
        /// </summary>
        public static string SpokenStart(int turn, int players)
        {
            if (players <= 1) return Spoken(turn, players);
            int count = players > MaxPlayers ? MaxPlayers : players;
            int who = ((turn % count) + count) % count;
            return Capitalised(TurnPalette.Name(who)) + " starts, " + Ordinal(who) + " of " + CountWord(count) + ".";
        }

        /// <summary>
        /// Vision run 20261128-1801, idea 3: the line a screen reader says when undo or redo hands the turn
        /// back while playing together, e.g. "Back to Honey, second of three." Alone, exactly what
        /// <see cref="Spoken"/> says. Same colour name, ordinal, count and wrapping as Spoken; never throws.
        /// </summary>
        public static string SpokenBack(int turn, int players)
        {
            if (players <= 1) return Spoken(turn, players);
            int count = players > MaxPlayers ? MaxPlayers : players;
            int who = ((turn % count) + count) % count;
            return "Back to " + Capitalised(TurnPalette.Name(who)) + ", " + Ordinal(who) + " of " + CountWord(count) + ".";
        }

        private static string Ordinal(int who)
        {
            string[] ordinals = { "first", "second", "third", "fourth" };
            return ordinals[who];
        }

        private static string CountWord(int count)
        {
            string[] counts = { "two", "three", "four" };
            return counts[count - 2];
        }

        private static string Capitalised(string word)
        {
            return char.ToUpperInvariant(word[0]) + word.Substring(1);
        }

        /// <summary>The line a screen reader says about the hint offer: said exactly when the offer glows.</summary>
        public static string SpokenHint(int turn, int players)
        {
            return OfferHint(turn, players) ? "Hint offered." : "";
        }

        /// <summary>
        /// Idea 3 (vision run 20261128-1502): the line a screen reader says when a board is solved while
        /// playing together, e.g. "Solved together, all three of you." Alone, nothing. Counts above four
        /// read as four. Never throws.
        /// </summary>
        public static string SpokenSolved(int players)
        {
            if (players <= 1) return "";
            if (players == 2) return "Solved together, both of you.";
            if (players == 3) return "Solved together, all three of you.";
            return "Solved together, all four of you.";
        }
    }
}
