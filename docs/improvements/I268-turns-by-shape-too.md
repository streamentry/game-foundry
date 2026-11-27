# I268 — Turns by shape, too

## Reviewer (accessibility)
- The take-turns token (I264) told sage's turn from honey's by colour alone.
  The two are close in brightness, and red-green colour blindness can merge
  them. The repository rules say colour must never be the only cue.

## PM decision
- On honey's turn, the two people on the token swap sides (the glyph mirrors).
  Colour and shape both say whose turn it is.

## Dev
- `TreePlay.PaintTurn`: the token's icon has `localScale.x = -1` on honey's
  turn.

## Result (b294)
- Tours complete in portrait, landscape and 150% text (84 shots each).
  - The river opens (the log shows "solves before 3 (wants 3)").
  - Mira's note waits sealed on the letterbox and is listed on the family
    page (`17u3`).
  - Settings shows the garden-notes switch (`17d`).
  - The soft ask (`17d2`) has legible hours. In b293 the hour labels were
    white on paper; fixed.
  - The tour logs the note that would be scheduled, for example "Orin: Your
    wisteria would deal you a fresh board of garden plots, if you fancy one."
- Found and fixed on the way:
  - the tour had stopped reaching the river step (b290 and b291);
  - the b292 Xcode link failure (see I275).
- Core tests: 347,183 assertions pass.
- Not verified: delivery of a real notification on a device, and the iOS
  permission prompt. Not yet tried by players aged 40+.
