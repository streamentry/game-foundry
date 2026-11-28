# Smart companion: architecture as built

For an engineer joining the project. This page says how the neighbours choose and word their remarks today (CP01 to CP46, branch `feat/mosselora-companion-50`). The design and the reasons are in [smart-companion-2026-11-27.md](proposals/smart-companion-2026-11-27.md). The work list is in [CHECKLIST.md](../../../../docs/improvements/companion-plus/CHECKLIST.md).

Paths below are under `games/evergrove/Assets/Scripts/` unless marked otherwise. Pure rules are in `Core/` and have no UnityEngine references. Glue is in `Unity/`.

For writing and checking the lines (voices, slots, banned words, the checker), see [companion-writing-guide.md](companion-writing-guide.md).

## 1. Pipeline

```
WorldModel + CompanionContext          (save, clock, weather, light, phase, fruit, turns)
  -> CompanionFacts.For                every true fact now, each with keeper, key, weight, flags
  -> CompanionRanker.Pick / Allot      which fact each neighbour says, given memory and the day
  -> CompanionLocale.Words             a bank line (LineBank) or today's sentence (CompanionSpeech.Legacy)
  -> CompanionMemory                   what was said: PlayerPrefs, one key per garden and language
  -> EvergroveApp.NeighborSays         the order the player sees (section 1f)
```

**1a. Facts (`Core/CompanionFacts.cs`).** `CompanionFacts.For(world, context)` returns an empty list unless `world.Mode == PlayMode.Living`. It is pure: no clock, no randomness. Each `Fact` has:

- `Kind`, `Keeper` (0 Mira, trees; 1 Orin, built things; 2 Fen, flowers; -1 anyone), `Key` (a stable memory identity, such as `away:yyyyMMdd`; a typed name stays raw here and is tidied only in `Slots`), `Slots`, `Weight`, `Waiting`, `Fresh`, `Once`, and `LegacyOrder` (the old hand-ordered chain, kept for parity).

`CompanionContext` carries what the save does not hold: the calendar date and clock, last visit, letters read on this device, weather and light, this visit's fruit and "together" puzzle, the map, and hour counts.

**1b. Ranker (`Core/CompanionRanker.cs`).**

- `Pick(facts, memory, speaker, day, garden, spokenThisVisit, canSay)` is the entry point for one neighbour. If this neighbour already said something this visit, only a *fresh* fact for them, or the same key still true, can follow. Otherwise it returns null: no second remark. If nothing has been said yet, it returns the speaker's entry in `Allot`.
- `Allot` gives each neighbour their best keeper-fact. Then it shares anyone-facts out, strongest first. Each goes to the neighbour who spoke longest ago (a neighbour who never spoke comes first), with the day hash breaking ties. A neighbour takes a shared fact only if it scores more than their own best.
- `Score` = weight, plus `NewWeight` if the key was never said, plus `OwnLineWeight` if the speaker is the keeper, minus the nag damp, minus the kind cooldown.
- `Eligible` and `Variant` are section 3. `Explain` gives the top three candidates with their numbers, for QA logs only.

**1c. Words (`Core/LineBank.cs`, `Core/CompanionLocale.cs`, `Core/CompanionSpeech.cs`).**

- `LineBank` reads one JSON bank per locale from `Assets/Resources/Evergrove/Lines/<locale>.json`, shaped `{ "Kind" or "Kind.Value": { "0|1|2|any": ["line with {slot}"] } }`. A qualified kind (such as `WeatherToday.Rainy`) picks its list by one slot. Own voice first, then `any`.
- `LineBank.Speak` chooses a wording with `CompanionRanker.Variant`. If the wording was already said this visit, it is kept (a reopened card does not change its words). If any `{slot}` is empty, the result is null, and the caller falls back.
- `CompanionLocale.CanSay` and `Words` decide the language rules (section 3). `CompanionSpeech.Legacy` holds today's English sentences, English only, as the fallback. `CompanionSpeech.Compose` adds the welcome-back sentence, or replaces the weather report with the 90-day letter.

**1d. Memory (`Core/CompanionMemory.cs`).** Immutable. `With` returns a copy. Section 3 has the rules. Storage is `PlayerPrefs` text, one remark per line: `day|speaker|kind|variant|key`. It is conversation state only. It is never in the save or the garden file, so a garden restored on another device may hear a remark again.

**1e. Attention (`Core/CompanionAttention.cs`).** The same weights pick three home-screen things. None uses memory, so the screen stays steady.

