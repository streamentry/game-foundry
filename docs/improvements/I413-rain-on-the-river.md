# I413: Rain on the river

Run 3, batch 16 ("the river alive").

On rainy days, faint rings open and fade across the river, several a second, at random spots on the real water cells. With the rain streaks (I354), the whole garden feels wet. None on a winter river.

- `Core/RiverMoments.cs` says *when* (`RainRings(season, weather)`); `RiverLifeLayer` draws: a pool of up to 24 Halo rings, each growing and fading over about a second.
- Cosmetic only, like all weather (I353). Tested.
