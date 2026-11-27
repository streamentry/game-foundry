# I218 — Neighbours keep clear of the buttons

## Player
- On a busy garden Fen stood right behind the round play button, half hidden,
  and Mira stood under the seed purse at the top. You can't tap a neighbour
  you can't see, and it looked careless.

## PM decision
- On home, the bottom band (the play button and the basket, 190 pt) and the top
  band (the menu, the purse and the letterbox, 140 pt) are kept clear of
  neighbours. Someone standing there steps up, or down, just out of it, and
  never onto another neighbour.
- Things you've grown don't move: only the people standing among them.

## Dev
- `GardenMapView.ClearBottom/ClearTop` (setting one re-lays the map, since the
  map lays itself out before the caller can set them), `KeepClear`: after the
  view settles, each standing neighbour is measured in window space and moved
  out of the band, then stepped on the same way until clear of the others (the
  first try moved Orin down onto Fen).
- `EvergroveApp.GardenMapOnHub`: 190 / 140 upright; sideways the play column is
  beside the map, so no bands.

## Result
- `I218-clear-of-the-buttons.jpg` (local only): b260 (Fen behind the play
  button; Mira under the purse) and b264 (both clear), and 150%.
- Portrait, landscape and 150% tours: 44 shots each, complete (b266). The b264 tours showed Orin stepping onto Fen; the clash check now measures the whole figure (portrait and name).
