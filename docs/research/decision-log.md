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


## 2026-11-21 — Implement Good Eye as an original first-play experiment

**Decision:** At user direction, implement the first ranked global visual experiment, **Good Eye**, as a separate Godot 4.7.2/GDScript project under `games/good-eye/`. This is a player-research prototype, not a production greenlight, store release or override of the Kakuro ASO evidence gates.

**What is implemented:** deterministic convex shapes; a draggable/rotatable line with large precision controls; two true clipped areas and complementary percentage/score feedback; freely retryable practice; five UTC daily first attempts locked locally; resume through a versioned on-device JSON save; seed-based web links when hosted; independent design brief; pure geometry/round-state tests, scene smoke tests and web-artifact CI. No SDK, backend, account or forced timer.

**Why:** the fastest inspectable falsification of the proposal that adults 45+ will voluntarily make and repeat an understandable visual judgment. The pure geometric scoring prevents the biggest avoidable product error: scoring a silhouette by its bounding box.

**Still unknown:** age-specific comprehension, comparative preference, physical-device controls, real referral activation, return, Android/iOS exports, ASO acquisition and willingness to pay. A successful software test cannot validate any of these.

**Next real action:** play against an existing 50/50 competitor on actual target players' phones. Apply the predeclared [playtest gates](global-45plus-top-three-games-2026-11-21.md) before expanding content or beginning a new game. Keep the search-led Kakuro decision conditional.


## 2026-11-21 — Quiet Shot original playtest prototype

**Decision:** Implement the second visual-mechanic hypothesis as an independent Godot 4.7.2/GDScript prototype. The player's decision is a single aim; fixed mirrors reflect the ball, round targets clear on contact, and failed shots show exactly where the ray traveled.

**Evidence standard:** eight authored arrangements with targets placed along known physical trajectories are verified by deterministic tests. This verifies rule correctness and baseline solvability, *not* a commercially appealing difficulty progression. No App Store demand, 45+ retention, preferences against Okay?, or referral activation has been measured.

**Engineering scope:** correct ray/segment intersections, two-dimensional reflection, target-circle entry, a four-reflection budget, trace drawing, angle adjustments, repeatable retries, progression, local JSON save/recovery, deep-linked challenge level, independent design brief, smoke tests and downloadable web artifact. No backend, ads, analytics SDK or native-signed builds.

**Strongest objection:** mirrors may require too precise an angle or feel indistinguishable from established one-shot games. The current automated tolerance test checks only narrow perturbations and cannot replace actual phone usability testing.

**Next real action:** observe 6–10 target players attempt a few original boards, note input-vs-reasoning failures, compare against an established competitor and apply the stop conditions in [the global visual experiment note](global-45plus-top-three-games-2026-11-21.md). Good Eye and the earlier conditional Kakuro search track remain unchanged.


## 2026-11-21 — One Path original playtest prototype

**Decision:** Implement the third global-visual hypothesis as an **independent** Godot 4.7.2 Euler-trail game. Eight original small graphs, including a deliberate drawing crossing that has no vertex, demonstrate route planning with large touch targets, drag/tap and letter-button alternatives.

**Core rules:** every edge must be traversed exactly once; revisiting vertices is allowed; an actual node marks the only legal junction. The pure engine validates connected 0/2-odd Eulerian levels, rejects repeated edges, understands partial/dead-end paths, and accepts more than one valid completed trail where appropriate.

**Scope:** game scenes and individual design spec, guided start markers, undo/restart, saved local progress with validation, simple browser level links when hosted, unit and scene tests and downloadable WebGL2 artifacts. No external backend, ad SDK, account or health claims.

**Unverified:** target-audience comprehension, native device usability and screen-reader parity, present competitor gaps, organic acquisition, return and willingness to pay. Code validity does not establish differentiation.

**Next real action:** comparative real-device playtests with 6–10 adults in appropriate age bands, including tap-versus-drag preference and confusion about crossings. Record failures and park if the rule remains unclear after one presentation revision. Do not infer a global hit from the existence of eight working levels.


## 2026-11-22 — Shikaku Garden as an additional small logic playtest

**User instruction:** continue building game prototypes beyond the first three; implementation does not invalidate the market-first production policy. Shikaku is the documented fallback to the Kakuro search-led product hypothesis, not a proven acquisition winner.

**Built:** original standalone Godot 4.7.2 project, 8 authored 4×4–6×6 rectangle partitions, corner-to-corner selection, no timers, undo/restart, persisted local progress, room-by-room feedback and level-based web challenge links when hosted. Deterministic exact-cover solver checks that every board has exactly one solution; placement rules validate area, clue count, bounds and overlaps. Game has an independent design identity and artifact workflow.

**Unknown:** 45+ usability, retention, differential value versus current Shikaku competitors, official country-specific Apple keyword popularity and economics. None of this is inferred from code or puzzle-solver tests.

**Most useful next falsification:** compare the first five original puzzles with an existing Shikaku title on real target phones; observe misunderstandings, voluntary extra play, and reason to choose this one. Stop after one presentation correction if no comparative preference or return signal emerges.


## 2026-11-22 — Odd Bloom, first-look visual-difference playtest

**Decision:** At the user's request, implement an additional fast-feedback casual visual experiment, independently branded and isolated from existing game projects. This is *exploratory implementation*, not approval to publish, monetize or claim a market advantage.

**Prototype:** 12 deterministic boards with 6–16 original vector-drawn flower cards each, exactly one structural outlier per round. Differences concern petal count, center ring, branch count, petal geometry or center mark, never hue alone. One-tap selection, short informative feedback after incorrect taps, optional same-round replay, guarded progression, local save validation and seed-reproducible web challenge links if hosted. Dedicated Godot tests, custom design, WebGL2 build artifact.

**Strongest opposing argument:** many visual odd-one-out games exist, and a first look may not produce return. Bigger card sets can measure eyestrain or motor precision more than enjoyment. Showing a working algorithm does not validate these mechanics or global 45+ demand.

**Cheapest falsification:** compare Odd Bloom with an existing odd-one-out title using 6–10 adult puzzle players on their own phones. Watch voluntary play after five rounds, confusion or strain on the 4×4 card view, and independent next-day return. Park if it is merely a one-time novelty after one specific design revision.

**Status:** No actual target-player data, App Store keyword volume, retention, referral activation, device QA or monetization evidence exists.


## 2026-11-22 — Kakuro Compass, sixth game prototype

**User-directed scope:** build additional standalone game. Choose the already-selected GF-001 Kakuro **proof of play** rather than inventing a seventh unmeasured niche. This does **not** waive the existing demand, competitor or device gates.

**Implementation:** original, independent Godot 4.7.2 Kakuro Compass; eight authored small 5×5 clue boards, unique-solution verification, true 1–9 across/down sum/no-repeat constraints, a *persistent intersection inspector* showing feasible values from each local run and their intersection; 69px editable cells, large 3×3 keypad, undo/erase/restart, local validated save/progression, hosted level links and WebGL2 export with HTTP asset tests.

**Strongest opposition:** established Kakuro titles already advertise hints, zoom and tutorials. A local constraint inspector may provide no real advantage and might reduce the challenge too much. The small boards prove rules, not sustained content depth. These are hypotheses, not measured outcomes.

**Cheapest falsification:** recruit 5 US adults 50–75 who actually enjoy Kakuro; alternate Kakuro Compass and a mature incumbent, watch the first 5 minutes on their phones. The pre-existing go/no-go standard is in [first-game-decision-2026-11-21.md](first-game-decision-2026-11-21.md). Record whether they correctly explain a crossing deduction unaided, not merely whether a test script passes. If the inspector brings no comparative preference after one UX correction, park this project rather than producing 100 levels.

**Status:** app-store organic keyword positions, segment-specific demand, return and monetization are unverified. No app store submission or public hosting.


## 2026-11-22 — Quiet Camp, seventh independent game prototype

**User instruction:** continue building after the first six playable experiments. Choose GF-006 Tents & Trees from the existing, unvalidated search-led discovery catalog as a visually distinct contrast to numeric Kakuro.

**Built:** a standalone Godot 4.7.2 project with eight original 5×5/6×6 forest clearings and one distinct valid tent set per board, as verified by an actual backtracking solution counter. The engine checks row and column counts, no contact between tents (including diagonal), and **one-to-one orthogonal matching between the trees and tents**. A tent can be adjacent to multiple trees as long as a valid pairing exists. The solver does not compare guesses to a hardcoded fixture. Original vector trees/tents, tap-to-cycle tent/grass/blank, accessible semantic labels, undo/restart, local validated save, deterministic level links when hosted, and WebGL2 export with HTTP asset tests.

**Strongest counterargument:** existing Tents & Trees apps provide this same generic mechanic, so distinct vector art and comfortable buttons do not prove a product wedge. The prototype's eight small boards are not a commercial puzzle library. Its actual player appeal and difficulty are unknown.

**Cheapest falsification:** on a handful of independent adults 45+ who play logic games, compare an incumbent Tents & Trees title and Quiet Camp on their own phones. Record whether they understand diagonal contact, whether row/column totals are legible without zoom, if they voluntarily attempt more boards, and which version they independently return to. If no comparative usability or preference signal survives one UX revision, park it.

**Status:** engineering prototype only. No age-specific demand, official App Store keyword popularity, installs, native signed builds, day-1/day-7 return, referrals, purchases or profit measured.


## 2026-11-22 — Five additional isolated logic experiments, no production approval

- **8. Futoshiki Terrace (GF-004):** Fill a 4×4 Latin grid using 1–4 exactly once in each row and column, while satisfying directional inequalities. **Evidence:** One valid solution per authored 4×4 board; no row/column duplicates; all inequalities true. **Critical counterargument:** Incumbent Futoshiki apps already cover the same rules; an inspector could be unnecessary. **Not measured:** official search intent, adult-45+ preference, return, device QA or monetization.
- **9. Star Atlas (GF-005):** Place exactly one star in each row, column and labeled connected region, without allowing stars to touch even diagonally. **Evidence:** All three independent regions are connected; each variant has one legal constellation; no diagonal contact. **Critical counterargument:** Rotations are equivalent layouts, and a three-base-board catalog is too small for sustained play. **Not measured:** official search intent, adult-45+ preference, return, device QA or monetization.
- **10. Bridge Isles (GF-007):** Connect visible neighboring numbered islands with zero, one or two straight bridges, without crossings, and keep the network connected. **Evidence:** Exact island degrees, no crossings, global connectivity, each authored board has one bridge-strength solution. **Critical counterargument:** Hashi is mature, and rendering bridges is not a defensible distribution advantage. **Not measured:** official search intent, adult-45+ preference, return, device QA or monetization.
- **11. Loop Lantern (GF-008):** Draw a single closed nonbranching Slitherlink loop using cell clues that specify how many surrounding edges are selected. **Evidence:** Unique single-loop solution per clue board, exact local counts, degree 0/2 vertices, one connected component. **Critical counterargument:** The discrete side controls may feel slower than direct tracing, and classic competitors already exist. **Not measured:** official search intent, adult-45+ preference, return, device QA or monetization.
- **12. Hitori Grove (GF-009):** Shade repeated numbers, ensuring no horizontally or vertically adjacent shaded squares and one connected group of unshaded squares. **Evidence:** Unique shading solution per original puzzle; no same-value unshaded row/column duplicates; unshaded connected. **Critical counterargument:** 4×4 grids and aesthetic polish are not enough to stand out in a familiar puzzle category. **Not measured:** official search intent, adult-45+ preference, return, device QA or monetization.

**Decision:** keep all five as user-requested research prototypes. Their Web artifacts must pass actual HTTP delivery tests; code checks do not replace age-specific real-player comparison or commercial acquisition screening. Stop adding games until experimental differences are tested and documented. See [the five-game research brief](next-five-game-experiments-2026-11-22.md).


## 2026-11-22 — Five additional mathematical-playtest prototypes (13–17)

At user direction, implement five new independent Godot vertical slices without altering the ASO-based conditional Kakuro production decision. These are research demonstrations, not evidence of user need, superior distribution or paying demand.

- **13: Nurikabe Bay (GF-010).** Classify every square as sea or island. Numbered islands have exact sizes; each island contains one clue, the sea is connected, and there can be no 2×2 all-sea block. **Countercase:** The genre is established; small grids and pretty shores are not a reason to switch or return.
- **14: Hidato Walk (GF-013).** Place numbers 1–16 exactly once so every consecutive pair touches horizontally, vertically or diagonally, while preserving given numbers. **Countercase:** Sixteen buttons plus sixteen cells may require scrolling on small screens; only actual device observation can confirm comfort.
- **15: Skyline Steps (GF-019).** Fill tower heights 1–4 without repeating any height in a row or column. Perimeter numbers count how many buildings are visible from that direction. **Countercase:** This resembles classic Sudoku-style input; a small 4×4 version may not reveal a meaningful commercial wedge.
- **16: Thermometer Studio (GF-021).** Fill liquid from each tube's bulb toward its tip only. Row and column totals constrain all eight disjoint two-cell thermometers. **Countercase:** Two-cell thermometers are an intentionally simplified mechanic; real puzzles may need varied tube lengths to stay interesting.
- **17: Cage Hearth (GF-023).** Fill 1–4 once per row and column while solving connected cages labeled with sums, products, differences, exact quotients or single values. **Countercase:** Arithmetic puzzles have incumbents and may overlap the studio's Futoshiki/Kakuro audiences; a new title alone adds no differentiated distribution.

All five must pass deterministic solution-count, local save/recovery and real-scene playthrough tests plus independent WebGL2 and localhost HTTP asset export before merge. No physical phone QA, 45+ sample, comparative replay, rank, keyword volume, referral acquisition or profit is claimed. **Stop condition:** park these lines after one UX correction if target players do not show actual voluntary replay and preference versus a direct incumbent. This is testable but not yet tested. See [implementation and research brief](game-experiments-13-to-17-2026-11-22.md).


## 2026-11-22 — Five new player-facing prototypes (18–22)

User-directed additions; preserve Kakuro's original conditional search-led production gates, the 100-row research dataset and all prior decisions. New implementations: 

- **18. Numberlink Paths (GF-014).** Two original orthogonal routes cover an entire 4×4 grid without crossing or branching; each connects a same-letter endpoint pair. **Testable UX value:** Can letter-first, large-tap path assignment beat precise drag gestures for short play? **Strongest objection:** Full-board coverage is a variant of Numberlink; tap-to-fill squares may be slower and less tactile than drawing lines.
- **19. Kropki Pairs (GF-020).** Fill 1–4 once per row and column; white dots mean consecutive digits, black dots exact 2:1 ratios. Undotted edges are unconstrained. **Testable UX value:** Do explicit directional dot labels make ratio/consecutive reasoning more legible than tiny colored dots? **Strongest objection:** This beginner positive-dot-only variant simplifies classic full Kropki, and dense clue labels can crowd a small phone.
- **20. Ripple Rooms (GF-022).** Fill irregular connected rooms with 1 through the room size, ensuring any repeated k in the same row or column has at least k cells between it. **Testable UX value:** Can room ID and size labels make spatial number-spacing deductions clear without zoom? **Strongest objection:** An additional room label on every tile creates visual noise and may not be needed after the first few puzzles.
- **21. Battleship Harbor (GF-026).** Find a fleet of exactly one length-3, one length-2, and one single-cell ship on a 5×5 chart, with no ship touching another even diagonally and exact line totals. **Testable UX value:** Can exact fleet rules and optional water notes make a no-guess logic game satisfying in brief sessions? **Strongest objection:** There are mature fleet-logic products; row/col counters alone establish no competitive preference.
- **22. Kuromasu Windows (GF-028).** Shade black cells so black cells never share an edge, all white cells stay connected, and each numbered white square sees exactly its indicated white squares in its row and column. **Testable UX value:** Do visible white/black labels clarify sight-line deduction more than abstract shading? **Strongest objection:** A full white/black/unknown mark per square adds tapping and could be tiring; readability must be tested on phones.

