# Companion writing guide

For whoever writes or reviews the neighbours' lines next, in English or another language. Every rule here comes from [companion-line-rules.json](companion-line-rules.json) or [tools/evergrove_line_bank_check.py](../../../../tools/evergrove_line_bank_check.py). If this page and those files disagree, the files win. How the lines are chosen and spoken is in [companion-architecture.md](companion-architecture.md).

## 1. Who speaks

Three neighbours say the lines. Each has a voice number, a keeper role, and the kinds of fact they own.

| Voice | Neighbour | Keeps | Tone (from the SC03 log and the review notes) |
|---|---|---|---|
| 0 | Mira | trees, the river | Warm, family sayings. Often "my grandmother said" or "my mother would". |
| 1 | Orin | built things: roofs, bridge, tools | Practical and dry. Notices craft, the sort of thing a shed or bench teaches. |
| 2 | Fen | flowers and small creatures | Bright and specific: bees, ladybirds, moths, wrens, butterflies. |

Each voice keeps to its own line of work. The WeatherToday notes put it as Mira on trees and river, Orin on roofs, bridge and tools, Fen on flowers and small creatures. The "any" list is shared: the game uses a speaker's own voice first, then "any" (architecture section 1c). The VisitorCame notes say each voice speaks only of its own visitor.

