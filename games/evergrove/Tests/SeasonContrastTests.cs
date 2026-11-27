using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;
using Evergrove.Core;

/// <summary>
/// I338–I341 (run 3), item 26: seasonal palette grading. One whole-scene grade per season, and
/// every text/background pair stays at WCAG 4.5:1 or better in every season, checked on
/// luminance (the grey level), so it holds for colour-blind and greyscale play too.
/// Palette colours and the painted-text literals are parsed from the Unity sources, so this
/// test cannot drift from what the game draws.
/// </summary>
internal static class SeasonContrastTests
{
    private const double Required = 4.5;
    private static int assertions;

    private static void Expect(bool ok, string message)
    {
        assertions++;
        if (!ok) throw new Exception("Season contrast test failed: " + message);
    }

    private sealed class Pair
    {
        public string Name = "";
        public GradeRgb Text;
        public float TextAlpha = 1f;
        public GradeRgb Background;
        /// <summary>True when the season grade lies under this text (the painting), false for UI and sprites.</summary>
        public bool Graded;
    }

    public static int Run()
    {
        string unity = FindRepoFile("games/evergrove/Assets/Scripts/Unity");
        string themeSource = File.ReadAllText(Path.Combine(unity, "MosseloraTheme.cs"));
        string mapSource = File.ReadAllText(Path.Combine(unity, "GardenMapView.cs"));
        Dictionary<string, GradeRgb> palette = ParsePalette(themeSource);

        // Literals the map draws text with, read from the source so they cannot drift.
        Match label = Regex.Match(mapSource, @"label\.color = MosseloraTheme\.(\w+);");
        Expect(label.Success, "river-name sign label colour found in GardenMapView.cs");
        Match beyond = Regex.Match(mapSource,
            @"beyond\.color = new Color\(([\d.]+)f, ([\d.]+)f, ([\d.]+)f, ([\d.]+)f\);");
        Expect(beyond.Success, "beyond-the-mist text colour found in GardenMapView.cs");

        // Painted plank under the river-name sign: median of the centre of stream-sign.png (sRGB 245,209,159).
        GradeRgb plank = new GradeRgb(245 / 255f, 209 / 255f, 159 / 255f);

        var pairs = new List<Pair>
        {
            MakePair("Ink on Paper (title, letters)", Get(palette, "Ink"), 1f, Get(palette, "Paper"), false),
            MakePair("Ink on CardFill (toasts)", Get(palette, "Ink"), 1f, Get(palette, "CardFill"), false),
            MakePair("SecondaryInk on Paper (secondary text)", Get(palette, "SecondaryInk"), 1f, Get(palette, "Paper"), false),
            MakePair("EarthInk on Paper (save warning)", Get(palette, "EarthInk"), 1f, Get(palette, "Paper"), false),
            MakePair("White on ForestAction (caption)", White, 1f, Get(palette, "ForestAction"), false),
            MakePair("White on WaterAction (caption)", White, 1f, Get(palette, "WaterAction"), false),
            MakePair("White on EarthAction (caption)", White, 1f, Get(palette, "EarthAction"), false),
            MakePair("Ink on QuietAction (quiet button)", Get(palette, "Ink"), 1f, Get(palette, "QuietAction"), false),
            MakePair("River-name sign label on plank", Get(palette, label.Groups[1].Value), 1f, plank, false),
            MakePair("Beyond-the-mist text on graded painting", ParseRgb(beyond), ParseAlpha(beyond), Get(palette, "Paper"), true),
        };

        var seasons = new[] { Season.Spring, Season.Summer, Season.Autumn, Season.Winter };
        double min = double.MaxValue;
        string minWhere = "";
        foreach (Season season in seasons)
        {
            foreach (Pair pair in pairs)
            {
                GradeRgb background = pair.Graded ? SeasonGrade.Apply(pair.Background, season) : pair.Background;
                GradeRgb text = SeasonGrade.Over(pair.Text, pair.TextAlpha, background);
                double ratio = SeasonGrade.Contrast(text, background);
                Expect(ratio >= Required,
                    $"{pair.Name} is {ratio:0.00}:1 in {season}, needs {Required}:1");
                if (ratio < min) { min = ratio; minWhere = $"{season}, {pair.Name}"; }
            }
        }
        Console.WriteLine(string.Format(CultureInfo.InvariantCulture, "Season contrast: min {0:0.00}:1 ({1}); {2} pairs x {3} seasons.", min, minWhere, pairs.Count, seasons.Length));

        // One table per season, keyed by season only: the grade is a single overlay per season.
        var tints = new HashSet<string>();
        foreach (Season season in seasons)
        {
            SeasonGrade.Grade grade = SeasonGrade.For(season);
            Expect(grade.Strength > 0f && grade.Strength <= 0.15f, $"{season} grade is a light overlay");
            tints.Add($"{grade.Tint.R:0.00},{grade.Tint.G:0.00},{grade.Tint.B:0.00}");
            GradeRgb once = SeasonGrade.Apply(Get(palette, "Paper"), season);
            GradeRgb again = SeasonGrade.Apply(Get(palette, "Paper"), season);
            Expect(once.R == again.R && once.G == again.G && once.B == again.B, $"{season} grade is deterministic");
        }
        Expect(tints.Count == seasons.Length, "each season has its own grade table");

        // The palette literals this test relies on still exist in MosseloraTheme.cs.
        foreach (string name in new[] { "Paper", "Ink", "SecondaryInk", "EarthInk", "CardFill", "ForestAction", "WaterAction", "EarthAction", "QuietAction" })
            Expect(palette.ContainsKey(name), $"palette colour {name} parsed from MosseloraTheme.cs");

        return assertions;
    }

