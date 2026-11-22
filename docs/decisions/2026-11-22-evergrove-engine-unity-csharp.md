# ADR: Unity + C# for Evergrove, Godot for the remaining studio games

- **Date:** 2026-11-22
- **Status:** **Accepted for Evergrove's current development and validation path**; **conditional for production** on editor, device, player and operating-cost evidence
- **Scope:** Evergrove (`PH-006`) only; not a studio-wide engine migration
- **Decision owner:** Founder / Game Foundry
- **Related:** [Evergrove game](../../games/evergrove/README.md) · [Evergrove build hypothesis](../research/evergrove-build-decision-2026-11-22.md) · [Portfolio architecture](../architecture.md)

## Decision

**Use Unity 6.3 LTS + C# for Evergrove's client. Keep Godot + GDScript as Game Foundry's default for other small puzzle games.**

Treat this as the best *current engineering bet* for a decade-long, mobile-first **living-world** aspiration, not as a claim that Unity is inherently superior or that any particular engine will remain unchanged for ten years. The exact Unity editor version is pinned in the Evergrove project and must be upgraded deliberately.

The game begins with a tiny **offline-first, deterministic world**: a tactile stream puzzle, three NPCs, locally remembered player choices, and visible second-order consequences. **No cloud service, LLM inference, multiplayer, UGC or marketplace is required to play.**

The important reusable asset is the **game model and simulation rules**, not the renderer. Keep puzzle constraints, NPC/world transitions and save-schema validation in ordinary independently testable C#. Build the visible world and presentation in Unity. Avoid building a speculative engine-agnostic framework until a concrete need justifies it.

**Cloudflare Workers + Durable Objects** remain a separate, optional backend in **TypeScript**, not C#. The current example `WATER_TREE` API **does not synchronize** the game's actual puzzle state, NPC histories or world decisions. Do not market it as production multiplayer or deploy it as a mandatory dependency.

## Why this choice

The ambition is a world that **reacts coherently, remembers consequential actions and remains welcoming to mobile players**; future 2.5D/3D presentation, simulations and richer interaction may benefit from Unity's established C# workflows, cross-device delivery ecosystem, tooling and available middleware.

Unity is a reasonable fit for that possible evolution and the founder's explicit preference for C#. It may lower future integration friction **if those more sophisticated needs emerge**. That benefit is a design/engineering judgment, not observed player evidence.

The causal foundation of a masterpiece is **meaningful decisions, legible consequences and player affection**. An AI dialogue engine or an infinite world does not automatically improve those outcomes.

## Alternatives and counterarguments

| Alternative | Strongest case | Why not selected for Evergrove now |
| --- | --- | --- |
| **Godot + GDScript** | Lightweight, open source (MIT), excellent 2D iteration, vendor independence and small-team economics. | Would reverse the explicit C# direction and require a separate path for potential C#-heavy simulations. **Could be superior** if Evergrove remains mostly a simple 2D puzzle. |
| **Godot + C#** | Open engine while retaining language reuse for pure rules. | Mobile/Web C# export and platform maturity should be individually verified against concrete release requirements before staking the product on it. |
| **Unity + C#** | Mature C# client development, broader production middleware and 2.5D/3D growth options. | **Selected provisionally**; heavier editor/runtime, proprietary license, package/version dependency and possible commercial fees are real costs. |

**Strongest opposing case:** Unity adds engineering/QA cost to what may ultimately be a small deterministic 2D puzzle with a thin story layer. Godot could then deliver the same player value faster and with greater long-term ownership. The engine itself is **not** a moat.

## Architecture guardrails

1. **Domain-first:** `games/evergrove/Assets/Scripts/Core/` contains testable puzzle and world rules without Unity APIs. Preserve semantic versioning of saved state and check migration needs before changing it.
2. **Presentation is replaceable, not free to replace:** Unity scenes, animation, assets and UI will still entail migration work. Do not promise painless engine switching.
3. **Offline-first:** Keep local play and progression functional without networking. Remote sync is an opt-in feature only after demonstrated player need and a designed reconciliation/account model.
4. **AI comes after gameplay:** First verify deterministic consequences are understandable and increase voluntary return. Only introduce bounded AI if measured incremental enjoyment exceeds latency, cost, reliability and privacy downsides.
5. **Ownership and compliance:** Monitor Unity licensing, third-party SDK/asset terms, mobile export constraints, store rules and build-system costs. No automatic production publishing.
6. **Portfolio isolation:** Do not rewrite other Godot games to C#. Reassess their engine independently.
7. **Simple distribution:** A living-world game still needs a realistic organic acquisition or community wedge. Avoid confusing high concept appeal with discoverability.

## Decision gates: real hardware before larger scope

**Next seven-day technical evaluation, starting when editor/device access is available:**

- Open/import the project in the pinned Unity editor and resolve compile/package issues. Record exact editor version and reproducible build steps.
- Produce **testable iOS and Android builds** on real representative devices, including at least one modest device where practical. Check cold start, touch targets, safe areas, text readability, focus/accessibility, orientation and memory/performance regressions.
- Complete the six-puzzle path; verify undo/restart, deterministic NPC consequences, save/restore across app termination, and full play while offline.
- Capture concrete blockers, crash symptoms, time spent on toolchain/export issues and a simple operating-cost estimate. Document what could be achieved more cheaply in Godot.

**Technical continuation:** Both mobile platforms can complete the primary game loop and resume reliably without critical bugs or unreasonable build/iteration overhead. A successful pure C# CI run **does not count** as a verified Unity Editor or device build.

**Technical reconsideration:** Revisit a Godot implementation **before building more world features** if Unity blocks practical mobile delivery, its tooling/licensing burden is disproportionate, or the project stays purely 2D and no specific Unity capability justifies the extra cost. Do not migrate merely because of opinions about engine popularity.

**Product continuation, separate gate:** Compare *Living world* with the **identical puzzle-only control** among 20–30 relevant recruited players as an exploratory test. Record comprehension, voluntary replay, actual D1/D7 return with denominators, and the player's explanation of causal changes. **Stop or simplify the simulation if the extra layer adds no observable value**; a positive small-sample result is not statistical proof or market validation.

## Review triggers

Review this ADR after the first verified Unity mobile build and initial comparative player test, or sooner if any of these happen: licensing economics change materially; a platform cannot be shipped reliably; a core Unity dependency becomes a serious risk; or observed gameplay no longer requires features that justify Unity.

Record evidence and changes in [the decision log](../research/decision-log.md). Until then, **Unity/C# is the working choice, not a permanent ten-year commitment**.

## What is still unknown

The Unity editor/IL2CPP and signed device builds have **not yet been verified** in the current repo evidence. Neither a global-hit forecast, a player-retention improvement, an AI moat, nor monetization/distribution economics have been demonstrated. This ADR records a deliberate, reversible choice with falsifiable gates—not a completed validation.
