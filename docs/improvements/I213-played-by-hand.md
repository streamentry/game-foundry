# I213 — Played by hand: winter, Pairs and the nursery

## Player (hands-on, iOS Simulator, b251)
- Set up a test garden as a player would: generated a garden file (9 streams
  grown, 300 seeds) and restored it through Menu → Settings → Restore from
  garden file. iOS asked to allow the paste (expected for a clipboard read).
- **Restore warning.** The confirm button reads "Confirm: erase this garden".
  It is honest (it does replace the garden) but alarming when you are
  restoring a garden you want. Noted for a later pass.
- **Winter looked switched off.** Home in winter (I198's grade) was so pale and
  grey that the whole screen read as disabled.
- **Pairs works and feels right.** Picked up the cowslips (8 pairs): a miss
  showed the forget-me-nots and the apple tree side by side, and both stayed up
  until the next tap; I remembered the apple and matched it two turns later.
- **The nursery works.** Picked the garden gate (50), tapped open grass: the
  gate stood exactly there, seeds 300 → 250, and the second row (100) opened.
  But the nursery token in the tray was just a sprout, like everything else,
  so nothing said "spend seeds here", and the third row showed faded art with
  no price, so I couldn't see what I was saving for.

## PM decision
- Fix now: a gentler winter; the nursery token shows your seed count on the
  same chip as the prices; closed rows show their price, greyed.
- Later: friendlier restore wording.

## Dev
- `tools/evergrove_art_pipeline.py`: winter grade frost 0.45 → 0.28,
  desaturation 0.6 → 0.5, plus contrast ×1.18; `garden-map-winter`
  regenerated. Previewed three grades side by side before choosing.
- `EvergroveApp.NurseryTokens/PriceChip`: every price is shown; closed rows get
  a grey chip and fainter digits. The nursery token carries a seed-count chip.

## Result
- `I213-hands-on.jpg` (local only): winter before/after, the Pairs match, the
  gate bought by hand, the third row with greyed prices (b253).
- Portrait, landscape and 150% tours: 38 shots each, complete (b253).
- Hands-on, not with players 40+.