Every new game must pass an independent rules/uniqueness suite, corruption-resistant local save, an actual first-board UI playthrough and a browser artifact export plus HTTP asset verification before merging. **No evidence claimed:** participant 45+ preference, physical iOS/Android accessibility, install demand, repeat-play, referrals, payment or profit. Publishing to app stores remains unauthorized. See [decision brief](game-experiments-18-to-22-2026-11-22.md).


## 2026-11-22 — Ten additional independent playtests (23–32)

At user instruction, build ten further original microgames spanning numeric deduction, language, picture logic and spatial movement. Six small authored levels each. **This does not supersede the Kakuro search-led research gate, add official keyword-volume evidence, or authorize store publication.**

- **23. Renzoku Meadow (GF-025):** 6 independently generated Latin/dot layouts with exactly one solution. **Question:** Do explicit edge labels make full positive-and-negative consecutive logic self-explanatory? **Strongest counterargument:** Full negative-edge rule may surprise users familiar only with positive-dot Kropki, which is already another studio prototype.
- **24. Word Ladder Lane (GF-031):** 6 original small curated word graphs with one accepted shortest path each. **Question:** Can four-letter small-lexicon choice ladders reduce frustrating typing while preserving inference? **Strongest counterargument:** Limited card dictionaries lower vocabulary burden but may feel too guided; free-entry word ladders have different appeal.
- **25. Word Square Studio (GF-038):** 6 original word-card arrangements with a single valid permutation each. **Question:** Can a three-word symmetry puzzle create a fast satisfying first-play loop without word-definition hints? **Strongest counterargument:** Three-word puzzles may be too trivial for practiced word-game players without adding larger verified word squares.
- **26. Vowel Garden (GF-043):** 6 original authored phrases with one accepted canonical restoration each, not proof of linguistic uniqueness. **Question:** Do short topic-labeled phrases make missing vowels genuinely pleasant for adults with varied vocabulary? **Strongest counterargument:** Consonant-only prompts can admit multiple sensible phrases; a single canonical answer is potentially unfair without broader acceptance.
- **27. Word Search Shores (GF-048):** 6 original 6×6 boards; each target word appears exactly once in the accepted straight-line directions. **Question:** Can explicit endpoint taps make offline word search more comfortable without drag precision? **Strongest counterargument:** Direct competitors have much more content; requiring two endpoint taps may be less natural than a continuous swipe for some players.
- **28. Nonogram Canvas (GF-056):** 6 original 5×5 pixel designs with uniquely determined row and column clues. **Question:** Are visible per-line completion counts and large tap targets enough to earn preference over mature Nonograms? **Strongest counterargument:** A 5×5 tutorial can demonstrate correctness but may provide little replay value, and clue headers may crowd narrow screens.
- **29. Akari Evening (GF-062):** 6 original 4×4 numbered-wall boards with one valid lamp arrangement each. **Question:** Will immediate illumination cues reduce the need for rule explanations and encourage further play? **Strongest counterargument:** Small boards and colored light feedback may be aesthetically appealing but insufficient to teach illumination to first-time players.
- **30. Pipe Garden (GF-064):** 6 original 3×3 pipe layouts with one distinct valid port-direction solution each. **Question:** Can a one-tap reversible rotation system deliver satisfying untimed spatial deduction? **Strongest counterargument:** Rotating small pipes can become mechanical trial-and-error rather than strategic play, and 3×3 grids are beginner-only.
- **31. Sliding Tiles (GF-068):** 6 original scrambled starts with verified shortest solution depths of 4,6,8,10,12 and 14 moves. **Question:** Do instant one-tap slides and no timer improve comfort enough to be demonstrably preferred? **Strongest counterargument:** The 8-puzzle is extremely commoditized; deterministic reachability is engineering hygiene, not a commercial differentiator.
- **32. Number Pyramid (GF-079):** 6 independent original pyramid clues with exactly one four-digit base solution each. **Question:** Can large visual sum stones make short arithmetic inference engaging without speed pressure? **Strongest counterargument:** Without scaffolded deduction explanations, this may feel like arithmetic guesswork rather than an enjoyable logic game.

Mathematically unique: Renzoku, 3-word square card placement, Nonogram, Akari, pipe port configurations, and sum pyramid. Unique only within curated word lists/move limit: Word Ladder. Authored-answer only: Vowel Garden. Unique target occurrences: Word Search. Verified shortest reachable scrambles but not unique move sequences: Sliding Tiles. Automated tests check scope honestly; real-player, physical-device and commercial tests remain open. See [decision and cheapest falsification tests](game-experiments-23-to-32-2026-11-22.md).

## 2026-11-22 — Conditional next-three portfolio experiment recommendation

**Decision:** Recommend a *sequential test portfolio*, not three parallel productions: **Kakuro Compass / Kakuro Zen** (existing GF-001 proof) → **Rune Chain / Three Cards** (PH-034 + GX-009, one new short strategy slice) → **Little Worlds / Evergrove Lite** (GC-021 + PH-006 + GX-002, one tiny persistent village test). [Complete evidence, counterarguments and stop gates](next-three-games-validation-decision-2026-11-22.md).

**What is known:** Kakuro Compass already exists with deterministic puzzle/inspector tests. Research documented Kakuro *web* search bands and incumbent Kakuro app features, not measured App Store search or player preference. Good Eye and Shikaku are already playable alternatives. Rune Chain and Little Worlds are conceptual, without measured retention or acquisition.

**Why:** Test three different assumptions—existing non-branded search acquisition, independent repeatable strategy depth, and additional return from persistent consequences—without confusing imagined global upside with a viable indie product. New ideas have no automatic exemption from ASO-only gate; a separate distribution decision is required to ship them on non-ASO grounds.

**Strongest counterargument:** The best investment may be **zero net-new game projects** until existing prototypes show real preference, voluntary return and acquisition potential. A great card or life sim can still fail for lack of discovery.

**Cheapest falsification:** 10–15 target-player Kakuro-vs-Conceptis counterbalanced sessions plus exact US App Store query audit; only then an original ~12-card, three-rule Rune Chain replay test; only then a five-puzzle Little Worlds persistent-world versus puzzle-only controlled screen.

**Continue / stop:** Preference and organic search route are prerequisites for Kakuro; voluntary replay and credible distribution for Rune Chain; causal understanding, incremental voluntary return and credible distribution for Little Worlds. Park after bounded UX revisions if players prefer incumbents or do not return. No new production approvals.

**Next action:** Obtain a shareable Kakuro browser playtest with approved hosting, recruit the first independent target player and log observed play and a direct competitor comparison. GitHub WebGL artifacts alone are *not* public hosting.

## 2026-11-22 — Evergrove selected for falsifiable build

**Founder decision:** Treat `PH-006 Evergrove` as the one long-horizon global game concept to explore. **Engineering response:** implement `games/evergrove/` with a tiny deterministic living-village loop, the same six Pipe Garden fixtures in an optional control mode, local choice memories and story consequences, and Godot tests.

**Evidence:** founder preference and design hypothesis only. No direct player tests, accessible market sizing, keyword demand, retention, App Store sales, device QA, or production-viability evidence.

**Strongest counterargument:** the added village may obscure or weaken an already enjoyable puzzle, while creating hidden costs and difficult distribution.

**Cheapest falsification:** manually balance 20–30 target players between living-world and puzzle-only modes; observe comprehension, head-to-head preference, voluntary replay and D1/D7 return.

**Stopping rule:** If the world does not produce meaningful voluntary behavior beyond the same puzzle, strip back the simulation and avoid adding LLMs or social infrastructure.

**Source:** [full decision record](evergrove-build-decision-2026-11-22.md). **Status:** research prototype, not a validated product or store launch.

## 2026-11-22 — Evergrove switches to Unity/C#

**User instruction:** convert Evergrove to C#. **Decision:** use a Unity 6.3 C# client with independently testable C# domain rules and a code-generated UI; retain the rest of Game Foundry's Godot projects unchanged. The not-yet-merged Evergrove Godot files are deleted on its feature branch.

**Backend:** a separate, optional Cloudflare Worker/Durable Object implementation in TypeScript stores one isolated example tree state. Workers are not a C# runtime. It does not currently sync Evergrove puzzle progress, NPC memory or village choices.

**Tradeoff:** richer Unity/C# development and familiarity versus an additional engine's maintenance, device validation and licensing/package surface. No claim of verified Android/iOS builds or gameplay retention.

**Falsifier:** if this engine change increases development or QA effort without improving actual player experience, reconsider it before large-scale work. See [Evergrove experiment](evergrove-build-decision-2026-11-22.md) and [Unity project](../../games/evergrove/README.md).

## 2026-11-22 — ADR: Unity/C# is Evergrove's conditional engine choice

**Decision:** Adopt Unity 6.3 LTS + C# for the current Evergrove development and playtest path. Godot remains the studio default for other puzzle games. See [full engine ADR](../decisions/2026-11-22-evergrove-engine-unity-csharp.md).

**Reason:** Potential 2.5D/3D living-world evolution and C# tooling; retain independently testable pure C# simulation. A coherent, consequential world matters more than rendering technology or unrestricted generative AI.

**Strongest contrary case:** Unity's proprietary license, editor weight and tooling overhead may not be justified if the experience remains a simple 2D puzzle; Godot would then be a better small-studio fit.

**Test:** Verify editor import, real iOS/Android builds, offline save and full game loop. Separately compare Living World with puzzle-only among recruited players. Record concrete costs and failures.

**Revisit if:** Unity cannot reliably ship, its economics deteriorate, or measured player value does not justify its complexity. No automatic migration of other games.

## 2026-11-22 — Evergrove 2.5D miniature-world visual direction

**Decision:** Select a stylized, mostly fixed-camera orthographic **2.5D garden/village** as Evergrove's *preferred long-term art hypothesis*, using lightweight 3D environments and characters with a crisp 2D puzzle interface. **No 3D implementation or production art-pipeline approval yet.** The current original 2D illustration remains the playable baseline. See the [full 2.5D ADR](../decisions/2026-11-22-evergrove-visual-direction-2p5d.md).

**Reasoning:** Persistent decisions (tree growth, bridge repair, visitor paths, flowers, NPC routines) might be more legible and emotionally tangible in a stable miniature environment. Actual effect on repeat play, performance and art costs is unknown.

**Strongest opposing case:** Excellent 2D art might provide equal attachment with lower asset/animation costs, stronger performance and simpler production. Simulation depth does not depend on 3D.

**Cheapest test:** First verify the existing 2D Unity game on real devices; then show the same one-garden scene and consequences in 2D versus 2.5D to ~10–15 target players, counterbalancing order. Measure comprehensibility, preference, attachment, art hours, mobile rendering and readability. Small samples are directional only. Keep this separate from the living-world vs puzzle-only retention experiment.

**Continue/stop:** Continue toward 2.5D only if additional player value warrants measurable cost; otherwise retain 2D. Do not develop full 3D exploration without a new decision and validated player need.

## 2026-11-22 — Branch-sensitive watercourses, no premature market certainty

**Problem:** Story choices changed environment artwork and dialogue but not the spatial puzzle itself, so Evergrove risked remaining a cosmetic renovation game.

**Change:** Introduce `ReactiveWatercourses`: the previous village choice selects a rotated but mathematically equivalent water network, with one protected, immutable landmark to route around. Tree / crossing / wildflower variants physically differ; `Puzzle Only` keeps its baseline fixture. Version 3 saves include the current and previous board variant; v1/v2 players keep the active board unchanged.

**Most serious caveat:** Symmetric rotations and one fixed tile remain shallow dynamics. This is a falsifiable *mechanical causality* slice, not a complete emergent ecological simulation. The treatment now also changes puzzle mechanics, so the existing `Living World` vs `Puzzle Only` experiment is **no longer a clean narrative-only test** beyond chapter one.

**Next evidence:** Prove all branches uniquely solvable by CI; test actual Unity build, compare difficulty and time-to-solve per branch, and run a separate controlled user test against matched topology before claiming superior retention. Do not ship under `Evergrove` without name/trademark review.

## 2026-11-22 — Source-grounded Evergrove handbook

**What changed:** Audited 12 authored puzzle chapters, 33 rotated/anchored choice-specific presentations, v3 save/migration, 2D vs 2.5D, and 30 selectable languages (English source + 29 draft catalogs; 76 compact keys). Created the [canonical product and engineering handbook](../../games/evergrove/docs/README.md) with actual rules, architecture, state, UX, experiments, QA, strongest objections and explicit stop conditions.

**Corrected prior notes:** Opt-in procedural 2.5D *is now implemented* (not production tested); the Living World vs Puzzle Only comparison is **confounded after chapter 1**; the locale catalog contains **76** keys, not 73. Documentation corrections are not new player evidence.

**Next real gate:** run Unity on a physical phone, conduct uncoached playtests and publish a continue/simplify/stop decision. Additional docs/code are not proof of demand.

## 2026-11-22 — Specs implementation slice with evidence boundaries

**Code delivered in PR #46:** source-derived catalog for 45 puzzle presentations (12 authored + 33 ecological branches), schema v4 for a three-step voluntary hint (row/tile/one correction), initial puzzle closer to the top, visible local save-failure warning, non-color per-tile mismatch markers, and a choice-grounded causal journal. Five new draft language keys added across 29 JSON catalogs (81 total). All changes are **source-level**, not an independent Unity phone test or native-language approval.

**Mechanism limitations:** rotations preserve the underlying puzzle graph; theoretical minimum taps are not difficulty; historical v3 saves do not contain enough branch event detail to justify retrospective claims. No proper complex-script shaping, lossless crash-resilient save certification, full contrast or VoiceOver/TalkBack sign-off, customer research, organic D1/D7, trademark clearance or monetization is implied.

**Next high-value reality gate:** licensed Unity Editor compile, physical Android/iOS smoke with early participants, then one controlled matched-puzzle comparison of whether actual causal consequences increase voluntary next-chapter play. Avoid interpreting task completion count as improved player value.

## 2026-11-23 — Mosselora working identity and design-system code slice

**Decision:** Implement a reversible player-facing rebrand and visual simplification in the Evergrove Unity source and one-puzzle browser study: Mosselora wordmark, shared palette, rounder pipe hearts, calmer non-color open-end markers, equal visual choice weighting, slightly richer original village artwork and consistent story/memory controls. Keep existing save paths and engine IDs unchanged; the name is **not legally cleared**.

**Mechanism:** comprehension should improve when water, puzzle and village share one causal visual language; the first choice should not be preselected by styling. These are hypotheses, not results.

**Strongest objection:** the new marker may be less obvious and art may increase friction; all work is lower value than a real Unity build and uncoached players. Do not over-interpret browser screenshots as proof of mobile user delight.

