using System;

namespace Evergrove.Core
{
    /// <summary>
    /// All state-to-text decisions are deterministic and locale-neutral.
    /// Language changes cannot alter rules, choices, save files or NPC truth.
    /// The full English authored manuscript remains available in VillageStory;
    /// this concise cross-locale script is intentionally parity-oriented.
    /// </summary>
    public static class LocalizedVillage
    {
        private static string T(string id, params object[] args)
        {
            return GameLocalization.Text(id, args);
        }

        public static string Prompt(WorldModel world)
        {
            if (world.Round < 4) return T("prompt_early");
            if (world.Round < 9) return T("prompt_mid");
            return T("prompt_late");
        }

        public static string Choice(VillageChoice choice)
        {
            switch (choice)
            {
                case VillageChoice.ShadeTree: return T("tree");
                case VillageChoice.Bridge: return T("bridge");
                default: return T("flowers");
            }
        }

        public static string ChoiceOutcome(VillageChoice choice, WorldModel world)
        {
            if (choice == VillageChoice.ShadeTree)
                return world.Count(VillageChoice.Bridge) > 0 ? T("world_tree_bridge") : T("world_tree");
            if (choice == VillageChoice.Bridge)
                return world.Count(VillageChoice.Flowers) > 0 ? T("world_bridge_flowers") : T("world_bridge");
            return world.Count(VillageChoice.ShadeTree) > 0 ? T("world_tree_flowers") : T("world_flowers");
        }

        public static string Memory(WorldModel world)
        {
            int[] choices = world.Choices;
            if (choices.Length == 0) return T("world_none");
            switch ((VillageChoice)choices[choices.Length - 1])
            {
                case VillageChoice.ShadeTree: return T("memory_tree");
                case VillageChoice.Bridge: return T("memory_bridge");
                default: return T("memory_flowers");
            }
        }

        public static string Consequence(WorldModel world)
        {
            // The first river can already be fully restored BEFORE the player
            // chooses a tree/bridge/flowers. Returning world_none at that
            // moment falsely tells the player the old creek still waits for
            // water, even though the world and puzzle visibly show full flow.
            // Reuse the existing translated solved key (all 30 catalogs)
            // instead of adding another uncertified translation draft.
            if (world.Choices.Length == 0 && world.Solved)
                return T("solved");
            bool trees = world.Count(VillageChoice.ShadeTree) > 0;
            bool bridge = world.Count(VillageChoice.Bridge) > 0;
            bool flowers = world.Count(VillageChoice.Flowers) > 0;
            if (trees && bridge && flowers) return T("world_all");
            if (trees && bridge) return T("world_tree_bridge");
            if (trees && flowers) return T("world_tree_flowers");
            if (bridge && flowers) return T("world_bridge_flowers");
            if (trees) return T("world_tree");
            if (bridge) return T("world_bridge");
            if (flowers) return T("world_flowers");
            return T("world_none");
        }

        public static string Speak(WorldModel world, int index)
        {
            switch (index)
            {
                case 0: return T(world.Count(VillageChoice.ShadeTree) == 0 ? "mira_new" : "mira_after");
                case 1: return T(world.Count(VillageChoice.Bridge) == 0 ? "orin_new" : "orin_after");
                case 2: return T(world.Count(VillageChoice.Flowers) == 0 ? "fen_new" : "fen_after");
                default: throw new ArgumentOutOfRangeException(nameof(index));
            }
        }

        public static string[] Journal(WorldModel world)
        {
            int[] choices = world.Choices;
            string[] lines = new string[choices.Length];
            for (int i = 0; i < choices.Length; i++)
            {
                string person = choices[i] == 0 ? "Mira" : choices[i] == 1 ? "Orin" : "Fen";
                // Journal facts are derived from observed, saved choices.
                // A historical migration may not have used rotated puzzles,
                // so only claim a mechanical effect when the *current* saved
                // variant demonstrably matches the most recent event.
                string immediate = choices[i] == 0 ? T("world_tree") :
                    choices[i] == 1 ? T("world_bridge") : T("world_flowers");
                string entry = T("journal_entry", i + 1, person,
                    Choice((VillageChoice)choices[i])) + "\n" + immediate;
                if (i == choices.Length - 1 && !world.Completed &&
                    world.BoardVariant == choices[i] + 1)
                    entry += "\n" + EnvironmentalRule(world);
                lines[i] = entry;
            }
            return lines;
        }

        public static string MemoryEvent(WorldModel world)
        {
            string[] lines = Journal(world);
            if (lines.Length == 0) return T("world_none");
            // Keep the story interlude concise; the detailed factual chain
            // belongs in the optional journal, not a mandatory tall card.
            int[] history = world.Choices;
            int last = history.Length - 1;
            string person = history[last] == 0 ? "Mira" :
                history[last] == 1 ? "Orin" : "Fen";
            return T("journal_entry", last + 1, person,
                Choice((VillageChoice)history[last])) + "\n" +
                Memory(world) + "\n" + Consequence(world);
        }

        public static string EnvironmentalRule(WorldModel world)
        {
            // The highlighted tile is a fixed habitat. The visible marker
            // and disabled rotation communicate immutability; localized
            // ecological consequence describes which action caused it.
            switch (world.BoardVariant)
            {
                case 1: return T("landmark_tree");
                case 2: return T("landmark_bridge");
                case 3: return T("landmark_flowers");
                default: return "";
            }
        }

        public static string Ending(WorldModel world)
        {
            if (!world.Completed) return T("world_none");
            if (world.Mode == PlayMode.PuzzleOnly) return T("complete");
            if (world.Count(VillageChoice.ShadeTree) > 0 &&
                world.Count(VillageChoice.Bridge) > 0 &&
                world.Count(VillageChoice.Flowers) > 0)
                return T("ending_all");
            int trees = world.Count(VillageChoice.ShadeTree);
            int bridges = world.Count(VillageChoice.Bridge);
            int flowers = world.Count(VillageChoice.Flowers);
            if (trees >= bridges && trees >= flowers) return T("ending_tree");
            if (bridges >= flowers) return T("ending_bridge");
            return T("ending_flowers");
        }
    }
}
