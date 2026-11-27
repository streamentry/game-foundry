# I287 — Hours under their suns, at any text size

## Player (150% text pass over everything added I189–I286)
- Almost everything holds at 150%: the family page, the sudoku tray, the bigger
  mahjong bloom, neighbours' lines, the night garden.
- One flaw: on the garden-notes ask, "9:00 15:00 19:00" crowded together and
  drifted off their sun tokens. Each column was only as wide as its token.

## PM decision
- Each hour column is wide enough for "15:00" at the player's text size
  (at least 72 points, growing with the text scale), so every hour sits under
  its own sun.

## Dev
- `EvergroveApp.ShowNotesAsk`: a `LayoutElement` on each hour column, width
  `max(72, 58 × text scale)`.

## Result
- b302 150% tour complete; the hours sit under their suns.
