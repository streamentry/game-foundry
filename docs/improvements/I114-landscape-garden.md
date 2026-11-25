# I114 — Turn the phone: garden full screen, play beside it

Date: 2026-11-25. Branch: feat/mosselora-painted-ui (PR #76).

## What changed

Turn the phone sideways and the garden takes over the screen; the play column sits beside it.

- **Landscape layout** (`FitColumn`): when the screen is wider than tall, the canvas uses a 896×414 reference on phones and the column moves to the right edge, 380–480 pt wide. The left side becomes a full-height painted garden scene, melting into the paper with a side fade (`MosseloraArt.SideFade`).
- **What the garden shows**: the newest memory vignette, otherwise the festival or the hero painting, with the village strip and the three neighbours along the bottom.
- **Hub**: window and village strip on the left 56%, wordmark and Play on the right.
- **Puzzle**: the board, water drops, Undo and Hint fit the column. Tiles are capped by the screen height, never smaller than 44 pt. The garden header, subtitle, in-page illustration and creek are left out, because the garden painting on the left already shows them.
- **Memory screen**: the vignette is not repeated in the column, and the card starts below the book/menu chrome (82 pt top inset).
- **Rotating mid-game** rebuilds the title and the current screen in place, keeping progress and the board state. Menu and journal sheets centre at 520 pt wide.
- **Portrait unchanged**: every landscape branch is gated on `aspect > 1`.

## Verification

- Live on the iOS simulator (b101–b104): the hub, the board with Mira's note, rotating moves, solving, the three choice cards, and the garden strip turning to flowing. The memory screen was captured after a rotation.
- A b103 capture showed the book button sitting 15 pt over the memory card's corner. The top inset went from 62 to 82 pt in b104.
- Portrait: b99 and b102 tour pixel diffs show only animation noise (≤310 px across 5 shots). The b104 portrait tour ran to DONE.
- C# tests: 41,113 assertions pass. `verify_evergrove_project.py` reports OK.
- The scripted QA tour is portrait-only; landscape is checked by hand.

## Not verified

- A real device, iPad landscape, and players aged 40+.
