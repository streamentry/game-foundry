using System;

namespace Evergrove.Core
{
    /// <summary>
    /// Living: village choices shape later boards. PuzzleOnly: unchanged
    /// authored boards, no choices. PuzzleMatched: no choices or story, but
    /// boards follow a predeclared variant schedule (the M4 control).
    /// </summary>
    public enum PlayMode { Living, PuzzleOnly, PuzzleMatched }
    public enum VillageChoice { ShadeTree = 0, Bridge = 1, Flowers = 2 }

    /// <summary>
    /// Unity JsonUtility compatible, stable flat schema. Version 1 (6 rooms)
    /// upgrades through v2, v3 and v4, preserving in-progress authored boards.
    /// </summary>
    [Serializable]
    public sealed class WorldSnapshot
    {
        public int version = 4;
        public string mode = "living";
        public int round;
        public bool solved;
        public int[] rotations = Array.Empty<int>();
        public int[] previous = Array.Empty<int>();
        public int[] choices = Array.Empty<int>();
        public int hintsUsed;
        public int boardVariant;      // 0=legacy/control, 1=tree, 2=bridge, 3=meadow
        public int lastSolvedVariant; // exact board to reopen after reconsidering
        public int hintStep;          // 0=no hint, 1=row noticed, 2=tile focused
        public int hintTile = -1;     // chosen wrong tile, -1 when hintStep=0
        // Optional, additive (absent in older JSON). Only for mode "matched":
        // the variant of boards 2..12, each 1..3, fixed when the run began.
        public int[] variantSchedule = Array.Empty<int>();
        // Optional, additive (EG-111, absent in older JSON). Flat undo stack of
        // prior tile-rotation boards for the current chapter, oldest first.
        // Length is a multiple of the board's cell count, at most 10 entries.
        // `previous` mirrors the newest entry for older binaries.
        public int[] history = Array.Empty<int>();
        // Optional, additive (E4, I177; absent in older JSON). Which garden of the
        // catalogue this is (0, 1, 2…), and where each choice's item stands on the
        // garden map: placements[i] is a map cell for choice i, -1 = not placed.
        public int garden;
        public int[] placements = Array.Empty<int>();
        // Optional, additive (I193; absent in older JSON). Free planting: more of
        // what you've grown, in pairs [choice index, map cell, choice index, map cell, ...].
        public int[] plantings = Array.Empty<int>();
        // Optional, additive (I204; absent in older JSON). Seeds earned by solving
        // the puzzles your grown things hold, and which things you have solved.
        public int seeds;
        public int[] treeSolved = Array.Empty<int>();
    }

    /// <summary>
    /// The authoritative offline rules. This class has zero Unity dependencies,
    /// zero wall-clock requirements and no hidden randomness.
    /// </summary>
    public sealed class WorldModel
    {
        public const int Version = 4;
        /// <summary>Maximum number of tile-rotation undo steps kept for one board (EG-111).</summary>
        public const int UndoLimit = 10;
        private readonly WorldSnapshot state;

        private WorldModel(WorldSnapshot snapshot) { state = snapshot; }

        public PlayMode Mode {
            get {
                if (state.mode == "living") return PlayMode.Living;
                return state.mode == "matched" ? PlayMode.PuzzleMatched : PlayMode.PuzzleOnly;
            }
        }
        /// <summary>Number of later boards whose variant is predeclared (boards 2..12).</summary>
        public static int ScheduleLength { get { return PuzzleCatalog.Count - 1; } }
        public int[] VariantSchedule { get { return (int[])state.variantSchedule.Clone(); } }
        public int Round { get { return state.round; } }
        public bool Solved { get { return state.solved; } }
        public bool Completed { get { return state.round == PuzzleCatalog.Count; } }
        /// <summary>Number of undo steps available on the current board, 0..UndoLimit.</summary>
        public int UndoDepth {
            get {
                return Completed || state.history.Length == 0 ? 0 : state.history.Length / CurrentPuzzle.CellCount;
            }
        }
        public bool CanUndo { get { return UndoDepth > 0; } }
        public int HintsUsed { get { return state.hintsUsed; } }
        public int HintStep { get { return state.hintStep; } }
        public int HintTile { get { return state.hintTile; } }
        public int BoardVariant { get { return state.boardVariant; } }
        public Puzzle CurrentPuzzle { get { return Completed ? null : ReactiveWatercourses.ForChapter(Round, state.boardVariant); } }
        public int[] Rotations { get { return (int[])state.rotations.Clone(); } }
        public int[] Choices { get { return (int[])state.choices.Clone(); } }
        public PipeInsight Insight {
            get {
                return Completed ? new PipeInsight(0, 0, 0, Array.Empty<bool>()) :
                    PipeRules.Inspect(CurrentPuzzle, state.rotations);
            }
        }

