# I167 — The creek behind the board fills as the water spreads

Date: 2026-11-25. Branch: feat/mosselora-i127-plus.

## Player (Haiku art-director critique of b167)

"**Water counter:** replace the drop row… with a water level in the painted stream along the board's edge. **Currency should look like part of the landscape, not a UI widget.**"

The painted creek behind the board (I110/I111) showed its **dry** painting until the stream was solved, then switched to **flowing**. The world didn't respond while the player was working, only at the end.

## PM decision

Keep the drop meter: it's the exact, countable reading (and the I130 and I146 work depends on it). Add the landscape version too: **the creek behind the board fills as the board does.** The flowing painting lies over the dry one, as opaque as the share of tiles the water reaches (0 to 9 out of 9), easing over 0.8s after each move. At the solve it's the full flowing creek, as before. With gentle motion off the change is instant.

## Dev

`EvergroveApp.CreekBehindBoard`: on unsolved boards it adds a `Creek water` image (the season's `-flowing` banner, envelope fit) above the dry art, at alpha `0.55 × share`. `RefreshBoard` eases it with `FillCreek` when the share changes. `CreekShare()` = connected ÷ cells.

## Result (b179, iPhone 17 simulator, tours)

- The creek band left of the bed measures mean RGB 199/185/150 on b178 (dry), 187/180/144 on b179 with 4 of 9 tiles watered, and 181/177/141 at 5+ of 9: darker and bluer as water reaches more tiles. 31,741 pixels changed between b178 and b179 on the first board, all in the creek band. `I167-creek-fills.jpg` (gitignored) shows b178 above and b179 below: a faint stream shimmer appears under the bed.
- Portrait, landscape and 150% tours pass (21/6/21). Verify passes.

**Honest limit:** the effect is **subtle**. Most of the creek sits behind the stone bed, and the visible band is thin. It's ambience, not information. If players don't notice it in M3, that's fine: the meter carries the information.
