using System;
using System.Collections.Generic;

namespace Evergrove.Core
{
    /// <summary>What one line of the river map is: an open stretch of river, or a relative known from this device.</summary>
    public enum RiverMapKind { Stretch, Family }

    /// <summary>One line on the river map. Family lines carry no stretch (Reach is -1).</summary>
    public sealed class RiverMapRow
    {
        public readonly RiverMapKind Kind;
        public readonly int Reach;   // 0 is home's stretch; -1 for a family line
        public readonly string Name;
        public readonly bool Current; // the stretch the garden's view is on

        public RiverMapRow(RiverMapKind kind, int reach, string name, bool current)
        {
            Kind = kind;
            Reach = reach;
            Name = name;
            Current = current;
        }
    }

    /// <summary>
    /// Checklist 45, the river map: a painted scroll of the open stretches, one tap from the menu.
    /// Each open stretch is a line named by RiverNames; the one the view is on is marked in words as
    /// well as by its check. Family names come only from what this device has (letters, help, lanterns),
    /// listed under the river's mouth, because nothing says where a relative lives. Pure: no UI types.
    /// </summary>
    public static class RiverMap
    {
        /// <summary>The menu entry's words (tap one).</summary>
        public const string MenuLabel = "River map";

        /// <summary>Most family names listed, so the scroll stays short however many letters have come.</summary>
        public const int MaxFamilyNames = 12;

        /// <summary>
        /// The stretches open now, in order from home's stretch, then the family names (deduplicated,
        /// trimmed, capped). Family names are added only when familyNames is given; null means none
        /// are shown (family features not offered yet). currentReach marks one open stretch; any other
        /// value marks none.
        /// </summary>
        public static List<RiverMapRow> Rows(int openReaches, int currentReach, string[] familyNames)
        {
            int open = Math.Min(Math.Max(0, openReaches), GardenMap.SafetyReachBound);
            var rows = new List<RiverMapRow>(open + (familyNames == null ? 0 : MaxFamilyNames));
            for (int reach = 0; reach < open; reach++)
                rows.Add(new RiverMapRow(RiverMapKind.Stretch, reach, RiverNames.For(reach), reach == currentReach));
            if (familyNames == null) return rows;

            var seen = new HashSet<string>(StringComparer.Ordinal);
            int family = 0;
            foreach (string given in familyNames)
            {
                if (family >= MaxFamilyNames) break;
                string name = given == null ? "" : given.Trim();
                if (name.Length == 0 || !seen.Add(name)) continue;
                rows.Add(new RiverMapRow(RiverMapKind.Family, -1, name, false));
                family++;
            }
            return rows;
        }

        /// <summary>
        /// The relatives this device knows: senders of family letters, helpers from family, and lantern
        /// senders, in that order, with village neighbours (Mira, Orin, Fen) left out. Nothing is sent or looked up.
        /// </summary>
        public static string[] FamilyNamesFrom(string[] letterFrom, string[] helpedFrom, string[] lanterns)
        {
            var names = new List<string>();
            AddFamily(names, letterFrom);
            AddFamily(names, helpedFrom);
            AddFamily(names, lanterns);
            return names.ToArray();
        }

        private static void AddFamily(List<string> names, string[] senders)
        {
            if (senders == null) return;
            foreach (string sender in senders)
                if (!string.IsNullOrWhiteSpace(sender) && !FamilyOffer.IsNeighbour(sender.Trim()))
                    names.Add(sender);
        }
    }
}
