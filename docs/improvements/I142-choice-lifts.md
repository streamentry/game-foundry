# I142 — A chosen card lifts before the memory arrives

Date: 2026-11-25. Branch: feat/mosselora-i127-plus.

## Player (screen recording of a choice, b145)

Tapping "Restore a crossing", the card squashes under the finger, and then **the whole page swaps** to the memory card with a page fade. Nothing says "this one, not the others". For a choice the game says it will remember (and that changes the next board, I134), the moment of choosing is the least ceremonial thing in the loop. It feels like submitting a form, not placing a bridge.

## PM decision

When a card is tapped, it **lifts** (scale 0.94 → 1.07) and the other two **step back** (fade to 35%) over a quarter second. Then the memory page fades in as before. That's one beat of "you chose this". It adds 0.24s to a decision made 12 times a game, a fair price for the core moment.

- Respect motion: with gentle motion off, choosing is instant, as before. The QA tour also chooses instantly.
- Taps during the lift are ignored (the cards stop taking input), so there's no double choice.

## Dev

`EvergroveApp.cs`:
- Choice cards call `SettleChoice(cards, choice)` instead of `MakeChoice` directly.
- `LiftChosen` coroutine: it marks every card's `CanvasGroup` non-interactable, removes the chosen card's `PressSquash` (so the two don't fight over scale), and eases the chosen scale and the others' alpha over 0.24s. Then it calls `MakeChoice`, unless the page was rebuilt meanwhile (guarded by `rebuildGeneration`). A `settlingChoice` flag blocks re-entry.

## Result (b147, iPhone 17 simulator, hands-on choice, recorded every 0.1s)

- Frames 11–13 in `I142-choice-lifts.jpg` (gitignored): the bridge card grows while the tree and flower cards fade to pale. Frame 14 starts the memory page fade, and Orin's bridge memory settles by frame 16.
- The scripted tour (instant choice) captures all 20 screens. Verify passes.

**Not verified:** whether 0.24s feels deliberate or slow on a real phone. If players tap twice out of impatience, shorten it.
