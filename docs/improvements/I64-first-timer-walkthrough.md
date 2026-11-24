# I64 — Proxy first-timer walkthrough: coach gets its sentence back, goals glow

Date: 2026-11-24. Branch: feat/mosselora-painted-ui (PR #76).

**What.** A Haiku sub-agent role-played a **first-time 46-year-old player** (plays a word game and solitaire, dislikes nagging apps) and walked seven b40 screens in order: launch hub, first puzzle, after a few taps, solved/choice, memory, next puzzle, next-day launch. This is a proxy, not real player evidence.

**What it found.**
- **It would most likely quit at the first board.** The pictogram coach (I57) did not say that a tap *turns* a pipe; "the hand icon is the only clue". This is exactly the risk recorded at I57.
- Next day: "Fen remembers the flowers you grew" felt warm and not nagging, so it would probably return, mildly.
- Its suggestions: one plain caption ("tap a pipe to turn it, link the wave to the sprout"), highlight the wave and sprout tiles, and label the drops.

**Change.**
- Mira's bubble keeps the pictogram (waves → sprout, tapping hand) and adds **one plain sentence** beneath it, the existing localized `first` line. This is instruction text on stream 1 only, not a button label, and it leaves after the first move.
- Before the first move on stream 1, the **spring and garden tiles glow** with a soft teal ring (breathing with gentle motion on, still with it off). The rings clear on the first move together with the bubble.
- The drop row stays unlabeled; its exact count is spoken (I27). Watch this in M3.

**Evidence.** b42 live iOS Simulator, stream 1 (`I64-I65-coach-plaques.png`).
