# I203 — Every tree holds its own puzzle (the engine)

## Player (and a guest designer)
- The founder's daughter proposed: each tree in the garden has its own puzzle,
  tap a tree to play it, dearer trees are a little harder and prettier. Fine-
  tuned against the vision in
  `games/evergrove/docs/specs/proposals/tree-puzzles-2026-11-26.md`.
- As a player: once the twelve streams are done, nothing new is left to play.
  Puzzles held by the things you grew would give calm play every day without
  streaks or timers.

## PM decision
- Build the engine first, pure and tested; the tap-to-play screen is I204.
- Her "5% / 10% harder" becomes steps you can feel: everyday 3×3; a little
  harder 4×4 with five tiles already right; harder 4×4 with every tile turned.
- Rejected from the idea: weekend-only buying (a wall-clock gate). Held for
  the founder: coins/seeds and the special tree.

## Dev
- `Core/TreePuzzles.For(seed, tier, name)`: a random path through every cell
  (randomized depth-first search), turned into End/Straight/Corner tiles with
  their answers; kept only if the board has exactly one answer
  (`PipeRules.CountSolutions`); then scrambled by tier. Uses its own xorshift
  generator so a tree's board is the same on every device.
- `TierFor(chapter)`: streams 0–3 everyday, 4–7 a little harder, 8–11 harder.
- Tests first (`Tests/TreePuzzleTests.cs`, red, then green): 360 boards over
  3 tiers, each with one answer, valid start, never solved at start, the same
  on every call; at least 6 distinct 3×3 and 40 distinct 4×4 boards; the top
  tier starts more turned than the middle one; unknown tiers refused.

## Result
- `dotnet run` Core tests: 55,670 assertions pass (52,783 before).
- No player-visible change yet.
