# Research and shipping decisions

## 2026-11-15 — Engine and approach

**Decision:** Godot 4.7.2 + GDScript, separate exportable game projects. First feature is a 4×4 Takuzu **engineering** sample, not a market launch.

**What we know:** Godot 4.7.2 is listed as stable in the official release archive; public App Store listings demonstrate existence of multiple niche puzzle categories; Apple Ads documents a relative search-term popularity API.

**Unknown:** actual search popularity for candidate keywords, top-10 organic competition, review gaps and commercial viability.

**Why:** open-source independence, portable reproducible logic tests, 2D suitability. Deployment/privacy SDK maturity on native iOS/Android still needs verification.

**Opposing case:** Flutter + Flame can be easier for word/UI games, while commercial engines may have more ready-made mobile monetization. We will not add multiple engines without a tested economic benefit.

**Stop condition:** if native export, accessibility or sustainable distribution proves impractical, reconsider engine/segment before scaling.

**Next observable action:** authenticate Apple Ads insights (if accessible), collect country-specific monthly data for `screen_first` queries and audit top-10 storefront results. Do not build a game backlog without measurement.

## Template

```text
Date / candidate:
Decision: research | prototype | ship | park | reject
Player & search intent:
Evidence: primary links, country, date, confidence
Strongest alternative explanation:
Assumptions:
Cheapest falsification:
Action within seven days:
Continue / stop thresholds:
Actual outcome & model update:
```

## 2026-11-15 — Make search discovery source-first

**Decision:** Add genre-first, pattern-first and exact-term research modes to Apple's authenticated insights collector. Preserve per-country provenance, full pagination vs truncated top-K coverage, and treat missing niche keywords as **unknown**, not unpopular.

**Why:** The prior 100-row catalog was selected by mechanic preference. Apple data should determine which search intents are real. Catalog API result order is not an organic ranking and rating counts are not demand estimates.

**Strongest counterargument:** Apple's search term popularity covers only popular terms, so its results can omit profitable small niches and branded terms may dominate. This is a *candidate discovery source*, not a definitive market sizing instrument.

**Stop condition:** If the Apple Ads source cannot produce enough specialist keywords, sample public storefront search suggestions/ASO sources and test organic acquisition experimentally; do not infer absent demand as zero.

**Next real action:** Obtain a valid Apple Ads API token via an authorized account, run a read-only monthly genre discovery, then audit the top observed non-branded queries.

## 2026-11-15 — Storefront audit evidence pack

**Decision:** Add a structured audit template and a fail-closed validator for issue #3. Do not add result or review observations until they are captured from the target storefront on a physical device.

**What changed:** The validator requires ten distinct unsponsored result positions, the physical device/storefront and capture timestamp, listing metadata and local screenshots, a mixed-sentiment review sample or a stated sampling limit, and a wedge/no-wedge finding with a falsifier.

**Evidence status:** The measurement worksheet remains header-only with 0 measured popularity rows. No organic ranks, review themes, or wedge claims were created. The tool checks structure and file paths; a human still needs to compare entries with source screenshots and App Store pages.

**Opposing case:** A detailed pack adds capture effort and cannot prevent a person from entering false ranks. Its value is the audit trail and missing-field gate, not automated proof.

**Cheapest falsification:** Complete one audit pack from a real device for a measured non-branded query; check whether the template captures the evidence needed to decide continue or no-wedge without rework.

**Next real action:** First obtain a measured keyword from the authorized Apple Ads collection in issue #2, then capture its ten organic results and reviews on a controlled physical device. Until then, keep issue #3's opportunity score unranked.

## 2026-11-15 — Mobile export preflight

**Decision:** Keep Takuzu Lab SDK-free. Treat Godot 4.7.2 mobile distribution as unverified until a signed build runs on physical iOS and Android devices.

**Evidence:** Xcode 27.2 and iOS 27.2 SDKs are installed. The official Godot 4.7.2 binary was run from temporary storage; headless import and rules checks passed. Device discovery found 11 shutdown iOS simulators and no physical iPhone; `adb devices -l` found no Android device. The Android SDK link resolves to a directory with Platform Tools but no Platform or Build Tools.

