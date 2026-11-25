# I145 — The new-memory mark sits fully on the book

Date: 2026-11-25. Branch: feat/mosselora-i127-plus.

## Player (Haiku walk-through of b152, as a 52-year-old nurse who dislikes games that "look cheap")

On a later board: "**There's also a small sprout badge overlapping the book icon, which looks like a glitch.**"

The badge is the "new memory" mark (I80/I112): a cream disc with a sprout, centred on the book button's top-right corner. The book sits at the screen's right edge, so half the disc hung past the button and against the screen edge. It looked like a stray sprite, not a deliberate mark. That costs the most with the picky players who decide in seconds whether a game is "cheap".

## PM decision

Move the mark to the book's **inner (top-left) corner**, toward the middle of the screen, a little larger (26pt), fully on screen. Its meaning, pulse and behaviour are unchanged.

## Dev

`EvergroveApp.MarkUnreadMemory`: the ornament anchor goes from `(1,1)` with offset `(-4,-4)` to `(0,1)` with offset `(4,-4)`, and from 24pt to 26pt.

## Result (b153, iPhone 17 simulator, scripted tour `16-winter-village`)

The top-right corner crop, b152 compared with b153 (`docs/improvements/I145-book-badge.jpg`, gitignored): the sprout disc moves from straddling the book's outer corner at the screen edge to sitting fully visible on its inner corner. The scripted tour captures all 20 screens. Verify passes.

**Rejected from the same walk-through, with reason:** "the painted garden visibly grows while I'm away." The vision's rule is that a garden left alone waits unchanged (no wall-clock events). The hub already greets a returning player with the painting of their latest memory and neighbours with news (I80/I107).
