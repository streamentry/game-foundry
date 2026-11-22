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
| [Verification matrix](verification-matrix.json) | Machine-checked 37-feature × 46-test contracts; no fabricated verification evidence |

## Specification control (2026-11-22 v2)

**Source audited at immutable SHA:** [`84713febd1e9d5921706d1916132b309b74a604f`](https://github.com/streamentry/game-foundry/commit/84713febd1e9d5921706d1916132b309b74a604f). This is a historic *audit baseline*, not a completed Unity build. The current source branch may include subsequent fixes.

**Responsible roles:** Product Owner; Gameplay Engineer; Unity/Release Engineer; UX & Accessibility QA; Localization Lead; independent Native Reviewer; Research Moderator; Store/Legal Owner. Roles are accountability categories, **not assigned individuals**. Each Ready feature needs a real assignee.

**Lifecycle:** `Proposed → Ready → In Progress → Implemented-Unverified → Verified` or `Deferred/Rejected`; `Blocked` is independent. **Verified requires a build SHA and mandatory test artifacts, not just code or a green pure-C# job.** See [09 — Execution control](09-EXECUTION-CONTROL.md).

**Evidence-first gates:** M0 specs → M1 Unity compile/build → M2 two physical devices and save/privacy → M3 first-session usability → M4 properly matched narrative-value study → M5 certified locale/text architecture → M6 commercial release. Do not implement replay, daily content or expansion systems as a substitute for a failed gate.

**Thirty draft locales ≠ thirty production-ready languages.** The 37 backlog features are conditional opportunities, **not a promise to ship 37**. Current `verification-matrix.json` contains **no certified test evidence**.

## What exists versus what is only specified

**In repository, not device-certified:** 12 authored stream puzzles (six 3×3 and six 4×4); 33 choice-conditioned follow-up variants; deterministic tree/bridge/meadow consequences and three NPCs; journal and endings; one-step undo/restart and a **v4 save-safe three-stage hint** (row → tile → one aligned connection); **45 source-derived board profiles** with human data correctly unmeasured; a compact first puzzle and visible save-failure warning; dry-to-flowing water; 2D default and optional 2.5D miniature; offline JSON save with versioning, backup and migration; opt-in in-memory local playtest counters; 30 selectable **draft** localization catalogs.

**Still unverified:** licensed Unity Editor compilation, signed iOS/Android binaries, real-device playability, full crash safety, screen-reader behavior, proper RTL/Indic shaping, native-language quality, target-audience demand, D1/D7 return, paid conversion, store name clearance. No document here should be read as evidence these gates passed.

**Still only proposed or partial:** a *measured* first minute, full causal reveal/journal, human-calibrated difficulty and visual authoring tool, deep ecological constraints, formal NPC memory, advanced text shaping, replay, return streams, multi-step undo, haptics, postcards and commercial experiments. The three-stage help and deterministic puzzle profiles are now implemented **in source**, not yet proven on devices or by users.

## Recent implementation evidence

- [45 source-derived puzzle profiles](puzzle-catalog.generated.json) generated by [PuzzleDiagnostics](../../Assets/Scripts/Core/PuzzleDiagnostics.cs) and checked in CI; observed difficulty/timing are **null**.
- [v4 staged hint system](../../Assets/Scripts/Core/WorldModel.cs), [frontier target planner](../../Assets/Scripts/Core/HintPlanner.cs) and Unity [first-puzzle focus + save warning](../../Assets/Scripts/Unity/EvergroveApp.cs).
- Five new UI texts are **draft translated**, bringing each locale to 81 keys. Physical-device, native review and real player gates still apply.

## Priority and status vocabulary

- **P0:** necessary for a credible, accessible and testable first commercial-quality slice. Some P0 polish items (sound/haptics) may be deferred if device/accessibility blockers remain.
- **P1:** differentiated depth to build **only after** the core causal loop is understood and preferred by players.
- **P2:** optional expansion or distribution optimization after product evidence.
- **Reject/avoid:** scope, coercion or infrastructure that weakens the promise without evidence.
- **Implemented in code ≠ verified on device.** Use **Existing / Partial / Proposed / Unverified** precisely. The backlog's numerical scores are **assumptions for discussion**, not measurements or market forecasts.

## Operating rule

The next real milestone is **one build in target players' hands**, not 37 completed tickets. Validate whether people can solve a stream, recognize the village change, remember why it happened, and **voluntarily want to return**. If the living-world layer does not outperform a matched puzzle-only experience, simplify it instead of adding features.

**Scope and authorization:** The specs directory itself is a future-feature proposal and cannot authorize releases, SDKs, live services or production approval. A separately reviewed implementation PR may change client code and test artifacts; completed source changes are listed above, and native Unity/device/player evidence remains an explicit external gate.
