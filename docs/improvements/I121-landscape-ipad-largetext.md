# I121 — iPad and large-text landscape, toured from the shell

Date: 2026-11-25. Branch: feat/mosselora-painted-ui (PR #76).

## Why

iPad landscape and larger-text landscape were unchecked since I114. Simulator rotation needs the interactive simulator panel, which did not have iPad access this session. The text-size launch argument also "did not take": on iOS, Unity reads dev options from `SIMCTL_CHILD_*` environment variables, not from simctl arguments.

## Change (dev builds only)

- `-mosseloraOrientation landscape` (on the simulator: `SIMCTL_CHILD_mosseloraOrientation=landscape`) sets `Screen.orientation = LandscapeLeft` at startup. It is inside `#if UNITY_EDITOR || DEVELOPMENT_BUILD`, and the verifier's dev-control guard reports OK.
- `tools/evergrove_sim_tour.sh <udid> <app> <tag> <portrait|landscape> [textscale]` runs the existing QA tour on any simulator, in either orientation and at any text size, entirely from the shell.

## Results (b121)

| Run | Shots | Finding |
|---|---|---|
| iPad (1180×820 pt) landscape | 5 | The painting fills the left; Mira's note spans it; board, drops, rail, choices and journal sheet fit the right column. Tiles hit the 90 pt cap, leaving empty paper below the board. |
| iPhone landscape, 150 % text | 5 | Mira's note goes sentence-only (picture row hidden above 100 %, as designed). Board and rail fit. Choices stack as wide cards and scroll. The journal reads in the book serif. |
| iPhone landscape | 5 | As verified by hand in I116 and I120. |
| iPhone portrait | 20 (DONE) | Unchanged. |

The scripted tour stops after 5 screens in landscape ("No visible element: Living storybook art"), because it waits for the portrait-only page painting. Five screens cover the first puzzle, the solve, the choice and the journal. Making the tour orientation-aware is the next step if landscape QA becomes routine.

Contact sheets (local only): `I121-ipad-landscape.jpg`, `I121-phone-landscape-150.jpg`.

## Not verified

- iPad portrait at 150 %, Android, real devices, players aged 40+ (**age fit not validated**).
