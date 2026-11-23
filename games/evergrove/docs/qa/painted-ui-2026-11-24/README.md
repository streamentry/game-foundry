# Painted storybook UI — iOS Simulator check

Date: 2026-11-24. Scope: visual check of the painted UI skin against the design references in `games/evergrove/screenshots/`. Not a device, accessibility or audience test.

## What changed

- Original watercolor art generated with the Codex `imagegen` skill (built-in image tool); masters, prompts and provenance in [`art/`](../../../art/README.md).
- Page: warm parchment (`#F9F2E3`), vine-corner header with a painted wordmark (the localized title stays as text for screen readers), sage stream pill.
- Board: rounded parchment tiles with an edge and soft shadow; copper (dry, dashed) and teal (connected, solid) pipes with couplings; painted spring and sprout markers; open-end markers drawn as rings.
- Actions: Undo (forest), Guide me (teal), Restart stream (terracotta) in one row; secondary actions are parchment with ink captions.
- Village strip: painted dry creek gives way to the flowing creek in proportion to real spring reach; saved choices add painted trees, bridges and wildflowers.
- Choice cards, journal and garden file use painted icons and toned actions; the ending opens under a village painting.

## Method

- Development iOS Simulator build from branch `feat/mosselora-painted-ui` (`EvergroveBuild.Build`, `EVERGROVE_IOS_SDK=simulator`, `xcodebuild` Debug arm64).
- Dedicated simulator "Mosselora Painted QA" (iPhone 17 Pro, iOS 26.5), English. QA tour (`-mosseloraQaTour`) produced all 17 PNGs; `01-first-launch.png` is a normal launch (the tour's first frame is captured during the launch fade and looks grey).

## Findings

| Screen | Verdict | Note |
| --- | --- | --- |
| 01 first launch | PASS | Header, pill, board, actions and how-to fit the first viewport; strip begins below the fold, as the board-first rule requires. |
| 03 choice | PASS | Three illustrated cards, no preselected choice. |
| 04 memory | PASS | Strip shows the new tree; memory card and teal Continue. |
| 07 chapter 2 | PASS | Tree from chapter 1 visible in the strip; protected landmark frame unchanged. |
| 10 journal | PASS | Each chapter carries its choice icon. |
| 11 ending | PASS | Ending card under the village painting. |
| 14 garden file | PASS | Copy (forest) and Restore (teal) buttons. |

Fixed during this check: the English runtime face has no "○" glyph, so open-end markers were invisible before this change (also in the 2026-11-23 build); they are now drawn as rings.

## Not verified

- Physical iPhone/Android, Android fonts (iOS headings use the runtime face in bold; the optional rounded display face was not found on the simulator), 125/150% text and Large board with the new skin, RTL/CJK locales with the new skin, VoiceOver.
- Age fit not validated: no adults 45+ have seen this skin.
- Contrast: token pairs computed (see `DESIGN-SYSTEM.md`); not measured on rendered pixels.

## Icon-first build (b6, 2026-11-24)

Same simulator method as above (iPhone 17 Pro, iOS 26.5, English). The QA tour produced all 17 screens. The build uses a Codex-generated 16-glyph icon set and painted neighbor portraits (masters in `art/generated/icons-ui.png` and `neighbors.png`).

- Stream 1 labels the three tools with small captions under the icons (Undo, Guide me, Restart stream). From stream 2 they are icon-only. This is deliberate progressive disclosure, responding to an independent review that flagged unlabeled icons as a risk for players aged 45 to 75.
- Droplet meter: one droplet per tile, filled teal as water reaches it. The text line below it is unchanged.
- Open-end rings are enlarged to 18 pt in terracotta.
- Memory screen: icon-only swap and arrow (continue). Settings row of four icons: globe, text size, board size, chime.
- Neighbors are portrait medallions with names.
- Motion (pipe swing on tap/undo, press squash, creek fade, solve bloom) is in the build but not visible in still screenshots and not verified frame by frame.

### Screens

| File | Verdict | Note |
| --- | --- | --- |
| icons-01-first-stream.png | PASS | Captions under all three tools; 4/9 droplets; open-end rings visible. |
| icons-07-chapter2.png | PASS | Tools are icon-only; root-anchored pipe note; 1/9 droplets. |
| icons-04-memory.png | PASS | Icons and settings row render. The "(dev)" controls below are development-build only (`UNITY_EDITOR \|\| DEVELOPMENT_BUILD`; verifier release guard OK). |
| icons-10-neighbors-journal.png | PASS | Choice icons and Mira, Orin, Fen medallions render. Dev-only controls as above. |

### Not verified

- Icon comprehension by adults 45+ (age fit not validated).
- VoiceOver reading of hidden captions on a device.
- Physical iPhone and Android.
- 125/150% text with captions.
- Motion frame by frame.

## Verification pass (b8/b9, 2026-11-24)

| Check | Result | Evidence |
| --- | --- | --- |
| Pipe swing on tap and undo, frame by frame | PASS: a tap tints the tile warm, then the pipe swings clockwise over about 6 frames (~200 ms); Undo swings it back counter-clockwise. | `motion-tap-undo-frames.png` (30 fps frames from a simulator recording, b7) |
| Solve payoff | FIXED, then PASS. Before the fix, the solving tap jumped straight to the choices in the same frame, so the bloom and the full creek were never seen. Now the solved board turns green, all droplets fill, tiles bloom outward from the spring, the board holds ~1.25 s, then glides (0.6 s) to "What will you help grow?". A player scroll during the hold cancels the glide. | `motion-solve-hold-glide-frames.png` (b8) |
| Pressed state | FIXED: the grey multiply looked dirty on parchment; it is now a warm tint. | same frames |
| 150% text, all 17 tour screens | FIXED, then PASS. The text-size badge wrapped ("150 / %") and the memory title collided with the leaf sprigs; both are fixed in b9. | `text150-before-fix.png`, `text150-after-fix.png` |
| Android | PASS (emulator, not a phone): development APK on an Android 36 emulator (1080x2400), QA tour `MOSSELORA_QA_TOUR_DONE 17`. Icons, portraits, droplets and the first-stream captions render in Bengali. | `android-bn-icons.png` |
| Remaining text buttons | Garden-file copy/restore and "explore another garden" are now icon-only. Words appear only on the erase confirmation, next to an icon-only cancel. The unreadable-save recovery flow keeps words on purpose (rare, data-loss risk). | b9 tour |

Process note: the Android run collided with another session's Bengali test on the same emulator; that session was told its run used this build.

Still not verified: icon comprehension with adults 45+ (age fit not validated), VoiceOver/TalkBack on a device, physical phones, Large board with the new skin.