        public const int MaxGarden = 1000000;
        public const int MaxMapCell = 10000;

        public int Garden { get { return state.garden; } }
        public int[] Placements { get { return (int[])state.placements.Clone(); } }
        /// <summary>Most things planted from the basket in one garden (I193).</summary>
        public const int MaxPlantings = 200;
        /// <summary>Free plantings as pairs [choice index, cell, ...] (I193).</summary>
        public int[] Plantings { get { return (int[])state.plantings.Clone(); } }
        public int PlantingCount { get { return state.plantings.Length / 2; } }

        // ---- I204: every grown thing holds a puzzle; solving it earns seeds.
        public const int MaxSeeds = 9999999;
        private static readonly int[] FirstReward = { 10, 20, 40 };
        private static readonly int[] ReplayReward = { 2, 4, 8 };
        public int Seeds { get { return state.seeds; } }

        public bool TreeSolved(int item) { return Array.IndexOf(state.treeSolved, item) >= 0; }

        /// <summary>Seeds for solving a tier's puzzle: most the first time, a little on every replay.</summary>
        public static int TreeReward(int tier, bool first)
        {
            int t = Math.Max(0, Math.Min(TreePuzzles.Tiers - 1, tier));
            return first ? FirstReward[t] : ReplayReward[t];
        }

        /// <summary>Records a solved puzzle of grown thing <paramref name="item"/> and returns the seeds earned.</summary>
        public int SolveTree(int item)
        {
            if (Mode != PlayMode.Living || item < 0 || item >= state.choices.Length) return 0;
            bool first = !TreeSolved(item);
            int reward = TreeReward(TreePuzzles.TierFor(item), first);
            if (first)
            {
                int[] next = new int[state.treeSolved.Length + 1];
                Array.Copy(state.treeSolved, next, state.treeSolved.Length);
                next[next.Length - 1] = item;
                state.treeSolved = next;
            }
            state.seeds = Math.Min(MaxSeeds, state.seeds + reward);
            return reward;
        }

        /// <summary>Plants one more of choice <paramref name="source"/>'s item on a cell (the map checks the land).</summary>
        public bool Plant(int source, int cell)
        {
            if (Mode != PlayMode.Living || source < 0 || source >= state.choices.Length ||
                cell < 0 || cell >= MaxMapCell || PlantingCount >= MaxPlantings) return false;
            int[] next = new int[state.plantings.Length + 2];
            Array.Copy(state.plantings, next, state.plantings.Length);
            next[next.Length - 2] = source;
            next[next.Length - 1] = cell;
            state.plantings = next;
            return true;
        }

        /// <summary>Moves planting <paramref name="k"/> to another cell.</summary>
        public bool MovePlanting(int k, int cell)
        {
            if (k < 0 || k >= PlantingCount || cell < 0 || cell >= MaxMapCell) return false;
            int[] next = (int[])state.plantings.Clone();
            next[k * 2 + 1] = cell;
            state.plantings = next;
            return true;
        }

        /// <summary>Takes planting <paramref name="k"/> back into the basket.</summary>
        public bool RemovePlanting(int k)
        {
            if (k < 0 || k >= PlantingCount) return false;
            int[] next = new int[state.plantings.Length - 2];
            Array.Copy(state.plantings, 0, next, 0, k * 2);
            Array.Copy(state.plantings, k * 2 + 2, next, k * 2, state.plantings.Length - k * 2 - 2);
            state.plantings = next;
            return true;
        }

        /// <summary>A new garden of the given catalogue number (living mode only uses it).</summary>
        public static WorldModel New(PlayMode mode, int garden)
        {
            WorldModel world = New(mode);
            world.state.garden = Math.Max(0, Math.Min(MaxGarden, garden));
            return world;
        }

