# I191 — One corner button opens everything else as a fan

Date: 2026-11-26. Branch: feat/mosselora-world-first.

## Player (b214)

Two wooden buttons sat in the top corners of every screen: the menu (left) and the book (right). The menu opened a long form: language, text size, board size, motion, chime, postcard, garden file, new garden, all in one scroll. The almanac was a tab hidden inside the book, and going back to my garden from a board wasn't possible at all without finishing the stream.

## PM decision

- **One corner button.** It opens a **fan**: a column of round tokens hanging under it, with the rest of the screen dimmed. A tap anywhere else folds it away. No words; each token's name is spoken and shown on press-and-hold.
- Tokens, only when they mean something:
  - **Home** (from a board): back to your garden at any time.
  - **Book**: your memories. The "new memory" sprout now sits on the corner button and on the book token.
  - **Almanac**: a painted sapling, straight to the 200-item almanac.
  - **Postcard**: the share sheet (or copy) in one tap.
  - **Settings**: the existing sheet (icon tiles come in I192).
- While the book or the sheet is open, the corner button turns green and closes it.
- The top-right book button is gone, so the top of the garden is free.

## Dev

`EvergroveApp`: `HubChrome` and `BuildChrome` build only "Open menu"; `ToggleFan(fromHub)` builds a full-screen "Menu fan" (scrim button that closes) and a "Fan column" hung under the corner button's world corners; `FanToken` makes 64 pt round tokens with a fixed layout height; `OpenBook(almanac)`; `FanIn` fades and scales tokens in turn (gentle motion only). `PlaceBook` removed. `DevQaTour`: the menu fallback presses the fan's Settings token, the journal closes with "Close journal", and a new shot `17b-menu-fan.png`.

## Result (b215 → b216; portrait, landscape and 150% tours, 24 each, all complete)

`I191-one-button-fan.jpg`: the year's garden with the fan open (book, almanac sapling, postcard, settings) at 100% and 150%; the book opened from the fan with the green corner button; stream 2 with only the corner button. `I191-fan-landscape.jpg`: the same fan sideways and the book sideways. A first build spaced the tokens about 130 pt apart (fixed in b216). Hands-on on a fresh garden: the fan shows postcard and settings only, as intended. Project verify passes.
