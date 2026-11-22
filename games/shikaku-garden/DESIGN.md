# Shikaku Garden — independent design system (v0.1)

**Positioning:** a quiet spatial planning game where every number finds its rightful rectangular home. No cognitive-benefit claims or "senior" labels.

**Three adjectives:** botanical · methodical · comfortable.

| Semantic token | Color | Use |
| --- | --- | --- |
| Page | `#F7F7EF` | Warm ivory |
| Ink | `#293C36` | Number and headline |
| Supporting text | `#54665E` | Instruction and result |
| Active corner and button | `#277A65` | Current selection/action |
| Neutral button | `#E5EDDC` | Low-contrast chrome |
| Garden regions | `#D1E6CA`, `#F0D4AC`, `#D2E3ED`, `#ECD4D8`, `#DCCBEA`, `#F0E6B5` | Separate placed rectangles |
| Grid stroke | `#BDCFC1` | Unfilled boundaries |

**Type scale:** system face, title 27 px, number clues 19–21 px, 15–16 px guidance and buttons, 12–13 px captions. **Spacing:** 18 px horizontal margin, 12 px vertical rhythm, 4 px grid gutters, button height ≥54 px. Grid cells ≥52 px on a 6×6 board; vertical ScrollContainer accommodates compact phones.

**Contrast:** dark numbers remain readable across every pastel region; selected corners use white text on deep green. Text confirms both placement validity and completion; never use color alone. Full physical-device contrast measurements remain unverified.

**Motion:** instantaneous region fill; no timer, confetti or moving camera. Undo removes the last region immediately. **Sound/haptics:** absent; no forced feedback.

**Puzzle feedback:** clear reason for invalid area, missing/multiple numbers or overlap; numeric coverage, rectangle count, and a short completion confirmation. Never autogenerate a solution for the player.

**Icon brief:** one deep-green garden boundary around three warm pastel rectangle beds with a small number in each. Original and unrelated to third-party Shikaku app graphics.

**Screenshots brief:** untouched 4×4 garden, first correct rectangle, a 6×6 intermediate board, completed board and controls on a narrow phone. These are future real-device screenshots, not claimed exports.

**Accessibility:** two-tap diagonal corners instead of precision dragging; 52–70 px cells and Undo, Restart and Next buttons. Screen-reader labels identify row, column, clue and occupancy. Nonvisual spatial navigation and real VoiceOver/TalkBack use have **not** been validated; this is a follow-up product test.

**Originality:** own original eight clue boards and styling; generic rules are mathematically reusable, but do not copy competitors' level packs, art or screenshots.
