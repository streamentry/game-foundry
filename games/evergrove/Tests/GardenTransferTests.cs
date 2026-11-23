using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using Evergrove.Core;

/// <summary>
/// Deterministic tests for the player-exportable garden file (format v1).
/// These cover the pure C# core only, not file pickers, share sheets or Unity JsonUtility.
/// </summary>
internal static class GardenTransferTests
{
    private const string Stamp = "2026-11-23T09:00:00Z";
    private static int assertions;

    private static void Expect(bool ok, string message)
    {
        assertions++;
        if (!ok) throw new Exception("GardenTransfer test failed: " + message);
    }

    private static void SolveChapter(WorldModel model)
    {
        Puzzle board = model.CurrentPuzzle;
        for (int index = 0; index < board.CellCount; index++)
        {
            int attempts = 0;
            while (model.Rotations[index] != board.Solution[index])
            {
                Expect(model.Rotate(index), "mutable tile rotates while solving");
                attempts++;
                Expect(attempts <= PipeRules.OrientationCount(board.Kinds[index]), "rotation bounded");
            }
        }
        Expect(model.Solved, "chapter solved");
    }

    private static void RotateFirstMutableTile(WorldModel model)
    {
        Puzzle board = model.CurrentPuzzle;
        for (int index = 0; index < board.CellCount; index++)
        {
            if (!board.IsAnchored(index))
            {
                Expect(model.Rotate(index), "mutable tile rotates");
                return;
            }
        }
        throw new Exception("GardenTransfer test failed: chapter has no mutable tile");
    }

    private static void CompleteChapter(WorldModel model, VillageChoice choice)
    {
        SolveChapter(model);
        Expect(model.Choose(choice), "choice commits after solving");
    }

    private static string Exported(WorldModel model)
    {
        return GardenTransfer.Export(model, Stamp);
    }

    private static bool SameInts(int[] a, int[] b)
    {
        if (a.Length != b.Length) return false;
        for (int i = 0; i < a.Length; i++)
            if (a[i] != b[i]) return false;
        return true;
    }

    private static bool SameState(WorldModel a, WorldModel b)
    {
        return a.Mode == b.Mode && a.Round == b.Round && a.Solved == b.Solved &&
               a.Completed == b.Completed && a.HintsUsed == b.HintsUsed &&
               a.HintStep == b.HintStep && a.HintTile == b.HintTile &&
               a.BoardVariant == b.BoardVariant && a.CanUndo == b.CanUndo &&
               SameInts(a.Rotations, b.Rotations) && SameInts(a.Choices, b.Choices);
    }

    private static void ExpectRoundTrip(WorldModel original, string label)
    {
        string exported = GardenTransfer.Export(original, Stamp);
        WorldModel imported;
        GardenImportError error;
        bool ok = GardenTransfer.TryImport(exported, out imported, out error);
        Expect(ok && error == GardenImportError.None && imported != null, label + " imports cleanly");
        Expect(SameState(original, imported), label + " restores identical state");
        Expect(GardenTransfer.Export(imported, Stamp) == exported, label + " re-exports byte-identically");
    }

    private static void ExpectError(string text, GardenImportError expected)
    {
        WorldModel world;
        GardenImportError error;
        bool ok = GardenTransfer.TryImport(text, out world, out error);
        Expect(!ok && world == null && error == expected,
               "expected " + expected + " but got " + error);
    }

    private static void ExpectArgumentException(Action action, string label)
    {
        bool threw = false;
        try { action(); }
        catch (ArgumentException) { threw = true; }
        Expect(threw, label);
    }

    /// <summary>Replaces the first occurrence only, and requires the fixture to contain it.</summary>
    private static string Replace(string text, string from, string to)
    {
        int at = text.IndexOf(from, StringComparison.Ordinal);
        Expect(at >= 0, "fixture contains " + from);
        return text.Substring(0, at) + to + text.Substring(at + from.Length);
    }

    private static string ChecksumOf(string exported)
    {
        string marker = "\"checksumSha256\":\"";
        int start = exported.IndexOf(marker, StringComparison.Ordinal) + marker.Length;
        int end = exported.IndexOf('"', start);
        return exported.Substring(start, end - start);
    }

    private static string SnapshotTextOf(string exported)
    {
        string marker = "\"snapshot\":";
        Expect(exported.EndsWith("}", StringComparison.Ordinal), "envelope ends with brace");
        int start = exported.IndexOf(marker, StringComparison.Ordinal) + marker.Length;
        return exported.Substring(start, exported.Length - start - 1);
    }

    private static string FlipFirstChecksumDigit(string exported)
    {
        string checksum = ChecksumOf(exported);
        string flipped = (checksum[0] == '0' ? "1" : "0") + checksum.Substring(1);
        return Replace(exported, checksum, flipped);
    }

