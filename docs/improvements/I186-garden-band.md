# I186 — Under every board the garden runs edge to edge

Date: 2026-11-26. Branch: feat/mosselora-i127-plus.

## Player (founder's direction for I185, applied to the rest of the game)

"The garden should be immersive, full top, left and right." I185 did this for the memory card. Under every board, though, the garden was still a framed rectangle inside the page margins: a picture on a page.

## PM decision

Under each board the garden **runs edge to edge** and **melts into the paper at its top and bottom**, so the page reads as board → your land, not board → photo. The size, the swiping, placing things and the neighbours on it are all unchanged.

## Dev

`EvergroveApp.PageGardenMap`: every page map now builds inside a bleed node (`Garden band`; `Garden hero` on the memory card) offset past the column margins (`17 + columnLeft/Right`). Non-hero bands add a 40pt `TopFade` paper melt at the top; all bands keep the 48pt bottom melt.

## Result (b205; portrait, landscape and 150% tours, 23 each)

`I186-garden-band.jpg` (gitignored): under stream 2's board, the winter board and the 150% board, the garden fills the screen width with soft paper edges above and below. Mira, Fen and Orin stand on it, and the memory card hero (I185) is unchanged. Verify passes.

**Noted:** neighbours waiting near the map's edges can be partly outside the window (Fen at the right edge at 150%). It's an open, pannable map, so a swipe shows them, but the waiting spots could favour the window's middle.
