# I288 — A hundred iterations: what Mosselora became, and what's still unknown

The run from I189 to I288. Every iteration has its own doc in this folder, written as player notes, PM decision, dev and result. This page is the map.

## What a player can do now

**Every day, alone**
- Tend the garden: each thing holds a puzzle.
- Eight kinds of puzzle:
  - water, Pairs and Mosaic (I203–I209);
  - planting by numbers, seed sudoku, garden plots, lead the stream and garden tiles (I243–I247).
- Each kind teaches itself with a picture rule (I254, I262) and a two-tap hint (I253, I256).
- The play mark on each thing shows its kind, so you choose today's puzzle (I255).
- Replays deal fresh boards (I259), and favourites deepen, shown in leaves (I260, I261).

**A logical, simple economy**
- First solves earn seeds, and replays a little (I206).
- Seeds buy nursery plants, row by row. The nursery restocks with new catalogue plants, and later editions bring new kinds of puzzle (I208, I240, I243).
- Tree solves pick fruit, which is for giving, never for buying (I228–I232).
- Gifts move seeds and fruit and never create them. All of it is tested.

**An open garden with a horizon**
- The river opens a stretch per four first solves after the story, up to a raisable ceiling, all worked out from the save (I220).
- Opening is visible: the mist lifts (I223), and drops on the mist show how close the next stretch is (I224).

**A year and a day**
- Four seasons by progress: the story walks spring to winter, then tending turns the year forever (I248–I252). The mist below shows the season to come.
- Light follows your day: dawn, golden hour, dusk, night and fireflies. It's cosmetic, can be switched off, and never touches progress (I272, I278–I282).

**A companion that only says true things**
- Mira, Orin and Fen notice:
  - your arrivals and gifts (I210, I222);
  - the puzzles you've solved, by kind (I257);
  - the river and its season (I225, I250);
  - fruit you brought (I231), a letter waiting (I236), playing together (I266), and the light (I281).
- Mira leaves a note when new river opens (I271).
- No guilt, no "we miss you", no meters. Banned-word tests guard every line.

**Family**
- Offline parcels carry seeds, cuttings, fruit, letters and help (I214–I230). A relative's garden can be visited and helped (I216, I227).
- Letters come sealed and can be written back to (I233–I237).
- A family page in the book (I269).
- Take turns on one device. The garden remembers puzzles played together without storing who (I263–I267).

**Garden notes**
- At most one a day, at your hour, only when something's truly waiting, in a neighbour's voice. Invited after a soft ask, with one switch in Settings, and it opens where it points (I273–I277, I283).
- A real note was delivered on the iOS simulator (I284).

## How it was checked

- Pure C# tests: 349,144 assertions, about 8 seconds. Every rule is test-first where the doc says so; the docs say plainly where tests were written alongside the code.
- Simulator tours in portrait, landscape and 150% text: 89 to 93 shots each for every batch merged.
- Tour bugs found and fixed along the way, including the river step being skipped and a native link failure hidden behind "0 C# errors".

## What is NOT known, and matters most

1. **No adult aged 40+ has played any of this.** Every reading of "easy", "deep", "calm" or "legible" is the team's, not the audience's.
2. **No physical device.** Touch, text size, haptics, performance, notification delivery and permission flow are unverified on a phone.
3. **No family has taken turns.** Co-play is a hypothesis (vision section 1a).
4. **Notifications:** whether the soft ask converts, whether notes feel welcome, and how many turn them off. The stop rule is in the spec.
5. **Economy pace:** how fast seeds come, nursery prices, and how quickly favourites deepen.
6. **Translations:** the new text exists in English only. Consumer builds ship English until native review.
7. **Art rights:** the food, letter and season art is AI-generated, like the existing masters. A rights review is needed before submission. New painted icons wait for image-tool credits (I255).

## The next real test

Three days with uncoached adults 40+ (bands 40–49, 50–64 and 65+) on their own phones. Watch:
- whether they choose puzzles by the marks and understand the drops on the mist;
- whether they return without a note;
- whether they turn notes on, and keep them on;
- with one grandparent and grandchild pair, whether they take turns unprompted.

The decision log entry of 2026-11-27 sets the stop rules.
