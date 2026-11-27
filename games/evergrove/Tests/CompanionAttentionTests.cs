using System;
using System.Collections.Generic;
using System.Linq;
using Evergrove.Core;

/// <summary>
/// Smart companion SC05: one ranker, many uses. The home arrow plays what's waiting that matters
/// most; the cottage card's waiting lines come in the ranker's order; one soft mark shows where
/// the top waiting thing lives. Never a count, never a nag; deterministic.
/// </summary>
internal static class CompanionAttentionTests
{
    private static int assertions;

    private static void Expect(bool ok, string message)
    {
        assertions++;
        if (!ok) throw new Exception("Companion attention test failed: " + message);
    }

    private static Fact F(FactKind kind, int keeper, string key, int weight, bool waiting, params string[] slots)
    {
        var map = new Dictionary<string, string>();
        for (int i = 0; i + 1 < slots.Length; i += 2) map[slots[i]] = slots[i + 1];
        return new Fact(kind, keeper, key, map, weight, waiting, false, 0);
    }

    public static int Run()
    {
        assertions = 0;
        Arrow();
        Mark();
        Cottage();
        return assertions;
    }

    private static void Arrow()
    {
        var today = new DateTime(2026, 11, 27);
        int day = CompanionContext.DayNumberOf(today);
        WorldModel growing = CompanionFactsTests.Grown(5);
        Expect(CompanionAttention.ArrowTarget(growing, CompanionFacts.For(growing, new CompanionContext(today)), day).None,
            "during the story the arrow keeps its old job (no target)");

        // A finished garden, nothing tended: as before, the newest unsolved thing.
        WorldModel done = CompanionFactsTests.Grown(GardenCatalog.ChaptersPerGarden);
        var target = CompanionAttention.ArrowTarget(done, CompanionFacts.For(done, new CompanionContext(today)), day);
        Expect(target.Thing == done.Choices.Length - 1 && target.Bought < 0, "no waiting facts: the newest unsolved thing, as I351 did");

        // Help from family on an unsolved thing outranks everything else the arrow could play.
        WorldSnapshot s = done.Export();
        s.helped = new[] { 2 };
        s.helpedFrom = new[] { "Anh" };
        s.seeds = 500;
        Expect(WorldModel.TryRestore(s, out WorldModel helped), "restore with help");
        Expect(helped.Buy(1, 40), "a new arrival too");
        IReadOnlyList<Fact> facts = CompanionFacts.For(helped, new CompanionContext(today));
        target = CompanionAttention.ArrowTarget(helped, facts, day);
        Expect(target.Thing == 2 && target.Bought < 0, "help waiting on thing 2 comes first: " + target.Thing + "/" + target.Bought);

        // Without help, the new arrival.
        WorldSnapshot t = done.Export();
        t.seeds = 500;
        Expect(WorldModel.TryRestore(t, out WorldModel buyer) && buyer.Buy(2, 40), "a new arrival");
        target = CompanionAttention.ArrowTarget(buyer, CompanionFacts.For(buyer, new CompanionContext(today)), day);
        Expect(target.Bought == 0 && target.Thing < 0, "the new arrival is played");
        Expect(CompanionAttention.ArrowTarget(buyer, CompanionFacts.For(buyer, new CompanionContext(today)), day).Bought == target.Bought,
            "deterministic");

        // Everything tended: no target, the arrow just continues.
        WorldModel tended = CompanionFactsTests.Grown(GardenCatalog.ChaptersPerGarden);
        for (int i = 0; i < tended.Choices.Length; i++) tended.SolveTree(i);
        Expect(CompanionAttention.ArrowTarget(tended, CompanionFacts.For(tended, new CompanionContext(today)), day).None, "all tended: nothing to point at");
    }

    private static void Mark()
    {
        const int day = 9000, garden = 2;
        Expect(CompanionAttention.Mark(new List<Fact>(), day, garden) == Attention.None, "nothing waiting: no mark");
        var ambient = new List<Fact> { F(FactKind.WeatherToday, -1, "weather:Rainy", 5, false), F(FactKind.KindSolved, 0, "solved:Water:2", 15, false) };
        Expect(CompanionAttention.Mark(ambient, day, garden) == Attention.None, "news that isn't waiting: no mark");
        var gift = new List<Fact>(ambient) { F(FactKind.GiftWaiting, 0, "gift:3", 50, true, "thing", "apple tree") };
        Expect(CompanionAttention.Mark(gift, day, garden) == Attention.Basket, "a cutting waiting: the basket");
        var letter = new List<Fact> { F(FactKind.LetterWaiting, 1, "letter:Anh:1", 50, true, "name", "Anh") };
        Expect(CompanionAttention.Mark(letter, day, garden) == Attention.Letterbox, "a letter waiting: the letterbox");
        var both = new List<Fact>(gift) { letter[0] };
        Attention one = CompanionAttention.Mark(both, day, garden);
        Expect(one == Attention.Basket || one == Attention.Letterbox, "both waiting: exactly one place");
        Expect(CompanionAttention.Mark(both, day, garden) == one, "deterministic");
        var seen = new HashSet<Attention>();
        for (int d = 0; d < 30; d++) seen.Add(CompanionAttention.Mark(both, day + d, garden));
        Expect(seen.Count == 2, "equal weights take turns across days");
        var help = new List<Fact> { F(FactKind.HelpedBy, 2, "helped:4", 50, true, "item", "4", "name", "Anh") };
        Expect(CompanionAttention.Mark(help, day, garden) == Attention.Play, "family help waiting: the play arrow");
    }

    private static void Cottage()
    {
        const int day = 9000, garden = 5;
        CottageItem[] all = { CottageItem.Wish, CottageItem.Yield, CottageItem.Waiting, CottageItem.Cuttings, CottageItem.Letters };
        List<CottageItem> plain = CompanionAttention.CottageOrder(all, new List<Fact>(), day, garden);
        Expect(plain.Count == all.Length && all.All(plain.Contains), "the same lines, reordered, nothing added or dropped");
        int Index(List<CottageItem> order, CottageItem item) => order.IndexOf(item);
        Expect(Index(plain, CottageItem.Letters) < Index(plain, CottageItem.Yield) && Index(plain, CottageItem.Cuttings) < Index(plain, CottageItem.Yield),
            "what's waiting for you comes before what the season can still give");
        Expect(Index(plain, CottageItem.Yield) < Index(plain, CottageItem.Wish), "rare before ordinary");

        // Family help waiting weighs like a letter or a cutting, so it rises above the season's line.
        var help = new List<Fact> { F(FactKind.HelpedBy, 2, "helped:4", 50, true, "item", "4") };
        List<CottageItem> helped = CompanionAttention.CottageOrder(all, help, day, garden);
        Expect(Index(helped, CottageItem.Waiting) < Index(helped, CottageItem.Yield), "help waiting lifts the waiting-puzzles line above the season's");
        Expect(CompanionAttention.CottageOrder(all, help, day, garden).SequenceEqual(helped), "deterministic");
        Expect(CompanionAttention.CottageOrder(new[] { CottageItem.Yield }, help, day, garden).Single() == CottageItem.Yield, "only the lines given");
        Expect(CompanionAttention.CottageOrder(null, help, day, garden).Count == 0, "no lines, no order");
    }
}
