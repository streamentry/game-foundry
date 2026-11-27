# I299 (fit) — The cottage card fits the screen

Date: 2026-11-28. Branch: `fix/mosselora-cottage-card-landscape`.

## Problem

On an iPhone held sideways, the cottage summary card (`EvergroveApp.OpenCottage`, I299) was taller than the screen. The card was centred on the screen and sized only by its contents, so its top lines, the garden's name and the season, sat above the top edge and couldn't be seen. Its last rows, decorate and look out, sat below the bottom edge. The undo token in the decorate row was also clipped at the card's right edge, because the card was only 360 pt wide. Evidence: companion tour `tour-c7l/17j5-cottage.png` (iPhone 17 Pro, landscape).

## Change

- **Capped to the safe area, with scrolling lines.** The card's lines now sit in a scroll view inside the paper. The paper is as tall as its lines but never taller than the safe area minus 12 pt top and bottom. It is centred in the safe area rather than the whole screen, so the Dynamic Island and home bar never cover the first or last line. The lines scroll only when there are more than fit, and they always open at the top, with the garden's name first.
- **Sideways, a little tighter and wider.** In landscape the card is up to 460 pt wide instead of 360 pt, so long lines wrap less and the decorate row's five tokens fit. Line spacing is 6 pt instead of 10, the inset is 12 pt instead of 18, and the line pictures are 36 pt instead of 44. Text sizes are unchanged.
- **Unchanged:** upright at 100% text, the card looks as it did. The card still shows the same lines in the same order; this change only alters the container they sit in (the `paper` variable that every line is added to is now the scrolling column). SC05's ordering of the waiting lines (`CompanionAttention.CottageOrder`) is not touched.
- **QA tour:** after `17j5-cottage.png`, the tour scrolls the card down by most of a view at a time and saves `17j5-cottage-2.png` and so on, until it reaches the end. For these shots only, it names the garden "Willow Bend" (QA-only `QaGardenName`), so the title line shows. The tour had never named the garden, so the line that was reported missing never appeared in any tour shot.

## Evidence (iOS Simulator, iPhone 17 Pro, own "Mosselora Cottage QA" simulator)

| Tour | Build | Cottage shots | Every line on screen |
|---|---|---|---|
| Landscape (`tour-k2l`) | k2 | 2 (top, then the end) | yes |
| Portrait 100% (`tour-k2p`) | k2 | 1 (no scrolling needed) | yes |
| Portrait 150% (`tour-k2x`) | k2 | 2 | yes |

- Unity iOS-simulator builds k1 and k2: 0 `error CS`, and the built `.app/Info.plist` exists.
- `games/evergrove/Tests` (`dotnet run`) passed, including `LongPressCaptionTests` (the cottage backdrop is still the only raw button in `OpenCottage`).
- Contact sheet `/Volumes/SSD/mosselora-builds/painted/cottage/I299-cottage-card-fits-the-screen.png` (not in git; `docs/improvements/*.png` is ignored): the top row shows portrait 100%, then portrait 150% (top, end); the bottom row shows landscape (top, end). The full shots are in the `tour-k2*` folders beside it.

## Still unverified

- Real devices, and the iPad layouts (the card uses the same cap, but no iPad tour was run).
- Whether players realise the card scrolls when it is cut off at the bottom. The cut-off row is the only cue. No players aged 40+ have tried this.
