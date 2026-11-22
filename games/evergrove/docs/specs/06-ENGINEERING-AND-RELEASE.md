# 06 — Engineering contracts, QA and release gates

**Reality check:** current repository contains a Unity 6.3/C# playtest project, domain tests and a license-gated build workflow. It does **not** contain independently verified Unity Editor compilation, signed store binaries or physical-device QA results. Do not turn a specification or passing .NET test into a shipping claim.

## Source of truth and architecture

| Responsibility | Existing source | Contract |
| --- | --- | --- |
| Puzzle topology, ports, connectivity, exhaustive solver | [PipeRules.cs](../../Assets/Scripts/Core/PipeRules.cs) | Deterministic, Unity-free logic; no hidden random answer |
| 12 authored puzzles and 33 choice variants | [PipeRules.cs](../../Assets/Scripts/Core/PipeRules.cs), [ReactiveWatercourses.cs](../../Assets/Scripts/Core/ReactiveWatercourses.cs) | Variant derived from previous saved choice; anchored tile consistent |
| Event history, current board, v4 snapshot and v1/v2/v3 migration | [WorldModel.cs](../../Assets/Scripts/Core/WorldModel.cs) | One authoritative history; no double choice or forged save |
| Story and localized state-dependent text | [VillageStory.cs](../../Assets/Scripts/Core/VillageStory.cs), [LocalizedVillage.cs](../../Assets/Scripts/Core/LocalizedVillage.cs) | NPCs never invent unsaved events |
| Pure scene state | [VillageScenePlan.cs](../../Assets/Scripts/Core/VillageScenePlan.cs) | Both renderers show identical world facts |
| UI, input, sound and local playtest opt-in | [EvergroveApp.cs](../../Assets/Scripts/Unity/EvergroveApp.cs) | Real touch, safe area, language switching, optional controls |
| 2D illustration and 2.5D experiment | [VillageIllustration.cs](../../Assets/Scripts/Unity/VillageIllustration.cs), [VillageDiorama.cs](../../Assets/Scripts/Unity/VillageDiorama.cs) | 2D baseline/fallback; 2.5D not mandatory |
| Offline save | [WorldStore.cs](../../Assets/Scripts/Unity/WorldStore.cs) | Validated JSON, backup, migration, recovery |
| Language selection and fonts | [LocaleRuntime.cs](../../Assets/Scripts/Unity/LocaleRuntime.cs), [LOCALIZATION.md](../../LOCALIZATION.md) | Locale changes cannot alter world; current shaping is not certified |
| Consent-first local study counters | [LocalPlaytest.cs](../../Assets/Scripts/Core/LocalPlaytest.cs) | In memory, no identifiers or automatic upload |
| Domain regression | [Tests/Program.cs](../../Tests/Program.cs) | Pure-C# correctness; **not** Unity/device coverage |

**Architecture rule:** domain model is authoritative. Renderer, language, haptic/sound, test mode and future store entitlement must not rewrite ecology choices or solution history. No backend dependency for core play. Optional Cloudflare preview is not connected to game state.

## Package/Editor compatibility — first verified static blocker

**2026-11-22 source fix:** `Packages/manifest.json` previously pinned `com.unity.ugui@2.6.0` while `ProjectSettings/ProjectVersion.txt` requested **Unity 6000.3.0f1**. Unity documents uGUI as an **editor-matched core package**, not an arbitrary package upgrade. Unity 6.3 uses **uGUI 2.0.0**; the uGUI 2.6.0 upgrade is associated with Unity 6.6. The manifest now pins **2.0.0**; `tools/verify_evergrove_project.py` rejects a regression. The separate `com.unity.inputsystem@1.20.1` is documented as released for Unity 6.3 and is retained. This is **source compatibility repair**, not evidence Unity imports successfully.

