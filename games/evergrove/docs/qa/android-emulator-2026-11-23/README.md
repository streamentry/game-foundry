# Android emulator QA — 2026-11-23

**Build:** Unity 6000.3.0f1 development APK (IL2CPP, unsigned debug), 67,872,862 bytes, sha256 `225fde49ce4302d5e0bb379044ad032da29bfb311769bf275c5704d02b143fb3`.
**Device:** Android Emulator, Android 15 (API 35) `google_apis` arm64-v8a, Pixel 7 profile (1080×2400), headless with SwiftShader GPU. **This is not a physical phone.**

| File | What it shows |
| --- | --- |
| `01-before-fix-blank-stripped-engine.png` | **Bug found:** the first APK launched (game loop running, no crash) but drew nothing. logcat showed endless `Could not produce class with ID 115 … stripped from the build` from `EvergroveApp.Label → TextSettings.GetCachedFontAsset → FontAsset.CreateFontAssetOSFallbackList`. Android players would have seen a blank screen. |
| `02-after-fix-chapter1.png` | After `PlayerSettings.stripEngineCode = false` in `EvergroveBuild`: chapter 1 renders board-first, with zero class-115 errors and no Unity errors. |
| `03-chapter1-solved-6-taps-choice.png` | Six `adb shell input tap` events solved chapter 1 ("The water flows through every tile."); the village choices appear. |

**Cost of the fix:** the APK grew from about 44 MB (stripped) to about 68 MB. Re-enabling stripping safely, with a `link.xml` or a preserved TextCore type, is a possible size optimization once a device test confirms it.
**Not verified:** physical Android devices, TalkBack, performance or battery, and save interruption on Android.
