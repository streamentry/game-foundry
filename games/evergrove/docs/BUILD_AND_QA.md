# Evergrove — build, quality gates, reproducibility and release runbook

**Status (2026-11-24):** the pure C# suite passes **39,931 assertions** on .NET SDK 8.0.428; the Python verifier validates **101 keys × 29 offline locale catalogs**. Current `HEAD` is `47ad558274a37c2fd78840be6e4247c5bbecf027`; Unity code last changed at `9be7c2d` with a research-only return diary guarded from release builds. The signed AAB (`1.0.0`, code 1, min SDK 25) and build-1 iOS exports predate current code; do not upload them. The old AAB did launch on an Android 16/API 36 AVD and passed first-loop/airplane-mode checks, but no controlled packet capture exists. A separate unsigned development APK after the engine-link fix rendered on Android 15/API 35, but it predates later source work. The installed Unity CLI cannot register the SSD Editor because its code signature is modified; no build from current `HEAD` exists. The Android upload key remains outside the repository. No store upload or submission has happened. Prior screenshots and clips are archived under `docs/qa/*release-candidate-2026-11-23/`; recapture them from current builds. No physical-phone or 45+ human test, or completed VoiceOver/TalkBack review is verified. This runbook does **not** claim store readiness.

## 1. Toolchain and entrypoints

| Component | Location / version | Purpose |
| --- | --- | --- |
| Unity Editor | `ProjectSettings/ProjectVersion.txt` → `6000.3.0f1` | Actual Unity import, compile, UI, device builds |
| uGUI | `Packages/manifest.json` → `com.unity.ugui 2.0.0` | Interactive UI |
| Input System | `com.unity.inputsystem 1.20.1` | New input touch event handling |
| .NET SDK | 8.x via CI | Pure C# gameplay/locale test runner; **not** a Unity compiler |
| Python | 3.x | Static Unity/locale Resources contract verifier |
| GameCI | `.github/workflows/evergrove-unity-playtest.yml` | Manual licensed preview build |
| Optional cloud | `backend/` TypeScript/Cloudflare Worker + Durable Objects | Separate prototype API, never required to play |

From repository root, run:

```bash
dotnet run --project games/evergrove/Tests/Evergrove.Core.Tests.csproj --configuration Release
python3 tools/verify_evergrove_project.py
```

These commands validate domain logic, 12 source puzzles + 33 branch presentation variants, **200 simulated save-journal interruptions/recoveries**, migration, locale catalogs and structural configuration. They **cannot** inspect real Unity shader imports, UI text shaping, audio, native signing or phone touch behavior.

### Build channels

`EvergroveBuild.Build()` reads `EVERGROVE_BUILD_CHANNEL`:

- `development` (default): `BuildOptions.Development`, research controls enabled, log `EVERGROVE_BUILD_SUCCEEDED <output>`.
- `release-candidate`: no Development flag, product `Mosselora`, id `com.streamentry.evergrove`, version from `EVERGROVE_BUNDLE_VERSION` (default `0.1.0`), research locale catalogs staged out during the build. Android emits `Mosselora.aab` with `buildAppBundle=true` and `EVERGROVE_ANDROID_BUNDLE_VERSION_CODE` (default `1`); iOS emits an Xcode project for device SDK by default or Simulator when `EVERGROVE_IOS_SDK=simulator`, with `CFBundleVersion` from `EVERGROVE_IOS_BUILD_NUMBER` (default `1`). Unity does not configure the Play upload key, iOS signing, or store upload. A locally generated AAB carries Unity's Android Debug signature until the old signing metadata is removed and the protected upload key signs it. Log `EVERGROVE_BUILD_SUCCEEDED release-candidate <output>`.

Headless macOS release candidate (licensed Unity required):

```bash
EVERGROVE_BUILD_CHANNEL=release-candidate EVERGROVE_OUTPUT_ROOT=/abs/private/out \
  "$UNITY_BIN" -batchmode -nographics -projectPath "$PWD/games/evergrove" \
  -buildTarget OSXUniversal -executeMethod Evergrove.UnityClient.Editor.EvergroveBuild.Build \
  -logFile /abs/private/out/rc.log -quit
```

