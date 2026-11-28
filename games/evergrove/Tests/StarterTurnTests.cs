using System;
using System.IO;
using Evergrove.Core;

/// <summary>
/// Pins the starter announcement and the spoken hint offer: a board that opens on someone other than
/// the first player says who starts (before any move), the hint offer says "Hint offered." exactly when
/// it glows, nothing throws on odd input, and TreePlay.cs reuses one hint caption rather than a node per turn.
/// </summary>
internal static class StarterTurnTests
{
    private static int assertions;

    private static void Expect(bool ok, string message)
    {
        assertions++;
        if (!ok) throw new Exception("StarterTurnTests failed: " + message);
    }

    public static int Run()
    {
        assertions = 0;

        string[] ordinals = { "First", "Second", "Third", "Fourth" };
        string[] counts = { "two", "three", "four" };

        // The starter line for every table of two to four, every starting turn.
        for (int players = 2; players <= 4; players++)
        {
            for (int turn = 0; turn < players; turn++)
            {
                string expected = ordinals[turn] + " player starts, of " + counts[players - 2] + ".";
                Expect(TakeTurns.SpokenStart(turn, players) == expected,
                    "start " + turn + " of " + players + " is \"" + expected + "\"");
            }
        }
        Expect(TakeTurns.SpokenStart(1, 3) == "Second player starts, of three.", "the example line reads as written");

        // Alone, or a player count that cannot be a table: the plain alone line.
        Expect(TakeTurns.SpokenStart(0, 1) == "Playing alone.", "one player is playing alone");
        Expect(TakeTurns.SpokenStart(3, 1) == "Playing alone.", "one player ignores the turn");
        Expect(TakeTurns.SpokenStart(0, 0) == "Playing alone.", "zero players is playing alone");
        Expect(TakeTurns.SpokenStart(2, -1) == "Playing alone.", "negative players is playing alone");

        // More than four players clamps to four; odd turns wrap round the table, never throw.
        Expect(TakeTurns.SpokenStart(0, 9) == "First player starts, of four.", "more than four players clamps to four");
        Expect(TakeTurns.SpokenStart(-1, 3) == "Third player starts, of three.", "turn -1 of three is the third player");
        Expect(TakeTurns.SpokenStart(-5, 2) == TakeTurns.SpokenStart(1, 2), "a negative turn wraps without throwing");
        Expect(TakeTurns.SpokenStart(int.MinValue, 4).Length > 0, "the smallest turn does not throw");
        Expect(TakeTurns.SpokenStart(int.MaxValue, 4).Length > 0, "the largest turn does not throw");
        Expect(TakeTurns.SpokenStart(0, int.MaxValue) == "First player starts, of four.", "a huge table clamps to four");
        Expect(TakeTurns.SpokenStart(0, int.MinValue) == "Playing alone.", "a huge negative table is playing alone");

        // Every starter line is short enough for a caption.
        for (int players = -1; players <= 9; players++)
        {
            for (int turn = -5; turn <= 5; turn++)
            {
                Expect(TakeTurns.SpokenStart(turn, players).Length < 40, "starter line under 40 characters");
            }
        }

        // The hint offer is spoken exactly when the offer glows.
        for (int players = 1; players <= 4; players++)
        {
            for (int turn = -1; turn <= 4; turn++)
            {
                string words = TakeTurns.SpokenHint(turn, players);
                bool offer = TakeTurns.OfferHint(turn, players);
                Expect((words == "Hint offered.") == offer,
                    "hint line matches the offer for turn " + turn + " of " + players);
                Expect(words == "" || offer, "no hint line without an offer");
            }
        }
        Expect(TakeTurns.SpokenHint(1, 2) == "Hint offered.", "second player of two is offered a hint");
        Expect(TakeTurns.SpokenHint(0, 2) == "", "the first player is not offered a hint");
        Expect(TakeTurns.SpokenHint(int.MinValue, int.MaxValue) == "", "odd input gives no hint line");

        // Source check: the starter and the hint offer each keep one persistent caption in TreePlay.
        string turns = Script("Unity/TreePlay.Turns.cs");
        string treePlay = Script("Unity/TreePlay.cs");
        Expect(turns.Contains("TakeTurns.SpokenStart(turn, players)"), "SpeakTurn announces the starter through SpokenStart");
        Expect(turns.Contains("TakeTurns.SpokenHint(turn, players)"), "the hint caption is set through SpokenHint");
        Expect(turns.Contains("token.Find(\"Hint offer caption\")"), "TreePlay reuses its single hint caption");
        Expect(turns.Contains("if (hintToken != null) SpeakHint(hintToken.transform);"), "each turn paint speaks the hint offer");
        Expect(treePlay.Contains("OpenTurn();") && turns.Contains("BloomPulse.Add(pips[turn].gameObject, 0f, 0.12f)"), "the starter pip pulses once when the board opens");

        return assertions;
    }

    private static string Script(string relative)
    {
        foreach (string start in new[] { Directory.GetCurrentDirectory(), AppContext.BaseDirectory })
        {
            for (var dir = new DirectoryInfo(start); dir != null; dir = dir.Parent)
            {
                string path = Path.Combine(dir.FullName, "games", "evergrove", "Assets", "Scripts", relative);
                if (File.Exists(path)) return File.ReadAllText(path);
            }
        }
        throw new Exception("StarterTurnTests: " + relative + " not found from " + Directory.GetCurrentDirectory());
    }
}
