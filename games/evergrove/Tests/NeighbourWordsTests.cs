using System;
using System.Text.RegularExpressions;
using Evergrove.Core;

/// <summary>
/// Vision run 20261128-2001, idea 2: neighbour arrival and basket lines name every
/// catalogue item in correct English (no "a" before a vowel, no "a" before a plural or
/// mass name, no "it" for a plural in Fen's and Orin's arrival lines).
/// </summary>
internal static class NeighbourWordsTests
{
    private static int assertions;

    private static void Expect(bool ok, string message)
    {
        assertions++;
        if (!ok) throw new Exception("Neighbour words test failed: " + message);
    }

    // Mira keeps trees, Orin keeps buildings (VillageChoice.Bridge), Fen keeps flowers.
    private static VillageChoice FamilyOf(int villager)
    {
        return villager == 0 ? VillageChoice.ShadeTree : villager == 1 ? VillageChoice.Bridge : VillageChoice.Flowers;
    }

    // "a" or "A" as a whole word, followed by a space and a vowel letter.
    private static bool ArticleBeforeVowel(string line)
    {
        return Regex.IsMatch(line, @"\b[aA] [aeiouAEIOU]");
    }

    private static bool ArticleBefore(string line, string name)
    {
        return Regex.IsMatch(line, @"\b[aA] " + Regex.Escape(name) + @"\b");
    }

    private static bool WordIt(string line)
    {
        return Regex.IsMatch(line, @"\bit\b");
    }

    private static int IndexOfName(VillageChoice family, string name)
    {
        int count = GardenCatalog.CountOf(family);
        for (int i = 0; i < count; i++)
            if (GardenCatalog.Get(family, i).Name == name) return i;
        return -1;
    }

    private static void TestItemWords()
    {
        Expect(ItemWords.Plural("sunflowers") && ItemWords.Plural("cosmos"), "plural names end in s, cosmos included");
        Expect(!ItemWords.Plural("lotus") && !ItemWords.Plural("hibiscus"), "lotus and hibiscus are not plural");
        Expect(!ItemWords.Plural("oak") && !ItemWords.Plural(null) && !ItemWords.Plural(""), "singular and empty are not plural");
        Expect(!ItemWords.Plural("vine trellis") && !ItemWords.Plural("apple press"), "trellis and press are singular");
        Expect(NeighbourNotice.Arrival(1, "vine trellis") == "That new vine trellis: I would like to see how it is joined.", "Orin arrival trellis");
        Expect(NeighbourNotice.Waiting(1, "apple press") == "There's an apple press in your basket, sent with care. Shall we find it a place?",
            "Orin waiting apple press");
        Expect(ItemWords.Mass("lavender") && ItemWords.Mass("lily of the valley"), "listed mass names");
        Expect(!ItemWords.Mass("oak") && !ItemWords.Mass(null), "unlisted and null are not mass");
        Expect(ItemWords.WithArticle("oak") == "an oak", "vowel takes an");
        Expect(ItemWords.WithArticle("quince tree") == "a quince tree", "consonant takes a");
        Expect(ItemWords.WithArticle("Scots pine") == "a Scots pine", "capital consonant takes a");
        Expect(ItemWords.WithArticle("") == "" && ItemWords.WithArticle(null) == "", "empty article is safe");
    }

    private static void TestExactLines()
    {
        int oak = IndexOfName(VillageChoice.ShadeTree, "oak");
        int quince = IndexOfName(VillageChoice.ShadeTree, "quince tree");
        int sunflowers = IndexOfName(VillageChoice.Flowers, "sunflowers");
        int lavender = IndexOfName(VillageChoice.Flowers, "lavender");
        int beehives = IndexOfName(VillageChoice.Bridge, "beehives");
        int windmill = IndexOfName(VillageChoice.Bridge, "windmill");
        Expect(oak >= 0 && quince >= 0 && sunflowers >= 0 && lavender >= 0 && beehives >= 0 && windmill >= 0,
            "exact test names exist in the catalogue");

        Expect(NeighbourNotice.Arrival(0, "oak") == "An oak, new in the garden. Give it a season and it will shade the whole bank.",
            "Mira arrival oak");
        Expect(NeighbourNotice.Arrival(0, "quince tree") == "A quince tree, new in the garden. Give it a season and it will shade the whole bank.",
            "Mira arrival quince tree");
        Expect(NeighbourNotice.Waiting(0, "oak") == "Someone sent you an oak cutting. It's waiting in your basket for a good spot.",
            "Mira waiting oak");
        Expect(NeighbourNotice.Waiting(0, "quince tree") == "Someone sent you a quince tree cutting. It's waiting in your basket for a good spot.",
            "Mira waiting quince tree");
        Expect(NeighbourNotice.Arrival(2, "sunflowers") == "New sunflowers! The bees will find them by noon.", "Fen arrival sunflowers");
        Expect(NeighbourNotice.Arrival(2, "lavender") == "New lavender! The bees will find it by noon.", "Fen arrival lavender");
        Expect(NeighbourNotice.Waiting(2, "sunflowers") == "Cuttings of sunflowers came for you! They're waiting in your basket.",
            "Fen waiting sunflowers");
        Expect(NeighbourNotice.Arrival(1, "beehives") == "Those new beehives: I would like to see how they are joined.", "Orin arrival beehives");
        Expect(NeighbourNotice.Arrival(1, "windmill") == "That new windmill: I would like to see how it is joined.", "Orin arrival windmill");
        Expect(NeighbourNotice.Waiting(1, "beehives") == "There are beehives in your basket, sent with care. Shall we find them a place?",
            "Orin waiting beehives");
        Expect(NeighbourNotice.Waiting(1, "windmill") == "There's a windmill in your basket, sent with care. Shall we find it a place?",
            "Orin waiting windmill");
    }

    private static void TestEveryCatalogueItem()
    {
        for (int v = 0; v < 3; v++)
        {
            VillageChoice family = FamilyOf(v);
            int count = GardenCatalog.CountOf(family);
            for (int i = 0; i < count; i++)
            {
                string name = GardenCatalog.Get(family, i).Name;
                // Trees are always singular (Mira), even when the name ends in "s" (cypress).
                bool plural = v != 0 && ItemWords.Plural(name);
                bool mass = v != 0 && ItemWords.Mass(name);
                string arrival = NeighbourNotice.Arrival(v, name);
                string waiting = NeighbourNotice.Waiting(v, name);
                Expect(!string.IsNullOrEmpty(arrival) && !string.IsNullOrEmpty(waiting), "lines exist for " + name);

                foreach (string line in new[] { arrival, waiting })
                {
                    Expect(!ArticleBeforeVowel(line), "no a/an error in: " + line);
                    if (plural || mass)
                        Expect(!ArticleBefore(line, name), "no a before plural or mass name in: " + line);
                }

                // Fen and Orin never say "it" about a plural arrival.
                if (v != 0 && plural)
                    Expect(!WordIt(arrival), "no 'it' for plural arrival of " + name + ": " + arrival);
            }
        }
    }

    public static int Run()
    {
        assertions = 0;
        TestItemWords();
        TestExactLines();
        TestEveryCatalogueItem();
        return assertions;
    }
}
