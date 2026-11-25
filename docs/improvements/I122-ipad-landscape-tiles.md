# I122 — Sideways iPad: a wider column and 120 pt tiles

Date: 2026-11-25. Branch: feat/mosselora-painted-ui (PR #76).

## Problem

In the I121 iPad landscape tour, the board stopped at the phone's 90 pt tile cap in a 480 pt column. That left most of the right half as empty paper below the board, on the device where 40+ players have the most room for big, easy targets.

## Change

- `BoardFit.Landscape(..., cap)` takes an optional cap; `BoardFit.TabletCap = 120` (the same as the existing Large-board cap).
- A **sideways tablet** (landscape with aspect below 1.6) gets a **560 pt** play column, up from 480, and the tablet cap. Phones are untouched: their aspect is at least 1.6, so the phone column and 90 pt cap still apply.

## Tests (50,072 assertions in total, 591 new)

- A sideways iPad (896 pt tall, 501 pt for the board) reaches the 120 pt tablet cap; without the cap the phone's 90 pt holds.
- Over heights 600–1,100 pt and columns 380–640 pt: tablet tiles stay within 44–120 pt, and the Undo/Hint rail always fits beside the board.

## Verification (b122)

- iPad landscape tour: 3×3 tiles grow from about 82 to about **106 pt** on screen. The board, drops, stones and rail fill the column, with less empty paper (`I122-ipad-landscape-board.jpg`, b121 top vs b122 bottom, local only).
- iPhone landscape: **0 pixels differ** from b121 on the first-puzzle shot.
- iPhone portrait tour DONE (20). The only differences from b121 are two animation frames (a leak pulse and the stone-arrival swell); layout is identical.
- Unity build: 0 `error CS`. The verifier reports OK.

## Not verified

- A real iPad, Android tablets, players aged 40+ (**age fit not validated**).
