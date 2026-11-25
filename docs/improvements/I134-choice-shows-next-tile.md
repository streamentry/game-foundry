# I134 — Each choice shows the tile it leaves in the next stream

Date: 2026-11-25. Branch: feat/mosselora-i127-plus.

## Player (Haiku sub-agent walk-through of eight b136 screens, as a 63-year-old morning puzzle player)

The top-ranked friction point was the choice screen: "'What will you help grow?' The three choices have no description of consequences." Asked what would bring them back tomorrow: "a one-line note on each choice… that showed what I picked."

Playing it myself: a choice does two things. A neighbour remembers it (the portrait badge already says who), and **it roots one protected tile in the next stream**. That tile is framed in bark, holds the chosen painting and can't turn. The second effect is the game's one distinctive claim ("your choice changes the next puzzle"), and the card said nothing about it. The player only discovers it afterwards, if at all.

## PM decision

- Show the second effect on the card **as a picture, not a sentence**: a miniature of the exact framed tile the player will meet on the next board, in the card's top-left corner. It has the bark frame, a dry channel through it and the choice's painting in its corner. So the card reads: painting (what), portrait (who remembers), mini tile (what it leaves on your next board).
- No mini tile on the last stream, where there is no next board to change.
- **Not now:** a text line per card. The screen reader already speaks the outcome sentence, and the memory card shows it after choosing.

## Dev

`EvergroveApp.cs`:
- `ChoiceCard` calls a new `NextStreamTile(picture, choice)`. It builds a 38pt rounded `AnchoredTile` body with a lift, a `LandmarkGraphic` configured to the choice's variant (`(int)choice + 1`, as `ReactiveWatercourses` uses), a dry straight `PipeGraphic` (as the player first meets the tile), and the choice painting (18pt) in the top-left. It is anchored to the picture's top-left, mirroring the portrait at the bottom-right.

## Result (b137 → b138, iPhone 17 simulator, scripted tour)

- b137: the mini tiles appeared on all three cards, but with a **wet** teal channel, while the real landmark on the next board starts **dry** and brown. b138 makes the mini channel dry, so the card and the board match exactly.
- `03-chapter1-solved-choice` shows each card with its framed mini tile at the top-left, its keeper's portrait at the bottom-right and the name below. `07-chapter2-after-shade-tree` shows the same framed tile, with the tree in its corner, on the next board. Local crop: `docs/improvements/I134-choice-shows-next-tile.jpg` (gitignored).
- The scripted tour captures all 20 screens. Verify and the .NET tests pass.

**Not verified:** whether a player connects the mini tile with the framed tile on the next board unprompted. That is the key question for the M3 session (P-list addition suggested: "Before choosing, what do you expect to change on the next board?"). The painting inside the mini tile is small (18pt).
