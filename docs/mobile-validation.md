# Mobile export and SDK assessment

**Previous detailed preflight:** 2026-11-15

## Current verification (2026-11-22)

**Decision:** no game is release-eligible. The eight requested projects remain prototypes without adult-45+ player validation, demand/retention evidence, or production sign-off. Console searches found no exact-title records; no records or listings were created.

**Toolchain:** Godot 4.7.2 standard editor and matching official templates are installed. The template archive SHA-256 was checked against the upstream GitHub release: f298490b8d44d934be425a5a65a51bf15f422428b229a06e11d9ffea248011. Xcode 27.2 and iOS SDK 27.2 are installed; iOS Simulator runtimes 18.5 and 26.5 are present. Android API 36, Build Tools 36.1.0, NDK 29.0.14206865, and current command-line tools are installed. JDK 21 was used for the local export proof; the repository baseline recommends JDK 17, which is not installed.

**Android debug proof:** temporary debug APKs for all eight projects compiled with min SDK 24 and target SDK 36, signed with Godot's debug key, installed, and launched on an Android 16/API 36 Pixel 7 virtual device. All eight Gradle AABs built but are unsigned. ADB found no attached physical Android device. The proofs used temporary package IDs and a generic test icon. The ETC2/ASTC setting is now present in all eight source projects, which re-imported successfully; production app icons remain unset.