The source guard `python3 tools/verify_evergrove_project.py` fails on any dev-only identifier outside an approved research guard. See [release-candidate audit](qa/release-candidate-audit-2026-11-23.md).

Android Play AAB candidate:

```bash
EVERGROVE_BUILD_CHANNEL=release-candidate \
EVERGROVE_BUNDLE_VERSION=1.0.0 \
EVERGROVE_ANDROID_BUNDLE_VERSION_CODE=1 \
EVERGROVE_OUTPUT_ROOT=/abs/private/out \
  "$UNITY_BIN" -batchmode -nographics -projectPath "$PWD/games/evergrove" \
  -buildTarget Android -executeMethod Evergrove.UnityClient.Editor.EvergroveBuild.Build \
  -logFile /abs/private/out/android-rc.log -quit
```

The generated AAB is **not upload-ready** until the Android Debug signature has been replaced with the Play upload key and verified with `jarsigner` and `bundletool`. Keep the JKS and password out of the repository and chat.

### One-command local licensed preflight

From the repository root of a **clean committed checkout** with Unity 6000.3.0f1 and relevant platform modules installed:

```bash
bash tools/evergrove_unity_local_preflight.sh smoke
bash tools/evergrove_unity_local_preflight.sh Android
bash tools/evergrove_unity_local_preflight.sh iOS
```

The tool accepts `UNITY_BIN=/path/to/Unity` when Unity Hub installs to another path. It records logs, requested version, exact source commit and per-file hashes in ignored `.evergrove-evidence/<SOURCE_SHA>/`; it deliberately does **not** install a license, sign or upload. `EVERGROVE_BUILD_CHANNEL=release-candidate` selects the AAB path for Android and the nondevelopment Xcode project for iOS. If Unity rewrites the tracked `ProjectSettings.asset`, the wrapper captures the exact effective build-input hash and file, then restores only that expected generated setting change. Other tracked input changes still block provenance and are left untouched. It also records permitted untracked Unity-generated `.meta` files and package locks as provisional inputs. Unknown untracked scripts and unexplained tracked changes block the probe. Synthetic fake-Editor tests cover stale artifact reuse, wrong output paths and captured/restored settings; they are **not Unity execution**. Every attempt is isolated under `.evergrove-evidence/<SOURCE_SHA>/<MODE>/run-*/`. GameCI leaves `EVERGROVE_OUTPUT_ROOT` unset and still exports under `build/<platform>/`. Before accepting a local result, check the exact success-log output path and file hashes. A successful import/build is not proof of physical-device behavior or player validation. Follow [M1/M2 execution criteria](specs/12-M1-M2-EXECUTION-CHECKLIST.md) and retain signing material privately.

### Unity Editor manual smoke

1. In **Unity Hub**, open `games/evergrove` using the pinned Unity version and required platform modules.
2. Wait for package import and resolve **all compiler errors**; no “test passed” until compilation succeeds.
3. Menu `Evergrove → Create or Open Playtest Scene`; confirm one empty boot scene. UI is created in runtime C#.
4. Play; test rotating every pipe kind, optional fixed tile, first dry→wet river transition, scene memory after choice.
5. Run `Evergrove → Verify C# Game and Scene` or use the batchmode entrypoint:

```bash
Unity -batchmode -nographics -projectPath games/evergrove \
  -executeMethod Evergrove.UnityClient.Editor.EvergroveSceneSetup.VerifyProject \
  -logFile - -quit
```

Adjust executable path to your installed Unity binary; **not run/verified here**.

## 2. Manual GitHub Actions playtest export

Source: `.github/workflows/evergrove-unity-playtest.yml` and `Assets/Editor/EvergroveBuild.cs`. The workflow is **manual** via `workflow_dispatch`, never a public store release.

