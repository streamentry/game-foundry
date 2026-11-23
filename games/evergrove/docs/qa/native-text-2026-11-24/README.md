# Native text rendering prototype (EG-006 iteration 7), 2026-11-24

## Method

- Scope: Bengali (`bn`) only. The native path is gated by `NativeTextLabel.Applies` (`GameLocalization.Current == "bn"` on iOS or Android device builds). Other locales and Unity Editor/desktop keep uGUI Text.
- iOS: `Assets/Plugins/iOS/MosseloraNativeText.mm` (CoreText, system font cascade with `bn` language tag, no bundled fonts, no network, no file writes). Output is bottom-up straight-alpha RGBA.
- Android: `NativeTextBridge.cs` drives `android.text.StaticLayout$Builder`, `TextPaint`, `Canvas` and `Bitmap` through `AndroidJavaObject`, encodes PNG in Java, and loads it with `ImageConversion.LoadImage`. No Java/Kotlin source files. `android.graphics` only, no network.
- Integration: `NativeTextLabel` keeps the original `Text` (disabled, holding the logical string) so layout and `ScreenReaderBridge` still read the real text. It draws a `RawImage` centred in the same node, sets `LayoutElement.preferredHeight` from the measured pixels, and re-renders on text, width, size, colour, weight or alignment change. Any native failure re-enables the uGUI Text for that label.
- Integrated at `EvergroveApp.Label`, the Button caption path and the save-warning banner.
- Build: Unity 6000.3.0f1 batch builds, iOS simulator (`EVERGROVE_IOS_SDK=simulator`, xcodebuild Debug arm64) and Android (development APK).
- QA: `-mosseloraQaTour` on the iOS simulator (UDID 938A516A-5AD8-459E-A727-503402F3D304, `AppleLanguages bn`) and on Android emulator serial `emulator-5570` (AVD `mosselora_qa`, API 35, locale set per app with `cmd locale set-app-locales`).

## Results

| Platform | Build | Verdict | Evidence |
| --- | --- | --- | --- |
| iOS simulator | Debug arm64, fixed build (alpha pass-through) | Frame 01 on the fixed build: pass for conjunct shaping and layout, hidden spoken title does not show. Frames 04 and 10 were reviewed on the pre-alpha-fix build (conjuncts and layout pass); the fixed-build copies were not re-viewed. | `ios/` |
| Android emulator | Development APK after the `StaticLayout$Builder` fix | Frames 01 and 10 reviewed: pass for conjunct shaping and layout. Frame 04 copied but not yet viewed. | `android/` |

Conjunct check (iOS and Android): সিদ্ধান্ত, জন্য and স্মৃতি form without a visible hasanta in the journal and chapter headers.

Earlier iterations: the first Android attempt used the wrong class (`StaticLayout` instead of `StaticLayout$Builder`). Every bn label fell back to uGUI. The fix made frames render natively. The first iOS attempt drew the hidden spoken title (alpha ignored). That is fixed by passing alpha to the plugin.

## Exceptions seen

- Android development console: `Could not produce class with ID 115` flooded logcat (tens of thousands of lines) in the pre-merge APK, in English and bn. Root cause: stripping removed the TextCore classes that the uGUI font path needs. Main already carries `Assets/link.xml` preserving `UnityEngine.TextCoreTextEngineModule`, `TextCoreFontEngineModule` and `UnityEngine.UI`. The branch was built before merging main, so it lacked that file. After `git merge main`, a fresh install of the merged build on emulator-5570 gives 0 class-115 lines in English and 0 in bn (logcat counts, 35 s after launch). The bn screenshot shows natively formed conjuncts and no overlay.
- Android emulator `ImmersiveModeConfirmation` appears on first launch and was dismissed by tap.
- `adb` daemon restart: an emulator (`emulator-5560`) owned by another session was used by mistake before the serial was corrected. Its results were discarded. The reported Android results come only from `emulator-5570` with a build installed and verified by lastUpdateTime.
- iOS: the simulator container was reset during the session. The reported iOS frames come from a re-run after reinstall.

## Limits

- Bengali only. Bengali stays off the shipping allowlist.
- Native reviewer approval is required before any claim on conjunct correctness or the text itself. Visual check only.
- Physical iPhone and Android phone QA: not run.
- Accessibility: the Text component keeps the real string for `ScreenReaderBridge`. Actual VoiceOver and TalkBack reading of the bitmap was not run.
- Binary size delta: not measured.
- Bold weight is approximated with CoreText bold traits and `Typeface.DEFAULT_BOLD`. Font metrics can differ slightly from uGUI.
- Texture cache is per label. A label re-renders only when its key changes. There is no cross-label cache.
- Unity Editor and desktop fall back to uGUI (no native path).
- `ProjectSettings.asset` changes from the build (SDK version bump) were reverted. `Packages/manifest.json` gained `com.unity.modules.androidjni`, which the Android native path needs.

## Lead fix: empty chapter pill (2026-11-24)

The lead checked the bn Android capture after the merge. The chapter pill ("ঝরনা 1 / 12") was an empty pale square. The pill's HorizontalLayoutGroup sizes itself to its child's preferred width. A disabled uGUI Text reports none, so the label width was 0 and the native renderer never ran (it skips widths ≤ 0). The fix is in `NativeTextLabel`: when the parent row controls the child width without force-expanding it, the label sets its `LayoutElement.preferredWidth` from the uGUI estimate plus a 12% margin. The estimate is recomputed when the text changes and cleared if the native path fails. Full-width labels are unaffected.

- Evidence: [android/bn-chapter1-pill-fixed.png](android/bn-chapter1-pill-fixed.png). Emulator-5570, fresh install, bn app locale. The pill shows formed text, and the logcat has 0 `class with ID 115` lines and no Unity exceptions.
- Checks after the fix: `verify_evergrove_project.py`, `check_evergrove_docs.py`, and the Unity batch smoke (`VerifyProject`) pass. The fix is in Unity-only code, so the C# Core tests are unaffected.
- iOS Simulator recheck (UDID 938A516A…, fresh Debug arm64 build from main): the bn pill reads "ঝরনা 1 / 12", the tagline conjuncts are formed, the English pill is unchanged ("STREAM 1 / 12 · First Bend"), and there are no console errors. Evidence: [ios/bn-chapter1-pill-fixed.png](ios/bn-chapter1-pill-fixed.png). The check was done by an AI agent and the capture was reviewed by the lead.

## Screen-reader labels with native bn text (Android emulator, 2026-11-24)

`ScreenReaderBridge` skips only inactive GameObjects (`ScreenReaderBridge.cs:160`). It reads `Text.text` even when the Text component is disabled (lines 167–169, captions at 203–208). `NativeTextLabel` keeps the logical string on the disabled Text. With TalkBack enabled on emulator-5570, `MOSSELORA_SR_STATUS enabled=True` was logged and 38 nodes were published (36 in the earlier English run). A `-mosseloraQaTour` run logged every label ([android/sr-nodes-bn.txt](android/sr-nodes-bn.txt)). Labels are Bengali in logical Unicode order, for example "ছোট ছোট সিদ্ধান্ত। প্রাণবন্ত একটি স্থান।", "ঝরনা 1 / 12", and tile buttons such as "সারি 1, কলাম 1: বাঁকা পাইপ, পানির সঙ্গে সংযুক্ত।". The app name node reads "MOSSELORA" in Latin script.

- Native-review note: the prompt uses জল for water, but the tile labels use পানি. A native reviewer should choose one.
- Not verified: TalkBack speech, focus order and gestures. No person listened. uiautomator still can't see Unity's nodes. iOS VoiceOver is not covered.
