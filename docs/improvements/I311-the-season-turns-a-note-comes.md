# I311 — The season turns, a note comes

Idea 26 of the [fifty](../../games/evergrove/docs/specs/proposals/world-games-50-ideas-2026-11-27.md), after farming life games, where each new season greets you.

## Player

"The garden changed to autumn and I almost didn't notice."

## PM decision

When the garden's season turns, which happens by progress and never by the clock (I248), a neighbour leaves a note in the letterbox, naming the new season. It flies into the box as letters do (I237) and keeps until read. The first time home is seen, the season is only recorded, so a new or restored garden doesn't get a sudden note. Notes never ask anything of the player.

## Dev

- `EvergroveApp.NoteSeasonTurn` runs when home's map is built. It compares `GardenSeason.For(world)` with the season last seen for this garden (`evergrove:season-seen:<garden>`), and on a change keeps a note through `WorldModel.KeepNote` (validated as a letter).
- What the note says is I312.

## Result

Season changes become an event you can read. Not known: whether a note per season feels welcome or like mail piling up. The letterbox caps at 20 letters.
