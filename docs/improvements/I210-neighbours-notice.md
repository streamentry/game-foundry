# I210 — Neighbours notice what you do

## Player
- Mira, Orin and Fen only ever spoke about the newest thing of their kind. I'd
  solved puzzles under Mira's trees and bought a poplar from the nursery, and
  nobody seemed to know.

## PM decision
- The companion, offline first (founder decision 2026-11-26): each neighbour
  keeps to their own kind (Mira trees, Orin buildings, Fen flowers) and notices
  true facts from your save:
  - a new plant from the nursery of their kind, by name, until you've solved
    its puzzle ("A poplar from the nursery. Give it a season and it will shade
    the whole bank.");
  - how many puzzles you've solved under their kind of thing ("You have freed
    the water under 2 of your trees. They are drinking deeply."; Orin: old
    paintings made whole; Fen: flower beds matched).
- Rules for every line: only true facts, one line, no guilt, no "I missed you",
  no clock, no randomness. When there's nothing new and true, the older lines
  speak. A new line lights the speech badge by itself (it differs from what
  you last heard).
- A real language model stays later in the plan.

## Dev
- Test-first: `Tests/NoticeTests.cs` (red: type missing), then
  `Core/NeighbourNotice.cs` (`Line`, `SolvedOfKind`). Tests: silence with
  nothing done, counts equal the true count from 1 to 4, the nursery plant
  named, determinism, a banned-phrase list (miss, lonely, come back, streak,
  hurry, only today…), only our three neighbours, none in puzzle-only mode.
- `EvergroveApp.NeighborSays` asks `NeighbourNotice` first (English).
- Note: the code for this iteration landed in commit 9e3ff44 with I208/I209
  (same files); that commit's message wrongly says it would be wired later.
- Tour: after the nursery purchase, Mira is asked what she thinks.

## Result
- Core tests: 70,376 assertions pass.
- `I210-noticing.jpg` (local only): Mira names the poplar just bought from the
  nursery, at 100%, 150% and sideways.
- Portrait, landscape and 150% tours: 38 shots each, complete (b249).
- Not tested with players 40+; whether the lines feel warm or watched is the
  first thing to ask them.
