using System;
using System.IO;
using System.Text.RegularExpressions;
using Evergrove.Core;

// Checklist 81: "Neighbours remember what you said." Reply chips in letters (three icons, not free text)
// are echoed by the neighbour within three days. Pure rules are tested here; the Unity side (the chip row,
// the echo in NeighborSays, the tour shots) is checked by source audit, as the other UI rules are.
internal static class ReplyChipsTests
{
    private static int assertions;

    private static void Expect(bool ok, string message)
    {
        assertions++;
        if (!ok) throw new Exception("ReplyChipsTests failed: " + message);
    }

    // The same loss-word rule as DayRhythmTests: no echo may carry guilt, loss or hurry.
    private static readonly Regex LossWords = new Regex(
        @"\b(died|dead|dying|die|wilt\w*|withered|lost|lose|loss|missed|miss|gone|broke|broken|weeds?|overgrown|decay\w*|too late|sorry|neglect\w*|abandon\w*|ruin\w*|forgot\w*|dry|dried|guilt\w*|waiting for you|wasted?|empty|fail\w*|left behind|away|hurry|hurry up|late)\b",
        RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

    private const int Mira = 0, Orin = 1, Fen = 2;

    public static int Run()
    {
        assertions = 0;
        ThreeChipsWithLabelsAndIcons();
        EchoDueOnDaysZeroToThree();
        EchoOnlyOnce();
        NewReplyReplacesOld();
        PerVillagerIsolation();
        SerialiseRoundTrip();
        GarbageInputIsEmpty();
        EchoLinesAreGentle();
        BadArgumentsChangeNothing();
        LettersShowChipsOnlyForNeighbours();
        NeighborSaysEchoesBeforeArcRemark();
        EchoStaysForTheDay();
        TourSavesBothShots();
        return assertions;
    }

    private static void ThreeChipsWithLabelsAndIcons()
    {
        Expect(ReplyChips.Chips.Count == 3, "three reply chips");
        var labels = new System.Collections.Generic.HashSet<string>();
        var icons = new System.Collections.Generic.HashSet<string>();
        foreach (ReplyChip chip in ReplyChips.Chips)
        {
            string label = ReplyChips.Label(chip);
            Expect(label.Length > 0 && label.Length <= 24, "a short label for " + chip + ": " + label);
            Expect(!LossWords.IsMatch(label), "a label carries no loss or guilt: " + label);
            Expect(!label.Contains("!"), "a label has no exclamation: " + label);
            Expect(ReplyChips.Icon(chip).Length > 0, "an icon for " + chip);
            labels.Add(label);
            icons.Add(ReplyChips.Icon(chip));
        }
        Expect(labels.Count == 3 && icons.Count == 3, "each chip has its own label and icon");
        Expect(ReplyChips.Label(ReplyChip.Warm) == "That made me smile", "the warm chip says so");
        Expect(ReplyChips.Label(ReplyChip.Curious) == "Tell me more", "the curious chip asks for more");
        Expect(ReplyChips.Label(ReplyChip.Thanks) == "Thank you", "the thanks chip says thank you");
    }

    private static void EchoDueOnDaysZeroToThree()
    {
        ReplyMemory memory = ReplyChips.Choose(ReplyMemory.Empty, Mira, ReplyChip.Curious, 10);
        for (int day = 10; day <= 13; day++)
            Expect(ReplyChips.EchoDue(memory, Mira, day), "the echo is due on day " + (day - 10) + " after the reply");
        Expect(!ReplyChips.EchoDue(memory, Mira, 14), "no echo four days after the reply");
        Expect(!ReplyChips.EchoDue(memory, Mira, 9), "no echo before the reply");
        Expect(!ReplyChips.EchoDue(ReplyMemory.Empty, Mira, 10), "no reply, no echo");
    }

    private static void EchoOnlyOnce()
    {
        ReplyMemory memory = ReplyChips.Choose(ReplyMemory.Empty, Orin, ReplyChip.Thanks, 3);
        Expect(ReplyChips.EchoDue(memory, Orin, 4), "echo due the day after");
        ReplyMemory used = ReplyChips.MarkEchoed(memory, Orin);
        Expect(!ReplyChips.EchoDue(used, Orin, 4), "an echoed reply is not echoed again");
        Expect(!ReplyChips.EchoDue(used, Orin, 5), "an echoed reply stays used up");
        Expect(ReplyChips.MarkEchoed(used, Orin).Serialize() == used.Serialize(), "marking twice changes nothing");
        Expect(ReplyChips.Echo(Orin, ReplyChip.Thanks).Length > 0, "the echo line is there to speak once");
    }

    private static void NewReplyReplacesOld()
    {
        ReplyMemory first = ReplyChips.Choose(ReplyMemory.Empty, Fen, ReplyChip.Warm, 5);
        first = ReplyChips.MarkEchoed(first, Fen);
        ReplyMemory second = ReplyChips.Choose(first, Fen, ReplyChip.Thanks, 7);
        Expect(second.Count == 1, "one reply per neighbour, the new one replaces the old");
        Expect(second.TryGet(Fen, out ReplyNote note) && note.Chip == ReplyChip.Thanks && note.Day == 7 && !note.Echoed,
            "the new reply is kept unechoed");
        Expect(ReplyChips.EchoDue(second, Fen, 8), "a new reply is echoed again");
        Expect(first.TryGet(Fen, out ReplyNote old) && old.Chip == ReplyChip.Warm, "the earlier memory is unchanged (immutable)");
    }

    private static void PerVillagerIsolation()
    {
        ReplyMemory memory = ReplyChips.Choose(ReplyMemory.Empty, Mira, ReplyChip.Warm, 2);
        memory = ReplyChips.Choose(memory, Fen, ReplyChip.Curious, 2);
        Expect(memory.Count == 2, "two neighbours, two replies");
        memory = ReplyChips.MarkEchoed(memory, Mira);
        Expect(!ReplyChips.EchoDue(memory, Mira, 3), "Mira's echo is used up");
        Expect(ReplyChips.EchoDue(memory, Fen, 3), "Fen's echo is still due");
        Expect(!ReplyChips.EchoDue(memory, Orin, 3), "Orin never replied, so has no echo");
        ReplyMemory withOrin = ReplyChips.Choose(memory, Orin, ReplyChip.Thanks, 3);
        Expect(withOrin.TryGet(Fen, out ReplyNote fen) && fen.Chip == ReplyChip.Curious, "a reply to Orin leaves Fen's alone");
    }

    private static void SerialiseRoundTrip()
    {
        ReplyMemory memory = ReplyChips.Choose(ReplyMemory.Empty, Mira, ReplyChip.Warm, 12);
        memory = ReplyChips.Choose(memory, Orin, ReplyChip.Thanks, 13);
        memory = ReplyChips.MarkEchoed(memory, Orin);
        string text = memory.Serialize();
        ReplyMemory back = ReplyMemory.Parse(text);
        Expect(back.Count == 2, "both replies survive a round trip");
        Expect(back.Serialize() == text, "serialising twice gives the same text");
        Expect(back.TryGet(Mira, out ReplyNote mira) && mira.Chip == ReplyChip.Warm && mira.Day == 12 && !mira.Echoed,
            "Mira's reply comes back whole");
        Expect(back.TryGet(Orin, out ReplyNote orin) && orin.Chip == ReplyChip.Thanks && orin.Day == 13 && orin.Echoed,
            "Orin's echoed flag comes back");
        Expect(ReplyMemory.Empty.Serialize() == "", "no replies serialise to nothing");
    }

    private static void GarbageInputIsEmpty()
    {
        Expect(ReplyMemory.Parse(null).Count == 0, "null parses to nothing");
        Expect(ReplyMemory.Parse("").Count == 0, "empty text parses to nothing");
        Expect(ReplyMemory.Parse("not a reply at all").Count == 0, "plain words parse to nothing");
        Expect(ReplyMemory.Parse("9|Warm|3|0").Count == 0, "an unknown villager is skipped");
        Expect(ReplyMemory.Parse("0|Shout|3|0").Count == 0, "an unknown chip is skipped");
        Expect(ReplyMemory.Parse("0|Warm|-4|0").Count == 0, "a negative day is skipped");
        Expect(ReplyMemory.Parse("0|Warm|3|yes").Count == 0, "a bad echoed flag is skipped");
        Expect(ReplyMemory.Parse("0|Warm|x|0").Count == 0, "a non-number day is skipped");
        ReplyMemory mixed = ReplyMemory.Parse("junk\n1|Curious|4|1\n|||\n0|Warm|2|0");
        Expect(mixed.Count == 2, "good lines survive beside bad ones");
        Expect(mixed.TryGet(Orin, out ReplyNote orin) && orin.Echoed, "the good line beside junk is read");
        Expect(ReplyMemory.Parse("0|Warm|2|0\n0|Thanks|6|0").TryGet(Mira, out ReplyNote last) && last.Chip == ReplyChip.Thanks,
            "a later line for the same neighbour replaces the earlier one");
    }

    // Every echo: nine lines, short, warm, distinct, no loss or guilt words, never an exclamation.
    private static void EchoLinesAreGentle()
    {
        var seen = new System.Collections.Generic.HashSet<string>();
        for (int villager = 0; villager < ReplyChips.Villagers; villager++)
            foreach (ReplyChip chip in ReplyChips.Chips)
            {
                string line = ReplyChips.Echo(villager, chip);
                Expect(line.Length > 0, "an echo for villager " + villager + " and " + chip);
                Expect(!LossWords.IsMatch(line), "no loss-like words in echo: " + line);
                Expect(!line.Contains("!") && !line.Contains("you missed"), "no reproach in echo: " + line);
                int words = line.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries).Length;
                Expect(words < 20, "under twenty words: " + line);
                Expect(seen.Add(line), "each echo is its own line: " + line);
                // Honest companion: an echo answers the reply; it never promises a later letter or gift
                // the game has no way to send.
                Expect(!System.Text.RegularExpressions.Regex.IsMatch(line, @"\b(I'll|I will|soon|next (one|time|letter)|tomorrow)\b",
                    System.Text.RegularExpressions.RegexOptions.IgnoreCase), "no promise of things to come: " + line);
            }
        Expect(seen.Count == 9, "nine echo lines in all");
    }

