# I137 — Neighbours' names and the stream name are readable at arm's length

Date: 2026-11-25. Branch: feat/mosselora-i127-plus.

## Player (Haiku walk-through of b136, as a 63-year-old; "hard to read")

- "the character names under the portraits are small"
- "the 'Rainwater' label is small"

Both were 14pt: the cream name plates under Mira, Orin and Fen on the village painting, and the stream name above the board, on the hub and on the memory card. The board's own icons and the book's lines are 16–17pt. These two labels are the words a player needs to name things ("I talked to Orin", "I'm on Rainwater"), and they were the smallest on the page.

## PM decision

Raise both to 16pt, the same size as the board's other labels. Give the name plate 2pt more padding so the larger name doesn't crowd its edge. Nothing else moves.

## Dev

`EvergroveApp.cs`:
- `PortraitButton` (on the painting): the name plate text goes from 14 to 16pt, and its padding from `8,8,1,2` to `10,10,2,3`.
- `StreamStones`: the "Stream name" goes from 14 to 16pt (bold, moss).

## Result (b142, iPhone 17 simulator, scripted tour)

- `07-chapter2-after-shade-tree`, b141 compared with b142: "Rainwater" and the three name plates are visibly larger and still fit on one line over the painting. The board, tools and village keep their places. Local sheet: `docs/improvements/I137-names-readable.jpg` (gitignored).
- The scripted tour captures all 20 screens. Verify passes.

**Not verified:** 150% text size with longer localised names (English-only builds today), and whether the plates now overlap the portrait art on the narrow iPhone SE.
