# I335 — The garden cat

Idea 20 of the [fifty](../../games/evergrove/docs/specs/proposals/world-games-50-ideas-2026-11-27.md), after Japanese pocket pets, without the neglect that made them stressful.

## Player

"A garden needs a cat."

## PM decision

Once anything has been tended, a small ginger cat naps in the garden:
- by day, beside the thing you most recently solved;
- after dusk, by the cottage door in the lamplight.

It breathes slowly in its sleep (still with reduced motion). It has no needs at all: nothing to feed, nothing that can go wrong, and nothing to miss while you're away. It's simply there.

## Dev

- `GardenCat.NapBy(world, phase)` (Core, new) and `WorldModel.NewestSolvedThing` (the solve order already kept). The test was written first. It checks:
  - no cat before anything is tended;
  - by day, beside the last solved thing;
  - after dusk, by the door;
  - living gardens only.
- `GardenMapView.SleepingCat` draws a curled shape (body, head, ears, tail, a stripe) from rounded forms; it's decorative and doesn't take taps. `ApplyDayLight` sets `CatBy`.

## Result

There's a small, undemanding life in the garden. The cat is drawn, not painted; when the image tool has credits it should become a painting in the house style.
