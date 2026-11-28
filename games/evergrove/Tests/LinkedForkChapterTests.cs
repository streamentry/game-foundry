using System;
using System.Collections.Generic;
using System.Diagnostics;
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
    private const int WideLimit = 240;
    private const int ExpectedWideQualifying = 197;
    private const int ExpectedWideLargestStep = 8;
    private static readonly int[] ExpectedWideSeeds = { 5, 52, 118, 162, 218, 54, 154, 207, 67, 159, 133, 3 };
    private static readonly int[] ExpectedWideWork = { 4, 4, 4, 4, 4, 6, 6, 6, 8, 8, 9, 17 };
    private static readonly int[] ExpectedSeeds = { 5, 15, 24, 35, 46, 2, 11, 37, 20, 47, 4, 3 };
    private static readonly int[] ExpectedWork = { 4, 4, 4, 4, 4, 6, 6, 6, 8, 8, 9, 17 };
    private const int ExpectedByLevelLargestStep = 2;
    private static readonly int[] ExpectedByLevelSeeds = { 5, 7, 0, 19, 4, 128, 70, 55, 51, 213, 111, 3 };
    private static readonly int[] ExpectedByLevelWork = { 4, 4, 6, 8, 9, 10, 11, 12, 13, 14, 15, 17 };
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

        // Idea 1 (vision run 20261128-1601): does a wider seed range fill the firm end? The 60-seed chapter jumps
        // from work 9 to 17 at its last board. A wide draw shows whether qualifying boards exist between them.
        var wideWatch = Stopwatch.StartNew();
        int[] wide = LinkedForkChapter.Draw(Length, WideLimit);
        int[] wideRanked = LinkedForkChapter.Draw(int.MaxValue, WideLimit);
        wideWatch.Stop();
        var wideWork = new int[wide.Length];
        var wideSeen = new HashSet<int>();
        for (int i = 0; i < wide.Length; i++)
        {
            Expect(wide[i] >= 0 && wide[i] < WideLimit, "wide chapter seed is in range");
            Expect(wideSeen.Add(wide[i]), "wide chapter seeds are distinct");
            wideWork[i] = LinkedForkChapter.Work(wide[i]);
            if (i > 0) Expect(wideWork[i] >= wideWork[i - 1], "wide work is non-decreasing along the chapter");
        }

        int wideMin = int.MaxValue, wideMax = int.MinValue;
        var wideSpread = new SortedDictionary<int, int>();
        foreach (int seed in wideRanked)
        {
            int w = LinkedForkChapter.Work(seed);
            wideMin = Math.Min(wideMin, w);
            wideMax = Math.Max(wideMax, w);
            wideSpread[w] = wideSpread.TryGetValue(w, out int c) ? c + 1 : 1;
        }
        var wideParts = new List<string>();
        foreach (var kv in wideSpread) wideParts.Add(kv.Key + "x" + kv.Value);
        int wideStep = LinkedForkChapter.LargestStep(wide);
        string verdict = wideStep <= 4 ? "filled" : "still a cliff";
        Console.WriteLine("LinkedForkChapter wide: limit " + WideLimit + " seeds " + Join(wide) + " work " + Join(wideWork) +
            " qualifying " + wideRanked.Length + " of " + WideLimit + "; spread " + string.Join(" ", wideParts) +
            "; largest step " + wideStep + " (" + verdict + ")");
        Console.WriteLine("LinkedForkChapter wide: two draws took " + wideWatch.ElapsedMilliseconds + " ms");
        Expect(wideWork[0] == wideMin, "the wide chapter opens on the gentlest qualifying board");
        Expect(wideWork[wideWork.Length - 1] == wideMax, "the wide chapter closes on the firmest qualifying board");

        Expect(LinkedForkChapter.LargestStep(null) == 0, "largest step of null is 0");
        Expect(LinkedForkChapter.LargestStep(new[] { 5 }) == 0, "largest step of one seed is 0");
        Expect(LinkedForkChapter.LargestStep(new[] { 3, 5 }) == 0, "a decreasing pair (work 17 to 4) has no step");
        Expect(LinkedForkChapter.LargestStep(new[] { 5, 3 }) == 13, "an increasing pair (work 4 to 17) steps by 13");
        Expect(LinkedForkChapter.LargestStep(chapter) == 8, "largest step of the 60-seed chapter is 8");
        Expect(wideRanked.Length == ExpectedWideQualifying, "locked wide qualifying count");
        Expect(SameInts(wide, ExpectedWideSeeds), "locked wide chapter seeds");
        Expect(SameInts(wideWork, ExpectedWideWork), "locked wide chapter work sequence");
        Expect(wideStep == ExpectedWideLargestStep, "locked wide largest step (the 9 to 17 cliff is still there)");

        // Drawn by work level (vision run 20261128-1701): the 240-seed range has 11 work levels, so the 12-board draw
        // takes every level once and then one extra board from the gentlest level, which closes the 9 to 17 cliff.
        int[] byLevel = LinkedForkChapter.DrawByLevel(Length, WideLimit);
        Expect(byLevel.Length == Length, "by-level draw returns 12 seeds");
        var byLevelWork = new int[byLevel.Length];
        var byLevelSeen = new HashSet<int>();
        for (int i = 0; i < byLevel.Length; i++)
        {
            Expect(byLevel[i] >= 0 && byLevel[i] < WideLimit, "by-level seed is in range");
            Expect(byLevelSeen.Add(byLevel[i]), "by-level seeds are distinct");
            byLevelWork[i] = LinkedForkChapter.Work(byLevel[i]);
            if (i > 0) Expect(byLevelWork[i] >= byLevelWork[i - 1], "by-level work is non-decreasing along the chapter");
        }
        int byLevelStep = LinkedForkChapter.LargestStep(byLevel);
        Console.WriteLine("LinkedForkChapter by level: limit " + WideLimit + " seeds " + Join(byLevel) + " work " + Join(byLevelWork) +
            "; largest step " + byLevelStep);
        Expect(byLevelWork[0] == 4, "the by-level chapter opens on the gentlest level");
        Expect(byLevelWork[byLevelWork.Length - 1] == 17, "the by-level chapter closes on the firmest level");
        Expect(byLevelStep <= 2, "the by-level chapter has no step larger than 2");
        Expect(LinkedForkChapter.DrawByLevel(0, WideLimit).Length == 0, "by-level zero length gives empty");
        Expect(LinkedForkChapter.DrawByLevel(Length, 0).Length == 0, "by-level zero seed limit gives empty");

        int[] threeLevels = LinkedForkChapter.DrawByLevel(3, WideLimit);
        Expect(threeLevels.Length == 3, "by-level draw of three returns three seeds");
        int threeFirst = LinkedForkChapter.Work(threeLevels[0]);
        int threeMiddle = LinkedForkChapter.Work(threeLevels[1]);
        int threeLast = LinkedForkChapter.Work(threeLevels[2]);
        Expect(threeFirst == 4, "three-board by-level draw opens on work 4");
        Expect(threeMiddle > 4 && threeMiddle < 17, "three-board by-level draw has a middle level");
        Expect(threeLast == 17, "three-board by-level draw closes on work 17");

        Expect(SameInts(byLevel, ExpectedByLevelSeeds), "locked by-level chapter seeds");
        Expect(SameInts(byLevelWork, ExpectedByLevelWork), "locked by-level chapter work sequence");
        Expect(byLevelStep == ExpectedByLevelLargestStep, "locked by-level largest step");
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