Small-creature facts must be true (SC03: butterflies taste with their feet, bees can see colours we can't). Animal habits follow the real rules in the code (owl at night, heron by day, hedgehog at dusk).

## 2. House rules

Derived from the rules file and the checker. Plain words first, then the mechanical check.

- **Specific.** A line names a real thing in the garden or a real habit.
- **Quiet.** No exclamation marks (the checker refuses "!"). Keep to the length limit below.
- **Varied.** Each kind needs at least its minimum per voice (20 for most; SeasonTurned 12, WishProgress 10, PairingFound 8, YieldGiven 3). No line may repeat in another voice or kind (CP26).
- **True.** Do not claim an event the save cannot prove (SC03). The KindSolved notes add: say nothing about how the player solved it. The HourHabit notes add: "often" or "usually" are true, "always" is not.
- **No guilt.** Banned: "missed you", "miss you", "come back", "waiting for you", "where have you been", "don't forget", "do not forget", "love you", "hurry up", "only today", "lonely", "loneliness".
- **No streaks or repetition pressure.** Banned: "streak", "in a row", "every day". The checker also flags "again".
- **No health, wellbeing or cognition claims.** Banned: "health", "healthy", "wellbeing", "well-being", "brain", "cognitive", "memory loss", "dementia", "therapy", "therapeutic", "medicine", "doctor", "good for you", "keeps you sharp", "stay sharp", "exercise your", "anxiety", "stress", "depress". Do not write a benefit claim even if it sounds harmless.
- **Child-safe.** Nothing a 7-year-old co-player should not read. The list is: kill, dead, death, die, dying, blood, drunk, beer, wine, hell, damn, stupid, hate, ugly, shut up, idiot, gun, knife.
- **Time claims only where allowed.** "yesterday", "last night", "last week", "this morning", "tonight", "days ago", "weeks ago", "since you", "while you were", "for days", "for weeks", "all week", "again". Only WeatherWhileAway may use them (its `allowTimeClaims` flag in the rules file).
- **Length.** At most 140 characters and 28 words, counted after filling every slot with the longest real value in the rules file (`longest`). That is the worst case, so a line that passes is safe for every garden.
- **Slots never start a sentence.** `{thing}` at the start of a sentence would be lowercase. Put the slot after a word.
- **No "a" or "an" before a slot.** "a {thing}" breaks when the value is "an apple" or plural.
- **`{thing}` is never a verb's subject.** "cowslips" is plural, so "{thing} is" breaks. The checker catches `{thing}` (optionally followed by `{where}`) directly before one of the listed verbs, such as "is", "has", "grows", "came"; rephrase so a verb does not follow the slot.
- **No digits.** Counts come from slots such as `have` and `need`, which are words.
- **Punctuation (CP25).** No double spaces, no leading or trailing space, end with . ! ? or … (a closing quote or bracket may follow). Do not mix straight (' ") and curly quotes in one line. No space before , . ! ? ; or :.

## 3. Slots and qualifiers

Slots are filled by the game. A line may use only the slots its kind fills (the checker rejects any other `{name}`).

| Kind | Slots | Voices | Min per voice |
|---|---|---|---|
| NewestThing | thing, where | 0, 1, 2 | 20 |
| KindSolved | work | 0, 1, 2 | 20 |
| SolvedTogether | work | 0, 1, 2 | 20 |
| GiftWaiting | thing | 0, 1, 2 | 20 |
| NewArrival | thing | 0, 1, 2 | 20 |
| HelpedBy | name, work | 0, 1, 2 | 20 |
| LetterWaiting | name | 1 only | 20 |
| WeatherWhileAway | weather | 0 only | 20 |
| WishProgress | have, need | 0, 1, 2 | 10 |
| FruitGiven | none | 0, 1, 2 | 20 |
| VisitorCame | none | 0, 1, 2 | 20 |
| RiverClose | none | 0 only | 20 |
| RainComing | none | 1 only | 20 |
| WeatherToday, Hour, SeasonTurned, YieldGiven, RiverFurther, PairingToFind, PairingFound, HourHabit | none (qualified; see below) | see below | see below |

**Qualifier groups.** A qualified kind is keyed `Kind.Value` in the bank (for example `WeatherToday.Rainy`). One slot-free choice picks the group, so the line can refer to the right fact (rain, dusk, a grove) without a slot. Each listed value needs its own minimum (`minEach`).

| Kind | Qualifier | Values by voice | minEach |
|---|---|---|---|
| WeatherToday | weather | all three: Rainy, Windy, Hot, Snowy | 5 |
| Hour | phase | all three: Dawn, Golden, Dusk, Night | 5 |
| HourHabit | phase | 0: Dawn, Morning; 1: Golden, Dusk; 2: Day, Night | 10 |
| RiverFurther | season | 0: all four seasons | 5 |
| SeasonTurned | season | all three: all four seasons | 3 |
| YieldGiven | season | 0: Summer, Autumn, Winter; 1: Winter; 2: Spring, Winter | 3 |
| PairingToFind | pairing | 0: Grove, Orchard; 1: ByTheWater, ShadySeat; 2: Sheltered, FlowerBed, Waterside | 6 |
| PairingFound | pairing | the same values as PairingToFind | 4 |

Why the qualifiers exist: the game knows the true facts, such as "it is raining" or "the grove is two trees side by side". Each qualifier value keeps the line true for that case. A "Rainy" line must not describe snow. A "Grove" pairing line must not describe a seat by the water. The values differ by voice, as the table shows: YieldGiven voice 1 (Orin) has only the Winter group, and voice 2 (Fen) has Spring and Winter. Use the table, not a guess.

**Which slots travel.** Only `name` is in `localizedSlots`. A typed name needs no translation. The other slots carry English item names ("lily of the valley") and an English grammar, so a translated bank may not use them. The checker refuses any non-English line that uses a slot outside `localizedSlots`.

**SlotTranslator.** `CompanionLocale.SlotTranslator` is a hook for a translated value. Today the app passes no translator, so only `name` can appear in another language, and the checker refuses a bank that uses any other slot. Without a bank, a language falls back to the local village speech (architecture section 3). Do not add a translated slot until there is a translator, a native-speaker review, and a test.

## 4. How to check

Run from the worktree root.

```sh
# Shipped bank: every rule, and every kind in the rules file present (CI runs this)
python3 tools/evergrove_line_bank_check.py --complete

# Draft review files in docs/improvements/review/ (before the founder approves them)
python3 tools/evergrove_line_bank_check.py --review

# Warnings count as failures (use when you want a clean run)
python3 tools/evergrove_line_bank_check.py --complete --strict

# Coverage table after the checks: lines per voice, average and longest words
python3 tools/evergrove_line_bank_check.py --complete --report

# Checker's own tests
python3 -m unittest discover -s tools/tests
```

The C# tests read the same rules file (`Tests/LineBankTests.cs`). Run them after any bank change: `cd games/evergrove/Tests && DOTNET_CLI_HOME=/Volumes/SSD/dotnet-home /Volumes/SSD/dotnet/dotnet run -c Release`.

`--review` has no separate rules. It runs the same checks on the draft files, so a draft that passes it is ready for the founder, not for the bank.

**Review flow.**

1. Draft the lines in `docs/improvements/review/companion-lines-<Kind>.md`, one heading per voice (for example `## Mira (voice 0) · Rainy`).
2. Run `--review`. Fix every problem. Read the warnings.
3. Founder review. The founder approves or changes each line. Mark the file's status line with the date.
4. Move the approved lines into `games/evergrove/Assets/Resources/Evergrove/Lines/en.json`. Run `--complete --strict`, then the C# tests.
5. Only then does the review file become the record. It stays in the repository.

**Translators.**

1. `python3 tools/evergrove_line_bank_export.py --out PATH.csv` writes every English line as CSV (UTF-8 with a BOM), with the slots and a blank translation column.
2. The translator fills the translation column. Only `name` may appear as a slot.
3. `python3 tools/evergrove_line_bank_import.py --locale fr --csv PATH.csv` checks each row against the English bank. It refuses a changed English text, a bad slot, or a dropped `name`. It writes a DRAFT to `docs/improvements/review/fr/companion-lines-draft.md`. It never writes a JSON bank, and it refuses any out-dir under `games/evergrove/Assets`.
4. A native speaker reviews the draft. Only after that review may a bank go into Resources. Until then the language uses the fallback.

## 5. Warnings to decide today

The checker prints 20 warnings for `en.json` and no problems. They do not fail the run. The founder decides each one. The lines below are unchanged pending the founder's review.

1. **Near-duplicate, VisitorCame voice 2 (Fen), #4 and #14.** Both lines say a hedgehog comes at dusk, with a flower by a tree. Jaccard 0.77. Keep one, or make the second say something new.
2. **Opening-word clusters (19 warnings, more than 5 lines in a group and voice start with the same word).**
   - NewestThing: voice 0 and voice 2 start with "I" (6 each); voice 1 with "I" (8).
   - GiftWaiting: voice 0 "your" (6) and "the" (6); voice 2 "your" (8).
   - FruitGiven: "Thank" in voices 0 (7), 1 (7), 2 (9).
   - SolvedTogether voice 2: "You" (8).
   - NewArrival: voice 1 "A" (6); voice 2 "A" (8) and "Your" (7).
   - RiverClose voice 0: "The" (8).
   - VisitorCame: voice 0 "An" (6); voices 1 and 2 "A" (8 each).
   - WeatherWhileAway voice 0: "Since" (6).
   - RainComing voice 1: "Rain" (8).

   The rule is a warning only (CP23). Decide whether a cluster is a voice habit or a repetition the player will hear.
3. **Reads oddly, not flagged.** KindSolved, voice 0 (Mira), line 18: "The water's easier to listen to once {work} is done. Or perhaps that's just me." When `{work}` is "the water", it reads "…once the water is done." The checker passes it, because the rule checks slots, not values. Founder to decide: a rewrite, or accept it. The line is unchanged.

## 6. Not verified

- No native-speaker review of any non-English bank exists. The app passes no slot translator.
- No players aged 40 or over have judged the lines. Age fit is not validated.
- The checker tests form, not meaning. A passing line can still be untrue or awkward; read it aloud in its context.