Authoritative sources: [Unity 6.3 uGUI core package](https://docs.unity.com/en-us/engine/6000.3/manual/packages-list/packages-all/pack-core/com-unity-ugui), [Unity 6.6 release notes](https://unity.com/releases/editor/whats-new/6000.6.0f1), [Unity 6.3 Input System release list](https://docs.unity.com/en-us/engine/6000.3/manual/packages-list/packages-all/pack-safe/com-unity-inputsystem).

## Build/evidence separation

Follow [11 — Release packets](11-EVIDENCE-AND-RELEASE-PACKETS.md) for exact source SHA vs binary hash, research/consumer build-channel audit, signature and platform evidence. **Do not confuse a green C# domain job with a verified Unity import, Android APK installed on a phone or iOS Xcode export.** The manual Ubuntu workflow exposes iOS as an option; the export host/toolchain must be verified by a real successful run. All current device/build evidence stays unverified until supplied.

## Build ladder — every step records evidence

| Gate | Procedure | Passing evidence |
| --- | --- | --- |
| 1. Static/catalog | `python3 tools/verify_evergrove_project.py` | Script output, exit code, commit SHA |
| 2. Pure C# | `dotnet run --project games/evergrove/Tests/Evergrove.Core.Tests.csproj --configuration Release` | All puzzle/variant/save/localization assertions pass |
| 3. Unity Editor | Open project in **Unity 6.3 LTS**, resolve packages, run `Evergrove → Verify C# Game and Scene` | Editor compile/import log, no exceptions |
| 4. CI Unity build | Run [manual GameCI workflow](../../../../.github/workflows/evergrove-unity-playtest.yml) with privately configured Unity license | Actual Android APK/WebGL ZIP/iOS Xcode project artifacts; never claim signed IPA |
| 5. Physical devices | Android touch APK + signed iOS app built from Xcode project | Cold launch, all 12 streams, choices, journal, safe area, language, resume |
| 6. Release readiness | Accessibility, localization, privacy, performance, store/legal and commercial gates | Device matrix, screenshots, QA sign-offs, decision record |

**No test was executed by this documentation change.** A Unity license and iOS signing are maintainer responsibilities; do not place secrets in repository or chat. See [BUILD_QA.md](../../BUILD_QA.md).

## Exact build, test and artifact contract

**First action:** licensed Unity 6.3 import, scene bootstrap and actual Android APK/iOS Xcode-project builds via the [manual workflow](../../../../.github/workflows/evergrove-unity-playtest.yml) (`BLD-001..003`). The existing public GitHub Actions static verifier and .NET suite are **not** Unity compilation. No real-device or Unity-build artifact was verified in the audit baseline; if a new run occurs, record it explicitly instead of preserving a stale claim.

**Test ladder:** pure C# rules/migrations/catalogs → Unity Editor import/compile → scene bootstrap/UI/text/font asset smoke → Unity Test Framework EditMode/PlayMode checks when implemented → physical Android/iPhone full chapter flows → save corruption/interruption → release-binary network audit → representative human playtest. A successful GameCI build is a **builder result**, not a UI/end-to-end test suite.

**Research channel:** `EvergroveBuild.Build` now explicitly requests `BuildOptions.Development`. Unity's `DEVELOPMENT_BUILD` compile symbol gates experiment mode, 2D/2.5D comparison, miniature motion and local research export controls in `EvergroveApp`. A future store build must use a **separate non-development pipeline** with `BLD-007` static and runtime verification. Do not sign/upload a GameCI playtest APK as a consumer release.

**Save filename hazard:** the persistent filename is `evergrove-world-v2.json` but the **WorldSnapshot schema is v4**. The filename is a historical storage identifier; **do not rename it merely because the schema increments**, without explicit file migration and regression tests.

### Candidate build evidence record

```text
SourceCommitSha: <immutable source SHA>
BuildSha: <hash of exact artifact>
UnityVersion: <actual>
PlatformTarget: Android | iOS | WebGL
WorkflowRunUrl: <actual>
ArtifactSha256: <actual>
BuildTimestampUtc: <actual>
DeviceModelAndOs: <actual>
Tester: <real role/name, private when necessary>
TestPlanVersion: <actual>
PassedTestIds: []
FailedTestIds: []
KnownIssues: []
NetworkAuditArtifact: <actual or NONE>
SaveFailureAuditArtifact: <actual or NONE>
Decision: BLOCKED | REWORK | GO
```

No placeholder in this template is evidence. Only a real record tied to the source/build SHA can move a feature to `Verified`. See [execution controls](09-EXECUTION-CONTROL.md) and [46 named test contracts](verification-matrix.json).

**New source-only data contracts:** snapshot version `4` persists staged help (`hintStep` and `hintTile`) and migrates versions 1, 2 and 3 without changing existing boards. The GUI now warns about a failed disk write; **this is not evidence of power-loss or forced-kill durability**. `SAVE-001/002/003` must still pass on devices.

**Pure C# metadata guard:** `dotnet run --project games/evergrove/Tests/Evergrove.Core.Tests.csproj --configuration Release -- --check-puzzle-catalog` must reproduce all 45 authored/choice-conditioned profiles from deterministic code. This proves catalog/source parity, **not experienced difficulty**.

## Save integrity (EG-008)

Current `WorldStore` writes `evergrove-world-v2.json` (filename is v2; **snapshot schema is v4**), validates JSON via `WorldModel.TryRestore`, writes a temporary file, attempts replacement with backup and has a copy-based fallback. It can recover a previous valid backup and migrate old PlayerPrefs v1 data. **This is an implemented strategy, not a verified atomic guarantee on every mobile filesystem.**

Required fault-injection plan:
1. Make one rotation, save, terminate; reopen and compare board, variant and hints.
2. Solve, choose tree/bridge/flowers, terminate at each write stage; verify exact event history and derived scene.
3. Reconsider latest choice, force-close, reopen; no phantom choice.
4. Corrupt/truncate primary file; verify backup recovery and explicit warning rather than silently resetting.
5. Migrate representative v1 and v2 snapshots; verify completed/in-progress legacy boards.
6. Change language/renderer, force-close; world snapshot must remain logically identical.
7. Repeat across Android/iOS (target **100 interruption points** when harness permits); record actual sample count and failures.

**Acceptance:** zero unrecoverable valid progress in the tested matrix; unknown/corrupt snapshots fail closed; destructive reset requires explicit confirmation. If fallback copy is not interruption-safe on a platform, redesign persistence before release.

## Offline, privacy and observability (EG-009)

- Production play works with airplane mode on; no ad SDK, forced login, server-time dependence, cloud save requirement or unexpected request.
- The existing local playtest is **opt-in and in-memory**. It counts first turn/solve, rotations, hints, undo, choices, conversations and visual switches; it does not record names, device IDs, IP, location or passive D1/D7.
- Export occurs **only** by deliberate player copy action. Consent can be withdrawn/discarded; do not quietly add background collection.
- For validation, a researcher separately asks for voluntary D1/D7 follow-up with consent and records raw denominators. A local counter is not an analytics cohort.
- Before release inspect build dependencies and actual traffic; prepare accurate Apple App Privacy and Google Play Data Safety disclosures. Do not equate "no analytics SDK" with "zero traffic" until measured.

## Performance and rendering

- 2D is the default baseline and fallback. 2.5D uses a fixed orthographic camera and a bounded RenderTexture; its optional animation defaults off.
- Benchmark on compact/older Android and iPhone: cold start, median/p95 input latency and frame time, memory peaks, GPU/render texture cost, battery/heat, APK size, locale switch and crash behavior.
- **Set numeric FPS/memory/size budgets after measuring target devices**, not by inventing figures. Fail the 2.5D option if it harms legibility, stability or affordable-device performance.
- No input should depend on 3D scene success. When miniature rendering fails, fall back to a usable 2D world without losing save progress.

## Regression matrix by subsystem

| Path | Must exercise |
| --- | --- |
| Puzzle | 12 fixtures + 33 variants, unique solutions, hint/undo/restart, protected landmarks |
| World | All choice combinations, deterministic journal/NPC/ending, reconsideration, save/reload |
| UI | Tap hit zones, scroll conflict, safe areas, Android back, large text, screen reader, 2D fallback |
| Localization | 30 draft catalogs validated; only certified subset enabled; language change at puzzle/choice/ending |
| Audio/motion | Silent default, optional chime, haptic off, reduced motion, app resume |
| Data | Corrupt file, backup, v1/v2 migration, airplane mode, network audit, privacy disclosures |
| Store | Name/trademark check, honest screenshots, supported device/orientation, signed binaries |

## Evidence record template

```text
Build commit:
Unity version / package lock:
Build target and artifact hash:
Device / OS / viewport / language:
Tester and date:
Test path and steps:
Observed result (pass/fail, not "seems fine"):
Logs/screenshots/video:
Known blocker:
Next owner/action:
```

A checkbox is not evidence until filled with an actual result. Release decisions must cite these records, not a proposed plan.

## Non-negotiable shipping blockers

No independently verified Unity/device build; lost valid save data; unusable touch targets; incorrectly rendered **enabled** language; untruthful privacy claims; critical accessibility path failures without a documented scope decision; misleading app name/store claim; unsolved legal/entitlement issues for a paid release. Audio/haptics/postcards are **not** allowed to delay these blockers or disguise them.
