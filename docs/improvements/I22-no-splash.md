# I22 — Launch goes straight to the painted hub

Date: 2026-11-24. Branch: feat/mosselora-painted-ui (PR #76).

**Player problem.** Every launch opened on a dark "Made with Unity" splash before the game, breaking the calm storybook threshold built in I11 and I17.

**Change.** In `ProjectSettings.asset` the Unity splash screen and logo are disabled (allowed for all plans since Unity 6). The splash background and the iOS launch-screen background colour are set to the page parchment (`#F9F2E3`).

**Evidence.** b24 launch recording at 0.4 s per frame (`I22-launch-frames.png`): home screen → a brief black system launch frame → the painted hub. The ~2 s Unity splash is gone.

**Known gap.** The iOS launch storyboard still draws black for about 0.8 s. The "Default" launch-screen type ignores the background colour, so a custom launch image or storyboard is needed. Follow-up.
