# I211 — Buildings stand on the grass, not in pale boxes

## Player
- On the map the bandstand, the raft and other buildings sat inside faint cream
  squares, like stickers on the painting. On the board, at home and in the
  nursery tray it looked unfinished.

## PM decision
- Fix it at the source, in the art pipeline, so it stays fixed when the art is
  regenerated.

## Dev
- Scan of all 200 catalogue paintings: the 25 from sheet `catalog-builds-d`
  (build-75 to build-99) had corner opacity 45–92 of 255. That master was
  delivered with a half-transparent cream background, and the pipeline's
  knock-out only handles fully opaque masters, so it passed through.
- `tools/evergrove_art_pipeline.py`: `clear_veil` clears light pixels below
  opacity 120 (the painting itself is opaque; dark antialiased edges are kept),
  applied to every catalogue sheet. Regenerated build-75..99 only; all four
  corners are now fully transparent.

## Result
- `I211-veil-before-after.jpg` (local only): six of the buildings on grass
  green, before (top, pale boxes) and after (bottom).
- `I211-clean-buildings.jpg`: home and the mended mosaic, before and after.
- A faint light fringe remains at some buildings' bases; left as is rather than
  risk eating light paint (white cloth, flowers).
- Portrait, landscape and 150% tours complete (b251).
