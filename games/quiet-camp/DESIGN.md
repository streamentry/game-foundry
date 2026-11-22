# Quiet Camp — standalone design system (v0.1 playtest)

**Positioning:** a peaceful spatial logic clearing. Every tree deserves a campsite; every tent has room to breathe. No health, memory improvement, or age-stereotype claims.

**Three adjectives:** woodland · clear · deliberate.

| Token | Hex | Role |
| --- | --- | --- |
| Warm moss paper | `#F4F8EF` | Page |
| Deep pine ink | `#273F37` | Text |
| Secondary olive | `#52665D` | Supporting text |
| Forest accent | `#2E6955` | Primary actions and matched counts |
| Light moss | `#E1ECDE` | Secondary actions |
| Playable grass | `#FDFBF3` | Blank space |
| Fixed tree tile | `#CCE0D1` | Noneditable cells |
| Tent tile | `#F4DCCB` | Occupied cells |
| Tent canvas | `#CB7957` | Original vector tent |
| Tree canopy | `#2E6955`, `#78A489` | Original pine geometry |
| Warning | `#9A5244` | Violation copy |

**Typography:** system face, 27px title, 15–16px gameplay feedback, 19px count labels, 13–14px rule notes. No text embedded inside complex sprites.

**Sizing:** 16px outer padding, 11px vertical rhythm, 3px grid gutter; 5×5 boards use 56px touch cells; 6×6 boards use 49px cells; navigation/actions ≥54px. Scroll rather than shrink text or crowd a phone.

**Contrast:** dark text on light paper, pale text on deep-green primary buttons; clues always have numeric labels. A row/column count changing color only is secondary feedback, never the sole way to diagnose a solution. Test actual screen size and contrast on devices.

**Pacing and interaction:** one tap cycles tent → grass → blank. No press-and-hold, precision swipe, timer, streak, ads or forced animation. Undo and Restart are always available. Original vector pine and tent art avoids external images and asset rights complexity.

**Feedback:** counts above columns, beside rows; a textual status for too many tents, touching diagonals, overfilled lines or impossible tree pairing. Completing all constraints unlocks Next. The matching rule is explicit: trees may have several adjacent tents, but must pair bijectively.

**Motion:** immediate state change, no confetti. **Sound/haptics:** none in v0.1; opt-in only if justified by player testing.

**Icon brief:** one small terracotta tent sitting beside a distinct tall pine in an open sage clearing, simple two-tone vector silhouette. No borrowed competitor shapes or logos.

**Screenshot brief:** first 5×5 clearing with counts, a marked grass cell, a text explained diagonal violation, a 6×6 solved clearing. All are future original physical-device screenshots, not existing assets.

**Accessibility:** native Button controls for all editable cells, ≥49×49 logical px touch area, semantic cell names, Undo/Restart, no timed actions, readable numeric totals. **Nonvisual inference of grid positions is not yet a fully accessible play route**; VoiceOver/TalkBack and text scale need direct user testing before release.

**Evidence boundary:** uniqueness/constraint tests are not commercial or age-specific validation. This is a GF-006 **hypothesis**. See [experiment brief](../../docs/research/next-game-experiments-2026-11-22.md).