        /// <summary>Puts choice <paramref name="index"/>'s item on a map cell (the map checks the land).</summary>
        public bool PlaceAt(int index, int cell)
        {
            if (Mode != PlayMode.Living || index < 0 || index >= state.choices.Length ||
                cell < -1 || cell >= MaxMapCell) return false;
            int[] next = new int[state.choices.Length];
            for (int i = 0; i < next.Length; i++)
                next[i] = i < state.placements.Length ? state.placements[i] : -1;
            next[index] = cell;
            state.placements = next;
            return true;
        }

        public static WorldModel New(PlayMode mode = PlayMode.Living)
        {
            if (mode == PlayMode.PuzzleMatched)
                throw new ArgumentException("Matched mode needs a variant schedule; use NewMatched.", nameof(mode));
            return Fresh(mode == PlayMode.Living ? "living" : "puzzle", Array.Empty<int>());
        }

        /// <summary>
        /// Matched run: same board stream a Living player would receive for
        /// the given variants (schedule[i] = variant of board i+2), but no village
        /// choices. The schedule is copied and persisted with the run.
        /// </summary>
        public static WorldModel NewMatched(int[] variantSchedule)
        {
            if (!IsValidSchedule(variantSchedule))
                throw new ArgumentException("Schedule must have " + ScheduleLength + " entries, each 1..3.", nameof(variantSchedule));
            return Fresh("matched", variantSchedule);
        }

        /// <summary>True when the schedule has exactly ScheduleLength entries, each 1..3.</summary>
        public static bool IsValidSchedule(int[] schedule)
        {
            if (schedule == null || schedule.Length != ScheduleLength) return false;
            foreach (int variant in schedule)
                if (variant < 1 || variant > 3) return false;
            return true;
        }

        /// <summary>
        /// Facilitator entry: exactly ScheduleLength characters, each '1'..'3'.
        /// No whitespace or separators are accepted; the caller trims input.
        /// </summary>
        public static bool TryParseSchedule(string text, out int[] schedule)
        {
            schedule = null;
            if (text == null || text.Length != ScheduleLength) return false;
            int[] parsed = new int[ScheduleLength];
            for (int i = 0; i < ScheduleLength; i++)
            {
                char digit = text[i];
                if (digit < '1' || digit > '3') return false;
                parsed[i] = digit - '0';
            }
            schedule = parsed;
            return true;
        }

        /// <summary>Inverse of TryParseSchedule, for copying a schedule to share with a facilitator.</summary>
        public static string FormatSchedule(int[] schedule)
        {
            if (!IsValidSchedule(schedule))
                throw new ArgumentException("Schedule must have " + ScheduleLength + " entries, each 1..3.", nameof(schedule));
            char[] digits = new char[schedule.Length];
            for (int i = 0; i < schedule.Length; i++) digits[i] = (char)('0' + schedule[i]);
            return new string(digits);
        }

        /// <summary>
        /// Converts a Living run's choices into the schedule a matched player needs.
        /// Only the first ScheduleLength choices affect boards; the last choice is inert.
        /// </summary>
        public static int[] ScheduleFromChoices(int[] choices)
        {
            if (choices == null || choices.Length < ScheduleLength)
                throw new ArgumentException("Need at least " + ScheduleLength + " Living choices.", nameof(choices));
            int[] schedule = new int[ScheduleLength];
            for (int i = 0; i < ScheduleLength; i++)
            {
                if (choices[i] < 0 || choices[i] > 2)
                    throw new ArgumentException("Choice out of range.", nameof(choices));
                schedule[i] = ReactiveWatercourses.VariantAfterChoice((VillageChoice)choices[i]);
            }
            return schedule;
        }

        /// <summary>Variant of the board at a chapter index; chapter 0 and completion are unchanged.</summary>
        private static int MatchedVariantFor(int[] schedule, int chapter)
        {
            return chapter <= 0 || chapter >= PuzzleCatalog.Count ? 0 : schedule[chapter - 1];
        }

