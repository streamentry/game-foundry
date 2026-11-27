using System;
using System.Collections.Generic;
using System.Globalization;

namespace Evergrove.Core
{
    /// <summary>
    /// One page of the almanac for one ISO week. Plain data: the lines are English for now; localize them
    /// where they are shown, not here, so this module stays a pure function of the save.
    /// </summary>
    public sealed class DiaryPage
    {
        public int Year;
        public int Week;
        /// <summary>For example "Week 48, 2026".</summary>
        public string Title;
        /// <summary>At most <see cref="WeeklyDiary.MaxLines"/> lines, each shorter than 80 characters.</summary>
        public string[] Lines;
    }

    /// <summary>
    /// The almanac's weekly diary page (checklist item 58). The page is written from what the save records,
    /// and the save keeps no event history, so a week is the difference between two snapshots.
    ///
    /// App contract (the wiring is not built yet): at the first launch of each ISO week, for the garden being
    /// played, the app stores <c>weekStart = world.Export()</c>. When it shows the almanac it calls
    /// <c>Write(weekStart, world.Export(), isoWeek, year)</c>. The caller works out the ISO week and year from
    /// its own calendar; this module never reads a clock, a random source or the current culture.
    ///
    /// Honesty limits, all visible in the code below:
    /// - Counts are net over the week. A plant moved or taken away nets against one planted; fruit cooked into
    ///   jam or sent in a parcel nets against the fruit gained, so only positive net movement is written.
    /// - Letters are counted by their text. Each letter in <c>now</c> that the week's start did not hold is one
    ///   arrival; the seed ledger is the only place a letter-count could be ambiguous, and it is not used here.
    /// - Seeds are split into earned and spent only when the ledger proves every change of the week: the start
    ///   ledger was not full (no line could have been dropped) and its lines are unchanged at the start of
    ///   <c>now</c>'s ledger, and the new lines add up to the seed change. Otherwise the page states the net change.
    /// - A week with no facts yields only <see cref="QuietLine"/>.
    ///
    /// Only gains and neutral facts are written. Nothing says what was missed, lost or late.
    /// </summary>
    public static class WeeklyDiary
    {
        /// <summary>Most lines on one page, the closing line included.</summary>
        public const int MaxLines = 8;

        /// <summary>The whole page for a week with no facts.</summary>
        public const string QuietLine = "A quiet week in the garden.";

        private static readonly string[] ClosingLines =
        {
            "The garden keeps its own time.",
            "Nothing here needs to be finished.",
            "Good things take their time to show.",
            "The village carries on at its own pace.",
            "Every small thing tended is written here.",
            "The water is patient, and so is the garden.",
        };

        /// <summary>A copy of the closing lines the page can end with.</summary>
        public static string[] Closings() { return (string[])ClosingLines.Clone(); }

        /// <summary>
        /// Writes the page for ISO week <paramref name="isoWeek"/> (1..53) of <paramref name="year"/>.
        /// Throws when the snapshots are not one garden's or are not valid saves; the caller owns the week start.
        /// Same inputs always give an identical page.
        /// </summary>
        public static DiaryPage Write(WorldSnapshot weekStart, WorldSnapshot now, int isoWeek, int year)
        {
            if (weekStart == null) throw new ArgumentNullException("weekStart");
            if (now == null) throw new ArgumentNullException("now");
            if (isoWeek < 1 || isoWeek > 53) throw new ArgumentOutOfRangeException("isoWeek");
            if (year < 1 || year > 9999) throw new ArgumentOutOfRangeException("year");
            if (weekStart.garden != now.garden) throw new ArgumentException("weekStart and now must be the same garden");
            WorldModel start;
            WorldModel end;
            if (!WorldModel.TryRestore(weekStart, out start)) throw new ArgumentException("not a valid save", "weekStart");
            if (!WorldModel.TryRestore(now, out end)) throw new ArgumentException("not a valid save", "now");

            var lines = new List<string>();
            AddIfSome(lines, GrowthLine(start, end));
            AddIfSome(lines, PlantingLine(start, end));
            AddIfSome(lines, PuzzlesLine(start, end));
            AddIfSome(lines, PairingsLine(start, end));
            AddIfSome(lines, LettersLine(start, end));
            AddIfSome(lines, HarvestLine(start, end));
            AddIfSome(lines, SeedsLine(start, end));
            if (lines.Count == 0) lines.Add(QuietLine);
            else lines.Add(ClosingFor(year, isoWeek, now.garden));

            return new DiaryPage
            {
                Year = year,
                Week = isoWeek,
                Title = "Week " + Num(isoWeek) + ", " + Num(year),
                Lines = lines.ToArray(),
            };
        }

        private static string GrowthLine(WorldModel start, WorldModel end)
        {
            int grown = Gain(end.Choices.Length, start.Choices.Length);
            return grown == 0 ? null : Counted(grown, "new thing", "new things") + " took root in the village.";
        }

