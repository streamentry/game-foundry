# I276 — The soft ask

## PM decision
- Never on first launch. Once there's a reason to want notes (the story's first
  season is done, or you've read a letter), a paper card on home asks in the
  garden's own words: "Would you like a short note from the garden once a day,
  at a time you choose? Only when something's waiting."
- Answer by tapping an hour: 9:00, 15:00 or 19:00 (a sun token each). Or
  "not now".
- Only after a yes does iOS show its own prompt.
- "Not now" means asked once more, after ten more puzzles. A second "not now"
  means never asked again; Settings still has the switch.

## Dev
- `EvergroveApp.MaybeAskNotes` runs once a session, not over another card and
  not in the tour. `ShowNotesAsk` and `CloseNotesAsk`.
- Tour: `17d2-garden-notes-ask` (`QaNotesAsk`).

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
