# I103 — Brand logo wins a blind read; journal tilt removed

Date: 2026-11-24. Branch: feat/mosselora-painted-ui (PR #76).

## Open items from I102

- The scrapbook-tilt comparison was cut off by a usage limit.
- The brand logo at large text had not been checked.

## Blind Haiku comparison (versions shown in random order, nothing said about what changed)

| Pair | Verdict |
|---|---|
| Opening screen: old flat wordmark vs repo brand logo | **Brand logo.** It "reads like storybook title art", while the old one "reads more like an app header". The text is equally easy to read in both, and nothing is broken. |
| Journal: tilted pages (I102) vs flat grid (I101) | "Nearly identical", with a low-confidence lean towards the **flat** grid. Neither is harder to read. |

## Decision

- **Keep the brand logo.** The comparison supports it, and you asked for it.
- **Remove the journal tilt.** The evidence shows no gain, and AGENTS.md says to delete what doesn't serve the player. The pages go back to a straight grid with the original lift. The I101 pinned close button and the I98 text sizes stay.

The reviewer also noted that the hub's menu button sits over the painted tree trunk, "a little crowded". I left it unchanged: the button has its own wooden plaque and the same corner as every other screen.

## Brand logo at large text (live, b85)

- **Hub at 150%:** the painting moves up (I85) and the logo stays sharp at 96 pt, with the two-line tagline, the stones, "First Bend" and Play all visible.
- **First puzzle at 150%:** the logo is in the header, and Mira's large note, the whole board, the water drops and all three tool plaques are on one screen.
- The menu at 125% (seen on the way) also keeps the logo clear of both corner buttons.

## Codex

Retried an eighth time; it still fails with "Your workspace is out of credits".

## Evidence

- `I103-logo-verified-tilt-removed.png`, left to right:
  1. old hub
  2. brand-logo hub
  3. hub at 150%
  4. first puzzle at 150%
- The verifier passes, and the b86 QA tour (tilt removed) captured all 20 screens.

## Still blocked

- Codex credits.
- No players aged 40+ have tried the game (m3-session-kit P1–P9).
