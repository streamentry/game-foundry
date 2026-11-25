# I182 — The map shows a first-time player how to plant

Date: 2026-11-26. Branch: feat/mosselora-i127-plus.

## Player (Haiku cold read of b195, as a 64-year-old Gardenscapes player)

On the memory card with the new birch in hand: "**I would not know I can move the birch.** The ring and the small pressing-finger mark look like decoration, and the only clear action is the big teal arrow. I'd press the arrow and assume the tree is already planted." It also said the garden "feels like a picture", and that putting the tree where *I* chose would make it feel like mine.

That's the whole open-world idea failing silently. The verb exists, but nobody finds it.

## PM decision

The brief prefers pictures to words, and I151 removed words from the first tip, but this is a *new verb* that a cold read missed entirely. So, once:
- **Until the player has placed something for the first time**, a small card sits at the bottom of the map: the item's picture, a pressing finger, and "**Tap the grass to plant your silver birch there.**" ("grow your … " for flowers, "build your … " for builds). Pictures only in other languages.
- After the first placement it never appears again (device setting `evergrove:placed-once`). The ring and finger stay as the quiet reminder.

## Dev

`EvergroveApp.PlaceTip(window, held)`: a card fill with lift, 38pt item and finger glyphs, and the English line at 16pt bold, anchored to the map window's bottom. It's added by `PageGardenMap` when an item is in hand and the flag is unset. `OnMapLand` sets the flag on a successful placement.

## Result (b196; portrait, landscape and 150% tours, 23 each)

`I182-place-tip.jpg` (gitignored): portrait and 150% show the birch in its ring by the cottage, and below it the card "Tap the grass to plant your silver birch there." It's readable at both sizes and doesn't cover the birch. Verify passes.

**Found:** in **landscape** the memory card has no garden map at all (the left side is still the old painting), so sideways players can't place anything. That's I183.
