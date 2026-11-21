# Mobile export and SDK assessment

**Checked:** 2026-11-15

**Related issue:** [#4 — validate Godot native iOS/Android export and privacy SDK options](https://github.com/streamentry/game-foundry/issues/4)

**Status:** Partial. The toolchain is documented; signed device builds and physical-device checks are not verified.

## Current project

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

- [ ] Install Godot 4.7.2 and matching standard templates; capture the exact installed template version.
- [ ] Export, sign, install, and launch on a physical iPhone; record iOS deployment target, Xcode project settings, and blockers.
- [ ] Install the Android API 36 toolchain; export, sign, install, and launch on a physical Android device; record the final manifest minimum/target SDK and template names.
- [ ] Complete the device checks above, including a representative lower-end Android device.
- [ ] Recheck the SDK choices only against the actual product, then document the privacy forms and third-party licenses for any SDK selected.

## Sources

- [Godot 4.7.2 archive and standard export templates](https://godotengine.org/download/archive/4.7.2-stable/)
- [Godot 4.7.2 Android build config](https://github.com/godotengine/godot/blob/4.7.2-stable/platform/android/java/app/config.gradle)
- [Godot 4.7 Android export guide](https://docs.godotengine.org/en/4.7/tutorials/export/exporting_for_android.html) and [iOS export guide](https://docs.godotengine.org/en/4.7/tutorials/export/exporting_for_ios.html)
- [Google Play target API requirement](https://developer.android.com/google/play/requirements/target-sdk)
- [GodotGooglePlayBilling documentation](https://docs.godotengine.org/en/stable/tutorials/platform/android/android_in_app_purchases.html)
- [Apple App Privacy Details](https://developer.apple.com/app-store/app-privacy-details/) and [Google Play Data safety](https://support.google.com/googleplay/android-developer/answer/10787469)
