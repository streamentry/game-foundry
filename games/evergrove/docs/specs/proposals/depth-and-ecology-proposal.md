# Depth and ecology proposal: puzzle mechanics that change the board

**Status: proposal only. Not authorized until M4 passes.** No production code, fixtures, catalog data or specs were changed. M4 is not met today: [09-EXECUTION-CONTROL](../09-EXECUTION-CONTROL.md) requires M1 (licensed Unity compilation) through M3 first, and every `PUZ-*` entry in [verification-matrix.json](../verification-matrix.json) is `verified: false`. Expanded ecological work is forbidden until M4 passes.

Sources read: [GAMEPLAY_AND_WORLD_RULES](../../GAMEPLAY_AND_WORLD_RULES.md), [03-PUZZLES-AND-CONTENT](../03-PUZZLES-AND-CONTENT.md), [02-FEATURE-BACKLOG](../02-FEATURE-BACKLOG.md) (EG-004, EG-005, EG-105, EG-106, EG-110), [PipeRules.cs](../../../Assets/Scripts/Core/PipeRules.cs), [PuzzleDiagnostics.cs](../../../Assets/Scripts/Core/PuzzleDiagnostics.cs), [ReactiveWatercourses.cs](../../../Assets/Scripts/Core/ReactiveWatercourses.cs), [puzzle-catalog.generated.json](../puzzle-catalog.generated.json), and [vision.md](../../../vision.md) §2 and §8.

---

## 1. Current depth audit

### What was measured (from the catalog file, and re-checked)

I re-implemented the exhaustive solver in Python (scratch script, not committed) and ran it over all 45 catalog rows. Every structural field in the catalog reproduced exactly. This is an independent check of the data, not the repo's C# test run, which I did not execute.

| Group | Profiles | Cells | Pipe mix (E / I / L) | Mutable tiles | Solutions (cap 2) | Min clockwise taps |
|---|---|---|---|---|---|---|
| 3×3 base (ch 1–6) | 6 | 9 | 2 / 2–3 / 4–5 | 9 | 1 each | 6–9 |
| 3×3 variants (ch 2–6) | 15 | 9 | 2 / 2–3 / 4–5 | 8 (one anchored) | 1 each | 5–9 |
| 4×4 base (ch 7–12) | 6 | 16 | 2 / 4–7 / 7–10 | 16 | 1 each | 22–28 |
| 4×4 variants (ch 7–12) | 18 | 16 | 2 / 4–7 / 7–10 | 15 (one anchored) | 1 each | 20–28 |

- **Uniqueness:** 45 of 45 boards have exactly one solution under the game's completion rule.
- **Structure of every solution:** every pipe has two ports except the two endpoints, which have one. A solution must be connected and leak-free, so it is a single path from spring to outlet that visits every tile. Each tile's type (straight or corner) fixes how the path turns there. The puzzle is a Hamiltonian path with prescribed straight/turn pattern.
- **Variant effect, measured:** the difference in minimum taps between a base board and its variant is 0 to 3, for all 33 variants. It equals the rotation distance of the single anchored tile, because the anchor starts already aligned. Rotating the board changes nothing else. This confirms the vision's claim: variants are rotation-isomorphic, and the only structural change is one tile's effort.
- **Starting state:** every 4×4 base board starts with all 16 tiles misaligned. 3×3 base boards start with 4–6 misaligned.
- **Difficulty fields present:** `minimumClockwiseTaps`, `initiallyMisalignedTiles`, `startingConnectedTiles`, `startingLeaks`, `solutionCountAtMostTwo`, `anchoredTile`. There is no difficulty band field, despite EG-004's listed metadata.

### What was not measured

- **No human solve times, hint rates, mis-taps, abandonment or ratings.** All `observed*`, `forcedMoveCount`, `humanReviewer` and `reviewDate` fields are `null`. Nothing in this proposal should be read as a measured difficulty.
- **Minimum taps is a structural signal, not a difficulty measure.** It is the sum of clockwise distances to the solution, with straight pieces counted modulo 2.
- **Solver runtime and any board larger than 4×4** were not measured. A 4×5 or 5×5 solver run is untested.
- **The 33 variants** have only been checked for uniqueness and rotation invariance. They have not been checked for any change in legal strategy beyond the anchored tile.

### Reading of the audit

Any metric invariant under board rotation is identical across branches. The variant effect is confined to one anchored tile. So depth has to come from changing the graph or the legal move set, not from rotating or re-anchoring. That is the gap EG-105 names.

---

## 2. Candidate mechanics

Each candidate keeps the one-sentence win rule where possible: *turn tiles until one unbroken, leak-free stream runs from spring through every pipe.* The "rule change" column flags any candidate that breaks it.

