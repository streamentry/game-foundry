# 12 — M1/M2 operator checklist: from source to two real phones

**Status as of 2026-11-22: NOT EXECUTED here.** The corrected Unity package manifest and a green ordinary CI job are **static/source evidence**, not successful Unity imports, APK/Xcode outputs or physical-device observations. This document is an operator procedure, not a release certificate.

**Goal:** retire the first two blockers before adding any Evergrove mechanic, art pipeline, locale launch or retention feature. The canonical test IDs and ownership remain in [09 — Execution control](09-EXECUTION-CONTROL.md) and [verification-matrix.json](verification-matrix.json). The artifact semantics, privacy handling and sign-off stay in [11 — Evidence packets](11-EVIDENCE-AND-RELEASE-PACKETS.md).

## 0. Freeze what is actually being tested

- [ ] Assign a **real** Unity/Release Engineer and device QA reviewer. Confirm access to a legal Unity 6.3 license, relevant platform build modules, Xcode/signing access on a Mac and two **physical** phones. Do not put credentials in GitHub issues, chat or this repository.
- [ ] Record the **40-character source commit SHA** from the exact checkout, repository URL, UTC time, platform, target OS, device model, reviewer and build channel. Never substitute the historical audit SHA or a later `main` commit for the tested source.
- [ ] Record `ProjectSettings/ProjectVersion.txt` and `Packages/manifest.json` from this SHA. Current source requests Unity `6000.3.0f1`, uGUI `2.0.0`, Input System `1.20.1`. A compatible manifest is only a prerequisite; the installed Editor version must be checked **in its log**.
- [ ] From repository root run `python3 tools/verify_evergrove_project.py`, `python3 tools/check_evergrove_docs.py` and `dotnet run --project games/evergrove/Tests/Evergrove.Core.Tests.csproj --configuration Release`. Record exit codes and logs. These do **not** satisfy BLD-001.

**Stop if:** source is dirty/ambiguous, package resolution fails, ownership or license is missing, or a deterministic test fails. Label the real problem; don't declare M1 passed.

## 1. BLD-001 — licensed Editor import and scene smoke

On a maintainer Mac with the exact Unity 6.3 Editor version installed, run from the repository root (adjust only the executable path):

```bash
UNITY_BIN="/Applications/Unity/Hub/Editor/6000.3.0f1/Unity.app/Contents/MacOS/Unity"
"$UNITY_BIN" -batchmode -nographics -quit \
  -projectPath "$PWD/games/evergrove" \
  -executeMethod Evergrove.UnityClient.Editor.EvergroveSceneSetup.VerifyProject \
  -logFile "$PWD/evergrove-unity-smoke.log"
```

- [ ] Confirm installed Editor version, package resolution and **zero compiler errors** in the complete log; verify scene creation and 12-puzzle smoke message. Record the exact command, shell exit status and log checksum.
- [ ] In the graphical Editor, open the created scene, press Play, and verify at least one real interaction. Batchmode method alone verifies bootstrap/domain integration, **not** rendering, focus, native input or 12-chapter gameplay.
- [ ] Keep generated logs, scene outputs and credentials out of unrelated source commits. If the Editor generates new project files, review and explicitly classify them rather than silently staging everything.

**BLD-001 is PASS only if the actual Editor imported and compiled.** An expected log string copied into docs is not evidence.

## 2. BLD-002 — unsigned Android research APK

