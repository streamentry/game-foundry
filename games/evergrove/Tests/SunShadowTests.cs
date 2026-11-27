using System;
using System.IO;
using Evergrove.Core;

// Checklist 22: every thing casts one soft shadow whose angle follows the sun. Pure rules are tested
// here; the Unity side (placement, sibling order, tour shots, the app's phase) is checked by source audit.
internal static class SunShadowTests
{
    private static int assertions;

    private static void Expect(bool ok, string message)
    {
        assertions++;
        if (!ok) throw new Exception("SunShadowTests failed: " + message);
    }

    private static readonly DayPhase[] AllPhases = { DayPhase.Dawn, DayPhase.Morning, DayPhase.Day, DayPhase.Golden, DayPhase.Dusk, DayPhase.Night };

    public static int Run()
    {
        assertions = 0;
        EveryPhaseHasAShadowRule();
        NightCastsNoShadow();
        DawnFallsLeftDuskAndGoldenFallRight();
        NoonIsShortestAndDarkest();
        DawnAndDuskAreLongest();
        AlphaStaysFaint();
        HourMapsToTheExpectedPhases();
        SourceAudits();
        return assertions;
    }

    private static void EveryPhaseHasAShadowRule()
    {
        foreach (DayPhase phase in AllPhases)
        {
            SunShadowLook look = SunShadows.For(phase);
            Expect(look.Length >= 0f, phase + " has a non-negative length");
            Expect(look.Shows || (look.Length == 0f && look.Alpha == 0f), phase + " without a shadow has no length or alpha");
        }
    }

    private static void NightCastsNoShadow()
    {
        SunShadowLook night = SunShadows.For(DayPhase.Night);
        Expect(!night.Shows, "night casts no shadow");
        Expect(!night.FallsLeft && !night.FallsRight, "a night shadow falls nowhere");
    }

    private static void DawnFallsLeftDuskAndGoldenFallRight()
    {
        Expect(SunShadows.For(DayPhase.Dawn).FallsLeft, "dawn shadow falls left (sun low in the east)");
        Expect(SunShadows.For(DayPhase.Morning).FallsLeft, "morning shadow leans left");
        Expect(SunShadows.For(DayPhase.Golden).FallsRight, "golden-hour shadow falls right (sun low in the west)");
        Expect(SunShadows.For(DayPhase.Dusk).FallsRight, "dusk shadow falls right");
        SunShadowLook noon = SunShadows.For(DayPhase.Day);
        Expect(!noon.FallsLeft && !noon.FallsRight, "noon shadow sits almost under the thing, toward the viewer");
    }

    private static void NoonIsShortestAndDarkest()
    {
        SunShadowLook noon = SunShadows.For(DayPhase.Day);
        foreach (DayPhase phase in AllPhases)
        {
            SunShadowLook other = SunShadows.For(phase);
            if (phase == DayPhase.Day || !other.Shows) continue;
            Expect(noon.Length < other.Length, "noon is shorter than " + phase);
            Expect(noon.Alpha > other.Alpha, "noon is darker than " + phase);
        }
    }

    private static void DawnAndDuskAreLongest()
    {
        float longest = 0f;
        foreach (DayPhase phase in AllPhases) longest = Math.Max(longest, SunShadows.For(phase).Length);
        Expect(SunShadows.For(DayPhase.Dawn).Length == longest, "dawn is among the longest shadows");
        Expect(SunShadows.For(DayPhase.Dusk).Length == longest, "dusk is among the longest shadows");
        Expect(SunShadows.For(DayPhase.Dawn).Length > SunShadows.For(DayPhase.Golden).Length, "dawn is longer than golden hour");
    }

    private static void AlphaStaysFaint()
    {
        foreach (DayPhase phase in AllPhases)
        {
            SunShadowLook look = SunShadows.For(phase);
            if (!look.Shows) continue;
            Expect(look.Alpha >= 0.3f && look.Alpha <= 0.5f, phase + " alpha is within [0.3, 0.5]: visible on the painted ground, never black");
            Expect(look.Alpha <= SunShadows.MaxAlpha, phase + " alpha is within the cap");
        }
        Expect(Math.Abs(SunShadows.For(DayPhase.Day).Alpha - 0.46f) < 0.001f, "noon alpha is about 0.46");
    }