**Static baseline:** The Godot 4.7.2 Android build config sets min SDK 24, compile/target SDK 36, Build Tools 36.1.0 and Java 17; the Android build detector pins NDK 29.0.14206865. API 36 meets Google Play's current new-app/update target requirement. See [mobile export and SDK assessment](../mobile-validation.md) for the exact template, plugin review, and commands.

**Opposing case:** Godot's matching export templates and Xcode are available for download/installation, so a small debug build may be straightforward once the missing environment and device access are restored. That does not prove signing, launch, touch handling, pause/resume, offline behavior or low-end performance.

**Unknown:** Successful export/install, the generated iOS deployment target, connected device identities, physical runtime behavior and any SDK-specific compatibility/privacy issues.

**Cheapest falsification:** Install the standard Godot 4.7.2 editor and matching templates, then export and launch the current sample on one physical iPhone and one Android device with local test signing.

**Next real action:** Restore access to both physical devices and the Android SDK build packages, then complete the device workflow in `docs/mobile-validation.md`. Do not add an SDK until a production game's data or monetization needs justify it.

**Checks from the 2026-11-15 machine snapshot (not current repository totals):** Candidate validation passed for 100 hypotheses and 15 public listing anchors, with 0 measured popularity rows at that time. The 28 Python unit tests passed. Godot 4.7.2 headless import passed; all 8 Takuzu rules checks passed. The editor remains absent from `PATH`; the temporary official binary was used explicitly.

**Other blockers observed on 2026-11-15:** Apple Ads token/account variables were absent in the preflight environment, so authenticated collection was not run in that session. `measurements.csv` then had only its header. **Later 2026-11-17 Apple Ads research supersedes the claim that no demand observations have ever been collected; see the subsequent dated decision entry.** No keyword or rank evidence was inferred from this machine snapshot.

## 2026-11-17 — Read-only demand scan; no game selected

**Decision:** Keep the first release unselected. Apple Ads now supplies measured search signals, but the organic competition and unmet need gates are still open.

**Evidence:** A read-only API user returned 2,500 genre observations for October 2026 across US, GB, AU, DE and JP; all five country queries were marked truncated to top-K. Exact searches for the 100 brainstormed candidates returned no rows, which remains **unknown**, not zero. Contains searches returned 113 observations for 40 queries; six localized Japanese patterns returned 12 more. The ten measured query/storefront pairs to audit are recorded in [the capture queue](storefront-capture-queue-2026-11-17.csv), with source snapshot hashes and no scores.

**Competition signal:** A separate public catalog scan found large, recently updated listings in Sudoku, Mahjong, color-by-number, word search, block puzzle, jigsaw and nonogram. The catalog API is not official organic rank, and ratings are not downloads or revenue. The findings justify a competition check, not a conclusion that a category is open or closed.

**Why no game is chosen:** No physical-device organic result set or balanced recent-review sample has been captured. Without those, we cannot support a product wedge, acquisition path or Apple 4.3 distinctness decision. `Takuzu Lab` remains an engineering reference.

**Engineering correction:** The unscored opportunity exporter now keeps distinct Unicode query terms such as `fussball` and `fußball` separate. The full research suite passes 42 tests.

**Next real action:** Capture the ten queued keyword/storefront pairs on controlled physical devices, record ten unsponsored results and a mixed review sample for each, then decide `wedge` or `no wedge`. Do not start a game-specific production project before that review.

## 2026-11-17 — Vietnam cờ tướng offline as a follow-up audit lead

**Decision:** Prioritize one Vietnam storefront audit for `cờ tướng offline` before building a game. This is an audit lead, not a product selection.

**Observed demand:** October Apple Ads reports `cờ tướng` at 83/100 within Games (genre rank 19) and `cờ tướng offline` at 61/100 (rank 472). The score is relative, not volume. Search patterns for `cờ thế`, `tàn cuộc`, `khai cuộc`, `học cờ tướng` and `luyện cờ tướng` returned no rows; demand for those sub-intents remains unknown.

**Observed supply:** Haga already offers offline play, thousands of board puzzles, AI and hints, has 12K ratings at 4.8, and sells a remove-ads purchase. Ziga has a large online community. Visible ad complaints are a lead for review sampling, not proof of frequency. Details and the proposed falsification threshold are in [the VN niche hypothesis](vn-niche-hypothesis-2026-11-17.md).

