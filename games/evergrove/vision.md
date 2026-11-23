# Mosselora — The Hundred-Year Garden

> **Status (revised 2026-11-23 after a full research audit):** founder-directed long-range vision. **This is an ambition, not a forecast, a roadmap commitment or a production approval.**
>
> **What the 100-million figure is:** a north-star target the founder chose. No research in this repository supports it, and the repo's own audit warns against mixing "the largest imaginable global outcome" with the best outcome a small studio can actually reach ([portfolio audit](../../docs/research/portfolio-ranking-audit-and-protocol-2026-11-22.md)).
>
> **Precedence:** code and passing tests describe what exists. The [handbook](docs/README.md) and [specs](docs/specs/README.md) govern what we test next, including the sequential [M0–M6 gates](docs/specs/09-EXECUTION-CONTROL.md). Where this file conflicts with a spec rule or a gate, **the spec wins** until a new decision record changes it. §9 lists every place where this vision currently goes beyond the specs.

---

## 0. Where we actually stand (the starting line)

| Area | Observed today | Source |
| --- | --- | --- |
| Players | **Zero** human sessions recorded; 0 of 46 test contracts verified with people | [verification matrix](docs/specs/verification-matrix.json) |
| Build | Pure C# rules pass (39,466 assertions). **M1 build evidence recorded at source `5acbac4`:** the official local preflight passed editor smoke, an Android development APK (sha256 `2897d954…`) and an iOS device Xcode export (BLD-001/002/003 marked verified from an AI agent's log review; [evidence](docs/evidence/5acbac4c5cba4164a31abf33dedf71ff9bf2f8ba/)). The IL2CPP iOS build runs the full loop with touch on the iOS Simulator ([iOS QA](docs/qa/ios-simulator-2026-11-23/)). **The Android APK runs and chapter 1 can be solved with real taps on an Android 15 emulator, after fixing an engine-stripping bug that left Android screens blank** ([Android QA](docs/qa/android-emulator-2026-11-23/)). **No physical phone tested yet**; M2–M6 are not done | [BUILD_AND_QA](docs/BUILD_AND_QA.md), [decision log](../../docs/research/decision-log.md) |
| Demand | "Pipe connect puzzle" (GF-064) is `hypothesis_only` with no measured popularity. `measurements.csv` holds a header only. A 26-query [capture queue](../../docs/research/pipe-water-keyword-queue-2026-11-23.md) is ready but unmeasured. 15 pipe-puzzle competitors are now [catalogued from listings](../../docs/research/pipe-puzzle-competitor-scan-2026-11-23.md); none showed a choice-changes-next-puzzle layer (listing pages only) | [candidates.csv](../../docs/ideas/candidates.csv), [market scan](../../docs/research/market-scan-2026-11-17.md) |
| Readiness for first playtests | Board-first puzzle screens; a non-color pipe cue; a water-wave spring; a framed landmark and badge on protected tiles; a consequence-first memory screen; text size 100/125/150% (EG-007); large-board mode (EG-112); ten-step undo (EG-111); copy and restore of the garden file; a first-pass screen-reader hierarchy (EG-113: with TalkBack on, the Android emulator publishes 36 labelled nodes; spoken output is not yet verified by a person); a reduced-motion audit (EG-114: no decorative motion ships); 97 localization keys (drafts). **A release-candidate build ships no dev or research controls (BLD-007 verified on the iOS Simulator).** Save recovery passed on the iOS Simulator (relaunch, force-close, corrupt-primary recovery, both-corrupt write block, suspend/resume). **Japanese and Vietnamese** render correctly in the real iOS build, and NPC names are localized throughout. Complex-script text: **Arabic, Hebrew, Persian and Urdu now render correctly in the real iOS build** (own Arabic shaper plus vendored MIT BidiSharp, line-by-line bidi; Urdu draws in a Naskh-style system font, and Nastaliq expectations need native review). **Bengali** vowel signs now attach correctly (own pre-base visual ordering). Conjuncts can't be formed by any Unity text path we tested, including the Advanced Text Generator; only native OS text (iOS CoreText, proven in a spike) forms them, so shipping Bengali needs a native text overlay decision. Native review is required before any of these ship; the shipping list stays English only ([EG-006 spike](docs/specs/proposals/eg-006-text-spike-2026-11-23.md)). **None of this is player-validated** | [PLAYER_EXPERIENCE](docs/PLAYER_EXPERIENCE.md), [release-candidate audit](docs/qa/release-candidate-audit-2026-11-23.md) |
| Depth | 33 variants are rotations of 12 boards; rotation keeps the same connection graph, so a variant is no harder to solve. 3×3 boards are beginner-only | [handbook](docs/README.md), [experiments 23–32](../../docs/research/game-experiments-23-to-32-2026-11-22.md) |
| Key experiment | Living World vs Puzzle Only is **confounded after chapter 1**. A puzzle-identical control (`PuzzleMatched`) now exists in code and can be selected in development builds; board equivalence is proven by tests. The [M4 preregistration](docs/playtest/m4-preregistration-draft.md) is drafted; **no participants yet** | [VALIDATION](docs/VALIDATION.md) |
| Name | "Mosselora" is **not trademark-cleared**. A [preliminary public knockout search](docs/legal/name-knockout-2026-11-23.md) found no exact conflict (official registers couldn't be queried); an attorney search is still required | [DECISIONS_AND_RISKS](docs/DECISIONS_AND_RISKS.md) |
| Portfolio | The recorded sequence put Kakuro first and "Little Worlds / Evergrove Lite" third and conditional. The founder then chose to explore PH-006 on preference, not data. The Kakuro test has no recorded result | [next-three decision](../../docs/research/next-three-games-validation-decision-2026-11-22.md), [decision log](../../docs/research/decision-log.md) |

**Honest summary:** Mosselora is a well-engineered hypothesis. Every claim above the Seed row in §5 is aspiration.

---

## 1. The north star

**Mosselora becomes the garden a hundred million people choose to tend, and one that some of them hand to their children.**

| Goal | What it means | What it does **not** mean |
| --- | --- | --- |
| **A century masterpiece** | A game still worth playing, still openable and still respectful decades from now; learned in seconds, enjoyed for years | Prestige graphics, a feature list that dates fast, or a promise that one engine lasts 100 years |
| **100 million players** | 100 million unique people who **chose to play** over Mosselora's lifetime, across platforms and languages | Paid installs bought for chart rank, inflated downloads, or growth from compulsion loops |

The second goal counts only if the first is kept.

---

## 2. What lasting a century requires (and what we already have)

1. **A rule that fits in one sentence.** *"Turn the tiles until one unbroken, leak-free stream runs from the spring through every pipe."* That is the real win condition ([game rules](docs/GAMEPLAY_AND_WORLD_RULES.md)), not a simplified "reach the garden".
2. **Depth from a system, not from content volume.** **This is unsolved today.** Rotated variants add no structural difficulty. Real depth has to come from ecological constraints that change the board graph (EG-105), larger and better-designed boards, and editorially reviewed difficulty curves ([backlog](docs/specs/02-FEATURE-BACKLOG.md)). If depth can't be found, the stop rule in §8 applies.
3. **The rules outlive the engine.** The asset that lasts is the **pure, engine-free C# rules core**, not Unity ([architecture](docs/ARCHITECTURE.md)). The [Unity ADR](../../docs/decisions/2026-11-22-evergrove-engine-unity-csharp.md) is explicitly "not a permanent ten-year commitment" and lists proprietary-license risk. Keep the core portable, and review the engine choice at the ADR's own trigger points.
4. **Players own their garden.** Today saves live in the app sandbox, can't be exported, and newer saves may not open in older versions ([save contract](docs/SAVE_AND_MIGRATION.md)). For a century goal we need an **exportable, documented save format before the first commercial release**, not at 100M players. Keep the existing rule: "never silently turn an old garden into a new topology", and write a migration test for every shipped save version.
5. **No dependence on a server for the core game.** The single-player game stays complete offline (already true and a spec rule).
6. **Respect is permanent, manipulation dates badly.** No streaks, energy, wall-clock gates, fake urgency, relationship meters or attachment pressure ([product spec](docs/specs/01-PRODUCT-AND-EXPERIENCE.md), [world spec](docs/specs/04-WORLD-AND-NARRATIVE.md)).
7. **Readable and touchable for the people we design for.** Adults 45+ are the studio's [primary audience](../../AGENTS.md). Test the age bands **45–54, 55–64 and 65+** separately. Accessibility is a **pre-launch gate** (M5), not a growth feature. It also isn't a competitive edge: competitors already offer large print, no ads and offline play ([45+ market screen](../../docs/research/45plus-market-screen-2026-11-21.md)). We do it because it's right and because it may improve conversion, not because it differentiates us.

**Check for every future feature:** *Will it still make sense, still run, and still feel kind in 50 years? Can a player take it with them?*

---

## 3. The one thing that could make Mosselora distinctive

Accessibility, offline play and calm design are **table stakes** in this category. The only candidate for a genuine difference is:

> **Your choice changes the next puzzle, and the place remembers it truthfully.**

Everything in this vision depends on that claim surviving **M4**: a preregistered, puzzle-identical Living World vs Puzzle Only comparison ([execution control](docs/specs/09-EXECUTION-CONTROL.md)). The spec's continue rule ([validation & GTM](docs/specs/07-VALIDATION-AND-GTM.md)):

> ≥80% of target first-time testers explain the puzzle goal unaided, ≥70–75% recall a specific real village consequence after 10 minutes, and a clear majority prefers the living-world condition **when board difficulty is matched**.

**If players prefer Puzzle Only,** the specs and the founder's own [build decision](../../docs/research/evergrove-build-decision-2026-11-22.md) say to stop or simplify. Mosselora would become an excellent premium water puzzle, and §4's living-world layers would be dropped.

---

## 4. The end-state experience (layers, each with its own smallest test)

None of this is built. Each layer must pass its **smallest test** before it is designed at scale, and each has a **cost gate**: estimate the per-player cost of generation, storage, moderation and live operations before scaling ([living-worlds thesis](../../docs/ideas/living-worlds-social-systems-2026-2036.md)).

### 4.1 The verb: guide the water

The water puzzle stays the heart of the game, and every layer must change *where water can go next*. The verb is our best current bet, not a vow: if evidence favours a simpler or different core, the [reversibility rule](docs/DECISIONS_AND_RISKS.md) applies.

### 4.2 A garden that answers

- **Deterministic ecology.** Water, shade, roots, pollinators and soil interact through small, readable rules that change real puzzle constraints. **Pull this earlier, not later.** The risk register rates "branching not meaningful" as **Critical**, and the specs allow EG-105 right after M4, before launch.
  - *Smallest test:* five deterministic interactions in one offline biome, compared with core-only play.
  - *Kill if:* it feels random or unfair, shows no replay gain, or hides the puzzle's joy.
- **Seasons and chapters by progress, never by clock.** Seasons, floods, droughts and winter channels arrive **through the player's progress**, never on a real-world timer. A garden left alone **waits unchanged**. The specs forbid wall-clock gating, made-up elapsed-time events and seasonal exclusives.
- **Generations (EG-109).** A finished garden can be kept as a storybook, and a new garden can begin downstream and inherit its landmarks.
  - *Kill if:* fewer than ~30% of replays are materially different (the spec's hypothesis threshold).

### 4.3 Neighbors who remember, honestly

- Mira, Orin, Fen and future villagers keep **3–5 salient written memories each** ([world spec](docs/specs/04-WORLD-AND-NARRATIVE.md)). Their lines come from real save state and are **never invented**.
- They react, recall and change with the player's chapters. They don't plead for attention, show distress at absence, or track "affection".
- **The player controls memory:** you can view it, reset it per character, and delete it with the save.
- **Generative AI** may vary *phrasing* only after an A/B test shows players prefer it to authored lines **and** the gain exceeds inference, latency, safety and QA cost ([four-genre outlook](../../docs/ideas/four-genre-outlook-2026-2036.md)). It must never invent player history, require a connection, or imply sentience. Any companion is disclosed as "simulated, not sentient" ([AI companion thesis](../../docs/ideas/ugc-ai-companion-2036-thesis.md)). *Current rule: not authorized.*
- *Smallest test:* the existing three authored neighbors vs a scripted no-memory control. Spec targets: ≥70% name one remembered event after chapter one, and ≥80% of returning testers reconstruct a past choice.

### 4.4 Sharing first, Commons maybe

This is where the original "global player network" idea lives. **The specs currently reject trading, social economies and creator marketplaces** ([backlog reject list](docs/specs/02-FEATURE-BACKLOG.md)), and backend and multiplayer are not authorized. Evidence also cuts against social play as core: **79% of US gamers 50+ usually play alone** ([45+ screen](../../docs/research/45plus-market-screen-2026-11-21.md)). So social play builds up in rungs, and it may stop at any rung:

1. **No-server sharing (Sprout).** A garden postcard or share card (EG-203). Measure the [referral funnel](../../docs/research/global-45plus-top-three-games-2026-11-21.md): sent → opened → completed → returned. Without this measurement, word-of-mouth growth is a guess.
2. **Local pass-and-play and friend-to-friend puzzle codes**, with no accounts.
3. **The Commons**, only after a separate identity, security and moderation decision, a cost model, and proof from rungs 1–2 that people actually play what friends send. It means downstream gifts between known friends: seeds and cuttings, never currency and never markets.
   - **A gift must never change whether someone else's puzzle can be solved.**
   - **No artificial obligation to reciprocate** (the [franchise benchmarks](../../docs/ideas/global-franchise-benchmarks-2026-2036.md) flag reciprocity loops as manipulation risk).
   - Strangers come last, if ever.

The [four-genre outlook](../../docs/ideas/four-genre-outlook-2026-2036.md) warns that evergreen solo loops and persistent social worlds "require different business models… Do **not** assume they belong in one game." **Mosselora is an evergreen solo game first.** The Commons is optional, and it is the first layer to drop if it raises cost or lowers retention.

### 4.5 Optional cloud sync

A convenience, not a requirement. It comes **after** the exportable save file (§2.4), after an identity and security decision (the current backend is a demo with a bearer token, "not a production identity system"), and after seeing whether players actually switch devices ([backend README](backend/README.md)).

### 4.6 Players as puzzle makers (EG-110)

Players build watercourses, and the solver checks each one has a unique solution. **Human editorial review before anything is shared publicly**, as the specs require for every shipped board.
- *Measure:* publish → recipient plays → remix, not the number of creations.
- *Kill if:* few completions, shared links go unplayed, or almost nobody remixes.

### 4.7 Every language worth serving, done properly

Today: English plus 29 **draft** catalogs. Only English is allowed in player builds, and none are certified. Right-to-left (Arabic, Persian, Urdu) and Indic scripts **can't render correctly** until the text system moves to a renderer that can shape them ([LOCALIZATION](LOCALIZATION.md)).
- Choose launch markets from **measured local demand and player evidence**, not speaker counts. The repo explicitly rejects "localize to every market by default" ([global puzzle priorities](../../docs/research/global-puzzle-market-priorities-2026-11-21.md)).
- Every language goes Draft → Technical QA → Native QA → Store QA → Certified.

---

## 5. The growth ladder: how 100 million would have to be earned

Player counts are **founder targets**, not benchmarks. Each rung reuses thresholds the repo already declares. Where none exists, the rung says it must be **predeclared before the test**, never fitted to results afterwards.

| Stage | Players (lifetime) | Must be true before advancing | Unlocks | Stop or pivot if |
| --- | --- | --- | --- | --- |
| **Seed: M1–M3** *(now)* | Playtests | Licensed Unity build, physical-device alpha with zero open S0/S1 bugs, 100 forced-kill save tests with zero losses. **8–12 uncoached** adults 45+ across age bands: median first input <15 s; ≥80% explain the goal. Design.md's small-sample rule: with 10 starters, if 3+ miss the goal or the restoration, redesign | M4 | Players can't learn the rule. Two presentation iterations fail |
| **Seed: M4** | 20–30 matched | Preregistered Living World vs Puzzle Only on **identical boards**; the §3 continue rule | Ecology v1 (EG-105), more chapters | **Puzzle Only preferred → simplify into a premium pure puzzle, or stop.** World layers are dropped |
| **Seed: M5–M6** | Release candidate | Certified launch languages and text rendering, accessibility review with at least one assistive-technology user, privacy/SDK audit, **name cleared**, **real price experiment** (10–30 target prospects), exportable save file, consented D1/D7 measurement method chosen | First commercial store release | No ethical price players accept. Name unclearable → rename before launch |
| **Sprout** | 1,000 non-branded organic | 1,000 non-branded organic installs by day 60 is a *stretch experimental checkpoint*, read **against actual search impressions** ([Kakuro ASO](../../docs/research/kakuro-global-aso-45plus-2026-11-21.md)). Voluntary D7 meets a threshold predeclared before launch, as [VALIDATION](docs/VALIDATION.md) requires (candidate values: the founder-proposed screening heuristics D1 ≥30% and D7 ≥15% from the [ten-year thesis](../../docs/ideas/ten-year-global-gaming-thesis-2026-11-22.md), "not industry baselines"). Share-card referral funnel measured | Seasons by progress, generations, pass-and-play | Retention below threshold after two honest iterations. **Good return but weak referrals → "the organic-breakout thesis is unsupported"**: find a credible acquisition route before expanding |
| **Sapling** | 100,000 | Each new layer passed its §4 smallest test. Cost model per layer. Second market chosen from measured demand | Expansion packs (if content demand is proven), friend puzzle codes | Growth depends on paid ads, or revenue needs coercion |
| **Grove** | 1–10 million | Referral funnel shows measurable peer-to-peer spread. Separate identity, moderation and cost decision approved | The Commons (friends only), optional cloud sync | Social features lower retention or create incidents we can't moderate |
| **Forest** | 10–50 million | Long-term (≥1 year) players are a measurable group. Localized storefronts sustain organic discovery | Puzzle creation with editorial review, tablet/desktop (EG-208) | Content quality or fairness can't keep pace |
| **Hundred-Year Garden** | 100 million | Years of fair, profitable, respectful operation | Permanent archive commitments | — |

**Measurement constraint:** the specs ban analytics SDKs and always-on analytics, and D1/D7 is currently a manual, consented follow-up. Every rung above Seed needs a **privacy-reviewed measurement decision**, including the install attribution and retention instrumentation [AGENTS.md](../../AGENTS.md) requires for a shipped game, **before** it can be judged. Without one, the ladder can't be measured. A [measurement decision proposal](docs/specs/proposals/measurement-decision-2026-11-24.md) now awaits founder approval: research builds get an opt-in, on-device return diary for M3/M4 participants (local dates only, never leaves the device, participant-exported); store builds rely only on platform aggregate analytics.

---

## 6. Discovery: how 100 million people would find it

This was the largest gap in the first draft, and the research has **no measured answer yet**.

- **Search:** demand for pipe, flow, water and connect puzzles has **never been measured**, and competitors aren't catalogued. *Next step:* add pipe/water/flow/garden-puzzle queries to the [30-query screen](../../docs/research/45plus-30-keyword-screen-2026-11-21.csv). Capture storefront popularity, and do a [storefront audit](../../docs/research/storefront-audit.md) on a physical device: 10 unsponsored results plus about 20 mixed reviews.
- **Referral:** the share card (EG-203) and the referral funnel are the only no-SDK path. Wordle-style spoiler-free sharing and Death Stranding-style indirect cooperation are the closest cheap analogues, both labelled unverified.
- **Founder-led and community distribution** as in the [GTM spec](docs/specs/07-VALIDATION-AND-GTM.md): creators, communities, localized screenshots and ASO tests (EG-206/207).
- **No paid acquisition** until quality and voluntary return justify it, and no ad or purchase SDK until pricing and ethics are evaluated ([VALIDATION](docs/VALIDATION.md)).
- **Context:** global mobile game downloads fell 12% in H1 2026, which "makes organic discovery harder" (sourced estimate in the [45+ screen](../../docs/research/45plus-market-screen-2026-11-21.md)).

**Rule:** "A game can be compelling but undiscoverable" ([ten-year thesis](../../docs/ideas/ten-year-global-gaming-thesis-2026-11-22.md)). Validate product, distribution, monetisation and retention **together**.

---

## 7. A business model that a century allows

Pricing is set to free in App Store Connect, but the business model isn't validated. The specs' preferred hypothesis ([GTM spec](docs/specs/07-VALIDATION-AND-GTM.md)):

- **Free demo, then a one-time full unlock**, with substantial optional expansion packs **only after content demand is shown**. Core puzzles, journal and saves stay offline. A store purchase needs a secure way to restore it.
- **Rejected:** forced ads, paid hints, lives, subscriptions, streak penalties, paid narrative outcomes, seasonal exclusives, loot boxes, real-money economies and selling player data.
- Test: *Would we be comfortable if a player's grandchild read how we made money?*

---

## 8. The strongest case against this vision

- **No evidence of demand.** Pipe-puzzle search demand, competitors and 45+ fit are all unmeasured.
- **Our own forecasts are pessimistic about this kind of outcome.** The studio's ten-year thesis reports "extremely low" confidence that any named title becomes a top global game. The franchise benchmarks say mass audiences come from "distribution, capital, creative ecosystems and network effects" a small studio can't copy. Every benchmark figure in the repo is flagged unverified. **We have no verified data on how any game reached 100M players.**
- **The depth problem.** Today's variants don't add structural difficulty. Without real system depth, decades of play are unlikely.
- **The strongest opposing case is written in the specs:** "a beautifully finished, ordinary offline water puzzle is better" ([evidence & decisions](docs/specs/08-EVIDENCE-AND-DECISIONS.md)).
- **Audience mismatch for social layers.** Most 50+ players play alone, so the Commons may never matter.
- **Portfolio cost.** Mosselora moved ahead of the recorded search-led sequence on founder preference. Every hour here is an hour not spent finishing the Kakuro learning loop.
- **Focus risk.** A big vision can pull effort away from the next real test.

**What would change our mind:**
- **Puzzle Only wins M4:** become a premium pure puzzle and drop the world layers.
- **Good retention but weak sharing:** become a durable premium game for a dedicated audience, and retire the 100M target and the Commons without regret.
- **No acquisition path after a fair, cheap test:** stop ([DECISIONS_AND_RISKS](docs/DECISIONS_AND_RISKS.md)).

---

## 9. Where this vision goes beyond current specs

These need a new decision record before any work starts:

| Vision element | Current rule | Source |
| --- | --- | --- |
| The Commons / gifting between players | Trading, social economy and UGC marketplace on the reject list; backend and multiplayer not authorized | [02 backlog](docs/specs/02-FEATURE-BACKLOG.md), [08 evidence](docs/specs/08-EVIDENCE-AND-DECISIONS.md) |
| Optional cloud sync | Not to be added "merely to make the world appear alive"; backend is a detached demo | [04 world spec](docs/specs/04-WORLD-AND-NARRATIVE.md), [backend](backend/README.md) |
| Generative AI phrasing | Runtime LLM not authorized | [08 evidence](docs/specs/08-EVIDENCE-AND-DECISIONS.md) |
| Exportable save file before release | Not yet specified; current saves aren't exportable | [save contract](docs/SAVE_AND_MIGRATION.md) |
| Shared creation tools | Allowed only with human editorial approval | [03 puzzles](docs/specs/03-PUZZLES-AND-CONTENT.md) |

---

## 10. How to use this document

- **Use it to break ties.** When two options both pass the gates, choose the one that lasts longer, is kinder, works offline and lets players keep their garden.
- **Don't use it as permission.** No layer may be built until its gate in §5 passes and is recorded in the [decision log](../../docs/research/decision-log.md).
- **It doesn't override the portfolio.** It does not displace the search-led Kakuro track in [AGENTS.md](../../AGENTS.md); sequencing between the two is a separate founder decision.
- **Revisit it** after every gate, including if the evidence says the vision is wrong.

*A small place changes because you were there. The vision is that this stays true for a very long time, for as many people as genuinely choose it.*
