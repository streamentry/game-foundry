# I159 — The demo finger no longer hides the pipe

Date: 2026-11-25. Branch: feat/mosselora-i127-plus.

## Player (Haiku critique of b167, as a 57-year-old Gardenscapes player)

"Screen 1… **The hand graphic sits on a tile and hides the pipe I need to see.**"

I147 put the demo finger at the tile's centre, right over the sprout tile's channel: the one pipe the player has to read in order to decide how to turn it. My I147 log had flagged this as a risk.

The art-director half of the same critique proposed removing the hand and the picture card and "teaching in the world" with animated water instead. That contradicts the I151 blind A/B, where the reader with the hand found the right first tile. Both are weak evidence, so the cheaper, reversible fix comes first.

## PM decision

Keep the demo, but **move the whole hand into the tile's lower-right quarter**, the part of a tile no channel ever crosses (arms run along the centre lines). It's slightly smaller (0.36 of a tile). It still presses the tile in time with the turning arrow (top-right corner), and the pipe stays fully visible. It stays inside its own tile, so neighbouring tiles never cut it off.

## Dev

`EvergroveApp.TurnMark`: hand anchor `(0.5, 0.5)` with offset becomes anchor `(0.74, 0.36)` with the fingertip pivot. Size goes from 0.46 to 0.36 × the tile.

## Result (b168 → b169, iPhone 17 simulator, scripted tours)

- b168 tried hanging the hand off the tile's corner. The pipe was clear, but the tile below drew over the hand and cut it off.
- b169 (`I159-hand-clear-of-pipe.jpg`, gitignored): the sprout tile's brown channel is fully visible, the turning-arrow disc sits top-right and the finger bottom-right, all within the tile. Portrait and landscape tours pass (20 + 5). Verify passes.

**Strongest counterargument (art director):** teach purely by showing water run from the spring, with no hand or card. That's a candidate if M3 shows the hand is ignored or misread.
