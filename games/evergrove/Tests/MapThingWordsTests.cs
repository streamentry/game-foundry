using System;
using System.IO;
using Evergrove.Core;

/// <summary>
/// Idea 5: a thing on the garden map says what it is. The caption wins, a blank caption falls
/// back to the family label, and the Unity map attaches the words to a hidden "Caption" child
/// while keeping the "On the map" node name.
/// </summary>
internal static class MapThingWordsTests
{
    private static int assertions;

    private static void Expect(bool ok, string message)
    {
        assertions++;
        if (!ok) throw new Exception("MapThingWords test failed: " + message);
    }

    public static int Run()
    {
        Expect(MapThingWords.Spoken("Apple tree, planted by Mum", "Shade tree") == "Apple tree, planted by Mum", "a caption wins over the family label");
        Expect(MapThingWords.Spoken("   ", "Shade tree") == "Shade tree", "a blank caption falls back to the family label");
        Expect(MapThingWords.Spoken("", "Flowers") == "Flowers", "an empty caption falls back to the family label");
        Expect(MapThingWords.Spoken(null, "Bridge") == "Bridge", "a null caption falls back to the family label");
        Expect(MapThingWords.Spoken(null, null) == "", "no caption and no label says nothing");
        Expect(MapThingWords.Spoken(" \t ", " ") == "", "blank caption and blank label say nothing");
        Expect(MapThingWords.Spoken("  Oak  ", "Shade tree") == "Oak", "a caption is trimmed");
        Expect(MapThingWords.Spoken(null, "  Flowers ") == "Flowers", "a label is trimmed");

        string mapView = Read("Assets/Scripts/Unity/GardenMapView.cs");
        string speech = Read("Assets/Scripts/Unity/GardenMapView.Speech.cs");
        Expect(mapView.Contains("SpeakMapThing(node.transform, MapThingWords.Spoken(item.Caption, LocalizedVillage.Choice(item.Family)))"),
            "the map speaks each thing's words, the same caption the long press shows");
        Expect(speech.Contains("new GameObject(\"Caption\"") && speech.Contains("text.raycastTarget = false"),
            "the map gives the thing a hidden Caption child that takes no taps");
        Expect(mapView.Contains("\"On the map\""), "the thing node keeps its name");
        return assertions;
    }

    private static string Read(string relative)
    {
        string dir = AppContext.BaseDirectory;
        while (dir != null && !File.Exists(Path.Combine(dir, relative)))
            dir = Path.GetDirectoryName(dir);
        if (dir == null) throw new Exception(relative + " not found");
        return File.ReadAllText(Path.Combine(dir, relative));
    }
}
