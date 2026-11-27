using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

// Checklist 20: no game source file over 800 lines, so navigation changes stop colliding in one monolith.
// Files in Allowed were not split in this batch; each records its line count when it was allowed, and
// may only shrink. EvergroveApp*.cs is never allowed.
internal static class FileSizeTests
{
    private const int MaxLines = 800;

    // Paths relative to Assets/Scripts, with their line counts when allowed.
    private static readonly Dictionary<string, int> Allowed = new Dictionary<string, int>
    {
        // Not split in batch 21 (checklist 20); split in a later batch.
        ["Unity/GardenMapView.cs"] = 2363,
        ["Unity/DevQaTour.cs"] = 1177,
        ["Unity/TreePlay.cs"] = 1100,
        ["Unity/MosseloraArt.cs"] = 1036,
        ["Core/WorldModel.cs"] = 994,
    };

    private static int assertions;

    private static void Expect(bool ok, string message)
    {
        assertions++;
        if (!ok) throw new Exception("FileSizeTests failed: " + message);
    }

    public static int Run()
    {
        assertions = 0;
        string scripts = ScriptsDir();
        Expect(!Allowed.Keys.Any(k => Path.GetFileName(k).StartsWith("EvergroveApp", StringComparison.Ordinal)),
            "EvergroveApp*.cs files must not be allowed over 800 lines");

        var sizes = new Dictionary<string, int>();
        foreach (string file in Directory.GetFiles(scripts, "*.cs", SearchOption.AllDirectories))
        {
            string relative = Path.GetRelativePath(scripts, file).Replace('\\', '/');
            sizes[relative] = File.ReadAllText(file).Count(c => c == '\n');
        }

        foreach (var entry in sizes)
        {
            if (entry.Value <= MaxLines) continue;
            Expect(Allowed.ContainsKey(entry.Key), entry.Key + " is " + entry.Value + " lines; the limit is " + MaxLines);
        }

        foreach (var allowed in Allowed)
        {
            int lines;
            Expect(sizes.TryGetValue(allowed.Key, out lines), "allowlisted file is missing: " + allowed.Key);
            Expect(lines > MaxLines, "allowlist entry is stale, now within the limit: " + allowed.Key);
            Expect(lines <= allowed.Value, allowed.Key + " grew from " + allowed.Value + " to " + lines + " lines");
        }

        Console.WriteLine("File sizes: " + sizes.Count + " source files checked, " + Allowed.Count + " allowed over " + MaxLines);
        return assertions;
    }

    private static string ScriptsDir()
    {
        foreach (string start in new[] { Directory.GetCurrentDirectory(), AppContext.BaseDirectory })
        {
            for (var dir = new DirectoryInfo(start); dir != null; dir = dir.Parent)
            {
                string nested = Path.Combine(dir.FullName, "games", "evergrove", "Assets", "Scripts");
                if (Directory.Exists(nested)) return nested;
                string direct = Path.Combine(dir.FullName, "Assets", "Scripts");
                if (Directory.Exists(direct)) return direct;
            }
        }
        throw new Exception("FileSizeTests: Assets/Scripts not found from " + Directory.GetCurrentDirectory());
    }
}
