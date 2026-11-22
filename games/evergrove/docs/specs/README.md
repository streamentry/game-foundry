# Evergrove — product specifications

**Status:** research-backed **proposals and acceptance criteria**, not shipped features, verified builds, production approval, or measured player outcomes.  
**Baseline date:** 2026-11-22. **Scope:** `games/evergrove/` (Unity 6.3/C#).  
**Product promise:** **A little world that changes because you were here.**

These specifications translate the supplied *Evergrove: evidence-backed product and feature strategy* research into a traceable, testable implementation plan. They complement, rather than overwrite, the [current project README](../../README.md), [design system](../../DESIGN.md), [build checklist](../../BUILD_QA.md), and [localization reality check](../../LOCALIZATION.md).

## Read in this order

| Document | Purpose |
| --- | --- |
| [01 — Product and experience](01-PRODUCT-AND-EXPERIENCE.md) | Audience, promise, first minute/10 minutes/7 days, product counterargument, top-ten priorities, art and commercial constraints |
| [02 — Full feature backlog](02-FEATURE-BACKLOG.md) | **37 individually specified features**: 13 P0, 16 P1, 8 P2; implementation status, player-visible behavior, acceptance, dependencies, risk, metrics, illustrative scoring; explicit rejections |
| [03 — Puzzle and content system](03-PUZZLES-AND-CONTENT.md) | Solver metadata, fair difficulty, progressive hints, authored ecology constraints, recombination, generator-as-tool, replay |
| [04 — Living world and narrative](04-WORLD-AND-NARRATIVE.md) | Choice → consequence → memory → next puzzle, NPC identities, state contract, causal journal, endings |
| [05 — Accessibility and localization](05-ACCESSIBILITY-AND-LOCALIZATION.md) | Touch/vision/motor/audio/screen-reader gates, 30 draft locales, shaping and native review, device matrix |
| [06 — Engineering and release QA](06-ENGINEERING-AND-RELEASE.md) | Existing code boundaries, save/privacy/device test plan, performance, build and shipping gates |
| [07 — Validation and go-to-market](07-VALIDATION-AND-GTM.md) | Seven-day experiment, six-week sequence, matched controls, thresholds, ASO, ethical pricing, naming check |
| [08 — Evidence and decisions](08-EVIDENCE-AND-DECISIONS.md) | Repo-source map, external evidence, confidence levels, strongest opposing case, load-bearing assumptions, decision log |

## What exists versus what is only specified

**In repository, not device-certified:** 12 authored stream puzzles (six 3×3 and six 4×4); 33 choice-conditioned follow-up variants; deterministic tree/bridge/meadow consequences and three NPCs; journal and endings; one-step undo/restart/one-tile hint; dry-to-flowing water; 2D default and optional 2.5D miniature; offline JSON save with versioning, backup and migration; opt-in in-memory local playtest counters; 30 selectable **draft** localization catalogs.

**Still unverified:** licensed Unity Editor compilation, signed iOS/Android binaries, real-device playability, full crash safety, screen-reader behavior, proper RTL/Indic shaping, native-language quality, target-audience demand, D1/D7 return, paid conversion, store name clearance. No document here should be read as evidence these gates passed.

**Proposed, not implemented:** the redesigned first minute, full causal reveal/journal, puzzle-authoring metadata, progressive hint ladder, deep ecological puzzle constraints, formal bounded NPC memory, advanced text rendering, replay garden, daily optional streams, multi-step undo, haptics, postcards and most commercial experiments.

## Priority and status vocabulary

- **P0:** necessary for a credible, accessible and testable first commercial-quality slice. Some P0 polish items (sound/haptics) may be deferred if device/accessibility blockers remain.
- **P1:** differentiated depth to build **only after** the core causal loop is understood and preferred by players.
- **P2:** optional expansion or distribution optimization after product evidence.
- **Reject/avoid:** scope, coercion or infrastructure that weakens the promise without evidence.
- **Implemented in code ≠ verified on device.** Use **Existing / Partial / Proposed / Unverified** precisely. The backlog's numerical scores are **assumptions for discussion**, not measurements or market forecasts.

## Operating rule

The next real milestone is **one build in target players' hands**, not 37 completed tickets. Validate whether people can solve a stream, recognize the village change, remember why it happened, and **voluntarily want to return**. If the living-world layer does not outperform a matched puzzle-only experience, simplify it instead of adding features.

**Documentation change only:** this directory adds no runtime code, asset, SDK, telemetry, license, service or release approval.
