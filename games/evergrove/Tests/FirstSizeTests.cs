using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Evergrove.Core;

/// <summary>
/// Checklist 6: text size is offered on first launch, before the first puzzle. The chooser lives
/// in Unity (EvergroveApp.FirstSize.cs), so these checks read its source: the once-flag, the
/// TextScale steps it offers, the hook's place on the first-run path, and the first-minutes audit.
/// </summary>
internal static class FirstSizeTests
{
    private static int assertions;

    private const string OnceFlag = "evergrove:first-size-offered:v1";

    private static void Expect(bool ok, string message)
    {
        assertions++;
        if (!ok) throw new Exception("FirstSizeTests failed: " + message);
    }

    public static int Run()
    {
        string root = EvergroveRoot();
        string unity = Path.Combine(root, "Assets", "Scripts", "Unity");
        string firstSize = File.ReadAllText(Path.Combine(unity, "EvergroveApp.FirstSize.cs"));
        string app = File.ReadAllText(Path.Combine(unity, "EvergroveApp.cs"));
        string textScale = File.ReadAllText(Path.Combine(unity, "TextScale.cs"));

        OnceFlagLivesOnlyInFirstSize(unity);
        ChooserOffersExactlyTextScaleSteps(firstSize, textScale);
        ChoicesAreLargeEnoughToTap(firstSize);
        HookIsOnFirstRunPathBeforeFirstPuzzle(app);
        SettingsKeepsItsControl(app);

        // The first-minutes audit owns the word budget; rerunning it here proves it still passes.
        Expect(FirstMinutesAuditTests.Run() > 0, "FirstMinutesAuditTests must run and count its checks");
        return assertions;
    }

    // The once-flag key is written and read only by EvergroveApp.FirstSize.cs.
    private static void OnceFlagLivesOnlyInFirstSize(string unity)
    {
        foreach (string file in Directory.GetFiles(unity, "*.cs", SearchOption.AllDirectories))
        {
            string text = File.ReadAllText(file);
            bool has = text.Contains(OnceFlag);
            if (Path.GetFileName(file) == "EvergroveApp.FirstSize.cs")
                Expect(has, "once-flag key missing from EvergroveApp.FirstSize.cs");
            else
                Expect(!has, "once-flag key must live only in EvergroveApp.FirstSize.cs: " + Path.GetFileName(file));
        }

        string firstSize = File.ReadAllText(Path.Combine(unity, "EvergroveApp.FirstSize.cs"));
        Expect(firstSize.Contains("FirstSizeOfferedKey"), "once-flag must be named FirstSizeOfferedKey");
        Expect(firstSize.Contains("TextScale.PreferenceKey"),
            "offer must be gated on no stored TextScale preference");
    }

    // The chooser iterates TextScale.Steps and hardcodes no percentages itself.
    private static void ChooserOffersExactlyTextScaleSteps(string firstSize, string textScale)
    {
        Match steps = Regex.Match(textScale, @"Steps\s*=\s*\{\s*([\d,\s]+?)\s*\}");
        Expect(steps.Success, "TextScale.Steps literal not found");
        int[] values = steps.Groups[1].Value.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
            .Select(part => int.Parse(part.Trim())).ToArray();
        Expect(values.SequenceEqual(new[] { 100, 125, 150 }), "TextScale.Steps must be 100, 125, 150");

        Expect(firstSize.Contains("TextScale.Steps"), "chooser must iterate TextScale.Steps");
        Expect(!Regex.IsMatch(firstSize, @"\b(100|125|150)\b"),
            "chooser must not hardcode step percentages; it offers TextScale.Steps");
        Expect(firstSize.Contains("TextScale.Choose(percent)"), "choosing must go through TextScale.Choose");
        Expect(Regex.IsMatch(firstSize, @"ChooseFirstSize\(int percent\)\s*\{[^}]*Rebuild\(\)"),
            "choosing a size must rebuild the garden so the preview is live");
    }

    private static void ChoicesAreLargeEnoughToTap(string firstSize)
    {
        Match height = Regex.Match(firstSize, @"const int ChoiceHeight = (\d+);");
        Expect(height.Success, "ChoiceHeight constant missing");
        Expect(int.Parse(height.Groups[1].Value) >= 56, "each size choice must be at least 56 pt tall");
    }

    // The offer is called from Rebuild after firstPuzzle is decided and before BuildPuzzle.
    private static void HookIsOnFirstRunPathBeforeFirstPuzzle(string app)
    {
        int rebuild = app.IndexOf("private void Rebuild(bool top = false)", StringComparison.Ordinal);
        int decided = app.IndexOf("bool firstPuzzle = world.Round == 0", rebuild, StringComparison.Ordinal);
        int hook = app.IndexOf("OfferFirstSize(firstPuzzle);", StringComparison.Ordinal);
        int build = app.IndexOf("BuildPuzzle();", rebuild, StringComparison.Ordinal);

        Expect(rebuild >= 0 && decided > rebuild, "firstPuzzle must be decided inside Rebuild");
        Expect(hook > decided, "text-size hook must follow the first-run decision");
        Expect(build > hook, "text-size hook must come before the first puzzle is built");
        Expect(app.IndexOf("OfferFirstSize(", hook + 1, StringComparison.Ordinal) < 0,
            "text-size hook must appear exactly once");
    }

    private static void SettingsKeepsItsControl(string app)
    {
        Expect(app.Contains("TextSizeButton(row.transform);"), "Settings must keep its text-size control");
    }

    // Walks up from the working directory and the assembly directory. Returns .../games/evergrove.
    private static string EvergroveRoot()
    {
        string[] starts = { Directory.GetCurrentDirectory(), AppContext.BaseDirectory };
        foreach (string start in starts)
        {
            DirectoryInfo current = new DirectoryInfo(start);
            while (current != null)
            {
                string nested = Path.Combine(current.FullName, "games", "evergrove");
                if (Directory.Exists(Path.Combine(nested, "Assets", "Scripts", "Core"))) return nested;
                if (Directory.Exists(Path.Combine(current.FullName, "Assets", "Scripts", "Core"))) return current.FullName;
                current = current.Parent;
            }
        }
        throw new Exception("Could not find games/evergrove from " + Directory.GetCurrentDirectory());
    }
}
