# Companion plus: 50 improvements to the smart companion

Follows SC01–SC07 ([spec](../../../games/evergrove/docs/specs/proposals/smart-companion-2026-11-27.md)). Each item is small, tested and committed on its own, one at a time, by a Haiku agent; the lead session checks each change before the next one starts.

## Ground rules

- **No new player-facing lines.** New wordings need the founder's review first (SC03 rule), so no item adds or edits a line in `en.json`. Items change how lines are chosen, checked, exported and tested.
- **Core rules test-first.** `dotnet run --project games/evergrove/Tests -c Release` must pass after every item.
- **Not verified without players:** none of these items has been seen by adults 40+. Age fit is `not validated`.

## Where the ideas come from

- **Valve's dynamic dialogue** (Elan Ruskin, GDC 2012, "AI-driven Dynamic Dialog through Fuzzy Pattern Matching"): facts and rules, the most specific rule wins, memory of what was said, cooldowns per concept and per speaker.
- **Supergiant's Hades**: priority tiers, essential lines first, ambient lines rarely, strong anti-repetition, and every line tied to something true that just happened.
- **Bark systems** in open-world games: who speaks is chosen as carefully as what is said, and the same speaker shouldn't hog the stage.
- **Ink and Yarn Spinner** (the dialogue-systems skill): stable line IDs from day one, validation before handing back, and walking every path with a script.
- **Localization practice**: give translators context (speaker, voice, slots, notes) and never assemble sentences from untranslated parts.
- **Game Accessibility Guidelines** and the studio's own rules: readable line lengths at 150% text, no pressure, and nothing a child co-player shouldn't read.
- **Our own playtest kit (SC07)**: the risks it names are that a waiting fact may nag, and that recall can't be checked without an export.

## The checklist

Status: `[ ]` to do, `[x]` done (commit noted).

### A. Memory and ranker robustness

- [x] **CP01 Corrupt memory never throws.** `CompanionMemory.Parse` skips malformed rows. A fuzz test over 1,000 random strings must never throw. Done: Already true; test added (CompanionMemoryTests: 1,000 seeded fuzz strings never throw, garbage rows dropped, valid row kept, round trip).
- [x] **CP02 Clock moved back.** A remark dated after today (the device clock was changed) is treated as said today, not dropped and not blocking forever. Once-facts stay said. Done: a future-dated remark is kept, re-dated to today, and counts as age 0 in the windows; tested in CompanionMemoryTests (clock-back) and CompanionRankerTests.
- [x] **CP03 Pruning keeps what was said.** Over `MaxEntries`, drop older repeats of a key before any key's only entry, so `EverSaid` survives the cap. Done: the cap drops older repeats first, then the oldest remaining, in With and Parse; tests cover a once-fact under 400 repeats, 350 distinct keys, and a capped round trip.
- [x] **CP04 Golden hash.** Pin `CompanionRanker.Hash` outputs for fixed inputs, so a refactor can't silently reshuffle tours. Done: GoldenHash test pins six Hash outputs (null, non-ASCII and 200-char keys) computed independently in Python, plus one Variant on an empty memory; Hash unchanged.
- [x] **CP05 Order independence.** `Allot` and `Pick` give the same result for a shuffled fact list. Done: already true; a test shuffles real garden facts (3, 9, 12 chapters, 40+ contexts) and a hand-made list with an anyone-fact and equal weights 50 times each, checking Allot and Pick with empty and heard memory.
- [x] **CP06 Memory helpers.** `LastSpoke(speaker)` and `HeardBy(key, speaker, day, days)` on `CompanionMemory`, with tests (used by CP07–CP09). Done: added LastSpoke, DaysHeardBy (distinct days, future-dated counts as today) and KindSaidOn to CompanionMemory, tested in CompanionMemoryTests (empty, window edges, same-day duplicates, other speakers ignored).
- [x] **CP07 Waiting facts don't nag.** A waiting fact one neighbour has said on two of the last three days loses weight for that neighbour (still eligible), so something new can be said; it comes back after a gap. This answers the playtest kit's named risk. Done: a waiting fact said on two of the last three days (not counting today) loses 40 weight for that neighbour, still eligible; fresh facts and other neighbours' history are exempt; tested in CompanionRankerTests.NagDamping.
- [x] **CP08 Speakers take turns.** An anyone-fact goes, on equal score, to the neighbour who spoke longest ago, before the hash tie-break. Done: an anyone-fact goes to the neighbour whose LastSpoke is oldest (never = MinValue), the hash only on a tie; tested in CompanionRankerTests.SpeakersTakeTurns (empty memory unchanged, oldest-silent takes it, recent speaker skipped).
- [x] **CP09 Kind cooldown.** A neighbour doesn't use the same fact kind on consecutive days unless it is waiting, fresh, or all they have (Valve's concept cooldown). Done: a same-kind remark yesterday costs KindCooldown (15) for that neighbour, not for waiting or fresh facts; tested in CompanionRankerTests.KindCooldown (nothing heard, penalty loses to a weaker other kind, penalty not a ban, waiting exempt, day 98 ignored).
- [x] **CP10 Explain.** `CompanionRanker.Explain(...)` returns a plain breakdown of each candidate's score and eligibility, for QA logs and tests. Done: Explain returns a best-first breakdown (score, base, new, own, nag, cooldown, eligibility) reusing the ranker's score; tested in CompanionRankerTests.ExplainLines (Pick agreement, exact line format, once-said, nag, cannot-say, top, null inputs).

