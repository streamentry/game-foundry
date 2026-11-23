# Mosselora release-candidate evidence — 2026-11-23

This is local build and emulator evidence for the `Evergrove` internal project, public title Mosselora. It does not record a store upload, submission, approval, or public release.

**Status refreshed 2026-11-24:** the artifacts below are from earlier source revisions. No current-`HEAD` mobile release candidate is verified.

## Source and checks

- Earlier release-candidate artifacts predate current `HEAD` / `origin/main` `5435e820a7175aea3d0e834299da9913c3172747`. Unity code last changed at `0d0b76d`; later commits add QA and language findings, including Bengali conjuncts still needing native text. Current code includes painted UI, Android engine-linking/minimum-SDK, accessibility, CJK and RTL changes. No build from current code has completed.
- Unity `6000.3.0f1` imported the project with 12 source puzzles and 29 locale catalogs. Android and iOS Build Support modules are installed beside the Editor app bundle.
- The .NET 8 game-core suite passed 39,900 assertions on SDK 8.0.428. The project verifier passed with 101 keys across 29 offline catalogs. The Unity code is unchanged since `0d0b76d`; later commits add QA findings and documentation. These checks do not prove store readiness or human usability.
- The release-candidate build method and Android/iOS build-number controls are committed in current source. A clean rebuild still needs the installed Editor path resolved and must use version code/build number 2.
- The previous Android AAB and iOS device/Simulator apps passed bounded scans for development-only control identifiers; those results do not cover current `HEAD`. Re-run the scan on a newly built current artifact. A bounded strings scan does not replace runtime/privacy/security review.

## Android

- Unity produced `Mosselora.aab` for `com.streamentry.evergrove`, version `1.0.0`, bundle version code `1`, min SDK `25`, target SDK `36`.
- Google `bundletool validate` passed. The Unity-generated Android Debug signature was removed; `jarsigner` verifies the final AAB with exactly one signer, `CN=Mosselora Google Play Upload Key`.
- Final AAB: `.evergrove-evidence/rc-android/output/Android/Mosselora-upload-signed.aab`  
  SHA-256: `dc5ac1cd78c6655e310e5e292257d05b987b8bda6d3268900fbade404a47be9b`
- This is build 1 from before later Android, accessibility, CJK and RTL changes; do not upload it. A separate 31.9 MB unsigned development APK, rebuilt after restoring engine stripping with `link.xml`, showed no class-115 errors and solved chapter one on Android 15/API 35. It predates some current code and is not a store candidate. A build-2 attempt from a clean archive of current `HEAD` did not reach Unity because the CLI rejected the SSD Editor bundle; `codesign --verify --deep --strict` reports that its signature was modified. The installed CLI has no registered Editors and offers only a validation bypass, which has not been authorized.
- The upload keystore is outside the repository under the user's Library support folder; its random password is in macOS Keychain. Neither secret is recorded here.
- The current source vendors BidiSharp as pure C# under MIT. Its source note records Unicode 6.3 data and says it is not on the shipping allowlist. Resolve the shipping status and package the required license notice before shipping the RTL implementation.
- A universal APK derived from the signed AAB installed and launched on the read-only `GameFoundryKakuroProof` AVD (Android 16/API 36, `1080×2400`). Touch input solved chapter one, selected “Plant a shade tree,” reached `STREAM 2 / 12`, and a swipe revealed the lower controls. A force-stop/relaunch preserved chapter two and the chosen tree. With airplane mode enabled and no default network, the app also solved chapter one and saved the tree choice. The AAB declares `android.permission.INTERNET`; this offline check does not replace a controlled network capture. These are emulator results, not a physical Android phone or a 45+ player session.
- Earlier build captures: [first launch](android-emulator-release-candidate-2026-11-23/mosselora-first-launch.png), [one tile turn](android-emulator-release-candidate-2026-11-23/mosselora-after-one-tap.png), [puzzle solved and choices](android-emulator-release-candidate-2026-11-23/mosselora-puzzle-complete.png), [choice outcome](android-emulator-release-candidate-2026-11-23/mosselora-after-tree-choice.png), [scroll check](android-emulator-release-candidate-2026-11-23/mosselora-after-scroll.png), [chapter two](android-emulator-release-candidate-2026-11-23/mosselora-chapter2-start.png), [saved state after relaunch](android-emulator-release-candidate-2026-11-23/mosselora-after-relaunch-stable.png), [first loop in airplane mode](android-emulator-offline-2026-11-23/mosselora-offline-choice.png), and [preview video](android-emulator-release-candidate-2026-11-23/mosselora-preview.mp4). Recapture after building current `HEAD` before store upload.

