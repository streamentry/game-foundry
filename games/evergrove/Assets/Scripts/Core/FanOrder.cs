using System;
using System.Collections.Generic;

namespace Evergrove.Core
{
    /// <summary>
    /// Checklist 15, the menu remembers: the fan opens with the token used last nearest the thumb,
    /// that is first in the column, right under the corner button just tapped. Every other token keeps
    /// its usual order, so the fan never reshuffles beyond that one move. Pure: names in, names out.
    /// </summary>
    public static class FanOrder
    {
        /// <summary>
        /// The tokens in fan order: <paramref name="lastUsed"/> first when it is one of them, then the
        /// rest as given. A missing, blank or unknown last-used name leaves the usual order. Never
        /// changes the input; duplicates are kept as given.
        /// </summary>
        public static List<string> Arrange(IList<string> usual, string lastUsed)
        {
            var order = new List<string>();
            if (usual == null) return order;
            int first = string.IsNullOrEmpty(lastUsed) ? -1 : IndexOf(usual, lastUsed);
            if (first >= 0) order.Add(usual[first]);
            for (int i = 0; i < usual.Count; i++)
                if (i != first) order.Add(usual[i]);
            return order;
        }

        /// <summary>How far down the column a token sits (0 = first, under the corner button), or -1.</summary>
        public static int Place(IList<string> usual, string lastUsed, string token)
        {
            return IndexOf(Arrange(usual, lastUsed), token);
        }

        private static int IndexOf(IList<string> names, string name)
        {
            for (int i = 0; i < names.Count; i++)
                if (string.Equals(names[i], name, StringComparison.Ordinal)) return i;
            return -1;
        }
    }
}
