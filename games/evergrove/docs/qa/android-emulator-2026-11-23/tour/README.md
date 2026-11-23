# Android emulator QA tour — 2026-11-23

**Device:** Android 15 (API 35) emulator "mosselora_qa", arm64, 1080×2400. **Emulator, not a phone.** Development APK, package `com.streamentry.evergrove.playtest`.

**Launch method (verified):** `adb shell "am start -n <pkg>/com.unity3d.player.UnityPlayerGameActivity --es unity '-mosseloraQaTour /data/user/0/<pkg>/files/tour'"`. The `--es unity` extra is picked up as a command-line argument. Note: `-e` with a value containing spaces did not work, and the value must be quoted for the device shell.

**Result: PASS.** logcat shows 10 `MOSSELORA_QA_TOUR_SHOT` lines and `MOSSELORA_QA_TOUR_DONE 14`, and the process quit on its own. 14 PNGs pulled with `run-as cat`. SHA-256 in `sha256-tour-pngs.txt`.

| File | Observed |
| --- | --- |
| 01-chapter1-start.png | Stream 1 at 4/9 water. Board fully visible. The background is dimmed, unlike the iOS run. The language/size/standard row is cut off at the bottom. |
| 03-chapter1-solved-choice.png | "The water flows through every tile." Three village choices shown. |
| 07-chapter2-after-shade-tree.png | Stream 2 "Living roots" with the protected tile, root badge and "Tree roots anchor this pipe. It cannot rotate." Board-first layout. A small glyph sits at top-left. |
| 10-journal.png | "Our choices" shows the four chapter choices in order. |
| 11-ending.png | "Our village thrives." Ending screen with memories and dev tools. |

**Android-specific notes:** the dimmed frame in 01 and the stray glyph in 07 were not checked against iOS. The bottom controls run past the fold on 01 and 03, which is a layout concern to check on a phone.

**Not verified:** physical devices, touch accuracy, TalkBack, performance, real safe areas.
