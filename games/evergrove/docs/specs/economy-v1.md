# Economy v1

Status: describes the code as of this commit. Numbers are copied from the code; file:symbol references point to `Assets/Scripts/Core/`.

## Purpose

Seeds are the only money in Mosselora. Players earn them by solving water puzzles, spend them on plants and on keepsakes, and move them to family in parcels. Nothing buys power, and nothing can be bought with real money. The economy exists so that play is always rewarded a little, repeated play is rewarded less, and giving is the cheapest way to share a plant. Checklist items 71 (one-page economy and a 365-day simulation), 73 (giving is cheaper than buying) and 80 (grinding gains less than a normal day) are checked by `Tests/EconomySimulationTests.cs` and `Tests/EconomyTests.cs`.

## Currencies

- **Seeds**: the only balance. Held in `WorldModel.Seeds`, range 0 to `WorldModel.MaxSeeds` (9,999,999). Every change goes through `WorldModel.Ledger.cs:ChangeSeeds`, which writes one ledger line (`SeedReason`, applied delta) and keeps the newest 20 (`MaxLedger`).
- **Fruit**: picked from trees, never bought. Used for gifts and jam. Range 0 to `MaxFruit` (99). Not convertible to seeds.
- **Jam**: three fruit cooked into one jar (`MakeJam`, `JamPerJar` = 3). Range 0 to `MaxJam` (10). Used for gifts only.
- **Cuttings**: plants held in a basket (`state.gifts`, max `MaxGifts` 50). Not a currency; they cost seeds only when they are bought or sent.

## Seeds in

| Source | Amount | Limit | SeedReason | Code |
|---|---|---|---|---|
| First solve of a grown thing | Tier 0: 10, tier 1: 20, tier 2: 40 | Once per thing; a garden has at most 12 things (`PuzzleCatalog.Count`), so at most 280 | FirstSolve | `WorldModel.cs:SolveTree`, `CreditSolve`, `TreeReward` |
| First solve of a bought plant | Its row's tier: 10, 20 or 40 | Once per bought plant; at most `MaxBought` 200 | FirstSolve | `WorldModel.cs:SolveBought` |
| Replay of a solved thing or bought plant | Paid under the daily cap (see Daily cap) | Replays are not capped by count, only by pay | Replay | `WorldModel.cs:PayReplay`, `ReplayPay` |
| Seed parcel opened | 1 to 500 (`FamilyParcel.MaxSeedsPerParcel`) | Once per parcel id per garden; not under the daily cap | ParcelOpened | `WorldModel.Family.cs:Open` |
| Pairing reward | none | Defined as `SeedReason.Pairing` (7) and restored from saves; no code produces it | Pairing | `WorldModel.Ledger.cs` (not built) |
| Seasonal yield | none | Defined as `SeedReason.Season` (8) and restored from saves; reserved for the run-3 seasons branch, which routes through `ChangeSeeds` | Season | `WorldModel.Ledger.cs` (not built) |

Tier of a thing is `TreePuzzles.TierFor(chapter)` = chapter / 4, capped to 0..2, so chapters 0 to 3 are tier 0, 4 to 7 tier 1, 8 to 11 tier 2.

## Seeds out

| Sink | Amount | Limit | SeedReason | Code |
|---|---|---|---|---|
| Buy a plant from the nursery | Row 0: 50, row 1: 100, row 2: 200 (`Nursery.Price`) | Row must be open (row 0 always; a row opens after owning a plant of the row before); a slot offers one plant at a time; at most `MaxBought` 200 | Nursery | `WorldModel.cs:Buy`, `RowOpen`, `NurseryStock` |
| Keepsake: take a neighbour's cutting for yourself | Full nursery price: 50, 100 or 200 | Basket room (`MaxGifts` 50) | Keepsake | `WorldModel.Family.cs:TakeKeepsake` |
| Send seeds to family | 1 to 500 | Balance must cover it | ParcelSent | `WorldModel.Family.cs:SendParcel` |
| Send a cutting to family | Gift price: 25, 50 or 100 (`Nursery.GiftPrice`) | Row must be open | ParcelSent | `WorldModel.Family.cs:SendParcel` |

