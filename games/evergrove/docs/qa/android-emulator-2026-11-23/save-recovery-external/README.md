# Android emulator save recovery (correct save path), 2026-11-23

**Why this folder exists:** the first Android save-recovery run (`../save-recovery/`) wrote its fixtures to the app's *internal* `files/` directory. On Android, Unity's `Application.persistentDataPath` is the **external** app directory, `/storage/emulated/0/Android/data/com.streamentry.evergrove.playtest/files/`. The game never read those fixtures, so that run is **inconclusive and superseded** by this one.

**Build:** development APK sha256 `8acd5e15…`, with engine stripping disabled. **Device:** Android 15 emulator (API 35, arm64, Pixel 7 profile, headless). **Not a physical phone.**

| Scenario | Steps | Result |
| --- | --- | --- |
| B. Progress survives force-stop | Pushed a valid v4 chapter-2 Bridge snapshot (the iOS fixture, sha256 `f4466fea…`), launched, then `am force-stop` and relaunched | **PASS.** Stream 2 "Bridge foundation" with the framed bridge landmark (`B1`). Identical after relaunch (`B2`, same PNG hash). Save hash unchanged |
| C1. Corrupt primary, valid backup | Truncated the primary save to 17 bytes; `.bak` = the valid fixture | **PASS.** Restored stream 2 Bridge from the backup (`C1`); no crash |
| C2. Both corrupt | Primary `garbage`, backup `garbage2` | **PASS (as designed).** Warning banner "Progress could not be saved. Check device storage.", plus the explicit-reset button "Explore another garden". The dev log reads "Unreadable Evergrove save retained. Saving is disabled until an explicit reset." Both corrupt files were left byte-for-byte untouched; no FATAL/AndroidRuntime lines |

**Finding (copy):** in C2 the banner blames device storage, but the cause is an unreadable save. Players may look for a storage problem that doesn't exist. Consider a dedicated "We couldn't read your saved garden" message (it needs a localization key).

**Not covered here:** scenario A (clean relaunch) and D (background and resume) passed in the first run, which did not depend on fixture paths. Power loss mid-write, physical devices and low-storage conditions remain open (M2).