| # | Mechanic | Village tie | Changes | Rule change |
|---|---|---|---|---|
| C1 | Blank root/stone cells (no ports, cannot turn) | Tree choice (`TreeCount`) | Graph (cells removed) | None, if "every pipe" means non-blank cells |
| C2 | Orientation-masked fragile tiles | Flowers choice (`FlowerCount`) | Legal moves and search space | None |
| C3 | Non-square growth: a row or column added | Bridge choice (`CrossingCount`) | Board size and search space | None |
| C4 | Authored distinct layouts per branch (not rotation) | Any choice | Graph | None |
| C5 | Junction tees and crosses (branching) | Bridge choice | Graph and win condition | **Yes**: branching breaks the single-path win |
| C6 | Relocated outlet by choice | Flowers or Bridge | Endpoint positions | None, but see code hazard below |

### C1. Blank root/stone cells: Tree choice removes cells

- **Mechanic:** a Tree choice places one or more blank root/stump cells. They have no ports, cannot rotate, and are not part of the stream. Water must route around them. The number of blanks could grow with cumulative `TreeCount`.
- **Why it matters:** it changes the graph. A blank cell removes a vertex from the Hamiltonian path, so the solution is no longer a rotation of the base board.
- **Rule impact:** none, if "every pipe" means non-blank cells. The current win check compares `ConnectedCount` with `CellCount`, so it must compare with the pipe count instead.
- **Uniqueness check:** the existing exhaustive search works after three changes: a blank kind with one orientation and no ports; the connected-count check uses pipe count; a pipe pointing into a blank counts as a leak, which the existing port-mismatch logic already does if blank has no ports.
- **Readability risk for adults 45+:** a blank cell might read as empty space or a rendering gap. Mitigation: a clearly drawn stump or stone with a text label, and the existing `!` leak markers on pipes that point into it. Shape and label, not colour.
- **Authoring cost:** low. One new kind and per-variant layouts. Fixture tooling needs to handle variable pipe counts.
- **Spec fit:** respects the one-sentence rule. Requires editing EG-105's wording ("roots restrict rotations" becomes "roots occupy cells") and adding a blank-cell field to EG-004 metadata.

### C2. Orientation-masked fragile tiles: Flowers choice

- **Mechanic:** a meadow tile is only allowed some orientations (for example, straight only). The tap cycle skips disallowed orientations, and the disallowed positions are shown.
- **Why it matters:** it changes legal moves, not just the anchored tile. EG-105 asks for exactly this: "changed valid moves or solution strategy beyond board rotation."
- **Rule impact:** none.
- **Uniqueness check:** the masks go into `ValidRotations`, the search loop and the minimum-tap calculation (distance to the next allowed orientation). Reject any mask that produces zero or more than one solution.
- **Readability risk:** taps that appear to do nothing will read as a bug to older players. Mitigation: a visible fragile frame, a short message when a tap is blocked, and no motion or timing involved. This is the main usability risk among the top two.
- **Authoring cost:** low to medium. One mask per fixture, checked by the solver.
- **Spec fit:** matches EG-105's "meadow protects fragile routes" and the spec's "limited permitted path shape." Must respect PUZ-002: the hint ladder's align step has to choose a target reachable under the mask.

### C3. Non-square growth: Bridge adds a row or column

- **Mechanic:** a Bridge choice adds a row or column. Boards grow from 3×3 and 4×4 to 3×4 or 4×5.
- **Why it matters:** the search space grows substantially. Uniqueness becomes harder to achieve and easier to verify only by checking.
- **Rule impact:** none.
- **Code impact:** moderate. `Puzzle` requires a square layout of 9 or 16 cells, and `Neighbor` assumes equal row and column counts.
- **Uniqueness check:** the exhaustive search should still be exact. Runtime on 20 cells is not measured.
- **Readability and touch risk:** this is the most physically constrained option. Geometry estimate only: a 350 pt board gives about 87 pt per tile at 4 columns and about 70 pt at 5. Both exceed the 44 pt iOS and 48 dp Android targets in [05-ACCESSIBILITY](../05-ACCESSIBILITY-AND-LOCALIZATION.md). The binding constraint is phone height and scanning, not hit area. Must be tested on device under PUZ-006.
- **Authoring cost:** low once the solver handles non-square boards.

### C4. Authored distinct layouts per branch (replace rotation)

- **Mechanic:** each branch gets its own pipe-kind layout, verified unique by the solver, instead of a rotated copy.
- **Why it matters:** this is the most direct fix. It changes the graph with no rule change, and it is the only candidate that clearly addresses the vision's complaint that 33 variants add no structural difficulty.
- **Rule impact:** none.
- **Uniqueness check:** exact, offline, with the existing solver. EG-110 already describes the pipeline: generate offline, reject multi-solution and near-duplicate boards, and have a human approve each board.
- **Readability:** the same as base boards. No new mechanic to teach.
- **Authoring cost:** high. 33 boards to author, solve and approve. EG-110 is a tool, not a shortcut, and the 09 table forbids "large content production" before M4.
- **Spec fit:** consistent with EG-110's "no unchecked random boards at runtime" rule. Best treated as the fallback if C1 or C2 fail.

### C5. Junction tees and crosses: Bridge choice adds branching

