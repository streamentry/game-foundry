using System;
using System.Collections.Generic;
using System.Text;
using Evergrove.Core;

/// <summary>
/// Honest depth leaves (idea 13): each engine measures how much reasoning a board needs
/// from its own solver, and Leaves (1..3) is that measure cut at thresholds chosen from
/// the real distribution over seeds 1..300 in every tier. Leaves is not the tier.
/// </summary>
internal static class DifficultyTests
{
    private static int assertions;
    private const int Seeds = 300;

    private static void Expect(bool ok, string message)
    {
        assertions++;
        if (!ok) throw new Exception("Difficulty test failed: " + message);
    }

    private sealed class Sample
    {
        public int Raw, Leaves;
    }

    private static string Mean(long sum, int count)
    {
        return count > 0 ? (sum / (double)count).ToString("0.0", System.Globalization.CultureInfo.InvariantCulture) : "-";
    }

    // One engine: builds (raw measure, leaves) for a seed and tier.
    private static void Check(string name, string rawName, Func<int, int, Sample> make)
    {
        var all = new Sample[TreePuzzles.Tiers][];
        var overall = new int[4];
        var sumRaw = new long[4];
        int minRawFor3 = int.MaxValue, maxRawFor1 = int.MinValue;
        bool someTierMixed = false;
        for (int tier = 0; tier < TreePuzzles.Tiers; tier++)
        {
            all[tier] = new Sample[Seeds];
            var perTier = new int[4];
            var rawHist = new SortedDictionary<int, int>();
            for (int seed = 1; seed <= Seeds; seed++)
            {
                Sample s = make(seed, tier);
                all[tier][seed - 1] = s;
                Expect(s.Leaves >= 1 && s.Leaves <= 3, name + " leaves is 1..3 (seed " + seed + ", tier " + tier + ")");
                perTier[s.Leaves]++;
                overall[s.Leaves]++;
                sumRaw[s.Leaves] += s.Raw;
                if (s.Leaves == 3) minRawFor3 = Math.Min(minRawFor3, s.Raw);
                if (s.Leaves == 1) maxRawFor1 = Math.Max(maxRawFor1, s.Raw);
                int n;
                rawHist.TryGetValue(s.Raw, out n);
                rawHist[s.Raw] = n + 1;
            }
            int distinct = (perTier[1] > 0 ? 1 : 0) + (perTier[2] > 0 ? 1 : 0) + (perTier[3] > 0 ? 1 : 0);
            if (distinct >= 2) someTierMixed = true;
            var sb = new StringBuilder();
            foreach (var kv in rawHist) sb.Append(' ').Append(kv.Key).Append('x').Append(kv.Value);
            Console.WriteLine(name + " tier " + tier + ": leaves 1/2/3 = " + perTier[1] + "/" + perTier[2] + "/" + perTier[3]
                + " of " + Seeds + "; " + rawName + " histogram:" + sb);
        }
        Console.WriteLine(name + " overall: leaves 1/2/3 = " + overall[1] + "/" + overall[2] + "/" + overall[3]
            + "; mean " + rawName + " " + Mean(sumRaw[1], overall[1]) + " / " + Mean(sumRaw[2], overall[2])
            + " / " + Mean(sumRaw[3], overall[3]));

        Expect(someTierMixed, name + ": within some tier at least two leaf values occur (leaves is not the tier)");
        int total = Seeds * TreePuzzles.Tiers;
        for (int v = 1; v <= 3; v++)
            Expect(overall[v] * 10 >= total, name + ": leaves " + v + " is at least 10% of boards overall");
        Expect(minRawFor3 > maxRawFor1, name + ": every leaves-3 board measures higher than every leaves-1 board");
        Expect(sumRaw[1] * overall[2] < sumRaw[2] * overall[1] && sumRaw[2] * overall[3] < sumRaw[3] * overall[2],
            name + ": the mean measure rises with the leaves");

        // Deterministic: a rebuilt board reports the same measure and leaves.
        for (int tier = 0; tier < TreePuzzles.Tiers; tier++)
            for (int seed = 1; seed <= Seeds; seed += 7)
            {
                Sample again = make(seed, tier);
                Expect(again.Raw == all[tier][seed - 1].Raw && again.Leaves == all[tier][seed - 1].Leaves,
                    name + ": measure is deterministic");
            }
    }

    public static int Run()
    {
        assertions = 0;
        Check("Seed sums", "passes", (seed, tier) =>
        {
            var g = new SeedSumsGame(seed, tier);
            return new Sample { Raw = g.DeductionPasses, Leaves = g.Leaves };
        });
        Check("Planting by numbers", "sweeps", (seed, tier) =>
        {
            var g = new NonogramGame(seed, tier);
            return new Sample { Raw = g.LineSweeps, Leaves = g.Leaves };
        });
        Check("Seed sudoku", "rounds", (seed, tier) =>
        {
            var g = new SeedSudokuGame(seed, tier);
            return new Sample { Raw = g.SolveRounds, Leaves = g.Leaves };
        });
        Check("Doorstep pattern", "decoy %", (seed, tier) =>
        {
            var g = new DoorstepGame(seed, tier);
            return new Sample { Raw = g.DecoyPercent, Leaves = g.Leaves };
        });
        ConsistencyChecks();
        return assertions;
    }

    // The public cut functions agree with the property, and the sudoku measure is coherent.
    private static void ConsistencyChecks()
    {
        int beyond = 0;
        for (int tier = 0; tier < TreePuzzles.Tiers; tier++)
            for (int seed = 1; seed <= Seeds; seed++)
            {
                var sums = new SeedSumsGame(seed, tier);
                Expect(sums.DeductionPasses >= 1 && sums.Leaves == SeedSumsGame.LeavesFor(sums.DeductionPasses), "seed sums leaves follow the passes");
                var bed = new NonogramGame(seed, tier);
                Expect(bed.LineSweeps >= 1 && bed.Leaves == NonogramGame.LeavesFor(bed.LineSweeps), "planting leaves follow the sweeps");
                var sudoku = new SeedSudokuGame(seed, tier);
                Expect(sudoku.SolveRounds >= 1 && sudoku.HiddenSinglesNeeded <= sudoku.SolveRounds, "sudoku rounds cover hidden singles");
                Expect(sudoku.Leaves == SeedSudokuGame.LeavesFor(sudoku.SolveRounds, sudoku.HiddenSinglesNeeded, sudoku.NeedsMoreThanSingles), "sudoku leaves follow the measure");
                Expect(sudoku.HiddenSinglesNeeded == 0 || sudoku.Leaves >= 2, "a hidden single is never a one-leaf bed");
                Expect(!sudoku.NeedsMoreThanSingles || sudoku.Leaves == 3, "a bed beyond singles is three leaves");
                if (sudoku.NeedsMoreThanSingles) beyond++;
                var door = new DoorstepGame(seed, tier);
                Expect(door.DecoyPercent >= 0 && door.DecoyPercent <= 100 && door.Leaves == DoorstepGame.LeavesFor(door.DecoyPercent), "doorstep leaves follow the decoys");
            }
        Console.WriteLine("Seed sudoku beds beyond singles (existing generator): " + beyond + " of " + Seeds * TreePuzzles.Tiers);
    }
}
