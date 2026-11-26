# I188 — Neighbours stand on the valley side, always in view

Date: 2026-11-26. Branch: feat/mosselora-i127-plus.

## Player (I187's build, b207)

With I187 the neighbours gathered round the newest thing on every board, but on the birch board the birch sits near the map's left edge, so the neighbour placed to its left (Orin) was cut off by the window. The spots offered were also so close together that most counted as taken by Mira.

## PM decision

Neighbours try the side **facing the middle of the valley** first, and the candidate spots are spaced wider (2.5 and 3.8 cells out, 1.6 rows up and down), so the search finds a free spot inside the view before ever trying the outer side. A neighbour standing beside their own thing also picks the inward side.

## Dev

`EvergroveApp.NeighborsOnMap`: `inward = ±1` from the newest item's column. Candidates are inward ×1.15, inward ×2.5, inward ×1.2 one-and-a-half rows down, inward ×1.2 one-and-a-half rows up, inward ×3.8, and finally outward ×1.15. An owner's own-item offset uses the same inward rule.

## Result (b207 → b209; portrait, landscape and 150% tours, 23 each)

- `I188-b207.jpg`: Orin cut off at the left of the birch.
- `I188-after.jpg` (b209): birch board with Mira and Orin to its right and Fen below; weathervane board with Orin and Mira to its right and Fen below; tulips board with Fen, Mira and Orin around the tulips; and the 150% birch board, all fully in view.

Both images are gitignored. Verify and the .NET tests pass. This also completes I187: its code-only commit is now built and checked.
