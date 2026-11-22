# Decision memo: the next three Game Foundry games to validate

**Date:** 2026-11-22  
**Decision status:** Provisional **sequential experiment priority**; no new full-production greenlight, App Store release, budget approval, or claim of validated demand.  
**Decision owner:** Game Foundry product strategy (recommendation; not a player-tested outcome).  
**Primary objective:** Maximize **differentiated repeat play and viable discovery per founder-hour** for an indie, mobile-first studio, initially focused on adults 45+. Preserve the option to build a much larger global IP without mistaking hypothetical upside for achievable returns.

## Executive decision

**1. Kakuro Compass / Kakuro Zen → 2. Rune Chain / Three Cards → 3. Little Worlds / Evergrove Lite.**

These are **three different bets**, deliberately not three near-identical puzzle launches:

| Order | Canonical experiment | Strategic question | Why now | Status |
| --- | --- | --- | --- | --- |
| **1 — Validate existing** | **Kakuro Compass** (`GF-001`; `GC-011 Kakuro Zen` is a *possible redesign*, not a second app) | Can we win existing **non-branded organic App Store search** with an experience demonstrably better than incumbents? | A small [Godot prototype](../../games/kakuro-compass/README.md) already exists; the research hypothesis and competitors are documented | Code-level proof of play; *market and player evidence unknown* |
| **2 — First new mechanic slice** | **Rune Chain** (`PH-034`) combining the constrained choices of **Three Cards** (`GX-009`) | Can 5-minute, deterministic, evolving-card decisions earn spontaneous repeat runs and give us an ownable game identity? | Bounded engineering surface and systemic depth; can be tested without LLMs, multiplayer or backend | **Concept only**; no playable implementation verified |
| **3 — Conditional long-term IP** | **Little Worlds** (`GC-021`) using selected **Evergrove** (`PH-006`) / **Neighbor Memory** (`GX-002`) ideas | Does a tiny, causally coherent world increase return beyond the *same puzzle game without persistent consequences*? | Tests ownership/attachment rather than endlessly making more puzzle content | **Concept only**; no playable implementation verified |

**Interpretation of “build first”:** finish one measurable learning loop before expanding to the next prototype. Kakuro is **not a request to create another Kakuro project**. Rune Chain is the first *new* build candidate. Little Worlds remains a bounded concept experiment until basic distribution and player evidence justify it.

**Important conditional:** The repo's existing Kakuro commercial test is **ASO-only**. Rune Chain and Little Worlds do **not** presently have a verified ASO wedge. They may be researched and playtested as novel mechanics, but **cannot advance to a production decision under an ASO-only constraint** without separately validating exact search-intent demand or explicitly recording a different acquisition strategy.

## What is known, inferred and missing

