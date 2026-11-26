# I168 — Book pages wear a folded corner so they invite a tap

Date: 2026-11-25. Branch: feat/mosselora-i127-plus.

## Player

I166 made every page in the book open to its memory, but nothing said a page *could* open. The pages looked exactly as before, flat paper cards. A cold read had already asked "whether they're tappable". A feature nobody discovers doesn't exist.

## PM decision

Give each memory page a **folded top-right corner (dog-ear)**, the universal "this page turns" cue from real scrapbooks and books. It's an affordance drawn in the world's own material, not a word or an arrow. Pages without a memory (none today) wouldn't get one.

## Dev

`EvergroveApp.DogEar(page)`: adds a `RectMask2D` to the page and a 26pt rounded square in the card-edge tone, rotated 45° and centred on the page's top-right corner. The page clips it into a triangle, with a soft lift. It's added only to tappable memory pages.

## Result (b180, iPhone 17 simulator, tour `10-journal`)

`I168-dog-ears.jpg` (gitignored): each page in "Our choices" shows a small folded corner at the top-right. Stones, numbers, paintings, portraits and lines are unchanged. Portrait, landscape and 150% tours pass (21/6/21). Verify passes.

**Not verified:** whether players now tap pages unprompted (worth one question in M3: "Is there more in the book?").
