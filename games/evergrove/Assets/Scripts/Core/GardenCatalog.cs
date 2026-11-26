using System;

namespace Evergrove.Core
{
    /// <summary>One specific thing a choice grows, blooms or builds (I171).</summary>
    public sealed class GardenItem
    {
        public readonly VillageChoice Family;
        public readonly int Index;      // position within its family's list
        public readonly string Art;     // sprite name: tree-00, flower-00, build-00
        public readonly string Card;    // "Plant a quince tree"
        public readonly string Memory;  // "the quince tree you planted"
        public readonly string Name;    // "quince tree"

        public GardenItem(VillageChoice family, int index, string art, string card, string memory, string name)
        {
            Family = family;
            Index = index;
            Art = art;
            Card = card;
            Memory = memory;
            Name = name;
        }
    }

    /// <summary>
    /// The garden's catalogue (I171): 50 trees (Mira), 50 flowers (Fen) and 100
    /// things to build or mend (Orin). The three families stay the rule-level
    /// choice (they decide the next board); the catalogue only says *which* tree,
    /// flower or building a choice is in a given garden. Deterministic: the same
    /// garden number and chapter always give the same item, never a clock or
    /// random draw. A garden never repeats an item; successive gardens walk
    /// different items, so the whole catalogue appears over time. English only.
    /// </summary>
    public static class GardenCatalog
    {
        // Trees: noun, a/an.
        private static readonly string[] Trees = {
            "apple tree", "pear tree", "cherry tree", "plum tree", "quince tree",
            "walnut tree", "sweet chestnut", "hazel", "fig tree", "olive tree",
            "lemon tree", "orange tree", "peach tree", "apricot tree", "mulberry tree",
            "elder", "rowan", "silver birch", "weeping willow", "oak",
            "copper beech", "linden", "sugar maple", "red maple", "ginkgo",
            "magnolia", "dogwood", "almond tree", "juniper", "Scots pine",
            "cedar", "cypress", "yew", "holly", "hawthorn",
            "crab apple", "medlar", "persimmon", "pomegranate", "bay laurel",
            "catalpa", "tulip tree", "horse chestnut", "alder", "aspen",
            "poplar", "hornbeam", "Japanese maple", "jacaranda", "lilac tree"
        };

        // Flowers: grown as a planting ("Grow poppies", "the poppies you grew").
        private static readonly string[] Flowers = {
            "lavender", "sunflowers", "poppies", "roses", "peonies",
            "tulips", "daffodils", "bluebells", "foxgloves", "hollyhocks",
            "lupins", "cornflowers", "daisies", "marigolds", "sweet peas",
            "irises", "lilies", "water lilies", "forget-me-nots", "primroses",
            "violets", "snowdrops", "crocuses", "hydrangeas", "dahlias",
            "zinnias", "cosmos", "asters", "chamomile", "clover",
            "heather", "honeysuckle", "jasmine", "wisteria", "camellias",
            "lily of the valley", "delphiniums", "snapdragons", "nasturtiums", "pansies",
            "geraniums", "lotus", "cowslips", "buttercups", "sweet williams",
            "alliums", "coneflowers", "hibiscus", "bleeding hearts", "anemones"
        };

