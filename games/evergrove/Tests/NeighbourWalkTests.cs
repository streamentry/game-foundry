using System;
using System.IO;
using System.Text.RegularExpressions;
using Evergrove.Core;

// Checklist 24: a neighbour whose spot moves on the hub walks there in a short gentle walk. The pure
// rules (NeighbourWalk) are tested here; the Unity side (NeighbourWalker, placement, the walk tour)
// is checked by source audit, since UnityEngine is not part of the headless Core build.
internal static class NeighbourWalkTests
{
    private static int assertions;

    private static void Expect(bool ok, string message)
    {
        assertions++;
        if (!ok) throw new Exception("NeighbourWalkTests failed: " + message);
    }

    public static int Run()
    {
        assertions = 0;
        DurationClampsAndGrows();
        StepsAreAtLeastTwo();
        ProgressIsEasedFromZeroToOne();
        BobAndTiltRestAtTheEnds();
        NoWalkBelowMinDistance();
        WalkerMovesOnlyPositionAndRotation();
        PlacementWalksOnlyWithGentleMotion();
        WalkSurvivesRelayout();
        ClashChecksUseWalkDestinations();
        TourSavesTheWalkShots();
        TourLogsEveryNeighbourSpot();
        HubCallsWalkTour();
        RegisteredInTheSuite();
        return assertions;
    }

    private static void DurationClampsAndGrows()
    {
        Expect(NeighbourWalk.Duration(0f) == 0.6f, "a zero move takes the shortest walk");
        Expect(NeighbourWalk.Duration(float.NaN) == 0.6f, "a bad distance takes the shortest walk");
        Expect(NeighbourWalk.Duration(1f) == 0.6f, "a tiny move is clamped to 0.6 s");
        Expect(NeighbourWalk.Duration(100000f) == 1.6f, "a long move is clamped to 1.6 s");
        float previous = 0f;
        for (float d = 6f; d <= 400f; d += 6f)
        {
            float seconds = NeighbourWalk.Duration(d);
            Expect(seconds >= 0.6f && seconds <= 1.6f, "duration within 0.6 to 1.6 s at " + d);
            Expect(seconds >= previous, "a farther move never walks for less time at " + d);
            previous = seconds;
        }
        Expect(NeighbourWalk.Duration(70f) < NeighbourWalk.Duration(140f), "a move twice as far takes longer");
        Expect(NeighbourWalk.Speed == 140f, "the gentle pace is 140 points a second");
    }

    private static void StepsAreAtLeastTwo()
    {
        Expect(NeighbourWalk.Steps(0f) == 2 && NeighbourWalk.Steps(1f) == 2, "a short walk still has two steps");
        Expect(NeighbourWalk.Steps(float.NaN) >= 2, "a bad distance still has two steps");
        int last = 0;
        for (float d = 0f; d <= 2000f; d += 5f)
        {
            int steps = NeighbourWalk.Steps(d);
            Expect(steps >= 2 && steps <= NeighbourWalk.MaxSteps, "steps between 2 and the maximum at " + d);
            Expect(steps >= last, "more distance never means fewer steps at " + d);
            last = steps;
        }
    }

    private static void ProgressIsEasedFromZeroToOne()
    {
        const float duration = 1.2f;
        Expect(NeighbourWalk.Progress(0f, duration) == 0f, "the walk starts at 0");
        Expect(NeighbourWalk.Progress(duration, duration) == 1f, "the walk ends at exactly 1");
        Expect(NeighbourWalk.Progress(duration * 3f, duration) == 1f, "past the duration it stays at 1");
        Expect(NeighbourWalk.Progress(-1f, duration) == 0f, "before the start it stays at 0");
        Expect(NeighbourWalk.Progress(0.3f, 0f) == 1f, "no duration means already there");
        float previous = -1f;
        for (int i = 0; i <= 1000; i++)
        {
            float p = NeighbourWalk.Progress(duration * i / 1000f, duration);
            Expect(p >= 0f && p <= 1f, "progress stays between 0 and 1");
            Expect(p >= previous, "progress never goes back");
            previous = p;
        }
        Expect(NeighbourWalk.Progress(duration / 2f, duration) == 0.5f, "eased progress is half way at the middle");
    }