| Finding | Evidence and source | Interpretation / limitation |
| --- | --- | --- |
| Kakuro Compass has a Godot project with 8 original 5×5 boards, an intersection inspector, undo and local state | [Project README](../../games/kakuro-compass/README.md) | **Repository-documented engineering proof**, not observed user preference, signed mobile release, or production QA |
| A US/UK/JP Kakuro *web-search* phrase had a Google Keyword Planner range of **1K–10K average monthly searches** (Nov 2025–Oct 2026) | [Kakuro demand dossier](kakuro-global-aso-45plus-2026-11-21.md) and [first-game decision](first-game-decision-2026-11-21.md) | **Historical Google web estimate** with close variants—not Apple search volume, installs, organic App Store rank, buyers or 45+ demand |
| Conceptis already advertises an Assistant for next-step hints (released in a 2025 update) | [Conceptis Kakuro US App Store listing](https://apps.apple.com/us/app/kakuro-number-crossword/id1197071375), checked 2026-11-22; [repo supply audit](first-game-decision-2026-11-21.md) | **Publisher/visible store-listing evidence**; a generic “great hints” differentiator is *not unique*; actual experience must be tested |
| Balatro demonstrates a commercially present premium mobile roguelike with card synergies | [Balatro US App Store listing](https://apps.apple.com/us/app/balatro/id6502453075), checked 2026-11-22 | **Genre existence and a strong incumbent**, not proof that a different indie deckbuilder will be discovered or retained |
| Folding and balancing are already served by direct mobile competitors | [Unfoldit](https://apps.apple.com/us/app/unfoldit-paper-puzzle-game/id1248922247), [Paper Fold](https://apps.apple.com/us/app/paper-fold/id1558177980), [Balance Stuff](https://apps.apple.com/us/app/balance-stuff/id1528333833); listing check 2026-11-22 | Existence and stated mechanics **do not prove** market saturation or exact organic keyword competition; they weaken claims that *folding/balancing alone* is a novel wedge |
| The repository has a separate [Good Eye prototype](../../games/good-eye/README.md) and [global visual playtest plan](global-45plus-top-three-games-2026-11-21.md) | Project and research notes | These are still useful, inexpensive controls. This recommendation **does not cancel** Good Eye → Quiet Shot → One Path research |
| Neither Rune Chain nor Little Worlds has cohort return, conversion, organic acquisition, reliable monetization or comparative preference evidence | [Original concept portfolio](../ideas/projected-global-hits-2026-2036.md), [genre experiments](../ideas/four-genre-outlook-2026-2036.md), [global mechanics](../ideas/global-concepts-2027-2036.md) | All upsides are **reasoned hypotheses**, not measured opportunity sizes |
| The repo already warns about biased recent-top-five selections | [Full-portfolio ranking correction](portfolio-ranking-audit-and-protocol-2026-11-22.md) | This memo is a **qualitative stage-gate decision among families**, not an exhaustive deduplicated, quantitatively scored global ranking |

**External check scope:** The 2026-11-22 App Store checks above confirm listings and described mechanics. They do **not** include an authenticated top-10 organic storefront search, hands-on device comparison, install attribution, trademark clearance or verified 45+ cohort preference. App Store ratings and review counts are deliberately not used as search-demand proxies.

## Why these three, and not the obvious alternatives?

The recommendation is based on a **portfolio of distinct information-gathering options**, not pretending to know which game will top global charts in 2036.

| Family / candidate | Why attractive | Why below the top three *for this studio now* | What could reverse the decision |
| --- | --- | --- | --- |
| **Unfold** (`GC-001`) | Wordless, high potential cross-market comprehension | Existing folding and unfolding games mean a *specific* new rule and acquisition route need proof | A clearly different 5–10-level mechanic wins blind preference and recruits players organically |
| **Still** (`GC-002`) | Calm, tactile, potentially durable brand | Balancing physics fairness, content depth and distinction from existing stacking games are untested | Repeated voluntary stacking play and verified differentiation on physical devices |
| **One Move** (`GC-003`) | Low tutorial overhead and inexpensive prototype | One-action novelty may fade and the relevant search/share route is unknown | Players voluntarily replay beyond first novelty and share without prompting |
| **Good Eye** (`GV-01`, *already built*) | Cheapest real-world baseline for immediate-comprehension puzzles | Visual estimation is highly testable but its enduring retention and discovery path remain unproven | Existing Good Eye users show organic return/referral exceeding speculative new concepts |
| **Shikaku Garden** (`GF-018`, *already built*) | Search-led alternative to Kakuro and no extra prototype needed | Earlier Google web-search evidence was weaker in some markets; incumbents already have explanations | Kakuro fails search or preference gate and Shikaku passes its own |
| **Vietnamese Nối Từ** (existing word hypothesis) | Prior research observed a relatively broad Vietnamese **web** search band | Dictionary correctness/licensing, storefront intent and relevant older-player appeal still unproven; regional rather than language-light global-first | Adjudicated word acceptance and credible organic installs outperform Kakuro's accessible market |
| **Arrowverse** (`GC-006`) | Intuitive action and an already recognizable mechanic class | Easily imitated; no demonstrated distinctive level grammar or low-cost discovery | A testable unique rule and direct head-to-head preference survive competitor review |
| **Nexusforge / Infinite Worlds** (`PH-002`, `GX-013`) | Potential huge creator/network effects | Requires creator cold start, user safety/moderation, discovery, hosting and likely more capital than current studio | A tiny shareable creator tool generates meaningful recipient play/remixes at manageable cost |
| **Echoes of Aether / persistent large RPG** (`PH-001`) | Very high hypothetical prestige and systemic depth | High-content, narrative, inference, QA and operating costs; no small-team distribution wedge | A tiny deterministic 3-NPC world already creates strong preference, then a credible distribution plan emerges |
| **One Clue** (`GC-014`) | Short cases could build a repeatable daily ritual | Story/editorial QA, fair deductions and translation dominate the build; AI-written clues risk inconsistency | One authored case earns return and organic sharing at acceptable production cost |

**Strongest objection to the entire top three:** Search-led Kakuro may have too little accessible demand, while Rune Chain and Little Worlds may have **no discoverable audience at all**. The best next investment might instead be **zero new games and rigorous playtests of already-built prototypes**. The first gate is therefore designed to permit exactly that outcome.

## Experiment 1 — Kakuro Compass (first action, not a new app)

**Player / market:** US adults 50–75 familiar with number or logic puzzles; record age bands separately. UK and Japanese localization remain conditional follow-ups.

**One sentence of differentiation to test:** “I can understand the across/down intersection and eliminate candidates without repeatedly zooming or guessing.” This must beat a polished alternative, not merely sound useful.

**Scope:** Host the existing browser playtest after privacy review; check the 8-board logic/inspector tests; do one phone usability pass. **No new generator, subscriptions, analytics SDK or second Kakuro app** initially.

**Comparison:** Give 10–15 independent target participants comparable first-board tasks in **Kakuro Compass and Conceptis Kakuro**, counterbalance which game comes first, and record:
- First correct unaided placement, ability to explain an intersection deduction, and completion/abandonment.
- Assistance, zoom and error-recovery needs; visibility and touch precision.
- Which app they freely choose for a second puzzle, with a concrete reason.
- Optional unsponsored later return, measured only from actual use.

**Proposed early screen (NOT a statistically validated benchmark):** A strong directional signal is **at least 10 of 15** preferring our experience for a second puzzle *after* a comparable task, with a recurring, attributable inspector advantage and no serious accessibility blocker. A weaker split demands revision or more research, not a declaration of victory.

**Independent acquisition gate:** Collect Apple Ads `searchPopularity1to100` (where authorized) for exact US terms, capture **actual top-10 unsponsored results** and dated review comparisons, and identify a credible path to **non-branded organic installs**. The web-search 1K–10K band cannot substitute.

**Stop / pivot:** After one targeted inspector UX revision, park Kakuro if preference still favors incumbents or specific US Apple search intent/competition cannot support a plausible organic wedge. **Fallback:** run Shikaku's *own* demand and player gates, not an automatic production switch.

## Experiment 2 — Rune Chain + Three Cards (first net-new playable slice)

**Concept identity:** `PH-034` evolving cards + `GX-009` constrained three-card choices. Treat these as **one** original prototype, not two apps. Proposed design is *not* poker scoring, Balatro art, copyrighted content or a clone of a recognizable rule set.

**Player promise:** “Three clear choices now; surprising but predictable card evolution later.”

**Minimal complete mechanics:** One self-contained 5-minute run, ~12 original cards, **three transparent evolution rules**, three encounter types, finite opponent behavior, deterministic seed, undo/replay where appropriate, and local state. Use Godot, no account/LLM/server.

**Why this is second:** It tests *decision depth per line of content* and deliberate voluntary replay, without NPC continuity or online infrastructure. However, the card/roguelike market is crowded and age-45+ appeal is **unknown**, not presumed. Recruit based on observed genre use, report age and previous deckbuilder familiarity.

**Cheapest falsification:** 10–15 independent players; after a short explanation, let each finish one run and **offer an unprompted second run**. Record strategy comprehension, whether they can predict card evolution, unique meaningful decisions, voluntary restart and next-day return. Compare with a small accessible card strategy reference, avoiding copyrighted implementation.

**Proposed screen:** A majority voluntarily replaying and explaining **one non-obvious, rule-consistent strategy** is only a reason for a larger test. If few choose a second run, evolution is opaque, or players overwhelmingly prefer a polished incumbent, **stop before a content factory**. Record exact counts, denominators, player cohort and test build.

**Critical commercial gate:** Name and validate a **real organic distribution route** (exact App Store search terms, communities, or trackable challenge sharing) *before* turning the prototype into a product. If ASO-only remains non-negotiable, the lack of searchable demand is a hard stop, even with excellent gameplay.

## Experiment 3 — Little Worlds / Evergrove Lite (conditional third slice)

**Concept identity:** `GC-021` puzzle-built miniature world + `PH-006` persistence/companions + `GX-002` visible NPC memory. **One** bounded prototype. No claim to a new MMO, AI friend or global creator platform.

**Player promise:** “Solve a small puzzle. Make a change. See that the village remembers.”

**Minimal complete mechanics:** One small room/garden, **three NPCs with visible goals**, five deterministic puzzles that alter **one shared persistent resource**, a readable event log of decisions/consequences, and local save/reset. Character reactions are rule-driven first. **No LLM, economy, always-online service, trading, push notification or public chat**.

**Critical controlled test:** Compare **the same five puzzles** with and without persistent consequences. Randomize presentation/order where feasible and keep difficulty, polish, content and rewards comparable. Observe whether users *choose to return because of the remembered change*, rather than because the underlying puzzle is fun.

**Proposed early evidence:** In 10–20 exploratory participants, look for understandable causal explanations, unsolicited choices to continue and later voluntary return. A small convenience sample cannot prove a modest D1/D7 uplift. If promising, run a larger preregistered comparison with defined denominators and adequate time. Do not declare a retention gain from sentiment alone.

**Stop / simplify:** If people prefer puzzle-only, cannot connect cause to effect, or the world layer adds most production complexity without observable additional return, **drop NPC simulation** and keep the best underlying puzzle. If retention is improved but no acquisition path exists, **research distribution before production**.

**Safety and product guardrails:** NPC memory must be inspectable/resettable; no emotional coercion, fake consciousness claims, private-data collection by default or unbounded generative dialogue. A living world is convincing when its consequences are **coherent**, not infinite.

## Practical seven-day sequence (decision work, not a promise of three releases)

| Window | Observable action | Artifact and decision consequence |
| --- | --- | --- |
| **Day 1** | Use the existing Kakuro build, confirm browser export/test status and physical-phone readability; choose direct comparison levels | Versioned link, screen recording/checklist and participant recruitment plan |
| **Days 2–3** | Recruit 10–15 relevant players; conduct counterbalanced Kakuro vs Conceptis comparison; separately capture exact US App Store organic results and authorized search-popularity data | Dated competitor audit; per-participant observations with consent and no unnecessary personal data |
| **Day 4** | Summarize Kakuro results against the predeclared preference and acquisition gates | **Continue/revise once/park** decision; no “ship” without evidence |
| **Days 5–6** | Only if capacity remains after real feedback, build one deterministic Rune Chain vertical slice and recruit initial players | Runnable Godot scene, rules tests, seed fixtures, concise tester instructions |
| **Day 7** | Run first independent Rune Chain replay observations, estimate a discovery path; write a **one-page** Little Worlds A/B brief *without building a backend* | Player counts and unknowns; choose next single experiment |

**Do not block a high-value player test merely to complete this calendar.** A public playtest URL requires separately approved hosting; [GitHub Actions ZIP artifacts are not public websites](../playtest.md). Never interpret a GitHub build passing as observed retention or market readiness.

## What would change the ordering?

- **Kakuro fails:** Stop after the predefined head-to-head and search gates; try Shikaku or the already-built Good Eye before automatically building Rune Chain.
- **Rune Chain exhibits poor spontaneous replay:** Prefer a simpler spatial or visual mechanic (Unfold, Still, Good Eye) over months of balancing added cards.
- **Little Worlds cannot outperform puzzle-only:** Keep the puzzle; shelve NPC/world persistence, even if the technical demo looks impressive.
- **Verified ASO-only constraint for all products:** Move novel concepts into research-only; reprioritize exact-demand niche apps instead of inferring discovery from a large genre.
- **A strong counterexample emerges:** An existing prototype shows observed organic acquisition, direct competitor preference and repeat play. Promote it over this *opinion-based* sequence.

## Evidence quality, confidence and provenance

**High confidence:** the repository already has an engineered Kakuro slice; its generic hint wedge overlaps incumbent marketing; a three-track *experiment* portfolio creates different learning opportunities.  
**Moderate confidence (judgment):** Rune Chain's bounded systems experiment is a good use of founder-level engineering time **if** distribution becomes credible.  
**Low confidence:** attainable revenue, organic growth, cohort retention, age-specific preference and global-breakout potential for all three.

**This is not a quantitative full-corpus rank:** The [prior methodological correction](portfolio-ranking-audit-and-protocol-2026-11-22.md) remains valid. We inspected the main candidate families and compared representative alternatives, but did **not** build a complete canonical deduplication table, measure all titles or run weighted sensitivity analysis. The above selection is a **conditional, decision-useful shortlist**, not a verified “top three globally.”

**Related repository records:** [Search-led Kakuro choice](first-game-decision-2026-11-21.md) · [Kakuro ASO recheck](kakuro-global-aso-45plus-2026-11-21.md) · [Global visual tests](global-45plus-top-three-games-2026-11-21.md) · [Portfolio ranking protocol](portfolio-ranking-audit-and-protocol-2026-11-22.md) · [Game research methods](README.md) · [Roadmap](../roadmap.md).

**Next real action:** arrange an external Kakuro-vs-Conceptis test using the already-built prototype and record the first independent player's observed behavior. Do not create a 34th game merely to avoid confronting player feedback.
