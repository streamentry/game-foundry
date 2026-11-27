# I270 — Their family moments, when visiting

## Player
- Visiting my daughter's garden, I'd like to see the flower bed she did with
  my grandson.

## PM decision
- A visited garden shows its things solved together with the same two-colour
  mark (I265): story things and nursery plants.

## Dev
- `EvergroveApp.VisitItems` sets `Together` from `visited.SolvedTogether(...)`.

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
