# I127 — A neighbour speaks over the page, not into it

Date: 2026-11-25. Branch: feat/mosselora-i127-plus.

## Player (hands-on, iPhone 17 simulator, build b125 = main at 1bb08a0)

I played as a first-time player, not through the scripted tour. On the title painting I tapped Orin's speech mark.

- The game jumped to the first board with Orin's line in a card at the bottom. To make room, the **whole page scrolled up**: the Mosselora sign, the stone path and the chapter name went off screen, and the top of the board's stone frame was cut off under the paper band.
- After I tapped the check, the page **stayed scrolled**. Mira's tip bubble ("Blue waves = spring…") was clipped at the top, and there was a blank strip of paper under the village.
- How it felt: like a web page whose layout jumps when a banner opens. Talking to a neighbour should feel like someone leaning over the garden wall, not like the page moving under my finger.

## PM decision

This is the most visible break of "immersive, not like a web" in the first minute, and it happens on the first neighbour a player meets. Fix it before any new feature.

- **Do:** the neighbour's line floats over the page and never joins the scrolling column, so talking can't move the board.
- **Delete:** the page-turn crossfade when someone speaks (a whole-page fade for one speech bubble), and the jump-to-card scroll.
- **Smart navigation:** tapping anywhere outside the card closes it, as the check does. The check stays for people who look for a button.
- **Not now:** a full redesign of the village row.

## Dev

`EvergroveApp.cs`:
- `NeighborConversation()` now builds into a `Neighbor speaking` layer inside the safe area (above the page, below the menu sheet and the chrome buttons), not into `content`. The card is anchored to the bottom of the column with a 14pt margin, its height is fitted to the line, and in landscape it follows the right-hand column.
- A light ink veil (10%) dims the page behind the card. A new `TapCatcher` (a plain pointer handler, not a `Button`, so screen readers don't list an unlabelled control) closes the conversation on any tap.
- The card rises 12pt and fades in over 0.22s the first time each neighbour speaks (skipped when gentle motion is off and in the QA tour).
- `speakingWith` is no longer part of the screen key, so speaking doesn't crossfade the page. `OpenConversation` no longer sets `revealNewScreen`. `Rebuild` destroys the layer, so the scroll position is kept as it was.

## Result (b128, iPhone 17 simulator, fresh install)

- I tapped Orin on the title painting. The board opened with the Mosselora sign, the stone path, Mira's tip and the whole board still in view, and Orin's line on one paper card at the bottom. Nothing behind it moved.
- First attempt (b127): the portrait stood outside the card, and the undo button showed beside his face. Fixed in b128, which puts the portrait and the words on one card with a stronger lift.
- A tap on the sign closed the card, and the page stayed exactly where it was.
- The scripted tour still captures all 20 screens. `verify_evergrove_project.py` and the .NET rules tests pass.
- Local screenshot: `docs/improvements/I127-neighbor-floats.png` (gitignored).

**Not verified:** VoiceOver order with the card open, landscape and iPad placement, and real hardware.
