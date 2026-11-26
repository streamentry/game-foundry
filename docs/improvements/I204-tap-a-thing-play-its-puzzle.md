# I204 — Tap a thing you grew, play its puzzle, earn seeds

## Player
- After the twelve streams there was nothing new to play, and the things I grew
  just stood there. My daughter-tester's idea (see the proposal
  `games/evergrove/docs/specs/proposals/tree-puzzles-2026-11-26.md`): each tree
  holds a puzzle; tap it to play.
- Founder decision (2026-11-26): solving earns **seeds**, an earned currency,
  never sold.

## PM decision
- Tap a thing at home: it's picked up (as before, tap grass to move it) and the
  big round button becomes that thing with a small play mark. Press it to play
  its puzzle over the dimmed garden. No new screen, no words.
- Pays most the first time, a little on every replay (everyday 10/2, a little
  harder 20/4, harder 40/8). No clock, no limit, no streak.
- A solved thing wears a soft golden ring on the ground; the seeds you have show
  top right of home (a sprout and a number).
- Practice only: the story, its boards and choices are untouched.

## Dev
- Core (TDD, part 1 commit 5ca8043): `WorldSnapshot.seeds/treeSolved`,
  `WorldModel.Seeds/TreeSolved/SolveTree/TreeReward`; validated on restore,
  cleared for a reconsidered choice, carried in the garden file with the
  checksum still accepting older files.
- `Unity/TreePlay.cs` (new, self-contained): card with close token, the thing's
  painting, the seed pill, and the board (same paper tiles and `PipeGraphic`
  channels as the streams). Solve → tiles bloom from the spring, seeds shown,
  closes by itself after 2.2 s. Card width fits the screen height sideways.
- `EvergroveApp`: `OpenTreePlay` (board seeded by garden and item, tier by the
  stream it grew in), `SeedPurse`, the held-thing play token, `Bloomed` items.
- Tour: four new shots (picked up, puzzle, solved, home after); tour hooks
  `QaHoldThing/QaOpenTreePlay/QaSolveTreePlay` are dev-build only.

## Result
- Hands-on on the simulator (b240): picked up the bandstand, solved its 4×4 by
  hand, the card closed and the purse went 0 → 20. Found and fixed: "+20"
  wrapped to "+2"; the play mark sat outside the button; sideways the card was
  taller than the screen.
- `I204-tree-puzzle.jpg` (local only): picked up, puzzle, solved, home with
  20 seeds, sideways and 150%.
- Portrait, landscape and 150% tours: 31 shots each, complete (b243). Core
  tests: 55,706 assertions pass.
- Not yet seen on screen: the golden ring (the solved tree was outside the
  tour's view). Seeds have nothing to buy yet (next: the nursery).
- Not tested on a real device or with players 40+.
