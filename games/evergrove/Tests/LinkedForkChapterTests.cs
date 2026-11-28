using System;
using System.Collections.Generic;
using Evergrove.Core;

/// <summary>
/// Pins the linked-fork chapter draw: qualifying seeds are an aha moment with the true orientation kept,
/// the draw is deterministic and sorted by work, and the locked seeds and work curve stay put.
/// Solver gate only; nothing in play uses these seeds.
/// </summary>
internal static class LinkedForkChapterTests
{
    private const int Seeds = 60;
    private const int Length = 12;
    private const int ExpectedQualifying = 47;
    private static readonly int[] ExpectedSeeds = { 5, 15, 24, 35, 46, 2, 11, 37, 20, 47, 4, 3 };
    private static readonly int[] ExpectedWork = { 4, 4, 4, 4, 4, 6, 6, 6, 8, 8, 9, 17 };
    private static int assertions;

    private static void Expect(bool ok, string message)
    {
        assertions++;
        if (!ok) throw new Exception("LinkedForkChapterTests failed: " + message);
    }

    public static int Run()
    {
        assertions = 0;

        int[] chapter = LinkedForkChapter.Draw(Length, Seeds);
        Expect(chapter.Length == Length, "draw returns 12 seeds");

        var seen = new HashSet<int>();
        var works = new int[chapter.Length];
        for (int i = 0; i < chapter.Length; i++)
        {
            int seed = chapter[i];
            Expect(seed >= 0 && seed < Seeds, "chapter seed is in range");
            Expect(seen.Add(seed), "chapter seeds are distinct");

            LinkedForkBoard board = LinkedForks.Generate(seed);
            Expect(board != null, "chapter seed has a linked board");
            AxleMomentResult moment = LinkedForks.AxleMoment(board);
            Expect(moment.SolvedWithoutGuess && moment.AxleRestarts && moment.TruthKept,
                "chapter seed satisfies all three axle conditions");

            works[i] = LinkedForkChapter.Work(seed);
            Expect(works[i] == moment.Cells - moment.FixedBeforeStall, "work matches the axle moment");
            if (i > 0) Expect(works[i] >= works[i - 1], "work is non-decreasing along the chapter");
        }

        int[] again = LinkedForkChapter.Draw(Length, Seeds);
        Expect(SameInts(chapter, again), "draw is deterministic");
        Expect(LinkedForkChapter.Draw(0, Seeds).Length == 0, "zero length gives empty");
        Expect(LinkedForkChapter.Draw(Length, 0).Length == 0, "zero seed limit gives empty");

        int qualifying = 0;
        for (int seed = 0; seed < Seeds; seed++)
        {
            LinkedForkBoard board = LinkedForks.Generate(seed);
            if (board == null) continue;
            AxleMomentResult moment = LinkedForks.AxleMoment(board);
            if (moment.SolvedWithoutGuess && moment.AxleRestarts && moment.TruthKept) qualifying++;
        }
        int[] everyone = LinkedForkChapter.Draw(200, Seeds);
        Expect(everyone.Length == qualifying, "a long draw returns only qualifying seeds");

        // The draw is spaced through the ranking, so it must span the whole range of work: the first board is
        // the gentlest qualifying board and the last the firmest, with a real curve between (vision run
        // 20261128-1502: taking the twelve lowest-work seeds instead gave work 4 twelve times, a flat chapter).
        int minWork = int.MaxValue, maxWork = int.MinValue;
        var spread = new SortedDictionary<int, int>();
        foreach (int seed in everyone)
        {
            int w = LinkedForkChapter.Work(seed);
            minWork = Math.Min(minWork, w);
            maxWork = Math.Max(maxWork, w);
            spread[w] = spread.TryGetValue(w, out int c) ? c + 1 : 1;
        }
        var parts = new List<string>();
        foreach (var kv in spread) parts.Add(kv.Key + "x" + kv.Value);
        Console.WriteLine("LinkedForkChapter: seeds " + Join(chapter) + " work " + Join(works) +
            " qualifying " + qualifying + " of " + Seeds + "; work spread " + string.Join(" ", parts));
        Expect(works[0] == minWork, "the chapter opens on the gentlest qualifying board");
        Expect(works[works.Length - 1] == maxWork, "the chapter closes on the firmest qualifying board");
        Expect(new HashSet<int>(works).Count >= 3, "the chapter has at least three steps of work");

        Expect(SameInts(chapter, ExpectedSeeds), "locked chapter seeds");
        Expect(SameInts(works, ExpectedWork), "locked chapter work sequence");
        Expect(qualifying == ExpectedQualifying, "locked qualifying count");
        return assertions;
    }

    private static bool SameInts(int[] a, int[] b)
    {
        if (a.Length != b.Length) return false;
        for (int i = 0; i < a.Length; i++) if (a[i] != b[i]) return false;
        return true;
    }

    private static string Join(int[] values)
    {
        return "[" + string.Join(", ", values) + "]";
    }
}
