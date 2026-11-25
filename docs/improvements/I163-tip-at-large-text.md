# I163 — Mira's tip is never empty at large text (fixes I151)

Date: 2026-11-25. Branch: feat/mosselora-i127-plus.

## Player (150% text tour, b173: the setting many 40+ players will choose)

On the first board, **Mira's tip bubble was empty**: a blank parchment box beside her portrait. With no pictures and no words, the first-ever screen at large text had no instruction at all.

## Cause: my own regression from I151

I85 hid the tip's **picture row** at larger text and let the **sentence** carry the rule, to save height. I151 then hid the **sentence** at every size (pictures only). Together, nothing remained at 125–150%. The I151 test only looked at 100%.

## PM decision

- **Pictures at every size** (the I151 decision stands).
- **At larger text, the sentence returns under the pictures.** A player who asks for bigger text is the player most likely to want the words, and the extra height is acceptable there: at 150% the board still fits above the tools.

## Dev

`EvergroveApp.CoachBubble`: `showPicture` is always true. The sentence is hidden (transparent, layout-ignored) only when `TextScale.Percent <= 100`.

## Result (b174, iPhone 17 simulator)

`I163-tip-at-large-text.jpg` (gitignored): 150% before (empty bubble, b173), 150% after (pictures plus the four-line sentence, board and tools still on screen), and 100% (pictures only, unchanged). Portrait, landscape and 150% tours pass. Verify passes.

**Process fix:** the 150% tour joins the regular checks for any change to the first board or the tip (added to `scratchpad/bt.sh` from now on).
