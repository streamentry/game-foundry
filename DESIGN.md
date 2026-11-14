# DESIGN.md — Product taste and design systems

## Product position

Calm, precise, joyful games for a player solving a real puzzle—not a feed to maximize compulsion. Every app should feel like a carefully designed standalone product, **not a reskin**.

### Non-negotiable shared principles

- **Clarity before decoration.** The next meaningful action is obvious in under 3 seconds.
- **Respect attention.** No forced streak anxiety, dark patterns, confetti overload, or deceptive ad controls.
- **Accessibility.** Touch targets at least 48 logical px, scalable legible text, sufficient contrast, icon + text cues, no color-only correctness signals, no mandatory timers.
- **Responsiveness.** Test compact iPhone, large iPhone, iPad, Android; safe areas and portrait/landscape as relevant.
- **Fast first play.** Launch to a playable first move; no mandatory account or backend.
- **One-handed basics.** Undo, notes and restart reachable without acrobatics; don't obscure the board with ads.
- **Physicality without clutter.** Short, meaningful transitions; avoid heavyweight GPU effects in grid/word games.
- **Originality.** Distinctive mechanics, content, educational value, accessibility or interaction: visual styling alone is not a sufficient differentiator.

### Shared tokens vs independent brands

Reuse semantic tokens (surface, ink, accent, warning, spacing, corner radius, motion duration), interaction standards and QA tests. Each **game** must declare a unique art direction, logo, typography, icon treatment, emotional promise, puzzle language and signature interaction in its own `DESIGN.md`.

A per-game spec must include:
`positioning`, `three adjectives`, `type scale`, `colors (hex)`, `contrast`, `spacing`, `motion`, `sound/haptics`, `puzzle feedback`, `icon brief`, `screenshots`, `accessibility`.

### Reference lab — Takuzu

This is an engineering demo, **not** the brand for the eventual Takuzu product.
- Mood: **Quiet clarity / tactile notebook / soft precision**.
- Ink `#20352F`; paper `#F5F3EB`; fresh leaf `#CCE3D5`; warm coral `#EBA77E`; error `#9E3E40`.
- 4×4 grid, no timer, bold numbers, disabled givens, inline rule feedback, undo and restart.
- Avoid implying health benefits from brain games without appropriate evidence.

### Launch quality bar

A stranger should understand the rules quickly, play offline, resume seamlessly and see original value on the first screen. Visual QA is required on **real phones**; headless CI alone cannot validate taste.