    private static void BobAndTiltRestAtTheEnds()
    {
        for (int steps = 2; steps <= NeighbourWalk.MaxSteps; steps++)
        {
            const float duration = 1f;
            Expect(NeighbourWalk.Bob(0f, duration, steps) == 0f, "no lift at the start (" + steps + " steps)");
            Expect(NeighbourWalk.Bob(duration, duration, steps) == 0f, "no lift at the end (" + steps + " steps)");
            Expect(NeighbourWalk.Tilt(0f, duration, steps) == 0f, "no tilt at the start (" + steps + " steps)");
            Expect(NeighbourWalk.Tilt(duration, duration, steps) == 0f, "no tilt at the end (" + steps + " steps)");
            float peak = 0f, rock = 0f;
            for (int i = 0; i <= 2000; i++)
            {
                float t = duration * i / 2000f;
                float lift = NeighbourWalk.Bob(t, duration, steps);
                Expect(lift >= 0f && lift <= 6f, "the lift is small (at most 6 points): " + lift);
                peak = Math.Max(peak, lift);
                rock = Math.Max(rock, Math.Abs(NeighbourWalk.Tilt(t, duration, steps)));
                Expect(Math.Abs(NeighbourWalk.Tilt(t, duration, steps)) <= NeighbourWalk.TiltDegrees + 0.0001f, "the tilt is at most 3 degrees");
            }
            Expect(peak > 4.5f && peak <= 5f + 0.0001f, "each step lifts about 5 points at its peak");
            Expect(rock > 2.5f, "each step rocks about 3 degrees");
        }
        // Each step's rock alternates: the first step leans one way, the next the other.
        // Four steps over one second: the middles of the first two steps lean opposite ways.
        Expect(NeighbourWalk.Tilt(0.125f, 1f, 4) > 0f && NeighbourWalk.Tilt(0.375f, 1f, 4) < 0f, "the rock alternates from step to step");
        Expect(NeighbourWalk.Bob(0.3f, 1f, 3) == NeighbourWalk.Bob(0.3f, 1f, 3), "the walk is deterministic");
    }

    private static void NoWalkBelowMinDistance()
    {
        Expect(NeighbourWalk.MinDistance == 6f, "a walk needs at least 6 points of distance");
        Expect(!NeighbourWalk.Moves(0f) && !NeighbourWalk.Moves(5.9f), "a move under 6 points does not walk");
        Expect(NeighbourWalk.Moves(6f) && NeighbourWalk.Moves(120f), "a move of 6 points or more walks");
        Expect(!NeighbourWalk.Moves(float.NaN), "a bad distance does not walk");
    }

    // The walker moves the holder with lerp, bob and tilt, and changes nothing else on the transform.
    private static void WalkerMovesOnlyPositionAndRotation()
    {
        string walker = ReadEvergrove("Assets/Scripts/Unity/NeighbourWalker.cs");
        Expect(walker.Contains("Time.unscaledTime - StartedAt") && !Regex.IsMatch(walker, @"\bTime\.deltaTime\b"),
            "the walker runs on unscaled time");
        Expect(walker.Contains("rect.anchoredPosition = To;") && walker.Contains("rect.localRotation = Quaternion.identity;") &&
               walker.Contains("enabled = false;"), "the walker ends exactly at To, upright, and disables itself");
        Expect(walker.Contains("NeighbourWalk.Progress(") && walker.Contains("NeighbourWalk.Bob(") && walker.Contains("NeighbourWalk.Tilt("),
            "the walker uses the pure walk rules");
        foreach (Match m in Regex.Matches(walker, @"\b(rect|transform)\.(\w+)\s*(=|\+=|-=)"))
            Expect(m.Groups[2].Value == "anchoredPosition" || m.Groups[2].Value == "localRotation",
                "the walker only moves anchoredPosition and localRotation, not " + m.Groups[2].Value);
        foreach (string token in new[] { "sizeDelta", "offsetMin", "offsetMax", "anchorMin", "anchorMax", "LayoutElement",
                     "preferredWidth", "preferredHeight", "localScale", "fontSize", "localPosition", "SetParent" })
            Expect(!walker.Contains(token), "the walker does not touch " + token);
    }

    // The hub places each neighbour: a walk from the last spot only with gentle motion, the spot is
    // always recorded, and the record is cleared when the garden changes.
    private static void PlacementWalksOnlyWithGentleMotion()
    {
        string app = UnityAppSource.ReadAll();
        string neighbours = ReadEvergrove("Assets/Scripts/Unity/EvergroveApp.Neighbours.cs");
        string helper = ReadEvergrove("Assets/Scripts/Unity/EvergroveApp.NeighbourWalk.cs");
        Expect(neighbours.Contains("bool walking = NeighbourStartFor(villager, at, fromTitle, out Vector2 walkFrom, out float walkSince);"),
            "placement asks where the neighbour walks from");
        Expect(neighbours.Contains("rect.anchoredPosition = walking ? NeighbourWalker.PoseAt(walkFrom, at, walkSince) : at;"), "a walking neighbour starts where its walk has reached (the last spot, at first)");
        Expect(neighbours.Contains("holder.AddComponent<NeighbourWalker>()") && neighbours.Contains("walker.From = walkFrom;") &&
               neighbours.Contains("walker.To = at;"), "a walk is added from the last spot to the new one");
        Expect(neighbours.Contains("else if (fromTitle && waveHello && gentleMotion)") && neighbours.IndexOf("if (walking)", StringComparison.Ordinal) <
               neighbours.IndexOf("else if (fromTitle && waveHello && gentleMotion)", StringComparison.Ordinal),
            "the walk replaces the hello hop for that neighbour");
        Expect(helper.Contains("if (!known || !gentleMotion || !NeighbourWalk.Moves(Vector2.Distance(last, at))) return false;"),
            "no walk without gentle motion or a known, long-enough move");
        Expect(helper.Contains("neighbourLastAt[villager] = at;"), "the hub spot is always recorded");
        Expect(helper.Contains("if (world.Garden != neighbourWalkGarden)") && helper.Contains("neighbourLastAt.Clear();"),
            "the record is cleared when the garden changes");
        Expect(helper.Contains("if (!fromTitle) return false;"), "only the hub compares spots");
        Expect(app.Contains("private readonly Dictionary<int, Vector2> neighbourLastAt") || helper.Contains("Dictionary<int, Vector2> neighbourLastAt"),
            "the last spots are kept per villager");
    }

