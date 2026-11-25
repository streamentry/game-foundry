# I172 — Every choice is a specific tree, flower or building

Date: 2026-11-25. Branch: feat/mosselora-i127-plus.

## Player

After I171 the catalogue existed only in Core. The game still showed the same shade tree, bridge and wildflower bed on every card, memory, page and painting.

## PM decision (E1 of the end-game plan, `docs/specs/proposals/endgame-village-2026-11-25.md`)

Every place a choice appears now shows **the specific item** for this garden and chapter:
- **Choice cards:** "Plant a quince tree" / "Mend the mill wheel" / "Grow foxgloves", each with its own painted icon.
- **Memory card:** "Mira remembers the quince tree you planted.", with the item painted on a cream disc in the corner of the memory painting.
- **Book:** each page's icon and line, and the open page's line and badge.
- **Ending:** the garden's story shows the twelve real items on their stones.
- **Board:** the protected landmark tile wears the item that rooted it, and the next-stream sign wears it too.
- **Village painting:** each planted or built landmark is the actual item (the nth tree is the nth tree you chose).
- **Gardens advance:** "Explore another garden" moves to the next garden number, which meets the next twelve trees, flowers and builds.

Words: English only, as the catalogue is. Other languages keep their reviewed family lines (`LocalizedVillage`) until translated.

## Dev

- `EvergroveApp`: `gardenNumber` (PlayerPrefs `evergrove:garden-number`, incremented in `ConfirmedReset`). `ItemAt(chapter, family)`, `ItemArt(chapter, family, fallback)` (falls back to the family's own icon while an item's art is missing), `Remembered(choices, index)`, `ChoiceTitle(choice)` and `ItemBadge(painting, chapter, family)`. Every former `ChoiceIcon` and `MemoryAt` call site uses them. `ShowChoiceToChange` takes the sprite.
- `PaintedVillage.ItemSprite(family, nth)`: a callback the app sets. `Place` asks it for each landmark, with the family art as fallback.

## Result (b185 and b186, iPhone 17 simulator; portrait, landscape and 150% tours)

See I173's screenshots: "Plant a silver birch / Raise the weathervane / Grow tulips", "Mira remembers the silver birch you planted." with the birch badge, and varied items in the book and the ending. All tours pass. Verify and the .NET tests pass.

**Note:** this commit's `EvergroveApp.cs` also carries the almanac screen (I174), which was built and checked in the same build (b186).
