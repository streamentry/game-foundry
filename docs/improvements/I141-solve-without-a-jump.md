# I141 — The first solve no longer jumps the board

Date: 2026-11-25. Branch: feat/mosselora-i127-plus.

## Player (screen recording of the first solve, b145, frames every 0.25s)

The best moment in the game is the last turn that sends the water all the way to the sprout. Frame by frame:

1. The last elbow turns.
2. **0.25s later, the whole board jumps up about 60pt.** The solve rebuilds the page, and Mira's spent tip (faded in I131, keeping its space) is no longer built, so its space disappears.
3. The butterfly flies across the board.
4. About a second later the page glides up to "What will you help grow?".

So the payoff moment has two movements, and the first is an instant reflow under the player's eyes, just as they look for the water reaching the sprout. It's the same "web page reflow" feeling I131 removed for the first move, now at the first success.

## PM decision

For the rest of the first board, **keep the spent tip's space** through any rebuild (the solve, opening the menu, changing text size). The board stays put at the solve, the butterfly flies over a still board, and the deliberate glide to the choices becomes the one movement. The space goes with the page turn to stream 2.

## Dev

`EvergroveApp.cs`:
- New `spentTipHeight`. When `RefreshBoard` fades the coach bubble (I131) and the bubble sits in the page column (not the landscape painting), it records the bubble's height.
- `BuildPuzzle`, round 0: if the bubble isn't built (after the first move) but a height was recorded, it adds a `Spent tip space` node of that height in the bubble's place (portrait only). Any later round resets the height to 0.

## Result (b146, iPhone 17 simulator, the same hands-on solve, recorded)

- `I141-solve-before.jpg` (b145): between frames 3 and 4 the board jumps from about y 160 to about y 100 (sheet scale).
- `I141-solve-after.jpg` (b146): frames 3 and 4 show the board at the same height. The choices appear below, the butterfly crosses a still board, and then the page glides up once (frames 9–11). Both sheets are local only (gitignored).
- The scripted tour captures all 20 screens. Verify passes.

**Not verified:** the menu or text-size rebuild during round 0 (same code path, not recorded), and landscape (the tip lives on the painting there and never took page space).
