using System;
using System.IO;
using Evergrove.Core;

/// <summary>
/// Pins the spoken turn line: alone or one player says "Playing alone.", each turn names its
/// ordinal and the count, turns wrap round the table, every line stays short, and TreePlay.cs
/// speaks the turn through one persistent caption rather than adding a node per turn.
/// </summary>
internal static class SpokenTurnTests
{
    private static int assertions;

    private static void Expect(bool ok, string message)
    {
        assertions++;
        if (!ok) throw new Exception("SpokenTurnTests failed: " + message);
    }

    public static int Run()
    {
        assertions = 0;

        // Alone, or a player count that cannot be a table.
        Expect(TakeTurns.Spoken(0, 1) == "Playing alone.", "one player is playing alone");
        Expect(TakeTurns.Spoken(5, 0) == "Playing alone.", "zero players is playing alone");
        Expect(TakeTurns.Spoken(0, -2) == "Playing alone.", "negative players is playing alone");

        // Every turn for two to four players.
        string[] ordinals = { "First", "Second", "Third", "Fourth" };
        string[] counts = { "two", "three", "four" };
        for (int players = 2; players <= 4; players++)
        {
            for (int turn = 0; turn < players; turn++)
            {
                string expected = ordinals[turn] + " player's turn, of " + counts[players - 2] + ".";
                Expect(TakeTurns.Spoken(turn, players) == expected,
                    "turn " + turn + " of " + players + " is \"" + expected + "\"");
            }
        }
        Expect(TakeTurns.Spoken(1, 3) == "Second player's turn, of three.", "the example line reads as written");

        // Wrap-around, including negative turns.
        Expect(TakeTurns.Spoken(2, 2) == TakeTurns.Spoken(0, 2), "turn 2 of two wraps to the first player");
        Expect(TakeTurns.Spoken(-1, 3) == "Third player's turn, of three.", "turn -1 of three is the third player");
        Expect(TakeTurns.Spoken(0, 9) == "First player's turn, of four.", "more than four players clamps to four");

        // Every line is short enough for a caption.
        for (int players = -1; players <= 9; players++)
        {
            for (int turn = -5; turn <= 5; turn++)
            {
                Expect(TakeTurns.Spoken(turn, players).Length < 40, "line under 40 characters");
            }
        }

        // Source check: TreePlay speaks through one persistent caption, and Players has its own line.
        string treePlay = Script("Unity/TreePlay.cs");
        string turns = Script("Unity/TreePlay.Turns.cs");
        Expect(treePlay.Contains("SpeakTurn(disc.transform);"), "PaintTurn speaks the turn");
        Expect(turns.Contains("TakeTurns.Spoken(turn, players)"), "TreePlay speaks the turn through TakeTurns.Spoken");
        Expect(turns.Contains("token.Find(\"Turn caption\")"), "TreePlay reuses its single turn caption");
        string[] lines = treePlay.Split('\n');
        Expect(Array.Exists(lines, l => l.Trim() == "internal int Players { get { return players; } }"),
            "Players has a line of its own");
        Expect(!Array.Exists(lines, l => l.Contains("PlayedTogether") && l.Contains("internal int Players")),
            "Players is not on the same line as PlayedTogether");

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
        throw new Exception("SpokenTurnTests: " + relative + " not found from " + Directory.GetCurrentDirectory());
    }
}
