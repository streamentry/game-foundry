using System;
using System.Text.Json;
using Evergrove.Core;

/// <summary>Preview must predict a real choice without spending or changing it.</summary>
internal static class ChoicePreviewTests
{
    private static int assertions;
    private static readonly JsonSerializerOptions Options = new JsonSerializerOptions { IncludeFields = true };

    private static void Expect(bool condition, string message)
    {
        assertions++;
        if (!condition) throw new Exception("Choice preview test failed: " + message);
    }

    private static string Snapshot(WorldModel world)
    {
        return JsonSerializer.Serialize(world.Export(), Options);
    }

    private static WorldModel Copy(WorldModel world)
    {
        WorldModel result;
        Expect(WorldModel.TryRestore(world.Export(), out result), "fixture restores independently");
        return result;
    }

    private static void Solve(WorldModel world)
    {
        Puzzle board = world.CurrentPuzzle;
        for (int tile = 0; tile < board.CellCount; tile++)
            for (int turn = 0; turn < 4 && world.Rotations[tile] != board.Solution[tile]; turn++)
                Expect(world.Rotate(tile), "solver turns only mutable tiles");
        Expect(world.Solved, "fixture reaches a valid solution");
    }

    private static void SameBoard(Puzzle preview, Puzzle actual, string context)
    {
        Expect(preview != null && actual != null, context + ": both have a next board");
        Expect(preview.Size == actual.Size && preview.Name == actual.Name,
            context + ": exact next chapter, variant and dimensions");
        Expect(preview.AnchoredTile == actual.AnchoredTile && preview.AnchoredTile >= 0,
            context + ": exact fixed-tile location");
        Expect(preview.EnvironmentalRule == actual.EnvironmentalRule,
            context + ": exact environmental rule");
        for (int i = 0; i < actual.CellCount; i++)
            Expect(preview.Kinds[i] == actual.Kinds[i] &&
                   preview.Initial[i] == actual.Initial[i] &&
                   preview.Solution[i] == actual.Solution[i],
                context + ": tile " + i + " has the real channel and orientations");
        int fixedTile = preview.AnchoredTile;
        Expect(preview.Initial[fixedTile] == preview.Solution[fixedTile] &&
               preview.IsAnchored(fixedTile), context + ": displayed landmark is already fixed correctly");
    }

    private static void EveryChapterAndChoiceMatchesCommit()
    {
        WorldModel world = WorldModel.New();
        bool sawCornerAnchor = false;
        bool sawStraightAnchor = false;
        for (int chapter = 0; chapter < PuzzleCatalog.Count; chapter++)
        {
            // Mixed hint and manual play leaves real undo and hint-use state to preserve.
            Expect(world.GuideOneConnection(), "a voluntary hint is available before solving");
            Solve(world);
            string before = Snapshot(world);
            for (int option = 0; option < 3; option++)
            {
                var choice = (VillageChoice)option;
                VillageChoicePreview preview;
                Expect(VillageChoicePreview.TryCreate(world, choice, out preview),
                    "solved chapter " + chapter + " previews option " + option);
                Expect(preview.Choice == choice && !ReferenceEquals(preview.WorldAfter, world),
                    "preview identifies the option and owns a separate world");
                Expect(Snapshot(world) == before,
                    "preview leaves all progress, choices, hint use and undo arrays unchanged");

                WorldModel committed = Copy(world);
                Expect(committed.Choose(choice), "the same real choice is accepted");
                Expect(Snapshot(preview.WorldAfter) == Snapshot(committed),
                    "preview and real commit produce the same complete save state");
                Expect(VillageScenePlan.From(preview.WorldAfter).Fingerprint ==
                       VillageScenePlan.From(committed).Fingerprint,
                    "preview paints the same landscape as the committed choice");
                Expect(LocalizedVillage.Consequence(preview.WorldAfter) == LocalizedVillage.Consequence(committed) &&
                       LocalizedVillage.Memory(preview.WorldAfter) == LocalizedVillage.Memory(committed),
                    "preview text describes the actual saved consequence and memory");
                if (chapter + 1 < PuzzleCatalog.Count)
                {
                    SameBoard(preview.NextPuzzle, committed.CurrentPuzzle, "chapter " + chapter + " option " + option);
                    PipeKind kind = preview.NextPuzzle.Kinds[preview.NextPuzzle.AnchoredTile];
                    sawCornerAnchor |= kind == PipeKind.Corner;
                    sawStraightAnchor |= kind == PipeKind.Straight;
                    Expect(preview.WorldAfter.BoardVariant == option + 1 &&
                           LocalizedVillage.EnvironmentalRule(preview.WorldAfter) ==
                           LocalizedVillage.EnvironmentalRule(committed),
                        "next-board rule follows the selected option");
                }
                else
                {
                    Expect(preview.NextPuzzle == null && preview.WorldAfter.Completed,
                        "final choice offers an ending, never a nonexistent thirteenth stream");
                    Expect(LocalizedVillage.Ending(preview.WorldAfter) == LocalizedVillage.Ending(committed),
                        "last-choice preview has the real ending");
                }
            }
            Expect(Snapshot(world) == before, "comparing all three options does not make a choice");
            Expect(world.Choose((VillageChoice)(chapter % 3)), "actual mixed campaign advances once");
        }
        Expect(sawCornerAnchor && sawStraightAnchor,
            "fixtures cover both corner and straight landmarks; a decorative straight pipe cannot stand in for all");
    }

