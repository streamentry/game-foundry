using System;
using Evergrove.Core;

internal static class Program
{
    private static int assertions;

    private static void Expect(bool condition, string description)
    {
        assertions++;
        if (!condition) throw new Exception("FAILED: " + description);
    }

    private static void Solve(WorldModel model)
    {
        Puzzle board = model.CurrentPuzzle;
        for (int index = 0; index < board.CellCount; index++)
        {
            int attempts = 0;
            while (model.Rotations[index] != board.Solution[index])
            {
                Expect(!board.IsAnchored(index) && model.Rotate(index), "only mutable tile rotates");
                attempts++;
                Expect(attempts <= PipeRules.OrientationCount(board.Kinds[index]),
                    "rotation bounded");
            }
        }
        Expect(model.Solved, "reference solution accepted");
        Expect(model.Insight.LeakCount == 0 &&
               model.Insight.ConnectedCount == board.CellCount, "all water reaches the outlet");
    }

    private static void ContentAndRules()
    {
        Expect(PuzzleCatalog.Count == 12, "12 authored water puzzles");
        for (int index = 0; index < PuzzleCatalog.Count; index++)
        {
            Puzzle board = PuzzleCatalog.Get(index);
            int expectedSize = index < 6 ? 3 : 4;
            Expect(board.Size == expectedSize, "difficulty graduates to 4x4");
            Expect(board.Kinds.Length == board.Size * board.Size, "board has exact size");
            Expect(PipeRules.ValidRotations(board, board.Initial), "board starts valid");
            Expect(PipeRules.ValidRotations(board, board.Solution), "authored answer valid");
            Expect(!PipeRules.IsComplete(board, board.Initial), "board starts unsolved");
            Expect(PipeRules.IsComplete(board, board.Solution), "authored solution wins");
            Expect(PipeRules.CountSolutions(board, 2) == 1, "one mathematical solution");
            Expect(PipeRules.Inspect(board, board.Solution).JoinedEdges == board.CellCount - 1,
                "complete path has N-1 joined edges");
            Expect(PipeRules.Inspect(board, board.Solution).LeakCount == 0,
                "complete path never leaks");
            int[] wrong = (int[])board.Solution.Clone();
            wrong[0] = wrong[0] % PipeRules.OrientationCount(board.Kinds[0]) + 1;
            Expect(!PipeRules.IsComplete(board, wrong), "wrong orientation fails");
            wrong[0] = 100;
            Expect(!PipeRules.ValidRotations(board, wrong), "out of range fails");
        }
    }

    private static void RestoreAndMigration()
    {
        WorldModel initial = WorldModel.New();
        Expect(initial.CanSwitchMode, "mode can be chosen before first action");
        Expect(!initial.Choose(VillageChoice.ShadeTree), "choices gated until water flows");
        Expect(!initial.Rotate(-1) && !initial.Rotate(9), "out of range tile rejected");
        Expect(initial.Rotate(0), "rotation succeeds");
        Expect(!initial.CanSwitchMode, "mode locks during play");
        Expect(initial.Undo(), "last rotation can be undone");
        Expect(initial.CanSwitchMode, "mode unlocks after undo");
        Expect(initial.GuideOneConnection(), "optional hint aligns exactly one tile");
        Expect(initial.HintsUsed == 1, "hint count recorded");
        Expect(initial.Undo(), "guided tile can also be undone");
        Expect(initial.HintsUsed == 1, "undo does not secretly refund a used hint");
        Expect(initial.Restart(), "restart works");

        WorldSnapshot corrupt = initial.Export();
        corrupt.choices = new[] { 0, 1 };
        WorldModel parsed;
        Expect(!WorldModel.TryRestore(corrupt, out parsed), "forged actions fail closed");
        corrupt = initial.Export();
        corrupt.rotations[0] = 999;
        Expect(!WorldModel.TryRestore(corrupt, out parsed), "invalid rotations fail closed");
        corrupt = initial.Export();
        corrupt.solved = true;
        Expect(!WorldModel.TryRestore(corrupt, out parsed), "forged win fails closed");
        corrupt = initial.Export();
        corrupt.version = 999;
        Expect(!WorldModel.TryRestore(corrupt, out parsed), "unknown version fails closed");
        corrupt = initial.Export();
        corrupt.hintsUsed = -2;
        Expect(!WorldModel.TryRestore(corrupt, out parsed), "invalid hint count fails closed");

        // Original Evergrove v1 ran for exactly six chapters.
        var legacyDone = new WorldSnapshot {
            version = 1, mode = "living", round = 6,
            solved = false, rotations = Array.Empty<int>(), previous = Array.Empty<int>(),
            choices = new[] { 0, 1, 2, 0, 1, 2 }
        };
        Expect(WorldModel.TryRestore(legacyDone, out parsed), "legacy completed garden migrates");
        Expect(parsed.Round == 6 && !parsed.Completed, "old ending unlocks chapter seven");
        Expect(parsed.Rotations.Length == 16, "migration creates first 4x4 board");
        Expect(parsed.Choices.Length == 6, "original history retained");
        Expect(parsed.Export().version == WorldModel.Version, "migrated snapshot uses v2");

        var legacyPartial = WorldModel.New().Export();
        legacyPartial.version = 1;
        Expect(WorldModel.TryRestore(legacyPartial, out parsed), "v1 incomplete progress upgrades");
        Expect(parsed.Round == 0 && parsed.Rotations.Length == 9, "incomplete board survives upgrade");
    }

