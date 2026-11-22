# 10 — Experiment protocols: preregistration, assignment, measurement and decision

**Status: executable research protocol, not completed experiments.** No participants, outcomes, D1/D7 evidence or native builds are asserted here. This protocol operationalizes [07 — Validation and GTM](07-VALIDATION-AND-GTM.md), [09 — Execution control](09-EXECUTION-CONTROL.md) and test IDs `VAL-001`, `VAL-002`, `VAL-003`, `VAL-004`, `VAL-005`.

## Optional earliest check — offline browser first-minute stimulus

A small [browser research slice](../../research/first-minute/) now reproduces **the first authored Unity puzzle** and one illustrated choice, without Unity, accounts or automatic telemetry. Its code and data are deliberately distinct from the 12-chapter Unity runtime. Use it to observe **whether the first tile is visible, whether users infer rotation and water flow, whether they notice complete creek restoration, and whether the chosen village change means anything** before investing further in art.

**Evidence status:** browser behavior has source/unit tests but **has not had independent mobile browser visual/accessibility QA or participant observations**. Even after observation it is an *early formative stimulus*, **not** `VAL-001` on the actual Unity candidate build, a phone M1/M2 pass, evidence of next-chapter causality, nor a suitable randomized narrative-value test. Do not count its completions in the Unity milestone ledger. See the [browser slice's research protocol](../../research/first-minute/README.md). If people cannot explain its first water move without coaching, fix it before buying more content.

## Three questions — three separate designs

| Study | Unit/assignment | What differs | What remains identical | What the result can answer |
| --- | --- | --- | --- | --- |
| A. Uncoached usability, `VAL-001` | 8–12 target players, individual observed session; no randomization claim | Players' prior familiarity varies; report it | Same candidate build, fixed first chapter, phone class recorded | Comprehension, friction and bugs; **not** population effect size |
| B. Village value, `VAL-002` | ~20–30 people, randomized or pre-balanced assignment *before gameplay* | Village reveal/story on vs off; no mechanic differences | **Exact puzzle hashes, rotations, anchors, hints, controls, instruction duration, devices and difficulty** | Directional incremental value of story/world; not small-effect statistical certainty or D7 |
| C. Visual causality, `VAL-003` | ~10–15, within-subject A/B with order counterbalanced | 2D vs 2.5D renderer only | Same saved history, puzzle, camera content, captions, sound and motion level | Which style communicates consequence better at acceptable measured device/art cost |

**Existing modes are not a valid Study B after chapter 1**: `Living World` selects rotated, anchored boards; `Puzzle Only` keeps source fixtures. First-chapter study A may compare their opening boards, but a true B requires a **new matched-world-hidden condition** with the same board and rules. Do not deploy the B study before this is implemented and test-verified.

A fourth **commercial study** is separate: message-variant / ASO / one-time price research, measuring actual click→start and payment intent with explicit disclosures. It does not claim store ranking or retention from responses to a hypothetical survey.

## Preregistration: freeze this record before recruiting

Store a redacted public study plan in `docs/research/`; keep individual consents and unredacted recordings off public GitHub.

| Field | Required pre-commit decision |
| --- | --- |
| Study ID, date, research owner | Stable ID and assigned human; no anonymous invented owner |
| Candidate build | Git SHA, build SHA/hash, Unity version, platform, release/development flag |
| Primary question | One falsifiable sentence; no bundled “fun, retention, art, ASO” question |
| Primary endpoint | One exact observable behavior and denominator |
| Secondary endpoints | Maximum three to discourage cherry-picking |
| Eligibility / recruitment | Age range, prior puzzle skill, language, accessibility needs, acquisition source |
| Assignment | Fixed prewritten random seed + allocation sequence for between-subject study; who can view it |
| Treatment / control | Complete visual and gameplay differences; fixture hash **must match for B** |
| Sample and missingness | Planned recruited, starters, exclusions defined before test, dropouts, incomplete follow-up |
| Observation script | Exact neutral questions and timing, intervention protocol, when stopping is permitted |
| Analysis | Compare raw counts and medians, show failures; small-sample uncertainty and confounds |
| Continue/simplify/stop | Prospective thresholds and one action after each outcome |
| Privacy / safety | What is collected, explicit consent, redaction and deletion schedule |
| Deviations | Time-stamped protocol changes, preserve original; never retroactively label a deviation planned |

### Allocation method: avoid “I gave the enthusiastic person the village”

For a **two-arm, manually assigned** pilot: define ordered eligible participants by consent timestamp; prior to enrollment generate a balanced randomized block schedule (e.g., permuted blocks of 4) with a recorded seed; keep the assignment list hidden from the facilitator until that participant consents. Record device, language and prior puzzle experience. Analyze **as assigned**, report crossovers separately. If random assignment is not feasible, label the study observational and do not use causal “lift” language.

For **visual A/B**: alternate sequences AB and BA using a prewritten shuffled balanced order. Same world histories, time budget and narrative exposure. Record order/carryover effects.

## Primary metrics and truthful denominators

| Metric | Numerator / measure | Denominator | Failure modes to report |
| --- | --- | --- | --- |
| First successful tap | Participants rotating intended tile within 30 s without coaching | **All starters**, not only completers | Couldn't see tile, tapped wrong target, didn't understand rotate, UI unresponsive |
| Unassisted first solve | First chapter solved without hint/facilitator aid | All starters | Abandonment, timeouts, accidental win, hint use |
| Unprompted causal explanation | Correctly states a **specific prior choice** and its next-world/board effect without leading | All exposed to consequence; additionally report all starters | Guessing, parroting copy, no exposure, different board confusion |
| Voluntary second chapter | Starts second chapter after clearly being allowed to stop, without prompting | First chapter completers **and** all starters separately | Moderator pressure or automatic navigation |
| Qualified preference | Chooses Evergrove when equally playable polished reference is available | Participants experiencing both; show order | Forced choice, price mismatch, unfinished art |
| D1/D7 voluntary return | A consented participant opens on next day / within 7 days without an invitation tied to a reward | Consented eligible starters at baseline; report missing follow-up | Self-selection, facilitator reminders, paid session, incomplete attribution |
| Branch difficulty | Time to solve, hints, restarts by chapter+variant | Everyone exposed to that board, including abandoned | 90° rotations, protected tile reduces degrees of freedom, phone skill |

**Current in-memory `LocalPlaytest` tracks within-session counters only.** It is not an identity, session continuity, D1/D7 or consent-management system. A researcher must separately arrange permissioned follow-up; do not introduce tracking SDKs as an undocumented shortcut.

## Observational prompts

1. “Please try the game however you like. You may stop at any time.” Then **remain silent** for the first minute.
2. After first solve: “What happened?” and “What caused it?”
3. Before next chapter: “What do you expect might change?”
4. After next chapter begins: “Did anything from the earlier choice matter? What evidence makes you think so?”
5. Exit: “What was difficult or unnecessary?” “Would you open this when nobody asked you?” “Which game would you choose right now, and why?”

The moderator records **actual player phrases**, not an interpreted emotion label. No “Do you love the magical world?” wording.

## Contamination, safety and missing data

- Never show an experimental assignment picker or describe the hypothesis to players before baseline observations.
- Don't compare a highly polished art treatment with an unfinished bare control and attribute the result to “living worlds.”
- An incomplete session is **not excluded** unless a pre-specified technical failure invalidates the trial; report every original starter denominator.
- Record consent withdrawals and delete the corresponding private record by policy; publish only aggregate/redacted evidence.
- Avoid recording voice, faces, IPs, location or medical/accessibility details in the public repository.
- A participant completing a paid research task is **not a natural D1/D7 return**. Mark prompted returns separately.
- Treat small samples as directional: no claim of statistical significance or population effect from 8–30 people without a suitable power/design analysis.

## Interpretation and actual decisions

| Outcome | Action, not rhetoric |
| --- | --- |
| Cannot tap/solve | Simplify controls/first board; rerun formative study; **no** content expansion |
| Puzzle understood, consequences not | Improve visible cause-and-effect once; rerun matched small study |
| Consequences understood but no preference | Test a much simpler pure puzzle alongside; consider removing village scope |
| 2D ~ 2.5D in comprehension | Keep 2D unless 2.5D clearly reduces other costs |
| Mobile builds or save corrupt | Stop study; fix reliability; discard invalid affected sessions |
| Strong unaided attachment + spontaneous repeat + viable channel | Advance one bounded mechanic, not all P1/P2 features |

**Study exit artifact:** assigned counts + failures + verbatim anonymized observations + exact build/fixture hashes + all deviations + strongest alternative explanation + one signed `continue / simplify / stop` decision. No code development greenlight comes from a pleasant interview alone.
