# I352 — Where the piece fits

Run 3, batch 3 (P10).

## Player

"Turning a piece to see if it fits still takes guesswork."

## PM decision

When a piece is chosen, the empty squares it could cover as it's turned now glow a soft green. Turn it and the green moves. Laying it is still the player's choice, and the soft green never tells which spot is right, only where it would fit.

## Dev

- `GardenPiecesGame.CanPlace(piece, cell)` (Core) is a question that never moves anything. Tested:
  - asking never moves or places;
  - where it says it fits, `Place` succeeds;
  - the hint's spot always fits.
- `PiecesBoard.FitsChosen` marks every square under any fitting placement, since any square of the piece may be the one tapped (`TryLay`).

## Result

Turning becomes a visible search, not trial and error. The hint (I253) still exists for the right answer.
