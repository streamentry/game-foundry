using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Evergrove.Core;

/// <summary>
/// Step 39, undo everywhere. Every board kind with moves gets a seeded random walk of
/// 300 changing moves, with undo (30%) and redo (10%) interleaved. After each undo the board
/// equals exactly the state before that move; after each redo, the state after it; a new move
/// clears redo; undo back to the start restores the starting board. The walk runs through the
/// same BoardHistory the boards use, so it exercises the real replay path.
/// </summary>
internal static class UndoEverywhereTests
{
    private const int Steps = 300;
    private static int assertions;

    private static void Expect(bool ok, string message)
    {
        assertions++;
        if (!ok) throw new Exception("UndoEverywhereTests failed: " + message);
    }

    public static int Run()
    {
        for (int tier = 0; tier < TreePuzzles.Tiers; tier++)
        {
            int t = tier;
            int seed = 1000 + tier * 17;
            Walk("Nonogram", seed, t, (s, k) => new NonogramGame(s, k), DescribeNonogram, PickNonogram);
            Walk("Sowing", seed, t, (s, k) => new SowingGame(s, k), DescribeSowing, PickSowing);
            Walk("Doorstep", seed, t, (s, k) => new DoorstepGame(s, k), DescribeDoorstep, PickDoorstep);
            Walk("Stream", seed, t, (s, k) => new StreamPathGame(s, k), DescribeStream, PickStream);
            Walk("Pieces", seed, t, (s, k) => new GardenPiecesGame(s, k), DescribePieces, PickPieces);
            Walk("Sudoku", seed, t, (s, k) => new SeedSudokuGame(s, k), DescribeSudoku, PickSudoku);
            Walk("Plots", seed, t, (s, k) => new PlotsGame(s, k), DescribePlots, PickPlots);
            Walk("Mahjong", seed, t, (s, k) => new MahjongGame(s, k), DescribeMahjong, PickMahjong);
            Walk("Sums", seed, t, (s, k) => new SeedSumsGame(s, k), DescribeSums, PickSums);
            Walk("Pairs", seed, t, (s, k) => new PairsGame(s, k), DescribePairs, PickPairs);
            Walk("Mosaic", seed, t, (s, k) => new MosaicGame(s, k), DescribeMosaic, PickMosaic);
        }
        WaterWalk();
        WaterSaveStillCapped();
        WaterControlsSource();
        return assertions;
    }

    // The water board's Undo row must offer Redo and a step count, from WorldModel's own API.
    private static void WaterControlsSource()
    {
        string root = FindRepoRoot();
        Expect(root != null, "could not locate games/evergrove from the working or base directory");
        string app = File.ReadAllText(Path.Combine(root, "games", "evergrove", "Assets", "Scripts", "Unity", "EvergroveApp.cs"));
        Expect(app.Contains("world.Redo()"), "water controls: the Redo token calls WorldModel.Redo()");
        Expect(app.Contains("world.RedoDepth") || app.Contains("world.CanRedo"),
            "water controls: the Redo token is enabled from WorldModel's redo depth");
        Expect(app.Contains("world.UndoDepth") && app.Contains("undoStepCount.text"),
            "water controls: the Undo row shows WorldModel's undo step count");
        Expect(app.Contains("\"Undo, \"") && app.Contains("\"Redo, \""),
            "water controls: the tokens are named \"Undo, N steps\" and \"Redo, N steps\" like BoardKit's row");
        Expect(app.Contains("RefreshUndoRow();"), "water controls: the row is refreshed after each action");
    }

    private static string FindRepoRoot()
    {
        foreach (string start in new[] { Directory.GetCurrentDirectory(), AppContext.BaseDirectory })
        {
            string dir = Path.GetFullPath(start);
            for (int up = 0; up < 12 && dir != null; up++)
            {
                if (File.Exists(Path.Combine(dir, "games", "evergrove", "Assets", "Scripts", "Unity", "EvergroveApp.cs")))
                    return dir;
                dir = Directory.GetParent(dir)?.FullName;
            }
        }
        return null;
    }

