# I20 — Hint tile wears a breathing honey glow

Date: 2026-11-24. Branch: feat/mosselora-painted-ui (PR #76).

**Player problem.** The second hint step ("Fix one connection") marked its tile with a tiny "?" in a corner, easy to miss and visually noisy.

**Change.** The tile to turn now wears a **soft honey glow ring** around its edge (a sliced ring sprite, outset 5 pt) on top of the existing honey tile tint. With gentle motion on it breathes slowly (a `Breathe` alpha cycle at about 0.4 Hz); with gentle motion off it holds still. The hint button shows its depth as two pips (I13 badge rule).

**Evidence.** b23 live iOS Simulator after two real taps on Guide me (`I20-hint-glow.png`): the glow around row 2, column 2, two filled pips, and "Row 2, column 2: compare the open ends."
