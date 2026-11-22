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
| [09 — Execution control](09-EXECUTION-CONTROL.md) | Immutable audit baseline, lifecycle, owners, milestone gates, defect severity, proof artifacts, release vs research build contract |
| [10 — Research protocols](10-EXPERIMENT-PROTOCOLS.md) | Preregistration, matched conditions, denominators, consent, missingness and go/no-go |
| [11 — Evidence and release packets](11-EVIDENCE-AND-RELEASE-PACKETS.md) | Actual build proof, platform QA, test ownership, sign-off and rollback |
| [12 — M1/M2 execution checklist](12-M1-M2-EXECUTION-CHECKLIST.md) | Operator-ready Unity 6.3 preflight, licensed Editor/Android/iOS build proof and compact-phone test campaign |
| [Verification matrix](verification-matrix.json) | Machine-checked 37-feature × 46-test contracts; no fabricated verification evidence |

## Current-state reconciliation and two practical playbooks

**Source state:** concurrent Evergrove implementation commits have added **v4 world saves, 81-key translations, a 45-entry generated puzzle diagnostics catalog (12 authored + 33 geometry variants), staged hints and improved release-mode guards**. These remain **source-implemented, not Unity-device or player-verified**. The original immutable audit baseline below is *historical*. Always inspect the exact latest commit and any competing open feature PR before upgrading a feature's status.

- [10 — Preregistered experiments](10-EXPERIMENT-PROTOCOLS.md): matched-board controls, allocation, neutral observation and truthful denominators.
- [11 — Build evidence and release packets](11-EVIDENCE-AND-RELEASE-PACKETS.md): artifact checksums, physical device QA, sign-off, rollback and unverified iOS-export caveat.
- [12 — M1/M2 execution checklist](12-M1-M2-EXECUTION-CHECKLIST.md): concrete commands and evidence capture for the presently blocked build/device gates.

**A complete specification does not mean 37 completed features.** No feature is `Verified` without real evidence tied to the target build. The as-built [handbook](../README.md) was updated for v4 in PR #49; keep it synchronized with subsequent changes. **The exact source commit remains the authority** whenever documentation and code differ.

## M1 readiness repair (source-level only)

The Unity 6.3 project previously pinned the uGUI **2.6.0** core package, which belongs to the Unity 6.6 line. The manifest now requests **uGUI 2.0.0** to match Unity 6.3, with a CI regression check. A successful **licensed** GameCI export must emit `evergrove-build-provenance.json` containing per-file SHA256, source SHA, workflow link, requested Unity version and research build channel; missing/empty platform artifacts fail closed. Six synthetic Python tests cover this tool.

**Not passed by the repair:** BLD-001 (real Editor compile), BLD-002/003 (real platform export), BLD-004/005/006 (physical device and suspend/resume), Unity accessibility, or player trials. Follow [Release evidence packets](11-EVIDENCE-AND-RELEASE-PACKETS.md) and the [manual workflow](../../../../.github/workflows/evergrove-unity-playtest.yml). The Android synthetic test file is **not** a real APK.

## Specification control (2026-11-22 v2)

**Source audited at immutable SHA:** [`84713febd1e9d5921706d1916132b309b74a604f`](https://github.com/streamentry/game-foundry/commit/84713febd1e9d5921706d1916132b309b74a604f). This is a historic *audit baseline*, not a completed Unity build. The current source branch may include subsequent fixes.

**Responsible roles:** Product Owner; Gameplay Engineer; Unity/Release Engineer; UX & Accessibility QA; Localization Lead; independent Native Reviewer; Research Moderator; Store/Legal Owner. Roles are accountability categories, **not assigned individuals**. Each Ready feature needs a real assignee.

**Lifecycle:** `Proposed → Ready → In Progress → Implemented-Unverified → Verified` or `Deferred/Rejected`; `Blocked` is independent. **Verified requires a build SHA and mandatory test artifacts, not just code or a green pure-C# job.** See [09 — Execution control](09-EXECUTION-CONTROL.md).

**Evidence-first gates:** M0 specs → M1 Unity compile/build → M2 two physical devices and save/privacy → M3 first-session usability → M4 properly matched narrative-value study → M5 certified locale/text architecture → M6 commercial release. Do not implement replay, daily content or expansion systems as a substitute for a failed gate.

**Thirty draft locales (81 runtime keys each) ≠ thirty production-ready languages.** The 37 backlog features are conditional opportunities, **not a promise to ship 37**. Current `verification-matrix.json` contains **no certified test evidence**.

## What exists versus what is only specified

**In repository, not device-certified:** 12 authored stream puzzles (six 3×3 and six 4×4); 33 choice-conditioned follow-up variants; **45 deterministic source-derived board profiles**; one-step undo/restart, **v4 three-stage hints** (row → tile → one alignment); non-color leak markers, visible save-failure warning, a compact first-screen board, source-grounded journal and endings; deterministic tree/bridge/meadow consequences and three NPCs; dry-to-flowing water; 2D default and optional 2.5D miniature; offline JSON save/backup with v1/v2/v3 → v4 migration; opt-in in-memory local playtest counters; 30 selectable **draft** language choices with **81 keys** each.

**Still unverified:** licensed Unity Editor compilation, signed iOS/Android binaries, real-device playability, full crash safety, screen-reader behavior, proper RTL/Indic shaping, native-language quality, target-audience demand, D1/D7 return, paid conversion, store name clearance. No document here should be read as evidence these gates passed.

**Still partial or proposed:** first-minute time-to-action and actual readability, observed puzzle difficulty calibration, full causal before/after reveal, richer ecology constraints, formal bounded NPC memory, certified RTL/Indic rendering, replay garden, optional return streams, multi-step undo, haptics, postcards and commercial experiments. Source code for staged hints, derived board diagnostics and a basic causal journal exists, but no player/device validation establishes its quality.

## Priority and status vocabulary

- **P0:** necessary for a credible, accessible and testable first commercial-quality slice. Some P0 polish items (sound/haptics) may be deferred if device/accessibility blockers remain.
- **P1:** differentiated depth to build **only after** the core causal loop is understood and preferred by players.
- **P2:** optional expansion or distribution optimization after product evidence.
- **Reject/avoid:** scope, coercion or infrastructure that weakens the promise without evidence.
- **Implemented in code ≠ verified on device.** Use **Existing / Partial / Proposed / Unverified** precisely. The backlog's numerical scores are **assumptions for discussion**, not measurements or market forecasts.

## Operating rule

The next real milestone is **one build in target players' hands**, not 37 completed tickets. Validate whether people can solve a stream, recognize the village change, remember why it happened, and **voluntarily want to return**. If the living-world layer does not outperform a matched puzzle-only experience, simplify it instead of adding features.

**Documentation change only:** this directory adds no runtime code, asset, SDK, telemetry, license, service or release approval.