    private static void HourMapsToTheExpectedPhases()
    {
        Expect(DayLight.PhaseAt(6, 0) == DayPhase.Dawn, "06:00 is dawn");
        Expect(DayLight.PhaseAt(12, 0) == DayPhase.Day, "12:00 is day");
        Expect(DayLight.PhaseAt(19, 0) == DayPhase.Dusk, "19:00 is dusk");
    }

    private static void SourceAudits()
    {
        string view = ReadEvergrove("Assets/Scripts/Unity/GardenMapView.cs");
        string shadow = ReadEvergrove("Assets/Scripts/Unity/SunShadow.cs");
        string app = ReadEvergrove("Assets/Scripts/Unity/EvergroveApp.Daylight.cs");
        string tour = ReadEvergrove("Assets/Scripts/Unity/DevQaTour.Shadows.cs");
        string tourMain = ReadEvergrove("Assets/Scripts/Unity/DevQaTour.cs");

        int onMap = view.IndexOf("var node = new GameObject(\"On the map\"", StringComparison.Ordinal);
        int cast = view.IndexOf("SunShadow.Cast(rect, nodes);", StringComparison.Ordinal);
        Expect(cast > 0 && onMap > 0 && cast > onMap, "the map casts a sun shadow for every placed thing");
        Expect(view.IndexOf("SunShadow.Cast(", StringComparison.Ordinal) == cast, "one shadow call per thing");
        Expect(view.Split('\n').Length - 1 == 2362, "GardenMapView.cs line count is unchanged (2362)");

        Expect(shadow.Contains("shadow.transform.SetSiblingIndex(thing.GetSiblingIndex());"), "the shadow takes the thing's sibling index");
        Expect(shadow.IndexOf("SetSiblingIndex", StringComparison.Ordinal) < shadow.IndexOf("AddComponent<Image>()", StringComparison.Ordinal),
            "the shadow is placed before the thing is drawn over it");
        Expect(shadow.Contains("image.raycastTarget = false;"), "the shadow never takes input");
        Expect(shadow.Contains("nodes.Add(rect);"), "the shadow is recorded with the nodes a relayout destroys");
        Expect(shadow.Contains("MosseloraArt.SoftGlow()"), "the shadow uses the soft glow sprite");

        Expect(app.Contains("SunShadow.Phase = on ? DayLight.PhaseAt(hour, minute) : DayPhase.Night;"), "the app sets the sun's phase with the light");

        Expect(tour.Contains("SavePng(\"22-shadow-dawn.png\")"), "the tour saves the dawn shadow shot");
        Expect(tour.Contains("SavePng(\"22-shadow-noon.png\")"), "the tour saves the noon shadow shot");
        Expect(tour.Contains("SavePng(\"22-shadow-dusk.png\")"), "the tour saves the dusk shadow shot");
        Expect(tour.Contains("LogShadowCount(\"dawn\")") && tour.Contains("LogShadowCount(\"noon\")") && tour.Contains("LogShadowCount(\"dusk\")"),
            "each shadow shot logs how many sun shadows are on the map (a faint effect is checked by count)");
        Expect(tour.Contains("app.QaSetClock(6);") && tour.Contains("app.QaSetClock(12);") && tour.Contains("app.QaSetClock(19);"),
            "the tour sets the clock to dawn, noon and dusk");
        Expect(tourMain.Contains("yield return ShadowShots();"), "the tour runs the shadow shots");
    }

    private static string ReadEvergrove(string relative)
    {
        foreach (string start in new[] { Directory.GetCurrentDirectory(), AppContext.BaseDirectory })
        {
            for (var dir = new DirectoryInfo(start); dir != null; dir = dir.Parent)
            {
                string nested = Path.Combine(dir.FullName, "games", "evergrove", relative);
                if (File.Exists(nested)) return File.ReadAllText(nested);
                string direct = Path.Combine(dir.FullName, relative);
                if (File.Exists(direct)) return File.ReadAllText(direct);
            }
        }
        throw new Exception("Could not find " + relative + " from " + Directory.GetCurrentDirectory());
    }
}