    // ---- The walk: one loop for every kind. `pick` returns the next changing move, a move that
    // may change nothing, or null when the board can go no further (solved, or no ripe bed); a
    // null pick starts the board over, as the app does, and forgets its steps.
    private static void Walk<TGame>(string kind, int seed, int tier, Func<int, int, TGame> make,
                                    Func<TGame, string> describe,
                                    Func<Random, TGame, Func<TGame, bool>> pick) where TGame : class
    {
        string label = kind + " tier " + tier;
        var rng = new Random(seed + tier);
        string start = describe(make(seed, tier));
        var history = new BoardHistory<TGame>(make(seed, tier), () => make(seed, tier));
        Expect(describe(history.Current) == start, label + ": history starts on the dealt board");

        var before = new List<string>(); // state before each undoable move, oldest first
        var undone = new List<string>(); // state after each undone move, oldest first (redo targets)
        string current = start;
        int moves = 0, undos = 0, redos = 0, resets = 0;
        for (int guard = 0; moves < Steps && guard < Steps * 200; guard++)
        {
            double roll = rng.NextDouble();
            if (roll < 0.30 && history.CanUndo)
            {
                Expect(history.Undo(), label + ": undo succeeds when a step is recorded");
                string target = before[before.Count - 1];
                before.RemoveAt(before.Count - 1);
                undone.Add(current);
                current = target;
                undos++;
                Expect(describe(history.Current) == current, label + ": undo restores the exact state before the move");
            }
            else if (roll < 0.40 && history.CanRedo)
            {
                Expect(history.Redo(), label + ": redo succeeds when a step is undone");
                string target = undone[undone.Count - 1];
                undone.RemoveAt(undone.Count - 1);
                before.Add(current);
                current = target;
                redos++;
                Expect(describe(history.Current) == current, label + ": redo restores the exact state after the move");
            }
            else
            {
                Func<TGame, bool> move = pick(rng, history.Current);
                if (move == null)
                {
                    history.Reset();
                    before.Clear();
                    undone.Clear();
                    current = start;
                    resets++;
                    Expect(describe(history.Current) == start && history.UndoSteps == 0 && history.RedoSteps == 0,
                        label + ": a restart returns to the dealt board with no steps");
                    continue;
                }
                Expect(describe(history.Current) == current, label + ": the live board matches the model before a move");
                bool changed = history.Play(move, b => b);
                if (!changed)
                {
                    Expect(describe(history.Current) == current && history.UndoSteps == before.Count
                        && history.RedoSteps == undone.Count, label + ": a refused move changes nothing and records nothing");
                    continue;
                }
                moves++;
                before.Add(current);
                current = describe(history.Current);
                undone.Clear();
                Expect(!history.CanRedo && history.RedoSteps == 0, label + ": a new move clears redo");
            }
            Expect(history.UndoSteps == before.Count && history.RedoSteps == undone.Count,
                label + ": step counts match the moves made");
        }
        Expect(moves >= Steps, label + ": " + Steps + " changing moves were made (got " + moves + ")");
        Expect(undos > 0 && redos > 0, label + ": the walk used both undo and redo");

        // Back to the very start, one undo at a time, each exact; then the dealt board again.
        while (history.CanUndo)
        {
            Expect(history.Undo(), label + ": undo back to the start");
            string target = before[before.Count - 1];
            before.RemoveAt(before.Count - 1);
            Expect(describe(history.Current) == target, label + ": the unwinding restores each earlier state exactly");
            current = target;
        }
        Expect(describe(history.Current) == start, label + ": undo back to the start restores the starting board exactly");
        Expect(history.UndoSteps == 0 && !history.Undo(), label + ": nothing left to undo at the start");
    }

    // ---- Random moves per kind. Each returns a changing-or-not move on the live board.

    private static Func<NonogramGame, bool> PickNonogram(Random rng, NonogramGame g)
    {
        if (g.Solved) return null;
        int cell = rng.Next(g.Size * g.Size);
        return x => x.Tap(cell) != NonogramMove.Ignored;
    }

    private static Func<SowingGame, bool> PickSowing(Random rng, SowingGame g)
    {
        if (g.Solved) return null;
        var ripe = new List<int>();
        for (int bed = 1; bed <= g.Beds; bed++) if (g.IsRipe(bed)) ripe.Add(bed);
        if (ripe.Count == 0) return null; // stuck: the board starts again
        int pick = ripe[rng.Next(ripe.Count)];
        return x => x.Sow(pick) == SowMove.Sowed;
    }

    private static Func<DoorstepGame, bool> PickDoorstep(Random rng, DoorstepGame g)
    {
        if (g.Solved) return null;
        int cell = rng.Next(g.Size * g.Size);
        return x => x.Tap(cell) == DoorMove.Changed;
    }

    private static Func<StreamPathGame, bool> PickStream(Random rng, StreamPathGame g)
    {
        if (g.Solved) return null;
        int n = g.Size;
        int[] path = g.Path;
        int tip = path[path.Length - 1];
        int row = tip / n, column = tip % n;
        int cell = rng.Next(n * n);
        if (rng.NextDouble() < 0.8)
        {
            var around = new List<int>();
            if (row > 0) around.Add(tip - n);
            if (row < n - 1) around.Add(tip + n);
            if (column > 0) around.Add(tip - 1);
            if (column < n - 1) around.Add(tip + 1);
            cell = around[rng.Next(around.Count)];
        }
        int at = cell;
        return x =>
        {
            StreamPathGame.PathMove m = x.Step(at);
            return m == StreamPathGame.PathMove.Extended || m == StreamPathGame.PathMove.Trimmed;
        };
    }

