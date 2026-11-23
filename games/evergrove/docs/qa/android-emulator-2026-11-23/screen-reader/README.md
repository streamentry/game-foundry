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
