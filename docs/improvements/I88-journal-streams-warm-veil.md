# I88 — Journal pages name their stream; warm sheet veil

Date: 2026-11-24. Branch: feat/mosselora-painted-ui (PR #76).

## Fresh Haiku review (b64 screens: hub, coach, choice, stream 2, journal, ending)

| Review | Immersion | Navigation | Icon-first | Polish | Daily appeal | Fit 40–49 |
|---|---|---|---|---|---|---|
| I83 (b56) | 7 | 5 | 7 | 7 | 7 | 6 |
| I88 (b64) | 7 | 6 | 7 | 7 | 6 | 6 |

I checked each finding against the actual screens.

- **Rejected: "stream 2 board clipped under the header; wordmark, stones and book gone".** The b64 tour shot `09-chapter2-after-flowers` shows the full header, stones, "🌼 Rainwater" and the whole board.
- **Rejected as stale: "stones about 14 px and pale".** The reviewer judged the stones from the I79 and I83 screenshots, which predate I84. I84 made them opaque season tones (3.3–3.5:1) and grew them from 11 to 14 pt, with the current ring at 22 pt.
- **Accepted: the journal showed "Orin remembers the bridge you restored." twice, identically.** The tour garden built a bridge on streams 1 and 4. That is accurate, but on screen it reads as a bug.
- **Accepted: the grey-green scrim behind the sheets looks like a web modal.**

## Changes (b65)

- **Journal pages:** each page opens with its stream: a 10 pt season stone (the same colours as the progress stones) and the existing localized "STREAM n / 12" label (`stream` key, 29 catalogs, no new key). Two bridges now read as two memories from two streams.
- **Sheet veil:** behind the menu and the journal, the veil is now a warm umber (0.30, 0.22, 0.13, 40%) instead of a cool dark green-grey. The garden behind dims like an old photograph rather than a dialog backdrop. The parchment sheet's rim and lift keep it distinct.

## Evidence

- `I88-journal-streams-warm-veil.png` shows the journal from the QA tour (`10-journal`): b64 on the left, b65 on the right.
- The verifier passes, and the b65 QA tour captured all 19 screens.

## Still open

- Repeated choices still produce the same memory sentence. Varied lines per repeat would need new text in 30 catalogs.
- The Codex change-choice glyph is still blocked by credits.
- No players aged 40+ have tried this (m3-session-kit P1–P9).
