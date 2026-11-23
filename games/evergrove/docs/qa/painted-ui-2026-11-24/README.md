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

### Follow-up review fixes (b10)

An independent Haiku re-review of b9 marked choice cards resolved and raised three new issues. All three are now fixed:

- Change choice used circular arrows like Restart; it is now two opposite straight arrows (new Codex glyph, `art/generated/icons-ui-2.png`).
- The chime bell read as "notifications"; it is now a speaker with waves or a speaker with an X.
- The post-win "revisit water" undo sat between the payoff line and the choices, where it read as "go back"; it now sits below the choices.
- Board size uses a magnifier over a grid instead of a grid with a plus. Open-end rings use a heavier stroke so they stay visible on pale-blue wet tiles.

Evidence: `icons-v2-b10-150pct.png` (b10 tour at 150% text; all 17 screens produced).

### Learnability and robustness pass (b11/b12)

- **Icon cold-read (proxy, not human evidence):** a fresh Haiku agent saw only the 15 unlabelled icons (`icon-cold-read-sheet.png`). It read 8 with HIGH confidence and 7 with MEDIUM; none was LOW. The one wrong reading, the journal book as "how-to-play rules", is fixed with a memory-count badge on the book. "New garden" was read as "plant something"; it is guarded because its first tap only shows the worded erase confirmation.
- **Press-and-hold names:** holding any icon-only control for 0.45 s shows its localized name in a bubble. A hold that showed the bubble does not run the action. Verified on the simulator: the bubble appears above the board-size button, and the board stays at 1.2× after release (`b11-long-press-tip.png`). The size controls still show their language-neutral captions ("Aa 100%", "▦ 1.2×"); worded size labels would need new keys in all 30 catalogs and are left as a follow-up.
- **Screen-reader labels:** the app's semantic tree from the b11 tour (`MOSSELORA_SR_NODES 36`) lists the icon buttons by their spoken captions: Undo, Guide me, Restart stream, Language: English, Aa 100%, ▦ 1×, Gentle chime, Mira, Orin, Fen, Copy garden file, Restore from garden file. This is the app-side label path; VoiceOver itself was not run.
- **Cold-start bug fixed:** on the first build after launch the painted strip was ~3x too tall, because `CanvasScaler.scaleFactor` is not the live scale. Page width now uses the scaler's ScaleWithScreenSize formula, and board sizing uses the same, which gives identical tiles on current phones. Verified on cold launches with Standard and Large boards (`b12-cold-start-standard-large.png`).
- **Large board:** renders with the new skin; rings, markers and pipes scale with the tile.

### Paper, pips and code-review fixes (b13)

- **Codex paper grain:** a new Codex-generated seamless watercolor paper (`art/generated/paper-grain.png`, prompt `art/prompts/paper.txt`) becomes a tintable 9-sliced tile sprite (`tile-paper`). Dry, wet, solved and hint tiles now read as painted paper rather than flat fills. State colours still come from `MosseloraTheme.TileFill`. `b13-paper-board.png`.
- **No text badges:** hint depth (2 dots), text size (3 dots) and board size (2 dots) show their level as filled dots; the settings row has no text at all. The journal keeps its memory count. `b13-paper-tiles-pips.png` (125% text).
- **Independent Haiku C# review, four fixes:** a pipe could stay mid-swing after a snap (restart during a turn); the solve glide could run to a stale target after an unrelated rebuild (now cancelled by a rebuild counter); teach-mode tool buttons ignored text scale for their minimum height; and a hint-completed creek skipped the bloom, while the bloom flag could leak past a language-picker rebuild. The review's fifth point, idle `Update` loops, was kept as is: disabling those components would also stop pointer events, and each loop returns early.

### Open mouths on pipe ends (b14)

A final Haiku art-director review scored b13 at 5-7 out of 10: cohesion 7, icon clarity 7, board readability 7, button-text avoidance 6, delight 5 (from stills only). Its highest-impact fix was the open-end marker: a small ring in the tile corner that had to be matched to a pipe by eye. Each unmatched port is now drawn as a cream mouth in a dark collar on the pipe end itself, as in the reference design. The rule is the same as `PipeRules.Inspect` (a board edge, or a neighbor without the facing port). The count matches the sentence: stream 1 shows exactly 10 mouths for "10 open joins". The hidden "○" stays as the semantic source. `b14-open-mouths.png`.
