# I260 — Favourites deepen

## Player
- Things I keep coming back to never get harder. A puzzle I love should grow
  with me.

## PM decision
- Every three replays, a thing's puzzle grows one tier deeper (bigger board,
  fewer givens), up to the hardest. The first three replays stay at its own
  depth: a gentle start.
- Seeds stay by the thing's own tier, so depth is the reward. The economy is
  unchanged: a first solve always pays more than any replay (I206).

## Dev
- `ReplayBoards.Tier(baseTier, replays)`; `ReplaysPerTier = 3`. Tested: steady,
  within range, never below the thing's own tier.

## Result (b288)
- Tours complete in portrait, landscape and 150% text (79 shots each).
  - The family card is two columns sideways (`17p`, landscape).
  - `17h2`: a replay deals a fresh board.
  - The depth leaves are on their own pill.
  - The classic three show their picture rules (`17f`, `17i`, `17m`).
- Core tests: 343,997 assertions pass.
- Not yet tried by players aged 40+. How quickly favourites should deepen is
  unmeasured.
