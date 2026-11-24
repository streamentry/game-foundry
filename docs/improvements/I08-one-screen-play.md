# I08 — Board, tools and village share one screen after stream 1

Date: 2026-11-24. Branch: feat/mosselora-painted-ui (PR #76).

**Player problem.** After the first stream, the tagline "A living garden that remembers." pushed the board and tools down, so the painted village fell below the first screen. The memory screen used a larger wordmark that collided with the floating menu and book buttons.

**Change.**
- Puzzle screens show the tagline only on stream 1, where it introduces the game. Story screens (memory, ending) keep it. From stream 2, the wordmark, stream pill, board, droplet meter, tools and the top of the painted village share one phone screen.
- The wordmark is 46 pt on every screen, so it always clears the floating buttons.

**Evidence.** b18/b19 tours, 17 screens each. Stream 2 (`I06-gentle-motion.png`, left) shows board, tools and the painting together, and the memory screen (`I07-neighbors-in-painting.png`, right) shows the wordmark between the buttons.
