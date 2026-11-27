# I243 — Five new puzzles, and planting by numbers

## Player (founder request)
- "Add top 5 puzzles suitable for Mosselora." Today every thing holds one of
  three puzzles: water, Pairs or Mosaic.

## PM decision
- Five new kinds, chosen from the studio's own research
  (`docs/research/45plus-market-screen-2026-11-21.md`,
  `global-45plus-top-three-games-2026-11-21.md`).
  - All are language-free, so they work in all 30 locales and suit icons over
    text.
  - Each is dressed in the garden.

  | Kind | In Mosselora | Evidence |
  | --- | --- | --- |
  | Nonogram | Planting by numbers | Nonogram.com 50M+ installs; `nonogram puzzle` 1K–10K searches, low competition |
  | Mahjong solitaire | Garden tiles | Solitaire and mahjong lead with players 50+ |
  | Sudoku | Seed sudoku, with plants for digits | "sudoku for seniors" is a real search |
  | Shikaku | Garden plots | The studio's search-led fallback; +900% year on year |
  | One Path | Lead the stream | Playtest shortlist number 3 |

- Word search was left out: it's text-heavy and hard to do well in 30 languages.
- Where they live (`ThingPuzzle`, tested):
  - the story's first four things teach the classic three by family;
  - things 5 to 12 cycle through the five new kinds, so every garden shows all
    five;
  - the nursery's restocked editions 1 to 4 carry them too (I240), each new
    kind in at least two families.

  Growing and buying both open new kinds of play, at the tier of the thing.
- Planting by numbers:
  - numbers beside each row and above each column say how many plants grow
    there in unbroken runs;
  - tap to plant a sprout, again to mark "nothing grows here", again to clear;
  - a row's numbers fade once the row is right.

## Dev
- Five rule engines were built in parallel by Sonnet subagents, each in an
  isolated worktree, test-first; I reviewed and integrated them.
  - `NonogramGame`: line-solver-unique, so it's solvable by logic with no
    guessing; it has hints.
  - `MahjongGame`: deals built from a valid removal order, so they're solvable;
    gentle reshuffle when stuck.
  - `SeedSudokuGame`: unique, and solvable with singles only.
  - `PlotsGame`: unique by exact cover. I added an independent cover count on
    180 fields.
  - `StreamPathGame`: unique Hamiltonian path, with an independent brute-force
    check.
- `ThingPuzzle` and `PuzzleKind` (`ThingPuzzleTests`).
- `TreePlay.IPuzzleBoard` lets one card host any board. `BoardKit` gives shared
  beds, cells, clue numbers and a gentle shiver for "not like that" (no sound,
  no penalty). `NonogramBoard`.
- Tour: the `18-<kind>` and `18-<kind>-solved` shots for all five.

## Result
- Core tests: 343,066 assertions pass (the five engines and `ThingPuzzle`).
- b280 tours complete in portrait, landscape and 150% text (68 shots each).
  Every new kind is shot unsolved and solved (`18-<kind>`, `18-<kind>-solved`).
- Found in b279 and tuned in b280:
  - the sudoku tray was too small;
  - neighbouring plots shared colours;
  - a cleared mahjong table was an empty bed.
- Not yet played by adults 40+. Difficulty per tier is unmeasured.
