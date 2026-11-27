using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

/// <summary>
/// Checklist item 40, "Mistakes shown kindly": conflicts show only when the player asks
/// (a lens token, device setting "evergrove:showClashes", default off), and never in red.
///
/// These are text checks on the Unity sources, so they run without Unity. The sources are
/// found by walking up from the current directory and from the test binary's folder, looking
/// for Assets/Scripts/Unity. A missing folder fails the test rather than passing silently.
///
/// Rules:
/// 1. Every colour literal (new Color, new Color32, Hex("#...")) within three lines of a
///    conflict word (clash, conflict, error, wrong, mistake) is not red-like. Red-like means
///    either the strict error red (r >= 0.6, g <= 0.4, b <= 0.4) or any pink or red hue (about
///    340 to 20 degrees, saturation >= 0.12, brightness >= 0.5). The second rule catches pale
///    pinks such as the old conflict tile, which the first rule alone would let through.
/// 2. No Color.red in the board, tree, kit or theme sources.
/// 3. The setting key lives in BoardKit.cs, and every file that paints conflicts
///    (game.Conflicts or BoardKit.Clash) is gated on BoardKit.ShowClashes, uses the ring
///    cue (BoardKit.MarkClash) and places the lens (BoardKit.ClashLens). At least three
///    boards must be gated, so the check cannot pass vacuously.
/// </summary>
internal static class KindMistakesTests
{
    private static int assertions;

    private static readonly Regex ConflictWord = new Regex("(?i)clash|conflict|error|wrong|mistake");
    private static readonly Regex FloatColor = new Regex(
        @"new\s+Color\(\s*(-?[0-9]*\.?[0-9]+)f?\s*,\s*(-?[0-9]*\.?[0-9]+)f?\s*,\s*(-?[0-9]*\.?[0-9]+)f?");
    private static readonly Regex ByteColor = new Regex(@"new\s+Color32\(\s*(\d+)\s*,\s*(\d+)\s*,\s*(\d+)");
    private static readonly Regex HexColor = new Regex(@"Hex\(\s*""#([0-9A-Fa-f]{6})""\s*\)");
    private static readonly Regex ColorRed = new Regex(@"\bColor\.red\b");

    private static void Expect(bool ok, string message)
    {
        assertions++;
        if (!ok) throw new Exception("KindMistakesTests failed: " + message);
    }

    public static int Run()
    {
        string unity = FindUnityDir();
        Expect(unity != null, "Assets/Scripts/Unity found from the working or binary directory");
        var files = Directory.GetFiles(unity, "*.cs").OrderBy(f => f, StringComparer.Ordinal).ToArray();
        Expect(files.Length > 0, "Unity sources present");

        // Rule 1: no red-like colour literal in a conflict context.
        foreach (string file in files)
        {
            string[] lines = File.ReadAllLines(file);
            for (int i = 0; i < lines.Length; i++)
            {
                var colours = ParseColours(lines[i]);
                if (colours.Count == 0 || !ConflictNear(lines, i)) continue;
                foreach (var c in colours)
                    Expect(!IsRedLike(c[0], c[1], c[2]),
                        Path.GetFileName(file) + ":" + (i + 1) + " conflict colour is red-like: " + lines[i].Trim());
            }
        }

        // The shared conflict tile itself, explicitly (it is also covered by rule 1).
        string kit = File.ReadAllText(Path.Combine(unity, "BoardKit.cs"));
        string clashLine = File.ReadAllLines(Path.Combine(unity, "BoardKit.cs"))
            .FirstOrDefault(l => l.Contains("Color Clash ="));
        Expect(clashLine != null, "BoardKit.Clash declared");
        var clash = ParseColours(clashLine);
        Expect(clash.Count == 1 && !IsRedLike(clash[0][0], clash[0][1], clash[0][2]),
            "BoardKit.Clash is a calm non-red tile");

        // Rule 2: Color.red never appears in the board, tree, kit or theme sources.
        foreach (string file in files)
        {
            string name = Path.GetFileName(file);
            if (!(name.Contains("Board") || name == "TreePlay.cs" || name == "BoardKit.cs" || name == "MosseloraTheme.cs"))
                continue;
            Expect(!ColorRed.IsMatch(File.ReadAllText(file)), name + " uses no Color.red");
        }

        // Rule 3: the setting, and the gating of every conflict painter.
        Expect(kit.Contains("\"evergrove:showClashes\""), "setting key evergrove:showClashes in BoardKit.cs");
        Expect(kit.Contains("PlayerPrefs.Save()"), "setting persists immediately");
        int gated = 0;
        foreach (string file in files)
        {
            string text = File.ReadAllText(file);
            if (!(text.Contains("Conflicts(") || text.Contains("BoardKit.Clash"))) continue;
            string name = Path.GetFileName(file);
            if (name == "BoardKit.cs") continue;
            Expect(text.Contains("BoardKit.ShowClashes"), name + " paints conflicts only when ShowClashes is on");
            Expect(text.Contains("BoardKit.MarkClash"), name + " marks conflicts with the ring, not colour alone");
            Expect(text.Contains("BoardKit.ClashLens"), name + " offers the lens token");
            gated++;
        }
        Expect(gated >= 3, "at least three boards gated (found " + gated + ")");

        return assertions;
    }

