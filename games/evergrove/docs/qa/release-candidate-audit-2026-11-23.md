# Release-candidate dev-control audit (BLD-007, 2026-11-23)

Scope: gate M5, "Nondevelopment player has no experiment modes, visual comparison or playtest export controls". This audit covers the source guards, a headless macOS release-candidate build, and a binary string scan. It is **not** a device test.

## Verdict

**Source guards: pass after fixes. Binary scan: pass for every listed dev identifier. Screenshot evidence: pending lead capture.**

The M5 gate is not closed until the lead captures an iOS-simulator release-candidate screenshot showing no comparison, mode, miniature, or playtest controls, and that screenshot is attached to BLD-007.

## Build channel

`EVERGROVE_BUILD_CHANNEL` selects the channel in `Assets/Editor/EvergroveBuild.cs`.

| Channel | BuildOptions | productName | Application id | Version | Log marker |
| --- | --- | --- | --- | --- | --- |
| `development` (default, unchanged) | `Development` | Mosselora Playtest | `com.streamentry.evergrove.playtest` | 0.1.0 | `EVERGROVE_BUILD_SUCCEEDED <output>` |
| `release-candidate` | `None` | Mosselora | `com.streamentry.evergrove` | `EVERGROVE_BUNDLE_VERSION` or 0.1.0 | `EVERGROVE_BUILD_SUCCEEDED release-candidate <output>` |

The development marker is unchanged on purpose. `tools/evergrove_unity_local_preflight.sh` matches `EVERGROVE_BUILD_SUCCEEDED $OUTPUT` exactly.

Release-candidate builds also move the 29 research catalogs (`*.json` and `*.json.meta`) out of `Assets/Resources/Evergrove/Locales` into `<EVERGROVE_OUTPUT_ROOT>/rc-locale-staging` before `BuildPlayer`. A `finally` block restores them. English has no catalog file, and `LocaleShippingPolicy` allows English only. Signing, upload and store submission are not configured.

## Guard table

Guard tokens: `U+D` means `#if UNITY_EDITOR || DEVELOPMENT_BUILD`. `U+D+R` means the same token plus `|| EVERGROVE_RESEARCH_TOOLS`, used only by the test project.

| Control or class | File | Guard | Release state | Status |
| --- | --- | --- | --- | --- |
| DevQaTour (scripted QA tour) | Unity/DevQaTour.cs | whole file U+D | absent | was guarded |
| DevScreenshot (screenshot hook) | Unity/DevScreenshot.cs | whole file U+D | absent | **fixed**: was runtime-gated only |
| TextSpike (text spike) | Unity/Dev/TextSpike.cs | whole file U+D | absent | **fixed**: was runtime-gated only |
| VillageDiorama (2.5D miniature) | Unity/VillageDiorama.cs | whole file U+D | absent | **fixed**: compiled into release |
| Miniature field and state (`showMiniature`, `animateMiniature`, `diorama`, `ReducedMotionKey`) | Unity/EvergroveApp.cs | U+D | absent | **fixed**: unguarded fields |
| Living art branch that renders the miniature | Unity/EvergroveApp.cs | U+D around branch; release label "Living storybook art" | 2D art only | **fixed** |
| Diorama visual comparison and miniature-motion buttons | Unity/EvergroveApp.cs | U+D (block 728-793) | absent | was guarded |
| Living / Puzzle control mode switch (`SwitchMode`) | Unity/EvergroveApp.cs | U+D | absent | was guarded |
| Matched control entry, schedule input, confirm, copy schedule | Unity/EvergroveApp.cs | U+D (fields, block, methods) | absent | was guarded |
| LocalPlaytest recorder (class) | Core/LocalPlaytest.cs | whole file U+D+R | absent | **fixed**: compiled into release |
| LocalPlaytest consent, copy and report buttons | Unity/EvergroveApp.cs | U+D | absent | was guarded |
| LocalPlaytest call sites (Reconsider, Solve, Talk, Hint, Rotate, Act switch, Choose) | Unity/EvergroveApp.cs | U+D each | no calls | **fixed**: 7 unguarded sites |
| Dev text-scale override (`-mosseloraTextScale`) | Unity/TextScale.cs | U+D | absent | **fixed**: const string was unguarded |
| ScreenReaderBridge.LogForQa | Unity/ScreenReaderBridge.cs | U+D | absent | was guarded |
| EvergroveApp QA seam (`QaWorld`, `QaReplaceWorld`) | Unity/EvergroveApp.cs | U+D | absent | was guarded |
| Save bypass for QA tour (`qaInMemoryOnly`) | Unity/EvergroveApp.cs | U+D | absent | was guarded |
| Research locales (`ResearchBuild` true) | Unity/LocaleRuntime.cs, Core/LocaleShippingPolicy.cs | U+D; release = English only | only English selectable | was guarded; catalogs now staged out |
| Store-build `showMiniature = false` assignments | Unity/EvergroveApp.cs | removed (fields compiled out) | n/a | **fixed** |
| Matched exception text and doc comment | Core/WorldModel.cs | not guarded (domain code) | string reworded | **fixed**: "Matched control" wording removed |

