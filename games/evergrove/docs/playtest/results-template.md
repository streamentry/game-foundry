# Results template: M3 and M4 raw counts

**Status: blank.** Nothing here is a result. Fill it only from observation sheets recorded during real sessions. Rules come from [10 — Experiment protocols](../specs/10-EXPERIMENT-PROTOCOLS.md) and [09 — Execution control](../specs/09-EXECUTION-CONTROL.md).

## Reporting rules

- Every percentage is printed next to its denominator as `n/N (x%)`. A percentage with no `N` is not allowed.
- Use `N` = all starters of the group, unless the endpoint names a narrower denominator (for example chapter-one completers for S2 in M4).
- Missing or `not_observed` values are counted as missing, not as failures and not as successes. Report missing counts in their own row.
- Small-sample rule: these are formative or directional. Do not report a population rate, a significance claim, or a D1/D7 figure from these tables ([09](../specs/09-EXECUTION-CONTROL.md), [10](../specs/10-EXPERIMENT-PROTOCOLS.md)).
- Participants are identified only by anonymous code, for example `P-01`. No names.

---

## 1. Study identification

| Field | Value |
| --- | --- |
| Gate | M3 / M4 |
| Study or session-series ID | `TO PREDECLARE` |
| Candidate build: Git SHA (40 characters) | |
| Candidate build: artifact SHA-256 | |
| Unity version (from the Editor log) | |
| Build channel | research / development / consumer |
| Fixture hashes (M4 only) | Living World: ; Puzzle Only: |
| Preregistration reference | [m4-preregistration-draft.md](m4-preregistration-draft.md) (M4) or [m3-session-kit.md](m3-session-kit.md) (M3) |
| Date range of sessions (UTC) | from: to: |
| Research owner | |
| Scorer (blinded where M4 requires it) | |

## 2. Starters, exclusions and missingness (per age band)

Age band is recorded at screening. Use the bands 45–54, 55–64, 65+, plus `not recorded` if a band is missing.

| Age band | Recruited | Screened out | Excluded after start (reason) | Starters (N) | Completed chapter one | Withdrawn (consent) |
| --- | --- | --- | --- | --- | --- | --- |
| 45–54 | | | | | | |
| 55–64 | | | | | | |
| 65+ | | | | | | |
| not recorded | | | | | | |
| **Total** | | | | | | |

For M4, repeat this table per arm: Living World `N=`, Puzzle Only `N=`. Report crossovers separately if any.

## 3. Device and language mix

| Device class | Starters (N) | Notes |
| --- | --- | --- |
| Compact phone | | |
| Larger phone | | |
| Other / unknown | | |

| Language (certified locales only for shipping claims) | Starters (N) |
| --- | --- |
| | |

## 4. M3: first-minute observations (per age band)

Denominator for each row is all starters in that age band, unless stated.

| Measure | 45–54 | 55–64 | 65+ | All starters |
| --- | --- | --- | --- | --- |
| First correct action within 30 s, unaided (`n/N`) | | | | |
| Time to first meaningful input, median (s), with `n` observed | | | | |
| Unaided goal explanation, yes (`n/N`) | | | | |
| Unaided goal explanation, partial (`n/N`) | | | | |
| Restoration noticed, unprompted (`n/N`) | | | | |
| Consequence noticed (`n/N`), with prompted and unprompted counts | | | | |
| Causal recall after 10 min: specific prior choice named (`n/N`) | | | | |
| Mis-taps, median per 10 min, with `n` observed | | | | |
| Hint requests, median per session (all requests) | | | | |
| Tiles aligned by hints (`WorldModel.HintsUsed`), median per session | | | | |
| Restarts, median per session | | | | |
| Abandoned (`n/N`) | | | | |
| Facilitator interventions (should be 0) | | | | |

**Failure modes (counts, with denominator):**

| Failure mode | 45–54 | 55–64 | 65+ | All |
| --- | --- | --- | --- | --- |
| Could not see the tile | | | | |
| Tapped the wrong target | | | | |
| Did not understand rotate | | | | |
| Misread water flow | | | | |
| Unresponsive UI | | | | |
| Other (describe in notes) | | | | |

## 5. M4: matched comparison (as assigned)

Allocation: permuted blocks of 4, seed `SEED_PLACEHOLDER` (see the preregistration draft). Analysis is as assigned. Crossovers are shown separately.

| Endpoint | Living World (n/N) | Puzzle Only (n/N) | Denominator source |
| --- | --- | --- | --- |
| Primary: unprompted causal explanation after 10 min | | | All randomized starters |
| S1: unassisted first solve, chapter one | | | All starters |
| S2: voluntary start of chapter two (chapter-one completers) | | | Completers |
| S2: voluntary start of chapter two (all starters) | | | All starters |
| S3: preference, if the design in the preregistration includes it | | | `TO PREDECLARE` |

**Medians (with `n`):**

| Measure | Living World | Puzzle Only |
| --- | --- | --- |
| Time to solve chapter one (s) | | |
| Hints requested per chapter | | |
| Restarts per chapter | | |

**Matching verification (must be completed before analysis):**

| Check | Result |
| --- | --- |
| Board topology hashes identical across arms | |
| Starting rotations identical | |
| Anchor rules identical | |
| Hint sequence identical | |
| Session length (instruction duration) comparable | |
| Device class recorded per arm | |
| Age band mix per arm | |

If any check fails, see the "Invalid" row of the decision rule in the preregistration.

**Alternative explanations to state:** `TO PREDECLARE` in the write-up: puzzle difficulty differences, age-band imbalance, device imbalance, facilitator effects, order effects.

## 6. Verbatim observations

Exact words, by prompt. One row per quote. No interpreted emotion labels.

| Code | Prompt (from the observation sheet) | Verbatim quote | Age band | Condition (M4) |
| --- | --- | --- | --- | --- |
| P-01 | | | | |

Do not paste anything that identifies a participant. Use a redacted form for any quote that includes a name, place or detail.

## 7. Continue / simplify / stop record

Fill this in once, after the data are complete. Use the decision rule as it was predeclared. Do not edit the thresholds after seeing the data; record any change as a dated deviation ([10](../specs/10-EXPERIMENT-PROTOCOLS.md)).

| Field | Entry |
| --- | --- |
| Gate | M3 / M4 |
| Decision date (UTC) | |
| Thresholds used (link to the preregistration or to the M3 section) | |
| Observed values, with denominators (link to sections 2–5) | |
| Decision | Continue / Simplify / Stop or pivot / Invalid |
| Strongest argument against this decision | |
| Alternative explanation considered | |
| Deviations from the protocol (with dates) | |
| Action and owner | |
| Rollback or revisit trigger | |
| Signatories (role, name, date) | |

**Decision log link:** record the same decision in [decision-log.md](../../../../docs/research/decision-log.md) using its template fields: `Date / candidate`, `Decision`, `Player & search intent`, `Evidence`, `Strongest alternative explanation`, `Assumptions`, `Cheapest falsification`, `Action within seven days`, `Continue / stop thresholds`. Paste the log entry's date and heading here:

- Decision log entry: `TO PREDECLARE` (date and heading)

Do not record a decision based on a pleasant interview alone ([10 Interpretation](../specs/10-EXPERIMENT-PROTOCOLS.md)).