**Next real action:** Capture the top ten unsponsored Vietnam App Store results for the exact offline query and a balanced recent-review sample for Haga and the top two direct alternatives. Continue only if a distinct training need repeats and current listings do not already solve it.

## 2026-11-17 — Vietnam cờ tướng offline: audit lead only

**Decision:** Add offline cờ tướng tactics training as a Vietnam storefront audit hypothesis, not as the first game selection.

**Evidence:** October Apple Ads data reports `cờ tướng` at 83/100 within Games and `cờ tướng offline` at 61/100. The public catalog sample shows Haga with 12K ratings, 4.8 stars, offline puzzles, AI and replay; Ziga has 130K ratings and online matchmaking. Haga's listing shows a remove-ads purchase. A few visible recent reviews complain about ads, but the sample is too small to infer how common the issue is. See [VN niche hypothesis](vn-niche-hypothesis-2026-11-17.md).

**Strongest counterargument:** Haga already offers offline board puzzles and AI with a high rating. A clone or an ad-free claim alone is unlikely to be a sufficient wedge.

**Unknown:** Organic App Store positions, review frequency, absolute search volume, willingness to pay, and whether a structured training loop is missing.

**Next real action:** Audit the top ten unsponsored results for `cờ tướng offline` on a physical Vietnam storefront device and sample 20 recent positive and negative reviews from the top three relevant apps. Use the predeclared screening rule in the hypothesis note before prototyping.

## 2026-11-21 — Google Keyword Planner: promote Nối Từ to audit lead

**Decision:** Prioritize Vietnamese Nối Từ for the next market audit; do not start production. This replaces cờ tướng offline as the first hypothesis to test, while retaining cờ tướng as an alternative.

**Observed signal:** Google Keyword Planner showed `nối từ` at 10K–100K average monthly searches in Vietnam, with low Google Ads advertiser competition. The App Store catalog includes direct word-chain games; visible reviews across multiple titles report word-validation errors. Google web-search demand is not App Store install demand, and its competition column is not App Store competition.

**Strongest counterargument:** The direct App Store game with nearly 3K ratings already implements the core loop. Review excerpts are small and partly old. A reliable, licensed Vietnamese phrase list is a separate risk; candidate datasets carry GPL, CC BY-SA/GFDL, mixed, or unclear source terms.

**Next real action:** Audit the physical VN App Store top ten for `nối từ`, sample 20 recent reviews across the top three direct apps, and verify a commercial-compatible lexicon against 200 human-adjudicated chains. The continue/stop rule is in [the GKP niche scan](gkp-niche-scan-vn-2026-11-21.md). Do not build a release candidate until these gates pass.

## 2026-11-21 — Expand demand screen to the top 30 language groups

**Decision:** Expand discovery to 30 language-market pairs before choosing the first game. Keep Vietnamese Nối Từ as one local lead; it is not the global selection.

**Evidence and limit:** The roster uses Ethnologue's 2026 total-speaker ordering as a population screen. It does not measure game demand, mobile-store reach, or revenue. Only the Vietnam Google Keyword Planner observations are measured so far; all other language-market demand remains unknown. Draft seed phrases and one market anchor per language are in [the language screen](gkp-top-30-language-screen-2026-11-21.md) and [its query queue](gkp-top-30-language-seeds-2026-11-21.csv).

**Strongest counterargument:** Speaker count is a poor substitute for reachable game users. Chinese, Arabic, Punjabi, Nigerian Pidgin and multilingual Indian markets cannot be cleanly measured with one language filter or one country. Broad keyword estimates may also capture non-app intent.

**Next real action:** Complete a common Google Keyword Planner screen for the 30 market-language pairs, then select at most three candidates for physical storefront and balanced review audits. Do not infer a wedge from Google Ads competition.

## 2026-11-21 — Set adults 45+ as the primary game audience

**Decision:** Focus future Game Foundry research, prototypes and products on adults aged 45 and older. This defines the audience; it does not select a game or establish age-specific demand.

**Evidence and limit:** This is the user's explicit product direction. Existing keyword, language-speaker and storefront evidence is not age-segmented, so fit for adults 45+ remains unknown. The updated audience rules are in [AGENTS.md](../../AGENTS.md); the current language screen now states this limitation.

