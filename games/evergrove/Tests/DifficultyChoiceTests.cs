using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Evergrove.Core;

// Item 34: the player's chosen leaves (1 gentlest .. 3 deepest) per puzzle kind. Every kind must
// have a gentle level whose boards are valid and measurably gentler than level 3 by the kind's own
// measure (leaves for the I336 kinds, board size for the rest), TierFor must be monotonic, and the
// remembered key must live in the Unity difficulty partial only.
internal static class DifficultyChoiceTests
{
    private static int assertions;
    private const int Sample = 50;

    private static void Expect(bool ok, string message)
    {
        assertions++;
        if (!ok) throw new Exception("DifficultyChoiceTests failed: " + message);
    }

    private static readonly PuzzleKind[] AllKinds = (PuzzleKind[])Enum.GetValues(typeof(PuzzleKind));

    public static int Run()
    {
        assertions = 0;
        Expect(AllKinds.Length == 12, "twelve puzzle kinds");
        TierMapping();
        foreach (PuzzleKind kind in AllKinds) GentleLevelPerKind(kind);
        KeyOnlyInDifficultyPartial();
        return assertions;
    }

    // TierFor is monotonic, level 1..3 round-trips through the tier, and clamps outside 1..3.
    private static void TierMapping()
    {
        Expect(DifficultyChoice.Levels == TreePuzzles.Tiers, "one level per tier");
        foreach (PuzzleKind kind in AllKinds)
        {
            int previous = -1;
            for (int level = 1; level <= DifficultyChoice.Levels; level++)
            {
                int tier = DifficultyChoice.TierFor(kind, level);
                Expect(tier >= 0 && tier < TreePuzzles.Tiers, "tier in range for " + kind + " level " + level);
                Expect(tier >= previous, "TierFor is monotonic for " + kind);
                Expect(DifficultyChoice.LevelOfTier(tier) == level, "level " + level + " round-trips for " + kind);
                previous = tier;
            }
            Expect(DifficultyChoice.TierFor(kind, -5) == 0 && DifficultyChoice.TierFor(kind, 99) == TreePuzzles.Tiers - 1,
                "levels clamp to 1..3 for " + kind);
            Expect(DifficultyChoice.GentleTier(kind) == 0, "the gentle tier is tier 0 for " + kind);
        }
    }

    // For one kind: sample seeds at level 1 and level 3, check validity, the leaves for the I336
    // kinds, and that the gentle board is never larger (or harder) than the deepest one.
    private static void GentleLevelPerKind(PuzzleKind kind)
    {
        bool logic = DifficultyChoice.MeasuresLogic(kind);
        int maxRawGentle = int.MinValue, minRawDeep = int.MaxValue;
        int maxSizeGentle = int.MinValue, minSizeDeep = int.MaxValue;
        for (int seed = 1; seed <= Sample; seed++)
        {
            DifficultyChoice.Deal gentle = DifficultyChoice.DealAt(kind, seed, DifficultyChoice.Gentlest);
            DifficultyChoice.Deal deep = DifficultyChoice.DealAt(kind, seed, DifficultyChoice.Levels);
            string where = kind + " seed " + seed;
            Expect(gentle.Tier == 0 && deep.Tier == TreePuzzles.Tiers - 1, "level tiers for " + where);
            Expect(gentle.Level == 1 && deep.Level == 3, "level recorded for " + where);
            Expect(IsValid(kind, gentle.Seed, gentle.Tier), "gentle board is valid: " + where);
            Expect(IsValid(kind, deep.Seed, deep.Tier), "deep board is valid: " + where);
            int sizeGentle = DifficultyChoice.SizeOf(kind, gentle.Seed, gentle.Tier);
            int sizeDeep = DifficultyChoice.SizeOf(kind, deep.Seed, deep.Tier);
            maxSizeGentle = Math.Max(maxSizeGentle, sizeGentle);
            minSizeDeep = Math.Min(minSizeDeep, sizeDeep);
            Expect(sizeGentle <= sizeDeep, "gentle board is no larger than deep: " + where);

            if (!logic) continue;
            Expect(DifficultyChoice.LeavesOf(kind, gentle.Seed, gentle.Tier) == 1, "gentle board measures 1 leaf: " + where);
            Expect(DifficultyChoice.LeavesOf(kind, deep.Seed, deep.Tier) == 3, "deep board measures 3 leaves: " + where);
            DifficultyChoice.Deal middle = DifficultyChoice.DealAt(kind, seed, 2);
            Expect(DifficultyChoice.LeavesOf(kind, middle.Seed, middle.Tier) == 2, "level 2 measures 2 leaves: " + where);
            maxRawGentle = Math.Max(maxRawGentle, DifficultyChoice.RawOf(kind, gentle.Seed, gentle.Tier));
            minRawDeep = Math.Min(minRawDeep, DifficultyChoice.RawOf(kind, deep.Seed, deep.Tier));
        }
        if (logic)
            Expect(maxRawGentle < minRawDeep, kind + ": every gentle board measures lower than every deep board");
        else
            Expect(maxSizeGentle < minSizeDeep, kind + ": gentle boards are measurably smaller than deep ones");
    }