- Configure repository-owned Unity license in GitHub **Actions secrets**, not this documentation. GameCI Personal expects `UNITY_LICENSE`; appropriate Pro setups use `UNITY_SERIAL`, `UNITY_EMAIL` and `UNITY_PASSWORD`. Verify with GameCI's current activation documentation.
- Choose **Android**, **WebGL** or **iOS**; GameCI invokes `Evergrove.UnityClient.Editor.EvergroveBuild.Build` after Unity opens.
- Android target produces a **test APK**, not a signed Play Store listing. WebGL produces an **unhosted output folder**. iOS is intended as an **unsigned Xcode project** requiring Mac/Xcode and personal signing to run; **its Linux runner feasibility has not been independently proven**.
- Preview artifact retention is **7 days**, not permanent distribution. Inspect the actual workflow and output paths rather than assuming success.
- Workflow source avoids silently deploying Cloudflare, generating accounts or submitting store applications.

For exact M1/M2 pass criteria, commands, retained logs and phone evidence, use [the execution checklist](specs/12-M1-M2-EXECUTION-CHECKLIST.md). A correct manifest pin or artifact marker is not proof that the Editor and phone launch successfully.

**Potential build risks to test:** compatibility of Unity 6.3 with the pinned packages; platform modules and license activation; whether GameCI output directory corresponds to the custom `build/<platform>` location; iOS export support and host OS; import/packaging of Resources locale catalogs; missing font/renderer APIs.

## 3. Automated test map

| Suite / command | Current scope | Does not prove |
| --- | --- | --- |
| `dotnet run --project games/evergrove/Tests/Evergrove.Core.Tests.csproj --configuration Release` | Fixture shape, exhaustive uniqueness, leaks/connectivity, 33 branch variants, immutable landmarks, campaign completion, Hint/Undo/Restart/Choice/Reconsider, save v1/v2/v3 rules, scene flags and local playtest counters; catalog validation for 29 non-English languages | Unity UI compile, gameplay feel, device performance, bilingual rendering |
| `python3 tools/verify_evergrove_project.py` | Unity manifest, UI input guards, generated build entrypoint, Resources bundles, 101 keys each, locale code and placeholders, opt-in playtest contract | True RTL shaping, Unity engine import, native font availability |
| `npm test` under `games/evergrove/backend` | Separate optional Worker model API behavior | Any linkage to actual offline game; cloud security or billing |
| `.github/workflows/ci.yml` | Repo-wide research tooling and unrelated Godot game jobs | Unity-native correctness |

**Important:** On a future source edit, record a **link to the exact CI run**, commit SHA and assertion count in the PR. Do not assume the last successful run applies to a new commit or cite an old number as current.

## 4. Functional acceptance: test complete player journeys

| ID | Test | Expected observation |
| --- | --- | --- |
| EG-01 | Fresh install, Chapter 1, no prior save | Dry riverbed; 3×3 puzzle with wave/sprout endpoint icons; default 2D; sound/motion off; no account prompt |
| EG-02 | Tap each kind (corner, straight, endpoint) | Correct clockwise rotation/port; live water reach and leak count; no false “solved” |
| EG-03 | Solve Chapter 1 | Water visibly returns to village; exactly three choice actions; no timed paywall |
| EG-04 | Choose tree | Next chapter uses variant 1 (90° rotated); one amber locked root connection; Mira acknowledges tree |
| EG-05 | Alternate save choosing crossing | Variant 2 (180° rotated), locked foundation; persistent bridge/crossing |
| EG-06 | Alternate save choosing flowers | Variant 3 (270° rotated), locked sanctuary; meadow and Fen reaction |
| EG-07 | Tap anchored tile | No rotation; still clearly labeled as intentional protection |
| EG-08 | Hint then Undo then Restart in branched chapter | Same variant and anchor retained; no duplicated choices; no hint refund |
| EG-09 | Reconsider immediately after choice | Reopen **exact previous solved variant**; recommit alternate choice; no re-solving |
| EG-10 | All 12 chapters in Living | 12 choices recorded; coherent multiple ending types; no unintended reset |
| EG-11 | PuzzleOnly mode | Original authored puzzles throughout; no village art/branch rule/choice, no fabricated NPC memories |
| EG-12 | 2D ↔ 2.5D | Same history and facts; default still; gracefully return to 2D if rendering unsupported |
| EG-13 | Kill/relaunch after move and choice | Rotations, variant, history survive or explicit save error surfaced; no inconsistent character |
| EG-14 | v1 six-chapter save, v2 mid-chapter save | Six choices remain; old active board not silently transformed; new branch rules only after the next eligible choice; no rewrite of saved active board |
| EG-15 | Corrupt primary, valid backup | Recover valid prior garden without poisoning backup; log potential loss of the latest move |
| EG-16 | Change locale while in a branched board | Game rule, orientation, prior choices unchanged; 101 keys load correctly |
| EG-17 | 320px phone + tablets + cutout devices | No clipped/out-of-view actionable puzzle, controls, wave/sprout endpoint icons or choices |
| EG-18 | VoiceOver/TalkBack, keyboard and RTL/Indic | Accessible names, focus order and shaped text actually correct; **currently unverified** |
| EG-19 | In-game local playtest report | Zero counters without opt-in; copy only when tapped; no network/identifiers |
| EG-20 | Disk full / unavailable storage | Warning appears; no save success is claimed. Corrupt-all-slots blocks writes until explicit reset. Verify on phones |

