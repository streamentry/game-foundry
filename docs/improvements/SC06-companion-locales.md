# SC06: Locale plumbing

Smart companion, iteration 6 of 7 ([spec](../../games/evergrove/docs/specs/proposals/smart-companion-2026-11-27.md) §3, step 6).

## Player note

Nothing changes outside English today. In every other language the neighbours say the story's own lines, exactly as before. When a language gets a line bank reviewed by a native speaker, its neighbours will notice things in that language too.

## PM decision

- **No bank, no change.** A language with no `Lines/<locale>.json` keeps `LocalizedVillage`, as it always has. All 29 non-English languages are checked.
- **Only reviewed banks ship.** The spec says machine translation is a draft until a native speaker reviews it. So no translated bank was written into `Resources` in this iteration, and none ships. Drafts, when they exist, belong in `docs/improvements/review/<locale>/` and stay marked as drafts.
- **No English inside another language.** A bank for another language speaks only its own words: never an English fallback sentence, and only slots whose values need no translation, which today is just a typed family name. `{thing}` ("quince tree"), `{where}`, `{work}` and the weather phrase are English text, so a French line using them would put English words in a French sentence. Those kinds stay silent in that language until the slot values themselves are localized (a follow-up).

## Dev

- `Core/CompanionLocale.cs`:
  - `Speaks(locale, bank)`: English, or a non-empty bank.
  - `CanSay(locale, bank, fact, speaker)`: English is today's sentence or the bank; another language is the bank only, with localized slots only.
  - `Words(...)`: the composed words, or null, so the story line speaks.
- `EvergroveApp`: `CompanionPick` asks `Speaks`; the ranker's `canSay` and `CompanionWords` go through `CompanionLocale`. The English-only checks before SC06 are gone.
- Rules: `localizedSlots: ["name"]`. `tools/evergrove_line_bank_check.py` refuses any slot in a non-English bank other than those.

## Result

- `Tests/CompanionLocaleTests.cs`:
  - for every shipped non-English language (29), with no bank: the companion is silent, can't say any of a garden's facts, never returns an English sentence, and `LocalizedVillage` still speaks;
  - a French test bank speaks its own fruit and rain lines, fills a typed name, and refuses a line that would carry an English item name;
  - English without a bank says today's sentence.
- `dotnet run --project games/evergrove/Tests -c Release`: passed, 2,337,422 assertions on main after #145.
- Tours `c9p`, `c9l` and `c9x` in English: the neighbours speak the bank as before. A non-English tour wasn't run; the Core tests cover that path.

## Not verified

- No language other than English has a bank, so no translated line has been seen on screen.
- No physical device.
- No players aged 40+: age fit not validated.
