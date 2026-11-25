# I162 — The stream's name hangs on a painted sign

Date: 2026-11-25. Branch: feat/mosselora-i127-plus.

## Player (Haiku art-director critique of b167)

"**Paint the chapter name onto the riverbank scenery**… The board should be the dominant element."

The stream's name ("Rainwater", "Old Orchard") was a bold green caption under the stone path: a web subheading. A game world labels places with **signs**.

## PM decision

The name is written on a **small painted wooden plank** (Codex art in the plaque style: honey wood, two nails, a sprig of ivy) under the stones, in dark ink, on one line. The choice's landmark icon sits on the plank beside the name. It's a sign by the path, not a heading. Its height matches the old line, so the board doesn't move down.

## Art (Codex image generation)

`art/prompts/sign.txt` → `art/generated/stream-sign.png`, with the plaque art as the style reference. `tools/evergrove_art_pipeline.py` clears faint alpha speckles, trims, fits it to 480px and 9-slices it at half its height, so only the plain middle stretches.

## Dev

- `MosseloraArt.StreamSign`.
- `EvergroveApp.StreamStones`: the name row holds a sliced `Stream sign` image (padding 24/24/7/8, a fixed 36pt height, scaled with text size) containing the landmark icon and the name in `Ink`. The name doesn't wrap and sets its own preferred width, so the plank fits the name. It falls back to the old plain caption if the art is missing.

## Result (b172 → b173, iPhone 17 simulator, scripted tours)

- b172: the plank came out about 75pt tall (the sliced sprite's height drove the layout), pushing stream 2 into a scroll so the wordmark slid away.
- b173 (`I162-stream-sign.jpg`, gitignored), with b171 on the far left for comparison: a slim plank reading "Rainwater" (with the tree), "First Bend" and "Old Orchard" (with the bridge), and the page height unchanged. Portrait and landscape tours pass (20 + 5). Verify passes.

**Not verified:** 150% text (the plank grows to 54pt) and non-English names (English-only builds today).
