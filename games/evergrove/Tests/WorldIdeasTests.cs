using System;
using System.Collections.Generic;
using Evergrove.Core;

/// <summary>
/// Ideas from the world's games (docs/specs/proposals/world-games-50-ideas-2026-11-27.md),
/// the rules part of each, test-first.
/// </summary>
internal static class WorldIdeasTests
{
    private static int assertions;

    private static void Expect(bool ok, string message)
    {
        assertions++;
        if (!ok) throw new Exception("World ideas test failed: " + message);
    }

    public static int Run()
    {
        // Idea 3 (a flower card for each month): Pairs deals the flowers of the season.
        var seen = new HashSet<int>();
        foreach (Season season in new[] { Season.Spring, Season.Summer, Season.Autumn, Season.Winter })
        {
            int[] flowers = GardenCatalog.FlowersOf(season);
            Expect(flowers.Length >= 8, season + " has at least eight flowers for the biggest Pairs table");
            foreach (int f in flowers)
            {
                Expect(f >= 0 && f < GardenCatalog.CountOf(VillageChoice.Flowers), "a real flower " + f);
                Expect(seen.Add(f), "each flower belongs to one season (" + f + ")");
            }
        }
        Expect(seen.Count == GardenCatalog.CountOf(VillageChoice.Flowers), "every flower has its season");
        Expect(Array.IndexOf(GardenCatalog.FlowersOf(Season.Winter), Index("snowdrops")) >= 0, "snowdrops are winter");
        Expect(Array.IndexOf(GardenCatalog.FlowersOf(Season.Spring), Index("tulips")) >= 0, "tulips are spring");
        Expect(Array.IndexOf(GardenCatalog.FlowersOf(Season.Summer), Index("sunflowers")) >= 0, "sunflowers are summer");
        Expect(Array.IndexOf(GardenCatalog.FlowersOf(Season.Autumn), Index("asters")) >= 0, "asters are autumn");
        // Idea 2 (mahjong's season tiles): one face is the season set, exactly four tiles
        // drawn as four different seasons, and any two of them match.
        for (int tier = 0; tier < TreePuzzles.Tiers; tier++)
            for (int seed = 1; seed <= 40; seed++)
            {
                var game = new MahjongGame(seed * 7919, tier);
                int seasons = 0;
                for (int t = 0; t < game.TileCount; t++)
                    if (game.Face(t) == MahjongGame.SeasonFace) seasons++;
                Expect(seasons == 4, "four season tiles at tier " + tier + " seed " + seed);
                int[] set = MahjongGame.SeasonTiles(game);
                Expect(set.Length == 4, "the season set lists four tiles");
                for (int k = 1; k < set.Length; k++) Expect(set[k] > set[k - 1], "season tiles in a fixed order");
            }
        // Idea 7 (shogi's promotion): a thing is crowned once a replay was solved at the
        // deepest tier; never by its first solve, however deep.
        int deepest = TreePuzzles.Tiers - 1;
        for (int baseTier = 0; baseTier <= deepest; baseTier++)
        {
            Expect(!ReplayBoards.Crowned(baseTier, 0), "a first solve alone is never crowned (base " + baseTier + ")");
            bool was = false;
            for (int done = 1; done <= 12; done++)
            {
                bool now = ReplayBoards.Crowned(baseTier, done);
                Expect(now == (ReplayBoards.Tier(baseTier, done - 1) == deepest),
                    "crowned exactly when the last replay was at the deepest tier");
                Expect(!was || now, "a crown is never lost");
                was = now;
            }
            Expect(was, "every thing can be crowned by replaying");
        }
        Expect(!ReplayBoards.Crowned(0, ReplayBoards.ReplaysPerTier * 2), "base 0 needs a replay at the deepest tier");
        Expect(ReplayBoards.Crowned(0, ReplayBoards.ReplaysPerTier * 2 + 1), "base 0 is crowned on its seventh replay");
        Expect(ReplayBoards.Crowned(deepest, 1), "a deep thing is crowned on its first replay");
        // Idea 27 (hanami and momijigari): what drifts over home is the season's own, and
        // only while there is light to see it.
        foreach (DayPhase phase in new[] { DayPhase.Dawn, DayPhase.Morning, DayPhase.Day, DayPhase.Golden })
        {
            Expect(GardenSeason.DriftOf(Season.Spring, phase) == Drift.Petals, "spring petals by day");
            Expect(GardenSeason.DriftOf(Season.Summer, phase) == Drift.None, "summer is still");
            Expect(GardenSeason.DriftOf(Season.Autumn, phase) == Drift.Leaves, "autumn leaves by day");
            Expect(GardenSeason.DriftOf(Season.Winter, phase) == Drift.Snow, "winter snow by day");
        }
        foreach (Season season in new[] { Season.Spring, Season.Summer, Season.Autumn, Season.Winter })
            foreach (DayPhase phase in new[] { DayPhase.Dusk, DayPhase.Night })
                Expect(GardenSeason.DriftOf(season, phase) == Drift.None, "nothing drifts in the dark (fireflies' time)");
        // Idea 28 (hygge): the cottage's lamp is lit from dusk, through the night, to dawn.
        Expect(DayLight.LampLit(DayPhase.Dusk) && DayLight.LampLit(DayPhase.Night) && DayLight.LampLit(DayPhase.Dawn),
            "the lamp burns from dusk to dawn");
        Expect(!DayLight.LampLit(DayPhase.Morning) && !DayLight.LampLit(DayPhase.Day) && !DayLight.LampLit(DayPhase.Golden),
            "no lamp in daylight");
        Expect(DayLight.LampLit(DayLight.PhaseAt(22, 0)) && !DayLight.LampLit(DayLight.PhaseAt(13, 0)), "by the clock: 22:00 lit, 13:00 not");
        // Idea 36 (the inn as a calm summary): your own cottage, top right of the first
        // reach, is a place to tap; the path's built cells below it and later reaches are not.
        var river = GardenMap.River(10, 15, Mask(), 3);
        int cottageCells = 0;
        for (int cell = 0; cell < river.CellCount; cell++)
        {
            if (!river.IsCottage(cell)) continue;
            cottageCells++;
            Expect(river.At(cell) == GardenMap.Built, "the cottage is built ground");
            Expect(cell / river.Columns < 2, "the cottage is in the first two rows");
        }
        Expect(cottageCells == 6, "your cottage is three cells wide and two deep (" + cottageCells + ")");
        Expect(river.IsCottage(8) && river.IsCottage(17), "roof cells are the cottage");
        Expect(!river.IsCottage(2 * 10 + 8), "the path below is not");
        Expect(!river.IsCottage(15 * 10 + 8) && !river.IsCottage(-1) && !river.IsCottage(river.CellCount), "other reaches and outside cells are not");
        // ...and tapping it says, truly, what's waiting.
        Expect(CottageSummary.For(null, 0) == null, "no garden, no summary");
        WorldModel home = WorldModel.New(PlayMode.Living);
        CottageSummary quiet = CottageSummary.For(home, 0);
        Expect(quiet.Waiting == 0 && quiet.Letters == 0 && quiet.Cuttings == 0 && quiet.Seeds == home.Seeds &&
               quiet.Fruit == home.Fruit, "a new garden: nothing waiting");
        Expect(!quiet.Anything, "nothing waiting reads as nothing waiting");
        for (int chapter = 0; chapter < 4; chapter++)
        {
            Puzzle board = home.CurrentPuzzle;
            for (int i = 0; i < board.CellCount; i++)
                while (home.Rotations[i] != board.Solution[i]) home.Rotate(i);
            home.Choose((VillageChoice)(chapter % 3));
        }
        int unsolved = 0;
        for (int i = 0; i < home.Choices.Length; i++) if (!home.TreeSolved(i)) unsolved++;
        CottageSummary grown = CottageSummary.For(home, 2);
        Expect(grown.Waiting == unsolved && unsolved > 0, "things still holding their first puzzle: " + grown.Waiting);
        Expect(grown.Letters == 2 && grown.Anything, "unread letters are counted as given");
        Expect(grown.DropsNeed == GardenMap.StreamsPerReach, "during the story the drops count streams toward a reach");
        home.SolveTree(0);
        Expect(CottageSummary.For(home, 0).Waiting == unsolved - (home.TreeSolved(0) ? 1 : 0), "a solve leaves one fewer waiting");
        Expect(grown.Season == GardenSeason.For(home), "the season is the garden's");
        // Idea 21 (bonsai): a thing grows a little fuller as its puzzle deepens, never smaller,
        // and never so big it crowds its cell.
        for (int baseTier = 0; baseTier < TreePuzzles.Tiers; baseTier++)
        {
            float last = 0f;
            for (int replays = 0; replays <= 12; replays++)
            {
                float size = ReplayBoards.Maturity(baseTier, replays);
                Expect(size >= 1f && size <= 1.16f, "a modest size: " + size);
                Expect(size >= last, "maturing never shrinks");
                last = size;
            }
            Expect(ReplayBoards.Maturity(baseTier, 0) == 1f, "a first-solve thing is its own size");
        }
        Expect(ReplayBoards.Maturity(0, ReplayBoards.ReplaysPerTier) > 1f, "one tier deeper shows");
        Expect(ReplayBoards.Maturity(TreePuzzles.Tiers - 1, 9) == 1f, "a thing born deep has no further to deepen");
        // Idea 22 (people live in the world): each neighbour stands by the newest thing of
        // their own kind, a nursery plant of theirs before a story thing.
        WorldModel kin = WorldModel.New(PlayMode.Living);
        for (int chapter = 0; chapter < 6; chapter++)
        {
            Puzzle board = kin.CurrentPuzzle;
            for (int i = 0; i < board.CellCount; i++)
                while (kin.Rotations[i] != board.Solution[i]) kin.Rotate(i);
            kin.Choose((VillageChoice)(chapter % 3));
        }
        for (int villager = 0; villager < 3; villager++)
        {
            int at = NeighbourNotice.StandBy(kin, villager, out bool fromNursery);
            Expect(!fromNursery && at >= 0 && kin.Choices[at] == villager, "a story thing of their kind");
            for (int later = at + 1; later < kin.Choices.Length; later++)
                Expect(kin.Choices[later] != villager, "and the newest one");
        }
        WorldSnapshot withPlants = kin.Export();
        int flowerKind = -1, treeKind = -1;
        for (int kind = 0; kind < Nursery.Kinds; kind++)
        {
            if (flowerKind < 0 && Nursery.Family(kind) == VillageChoice.Flowers) flowerKind = kind;
            if (treeKind < 0 && Nursery.Family(kind) == VillageChoice.ShadeTree) treeKind = kind;
        }
        withPlants.bought = new[] { flowerKind, 40, treeKind, 41, flowerKind, 42 };
        withPlants.boughtSolved = new int[0];
        Expect(WorldModel.TryRestore(withPlants, out WorldModel planted), "restore with nursery plants");
        int fen = NeighbourNotice.StandBy(planted, (int)VillageChoice.Flowers, out bool fenNursery);
        Expect(fenNursery && fen == 2, "Fen stands by the newest flower bought (" + fen + ")");
        int mira = NeighbourNotice.StandBy(planted, (int)VillageChoice.ShadeTree, out bool miraNursery);
        Expect(miraNursery && mira == 1, "Mira by the tree bought");
        int orin = NeighbourNotice.StandBy(planted, (int)VillageChoice.Bridge, out bool orinNursery);
        Expect(!orinNursery && planted.Choices[orin] == (int)VillageChoice.Bridge, "Orin, with none bought, by his story thing");
        Expect(NeighbourNotice.StandBy(WorldModel.New(PlayMode.Living), 0, out _) == -1, "nothing yet: no spot");
        // Idea 39 (four round one board): the take-turns token counts alone, two, three, four,
        // then alone again; turns go round in order; one move never skips anyone.
        Expect(TakeTurns.NextPlayers(1) == 2 && TakeTurns.NextPlayers(2) == 3 && TakeTurns.NextPlayers(3) == 4 &&
               TakeTurns.NextPlayers(4) == 1, "the token cycles 1, 2, 3, 4, 1");
        Expect(TakeTurns.NextPlayers(0) == 2 && TakeTurns.NextPlayers(9) == 1, "out-of-range counts recover");
        for (int players = 1; players <= TakeTurns.MaxPlayers; players++)
        {
            int turn = 0;
            var seenTurns = new HashSet<int>();
            for (int move = 0; move < players * 3; move++)
            {
                seenTurns.Add(turn);
                int next = TakeTurns.Next(turn, players);
                Expect(next == (turn + 1) % players, "turns go round in order");
                turn = next;
            }
            Expect(seenTurns.Count == players, "everyone gets a turn with " + players);
        }
        Expect(!TakeTurns.OfferHint(0, 3) && TakeTurns.OfferHint(1, 2) && TakeTurns.OfferHint(3, 4) && !TakeTurns.OfferHint(0, 1),
            "the hint is offered on every turn but the first player's, and never alone");
        // Idea 33 (every exhibit tells you something): a found almanac item carries a line
        // from its keeper, made only of true things: what you made, and a flower's season.
        string[] keepers = { "Mira", "Orin", "Fen" };
        foreach (VillageChoice family in new[] { VillageChoice.ShadeTree, VillageChoice.Bridge, VillageChoice.Flowers })
            for (int i = 0; i < GardenCatalog.CountOf(family); i++)
            {
                GardenItem item = GardenCatalog.Get(family, i);
                string line = Almanac.KeeperLine(item);
                Expect(line.StartsWith(keepers[(int)family] + ": "), "the family's keeper speaks: " + line);
                Expect(line.Contains(item.Memory), "it remembers what you made: " + line);
                Expect(!line.Contains("!") && line.Length <= 110, "calm and short: " + line);
                if (family == VillageChoice.Flowers)
                {
                    string season = "";
                    foreach (Season s2 in new[] { Season.Spring, Season.Summer, Season.Autumn, Season.Winter })
                        if (Array.IndexOf(GardenCatalog.FlowersOf(s2), i) >= 0) season = s2.ToString().ToLowerInvariant();
                    Expect(line.Contains("in " + season), "a flower's season: " + line);
                }
            }
        // Idea 4 (music puzzles, falling blocks): each move plays the next note of a gentle
        // phrase, in a pentatonic scale so any run of moves is in tune; the last solve note
        // comes home to the tonic. Each kind has its own phrase.
        var scale = new HashSet<int>(Melody.Scale);
        var phrases = new HashSet<string>();
        foreach (PuzzleKind kind in Enum.GetValues(typeof(PuzzleKind)))
        {
            string phrase = "";
            for (int move = 0; move < Melody.PhraseLength * 2; move++)
            {
                int note = Melody.NoteAt(kind, move);
                Expect(scale.Contains(note), "every note is in the scale: " + note);
                if (move > 0) Expect(Math.Abs(note - Melody.NoteAt(kind, move - 1)) <= 7, "no leap beyond a fifth");
                if (move < Melody.PhraseLength) phrase += note + ",";
                Expect(note == Melody.NoteAt(kind, move + Melody.PhraseLength), "the phrase repeats");
            }
            phrases.Add(phrase);
            Expect(Melody.Home(kind) == Melody.Scale[0] || Melody.Home(kind) == Melody.Scale[0] + 12, "the solve comes home to the tonic");
            double hz = Melody.Hertz(Melody.NoteAt(kind, 0));
            Expect(hz > 200 && hz < 1200, "a comfortable pitch: " + hz);
        }
        Expect(phrases.Count == Enum.GetValues(typeof(PuzzleKind)).Length, "each kind has its own phrase");
        Expect(Math.Abs(Melody.Hertz(0) - 261.63) < 0.01 && Math.Abs(Melody.Hertz(12) - 523.25) < 0.02, "semitones from middle C");
        return assertions;
    }

    private static string Mask()
    {
        string dir = AppContext.BaseDirectory;
        while (dir != null && !System.IO.File.Exists(System.IO.Path.Combine(dir, "Assets/Resources/Evergrove/garden-map-mask.txt")))
            dir = System.IO.Path.GetDirectoryName(dir.TrimEnd(System.IO.Path.DirectorySeparatorChar));
        if (dir == null) throw new Exception("garden-map-mask.txt not found");
        return System.IO.File.ReadAllText(System.IO.Path.Combine(dir, "Assets/Resources/Evergrove/garden-map-mask.txt")).Trim();
    }

    private static int Index(string name)
    {
        for (int i = 0; i < GardenCatalog.CountOf(VillageChoice.Flowers); i++)
            if (GardenCatalog.Get(VillageChoice.Flowers, i).Name == name) return i;
        throw new Exception("no flower " + name);
    }
}