        private static WorldModel Fresh(string mode, int[] schedule)
        {
            return new WorldModel(new WorldSnapshot {
                version = Version, mode = mode,
                round = 0, solved = false, hintsUsed = 0,
                boardVariant = 0, lastSolvedVariant = 0,
                hintStep = 0, hintTile = -1,
                rotations = (int[])PuzzleCatalog.Get(0).Initial.Clone(),
                previous = Array.Empty<int>(), choices = Array.Empty<int>(),
                variantSchedule = (int[])schedule.Clone(), history = Array.Empty<int>(),
                garden = 0, placements = Array.Empty<int>(), plantings = Array.Empty<int>(),
                seeds = 0, treeSolved = Array.Empty<int>()
            });
        }

        public WorldSnapshot Export()
        {
            return new WorldSnapshot {
                version = Version, mode = state.mode, round = Round, solved = Solved,
                rotations = (int[])state.rotations.Clone(), previous = (int[])state.previous.Clone(),
                choices = (int[])state.choices.Clone(), hintsUsed = state.hintsUsed,
                boardVariant = state.boardVariant, lastSolvedVariant = state.lastSolvedVariant,
                hintStep = state.hintStep, hintTile = state.hintTile,
                variantSchedule = (int[])state.variantSchedule.Clone(),
                history = (int[])state.history.Clone(),
                garden = state.garden, placements = (int[])state.placements.Clone(),
                plantings = (int[])state.plantings.Clone(),
                seeds = state.seeds, treeSolved = (int[])state.treeSolved.Clone()
            };
        }

