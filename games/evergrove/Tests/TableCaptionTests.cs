using System;
using Evergrove.Core;

/// <summary>
/// Vision run 20261128-1901, idea 3: the take-turns token's long-press caption names exactly what
/// the next tap does, for every table size, and out-of-range counts clamp.
/// </summary>
internal static class TableCaptionTests
{
    private static int assertions;

    private static void Expect(bool ok, string message)
    {
        assertions++;
        if (!ok) throw new Exception("TableCaptionTests failed: " + message);
    }

    public static int Run()
    {
        assertions = 0;

        // The four exact captions.
        Expect(TakeTurns.TableCaption(1) == "Playing alone. Tap to take turns with someone.", "alone");
        Expect(TakeTurns.TableCaption(2) == "Two at the table. Tap for three.", "two");
        Expect(TakeTurns.TableCaption(3) == "Three at the table. Tap for four.", "three");
        Expect(TakeTurns.TableCaption(4) == "Four at the table. Tap to play alone.", "four");

        // Clamping: below one reads as alone, above four as four.
        Expect(TakeTurns.TableCaption(0) == TakeTurns.TableCaption(1), "zero clamps to alone");
        Expect(TakeTurns.TableCaption(-3) == TakeTurns.TableCaption(1), "a negative count clamps to alone");
        Expect(TakeTurns.TableCaption(9) == TakeTurns.TableCaption(4), "nine clamps to four");

        // Each caption's promise matches the next tap: alone -> someone, 2 -> three, 3 -> four, 4 -> alone.
        for (int players = 1; players <= TakeTurns.MaxPlayers; players++)
        {
            int next = TakeTurns.NextPlayers(players);
            string word = next == 1 ? "alone" : next == 2 ? "someone" : next == 3 ? "three" : "four";
            Expect(TakeTurns.TableCaption(players).Contains(word),
                "the caption for " + players + " names the next table (" + word + ")");
        }

        return assertions;
    }
}