**Cheapest disproof / status:** run static/C#/browser checks, licensed Unity import and real-device QA; observe 8–12 uncoached players aged 45+ in a first-minute task. Continue only if goal, flow and consequence are understood; simplify if 3/10 starters fail on goal or restoration. No release or retention claims follow from passing CI.

## 2026-11-22 — Eight-game mobile publishing audit

**Decision:** do not create app records or submit any title yet. None of the eight prototypes has passed the repository's audience, market, physical-device, and release-readiness gates. The request for automatic release after approval remains unconfigured and does not waive those gates.

**Store evidence:** signed-in App Store Connect and Google Play Console were readable in the built-in Browser. Exact-title searches found no records for Good Eye, Kakuro Compass, Odd Bloom, One Path, Quiet Camp, Quiet Shot, Shikaku Garden, or Takuzu Lab. No store records or listings were created or changed. Play Console showed 24 account apps, 11 production apps, 11 apps with policy tasks, and one submission not yet sent for review. The console labels the account Personal; confirm the legal publisher/account owner before declarations.

**Search evidence:** Semrush keyword reports were unavailable because the connected account had no API units. Existing Google Keyword Planner rows are broad web-search ranges for 2025-11 through 2026-10; they are not App Store volume, installs, ranking, or evidence of 45+ searcher fit. No opportunity score or new search metric was generated.

**Local evidence:** all eight Godot 4.7.2 projects imported and passed their existing rules, persistence, and scene scripts. The research backlog validator and all 50 Python unit tests passed after the loopback-only web-playtest checks were rerun with local server access.

**Native export proof:** the matching official Godot 4.7.2 templates were downloaded from the upstream release and SHA-256 verified. Temporary debug APKs for all eight titles compiled with min SDK 24/target SDK 36, were signed with Godot's debug key, installed, and launched on an Android 16/API 36 Pixel 7 AVD. All eight Gradle AABs built but are unsigned. The required ETC2/ASTC Android texture-import setting was added to all eight requested projects and all eight re-imported successfully. Eight Android screenshots and short recordings are stored in each game's native-proof folder. Temporary Xcode projects for all eight titles exported and all eight arm64 device targets compiled without signing. Kakuro's Simulator link failed because the distributed simulator library slice is x86_64-only while this Apple Silicon Simulator target requires arm64. No physical iPhone or Android device is attached. Xcode has zero valid signing identities; 11 provisioning profiles exist but none is verified for these unassigned bundle IDs.

**What changed:** store/release-status.csv separates each game's iOS and Android record, build, upload, review, approval, release-setting, and public-availability state. Eight English masters and 200 translated-description drafts cover 25 unique store locales. All translation drafts remain native-review-pending; unsupported or proxy-only roster rows are not represented as direct translations. Generated feature-art concepts remain unapproved; all eight titles now have debug-only Android AVD captures, none of which are physical-device or store-ready evidence.

**Next decision gate:** recruit the intended adult-45+ players, complete the comparative market/player gate for each candidate, agree permanent identifiers and listing decisions with the account owner, then produce signed builds and run physical-device QA before any store submission. Takuzu Lab remains outside production selection.

## 2026-11-22 — Evergrove publishing preflight remains blocked at M1

**Source:** `795526432577120ed76bca432ea4ea8675622d23`, fetched `origin/main`; no newer upstream commit was present. Evergrove project files remain unchanged.

**Passed locally:** `tools/verify_evergrove_project.py` and `tools/check_evergrove_docs.py` passed. The required pure C# suite passed **5,404 assertions** with .NET SDK `8.0.428` installed under `/private/tmp`; this is domain logic evidence only.

**Still blocked:** the licensed Unity `6000.3.0f1` Editor is absent, so the local M1 preflight failed before Unity import. No Android or iOS Unity build, signing, physical-device run, or actual gameplay capture exists. Existing `EvergroveBuild` only emits development APK / unsigned Xcode playtest outputs with a playtest identifier, not production AAB/IPA artifacts.

**Store state:** App Store Connect redirected to login; the user was handed the Browser. Play Console was not checked. No app record was created, no binary uploaded, no review submitted, no approval or public listing verified, and no release option configured.

**Decision:** keep Evergrove ineligible for store submission. M1–M6 and the account-owner, title, privacy, pricing/territory, physical-device and player-evidence gates remain open. The English listing, 25 locale drafts and promotional key-art concept are preparation only; the translations are not natively reviewed and the image is not gameplay footage.

## 2026-11-23 — Free pricing confirmed for the publishing batch

**User decision:** all games in the current publishing batch, including Evergrove, are free. The English metadata masters now record that decision. No platform price setting has been applied because Evergrove has no app records yet and the original eight have not passed their release gates. This does not waive territory, rating, account-owner, build or review requirements.

## 2026-11-23 — Evergrove exact-title store lookup

**Observed in signed-in consoles:** the App Store Connect Apps search and Google Play Console app-table search both returned no exact-title `Evergrove` result. No app records or store settings were created. Evergrove's iOS bundle ID and Android package ID remain unassigned.

**Current decision:** prepare to create records only after the permanent identifiers and missing metadata choices are settled. The user confirmed a free price for every title; production territories, content rating, title clearance and owner declarations remain unresolved. Automatic Apple release and Google standard publishing remain requested but unconfigured. M1–M6 still prohibit submission.

## 2026-11-23 — Evergrove app identity and store-record attempt

**Apple:** registered the explicit Bundle ID `com.streamentry.evergrove` in the signed-in developer team, based on the source project's `com.streamentry.evergrove.playtest` namespace. Creating the App Store Connect app record failed validation because Apple reports the public name `Evergrove` is already in use. No trademark claim was filed; no app record was created.

**Google Play:** the signed-in account search had no exact-title result. The Create App form requires the account owner to attest Developer Program Policy compliance and U.S. export-law compliance. Neither checkbox was accepted, and no app record was created.

**Decision:** hold both store records until the account owner clears or changes the Apple title and can make the required Play attestations from the actual build. Free pricing is user-confirmed for all titles; no price setting, upload, review submission, or release configuration changed.

## 2026-11-23 — Evergrove: Living Garden store-record setup

**User-selected alternate:** after App Store Connect reported the name `Evergrove` was already in use, the user selected `Evergrove: Living Garden`. No trademark claim was filed; exact-title web searches found no indexed result, but legal trademark clearance remains unverified.

**iOS:** registered Bundle ID `com.streamentry.evergrove` and created App Store Connect app ID `6838015597` with internal SKU `GF-EVERGROVE-IOS-20261122`, primary language English (U.S.), and limited access. Version 1.0 is `Prepare for Submission`; **Automatically release this version** is selected. No build is uploaded and no review has been submitted.

**Android:** Play Console reports package name `com.streamentry.evergrove` available in the Create App form. The user-selected title, English (U.S.), Game type and Free price were entered for the preflight. The record was not created because Google requires the account owner to affirm Developer Program Policy compliance and U.S. export-law compliance. Both attestations remain unchecked; no package name is reserved.

**Decision:** keep both platforms unsubmitted. Unity M1, signed builds, device/player gates, privacy/support metadata, final rating/territories, trademark clearance and account-owner declarations remain open. All games are free per user instruction, but store pricing has not yet been configured.

## 2026-11-23 — Evergrove: Living Garden listing staged in App Store Connect

**User title decision:** use `Evergrove: Living Garden` as the alternate title.

**iOS record:** App Store Connect app ID `6838015597` exists with Bundle ID `com.streamentry.evergrove`, SKU `GF-EVERGROVE-IOS-20261122`, English (U.S.) and limited access. Version 1.0 remains `Prepare for Submission`. The source-based description and keywords are saved; sign-in-required is off. Automatic release after App Review is selected and read back. The record has no build, screenshot asset, support URL, privacy URL or review contact; it has not been submitted.

**Android:** package name `com.streamentry.evergrove` passed Play Console's availability check but is not reserved. The Create App form is staged with the selected title, English (U.S.), Game and Free. The account owner’s Developer Program Policy and U.S. export-law confirmations remain unchecked, so no Android app record exists.

**Limit:** App Store Connect accepted the alternate title for record creation; this is not legal trademark clearance. No public app is available on either store. M1–M6 and all build, privacy, device, player and owner-attestation gates remain open.

## 2026-11-23 — Evergrove Apple free-price schedule configured

**Store setting:** saved a $0.00 Apple price schedule with U.S. base pricing and comparable $0.00 prices for all 175 listed territories. Apple says this takes effect only when the app is Ready for Sale. App Availability is still unset, so this did not make the app public. Apple Silicon Mac and Vision Pro distribution were disabled to keep the release scope on iOS; the app's release-after-review option remains selected.

**Android:** no price setting exists yet because the Play app record is still blocked at the two account-owner certifications. No Play policy or U.S. export-law box was checked.

## 2026-11-23 — Play app creation halted at owner attestations

**Preflight:** entered `Evergrove: Living Garden`, `com.streamentry.evergrove`, English (U.S.), Game and Free in the signed-in Play Console. The account search returned no app with the alternate title, and the console confirmed the package name was available. No record was created because the form requires its account owner to attest that the app meets Developer Program Policies and U.S. export-law requirements. Both boxes remain unchecked; no Android package is reserved. The app has no Unity build yet, so the export declaration is not ready for owner review.

**Decision:** leave Play creation unsubmitted for the account owner. Do not bypass those declarations or use an unsigned/debug build to claim release readiness.

