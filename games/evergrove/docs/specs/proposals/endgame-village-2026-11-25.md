# Mosselora end game: the village you grow

**Status:** founder direction, 2026-11-25. **This is the target to build toward, not a claim that it works.** It goes beyond the current specs, which hold village expansion until the M4 player test. The founder chose to build toward the end game now. The decision log records that choice, and §9 lists the gates that still apply.

**Owner of taste:** the founder. Any feature that fails the taste charter (§1) is cut, however much work went into it.

---

## 0. The one sentence

**Restore the water, and a living village grows where you choose, among neighbours and animals who remember what you made, a garden that grandparents, parents and grandchildren tend together.**

The water puzzle stays the heart of the game. Everything else in the village is something the puzzle earned.

---

## 1. Taste charter: what "perfect" means here

Every screen, sound and system is judged against these. The founder's word: *taste is most important*.

1. **Calm, never idle.** There is always one beautiful next thing to do and never a list of chores. No timers, energy, streaks, wilting crops, or "you were away" guilt. A garden left alone waits, unchanged and glad to see you.
2. **Hand-made, not generated-looking.** One painted watercolour storybook style everywhere: art, UI, map, icons. If an asset looks like stock or AI slop next to its neighbours, it is redone or removed. Consistency beats quantity.
3. **The world explains itself.** Pictures, motion and touch before words. Words are kept for names, memories and rare lasting decisions. No tutorials that read like manuals.
4. **Adult, never childish or "senior".** For people over 40: rich, competent, quietly witty. Large readable type and big touch targets are standard, not a special mode.
5. **Everything you make is remembered truthfully.** Neighbours only speak about things that happened in your save. Nothing invented, nothing pleading for attention.
6. **Fewer, better things.** A feature ships only when it is finished: animated, sounded, readable at 150% text, working sideways, with no rough edges. Half-features are hidden until done.
7. **Respect the player's time and money.** Offline, no ads, no paid shortcuts, no manipulative loops. If a player's grandchild read how we made money, we'd be proud.
8. **Every addition must make the puzzle better or the world kinder.** If it does neither, it doesn't belong.

**Taste review rule:** before any feature merges, it is shown in portrait, landscape and at 150% text, and judged against these eight lines. A blind read by a stand-in player helps; a real player over 40 decides.

---

## 2. The end-game experience (what a player feels)

- **Morning, day 1:** a dry creek, a cottage, Mira, Orin and Fen. You turn pipe tiles until the water runs. The creek bank greens. You choose: *Plant a quince tree, mend the mill wheel, or grow foxgloves?* You hold the quince sapling and tap a sunny spot by the water. It grows in.
- **Day 10:** your garden map has a mill, an orchard, a hedgehog house you built because Fen asked, and a path you laid. Orin walks your bridge every morning, and he says so. Autumn arrived because you reached stream 7, not because the calendar turned.
- **Day 40:** the first garden is a finished, named place. You keep it as a page in your village book, and a new garden begins downstream, inheriting a landmark from the old one. Your almanac shows 37 of 200 trees, flowers and buildings found. You want to see the windmill.
- **Day 365:** the village is a long valley of gardens you made, each one remembered. You show a friend a postcard of it.

---

## 3. Systems

### 3.1 The water puzzle (unchanged heart)
Rotate tiles until one leak-free stream runs from spring to garden. A choice roots a protected tile in the next board (`ReactiveWatercourses`). Depth grows from the system (ecology constraints, EG-105), not from content volume.

### 3.2 The garden catalogue: 200 things to grow and build (I171)
- **50 trees** (Mira), **50 flowers** (Fen), **100 things to build or mend** (Orin): mills, wells, beehives, a forge, a tree house, a bug hotel, rope bridges, paper lanterns…
- Each choice offers one specific item per family. A garden never repeats an item, and the next garden continues the walk through the catalogue, so all 200 appear across gardens.
- Every item has its own painted icon in the house style (8 Codex sheets of 25, reviewed for consistency), its own name, and its own remembered line.

### 3.3 The garden map: a small open world you shape
- A **top-down painted garden map**, pannable and gently zoomable, built from terrain tiles (grass, meadow, path, water, stone, soil). It replaces the fixed village strip as the place where your choices live.
- **You place what you earn.** After choosing, the item appears in your hand and you tap a free spot to plant or build it. Drag to move it later. Placement snaps to a soft grid, so it's never fiddly. Invalid spots (water, under another item) simply don't accept it.
- **Water shapes the land.** Each restored stream extends the creek across the map, and dry ground near water turns green. Some items want water (water lilies, the mill wheel), some want shade or sun. That's a gentle, readable rule, never a failure.
- **Seasons repaint the map** as you progress through streams: spring blossom, summer, autumn leaves, winter frost.
- **It's yours, saved and portable.** Placement is part of the save (an additive field, migration-tested) and part of the garden file.
- **Simple for adults over 40:** one finger to pan, two to zoom, tap to place. No inventory management, no crafting trees, no resources.

