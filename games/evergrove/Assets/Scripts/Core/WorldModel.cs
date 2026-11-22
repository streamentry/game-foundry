using System;

namespace Evergrove.Core
{
    public enum PlayMode { Living, PuzzleOnly }
    public enum VillageChoice { ShadeTree = 0, Bridge = 1, Flowers = 2 }

    /// <summary>Serializable with UnityEngine.JsonUtility, but has no Unity dependencies.</summary>
    [Serializable]
    public sealed class WorldSnapshot
    {
        public int version = 1;
        public string mode = "living";
        public int round;
        public bool solved;
        public int[] rotations = Array.Empty<int>();
        public int[] previous = Array.Empty<int>();
        public int[] choices = Array.Empty<int>();
    }

    public sealed class WorldModel
    {
        public const int Version = 1;
        private WorldSnapshot state;

        private WorldModel(WorldSnapshot snapshot) { state = snapshot; }

        public PlayMode Mode { get { return state.mode == "living" ? PlayMode.Living : PlayMode.PuzzleOnly; } }
        public int Round { get { return state.round; } }
        public bool Solved { get { return state.solved; } }
        public bool Completed { get { return state.round == PuzzleCatalog.Count; } }
        public bool CanUndo { get { return state.previous.Length == 9 && !Completed; } }
        public int[] Rotations { get { return (int[])state.rotations.Clone(); } }
        public int[] Choices { get { return (int[])state.choices.Clone(); } }

        public static WorldModel New(PlayMode mode = PlayMode.Living)
        {
            return new WorldModel(new WorldSnapshot {
                version = Version,
                mode = mode == PlayMode.Living ? "living" : "puzzle",
                round = 0, solved = false,
                rotations = (int[])PuzzleCatalog.Get(0).Initial.Clone(),
                previous = Array.Empty<int>(), choices = Array.Empty<int>()
            });
        }

        public WorldSnapshot Export()
        {
            return new WorldSnapshot {
                version = state.version, mode = state.mode, round = state.round, solved = state.solved,
                rotations = (int[])state.rotations.Clone(), previous = (int[])state.previous.Clone(),
                choices = (int[])state.choices.Clone()
            };
        }

        public static WorldModel Restore(WorldSnapshot candidate)
        {
            if (candidate == null || candidate.version != Version ||
                (candidate.mode != "living" && candidate.mode != "puzzle") ||
                candidate.round < 0 || candidate.round > PuzzleCatalog.Count || candidate.choices == null ||
                candidate.rotations == null || candidate.previous == null)
                return New();
            if (candidate.mode == "puzzle" && candidate.choices.Length != 0)
                return New();
            if (candidate.mode == "living" && candidate.choices.Length != candidate.round)
                return New();
            foreach (int choice in candidate.choices)
                if (choice < 0 || choice > 2) return New();

            if (candidate.round == PuzzleCatalog.Count)
            {
                if (candidate.solved || candidate.rotations.Length != 0 || candidate.previous.Length != 0)
                    return New();
            }
            else
            {
                Puzzle puzzle = PuzzleCatalog.Get(candidate.round);
                if (!PipeRules.ValidRotations(puzzle, candidate.rotations) ||
                    candidate.solved != PipeRules.IsComplete(puzzle, candidate.rotations) ||
                    (candidate.previous.Length != 0 && !PipeRules.ValidRotations(puzzle, candidate.previous)))
                    return New();
            }
            return new WorldModel(new WorldSnapshot {
                version = Version,
                mode = candidate.mode,
                round = candidate.round,
                solved = candidate.solved,
                rotations = (int[])candidate.rotations.Clone(),
                previous = (int[])candidate.previous.Clone(),
                choices = (int[])candidate.choices.Clone()
            });
        }

        public bool CanSwitchMode {
            get {
                if (Completed || Round != 0 || state.solved || state.choices.Length != 0) return false;
                int[] initial = PuzzleCatalog.Get(0).Initial;
                for (int i = 0; i < initial.Length; i++)
                    if (state.rotations[i] != initial[i]) return false;
                return true;
            }
        }

        public bool Rotate(int tile)
        {
            if (Completed || Solved || tile < 0 || tile >= 9) return false;
            Puzzle puzzle = PuzzleCatalog.Get(Round);
            state.previous = (int[])state.rotations.Clone();
            state.rotations[tile] = state.rotations[tile] % PipeRules.OrientationCount(puzzle.Kinds[tile]) + 1;
            state.solved = PipeRules.IsComplete(puzzle, state.rotations);
            return true;
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
            if (Mode != PlayMode.Living || Completed || !Solved || (int)choice < 0 || (int)choice > 2 ||
                state.choices.Length != Round || !PipeRules.IsComplete(PuzzleCatalog.Get(Round), state.rotations))
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

        public string Memory
        {
            get {
                if (state.choices.Length == 0) return "Your first choice will become the village's first memory.";
                switch ((VillageChoice)state.choices[state.choices.Length - 1])
                {
                    case VillageChoice.ShadeTree: return "Mira remembers the shade you planted.";
                    case VillageChoice.Bridge: return "Orin remembers the crossing you repaired.";
                    default: return "Fen remembers the wildflowers you protected.";
                }
            }
        }

        public string Consequence
        {
            get {
                int trees = Count(VillageChoice.ShadeTree);
                int bridges = Count(VillageChoice.Bridge);
                int flowers = Count(VillageChoice.Flowers);
                if (trees >= 2 && flowers >= 1) return "The trees shelter a meadow full of pollinators.";
                if (trees >= 1 && bridges >= 1) return "The new shade now reaches the restored crossing.";
                if (bridges >= 1 && flowers >= 1) return "Visitors pause at the crossing to watch the meadow.";
                if (trees >= 1) return "A young tree is taking root beside the stream.";
                if (bridges >= 1) return "The footbridge lets neighbors cross the stream.";
                if (flowers >= 1) return "Wildflowers are growing along the water.";
                return "The village waits beside a quiet, dry stream.";
            }
        }

        public string[] Characters {
            get {
                return new[] {
                    Count(VillageChoice.ShadeTree) > 0
                        ? "Mira · Gardener — I remember where we planted our trees."
                        : "Mira · Gardener — The stream could shelter a tree.",
                    Count(VillageChoice.Bridge) > 0
                        ? "Orin · Builder — The old crossing is open because of you."
                        : "Orin · Builder — The old crossing needs help.",
                    Count(VillageChoice.Flowers) > 0
                        ? "Fen · Naturalist — The meadow welcomes visitors."
                        : "Fen · Naturalist — The stream bank needs flowers."
                };
            }
        }
    }
}
