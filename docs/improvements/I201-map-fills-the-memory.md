# I201 — The map takes the room left on the memory screen

## Player
- After I200 the memory fitted on one screen, but a strip of blank paper sat
  between the memory card and the buttons, while the map above (where I am
  asked to tap the grass) stayed the same size.

## PM decision
- The map grows into that room on every phone and text size. Measure what is
  on screen after layout; never guess per device.

## Dev
- First try: grow by the page's unused height. Logging showed none: the page
  already measured 876 pt in an 863 pt area (map 420, card 163, a 160 pt space
  reserved for the pinned buttons, which look about 76 pt tall). So the blank
  strip is inside the reserved space, not spare page.
- `GrowToFill(map, card, buttons)`: one frame after layout, compares the card's
  bottom with the top of the pinned buttons in page coordinates and grows the
  map by the gap minus 22 pt of air (at most 320 pt). The dev-only logging was
  removed.

## Result
- Before / after upright, the forget-me-nots, 150% and sideways:
  `I201-map-fills.jpg` (local only). Upright the map gains about 25 pt, more at
  150%; sideways is unchanged (no map in the column).
- Portrait, landscape and 150% tours: all complete (build b238).
- Open: the reserved space under the buttons is larger than the buttons; the
  placing tip is still a sentence.
- Not tested on a real device or with players 40+.
