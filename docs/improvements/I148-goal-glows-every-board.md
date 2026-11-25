# I148 — Every board shows its spring and sprout before the first move

Date: 2026-11-25. Branch: feat/mosselora-i127-plus.

## Player (Haiku walk-through of b152, as a 52-year-old)

On a later board: "**the board's goal isn't stated.** I can see a sprout and a framed tree tile, but not what finishing the board does."

On the first board, the spring and the sprout glow with soft teal rings until the first move (I-series coach). From stream 2 on, nothing marks them. A returning player who opens the app on stream 5 after three days sees nine brown pipes and has to find the spring and the sprout again.

## PM decision

Show the same soft start and goal rings at the start of **every** board, until the player's first move there. It's the existing visual language (the hint still wins on a shared tile, I91) and costs no words.

## Dev

`EvergroveApp.BuildPuzzle`: the goal-glow condition drops `world.Round == 0` (it's still `End` tiles only, before the first move, unsolved, not hinted). `RefreshBoard` already removes the glows on the first move.

## Result (b156, iPhone 17 simulator, scripted tour)

`07-chapter2-after-shade-tree`, b155 compared with b156 (`docs/improvements/I148-goal-glows.jpg`, gitignored): the spring (top-right) and the sprout (centre) now carry teal rings on stream 2, while the protected tree tile keeps its bark frame. The scripted tour captures all 20 screens. Verify passes.

**Not verified:** whether the rings help returning players (M3 or a diary study). If players find them redundant after a few streams, limit them to a return after a break.
