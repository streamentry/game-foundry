# Fifty ideas from the world's games, for Mosselora

**Status:** proposal and build list, 2026-11-27. Founder goal: "learn from the best games in world history, in many cultures; collect 50 ideas to make the vision come true; implement them one by one."

## How the ideas were chosen

- **Borrow principles and mechanics, never content.** Generic mechanics (matching pairs, sowing seeds round pits, drawing pictures by numbers) belong to everyone; another game's names, characters, art, levels or look do not. Every idea below is rebuilt in Mosselora's own world.
- **Every idea passes the vision's rules:** true facts only, no pressure, no streaks or meters, no clock gates (cosmetic time is fine, I272), no paid outcomes, cooperative family play, large targets and adjustable text, nothing about children stored.
- **Each idea is one iteration** (I290 onward), built test-first where there are rules. Batches of five go to a PR.
- **Effort:** S is one session; M needs Core rules plus UI; L is a new puzzle engine (built by a subagent, as I243–I247 were).

## The fifty

### A. Puzzles: what puzzle traditions do best

| # | From | What it does best | For Mosselora | Effort |
| --- | --- | --- | --- | --- |
| 1 | Japanese picture-logic magazines | The answer is a picture, so the solve has a reward | Planting by numbers draws real garden pictures (a leaf, a pear, a cottage), not random blobs | M |
| 2 | Mahjong (China) | Flower and season tiles match within their set | In garden tiles, the four season tiles match one another | S |
| 3 | Hanafuda (Japan) | A card for each month's flower | Pairs deals the flowers of the garden's current season | S |
| 4 | Music-puzzle games (Japan), falling blocks (Russia) | Every right move is a note; solving plays a tune | Each right move plays the next note of a gentle phrase; the last move finishes it | M |
| 5 | Patience (France, Britain) | The famous win cascade | Each kind ends with its own small flourish | S |
| 6 | Go handicap stones (China, Japan, Korea) | The stronger player gives a head start; nobody is shamed | After hints were needed on a kind, its next board starts with a few cells set | M |
| 7 | Shogi promotion (Japan) | A piece that goes far is crowned | A thing solved at its deepest tier wears a gold crown ring | S |
| 8 | Mancala family (Africa, Southeast Asia; Ô ăn quan in Vietnam) | Sowing seeds round pits: one rule, deep counting | A new kind: sow seeds round the garden beds | L |
| 9 | Tangram (China) | Seven pieces, endless shapes | A new kind: seven garden pieces fill a silhouette | L |
| 10 | Sum puzzles (Japan) | Arithmetic as logic, the studio's own first-game candidate | A new kind: seed sums | L |
| 11 | Puzzle-adventure games (Japan) | Puzzles found in the world | The old well on the map holds a puzzle of its own (deepest tier, pays fruit) | M |
| 12 | The daily word game shared as coloured squares (Wales, USA) | Sharing a result without spoiling it | Share a solved board as a little grid of garden squares | S |
| 13 | Nikoli (Japan) | Honest difficulty from the logic, not the size | Depth leaves come from the steps the solver needed | M |

### B. The open garden

| # | From | What it does best | For Mosselora | Effort |
| --- | --- | --- | --- | --- |
| 14 | Block-building sandboxes (Sweden) | Free creation, no cost | Lay stepping stones, fences and benches anywhere, free | M |
| 15 | The same | Always knowing your way home | A compass token: jump to the cottage or the newest stretch | S |
| 16 | Player-made worlds (USA) | Visiting what others built | Visits show a relative's stones, fences and benches | S |
| 17 | Cat-collecting games (Japan) | Visitors come to what you put out | Bees, birds and a hedgehog visit what you planted | M |
| 18 | Creature-garden games (Britain) | Visitors have logical conditions | Bees need three flower beds; birds need fruit trees (tested) | S |
| 19 | Creature-collecting games (Japan) | The joy of a full page | A visitors page in the almanac | S |
| 20 | Pocket pets (Japan), without neglect | A small life that's glad to see you | A garden cat naps near your latest solve; never hungry, never ill | M |
| 21 | Bonsai (Japan, China) | Patience shows in the tree | Favourites visibly mature as they deepen | S |
| 22 | Life simulations (USA) | People live in the world | Neighbours stand by the newest thing of their kind | S |
| 23 | Small-island hiking games (Canada) | A lookout rewards the climb | A bench on each new stretch shows the whole river so far | M |
| 24 | Action-adventure classics (Japan) | Secrets under stones | A lost cutting under a mossy stone, found by tapping | S |

### C. Seasons, light and ritual

