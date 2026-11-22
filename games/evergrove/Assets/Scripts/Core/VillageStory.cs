using System;

namespace Evergrove.Core
{
    /// <summary>
    /// Curated narrative from persisted actions. These conversations are
    /// intentionally deterministic, causal and free of LLM requests.
    /// </summary>
    public static class VillageStory
    {
        private static int Trees(WorldModel world) { return world.Count(VillageChoice.ShadeTree); }
        private static int Bridges(WorldModel world) { return world.Count(VillageChoice.Bridge); }
        private static int Flowers(WorldModel world) { return world.Count(VillageChoice.Flowers); }

        public static bool HasShadedCrossing(WorldModel world) { return Trees(world) > 0 && Bridges(world) > 0; }
        public static bool HasButterflyGrove(WorldModel world) { return Trees(world) > 0 && Flowers(world) > 0; }
        public static bool HasCommunityWalk(WorldModel world) { return Bridges(world) > 0 && Flowers(world) > 0; }
        public static bool HasLivingHeart(WorldModel world) { return Trees(world) > 0 && Bridges(world) > 0 && Flowers(world) > 0; }

        public static string ChapterPrompt(WorldModel world)
        {
            if (world.Completed) return "The river has found its way home.";
            if (world.Round < 3) return "A little water can change a place.";
            if (world.Round < 6) return "The villagers are beginning to notice your care.";
            if (world.Round < 9) return "The old creek now reaches unfamiliar ground.";
            return "Every choice has become part of this place.";
        }

        public static string Memory(WorldModel world)
        {
            int[] history = world.Choices;
            if (history.Length == 0) return "A quiet place, waiting for its first story.";
            switch ((VillageChoice)history[history.Length - 1])
            {
                case VillageChoice.ShadeTree: return "Mira remembers the shade you planted by the stream.";
                case VillageChoice.Bridge: return "Orin remembers the crossing you made possible.";
                default: return "Fen remembers how the meadow returned to life.";
            }
        }

        public static string Consequence(WorldModel world)
        {
            int trees = Trees(world), bridges = Bridges(world), flowers = Flowers(world);
            if (HasLivingHeart(world) && trees >= 2 && bridges >= 2 && flowers >= 2)
                return "Every bank is alive. Neighbors cross beneath the canopy while butterflies gather.";
            if (HasLivingHeart(world))
                return "The shady bridge leads to a meadow. A whole little community has formed.";
            if (HasShadedCrossing(world) && trees >= 2)
                return "A cool path now runs through the trees to the restored crossing.";
            if (HasButterflyGrove(world) && flowers >= 2)
                return "The tree roots protect a wildflower meadow buzzing with pollinators.";
            if (HasCommunityWalk(world))
                return "Visitors cross the bridge to watch the flowers by the water.";
            if (HasShadedCrossing(world)) return "The restored crossing is now shaded by a young tree.";
            if (HasButterflyGrove(world)) return "Shade shelters the first wildflowers and butterflies.";
            if (trees >= 2) return "A small canopy is forming along the stream.";
            if (bridges >= 2) return "Neighbors have restored more than one crossing.";
            if (flowers >= 2) return "The bank is turning into a bright, open meadow.";
            if (trees > 0) return "The first young tree is taking root by the water.";
            if (bridges > 0) return "The bridge has brought the two banks together.";
            if (flowers > 0) return "Wildflowers are finding their way back to the bank.";
            return "An old stream runs quietly past three hopeful neighbors.";
        }

        public static string[] Characters(WorldModel world)
        {
            int trees = Trees(world), bridges = Bridges(world), flowers = Flowers(world);
            string mira = trees == 0 ? "Mira  /  The orchard still waits for shade."
                : trees >= 3 ? "Mira  /  I can see a real canopy growing now."
                : HasButterflyGrove(world) ? "Mira  /  Fen's flowers thrive beneath our trees."
                : "Mira  /  I remember where we planted the first tree.";
            string orin = bridges == 0 ? "Orin  /  One day we might meet across the water."
                : HasShadedCrossing(world) ? "Orin  /  Our bridge is the coolest place to rest."
                : bridges >= 3 ? "Orin  /  Every new crossing brings a neighbor closer."
                : "Orin  /  You gave the village a way across.";
            string fen = flowers == 0 ? "Fen  /  I hope to hear the bees again."
                : HasCommunityWalk(world) ? "Fen  /  People are stopping to notice the meadow."
                : flowers >= 3 ? "Fen  /  Look how many colors the bank remembers."
                : "Fen  /  The meadow is coming back, one flower at a time.";
            return new[] { mira, orin, fen };
        }

        public static string ChoiceLabel(VillageChoice choice)
        {
            switch (choice)
            {
                case VillageChoice.ShadeTree: return "Plant a shade tree";
                case VillageChoice.Bridge: return "Restore a crossing";
                default: return "Grow wildflowers";
            }
        }

        public static string ChoicePromise(VillageChoice choice, WorldModel world)
        {
            switch (choice)
            {
                case VillageChoice.ShadeTree:
                    return Bridges(world) > 0 ? "Cool the crossing and shelter visitors." : "Give the bank roots, shade and a home.";
                case VillageChoice.Bridge:
                    return Flowers(world) > 0 ? "Invite neighbors into the meadow." : "Reconnect the two banks of the river.";
                default:
                    return Trees(world) > 0 ? "Bring butterflies into the young grove." : "Bring color and pollinators back.";
            }
        }

        public static string MemoryEvent(WorldModel world)
        {
            int[] history = world.Choices;
            if (history.Length == 0) return "The village has no recorded choices yet.";
            VillageChoice last = (VillageChoice)history[history.Length - 1];
            string action = ChoiceLabel(last);
            return "CHAPTER " + history.Length + "  /  " + action + "\n" + Memory(world) + "\n" + Consequence(world);
        }

        public static string[] Journal(WorldModel world)
        {
            int[] history = world.Choices;
            string[] log = new string[history.Length];
            for (int i = 0; i < history.Length; i++)
            {
                VillageChoice choice = (VillageChoice)history[i];
                string person = choice == VillageChoice.ShadeTree ? "Mira" :
                    choice == VillageChoice.Bridge ? "Orin" : "Fen";
                log[i] = "Chapter " + (i + 1) + " · " + person + " · " + ChoiceLabel(choice);
            }
            return log;
        }

        public static string Ending(WorldModel world)
        {
            if (!world.Completed || world.Mode == PlayMode.PuzzleOnly)
                return "The last stream is still waiting.";
            int trees = Trees(world), bridges = Bridges(world), flowers = Flowers(world);
            if (HasLivingHeart(world))
                return "THE VILLAGE WE GREW\n\nYour shade, crossings and meadows now belong to each other. " +
                    "There is no perfect garden—only a place shaped by what you cared to do.";
            if (trees >= bridges && trees >= flowers)
                return "THE QUIET ORCHARD\n\nYou chose shelter and patience. " +
                    "The trees hold the banks together, and the village rests in their shade.";
            if (bridges >= flowers)
                return "THE VILLAGE OF PATHS\n\nYou chose connection. " +
                    "Neighbors can meet in places they once could only see from across the river.";
            return "THE WILD MEADOW\n\nYou chose a place where life could return. " +
                "Every little flower brought another visitor to the stream.";
        }
    }
}