    private static Func<GardenPiecesGame, bool> PickPieces(Random rng, GardenPiecesGame g)
    {
        if (g.Solved) return null;
        int piece = rng.Next(g.PieceCount);
        if (g.Placed(piece))
            return x => x.Lift(piece) == PieceMove.Done;
        double roll = rng.NextDouble();
        if (roll < 0.45) return x => x.Rotate(piece) == PieceMove.Done;
        if (roll < 0.6) return x => x.Mirror(piece) == PieceMove.Done;
        int cell = rng.Next(g.Width * g.Height);
        return x => x.Place(piece, cell) == PieceMove.Done;
    }

    private static Func<SeedSudokuGame, bool> PickSudoku(Random rng, SeedSudokuGame g)
    {
        if (g.Solved) return null;
        int cell = rng.Next(g.Cells);
        for (int tries = 0; tries < 50 && g.IsGiven(cell); tries++) cell = rng.Next(g.Cells);
        int value = rng.Next(0, g.Size + 1);
        return x => x.Place(cell, value) != SudokuMove.Ignored;
    }

    private static Func<PlotsGame, bool> PickPlots(Random rng, PlotsGame g)
    {
        if (g.Solved) return null;
        int n = g.Size;
        int from = rng.Next(n * n);
        int to = from;
        if (rng.NextDouble() < 0.6)
        {
            int row = from / n, column = from % n;
            to = (row + rng.Next(n - row)) * n + column + rng.Next(n - column);
        }
        return x => x.Draw(from, to) != PlotMove.Invalid;
    }

    private static Func<MahjongGame, bool> PickMahjong(Random rng, MahjongGame g)
    {
        if (g.Solved) return null;
        if (!g.HasMove) return x => x.Reshuffle();
        int tile = rng.Next(g.TileCount);
        return x =>
        {
            MahjongMove m = x.Select(tile);
            return m != MahjongMove.Ignored && m != MahjongMove.NotFree;
        };
    }

    private static Func<SeedSumsGame, bool> PickSums(Random rng, SeedSumsGame g)
    {
        if (g.Solved) return null;
        int cells = g.Width * g.Height;
        int cell = rng.Next(cells);
        for (int tries = 0; tries < 50 && !g.IsOpen(cell); tries++) cell = rng.Next(cells);
        int digit = rng.Next(0, 10);
        return x => x.Place(cell, digit) != SumMove.Ignored;
    }

    private static Func<PairsGame, bool> PickPairs(Random rng, PairsGame g)
    {
        if (g.Solved) return null;
        int card = rng.Next(g.Count);
        return x => x.Flip(card) != PairsGame.Turn.Ignored;
    }

    private static Func<MosaicGame, bool> PickMosaic(Random rng, MosaicGame g)
    {
        if (g.Solved) return null;
        int slot = rng.Next(g.Count);
        return x => x.Select(slot) != MosaicGame.Step.Ignored;
    }

    // ---- The public state of each board, read through its public API only. Two boards with the
    // same description are the same board for the player.

    private static string Describe(string kind, params object[] parts)
    {
        var text = new StringBuilder(kind);
        foreach (object part in parts) text.Append('|').Append(part);
        return text.ToString();
    }

    private static string Cells(int count, Func<int, object> at)
    {
        var text = new StringBuilder();
        for (int c = 0; c < count; c++) text.Append(at(c)).Append(',');
        return text.ToString();
    }

    private static string Ints(int[] values)
    {
        return string.Join(",", values);
    }

    private static string DescribeNonogram(NonogramGame g)
    {
        return Describe("nonogram", g.Moves, g.Attempts, g.Solved, Cells(g.Size * g.Size, c => g.State(c)));
    }

    private static string DescribeSowing(SowingGame g)
    {
        var beds = new StringBuilder();
        for (int bed = 1; bed <= g.Beds; bed++) beds.Append(g.Seeds(bed)).Append(',');
        return Describe("sowing", g.Moves, g.InBasket, g.Solved, beds);
    }

    private static string DescribeDoorstep(DoorstepGame g)
    {
        return Describe("doorstep", g.Moves, g.Solved, Cells(g.Size * g.Size, c => g.State(c)));
    }

    private static string DescribeStream(StreamPathGame g)
    {
        return Describe("stream", g.Moves, g.Solved, g.OpenCount, Ints(g.Path));
    }

