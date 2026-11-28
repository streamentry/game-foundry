using System;
using System.IO;
using Evergrove.Core;

/// <summary>
/// Pins the solved-together line (vision run 20261128-1502, idea 3): alone nothing is said, two to four
/// players each get their own line, counts above four read as four, nothing throws, and TreePlay.cs
/// finishes a solved board through TableSolved, which sets the one reused turn caption.
/// </summary>
internal static class SolvedTogetherTests
{
    private static int assertions;

    private static void Expect(bool ok, string message)
    {
        assertions++;
        if (!ok) throw new Exception("SolvedTogetherTests failed: " + message);
    }

    public static int Run()
    {
        assertions = 0;

        Expect(TakeTurns.SpokenSolved(0) == "", "zero players says nothing");
        Expect(TakeTurns.SpokenSolved(1) == "", "one player says nothing");
        Expect(TakeTurns.SpokenSolved(-1) == "", "negative players say nothing");
        Expect(TakeTurns.SpokenSolved(2) == "Solved together, both of you.", "two players");
        Expect(TakeTurns.SpokenSolved(3) == "Solved together, all three of you.", "three players");
        Expect(TakeTurns.SpokenSolved(4) == "Solved together, all four of you.", "four players");
        Expect(TakeTurns.SpokenSolved(5) == TakeTurns.SpokenSolved(4), "five players read as four");
        Expect(TakeTurns.SpokenSolved(int.MaxValue) == "Solved together, all four of you.", "a huge table reads as four");
        Expect(TakeTurns.SpokenSolved(int.MinValue) == "", "the smallest count says nothing");

        string treePlay = Script("Unity/TreePlay.cs");
        string turns = Script("Unity/TreePlay.Turns.cs");
        int finish = treePlay.IndexOf("private void Finish()", StringComparison.Ordinal);
        int nextMethod = finish < 0 ? -1 : treePlay.IndexOf("private void Paint()", finish, StringComparison.Ordinal);
        Expect(finish >= 0, "Finish exists in TreePlay.cs");
        Expect(finish >= 0 && nextMethod > finish && treePlay.Substring(finish, nextMethod - finish).Contains("TableSolved();"),
            "Finish calls TableSolved");
        Expect(turns.Contains("private void TableSolved()"), "TableSolved is defined in TreePlay.Turns.cs");
        Expect(turns.Contains("TakeTurns.SpokenSolved(Players)"), "the solved caption counts the seats that moved (vision run 20261128-1901)");
        Expect(turns.Contains("if (!PlayedTogether) return;"), "a board one person solved alone is not celebrated as together");
        Expect(turns.Contains("ledger.Moved(p, tracked.UndoSteps)"), "only the seats that moved light up");
        Expect(treePlay.Contains("internal bool PlayedTogether { get { return together && MoversNow() >= 2; } }"),
            "the garden records together only when two or more seats moved");
        Expect(turns.Contains("token.Find(\"Turn caption\")"), "the solved caption reuses the turn caption");

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
        throw new Exception("SolvedTogetherTests: " + relative + " not found from " + Directory.GetCurrentDirectory());
    }
}
