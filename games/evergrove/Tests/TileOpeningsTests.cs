using System;
using System.IO;
using Evergrove.Core;

/// <summary>
/// Pins the spoken tile openings: each pipe tile says which directions it opens, in
/// up, right, down, left order, matching PipeRules.HasPort; invalid orientations say
/// nothing; every authored tile has words; and the Unity layout speaks them in English.
/// </summary>
internal static class TileOpeningsTests
{
    private static int assertions;

    private static void Expect(bool ok, string message)
    {
        assertions++;
        if (!ok) throw new Exception("TileOpeningsTests failed: " + message);
    }

    // Independent expectation built straight from HasPort, not from TileOpenings.
    private static string Expected(PipeKind kind, int rotation)
    {
        string[] names = { "up", "right", "down", "left" };
        var open = new System.Collections.Generic.List<string>();
        for (int d = 0; d < 4; d++)
            if (PipeRules.HasPort(kind, rotation, d)) open.Add(names[d]);
        if (open.Count == 0) return "";
        if (open.Count == 1) return "Opens " + open[0] + ".";
        return "Opens " + open[0] + " and " + open[1] + ".";
    }

    public static int Run()
    {
        assertions = 0;

        // Every kind and every valid rotation names exactly the ports HasPort reports.
        PipeKind[] kinds = { PipeKind.End, PipeKind.Straight, PipeKind.Corner };
        foreach (PipeKind kind in kinds)
        {
            for (int rotation = 1; rotation <= PipeRules.OrientationCount(kind); rotation++)
            {
                string words = TileOpenings.Words(kind, rotation);
                Expect(words == Expected(kind, rotation),
                    kind + " rotation " + rotation + " reads \"" + words + "\"");
                Expect(words.StartsWith("Opens ") && words.EndsWith("."),
                    kind + " rotation " + rotation + " is a full sentence");
            }
        }

        // Exact pins, ordered up, right, down, left.
        Expect(TileOpenings.Words(PipeKind.End, 1) == "Opens up.", "end rotation 1 opens up");
        Expect(TileOpenings.Words(PipeKind.Straight, 1) == "Opens up and down.", "straight rotation 1 opens up and down");
        Expect(TileOpenings.Words(PipeKind.Straight, 2) == "Opens right and left.", "straight rotation 2 opens right and left");
        Expect(TileOpenings.Words(PipeKind.Corner, 1) == "Opens up and right.", "corner rotation 1 opens up and right");
        Expect(TileOpenings.Words(PipeKind.Corner, 4) == "Opens up and left.", "corner rotation 4 opens up and left");

        // Invalid rotations say nothing.
        foreach (int bad in new[] { 0, 5, -1 })
        {
            foreach (PipeKind kind in kinds)
            {
                Expect(TileOpenings.Words(kind, bad) == "", kind + " rotation " + bad + " is silent");
            }
        }
        Expect(TileOpenings.Words(PipeKind.Straight, 3) == "", "straight has no rotation 3");

        // Every authored tile, in its starting orientation, has words.
        for (int p = 0; p < PuzzleCatalog.Count; p++)
        {
            Puzzle puzzle = PuzzleCatalog.Get(p);
            for (int i = 0; i < puzzle.CellCount; i++)
            {
                string words = TileOpenings.Words(puzzle.Kinds[i], puzzle.Initial[i]);
                Expect(words.Length > 0, puzzle.Name + " tile " + i + " has opening words");
            }
        }

        // Source check: the Unity layout speaks the openings through TileOpenings.Words.
        string layout = Script("Unity/EvergroveApp.Layout.cs");
        Expect(layout.Contains("TileOpenings.Words"), "TileSpeech speaks the tile openings");
        Expect(layout.Contains("GameLocalization.Current == \"en\""), "tile openings are English-only");

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
        throw new Exception("TileOpeningsTests: " + relative + " not found from " + Directory.GetCurrentDirectory());
    }
}
