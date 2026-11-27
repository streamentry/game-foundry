# I322 — A head start

Idea 6 of the [fifty](../../games/evergrove/docs/specs/proposals/world-games-50-ideas-2026-11-27.md), after the handicap stones of go (China, Japan, Korea): the stronger side gives a head start, and nobody is shamed.

## Player

"Seed sudoku beats me every time. I don't want it easier for everyone, just a nudge for me."

## PM decision

If you leaned on the hint for a kind last time (two or more hint taps), that kind's next board opens with a move or two already made: the same moves the hint would make. It's silent: no message, no "easy mode", no mark. The smallest boards never get one, and there are never more than two moves, so the puzzle is still yours. Use fewer hints and it fades away by itself.

## Dev

- `HeadStart.Moves(hintsLastBoard, tier)` (Core, new). The test ran red first (6 errors). It checks:
  - bounded at two moves;
  - only after two or more hint taps;
  - never at tier 0;
  - never less help after more hints.
- `TreePlay` counts hint taps (`HintsUsed`). `GiveHeadStart(moves)` presses the hint twice per move with the move sound muted.
- `EvergroveApp` keeps each kind's last hint count on the device (`evergrove:hints:<kind>`) and applies the head start when a board opens.

## Result

Help follows the player, quietly. Not known: whether players notice. If they read it as the game playing for them, the playtest should say.
