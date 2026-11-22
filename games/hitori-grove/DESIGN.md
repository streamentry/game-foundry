# Hitori Grove — independent visual system (v0.1)

**Positioning:** Leave only one of each. A short, untimed deduction experience for competent adult players. Never imply treatment or improvement of cognitive/medical conditions.

**Three adjectives:** editorial · earthy · contemplative.

| Semantic token | Hex | Role |
| --- | --- | --- |
| Paper | `#F5F1E7` | Background / play surface |
| Ink | `#3F3A35` | Legible numbers, graph labels |
| Accent | `#6B5846` | Primary action, emphasis |
| Pale | `#EAE1D2` | Secondary button / selected tile |
| White | `#FFFFFF` | High-contrast action text |

**Typography:** system fallback, 27–29px headline, 15–17px instructions and result, 16–23px game numbers, 13px helper text. **Spacing:** 16–18px side margin, 10–12px vertical rhythm, ≥48px interactive cells/controls with deliberate whitespace. Scroll on short screens; do not shrink essential controls below 48px.

**Contrast:** information never relies only on colors; numeric clues, state labels, region letters and explicit error messages identify current state. Accent/background combinations require physical-device/automated contrast verification before launch.

**Motion:** immediate, reversible response to taps; no countdowns, flashing, confetti or forced animation. **Sound/haptics:** no default audio or vibration; introduce only if opt-in player testing supports it.

**Puzzle feedback:** Shade repeated numbers, ensuring no horizontally or vertically adjacent shaded squares and one connected group of unshaded squares. Status reflects actual constraint checking; invalid/incomplete states never masquerade as a win. Undo/Restart always visible. Keyboard and screen-reader parity beyond native button labels remains untested.

**Icon brief:** black ink numbers on warm paper, with one softly crossed-out square. Independently drawn brand mark, no copied competitors' assets or trade dress.

**Screenshot plan:** 4×4 numbers unshaded, duplicate-mark decisions, finished connected white cells. Capture only real in-engine scenes; native-device captures not yet produced.

**Accessibility:** large semantic buttons and forgiving input, no mandatory timer, color-independent textual feedback. Nonvisual interaction and high text scaling need real VoiceOver/TalkBack and device testing, **not** assumed to be solved by headless CI.

**Commercial and originality boundaries:** this GF-009 experiment uses generic mathematics/logic with independently authored fixture boards. Store search demand, age-specific interest, differentiation, sustainable retention and pricing are **unmeasured**.
