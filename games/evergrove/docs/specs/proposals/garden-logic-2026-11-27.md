# The garden's logic (run 3 reference, 2026-11-27)

This is a designer's reference for the systems run 3 added (I338–I437). Everything here is a rule a player can learn by playing, with no hidden numbers. The rules live in pure Core code (`Assets/Scripts/Core`), and every rule is tested in `Tests/`. The garden's ways tab in the book (I403) shows the player the same rules in the same words.

**Status:** all of this is implemented and checked on the iOS simulator through scripted tours. **None of it has been tested with players aged 40+ yet** (`age fit not validated`). Nothing here is a claim about wellbeing or retention.

## 1. Good neighbours (where things stand matters)

"Beside" means touching, diagonals included. The rules are in `Pairings.Between`.

| Pairing | Rule | Open | First-time reward |
| --- | --- | --- | --- |
| Sheltered | a flower beside a tree | from the start | 15 seeds |
| By the water | something built beside the river | from the start | 15 seeds |
| A flower bed | two *different* flowers side by side | from the start | 15 seeds |
| A grove | two trees side by side | from the start | 15 seeds |
| Flowers by the water | a flower beside the river | once the first four are found | 25 seeds |
| A shady seat | something built beside a tree | once the first four are found | 25 seeds |
| An orchard | a tree touching two other trees | once the first four are found | 25 seeds |

- **It pays once per garden**, so moving things back and forth earns nothing (I368).
- **A pairing must still stand to keep working.** Its visitor and its season's gift need it in the garden *now*, not just found once.
- **Discovery is never a puzzle about the rules.** The rules are written in the garden's ways page; the keepers hint in their own voices (I370, I381); while you hold a thing, the free spots where it would make a new pairing glow (I372).
- **Family:** on a visit, a toast suggests the one thing that would give *their* garden a new pairing (I398/I418), and the send sheet remembers the idea (I408).

## 2. Visitors (nature answers where you put things)

| Visitor | Comes for | Out | Weather (seen only, I405) |
| --- | --- | --- | --- |
| Bees | 3 flowers growing | spring and summer days | stay in when wet |
| Birds | 2 trees tended | days, any season | — |
| Butterflies | any flower | spring and summer days | stay in when wet or windy |
| Heron | built by the water | days (not dawn), not winter | — |
| Hedgehog | sheltered flower | dusk and night, not winter (hibernates) | — |
| Owl | grove | night, any season | stays in on a rainy night |
| Kingfisher | flowers by the water | days, not winter | — |

Tap a visitor and it answers (I404). The owl is heard at night while a grove stands (I376), and bees are heard by day (I426). The fish rising on the river (I416) are why the heron and kingfisher come; the heron sometimes comes up with one (I427).

## 3. The garden's year (once per season, I388–I392)

| Season | Needs (found *and* standing) | Gives |
| --- | --- | --- |
| Spring | a flower bed | a self-sown flower cutting in the basket |
| Summer | a grove | 20 seeds |
| Autumn | an orchard | 5 fruit |
| Winter | fruit given to a neighbour this year | a jar of their jam |

- A season turn is the story's own season during the story, then each turn after (I251). The save field `yields` holds the last turn taken, plus one.
- **A full cap or basket keeps the gift waiting** rather than wasting it.
- The cottage card shows what this season can still give (I400).

## 4. The bees' work (pollination, I378/I379)

A tended tree with a flower beside it gives **one more fruit**, but only while bees are about and only when that solve itself picked fruit. Any limit on a tree's own fruit, such as a daily replay cap, therefore also limits pollination. On the map, a bee at such a flower flies over to the tree and back.

## 5. Economy bounds (summary)

Every source above is bounded by one of four things:

- **once per garden:** pairings;
- **once per season turn:** yields;
- **once per qualifying solve:** pollination;
- **an existing cap:** MaxSeeds, MaxFruit 99, MaxJam 10, MaxGifts 50.

Weather never changes the economy, because players can't control the date (I353, I405). A parallel session owns the seed ledger: every seed change goes through `ChangeSeeds` with a `SeedReason`.

## 6. The companion (device-only facts, I419–I422)

| Notice | From | Rule |
| --- | --- | --- |
| The hour you come | the first return of each day, counted by phase | at least 6 returns, one phase at 60% or more; the noticer speaks one day in 7 |
| Welcome back | days since last seen | 7 days or more, or 30 or more: reassurance only, never guilt |
| Anniversary | the first day seen on this device | Orin's note, once that day each year |
| Quiet hours | the clock | 22:00–06:59: sounds at 60% |

These facts stay in PlayerPrefs on the device. **They are never written to the garden file**, which travels to family.

## 7. The living world (cosmetic, by time and weather)

All of these are pure rules in Core (`DayLight`, `DayWeather`, `RiverMoments`, `SeasonLook`), drawn by `GardenMapView` and `RiverLifeLayer`. With reduced motion they hold still.

- **Weather of the day:** fair, rainy, windy, hot or snowy, by local date and garden (I353–I357).
- **Seasons:** look and drift (I338–I342).
- **Rainbow** after rain (I364).
- **Dew and frost** in the morning (I366).
- **Mist** on the river at dawn and morning, not on hot or windy days (I409).
- **Chimney smoke** on cold evenings (I410).
- **Frogs** on summer dusks (I411).
- **Footprints** in the snow (I412).
- **On the river:** rain rings (I413), ripples where you tap (I414), golden glitter (I415), fish rising (I416), autumn leaves drifting downstream (I417), moonlight on clear nights (I425).

## 8. What would change these rules

- **Players miss the pairings.** If players 40+ don't find any pairing within their first week without help, make the glow (I372) appear unasked for a thing just placed.
- **Words are skimmed.** If the garden's ways page is read as homework, cut it to the at-a-glance row (I423) plus one line per pairing.
- **The companion feels watched.** If the hour notice reads as surveillance in interviews, remove I419. The welcome back (I420) stays.