- `ArrowTarget` (needs `world.Completed` and Living): the top help-from-family or new-arrival fact, else the newest unsolved nursery plant, else the newest unsolved tree.
- `Mark`: one soft mark at the top *waiting* fact: letterbox, basket or play. Never a count, never a nag.
- `CottageOrder`: the order of the cottage card's waiting lines, by weight (letters and cuttings 50, waiting 50 or 15, yield 20, wish 15), ties by day hash.

**1f. App order (`Unity/EvergroveApp.Neighbours.cs`, `NeighborSays`).** For each neighbour, first match wins:

1. Speak-less silence (`SpeakLessSilences`, idea 85): empty text.
2. Reply echo (`ReplyEchoSaid`, checklist 81): the neighbour's answer to a reply, once, within three days.
3. Arc remark (`ArcRemark`, idea 83): a project step for Orin or Fen, when the step is news.
4. The companion (`CompanionPick`, then `CompanionWords`).
5. `LocalizedVillage.Speak`: the story's own line. It is also used when the companion has nothing, or when the language has no bank.

`OpenConversation` (`Unity/EvergroveApp.Research.cs`) calls `CompanionHeard` only when the companion's own line was shown.

**1g. Simulator (`Core/CompanionSimulator.cs`).** Runs visits with no app: each day, each neighbour 0, 1, 2 is tapped once, with the same `Pick`, `Words` and `Memory.With`. Read-only. Used by tests and the sample transcript. The fact list is also cached per frame and the parsed memory per raw string (CP37, CP38).

## 2. Constants

Every number that shapes the feed, the ranking, memory or the words. "Where" is the file and the symbol.

**Weights (`Core/CompanionFacts.cs`, lines 189 to 192)**

| Constant | Value | Used for |
|---|---|---|
| `CompanionFacts.WaitingWeight` | 50 | Letters, cuttings, help from family; fresh fruit and "together" add it too |
| `CompanionFacts.RareWeight` | 20 | Weather while away, river, visitors, season turn, yield, pairing found |
| `CompanionFacts.OwnDoingWeight` | 15 | Fruit given, arrivals, puzzles solved, wish progress |
| `CompanionFacts.AmbientWeight` | 5 | Weather today, light, pairing hint, hour habit, rain coming |
| `CompanionRanker.NewWeight` | 30 | Added when the key was never said (`CompanionRanker.cs` line 23) |
| `CompanionRanker.OwnLineWeight` | 10 | Added when the speaker is the keeper (line 24) |
| `CompanionRanker.NagDamp` | 40 | Taken off a waiting fact that nagged (line 28) |
| `CompanionRanker.KindCooldown` | 15 | Taken off a kind the same neighbour used yesterday (line 30) |

**Kind weights as built** (base weight, then the flags). Keeper: `v` means the neighbour named in the fact; `any` means anyone.

| Kind | Keeper | Weight | Flags |
|---|---|---|---|
| FruitGiven | v | 15 (+50 when fresh) | fresh when given this visit |
| SolvedTogether | v | 65 (15 + 50) | fresh |
| WeatherWhileAway | 0 | 20 | |
| LetterWaiting | 1 | 50 | waiting |
| GiftWaiting | v | 50 | waiting |
| HelpedBy | from the puzzle's owner | 50 | waiting |
| NewArrival | v | 15 | |
| RiverClose | 0 | 20 | |
| RiverFurther | 0 | 20 | |
| KindSolved | v | 15 | |
| WeatherToday | any | 5 | |
| PairingToFind | v | 5 | |
| Hour | any | 5 | only with the light on |
| NewestThing | v | 0 | |
| VisitorCame | v | 20 | |
| HourHabit | v | 5 | |
| WishProgress | v | 15 | once |
| SeasonTurned | any | 20 | once |
| YieldGiven | 2 in spring, 0 in summer and autumn, any in winter | 20 | once |
| PairingFound | v | 20 | once |
| RainComing | 1 | 5 | legacy-off |

| Constant | Value | Where |
|---|---|---|
| `CompanionFacts.Step` (private) | 100000 | Legacy order band per step |
| `CompanionFacts.NewKindsOrder` (private) | 14 × Step | Legacy order of the newer kinds |
| `Fact.AnyKeeper` | -1 | Keeper meaning "anyone" |
| `CompanionFacts.LegacyOffSlot` | `"_legacy-off"` | Marks facts the old chain never said |
| `CompanionFacts.MaxNameElements` | 24 | Text elements kept in a tidied name (line 512) |
| `CompanionContext.DayTurnsAtHour` | 4 | The companion's day turns at 04:00 (line 158) |

