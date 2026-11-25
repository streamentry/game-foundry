# I133 — Boards open on play and place, without a filler line

Date: 2026-11-25. Branch: feat/mosselora-i127-plus.

## Player

I played chapter 1 through to chapter 2 by hand (b135). The memory card is lovely: Mira planting the tree, the serif line, the stones. Two things broke the spell:

1. Every board after the first has the same kind of sentence over it: "A little water can change a place." (streams 2–4), then "The village is beginning to grow." and "Every choice becomes part of this place." It's the kind of line a website puts above a form. It says nothing about *this* board, and I stopped reading it after the second time.
2. On the memory card, "change my choice" was still a square parchment plaque beside the wide arrow, the last square tool after I129/I132.

## PM decision

- **Delete** the generic line over boards. The board, the stone path, the chapter name and the landmark already say where I am. The goal brief asks for fewer words on screen, and this one carried no information. `LocalizedVillage.Prompt` and its three catalogue keys stay in Core (unused by the UI), so this is one line to restore if a player test misses it.
- "Change my choice" becomes a round parchment token (76pt) holding the painted choice with its small turn-back badge. The wide "continue" arrow stays as the one primary action.

## Dev

`EvergroveApp.cs`:
- `BuildPuzzle` no longer adds the "Prompt" label for rounds after the first. Round 0 keeps Mira's coach bubble.
- The memory card's "Change your choice" uses `IconButton(..., 76, QuietAction, …, round: true)` and drops its fixed width.

## Result (b136, iPhone 17 simulator, scripted tour)

- `07-chapter2-after-shade-tree`, b135 compared with b136: the line is gone. The page is now short enough that it doesn't scroll, so the Mosselora sign stays at the top instead of fading under the chrome, and the board, tools and village keep their places.
- `16-winter-village` (4×4): the chapter name sits directly above the board.
- `04-consequence-shade-tree`: "change my choice" is a round parchment token holding the painted tree with its turn-back badge, beside the wide arrow.
- `verify_evergrove_project.py` and the .NET tests pass. Local sheet: `docs/improvements/I133-board-without-filler.jpg` (gitignored).

**Strongest counterargument:** the line gave a sense of progress ("The village is beginning to grow"). The stone path and the village painting now carry that alone. A player test should check that later chapters still feel like they're going somewhere.
