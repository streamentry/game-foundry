# I125 — Android plays the whole tour and speaks in Noto Serif

Date: 2026-11-25. Branch: feat/mosselora-painted-ui (PR #76).

## Why

Android was the remaining unchecked platform for I112–I124. I117's book serif (Charter, falling back to Georgia) does not exist on Android, so Android players would get the plain sans for the story.

## Android check (emulator, `mosselora_painted`, 1080×2400)

- Built the playtest APK with the existing `EvergroveBuild` (a124, then a125). Ran the **full QA tour on Android** by passing the dev option through Unity's launch-intent `unity` extra: `am start … -e unity '-mosseloraQaTour <app files dir>'`. Result: `MOSSELORA_QA_TOUR_DONE`, 20 screens.
- The painted stone path with spring and stream, the board, icon buttons, memory card, journal and autumn village all render as on iOS (`I125-android-tour.jpg`, local only).

## Change: the book serif on Android too

- `BookFont()` now looks for `Charter`, `Georgia`, then **`Noto Serif`**, which Android ships in `/system/fonts`. It prefers an exact family match, so a heavier cut ("Charter Black", "Noto Serif Bold") is never picked, and skips Sans, Bold and Italic variants. Still nothing bundled.
- Dev builds log the OS serif candidates, so the choice is visible in QA.

## Verification

- Android a125: `Mosselora book face: Noto Serif` (an exact match among 19 Noto Serif candidates). The memory card and journal speak in the serif while labels keep the sans (`I125-android-serif.jpg`, a124 vs a125, local only). Noto is a little wider, so one journal line wraps to three; the journal scrolls, and the memory lead still fits on one line.
- iOS b125: `Mosselora book face: Charter` (unchanged); portrait tour DONE (20); 0 `error CS`.
- C# tests: 50,072 assertions pass. The verifier reports OK.

## Not verified

- A physical Android or iOS device, players aged 40+ (**age fit not validated**).
