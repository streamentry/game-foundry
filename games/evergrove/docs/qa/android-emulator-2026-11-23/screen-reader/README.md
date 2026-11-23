# EG-113 Android screen-reader hierarchy: emulator probe (2026-11-23)

## Verdict

**Android accessibility tree exposes 0 Mosselora nodes.** `uiautomator dump` of the running puzzle screen returned only the root FrameLayout with no text or content-desc, while the screenshot clearly shows the puzzle, Undo, Guide me, Restart stream and the scene art. TalkBack was enabled during the probe, and a TalkBack focus ring was visible on the scene art, so a screen reader was running. The likely gap is that `ScreenReaderBridge.Publish` never ran or never reached `AssistiveSupport`, or Unity's Android accessibility bridge does not expose the hierarchy to uiautomator. This probe cannot tell those apart. Next step: check in the app whether `ScreenReaderBridge.Active` is true under TalkBack, and whether Publish is called on the puzzle screen.

## Scope and limits

- Headless Android 15 emulator (`emulator-5554`, `sdk_gphone64_arm64`, arm64-v8a). Not a physical device.
- No human TalkBack user. TalkBack was enabled by settings only; no spoken output was verified.
- Development APK `com.streamentry.evergrove.playtest`, activity `com.unity3d.player.UnityPlayerGameActivity`. Not reinstalled, and no game code was changed.
- iOS VoiceOver is not covered.
- The node count in the in-app dev log (35 nodes, per EG-113) was not confirmed by this probe. Dev log not seen in logcat (`MOSSELORA_SR_NODES` count 0).

## Commands

```sh
ADB=/Volumes/SSD/unity/editors/6000.3.0f1/PlaybackEngines/AndroidPlayer/SDK/platform-tools/adb

$ADB shell pm list packages | grep -i -E "talkback|accessibility"
# TalkBack present: com.google.android.marvin.talkback

$ADB shell settings put secure enabled_accessibility_services com.google.android.marvin.talkback/com.google.android.marvin.talkback.TalkBackService
$ADB shell settings put secure accessibility_enabled 1
$ADB shell settings put secure touch_exploration_enabled 1

# Launch issues: a notification-permission dialog (Accessibility Suite) covered the launcher.
# Dismissed via tap on "Don't allow" (permission_deny_button). Then:
$ADB shell am force-stop com.streamentry.evergrove.playtest
$ADB shell am start -n com.streamentry.evergrove.playtest/com.unity3d.player.UnityPlayerGameActivity

# Unreadable-save banner was showing. Per the run instructions, only these were removed:
#   files/evergrove-world-v2.json, files/evergrove-world-v2.json.bak (8 and 9 bytes)
$ADB shell rm <those two paths>

sleep 25
$ADB exec-out screencap -p > screen-app-after-reset.png
$ADB shell uiautomator dump /sdcard/mosselora-a11y.xml
$ADB pull /sdcard/mosselora-a11y.xml

$ADB logcat -d | grep -c MOSSELORA_SR     # 0

# Restore
$ADB shell settings put secure enabled_accessibility_services null
$ADB shell settings put secure accessibility_enabled 0
$ADB shell settings put secure touch_exploration_enabled 0
```

Device: Android 15 image, arm64-v8a.

## Results

| Check | Result |
|---|---|
| TalkBack package present | Yes (`com.google.android.marvin.talkback`, version 16.0.0.738667889) |
| TalkBack enabled during app run | Yes (settings set, restored to null/0/0 at the end) |
| Accessibility Suite notification prompt | Blocked launch first; dismissed |
| Unreadable-save banner | Shown at first launch; cleared by deleting the two save files |
| Puzzle screen visible after reset | Yes (screen-app-after-reset.png) |
| uiautomator nodes with text or content-desc | 0 (dump is 2386 bytes; one `bounds` element, the root) |
| Nodes for tiles, Undo, Guide me, Restart | Not exposed |
| TalkBack focus ring | Visible on the scene art (green outline), so TalkBack was focusing something |
| Node focus or click probe | Not performed. The dump had no nodes to probe, so the focus step was skipped |

## Excerpt table

Intended nodes from `ScreenReaderBridge.Collect`, and what the Android tree showed:

| Expected role | Expected label | Android dump |
|---|---|---|
| Header | Mosselora wordmark | absent |
| StaticText | Water reaches 4/9 tiles · 10 open joins | absent |
| Button | Undo | absent (visible on screen) |
| Button | Guide me | absent (visible on screen) |
| Button | Restart stream | absent (visible on screen) |
| Button | "Row 1, column 1: corner pipe, connected to water." | absent |
| Button | "Row 1, column 2: ..." | absent |
| Button | "Row 1, column 3: ..." | absent |
| Button | "Row 2, column 1: ..." | absent |

The dump's only element is the root FrameLayout (`bounds="[0,0][1080,2400]"`), plus empty containers.

## Files

