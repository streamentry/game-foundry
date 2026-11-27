# v1 batch 6: item 77 (ticked); 74 and 79 (rules and tests, UI to do)

Date: 2026-11-27. Branch `feat/mosselora-v1-batch-6`. Two Haiku agents did the work, and the coordinator reviewed it.

## 77 No currency for sale that buys power (ticked)

[monetisation-v1.md](../../games/evergrove/docs/specs/monetisation-v1.md) states the status as found in the code and store files: free, no in-app purchases, no ads, no accounts, and no analytics in the default build. The rule: only a one-time unlock or cosmetic season packs, never currency, power, timers, loot boxes or pressure. The spec is reviewed line by line against vision §7.

**Tensions for the founder** (decisions D1–D8 in the spec):
- The demo/unlock split versus all 12 chapters being free today.
- Season packs versus §7's rejection of "seasonal exclusives" (a pack must never be time-limited).
- The planned family-circle backend versus "offline".
- The Play audience age (18+ versus 13+).
- A `submitAnalytics: 1` flag in ProjectSettings whose effect isn't established.

## 74 Market stall and 79 neighbour trades (rules only, not ticked)

`Core/Market` and `WorldModel.SellFruit`, `SellJam` and `Trade` cover existing goods only. No wood or stone was added (decided: no new currencies).

**Stall prices** in seeds, by the garden's season (Spring/Summer/Autumn/Winter):

| Good | Spring | Summer | Autumn | Winter |
| --- | --- | --- | --- | --- |
| Fruit | 3 | 2 | 1 | 3 |
| Jam | 8 | 6 | 3 | 9 |

**Trades:**
- Orin gives 4 seeds for 2 fruit.
- Fen gives 5 seeds for 1 jam.
- Mira gives 1 jam for 4 fruit.

**Daily caps:** 10 fruit, 3 jars and 3 trades per local day. The caps reset only on a later day, as with the replay cap. New ledger reasons MarketFruit (9), MarketJam (10) and Trade (11) have seed-book labels.

**No profit loop:**
- Every conversion is worth at most zero at best-available valuation in every season.
- The goods graph has no cycle (nothing turns back into fruit). Brute force over every walk of up to 6 steps confirms this.
- A 365-day greedy bot earns 3,273 seeds against an honest bound of 3,291.

**Finding:** seasons advance only with first solves, so once a garden's first solves run out its season, and so its prices, stop changing.

**Still to do:** a stall and trade sheet in the UI.

## Verification

- `.NET` suite, run as CI does: 1,619,518 assertions pass.
- Unity 6.3 runtime compile: 0 errors with both the release and dev define sets.
- `verify_evergrove_project.py` and `check_evergrove_docs.py`: OK.