    private static void StoryAndCompleteRun()
    {
        WorldModel world = WorldModel.New();
        Solve(world);
        Expect(!world.Rotate(0), "no extra turns after win");
        Expect(world.CanUndo && world.Undo() && !world.Solved,
            "solved board can be undone without losing progress");
        Solve(world);
        Expect(world.Choose(VillageChoice.ShadeTree), "tree choice commits");
        Expect(world.Round == 1 && world.Count(VillageChoice.ShadeTree) == 1, "one tree remembered");
        Expect(world.Memory.Contains("Mira"), "Mira recalls tree");
        Expect(!world.Choose(VillageChoice.Bridge), "cannot choose twice without solving");

        Expect(world.ReconsiderLatestChoice(), "last garden choice can be reconsidered");
        Expect(world.Round == 0 && world.Solved &&
            world.Choices.Length == 0, "undo choice reopens solved board without history");
        Expect(world.Choose(VillageChoice.ShadeTree), "corrected choice can recommit");
        Expect(world.Round == 1 && world.Choices.Length == 1,
            "corrected history contains only one action");

        Solve(world);
        Expect(world.Choose(VillageChoice.Bridge), "bridge choice commits");
        Expect(VillageStory.HasShadedCrossing(world), "two decisions create a new landmark");
        Expect(world.Consequence.Contains("crossing"), "player sees combined effect");
        Solve(world);
        Expect(world.Choose(VillageChoice.Flowers), "wildflower choice commits");
        Expect(VillageStory.HasButterflyGrove(world) &&
               VillageStory.HasCommunityWalk(world) &&
               VillageStory.HasLivingHeart(world), "three decisions create interlocking systems");
        Expect(world.Characters.Length == 3, "NPC trio available");
        Expect(world.Journal.Length == 3, "ordered history visible");
        Expect(VillageStory.MemoryEvent(world).Contains("CHAPTER 3"),
            "memory interlude references exact chapter");

        WorldSnapshot saved = world.Export();
        WorldModel loaded;
        Expect(WorldModel.TryRestore(saved, out loaded) && loaded.Round == 3,
            "saved progress round trips");
        Expect(loaded.Consequence == world.Consequence, "persistent consequence deterministic");
        for (int chapter = 3; chapter < PuzzleCatalog.Count; chapter++)
        {
            Solve(loaded);
            Expect(loaded.Choose((VillageChoice)(chapter % 3)),
                "chapter progression and consequence");
        }
        Expect(loaded.Completed, "12 streams completed");
        Expect(loaded.Choices.Length == 12 && loaded.Journal.Length == 12,
            "all memories recorded");
        Expect(loaded.Ending.Contains("VILLAGE WE GREW"),
            "balanced choices create the shared-world ending");
        Expect(!loaded.Rotate(0) && !loaded.Choose(VillageChoice.ShadeTree),
            "completed world cannot double collect choices");
        Expect(loaded.Export().rotations.Length == 0, "completed snapshot has no board");
        Expect(WorldModel.TryRestore(loaded.Export(), out world) && world.Completed,
            "end-game round trip consistent");

        var solo = WorldModel.New(PlayMode.PuzzleOnly);
        Solve(solo);
        Expect(!solo.Choose(VillageChoice.Flowers), "control cannot alter world");
        Expect(solo.NextPuzzle(), "control advances without a story action");
        Expect(solo.Round == 1 && solo.Choices.Length == 0,
            "control keeps identical puzzles and no memories");
    }

