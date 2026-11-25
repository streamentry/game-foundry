# I136 — The ending's two offers say what they do

Date: 2026-11-25. Branch: feat/mosselora-i127-plus.

## Player (Haiku walk-through of b136, as a 63-year-old)

On the ending: "The two bottom buttons have no labels, so I wouldn't know what they do." And under "looks like an app": "the unlabeled icon buttons at the bottom of screen 8 also look like app controls."

The two offers are: **copy a garden postcard** (a tiny festival painting with a chat mark) and **explore another garden** (a seedling with a plus). Each is used once per season at most, and the second starts over. Press-and-hold names them, but nobody press-and-holds a button they've never seen.

## PM decision

The brief says "avoid text **if possible**" for buttons. For the board's tools, which are used every few seconds, pictures are enough, and the cold reads recognised undo and hint. These two are rare and lasting decisions, and a cold read couldn't tell them apart, so here a short caption under the picture is the right exception:

- Both become 150pt-wide plaques with the picture on top and the existing localised name underneath at 15pt ("Copy garden postcard", "Explore another garden"). No new strings.
- The board, menu chrome and memory card stay icon-only.

## Dev

`EvergroveApp.ResetControls` (ending only, `sharePostcard`): both buttons use `captionBelow` at 96pt. A new `EndingCaption(button)` sets 15pt captions, 9pt spacing (clear of the postcard's chat badge) and a 104pt minimum height. In the menu, the new-garden button keeps its own captioned style.

## Result (b140 → b141, iPhone 17 simulator, scripted tour `11-ending`)

- b140: the captions appeared at 13pt, and "Copy garden postcard" ran under the postcard's chat badge.
- b141: both captions sit clear under their pictures at 15pt, wrapping to two lines. Local crop: `docs/improvements/I136-ending-says-what.jpg` (gitignored).
- The scripted tour captures all 20 screens. Verify passes.

**Not verified:** 150% text size on the ending, where the captions will wrap further and the buttons grow taller.
