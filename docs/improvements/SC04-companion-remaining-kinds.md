# SC04: The rest of the fact kinds

Smart companion, iteration 4 of 7 ([spec](../../games/evergrove/docs/specs/proposals/smart-companion-2026-11-27.md)).

**Status: done.** The founder approved all 612 lines (15 kinds) on 2026-11-27, and they ship in `en.json`: 912 English lines, every fact kind covered. The one exception is `RainComing`, a v1 kind that reached main during this iteration. Its 20 lines are drafted and await review, and it speaks its fixed sentence meanwhile.

## Player note

Every true thing a neighbour can say now has many wordings in each voice. Four true things the neighbours never mentioned before join in, each said once:

- how far a neighbour's hope has come;
- the season turning;
- what the season gave;
- a good-neighbour pairing you found.

## PM decision

- **Run-3 kinds on main are included.** Pairings (hints and found), visitors (I377), seasonal yields (I388–I400), the hour you usually come (I419) and the welcome-back (I420) are all on main, so all are in the feed.
- **New kinds:**
  - `WishProgress` (I313's hopes, with their true count);
  - `SeasonTurned`;
  - `YieldGiven`, which the save proves: the hub takes the yield, and `YieldTaken` records it;
  - `PairingFound`.

  They were never in the old chain, so SC01's parity is untouched. They speak only through the reviewed bank.
- **Say once.** The new kinds are old news after one telling. A `Once` fact is never said again while remembered (60 days). Without this, "you found a grove" would come back every 3 days forever.
- **Wordings that depend on the moment.** Rain is not snow, and dawn is not dusk, so these kinds are keyed by their slot in the bank (`WeatherToday.Rainy`, `Hour.Dusk`, `PairingToFind.Grove`, …):

  | Kind | Lines per voice | Lines per sub-type |
  | --- | --- | --- |
  | WeatherToday, Hour | 20 | 5 |
  | PairingToFind | 20 | 6 |
  | HourHabit | 20 | 10 |
  | RiverFurther | 20 | 5 |
  | SeasonTurned, YieldGiven, PairingFound (said once a season or once ever) | fewer | 3–4 |

  The 14-day wording rule can't bite on something said once a season, so 20 wordings there would add review load and no variety.
- **Who speaks.** Only the voices that can say a kind: Orin for letters, and Mira for the river and the weather while away. The rules file lists the voices and sub-types per kind.
- **Truth across the year.** Rain and wind fall in winter too (`DayWeather` chances), and the light lines are said in any season. So rainy, windy, hour and habit lines avoid bees, butterflies, moths, snails, leaves and dew. Visitor lines speak of the visitor's habit, not a sighting now, because the hedgehog sleeps through winter.

## Dev

- `CompanionFacts`:
  - new kinds `WishProgress`, `SeasonTurned`, `YieldGiven`, `PairingFound`;
  - `Fact.Once`;
  - `{work}` on `HelpedBy`;
  - `Words(n)` for the `{have}`/`{need}` slots.
- `CompanionRanker.Eligible`: once-facts are never said again while remembered.
- `LineBank`:
  - qualified keys `Kind.Value`, picked by the fact's slot (`QualifierOf`);
  - `For(fact, speaker)`, `CanSay`, `Keys`;
  - the strict reader refuses qualifiers on kinds that have none.
- `CompanionSpeech.Compose`: I420's welcome stays first before a bank line for the weather while away.
- `EvergroveApp`:
  - the ranker's `canSay` is today's sentence or the bank's lines;
  - a missing wording falls back to the story line, never a blank card.
- Rules restructured (`companion-line-rules.json`): per kind, its slots, voices (with sub-types), minimum per voice and minimum per sub-type. Also `allowTimeClaims` for the weather while away, because "while you were away" is that fact.
  - `tools/evergrove_line_bank_check.py` gains `--complete`, so the English bank must cover every kind. It runs once SC04's lines ship.
  - `Tests/LineBankTests.cs` enforces the same rules in C#.

## Result

- `tools/evergrove_line_bank_check.py --review`: 1,212 lines (300 shipped and approved, 912 drafts), 0 problems. The first run caught one line that started a sentence with a name.
- `dotnet run --project games/evergrove/Tests`: passed, 1,745,952 assertions. Parity is still identical over 17,487 cases. Tests cover:
  - the new facts (true only under their condition, never voiced by the old chain);
  - once-facts staying unsaid for 60 days;
  - qualified bank keys;
  - the welcome staying first;
  - the longest slot values (24-character names, the weather phrase, wish counts) being the real longest.

- Unity build `c6`: 0 `error CS`, and `Info.plist` present. Tours are run once the lines ship.

- After approval:
  - `tools/evergrove_line_bank_check.py --review --complete`: 1,844 lines, 0 problems. The English bank covers every kind except `RainComing`, which the rules mark pending.
  - The C# tests require the English bank to speak, slots filled, for every fact of every reviewed kind.
- Merged main (v1 batches up to #141), keeping both sides:
  - v1's `RainComing` fact stays;
  - v1 #90's 90-day letter wins over bank lines too, as it does over today's sentence;
  - the companion now remembers a remark only when its words were what was shown, not a "speak less" silence (v1 #85) or an arc line (v1 #83).
- Tests with `-c Release` (as CI runs): passed, 2,216,950 assertions. Parity is still identical over 17,487 cases. In Debug under heavy machine load (load average 6), v1's `WildGrowthTests` timing check ("under a second") failed at about 1.9 s. That is not a companion test.
- Unity build `c7`: 0 `error CS`, and `Info.plist` present. Tours `c7p`, `c7l` and `c7x` each reached `MOSSELORA_QA_TOUR_DONE 128`.
- The scripted week: the waiting cutting rotates its wordings, Fen notes "three of the six kinds of flower" (`WishProgress`), Orin says "It's winter now…" once (`SeasonTurned`), and the pairing hints come in new wordings. No fact repeats within 3 days, and no wording within the week.
- Seen in the week, and not the companion's: v1 #83's arc lines read in the third person in the neighbour's own mouth ("Orin is gathering timber for a small boat."). Listed as a follow-up for that session.

## Not verified
- No physical device.
- No players aged 40+: age fit not validated.