**Additional Play gate:** the signed-in developer account is Personal, but its creation date is not visible in the app table. Google’s current help page requires accounts created after 2023-11-13 to complete a closed test with at least 12 testers opted in continuously for 14 days before production access. Treat this as conditional until the account date and app-specific access state are verified. See [official testing requirements](https://support.google.com/googleplay/android-developer/answer/14151465?hl=en-419).

## 2026-11-23 — Rechecked the original eight store records

**Observed:** signed-in App Store Connect and Google Play Console searches returned no exact-title result for Good Eye, Kakuro Compass, Odd Bloom, One Path, Quiet Camp, Quiet Shot, Shikaku Garden or Takuzu Lab. No records were created for those eight. Evergrove is separate: Apple has a draft record; Play creation remains blocked at owner attestations.

## 2026-11-23 — Latest Evergrove browser research slice passes automated checks

**Source:** fetched `main` to `5b2e58370caf7335e7ac8035499a3c9c564b57bd`; no overlapping local paths were present, so the fast-forward preserved local publishing work.

**Passed:** current .NET 8.0.428 domain/save suite (**5,469 assertions**); first-minute Node suite (**10/10**); project and docs verifiers; local preflight wrapper tests (**8/8**); offline Chromium viewport smoke at **320×568, 375×667 and 430×932**. Screenshots are generated under ignored `artifacts/evergrove-first-minute/` for human inspection.

**Limit:** the browser slice is not a Unity build, native app, physical-device test, human study, store screenshot or release asset. Unity `6000.3.0f1` remains absent and the local M1 preflight still exits before import.

## 2026-11-23 — No existing Unity action artifact to reuse

`gh run list --repo streamentry/game-foundry --workflow evergrove-unity-playtest.yml` returned no workflow runs. No Unity preview artifact exists to retrieve. The local machine still lacks the licensed Unity 6.3.0f1 Editor; the workflow only emits development artifacts and requires the maintainer's Unity license secret. No workflow was triggered.

## 2026-11-23 — Evergrove: Living Garden promotional concept

Generated and saved a second original key-art concept under `store/assets/evergrove/concepts/` for the user-selected title. It depicts the village creek, bridge, canopy and wildflowers without text, UI or gameplay claims. The PNG is 1672 × 941 (SHA-256 `38300de2056f39ae618de50961f667324db583b195f0c178e940c04cd6361387`). This is concept art only; screenshots and preview footage still require a running native build. No store, release-gate or publication state changed.

## 2026-11-23 — Evergrove source-only privacy inventory

Inspected `games/evergrove/Packages/manifest.json` and tracked C# sources. The manifest uses uGUI and Input System packages plus Unity UI, JSON and UnityWebRequest modules; no third-party analytics, advertising, account or attribution SDK is listed. `Assets/Scripts/Unity/WorldClient.cs` contains a separate optional Cloudflare reference client. No boot/scene reference was found, its Worker URL defaults to empty, and requests require an attached component, a configured HTTPS URL and an explicit method call. If enabled, it persists a random bearer capability in PlayerPrefs and sends a demo `WATER_TREE` GET/POST flow. This scan does not establish what Unity includes in a player binary or prove zero network traffic. Keep privacy/Data Safety declarations pending until a release-build dependency inspection and controlled device network capture confirm the shipped behavior.

## 2026-11-23 — Mosselora public working name

**User direction:** use `Mosselora` as the public working name; `Evergrove` is the internal code name. The latest source commit `fc9d7daebf8b5ddff5301d9ef582cce3dfd17801` adds a Mosselora design proposal that marks the name as pending formal trademark clearance and warns against silently renaming internal identifiers. This volume is case-insensitive and the repository tracked both `DESIGN.md` and `design.md`, so the Evergrove implementation guide was renamed `games/evergrove/DESIGN-SYSTEM.md`; the Mosselora proposal stays at `games/evergrove/design.md` and links were updated.

**Apple:** changed and saved the existing App Store Connect app name to `Mosselora` in App Information. Apple ID `6838015597`, Bundle ID `com.streamentry.evergrove`, SKU, saved free-price schedule, and automatic-release setting remain unchanged. This is a draft app record, not a public listing; trademark clearance remains unverified.

**Google Play:** app-table search for `Mosselora` returned no result. The Create App form is staged with title `Mosselora`, English (U.S.), Game, Free, and package `com.streamentry.evergrove`; the package availability check passed. The Play record was not created, and the account owner's policy and U.S. export-law attestations remain unchecked.

**Internal references:** retain the `evergrove` project path, app identifiers, SKU stem, source namespaces and development build labels. Public listing copy uses Mosselora. The app has no native build or submission; M1–M6 and formal name clearance remain open.

## 2026-11-23 — App Store category set for Mosselora

The App Store Connect record is a pipe-connection logic puzzle. Set and saved its primary category to **Games** and first subcategory to **Puzzle**. Left the optional second subcategory and secondary category blank. The subtitle is `A Living Garden`. The app remains `Prepare for Submission`; this category change does not alter eligibility, review or availability.

## 2026-11-23 — PH-006 pitch superseded; Mosselora long-range vision recorded

**User direction:** mark the original PH-006 pitch (hybrid-casual life-sim, AI companions, global player trading, cloud sync, "extremely high retention") as superseded, and write a long-range vision aiming for a century-lasting game with 100 million lifetime players.

**Change:** `docs/ideas/projected-global-hits-2026-2036.md` now marks PH-006 superseded and points to the as-built product. `games/evergrove/vision.md` brings back companions, a living ecology, player exchange and optional sync as **gated future layers** (authored companions, gifting without markets, an offline-complete core). Each layer is locked behind a growth stage with a stop rule.

**Evidence status:** no new player or market evidence. The 100-million figure is a founder-chosen target, not a forecast. The Seed stage (M1–M6) remains unfinished. Nothing in the vision authorizes building beyond the current specs.

## 2026-11-23 — Mosselora vision revised after full research audit

Four parallel read-only audits covered `games/evergrove/docs/specs/`, the handbook, decision records, `docs/research/` and `docs/ideas/`. The first `vision.md` conflicted with the specs in several places: wall-clock seasons, a supporter tier and paid seasons, the Commons arriving without acknowledging the reject list, ecology deferred to 100k players, M4 and pricing missing from the Seed gate, and saves described as open and durable when they aren't. It also left out discovery, measurement, trademark, text rendering and portfolio order.

**Change:** rewrote `games/evergrove/vision.md`. It now opens with a starting-line table of current evidence. It names one candidate differentiator (a choice changes the next puzzle) that depends on M4. Each layer gets its smallest test and kill rule from the theses. The growth ladder reuses declared thresholds (spec 07 continue rule, M1–M6, 1,000 installs read against search impressions, founder-proposed D1/D7 heuristics). New sections cover discovery, the spec-preferred one-time-unlock model, and a table of where the vision goes beyond current specs.

**Evidence status:** unchanged. Zero human sessions; pipe-puzzle demand is unmeasured. The 100M target remains a founder aspiration.

## 2026-11-23 — Mosselora store category and native toolchain check

**Category:** the game connects pipe tiles to route water through a puzzle, then lets the player choose a village restoration. Apple Games → Puzzle is the best primary category; optional subcategories and secondary category stay blank. The store metadata records Games / Puzzle and App Store Connect previously confirmed the save. The current ASC tab is signed out, so this turn could not read back the live value.

**Name screen:** the official USPTO wordmark search returned no exact `Mosselora` results. Searching `mossel` returned five records, including a live registered `MOSSEL` mark in IC 010 and a live pending `MOSSEL` mark in IC 017 for foam supports for floral arrangements. This is not a legal clearance: sound/appearance, goods relationship, unregistered rights, and other databases still need review. Preserve Mosselora as the user-selected working title, with clearance marked pending.

**Native toolchain (initial check before the later Editor smoke):** the Unity executable at `/Volumes/SSD/Unity/editors/6000.3.0f1` reports the pinned version, but `codesign --verify --deep --strict` reports an invalid signature. The Unity CLI refuses to register it. `unity status` and `unity editors running` report no matching instance, including outside the sandbox, while a direct headless launch reports a project-open conflict; the live Editor/lock state is unresolved. `Packages/manifest.json` has no `com.unity.pipeline`; `PlaybackEngines` contains only `MacStandaloneSupport`, with iOS and Android support absent. Headless VerifyProject stalled at the Licensing Client. No Unity import, device build, or native export was verified at this first check; the later Editor smoke is recorded below. Do not bypass signature checks.

**Signing:** a sanitized local check found zero valid Apple code-signing identities and zero of eleven provisioning profiles matching `com.streamentry.evergrove`. Certificate and profile contents were not printed. No signed Apple build can be produced with the current local signing setup.

**Play Console:** the signed-in Create App form shows Mosselora, package `com.streamentry.evergrove` available but not reserved, English (U.S.), Game, Free and automatic protection enabled. Developer Program Policy and U.S. export-law confirmations remain unchecked, so the app record was not created; those are account-owner attestations.

**Cloudflare live check:** the deployed `mosselora-site` workers.dev home, privacy, support and preview pages all returned HTTP 200. Their page titles use Mosselora and the HTML contains no public `Evergrove` text. `https://api.mosselora.com/healthz` returned `{"service":"mosselora-world-api","status":"prototype","gameplaySync":false}`. The requested `https://mosselora.com/` timed out. The site config still contains `mosselora.com/*`; a previous route deployment was rejected because it could replace existing apex-site traffic. Do not deploy that route until the owner explicitly approves the takeover.

**Configuration change (current):** ProjectSettings.asset names Mosselora, uses com.streamentry.evergrove.playtest for Android and iPhone, sets overrideDefaultApplicationIdentifier, targets iOS 15.0 and has empty iOS platform icon texture slots. The App Store record uses com.streamentry.evergrove. An uncommitted EvergroveBuild edit requests ARM64 for iOS Simulator, but iOS Build Support is absent so it remains uncompiled. The build entrypoint still uses Development/playtest identities.

## 2026-11-23 — First Unity Editor verification; vision execution wave 1

**Observed:** the local Unity `6000.3.0f1` Editor (installed at `/Volumes/SSD/unity`) ran `EvergroveSceneSetup.VerifyProject` headlessly. Output: `EVERGROVE_UNITY_IMPORT_CHECK_OK editor=6000.3.0f1 puzzles=12 importedLocales=29 snapshotSchema=4` and "Evergrove Unity Editor compilation, scene setup and puzzle smoke passed." This is the first time the Unity-specific C# has compiled. Unity also generated its standard ProjectSettings, `.meta` files and `packages-lock.json`, which are now tracked. The .NET 8 SDK (8.0.428) was installed to `/Volumes/SSD/dotnet`, and the pure core suite passed locally with 5,499 assertions.

**Blocked:** `EvergroveBuild.Build -buildTarget iOS` fails because the editor reports "0 entitlements" and loads only the Mac platform module. Player builds need the account owner to sign in to Unity Hub and activate a license; agents cannot do this. The Android module is also not installed. M1 is **partial**: editor import and smoke pass; APK and Xcode exports are not done.

**Research added (agent-prepared, unmeasured):** `pipe-puzzle-competitor-scan-2026-11-23.md` (15 listings, observed listing evidence only), `pipe-water-keyword-queue-2026-11-23.csv/.md` (26 queued queries, every measurement `unknown`), and the `games/evergrove/docs/playtest/` M3 kit and M4 preregistration draft (no sessions run). The playtest review found that vision.md attributed D1/D7 heuristics to the wrong file; this is now corrected.

**Decision:** continue Seed-stage work. The next human steps are Unity license activation and recruiting 8–12 uncoached adults 45+ for M3.

## 2026-11-23 — Source and QA progress after first Editor smoke

**Source changes on local `main`:** `PuzzleMatched` now has a fixed per-run board schedule for M4 comparison; `GardenTransfer` adds a bounded, checksum-checked text save-file format; the macOS Development player has a dev-only screenshot hook; connected and dry pipe strokes now differ by a continuous versus segmented stripe, not color alone. These do not add cloud sync or phone file-picker UI.

**Verification:** `/Volumes/SSD/dotnet/dotnet run --project games/evergrove/Tests/Evergrove.Core.Tests.csproj --configuration Release` passed **38,835 assertions** (including 1,626 save-journal and 2,100 transfer assertions). `tools/verify_evergrove_project.py` passed its static checks; the browser first-minute suite passed 10/10. A chapter-one screenshot exists at `games/evergrove/docs/qa/mac-player-2026-11-23/01-launch-chapter1.png` from a macOS Development player. That screenshot predates the connected/dry stripe change, is desktop-only and does not verify phone rendering.

**Limit:** M1 Editor import smoke is recorded, but the currently discoverable Editor still fails macOS signature verification and only the Mac platform module is installed. No iOS/Android export, signed build, phone launch or 45+ session is proven. The contrast audit still lists 21 failed color pairs; further fixes and device review remain open.

## 2026-11-23 — Vision execution wave 2: matched control, exportable saves, first rendered build

**Implemented and verified in code:**
- `PlayMode.PuzzleMatched` (puzzle-identical M4 control; all 33 chapter×variant pairs and 3^7 choice prefixes checked), selectable through a development-build research control in `EvergroveApp`.
- `GardenTransfer` exportable save envelope (`mosselora-garden` v1, SHA-256 integrity check, strict parser, 1 MB cap); it round-trips matched schedules. No share or file UI yet.
- A non-color dashed/solid cue for dry vs connected pipes, `earth.dry` set to `#7D684F`, and contrast fixes in the browser slice (node tests 10/10).
- Doc drift fixed: 81 keys, Vietnamese endpoint wording, and a canonical M3/M4 threshold table that lists `FOUNDER DECISION NEEDED` items.

**Observed:** the C# suite reports 38,835 assertions. The Unity 6000.3.0f1 compile and smoke pass. **A development macOS player built and rendered chapter 1**, captured with a new dev-only `-mosseloraScreenshot` hook (`games/evergrove/docs/qa/mac-player-2026-11-23/`). Follow-up: the spring endpoint glyph was changed to three sinusoidal water strokes in commits cd76cf3 and d8a5d38. Screenshot 03 visually shows this correction in the macOS Development player; phone and 45+ validation remain open.

**Research added:** WCAG contrast audit (21 of 97 pairs failed before fixes), preliminary name knockout (no exact conflict; not clearance), and a post-M4 depth proposal (variants add 0–3 taps; recommends blank root cells and orientation-masked tiles).

**Human blockers (unchanged):** activate a Unity license so iOS builds work, and approve the Android module install (about 3.6 GB download, needs Unity EULA acceptance). Recruit uncoached adults 45+ for M3, give founder decisions on the M3/M4 thresholds, and arrange an attorney name clearance.

## 2026-11-23 — scripted desktop QA tour captured

**Change:** development-only `DevQaTour` runs a fresh in-memory world, invokes real UI button listeners and saves thirteen named PNG states. It does not load or save player progress. The committed evidence set now has 29 macOS Development-player screenshots: three standalone frames, thirteen core-loop tour states and thirteen 150% text-size tour states under `games/evergrove/docs/qa/mac-player-2026-11-23/`. Screenshot 03 confirms the new spring-wave glyph; the tour screenshot for the M4 matched chapter remains development UI.

**Verification:** the current pure C# suite passed 38,835 assertions, the static Unity contract verifier passed, and the browser first-minute suite passed 10/10. These results establish source behavior and desktop rendering only. The tour invokes button callbacks programmatically; it is not touch-input, device, human-use or store-preview footage evidence.

**Follow-up visual work:** commits `fa51c5b` and `2f99d69` add a non-color protected-landmark badge and adjust memory, journal and ending layout/scroll positioning; the corresponding tour frames were regenerated. Screenshot 07 now shows the landmark badge, and frames 10–11 show the revised journal/ending screens. They remain desktop Development-player captures; the screenshot set has no phone or touch-input pass.

## 2026-11-23 — desktop Development-player privacy inspection (EG-009 partial)

The new [binary inspection report](../../games/evergrove/docs/qa/privacy-binary-inspection-2026-11-23.md) covers a macOS Development QA player only. It found no third-party SDK assemblies, Unity cloud services disabled, and `WorldClient` compiled but unattached. A roughly six-second run observed no outbound TCP connections, but did see the Development Player Connection listener on TCP 55000 and unbound UDP sockets. This is not a traffic capture, does not prove no data transfer, and says nothing conclusive about iOS or Android release builds. EG-009 remains open pending signed mobile artifact inspection, controlled device network capture and reconciled store privacy declarations.

## 2026-11-23 — EG-007 text-size control implemented

`TextScale` adds an in-game 100% / 125% / 150% text-size control. The preference is stored separately from the garden save; dev QA can override one run without persistence. A 13-frame 150% tour at 375×667 was captured from the macOS Development player. The UI captures show the board, choice, memory and ending states at that scale, but they do not verify iPhone/Android safe areas, OS text-size integration, screen-reader behavior, or adults aged 45+.

## 2026-11-23 — Vision execution waves 3–5: visual QA loop and pre-M3 fixes

**Tooling:** a dev-only `-mosseloraQaTour <dir>` drives the real `EvergroveApp` through puzzle → choice → consequence → choice-shaped chapter 2 → journal → ending → matched control → anchored-tile tap. It uses an in-memory world and never touches the player's save. 13 core-loop PNGs are in `games/evergrove/docs/qa/mac-player-2026-11-23/tour/` and 13 text-size PNGs are in the 375×667 150% set; the 13th frame covers tapping an anchored tile.

**Defects the tour found and fixed (verified in the macOS dev player, not on a phone):**
- The spring marker looked like a menu icon. It's now three parallel procedural waves.
- The protected landmark showed as a tiny dot. It now has a heavy frame and a badge per choice (roots, bridge footing, flower).
- Memory, journal and ending cards had fixed minimum heights, which left empty gaps. The ending opened scrolled past its top.
- On the consequence screen, settings pushed the memory card below the fold. It's now consequence-first (EG-003).
- Tapping an anchored tile did nothing. It now shows the anchor message.

**Added:** in-game text size 100/125/150% (EG-007; separate PlayerPrefs; no new localization keys). Desktop privacy inspection for EG-009: no third-party SDKs; `WorldClient` compiled but not attached; no outbound sockets in a short run; dev Player Connection port 55000 open (confirm it's absent in release).

**Verification on merged main:** C# 38,835 assertions; verifier OK (81 keys × 30); first-minute node tests 10/10; Unity macOS build with 0 compile errors; QA tour 13/13 screens.

**Open:**
- The ending has no visible heading; adding one needs a new localization key.
- The landmark badges are about 27 px and the root badge could be misread. Player testing should check this.
- All evidence is from desktop and agents. **Age fit is not validated** and there are zero human sessions.

**Decision:** the agent-doable pre-M3 polish is largely done. The next real progress needs people: Unity license activation (iOS), approval of the Android module EULA and download, a phone build, then 8–12 uncoached adults 45+ for M3.

## 2026-11-23 — localized ending heading added

The new `ending_title` key is present in all 29 offline locale catalogs, bringing them to **82 keys each**. The Python project verifier passes and the current pure C# suite passes **38,864 assertions**. These checks cover catalog structure and rules; the recorded Unity import/smoke predates this UI localization change, so a fresh Unity import and device render are still required. The 25 store-description drafts remain native-review pending.

## 2026-11-23 — M1 build evidence; undo history, garden-file UI, screen-reader first pass

**M1:** `tools/evergrove_unity_local_preflight.sh` passed `smoke`, `Android` and `iOS` at source `5acbac4c5cba4164a31abf33dedf71ff9bf2f8ba` (Unity 6000.3.0f1 Personal license, tracked tree clean). The editor logs name the pinned version, the import check, and `EVERGROVE_BUILD_SUCCEEDED` with each run's own output path; there are zero compile errors. Evidence packets live under `games/evergrove/docs/evidence/5acbac4…/BLD-00{1,2,3}/`, with hashes only; raw logs stay in the git-ignored `.evergrove-evidence/`. BLD-001/002/003 are marked verified in the matrix, and the reviewer role says this was an AI agent's log review, not independent human verification. The committed iOS default SDK is now the device SDK; simulator builds switch it at build time.

**Implemented (verified in code, Unity compile and the macOS tour; not on devices):**
- EG-111 ten-step undo, with an additive `history` snapshot field and legacy fallback.
- Copy and restore of the garden file through the clipboard, with a two-step confirmation and per-error messages (9 keys). The QA tour checks an exact round trip.
- EG-113 first-pass `UnityEngine.Accessibility` hierarchy: 35 labelled nodes on a puzzle screen; tiles can be activated through the tap path (6 keys). Not yet tried with VoiceOver or TalkBack.

**Verification on main:** 39,466 C# assertions; project verifier and docs contract pass at 97 keys; Unity macOS build has 0 compile errors; QA tour 14/14.

**Still human-gated:** a physical-phone run (Apple signing, or an Android device or emulator), M3 sessions with 8–12 uncoached adults 45+, the founder's M3/M4 threshold decisions, and attorney name clearance.

## 2026-11-23 — Release-candidate channel (BLD-007), large board, reduced motion, text spike on iOS

- **BLD-007 verified (AI agent review):** `EVERGROVE_BUILD_CHANNEL=release-candidate` produces a non-development player. The audit found and guarded leaks (DevScreenshot, TextSpike, VillageDiorama, LocalPlaytest call sites), and a static source guard now fails on unguarded dev identifiers. At `79d19ec`, the release-candidate iOS Simulator build has zero dev identifiers in IL2CPP metadata (positive controls present). Its screens show only player controls. The evidence packet records the UnityFramework sha256.
- **EG-112 large board:** a persisted Standard/Large preference with no new keys. On the iOS Simulator, the large 3×3 board fills the width and the Undo row stays visible. The 4×4 case is not checked.
- **EG-114:** the audit found no decorative motion in store builds; the dev 2.5D miniature motion stays off by default. Unity 6000.3 exposes no OS reduce-motion API.
- **iOS QA tooling:** `DevArgs` reads dev options from `SIMCTL_CHILD_*` environment variables, because Unity iOS doesn't expose launch arguments. The 14-step QA tour and the EG-006 text spike now run on the iOS Simulator; the macOS session was inactive, so window rendering stopped there.
- **EG-006 finding:** legacy uGUI on iOS shows Arabic, Persian and Hindi plausibly but Hebrew reversed. Bengali, Tamil and Thai have detached vowel signs. The spike's UI Toolkit `languageDirection=RTL` setup reverses text, including Latin, and doesn't fix the Indic scripts or Thai. Keep every complex-script locale out of shipping builds (allowlist is English only). The next spike needs Advanced text enabled globally and properly licensed (e.g. OFL Noto) font assets, which is a third-party asset decision.
- **Checks on main:** 39,466 C# assertions; verifier, source guard and docs contract pass (97 keys); Unity compile 0 errors.


## 2026-11-23 — Mosselora release-candidate builds and publication blockers

**Git:** `git fetch origin` completed. `HEAD` and fetched `origin/main` are both `20ea0a69541c72bc2a2091de08c13103f8f524d3`; no upstream commit was pending. Existing publishing edits were preserved.

**Native builds:** Unity 6000.3.0f1 with Android Build Support and iOS Support imported the project. The C# suite passed 39,466 assertions; the static project verifier passed with 97 keys across 29 offline locale catalogs; the local preflight/provenance contract suite passed 15 tests. A release-candidate iOS Simulator project compiled with Xcode and signing disabled, then launched as `com.streamentry.evergrove` version 1.0.0 on iOS 26.5 Simulator. A separate arm64 device-SDK Xcode app compiled with version 1.0.0 / `CFBundleVersion=1`, but `codesign` confirms it is unsigned. A release-candidate Android AAB validated at package `com.streamentry.evergrove`, version 1.0.0/code 1, min SDK 25 and target API 36. After removing Unity's Android Debug signature, the AAB verified with exactly one Mosselora Play upload signer. SHA-256: `dc5ac1cd78c6655e310e5e292257d05b987b8bda6d3268900fbade404a47be9b`. The signing key remains outside Git, with its password in Keychain.

**Emulator behavior:** a universal APK derived from that signed AAB installed and launched on the read-only Android 16/API 36 `GameFoundryKakuroProof` AVD. Touch input solved chapter one, selected the shade-tree consequence, reached chapter two with the tree-anchored tile, and a swipe exposed the lower controls. A force-stop/relaunch preserved chapter two and the selected tree. With airplane mode on and no default network, the app still solved chapter one and saved the tree choice. The signed AAB manifest declares `android.permission.INTERNET`; a controlled packet capture was not performed, so this does not establish that the app makes no network requests. These are emulator results, not physical-phone or 45+ player evidence. Actual-build screenshots and preview clips are in `games/evergrove/docs/qa/ios-simulator-release-candidate-2026-11-23/` and `games/evergrove/docs/qa/android-emulator-release-candidate-2026-11-23/`.

**Cloudflare:** `api.mosselora.com/healthz` returned HTTP 200 with `status: prototype` and `gameplaySync: false`; unauthenticated `/api/world` returned HTTP 401 `CAPABILITY_REQUIRED`. The API remains a nonintegrated tree-growth prototype. The Astro site built with Node 24.22.2; its separate preview Worker at `https://mosselora-site-preview.4rqnf2gvxf.workers.dev` serves home, privacy, support, preview and generated ImageGen key art with HTTP 200. Public DNS for `mosselora.com` resolves to Cloudflare A/AAAA and nameserver records, but HTTPS still times out. The earlier automatic-review rejection for attaching a route that could replace apex traffic remains unresolved; no apex change was attempted.

**Promotional art:** ImageGen produced one text-free 1672×941 landscape village-stream key-art concept. SHA-256: `597c9077d945b40cc852719e397b7cdce93c37f8b0639b2fad17c23de057fbe1`. It is saved at `store/assets/evergrove/concepts/mosselora-promo-key-art.png` and used by the Astro preview. It remains promotional concept art, not a gameplay capture.

**Localization:** `descriptions-draft.tsv` now accounts for all 30 roster rows. It contains 25 machine-generated locale descriptions, the English source row, and explicit proxy/unsupported rows for the remaining roster entries. Zero translations have native review. Apple and Google locale codes were checked against their current official localization lists; Hausa and Nigerian Pidgin have no exact listing locale in either, Egyptian Arabic uses the shared Arabic MSA locale, and Wu Chinese uses the shared Simplified Chinese locale as a proxy.

**Store state:** App Store Connect has Apple ID `6838015597`, Bundle ID `com.streamentry.evergrove`, Mosselora name, Games/Puzzle category, and automatic release after review selected. Version 1.0 is still Prepare for Submission; no binary was uploaded or submitted. The ASC tab is at login and local Xcode has zero valid signing identities. Google Play has no Mosselora record; the package availability check passed and the Create App form is staged with English (U.S.), Game, Free and automatic protection. The account-owner policy and U.S. export-law declarations remain unchecked; the app record was not created. No platform upload, review submission, approval or verified public availability occurred.

**Evidence limit:** the new export, signing, launch, touch, screenshot and video results close the first local native-build loop. They do not prove physical-device behavior, VoiceOver/TalkBack, 45+ usability, legal title clearance, Play owner declarations, store approval or public release. The name `Mosselora` remains a preliminary working name pending legal clearance.

## 2026-11-23 — First Android runtime: blank-screen bug found and fixed

The user approved the Android emulator. Unity's bundled SDK gained the emulator and an Android 15 arm64 `google_apis` image (SDK licenses accepted with the user's approval). The AVD and all build output now live on the SSD, after the system disk filled up with scratchpad builds; about 9.4 GB of the agent's own regenerable outputs were deleted.

**Bug:** the first APK ran its game loop but drew nothing. logcat showed `Could not produce class with ID 115` (stripped native class) from `EvergroveApp.Label → TextSettings.GetCachedFontAsset → FontAsset.CreateFontAssetOSFallbackList`. Every label threw. **Android players would have seen a blank screen**, and no earlier check could have caught it.

**Fix:** `EvergroveBuild` sets `PlayerSettings.stripEngineCode = false`, and ProjectSettings is committed to match. After the fix, chapter 1 renders board-first with zero class-115 errors, and six real `adb input tap` events solve it and reach the choice screen. Cost: the APK grows from about 44 to about 68 MB. A `link.xml` that keeps stripping is a later optimization. Evidence: `games/evergrove/docs/qa/android-emulator-2026-11-23/`.

**Still open for M2:** physical Android and iPhone devices, TalkBack/VoiceOver, and Android save interruption.


## 2026-11-24 — Primary audience widened from 45+ to 40+

**Decision (founder direction):** the studio's default primary audience is now adults **40 and older**, replacing 45+. `AGENTS.md`, Mosselora's vision, design system, handbook and specs were updated. Recruitment bands are now 40–49, 50–64 and 65+.

**What did not change:** research recorded before this date keeps its original 45+ / 50+ labels and file names, because that is what it measured. The U.S. evidence still covers adults 50+ only; **40–49 demand and fit are `unknown`**, never inferred from the 50+ data. Other games' READMEs still say 45+; updating them is a separate, mechanical follow-up.

**What would change our mind:** first-session results where 40–49 players behave very differently from 50+ (for example, finding the pace too slow or the help too explicit) would argue for a band-specific default rather than one 40+ design.

## 2026-11-24 — Mosselora store-release checkpoint

Current `HEAD` and `origin/main` are `47ad558274a37c2fd78840be6e4247c5bbecf027`; Unity code last changed at `9be7c2d`. The pure C# suite passes 39,931 assertions on .NET SDK 8.0.428; the project verifier passes 101 keys across 29 offline catalogs. Current code includes the painted UI, Bengali visual ordering and a research-only return diary compiled behind Editor/Development guards. The measurement decision proposal remains unapproved and gates new store builds, store listing changes and App Privacy/Data Safety answers on founder approval and privacy review. The signed Android AAB (version 1.0.0/code 1, min SDK 25) and iOS build-1 captures predate current source and are not release candidates. No build from current code has completed.

**Editor blocker:** the installed Unity 6.3.0f1 bundle is present with both mobile modules, but `codesign --verify --deep --strict` reports that its signature was modified. Unity CLI 1.0 finds no registered Editors and rejects the app bundle. I did not bypass its signature/bundle-ID validation. The user must authorize that exact bypass or provide a verified Hub-managed Editor before a current-source build can proceed.

**Web/store progress:** after the user explicitly requested apex configuration, the production Worker `mosselora-site` was deployed to `mosselora.com/*` (version `f8c23470-8134-4aee-adbf-6a6a3411f2ea`). Prior GET checks returned HTTP 200 for the homepage, support, privacy, preview and promo-art paths; the support page displays `hi@mosselora.com`. App Store Connect version 1.0 retains automatic release after review; Support and Marketing URLs use `mosselora.com`. Its submission checklist still requires Content Rights, age-rating answers, mobile privacy URL, build, Admin App Privacy completion and review contact fields. In Play Console, Games/Puzzle and the public support email/website are saved. The English Mosselora title and descriptions, 512×512 app icon and ImageGen-derived 1024×500 feature graphic are saved on the default listing draft. Current-build phone screenshots are still missing. Play setup is 7/12 complete; no store upload, submission, approval or public availability is verified.

**Current console readback (2026-11-24):** the signed-in App Store Connect Apps list shows Mosselora, Apple ID `6838015597`; version 1.0 is `Prepare for Submission` and the automatic-release-after-review option remains selected. The version page requires Content Rights, age-rating answers, a mobile Privacy Policy URL, a selected build, Admin completion of App Privacy, and first name, last name, email and phone for App Review. The Google Play Home row shows Mosselora as `Draft`, app ID `4974796025760682569`, package `com.streamentry.evergrove`. Dashboard reads 7/12 setup tasks complete. Saved: sign-in not required, no ads, 18+ target audience, no government/financial/health features, Games/Puzzle category, contact email/website, English listing text, icon and feature graphic. Still incomplete: privacy policy, IARC rating questionnaire/terms, Data Safety, trader status, and listing screenshots. No current-build screenshots or video are uploaded. Nothing is uploaded as an app binary or submitted for store review.

**Current technical/privacy gates:** fetch found no changes beyond local `HEAD`; `main` and `origin/main` are both `47ad558274a37c2fd78840be6e4247c5bbecf027`. The .NET 8 suite passed 39,931 assertions; the documentation and project verifiers passed. Source-level mobile privacy review is recorded in [mosselora-privacy-source-review-2026-11-24.md](../../games/evergrove/docs/qa/mosselora-privacy-source-review-2026-11-24.md); final binary and network review remain open. Unity 6000.3.0f1 is present at `/Volumes/SSD/Unity/editors/6000.3.0f1/Unity.app`, but it is not registered in Unity CLI and `codesign --verify --deep --strict` reports an invalid signature. No current-source build exists. Xcode has zero valid Apple signing identities. Two Android emulators and two iOS simulators are booted, but no physical devices are attached. The old signed AAB is stale. BidiSharp remains outside the shipping allowlist. The top-30 listing drafts have zero native reviews; adult-45+ and accessibility device sessions remain unverified.

**Decision:** do not upload or submit a stale build. Resolve a verified Unity installation, signed distribution identities, current-build privacy review, remaining Apple/Google setup, required owner declarations, and physical-device/product gates before any submission. Apple automatic release stays selected; Google standard publishing remains requested.


## 2026-11-24 — Mosselora release tooling recovery

Fetched and fast-forward checked main at `1e4caa2445adc8eff9aa4ebec5725448534b4a63`. The existing Unity 6000.3.0f1 Editor bundle is on the external SSD and still fails macOS signature verification. Its Android and iOS modules sit beside the app bundle. The Unity CLI repair log identifies exhausted space on the download volume, and the partial Hub package fails verification. A separate download of the official arm64 Editor package is in progress. Verify its package signature and checksum before using it. The latest .NET suite passed 39,931 assertions. Xcode reports zero valid signing identities.

Live checks returned HTTP 200 at the Astro site apex and API health endpoint. The API still reports prototype status with gameplay sync disabled. No signed build or store submission was produced by this recovery attempt.

## 2026-11-25 — Mosselora: build toward the end-game village now

**Decision (founder):** build Mosselora toward its end-game version, a calm village the player grows and shapes, now rather than after M4. Taste is the top priority, and the goal is to release a finished, polished game. Full design: [`games/evergrove/docs/specs/proposals/endgame-village-2026-11-25.md`](../../games/evergrove/docs/specs/proposals/endgame-village-2026-11-25.md).

**What changes:** a 200-item garden catalogue (50 trees, 50 flowers, 100 things to build or mend; I171), then an almanac, a pannable garden map where the player places what they earn, neighbours who walk among the player's things, and generations of gardens down a valley. The water puzzle stays the core verb.

**What we know:** the core loop, save system and painted UI exist, verified on simulators and an emulator. No adults 40+ have played any version. No physical-device test has been run.

**Unknown:** whether players value the village over a pure puzzle (M4), whether placement is easy for players over 40 (M3 probe), and the device performance of a tile map.

**Why:** founder direction. Three fixed choices (tree, bridge, flowers) and a fixed painting were judged too thin for daily play, and an open, shapeable garden is the end-game the vision describes.

**Opposing case:** the specs hold village expansion until M4 proves the living world beats a puzzle-only control on identical boards. Building the village first risks large effort on a layer players may not value, and puzzle dilution.

**Stop condition:** if M4 shows players prefer Puzzle Only, or M3 shows placement confuses players over 40, cut the village back to what players use.

**Next observable action:** E1, the catalogue live with painted icons (I172–I173), verified on simulator tours.

## 2026-11-25 — Mosselora iOS build 2 delivered to Apple

Xcode exported the App Store distribution package for version 1.0.0 build 2 from source `84c0592`. The upload log reports successful delivery and the start of Apple processing at 07:58 ICT. Processing completion, review submission, approval and public availability remain unverified. Automatic release after approval was last observed selected in App Store Connect. Android build 2 passed bundle validation and completed the first puzzle on the Android 16 emulator. After selecting a shade tree, the game displayed Rainwater as the next level and preserved the tree choice and next level after a force-stop and cold launch. See [Android build 2 captures](../../games/evergrove/docs/qa/android-build-2-2026-11-25/). Upload-key signing, physical-device QA and Play submission remain open.

App Store Connect processed build 2. The build was attached and saved on the public iOS draft, with automatic release after review still selected. Apple blocked Add for Review pending content rights, age rating, privacy policy URL, copyright, reviewer contact, and iPhone and 13-inch iPad screenshots. A release source iOS Simulator export was started for capture. Play Console showed 7 of 12 setup tasks complete, Managed publishing off, and Send app for review disabled. Neither release has been submitted.

Google Play accepted two 1080 × 1920 screenshots from Android build 2: the garden after restoring the shade tree and the Rainwater puzzle. The default listing now reads Ready to send for review. This status applies to the listing only. The app still needs the signed bundle and remaining declarations.

The release-source iOS Simulator build ran on iPhone 17 Pro and iPad Pro 13-inch. App Store Connect accepted both original-size screenshots, and a new Add for Review check cleared the screenshot requirements. Content rights, age rating, copyright, review contact, and App Privacy still block review. Android and iOS captures are under `store/assets/evergrove/1.0/`.

## 2026-11-25 — Mosselora latest-source release recheck

Fetched `origin/main` after checking the dirty worktree, then fast-forwarded `main` from `84c0592` to `8c9e543`. The local decision log, release ledger, Android QA captures, Apple/Google screenshot captures, and `tools/batch_i112_i130.py` were preserved. The upstream update contains the later I112–I165 UI and art changes. The iOS build 2 in App Store Connect was built from `84c0592`; it predates the current source and is not the candidate to submit.

`python3 tools/verify_evergrove_project.py` passes at `8c9e543`: 121 runtime keys across 29 offline catalogs, with the development-only source guard passing. The 15 existing Python preflight/provenance tests pass. The .NET 8 domain tests were not run because no `dotnet` executable is available in the current shell or usual local SDK paths.

The installed Unity 6000.3.0f1 app exists on `/Volumes/SSD`, but the original and staged Editor bundles both fail `codesign --verify --deep --strict`. Unity CLI `editors verify` does not recognize it as an installed editor. `unity self-update --check` failed with `ENOTFOUND`; the partial official package is not verified. No signature-validation bypass or modified Editor was used. The current source has no Unity build, signed Android AAB, or signed iOS build. Xcode currently reports zero valid code-signing identities. The retained Android AAB (SHA-256 `5ca5110113fa381e31f9ac7c0d163b5ff9c1897b80a9f27aab7b211442431f22`) verifies as signed by `Android Debug`, not the Play upload key, and must not be uploaded.

A local-only Unity smoke was attempted in a clean temporary clone at `/private/tmp/game-foundry-current-8c9e543`, pinned to `8c9e543`. The first sandboxed run could not create Unity Package Manager's local IPC socket. The escalated retry started Unity 6000.3.0f1 but never initialized licensing: the log reports missing `com.unity.editor.headless`, repeated Licensing Client launch, and no smoke success marker. The run was interrupted after the license-client retry loop persisted; no build or M1 evidence was produced. Its log remains at `.evergrove-evidence/8c9e543e3e14863084768d0cff2c38a79146e79a/smoke/run-n7fH9d/unity-editor.log` in the temporary snapshot. The shared checkout was not changed by the probe.

Further local repair checks: Unity Hub's cached 6000.3.0f1 package is only 498 MB and fails `pkgutil --check-signature`; the separate 2.19 GB `/private/tmp` package also fails checksum verification. `unity install ... --download-only --dry-run` cannot reach Unity's release service. `/Applications/Unity Hub.app` also fails strict code-signature verification, and LaunchServices returned `kLSNoExecutableErr` when asked to open it. A Hub log from 2026-11-23 reports that the existing auth token could not be read and no gateway token was available. Do not run or repair these invalid app bundles in place; a verified Hub/Editor package and owner-managed Unity sign-in/license are needed for the next build attempt.

The read-only `unity license status --format json` command timed out after 15 seconds, and `open -a '/Applications/Unity Hub.app'` failed with `kLSNoExecutableErr`. The isolated smoke's licensing loop was stopped; no user credentials or license data were read or changed.

## 2026-11-25 — Store Browser login handoff

On the resumed goal, the Browser runtime became available. Its tab list contained: App Store Connect `/login?targetUrl=%2Fapps&authResult=FAILED`, a Google Play Console developer Home tab, and an App Store Connect version page. I invoked the Browser tab's `requestUserTakeover()` on the failed Apple login tab and stopped console work as instructed. No page content was read and no login/2FA response was entered; Play Console content remains unverified.

## 2026-11-25 — Apple first-market availability configured

After the account session became authenticated, App Store Connect Apps showed the existing Mosselora record (Apple ID `6838015597`), version 1.0 `Prepare for Submission`. The attached build remains build 2 from older source `84c0592`; the version release option “Automatically release this version” is checked. App Privacy is blank and the Privacy Policy URL is unset. Review contact and copyright fields are blank.

The App Store price schedule remains set for 175 territories, but App Availability had not been configured. Following the repository's U.S.-English first-market plan and with zero native-reviewed translations, I configured **United States only**, available when the app is released. Readback showed “Availability (1 Country or Region)” and “United States — Available on App Release”; China Mainland was not available. This did not publish the draft. The user later requested all countries free; that setting is recorded in the newer Nov 26 section below.

The App Review contact name/email and no-sign-in notes are now saved on version 1.0; the reviewer phone remains blank. App Privacy, Privacy Policy URL, copyright, Content Rights, and age-rating responses remain incomplete.

Google Play Console is authenticated on its Home page. Its Mosselora record (`4974796025760682569`, package `com.streamentry.evergrove`) remains Draft, last updated Nov 25, 2026. Setup is 8/12 complete; Privacy policy, Content rating, Data safety, and Trader status are pending. A country/region rollout was not set. The draft still has no uploadable current-source AAB or submission.

The current Play Publishing overview reads `Managed publishing off`; `Send app for review` is disabled. Advanced settings showed Google Play Games on PC opted in, with no PC QA or PC artifact. I opted out; readback shows the opt-in locked and no PC form-factor change in pending review. Android XR remains active and uses the mobile track by default. A dedicated XR track would still serve mobile artifacts until a separate XR release exists, so I canceled that change. Play country targeting remains unavailable until release.

Device checks found no attached Android device (`adb devices -l` returned an empty list). Four iOS simulators are booted, but CoreDeviceService timed out; no physical iPhone or iPad was observed. The previous simulator/emulator captures and their store uploads are build 2 evidence only, not current-source proof. No current-build preview video exists.

The Browser runtime initialized but `browsers.list()` returned no browser and `getDefault()` returned “No browser is available.” Codex only queued the App Store Connect and Play Console URLs; no signed-in page content or controls were available in this session. Last known store state remains: Apple build 2 uploaded/processed and attached to the draft, automatic release selected, no submission; Google listing assets/screenshots saved, Managed publishing off, no AAB upload or submission. These states were not re-read in this session.

Latest-source release remains blocked by M1/M2/M3/M5/M6 evidence and owner actions: a clean licensed current-source Unity build; physical iPhone and Android QA; the required uncoached adult-40+ study; current binary privacy and network review; store name/rights and account-owner declarations; and reviewed translations (25 mapped locale drafts exist, zero native reviews). Keep the English listing as the only eligible listing locale until native reviewers sign off. Do not submit build 2 as if it matched `6098810`.

## 2026-11-25 — Mosselora family goal: generations play together

**Decision (founder direction):** Mosselora should be a game grandparents, parents and children can play together gently. A key goal is to help older players feel less alone by connecting them with family, not only to entertain them. The adult 40+ stays the primary player; children and parents are co-players. Recorded in [vision §1a](../../games/evergrove/vision.md#1a-family-play-connection-not-only-entertainment), the [product brief](../../games/evergrove/docs/PRODUCT.md), both AGENTS.md files, and a new [family co-play session](../../games/evergrove/docs/playtest/family-coplay-session.md) protocol.

| Preflight | Answer |
| --- | --- |
| Truth | Founder direction. No observation of families playing Mosselora exists. AARP's self-reported data says 79% of U.S. gamers 50+ usually play alone, which cuts against co-play as common use |
| Owner | Founder |
| Delete | No feature is added. The first test uses the current build on one device |
| User value | Families get a calm, failure-free thing to do and talk about together; the choice after each puzzle is a natural "which one should we pick?" moment |
| Audience fit | Unknown. Age bands to observe: adult 40–49 / 50–64 / 65+ with child 6–8 / 9–12 |
| Falsification | 5 family groups play the current build on one device without instruction. Fewer than 2 of 5 playing together at all drops co-play as a near-term feature |
| Stop | Children disengage within 5 minutes in 3+ groups, or adults take over in most groups: redesign before building co-play |
| Evidence | Anonymized counts in the playtest results template |

**Rules added:** cooperative only (no scores between players, no timers), one device first, no chat or strangers, no guilt or obligation, no data about children. **No claim** that Mosselora reduces loneliness, improves wellbeing or helps memory without evidence; store copy describes only what the game lets people do.

**Open owner decision:** Google Play's target audience is saved as "18 and over". If a build is designed for children to play, the account owner must decide the Families-policy and Apple age-rating declarations before submission. Current store copy makes no family or loneliness claim and was left unchanged. The M3 solo study and the release blockers above are unchanged.

## 2026-11-25 — Mosselora family circle: remote play with optional sign-up

**Founder direction:** family members who live far apart should be able to play together. That needs a backend and a simple sign-up, used only by people who want to connect. Everyone else plays alone and offline with no account. Recorded in [vision §1a](../../games/evergrove/vision.md#1a-family-play-connection-not-only-entertainment) with the minimum requirements: adult-only sign-up (email link or Sign in with Apple / Google, display name only), children as adult-managed profiles with no child data, in-app account deletion, invite-only family circles, asynchronous turn-based play first, and the solo game working when the server is down.

**Not built and not authorized yet.** The existing backend is a demo, and the specs list backend, accounts and multiplayer as not authorized. Before building, run a cheap test of whether families keep playing apart (for example, trading puzzle codes by message for two weeks). Adding accounts changes the App Privacy and Data safety answers, which currently describe no account and no data collection, so the first store release should stay offline-only unless the owner decides otherwise.

## 2026-11-25 — Latest authenticated store fields

App Store Connect App Information confirms Games/Puzzle, but Content Rights and Age Ratings are not set up. App Privacy and the mobile Privacy Policy URL are blank. Review-contact name/email and no-sign-in notes are saved; the phone remains blank. Build 2 from `84c0592` is attached, with automatic release checked, but current main is `6098810` and requires a fresh signed build.

Google Play Publishing overview confirms Managed publishing off and `Send app for review` disabled. The 8/12 setup card lists Privacy policy, Content rating, Data safety, and Trader status as pending. PC opt-out is saved; Android XR remains on the mobile track. No current-source bundle or submission exists.

## 2026-11-25 — Main and live store recheck (6098810)

Current `main` and `origin/main` both resolve to `609881069aea4fc4af5b7b82709a8d3be46c5b23` after PR #81. The diff from `8c9e543` contains documentation, playtest protocol and evidence assets only; no Unity `Assets`, `Packages`, or `ProjectSettings` runtime inputs changed. The family co-play direction is a founder hypothesis and playtest protocol, not an implemented feature or store claim.

Authenticated App Store Connect readback at that time: Mosselora Apple ID `6838015597`, version 1.0 `Prepare for Submission`, old build 2 from `84c0592` attached, “Automatically release this version” checked. Availability was then U.S.-only, set to “Available on App Release”; the user's later all-country request supersedes that setting below. Reviewer name/email and no-sign-in notes are saved; phone remains blank. App Privacy and mobile Privacy Policy URL are blank. Copyright, Content Rights and age-rating answers remain incomplete.

Authenticated Play readback: app `4974796025760682569` / `com.streamentry.evergrove` remains Draft, 8/12 setup tasks complete. Privacy policy, content rating, Data safety and trader status are pending. Managed publishing is off and `Send app for review` is disabled. Google Play Games on PC is opted out because no PC build/QA exists; Android XR remains on the mobile track by default. Country targeting is not yet settable until a track is released.

No current-main Unity build exists. The isolated smoke at `8c9e543` reached Unity but failed licensing (`com.unity.editor.headless` missing); PR #81 did not change the runtime inputs, but no successful build was produced. The retained Android AAB is Android-Debug-signed, not uploadable. Neither platform has a current-source signed build, submission, approval, or verified public listing.

## 2026-11-25 — Authenticated store status and platform settings (6098810)

The live App Store Connect Apps page confirmed Apple ID `6838015597`, Mosselora version 1.0 `Prepare for Submission`, build 2 from `84c0592` attached, and automatic release checked. App Store Information shows Games/Puzzle selected; Content Rights and Age Ratings are not set up. App Privacy is blank and the Privacy Policy URL is unset. Review contact name/email and no-sign-in notes are saved; phone remains blank. Availability is United States only, Available on App Release. App Compliance identifies the developer as a trader; China Mainland Security Assessment and AIGC rows remain Missing Info, although China is excluded.

Google Play confirms draft `4974796025760682569` / package `com.streamentry.evergrove`, setup 8/12. Privacy policy, Content rating, Data safety and Trader status remain open. Publishing overview says Managed publishing off and Send app for review disabled. Google Play Games on PC is opted out; Android XR remains active and uses the mobile release by default. Country targeting is not configurable until a track is released. No current-source signed AAB or submission exists.

`main` is now `609881069aea4fc4af5b7b82709a8d3be46c5b23`, matching `origin/main`. PR #81 adds family-goal documentation and playtest protocol only; no Unity runtime, package or player settings changed from `8c9e543`. The family co-play goal remains a hypothesis, not implemented or claimed in the store listing. The current Unity smoke still failed at licensing, so the old uploaded build 2 is not eligible for submission as the current source.

## 2026-11-25 — Public listing URLs checked

The exact U.S. App Store URL for Apple ID `6838015597` renders “The page you're looking for can't be found.” The exact Google Play URL for `com.streamentry.evergrove` renders “We're sorry, the requested URL was not found on this server.” Both public listings are therefore not available as of this check.

Unity CLI auth status now reports a stored OAuth login with `sessionState=stale`; a login refresh failed because the Unity sign-in service was unreachable. `unity license status` times out, and the Editor reports missing `com.unity.editor.headless`. No credentials or license files were read or changed.

## 2026-11-25 — Mosselora audience 7+, accounts and data collection

**Founder decisions:** Mosselora's audience is **ages 7 and up**, and the game **will have optional accounts and data collection** for the family circle. Recorded in [vision §1a](../../games/evergrove/vision.md#1a-family-play-connection-not-only-entertainment), the root AGENTS.md and the [store metadata](../../store/metadata/evergrove/en-US.json).

**Observed today:** no build or branch contains account, sign-up or data-collection code. `WorldClient` is a demo that nothing in the game calls, the backend is a demo, and the website privacy page says the site has no accounts. Store privacy answers must describe the build actually submitted, so a build without the family circle still declares no account.

**Consequences to resolve before a 7+ build with accounts ships:** the account owner changes Play's target audience from 18+ to include ages 6–8 and 9–12 (Families policy); IARC and Apple content questionnaires are answered by the owner (Apple has no 7+ tier); children under 13 need verifiable consent from a parent or legal guardian (COPPA) before any child data reaches the server; in-app account deletion, a mobile privacy policy and updated store copy ("without an account") are required. No console setting was changed.

## 2026-11-26 — All-country free store settings readback

At the user's request, App Store Connect was checked: all 175 countries/regions read `Available on App Release`, and the price matrix shows $0.00 across all 175 (read back on 2026-11-26). This does not publish the draft. China Mainland is included, while its Security Assessment and AIGC compliance rows remain `Missing Info`; the account owner must provide the required filings before that territory is released.

Google Play remains Draft with no signed bundle or active release. App pricing reads Free. Production Countries/regions now reads Targeted (178): all 177 listed countries/regions plus Rest of World. These changes appear in Publishing overview as not yet submitted for review; Send app for review is disabled. Managed publishing is off (standard publishing), PC opt-in is off, and Android XR remains on the mobile track.

Fresh Unity prerequisite check: the installed Unity 6.3 Editor still fails strict arm64 signature verification. `unity auth status --format json` returned `LOCAL_STORE_UNWRITABLE` in the workspace sandbox; `unity license status --format json` hung, as did an escalated auth-status attempt. Both hanging checks were interrupted. No credentials were entered or displayed; no build or license activation completed.

## 2026-11-26 — Mosselora API access and signing recovery

The Golden Moments App Store Connect key returned HTTP 200 for the existing Mosselora app record and the response matched the expected bundle ID. The Golden Moments Play service account obtained an OAuth token and could read Golden Moments reviews. The equivalent Mosselora query returned HTTP 403, so Mosselora release access remains unverified. Play Console is signed in and the first Android upload will use that console. No new credential files were created in the repository.

Outside the workspace sandbox, macOS found a valid distribution identity matching the existing Mosselora App Store provisioning profile. The older zero-identity result applies to the sandbox. Remote `main` remains `6098810`. The .NET suite passed 50,072 assertions and the static project verifier passed. These checks do not establish a native build. The installed Hub passed strict verification outside the sandbox. After opening it, Unity reported an active license and signed-in session. The original modular Editor failed the bundle signature check. I restored the vendor’s existing signing attributes on an isolated base Editor copy, which passed strict verification, and added the signed iOS support module and the existing installed Android module. The current-source Unity import and both release-candidate exports succeeded.

Build 3 from `5bfe306` was signed with the existing local Apple distribution identity and Android upload key. Apple validated and accepted the IPA via `altool` with API authentication. The App Store Connect API confirmed build upload `COMPLETE` and build processing `VALID`. IPA SHA-256: `1f39968dfe3ae4c3e2fe37c502ab41d3bfe569e7f7d77e9aeea86af9c2982b55`. Play Console accepted the first signed AAB, version code 3 (1.0.0), min SDK 26 and target SDK 36. The bundle persisted on the internal testing draft after reload. AAB SHA-256: `b7fe2211b39a8fb7f2dd098dd4932da631ec26742ceb1fb10c199815b26244e9`. Neither build was submitted for review or rolled out. Device QA and owner declarations remain open.

## 2026-11-26 — Store review validation

Attached iOS build 3 to version 1.0 through App Store Connect and verified automatic release after approval. Apple refused Add for Review. Its checklist requires export compliance for build 3, Content Rights, age-rating answers, a mobile privacy policy URL, and copyright. China Mainland filings are also flagged on the draft. Play Console still shows 8 of 12 setup tasks complete. Privacy policy, content rating, Data safety and Trader status remain open, and the incomplete IARC questionnaire requires acceptance of its terms. Send app for review is disabled and Managed publishing is off. Android code 3 remains in the internal draft. No review submission or rollout occurred.

## 2026-11-26 — Mobile privacy policy and review declarations

Deployed the mobile privacy policy to the existing production site and verified HTTP 200 and the mobile text at the public URL. The full static site deploy also refreshed generated preview assets; the preview and support routes returned HTTP 200. Play Console saved the privacy URL as a change pending review. App Store Connect returned to sign-in, so its privacy URL remains unset. The [owner handoff](../../games/evergrove/docs/qa/mosselora-store-owner-handoff-2026-11-25.md) records proposed declaration answers and the outstanding confirmations.

The uploaded build 3 lacks the in-app privacy link both stores require. Added a localized menu link in the shared source and started replacement build 4 from isolated commit `5b16299`. The iOS Unity export passed and the Android export began. Static checks validated 122 keys across 29 catalogs and the .NET suite passed 50,103 assertions. The build 3 binary review checked the Apple privacy manifest and Android runtime permissions. No physical device or network capture was available. Neither store review was submitted.

Apple privacy URL saved and verified via App Store Connect API. Play accepted signed build 4 (1.0.0), code 4, from isolated source `5b16299`. Removed code 3 from the internal draft and verified code 4 persisted alone after reload. Both store privacy URLs now point to the published mobile policy. The user confirmed the proposed factual answers in general and has been asked for the exact owner values and console actions. No review submission or legal attestation was made.

iOS 1.0.0 build 4 was verified, signed and uploaded through Apple API authentication. IPA SHA-256: `3aa30403a7d583b6a895be5a229232a6a95cbc0720af5d6b631fe36c6331e472`. Last API readback showed processing. App Store Connect browser sign-in was restored. No physical test device was available.

Apple subsequently confirmed build 4 upload COMPLETE and build processing VALID. Attached build 4 to version 1.0 and verified automatic release after approval. No review was submitted.

## 2026-11-26 — Owner copyright and territory choices

Saved Apple copyright `2026 Cloudjet Solutions` and verified it after reload. Removed China Mainland at owner direction and verified 174 enabled territories. Apple’s fresh submission validation requires Content Rights, age rating, App Privacy and export compliance for build 4. Play Dashboard shows 10/12 setup tasks complete, with Data safety and Trader status open. The app inherits the account’s trader status and that option is selected. The owner confirmed trader in chat and must save the app-level declaration in Play Console. No review submission occurred.

Apple age-rating answers entered from confirmed game features and content. The calculated global 4+ rating with regional exceptions persisted after reload. Kids Category remained unselected. Content Rights requires an owner attestation. Play still locks production setup and disables Send app for review. The remaining dashboard tasks are Data safety and Trader status. Fetched upstream main and verified it still resolves to `6098810`. The runtime patch in the shared checkout matches build 4’s isolated source.

Prepared and saved the complete Play Data safety form as an unpublished draft. The preview displays no data collection declared and no data shared with third parties. This prepares the owner’s review and does not complete the app-content declaration. The selected trader declaration is still awaiting owner Save. Apple still shows Content Rights, App Privacy and build 4 export compliance open. Neither platform is in review.

Saved the confirmed App Store Connect App Privacy answer as an unpublished draft. Apple’s preview displays Data Not Collected and enables Publish. The owner must publish the declaration. Play Data safety and Trader still require owner action. Neither store is awaiting review.

Build 4 export compliance preparation: the distributed iOS UnityFramework binary contains UnityTLS/Mbed TLS implementation strings, including TLS client handshake functions. The garden transfer code uses SHA-256 for integrity. Staged Apple’s standard-encryption-outside-the-OS choice in the form without filing. The owner must review the classification and complete any follow-up documentation. No store review submission occurred.

## 2026-11-27 — Build 5 and store state

Both release-candidate builds from isolated commit `b8a50f3` were produced and signed. Verified the BidiSharp MIT notice in both artifacts against the vendored license. iOS IPA SHA-256: `69805bde0ede4e7395d9ff5179dcfe322032c94b539b1de30dfa3f5b6e66df4b`. Android AAB SHA-256: `0358c8a3c62c8fa2a53c91d7389df2f958fe84d6a9ba927c6c1db10d60ae5f5c`. Neither build 5 upload is verified. Apple browser returned to login and was handed back for authentication. Play Publishing overview still shows Send app for review disabled. Data safety and trader changes are saved pending review. No store submission occurred.

## 2026-11-27 — Build 5 store upload

Android signed build 5 was accepted in Play Console, replaced build 4 in the production draft, and was saved to Publishing overview with full rollout pending. Managed publishing remains off. Play's review button is disabled by an incomplete advertising ID declaration. The build manifest contains no AD_ID permission, and an archive filename scan found no advertising SDK. The no-advertising-ID answer is the supported proposed answer. Browser auto-review rejected saving it because AGENTS.md reserves account-owner policy declarations for the owner. The pending target-audience declaration currently reads 13 and older, which must be reconciled with the founder's 7+ co-player direction before submission. iOS build 5 was validated and uploaded with altool; Apple reported PROCESSING. Build 4 remains attached until build 5 becomes VALID. No review was submitted.

Apple subsequently marked iOS build 5 VALID. Attached it to version 1.0 and verified automatic release after approval. Add for Review remains blocked by Content Rights, App Privacy, and build 5 export compliance. Play production build 5 is staged; automated check requires the owner advertising ID declaration. The pending Play audience reads 13 and older, inconsistent with the 7+ co-player direction. No submission occurred.

## 2026-11-27 — Play submission and iOS crash feedback

Play submission activity shows submission 1 In review, including production version code 5 full rollout. Managed publishing is off. Publishing overview still shows automated checks underway. Submitted target audience is 13 and older, while founder direction names 7+ co-players. TestFlight Crash Feedback lists build 4 on iPhone 15 Pro, iOS 27.2.1, on Nov 27 at 07:44; another report is build 2. The browser blocked Open in Xcode under its security policy, so no crash stack was obtained and root cause is unknown. No iOS crash fix or iOS review submission has been verified.

The supplied TestFlight crash log identifies iOS build 4 on iPhone 15 Pro, iOS 27.2.1, failing at launch with `EXC_BREAKPOINT` in `UIApplicationEvaluateRuntimeIssueForNoSceneLifecycleAdoption`. Both build 4 and build 5 IPA Info.plist files lack a scene manifest. Apple documents a launch failure when apps built with the latest SDK do not adopt scenes on iOS 27. Unity's official guidance says 6000.3.8f1 or later supports this lifecycle; the existing project uses 6000.3.0f1. An isolated commit `abf5330` upgrades the project pin and its verifier to 6000.3.8f1. Source verifier and .NET core suite passed, 50,103 assertions. Official editor installation is in progress; no fixed iOS build or runtime validation exists yet. Only iOS 18.5 and 26.5 simulators are installed, so validating the reported iOS 27 crash requires the owner's iOS 27 device or a suitable simulator runtime. Play submission activity shows submission 1 In review, including production build 5. Review and approval are distinct.

## 2026-11-27 — iOS scene lifecycle repair candidate

The TestFlight build 4 crash log shows `EXC_BREAKPOINT` at UIKit's `UIApplicationEvaluateRuntimeIssueForNoSceneLifecycleAdoption` during launch on iOS 27.2.1. Apple documents that iOS 27 requires the scene lifecycle for apps built with the latest SDK. Unity identifies 6000.3.8f1 as the first 6.3 LTS version with scene lifecycle support. Build 4 and build 5 lack `UIApplicationSceneManifest`.

Fetched latest upstream main `d51cf14` and merged it with the privacy link and BidiSharp notice in an isolated checkout. Updated the Unity patch and explicit editor validator in isolated commit `751342d`. The shared checkout remains unchanged by the merge. Verified the Unity base editor signature and its official signed iOS module. The source verifier passed, as did 72,001 .NET assertions. Unity exported the iOS candidate, whose Xcode project has `UnityScene` and the scene manifest. A signed build 6 was produced; the signed IPA contains the scene manifest and BidiSharp notice. SHA-256 `dd8f2bdec2b20e881d13ab77aa008f376a020b2c07950cc8a902c22d623957c3`. Apple altool validation and upload succeeded; build processing is pending. An iOS 26.5 simulator build installed, launched and displayed the game, with its process still running. No iOS 27 simulator or physical device was available; the original crash is not yet verified fixed on iOS 27. Do not submit build 5 as the fix.

Apple build 6 later reached processing state VALID, upload COMPLETE. An attempt to associate it with the existing internal TestFlight group returned HTTP 422 ENTITY_UNPROCESSABLE. Readback shows `usesNonExemptEncryption` unset; the exact cause of the 422 is not confirmed. The temporary isolated checkout and build artifacts were cleared by the environment after upload. The shared checkout still contains the earlier privacy and license changes, but the isolated Unity upgrade commits are no longer available in that temporary directory. App Store Connect browser returned to authentication and was handed to the user. No device launch on iOS 27 is verified.

## 2026-11-26 — Understand a garden choice before applying it

**Decision:** improve the existing offline choice loop. The founder requested a source review and practical improvements toward Mosselora's vision. This change makes the current consequence inspectable; it does not authorize family accounts, multiplayer or a store release.

| Preflight | Answer |
| --- | --- |
| Truth | The previous choice cards hid consequence copy from sighted players, committed immediately, and pictured the next protected pipe as a hardcoded straight channel. The actual next tile can be a corner, differently oriented, wet or dry |
| Owner | Founder request; the existing design contract requires truthful, reversible inspection before confirmation |
| Delete | Reuse the real model transition, existing art and localization. No extra garden simulation, choice currency, profile, network call or save schema |
| Player value | Compare tree, bridge and flowers; see the resulting garden fact and actual next protected pipe; apply one choice explicitly |
| Audience fit | Not validated with people. Readable labels, scaled text, equally weighted choices and no timed decision preserve the adult 40+ direction. The pause may also support side-by-side discussion, but that is unobserved |
| Strongest objection | An extra tap and explanation may slow an otherwise simple choice |
| Cheapest disproof | Observe five uncoached players or family groups at their first decision. Ask afterwards what changed and when it was saved |
| Stop | Simplify the preview before expanding it if at least 2 of 5 think inspection already saved the choice or cannot find the apply action |

**Implemented:** a temporary preview clones the current snapshot, restores it, and calls the same WorldModel.Choose operation as committed play. Comparing, closing or losing the preview on relaunch leaves the solved garden unchanged. Only the explicit apply action reaches the existing save and choice-recorder path. The next pipe uses its real shape, orientation, water reach and position; the final choice has no next-stream promise. The preview is invalidated when the world, round or solved state changes. Old apply/close callbacks cannot act on a newer selection; accessibility activations reject inactive controls and respect interaction locks. All three new strings exist in English and 29 draft catalogs (124 keys total); draft status is unchanged.

**Reliability fix:** clipboard garden restoration now accepts standard leading JSON whitespace. A regression first reproduced rejection, then proved restoration retains choices, the unfinished board, undo history and a staged hint. Checksum and malformed-input rejection remain in place.

**Observed verification:** .NET 8 Release reports 51,611 assertions, including 1,416 preview assertions; the save-journal suite separately reports 1,626. All 45 source-derived puzzle profiles match. The static Unity project contract and 65 Python tool tests pass. The preview/commit comparison covers all 12 chapters × 3 choices and legacy v1/v2/v3 saves. Roslyn syntax parsing under five Unity/test define sets passes; this is not Unity type checking.

**Not run:** licensed Unity import/compile, the expanded native QA tour, physical iPhone/Android tests, native-reader review or human sessions. Historical simulator/emulator evidence predates this source change. The existing native tour now checks preview, comparison, cancellation, stale callbacks, actual committed state, the final chapter and the matched control; it must be executed on the new build.

**Next real action:** build this source with the pinned licensed Editor, run the updated tour and phone accessibility/layout checks, then use the existing solo and family session kits. Continue to M4 only after comprehension passes. Rotation-based variants still preserve the connection graph: this change improves clarity, not ecological depth, retention or validated demand.

## 2026-11-27 — Mosselora iteration run I189–I288: depth, family, seasons, notes

**Decisions (founder directions in this run):** earned seeds and a nursery; offline neighbours who notice (a language-model companion later); five more puzzle kinds (nonogram, seed sudoku, garden plots, one-path stream, mahjong solitaire) chosen from the studio's own demand research; four seasons by progress; light that follows the player's local time (cosmetic only); on-device "garden notes" (local notifications) under the rules in [notifications-2026-11-27.md](../../games/evergrove/docs/specs/proposals/notifications-2026-11-27.md). Each iteration is documented in `docs/improvements/I189` to `I288`.

| Preflight | Answer |
| --- | --- |
| Truth | Pure C# tests (349,144 assertions) and simulator tours in portrait, landscape and 150% text for every merged batch. One real local notification delivered on the iOS simulator (I284). No physical device, no player aged 40+, no family group has played any of it |
| Owner | Founder, through the iteration goal and in-session requests |
| Delete | Notifications stay off until the player says yes; Android notes are not built; translations of new text wait for native review (consumer builds ship English only) |
| User value | A daily loop with a horizon (seeds, nursery, river, seasons), eight puzzle kinds that teach themselves, family letters, help, gifts and playing together on one device, and a neighbour's true note at the player's own hour |
| Audience fit | Unknown: none of it has been observed with adults 40+ |
| Falsification | Uncoached adults 40+ (bands 40–49, 50–64, 65+) play three days: do they return without the note, choose puzzle kinds by the marks, understand the drops on the mist, and find any note pressuring? |
| Stop | Notes: more than one in four who turn them on turn them off within a week, or any note called pressure. Puzzles: a kind skipped by most players moves to later editions or is cut |
| Evidence | The research diary and session notes; per-iteration docs |

**Never claim** from this run: retention, wellbeing, memory or connection effects; that families play together; that the 29 non-English catalogs cover the new text.

## 2026-11-27 — Mosselora run 3, I338–I437: the garden's logic, a living world, a quieter companion

**Decisions (founder goal for this run):**

- Icons over text, with smart navigation.
- An immersive, easy, everyday garden with deep logic behind simple play.
- An open, endless river; family visits and sharing.
- Simple, logical economics.
- A subtle companion.

In-run requests added seasons (snow, autumn, spring, summer) and weather days (rain, snow, wind, hot, fair). Each iteration is documented in `docs/improvements/I338` to `I437`; the systems are specified in [garden-logic-2026-11-27.md](../../games/evergrove/docs/specs/proposals/garden-logic-2026-11-27.md).

| Preflight | Answer |
| --- | --- |
| Truth | Pure C# rules with deterministic tests (about 1.28M assertions with the parallel economy session's). Release-define and development compiles against the Unity 6.3 player assemblies. iOS simulator tours in portrait, landscape and 150% text for each batch (b311–b330); from I433, logged object counts for effects too faint to trust by eye. No physical device; no adult 40+; no family pair |
| Owner | Founder, through the run goal and in-session requests. Economy items 71–80 belonged to a parallel session; seed changes route through its ledger |
| Delete | Weather never touches the economy. Device-only companion facts never enter the garden file. New English lines aren't translated; consumer builds ship English only |
| User value | Seven good neighbours with written rules; seven visitors who come for reasons; a gift each season; pollination; family gift ideas on visits; a river and sky that change with the hour and weather; fewer words on screen |
| Audience fit | Unknown: `age fit not validated` |
| Falsification | Uncoached adults 40+ (bands 40–49, 50–64, 65+), solo and with a family member, for a week on a physical phone. Do they find a pairing unaided, notice a visitor, understand a season's gift? Does any notice feel like pressure or surveillance? |
| Stop | If most find no pairing in a week, the glow comes unasked. If the ways page reads as homework, cut it to the at-a-glance row. If the hour notice feels like being watched, remove it (I419) |
| Evidence | Session notes and the research diary; per-iteration docs; tour logs (`MOSSELORA_ATMOSPHERE` lines) |

**Fixed on main along the way:** release builds failed to compile (`qaRainbow` was declared inside the dev-only block; #118). The docs contract counted 124 keys after the privacy link added a 125th (#111).

**Never claim** from this run: retention, wellbeing, memory or connection effects; that families play together; that adults 40+ find the pairings, visitors or seasons understandable.

## 2026-11-27 — Mosselora smart companion SC01–SC07: true facts, one ranker, reviewed lines

**Decision (founder goal):** make the neighbours "smart without trying too hard", as specified in [smart-companion-2026-11-27.md](../../games/evergrove/docs/specs/proposals/smart-companion-2026-11-27.md):

- a fact feed reads true facts from the save;
- a deterministic ranker picks the one worth saying, with memory kept on the phone only;
- a hand-written line bank supplies the words, and the founder reviewed every line before it shipped.

No model runs on the phone; nothing is generated at runtime.

**Built** (logs `docs/improvements/SC01`–`SC07`):

| Iteration | What |
| --- | --- |
| SC01 | The feed. Proven to reproduce the old `NeighborSays` word for word over 17,487 cases |
| SC02 | The ranker and memory (3-day key rule, one remark per visit, anyone-facts shared out) |
| SC03–SC04 | 912 English lines, with 20 wordings per kind and voice and never the same wording within 14 days, and four new kinds said once (a hope's progress, the season turning, the season's yield, a pairing found) |
| SC05 | The home arrow, the cottage card's order and one soft mark follow the same ranker |
| SC06 | Another language speaks only a reviewed bank and its own slots, else its story lines, unchanged |
| SC07 | The [companion week kit](../../games/evergrove/docs/playtest/companion-week-kit.md) |

| Preflight | Answer |
| --- | --- |
| Truth | Pure C# tests, more than 2.2M assertions in all. Line rules are enforced twice (`tools/evergrove_line_bank_check.py` and `LineBankTests`). iOS simulator builds and tours in portrait, landscape and 150% text, with a scripted week of each neighbour talking. No physical device; no adult 40+; no family pair |
| Owner | Founder: "a real AI companion in subtle ways", "smart without trying too hard"; every shipped line approved by the founder |
| Delete | No runtime model, backend, account, network, new save fields or free text. Machine translations are never shipped; no locale bank exists yet |
| User value | Neighbours who notice what you did, in many true wordings, without repeating themselves; the home screen points at what is waiting for you |
| Audience fit | Unknown: `age fit not validated`. Lines are written for adults and kept suitable for a 7-year-old co-player (checked by rule and by the founder) |
| Falsification | The companion week kit: 5–10 adults 40+ for a week, by band. Ask what the neighbours said |
| Stop | Testers can't recall any remark, call the neighbours repetitive within the week, or prefer the old lines. Any report of pressure or surveillance removes or softens that kind |
| Evidence | Day-7 call notes, verbatim, by band, with denominators; recorded here when run |

**Not yet run.** Thresholds are `TO PREDECLARE` and must be fixed here before recruiting.

**Never claim** from this work: that the companion improves wellbeing, memory, connection or loneliness; that adults 40+ remember or like the remarks; that it works in other languages.


## 2026-11-28 — Mosselora v1 design calls for items 11, 38 and 91 (founder)

The founder chose the recommended option in each case, so the checklist items that were waiting can be built:

| Item | Question | Decision |
| --- | --- | --- |
| 91 | 9×9 boards can't fit 48 pt cells on a phone | Pinch or tap to zoom a region of a dense board so cells reach 48 pt, with a clear way back; the whole board stays visible by default |
| 91 | The landscape puzzle card is scaled to fit, so its controls fall to 29–45 pt | Redesign landscape: the board on the left, controls and trays in a full-height panel on the right at full size |
| 38 | What "a stream path waters the plots" and "plots feed the nursery" do in play | Visible only: a short on-screen cause and a small cosmetic change. No new seeds and no free nursery stock; the economy spec is unchanged |
| 11 | Fold hub controls into the corner fan | The letterbox and the basket move into the fan; the seed count stays visible as a read-only label, not a button. The hub at rest shows at most two controls (menu and play) |

| Preflight | Answer |
| --- | --- |
| Truth | Simulator touch scan (batch 23 log) for 91; source audit for 38; the hub's resting controls counted for 11. No player evidence |
| Owner | Founder, 2026-11-28 |
| Delete | 38 adds no economy change; 11 removes resting buttons rather than adding any |
| Audience fit | `age fit not validated`. Zoom, a side panel and fewer resting buttons are aimed at reach and readability for adults 40+ |
| Falsification | 40+ players can't find the letterbox or basket in the fan, or zooming a board costs more taps than it saves |
| Stop | Either of those in a playtest reverts that change |

## 2026-11-28 — Mosselora v1 design calls for items 41, 94, 13 and 5 (founder)

The founder chose the recommended option in each case:

| Item | Question | Decision |
| --- | --- | --- |
| 41 | The river stops at about 58 stretches, because stretches come from distinct first solves | Once there is nothing new left to solve, replays slowly open the river (one stretch per N replays, about 8). It still opens through play, never payment, and the economy is unchanged |
| 94 | The menu button sits top-left, out of one-thumb reach on a large phone held upright | Move it to a bottom corner, with its fan opening upward. The top of the screen keeps only read-only things (the seed count) |
| 13 | A tapped thing's play token appears at the bottom of the screen | A tapped thing shows Play and Move tokens (52 pt) right beside it, and the bottom play token goes |
| 5 | Remember the hand | Ask once, on the first long-press, whether the player is left- or right-handed; mirror the corner menu, fan and trays. It can be changed in settings |

| Preflight | Answer |
| --- | --- |
| Truth | Source audit (the solve-derived stretch count), the simulator reach of the top-left menu, and the current tap path for a thing. No player evidence |
| Owner | Founder, 2026-11-28 |
| Delete | 13 removes the bottom play token; 94 moves a control rather than adding one |
| Audience fit | `age fit not validated`. Reach and handedness are aimed at one-handed use by adults 40+ |
| Falsification | 40+ players miss the moved menu, or the beside-the-thing tokens get in the way of planting |
| Stop | Either of those in a playtest reverts that change |

## 2026-11-28 — Mosselora v1 item 94 follow-up: closing and the puzzle card's controls (founder)

The batch 30 reach audit found these out of one-thumb reach in portrait: every sheet's close button (top-right, and iOS has no back gesture), the puzzle card's top row (hint, take turns, send), and the fan's last token. The founder chose:

| Question | Decision |
| --- | --- |
| How should closing become reachable? | Any sheet or card also closes with a downward swipe anywhere on it, as iOS sheets do. The top-right button stays the one visual back position |
| The puzzle card's top-row controls in portrait | They move to a row at the card's foot, beside undo. The thing's picture and the seed pill stay at the top |

The fan stops growing into the out-of-reach top third and wraps into another column instead; that is an engineering fix, not a design call.

## 2026-11-28 — Mosselora companion plus (CP01–CP50)

**Decision (founder goal):** make the neighbours' remarks steadier, easier to check and safer to localize, without adding a single player-facing line. The 50 items are in [CHECKLIST.md](../improvements/companion-plus/CHECKLIST.md), built on the SC01–SC07 spec. No line in `en.json` changed during this work.

**Built** (one small commit per item, from `1bb95374` to `d37a4aa9`):

- **Ranker robustness and pacing (CP01–CP10, CP42):** a remark dated after today (clock moved back) counts as today; the memory cap drops repeats before a fact's only entry, so once-facts survive it; a waiting fact said on two of the last three days loses weight for that neighbour (nag damping, CP07); anyone-facts go to the neighbour who spoke longest ago; a kind isn't repeated the next day unless waiting; fixed sentences wait 14 days; the companion's day turns at 4 a.m.; corrupt memory never throws; a golden-hash test pins the shuffle.
- **Simulator and property tests (CP11–CP20):** a pure simulator runs 30- and 120-day gardens. Tests check no wording repeats within 14 days, no once-fact is said twice, a remark on at least 80% of days, a fair share of shared remarks, fuzzed feeds over 500 worlds, weather and slot truth, and a 2 ms budget in Release.
- **Checker rules and tools (CP21–CP30, CP44–CP45):** stable line IDs; near-duplicate, opening-word, punctuation and 28-word checks; no line shared across voices or kinds; a coverage report; a translator export and an import check (drafts go to `docs/improvements/review/`, never `Resources`); the checker runs in CI with `--complete`.
- **Locale safety (CP31–CP36):** an optional slot translator (nothing changes without one); count wording checked for one and many; typed names tidied; blank names never said; slot values can't inject slots; each non-English bank keeps its own memory key.
- **In the app (CP37–CP41):** facts computed once per frame; memory parsed once and cached; a development-only export copies kinds and dates, never words or keys; the QA log lists the top three candidates (`MOSSELORA_COMPANION_WHY`); a new garden forgets the old garden's memory.
- **Process and docs (CP43, CP47–CP49):** dead helpers removed; [architecture page](../../games/evergrove/docs/specs/companion-architecture.md) and [writing guide](../../games/evergrove/docs/specs/companion-writing-guide.md) added; the playtest kit updated.

| Preflight | Answer |
| --- | --- |
| Truth | Pure C# tests (5,477,201 assertions, passing); 162 Python tests; line bank 932 lines, 0 problems, 20 warnings. Unity build c12 with 0 `error CS` and `Info.plist` present; simulator tours c12p (portrait), c12l (landscape) and c12x (150% text) all reached `MOSSELORA_QA_TOUR_DONE`. No physical device; no adult 40+; no family pair; no non-English bank exists |
| Owner | Founder, through the SC01–SC07 goal and the companion-plus checklist. Founder review of the 20 checker warnings and one echo line is still pending |
| Delete | No runtime model, backend, account or network. The export leaves the phone only when a participant pastes it. Machine translations are never shipped; English is the only bank |
| User value | Neighbours who don't repeat themselves, don't nag about a waiting fact, and take turns; a home screen and QA log that explain a choice; a safer path for a translated bank |
| Audience fit | Unknown: `age fit not validated` |
| Falsification | The companion week kit (CP49): 5–10 adults 40+ per age band for a week. Do they notice repetition or nagging? Does the CP46 transcript read as natural in context? |
| Stop | Testers call the neighbours repetitive within the week, or find a remark pushy; then change the nag constants (unvalidated tunables) or drop that kind |
| Evidence | Test output above; tour logs (24 `MOSSELORA_COMPANION_WHY` lines per tour); the week kit's day-7 notes, verbatim, when run |

**Findings for the founder (no line changed here):**

- The checker reports 20 warnings, none blocking. One near-duplicate: VisitorCame voice 2, #4 and #14 (Jaccard 0.77): "Your flower beside the tree has a visitor at dusk: a hedgehog." and "Your flower beside the tree has a prickly friend. A hedgehog, at dusk." The other 19 are opening-word clusters (for example VisitorCame/2 with eight lines starting "a", RainComing/1 with eight starting "rain").
- "The water's easier to listen to once {work} is done." (`en.json` line 89) reads "…once the water is done." when the work is water. The echo is awkward; a founder call.
- In the tour's rain-coming card shown in falling snow, the fact is fixed (commit `d37a4aa9`: rain-coming only on a dry day). The snow left in the 21-week shots is the tour not redrawing the weather between scripted days (the log shows the day was Fair). This is a tour follow-up, not a companion fault.

**Not verified:** a physical device; adults 40+ (age fit not validated); non-English banks (none exist); the development-only export button, tapped by hand; the playtest feel of the nag constants.

**Never claim** from this work: that the companion improves wellbeing, memory, connection or loneliness; that adults 40+ notice or like the remarks; that it works in another language; that the nag constants are right.
