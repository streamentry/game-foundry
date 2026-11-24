# I86 — Stream header shows the landmark; 125% verified

Date: 2026-11-24. Branch: feat/mosselora-painted-ui (PR #76).

## Change (b63)

**Stream header shows the landmark, not a label.** Stream 2 onward, after a choice, carries a reactive rule: one tile rooted by your landmark. Its header read "Rainwater · Wildflower sanctuary", a cryptic rule name in moss text. In I73 the hub had already moved to the painted landmark plus the stream name. The puzzle header now does the same: the wildflower (or shade-tree, or bridge) icon at 26 pt, then "Rainwater". It sits right above the board whose rooted tile wears that same painting.

- Screen readers still hear the sanctuary name, through the rooted tile's spoken rule ("Wildflower sanctuary preserve one connection…"). I did not add a second hidden label, because every text node is read and it would say the name twice.
- There is no per-tap animation; the header is rebuilt on every move.

## 125% text checked live (closes I85's open item)

With text at 125% on b62:

- The returning hub fits: painting, three neighbors with news, wordmark, tagline, stones, memory line, landmark plus "Rainwater", and Play.
- Stream 2 fits: header, board, drops, all three plaques and the neighbor strip.

`I86-stream-header-landmark.png` shows, left to right: the stream 2 header on b62 then b63 (QA tour shot `09-chapter2-after-flowers`), then the 125% hub and the 125% stream 2 from live play.

## Codex

The `icons-ui-6` change-choice glyph was retried a third time and failed again with "Your workspace is out of credits". Still blocked on credits.

## Evidence

- The verifier passes, and the b63 QA tour captured all 19 screens.

## Still unverified

- Shorter phones (SE class) were not checked.
- No players aged 40+ have tried this (m3-session-kit P1–P9).
