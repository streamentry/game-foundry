using System;
using System.Collections.Generic;

namespace Evergrove.Core
{
    /// <summary>
    /// One chapter of the year: the kind of thing a season turn brings for the first time. It names
    /// existing content only: the season's gift from <see cref="SeasonalYields"/>, with the good
    /// neighbour it needs, so the chapter is usable the turn it opens.
    /// </summary>
    public sealed class SeasonUnlock
    {
        public SeasonUnlock(int turn, string brings)
        {
            Turn = turn;
            Brings = brings;
            Season = GardenSeason.ForStory(turn);
            Gives = SeasonalYields.Gives(Season);
            Needs = SeasonalYields.Needs(Season);
        }

        /// <summary>The season turn this chapter opens on: 0 spring, 1 summer, 2 autumn, 3 winter.</summary>
        public int Turn { get; }
        public Season Season { get; }
        /// <summary>Plain words for the almanac: "spring can bring &lt;Brings&gt;".</summary>
        public string Brings { get; }
        public SeasonYield Gives { get; }
        /// <summary>The good neighbour the season's gift needs standing in the garden; null for winter.</summary>
        public Pairing? Needs { get; }
    }

    /// <summary>
    /// I312 (idea 38): "the year has a shape". Each of the first year's four season turns opens one
    /// new chapter, and the chapters are the seasonal gifts that already exist (I388 to I391), so no
    /// new content is added. The year is the story's four seasons, spring to winter, by progress.
    /// Chapters never close: <see cref="Unlocked"/> only grows as the season turns.
    /// After the first year <see cref="For"/> names no new chapter; see the report in docs/improvements.
    /// Deterministic and derived from the save (the season key), never stored.
    /// </summary>
    public static class SeasonUnlocks
    {
        public const int FirstYearTurns = 4;

        private static readonly SeasonUnlock[] FirstYear =
        {
            new SeasonUnlock(0, "a seedling from the flower bed"),
            new SeasonUnlock(1, "seeds from the grove"),
            new SeasonUnlock(2, "fruit from the orchard"),
            new SeasonUnlock(3, "a jar of jam from neighbours you gave fruit to"),
        };

        /// <summary>The chapter a season turn opens, or null outside the first year.</summary>
        public static SeasonUnlock For(int seasonTurn)
        {
            if (seasonTurn < 0 || seasonTurn >= FirstYearTurns) return null;
            return FirstYear[seasonTurn];
        }

        /// <summary>The chapters open so far, in season order. Empty when there is no living garden.</summary>
        public static SeasonUnlock[] Unlocked(WorldModel world)
        {
            int key = SeasonalYields.Key(world);
            if (key < 0) return Array.Empty<SeasonUnlock>();
            int count = Math.Min(key + 1, FirstYearTurns);
            var open = new SeasonUnlock[count];
            Array.Copy(FirstYear, open, count);
            return open;
        }

        /// <summary>The almanac's line, "This year: spring can bring ..., summer can bring ...", or null before any chapter.</summary>
        public static string YearLine(WorldModel world)
        {
            SeasonUnlock[] open = Unlocked(world);
            if (open.Length == 0) return null;
            var parts = new List<string>(open.Length);
            // "can bring", not "brought": a chapter is open, but its gift still needs its pairing standing
            // (a tour showed "spring brought …" beside "Nothing paired up yet").
            foreach (SeasonUnlock chapter in open)
                parts.Add(chapter.Season.ToString().ToLowerInvariant() + " can bring " + chapter.Brings);
            return "This year: " + string.Join(", ", parts.ToArray()) + ".";
        }
    }
}
