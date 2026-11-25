# I131 — The board stays under the finger when the tip leaves

Date: 2026-11-25. Branch: feat/mosselora-i127-plus.

## Player

I found this while recording I130. On the first board, Mira's tip bubble ("Blue waves = spring…") sits above the board. The instant I make my first turn, the bubble is removed and **the whole board jumps up about 150pt**. My finger is still on the tile I turned, which now sits somewhere else. Of all the moments to move the board, the player's very first touch is the worst: it feels like a web page reflowing and makes the second tap land on the wrong tile.

## PM decision

- The tip still leaves after the first move (it has done its job), but it **fades where it stands and keeps its space** for the rest of that board. The next board has no tip, so the space goes away with the page turn.
- The spent tip is no longer read by the screen reader.

## Dev

- `EvergroveApp.RefreshBoard`: instead of `coachBubble.SetActive(false)` (which collapsed the layout), it starts `FadeAway(coachBubble)`. That coroutine fades a `CanvasGroup` to 0 over 0.35s (instantly with gentle motion off) and stops the group blocking or taking taps.
- `ScreenReaderBridge.Walk` skips a subtree whose `CanvasGroup` has faded to 0 and no longer blocks raycasts, so the faded tip is not announced.

## Result (b134, iPhone 17 simulator, fresh install, hands-on)

- On the first board I made the same first turn as in I130 (the bottom-left elbow). The board stayed exactly where it was. The turned tile is still under the finger, now wet, and the fifth drop filled. The tip faded out above the board.
- Trade-off: a blank band of paper stays where the tip was until the next board. A steady board matters more than that space for one board. A later pass could use the band for the board's own picture.
- The scripted tour captures all 20 screens. Local screenshot: `docs/improvements/I131-board-stays.png` (gitignored).

**Not verified:** VoiceOver on a device (the skip rule is code-reviewed only), and landscape, where the tip sits on the painting and never moved the board.