Sending a cutting removes nothing from the sender's nursery or garden; the sender pays the gift price and the recipient receives the plant.

## Daily cap

Constants: `FullReplaysPerDay` = 3, `SmallReplaysPerDay` = 5, `SmallReplayReward` = 1 (`WorldModel.cs`).

- Replay 1 to 3 of a day pays the tier's full replay reward: tier 0: 2, tier 1: 4, tier 2: 8 (`TreeReward(tier, false)`).
- Replay 4 to 8 of a day pays 1.
- Replay 9 onward of a day pays 0. The replay still counts, up to `MaxReplaysPaid` 1000 in the save.
- Maximum replay income in one day is 3 x 8 + 5 x 1 = 29 (`dailyCapMax` in the test).
- Fruit follows the full-paid replays: a replay picks fruit only if it is one of the day's first 3 (v1 #76). Day 0 picks on every replay, as old callers expect.
- The count is stored with the garden as `payDay` and `replaysPaid`. A later day resets the count. An earlier day (clock set back) counts against the latest paid day, so switching dates cannot reset it.
- First solves never touch the cap. Each thing or bought plant can be first-solved once, so the first solve of the day always pays in full.
- Day 0 (or less) pays the uncapped replay reward. This exists for old saves and tests. Confirm the app always passes a real yyyymmdd; `EvergroveApp.cs` passes `day` to `SolveTree` and `SolveBought` (not verified here).

## Gifts versus buying

Gift price is `Price / 2` (`Nursery.GiftPrice`). Keepsake for yourself is the full price.

| Row | Plant price | Cutting sent to family | Keepsake for yourself |
|---|---|---|---|
| 0 | 50 | 25 | 50 |
| 1 | 100 | 50 | 100 |
| 2 | 200 | 100 | 200 |

Seed parcels are a transfer: `SendParcel` debits the sender by the amount and `Open` credits the recipient by the amount actually applied (capped at `MaxSeeds`). A sent seed parcel creates nothing. A forged parcel does create seeds (see Invariants and the findings below).

## Fruit and jam

- **Fruit in**: solving a thing whose choice was `ShadeTree` (`SolveTree`), solving a bought plant of the ShadeTree family (`SolveBought`): one fruit for the first solve and for each full-paid replay of the day (see Daily cap), never more; `TendWell` gives 2 fruit per well board, once per board waiting (`OldWell.cs`); opening a fruit parcel (up to 10 per parcel; refused with `LarderFull` if it would not all fit under `MaxFruit`).
- **Fruit out**: `GiveFruit` to Mira, Orin or Fen (one at a time, no seeds paid); `MakeJam` (3 fruit for 1 jar); fruit parcels sent (1 to 10, free).
- **Jam in**: `MakeJam` (uses 3 fruit, adds 1 jar, refused at `MaxJam` 10 without losing fruit); opening a jam parcel (1 to 3 jars; refused with `LarderFull` if it would not all fit under `MaxJam`).
- **Jam out**: jam parcels sent (1 to 3 jars, free).

Nothing is made from nothing: `MakeJam` consumes fruit first and refuses when the pantry is full. Fruit and jam never change seeds.

