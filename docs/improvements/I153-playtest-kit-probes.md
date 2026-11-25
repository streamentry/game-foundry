# I153 — The M3 playtest kit checks the new bets

Date: 2026-11-25. Branch: feat/mosselora-i127-plus.

## Player

Not a screen this time but the question every review keeps ending on: **"fit with adults 40+ isn't validated."** The last 26 iterations made new bets that only real players over 40 can settle: no sentence in the first tip, a demo finger, hollow drops, the mini next-board tile, haptics, running water, a share sheet. If the M3 session kit doesn't ask about them, a playtest could pass without testing any of them.

## PM decision

Bring the M3 session kit's painted-build addendum (§7) up to date, so the first real session with adults 40+ checks these bets, each with a fail rule and a written fallback (no redesigning from intuition between sessions):

- **P2** now covers the picture-only tip (I151), goal rings on every board (I148), the turning arrow (I138) and the demo finger (I147). Fallback: restore the sentence first.
- **P4** adds hollow drops (I146) and the tile → drop flight (I130). It fails if 2+ of 10 read the drops as lives or moves. Fallback: a visible "4 of 9 watered" line.
- **P5** adds the mini framed tile (I134), with one direct question before choosing: "What do you expect to change on the next board?"
- **New P13** haptics (I140): fails if 2+ of 10 call them annoying or nobody notices. Fallback: off by default.
- **New P14** the payoff of the running water (I144) and the brighter finished stream (I132).
- **New P15** the postcard share sheet (I149). Fallback: copy-to-clipboard wording.

## Dev

`games/evergrove/docs/playtest/m3-session-kit.md` §7: the heading now covers I01–I152, with three rows updated, three added, and the reversal rule extended.

## Result

`tools/check_evergrove_docs.py` passes (12 documents, 124 local links). No code change.

**Still open (needs the founder):** recruiting 8–12 uncoached adults 40+ across 40–49, 50–64 and 65+, and running the session on a real phone.
