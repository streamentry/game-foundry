# I170 — The sideways memory card keeps its painting (fixes I164)

Date: 2026-11-25. Branch: feat/mosselora-i127-plus.

## Player

I164 pinned the memory card's actions to the foot of the screen and listed "landscape memory card" as unverified: the landscape tour stopped after the journal (6 screens), because it tried to scroll to the village strip, which lives on the left painting when the phone is sideways.

## PM decision

1. Make the landscape tour run the **whole game** (21 screens), so sideways regressions can't hide.
2. Fix what it finds.

## Found

On the sideways memory card, **the pinned bar's paper fade spanned the full screen width** and blanked the lower half of the garden painting and Mira, Orin and Fen's portraits on the left. That was my own I164 regression.

## Dev

- `DevQaTour`: the autumn and winter village shots scroll to the village strip only in portrait. In landscape they capture the screen as it is.
- `EvergroveApp.PinToFoot`: the bar's horizontal offsets follow the page column (`columnLeft`, `columnRight`), and the row inside it is centred in that column.
- Build script: the 150% tour runs on the SE simulator, so its reinstall no longer truncates the landscape tour's screenshots on the shared simulator (b182 had 13 empty files; I re-ran it alone).

## Result (b183, after an Xcode framework-copy retry)

`I170-landscape-bar.jpg` (gitignored): landscape `04` before (b182, cream block over the painting) and after (b183, painting and portraits intact, bar only under the right column), plus portrait `04` (unchanged). The landscape tour now captures 21 screens with no empty files, and portrait passes. Verify passes.