    private static void ScenicCausality()
    {
        var world = WorldModel.New();
        VillageScenePlan initial = VillageScenePlan.From(world);
        Expect(initial.TreeCount == 0 && initial.CrossingCount == 0 &&
               initial.FlowerCount == 0, "fresh scene is undecorated");
        Expect(!initial.WaterRestored, "new world begins with a dry creek");
        Expect(!initial.HasBridge && !initial.HasButterflies &&
               !initial.HasVisitors && !initial.NeighborsGather,
               "scenic elements do not appear before related choices");
        Expect(initial.LandscapePhase == 0, "first chapter uses the first landscape");
        string fingerprint = initial.Fingerprint;

        Solve(world);
        Expect(VillageScenePlan.From(world).WaterRestored,
               "water appears the moment the first stream puzzle is solved");
        Expect(world.Choose(VillageChoice.ShadeTree), "first tree grows");
        VillageScenePlan tree = VillageScenePlan.From(world);
        Expect(tree.WaterRestored && tree.TreeCount == 1 && !tree.HasBridge && !tree.HasVisitors,
               "tree alone visibly changes only tree assets");
        Expect(tree.Fingerprint != fingerprint, "scene fingerprint changes after an action");

        Solve(world);
        Expect(world.Choose(VillageChoice.Flowers), "meadow grows");
        VillageScenePlan meadow = VillageScenePlan.From(world);
        Expect(meadow.HasButterflies && !meadow.HasVisitors,
               "butterflies require tree plus meadow, not a bridge");

        Solve(world);
        Expect(world.Choose(VillageChoice.Bridge), "crossing opens");
        VillageScenePlan village = VillageScenePlan.From(world);
        Expect(village.HasBridge && village.HasButterflies &&
               village.HasVisitors && village.NeighborsGather,
               "combined actions unlock gathering neighbors and visitors");
        Expect(village.LandscapePhase == 1, "visual seasons respond to chapter progression");

        WorldModel recovered;
        Expect(WorldModel.TryRestore(world.Export(), out recovered),
               "valid saved world recovers for scenic comparison");
        Expect(village.Fingerprint == VillageScenePlan.From(recovered).Fingerprint,
               "reloading cannot scramble the rendered world");
        Expect(world.Characters.Length == 3 &&
               world.Consequence.Contains("shady bridge"),
               "human-readable narrative explains the visual interaction");

        string mira = VillageStory.Speak(world, 0);
        string orin = VillageStory.Speak(world, 1);
        string fen = VillageStory.Speak(world, 2);
        Expect(VillageStory.VillagerName(0).Contains("Mira") &&
               VillageStory.VillagerName(1).Contains("Orin") &&
               VillageStory.VillagerName(2).Contains("Fen"),
               "all three neighbors have stable identities");
        Expect(mira.Contains("shade") && orin.Contains("crossing") &&
               fen.Contains("wildflower"),
               "different villagers recall consequences through their own concerns");
        Expect(mira != orin && mira != fen && orin != fen,
               "three distinct voices do not share generic generated dialogue");
        Expect(VillageStory.Speak(recovered, 0) == mira &&
               VillageStory.Speak(recovered, 1) == orin &&
               VillageStory.Speak(recovered, 2) == fen,
               "authored dialogue remains stable across save/load");
        var untouched = WorldModel.New();
        Expect(VillageStory.Speak(untouched, 0).Contains("sapling") &&
               !VillageStory.Speak(untouched, 1).Contains("the first plank"),
               "characters do not fabricate prior actions in a new game");

        var control = WorldModel.New(PlayMode.PuzzleOnly);
        VillageScenePlan emptyControl = VillageScenePlan.From(control);
        Expect(emptyControl.Fingerprint == initial.Fingerprint,
               "puzzle-only control begins with identical scenery");
    }

