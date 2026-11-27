# I339 — Autumn you can see

Run 3, batch 1. The founder's season question, continued.

## Player

"Autumn turns the grass golden, but my trees are still summer green."

## PM decision

In autumn the trees turn: a warm amber tint. Fallen leaves gather at their feet, and the drifting leaves are fuller (20). Flowers keep blooming, as asters and dahlias do.

## Dev

- `ThingLook.Turning` for trees in autumn (`SeasonLook`, tested).
- `DressForSeason` tints the painting and scatters six fallen leaves at its foot, in two russet tones.
- `SeasonDrift` takes its count from `SeasonLook.DriftCount`.

## Result

The trees join the season. The tint is a multiply over the painting, so a turned tree keeps its own shapes.