### 3.4 Neighbours who notice
- Mira, Orin and Fen walk the map's paths between things you built and stop at them.
- They speak short, true lines about your things: "The bees found your lavender." Their words come only from real save state (the world spec rule).
- More villagers can arrive as the village grows (a baker when you rebuild the bread oven, a beekeeper with the hives). Each one is authored, with 3–5 memories.
- No affection meters, no needs, no guilt.

### 3.4b Animals who come to live there
- **Animals arrive because of what you made**, never at random: a bug hotel brings beetles and ladybirds, the hedgehog house brings a hedgehog, the lily pond brings frogs and a heron, beehives and lavender bring bees, nesting boxes bring blue tits, the duck house brings ducks, an orchard brings a fox at dusk, the barn brings an owl.
- They live on the map: the hedgehog snuffles under your hedge, the ducks paddle the creek you restored. Tap one and it reacts (a hop, a quack, a purr). No feeding chores, no needs, and nothing ever leaves because you were away.
- Each animal is a found entry in the almanac, with where it came from ("came for your lily pond").
- Target: about 40 animals, each tied to one or more catalogue items, so building is also an invitation.

### 3.4b2 A garden pet
- **A companion that lives in your garden**: a dog, a cat, a rabbit, a goat or a duckling. The first one comes with the cottage. Others arrive the way animals do: the goat with the goat shed, a cat that adopts the bread oven.
- **It keeps you company, it doesn't need you.** It follows you around the map, sits by the creek while you solve a board, wags or purrs when water runs, and naps under the tree you planted. When you come back, it is simply happy you're here. It is **never** hungry, sick, sad or lonely, and never asks for anything. No feeding timers, no care meters, no guilt.
- **Gentle play:** tap to pet it, toss a ball, or let it carry a stick across the bridge you built. The grandchild's favourite turn.
- **One for each family gardener (§3.5b)** if they like: a name chosen by the family, remembered in the book ("Biscuit found the frog pond first").
- **Never sold:** no paid breeds, outfits or accessories. Pets come from what you build and grow.

### 3.4c A village that grows its people
- The cast grows from three to about **12 neighbours**, each arriving with something you built: the baker with the bread oven, a beekeeper with the hives, a boatwright with the boathouse, a schoolteacher with the school bell, a potter with the kiln, two children who claim the tree house.
- Every newcomer is authored (a portrait, a voice, 3–5 true memories) and lives where they arrived. Small honest moments between them: the baker brings bread to the potter. No relationship meters.

### 3.5 Seasons, chapters and generations
- A garden is 12 streams across a year of progress (not the calendar).
- A finished garden becomes a **page in the village book** (its map, its memories, its festival painting). A new garden starts downstream and inherits one landmark (EG-109).
- The **valley view** shows your gardens side by side along the creek: the long-term home screen.

### 3.5b Played across generations (the family garden)
Mosselora is a game **grandparents, parents and grandchildren can play together, gently**. This is what "generations" means here, as well as gardens handed down the valley.
- **One phone, passed hand to hand.** Sitting together, anyone can take a turn: a child turns tiles, a grandparent chooses what to grow. No accounts, no logins. The board is readable without reading.
- **Family gardeners.** Each person can pick a painted gardener badge (no names required). What each of them planted or built is remembered as theirs: "Grandma planted the plum tree; Leo built the tree house." The book shows who made what.
- **Kid-safe by design, not by a separate mode:** no ads, no purchases, no links out, no chat, no text needed to play. Large targets, gentle motion, nothing scary.
- **A valley that passes down.** Each family member can start their own garden downstream of the others, so the family's valley grows side by side. A postcard of the family valley can be shared to relatives through the share sheet.
- **Together, not competing.** No scores between family members, no leaderboards. Grandparents who play daily can leave a small gift in the garden (a planted seed, a note on the signpost) for a grandchild's next visit on the same device.
- **Primary audience stays adults 40+:** they are the daily players, and the family is who they play with. Child-directed store categories and privacy rules (COPPA, GDPR-K) need review before any "for kids" claim (§9).

### 3.6 The almanac
- A painted collection book of all 200 items: found ones in colour, with where and when you made them; unfound ones as soft pencil silhouettes.
- It's a gentle reason to begin another garden. Never a checklist with rewards or pressure.

### 3.7 Sharing (later, no server first)
- A **postcard** of your map through the phone's share sheet (I149 already does this for text).
- Later, if earned: visit a friend's valley as a read-only postcard. Trading and economies stay rejected.