    // A board that is actually solvable: the checks use each game's public state.
    private static bool IsValid(PuzzleKind kind, int seed, int tier)
    {
        switch (kind)
        {
            case PuzzleKind.Water:
            {
                Puzzle puzzle = TreePuzzles.For(seed, tier);
                return PipeRules.CountSolutions(puzzle, 2) == 1 && puzzle.Solution.Length == puzzle.CellCount;
            }
            case PuzzleKind.Pairs:
            {
                var game = new PairsGame(seed, tier);
                var counts = new Dictionary<int, int>();
                for (int card = 0; card < game.Count; card++)
                {
                    int face = game.Face(card);
                    counts[face] = counts.TryGetValue(face, out int n) ? n + 1 : 1;
                }
                return game.Count == PairsGame.PairsFor(tier) * 2 && counts.Values.All(n => n == 2);
            }
            case PuzzleKind.Mosaic:
            {
                var game = new MosaicGame(seed, tier);
                var slots = new HashSet<int>();
                for (int slot = 0; slot < game.Count; slot++) slots.Add(game.At(slot));
                return game.Size * game.Size == game.Count && slots.Count == game.Count
                    && slots.Min() == 0 && slots.Max() == game.Count - 1 && !game.Solved;
            }
            case PuzzleKind.Nonogram:
            {
                var game = new NonogramGame(seed, tier);
                for (int r = 0; r < game.Size; r++)
                {
                    var row = new int[game.Size];
                    for (int c = 0; c < game.Size; c++) row[c] = game.Solution(r * game.Size + c) ? 1 : 0;
                    if (!SameRuns(row, game.RowClues[r])) return false;
                }
                for (int c = 0; c < game.Size; c++)
                {
                    var column = new int[game.Size];
                    for (int r = 0; r < game.Size; r++) column[r] = game.Solution(r * game.Size + c) ? 1 : 0;
                    if (!SameRuns(column, game.ColumnClues[c])) return false;
                }
                return true;
            }
            case PuzzleKind.Mahjong:
            {
                var game = new MahjongGame(seed, tier);
                var counts = new Dictionary<int, int>();
                for (int tile = 0; tile < game.TileCount; tile++)
                {
                    int face = game.Face(tile);
                    counts[face] = counts.TryGetValue(face, out int n) ? n + 1 : 1;
                }
                return game.TileCount > 0 && game.HasMove && counts.Values.All(n => n % 2 == 0);
            }
            case PuzzleKind.Sudoku:
            {
                var game = new SeedSudokuGame(seed, tier);
                int size = game.Size;
                for (int i = 0; i < size; i++)
                {
                    var row = new HashSet<int>();
                    var column = new HashSet<int>();
                    for (int j = 0; j < size; j++)
                    {
                        row.Add(game.Solution(i * size + j));
                        column.Add(game.Solution(j * size + i));
                    }
                    if (row.Count != size || column.Count != size) return false;
                }
                for (int cell = 0; cell < game.Cells; cell++)
                    if (game.Given(cell) != 0 && game.Given(cell) != game.Solution(cell)) return false;
                return game.SolveRounds >= 1;
            }
            case PuzzleKind.Plots:
            {
                var game = new PlotsGame(seed, tier);
                int sum = 0;
                foreach (int marker in game.Markers) sum += game.Clue(marker);
                return game.Size == PlotsGame.SizeFor(tier) && sum == game.Size * game.Size && !game.Solved;
            }
            case PuzzleKind.StreamPath:
            {
                var game = new StreamPathGame(seed, tier);
                // The answer runs from spring to pond through touching cells, and the spring starts on it.
                int[] answer = game.SolutionPath();
                if (game.Size != StreamPathGame.SizeFor(tier) || answer.Length < 2) return false;
                if (answer[0] != game.Spring || answer[answer.Length - 1] != game.Pond || !game.OnPath(game.Spring)) return false;
                for (int i = 1; i < answer.Length; i++)
                    if (Math.Abs(answer[i] / game.Size - answer[i - 1] / game.Size) + Math.Abs(answer[i] % game.Size - answer[i - 1] % game.Size) != 1)
                        return false;
                return true;
            }
            case PuzzleKind.Sowing:
            {
                var game = new SowingGame(seed, tier);
                int seeds = 0;
                for (int bed = 1; bed <= game.Beds; bed++) seeds += game.Seeds(bed); // beds are numbered from 1
                return game.Beds > 0 && seeds == game.Total
                    && game.Total >= SowingGame.MinSeeds(tier) && game.Total <= SowingGame.MaxSeeds(tier);
            }
            case PuzzleKind.Sums:
                return SumsValid(new SeedSumsGame(seed, tier));
            case PuzzleKind.Pieces:
            {
                var game = new GardenPiecesGame(seed, tier);
                int cells = 0;
                for (int piece = 0; piece < game.PieceCount; piece++)
                {
                    int n = game.PieceCells(piece).Length / 2; // x, y pairs
                    if (n == 0) return false;
                    cells += n;
                }
                return cells == game.CellCount && game.PieceCount >= 3;
            }
            case PuzzleKind.Doorstep:
            {
                var game = new DoorstepGame(seed, tier);
                bool any = false;
                for (int cell = 0; cell < game.Size * game.Size; cell++)
                {
                    if (!game.Solution(cell)) continue;
                    any = true;
                    foreach (int other in game.Orbit(cell))
                        if (!game.Solution(other)) return false;
                }
                return any;
            }
            default:
                return false;
        }
    }

