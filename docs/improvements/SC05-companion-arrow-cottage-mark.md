# SC05: One ranker for the arrow, the cottage card and one soft mark

Smart companion, iteration 5 of 7 ([spec](../../games/evergrove/docs/specs/proposals/smart-companion-2026-11-27.md) §5).

## Player note

- After the story, the home arrow plays the thing that matters most. Family help on a thing you haven't solved comes first, then something new from the nursery, then (as before) the newest thing still holding its first puzzle.
- The cottage card lists what's waiting for you (a letter, a cutting, family help) above what the season can still give, then your nearest hope.
- When a cutting is the thing waiting most, the basket wears a small honey ribbon. There is only ever one such mark, it's never a count, and it's gone once the cutting is planted. The letterbox's sealed envelope (I236) and the arrow's own picture already mark the other two kinds of waiting.

## PM decision

The spec gives the same facts and weights four uses. The weekly letter is "later" in the spec, and stays a follow-up. Three choices:

- **No memory here.** The neighbours move on once they've said something, but the arrow, card and mark show what is still true and waiting. They don't change because a remark was heard, so the screen stays steady through a visit.
- **One mark.** The soft mark goes only where the top waiting fact lives. Equal weights (a letter and a cutting) take turns by the day's hash. The ribbon reuses the basket gifts' honey ribbon, following v1's badge rule (B5: no red on a badge).
- **Unchanged parts.** The fixed lines of the cottage card (name, season, weather) keep their places. The river drops and days lines move to after the waiting block.

## Dev

- `Core/CompanionAttention.cs` (pure):
  - `ArrowTarget(world, facts, day)`;
  - `Mark(facts, day, garden)`, returning `Attention.None/Letterbox/Basket/Play`;
  - `CottageOrder(items, facts, day, garden)`.
  - Ties are broken by `CompanionRanker.Hash`.
- Facts gain target slots: `item` on `HelpedBy`, `bought` on `NewArrival`.
- `EvergroveApp`:
  - the I351 arrow uses `ArrowTarget`;
  - `OpenCottage` builds its waiting lines in `CottageOrder`;
  - `BasketControls` adds the ribbon when `Mark` says `Basket`.
- Tour: `22-attention-hub.png` and `22-attention-cottage.png`, on a finished garden with family help on thing 2 and a flower cutting in the basket.

## Result

- `Tests/CompanionAttentionTests.cs`:
  - nothing changes during the story;
  - with no waiting facts, the arrow behaves as before;
  - help outranks a new arrival;
  - everything tended means no target;
  - no mark when nothing waits;
  - exactly one mark, and equal weights take turns across days;
  - the cottage order reorders without adding or dropping lines, puts what waits for you before the season's line, and is deterministic.
- Unity build `c8`: 0 `error CS`, and `Info.plist` present. Tour `c8p` reached `MOSSELORA_QA_TOUR_DONE 130`. The hub shows the basket's ribbon with no number; the cottage card lists "12 things hold a new puzzle" and "A cutting in your basket" before the season's line.
- Seen in the shots, and not caused by SC05: the arrow's "what it plays" picture doesn't show in this hub layout, here or in the earlier `c7p` shot. The arrival toast ("Good neighbours: …") overlaps the top of the cottage card in the tour, because the tour opens the card straight after the toast.

## Not verified

- Tapping the arrow on a device, to see that it plays the helped thing. Core tests cover the choice.
- No physical device.
- No players aged 40+: age fit not validated.
