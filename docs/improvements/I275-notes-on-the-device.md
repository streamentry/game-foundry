# I275 — Notes on the device

## PM decision
- iOS local notifications through Apple's UserNotifications framework only:
  no server, no SDK, nothing leaves the device. It's the same small
  first-party plugin approach as the share sheet and haptics.
- One identifier, replaced each time the game closes, so at most one note is
  ever waiting.
- Opening the game, by a note or not, clears it. A note therefore only arrives
  if you haven't visited before your hour.

## Dev
- `Assets/Plugins/iOS/MosseloraNotifications.mm`: request, permission,
  schedule and cancel. `UserNotifications` is declared as a framework
  dependency in its `.meta`.
- `GardenNotes.cs`: on/off, hour, the "not now" count, `Opened`, `Schedule`.
  Elsewhere it logs instead of scheduling.
- `EvergroveApp.OnApplicationPause` and `OnApplicationQuit` queue the note when
  leaving and clear it when returning; a cold start counts as an opening. The
  dev-only research hook moved into `ResearchPause`.
- In the tour, nothing is scheduled; it logs `MOSSELORA_GARDEN_NOTE_QA` with
  what would be said.
- Found in b292: the Xcode link failed with undefined `UN*` symbols, because
  the plugin `.meta`'s framework dependency wasn't applied to the
  UnityFramework target. The build script's "0 C# errors" hid it; the bundle
  had no `Info.plist`, so the tours couldn't launch.
- Fixed in b293: `Assets/Editor/MosseloraNotificationsLink.cs` adds
  `UserNotifications.framework`, weakly linked, in the iOS post-process step.
- Lesson: check the app bundle after every build, not only the C# error count.

## Result (b294)
- Tours complete in portrait, landscape and 150% text (84 shots each).
  - The river opens (the log shows "solves before 3 (wants 3)").
  - Mira's note waits sealed on the letterbox and is listed on the family
    page (`17u3`).
  - Settings shows the garden-notes switch (`17d`).
  - The soft ask (`17d2`) has legible hours. In b293 the hour labels were
    white on paper; fixed.
  - The tour logs the note that would be scheduled, for example "Orin: Your
    wisteria would deal you a fresh board of garden plots, if you fancy one."
- Found and fixed on the way:
  - the tour had stopped reaching the river step (b290 and b291);
  - the b292 Xcode link failure (see I275).
- Core tests: 347,183 assertions pass.
- Not verified: delivery of a real notification on a device, and the iOS
  permission prompt. Not yet tried by players aged 40+.
