# I306 — A voice for each kind

Idea 47 of the [fifty](../../games/evergrove/docs/specs/proposals/world-games-50-ideas-2026-11-27.md), after the craft of Japanese studios, where every touch answers in its own way.

## Player

"Tiles, cards and seeds all go tock."

## PM decision

Each kind of puzzle has its own touch sound, synthesised in the game, at the pitch of its next note (I305):

| Kind | Sound |
| --- | --- |
| Water | The wooden pipe tock |
| Pairs | A card's brush |
| Mosaic | A glass tile set down |
| Planting by numbers | A soft pluck |
| Garden tiles | A tile's clack |
| Seed sudoku | A little marimba |
| Garden plots | A low warm note |
| Lead the stream | A drop of water |

All are quiet, short (0.16 to 0.26 s), and fade at both ends so they never click.

## Dev

- `GardenSounds` (Unity, new file, kept out of EvergroveApp) synthesises one clip per kind and note, then caches it.
- There are no audio files, so there are no rights questions. Helping a relative uses the thing's own kind too.
- It isn't testable in Core beyond the melody. It was checked in the build; listening needs a device.

## Result

The eight kinds now sound different. Not verified: how the sounds come across through a phone speaker.
