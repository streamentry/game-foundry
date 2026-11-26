using System;
using System.Collections.Generic;
using System.Text.Json;
using Evergrove.Core;

/// <summary>
/// EG-111 ten-step undo history. Pure C# only; Unity JsonUtility is not exercised here.
/// </summary>
internal static class UndoHistoryTests
{
    private const string Stamp = "2026-11-23T09:00:00Z";
    private static int assertions;
    private static readonly JsonSerializerOptions Options = new JsonSerializerOptions { IncludeFields = true };

    private static void Expect(bool ok, string message)
    {
        assertions++;
        if (!ok) throw new Exception("Undo history test failed: " + message);
    }

    private static string Key(int[] values)
    {
        return string.Join(",", values);
    }

    private static List<int> Movable(Puzzle board)
    {
        var tiles = new List<int>();
        for (int i = 0; i < board.CellCount; i++)
            if (!board.IsAnchored(i)) tiles.Add(i);
        return tiles;
    }

    /// <summary>Rotates movable tiles and records every board: states[k] is the board after k rotations.</summary>
    private static List<int[]> RotateSteps(WorldModel world, int count)
    {
        var states = new List<int[]> { world.Rotations };
        List<int> tiles = Movable(world.CurrentPuzzle);
        for (int step = 0; step < count; step++)
        {
            Expect(world.Rotate(tiles[step % tiles.Count]), "movable tile rotates");
            states.Add(world.Rotations);
        }
        return states;
    }

    /// <summary>Undoes `steps` times from state index `fromIndex`; each must restore the exact earlier board.</summary>
    private static void ExpectUndoes(WorldModel world, List<int[]> states, int fromIndex, int steps, string label)
    {
        for (int step = 0; step < steps; step++)
        {
            int expected = fromIndex - 1 - step;
            Expect(world.CanUndo && world.Undo(), label + ": undo " + (step + 1) + " succeeds");
            Expect(Key(world.Rotations) == Key(states[expected]), label + ": undo " + (step + 1) + " restores exact board");
        }
    }

    private static void SolveBoard(WorldModel world)
    {
        Puzzle board = world.CurrentPuzzle;
        for (int tile = 0; tile < board.CellCount; tile++)
            for (int attempt = 0; attempt < 4 && world.Rotations[tile] != board.Solution[tile]; attempt++)
                Expect(world.Rotate(tile), "solver rotates a movable tile");
        Expect(world.Solved, "board solved by reference orientations");
    }

    /// <summary>Removes the optional "history" array from serialized JSON text (simulates an older save).</summary>
    // Files from before EG-111 predate the garden map (I177) too: strip both.
    private static string RemoveHistoryKey(string text)
    {
        text = RemoveMapKeys(text);
        int start = text.IndexOf(",\"history\":", StringComparison.Ordinal);
        Expect(start >= 0, "serialized text contains a history key to remove");
        int end = text.IndexOf(']', start);
        return text.Substring(0, start) + text.Substring(end + 1);
    }

    internal static string RemoveMapKeys(string text)
    {
        int start = text.IndexOf(",\"garden\":", StringComparison.Ordinal);
        if (start < 0) return text;
        int end = text.IndexOf(']', text.IndexOf(",\"placements\":", start, StringComparison.Ordinal));
        int plantings = text.IndexOf(",\"plantings\":", start, StringComparison.Ordinal);
        if (plantings >= 0) end = text.IndexOf(']', plantings); // I193 key follows the map keys
        int solvedTrees = text.IndexOf(",\"treeSolved\":", start, StringComparison.Ordinal);
        if (solvedTrees >= 0) end = text.IndexOf(']', solvedTrees); // I204 keys follow plantings
        return text.Substring(0, start) + text.Substring(end + 1);
    }

    private static void TenStepsUndoExactly()
    {
        WorldModel world = WorldModel.New();
        List<int[]> states = RotateSteps(world, 10);
        Expect(world.UndoDepth == 10 && world.CanUndo, "ten rotations fill the undo history");
        Expect(world.Export().history.Length == 10 * world.CurrentPuzzle.CellCount,
            "history stores ten whole boards as a flat array");
        ExpectUndoes(world, states, 10, 10, "ten-step");
        Expect(!world.CanUndo && world.UndoDepth == 0 && !world.Undo(), "history exhausted at the starting board");
        Expect(world.Export().previous.Length == 0, "legacy previous mirror empty when history is empty");
    }

    private static void EleventhRotationDropsOldest()
    {
        WorldModel world = WorldModel.New();
        List<int[]> states = RotateSteps(world, 11);
        Expect(world.UndoDepth == WorldModel.UndoLimit, "eleventh rotation keeps exactly ten steps");
        ExpectUndoes(world, states, 11, 10, "eleven-rotation");
        Expect(!world.CanUndo, "the oldest step (starting board) was dropped, not restored");
    }