    private static void BadArgumentsChangeNothing()
    {
        ReplyMemory memory = ReplyChips.Choose(ReplyMemory.Empty, Mira, ReplyChip.Warm, 1);
        Expect(ReplyChips.Choose(memory, 7, ReplyChip.Warm, 1).Serialize() == memory.Serialize(), "an unknown villager is ignored");
        Expect(ReplyChips.Choose(memory, Orin, ReplyChip.Warm, -1).Serialize() == memory.Serialize(), "a negative day is ignored");
        Expect(ReplyChips.Choose(null, Orin, ReplyChip.Warm, 1).Count == 1, "null memory starts empty");
        Expect(!ReplyChips.EchoDue(null, Mira, 1), "no memory, no echo due");
        Expect(ReplyChips.Echo(-1, ReplyChip.Warm) == "", "no echo for an unknown villager");
        Expect(ReplyChips.Echo(Mira, (ReplyChip)9) == "", "no echo for an unknown chip");
        Expect(ReplyChips.MarkEchoed(null, Mira).Count == 0, "marking null is empty");
    }

    private static void LettersShowChipsOnlyForNeighbours()
    {
        string letters = ReadEvergrove("Assets/Scripts/Unity/EvergroveApp.Letters.cs");
        Expect(letters.Contains("int replyTo = Array.IndexOf(VillagerNames, sender);"), "a letter's sender is matched to a neighbour");
        Expect(letters.Contains("if (replyTo >= 0 && world.Mode == PlayMode.Living) ReplyChipRow(paper.transform, replyTo);"),
            "reply chips show only for a neighbour's letter in a living garden");
        Expect(letters.IndexOf("ReplyChipRow(paper.transform, replyTo);", StringComparison.Ordinal) <
               letters.IndexOf("CenteredRowIn(paper.transform, \"Letter actions\")", StringComparison.Ordinal),
            "the reply chips sit just above the letter actions");
        Expect(letters.Contains("IconButton(replyChipRow, \"Reply chip \" + chip, ReplyChips.Label(chip), ReplyChips.Icon(chip),") &&
               letters.Contains("() => ChooseReplyChip(villager, pick), 56, MosseloraTheme.QuietAction"),
            "each chip is a 56 point quiet icon button with its label");
        Expect(letters.Contains("MosseloraArt.StateMark(token, hasReply && note.Chip == chip"), "the chosen chip shows a check mark, not colour alone");
        Expect(letters.Contains("SaveReplies(ReplyChips.Choose(ReplyMemoryNow(), villager, chip, ReplyDayNow()));"),
            "a tapped chip saves the reply and does nothing else");
        Expect(letters.Contains("private const string ReplyKeyPrefix = \"evergrove:reply:\";"), "replies are kept under the device key");
        Expect(!letters.Contains("world.Garden", StringComparison.Ordinal) || letters.Contains("ReplyKeyPrefix + world.Garden"),
            "replies are kept per garden");
    }

