# I150 — The sideways book is back on screen (fixes I128)

Date: 2026-11-25. Branch: feat/mosselora-i127-plus.

## Player (iPad landscape tour, b157; then the phone landscape tour from I142 looked at again)

On an iPad held sideways I opened the book. The sheet started near the middle of the screen and **ran off the right edge**: the second column of pages was cut ("Mira remembers the tre…") and **the close button was off screen**. The only way out was tapping the dimmed garden. The phone landscape tour (b142/b152) showed the same cut, which I had missed.

## Cause: my own regression from I128

I128's `FitSheetToPages` set the panel's top anchor as `new Vector2(1f, height)`. In portrait the x anchor is already 1, so nothing changed. Sideways, `FitSheet` (I114) centres the sheet with x anchors of 0.5 and ±260pt offsets. Forcing x to 1 moved the right edge 260pt past the screen.

## PM decision

Fix it now, before any new feature. The sizing touches only the height: it keeps whatever x anchor the orientation set, and caps at the orientation's own maximum (92% sideways, 86% upright).

## Dev

`EvergroveApp.FitSheetToPages`: `anchorMax = (panel.anchorMax.x, clamp(wanted / height, 0.34, cap))`, where `cap` is 0.92 for a centred (landscape) panel and 0.86 otherwise.

## Result (b158, scripted tours: iPad landscape, iPhone landscape, iPhone portrait)

`I150-sideways-sheet.jpg` (gitignored), left to right: iPad b157 (cut, no close button), iPad b158, phone landscape b158 and phone portrait b158. Both landscape sheets are centred, with the close button and both page columns visible, and portrait is unchanged. All tours complete (iPad 5, phone landscape 5, portrait 20 screens). Verify passes.

**Lesson recorded:** after any change to shared sheet or layout code, re-run the landscape tours, not only portrait. I128 was verified in portrait only.