The static guard in `tools/verify_evergrove_project.py` now fails on any dev-only identifier in `Assets/**/*.cs` that is outside an approved guard. The approved guards are exactly `#if UNITY_EDITOR || DEVELOPMENT_BUILD` and `#if UNITY_EDITOR || DEVELOPMENT_BUILD || EVERGROVE_RESEARCH_TOOLS`. The guard checks only the `#if` branch, not the `#else` branch. The scanner has a self-test on an inline sample.

## Commands and results

```
DOTNET_ROOT=/Volumes/SSD/dotnet DOTNET_CLI_TELEMETRY_OPTOUT=1 /Volumes/SSD/dotnet/dotnet run --project games/evergrove/Tests/Evergrove.Core.Tests.csproj --configuration Release
  -> Evergrove C# tests passed: 39466 assertions

python3 tools/verify_evergrove_project.py
  -> ... guard: OK (no unguarded dev-only identifiers)

EVERGROVE_BUILD_CHANNEL=release-candidate EVERGROVE_OUTPUT_ROOT=<scratch>/unity/rc-build \
  "/Volumes/SSD/unity/editors/6000.3.0f1/Unity.app/Contents/MacOS/Unity" -batchmode -nographics \
  -projectPath <worktree>/games/evergrove -buildTarget OSXUniversal \
  -executeMethod Evergrove.UnityClient.Editor.EvergroveBuild.Build -logFile <scratch>/unity/rc-build.log -quit
  -> exit 0
  -> EVERGROVE_BUILD_SUCCEEDED release-candidate <scratch>/unity/rc-build/StandaloneOSX/Mosselora-QA.app
  -> "development" appears 0 times in the build log
  -> locale staging folder removed; 58 locale files (29 json + 29 meta) restored
```

The Unity build wrote `productName` and a Standalone `applicationIdentifier` into `ProjectSettings/ProjectSettings.asset`. Those edits were reverted with `git checkout` and are not part of the commit. The build script sets them each run.

## Shipped binary checks

Player: `Mosselora-QA.app` from the release-candidate build (macOS, OSXUniversal).

- `Info.plist`: `CFBundleIdentifier` = `com.streamentry.evergrove`, `CFBundleName` = `Mosselora`, `CFBundleShortVersionString` = `0.1.0`. `CFBundleVersion` is `0`, which is Unity's standalone build number and is separate from the bundle version.
- `Contents/Resources/Data/Managed/Assembly-CSharp.dll`, ASCII and UTF-16LE scan (C# literals are stored as UTF-16):

| Identifier | Result |
| --- | --- |
| DevQaTour, DevScreenshot, TextSpike | absent (ASCII and UTF-16) |
| `matched control`, `Matched control`, `Start matched control` | absent |
| mosseloraQaTour, mosseloraScreenshot, mosseloraTextSpike, mosseloraTextScale | absent |
| LocalPlaytest, VillageDiorama | absent |
| `Diorama visual comparison`, `Playtest consent`, `Copy playtest`, `Miniature diorama` | absent |
| MOSSELORA_QA_TOUR_DONE, MOSSELORA_SR_NODES, MOSSELORA_TEXT_SCALE_OVERRIDE | absent |
| Positive control "Living storybook art" (UTF-16) | present, so the scan method works |
| Mosselora, Evergrove | present |

- Player data files (`sharedassets0.assets`, `globalgamemanagers.assets`, `level0`, `Resources/*`): zero occurrences of `Locales/`, so the research catalogs are not packaged.
- Development flag: `BuildOptions.None` is passed. The Unity log has no development-build mention. Unity writes no development flag into `Info.plist`, so this is evidence from the build options and the log, not from a flag in the artifact.

## Residuals (not fixed here)

These are inert text or domain code. They are not controls, and no player path reaches them in release.

1. `GameLocalization` dictionary text for research UI is still in `Assembly-CSharp.dll`: "Visual: 2.5D miniature (tap for 2D)", "Miniature motion: ON/OFF". Removing these needs the 97-key catalog contract and tests to change, so it is a separate task.
2. Core domain code is compiled into release and is unreachable from UI: `PlayMode.PuzzleOnly` and `PuzzleMatched`, `WorldModel.NewMatched`, `FormatSchedule`, `TryParseSchedule`. Puzzle-mode labels such as "Puzzle condition" are displayed in that mode. A release player reaches it only through a save file with a non-living mode.
3. The `RC` locale staging relies on Unity's asset database refresh. It was verified once, on this machine and this Unity build.
4. No positive-control dev build was run. The check that the locale catalogs are absent relies on the absence of any `Locales/` path string, not on a dev build containing one.

## Pending (lead)

- Capture an iOS-simulator release-candidate screenshot from `EVERGROVE_BUILD_CHANNEL=release-candidate` and attach it to BLD-007 with this audit. Confirm no matched, mode, miniature, comparison, or playtest controls are visible.
- Playtest and accessibility review with the intended 45+ audience is out of scope for this audit.
