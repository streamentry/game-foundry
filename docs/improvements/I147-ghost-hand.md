# I147 — A painted finger shows the first tap

Date: 2026-11-25. Branch: feat/mosselora-i127-plus.

## Player (Haiku walk-through of b152, as a 52-year-old who plays Royal Match and Wordscapes)

Top of the "looks like a website" list: "**Screen 1, the tutorial bubble:** the small text-and-icon row ('Blue waves = spring · green sprout = garden') reads like a web tooltip. **A top game would show this as an animated hand tracing one pipe on the board.**"

I138 put a turning-arrow mark on the first useful tile, which shows *what* happens. Nothing showed *where to put your finger*. The finger icon lives only inside the bubble, away from the board.

## PM decision

- A **painted finger rests on that same tile** (fingertip on the tile, 0.46 × tile, dark ink at 82%) and **presses** (down 5pt, 12% smaller) in time with each quarter turn of the arrow beside it. Press, turn, press, turn: the board demonstrates its own verb.
- It leaves with the arrow and glows on the first move. It never blocks taps. With gentle motion off, the hand is still and the arrow doesn't turn.
- **Keep Mira's bubble for now.** Removing words from the first-ever screen is a bigger bet than adding a demo, so it's a candidate for a later iteration once the demo is seen to work with people.

## Dev

`EvergroveApp.TurnMark` also adds a `Shows a tap here` image (`ui-tap`, pivot at the fingertip, registered in `goalGlows`). A new `GhostPress` behaviour runs a 1.45s cycle matching `QuarterTurns` (press from 0.85 to 1.25s as a half-sine).

## Result (b155, iPhone 17 simulator)

- Tour `01-chapter1-start`: the finger sits on the sprout tile under the turning-arrow disc (`I147-ghost-hand.jpg`).
- Hands-on recording, every 0.1s, cropped to the tile (`I147-ghost-hand-frames.jpg`): the arrow's quarter turns are visible between frames, and the finger dips and shrinks just before each one. Both images are local only (gitignored).
- The scripted tour captures all 20 screens. Verify passes.

**Not verified:** whether a first-time player taps the hinted tile sooner than without the hand (M3 first-input timing), and whether the finger hides the pipe shape they need to read (it covers part of the sprout tile's channel until the first move).
