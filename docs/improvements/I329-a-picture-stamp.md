# I329 — A picture stamp

Idea 37 of the [fifty](../../games/evergrove/docs/specs/proposals/world-games-50-ideas-2026-11-27.md), after Australian wordless story games, where pictures say it.

## Player

"I'm not one for typing on a phone. Could I just send a picture with it?"

## PM decision

Any parcel can carry a picture stamp. One token turns through twelve of the game's own paintings: flowers, trees, a pear, an apple, a jar of jam, the season stones, a sealed letter, lanterns. The relative sees the stamp large when the parcel is opened, and it's kept with the letter. It's picked, never typed, so it can't carry anything unkind.

## Dev

- Core (subagent):
  - `FamilyParcel.Stamps` holds 12 art keys; a test checks each exists.
  - The parcel gains an optional `stamp` key; with a blessing or a stamp it's format version 2, otherwise byte-identical to version 1.
  - The save gains `letterStamps`, alongside the letters, and `WorldModel.LetterStamps`.
- Unity: FamilySheet gains the stamp token above the letter and shows the stamp large on opening.

## Result

Letters can be said in pictures. Not yet shown on the letter card when re-reading (the stamp is stored); that's a small follow-up.
