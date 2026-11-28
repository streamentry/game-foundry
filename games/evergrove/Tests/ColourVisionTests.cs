using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;
using Evergrove.Core;

/// <summary>
/// Vision run 20261128-1101: wet and dry pipes must differ for every eye. ThemeContrastTests
/// proves each channel has 3:1 against its tile. This proves the water channel and the dry
/// channel still contrast with each other under protanopia, deuteranopia and tritanopia
/// (Machado 2009, severity 1.0). Colours are parsed from MosseloraTheme.cs.
/// </summary>
internal static class ColourVisionTests
{
    private const double MinChannelDistance = 0.2;
    private static int assertions;

    private static void Expect(bool ok, string message)
    {
        assertions++;
        if (!ok) throw new Exception("ColourVisionTests failed: " + message);
    }

    public static int Run()
    {
        assertions = 0;

        // Matrix rows sum to 1 so white and grey stay neutral under each simulation.
        foreach (ColourVision.Vision v in new[] {
            ColourVision.Vision.Protanopia, ColourVision.Vision.Deuteranopia, ColourVision.Vision.Tritanopia })
        {
            double[] white = ColourVision.Simulate(1, 1, 1, v);
            for (int i = 0; i < 3; i++)
                Expect(Math.Abs(white[i] - 1.0) <= 0.01, v + " keeps white white");
        }

        double[] before = { 0.2, 0.5, 0.8 };
        double[] normal = ColourVision.Simulate(before[0], before[1], before[2], ColourVision.Vision.Normal);
        for (int i = 0; i < 3; i++)
            Expect(Math.Abs(normal[i] - before[i]) <= 1e-9, "Normal simulation returns the input");

        foreach (ColourVision.Vision v in AllVisions())
        {
            double[] grey = ColourVision.Simulate(0.5, 0.5, 0.5, v);
            Expect(Math.Abs(grey[0] - grey[1]) <= 0.02 && Math.Abs(grey[1] - grey[2]) <= 0.02,
                "mid grey stays grey under " + v);
        }

        Dictionary<string, double[]> theme = ParseTheme(File.ReadAllText(ThemePath()));
        double[] water = theme["WaterChannel"];
        double[] dry = theme["DryChannel"];
        // Finding (2026-11-28): WaterChannel #18717F and DryChannel #9C5A38 are near-equal in
        // luminance (WCAG 1.06 normal, 1.18 protan, 1.22 deutan, 1.00 tritan). The board's wet pipe
        // is therefore WetPipe, a darker same-hue teal, checked below at 1.5:1. WaterChannel stays for
        // icons, maps and marks, so only RGB distance is asserted for it here.
        foreach (ColourVision.Vision v in AllVisions())
        {
            double distance = Distance(ColourVision.Simulate(water[0], water[1], water[2], v),
                                       ColourVision.Simulate(dry[0], dry[1], dry[2], v));
            Expect(distance >= MinChannelDistance,
                "water and dry channels stay distinct in RGB under " + v + ", distance " +
                distance.ToString("0.000", CultureInfo.InvariantCulture));
        }

        // The wet pipe on the board is WetPipe, a darker teal than WaterChannel, so its lightness
        // differs from DryChannel under every simulation (1.5:1 bar).
        double[] wet = theme["WetPipe"];
        foreach (ColourVision.Vision v in AllVisions())
        {
            double contrast = ColourVision.ContrastUnder(v, wet, dry);
            Expect(contrast >= 1.5,
                "WetPipe and DryChannel contrast is at least 1.5:1 under " + v + ", was " +
                contrast.ToString("0.000", CultureInfo.InvariantCulture));
        }

        // Vision run 20261128-1801 idea 4: the protected tile is told by its frame for every eye.
        // Finding (2026-11-28, Machado 2009 severity 1.0): LandmarkFrame #4A3524 on AnchoredTile
        // #F1DEB6 measures 8.61:1 at its lowest (deuteranopia; normal 8.69, protan 8.80, tritan 8.67).
        // The AnchoredTile fill stays distinct from the other tile states: lowest distance 0.105
        // (tritanopia, AnchoredTile vs DryTile); the other pairs are 0.148 to 0.252.
        double[] frame = theme["LandmarkFrame"];
        double[] anchored = theme["AnchoredTile"];
        double[] dryTile = theme["DryTile"];
        double[] wetTile = theme["WetTile"];
        double[] solvedTile = theme["SolvedTile"];
        foreach (ColourVision.Vision v in AllVisions())
        {
            double frameContrast = ColourVision.ContrastUnder(v, frame, anchored);
            Expect(frameContrast >= 4.5,
                "LandmarkFrame on AnchoredTile is at least 4.5:1 under " + v + ", was " +
                frameContrast.ToString("0.000", CultureInfo.InvariantCulture));

            double[] simAnchored = ColourVision.Simulate(anchored[0], anchored[1], anchored[2], v);
            foreach (KeyValuePair<string, double[]> other in new[] {
                new KeyValuePair<string, double[]>("DryTile", dryTile),
                new KeyValuePair<string, double[]>("WetTile", wetTile),
                new KeyValuePair<string, double[]>("SolvedTile", solvedTile) })
            {
                double[] simOther = ColourVision.Simulate(other.Value[0], other.Value[1], other.Value[2], v);
                double fillDistance = Distance(simAnchored, simOther);
                Expect(fillDistance >= 0.08,
                    "AnchoredTile and " + other.Key + " stay distinct under " + v + ", distance " +
                    fillDistance.ToString("0.000", CultureInfo.InvariantCulture));
            }
        }
        AssertPipeInksStandOff(theme);
        return assertions;
    }

