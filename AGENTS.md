# AGENTS.md — Game Foundry

## Mission

Build **distinctive, accessible, mobile-first games** from **verified user-search demand**. The optimization target is validated long-term portfolio value, not game count or lines of code.

## Source of truth

1. `docs/research/README.md` governs demand research and evidentiary labels.
2. `docs/ideas/candidates.csv` records **hypotheses**, not approved production titles.
3. `docs/architecture.md` governs project boundaries.
4. `DESIGN.md` governs product quality, with per-game design specs overriding aesthetics only.
5. Each game's `project.godot`, tests and README govern build/run commands.

## Required work cycle

1. State the testable player problem, search term, distribution path and stopping rule.
2. Audit evidence before choosing a concept. Mark missing metrics as `unknown`, never zero or guessed.
3. Prefer the smallest offline-capable vertical slice with independently testable game rules.
4. Add deterministic tests for generator validity, uniqueness, difficulty and persistence where relevant.
5. Run the Godot headless import/test commands and Python checks. If unable, report **not run** with reason; never imply success.
6. Before a release, check real iOS/Android devices, accessibility, Apple 4.3, assets/rights, privacy, crash reporting and store metadata.
7. Record the result and what the evidence changed in `docs/research/decision-log.md`.

## Engineering invariants

- Pin to stable Godot **4.7.2** until a tested upgrade PR changes the pin.
- Use **GDScript**; avoid C# and native plugins unless a capability demands them.
- Gameplay logic must be pure/testable without a UI scene wherever feasible.
- Never add copied iconography, level packs, sounds, screenshots or trade dress.
- No arbitrary third-party SDK copied from a gist; audit version, maintenance, licenses, privacy and mobile platform support.
- Keep independent Godot projects per game. Share tested code via explicitly versioned add-ons or vendoring; never fragile `../` resource paths.
- Keep app IDs, certificates, Apple/Google credentials and API tokens out of git. Do not make publishing automatic.
- Never fabricate API metrics, keyword volumes, reviews, retention, LTV or competitor weaknesses.
- Prefer meaningful PRs and passing tests over agent-generated noise.

## Agent responsibilities (roles, not separate services)

- **Scout:** Find primary demand evidence, collect source URLs and dates, challenge ranking assumptions.
- **Producer:** Write a one-page game hypothesis with differentiation, monetization and stop criteria.
- **Builder:** Deliver a complete game loop, original visuals and accessible input, with pure rules.
- **Reviewer:** Try to falsify uniqueness, correctness, mobile usability, security and policy compliance.
- **Publisher:** Validate exports, metadata, receipts/consent where used and rollback plan. Requires human approval.

## Done means

A prototype is done only with clear instructions, reproducible rules tests, launch screen on a real target device (or explicitly marked unverified), and a decision record. A shipped game additionally needs platform builds, store approval, privacy disclosure, install attribution and retention instrumentation.

**Never conflate a working prototype with validated organic demand.**
