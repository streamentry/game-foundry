using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Evergrove.Core;

/// <summary>
/// Pins the spring's outlet sentence: one, two and three-or-more outlets in reading order
/// with 1-based row and column, silence for null or outlet-free boards, and for every
/// authored catalogue board a sentence that names exactly the outlets that exist.
/// </summary>
internal static class OutletWordsTests
{
    private static int assertions;

    private static void Expect(bool ok, string message)
    {
        assertions++;
        if (!ok) throw new Exception("OutletWordsTests failed: " + message);
    }

    private static Puzzle Board(string name, string layout)
    {
        int cells = layout.Length;
        var zeros = new int[cells];
        return new Puzzle(name, layout, zeros, zeros);
    }

    // Independent expectation: "row r, column c" items, last two joined by " and ".
    private static string Expected(List<int> outlets, int size)
    {
        if (outlets.Count == 0) return "";
        var text = new StringBuilder("The water must reach ");
        for (int i = 0; i < outlets.Count; i++)
        {
            if (i > 0) text.Append(i == outlets.Count - 1 ? " and " : ", ");
            text.Append("row " + (outlets[i] / size + 1) + ", column " + (outlets[i] % size + 1));
        }
        return text.Append('.').ToString();
    }

    private static List<int> EndTiles(Puzzle puzzle)
    {
        var ends = new List<int>();
        for (int i = 0; i < puzzle.Kinds.Length; i++)
            if (puzzle.Kinds[i] == PipeKind.End) ends.Add(i);
        return ends;
    }

    public static int Run()
    {
        assertions = 0;

        // One outlet: spring at 0, outlet at row 3, column 3.
        Puzzle one = Board("one", "EIIIIIIIE");
        Expect(OutletWords.For(one) == "The water must reach row 3, column 3.", "one outlet sentence");

        // Two outlets: row 1, column 4 and row 3, column 3, in reading order.
        Puzzle two = Board("two", "EIIEIIIIIIEIIIII");
        Expect(OutletWords.For(two) == "The water must reach row 1, column 4 and row 3, column 3.",
            "two outlet sentence");

        // Three outlets: commas, then " and " before the last.
        Puzzle three = Board("three", "EIIEEIIIIIIIIIIE");
        Expect(OutletWords.For(three) ==
               "The water must reach row 1, column 4, row 2, column 1 and row 4, column 4.",
            "three outlet sentence");

        // No outlets, no spring, or null: silent.
        Expect(OutletWords.For(Board("spring only", "EIIIIIIII")) == "", "spring alone names no outlets");
        Expect(OutletWords.For(Board("no ends", "IIIIIIIII")) == "", "no end tiles is silent");
        Expect(OutletWords.For(null) == "", "null puzzle is silent");

        // Every authored catalogue board: the sentence names exactly the outlets that exist.
        for (int p = 0; p < PuzzleCatalog.Count; p++)
        {
            Puzzle puzzle = PuzzleCatalog.Get(p);
            List<int> ends = EndTiles(puzzle);
            List<int> outlets = ends.GetRange(Math.Min(1, ends.Count), Math.Max(0, ends.Count - 1));
            string sentence = OutletWords.For(puzzle);
            Expect(sentence == Expected(outlets, puzzle.Size), puzzle.Name + " matches the independent sentence");

            if (outlets.Count == 0)
            {
                Expect(sentence == "", puzzle.Name + " with no outlets is silent");
                continue;
            }

            Expect(sentence.StartsWith("The water must reach ") && sentence.EndsWith("."),
                puzzle.Name + " is a full sentence");
            string body = sentence.Substring("The water must reach ".Length, sentence.Length - "The water must reach ".Length - 1);
            int rowWords = 0;
            for (int at = body.IndexOf("row ", StringComparison.Ordinal); at >= 0;
                 at = body.IndexOf("row ", at + 4, StringComparison.Ordinal))
                rowWords++;
            Expect(rowWords == outlets.Count, puzzle.Name + " names " + outlets.Count + " outlets");

            string[] parts = body.Replace(" and ", ", ").Split(new[] { ", " }, StringSplitOptions.None);
            Expect(parts.Length == outlets.Count * 2, puzzle.Name + " splits into row/column pairs");
            for (int i = 0; i < outlets.Count; i++)
            {
                int row = int.Parse(parts[i * 2].Substring("row ".Length));
                int column = int.Parse(parts[i * 2 + 1].Substring("column ".Length));
                int index = (row - 1) * puzzle.Size + (column - 1);
                Expect(index == outlets[i], puzzle.Name + " names outlet " + index + " in reading order");
                Expect(puzzle.Kinds[index] == PipeKind.End, puzzle.Name + " named cell is an end tile");
                Expect(index != ends[0], puzzle.Name + " named cell is not the spring");
            }
        }

        // Source check: the spring-only append sits inside the English guard in TileSpeech.
        string layout = Script("Unity/EvergroveApp.Layout.cs");
        int guard = layout.IndexOf("GameLocalization.Current == \"en\"", StringComparison.Ordinal);
        int spring = layout.IndexOf("index == firstEnd)", StringComparison.Ordinal);
        int outletsCall = layout.IndexOf("OutletWords.For(puzzle)", StringComparison.Ordinal);
        Expect(guard >= 0 && spring > guard && outletsCall > spring,
            "TileSpeech appends outlets for the spring inside the English guard");

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
        throw new Exception("OutletWordsTests: " + relative + " not found from " + Directory.GetCurrentDirectory());
    }
}
