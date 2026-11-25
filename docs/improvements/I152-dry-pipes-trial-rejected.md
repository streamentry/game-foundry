# I152 — Paler dry pipes tested blind and rejected

Date: 2026-11-25. Branch: feat/mosselora-i127-plus.

## Player (two blind Haiku readers in I151)

Both readers asked why some pipes are brown and others blue ("The blue and brown pipes look different, and I don't know why."). By the logic of I146 (empty should look empty), the saturated copper of dry pipes might read as a different *kind* of pipe, not as a pipe *without water*.

## Trial

1. **Stone grey** (`#8E8172`, b160): the teal water popped, but the board lost the warm, earthy look that matches the paintings (`I152-grey-trial.jpg`).
2. **Warm sandstone** (`#94806A`, about 3.2:1 on the dry tile, b161), with matching collar and dashed stripe (`I152-sandstone-trial.jpg`).

Blind comparison (Haiku as a 58-year-old, P = copper b159 against Q = sandstone b161, same autumn 4×4 board):
- Meaning: "In both, the brown pipes look like plain, unfilled pipe. **I can't see a difference** between P and Q."
- Clarity of "no water yet": **same**. Fit with the painted art: **same**.
- Risk: "the brown pipes are **fairly low-contrast against the cream tiles**, so they could be harder to read at a glance."

## PM decision: rejected, reverted

The paler channel bought no measurable gain in meaning or appeal and costs contrast: copper is about 5:1 against the dry tile, sandstone only 3.2:1. For players over 40, contrast outranks a theory. **No code change ships.** Copper stays, and the "blue versus brown" question is better answered by motion (I144: water visibly runs into the brown pipe and turns it blue) than by recolouring.

This entry records a tested-and-rejected idea, so it isn't tried again without new evidence (for example a real player who reads copper pipes as already full).

## Result

The source is unchanged (`MosseloraTheme.cs` and `PipeGraphic.cs` reverted). The trial builds b160 and b161 stay on the SSD. Local trial images are gitignored.
