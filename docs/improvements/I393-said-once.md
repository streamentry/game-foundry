# I393: Said once

Run 3, batch 12 ("fewer words, clearer pictures").

## Player note (P18, first-minute pass on b319)

Every choice screen repeated "Tap a card to compare. Your garden changes only when you confirm." A 40+ player learns that at the first choice; after that it's clutter.

## PM decision

Show the line at the first choice only. After that it is *spoken only*: screen readers still read it, but the screen doesn't show it.

## Dev

- `SpokenOnly(Text)`: transparent text that ignores layout, so it takes no space but keeps its accessible name.
- Used for the choice help when `world.Round > 0`.