    /// <summary>Walks up from each start folder looking for the Unity sources; null when absent.</summary>
    private static string FindUnityDir()
    {
        foreach (string start in new[] { Directory.GetCurrentDirectory(), AppContext.BaseDirectory })
        {
            string dir = Path.GetFullPath(start);
            while (dir != null)
            {
                string[] candidates =
                {
                    Path.Combine(dir, "games", "evergrove", "Assets", "Scripts", "Unity"),
                    Path.Combine(dir, "Assets", "Scripts", "Unity"),
                };
                foreach (string candidate in candidates)
                    if (Directory.Exists(candidate)) return candidate;
                dir = Path.GetDirectoryName(dir);
            }
        }
        return null;
    }

    private static bool ConflictNear(string[] lines, int index)
    {
        int from = Math.Max(0, index - 3);
        int to = Math.Min(lines.Length - 1, index + 3);
        for (int j = from; j <= to; j++)
            if (ConflictWord.IsMatch(lines[j])) return true;
        return false;
    }

    /// <summary>Colour literals on one line as RGB in 0 to 1.</summary>
    private static List<float[]> ParseColours(string line)
    {
        var result = new List<float[]>();
        foreach (Match m in FloatColor.Matches(line))
            result.Add(new[]
            {
                Parse(m.Groups[1].Value), Parse(m.Groups[2].Value), Parse(m.Groups[3].Value),
            });
        foreach (Match m in ByteColor.Matches(line))
            result.Add(new[]
            {
                Parse(m.Groups[1].Value) / 255f, Parse(m.Groups[2].Value) / 255f, Parse(m.Groups[3].Value) / 255f,
            });
        foreach (Match m in HexColor.Matches(line))
        {
            string hex = m.Groups[1].Value;
            result.Add(new[]
            {
                Convert.ToInt32(hex.Substring(0, 2), 16) / 255f,
                Convert.ToInt32(hex.Substring(2, 2), 16) / 255f,
                Convert.ToInt32(hex.Substring(4, 2), 16) / 255f,
            });
        }
        return result;
    }

    private static float Parse(string text)
    {
        return float.Parse(text, NumberStyles.Float, CultureInfo.InvariantCulture);
    }

    /// <summary>Red-like by either rule in the class comment.</summary>
    internal static bool IsRedLike(float r, float g, float b)
    {
        if (r >= 0.6f && g <= 0.4f && b <= 0.4f) return true;
        float max = Math.Max(r, Math.Max(g, b));
        float min = Math.Min(r, Math.Min(g, b));
        float delta = max - min;
        if (max < 0.5f || delta <= 0f) return false;
        if (delta / max < 0.12f) return false;
        float hue;
        if (max == r) hue = 60f * (((g - b) / delta) % 6f);
        else if (max == g) hue = 60f * ((b - r) / delta + 2f);
        else hue = 60f * ((r - g) / delta + 4f);
        if (hue < 0f) hue += 360f;
        return hue <= 20f || hue >= 340f;
    }
}
