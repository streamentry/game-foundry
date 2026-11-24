# I67 — Hub and ending buttons become icon-only

Date: 2026-11-24. Branch: feat/mosselora-painted-ui (PR #76).

**Player problem (I67 review, finding 1).** Two buttons outside the menu still carried words: the hub's play button ("→ First Bend") and the ending's "Explore another garden".

**Change.**
- **Hub.** The play button is a **plain arrow plaque** (teal enamel, I65). The next stream's name sits *above* it as a quiet label (English builds) and is the button's spoken name. Wayfinding is kept, but the button itself has no text.
- **Ending.** The new-garden button is the **seedling icon only**. Its first tap still opens only the worded erase confirmation beside an icon-only cancel, so a curious tap is safe, and the press-and-hold name says "Explore another garden".
- **Buttons with visible words in a release build are now:** the settings, garden-file, postcard and new-garden tiles inside the **menu** (where words belong), the erase **confirmation**, and the unreadable-save **recovery** flow (rare, data-loss risk). On the play path (hub, puzzle, choice, memory, journal, ending, neighbors) every button is an icon, a picture or a portrait; neighbor names are identity plates.

**Evidence.** b44 hub and ending (`I67-I68-icon-hub-quiet-page.png`, left and right). QA tour: 19 screens.
