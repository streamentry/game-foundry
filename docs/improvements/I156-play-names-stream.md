# I156 — Play names the stream it continues

Date: 2026-11-25. Branch: feat/mosselora-i127-plus.

## Player (Haiku cold read of the returning home screen, as a 66-year-old opening the game the next morning)

- "First tap: the big arrow. It's the largest and clearest control… **The arrow has no words, so I don't know what it will start.**"
- "One change: put a plain label on the big button, such as 'Continue: Rainwater bridge,' **so I know what I'm going back to.**"
- "The 'Rainwater' text is small."

The next stream's name was there, as a separate small line above an icon-only arrow (I67, I78), but the eye goes to the button and the name didn't belong to it.

## PM decision

Follow the top casual-game convention, where Play carries the level ("Level 214"): the home screen's play plaque now reads **→ First Bend**, with the arrow plus the next stream's place name in 20pt bold. It's a place name, not an instruction, so it's the one word the brief's "avoid text if possible" can't replace with an icon. The separate small line above is deleted. When there's no stream name (a finished garden, or a non-English language for now), the plaque stays the bare arrow. The board, menu and tools stay icon-only.

## Dev

`EvergroveApp` home screen: the play `IconButton` is captioned (`showCaption`) with the stream name up to " · " (the landmark suffix), restyled to 20pt bold, and 250pt wide. The `NextStreamLine(…, abovePlay: true)` call on the home screen is removed (the memory card still uses `NextStreamLine`).

## Result (b164 → b165, iPhone 17 simulator, fresh install, home screen)

- b164: "→ First Bend" on the teal plaque in regular weight. b165: bold, which reads more clearly on teal (`docs/improvements/I156-play-names-stream.png`, gitignored).
- Tours (which skip the home screen) capture all 20 portrait and 5 landscape screens. Verify passes.

**Not verified:** the landscape home screen (its right-hand column is narrower; the 250pt plaque should fit 380pt+ columns), long stream names (the longest English name, "Stone Crossing", should fit), and the returning-player cold read repeated on this build.