**Strongest counterargument:** “45+” includes distinct life stages and player needs. A single age label can encourage stereotypes and produce a broad product with no clear job to serve.

**Next real action:** For each candidate that clears broad demand screening, identify the specific 45+ player segment and validate the need, controls, readability, pacing and game appeal with people in that segment before production.

## 2026-11-21 — Focus 45+ strategy on familiar games and a 30-keyword screen

**Decision:** Keep adults 45+ as the primary audience; the first segment to test is U.S. adults 50–75. AARP's available survey is an aggregate for adults 50+ and does not isolate this age range. Prioritize familiar puzzle, word, card, board, and traditional mechanics only where search demand and a repeatable experience gap coexist. Compare 30 queries across adult-oriented familiar puzzles, regional/traditional games, and specialized logic puzzles before selecting at most three storefront audits.

**Evidence and limits:** AARP's U.S. 50+ survey finds substantial play, smartphone use, mostly solo play, and relaxation/mental-sharpness motivations. Its 2023 work reports design and ad frustrations. Neither source identifies App Store keyword volume or proves willingness to install a new game. Public U.S. catalog listings show direct large-print/no-ad products already exist. The new market report separates these facts from the open question of organic acquisition.

**Strongest counterargument:** Familiar categories attract large, established competitors. Accessibility and “no ads” are already product features in direct listings, while broad category growth does not isolate older users or a new entrant's chance to rank.

**Market screen result:** The U.S. group measured `large print word search` at 1K–10K; `solitaire no ads`, `solitaire for seniors`, `nonogram puzzle`, `kakuro puzzle`, and `shikaku puzzle` also measured 1K–10K. Vietnam measured `nối từ` at 10K–100K and `cờ tướng online` at 100K–1M. All figures are Google web searches, not app-store installs. Store evidence shows large incumbents and products that already provide the proposed accessibility/no-ad features. A Vietnamese Android Nối Từ competitor with 50K+ downloads states it expanded its dictionary after false-rejection complaints. The audit shortlist is `large print word search`, `nối từ`, and `shikaku puzzle`; no game is selected.

**Next real action:** Capture the physical top ten unsponsored results and a balanced recent-review sample for the three audit candidates. Device discovery currently times out for iOS and ADB does not return a device list, so this gate is pending a connected phone. Test the strongest candidate with adults 50+ in the intended player segment before prototyping. Keep 1,000 non-branded organic installs in 60 days as a proposed experiment goal, not a forecast, and define the Day-7 retention gate before launch.

## 2026-11-21 — Five research priorities

**Decision:** Shortlist Nối Từ, large-print word search, Shikaku, Ô Ăn Quan, and Kakuro for further validation, in that order. This is a research ranking, not a production decision. The first three retain priority for physical storefront audits; the remaining two are backups. See [the five-idea comparison](top-five-ideas-2026-11-21.md) for counterarguments and stop conditions. Organic ranking, age-specific demand and monetization remain unknown.

## 2026-11-21 — Global markets for three selected concepts

**Decision:** At the user's direction, focus further research on large-print word search, Shikaku and Kakuro in major global markets. The earlier five-idea ranking is superseded. Begin with U.S., U.K., Canada, Australia, Japan, Germany, France and South Korea as country-specific research candidates; none is a proven launch market. See [global market priorities](global-puzzle-market-priorities-2026-11-21.md).

**Counterargument:** Broad mobile spending does not prove demand for these mechanics among adults 45+; major markets may have strong incumbents and expensive localization. Existing apps already offer many proposed features.

**Next action:** Measure localized search terms and audit organic results and balanced reviews for each market–game pair. Select one pair only after demand, player need, distribution and content rights are supported.

## 2026-11-21 — Choose Kakuro as the first build candidate

**Decision:** Start with an English Kakuro concept for U.S. adults aged 50–75. The U.K. is the English follow-up and Japan the first localization candidate. Pick Shikaku as the fallback if the player test fails.

**Evidence:** Kakuro reached 1K–10K monthly Google searches in the U.S., U.K. and Japan, with the same numeric grids usable in each language. Mature apps already offer tutorials, zoom and hints. The [first-game decision](first-game-decision-2026-11-21.md) records the country coverage, the proposed guided first solve and the conditions for moving into production.

