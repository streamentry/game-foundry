using System;
using System.Collections.Generic;
using Evergrove.Core;

/// <summary>
/// Seed Sudoku technique detector (v1 #36): crafted 4x4 positions, then agreement with an
/// independent singles solver on 1,000 generated boards.
/// </summary>
internal static class TechniqueTests
{
    private static int assertions;

    private static void Expect(bool ok, string message)
    {
        assertions++;
        if (!ok) throw new Exception("TechniqueTests failed: " + message);
    }

    private static void Agree(bool ok, int seed, int cell, int value, string what)
    {
        assertions++;
        if (!ok) throw new Exception("Technique detector disagrees: seed " + seed + " cell " + cell + " value " + value + ": " + what);
    }

    // Solved 4x4 bed (2x2 boxes) used for the crafted positions.
    private static readonly int[] Solved4 = { 1, 2, 3, 4, 3, 4, 1, 2, 2, 1, 4, 3, 4, 3, 2, 1 };

    public static int Run()
    {
        CraftedPositions();
        AgreementOnGeneratedBoards();
        return assertions;
    }

    private static void CraftedPositions()
    {
        // Only one plant fits cell 0 (its row, column and box hold 2, 3 and 4): naked single.
        var lone = (int[])Solved4.Clone();
        lone[0] = 0;
        Expect(SudokuTechnique.Classify(4, 2, 2, lone, 0, 1) == SudokuTechnique.Technique.NakedSingle,
            "one candidate left is a naked single");

        // The same placement is also the only place for plant 1 in row 0: naked wins the tie.
        Expect(SudokuTechnique.Classify(4, 2, 2, lone, 0, 1) != SudokuTechnique.Technique.HiddenSingle,
            "both-apply reports the simpler naked single");

        // A plant that repeats in the row is a contradiction, not a single.
        Expect(SudokuTechnique.Classify(4, 2, 2, lone, 0, 3) == SudokuTechnique.Technique.None,
            "plant already in the row is not a single");

        // Hidden single: cell 0 keeps plants 1..4, but plant 1 can go nowhere else in row 0
        // because column 1 (index 9), column 2 (index 14) and column 3 (index 7) each hold a 1.
        var hidden = new int[16];
        hidden[9] = 1;
        hidden[14] = 1;
        hidden[7] = 1;
        Expect(SudokuTechnique.Classify(4, 2, 2, hidden, 0, 1) == SudokuTechnique.Technique.HiddenSingle,
            "only place for plant 1 in row 0 is a hidden single");

        // Cell 0 can still take 2, so 2 is a guess here: not a single.
        Expect(SudokuTechnique.Classify(4, 2, 2, hidden, 0, 2) == SudokuTechnique.Technique.None,
            "a plant with several places is a guess");

        // Cell 2 already shares column 2 with a 1: placing 1 there is a contradiction.
        Expect(SudokuTechnique.Classify(4, 2, 2, hidden, 2, 1) == SudokuTechnique.Technique.None,
            "plant repeating in a unit is not a single");

        // Filled, out-of-range and wrong-size input names nothing.
        Expect(SudokuTechnique.Classify(4, 2, 2, hidden, 9, 2) == SudokuTechnique.Technique.None,
            "filled cell is not a single");
        Expect(SudokuTechnique.Classify(4, 2, 2, hidden, -1, 1) == SudokuTechnique.Technique.None,
            "out of range cell is not a single");
        Expect(SudokuTechnique.Classify(4, 2, 2, hidden, 0, 0) == SudokuTechnique.Technique.None,
            "clearing a cell is not a single");
        Expect(SudokuTechnique.Classify(4, 2, 2, new int[15], 0, 1) == SudokuTechnique.Technique.None,
            "board of the wrong size names nothing");

        Expect(SudokuTechnique.Name(SudokuTechnique.Technique.NakedSingle) == "One plant left", "naked single name");
        Expect(SudokuTechnique.Name(SudokuTechnique.Technique.HiddenSingle) == "Only place for it", "hidden single name");
        Expect(SudokuTechnique.Name(SudokuTechnique.Technique.None) == "", "no technique has no name");
    }