| Constant | Value | Where |
|---|---|---|
| `CompanionRanker.KeyDays` | 3 | A key is not repeated within 3 days, unless waiting or fresh (line 21) |
| `CompanionRanker.VariantDays` | 14 | A fixed sentence waits 14 days; a wording is not reused in one voice within 14 days (line 22) |
| `CompanionRanker.NagDays` | 3 | Window for counting nags (line 26) |
| `CompanionRanker.NagTimes` | 2 | Days heard in the window before the damp applies (line 27) |
| `CompanionMemory.KeepDays` | 60 | Memory keeps a remark this long (except once-kinds) (`CompanionMemory.cs` line 35) |
| `CompanionMemory.MaxEntries` | 300 | Memory cap (line 36) |
| `CompanionMemory.StorageKeyPrefix` | `"evergrove:companion-heard:"` | PlayerPrefs prefix (line 39) |
| `ReplyChips.EchoWindowDays` | 3 | Reply echo window (`Core/ReplyChips.cs` line 112) |

| Constant | Value | Where |
|---|---|---|
| `Companion.PhaseCount` | 6 | Hour buckets in the habit counts (line 13) |
| `Companion.HabitNeeds` | 5 | Visits to one hour before a habit shows (line 14) |
| `Companion.LongAway` | 7 | Days away for the "good to see you" welcome (line 15) |
| `Companion.VeryLongAway` | 30 | Days away for the longer welcome (line 15) |
| `Companion.LetterAway` | 90 | Days away before the return letter replaces the weather (line 16) |

The habit rule also uses the day number: a habit is said only on its noticer's day, `dayNumber % 7 == villager × 2` (line 47).

| Constant | Value | Where |
|---|---|---|
| `CompanionLocale.English` | `"en"` | The English locale code (`Core/CompanionLocale.cs` line 21) |
| `CompanionLocale.LocalizedSlots` | `{ "name" }` | The only slot a non-English line may use without a translation |
| `LineBank.AnyVoice` | `"any"` | Shared voice (`Core/LineBank.cs` line 17) |
| Voices | `0`, `1`, `2`, `any` | The only voice keys a bank may use |
| Lines per voice | `min` 20 (WishProgress 10, SeasonTurned 12, YieldGiven 3, PairingFound 8) | `docs/specs/companion-line-rules.json` |
| Lines per qualifier value | `minEach` 3 to 10 (SeasonTurned 3, YieldGiven 3, PairingFound 4, RiverFurther 5, WeatherToday 5, Hour 5, PairingToFind 6, HourHabit 10) | same file |
| `maxChars` | 140 | same file |
| `maxWords` | 28 | same file |
| `NEAR_DUPLICATE` | 0.75 | Word overlap that counts as a near-duplicate (`tools/evergrove_line_bank_check.py` line 43) |
| `OPENING_WORD_MAX` | 5 | Opening-word check (line 218) |

Speak-less keeps about half of the remark slots, by hash parity (`Core/QuietRemarks.cs`). The companion clock is `DateTime.Now` plus the tour's day offset (`Unity/EvergroveApp.Daylight.cs`).

## 3. The rules in plain words

