# I164 — The memory card's continue arrow is always in reach

Date: 2026-11-25. Branch: feat/mosselora-i127-plus.

## Player (b152 critique, then checked on b174)

"Screen 3, the bottom bar: the flat teal '→' button is **cut off at the bottom edge**…"

On the memory card, the screen after every choice, the **continue arrow**, the one thing to press next, was cut off by the bottom edge at 100% text, and at **150% text almost entirely below the screen**. A player had to discover they could scroll to move on. Games keep the primary action in reach. A web page lets it sit wherever the content ends.

## PM decision

**Pin the memory card's action row (change-my-choice token and continue arrow) to the foot of the screen**, over a soft paper fade, at every text size. The page keeps an equal space at its end, so everything above can still scroll clear of the bar. Nothing else on the page changes.

## Dev

`EvergroveApp.PinToFoot(row)`: measures the row, creates a `Pinned actions` layer in the safe area (above the page, under the menu sheet and the chrome) with a flipped `TopFade` and a solid paper base, re-parents the row there (anchored bottom, 14pt margin, following the column in landscape), and adds an empty `Space for the pinned actions` of the same height to the page. `Rebuild` destroys the layer. The memory card calls it after building its actions. The QA tour still finds "Continue" (scene-wide search).

## Result (b175, iPhone 17 simulator, tours)

`I164-before.jpg` (b174: 100% cut off, bridge memory cut off, 150% nearly hidden) compared with `I164-after.jpg` (b175, both gitignored): at 100% and 150%, the token and the arrow sit fully visible at the foot, with the card and the path melting under the fade. The bottom-scrolled view (`04b`) shows the path and the "Rainwater" sign clear above the bar. Portrait, landscape and 150% tours pass (20/5/20). Verify passes.

**Not verified:** the landscape memory card (the landscape tour doesn't reach it), and VoiceOver order (the actions now come after the page in the tree, which matches reading order).
