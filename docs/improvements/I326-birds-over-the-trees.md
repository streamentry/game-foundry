# I326 — Birds over the trees

Ideas 17 and 18 of the [fifty](../../games/evergrove/docs/specs/proposals/world-games-50-ideas-2026-11-27.md), continued.

## Player

"My trees bear fruit now. Do the birds know?"

## PM decision

Once two trees have been tended (they bear fruit, I229), a pair of birds glides high over the garden now and then: about every 9 to 17 seconds, across in 6, wings beating then gliding. Birds stay through winter, but not at night.

## Dev

- `Visitors.Welcomes(world, Birds)` counts trees solved, story and nursery; `Out` is any season, never in the dark. Tested with I325.
- `GardenMapView.BirdsPass` draws two small dark "v" silhouettes from two thin bars each, flapping then gliding along a gentle arc. `ApplyDayLight` sets `BirdsOut`.

## Result

Tending trees changes the sky. Unverified: whether the simple silhouettes sit well over the painted map.
