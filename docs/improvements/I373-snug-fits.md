# I373: Snug fits in Garden Pieces

Run 3, batch 8.

## Player note (P14)

In Garden Pieces, picking up a piece early on turned almost the whole board pale green (I352). A hint that lights everything says nothing.

## PM decision

Glow only the *snug* fits for the piece as it's turned now, the way a careful player fills a board:

1. **Press against the most edges.** Count the edges of the piece's squares that touch the silhouette's border or a piece already down. The fits with the highest count glow.
2. **Never leave a dead pocket.** A fit that would leave an empty pocket smaller than every piece still in the tray is never snug, because nothing could fill that pocket.

These are two plain rules a player can learn and then use without the glow. The glow points at good places, not at the answer.

## Dev

- `Core/GardenPiecesFit.cs`:
  - `Snug(game, piece)`;
  - `Pressed(game, covered)`, the edge count;
  - `LeavesDeadPocket(game, covered, smallest)`, a flood fill;
  - `Cover`.
  It is pure and uses only the game's public API.
- `PiecesBoard.FitsChosen` uses it. The tint is a little deeper, since fewer squares glow.
- `GardenPiecesFitTests`, over 180 boards in three tiers:
  - snug squares are always fit squares;
  - laying the generator's answer step by step never leaves a dead pocket;
  - a piece turned as the answer has it always has a snug fit;
  - snug squares are fewer than half the plain ones. Measured: **4,634 of 16,209 (29%)**.

## Result

The hint narrows from "almost everywhere" to a few places. It also teaches a real skill: fill from the corners and leave no pockets.
