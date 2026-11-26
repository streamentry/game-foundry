# AGENTS.md — Game Foundry

## Mission

Build **distinctive, accessible, mobile-first games for adults aged 40 and older** from **verified user-search demand and observed player needs**. The optimization target is validated long-term portfolio value, not game count or lines of code.

## Primary audience: adults aged 40+

- Treat adults aged 40 and older as the default primary audience (changed from 45+ at the founder's direction on 2026-11-24) for research, prototypes, and products. Change this only at the user's direction or when measured evidence supports a different audience.
- The first market hypothesis is U.S. adults 50–75 who already play familiar mobile games. The U.S. evidence currently covers adults 50+, not ages 40–49, and does not prove this segment is the best buyer group. Keep the 40+ audience as the studio direction while testing narrower age bands; research recorded before 2026-11-24 used 45+ and keeps its original labels.
- **Mosselora (Evergrove) adds a family goal** (founder direction, 2026-11-25): grandparents, parents and children should be able to play it together gently, so older players feel connected to family and not only entertained. The 40+ adult stays the primary player; children are co-players. Solo offline play stays complete with no account; family members who live apart may opt in to connect through a backend and a simple sign-up (not yet authorized or built). Treat this as a hypothesis to test, never as a wellbeing or loneliness claim, and resolve the store's child-audience declaration before submitting a build designed for children. See `games/evergrove/vision.md` §1a.
- Do not treat 40+ as one uniform group. When evidence and recruitment allow, distinguish relevant age bands such as 40–49, 50–64, and 65+; do not generalize from one band to all adults 40+.
- Prefer familiar word, puzzle, card, board, and traditional-game mechanics when measured demand supports them. The product hypothesis is a concrete improvement to an experience players already seek, such as readable controls, adjustable pace, reliable rules, offline play, or fair monetization. Validate each gap against current competitors; a familiar genre does not imply an open market.
- Start acquisition research in the U.S. English storefront, then test one regional-language market supported by local demand and player evidence. Vietnamese Nối Từ remains a candidate, not a selection.
- The current first-game recommendation under the search-led track is Kakuro for a U.S. English proof of play, with the U.K. as an English follow-up and Japan as the first localization candidate. Shikaku is the fallback. Follow the player and storefront gates in `docs/research/first-game-decision-2026-11-21.md` before committing to production.
- The user-requested global visual playtest shortlist is Good Eye, Quiet Shot, then One Path; see `docs/research/global-45plus-top-three-games-2026-11-21.md`. This orders exploratory mechanic tests, not ASO opportunity scores or production approvals. Preserve the existing search-led selection and launch gates; actual referrals are not evidence of keyword demand.
- Compare 30 queries before choosing a prototype: 10 adult-oriented familiar puzzles, 10 regional/traditional games, and 10 specialized logic puzzles. Use the queue in `docs/research/45plus-30-keyword-screen-2026-11-21.csv`; keep each market and language separate.
- Age alone does not establish a player's vision, hearing, dexterity, cognition, technical confidence, preferred difficulty, session length, or interest in social play or brain-training claims. Learn these from players and reliable evidence.
- Design for adult competence and agency. Avoid infantilizing language, stereotypes about aging, or a visual identity that signals “senior” without player evidence. Accessible design must not mean shallow mechanics or low challenge.
- Test readability, contrast, touch precision, pacing, help and recovery with adults 40+ from the intended audience when feasible. Keep accessibility options visible and adjustable; do not make timers, small text, color-only cues, or fast gestures the only route through play.
- Do not claim that a game improves memory, cognition, or health without appropriate evidence. Do not infer age-specific demand from broad keyword volumes, language speaker counts, category popularity, or competitor age assumptions. Mark it `unknown` unless the source directly supports adults 40+.

## Naming: Mosselora = Evergrove

**Mosselora** is the public product name (app, store listing, web site at mosselora.com). **Evergrove** is its *internal codename* and remains in paths (`games/evergrove/`), code namespaces, bundle IDs, docs and the decision log. Do not rename internal identifiers. Use "Mosselora" in anything a player or store reviewer sees. Brand masters live in `games/evergrove/brand/` (`mosselora-logo.png`, `mosselora-app-icon-1024.png`); the site and Unity project use copies of them. Its end-game direction and taste charter: `games/evergrove/docs/specs/proposals/endgame-village-2026-11-25.md`.

## Source of truth

1. `docs/research/README.md` governs demand research and evidentiary labels.
2. `docs/ideas/candidates.csv` records **hypotheses**, not approved production titles.
3. `docs/architecture.md` governs project boundaries.
4. `DESIGN.md` governs product quality, with per-game design specs overriding aesthetics only.
5. Each game's `project.godot`, tests and README govern build/run commands.
6. `docs/store-publishing-workflow.md` governs iOS and Android listing, localization, assets, binary upload, review and release settings.

## First-principles operating discipline

Adapted from the user-supplied "How To Win — Elon Musk" principles sheet. Treat these as **challenge questions and decision habits**, not as scientific laws, verified quotations, or permission to disregard platform, legal, ethical, security or human constraints.

### 1. Think from fundamentals, not clones

- **Question the premise first.** What player problem, search intent, and willingness to play can we *observe*? What assumptions are merely copied from a competing game, from our own previous work, or from popular industry advice?
- Reconstruct the simplest game that solves the problem from known constraints (rules, human attention, device capabilities, distribution), rather than imitating another app's design, levels, branding, or UX.
- Separate **possible**, **likely**, and **commercially worthwhile**. Prove mechanical feasibility with a tiny test; then estimate the probability of discoverability, retention, and viable economics from evidence. *Possible does not imply worth building.*
- Favor original value, not contrarian novelty for its own sake. Generic game mechanics can be reused legally; content, design and differentiation must be independently created.

### 2. Apply the five-step simplification sequence IN ORDER

For any significant feature, integration, release process, research pipeline, or proposed agent:

1. **Question requirements.** Who requested each requirement, what observed outcome does it serve, and what would fail if we removed it? Challenge requirements regardless of the proposer's seniority, intelligence, or prior commitment. Distinguish hard constraints (e.g. store policy, security, privacy) from preferences.
2. **Delete.** Remove unnecessary features, dependencies, ceremony, dashboards, prompts, agents, and build steps before touching their performance. If the player/market outcome survives without it, keep it deleted. Never delete legal safeguards or essential validation simply to move faster.
3. **Simplify and optimize.** Only optimize a component that passed the first two steps; prefer fewer states, smaller interfaces, deterministic rules, and obvious user journeys.
4. **Accelerate feedback.** Shrink the experiment → observation → decision loop. Time-box work and surface failures early; do not mistake commit frequency, agent throughput, or game count for progress.
5. **Automate last.** After the manual workflow reliably produces value, automate the stable, repeatable portion with tests and rollback. Automation of an invalid process magnifies waste.

**Before building, answer:** Can this requirement be disproved, deleted, or tested more cheaply? What happens if we do nothing?

### 3. Urgency without recklessness

- Bias toward the **next observable real-world test** rather than a larger architecture, speculative features, or repeated planning.
- Keep scope small enough to fail informatively. A failed, well-instrumented experiment is useful; **repeating failure without learning is not**.
- Solicit disconfirming evidence, including critical App Store reviews and direct player feedback; record the strongest counterargument and evidence that would change our mind.
- Be willing to be wrong, but never **confident without verification**. Label facts, assumptions, estimates, and unknowns distinctly.
- Protect focus: stop efforts that cannot plausibly improve player value, distribution, reliability, or learning. Do not mistake low-level activity for a meaningful outcome.
- Urgency does **not** override sustainable work, family responsibilities, sleep, security, user consent, store policy, or human approval for publishing/spending. Hours worked are not a proxy for value delivered.

### 4. A 60-second preflight for every substantial task

Record these briefly in the PR description, issue, or decision note; for trivial changes, apply mentally without adding ceremony.

| Question | Required answer |
| --- | --- |
| **Truth** | Which facts are independently observed? Which are assumptions? |
| **Owner** | Who or what imposed this requirement? Is it truly necessary? |
| **Delete** | What can be removed, including the entire task? |
| **User value** | What changes for the player, or for organic acquisition? |
| **Audience fit** | What evidence shows the need and proposed experience fit adults aged 40+? Which age bands were observed? |
| **Falsification** | What is the cheapest test that could prove this direction wrong? |
| **Stop** | What measured result would cause us to stop or pivot? |
| **Evidence** | What test, external response, or artifact will verify the outcome? |

If answers are weak, run the falsification test before expanding the implementation.

## Required work cycle

1. State the testable player problem, search term, distribution path and stopping rule for adults aged 40+.
2. Audit evidence before choosing a concept. Record whether each source actually identifies adults 40+ (or the narrower band it measured); mark missing metrics and age fit as `unknown`, never zero or guessed.
3. Prefer the smallest offline-capable vertical slice with independently testable game rules.
4. Add deterministic tests for generator validity, uniqueness, difficulty and persistence where relevant.
5. Run the Godot headless import/test commands and Python checks. If unable, report **not run** with reason; never imply success.
6. Before a release, check real iOS/Android devices, accessibility and usability with the intended 40+ audience where feasible, Apple 4.3, assets/rights, privacy, crash reporting and store metadata.
7. Record the result and what the evidence changed in `docs/research/decision-log.md`.

## Engineering invariants

- Pin to stable Godot **4.7.2** for existing Godot games until a tested upgrade PR changes the pin. **Exception:** `games/evergrove/` is explicitly selected for Unity 6.3/C#; run its `.NET 8` pure-domain tests and separately verify Unity editor/mobile builds. Do not rewrite unrelated games.
- Use **GDScript**; avoid C# and native plugins unless a capability demands them.
- Gameplay logic must be pure/testable without a UI scene wherever feasible.
- Never add copied iconography, level packs, sounds, screenshots or trade dress.
- No arbitrary third-party SDK copied from a gist; audit version, maintenance, licenses, privacy and mobile platform support.
- Keep independent Godot projects per game. Share tested code via explicitly versioned add-ons or vendoring; never fragile `../` resource paths.
- Keep app IDs, certificates, Apple/Google credentials and API tokens out of git. Do not submit or release a game without explicit user authorization for that release. When the user explicitly requests automatic release after store approval, configure that store option while preserving all build, review, privacy and legal gates. Never automate spending or an account owner's legal attestation.
- Never fabricate API metrics, keyword volumes, reviews, retention, LTV or competitor weaknesses.
- Never present broad market metrics as proof of demand or usability among adults aged 40+.
- The proposed first-launch target of 1,000 non-branded organic installs within 60 days is an experiment goal, not an industry benchmark or forecast. Define a numeric Day-7 retention threshold before launch; do not call retention “meaningful” without a predeclared measure.
- Prefer meaningful PRs and passing tests over agent-generated noise.

## Agent responsibilities (roles, not separate services)

- **Scout:** Find primary demand evidence, collect source URLs and dates, separate age-specific evidence from broad market signals, and challenge ranking assumptions.
- **Producer:** Write a one-page game hypothesis for adults 40+ with differentiation, monetization and stop criteria.
- **Builder:** Deliver a complete game loop, original visuals and accessible input, with pure rules.
- **Reviewer:** Try to falsify uniqueness, correctness, mobile usability for the intended 40+ audience, security and policy compliance.
- **Publisher:** Validate exports, metadata, receipts/consent where used and rollback plan. Requires explicit user authorization to submit; account-owner declarations remain for the account owner to confirm.

## Done means

A prototype is done only with clear instructions, reproducible rules tests, a launch screen on a real target device (or explicitly marked unverified), feedback from adults in the intended 40+ audience (or an explicit `age fit not validated` status), and a decision record. A shipped game additionally needs platform builds, store approval, privacy disclosure, install attribution and retention instrumentation.

**Never conflate a working prototype with validated organic demand.**
