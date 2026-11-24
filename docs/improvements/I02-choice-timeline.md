# I02 — Painted choice timeline in journal and ending

Date: 2026-11-24. Branch: feat/mosselora-painted-ui (PR #76).

**Player problem.** The journal and the ending showed the village's story as a text list ("Chapter 1 · Orin · Restore a crossing…"). The Haiku art-director review called this the biggest remaining break from an immersive, picture-first experience. The "Tap to turn a pipe…" sentence also repeated under every board after the player had already learned it.

**Change.** `ChoiceTimeline` draws every past choice, in order, as its painted icon (tree, bridge, wildflowers) in a wrapping strip of up to six per row. It appears at the top of the journal and above the ending's final words. The strip has no text, so screen readers hear only the existing journal lines. The how-to sentence now shows only during stream 1; each tile's spoken hint still carries it.

**Vision fit.** §3: "your choice changes the next puzzle, and the place remembers it truthfully". The memory is now visible as a picture, not only read.

**Evidence.** b15 QA tour (17 screens) passes. The journal and ending screens render the strip.

**Not verified.** Comprehension of the strip without the text lines.