    private static void UndoSurvivesSaveAndRestore()
    {
        WorldModel world = WorldModel.New();
        List<int[]> states = RotateSteps(world, 6);
        WorldModel restored;
        Expect(WorldModel.TryRestore(world.Export(), out restored) && restored.UndoDepth == 6,
            "undo history survives snapshot restore");
        ExpectUndoes(restored, states, 6, 3, "restored snapshot");

        string json = JsonSerializer.Serialize(restored.Export(), Options);
        WorldSnapshot parsed = JsonSerializer.Deserialize<WorldSnapshot>(json, Options);
        WorldModel reloaded;
        Expect(WorldModel.TryRestore(parsed, out reloaded) && reloaded.UndoDepth == 3,
            "mid-history save survives serialization");
        ExpectUndoes(reloaded, states, 3, 3, "reloaded mid-history");
        Expect(!reloaded.CanUndo, "reloaded history ends at the starting board");
    }

    private static void HintAndGuideInteractions()
    {
        WorldModel world = WorldModel.New();
        var states = new List<int[]> { world.Rotations };
        Expect(world.Rotate(Movable(world.CurrentPuzzle)[0]), "manual turn before hints");
        states.Add(world.Rotations);
        Expect(world.AdvanceGentleHint() == 1 && world.UndoDepth == 1,
            "noticing a row does not add an undo step");
        Expect(world.AdvanceGentleHint() == 2 && world.UndoDepth == 1,
            "focusing a tile does not add an undo step");
        Expect(world.AdvanceGentleHint() == 3 && world.UndoDepth == 2,
            "a corrected hint tile is one undo step");
        states.Add(world.Rotations);
        int helpsBefore = world.HintsUsed;
        Expect(world.GuideOneConnection() && world.UndoDepth == 3,
            "direct guide is one undo step");
        states.Add(world.Rotations);
        Expect(world.Export().history.Length == 3 * world.CurrentPuzzle.CellCount,
            "history keeps every board across mixed manual and hint steps");

        ExpectUndoes(world, states, 3, 3, "hint-mixed");
        Expect(world.HintsUsed == helpsBefore + 1, "undo never refunds a used hint");

        var hinted = WorldModel.New();
        Expect(hinted.AdvanceGentleHint() == 1 && hinted.HintStep == 1 && !hinted.CanUndo,
            "a hint notice alone has nothing to undo");
        WorldModel marked = WorldModel.New();
        Expect(marked.Rotate(Movable(marked.CurrentPuzzle)[0]) && marked.AdvanceGentleHint() == 1,
            "hint stage is active before undo");
        Expect(marked.Undo() && marked.HintStep == 0 && marked.HintTile == -1,
            "undo clears a staged hint like any other board change");
    }

    private static void RestartAndNextBoardClearHistory()
    {
        WorldModel world = WorldModel.New();
        RotateSteps(world, 4);
        Expect(world.Restart() && !world.CanUndo && world.Export().history.Length == 0,
            "restart clears undo history");

        SolveBoard(world);
        Expect(world.UndoDepth > 0, "solving the board recorded undo steps");
        Expect(world.Choose(VillageChoice.ShadeTree) && world.Round == 1,
            "choice advances to the next board");
        WorldSnapshot next = world.Export();
        Expect(!world.CanUndo && next.history.Length == 0 && next.previous.Length == 0,
            "next board starts with empty undo history");
    }

    private static void LegacySnapshotsLoad()
    {
        WorldModel world = WorldModel.New();
        List<int[]> states = RotateSteps(world, 2);

        // v4 saved before the history field existed: only `previous` carries one step.
        WorldSnapshot v4Legacy = world.Export();
        v4Legacy.history = Array.Empty<int>();
        WorldModel restored;
        Expect(WorldModel.TryRestore(v4Legacy, out restored) && restored.UndoDepth == 1,
            "v4 save without history derives one undo step from previous");
        ExpectUndoes(restored, states, 2, 1, "v4 legacy");

        string legacyJson = RemoveHistoryKey(JsonSerializer.Serialize(world.Export(), Options));
        WorldSnapshot fromText = JsonSerializer.Deserialize<WorldSnapshot>(legacyJson, Options);
        Expect(WorldModel.TryRestore(fromText, out restored) && restored.UndoDepth == 1,
            "v4 JSON without the history key loads with one undo step");

        int[] initial = PuzzleCatalog.Get(0).Initial;
        WorldModel oneTurn = WorldModel.New();
        Expect(oneTurn.Rotate(Movable(oneTurn.CurrentPuzzle)[0]), "turn for legacy version fixture");
        for (int version = 1; version <= 3; version++)
        {
            var snapshot = new WorldSnapshot {
                version = version, mode = "living", round = 0, solved = false,
                rotations = oneTurn.Rotations, previous = (int[])initial.Clone(), choices = new int[0]
            };
            WorldModel old;
            Expect(WorldModel.TryRestore(snapshot, out old) && old.UndoDepth == 1,
                "v" + version + " snapshot without history loads with one undo step");
            Expect(old.Undo() && Key(old.Rotations) == Key(initial),
                "v" + version + " legacy undo restores the previous board");
        }
    }

