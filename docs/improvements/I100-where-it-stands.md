# I100 — 100 iterations: where Mosselora's design stands

Date: 2026-11-24. Branch: feat/mosselora-painted-ui (PR #76).

This entry closes the 100-iteration run you asked for ("Let's do 100 iterations and log into docs/improvements/I[xx]-description.md, commit push each iteration"). It records where the design stands, how it got there, and what still needs a person.

## Final AI review (fresh Haiku agent, b79, seven current screens)

| Checkpoint | Immersion | Navigation | Icon-first | Polish | Daily appeal | Fit 40–49 |
|---|---|---|---|---|---|---|
| I12 (b20) | 5 | 5 | 4 | 6 | 5 | – |
| I25 | 6 | 6 | 6 | 5 | 5 | – |
| I51 | 7 | 7 | 5 | 6 | 6 | – |
| I60 | 7 | 7 | 5 | 6 | 6 | 6 |
| I67 (b43) | 8 | 7 | 6 | 8 | 5 | 7 |
| I73 (b46, live play) | 7 | 5 | 6 | 7 | 5 | 6 |
| I77 | 7 | 5 | 7 | 6 | 6 | 7 |
| I79 | 7 | 6 | 7 | 7 | 6 | 6 |
| I83 | 7 | 5 | 7 | 7 | 7 | 6 |
| I88 | 7 | 6 | 7 | 7 | 6 | 6 |
| I96 | 8 | 6 | 6 | 8 | 6 | 6 |
| **I100 (b79)** | **8** | **7** | **8** | **7** | **6** | **8** |

The scores come from different screen sets and different fresh agents, so read the trend rather than single points.

- **Strongest element (reviewer):** "The hub, with the painted garden, named villagers, and a single top-left menu icon, makes a clear and inviting first impression."
- **Biggest weakness (reviewer):** "The journal and puzzle screens switch to flat, uniform cards and tiles." Also noted: in a long, scrolled journal, the close button scrolls away with the pages.

## What the brief asked for, and what exists now

| Brief | State | Where |
|---|---|---|
| Icon-first buttons | Play tools, hub, memory, ending, share and change-choice are icon-only, with spoken names and press-and-hold tips. Words remain only on settings tiles. | I53, I65–I67, I76, I87, I97 |
| One menu button, smart navigation | A single wooden menu (settings, files, new garden) plus a memories book that appears only once there are memories. The same corners are used on the hub, the game and the ending. Each screen shows only its actions. | I03, I41, I78, I83, I93 |
| Immersive, not a web page | Codex-painted village, seasons, festival, vignettes, plaques, board bed, portraits and paper. The page and sheets are painted paper. There are page-turn crossfades, procedural creek sound, a solve butterfly and a growing landmark. | I01, I19, I21, I23, I24, I31, I35, I94, I95 |
| Codex image generation | 25 Codex-generated masters became 60 runtime sprites (`tools/evergrove_art_pipeline.py`). | `games/evergrove/art/` |
| Haiku sub-agents | Used in 28 iteration logs: art-director checkpoints, cold-reads, a first-timer proxy walkthrough, accessibility and code reviews, 29-language translation drafts, and two implementations (I93 screen-reader order, I99 tour step). | e.g. I12, I25, I51, I64, I87, I90, I93, I99 |
| Audience 40+ | `AGENTS.md`, the decision log entry of 2026-11-24, vision and design docs, and the M3 playtest kit with 40+ age bands. Readability work: 125% and 150% text pass on every main screen, stones at 3:1 contrast, and larger journal text. | I56, I70, I84–I86, I98–I99 |

Text is in all 30 languages: English plus 29 catalogs, 121 keys each. All new lines are AI drafts that still need native review.

## Bugs the iterations found and fixed (beyond design)

- **Hints hid the water already flowing (I91).**
- **The book on the memory screen opened blank (I98).**
- **Hidden board controls were readable by VoiceOver behind the hub (I82).**
- **The QA tour hid I74's whole-board stop (I96).**
- **The tour's "journal bottom" shot never scrolled the journal (I99).**

## Not done, and why

1. **"Insanely great" for adults 40+ is unproven.** Every score above comes from an AI reviewer. No adult aged 40+ has played this build. The decisive step is 8–12 uncoached sessions with `games/evergrove/docs/playtest/m3-session-kit.md` (checks P1–P9, with predeclared fail thresholds). Recruiting players is yours to do.
2. **Codex art is blocked.** Since I84, every `codex exec` has returned "Your workspace is out of credits" (seven tries). The queued change-choice glyph prompt is `art/prompts/ui-icons-6.txt`, and its pipeline step (`icons-ui-6` → `ui-choose-*`) is ready. I87 and I97 worked around it with existing painted art.
3. **Not in `main`.** PR #76 merged on 2026-11-23. I73 onward live on `feat/mosselora-painted-ui` (108 commits ahead of `origin/main` at I100) and need a new PR.
4. **Never checked on real devices:** VoiceOver or TalkBack audio, a physical iPhone or Android phone, an iPad, and short (SE-class) phones.
5. **Next design step (from this review):** give the journal grid more painted variety, and pin the journal's close button so a long scroll can't carry it away.

## Evidence

- `I100-where-it-stands.png` shows the seven screens the final review saw, left to right:
  1. hub
  2. first hint
  3. choice
  4. memory
  5. stream 2
  6. ending
  7. full journal
- b79: the verifier passes, the QA tour captures 20 screens, and the C# tests last ran at 40,813 assertions (I90; core rules unchanged since).