## 2026-11-21 — Kakuro global ASO supply correction

**Decision:** Keep Kakuro as the first **gated** U.S. 50-75 proof-of-play candidate, but **withdraw any assumption that explainable hints or large numbers are unique differentiators**. Production stays blocked by search, competitor and head-to-head player evidence; Shikaku remains the fallback.

**New evidence:** [Kakuro Grid in France](https://apps.apple.com/fr/app/id6817269735) already advertises next-deduction explanations; [Conceptis](https://apps.apple.com/us/app/id1197071375) has an Assistant and strong existing ratings; [Zenkuro in Germany](https://apps.apple.com/de/app/id6787019507) pitches calm generated puzzles. Japan's [Conceptis listing](https://apps.apple.com/jp/app/id1197071375) uses the title `加算パズル`, not `カックロ`. Public listing descriptions and rating counts are **not** physical App Store ranks, actual installs, tested product quality or 45+ fit.

**Revised wedge to falsify:** visible intersecting sums + transparent candidate elimination without zoom/pan. Test against incumbents and newer explainable-hint competitors before writing production code. Measure `カックロ`, `加算パズル` and equivalent local native queries separately. The 2025-11 to 2026-10 GKP 1K-10K bands for U.S., U.K. and Japan are **Google Search** estimates only. No verified ASO-only acquisition gap has been established.

**Cheapest next action:** acquire official Apple popularity, capture ten unsponsored results for U.S. `kakuro`/`kakuro puzzle`, and compare a tiny intersection-clue prototype against existing apps with five target players. Park on the predeclared gate if the problem does not reproduce. [Full dossier](kakuro-global-aso-45plus-2026-11-21.md); [query queue](kakuro-aso-keyword-queue-2026-11-21.csv).

## 2026-11-21 — Document three global visual playtest hypotheses

**Decision:** At the user's request, document **Good Eye → Quiet Shot → One Path** as the exploratory playtest order for a simple global game serving adults 45+. The ranking buys information about three different experiences: estimation, trajectory prediction and route planning. It is not a measured ASO ranking or a production approval. Kakuro remains the conditional first candidate in the search-led track.

**Evidence:** AARP's October 2026 U.S. 50+ study supports testing varied motivations and comfortable solo play, not any particular mechanic or worldwide age fit. Direct listings already cover 50/50 cutting, shared daily challenges, one-shot board clearing and one-stroke graph puzzles. Public cumulative download bands demonstrate historical reach of some competitors; they do not measure current organic demand, profitability or 45+ adoption. Sources and limits are in [the strategy and three game briefs](global-45plus-top-three-games-2026-11-21.md).

**Strongest counterargument:** Good Eye may be a brief novelty; Quiet Shot may reward fine motor precision more than planning; One Path may feel familiar and repetitive. Comfortable controls and daily sharing are not established differentiators. A low-cost prototype can still distract from the harder work of reaching and retaining players.

**Assumptions:** people voluntarily replay, can control the intended action fairly, return without reminders and sometimes introduce another player. Repeat use and referrals remain separate from willingness to pay. A primarily referral-led commercial route would require its own recorded distribution decision; the existing ASO production gate is not waived.

**Cheapest falsification / seven-day action:** compare the Good Eye interaction against an existing 50/50 game on target players' own phones, fix one clear input problem, then decide whether to run a fresh 30-person cohort. Proposed cohort gates are 24/30 unaided first attempts, 18/30 voluntarily completing five cuts, 9/30 next-day return, 6/30 day-three return and three additional referred players completing an attempt. These are internal spending gates, not benchmarks. Observe comparative preference as well. Participant day-seven return must be measured later than a project-day-seven review when recruitment starts on day 2 or 3.

**Continue / stop:** good early replay, return and referrals justify a larger independent cohort and a second market. Persistent control problems or boredom after one deliberate revision park the mechanic. Good retention without referrals requires acquisition work before expansion. Test subsequent mechanics sequentially and predeclare comparable engagement measures.

**Actual outcome:** documentation completed; no target-player results, keyword measurements, new production projects or release claims are added by this decision.