    private static readonly GradeRgb White = new GradeRgb(1f, 1f, 1f);

    private static Pair MakePair(string name, GradeRgb text, float alpha, GradeRgb background, bool graded)
    {
        return new Pair { Name = name, Text = text, TextAlpha = alpha, Background = background, Graded = graded };
    }

    private static GradeRgb Get(Dictionary<string, GradeRgb> palette, string name)
    {
        Expect(palette.ContainsKey(name), $"palette colour {name} exists");
        return palette[name];
    }

    private static GradeRgb ParseRgb(Match m)
    {
        return new GradeRgb(float.Parse(m.Groups[1].Value, CultureInfo.InvariantCulture),
            float.Parse(m.Groups[2].Value, CultureInfo.InvariantCulture),
            float.Parse(m.Groups[3].Value, CultureInfo.InvariantCulture));
    }

    private static float ParseAlpha(Match m)
    {
        return float.Parse(m.Groups[4].Value, CultureInfo.InvariantCulture);
    }

    /// <summary>Reads every `public static readonly Color Name = Hex("#RRGGBB");` in MosseloraTheme.cs.</summary>
    private static Dictionary<string, GradeRgb> ParsePalette(string source)
    {
        var palette = new Dictionary<string, GradeRgb>();
        foreach (Match m in Regex.Matches(source, @"public static readonly Color (\w+) = Hex\(""#([0-9A-Fa-f]{6})""\)"))
        {
            string hex = m.Groups[2].Value;
            palette[m.Groups[1].Value] = new GradeRgb(
                Convert.ToInt32(hex.Substring(0, 2), 16) / 255f,
                Convert.ToInt32(hex.Substring(2, 2), 16) / 255f,
                Convert.ToInt32(hex.Substring(4, 2), 16) / 255f);
        }
        Expect(palette.Count >= 9, "theme palette parsed from MosseloraTheme.cs");
        return palette;
    }

    /// <summary>Finds a folder by walking up from the working directory and the test binary.</summary>
    private static string FindRepoFile(string relativeFolder)
    {
        foreach (string start in new[] { Directory.GetCurrentDirectory(), AppContext.BaseDirectory })
        {
            string dir = Path.GetFullPath(start);
            while (dir != null)
            {
                string candidate = Path.Combine(dir, relativeFolder);
                if (File.Exists(Path.Combine(candidate, "MosseloraTheme.cs")))
                    return candidate;
                string inner = Path.Combine(dir, relativeFolder.Substring("games/evergrove/".Length));
                if (relativeFolder.StartsWith("games/evergrove/") && File.Exists(Path.Combine(inner, "MosseloraTheme.cs")))
                    return inner;
                dir = Path.GetDirectoryName(dir);
            }
        }
        throw new Exception("Season contrast test could not locate " + relativeFolder + " from the working or binary directory.");
    }
}