        /// <summary>Fail closed on corruption, never silently report invalid data as a valid save.</summary>
        public static bool TryRestore(WorldSnapshot source, out WorldModel result)
        {
            result = null;
            if (source == null || (source.version != 1 && source.version != 2 &&
                 source.version != 3 && source.version != Version) ||
                (source.mode != "living" && source.mode != "puzzle" && source.mode != "matched") ||
                (source.mode == "matched" && source.version != Version) ||
                source.round < 0 || source.round > PuzzleCatalog.Count ||
                source.choices == null || source.rotations == null || source.previous == null ||
                source.history == null ||
                (source.version != Version && source.history.Length != 0) ||
                source.hintsUsed < 0 || source.hintsUsed > 1000000 ||
                (source.version >= 3 &&
                 (source.boardVariant < 0 || source.boardVariant > 3 ||
                  source.lastSolvedVariant < 0 || source.lastSolvedVariant > 3)) ||
                (source.version == Version &&
                 (source.hintStep < 0 || source.hintStep > 2 ||
                  (source.hintStep == 0 && source.hintTile != -1))))
                return false;
            int[] schedule = source.variantSchedule ?? Array.Empty<int>();
            if ((source.mode == "living" && source.choices.Length != source.round) ||
                (source.mode != "living" && source.choices.Length != 0))
                return false;
            if (source.mode == "matched" ? !IsValidSchedule(schedule) : schedule.Length != 0)
                return false;
            foreach (int choice in source.choices)
                if (choice < 0 || choice > 2) return false;
            int[] placements = source.placements ?? Array.Empty<int>();
            if (source.garden < 0 || source.garden > MaxGarden ||
                placements.Length > source.choices.Length ||
                (source.mode != "living" && (placements.Length != 0 || source.garden != 0)))
                return false;
            foreach (int cell in placements)
                if (cell < -1 || cell >= MaxMapCell) return false;
            int[] plantings = source.plantings ?? Array.Empty<int>();
            if (plantings.Length % 2 != 0 || plantings.Length > MaxPlantings * 2 ||
                (source.mode != "living" && plantings.Length != 0))
                return false;
            for (int i = 0; i < plantings.Length; i += 2)
                if (plantings[i] < 0 || plantings[i] >= source.choices.Length ||
                    plantings[i + 1] < 0 || plantings[i + 1] >= MaxMapCell) return false;
            int[] treeSolved = source.treeSolved ?? Array.Empty<int>();
            if (source.seeds < 0 || source.seeds > MaxSeeds || treeSolved.Length > source.choices.Length ||
                (source.mode != "living" && (source.seeds != 0 || treeSolved.Length != 0)))
                return false;
            for (int i = 0; i < treeSolved.Length; i++)
                if (treeSolved[i] < 0 || treeSolved[i] >= source.choices.Length ||
                    Array.IndexOf(treeSolved, treeSolved[i]) != i) return false;

            int chapter = source.round;
            int[] rotations = (int[])source.rotations.Clone();
            int[] previous = (int[])source.previous.Clone();
            // Legacy saves carry only `previous`; it becomes a one-entry history.
            int[] history = source.history.Length == 0 && previous.Length != 0
                ? (int[])previous.Clone() : (int[])source.history.Clone();
            bool solved = source.solved;
            int variant = source.version >= 3 ? source.boardVariant : 0;
            int lastVariant = source.version >= 3 ? source.lastSolvedVariant : 0;
            int hintStep = source.version == Version ? source.hintStep : 0;
            int hintTile = source.version == Version ? source.hintTile : -1;
            if (source.mode == "matched")
            {
                if (variant != MatchedVariantFor(schedule, chapter) ||
                    lastVariant != MatchedVariantFor(schedule, chapter - 1))
                    return false;
            }
            else if ((source.mode == "puzzle" && (variant != 0 || lastVariant != 0)) ||
                (chapter == 0 && variant != 0) ||
                (chapter == PuzzleCatalog.Count && variant != 0) ||
                (variant != 0 && (chapter < 1 || source.choices[chapter - 1] + 1 != variant)))
                return false;

            // The original prototype ended after six chapters. In v2 that
            // finished game becomes the first new 4x4 stream, preserving every
            // recorded choice and clearing only ephemeral board state.
            if (source.version == 1 && chapter == 6 &&
                rotations.Length == 0 && previous.Length == 0 && !solved)
            {
                rotations = (int[])PuzzleCatalog.Get(chapter).Initial.Clone();
            }
            if (chapter == PuzzleCatalog.Count)
            {
                if (solved || rotations.Length != 0 || previous.Length != 0 || history.Length != 0 ||
                    hintStep != 0 || hintTile != -1)
                    return false;
            }
            else
            {
                Puzzle puzzle = ReactiveWatercourses.ForChapter(chapter, variant);
                if (!PipeRules.ValidRotations(puzzle, rotations) ||
                    solved != PipeRules.IsComplete(puzzle, rotations) ||
                    (previous.Length != 0 && !PipeRules.ValidRotations(puzzle, previous)) ||
                    !IsValidHistory(puzzle, history, previous))
                    return false;
                if (hintStep > 0 &&
                    (solved || hintTile < 0 || hintTile >= puzzle.CellCount ||
                     puzzle.IsAnchored(hintTile) ||
                     rotations[hintTile] == puzzle.Solution[hintTile]))
                    return false;
            }
            result = new WorldModel(new WorldSnapshot {
                version = Version, mode = source.mode, round = chapter, solved = solved,
                rotations = rotations, previous = previous,
                choices = (int[])source.choices.Clone(), hintsUsed = source.hintsUsed,
                boardVariant = variant, lastSolvedVariant = lastVariant,
                hintStep = hintStep, hintTile = hintTile,
                variantSchedule = (int[])schedule.Clone(), history = history,
                garden = source.garden, placements = (int[])placements.Clone(),
                plantings = (int[])plantings.Clone(),
                seeds = source.seeds, treeSolved = (int[])treeSolved.Clone()
            });
            return true;
        }

        /// <summary>
        /// Undo stack must be whole boards of the current size, at most UndoLimit
        /// deep, each a valid orientation set, and its newest entry must equal `previous`.
        /// </summary>
        private static bool IsValidHistory(Puzzle puzzle, int[] history, int[] previous)
        {
            int cells = puzzle.CellCount;
            if (history.Length % cells != 0 || history.Length / cells > UndoLimit) return false;
            for (int entry = 0; entry < history.Length / cells; entry++)
            {
                int[] board = new int[cells];
                Array.Copy(history, entry * cells, board, 0, cells);
                if (!PipeRules.ValidRotations(puzzle, board)) return false;
            }
            if (history.Length == 0) return previous.Length == 0;
            if (previous.Length != cells) return false;
            for (int i = 0; i < cells; i++)
                if (history[history.Length - cells + i] != previous[i]) return false;
            return true;
        }

        public static WorldModel Restore(WorldSnapshot source)
        {
            WorldModel result;
            return TryRestore(source, out result) ? result : New();
        }

