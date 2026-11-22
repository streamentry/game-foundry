# 09 — Execution control: gates, evidence and release authority

**Version:** 2 (2026-11-22 audit integration). **Immutable audit baseline:** [`84713febd1e9d5921706d1916132b309b74a604f`](https://github.com/streamentry/game-foundry/commit/84713febd1e9d5921706d1916132b309b74a604f). This is the source snapshot audited before the changes in this specification, **not** an assertion that the modified branch has been built.

**Canonical machine-readable register:** [verification-matrix.json](verification-matrix.json): all **37 feature IDs**, owner *roles*, milestone gate, dependencies, player-visible outcome, verification IDs, evidence paths and stop/rollback conditions; plus **46 named test contracts**. The [feature backlog](02-FEATURE-BACKLOG.md) is the human-facing explanation. New feature or requirement changes must update both; CI rejects drift.

**Product goal:** a short, excellent puzzle that leaves a legible, truthful change in an offline village. The backlog is an *opportunity inventory*, **not 37 features promised for release**. Default next work is risk retirement, not additional scope.

## Gate semantics and test ownership

`decision_gate` in the feature ledger means the **first milestone when an item becomes relevant**, not necessarily when all its tests can complete. For example, `EG-001` starts at M1, but physical device tests BLD-004/005/006 close at M2; `EG-007` starts at M2, but the largest-text test A11Y-005 is M5. Every feature needs **all** mandatory linked verification evidence before it may become `Verified`.

The [46-test registry](verification-matrix.json) now also assigns **gate-wide tests**, such as release-channel integrity and store metadata, to milestones even when no single feature owns them. Human study setup is in [10 — protocols](10-EXPERIMENT-PROTOCOLS.md), and actual run evidence/rollback in [11 — release packets](11-EVIDENCE-AND-RELEASE-PACKETS.md).


## Source versus verification states

The feature state machine is:

`Proposed → Ready → In Progress → Implemented-Unverified → Verified`

A feature may instead become `Deferred` or `Rejected`. **`Blocked` is an independent boolean**, not a progress state. A feature may be implemented yet be blocked for verification. No transition is inferred from an enthusiastic commit message.

| State | Entry proof |
| --- | --- |
| Proposed | A candidate outcome with a hypothesis; no implementation commitment |
| Ready | Product owner approves scope, acceptance/test IDs, owner, dependency and rollback |
| In Progress | A named implementer has started tracked work |
| Implemented-Unverified | Source is present, but some mandatory Unity/device/human evidence remains absent |
| Verified | All mandatory tests have dated evidence tied to the **exact candidate build SHA**; UX claims also require observed human evidence |
| Deferred | Value/cost/dependencies currently fail the gate; include a revisit trigger |
| Rejected | Evidence shows poor fit or ethics/risk is unacceptable; record the reason |

Source-level validated means only source-level validated; it is **not** a substitute for `Verified`. All current evidence/artifact arrays in the register are empty. Do not mark a feature verified just because its test ID exists.

## Accountability (roles, not invented people)

| Role | Owns / decides |
| --- | --- |
| **Product Owner (PO)** | Player promise, opportunity cost, priority, commercial go/no-go, conditional scope approval |
| **Gameplay Engineer (GE)** | Puzzle invariants/solvers, world state, deterministic tests and save-model migration |
| **Unity/Release Engineer (URE)** | Licensed Editor, CI build, signed/unsigned artifacts, platform lifecycle, release build channel, runtime profiling |
| **UX & Accessibility QA (UXA)** | Uncoached usability, physical target sizes, screen-reader and motion/contrast requirements |
| **Localization Lead (LL)** | Locale inventory, font/text architecture, certification pipeline and translation change management |
| **Native Reviewer (NR)** | Independent linguistic/cultural and store-text sign-off for each enabled locale |
| **Research Moderator (RM)** | Recruitment, consent, preregistration, fixture matching, raw observations and bias reporting |
| **Store/Legal Owner (SLO)** | Name/store policy/brand clearance, privacy declarations and purchase architecture |

One person may hold several roles, but **native-language QA and matched human observation must not be self-certified**. Roles must be assigned to real GitHub issue assignees before moving a feature to `Ready`. Missing assignment is not implicitly the founder's acceptance.

## Milestone gates and feature freeze

| Gate | Exit evidence (all required) | Forbidden until this gate passes |
| --- | --- | --- |
| **M0 — Governed specs** | Immutable audit baseline; all 37 feature/control records present; document checker passes; no false shipping claim; current source descriptions repaired | Untracked feature growth |
| **M1 — Real Unity compilation** | Licensed Unity 6.3 Editor import, scene-bootstrap smoke, Android APK and iOS Xcode-project build **with run links and hashes** | Additional mechanics, art systems or retention |
| **M2 — Physical device alpha** | One compact Android and one iPhone complete 12 streams; suspend/resume; recoverable save corruption/interruption matrix; safe areas and tap geometry; controlled network audit; zero open S0/S1 | P1 and P2 work |
| **M3 — Core comprehension** | First-minute/visible-consequence UX tested with 8–12 **uncoached** representative participants; failures fixed or documented; puzzle calibration and hints reviewed | Replay, daily streams, social, paid expansions or retention systems |
| **M4 — Matched-value experiment** | Preregistered puzzle-identical Living World vs Puzzle Only study; raw preference/recall/continuation and uncertainty; explicit continue/simplify/stop decision | Expanded ecological world, large content production or recurring live features |
| **M5 — Launch architecture** | 2D vs 2.5D decision backed by device/player costs; Unicode shaping, text-size and accessibility review; **certified** launch-locale subset; final privacy/SDK audit | Claims about 30 production-ready languages |
| **M6 — Commercial release slice** | Release candidate without research UI; signed distribution route, app-name review, store assets/disclosures, real price experiment and final S0/S1 audit | PC/tablet port, monetized content expansions without evidence |

The gates are **sequential evidence requirements**, not calendar milestones. M1/M2 have not been independently verified. M3/M4/M5/M6 must not be marked complete by a successful .NET test.

**Immediate highest-value action:** use [manual Unity playtest workflow](../../../../.github/workflows/evergrove-unity-playtest.yml) with maintainer-controlled Unity activation. It generates research/development artifacts, **not** signed store releases. The connector cannot supply private Unity license, Android signing or Xcode credentials. Record blockers and do not bypass the gate.

## Research vs consumer build channel

The current research pipeline uses the Unity **Development Build** option, which defines `DEVELOPMENT_BUILD` under Unity 6.3. `EvergroveApp` includes Puzzle Only comparison, 2D/2.5D switch, miniature motion and private playtest-report controls **only under `#if UNITY_EDITOR || DEVELOPMENT_BUILD`**. Ordinary non-development player builds omit those UI branches and force 2D fallback. This is a **source-level guard**, not yet a verified shipping binary; perform `BLD-007` before release.

- **DEV/RESEARCH:** Unity Editor or dedicated Development Build; comparison modes enabled for a *researcher*, never to imply randomized assignment.
- **RELEASE:** separate, non-development Unity pipeline; no exposed mode/visual experiment picker, consent/report UI or hidden testing backdoors. Maintain a separate release-binary inspection.
- The *saved mode* from an installed research build may be `PuzzleOnly`. An ordinary release build hides the switch but cannot truthfully backfill earlier narrative choices. A migration/upgrade test must decide how to handle that edge case **without silently erasing progress or fabricating choices**. No production claim until resolved.
- Existing 2D art remains the default. An old 2.5D preference cannot enable experimental renderer in a non-development build.
- **Do not rely on `DEBUG`** as a production-channel contract: use explicit Unity build options and inspect the final compiled build.

Official source: [Unity 6.3 Development Build scripting define](https://docs.unity.com/en-us/engine/6000.3/manual/building-and-publishing/build-settings/build-profiles-reference).

## Newly integrated implementation, gate statuses unchanged

The repo now contains source-level versions of EG-002 (compact first screen), EG-004 (45 derived board profiles), EG-005 (three stages of hint in v4), EG-008 (visible save failure), EG-011 (noncolor leak cue), and EG-101 (truthful causal journal). The 30 offline translation choices now cover **81 draft runtime keys**. The original **37 features × 46 test contracts** remain the accountability boundary: none of the above has independent Unity/device/human evidence, and M1–M6 are still open.

The `BuildOptions.Development` research pipeline and `UNITY_EDITOR || DEVELOPMENT_BUILD` compile guards must remain intact. Do not let a later merge reintroduce public Puzzle Only, renderer switches or local study export controls into a release build.

## Toolchain reality and release-host caveats

The only checked-in Unity build workflow uses **private maintainer activation** and `BuildOptions.Development`. Android and WebGL targets and an iOS Xcode-project export are **requested** by the script, not verified successful. An Ubuntu-based Unity builder may be unable to produce a usable iOS Xcode project without supported host/platform modules; treat `BLD-003` as **blocked** until actual output and logs prove otherwise. If required, run iOS export on a licensed Mac Unity Editor and signing/build in Xcode. The test is **not passed** merely because the workflow lists `iOS` as an input.


## Test registry and evidence rule

[verification-matrix.json](verification-matrix.json) contains stable IDs under: `BLD-*`, `SAVE-*`, `PUZ-*`, `LOC-*`, `A11Y-*`, `PRIV-*`, `VAL-*`, `WLD-*`, `UX-*`, `TOOL-*`, `PERF-*`, `STORE-*`. Each feature must point to at least one defined test.

For **EG-001**, the mandatory first evidence set is:

| ID | Required proof |
| --- | --- |
| BLD-001 | Unity 6.3 Editor compile/import and bootstrap smoke log |
| BLD-002 | Android APK artifact, run ID, commit SHA and checksum |
| BLD-003 | iOS Xcode project artifact and checksum (not a signed IPA) |
| BLD-004 | Full 12-stream physical Android session |
| BLD-005 | Full 12-stream physical iPhone session |
| BLD-006 | Suspend/resume + force-close/reopen without lost progress |

**Do not manufacture files or placeholder `PASS` records.** `games/evergrove/docs/evidence/<BUILD_SHA>/<FEATURE_ID>/verification.md` is a *future evidence location*, not an existing artifact. The machine-readable file uses `evidence_artifacts: []` and `verified: false` until tests have actually happened. CI requires an existing, linked artifact before any feature/test may be marked verified.

Minimum record, when a real run exists:

```yaml
feature: EG-001
test_id: BLD-004
source_commit_sha: <full SHA of exact tested source>
candidate_build_sha: <full SHA of exact binary>
platform: Android
device_model: <actual model>
os_version: <actual OS version>
unity_version: <actual Unity version>
run_url: <actual GitHub Actions or session record URL>
artifact_sha256: <actual checksum>
tester: <real reviewer role/person>
test_date_utc: <real ISO 8601 UTC time>
result: PASS | FAIL | BLOCKED
observed_behavior: <not a prediction or checklist>
log_or_video_reference: <retained, permissioned evidence>
known_issues: []
```

Keep personal data, identities and raw participant responses out of the public repo; publish anonymized counts or a redacted summary with restricted original research records. Do not store private mobile signing credentials, Unity license files or permissioned device videos here.

## Defect triage

| Severity | Definition | Release decision |
| --- | --- | --- |
| S0 | Irrecoverable valid progress loss, privacy/security incident, cannot launch/progress, blocked accessibility path **for a claimed supported mode** | Must fix; no exception |
| S1 | Major interaction, puzzle, save or localization failure with no reasonable workaround | Must fix; no exception |
| S2 | Material issue with a safe workaround; document the affected audience and follow-up | PO explicitly accepts risk only after user impact is reviewed |
| S3 | Cosmetic issue without meaningful user harm | May defer with owner/revisit trigger |

A release candidate needs **zero open S0/S1**. No broad "perfect" or "accessible" claims without device evidence.

## Gate M4 validity: eliminate the confound

The existing Living World mode selects **rotated/anchored branch boards after Chapter One**; Puzzle Only selects original boards. Therefore existing mode switches are **not a valid causal A/B test of the village**.

`VAL-002` is valid only if both groups have **identical board topology, starting rotations, anchor rules, hint sequence, difficulty settings, device class, text legibility, gameplay exposure and session length**. Only narrative consequence visibility may differ. Store per-condition puzzle hashes with the preregistration **before** observing participants. If these cannot be matched, stop the experiment or label it a non-causal preference observation.

The 8–12-person initial study is **formative**. Do not interpret 8/10 as a precise population percentage; report raw counts and repeated failure patterns. The later 20–30-person matched study is still *directional*, not proof of retention uplift.

## Region and script-specific shipping truth

- Thirty draft locales are in the source; **zero per-locale native/editor/device certification records have been provided**. Use `Draft → Technical QA → Native QA → Store QA → Certified`.
- A production locale is visible **only if Certified**; the raw catalog count is not permission to claim 30 high-quality launch languages.
- `pt-PT` currently routes to `pt-BR`. This is **a documented fallback, not European Portuguese**. Require a sign-off decision to certify that fallback for PT users, add `pt-PT` translation, or explicitly avoid advertising it as supported.
- The first puzzle now identifies endpoints by **blue waves and green sprout drawn from geometry**, rather than Latin S/G markers. This is a code change requiring actual Unity touch/contrast checks, not proof of full screen-reader support.
- Correct RTL/Indic shaping still requires text-stack/font architecture, device screenshot matrix and native review.

## How to approve or reverse a feature

1. Identify the user problem and one observable player change.
2. Determine whether its gate is open and a responsible human owns the feature.
3. Register the affected test IDs and exact acceptance evidence in `verification-matrix.json`.
4. Record the strongest counterargument, cost and stop/rollback trigger **before implementing**.
5. Run C# + static checks, then Unity/device/human tests as applicable; attach build SHA and artifacts.
6. Advance to `Verified` **only** when all required evidence exists and is reviewed.
7. If a gate fails, **defer/reject** adjacent features and choose the cheapest falsifying test. No scope expansion as compensation.

The strongest contrary hypothesis is that **a beautifully finished, ordinary offline water puzzle is better than this puzzle plus a thin village**. The experiment is allowed to reject the living-world layer. User trust, not sunk cost or feature count, is the decision criterion.
