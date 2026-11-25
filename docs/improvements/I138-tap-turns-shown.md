# I138 — The first board shows that a tap turns a tile

Date: 2026-11-25. Branch: feat/mosselora-i127-plus.

## Player (Haiku walk-through of b136, as a 63-year-old)

On the very first board: "I think I connect the blue water to the sprout by tapping pipes. **I'm unsure what one tap does (rotate? slide?).**"

Mira's tip shows spring → sprout → a tapping finger, and the words say "Tap to connect water". Nothing on the board says that a tap **turns** a tile. Sliding-tile puzzles are at least as familiar to this audience as rotating-pipe puzzles, so the first tap is a guess.

## PM decision

- **Show the verb on the board, not in more words.** Before the first move, one tile that genuinely needs turning (the same tile the hint system would pick, `HintPlanner.SelectTarget`) wears a small cream disc at its top-right corner holding the turning-arrow glyph. With gentle motion on, the arrow turns a quarter at a time with a pause between, the same quarter turn the tile makes when tapped.
- It leaves with the start and goal glows on the first move, so it costs nothing after the lesson. It's only on the first board.
- **Also:** the spent tip's fade is instant in the scripted QA tour, which had caught it half-faded on the iPhone SE (a tour artefact, not seen in play).

## Dev

`EvergroveApp.cs`:
- `BuildPuzzle`: `firstTurn = HintPlanner.SelectTarget(puzzle, rotations)` on round 0 before any move. That tile gets `TurnMark(tile, tileSize)`: a disc of 30–44pt (0.42 × tile) at the top-right corner with a lift and the `ui-turn` glyph in water ink, registered in `goalGlows` so `RefreshBoard` removes it on the first move.
- New `QuarterTurns` behaviour: a 1.1s pause, then a smooth 90° clockwise swing over 0.35s, repeated. It's added only when gentle motion is on.
- `FadeAway` skips its animation in the QA tour.

## Result (b143, iPhone 17 simulator, scripted tour `01-chapter1-start`)

- The first board shows the cream disc with the turning arrow at the top-right of the sprout tile, which is the tile the planner picks as the next useful turn on this board. Mira's tip, the start and goal glows and the board are unchanged. Local crop: `docs/improvements/I138-turn-mark.jpg` (gitignored).
- The scripted tour captures all 20 screens (the mark is gone by `02`, after the first moves). Verify passes.

**Not verified:** the quarter-turn motion itself (the tour skips motion; it's code-reviewed only), and whether a first-time player reads it as "tap turns" rather than "this tile is special". That needs the M3 session's first-input timing.