    // Every clue equals the sum of the open run it leads, across and down, and the answers are digits.
    private static bool SumsValid(SeedSumsGame game)
    {
        int w = game.Width, h = game.Height, cells = w * h;
        for (int cell = 0; cell < cells; cell++)
        {
            if (game.IsOpen(cell) && (game.Solution(cell) < 1 || game.Solution(cell) > 9)) return false;
            int across = game.ClueAcross(cell);
            if (across > 0 && SumRun(game, cell, 1, w, h) != across) return false;
            int down = game.ClueDown(cell);
            if (down > 0 && SumRun(game, cell, w, w, h) != down) return false;
        }
        return true;
    }

    // Sums the open cells after `from`, along a step: across stays in its row, down in its column.
    private static int SumRun(SeedSumsGame game, int from, int step, int w, int h)
    {
        int sum = 0;
        for (int next = from + step; next >= 0 && next < w * h; next += step)
        {
            if (step == 1 && next / w != from / w) break;
            if (!game.IsOpen(next)) break;
            sum += game.Solution(next);
        }
        return sum;
    }

    // The runs of 1s in a line equal the clue (zero entries in a clue mean none).
    private static bool SameRuns(int[] line, int[] clue)
    {
        var runs = new List<int>();
        int run = 0;
        foreach (int value in line)
        {
            if (value == 1) run++;
            else if (run > 0) { runs.Add(run); run = 0; }
        }
        if (run > 0) runs.Add(run);
        return runs.SequenceEqual(clue.Where(n => n > 0));
    }

    // The remembered level's key is written and read only in the Unity difficulty partial.
    private static void KeyOnlyInDifficultyPartial()
    {
        string scripts = FindScripts();
        Expect(scripts != null, "Assets/Scripts found from the working or binary directory");
        const string key = "evergrove:leaves:";
        string partial = Path.Combine(scripts, "Unity", "EvergroveApp.Difficulty.cs");
        Expect(File.Exists(partial), "EvergroveApp.Difficulty.cs exists");
        Expect(File.ReadAllText(partial).Contains(key), "the difficulty partial holds the leaves key");
        foreach (string file in Directory.GetFiles(scripts, "*.cs", SearchOption.AllDirectories))
        {
            if (Path.GetFullPath(file) == Path.GetFullPath(partial)) continue;
            Expect(!File.ReadAllText(file).Contains(key), "leaves key only in the difficulty partial: " + file);
        }
    }

    private static string FindScripts()
    {
        string[] starts = { Directory.GetCurrentDirectory(), AppContext.BaseDirectory };
        foreach (string start in starts)
        {
            string dir = start;
            while (!string.IsNullOrEmpty(dir))
            {
                string candidate = Path.Combine(dir, "games", "evergrove", "Assets", "Scripts");
                if (Directory.Exists(candidate)) return candidate;
                candidate = Path.Combine(dir, "Assets", "Scripts");
                if (Directory.Exists(candidate)) return candidate;
                dir = Path.GetDirectoryName(dir);
            }
        }
        return null;
    }
}
