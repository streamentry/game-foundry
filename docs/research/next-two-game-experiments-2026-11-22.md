# Next two game experiments: Shikaku Garden and Odd Bloom

**Decision date:** 2026-11-22. **Status:** engineering playtest prototypes at user direction; no market or production approval.

After the three visual mechanic prototypes, the user requested further games. We are deliberately trying **two very different interaction costs**, not inventing a commercial ranking.

| Game | Player action | What it tests | Strongest objection | Minimal real-world test |
| --- | --- | --- | --- | --- |
| **4. Shikaku Garden** ([GF-018 research hypothesis](../ideas/candidates.csv)) | Choose two opposite corners; partition numbered squares into valid rectangles. | Does calm, deeper spatial reasoning earn 10+ minutes of optional engagement? | The classical genre is mature and its exact-match search demand is still unknown. | 6–10 age-relevant adults, 5 introductory puzzles each, compared with a direct Shikaku incumbent. |
| **5. Odd Bloom** (new exploratory mechanic, **not** a measured keyword) | Tap the one structurally different flower. | Does instant, language-light first-play comprehension generate genuine voluntary repeat behavior? | Well-known odd-one-out games exist; the fast visual novelty may not hold attention or be visually comfortable on small devices. | 6–10 age-relevant adults, first 5 patterns and a later optional return, against a competing odd-one-out game. |

## Why these two

Shikaku adds a **search-led classical format** that was already present in the research backlog, but no search-popularity figures, competitor conversion gap, or paying segment are verified. Odd Bloom adds an extreme opposite **low-instruction one-tap** game to contrast with Good Eye's estimation, Quiet Shot's trajectory prediction, One Path's edge planning and Shikaku's partition reasoning.

These are not "next best global markets". They are technical experiments to expose whether players seek precision, strategy or instant recognition. **Five built games with no representative player test are still zero validated game opportunities.**

## Product boundaries

- Original playable offline-first Godot 4.7.2 projects, each with its own `DESIGN.md`, tests, export preset and artifact CI.
- No backend, automatic store release, tracking SDK, purchase system or repetitive app submissions.
- Link-based challenges are usable only once a build is actually hosted on an approved HTTPS site. An artifact is not a public URL.
- Automated tests establish rule correctness and deterministic content, **not** age-specific preference, retention, ASO demand, referral activation, or economics.
- Target cohorts must include adults 45+ from outside the dev circle. Record age bands separately. Never present a convenience sample as global adoption.

## Falsification and stop conditions

For Shikaku: if users can solve the first two puzzles comfortably but repeatedly prefer a direct established alternative after one concrete UI correction, **park**. For Odd Bloom: if most participants complete the first attempt but do not independently choose additional rounds or return the next day, **park**, rather than building 200 more vector patterns. For either, compare actual unaided comprehension, voluntary replay and delayed return; counts must have denominators and recorded versions. Predeclare thresholds in the playtest plan before recruiting; do not retroactively label successes.

## Next observable action

Share browser builds to a small group on their own phones, rotate which game they try first, and ask **which one they voluntarily reopen later without prompts**. This produces substantially more information about a viable game than another implementation sprint.

**Confidence:** high in feasibility of these mechanics; low in their acquisition and retention potential until measured.
