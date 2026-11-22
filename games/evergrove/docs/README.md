# Evergrove handbook — product, game rules, architecture and release evidence

**Updated:** 2026-11-22. **Scope:** the actual `games/evergrove` Unity/C# implementation on the default branch, not a projected 2036 AI world. **Audience:** founder, producer, designer, engineer, QA lead, localization reviewer, and AI coding agent.

> **One promise:** A small place changes because the player was there. Show the consequence first; explain it second. Never substitute an impressive-looking feature list for observed player attachment.

## Two complementary documentation layers

- **This handbook (`docs/*.md`):** the **as-built implementation**—exact current code rules, save contract, rendering, localization, QA limitations, and falsifiable research design. Read this when debugging or changing source behavior.
- **[Product specifications and 37-feature backlog](specs/README.md):** **future proposals and acceptance criteria**, including 13 P0, 16 P1 and 8 P2 feature candidates. Read those when deciding what to test/build next; `Proposed` or `P0` never means implemented or approved.
- **Precedence:** actual code + passing tests describe present behavior; the feature backlog describes possibilities; verified player data and a new decision record authorize product direction. If they conflict, **call out and resolve** the difference, not quietly upgrade proposals into capabilities.


## Read in this order

| Document | Why it exists | Who needs it |
| --- | --- | --- |
| [Product brief and positioning](PRODUCT.md) | Audience, promise, differentiation, non-goals, commercial assumptions | Founder, producer, marketing |
| [Gameplay and world rules](GAMEPLAY_AND_WORLD_RULES.md) | Exact puzzle math, branching, consequences, limits, state transitions | Game design, gameplay programmer, QA |
| [Architecture and source map](ARCHITECTURE.md) | Components, rendering, localization, optional backend, trust boundaries | Engineers, AI agents |
| [Save state and migration](SAVE_AND_MIGRATION.md) | v1 → v2 → v3 → v4, invariants, backup, restore, failures | Client engineer, QA |
| [Player experience and content](PLAYER_EXPERIENCE.md) | First-use, screens, inclusive interaction, narrative and 30-locale status | Design, writing, accessibility |
| [Validation and distribution](VALIDATION.md) | Experiments, metrics, confounds, falsification, ASO/distribution | Founder, product research |
| [Build and QA runbook](BUILD_AND_QA.md) | Commands, exact acceptance checks, release gates, rollback | Engineers, QA, release |
| [Decisions, risks and open questions](DECISIONS_AND_RISKS.md) | Trade-offs, strongest objections, explicit unknowns, prioritized action | Everyone |
| [Tóm tắt chiến lược (Tiếng Việt)](TOM_TAT.vi.md) | Bản tóm tắt tiếng Việt có thể đưa cho team | Founder, collaborators |

**Existing canonical companions:** [Design system](../DESIGN.md), [localization status](../LOCALIZATION.md), [original build QA](../BUILD_QA.md), [founder selection and 7-day experiment](../../../docs/research/evergrove-build-decision-2026-11-22.md), [2D vs 2.5D test protocol](../../../docs/research/evergrove-visual-comparison-protocol-2026-11-22.md), [Unity engine ADR](../../../docs/decisions/2026-11-22-evergrove-engine-unity-csharp.md), [visual ADR](../../../docs/decisions/2026-11-22-evergrove-visual-direction-2p5d.md).

## Evidence ladder — do not skip levels

| Evidence level | Current claim | What it actually means |
| --- | --- | --- |
| **Implemented in repository** | Unity/C# 12-chapter loop, deterministic branch-sensitive watercourses, 2D and opt-in 2.5D world, 3 authored neighbors, local save, 30 selectable language catalogs | Code exists; not equivalent to a verified native runtime |
| **Verified in CI** | Pure .NET 8 puzzle/state tests (including 33 ecological variants), static Unity project contracts, optional backend tests; separate Godot games' checks | Logic/test harness passed, not real Unity Editor/device QA |
| **Unverified on device** | Touch and input on phones, font shaping, accessibility APIs, Unity native exports, frame time, battery, save recovery under force-kill | Must be tested with actual Unity Editor and target devices |
| **Unverified with players** | Fun, 1–3 minute session fit, 45+ suitability, voluntary return, preference to leading games, distribution/ASO, economics | Hypothesis, not a proven market opportunity |
| **Not implemented** | Autonomous simulated ecosystem, runtime learning AI/NPCs, persistent multiplayer, production Cloudflare synchronization, UGC marketplace, paid monetization | Do not pitch as shipped or promise delivery |

The **33 variants** are three oriented presentations of each of chapters 2–12, with one protected pipe. They are **not 33 independently designed levels**. In fact, 90° board rotations preserve the same graph's connectivity; the fixed tile changes interaction, not the underlying network complexity. Details: [Gameplay](GAMEPLAY_AND_WORLD_RULES.md).

## Change contract for future contributors

1. Read `AGENTS.md` at repo root, this index, and the relevant domain document.
2. Name the exact **player problem** and the cheapest way to falsify the proposed solution. No feature without a corresponding observable behavior.
3. Preserve local deterministic game truth, existing save migrations and accessible controls.
4. Update the source file, pure C# tests, and the applicable document **in the same PR**. Do not turn a narrative hypothesis into an asserted fact.
5. Prove all 12 authored solutions, all 33 branch variants, restore/undo/reconsider and locale catalog consistency. Separately label Unity Editor and phone tests **not run** when not executed.
6. Keep optional Cloudflare separate from the offline game until a consent, security and actual player-value review approves integration.
7. Update `../../../docs/research/decision-log.md` with observed evidence and a **continue / simplify / stop** decision.

## Quick navigation

From repository root:

```bash
dotnet run --project games/evergrove/Tests/Evergrove.Core.Tests.csproj --configuration Release
python3 tools/verify_evergrove_project.py
```

To get a real player: open `games/evergrove` in Unity Hub with the version in `ProjectSettings/ProjectVersion.txt`; use `Evergrove → Create or Open Playtest Scene` and press Play. A **manual, licensed Unity** GitHub Actions build is described in [Build and QA](BUILD_AND_QA.md). No .NET test proves Unity itself compiles.

## Automated documentation integrity

`python3 tools/check_evergrove_docs.py` runs in repository CI and checks all canonical handbook files, relative links, Markdown code fences, and current 12-puzzle / v4-save / 30-language / 81-key source contracts. These checks **do not validate product claims or native device behavior**; they only prevent simple drift and broken handoffs.

## Documentation rules

- **Code is the authority for current behavior**, not this document when they differ. Flag the mismatch and fix it.
- **Verified** means a test log, actual observation or authoritative source exists. **Designed** means a proposed rule. **Assumption** means no player evidence yet.
- Do not infer originality, global reach, regulatory compliance, trademark clearance or high retention from absence of a familiar competitor.
- Avoid new docs that duplicate this handbook; extend the canonical section and link back from PRs.
