# 02 — Full feature backlog (37 candidates)

**Source:** supplied 2026-11-22 Evergrove Deep Research synthesis, cross-checked against repository source/README; implementation status is **not** a build verification. [Evidence keys](08-EVIDENCE-AND-DECISIONS.md).

**Scoring is illustrative, not measured:** relative Reach (R), Impact (I), Confidence in impact (C) and Effort (E), each 1–5; **score = R × I × C ÷ E**. This is a RICE-like prioritization heuristic without real user counts, time estimates or statistically estimated confidence. Scores **do not** override release blockers, dependency order, ethics or player-test results. Risk is qualitative. A feature labeled P0 is not automatically required in the first 7-day test; prioritize builds and comprehension over polish.

**Execution states:** `Proposed → Ready → In Progress → Implemented-Unverified → Verified`; alternatively `Deferred` or `Rejected`. `Blocked` is a separate flag. Existing code, copied R/I/C/E scores or passing pure C# tests are **not a verified Unity/device feature**. The authoritative [gate, owner, acceptance test and evidence registry](verification-matrix.json) is explained in [execution control](09-EXECUTION-CONTROL.md).

**Priority is not execution order:** gate M1 build and M2 reliability must pass before expanding the feature set. P0 audio/haptics EG-012/013 are **M6 conditional polish**, not M1 blockers. EG-006/010 are launch blockers **only for locales actually enabled for release**. Uncommitted P1/P2 remain research candidates.