    private static void ReactiveCampaign()
    {
        for (int chapter = 1; chapter < PuzzleCatalog.Count; chapter++)
        {
            for (int variant = 1; variant <= 3; variant++)
            {
                Puzzle puzzle = ReactiveWatercourses.ForChapter(chapter, variant);
                Expect(puzzle.AnchoredTile >= 0, "a future habitat is physically protected");
                Expect(puzzle.IsAnchored(puzzle.AnchoredTile), "landmark is readable in puzzle contract");
                Expect(puzzle.Initial[puzzle.AnchoredTile] == puzzle.Solution[puzzle.AnchoredTile],
                    "protected habitat begins aligned");
                Expect(PipeRules.ValidRotations(puzzle, puzzle.Initial), "branch begins valid");
                Expect(!PipeRules.IsComplete(puzzle, puzzle.Initial), "branch does not autosolve");
                Expect(PipeRules.IsComplete(puzzle, puzzle.Solution), "branch has full water path");
                Expect(PipeRules.CountSolutions(puzzle, 2) == 1, "every branch retains unique solution");
                int[] changed = (int[])puzzle.Solution.Clone();
                changed[puzzle.AnchoredTile] = changed[puzzle.AnchoredTile] %
                    PipeRules.OrientationCount(puzzle.Kinds[puzzle.AnchoredTile]) + 1;
                Expect(!PipeRules.ValidRotations(puzzle, changed),
                    "no mutation of protected habitats is permitted");
            }
        }

        string[] fingerprints = new string[3];
        for (int choiceIndex = 0; choiceIndex < 3; choiceIndex++)
        {
            var world = WorldModel.New();
            Solve(world);
            Expect(world.Choose((VillageChoice)choiceIndex), "player chooses ecological priority");
            Expect(world.Round == 1 && world.BoardVariant == choiceIndex + 1,
                "the next puzzle depends on player choice");
            Puzzle variantBoard = world.CurrentPuzzle;
            fingerprints[choiceIndex] = variantBoard.Name + ":" + variantBoard.AnchoredTile +
                ":" + string.Join(",", variantBoard.Solution);
            Expect(!world.Rotate(variantBoard.AnchoredTile),
                "immutable habitat tiles ignore player taps");
            WorldModel restored;
            Expect(WorldModel.TryRestore(world.Export(), out restored) &&
                restored.CurrentPuzzle.Name == variantBoard.Name &&
                restored.BoardVariant == world.BoardVariant,
                "save/load retains branch topology exactly");
            int[] rotations = world.Rotations;
            Expect(world.Restart() && string.Join(",", rotations) ==
                string.Join(",", world.Rotations),
                "restart preserves the ecology-shaped board");
            Solve(world);
            Expect(world.ReconsiderLatestChoice(), "reconsider opens solved previous habitat");
            Expect(world.Round == 0 && world.BoardVariant == 0 && world.Solved,
                "reconsider returns to exact first authored board");
            Expect(world.Choose((VillageChoice)choiceIndex), "reapply world decision");
            Expect(world.CurrentPuzzle.Name == variantBoard.Name,
                "same choices lead to same future route");
        }
        Expect(fingerprints[0] != fingerprints[1] && fingerprints[1] != fingerprints[2] &&
            fingerprints[0] != fingerprints[2],
            "all three ecological decisions yield different watercourse topology");

        var control = WorldModel.New(PlayMode.PuzzleOnly);
        Solve(control);
        Expect(control.NextPuzzle() && control.BoardVariant == 0 &&
            control.CurrentPuzzle.AnchoredTile == -1,
            "puzzle-only control still plays unchanged authored fixtures");

        // Existing v2 players must keep the unfinished puzzle, regardless
        // of previously recorded decisions. Branches start after NEXT choice.
        var legacy = new WorldSnapshot {
            version = 2, mode = "living", round = 1, solved = false,
            rotations = (int[])PuzzleCatalog.Get(1).Initial.Clone(),
            previous = Array.Empty<int>(), choices = new[] { 0 }
        };
        WorldModel migrated;
        Expect(WorldModel.TryRestore(legacy, out migrated) &&
            migrated.BoardVariant == 0 && migrated.Round == 1,
            "v2 in-progress watercourse is not transformed on upgrade");
        Solve(migrated);
        Expect(migrated.Choose(VillageChoice.Flowers) &&
            migrated.BoardVariant == 3, "next choice activates new branching rule");
        Expect(migrated.ReconsiderLatestChoice() && migrated.Round == 1 &&
            migrated.BoardVariant == 0 && migrated.Solved,
            "reconsider after migration reopens the unchanged legacy route");
        WorldSnapshot corrupt = migrated.Export();
        corrupt.boardVariant = 3;
        WorldModel invalid;
        Expect(!WorldModel.TryRestore(corrupt, out invalid),
            "tampering with variant inconsistent with history fails closed");
    }

