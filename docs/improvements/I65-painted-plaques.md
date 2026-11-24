# I65 — Buttons become painted enamel-and-wood plaques

Date: 2026-11-24. Branch: feat/mosselora-painted-ui (PR #76).

**Player problem.** Buttons were flat rounded rectangles in solid colours, the last element that still looked like a web app rather than a painted storybook.

**Change.** Codex painted three **button plaques** (`art/generated/button-plaques.png`, prompt `art/prompts/plaques.txt`), using the hero painting as the style reference: forest-green enamel and teal enamel, each on a thin warm wood rim with a top highlight, and a cream parchment-wood plaque with grain. The pipeline slices them into 9-slice sprites (`plaque-forest`, `plaque-teal`, `plaque-parchment`; border = 45% of height, so the ends stay round).

Every icon button and text button with those tones now wears its plaque. The plaque is a **layout-ignoring child**: the first attempt let the sliced sprite's natural size inflate the floating menu button to about 125 pt, and that is fixed. The button's own transparent body keeps the hit area and its designed size, and press, disabled and squash states tint the plaque. Icon contrast is unchanged: white on the enamel, ink on parchment.

**Evidence.** b42 live (`I64-I65-coach-plaques.png`): the menu (wood), Undo (forest, shown disabled before the first move), Guide me (teal) and Restart (wood). QA tour: 19 screens.