    private static string DescribePieces(GardenPiecesGame g)
    {
        var pieces = new StringBuilder();
        for (int p = 0; p < g.PieceCount; p++)
            pieces.Append(g.Placed(p) ? g.PlacedAt(p) : -1).Append('/').Append(g.Turn(p)).Append('/')
                  .Append(g.Mirrored(p) ? 1 : 0).Append(',');
        return Describe("pieces", g.Moves, g.Solved, g.Perimeter, pieces, Cells(g.CellCount, c => g.Owner(c)));
    }

    private static string DescribeSudoku(SeedSudokuGame g)
    {
        return Describe("sudoku", g.Moves, g.Solved, Cells(g.Cells, c => g.Value(c)));
    }

    private static string DescribePlots(PlotsGame g)
    {
        return Describe("plots", g.Moves, g.Solved, g.LastReplaced, Ints(g.Markers), Cells(g.Size * g.Size, c => g.BedAt(c)));
    }

    private static string DescribeMahjong(MahjongGame g)
    {
        return Describe("mahjong", g.Moves, g.Reshuffles, g.Selected, g.Remaining,
            Cells(g.TileCount, t => (g.Removed(t) ? "gone" : "here") + g.Face(t) + "@" + g.X(t) + "," + g.Y(t) + "," + g.Z(t)));
    }

    private static string DescribeSums(SeedSumsGame g)
    {
        return Describe("sums", g.Moves, g.Solved, Cells(g.Width * g.Height, c => g.Value(c)));
    }

    private static string DescribePairs(PairsGame g)
    {
        return Describe("pairs", g.Turns, g.Solved, Cells(g.Count, c => (g.Kept(c) ? "k" : "-") + (g.Showing(c) ? "u" : "-")));
    }

    private static string DescribeMosaic(MosaicGame g)
    {
        return Describe("mosaic", g.Moves, g.Picked, g.Solved, Cells(g.Count, s => g.At(s)));
    }

    // ---- Water (WorldModel): in session the undo history is not capped; saves keep ten boards.

    private static List<int> MovableTiles(Puzzle board)
    {
        var tiles = new List<int>();
        for (int i = 0; i < board.CellCount; i++)
            if (!board.IsAnchored(i)) tiles.Add(i);
        return tiles;
    }

    private static string Key(int[] values)
    {
        return string.Join(",", values);
    }

    private static void WaterWalk()
    {
        const int Rotations = 25;
        WorldModel world = WorldModel.New();
        List<int> tiles = MovableTiles(world.CurrentPuzzle);
        var states = new List<string> { Key(world.Rotations) };
        for (int step = 0; step < Rotations; step++)
        {
            Expect(world.Rotate(tiles[step % tiles.Count]), "water: a movable tile turns");
            states.Add(Key(world.Rotations));
        }
        Expect(world.UndoDepth == Rotations && world.CanUndo, "water: every in-session move is an undo step, past ten");
        for (int step = Rotations; step > 0; step--)
        {
            Expect(world.Undo(), "water: undo " + (Rotations - step + 1));
            Expect(Key(world.Rotations) == states[step - 1], "water: undo restores the exact board after " + (step - 1) + " turns");
        }
        Expect(!world.CanUndo && world.UndoDepth == 0 && !world.Undo(), "water: undo stops at the starting board");
        Expect(Key(world.Rotations) == states[0], "water: the starting board is restored exactly");

        for (int step = 0; step < Rotations; step++)
        {
            Expect(world.CanRedo && world.Redo(), "water: redo " + (step + 1));
            Expect(Key(world.Rotations) == states[step + 1], "water: redo restores the exact board after " + (step + 1) + " turns");
        }
        Expect(!world.CanRedo && world.RedoDepth == 0 && !world.Redo(), "water: redo stops at the newest board");

        Expect(world.Undo() && world.CanRedo && world.RedoDepth == 1, "water: one undo leaves one redo");
        Expect(world.Rotate(tiles[0]) && !world.CanRedo, "water: a new turn clears redo");
        Expect(world.UndoDepth == Rotations, "water: the new turn is one more step (undo count unchanged by the undo)");
    }

    private static void WaterSaveStillCapped()
    {
        WorldModel world = WorldModel.New();
        List<int> tiles = MovableTiles(world.CurrentPuzzle);
        for (int step = 0; step < 14; step++) Expect(world.Rotate(tiles[step % tiles.Count]), "water save: a turn");
        int cells = world.CurrentPuzzle.CellCount;
        WorldSnapshot saved = world.Export();
        Expect(saved.history.Length == WorldModel.UndoLimit * cells,
            "water save: a save keeps the newest " + WorldModel.UndoLimit + " boards only");
        WorldModel reopened;
        Expect(WorldModel.TryRestore(saved, out reopened) && reopened.UndoDepth == WorldModel.UndoLimit,
            "water save: a reopened board has at most " + WorldModel.UndoLimit + " undo steps");
        Expect(reopened.RedoDepth == 0, "water save: redo is session only and is not saved");
    }
}