    private static void NeighborSaysEchoesBeforeArcRemark()
    {
        string neighbours = ReadEvergrove("Assets/Scripts/Unity/EvergroveApp.Neighbours.cs");
        int says = neighbours.IndexOf("private string NeighborSays(int villager)", StringComparison.Ordinal);
        Expect(says >= 0, "NeighborSays is found");
        string body = neighbours.Substring(says, Math.Min(900, neighbours.Length - says));
        int echo = body.IndexOf("ReplyEchoSaid(villager)", StringComparison.Ordinal);
        int arc = body.IndexOf("ArcRemark(villager)", StringComparison.Ordinal);
        Expect(echo > 0 && arc > 0 && echo < arc, "a due echo is spoken before an arc remark");
        Expect(body.IndexOf("if (echo != null) return echo;", StringComparison.Ordinal) > 0, "a due echo is returned");
        string letters = ReadEvergrove("Assets/Scripts/Unity/EvergroveApp.Letters.cs");
        Expect(letters.Contains("if (!ReplyChips.EchoDue(memory, villager, today)) return null;") &&
               letters.Contains("SaveReplies(ReplyChips.MarkEchoed(memory, villager));"),
            "the echo is checked and then marked used, saved");
    }

    // Speaking to a neighbour asks NeighborSays twice (the heard-line note, then the bubble); the answer
    // is kept for the rest of the day so the bubble shows it rather than a line after it was used up.
    private static void EchoStaysForTheDay()
    {
        string letters = ReadEvergrove("Assets/Scripts/Unity/EvergroveApp.Letters.cs");
        Expect(letters.Contains("if (replyEchoToday.TryGetValue(villager, out string said)) return said;"),
            "an answer already said today is said again, not used up");
        Expect(letters.IndexOf("replyEchoToday[villager] = line;", StringComparison.Ordinal) >
               letters.IndexOf("SaveReplies(ReplyChips.MarkEchoed(memory, villager));", StringComparison.Ordinal),
            "the answer is kept for the day when it is marked as echoed");
    }