### B. Simulation and properties

- [x] **CP11 Simulator.** A pure `CompanionSimulator` runs N days of visits over a world and returns a transcript (day, speaker, kind, key, variant, words). Done: `CompanionSimulator.Run` mirrors the app visit (ranker + locale words, one tap per neighbour per day) and is tested for completeness, determinism, read-only world and memory.
- [x] **CP12 No repeated wording.** In a 30-day simulation, no neighbour repeats the same words within 14 days. Done: a fixed sentence (no bank wording, variant -1) now waits VariantDays before it returns, which fixed 208 repeats of NewestThing; the 30-day simulation over four gardens checks 336 lines with none repeated within 14 days per speaker.
- [x] **CP13 Once is once.** Over 120 simulated days, no once-fact is said twice. Done: the simulator gives each context the garden map (0 of 358 lines now fall back to fixed wording, from 24 of 336), and once-facts are kept past the 60-day memory window so a 120-day run says none twice (new OnceIsOnce test, which failed before the fix).
- [x] **CP14 No starvation.** When a neighbour has something true to say, they say a companion line on at least 80% of simulated days. Done: new NoStarvation test in CompanionSimulatorTests (30 days, gardens 6/9/12, sayable = CompanionFacts.For with CompanionLocale.CanSay); every neighbour speaks on at least 93% of sayable days (lowest garden 9, neighbour 1, 28/30); no Core change.
- [x] **CP15 Fair sharing.** Each neighbour has at least 20% of all remarks over 30 simulated days, and shared remarks reach more than one neighbour. Done: the first measure (each gets 20% of *shared* remarks) contradicted the spec, where a shared remark goes to whoever has nothing better of their own; Mira usually has her own news. Measured: all remarks 90/88/90 of 268; shared 2/23/24 of 49.
- [x] **CP16 Fuzz the feed.** `CompanionFacts.For` never throws across 500 seeded random worlds and contexts. Done: CompanionFuzzTests (seed 1609) runs 500 random worlds/contexts, asserts no throw, keeper/key/weight/Once invariants; no Core fault found, so no code changed.
- [x] **CP17 The feed is pure.** Calling `For` twice gives equal lists and leaves the world's save unchanged. Done: CompanionFuzzTests FeedIsPure checks 100 seeded cases for equal feeds and unchanged save and memory text after For, Allot and the simulator; LetterNamesStayClean refuses sender names with '|' or a newline. No Core fault found, Core unchanged.
- [x] **CP18 Weather truth.** Weather facts and weather-qualified lines only appear for the day's actual weather. Done: already true; CompanionSimulatorTests.WeatherTruth checks facts' weather slot and key, simulated lines against their own WeatherToday group, and RainComing exactly on days before rain at home.
- [x] **CP19 Slot truth.** Every composed line in a simulation has no `{` left, and every slot it used is a non-empty value of its fact. Done: SlotTruth checks 1600 simulated lines over 60 days (3 gardens, 3 scenarios including a letter waiting and fruit given; 13 kinds) for no brace, tidy words, and every bank-line slot a non-empty verbatim value; 0 faults, no Core change.
- [x] **CP20 Performance budget.** Facts plus allotment for a large garden stay under 2 ms on average in Release. Done: CompanionPerfTests times 200 iterations of facts plus allotment on the largest garden (median 236.9 us, p95 380.1 us in Release); the median is asserted under 2 ms and the assertion is skipped with EVERGROVE_SKIP_TIMING=1.

### C. Line bank tooling

