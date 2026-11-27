# I433: The tour counts the atmosphere

Run 3, batch 20 ("proof").

## Why

The b324 tour didn't visibly show the morning mist (I409) at dawn or the footprints (I412) in winter, though the build contains both. A faint effect can hide in a screenshot; a missing one looks the same. I want ground truth, not a guess.

## Dev

After each season shot (`19-*-home`) and each light shot (`20-light-*`), `DevQaTour.LogAtmosphere(shot)` writes one line to the tour log: `MOSSELORA_ATMOSPHERE <shot> | Morning mist=n | Dew=n | Frost=n | Chimney smoke=n | Footprint in the snow=n | The river's life=n | Ring on the water=n | Glitter on the water=n | Moonlight on the water=n | A leaf on the water=n | A lantern by the seat=n`. It counts active RectTransforms by name, so it costs nothing at runtime and is dev builds only. The result is recorded in I437.
