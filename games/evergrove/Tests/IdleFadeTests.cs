using System;
using System.IO;

/// <summary>
/// Checklist 18: the corner chrome fades to 20% after 6 s with no touch. Pins the pure fade curve
/// (IdleFade.AlphaAt) and checks that the Unity partial only changes CanvasGroup alpha, never active
/// state or raycasts, so a faded control still takes the first tap.
/// </summary>
internal static class IdleFadeTests
{
    private static int assertions;

    private static void Expect(bool ok, string message)
    {
        assertions++;
        if (!ok) throw new Exception("IdleFadeTests failed: " + message);
    }

    private static void Near(float actual, float expected, string message)
    {
        Expect(Math.Abs(actual - expected) < 0.0005f, message + " expected " + expected + " got " + actual);
    }

    public static int Run()
    {
        assertions = 0;

        Near(Evergrove.Core.IdleFade.AlphaAt(0f, false), 1f, "0 s is full");
        Near(Evergrove.Core.IdleFade.AlphaAt(5.9f, false), 1f, "5.9 s is full");
        Near(Evergrove.Core.IdleFade.AlphaAt(6.0f, false), 1f, "6.0 s starts at full");
        Near(Evergrove.Core.IdleFade.AlphaAt(6.4f, false), 0.6f, "6.4 s is halfway (smoothstep)");
        Near(Evergrove.Core.IdleFade.AlphaAt(6.8f, false), 0.2f, "6.8 s is faded");
        Near(Evergrove.Core.IdleFade.AlphaAt(30f, false), 0.2f, "30 s stays faded");

        Near(Evergrove.Core.IdleFade.AlphaAt(5.9f, true), 1f, "reduced motion 5.9 s is full");
        Near(Evergrove.Core.IdleFade.AlphaAt(6.0f, true), 0.2f, "reduced motion jumps at 6 s");
        Near(Evergrove.Core.IdleFade.AlphaAt(6.01f, true), 0.2f, "reduced motion stays at 0.2");

        Expect(!float.IsNaN(Evergrove.Core.IdleFade.AlphaAt(float.NaN, false)), "NaN gives a number");
        Near(Evergrove.Core.IdleFade.AlphaAt(float.NaN, false), 1f, "NaN counts as a fresh touch");
        Near(Evergrove.Core.IdleFade.AlphaAt(-3f, false), 1f, "negative counts as a fresh touch");

        float previous = float.MaxValue;
        for (float s = 0f; s <= 10f; s += 0.01f)
        {
            float a = Evergrove.Core.IdleFade.AlphaAt(s, false);
            Expect(a <= previous + 1e-6f, "monotonic non-increasing at " + s);
            Expect(a >= 0.2f - 1e-6f && a <= 1f + 1e-6f, "alpha stays in [0.2, 1] at " + s);
            previous = a;
        }

        string source = ReadRepoFile("games/evergrove/Assets/Scripts/Unity/EvergroveApp.Idle.cs");
        Expect(source != null, "EvergroveApp.Idle.cs can be found");
        if (source != null)
        {
            Expect(source.Contains(".alpha = ") || source.Contains(".alpha != alpha") || source.Contains("group.alpha"),
                "idle file sets CanvasGroup.alpha");
            Expect(!source.Contains("SetActive"), "idle file never calls SetActive");
            Expect(!source.Contains("interactable"), "idle file never sets interactable");
            Expect(!source.Contains("blocksRaycasts"), "idle file never sets blocksRaycasts");
        }

        return assertions;
    }

    private static string ReadRepoFile(string relative)
    {
        string[] starts = { Directory.GetCurrentDirectory(), AppContext.BaseDirectory };
        foreach (string start in starts)
        {
            string dir = Path.GetFullPath(start);
            for (int depth = 0; depth < 12 && dir != null; depth++)
            {
                string candidate = Path.Combine(dir, relative);
                if (File.Exists(candidate)) return File.ReadAllText(candidate);
                dir = Path.GetDirectoryName(dir);
            }
        }
        return null;
    }
}