**Larder full (v1 #76):** `Open` refuses a fruit or jam parcel that would not all fit, with `ParcelError.LarderFull`. It changes nothing and does not remember the parcel id, so the same text opens later, after jam is made or fruit or jam is given away. Seed parcels keep their behaviour (capped at `MaxSeeds`).

## Invariants

The tests enforce these:

- Seeds are never negative and never above `MaxSeeds`; saves with seeds out of range fail to restore.
- Every change of seeds writes exactly one ledger line with its applied amount; a change that applies nothing writes none (`EconomyTests`).
- A purchase writes a `Nursery` line of minus the price; a keepsake and a sent parcel write `Keepsake` and `ParcelSent` lines.
- A sent parcel and an opened parcel are the same amount in the parcel. A parcel can be opened once per garden and never by the garden that sent it.
- Gift price is always below the nursery price and exactly half of it (`GiftsAreCheaperThanBuying`, simulation test).
- Replay pay follows the daily cap for each day; a day's replay pay never exceeds 29.
- Over 365 simulated days, the grinder's replay income beyond a daily player's is at most 365 x 5 x 1 = 1,825, and on every day the grinder's replays past the first three earn less than one normal day's replay pay at the top tier (24).
- Fruit and jam from a parcel are never silently dropped: a parcel opens in full or not at all (`LarderFull`, v1 #76).
- After the first solves are done, the balance rises by at most 29 a day, and purchases only lower it.

## What is not built yet

- 72 Prices that follow growth time and size, with a label that shows why: not built. Prices are fixed by row.
- 74 Market stall: sell fruit and jam to neighbours at seasonal prices: not built. No seeds come from fruit or jam.
- 75 Recipes combining two harvests: not built.
- 76 Storage limit that can grow (larder shelves): not built. Current limits are the fixed caps above. The no-silent-loss part is built (`LarderFull`).
- 77 No currency for sale that buys power: a policy, not code. See the stance below.
- 79 Neighbours trading wood, stone and fruit for jam: not built. `SeedReason.Pairing` exists but nothing uses it.

## Monetisation stance

Item 77 is the stance: if the game is ever monetised, only a one-time unlock or cosmetic season packs, never a currency that buys power. This is a plan. The code contains no purchases, no store calls and no paid currency. Anything that would change that needs a new spec.

## Findings the code contradicts

These are gaps between the intent of the checklist and the code. None is fixed here, and nothing in `Assets/` was changed.

1. **Seed parcels can be forged.** `FamilyParcel.TryRead` checks a SHA-256 of the parcel's own contents, which anyone can recompute. `Open` credits up to 500 seeds per parcel with no check on the sender's balance and no daily cap. Each parcel id is new, so an unlimited number of forged parcels could be opened, each adding up to 500 seeds. This is a seed source with no matching sink, so "parcels move value but never create it" holds only for honest parcels. Fix direction: sign parcels, or accept that opened seeds are a gift from anyone and cap the total per day.
2. **Fruit is farmed without a cap.** Every replay of a ShadeTree thing picks one fruit, outside the daily cap, up to 99. Fruit is not seeds today, but if item 74 (market) converts fruit to seeds, this becomes an uncapped seed source. **Fixed in this batch (v1 #76):** fruit now follows only the first solve and the day's full-paid replays.
3. **Day 0 is uncapped.** `SolveTree(item)` with no day argument pays full replay reward forever. Any caller that forgets the day bypasses the cap. The app passes `day`; this should be asserted in tests.
4. **A forward clock change is not checked.** The cap resets whenever the stored day is earlier than the new day. Setting the device clock forward a day resets the count. The 365-day test assumes real calendar days.
5. **Capped transfers lose value silently.** A seed parcel can credit less than it debited if the recipient is near `MaxSeeds`; a fruit or jam parcel loses the excess above the cap. The sender is charged the full amount. This is documented in `FamilyParcel` but not in the player's view. **Fruit and jam part fixed in this batch (v1 #76):** such a parcel is refused with `LarderFull` and changes nothing. The seed part still stands.
6. **Pairing reason is dead.** `SeedReason.Pairing` restores from saves but nothing writes it, so saves cannot yet contain it legitimately.
7. **Garden copies may duplicate the cap.** The daily cap state is copied with the garden file. Unknown whether two imported copies of one garden can both replay at full pay the same day.
8. **Bought plants pay back less than they cost.** A row-2 plant costs 200 and returns 40 on its first solve; net sinks hold per plant. Not a runaway, but a bought-then-gifted loop (sender pays 100, recipient earns 40) is also net negative for the system.

## Verification

- `Tests/EconomyTests.cs`: ledger, caps, saves, forgeries, gift prices (existing).
- `Tests/EconomySimulationTests.cs`: 365-day run of three profiles (gentle, daily, grinder) through the public API. Summary line printed on each run.
