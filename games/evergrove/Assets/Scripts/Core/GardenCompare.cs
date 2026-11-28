using System;
using System.Collections.Generic;
using System.Globalization;

namespace Evergrove.Core
{
    /// <summary>
    /// The plain facts about a garden, shown on the restore card so the player can compare the garden
    /// on this phone with the garden in a file before confirming. Pure: reads a world, never changes it.
    /// </summary>
    public static class GardenCompare
    {
        public struct Summary
        {
            public string Name;
            public int Streams;
            public int Things;
            public int Days;
        }

        public const string SmallerLine = "The file's garden is smaller than this phone's.";

        private const string Separator = " · ";

        /// <summary>The facts of one world; a null world gives an empty summary.</summary>
        public static Summary Of(WorldModel world)
        {
            if (world == null)
                return new Summary { Name = "", Streams = 0, Things = 0, Days = 0 };
            return new Summary
            {
                Name = world.GardenName ?? "",
                Streams = world.Choices.Length,
                Things = world.PlantingCount + world.BoughtCount,
                Days = world.DaysPlayed,
            };
        }

        /// <summary>One line, for example "On this phone: Willowbank · 7 streams · 23 things · 12 days".</summary>
        public static string Line(string label, Summary s)
        {
            var parts = new List<string>();
            if (!string.IsNullOrWhiteSpace(s.Name)) parts.Add(s.Name);
            parts.Add(Count(s.Streams, "stream", "streams"));
            parts.Add(Count(s.Things, "thing", "things"));
            parts.Add(Count(s.Days, "day", "days"));
            return label + ": " + string.Join(Separator, parts);
        }

        /// <summary>
        /// The restore card lines: this phone, then the file. A third line warns when the file's garden
        /// has fewer streams or fewer things than this phone's.
        /// </summary>
        public static string[] Lines(WorldModel current, WorldModel incoming)
        {
            Summary now = Of(current);
            Summary file = Of(incoming);
            var lines = new List<string> { Line("On this phone", now), Line("In the file", file) };
            if (file.Streams < now.Streams || file.Things < now.Things)
                lines.Add(SmallerLine);
            return lines.ToArray();
        }

        private static string Count(int n, string singular, string plural)
        {
            int value = Math.Max(0, n);
            return value == 1
                ? "1 " + singular
                : value.ToString(CultureInfo.InvariantCulture) + " " + plural;
        }
    }
}
