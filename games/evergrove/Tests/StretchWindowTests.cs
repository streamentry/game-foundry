using System;
using System.IO;
using Evergrove.Core;

// Checklist 50: only the stretches on screen plus one either side stay live (StretchWindowRule). The
// rule is pure and tested here; the component's wiring and the memory tour are checked by source audit.
internal static class StretchWindowTests
{
    private static int assertions;

    private static void Expect(bool ok, string message)
    {
        assertions++;
        if (!ok) throw new Exception("StretchWindowTests failed: " + message);
    }

    private const float H = 600f; // one stretch's height, in points

    public static int Run()
    {
        assertions = 0;
        KeepsTheViewAndOneEitherSide();
        ClampsAtBothEnds();
        ShortRiversKeepEverything();
        BadInputKeepsEverything();
        LiveCountIsBoundedForAnyLength();
        DepthToStretch();
        WiredIntoTheMapView();
        TourMeasuresMemoryBothWays();
        RegisteredInTheSuite();
        return assertions;
    }

    private static void KeepsTheViewAndOneEitherSide()
    {
        StretchWindowRule.Range(10f * H + 100f, 10f * H + 500f, H, 34, out int first, out int last);
        Expect(first == 9 && last == 11, "a view inside stretch 10 keeps 9 to 11, got " + first + ".." + last);
        StretchWindowRule.Range(10f * H + 400f, 11f * H + 200f, H, 34, out first, out last);
        Expect(first == 9 && last == 12, "a view across 10 and 11 keeps 9 to 12, got " + first + ".." + last);
        StretchWindowRule.Range(11f * H + 200f, 10f * H + 400f, H, 34, out int f2, out int l2);
        Expect(f2 == first && l2 == last, "top and bottom may come in either order");
        Expect(StretchWindowRule.Keep(9f * H + 1f, H, 34, 9, 11) && !StretchWindowRule.Keep(8f * H + 1f, H, 34, 9, 11) &&
               !StretchWindowRule.Keep(12f * H + 1f, H, 34, 9, 11), "only 9 to 11 are kept");
    }

    private static void ClampsAtBothEnds()
    {
        StretchWindowRule.Range(0f, 300f, H, 34, out int first, out int last);
        Expect(first == 0 && last == 1, "at the top: 0 and 1, got " + first + ".." + last);
        StretchWindowRule.Range(-200f, 100f, H, 34, out first, out last);
        Expect(first == 0, "an elastic pull above the top still starts at 0");
        StretchWindowRule.Range(33f * H + 100f, 34f * H + 200f, H, 34, out first, out last);
        Expect(first == 32 && last == 34, "at the bottom the misty land (index 34) is kept, got " + first + ".." + last);
        Expect(StretchWindowRule.StretchAt(1e9f, H, 34) == 34 && StretchWindowRule.StretchAt(-1e9f, H, 34) == 0, "far depths clamp");
    }

    private static void ShortRiversKeepEverything()
    {
        for (int n = 1; n < StretchWindowRule.MinStretches; n++)
        {
            StretchWindowRule.Range(0f, 100f, H, n, out int first, out int last);
            Expect(first == 0 && last == n, n + " stretches: everything stays live");
        }
    }

    private static void BadInputKeepsEverything()
    {
        StretchWindowRule.Range(0f, 100f, 0f, 34, out int first, out int last);
        Expect(first == 0 && last == 34, "a zero stretch height keeps everything");
        StretchWindowRule.Range(float.NaN, 100f, H, 34, out first, out last);
        Expect(first == 0 && last == 34, "a NaN depth keeps everything");
        Expect(StretchWindowRule.StretchAt(float.NaN, H, 34) == 0 && StretchWindowRule.StretchAt(5f, -1f, 34) == 0, "bad depths read as the top");
    }

    // However long the river, a phone-sized view keeps at most a handful of stretches live.
    private static void LiveCountIsBoundedForAnyLength()
    {
        foreach (int n in new[] { 4, 34, 200, 100000 })
            for (int s = 0; s < Math.Min(n, 60); s++)
            {
                StretchWindowRule.Range(s * H + 50f, s * H + 50f + 0.9f * H, H, n, out int first, out int last);
                Expect(last - first + 1 <= 4, n + " stretches at " + s + ": at most 4 kept, got " + (last - first + 1));
            }
    }

    private static void DepthToStretch()
    {
        Expect(StretchWindowRule.StretchAt(0f, H, 34) == 0 && StretchWindowRule.StretchAt(H - 0.01f, H, 34) == 0 &&
               StretchWindowRule.StretchAt(H, H, 34) == 1, "depths map to stretches at each boundary");
    }

    private static void WiredIntoTheMapView()
    {
        string view = ReadEvergrove("Assets/Scripts/Unity/GardenMapView.cs");
        string window = ReadEvergrove("Assets/Scripts/Unity/StretchWindow.cs");
        string hub = ReadEvergrove("Assets/Scripts/Unity/EvergroveApp.Hub.cs");
        Expect(view.Contains("StretchWindow.Attach(hostRect, landRect, plotRect, reaches);"), "every map view gets a stretch window");
        Expect(window.Contains("internal static bool Enabled = true;"), "the window is on in play");
        Expect(window.Contains("StretchWindowRule.Range(top, bottom, stretchHeight, stretches, out first, out last);") &&
               window.Contains("StretchWindowRule.Keep("), "the component follows the pure rule");
        Expect(window.Contains("if (first == lastFirst && last == lastLast && painting.childCount == lastPaintings"),
            "work is done only when the range or the painting's children change, not every frame");
        Expect(window.Contains("rect.rect.height > stretchHeight * 2f"), "layers spanning the river stay live");
        Expect(hub.Contains("if (qaLongRiver > 0) reaches = qaLongRiver;") && hub.Contains("private int qaLongRiver;"),
            "only the tour lengthens the river; 0 in play");
    }

    private static void TourMeasuresMemoryBothWays()
    {
        string tour = ReadEvergrove("Assets/Scripts/Unity/DevQaTour.Window.cs");
        string main = ReadEvergrove("Assets/Scripts/Unity/DevQaTour.cs");
        Expect(tour.StartsWith("#if UNITY_EDITOR || DEVELOPMENT_BUILD"), "the memory tour is dev-only");
        Expect(tour.Contains("foreach (bool on in new[] { false, true })"),
            "the river is scrolled with the window off, then on");
        Expect(tour.Contains("heapKB") && tour.Contains("unityKB") && tour.Contains("live") && tour.Contains("StretchesScrolled = 30"),
            "each of 30 stretches logs memory and live things");
        Expect(tour.Contains("StretchWindow.Enabled = true;") && tour.Contains("app.QaLongRiver(0);"), "the tour puts play's settings back");
        Expect(main.Contains("yield return WindowShots();"), "the tour runs the memory test");
    }

    private static void RegisteredInTheSuite()
    {
        Expect(ReadEvergrove("Tests/Program.cs").Contains("StretchWindowTests.Run()"), "StretchWindowTests is registered");
    }

    private static string ReadEvergrove(string relative)
    {
        string dir = AppContext.BaseDirectory;
        while (dir != null && !File.Exists(Path.Combine(dir, relative)))
            dir = Path.GetDirectoryName(dir);
        if (dir == null) throw new Exception(relative + " not found");
        return File.ReadAllText(Path.Combine(dir, relative));
    }
}
