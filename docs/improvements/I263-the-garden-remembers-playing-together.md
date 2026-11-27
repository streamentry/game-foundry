# I263 — The garden remembers playing together

## Player (vision, family goal)
- "Grandparents, parents and children should be able to play it together
  gently." Today every puzzle is one person's. When my granddaughter sits on my
  lap, she has no part, and the garden keeps no trace that she was there.

## PM decision
- The save remembers which things were solved together, taking turns on one
  device (I264). It stores only that it was together: never who, never a name
  or an age. The family goal collects no data about children.
- It changes nothing else: seeds, fruit, solves and the river are unchanged.

## Dev
- Rules and tests by a Sonnet subagent, reviewed by me.
  - `WorldModel.MarkTogether(thing)` works only on a solved thing, in Living
    mode, and is idempotent.
  - `SolvedTogether`, `Together`, `TogetherCount`; `BoughtBase = 2000` matches
    the map index.
  - The snapshot field `together` is sorted, unique, in range and validated
    fail-closed.
  - Garden-file key with a `matchesPreTogether` matcher; older checksums can't
    carry it.
- `Tests/TogetherTests.cs` (70 assertions). By the subagent's own report the
  tests were written with the code, not red-first.

## Result (b289)
- Tours complete in portrait, landscape and 150% text (82 shots each).
  - `17i2`: the take-turns token in sage.
  - `17j2`: the forget-me-nots wear the two-colour mark.
  - `17j3`: Fen says "The two of you matched the flower beds together. My
    bees approve."
- Core tests: 344,155 assertions pass.
- Not yet tried with two real people on one device. That's the next family
  playtest.