    private static void CorruptHistoryRejected()
    {
        WorldModel world = WorldModel.New();
        RotateSteps(world, 3);
        int cells = world.CurrentPuzzle.CellCount;
        WorldModel parsed;

        WorldSnapshot ragged = world.Export();
        ragged.history = Append(ragged.history, 1);
        Expect(!WorldModel.TryRestore(ragged, out parsed), "history length not a whole number of boards rejected");

        WorldSnapshot tooDeep = world.Export();
        var eleven = new int[WorldModel.UndoLimit * cells + cells];
        for (int i = 0; i < eleven.Length; i++) eleven[i] = world.Export().history[i % (3 * cells)];
        tooDeep.history = eleven;
        tooDeep.previous = Slice(eleven, eleven.Length - cells, cells);
        Expect(!WorldModel.TryRestore(tooDeep, out parsed), "more than ten undo steps rejected");

        WorldSnapshot badValue = world.Export();
        badValue.history[0] = 999;
        Expect(!WorldModel.TryRestore(badValue, out parsed), "out-of-range historical rotation rejected");

        WorldSnapshot mismatch = world.Export();
        mismatch.previous = (int[])PuzzleCatalog.Get(0).Initial.Clone();
        Expect(!WorldModel.TryRestore(mismatch, out parsed),
            "previous disagreeing with newest history entry rejected");
        WorldSnapshot stale = world.Export();
        stale.previous = new int[0];
        Expect(!WorldModel.TryRestore(stale, out parsed), "history without a matching previous rejected");

        WorldSnapshot nullHistory = world.Export();
        nullHistory.history = null;
        Expect(!WorldModel.TryRestore(nullHistory, out parsed), "null history rejected");

        WorldSnapshot wrongSize = world.Export();
        wrongSize.history = new int[2 * (cells + 7)];
        Expect(!WorldModel.TryRestore(wrongSize, out parsed), "history sized for another board rejected");

        WorldSnapshot oldVersion = world.Export();
        oldVersion.version = 3;
        Expect(!WorldModel.TryRestore(oldVersion, out parsed), "history inside a pre-v4 snapshot rejected");

        WorldSnapshot good = world.Export();
        Expect(WorldModel.TryRestore(good, out parsed) && parsed.UndoDepth == 3,
            "the uncorrupted history still restores");
    }

    private static void GardenRoundTripWithHistory()
    {
        WorldModel world = WorldModel.New();
        List<int[]> states = RotateSteps(world, 5);
        string text = GardenTransfer.Export(world, Stamp);
        WorldModel imported;
        GardenImportError error;
        Expect(GardenTransfer.TryImport(text, out imported, out error) && error == GardenImportError.None,
            "garden with undo history imports");
        Expect(imported.UndoDepth == 5, "imported garden keeps all undo steps");
        Expect(GardenTransfer.Export(imported, Stamp) == text, "undo history exports byte-identically after import");
        ExpectUndoes(imported, states, 5, 5, "imported garden");

        // A garden exported before EG-111 has no history key and an older checksum; it must still import.
        string snapshotText = SnapshotText(text);
        string legacySnapshot = RemoveHistoryKey(snapshotText);
        string legacy = Envelope(legacySnapshot, GardenTransfer.Sha256Hex(legacySnapshot));
        WorldModel fromLegacy;
        Expect(GardenTransfer.TryImport(legacy, out fromLegacy, out error) && error == GardenImportError.None,
            "pre-history garden with its original checksum imports");
        Expect(fromLegacy.UndoDepth == 1, "pre-history garden gets one undo step from previous");

        // The old checksum must not validate a snapshot that carries history.
        string forged = Envelope(snapshotText, GardenTransfer.Sha256Hex(legacySnapshot));
        Expect(!GardenTransfer.TryImport(forged, out imported, out error) &&
               error == GardenImportError.ChecksumMismatch,
            "history cannot be added under a pre-history checksum");
    }

    private static string SnapshotText(string envelope)
    {
        const string marker = "\"snapshot\":";
        int start = envelope.IndexOf(marker, StringComparison.Ordinal) + marker.Length;
        return envelope.Substring(start, envelope.Length - start - 1);
    }

    private static string Envelope(string snapshot, string checksum)
    {
        return "{\"format\":\"mosselora-garden\",\"formatVersion\":1,\"exportedAtUtc\":\"" + Stamp +
            "\",\"checksumSha256\":\"" + checksum + "\",\"snapshot\":" + snapshot + "}";
    }

    private static int[] Append(int[] values, int value)
    {
        var result = new int[values.Length + 1];
        Array.Copy(values, result, values.Length);
        result[values.Length] = value;
        return result;
    }

    private static int[] Slice(int[] values, int start, int length)
    {
        var result = new int[length];
        Array.Copy(values, start, result, 0, length);
        return result;
    }

    public static int Run()
    {
        assertions = 0;
        TenStepsUndoExactly();
        EleventhRotationDropsOldest();
        UndoSurvivesSaveAndRestore();
        HintAndGuideInteractions();
        RestartAndNextBoardClearHistory();
        LegacySnapshotsLoad();
        CorruptHistoryRejected();
        GardenRoundTripWithHistory();
        Console.WriteLine("Evergrove undo history tests: " + assertions + " assertions.");
        return assertions;
    }
}