- [x] **CP21 Stable line IDs.** `LineBank.LineId(kind, voice, text)` gives a short, stable ID. A test checks they are unique across `en.json`. Done: `LineBank.LineId` (FNV-1a over group|voice|text, "cl_" plus 8 hex digits) and `LineBank.All()` added; LineBankTests pins one value and checks all 932 shipped English lines have unique IDs.
- [x] **CP22 Near-duplicates.** The checker flags two lines in one kind and voice that share most of their words. Done: checker prints near-duplicate (Jaccard >= 0.75, same group and voice) as warnings, failing only with --strict; 1 warning on the real bank; unit tests in tools/tests/test_evergrove_line_bank_check.py.
- [x] **CP23 Opening-word variety.** The checker warns when more than 5 of a voice's lines in one kind start with the same word. Done: the checker warns (CP23 `opening_words`, 19 warnings on the real bank, exit 0 and 0 problems with --complete) and six unit tests cover 6 vs 5, case, punctuation and slot tokens.
- [x] **CP24 Readable length.** The checker refuses lines over 28 words (readable at 150% text). Done: `maxWords` 28 in the rules JSON; the checker counts words after filling slots with their longest values and refuses over-limit lines (CP24 `check_line`, tests in `test_evergrove_line_bank_check.py`), mirrored in C# `Rules.Check`. The longest shipped line is exactly 28 words, so nothing is refused and no warning mode was needed.
- [ ] **CP25 Punctuation hygiene.** The checker refuses double spaces, leading or trailing spaces, mixed straight and curly quotes, and lines without closing punctuation.
- [ ] **CP26 No shared lines.** The checker refuses the same line used in two voices or two kinds.
- [ ] **CP27 Coverage report.** `--report` prints a markdown table of kinds × voices, with counts and average words.
- [ ] **CP28 Checker tests.** Python unit tests for the checker's rules, in `tools/tests`.
- [ ] **CP29 Translator export.** `tools/evergrove_line_bank_export.py` writes a CSV with ID, kind, qualifier, voice, English, slots and notes, so a native speaker has context.
- [ ] **CP30 Translation import check.** A tool reads a translated CSV, checks its slots against the rules, and writes a draft into `docs/improvements/review/<locale>/`, never into `Resources`.

### D. Localization and safety

- [ ] **CP31 Slot localizer hook.** `CompanionLocale` accepts an optional translator for slot values. With none, nothing changes. With one, a translated slot becomes sayable.
- [ ] **CP32 Counts read right.** No composed line says "1 things" or "0 …": `CompanionFacts.Words` and every count slot are checked for one and many.
- [ ] **CP33 A typed name is tidy.** A family name is trimmed, kept to 24 characters, and stripped of control characters and braces before it fills a slot.
- [ ] **CP34 No slot injection.** A name like `{thing}` is filled as text, never expanded.
- [ ] **CP35 An empty name isn't said.** A fact whose name slot is blank can't be said, so the neighbour moves to their next fact.
- [ ] **CP36 Memory per language.** Wording numbers belong to one bank, so a non-English bank keeps its own memory key; English keeps today's key.

### E. In the app

- [ ] **CP37 Facts once per rebuild.** The app computes the fact list once per frame, not once per neighbour.
- [ ] **CP38 Memory parsed once.** The app reuses the parsed memory while the stored string hasn't changed.
- [ ] **CP39 What the neighbours said, for a playtest.** `CompanionMemory.ToReadable(...)` plus a development-only control that copies it to the clipboard. Nothing leaves the phone unless the participant pastes it.
- [ ] **CP40 QA log explains.** In development builds, the `MOSSELORA_COMPANION` log adds the top three candidates from `Explain`.
- [ ] **CP41 A fresh garden forgets.** Starting a garden over clears that garden's companion memory.
- [ ] **CP42 The day turns at 4 a.m.** For the companion, a visit at 00:30 still belongs to the evening before, so night players aren't told a new day's news mid-evening.

### F. Process and docs

- [ ] **CP43 Dead code.** Remove companion helpers nothing calls since SC06.
- [ ] **CP44 CI runs the checker.** `ci.yml` runs `tools/evergrove_line_bank_check.py --complete` on every PR.
- [ ] **CP45 CI runs the checker's tests.** The Python test discovery covers CP28, and CI is shown to run it.
- [ ] **CP46 Sample transcript.** A tool writes a 30-day transcript of one scripted garden to `docs/improvements/companion-plus/sample-transcript.md`, for reading the lines in context.
- [ ] **CP47 Architecture page.** One page on the feed, ranker, bank and locale, with every constant and where it lives.
- [ ] **CP48 Writing guide.** How to write a line: voices, qualifiers, slots, banned words and how to run the checker, drawn from the rules file. No new lines.
- [ ] **CP49 Playtest kit update.** Add the export (CP39), the nag damping (CP07) and the transcript pre-check (CP46).
- [ ] **CP50 Decision log and final check.** A decision-log entry, then tests, docs checks, a Unity build and the three tour layouts.
