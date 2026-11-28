using System;
using System.IO;
using Evergrove.Core;

/// <summary>
/// Vision run 20261128-1401, idea 3: someone joins without resetting the turn. The turn after a table
/// changes size stays in range and follows the rule, and ToggleTogether resizes the table through
/// ResizeTable instead of sending the turn back to the first player (TreePlay.cs stays the same length).
/// </summary>
internal static class JoinTableTests
{
    private static int assertions;

    private static void Expect(bool ok, string message)
    {
        assertions++;
        if (!ok) throw new Exception("JoinTableTests failed: " + message);
    }

    public static int Run()
    {
        assertions = 0;

        // Every turn from -3 to 6 at every table size from -1 to 6: the rule, and always in range.
        for (int players = -1; players <= 6; players++)
        {
            for (int turn = -3; turn <= 6; turn++)
            {
                int got = TakeTurns.TurnAfterResize(turn, players);
                int expected;
                if (players <= 1 || players > TakeTurns.MaxPlayers) expected = 0;
                else if (turn >= 0 && turn < players) expected = turn;
                else expected = 0;
                Expect(got == expected, "turn " + turn + " of " + players + " resizes to " + expected);
                Expect(got >= 0 && got < Math.Max(players, 1),
                    "turn " + turn + " of " + players + " stays in range");
            }
        }

        // Examples: a join keeps the seat; alone and out-of-range tables go to the first player.
        Expect(TakeTurns.TurnAfterResize(1, 3) == 1, "the second player keeps the turn when a third joins");
        Expect(TakeTurns.TurnAfterResize(2, 2) == 0, "a seat that left goes back to the first player");
        Expect(TakeTurns.TurnAfterResize(3, 4) == 3, "the fourth player keeps the turn at a full table");
        Expect(TakeTurns.TurnAfterResize(2, 1) == 0, "going alone sends the turn to the first player");
        Expect(TakeTurns.TurnAfterResize(1, 5) == 0, "more than four players is out of range");

        // Source checks on TreePlay: ToggleTogether resizes the table and no longer resets the turn.
        string treePlay = Script("Unity/TreePlay.cs");
        string turns = Script("Unity/TreePlay.Turns.cs");
        int start = treePlay.IndexOf("private void ToggleTogether()", StringComparison.Ordinal);
        int end = treePlay.IndexOf("private void FlipTurn()", StringComparison.Ordinal);
        Expect(start >= 0 && end > start, "ToggleTogether is found in TreePlay.cs");
        string toggle = start >= 0 && end > start ? treePlay.Substring(start, end - start) : "";
        Expect(!toggle.Contains("turn = 0;"), "ToggleTogether no longer resets the turn to zero");
        Expect(toggle.Contains("ResizeTable();"), "ToggleTogether calls ResizeTable()");
        Expect(turns.Contains("TakeTurns.TurnAfterResize(turn, players)"), "ResizeTable uses TurnAfterResize");

        // TreePlay.cs must not grow (the change is one line for one line).
        int lines = 0;
        foreach (char c in treePlay) if (c == '\n') lines++;
        Expect(lines <= 1099, "TreePlay.cs did not grow past 1099 lines (now " + lines + ")");

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
        throw new Exception("JoinTableTests: " + relative + " not found from " + Directory.GetCurrentDirectory());
    }
}
