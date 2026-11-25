using System;
using System.Collections.Generic;
using System.Text;

namespace Evergrove.Core
{
    /// <summary>
    /// Which garden items a player has ever grown or built, across all gardens
    /// (E2, I174). Stored as a short text of art keys ("build-03,tree-07") so it
    /// survives starting a new garden. Pure, order-independent, tolerant of junk.
    /// Never a checklist with rewards: it only remembers what you made.
    /// </summary>
    public sealed class Almanac
    {
        private readonly HashSet<string> found = new HashSet<string>(StringComparer.Ordinal);

        public static Almanac Parse(string saved)
        {
            var almanac = new Almanac();
            if (string.IsNullOrEmpty(saved)) return almanac;
            foreach (string part in saved.Split(','))
            {
                string key = part.Trim();
                if (IsItemKey(key)) almanac.found.Add(key);
            }
            return almanac;
        }

        /// <summary>True only for keys the catalogue actually has (tree-00..49, flower-00..49, build-00..99).</summary>
        public static bool IsItemKey(string key)
        {
            if (string.IsNullOrEmpty(key)) return false;
            int dash = key.LastIndexOf('-');
            if (dash <= 0 || dash != key.Length - 3) return false;
            if (!int.TryParse(key.Substring(dash + 1), out int index) || index < 0) return false;
            string family = key.Substring(0, dash);
            VillageChoice kind;
            if (family == "tree") kind = VillageChoice.ShadeTree;
            else if (family == "flower") kind = VillageChoice.Flowers;
            else if (family == "build") kind = VillageChoice.Bridge;
            else return false;
            return index < GardenCatalog.CountOf(kind);
        }

        /// <summary>Adds an item; returns true when it is new to the almanac.</summary>
        public bool Add(GardenItem item)
        {
            return item != null && found.Add(item.Art);
        }

        public bool Has(GardenItem item) { return item != null && found.Contains(item.Art); }

        public int Count { get { return found.Count; } }

        public int CountOf(VillageChoice family)
        {
            int count = 0;
            int total = GardenCatalog.CountOf(family);
            for (int i = 0; i < total; i++) if (found.Contains(GardenCatalog.Get(family, i).Art)) count++;
            return count;
        }

        /// <summary>Stable text form: keys sorted, comma-separated.</summary>
        public string Serialize()
        {
            var keys = new List<string>(found);
            keys.Sort(StringComparer.Ordinal);
            var text = new StringBuilder();
            for (int i = 0; i < keys.Count; i++)
            {
                if (i > 0) text.Append(',');
                text.Append(keys[i]);
            }
            return text.ToString();
        }
    }
}
