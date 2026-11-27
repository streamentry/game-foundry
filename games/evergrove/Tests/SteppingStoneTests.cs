using System;
using System.Collections.Generic;
using System.Linq;
using Evergrove.Core;

/// <summary>
/// Stepping stones are the Stones decoration (I330): grid-snapped cell indices on free land, refusals that
/// change nothing, a session undo for the last 20 decoration steps, and a garden file that keeps them.
/// </summary>
internal static class SteppingStoneTests
{
    private static int assertions;
    private const string Stamp = "2026-11-27T12:00:00Z";

    private static void Expect(bool ok, string message)
    {
        assertions++;
        if (!ok) throw new Exception("SteppingStoneTests failed: " + message);
    }

    /// <summary>12 columns by 10 rows: a stream at row 1 (cells 13-15), a cottage at cell 52, the rest land.</summary>
    private static GardenMap Map()
    {
        string[] rows = new string[10];
        for (int r = 0; r < 10; r++) rows[r] = "............";
        rows[1] = ".~~~........";
        rows[4] = "...#........";
        return new GardenMap(12, 10, string.Concat(rows));
    }

    private static WorldModel Grown(int count, int seeds = 0)
    {
        WorldModel world = WorldModel.New(PlayMode.Living);
        for (int chapter = 0; chapter < count; chapter++)
        {
            Puzzle board = world.CurrentPuzzle;
            for (int i = 0; i < board.CellCount; i++)
                while (world.Rotations[i] != board.Solution[i]) world.Rotate(i);
            world.Choose((VillageChoice)(chapter % 3));
        }
        if (seeds == 0) return world;
        WorldSnapshot s = world.Export();
        s.seeds = seeds;
        Expect(WorldModel.TryRestore(s, out world), "rich garden restores");
        return world;
    }

    private static string State(WorldModel w) { return GardenTransfer.SnapshotJson(w.Export()); }

    /// <summary>A land cell that nothing stands on in this garden.</summary>
    private static int FreeCell(WorldModel w, GardenMap map, int skip = 0)
    {
        int[] occupied = w.Occupied;
        int[] layout = map.Layout(w.Choices, w.Placements, occupied);
        for (int cell = 0; cell < map.CellCount; cell++)
            if (map.IsFree(layout, occupied, cell) && skip-- <= 0) return cell;
        throw new Exception("no free cell");
    }

    /// <summary>Every decoration is a whole cell index on free land, one to a cell, within the map.</summary>
    private static void CheckGrid(WorldModel w, GardenMap map)
    {
        int[] decor = w.Decor;
        Expect(decor.Length % 2 == 0 && decor.Length / 2 <= WorldModel.MaxDecor, "pairs, at most sixty");
        var seen = new HashSet<int>();
        for (int i = 0; i < decor.Length; i += 2)
        {
            int kind = decor[i], cell = decor[i + 1];
            Expect(kind >= 0 && kind <= 3, "a known kind");
            Expect(cell >= 0 && cell < map.CellCount && map.IsLand(cell), "a whole land cell at " + cell);
            Expect(seen.Add(cell), "one piece to a cell at " + cell);
        }
    }

    public static int Run()
    {
        Placing();
        Refusals();
        UndoBasics();
        UndoLimit();
        UndoRefusedWhenItNoLongerFits();
        RandomWalkUndoesExactly();
        Saving();
        return assertions;
    }

    private static void Placing()
    {
        GardenMap map = Map();
        WorldModel w = Grown(3, 50);
        int a = FreeCell(w, map), b = FreeCell(w, map, 4);
        Expect(w.PlaceDecor(Decoration.Stones, a, map) && w.PlaceDecor(Decoration.Stones, b, map), "stones go on free land");
        Expect(w.Decor.SequenceEqual(new[] { 0, a, 0, b }), "stored as whole cell indices, kind Stones = 0");
        CheckGrid(w, map);
        Expect(w.Seeds == 50, "stones cost nothing");
    }

