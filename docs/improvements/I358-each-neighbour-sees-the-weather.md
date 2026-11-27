# I358 — Each neighbour sees the weather

Run 3, batch 5. A follow-up to I357, where every neighbour said the same weather line.

## Player

"All three of them say exactly the same thing about the rain."

## PM decision

Each neighbour notices the weather in their own way:
- Mira through the trees ("the trees are drinking deep; you can almost hear the roots");
- Orin by what's built ("the gutters are singing and the bridge stones shine");
- Fen by the flowers ("every flower's holding a little cup of it").

That's twelve lines in all: three neighbours, four kinds of weather. On fair days they say nothing about the weather.

## Dev

- `DayWeather.Remark(weather, villager)` (Core). Tested: every line is calm and short, and the three neighbours' lines differ for each weather.
- `NeighborSays` passes the speaker.

## Result

The companions have their own eyes, the subtle AI-companion feel the goal asks for, with nothing invented.
