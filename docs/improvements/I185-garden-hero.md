# I185 — The memory card opens on a full-bleed garden with the title floating on it

Date: 2026-11-26. Branch: feat/mosselora-i127-plus.

## Player (founder, with a screenshot of the memory card)

"The garden should be immersive, full top, left and right, and the logo, tagline and button can float over it."

The memory card's garden map was a framed rectangle inside the page margins, with the wordmark and tagline stacked above it on plain paper: a web page with a picture in it, not a world.

## PM decision

- **The memory card opens on your garden, full bleed**: edge to edge and up under the status bar (420pt tall, 360pt at larger text), melting into the paper at its foot.
- **The wordmark floats on the garden**, and the tagline sits on a small cream plate so it reads over any grass. The menu and book buttons already float above everything.
- **The new thing in your hand sits lower in view** (70% down), so the floating title never covers it, and the year's items now start five rows down the valley instead of at the top edge.
- To allow full bleed, **upright pages now reach the top of the screen**: the status-bar height became padding (and the chrome and its scroll band move down by it), so nothing else on the page moves.

## Dev

- `FitSafeArea`: upright, the scroll area's top anchor is the screen top. `FitColumn` computes `topInset` (status bar in canvas units) and adds it to the column's top padding, the chrome's position, and the chrome band's height, solid and fade.
- `EvergroveApp`: `heroGarden` on the living memory card skips `GardenHeader` and the tagline. `PageGardenMap` makes the map window the page's first child and bleeds a `Garden hero` past the margins and up by `15 + topInset`, adds a paper melt, and `HeroTitle()` floats the wordmark and the plated tagline.
- `GardenMapView.FocusFromTop` (default 0.5; the hero uses 0.7).
- `GardenMap`: the seasonal band starts at row 5 (was 0). Tests pass unchanged.

## Result (b200 → b204; portrait, landscape and 150% tours, 23 each)

`I185-garden-hero.jpg` (gitignored): the memory card at 100% and 150% shows the garden from the top of the screen, the wordmark and plated tagline floating on it, the new silver birch in its ring with the pointing finger below the title, the planting tip, then the memory card. The stream 2 board (third panel) is unchanged.

- b200's scrolled boards showed the page under an uncovered status bar: the chrome band's solid and fade hadn't grown. Fixed in b201.
- b201 to b203 still had the birch under the title. Fixed by the row-5 start and an explicit focus (b204).

Verify and the .NET tests pass.

**Noted:** on the board pages' smaller map, a neighbour waiting near the top can be cut off by the window's top edge.
