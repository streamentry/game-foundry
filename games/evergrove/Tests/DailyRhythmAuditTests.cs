using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text.Json;
using System.Text.RegularExpressions;
using Evergrove.Core;

/// <summary>
/// Audit of the daily rhythm (checklist 52, 53 and 60): no streak that can break, a return that
/// only reports gains, and an exact, file-checked list of every reason to come back. The lists
/// below pin the known findings: a new violation fails, and a fixed one must be removed here.
/// </summary>
internal static class DailyRhythmAuditTests
{
    private static int assertions;

    // Checklist 60: rows whose doc says "yes" for expires or punishes absence. Exactly these.
    private static readonly string[] KnownViolations =
    {
    };

    // Checklist 60: every reason the doc must name (first column). Removing a row fails the test.
    private static readonly string[] ReasonNames =
    {
        "Garden note: letter waiting",
        "Garden note: cutting in basket",
        "Garden note: mist will lift",
        "Garden note: puzzle still waiting",
        "Garden note: favourite board",
        "Garden note: daily reminder",
        "Since you were last here",
        "Today's weather",
        "Neighbour's word on the weather",
        "Rainbow after rain",
        "Light, dew and fireflies",
        "Hour remark",
        "Visitors come to the garden",
        "Keeper's word about a visitor",
        "Garden cat naps",
        "Seasonal yield from a neighbour",
        "Neighbour's cutting at a milestone",
        "Neighbour's wish",
        "Neighbour notices your garden",
        "Family parcel: letter, gift, help or lantern",
        "Take turns on one device",
        "Replay deals a fresh board",
        "First solve and new pairing seeds",
        "Old well board",
        "Next puzzle of your garden",
        "Hub recalls a memory",
        "Memory shelf",
        "Festival after the story",
        "Nursery: buy a plant",
    };

    // Checklist 53: wording that matches the loss list but is neutral in context. Exact literals.
    private static readonly string[] NeutralLiterals =
    {
        "Snowing properly today. Everything's gone quiet under it.",
        "One of the old paintings is whole again. I had forgotten that day.",
        " of the old paintings are whole again. I had forgotten some of those days.",
        "Thank you for the fruit. I'll dry some for the winter.",
        "You sowed a whole row of beds into the basket. Not a seed left behind.",
        " rows of beds into the basket. Not a seed left behind.",
    };

    // Checklist 53: source files whose string literals are return or day-visit wording.
    private static readonly string[] ReturnTextFiles =
    {
        "DayWeather.cs", "GardenNote.cs", "NeighbourNotice.cs", "NeighbourWish.cs",
        "Visitors.cs", "DayLight.cs", "GardenCat.cs", "NeighbourGifts.cs",
    };

    // Checklist 53: the only words the "since you were last here" line may use.
    private static readonly HashSet<string> KindWords = new HashSet<string>(StringComparer.Ordinal)
    {
        "since", "you", "were", "last", "here", "a", "good", "while", "has", "passed", "with", "all",
        "sorts", "of", "weather", "the", "garden", "looked", "after", "itself", "fine", "and",
        "rainy", "snowy", "windy", "hot", "day", "days", "one", "two", "three", "four", "five",
        "six", "seven", "eight", "nine",
    };

