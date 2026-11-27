using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
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
        // The current hint is an explicit mutating one-tile alignment, not
        // merely a highlight or a single clockwise turn.
        int[] beforeHint = initial.Rotations;
        int firstWrong = -1;
        for (int tile = 0; tile < initial.CurrentPuzzle.CellCount; tile++)
            if (beforeHint[tile] != initial.CurrentPuzzle.Solution[tile])
            {
                firstWrong = tile;
                break;
            }
        Expect(firstWrong >= 0, "there is an incorrect tile to guide");
        Expect(initial.GuideOneConnection(), "optional hint aligns exactly one tile");
        int[] afterHint = initial.Rotations;
        Expect(afterHint[firstWrong] == initial.CurrentPuzzle.Solution[firstWrong],
            "hint sets the selected tile to full solution orientation");
        for (int tile = 0; tile < beforeHint.Length; tile++)
            if (tile != firstWrong)
                Expect(afterHint[tile] == beforeHint[tile], "hint never silently changes another tile");
        Expect(initial.HintsUsed == 1, "hint count recorded");
        Expect(initial.Undo(), "guided tile can also be undone");
        for (int tile = 0; tile < beforeHint.Length; tile++)
            Expect(initial.Rotations[tile] == beforeHint[tile], "hint undo restores original rotations");
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
        Expect(parsed.Export().version == WorldModel.Version, "migrated snapshot uses current v4");

        var legacyPartial = WorldModel.New().Export();
        legacyPartial.version = 1;
        Expect(WorldModel.TryRestore(legacyPartial, out parsed), "v1 incomplete progress upgrades");
        Expect(parsed.Round == 0 && parsed.Rotations.Length == 9, "incomplete board survives upgrade");
    }

    private static void StoryAndCompleteRun()
    {
        WorldModel world = WorldModel.New();
        Expect(LocalizedVillage.Consequence(world) == GameLocalization.Text("world_none"),
            "a new unsolved creek truthfully waits for water");
        Solve(world);
        Expect(LocalizedVillage.Consequence(world) == GameLocalization.Text("solved"),
            "the first solved creek must not still claim to be waiting for water");
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
        Expect(!initial.WaterRestored, "new world has not restored the creek");
        Expect(initial.WaterReachPercent > 0 && initial.WaterReachPercent < 100,
            "spring-connected inlet is partially wet even before full restoration");
        var firstTap = WorldModel.New();
        int beforeReach = VillageScenePlan.From(firstTap).WaterReachPercent;
        Expect(firstTap.Rotate(6), "single first-chapter tile rotates");
        VillageScenePlan afterTap = VillageScenePlan.From(firstTap);
        Expect(afterTap.WaterReachPercent > beforeReach && !afterTap.WaterRestored,
            "connecting another tile advances the creek before the puzzle is solved");
        Expect(firstTap.Undo() && VillageScenePlan.From(firstTap).WaterReachPercent == beforeReach,
            "undo retracts only the water that became disconnected");
        Expect(firstTap.Export().choices.Length == 0,
            "partial water does not fabricate a village decision");
        Expect(!initial.HasBridge && !initial.HasButterflies &&
               !initial.HasVisitors && !initial.NeighborsGather,
               "scenic elements do not appear before related choices");
        Expect(initial.LandscapePhase == 0, "first chapter uses the first landscape");
        string fingerprint = initial.Fingerprint;

        Solve(world);
        Expect(VillageScenePlan.From(world).WaterRestored &&
               VillageScenePlan.From(world).WaterReachPercent == 100,
               "water reaches the full village only after the first stream is solved");
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
            Expect(!string.IsNullOrWhiteSpace(LocalizedVillage.EnvironmentalRule(world)) &&
                   LocalizedVillage.EnvironmentalRule(world).Contains("cannot rotate"),
                   "fixed ecological tile has understandable English rule");
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

    private static void ProgressiveHintAndSaveV4()
    {
        var world = WorldModel.New();
        int[] original = world.Rotations;
        Expect(world.Export().version == 4 && world.HintStep == 0 && world.HintTile == -1,
            "new saves have v4 hint fields with an idle sentinel");
        int chosen = HintPlanner.SelectTarget(world.CurrentPuzzle, original);
        Expect(chosen >= 0 && !world.CurrentPuzzle.IsAnchored(chosen) &&
               original[chosen] != world.CurrentPuzzle.Solution[chosen],
            "hint chooses a genuinely incorrect, movable tile");
        Expect(world.AdvanceGentleHint() == 1 && world.HintStep == 1 &&
               world.HintTile == chosen && world.HintsUsed == 0,
            "first hint identifies a row without rotating tiles");
        Expect(string.Join(",", original) == string.Join(",", world.Rotations),
            "noticing does not secretly solve puzzle");
        WorldModel loaded;
        Expect(WorldModel.TryRestore(world.Export(), out loaded) && loaded.HintStep == 1 &&
               loaded.HintTile == chosen, "first hint survives save/load");
        Expect(loaded.AdvanceGentleHint() == 2 && loaded.HintStep == 2 &&
               loaded.HintsUsed == 0,
            "second hint focuses a tile without granting free rotations");
        Expect(WorldModel.TryRestore(loaded.Export(), out world) && world.HintStep == 2,
            "focus stage survives interruption and restores precisely");
        Expect(world.AdvanceGentleHint() == 3 &&
               world.Rotations[chosen] == world.CurrentPuzzle.Solution[chosen] &&
               world.HintStep == 0 && world.HintTile == -1 && world.HintsUsed == 1,
            "third requested hint corrects precisely one targeted connection");
        Expect(world.Undo() && string.Join(",", world.Rotations) ==
               string.Join(",", original) && world.HintsUsed == 1,
            "hint undo restores the old board but never refunds help count");
        Expect(world.AdvanceGentleHint() == 1 && world.HintStep == 1,
            "next help session starts at notice, not an automatic correction");
        Expect(world.Rotate(chosen) && world.HintStep == 0 && world.HintTile == -1,
            "manual turn resets staged guidance");
        Expect(world.AdvanceGentleHint() == 1 && world.Restart() &&
               world.HintStep == 0 && world.HintTile == -1,
            "restart clears stale hint selection");

        WorldSnapshot corrupt = world.Export();
        corrupt.hintStep = 2;
        corrupt.hintTile = -1;
        Expect(!WorldModel.TryRestore(corrupt, out loaded),
            "v4 hint stages without a real tile fail closed");
        corrupt = world.Export();
        corrupt.hintStep = 3;
        Expect(!WorldModel.TryRestore(corrupt, out loaded),
            "v4 forged final hint stage rejected");
        corrupt = world.Export();
        corrupt.hintTile = 0;
        Expect(!WorldModel.TryRestore(corrupt, out loaded),
            "idle hint must not smuggle an active target");

        for (int chapter = 0; chapter < PuzzleCatalog.Count; chapter++)
            for (int variant = 0; variant <= (chapter == 0 ? 0 : 3); variant++)
            {
                Puzzle board = ReactiveWatercourses.ForChapter(chapter, variant);
                int selected = HintPlanner.SelectTarget(board, board.Initial);
                Expect(selected >= 0 && !board.IsAnchored(selected) &&
                    board.Initial[selected] != board.Solution[selected],
                    "all 45 boards have valid movable hint target");
                Expect(HintPlanner.SelectTarget(board, board.Solution) == -1,
                    "solved puzzle has no misleading hint");
            }
        // v3 saves must preserve ecological variant and be upgraded to v4
        // without relying on fields absent from the old on-disk schema.
        var prior = WorldModel.New();
        Solve(prior);
        Expect(prior.Choose(VillageChoice.Bridge), "old v3 world can have a bridge variant");
        prior.AdvanceGentleHint();
        WorldSnapshot v3 = prior.Export();
        v3.version = 3;
        v3.hintStep = 2; // simulate ignored new fields, absent from real v3 JSON
        v3.hintTile = v3.boardVariant;
        Expect(WorldModel.TryRestore(v3, out loaded) && loaded.BoardVariant == 2 &&
               loaded.HintStep == 0 && loaded.HintTile == -1 &&
               loaded.CurrentPuzzle.Name == prior.CurrentPuzzle.Name,
            "v3 choice-conditioned active board survives migration unchanged");
        Expect(loaded.Export().version == 4, "old v3 session upgrades to v4 schema");

        // Wrong targets, protected habitat and solved states never allow hint.
        WorldSnapshot invalid = loaded.Export();
        invalid.hintStep = 1;
        invalid.hintTile = loaded.CurrentPuzzle.AnchoredTile;
        Expect(!WorldModel.TryRestore(invalid, out world),
            "staged hints may never target ecological protected tiles");
        var solved = WorldModel.New();
        Solve(solved);
        Expect(solved.AdvanceGentleHint() == 0,
            "no assistance allowed after water puzzle is already solved");
    }

    // Memory cards and journal lines must use the active locale's NPC name,
    // not the Latin source name.
    private static void LocalizedCharacterNamesInMemories()
    {
        string file = Path.Combine(FindLocaleDirectory(), "ja.json");
        TranslationBundle bundle = JsonSerializer.Deserialize<TranslationBundle>(
            File.ReadAllText(file), new JsonSerializerOptions { IncludeFields = true });
        Expect(GameLocalization.Register(bundle) && GameLocalization.SetCurrent("ja"),
            "Japanese catalog with character names activates");
        try
        {
            var names = new[] { "ミラ", "オリン", "フェン" };
            var choices = new[] { VillageChoice.ShadeTree, VillageChoice.Bridge, VillageChoice.Flowers };
            for (int i = 0; i < choices.Length; i++)
            {
                var world = WorldModel.New();
                Solve(world);
                Expect(world.Choose(choices[i]), "localized name source action");
                string journal = LocalizedVillage.Journal(world)[0];
                string memory = LocalizedVillage.MemoryEvent(world);
                Expect(journal.Contains(names[i]) && memory.Contains(names[i]),
                    "memory and journal show the localized name " + names[i]);
                Expect(!journal.Contains("Mira") && !journal.Contains("Orin") &&
                       !journal.Contains("Fen") && !memory.Contains("Mira") &&
                       !memory.Contains("Orin") && !memory.Contains("Fen"),
                    "no Latin NPC name leaks into a Japanese memory card");
                Expect(LocalizedVillage.Name(i) == names[i], "Name matches the catalog NPC name");
            }
        }
        finally
        {
            GameLocalization.SetCurrent("en");
        }
        Expect(GameLocalization.Text("name_mira") == "Mira",
            "English source keeps Mira after the locale is restored");
    }

    private static void NonColorLeaksAndCausalJournal()
    {
        for (int chapter = 0; chapter < PuzzleCatalog.Count; chapter++)
            for (int variant = 0; variant <= (chapter == 0 ? 0 : 3); variant++)
            {
                Puzzle board = ReactiveWatercourses.ForChapter(chapter, variant);
                PipeInsight first = PipeRules.Inspect(board, board.Initial);
                PipeInsight finished = PipeRules.Inspect(board, board.Solution);
                Expect(first.Leaking.Length == board.CellCount &&
                       finished.Leaking.Length == board.CellCount,
                    "noncolor leak flags exist for every tile across all 45 boards");
                bool anyMarked = false;
                for (int i = 0; i < board.CellCount; i++)
                {
                    if (first.Leaking[i]) anyMarked = true;
                    Expect(!finished.Leaking[i], "complete route has no misleading exclamation");
                }
                Expect(first.LeakCount == 0 || anyMarked,
                    "every nonzero leak count has a visible noncolor tile marker");
                Expect(finished.LeakCount == 0, "win has no unresolved leaks");
            }

        var world = WorldModel.New();
        Solve(world);
        Expect(world.Choose(VillageChoice.ShadeTree), "journal source action tree");
        string[] log = LocalizedVillage.Journal(world);
        Expect(log.Length == 1 && log[0].Contains("Mira") &&
               log[0].Contains("young tree") && log[0].Contains("roots"),
            "latest tree action records identity, true scene and active puzzle consequence");
        Solve(world);
        Expect(world.Choose(VillageChoice.Bridge), "journal source action crossing");
        log = LocalizedVillage.Journal(world);
        Expect(log.Length == 2 && log[0].Contains("young tree") &&
               !log[0].Contains("roots") && log[1].Contains("Orin") &&
               log[1].Contains("crossing") && log[1].Contains("foundations"),
            "historical event keeps true scene effect; only newest active branch is asserted");
        WorldModel restored;
        Expect(WorldModel.TryRestore(world.Export(), out restored) &&
               string.Join("|", LocalizedVillage.Journal(restored)) == string.Join("|", log),
            "causal journal stable across save and reload");

        var old = new WorldSnapshot {
            version = 3, mode = "living", round = 1, solved = false,
            rotations = (int[])PuzzleCatalog.Get(1).Initial.Clone(),
            previous = Array.Empty<int>(), choices = new[] { 0 },
            boardVariant = 0, lastSolvedVariant = 0
        };
        Expect(WorldModel.TryRestore(old, out restored),
            "old v3 nonbranching active progress remains a valid migration");
        Expect(!LocalizedVillage.Journal(restored)[0].Contains("roots"),
            "old saves do not invent board changes that never happened");
    }

    private static void PuzzleDiagnosticsTests()
    {
        PuzzleProfile[] all = PuzzleDiagnostics.All();
        Expect(all.Length == 45, "12 base fixtures and 33 branch presentations");
        var ids = new System.Collections.Generic.HashSet<string>();
        foreach (PuzzleProfile profile in all)
        {
            Expect(ids.Add(profile.id), "stable unique metadata ID");
            Puzzle puzzle = ReactiveWatercourses.ForChapter(profile.chapter - 1, profile.variant);
            Expect(profile.solutionCountAtMostTwo == 1, "all catalog boards uniquely solvable");
            Expect(profile.size == puzzle.Size && profile.initialOrientations.Length == puzzle.CellCount,
                "metadata reflects actual board size and arrays");
            Expect(profile.minimumClockwiseTaps > 0 && profile.initiallyMisalignedTiles > 0 &&
                profile.minimumClockwiseTaps >= profile.initiallyMisalignedTiles,
                "minimum clockwise taps derived from exact source orientations");
            int taps = 0;
            for (int tile = 0; tile < puzzle.CellCount; tile++)
            {
                int[] copy = (int[])puzzle.Initial.Clone();
                while (copy[tile] != puzzle.Solution[tile])
                {
                    copy[tile] = copy[tile] % PipeRules.OrientationCount(puzzle.Kinds[tile]) + 1;
                    taps++;
                }
            }
            Expect(taps == profile.minimumClockwiseTaps,
                "independent clockwise tap count matches metadata");
            Expect(profile.forcedMoveCount == null && profile.observedMedianSeconds == null &&
                profile.observedP90Seconds == null && profile.observedHintRate == null &&
                profile.humanReviewer == null, "not-yet-observed human and logic metrics remain null");
            Expect((profile.anchoredTile.HasValue) == (profile.variant != 0),
                "only branch profiles have protected landmarks");
        }
        Expect(PuzzleDiagnostics.All()[0].id == "stream-01-base", "stable first catalog ID");
    }

    private static readonly JsonSerializerOptions CatalogOptions =
        new JsonSerializerOptions { IncludeFields = true, WriteIndented = true };

    private static bool CheckPuzzleCatalog()
    {
        const string path = "games/evergrove/docs/specs/puzzle-catalog.generated.json";
        if (!File.Exists(path))
        {
            Console.Error.WriteLine("Generated puzzle catalog missing: " + path);
            return false;
        }
        PuzzleProfile[] stored = JsonSerializer.Deserialize<PuzzleProfile[]>(
            File.ReadAllText(path), new JsonSerializerOptions { IncludeFields = true });
        return stored != null &&
            JsonSerializer.Serialize(stored, CatalogOptions) ==
            JsonSerializer.Serialize(PuzzleDiagnostics.All(), CatalogOptions);
    }

    private static DateTime Day(int year, int month, int day)
    {
        return new DateTime(year, month, day);
    }

    private static void ReturnDiaryTests()
    {
        // Nothing is recorded before explicit consent.
        var diary = new ReturnDiary();
        Expect(!diary.RecordOpen(Day(2026, 11, 24)) && diary.Days.Count == 0 && !diary.Consented,
            "no diary entry is recorded before consent");
        Expect(diary.Status(1, Day(2026, 11, 25)) == ReturnStatus.NotConsented,
            "unconsented diary reports no return status");
        Expect(diary.Export(Day(2026, 11, 24)).Contains("CONSENT: none; nothing recorded"),
            "unconsented export says nothing was recorded");

        // Consent starts the diary once; same-day opens are deduplicated.
        Expect(diary.GrantConsent(Day(2026, 11, 24)) && !diary.GrantConsent(Day(2026, 11, 26)),
            "consent is granted once");
        Expect(diary.FirstDay == Day(2026, 11, 24) && diary.Days.Count == 1,
            "first consented day is the only entry at consent");
        Expect(!diary.RecordOpen(Day(2026, 11, 24)), "same local date is not duplicated");
        Expect(diary.RecordOpen(Day(2026, 11, 25)) && !diary.RecordOpen(Day(2026, 11, 25)),
            "a new local date is kept once");
        Expect(!diary.RecordOpen(Day(2026, 11, 20)), "dates before the first consented day are ignored");
        Expect(diary.Days.Count == 2, "dedupe keeps two distinct dates");

        // Cap: 60 entries, the first consented day is never dropped.
        var capped = new ReturnDiary();
        DateTime start = Day(2026, 1, 1);
        capped.GrantConsent(start);
        for (int offset = 1; offset <= 69; offset++) capped.RecordOpen(start.AddDays(offset));
        Expect(capped.Days.Count == ReturnDiary.MaxEntries, "diary is capped at 60 entries");
        Expect(capped.Days[0] == start, "first consented day survives the cap");
        Expect(capped.Days[1] == start.AddDays(11), "oldest non-first entries are dropped first");
        Expect(capped.Days[capped.Days.Count - 1] == start.AddDays(69) && capped.Truncated,
            "cap keeps the newest date and reports truncation");

        // Delete is one action and leaves no residue.
        var deleted = new ReturnDiary();
        deleted.GrantConsent(Day(2026, 11, 24));
        deleted.RecordOpen(Day(2026, 11, 25));
        deleted.Delete();
        Expect(!deleted.Consented && deleted.Days.Count == 0 && !deleted.Truncated,
            "delete clears consent and every recorded date");
        Expect(!deleted.RecordOpen(Day(2026, 11, 26)), "deleted diary records nothing until new consent");
        Expect(!ReturnDiary.Parse(deleted.Serialize()).Consented, "deleted state round-trips as unconsented");

        // D1/D7 across month, year and leap-day boundaries.
        var yearEnd = new ReturnDiary();
        yearEnd.GrantConsent(Day(2026, 12, 30));
        yearEnd.RecordOpen(Day(2026, 12, 31));
        Expect(yearEnd.Status(1, Day(2027, 1, 1)) == ReturnStatus.Returned,
            "day 1 crosses the year boundary");
        Expect(yearEnd.Status(7, Day(2027, 1, 5)) == ReturnStatus.NotYetDue,
            "day 7 is not due before its date");
        Expect(yearEnd.Status(7, Day(2027, 1, 6)) == ReturnStatus.NotReturned,
            "day 7 is not returned when no open falls on it");
        yearEnd.RecordOpen(Day(2027, 1, 6));
        Expect(yearEnd.Status(7, Day(2027, 1, 6)) == ReturnStatus.Returned
            && yearEnd.ReturnedWithin(7), "day 7 crosses the year boundary");
        var leap = new ReturnDiary();
        leap.GrantConsent(Day(2028, 2, 28));
        Expect(leap.Status(1, Day(2028, 2, 28)) == ReturnStatus.NotYetDue,
            "day 1 is not due on the consent day");
        Expect(leap.Status(1, Day(2028, 2, 29)) == ReturnStatus.NotReturned,
            "leap day is a real calendar day for day 1");
        leap.RecordOpen(Day(2028, 2, 29));
        Expect(leap.Status(1, Day(2028, 2, 29)) == ReturnStatus.Returned,
            "leap-day open counts as day 1");
        Expect(leap.Status(7, Day(2028, 3, 5)) == ReturnStatus.NotYetDue
            && leap.Status(7, Day(2028, 3, 6)) == ReturnStatus.NotReturned,
            "day 7 crosses a leap February");

        // Stable export format.
        var sample = new ReturnDiary();
        sample.GrantConsent(Day(2026, 11, 24));
        sample.RecordOpen(Day(2026, 11, 25));
        sample.RecordOpen(Day(2026, 11, 30));
        string expected =
            "EVERGROVE RETURN DIARY v1\n" +
            "CONSENT: opted in on this device; local calendar dates only; no times, identifiers or game content; exported only by the participant's copy action\n" +
            "first_day=2026-11-24\n" +
            "opened_days=3\n" +
            "days=2026-11-24,2026-11-25,2026-11-30\n" +
            "day1_return=returned\n" +
            "day7_return=not_returned\n" +
            "returned_within_7_days=yes\n" +
            "truncated=no\n" +
            "LIMIT: local dates only; does not measure identity, cohorts, install source, satisfaction or causality\n";
        Expect(sample.Export(Day(2026, 12, 2)) == expected, "export format is stable");
        Expect(sample.Export(Day(2026, 11, 26)).Contains("day7_return=not_yet_due")
            && sample.Export(Day(2026, 11, 26)).Contains("returned_within_7_days=yes"),
            "export reports pending day 7 before its date");

        // Storage round-trip, and malformed storage falls back to unconsented.
        ReturnDiary restored = ReturnDiary.Parse(sample.Serialize());
        Expect(restored.Consented && restored.Days.Count == 3 && restored.Export(Day(2026, 12, 2)) == expected,
            "serialized diary restores exactly");
        Expect(!ReturnDiary.Parse("v1;first=2026-11-24;days=2026-11-25;truncated=0").Consented,
            "stored days must start with the first consented day");
        Expect(!ReturnDiary.Parse("v1;first=2026-11-24;days=2026-11-24,2026-11-24;truncated=0").Consented,
            "stored days must be strictly ascending");
        Expect(!ReturnDiary.Parse("v1;first=2026-13-01;days=2026-13-01;truncated=0").Consented,
            "invalid stored dates are rejected");
        Expect(!ReturnDiary.Parse("garbage").Consented && !ReturnDiary.Parse(null).Consented,
            "garbage storage is unconsented");
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

    private static string FindLocaleDirectory()
    {
        DirectoryInfo current = new DirectoryInfo(Directory.GetCurrentDirectory());
        while (current != null)
        {
            string at = Path.Combine(current.FullName, "games", "evergrove",
                "Assets", "Resources", "Evergrove", "Locales");
            if (Directory.Exists(at)) return at;
            current = current.Parent;
        }
        throw new Exception("Could not find checked-in locale assets");
    }

    // Fixed-width wrap used as a stand-in for the engine's measured line starts.
    private static List<int> FixedWidthLines(string display, int width)
    {
        var starts = new List<int> { 0 };
        int column = 0;
        for (int index = 0; index < display.Length; index++)
        {
            if (display[index] == '\n')
            {
                starts.Add(index + 1);
                column = 0;
                continue;
            }
            if (column == width)
            {
                starts.Add(index);
                column = 0;
            }
            column++;
        }
        return starts;
    }

    private static string Reversed(string value)
    {
        char[] chars = value.ToCharArray();
        Array.Reverse(chars);
        return new string(chars);
    }

    private static void IndicVisualOrderTests()
    {
        // ো = U+09C7 + U+09BE: pre-base part moves before the consonant, post-base stays.
        Expect(IndicVisualOrder.Visual("bn", "\u09B9\u09B2\u09CB") == "\u09B9\u09C7\u09B2\u09BE",
            "holo (হলো) becomes visual order হেলা");
        // ি (pre-base) goes before the consonant.
        Expect(IndicVisualOrder.Visual("bn", "\u0995\u09BF") == "\u09BF\u0995", "i-kar precedes consonant");
        // ে and ৈ go before the consonant.
        Expect(IndicVisualOrder.Visual("bn", "\u0995\u09C7") == "\u09C7\u0995", "e-kar precedes consonant");
        Expect(IndicVisualOrder.Visual("bn", "\u0995\u09C8") == "\u09C8\u0995", "ai-kar precedes consonant");
        // ৌ = U+09C7 + U+09D7: pre-base part first, length mark after.
        Expect(IndicVisualOrder.Visual("bn", "\u0995\u09CC") == "\u09C7\u0995\u09D7", "au-kar split and reordered");
        // Post-base signs and independent text stay in logical order.
        Expect(IndicVisualOrder.Visual("bn", "\u0995\u09BE") == "\u0995\u09BE", "aa-kar untouched");
        Expect(IndicVisualOrder.Visual("bn", "\u0995\u09C1") == "\u0995\u09C1", "u-kar untouched");
        // Conjunct cluster with virama: pre-base sign precedes the whole cluster.
        Expect(IndicVisualOrder.Visual("bn", "\u0995\u09CD\u09B7\u09BF") == "\u09BF\u0995\u09CD\u09B7",
            "i-kar precedes the virama conjunct cluster");
        Expect(IndicVisualOrder.Visual("bn", "\u09B8\u09CD\u09A5\u09C7") == "\u09C7\u09B8\u09CD\u09A5",
            "e-kar precedes a virama conjunct");
        // Latin, digits and punctuation are untouched; Bengali word inside a Latin line is reordered only locally.
        Expect(IndicVisualOrder.Visual("bn", "MOSSELORA 12/3 (dev)") == "MOSSELORA 12/3 (dev)",
            "Latin and digits untouched");
        Expect(IndicVisualOrder.Visual("bn", "Level \u09B9\u09B2\u09CB 2") == "Level \u09B9\u09C7\u09B2\u09BE 2",
            "Bengali run reordered inside mixed text");
        // A sign with no consonant before it keeps its position.
        Expect(IndicVisualOrder.Visual("bn", "\u09BF") == "\u09BF", "orphan sign kept");
        Expect(IndicVisualOrder.Visual("bn", " \u09C7\u0995") == " \u09C7\u0995", "sign after space kept");
        // Multi-line: reordered line by line, line breaks preserved.
        Expect(IndicVisualOrder.Visual("bn", "\u0995\u09BF\n\u09B9\u09B2\u09CB") ==
               "\u09BF\u0995\n\u09B9\u09C7\u09B2\u09BE", "multi-line text ordered per line");
        Expect(IndicVisualOrder.Visual("bn", "\u0995\u09BF\r\n\u09B9\u09B2\u09CB") ==
               "\u09BF\u0995\r\n\u09B9\u09C7\u09B2\u09BE", "CRLF text keeps its line breaks");
        // Other locales are never changed.
        Expect(IndicVisualOrder.Visual("hi", "\u0995\u09BF") == "\u0995\u09BF", "other locales untouched");
        Expect(IndicVisualOrder.Visual("bn", "") == "" && IndicVisualOrder.Visual("bn", null) == "",
            "empty and null input pass through");

        // Every shipped Bengali catalog string: no throw, and only ো/ৌ change length.
        string file = Path.Combine(FindLocaleDirectory(), "bn.json");
        using (JsonDocument document = JsonDocument.Parse(File.ReadAllText(file)))
        {
            int checkedCount = 0;
            foreach (JsonElement entry in document.RootElement.GetProperty("entries").EnumerateArray())
            {
                string logical = entry.GetProperty("value").GetString() ?? "";
                string visual = null;
                bool threw = false;
                try { visual = IndicVisualOrder.Visual("bn", logical); } catch (Exception) { threw = true; }
                Expect(!threw && visual != null, "bn catalog string does not throw: " + logical);
                int twoPart = 0;
                foreach (char c in logical) if (c == '\u09CB' || c == '\u09CC') twoPart++;
                Expect(visual.Length == logical.Length + twoPart, "bn catalog string keeps its characters: " + logical);
                checkedCount++;
            }
            Expect(checkedCount > 0, "bn catalog has entries to check");
        }
    }

    private static void RtlTextTests()
    {
        Expect(RtlText.IsRtlScriptLocale("ar") && RtlText.IsRtlScriptLocale("fa") &&
               RtlText.IsRtlScriptLocale("ur") && RtlText.IsRtlScriptLocale("he"),
            "ar, fa, ur and he use RTL text preparation");

        // Regression: multi-line labels threw "Stack empty" in BidiSharp on every frame.
        string first = "ذكرى جديدة في القرية", second = "الفصل 1 · ميرا";
        string multi = null;
        bool threw = false;
        try { multi = RtlText.Visual(first + "\n" + second); } catch (Exception) { threw = true; }
        Expect(!threw, "multi-line RTL text does not throw");
        Expect(multi == RtlText.Visual(first) + "\n" + RtlText.Visual(second),
            "multi-line RTL text is reordered line by line");
        Expect(RtlText.Visual("שלום\r\nעולם") == RtlText.Visual("שלום") + "\n" + RtlText.Visual("עולם"),
            "CRLF line endings are handled per line");
        Expect(!RtlText.IsRtlScriptLocale("ja") && !RtlText.IsRtlScriptLocale("en") &&
               !RtlText.IsRtlScriptLocale("hi"), "other locales skip RTL preparation");

        // Arabic joining: sin takes the initial form, lam+alef form the final lam-alef ligature,
        // and meem is isolated because alef does not join forward.
        Expect(ArabicShaper.Shape("سلام") == "ﺳﻼﻡ", "Arabic word takes contextual forms and lam-alef");
        // Dual-joining letters in the middle of a word take the medial form.
        Expect(ArabicShaper.Shape("بتب") == "ﺑﺘﺐ","dual-joining letters take initial, medial and final forms");
        // Lam-alef ligature: isolated alone, final after a joining letter.
        Expect(ArabicShaper.Shape("لا") == "ﻻ", "lam-alef ligature stands alone");
        Expect(ArabicShaper.Shape("سلا") == "ﺳﻼ", "lam-alef ligature takes final form after a dual-joining letter");
        // Urdu tteh, isolated.
        Expect(ArabicShaper.Shape("ٹ") == "ﭦ", "Urdu letter uses its presentation form");
        // Hebrew is not joining; shaping leaves it unchanged.
        Expect(ArabicShaper.Shape("שלום") == "שלום", "Hebrew passes through shaping unchanged");

        // Bidi: a pure RTL word is drawn in reverse logical order.
        Expect(RtlText.Visual("שלום") == Reversed("שלום"), "Hebrew word is reversed for left-to-right drawing");
        Expect(RtlText.Visual("سلام") == Reversed(ArabicShaper.Shape("سلام")),
            "Arabic word is shaped and reversed");

        // Latin and digit runs keep their order inside RTL text.
        Expect(RtlText.Visual("MOSSELORA") == "MOSSELORA", "Latin run is not reversed");
        Expect(RtlText.Visual("1/12") == "1/12", "digits and separators are not reversed");
        Expect(RtlText.Visual("שלום MOSSELORA") == "MOSSELORA " + Reversed("שלום"),
            "Latin phrase after Hebrew stays left to right and moves to the visual start");
        Expect(RtlText.Visual("سلام 12") == "12 " + Reversed(ArabicShaper.Shape("سلام")),
            "Arabic number follows RTL base direction with digits kept in order");

        // Mirroring: brackets in an RTL run swap glyphs (UAX #9 L4).
        Expect(RtlText.Visual("(שלום)") == "(" + Reversed("שלום") + ")",
            "brackets around Hebrew are mirrored");

        // Empty input stays empty.
        Expect(RtlText.Visual("") == "", "empty line stays empty");
        // Latin-only captions keep LTR base direction, so their brackets are not mirrored.
        Expect(RtlText.Visual("Start matched control (dev)") == "Start matched control (dev)",
            "Latin-only line keeps its order and brackets");

        // Wrapping measures visual lines and keeps each line visually ordered.
        Func<string, float> charWidth = visual => visual.Length;
        string wrapped = RtlText.Wrap("שלום עולם", 5f, charWidth);
        Expect(wrapped == RtlText.Visual("שלום") + "\n" + RtlText.Visual("עולם"),
            "words that do not fit move to the next visual line");
        Expect(RtlText.Wrap("א\nב", 10f, charWidth) == RtlText.Visual("א") + "\n" + RtlText.Visual("ב"),
            "source newlines start new paragraphs");
        Expect(RtlText.Wrap("שלום עולם", 0f, charWidth) == RtlText.Visual("שלום עולם"),
            "non-positive width disables wrapping");
    }

    private static void KinsokuLineBreakTests()
    {
        Expect(KinsokuLineBreaks.IsCjkLocale("ja") && KinsokuLineBreaks.IsCjkLocale("zh-Hans") &&
               KinsokuLineBreaks.IsCjkLocale("zh-Hant"), "CJK locales get line-start rules");
        Expect(!KinsokuLineBreaks.IsCjkLocale("ko") && !KinsokuLineBreaks.IsCjkLocale("en"),
            "other locales keep plain wrapping");
        Expect(KinsokuLineBreaks.IsNoLineStart('。') && KinsokuLineBreaks.IsNoLineStart('、') &&
               !KinsokuLineBreaks.IsNoLineStart('あ'), "closing punctuation identified");
        Expect(KinsokuLineBreaks.IsNoLineEnd('「') && !KinsokuLineBreaks.IsNoLineEnd('。'),
            "opening bracket identified");

        // Source: あ0 い1 う2 え3 お4 。5 か6 き7 — line 2 starts with 。
        string sentence = "あいうえお。かき";
        var breaks = KinsokuLineBreaks.FindBreaksBefore(sentence, new[] { 0, 5 });
        Expect(breaks.Count == 1 && breaks[0] == 4, "closing mark moves the preceding character down");
        Expect(KinsokuLineBreaks.Insert(sentence, breaks) == "あいうえ\nお。かき",
            "insert places the break one character earlier");

        // Opening bracket at the end of a line moves down with the next character.
        string bracket = "さくら「ほ";
        var openerBreaks = KinsokuLineBreaks.FindBreaksBefore(bracket, new[] { 0, 4 });
        Expect(openerBreaks.Count == 1 && openerBreaks[0] == 3,
            "opening bracket at line end moves down with the break before it");

        // Never create an empty line, and never touch the first line.
        Expect(KinsokuLineBreaks.FindBreaksBefore("あ。い", new[] { 0, 1 }).Count == 0,
            "a single-character line is not emptied");
        Expect(KinsokuLineBreaks.FindBreaksBefore("あ\n。い", new[] { 0, 2 }).Count == 0,
            "an explicit newline is not doubled");
        Expect(KinsokuLineBreaks.FindBreaksBefore("日本語の文章", new[] { 0, 3 }).Count == 0,
            "ordinary wrap points are untouched");

        // Fit re-wraps with a fixed-width engine stand-in.
        string fitted = KinsokuLineBreaks.Fit(sentence, display => FixedWidthLines(display, 5));
        Expect(fitted == "あいうえ\nお。かき", "fit moves the punctuation off the line start");
        Expect(KinsokuLineBreaks.Fit(sentence, display => FixedWidthLines(display, 10)) == sentence,
            "text that already fits is unchanged");
        Expect(KinsokuLineBreaks.Fit("", display => FixedWidthLines(display, 5)) == "",
            "empty text stays empty");
        Expect(KinsokuLineBreaks.Fit(sentence, null) == sentence, "missing measurer keeps source");
        Expect(KinsokuLineBreaks.Fit(sentence, display => null) == sentence,
            "missing line starts keep source");

        // Two closing marks in a row: each pass moves one more character down.
        string run = "あいう。。えお";
        string runFit = KinsokuLineBreaks.Fit(run, display => FixedWidthLines(display, 3));
        string[] lines = runFit.Split('\n');
        foreach (string line in lines)
            Expect(line.Length == 0 || !KinsokuLineBreaks.IsNoLineStart(line[0]) || line == lines[0],
                "no wrapped line starts with closing punctuation");

        // Fit stops after MaxPasses even if the engine keeps reporting a bad start.
        int calls = 0;
        string bounded = KinsokuLineBreaks.Fit("あいうえおかきくけこ", display =>
        {
            calls++;
            return new List<int> { 0, 1, 2, 3, 4, 5, 6 };
        });
        Expect(calls <= KinsokuLineBreaks.MaxPasses, "fit is bounded by MaxPasses");
        Expect(bounded.Length >= "あいうえおかきくけこ".Length, "bounded fit keeps every character");
    }

    private static void LocalizationTests()
    {
        Expect(GameLocalization.Available.Count == 30, "exactly thirty research catalog choices");
        var consumer = LocaleShippingPolicy.Selectable(false);
        var research = LocaleShippingPolicy.Selectable(true);
        Expect(consumer.Count == 1 && consumer[0].Code == "en",
            "consumer source allowlist intentionally exposes only English candidate");
        Expect(research.Count == GameLocalization.Available.Count,
            "development/research builds can inspect all thirty draft catalogs");
        Expect(LocaleShippingPolicy.Allows("ja", true) &&
               LocaleShippingPolicy.Allows("ar", true) &&
               !LocaleShippingPolicy.Allows("ja", false) &&
               !LocaleShippingPolicy.Allows("ar", false),
            "uncertified draft locales cannot be selected in consumer policy");
        Expect(LocaleShippingPolicy.Resolve("ja", false) == "en" &&
               LocaleShippingPolicy.Resolve("ar", false) == "en" &&
               LocaleShippingPolicy.Resolve("ja", true) == "ja" &&
               LocaleShippingPolicy.Resolve("invalid", true) == "en",
            "old research preferences and auto-detected draft locales safely fall back");
        Expect(GameLocalization.Keys.Count == 121,"authoritative localization keys");
        Expect(GameLocalization.Normalize("zh-Hant-TW") == "zh-Hant", "traditional Chinese script");
        Expect(GameLocalization.Normalize("zh-TW") == "zh-Hant", "Taiwan defaults traditional");
        Expect(GameLocalization.Normalize("zh-HK") == "zh-Hant", "Hong Kong defaults traditional");
        Expect(GameLocalization.Normalize("zh-CN") == "zh-Hans", "mainland defaults simplified");
        Expect(GameLocalization.Normalize("pt-PT") == "pt-BR", "single Brazilian Portuguese catalog");
        Expect(GameLocalization.Normalize("fil-PH") == "fil", "Filipino region suffix");
        Expect(GameLocalization.Normalize("es-MX") == "es", "Spanish region suffix");
        Expect(GameLocalization.Normalize("tl-PH") == "fil", "Tagalog alias");
        Expect(GameLocalization.Normalize("unknown") == "en", "unsupported locales fall back");
        Expect(GameLocalization.Detect("ja", "fr-FR", "French") == "ja",
            "explicit user override beats device");
        Expect(GameLocalization.Detect("auto", "zh-Hant-HK", "Chinese") == "zh-Hant",
            "automatic selection respects language script");
        Expect(GameLocalization.Detect("auto", "xx-XX", "Japanese") == "ja",
            "system language fallback works when culture is unavailable");
        Expect(GameLocalization.Detect("auto", "en-US", "Japanese") == "ja",
            "Unity device language beats runtime default English culture on WebGL");
        Expect(GameLocalization.Detect("auto", "en-US", "English") == "en",
            "English is preserved when both platform signals agree");
        Expect(GameLocalization.IsRtl("ar") && GameLocalization.IsRtl("he") &&
               GameLocalization.IsRtl("fa") && GameLocalization.IsRtl("ur"),
               "four RTL language families flagged");
        Expect(!GameLocalization.IsRtl("en") && !GameLocalization.IsRtl("hi"),
            "LTR scripts not reversed");
        Expect(!GameLocalization.SetCurrent("xx"), "unknown locale cannot become active");

        string directory = FindLocaleDirectory();
        string[] assets = Directory.GetFiles(directory, "*.json");
        Expect(assets.Length == 29, "29 offline catalogs plus built-in English");
        int tested = 0;
        var options = new JsonSerializerOptions { IncludeFields = true };
        foreach (GameLanguage language in GameLocalization.Available)
        {
            if (language.Code == "en") continue;
            string file = Path.Combine(directory, language.Code + ".json");
            Expect(File.Exists(file), "locale file exists for " + language.Code);
            TranslationBundle bundle = JsonSerializer.Deserialize<TranslationBundle>(
                File.ReadAllText(file), options);
            Expect(bundle != null && bundle.code == language.Code,
                "declared locale matches file name");
            Expect(bundle.entries.Length == GameLocalization.Keys.Count,
                "catalog has a complete translation for " + language.Code);
            Expect(GameLocalization.Register(bundle), "all keys and placeholders valid for " + language.Code);
            Expect(GameLocalization.SetCurrent(language.Code), "language can be selected");
            Expect(GameLocalization.Text("title") == "MOSSELORA",
                "the wordmark is stable across draft language catalogs: " + language.Code);
            int different = 0;
            foreach (string key in GameLocalization.Keys)
            {
                string localized = GameLocalization.Text(key);
                Expect(!string.IsNullOrWhiteSpace(localized) && !localized.StartsWith("[missing:"),
                    "nonempty translation: " + language.Code + "/" + key);
                if (localized != GameLocalization.Source(key)) different++;
            }
            Expect(different >= 63, "catalog includes actual translations, not a mostly-English stub");
            string firstCopy = GameLocalization.Text("first");
            Expect(!firstCopy.Contains("S =") && !firstCopy.Contains("G =") &&
                   !firstCopy.Contains("S＝") && !firstCopy.Contains("G＝"),
                "first stream uses universal icon guidance, not English endpoint letters");
            Expect(GameLocalization.Text("stream", 1, 12).Contains("1") &&
                   GameLocalization.Text("progress", 3, 9, 2).Contains("9") &&
                   GameLocalization.Text("journal_entry", 1, "Mira", "Tree").Contains("Mira"),
                   "interpolation works for translated placeholders");

            // Locale switching must not mutate the puzzle state, history or
            // NPC-memory derivation, even for Arabic/Hebrew/Indic scripts.
            var world = WorldModel.New();
            Expect(LocalizedVillage.Consequence(world) == GameLocalization.Text("world_none"),
                "each locale has a truthful dry creek label: " + language.Code);
            Solve(world);
            Expect(LocalizedVillage.Consequence(world) == GameLocalization.Text("solved"),
                "each locale has a truthful restored creek label: " + language.Code);
            Expect(world.Choose(VillageChoice.ShadeTree), "locale test first real choice");
            WorldSnapshot before = world.Export();
            Expect(!string.IsNullOrWhiteSpace(LocalizedVillage.Consequence(world)) &&
                   !string.IsNullOrWhiteSpace(LocalizedVillage.Speak(world, 0)) &&
                   !string.IsNullOrWhiteSpace(LocalizedVillage.Memory(world)) &&
                   LocalizedVillage.Journal(world).Length == 1 &&
                   !string.IsNullOrWhiteSpace(LocalizedVillage.Choice(VillageChoice.Flowers)) &&
                   !string.IsNullOrWhiteSpace(LocalizedVillage.Prompt(world)) &&
                   !string.IsNullOrWhiteSpace(LocalizedVillage.EnvironmentalRule(world)),
                   "all narrative surfaces translated for " + language.Code);
            WorldSnapshot after = world.Export();
            Expect(before.round == after.round && before.solved == after.solved &&
                   before.choices.Length == after.choices.Length &&
                   before.rotations.Length == after.rotations.Length &&
                   before.choices[0] == after.choices[0],
                   "language changes cannot touch game state");
            tested++;
        }
        Expect(tested == 29, "all translated locale assets validated");
        Expect(GameLocalization.SetCurrent("en"), "English fallback restored");
        Expect(GameLocalization.Text("undo") == "Undo", "English source remains canonical");
        Expect(GameLocalization.Text("title") == "MOSSELORA",
            "English player-facing title is Mosselora; save and engine IDs stay legacy");

        TranslationBundle incomplete = new TranslationBundle {
            code = "fr", entries = new[] {
                new TranslationEntry { key = "undo", value = "Annuler" }
            }
        };
        Expect(!GameLocalization.Register(incomplete), "incomplete bundle must be rejected");
        TranslationBundle duplicate = new TranslationBundle {
            code = "fr", entries = new[] {
                new TranslationEntry { key = "undo", value = "Annuler" },
                new TranslationEntry { key = "undo", value = "Annuler" }
            }
        };
        Expect(!GameLocalization.Register(duplicate), "duplicate translation keys rejected");
        Expect(GameLocalization.SetCurrent("en"), "English remains available after invalid imports");
    }

    private static bool Throws<TException>(Action action) where TException : Exception
    {
        try { action(); }
        catch (Exception ex) { return ex is TException; }
        return false;
    }

    private static readonly JsonSerializerOptions SaveOptions =
        new JsonSerializerOptions { IncludeFields = true };

    private static WorldSnapshot CopySnapshot(WorldSnapshot source)
    {
        return JsonSerializer.Deserialize<WorldSnapshot>(
            JsonSerializer.Serialize(source, SaveOptions), SaveOptions);
    }

    private static int FirstMovable(Puzzle board)
    {
        for (int tile = 0; tile < board.CellCount; tile++)
            if (!board.IsAnchored(tile)) return tile;
        throw new Exception("board has no movable tile");
    }

    private static string Describe(Puzzle board, int variant, int[] rotations)
    {
        return board.Name + "|v" + variant + "|size=" + board.Size +
            "|anchor=" + board.AnchoredTile +
            "|kinds=" + string.Join(",", board.Kinds) +
            "|initial=" + string.Join(",", board.Initial) +
            "|solution=" + string.Join(",", board.Solution) +
            "|rotations=" + string.Join(",", rotations);
    }

    private static string BoardKey(WorldModel world)
    {
        return Describe(world.CurrentPuzzle, world.BoardVariant, world.Rotations);
    }

    private static void SolveQuietly(WorldModel world)
    {
        Puzzle board = world.CurrentPuzzle;
        for (int tile = 0; tile < board.CellCount; tile++)
        {
            int guard = 0;
            while (world.Rotations[tile] != board.Solution[tile])
            {
                if (!world.Rotate(tile) || ++guard > 4)
                    throw new Exception("solve failed at tile " + tile);
            }
        }
        if (!world.Solved) throw new Exception("solved board not accepted");
    }

    // Plays from the current chapter to the end. Living consumes the choices;
    // matched and puzzle-only advance with NextPuzzle and never receive them.
    private static string[] PlayRun(WorldModel world, int[] choices)
    {
        string[] boards = new string[PuzzleCatalog.Count];
        for (int chapter = world.Round; chapter < PuzzleCatalog.Count; chapter++)
        {
            boards[chapter] = BoardKey(world);
            SolveQuietly(world);
            bool advanced = world.Mode == PlayMode.Living
                ? world.Choose((VillageChoice)choices[chapter])
                : world.NextPuzzle();
            if (!advanced) throw new Exception("could not advance at chapter " + chapter);
        }
        if (!world.Completed) throw new Exception("run did not complete");
        return boards;
    }

    // Living run for these choices versus a matched run on the derived schedule.
    private static string[] AssertMatchedEqualsLiving(int[] choices)
    {
        var living = WorldModel.New(PlayMode.Living);
        string[] livingBoards = PlayRun(living, choices);
        Expect(living.Choices.Length == PuzzleCatalog.Count,
            "living run records one choice per chapter");

        var matched = WorldModel.NewMatched(WorldModel.ScheduleFromChoices(choices));
        string[] matchedBoards = PlayRun(matched, choices);
        Expect(matched.Completed && matched.Choices.Length == 0,
            "matched run completes with no village choices");
        for (int chapter = 0; chapter < PuzzleCatalog.Count; chapter++)
            Expect(livingBoards[chapter] == matchedBoards[chapter],
                "matched board identical to living board at chapter " + chapter);
        return livingBoards;
    }

    private static void MatchedScheduleText()
    {
        // Facilitator entry must accept exactly the 11-digit form and round-trip it.
        int[] parsed;
        Expect(WorldModel.TryParseSchedule("12332112312", out parsed) &&
               parsed.Length == 11 && parsed[0] == 1 && parsed[2] == 3 && parsed[10] == 2,
            "11 digits 1..3 parse into a schedule");
        Expect(WorldModel.IsValidSchedule(parsed), "parsed text is a valid schedule");
        Expect(WorldModel.FormatSchedule(parsed) == "12332112312", "schedule formats back to the same digits");
        foreach (string bad in new[] { "", "1233211231", "123321123120", "1233211231 ", "1233211231a",
                                       "12332112304", "12332112314", "1233211231-" })
            Expect(!WorldModel.TryParseSchedule(bad, out parsed) && parsed == null,
                "invalid schedule text rejected: '" + bad + "'");
        Expect(!WorldModel.TryParseSchedule(null, out parsed), "null schedule text rejected");
        Expect(Throws<ArgumentException>(() => WorldModel.FormatSchedule(new int[10])),
            "formatting an invalid schedule is rejected");

        // A Living run's copied schedule must recreate the same matched boards.
        int[] choices = { 0, 1, 2, 0, 1, 2, 0, 1, 2, 0, 1 };
        string copied = WorldModel.FormatSchedule(WorldModel.ScheduleFromChoices(choices));
        Expect(WorldModel.TryParseSchedule(copied, out parsed) &&
               WorldModel.IsValidSchedule(parsed), "copied Living schedule is re-enterable");
        WorldModel entered = WorldModel.NewMatched(parsed);
        Expect(entered.Mode == PlayMode.PuzzleMatched && !entered.CanSwitchMode && !entered.Choose(VillageChoice.Bridge),
            "entered schedule starts a locked matched run with no village choices");
    }

    private static void MatchedControlCondition()
    {
        // Input validation: exactly 11 later boards, each a branch variant 1..3.
        Expect(WorldModel.ScheduleLength == 11, "schedule covers boards 2..12");
        Expect(Throws<ArgumentException>(() => WorldModel.NewMatched(null)),
            "null schedule rejected");
        Expect(Throws<ArgumentException>(() => WorldModel.NewMatched(new int[10])),
            "short schedule rejected");
        Expect(Throws<ArgumentException>(() => WorldModel.NewMatched(new int[12])),
            "long schedule rejected");
        int[] zeroVariant = { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 0 };
        int[] fourVariant = { 4, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 };
        Expect(Throws<ArgumentException>(() => WorldModel.NewMatched(zeroVariant)),
            "variant 0 rejected in a matched schedule");
        Expect(Throws<ArgumentException>(() => WorldModel.NewMatched(fourVariant)),
            "variant 4 rejected in a matched schedule");
        Expect(Throws<ArgumentException>(() => WorldModel.New(PlayMode.PuzzleMatched)),
            "matched mode cannot be created without an explicit schedule");
        Expect(Throws<ArgumentException>(() => WorldModel.ScheduleFromChoices(new int[10])),
            "too few living choices cannot derive a schedule");
        Expect(Throws<ArgumentException>(() => WorldModel.ScheduleFromChoices(
            new[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 3 })),
            "invalid living choice cannot derive a schedule");

        int[] source = { 1, 2, 3, 1, 2, 3, 1, 2, 3, 1, 2 };
        WorldModel copied = WorldModel.NewMatched(source);
        source[0] = 3;
        Expect(copied.VariantSchedule[0] == 1, "schedule is copied, not aliased");
        Expect(copied.Mode == PlayMode.PuzzleMatched && copied.Round == 0 &&
               copied.BoardVariant == 0 && !copied.CanSwitchMode,
            "matched run starts on the unchanged first board and locks its condition");
        Expect(!copied.Choose(VillageChoice.ShadeTree) && !copied.ReconsiderLatestChoice(),
            "matched mode never accepts village choices");
        Expect(!copied.NextPuzzle(), "matched board cannot advance before it is solved");

        // Every chapter x every branch variant: a living player reaching that
        // variant must see the same board as a matched player given the schedule.
        for (int chapter = 1; chapter < PuzzleCatalog.Count; chapter++)
            for (int variant = 1; variant <= 3; variant++)
            {
                int[] choices = new int[PuzzleCatalog.Count];
                for (int i = 0; i < choices.Length; i++) choices[i] = (i + chapter) % 3;
                choices[chapter - 1] = variant - 1;
                string[] boards = AssertMatchedEqualsLiving(choices);
                Puzzle expected = ReactiveWatercourses.ForChapter(chapter, variant);
                Expect(boards[chapter] == Describe(expected, variant, expected.Initial),
                    "living chapter " + chapter + " really shows variant " + variant);
            }

        // All 3^7 = 2187 living prefixes for chapters 1..7, with varied tails.
        int enumerated = 0;
        for (int code = 0; code < 2187; code++)
        {
            int[] choices = new int[PuzzleCatalog.Count];
            int rest = code;
            for (int i = 0; i < 7; i++) { choices[i] = rest % 3; rest /= 3; }
            for (int i = 7; i < choices.Length; i++) choices[i] = (i + code) % 3;
            AssertMatchedEqualsLiving(choices);
            enumerated++;
        }
        Expect(enumerated == 2187, "all 3^7 living prefixes compared against matched runs");

        // Three full 12-chapter living paths with distinct choice sequences.
        int[][] named = {
            new int[PuzzleCatalog.Count],
            new[] { 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2 },
            new[] { 0, 1, 2, 0, 1, 2, 0, 1, 2, 0, 1, 2 }
        };
        foreach (int[] path in named) AssertMatchedEqualsLiving(path);

        // Mid-game save: the transformed board, rotation and schedule survive.
        int[] midChoices = { 0, 2, 1, 1, 0, 2, 0, 1, 2, 0, 1, 0 };
        int[] midSchedule = WorldModel.ScheduleFromChoices(midChoices);
        string[] reference = AssertMatchedEqualsLiving(midChoices);
        var world = WorldModel.NewMatched(midSchedule);
        for (int chapter = 0; chapter < 5; chapter++)
        {
            SolveQuietly(world);
            Expect(world.NextPuzzle(), "matched advances like puzzle-only after a solve");
        }
        Puzzle expectedBoard5 = ReactiveWatercourses.ForChapter(5, midSchedule[4]);
        string expectedBoard5Key = Describe(expectedBoard5, midSchedule[4], expectedBoard5.Initial);
        Expect(BoardKey(world) == expectedBoard5Key, "chapter 6 uses the scheduled transformed board");
        Expect(world.Rotate(FirstMovable(expectedBoard5)), "mid-game turn on the matched board");
        string midKey = BoardKey(world);
        string json = JsonSerializer.Serialize(world.Export(), SaveOptions);

        WorldModel restored;
        Expect(WorldModel.TryRestore(CopySnapshot(world.Export()), out restored),
            "mid-game matched save restores");
        Expect(restored.Mode == PlayMode.PuzzleMatched && restored.Round == 5 && !restored.Solved,
            "restored matched run keeps its mode and progress");
        Expect(BoardKey(restored) == midKey, "restored board, rotations and variant are identical");
        Expect(string.Join(",", restored.VariantSchedule) == string.Join(",", midSchedule),
            "variant schedule persists through save");
        WorldSnapshot reloaded = JsonSerializer.Deserialize<WorldSnapshot>(json, SaveOptions);
        Expect(WorldModel.TryRestore(reloaded, out restored) &&
               JsonSerializer.Serialize(restored.Export(), SaveOptions) == json,
            "matched JSON round trip is stable");
        Expect(restored.Undo() && BoardKey(restored) == expectedBoard5Key,
            "undo returns to the transformed initial board");
        Expect(restored.AdvanceGentleHint() == 1 && restored.AdvanceGentleHint() == 2 &&
               restored.AdvanceGentleHint() == 3 && restored.HintsUsed == 1,
            "staged hint acts on the transformed board");
        Expect(!restored.Rotate(expectedBoard5.AnchoredTile),
            "protected tile of the transformed board stays fixed after restore");
        Expect(restored.Restart() && BoardKey(restored) == expectedBoard5Key,
            "restart keeps the transformed board");
        string[] tail = PlayRun(restored, midChoices);
        for (int chapter = 5; chapter < PuzzleCatalog.Count; chapter++)
            Expect(tail[chapter] == reference[chapter],
                "resumed matched run equals living board at chapter " + chapter);
        Expect(restored.Completed && restored.Choices.Length == 0,
            "resumed matched run completes without choices");

        // Ending and consequences equal puzzle-only: neither has village decisions.
        var solo = WorldModel.New(PlayMode.PuzzleOnly);
        PlayRun(solo, midChoices);
        var matchedDone = WorldModel.NewMatched(midSchedule);
        PlayRun(matchedDone, midChoices);
        Expect(matchedDone.Ending == solo.Ending && matchedDone.Consequence == solo.Consequence &&
               matchedDone.Journal.Length == 0,
            "matched control shows no village story, same as puzzle-only");

        var notes = new LocalPlaytest(PlayMode.PuzzleMatched, false, 0, 10.0);
        Expect(notes.Report(10.0).Contains("condition=PuzzleMatched"),
            "private playtest notes distinguish the matched condition");

        // Snapshot tampering fails closed.
        WorldSnapshot good = world.Export();
        WorldSnapshot bad = CopySnapshot(good);
        bad.variantSchedule = null;
        Expect(!WorldModel.TryRestore(bad, out restored), "matched save without schedule rejected");
        bad = CopySnapshot(good);
        bad.variantSchedule = new int[10];
        Expect(!WorldModel.TryRestore(bad, out restored), "short persisted schedule rejected");
        bad = CopySnapshot(good);
        bad.variantSchedule[4] = 0;
        Expect(!WorldModel.TryRestore(bad, out restored), "zero persisted variant rejected");
        bad = CopySnapshot(good);
        bad.variantSchedule[10] = 4;
        Expect(!WorldModel.TryRestore(bad, out restored), "out-of-range persisted variant rejected");
        bad = CopySnapshot(good);
        bad.version = 3;
        Expect(!WorldModel.TryRestore(bad, out restored), "matched mode cannot exist before v4");
        bad = CopySnapshot(good);
        bad.choices = new[] { 0 };
        Expect(!WorldModel.TryRestore(bad, out restored), "matched save with village choices rejected");
        bad = CopySnapshot(good);
        bad.boardVariant = midSchedule[4] % 3 + 1;
        Expect(!WorldModel.TryRestore(bad, out restored), "board inconsistent with schedule rejected");
        bad = CopySnapshot(good);
        bad.lastSolvedVariant = midSchedule[3] % 3 + 1;
        Expect(!WorldModel.TryRestore(bad, out restored), "previous board inconsistent with schedule rejected");
        WorldSnapshot livingWithSchedule = WorldModel.New().Export();
        livingWithSchedule.variantSchedule = midSchedule;
        Expect(!WorldModel.TryRestore(livingWithSchedule, out restored),
            "living save cannot smuggle a schedule");
        WorldSnapshot puzzleWithSchedule = WorldModel.New(PlayMode.PuzzleOnly).Export();
        puzzleWithSchedule.variantSchedule = midSchedule;
        Expect(!WorldModel.TryRestore(puzzleWithSchedule, out restored),
            "puzzle-only save cannot smuggle a schedule");

        // Existing v4 saves written before this field existed still restore.
        string plain = JsonSerializer.Serialize(WorldModel.New().Export(), SaveOptions);
        string legacyJson = plain.Replace(",\"variantSchedule\":[]", "");
        Expect(legacyJson != plain, "test removes the optional field from a v4 save");
        Expect(WorldModel.TryRestore(JsonSerializer.Deserialize<WorldSnapshot>(legacyJson, SaveOptions),
            out restored) && restored.Mode == PlayMode.Living &&
            restored.Export().variantSchedule.Length == 0,
            "v4 save without variantSchedule restores as living");
    }

    private static void SeasonalNeighborTests()
    {
        WorldModel world = WorldModel.New();
        Expect(LocalizedVillage.Speak(world, 0) == GameLocalization.Text("mira_new"), "Mira asks before any tree");
        // Walk to autumn (stream 7) choosing trees only: Mira speaks of her tree's season,
        // Orin and Fen, with no crossing or flowers yet, still ask.
        while (world.Round < 6) { Solve(world); Expect(world.Choose(VillageChoice.ShadeTree), "tree choice"); }
        Expect(VillageScenePlan.From(world).LandscapePhase == 2, "stream 7 is autumn");
        Expect(LocalizedVillage.Speak(world, 0) == GameLocalization.Text("mira_autumn"), "Mira's autumn line once a tree exists");
        Expect(LocalizedVillage.Speak(world, 1) == GameLocalization.Text("orin_new"), "no seasonal line without a crossing");
        while (world.Round < 9) { Solve(world); Expect(world.Choose(VillageChoice.Flowers), "flower choice"); }
        Expect(LocalizedVillage.Speak(world, 0) == GameLocalization.Text("mira_winter"), "Mira's winter line");
        Expect(LocalizedVillage.Speak(world, 2) == GameLocalization.Text("fen_winter"), "Fen's winter line once flowers exist");
        while (!world.Completed) { Solve(world); Expect(world.Choose(VillageChoice.Bridge), "bridge choice"); }
        Expect(LocalizedVillage.Speak(world, 1) == GameLocalization.Text("orin_after"), "a finished garden returns to summer lines");
    }

    private static void RepeatedMemoryTests()
    {
        // A landmark chosen again is remembered in a new way (I90): first, second
        // and third time differ, then the three lines cycle.
        WorldModel world = WorldModel.New();
        for (int i = 0; i < 4; i++) { Solve(world); Expect(world.Choose(VillageChoice.Bridge), "bridge choice"); }
        int[] choices = world.Choices;
        string first = LocalizedVillage.MemoryAt(choices, 0);
        string second = LocalizedVillage.MemoryAt(choices, 1);
        string third = LocalizedVillage.MemoryAt(choices, 2);
        Expect(first == GameLocalization.Text("memory_bridge"), "first bridge keeps the original line");
        Expect(second == GameLocalization.Text("memory_bridge_2"), "second bridge has its own line");
        Expect(third == GameLocalization.Text("memory_bridge_3"), "third bridge has its own line");
        Expect(first != second && second != third && first != third, "three distinct bridge memories");
        Expect(LocalizedVillage.MemoryAt(choices, 3) == first, "the fourth bridge cycles back");
        Expect(LocalizedVillage.Memory(world) == LocalizedVillage.MemoryAt(choices, 3),
            "the memory screen and the journal agree on the latest line");
        WorldModel mixed = WorldModel.New();
        Solve(mixed); Expect(mixed.Choose(VillageChoice.ShadeTree), "tree choice");
        Solve(mixed); Expect(mixed.Choose(VillageChoice.Flowers), "flower choice");
        Solve(mixed); Expect(mixed.Choose(VillageChoice.ShadeTree), "second tree");
        Expect(LocalizedVillage.MemoryAt(mixed.Choices, 1) == GameLocalization.Text("memory_flowers"),
            "a different landmark in between starts its own count");
        Expect(LocalizedVillage.MemoryAt(mixed.Choices, 2) == GameLocalization.Text("memory_tree_2"),
            "a repeat is counted across other choices");
        Expect(LocalizedVillage.MemoryAt(mixed.Choices, 9) == GameLocalization.Text("world_none"),
            "an index past the history is the empty memory");
    }

    // I171: 50 trees, 50 flowers, 100 things to build or mend.
    private static void GardenCatalogTests()
    {
        Expect(GardenCatalog.CountOf(VillageChoice.ShadeTree) == GardenCatalog.TreeCount, "50 trees");
        Expect(GardenCatalog.CountOf(VillageChoice.Flowers) == GardenCatalog.FlowerCount, "50 flowers");
        Expect(GardenCatalog.CountOf(VillageChoice.Bridge) == GardenCatalog.BuildCount, "100 things to build");
        Expect(GardenCatalog.Total == 200, "200 garden items in all");
        var names = new HashSet<string>();
        var arts = new HashSet<string>();
        foreach (VillageChoice family in new[] { VillageChoice.ShadeTree, VillageChoice.Bridge, VillageChoice.Flowers })
        {
            int count = GardenCatalog.CountOf(family);
            int[] order = GardenCatalog.Order(family);
            Expect(order.Length == count, "order covers the family");
            var seen = new HashSet<int>(order);
            Expect(seen.Count == count, "order is a permutation");
            for (int i = 0; i < count; i++)
            {
                GardenItem item = GardenCatalog.Get(family, i);
                Expect(item.Family == family && item.Index == i, "item knows its place");
                Expect(names.Add(item.Name), "item names are unique: " + item.Name);
                Expect(arts.Add(item.Art), "item art keys are unique: " + item.Art);
                Expect(item.Card.Length > 4 && item.Memory.StartsWith("the ") && item.Memory.Contains(item.Name),
                    "item has a card title and a memory phrase: " + item.Name);
                Expect(!item.Card.Contains("  ") && !item.Card.EndsWith(" "), "card title is tidy: " + item.Card);
            }
            // Deterministic: the same garden and chapter always give the same item.
            Expect(GardenCatalog.For(3, 5, family).Index == GardenCatalog.For(3, 5, family).Index, "deterministic");
            // No repeats inside a garden; consecutive gardens differ; full coverage.
            var covered = new HashSet<int>();
            int gardensToCover = (count + GardenCatalog.ChaptersPerGarden - 1) / GardenCatalog.ChaptersPerGarden;
            for (int garden = 0; garden < 40; garden++)
            {
                var inGarden = new HashSet<int>();
                for (int chapter = 0; chapter < GardenCatalog.ChaptersPerGarden; chapter++)
                {
                    int index = GardenCatalog.For(garden, chapter, family).Index;
                    Expect(inGarden.Add(index), "no repeat inside garden " + garden);
                    if (garden < gardensToCover) covered.Add(index);
                }
                if (garden > 0 && count >= 2 * GardenCatalog.ChaptersPerGarden)
                {
                    var previous = new HashSet<int>();
                    for (int chapter = 0; chapter < GardenCatalog.ChaptersPerGarden; chapter++)
                        previous.Add(GardenCatalog.For(garden - 1, chapter, family).Index);
                    previous.IntersectWith(inGarden);
                    Expect(previous.Count == 0 || (garden * GardenCatalog.ChaptersPerGarden) % count < GardenCatalog.ChaptersPerGarden,
                        "consecutive gardens differ (except where the catalogue wraps)");
                }
            }
            Expect(covered.Count == count, "every item appears within " + gardensToCover + " gardens");
        }
        Expect(GardenCatalog.Get(VillageChoice.ShadeTree, 0).Card == "Plant an apple tree", "a/an for trees");
        Expect(GardenCatalog.Get(VillageChoice.Bridge, 3).Card == "Mend the mill wheel", "build verbs");
        Expect(GardenCatalog.Get(VillageChoice.Flowers, 2).Memory == "the poppies you grew", "flower memory phrase");
        bool threw = false;
        try { GardenCatalog.For(0, 12, VillageChoice.Bridge); } catch (ArgumentOutOfRangeException) { threw = true; }
        Expect(threw, "chapter 13 is out of range");
    }

    // E2 / I174: the almanac remembers every item ever made, across gardens.
    private static void AlmanacTests()
    {
        Almanac empty = Almanac.Parse(null);
        Expect(empty.Count == 0 && empty.Serialize() == "", "empty almanac");
        Almanac junk = Almanac.Parse("tree-07, ,banana,tree-50,build-99,flower-xx,tree-7,build-03,build-03");
        Expect(junk.Count == 3, "junk and out-of-range keys are ignored, duplicates merged");
        Expect(junk.Serialize() == "build-03,build-99,tree-07", "stable sorted text");
        Expect(Almanac.Parse(junk.Serialize()).Serialize() == junk.Serialize(), "round trip");
        var almanac = new Almanac();
        GardenItem quince = GardenCatalog.Get(VillageChoice.ShadeTree, 4);
        Expect(almanac.Add(quince), "a new item is new");
        Expect(!almanac.Add(quince), "the same item twice is not new");
        Expect(almanac.Has(quince) && almanac.CountOf(VillageChoice.ShadeTree) == 1 && almanac.CountOf(VillageChoice.Bridge) == 0,
            "family counts");
        Expect(!almanac.Add(null), "null is not added");
        // A full catalogue round-trips and counts 200.
        var full = new Almanac();
        foreach (VillageChoice family in new[] { VillageChoice.ShadeTree, VillageChoice.Bridge, VillageChoice.Flowers })
            for (int i = 0; i < GardenCatalog.CountOf(family); i++) full.Add(GardenCatalog.Get(family, i));
        Expect(full.Count == GardenCatalog.Total, "every item can be found");
        Expect(Almanac.Parse(full.Serialize()).Count == GardenCatalog.Total, "full almanac round trip");
        Expect(Almanac.IsItemKey("flower-49") && !Almanac.IsItemKey("flower-50") && !Almanac.IsItemKey("build-100"), "key ranges");
    }

    // E3 / I175: the open garden map's grid, free land and default layout.
    private static void GardenMapTests()
    {
        string mask =
            "...~......" +
            "...~....##" +
            "....~...##" +
            "....~....." +
            ".....~...." +
            ".....~...." +
            "....~....." +
            "...~......";
        var map = new GardenMap(10, 8, mask);
        Expect(map.CellCount == 80 && map.IsLand(0) && !map.IsLand(3) && !map.IsLand(18), "mask parsed");
        Expect(map.At(3) == GardenMap.Water && map.At(18) == GardenMap.Built, "water and built cells");
        Expect(!map.IsLand(-1) && !map.IsLand(80), "outside is not land");
        Expect(map.DistanceToWater(2) == 1 && map.DistanceToWater(3) == 0, "distance to water");
        bool threw = false;
        try { new GardenMap(10, 8, "..."); } catch (ArgumentException) { threw = true; }
        Expect(threw, "a mask of the wrong size is refused");
        // Twelve choices of every family fit, on land, never stacked, deterministic.
        int[] choices = { 0, 1, 2, 1, 0, 2, 2, 1, 0, 1, 2, 0 };
        int[] layout = map.Layout(choices, null);
        Expect(layout.Length == 12, "one cell per choice");
        var used = new HashSet<int>();
        for (int i = 0; i < layout.Length; i++)
        {
            Expect(map.IsLand(layout[i]), "item " + i + " stands on land");
            Expect(used.Add(layout[i]), "no two items share a cell");
        }
        int[] again = map.Layout(choices, null);
        for (int i = 0; i < layout.Length; i++) Expect(again[i] == layout[i], "layout is deterministic");
        // Builds sit closer to the creek than trees on average.
        int buildWater = 0, treeWater = 0, builds = 0, trees = 0;
        for (int i = 0; i < choices.Length; i++)
        {
            if (choices[i] == 1) { buildWater += map.DistanceToWater(layout[i]); builds++; }
            if (choices[i] == 0) { treeWater += map.DistanceToWater(layout[i]); trees++; }
        }
        Expect(buildWater * trees < treeWater * builds, "builds by the water, trees further out");
        // A player's own valid spot is kept; an invalid or taken one falls back.
        int[] placed = { 79, 3, 79 };
        int[] kept = map.Layout(new[] { 0, 1, 2 }, placed);
        Expect(kept[0] == 79, "a valid placement is kept");
        Expect(map.IsLand(kept[1]) && kept[1] != 3, "water placement falls back to land");
        Expect(kept[2] != 79 && map.IsLand(kept[2]), "a taken cell falls back");
        Expect(map.CanPlace(kept, 0) == (Array.IndexOf(kept, 0) < 0), "can place on free land");
        Expect(!map.CanPlace(kept, 79) && map.CanPlace(kept, 79, 0), "moving an item may keep its own cell");
        // A full map reports -1 rather than stacking.
        var tiny = new GardenMap(2, 1, "..");
        int[] crowded = tiny.Layout(new[] { 0, 1, 2 }, null);
        Expect(crowded[0] >= 0 && crowded[1] >= 0 && crowded[2] == -1, "no free cell gives -1");
    }

    // I192: the endless river, reach by reach.
    private static void RiverMapTests()
    {
        string reach =
            "...~....##" +
            "....~....." +
            ".....~...." +
            "......~...";
        GardenMap one = GardenMap.River(10, 4, reach, 1);
        Expect(one.Rows == 4 && one.ReachRows == 4 && one.Reaches == 1, "one reach");
        GardenMap three = GardenMap.River(10, 4, reach, 3);
        Expect(three.Rows == 12 && three.Reaches == 3 && three.CellCount == 120, "three reaches stack");
        Expect(three.At(3) == GardenMap.Water && three.At(8) == GardenMap.Built, "the first reach is the painting");
        // The second reach is mirrored: its first row is the painting's last row.
        Expect(three.At(40 + 6) == GardenMap.Water && three.At(70 + 3) == GardenMap.Water && three.At(70 + 8) == GardenMap.Built,
            "the second reach is mirrored so the banks join");
        Expect(three.At(80 + 3) == GardenMap.Water && three.At(80 + 8) == GardenMap.Built, "the third reach repeats the first");
        Expect(GardenMap.ReachesFor(0, false) == 1 && GardenMap.ReachesFor(2, false) == 1 && GardenMap.ReachesFor(3, false) == 2
            && GardenMap.ReachesFor(11, false) == 4 && GardenMap.ReachesFor(12, false) == 5 && GardenMap.ReachesFor(99, false) == 5
            && GardenMap.ReachesFor(0, true) == 5, "a new reach every three streams, all of them at the end");
        bool threw = false;
        try { GardenMap.River(10, 4, reach, 0); } catch (ArgumentOutOfRangeException) { threw = true; }
        Expect(threw, "a river needs a reach");
        // Each stream's thing grows in the reach its stream opened.
        string full = "";
        for (int r = 0; r < 15; r++) full += r % 3 == 0 ? "...~....##" : "....~.....";
        GardenMap river = GardenMap.River(10, 15, full, 5);
        int[] choices = { 0, 1, 2, 0, 1, 2, 0, 1, 2, 0, 1, 2 };
        int[] layout = river.Layout(choices, null);
        for (int i = 0; i < choices.Length; i++)
        {
            Expect(river.IsLand(layout[i]), "river item " + i + " on land");
            Expect(layout[i] / river.Columns / 15 == Math.Min(i / 3, 4), "item " + i + " stands in its stream's reach");
        }
        // Opening more river never moves what already grew.
        GardenMap shorter = GardenMap.River(10, 15, full, 2);
        int[] early = shorter.Layout(new[] { 0, 1, 2, 0, 1 }, null);
        int[] later = river.Layout(new[] { 0, 1, 2, 0, 1 }, null);
        for (int i = 0; i < early.Length; i++) Expect(early[i] == later[i], "item " + i + " stays put as the river grows");
    }

    // E4 / I177: the garden number and where things stand live in the save and the garden file.
    private static void PlacementSaveTests()
    {
        WorldModel world = WorldModel.New(PlayMode.Living, 3);
        Expect(world.Garden == 3 && world.Placements.Length == 0, "a new garden knows its number");
        Expect(!world.PlaceAt(0, 5), "nothing to place before the first choice");
        SolveCurrent(world);
        Expect(world.Choose(VillageChoice.Bridge), "first choice");
        Expect(world.Placements.Length == 1 && world.Placements[0] == -1, "a new item starts unplaced");
        Expect(world.PlaceAt(0, 42) && world.Placements[0] == 42, "an item is placed");
        Expect(!world.PlaceAt(0, WorldModel.MaxMapCell) && !world.PlaceAt(0, -2) && !world.PlaceAt(1, 3),
            "out-of-range cells and items are refused");
        SolveCurrent(world);
        Expect(world.Choose(VillageChoice.Flowers), "second choice");
        Expect(world.Placements.Length == 2 && world.Placements[0] == 42 && world.Placements[1] == -1,
            "earlier placements stay; the new item waits");
        Expect(world.ReconsiderLatestChoice() && world.Placements.Length == 1 && world.Placements[0] == 42,
            "reconsidering trims only the newest placement");
        // Save round trip keeps both.
        WorldModel restored;
        Expect(WorldModel.TryRestore(world.Export(), out restored), "placed garden restores");
        Expect(restored.Garden == 3 && restored.Placements.Length == 1 && restored.Placements[0] == 42,
            "garden and placements survive a save");
        // Older saves (no fields) restore as garden 0, nothing placed.
        WorldSnapshot old = world.Export();
        old.garden = 0;
        old.placements = null;
        Expect(WorldModel.TryRestore(old, out restored) && restored.Garden == 0 && restored.Placements.Length == 0,
            "a save from before the map restores");
        WorldSnapshot bad = world.Export();
        bad.placements = new[] { 1, 2, 3 };
        Expect(!WorldModel.TryRestore(bad, out restored), "more placements than choices is refused");
        bad = world.Export();
        bad.garden = -1;
        Expect(!WorldModel.TryRestore(bad, out restored), "a negative garden is refused");
        // The garden file carries them, and older files without them still import.
        string file = GardenTransfer.Export(world, "2026-11-25T00:00:00Z");
        Expect(file.Contains("\"garden\":3") && file.Contains("\"placements\":[42]"), "garden file carries the map");
        WorldModel imported;
        GardenImportError error;
        Expect(GardenTransfer.TryImport(file, out imported, out error) && imported.Placements[0] == 42 && imported.Garden == 3,
            "garden file round trip keeps the map");
        WorldModel plain = WorldModel.New();
        string plainFile = GardenTransfer.Export(plain, "2026-11-25T00:00:00Z");
        string inner = plainFile.Substring(plainFile.IndexOf("{\"version\"", StringComparison.Ordinal));
        inner = inner.Substring(0, inner.IndexOf('}') + 1);
        string preMap = UndoHistoryTests.RemoveMapKeys(inner);
        Expect(preMap != inner && !preMap.Contains("placements"), "pre-map snapshot text built");
        string preMapFile = plainFile.Replace(inner, preMap)
            .Replace(GardenTransfer.Sha256Hex(inner), GardenTransfer.Sha256Hex(preMap));
        Expect(GardenTransfer.TryImport(preMapFile, out imported, out error) && error == GardenImportError.None,
            "a garden file from before the map still imports");
        string tampered = file.Replace("\"placements\":[42]", "\"placements\":[43]");
        Expect(!GardenTransfer.TryImport(tampered, out imported, out error), "an edited placement fails its checksum");
    }

    // I193: free planting from the basket.
    private static void PlantingTests()
    {
        WorldModel world = WorldModel.New(PlayMode.Living, 1);
        Expect(!world.Plant(0, 10), "nothing to plant before the first choice");
        SolveCurrent(world);
        Expect(world.Choose(VillageChoice.Flowers), "first choice");
        Expect(world.Plant(0, 10) && world.Plant(0, 11) && world.PlantingCount == 2, "plant two more of what you grew");
        Expect(!world.Plant(1, 12) && !world.Plant(0, -1) && !world.Plant(0, WorldModel.MaxMapCell),
            "only grown things, on real cells");
        Expect(world.MovePlanting(1, 20) && world.Plantings[3] == 20, "a planting moves");
        Expect(!world.MovePlanting(2, 5) && !world.MovePlanting(0, -3), "bad moves are refused");
        Expect(world.RemovePlanting(0) && world.PlantingCount == 1 && world.Plantings[1] == 20, "a planting goes back to the basket");
        Expect(!world.RemovePlanting(1), "removing past the end is refused");
        WorldModel restored;
        Expect(WorldModel.TryRestore(world.Export(), out restored) && restored.PlantingCount == 1 && restored.Plantings[1] == 20,
            "plantings survive the save");
        WorldSnapshot bad = world.Export();
        bad.plantings = new[] { 0 };
        Expect(!WorldModel.TryRestore(bad, out restored), "an odd planting list is refused");
        bad.plantings = new[] { 5, 3 };
        Expect(!WorldModel.TryRestore(bad, out restored), "a planting of something never grown is refused");
        bad.plantings = null;
        Expect(WorldModel.TryRestore(bad, out restored) && restored.PlantingCount == 0, "a save without plantings still loads");
        string file = GardenTransfer.Export(world, "2026-11-26T00:00:00Z");
        WorldModel imported;
        GardenImportError error;
        Expect(file.Contains("\"plantings\":[0,20]") && GardenTransfer.TryImport(file, out imported, out error) &&
            imported.PlantingCount == 1, "the garden file carries plantings");
        Expect(!GardenTransfer.TryImport(file.Replace("\"plantings\":[0,20]", "\"plantings\":[0,21]"), out imported, out error),
            "an edited planting fails its checksum");
        // Reconsidering the newest choice takes its plantings back too.
        SolveCurrent(world);
        Expect(world.Choose(VillageChoice.ShadeTree) && world.Plant(1, 30) && world.PlantingCount == 2, "second choice planted");
        Expect(world.ReconsiderLatestChoice() && world.PlantingCount == 1 && world.Plantings[0] == 0,
            "reconsidering returns that choice's plantings");
        for (int i = world.PlantingCount; i < WorldModel.MaxPlantings; i++) world.Plant(0, 40 + i);
        Expect(world.PlantingCount == WorldModel.MaxPlantings && !world.Plant(0, 9), "the basket has a sensible limit");
        // Defaults step around planted cells.
        string mask = "....".PadRight(40, '.');
        var map = new GardenMap(10, 4, mask);
        int[] plain = map.Layout(new[] { 2 }, null);
        int[] around = map.Layout(new[] { 2 }, null, new[] { 0, plain[0] });
        Expect(around[0] != plain[0] && map.IsLand(around[0]), "a default never lands on a planting");
        Expect(!map.IsFree(around, new[] { 0, plain[0] }, plain[0]) && !map.IsFree(around, null, around[0]) &&
            map.IsFree(around, new[] { 0, plain[0] }, 39 == plain[0] ? 38 : 39), "free means no item and no planting");
    }

    private static void SolveCurrent(WorldModel world)
    {
        Puzzle puzzle = world.CurrentPuzzle;
        for (int i = 0; i < puzzle.CellCount; i++)
            for (int turn = 0; turn < 4 && world.Rotations[i] != puzzle.Solution[i]; turn++)
                if (!world.Rotate(i)) break;
    }

    private static void BoardFitTests()
    {
        // Portrait keeps the EG-007 sizing: a 402 pt iPhone column gives the 3x3 cap.
        Expect(BoardFit.Portrait(343f, 3, 7) == 90, "portrait 3x3 on a 402 pt phone hits the 90 pt cap");
        Expect(BoardFit.Portrait(200f, 3, 7) == 62, "a narrow column shrinks portrait tiles");
        Expect(BoardFit.Portrait(120f, 3, 7) == 48, "portrait tiles never fall below 48 pt");
        Expect(BoardFit.Portrait(343f, 4, 5) == 67, "portrait 4x4 hits its 67 pt cap");

        // Sideways on an iPhone 16 Pro class phone (about 392 pt tall, 361 pt column):
        // the tool rail frees height, so the unsolved board is bigger than the solved one (I116).
        int unsolved = BoardFit.Landscape(392f, 361f, 3, 7, false);
        int solved = BoardFit.Landscape(392f, 361f, 3, 7, true);
        Expect(unsolved == 78, "sideways unsolved 3x3 tiles are 78 pt on a 392 pt screen");
        Expect(solved == 53, "sideways solved 3x3 tiles are 53 pt without the rail");
        Expect(unsolved > solved, "the tool rail buys the unsolved board height");

        // A sideways iPad (896 pt logical height, 560 pt column) gets bigger tiles
        // than the phone cap allows, up to the tablet cap (I122).
        int tabletTile = BoardFit.Landscape(896f, 560f - 59f, 3, 7, false, BoardFit.TabletCap);
        Expect(tabletTile == BoardFit.TabletCap, "a sideways iPad grows 3x3 tiles to the 120 pt tablet cap");
        Expect(BoardFit.Landscape(896f, 501f, 3, 7, false) == 90, "without the tablet cap the phone cap holds");
        Expect(BoardFit.Landscape(896f, 501f, 4, 5, false, BoardFit.TabletCap) <= BoardFit.TabletCap,
            "the tablet cap bounds 4x4 tiles too");
        for (float height = 600f; height <= 1100f; height += 25f)
            for (float width = 380f; width <= 640f; width += 20f)
            {
                int capped = BoardFit.Landscape(height, width, 3, 7, false, BoardFit.TabletCap);
                Expect(capped >= BoardFit.MinTouch && capped <= BoardFit.TabletCap, "tablet tiles stay within bounds");
                if (capped > BoardFit.MinTouch)
                    Expect(BoardFit.Span(capped, 3, 7) + BoardFit.RailWidth + BoardFit.RailGap <= width + 0.5f,
                        "the rail still fits beside a tablet board");
            }

        // Properties across phone heights and column widths, sizes 3 to 5.
        for (int size = 3; size <= 5; size++)
        {
            int gap = size == 3 ? 7 : 5;
            int previous = 0;
            for (float height = 300f; height <= 460f; height += 4f)
            {
                for (float width = 260f; width <= 480f; width += 20f)
                {
                    foreach (bool done in new[] { false, true })
                    {
                        int tile = BoardFit.Landscape(height, width, size, gap, done);
                        Expect(tile >= BoardFit.MinTouch, "sideways tiles keep a 44 pt touch target");
                        Expect(tile <= BoardFit.MaxTile(size), "sideways tiles respect the size cap");
                        int span = BoardFit.Span(tile, size, gap);
                        float below = done ? BoardFit.BelowSolved : BoardFit.BelowUnsolved;
                        // Whenever the minimum does not bind, the board and its chrome fit the height.
                        if (tile > BoardFit.MinTouch)
                            Expect(span + gap + below <= height + 0.5f,
                                "sideways board and chrome fit the screen height");
                        // Unsolved, the rail always has its room beside the board.
                        if (!done && tile > BoardFit.MinTouch)
                            Expect(span + BoardFit.RailWidth + BoardFit.RailGap <= width + 0.5f,
                                "the tool rail fits beside the board");
                    }
                }
                int tallest = BoardFit.Landscape(height, 480f, size, gap, false);
                Expect(tallest >= previous, "a taller screen never shrinks the tiles");
                previous = tallest;
            }
        }
    }

    private static void HubRecallTests()
    {
        // The hub recalls the newest memory first, then a different past one on
        // each later launch, cycling through all of them (I107). Never the clock.
        Expect(HubRecall.Next(0, 0, -1) == -1, "no memories, nothing to recall");
        Expect(HubRecall.Next(1, 0, -1) == 0, "the first memory is recalled first");
        Expect(HubRecall.Next(3, 2, 1) == 2, "a new memory since last launch is recalled first");
        Expect(HubRecall.Next(3, 3, 2) == 1, "next launch steps back to an earlier memory");
        Expect(HubRecall.Next(3, 3, 1) == 0, "and again");
        Expect(HubRecall.Next(3, 3, 0) == 2, "then cycles back to the newest");
        Expect(HubRecall.Next(1, 1, 0) == 0, "a single memory stays");
        Expect(HubRecall.Next(4, 4, 9) == 3, "an out-of-range stored index falls back to the newest");
        int seen = 0, index = HubRecall.Next(5, 0, -1);
        var visited = new System.Collections.Generic.HashSet<int>();
        for (int launch = 0; launch < 5; launch++) { visited.Add(index); index = HubRecall.Next(5, 5, index); seen++; }
        Expect(visited.Count == 5, "five launches visit all five memories");
    }

    private static void GardenPostcardTests()
    {
        WorldModel fresh = WorldModel.New();
        string empty = GardenPostcard.Build(fresh, "Stream 1 / 12");
        Expect(empty.StartsWith("Mosselora\n"), "postcard opens with the game name");
        Expect(empty.Contains("○○○○○○○○○○○○  Stream 1 / 12"),
            "a fresh garden shows twelve stones ahead");
        WorldModel walked = WorldModel.New();
        Solve(walked);
        Expect(walked.Choose(VillageChoice.Bridge), "postcard world accepts a choice");
        Solve(walked);
        Expect(walked.Choose(VillageChoice.ShadeTree), "postcard world accepts a second choice");
        string card = GardenPostcard.Build(walked, "Stream 3 / 12");
        Expect(card.Contains("\U0001F309\U0001F333\n"), "choices appear in order as pictures");
        Expect(card.Contains("●●○"), "two stones walked, the rest ahead");
        Expect(GardenPostcard.Build(walked, "Stream 3 / 12") == card, "postcard is deterministic");
        Expect(!card.Contains("http") && !card.Contains("@"), "postcard carries no link or identifier");
    }

    public static int Main(string[] args)
    {
        try
        {
            if (args.Length == 1 && args[0] == "--print-puzzle-catalog")
            {
                Console.WriteLine(JsonSerializer.Serialize(
                    PuzzleDiagnostics.All(), CatalogOptions));
                return 0;
            }
            if (args.Length == 1 && args[0] == "--check-puzzle-catalog")
            {
                if (!CheckPuzzleCatalog())
                {
                    Console.Error.WriteLine("Puzzle catalog drift detected; regenerate from core.");
                    return 1;
                }
                Console.WriteLine("All 45 puzzle profiles match the source-derived metadata.");
                return 0;
            }
            if (args.Length != 0) throw new ArgumentException("Unknown test argument.");
            ContentAndRules();
            RestoreAndMigration();
            SaveJournalTests.Run();
            assertions += UndoHistoryTests.Run();
            assertions += GardenTransferTests.Run();
            StoryAndCompleteRun();
            ScenicCausality();
            ReactiveCampaign();
            MatchedControlCondition();
            MatchedScheduleText();
            ProgressiveHintAndSaveV4();
            NonColorLeaksAndCausalJournal();
            LocalizedCharacterNamesInMemories();
            PuzzleDiagnosticsTests();
            PrivatePlaytestReport();
            ReturnDiaryTests();
            LocalizationTests();
            KinsokuLineBreakTests();
            RtlTextTests();
            GardenPostcardTests();
            SeasonalNeighborTests();
            RepeatedMemoryTests();
            HubRecallTests();
            BoardFitTests();
            GardenCatalogTests();
            AlmanacTests();
            GardenMapTests();
            RiverMapTests();
            PlantingTests();
            assertions += TreePuzzleTests.Run();
            assertions += NurseryTests.Run();
            assertions += PairsTests.Run();
            assertions += MosaicTests.Run();
            assertions += NoticeTests.Run();
            assertions += RiverGrowsTests.Run();
            assertions += FamilyParcelTests.Run();
            assertions += FamilyHelpTests.Run();
            assertions += FruitTests.Run();
            PlacementSaveTests();
            IndicVisualOrderTests();
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