    private static void AgreementOnGeneratedBoards()
    {
        const int boards = 1000;
        int tiers = TreePuzzles.Tiers;
        int placements = 0, naked = 0, hiddenCount = 0;
        var clock = System.Diagnostics.Stopwatch.StartNew();

        for (int seed = 0; seed < boards; seed++)
        {
            int tier = seed % tiers;
            var game = new SeedSudokuGame(seed, tier);
            int size = game.Size, br = game.BoxRows, bc = game.BoxColumns, n = game.Cells;
            var units = Units(size, br, bc);

            // Independent replay: the test's own candidate masks and single search, not the game's.
            var values = new int[n];
            for (int i = 0; i < n; i++) values[i] = game.Given(i);

            while (true)
            {
                var masks = new int[n];
                int empty = 0;
                for (int i = 0; i < n; i++)
                {
                    if (values[i] != 0) continue;
                    empty++;
                    masks[i] = Holdable(values, size, br, bc, i);
                }
                if (empty == 0) break;

                int cell = -1, value = 0;
                bool isNaked = false;
                for (int i = 0; i < n && cell < 0; i++)
                {
                    if (values[i] != 0 || masks[i] == 0 || (masks[i] & (masks[i] - 1)) != 0) continue;
                    cell = i; value = Bit(masks[i]); isNaked = true;
                }
                // No naked single: take a hidden single (a plant with exactly one empty home in a unit).
                for (int u = 0; u < units.Count && cell < 0; u++)
                {
                    int[] unit = units[u];
                    for (int v = 1; v <= size && cell < 0; v++)
                    {
                        int homes = 0, home = -1;
                        foreach (int i in unit)
                            if (values[i] == 0 && (masks[i] & (1 << v)) != 0) { homes++; home = i; }
                        if (homes == 1) { cell = home; value = v; }
                    }
                }
                Agree(cell >= 0, seed, cell, value, "solver stuck: the bed needs more than singles");

                // The single the loop used must be the game's own solution.
                Agree(value == game.Solution(cell), seed, cell, value, "single is not the solution");

                // Classify agrees with the single's kind, on the array board and on the game.
                var expected = isNaked ? SudokuTechnique.Technique.NakedSingle : SudokuTechnique.Technique.HiddenSingle;
                Agree(SudokuTechnique.Classify(size, br, bc, values, cell, value) == expected, seed, cell, value,
                    "array classify vs " + expected);
                Agree(SudokuTechnique.Classify(game, cell, value) == expected, seed, cell, value,
                    "game classify vs " + expected);

                // Any wrong plant for this cell must never be called a single.
                for (int w = 1; w <= size; w++)
                {
                    if (w == game.Solution(cell)) continue;
                    Agree(SudokuTechnique.Classify(size, br, bc, values, cell, w) == SudokuTechnique.Technique.None,
                        seed, cell, w, "wrong plant called a single");
                }

                values[cell] = value;
                Agree(game.Place(cell, value) == SudokuMove.Placed, seed, cell, value, "game refused placement");
                placements++;
                if (isNaked) naked++; else hiddenCount++;
            }

            for (int i = 0; i < n; i++)
                Agree(values[i] == game.Solution(i), seed, i, values[i], "replay did not end at the solution");
            Agree(game.Solved, seed, -1, 0, "game not solved after replay");
        }

        Expect(placements > boards, "boards carry many placements");
        Console.WriteLine("Technique detector: " + boards + " boards, " + placements + " placements, naked "
            + naked + " / hidden " + hiddenCount + ", 0 disagreements, " + clock.ElapsedMilliseconds + " ms");
    }

    // Bit v set when plant v can still go in the cell (row, column and box free of it).
    private static int Holdable(int[] values, int size, int br, int bc, int cell)
    {
        int mask = 0;
        for (int v = 1; v <= size; v++)
        {
            bool ok = true;
            for (int o = 0; o < values.Length && ok; o++)
                if (values[o] == v && Peers(size, br, bc, cell, o)) ok = false;
            if (ok) mask |= 1 << v;
        }
        return mask;
    }

    private static int Bit(int mask)
    {
        int v = 0;
        while ((mask >> v) != 1) v++;
        return v;
    }

    private static bool Peers(int size, int br, int bc, int a, int b)
    {
        int ar = a / size, ac = a % size, brow = b / size, bcol = b % size;
        return ar == brow || ac == bcol || (ar / br == brow / br && ac / bc == bcol / bc);
    }

    // Every row, column and box as a list of cell indices.
    private static List<int[]> Units(int size, int br, int bc)
    {
        var units = new List<int[]>();
        for (int r = 0; r < size; r++)
        {
            var row = new int[size];
            for (int c = 0; c < size; c++) row[c] = r * size + c;
            units.Add(row);
        }
        for (int c = 0; c < size; c++)
        {
            var col = new int[size];
            for (int r = 0; r < size; r++) col[r] = r * size + c;
            units.Add(col);
        }
        for (int r0 = 0; r0 < size; r0 += br)
            for (int c0 = 0; c0 < size; c0 += bc)
            {
                var box = new List<int>();
                for (int r = r0; r < r0 + br; r++)
                    for (int c = c0; c < c0 + bc; c++) box.Add(r * size + c);
                units.Add(box.ToArray());
            }
        return units;
    }
}
