# I38 — Rooted pipes answer a tap with a gentle nudge

Date: 2026-11-24. Branch: feat/mosselora-painted-ui (PR #76).

**Player problem.** Tapping a rooted (protected) pipe silently did nothing visible except the spoken status line. Players read that as "the tap didn't register".

**Change.** The rooted pipe gives a short, damped side-to-side **nudge** (±4 pt over 0.35 s) that says "this one stays" without scolding: no red, no buzz, no penalty. No move or undo entry is spent, as before. Gentle-motion off skips it.

**Evidence.** b30 QA tour step 13 (tapping the protected tile) passes and asserts the board and undo state are unchanged. **Not verified:** the shake frame by frame.
