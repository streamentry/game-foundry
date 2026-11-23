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
