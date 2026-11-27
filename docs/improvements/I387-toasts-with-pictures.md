# I387: Toasts with pictures

Run 3, batch 10.

## PM decision

The goal says to avoid text where possible. The good-neighbours toast now shows the two partners' paintings side by side above its words, for example a flower and a tree, or a built thing and a wet stone for the water. The bees' toast shows a flower and a pear. The picture says it first and the words confirm it. Toasts with pictures stay a second longer (3.6 s).

## Dev

- `RuleToast.Show(..., Sprite[] marks)` adds a row of 46 pt paintings above the words. The old signature is unchanged.
- `PairingMarks(pairing)` gives two paintings per pairing; the orchard gets three trees.
- `toastMarks` travels with the queued toast.
