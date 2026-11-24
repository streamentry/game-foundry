# Mosselora mobile privacy review — source-level

**Reviewed:** 2026-11-24  
**Scope:** current source at `47ad558274a37c2fd78840be6e4247c5bbecf027`  
**Status:** source review only. No current-source mobile build exists, so this is not a final App Privacy or Google Play Data Safety declaration and is not legal advice.

## Observed in the project

- `Packages/manifest.json` has Unity UI, Input System, Accessibility, JSON and networking modules. It has no third-party analytics, advertising, attribution, crash-reporting or purchase SDK package.
- `ProjectSettings/UnityConnectSettings.asset` has Unity Connect, Insights, Crash Reporting, Cloud Diagnostics, Analytics, Ads and Performance Reporting disabled. The project has no Unity organization ID. Unity's Unity 6.3 documentation says Analytics requires the Analytics package and an explicit Unity Services initialization flow; the project has neither. Unity's Diagnostics documentation says collection requires a Unity Cloud linked project and enabled diagnostics; neither is present in the checked project settings.
- `ProjectSettings/ProjectSettings.asset` contains `submitAnalytics: 1`. Its effect on the final mobile player has not been established from this source snapshot. Resolve it in a verified Unity Editor/build profile and inspect the resulting binaries before making a collection claim.
- `Assets/Scripts/Unity/WorldClient.cs` is the only runtime source reference to `WorldClient` and the only `UnityWebRequest` usage found. It is an optional prototype component; no scene, prefab or C# bootstrap reference to it was found. Its URL defaults to empty, and `CanSend()` rejects an empty URL. The separate API Worker is not connected to the game save flow. The Unity networking module can still cause platform network capability declarations; inspect generated manifests.
- Gameplay progress, settings and language/text-size preferences use local storage. There is no account or sign-in flow in the current game code. The return diary is guarded for Editor/development builds; the static release-candidate verifier passed this guard.

## What this does not prove

The signed release archive has not been produced. This review does not prove the final iOS/Android binary has no telemetry, identifiers, network traffic, linked Unity service, third-party code or platform-mediated backup. It also does not verify a clean-install network trace.

The current public `/privacy/` page covers the website and research preview, not the mobile game. Do not use it as the app's privacy-policy URL yet.

## Release checks still required

1. Inspect the non-development signed archive: package inventory, permissions, privacy manifests, SDKs, identifiers and compiled development controls.
2. Install a clean copy on each platform, exercise first launch, puzzle/save, background/resume and offline mode, and capture outbound traffic.
3. Read back Unity Diagnostics and `submitAnalytics` settings for the exact release profiles.
4. Write and publish the mobile policy from those results; then have the account owner confirm Apple App Privacy and Google Play Data Safety answers.

**Conclusion:** the source supports an offline-first, local-save design and has no identified third-party measurement SDK. Binary-level privacy verification remains open; no store privacy declaration is ready.

## Primary Unity references checked

- [Unity 6.3 Diagnostics project settings](https://docs.unity.com/en-us/engine/6000.3/manual/unity-editor/editor-settings-reference/comp-manager-group/diagnostics-settings)
- [Unity Analytics setup and collection](https://docs.unity.com/en-us/analytics/get-started/get-started)
