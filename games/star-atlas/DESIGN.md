# Star Atlas — independent visual system (v0.1)

**Positioning:** Five stars. A little order. A short, untimed deduction experience for competent adult players. Never imply treatment or improvement of cognitive/medical conditions.

**Three adjectives:** celestial · serene · deliberate.

| Semantic token | Hex | Role |
| --- | --- | --- |
| Paper | `#F2F4FA` | Background / play surface |
| Ink | `#27364D` | Legible numbers, graph labels |
| Accent | `#465F93` | Primary action, emphasis |
| Pale | `#DDE5F2` | Secondary button / selected tile |
| White | `#FFFFFF` | High-contrast action text |

**Typography:** system fallback, 27–29px headline, 15–17px instructions and result, 16–23px game numbers, 13px helper text. **Spacing:** 16–18px side margin, 10–12px vertical rhythm, ≥48px interactive cells/controls with deliberate whitespace. Scroll on short screens; do not shrink essential controls below 48px.

**Contrast:** information never relies only on colors; numeric clues, state labels, region letters and explicit error messages identify current state. Accent/background combinations require physical-device/automated contrast verification before launch.

**Motion:** immediate, reversible response to taps; no countdowns, flashing, confetti or forced animation. **Sound/haptics:** no default audio or vibration; introduce only if opt-in player testing supports it.

**Puzzle feedback:** Place exactly one star in each row, column and labeled connected region, without allowing stars to touch even diagonally. Status reflects actual constraint checking; invalid/incomplete states never masquerade as a win. Undo/Restart always visible. Keyboard and screen-reader parity beyond native button labels remains untested.

**Icon brief:** five separate ink-blue stars distributed among pale irregular map regions. Independently drawn brand mark, no copied competitors' assets or trade dress.

**Screenshot plan:** new 5×5 board with region letters, partially filled constellation, solved stars. Capture only real in-engine scenes; native-device captures not yet produced.

**Accessibility:** large semantic buttons and forgiving input, no mandatory timer, color-independent textual feedback. Nonvisual interaction and high text scaling need real VoiceOver/TalkBack and device testing, **not** assumed to be solved by headless CI.

**Commercial and originality boundaries:** this GF-005 experiment uses generic mathematics/logic with independently authored fixture boards. Store search demand, age-specific interest, differentiation, sustainable retention and pricing are **unmeasured**.
