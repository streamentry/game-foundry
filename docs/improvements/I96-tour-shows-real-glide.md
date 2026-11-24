# I96 — QA tour shows the real post-solve glide

Date: 2026-11-24. Branch: feat/mosselora-painted-ui (PR #76).

## Fresh Haiku review (b74: hub, first board, choice, memory, stream 2, journal, ending)

| Review | Immersion | Navigation | Icon-first | Polish | Daily appeal | Fit 40–49 |
|---|---|---|---|---|---|---|
| I88 (b64) | 7 | 6 | 7 | 7 | 6 | 6 |
| I96 (b74) | **8** | 6 | 6 | **8** | 6 | 6 |

Immersion and polish reached 8 after I94 and I95 painted the page and sheets. The reviewer: "the puzzle sits on paper rather than a web page". A first run of this review failed to load the full-resolution hub image, so I downscaled it and re-ran.

## Findings, checked against the real screens

- **Fixed: "solved screen: board's top row cut off under a fade" (screenshot 3).** True of the QA tour shot, but not of play. The tour solved the board with real taps, then about 10 frames later forced its own `ScrollTo("Choose title")`. That overrode the game's 1.25 s hold and 0.6 s glide, which I74 built to stop with the whole board under the menu. Live play on b48 and later already showed the whole board. The tour now waits 2.4 s of real time for the game's own glide before the shot. The b75 shot `03-chapter1-solved-choice` shows the whole board, the full meter, the question, all three cards and undo, as a player sees it. This tour shot had been misleading reviewers since I74.
- **Declined: "ending: make starting a new season the teal primary".** I89 chose this deliberately. Starting over erases the garden, so sharing the finished season is the primary action and the new garden stays quieter beside it, with its two-step confirm.
- **Already addressed: "stepping-stone dots about 15 px, pale".** Since I84 they are 14 pt opaque season tones (3.3–3.5:1), with the current stone at 22 pt. The suggested visible "Stream 3 of 12" text stays spoken-only under the icon-first brief.

## Also checked: I95's open item

The scrolled journal under the new sheet mask is still unverified. The tour's `10b-journal-bottom` holds only four memories, which fit without scrolling, so it cannot test this.

## Evidence

- `I96-tour-shows-real-glide.png` shows `03-chapter1-solved-choice` from b74 (forced jump, board cut) beside b75 (the game's own glide, whole board).
- The verifier passes, and the b75 QA tour captured all 19 screens.

## Still blocked

- Codex art: out of credits.
- No players aged 40+ have tried the game (m3-session-kit P1–P9).
