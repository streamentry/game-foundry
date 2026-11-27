# I321 — Pictures by numbers

Idea 1 of the [fifty](../../games/evergrove/docs/specs/proposals/world-games-50-ideas-2026-11-27.md), after Japanese picture-logic magazines, where the answer is a picture, so the solve has a reward.

## Player

"I filled in the planting-by-numbers grid and got a random splodge."

## PM decision

Every planting-by-numbers bed now draws a garden picture:

| Tier | Size | Pictures |
| --- | --- | --- |
| 0 | 5×5 | 9 |
| 1 | 6×6 | 10 |
| 2 | 8×8 | 12 |

The subjects are a pear, a tulip, a mushroom, a cottage, a snail, a bird, a bell, a cup and more. Each is also shown mirrored, so every picture appears in two poses. When the bed is solved, it says what it drew: "It's a pear." Every picture is solvable by row and column logic alone, never by guessing.

## Dev

- Core (a Sonnet subagent, test-first, reviewed and merged): `NonogramPictures` holds the original pixel drawings. `NonogramGame` picks one by seed and adds `PictureName`. An independent line solver proves every picture and its mirror reaches the unique solution. Dropped pictures (not line-solvable, or not recognisable) are listed in the commit.
- One older test ("beds differ across seeds") was replaced, because variety is now the library's size; the replacement checks every picture appears.
- Unity: `NonogramBoard.Lock` shows the name as a toast over the bed.

## Result

Solving draws something you can name. Recognisability at 5×5 and 6×6 was checked as text drawings, not by players. Check it on a phone.
