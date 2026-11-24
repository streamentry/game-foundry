# I33 — Tiles swell softly as water arrives

Date: 2026-11-24. Branch: feat/mosselora-painted-ui (PR #76).

**Player problem.** A turn that brought water further lit tiles teal, but in the same frame as everything else. The moment water *arrives* is the core pleasure of the puzzle, and it deserves a beat.

**Change.** After each move, every tile that just became spring-connected gives one soft swell (a 7% `BloomPulse` over 0.36 s). It is applied to the pipe artwork, not the tile, so it never fights the press squash. Tiles that lost water do nothing. Gentle-motion off disables it. The comparison uses the previous move's `Flowing` array, so the cause and effect is exact.

**Evidence.** Code path exercised by the b27/b28 QA tours (17–18 screens, no exceptions). **Not verified:** frame by frame. A recording is queued with the next motion review.