        // Things to build or mend: verb, past tense, noun.
        private static readonly string[,] Builds = {
            { "Build", "built", "stone bridge" }, { "Build", "built", "footbridge" },
            { "Lay", "laid", "stepping stones" }, { "Mend", "mended", "mill wheel" },
            { "Restore", "restored", "windmill" }, { "Dig", "dug", "village well" },
            { "Raise", "raised", "dovecote" }, { "Set up", "set up", "beehives" },
            { "Build", "built", "glasshouse" }, { "Mend", "mended", "garden shed" },
            { "Build", "built", "potting bench" }, { "Set", "set", "bird bath" },
            { "Hang", "hung", "nesting boxes" }, { "Raise", "raised", "pergola" },
            { "Raise", "raised", "rose arch" }, { "Build", "built", "gazebo" },
            { "Restore", "restored", "bandstand" }, { "Carve", "carved", "creekside bench" },
            { "Build", "built", "picnic table" }, { "Mend", "mended", "picket fence" },
            { "Rebuild", "rebuilt", "dry stone wall" }, { "Hang", "hung", "garden gate" },
            { "Light", "lit", "lantern posts" }, { "Set", "set", "sundial" },
            { "Make", "made", "scarecrow" }, { "Build", "built", "compost bins" },
            { "Set out", "set out", "rain barrels" }, { "Mend", "mended", "hand pump" },
            { "Carve", "carved", "stone trough" }, { "Restore", "restored", "fountain" },
            { "Dig", "dug", "lily pond" }, { "Build", "built", "duck house" },
            { "Restore", "restored", "boathouse" }, { "Mend", "mended", "rowing boat" },
            { "Build", "built", "jetty" }, { "Mend", "mended", "lock gate" },
            { "Rebuild", "rebuilt", "weir" }, { "Restore", "restored", "little aqueduct" },
            { "Dig", "dug", "garden rill" }, { "Build", "built", "terraces" },
            { "Build", "built", "raised beds" }, { "Build", "built", "cold frame" },
            { "Make", "made", "orchard ladder" }, { "Mend", "mended", "hay cart" },
            { "Rebuild", "rebuilt", "bread oven" }, { "Build", "built", "hen house" },
            { "Build", "built", "goat shed" }, { "Mend", "mended", "stable door" },
            { "Mend", "mended", "barn roof" }, { "Thatch", "thatched", "cottage roof" },
            { "Rebuild", "rebuilt", "chimney" }, { "Raise", "raised", "weathervane" },
            { "Mend", "mended", "clock tower" }, { "Hang", "hung", "school bell" },
            { "Hang", "hung", "swing" }, { "Build", "built", "tree house" },
            { "Hang", "hung", "rope swing" }, { "Build", "built", "seesaw" },
            { "Lay", "laid", "cobbled lane" }, { "Pave", "paved", "village square" },
            { "Build", "built", "market stall" }, { "Paint", "painted", "flower cart" },
            { "Open", "opened", "tea house" }, { "Build", "built", "book swap box" },
            { "Set up", "set up", "post box" }, { "Carve", "carved", "signpost" },
            { "Build", "built", "lookout tower" }, { "Build", "built", "viewing deck" },
            { "Build", "built", "bird hide" }, { "Hang", "hung", "bat boxes" },
            { "Build", "built", "bug hotel" }, { "Build", "built", "hedgehog house" },
            { "Stack", "stacked", "log pile" }, { "Dig", "dug", "frog pond" },
            { "Build", "built", "butterfly house" }, { "Dig", "dug", "kitchen garden" },
            { "Build", "built", "herb spiral" }, { "Raise", "raised", "vine trellis" },
            { "Raise", "raised", "hop poles" }, { "Mend", "mended", "apple press" },
            { "Build", "built", "fruit store" }, { "Dig", "dug", "root cellar" },
            { "Restore", "restored", "ice house" }, { "String", "strung", "washing line" },
            { "Restore", "restored", "wash house" }, { "Relight", "relit", "forge" },
            { "Fire", "fired", "pottery kiln" }, { "Open", "opened", "weaving shed" },
            { "Open", "opened", "woodworking shed" }, { "Mend", "mended", "sawmill" },
            { "Build", "built", "ferry raft" }, { "Hang", "hung", "rope bridge" },
            { "Build", "built", "covered bridge" }, { "Hang", "hung", "swing bench" },
            { "Build", "built", "fire pit" }, { "Build", "built", "outdoor stage" },
            { "Hang", "hung", "paper lanterns" }, { "Hang", "hung", "wind chimes" },
            { "Lay", "laid", "cascade steps" }, { "Build", "built", "rain shelter" }
        };

        public const int TreeCount = 50;
        public const int FlowerCount = 50;
        public const int BuildCount = 100;
        public static int Total { get { return TreeCount + FlowerCount + BuildCount; } }

        public static int CountOf(VillageChoice family)
        {
            return family == VillageChoice.ShadeTree ? Trees.Length
                : family == VillageChoice.Flowers ? Flowers.Length : Builds.GetLength(0);
        }

        /// <summary>The item at a position in a family's list.</summary>
        public static GardenItem Get(VillageChoice family, int index)
        {
            int count = CountOf(family);
            if (index < 0 || index >= count) throw new ArgumentOutOfRangeException(nameof(index));
            if (family == VillageChoice.ShadeTree)
            {
                string tree = Trees[index];
                string article = "aeiouAEIOU".IndexOf(tree[0]) >= 0 ? "an " : "a ";
                return new GardenItem(family, index, "tree-" + index.ToString("00"),
                    "Plant " + article + tree, "the " + tree + " you planted", tree);
            }
            if (family == VillageChoice.Flowers)
            {
                string flower = Flowers[index];
                return new GardenItem(family, index, "flower-" + index.ToString("00"),
                    "Grow " + flower, "the " + flower + " you grew", flower);
            }
            string verb = Builds[index, 0], past = Builds[index, 1], thing = Builds[index, 2];
            return new GardenItem(family, index, "build-" + index.ToString("00"),
                verb + " the " + thing, "the " + thing + " you " + past, thing);
        }

        /// <summary>
        /// Which item a family's choice is in a garden's chapter. Each family has
        /// one fixed shuffled order; garden g reads the next 12 items of it from
        /// position 12g (wrapping). So no item repeats inside a garden, consecutive
        /// gardens show different items, and every item appears within a few gardens.
        /// </summary>
        public static GardenItem For(int garden, int chapter, VillageChoice family)
        {
            int count = CountOf(family);
            if (chapter < 0 || chapter >= ChaptersPerGarden) throw new ArgumentOutOfRangeException(nameof(chapter));
            if (garden < 0) throw new ArgumentOutOfRangeException(nameof(garden));
            int[] order = Order(family);
            long at = ((long)garden * ChaptersPerGarden + chapter) % count;
            return Get(family, order[at]);
        }

        public const int ChaptersPerGarden = 12;

        /// <summary>A family's fixed shuffled order (seeded Fisher–Yates; no clock, no randomness).</summary>
        public static int[] Order(VillageChoice family)
        {
            int count = CountOf(family);
            var order = new int[count];
            for (int i = 0; i < count; i++) order[i] = i;
            uint state = 0x9E3779B9u ^ (uint)(((int)family + 1) * 40503);
            for (int i = count - 1; i > 0; i--)
            {
                state = state * 1664525u + 1013904223u;
                int j = (int)((state >> 8) % (uint)(i + 1));
                int swap = order[i];
                order[i] = order[j];
                order[j] = swap;
            }
            return order;
        }
    }
}
