# I34 — Parchment launch storyboard and paper camera (black frame persists)

Date: 2026-11-24. Branch: feat/mosselora-painted-ui (PR #76).

**Player problem (I22 gap).** Between tapping the icon and the painted hub, iOS showed about a second of black. For a calm game, the threshold should be the same warm paper as the game.

**Change.**
- **Launch storyboard.** A post-build editor hook, `Assets/Editor/MosseloraLaunchScreen.cs` (`[PostProcessBuild]`, iOS only), writes the parchment background colour into Unity's exported `LaunchScreen-iPhone/iPad.storyboard`. Unity exported white with a 1×1 placeholder, and its launch-screen settings did not take. The hook also copies a painted launch image (`brand/launch-portrait.png`: parchment, vine corners, wordmark, built by the art pipeline) over the placeholder. Build log: `MOSSELORA_LAUNCH_SCREEN parchment True`. The exported storyboard and the compiled `.app` contain both.
- **Paper camera.** The scene had **no camera**, so Unity cleared to black until the UI canvas existed. A paper-clearing camera (`cullingMask 0`) is now created at `BeforeSplashScreen`.

**Result: partial.** Screenshot bursts on the existing simulator, and on a **brand-new** simulator (no launch-snapshot cache), still show about 1.5 s of black before the hub (`I34-launch-probe.png`). So the black comes from a stage neither change controls, most likely Unity's own first-frame view on iOS 26 before the player loop renders. **Not fixed.** The next step is a physical device check (the simulator may differ) and, if it persists, the iOS trampoline's splash view controller.
