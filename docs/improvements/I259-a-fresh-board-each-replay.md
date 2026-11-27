# I259 — A fresh board each replay

## Player
- I love my pear tree's puzzle, but replaying it means re-solving a board I
  remember. "The more you play, the more you love it" needs new boards.

## PM decision
- A thing's first play is its own board, seeded by garden and place, so a
  relative helping plays the same one (I227).
- Every replay deals a different board of the same kind.
- No change to seeds or progress. Which replay you're on is counted on the
  device: it only decides which board comes next.

## Dev
- `ReplayBoards.Seed(base, replays)`. Tests (`ReplayBoardTests`) were written
  with the code, not red-first. They check:
  - 0 replays gives the base board;
  - 200 replays give 200 distinct boards;
  - every kind's engine deals a board for the varied seeds and tiers.
- `EvergroveApp`: `Replays` and `CountReplay` keep a count per garden and per
  thing. `OpenTreePlay` and `OpenBoughtPlay` deal `Seed(base, replays + 1)` on
  a replay.
- Bug caught before the tour: the first replay passed 0 and dealt the original
  board again. Replays now count from 1.
- Tour: `17h2-replay-fresh-board`.

## Result (b288)
- Tours complete in portrait, landscape and 150% text (79 shots each).
  - The family card is two columns sideways (`17p`, landscape).
  - `17h2`: a replay deals a fresh board.
  - The depth leaves are on their own pill.
  - The classic three show their picture rules (`17f`, `17i`, `17m`).
- Core tests: 343,997 assertions pass.
- Not yet tried by players aged 40+. How quickly favourites should deepen is
  unmeasured.
