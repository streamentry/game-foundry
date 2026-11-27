# Economy v1

Status: describes the code as of this commit. Numbers are copied from the code; file:symbol references point to `Assets/Scripts/Core/`.

## Purpose

Seeds are the only money in Mosselora. Players earn them by solving water puzzles, spend them on plants and on keepsakes, and move them to family in parcels. Nothing buys power, and nothing can be bought with real money. The economy exists so that play is always rewarded a little, repeated play is rewarded less, and giving is the cheapest way to share a plant. Checklist items 71 (one-page economy and a 365-day simulation), 73 (giving is cheaper than buying) and 80 (grinding gains less than a normal day) are checked by `Tests/EconomySimulationTests.cs` and `Tests/EconomyTests.cs`.

## Currencies

- **Seeds**: the only balance. Held in `WorldModel.Seeds`, range 0 to `WorldModel.MaxSeeds` (9,999,999). Every change goes through `WorldModel.Ledger.cs:ChangeSeeds`, which writes one ledger line (`SeedReason`, applied delta) and keeps the newest 20 (`MaxLedger`).
- **Fruit**: picked from trees, never bought. Used for gifts, jam, and the market stall and neighbour trades (below). Range 0 to `FruitCapacity`: the base `MaxFruit` (99) plus 20 for each larder shelf (item 76, below).
- **Jam**: three fruit cooked into one jar (`MakeJam`, `JamPerJar` = 3). Range 0 to `JamCapacity`: the base `MaxJam` (10) plus 5 for each larder shelf. Used for gifts, the market stall and neighbour trades.
- **Cuttings**: plants held in a basket (`state.gifts`, max `MaxGifts` 50). Not a currency; they cost seeds only when they are bought or sent.

## Seeds in

| Source | Amount | Limit | SeedReason | Code |
|---|---|---|---|---|
| First solve of a grown thing | Tier 0: 10, tier 1: 20, tier 2: 40 | Once per thing; a garden has at most 12 things (`PuzzleCatalog.Count`), so at most 280 | FirstSolve | `WorldModel.cs:SolveTree`, `CreditSolve`, `TreeReward` |
| First solve of a bought plant | Its row's tier: 10, 20 or 40 | Once per bought plant; at most `MaxBought` 200 | FirstSolve | `WorldModel.cs:SolveBought` |
| Replay of a solved thing or bought plant | Paid under the daily cap (see Daily cap) | Replays are not capped by count, only by pay | Replay | `WorldModel.cs:PayReplay`, `ReplayPay` |
| Seed parcel opened | 1 to 500 (`FamilyParcel.MaxSeedsPerParcel`) | Once per parcel id per garden; not under the daily cap | ParcelOpened | `WorldModel.Family.cs:Open` |
| Pairing reward | none | Defined as `SeedReason.Pairing` (7) and restored from saves; no code produces it | Pairing | `WorldModel.Ledger.cs` (not built) |
| Market stall: fruit sold | Season price x count: fruit 3 (spring), 2 (summer), 1 (autumn), 3 (winter) | At most 10 fruit per local day; the garden's own season only; whole sale or nothing | MarketFruit (9) | `Market.cs:FruitPrice`, `WorldModel.Market.cs:SellFruit` |
| Market stall: jam sold | Season price x count: jam 8, 6, 3, 9 | At most 3 jars per local day; whole sale or nothing | MarketJam (10) | `Market.cs:JamPrice`, `WorldModel.Market.cs:SellJam` |
| Neighbour trade: Orin | 4 seeds for 2 fruit (fixed) | Trades per local day, all neighbours together: at most 3 | Trade (11) | `Market.cs:TradeTable`, `WorldModel.Market.cs:Trade` |
| Neighbour trade: Fen | 5 seeds for 1 jar of jam (fixed) | As above | Trade (11) | As above |

Mira's trade (4 fruit for 1 jar of jam) moves goods and pays no seeds, so it writes no ledger line.
| Seasonal yield | none | Defined as `SeedReason.Season` (8) and restored from saves; reserved for the run-3 seasons branch, which routes through `ChangeSeeds` | Season | `WorldModel.Ledger.cs` (not built) |

