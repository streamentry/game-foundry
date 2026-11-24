# I59 — Rooted-tile rule appears as a toast on tap, not a page line

Date: 2026-11-24. Branch: feat/mosselora-painted-ui (PR #76).

**Player problem (I60 review, finding 4).** "Tree roots anchor this pipe. It cannot rotate." sat above every board with a rooted tile. It competed with the board and pushed it down, though the framed tile now shows the painted landmark (I58).

**Change.** The rule line is **spoken-only** (transparent, out of layout). Tapping the rooted tile still gives the gentle nudge (I38), and now also shows a small **parchment toast** with the rule just above that tile. It fades after about 2.6 s, shows one at a time, clamps to the screen edges and follows the text-size setting. No move or undo entry is spent (the QA tour asserts this).

**Evidence.** b37 tour (`I59-rule-toast.png`). Left: the stream 2 board sits directly under the chrome with no rule sentence. Right: tour step 13 (a dev puzzle-only run) after tapping the rooted tile shows "Bridge foundations anchor this pipe. It cannot rotate." above it.