| ID | Feature | Priority | State | Type | R/I/C/E | Score | Gate | Owner role | Verification IDs |
| --- | --- | --- | --- | --- | --- | ---: | --- | --- | --- |
| [EG-001](#eg-001--verified-phone-builds) | Verified phone builds | P0 | Proposed | Reliability | 5/5/5/2 | 62.5 | M1 | Unity/Release Engineer | BLD-001, BLD-002, BLD-003, BLD-004, BLD-005, BLD-006 |
| [EG-002](#eg-002--first-60-second-complete-loop) | First 60-second complete loop | P0 | Implemented-Unverified | Core quality | 5/5/3/2 | 37.5 | M3 | UX & Accessibility QA | PUZ-005, VAL-001 |
| [EG-003](#eg-003--consequence-first-visual-reveal) | Consequence-first visual reveal | P0 | Proposed | Core quality | 5/5/3/2 | 37.5 | M3 | Product Owner | WLD-001, VAL-001 |
| [EG-004](#eg-004--puzzle-solver-and-difficulty-metadata) | Puzzle solver and difficulty metadata | P0 | Implemented-Unverified | Core quality | 5/4/4/3 | 26.7 | M3 | Gameplay Engineer | PUZ-001, PUZ-003 |
| [EG-005](#eg-005--progressive-non-punitive-hints) | Progressive non-punitive hints | P0 | Implemented-Unverified | Core quality | 5/4/4/2 | 40.0 | M3 | Gameplay Engineer | PUZ-002, VAL-001 |
| [EG-006](#eg-006--correct-complex-script-text-rendering) | Correct complex-script text rendering | P0 | Proposed | Release quality | 4/5/5/4 | 25.0 | M5 | Localization Lead | LOC-002, A11Y-005 |
| [EG-007](#eg-007--large-forgiving-touch-and-readable-text) | Large, forgiving touch and readable text | P0 | Proposed | Release quality | 5/5/5/2 | 62.5 | M2 | UX & Accessibility QA | A11Y-001, A11Y-005 |
| [EG-008](#eg-008--recoverable-saves-under-interruption-and-corruption) | Recoverable saves under interruption and corruption | P0 | Implemented-Unverified | Reliability | 5/5/5/2 | 62.5 | M2 | Unity/Release Engineer | SAVE-001, SAVE-002, SAVE-003 |
| [EG-009](#eg-009--offlineprivacy-binary-verification) | Offline/privacy binary verification | P0 | Proposed | Trust | 5/5/5/2 | 62.5 | M2 | Unity/Release Engineer | PRIV-001, PRIV-002 |
| [EG-010](#eg-010--native-certified-launch-locale-subset) | Native-certified launch locale subset | P0 | Proposed | Release quality | 4/4/5/3 | 26.7 | M5 | Localization Lead | LOC-001, LOC-002, LOC-003, LOC-004 |
| [EG-011](#eg-011--non-color-connectivity-and-contrast) | Non-color connectivity and contrast | P0 | Implemented-Unverified | Release quality | 5/4/5/2 | 50.0 | M3 | UX & Accessibility QA | A11Y-002 |
| [EG-012](#eg-012--subtle-audio-feedback-and-ambience) | Subtle audio feedback and ambience | P0 | Implemented-Unverified | Polish | 4/3/3/3 | 12.0 | M6 | Unity/Release Engineer | UX-001 |
| [EG-013](#eg-013--optional-haptic-response) | Optional haptic response | P0 | Proposed | Polish | 3/2/3/1 | 18.0 | M6 | Unity/Release Engineer | UX-001 |
| [EG-101](#eg-101--causal-journal) | Causal journal | P1 | Implemented-Unverified | Differentiation | 5/4/3/2 | 30.0 | M4 | Gameplay Engineer | WLD-003, VAL-002 |
| [EG-102](#eg-102--three-depth-authored-consequences) | Three-depth authored consequences | P1 | Proposed | Differentiation | 5/5/3/4 | 18.8 | M4 | Product Owner | WLD-001, WLD-004, VAL-002 |
| [EG-103](#eg-103--bounded-npc-memory-tokens) | Bounded NPC memory tokens | P1 | Proposed | Differentiation | 4/4/3/3 | 16.0 | M4 | Gameplay Engineer | WLD-002, VAL-002 |
| [EG-104](#eg-104--distinct-character-values-and-voices) | Distinct character values and voices | P1 | Proposed | Differentiation | 4/3/3/3 | 12.0 | M4 | Product Owner | WLD-002, VAL-002 |
| [EG-105](#eg-105--ecological-constraints-change-reasoning) | Ecological constraints change reasoning | P1 | Proposed | Differentiation | 5/5/3/4 | 18.8 | M4 | Gameplay Engineer | PUZ-004, VAL-002 |
| [EG-106](#eg-106--recombine-learned-mechanics) | Recombine learned mechanics | P1 | Proposed | Core quality | 5/4/3/4 | 15.0 | M4 | Gameplay Engineer | PUZ-003, PUZ-004 |
| [EG-107](#eg-107--transparent-difficulty-choice) | Transparent difficulty choice | P1 | Proposed | Accessibility | 4/4/3/3 | 16.0 | M5 | UX & Accessibility QA | PUZ-003, A11Y-001 |
| [EG-108](#eg-108--optional-return-stream) | Optional return stream | P1 | Proposed | Expansion | 4/3/2/3 | 8.0 | M6 | Product Owner | VAL-004 |
| [EG-109](#eg-109--meaningfully-different-new-garden) | Meaningfully different new garden | P1 | Proposed | Expansion | 4/4/2/4 | 8.0 | M6 | Gameplay Engineer | PUZ-003, WLD-004 |
| [EG-110](#eg-110--solver-assisted-human-authoring-tool) | Solver-assisted human authoring tool | P1 | Proposed | Tooling | 5/4/3/4 | 15.0 | M5 | Gameplay Engineer | TOOL-001, PUZ-001 |
| [EG-111](#eg-111--ten-step-undo-history) | Ten-step undo history | P1 | Proposed | Accessibility | 3/3/4/2 | 18.0 | M5 | Gameplay Engineer | SAVE-003, PUZ-002 |
| [EG-112](#eg-112--large-board-focus-mode) | Large-board focus mode | P1 | Proposed | Accessibility | 4/4/4/3 | 21.3 | M5 | UX & Accessibility QA | A11Y-001, A11Y-005 |
| [EG-113](#eg-113--screen-reader-puzzle-semantics) | Screen-reader puzzle semantics | P1 | Proposed | Accessibility | 3/5/4/4 | 15.0 | M5 | UX & Accessibility QA | A11Y-003 |
| [EG-114](#eg-114--comprehensive-reduced-motion) | Comprehensive reduced motion | P1 | Proposed | Accessibility | 3/4/4/2 | 24.0 | M5 | UX & Accessibility QA | A11Y-004 |
| [EG-115](#eg-115--local-world-postcard) | Local world postcard | P1 | Proposed | Polish | 4/3/2/2 | 12.0 | M6 | Product Owner | UX-002, WLD-003 |
| [EG-116](#eg-116--consequence-driven-ending-comparison) | Consequence-driven ending comparison | P1 | Proposed | Differentiation | 3/3/3/2 | 13.5 | M6 | Product Owner | WLD-004 |
| [EG-201](#eg-201--small-authored-world-micro-events) | Small authored world micro-events | P2 | Proposed | Expansion | 3/3/2/3 | 6.0 | M6 | Product Owner | WLD-001 |
| [EG-202](#eg-202--cosmetic-seasonal-palette) | Cosmetic seasonal palette | P2 | Proposed | Polish | 2/2/2/3 | 2.7 | M6 | UX & Accessibility QA | A11Y-002 |
| [EG-203](#eg-203--explicit-offline-share-card) | Explicit offline share card | P2 | Proposed | Distribution | 3/3/2/2 | 9.0 | M6 | Store/Legal Owner | UX-002, PRIV-002 |
| [EG-204](#eg-204--accessible-first-run-presets) | Accessible first-run presets | P2 | Proposed | Accessibility | 3/3/3/3 | 9.0 | M6 | UX & Accessibility QA | A11Y-001, A11Y-005 |
| [EG-205](#eg-205--one-time-chapter-expansion-support) | One-time chapter expansion support | P2 | Proposed | Commercial | 3/3/2/3 | 6.0 | M6 | Store/Legal Owner | STORE-004, SAVE-003 |
| [EG-206](#eg-206--localized-store-screenshots) | Localized store screenshots | P2 | Proposed | Distribution | 4/3/3/2 | 18.0 | M6 | Store/Legal Owner | STORE-003, LOC-003 |
| [EG-207](#eg-207--aso-positioning-experiments) | ASO positioning experiments | P2 | Proposed | Distribution | 4/4/3/2 | 24.0 | M6 | Store/Legal Owner | STORE-003, VAL-005 |
| [EG-208](#eg-208--tabletpc-expansion-after-mobile-proof) | Tablet/PC expansion after mobile proof | P2 | Proposed | Expansion | 2/3/2/4 | 3.0 | M6 | Unity/Release Engineer | BLD-004, BLD-005, PERF-001 |

## P0 — release and core-quality gates

### EG-001 — Verified phone builds

**State:** Scaffolded; no Unity Editor/device pass recorded. **Type:** Reliability. **Risk:** Build/signing/toolchain delays. **Evidence:** R1,R2. **R/I/C/E:** 5/5/5/2; **illustrative score:** 62.5.

**Execution:** `Proposed`; **M1**; owner role **Unity/Release Engineer**; verification **`BLD-001`, `BLD-002`, `BLD-003`, `BLD-004`, `BLD-005`, `BLD-006`**; expected evidence `games/evergrove/docs/evidence/<SOURCE_SHA>/EG-001/verification.md` (**NOT COLLECTED**); **rollback/stop:** Build failure, device blocker or missing Unity evidence.

- **Player problem:** A plausible prototype may not compile or handle real touch.
- **Player-visible behavior:** Launch and finish the campaign on a compact Android and an iPhone; respect safe areas, back navigation and resume.
- **Acceptance criteria:** Document Unity import/build logs; finish all 12 streams on both devices; zero blocker defects; recover from force-close.
- **Success signal:** Full-campaign pass on 2 device families; blocker count 0.
- **Dependencies:** Unity license, platform modules, Xcode signing, physical devices.

### EG-002 — First 60-second complete loop

**State:** Proposed UX change; dry→flowing water already exists. **Type:** Core quality. **Risk:** Over-directed opening may feel trivial. **Evidence:** R1,R3,Q1. **R/I/C/E:** 5/5/3/2; **illustrative score:** 37.5.

**Execution:** `Implemented-Unverified`; **M3**; owner role **UX & Accessibility QA**; verification **`PUZ-005`, `VAL-001`**; expected evidence `games/evergrove/docs/evidence/<SOURCE_SHA>/EG-002/verification.md` (**NOT COLLECTED**); **rollback/stop:** First action is delayed or first-puzzle comprehension regresses.

- **Player problem:** Players may see menus or a generic pipe board before Evergrove's value.
- **Player-visible behavior:** Start at dry creek with obvious first tile; first rotations extend water; first solve visibly revives a village detail.
- **Acceptance criteria:** No mandatory modal/lore/mode screen; first input obvious; dry→flowing payoff tested on both phones.
- **Success signal:** ≥80% unaided goal comprehension; median first meaningful input <15s (provisional).
- **Dependencies:** EG-001, first-puzzle tuning, presentation/UI.

### EG-003 — Consequence-first visual reveal

**State:** Implemented-Unverified: the post-choice screen now orders illustration, one consequence line, the memory card, then Continue/Change and a settings row; tapping a protected landmark explains it in the status line without spending a move. macOS QA tour screenshots (540x960; 375x667 at 150%) show the memory card above the fold; device and player verification still needed. **Type:** Core quality. **Risk:** Visual clutter or longer forced cutscene. **Evidence:** R1,R4,H1. **R/I/C/E:** 5/5/3/2; **illustrative score:** 37.5.

**Execution:** `Proposed`; **M3**; owner role **Product Owner**; verification **`WLD-001`, `VAL-001`**; expected evidence `games/evergrove/docs/evidence/<SOURCE_SHA>/EG-003/verification.md` (**NOT COLLECTED**); **rollback/stop:** Consequence recall does not improve after two iterations.

- **Player problem:** Text-only explanation makes choices feel cosmetic.
- **Player-visible behavior:** After tree/bridge/meadow selection, frame and highlight the changed landscape before one brief NPC response.
- **Acceptance criteria:** Before/after state remains readable in 2D, 2.5D and reduced motion; no false causal claims.
- **Success signal:** ≥75% recall a specific consequence after 10 minutes (provisional).
- **Dependencies:** EG-001, deterministic scene plan, EG-002.

### EG-004 — Puzzle solver and difficulty metadata

**State:** Unique-solution solver tests exist; authoring metadata proposed. **Type:** Core quality. **Risk:** Solver complexity mistaken for felt difficulty. **Evidence:** R1,Q2,H1. **R/I/C/E:** 5/4/4/3; **illustrative score:** 26.7.

**Execution:** `Implemented-Unverified`; **M3**; owner role **Gameplay Engineer**; verification **`PUZ-001`, `PUZ-003`**; expected evidence `games/evergrove/docs/evidence/<SOURCE_SHA>/EG-004/verification.md` (**NOT COLLECTED**); **rollback/stop:** Any authored board loses unique solution or observed difficulty spikes unexplained.

- **Player problem:** Unique solution does not guarantee fair or engaging difficulty.
- **Player-visible behavior:** Store solution proof, minimum rotations, error traps, hint path, ecological rule, target band and observed solve times per board.
- **Acceptance criteria:** All 12 originals and 33 variants have reproducible validity checks; no unsolvable/multi-solution shipping board; human times recorded separately.
- **Success signal:** No invalid boards; flag >2× adjacent solve-time spikes for review, not automatic rejection.
- **Dependencies:** PipeRules, Tests/Program.cs, representative testers.

### EG-005 — Progressive non-punitive hints

**State:** One-tile Guide Me exists; 3-stage ladder proposed. **Type:** Core quality. **Risk:** Hint reveals too much or implies a unique move when multiple are possible. **Evidence:** R1,Q1,H1. **R/I/C/E:** 5/4/4/2; **illustrative score:** 40.0.

**Execution:** `Implemented-Unverified`; **M3**; owner role **Gameplay Engineer**; verification **`PUZ-002`, `VAL-001`**; expected evidence `games/evergrove/docs/evidence/<SOURCE_SHA>/EG-005/verification.md` (**NOT COLLECTED**); **rollback/stop:** Hint reveals unwanted solutions or becomes misleading.

- **Player problem:** One hint may spoil a puzzle or fail to unstick a player.
- **Player-visible behavior:** Stage 1 highlights a useful region; stage 2 names a tile; stage 3 performs one correct rotation; no penalty/currency.
- **Acceptance criteria:** Each stage makes valid progress on every board/variant; anchored tiles remain fixed; hint state survives undo/restart appropriately.
- **Success signal:** ≥80% of stuck testers resume unaided; <20% report unwanted full spoiler.
- **Dependencies:** EG-004, undo and save contracts.

### EG-006 — Correct complex-script text rendering

**State:** Legacy uGUI Text and best-effort OS fonts; shipping blocker. **Type:** Release quality. **Risk:** Text migration changes layout, focus or performance. **Evidence:** R3,U1. **R/I/C/E:** 4/5/5/4; **illustrative score:** 25.0.

**Execution:** `Proposed`; **M5**; owner role **Localization Lead**; verification **`LOC-002`, `A11Y-005`**; expected evidence `games/evergrove/docs/evidence/<SOURCE_SHA>/EG-006/verification.md` (**NOT COLLECTED**); **rollback/stop:** Any enabled script renders broken glyphs or uncontrolled overflow.

- **Player problem:** RTL and Indic strings can be unreadable despite complete catalogs.
- **Player-visible behavior:** Use Unity Advanced Text Generator/UI Toolkit or a tested equivalent for all player-facing dynamic text, bidi and shaping.
- **Acceptance criteria:** Arabic/Persian/Hebrew/Urdu bidi; Hindi/Bengali/Tamil/Telugu conjuncts; Thai breaks; CJK glyphs; compact/large-text screenshots pass.
- **Success signal:** Zero critical shaping/overflow defects in each enabled locale.
- **Dependencies:** UI architecture decision, licensed font assets, native reviewers.

### EG-007 — Large, forgiving touch and readable text

**State:** Safe-area scroll, large controls and in-game text size 100/125/150% (`TextScale`, Aa control) implemented; device/OS audit still unverified. **Type:** Release quality. **Risk:** Larger targets crowd 4×4 board. **Evidence:** R2,P1,Q1. **R/I/C/E:** 5/5/5/2; **illustrative score:** 62.5.

**Execution:** `Proposed`; **M2**; owner role **UX & Accessibility QA**; verification **`A11Y-001`, `A11Y-005`**; expected evidence `games/evergrove/docs/evidence/<SOURCE_SHA>/EG-007/verification.md` (**NOT COLLECTED**); **rollback/stop:** Targets remain inaccessible or mis-taps block play.

- **Player problem:** Small taps, complex gestures and low text size exclude casual/older users.
- **Player-visible behavior:** One-tap rotation; minimum frequent-action hit areas 44pt iOS/48dp Android; visible pressed state; scalable text.
- **Acceptance criteria:** Every frequent action passes physical target-size audit; no precision swipe; 320–430 logical-width layouts and OS scaling tested.
- **Success signal:** Median ≤2 mis-taps per observed 10-minute session; all key controls operable.
- **Dependencies:** EG-001, layout refactor, target devices.

### EG-008 — Recoverable saves under interruption and corruption

**State:** Versioned JSON v3, backup and migration implemented; device fault tests open. **Type:** Reliability. **Risk:** Platform-specific File.Replace/fallback semantics. **Evidence:** R1,R2. **R/I/C/E:** 5/5/5/2; **illustrative score:** 62.5.

**Execution:** `Implemented-Unverified`; **M2**; owner role **Unity/Release Engineer**; verification **`SAVE-001`, `SAVE-002`, `SAVE-003`**; expected evidence `games/evergrove/docs/evidence/<SOURCE_SHA>/EG-008/verification.md` (**NOT COLLECTED**); **rollback/stop:** Previously valid progress is irrecoverable after interruption.

- **Player problem:** Lost garden history destroys trust and emotional ownership.
- **Player-visible behavior:** Write validated snapshots, recover previous valid save, migrate v1/v2, confirm destructive reset.
- **Acceptance criteria:** 100 scripted/app-force-kill points across rotation/choice/save where feasible; no unrecoverable valid history; corrupted primary restores backup.
- **Success signal:** Zero unrecoverable valid saves in documented fault-injection suite.
- **Dependencies:** WorldModel, WorldStore, Android/iOS storage testing.

### EG-009 — Offline/privacy binary verification

**State:** Offline design and opt-in in-memory playtest exist; network audit open. **Type:** Trust. **Risk:** Platform service traffic confused with app traffic. **Evidence:** R1,R2,P2. **R/I/C/E:** 5/5/5/2; **illustrative score:** 62.5.

**Execution:** `Proposed`; **M2**; owner role **Unity/Release Engineer**; verification **`PRIV-001`, `PRIV-002`**; expected evidence `games/evergrove/docs/evidence/<SOURCE_SHA>/EG-009/verification.md` (**NOT COLLECTED**); **rollback/stop:** Unexplained network transmission or undeclared collection.

- **Player problem:** Offline promise can be broken by SDKs or unexpected traffic.
- **Player-visible behavior:** Full campaign works in airplane mode; no ad/analytics SDK; session notes exist only after consent and export only on request.
- **Acceptance criteria:** Inspect dependencies and production network traffic; no unexplained outbound requests; correct platform privacy disclosures.
- **Success signal:** Zero unexpected transmissions; no identifiers in local report.
- **Dependencies:** Release build, dependency review, privacy policy.

### EG-010 — Native-certified launch locale subset

**State:** 30 complete draft catalogs; no native/device sign-off. **Type:** Release quality. **Risk:** Fewer initial markets; QA cost. **Evidence:** R3,U1,P2. **R/I/C/E:** 4/4/5/3; **illustrative score:** 26.7.

**Execution:** `Proposed`; **M5**; owner role **Localization Lead**; verification **`LOC-001`, `LOC-002`, `LOC-003`, `LOC-004`**; expected evidence `games/evergrove/docs/evidence/<SOURCE_SHA>/EG-010/verification.md` (**NOT COLLECTED**); **rollback/stop:** An enabled locale lacks native/technical/store signoff.

- **Player problem:** Thirty selectable languages can create thirty broken first impressions.
- **Player-visible behavior:** Enable only locales passing translation, shaping, native editorial and screenshot QA; retain draft files in repo.
- **Acceptance criteria:** Every shipping locale has signed reviewer checklist, text/glyph/layout matrix and localized store copy; unverified locales disabled.
- **Success signal:** 100% enabled locales certified; 0 known critical translation defects.
- **Dependencies:** EG-006, EG-007, LOCALIZATION.md.

### EG-011 — Non-color connectivity and contrast

**State:** Color-coded scene/pipe UI exists; formal audit proposed. **Type:** Release quality. **Risk:** Extra strokes obscure small boards. **Evidence:** P1,Q1. **R/I/C/E:** 5/4/5/2; **illustrative score:** 50.0.

**Execution:** `Implemented-Unverified`; **M3**; owner role **UX & Accessibility QA**; verification **`A11Y-002`**; expected evidence `games/evergrove/docs/evidence/<SOURCE_SHA>/EG-011/verification.md` (**NOT COLLECTED**); **rollback/stop:** Flow or protected state is indistinguishable without hue.

- **Player problem:** Players may not distinguish connected, leaking and anchored tiles by hue.
- **Player-visible behavior:** Use pipe shape, port outline, water movement/pattern and labels in addition to color; provide high-contrast mode.
- **Acceptance criteria:** Solve full sample in grayscale/color-filter conditions; all states recognizable without audio or color.
- **Success signal:** No critical state-confusion failures in accessibility test.
- **Dependencies:** PipeGraphic, EG-007, user testing.

### EG-012 — Subtle audio feedback and ambience

**State:** Optional gentle completion chime exists; richer sound proposed. **Type:** Polish. **Risk:** Cost, repetition, sensory overload; defer behind blockers. **Evidence:** R1,H1. **R/I/C/E:** 4/3/3/3; **illustrative score:** 12.0.

**Execution:** `Implemented-Unverified`; **M6**; owner role **Unity/Release Engineer**; verification **`UX-001`**; expected evidence `games/evergrove/docs/evidence/<SOURCE_SHA>/EG-012/verification.md` (**NOT COLLECTED**); **rollback/stop:** Sound distracts, annoys or is required to understand play.

- **Player problem:** Water restoration can lack sensory satisfaction.
- **Player-visible behavior:** Add quiet rotation/water/success cues and state-dependent ambience; muted by default until preference, with independent mute.
- **Acceptance criteria:** All game information remains visible when muted; respect ringer/accessibility settings; no autoplay surprises.
- **Success signal:** ≥70% of audio-on/off testers prefer sound-on without discomfort (exploratory).
- **Dependencies:** EG-001, audio assets and mobile testing.

### EG-013 — Optional haptic response

**State:** Proposed. **Type:** Polish. **Risk:** Battery/annoyance; defer if costly. **Evidence:** P1,H1. **R/I/C/E:** 3/2/3/1; **illustrative score:** 18.0.

**Execution:** `Proposed`; **M6**; owner role **Unity/Release Engineer**; verification **`UX-001`**; expected evidence `games/evergrove/docs/evidence/<SOURCE_SHA>/EG-013/verification.md` (**NOT COLLECTED**); **rollback/stop:** Haptics reduce accessibility or have no measurable touch benefit.

- **Player problem:** Touch rotation can feel imprecise.
- **Player-visible behavior:** Light tick per valid rotation and distinct completion pulse; easy toggle and OS respect.
- **Acceptance criteria:** No haptic as sole signal; disabled on unsupported devices; off switch works.
- **Success signal:** Fewer perceived missed taps or higher touch confidence in paired sessions.
- **Dependencies:** EG-001, EG-007, native haptic support.


## P1 — prove differentiation before expanding

### EG-101 — Causal journal

**State:** Ordered journal exists; causal chain view proposed. **Type:** Differentiation. **Risk:** Text overload; offer compact visual summary. **Evidence:** R1,H1. **R/I/C/E:** 5/4/3/2; **illustrative score:** 30.0.

**Execution:** `Implemented-Unverified`; **M4**; owner role **Gameplay Engineer**; verification **`WLD-003`, `VAL-002`**; expected evidence `games/evergrove/docs/evidence/<SOURCE_SHA>/EG-101/verification.md` (**NOT COLLECTED**); **rollback/stop:** Journal invents causes or players ignore it.

- **Player problem:** Returning players may forget why the world looks different.
- **Player-visible behavior:** For each choice show Choice → World effect → NPC memory → Next-puzzle effect; all derived from saved events.
- **Acceptance criteria:** No invented causes; entries remain correct after reload/reconsideration and language switch.
- **Success signal:** ≥80% returning testers reconstruct a past choice unaided.
- **Dependencies:** EG-003, WorldModel, localization.

### EG-102 — Three-depth authored consequences

**State:** Immediate/next/ending hooks partly exist; full contract proposed. **Type:** Differentiation. **Risk:** Combinatorial content burden. **Evidence:** R1,H1. **R/I/C/E:** 5/5/3/4; **illustrative score:** 18.8.

**Execution:** `Proposed`; **M4**; owner role **Product Owner**; verification **`WLD-001`, `WLD-004`, `VAL-002`**; expected evidence `games/evergrove/docs/evidence/<SOURCE_SHA>/EG-102/verification.md` (**NOT COLLECTED**); **rollback/stop:** Delayed effects fail causal recognition or contradict history.

- **Player problem:** One-step choices can feel like decorative rewards.
- **Player-visible behavior:** Every major choice has immediate visual, later puzzle/systemic and chapter-ending consequences, authored and deterministic.
- **Acceptance criteria:** Consequence matrix covers each choice and meaningful pair; automated invariants prevent contradictions.
- **Success signal:** Players correctly identify delayed effect and say it mattered.
- **Dependencies:** EG-003 and authored choice-consequence matrix. EG-105 is the later ecological-rule extension, not a prerequisite.

### EG-103 — Bounded NPC memory tokens

**State:** State-dependent authored conversations exist; formal memory tokens proposed. **Type:** Differentiation. **Risk:** Repetitive dialogue or token-state complexity. **Evidence:** R1,H1. **R/I/C/E:** 4/4/3/3; **illustrative score:** 16.0.

**Execution:** `Proposed`; **M4**; owner role **Gameplay Engineer**; verification **`WLD-002`, `VAL-002`**; expected evidence `games/evergrove/docs/evidence/<SOURCE_SHA>/EG-103/verification.md` (**NOT COLLECTED**); **rollback/stop:** NPC remembers unmade events or dialogue adds no player value.

- **Player problem:** Dialogue may sound generic or falsely imply memory.
- **Player-visible behavior:** Mira/Orin/Fen reference at most 3–5 salient real choice events each; no chat-history illusion.
- **Acceptance criteria:** Every utterance traceable to saved events; memory survives reload; no reference to unmade choice.
- **Success signal:** ≥70% can name one remembered event after Chapter One.
- **Dependencies:** EG-102, VillageStory, LocalizedVillage.

### EG-104 — Distinct character values and voices

**State:** Mira/Orin/Fen have authored distinct responses; attribution untested. **Type:** Differentiation. **Risk:** Stereotypes or forced conflict. **Evidence:** R1,H1. **R/I/C/E:** 4/3/3/3; **illustrative score:** 12.0.

**Execution:** `Proposed`; **M4**; owner role **Product Owner**; verification **`WLD-002`, `VAL-002`**; expected evidence `games/evergrove/docs/evidence/<SOURCE_SHA>/EG-104/verification.md` (**NOT COLLECTED**); **rollback/stop:** Characters are interchangeable or constrain choices.

- **Player problem:** Three NPCs can feel interchangeable.
- **Player-visible behavior:** Mira favors habitat, Orin practical access, Fen community/beauty; each responds honestly without a morality meter.
- **Acceptance criteria:** Blind attribution of shuffled NPC lines; no NPC asserts a single correct ecological choice.
- **Success signal:** ≥70% blind voice attribution after play (exploratory).
- **Dependencies:** EG-103, native script review.

### EG-105 — Ecological constraints change reasoning

**State:** Rotated topologies and fixed landmark implemented; richer rules proposed. **Type:** Differentiation. **Risk:** Complexity undermines accessible core. **Evidence:** R1,H1. **R/I/C/E:** 5/5/3/4; **illustrative score:** 18.8.

**Execution:** `Proposed`; **M4**; owner role **Gameplay Engineer**; verification **`PUZ-004`, `VAL-002`**; expected evidence `games/evergrove/docs/evidence/<SOURCE_SHA>/EG-105/verification.md` (**NOT COLLECTED**); **rollback/stop:** Extra ecology rules reduce comprehension or are merely board rotation.

- **Player problem:** Current branches are mathematically similar and may feel cosmetic.
- **Player-visible behavior:** Roots restrict rotations, bridge creates a meaningful crossing, meadow protects fragile routes; constraints interact in later puzzles.
- **Acceptance criteria:** For each rule, demonstrate changed valid moves or solution strategy beyond board rotation; solver proves uniqueness.
- **Success signal:** ≥70% can explain how a choice altered a later puzzle.
- **Dependencies:** EG-004, EG-102, rule engine changes.

### EG-106 — Recombine learned mechanics

**State:** Proposed; current 12 boards are fixed authored fixtures. **Type:** Core quality. **Risk:** Over-complex boards for casual cohort. **Evidence:** Q2,H1. **R/I/C/E:** 5/4/3/4; **illustrative score:** 15.0.

**Execution:** `Proposed`; **M4**; owner role **Gameplay Engineer**; verification **`PUZ-003`, `PUZ-004`**; expected evidence `games/evergrove/docs/evidence/<SOURCE_SHA>/EG-106/verification.md` (**NOT COLLECTED**); **rollback/stop:** Combined rules create unteachable difficulty or unfair solutions.

- **Player problem:** New puzzle rules can be introduced then abandoned.
- **Player-visible behavior:** Final third combines two or three previously taught ecology constraints with readable previews.
- **Acceptance criteria:** Every new mechanic is re-used at least twice; later puzzles require prior learning without surprise exceptions.
- **Success signal:** No unexplained difficulty spike; majority can articulate combined rule.
- **Dependencies:** EG-004, EG-105, playtest ladder.

### EG-107 — Transparent difficulty choice

**State:** Proposed. **Type:** Accessibility. **Risk:** Triple content/QA burden. **Evidence:** Q1,H1. **R/I/C/E:** 4/4/3/3; **illustrative score:** 16.0.

**Execution:** `Proposed`; **M5**; owner role **UX & Accessibility QA**; verification **`PUZ-003`, `A11Y-001`**; expected evidence `games/evergrove/docs/evidence/<SOURCE_SHA>/EG-107/verification.md` (**NOT COLLECTED**); **rollback/stop:** Mode switching corrupts state or increases confusion.

- **Player problem:** Fixed ladder may bore experts and overwhelm new players.
- **Player-visible behavior:** Offer Gentle / Standard / Thoughtful; optionally ask about a calmer board after repeated hints; never silently change difficulty.
- **Acceptance criteria:** All modes solvable, fair and narratively equivalent; switching does not erase or change village history.
- **Success signal:** Lower abandonment after stuck moments without lower reported agency.
- **Dependencies:** EG-004, EG-005, content variants.

### EG-108 — Optional return stream

**State:** Proposed. **Type:** Expansion. **Risk:** Repetition and pseudo-retention. **Evidence:** Q3,H1. **R/I/C/E:** 4/3/2/3; **illustrative score:** 8.0.

**Execution:** `Proposed`; **M6**; owner role **Product Owner**; verification **`VAL-004`**; expected evidence `games/evergrove/docs/evidence/<SOURCE_SHA>/EG-108/verification.md` (**NOT COLLECTED**); **rollback/stop:** Return feature requires streaks or no one voluntarily reopens.

- **Player problem:** A 12-chapter campaign may have no reason to reopen.
- **Player-visible behavior:** Offer an optional short authored/verified stream accessible anytime; no streak, calendar lock, expiration or penalty.
- **Acceptance criteria:** Offline and time-zone-independent; no unique reward gated to a particular day.
- **Success signal:** ≥25% of engaged pilot players voluntarily open a return stream (hypothesis).
- **Dependencies:** EG-110, content economics, return evidence.

### EG-109 — Meaningfully different new garden

**State:** Replay via reset exists; seeded ecology/replay variety proposed. **Type:** Expansion. **Risk:** Content explosion; not before first-run quality. **Evidence:** R1,H1. **R/I/C/E:** 4/4/2/4; **illustrative score:** 8.0.

**Execution:** `Proposed`; **M6**; owner role **Gameplay Engineer**; verification **`PUZ-003`, `WLD-004`**; expected evidence `games/evergrove/docs/evidence/<SOURCE_SHA>/EG-109/verification.md` (**NOT COLLECTED**); **rollback/stop:** Replay differs only cosmetically or reduces first-run quality.

- **Player problem:** Repeating same boards may not justify second run.
- **Player-visible behavior:** After ending, begin an alternate authored ecological route with different puzzle and consequence combinations.
- **Acceptance criteria:** Second run has ≥30% materially different puzzle/consequence states, verified by automated diff; no RNG unfairness.
- **Success signal:** Voluntary second-run start and meaningful completion.
- **Dependencies:** EG-105, EG-110, EG-116.

### EG-110 — Solver-assisted human authoring tool

**State:** Existing solver checks; generation/editor workflow proposed. **Type:** Tooling. **Risk:** Optimization for solver scores over human fun. **Evidence:** R1,H1. **R/I/C/E:** 5/4/3/4; **illustrative score:** 15.0.

**Execution:** `Proposed`; **M5**; owner role **Gameplay Engineer**; verification **`TOOL-001`, `PUZ-001`**; expected evidence `games/evergrove/docs/evidence/<SOURCE_SHA>/EG-110/verification.md` (**NOT COLLECTED**); **rollback/stop:** Generated puzzles bypass human review or unique-solution proof.

- **Player problem:** Manual high-quality puzzle production may be too slow.
- **Player-visible behavior:** Offline generator proposes candidates; exact solver checks uniqueness/rotation cost; human approves and themes every shipped board.
- **Acceptance criteria:** Reject unsolvable, multi-solution, trivial and duplicate candidates; output reproducible metadata and proof.
- **Success signal:** Human-approved puzzles per authoring hour with stable player-rated quality.
- **Dependencies:** EG-004, puzzle fixtures, test harness.

### EG-111 — Ten-step undo history

**State:** Ten-step tile-rotation undo implemented in the pure core (v4 additive `history`; legacy `previous` still read and written); pure C# and Unity batch compile verified, device QA not run. **Type:** Accessibility. **Risk:** Save compatibility and larger history state. **Evidence:** R1,Q1. **R/I/C/E:** 3/3/4/2; **illustrative score:** 18.0.

**Execution:** `Proposed`; **M5**; owner role **Gameplay Engineer**; verification **`SAVE-003`, `PUZ-002`**; expected evidence `games/evergrove/docs/evidence/<SOURCE_SHA>/EG-111/verification.md` (**NOT COLLECTED**); **rollback/stop:** Expanded undo corrupts saves or loses valid history.

- **Player problem:** An accidental tap can force a restart.
- **Player-visible behavior:** Undo up to ten tile rotations; keep choice reconsideration as a separate, explicit action.
- **Acceptance criteria:** Undo/redo boundary consistent after hint, solve, save/load and anchored tile; no irreversible mis-tap.
- **Success signal:** Lower restart rate caused by input errors.
- **Dependencies:** WorldModel snapshot/schema migration, EG-008.

### EG-112 — Large-board focus mode

**State:** First-pass code: Standard/Large board preference (`BoardSize`, PlayerPrefs `evergrove:board-size:v1`, settings-row control "▦ Standard"/"▦ Large"). Large widens the board card, fills the width and lifts the tile cap; markers and badges scale with the tile; no zoom or pan. Unity 6000.3.0f1 batch compile passes. **Unverified:** no visual check on the iOS Simulator or devices, no low-vision testers, and no confirmation that controls stay reachable without scrolling at 402×874 pt on 4×4. **Type:** Accessibility. **Risk:** Scrolling conflicts with tile taps. **Evidence:** Q1,P1. **R/I/C/E:** 4/4/4/3; **illustrative score:** 21.3.

**Execution:** `Proposed`; **M5**; owner role **UX & Accessibility QA**; verification **`A11Y-001`, `A11Y-005`**; expected evidence `games/evergrove/docs/evidence/<SOURCE_SHA>/EG-112/verification.md` (**NOT COLLECTED**); **rollback/stop:** Board zoom hides controls or adds mis-taps.

- **Player problem:** Whole 4×4 board may be hard to read with low vision.
- **Player-visible behavior:** Offer 1.25×/1.5× board, thicker channels, clear focus; pan only if necessary.
- **Acceptance criteria:** Every tile remains operable at max scale and smallest supported screen; no hidden controls.
- **Success signal:** Low-vision testers complete sample without facilitator help.
- **Dependencies:** EG-007, EG-011, layout architecture.

### EG-113 — Screen-reader puzzle semantics

**State:** First-pass code: `ScreenReaderBridge.cs` publishes a reading-order AccessibilityHierarchy for puzzle screens on iOS/Android while a screen reader is on; tile labels and activation reuse the tap path. Unity 6000.3.0f1 batch compile and editor smoke pass. **Unverified:** no VoiceOver/TalkBack device run, no first-three-boards completion; frame accuracy and focus behavior unproven. **Type:** Accessibility. **Risk:** Screen-reader interaction with dynamic puzzle may require substantial redesign. **Evidence:** R2,P1. **R/I/C/E:** 3/5/4/4; **illustrative score:** 15.0.

**Execution:** `Proposed`; **M5**; owner role **UX & Accessibility QA**; verification **`A11Y-003`**; expected evidence `games/evergrove/docs/evidence/<SOURCE_SHA>/EG-113/verification.md` (**NOT COLLECTED**); **rollback/stop:** Screen reader cannot complete first three boards.

- **Player problem:** A purely visual pipe board excludes nonvisual players.
- **Player-visible behavior:** Expose tile coordinates, ports, rotation state, connected/leak status, action labels and journal meaning to VoiceOver/TalkBack.
- **Acceptance criteria:** Complete first three streams with screen reader alone in device test; no inaccessible modal traps.
- **Success signal:** Task completion without sighted intervention; critical defect count 0.
- **Dependencies:** UI/text migration, semantic control adapter, device QA.

### EG-114 — Comprehensive reduced motion

**State:** Motion audit done (`docs/PLAYER_EXPERIENCE.md` section 6a). Store builds have no animated decorative motion; the only motion is the dev-only 2.5D miniature, gated by one reduced-motion preference defaulting to off. Still-frame and device verification (`A11Y-004`) not collected. **Type:** Accessibility. **Risk:** Separate animation QA paths. **Evidence:** R1,P1. **R/I/C/E:** 3/4/4/2; **illustrative score:** 24.0.

**Execution:** `Proposed`; **M5**; owner role **UX & Accessibility QA**; verification **`A11Y-004`**; expected evidence `games/evergrove/docs/evidence/<SOURCE_SHA>/EG-114/verification.md` (**NOT COLLECTED**); **rollback/stop:** Reduced motion hides causal effects or still causes discomfort.

- **Player problem:** Shimmer, camera movement and insects can distract or discomfort.
- **Player-visible behavior:** One reduced-motion preference disables nonessential camera drift, butterflies, water shimmer and transitions without hiding game state.
- **Acceptance criteria:** All outcomes readable in still frames; preference persists; default 2.5D motion remains off.
- **Success signal:** Zero motion-triggered blockers in recruited sensitive testers.
- **Dependencies:** 2D/2.5D renderer, EG-003.

### EG-115 — Local world postcard

**State:** Proposed. **Type:** Polish. **Risk:** Scope distracts from gameplay; optional. **Evidence:** H1. **R/I/C/E:** 4/3/2/2; **illustrative score:** 12.0.

**Execution:** `Proposed`; **M6**; owner role **Product Owner**; verification **`UX-002`, `WLD-003`**; expected evidence `games/evergrove/docs/evidence/<SOURCE_SHA>/EG-115/verification.md` (**NOT COLLECTED**); **rollback/stop:** Postcard misstates world or distracts from completion.

- **Player problem:** Session payoff may be forgotten or difficult to share.
- **Player-visible behavior:** Show optional still image plus one truthful sentence about today's world change, stored locally.
- **Acceptance criteria:** Image/text derived from actual saved state; no account/upload; readable at large text.
- **Success signal:** ≥30% voluntarily reopen postcard/history (exploratory).
- **Dependencies:** EG-003, EG-101, export/asset work.

### EG-116 — Consequence-driven ending comparison

**State:** Multiple authored endings exist; comparative epilogue proposed. **Type:** Differentiation. **Risk:** Spoiling alternatives or judging choices. **Evidence:** R1,H1. **R/I/C/E:** 3/3/3/2; **illustrative score:** 13.5.

**Execution:** `Proposed`; **M6**; owner role **Product Owner**; verification **`WLD-004`**; expected evidence `games/evergrove/docs/evidence/<SOURCE_SHA>/EG-116/verification.md` (**NOT COLLECTED**); **rollback/stop:** Epilogue describes choices the player did not make.

- **Player problem:** Ending may feel generic and replay reason unclear.
- **Player-visible behavior:** Show 'the village you made' with three major choice consequences, optionally compare alternate possible paths without moral score.
- **Acceptance criteria:** Ending matches event history; no false unseen-world claims; localized and accessible.
- **Success signal:** Players can explain why their ending differs; voluntary replay interest.
- **Dependencies:** EG-101, EG-102, endings, localization.


## P2 — optional after product evidence

### EG-201 — Small authored world micro-events

**State:** Proposed; some existing scene-state visitors/butterflies. **Type:** Expansion. **Risk:** Art/content maintenance. **Evidence:** R1,H1. **R/I/C/E:** 3/3/2/3; **illustrative score:** 6.0.

**Execution:** `Proposed`; **M6**; owner role **Product Owner**; verification **`WLD-001`**; expected evidence `games/evergrove/docs/evidence/<SOURCE_SHA>/EG-201/verification.md` (**NOT COLLECTED**); **rollback/stop:** Micro-events invent time or become chores.

- **Player problem:** Village may feel static between large choices.
- **Player-visible behavior:** Trigger visitor, repaired bench, bird or shade gathering from exact state combinations; no clock-based chores.
- **Acceptance criteria:** Each event has deterministic trigger and clear source; cannot contradict saved history.
- **Success signal:** Improved world-change recall without slower sessions.
- **Dependencies:** EG-102, scene state model.

### EG-202 — Cosmetic seasonal palette

**State:** Proposed; chapter landscape phases exist. **Type:** Polish. **Risk:** Palette QA overhead. **Evidence:** H1. **R/I/C/E:** 2/2/2/3; **illustrative score:** 2.7.

**Execution:** `Proposed`; **M6**; owner role **UX & Accessibility QA**; verification **`A11Y-002`**; expected evidence `games/evergrove/docs/evidence/<SOURCE_SHA>/EG-202/verification.md` (**NOT COLLECTED**); **rollback/stop:** Seasonal color harms contrast or gates content.

- **Player problem:** Long-run visual repetition.
- **Player-visible behavior:** Optional local palette change without real-world time gate or seasonal exclusives.
- **Acceptance criteria:** No puzzle or story content locked by calendar; accessible contrast in each palette.
- **Success signal:** Preference without reduced readability.
- **Dependencies:** EG-011, scene art.

### EG-203 — Explicit offline share card

**State:** Proposed. **Type:** Distribution. **Risk:** Low share motivation. **Evidence:** H1. **R/I/C/E:** 3/3/2/2; **illustrative score:** 9.0.

**Execution:** `Proposed`; **M6**; owner role **Store/Legal Owner**; verification **`UX-002`, `PRIV-002`**; expected evidence `games/evergrove/docs/evidence/<SOURCE_SHA>/EG-203/verification.md` (**NOT COLLECTED**); **rollback/stop:** Sharing is automatic, identifiable or unwanted.

- **Player problem:** Organic discovery is difficult for small indie games.
- **Player-visible behavior:** Generate local image of the player's real village and use OS share sheet only on explicit tap.
- **Acceptance criteria:** No tracking, automatic upload or personal data; clear attribution; opt-out.
- **Success signal:** Opt-in share rate and qualified visits when measurable without invasive SDKs.
- **Dependencies:** EG-115, store branding clearance.

### EG-204 — Accessible first-run presets

**State:** Proposed. **Type:** Accessibility. **Risk:** Conflicts with fast first-minute goal. **Evidence:** Q1,H1. **R/I/C/E:** 3/3/3/3; **illustrative score:** 9.0.

**Execution:** `Proposed`; **M6**; owner role **UX & Accessibility QA**; verification **`A11Y-001`, `A11Y-005`**; expected evidence `games/evergrove/docs/evidence/<SOURCE_SHA>/EG-204/verification.md` (**NOT COLLECTED**); **rollback/stop:** Presets delay first input or are irreversible.

- **Player problem:** Accessibility settings can be difficult to discover.
- **Player-visible behavior:** Optional Calm / Large Text / High Contrast quick preset, skippable and editable later.
- **Acceptance criteria:** Setup <30s, no mandatory choice; all settings independently reversible.
- **Success signal:** Higher setup success without slower first action.
- **Dependencies:** EG-007, EG-011, EG-114.

### EG-205 — One-time chapter expansion support

**State:** Proposed business architecture. **Type:** Commercial. **Risk:** Entitlement/offline complexity and support. **Evidence:** H1,P2. **R/I/C/E:** 3/3/2/3; **illustrative score:** 6.0.

**Execution:** `Proposed`; **M6**; owner role **Store/Legal Owner**; verification **`STORE-004`, `SAVE-003`**; expected evidence `games/evergrove/docs/evidence/<SOURCE_SHA>/EG-205/verification.md` (**NOT COLLECTED**); **rollback/stop:** Purchase dependency breaks offline base or requires paid hints.

- **Player problem:** Need sustainable revenue without pressure mechanics.
- **Player-visible behavior:** Offer optional substantial new authored chapter packs after the base experience; offline access once entitled.
- **Acceptance criteria:** No paid hints, currencies, time gates or progression fairness changes; clear restore-purchase UX.
- **Success signal:** Willingness-to-pay and expansion attach rate, only after validated base.
- **Dependencies:** Store billing, pricing tests, content pipeline.

### EG-206 — Localized store screenshots

**State:** Proposed. **Type:** Distribution. **Risk:** Translation and creative cost. **Evidence:** P2,H1. **R/I/C/E:** 4/3/3/2; **illustrative score:** 18.0.

**Execution:** `Proposed`; **M6**; owner role **Store/Legal Owner**; verification **`STORE-003`, `LOC-003`**; expected evidence `games/evergrove/docs/evidence/<SOURCE_SHA>/EG-206/verification.md` (**NOT COLLECTED**); **rollback/stop:** Localized screenshots are misleading or uncertified.

- **Player problem:** An English-only store page underuses certified locales.
- **Player-visible behavior:** Localize the first three causal-loop screenshots for each launch-certified market.
- **Acceptance criteria:** Native review, truthful screenshots and legible mobile text for every targeted listing.
- **Success signal:** Store-page conversion by locale, measured via platform tools.
- **Dependencies:** EG-010, EG-003, launch art.

### EG-207 — ASO positioning experiments

**State:** Proposed. **Type:** Distribution. **Risk:** Low traffic yields inconclusive tests. **Evidence:** P2,H1. **R/I/C/E:** 4/4/3/2; **illustrative score:** 24.0.

**Execution:** `Proposed`; **M6**; owner role **Store/Legal Owner**; verification **`STORE-003`, `VAL-005`**; expected evidence `games/evergrove/docs/evidence/<SOURCE_SHA>/EG-207/verification.md` (**NOT COLLECTED**); **rollback/stop:** Traffic is too small for a credible test or creative is misleading.

- **Player problem:** One listing may not communicate both puzzle and village appeal.
- **Player-visible behavior:** Test truthful 'water puzzle', 'cozy offline', and 'choices change village' creative/metadata variants via store-supported experiments.
- **Acceptance criteria:** At least two comparable creative hypotheses; no competitor trademark stuffing or unverifiable AI/brain-training claims.
- **Success signal:** Qualified listing conversion and retained installs, with sample/uncertainty shown.
- **Dependencies:** Market clearance, EG-206, platform access.

### EG-208 — Tablet/PC expansion after mobile proof

**State:** Proposed. **Type:** Expansion. **Risk:** Platform maintenance splits founder focus. **Evidence:** H1. **R/I/C/E:** 2/3/2/4; **illustrative score:** 3.0.

**Execution:** `Proposed`; **M6**; owner role **Unity/Release Engineer**; verification **`BLD-004`, `BLD-005`, `PERF-001`**; expected evidence `games/evergrove/docs/evidence/<SOURCE_SHA>/EG-208/verification.md` (**NOT COLLECTED**); **rollback/stop:** Porting consumes resources before mobile product proof.

- **Player problem:** Potential premium audience beyond phone.
- **Player-visible behavior:** Adapt layout/input only after a mobile product/price/retention threshold is met.
- **Acceptance criteria:** No port work before mobile go decision; keyboard/mouse/controller and larger-screen QA documented if approved.
- **Success signal:** Incremental demand per porting/support hour.
- **Dependencies:** Mobile product evidence, build pipeline.

## Explicit reject / avoid list

| Avoid | Why it is the wrong next move |
| --- | --- |
| Runtime LLM NPC dialogue, unlimited chat memory, infinite generated stories | Expensive, inconsistent, difficult to localize and moderate; no evidence it beats truthful authored memory. |
| Full 3D free-roam village, dozens of NPCs, elaborate crafting/inventory or relationship meters | Large art/content/UX burden that dilutes the short puzzle→place loop. |
| Multiplayer villages, guilds, trading, social economy, creator marketplace/UGC | Network effects, moderation, cloud costs and cold-start risks without demonstrated player need. |
| Mandatory cloud account, backend-dependent core gameplay, always-on analytics | Breaks offline/privacy positioning and increases support burden. |
| Forced ads, battle pass, loot boxes, subscriptions, hint currencies, lives/energy | Adds friction or coercion to the calm/respect promise. |
| Daily streak loss, mandatory notifications, real-time crop waiting, expiring rewards | Artificial retention; penalizes absence and invites repetitive chores. |
| Unsupervised procedural puzzle shipping | Mathematical validity and human fun are not interchangeable; require solver + human approval. |
| Unvalidated 30-language launch or unverified 'AI/brain training' claims | Damages trust; localization needs native/device certification and medical claims need stronger evidence. |

## Definition of done for any feature

1. User-visible behavior is demonstrated on the target device; not merely present in C# source.
2. Relevant deterministic tests and Unity import/build gates pass, with logs and date.
3. Accessibility, localization, save/restart and offline behavior are checked for the affected path.
4. Human playtest evidence includes denominator, target cohort, observation method and failure examples; no invented retention.
5. Feature is retained only if it improves the **puzzle → consequence → memory → return** promise enough to justify its complexity.

## Dependency-aware build order

**First:** EG-001, EG-002, EG-003, EG-007, EG-008, EG-009. **Then:** EG-004, EG-005, EG-006, EG-010, EG-011. **Only after causal preference evidence:** EG-101/102/103/105/106/110. **Optional polish:** EG-012/013/115. **Later distribution and expansion:** EG-201–208. See [validation plan](07-VALIDATION-AND-GTM.md).

## Audited source-level delivery increment — 2026-11-22

**Implemented in code, not Unity/device-verified:**

- **EG-002 (partial):** Compact first-screen composition with the playable board appearing before secondary language settings; no observed first-action timing.
- **EG-004 (partial):** [45 source-derived puzzle profiles](puzzle-catalog.generated.json) and reproducible `--check-puzzle-catalog` CI verification. `minimumClockwiseTaps` is a structural signal, not measured player difficulty. All observed solve-time/restart-rate fields remain intentionally blank/null.
- **EG-005 (partial):** World snapshot schema **v4** persists a 3-stage hint: notice row → focus tile → align one incorrect connection. Stages 1/2 are non-mutating; stage 3 changes one tile orientation (not necessarily one clockwise tap) and Undo can revert it. Existing `GuideOneConnection` remains an immediate alignment helper for compatibility.
- **EG-008 (partial):** The UI now displays a localized save-failure warning; a recovery/atomicity claim still needs real interruption tests.
- **EG-011 (partial):** Visible `!` text markers highlight tiles with leaks; contrast, screen-reader and mobile layout tests remain open.
- **EG-101 (partial):** Journal entries reflect saved choices and known immediate consequences; only the current board's proven branch rule is included. Full historical causality, screenshots and human recall remain unverified.

**Gate policy:** these implementation increments do **not** close M1–M4. The [execution register](verification-matrix.json) deliberately has **zero verified features** and **no fabricated evidence paths**. The open licensed Unity build, physical-device campaign and matched-controls study are more important than extra gameplay systems.