    private static void EnvelopeShapeAndChecksumDefinition()
    {
        WorldModel world = WorldModel.New();
        string first = GardenTransfer.Export(world, "2026-01-01T00:00:00Z");
        string second = GardenTransfer.Export(world, "2030-06-30T12:00:00Z");

        Expect(first.StartsWith("{\"format\":\"mosselora-garden\",\"formatVersion\":1,\"exportedAtUtc\":\"2026-01-01T00:00:00Z\",\"checksumSha256\":\"", StringComparison.Ordinal),
               "envelope field order is fixed");
        string snapshot = SnapshotTextOf(first);
        Expect(snapshot.StartsWith("{\"version\":4,\"mode\":\"living\",\"round\":0,\"solved\":false,\"rotations\":[", StringComparison.Ordinal),
               "snapshot uses JsonUtility-style field order");
        Expect(ChecksumOf(first) == GardenTransfer.Sha256Hex(snapshot),
               "checksum is SHA-256 of the snapshot text");
        using (SHA256 sha = SHA256.Create())
        {
            byte[] hash = sha.ComputeHash(Encoding.UTF8.GetBytes(snapshot));
            var sb = new StringBuilder();
            foreach (byte b in hash) sb.Append(b.ToString("x2"));
            Expect(ChecksumOf(first) == sb.ToString(), "checksum matches independently computed SHA-256");
        }
        Expect(ChecksumOf(first) == ChecksumOf(second), "checksum ignores exportedAtUtc");
        Expect(first == GardenTransfer.Export(world, "2026-01-01T00:00:00Z"), "export is deterministic");

        ExpectArgumentException(() => GardenTransfer.Export(world, null), "null timestamp is rejected");
        ExpectArgumentException(() => GardenTransfer.Export(world, "   "), "blank timestamp is rejected");
        ExpectArgumentException(() => GardenTransfer.Export(null, Stamp), "null world is rejected");
    }

    private static void FullLivingRunRoundTrips()
    {
        WorldModel run = WorldModel.New();
        ExpectRoundTrip(run, "start");

        RotateFirstMutableTile(run);
        ExpectRoundTrip(run, "mid chapter one with rotation");
        Expect(run.CanUndo, "undo history is part of the saved state");

        SolveChapter(run);
        ExpectRoundTrip(run, "chapter one solved before choice");
        Expect(run.Choose(VillageChoice.ShadeTree), "first choice commits");
        ExpectRoundTrip(run, "after first choice");

        for (int chapter = 1; chapter < PuzzleCatalog.Count; chapter++)
        {
            if (chapter == 3)
            {
                RotateFirstMutableTile(run);
                ExpectRoundTrip(run, "mid chapter four with rotation");
            }
            if (chapter == 6)
            {
                Expect(run.AdvanceGentleHint() == 1, "row hint stage");
                Expect(run.AdvanceGentleHint() == 2, "tile focus hint stage");
                ExpectRoundTrip(run, "chapter seven with focused hint");
                Expect(run.HintStep == 2, "focused hint persisted");
            }
            SolveChapter(run);
            if (chapter == PuzzleCatalog.Count - 1)
                ExpectRoundTrip(run, "final chapter solved before choice");
            Expect(run.Choose((VillageChoice)(chapter % 3)), "choice commits");
            ExpectRoundTrip(run, "after chapter " + (chapter + 1) + " choice");
        }

        Expect(run.Completed && run.Choices.Length == 12, "twelve chapter run completes");
        ExpectRoundTrip(run, "completed garden");
    }

    private static void PuzzleOnlyRunRoundTrips()
    {
        WorldModel run = WorldModel.New(PlayMode.PuzzleOnly);
        SolveChapter(run);
        Expect(run.NextPuzzle(), "puzzle-only advances without a choice");
        RotateFirstMutableTile(run);
        ExpectRoundTrip(run, "puzzle-only second board in progress");

        while (!run.Completed)
        {
            SolveChapter(run);
            Expect(run.NextPuzzle(), "puzzle-only advances");
        }
        ExpectRoundTrip(run, "puzzle-only completed");
        Expect(run.Choices.Length == 0, "puzzle-only has no village choices");
    }

    private static void MatchedRunRoundTrips()
    {
        int[] schedule = { 1, 2, 3, 3, 2, 1, 1, 2, 3, 1, 2 };
        WorldModel run = WorldModel.NewMatched(schedule);
        SolveChapter(run);
        Expect(run.NextPuzzle(), "matched control advances without a choice");
        RotateFirstMutableTile(run);
        ExpectRoundTrip(run, "matched second board in progress");
        string text = Exported(run);
        WorldModel imported;
        GardenImportError error;
        Expect(GardenTransfer.TryImport(text, out imported, out error) &&
               imported.Mode == PlayMode.PuzzleMatched &&
               SameInts(imported.VariantSchedule, schedule) &&
               imported.BoardVariant == schedule[0],
               "matched export keeps mode, schedule and transformed board");
        while (!run.Completed)
        {
            SolveChapter(run);
            Expect(run.NextPuzzle(), "matched control advances");
        }
        ExpectRoundTrip(run, "matched completed");
    }

