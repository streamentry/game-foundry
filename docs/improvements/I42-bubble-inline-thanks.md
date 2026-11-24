# I42 — Speech bubble's confirm tucks into its corner

Date: 2026-11-24. Branch: feat/mosselora-painted-ui (PR #76).

**Player problem (I40 nit).** The neighbor's confirm button sat on its own row inside the speech bubble, leaving a tall empty bubble under a one-line remark.

**Change.** The confirm button (44 pt) is tucked into the bubble's lower-right corner, outside the text flow, with bottom padding reserved, so the bubble is only as tall as the words plus the button.

**Evidence.** b31 live: tapping Orin on the hub shows "I'm Orin. I hope we can rebuild the crossing." with the check in the corner (`I42-bubble-inline-thanks.png`).