## iOS

- Unity exported an iOS device-SDK Xcode project with production bundle ID `com.streamentry.evergrove`, short version `1.0.0`, and `CFBundleVersion=1`. Xcode compiled the arm64 iOS device target with `CODE_SIGNING_ALLOWED=NO`; the resulting `.app` is unsigned and is not an IPA.
- A matching Simulator candidate with `CFBundleVersion=1` installed and launched on iOS 26.5 Simulator. It remains unsigned and is only visual/runtime evidence.
- Earlier captures: [Simulator first launch, build 1](ios-simulator-release-candidate-2026-11-23/mosselora-first-launch-build1.png), [15-second preview clip, build 1](ios-simulator-release-candidate-2026-11-23/mosselora-preview-build1.mp4). Earlier build-0 captures are retained in the same folder for history. Recapture from the latest signed candidate before store upload.
- No physical iPhone run is verified. Local Xcode reports zero valid code-signing identities; no signed IPA or Transporter upload exists.

## Store and web state

- App Store Connect has draft Apple app ID `6838015597`, bundle ID `com.streamentry.evergrove`, name Mosselora, category Games/Puzzle, and automatic release after App Review selected. In the signed-in Browser, version 1.0 is still Prepare for Submission; no build is selected. Support and Marketing URLs point to `mosselora.com`. The version page lists missing Content Rights information, age rating, mobile Privacy Policy URL, selected build, App Privacy completion by an Admin, and App Review first name, last name, email and phone. Local Xcode reports zero valid signing identities, so no signed IPA exists.
- Google Play Home search finds no Mosselora app record. The Create App form is restaged with Mosselora, `com.streamentry.evergrove`, English (U.S.), Game and Free; package availability is confirmed. Developer Program Policy and U.S. export-law declarations remain unchecked for the account owner.
- The API Worker is live but remains a prototype: `/healthz` returns HTTP 200 with `gameplaySync=false`, and unauthenticated `/api/world` returns 401. It is not connected to game saves.
- Astro production is live at `https://mosselora.com`, version `f8c23470-8134-4aee-adbf-6a6a3411f2ea`; the home, support, privacy, preview and promo-art routes return HTTP 200. The support page displays `hi@mosselora.com`. The preview Worker remains available at `mosselora-site-preview.workers.dev`. The user explicitly approved routing the apex after the earlier review rejected it because it could replace existing site traffic.

## Store media candidates

- JPEG exports are under [`store/assets/evergrove/1.0/`](../../../../store/assets/evergrove/1.0/README.md). They preserve the actual app captures' pixel dimensions and remove the alpha channel; the original PNGs remain in the QA folders.
- Apple has one earlier iOS Simulator screenshot at 1206 × 2622, a listed accepted iPhone Dynamic Island medium-display size; it is from build 1 and is not uploaded. A native-resolution current painted-UI development screenshot is archived at [`painted-ui-2026-11-24/15-high-resolution-live-simulator.png`](painted-ui-2026-11-24/15-high-resolution-live-simulator.png), but it visibly includes a development-only control and is QA-only.
- Google Play has three actual phone AVD screenshots at 1080 × 2400. They fit Google's 320–3840 pixel dimensions and 2.3:1 maximum aspect ratio, but the game recommendation guidance calls for 9:16 portrait or 16:9 landscape, and the listing still needs a second device type. The current images alone do not meet those targets. No screenshot has been uploaded.
- Raw preview footage exists for both platforms. The Apple clip still needs accepted-resolution validation; no preview video has been uploaded.

## Remaining release gates

No store upload, submission, review approval, or public availability is verified. Apple signing, privacy-policy URL, copyright and review contact remain open. Google app-record creation still needs the account owner to complete both legal declarations; the Create App form is staged and neither checkbox was selected. The published support contact is `hi@mosselora.com`; support and marketing use the live apex. Resolve BidiSharp's shipping allowlist and include its MIT notice. Store privacy/data declarations from the final binary, age rating, availability, formal title clearance, current-build screenshots, physical-device QA, VoiceOver/TalkBack testing and intended-audience feedback remain open.
