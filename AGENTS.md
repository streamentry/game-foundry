# AGENTS.md — Game Foundry

## Mission

Build **distinctive, accessible, mobile-first games** from **verified user-search demand**. The optimization target is validated long-term portfolio value, not game count or lines of code.

## Source of truth

1. `docs/research/README.md` governs demand research and evidentiary labels.
2. `docs/ideas/candidates.csv` records **hypotheses**, not approved production titles.
3. `docs/architecture.md` governs project boundaries.
4. `DESIGN.md` governs product quality, with per-game design specs overriding aesthetics only.
5. Each game's `project.godot`, tests and README govern build/run commands.

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
| **Falsification** | What is the cheapest test that could prove this direction wrong? |
| **Stop** | What measured result would cause us to stop or pivot? |
| **Evidence** | What test, external response, or artifact will verify the outcome? |

If answers are weak, run the falsification test before expanding the implementation.

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
