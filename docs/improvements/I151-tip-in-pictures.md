# I151 — Mira's first tip is pictures only (blind A/B)

Date: 2026-11-25. Branch: feat/mosselora-i127-plus.

## Player (Haiku walk-through of b152)

"Screen 1, the tutorial bubble: the small text-and-icon row ('Blue waves = spring · green sprout = garden') **reads like a web tooltip**."

Mira's first tip shows pictures (waves → sprout, a finger) **and** a sentence: "Blue waves = spring · green sprout = garden. Tap to connect water." I64 added the sentence because a stand-in player couldn't tell from pictures alone that a tap turns a pipe. Since then the board shows that itself: the turning-arrow disc (I138), the pressing finger on the tile (I147) and the start and goal rings. The brief says to avoid text where possible. Is the sentence still needed?

## Test (blind, between subjects, Haiku as a 61-year-old card and word gamer, one reader per version)

Both readers got the same first-screen screenshot and the same note that the finger presses and the arrow turns.

| | A: with sentence (b158) | B: pictures only (b159) |
|---|---|---|
| Goal | "join the pipes… from the blue spring to the green sprout" ✔ | "turn the pipe pieces so water flows from the spring… to the sprout" ✔ |
| What a tap does | "turn its pipe a quarter turn" (inferred) ✔ | "turn that pipe piece a quarter turn" (inferred) ✔ |
| First tap | "top-left elbow… I'm guessing" | "**the square with the sprout, because the finger is resting on it**" |
| Confidence | 2/5 | 2/5 |
| Confusion | the sentence, the finger and the arrow "both seem to mean tapping"; is the arrow a separate button? | drop meaning, blue vs brown pipes, unlabelled undo/hint |

## PM decision

**Keep B (pictures only).** It understood the goal and the turn as well as A. It followed the board's own demonstration to the right first tile, while A guessed. And A's extra words created an overlap ("is the arrow a separate button?"). The sentence stays as the spoken text for screen readers. Mira's bubble shrinks to one row of pictures, and the board moves up.

**Strength of evidence: weak.** One reader per version, both simulated, and the same 2/5 confidence. This is a provisional call to check in M3. **Reverse if** real first-timers take longer to make a first move without the sentence (the M3 median first-input target is under 15s).

Both readers still question the water drops and the brown versus blue pipes. Those are the next candidates.

## Dev

`EvergroveApp.CoachBubble`: the "Coach words" label is transparent and layout-ignored, keeping its localised text for screen readers.

## Result (b159, scripted tours)

The first board shows Mira's bubble as one row (waves → sprout, finger), with the board higher on the page. All 20 portrait and 5 landscape tour screens are captured. Verify passes. Local A/B images: `docs/improvements/I151-tip-A-words.jpg`, `I151-tip-B-pictures.jpg` (gitignored).
