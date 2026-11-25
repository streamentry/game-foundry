# I165 — The memory card points to the next stream with the same sign

Date: 2026-11-25. Branch: feat/mosselora-i127-plus.

## Player

After I162 every board hangs its name on a painted wooden plank, but the memory card, which says *where you're going next*, still showed "Rainwater" as a plain green caption. Same place, two presentations, one screen apart. Consistency is what makes a world feel built rather than assembled.

## PM decision

The memory card's "next stream" uses **the same painted sign** as the board it leads to. The player sees the "Rainwater" plank on the memory card, taps continue, and finds the same plank over the board.

## Dev

`EvergroveApp`: I162's plank code moved into `PaintedSign(parent)` (a sliced plank and row, 36pt × text scale) and `OneLine(text)` (no wrap, fits the width). `StreamStones` and `NextStreamLine` both use them. Both fall back to the plain caption if the art is missing.

## Result (b176, iPhone 17 simulator, tours)

`I165-same-sign.jpg` (gitignored): the memory card's bottom (`04b`) shows the "Rainwater" plank with the tree under the path, and stream 2's board (`07`) shows the identical plank. At 150% text the plank and name scale together, and the I164 pinned actions stay clear. Portrait, landscape and 150% tours pass (20/5/20). Verify passes.
