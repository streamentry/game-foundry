# 03 — Puzzle quality, difficulty and content pipeline

**Goal:** puzzles are short, fair, readable and increasingly interesting; the ecology makes the player reason differently without sacrificing calm. A mathematically unique answer is necessary for this design, **not sufficient** for player enjoyment.

## Existing rule system (verified in source, not on device)

- `PuzzleCatalog` defines **12 authored boards**: six 3×3 and six 4×4, with end/straight/corner tiles.
- `PipeRules` checks rotations, flow connectivity, unmatched joins/leaks and completion; a test-side exhaustive search checks unique solutions.
- `ReactiveWatercourses` creates **33** follow-up variants (11 chapters × 3 prior choices). It rotates the entire source board, rotates each tile orientation and anchors one already-correct non-end tile. This **preserves a valid unique solution**, but does **not** yet create genuinely different root/bridge/flower logic.
- `WorldModel` now persists a voluntary **three-stage hint** (row → tile → align) in snapshot **v4** alongside one-step undo/restart. The direct `GuideOneConnection()` compatibility helper still aligns one incorrect tile immediately; release UI uses `AdvanceGentleHint()`.
- The source tests check all originals/variants; **these are domain checks, not Unity/device or human difficulty results**.

Source: [PipeRules.cs](../../Assets/Scripts/Core/PipeRules.cs), [ReactiveWatercourses.cs](../../Assets/Scripts/Core/ReactiveWatercourses.cs), [WorldModel.cs](../../Assets/Scripts/Core/WorldModel.cs), [Tests/Program.cs](../../Tests/Program.cs).

## Difficulty principles

1. **Teach before testing:** show one tile rotation, then connected water, then leaks, then anchored habitat, then recombinations.
2. **No hidden penalty:** no clock, lives, forced ads, currency or shame copy. Undo/restart/hints are always available.
3. **Readable feedback:** water reach, open ports and protected tiles are shown by geometry/pattern/label, never hue alone.
4. **Honest challenge:** do not silently solve a board or claim an anchored tile is movable. Optional difficulty choices must be transparent.
5. **Fairness is empirical:** solver distance and branch count are proxies. Human solve time, mis-taps, hint usage and confusion determine the final ladder.
6. **Content economy:** prefer recombining two known constraints over introducing a disposable gimmick.

### Proposed 12-stream learning arc

| Segment | Target experience | Guardrail |
| --- | --- | --- |
| 1–3 | First solve quickly, recognize water flow, make first choice, see first altered board | No mandatory reading or novel mechanic before a first reward |
| 4–6 | Recognize two ecological constraints and their effect on route planning | No >2× unexplained median-time jump without review |
| 7–9 | 4×4 boards combine already learned ideas | Test with players using larger text and older phones |
| 10–12 | Intentional synthesis of prior mechanics; satisfying final world outcome | Challenge must come from reasoning, not tiny targets or visual ambiguity |

These are **proposed content targets**; current chapter fixtures are not certified against them.

## Required authoring metadata (EG-004)

A source-controlled, deterministic record for every authored puzzle and variant:

```json
{
  "id": "stream-01",
  "chapter": 1,
  "branch": "base",
  "size": 3,
  "tileKinds": "LIEIELLIL",
  "initialOrientations": [],
  "solutionOrientations": [],
  "anchoredTile": null,
  "ecologicalConstraint": null,
  "solutionCount": null,
  "minimumPlayerRotations": null,
  "forcedMoveCount": null,
  "plausibleErrorPoints": [],
  "hintPlan": [],
  "targetDifficulty": "gentle",
  "observedMedianSeconds": null,
  "observedP90Seconds": null,
  "observedHintRate": null,
  "reviewer": null,
  "reviewDate": null
}
```

**This is a schema illustration, not a valid fixture or measured result.** Use the actual source arrays; never fill missing solve times with fabricated numbers. Clarify whether `minimumPlayerRotations` counts clockwise taps only and how symmetric straight pipes are normalized. If the solver returns equivalent orientation encodings, canonicalize them before calling a solution unique.

### Acceptance

