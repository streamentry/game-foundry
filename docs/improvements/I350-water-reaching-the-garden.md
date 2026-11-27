# I350 — Water reaching the garden

Run 3, batch 3 (P7).

## Player

"The droplet row under the board, some filled and some empty, is unexplained."

## PM decision

The droplet row now starts at the spring (the board's own waves mark, in water blue) and ends at the sprout (the garden). Read left to right, it's water on its way to the garden, one drop per tile reached, in the board's own marks.

## Dev

- `WaterMeter` gains `MeterEnd` glyphs at both ends: `glyph-waves` tinted `WaterAction`, and `glyph-sprout`. The drops and `PaintDrops` are unchanged.

## Result

The meter explains itself without a word. Not checked: the widest boards (more than 16 tiles), where two more glyphs make the row longer.
