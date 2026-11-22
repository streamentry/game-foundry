using System;

namespace Evergrove.Core
{
    public enum PlayMode { Living, PuzzleOnly }
    public enum VillageChoice { ShadeTree = 0, Bridge = 1, Flowers = 2 }

    /// <summary>
    /// Unity JsonUtility compatible, stable flat schema. Version 1 (6 rooms)
    /// upgrades in place to version 2 (12 rooms) without losing a player's garden.
    /// </summary>
    [Serializable]
    public sealed class WorldSnapshot
    {
        public int version = 2;
        public string mode = "living";
        public int round;
        public bool solved;
        public int[] rotations = Array.Empty<int>();
        public int[] previous = Array.Empty<int>();
        public int[] choices = Array.Empty<int>();
        public int hintsUsed;
    }

    /// <summary>
    /// The authoritative offline rules. This class has zero Unity dependencies,
    /// zero wall-clock requirements and no hidden randomness.
    /// </summary>
    public sealed class WorldModel
    {
        public const int Version = 2;
        private readonly WorldSnapshot state;

        private WorldModel(WorldSnapshot snapshot) { state = snapshot; }

        public PlayMode Mode { get { return state.mode == "living" ? PlayMode.Living : PlayMode.PuzzleOnly; } }
        public int Round { get { return state.round; } }
        public bool Solved { get { return state.solved; } }
        public bool Completed { get { return state.round == PuzzleCatalog.Count; } }
        public bool CanUndo { get { return !Completed && state.previous.Length == PuzzleCatalog.Get(Round).CellCount; } }
        public int HintsUsed { get { return state.hintsUsed; } }
        public int[] Rotations { get { return (int[])state.rotations.Clone(); } }
        public int[] Choices { get { return (int[])state.choices.Clone(); } }
        public PipeInsight Insight {
            get {
                return Completed ? new PipeInsight(0, 0, 0, Array.Empty<bool>()) :
                    PipeRules.Inspect(PuzzleCatalog.Get(Round), state.rotations);
            }
        }

        public static WorldModel New(PlayMode mode = PlayMode.Living)
        {
            return new WorldModel(new WorldSnapshot {
                version = Version, mode = mode == PlayMode.Living ? "living" : "puzzle",
                round = 0, solved = false, hintsUsed = 0,
                rotations = (int[])PuzzleCatalog.Get(0).Initial.Clone(),
                previous = Array.Empty<int>(), choices = Array.Empty<int>()
            });
        }

        public WorldSnapshot Export()
        {
            return new WorldSnapshot {
                version = Version, mode = state.mode, round = Round, solved = Solved,
                rotations = (int[])state.rotations.Clone(), previous = (int[])state.previous.Clone(),
                choices = (int[])state.choices.Clone(), hintsUsed = state.hintsUsed
            };
        }

        /// <summary>Fail closed on corruption, never silently report invalid data as a valid save.</summary>
        public static bool TryRestore(WorldSnapshot source, out WorldModel result)
        {
            result = null;
            if (source == null || (source.version != 1 && source.version != Version) ||
                (source.mode != "living" && source.mode != "puzzle") ||
                source.round < 0 || source.round > PuzzleCatalog.Count ||
                source.choices == null || source.rotations == null || source.previous == null ||
                source.hintsUsed < 0 || source.hintsUsed > 1000000)
                return false;
            if ((source.mode == "living" && source.choices.Length != source.round) ||
                (source.mode == "puzzle" && source.choices.Length != 0))
                return false;
            foreach (int choice in source.choices)
                if (choice < 0 || choice > 2) return false;

            int chapter = source.round;
            int[] rotations = (int[])source.rotations.Clone();
            int[] previous = (int[])source.previous.Clone();
            bool solved = source.solved;

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
                if (solved || rotations.Length != 0 || previous.Length != 0)
                    return false;
            }
            else
            {
                Puzzle puzzle = PuzzleCatalog.Get(chapter);
                if (!PipeRules.ValidRotations(puzzle, rotations) ||
                    solved != PipeRules.IsComplete(puzzle, rotations) ||
                    (previous.Length != 0 && !PipeRules.ValidRotations(puzzle, previous)))
                    return false;
            }
            result = new WorldModel(new WorldSnapshot {
                version = Version, mode = source.mode, round = chapter, solved = solved,
                rotations = rotations, previous = previous,
                choices = (int[])source.choices.Clone(), hintsUsed = source.hintsUsed
            });
            return true;
        }

        public static WorldModel Restore(WorldSnapshot source)
        {
            WorldModel result;
            return TryRestore(source, out result) ? result : New();
        }

        public bool CanSwitchMode {
            get {
                if (Completed || Round != 0 || Solved || state.choices.Length != 0) return false;
                int[] initial = PuzzleCatalog.Get(0).Initial;
                for (int i = 0; i < initial.Length; i++)
                    if (state.rotations[i] != initial[i]) return false;
                return true;
            }
        }

        public bool Rotate(int tile)
        {
            if (Completed || Solved) return false;
            Puzzle puzzle = PuzzleCatalog.Get(Round);
            if (tile < 0 || tile >= puzzle.CellCount) return false;
            state.previous = (int[])state.rotations.Clone();
            state.rotations[tile] = state.rotations[tile] % PipeRules.OrientationCount(puzzle.Kinds[tile]) + 1;
            state.solved = PipeRules.IsComplete(puzzle, state.rotations);
            return true;
        }

        /// <summary>Optional, no-pressure hint fixes one tile. Not a hidden auto-solve.</summary>
        public bool GuideOneConnection()
        {
            if (Completed || Solved) return false;
            Puzzle puzzle = PuzzleCatalog.Get(Round);
            for (int tile = 0; tile < puzzle.CellCount; tile++)
            {
                if (state.rotations[tile] == puzzle.Solution[tile]) continue;
                state.previous = (int[])state.rotations.Clone();
                state.rotations[tile] = puzzle.Solution[tile];
                state.hintsUsed++;
                state.solved = PipeRules.IsComplete(puzzle, state.rotations);
                return true;
            }
            return false;
        }

        public bool Undo()
        {
            if (!CanUndo) return false;
            state.rotations = state.previous;
            state.previous = Array.Empty<int>();
            state.solved = PipeRules.IsComplete(PuzzleCatalog.Get(Round), state.rotations);
            return true;
        }

        public bool Restart()
        {
            if (Completed) return false;
            state.rotations = (int[])PuzzleCatalog.Get(Round).Initial.Clone();
            state.previous = Array.Empty<int>();
            state.solved = PipeRules.IsComplete(PuzzleCatalog.Get(Round), state.rotations);
            return true;
        }

        private void Advance()
        {
            state.round++;
            state.solved = false;
            state.previous = Array.Empty<int>();
            state.rotations = Completed ? Array.Empty<int>() : (int[])PuzzleCatalog.Get(Round).Initial.Clone();
        }

        public bool Choose(VillageChoice choice)
        {
            if (Mode != PlayMode.Living || Completed || !Solved ||
                (int)choice < 0 || (int)choice > 2 || state.choices.Length != Round ||
                !PipeRules.IsComplete(PuzzleCatalog.Get(Round), state.rotations))
                return false;
            int[] next = new int[state.choices.Length + 1];
            Array.Copy(state.choices, next, state.choices.Length);
            next[next.Length - 1] = (int)choice;
            state.choices = next;
            Advance();
            return true;
        }

        public bool NextPuzzle()
        {
            if (Mode != PlayMode.PuzzleOnly || Completed || !Solved ||
                !PipeRules.IsComplete(PuzzleCatalog.Get(Round), state.rotations)) return false;
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
