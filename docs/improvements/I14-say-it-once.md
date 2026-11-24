# I14 — Memory card leads with the neighbor; no repeated lines

Date: 2026-11-24. Branch: feat/mosselora-painted-ui (PR #76).

**Player problem (I12 findings 3–4).** Two screens said things twice. The solved board repeated "The water flows through every tile." under the painting. The memory screen printed "Mira remembers the tree you planted" and "A young tree grows by the stream" above the memory card, which then repeated both.

**Change.** Each line is said once. The "World consequence" and "Remembered choice" lines are skipped on the solved board and on the memory screen. The memory card now opens with the painted portrait of the neighbor who will remember this choice (choice index = villager: tree → Mira, crossing → Orin, wildflowers → Fen), so the memory belongs to a person.

**Evidence.** b21 QA tour, 17 screens (`I13-I15-title-memory-menu.png`): memory card with Mira's portrait and no duplicate lines; choice screen with the solved line shown once.