    private static List<string> ContinuePlay(WorldModel model)
    {
        var log = new List<string>();
        log.Add("undo:" + model.Undo() + ":" + Exported(model));
        RotateFirstMutableTile(model);
        log.Add("hint1:" + model.AdvanceGentleHint() + ":" + Exported(model));
        log.Add("hint2:" + model.AdvanceGentleHint() + ":" + Exported(model));
        log.Add("hint3:" + model.AdvanceGentleHint() + ":" + Exported(model));
        log.Add("restart:" + model.Restart() + ":" + Exported(model));
        SolveChapter(model);
        log.Add("choose:" + model.Choose(VillageChoice.Flowers) + ":" + Exported(model));
        while (!model.Completed)
        {
            SolveChapter(model);
            log.Add("choose:" + model.Choose((VillageChoice)(model.Round % 3)) + ":" + Exported(model));
        }
        return log;
    }

    private static void ImportedWorldContinuesIdentically()
    {
        WorldModel original = WorldModel.New();
        CompleteChapter(original, VillageChoice.Bridge);
        CompleteChapter(original, VillageChoice.Flowers);
        RotateFirstMutableTile(original);

        WorldModel imported;
        GardenImportError error;
        Expect(GardenTransfer.TryImport(Exported(original), out imported, out error) &&
               error == GardenImportError.None, "mid-run garden imports");

        List<string> left = ContinuePlay(original);
        List<string> right = ContinuePlay(imported);
        Expect(left.Count == right.Count, "same number of play steps");
        for (int i = 0; i < left.Count; i++)
            Expect(left[i] == right[i], "play step " + i + " identical after import");
        Expect(original.Completed && imported.Completed, "both gardens finish the run");
    }

    private static void HistoricalSnapshotVersionsImport()
    {
        int[] initial = PuzzleCatalog.Get(0).Initial;
        for (int version = 1; version <= 3; version++)
        {
            var snapshot = new WorldSnapshot {
                version = version, mode = "living", round = 0, solved = false,
                rotations = (int[])initial.Clone(), previous = new int[0], choices = new int[0]
            };
            WorldModel imported;
            GardenImportError error;
            Expect(GardenTransfer.TryImport(GardenTransfer.ExportSnapshot(snapshot, Stamp), out imported, out error) &&
                   error == GardenImportError.None, "snapshot version " + version + " imports");
            Expect(imported.Export().version == WorldModel.Version, "historical version migrates to current");
        }

        var legacyDone = new WorldSnapshot {
            version = 1, mode = "living", round = 6, solved = false,
            rotations = new int[0], previous = new int[0], choices = new[] { 0, 1, 2, 0, 1, 2 }
        };
        WorldModel migrated;
        GardenImportError migratedError;
        Expect(GardenTransfer.TryImport(GardenTransfer.ExportSnapshot(legacyDone, Stamp), out migrated, out migratedError) &&
               migratedError == GardenImportError.None, "legacy completed v1 garden imports");
        Expect(migrated.Round == 6 && !migrated.Completed && migrated.Rotations.Length == 16,
               "legacy completed garden unlocks chapter seven");

        var futureSnapshot = new WorldSnapshot { version = 5, mode = "living" };
        ExpectError(GardenTransfer.ExportSnapshot(futureSnapshot, Stamp), GardenImportError.InvalidSnapshot);
    }