Tier of a thing is `TreePuzzles.TierFor(chapter)` = chapter / 4, capped to 0..2, so chapters 0 to 3 are tier 0, 4 to 7 tier 1, 8 to 11 tier 2.

## Seeds out

| Sink | Amount | Limit | SeedReason | Code |
|---|---|---|---|---|
| Buy a plant from the nursery | Row 0: 50, row 1: 100, row 2: 200 (`Nursery.Price`) | Row must be open (row 0 always; a row opens after owning a plant of the row before); a slot offers one plant at a time; at most `MaxBought` 200 | Nursery | `WorldModel.cs:Buy`, `RowOpen`, `NurseryStock` |
| Keepsake: take a neighbour's cutting for yourself | Full nursery price: 50, 100 or 200 | Basket room (`MaxGifts` 50) | Keepsake | `WorldModel.Family.cs:TakeKeepsake` |
| Send seeds to family | 1 to 500 | Balance must cover it | ParcelSent | `WorldModel.Family.cs:SendParcel` |
| Send a cutting to family | Gift price: 25, 50 or 100 (`Nursery.GiftPrice`) | Row must be open | ParcelSent | `WorldModel.Family.cs:SendParcel` |
| Larder shelf (item 76) | 150, 300, 600 for the first, second and third shelf (`WorldModel.Larder.cs:ShelfPrices`) | At most 3 shelves; Living gardens only; balance must cover it | Larder (12) | `WorldModel.Larder.cs:BuyShelf` |

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
- **Fruit out**: `GiveFruit` to Mira, Orin or Fen (one at a time, no seeds paid); `MakeJam` (3 fruit for 1 jar); fruit parcels sent (1 to 10, free); market stall (`SellFruit`, seeds in); Orin's trade (2 fruit for 4 seeds); Mira's trade (4 fruit for 1 jar).
- **Jam in**: `MakeJam` (uses 3 fruit, adds 1 jar, refused at `MaxJam` 10 without losing fruit); opening a jam parcel (1 to 3 jars; refused with `LarderFull` if it would not all fit under `MaxJam`).
- **Jam out**: jam parcels sent (1 to 3 jars, free); market stall (`SellJam`, seeds in); Fen's trade (1 jar for 5 seeds).

Nothing is made from nothing: `MakeJam` consumes fruit first and refuses when the pantry is full. Fruit and jam change seeds only by being sold to the stall or traded (below), and every such sale removes the goods it pays for.

## Market stall and neighbour trades (items 74 and 79)

Code: `Market.cs` (prices, trade table, caps, valuation) and `WorldModel.Market.cs` (SellFruit, SellJam, Trade, validation). Living gardens only.

**Deviation (decided):** the game has no wood or stone and adds no currency. Item 79 is built over the existing goods only: Orin gives seeds for fruit, Fen gives seeds for jam, and Mira swaps fruit for a jar of jam. Seeds stay the only money.

**Prices follow the season, not the date.** GardenSeason follows progress and never the clock, so there is no date-to-season mapping. A price is a fixed function of the garden's season. `SellFruit` and `SellJam` take a season, which must equal `GardenSeason.For(world)`; any other season is refused, so the caller cannot choose a dearer one.

| Season | Fruit (stall, each) | Jam (stall, each) | Orin (fruit, fixed) | Fen (jam, fixed) |
|---|---|---|---|---|
| Spring | 3 | 8 | 2 per fruit | 5 per jar |
| Summer | 2 | 6 | 2 per fruit | 5 per jar |
| Autumn | 1 | 3 | 2 per fruit | 5 per jar |
| Winter | 3 | 9 | 2 per fruit | 5 per jar |

So the stall is best in spring and winter; Orin and Fen beat it in autumn (and Fen in summer). Mira's rate is 4 fruit for a jar, one more than the kitchen's 3, so cooking your own jam stays better.

**Caps (per local day, all-or-nothing):** 10 fruit sold, 3 jars sold, 3 trades of any neighbour. Refused calls change nothing. A later day starts a fresh count; an earlier date counts against the latest day already used, so setting the clock back cannot reset the cap (the same rule as the replay cap). A day of 0, or outside a yyyymmdd range, is refused. A sale or trade that would overflow seeds, fruit or jam is refused whole.

