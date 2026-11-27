# I309 — Send the very board

Ideas 42 and 12 of the [fifty](../../games/evergrove/docs/specs/proposals/world-games-50-ideas-2026-11-27.md), after the daily shared puzzle that everyone plays and compares (Wales, USA), with sharing that never spoils.

## Player

"That garden-tiles puzzle was lovely. I want my sister to try the same one."

## PM decision

Once one of your own boards is solved, the hint token becomes a sealed letter: "Send this board to family". A tap keeps the card open and shares a parcel holding the board's kind, depth and seed: not the answer, so nothing is spoiled. The relative opens it from Family, and a "Play the board they sent" token opens the very same puzzle. It pays nothing and changes nothing in their garden; it's simply the same puzzle, shared. Sending is free.

Idea 12 (sharing a solved grid) is folded in here. For this audience, sending the board itself to family says more than a grid of squares on social media.

## Dev

- Core (subagent):
  - `ParcelGift.Board` stores kind and tier in `kind`, and the seed in `amount`.
  - `WorldModel.SendBoard`.
  - `ParcelOpenResult.BoardKind`, `BoardTier` and `BoardSeed`.
  - Tests cover forged kinds and tiers and negative seeds.
- `TreePlay.OfferToSend` swaps the hint for the letter after a solve, only when the app sets `SendBoard`: your own boards, never a helped or received one.
- `EvergroveApp.SendBoardToFamily` opens Family first if you haven't written your name yet. `PlayTheirBoard` opens the board with a reward of 0, and only from home.

## Result

Families can play the same puzzle apart. Unverified: the full round trip between two phones (tests cover the parcel; the tour only shows the send token).