- `mosselora-a11y.xml`: uiautomator dump, after the save reset
- `screen-app-after-reset.png`: puzzle screen with TalkBack on
- `screen-before-dump.png`: notification prompt over the launcher, before dismissal
- `logcat-a11y-excerpt.txt`: accessibility-related logcat lines (no MOSSELORA_SR line)

Raw working copies are in `/Volumes/SSD/mosselora-builds/screen-reader-2026-11-23/`.

## Instrumented re-test

Build: development channel APK (`BuildOptions.Development`, so `DEVELOPMENT_BUILD` is set), built from this branch with `Evergrove.UnityClient.Editor.EvergroveBuild.Build` to `/Volumes/SSD/mosselora-builds/sr-apk/Android/Evergrove-Playtest.apk` (55.8 MB). Build log: `/Volumes/SSD/mosselora-builds/sr-apk.log` ("Exiting batchmode successfully now!").

Diagnostics added (development builds only, inside `#if UNITY_EDITOR || DEVELOPMENT_BUILD`): `MOSSELORA_SR_STATUS enabled=<isScreenReaderEnabled> reason=<awake|changed-to-on|changed-to-off>` and `MOSSELORA_SR_PUBLISHED <count> first=<label frame=...>`.

Commands (emulator-5554, Android 15, arm64):

```sh
adb install -r .../Evergrove-Playtest.apk          # Success
adb shell settings put secure enabled_accessibility_services com.google.android.marvin.talkback/com.google.android.marvin.talkback.TalkBackService
adb shell settings put secure accessibility_enabled 1
adb shell am force-stop com.streamentry.evergrove.playtest
adb logcat -c
adb shell am start -n com.streamentry.evergrove.playtest/com.unity3d.player.UnityPlayerGameActivity
sleep 25; adb shell uiautomator dump /sdcard/sr-dump.xml; adb logcat -d | grep MOSSELORA_SR
# Restore (the empty string "" is rejected by this adb shell; null was used)
adb shell settings put secure enabled_accessibility_services null
adb shell settings put secure accessibility_enabled 0
adb shell settings put secure touch_exploration_enabled 0
```

Logcat (`/Volumes/SSD/mosselora-builds/screen-reader-sr-2026-11-23/logcat-mosselora-sr.txt`):

```
MOSSELORA_SR_STATUS enabled=True reason=awake
MOSSELORA_SR_STATUS enabled=True reason=changed-to-on
MOSSELORA_SR_PUBLISHED 36 first=MOSSELORA frame=(x:44.94, y:2155.62, width:990.12, height:68.73)
MOSSELORA_SR_STATUS enabled=False reason=changed-to-off
MOSSELORA_SR_STATUS enabled=True reason=changed-to-on
MOSSELORA_SR_PUBLISHED 36 first=MOSSELORA frame=(x:44.94, y:2155.62, width:990.12, height:68.73)
```

Android side (`dumpsys accessibility`, same run): the window "Mosselora Playtest" is window 40 and is the active and focused window. TalkBack is bound and enabled, and `touchExplorationEnabled=true`.

`uiautomator dump`: 2386 bytes, 0 text or content-desc nodes. logcat shows why:

```
AccessibilityNodeInfoDumper: Skipping invisible child: ... boundsInScreen: Rect(0, 0 - 0, 0); packageName: com.streamentry.evergrove.playtest; importantForAccessibility: false; visible: false
```

The dumper prunes an invisible child together with its whole subtree. The Unity window's child is that invisible node with zero bounds, so the 36 published nodes are probably below it and are not shown by the dump. The dump cannot show whether they exist.

Interpretation:

- Unity side works: `isScreenReaderEnabled` turns on and off with TalkBack, the status callback fires both ways, and `activeHierarchy` is published with 36 nodes and non-zero screen frames. No managed exception or Unity accessibility error appears in logcat (only `AccessibilityManager:Internal_Update_Impl()` info lines).
- No bridge bug found in `ScreenReaderBridge.Publish` or `EvergroveApp` (status subscription, dirty flag, and reassignment on TalkBack on all run as the Unity docs require). No code change to the bridge was made.
- The "0 nodes" result in the first probe was a measurement limit of `uiautomator`, not proof that the nodes are absent. A third-party uGUI bridge (luanrobs, Screen Reader for uGUI) reports the same gap on an Android emulator: Unity publishes, `uiautomator` lists none of Unity's nodes.
- TalkBack's own log lines ("Failed to query component interface for required system resources: 6", "Could not find package com.google.android.aicore") show that the emulator's TalkBack is not fully provisioned. Speech output was not verified.
- Not verified: that TalkBack speaks or focuses the 36 nodes. This needs a TalkBack gesture run, on the emulator with a focus probe or on a device with a person listening.

Other finding (not changed here): `ProjectSettings.asset` has `AndroidMinSdkVersion: 25`, but the Unity docs state the Accessibility APIs are supported from Android 8.0 (API 26). On API 25 the bridge is expected to be inactive; decide whether to raise minSdk or to document it.

Conclusion: environment and measurement limit, not a confirmed bridge bug. Screen-reader wiring is verified on the Unity side; TalkBack behavior remains unverified.