        private static string PlantingLine(WorldModel start, WorldModel end)
        {
            int planted = Gain(end.PlantingCount + end.BoughtCount, start.PlantingCount + start.BoughtCount);
            return planted == 0 ? null : Counted(planted, "plant", "plants") + " went into the garden.";
        }

        private static string PuzzlesLine(WorldModel start, WorldModel end)
        {
            int solved = Gain(end.PuzzlesSolved, start.PuzzlesSolved);
            return solved == 0 ? null : Counted(solved, "puzzle", "puzzles") + " solved for the first time.";
        }

        private static string PairingsLine(WorldModel start, WorldModel end)
        {
            int found = CountBits(end.PairingsFound & ~start.PairingsFound);
            return found == 0 ? null : Counted(found, "good-neighbour pairing", "good-neighbour pairings") + " found.";
        }

        private static string LettersLine(WorldModel start, WorldModel end)
        {
            int arrived = NewLetters(start.Letters, end.Letters);
            return arrived == 0 ? null : Counted(arrived, "letter", "letters") + " arrived.";
        }

        // Fruit and jam are net: only a positive change is written, so cooking or giving never reads as a loss.
        private static string HarvestLine(WorldModel start, WorldModel end)
        {
            var parts = new List<string>();
            int fruit = Gain(end.Fruit, start.Fruit);
            int jam = Gain(end.Jam, start.Jam);
            if (fruit > 0) parts.Add("Fruit: " + Num(fruit) + " more in the basket.");
            if (jam > 0) parts.Add("Jam: " + Counted(jam, "jar", "jars") + " more in the pantry.");
            return parts.Count == 0 ? null : string.Join(" ", parts.ToArray());
        }

        private static string SeedsLine(WorldModel start, WorldModel end)
        {
            int earned;
            int spent;
            if (SeedFlow(start, end, out earned, out spent))
            {
                var parts = new List<string>();
                if (earned > 0) parts.Add("earned " + Num(earned));
                if (spent > 0) parts.Add("spent " + Num(spent));
                return parts.Count == 0 ? null : "Seeds: " + string.Join(", ", parts.ToArray()) + ".";
            }
            int change = end.Seeds - start.Seeds;
            if (change == 0) return null;
            return change > 0 ? "Seeds: " + Num(change) + " gained, net." : "Seeds: " + Num(-change) + " spent, net.";
        }

        // True only when the ledger accounts for every seed change of the week (see the honesty limits above).
        private static bool SeedFlow(WorldModel start, WorldModel end, out int earned, out int spent)
        {
            earned = 0;
            spent = 0;
            LedgerLine[] before = Oldest(start.Ledger);
            LedgerLine[] after = Oldest(end.Ledger);
            if (before.Length >= WorldModel.MaxLedger || after.Length < before.Length) return false;
            for (int i = 0; i < before.Length; i++)
                if (before[i].Reason != after[i].Reason || before[i].Delta != after[i].Delta) return false;
            int sum = 0;
            for (int i = before.Length; i < after.Length; i++)
            {
                int delta = after[i].Delta;
                sum += delta;
                if (delta > 0) earned += delta;
                else spent -= delta;
            }
            return sum == end.Seeds - start.Seeds;
        }

        // Letters are compared by text as a multiset: each kept letter of the start can match one arrival.
        private static int NewLetters(string[] before, string[] after)
        {
            var kept = new Dictionary<string, int>(StringComparer.Ordinal);
            foreach (string text in before)
            {
                int count;
                kept.TryGetValue(text, out count);
                kept[text] = count + 1;
            }
            int fresh = 0;
            foreach (string text in after)
            {
                int count;
                if (kept.TryGetValue(text, out count) && count > 0) kept[text] = count - 1;
                else fresh++;
            }
            return fresh;
        }

        // The closing line is a pure function of (year, week, garden): FNV-1a over an ordinal key.
        private static string ClosingFor(int year, int isoWeek, int garden)
        {
            string key = Num(year) + ":" + Num(isoWeek) + ":" + Num(garden);
            return ClosingLines[(int)(Fnv1a(key) % (uint)ClosingLines.Length)];
        }

        private static uint Fnv1a(string text)
        {
            unchecked
            {
                uint hash = 2166136261;
                foreach (char c in text)
                {
                    hash ^= c;
                    hash *= 16777619;
                }
                return hash;
            }
        }

        private static LedgerLine[] Oldest(LedgerLine[] newestFirst)
        {
            var lines = (LedgerLine[])newestFirst.Clone();
            Array.Reverse(lines);
            return lines;
        }

        private static int CountBits(int mask)
        {
            int count = 0;
            for (int bits = mask; bits != 0; bits &= bits - 1) count++;
            return count;
        }

        private static int Gain(int now, int before) { return now > before ? now - before : 0; }

        private static void AddIfSome(List<string> lines, string line)
        {
            if (line != null) lines.Add(line);
        }

        private static string Counted(int n, string one, string many)
        {
            return Num(n) + " " + (n == 1 ? one : many);
        }

        private static string Num(int n) { return n.ToString(CultureInfo.InvariantCulture); }
    }
}
