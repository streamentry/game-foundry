using System;
using Evergrove.Core;

/// <summary>
/// Changing your mind leaves no ghosts. After ReconsiderLatestChoice() and a new choice, the
/// landmark cause line, the lantern letter, the memory, the consequence and the journal name only
/// the new choice, never the reconsidered one. Covers every ordered pair of families.
/// </summary>
internal static class ReconsiderTruthTests
{
    private static int assertions;

    private static void Expect(bool ok, string message)
    {
        assertions++;
        if (!ok) throw new Exception("ReconsiderTruthTests failed: " + message);
    }

    public static int Run()
    {
        assertions = 0;
        VillageChoice[] families = { VillageChoice.ShadeTree, VillageChoice.Bridge, VillageChoice.Flowers };
        int[] days = { 20261101, 20261102, 20261103, 20261104, 20261105 };

        foreach (VillageChoice first in families)
        {
            foreach (VillageChoice second in families)
            {
                if (second == first) continue;
                CheckSwap(first, second, days);
            }
        }

        MarksLeaveWithTheThing();
        return assertions;
    }

    // A reconsidered thing takes its "solved together" and "helped by" marks with it. Before the fix
    // they stayed on the reconsidered index: the next thing chosen inherited them, and a save written in
    // between failed validation on reload (helped indexes must be below the number of choices).
    private static void MarksLeaveWithTheThing()
    {
        WorldModel world = WorldModel.New(PlayMode.Living);
        Solve(world);
        Expect(world.Choose(VillageChoice.ShadeTree), "marks: first choice accepted");
        world.SolveTree(0);
        Expect(world.MarkTogether(0), "marks: the thing's puzzle was solved together");
        WorldSnapshot snap = world.Export();
        snap.helped = new[] { 0 };
        snap.helpedFrom = new[] { "Nan" };
        Expect(WorldModel.TryRestore(snap, out world), "marks: a helped garden restores");
        Expect(world.IsHelped(0) && world.SolvedTogether(0), "marks: both marks are on the thing");

        Expect(world.ReconsiderLatestChoice(), "marks: the choice can be reconsidered");
        Expect(!world.SolvedTogether(0) && world.TogetherCount == 0, "marks: 'solved together' leaves with the thing");
        Expect(!world.IsHelped(0) && world.Helped.Length == 0 && world.HelpedFrom.Length == 0, "marks: 'helped by' leaves with the thing");
        Expect(WorldModel.TryRestore(world.Export(), out WorldModel reloaded), "marks: the save right after reconsidering reloads");

        Expect(reloaded.Choose(VillageChoice.Bridge), "marks: a new choice is accepted");
        Expect(!reloaded.SolvedTogether(0) && !reloaded.IsHelped(0), "marks: the new thing inherits nothing");
    }

    private static void CheckSwap(VillageChoice a, VillageChoice b, int[] days)
    {
        string tag = a + " -> " + b;
        WorldModel world = WorldModel.New(PlayMode.Living);
        Solve(world);
        Expect(world.Choose(a), tag + ": first choice accepted");
        string aItem = GardenCatalog.For(world.Garden, 0, a).Name;
        string bItem = GardenCatalog.For(world.Garden, 0, b).Name;
        string board = PuzzleCatalog.Get(0).Name;
        // Names that are substrings of one another cannot be told apart in text; skip ghost search.
        bool distinct = !Mentions(bItem, aItem) && !Mentions(aItem, bItem);

        // Before the swap: the save names A's item in the lantern letter and the landmark line names A.
        Expect(LandmarkCause.For(world) == LandmarkCause.Line(a, board, aItem), tag + ": landmark names A before swap");
        foreach (int day in days)
            Expect(Mentions(DayRhythm.LanternWords(day, world), aItem), tag + ": lantern names A before swap on " + day);

        // Reconsider: the choice is undone and the previous board is solved again.
        Expect(world.ReconsiderLatestChoice(), tag + ": reconsider accepted");
        Expect(world.Round == 0, tag + ": reconsider returns to board 0");
        Expect(world.Solved, tag + ": reopened board is solved");
        Expect(world.Count(a) == 0, tag + ": A is no longer counted after reconsider");
        Expect(world.Choices.Length == 0, tag + ": no choices remain after reconsider");
        Expect(LandmarkCause.For(world) == LocalizedVillage.EnvironmentalRule(world),
            tag + ": landmark is generic while no choice is recorded");
        foreach (int day in days)
            Expect(DayRhythm.LanternWords(day, world) == DayRhythm.LanternWords(day),
                tag + ": lantern is generic after reconsider on " + day);

        // Choose the other family.
        Expect(world.Choose(b), tag + ": second choice accepted");
        Expect(world.Count(a) == 0, tag + ": A still counts zero after swap");
        Expect(world.Count(b) == 1, tag + ": B counts one after swap");

        // Landmark: exactly the line for B, and no A wording.
        string landmark = LandmarkCause.For(world);
        Expect(landmark == LandmarkCause.Line(b, board, bItem), tag + ": landmark names B after swap (exact line, so no A wording)");
        Expect(landmark != LandmarkCause.Line(a, board, aItem), tag + ": landmark differs from the A line");

        // Lantern: names B's item, never A's (when the names are distinct).
        foreach (int day in days)
        {
            string letter = DayRhythm.LanternWords(day, world);
            Expect(Mentions(letter, bItem), tag + ": lantern names B on " + day);
            if (distinct) Expect(!Mentions(letter, aItem), tag + ": lantern has no A ghost on " + day);
        }

        // Memory, consequence and every journal entry carry no A item name.
        if (distinct)
        {
            Expect(!Mentions(world.Memory, aItem), tag + ": memory has no A ghost");
            Expect(!Mentions(world.Consequence, aItem), tag + ": consequence has no A ghost");
            foreach (string entry in world.Journal)
                Expect(!Mentions(entry, aItem), tag + ": journal entry has no A ghost: " + entry);
        }
        Expect(world.Journal.Length == 1, tag + ": journal has exactly one entry");
    }

    private static bool Mentions(string text, string name)
    {
        return text != null && text.IndexOf(name, StringComparison.OrdinalIgnoreCase) >= 0;
    }

    private static void Solve(WorldModel world)
    {
        Puzzle board = world.CurrentPuzzle;
        for (int tile = 0; tile < board.CellCount; tile++)
            for (int turn = 0; turn < 4 && world.Rotations[tile] != board.Solution[tile]; turn++)
                Expect(world.Rotate(tile), "solver turns only mutable tiles");
        Expect(world.Solved, "fixture reaches a valid solution");
    }
}