    // Vision run 20261128-2001 idea 4: every pipe ink stands off the tile it is drawn on, for every eye.
    // PipeGraphic draws the outer channel as DryChannel (dry), WetPipe (wet) or a completed teal literal
    // (solved); TileFill paints the tile beneath it, and TreePlay's hint paints a honey literal.
    // Pairs checked (ink on fill), each under Normal, Protanopia, Deuteranopia and Tritanopia:
    //   DryChannel on DryTile, DryChannel on AnchoredTile, DryChannel on the hint literal,
    //   WetPipe on WetTile, WetPipe on AnchoredTile, WetPipe on the hint literal,
    //   completed teal on SolvedTile, completed teal on AnchoredTile.
    // Measured 2026-11-28, minimum over the four visions (normal, protan, deutan, tritan):
    //   DryChannel/DryTile 4.229 (deutan); DryChannel/AnchoredTile 3.796 (deutan);
    //   DryChannel/hint 4.274 (deutan); WetPipe/WetTile 8.099 (protan);
    //   WetPipe/AnchoredTile 6.898 (protan); WetPipe/hint 7.836 (protan);
    //   completed teal/SolvedTile 3.607 (protan); completed teal/AnchoredTile 2.949 (protan).
    // Finding: the completed teal (then a #168492 literal in PipeGraphic) measured 2.949 on AnchoredTile
    // under protanopia. It is now MosseloraTheme.SolvedPipe, darkened 3%; see the values below.
    // The hint honey (0.98, 0.93, 0.74) is still an inline literal in TreePlay.cs, copied here.
    private static void AssertPipeInksStandOff(Dictionary<string, double[]> theme)
    {
        double[] completedInk = theme["SolvedPipe"];
        double[] hintFill = { 0.98, 0.93, 0.74 };
        var pairs = new List<KeyValuePair<string, double[][]>>
        {
            new KeyValuePair<string, double[][]>("DryChannel on DryTile", new[] { theme["DryChannel"], theme["DryTile"] }),
            new KeyValuePair<string, double[][]>("DryChannel on AnchoredTile", new[] { theme["DryChannel"], theme["AnchoredTile"] }),
            new KeyValuePair<string, double[][]>("DryChannel on hint literal", new[] { theme["DryChannel"], hintFill }),
            new KeyValuePair<string, double[][]>("WetPipe on WetTile", new[] { theme["WetPipe"], theme["WetTile"] }),
            new KeyValuePair<string, double[][]>("WetPipe on AnchoredTile", new[] { theme["WetPipe"], theme["AnchoredTile"] }),
            new KeyValuePair<string, double[][]>("WetPipe on hint literal", new[] { theme["WetPipe"], hintFill }),
            new KeyValuePair<string, double[][]>("completed teal on SolvedTile", new[] { completedInk, theme["SolvedTile"] }),
            new KeyValuePair<string, double[][]>("completed teal on AnchoredTile", new[] { completedInk, theme["AnchoredTile"] }),
        };

        var failures = new List<string>();
        foreach (KeyValuePair<string, double[][]> pair in pairs)
        {
            double lowest = double.MaxValue;
            foreach (ColourVision.Vision v in AllVisions())
            {
                double c = ColourVision.ContrastUnder(v, pair.Value[0], pair.Value[1]);
                lowest = Math.Min(lowest, c);
            }
            if (lowest < 3.0) failures.Add(pair.Key + " " + lowest.ToString("0.000", CultureInfo.InvariantCulture));
        }
        Expect(failures.Count == 0, "pipe ink stands off its tile at 3:1 under every vision: " + string.Join("; ", failures));
    }