    private static void Refusals()
    {
        GardenMap map = Map();
        WorldModel w = Grown(3, 50);
        Expect(w.Plant(0, FreeCell(w, map)), "a seedling to refuse around");
        int stone = FreeCell(w, map);
        Expect(w.PlaceDecor(Decoration.Stones, stone, map), "a stone to refuse around");
        string before = State(w);
        int depth = w.DecorUndoDepth;
        int planting = w.Plantings[1];
        int[] invalid = { -1, -100, 13, 14, map.CellCount, map.CellCount + 1, WorldModel.MaxMapCell, stone, planting };
        foreach (int cell in invalid)
            Expect(!w.PlaceDecor(Decoration.Stones, cell, map), "refused cell " + cell);
        Expect(!w.PlaceDecor(Decoration.Stones, FreeCell(w, map), null), "a map is required");
        Expect(!w.PlaceDecor((Decoration)7, FreeCell(w, map), map), "a kind that does not exist");
        Expect(!w.MoveDecor(0, planting, map) && !w.MoveDecor(0, 13, map) && !w.MoveDecor(3, FreeCell(w, map), map), "moves refused");
        Expect(!w.RemoveDecor(5) && !w.RemoveDecor(-1), "removal of a missing piece refused");
        Expect(State(w) == before && w.DecorUndoDepth == depth, "refusals change nothing and leave no undo step");
    }

    private static void UndoBasics()
    {
        GardenMap map = Map();
        WorldModel w = Grown(3, 50);
        Expect(!w.CanUndoDecor && !w.UndoDecor(map), "nothing to undo in a new session");
        string start = State(w);
        int a = FreeCell(w, map), b = FreeCell(w, map, 3);
        w.PlaceDecor(Decoration.Stones, a, map);
        w.PlaceDecor(Decoration.Stones, b, map);
        Expect(w.DecorUndoDepth == 2, "two steps recorded");
        Expect(w.UndoDecor(map) && w.Decor.SequenceEqual(new[] { 0, a }), "undo takes back the last stone");
        Expect(w.UndoDecor(map) && State(w) == start, "undo takes back the first, back to the exact start");
        Expect(!w.CanUndoDecor && !w.UndoDecor(map) && State(w) == start, "a further undo changes nothing");
        Expect(!w.UndoDecor(null) && !WorldModel.New(PlayMode.PuzzleOnly).UndoDecor(map), "needs a map and a living garden");

        w.PlaceDecor(Decoration.Stones, a, map);
        w.PlaceDecor(Decoration.Stones, b, map);
        string twoStones = State(w);
        Expect(w.MoveDecor(1, FreeCell(w, map, 6), map), "a stone moves");
        Expect(w.UndoDecor(map) && State(w) == twoStones, "undo puts the moved stone back");
        Expect(w.RemoveDecor(0) && w.UndoDecor(map) && State(w) == twoStones, "undo puts a removed stone back in its order");
        Expect(w.UndoDecor(map) && w.Decor.SequenceEqual(new[] { 0, a }), "and the steps before still undo");
        Expect(w.UndoDecor(map) && State(w) == start, "and the garden is as it started");
    }

    private static void UndoLimit()
    {
        GardenMap map = Map();
        WorldModel w = Grown(2, 50);
        var states = new List<string>();
        for (int i = 0; i < 25; i++)
        {
            states.Add(State(w));
            Expect(w.PlaceDecor(Decoration.Stones, FreeCell(w, map), map), "stone " + i + " goes down");
        }
        Expect(w.DecorUndoDepth == WorldModel.UndoDecorLimit && WorldModel.UndoDecorLimit == 20, "twenty steps are kept");
        for (int i = 0; i < 20; i++)
            Expect(w.UndoDecor(map), "undo " + i);
        Expect(!w.CanUndoDecor && !w.UndoDecor(map), "the twenty-first is gone");
        Expect(w.DecorCount == 5 && State(w) == states[5], "the oldest five placements stay, as they were");
    }

    /// <summary>After a plant takes a cell an undo would bring a stone back to, the undo is refused and nothing changes.</summary>
    private static void UndoRefusedWhenItNoLongerFits()
    {
        GardenMap map = Map();
        WorldModel w = Grown(3, 50);
        int cell = FreeCell(w, map);
        w.PlaceDecor(Decoration.Stones, cell, map);
        w.RemoveDecor(0);
        Expect(w.Plant(0, cell), "the freed cell takes a seedling");
        string before = State(w);
        int depth = w.DecorUndoDepth;
        Expect(!w.UndoDecor(map), "the stone cannot come back onto the seedling");
        Expect(State(w) == before && w.DecorUndoDepth == depth && w.DecorCount == 0, "nothing changes, the step stays");
        Expect(w.Plantings[1] == cell, "the seedling stayed put");
    }

