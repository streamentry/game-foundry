# I54 — Pipes gain a painted light edge

Date: 2026-11-24. Branch: feat/mosselora-painted-ui (PR #76).

**Player problem (I51 finding 2).** Pipes were flat vector bars, the least painterly element on a painted board.

**Change.** Each pipe arm gets a soft **light edge** (white at 27% alpha, 16% of the tube width, offset toward the upper-left), so tubes read as rounded glazed clay or copper. The state colours, the solid vs dashed water stripe (non-colour cue) and the contrast of the tube body are unchanged; the highlight sits inside the body.

**Evidence.** b35 close-up (`I54-pipe-shading.png`).
