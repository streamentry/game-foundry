# iOS Simulator QA — 2026-11-23

**Build:** Unity 6000.3.0f1 development player, IL2CPP iOS Xcode export (`EVERGROVE_IOS_SDK=simulator`, arm64), compiled with Xcode 27.2 for the simulator with code signing disabled.
**Device:** iOS Simulator, iPhone 17 Pro, iOS 26.5 (402×874 pt). **This is not a physical phone.**

| File | What it shows |
| --- | --- |
| `01-chapter2-before-tap.png` | First iOS run. It shows stream 2 after a Bridge choice; the saved game says chapter 1 was solved in the simulator panel before capture. The board-first layout problem is visible: settings, the illustration and four text lines push the board below the fold. |
| `02-chapter2-after-tap-rotated.png` | A tap injected at (201, 771) pt rotated the top-middle pipe from horizontal to vertical. This shows touch input reaching the IL2CPP build. |

**Not shown:** physical-device touch accuracy, safe areas on real hardware, performance, battery, VoiceOver, or interrupted-save recovery (M2).

## Second run: after the board-first fix (fresh install)

The app was uninstalled, rebuilt and reinstalled. A fresh install opens at chapter 1. This confirms that the stream-2 state in `01` came from earlier play in the panel, not automatic progress.

| File | What it shows |
| --- | --- |
| `03-fresh-install-chapter1-board-first.png` | Chapter 1 on first launch. The whole board, the water-reach line and Undo/Guide me/Restart fit above the fold at 402×874 pt. |
| `04-chapter1-solved-6-taps-choice.png` | Six injected taps solved chapter 1 ("The water flows through every tile."). The three village choices appear. |
| `05-consequence-first-shade-tree.png` | After "Plant a shade tree": the illustration shows the new tree, then the consequence line, Mira's memory card, Continue, and settings last. |
| `06-chapter2-living-roots-landmark.png` | Stream 2 "Living roots", board-first. The protected tile has a heavy frame and a root badge, directly under "Tree roots anchor this pipe." |

This is simulator evidence of the complete core loop running in the IL2CPP iOS build with touch input. It is **not** physical-device, accessibility or player evidence.