Temporary Godot iOS projects for all eight titles exported, and Xcode compiled all eight arm64 device targets without signing. The Kakuro iOS Simulator build failed: local inspection of the installed 4.7.2 simulator framework showed its declared simulator slice contains x86_64-only objects, while this Apple Silicon Simulator target requires arm64. This matches the packaging failure reported for the prior 4.7.1 templates in [Godot issue 122379](https://github.com/godotengine/godot/issues/122379); the local 4.7.2 archive was checked directly. No physical iPhone is attached. Xcode has zero valid code-signing identities; 11 provisioning profiles exist, but none is verified for the unassigned app IDs.

Each game's Android emulator screenshot and short recording are in its native-proof/android-api36-pixel7-avd folder; see the capture index at store/assets/native-proof-index.md. They show real running debug builds but are not physical-device QA or final store media. No iOS capture was possible.

All eight projects passed their existing Godot import, rule, persistence, and scene checks. The research backlog validator and all 50 Python unit tests passed. The web-playtest tests needed loopback access to bind a local HTTP server.

**Remaining release blockers:** permanent bundle/package IDs, production icons, final support and privacy URLs, signed release artifacts and keys, physical-device QA, audience and market gates, age/content and privacy declarations, price and territory decisions, and account-owner confirmations.

**Related issue:** [#4 — validate Godot native iOS/Android export and privacy SDK options](https://github.com/streamentry/game-foundry/issues/4)

**Current status:** Partial. Local native export proofs exist for Kakuro only; signed store builds and physical-device checks remain open.

## Historical Takuzu Lab project snapshot (2026-11-15)

Takuzu Lab is an engineering reference, not a release candidate. It uses the Compatibility renderer and **already has local, versioned JSON progress save/resume** (`scripts/progress_store.gd`) plus a **Web Playtest export preset** (`export_presets.cfg`). It has **no iOS or Android export presets**, platform app identifiers, mobile signing setup, or SDK plugins yet. No platform SDK should be added until a real game needs it.

The environment observations below reflect **one development-machine preflight on 2026-11-15**, not a fresh scan of today's devices or toolchain. Headless and CI evidence is not proof of native mobile readiness.

## Historical local preflight (2026-11-15)

| Item | Observation |
| --- | --- |
| Godot | `godot` and `godot4` are not on `PATH`. The official 4.7.2 binary was downloaded to temporary storage; headless import and rules checks passed with `4.7.2.stable.official.ed1daf0bf`. No mobile export was run. |
| Xcode | Xcode 27.2 (build 27B52); iOS device and simulator SDKs 27.2 are installed. |
| iPhone | Escalated `xcrun devicectl list devices` found 11 shutdown simulators and no physical iPhone. No booted simulator was reported. |
| Android SDK | `/Users/james/Library/Android/sdk` points to `/Volumes/SSD/Developer/Android/sdk`. Platform Tools are present; `build-tools` and `platforms` directories are absent. `ANDROID_HOME` is unset. |
| Android device | Escalated `adb devices -l` reported no attached devices. |

No signed build was produced. The iOS signing profile, physical test devices, Android build packages, and matching Godot export templates remain unverified.

## Godot 4.7.2 export baseline

Use the **standard** engine and the matching [Godot 4.7.2 standard export templates](https://godotengine.org/download/archive/4.7.2-stable/). The template archive is `Godot_v4.7.2-stable_export_templates.tpz`; do not mix templates from another engine version or the .NET edition.

The exact [4.7.2 Android build config](https://github.com/godotengine/godot/blob/4.7.2-stable/platform/android/java/app/config.gradle) sets `minSdk 24`, `compileSdk 36`, `targetSdk 36`, Build Tools `36.1.0`, and Java 17. Godot's Android detection code pins NDK `29.0.14206865`. These are engine defaults; a project preset or native plugin can raise the minimum. Confirm the final manifest after export.

This target meets Google Play's current requirement: since 2026-08-31, new apps and updates must target Android 16 / API 36 or higher. The older package list on the Godot 4.7 Android export page still says Platform 35, Build Tools 35.0.1 and NDK 28.1.13356709. For this pinned patch release, follow the tagged 4.7.2 build config, then verify the exported artifact.

For a Gradle export with Android plugins, use JDK 17, Platform Tools, Android Platform 36, Build Tools 36.1.0 and Command-line Tools. The engine config pins NDK 29.0.14206865; install it when the selected plugin or build requires it. Godot's built-in Android IAP plugin and native Android plugins require a Gradle build. An ordinary test export without plugins can use the matching prebuilt Android templates.

## Reproducible device workflow

Keep signing credentials local. Do not commit certificates, provisioning profiles, keystores, or passwords.

1. Install the standard Godot 4.7.2 editor and its matching export templates. Confirm `godot --version` reports `4.7.2.stable`.
2. Import the project:

   ```sh
   godot --headless --path games/takuzu-lab --editor --import --quit
   ```

3. Add an iOS export preset with a unique bundle identifier and the local development signing profile. Export the iOS project as a ZIP, open its `.xcodeproj` in Xcode, select the connected iPhone, and run with local signing. The Godot [iOS export guide](https://docs.godotengine.org/en/4.7/tutorials/export/exporting_for_ios.html) documents this flow.
4. Add an Android debug export preset. Export an APK signed with a local debug key, inspect the manifest for `minSdkVersion=24` and `targetSdkVersion=36`, then install it on the connected device. Use an AAB and a private release keystore only for a Play testing track.
5. On both physical devices, record cold start, tap-target fit, safe areas and orientation, pause/resume, offline play, and performance on a lower-end device. These checks have not been run.

After installing Android Command-line Tools, install the package versions pinned by the 4.7.2 config:

```sh
GAME_FOUNDRY_ANDROID_SDK="$HOME/Library/Android/sdk"
"$GAME_FOUNDRY_ANDROID_SDK/cmdline-tools/latest/bin/sdkmanager" \
  --sdk_root="$GAME_FOUNDRY_ANDROID_SDK" \
  "platform-tools" \
  "build-tools;36.1.0" \
  "platforms;android-36" \
  "cmdline-tools;latest" \
  "ndk;29.0.14206865"
```

Godot's editor settings must point to that SDK root and the local JDK 17 installation. Confirm the generated `android/build/config.gradle` still has `minSdk 24` and `targetSdk 36` before exporting.

## SDK shortlist

| Need | Candidate and evidence checked on 2026-11-15 | Decision |
| --- | --- | --- |
| Ads | [Godot AdMob](https://github.com/godot-sdk-integrations/godot-admob) v7.0 says it supports Godot 4.7 and iOS/Android. It includes Google's UMP consent flow and iOS tracking authorization. The plugin is MIT-licensed. | Candidate only. It adds native SDK and consent work; no current game has an ad requirement. Verify the actual exported app and data disclosures before adoption. |
| Google Play purchases | [GodotGooglePlayBilling](https://github.com/godot-sdk-integrations/godot-google-play-billing) is documented by Godot as its first-party Android Billing integration, compatible with Godot 4.2+. It needs a Gradle Android build and is MIT-licensed. | Candidate for Android IAP. Test pending purchases, acknowledgement, restore/ownership and refunds on a Play test track before use. |
| Apple purchases | [Godot StoreKit 2](https://github.com/godot-sdk-integrations/godot-storekit2) v0.2 is MIT-licensed and iOS-only. Its README says development is ongoing and the API may change or contain bugs. | Do not select for production until Godot 4.7.2 compatibility and sandbox purchase/restore flows are proven on an iPhone. |
| Firebase | [Godotx Firebase](https://github.com/godot-x/firebase) claims Godot 4.7 support, MIT licensing, iOS 13+ and Android API 24+. It packages Analytics, Crashlytics and Messaging modules with native SDK dependencies. | Defer. Takuzu Lab needs no account, cloud sync, push, or analytics. The upstream README reports added binary size and describes consent defaults; independently review the bundled SDKs and privacy behavior before use. |
| Privacy-light prototype | Keep gameplay local and omit ads, analytics and account SDKs. Use consent-based play sessions to find interaction problems; add store reporting only after a release exists. | Best fit now: no user-data flow or native SDK maintenance without a demonstrated need. |

Apple requires App Store privacy disclosures to include data practices of third-party SDK partners. Google Play also requires a Data safety disclosure. An MIT plugin license does not license the vendor SDKs or remove those disclosure duties; audit each bundled dependency separately.

## Remaining acceptance work (not yet signed off)

- [x] Install Godot 4.7.2 and matching standard export templates; archive SHA-256 recorded above.
- [x] Install Android API 36, Build Tools 36.1.0, NDK 29.0.14206865, and current command-line tools. JDK 21 was used in the local proof; install/retest with the documented JDK 17 baseline before release.
- [x] Export and inspect temporary Android debug APKs for all eight titles; min SDK 24 and target SDK 36 verified; install and launch each on a Pixel 7 AVD.
- [x] Export and compile temporary iOS arm64 device targets for all eight titles without signing.
- [ ] Produce a release-signed Android AAB with a production package ID and user-owned upload key.
- [ ] Sign and install an iOS build using the verified Apple team identity and a matching provisioning profile.
- [ ] Rebuild the iOS Simulator template for arm64 or use a supported simulator build; current template fails on this Apple Silicon host.
- [ ] Run physical-device checks on an iPhone and Android phone, including a representative lower-end device.
- [ ] Complete adult-45+ usability and market gates, then finalize each app's privacy, age/content, export, price, territory, rights, and account-owner declarations.

## Sources

- [Godot 4.7.2 archive and standard export templates](https://godotengine.org/download/archive/4.7.2-stable/)
- [Godot 4.7.2 Android build config](https://github.com/godotengine/godot/blob/4.7.2-stable/platform/android/java/app/config.gradle)
- [Godot 4.7 Android export guide](https://docs.godotengine.org/en/4.7/tutorials/export/exporting_for_android.html), [iOS export guide](https://docs.godotengine.org/en/4.7/tutorials/export/exporting_for_ios.html), and [Godot issue 122379 on the simulator architecture packaging defect](https://github.com/godotengine/godot/issues/122379)
- [Google Play target API requirement](https://developer.android.com/google/play/requirements/target-sdk)
- [GodotGooglePlayBilling documentation](https://docs.godotengine.org/en/stable/tutorials/platform/android/android_in_app_purchases.html)
- [Apple App Privacy Details](https://developer.apple.com/app-store/app-privacy-details/) and [Google Play Data safety](https://support.google.com/googleplay/android-developer/answer/10787469)