**Saved as** (optional, absent in older JSON, not written while 0): `marketStamp` (the latest day the caps count for), `fruitSold`, `jamSold`, `tradesDone`. Fail-closed on restore: each count is within its cap, a stamp of 0 holds no counts, and all are 0 outside a Living garden. The stamp is named without "day" because `DailyRhythmAuditTests` allows only `payDay` as a date stamp.

**New seed reasons** (appended, values fixed once shipped): `MarketFruit` = 9 (fruit sold), `MarketJam` = 10 (jam sold), `Trade` = 11 (seeds from Orin or Fen), `Larder` = 12 (a shelf bought). Every seed change writes one ledger line.

### No-profit invariant (item 79)

No sequence of sales, trades and cooking turns a basket into more total value, with no outside input.

**Valuation.** `Market.ValueOf(good, season)` values a unit at the best seeds the garden can get for it that season: fruit is worth max(stall price, Orin's rate); jam is worth max(stall price, Fen's rate); a seed is worth 1. This gives spring fruit 3, summer 2, autumn 2, winter 3; jam 8, 6, 5, 9. The valuation is not the cheapest season's price, because that would make every ordinary stall sale look like a gain.

**Proof.** Within one season, each conversion has gain (value out minus value in) at most zero:
- stall sales: price is at most the valuation, so gain is at most 0;
- Orin and Fen: their rates are at most the valuation by construction;
- Mira and cooking (3 fruit to 1 jar): value of a jar is at most the value of the fruit it takes in each season (for example spring 8 at most 9, summer 6 at most 6, autumn 5 at most 6, winter 9 at most 9).

No conversion produces fruit, so the goods graph (fruit, jam, seeds) has no cycle. `MarketTests.NoProfitLoop` checks each edge in each season and brute-forces every walk of up to 6 steps from each good: 40 walks, no closed walk.

**Run-time check.** `MarketTests.ArbitrageBot` plays 365 days with a greedy seller under the caps. Every market step is checked: the valuation never rises, and no step creates fruit or jam. Its seeds from fruit sold to the market (3,273) stay at or below the best-price value of all fruit from play (1,097 x 3 = 3,291). Fruit from play is itself capped: at most 3 full-paid replays a day plus one for each ShadeTree first solve.

**Limit (accepted).** Goods held across a season change can gain up to the spread in valuation between seasons, which is at most 1 seed per fruit (`MarketTests` checks the bound). That gain comes from progress, which is play, not from the market.

## Recipes and the larder (items 75 and 76)

Code: `Recipes.cs` (the recipe table, found mask, `CookRecipe`) and `WorldModel.Larder.cs` (shelves and capacities). Both live-only.

**Recipes: named cooking, no new food (decided).** A new stored food would need its own parcel gift, a market price that keeps the no-profit proof true, a ledger path and almanac copy. None of that is in this batch, and a second harvest would have no use in the game's goods except fruit and jam. So a recipe is a named way of cooking goods the garden already has. The table has one recipe:

| Recipe | Takes | Makes | Found | Code |
|---|---|---|---|---|
| Jam | 3 fruit | 1 jar of jam | the first jar, from any cooking path | `Recipes.Jam`, `MakeJam` |