        public bool CanSwitchMode {
            get {
                if (Mode == PlayMode.PuzzleMatched) return false; // research condition is fixed at start
                if (Completed || Round != 0 || Solved || state.choices.Length != 0) return false;
                int[] initial = PuzzleCatalog.Get(0).Initial;
                for (int i = 0; i < initial.Length; i++)
                    if (state.rotations[i] != initial[i]) return false;
                return true;
            }
        }

        private void ClearHint()
        {
            state.hintStep = 0;
            state.hintTile = -1;
        }

        /// <summary>Replaces the undo stack and keeps the legacy `previous` mirror in step.</summary>
        private void SetHistory(int[] flat, int cells)
        {
            state.history = flat;
            state.previous = new int[0];
            if (flat.Length == 0) return;
            state.previous = new int[cells];
            Array.Copy(flat, flat.Length - cells, state.previous, 0, cells);
        }

        /// <summary>Pushes the board as it was before a mutation, dropping the oldest step past the limit.</summary>
        private void PushHistory(int[] before)
        {
            int cells = before.Length;
            int keep = Math.Min(state.history.Length / cells, UndoLimit - 1) * cells;
            int[] next = new int[keep + cells];
            Array.Copy(state.history, state.history.Length - keep, next, 0, keep);
            Array.Copy(before, 0, next, keep, cells);
            SetHistory(next, cells);
        }

        /// <summary>
        /// Three requested stages: identify one row, focus a tile, finally
        /// align that one tile. The first two never mutate puzzle rotations.
        /// Stage progress is saved so reopening cannot silently jump to help.
        /// </summary>
        public int AdvanceGentleHint()
        {
            if (Completed || Solved) return 0;
            Puzzle puzzle = CurrentPuzzle;
            if (state.hintStep == 0)
            {
                int tile = HintPlanner.SelectTarget(puzzle, state.rotations);
                if (tile < 0) return 0;
                state.hintTile = tile;
                state.hintStep = 1;
                return 1;
            }
            if (state.hintStep == 1)
            {
                state.hintStep = 2;
                return 2;
            }
            int chosen = state.hintTile;
            if (chosen < 0 || chosen >= puzzle.CellCount ||
                puzzle.IsAnchored(chosen) || state.rotations[chosen] == puzzle.Solution[chosen])
            {
                ClearHint();
                return 0;
            }
            PushHistory((int[])state.rotations.Clone());
            state.rotations[chosen] = puzzle.Solution[chosen];
            state.hintsUsed++;
            ClearHint();
            state.solved = PipeRules.IsComplete(puzzle, state.rotations);
            return 3;
        }

        public bool Rotate(int tile)
        {
            if (Completed || Solved) return false;
            Puzzle puzzle = CurrentPuzzle;
            if (tile < 0 || tile >= puzzle.CellCount || puzzle.IsAnchored(tile)) return false;
            PushHistory((int[])state.rotations.Clone());
            ClearHint();
            state.rotations[tile] = state.rotations[tile] % PipeRules.OrientationCount(puzzle.Kinds[tile]) + 1;
            state.solved = PipeRules.IsComplete(puzzle, state.rotations);
            return true;
        }

        /// <summary>
        /// Backward-compatible direct help used by existing tests and old
        /// gameplay code. The UI uses AdvanceGentleHint instead.
        /// </summary>
        public bool GuideOneConnection()
        {
            if (Completed || Solved) return false;
            Puzzle puzzle = CurrentPuzzle;
            int tile = -1;
            for (int i = 0; i < puzzle.CellCount; i++)
                if (!puzzle.IsAnchored(i) && state.rotations[i] != puzzle.Solution[i])
                { tile = i; break; }
            if (tile < 0) return false;
            PushHistory((int[])state.rotations.Clone());
            state.rotations[tile] = puzzle.Solution[tile];
            state.hintsUsed++;
            ClearHint();
            state.solved = PipeRules.IsComplete(puzzle, state.rotations);
            return true;
        }

        public bool Undo()
        {
            if (!CanUndo) return false;
            int cells = CurrentPuzzle.CellCount;
            int[] remaining = new int[state.history.Length - cells];
            Array.Copy(state.history, remaining, remaining.Length);
            int[] restored = new int[cells];
            Array.Copy(state.history, remaining.Length, restored, 0, cells);
            SetHistory(remaining, cells);
            state.rotations = restored;
            ClearHint();
            state.solved = PipeRules.IsComplete(CurrentPuzzle, state.rotations);
            return true;
        }

