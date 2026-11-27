# I403: The garden's ways

Run 3, batch 14 ("the garden's ways, alive").

## Player note (P24, b321 almanac)

With seven visitors, seven good neighbours and four seasons, the almanac had become a long, text-heavy scroll. The collections it was made for sat underneath.

## PM decision

Give the garden's logic its own tab in the book, with a sprig icon: **The garden's ways**. It holds the good neighbours (with what each brings, I401), who visits and why, and the year. The almanac goes back to being a collection: the memory shelf and the kinds you've grown.

## Dev

- `waysTab` sits beside `almanacTab` and `familyTab`. Every tab and the close button reset it.
- `WaysPage()` shows `PairingRows`, `VisitorsRows` and `YearRows`. `AlmanacPage()` keeps the shelf and the collections.
- The tour adds `10e-garden-ways.png` after `10d-almanac.png`.