- A recipe is found the first time it is cooked (`MakeJam` sets the bit, so the kitchen card and the stall's Kitchen section both find it). A refused cook finds nothing.
- Found recipes are a mask, `recipes` in the save (bit n = recipe n, not written while 0). Restore refuses bits above the table and any mask outside a Living garden.
- `Recipes.Found(world)` gives the names the almanac lists ("Recipes found: Jam"); nothing is listed before a recipe is found.
- The one recipe is value-nonincreasing: `RecipeTests` checks that three fruit are worth at least one jar in every season (jam is valued at 5 to 9, fruit at 2 to 3 each).
- **Item 75 is not met.** The checklist asks for two harvests combined into a new food. That needs a new food and the work above; it is a spec decision, not done here.

**Larder: shelves grow the caps (item 76).**

| Shelves | Fruit capacity | Jam capacity | Seeds for this shelf |
|---|---|---|---|
| 0 (base) | 99 | 10 | none |
| 1 | 119 | 15 | 150 |
| 2 | 139 | 20 | 300 |
| 3 | 159 | 25 | 600 |

- Shelves are bought at the stall for seeds (one `Larder` ledger line, minus the price). A shelf is refused when the garden is not Living, the larder has three shelves, or the purse is short; a refusal changes nothing.
- `FruitCapacity` and `JamCapacity` replace the base caps in `PickFruit`, `Open` (`LarderFull`), `MakeJam` and the restore checks. The base constants stay as the floor.
- Saved as `larderShelves` (0..3, not written while 0). Restore refuses shelves outside 0..3, shelves outside a Living garden, and fruit or jam above the capacity for the stored shelf count.
- A full larder still refuses parcels with `LarderFull` and drops nothing.

**Not yet routed to the capacity (follow-up).** These sites still use the base constants, so shelves do not reach them. None loses goods silently, but they refuse or clip at 99 or 10 (see Findings, item 10).

## What is not built yet

- 72 Prices that follow growth time and size, with a label that shows why: not built. Prices are fixed by row.
- 75 Recipes combining two harvests: not built. Recipes exist as named cooking (see Recipes and the larder); no new food is stored.
- 76 Storage limit that can grow: built for the stall's Larder row, `PickFruit`, parcels and the kitchen (see Recipes and the larder). Not yet routed: the four sites in Findings item 10. The no-silent-loss part is built (`LarderFull`).
- 77 No currency for sale that buys power: a policy, not code. See the stance below.
- 74 and 79 are built (see Market stall and neighbour trades), with the "existing goods only" deviation.

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
- After the first solves are done, the balance rises by at most 29 a day from replays, and purchases only lower it. Market sales and trades add seeds, but they are bounded by the market caps and by fruit and jam from play (see Market stall and neighbour trades).
- No market step raises the garden's valuation (see Market stall and neighbour trades), and none creates fruit or jam (`MarketTests`).

## What is not built yet

- 72 Prices that follow growth time and size, with a label that shows why: not built. Prices are fixed by row.
- 74 Market stall: sell fruit and jam to neighbours at seasonal prices: not built. No seeds come from fruit or jam.
- 75 Recipes combining two harvests: not built. Recipes exist as named cooking (see Recipes and the larder); no new food is stored.
- 76 Storage limit that can grow: built for the stall's Larder row, `PickFruit`, parcels and the kitchen (see Recipes and the larder). Not yet routed: the four sites in Findings item 10. The no-silent-loss part is built (`LarderFull`).
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
9. **Seasons stop turning once first solves run out.** A season turns every 4 first solves (`GardenMap.SolvesPerReach`), and only first solves count toward `PuzzlesSolved` (bought plants count too). A garden with its 12 grown things solved reaches spring and then stays there until a bought plant is solved. Because stall and trade prices depend on the season, prices go static for most gardens. This is consistent with "a garden left alone waits unchanged", but it means the market is mostly a spring market for players who have solved their grown things. Decide before shipping whether the seasons should keep turning through replays or through a clock, which would need a new spec.
10. **Larder shelves do not reach every cap yet.** These sites still compare against the base constants `MaxFruit` (99) or `MaxJam` (10), so a bought shelf does not apply there: `SeasonalYields.cs` (autumn fruit clipped at 99, and the jam yield refused at 10), `Pairings.cs` (a bee-visit fruit refused at 99), `WorldModel.Market.cs` `HasRoom` (a trade that would take fruit or jam above the base cap is refused, which also blocks Mira's and Orin's trades into a larger larder), and `EvergroveApp.cs` (the cottage's "Make a jar of jam" button is hidden at 10 jars). Fix direction: route each through `FruitCapacity` and `JamCapacity`. Not done in this batch because those files were outside its scope.

## Verification

- `Tests/EconomyTests.cs`: ledger, caps, saves, forgeries, gift prices (existing).
- `Tests/MarketTests.cs`: prices, sales and trades per season, refusals and caps, saves and forgeries, the no-profit walk over all seasons, and a 365-day greedy market bot.
- `Tests/EconomySimulationTests.cs`: 365-day run of three profiles (gentle, daily, grinder) through the public API. Summary line printed on each run.
