# I269 — The family page

## Player
- The book remembers the story's choices, but not the family: Nan's letters,
  the help, the puzzles we played together. Those are the moments I'd most
  want to look back on.

## PM decision
- A third tab in the book (two people): Family in the garden. All of it comes
  from the save, with no dates and no counts to beat:
  - Letters: the six newest, with who wrote them and their first words.
  - Helped: who helped with which of your things.
  - Played together: each thing, "taking turns".
- With nothing yet, one line says what will be kept here.

## Dev
- `EvergroveApp`: `familyTab`, `FamilyPage()` and `FamilyHeading()`; tabs in
  `JournalTabs()`. The tab resets on open and close.
- Tour: `17u3-family-page`.

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
