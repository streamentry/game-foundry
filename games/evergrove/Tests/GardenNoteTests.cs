using System;
using Evergrove.Core;

/// <summary>
/// I274: the one daily note from the garden (docs/specs/proposals/notifications-2026-11-27.md).
/// Only true things, in a neighbour's voice, never pressure; never at night; backs off by itself.
/// </summary>
internal static class GardenNoteTests
{
    private static int assertions;

    private static void Expect(bool ok, string message)
    {
        assertions++;
        if (!ok) throw new Exception("Garden note test failed: " + message);
    }

    private static readonly string[] Banned = {
        "miss", "lonely", "come back", "streak", "don't forget", "hurry", "only today", "waiting for you",
        "love you", "lose", "days", "last chance", "before it's too late", "!"
    };

    private static void Kind(GardenNote note, string label)
    {
        string text = (note.From + " " + note.Body).ToLowerInvariant();
        foreach (string word in Banned) Expect(!text.Contains(word), label + ": no pressure (" + word + "): " + note.Body);
        Expect(note.Body.Length <= 140, label + ": short enough for a lock screen: " + note.Body.Length);
        Expect(!string.IsNullOrEmpty(note.From), label + ": from someone");
    }

    public static int Run()
    {
        // Nothing waiting: nothing is said.
        WorldModel empty = WorldModel.New(PlayMode.Living);
        Expect(GardenNote.For(empty, false) == null, "a brand-new garden has nothing to say yet");
        Expect(GardenNote.For(WorldModel.New(PlayMode.PuzzleOnly), true) == null, "never in puzzle-only mode");
        Expect(GardenNote.For(null, true) == null, "no garden, no note");

        // Checklist 57: a thing still holding its first puzzle is a reminder, not an arrival.
        WorldModel world = WorldModel.New(PlayMode.Living);
        Grow(world, 5);
        Expect(GardenNote.For(world, false) == null, "a thing with its first puzzle says nothing");

        // An unread letter outranks everything.
        GardenNote letter = GardenNote.For(withLetter(world), true);
        Expect(letter != null && letter.Target == NoteTarget.Letterbox && letter.Body.Contains("Nan"),
            "an unread letter first: " + letter?.Body);
        Kind(letter, "letter");
        Expect(GardenNote.For(withLetter(world), false) == null, "a read letter isn't news, and nothing else has arrived");

        // A cutting waiting in the basket is an arrival.
        WorldSnapshot s = world.Export();
        s.gifts = new[] { 2 };
        Expect(WorldModel.TryRestore(s, out WorldModel gift), "restore with a gift");
        GardenNote cutting = GardenNote.For(gift, false);
        Expect(cutting != null && cutting.Target == NoteTarget.Basket && cutting.Body.Contains(Nursery.Item(2).Name),
            "the cutting in the basket: " + cutting?.Body);
        Kind(cutting, "cutting");

        // Checklist 57: one drop from more river is a prize, not an arrival.
        WorldModel done = WorldModel.New(PlayMode.Living);
        Grow(done, 12);
        for (int i = 0; i < GardenMap.SolvesPerReach - 1; i++) done.SolveTree(i);
        Expect(GardenNote.For(done, false) == null, "the river mist is not an arrival");

        // Checklist 57: everything solved, a favourite to replay, is a reminder, not an arrival.
        WorldModel all = WorldModel.New(PlayMode.Living);
        Grow(all, 12);
        for (int i = 0; i < 12; i++) all.SolveTree(i);
        Expect(GardenNote.For(all, false) == null, "a fresh board of a favourite is not an arrival");

        // Deterministic.
        Expect(GardenNote.For(withLetter(world), true).Body == GardenNote.For(withLetter(world), true).Body, "same garden, same note");

        // When: the player's hour, never at night, today if there's time else tomorrow.
        var morning = new DateTime(2026, 11, 27, 6, 0, 0);
        Expect(GardenNote.NextTime(morning, 9) == new DateTime(2026, 11, 27, 9, 0, 0), "today at nine");
        var late = new DateTime(2026, 11, 27, 19, 50, 0);
        Expect(GardenNote.NextTime(late, 19) == new DateTime(2026, 11, 28, 19, 0, 0), "tomorrow if the hour has passed");
        Expect(GardenNote.NextTime(new DateTime(2026, 11, 27, 18, 45, 0), 19) == new DateTime(2026, 11, 28, 19, 0, 0),
            "not within the next half hour either (you just played)");
        Expect(GardenNote.NextTime(morning, 23).Hour == GardenNote.LatestHour && GardenNote.NextTime(morning, 3).Hour == GardenNote.EarliestHour,
            "quiet hours: clamped into the day");
        for (int h = 0; h < 24; h++)
        {
            DateTime at = GardenNote.NextTime(morning, h);
            Expect(at.Hour >= GardenNote.EarliestHour && at.Hour <= GardenNote.LatestHour && at > morning,
                "every chosen hour lands in daytime, in the future (" + h + ")");
        }

        // Backs off: three unopened in a row and it stops until the game is opened.
        Expect(GardenNote.MaySchedule(0) && GardenNote.MaySchedule(2) && !GardenNote.MaySchedule(3) && !GardenNote.MaySchedule(9),
            "stops after three unopened");
        return assertions;
    }

    private static WorldModel withLetter(WorldModel world)
    {
        WorldSnapshot s = world.Export();
        s.letters = new[] { "The pears are in." };
        s.letterFrom = new[] { "Nan" };
        Expect(WorldModel.TryRestore(s, out WorldModel w), "restore with a letter");
        return w;
    }

    private static void Grow(WorldModel world, int count)
    {
        for (int chapter = 0; chapter < count; chapter++)
        {
            Puzzle board = world.CurrentPuzzle;
            for (int i = 0; i < board.CellCount; i++)
                while (world.Rotations[i] != board.Solution[i]) world.Rotate(i);
            world.Choose((VillageChoice)(chapter % 3));
        }
    }
}