- **Once-facts.** Wishes, season turns, yields and pairings found are said once for good. Memory keeps their keys past 60 days (`IsOnceKind`).
- **Eligibility, in order.** A once-fact already said is out. A waiting or fresh fact is in. A fixed sentence (no wording to vary) heard within 14 days is out. Any other fact heard within 3 days is out. Everything else is in.
- **Waiting and fresh.** Waiting facts (unread letter, cutting in the basket, help from family) skip the 3-day and 14-day rules and the kind cooldown; their nag damp still applies. Fresh facts (fruit handed over, puzzle solved together) are said at once.
- **Nag damp.** A waiting, non-fresh fact said by this neighbour on 2 of the last 3 days (yesterday and the two before) loses 40. Today is not counted; the visit lock covers today.
- **Kind cooldown.** A non-waiting, non-fresh fact loses 15 for a neighbour who used the same kind yesterday. A penalty, not a ban.
- **Wording.** A hash of (garden, day, key) picks the wording. It steps past any wording heard in that voice within 14 days; if all were heard, the one heard longest ago. A wording said this visit is kept.
- **The 4 a.m. day.** Facts use the calendar date. The companion's day (memory and the visit lock) turns at 04:00, so 00:30 still belongs to the evening before.
- **Clock moved back.** A remark dated after today is re-dated to today (age 0), never dropped or blocked for ever.
- **Pruning.** Over 60 days old, dropped (except once-kinds). Over 300 entries, the oldest repeats go first, so each key keeps its newest remark.
- **Turns and visits.** Anyone-facts go to the neighbour who spoke longest ago, one per neighbour. Within a visit a remark stays put; only a fresh fact can follow, and if the remark is no longer true there is no second one.
- **Languages.** English: the English bank, else today's sentence. Another language with a bank: that bank only, and every slot must be `name` (the only localized slot); the app passes no translator, so no other slot can appear. Another language without a bank: the companion is silent and `LocalizedVillage` speaks, as before SC01. Memory has one key per language: English `evergrove:companion-heard:<garden>`, others `evergrove:companion-heard:<locale>:<garden>` (lowercased, unsafe characters become `-`). A reset deletes every language's key (CP41).
- **Tidy names.** `TidyName` trims a typed name for the words: braces and pipes dropped, whitespace made single spaces, at most 24 text elements. A name that tidies to nothing gives no sentence (CP35). The memory key keeps the typed name.
- **Bank checks (checker, not runtime).** Banned phrases (health, memory, loneliness, streaks), child-unsafe words, time claims (unless a kind allows them), no `!`, no double spaces or mixed quotes. Two lines in one kind and voice with 0.75 word overlap are warnings. Counts are checked per kind and voice, with `minEach` per qualifier value.

## 4. Tests and tools

**C# tests (`games/evergrove/Tests/`).** A custom runner, not xUnit. Each class is `internal static class XTests` with `public static int Run()`. They are registered in `Tests/Program.cs`.

- `Companion*Tests.cs` (facts, ranker, memory, locale, attention, simulator, day, name, rules, signature, fuzz, perf), `HonestCompanionTests.cs`, `LineBankTests.cs`.
- `CompanionSimulatorTests.WriteSampleTranscript` writes the 30-day transcript only when `EVERGROVE_COMPANION_TRANSCRIPT` is set to an absolute output path. Otherwise it adds no assertions.

Run the tests:

```sh
cd games/evergrove/Tests && DOTNET_CLI_HOME=/Volumes/SSD/dotnet-home /Volumes/SSD/dotnet/dotnet run -c Release
```

Sample transcript (CP46), 30 simulated days written to a file. It is not a played session:

```sh
cd games/evergrove/Tests && EVERGROVE_COMPANION_TRANSCRIPT=<absolute path to .md> DOTNET_CLI_HOME=/Volumes/SSD/dotnet-home /Volumes/SSD/dotnet/dotnet run -c Release
```

**Line bank checker (`tools/evergrove_line_bank_check.py`).** `--complete` (every non-pending kind must have English lines, CP44), `--strict` (warnings count as problems; CI runs `--complete --strict`), `--review` (also the drafts in `docs/improvements/review/`), `--report` (coverage table). Python tests: `python3 -m unittest discover -s tools/tests` (CP45).

**Translators.** `tools/evergrove_line_bank_export.py --out PATH.csv` writes the rows. `tools/evergrove_line_bank_import.py --locale fr --csv PATH.csv` checks the filled rows and writes a DRAFT review file, never a bank.

**Playtest export (CP39).** A dev-only control in `Unity/EvergroveApp.Research.cs` copies `CompanionMemory.ToReadable` (dates, speakers, kinds; no keys, no wording).

## 5. QA logs

The dev tour (`Unity/DevQaTour.cs`, lines 914 and 915) prints two lines per neighbour per day:

- `MOSSELORA_COMPANION day N <Name>: <words>`: what the neighbour said.
- `MOSSELORA_COMPANION_WHY day N <Name>: <top three candidates>`: each line reads `Kind Key score= base= new= own= nag= cooldown= eligible|not-eligible:reason` (`CompanionRanker.Explain`, CP40).

## 6. Not verified

- No device run of the CP changes yet. The tests and simulator are the only evidence.
- No players aged 40 or over have used the companion. Age fit is not validated. The 40–49, 50–64 and 65+ bands are not separated by any data.
- The non-English banks are not reviewed by native speakers, and the app passes no slot translator.
- No claim of wellbeing, memory or loneliness effects is supported. Do not write one.