### 3.8 Sound and touch
- Each family has its own soft sounds (a sapling's rustle, a hammer's three knocks, a petal chime) and a light haptic. The creek's sound grows with the water. All of it is optional and none of it is required to understand play.

---

## 4. What we will not build

Economies, currencies, shops, energy, timers, streaks, crafting chains, worker management, needs bars, combat, multiplayer competition, ads, loot boxes, paid hints, notifications that nag, generated dialogue that invents history. (See the vision, §2.6, and the reject list in `02-FEATURE-BACKLOG.md`.)

---

## 5. Build plan: in order, each step finished before the next

| Phase | Ships when | Done means |
|---|---|---|
| **E1: Catalogue live** (I171–I173) | 200 items with painted icons on choice cards, memories, book, ending and village | Icons reviewed for style consistency; card text fits at 150%; portrait, landscape and 150% tours pass |
| **E2: Almanac** | Collection book in the menu | Found and unfound items reading clearly; counts honest; no pressure language |
| **E3: Garden map v1** | Pannable painted tile map replacing the village strip; items appear at preset spots | Map art cohesive; 60 fps on a mid phone; readable at arm's length |
| **E4: Place what you earn** | Tap-to-place and drag-to-move; saved; migration test | 8 players over 40 place an item unprompted in under 15 s (M3 probe) |
| **E5: Water shapes the land** | Creek extends per stream; greening; water-loving items | The rule explained by the picture alone in a cold read |
| **E6: Neighbours walk** | Villagers path between your things; true remarks | Lines traceable to save state (test); not repetitive over 12 streams |
| **E7: Generations and valley** | Finished garden kept as a page; downstream new garden inherits a landmark | Save and file migration tests; ≥30% of replays materially different (EG-109) |
| **E6b: Animals and newcomers** | Animals drawn by items; neighbours arriving with buildings | Each line and each arrival traceable to save state (tests); animals readable and charming at 80 px |
| **E6c: Garden pet** | One companion on the map, reacting to play; more arrive with buildings | Charming, never needy (no state that decays); reads clearly at 80 px; a child plays with it unprompted |
| **E7b: Family garden** | Gardener badges, "who made what", pass-and-play turns | A grandparent and child play 10 min together unaided (session probe); no reading needed for the child's turns |
| **E8: Sound and touch pass** | Per-family sounds, creek ambience scaling | Sound-off parity; no annoyance in M3 |
| **E9: Polish to release** | Every screen passes the taste review (§1) | Real-device QA, accessibility review, store assets |

Each phase gets its own iteration logs (`docs/improvements/I*`), tests in `Tests/Program.cs`, and portrait, landscape and 150% tours.

---

## 6. Technical shape

- **Core stays pure C#** (no Unity): `GardenCatalog` (done), and to come `GardenMap` (tiles, placements, validity, water spread), `Almanac` (found set), `Generations`. All deterministic, no clock, fully tested.
- **Save:** additive optional fields only (`placements`, `gardenNumber`, `foundItems`), with a migration test per version. The garden file carries them.
- **Unity:** the map is a pooled tile renderer in uGUI, or a dedicated 2D camera if performance requires it (decided at E3 with a frame-time test on device).
- **Art:** Codex image generation in fixed-style sheets with the existing art as reference, run through `tools/evergrove_art_pipeline.py`. Every sheet is reviewed by eye. Off-style cells are regenerated, never shipped.

---

## 7. Measures (honest)

- **Taste:** blind stand-in reads per phase, then real players over 40 (40–49, 50–64, 65+ separately).
- **Return:** voluntary day-2 and day-7 return in the consented diary (the measurement decision proposal). No dark patterns to move it.
- **Core claim still tested (M4):** does the living village beat a puzzle-only version on identical boards? If not, the village is trimmed to what players actually value.

---

## 8. The strongest case against this

- **Scope.** An open garden map, villagers and generations are a lot of content and polish. AI-assisted coding speeds up code, but taste, art review and device testing are still the slow parts. Mitigation: strict phase order, nothing half-done ships.
- **Puzzle dilution.** A builder layer can make the puzzle feel like a chore that gates decoration. Mitigation: taste charter rule 8, and M4.
- **Audience.** Most players over 50 play alone in short sessions. Mitigation: every session must still be satisfying in 3–5 minutes.

---

## 9. Gates that still apply

- Nothing here overrides store policy, privacy, offline play or the release gates (M5/M6).
- The M3 first-session usability gate and the M4 comparison stay as planned. The village grows in parallel, and their results can cut it back.
- Names, memories and catalogue text are English-only until translated and natively reviewed.
- Any claim that the game is for children (store "Kids" category, child-directed marketing) needs a privacy and store-policy review first (COPPA, GDPR-K, Apple Kids Category, Google Families). The design stays kid-safe either way.
