# I283 — The note opens where it points

## Player
- My note said there's a letter from Nan. When I open the game I should see
  Nan's letter, not hunt for it. That's rule 8 of the notification design.

## PM decision
- When the game opens after your note was due, the garden shows what it
  pointed at, once:
  - a letter opens (sealed, I234);
  - the basket opens with the cutting picked;
  - the thing it named is picked up, its puzzle one tap away;
  - for the river, home scrolls to the mist and its drops.
- If you open the game before your hour, the note is simply cleared; nothing
  jumps.

## Dev
- `GardenNotes.Schedule` saves the due time, target and thing.
  `GardenNotes.Opened()` returns the note if its hour has passed.
- `EvergroveApp.FollowNote()` runs after home is built, and straight away when
  the game resumes onto home.
- Device-only preferences; nothing goes into the save.

## Result
- b301 tours complete in portrait, landscape and 150% text (89 shots each).
- Core tests: 349,144 assertions pass.
