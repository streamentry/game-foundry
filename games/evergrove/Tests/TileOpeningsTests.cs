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

        // Long-press caption: the openings follow the action in both anchored states.
        foreach (PipeKind kind in kinds)
        {
            for (int rotation = 1; rotation <= PipeRules.OrientationCount(kind); rotation++)
            {
                string words = TileOpenings.Words(kind, rotation);
                string free = TileOpenings.Caption(false, kind, rotation);
                string pinned = TileOpenings.Caption(true, kind, rotation);
                if (words.Length > 0)
                {
                    Expect(free == "Turn this tile. " + words, kind + " rotation " + rotation + " free caption");
                    Expect(pinned == "This tile stays where it is. " + words, kind + " rotation " + rotation + " pinned caption");
                }
                else
                {
                    Expect(free == "Turn this tile", kind + " rotation " + rotation + " bare free caption");
                    Expect(pinned == "This tile stays where it is.", kind + " rotation " + rotation + " bare pinned caption");
                }
            }
        }
        Expect(TileOpenings.Caption(false, PipeKind.Corner, 1) == "Turn this tile. Opens up and right.", "corner caption pins words");
        Expect(TileOpenings.Caption(true, PipeKind.Corner, 3) == "This tile stays where it is. Opens down and left.", "pinned corner caption pins words");

        // Out-of-range rotations keep the bare captions and never throw.
        foreach (int bad in new[] { 0, -1, 99 })
        {
            foreach (PipeKind kind in kinds)
            {
                Expect(TileOpenings.Caption(false, kind, bad) == "Turn this tile", kind + " rotation " + bad + " bare free caption");
                Expect(TileOpenings.Caption(true, kind, bad) == "This tile stays where it is.", kind + " rotation " + bad + " bare pinned caption");
            }
        }

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

        // Source check (vision run 1401): the long-press caption on a water tile is read live, so it
        // describes the tile as it is now, not as it was when the board was built.
        string puzzleView = Script("Unity/EvergroveApp.Puzzle.cs");
        string caption = Script("Unity/LongPressCaption.cs");
        Expect(puzzleView.Contains("LongPressCaption.AttachLive(") && puzzleView.Contains("TileOpenings.Caption("),
            "the water tile's caption names its openings");
        Expect(puzzleView.Contains("world.Rotations[tappedIndex]"), "the caption reads the tile's current rotation");
        Expect(caption.Contains("live != null ? live() : null"), "the caption's words are read when it is shown");

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
