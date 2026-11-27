# I274 — What the garden may say

## PM decision
- One pure function chooses the note from the save, first true one wins, in a
  neighbour's voice:
  - Orin, for an unread letter: "There's a letter in your box from Nan. It'll
    keep until you're ready."
  - The keeper of a cutting's kind: "A clover cutting is in your basket, ready
    for a good spot."
  - Mira, one drop from more river: "One more thing tended and the mist
    downstream will lift."
  - A thing's keeper, for its first puzzle: "Your weathervane still holds a
    puzzle of garden plots, whenever you fancy it."
  - A favourite to replay: "Your forget-me-nots would deal you a fresh board of
    flower pairs, if you fancy one."
- Nothing waiting, nothing said.
- Timing: your hour, clamped to 8:00–20:00, at least half an hour away (you
  just played).

## Dev
- Test-first (`GardenNoteTests`, red on the missing type):
  - `GardenNote.For(world, unreadLetter)`, `NextTime(now, hour)` and
    `MaySchedule(unopened)`;
  - the priority order;
  - a banned-word list (including "!", "days", "lose" and "come back");
  - a lock-screen length limit (140 characters);
  - quiet hours for every chosen hour;
  - deterministic.

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