    /// <summary>500 seeded place, remove, move and undo steps: every undo restores the exact prior garden.</summary>
    private static void RandomWalkUndoesExactly()
    {
        GardenMap map = Map();
        WorldModel w = Grown(2, 50);
        var rng = new Random(47);
        var undoable = new List<string>(); // the garden before each step still on the stack, oldest first
        int placed = 0, removed = 0, moved = 0, undone = 0, refused = 0;
        for (int step = 0; step < 500; step++)
        {
            string before = State(w);
            int roll = rng.Next(4);
            if (roll == 0)
            {
                bool ok = w.PlaceDecor((Decoration)rng.Next(4), rng.Next(-2, map.CellCount + 2), map);
                if (ok) placed++; else refused++;
                Expect(ok || State(w) == before, "a refused placement changes nothing at step " + step);
                if (ok) Push(undoable, before);
            }
            else if (roll == 1)
            {
                bool ok = w.RemoveDecor(rng.Next(-1, w.DecorCount + 1));
                if (ok) { removed++; Push(undoable, before); } else refused++;
            }
            else if (roll == 2)
            {
                bool ok = w.MoveDecor(rng.Next(-1, w.DecorCount + 1), rng.Next(-2, map.CellCount + 2), map);
                if (ok) { moved++; if (State(w) != before) Push(undoable, before); } else refused++;
            }
            else
            {
                if (undoable.Count == 0)
                {
                    Expect(!w.UndoDecor(map) && State(w) == before, "nothing to undo at step " + step);
                    continue;
                }
                string back = undoable[undoable.Count - 1];
                undoable.RemoveAt(undoable.Count - 1);
                Expect(w.UndoDecor(map), "undo succeeds at step " + step);
                Expect(State(w) == back, "undo restores the exact prior garden at step " + step);
                undone++;
            }
            Expect(w.DecorUndoDepth == undoable.Count, "undo depth tracks the steps at " + step);
            CheckGrid(w, map);
        }
        Expect(placed > 40 && removed > 20 && moved > 20 && undone > 40 && refused > 20,
            "the walk exercised every operation (" + placed + "/" + removed + "/" + moved + "/" + undone + "/" + refused + ")");
    }

    private static void Push(List<string> undoable, string before)
    {
        undoable.Add(before);
        if (undoable.Count > WorldModel.UndoDecorLimit) undoable.RemoveAt(0);
    }

    private static void Saving()
    {
        GardenMap map = Map();
        WorldModel w = Grown(3, 80);
        w.Buy(0, FreeCell(w, map));
        w.Plant(1, FreeCell(w, map));
        w.PlaceDecor(Decoration.Stones, FreeCell(w, map), map);
        w.PlaceDecor(Decoration.Stones, FreeCell(w, map), map);
        Expect(WorldModel.TryRestore(w.Export(), out WorldModel back), "a garden with stones restores");
        Expect(back.Decor.SequenceEqual(w.Decor) && State(back) == State(w), "stones survive a save");
        Expect(!back.CanUndoDecor && back.DecorUndoDepth == 0, "the undo steps are not saved");

        string file = GardenTransfer.Export(w, Stamp);
        Expect(GardenTransfer.TryImport(file, out WorldModel copy, out GardenImportError error) && copy.Decor.SequenceEqual(w.Decor),
            "a garden file keeps the stones (" + error + ")");
        Expect(GardenTransfer.Export(copy, Stamp) == file, "and exports back byte-identical");

        // Forged saves: each is re-checksummed, so only the structural rules can refuse it.
        WorldSnapshot forged = w.Export();
        forged.decor = new[] { 0, w.Plantings[1] };
        Expect(!WorldModel.TryRestore(forged, out _), "stones on a planting refused");
        forged = w.Export();
        forged.decor = new[] { 0, w.Bought[1] };
        Expect(!WorldModel.TryRestore(forged, out _), "stones on a bought plant refused");
        forged = w.Export();
        forged.decor = new[] { 0, 9, 0, 9 };
        Expect(!WorldModel.TryRestore(forged, out _), "two stones on one cell refused");
        forged = w.Export();
        forged.decor = new[] { 0, -3 };
        Expect(!WorldModel.TryRestore(forged, out _), "a negative cell refused");
        forged = w.Export();
        forged.decor = new[] { 4, 9 };
        Expect(!WorldModel.TryRestore(forged, out _), "a fifth kind refused");
        forged = w.Export();
        forged.decor = new[] { 0, 9, 0 };
        Expect(!WorldModel.TryRestore(forged, out _), "a half pair refused");
        string storedDecor = "\"decor\":[" + string.Join(",", w.Decor) + "]";
        Expect(file.Contains(storedDecor), "the file carries the stones");
        Expect(!GardenTransfer.TryImport(file.Replace(storedDecor, "\"decor\":[0,-3]"), out _, out _), "an edited stone cell fails the checksum");
    }
}
