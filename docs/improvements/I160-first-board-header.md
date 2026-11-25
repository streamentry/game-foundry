# I160 — The first board opens like every board, without the tagline

Date: 2026-11-25. Branch: feat/mosselora-i127-plus.

## Player (Haiku art-director critique of b167)

"**Header stack:** the tagline, pebble track, chapter name and water counter stack above the board… **The board should be the dominant element.**"

On the very first board, the player's first impression of play, the column was: wordmark, the tagline "A living garden that remembers.", the stones, "First Bend", Mira's tip, then the board. Later boards have no tagline (removed long ago). The player has just read the same tagline on the home screen one tap earlier.

## PM decision

**Delete the tagline from the first board**, so every board opens the same way: wordmark, path, stream name, board. The tagline stays where it belongs: the home screen, the memory card and the ending, the screens that tell the story. The path and the stream name stay, because they're the player's position. (The critique's bigger ideas, painting the chapter name into the scenery and showing water as a stream level, are kept as candidates.)

## Dev

`EvergroveApp.Rebuild`: the tagline condition `(!puzzleScreen || world.Round == 0) && …` becomes `!puzzleScreen && !(landscape && memoryScreen)`.

## Result (b170, iPhone 17 simulator, scripted tours)

`I160-first-board-header.jpg` (gitignored), `01` b169 compared with b170: the board moves up by about one line, and the village strip with Mira, Orin and Fen now shows on the first board's first screen. `04` (memory card) keeps its tagline. Tours pass (20 + 5). Verify passes.
