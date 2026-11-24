# I44 — Accessibility review: name plates, larger text, visible empty drops

Date: 2026-11-24. Branch: feat/mosselora-painted-ui (PR #76).

**What.** A Haiku sub-agent, acting as a WCAG 2.2 AA mobile specialist, reviewed eight full-resolution b31 screenshots for older players. Its findings and what was done:

| Finding | Action |
| --- | --- |
| White neighbor names over the painting, about 2:1 | **Fixed.** Names sit on a cream name plate (`#FFF8EA` at 94%) in ink, well above 7:1. The screen-reader caption lookup now reads text one level down, so Mira, Orin and Fen are still spoken (b32 SR tree). |
| Menu footer and privacy lines at 12–13 pt grey | **Fixed.** 15 pt in ink. |
| Choice-card consequence text at 13 pt grey | **Fixed.** 14 pt in ink. |
| Scrapbook page text at 13 pt | **Fixed.** 14 pt. |
| Empty droplets about 1.3:1 | **Fixed.** A grey silhouette (`#666F66` at 50%). The exact count stays spoken (I27). |
| Level dots about 6 pt | **Fixed.** 10 pt. |
| Disabled Undo, sage at about 3.4:1 | **Kept.** WCAG 1.4.3 exempts inactive components, and the paler tone is the intended "nothing to undo" signal. |
| Text labels under the menu and book icons | **Kept.** This conflicts with the icon-first brief. Names show on press-and-hold (I-tips) and are spoken. |

The reviewer found **no touch targets under 44 pt** and judged the pipe colour and dash cues clear.

**Evidence.** b32 QA tour, 19 screens (`I44-a11y-fixes.png`).
