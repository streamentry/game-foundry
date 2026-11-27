# v1 economy, batch 1: items 71, 73, 78 (core), 80

Date: 2026-11-27. Checklist: [100-improvements-v1.md](100-improvements-v1.md), section H. Branch `feat/mosselora-v1-economy-71-80`. Implementation by Haiku agents under review; review fixes noted below.

## Preflight

| Question | Answer |
| --- | --- |
| Truth | Observed in code: seeds came from first solves (10/20/40) and unlimited replays (2/4/8), with no daily limit and no record of where seeds went. A cutting sent to family cost the same as buying it. |
| Owner | The founder's checklist, section H (items 71, 73, 78, 80). |
| Delete | No new screens. The ledger UI (78) and the other economy features (72, 74–77, 79) are deferred. |
| User value | Replaying stays open but can't be farmed. Giving is cheaper than buying. Every seed movement is accountable. |
| Audience fit | `age fit not validated`. No 40+ player has seen these rules. |
| Falsification | Simulation: does a grinder out-earn a normal daily player by more than the cap? (No: at most 5 seeds a day.) |
| Stop | If playtesters read a 0-seed replay as "broken", replace the hidden pill with a plain "just for fun" note. |
| Evidence | `Tests/EconomyTests.cs` and `Tests/EconomySimulationTests.cs`. Full suite: 1,268,208 assertions pass. |

## What changed

- **78 (core only, not ticked):** `WorldModel.ChangeSeeds` is now the only place seeds change. Each change writes a ledger line (`SeedReason`, applied delta), and the newest 20 are kept in the save. `SeedReason.Pairing` is reserved for the pairing reward on the I368/I380 branch. Players can't see the ledger yet; that needs a small "seed book" page.
- **80 (ticked):** replays pay full 3 times per local day, then 1 seed 5 times, then nothing. First solves always pay full. Only a later day resets the count, so setting the clock back can't. The app shows the reward the board will actually pay, and hides the seed pill when it pays 0.
- **73 (ticked):** a cutting sent to family costs `Nursery.GiftPrice`, half the nursery price. A keepsake for yourself stays at full price. The family sheet shows the gift price.
- **71 (ticked):** [economy-v1.md](../../games/evergrove/docs/specs/economy-v1.md) covers every source and sink with exact numbers. A 365-day simulation runs three profiles: gentle/daily/grinder end balances = 10/8,920/10,625, and the grinder's extra pay is at most 5 seeds a day.

## Review fixes on top of the Haiku output

- The cap reset on any day change, so switching between two dates reset it. Now only a later day resets it (tested).
- "Send this board to family" was gated on `reward > 0`, so it would have vanished on capped replays. It's now gated on an explicit `ownBoard` flag.

## Verification

- `.NET` domain suite: 1,268,208 assertions pass. The StreamPath "generation is fast" timing check flaked once under machine load and passed on rerun.
- `tools/verify_evergrove_project.py`: OK.
- Runtime scripts compile against the Unity 6.3 engine assemblies with 0 errors. **Not run** in the Unity editor, a tour, or on a device.

## Open findings (not fixed here)

From the spec's "Findings" section:

- **Seed parcels can be forged** (the content hash is self-verifiable), which creates seeds. This predates this batch. Fixing it needs signed parcels or the family backend (item 61, founder decision).
- **Fruit from ShadeTree replays is uncapped.** This must be capped before a market stall (74) converts fruit to seeds.