    // Clash checks place the next neighbour against where each placed one will stand (a walker's
    // destination), never where its walk starts, so nobody ends a walk on top of another.
    // The map lays its land out several times after a build and places the neighbours each time, so a
    // walk is kept per neighbour and a re-placement mid-walk carries on from the same start and time.
    private static void WalkSurvivesRelayout()
    {
        string walk = ReadEvergrove("Assets/Scripts/Unity/EvergroveApp.NeighbourWalk.cs");
        string neighbours = ReadEvergrove("Assets/Scripts/Unity/EvergroveApp.Neighbours.cs");
        Expect(walk.Contains("neighbourWalks.TryGetValue(villager, out NeighbourWalkUnderWay under)") &&
               walk.Contains("startedAt = under.StartedAt;"), "a walk under way to the same spot carries on");
        Expect(walk.Contains("neighbourWalks.Clear();"), "walks are forgotten with the garden");
        Expect(neighbours.Contains("NeighbourWalker.PoseAt(walkFrom, at, walkSince)") && neighbours.Contains("walker.StartedAt = walkSince;"),
            "a re-placed holder starts where the walk has reached, not back at the start");
    }

    private static void ClashChecksUseWalkDestinations()
    {
        string neighbours = ReadEvergrove("Assets/Scripts/Unity/EvergroveApp.Neighbours.cs");
        string walk = ReadEvergrove("Assets/Scripts/Unity/EvergroveApp.NeighbourWalk.cs");
        Expect(!neighbours.Contains("other.anchoredPosition"), "no clash check reads a walker's starting point");
        Expect(neighbours.Contains("NeighbourSpot(other)"), "clash checks use NeighbourSpot");
        Expect(walk.Contains("return walker != null ? walker.To : placed.anchoredPosition;"), "NeighbourSpot is the walk's end");
    }

    private static void TourSavesTheWalkShots()
    {
        string tour = ReadEvergrove("Assets/Scripts/Unity/DevQaTour.Walk.cs");
        Expect(tour.Contains("private IEnumerator WalkShots()"), "the walk tour is a coroutine");
        Expect(tour.Contains("app.QaSetClock(9);") && tour.Contains("app.QaSetClock(19);") && tour.Contains("app.QaRebuildHub();"),
            "the tour moves from morning to dusk and redraws the hub");
        Expect(tour.IndexOf("app.QaShowHub(true);", StringComparison.Ordinal) < tour.IndexOf("app.QaSetClock(19);", StringComparison.Ordinal),
            "the morning hub is shown before the clock moves");
        Expect(tour.Contains("for (int i = 0; i < 16; i++)") && tour.Contains("WaitForSecondsRealtime(0.1f)") &&
               tour.Contains("WaitForEndOfFrame()") && tour.Contains("SavePng(\"23-walk-\" + i.ToString(\"00\") + \".png\")"),
            "the tour saves 16 frames a tenth of a second apart");
        Expect(tour.Contains("app.QaShowHub(false);") && tour.Contains("app.QaSetClock(13);"), "the tour closes the hub and restores the clock");
        Expect(ReadEvergrove("Assets/Scripts/Unity/EvergroveApp.WalkQa.cs").Contains("RefreshMaps();"),
            "the QA rebuild redraws the hub without closing it");
    }

    private static void TourLogsEveryNeighbourSpot()
    {
        string tour = ReadEvergrove("Assets/Scripts/Unity/DevQaTour.Walk.cs");
        Expect(tour.Contains("Debug.Log(\"MOSSELORA_WALK \" + frame"), "each frame logs the neighbour spots as MOSSELORA_WALK");
        Expect(tour.Contains("rect.name != \"Neighbour on the map\""), "the log finds the neighbours by their holder name");
        Expect(tour.Contains("FindObjectsByType<RectTransform>"), "the log finds the holders in the scene");
    }

    private static void HubCallsWalkTour()
    {
        Expect(ReadEvergrove("Assets/Scripts/Unity/DevQaTour.cs").Contains("yield return WalkShots();"), "the tour runs the walk shots");
        string tour = ReadEvergrove("Assets/Scripts/Unity/DevQaTour.cs");
        Expect(tour.IndexOf("yield return WalkShots();", StringComparison.Ordinal) > tour.IndexOf("yield return ShadowShots();", StringComparison.Ordinal),
            "the walk shots come after the shadow shots");
    }

    private static void RegisteredInTheSuite()
    {
        string program = ReadEvergrove("Tests/Program.cs");
        Expect(program.Contains("assertions += NeighbourWalkTests.Run();"), "NeighbourWalkTests is registered in the suite");
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
