# I200 — The memory fits on one screen

## Player
- After choosing, the memory card showed the map, the placing tip, then a big
  painting that ran under the pinned buttons. The sentence that matters, "Mira
  remembers the silver birch you planted.", was below the fold, behind the
  buttons, under a generic heading ("A new memory in the grove"). I had to
  scroll a page to read it: the web feeling again.
- Below that came "A young tree grows by the stream" (the map already shows
  it), the walk stones and the next stream's sign (the next board shows both).

## PM decision
- One screen, no scrolling: the painting sits beside the neighbour's sentence,
  narrower at large text so the words keep their room.
- Delete the generic heading, the visible "what changed" line (still read
  aloud by screen readers, hidden), and the stones and sign on this screen.

## Dev
- `Rebuild` memory branch: a "Memory row" (HorizontalLayoutGroup) holds the
  vignette (48% of the page width, 38% above 125% text) and a words column with
  the story, left-aligned. The consequence label is transparent with
  `ignoreLayout`, so the screen reader still reads it. The corner item badge is
  gone from this painting (the change token and the map both show the item).
- The stones and `NextStreamLine` no longer render on the memory screen.
- The first try stacked the painting above the words at 150% text; the
  sentence still slid under the buttons, so the row is used at every size.

## Result
- Before / after (birch, weathervane), 150% and sideways:
  `I200-memory-one-screen.jpg` (local only).
- Portrait, landscape and 150% tours: all complete (build b234).
- Next: the map could take the empty room between the card and the buttons;
  the placing tip is still a sentence.
- Not tested on a real device or with players 40+.