    private static void ComparisonsAreIsolated()
    {
        WorldModel world = WorldModel.New();
        Solve(world);
        string before = Snapshot(world);
        VillageChoicePreview tree;
        VillageChoicePreview bridge;
        VillageChoicePreview treeAgain;
        Expect(VillageChoicePreview.TryCreate(world, VillageChoice.ShadeTree, out tree), "preview a tree");
        Expect(VillageChoicePreview.TryCreate(world, VillageChoice.Bridge, out bridge), "compare a bridge");
        Expect(VillageChoicePreview.TryCreate(world, VillageChoice.ShadeTree, out treeAgain),
            "compare the tree again without committing");
        string bridgeBefore = Snapshot(bridge.WorldAfter);
        string treeBefore = Snapshot(treeAgain.WorldAfter);
        Expect(Snapshot(tree.WorldAfter) == treeBefore, "returning to the same option is deterministic");
        Expect(tree.WorldAfter.Count(VillageChoice.Bridge) == 0 &&
               bridge.WorldAfter.Count(VillageChoice.ShadeTree) == 0,
            "a later preview does not inherit an earlier tentative choice");
        int tile = tree.NextPuzzle.IsAnchored(0) ? 1 : 0;
        Expect(tree.WorldAfter.Rotate(tile), "a detached projection can be changed independently");
        Expect(Snapshot(world) == before && Snapshot(bridge.WorldAfter) == bridgeBefore &&
               Snapshot(treeAgain.WorldAfter) == treeBefore,
            "even changing a detached projection cannot affect the player or another preview");
        Expect(world.Choose(VillageChoice.Flowers) && world.Choices.Length == 1 &&
               world.Choices[0] == (int)VillageChoice.Flowers,
            "after discarding previews, only the explicitly committed choice is remembered");
        Expect(world.ReconsiderLatestChoice(), "a committed option can still be reconsidered");
        VillageChoicePreview replacement;
        Expect(VillageChoicePreview.TryCreate(world, VillageChoice.Bridge, out replacement) &&
               replacement.WorldAfter.Choices.Length == 1 &&
               replacement.WorldAfter.Choices[0] == (int)VillageChoice.Bridge,
            "preview also works after reconsidering, without keeping the discarded memory");
    }

    private static void InvalidRequestsDoNotChangeTheGarden()
    {
        VillageChoicePreview preview;
        Expect(!VillageChoicePreview.TryCreate(null, VillageChoice.ShadeTree, out preview) && preview == null,
            "missing world has no preview");
        WorldModel world = WorldModel.New();
        Expect(world.Rotate(0) && world.AdvanceGentleHint() == 1 && world.AdvanceGentleHint() == 2,
            "unsolved fixture has staged help and undo state");
        string before = Snapshot(world);
        Expect(!VillageChoicePreview.TryCreate(world, VillageChoice.ShadeTree, out preview) && preview == null &&
               Snapshot(world) == before, "unsolved preview cannot consume staged help or undo");
        Solve(world);
        before = Snapshot(world);
        foreach (int option in new[] { -1, 3, int.MaxValue })
            Expect(!VillageChoicePreview.TryCreate(world, (VillageChoice)option, out preview) &&
                   preview == null && Snapshot(world) == before, "invalid enum option is rejected without change");

        var schedule = new int[WorldModel.ScheduleLength];
        Array.Fill(schedule, 1);
        foreach (WorldModel control in new[] { WorldModel.New(PlayMode.PuzzleOnly), WorldModel.NewMatched(schedule) })
        {
            Solve(control);
            before = Snapshot(control);
            Expect(!VillageChoicePreview.TryCreate(control, VillageChoice.Bridge, out preview) && preview == null &&
                   Snapshot(control) == before, "research control has no story-choice preview");
        }

        for (int chapter = 0; chapter < PuzzleCatalog.Count; chapter++)
        {
            if (!world.Solved) Solve(world);
            Expect(world.Choose(VillageChoice.Flowers), "complete the garden fixture");
        }
        before = Snapshot(world);
        Expect(!VillageChoicePreview.TryCreate(world, VillageChoice.ShadeTree, out preview) && preview == null &&
               Snapshot(world) == before, "finished garden cannot accumulate a thirteenth choice");
    }

    private static void MigratedBoardsStayIntact()
    {
        for (int version = 1; version <= 3; version++)
        {
            var snapshot = new WorldSnapshot {
                version = version, mode = "living", round = 3, solved = false,
                rotations = (int[])PuzzleCatalog.Get(3).Initial.Clone(),
                previous = Array.Empty<int>(), choices = new[] { 0, 1, 2 }
            };
            WorldModel migrated;
            Expect(WorldModel.TryRestore(snapshot, out migrated) && migrated.BoardVariant == 0,
                "v" + version + " fixture retains its original unrotated board");
            Solve(migrated);
            string before = Snapshot(migrated);
            VillageChoicePreview preview;
            Expect(VillageChoicePreview.TryCreate(migrated, VillageChoice.Bridge, out preview),
                "migrated board offers a factual choice preview");
            WorldModel committed = Copy(migrated);
            Expect(committed.Choose(VillageChoice.Bridge) && Snapshot(preview.WorldAfter) == Snapshot(committed),
                "preview predicts the real transition from legacy to a reactive next board");
            Expect(Snapshot(migrated) == before, "preview never rewrites the migrated solved board");
            Expect(preview.WorldAfter.ReconsiderLatestChoice() && preview.WorldAfter.BoardVariant == 0 &&
                   preview.WorldAfter.Round == migrated.Round,
                "detached result preserves the exact legacy board to reconsider");
            Expect(Snapshot(migrated) == before, "reconsidering the detached result leaves the real legacy garden intact");
        }
    }

    public static int Run()
    {
        assertions = 0;
        EveryChapterAndChoiceMatchesCommit();
        ComparisonsAreIsolated();
        InvalidRequestsDoNotChangeTheGarden();
        MigratedBoardsStayIntact();
        Console.WriteLine("Evergrove choice preview tests: " + assertions + " assertions.");
        return assertions;
    }
}
