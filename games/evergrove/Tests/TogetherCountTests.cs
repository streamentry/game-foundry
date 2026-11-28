using System;
using System.Collections.Generic;
using System.IO;
using Evergrove.Core;

/// <summary>
/// "The two of you" only when it is two. Pins the two-player line (unchanged), the fallback for
/// counts outside 3 to 4, the three- and four-player lines for every puzzle kind, the players
/// slot that carries the count into the companion fact, and the Unity wiring that sets it.
/// </summary>
internal static class TogetherCountTests
{
    private static int assertions;

    private static void Expect(bool ok, string message)
    {
        assertions++;
        if (!ok) throw new Exception("TogetherCountTests failed: " + message);
    }

    public static int Run()
    {
        assertions = 0;

        // Exact old two-player text, pinned for one kind.
        Expect(NeighbourNotice.TogetherLine(PuzzleKind.Water) ==
            "The two of you freed the water together. It ran twice as glad.", "water two-player line is unchanged");

        foreach (PuzzleKind kind in Enum.GetValues(typeof(PuzzleKind)))
        {
            string two = NeighbourNotice.TogetherLine(kind, 2);
            Expect(two == NeighbourNotice.TogetherLine(kind), kind + ": two players equals the one-argument line");

            foreach (int count in new[] { 1, 0, 5, -3 })
                Expect(NeighbourNotice.TogetherLine(kind, count) == two, kind + ": " + count + " players keeps the two-player line");

            string three = NeighbourNotice.TogetherLine(kind, 3);
            string four = NeighbourNotice.TogetherLine(kind, 4);
            CheckGroup(kind, three, "three", two);
            CheckGroup(kind, four, "four", two);
        }

        // Companion facts: the players slot reaches the speech.
        WorldModel world = WorldModel.New(PlayMode.Living);
        DateTime today = new DateTime(2026, 11, 28);
        foreach (int players in new[] { 3, 4 })
        {
            var context = new CompanionContext(today, togetherKeeper: 0, togetherKind: PuzzleKind.Water, togetherPlayers: players);
            Fact fact = Find(CompanionFacts.For(world, context), FactKind.SolvedTogether, 0);
            Expect(fact != null, "a together fact exists for " + players + " players");
            Expect(fact.Slot("players") == players.ToString(), "the fact stores the player count " + players);
            Expect(CompanionSpeech.Legacy(fact, 0) == NeighbourNotice.TogetherLine(PuzzleKind.Water, players),
                "speech for " + players + " players is the group line");
        }

        // Two players (default) keeps the old line, and a fact without a players slot does too.
        Fact plain = Find(CompanionFacts.For(world, new CompanionContext(today, togetherKeeper: 0, togetherKind: PuzzleKind.Water)),
            FactKind.SolvedTogether, 0);
        Expect(plain != null && plain.Slot("players") == "2", "the default context records two players");
        Expect(CompanionSpeech.Legacy(plain, 0) == NeighbourNotice.TogetherLine(PuzzleKind.Water), "two players speaks the old line");

        var slots = new Dictionary<string, string> { { "puzzle", PuzzleKind.Water.ToString() } };
        var old = new Fact(FactKind.SolvedTogether, 0, "together:old", slots, 1, false, true, 1);
        Expect(CompanionSpeech.Legacy(old, 0) == NeighbourNotice.TogetherLine(PuzzleKind.Water),
            "an old fact without a players slot speaks the two-player line");

        var threeSlots = new Dictionary<string, string> { { "puzzle", PuzzleKind.Water.ToString() }, { "players", "3" } };
        var threeFact = new Fact(FactKind.SolvedTogether, 0, "together:three", threeSlots, 1, false, true, 1);
        Expect(CompanionSpeech.Legacy(threeFact, 0) == NeighbourNotice.TogetherLine(PuzzleKind.Water, 3),
            "a players slot of 3 speaks the three-player line");

        // Unity wiring: the count is recorded on solve and handed to the companion context.
        string hub = ReadRepoFile("games/evergrove/Assets/Scripts/Unity/EvergroveApp.Hub.cs");
        Expect(hub != null && hub.Contains("togetherPlayers = openCard.Players"), "Hub records the players of the solved card");
        string neighbours = ReadRepoFile("games/evergrove/Assets/Scripts/Unity/EvergroveApp.Neighbours.cs");
        Expect(neighbours != null && neighbours.Contains("togetherPlayers: togetherPlayers"),
            "Neighbours passes the players into the companion context");

        return assertions;
    }

    private static void CheckGroup(PuzzleKind kind, string line, string word, string two)
    {
        Expect(!string.IsNullOrEmpty(line), kind + ": " + word + " line is not empty");
        Expect(line.Contains(word), kind + ": " + word + " line names the group");
        foreach (string banned in new[] { " two", "Two", "both", "twice", " pair", "each other" })
            Expect(!line.Contains(banned), kind + ": " + word + " line has no \"" + banned + "\": " + line);
        Expect(line.Length < 110, kind + ": " + word + " line is under 110 characters");
        Expect(line != two, kind + ": " + word + " line differs from the two-player line");
    }

    private static Fact Find(IReadOnlyList<Fact> feed, FactKind kind, int keeper)
    {
        foreach (Fact f in feed)
            if (f.Kind == kind && f.Keeper == keeper) return f;
        return null;
    }

    private static string ReadRepoFile(string relative)
    {
        foreach (string start in new[] { Directory.GetCurrentDirectory(), AppContext.BaseDirectory })
        {
            string dir = Path.GetFullPath(start);
            for (int depth = 0; depth < 12 && dir != null; depth++)
            {
                string candidate = Path.Combine(dir, relative);
                if (File.Exists(candidate)) return File.ReadAllText(candidate);
                dir = Path.GetDirectoryName(dir);
            }
        }
        return null;
    }
}
