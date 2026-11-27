using System;
using System.Collections.Generic;

namespace Evergrove.Core
{
    /// <summary>Where something waiting for you lives on the home screen.</summary>
    public enum Attention { None, Letterbox, Basket, Play }

    /// <summary>The cottage card's lines about what's waiting (I299), in an order the ranker decides.</summary>
    public enum CottageItem { Letters, Cuttings, Waiting, Yield, Wish }

    /// <summary>
    /// Smart companion SC05: one ranker, many uses. The same facts and weights that choose what a
    /// neighbour says also choose where the home arrow points, the order of the cottage card's
    /// lines, and the one soft mark on the home screen. Content waiting only: never a count,
    /// never a nag. Pure and deterministic; memory is not used, so the screen stays steady while
    /// the neighbours move on to new things to say.
    /// </summary>
    public static class CompanionAttention
    {
        /// <summary>The thing the home arrow plays: a nursery plant (Bought) or a grown thing (Thing).</summary>
        public readonly struct Target
        {
            public readonly int Bought, Thing;
            public Target(int bought, int thing) { Bought = bought; Thing = thing; }
            public bool None { get { return Bought < 0 && Thing < 0; } }
        }

        /// <summary>
        /// I351 by the ranker: after the story, the arrow plays the waiting thing that matters most
        /// (help from family on an unsolved thing, then a new arrival); with neither, as before, the
        /// newest unsolved nursery plant, else the newest unsolved thing. Nothing before the story ends.
        /// </summary>
        public static Target ArrowTarget(WorldModel world, IReadOnlyList<Fact> facts, int day)
        {
            if (world == null || !world.Completed || world.Mode != PlayMode.Living) return new Target(-1, -1);
            Fact best = Top(facts, day, world.Garden, f =>
                (f.Kind == FactKind.HelpedBy && f.IntSlotOr("item", -1) >= 0) ||
                (f.Kind == FactKind.NewArrival && f.IntSlotOr("bought", -1) >= 0));
            if (best != null)
                return best.Kind == FactKind.HelpedBy ? new Target(-1, best.IntSlotOr("item", -1)) : new Target(best.IntSlotOr("bought", -1), -1);
            for (int k = world.BoughtCount - 1; k >= 0; k--) if (!world.BoughtSolved(k)) return new Target(k, -1);
            for (int i = world.Choices.Length - 1; i >= 0; i--) if (!world.TreeSolved(i)) return new Target(-1, i);
            return new Target(-1, -1);
        }

        /// <summary>The one home-screen place to mark softly: where the top waiting fact lives, or none.</summary>
        public static Attention Mark(IReadOnlyList<Fact> facts, int day, int garden)
        {
            Fact top = Top(facts, day, garden, f => f.Waiting);
            if (top == null) return Attention.None;
            switch (top.Kind)
            {
                case FactKind.LetterWaiting: return Attention.Letterbox;
                case FactKind.GiftWaiting: return Attention.Basket;
                case FactKind.HelpedBy: return Attention.Play;
                default: return Attention.None;
            }
        }

        /// <summary>
        /// The order of the cottage card's waiting lines: each line weighs what its facts weigh
        /// (a letter or cutting waiting first), ties broken by the day's hash. Only the lines given
        /// are returned; the card's fixed lines (name, season, weather, days) stay where they are.
        /// </summary>
        public static List<CottageItem> CottageOrder(IEnumerable<CottageItem> present, IReadOnlyList<Fact> facts, int day, int garden)
        {
            var items = new List<CottageItem>(present ?? Array.Empty<CottageItem>());
            items.Sort((a, b) =>
            {
                int wa = Weight(a, facts), wb = Weight(b, facts);
                if (wa != wb) return wb.CompareTo(wa);
                return CompanionRanker.Hash(garden, day, "cottage:" + a).CompareTo(CompanionRanker.Hash(garden, day, "cottage:" + b));
            });
            return items;
        }

        private static int Weight(CottageItem item, IReadOnlyList<Fact> facts)
        {
            switch (item)
            {
                case CottageItem.Letters: return Max(facts, FactKind.LetterWaiting, CompanionFacts.WaitingWeight);
                case CottageItem.Cuttings: return Max(facts, FactKind.GiftWaiting, CompanionFacts.WaitingWeight);
                case CottageItem.Waiting:
                    return Math.Max(Max(facts, FactKind.HelpedBy, 0), Max(facts, FactKind.NewArrival, CompanionFacts.OwnDoingWeight));
                case CottageItem.Yield: return CompanionFacts.RareWeight;
                default: return Max(facts, FactKind.WishProgress, CompanionFacts.OwnDoingWeight);
            }
        }

        private static int Max(IReadOnlyList<Fact> facts, FactKind kind, int otherwise)
        {
            int best = int.MinValue;
            if (facts != null)
                foreach (Fact f in facts)
                    if (f.Kind == kind) best = Math.Max(best, f.Weight);
            return best == int.MinValue ? otherwise : best;
        }

        // The heaviest fact passing the filter; ties by the hash of (garden, day, key).
        private static Fact Top(IReadOnlyList<Fact> facts, int day, int garden, Predicate<Fact> which)
        {
            Fact best = null;
            if (facts == null) return null;
            foreach (Fact f in facts)
            {
                if (!which(f)) continue;
                if (best == null || f.Weight > best.Weight ||
                    (f.Weight == best.Weight && CompanionRanker.Hash(garden, day, f.Key) < CompanionRanker.Hash(garden, day, best.Key)))
                    best = f;
            }
            return best;
        }
    }
}
