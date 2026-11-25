# I113 — Codex choose-again glyphs generated, tested, not adopted

Date: 2026-11-25. Branch: feat/mosselora-painted-ui (PR #76).

## Codex art arrived

Codex image generation worked again: `art/generated/icons-ui-6.png`, from the prompt `art/prompts/ui-icons-6.txt` queued since I84. It holds four "choose again / change what I planted" glyph candidates in the house icon style:

- A: a sprout with a counter-clockwise arrow;
- B: a pencil over a leaf;
- C: two cards with a swap arrow;
- D: a seed in soil with a return arrow.

The I84 pipeline step sliced them cleanly into tintable 128 px sprites.

## Two blind Haiku cold-reads (a first-time 55-year-old, nothing said about intent)

**Round 1:** the four Codex glyphs plus the current button (E: the I87 painted landmark with an undo badge).

| Option | First read | Confidence |
|---|---|---|
| B (pencil) | "change or edit the plant choice" | medium, ranked **1st** |
| C (cards) | "swap or shuffle the options" | medium-low |
| E (current) | "returns to the previous choice screen" | low-medium, ranked 3rd |
| A, D | undo, restart or replant | low |

**I built the best combination for b100:** the landmark (which choice) with the Codex pencil as its badge (change it).

**Round 2:** that combination, the pencil alone, and the current button.

| Option | First read | Confidence |
|---|---|---|
| R (current, undo badge) | "reverts the choice so the player can pick again" | **high, ranked 1st** |
| P (landmark with pencil badge) | "opens the flower picker or edit" | medium, ranked 2nd |
| Q (pencil alone) | "a tool or editor" | low, ranked 3rd |

## Decision: keep the current button and do not ship the Codex glyph yet

- The proxies disagree about the current button: 3rd in round 1, 1st in round 2.
- The combined pencil design never beat it.
- AGENTS.md: delete what does not earn its place, and never be confident without verification. So the b100 code change is reverted and no unused sprite is committed.
- **Kept:** the Codex master (`art/generated/icons-ui-6.png`) and its pipeline step. If players aged 40+ stumble on this button in M3 sessions, B (pencil) is the tested first alternative.

## Evidence

- `I113-codex-choose-glyphs.png` (local only, gitignored) shows the Codex sheet, the current button, and the b100 pencil-badge version.
- b100 built and the QA tour captured all 20 screens with the pencil badge. Reverted afterwards; no build of the reverted code was needed because it matches b99.
