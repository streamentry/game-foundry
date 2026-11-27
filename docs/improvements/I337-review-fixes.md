# I337 — Fixes from an independent review

Not one of the fifty ideas: the clean-up after them. An independent read-only review of the new Unity code (boards, quiet mode, decorations, the well, take-turns), and the difficulty work, found these. All are fixed here.

## What was wrong, and what changed

**Seed sudoku could need guessing.** The generator's check "solvable by singles" read a naked single as one value too high (v + 1), so a few boards that need more than singles got through. One in 900 sampled boards needed guessing, which breaks the promise of fair, logic-only puzzles. The one-character fix shifts the value correctly. A new test proves an honest singles solver finishes every board, 300 seeds at each tier; it failed at tier 2, seed 295, before the fix. Some seeds now deal different boards; nothing stored depends on a board's layout.

**A decoration chosen at the cottage could lock the garden.** With a decoration "in hand", every land tap went to placing it. A tap where it couldn't stand did nothing, so planting, buying and the cottage itself stopped responding, with no sign why. Now:
- a refused spot puts the decoration back, with a word;
- tapping any thing lets go of it;
- a toast says what to do;
- the choice is offered only while there's room for another.

**Smaller issues:**
- **Pinching in the lookout** scaled the map a hundredfold and lost the saved zoom. Pinch is ignored while looking out.
- **Garden pieces passed the turn** on every turn, mirror or lift. Now only laying a piece is a move.
- **Garden pieces, empty tray slot:** tapping a placed piece's empty slot silently broke the next tap; such taps are now ignored.
- **The "send this board" letter** vanished 2.9 s after solving. The card now stays 3 s longer when it's offered.
- **The well's card said "+0"** (seeds) though it pays fruit. It now shows a pear and "+2".
- **The take-turns hint glow** could keep a stale honey colour for one frame; the glow now stops at once.
- **Repeated bad taps** could stack shivers and leave a cell a few pixels off.
- **Lanterns** made home lay out twice, replaying the newest thing's growth. They're now known before the first layout.
- **Silent no-ops:** a mossy stone with a full basket, and a keepsake without enough seeds, now explain themselves. A wish's thank-you is only marked sent once the letter is kept.

## Not changed

The review noted that home's statics (cat, bees, lamps) could show on a visited garden after an unusual re-layout during a visit. It's cosmetic and rare; left for later.
