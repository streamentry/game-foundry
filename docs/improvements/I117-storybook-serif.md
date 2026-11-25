# I117 — The story speaks in a book serif; the interface keeps its rounded sans

Date: 2026-11-25. Branch: feat/mosselora-painted-ui (PR #76).

## Problem

Every word in the game was set in the runtime sans (Arial-like), with Arial Rounded for headings. In the I116 cold-read, Haiku named "generic sans-serif labels" as what still makes the screens "lean toward app UI". The story itself (who remembers what, what changed, what neighbours say, how the season ends) spoke in the same voice as the buttons and labels.

## Change: a typographic pairing

- **The story's own voice is set like a book**, in a serif already on the device: **Charter**, falling back to Georgia. iOS ships both; nothing is bundled and there is no licence to track. Without either, the text keeps the runtime face.
- It applies to the memory card's lead and "what changed" line, neighbour speech, keeper lines, memory-journal entries, the world consequence on the hub, and the ending's final words.
- **Labels, headings and instructions keep the rounded sans** (A NEW MEMORY IN THE GROVE, Mira's how-to-play note, stream names), so "story" and "interface" read as two different voices.
- English only, the same rule as the rounded display face; other languages keep their locale fonts.
- **Upright roman only.** A bold serif lead ran wide and pushed "grew." onto its own line (b110). An italic aside was judged too thin on cream paper for 40+ eyes in a Haiku cold-read (b111). The aside is now upright at 17 pt (was 16).

## Verification

- `Mosselora book face: Charter` was logged on the iPhone simulator.
- Portrait tour b112 ran to DONE (20 screens). Only the screens with story text changed (memory cards, journal, ending). On the memory card the layout positions are unchanged from b109: card height, stones and arrow (`I117-memory-before/after.png`, local only).
- Haiku cold-read (b109 vs b111): the serif version "feels more like a storybook game and less like a web app". It flagged the thin italic, which is fixed in b112.
- C# tests: 41,113 assertions pass. The verifier reports OK.

## Not verified

- Android, which has neither Charter nor Georgia, so it keeps the runtime face. Not run.
- Larger text sizes with the serif, a real device, and players aged 40+ (**age fit not validated**).
