# I50 — Memory screen opens on a painted moment of the neighbor

Date: 2026-11-24. Branch: feat/mosselora-painted-ui (PR #76).

**Player problem (I25 finding 2).** The memory screen, the emotional peak of every chapter, was a text card: "Chapter 1 · Mira · Plant a shade tree / Mira remembers… / A young tree grows…", a web dialog at the most important moment.

**Change.** Codex painted three **memory vignettes**, each attached to the hero painting so it is the same village and the same people (`art/generated/vignette-tree|bridge|flowers.png`, prompts in `art/prompts/`):
- **Mira** kneeling to firm the soil around a sapling she has just planted, a watering can beside her.
- **Orin** fitting the last plank onto a small footbridge, mallet in hand.
- **Fen** scattering wildflower seed from a pouch among coneflowers and lavender.

The memory card now leads with the vignette for the choice just made, then "A new memory in the grove", then **one** remembered sentence in bold ("Mira remembers the tree you planted.") and its consequence in smaller text. The chapter header moved to the scrapbook only.

**Evidence.** b34 QA tour, consequence screens 04–06 (`I50-memory-vignettes.png`).
