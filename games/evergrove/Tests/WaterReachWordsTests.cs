using System;
using System.IO;
using Evergrove.Core;

/// <summary>
/// Pins the water reach line (vision run 20261128-1502, idea 5): the wet count out of every pipe, clamped
/// to the board, the solved line when every pipe is wet, the solved and scrambled fixtures of the first
/// puzzle, and TreePlay.cs speaking the reach from Paint through one reused caption.
/// </summary>
internal static class WaterReachWordsTests
{
    private static int assertions;

    private static void Expect(bool ok, string message)
    {
        assertions++;
        if (!ok) throw new Exception("WaterReachWordsTests failed: " + message);
    }

    public static int Run()
    {
        assertions = 0;

        Expect(WaterReachWords.Line(0, 9) == "Water reaches 0 of 9 pipes.", "nothing wet of nine");
        Expect(WaterReachWords.Line(5, 9) == "Water reaches 5 of 9 pipes.", "five of nine");
        Expect(WaterReachWords.Line(9, 9) == "Water runs through every pipe.", "every pipe of nine");
        Expect(WaterReachWords.Line(12, 9) == "Water runs through every pipe.", "more wet than pipes clamps to every pipe");
        Expect(WaterReachWords.Line(-3, 9) == "Water reaches 0 of 9 pipes.", "negative wet clamps to none");
        Expect(WaterReachWords.Line(0, 0) == "", "no pipes, no line");
        Expect(WaterReachWords.Line(1, 1) == "Water runs through every pipe.", "a single pipe, wet");
        Expect(WaterReachWords.Line(3, -2) == "", "a negative total, no line");

        // Real boards: the first puzzle, solved and as scrambled.
        Puzzle puzzle = PuzzleCatalog.Get(0);
        PipeInsight solved = PipeRules.Inspect(puzzle, puzzle.Solution);
        Expect(WaterReachWords.For(solved, puzzle.CellCount) == "Water runs through every pipe.",
            "the solved board says every pipe is wet");
        PipeInsight scrambled = PipeRules.Inspect(puzzle, puzzle.Initial);
        Expect(scrambled.ConnectedCount < puzzle.CellCount, "the scrambled board does not reach every pipe");
        Expect(WaterReachWords.For(scrambled, puzzle.CellCount) ==
            "Water reaches " + scrambled.ConnectedCount + " of " + puzzle.CellCount + " pipes.",
            "the scrambled board names its wet count out of every cell");

        // Odd input never throws and never names a count.
        Expect(WaterReachWords.For(null, 9) == "", "no inspection, no line");
        Expect(WaterReachWords.For(scrambled, 0) == "", "no pipe count, no line");
        Expect(WaterReachWords.For(new PipeInsight(int.MaxValue, 0, 0, new bool[0]), 9) == "Water runs through every pipe.",
            "a huge wet count clamps to every pipe");

        string paint = Script("Unity/TreePlay.cs");
        string reach = Script("Unity/TreePlay.Reach.cs");
        Expect(paint.Contains("PipeInsight insight = PipeRules.Inspect(puzzle, now); SpeakReach(insight);"),
            "Paint speaks the reach from the live inspection");
        Expect(reach.Contains("\"Water reach\""), "the reach caption is one named node");
        Expect(reach.Contains("WaterReachWords.For(insight, puzzle.CellCount)"), "the reach line comes from WaterReachWords");

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
        throw new Exception("WaterReachWordsTests: " + relative + " not found from " + Directory.GetCurrentDirectory());
    }
}
