# I40 — Neighbors speak in a bubble beside their portrait

Date: 2026-11-24. Branch: feat/mosselora-painted-ui (PR #76).

**Player problem.** Talking to a neighbor opened a generic text card. Neighbors are people; they should speak.

**Change.** The neighbor's painted portrait (64 pt) sits beside a **speech bubble** with their name, their line (from the real save state) and a small confirm icon to end the conversation. Their medallion on the painting gets a leaf-green ring while they speak. Tapping a neighbor on the hub steps straight into their speech.

**Evidence.** b30 live iOS Simulator: tapping Fen on the hub opens her speech below the creek, "I'm Fen. I miss seeing wildflowers along the water." (`I40-neighbor-speaks.png`).

**Known nit.** The confirm button sits on its own row, leaving air inside the bubble. It could share the line; that is a later polish iteration.