    private static double Distance(double[] a, double[] b)
    {
        double dr = a[0] - b[0], dg = a[1] - b[1], db = a[2] - b[2];
        return Math.Sqrt(dr * dr + dg * dg + db * db);
    }

    private static ColourVision.Vision[] AllVisions()
    {
        return new[]
        {
            ColourVision.Vision.Normal,
            ColourVision.Vision.Protanopia,
            ColourVision.Vision.Deuteranopia,
            ColourVision.Vision.Tritanopia,
        };
    }

    private static Dictionary<string, double[]> ParseTheme(string source)
    {
        var colours = new Dictionary<string, double[]>(StringComparer.Ordinal);
        foreach (Match m in Regex.Matches(source, @"Color (\w+) = Hex\(""#([0-9A-Fa-f]{6})""\)"))
        {
            string hex = m.Groups[2].Value;
            colours[m.Groups[1].Value] = new double[]
            {
                int.Parse(hex.Substring(0, 2), NumberStyles.HexNumber) / 255.0,
                int.Parse(hex.Substring(2, 2), NumberStyles.HexNumber) / 255.0,
                int.Parse(hex.Substring(4, 2), NumberStyles.HexNumber) / 255.0,
            };
        }
        Expect(colours.ContainsKey("WaterChannel") && colours.ContainsKey("DryChannel") && colours.ContainsKey("WetPipe") &&
               colours.ContainsKey("LandmarkFrame") && colours.ContainsKey("AnchoredTile") &&
               colours.ContainsKey("DryTile") && colours.ContainsKey("WetTile") && colours.ContainsKey("SolvedTile"),
            "WaterChannel, WetPipe, DryChannel, LandmarkFrame, AnchoredTile, DryTile, WetTile and SolvedTile parsed from MosseloraTheme.cs");
        return colours;
    }

    private static string ThemePath()
    {
        foreach (string start in new[] { Directory.GetCurrentDirectory(), AppContext.BaseDirectory })
        {
            string dir = Path.GetFullPath(start);
            while (dir != null)
            {
                foreach (string rel in new[] { "games/evergrove/Assets/Scripts/Unity", "Assets/Scripts/Unity" })
                {
                    string candidate = Path.Combine(dir, rel, "MosseloraTheme.cs");
                    if (File.Exists(candidate)) return candidate;
                }
                dir = Path.GetDirectoryName(dir);
            }
        }
        throw new Exception("ColourVisionTests could not locate MosseloraTheme.cs.");
    }
}
