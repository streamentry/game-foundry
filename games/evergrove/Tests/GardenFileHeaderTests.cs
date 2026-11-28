using System;
using Evergrove.Core;

/// <summary>
/// The garden file's plain-language header (GardenTransfer.Header / ExportForPeople) and the tolerant
/// import that skips it. Pins the wording, singular and plural, the optional name, the brace rule,
/// the 300-character header limit, and that plain Export output is unchanged.
/// </summary>
internal static class GardenFileHeaderTests
{
    private const string Stamp = "2026-01-01T00:00:00Z";
    private const string Tail = " restored. Keep all of this text to bring the garden back.";
    private static int assertions;

    private static void Expect(bool ok, string message)
    {
        assertions++;
        if (!ok) throw new Exception("GardenFileHeaderTests failed: " + message);
    }

    private static void ExpectError(string text, GardenImportError expected)
    {
        WorldModel world;
        GardenImportError error;
        bool ok = GardenTransfer.TryImport(text, out world, out error);
        Expect(!ok && world == null && error == expected,
            "import gives " + expected + " (got " + error + ", ok " + ok + ")");
    }

    private static void Solve(WorldModel world)
    {
        Puzzle board = world.CurrentPuzzle;
        for (int tile = 0; tile < board.CellCount; tile++)
            for (int turn = 0; turn < 4 && world.Rotations[tile] != board.Solution[tile]; turn++)
                Expect(world.Rotate(tile), "solver turns only mutable tiles");
        Expect(world.Solved, "fixture reaches a valid solution");
    }

    private static WorldModel Garden(string name, int streams)
    {
        WorldModel world = WorldModel.New(PlayMode.Living);
        if (name != null) Expect(world.SetGardenName(name), "fixture name is accepted");
        for (int i = 0; i < streams; i++)
        {
            Solve(world);
            Expect(world.Choose((VillageChoice)(i % 3)), "fixture choice commits");
        }
        Expect(world.Choices.Length == streams, "fixture has the requested streams");
        return world;
    }

    public static int Run()
    {
        assertions = 0;

        // Header wording: named, plural.
        WorldModel named = Garden("Willowbank", 2);
        Expect(GardenTransfer.Header(named) ==
               "Mosselora garden file: Willowbank, 2 streams" + Tail,
            "named plural header is exact");

        // Unnamed, singular: the name part is omitted.
        WorldModel single = Garden(null, 1);
        Expect(GardenTransfer.Header(single) == "Mosselora garden file: 1 stream" + Tail,
            "unnamed singular header is exact");

        // Unnamed, zero streams: plural with zero.
        WorldModel fresh = Garden(null, 0);
        Expect(GardenTransfer.Header(fresh) == "Mosselora garden file: 0 streams" + Tail,
            "unnamed zero-stream header is plural");

        // A name containing a brace is omitted entirely; the header then has no braces.
        WorldModel braced = Garden("Brace {x}", 1);
        string bracedHeader = GardenTransfer.Header(braced);
        Expect(bracedHeader == "Mosselora garden file: 1 stream" + Tail, "a name with braces is omitted");
        Expect(bracedHeader.IndexOf('{') < 0 && bracedHeader.IndexOf('}') < 0, "header has no braces");
        WorldModel closeBrace = Garden("Ok}", 1);
        Expect(GardenTransfer.Header(closeBrace) == "Mosselora garden file: 1 stream" + Tail,
            "a name with a closing brace is omitted");

        // ExportForPeople is the header, a newline, then the unchanged envelope.
        string people = GardenTransfer.ExportForPeople(named, Stamp);
        string plain = GardenTransfer.Export(named, Stamp);
        Expect(people == GardenTransfer.Header(named) + "\n" + plain,
            "ExportForPeople is Header + newline + Export");
        Expect(people.StartsWith("Mosselora garden file: ", StringComparison.Ordinal),
            "the copy starts with the header");

        // Tolerant import: the header-carrying text restores the same garden.
        WorldModel imported;
        GardenImportError error;
        Expect(GardenTransfer.TryImport(people, out imported, out error) &&
               error == GardenImportError.None && imported != null,
            "ExportForPeople output imports");
        Expect(GardenTransfer.Export(imported, Stamp) == plain,
            "the restored world exports identically to the original");

        // Plain Export still imports.
        Expect(GardenTransfer.TryImport(plain, out imported, out error) && imported != null,
            "plain Export still imports");

        // A 300-character prefix (no braces) is accepted; 301 is rejected.
        string prefix300 = new string('x', 300);
        Expect(GardenTransfer.TryImport(prefix300 + plain, out imported, out error) && imported != null,
            "a 300-character prefix is accepted");
        ExpectError(new string('x', 301) + plain, GardenImportError.Malformed);

        // A prefix containing '}' is rejected.
        ExpectError("note}" + plain, GardenImportError.Malformed);

        // Junk with no '{' is rejected as Malformed, as before.
        ExpectError("Mosselora garden file: 1 stream restored.", GardenImportError.Malformed);
        ExpectError("no braces in this text at all", GardenImportError.Malformed);

        // Existing error kinds are unchanged for text that starts with '{'.
        ExpectError("{", GardenImportError.Malformed);
        ExpectError(plain + "x", GardenImportError.Malformed);

        // Export is deterministic and unchanged by the header.
        Expect(GardenTransfer.Export(named, Stamp) == plain, "Export output is unchanged versus a fresh call");
        Expect(GardenTransfer.ExportForPeople(named, Stamp) == people, "ExportForPeople is deterministic");

        return assertions;
    }
}