**Cross-platform matrix:** at least one small/older Android phone, one recent Android, one small iPhone, one large iPhone, relevant tablet, and WebGL when chosen as distribution. Capture OS build/version, device class, repro steps, screenshot, memory and frame-time evidence. Avoid invented “60 FPS on all phones” claims.

## 5. Additional non-color and journal regression

- [ ] Every unmatched/open connection marks a tile with !; completed puzzles have no false exclamation. Verify the result with grayscale, magnification and a device screen reader.
- [ ] The optional journal contains a real player choice and scene effect. It may name a current puzzle constraint only if the saved variant proves it, never a fabricated past effect for migrated older saves.
- [ ] Expanded journal rows and translated hint notices remain readable at 320px and through font scaling.

## 6. Visual and language quality gate

For each launch language:

- Native speaker approves all 81 compact keys, actual runtime combinations and tone.
- Screenshot test: language picker, board, ecological rule, decision cards, conversations, journal, end screen.
- Correct font shaping, bidi order, line breaks and numerical placeholders, especially Arabic/Persian/Hebrew/Urdu, Hindi/Bengali/Tamil/Telugu and Thai.
- Focus order, readable contrast, large touch targets and safe-area handling on small phones.
- Do not enable a catalog merely because its JSON parses. The default English copy is also an editorial draft until tested with players.

For the 2.5D art option: compare same world state with 2D, record memory/CPU/GPU/thermal cost and exact visual benefits. If it is slower or no clearer, keep 2D.

## 7. Release gating and rollback

**Gate A: logic.** All .NET branch, save and locale tests pass; no unexplained changes to existing 12 fixtures.

**Gate B: actual build.** Unity Editor compilation, scene launch, Android test APK or WebGL player runs; iOS local Xcode export/signing tested if targeting iOS.

**Gate C: basic trustworthy play.** EG-01 through EG-20 relevant cases pass on target devices; privacy/security and accessibility issues triaged; progress can't silently disappear.

**Gate D: real player comprehension.** At least one small uncoached study with real target players; log failures and corrective actions.

**Gate E: commercial decision.** Evidence of voluntary return, clear reason to choose over a polished competitor, plausible first distribution channel, game title cleared, ethical monetization strategy. Only this gate authorizes production/marketing claims.

**Rollback path:** Keep prior git commit/tag and compatible local save fixtures; if later Unity build is broken, revert the code while noting that v3 save may not be readable by an older binary. Do not roll back gameplay without a save-format compatibility check. Do not delete player data or publish cloud changes to solve a rendering issue.

## 8. Who signs off?

- **Engineering:** reproducible Unity build and tests, battery/memory/crash/performance logs.
- **Game design:** mechanics are understandable and consequences causal, not deceptive.
- **Content/localization:** native language + accessibility QA for **each** released locale.
- **Product/research:** actual user quotes and cohort behavior, not internal enthusiasm.
- **Founder:** distribution/pricing/brand risks and explicit continue/simplify/stop decision.

**Single highest-leverage next real action:** obtain a Unity-compiled playable build and watch an uncoached target player reach chapter 2, then ask what the changed tile means.
