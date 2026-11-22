# 02 — Full feature backlog (37 candidates)

**Source:** supplied 2026-11-22 Evergrove Deep Research synthesis, cross-checked against repository source/README; implementation status is **not** a build verification. [Evidence keys](08-EVIDENCE-AND-DECISIONS.md).

**Scoring is illustrative, not measured:** relative Reach (R), Impact (I), Confidence in impact (C) and Effort (E), each 1–5; **score = R × I × C ÷ E**. This is a RICE-like prioritization heuristic without real user counts, time estimates or statistically estimated confidence. Scores **do not** override release blockers, dependency order, ethics or player-test results. Risk is qualitative. A feature labeled P0 is not automatically required in the first 7-day test; prioritize builds and comprehension over polish.

**Statuses:** Existing = code present; Partial = some mechanism exists but acceptance is open; Proposed = not known implemented; Unverified = no independent real-device proof. **A spec is not a completion claim.**

| ID | Feature | Priority | Status | Type | R/I/C/E | Score |
| --- | --- | --- | --- | --- | --- | ---: |
| [EG-001](#eg-001--verified-phone-builds) | Verified phone builds | P0 | Scaffolded | Reliability | 5/5/5/2 | 62.5 |
| [EG-002](#eg-002--first-60-second-complete-loop) | First 60-second complete loop | P0 | Compact intro code; phone QA open | Core quality | 5/5/3/2 | 37.5 |
| [EG-003](#eg-003--consequence-first-visual-reveal) | Consequence-first visual reveal | P0 | Scene changes exist | Core quality | 5/5/3/2 | 37.5 |
| [EG-004](#eg-004--puzzle-solver-and-difficulty-metadata) | Puzzle solver and difficulty metadata | P0 | 45 source-derived profiles; human QA open | Core quality | 5/4/4/3 | 26.7 |
| [EG-005](#eg-005--progressive-non-punitive-hints) | Progressive non-punitive hints | P0 | 3-stage v4 code; phone QA open | Core quality | 5/4/4/2 | 40.0 |
| [EG-006](#eg-006--correct-complex-script-text-rendering) | Correct complex-script text rendering | P0 | Legacy uGUI Text and best-effort OS fonts | Release quality | 4/5/5/4 | 25.0 |
| [EG-007](#eg-007--large-forgiving-touch-and-readable-text) | Large, forgiving touch and readable text | P0 | Safe-area scroll and large controls exist | Release quality | 5/5/5/2 | 62.5 |
| [EG-008](#eg-008--crash-safe-offline-saves) | Crash-safe offline saves | P0 | v4 + warning; device fault tests open | Reliability | 5/5/5/2 | 62.5 |
| [EG-009](#eg-009--offlineprivacy-binary-verification) | Offline/privacy binary verification | P0 | Offline design and opt-in in-memory playtest exist | Trust | 5/5/5/2 | 62.5 |
| [EG-010](#eg-010--native-certified-launch-locale-subset) | Native-certified launch locale subset | P0 | 30 complete draft catalogs | Release quality | 4/4/5/3 | 26.7 |
| [EG-011](#eg-011--non-color-connectivity-and-contrast) | Non-color connectivity and contrast | P0 | Color-coded scene/pipe UI exists | Release quality | 5/4/5/2 | 50.0 |
| [EG-012](#eg-012--subtle-audio-feedback-and-ambience) | Subtle audio feedback and ambience | P0 | Optional gentle completion chime exists | Polish | 4/3/3/3 | 12.0 |
| [EG-013](#eg-013--optional-haptic-response) | Optional haptic response | P0 | Proposed | Polish | 3/2/3/1 | 18.0 |
| [EG-101](#eg-101--causal-journal) | Causal journal | P1 | Ordered journal exists | Differentiation | 5/4/3/2 | 30.0 |
| [EG-102](#eg-102--three-depth-authored-consequences) | Three-depth authored consequences | P1 | Immediate/next/ending hooks partly exist | Differentiation | 5/5/3/4 | 18.8 |
| [EG-103](#eg-103--bounded-npc-memory-tokens) | Bounded NPC memory tokens | P1 | State-dependent authored conversations exist | Differentiation | 4/4/3/3 | 16.0 |
| [EG-104](#eg-104--distinct-character-values-and-voices) | Distinct character values and voices | P1 | Mira/Orin/Fen have authored distinct responses | Differentiation | 4/3/3/3 | 12.0 |
| [EG-105](#eg-105--ecological-constraints-change-reasoning) | Ecological constraints change reasoning | P1 | Rotated topologies and fixed landmark implemented | Differentiation | 5/5/3/4 | 18.8 |
| [EG-106](#eg-106--recombine-learned-mechanics) | Recombine learned mechanics | P1 | Proposed | Core quality | 5/4/3/4 | 15.0 |
| [EG-107](#eg-107--transparent-difficulty-choice) | Transparent difficulty choice | P1 | Proposed | Accessibility | 4/4/3/3 | 16.0 |
| [EG-108](#eg-108--optional-return-stream) | Optional return stream | P1 | Proposed | Expansion | 4/3/2/3 | 8.0 |
| [EG-109](#eg-109--meaningfully-different-new-garden) | Meaningfully different new garden | P1 | Replay via reset exists | Expansion | 4/4/2/4 | 8.0 |
| [EG-110](#eg-110--solver-assisted-human-authoring-tool) | Solver-assisted human authoring tool | P1 | Existing solver checks | Tooling | 5/4/3/4 | 15.0 |
| [EG-111](#eg-111--ten-step-undo-history) | Ten-step undo history | P1 | One-step undo implemented | Accessibility | 3/3/4/2 | 18.0 |
| [EG-112](#eg-112--large-board-focus-mode) | Large-board focus mode | P1 | Proposed | Accessibility | 4/4/4/3 | 21.3 |
| [EG-113](#eg-113--screen-reader-puzzle-semantics) | Screen-reader puzzle semantics | P1 | Unverified | Accessibility | 3/5/4/4 | 15.0 |
| [EG-114](#eg-114--comprehensive-reduced-motion) | Comprehensive reduced motion | P1 | 2.5D motion toggle exists | Accessibility | 3/4/4/2 | 24.0 |
| [EG-115](#eg-115--local-world-postcard) | Local world postcard | P1 | Proposed | Polish | 4/3/2/2 | 12.0 |
| [EG-116](#eg-116--consequence-driven-ending-comparison) | Consequence-driven ending comparison | P1 | Multiple authored endings exist | Differentiation | 3/3/3/2 | 13.5 |
| [EG-201](#eg-201--small-authored-world-micro-events) | Small authored world micro-events | P2 | Proposed | Expansion | 3/3/2/3 | 6.0 |
| [EG-202](#eg-202--cosmetic-seasonal-palette) | Cosmetic seasonal palette | P2 | Proposed | Polish | 2/2/2/3 | 2.7 |
| [EG-203](#eg-203--explicit-offline-share-card) | Explicit offline share card | P2 | Proposed | Distribution | 3/3/2/2 | 9.0 |
| [EG-204](#eg-204--accessible-first-run-presets) | Accessible first-run presets | P2 | Proposed | Accessibility | 3/3/3/3 | 9.0 |
| [EG-205](#eg-205--one-time-chapter-expansion-support) | One-time chapter expansion support | P2 | Proposed business architecture | Commercial | 3/3/2/3 | 6.0 |
| [EG-206](#eg-206--localized-store-screenshots) | Localized store screenshots | P2 | Proposed | Distribution | 4/3/3/2 | 18.0 |
| [EG-207](#eg-207--aso-positioning-experiments) | ASO positioning experiments | P2 | Proposed | Distribution | 4/4/3/2 | 24.0 |
| [EG-208](#eg-208--tabletpc-expansion-after-mobile-proof) | Tablet/PC expansion after mobile proof | P2 | Proposed | Expansion | 2/3/2/4 | 3.0 |

## Status update — actual code is not player evidence

This pass adds source-derived puzzle metadata, staged help, a shortened initial layout and visible save warnings. **Each remains partial against its original P0 acceptance criteria** until real Unity Editor compilation, phone QA and players prove the intended improvement. All 37 feature IDs and historical illustrative prioritization scores remain traceable.

## P0 — release and core-quality gates

### EG-001 — Verified phone builds

**State:** Scaffolded; no Unity Editor/device pass recorded. **Type:** Reliability. **Risk:** Build/signing/toolchain delays. **Evidence:** R1,R2. **R/I/C/E:** 5/5/5/2; **illustrative score:** 62.5.

- **Player problem:** A plausible prototype may not compile or handle real touch.
- **Player-visible behavior:** Launch and finish the campaign on a compact Android and an iPhone; respect safe areas, back navigation and resume.
- **Acceptance criteria:** Document Unity import/build logs; finish all 12 streams on both devices; zero blocker defects; recover from force-close.
- **Success signal:** Full-campaign pass on 2 device families; blocker count 0.
- **Dependencies:** Unity license, platform modules, Xcode signing, physical devices.

### EG-002 — First 60-second complete loop

**State:** Compact first-puzzle art and language controls rearranged in code; no measured first-minute results or device pass. **Type:** Core quality. **Risk:** Over-directed opening may feel trivial. **Evidence:** R1,R3,Q1. **R/I/C/E:** 5/5/3/2; **illustrative score:** 37.5.

- **Player problem:** Players may see menus or a generic pipe board before Evergrove's value.
- **Player-visible behavior:** Start at dry creek with obvious first tile; first rotations extend water; first solve visibly revives a village detail.
- **Acceptance criteria:** No mandatory modal/lore/mode screen; first input obvious; dry→flowing payoff tested on both phones.
- **Success signal:** ≥80% unaided goal comprehension; median first meaningful input <15s (provisional).
- **Dependencies:** EG-001, first-puzzle tuning, presentation/UI.

### EG-003 — Consequence-first visual reveal

**State:** Scene changes exist; reveal sequence needs device/player verification. **Type:** Core quality. **Risk:** Visual clutter or longer forced cutscene. **Evidence:** R1,R4,H1. **R/I/C/E:** 5/5/3/2; **illustrative score:** 37.5.

- **Player problem:** Text-only explanation makes choices feel cosmetic.
- **Player-visible behavior:** After tree/bridge/meadow selection, frame and highlight the changed landscape before one brief NPC response.
- **Acceptance criteria:** Before/after state remains readable in 2D, 2.5D and reduced motion; no false causal claims.
- **Success signal:** ≥75% recall a specific consequence after 10 minutes (provisional).
- **Dependencies:** EG-001, deterministic scene plan, EG-002.

### EG-004 — Puzzle solver and difficulty metadata

**State:** 45 source-derived board profiles checked against pure C#; observed human difficulty, forced moves and editorial acceptance remain unknown. **Type:** Core quality. **Risk:** Solver complexity mistaken for felt difficulty. **Evidence:** R1,Q2,H1. **R/I/C/E:** 5/4/4/3; **illustrative score:** 26.7.

- **Player problem:** Unique solution does not guarantee fair or engaging difficulty.
- **Player-visible behavior:** [45 checked-in profiles](puzzle-catalog.generated.json) now include layout, solution and at-most-two solution count, minimum clockwise taps, initial leaks/reach, and anchor. Advanced forced-move/hint-path metrics, difficulty labels and observed human times remain unimplemented or unknown.
- **Acceptance criteria:** All 12 originals and 33 variants have reproducible validity checks; no unsolvable/multi-solution shipping board; human times recorded separately.
- **Success signal:** No invalid boards; flag >2× adjacent solve-time spikes for review, not automatic rejection.
- **Dependencies:** PipeRules, Tests/Program.cs, representative testers.

### EG-005 — Progressive non-punitive hints

**State:** Three on-request clue stages (row, tile, one aligned connection) implemented and persisted in v4; target-device accessibility and recovery remain unverified. **Type:** Core quality. **Risk:** Hint reveals too much or implies a unique move when multiple are possible. **Evidence:** R1,Q1,H1. **R/I/C/E:** 5/4/4/2; **illustrative score:** 40.0.

- **Player problem:** One hint may spoil a puzzle or fail to unstick a player.
- **Player-visible behavior:** Stage 1 highlights a row; stage 2 marks the target tile; stage 3 aligns exactly one pipe to its reference orientation; no penalty/currency.
- **Acceptance criteria:** Each stage makes valid progress on every board/variant; anchored tiles remain fixed; hint state survives undo/restart appropriately.
- **Success signal:** ≥80% of stuck testers resume unaided; <20% report unwanted full spoiler.
- **Dependencies:** EG-004, undo and save contracts.

### EG-006 — Correct complex-script text rendering

**State:** Legacy uGUI Text and best-effort OS fonts; shipping blocker. **Type:** Release quality. **Risk:** Text migration changes layout, focus or performance. **Evidence:** R3,U1. **R/I/C/E:** 4/5/5/4; **illustrative score:** 25.0.

- **Player problem:** RTL and Indic strings can be unreadable despite complete catalogs.
- **Player-visible behavior:** Use Unity Advanced Text Generator/UI Toolkit or a tested equivalent for all player-facing dynamic text, bidi and shaping.
- **Acceptance criteria:** Arabic/Persian/Hebrew/Urdu bidi; Hindi/Bengali/Tamil/Telugu conjuncts; Thai breaks; CJK glyphs; compact/large-text screenshots pass.
- **Success signal:** Zero critical shaping/overflow defects in each enabled locale.
- **Dependencies:** UI architecture decision, licensed font assets, native reviewers.

### EG-007 — Large, forgiving touch and readable text

**State:** Safe-area scroll and large controls exist; device audit unverified. **Type:** Release quality. **Risk:** Larger targets crowd 4×4 board. **Evidence:** R2,P1,Q1. **R/I/C/E:** 5/5/5/2; **illustrative score:** 62.5.

- **Player problem:** Small taps, complex gestures and low text size exclude casual/older users.
- **Player-visible behavior:** One-tap rotation; minimum frequent-action hit areas 44pt iOS/48dp Android; visible pressed state; scalable text.
- **Acceptance criteria:** Every frequent action passes physical target-size audit; no precision swipe; 320–430 logical-width layouts and OS scaling tested.
- **Success signal:** Median ≤2 mis-taps per observed 10-minute session; all key controls operable.
- **Dependencies:** EG-001, layout refactor, target devices.

### EG-008 — Crash-safe offline saves

**State:** v4 JSON, backup and v1/v2/v3 migration; visible translated warning on failed save. Device fault tests still open. **Type:** Reliability. **Risk:** Platform-specific File.Replace/fallback semantics. **Evidence:** R1,R2. **R/I/C/E:** 5/5/5/2; **illustrative score:** 62.5.

- **Player problem:** Lost garden history destroys trust and emotional ownership.
- **Player-visible behavior:** Write validated snapshots, recover previous valid save, migrate v1/v2, confirm destructive reset.
- **Acceptance criteria:** 100 scripted/app-force-kill points across rotation/choice/save where feasible; no unrecoverable valid history; corrupted primary restores backup.
- **Success signal:** Zero unrecoverable valid saves in documented fault-injection suite.
- **Dependencies:** WorldModel, WorldStore, Android/iOS storage testing.

### EG-009 — Offline/privacy binary verification

**State:** Offline design and opt-in in-memory playtest exist; network audit open. **Type:** Trust. **Risk:** Platform service traffic confused with app traffic. **Evidence:** R1,R2,P2. **R/I/C/E:** 5/5/5/2; **illustrative score:** 62.5.

- **Player problem:** Offline promise can be broken by SDKs or unexpected traffic.
- **Player-visible behavior:** Full campaign works in airplane mode; no ad/analytics SDK; session notes exist only after consent and export only on request.
- **Acceptance criteria:** Inspect dependencies and production network traffic; no unexplained outbound requests; correct platform privacy disclosures.
- **Success signal:** Zero unexpected transmissions; no identifiers in local report.
- **Dependencies:** Release build, dependency review, privacy policy.

### EG-010 — Native-certified launch locale subset

**State:** 30 complete draft catalogs; no native/device sign-off. **Type:** Release quality. **Risk:** Fewer initial markets; QA cost. **Evidence:** R3,U1,P2. **R/I/C/E:** 4/4/5/3; **illustrative score:** 26.7.

- **Player problem:** Thirty selectable languages can create thirty broken first impressions.
- **Player-visible behavior:** Enable only locales passing translation, shaping, native editorial and screenshot QA; retain draft files in repo.
- **Acceptance criteria:** Every shipping locale has signed reviewer checklist, text/glyph/layout matrix and localized store copy; unverified locales disabled.
- **Success signal:** 100% enabled locales certified; 0 known critical translation defects.
- **Dependencies:** EG-006, EG-007, LOCALIZATION.md.

### EG-011 — Non-color connectivity and contrast

**State:** Color-coded scene/pipe UI exists; formal audit proposed. **Type:** Release quality. **Risk:** Extra strokes obscure small boards. **Evidence:** P1,Q1. **R/I/C/E:** 5/4/5/2; **illustrative score:** 50.0.

- **Player problem:** Players may not distinguish connected, leaking and anchored tiles by hue.
- **Player-visible behavior:** Use pipe shape, port outline, water movement/pattern and labels in addition to color; provide high-contrast mode.
- **Acceptance criteria:** Solve full sample in grayscale/color-filter conditions; all states recognizable without audio or color.
- **Success signal:** No critical state-confusion failures in accessibility test.
- **Dependencies:** PipeGraphic, EG-007, user testing.

### EG-012 — Subtle audio feedback and ambience

**State:** Optional gentle completion chime exists; richer sound proposed. **Type:** Polish. **Risk:** Cost, repetition, sensory overload; defer behind blockers. **Evidence:** R1,H1. **R/I/C/E:** 4/3/3/3; **illustrative score:** 12.0.

- **Player problem:** Water restoration can lack sensory satisfaction.
- **Player-visible behavior:** Add quiet rotation/water/success cues and state-dependent ambience; muted by default until preference, with independent mute.
- **Acceptance criteria:** All game information remains visible when muted; respect ringer/accessibility settings; no autoplay surprises.
- **Success signal:** ≥70% of audio-on/off testers prefer sound-on without discomfort (exploratory).
- **Dependencies:** EG-001, audio assets and mobile testing.

### EG-013 — Optional haptic response

**State:** Proposed. **Type:** Polish. **Risk:** Battery/annoyance; defer if costly. **Evidence:** P1,H1. **R/I/C/E:** 3/2/3/1; **illustrative score:** 18.0.

- **Player problem:** Touch rotation can feel imprecise.
- **Player-visible behavior:** Light tick per valid rotation and distinct completion pulse; easy toggle and OS respect.
- **Acceptance criteria:** No haptic as sole signal; disabled on unsupported devices; off switch works.
- **Success signal:** Fewer perceived missed taps or higher touch confidence in paired sessions.
- **Dependencies:** EG-001, EG-007, native haptic support.


## P1 — prove differentiation before expanding

### EG-101 — Causal journal

**State:** Ordered journal exists; causal chain view proposed. **Type:** Differentiation. **Risk:** Text overload; offer compact visual summary. **Evidence:** R1,H1. **R/I/C/E:** 5/4/3/2; **illustrative score:** 30.0.

- **Player problem:** Returning players may forget why the world looks different.
- **Player-visible behavior:** For each choice show Choice → World effect → NPC memory → Next-puzzle effect; all derived from saved events.
- **Acceptance criteria:** No invented causes; entries remain correct after reload/reconsideration and language switch.
- **Success signal:** ≥80% returning testers reconstruct a past choice unaided.
- **Dependencies:** EG-003, WorldModel, localization.

### EG-102 — Three-depth authored consequences

**State:** Immediate/next/ending hooks partly exist; full contract proposed. **Type:** Differentiation. **Risk:** Combinatorial content burden. **Evidence:** R1,H1. **R/I/C/E:** 5/5/3/4; **illustrative score:** 18.8.

- **Player problem:** One-step choices can feel like decorative rewards.
- **Player-visible behavior:** Every major choice has immediate visual, later puzzle/systemic and chapter-ending consequences, authored and deterministic.
- **Acceptance criteria:** Consequence matrix covers each choice and meaningful pair; automated invariants prevent contradictions.
- **Success signal:** Players correctly identify delayed effect and say it mattered.
- **Dependencies:** EG-003, EG-105, narrative matrix.

### EG-103 — Bounded NPC memory tokens

**State:** State-dependent authored conversations exist; formal memory tokens proposed. **Type:** Differentiation. **Risk:** Repetitive dialogue or token-state complexity. **Evidence:** R1,H1. **R/I/C/E:** 4/4/3/3; **illustrative score:** 16.0.

- **Player problem:** Dialogue may sound generic or falsely imply memory.
- **Player-visible behavior:** Mira/Orin/Fen reference at most 3–5 salient real choice events each; no chat-history illusion.
- **Acceptance criteria:** Every utterance traceable to saved events; memory survives reload; no reference to unmade choice.
- **Success signal:** ≥70% can name one remembered event after Chapter One.
- **Dependencies:** EG-102, VillageStory, LocalizedVillage.

### EG-104 — Distinct character values and voices

**State:** Mira/Orin/Fen have authored distinct responses; attribution untested. **Type:** Differentiation. **Risk:** Stereotypes or forced conflict. **Evidence:** R1,H1. **R/I/C/E:** 4/3/3/3; **illustrative score:** 12.0.

- **Player problem:** Three NPCs can feel interchangeable.
- **Player-visible behavior:** Mira favors habitat, Orin practical access, Fen community/beauty; each responds honestly without a morality meter.
- **Acceptance criteria:** Blind attribution of shuffled NPC lines; no NPC asserts a single correct ecological choice.
- **Success signal:** ≥70% blind voice attribution after play (exploratory).
- **Dependencies:** EG-103, native script review.

### EG-105 — Ecological constraints change reasoning

**State:** Rotated topologies and fixed landmark implemented; richer rules proposed. **Type:** Differentiation. **Risk:** Complexity undermines accessible core. **Evidence:** R1,H1. **R/I/C/E:** 5/5/3/4; **illustrative score:** 18.8.

- **Player problem:** Current branches are mathematically similar and may feel cosmetic.
- **Player-visible behavior:** Roots restrict rotations, bridge creates a meaningful crossing, meadow protects fragile routes; constraints interact in later puzzles.
- **Acceptance criteria:** For each rule, demonstrate changed valid moves or solution strategy beyond board rotation; solver proves uniqueness.
- **Success signal:** ≥70% can explain how a choice altered a later puzzle.
- **Dependencies:** EG-004, EG-102, rule engine changes.

### EG-106 — Recombine learned mechanics

**State:** Proposed; current 12 boards are fixed authored fixtures. **Type:** Core quality. **Risk:** Over-complex boards for casual cohort. **Evidence:** Q2,H1. **R/I/C/E:** 5/4/3/4; **illustrative score:** 15.0.

- **Player problem:** New puzzle rules can be introduced then abandoned.
- **Player-visible behavior:** Final third combines two or three previously taught ecology constraints with readable previews.
- **Acceptance criteria:** Every new mechanic is re-used at least twice; later puzzles require prior learning without surprise exceptions.
- **Success signal:** No unexplained difficulty spike; majority can articulate combined rule.
- **Dependencies:** EG-004, EG-105, playtest ladder.

### EG-107 — Transparent difficulty choice

**State:** Proposed. **Type:** Accessibility. **Risk:** Triple content/QA burden. **Evidence:** Q1,H1. **R/I/C/E:** 4/4/3/3; **illustrative score:** 16.0.

- **Player problem:** Fixed ladder may bore experts and overwhelm new players.
- **Player-visible behavior:** Offer Gentle / Standard / Thoughtful; optionally ask about a calmer board after repeated hints; never silently change difficulty.
- **Acceptance criteria:** All modes solvable, fair and narratively equivalent; switching does not erase or change village history.
- **Success signal:** Lower abandonment after stuck moments without lower reported agency.
- **Dependencies:** EG-004, EG-005, content variants.

### EG-108 — Optional return stream

**State:** Proposed. **Type:** Expansion. **Risk:** Repetition and pseudo-retention. **Evidence:** Q3,H1. **R/I/C/E:** 4/3/2/3; **illustrative score:** 8.0.

- **Player problem:** A 12-chapter campaign may have no reason to reopen.
- **Player-visible behavior:** Offer an optional short authored/verified stream accessible anytime; no streak, calendar lock, expiration or penalty.
- **Acceptance criteria:** Offline and time-zone-independent; no unique reward gated to a particular day.
- **Success signal:** ≥25% of engaged pilot players voluntarily open a return stream (hypothesis).
- **Dependencies:** EG-110, content economics, return evidence.

### EG-109 — Meaningfully different new garden

**State:** Replay via reset exists; seeded ecology/replay variety proposed. **Type:** Expansion. **Risk:** Content explosion; not before first-run quality. **Evidence:** R1,H1. **R/I/C/E:** 4/4/2/4; **illustrative score:** 8.0.

- **Player problem:** Repeating same boards may not justify second run.
- **Player-visible behavior:** After ending, begin an alternate authored ecological route with different puzzle and consequence combinations.
- **Acceptance criteria:** Second run has ≥30% materially different puzzle/consequence states, verified by automated diff; no RNG unfairness.
- **Success signal:** Voluntary second-run start and meaningful completion.
- **Dependencies:** EG-105, EG-110, EG-116.

### EG-110 — Solver-assisted human authoring tool

**State:** Existing solver checks; generation/editor workflow proposed. **Type:** Tooling. **Risk:** Optimization for solver scores over human fun. **Evidence:** R1,H1. **R/I/C/E:** 5/4/3/4; **illustrative score:** 15.0.

- **Player problem:** Manual high-quality puzzle production may be too slow.
- **Player-visible behavior:** Offline generator proposes candidates; exact solver checks uniqueness/rotation cost; human approves and themes every shipped board.
- **Acceptance criteria:** Reject unsolvable, multi-solution, trivial and duplicate candidates; output reproducible metadata and proof.
- **Success signal:** Human-approved puzzles per authoring hour with stable player-rated quality.
- **Dependencies:** EG-004, puzzle fixtures, test harness.

### EG-111 — Ten-step undo history

**State:** One-step undo implemented. **Type:** Accessibility. **Risk:** Save compatibility and larger history state. **Evidence:** R1,Q1. **R/I/C/E:** 3/3/4/2; **illustrative score:** 18.0.

- **Player problem:** An accidental tap can force a restart.
- **Player-visible behavior:** Undo up to ten tile rotations; keep choice reconsideration as a separate, explicit action.
- **Acceptance criteria:** Undo/redo boundary consistent after hint, solve, save/load and anchored tile; no irreversible mis-tap.
- **Success signal:** Lower restart rate caused by input errors.
- **Dependencies:** WorldModel snapshot/schema migration, EG-008.

### EG-112 — Large-board focus mode

**State:** Proposed. **Type:** Accessibility. **Risk:** Scrolling conflicts with tile taps. **Evidence:** Q1,P1. **R/I/C/E:** 4/4/4/3; **illustrative score:** 21.3.

- **Player problem:** Whole 4×4 board may be hard to read with low vision.
- **Player-visible behavior:** Offer 1.25×/1.5× board, thicker channels, clear focus; pan only if necessary.
- **Acceptance criteria:** Every tile remains operable at max scale and smallest supported screen; no hidden controls.
- **Success signal:** Low-vision testers complete sample without facilitator help.
- **Dependencies:** EG-007, EG-011, layout architecture.

### EG-113 — Screen-reader puzzle semantics

**State:** Unverified; current generated uGUI has no certified screen-reader path. **Type:** Accessibility. **Risk:** Screen-reader interaction with dynamic puzzle may require substantial redesign. **Evidence:** R2,P1. **R/I/C/E:** 3/5/4/4; **illustrative score:** 15.0.

- **Player problem:** A purely visual pipe board excludes nonvisual players.
- **Player-visible behavior:** Expose tile coordinates, ports, rotation state, connected/leak status, action labels and journal meaning to VoiceOver/TalkBack.
- **Acceptance criteria:** Complete first three streams with screen reader alone in device test; no inaccessible modal traps.
- **Success signal:** Task completion without sighted intervention; critical defect count 0.
- **Dependencies:** UI/text migration, semantic control adapter, device QA.

### EG-114 — Comprehensive reduced motion

**State:** 2.5D motion toggle exists; broader mode proposed. **Type:** Accessibility. **Risk:** Separate animation QA paths. **Evidence:** R1,P1. **R/I/C/E:** 3/4/4/2; **illustrative score:** 24.0.

- **Player problem:** Shimmer, camera movement and insects can distract or discomfort.
- **Player-visible behavior:** One reduced-motion preference disables nonessential camera drift, butterflies, water shimmer and transitions without hiding game state.
- **Acceptance criteria:** All outcomes readable in still frames; preference persists; default 2.5D motion remains off.
- **Success signal:** Zero motion-triggered blockers in recruited sensitive testers.
- **Dependencies:** 2D/2.5D renderer, EG-003.

### EG-115 — Local world postcard

**State:** Proposed. **Type:** Polish. **Risk:** Scope distracts from gameplay; optional. **Evidence:** H1. **R/I/C/E:** 4/3/2/2; **illustrative score:** 12.0.

- **Player problem:** Session payoff may be forgotten or difficult to share.
- **Player-visible behavior:** Show optional still image plus one truthful sentence about today's world change, stored locally.
- **Acceptance criteria:** Image/text derived from actual saved state; no account/upload; readable at large text.
- **Success signal:** ≥30% voluntarily reopen postcard/history (exploratory).
- **Dependencies:** EG-003, EG-101, export/asset work.

### EG-116 — Consequence-driven ending comparison

**State:** Multiple authored endings exist; comparative epilogue proposed. **Type:** Differentiation. **Risk:** Spoiling alternatives or judging choices. **Evidence:** R1,H1. **R/I/C/E:** 3/3/3/2; **illustrative score:** 13.5.

- **Player problem:** Ending may feel generic and replay reason unclear.
- **Player-visible behavior:** Show 'the village you made' with three major choice consequences, optionally compare alternate possible paths without moral score.
- **Acceptance criteria:** Ending matches event history; no false unseen-world claims; localized and accessible.
- **Success signal:** Players can explain why their ending differs; voluntary replay interest.
- **Dependencies:** EG-101, EG-102, endings, localization.


## P2 — optional after product evidence

### EG-201 — Small authored world micro-events

**State:** Proposed; some existing scene-state visitors/butterflies. **Type:** Expansion. **Risk:** Art/content maintenance. **Evidence:** R1,H1. **R/I/C/E:** 3/3/2/3; **illustrative score:** 6.0.

- **Player problem:** Village may feel static between large choices.
- **Player-visible behavior:** Trigger visitor, repaired bench, bird or shade gathering from exact state combinations; no clock-based chores.
- **Acceptance criteria:** Each event has deterministic trigger and clear source; cannot contradict saved history.
- **Success signal:** Improved world-change recall without slower sessions.
- **Dependencies:** EG-102, scene state model.

### EG-202 — Cosmetic seasonal palette

**State:** Proposed; chapter landscape phases exist. **Type:** Polish. **Risk:** Palette QA overhead. **Evidence:** H1. **R/I/C/E:** 2/2/2/3; **illustrative score:** 2.7.

- **Player problem:** Long-run visual repetition.
- **Player-visible behavior:** Optional local palette change without real-world time gate or seasonal exclusives.
- **Acceptance criteria:** No puzzle or story content locked by calendar; accessible contrast in each palette.
- **Success signal:** Preference without reduced readability.
- **Dependencies:** EG-011, scene art.

### EG-203 — Explicit offline share card

**State:** Proposed. **Type:** Distribution. **Risk:** Low share motivation. **Evidence:** H1. **R/I/C/E:** 3/3/2/2; **illustrative score:** 9.0.

- **Player problem:** Organic discovery is difficult for small indie games.
- **Player-visible behavior:** Generate local image of the player's real village and use OS share sheet only on explicit tap.
- **Acceptance criteria:** No tracking, automatic upload or personal data; clear attribution; opt-out.
- **Success signal:** Opt-in share rate and qualified visits when measurable without invasive SDKs.
- **Dependencies:** EG-115, store branding clearance.

### EG-204 — Accessible first-run presets

**State:** Proposed. **Type:** Accessibility. **Risk:** Conflicts with fast first-minute goal. **Evidence:** Q1,H1. **R/I/C/E:** 3/3/3/3; **illustrative score:** 9.0.

- **Player problem:** Accessibility settings can be difficult to discover.
- **Player-visible behavior:** Optional Calm / Large Text / High Contrast quick preset, skippable and editable later.
- **Acceptance criteria:** Setup <30s, no mandatory choice; all settings independently reversible.
- **Success signal:** Higher setup success without slower first action.
- **Dependencies:** EG-007, EG-011, EG-114.

### EG-205 — One-time chapter expansion support

**State:** Proposed business architecture. **Type:** Commercial. **Risk:** Entitlement/offline complexity and support. **Evidence:** H1,P2. **R/I/C/E:** 3/3/2/3; **illustrative score:** 6.0.

- **Player problem:** Need sustainable revenue without pressure mechanics.
- **Player-visible behavior:** Offer optional substantial new authored chapter packs after the base experience; offline access once entitled.
- **Acceptance criteria:** No paid hints, currencies, time gates or progression fairness changes; clear restore-purchase UX.
- **Success signal:** Willingness-to-pay and expansion attach rate, only after validated base.
- **Dependencies:** Store billing, pricing tests, content pipeline.

### EG-206 — Localized store screenshots

**State:** Proposed. **Type:** Distribution. **Risk:** Translation and creative cost. **Evidence:** P2,H1. **R/I/C/E:** 4/3/3/2; **illustrative score:** 18.0.

- **Player problem:** An English-only store page underuses certified locales.
- **Player-visible behavior:** Localize the first three causal-loop screenshots for each launch-certified market.
- **Acceptance criteria:** Native review, truthful screenshots and legible mobile text for every targeted listing.
- **Success signal:** Store-page conversion by locale, measured via platform tools.
- **Dependencies:** EG-010, EG-003, launch art.

### EG-207 — ASO positioning experiments

**State:** Proposed. **Type:** Distribution. **Risk:** Low traffic yields inconclusive tests. **Evidence:** P2,H1. **R/I/C/E:** 4/4/3/2; **illustrative score:** 24.0.

- **Player problem:** One listing may not communicate both puzzle and village appeal.
- **Player-visible behavior:** Test truthful 'water puzzle', 'cozy offline', and 'choices change village' creative/metadata variants via store-supported experiments.
- **Acceptance criteria:** At least two comparable creative hypotheses; no competitor trademark stuffing or unverifiable AI/brain-training claims.
- **Success signal:** Qualified listing conversion and retained installs, with sample/uncertainty shown.
- **Dependencies:** Market clearance, EG-206, platform access.

### EG-208 — Tablet/PC expansion after mobile proof

**State:** Proposed. **Type:** Expansion. **Risk:** Platform maintenance splits founder focus. **Evidence:** H1. **R/I/C/E:** 2/3/2/4; **illustrative score:** 3.0.

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
