# I328 — A jar of jam

Idea 45 of the [fifty](../../games/evergrove/docs/specs/proposals/world-games-50-ideas-2026-11-27.md), after Japanese cooking games, where ingredients become a gift.

## Player

"My fruit just sits in the basket. I'd like to make something of it."

## PM decision

At the cottage, three fruit make a jar of jam: a richer gift, made, not bought. The cottage card counts the jars in the pantry (up to 10). In Family, "a jar of jam" can be sent like fruit, in a festive envelope with a wish if you like (I308). When opened, it goes to the relative's pantry. Nothing is created: three fruit become one jar, and the jar moves between gardens.

## Dev

- Core (a Sonnet subagent, test-first, merged after review):
  - `WorldModel.MakeJam` (refused under 3 fruit, or with a full pantry) and `Jam`.
  - `ParcelGift.Jam`, sent 1 to 3 jars, and `ParcelOpenResult.JamGained`; a blessing is allowed on jam.
  - The save gains `jam`, written only when non-zero, so older saves are unchanged.
- Unity: the cottage card gains a pantry line and a make-jam token (the painted `food-jam`). FamilySheet gains a jam gift, with its sent and opened wording and the "no jam yet" refusal. Tour step 17j7 makes a jar.

## Result

Fruit has a second life. Not known: whether 3 fruit per jar feels fair. Tune it in the economy playtest.
