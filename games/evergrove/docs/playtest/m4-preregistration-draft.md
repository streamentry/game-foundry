# M4 preregistration draft: Living World vs Puzzle Only on identical boards

**Status: DRAFT. Not registered. Not recruiting.** Fill in the `TO PREDECLARE` fields and file the final version, with a timestamp, before any participant is observed. Protocol source: [10 — Experiment protocols](../specs/10-EXPERIMENT-PROTOCOLS.md), section "Preregistration". Test: `VAL-002` in [verification-matrix.json](../specs/verification-matrix.json). Gate: [09 M4](../specs/09-EXECUTION-CONTROL.md).

---

## Blocking prerequisite (must be closed before any recruiting)

**The current in-app Puzzle Only mode is not a valid control after chapter one.**

- `PlayMode.Living` uses branch-rotated, anchored boards from chapter two onward. `PlayMode.PuzzleOnly` keeps the original fixtures ([WorldModel.cs](../../Assets/Scripts/Core/WorldModel.cs), [07](../specs/07-VALIDATION-AND-GTM.md), [VALIDATION](../VALIDATION.md)).
- [09 M4 validity](../specs/09-EXECUTION-CONTROL.md#gate-m4-validity-eliminate-the-confound) requires identical board topology, starting rotations, anchor rules, hint sequence, difficulty settings, device class, text legibility, exposure and session length. Only narrative consequence visibility may differ.
- A **matched control build** (a new condition with the same boards and the same rules as the Living World build, with the village hidden) must be implemented and test-verified. It is now selectable in Editor and Development builds only (facilitator entry, see [VALIDATION.md](../VALIDATION.md)), but it **has not been tested on a device**. **This study does not start until a build with a predeclared Git SHA and hash is recorded.** Record its Git SHA and fixture hashes in section 1.
- If matching cannot be implemented, the study must be stopped, or relabelled as a non-causal preference observation ([09](../specs/09-EXECUTION-CONTROL.md), [07](../specs/07-VALIDATION-AND-GTM.md)).

---

## 1. Study identity

| Field | Entry |
| --- | --- |
| Study ID | `TO PREDECLARE` |
| Date registered (UTC) | `TO PREDECLARE` |
| Research owner (named role or person) | `TO PREDECLARE`. Research Moderator role per [09 Accountability](../specs/09-EXECUTION-CONTROL.md#accountability-roles-not-invented-people). |
| Candidate build: Git source SHA (40 characters) | `TO PREDECLARE` |
| Candidate build: artifact SHA-256 | `TO PREDECLARE` |
| Unity version | `TO PREDECLARE` (the source requests 6000.3.0f1; the installed Editor must be confirmed in its log) |
| Platform(s) and device class(es) | `TO PREDECLARE` |
| Build channel | Research / development build, with `BuildOptions.Development` ([11 §1](../specs/11-EVIDENCE-AND-RELEASE-PACKETS.md)). Must be disclosed as a research build. |
| Matched control build: Git SHA and build hash | `TO PREDECLARE`, blocking |
| Fixture hashes: Living World boards, per chapter | `TO PREDECLARE`, blocking. Per-condition puzzle hashes, recorded before observation ([09](../specs/09-EXECUTION-CONTROL.md)). |
| Fixture hashes: Puzzle Only boards, per chapter | `TO PREDECLARE`, blocking. Must match the Living World hashes. |

---

## 2. Primary question

One falsifiable sentence, as required by [10](../specs/10-EXPERIMENT-PROTOCOLS.md):

> `TO PREDECLARE`. Suggested form, from [07](../specs/07-VALIDATION-AND-GTM.md): "When board topology, rotations, hints, difficulty and session length are identical, do participants who see the village's consequences recall a specific real consequence after 10 minutes more often than participants who do not?"

Do not bundle fun, retention, art or store questions into the primary question ([10](../specs/10-EXPERIMENT-PROTOCOLS.md)).

## 3. Hypotheses

- **H1 (directional):** Among participants on identical boards, the Living World condition yields a higher rate of unprompted causal recall than Puzzle Only. Direction and margin: `TO PREDECLARE`.
- **H0:** No difference in causal recall between conditions on identical boards.

Preference and return are not hypotheses in this study. They are secondary endpoints, and they are directional at best with 20–30 participants.

## 4. Primary endpoint

| Field | Entry |
| --- | --- |
| Behavior | Unprompted causal explanation: names a **specific prior choice** and its effect on the board, creek or village, without leading prompts ([10 primary metrics](../specs/10-EXPERIMENT-PROTOCOLS.md)). Scored against the saved event history, not against vague "it got nicer" ([07 metric dictionary](../specs/07-VALIDATION-AND-GTM.md)). |
| Timing | After 10 minutes of play, measured from the first interactive screen. Unit: 10 minutes, from [07](../specs/07-VALIDATION-AND-GTM.md). |
| Denominator | **All randomized starters, analyzed as assigned** (not only completers). |
| Source of threshold | 07: "≥70–75% recall a specific real village consequence after 10 minutes" (continue rule). |
| Failure modes to report | Guessing; parroting copy; no exposure to a consequence; confusion with a different board ([10](../specs/10-EXPERIMENT-PROTOCOLS.md)). |

## 5. Secondary endpoints (maximum three)

Predeclared to prevent cherry-picking ([10](../specs/10-EXPERIMENT-PROTOCOLS.md)).

| # | Endpoint | Denominator | Notes |
| --- | --- | --- | --- |
| S1 | Unassisted first solve of chapter one, without hint or facilitator aid | All starters | Same boards in both arms, so this checks that difficulty is matched. |
| S2 | Voluntary start of chapter two after being told they may stop, without prompting | Completers of chapter one and all starters, reported separately | Not counted if the participant was pressed by the facilitator ([VALIDATION](../VALIDATION.md)). |
| S3 | Living-world preference, forced or ranked, with an open reason | `TO PREDECLARE`: see the design issue in section 7 | Report order and confidence. |

Return (D1/D7) is **not** an endpoint in this study. It needs a separately consented follow-up and is tracked as `VAL-004` at gate M6 ([09](../specs/09-EXECUTION-CONTROL.md)).

## 6. Design, sample and allocation

| Field | Entry |
| --- | --- |
| Design | Between-subject, two arms, with consequence visibility as the only intended difference. |
| Sample size, planned | 20–30 ([09](../specs/09-EXECUTION-CONTROL.md), [10](../specs/10-EXPERIMENT-PROTOCOLS.md)). Exact N: `TO PREDECLARE`. |
| Power or design analysis | None in the source docs. `TO PREDECLARE`. Without one, results are directional only; no significance claim ([10](../specs/10-EXPERIMENT-PROTOCOLS.md)). |
| Allocation method | Permuted blocks of 4 (two per arm within each block), per [10](../specs/10-EXPERIMENT-PROTOCOLS.md). |
| Random seed | `SEED_PLACEHOLDER`, value `TO PREDECLARE`. Record the seed, the generator and the full allocation sequence before enrollment. |
| Allocation order | By consent timestamp of eligible participants ([10](../specs/10-EXPERIMENT-PROTOCOLS.md)). |
| Concealment | The allocation list is hidden from the facilitator until the participant has consented. Named person who holds the list: `TO PREDECLARE`. |
| Recorded at enrollment | Device, language, prior puzzle experience, age band ([10](../specs/10-EXPERIMENT-PROTOCOLS.md)). |
| Treatment (Living World) | Matched build, village visible, visible consequences on. |
| Control (Puzzle Only) | Matched build, same boards and rules, village consequences hidden. Must be built (see the blocking prerequisite). |
| Held identical | Board topology, starting rotations, anchors, hint sequence, difficulty, device class, text legibility, instruction duration, session length ([09](../specs/09-EXECUTION-CONTROL.md)). |
| Instructions | The same neutral opener as the M3 kit (`m3-session-kit.md`, section 3). |

**Note on the mode switch:** the in-game Living World / Puzzle Only toggle is a facilitator tool, not randomization ([07](../specs/07-VALIDATION-AND-GTM.md)). Do not use it to assign participants.

## 7. Design issue: the preference endpoint

The continue rule in section 8 includes "a clear majority prefers the living-world condition". A between-subject design shows each participant only one condition, so a preference between conditions cannot be observed unless each participant experiences both. Two options, to be chosen and recorded before enrollment:

- **Option A:** Keep the study between-subject. Drop S3 from the decision rule. Preference is then not a continue criterion, and the decision relies on primary endpoint and S1–S2.
- **Option B:** Add an within-subject exposure (each participant plays both conditions, order counterbalanced). This changes the design, the time per session, and the carryover risk. It needs its own section and a new allocation sequence.

`TO PREDECLARE`. Do not decide this after seeing the data.

## 8. Exclusion and missing data

Defined before observation ([10](../specs/10-EXPERIMENT-PROTOCOLS.md)).

- **Not excluded:** incomplete sessions, abandonment, hint use, and restarts. Report every starter in the denominator.
- **Excluded, pre-specified technical failure:** list each condition, for example crash or save corruption on a device, that invalidates a session. `TO PREDECLARE`. Each excluded session is reported with its reason.
- **Consent withdrawal:** the record is deleted by policy, and the participant is removed from the denominator with the reason recorded. Withdrawals are reported as a count, not as data.
- **Staff, founders and their close circle:** excluded at screening, as in [m3-session-kit.md](m3-session-kit.md). Record any excluded starter.
- **Crossovers:** report separately. Analysis is as assigned.
- **Stop conditions:** save loss, mobile build failure, or a broken touch path stop the study and invalidate affected sessions ([10](../specs/10-EXPERIMENT-PROTOCOLS.md)).

## 9. Observation script

Same as `m3-session-kit.md`, sections 3 and 4, with these additions:

- Record the saved event history for each participant so causal recall can be scored against it.
- Blind the scorer where possible: the person scoring recall should not know the condition. `TO PREDECLARE` who scores.
- Keep the wording of every prompt identical across arms.

## 10. Analysis (as assigned)

- Compare raw counts and medians per arm, for each endpoint, with its denominator.
- Show failures and repeated failure patterns, with verbatim quotes.
- Report uncertainty and small-sample limits. Do not claim statistical significance or a population effect from 20–30 people ([10](../specs/10-EXPERIMENT-PROTOCOLS.md), [07](../specs/07-VALIDATION-AND-GTM.md)).
- Method for uncertainty intervals: `TO PREDECLARE`. The source docs do not declare one.
- Strongest alternative explanation must be stated, including puzzle difficulty and the age-band mix of each arm.
- Subgroups by age band (45–54, 55–64, 65+) are reported descriptively with their own denominators. They are not tested as separate hypotheses.

## 11. Continue / simplify / stop rule

Quoted from [07 — Provisional thresholds and stop rules](../specs/07-VALIDATION-AND-GTM.md), with the table from [10](../specs/10-EXPERIMENT-PROTOCOLS.md). Both are labelled heuristics, not validated benchmarks. Decide the final thresholds before enrollment.

| Decision | Condition (source) | Action |
| --- | --- | --- |
| **Continue** | ≥80% of target first-time testers explain the goal unaided; ≥70–75% recall a specific village consequence after 10 minutes; a clear majority prefers the living-world condition when difficulty is matched. Show raw counts. | Continue testing the integrated concept. The preference clause depends on the option chosen in section 7. |
| **Simplify** | Puzzle enjoyment and comprehension are strong, but causal recall, attachment or matched preference is weak after two presentation improvements. | Simplify the village layer. Test a pure puzzle alongside ([07](../specs/07-VALIDATION-AND-GTM.md), [10](../specs/10-EXPERIMENT-PROTOCOLS.md): "Consequences understood but no preference"). |
| **Stop or pivot** | The living-world layer adds no observable preference or return desire after a fair test; or readability stays poor; or Puzzle Only is preferred ([vision §5](../../vision.md)). | Stop scope expansion. Become a premium pure puzzle, and drop the world layers ([09](../specs/09-EXECUTION-CONTROL.md), [11 §5](../specs/11-EVIDENCE-AND-RELEASE-PACKETS.md)). |
| **Invalid** | Matching not achieved, or fixture hashes differ between arms, or a technical failure affected arms unequally. | Do not report a living-world lift. Relabel as a non-causal observation or stop ([09](../specs/09-EXECUTION-CONTROL.md)). |

The **one signed decision** is recorded in [results-template.md](results-template.md), section 6.

Any change to these thresholds after data collection begins is a documented deviation, dated, with the original kept ([10 Deviations](../specs/10-EXPERIMENT-PROTOCOLS.md)).

## 12. Privacy and safety

- Collect: anonymous participant code, age band, device class, language, prior experience, timed observations, verbatim quotes, and consent flags.
- Do not collect: names, contact details, voice or faces unless separately consented, IP, location, medical or accessibility details ([10](../specs/10-EXPERIMENT-PROTOCOLS.md)).
- Retention and deletion schedule: `TO PREDECLARE`.
- Recruitment source: recorded per starter.

## 13. Deviations log

| Date (UTC) | Deviation | Reason | Who approved |
| --- | --- | --- | --- |
| | | | |

Preserve the original text of each section above. Record changes here as dated entries rather than editing the original ([10](../specs/10-EXPERIMENT-PROTOCOLS.md)).