| # | From | What it does best | For Mosselora | Effort |
| --- | --- | --- | --- | --- |
| 25 | Farming life games (USA, Japan) | Festivals mark the year | When the year turns, a small festival: lanterns, neighbours gather | M |
| 26 | Same | Seasons greet you | A note from a neighbour on the first day of each season (by progress) | S |
| 27 | Hanami and momijigari (Japan) | Watching petals and leaves fall | Spring petals and autumn leaves drift over home | S |
| 28 | Hygge (Denmark) | A warm window at night | Cottage windows glow after dusk | S |
| 29 | Cosy life games (Japan) | Weather you notice | A short shower over a thing after its water is freed | S |
| 30 | Japanese animated film | Wind you can see | Things sway gently in the breeze (still with reduced motion) | S |

### D. Companion and story

| # | From | What it does best | For Mosselora | Effort |
| --- | --- | --- | --- | --- |
| 31 | Ferry-of-souls caring games (Canada) | Each companion has a wish you can grant | Mira, Orin and Fen each have a wish the garden can fulfil, then a letter | M |
| 32 | Village life games (Japan) | Neighbours give back | After true milestones, a neighbour leaves a cutting of their kind | S |
| 33 | The same, museum | Every exhibit tells you something | Almanac items carry a line from their keeper | S |
| 34 | Unpacking games (Australia) | A life told through objects | A memory shelf in the cottage keeps your firsts (first letter, first fruit) | M |
| 35 | Wordless companionship games (USA) | Presence without words | Neighbours wave when you return | S |
| 36 | Classic role-playing inns (Japan) | The inn as a calm summary | Tap the cottage: what's waiting in the garden, at a glance | S |
| 37 | Wordless story games (Australia) | Pictures say it | Picture stamps on letters, for those who'd rather not type | M |
| 38 | A Thousand and One Nights (Persia, Arabia) | One tale a night, by progress | Each neighbour tells a short tale each season | M |

### E. Family, from many tables

| # | From | What it does best | For Mosselora | Effort |
| --- | --- | --- | --- | --- |
| 39 | Mahjong tables, Ludo (China, India) | Four around one board | Take turns with three or four players (four colours) | S |
| 40 | Omiyage (Japan) | Bring something home from a visit | Take a keepsake cutting home from a relative's garden (copied, not taken) | S |
| 41 | Lì xì and red envelopes (Vietnam, China, Korea) | A gift wrapped with a blessing | Send seeds in a festive envelope with a blessing | S |
| 42 | The daily shared puzzle (Wales, USA) | Everyone plays the same one | Send a relative the very board you just solved | S |
| 43 | Wordless co-op (USA) | Leaving a light for another | A visit leaves a lantern in their garden | S |
| 44 | Kolam and rangoli (India) | Symmetric threshold drawings | A new kind: a doorstep pattern by mirror symmetry | L |

### F. Feel and craft

| # | From | What it does best | For Mosselora | Effort |
| --- | --- | --- | --- | --- |
| 45 | Cooking games (Japan) | Turning ingredients into a gift | Three fruit make a jar of jam, a richer gift | M |
| 46 | Impossible-geometry puzzles (Britain) | The solved world transforms | The solved board fades into the thing it belongs to | S |
| 47 | Nintendo-style "juice" (Japan) | Every touch answers | Each kind has its own touch sound: a tile's clack, a card's flip | S |
| 48 | Gentle platformers (Japan) | Help for small hands | In take-turns, honey's (the second player's) turn may glow a hint | S |
| 49 | Ambient puzzle games (Japan) | Sound as a place | A soft soundscape by season and hour | M |
| 50 | Ma (Japan) | The beauty of empty space | A quiet mode: hide everything and just watch the garden | S |

## Build order

Value first, then dependencies.

1. **Quick wins that deepen what exists:** 2, 3, 5, 7, 12, 15, 21, 22, 27, 28, 30, 33, 35, 36, 39, 40, 41, 42, 43, 46, 48, 50.
2. **Systems:**
   - 1 and 13 (better puzzles);
   - 17, 18 and 19 (visitors);
   - 14 and 16 (free building);
   - 20 (the cat);
   - 25, 26, 31, 32 and 38 (story);
   - 45 (kitchen);
   - 4, 47 and 49 (sound);
   - 6, 11, 23, 24, 29, 34 and 37.
3. **New engines:** 8, 9, 10 and 44.

## Never claim

That any idea improves wellbeing, memory or connection; that it raises retention (unmeasured until the [daily-loop kit](../../playtest/daily-loop-kit.md) runs).
