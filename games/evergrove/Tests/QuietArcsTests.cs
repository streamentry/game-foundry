using System;
using System.IO;
using Evergrove.Core;

internal static class QuietArcsTests
{
    private static int assertions;

    private static void Expect(bool ok, string message)
    {
        assertions++;
        if (!ok) throw new Exception("QuietArcsTests failed: " + message);
    }

    private static readonly string[] BannedWords = { "lonely", "health", "memory loss", "sorry", "missed" };

    public static int Run()
    {
        int before = assertions;
        QuietRemarksShare();
        QuietRemarksDeterministic();
        ArcStepsMonotonic();
        ArcsEndAndStayFinal();
        ArcLinesPassWording();
        SpeakLessKeyExists();
        return assertions - before;
    }

    // Idea 85: over 10,000 consecutive opportunities, the kept share is 50% plus or minus 2%.
    private static void QuietRemarksShare()
    {
        const int total = 10000;
        int kept = 0;
        for (int n = 0; n < total; n++)
        {
            int day = n / 3 + 1;
            int villager = n % 3;
            int slot = n / 7;
            if (QuietRemarks.Keep(day, villager, slot)) kept++;
        }
        double share = kept / (double)total;
        Expect(share >= 0.48 && share <= 0.52, "speak less keeps about half of 10,000 opportunities: " + share);

        int keptOneVillager = 0;
        for (int slot = 0; slot < total; slot++)
            if (QuietRemarks.Keep(20261127, 1, slot)) keptOneVillager++;
        double single = keptOneVillager / (double)total;
        Expect(single >= 0.48 && single <= 0.52, "one villager's consecutive slots keep about half: " + single);
    }

    private static void QuietRemarksDeterministic()
    {
        for (int n = 0; n < 2000; n++)
        {
            int day = 20261001 + n % 60;
            int villager = n % 3;
            Expect(QuietRemarks.Keep(day, villager, n) == QuietRemarks.Keep(day, villager, n),
                "same opportunity always gives the same answer");
        }
        Expect(QuietRemarks.Keep(5, 1, 9) == QuietRemarks.Keep(5, 1, 9), "fixed opportunity is stable");
    }

    private static readonly int[] Villagers = { 1, 2 };

    private static void ArcStepsMonotonic()
    {
        foreach (int villager in Villagers)
        {
            int count = NeighbourArcs.StepCount(villager);
            Expect(count >= 4 && count <= 6, "arc has 4 to 6 steps");
            Expect(NeighbourArcs.ReachedAt(villager, 0) == 0, "first step starts at zero progress");
            for (int i = 1; i < count; i++)
                Expect(NeighbourArcs.ReachedAt(villager, i) > NeighbourArcs.ReachedAt(villager, i - 1),
                    "arc steps reached in increasing progress");
            int last = -1;
            for (int progress = 0; progress <= 200; progress++)
            {
                int step = NeighbourArcs.StepAt(villager, progress);
                Expect(step >= last, "arc step never goes backwards as progress grows");
                last = step;
            }
        }
        Expect(NeighbourArcs.StepAt(0, 5) == -1, "Mira has no arc");
        Expect(NeighbourArcs.LineAt(0, 5) == null, "no arc line for Mira");
    }

    private static void ArcsEndAndStayFinal()
    {
        foreach (int villager in Villagers)
        {
            int final = NeighbourArcs.StepCount(villager) - 1;
            int reachedAt = NeighbourArcs.ReachedAt(villager, final);
            Expect(reachedAt > 0 && reachedAt < 100, "final step reached at a finite, modest progress");
            Expect(NeighbourArcs.StepAt(villager, reachedAt) == final, "final step reached");
            Expect(NeighbourArcs.StepAt(villager, reachedAt - 1) < final, "final step not reached early");
            for (int progress = reachedAt; progress <= 100000; progress += 97)
                Expect(NeighbourArcs.StepAt(villager, progress) == final, "final step stays after it is reached");
        }
        Expect(NeighbourArcs.LineAt(1, 100000) == "The boat is in the water.", "Orin's boat ends in the water");
        Expect(NeighbourArcs.LineAt(2, 100000) == "The mill is turning again.", "Fen's mill ends turning");
    }

    private static void ArcLinesPassWording()
    {
        foreach (int villager in Villagers)
        {
            int count = NeighbourArcs.StepCount(villager);
            for (int step = 0; step < count; step++)
            {
                string line = NeighbourArcs.LineAt(villager, NeighbourArcs.ReachedAt(villager, step));
                Expect(line != null && line.Length > 0 && line.Length <= 70, "arc line within 70 characters: " + line);
                Expect(line.EndsWith("."), "arc line is a full sentence");
                Expect(line.IndexOf('!') < 0, "arc line has no exclamation marks");
                string lower = line.ToLowerInvariant();
                foreach (string word in BannedWords)
                    Expect(lower.IndexOf(word, StringComparison.Ordinal) < 0, "arc line avoids banned wording: " + word);
            }
        }
    }

    private static void SpeakLessKeyExists()
    {
        string root = FindRepoRoot();
        string quiet = Path.Combine(root, "games", "evergrove", "Assets", "Scripts", "Unity", "EvergroveApp.Quiet.cs");
        string app = Path.Combine(root, "games", "evergrove", "Assets", "Scripts", "Unity", "EvergroveApp.cs");
        string text = (File.Exists(quiet) ? File.ReadAllText(quiet) : "") + (File.Exists(app) ? File.ReadAllText(app) : "");
        Expect(text.Contains("\"evergrove:speakLess\""), "speak less setting key is defined in the Unity client");
    }

    private static string FindRepoRoot()
    {
        string[] starts = { Directory.GetCurrentDirectory(), AppContext.BaseDirectory };
        foreach (string start in starts)
        {
            DirectoryInfo dir = new DirectoryInfo(start);
            while (dir != null)
            {
                if (File.Exists(Path.Combine(dir.FullName, "games", "evergrove", "Assets", "Scripts", "Unity", "EvergroveApp.cs")))
                    return dir.FullName;
                dir = dir.Parent;
            }
        }
        throw new Exception("QuietArcsTests failed: repository root not found");
    }
}
