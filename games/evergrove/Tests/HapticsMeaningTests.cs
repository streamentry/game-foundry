using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Evergrove.Core;

/// <summary>
/// Checklist 29: haptics with meaning. Three cues only (Placed, Finished, Arrived),
/// every call site uses one of them, each cue checks the single haptics switch
/// before any native call, and the switch is written by a settings toggle.
/// Source-level, because Unity cannot run in this test project.
/// </summary>
internal static class HapticsMeaningTests
{
    private static int assertions;
    private static readonly string[] Cues = { "Placed", "Finished", "Arrived" };

    private static void Expect(bool ok, string message)
    {
        assertions++;
        if (!ok) throw new Exception("HapticsMeaningTests failed: " + message);
    }

    // games/evergrove, found by walking up from the working directory or the test binary.
    private static string GameRoot()
    {
        foreach (string start in new[] { Directory.GetCurrentDirectory(), AppContext.BaseDirectory })
        {
            for (DirectoryInfo dir = new DirectoryInfo(start); dir != null; dir = dir.Parent)
            {
                string direct = Path.Combine(dir.FullName, "Assets", "Scripts", "Unity", "Haptics.cs");
                if (File.Exists(direct)) return dir.FullName;
                string nested = Path.Combine(dir.FullName, "games", "evergrove");
                if (File.Exists(Path.Combine(nested, "Assets", "Scripts", "Unity", "Haptics.cs"))) return nested;
            }
        }
        throw new Exception("HapticsMeaningTests failed: cannot locate games/evergrove from " + Directory.GetCurrentDirectory());
    }

    // The text of one public static method body, from its opening brace to the matching close.
    private static string Body(string source, string signature)
    {
        int head = source.IndexOf(signature, StringComparison.Ordinal);
        Expect(head >= 0, "signature present: " + signature);
        int open = source.IndexOf('{', head);
        int depth = 0;
        for (int i = open; i < source.Length; i++)
        {
            if (source[i] == '{') depth++;
            else if (source[i] == '}' && --depth == 0) return source.Substring(open + 1, i - open - 1);
        }
        throw new Exception("HapticsMeaningTests failed: unclosed body for " + signature);
    }

    public static int Run()
    {
        string game = GameRoot();
        string haptics = File.ReadAllText(Path.Combine(game, "Assets", "Scripts", "Unity", "Haptics.cs"));
        string app = UnityAppSource.ReadAll();
        string[] unitySources = Directory.GetFiles(Path.Combine(game, "Assets"), "*.cs", SearchOption.AllDirectories);

        // 1. Exactly three public cue methods (the switch is a property, not a method).
        var publicMethods = Regex.Matches(haptics, @"public\s+static\s+\w+\s+(\w+)\s*\(")
            .Cast<Match>().Select(m => m.Groups[1].Value).OrderBy(n => n).ToArray();
        Expect(publicMethods.SequenceEqual(Cues.OrderBy(n => n)),
            "Haptics exposes exactly Placed, Finished, Arrived; found " + string.Join(", ", publicMethods));
        Expect(Regex.IsMatch(haptics, @"public\s+static\s+bool\s+Enabled\b"), "Haptics exposes the Enabled switch");

        // 2. Each cue returns before any native call when the switch is off.
        foreach (string cue in Cues)
        {
            string body = Body(haptics, "public static void " + cue + "()");
            string trimmed = body.TrimStart();
            Expect(Regex.IsMatch(trimmed, @"^if \(!Enabled\b[^)]*\)\s*return;"),
                cue + " starts with the switch check and returns early");
            int check = body.IndexOf("if (!Enabled", StringComparison.Ordinal);
            int native = body.IndexOf("_Mosselora", StringComparison.Ordinal);
            int perform = body.IndexOf("Perform(", StringComparison.Ordinal);
            Expect(native < 0 || check < native, cue + ": switch precedes native calls");
            Expect(perform < 0 || check < perform, cue + ": switch precedes Perform");
        }

        // 3. Every call site outside Haptics.cs uses only the three cues.
        var counts = Cues.ToDictionary(c => c, c => 0);
        int sites = 0;
        foreach (string file in unitySources)
        {
            if (Path.GetFileName(file) == "Haptics.cs") continue;
            string text = File.ReadAllText(file);
            foreach (Match m in Regex.Matches(text, @"\bHaptics\.(\w+)\b"))
            {
                string member = m.Groups[1].Value;
                if (member == "Enabled") continue; // the settings toggle
                int after = m.Index + m.Length;
                bool call = after < text.Length && text[after] == '(';
                Expect(call && counts.ContainsKey(member),
                    Path.GetFileName(file) + " calls only Haptics.Placed/Finished/Arrived, found Haptics." + member);
                counts[member]++;
                sites++;
            }
        }
        Expect(sites > 0, "at least one cue call site exists");

        // 4. The switch key is written by the settings toggle.
        Expect(haptics.Contains("SwitchKey = \"evergrove:haptics\""), "switch key is evergrove:haptics");
        Expect(Regex.IsMatch(haptics, @"PlayerPrefs\.SetInt\(SwitchKey"), "Haptics.Enabled writes the switch key");
        Expect(Regex.IsMatch(app, @"Haptics\.Enabled\s*=\s*!Haptics\.Enabled"), "the settings toggle flips the haptics switch");
        Expect(app.Contains("\"Haptics setting\""), "the haptics toggle sits in the settings row");

        Console.WriteLine("Haptics: 3 cues, " + sites + " call sites (" +
            string.Join(", ", Cues.Select(c => c + " " + counts[c])) + ")");
        return assertions;
    }
}
