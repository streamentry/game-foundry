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

        public static string VillagerName(int index)
        {
            switch (index)
            {
                case 0: return "Mira · Gardener";
                case 1: return "Orin · Builder";
                case 2: return "Fen · Naturalist";
                default: throw new ArgumentOutOfRangeException(nameof(index));
            }
        }

        /// <summary>
        /// Authored, inspectable conversations. Characters remember the player's
        /// actual world choices, not conversations, sensitive data or inferred mood.
        /// A future LLM must never fabricate this persistent causal truth.
        /// </summary>
        public static string Speak(WorldModel world, int index)
        {
            int trees = Trees(world), bridges = Bridges(world), flowers = Flowers(world);
            switch (index)
            {
                case 0:
                    if (trees == 0 && bridges == 0 && flowers == 0)
                        return "I'm Mira. The bank used to be shaded in summer. I wonder whether a new sapling could take root by the water.";
                    if (HasLivingHeart(world))
                        return "You planted the first tree. Now Orin's crossing leads to Fen's meadow, and people rest in its shade. I like how the three little things belong together.";
                    if (HasButterflyGrove(world))
                        return "Do you remember the little tree beside the creek? Fen's flowers brought butterflies under its leaves. I wouldn't have guessed that when we planted it.";
                    if (HasShadedCrossing(world))
                        return "Orin's crossing would be a hot place to stop without your trees. Now we can rest in their shade and hear the river below.";
                    if (trees >= 2)
                        return "I can almost see the canopy forming. One tree was a beginning; the next gave the first somewhere to belong.";
                    if (trees > 0)
                        return "I remember the day you planted the first sapling. Its roots have begun to hold the bank in place. Thank you for giving it a chance.";
                    return "I see the things you've helped the village build. There's still room for one small tree when you feel like planting one.";
                case 1:
                    if (bridges == 0 && trees == 0 && flowers == 0)
                        return "I'm Orin. I can see Fen waving from the other side of the creek, but the old bridge is gone. We could build a new way across someday.";
                    if (HasLivingHeart(world))
                        return "I remember the first plank we laid. Now I cross to Fen's meadow through Mira's shade. The crossing has become a place to meet, not just a way through.";
                    if (HasCommunityWalk(world))
                        return "Before you fixed the bridge, people passed by the meadow without meeting. Now they cross together to see the flowers. It's a good kind of busy.";
                    if (HasShadedCrossing(world))
                        return "I thought a bridge was just wood and stone. Then you planted a tree beside it. Now the crossing feels like somewhere you'd want to stay.";
                    if (bridges > 0)
                        return "The boards of our crossing are holding beautifully. A neighbor came across today just to say hello. A little path can make a big difference.";
                    if (trees > 0)
                        return "Mira's young trees are changing the banks. Maybe one day we could build a footbridge under their shade.";
                    return "The water is finding its way back. I still hope we'll have a crossing where neighbors can meet.";
                case 2:
                    if (flowers == 0 && trees == 0 && bridges == 0)
                        return "I'm Fen. This bank used to be full of wildflowers. I haven't heard a bee here in a long while. It doesn't need to happen all at once.";
                    if (HasLivingHeart(world))
                        return "I remember the first handful of wildflower seeds. Now butterflies rest under Mira's trees, and neighbors come across Orin's bridge to watch them.";
                    if (HasButterflyGrove(world))
                        return "When you planted the trees, the bank grew cooler. When you grew the flowers, butterflies appeared. We made a little shelter without planning it.";
                    if (HasCommunityWalk(world))
                        return "The meadow is beautiful, but my favorite thing is seeing someone pause on Orin's crossing to notice it. We gave them a reason to stop.";
                    if (flowers >= 2)
                        return "The first flowers brought color. The next patch brought bees. It feels like the bank is slowly finding its own voice.";
                    if (flowers > 0)
                        return "I remember the first seeds you scattered. There are flowers beside the river again. I wonder what might come and visit them.";
                    if (bridges > 0)
                        return "Orin opened the crossing, so more neighbors can see this quiet bank. Imagine if it were full of flowers.";
                    return "I love the way the creek sounds now. Maybe a few wildflowers will grow along it one day.";
                default:
                    throw new ArgumentOutOfRangeException(nameof(index));
            }
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
