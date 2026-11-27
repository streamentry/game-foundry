# I316 — The sound of the hour

Idea 49 of the [fifty](../../games/evergrove/docs/specs/proposals/world-games-50-ideas-2026-11-27.md), after ambient Japanese puzzle games where sound is a place, not a soundtrack.

## Player

"It's very quiet at home once the story's creek stops."

## PM decision

Over home, now and then, you hear a voice of the season and the hour:
- birds by day;
- crickets on summer and autumn evenings;
- wind through winter;
- the odd drop into the stream.

There are one or two voices at a time, sparse (about every 4 to 11 s, varied), and silent while a puzzle or the family page is open. It's cosmetic like the light (I272), and the sound switch silences it. Everything is synthesised in the game, with no recordings and no rights questions.

## Dev

- `Soundscape` (Core, new): `For(season, phase)` and `SecondsBetween(voice)`. The test was written alongside the class, not before it. It checks one or two voices always, no birds in the dark, crickets only on summer and autumn evenings, winter always windy, a spring dawn chorus, and sparse timing.
- `GardenSounds.Ambient` makes three variants per voice. `EvergroveApp.TickSoundscape` runs in `Update`, uses the season and hour (or "day" when the light switch is off), and skips while a card is open.

## Result

Home has an ambience. It can't be judged from screenshots: it needs listening on a phone, and real players may well prefer silence. The sound switch is the escape.