    private static void TourSavesBothShots()
    {
        string tour = ReadEvergrove("Assets/Scripts/Unity/DevQaTour.Replies.cs");
        Expect(tour.StartsWith("#if UNITY_EDITOR || DEVELOPMENT_BUILD"), "the reply tour is guarded like the other tour parts");
        Expect(tour.Contains("private IEnumerator ReplyShots()"), "the tour has ReplyShots");
        Expect(tour.Contains("SavePng(\"21-reply-chips.png\")"), "the tour saves the letter with chips");
        Expect(tour.Contains("SavePng(\"21-reply-chosen.png\")"), "the tour saves the chosen chip with its check");
        Expect(tour.IndexOf("app.QaShowHub(false);", StringComparison.Ordinal) < tour.IndexOf("app.QaTalk(0);", StringComparison.Ordinal),
            "the echo is heard in the village view, where neighbours speak");
        Expect(tour.Contains("SavePng(\"21-reply-echo.png\")"), "the tour saves the echo");
        Expect(tour.IndexOf("SavePng(\"21-reply-chips.png\")", StringComparison.Ordinal) <
               tour.IndexOf("app.QaCloseLetter();", StringComparison.Ordinal), "the chips are shot before the letter closes");
        Expect(tour.IndexOf("app.QaChooseReply(", StringComparison.Ordinal) < tour.IndexOf("app.QaSetDay(1);", StringComparison.Ordinal),
            "a chip is chosen before the next day");
        Expect(tour.IndexOf("app.QaSetDay(1);", StringComparison.Ordinal) < tour.IndexOf("SavePng(\"21-reply-echo.png\")", StringComparison.Ordinal),
            "the echo is shot on the next day");
        Expect(ReadEvergrove("Assets/Scripts/Unity/DevQaTour.cs").Contains("yield return ReplyShots();"), "the tour runs the reply shots");
        string qa = ReadEvergrove("Assets/Scripts/Unity/EvergroveApp.ReplyQa.cs");
        Expect(qa.StartsWith("#if UNITY_EDITOR || DEVELOPMENT_BUILD"), "the reply QA hooks are dev-only");
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
