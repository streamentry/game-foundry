# I232 — A basket you can see

## Player
- Looking at Fen's card in b272, the basket token beside the check was a smudge,
  about 20 points of painted fruit on a plaque. At arm's length I couldn't tell
  it was fruit at all.

## PM decision
- The painted basket is shown at 38 points on a 64 by 48 token, about twice a
  glyph, because painted art needs more room than a line icon to read.
- It stays beside the check, in the same place.

## Dev
- `EvergroveApp.NeighborConversation`: the give token is 64 by 48 and its icon
  `LayoutElement` is 38 by 38.

## Result
- b273: the basket reads as fruit in portrait, landscape and 150% text (54 shots each, complete).
- b274: at 150% text the last line touched the tokens, so the bubble gets 8 points more room under the words when the basket shows. The 150% tour is complete and the gap is clear.
