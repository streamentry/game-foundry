# Privacy and offline binary inspection, 2026-11-23

Task: partial evidence for EG-009 (Offline/privacy binary verification, `docs/specs/02-FEATURE-BACKLOG.md`), trust boundaries in `docs/ARCHITECTURE.md`, and the source-only privacy inventory note in `docs/research/decision-log.md`.

## Scope and limits

- **Artifact inspected:** a Unity development build for macOS, `Mosselora-QA.app`, built into the session scratchpad. The executable inside is `Contents/MacOS/Mosselora Playtest` (Info.plist bundle id `com.Game-Foundry.Mosselora-Playtest`, version 0.1.0).
- **Evidence level:** desktop development build only. It is not a phone build. IL2CPP, stripping, release-mode Player Connection, and platform SDK differences mean the conclusions only partly transfer to iOS or Android.
- **Not verified:** the real iOS or Android release builds, controlled network capture on a device, and store privacy declarations.
- **Network observation:** a single short run of about 6 seconds, with `lsof` sampling. This is not a traffic capture, so it cannot show encrypted payloads or domain lookups. It does not prove absence of traffic outside that window.
- No files in the project were edited. The only write is this document.

## 1. Managed assemblies shipped

`Contents/Resources/Data/Managed` holds 107 `.dll` files. Grouped by origin:

| Group | Assemblies | Notes |
|---|---|---|
| Game code | `Assembly-CSharp.dll` | Contains `Evergrove.*`, `EvergroveApp`, `WorldClient`, `MosseloraTheme`, `DevQaTour`, `DevScreenshot`. |
| Unity packages (in `Packages/manifest.json` or lock) | `Unity.InputSystem.dll`, `Unity.InputSystem.ForUI.dll`, `Unity.TextMeshPro.dll`, `UnityEngine.UI.dll`, `Unity.InternalAPIEngineBridge.004.dll` | First-party Unity code. |
| Unity engine modules | `UnityEngine.*Module.dll` plus `UnityEngine.dll`, `UnityEngine.CoreModule.dll` | Includes `UnityWebRequestModule`, `UnityConnectModule`, `UnityAnalyticsModule`, `UnityAnalyticsCommonModule`, `UnityConsentModule`, `CrashReportingModule`, `PerformanceReportingModule`, `UnityCurlModule`, `GameCenterModule`, `VideoModule`, `XRModule`. |
| .NET base libraries | `mscorlib.dll`, `netstandard.dll`, `System*.dll`, `Mono.Security.dll` | Standard BCL. |

**Third-party SDK check:** no assembly beyond the groups above is present. A filename search for `appsflyer`, `adjust`, `firebase`, `amplitude`, `mixpanel`, `sentry`, `gameanalytics`, `admob`, `unityads`, `facebook`, `applovin`, `ironsource` found no SDK assembly. Byte-level string hits for these names were all incidental (for example `GetAdjustedFormat` in CoreModule, `UnityAdsSettings` as an engine type name, and `amplitude` as a substring of the word "amplitude" in `WaterWaveIcon.cs`). The only non-Unity, non-BCL assembly is `Assembly-CSharp.dll`.

Matches `games/evergrove/Packages/manifest.json` (`ugui`, `inputsystem`, `modules.ui`, `modules.jsonserialize`, `modules.unitywebrequest`, `modules.unitywebrequestwww`). `packages-lock.json` also pulls `unitywebrequestassetbundle` and `unitywebrequestaudio`, which are Unity-owned modules.

## 2. WorldClient and UnityWebRequest in the player

### 2a. Compiled in

- `Assembly-CSharp.dll` contains the type `Evergrove.UnityClient.WorldClient` and its compiler-generated state machines `<RequestWorld>d__17` and `<PostWater>d__18`. The source file path `\Assets\Scripts\Unity\WorldClient.cs` is embedded in the PDB-style metadata.
- It references `UnityEngine.UnityWebRequestModule`, `UnityWebRequest`, and `UnityWebRequestAsyncOperation`.
- So `WorldClient` and `UnityWebRequest` calls are **compiled into** the player.

### 2b. Instantiated at boot?

Source reading:

```
$ grep -rn -E "WorldClient" --include='*.cs' Assets
Assets/Scripts/Unity/WorldClient.cs:42:    public sealed class WorldClient : MonoBehaviour
```

- There is no `AddComponent<WorldClient>`, `new WorldClient`, or other reference in any `.cs` file except its own definition.
- No `.unity` scene or `.prefab` under `Assets` references `WorldClient` (grep returned nothing).
- `Assets/Editor/EvergroveSceneSetup.cs` contains no reference to `WorldClient` or `EvergroveApp`.
- The boot path is `EvergroveApp.Bootstrap()` (`[RuntimeInitializeOnLoadMethod(AfterSceneLoad)]`), which creates a `GameObject("Evergrove")` and adds only `EvergroveApp`. `EvergroveApp` builds its UI from code and makes no network calls.
- `WorldClient.workerBaseUrl` defaults to `""`. Its request methods return early unless the URL begins with `https://`. It is only set by `SetPreviewWorkerUrl`, which nothing calls.
- `docs/ARCHITECTURE.md` (line 45) also states: "Not attached by boot; does not sync this game's true world."

Shipped-binary check (UTF-16 literals, because C# user strings are stored as UTF-16LE in the `#US` heap). A short Python scan (`python3 -I`) decoded each managed DLL's UTF-16LE and ASCII runs and matched `https?://`, `mosselora`, `workers.dev`, `api.`, `cloudflare`, `analytics`, `sentry`, `crash`:

```
Assembly-CSharp.dll U16: -mosseloraQaTour
Assembly-CSharp.dll U16: -mosseloraScreenshot
Assembly-CSharp.dll U16: MOSSELORA_QA_TOUR_DONE
Assembly-CSharp.dll U16: MOSSELORA_SCREENSHOT_SAVED
Assembly-CSharp.dll A8: $Evergrove.UnityClient|MosseloraTheme
```

- **No hard-coded endpoint** (`https://`, `mosselora.com`, `workers.dev`, or an API host) appears in `Assembly-CSharp.dll` as a user string. The only `https://` hits are inside `System.*` and `Mono.Security` (XML namespace and deprecation-message URLs).
- The only `WorldClient` string in the game DLL is type metadata. The `httpsBaseUrl` hit from an earlier `strings` pass is the parameter name of `SetPreviewWorkerUrl`, not a value.

Verdict for 2b: **WorldClient is compiled but not instantiated at boot, and no network endpoint is embedded in the game assembly.** This is a source-plus-binary-string result, not a runtime proof. A runtime check of the object graph (for example via the Player Connection profiler) was not performed.

### 2c. Development-only code in this build

`DevScreenshot` and `DevQaTour` are compiled under `#if UNITY_EDITOR || DEVELOPMENT_BUILD`, and their strings (`-mosseloraScreenshot`, `MOSSELORA_QA_TOUR_*`) are present here. That confirms this is a development-style build and that QA-only code is included. A release build must be checked to confirm it is stripped.

## 3. Unity services settings

`ProjectSettings/UnityConnectSettings.asset`:

```
  m_Enabled: 0
  InsightsSettings:
    m_EngineDiagnosticsEnabled: 0
    m_Enabled: 0
  CrashReportingSettings:
    m_EnableCloudDiagnosticsReporting: 0
  UnityPurchasingSettings:
    m_Enabled: 0
  UnityAnalyticsSettings:
    m_Enabled: 0
  UnityAdsSettings:
    m_Enabled: 0
  PerformanceReportingSettings:
    m_Enabled: 0
```

`ProjectSettings/ProjectSettings.asset`:

```
  submitAnalytics: 1
  usePlayerLog: 1
  cloudServicesEnabled: {}
  cloudProjectId:
```

- Every Unity cloud service flag (Analytics, Ads, Purchasing, Insights, Crash reporting, Performance reporting) is **disabled** in the asset.
- `submitAnalytics: 1` is not a player SDK switch. It is generally the Editor usage-analytics setting. I did not verify its effect in the built player, so treat it as unverified rather than proven harmless.
- `usePlayerLog: 1` writes the player log locally. It makes no network request by itself.
- No cloud project id is set.

## 4. Runtime run and socket sampling

Command (arguments as specified, run via Python `subprocess`, which launches the binary directly):

```
"Contents/MacOS/Mosselora Playtest" -screen-width 540 -screen-height 960 -screen-fullscreen 0 \
  -mosseloraScreenshot <scratchpad>/unity/priv.png -mosseloraScreenshotFrames 300 \
  -logFile <scratchpad>/unity/privacy-run.log
```

`lsof -i -n -P -a -p <pid>` was sampled at about 1.0, 2.5, 4.1, and 5.6 s after launch. The process exited on its own with code 0 at about 7 s.

Sampled output (identical across the four samples):

```
COMMAND     PID  USER   FD   TYPE             DEVICE SIZE/OFF NODE NAME
Mosselora 62602 james    5u  IPv4 ...      0t0  UDP *:*
Mosselora 62602 james    6u  IPv4 ...      0t0  UDP *:*
Mosselora 62602 james    7u  IPv4 ...      0t0  TCP *:55000 (LISTEN)
Mosselora 62602 james    8u  IPv4 ...      0t0  UDP *:62297
```

- **No outbound TCP connection (ESTABLISHED or SYN) was observed** in any of the four samples.
- **One TCP listener on port 55000 on all interfaces (`*:55000`)** is present. The player log identifies it as Unity's Player Connection target (`[IP] <LAN address> [Port] 55000 ... [Debug] 0 ... [ProjectName] Mosselora Playtest`). It also logs a UDP target-info broadcast on `225.0.0.222:54997`.
- Four unbound UDP sockets (`*:*`, `*:62297`) are present. I could not attribute them to a component from `lsof` alone. They are not observed as connected to any remote host.
- `boot.config` contains `player-connection-mode=Listen`, which is consistent with a Development Build. Release builds normally do not open this listener, but this has to be confirmed on the release artifact.
- The log contains no lines from `UnityWebRequest`, `WorldClient`, Unity Connect, Analytics, Ads, or crash reporting. The memory-label dump at shutdown lists `WebRequest`, `CloudService`, and `CoreBusinessMetrics` as allocation categories. Those are engine allocation labels, not evidence of traffic.
- The screenshot was saved (`MOSSELORA_SCREENSHOT_SAVED`), and the process exited cleanly.

Limits: `lsof` shows socket state, not payloads. A ~6 s window does not exercise purchases, save/restore, or any user action that could trigger a request. The Player Connection listener is a development-only behaviour and is not representative of a release build.

## Verdict (evidence level)

- **Desktop development build, Mosselora Playtest (macOS):**
  - No third-party analytics, ads, attribution, or crash SDK assembly is shipped.
  - `WorldClient` and `UnityWebRequest` are compiled in, but not attached by the boot path or any scene, and the game assembly embeds no production endpoint.
  - Unity cloud services are disabled in project settings.
  - No outbound network socket was observed in a single short (~6 s) run.
  - The only listening socket is the development Player Connection listener on TCP 55000, which is a development-build artifact.
- This **does not** verify the iOS or Android app, and does not establish that the player never sends traffic.

## What remains for EG-009

1. **Release-configuration IL2CPP builds for iOS and Android** (not Development Builds). Confirm that Player Connection, `DevScreenshot`, and `DevQaTour` are absent or inert, and that `WorldClient` remains unattached.
2. **Controlled device network capture** on the release build during a full offline session: first launch, play, save, quit, relaunch, and language change. Record all DNS and TLS destinations. Expected result: zero destinations outside the device OS itself. Use a proxy or packet capture on a dedicated test network, and keep the capture with the evidence.
3. **Symbol and string check on the release artifact**, re-running the UTF-16 literal scan on the IL2CPP output (`GameAssembly` binaries) rather than the managed DLLs.
4. **Store declarations:** App Privacy (Apple) and Data Safety (Google) answers reconciled against the capture. Both should state no data collected or shared, unless the capture shows otherwise.
5. **Evidence file:** `games/evergrove/docs/evidence/<SOURCE_SHA>/EG-009/verification.md`, per the spec. It is currently **NOT COLLECTED**.

Mosselora is not verified as offline or privacy-clean on any phone build.
