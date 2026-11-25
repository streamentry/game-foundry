# I155 — Undo appears only when there is something to undo

Date: 2026-11-25. Branch: feat/mosselora-i127-plus.

## Player

Every board opens with **two** tokens under it: Hint (useful, since you may be stuck from the start) and Undo, greyed out because there's nothing to undo yet. A greyed control is a web pattern ("disabled button"): something to look at, decode and ignore, on every one of the 12 boards. The brief asks for this directly: "buttons and navigation should be smart, does not need to display all buttons everytime".

## PM decision

**Undo appears only when it can do something.** Before the first move on a board its place is empty: the space is kept, so the drops and Hint don't shift. On the first move it fades in over 0.25s (instantly with gentle motion off). After a player undoes back to the start, it fades out again. Hint stays visible throughout.

## Dev

`EvergroveApp.ShowUndo(available, animate)` drives a `CanvasGroup` on the Undo token (alpha 0 or 1, raycasts blocked when hidden). It's called when the tools are built (no animation) and from `RefreshBoard` (animated) next to the existing `interactable` update. `FadeGroup` is a small reusable coroutine for the fade. The QA tour skips the fade.

## Result (b163, iPhone 17 simulator, scripted tours)

`I155-undo-when-useful.jpg` (gitignored): `01` (first board, no move yet) shows only the drops and Hint, with Undo's place empty. `02` (after moves) shows Undo at its usual left place. `07` (start of stream 2) has no Undo again. In the landscape rail, only Hint shows at the start. The board, the drops and Hint keep their positions throughout. All 20 portrait and 5 landscape screens are captured. Verify passes.

**Not verified:** whether players look for Undo before their first move (they can't need it then), and VoiceOver: the hidden token is non-interactable, but its label may still be listed.
