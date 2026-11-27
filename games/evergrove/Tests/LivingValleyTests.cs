using System;
using System.Collections.Generic;
using Evergrove.Core;

/// <summary>
/// I363–I367 (run 3): a living valley. Wildflowers spread along the banks as streams come
/// back; a rainbow follows rain; dew and frost at morning; neighbours gather by the
/// lamplit door in the evening. All cosmetic and all true to the save, the date or the hour.
/// </summary>
internal static class LivingValleyTests
{
    private static int assertions;

    private static void Expect(bool ok, string message)
    {
        assertions++;
        if (!ok) throw new Exception("Living valley test failed: " + message);
    }

    public static int Run()
    {
        // I363: wildflowers on the banks, more as streams are restored, never on a thing.
        GardenMap map = GardenMap.River(10, 15, Mask(), 2);
        Expect(MeadowLife.Count(0) == 0 && MeadowLife.Count(1) > 0 && MeadowLife.Count(12) >= MeadowLife.Count(6), "more with each stream");
        Expect(MeadowLife.Count(200) <= MeadowLife.Most, "never a carpet");
        int[] tufts = MeadowLife.Cells(map, MeadowLife.Count(12), new int[0]);
        var used = new HashSet<int>();
        foreach (int cell in tufts)
        {
            Expect(map.IsLand(cell), "on land");
            Expect(used.Add(cell), "one tuft per cell");
        }
        Expect(tufts.Length == MeadowLife.Count(12), "as many as the count");
        int[] avoid = MeadowLife.Cells(map, 6, new[] { tufts[0] });
        Expect(Array.IndexOf(avoid, tufts[0]) < 0, "never where a thing stands");
        Expect(string.Join(",", MeadowLife.Cells(map, 6, new int[0])) == string.Join(",", MeadowLife.Cells(map, 6, new int[0])), "always the same banks");

        // I364: a rainbow on a fair day that follows a rainy one, by day only.
        var start = new DateTime(2026, 4, 1);
        int bows = 0;
        for (int d = 1; d < 200; d++)
        {
            DateTime day = start.AddDays(d);
            bool bow = DayWeather.Rainbow(day, Season.Spring, 4);
            bool expected = DayWeather.For(day, Season.Spring, 4) == Weather.Fair &&
                            DayWeather.For(day.AddDays(-1), Season.Spring, 4) == Weather.Rainy;
            Expect(bow == expected, "a rainbow exactly after rain");
            if (bow) bows++;
        }
        Expect(bows > 5, "rainbows happen (" + bows + ")");

        // I366: dew at dawn and morning; frost in winter; nothing later in the day.
        Expect(DayLight.Dew(DayPhase.Dawn, Season.Spring) == Dew.Dew && DayLight.Dew(DayPhase.Morning, Season.Winter) == Dew.Frost,
            "dew in the morning, frost in winter");
        Expect(DayLight.Dew(DayPhase.Day, Season.Spring) == Dew.None && DayLight.Dew(DayPhase.Night, Season.Winter) == Dew.None,
            "gone by day, not at night");

        // I367: in the evening the neighbours gather by the lamplit door.
        Expect(DayLight.NeighboursGather(DayPhase.Dusk) && DayLight.NeighboursGather(DayPhase.Night), "evenings together");
        Expect(!DayLight.NeighboursGather(DayPhase.Day) && !DayLight.NeighboursGather(DayPhase.Morning), "by day, by their things");
        return assertions;
    }

    private static string Mask()
    {
        string dir = AppContext.BaseDirectory;
        while (dir != null && !System.IO.File.Exists(System.IO.Path.Combine(dir, "Assets/Resources/Evergrove/garden-map-mask.txt")))
            dir = System.IO.Path.GetDirectoryName(dir.TrimEnd(System.IO.Path.DirectorySeparatorChar));
        if (dir == null) throw new Exception("garden-map-mask.txt not found");
        return System.IO.File.ReadAllText(System.IO.Path.Combine(dir, "Assets/Resources/Evergrove/garden-map-mask.txt")).Trim();
    }
}
