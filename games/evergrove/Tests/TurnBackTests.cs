using System;
using System.IO;
using Evergrove.Core;

/// <summary>
/// Vision run 20261128-1001, idea 3: undo hands the turn back. The turn is looked up by undo step
/// (TurnLedger), because not every recorded move passes the turn: lifting a piece, a reshuffle and
/// multi-step hints record steps without a turn. Also pins the Unity wiring by source text.
/// </summary>
internal static class TurnBackTests
{
    private static int assertions;

    private static void Expect(bool ok, string message)
    {
        assertions++;
        if (!ok) throw new Exception("TurnBackTests failed: " + message);
    }

    public static int Run()
    {
        assertions = 0;
        RoundTheTable();
        StepsWithoutATurn();
        NewMoveForgetsTheFuture();
        SourceWiring();
        return assertions;
    }

    // Three players, one move each: undo walks the turn back one player per step, redo forward.
    private static void RoundTheTable()
    {
        var ledger = new TurnLedger();
        Expect(ledger.At(0) == 0, "first player before any move");
        int turn = 0, step = 0;
        for (int i = 0; i < 5; i++)
        {
            step++; turn = TakeTurns.Next(turn, 3);
            ledger.Record(step, turn);
        }
        for (int s = 5; s >= 0; s--) Expect(ledger.At(s) == s % 3, "after " + s + " moves it is player " + (s % 3) + "'s turn");
    }

    // A lift (no turn) at step 2, then a hint that records steps 4-6 and passes the turn once.
    private static void StepsWithoutATurn()
    {
        var ledger = new TurnLedger();
        ledger.Record(1, 1);          // player 0 moved
        /* step 2: a lift, no turn */
        ledger.Record(3, 0);          // player 1 placed the lifted piece
        /* steps 4 and 5: hint taps */
        ledger.Record(6, 1);          // the hint's last tap passes the turn once
        Expect(ledger.At(2) == 1, "undoing to a lift keeps the turn of the player who lifted");
        Expect(ledger.At(3) == 0, "undoing the hint returns the turn to the player who asked");
        Expect(ledger.At(4) == 0 && ledger.At(5) == 0, "part of a hint does not pass the turn");
        Expect(ledger.At(6) == 1, "redo through the hint passes it again");
        Expect(ledger.At(99) == 1, "beyond the last note keeps the last turn");
        Expect(ledger.At(-3) == 0, "a negative step is the first player");
    }

    private static void NewMoveForgetsTheFuture()
    {
        var ledger = new TurnLedger();
        ledger.Record(1, 1); ledger.Record(2, 0); ledger.Record(3, 1);
        ledger.Record(2, 1);          // undid to 1, a different move reached 2
        Expect(ledger.At(3) == 1 && ledger.At(2) == 1, "an abandoned redo step is forgotten");
        ledger.Clear();
        Expect(ledger.At(2) == 0, "clearing restarts with the first player");
        ledger.Record(-1, 1);
        Expect(ledger.At(0) == 1, "a negative step is noted as step 0");
    }

    private static void SourceWiring()
    {
        string unity = Folder();
        string kit = File.ReadAllText(Path.Combine(unity, "BoardKit.cs"));
        string turns = File.ReadAllText(Path.Combine(unity, "TreePlay.Turns.cs"));
        string play = File.ReadAllText(Path.Combine(unity, "TreePlay.cs"));
        Expect(kit.Contains("Track(history)"), "the undo row tells its TreePlay which history it steps");
        Expect(kit.Split(new[] { "TurnToStep()" }, StringSplitOptions.None).Length == 3, "undo and redo both hand the turn back");
        Expect(turns.Contains("internal void TurnToStep()") && turns.Contains("ledger.Record(tracked.UndoSteps, turn)"), "TreePlay looks the turn up by step");
        Expect(play.Contains("play.FlipTurn(); play.NoteTurn();"), "every passed turn is noted");
        Expect(play.Contains("turn = 0; ledger.Clear();"), "changing the number of players clears the notes");
    }

    private static string Folder()
    {
        foreach (string start in new[] { Directory.GetCurrentDirectory(), AppContext.BaseDirectory })
        {
            string dir = Path.GetFullPath(start);
            while (dir != null)
            {
                foreach (string rel in new[] { "games/evergrove/Assets/Scripts/Unity", "Assets/Scripts/Unity" })
                    if (File.Exists(Path.Combine(dir, rel, "BoardKit.cs"))) return Path.Combine(dir, rel);
                dir = Path.GetDirectoryName(dir);
            }
        }
        throw new Exception("TurnBackTests could not locate the Unity scripts.");
    }
}