- **Mechanic:** tee (3 ports, four orientations) and cross (4 ports) tiles let water branch.
- **Why it matters:** the graph and search space change most of all here.
- **Rule impact:** **yes, and it is serious.** A tee creates a third endpoint in a tree. The one-sentence rule and the "exactly two endpoints" win condition in [GAMEPLAY_AND_WORLD_RULES](../../GAMEPLAY_AND_WORLD_RULES.md) both have to change. Vision §2 item 1 treats that sentence as the product's core.
- **Uniqueness check:** the solver extends easily, but the rule rewrite is the real cost.
- **Readability:** many glyph types, and crosses are hard to read at small size. Colour-only confusion is a known risk.
- **Verdict:** deferred. Revisit only if the rule sentence is rewritten and tested separately.

### C6. Relocated outlet by choice

- **Mechanic:** a Flowers choice moves the outlet to the meadow edge, or a Bridge choice moves the spring to the bank.
- **Why it matters:** the endpoints change, which changes which Hamiltonian path is valid.
- **Rule impact:** none in principle.
- **Code hazard:** `PipeRules.Inspect` treats the first `E` in row-major order as the spring. Moving an outlet to an earlier row can silently swap spring and outlet. Any version of this mechanic needs an explicit spring index, not traversal order.
- **Uniqueness check:** exact, with the existing solver.
- **Readability:** endpoint icons (blue waves, green sprout) must stay clear at new positions.
- **Authoring cost:** low to medium.

---

## 3. Ranking and recommendation

**Prototype first: C1 (blank root/stone cells, Tree choice).**
It changes the graph and keeps the one-sentence win rule. It has the smallest engine change, the exact solver already covers it, it ties directly to a cumulative count, and its readability risk is about recognition, which can be tested.

**Prototype second: C2 (orientation-masked fragile tiles, Flowers choice).**
It changes legal moves, not just the graph, which is exactly what EG-105 asks for. It keeps the rule and uses the existing solver. Its main risk is the "tap looks broken" problem, which is a fair thing to test with adults 45+.

**Not chosen now:**
- **C3** is the best candidate for the Bridge choice, which currently has no strong mechanic. It is held because phone size and scanning are untested.
- **C4** is the fallback if neither C1 nor C2 creates a comprehensible difference. It is expensive.
- **C5** breaks the core rule.
- **C6** is viable but needs the spring-index fix first.

Bridge is an open gap: no candidate yet changes its board without a rule change or a large content job. Flag this for the Product Owner.

---

## 4. Smallest test for C1 and C2

Run one test per mechanic, not both at once, so their effects can't be confused.

**Participants:** 8–10 uncoached adults aged 45+ on a compact Android or iPhone build (the M2 device class). Record age band (45–54, 55–64, 65+). Do not infer vision, dexterity or confidence from age.

**Note on sample size:** the user brief suggested 6–10. The spec's formative range is 8–12 ([09-EXECUTION-CONTROL](../09-EXECUTION-CONTROL.md) M3 and M4). Use 8 minimum. Six is below that floor and would be observation only.

**Stimuli (matched, preregistered by puzzle hash before any session):**
1. A base board from the same chapter (rotation-only, same cell count, solution count 1, minimum taps within ±1 of the mechanic board).
2. The mechanic board: one blank cell (C1) or one fragile mask (C2).
3. A later board whose layout depends on the choice made after board 1.

Counterbalance the order of 1 and 2 across participants. Record puzzle hashes and build SHA.

**Predeclared measures:**
- **Primary:** share of participants who can explain, unprompted or after one neutral question, how the earlier choice changed the later board. Threshold: **≥70%** (EG-105). Report as n/N with no population estimate.
- **Spike flag:** median solve time on board 2 or 3 more than **2×** the median of the preceding base board. Flag for review, not automatic rejection (EG-004).
- **Secondary:** hint use, mis-taps on the blank or masked tile, restarts, abandonment.
- **Recognition:** whether participants identify the blank or fragile tile without help, checked in greyscale.

**Kill criteria (predeclared; any one triggers a stop or redesign for that mechanic):**
- **K1:** fewer than 70% explain the link (fewer than 6 of 8, or 7 of 10).
- **K2:** the spike flag fires on the mechanic board in at least half the sample, and the cause is not a UI defect that can be fixed in the session.
- **K3:** fewer than half identify the blank or masked tile without help.
- **K4:** two or more of the first four participants abandon the mechanic board.
- **K5 (automated, before any playtest):** the mechanic fails the solver gate. Any non-unique board, or any board where the new rule adds no legal strategy beyond rotation (PUZ-004), is rejected.

**Pass** means the mechanic is a candidate for the EG-105 expansion. It still does not authorize shipping. That needs the matched Living World vs Puzzle Only study at 20–30 participants, and M4.

---

## 5. Caveats

- Every uniqueness claim for C1–C6 is a design claim. No solver run has been performed on any new mechanic.
- The 45 existing profiles were re-checked in Python, not through the C# test suite.
- Solver runtime for boards larger than 4×4 is unmeasured.
- Any mechanic that reads as "rotation only" to a player fails the test even if the solver passes it.
- Adding a mechanic does not solve retention or demand. The vision's own §8 stop rule still applies.

---

**Status: proposal only, not authorized until M4 passes.** M4 requires M1 (licensed Unity compile) and M2/M3 (device and comprehension evidence), none of which is verified.
