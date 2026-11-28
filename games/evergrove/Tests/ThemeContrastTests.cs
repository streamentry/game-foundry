using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;
using Evergrove.Core;

/// <summary>
/// Vision run 20261128-1001, idea 4: the palette proves the contrast its comments claim.
/// SeasonContrastTests already holds body text at 4.5:1 on Paper, CardFill and the action
/// tones in every season. This test covers the pairs it does not: the two pipe channels
/// on every tile state they are drawn on (WCAG 1.4.11 non-text contrast, 3:1), ink on the
/// other panels it sits on (4.5:1), and the protected-landmark frame. Colours are parsed
/// from MosseloraTheme.cs, so the test cannot drift from what the game draws.
/// </summary>
internal static class ThemeContrastTests
{
    private static int assertions;

    private static void Expect(bool ok, string message)
    {
        assertions++;
        if (!ok) throw new Exception("ThemeContrastTests failed: " + message);
    }

    public static int Run()
    {
        assertions = 0;
        Dictionary<string, double> lum = ParseTheme(File.ReadAllText(ThemePath()));

        string[] tiles = { "DryTile", "WetTile", "SolvedTile", "AnchoredTile", "HintRowTile", "HintFocusTile" };
        foreach (string channel in new[] { "DryChannel", "WaterChannel", "WetPipe" })
            foreach (string tile in tiles)
                AtLeast(lum, channel, tile, 3.0);

        string[] panels = { "Paper", "CardFill", "Sand", "SagePanel", "LeafSoft", "RiverWash" };
        foreach (string ink in new[] { "Ink", "SecondaryInk" })
            foreach (string panel in panels)
                AtLeast(lum, ink, panel, 4.5);

        AtLeast(lum, "LandmarkFrame", "AnchoredTile", 4.5);
        AtLeast(lum, "LandmarkFrame", "LandmarkPlate", 4.5);
        AtLeast(lum, "Ink", "AnchoredTile", 4.5);
        return assertions;
    }

    private static void AtLeast(Dictionary<string, double> lum, string fore, string back, double required)
    {
        Expect(lum.ContainsKey(fore) && lum.ContainsKey(back), fore + " and " + back + " are defined in MosseloraTheme.cs");
        double ratio = TurnPalette.Contrast(lum[fore], lum[back]);
        Expect(ratio >= required, fore + " on " + back + " is " + ratio.ToString("0.00", CultureInfo.InvariantCulture) +
            ":1, needs " + required.ToString("0.0", CultureInfo.InvariantCulture) + ":1");
    }

    private static Dictionary<string, double> ParseTheme(string source)
    {
        var lum = new Dictionary<string, double>(StringComparer.Ordinal);
        foreach (Match m in Regex.Matches(source, @"Color (\w+) = Hex\(""#([0-9A-Fa-f]{6})""\)"))
        {
            string hex = m.Groups[2].Value;
            float r = int.Parse(hex.Substring(0, 2), NumberStyles.HexNumber) / 255f;
            float g = int.Parse(hex.Substring(2, 2), NumberStyles.HexNumber) / 255f;
            float b = int.Parse(hex.Substring(4, 2), NumberStyles.HexNumber) / 255f;
            lum[m.Groups[1].Value] = TurnPalette.Luminance(r, g, b);
        }
        Expect(lum.Count >= 20, "theme palette parsed from MosseloraTheme.cs");
        return lum;
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
        throw new Exception("ThemeContrastTests could not locate MosseloraTheme.cs.");
    }
}
