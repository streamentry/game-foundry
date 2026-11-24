# I43 — Hint sentences become spoken-only; the board shows the clue

Date: 2026-11-24. Branch: feat/mosselora-painted-ui (PR #76).

**Player problem.** After pressing Guide me, a sentence such as "Row 2, column 2: compare the open ends." appeared under the tools. It was a text block that repeated what the board now shows (the honey row tint, then the breathing glow from I20).

**Change.** The progressive clue is **spoken, not shown**: a transparent, out-of-layout text that screen readers still announce. The board itself is the hint.

**Evidence.** b31 QA tour, 19 screens. The hint glow screen in I20 shows the visual cue that remains.