    private static void PrivatePlaytestReport()
    {
        var notes = new LocalPlaytest(PlayMode.Living, false, 0, 10.0);
        string empty = notes.Report(10.0);
        Expect(empty.Contains("first_tap_seconds=not_observed"),
            "empty private session never invents first moves");
        Expect(empty.Contains("condition=LivingWorld") &&
               empty.Contains("visual=Storybook"),
            "playtest defaults reflect consent-time configuration");
        notes.Rotate(11.5);
        notes.Rotate(13.5);
        notes.Hint(20.0);
        notes.Undo(21.0);
        notes.Restart(22.0);
        notes.Solve(49.0);
        notes.Choose(50.0);
        notes.Reconsider(51.0);
        notes.Talk(53.0);
        notes.ChangeVisual(true, 54.0);
        notes.ChangeMode(PlayMode.PuzzleOnly, 55.0);
        string report = notes.Report(60.0);
        Expect(report.Contains("session_seconds=50.0") &&
               report.Contains("first_tap_seconds=1.5") &&
               report.Contains("first_solve_seconds=39.0"),
            "times are relative and invariant-culture formatted");
        Expect(report.Contains("rotations=2") && report.Contains("hints=1") &&
               report.Contains("undos=1") && report.Contains("puzzle_restarts=1"),
            "local research counters count only observed actions");
        Expect(report.Contains("village_choices=1") &&
               report.Contains("choice_corrections=1") &&
               report.Contains("npc_conversations_opened=1"),
            "counters include consequential actions");
        Expect(report.Contains("visual_mode_switches=1") &&
               report.Contains("experiment_condition_switches=1") &&
               report.Contains("condition=PuzzleOnly"),
            "condition switching remains disclosed in report");
        Expect(!report.Contains("device_id") && !report.Contains("email=") &&
               !report.Contains("latitude") && !report.Contains("IP="),
            "report contains no device identifiers or personal location");
        bool rejected = false;
        try { notes.Rotate(9.0); }
        catch (ArgumentOutOfRangeException) { rejected = true; }
        Expect(rejected, "invalid monotonic time is rejected");
    }

    public static int Main()
    {
        try
        {
            ContentAndRules();
            RestoreAndMigration();
            StoryAndCompleteRun();
            ScenicCausality();
            ReactiveCampaign();
            PrivatePlaytestReport();
            Console.WriteLine("Evergrove C# tests passed: " + assertions + " assertions");
            return 0;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine(ex);
            return 1;
        }
    }
}