    private static void ErrorCases()
    {
        string valid = GardenTransfer.Export(WorldModel.New(), Stamp);
        string checksum = ChecksumOf(valid);

        ExpectError("", GardenImportError.Empty);
        ExpectError(null, GardenImportError.Empty);
        ExpectError("  \r\n\t", GardenImportError.Empty);

        ExpectError("{", GardenImportError.Malformed);
        ExpectError("not json", GardenImportError.Malformed);
        ExpectError("[]", GardenImportError.Malformed);
        ExpectError("\"text\"", GardenImportError.Malformed);
        ExpectError(valid + "x", GardenImportError.Malformed);
        ExpectError(valid.Substring(0, valid.Length - 1), GardenImportError.Malformed);
        ExpectError(Replace(valid, "\"formatVersion\":1,", "\"formatVersion\":1,\"formatVersion\":1,"), GardenImportError.Malformed);
        ExpectError(Replace(valid, "\"exportedAtUtc\":\"" + Stamp + "\",", ""), GardenImportError.Malformed);
        ExpectError(Replace(valid, "{\"version\"", "{\"version\":4,\"version\""), GardenImportError.Malformed);
        ExpectError(Replace(valid, "\"hintsUsed\":0", "\"hintsUsed\":0,"), GardenImportError.Malformed);

        ExpectError(Replace(valid, "\"format\":\"mosselora-garden\",", ""), GardenImportError.WrongFormat);
        ExpectError(Replace(valid, "mosselora-garden", "other-game"), GardenImportError.WrongFormat);
        ExpectError(Replace(valid, "\"formatVersion\":1,", "\"formatVersion\":0,"), GardenImportError.WrongFormat);
        ExpectError(Replace(valid, "\"formatVersion\":1,", "\"formatVersion\":\"1\","), GardenImportError.WrongFormat);
        ExpectError(Replace(valid, "\"formatVersion\":1,", "\"formatVersion\":1.0,"), GardenImportError.WrongFormat);

        ExpectError(Replace(valid, "\"formatVersion\":1,", "\"formatVersion\":2,"), GardenImportError.UnsupportedFutureVersion);
        ExpectError(Replace(valid, "\"formatVersion\":1,", "\"formatVersion\":99,"), GardenImportError.UnsupportedFutureVersion);

        ExpectError(FlipFirstChecksumDigit(valid), GardenImportError.ChecksumMismatch);
        ExpectError(Replace(valid, checksum, new string('0', 64)), GardenImportError.ChecksumMismatch);
        string upper = checksum.ToUpperInvariant();
        Expect(upper != checksum, "checksum fixture has letters for case test");
        ExpectError(Replace(valid, checksum, upper), GardenImportError.ChecksumMismatch);
        ExpectError(Replace(valid, "\"hintsUsed\":0", "\"hintsUsed\":1"), GardenImportError.ChecksumMismatch);

        ExpectError(Replace(valid, "\"round\":0", "\"round\":\"0\""), GardenImportError.InvalidSnapshot);
        ExpectError(Replace(valid, "\"round\":0", "\"round\":0.0"), GardenImportError.InvalidSnapshot);
        ExpectError(Replace(valid, "\"hintsUsed\":0", "\"hintsUsed\":0,\"bonus\":1"), GardenImportError.InvalidSnapshot);
        ExpectError("{\"format\":\"mosselora-garden\",\"formatVersion\":1,\"exportedAtUtc\":\"x\",\"checksumSha256\":\"00\"}",
                    GardenImportError.InvalidSnapshot);
        ExpectError("{\"format\":\"mosselora-garden\",\"formatVersion\":1,\"exportedAtUtc\":\"x\",\"checksumSha256\":\"00\",\"snapshot\":[]}",
                    GardenImportError.InvalidSnapshot);

        WorldSnapshot solvedLie = WorldModel.New().Export();
        solvedLie.solved = true;
        ExpectError(GardenTransfer.ExportSnapshot(solvedLie, Stamp), GardenImportError.InvalidSnapshot);

        WorldModel imported;
        GardenImportError error;
        Expect(GardenTransfer.TryImport(valid + "\n", out imported, out error) &&
               error == GardenImportError.None, "trailing whitespace is accepted");
        Expect(GardenTransfer.TryImport(Replace(valid, Stamp, "2031-01-01T00:00:00Z"), out imported, out error) &&
               error == GardenImportError.None, "exportedAtUtc is metadata outside the checksum");
    }

    private static void SizeLimit()
    {
        ExpectError("{" + new string(' ', GardenTransfer.MaxImportBytes) + "}", GardenImportError.TooLarge);

        string valid = GardenTransfer.Export(WorldModel.New(), Stamp);
        string atLimit = valid + new string(' ', GardenTransfer.MaxImportBytes - Encoding.UTF8.GetByteCount(valid));
        Expect(Encoding.UTF8.GetByteCount(atLimit) == GardenTransfer.MaxImportBytes, "padding reaches the cap exactly");
        WorldModel imported;
        GardenImportError error;
        Expect(GardenTransfer.TryImport(atLimit, out imported, out error) && error == GardenImportError.None,
               "input exactly at the cap imports");
        ExpectError(atLimit + " ", GardenImportError.TooLarge);
    }

    public static int Run()
    {
        assertions = 0;
        EnvelopeShapeAndChecksumDefinition();
        FullLivingRunRoundTrips();
        PuzzleOnlyRunRoundTrips();
        MatchedRunRoundTrips();
        ImportedWorldContinuesIdentically();
        HistoricalSnapshotVersionsImport();
        ErrorCases();
        SizeLimit();
        Console.WriteLine("Evergrove garden transfer tests: " + assertions + " deterministic assertions.");
        return assertions;
    }
}
