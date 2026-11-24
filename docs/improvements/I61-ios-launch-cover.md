# I61 — Native parchment cover replaces the black iOS launch gap

Date: 2026-11-24. Branch: feat/mosselora-painted-ui (PR #76).

**Player problem (I34, partial).** About 1.5 s of black between tapping the icon and the painted hub. I34's investigation showed the cause: **Unity 6's iOS trampoline has no splash view**. The Unity view is an opaque Metal surface that stays black until the first frame renders, so neither the launch storyboard colour nor a scene camera could help.

**Change.** A small native plugin, `Assets/Plugins/iOS/MosseloraLaunchCover.mm` (Objective-C++, display only, no data or network):
- On `UIApplicationDidFinishLaunchingNotification` it opens a **parchment cover window** above Unity, scene-aware on iOS 13+, showing the painted launch image (parchment, vine corners, wordmark) from I34.
- `EvergroveApp` calls `_MosseloraLaunchCoverDone()` (`DllImport("__Internal")`, iOS player only) after its first screen is laid out and a frame has ended. The cover then fades out over 0.25 s.
- A safety timeout hides the cover after 8 s even if the game never signals, so it can never trap the player.

**Evidence.** b38 cold launch on the iOS Simulator, screenshot burst (`I61-launch-cover.png`): home zoom → one dark frame from the **system's** launch animation → **parchment with the wordmark** → the painted hub. The ~1.5 s black stretch is gone. QA tour: 19 screens.

**Still open.** One system-drawn dark frame during the zoom; the compiled launch storyboard should cover it, and the simulator may be serving a cached launch snapshot. Verify on a physical iPhone. Android is not affected (its frames show the parchment camera from I34).
