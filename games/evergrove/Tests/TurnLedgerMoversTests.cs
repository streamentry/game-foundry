using System;
using Evergrove.Core;

/// <summary>
/// Vision run 20261128-1901, idea 2: the ledger counts distinct seats that actually moved up to a step.
/// Key 0 is the opening turn, a move at key k belongs to whoever held the turn at k - 1, undone moves
/// don't count, and a cleared ledger counts nobody.
/// </summary>
internal static class TurnLedgerMoversTests
{
    private static int assertions;

    private static void Expect(bool ok, string message)
    {
        assertions++;
        if (!ok) throw new Exception("TurnLedgerMoversTests failed: " + message);
    }

    public static int Run()
    {
        assertions = 0;

        // Nothing recorded, or only the opening turn: nobody has moved.
        var empty = new TurnLedger();
        Expect(empty.Movers(0) == 0 && empty.Movers(5) == 0, "an empty ledger has no movers");
        var opening = new TurnLedger();
        opening.Record(0, 0);
        Expect(opening.Movers(0) == 0 && opening.Movers(3) == 0, "the opening turn is not a move");
        opening.Record(0, 1);
        Expect(opening.Movers(0) == 0, "a different opening turn is still not a move");
        Expect(new TurnLedger().Movers(-1) == 0, "a negative step counts nobody");

        // Seat 0 alone moving with the turn passing: after one move the turn is seat 1's.
        var soloFirst = new TurnLedger();
        soloFirst.Record(0, 0);
        soloFirst.Record(1, 1);
        soloFirst.Record(2, 0);
        Expect(soloFirst.Movers(1) == 1, "only seat 0 has moved at step 1");
        Expect(soloFirst.Movers(2) == 2, "seat 1 has moved by step 2");

        // Two-player sequence where only seat 0's moves are before the step.
        var twoSeatsOnlyFirst = new TurnLedger();
        twoSeatsOnlyFirst.Record(0, 0);
        twoSeatsOnlyFirst.Record(1, 1);
        twoSeatsOnlyFirst.Record(2, 0);
        Expect(twoSeatsOnlyFirst.Movers(1) == 1, "a two-player sequence with only seat 0 before the step counts one");

        // Three seats each moving once.
        var three = new TurnLedger();
        three.Record(0, 0);
        three.Record(1, 1);
        three.Record(2, 2);
        three.Record(3, 0);
        Expect(three.Movers(3) == 3, "three seats moved counts three");
        Expect(three.Movers(4) == 3, "a later step with no further move still counts three");

        // Gap: steps 1 and 2 were never noted, so key 3 is credited to At(2), which is seat 0.
        var gap = new TurnLedger();
        gap.Record(0, 0);
        gap.Record(3, 1);
        Expect(gap.Movers(3) == 1, "a move after an un-noted gap is credited to the turn at the step before it");
        Expect(gap.Movers(2) == 0, "steps with no note count no move");

        // Undo, then a new move: abandoned moves above the step are forgotten.
        var undone = new TurnLedger();
        undone.Record(0, 0);
        undone.Record(1, 1);
        undone.Record(2, 0);
        undone.Record(1, 1); // undo back to step 1: key 2 is forgotten
        Expect(undone.Movers(5) == 1, "an undone move no longer counts");
        undone.Record(2, 0); // a new move by seat 1 at step 2
        Expect(undone.Movers(2) == 2, "the new move by seat 1 counts after the undo");

        // Moved names the seats that moved, by the same rule.
        var seats = new TurnLedger();
        seats.Record(0, 1); // seat 1 opens a three-seat board
        seats.Record(1, 2); // seat 1 moves
        seats.Record(2, 0); // seat 2 moves
        Expect(seats.Moved(1, 2) && seats.Moved(2, 2), "seats 1 and 2 moved");
        Expect(!seats.Moved(0, 2), "seat 0 never moved, so it does not light");
        Expect(!seats.Moved(2, 1), "seat 2's move after step 1 does not count at step 1");

        // Clear forgets everything.
        var cleared = new TurnLedger();
        cleared.Record(0, 0);
        cleared.Record(1, 1);
        cleared.Record(2, 0);
        cleared.Clear();
        Expect(cleared.Movers(3) == 0, "a cleared ledger counts nobody");

        return assertions;
    }
}