- [ ] Configure Unity activation privately in GitHub **Settings → Secrets and variables → Actions**, following current [GameCI activation guidance](https://game.ci/docs/github/activation/). The workflow is intentionally manual and does not publish.
- [ ] On the exact candidate SHA, run [Build Evergrove Unity Playtest](../../../../.github/workflows/evergrove-unity-playtest.yml) with `platform=Android`. The workflow uses Ubuntu and requests a Unity **Development Build**.
- [ ] Inspect the actual runner log, not just the workflow's green status. Download `evergrove-Android-playtest`; require a nonempty `Evergrove-Playtest.apk` and `evergrove-build-provenance.json`.
- [ ] Check `source_commit_sha`, workflow run URL, requested-versus-actual Editor version, and per-file SHA-256 in the generated provenance. The manifest's payload-tree hash is **not** the ZIP checksum or a signing receipt.
- [ ] Independently hash the downloaded APK (on macOS: `shasum -a 256 Evergrove-Playtest.apk`); record this digest, artifact retention location and build channel `research`. Store raw private logs securely beyond the workflow's seven-day artifact retention.
- [ ] If the export path, manifest marker or Android installation fails, record **FAIL** with the log and do not move to M2. A generated APK is **not** proof of phone UX.

## 3. BLD-003 — unsigned iOS Xcode project, not an IPA

The manual workflow lists iOS but its Ubuntu host/toolchain capability is **unverified**. Do not call the iOS gate complete merely because iOS is offered in the dropdown.

- [ ] First attempt a supported Unity iOS export and capture the actual Editor/platform-module log and output. If Ubuntu cannot export the project, record the concrete failure, then use a licensed Unity 6.3 Editor on a supported Mac.
- [ ] For a local macOS export after the BLD-001 smoke, use the exact checked-out source:
  
```bash
"$UNITY_BIN" -batchmode -nographics -quit \
  -projectPath "$PWD/games/evergrove" -buildTarget iOS \
  -executeMethod Evergrove.UnityClient.Editor.EvergroveBuild.Build \
  -logFile "$PWD/evergrove-ios-export.log"
```

- [ ] Inspect `build/iOS/Evergrove-Xcode/Unity-iPhone.xcodeproj/project.pbxproj` and all generated bytes; hash a retained archive/project manifest, record Editor log, source SHA and export host. **Do not** substitute the manifest tree hash for a signed IPA checksum.
- [ ] Open the Xcode project on the Mac, configure legitimate local provisioning and build/install **separately**. A successful Unity Xcode export is M1 BLD-003; a signed, installed phone app is later **M2** evidence. If credentials or hardware are unavailable, mark the corresponding stage **BLOCKED**, never PASS.

## 4. M2 — real Android + iPhone campaign, one observed record per test

**M2 cannot be inferred from M1.** Use the same source SHA (with platform-appropriate compiled binaries), document OS/version, locale, orientation, safe areas, build channel and per-device observations. Start with one compact Android and one physical iPhone; add older/large devices before wider release.

| Test ID | Run on device | Required observation / failure record |
| --- | --- | --- |
| `BLD-004` / `BLD-005` | Fresh install; finish **all 12 streams** on Android and iPhone separately, including a choice each chapter | Actual 12/12 progress per device, scene/goal/choice visible, no S0/S1; capture chapter-specific defects |
| `BLD-006` | Suspend, resume, OS back, force-close just after rotation and just after ecology choice | Restored exact board/choices, warning if persistence fails, no duplicate event |
| `SAVE-001` | With a **backed-up test-only save**, corrupt primary but preserve valid backup | Recovery of exact prior valid state; log expected vs observed; no production-user files touched |
| `SAVE-002` | Inject interruption points through write/backup/rename lifecycle | Pre-registered **100 attempts**, all outcomes logged; do not report 100/100 without 100 observed results |
| `A11Y-001` / `A11Y-002` | Inspect actual hit rectangles, grayscale icons, leak/anchor cues, compact safe areas | ≥44 pt iOS / ≥48 dp Android frequent targets, water path distinguishable without hue |
| `PUZ-007` / `SAVE-004` | Icons + language/renderer changes on each phone | No S/G text dependence; world/history/solution unchanged |
| `PRIV-001` / `PRIV-002` | Offline/airplane mode, then controlled network capture, local report consent toggles | No unaccounted traffic; opt-in counters/copy only; declare exactly what was inspected |

**Pure C# source test:** `SaveJournalTests.Run()` now exercises backup/staged recovery and 100 deterministic truncation cases in ordinary C# CI. This does **not** close `SAVE-001` or `SAVE-002`: those require native Unity save/restore, actual interruption points and hardware/file-system observations.

**All 12 streams matter:** a test that sees only the first board is not `BLD-004` or `BLD-005`. For `SAVE-002`, record attempted/valid/recovered/lost denominators by injection stage. No fault-injection harness or 100-observation dataset has yet been supplied; keep it open rather than assuming 100/100 reliability.

## 5. Log a real result and decide

- [ ] For each executed ID write **observed**, not predicted, steps/results. Use `games/evergrove/docs/evidence/<SOURCE_SHA>/<FEATURE_ID>/verification.md`; for milestone-only tests, `.../<SOURCE_SHA>/_gates/<TEST_ID>/verification.md`. Store only **redacted references**, not secrets, identifying research media or signing material in a public repo.
- [ ] For a future `verified: true` test populate `source_commit_sha`, `build_artifact_sha256` (required for Unity/device/human/manual; source-only automation may omit), `reviewer_role`, `reviewed_at_utc` and `evidence_path` in [verification-matrix.json](verification-matrix.json). The file must record `test_id`, source SHA, `result: PASS` and the matching hash if applicable. A feature becomes `Verified` only after **all** its linked tests pass and are reviewed.
- [ ] Report every **FAIL / BLOCKED / NOT_RUN**, with reproducible step, severity, next owner/action, and actual denominator. **S0** (lost valid progress, privacy breach, cannot launch) and **S1** (major unusable input, impossible puzzle, mandatory locale unreadable) stop deployment; no exception.
- [ ] Have the real release engineer and product/QA reviewer sign one scoped `GO / CONDITIONAL_RESEARCH_ONLY / NO_GO` decision in [11 — Evidence packets](11-EVIDENCE-AND-RELEASE-PACKETS.md). Passing M2 authorizes only the next user study, **not** a public store release.

**M1 exit:** BLD-001, BLD-002 and BLD-003 have actual Unity build/import/export evidence. **M2 exit:** BLD-004–006 plus applicable SAVE/A11Y/PRIV/gate-wide tests pass on both phone families with **zero open S0/S1**. If either gate fails, fix the observed blocker before developing another puzzle or village feature.