    // Checklist 53: loss-like words. A literal matching these must be in NeutralLiterals.
    private static readonly Regex LossWords = new Regex(
        @"\b(died|dead|dying|die|wilt\w*|withered|lost|lose|loss|missed|miss|gone|broke|broken|weeds?|overgrown|decay\w*|too late|sorry|neglect\w*|abandon\w*|ruin\w*|forgot\w*|dry|dried|guilt\w*|waiting for you|wasted?|empty|fail\w*|left behind|away)\b",
        RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

    // Checklist 53: save fields that move with play and are not owned things.
    // replaysPaid resets per local day by design (v1 #80); it is a daily cap, not a possession.
    private static readonly string[] CursorFields =
    {
        "version", "mode", "garden", "solved", "round", "hintsUsed", "boardVariant",
        "lastSolvedVariant", "hintStep", "hintTile", "lastFruitTo", "variantSchedule",
        "history", "rotations", "previous", "replaysPaid",
    };

    private static void Expect(bool ok, string message)
    {
        assertions++;
        if (!ok) throw new Exception("DailyRhythmAuditTests failed: " + message);
    }

    public static int Run()
    {
        NoStreakState();
        SkippedDaysCostNothing();
        KindReturn();
        ReturnTextHasNoLoss();
        ReturnReasonsDoc();
        return assertions;
    }

    // ---- Checklist 52: no streak that can break; days played only grows.

    private static void NoStreakState()
    {
        var suspicious = new List<string>();
        var dayCounters = new List<string>();
        foreach (FieldInfo field in typeof(WorldSnapshot).GetFields(BindingFlags.Public | BindingFlags.Instance))
        {
            if (Regex.IsMatch(field.Name, "streak|consecutive|combo|chain|strike|missed|lost|expir", RegexOptions.IgnoreCase))
                suspicious.Add(field.Name);
            // payDay is a yyyymmdd date stamp, not a days-played total.
            if (field.Name != "payDay" && Regex.IsMatch(field.Name, "day", RegexOptions.IgnoreCase))
                dayCounters.Add(field.Name);
        }
        Expect(suspicious.Count == 0, "no streak-like save field: " + string.Join(",", suspicious));
        // Checklist 52: the only day fields are the days-played total and its date stamp (batch 5).
        dayCounters.Sort(StringComparer.Ordinal);
        Expect(string.Join(",", dayCounters) == "daysPlayed,lastPlayedDay",
            "the only day fields are the days-played total and its stamp: " + string.Join(",", dayCounters));
        // The total only grows across a 3-day gap and never resets (DaysPlayedTests covers the details).
        WorldModel played = WorldModel.New(PlayMode.Living);
        Expect(played.MarkPlayed(20261201) && played.MarkPlayed(20261202) && played.MarkPlayed(20261206) &&
               played.DaysPlayed == 3, "a 3-day gap still counts every day played and resets nothing");
    }

    private static void SkippedDaysCostNothing()
    {
        DateTime day0 = new DateTime(2026, 11, 20);
        DateTime day1 = day0.AddDays(1);
        DateTime day5 = day0.AddDays(5); // days 2, 3 and 4 are skipped
        int tier = TreePuzzles.TierFor(0);

        WorldModel world = LivingGarden(7);
        Expect(world.SolveTree(0, Stamp(day0)) == WorldModel.TreeReward(tier, true), "first solve pays in full");
        world.SolveTree(0, Stamp(day0));
        world.SolveTree(0, Stamp(day1));
        WorldSnapshot beforeGap = world.Export();
        int keyBefore = SeasonalYields.Key(world);

        ReadEverythingOnReturn(world, day1, day5);

        Expect(SeasonalYields.Key(world) == keyBefore, "the season does not move with the calendar");
        WorldSnapshot afterReads = world.Export();
        Expect(Dump(beforeGap) == Dump(afterReads), "reading the return changes no saved state");
        ExpectNoLoss(beforeGap, afterReads, "reading on day 5 after a four-day gap");

        Expect(world.ReplayRewardOn(tier, Stamp(day5)) == WorldModel.TreeReward(tier, false),
            "a new day pays the full replay again");
        int seedsBefore = world.Seeds;
        int earned = world.SolveTree(0, Stamp(day5));
        Expect(earned > 0 && world.Seeds == seedsBefore + earned, "the return replay is credited");
        ExpectNoLoss(afterReads, world.Export(), "the first replay after the gap");

        // The diary (research builds only) records each opened date and never drops one on a gap.
        var diary = new ReturnDiary();
        Expect(diary.GrantConsent(day0), "diary consent");
        diary.RecordOpen(day1);
        Expect(diary.Days.Count == 2, "diary keeps day 0 and day 1");
        diary.RecordOpen(day5);
        Expect(diary.Days.Count == 3 && diary.Days[0] == day0, "a gap adds nothing and removes nothing from the diary");
    }

    private static void ReadEverythingOnReturn(WorldModel world, DateTime last, DateTime today)
    {
        Season season = GardenSeason.For(world);
        DayWeather.For(today, season, world.Garden);
        DayWeather.SinceLastVisit(last, today, season, world.Garden);
        world.ReplayRewardOn(TreePuzzles.TierFor(0), Stamp(today));
        GardenNote.For(world, false);
        GardenNote.NextTime(today, 19);
        Visitors.Welcomes(world, Visitor.Bees);
        Visitors.Out(Visitor.Bees, season, DayPhase.Day);
        HubRecall.Next(world.Choices.Length, world.Choices.Length, 0);
        NeighbourWish.For(world, 0);
        NeighbourNotice.Line(0, world);
        GardenCat.NapBy(world, DayPhase.Day);
    }

    // ---- Checklist 53: a return only reports gains or neutral events.

    private static void KindReturn()
    {
        DateTime last = new DateTime(2026, 9, 3);
        int[] gaps = { 1, 3, 14, 60 };
        Season[] seasons = { Season.Spring, Season.Summer, Season.Autumn, Season.Winter };
        int[] gardens = { 0, 7, 42, 999 };
        foreach (int gap in gaps)
        {
            foreach (Season season in seasons)
            {
                foreach (int garden in gardens)
                {
                    string line = DayWeather.SinceLastVisit(last, last.AddDays(gap), season, garden);
                    if (gap == 1)
                    {
                        Expect(line == null, "one day away says nothing");
                        continue;
                    }
                    Expect(line != null, gap + " days away has a return line");
                    Expect(line.EndsWith("The garden looked after itself."), "the garden is said to look after itself: " + line);
                    Expect(!line.Contains("!"), "no exclamation: " + line);
                    foreach (string token in Regex.Split(line.ToLowerInvariant(), "[^a-z]+"))
                        if (token.Length > 0)
                            Expect(KindWords.Contains(token), "only kind words in a return line, found '" + token + "' in: " + line);
                }
            }
        }
        Expect(DayWeather.SinceLastVisit(last, last.AddDays(60), Season.Winter, 7) ==
            "A good while has passed, with all sorts of weather. The garden looked after itself.",
            "a long absence is told as a good while, and nothing else");

        // The owned state after each return is at least what it was before it.
        foreach (int gap in gaps)
        {
            WorldModel world = LivingGarden(7);
            world.SolveTree(0, Stamp(last));
            WorldSnapshot before = world.Export();
            DateTime back = last.AddDays(gap);
            DayWeather.SinceLastVisit(last, back, GardenSeason.For(world), world.Garden);
            GardenNote.For(world, false);
            world.SolveTree(0, Stamp(back));
            ExpectNoLoss(before, world.Export(), gap + " days away, then the return");
        }
    }

    private static void ReturnTextHasNoLoss()
    {
        string core = Path.Combine(EvergroveRoot(), "Assets", "Scripts", "Core");
        var flagged = new List<string>();
        foreach (string name in ReturnTextFiles)
        {
            string text = File.ReadAllText(Path.Combine(core, name));
            foreach (string literal in Literals(text))
                if (LossWords.IsMatch(literal)) flagged.Add(literal);
        }
        foreach (string literal in flagged)
            Expect(Array.IndexOf(NeutralLiterals, literal) >= 0, "loss-like wording in return text: " + literal);
        foreach (string neutral in NeutralLiterals)
            Expect(flagged.Contains(neutral), "neutral allowance no longer used, remove it: " + neutral);
    }

    private static List<string> Literals(string text)
    {
        var found = new List<string>();
        foreach (string rawLine in text.Split('\n'))
        {
            string line = Regex.Replace(rawLine, "//.*$", "");
            foreach (Match match in Regex.Matches(line, @"""((?:[^""\\]|\\.)*)"""))
                found.Add(match.Groups[1].Value);
        }
        return found;
    }

    // ---- Checklist 60: every return reason is tied to garden or family and never punishes absence.

    private static void ReturnReasonsDoc()
    {
        string path = Path.Combine(EvergroveRoot(), "docs", "specs", "return-reasons-v1.md");
        Expect(File.Exists(path), "return-reasons-v1.md exists");
        var rows = new List<string[]>();
        foreach (string raw in File.ReadAllLines(path))
        {
            string line = raw.Trim();
            if (!line.StartsWith("|", StringComparison.Ordinal)) continue;
            string[] cells = line.Trim('|').Split('|');
            for (int i = 0; i < cells.Length; i++) cells[i] = cells[i].Trim();
            if (cells[0] == "Reason" || cells[0].StartsWith("---", StringComparison.Ordinal)) continue;
            Expect(cells.Length == 7, "row has seven columns: " + cells[0]);
            rows.Add(cells);
        }
        Expect(rows.Count >= ReasonNames.Length, "at least as many rows as reasons (" + rows.Count + ")");

        var names = new List<string>();
        var expiring = new List<string>();
        foreach (string[] row in rows)
        {
            names.Add(row[0]);
            Expect(row[4] == "yes", "ties to garden or family: " + row[0]);
            Expect(row[5] == "yes" || row[5] == "no", "expires cell is yes or no: " + row[0]);
            if (row[5] == "yes") expiring.Add(row[0]);
            MatchCollection refs = Regex.Matches(row[2], @"`(Assets/[^`]+?\.cs):([A-Za-z_]\w*)`");
            Expect(refs.Count > 0, "has a code reference: " + row[0]);
            foreach (Match reference in refs)
            {
                string file = Path.Combine(EvergroveRoot(), reference.Groups[1].Value);
                Expect(File.Exists(file), "referenced file exists: " + reference.Groups[1].Value);
                Expect(File.ReadAllText(file).Contains(reference.Groups[2].Value),
                    "referenced symbol exists: " + reference.Groups[2].Value + " in " + reference.Groups[1].Value);
            }
        }
        foreach (string reason in ReasonNames)
            Expect(names.Contains(reason), "reason still documented: " + reason);
        Expect(new HashSet<string>(names).Count == names.Count, "each reason appears once");

        var known = new List<string>(KnownViolations);
        known.Sort(StringComparer.Ordinal);
        expiring.Sort(StringComparer.Ordinal);
        Expect(string.Join("|", expiring) == string.Join("|", known),
            "rows that expire or punish absence are exactly the known violations: " + string.Join("|", expiring));
    }

    // ---- Helpers.

    // Walks up from the working directory and from the assembly's directory, so the test runs both from
    // games/evergrove/Tests and from the repository root (CI). Returns .../games/evergrove.
    private static string EvergroveRoot()
    {
        string[] starts = { Directory.GetCurrentDirectory(), AppContext.BaseDirectory };
        foreach (string start in starts)
        {
            DirectoryInfo current = new DirectoryInfo(start);
            while (current != null)
            {
                string nested = Path.Combine(current.FullName, "games", "evergrove");
                if (Directory.Exists(Path.Combine(nested, "Assets", "Scripts", "Core"))) return nested;
                if (Directory.Exists(Path.Combine(current.FullName, "Assets", "Scripts", "Core"))) return current.FullName;
                current = current.Parent;
            }
        }
        throw new Exception("Could not find games/evergrove from " + Directory.GetCurrentDirectory());
    }

    private static int Stamp(DateTime day) { return day.Year * 10000 + day.Month * 100 + day.Day; }

    private static string Dump(WorldSnapshot snapshot)
    {
        return JsonSerializer.Serialize(snapshot, new JsonSerializerOptions { IncludeFields = true });
    }

    private static void SolveBoard(WorldModel model)
    {
        Puzzle board = model.CurrentPuzzle;
        for (int index = 0; index < board.CellCount; index++)
        {
            for (int turns = 0; turns < 8 && model.Rotations[index] != board.Solution[index]; turns++)
                model.Rotate(index);
        }
        Expect(model.Solved, "board solved by rotation");
    }

    private static WorldModel LivingGarden(int garden)
    {
        WorldModel world = WorldModel.New(PlayMode.Living, garden);
        SolveBoard(world);
        Expect(world.Choose(VillageChoice.Flowers), "first village choice recorded");
        return world;
    }

    // Every saved counter that is an owned thing must not drop across the call.
    private static void ExpectNoLoss(WorldSnapshot before, WorldSnapshot after, string when)
    {
        foreach (FieldInfo field in typeof(WorldSnapshot).GetFields(BindingFlags.Public | BindingFlags.Instance))
        {
            if (Array.IndexOf(CursorFields, field.Name) >= 0) continue;
            object a = field.GetValue(before);
            object b = field.GetValue(after);
            if (field.FieldType == typeof(int))
                Expect((int)b >= (int)a, when + ": " + field.Name + " did not drop");
            else if (field.FieldType == typeof(int[]) || field.FieldType == typeof(string[]))
                Expect(((Array)b).Length >= ((Array)a).Length, when + ": " + field.Name + " did not shrink");
        }
    }
}