- Every published board has valid start and end; exact solution count = 1 under the game's completion rule.
- Every ecological anchor is non-end, already aligned, visually explained and non-rotatable.
- Every hint step is demonstrably helpful and does not contradict the solved route.
- A tester can explain *why* the final connection worked; random tapping alone is not the intended experience.
- Source fixture, metadata, translation label and replay/save identity remain consistent across builds.

## Progressive hint ladder (EG-005) — source implementation v4, device QA pending

| Step | Player sees | Technical rule |
| --- | --- | --- |
| 1 — Notice | Soft highlight around a region with a useful next move | Never highlights only a decorative/anchored tile; no solution rotation |
| 2 — Focus | One tile plus a plain-language reason to inspect it | Works for each current rotation state, not only authored initial state |
| 3 — Help me turn | One correct rotation with clear feedback | May require multiple uses; does not auto-complete the entire puzzle |

- No penalty, score, currency or cooldown.
- Existing one-tile hint can be reused as step 3 **after** validating it truly advances from arbitrary intermediate states.
- Hint use and undo history must be deterministic after save/load. Do not quietly reset the count to flatter metrics.
- The user can ignore hints; the UI must not repeatedly nag.

### Important current-hint behavior (audited source)

`WorldModel.GuideOneConnection()` now uses `HintPlanner.SelectTarget()` to prefer an incorrect, non-anchored tile near the connected-water frontier, then **sets its orientation directly to `puzzle.Solution[tile]`**, increments `hintsUsed`, saves the previous rotations for one undo and re-evaluates the solved board. It does **not** just highlight a region, and can skip multiple clockwise rotations. `Undo()` restores the tile state but does **not** refund the recorded hint. The normal UI uses `AdvanceGentleHint()` to expose the first two non-mutating stages before this mutation.

Therefore the proposed three-stage ladder has three **distinct mutation contracts**:

- **Stage A — notice:** only highlights a useful region; no puzzle state mutation.
- **Stage B — focus:** identifies one tile and a reason; no puzzle state mutation.
- **Stage C — align:** sets exactly one currently incorrect tile to the solution orientation (an explicitly mutating, undoable action). Do not describe it as "one clockwise rotation".

Before releasing Stage A/B, define deterministic regions and correct suggestions for every reachable intermediate board, every anchored variant and undo/restart/reload condition. The current Unity UI requests `AdvanceGentleHint()` and stores stage progress (0/1/2) and chosen target index in v4 snapshots. Old v1/v2/v3 saves initialize that ephemeral state to idle. UI stages 1 and 2 display non-mutating hints; stage 3 directly aligns the tile. Domain tests exercise the transition and undo. **No player clarity or native UI test has been performed** (`PUZ-002`).

### Calibrated metadata, not invented difficulty labels

For each **12 authored + 33 existing rotated/anchored variants**, eventually record: `PuzzleId`, `TopologyHash`, `BoardSize`, `InitialRotations`, `SolutionRotations`, `UniqueSolutionProof`, `MinimumPlayerRotations`, `AnchoredCells`, `EnvironmentalRuleId`, `HintStep1Region`, `HintStep2Tile`, `HintStep3SolveTile`, `ObservedN`, `ObservedMedianSolveSeconds`, `ObservedP90SolveSeconds`, `ObservedHintRate`, `ObservedRestartRate`, `ObservedMisTapRate`, `DifficultyBand`, `CalibrationDate`, `BuildSha`.

### 45 source-derived board profiles — implemented in C#, not player-rated

[Generated board catalog](puzzle-catalog.generated.json) is reproduced by `PuzzleDiagnostics.All()` and checked by `dotnet run --project games/evergrove/Tests/Evergrove.Core.Tests.csproj --configuration Release -- --check-puzzle-catalog`. It covers **12 original + 33 choice-conditioned** presentations, including puzzle layout, initial and solution rotations, anchored cell, source-start leaks and direct minimum clockwise taps. **No player time, difficulty band, abandonment or human rating has been measured.** Null observations must remain null. A rotated board is not an entirely new authored challenge.

