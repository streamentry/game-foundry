using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Evergrove.Core;

// Checklist 38, chains between puzzles (visible only): a stream path waters the plots, the plots send
// a seedling to the nursery. The rules are pure; the card line, the watered mark and the tour are
// checked by source audit (the Unity client is not compiled here).
internal static class PuzzleChainTests
{
    private static int assertions;

    private static void Expect(bool ok, string message)
    {
        assertions++;
        if (!ok) throw new Exception("PuzzleChainTests failed: " + message);
    }

    // The same loss and guilt words as DayRhythmTests (checklist 54's wording rule).
    private static readonly Regex LossWords = new Regex(
        @"\b(died|dead|dying|die|wilt\w*|withered|lost|lose|loss|missed|miss|gone|broke|broken|weeds?|overgrown|decay\w*|too late|sorry|neglect\w*|abandon\w*|ruin\w*|forgot\w*|dry|dried|guilt\w*|waiting for you|wasted?|empty|fail\w*|left behind|away|hurry|hurry up|late)\b",
        RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

    private static readonly Regex NoPromises = new Regex(
        @"\b(I'll|soon|tomorrow|will|always|never|guarantee\w*|promise\w*|easy)\b",
        RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

    public static int Run()
    {
        assertions = 0;
        OnlyTwoKindsGiveALine();
        StreamNeedsAPlotsThing();
        PlotsNeedTheNursery();
        LinesFollowTheWordingRules();
        GardenHoldsPlotsFromTheStoryAndTheNursery();
        WateredAndSeedlingAreSameDayOnly();
        DayStampIsLocalCalendarDay();
        NoSeedsOrStockInTheNewFiles();
        CardShowsTheLineNoLaterThanTheSettle();
        ReducedMotionKeepsTheLineStill();
        ViewFilesDidNotGrow();
        NurseryShowsTheSeedling();
        TourSavesShotsAndTimesTheLine();
        Console.WriteLine("Puzzle chain: " + assertions + " assertions");
        return assertions;
    }

    private static void OnlyTwoKindsGiveALine()
    {
        int kindsWithALine = 0;
        foreach (PuzzleKind kind in Enum.GetValues(typeof(PuzzleKind)))
        {
            bool any = false;
            foreach (bool plots in new[] { false, true })
                foreach (bool nursery in new[] { false, true })
                    if (PuzzleChain.CauseFor(kind, plots, nursery) != null) any = true;
            if (any) kindsWithALine++;
            if (kind != PuzzleKind.StreamPath && kind != PuzzleKind.Plots)
                Expect(!any, kind + " gives no chain line");
        }
        Expect(kindsWithALine == 2, "exactly two kinds start a chain: " + kindsWithALine);
    }

    private static void StreamNeedsAPlotsThing()
    {
        Expect(PuzzleChain.CauseFor(PuzzleKind.StreamPath, true, false) == PuzzleChain.StreamCause,
            "a stream path with plots in the garden names its cause");
        Expect(PuzzleChain.CauseFor(PuzzleKind.StreamPath, false, true) == null,
            "a stream path with no plots in the garden shows nothing");
        Expect(PuzzleChain.CauseFor(PuzzleKind.StreamPath, true, true) == PuzzleChain.StreamCause,
            "the nursery does not change the stream's cause");
    }

    private static void PlotsNeedTheNursery()
    {
        Expect(PuzzleChain.CauseFor(PuzzleKind.Plots, false, true) == PuzzleChain.SeedlingCause,
            "plots with a nursery name the seedling");
        Expect(PuzzleChain.CauseFor(PuzzleKind.Plots, true, false) == null,
            "plots with no nursery show nothing");
    }

    private static void LinesFollowTheWordingRules()
    {
        foreach (string line in new[] { PuzzleChain.StreamCause, PuzzleChain.SeedlingCause })
        {
            Expect(!LossWords.IsMatch(line), "no loss or guilt words in: " + line);
            Expect(!NoPromises.IsMatch(line), "no promises or time words in: " + line);
            Expect(!line.Contains("!"), "no exclamation in: " + line);
            Expect(line.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries).Length < 12,
                "under twelve words: " + line);
            Expect(line.EndsWith("."), "a plain sentence: " + line);
        }
    }

    private static void GardenHoldsPlotsFromTheStoryAndTheNursery()
    {
        // Story chapters 4 to 12 cycle through the new kinds: chapter 6 is plots (Cycle[2]).
        int[] withSix = new int[7];
        Expect(PuzzleChain.GardenHoldsPlots(withSix, new int[0]), "a chosen story thing at chapter 6 holds plots");
        Expect(!PuzzleChain.GardenHoldsPlots(new[] { 0, 1, 2, 0 }, new int[0]),
            "the classic four things hold no plots");
        Expect(!PuzzleChain.GardenHoldsPlots(new[] { -1, -1, -1, -1, -1, -1, -1 }, new int[0]),
            "an unchosen thing holds nothing");
        bool anyNurseryPlots = false;
        for (int kind = 0; kind < Nursery.Kinds; kind++)
        {
            if (ThingPuzzle.ForNursery(kind) != PuzzleKind.Plots) continue;
            anyNurseryPlots = true;
            Expect(PuzzleChain.GardenHoldsPlots(new int[0], new[] { kind, 0 }), "a nursery plant of kind " + kind + " holds plots");
        }
        Expect(anyNurseryPlots, "the nursery has a plots plant to test with");
        Expect(!PuzzleChain.GardenHoldsPlots(null, null), "no garden, no plots");
    }

    private static void WateredAndSeedlingAreSameDayOnly()
    {
        Expect(PuzzleChain.WateredToday(20261128, 20261128), "watered on the day of the solve");
        Expect(!PuzzleChain.WateredToday(20261127, 20261128), "not watered the next day");
        Expect(!PuzzleChain.WateredToday(0, 20261128), "never watered with no solve on record");
        Expect(!PuzzleChain.WateredToday(0, 0), "no day stamp is never a day");
        Expect(PuzzleChain.SeedlingToday(20261128, 20261128), "a seedling on the day of the plots solve");
        Expect(!PuzzleChain.SeedlingToday(20261127, 20261128), "no seedling the next day");
        Expect(!PuzzleChain.SeedlingToday(0, 0), "no seedling without a day");
    }

    private static void DayStampIsLocalCalendarDay()
    {
        Expect(PuzzleChain.DayStamp(new DateTime(2026, 11, 28, 23, 59, 0)) == 20261128, "the day stamp is yyyymmdd");
        Expect(PuzzleChain.DayStamp(new DateTime(2027, 1, 2)) == 20270102, "the day stamp pads month and day");
    }

    private static void NoSeedsOrStockInTheNewFiles()
    {
        string[] files = { "Core/PuzzleChain.cs", "Unity/ChainCause.cs", "Unity/DevQaTour.Chains.cs" };
        string[] forbidden = { "WorldModel", "ChangeSeeds", "AddSeeds", "SetSeeds", "SaveWorld", "SolveTree(",
            "SolveBought(", "NurseryStock", "BuyFrom", "state.", "Restock", "PlaceFree" };
        foreach (string file in files)
        {
            string text = Script(file);
            foreach (string word in forbidden)
                Expect(!text.Contains(word), file + " must not contain " + word);
        }
    }

    private static void CardShowsTheLineNoLaterThanTheSettle()
    {
        string chain = Script("Unity/ChainCause.cs");
        string tree = Script("Unity/TreePlay.cs");
        float settle = ReadFloat(tree, @"SettleAfter = ([0-9.]+)f", "TreePlay.SettleAfter");
        float showAfter = ReadFloat(chain, @"ShowAfter = ([0-9.]+)f", "ChainCause.ShowAfter");
        float fade = ReadFloat(chain, @"FadeTime = ([0-9.]+)f", "ChainCause.FadeTime");
        Expect(showAfter <= settle, "the line shows no later than the settle: " + showAfter + " <= " + settle);
        Expect(showAfter + fade <= settle, "the line is fully shown within the settle: " + (showAfter + fade));
        Expect(showAfter <= 1.0f, "within two seconds of the solve, and within the one-second target");
        Expect(tree.Contains("board.Lock(); ChainCause.Show(cardRect, uiFont, gentle);"),
            "TreePlay shows the line when a board is solved, with the card's motion setting");
    }

    private static void ReducedMotionKeepsTheLineStill()
    {
        string chain = Script("Unity/ChainCause.cs");
        Expect(chain.Contains("drift = gentle ? icon : null;"), "the drift is only set with motion on");
        Expect(chain.Contains("group.alpha = gentle ? 0f : 1f;"), "with reduced motion the line is shown at once");
        Expect(chain.Contains("if (drift != null)"), "the drift runs only while it is set");
        Expect(chain.Contains("if (!gentle) ChainCause.ShownAt = Time.unscaledTime;"),
            "with reduced motion the line counts as shown at once");
        Expect(Regex.Matches(chain, "raycastTarget = false").Count >= 5, "every glyph and text is non-blocking");
    }

    private static void ViewFilesDidNotGrow()
    {
        // Checklist 20: these two are allowlisted and may only shrink; the chain adds calls, not lines.
        Expect(Script("Unity/TreePlay.cs").Count(c => c == '\n') == 1099, "TreePlay.cs is still 1099 lines");
        Expect(Script("Unity/GardenMapView.cs").Count(c => c == '\n') == 2362, "GardenMapView.cs is still 2362 lines");
        Expect(Script("Unity/GardenMapView.cs").Contains("ChainCause.Glisten(land, at, cellW, cellH, item.Kind, item.Watered && !held);"),
            "the watered mark is drawn on the map");
    }

    // The plots-to-nursery link's cosmetic change: the nursery bag token wears a sprout on the day of a plots
    // solve, drawn only (no stock, no seeds), and never takes a tap.
    private static void NurseryShowsTheSeedling()
    {
        string actions = Script("Unity/EvergroveApp.MapActions.cs");
        int mark = actions.IndexOf("if (ChainCause.SeedlingNow(world.Garden))", StringComparison.Ordinal);
        Expect(mark > 0, "the nursery bag shows the seedling mark on a plots day");
        string block = actions.Substring(mark, Math.Min(500, actions.Length - mark));
        Expect(block.Contains("\"A seedling from the plots\"") && block.Contains("sprout.raycastTarget = false;"),
            "the mark is a named, untappable ornament");
        Expect(!block.Contains("ChangeSeeds") && !block.Contains("Buy("), "the mark changes no seeds or stock");
    }

    private static void TourSavesShotsAndTimesTheLine()
    {
        string tour = Script("Unity/DevQaTour.Chains.cs");
        Expect(tour.Contains("#if UNITY_EDITOR || DEVELOPMENT_BUILD"), "the tour is guarded like the other tour files");
        Expect(tour.Contains("private IEnumerator ChainShots()"), "the tour defines ChainShots");
        Expect(tour.Contains("\"27-chain-stream\"") && tour.Contains("\"27-chain-plots\""), "the stream and plots shots are named");
        Expect(tour.Contains("SavePng(shot + \"-0.6s.png\")") && tour.Contains("SavePng(shot + \"-1.5s.png\")"),
            "the shots are taken at 0.6 s and 1.5 s after the solve");
        Expect(tour.Contains("SavePng(\"27-chain-watered.png\")"), "the hub's watered shot is saved");
        Expect(tour.Contains("\"MOSSELORA_CHAIN \""), "the tour logs MOSSELORA_CHAIN");
        string tourMain = Script("Unity/DevQaTour.cs");
        int chain = tourMain.IndexOf("yield return ChainShots();", StringComparison.Ordinal);
        int window = tourMain.IndexOf("yield return WindowShots();", StringComparison.Ordinal);
        Expect(chain >= 0 && window > chain, "DevQaTour calls ChainShots just before WindowShots");
        Expect(tourMain.Count(c => c == '\n') <= 1177, "DevQaTour.cs stays within its allowed size");
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
        throw new Exception("PuzzleChainTests: " + relative + " not found from " + Directory.GetCurrentDirectory());
    }

    private static float ReadFloat(string text, string pattern, string what)
    {
        Match m = Regex.Match(text, pattern);
        Expect(m.Success, what + " is declared");
        return float.Parse(m.Groups[1].Value, System.Globalization.CultureInfo.InvariantCulture);
    }
}
