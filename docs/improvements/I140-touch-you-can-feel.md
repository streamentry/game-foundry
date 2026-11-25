# I140 — A turn you can feel: light haptics on turn and finish

Date: 2026-11-25. Branch: feat/mosselora-i127-plus.

## Player

Turning a tile looks right: the pipe swings a quarter, and with sound on there's a wooden tock. But sound is **off by default** (the game is often played somewhere quiet), so for most players a turn is silent and weightless under the thumb. A finished stream has the butterfly, the bloom and the chime (if sound is on), but the hand feels nothing. "Super immersive, not like a web" includes touch, and the backlog already names it: EG-013, "Touch rotation can feel imprecise… light tick per valid rotation and distinct completion pulse."

## PM decision

- **A light tick on every valid turn** and **a distinct success pulse on a finished stream**, never as the only signal. Motion always shows the same moment, and sound can too.
- **Off switches without a new button or string:** haptics follow the existing **gentle motion** setting (on by default), and both platforms' own system haptics settings are honoured (iOS `UIFeedbackGenerator`, Android `performHapticFeedback`). EG-013 asks for "easy toggle and OS respect". A separate "touch feedback" toggle would need a new icon and new strings in 30 catalogues. That's deferred until a player asks for haptics without motion.
- A turn that's refused (a protected landmark) gives no tick. The landmark already wiggles.

## Dev

- New `Assets/Plugins/iOS/MosseloraHaptics.mm` (+ PluginImporter `.meta`, iOS only): `_MosseloraHapticTick` (a reused, prepared `UIImpactFeedbackGenerator` Light) and `_MosseloraHapticSuccess` (`UINotificationFeedbackGenerator` Success).
- New `Assets/Scripts/Unity/Haptics.cs`: `Tick()` / `Success()`. On iOS: `DllImport("__Internal")`. On Android: on the UI thread, the decor view's `performHapticFeedback(CLOCK_TICK)`, with `CONFIRM` (API 30+, else `VIRTUAL_KEY`) for success. On any failure it logs one warning and goes silent. The editor and other platforms do nothing.
- `EvergroveApp.TapTile`: `Haptics.Tick()` after a valid rotation (with `PlayTock`). `AfterPuzzleAction`: `Haptics.Success()` with the solve chime. Both only when `gentleMotion` is on.

## Result

- **iOS (b145, iPhone 17 simulator):** the build links `MosseloraHaptics.mm` (it's in the generated Xcode project). The scripted tour presses tile buttons (`onClick`, through `TapTile`) and solves boards, so both calls run. All 20 screens were captured, with no warnings.
- **Android (a140, Android 15 emulator):** the APK builds, so the Android branch compiles. The same tour ran to `MOSSELORA_QA_TOUR_DONE 20`, with no `haptics unavailable`, `AndroidJavaException` or `FATAL` lines in logcat.
- Verify and the .NET tests pass.

**Not verified:** the feel itself. Neither the simulator nor the emulator has a haptic engine, so the light tick and the success pulse need a real iPhone and Android phone. Also untested: whether a tick on every turn is welcome or tiring over a 16-tile board, which needs the M3 session. Stop rule (EG-013): if players find it annoying or it adds nothing to touch confidence, remove it.
