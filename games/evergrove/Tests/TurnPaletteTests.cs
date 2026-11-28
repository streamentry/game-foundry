using System;
using System.IO;
using Evergrove.Core;

/// <summary>
/// Pins the take-turns palette: every pair of colours keeps a WCAG contrast of at least 1.3
/// (so they differ in greyscale), each colour keeps its hue family, Rgb returns fresh copies
/// and wraps, and TreePlay.cs reads its colours from TurnPalette rather than the old fields.
/// </summary>
internal static class TurnPaletteTests
{
    private static int assertions;

    private static void Expect(bool ok, string message)
    {
        assertions++;
        if (!ok) throw new Exception("TurnPaletteTests failed: " + message);
    }

    public static int Run()
    {
        assertions = 0;

        // Pairwise greyscale separation.
        double[] lum = new double[TurnPalette.Count];
        for (int i = 0; i < TurnPalette.Count; i++)
        {
            float[] c = TurnPalette.Rgb(i);
            lum[i] = TurnPalette.Luminance(c[0], c[1], c[2]);
        }
        for (int i = 0; i < TurnPalette.Count; i++)
        {
            for (int j = i + 1; j < TurnPalette.Count; j++)
            {
                Expect(TurnPalette.Contrast(lum[i], lum[j]) >= 1.3,
                    "colours " + i + " and " + j + " keep contrast 1.3 or more");
            }
        }

        // Hue families: sage green, honey warm, sky blue, rose red.
        float[] sage = TurnPalette.Rgb(0);
        Expect(sage[1] > sage[0] && sage[1] > sage[2], "sage is green-led");
        float[] honey = TurnPalette.Rgb(1);
        Expect(honey[0] >= honey[1] && honey[1] > honey[2], "honey is warm");
        float[] sky = TurnPalette.Rgb(2);
        Expect(sky[2] > sky[0] && sky[2] > sky[1], "sky is blue-led");
        float[] rose = TurnPalette.Rgb(3);
        Expect(rose[0] > rose[1] && rose[0] > rose[2], "rose is red-led");

        // Copies and wrap-around.
        float[] first = TurnPalette.Rgb(0);
        first[0] = 0f;
        Expect(TurnPalette.Rgb(0)[0] == 0.42f, "Rgb returns a copy");
        float[] wrapped = TurnPalette.Rgb(4);
        float[] zero = TurnPalette.Rgb(0);
        Expect(wrapped[0] == zero[0] && wrapped[1] == zero[1] && wrapped[2] == zero[2], "Rgb(4) equals Rgb(0)");
        float[] minusOne = TurnPalette.Rgb(-1);
        float[] three = TurnPalette.Rgb(3);
        Expect(minusOne[0] == three[0] && minusOne[1] == three[1] && minusOne[2] == three[2], "Rgb(-1) equals Rgb(3)");

        // Luminance and contrast.
        Expect(Math.Abs(TurnPalette.Luminance(1f, 1f, 1f) - 1.0) < 1e-9, "white has luminance 1");
        Expect(TurnPalette.Luminance(0f, 0f, 0f) == 0.0, "black has luminance 0");
        Expect(TurnPalette.Contrast(0.2, 0.7) == TurnPalette.Contrast(0.7, 0.2), "contrast is symmetric");

        // Source check: TreePlay uses the palette and not the removed fields.
        string treePlay = Script("Unity/TreePlay.cs");
        Expect(treePlay.Contains("TurnPalette.Rgb("), "TreePlay reads colours from TurnPalette");
        Expect(!treePlay.Contains("TurnSage"), "TreePlay no longer has the old TurnSage field");

        return assertions;
    }

    private static string Script(string relative)
    {
        foreach (string start in new[] { Directory.GetCurrentDirectory(), AppContext.BaseDirectory })
        {
            for (var dir = new DirectoryInfo(start); dir != null; dir = dir.Parent)
            {
                string path = Path.Combine(dir.FullName, "games", "evergrove", "Assets", "Scripts", relative);
                if (File.Exists(path)) return File.ReadAllText(path);
            }
        }
        throw new Exception("TurnPaletteTests: " + relative + " not found from " + Directory.GetCurrentDirectory());
    }
}
