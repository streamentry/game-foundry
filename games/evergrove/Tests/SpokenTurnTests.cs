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
        string[] names = { "Sage", "Honey", "Sky", "Rose" };
        string[] ordinals = { "first", "second", "third", "fourth" };
        string[] counts = { "two", "three", "four" };
        for (int players = 2; players <= 4; players++)
        {
            for (int turn = 0; turn < players; turn++)
            {
                string expected = names[turn] + "'s turn, " + ordinals[turn] + " of " + counts[players - 2] + ".";
                Expect(TakeTurns.Spoken(turn, players) == expected,
                    "turn " + turn + " of " + players + " is \"" + expected + "\"");
            }
        }
        Expect(TakeTurns.Spoken(1, 3) == "Honey's turn, second of three.", "the example line reads as written");
        Expect(TakeTurns.Spoken(0, 2) == "Sage's turn, first of two.", "turn 0 of two is sage, first of two");
        Expect(TakeTurns.Spoken(3, 4) == "Rose's turn, fourth of four.", "turn 3 of four is rose, fourth of four");

        // Idea 3 (vision run 20261128-1801): undo or redo hands the turn back in words.
        Expect(TakeTurns.SpokenBack(1, 3) == "Back to Honey, second of three.", "the handback example reads as written");
        Expect(TakeTurns.SpokenBack(0, 2) == "Back to Sage, first of two.", "handback to sage of two");
        Expect(TakeTurns.SpokenBack(3, 4) == "Back to Rose, fourth of four.", "handback to rose of four");
        Expect(TakeTurns.SpokenBack(2, 1) == TakeTurns.Spoken(2, 1), "alone, handback is the plain line");
        Expect(TakeTurns.SpokenBack(0, 0) == TakeTurns.Spoken(0, 0), "zero players, handback is the plain line");
        Expect(TakeTurns.SpokenBack(-1, 3) == "Back to Sky, third of three.", "handback turn -1 of three is sky");
        Expect(TakeTurns.SpokenBack(int.MinValue, 4).Length > 0, "handback with the smallest turn does not throw");
        Expect(TakeTurns.SpokenBack(int.MaxValue, 4) == "Back to Rose, fourth of four.", "handback with the largest turn wraps to rose");
        Expect(TakeTurns.SpokenBack(0, 9) == "Back to Sage, first of four.", "handback clamps a huge table to four");
        for (int players = 2; players <= 4; players++)
        {
            for (int turn = 0; turn <= 7; turn++)
            {
                string spoken = TakeTurns.Spoken(turn, players);
                string marker = "'s turn, ";
                int cut = spoken.IndexOf(marker, StringComparison.Ordinal);
                Expect(cut > 0, "spoken line has a turn marker");
                string name = spoken.Substring(0, cut);
                string rest = spoken.Substring(cut + marker.Length);
                Expect(TakeTurns.SpokenBack(turn, players) == "Back to " + name + ", " + rest,
                    "handback " + turn + " of " + players + " matches the spoken name and ordinal");
            }
        }

        // Wrap-around, including negative turns and turns at or past the count, never throwing.
        Expect(TakeTurns.Spoken(2, 2) == TakeTurns.Spoken(0, 2), "turn 2 of two wraps to the first player");
        Expect(TakeTurns.Spoken(-1, 3) == "Sky's turn, third of three.", "turn -1 of three is the third player");
        Expect(TakeTurns.Spoken(-4, 4) == TakeTurns.Spoken(0, 4), "turn -4 of four wraps to the first player");
        Expect(TakeTurns.Spoken(int.MinValue, 4).Length > 0, "the smallest turn does not throw");
        Expect(TakeTurns.Spoken(int.MaxValue, 4).Length > 0, "the largest turn does not throw");
        for (int players = 2; players <= 4; players++)
        {
            for (int turn = -9; turn <= 9; turn++)
            {
                int who = ((turn % players) + players) % players;
                Expect(TakeTurns.Spoken(turn, players) == TakeTurns.Spoken(who, players),
                    "turn " + turn + " of " + players + " wraps to seat " + who);
            }
        }
        Expect(TakeTurns.Spoken(0, 9) == "Sage's turn, first of four.", "more than four players clamps to four");
        Expect(TakeTurns.Spoken(3, 9) == "Rose's turn, fourth of four.", "a turn of three in a huge table is rose");

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
        Expect(Array.Exists(lines, l => l.Trim() == "internal int Players { get { return PlayedTogether ? Math.Min(players, MoversNow()) : 1; } }"),
            "Players has a line of its own");
        Expect(!Array.Exists(lines, l => l.Contains("internal bool PlayedTogether") && l.Contains("internal int Players")),
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
