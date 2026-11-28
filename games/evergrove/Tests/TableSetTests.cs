using System;
using System.IO;
using Evergrove.Core;

/// <summary>
/// The table stays set: once a family sets its number of players, the next board opens at that
/// number, and its first move rotates round the table (TakeTurns.StartingTurn). Session memory only;
/// the source checks pin that nothing is saved and that TreePlay.cs does not grow.
/// </summary>
internal static class TableSetTests
{
    private static int assertions;

    private static void Expect(bool ok, string message)
    {
        assertions++;
        if (!ok) throw new Exception("TableSetTests failed: " + message);
    }

    public static int Run()
    {
        assertions = 0;
        RotatesRoundTheTable();
        AloneAlwaysStartsWithTheFirstPlayer();
        OutOfRangeNeverThrows();
        SourceWiring();
        return assertions;
    }

    private static void RotatesRoundTheTable()
    {
        for (int players = 2; players <= TakeTurns.MaxPlayers; players++)
            for (int boards = 0; boards <= 8; boards++)
                Expect(TakeTurns.StartingTurn(boards, players) == boards % players,
                    "board " + boards + " of " + players + " players starts with " + (boards % players));
    }

    private static void AloneAlwaysStartsWithTheFirstPlayer()
    {
        for (int boards = 0; boards <= 8; boards++)
            Expect(TakeTurns.StartingTurn(boards, 1) == 0, "alone, board " + boards + " starts with the first player");
    }

    private static void OutOfRangeNeverThrows()
    {
        int[] players = { int.MinValue, -1, 0, 5, int.MaxValue };
        int[] boards = { int.MinValue, -1, 0, 3, int.MaxValue };
        foreach (int p in players)
            foreach (int b in boards)
            {
                int start = TakeTurns.StartingTurn(b, p);
                if (p < 2 || p > TakeTurns.MaxPlayers || b < 0)
                    Expect(start == 0, "out of range (" + b + ", " + p + ") starts with the first player");
                else
                    Expect(start >= 0 && start < p, "in range (" + b + ", " + p + ") stays within the table");
            }
        Expect(TakeTurns.StartingTurn(int.MaxValue, 4) == int.MaxValue % 4, "a long session still rotates");
    }

    private static void SourceWiring()
    {
        string unity = Folder();
        string turns = File.ReadAllText(Path.Combine(unity, "TreePlay.Turns.cs"));
        string play = File.ReadAllText(Path.Combine(unity, "TreePlay.cs"));

        Expect(turns.Contains("private static int tablePlayers = 1;"), "the table's size is session memory in TreePlay.Turns.cs");
        Expect(turns.Contains("private static int boardsTogether;"), "the boards finished together are session memory");
        Expect(turns.Contains("TakeTurns.StartingTurn(boardsTogether, tablePlayers)"), "a new board opens on the rotated turn");
        Expect(play.Contains("private int players = tablePlayers;"), "a new board opens with the table's size");
        Expect(play.Contains("private int turn = OpeningTurn();"), "a new board's first turn comes from the table");
        Expect(play.Contains("RememberTable();") && play.Contains("ledger.Clear(); RememberTable();"),
            "ToggleTogether remembers the table");
        Expect(play.Contains("ZoomDone(); TableFinished();"), "a solved board counts toward the rotation");
        Expect(turns.Contains("ledger.Record(0, turn)"), "the undo ledger starts with the board's first turn");

        foreach (string file in Directory.GetFiles(unity, "TreePlay*.cs"))
        {
            string text = File.ReadAllText(file);
            foreach (string line in text.Split('\n'))
                Expect(!(line.Contains("PlayerPrefs") && line.ToLowerInvariant().Contains("\"table")),
                    Path.GetFileName(file) + " must not save the table");
            Expect(!text.Contains("WorldModel"), Path.GetFileName(file) + " does not touch the saved world");
        }

        int lines = 0;
        foreach (char c in play) if (c == '\n') lines++;
        Expect(lines <= 1100, "TreePlay.cs stays within 1100 lines (now " + lines + ")");
    }

    private static string Folder()
    {
        foreach (string start in new[] { Directory.GetCurrentDirectory(), AppContext.BaseDirectory })
        {
            string dir = Path.GetFullPath(start);
            while (dir != null)
            {
                foreach (string rel in new[] { "games/evergrove/Assets/Scripts/Unity", "Assets/Scripts/Unity" })
                    if (File.Exists(Path.Combine(dir, rel, "TreePlay.cs"))) return Path.Combine(dir, rel);
                dir = Path.GetDirectoryName(dir);
            }
        }
        throw new Exception("TableSetTests could not locate the Unity scripts.");
    }
}