`EG-004` is **Implemented-Unverified** only for deterministic diagnostics; human difficulty calibration, authoring proof metadata and editor approval remain open.

**Only the structural fields can be derived from code.** The observed metrics remain `null` until uncoached human play. `MinimumPlayerRotations` must define whether taps are clockwise and whether straight-pipe 180-degree symmetry counts as equivalent. An exhaustive solver's distance is a **structural complexity signal**, not measured player difficulty.

The [verification registry](verification-matrix.json) defines `PUZ-001`, `PUZ-002`, `PUZ-003` and `PUZ-004`. Shipping new ecology constraints without a unique-solution proof plus human explanation is a blocked change.

## Ecological puzzle mechanics (EG-105, proposed)

These are **new rule candidates**, not claims about `ReactiveWatercourses`:

| Choice | Proposed constraint | What changes in reasoning | Teach it by |
| --- | --- | --- | --- |
| Shade tree | Root-protected cells cannot rotate; water must route around/through fixed root channels | Constraint planning and route ordering | One highlighted root tile with a simple "protected" icon |
| Footbridge | A crossing/culvert must carry water while keeping a walkable path intact | Mandatory crossing and connectivity constraint | One visible bridge span, not hidden scoring |
| Wildflowers | Fragile meadow cells cannot be disturbed or have a limited permitted path shape | Spatial exclusion and conservation | A visible protected patch and gentle explanation |
| Combined choices | Two constraints interact on one small board | Transfer learning rather than a new gimmick | Reuse familiar icons and an optional recap |

**Design risk:** each new constraint can make 4×4 boards harder to parse. Ship one rule at a time, test, and discard any rule that makes causal consequences less legible. Preserve a solver-proof route in every state.

## Solver-assisted authoring workflow (EG-110)

1. Generate/author candidate topologies **offline** with fixed seed and known ecological constraints.
2. Reject disconnected, invalid, already-solved, multi-solution and near-duplicate boards.
3. Compute proof, orientation equivalence, minimum tap distance, forced moves and possible hint sequences.
4. Present a **human-review screen** with board, solution path, environmental meaning and estimated effort.
5. Have an editor play each candidate without answer visibility. Reject confusing, trivial or repetitive candidates.
6. Playtest representative players; record **observed** median/p90 solve time, hint usage, mis-taps and unsolicited complaints.
7. Freeze approved fixtures, metadata and regression tests together. Do **not** generate unchecked random boards at runtime.

**Tool success metric:** human-approved, player-liked puzzles per authoring hour; solver throughput alone is not success.

## Transparent difficulty (EG-107) and replay (EG-108/109)

- **Gentle:** fewer misleading branches, more visible water feedback, optional hints; no altered story reward.
- **Standard:** reference authored puzzle challenge.
- **Thoughtful:** more reasoning and recombination, not a smaller hit target or time pressure.
- A switch is explicit, reversible and must preserve narrative state; changing mode should not silently rewrite a player's history.
- **Return stream:** optional, playable anytime, no streaks, expiration or calendar punishment; only after evidence players want more.
- **New garden:** materially different consequence and puzzle paths, not simply shuffled art. The proposed ≥30% difference is a testable target, not a current feature.

## Puzzle acceptance and regression matrix

- **Core:** original + variant solution count, no leak on solution, no accidental pre-solved board, correct source/outlet, straight-pipe symmetry, all legal rotations.
- **Choices:** each prior choice selects the intended board; protected tile remains fixed; reconsidering a choice restores exact previous solved board.
- **Assistance:** all three hints, undo (including after hint), restart, pause/resume, localization switch, larger text and reduced motion.
- **Save:** export/restore active board and hint state exactly; legacy saves retain their intended boards.
- **Device:** 3×3/4×4 tap areas, focus/pan, visual state in grayscale, no accidental scroll when tapping.
- **Human:** observe solve-time distributions by experience/age/vision needs; record denominators and distinguish puzzle difficulty from UI difficulty.

**Stop adding mechanics** when comprehension, puzzle fairness or voluntary return declines. More puzzle rules are not automatically a better puzzle.
