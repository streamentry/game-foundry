# I271 — A note from Mira

## Player
- Letters only ever come from family. Between visits, the village could reach
  out too, quietly and only when there's something true to say.

## PM decision
- When tending opens new river after the story (I223), Mira leaves a short
  note in your letterbox naming the season it brought. For example: "The river
  runs further than it ever did. Autumn came down with it; the new bank is gold
  already. There's new bank waiting for roots, whenever you like."
- It flies into the letterbox like a family letter (I237), waits sealed, Orin
  mentions it (I236), and it's kept on the family page.
- Only on a real event. Never on a timer, never "we miss you".

## Dev
- Test-first (`TogetherTests`, red on the missing method):
  `WorldModel.KeepNote(from, text)` keeps a note through the letters store,
  with the same cap and validation, and moves nothing. All four seasons' notes
  are valid letters.
- `NeighbourNotice.RiverNote(season)`.
- `EvergroveApp`: on `riverOpened`, `KeepNote("Mira", ...)` sets
  `letterArrived`.

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
