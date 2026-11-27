# I347 — The neighbour comes forward

Run 3, batch 2 (P6).

## Player

"A big story painting comes first; Mira and what she says sit at the bottom. I came to talk to her."

## PM decision

When you talk to a neighbour:
- the page behind steps back (dimmed to 36%);
- their face is larger (92, from 64);
- their words are bigger (20 pt);
- the conversation card is the clear focus.

It still closes with a tap anywhere, as before.

## Dev

- `NeighborConversation`: the veil, portrait size and label sizes change. The layout is unchanged, so landscape and RTL behave as before.

## Result

Talking feels like talking to someone. The larger card must still fit at 150% text in landscape; the tour checks this.
