# I305 — Moves that make a tune

Idea 4 of the [fifty](../../games/evergrove/docs/specs/proposals/world-games-50-ideas-2026-11-27.md), after Japanese music puzzle games and the tune under the Russian falling-blocks game, where playing well sounds well.

## Player

"Every move makes the same tock. Ten minutes in, I turned the sound off."

## PM decision

Each move plays the next note of a short phrase, eight notes, and each kind of puzzle has its own phrase. The notes come from the major pentatonic scale, so any run of moves is in tune, and no step leaps more than a fifth. A solve ends on the chime, C and E an octave above middle C, so the phrase comes home. The sound setting still silences it all. No sound carries meaning that isn't also shown.

## Dev

- `Melody` (Core, new): `Scale`, `NoteAt(kind, move)`, `Home(kind)` and `Hertz(semitones)`. The tests ran red first (15 errors). They check, for every kind:
  - every note is in the scale;
  - no leap beyond a fifth, including the wrap;
  - the phrase repeats;
  - each kind's phrase differs;
  - comfortable pitches (200 to 1,200 Hz);
  - the tonic home;
  - correct tuning from middle C.
- `EvergroveApp.PlayMove(kind)` counts moves per board (`moveNote` resets when a board opens) and plays the clip.

## Result

Solving is also a small tune. It's unheard by players; whether 40+ listeners find it pleasant or fussy is a question for the playtest. The sound can be switched off as before.