        public bool Restart()
        {
            if (Completed) return false;
            state.rotations = (int[])CurrentPuzzle.Initial.Clone();
            SetHistory(Array.Empty<int>(), 0);
            ClearHint();
            state.solved = PipeRules.IsComplete(CurrentPuzzle, state.rotations);
            return true;
        }

        private void Advance()
        {
            state.lastSolvedVariant = state.boardVariant;
            ClearHint();
            state.round++;
            state.solved = false;
            SetHistory(Array.Empty<int>(), 0);
            state.boardVariant = Completed ? 0 : NextVariant();
            state.rotations = Completed ? Array.Empty<int>() : (int[])CurrentPuzzle.Initial.Clone();
        }

        private int NextVariant()
        {
            if (Mode == PlayMode.PuzzleOnly) return 0;
            if (Mode == PlayMode.PuzzleMatched) return MatchedVariantFor(state.variantSchedule, Round);
            return ReactiveWatercourses.VariantAfterChoice((VillageChoice)state.choices[state.choices.Length - 1]);
        }

        public bool Choose(VillageChoice choice)
        {
            if (Mode != PlayMode.Living || Completed || !Solved ||
                (int)choice < 0 || (int)choice > 2 || state.choices.Length != Round ||
                !PipeRules.IsComplete(CurrentPuzzle, state.rotations))
                return false;
            int[] next = new int[state.choices.Length + 1];
            Array.Copy(state.choices, next, state.choices.Length);
            next[next.Length - 1] = (int)choice;
            state.choices = next;
            // The new item is not placed yet; earlier placements stay as they are.
            int[] placed = new int[next.Length];
            for (int i = 0; i < placed.Length; i++)
                placed[i] = i < state.placements.Length ? state.placements[i] : -1;
            state.placements = placed;
            Advance();
            return true;
        }

        /// <summary>
        /// Correct a mis-tap while the choice is still on the memory interlude.
        /// The previous puzzle is reopened as solved so the player can choose
        /// another village outcome without replaying the entire board.
        /// </summary>
        public bool ReconsiderLatestChoice()
        {
            if (Mode != PlayMode.Living || Round == 0 ||
                state.choices.Length != Round) return false;
            int previousChapter = Round - 1;
            int[] trimmed = new int[state.choices.Length - 1];
            Array.Copy(state.choices, trimmed, trimmed.Length);
            state.choices = trimmed;
            if (state.placements.Length > trimmed.Length)
            {
                int[] kept = new int[trimmed.Length];
                Array.Copy(state.placements, kept, kept.Length);
                state.placements = kept;
            }
            // Plantings of the reconsidered thing go back to the basket with it (I193).
            var stillOurs = new System.Collections.Generic.List<int>();
            for (int i = 0; i + 1 < state.plantings.Length; i += 2)
                if (state.plantings[i] < trimmed.Length) { stillOurs.Add(state.plantings[i]); stillOurs.Add(state.plantings[i + 1]); }
            state.plantings = stillOurs.ToArray();
            // A reconsidered thing's puzzle is no longer solved; seeds earned stay.
            state.treeSolved = Array.FindAll(state.treeSolved, item => item < trimmed.Length);
            state.round = previousChapter;
            state.boardVariant = state.lastSolvedVariant;
            state.rotations = (int[])CurrentPuzzle.Solution.Clone();
            SetHistory(Array.Empty<int>(), 0);
            ClearHint();
            state.solved = true;
            return true;
        }

        public bool NextPuzzle()
        {
            if (Mode == PlayMode.Living || Completed || !Solved ||
                !PipeRules.IsComplete(CurrentPuzzle, state.rotations)) return false;
            Advance();
            return true;
        }

        public int Count(VillageChoice choice)
        {
            int count = 0;
            foreach (int c in state.choices) if (c == (int)choice) count++;
            return count;
        }

        public string Memory { get { return VillageStory.Memory(this); } }
        public string Consequence { get { return VillageStory.Consequence(this); } }
        public string[] Characters { get { return VillageStory.Characters(this); } }
        public string[] Journal { get { return VillageStory.Journal(this); } }
        public string Ending { get { return VillageStory.Ending(this); } }
    }
}
