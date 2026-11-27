# I197 — Sideways, home is your garden too

## Player
- Turning the phone on its side brought back the old home: the garden on the
  left half, a paper column on the right with the logo, the tagline, a line of
  small text and a long arrow button. It felt like a different, older app.
- There was no basket sideways, so I could not plant anything.
- First try at a full-screen sideways home (hands-on, fresh save): the map was
  zoomed in so far that Mira's portrait sat under the stream sign and the play
  button, and then under the walk stones.

## PM decision
- One home in both orientations: the garden edge to edge, one menu button, the
  basket bottom-left, the play token.
- Sideways the screen is short and the neighbours stand mid-garden, so the sign
  and play token keep to a right-hand column, and the walk stones are left to the
  board screen (they show there above the board).
- The map is a little less zoomed sideways (painting as wide as the screen) and
  frames what you made nearer the top.

## Dev
- `BuildTitleScreen`: `worldHome = mapShown` (the `!landscape` guard is gone).
  Sideways: stack anchored to the right 30% of the screen, no stream stones.
  The old two-column sideways layout now only runs when there is no map.
- `GardenMapOnHub`: width scale 1.0 sideways (1.3 upright), meadow apron in both
  orientations, focus 0.24 from the top sideways (0.36 upright).

## Result
- Before / after sideways (home, menu fan, basket tray) and upright:
  `I197-sideways-home.jpg` (local only).
- Hands-on on the simulator, sideways, fresh save: home shows the garden, the
  neighbours stand clear, and tapping play opens the board.
- Portrait, landscape and 150% tours: all complete (build b229).
- Not hands-on yet: planting from the basket sideways with a real tap (a fresh
  save has nothing in the basket; the tour plants through the QA hook).
- Still open: the board itself sideways is the garden on the left and a paper
  column on the right.
- Not tested on a real device or with players 40+.
