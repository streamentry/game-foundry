# I398: A gift idea

Run 3, batch 13 ("good neighbours, shared").

## PM decision

Visiting a relative's garden, you arrive to a picture toast: the one thing that would give *their* garden a good neighbour it hasn't found. For example: "A gift idea: a flower would give them a sheltered flower." It shows a flower, a sprig, then the pairing's two paintings. It's a subtle prompt to send something meaningful (a cutting, I334). The garden itself suggests the gift, rather than a menu.

## Dev

- `Pairings.GiftIdea(map, things, foundMask, openMask, out family, out pairing)` is pure. It tries flower, tree and built thing in turn, at every free land cell of their map, using `NewAt` (I372), and respects which pairings are open for them (I380).
- `EvergroveApp.VisitGiftIdea` runs at the end of `OpenVisit`.
- `RuleToast` now clamps vertically. Anchored to a whole screen, "above it" would have been off the top, which also affected the older keepsake toast.
- Tests: a lone tree suggests a flower (sheltered); once sheltered is found, a different idea; nothing when all are found.
