# I16 — First-run coach bubble replaces stream-1 page text

Date: 2026-11-24. Branch: feat/mosselora-painted-ui (PR #76).

**Player problem (I12 finding 1).** Stream 1 taught with two lines of page text: "Blue waves = spring · green sprout = garden…" above the board and "Tap to turn a pipe…" below it. Page text reads like a manual, not a game.

**Change.** A **coach bubble**: a dark-green rounded speech bubble with a tail pointing down at the board, carrying the "first" line. It appears only on stream 1 before the first move. On the first move it steps aside in place (`RefreshBoard`, no rebuild) and the board slides up into the space. The how-to line below the board is gone. Every tile's spoken hint still carries it for screen readers.

**Evidence.** b22 live iOS Simulator (`I16-I17-hub-coach.png`, middle and right): the bubble points at the board; after one real tap it has gone, the board has moved up, and the village with its neighbors is in view.
