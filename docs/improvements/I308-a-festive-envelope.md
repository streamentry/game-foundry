# I308 — A festive envelope

Idea 41 of the [fifty](../../games/evergrove/docs/specs/proposals/world-games-50-ideas-2026-11-27.md), after lì xì and red envelopes in Vietnam, China and Korea: a gift wrapped with a blessing.

## Player

"When I send my granddaughter seeds, I'd like it to feel like a little present, not a transfer."

## PM decision

A gift of seeds or fruit can go in a festive envelope with a wish. One envelope token turns through six wishes, all short, warm and culture-neutral:
- Good health
- A happy year
- Peace at home
- A good harvest
- Sweet days ahead
- With love

The wish is picked, never typed, so it can't carry anything unkind or personal. When the parcel is opened, the envelope's wish is read first. An envelope changes nothing in the economy: the gift is exactly the same.

## Dev

- Core (a Sonnet subagent, test-first, reviewed and merged):
  - `ParcelContents.Blessing` and `FamilyParcel.Blessings`.
  - An optional `blessing` key, allowed only on seeds and fruit, written as format version 2.
  - Every parcel without a blessing stays byte-identical to version 1; a test pins one.
  - Strict pairing: version 2 must have the key, version 1 must not.
  - `FamilyParcelTests` now uses version 3 as its "future" example.
- `FamilySheet`: the wish token under the gifts, in envelope red, only for seeds or fruit. The opening line names the wish.

## Result

A gift can carry a wish. Not known: whether the six wishes feel right across the families who'll use them; they should be reviewed with players from several cultures.
